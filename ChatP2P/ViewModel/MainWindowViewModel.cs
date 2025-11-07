using ChatP2P.Model;
using ChatP2P.View;
using ChatP2P.ViewModel.Command;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ChatP2P.ViewModel
{

    internal class MainWindowViewModel : INotifyPropertyChanged
    {

        private string errorMessage = "";
        public string ErrorMessage { get { return errorMessage; } set { errorMessage = value; OnPropertyChanged("ErrorMessage"); } }


        private ObservableCollection<string> ipAddresses = new ObservableCollection<string>();
        public ObservableCollection<string> IpAddresses { get { return ipAddresses; } }


        private string selectedIp = "127.0.0.1";
        public string SelectedIp { get { return selectedIp; } set { selectedIp = value; } }


        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }


        private string port;
        public string Port
        {
            get { return port; }
            set { port = value; }
        }


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


        public void StartChatClient()
        {
            if (name.Length < 2)
            {
                ErrorMessage = "Tên phải có ít nhất 2 ký tự.";
            }
            else if (!IsValidPort())
            {
                ErrorMessage = "Vui lòng chọn cổng (port) từ 10.000 đến 64.000.";
            }
            else if (NetworkManager.IsPortOccupied(port))
            {
                ErrorMessage = "Cổng " + port + " hiện đang được sử dụng.";
            }
            else
            {
                ErrorMessage = "";
                NetworkManager.Instance.Listen(new UserModel("127.0.0.1", port, name));
                ChatClientWindow chatClientWindow = new ChatClientWindow();
                chatClientWindow.ShowDialog();
            }
        }

        public void OnError(object sender, EventArgs e)
        {
            ErrorMessage = $"Không thể lắng nghe (listen) trên cổng {port}.";
        }

        public void OnSuccess(object sender, EventArgs e)
        {
            ErrorMessage = "";
            NetworkManager.Instance.Listen(new UserModel(selectedIp, port, name));
            ChatClientWindow chatClientWindow = new ChatClientWindow();
            chatClientWindow.ShowDialog();
        }

        private bool IsValidPort()
        {
            int portInt = 0;

            try
            {
                portInt = Convert.ToInt32(port);
            }
            catch (FormatException)
            {
                return false;
            }

            if (portInt < 10000 || portInt > 64000)
            {
                return false;
            }

            return true;
        }
    }
}
