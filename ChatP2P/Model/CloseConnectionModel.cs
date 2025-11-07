using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatP2P.Model
{
    public class CloseConnectionModel : DataModel
    {
        public CloseConnectionModel(UserModel sender, string receiver) : base(sender, receiver) { }
    }
}