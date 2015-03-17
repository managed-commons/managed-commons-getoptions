using System;
using Commons.GetOptions;

namespace TestApp
{
	public class AppOptions: Options
	{
		public AppOptions (string[] args) : base(args)
		{
		}

		[KillInheritedOption]
		public override WhatToDoNext DoHelp2 ()
		{
			return base.DoHelp2();
		}
	}
}

