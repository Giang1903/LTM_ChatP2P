using ChatP2P.View.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Xml.Linq;

namespace ChatP2P.Model
{
    // ConversationManager quản lý tất cả các cuộc hội thoại trong ứng dụng.
    // Sử dụng Singleton pattern để đảm bảo chỉ có một instance duy nhất.
    public sealed class ConversationManager : INotifyPropertyChanged
    {
        private static readonly Lazy<ConversationManager> _conversationManager = new Lazy<ConversationManager>(() => new ConversationManager());
        private Dictionary<string, ConversationModel?> conversations; // Các cuộc trò chuyện đang hoạt động
        private Dictionary<string, ConversationModel> inactiveConversations; // Các cuộc trò chuyện không hoạt động
        private ConversationSerializer serializer;


        public event EventHandler activeConversationSetEvent;
        public event EventHandler inactiveConversationSetEvent;
        public event EventHandler buzzEvent;

        private NotificationManager? notificationManager = null;
        public NotificationManager NotificationManager { get { return notificationManager; } set { notificationManager = value; } }

        private string currentConversation = null;
        public string CurrentConversation { get { return currentConversation; } set { currentConversation = value; OnPropertyChanged("CurrentConversation"); } }
        public bool CurrentConversationIsActive { get { if (currentConversation == null) return false; return conversations.ContainsKey(currentConversation); } }

        // Lấy danh sách các cuộc trò chuyện đang hoạt động
        public List<ConversationModel> GetActiveConversations()
        {
            return conversations.Values.ToList();
        }

        // Lấy danh sách các cuộc trò chuyện không hoạt động, sắp xếp theo thời gian hoạt động gần nhất
        public List<ConversationModel> GetInactiveConversations()
        {
            return inactiveConversations.Values.ToList().OrderByDescending(item => item.LastActivity).ToList();
        }


        // Chọn cuộc trò chuyện hiện tại
        public void AssignCurrentConversation(string? endpoint)
        {
            if (endpoint == null || CurrentConversation == endpoint) return;

            if (conversations.Keys.Contains(endpoint))
            {
                conversations[endpoint].UnreadMessages = false;
                conversations[endpoint].UnreadBuzz = false;
                CurrentConversation = endpoint;
                activeConversationSetEvent?.Invoke(this, new EventArgs());
                conversationsUpdatedEvent?.Invoke(this, EventArgs.Empty);
            }
            else if (inactiveConversations.Keys.Contains(endpoint))
            {
                CurrentConversation = endpoint;
                inactiveConversationSetEvent?.Invoke(this, new EventArgs());
            }
            else
            {
                SendNotification("❗ Đã xảy ra sự cố khi chuyển đổi cuộc trò chuyện...");
            }
        }

        private List<UserModel> pendingRequests = new List<UserModel>();
        public event EventHandler newRequestEvent;
        public event EventHandler conversationsUpdatedEvent;
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // Constructor private cho singleton, load tất cả cuộc trò chuyện không hoạt động
        private ConversationManager()
        { 
            serializer = new ConversationSerializer();
            conversations = new Dictionary<string, ConversationModel>();
            inactiveConversations = new Dictionary<string, ConversationModel>();
            List<ConversationModel> fetchedConversations = serializer.LoadAll();

            foreach (ConversationModel conversation in fetchedConversations)
            {
                inactiveConversations[conversation.User.Address] = conversation;
            }
        }

        public static ConversationManager Instance 
        {
            get
            {
                return _conversationManager.Value;
            }
        }


        // Khởi tạo hoặc kích hoạt cuộc trò chuyện
        public void InitializeConversation(UserModel user)
        {
            if (inactiveConversations.ContainsKey(user.Address))
            {
                conversations[user.Address] = inactiveConversations[user.Address];
                conversations[user.Address].UnreadMessages = false;
                conversations[user.Address].UnreadBuzz = false;
                inactiveConversations.Remove(user.Address);
            }
            else
            {
                conversations[user.Address] = new ConversationModel(user);
            }

            CurrentConversation = user.Address;
            activeConversationSetEvent?.Invoke(this, EventArgs.Empty);
            conversationsUpdatedEvent?.Invoke(this, EventArgs.Empty);
        }

        // Gửi tin nhắn đến người nhận
        public async Task SendMessage(DataModel message)
        {
            conversations[message.Receiver].ReceiveMessage(message);
            await NetworkManager.Instance.SendMessage(message);
        }

        // Gửi buzz
        public async Task SendBuzz(BuzzModel buzz)
        {
            await NetworkManager.Instance.SendMessage(buzz);
        }

        // Nhận tin nhắn và cập nhật GUI
        public void ReceiveMessage(DataModel message)
        {
            conversations[message.SenderAddr].ReceiveMessage(message);
            conversationsUpdatedEvent?.Invoke(this, EventArgs.Empty);
        }

        // Nhận buzz và cập nhật GUI
        public void ReceiveBuzz(DataModel buzz)
        {
            if (buzz.SenderAddr != currentConversation)
            {
                conversations[buzz.SenderAddr].UnreadBuzz = true;
                conversations[buzz.SenderAddr].UnreadMessages = false;
            }

            conversationsUpdatedEvent?.Invoke(this, EventArgs.Empty);
            buzzEvent?.Invoke(this, EventArgs.Empty);
        }

        // Lấy cuộc trò chuyện hiện tại
        public ConversationModel GetConversation()
        {
            if (currentConversation == null)
            {
                return null;
            }
            else if (conversations.ContainsKey(currentConversation))
            {
                return conversations[currentConversation];
            }
            else
            {
                return inactiveConversations[currentConversation];
            }

        }

        // Thông báo yêu cầu kết nối mới
        public void OnNewRequest(UserModel user)
        {
            pendingRequests.Add(user);
            newRequestEvent?.Invoke(this, EventArgs.Empty);
        }

        public UserModel? GetPendingRequest()
        {
            return pendingRequests.First();
        }

        // Chấp nhận yêu cầu kết nối
        public void AcceptRequest()
        {
            UserModel user = pendingRequests.First();
            pendingRequests.RemoveAt(0);
            InitializeConversation(user);
            NetworkManager.Instance.AcceptRequest(user);
            if (pendingRequests.Count > 0)
            {
                newRequestEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        // Từ chối yêu cầu kết nối
        public void DeclineRequest()
        {
            UserModel user = pendingRequests.First();
            pendingRequests.RemoveAt(0);
            NetworkManager.Instance.RefuseRequest(user);
            if (!(pendingRequests.Count == 0))
            {
                newRequestEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        // Đóng cuộc trò chuyện, chuyển sang không hoạt động và lưu lịch sử
        public void CloseConversation(string clientAddress)
        {
            ConversationModel conversation = conversations[clientAddress];

            serializer.Save(conversation);
            conversations.Remove(clientAddress);
            inactiveConversations[clientAddress] = conversation;
            if (conversations.Count == 0)
            {
                CurrentConversation = null;
            }

            conversationsUpdatedEvent?.Invoke(this, EventArgs.Empty);

        }


        // Lưu tất cả cuộc trò chuyện đang hoạt động khi thoát
        public void OnExit()
        {
            foreach (var conv in conversations)
            {
                if (conv.Value != null)
                    serializer.Save(conv.Value);
            }
        }

        // Gửi thông báo lên GUI
        public void SendNotification(string message)
        {
            if (notificationManager != null)
                notificationManager.AddNotification(message);
        }
    }
}