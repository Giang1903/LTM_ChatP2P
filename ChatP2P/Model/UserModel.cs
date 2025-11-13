using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatP2P.Model
{
    public class UserModel
    {
        private string ip { get; }
        private string port { get; }
        private string address { get; }
        private string name { get; }
        public UserModel(string ip, string port, string name)
        {
            this.ip = ip;
            this.port = port;
            this.address = ip + ":" + port;
            this.name = name;
        }
        public string Ip { get { return ip; } }
        public string Port { get { return port; } }
        public string Address { get { return ip + ":" + port; } }
        public string Name { get { return name; } }
    }
}