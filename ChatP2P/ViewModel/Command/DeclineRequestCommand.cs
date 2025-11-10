using System;
using System.Windows.Input;

namespace ChatP2P.ViewModel.Command
{
    internal class DeclineRequestCommand : ICommand
	{
		private readonly PendingRequestBarViewModel viewModel;

		public DeclineRequestCommand(PendingRequestBarViewModel viewModel)
		{
			this.viewModel = viewModel;
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute(object parameter)
		{
			viewModel.DeclineRequest();
		}
	}
}
