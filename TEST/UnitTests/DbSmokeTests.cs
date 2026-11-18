using System;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EcgBpAnalysis.UnitTests
{
    /// <summary>
    /// SQLite native provider 可用性冒烟测试。
    /// 用于确认测试环境能否加载 e_sqlcipher.dll，从而支撑后续 DB 集成测试。
    /// </summary>
    [TestClass]
    public class DbSmokeTests
    {
        [TestMethod]
        public void SqliteNativeProvider_CanOpenInMemoryConnection()
        {
            try
            {
                SQLitePCL.Batteries_V2.Init();
            }
            catch (Exception ex)
            {
                Assert.Fail($"SQLitePCL 初始化失败（检查 e_sqlcipher.dll 是否在测试输出目录）: {ex.Message}");
            }

            using (var conn = new SqliteConnection("Data Source=:memory:"))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT 1+1;";
                    object result = cmd.ExecuteScalar();
                    Assert.AreEqual(2L, Convert.ToInt64(result), "内存数据库应可用");
                }
            }
        }
    }
}
