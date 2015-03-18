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
using System.Linq;

namespace Commons.GetOptions
{
	public interface ICommand
	{
		string Description { get; }

		string Name { get; }

		void Execute(IEnumerable<string> args, ErrorReporter ReportError);
	}

	public class HelpCommand : ICommand
	{
		private readonly List<ICommand> _commands;
		private readonly OptionList _optionParser;
		private readonly OptionsContext _context;

		public HelpCommand(List<ICommand> commands, OptionList optionParser, OptionsContext context)
		{
			_commands = commands;
			_optionParser = optionParser;
			_context = context;
		}

		public string Description { get { return "Show help about commands"; } }

		public string Name { get { return "help"; } }

		public void Execute(IEnumerable<string> args, ErrorReporter ReportError)
		{
			var commandName = args.FirstOrDefault();
			if (!string.IsNullOrWhiteSpace(commandName)) {
				foreach (var command in _commands) {
					if (commandName.Equals(command.Name, StringComparison.InvariantCultureIgnoreCase)) {
						OptionList parser = new OptionList(command, _context);
						parser.DoHelp();
						return;
					}
				}
			} else
				_optionParser.DoHelp(_commands);
		}
	}

	public class Commands : IEqualityComparer<ICommand>
	{
		public readonly List<ICommand> AllCommands = new List<ICommand>();
		public readonly OptionsContext Context = new OptionsContext();

		public virtual string AdditionalBannerInfo { get { return null; } }

		public void ProcessArgs(string[] args, Func<int, string[]> exitFunc)
		{
			OptionList optionParser = new OptionList(this, Context, stopOnFirstNonOption: true);
			optionParser.AdditionalBannerInfo = AdditionalBannerInfo;
			if (args == null || args.Length == 0) {
				optionParser.DoUsage();
				return;
			}

			var helpCommand = new HelpCommand(AllCommands, optionParser, Context);
			AllCommands.Add(helpCommand);
			var commands = AllCommands.Distinct(this).OrderBy(command => command.Name.ToLowerInvariant()).ToList();
			AllCommands.Clear();
			AllCommands.AddRange(commands);

			var remainingArgs = optionParser.ProcessArgs(args, exitFunc);
			var commandName = remainingArgs.FirstOrDefault();
			var commandArgs = remainingArgs.Skip(1).ToArray();

			if (string.IsNullOrWhiteSpace(commandName)) {
				optionParser.DoUsage();
				return;
			}

			foreach (var command in AllCommands) {
				if (commandName.Equals(command.Name, StringComparison.InvariantCultureIgnoreCase)) {
					OptionList parser = new OptionList(command, Context);
					parser.AdditionalBannerInfo = AdditionalBannerInfo;
					command.Execute(parser.ProcessArgs(commandArgs, exitFunc), Context.ReportError);
				}
			}
		}

		public bool Equals(ICommand x, ICommand y)
		{
			return x.Name.Equals(y.Name,StringComparison.InvariantCultureIgnoreCase);
		}

		public int GetHashCode(ICommand obj)
		{
			return obj.Name.GetHashCode();
		}
	}
}