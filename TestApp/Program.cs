using System;

namespace TestApp
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var options = new AppOptions(args);
			options.DoAbout();
			Console.WriteLine("======================================================");
			options.DoUsage();
			Console.WriteLine("======================================================");
			options.DoHelp();
			Console.WriteLine("======================================================");
			options.DoHelp2();
			Console.WriteLine("======================================================");
		}
	}
}
