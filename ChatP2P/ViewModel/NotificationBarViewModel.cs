using ChatP2P.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatP2P.ViewModel
{
    class NotificationBarViewModel : INotifyPropertyChanged
    {
        // Chuỗi thông báo sẽ hiển thị cho người dùng
        private string notification = "";
        public string Notification
        {
            get { return notification; }
            set { notification = value; OnPropertyChanged("Notification"); }
        }

        // Tham chiếu đến Notification Manager
        private NotificationManager notificationManager;
        public NotificationManager NotificationManager
        {
            get { return notificationManager; }
            set { notificationManager = value; }
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

        // Hàm khởi tạo: tạo Notification Manager và liên kết với NetworkManager và ConversationManager
        public NotificationBarViewModel(NotificationManager manager)
        {
            notificationManager = manager;
            NetworkManager.Instance.NotificationManager = notificationManager;
            ConversationManager.Instance.NotificationManager = notificationManager;

            
        }

        // Hiển thị tất cả thông báo trong hàng đợi nếu còn thông báo
        private async Task SendNotification()
        {
            while (notificationManager.HasMoreNotifications())
            {
                Notification = notificationManager.GetLatestNotification();
                await Task.Delay(3000); // Hiển thị mỗi thông báo 3 giây
                notificationManager.DequeueNotification();
            }
            Notification = "";
        }

        // Gọi khi có thông báo mới cần hiển thị
        public void DisplayNotification(object sender, EventArgs e)
        {
            Task.Run(() => SendNotification().ConfigureAwait(false));
        }
    }

}
