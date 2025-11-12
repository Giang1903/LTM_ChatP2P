using ChatP2P.Model;
using ChatP2P.ViewModel.Command;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatP2P.ViewModel
{
    internal class ConversationViewModel : INotifyPropertyChanged
    {
        private ConversationModel currentConversation;
        private readonly SendMessageCommand sendMessageCommand;
        private readonly SendBuzzCommand sendBuzzCommand;
        private string outgoingMessage = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ConversationModel CurrentConversation
        {
            get => currentConversation;
            private set
            {
                if (!ReferenceEquals(currentConversation, value))
                {
                    if (currentConversation != null)
                    {
                        currentConversation.PropertyChanged -= OnConversationPropertyChanged;
                    }

                    currentConversation = value;

                    if (currentConversation != null)
                    {
                        currentConversation.PropertyChanged += OnConversationPropertyChanged;
                    }

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Messages));
                    OnPropertyChanged(nameof(ConversationName));
                    OnPropertyChanged(nameof(HasConversation));
                    OnPropertyChanged(nameof(CanSend));
                    sendMessageCommand.RaiseCanExecuteChanged();
                    sendBuzzCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<DataModel>? Messages => currentConversation?.Messages;

        public string ConversationName => currentConversation?.Username ?? "Chưa chọn cuộc trò chuyện";

        public bool HasConversation => currentConversation != null;

        public bool CanSend => currentConversation?.IsActive == true;

        public string OutgoingMessage
        {
            get => outgoingMessage;
            set
            {
                if (outgoingMessage != value)
                {
                    outgoingMessage = value;
                    OnPropertyChanged();
                    sendMessageCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand SendMessage => sendMessageCommand;
        public ICommand SendBuzz => sendBuzzCommand;

        public ConversationViewModel()
        {
            sendMessageCommand = new SendMessageCommand(this);
            sendBuzzCommand = new SendBuzzCommand(this);
        }

        public void SetCurrentConversation(ConversationModel conversation)
        {
            CurrentConversation = conversation;

            if (conversation != null)
            {
                ConversationManager.Instance.CurrentConversation = conversation.Endpoint;
                conversation.MarkAsRead();
            }
        }

        public async Task SendMessageAsync()
        {
            if (!CanSend || string.IsNullOrWhiteSpace(OutgoingMessage) || CurrentConversation == null)
            {
                return;
            }

            var host = NetworkManager.Instance.Host;
            if (host == null)
            {
                NotificationManager.Instance.AddNotification("⚠️ Chưa khởi tạo thông tin người dùng cục bộ.");
                return;
            }

            string messageText = OutgoingMessage.Trim();
            OutgoingMessage = string.Empty;

            try
            {
                var message = new MessageModel(host, CurrentConversation.Endpoint, messageText);
                await NetworkManager.Instance.SendMessage(message);
                CurrentConversation.ReceiveMessage(message);
                CurrentConversation.MarkAsRead();
                OnPropertyChanged(nameof(Messages));
            }
            catch (Exception ex)
            {
                NotificationManager.Instance.AddNotification($"❌ Gửi tin nhắn thất bại. {ex.Message}");
            }
        }

        public async Task SendBuzzAsync()
        {
            if (!CanSend || CurrentConversation == null)
            {
                return;
            }

            var host = NetworkManager.Instance.Host;
            if (host == null)
            {
                NotificationManager.Instance.AddNotification("⚠️ Chưa khởi tạo thông tin người dùng cục bộ.");
                return;
            }

            try
            {
                var buzz = new BuzzModel(host, CurrentConversation.Endpoint);
                await NetworkManager.Instance.SendMessage(buzz);
                CurrentConversation.ReceiveBuzz(buzz);
            }
            catch (Exception ex)
            {
                NotificationManager.Instance.AddNotification($"❌ Không thể gửi buzz. {ex.Message}");
            }
        }

        private void OnConversationPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ConversationModel.IsActive))
            {
                OnPropertyChanged(nameof(CanSend));
                sendMessageCommand.RaiseCanExecuteChanged();
                sendBuzzCommand.RaiseCanExecuteChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private ConversationModel conversation = null;

        /// <summary>
        /// An observable collection of all messages in the currently selected conversation.
        /// </summary>
        private ObservableCollection<DataModel> messages = new ObservableCollection<DataModel>();
        public ObservableCollection<DataModel> Messages { get { return messages; } }

        /// <summary>
        /// Boolean that is true if its possible to send a message in the currently selected conversation.
        /// </summary>
        public bool CanSendMessage { get { return conversation != null && ConversationManager.Instance.CurrentConversationIsActive; } }
        public bool CanReconnect { get { return conversation != null && !ConversationManager.Instance.CurrentConversationIsActive; } }

        /// <summary>
        /// Boolean that trigger the GUI to autoscroll to the bottom when a new message is received.
        /// </summary>
        private bool shouldScrollToEnd;
        public bool ShouldScrollToEnd
        {
            get { return shouldScrollToEnd; }
            set
            {
                shouldScrollToEnd = value;
                OnPropertyChanged(nameof(ShouldScrollToEnd));
            }
        }

        /// <summary>
        /// The message that the user wants to send. Binds to a textbox in the GUI.
        /// </summary>
        private string message;
        public string Message
        {
            get { return message; }
            set { message = value; OnPropertyChanged("Message"); }
        }

        /// <summary>
        /// Command that is triggered when the user wants to send a new message.
        /// </summary>
        private ICommand sendMessageCommand = null;
        public ICommand SendMessageCommand
        {
            get
            {
                if (sendMessageCommand == null)
                {
                    sendMessageCommand = new SendMessageCommand(this);
                }
                return sendMessageCommand;
            }
            set { sendMessageCommand = value; }
        }

        /// <summary>
        /// Command that is triggered when the user wants to send a buzz.
        /// </summary>
        private ICommand sendBuzzCommand = null;
        public ICommand SendBuzzCommand
        {
            get
            {
                if (sendBuzzCommand == null)
                {
                    sendBuzzCommand = new SendBuzzCommand(this);
                }
                return sendBuzzCommand;
            }
            set { sendBuzzCommand = value; }
        }

        /// <summary>
        /// Command that is triggered when the user wants to reconnect to an old conversation.
        /// </summary>
        private ICommand reconnectCommand = null;
        public ICommand ReconnectCommand
        {
            get
            {
                if (reconnectCommand == null)
                {
                    reconnectCommand = new ReconnectCommand(this);
                }
                return reconnectCommand;
            }
            set { reconnectCommand = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ConversationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.UpdateConversation();
        }

        private void ConversationPropertyChanged(object sender, EventArgs e)
        {
            this.UpdateConversation();

        }

        /// <summary>
        /// Constructor that setups the eventlisteners.
        /// </summary>
        public ConversationViewModel()
        {
            ConversationManager.Instance.PropertyChanged += ConversationPropertyChanged;
            ConversationManager.Instance.conversationsUpdatedEvent += ConversationPropertyChanged;
        }

        /// <summary>
        /// Updates the conversation and sends a message to the Conversation Manager.
        /// </summary>
        public void SendMessage()
        {
            UpdateConversation();
            if (this.conversation != null)
            {
                MessageModel msg = new MessageModel(NetworkManager.Instance.Host, ConversationManager.Instance.CurrentConversation, message);
                ConversationManager.Instance.SendMessage(msg);
            }

            Message = "";
        }

        /// <summary>
        /// Sends a buzz to the Conversation Manager.
        /// </summary>
        public void SendBuzz()
        {
            if (this.conversation != null)
            {
                BuzzModel msg = new BuzzModel(NetworkManager.Instance.Host, ConversationManager.Instance.CurrentConversation);
                ConversationManager.Instance.SendBuzz(msg);
            }
        }

        /// <summary>
        /// Loads the conversation that is currently selected by the user.
        /// </summary>
        private void UpdateConversation()
        {
            conversation = ConversationManager.Instance.GetConversation();
            if (conversation != null)
            {
                messages = conversation.Messages;
                ShouldScrollToEnd = true;
            }
            else
            {
                messages = new ObservableCollection<DataModel>();
            }
            OnPropertyChanged("Messages");
            OnPropertyChanged("CanSendMessage");
            OnPropertyChanged("CanReconnect");
        }

        /// <summary>
        /// Sends a request to the Network Manager that the user wants to reconnect to a specific ip and port.
        /// </summary>
        public void AttemptReconnection()
        {
            try
            {
                string ip = conversation.User.Ip;
                string port = conversation.User.Port;
                NetworkManager.Instance.Connect(ip, port);
            }
            catch (KeyNotFoundException e)
            {
                ConversationManager.Instance.SendNotification($"❌ Error: Cannot reconnect to {conversation.User.Ip}:{conversation.User.Port}.");
            }

        }

    }

}
