using System.Collections.ObjectModel;
using System.Windows;

namespace ChatP2P.Model
{
    //Lớp ConversationModel định nghĩa một cuộc hội thoại giữa người dùng hiện tại và một người dùng khác.
    // Chứa ObservableCollection tin nhắn và các thông tin liên quan đến cuộc trò chuyện.
    public class ConversationModel
    {
        private string endpoint;
        // Lấy endpoint (địa chỉ của người khác trong cuộc trò chuyện)
        public string Endpoint { get { return endpoint; } set { endpoint = value; } }

        private string name;
        // Tên người tham gia khác
        public string Name { get; set; }

        private DateTime lastActivity;
        // Thời điểm hoạt động cuối cùng
        public DateTime LastActivity { get => lastActivity; set { lastActivity = value; } }
        // Thời gian từ lần hoạt động cuối cùng (chuỗi để hiển thị GUI)
        public string LastActivityToString { get { return DateToString(lastActivity); } }

        private bool unreadMessages = false;
        // Kiểm tra có tin nhắn chưa đọc không
        public bool UnreadMessages { get { return unreadMessages; } set { unreadMessages = value; } }

        private bool unreadBuzz = false;
        // Kiểm tra có buzz chưa đọc không
        public bool UnreadBuzz { get { return unreadBuzz; } set { unreadBuzz = value; } }

        public string Username { get { return user.Name; } }

        private UserModel user;
        // Lấy thông tin UserModel của người khác
        public UserModel User { get { return user; } set { user = value; } }

        private ObservableCollection<DataModel> messages = new ObservableCollection<DataModel>();
        // Danh sách tin nhắn trong cuộc trò chuyện
        public ObservableCollection<DataModel> Messages { get { return messages; } }

        // Constructor rỗng để hỗ trợ serialization
        public ConversationModel() { }

        // Constructor chỉ nhận endpoint
        public ConversationModel(string endpoint)
        {
            this.endpoint = endpoint;
        }

        // Tạo cuộc trò chuyện mới với UserModel
        public ConversationModel(UserModel user)
        {
            this.user = user;
        }

        // Nhận tin nhắn mới trong cuộc trò chuyện
        public void ReceiveMessage(DataModel message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                lastActivity = message.Date;
                messages.Add(message);

                // Nếu không phải cuộc trò chuyện hiện tại, đánh dấu chưa đọc
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

        // Chuyển DateTime thành chuỗi hiển thị GUI (thời gian từ hoạt động cuối)
        private static string DateToString(DateTime dateTime)
        {
            int seconds = (int)(DateTime.Now - dateTime).TotalSeconds;

            if (seconds < 30)
            {
                return "Vừa xong";
            }
            else if (seconds < 60)
            {
                return $"{seconds} giây trước";
            }
            else if (seconds < 60 * 60)
            {
                int minutes = seconds / 60;
                return $"{minutes} phút trước";
            }
            else if (seconds < (60 * 60 * 24))
            {
                int hours = seconds / 3600;
                string tmp = hours == 1 ? "giờ" : "giờ";
                return $"{hours} {tmp} trước";
            }
            else if (seconds < (60 * 60 * 24 * 30))
            {
                int days = seconds / (3600 * 24);
                string tmp = days == 1 ? "ngày" : "ngày";
                return $"{days} {tmp} trước";
            }
            else
            {
                return "Lâu rồi";
            }
        }
    }
}