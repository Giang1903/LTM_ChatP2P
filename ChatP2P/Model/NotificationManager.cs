using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ChatP2P.Model
{
    public class NotificationManager
    {
        private Queue<String> notifications = new Queue<String>();
        public event EventHandler newNotification;
        private readonly object _lock = new();

        // Kiểm tra còn thông báo nào chưa hiển thị không
        public bool HasMoreNotifications() => notifications.Count != 0;

        // Thêm thông báo mới vào hàng đợi
        public void AddNotification(string message)
        {
            // Có thể gọi từ nhiều luồng → đảm bảo chỉ một luồng thao tác dữ liệu tại một thời điểm
            lock (_lock)
            {
                notifications.Enqueue(message);
            }
            if (notifications.Count == 1)
            {
                // Gửi tín hiệu đến ViewModel khi đây là thông báo đầu tiên trong hàng đợi
                newNotification?.Invoke(this, EventArgs.Empty);
            }
        }

        // Lấy thông báo cũ nhất
        public string GetLatestNotification()
        {
            return notifications.First();
        }

        // Xóa thông báo sau khi đã hiển thị
        public void DequeueNotification()
        {
            // Có thể gọi từ nhiều luồng → đảm bảo chỉ một luồng thao tác dữ liệu tại một thời điểm
            lock (_lock)
            {
                notifications.Dequeue();
            }
        }
    }
}