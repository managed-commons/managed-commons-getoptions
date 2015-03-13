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

		public OptionList(object optionBundle,
						   OptionsParsingMode mode = OptionsParsingMode.Both,
						   bool breakSingleDashManyLettersIntoManyOptions = false,
						   bool endOptionProcessingWithDoubleDash = true,
						   bool dontSplitOnCommas = false,
						   ErrorReporter errorReporter = null)
		{
			if (optionBundle == null)
				throw new ArgumentNullException("optionBundle");

			Type optionsType = optionBundle.GetType();
			this.optionBundle = optionBundle;
			this.parsingMode = mode;
			this.breakSingleDashManyLettersIntoManyOptions = breakSingleDashManyLettersIntoManyOptions;
			this.endOptionProcessingWithDoubleDash = endOptionProcessingWithDoubleDash;
			this.ReportError = errorReporter;

			ExtractEntryAssemblyInfo(optionsType);

			object[] classAttribs = optionsType.GetCustomAttributes(typeof(CommandProcessorAttribute), false);
			if (classAttribs != null && classAttribs.Length > 0) {
				var commandProcessor = (CommandProcessorAttribute)classAttribs[0];
				this.CommandName = commandProcessor.Name;
				this.CommandDescription = commandProcessor.Description;
			}

			foreach (MemberInfo mi in optionsType.GetMembers()) {
				object[] attribs = mi.GetCustomAttributes(typeof(KillInheritedOptionAttribute), true);
				if (attribs == null || attribs.Length == 0) {
					attribs = mi.GetCustomAttributes(typeof(OptionAttribute), true);
					if (attribs != null && attribs.Length > 0) {
						OptionDetails option = new OptionDetails(mi, (OptionAttribute)attribs[0], optionBundle, mode, dontSplitOnCommas, translate);
						list.Add(option);
						HasSecondLevelHelp = HasSecondLevelHelp || option.SecondLevelHelp;
					} else if (mi.DeclaringType == mi.ReflectedType) { // not inherited
						attribs = mi.GetCustomAttributes(typeof(ArgumentProcessorAttribute), true);
						if (attribs != null && attribs.Length > 0)
							AddArgumentProcessor(mi);
					}
				}
			}

			if (argumentProcessor == null) // try to find an inherited one
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
				return translate(appAboutDetails);
			}
		}

		public string Usage
		{
			get
			{
				string format = translate("Usage: {0} {2}[options] {1}");
				return string.Format(format, appExeName, translate(appUsageComplement), translate(paddedCommandName()));
			}
		}

		public string[] ProcessArgs(IEnumerable<string> theArgs)
		{
			string[] args = theArgs.ToArray();
			string arg;
			string nextArg;
			bool OptionWasProcessed;

			list.Sort();

			OptionDetails.LinkAlternatesInsideList(list);

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
						foreach (OptionDetails option in list) {
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

				foreach (OptionDetails option in list)
					option.TransferValues();

				foreach (string argument in argumentsTail)
					ProcessNonOption(argument);

				return (string[])arguments.ToArray(typeof(string));
			} catch (Exception ex) {
				System.Console.WriteLine(ex.ToString());
				System.Environment.Exit(1);
			}

			return null;
		}

		public void ShowBanner()
		{
			if (!bannerAlreadyShown) {
				Console.WriteLine(translate(appTitle) + " " + translate(appVersion) + " - " + translate(appCopyright));
				if (appLicenseDetails != null)
					Console.WriteLine("-- " + appLicenseDetails);
				if (AdditionalBannerInfo != null)
					Console.WriteLine(AdditionalBannerInfo);
			}
			bannerAlreadyShown = true;
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

		internal WhatToDoNext DoHelp(List<ICommand> commands)
		{
			ShowHelp(commands);
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
			return ((parsingMode & OptionsParsingMode.Windows) > 0 && arg[0] == '/') ||
			((parsingMode & OptionsParsingMode.Linux) > 0 && arg[0] == '-');
		}

		private string appAboutDetails = null;
		private string appAdditionalInfo = null;
		private IList<string> appAuthors = new List<string>();
		private string appCopyright = "Add a [assembly: AssemblyCopyright(\"(c)200n Here goes the copyright holder name\")] to your assembly";
		private string appDescription = "Add a [assembly: AssemblyDescription(\"Here goes the short description\")] to your assembly";
		private string appExeName;
		private string appLicenseDetails = null;
		private string appReportBugsTo = null;
		private string appTitle = "Add a [assembly: AssemblyTitle(\"Here goes the application name\")] to your assembly";
		private string appUsageComplement = null;
		private string appVersion;
		private MethodInfo argumentProcessor = null;
		private ArrayList arguments = new ArrayList();
		private ArrayList argumentsTail = new ArrayList();
		private bool bannerAlreadyShown = false;
		private bool breakSingleDashManyLettersIntoManyOptions;
		private string CommandDescription;
		private string CommandName;
		private bool endOptionProcessingWithDoubleDash;
		private Assembly entry;
		private bool HasSecondLevelHelp = false;
		private ArrayList list = new ArrayList();
		private object optionBundle = null;
		private OptionsParsingMode parsingMode;

		private static int IndexOfAny(string where, params char[] what)
		{
			return where.IndexOfAny(what);
		}

		private void AddArgumentProcessor(MemberInfo memberInfo)
		{
			if (argumentProcessor != null)
				throw new NotSupportedException(translate("More than one argument processor method found"));

			if ((memberInfo.MemberType == MemberTypes.Method && memberInfo is MethodInfo)) {
				if (((MethodInfo)memberInfo).ReturnType.FullName != typeof(void).FullName)
					throw new NotSupportedException(translate("Argument processor method must return 'void'"));

				ParameterInfo[] parameters = ((MethodInfo)memberInfo).GetParameters();
				if ((parameters == null) || (parameters.Length != 1) || (parameters[0].ParameterType.FullName != typeof(string).FullName))
					throw new NotSupportedException(translate("Argument processor method must have a string parameter"));

				argumentProcessor = (MethodInfo)memberInfo;
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
			entry = optionsType.Assembly;
			if (entry == this.GetType().Assembly) {
				entry = Assembly.GetEntryAssembly();
			}

			appExeName = entry.GetName().Name;
			GetAssemblyAttributeValue(typeof(AssemblyInformationalVersionAttribute), "InformationalVersion", ref appVersion);
			if (string.IsNullOrEmpty(appVersion)) {
				appVersion = entry.GetName().Version.ToString();
			}
			GetAssemblyAttributeValue(typeof(AssemblyTitleAttribute), "Title", ref appTitle);
			GetAssemblyAttributeValue(typeof(AssemblyCopyrightAttribute), "Copyright", ref appCopyright);
			GetAssemblyAttributeValue(typeof(AssemblyDescriptionAttribute), "Description", ref appDescription);
			GetAssemblyAttributeValue(typeof(Commons.LicenseAttribute), ref appLicenseDetails);
			GetAssemblyAttributeValue(typeof(Commons.AboutAttribute), ref appAboutDetails);
			GetAssemblyAttributeValue(typeof(Commons.UsageComplementAttribute), ref appUsageComplement);
			GetAssemblyAttributeValue(typeof(Commons.AdditionalInfoAttribute), ref appAdditionalInfo);
			GetAssemblyAttributeValue(typeof(Commons.ReportBugsToAttribute), ref appReportBugsTo);
			var company = "";
			GetAssemblyAttributeValue(typeof(AssemblyCompanyAttribute), "Company", ref company);
			if (string.IsNullOrEmpty(company)) {
				appAuthors.Add("Add one [assembly: AssemblyCompany(\"Here goes the comma-separated list of author names\")] to your assembly");
			} else {
				foreach (var author in company.Split(',').Select(s => s.Trim()))
					appAuthors.Add(author);
			}
		}

		private object[] GetAssemblyAttributes(Type type)
		{
			return entry.GetCustomAttributes(type, false);
		}

		private void GetAssemblyAttributeStrings(Type type, IList<string> list)
		{
			object[] result = GetAssemblyAttributes(type);

			if ((result == null) || (result.Length == 0))
				return;

			foreach (object o in result)
				list.Add(o.ToString());
		}

		private void GetAssemblyAttributeValue(Type type, string propertyName, ref string var)
		{
			object[] result = GetAssemblyAttributes(type);

			if ((result != null) && (result.Length > 0))
				var = (string)type.InvokeMember(propertyName, BindingFlags.Public | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Instance, null, result[0], new object[] { });
			;
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
						if (endOptionProcessingWithDoubleDash && (arg == "--")) {
							ParsingOptions = false;
							continue;
						}

						if ((parsingMode & OptionsParsingMode.Linux) > 0 &&
							arg[0] == '-' && arg.Length > 1 && arg[1] != '-' &&
							breakSingleDashManyLettersIntoManyOptions) {
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
						argumentsTail.Add(arg);
						continue;
					}

					// if nothing else matches then it get here
					result.Add(arg);
				}
			}

			return (string[])result.ToArray(typeof(string));
		}

		private string paddedCommandName()
		{
			if (string.IsNullOrEmpty(CommandName))
				return string.Empty;
			return CommandName + " ";
		}

		private void ProcessNonOption(string argument)
		{
			if (argumentProcessor == null)
				arguments.Add(argument);
			else
				argumentProcessor.Invoke(optionBundle, new object[] { argument });
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
			if (!string.IsNullOrEmpty(appAboutDetails))
				Console.WriteLine(translate(appAboutDetails));
			Console.Write(translate("Authors: "));
			Console.WriteLine(string.Join(", ", appAuthors.ToArray()));
		}

		private void ShowHelp(List<ICommand> commands)
		{
			ShowTitleLines();
			Console.WriteLine(Usage);
			int tabSize = 0;
			foreach (var command in commands) {
				int pos = command.Name.Length;
				if (pos > tabSize)
					tabSize = pos;
			}
			tabSize += 2;
			foreach (var command in commands) {
				Console.Write(command.Name.PadRight(tabSize));
				string[] parts = command.Description.Split('\n');
				Console.WriteLine(parts[0]);
				if (parts.Length > 1) {
					string spacer = new string(' ', tabSize);
					for (int i = 1; i < parts.Length; i++) {
						Console.Write(spacer);
						Console.WriteLine(parts[i]);
					}
				}
			}
		}

		private void ShowHelp(bool showSecondLevelHelp)
		{
			ShowTitleLines();
			Console.WriteLine(Usage);
			Console.WriteLine(translate("Options:"));
			ArrayList lines = new ArrayList(list.Count);
			int tabSize = 0;
			foreach (OptionDetails option in list)
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
			if (appAdditionalInfo != null)
				Console.WriteLine("\n{0}", translate(appAdditionalInfo));
			if (appReportBugsTo != null)
				Console.WriteLine(translate("\nPlease report bugs {0} <{1}>"),
					(appReportBugsTo.IndexOf('@') > 0) ? translate("to") : translate("at"),
					translate(appReportBugsTo));
		}

		private void ShowTitleLines()
		{
			ShowBanner();
			Console.WriteLine(translate(appDescription));
			Console.WriteLine();
		}

		private void ShowUsage()
		{
			Console.WriteLine(Usage);
			Console.Write(translate("Short Options: "));
			foreach (OptionDetails option in list)
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