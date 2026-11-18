using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace Zoncare.Common
{
    /// <summary>
    /// 命名管道服务端
    /// </summary>
    public class PipeServer
    {
        private NamedPipeServerStream _pipeServer;
        private readonly string _pipeName;
        private bool _isRunning;

        public Action OnClientConnected;
        public Action<string> OnMessageReceived;

        public PipeServer(string pipeName)
        {
            _pipeName = pipeName;
        }

        public void Start()
        {
            _isRunning = true;
            ThreadPool.QueueUserWorkItem(ServerThread);
        }

        public void Stop()
        {
            _isRunning = false;
            _pipeServer?.Dispose();
        }

        private void ServerThread(object state)
        {
            while (_isRunning)
            {
                try
                {
                    // 创建命名管道服务器
                    _pipeServer = new NamedPipeServerStream(
                        _pipeName,
                        PipeDirection.InOut,
                        1,
                        PipeTransmissionMode.Message,
                        PipeOptions.Asynchronous);

                    Console.WriteLine("等待客户端连接...");
                    _pipeServer.WaitForConnection();
                    OnClientConnected?.Invoke();
                    Console.WriteLine("客户端已连接");

                    // 启动异步读取
                    byte[] buffer = new byte[1024];
                    _pipeServer.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);

                    // 保持连接开放，直到客户端断开
                    while (_pipeServer.IsConnected && _isRunning)
                    {
                        // 可以在这里处理其他任务或发送消息
                        Thread.Sleep(1);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"服务器错误: {ex.Message}");
                }
                finally
                {
                    _pipeServer?.Dispose();
                }
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                byte[] buffer = (byte[])ar.AsyncState;
                int bytesRead = _pipeServer.EndRead(ar);

                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"收到客户端消息: {message}");

                    // 处理消息并回复
                    string response = ProcessMessage(message);
                    //if (!string.IsNullOrEmpty(response))
                    //{
                    //    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    //    _pipeServer.Write(responseBytes, 0, responseBytes.Length);
                    //    _pipeServer.Flush();
                    //}

                    // 保持长连接, 继续读取下一条消息
                    if (_pipeServer.IsConnected)
                    {
                        buffer = new byte[1024];
                        _pipeServer.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取错误: {ex.Message}");
                _pipeServer?.Dispose();
            }
        }

        private string ProcessMessage(string message)
        {
            OnMessageReceived?.Invoke(message);

            return $"服务端已收到消息: {message}";
        }

        public void SendMessage(string message)
        {
            if (_pipeServer != null && _pipeServer.IsConnected)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                _pipeServer.Write(buffer, 0, buffer.Length);
                _pipeServer.Flush();
            }
        }
    }

    /// <summary>
    /// 命名管道客户端
    /// </summary>
    public class PipeClient
    {
        private NamedPipeClientStream _pipeClient;
        private readonly string _pipeName;
        private bool _isRunning;

        public bool IsConnected
        {
            get
            {
                if (_pipeClient != null)
                    return _pipeClient.IsConnected;
                else
                    return false;
            }
        }

        public Action<string> OnMessageReceived;

        public PipeClient(string pipeName)
        {
            _pipeName = pipeName;
        }

        public void Connect()
        {
            try
            {
                _pipeClient = new NamedPipeClientStream(
                    ".",
                    _pipeName,
                    PipeDirection.InOut,
                    PipeOptions.Asynchronous);

                Console.WriteLine("正在连接到服务器...");
                _pipeClient.Connect();
                Console.WriteLine("已连接到服务器");

                _isRunning = true;
                ThreadPool.QueueUserWorkItem(ClientThread);

                // 启动异步读取
                byte[] buffer = new byte[1024];
                _pipeClient.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"连接错误: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            _isRunning = false;
            _pipeClient?.Dispose();
        }

        private void ClientThread(object state)
        {
            while (_isRunning && _pipeClient != null && _pipeClient.IsConnected)
            {
                // 可以在这里发送消息或处理其他任务
                Thread.Sleep(100);
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                byte[] buffer = (byte[])ar.AsyncState;
                int bytesRead = _pipeClient.EndRead(ar);

                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"收到服务端消息: {message}");
                    OnMessageReceived?.Invoke(message);

                    // 保持长连接, 继续读取下一条消息
                    if (_pipeClient.IsConnected)
                    {
                        buffer = new byte[1024];
                        _pipeClient.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取错误: {ex.Message}");
                _pipeClient?.Dispose();
            }
        }

        public void SendMessage(string message)
        {
            if (_pipeClient != null && _pipeClient.IsConnected)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                _pipeClient.Write(buffer, 0, buffer.Length);
                _pipeClient.Flush();
            }
        }
    }
}
