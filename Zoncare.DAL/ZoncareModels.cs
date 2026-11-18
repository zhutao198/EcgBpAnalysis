using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using Zoncare.Common;
using Zoncare.Utilities;

namespace Zoncare.DAL
{
    //此属性用于指定配置类
    //[DbConfigurationType(typeof(MyConfiguration))]
    public class ZoncareEntities : DbContext
    {
        private const string ACCESS_KEY = "zq2877";

        public const string DefaultDbFileName = "ecg.db";

        private static string dbFileName;
        private static string dbConnString;
        private static SqliteConnection dbConn;
        private static ZoncareEntities dbEntities;
        private static object _lockObject = new object();

        /// <summary>
        /// 实例化方式（构造函数传入数据库文件/连接）创建的上下文所独占的数据库连接；
        /// 为 null 表示使用静态共享连接
        /// </summary>
        private SqliteConnection _ownedConnection;

        public static SqliteConnection DbConnection { get => dbConn; set => dbConn = value; }
        public static bool IsReadyDbConn { get; set; }
        public static bool IsReadyDbContext { get; set; }

        /// <summary>
        /// 静态单例 <see cref="Instance"/> 实际使用的连接字符串
        /// </summary>
        public static string DbConnectionString
        {
            get
            {
                if (!string.IsNullOrEmpty(dbConnString))
                {
                    return dbConnString;
                }

                //未经过 Init（例如由外部直接给 DbConnection 赋值）时，退化为连接自身的字符串
                return dbConn != null ? dbConn.ConnectionString : null;
            }
        }

