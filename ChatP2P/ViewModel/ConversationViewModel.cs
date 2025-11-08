using ChatP2P.Model;
using ChatP2P.ViewModel.Command;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
    }
}
