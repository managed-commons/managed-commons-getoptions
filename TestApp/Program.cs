using System;

namespace TestApp
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var options = new AppOptions(args);
            Console.WriteLine("=================================================== -V --version");
            options.DoAbout();
			Console.WriteLine("=================================================== usage");
			options.DoUsage();
			Console.WriteLine("=================================================== -? --help");
			options.DoHelp();
			Console.WriteLine("=================================================== --help2");
			options.DoHelp2();
			Console.WriteLine("=================================================== Press a key");
            Console.ReadKey();
		}
	}
}
