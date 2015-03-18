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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using _ = Commons.TranslationService;

namespace Commons.GetOptions
{
	/// <summary>
	/// Option Parsing
	/// </summary>
	public class OptionList
	{
		public OptionList(object optionBundle, OptionsContext context, bool stopOnFirstNonOption = false)
		{
			if (optionBundle == null)
				throw new ArgumentNullException("optionBundle");
			if (context == null)
				throw new ArgumentNullException("context");

			Type optionsType = optionBundle.GetType();
			_optionBundle = optionBundle;
			_context = context;
			_stopOnFirstNonOption = stopOnFirstNonOption;
			_assemblyInfo = AssemblyInformation.FromEntryAssembly.WithDefaults;

			foreach (MemberInfo mi in optionsType.GetMembers()) {
				object[] attribs = mi.GetCustomAttributes(typeof(KillInheritedOptionAttribute), true);
				if (attribs == null || attribs.Length == 0) {
					attribs = mi.GetCustomAttributes(typeof(OptionAttribute), true);
					if (attribs != null && attribs.Length > 0) {
						OptionDetails option = new OptionDetails(mi, (OptionAttribute)attribs[0], optionBundle, _context.ParsingMode, false);
						_list.Add(option);
						_hasSecondLevelHelp = _hasSecondLevelHelp || option.SecondLevelHelp;
					} else if (mi.DeclaringType == mi.ReflectedType) { // not inherited
						attribs = mi.GetCustomAttributes(typeof(ArgumentProcessorAttribute), true);
						if (attribs != null && attribs.Length > 0)
							AddArgumentProcessor(mi);
					}
				}
			}

			if (_argumentProcessor == null) // try to find an inherited one
				foreach (MemberInfo mi in optionsType.GetMembers())
					if (mi.DeclaringType != mi.ReflectedType) { // inherited
						object[] attribs = mi.GetCustomAttributes(typeof(ArgumentProcessorAttribute), true);
						if (attribs != null && attribs.Length > 0)
							AddArgumentProcessor(mi);
					}

			_list.Sort();
			OptionDetails.LinkAlternatesInsideList(_list);
		}

		public string AdditionalBannerInfo { set { _assemblyInfo.AdditionalBannerInfo = value; } }

		public string[] ProcessArgs(string[] originalArgs, Func<int, string[]> exitFunc)
		{
			var args = NormalizeArgs(originalArgs);
			try {
				int argc = args.Length;
				bool foundNonOption = false;
				for (int i = 0; i < argc; i++) {
					string arg = args[i];
					string nextArg = (i + 1 < argc) ? nextArg = args[i + 1] : null;

					if (!string.IsNullOrWhiteSpace(arg)) {
						if (MaybeAnOption(arg) && !foundNonOption) {
							bool OptionWasProcessed = false;
							foreach (OptionDetails option in _list) {
								OptionProcessingResult result = option.ProcessArgument(arg, nextArg);
								if (result != OptionProcessingResult.NotThisOption) {
									OptionWasProcessed = true;
									if (result == OptionProcessingResult.OptionConsumedParameter)
										i++;
									break;
								}
							}
							if (!OptionWasProcessed) {
								Console.WriteLine(_.Translate("Invalid argument: '{0}'"), arg);
								DoHelp();
								return exitFunc(1);
							}
						} else {
							ProcessNonOption(arg);
							if (_stopOnFirstNonOption && !foundNonOption)
								foundNonOption = true;
						}
					}
				}

				foreach (OptionDetails option in _list)
					option.TransferValues();

				foreach (string argument in _argumentsTail)
					ProcessNonOption(argument);

				return (string[])_arguments.ToArray(typeof(string));
			} catch (Exception ex) {
				Console.WriteLine(_.Translate("Exception: {0}"), ex);
				return exitFunc(1);
			}
		}

		public void ShowBanner()
		{
			if (!_bannerAlreadyShown)
				_assemblyInfo.ShowBanner();
			_bannerAlreadyShown = true;
		}

		internal WhatToDoNext DoAbout()
		{
			_assemblyInfo.ShowAbout();
			return WhatToDoNext.AbandonProgram;
		}

		internal WhatToDoNext DoHelp()
		{
			ShowHelp(false);
			return WhatToDoNext.AbandonProgram;
		}

		internal WhatToDoNext DoHelp(List<ICommand> allCommands)
		{
			ShowHelp(allCommands);
			return WhatToDoNext.AbandonProgram;
		}

		internal WhatToDoNext DoHelp2()
		{
			ShowHelp(true);
			return WhatToDoNext.AbandonProgram;
		}

		internal WhatToDoNext DoUsage()
		{
			_assemblyInfo.ShowUsage(_list.Where(d => d.ShortForm != default(char)).Select(d => d.ShortForm));
			return WhatToDoNext.AbandonProgram;
		}

		internal void Reset()
		{
			_bannerAlreadyShown = false;
		}

		private readonly AssemblyInformation _assemblyInfo;

		private readonly OptionsContext _context;

		private readonly List<OptionDetails> _list = new List<OptionDetails>();

		private readonly object _optionBundle;

		private readonly bool _stopOnFirstNonOption;

		private MethodInfo _argumentProcessor = null;

		private ArrayList _arguments = new ArrayList();

		private ArrayList _argumentsTail = new ArrayList();

		private bool _bannerAlreadyShown = false;

		private bool _hasSecondLevelHelp = false;

		private static int IndexOfAny(string where, params char[] what)
		{
			return where.IndexOfAny(what);
		}

