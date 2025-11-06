using ChatP2P.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ChatP2P.Model;

namespace ChatP2P.View
{
    
    public partial class ChatClientWindow : Window
    {
        public ChatClientWindow()
        {
            InitializeComponent();
            this.DataContext = new ChatClientWindowViewModel();
        }

    }
}
