using ChatP2P.Model;
using ChatP2P.ViewModel.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatP2P.ViewModel
{
    internal class ConversationViewModel : INotifyPropertyChanged
    {
        // Tham chiếu đến cuộc trò chuyện hiện đang chọn
        private ConversationModel conversation = null;

        // Danh sách tin nhắn trong cuộc trò chuyện hiện tại
        private ObservableCollection<DataModel> messages = new ObservableCollection<DataModel>();
        public ObservableCollection<DataModel> Messages { get { return messages; } }

        // Kiểm tra có thể gửi tin nhắn không
        public bool CanSendMessage { get { return conversation != null && ConversationManager.Instance.CurrentConversationIsActive; } }
        public bool CanReconnect { get { return conversation != null && !ConversationManager.Instance.CurrentConversationIsActive; } }

        // Tự động cuộn xuống khi có tin nhắn mới
        private bool shouldScrollToEnd;
        public bool ShouldScrollToEnd
        {
            get { return shouldScrollToEnd; }
            set
            {
                shouldScrollToEnd = value;
                OnPropertyChanged(nameof(ShouldScrollToEnd));
            }
        }

        // Tin nhắn người dùng muốn gửi (liên kết textbox GUI)
        private string message;
        public string Message
        {
            get { return message; }
            set { message = value; OnPropertyChanged("Message"); }
        }

        // Lệnh gửi tin nhắn
        private ICommand sendMessageCommand = null;
        public ICommand SendMessageCommand
        {
            get
            {
                if (sendMessageCommand == null)
                {
                    sendMessageCommand = new SendMessageCommand(this);
                }
                return sendMessageCommand;
            }
            set { sendMessageCommand = value; }
        }

        // Lệnh gửi buzz
        private ICommand sendBuzzCommand = null;
        public ICommand SendBuzzCommand
        {
            get
            {
                if (sendBuzzCommand == null)
                {
                    sendBuzzCommand = new SendBuzzCommand(this);
                }
                return sendBuzzCommand;
            }
            set { sendBuzzCommand = value; }
        }

        // Lệnh kết nối lại cuộc trò chuyện
        private ICommand reconnectCommand = null;
        public ICommand ReconnectCommand
        {
            get
            {
                if (reconnectCommand == null)
                {
                    reconnectCommand = new ReconnectCommand(this);
                }
                return reconnectCommand;
            }
            set { reconnectCommand = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Thông báo thay đổi thuộc tính
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // Cập nhật conversation khi dữ liệu thay đổi
        private void ConversationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.UpdateConversation();
        }

        private void ConversationPropertyChanged(object sender, EventArgs e)
        {
            this.UpdateConversation();
        }

        // Khởi tạo: thiết lập các sự kiện lắng nghe
        public ConversationViewModel()
        {
            ConversationManager.Instance.PropertyChanged += ConversationPropertyChanged;
            ConversationManager.Instance.conversationsUpdatedEvent += ConversationPropertyChanged;
        }

        // Gửi tin nhắn
        public void SendMessage()
        {
            UpdateConversation();
            if (this.conversation != null)
            {
                MessageModel msg = new MessageModel(NetworkManager.Instance.Host, ConversationManager.Instance.CurrentConversation, message);
                ConversationManager.Instance.SendMessage(msg);
            }

            Message = "";
        }

        // Gửi buzz
        public void SendBuzz()
        {
            if (this.conversation != null)
            {
                BuzzModel msg = new BuzzModel(NetworkManager.Instance.Host, ConversationManager.Instance.CurrentConversation);
                ConversationManager.Instance.SendBuzz(msg);
            }
        }

        // Cập nhật conversation hiện tại từ ConversationManager
        private void UpdateConversation()
        {
            conversation = ConversationManager.Instance.GetConversation();
            if (conversation != null)
            {
                messages = conversation.Messages;
                ShouldScrollToEnd = true;
            }
            else
            {
                messages = new ObservableCollection<DataModel>();
            }
            OnPropertyChanged("Messages");
            OnPropertyChanged("CanSendMessage");
            OnPropertyChanged("CanReconnect");
        }

        // Thử kết nối lại cuộc trò chuyện với ip và port cụ thể
        public void AttemptReconnection()
        {
            try
            {
                string ip = conversation.User.Ip;
                string port = conversation.User.Port;
                NetworkManager.Instance.Connect(ip, port);
            }
            catch (KeyNotFoundException e)
            {
                ConversationManager.Instance.SendNotification($"❌ Lỗi: Không thể kết nối lại với {conversation.User.Ip}:{conversation.User.Port}.");
            }
        }
    }
}
