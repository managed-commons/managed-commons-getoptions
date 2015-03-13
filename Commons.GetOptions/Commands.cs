//
// CommandsAndOptions.cs
//
// Copyright ©2002-2014 Rafael 'Monoman' Teixeira, Managed Commons Team
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;

namespace Commons.GetOptions
{
	public interface ICommand
	{
		string Description { get; }

		string Name { get; }

		void Execute(IEnumerable<string> args, ErrorReporter ReportError);
	}

	public class Commands
	{
		public List<ICommand> AllCommands = new List<ICommand>();
		public bool BreakSingleDashManyLettersIntoManyOptions = false;
		public bool DontSplitOnCommas = false;
		public bool EndOptionProcessingWithDoubleDash = true;
		public OptionsParsingMode ParsingMode = OptionsParsingMode.Both;
		public ErrorReporter ReportError;

		public virtual string AdditionalBannerInfo { get { return null; } }

		public void ProcessArgs(string[] args)
		{
			OptionList optionParser = new OptionList(this, ParsingMode, BreakSingleDashManyLettersIntoManyOptions, EndOptionProcessingWithDoubleDash, DontSplitOnCommas, ReportError);
			optionParser.AdditionalBannerInfo = AdditionalBannerInfo;
			if (args == null || args.Length == 0) {
				optionParser.DoUsage();
				return;
			}

			if (args[0].Equals("help", StringComparison.InvariantCultureIgnoreCase)) {
				optionParser.DoHelp(AllCommands);
				return;
			}
			foreach (var command in AllCommands) {
				if (args[0].Equals(command.Name, StringComparison.InvariantCultureIgnoreCase)) {
					OptionList parser = new OptionList(command, ParsingMode, BreakSingleDashManyLettersIntoManyOptions, EndOptionProcessingWithDoubleDash, DontSplitOnCommas, ReportError);
					parser.AdditionalBannerInfo = AdditionalBannerInfo;
					parser.ProcessArgs(args);
				}
			}
		}

		private static void DefaultErrorReporter(int number, string message)
		{
			if (number > 0)
				Console.WriteLine("Error {0}: {1}", number, message);
			else
				Console.WriteLine("Error: {0}", message);
		}
	}
}