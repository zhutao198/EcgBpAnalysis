using Microsoft.Data.Sqlite;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Zoncare.Common;

namespace Zoncare.DAL
{
    public class SqliteSchemaChecker
    {
        /// <summary>
        /// 从 JSON 文件读取 schema 并同步到数据库。
        /// 只处理「新增的表」和「新增的字段」，已存在的表/字段不做任何改动；
        /// 根节点下的 "sql" 数组为数据库升级脚本文件路径列表，在表结构同步完成后依次执行。
        /// </summary>
        /// <param name="conn">已打开的数据库连接</param>
        /// <param name="jsonFilePath">schema 定义文件（config/dbCheck.json）路径</param>
        public static void SyncFromJsonFile(SqliteConnection conn, string jsonFilePath)
        {
            if (conn == null) throw new ArgumentNullException(nameof(conn));

            if (string.IsNullOrWhiteSpace(jsonFilePath) || !File.Exists(jsonFilePath))
            {
                WriteLog($"schema 文件不存在，跳过同步: {jsonFilePath}");
                return;
            }

            string json = File.ReadAllText(jsonFilePath, Encoding.UTF8);
            JObject schema = JObject.Parse(json);

            if (schema == null || schema.Count == 0) return;

            // 版本指纹：dbCheck.json 内容未变化时整段跳过同步（结构只增不改，上次同步成功且配置未变则无需重跑）。
            // 注意：若数据库文件被回滚为旧版本但配置文件未变，将不会补建表/字段，此时需手动删除指纹表触发完整同步。
            string jsonHash = ClientKey.SHA256EncryptString(json);
            if (IsSchemaVersionMatch(conn, jsonHash))
            {
                //WriteLog($"dbCheck.json 与上次一致（SHA256: {jsonHash}），跳过表结构同步");
                return;
            }

            // 表定义在根节点 "table" 下；兼容旧格式（表定义直接放在根节点）
            JObject tablesNode = schema["table"] as JObject;
            if (tablesNode == null)
            {
                tablesNode = new JObject();
                foreach (var prop in schema.Properties())
                {
                    if (prop.Name == "sql") continue;
                    if (prop.Value is JObject) tablesNode.Add(prop.Name, prop.Value);
                }
            }

            using (var tran = conn.BeginTransaction())
            {
                foreach (var tableEntry in tablesNode)
                {
                    string tableName = tableEntry.Key;
                    var fieldObj = tableEntry.Value as JObject;
                    if (fieldObj == null) continue;

                    ValidateIdentifier(tableName);
                    List<FieldItem> fields = ParseFields(tableName, fieldObj);

                    if (!TableExists(conn, tableName, tran))
                    {
                        string createSql = BuildCreateTableSql(tableName, fields);
                        if (!string.IsNullOrEmpty(createSql))
                        {
                            using (var cmd = new SqliteCommand(createSql, conn, tran))
                            {
                                cmd.ExecuteNonQuery();
                            }
                            WriteLog($"已创建表 {tableName}，SQL: {createSql}");
                        }
                        continue;
                    }

                    HashSet<string> existingColumns = getTableColumns(conn, tableName, tran);
                    foreach (FieldItem field in fields)
                    {
                        string columnName = field.FieldName;
                        FieldDef def = field.Def;

                        if (existingColumns.Contains(columnName, StringComparer.OrdinalIgnoreCase))
                            continue; // 字段已存在则不处理（SQLite 也无法给已有字段追加 DEFAULT 约束）

                        string sqlType = resolveSqliteType(tableName, columnName, def);

                        string sql = BuildAddColumnSql(tableName, columnName, def, sqlType);
                        using (var cmd = new SqliteCommand(sql, conn, tran))
                        {
                            cmd.ExecuteNonQuery();
                        }
                        WriteLog($"已为表 {tableName} 新增字段 {columnName} {def.type}，SQL: {sql}");
                    }
                }
                tran.Commit();
            }

            // 表结构同步完成后，再执行根节点 "sql" 中配置的升级脚本。
            // 脚本在事务外执行：脚本内部可能自带 BEGIN/COMMIT，SQLite 不允许嵌套事务。
            JToken sqlNode = schema["sql"];
            if (sqlNode != null && sqlNode.HasValues)
            {
                ExecuteSqlScripts(conn, sqlNode, Path.GetDirectoryName(jsonFilePath));
            }

            // 表结构同步（含升级脚本）全部完成后再记录版本指纹；
            // 若中途抛异常则不记录，下次启动仍会执行完整同步（同步本身幂等，重跑无害）
            MarkSchemaVersion(conn, jsonHash);
        }

