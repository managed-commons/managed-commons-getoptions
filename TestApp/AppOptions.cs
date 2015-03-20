using System;
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
			Console.WriteLine("Bonkers...");
			return WhatToDoNext.GoAhead;
		}
	}
}