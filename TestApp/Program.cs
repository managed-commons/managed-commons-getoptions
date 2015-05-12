using Commons;
using Commons.GetOptions;
using static System.Console;
using static Commons.Translation.TranslationService;

[assembly: About("See https://github.com/managed-commons/managed-commons-getoptions for details")]
[assembly: License(LicenseType.MIT)]

namespace TestApp
{
    internal class MainClass
    {
        public static void Main(string[] args)
        {
            RegisterTranslator(new AppTranslator());
            var options = new AppOptions(args);
            WriteLine("\n=================================================== -? --help\n");
            options.Reset();
            options.DoHelp();
            WriteLine("\n=================================================== -V --version\n");
            options.Reset();
            options.DoAbout();
            WriteLine("\n=================================================== --bonkers\n");
            options.DoBonkers();

            var commands = new AppCommands();
            WriteLine("\n=================================================== command help\n");
            commands.ProcessArgs(new string[] { "help" }, exitCode => (string[])null);
            WriteLine("\n=================================================== command help theta\n");
            commands.ProcessArgs(new string[] { "help", "theta" }, exitCode => (string[])null);
            WriteLine("\n=================================================== command help gamma\n");
            commands.ProcessArgs(new string[] { "help", "gamma" }, exitCode => (string[])null);
            WriteLine("\n=================================================== command gamma\n");
            commands.ProcessArgs(new string[] { "gamma" }, exitCode => (string[])null);
            WriteLine("\n=================================================== command gamma --GammaCorrected\n");
            commands.ProcessArgs(new string[] { "gamma", "--GammaCorrected" }, exitCode => (string[])null);
            WriteLine("\n=================================================== Press a key\n");
            ReadKey();
        }
    }
}