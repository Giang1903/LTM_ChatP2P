using ChatP2P.ViewModel;
using System;
using System.Windows.Input;

namespace ChatP2P.ViewModel.Command
{
    internal class SendMessageCommand : ICommand
    {
        private ConversationViewModel parent;

        public SendMessageCommand(ConversationViewModel parent)
        {
            this.parent = parent;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return parent.CanSend && !string.IsNullOrWhiteSpace(parent.OutgoingMessage);
        }

        public async void Execute(object parameter)
        {
            await parent.SendMessageAsync();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
