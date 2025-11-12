using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace ChatP2P.Model
{
    //Lớp ConversationModel định nghĩa một cuộc hội thoại giữa người dùng hiện tại và một người dùng khác.
    // Chứa ObservableCollection tin nhắn và các thông tin liên quan đến cuộc trò chuyện.
    public class ConversationModel : INotifyPropertyChanged
    {
        private string endpoint;
        public string Endpoint { get { return endpoint; } set { endpoint = value; } }

        private string name;
        public string Name { get; set; }

        private DateTime lastActivity;
        public DateTime LastActivity 
        { 
            get => lastActivity; 
            set 
            { 
                lastActivity = value; 
                OnPropertyChanged(nameof(LastActivity));
                OnPropertyChanged(nameof(LastActivityToString));
            } 
        }
        public string LastActivityToString { get { return DateToString(lastActivity); } }

        private bool unreadMessages = false;
        public bool UnreadMessages 
        { 
            get { return unreadMessages; } 
            set 
            { 
                unreadMessages = value; 
                OnPropertyChanged(nameof(UnreadMessages));
            } 
        }

        private bool unreadBuzz = false;
        public bool UnreadBuzz 
        { 
            get { return unreadBuzz; } 
            set 
            { 
                unreadBuzz = value; 
                OnPropertyChanged(nameof(UnreadBuzz));
            } 
        }

        private bool isActive = false;
       
        // Xác định conversation có đang active (có kết nối TCP) hay không
 
        public bool IsActive 
        { 
            get { return isActive; } 
            set 
            { 
                isActive = value; 
                OnPropertyChanged(nameof(IsActive));
            } 
        }

        public string Username { get { return user?.Name ?? ""; } }
        private UserModel user;
        public UserModel User 
        { 
            get { return user; } 
            set 
            { 
                user = value; 
                OnPropertyChanged(nameof(Username));
            } 
        }

        // ObservableCollection chứa tất cả các tin nhắn trong cuộc hội thoại
 
        private ObservableCollection<DataModel> messages = new ObservableCollection<DataModel>();
        public ObservableCollection<DataModel> Messages { get { return messages; } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ConversationModel() 
        {
            lastActivity = DateTime.Now;
        }
        public void ReceiveMessage(DataModel message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                lastActivity = message.Date;
                messages.Add(message);
                if (!(ConversationManager.Instance.CurrentConversation == User.Address))
                {
                    unreadMessages = true;
                    UnreadBuzz = false;
                }
                else
                {
                    unreadMessages = false;
                }
            });

        public ConversationModel(string endpoint)
        {
            this.endpoint = endpoint;
            lastActivity = DateTime.Now;
        }

        public ConversationModel(UserModel user)
        {
            this.user = user;
            this.endpoint = user.Address;
            this.name = user.Name;
            lastActivity = DateTime.Now;
        }


        // Nhận và thêm tin nhắn mới vào conversation

        public void ReceiveMessage(DataModel message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                lastActivity = message.Date;
                messages.Add(message);
                if (!(ConversationManager.Instance.CurrentConversation == User.Address))
                {
                    unreadMessages = true;
                    UnreadBuzz = false;
                }
                else
                {
                    unreadMessages = false;
                }
            });

        }

        // Nhận và xử lý tín hiệu buzz

        public void ReceiveBuzz(DataModel buzz)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                lastActivity = buzz.Date;
                if (User != null && !(ConversationManager.Instance.CurrentConversation == User.Address))
                {
                    unreadBuzz = true;
                    unreadMessages = false;
                }
                else
                {
                    unreadBuzz = false;
                }
                OnPropertyChanged(nameof(LastActivity));
                OnPropertyChanged(nameof(LastActivityToString));
            });
        }


        // Đánh dấu đã đọc tất cả tin nhắn
     
        public void MarkAsRead()
        {
            UnreadMessages = false;
            UnreadBuzz = false;
        }
        private static string DateToString(DateTime dateTime)
        {
            int seconds = (int)(DateTime.Now - dateTime).TotalSeconds;

            if (seconds < 30)
            {
                return "Just now";
            }
            else if (seconds < 60)
            {
                return $"{seconds} sec ago";
            }
            else if (seconds < 60 * 60)
            {
                int minutes = seconds / 60;
                return $"{minutes} min ago";
            }
            else if (seconds < (60 * 60 * 24))
            {
                int hours = seconds / 60 / 60;
                string tmp = hours == 1 ? "hour" : "hours";
                return $"{hours} {tmp} ago";
            }
            else if (seconds < (60 * 60 * 24 * 30))
            {
                int days = seconds / 60 / 60 / 24;
                string tmp = days == 1 ? "day" : "days";
                return $"{days} {tmp} ago";
            }
            else
            {
                return "Long ago";
            }
        }
    }
}