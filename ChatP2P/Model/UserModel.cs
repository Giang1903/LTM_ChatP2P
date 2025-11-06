using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatP2P.Model
{
    public class UserModel
    {
        public string Address { get; }
        public string Port { get; }
        public string Name { get; }

        public UserModel(string address, string port, string name)
        {
            Address = address;
            Port = port;
            Name = name;
        }

        public override string ToString()
        {
            return $"{Name} ({Address}:{Port})";
        }
    }
}
