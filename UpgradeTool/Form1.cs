using Microsoft.Data.Sqlite;
using SQLitePCL;
using System;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Utilities;

namespace UpgradeTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CommonValues.LogPath = PubFuns.GetBaseDirectory() + "\\log";
            
            try
            {
                Batteries_V2.Init();

            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "程序升级");
            }
        }

        private void btnUpgradeDB_Click(object sender, EventArgs e)
        {
            string connString = "";
            string dbFileName = PubFuns.GetAbsolutePath("ecg.db");
            connString = new SqliteConnectionStringBuilder()
            {
                DataSource = dbFileName,
                Mode = SqliteOpenMode.ReadWriteCreate,
                ForeignKeys = true
            }.ToString();

            using (SqliteConnection dbConn = new SqliteConnection(connString))
            {
                dbConn.Open();

                try
                {
                    using (var cmd = dbConn.CreateCommand())
                    {
                        cmd.CommandText = "PRAGMA key = 'zq2877'";
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
                    Logger.CreateLog("数据库连接成功", "程序升级");

                    // 检查字段是否存在
                    using (var cmd = new SqliteCommand("PRAGMA table_info(operate_log);", dbConn))
                    {
                        //日志表
                        using (var reader = cmd.ExecuteReader())
                        {
                            bool columnExists = false;
                            while (reader.Read())
                            {
                                string columnName = reader["name"].ToString();
                                if (columnName.Equals("duration", StringComparison.OrdinalIgnoreCase))
                                {
                                    columnExists = true;
                                    break;
                                }
                            }

                            if (!columnExists)
                            {
                                using (var addCmd = new SqliteCommand("ALTER TABLE operate_log ADD COLUMN duration integer;", dbConn))
                                {
                                    addCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        //病例表
                        cmd.CommandText = "PRAGMA table_info(ecg);";
                        using (var reader = cmd.ExecuteReader())
                        {
                            bool columnExists = false;
                            while (reader.Read())
                            {
                                string columnName = reader["name"].ToString();
                                if (columnName.Equals("file_directory", StringComparison.OrdinalIgnoreCase))
                                {
                                    columnExists = true;
                                    break;
                                }
                            }

                            if (!columnExists)
                            {
                                using (var addCmd = new SqliteCommand("ALTER TABLE [ecg] ADD COLUMN file_directory varchar(255);", dbConn))
                                {
                                    addCmd.ExecuteNonQuery();
                                }

                                using (var updateCmd = new SqliteCommand(@"UPDATE ecg 
                                    SET file_directory = TRIM(SUBSTR(file_path, 1, INSTR(file_path, '\holter\') - 1)) 
                                    WHERE file_directory is null and INSTR(file_path, '\holter\') > 0;", dbConn))
                                {
                                    updateCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        //科室表
                        cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='apply_dept'";
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        if (count == 0)
                        {
                            string createSql = @"CREATE TABLE apply_dept (
                                id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                                dept_code varchar(50),
                                dept_name varchar(255),
                                hospital varchar(255),
                                [create_time] datetime, 
	                            [create_by] varchar(50), 
	                            [update_time] datetime, 
	                            [update_by] varchar(50)
                            )";
                            using (SqliteCommand createCmd = new SqliteCommand(createSql, dbConn))
                            {
                                createCmd.ExecuteNonQuery();
                            }
                        }
                    }
                    MessageBox.Show("数据库升级成功");
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex, "程序升级");
                }
                finally
                {
                    dbConn.Close();
                }
            }
        }

    }
}
