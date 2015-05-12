using Commons.GetOptions;
using static System.Console;

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