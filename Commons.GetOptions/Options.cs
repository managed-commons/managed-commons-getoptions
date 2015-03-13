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

using System;
using System.Collections;

namespace Commons.GetOptions
{
	public class Options
	{
		public bool BreakSingleDashManyLettersIntoManyOptions;
		public bool DontSplitOnCommas;
		public bool EndOptionProcessingWithDoubleDash;
		public OptionsParsingMode ParsingMode;
		public string[] RemainingArguments;
		public ErrorReporter ReportError;

		public Options()
			: this(null)
		{
		}

		public Options(string[] args)
			: this(args, OptionsParsingMode.Both, false, true, false, null)
		{
		}

		public Options(string[] args,
					   OptionsParsingMode parsingMode,
					   bool breakSingleDashManyLettersIntoManyOptions,
					   bool endOptionProcessingWithDoubleDash,
					   bool dontSplitOnCommas) :
			this(args, OptionsParsingMode.Both, false, true, false, null)
		{ }

		public Options(string[] args,
					   OptionsParsingMode parsingMode,
					   bool breakSingleDashManyLettersIntoManyOptions,
					   bool endOptionProcessingWithDoubleDash,
					   bool dontSplitOnCommas,
					   ErrorReporter reportError)
		{
			ParsingMode = parsingMode;
			BreakSingleDashManyLettersIntoManyOptions = breakSingleDashManyLettersIntoManyOptions;
			EndOptionProcessingWithDoubleDash = endOptionProcessingWithDoubleDash;
			DontSplitOnCommas = dontSplitOnCommas;
			if (reportError == null)
				ReportError = new ErrorReporter(DefaultErrorReporter);
			else
				ReportError = reportError;
			InitializeOtherDefaults();
			if (args != null)
				ProcessArgs(args);
		}

		public virtual string AdditionalBannerInfo { get { return null; } }

		[Option("Show debugging info while processing options", '~', "debugoptions", SecondLevelHelp = true)]
		public bool DebuggingOfOptions
		{
			set
			{
				_debuggingOfOptions = value;
				if (value) {
					Console.WriteLine("ParsingMode = {0}", ParsingMode);
					Console.WriteLine("BreakSingleDashManyLettersIntoManyOptions = {0}", BreakSingleDashManyLettersIntoManyOptions);
					Console.WriteLine("EndOptionProcessingWithDoubleDash = {0}", EndOptionProcessingWithDoubleDash);
					Console.WriteLine("DontSplitOnCommas = {0}", DontSplitOnCommas);
				}
			}
			get { return _debuggingOfOptions; }
		}

		public string FifthArgument { get { return (_arguments.Count > 4) ? (string)_arguments[4] : null; } }

		public string FirstArgument { get { return (_arguments.Count > 0) ? (string)_arguments[0] : null; } }

		public string FourthArgument { get { return (_arguments.Count > 3) ? (string)_arguments[3] : null; } }

		public bool GotNoArguments { get { return _arguments.Count == 0; } }

		public bool RunningOnWindows
		{
			get
			{
				// check for non-Unix platforms - see FAQ for more details
				// http://www.mono-project.com/FAQ:_Technical#How_to_detect_the_execution_platform_.3F
				int platform = (int)Environment.OSVersion.Platform;
				return ((platform != 4) && (platform != 128));
			}
		}

		public string SecondArgument { get { return (_arguments.Count > 1) ? (string)_arguments[1] : null; } }

		public string ThirdArgument { get { return (_arguments.Count > 2) ? (string)_arguments[2] : null; } }

		[Option("Show verbose parsing of options", '.', "verbosegetoptions", SecondLevelHelp = true)]
		public bool VerboseParsingOfOptions
		{
			set { _verboseParsingOfOptions = value; }
			get { return _verboseParsingOfOptions; }
		}

		[ArgumentProcessor]
		public virtual void DefaultArgumentProcessor(string argument)
		{
			_arguments.Add(argument);
		}

		[Option("Display version and licensing information", 'V', "version")]
		public virtual WhatToDoNext DoAbout()
		{
			return _optionParser.DoAbout();
		}

		[Option("Show this help list", '?', "help")]
		public virtual WhatToDoNext DoHelp()
		{
			return _optionParser.DoHelp();
		}

		[Option("Show an additional help list", "help2")]
		public virtual WhatToDoNext DoHelp2()
		{
			return _optionParser.DoHelp2();
		}

		[Option("Show usage syntax and exit", "usage")]
		public virtual WhatToDoNext DoUsage()
		{
			return _optionParser.DoUsage();
		}

		public void ProcessArgs(string[] args)
		{
			_optionParser = new OptionList(this);
			_optionParser.AdditionalBannerInfo = AdditionalBannerInfo;
			_optionParser.ProcessArgs(args);
			RemainingArguments = (string[])_arguments.ToArray(typeof(string));
		}

		public void ShowBanner()
		{
			_optionParser.ShowBanner();
		}

		protected virtual void InitializeOtherDefaults()
		{
		}

		// Only subclasses may need to implement something here
		private ArrayList _arguments = new ArrayList();

		private bool _debuggingOfOptions = false;
		private OptionList _optionParser;
		private bool _verboseParsingOfOptions = false;

		private static void DefaultErrorReporter(int number, string message)
		{
			if (number > 0)
				Console.WriteLine("Error {0}: {1}", number, message);
			else
				Console.WriteLine("Error: {0}", message);
		}
	}

	public delegate void ErrorReporter(int num, string msg);
}