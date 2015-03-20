using System;
using System.Collections.Generic;
using System.Linq;

namespace Commons.GetOptions
{
	public class OptionsWithSecondLevel : Options
	{
		public OptionsWithSecondLevel(OptionsContext context, string[] args = null)
			: base(context)
		{
		}

		[Option("Show an additional help list", Name = "help2")]
		public virtual WhatToDoNext DoHelp2()
		{
			return OptionParser.DoHelp2();
		}
	}
}