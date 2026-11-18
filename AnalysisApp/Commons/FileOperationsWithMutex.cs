using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Zoncare.Common;

namespace AnalysisApp.Commons
{
    public static class FileOperationsWithMutex
    {
        /// <summary>
        /// 使用命名Mutex安全地写入文件
        /// content     写入的内容
        /// MutexName   命名Mutex的名字，必须全局唯一且所有进程使用相同名字
        /// FilePath    要操作的文件路径
        /// </summary>
        public static void SafeWriteToFile(string content, string MutexName, string FilePath)
        {
            // 尝试获取命名Mutex的所有权，如果已被占用则等待
            using (var mutex = new Mutex(false, MutexName))
            {
                bool hasHandle = false;
                try
                {
                    // 等待获取互斥锁（最多等待10秒，防止死锁）
                    hasHandle = mutex.WaitOne(TimeSpan.FromSeconds(10));
                    if (!hasHandle)
                    {
                        //Console.WriteLine("获取互斥锁超时，无法执行写入操作。");
                        return;
                    }

                    // 成功获取锁，安全地写入文件
                    //Console.WriteLine($"[进程 {Process.GetCurrentProcess().Id}] 获取到锁，开始写入...");
                    File.WriteAllText(FilePath, content);
                    //Console.WriteLine($"[进程 {Process.GetCurrentProcess().Id}] 写入完成。");
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex, "文件读写");
                    //Console.WriteLine($"写入文件时发生异常: {ex.Message}");
                }
                finally
                {
                    // 释放互斥锁
                    if (hasHandle)
                    {
                        mutex.ReleaseMutex();
                    }
                }
            }
        }

        /// <summary>
        /// 使用命名Mutex安全地读取文件
        /// </summary>
        public static string SafeReadFromFile(string MutexName, string FilePath)
        {
            using (var mutex = new Mutex(false, MutexName))
            {
                bool hasHandle = false;
                try
                {
                    hasHandle = mutex.WaitOne(TimeSpan.FromSeconds(10));
                    if (!hasHandle)
                    {
                        //Console.WriteLine("获取互斥锁超时，无法执行读取操作。");
                        return null;
                    }

                    //Console.WriteLine($"[进程 {Process.GetCurrentProcess().Id}] 获取到锁，开始读取...");
                    string content = File.ReadAllText(FilePath);
                    //Console.WriteLine($"[进程 {Process.GetCurrentProcess().Id}] 读取完成。");
                    return content;
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex, "文件读写");

                    //Console.WriteLine($"读取文件时发生异常: {ex.Message}");
                    return null;
                }
                finally
                {
                    if (hasHandle)
                    {
                        mutex.ReleaseMutex();
                        //Console.WriteLine($"[进程 {Process.GetCurrentProcess().Id}] 释放了锁。");
                    }
                }
            }
        }
    }
}
