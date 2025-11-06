using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatP2P.Model
{
	public class BuzzModel
	{
		public bool IsOutgoing { get; set; }

		public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
	}
}
