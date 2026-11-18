using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SQLitePCL;
using System;
using System.Data;
using System.Linq;
using Zoncare.Utilities;

namespace Zoncare.DAL
{
    //此属性用于指定配置类
    //[DbConfigurationType(typeof(MyConfiguration))]
    public class ZoncareModelsPlus : DbContext
    {
        private const string ACCESS_KEY = "zq2877";
        private static string dbFileName;
        private static SqliteConnection dbConn;
        private static ZoncareEntities dbEntities;
        private static object _lockObject = new object();

        public static SqliteConnection DbConnection { get => dbConn; set => dbConn = value; }
        public static bool IsReadyDbConn { get; set; }
        public static bool IsReadyDbContext { get; set; }


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
                            string connString = new SqliteConnectionStringBuilder()
                            {
                                DataSource = dbFileName,
                                Mode = SqliteOpenMode.ReadWriteCreate,
                                ForeignKeys = true
                            }.ToString();

                            initDatabaseConnection(connString);

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

        public ZoncareModelsPlus()
        {
            dbEntities = this;
           
            //Database.Log = (sql) => { Console.WriteLine(sql); };
        }

        //初始化数据库文件及运行时环境
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
            dbConn = new SqliteConnection(connString);
            dbConn.Open();

            using (var cmd = dbConn.CreateCommand())
            {
                cmd.CommandText = "PRAGMA key = '" + ACCESS_KEY + "'";
                int execResult1 = cmd.ExecuteNonQuery();

                cmd.CommandText = "PRAGMA cipher_compatibility = 3;";
                int execResult2 = cmd.ExecuteNonQuery();

                // 增大缓存页数（默认 2000，根据内存调整）-kB 单位
                cmd.CommandText = "PRAGMA cache_size = -10000;";
                int execResult4 = cmd.ExecuteNonQuery();

                //// 禁用外键约束检查（如果不需要）
                //cmd.CommandText = "PRAGMA foreign_keys = OFF;";
                //int execResult5 = cmd.ExecuteNonQuery();
            }

            try
            {
                string query = "SELECT * FROM [user] limit 1";
                using (SqliteCommand cmd = new SqliteCommand(query, dbConn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine(reader["id"]);
                        }
                    }
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
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

    }

}