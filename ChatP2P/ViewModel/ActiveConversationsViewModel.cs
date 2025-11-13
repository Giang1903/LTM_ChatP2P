using ChatP2P.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using Newtonsoft.Json.Linq;

namespace ChatP2P.ViewModel
{
    internal class ActiveConversationsViewModel : INotifyPropertyChanged
    {
        // Danh sách các cuộc trò chuyện đang hoạt động
        private ObservableCollection<ConversationModel> conversations = new ObservableCollection<ConversationModel>(ConversationManager.Instance.GetActiveConversations());
        public ObservableCollection<ConversationModel> Conversations { get { return conversations; } }

        // Cuộc trò chuyện hiện đang được chọn
        private ConversationModel selectedConversation;
        public ConversationModel SelectedConversation
        {
            get { return selectedConversation; }
            set
            {
                selectedConversation = value;
                if (value != null)
                {
                    ConversationManager.Instance.AssignCurrentConversation(value.User.Address);
                }
                OnPropertyChanged(nameof(SelectedConversation));
            }
        }

        // Khởi tạo và thiết lập sự kiện lắng nghe
        public ActiveConversationsViewModel()
        {
            ConversationManager.Instance.conversationsUpdatedEvent += ReloadConversations;
            ConversationManager.Instance.inactiveConversationSetEvent += InactiveConversationSet;
            ConversationManager.Instance.activeConversationSetEvent += ActiveConversationSet;
        }

        // Đặt SelectedConversation = null khi người dùng chọn một cuộc trò chuyện từ danh sách cũ
        private void InactiveConversationSet(object sender, EventArgs e)
        {
            SelectedConversation = null;
        }

        // Cập nhật cuộc trò chuyện hiện tại
        public void ActiveConversationSet(object sender, EventArgs e)
        {
            if (SelectedConversation != ConversationManager.Instance.GetConversation())
                SelectedConversation = ConversationManager.Instance.GetConversation();
        }

        // Tải lại tất cả cuộc trò chuyện đang hoạt động từ Conversation Manager
        private void ReloadConversations(object sender, EventArgs e)
        {
            conversations = new ObservableCollection<ConversationModel>(ConversationManager.Instance.GetActiveConversations());
            OnPropertyChanged("Conversations");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Gửi thông báo khi thuộc tính thay đổi
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}