using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml.Linq;

namespace ChatP2P.Model
{
    // Lớp singleton quản lý tất cả yêu cầu mạng.
    // Mọi tin nhắn gửi/nhận qua mạng đều đi qua lớp này, mỗi server và client chạy trên luồng riêng.
    public sealed class NetworkManager
    {
        private static readonly Lazy<NetworkManager> _networkManager = new Lazy<NetworkManager>(() => new NetworkManager());

        private NotificationManager? notificationManager = null;

        // Lấy hoặc gán đối tượng NotificationManager (được khởi tạo thông qua ConversationManager)
        public NotificationManager NotificationManager { get { return notificationManager; } set { notificationManager = value; } }

        private Dictionary<string, TcpClient> connections;
        private readonly object _lock = new();
        private Protocol protocol = new();
        private UserModel? host;
        private TcpListener? server = null;

        private CancellationTokenSource cts = new CancellationTokenSource();

        public event EventHandler listenerSuccessEvent;
        public event EventHandler listenerFailedEvent;

        // Khởi tạo đối tượng NetworkManager toàn cục
        private NetworkManager()
        {
            connections = new Dictionary<string, TcpClient>();
        }

        // Trả về thể hiện duy nhất của NetworkManager
        public static NetworkManager Instance
        {
            get
            {
                return _networkManager.Value;
            }
        }

        // Lấy thông tin người dùng hiện tại
        public UserModel Host { get { return host; } }

        // Quản lý kết nối client (xử lý tin nhắn gửi/nhận)
        private async Task ManageClientConnection(TcpClient client)
        {
            string clientAddress = client.Client.RemoteEndPoint.ToString();
            string clientName = "";
            bool exit = false;

            try
            {
                while (!exit)
                {
                    Byte[] bytes = new byte[4096];
                    string? data = null;
                    NetworkStream stream = client.GetStream();
                    DataModel message;

                    int i;
                    while ((i = await stream.ReadAsync(bytes, 0, bytes.Length)) != 0)
                    {
                        try
                        {
                            message = protocol.Decode(bytes);
                        }
                        catch (JsonSerializationException)
                        {
                            exit = true;
                            notificationManager.AddNotification($"❌ {clientAddress}: Không thể giải mã tin nhắn từ {clientName}. Đang đóng kết nối.");
                            break;
                        }
                        catch (ArgumentException)
                        {
                            exit = true;
                            notificationManager.AddNotification($"❌ {clientAddress}: Phiên bản giao thức sai. Đang đóng kết nối.");
                            break;
                        }

                        // Xử lý từng loại tin nhắn khác nhau
                        if (message is ConnectionRequestModel) // yêu cầu kết nối ban đầu
                        {
                            ConversationManager.Instance.OnNewRequest(message.Sender);
                            connections[message.SenderAddr] = connections[clientAddress];
                            connections.Remove(clientAddress);
                            clientAddress = message.SenderAddr;
                            clientName = message.Sender.Name;
                        }
                        else if (message is AcceptRequestModel) // chấp nhận kết nối
                        {
                            clientName = message.Sender.Name;
                            clientAddress = message.SenderAddr;
                            if (notificationManager != null)
                                notificationManager.AddNotification($"✔️ {clientAddress}: {clientName} đã chấp nhận yêu cầu kết nối! Giờ bạn có thể trò chuyện.");
                            ConversationManager.Instance.InitializeConversation(message.Sender);
                        }
                        else if (message is RefuseRequestModel) // từ chối kết nối
                        {
                            clientAddress = message.SenderAddr;
                            if (notificationManager != null)
                                notificationManager.AddNotification($"❌ {clientAddress}: {message.Sender.Name} đã từ chối yêu cầu kết nối.");
                            exit = true;
                            break;
                        }
                        else if (message is CloseConnectionModel) // đóng kết nối
                        {
                            clientAddress = message.SenderAddr;
                            if (notificationManager != null)
                                notificationManager.AddNotification($"❌ {clientAddress}: {message.Sender.Name} đã đóng cuộc trò chuyện.");
                            exit = true;
                            break;
                        }
                        else if (message is MessageModel) // tin nhắn thông thường
                        {
                            ConversationManager.Instance.ReceiveMessage(message);
                        }
                        else if (message is BuzzModel) // tín hiệu buzz
                        {
                            ConversationManager.Instance.ReceiveBuzz(message);
                        }
                        bytes = new byte[4096];
                    }

                    if (exit)
                        break;
                }
            }
            catch (SocketException e)
            {
                if (notificationManager != null)
                {
                    notificationManager.AddNotification($"❗️ {clientAddress}: Kết nối đến {clientName} bị ngắt bất ngờ.");
                }
            }
            catch (IOException e)
            {
                if (notificationManager != null)
                {
                    notificationManager.AddNotification($"❗️ {clientAddress}: Lỗi đọc dữ liệu từ {clientName}. Kết nối sẽ đóng.");
                }
            }
            finally
            {
                client.Close();
                if (!exit && notificationManager != null)
                {
                    notificationManager.AddNotification($"❌ {clientAddress}: Kết nối đến {clientName} đã bị đóng.");
                }
                lock (_lock)
                {
                    ConversationManager.Instance.CloseConversation(clientAddress);
                    connections.Remove(clientAddress);
                }
            }
        }

