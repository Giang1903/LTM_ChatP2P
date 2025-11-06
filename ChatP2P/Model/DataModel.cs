using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatP2P.Model
{
    public abstract class DataModel
    {
        private readonly UserModel sender;
        private readonly string receiver;
        private DateTime date;
        private string? message;

        public DataModel(UserModel sender, string receiver, string? message = null, DateTime? date = null)
        {
            this.sender = sender;
            this.receiver = receiver;
            this.message = message;
            this.date = date ?? DateTime.Now;
        }

        public string SenderAddr { get { return sender.Address; } }
        public UserModel Sender { get { return sender; } }
        public string ReceiverAddr { get { return receiver; } }
        public DateTime Date { get { return date; } }
        public string Name { get { return sender.Name; } }
        public string? Message { get { return message; } set { message = value; } }
    }
}
