// Commons.GetOptions
//
// Copyright (c) 2002-2015 Rafael 'Monoman' Teixeira, Managed Commons Team
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using Commons.GetOptions;
using static System.Console;
using static Commons.Translation.TranslationService;

namespace TestApp
{
    internal class AppCommands : Commands
    {
        public AppCommands()
        {
            AllCommands.Add(new GammaCommand());
            AllCommands.Add(new AlphaCommand());
            AllCommands.Add(new BetaCommand());
            AllCommands.Add(new ThetaCommand());
        }

        class AlphaCommand : ICommand
        {
            [Option("Only gamma command needs this set!")]
            public bool GammaCorrected = false;

            public virtual string Description => _("First mock command");

            public virtual string Name => "alpha";

            public virtual void Execute(IEnumerable<string> args, ErrorReporter ReportError)
            {
                WriteLine(TranslateAndFormat("Command {0} executed!", Name));
            }
        }

        private class BetaCommand : AlphaCommand
        {
            public override string Description => _("Second mock command");

            public override string Name => "beta";
        }

        class GammaCommand : AlphaCommand
        {
            public override string Description => _("Third mock command");

            public override string Name => "gamma";

            public override void Execute(IEnumerable<string> args, ErrorReporter ReportError)
            {
                if (!GammaCorrected)
                    ReportError(13, _("Should have been gamma corrected!!!"));
                else
                    base.Execute(args, ReportError);
            }
        }

        class ThetaCommand : AlphaCommand
        {
            public override string Description => _("Fourth mock command");

            public override string Name => "theta";
        }
    }
}
