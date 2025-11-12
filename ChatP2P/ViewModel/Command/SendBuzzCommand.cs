using ChatP2P.ViewModel;
using System;
using System.Windows.Input;

namespace ChatP2P.ViewModel.Command
{
    internal class SendBuzzCommand : ICommand
    {
        private ConversationViewModel parent;

        public SendBuzzCommand(ConversationViewModel parent)
        {
            this.parent = parent;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return parent.CanSend;
        }

        public async void Execute(object parameter)
        {
            await parent.SendBuzzAsync();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
