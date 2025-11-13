using ChatP2P.Model;
using ChatP2P.ViewModel.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatP2P.ViewModel
{
    internal class SidebarRequestViewModel
    {
        // Địa chỉ IP nhập từ người dùng
        private string ip;
        public string Ip
        {
            get { return ip; }
            set { ip = value; }
        }

        // Cổng kết nối nhập từ người dùng
        private string port;
        public string Port
        {
            get { return port; }
            set { port = value; }
        }

        // Lệnh gửi yêu cầu kết nối
        private ICommand sendRequest = null;
        public ICommand SendRequest
        {
            get
            {
                if (sendRequest == null)
                {
                    sendRequest = new RequestCommand(this);
                    Ip = "";
                    Port = "";
                }
                return sendRequest;
            }
            set { sendRequest = value; }
        }

        // Thực hiện gửi yêu cầu kết nối đến một địa chỉ khác
        public void SendNewRequest()
        {
            NetworkManager manager = NetworkManager.Instance;
            string addr = ip + ":" + port;

            // Kiểm tra không được kết nối đến chính mình
            if (manager.Host.Address == addr)
            {
                ConversationManager.Instance.SendNotification("❌ Không thể kết nối với chính mình!");
            }
            // Kiểm tra đã kết nối với client này chưa
            else if (manager.IsClientConnected(addr))
            {
                ConversationManager.Instance.SendNotification($"❌ Bạn đã kết nối với {addr} rồi!");
                Ip = "";
                Port = "";
            }
            // Thực hiện kết nối mới
            else
            {
                manager.Connect(Ip, Port);
            }
        }
    }
}
