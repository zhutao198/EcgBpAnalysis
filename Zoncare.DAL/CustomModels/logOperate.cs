using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Zoncare.Common;

namespace Zoncare.DAL
{
    public partial class OperateLog
    {
        private static object _lockObj = new object();
        private static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings();

        /// <summary>
        /// 新增操作日志信息
        /// </summary>
        /// <param name="title"></param>
        /// <param name="parameterInfo"></param>
        /// <param name="duration"></param>
        /// <param name="userName"></param>
        public static void logParam(string title, object parameterInfo, long duration, string userName)
        {
            if (!CommonValues.IsSaveOperateLog) return;

            Task.Run(() =>
            {
                OperateLog opLog = new OperateLog();
                try
                {
                    opLog.title = title;
                    opLog.addr = "127.0.0.1";

                    IPAddress localIP = getLocalIP();
                    if (null != localIP)
                        opLog.addr = localIP.ToString();

                    if (parameterInfo is string)
                        opLog.param = parameterInfo.ToString();
                    else
                        opLog.param = JsonConvert.SerializeObject(parameterInfo, _jsonSettings);
                    opLog.duration = duration;
                    opLog.operateUser = userName;
                    opLog.operateTime = DateTime.Now;

                    log_add(opLog);
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex, "操作日志", true);
                }
            });
        }

        /// <summary>
        /// 新增操作日志信息
        /// </summary>
        /// <param name="title">主题</param>
        /// <param name="parameters">参数信息</param>
        /// <param name="exception">异常信息</param>
        /// <param name="duration">操作时长</param>
        /// <param name="userName">操作用户名</param>
        /// <param name="result">请求结果</param>
        /// <param name="reqUri">请求的uri</param>
        /// <param name="method">HTTP请求方法</param>
        public static void logParam(string title, string parameters, string exception, long duration, string userName, string result, string reqUri = "", string method = "post")
        {
            if (!CommonValues.IsSaveOperateLog) return;

            Task.Run(() =>
            {
                OperateLog opLog = new OperateLog();
                try
                {
                    opLog.title = title;
                    opLog.addr = "127.0.0.1";

                    IPAddress localIP = getLocalIP();
                    if (null != localIP)
                        opLog.addr = localIP.ToString();

                    opLog.url = reqUri;
                    opLog.method = method;
                    opLog.param = parameters;
                    opLog.duration = duration;
                    opLog.operateUser = userName;
                    opLog.operateTime = DateTime.Now;

                    log_add(opLog);
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex, "操作日志");
                }
            });
        }

        /// <summary>
        /// 保存操作日志
        /// </summary>
        /// <param name="info">新增日志信息</param>
        /// <returns></returns>
        public static bool log_add(OperateLog info)
        {
            bool result = false;
            try
            {
                //Write log information into DB
                lock (_lockObj)
                {
                    using (ZoncareEntities dbEntities = ZoncareEntities.CreateIndependent())
                    {
                        dbEntities.Log.Add(info);
                        dbEntities.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "操作日志");
            }
            return result;
        }

        private static IPAddress getLocalIP()
        {
            IPAddress localIp = null;
            try
            {
                IPAddress[] ipArray;
                ipArray = Dns.GetHostAddresses(Dns.GetHostName());

                localIp = ipArray.First(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            }
            catch (Exception ex)
            {
            }

            return localIp;
        }
    }
}