		private void AddArgumentProcessor(MemberInfo memberInfo)
		{
			if (_argumentProcessor != null)
				throw new NotSupportedException(_.Translate("More than one argument processor method found"));

			if ((memberInfo.MemberType == MemberTypes.Method && memberInfo is MethodInfo)) {
				if (((MethodInfo)memberInfo).ReturnType.FullName != typeof(void).FullName)
					throw new NotSupportedException(_.Translate("Argument processor method must return 'void'"));

				ParameterInfo[] parameters = ((MethodInfo)memberInfo).GetParameters();
				if ((parameters == null) || (parameters.Length != 1) || (parameters[0].ParameterType.FullName != typeof(string).FullName))
					throw new NotSupportedException(_.Translate("Argument processor method must have a string parameter"));

				_argumentProcessor = (MethodInfo)memberInfo;
			} else
				throw new NotSupportedException(_.Translate("Argument processor marked member isn't a method"));
		}

		private ArrayList ExpandResponseFiles(string[] args)
		{
			ArrayList result = new ArrayList();
			foreach (string arg in args)
				if (arg.StartsWith("@"))
					processResponseFile(arg.Substring(1), result);
				else
					result.Add(arg);
			return result;
		}

		private bool MaybeAnOption(string arg)
		{
			return ((_context.ParsingMode & OptionsParsingMode.Windows) > 0 && arg[0] == '/') ||
					((_context.ParsingMode & OptionsParsingMode.Linux) > 0 && arg[0] == '-');
		}

		private string[] NormalizeArgs(string[] args)
		{
			bool ParsingOptions = true;
			var result = new List<string>();

			foreach (string arg in ExpandResponseFiles(args)) {
				if (arg.Length > 0) {
					if (ParsingOptions) {
						if (_context.EndOptionProcessingWithDoubleDash && (arg == "--")) {
							ParsingOptions = false;
							continue;
						}

						if ((_context.ParsingMode & OptionsParsingMode.Linux) > 0 &&
							 arg[0] == '-' && arg.Length > 1 && arg[1] != '-' &&
							 _context.BreakSingleDashManyLettersIntoManyOptions) {
							foreach (char c in arg.Substring(1)) // many single-letter options
								result.Add("-" + c); // expand into individualized options
							continue;
						}

						if (MaybeAnOption(arg)) {
							int pos = IndexOfAny(arg, ':', '=');

							if (pos < 0)
								result.Add(arg);
							else {
								result.Add(arg.Substring(0, pos));
								result.Add(arg.Substring(pos + 1));
							}
							continue;
						}
					} else {
						_argumentsTail.Add(arg);
						continue;
					}

					// if nothing else matches then it get here
					result.Add(arg);
				}
			}

			return result.ToArray();
		}

		private void ProcessNonOption(string argument)
		{
			if (_argumentProcessor == null)
				_arguments.Add(argument);
			else
				_argumentProcessor.Invoke(_optionBundle, new object[] { argument });
		}

		private void processResponseFile(string filename, ArrayList result)
		{
			StringBuilder sb = new StringBuilder();
			string line;
			try {
				using (StreamReader responseFile = new StreamReader(filename)) {
					while ((line = responseFile.ReadLine()) != null)
						processResponseFileLine(line, result, sb);
					responseFile.Close();
				}
			} catch (FileNotFoundException) {
				_context.ReportError(2011, _.Translate("Unable to find response file '") + filename + "'");
			} catch (Exception exception) {
				_context.ReportError(2011, _.Translate("Unable to open response file '") + filename + "'. " + exception.Message);
			}
		}

		private void processResponseFileLine(string line, ArrayList result, StringBuilder sb)
		{
			int t = line.Length;
			for (int i = 0; i < t; i++) {
				char c = line[i];
				if (c == '"' || c == '\'') {
					char end = c;
					for (i++; i < t; i++) {
						c = line[i];
						if (c == end)
							break;
						sb.Append(c);
					}
				} else if (c == ' ') {
					if (sb.Length > 0) {
						result.Add(sb.ToString());
						sb.Length = 0;
					}
				} else {
					sb.Append(c);
				}
			}
			if (sb.Length > 0) {
				result.Add(sb.ToString());
				sb.Length = 0;
			}
		}

		private void ShowHelp(List<ICommand> allCommands)
		{
			_assemblyInfo.ShowTitleLines();
			Console.WriteLine(_assemblyInfo.Usage);
			Console.WriteLine(_.Translate("Commands:"));
			foreach (var command in allCommands) {
				Console.WriteLine("\t{0}\t{1}", command.Name.ToLowerInvariant(), command.Description);
			}
		}

		private void ShowHelp(bool showSecondLevelHelp)
		{
			_assemblyInfo.ShowTitleLines();
			Console.WriteLine(_assemblyInfo.Usage);
			Console.WriteLine(_.Translate("Options:"));
			ArrayList lines = new ArrayList(_list.Count);
			int tabSize = 0;
			foreach (OptionDetails option in _list)
				if (option.SecondLevelHelp == showSecondLevelHelp) {
					string[] optionLines = option.ToString().Split('\n');
					foreach (string line in optionLines) {
						int pos = line.IndexOf('\t');
						if (pos > tabSize)
							tabSize = pos;
						lines.Add(line);
					}
				}
			tabSize += 2;
			foreach (string line in lines) {
				string[] parts = line.Split('\t');
				Console.Write(parts[0].PadRight(tabSize));
				Console.WriteLine(parts[1]);
				if (parts.Length > 2) {
					string spacer = new string(' ', tabSize);
					for (int i = 2; i < parts.Length; i++) {
						Console.Write(spacer);
						Console.WriteLine(parts[i]);
					}
				}
			}
			_assemblyInfo.ShowFooter();
		}
	}
}