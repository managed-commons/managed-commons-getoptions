using System;
using System.Console;
using Commons.GetOptions;

namespace TestApp
{
	public class AppOptions : Options
	{
		public AppOptions(string[] args)
			: base(new OptionsContext())
		{
		}

		[Option("Just some bonking for testing...", Name = "bonkers")]
		public WhatToDoNext DoBonkers()
		{
			WriteLine("Bonkers...");
			return WhatToDoNext.GoAhead;
		}
	}
}