        public static ZoncareEntities Instance
        {
            get
            {
                lock (_lockObject)
                {
                    if (dbEntities != null)
                    {
                        return dbEntities;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(dbFileName))
                        {
                            //连接参数唯一来源：BuildConnectionString（文件路径 -> 连接字符串）
                            initDatabaseConnection(BuildConnectionString(dbFileName));

                            dbEntities = new ZoncareEntities();
                            return dbEntities;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 默认构造函数：使用静态共享连接（须先调用 <see cref="Init"/> 完成初始化），保持原有单例用法不变
        /// </summary>
        public ZoncareEntities()
            : base(CreateOptionsForStaticConnection())
        {
            dbEntities = this;

            //Database.Log = (sql) => { Console.WriteLine(sql); };
        }

        /// <summary>
        /// 实例化用法：以指定的数据库文件（或完整的 Sqlite 连接字符串）创建独立的上下文实例。
        /// 该实例持有自己独占的连接，可与其他实例（或静态单例）同时存在、互不影响。
        /// 注意：EF Core 不会释放通过 UseSqlite(connection) 传入的外部连接，因此务必使用 using 或显式调用
        /// <see cref="Dispose"/> 释放实例，否则数据库文件句柄会一直占用；
        /// </summary>
        /// <param name="dbFileFullNameOrConnectionString">
        /// 数据库文件全路径（如 "D:\data\ecg.db"），或含 "=" 的完整 Sqlite 连接字符串
        /// </param>
        public ZoncareEntities(string dbFileFullNameOrConnectionString)
            : this(CreateConnection(dbFileFullNameOrConnectionString), true)
        {
        }

        /// <summary>
        /// 实例化用法：使用调用方提供的连接创建上下文实例。
        /// 典型用法是复用静态共享连接（<see cref="DbConnection"/>，即 <see cref="Instance"/> 正在使用的连接）
        /// 创建一个新的上下文，而不会影响静态单例 <see cref="Instance"/>：
        /// </summary>
        /// <param name="connection">
        /// 已存在的 Sqlite 连接；若连接尚未打开，则由本实例负责打开并完成加密等参数配置（PRAGMA key）；
        /// 若连接已打开，则视为调用方已完成连接配置，本实例不再修改该连接。
        /// 传入连接为 null 时抛出 <see cref="ArgumentNullException"/>
        /// </param>
        /// <param name="ownsConnection">
        /// 是否由本实例负责释放该连接，默认为 true。
        /// 特例：若 <paramref name="connection"/> 正是静态共享连接（<see cref="DbConnection"/>），
        /// 则无论本参数取值如何都不会被本实例释放，以保证 <see cref="Instance"/> 始终可用
        /// </param>
        public ZoncareEntities(SqliteConnection connection, bool ownsConnection = true)
            : base(CreateOptions(EnsureNotNull(connection)))
        {
            if (connection.State != ConnectionState.Open)
            {
                OpenAndConfigConnection(connection);
            }

            //静态共享连接永不随实例释放：即使调用方误传 ownsConnection = true，
            //也不会导致 ZoncareEntities.Instance 的连接被释放而失效
            _ownedConnection = ownsConnection && !ReferenceEquals(connection, dbConn) ? connection : null;
        }

        /// <summary>
        /// 实例化用法：使用上下文选项创建实例（便于依赖注入、单元测试等场景）。
        /// 连接的所有权由调用方（或选项配置方）负责，本实例不负责释放。
        /// </summary>
        /// <param name="options">上下文选项，其中应已配置数据库提供程序与连接</param>
        public ZoncareEntities(DbContextOptions<ZoncareEntities> options)
            : base(options)
        {
        }

        /// <summary>
        /// 以指定的数据库文件（或 Sqlite 连接字符串）创建上下文实例（等价于 <see cref="ZoncareEntities(string)"/>）。
        /// 使用完毕后请释放该实例，否则连接与数据库文件句柄不会释放。
        /// </summary>
        /// <param name="dbFileFullNameOrConnectionString">数据库文件全路径或 Sqlite 连接字符串</param>
        /// <returns>已连接指定数据库的上下文实例，使用完毕后请释放</returns>
        public static ZoncareEntities Create(string dbFileFullNameOrConnectionString)
        {
            return new ZoncareEntities(dbFileFullNameOrConnectionString);
        }

        /// <summary>
        /// 复用静态共享连接（<see cref="DbConnection"/>）创建一个新的上下文实例，
        /// 供"与静态单例 <see cref="Instance"/> 共用同一连接，但不影响单例"的场景使用（等价于 <c>new ZoncareEntities(DbConnection, false)</c>）。
        /// 该实例不持有连接所有权，释放时不会关闭/释放静态连接，<see cref="Instance"/> 不受影响；
        /// 同时也<strong>不会</strong>改写静态单例 <c>dbEntities</c>（这点与 <see cref="ZoncareEntities()"/> 不同，后者会把 <see cref="Instance"/> 替换为新实例）。
        /// 注意：与 <see cref="Instance"/> 共用同一物理连接，事务与连接级设置是共享的，且不可多线程并发使用。
        /// </summary>
        /// <returns>使用静态共享连接的上下文实例，使用完毕后请释放（不会释放其连接）</returns>
        /// <exception cref="InvalidOperationException">静态共享连接尚未初始化（未调用 <see cref="Init"/> 或 <see cref="Instance"/>）</exception>
        public static ZoncareEntities CreateShared()
        {
            SqliteConnection connection = dbConn;

            if (connection == null)
            {
                throw new InvalidOperationException("静态共享连接尚未初始化，请先调用 ZoncareEntities.Init(dbFileFullName) 或访问 ZoncareEntities.Instance");
            }

            return new ZoncareEntities(connection, false);
        }

        /// <summary>
        /// 使用与静态单例 <see cref="Instance"/> <strong>完全相同的连接参数</strong>
        /// </summary>
        /// <returns>使用同一套连接参数、但拥有独立连接的上下文实例；使用完毕后请释放</returns>
        public static ZoncareEntities CreateIndependent()
        {
            string connString = DbConnectionString;

            if (string.IsNullOrEmpty(connString))
            {
                throw new InvalidOperationException("尚未初始化连接参数，请先调用 ZoncareEntities.Init() / Init(dbFileFullName) 或访问 ZoncareEntities.Instance");
            }

            return new ZoncareEntities(connString);
        }


        /// <summary>
        /// 初始化数据库文件及运行时环境（使用默认数据库文件 <see cref="DefaultDbFileName"/>）。
        /// 各进程入口调用本重载即可，数据库文件路径只在 <see cref="DefaultDbFileName"/> 一处定义
        /// </summary>
        public static void Init()
        {
            Init(PubFuns.GetAbsolutePath(DefaultDbFileName));
        }

        /// <summary>
        /// 初始化数据库文件及运行时环境。
        /// 此处传入的路径是全局唯一的"连接参数"来源：<see cref="Instance"/> 使用 <see cref="BuildConnectionString"/> 由它生成连接字符串，
        /// 之后 <see cref="CreateIndependent"/> 与 <see cref="DbConnectionString"/> 都复用同一份参数
        /// </summary>
        /// <param name="dbFileFullName">数据库文件全路径</param>
        public static void Init(string dbFileFullName)
        {
            dbFileName = dbFileFullName;
            dbEntities = null;

            Batteries_V2.Init();
            //raw.SetProvider(new SQLite3Provider_e_sqlcipher());
        }

        //初始化数据库连接
        private static void initDatabaseConnection(string connString)
        {
            dbConnString = connString;

            dbConn = new SqliteConnection(connString);
            OpenAndConfigConnection(dbConn);

            try
            {
                using (SqliteCommand cmd = new SqliteCommand("SELECT COUNT(*) FROM sqlite_master;", dbConn))
                {
                    cmd.ExecuteScalar();
                    IsReadyDbConn = true;
                }

                SqliteSchemaChecker.SyncFromJsonFile(dbConn, PubFuns.GetAbsolutePath("config\\dbCheck.json"));
            }
            catch (SqliteException dbEx)
            {
                dbConn.Close();
            }
            catch (Exception ex)
            {
                dbConn.Close();
            }
        }

        /// <summary>
        /// 创建并打开一个已完成加密等参数配置的数据库连接（供实例化用法使用）。
        /// </summary>
        /// <param name="dbFileFullNameOrConnectionString">数据库文件全路径，或完整的 Sqlite 连接字符串</param>
        /// <returns>已打开且已设置加密密钥的连接，使用完毕后请自行关闭并释放</returns>
        public static SqliteConnection CreateConnection(string dbFileFullNameOrConnectionString)
        {
            SqliteConnection connection = new SqliteConnection(BuildConnectionString(dbFileFullNameOrConnectionString));
            OpenAndConfigConnection(connection);
            return connection;
        }

        /// <summary>
        /// 打开连接并完成加密（PRAGMA key）等运行参数配置；静态共享连接与实例独占连接均使用本方法
        /// </summary>
        /// <param name="connection">待打开/配置的 Sqlite 连接</param>
        public static void OpenAndConfigConnection(SqliteConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            //初始化 SQLite 运行时环境（可重复调用）
            Batteries_V2.Init();

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            ApplyConnectionPragmas(connection);
        }

        /// <summary>
        /// 设置加密密钥及运行参数（必须在访问数据库数据之前执行）
        /// </summary>
        /// <param name="connection">已打开的 Sqlite 连接</param>
        private static void ApplyConnectionPragmas(SqliteConnection connection)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA key = '" + ACCESS_KEY + "'";
                int execResult1 = cmd.ExecuteNonQuery();

                cmd.CommandText = "PRAGMA cipher_compatibility = 3;";
                int execResult2 = cmd.ExecuteNonQuery();

                // 关闭内存加密校验（SQLCipher 4 默认开启，每次读写均有额外校验开销；业务库场景可安全关闭）
                cmd.CommandText = "PRAGMA cipher_memory_security = OFF;";
                int execResult3 = cmd.ExecuteNonQuery();

                // 增大缓存页数（默认 2000，根据内存调整）-kB 单位
                cmd.CommandText = "PRAGMA cache_size = -10000;";
                int execResult4 = cmd.ExecuteNonQuery();

                // 允许一个写入者与多个读取者并发
                cmd.CommandText = "PRAGMA journal_mode=WAL;";
                int execResult5 = cmd.ExecuteNonQuery();

                //// 禁用外键约束检查（如果不需要）
                //cmd.CommandText = "PRAGMA foreign_keys = OFF;";
                //int execResult6 = cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 将数据库文件全路径转换为 Sqlite 连接字符串（若传入的已是连接字符串则直接返回）。
        /// <para>
        /// 本方法是<strong>全项目唯一定义连接参数的地方</strong>：数据库文件、打开模式（ReadWriteCreate）、外键开关都在这里设置，
        /// <see cref="Instance"/>、<see cref="CreateIndependent"/> 等所有入口都经由它生成连接字符串，
        /// 因此新增实例时不要再自己拼连接字符串，直接复用这些入口即可。
        /// </para>
        /// </summary>
        /// <param name="dbFileFullNameOrConnectionString">数据库文件全路径或 Sqlite 连接字符串</param>
        private static string BuildConnectionString(string dbFileFullNameOrConnectionString)
        {
            if (string.IsNullOrWhiteSpace(dbFileFullNameOrConnectionString))
            {
                throw new ArgumentException("数据库文件路径或连接字符串不能为空", nameof(dbFileFullNameOrConnectionString));
            }

            //含 "=" 时视为连接字符串，例如 "Data Source=D:\data\ecg.db;Mode=ReadWriteCreate"
            if (dbFileFullNameOrConnectionString.IndexOf('=') >= 0)
            {
                return dbFileFullNameOrConnectionString;
            }

            return new SqliteConnectionStringBuilder()
            {
                DataSource = dbFileFullNameOrConnectionString,
                Mode = SqliteOpenMode.ReadWriteCreate,
                ForeignKeys = true
            }.ToString();
        }

        /// <summary>
        /// 构造使用静态共享连接的上下文选项（供默认构造函数使用）
        /// </summary>
        private static DbContextOptions CreateOptionsForStaticConnection()
        {
            //静态连接尚未初始化时返回空选项，由 OnConfiguring 在使用阶段补充配置（保持原有报错时机）
            if (dbConn == null)
            {
                return new DbContextOptionsBuilder<ZoncareEntities>().Options;
            }

            return CreateOptions(dbConn);
        }

        /// <summary>
        /// 构造使用指定连接的上下文选项
        /// </summary>
        /// <param name="connection">供上下文使用的 Sqlite 连接</param>
        private static DbContextOptions CreateOptions(SqliteConnection connection)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ZoncareEntities>();
            optionsBuilder.UseSqlite(connection);

            return optionsBuilder.Options;
        }

        /// <summary>
        /// 校验连接非空（供构造函数初始化器使用）
        /// </summary>
        /// <param name="connection">待校验的连接</param>
        private static SqliteConnection EnsureNotNull(SqliteConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            return connection;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //实例化用法（构造函数已配置数据库提供程序）时不再重复配置，避免连接被覆盖
            if (optionsBuilder.IsConfigured)
            {
                return;
            }

            optionsBuilder.UseSqlite(dbConn);

            // loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            //{
            //    builder
            //        .AddFilter((category, level) =>
            //            category == DbLoggerCategory.Database.Command.Name &&
            //            level == LogLevel.Information)
            //        .AddConsole(); // 输出到控制台
            //});
            //optionsBuilder.UseLoggerFactory(loggerFactory);
        }

        /// <summary>
        /// 生成数据库模型时引发的方法
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 显式指定表名为单数形式
            modelBuilder.Entity<user>().ToTable("user");
            //modelBuilder.Entity<OperateLog>().ToTable("operate_log");
            modelBuilder.Entity<basedict>().ToTable("basedict");
            modelBuilder.Entity<advancefeature>().ToTable("advancefeature");
            modelBuilder.Entity<ecgreport>().ToTable("ecgreport");
            modelBuilder.Entity<ecgdiagnosis>().ToTable("ecgdiagnosis");

            //敏感信息加解密
            modelBuilder.Entity<examCase>().Property(e => e.name).HasConversion(new EncryptedConverter());
            modelBuilder.Entity<examCase>().Property(e => e.birthday).HasConversion(new EncryptedConverter());
            modelBuilder.Entity<examCase>().Property(e => e.patientNo).HasConversion(new EncryptedConverter());
            modelBuilder.Entity<examCase>().Property(e => e.bedNo).HasConversion(new EncryptedConverter());
            modelBuilder.Entity<examCase>().Property(e => e.idcard).HasConversion(new EncryptedConverter());
            modelBuilder.Entity<examCase>().Property(e => e.address).HasConversion(new EncryptedConverter());
            modelBuilder.Entity<examCase>().Property(e => e.phone).HasConversion(new EncryptedConverter());

            //ecg_collect.ecg_id -> ecg.id 的逻辑外键(仅在模型层指定)，单向关系：仅从 examCase 侧指向 collectCase，
            //查病例记录时通过 Include(e => e.collectCases) 或投影即可连带查出收藏记录(如 collectCase.id)。
            modelBuilder.Entity<examCase>()
                .HasMany(e => e.collectCases)
                .WithOne()
                .HasForeignKey(c => c.ecgId)
                .IsRequired(false); //ecgId 可空，可选关系，生成 LEFT JOIN
        }

        public int ExecuteRawSql(string sql)
        {
            return Database.ExecuteSqlCommand(sql);
        }

        /// <summary>
        /// 通过执行SQL语句获取表数据
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static DataTable RawQuery(string sql)
        {
            DataTable dataTable = new DataTable();
            using (SqliteCommand cmd = new SqliteCommand(sql, dbConn))
            {
                try
                {
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        dataTable.Load(reader);
                    }
                }
                catch
                {
                }
            }

            return dataTable;
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static int RawExecute(string sql, SqliteTransaction tran = null)
        {
            using (SqliteCommand cmd = new SqliteCommand(sql, dbConn))
            {
                if (tran != null) cmd.Transaction = tran;
                try
                {
                    return cmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        /// <summary>
        /// 重写SaveChanges 方法, 更新系统内静态变量(数据集)并清除未处理的异常
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges()
        {
            int result = 0;
            try
            {
                result = base.SaveChanges();

            }
            catch (Exception ex)
            {
                if (ex is DbUpdateException)
                {
                    foreach (EntityEntry entry in (ex as DbUpdateException).Entries)
                    {
                        switch (entry.State)
                        {
                            case EntityState.Added:
                                entry.State = EntityState.Detached;
                                break;
                            case EntityState.Modified:
                                entry.CurrentValues.SetValues(entry.OriginalValues);
                                entry.State = EntityState.Unchanged;
                                break;
                            case EntityState.Deleted:
                                entry.State = EntityState.Unchanged;
                                break;
                        }
                    }
                }

                throw new Exception("数据保存失败", ex);
            }

            return result;
        }

        public void RefreshCache()
        {
            var entries = this.ChangeTracker.Entries().ToList();
            foreach (var entry in entries)
            {
                entry.State = EntityState.Detached; // 分离所有实体
            }
        }

        /// <summary>
        /// 当前上下文实际使用的数据库连接
        /// </summary>
        public SqliteConnection Connection => Database.GetDbConnection() as SqliteConnection;

        /// <summary>
        /// 释放上下文。实例化用法下同时关闭并释放本实例独占的连接；静态共享连接不受影响。
        /// 说明：EF Core 2.2 的 DbContext 只有无参 Dispose()（无 Dispose(bool) 可重写），故此处重写无参版本。
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            SqliteConnection connection = _ownedConnection;
            _ownedConnection = null;

            if (connection == null)
            {
                return;
            }

            try
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
            catch (Exception)
            {
                //忽略连接关闭异常
            }

            connection.Dispose();
        }

        public DbSet<examCase> ExamCases { get; set; }
        public DbSet<collectCase> CollectCases { get; set; }
        public DbSet<bpDetail> BpDetails { get; set; }
        public DbSet<bpTrendRemark> BpTrendRemarks { get; set; }
        public DbSet<tableColumns> TableColumns { get; set; }
        public DbSet<customEvent> CustomEvents { get; set; }
        public DbSet<department> Departments { get; set; }
        public DbSet<ecgDictionarySelf> EcgDictionarySelf { get; set; }
        public DbSet<holterMenuSet> HolterMenuSets { get; set; }
        public DbSet<holterSetting> HolterSettings { get; set; }
        public DbSet<mainMenu> MainMenu { get; set; }

        public DbSet<ecgreport> EcgReport { get; set; }
        public DbSet<ecgdiagnosis> EcgDiagnosis { get; set; }
        public DbSet<user> User { get; set; }
        public DbSet<basedict> BaseDict { get; set; }
        public DbSet<OperateLog> Log { get; set; }
        public DbSet<advancefeature> AdvanceFeature { get; set; }
        public DbSet<sysSetting> SysSettings { get; set; }
        public DbSet<reportTemplate> ReportTemplates { get; set; }
    }

    public class EncryptedConverter : ValueConverter<string, string>
    {
        public EncryptedConverter()
            : base(
                v => AESEncrypt.Encrypt(v),  // 转换为数据库值
                v => AESEncrypt.Decrypt(v))  // 从数据库值转换
        { }
    }

    #region 数据库表映射类
    [Table("bp_detail")]
    public class bpDetail
    {
        [Key]
        public int id { get; set; }
        [Column("ecg_id")]
        public int caseId { get; set; }
        [Column("original_time")]
        public Nullable<System.DateTime> OriginalTime { get; set; }
        [Column("time")]
        public string time { get; set; }
        [Column("effective_systolic_pressure")]
        public int? sysPressure { get; set; }
        [Column("effective_diastolic_pressure")]
        public int? diaPressure { get; set; }

        [JsonProperty("effectiveSystolicPressure")]
        private int? sysPressure_old
        {
            set { sysPressure = value; }
        }
        [JsonProperty("effectiveDiastolicPressure")]
        private int? diaPressure_old
        {
            set { diaPressure = value; }
        }

        public int pulse { get; set; }

        [Column("mean_arterial_pressure")]
        public int mapPressure { get; set; }

        [JsonProperty("meanArterialPressure")]
        private int mapPressure_old
        {
            set { mapPressure = value; }
        }

        [Column("motion_state")]
        public string motionState { get; set; }
        [Column("status")]
        public int? status { get; set; }
        [Column("data_status")]
        public int? dataStatus { get; set; } //0:排除的数据; 1:正常数据
        [Column("pulse_wave")]
        public int? pulseWave { get; set; }

        [Column("notes")]
        public string notes { get; set; }
        [Column("error_code")]
        public string errorCode { get; set; }
        [Column("measure_type")]
        public int? measureType { get; set; }
        [Column("create_time")]
        public Nullable<System.DateTime> createTime { get; set; }
    }

    [Table("bp_trend_remark")]
    public class bpTrendRemark
    {
        [Key]
        public int id { get; set; }

        public int ecgId { get; set; }
        public string remark { get; set; }
    }

    [Table("column")]
    public class tableColumns
    {
        [Key]
        public int id { get; set; }
        [Column("user_id")]
        public int userId { get; set; }
        public string column { get; set; }
        public string remark { get; set; }
        public int? del_flag { get; set; }
        [Column("create_time")]
        public Nullable<System.DateTime> createTime { get; set; }
        [Column("create_by")]
        public string createBy { get; set; }
        [Column("update_time")]
        public Nullable<System.DateTime> updateTime { get; set; }
        [Column("update_by")]
        public string updateBy { get; set; }
    }

    [Table("custom_event")]
    public class customEvent
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public string letter { get; set; }
        public string remark { get; set; }
        public int? del_flag { get; set; }
        [Column("create_time")]
        public Nullable<System.DateTime> createTime { get; set; }
        [Column("create_by")]
        public string createBy { get; set; }
        [Column("update_time")]
        public Nullable<System.DateTime> updateTime { get; set; }
        [Column("update_by")]
        public string updateBy { get; set; }
    }

    [Table("dept")]
    public class department
    {
        [Key]
        public int id { get; set; }
        [Column("dept_name")]
        public string deptName { get; set; }
        [Column("dept_code")]
        public string deptCode { get; set; }
        public string remark { get; set; }
        [Column("del_flag")]
        public int? delFlag { get; set; }
        [Column("create_time")]
        public Nullable<System.DateTime> createTime { get; set; }
        [Column("create_by")]
        public string createBy { get; set; }
        [Column("update_time")]
        public Nullable<System.DateTime> updateTime { get; set; }
        [Column("update_by")]
        public string updateBy { get; set; }
    }

    //病例表
    [Table("ecg")]
    public partial class examCase
    {
        [Key]
        public int id { get; set; }
        [Column("case_no")]
        public string caseNo { get; set; }
        /// <summary>
        /// 登记时间
        /// </summary>
        [Column("appointment_time")]
        public Nullable<System.DateTime> appointmentTime { get; set; }
        /// <summary>
        /// 相关数据的记录时间
        /// </summary>
        [Column("record_time")]
        public Nullable<System.DateTime> recordTime { get; set; }
        /// <summary>
        /// 上传时间
        /// </summary>
        [Column("upload_time")]
        public Nullable<System.DateTime> uploadTime { get; set; }
        [Column("id_type")]
        public int? idType { get; set; } //病人来源
        [Column("id_resource")]
        public string idResource { get; set; }

        public string @name { get; set; }
        public string sex { get; set; }
        [NotMapped, JsonIgnore]
        public string sexName => (SysConstants.Genders.ContainsKey(sex) ? SysConstants.Genders[sex] : SysConstants.Genders["U"]);
        public int? age { get; set; }
        [Column("age_days")]
        public int? ageDays { get; set; }
        [Column("age_unit")]
        public string ageUnit { get; set; }
        [NotMapped, JsonIgnore]
        public string ageName => CommonFuns.GetAgeText(age, ageUnit);
        [Column("birthday")]
        public string birthday { get; set; }
        public string height { get; set; }
        public string weight { get; set; }
        [Column("patient_no")]
        public string patientNo { get; set; }
        [Column("bed_no")]
        public string bedNo { get; set; }
        [Column("device_type")]
        public string deviceType { get; set; } //1:旧心电设备(IE90/IE95); 2:心电(8333); 3:血压(8337); 4:二合一设备(8377);
        [Column("device_model")]
        public string deviceModel { get; set; }
        [Column("device_no")]
        public string deviceNo { get; set; }
        [Column("check_item")]
        public int checkItem { get; set; } // 1:动态心电, 2:动态血压, 3:心电和血压
        [NotMapped, JsonIgnore]
        public string checkItemName => (checkItem == 0 ? "" : SysConstants.CheckItems[checkItem]);
        [Column("data_type")]
        public int? dataType { get; set; } // 数据类型(e.g. 12导联: 30秒)
        [NotMapped, JsonIgnore]
        public string dataTypeName
        {
            get
            {
                string timeString = CommonFuns.GetTimeDurationText(dataDuration);

                if (dataType.HasValue)
                {
                    if (!string.IsNullOrEmpty(timeString))
                        return dataType.Value + (dataType > 1 ? CommonValues.Langs["leads"] : CommonValues.Langs["lead"]) + ": " + timeString;
                    else
                        return dataType.Value + (dataType > 1 ? CommonValues.Langs["leads"] : CommonValues.Langs["lead"]);
                }
                else { return ""; }
            }
        }
        [NotMapped, JsonIgnore]
        public string leadAlarm { get; set; }
        [Column("data_duration")]
        public string dataDuration { get; set; }
        [Column("holter_auto_report_conclusion")]
        public string holterAutoReportConclusion { get; set; }
        [Column("holter_report_conclusion")]
        public string holterReportConclusion { get; set; }
        [Column("bp_report_conclusion")]
        public string bpReportConclusion { get; set; }
        [NotMapped, JsonIgnore]
        public string reportConclusion
        {
            get
            {
                if (CommonValues.SystemType == "bp")
                {
                    return bpReportConclusion;
                }
                else
                {
                    string ecgConclusion = "";
                    if (CommonValues.SystemSettings.reportSets.displayConclusionType == 0)
                        ecgConclusion = holterAutoReportConclusion;
                    else if (CommonValues.SystemSettings.reportSets.displayConclusionType == 1)
                        ecgConclusion = holterReportConclusion;
                    else
                        ecgConclusion = holterAutoReportConclusion + "\n" + holterReportConclusion;

                    if (CommonValues.SystemType == "ecg")
                        return ecgConclusion;
                    else
                        return ecgConclusion + "\n" + bpReportConclusion;
                }
            }
        }

        [Column("diagnose_sign")]
        public string diagnoseSign { get; set; }
        [Column("audit_sign")]
        public string auditSign { get; set; }
        [Column("check_dept")]
        public string checkDept { get; set; }
        [Column("check_doctor")]
        public string checkDoctor { get; set; }
        [Column("apply_dept")]
        public string applyDept { get; set; }

        [Column("apply_time")]
        public Nullable<System.DateTime> applyTime { get; set; }
        [Column("apply_doctor")]
        public string applyDoctor { get; set; }
        [Column("diagnose_time")]
        public Nullable<System.DateTime> diagnoseTime { get; set; }
        [Column("diagnose_doctor")]
        public string diagnoseDoctor { get; set; }
        [Column("audit_time")]
        public Nullable<System.DateTime> auditTime { get; set; }
        [Column("audit_doctor")]
        public string auditDoctor { get; set; }

        public string idcard { get; set; }
        public string address { get; set; }
        public string phone { get; set; }

        [Column("clinical_diagnosis")]
        public string clinicalDiagnosis { get; set; }
        [Column("medication")]
        public string medication { get; set; }
        [Column("has_pacemaker")]
        public int hasPacemaker { get; set; }

        [Column("file_directory")]
        public string fileDirectory { get; set; }
        [Column("file_num")]
        public int? fileNum { get; set; }
        [Column("file_path")]
        public string filePath { get; set; }

        [Column("status")]
        public int status { get; set; } = 1; //病例状态(1:已登记, 2:已上传, 3:已诊断, 4:已审核)
        [NotMapped, JsonIgnore]
        public string statusName
        {
            get
            {
                if (string.IsNullOrEmpty(webSourceId))
                {
                    return SysConstants.CaseStatus[status];
                }
                else
                {
                    if (status == 3 || status == 4)
                        return CommonValues.Langs["已审核"];
                    else
                        return CommonValues.Langs["未分析"];
                }
            }
        }

        [Column("print_status")]
        public int printStatus { get; set; } = 1; //打印状态

        public string remark { get; set; }

        [Column("cancel_reason")]
        public string cancelReason { get; set; }
        [Column("del_flag")]
        public int delFlag { get; set; }

        [Column("create_time")]
        public Nullable<System.DateTime> createTime { get; set; }
        [Column("create_by")]
        public string createBy { get; set; }
        [Column("update_time")]
        public Nullable<System.DateTime> updateTime { get; set; }
        [Column("update_by")]
        public string updateBy { get; set; }
        [Column("web_source_id")]
        public string webSourceId { get; set; }

        /// <summary>
        /// 收藏记录(ecg_collect 表)；数据库中未定义外键，仅在 EF 模型层指定关系，
        /// 查询病例时通过 Include(e => e.collectCases) 或投影即可连带查出收藏记录(如 id)。
        /// 一个病例可能被多个用户收藏，故为一对多集合
        /// </summary>
        public ICollection<collectCase> collectCases { get; set; }
    }

    //病例收藏表
    [Table("ecg_collect")]
    public class collectCase
    {
        [Key]
        public int id { get; set; }

        [Column("user_id")]
        public int? userId { get; set; }
        [Column("ecg_id")]
        public int? ecgId { get; set; } //关联病例(ecg.id)的逻辑外键，仅在模型层于 examCase 侧指定单向关系
        [Column("collect_time")]
        public Nullable<System.DateTime> collectTime { get; set; }
    }

    //报告术语表
    [Table("ecg_dictionary_self")]
    public class ecgDictionarySelf
    {
        [Key]
        public int id { get; set; }

        [Column("template_id")]
        public int? templateId { get; set; }
        [Column("parent_id")]
        public int? parentId { get; set; }
        [Column("dict_text")]
        public string dictText { get; set; }
        [Column("dict_en")]
        public string dictEn { get; set; }

        public int? sort { get; set; }
    }

    //分析菜单设置表
    [Table("holter_menu_set")]
    public class holterMenuSet
    {
        [Key]
        public int id { get; set; }

        [Column("user_id")]
        public int? userId { get; set; }
        public string title { get; set; }
        public string path { get; set; }
        public string icon { get; set; }

        [Column("is_select")]
        public int? isSelect { get; set; }
        public int? sort { get; set; }
    }

    //系统设置表(用户)
    [Table("holter_setting")]
    public class holterSetting
    {
        [Key]
        public int id { get; set; }

        [Column("user_id")]
        public int? userId { get; set; }
        [Column("setting")]
        public string ecgSetting { get; set; }
        [Column("bp_deviceSetting")]
        public string bpDeviceSetting { get; set; }
        [Column("bp_diagnoseSetting")]
        public string bpDiagnoseSetting { get; set; }
        [Column("update_time")]
        public Nullable<System.DateTime> updateTime { get; set; }
    }

    //主界面菜单表
    [Table("menu")]
    public class mainMenu
    {
        [Key]
        public int id { get; set; }

        [Column("user_id")]
        public int? userId { get; set; }

        [Column("menu_name")]
        public string menuName { get; set; }
        [Column("menu_code")]
        public string menuCode { get; set; }
        [Column("remark")]
        public string remark { get; set; }
        [Column("del_flag")]
        public int? delFlag { get; set; }
        [Column("create_time")]
        public Nullable<System.DateTime> createTime { get; set; }
        [Column("create_by")]
        public string createBy { get; set; }
        [Column("update_time")]
        public Nullable<System.DateTime> updateTime { get; set; }
        [Column("update_by")]
        public string updateBy { get; set; }
    }

    //病历登记表
    [Table("ecgreport")]
    public class ecgreport
    {
        [Key]
        public long id { get; set; }                    // 数据库中ID
        public string pid { get; set; }                 // 患者ID
        public string name { get; set; }                // 姓名
        public string gender { get; set; }              // 性别: M(男)、F(女)、U(未知)
        public int? age { get; set; }                   // 年龄
        public string ageUnit { get; set; }             // 年龄单位：Y(岁)、M(月)、D(天)、U(未知)，默认为“U”
        public int? examinationCategory { get; set; }   // 检查类别：0(标准心电)，1(向量心电)，2(时间向量)，3(心室晚心位)，4(频谱心电)，5(QT离散度)，6(心率变异性)，7(节律分析)
        public int? examinationItem { get; set; }       // 检查项目：0(6导)，1(12导)，2(15导)，3(18导), 4(9导)
        public string examinationDoctor { get; set; }   // 检查医生
        public string examinationTime { get; set; }     // 检查时间，例如：2018-08-16 09:56:04
        [NotMapped]
        public string diagnosticDoctor { get; set; }   // 诊断医生
        [NotMapped]
        public string diagnosticTime { get; set; }     // 诊断时间

        public string hospitalSection { get; set; }    // 病区
        public string ward { get; set; }               // 病房
        public string bedNo { get; set; }              // 床号
        public string birthdate { get; set; }          // 出生日期，例如：2018-08-16
        public string patientSource { get; set; }      // 病人来源，例如：体检，门诊，住院，急诊
        public string applicationNo { get; set; }      // 申请号
        public string applicationDept { get; set; }    // 申请科室
        public string applicationDoctor { get; set; }  // 申请医生
        public string applicationTime { get; set; }    // 申请时间
        public int? height { get; set; }                 // 身高
        public double? weight { get; set; }              // 体重
        public string bloodPressure { get; set; }      // 血压，例如：90/140
        public string hospitalNo { get; set; }         // 门诊/住院/体检号
        public string examinationDept { get; set; }    // 检查科室
        public string examinationRoom { get; set; }    // 检查房间
        public string examinationPurpose { get; set; } // 检查目的
        public string clinicalDiagnosis { get; set; }  // 临床诊断
        public string race { get; set; }               // 种族: C(白人)、N(非洲)、M(东方)、O(其他)、U(未知)
        public string drug { get; set; }               // 用药：0(未定义)、1(没有用药)、2(未知药物)、3(洋地黄)、4(利尿剂)、5(β-受体阻滞药)、6(喹哪啶)、7(普鲁卡因)、8(胺碘达隆)、
                                                       //      9(达舒平)、10(利诺卡因)、11(其他抗心律失常药)、12(作用于精神的药物)、13(类固醇)、14(其他药)、255()
        public string prevDiag { get; set; }           // 先前诊断：0(未定义)、10(正常)、11(心肌梗死)、12(心肌缺血)、13(高血压)、15(先天性心脏病)、17(心脏瓣膜病)、30(心包炎)、
                                                       //         31(呼吸道疾病)、33(内分泌病)、46(植入起搏器)、47(肺栓塞)、49(术后改变)、50(其他)、51(心肌病)、52(未知)、255()

        public string fileDirectory { get; set; }       // 文件存储目录路径
        public string fileName { get; set; }            // 文件名
        public string filePath { get; set; }            // 文件路径
        public int? checkStatus { get; set; }			// 检查状态: 0(已登记), 1(已检查), 2(已诊断)，3（已审核）
        public int? isDeleted { get; set; }             // 是否已删除
    }

    //病历诊断表
    [Table("ecgdiagnosis")]
    public class ecgdiagnosis
    {
        [Key]
        public long id { get; set; }                     // 数据库中ID
        public long rid { get; set; }                    // 关联的ecgreport.id
        public int? hr { get; set; }                     // 心率
        public int? p { get; set; }                      // P时限
        public int? rr { get; set; }                     // RR间期
        public int? pr { get; set; }                     // PR间期
        public int? qrs { get; set; }                    // QRS时限
        public int? qt { get; set; }                     // QT间期
        public int? qtc { get; set; }                    // QTC间期
        public int? pAxis { get; set; }                  // P电轴
        public int? qrsAxis { get; set; }                // QRS电轴
        public int? tAxis { get; set; }                  // T电轴
        public double? rv5 { get; set; }                 // RV5值
        public double? sv1 { get; set; }                 // SV1值
        public int? pBegin { get; set; }                //平均模板P1所在位置
        public int? pEnd { get; set; }                   //平均模板P2所在位置
        public int? qBegin { get; set; }                //平均模板Q所在位置
        public int? sEnd { get; set; }                    //平均模板S所在位置
        public int? tBegin { get; set; }                  //平均模板T1所在位置
        public int? tEnd { get; set; }                     //平均模板T2所在位置
        public string minnesotaCode { get; set; }       // 明尼苏达码
        public string diagnosticResult { get; set; }    // 诊断结果
        public string diagnosticDoctor { get; set; }    // 诊断医生
        public string diagnosticTime { get; set; }      // 诊断时间
        public string loginUser { get; set; }           // 登录用户名
        public int? startMs { get; set; }                // 心电波形诊断起始位置，默认为0(实际存储波形数据点序号)
        public int? lead { get; set; }                   // 心电图导联数：6、12、15、18
        public double? speed { get; set; }               // 纸速(mm/s)：5, 6.25, 10, 12.5, 25, 50
        public double? limbGain { get; set; }           // 增益(mm/mv)：1.25, 2.5, 5, 10, 20, 40
        public double? chestGain { get; set; }          // 胸导增益(mm/mv)：1.25, 2.5, 5, 10, 20, 40
        public int? autoGainFlag { get; set; }          // 自动增益标志：0(否), 1(是)
        public string layout { get; set; }             // 布局：
                                                       //      6导：3x2, 3x2+1, 6x1
                                                       //     12导：3x4, 3x4+1, 3x4+3, 6x2, 6x2+1, 12x1
                                                       //     15导：5x3, 5x3+1
                                                       //     18导：6x3, 6x3+1, 9x2, 9x2+1
        public double? templateSpeed { get; set; }       // 平均模板纸速(mm/s)：50, 100, 200
        public double? templateGain { get; set; }        // 平均模板增益(mm/mv)：20, 40, 80
        public string templateIndex { get; set; }      // 平均模板显示导联索引：0~12
        public string templatePos { get; set; }        // 平均模板P1、Q、S、T2位置
        public int? verify { get; set; }                 // 诊断核实，0：未诊断或无效诊断，1：已核实诊断
    }

    //用户表
    public partial class user
    {
        [Key]
        public int id { get; set; }
        [Column("user_name")]
        public string userName { get; set; }
        [Column("real_name")]
        public string realname { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        [Column("emp_no")]
        public string empNo { get; set; } //工号
        [Column("phone")]
        public string phone { get; set; }
        [Column("role_id")]
        public int roleId { get; set; }
        [NotMapped, JsonIgnore]
        public string roleName => SysConstants.RoleTypes[roleId];
        [Column("dept_name")]
        public string deptName { get; set; }
        public string sign { get; set; } //电子签名文件路径
        public string remark { get; set; }
        [Column("del_flag")]
        public int? delFlag { get; set; }
        [Column("rem_flag")]
        public int? remFlag { get; set; }
        [Column("login_time")]
        public Nullable<System.DateTime> loginTime { get; set; }
        [Column("create_time")]
        public Nullable<System.DateTime> createTime { get; set; }
        [Column("create_by")]
        public string createBy { get; set; }
        [Column("update_time")]
        public Nullable<System.DateTime> updateTime { get; set; }
        [Column("update_by")]
        public string updateBy { get; set; }
        [Column("web_source_id")]
        public string webSourceId { get; set; }
    }

    //公共参数设置表
    [Table("setting")]
    public partial class sysSetting
    {
        [Key]
        public int id { get; set; }
        [Column("base_info")]
        public string baseInfo { get; set; }
        [Column("ecg_info")]
        public string ecgInfo { get; set; }
        [Column("report_info")]
        public string reportInfo { get; set; }
        [Column("operate_info")]
        public string operateInfo { get; set; }

    }

    //操作日志表
    [Table("operate_log")]
    public partial class OperateLog
    {
        public long id { get; set; }
        public string title { get; set; }
        public string addr { get; set; }
        public string url { get; set; }
        public string method { get; set; }
        public string param { get; set; }
        public long duration { get; set; } //时长(单位:ms)
        [Column("operate_user")]
        public string operateUser { get; set; }
        [Column("operate_time")]
        public Nullable<System.DateTime> operateTime { get; set; }
    }

    //报告结论模版
    [Table("report_template")]
    public partial class reportTemplate
    {
        [Key]
        public int id { get; set; }
        [Column("language_id")]
        public int languageId { get; set; }
        [Column("system_type")]
        public string systemType { get; set; }
        [Column("template_name")]
        public string templateName { get; set; }
        [Column("content")]
        public string content { get; set; }
        [Column("original_expression")]
        public string originalExpression { get; set; }
        [Column("is_default")]
        public int? isDefault { get; set; }

        [Column("create_time")]
        public Nullable<System.DateTime> createTime { get; set; }
        [Column("create_by")]
        public string createBy { get; set; }
        [Column("update_time")]
        public Nullable<System.DateTime> updateTime { get; set; }
        [Column("update_by")]
        public string updateBy { get; set; }
    }


    //基础数据字典表
    public class basedict
    {
        [Key]
        public int id { get; set; }
        public string category { get; set; }
        public string label { get; set; }
        public string value { get; set; }
        public int sort { get; set; }
    }

    public class advancefeature
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public int? isEnable { get; set; }
    }

    //病历字典表(未与数据库表映射)
    public class caseEntry
    {
        public int id { get; set; }
        public int pid { get; set; }
        public int number { get; set; }
        public string explain { get; set; }
        public string pinyin { get; set; }
    }
    #endregion

}