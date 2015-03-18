using System;
using Commons;
using Commons.GetOptions;

[assembly: UsageComplement("...")]
[assembly: About("See https://github.com/managed-commons/managed-commons-getoptions for details")]
[assembly: License(LicenseType.MIT)]

namespace TestApp
{
	internal class MainClass
	{
		public static void Main(string[] args)
		{
			var options = new AppOptions(args);
			Console.WriteLine("\n=================================================== usage\n");
			options.Reset();
			options.DoUsage();
			Console.WriteLine("\n=================================================== -? --help\n");
			options.Reset();
			options.DoHelp();
			Console.WriteLine("\n=================================================== -V --version\n");
			options.Reset();
			options.DoAbout();
			Console.WriteLine("\n=================================================== --bonkers\n");
			options.DoBonkers();

			var commands = new AppCommands();
			Console.WriteLine("\n=================================================== command help\n");
			commands.ProcessArgs(new string[] {"help"}, exitCode => (string[])null);
			Console.WriteLine("\n=================================================== command help theta\n");
			commands.ProcessArgs(new string[] { "help", "theta" }, exitCode => (string[])null);
			Console.WriteLine("\n=================================================== command help gamma\n");
			commands.ProcessArgs(new string[] { "help", "gamma" }, exitCode => (string[])null);
			Console.WriteLine("\n=================================================== command gamma\n");
			commands.ProcessArgs(new string[] {"gamma"}, exitCode => (string[])null);
			Console.WriteLine("\n=================================================== command gamma --GammaCorrected\n");
			commands.ProcessArgs(new string[] { "gamma", "--GammaCorrected" }, exitCode => (string[])null);
			Console.ReadKey();
			Console.WriteLine("\n=================================================== Press a key\n");
		}
	}
}