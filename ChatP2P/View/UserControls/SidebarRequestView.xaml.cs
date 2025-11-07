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

namespace ChatP2P.View.UserControls
{
    /// <summary>
    /// Interaction logic for SidebarRequestView.xaml
    /// </summary>
    public partial class SidebarRequestView : UserControl
    {
        public SidebarRequestView()
        {
            InitializeComponent();
            this.DataContext = new SidebarRequestViewModel();
        }
    }
}