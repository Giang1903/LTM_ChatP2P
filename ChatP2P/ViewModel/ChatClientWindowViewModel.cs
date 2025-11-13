using ChatP2P.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatP2P.ViewModel.Command;
using ChatP2P.View;
using System.Windows;

namespace ChatP2P.ViewModel
{

    internal class ChatClientWindowViewModel : INotifyPropertyChanged
    {
        // Tiêu đề cửa sổ, kết hợp tên và địa chỉ endpoint
        private string windowTitle = "";
        public string WindowTitle
        {
            get { return windowTitle; }
            set
            {
                windowTitle = value;
                OnPropertyChanged("WindowTitle");
            }
        }

        // Biến kích hoạt rung cửa sổ khi nhận buzz
        private bool shouldShake;
        public bool ShouldShake
        {
            get { return shouldShake; }
            set
            {
                if (shouldShake != value)
                {
                    shouldShake = value;
                    OnPropertyChanged(nameof(ShouldShake));

                    // Sau 500ms tự tắt hiệu ứng rung
                    Task.Delay(500).ContinueWith(t => ShouldShake = false);
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Command chạy khi đóng cửa sổ
        public ICommand onClose { get; private set; }
        public ICommand OnClose
        {
            get
            {
                if (onClose == null)
                {
                    onClose = new CloseWindowCommand(param => OnWindowClose(), null);
                }
                return onClose;
            }
            set { onClose = value; }
        }

        // Constructor: đăng ký sự kiện buzz và tạo tiêu đề cửa sổ
        public ChatClientWindowViewModel()
        {
            ConversationManager.Instance.buzzEvent += ActivateBuzz;
            UserModel host = NetworkManager.Instance.Host;
            WindowTitle = $"{host.Name} - {host.Address}";
        }

        // Kích hoạt hiệu ứng rung khi nhận buzz
        private void ActivateBuzz(object sender, EventArgs e)
        {
            ShouldShake = true;
        }

        // Được gọi khi đóng cửa sổ: đóng server và thoát ứng dụng
        public static void OnWindowClose()
        {
            NetworkManager.Instance.CloseServer();
            Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
        }
    }
}