        // Bắt đầu lắng nghe kết nối mới
        public async Task Listen(UserModel user)
        {
            connections.Clear();
            host = user;
            TcpClient incomingClient;

            IPAddress localAddr = IPAddress.Parse(host.Ip);
            Int32 portInt = Convert.ToInt32(host.Port);

            try
            {
                server = new TcpListener(localAddr, portInt);
                server.Start();
            }
            catch (SocketException)
            {
                listenerFailedEvent?.Invoke(this, EventArgs.Empty);
                return;
            }

            // Lặp liên tục để lắng nghe các kết nối mới
            while (!cts.Token.IsCancellationRequested)
            {
                if (server.Pending())
                {
                    incomingClient = await server.AcceptTcpClientAsync();
                    lock (_lock)
                    {
                        string clientAddress = incomingClient.Client.RemoteEndPoint.ToString();
                        connections[clientAddress] = incomingClient;
                    }
                    Task.Run(() => ManageClientConnection(incomingClient).ConfigureAwait(false));
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }

        // Kiểm tra cổng có đang được sử dụng không
        public static bool IsPortOccupied(string port)
        {
            var activeConnections = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
            foreach (var activeConnection in activeConnections)
            {
                if (activeConnection.Port == Convert.ToInt32(port))
                { return true; }    
            }
            return false;
        }

        // Lấy địa chỉ IP của máy
        public static string GetIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            string found_ip = null;
            try
            {
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        found_ip = ip.ToString();
                        break;
                    }
                }
            }
            catch (SocketException e)
            {
                found_ip = null;
            }
            return found_ip;
        }

        // Đóng server lắng nghe
        public async Task CloseServer()
        {
            var sendTasks = connections.Select(connection =>
                SendMessage(new CloseConnectionModel(host, connection.Key)));

            await Task.WhenAll(sendTasks);
            ConversationManager.Instance.OnExit();

            if (server != null)
            {
                cts.Cancel();
                server.Stop();
                server = null;
            }
        }

        // Kết nối đến IP và cổng cụ thể
        public async Task Connect(string ip, string port)
        {
            string targetIp = ip + ":" + port;
            notificationManager.AddNotification($"❗ Đang gửi yêu cầu kết nối đến {targetIp}...");
            Int32 portInt = Convert.ToInt32(port);
            TcpClient client = new TcpClient();

            try
            {
                IPAddress localAddr = IPAddress.Parse(ip);
                await client.ConnectAsync(ip, portInt);
            }
            catch (SocketException e)
            {
                notificationManager.AddNotification($"❌ Không có host nào đang lắng nghe tại {targetIp}. Kết nối thất bại.");
                return;
            }

            connections[targetIp] = client;
            await SendMessage(new ConnectionRequestModel(host, targetIp));
            _ = Task.Run(() => ManageClientConnection(client).ConfigureAwait(false));
        }

        // Gửi tin nhắn (mọi loại DataModel)
        public async Task SendMessage(DataModel dataModel)
        {
            NetworkStream stream = connections[dataModel.Receiver].GetStream();
            try
            {
                await stream.WriteAsync(protocol.Encode(dataModel));
            }
            catch (ArgumentOutOfRangeException)
            {
                if (notificationManager != null)
                {
                    notificationManager.AddNotification($"❌ Tin nhắn quá dài, không thể gửi.");
                }
            }
            catch (Exception e)
            {
                if (notificationManager != null)
                {
                    notificationManager.AddNotification($"❌ Không thể gửi tin đến {dataModel.Receiver}");
                }
            }
        }

        // Gửi tin chấp nhận kết nối
        public void AcceptRequest(UserModel user)
        {
            AcceptRequestModel msg = new AcceptRequestModel(Host, user.Address);
            SendMessage(msg);
        }

        // Gửi tin từ chối kết nối
        public void RefuseRequest(UserModel user)
        {
            RefuseRequestModel msg = new RefuseRequestModel(Host, user.Address);
            SendMessage(msg);
        }

        // Kiểm tra client có đang kết nối hay không
        public bool IsClientConnected(string addr)
        {
            return connections.ContainsKey(addr);
        }
    }
}