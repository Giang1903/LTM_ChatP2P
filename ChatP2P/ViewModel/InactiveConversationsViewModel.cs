using ChatP2P.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace ChatP2P.ViewModel
{
    internal class InactiveConversationsViewModel : INotifyPropertyChanged
    {
        // Danh sách các cuộc trò chuyện cũ
        private ObservableCollection<ConversationModel> conversations;
        public ObservableCollection<ConversationModel> Conversations { get { return conversations; } }

        // Danh sách các cuộc trò chuyện cũ sau khi lọc theo từ khóa tìm kiếm
        private ObservableCollection<ConversationModel> filteredConversations = new ObservableCollection<ConversationModel>();
        public ObservableCollection<ConversationModel> FilteredConversations
        {
            get { return filteredConversations; }
            set { filteredConversations = value; OnPropertyChanged("FilteredConversations"); }
        }

        // Từ khóa tìm kiếm (liên kết với ô tìm kiếm trong giao diện)
        private string searchQuery = "";
        public string SearchQuery
        {
            get { return searchQuery; }
            set
            {
                searchQuery = value;
                OnPropertyChanged("SearchQuery");
                UpdateSearch();
            }
        }

        // Cuộc trò chuyện hiện được chọn
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

        // Làm mới danh sách cuộc trò chuyện cũ mỗi phút
        private async Task Refresh()
        {
            while (true)
            {
                UpdateSearch();
                await Task.Delay(60000);
            }
        }

        // Hàm khởi tạo: thiết lập sự kiện và tải danh sách cuộc trò chuyện cũ từ Conversation Manager
        public InactiveConversationsViewModel()
        {
            ConversationManager.Instance.conversationsUpdatedEvent += ReloadInactiveConversations;
            ConversationManager.Instance.inactiveConversationSetEvent += InactiveConversationSet;
            ConversationManager.Instance.activeConversationSetEvent += ActiveConversationSet;

            conversations = new ObservableCollection<ConversationModel>(ConversationManager.Instance.GetInactiveConversations());
            FilteredConversations = conversations;

            Task.Run(() => { Refresh(); }).ConfigureAwait(false);
        }

        // Cập nhật lại cuộc trò chuyện hiện tại khi người dùng chọn một cuộc trò chuyện cũ
        private void InactiveConversationSet(object sender, EventArgs e)
        {
            if (SelectedConversation != ConversationManager.Instance.GetConversation())
                SelectedConversation = ConversationManager.Instance.GetConversation();
        }

        // Bỏ chọn cuộc trò chuyện cũ khi người dùng chọn một cuộc trò chuyện đang hoạt động
        public void ActiveConversationSet(object sender, EventArgs e)
        {
            SelectedConversation = null;
        }

        // Tải lại tất cả cuộc trò chuyện cũ từ Conversation Manager
        private void ReloadInactiveConversations(object sender, EventArgs e)
        {
            conversations = new ObservableCollection<ConversationModel>(ConversationManager.Instance.GetInactiveConversations());
            UpdateSearch();
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

        // Cập nhật danh sách cuộc trò chuyện được lọc mỗi khi người dùng nhập từ khóa tìm kiếm
        private void UpdateSearch()
        {
            if (searchQuery.Length > 0)
            {
                FilteredConversations = new ObservableCollection<ConversationModel>(conversations.Where(item => item.User.Name.ToUpper().Contains(searchQuery.ToUpper())).ToList());
            }
            else
            {
                FilteredConversations = new ObservableCollection<ConversationModel>(conversations);
            }
        }
    }
}