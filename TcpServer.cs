using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace KTVApp
{
    public class TcpServer
    {
        private TcpListener _listener;
        private bool _isRunning;

        // 添加事件来通知接收到的消息
        public event Action<string> MessageReceived;

        public TcpServer(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            _isRunning = true;
            _listener.Start();
            ListenForClients();
        }

        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();
        }

        private async void ListenForClients()
        {
            while (_isRunning)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    HandleClient(client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting client: {ex.Message}");
                }
            }
        }

        private async void HandleClient(TcpClient client)
        {
            Console.WriteLine("Client connected.");
            NetworkStream stream = client.GetStream();

            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received: {message}");

                    // 触发事件通知主窗体
                    MessageReceived?.Invoke(message);

                    // Echo the message back to the client
                    await stream.WriteAsync(buffer, 0, bytesRead);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine("Client disconnected.");
            }
        }
    }
}
