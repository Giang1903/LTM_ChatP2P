using ChatP2P.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
﻿using System;
using System.Windows.Input;

namespace ChatP2P.ViewModel.Command
{
    
    internal class AcceptRequestCommand : ICommand
    {
        private PendingRequestBarViewModel parent;

        public AcceptRequestCommand(PendingRequestBarViewModel parent)
        {
            this.parent = parent;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            parent.AcceptRequest();
        }
    }

}
    internal class AcceptRequestCommand : ICommand
	{
		private readonly PendingRequestBarViewModel viewModel;

		public AcceptRequestCommand(PendingRequestBarViewModel viewModel)
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
			viewModel.AcceptRequest();
		}
	}
}
