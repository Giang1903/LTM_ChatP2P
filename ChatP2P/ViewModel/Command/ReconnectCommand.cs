using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatP2P.ViewModel.Command
{
    internal class ReconnectCommand : ICommand
    {
        // Tham chiếu đến ViewModel của cuộc trò chuyện
        private ConversationViewModel parent;

        // Khởi tạo command với parent ViewModel
        public ReconnectCommand(ConversationViewModel parent)
        {
            this.parent = parent;
        }

        public event EventHandler? CanExecuteChanged;

        // Luôn có thể thực thi lệnh
        public bool CanExecute(object parameter)
        {
            return true;
        }

        // Thực thi lệnh: gọi hàm kết nối lại cuộc trò chuyện
        public void Execute(object parameter)
        {
            System.Diagnostics.Debug.WriteLine("Nút kết nối lại được nhấn...");
            parent.AttemptReconnection();
        }
    }
}
