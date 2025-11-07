using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ChatP2P.Model
{
    /// <summary>
    /// ConversationManager quản lý tất cả các cuộc hội thoại trong ứng dụng.
    /// Sử dụng Singleton pattern để đảm bảo chỉ có một instance duy nhất.
    /// </summary>
    internal class ConversationManager
    {
        // Singleton pattern với Lazy initialization
        private static readonly Lazy<ConversationManager> _instance = 
            new Lazy<ConversationManager>(() => new ConversationManager());

        /// <summary>
        /// Instance duy nhất của ConversationManager
        /// </summary>
        public static ConversationManager Instance => _instance.Value;

        // Dictionary lưu trữ tất cả conversations: key = địa chỉ IP:Port, value = ConversationModel
        private Dictionary<string, ConversationModel> conversations;
        
        // ObservableCollection để binding với UI (active conversations)
        private ObservableCollection<ConversationModel> activeConversations;
        
        // ObservableCollection để binding với UI (inactive conversations)
        private ObservableCollection<ConversationModel> inactiveConversations;

        // Conversation hiện tại đang được chọn
        private string currentConversation = "";

        // Sự kiện khi có buzz
        public event EventHandler buzzEvent;

        // Lock object để đảm bảo thread-safe
        private readonly object _lock = new object();

        /// <summary>
        /// Constructor private để đảm bảo chỉ có thể tạo instance qua Instance property
        /// </summary>
        private ConversationManager()
        {
            conversations = new Dictionary<string, ConversationModel>();
            activeConversations = new ObservableCollection<ConversationModel>();
            inactiveConversations = new ObservableCollection<ConversationModel>();
        }

        /// <summary>
        /// Property trả về conversation hiện tại đang được chọn
        /// </summary>
        public string CurrentConversation
        {
            get { return currentConversation; }
            set
            {
                if (currentConversation != value)
                {
                    // Đánh dấu conversation cũ là đã đọc
                    if (!string.IsNullOrEmpty(currentConversation) && conversations.ContainsKey(currentConversation))
                    {
                        conversations[currentConversation].MarkAsRead();
                    }

                    currentConversation = value;

                    // Đánh dấu conversation mới là đã đọc
                    if (!string.IsNullOrEmpty(currentConversation) && conversations.ContainsKey(currentConversation))
                    {
                        conversations[currentConversation].MarkAsRead();
                    }
                }
            }
        }

        /// <summary>
        /// Lấy danh sách các conversations đang active (có kết nối TCP)
        /// </summary>
        /// <returns>ObservableCollection chứa các active conversations</returns>
        public ObservableCollection<ConversationModel> GetActiveConversations()
        {
            lock (_lock)
            {
                // Cập nhật lại danh sách active conversations dựa trên IsActive property
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var toRemove = activeConversations.Where(c => !c.IsActive).ToList();
                    foreach (var conv in toRemove)
                    {
                        activeConversations.Remove(conv);
                        if (!inactiveConversations.Contains(conv))
                        {
                            inactiveConversations.Add(conv);
                        }
                    }

                    var toAdd = conversations.Values
                        .Where(c => c.IsActive && !activeConversations.Contains(c))
                        .OrderByDescending(c => c.LastActivity)
                        .ToList();
                    
                    foreach (var conv in toAdd)
                    {
                        activeConversations.Add(conv);
                        inactiveConversations.Remove(conv);
                    }

                    // Sắp xếp lại theo LastActivity (mới nhất trước)
                    var sorted = activeConversations.OrderByDescending(c => c.LastActivity).ToList();
                    activeConversations.Clear();
                    foreach (var conv in sorted)
                    {
                        activeConversations.Add(conv);
                    }
                });

                return activeConversations;
            }
        }

        /// <summary>
        /// Lấy danh sách các conversations không active (không có kết nối TCP)
        /// </summary>
        /// <returns>ObservableCollection chứa các inactive conversations</returns>
        public ObservableCollection<ConversationModel> GetInactiveConversations()
        {
            lock (_lock)
            {
                // Cập nhật lại danh sách inactive conversations
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var toRemove = inactiveConversations.Where(c => c.IsActive).ToList();
                    foreach (var conv in toRemove)
                    {
                        inactiveConversations.Remove(conv);
                        if (!activeConversations.Contains(conv))
                        {
                            activeConversations.Add(conv);
                        }
                    }

                    var toAdd = conversations.Values
                        .Where(c => !c.IsActive && !inactiveConversations.Contains(c))
                        .OrderByDescending(c => c.LastActivity)
                        .ToList();
                    
                    foreach (var conv in toAdd)
                    {
                        inactiveConversations.Add(conv);
                        activeConversations.Remove(conv);
                    }

                    // Sắp xếp lại theo LastActivity (mới nhất trước)
                    var sorted = inactiveConversations.OrderByDescending(c => c.LastActivity).ToList();
                    inactiveConversations.Clear();
                    foreach (var conv in sorted)
                    {
                        inactiveConversations.Add(conv);
                    }
                });

                return inactiveConversations;
            }
        }

        /// <summary>
        /// Xử lý khi có yêu cầu kết nối mới
        /// </summary>
        public void OnNewRequest(UserModel sender)
        {
            lock (_lock)
            {
                // Nếu chưa có conversation với user này, tạo mới
                if (!conversations.ContainsKey(sender.Address))
                {
                    var newConversation = new ConversationModel(sender);
                    conversations[sender.Address] = newConversation;
                }
            }
        }

        /// <summary>
        /// Khởi tạo conversation khi kết nối được chấp nhận
        /// </summary>
        public void InitializeConversation(UserModel user)
        {
            lock (_lock)
            {
                ConversationModel conversation;
                
                if (conversations.ContainsKey(user.Address))
                {
                    conversation = conversations[user.Address];
                }
                else
                {
                    conversation = new ConversationModel(user);
                    conversations[user.Address] = conversation;
                }

                // Đánh dấu conversation là active
                Application.Current.Dispatcher.Invoke(() =>
                {
                    conversation.IsActive = true;
                    conversation.LastActivity = DateTime.Now;

                    // Thêm vào active conversations nếu chưa có
                    if (!activeConversations.Contains(conversation))
                    {
                        activeConversations.Add(conversation);
                        inactiveConversations.Remove(conversation);
                    }
                });
            }
        }

        /// <summary>
        /// Nhận tin nhắn mới và thêm vào conversation tương ứng
        /// </summary>
        public void ReceiveMessage(DataModel message)
        {
            lock (_lock)
            {
                string senderAddress = message.SenderAddr;

                if (!conversations.ContainsKey(senderAddress))
                {
                    // Tạo conversation mới nếu chưa có
                    var newConversation = new ConversationModel(message.Sender);
                    newConversation.IsActive = NetworkManager.Instance.IsClientConnected(senderAddress);
                    conversations[senderAddress] = newConversation;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (newConversation.IsActive)
                        {
                            activeConversations.Add(newConversation);
                        }
                        else
                        {
                            inactiveConversations.Add(newConversation);
                        }
                    });
                }

                conversations[senderAddress].ReceiveMessage(message);
            }
        }

        /// <summary>
        /// Nhận tín hiệu buzz và kích hoạt sự kiện buzz
        /// </summary>
        public void ReceiveBuzz(DataModel buzz)
        {
            lock (_lock)
            {
                string senderAddress = buzz.SenderAddr;

                if (!conversations.ContainsKey(senderAddress))
                {
                    // Tạo conversation mới nếu chưa có
                    var newConversation = new ConversationModel(buzz.Sender);
                    newConversation.IsActive = NetworkManager.Instance.IsClientConnected(senderAddress);
                    conversations[senderAddress] = newConversation;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (newConversation.IsActive)
                        {
                            activeConversations.Add(newConversation);
                        }
                        else
                        {
                            inactiveConversations.Add(newConversation);
                        }
                    });
                }

                conversations[senderAddress].ReceiveBuzz(buzz);

                // Kích hoạt sự kiện buzz nếu đây là conversation hiện tại
                if (CurrentConversation == senderAddress)
                {
                    buzzEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Đóng conversation (đánh dấu là inactive)
        /// </summary>
        public void CloseConversation(string address)
        {
            lock (_lock)
            {
                if (conversations.ContainsKey(address))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var conversation = conversations[address];
                        conversation.IsActive = false;

                        // Di chuyển từ active sang inactive
                        activeConversations.Remove(conversation);
                        if (!inactiveConversations.Contains(conversation))
                        {
                            inactiveConversations.Add(conversation);
                        }

                        // Nếu đây là conversation hiện tại, reset current conversation
                        if (CurrentConversation == address)
                        {
                            CurrentConversation = "";
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Lấy conversation theo địa chỉ
        /// </summary>
        public ConversationModel GetConversation(string address)
        {
            lock (_lock)
            {
                if (conversations.ContainsKey(address))
                {
                    return conversations[address];
                }
                return null;
            }
        }

        /// <summary>
        /// Xử lý khi ứng dụng đóng (lưu dữ liệu nếu cần)
        /// </summary>
        public void OnExit()
        {
            lock (_lock)
            {
                // Đánh dấu tất cả conversations là inactive
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var conversation in conversations.Values)
                    {
                        conversation.IsActive = false;
                    }
                    activeConversations.Clear();
                    foreach (var conversation in conversations.Values)
                    {
                        inactiveConversations.Add(conversation);
                    }
                });
            }
        }
    }
}
