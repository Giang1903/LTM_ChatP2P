using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatP2P.Model
{
    public class MessageModel : DataModel
    {
        public MessageModel(UserModel sender, string receiver, string message) : base(sender, receiver)
        {
            Message = message;
        }
    }
}