        /// <summary>
        /// schema 版本指纹表名
        /// </summary>
        private const string SchemaVersionTableName = "schema_version_log";

        /// <summary>
        /// 确保 schema 版本指纹表存在
        /// </summary>
        private static void EnsureSchemaVersionTable(SqliteConnection conn)
        {
            using (var cmd = new SqliteCommand(
                "CREATE TABLE IF NOT EXISTS \"schema_version_log\" (" +
                "\"content_sha256\" TEXT PRIMARY KEY, " +
                "\"checked_time\" TEXT NOT NULL);", conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 判断 dbCheck.json 的 SHA256 是否与上次同步成功时记录的一致。
        /// 检查失败时返回 false（退化为完整同步，保证正确性优先）
        /// </summary>
        private static bool IsSchemaVersionMatch(SqliteConnection conn, string jsonHash)
        {
            try
            {
                EnsureSchemaVersionTable(conn);

                using (var cmd = new SqliteCommand(
                    "SELECT COUNT(*) FROM \"schema_version_log\" WHERE \"content_sha256\"=@h;", conn))
                {
                    cmd.Parameters.AddWithValue("@h", jsonHash);
                    return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
                }
            }
            catch (Exception ex)
            {
                WriteLog($"检查 schema 版本指纹失败，将继续执行完整同步：{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 记录当前 dbCheck.json 的 SHA256（清空旧记录后写入，只保留最新指纹）
        /// </summary>
        private static void MarkSchemaVersion(SqliteConnection conn, string jsonHash)
        {
            try
            {
                EnsureSchemaVersionTable(conn);

                using (var cmd = new SqliteCommand(
                    "DELETE FROM \"schema_version_log\";" +
                    "INSERT INTO \"schema_version_log\" (\"content_sha256\", \"checked_time\") VALUES (@h, @t);", conn))
                {
                    cmd.Parameters.AddWithValue("@h", jsonHash);
                    cmd.Parameters.AddWithValue("@t", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                WriteLog($"记录 schema 版本指纹失败（不影响本次同步结果，下次启动将重新完整同步）：{ex.Message}");
            }
        }

        /// <summary>
        /// 执行根节点 "sql" 中配置的数据库升级脚本。
        /// 格式："sql": ["a.sql", "b.sql"]（也兼容单个字符串 "sql": "a.sql"）。
        /// 安全约束（防止恶意/被篡改的脚本被执行）：
        /// 1. 只允许相对路径，且解析后必须落在配置文件所在目录内（拒绝绝对路径、UNC、".. " 穿越）；
        /// 2. 脚本必须登记在程序内白名单 ScriptWhitelist 中（json 可被篡改，白名单编译在代码里改不到）；
        /// 3. 脚本内容 SHA256 必须与白名单登记值一致（复用 ClientKey.SHA256EncryptString），不一致视为被篡改。
        /// 同一脚本（路径+内容均未变化）只执行一次；脚本在事务外执行，内部可自带 BEGIN/COMMIT。
        /// </summary>
        private static void ExecuteSqlScripts(SqliteConnection conn, JToken sqlNode, string baseDir)
        {
            if (ScriptWhitelist.Count == 0)
            {
                WriteLog("升级脚本白名单为空，跳过 \"sql\" 节点中的所有脚本");
                return;
            }

            string baseDirFull = Path.GetFullPath(string.IsNullOrEmpty(baseDir) ? "." : baseDir)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            JArray items = sqlNode.Type == JTokenType.Array ? (JArray)sqlNode : new JArray(sqlNode);

            foreach (JToken item in items)
            {
                string scriptPath = item.Value<string>();
                if (string.IsNullOrWhiteSpace(scriptPath)) continue;

                // 1) 只允许相对路径，防止指向用户盘、网络共享等任意位置的脚本
                if (Path.IsPathRooted(scriptPath))
                {
                    WriteLog($"SQL 脚本使用了绝对路径，已拒绝执行（只允许配置文件目录下的相对路径）: {scriptPath}");
                    continue;
                }

                string fullPath = Path.GetFullPath(Path.Combine(baseDir, scriptPath));

                // 路径穿越防护（config\..\..\x.sql 这类写法归一化后必然越出配置目录）
                if (!fullPath.StartsWith(baseDirFull, StringComparison.OrdinalIgnoreCase))
                {
                    WriteLog($"SQL 脚本路径越出配置目录，已拒绝执行: {scriptPath}");
                    continue;
                }

                // 2) 白名单校验：未登记的脚本一律拒绝（json 被篡改新增的脚本走不到这里）
                string whitelistKey = scriptPath.Replace('\\', '/').Trim().ToLowerInvariant();
                string expectedSha256;
                if (!ScriptWhitelist.TryGetValue(whitelistKey, out expectedSha256))
                {
                    WriteLog($"SQL 脚本未登记在程序白名单中，已拒绝执行: {whitelistKey}");
                    continue;
                }

                if (!File.Exists(fullPath))
                {
                    WriteLog($"SQL 脚本文件不存在，跳过: {fullPath}");
                    continue;
                }

                string content = File.ReadAllText(fullPath, Encoding.UTF8);

                // 3) 内容完整性校验：与白名单登记的 SHA256 不一致说明文件被篡改或损坏
                string actualSha256 = ClientKey.SHA256EncryptString(content);
                if (string.IsNullOrEmpty(actualSha256) ||
                    !string.Equals(actualSha256, expectedSha256, StringComparison.OrdinalIgnoreCase))
                {
                    WriteLog($"SQL 脚本内容与白名单 SHA256 不一致，疑似被篡改，已拒绝执行: {whitelistKey}");
                    continue;
                }

                // 同一脚本（路径+内容相同）已执行过则跳过，避免每次启动重复执行升级脚本
                if (IsScriptExecuted(conn, fullPath, actualSha256))
                {
                    WriteLog($"SQL 脚本已执行过，跳过: {fullPath}");
                    continue;
                }

                try
                {
                    using (var cmd = new SqliteCommand(content, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    MarkScriptExecuted(conn, fullPath, actualSha256);
                    WriteLog($"SQL 脚本执行成功: {fullPath}");
                }
                catch (SqliteException ex)
                {
                    WriteLog($"SQL 脚本执行失败: {fullPath}，{ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// 升级脚本白名单：相对路径（小写、正斜杠，相对 config 目录）→ 脚本内容的 SHA256。
        /// 白名单从程序集内嵌资源 whitelist.json 加载（而非磁盘文件）：
        /// dbCheck.json 的 "sql" 节点可能被篡改，但白名单随 dll 编译发布，改不到；
        /// 未登记的脚本、内容被修改的脚本都会被拒绝执行。
        /// 发版时新增或修改升级脚本，需同步更新 whitelist.json 并重新编译。
        /// </summary>
        private static readonly Dictionary<string, string> ScriptWhitelist = LoadScriptWhitelist();

        /// <summary>
        /// 白名单内嵌资源名（默认命名空间 Zoncare.DAL + 文件名）
        /// </summary>
        private const string ScriptWhitelistResourceName = "Zoncare.DAL.whitelist.json";

        /// <summary>
        /// 从程序集内嵌资源加载升级脚本白名单。
        /// 加载失败或资源缺失时返回空字典（fail-closed：所有升级脚本都被拒绝）。
        /// </summary>
        private static Dictionary<string, string> LoadScriptWhitelist()
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            try
            {
                using (Stream stream = typeof(SqliteSchemaChecker).Assembly.GetManifestResourceStream(ScriptWhitelistResourceName))
                {
                    if (stream == null)
                    {
                        WriteLog($"内嵌资源 {ScriptWhitelistResourceName} 不存在，所有升级脚本将被拒绝执行");
                        return result;
                    }

                    string json;
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        json = reader.ReadToEnd();
                    }

                    JObject root = JObject.Parse(json);
                    foreach (var prop in root.Properties())
                    {
                        // 以下划线开头的键是注释，不参与校验
                        if (prop.Name.StartsWith("_")) continue;

                        string sha256 = prop.Value.Value<string>();
                        if (string.IsNullOrWhiteSpace(sha256) || !IsSha256Hex(sha256))
                        {
                            WriteLog($"白名单中脚本 {prop.Name} 的 SHA256 值无效，该条目被忽略");
                            continue;
                        }

                        result[prop.Name.Replace('\\', '/').Trim().ToLowerInvariant()] = sha256.Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                // 白名单读不出来时宁可拒绝所有脚本，也不能放行
                WriteLog($"加载升级脚本白名单失败，所有升级脚本将被拒绝执行：{ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// 校验是否为 64 位十六进制 SHA256 值
        /// </summary>
        private static bool IsSha256Hex(string value)
        {
            if (value.Length != 64) return false;
            foreach (char c in value)
            {
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 确保脚本执行记录表存在
        /// </summary>
        private static void EnsureScriptLogTable(SqliteConnection conn)
        {
            using (var cmd = new SqliteCommand(
                "CREATE TABLE IF NOT EXISTS \"schema_script_log\" (" +
                "\"script_path\" TEXT PRIMARY KEY, " +
                "\"content_sha256\" TEXT NOT NULL, " +
                "\"executed_time\" TEXT NOT NULL);", conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 判断脚本（同一路径且内容未变化）是否已执行过
        /// </summary>
        private static bool IsScriptExecuted(SqliteConnection conn, string scriptPath, string sha256)
        {
            EnsureScriptLogTable(conn);

            using (var cmd = new SqliteCommand(
                "SELECT COUNT(*) FROM \"schema_script_log\" WHERE \"script_path\"=@p AND \"content_sha256\"=@m;", conn))
            {
                cmd.Parameters.AddWithValue("@p", Path.GetFullPath(scriptPath));
                cmd.Parameters.AddWithValue("@m", sha256);
                return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
            }
        }

        /// <summary>
        /// 记录脚本已执行
        /// </summary>
        private static void MarkScriptExecuted(SqliteConnection conn, string scriptPath, string sha256)
        {
            EnsureScriptLogTable(conn);

            using (var cmd = new SqliteCommand(
                "INSERT OR REPLACE INTO \"schema_script_log\" (\"script_path\", \"content_sha256\", \"executed_time\") " +
                "VALUES (@p, @m, @t);", conn))
            {
                cmd.Parameters.AddWithValue("@p", Path.GetFullPath(scriptPath));
                cmd.Parameters.AddWithValue("@m", sha256);
                cmd.Parameters.AddWithValue("@t", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 解析表定义，得到字段列表（含 type / length / isPrimaryKey / defaultValue）
        /// </summary>
        private static List<FieldItem> ParseFields(string tableName, JObject fieldObj)
        {
            List<FieldItem> fields = new List<FieldItem>();
            foreach (var fieldProp in fieldObj.Properties())
            {
                var defObj = fieldProp.Value as JObject;
                if (defObj == null) continue;

                FieldDef def = new FieldDef
                {
                    type = defObj.Value<string>("type") ?? "TEXT",
                    length = defObj.Value<int?>("length") ?? 0,
                    isPrimaryKey = defObj.Value<bool?>("isPrimaryKey") ?? false,
                    // 未配置时 indexer 返回 null；显式配置为 null 时 HasDefaultValue 也为 false
                    defaultValue = defObj["defaultValue"]
                };

                ValidateIdentifier(fieldProp.Name);
                fields.Add(new FieldItem { FieldName = fieldProp.Name, Def = def });
            }
            return fields;
        }

        /// <summary>
        /// 构建 ALTER TABLE ADD COLUMN 语句(SQLite 的 ALTER TABLE 不支持指定长度，长度信息仅作参考)
        /// </summary>
        /// <param name="sqlType">已确定好的 SQLite 列类型（由 resolveSqliteType 得出，避免重复解析）</param>
        private static string BuildAddColumnSql(string tableName, string columnName, FieldDef def, string sqlType)
        {
            ValidateIdentifier(tableName);
            ValidateIdentifier(columnName);

            // SQLite 中 ALTER TABLE ADD COLUMN 不能添加 NOT NULL 且无默认值的列，这里统一允许 NULL；
            // 配置了 defaultValue 时带上（均为常量默认值），SQLite 会把新增列之前的已有行按该默认值读出。
            string columnDef = $"\"{columnName}\" {sqlType}";
            if (def.HasDefaultValue)
            {
                columnDef += " DEFAULT " + BuildDefaultLiteral(columnName, def.defaultValue, sqlType);
            }

            return $"ALTER TABLE \"{tableName}\" ADD COLUMN {columnDef};";
        }

        /// <summary>
        /// 根据字段定义生成 CREATE TABLE 语句
        /// </summary>
        private static string BuildCreateTableSql(string tableName, List<FieldItem> fields)
        {
            ValidateIdentifier(tableName);

            if (fields == null || fields.Count == 0) return "";

            StringBuilder sb = new StringBuilder();
            sb.Append($"CREATE TABLE IF NOT EXISTS \"{tableName}\" (");

            var columnDefs = new List<string>();

            foreach (FieldItem fieldEntry in fields)
            {
                string columnName = fieldEntry.FieldName;
                FieldDef def = fieldEntry.Def;

                ValidateIdentifier(columnName);

                string sqlType = resolveSqliteType(tableName, columnName, def);
                string columnDef = $"\"{columnName}\" {sqlType}";

                if (def.HasDefaultValue)
                {
                    columnDef += " DEFAULT " + BuildDefaultLiteral(columnName, def.defaultValue, sqlType);
                }

                if (def.isPrimaryKey)
                {
                    columnDef += " PRIMARY KEY";
                }

                columnDefs.Add(columnDef);
            }

            sb.Append(string.Join(", ", columnDefs));
            sb.Append(");");

            return sb.ToString();
        }

        /// <summary>
        /// 解析字段最终的 SQLite 列类型。
        /// 日期时间族：拿 defaultValue 按几种常见日期格式尝试解析，解析成功按 DATETIME 建列，失败退回 TEXT；
        /// 其余类型直接按亲和性映射。
        /// </summary>
        private static string resolveSqliteType(string tableName, string columnName, FieldDef def)
        {
            if (!IsDateTimeType(def.type))
                return mapToSqliteType(def.type);

            // 无样值（未配置 defaultValue）：按 DATETIME 声明
            string sample = getDateTimeSample(def);
            if (sample == null)
                return toCanonicalDateTimeType(def.type);

            bool datetimeParsed = DateTime.TryParseExact(sample, SupportedDateTimeFormats,
                CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime parsed);
            if (datetimeParsed)
                return toCanonicalDateTimeType(def.type);

            // 解析失败（如紧凑纯数字 20240101、随意文本）：
            // DATETIME 是 NUMERIC 亲和性，纯数字日期会被存成整数、之后按 DateTime 读取会出错，只能用 TEXT
            WriteLog($"表 {tableName} 字段 {columnName} 的样值 \"{sample}\" 无法按常见日期格式解析，按 TEXT 建列");
            return "TEXT";
        }

        /// <summary>
        /// 常见日期时间格式（按这几种逐一尝试）
        /// </summary>
        private static readonly string[] SupportedDateTimeFormats = new string[]
        {
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd HH:mm:ss.fff",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-dd",
            "yyyy/MM/dd HH:mm:ss",
            "yyyy/MM/dd HH:mm",
            "yyyy/MM/dd",
            "yyyy年MM月dd日 HH:mm:ss",
            "yyyy年MM月dd日"
        };

        /// <summary>
        /// 判断 JSON 类型是否属于日期时间族
        /// </summary>
        private static bool IsDateTimeType(string jsonType)
        {
            switch ((jsonType ?? "").Trim().ToUpperInvariant())
            {
                case "DATETIME":
                case "DATETIME2":
                case "SMALLDATETIME":
                case "TIMESTAMP":
                case "DATE":
                case "TIME":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 日期时间族的规范声明类型（SQLite 只认亲和性，这里保留可读的类型名）
        /// </summary>
        private static string toCanonicalDateTimeType(string jsonType)
        {
            switch ((jsonType ?? "").Trim().ToUpperInvariant())
            {
                case "DATE":
                    return "DATE";
                case "TIME":
                    return "TIME";
                default:
                    return "DATETIME";
            }
        }

        /// <summary>
        /// 取出用于格式判定的日期时间样值（即 defaultValue）：
        /// 数字会被转成字符串参与解析；空值不作为样值
        /// </summary>
        private static string getDateTimeSample(FieldDef def)
        {
            JToken token = def.defaultValue;
            if (token == null || token.Type == JTokenType.Null || token.Type == JTokenType.Undefined)
                return null;

            if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float)
                return token.ToString();

            if (token.Type != JTokenType.String)
                return null;

            string text = (token.Value<string>() ?? "").Trim();
            return text.Length == 0 ? null : text;
        }

        /// <summary>
        /// 将 JSON 中的类型映射为 SQLite 类型。
        /// SQLite 只有 TEXT / INTEGER / REAL / BLOB / NUMERIC 五种存储亲和性，这里按亲和性归类；
        /// 日期时间族的具体类型由 resolveSqliteType 结合默认值格式决定，这里只作为兜底返回 TEXT。
        /// </summary>
        private static string mapToSqliteType(string jsonType)
        {
            switch ((jsonType ?? "").Trim().ToUpperInvariant())
            {
                // 文本
                case "TEXT":
                case "VARCHAR":
                case "VARCHAR2":
                case "NVARCHAR":
                case "CHAR":
                case "NCHAR":
                case "STRING":
                case "LONGTEXT":
                case "MEDIUMTEXT":
                case "JSON":
                    return "TEXT";

                // 日期时间族：具体的列类型由 resolveSqliteType 结合默认值格式决定（DATETIME / DATE / TIME / TEXT）
                case "DATETIME":
                case "DATETIME2":
                case "SMALLDATETIME":
                case "TIMESTAMP":
                case "DATE":
                case "TIME":
                    return "TEXT";

                // 整数；SQLite 无布尔类型，布尔统一按 1/0 存 INTEGER
                case "INTEGER":
                case "INT":
                case "BIGINT":
                case "SMALLINT":
                case "TINYINT":
                case "BOOLEAN":
                case "BOOL":
                    return "INTEGER";

                case "REAL":
                case "FLOAT":
                case "DOUBLE":
                case "DECIMAL":
                    return "REAL";

                case "BLOB":
                    return "BLOB";

                case "NUMERIC":
                    return "NUMERIC";

                default:
                    WriteLog($"未识别的字段类型 \"{jsonType}\"，按 TEXT 处理");
                    return "TEXT";
            }
        }

        /// <summary>
        /// 把 JSON 中的 defaultValue 转换为 SQL 字面量。
        /// 规则：字符串按文本处理并转义单引号（始终是常量默认值，可直接用于 ADD COLUMN）；
        /// 目标为数字类型时允许直接写数字字符串；布尔值统一存为 1/0（SQLite 无布尔类型）。
        /// </summary>
        /// <param name="columnName">字段名（仅用于错误提示）</param>
        /// <param name="token">defaultValue 原始节点</param>
        /// <param name="sqliteType">已映射好的 SQLite 列类型（TEXT/INTEGER/REAL/BLOB/NUMERIC/DATETIME/DATE/TIME）</param>
        private static string BuildDefaultLiteral(string columnName, JToken token, string sqliteType)
        {
            if (token == null || token.Type == JTokenType.Null || token.Type == JTokenType.Undefined)
                return "NULL";

            switch (token.Type)
            {
                case JTokenType.Boolean:
                    return token.Value<bool>() ? "1" : "0";

                case JTokenType.Integer:
                    return Convert.ToInt64(token).ToString(CultureInfo.InvariantCulture);

                case JTokenType.Float:
                    return Convert.ToDouble(token).ToString("R", CultureInfo.InvariantCulture);

                case JTokenType.String:
                    string raw = token.Value<string>() ?? "";
                    string probe = raw.Trim(); // 仅用于数字判断，输出时保留原字符串

                    // 数字类型允许用字符串配置默认值，避免 SQLite 隐式转换造成类型不一致
                    if (sqliteType == "INTEGER")
                    {
                        long longValue;
                        if (long.TryParse(probe, NumberStyles.Integer, CultureInfo.InvariantCulture, out longValue))
                            return longValue.ToString(CultureInfo.InvariantCulture);
                    }
                    else if (sqliteType == "REAL" || sqliteType == "NUMERIC")
                    {
                        double doubleValue;
                        if (double.TryParse(probe, NumberStyles.Float, CultureInfo.InvariantCulture, out doubleValue))
                            return doubleValue.ToString("R", CultureInfo.InvariantCulture);
                    }

                    return QuoteSqlString(raw);

                default:
                    throw new ArgumentException(
                        $"字段 {columnName} 的 defaultValue 只支持 字符串/数字/布尔/null，当前为 {token.Type}");
            }
        }

        /// <summary>
        /// 输出同步日志：控制台 + 应用日志文件（Console 在 WinForms 下不可见）
        /// </summary>
        private static void WriteLog(string msg)
        {
            Console.WriteLine(msg);
            try
            {
                Logger.CreateLog(msg, "数据库结构同步");
            }
            catch
            {
                // 日志失败不能影响数据库结构同步
            }
        }

        /// <summary>
        /// 字符串转 SQL 字面量（SQLite 用两个单引号转义单引号，不支持反斜杠转义）
        /// </summary>
        private static string QuoteSqlString(string value)
        {
            return "'" + (value ?? "").Replace("'", "''") + "'";
        }

        /// <summary>
        /// 判断表是否存在
        /// </summary>
        private static bool TableExists(SqliteConnection conn, string tableName, SqliteTransaction tran = null)
        {
            string sql = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@name;";
            using (var cmd = new SqliteCommand(sql, conn))
            {
                if (tran != null) cmd.Transaction = tran;
                cmd.Parameters.AddWithValue("@name", tableName);
                long count = Convert.ToInt64(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        /// <summary>
        /// 获取表已存在的字段名
        /// </summary>
        private static HashSet<string> getTableColumns(SqliteConnection conn, string tableName, SqliteTransaction tran = null)
        {
            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string sql = $"PRAGMA table_info(\"{tableName}\");";

            using (var cmd = new SqliteCommand(sql, conn))
            {
                if (tran != null) cmd.Transaction = tran;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // PRAGMA table_info 返回列: cid, name, type, notnull, dflt_value, pk
                        //string name = reader.GetString(1);
                        string name = reader["name"].ToString();
                        columns.Add(name);
                    }
                }
            }
            return columns;
        }

        /// <summary>
        /// 简单的标识符校验，防止 SQL 注入
        /// </summary>
        private static void ValidateIdentifier(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("表名或字段名不能为空");

            foreach (char c in name)
            {
                if (!(char.IsLetterOrDigit(c) || c == '_'))
                    throw new ArgumentException($"非法的标识符: {name}");
            }
        }

        class FieldItem
        {
            public string FieldName { get; set; }
            public FieldDef Def { get; set; }
        }

        class FieldDef
        {
            public string type { get; set; }
            public int length { get; set; }
            public bool isPrimaryKey { get; set; } = false;

            /// <summary>
            /// JSON 中配置的 defaultValue 原始值，未配置时为 null
            /// </summary>
            public JToken defaultValue { get; set; }

            /// <summary>
            /// 是否配置了有效的默认值（未配置或显式写 "defaultValue": null 均为 false）
            /// </summary>
            public bool HasDefaultValue
            {
                get
                {
                    return defaultValue != null
                        && defaultValue.Type != JTokenType.Null
                        && defaultValue.Type != JTokenType.Undefined;
                }
            }
        }
    }
}
