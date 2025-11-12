using ChatP2P.Model;
using ChatP2P.View;
using ChatP2P.ViewModel.Command;
using ChatP2P.Model;
using ChatP2P.ViewModel.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ChatP2P.ViewModel
{
   
    internal class ChatClientWindowViewModel : INotifyPropertyChanged
    {

      
        private string windowTitle = "";
        public string WindowTitle
        {
            get
            {
                return windowTitle;
            }
            set
            {
                windowTitle = value;
                OnPropertyChanged("WindowTitle");
            }
        }

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

                    Task.Delay(500).ContinueWith(t => ShouldShake = false);
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

      
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

        public ChatClientWindowViewModel()
        {
            ConversationManager.Instance.buzzEvent += ActivateBuzz;
            ConversationManager.Instance.Load();
            UserModel host = NetworkManager.Instance.Host;
            WindowTitle = $"{host.Name} - {host.Address}";
        }

      
        private void ActivateBuzz(object sender, EventArgs e)
        {
            ShouldShake = true;
        }

        public static void OnWindowClose()
        {
            NetworkManager.Instance.CloseServer();
            Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
        }
    }
}