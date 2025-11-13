using ChatP2P.Model;
using ChatP2P.View;
using ChatP2P.ViewModel.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatP2P.ViewModel
{
    // ViewModel cho cửa sổ chính
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        // Thông báo lỗi hiển thị khi có lỗi xảy ra
        private string errorMessage = "";
        public string ErrorMessage { get { return errorMessage; } set { errorMessage = value; OnPropertyChanged("ErrorMessage"); } }

        // Danh sách các địa chỉ IP khả dụng để lắng nghe
        private ObservableCollection<string> ipAddresses = new ObservableCollection<string>();
        public ObservableCollection<string> IpAddresses { get { return ipAddresses; } }

        // Địa chỉ IP được chọn hiện tại
        private string selectedIp = "127.0.0.1";
        public string SelectedIp { get { return selectedIp; } set { selectedIp = value; } }

        // Tên người dùng nhập vào
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        // Cổng người dùng nhập vào
        private string port;
        public string Port
        {
            get { return port; }
            set { port = value; }
        }

        // Lệnh để bắt đầu cửa sổ chat
        private ICommand startClient;
        public ICommand StartClient
        {
            get
            {
                if (startClient == null)
                {
                    startClient = new LoginCommand(this);
                }
                return startClient;
            }
            set { startClient = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // Hàm khởi tạo: thiết lập sự kiện và lấy danh sách IP có sẵn
        public MainWindowViewModel()
        {
            NetworkManager.Instance.listenerFailedEvent += OnError;
            NetworkManager.Instance.listenerSuccessEvent += OnSuccess;
            ipAddresses.Add("127.0.0.1");

            string localIp = NetworkManager.GetIpAddress();
            if (localIp != null)
            {
                ipAddresses.Add(localIp);
            }

            OnPropertyChanged("IpAddresses");
        }

        // Bắt đầu cửa sổ chat hoặc hiển thị lỗi nếu có vấn đề
        public void StartChatClient()
        {
            if (name.Length < 2)
            {
                ErrorMessage = "Tên phải có ít nhất 2 ký tự.";
            }
            else if (!IsValidPort())
            {
                ErrorMessage = "Vui lòng chọn cổng trong khoảng từ 10.000 đến 64.000.";
            }
            else if (NetworkManager.IsPortOccupied(port))
            {
                ErrorMessage = $"Cổng {port} hiện đang được sử dụng.";
            }
            else
            {
                ErrorMessage = "";
                NetworkManager.Instance.Listen(new UserModel("127.0.0.1", port, name));
                ChatClientWindow chatClientWindow = new ChatClientWindow();
                chatClientWindow.ShowDialog();
            }
        }

        // Hiển thị lỗi khi không thể lắng nghe cổng
        public void OnError(object sender, EventArgs e)
        {
            ErrorMessage = $"Không thể lắng nghe trên cổng {port}.";
        }

        // Mở cửa sổ chat khi không có lỗi
        public void OnSuccess(object sender, EventArgs e)
        {
            ErrorMessage = "";
            NetworkManager.Instance.Listen(new UserModel(selectedIp, port, name));
            ChatClientWindow chatClientWindow = new ChatClientWindow();
            chatClientWindow.ShowDialog();
        }

        // Kiểm tra xem cổng có hợp lệ không
        private bool IsValidPort()
        {
            Int32 portInt = 0;

            try
            {
                portInt = Convert.ToInt32(port);
            }
            catch (FormatException)
            {
                return false;
            }

            if (portInt < 9999 || portInt > 64001)
            {
                return false;
            }

            return true;
        }
    }
}
