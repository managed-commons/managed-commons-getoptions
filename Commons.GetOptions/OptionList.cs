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
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Commons.GetOptions
{
	/// <summary>
	/// Option Parsing
	/// </summary>
	public class OptionList
	{
		public TranslatePlural PluralTranslator;
		public ErrorReporter ReportError;
		public Translate Translator;

		public OptionList(Options optionBundle)
		{
			if (optionBundle == null)
				throw new ArgumentNullException("optionBundle");

			Type optionsType = optionBundle.GetType();
			_optionBundle = optionBundle;
			_parsingMode = optionBundle.ParsingMode;
			_breakSingleDashManyLettersIntoManyOptions = optionBundle.BreakSingleDashManyLettersIntoManyOptions;
			_endOptionProcessingWithDoubleDash = optionBundle.EndOptionProcessingWithDoubleDash;
			this.ReportError = optionBundle.ReportError;

			ExtractEntryAssemblyInfo(optionsType);

			foreach (MemberInfo mi in optionsType.GetMembers()) {
				object[] attribs = mi.GetCustomAttributes(typeof(KillInheritedOptionAttribute), true);
				if (attribs == null || attribs.Length == 0) {
					attribs = mi.GetCustomAttributes(typeof(OptionAttribute), true);
					if (attribs != null && attribs.Length > 0) {
						OptionDetails option = new OptionDetails(mi, (OptionAttribute)attribs[0], optionBundle, translate);
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
		}

		public string AboutDetails
		{
			get
			{
				return translate(_appAboutDetails);
			}
		}

		public string Usage
		{
			get
			{
				string format = translate("Usage: {0} [options] {1}");
				return string.Format(format, _appExeName, translate(_appUsageComplement));
			}
		}

		public string[] ProcessArgs(string[] args)
		{
			string arg;
			string nextArg;
			bool OptionWasProcessed;

			_list.Sort();

			OptionDetails.LinkAlternatesInsideList(_list);

			args = NormalizeArgs(args);

			try {
				int argc = args.Length;
				for (int i = 0; i < argc; i++) {
					arg = args[i];
					if (i + 1 < argc)
						nextArg = args[i + 1];
					else
						nextArg = null;

					OptionWasProcessed = false;

					if (arg.Length > 1 && (arg.StartsWith("-") || arg.StartsWith("/"))) {
						foreach (OptionDetails option in _list) {
							OptionProcessingResult result = option.ProcessArgument(arg, nextArg);
							if (result != OptionProcessingResult.NotThisOption) {
								OptionWasProcessed = true;
								if (result == OptionProcessingResult.OptionConsumedParameter)
									i++;
								break;
							}
						}
					}

					if (!OptionWasProcessed)
						ProcessNonOption(arg);
				}

				foreach (OptionDetails option in _list)
					option.TransferValues();

				foreach (string argument in _argumentsTail)
					ProcessNonOption(argument);

				return (string[])_arguments.ToArray(typeof(string));
			} catch (Exception ex) {
				System.Console.WriteLine(ex.ToString());
				System.Environment.Exit(1);
			}

			return null;
		}

		public void ShowBanner()
		{
			if (!_bannerAlreadyShown) {
				Console.WriteLine(translate(_appTitle) + "  " + translate(_appVersion) + " - " + translate(_appCopyright));
				if (AdditionalBannerInfo != null)
					Console.WriteLine(AdditionalBannerInfo);
			}
			_bannerAlreadyShown = true;
		}

		internal string AdditionalBannerInfo;

		internal WhatToDoNext DoAbout()
		{
			ShowAbout();
			return WhatToDoNext.AbandonProgram;
		}

		internal WhatToDoNext DoHelp()
		{
			ShowHelp(false);
			return WhatToDoNext.AbandonProgram;
		}

		internal WhatToDoNext DoHelp2()
		{
			ShowHelp(true);
			return WhatToDoNext.AbandonProgram;
		}

		internal WhatToDoNext DoUsage()
		{
			ShowUsage();
			return WhatToDoNext.AbandonProgram;
		}

		internal bool MaybeAnOption(string arg)
		{
			return ((_parsingMode & OptionsParsingMode.Windows) > 0 && arg[0] == '/') ||
					((_parsingMode & OptionsParsingMode.Linux) > 0 && arg[0] == '-');
		}

		private string _appAboutDetails = "Add a [assembly: Commons.About(\"Here goes the short about details\")] to your assembly";
		private string _appAdditionalInfo = null;
		private string[] _appAuthors;
		private string _appCopyright = "Add a [assembly: AssemblyCopyright(\"(c)200n Here goes the copyright holder name\")] to your assembly";
		private string _appDescription = "Add a [assembly: AssemblyDescription(\"Here goes the short description\")] to your assembly";
		private string _appExeName;
		private string _appReportBugsTo = null;
		private string _appTitle = "Add a [assembly: AssemblyTitle(\"Here goes the application name\")] to your assembly";
		private string _appUsageComplement = "Add a [assembly: Commons.UsageComplement(\"Here goes the usage clause complement\")] to your assembly";
		private string _appVersion;
		private MethodInfo _argumentProcessor = null;
		private ArrayList _arguments = new ArrayList();
		private ArrayList _argumentsTail = new ArrayList();
		private bool _bannerAlreadyShown = false;
		private bool _breakSingleDashManyLettersIntoManyOptions;
		private bool _endOptionProcessingWithDoubleDash;
		private Assembly _entry;
		private bool _hasSecondLevelHelp = false;
		private ArrayList _list = new ArrayList();
		private Options _optionBundle = null;
		private OptionsParsingMode _parsingMode;
		private string _appLicense;

		private static int IndexOfAny(string where, params char[] what)
		{
			return where.IndexOfAny(what);
		}

		private void AddArgumentProcessor(MemberInfo memberInfo)
		{
			if (_argumentProcessor != null)
				throw new NotSupportedException(translate("More than one argument processor method found"));

			if ((memberInfo.MemberType == MemberTypes.Method && memberInfo is MethodInfo)) {
				if (((MethodInfo)memberInfo).ReturnType.FullName != typeof(void).FullName)
					throw new NotSupportedException(translate("Argument processor method must return 'void'"));

				ParameterInfo[] parameters = ((MethodInfo)memberInfo).GetParameters();
				if ((parameters == null) || (parameters.Length != 1) || (parameters[0].ParameterType.FullName != typeof(string).FullName))
					throw new NotSupportedException(translate("Argument processor method must have a string parameter"));

				_argumentProcessor = (MethodInfo)memberInfo;
			} else
				throw new NotSupportedException(translate("Argument processor marked member isn't a method"));
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

		private void ExtractEntryAssemblyInfo(Type optionsType)
		{
			_entry = optionsType.Assembly;
			if (_entry == this.GetType().Assembly) {
				_entry = Assembly.GetEntryAssembly();
			}

			_appExeName = _entry.GetName().Name;
			GetAssemblyAttributeValue(typeof(AssemblyInformationalVersionAttribute), "InformationalVersion", ref _appVersion);
			if (string.IsNullOrWhiteSpace(_appVersion))
				_appVersion = _entry.GetName().Version.ToString();
			GetAssemblyAttributeValue(typeof(AssemblyTitleAttribute), "Title", ref _appTitle);
			GetAssemblyAttributeValue(typeof(AssemblyCopyrightAttribute), "Copyright", ref _appCopyright);
			GetAssemblyAttributeValue(typeof(AssemblyDescriptionAttribute), "Description", ref _appDescription);
			GetAssemblyAttributeValue(typeof(Commons.AboutAttribute), ref _appAboutDetails);
			GetAssemblyAttributeValue(typeof(Commons.UsageComplementAttribute), ref _appUsageComplement);
			GetAssemblyAttributeValue(typeof(Commons.AdditionalInfoAttribute), ref _appAdditionalInfo);
			GetAssemblyAttributeValue(typeof(Commons.ReportBugsToAttribute), ref _appReportBugsTo);
			string authors = string.Empty;
			GetAssemblyAttributeValue(typeof(AssemblyCompanyAttribute), "Company", ref authors);
			_appLicense = GetAssemblyAttributeStrings(typeof(LicenseAttribute)).FirstOrDefault();
			if (string.IsNullOrWhiteSpace(authors)) {
				_appAuthors = new String[1];
				_appAuthors[0] = "Add one or more [assembly: AssemblyCompany(\"Here goes the authors names, separated by commas\")] to your assembly";
			} else
				_appAuthors = authors.Split(',');

		}

		private object[] GetAssemblyAttributes(Type type)
		{
			return _entry.GetCustomAttributes(type, false);
		}

		private string[] GetAssemblyAttributeStrings(Type type)
		{
			object[] result = GetAssemblyAttributes(type);

			if ((result == null) || (result.Length == 0))
				return new string[0];

			int i = 0;
			string[] var = new string[result.Length];

			foreach (object o in result)
				var[i++] = o.ToString();

			return var;
		}

		private void GetAssemblyAttributeValue(Type type, string propertyName, ref string var)
		{
			object[] result = GetAssemblyAttributes(type);

			if ((result != null) && (result.Length > 0))
				var = (string)type.InvokeMember(propertyName, BindingFlags.Public | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Instance, null, result[0], new object[] { }); ;
		}

		private void GetAssemblyAttributeValue(Type type, ref string var)
		{
			object[] result = GetAssemblyAttributes(type);

			if ((result != null) && (result.Length > 0))
				var = result[0].ToString();
		}

		private string[] NormalizeArgs(string[] args)
		{
			bool ParsingOptions = true;
			ArrayList result = new ArrayList();

			foreach (string arg in ExpandResponseFiles(args)) {
				if (arg.Length > 0) {
					if (ParsingOptions) {
						if (_endOptionProcessingWithDoubleDash && (arg == "--")) {
							ParsingOptions = false;
							continue;
						}

						if ((_parsingMode & OptionsParsingMode.Linux) > 0 &&
							 arg[0] == '-' && arg.Length > 1 && arg[1] != '-' &&
							 _breakSingleDashManyLettersIntoManyOptions) {
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

			return (string[])result.ToArray(typeof(string));
		}

		private void ProcessNonOption(string argument)
		{
			if (_optionBundle.VerboseParsingOfOptions)
				Console.WriteLine(translate("argument") + " [" + argument + "]");
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
				ReportError(2011, translate("Unable to find response file '") + filename + "'");
			} catch (Exception exception) {
				ReportError(2011, translate("Unable to open response file '") + filename + "'. " + exception.Message);
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

		private void ShowAbout()
		{
			ShowTitleLines();
			Console.WriteLine(translate(_appAboutDetails));
			Console.Write(translate("Authors: "));
			Console.WriteLine(string.Join(", ", _appAuthors));
		}

		private void ShowHelp(bool showSecondLevelHelp)
		{
			ShowTitleLines();
			Console.WriteLine(Usage);
			Console.WriteLine(translate("Options:"));
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
			if (_appAdditionalInfo != null)
				Console.WriteLine("\n{0}", translate(_appAdditionalInfo));
			if (_appReportBugsTo != null)
				Console.WriteLine(translate("\nPlease report bugs {0} <{1}>"),
								  (_appReportBugsTo.IndexOf('@') > 0) ? translate("to") : translate("at"),
								  translate(_appReportBugsTo));
		}

		private void ShowTitleLines()
		{
			ShowBanner();
			Console.WriteLine(translate(_appDescription));
			if (!string.IsNullOrWhiteSpace(_appLicense))
				Console.WriteLine("\r\n" + translate("License: ") + _appLicense);
			Console.WriteLine();
		}

		private void ShowUsage()
		{
			Console.WriteLine(Usage);
			Console.Write(translate("Short Options: "));
			foreach (OptionDetails option in _list)
				Console.Write(option.ShortForm.Trim());
			Console.WriteLine();
		}

		private string translate(string textToTranslate)
		{
			return (Translator == null) ? textToTranslate : Translator(textToTranslate);
		}

		private string translatePlural(string singular, string plural, int quantity)
		{
			return (PluralTranslator == null) ?
					(quantity == 1 ? singular : plural) :
					PluralTranslator(singular, plural, quantity);
		}
	}

	public delegate string Translate(string textToTranslate);

	public delegate string TranslatePlural(string singular, string plural, int quantity);
}