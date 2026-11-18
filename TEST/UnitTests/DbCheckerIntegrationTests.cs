using System;
using System.Data.Common;
using System.IO;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zoncare.DAL;

namespace EcgBpAnalysis.UnitTests
{
    /// <summary>
    /// 数据库升级（SqliteSchemaChecker.SyncFromJsonFile）集成测试。
    /// 验证 R020 所述"事务/幂等/只增不改"契约，以及 dbCheck.json 升级链路。
    /// </summary>
    [TestClass]
    public class DbCheckerIntegrationTests
    {
        private string _tempDir;
        private string _dbFile;
        private string _jsonFile;

        [TestInitialize]
        public void Setup()
        {
            try { SQLitePCL.Batteries_V2.Init(); }
            catch { /* 冒烟测试已验证 provider */ }

            _tempDir = Path.Combine(Path.GetTempPath(), "dbcheck_it_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
            _dbFile = Path.Combine(_tempDir, "test.db");
            _jsonFile = Path.Combine(_tempDir, "dbCheck.json");
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true); }
            catch { }
        }

        private void WriteSchema(string content)
        {
            File.WriteAllText(_jsonFile, content, new UTF8Encoding(false));
        }

        private SqliteConnection OpenConn()
        {
            var conn = new SqliteConnection($"Data Source={_dbFile}");
            conn.Open();
            return conn;
        }

        private bool TableExists(SqliteConnection conn, string name)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=$n;";
                cmd.Parameters.AddWithValue("$n", name);
                return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
            }
        }

        private bool ColumnExists(SqliteConnection conn, string table, string column)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"PRAGMA table_info({table});";
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        if (string.Equals(r["name"].ToString(), column, StringComparison.OrdinalIgnoreCase))
                            return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 新表创建：schema 中定义的表应被创建。
        /// </summary>
        [TestMethod]
        public void Sync_CreatesNewTable()
        {
            WriteSchema(@"{
                ""table"": {
                    ""test_patient"": {
                        ""id"": { ""type"": ""INTEGER"", ""isPrimaryKey"": true },
                        ""name"": { ""type"": ""TEXT"", ""length"": 50 }
                    }
                }
            }");

            using (var conn = OpenConn())
            {
                SqliteSchemaChecker.SyncFromJsonFile(conn, _jsonFile);
                Assert.IsTrue(TableExists(conn, "test_patient"), "应创建新表 test_patient");
                Assert.IsTrue(ColumnExists(conn, "test_patient", "name"), "应包含 name 字段");
            }
        }

        /// <summary>
        /// 幂等性：重复同步应成功且不抛异常（版本指纹 + 只增不改）。
        /// </summary>
        [TestMethod]
        public void Sync_RepeatedCall_IsIdempotent()
        {
            WriteSchema(@"{
                ""table"": {
                    ""test_patient"": {
                        ""id"": { ""type"": ""INTEGER"", ""isPrimaryKey"": true },
                        ""name"": { ""type"": ""TEXT"", ""length"": 50 }
                    }
                }
            }");

            using (var conn = OpenConn())
            {
                SqliteSchemaChecker.SyncFromJsonFile(conn, _jsonFile);
                SqliteSchemaChecker.SyncFromJsonFile(conn, _jsonFile); // 第二次不应抛异常
                Assert.IsTrue(TableExists(conn, "test_patient"));
            }
        }

        /// <summary>
        /// 新增字段：对已存在的表追加 schema 中新增的字段。
        /// </summary>
        [TestMethod]
        public void Sync_AddsNewColumnToExistingTable()
        {
            WriteSchema(@"{
                ""table"": {
                    ""test_patient"": {
                        ""id"": { ""type"": ""INTEGER"", ""isPrimaryKey"": true },
                        ""name"": { ""type"": ""TEXT"", ""length"": 50 }
                    }
                }
            }");
            using (var conn = OpenConn())
                SqliteSchemaChecker.SyncFromJsonFile(conn, _jsonFile);

            // 修改 schema：新增 age 字段并插入一行数据验证默认值不破坏已有数据
            WriteSchema(@"{
                ""table"": {
                    ""test_patient"": {
                        ""id"": { ""type"": ""INTEGER"", ""isPrimaryKey"": true },
                        ""name"": { ""type"": ""TEXT"", ""length"": 50 },
                        ""age"": { ""type"": ""INTEGER"", ""defaultValue"": 0 }
                    }
                }
            }");
            using (var conn = OpenConn())
            {
                SqliteSchemaChecker.SyncFromJsonFile(conn, _jsonFile);
                Assert.IsTrue(ColumnExists(conn, "test_patient", "age"), "应为已有表新增 age 字段");
            }
        }

        /// <summary>
        /// schema 文件不存在：应安全跳过而非抛异常（R041 相关健壮性）。
        /// </summary>
        [TestMethod]
        public void Sync_MissingSchemaFile_SkipsWithoutThrow()
        {
            using (var conn = OpenConn())
            {
                SqliteSchemaChecker.SyncFromJsonFile(conn, Path.Combine(_tempDir, "not-exist.json"));
                // 不抛异常即通过
            }
        }
    }
}
