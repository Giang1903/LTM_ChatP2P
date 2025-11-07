using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ChatP2P.Model
{
    /// Lớp NetworkManager chịu trách nhiệm quản lý toàn bộ hoạt động mạng:
    /// - Lắng nghe (Listen)
    /// - Kết nối (Connect)
    /// - Gửi & nhận tin nhắn (SendMessage / ManageClientConnection)
    /// - Đóng kết nối
    public sealed class NetworkManager
    {
        // Singleton (chỉ có 1 thể hiện duy nhất trong toàn ứng dụng)
        private static readonly Lazy<NetworkManager> _networkManager = new Lazy<NetworkManager>(() => new NetworkManager());

        private NotificationManager? notificationManager = null;
        public NotificationManager NotificationManager { get { return notificationManager; } set { notificationManager = value; } }

        // Danh sách các kết nối hiện có: key = địa chỉ IP:port, value = đối tượng TcpClient tương ứng
        private Dictionary<string, TcpClient> connections;
        private readonly object _lock = new(); // Đảm bảo thread an toàn khi truy cập connections

        private Protocol protocol = new();     // Giao thức đóng gói & giải mã dữ liệu
        private UserModel? host;               // Lưu thông tin người dùng hiện tại (máy chủ)
        private TcpListener? server = null;    // Dùng để lắng nghe các kết nối đến
        private CancellationTokenSource cts = new CancellationTokenSource();

        // Sự kiện thông báo khi bắt đầu lắng nghe thành công hoặc thất bại
        public event EventHandler listenerSuccessEvent;
        public event EventHandler listenerFailedEvent;

        // Hàm khởi tạo private để đảm bảo chỉ có thể khởi tạo qua Instance
        private NetworkManager()
        {
            connections = new Dictionary<string, TcpClient>();
        }

        //Truy cập thể hiện duy nhất của NetworkManager 
        public static NetworkManager Instance => _networkManager.Value;

        //Trả về thông tin người dùng đang host 
        public UserModel Host => host;
        //Quản lý luồng nhận dữ liệu từ client — đọc và xử lý từng loại message khác nhau.
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
                    NetworkStream stream = client.GetStream();
                    DataModel message;

                    int i;
                    while ((i = await stream.ReadAsync(bytes, 0, bytes.Length)) != 0)
                    {
                        try
                        {
                            // Giải mã thông điệp nhận được từ byte[]
                            message = protocol.Decode(bytes);
                        }
                        catch (JsonSerializationException)
                        {
                            exit = true;
                            notificationManager?.AddNotification($"❌ {clientAddress}: Không giải mã được tin nhắn từ {clientName}.");
                            break;
                        }
                        catch (ArgumentException)
                        {
                            exit = true;
                            notificationManager?.AddNotification($"❌ {clientAddress}: Gói tin sai phiên bản giao thức.");
                            break;
                        }

                        // Xử lý các loại message khác nhau:
                        if (message is ConnectionRequestModel) // Yêu cầu kết nối ban đầu
                        {
                            ConversationManager.Instance.OnNewRequest(message.Sender);
                            connections[message.SenderAddr] = connections[clientAddress];
                            connections.Remove(clientAddress);
                            clientAddress = message.SenderAddr;
                            clientName = message.Sender.Name;
                        }
                        else if (message is AcceptRequestModel) // Chấp nhận kết nối
                        {
                            clientName = message.Sender.Name;
                            clientAddress = message.SenderAddr;
                            notificationManager?.AddNotification($"✔️ {clientAddress}: {clientName} đã chấp nhận kết nối!");
                            ConversationManager.Instance.InitializeConversation(message.Sender);
                        }
                        else if (message is RefuseRequestModel) // Từ chối kết nối
                        {
                            clientAddress = message.SenderAddr;
                            notificationManager?.AddNotification($"❌ {clientAddress}: {message.Sender.Name} đã từ chối kết nối.");
                            exit = true;
                            break;
                        }
                        else if (message is CloseConnectionModel) // Đóng kết nối
                        {
                            clientAddress = message.SenderAddr;
                            notificationManager?.AddNotification($"❌ {clientAddress}: {message.Sender.Name} đã đóng trò chuyện.");
                            exit = true;
                            break;
                        }
                        else if (message is MessageModel) // Tin nhắn văn bản
                        {
                            ConversationManager.Instance.ReceiveMessage(message);
                        }
                        else if (message is BuzzModel) // Tín hiệu “Buzz”
                        {
                            ConversationManager.Instance.ReceiveBuzz(message);
                        }

                        bytes = new byte[4096]; // reset bộ đệm
                    }

                    if (exit)
                        break;
                }
            }
            catch (SocketException)
            {
                notificationManager?.AddNotification($"❗️ {clientAddress}: Kết nối đến {clientName} bị gián đoạn.");
            }
            catch (IOException)
            {
                notificationManager?.AddNotification($"❗️ {clientAddress}: Không thể đọc dữ liệu từ {clientName}, kết nối sẽ bị đóng.");
            }
            finally
            {
                client.Close();
                if (!exit)
                    notificationManager?.AddNotification($"❌ {clientAddress}: Kết nối đến {clientName} đã đóng.");

                lock (_lock)
                {
                    ConversationManager.Instance.CloseConversation(clientAddress);
                    connections.Remove(clientAddress);
                }
            }
        }
        // Bắt đầu lắng nghe kết nối đến (từ các peer khác).
        public async Task Listen(UserModel user)
        {
            connections.Clear();
            host = user;

            IPAddress localAddr = IPAddress.Parse(host.Ip);
            Int32 portInt = Convert.ToInt32(host.Port);

            try
            {
                server = new TcpListener(localAddr, portInt);
                server.Start();
                listenerSuccessEvent?.Invoke(this, EventArgs.Empty);
            }
            catch (SocketException)
            {
                listenerFailedEvent?.Invoke(this, EventArgs.Empty);
                return;
            }

            // Vòng lặp chính lắng nghe client mới
            while (!cts.Token.IsCancellationRequested)
            {
                if (server.Pending())
                {
                    TcpClient incomingClient = await server.AcceptTcpClientAsync();

                    lock (_lock)
                    {
                        string clientAddress = incomingClient.Client.RemoteEndPoint.ToString();
                        connections[clientAddress] = incomingClient;
                    }

                    // Xử lý client mới trên thread riêng
                    Task.Run(() => ManageClientConnection(incomingClient).ConfigureAwait(false));
                }
                else
                {
                    await Task.Delay(100); // nghỉ nhẹ để tránh chiếm CPU
                }
            }
        }
        // Kiểm tra xem port có đang bị chiếm không.
        public static bool IsPortOccupied(string port)
        {
            var activeConnections = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
            return activeConnections.Any(c => c.Port == Convert.ToInt32(port));
        }

        // Lấy địa chỉ IP nội bộ của máy hiện tại.
        public static string GetIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }
            return null;
        }
        // Đóng server và thông báo đến tất cả các client.
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
        // Gửi yêu cầu kết nối đến một IP cụ thể.
        public async Task Connect(string ip, string port)
        {
            string targetIp = ip + ":" + port;
            notificationManager?.AddNotification($"📡 Đang gửi yêu cầu kết nối đến {targetIp}...");
            Int32 portInt = Convert.ToInt32(port);
            TcpClient client = new TcpClient();

            try
            {
                await client.ConnectAsync(ip, portInt);
            }
            catch (SocketException)
            {
                notificationManager?.AddNotification($"❌ Không có máy chủ nào lắng nghe tại {targetIp}.");
                return;
            }

            connections[targetIp] = client;
            await SendMessage(new ConnectionRequestModel(host, targetIp));
            _ = Task.Run(() => ManageClientConnection(client).ConfigureAwait(false));
        }

        //Gửi một gói tin (DataModel) đến người nhận.
        public async Task SendMessage(DataModel dataModel)
        {
            NetworkStream stream = connections[dataModel.Receiver].GetStream();
            try
            {
                await stream.WriteAsync(protocol.Encode(dataModel));
            }
            catch (ArgumentOutOfRangeException)
            {
                notificationManager?.AddNotification($"❌ Tin nhắn quá dài, không thể gửi.");
            }
            catch (Exception)
            {
                notificationManager?.AddNotification($"❌ Gửi tin nhắn đến {dataModel.Receiver} thất bại.");
            }
        }

        //Gửi thông báo chấp nhận kết nối 
        public void AcceptRequest(UserModel user)
        {
            AcceptRequestModel msg = new AcceptRequestModel(Host, user.Address);
            SendMessage(msg);
        }

        // Gửi thông báo từ chối kết nối 
        public void RefuseRequest(UserModel user)
        {
            RefuseRequestModel msg = new RefuseRequestModel(Host, user.Address);
            SendMessage(msg);
        }
        // Kiểm tra client có đang kết nối không.
        public bool IsClientConnected(string addr)
        {
            return connections.ContainsKey(addr);
        }
    }
}
