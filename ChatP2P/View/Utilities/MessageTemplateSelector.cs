using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatP2P.Model;
using System.Windows;
using System.Windows.Controls;

namespace ChatP2P.View.Utilities
{
    public class MessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UserMessageTemplate { get; set; }
        public DataTemplate OtherUserMessageTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var message = item as MessageModel;
            if (message == null)
                return base.SelectTemplate(item, container);

            return message.Sender.Address == NetworkManager.Instance.Host.Address ?
                UserMessageTemplate :
                OtherUserMessageTemplate;
        }
    }
}