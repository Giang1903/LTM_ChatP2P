using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatP2P.Model
{
	public class UserModel
	{
		public string Name { get; set; } = string.Empty;

		public string IpAddress { get; set; } = string.Empty;

		public int Port { get; set; }
	}
}
