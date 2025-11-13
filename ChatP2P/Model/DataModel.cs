using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace ChatP2P.Model
{
    public abstract class DataModel
    {
        private UserModel sender;
        private string receiver;
        private DateTime date;
        private string? message;

        public DataModel(UserModel sender, string receiver, string message = null)
        {
            this.sender = sender;
            this.receiver = receiver;
            date = DateTime.Now;
        }

        public string SenderAddr { get { return sender.Address; } }
        public UserModel Sender { get { return sender; } }
        public string Receiver { get { return receiver; } }
        public DateTime Date { get { return date; } }
        public string Name { get { return sender.Name; } }
        public string Message { get { return message; } set { message = value; } }
    }
}
