using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ChatP2P.Model
{
    public sealed class NetworkManager
    {
        // Singleton instance
        private static readonly Lazy<NetworkManager> _networkManager = new Lazy<NetworkManager>(() => new NetworkManager());

        // Danh sách kết nối
        private Dictionary<string, TcpClient> connections;
        private TcpListener? server = null;
        private CancellationTokenSource cts = new CancellationTokenSource();

        // Constructor riêng (private)
        private NetworkManager()
        {
            connections = new Dictionary<string, TcpClient>();
        }

        // Thuộc tính Singleton
        public static NetworkManager Instance
        {
            get { return _networkManager.Value; }
        }

        // Hàm Listen — bắt đầu lắng nghe kết nối đến
        public async Task Listen(string ip, string port)
        {
            connections.Clear();
            IPAddress localAddr = IPAddress.Parse(ip);
            Int32 portInt = Convert.ToInt32(port);

            try
            {
                server = new TcpListener(localAddr, portInt);
                server.Start();
                Console.WriteLine($" Server started at {ip}:{port}");
            }
            catch (SocketException)
            {
                Console.WriteLine(" Port is already in use or invalid IP.");
                return;
            }

            while (!cts.Token.IsCancellationRequested)
            {
                if (server.Pending())
                {
                    // Khi có client mới
                    TcpClient incomingClient = await server.AcceptTcpClientAsync();
                    string clientAddr = incomingClient.Client.RemoteEndPoint.ToString();
                    connections[clientAddr] = incomingClient;
                    Console.WriteLine($" Client connected: {clientAddr}");
                }
                else
                {
                    // Không có client mới thì đợi chút rồi kiểm tra lại
                    await Task.Delay(100);
                }
            }
        }

        // Hàm dừng server
        public void CloseServer()
        {
            cts.Cancel();
            server?.Stop();
            server = null;
            Console.WriteLine(" Server stopped.");
        }
    }
}
