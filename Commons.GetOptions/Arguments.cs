using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.GetOptions
{
	public class Arguments : List<string>
	{
		public Arguments(IEnumerable<string> list) : base(list) { }

		public string FifthArgument { get { return (Count > 4) ? this[4] : null; } }

		public string FirstArgument { get { return (Count > 0) ? this[0] : null; } }

		public string FourthArgument { get { return (Count > 3) ? this[3] : null; } }

		public bool GotNoArguments { get { return Count == 0; } }

		public string SecondArgument { get { return (Count > 1) ? this[1] : null; } }

		public string ThirdArgument { get { return (Count > 2) ? this[2] : null; } }
	}
}