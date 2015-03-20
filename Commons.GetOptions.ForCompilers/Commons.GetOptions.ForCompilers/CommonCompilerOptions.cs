// Commons.GetOptions.ForCompilers
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
using System.Console;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Commons.GetOptions;

namespace Commons.Compilers
{
	public delegate void AssemblyAdder(Assembly loadedAssembly);

	public delegate void ModuleAdder(System.Reflection.Module module);

	public enum InternalCompilerErrorReportAction
	{
		prompt, send, none
	}

	public enum TargetType
	{
		Library, Exe, Module, WinExe
	};

	public struct FileToCompile
	{
		public Encoding Encoding;
		public string Filename;

		public FileToCompile(string filename, Encoding encoding)
		{
			this.Filename = filename;
			this.Encoding = encoding;
		}
	}

	public class CommonCompilerOptions : OptionsWithSecondLevel
	{
		[Option("Allows unsafe code", Name = "unsafe", SecondLevelHelp = true)]
		public bool AllowUnsafeCode = false;

		public ArrayList AssembliesToReference = new ArrayList();

		// TODO: force option to accept number in hex format
		//		[Option("[NOT IMPLEMENTED YET]The base {address} for a library or module (hex)", SecondLevelHelp = true)]
		public int baseaddress;

		public bool CheckedContext = true;

		// support for the Compact Framework
		//------------------------------------------------------------------
		//		[Option("[NOT IMPLEMENTED YET]Sets the compiler to TargetFileType the Compact Framework", Name = "netcf")]
		public bool CompileForCompactFramework = false;

		//		[Option("[NOT IMPLEMENTED YET]Create bug report {file}", Name = "bugreport")]
		public string CreateBugReport;

		public ArrayList DebugListOfArguments = new ArrayList();

		public bool DebugSymbolsDbOnly = false;

		// Defines
		//------------------------------------------------------------------
		public Hashtable Defines = new Hashtable();

		// Signing options
		//------------------------------------------------------------------
		//		[Option("[NOT IMPLEMENTED YET]Delay-sign the assembly using only the public portion of the strong name key", VBCStyleBoolean = true)]
		public bool delaysign;

		[Option("Do not display compiler copyright banner", Name = "nologo")]
		public bool DontShowBanner = false;

		// resource options
		//------------------------------------------------------------------
		public ArrayList EmbeddedResources = new ArrayList();

		public bool FullDebugging = true;

		public ArrayList Imports = new ArrayList();

		//		[Option("[NOT IMPLEMENTED YET]Specifies a strong name key {container}")]
		public string keycontainer;

		//		[Option("[NOT IMPLEMENTED YET]Specifies a strong name key {file}")]
		public string keyfile;

		public ArrayList LinkedResources = new ArrayList();

		[Option("Specifies the {name} of the Class or Module that contains Sub Main \tor inherits from System.Windows.Forms.Form.\tNeeded to select among many entry-points for a program (target=exe|winexe)", ShortForm = 'm', Name = "main")]
		public string MainClassName = null;

		public ArrayList NetModulesToAdd = new ArrayList();

		[Option("Disables implicit references to assemblies", Name = "noconfig", SecondLevelHelp = true)]
		public bool NoConfig = false;

		[Option("Don\'t assume the standard library", Name = "nostdlib", SecondLevelHelp = true)]
		public bool NoStandardLibraries = false;

		//		[Option("[NOT IMPLEMENTED YET]Enable optimizations", Name = "optimize", VBCStyleBoolean = true)]
		public bool Optimize = false;

		[Option("[IGNORED] Emit compiler output in UTF8 character encoding", Name = "utf8output", SecondLevelHelp = true, VBCStyleBoolean = true)]
		public bool OutputInUTF8;

		public ArrayList PathsToSearchForLibraries = new ArrayList();

		[Option("Specifies the root {namespace} for all type declarations", Name = "rootnamespace", SecondLevelHelp = true)]
		public string RootNamespace = null;

		//		[Option("[NOT IMPLEMENTED YET]Specifies the {path} to the location of mscorlib.dll and microsoft.visualbasic.dll", Name = "sdkpath")]
		public string SDKPath = null;

		public ArrayList SourceFilesToCompile = new ArrayList();

		// Compiler output options
		//------------------------------------------------------------------
		//TODO: Correct semantics
		[Option("Commands the compiler to show only error messages for syntax-related errors and warnings", ShortForm = 'q', Name = "quiet", SecondLevelHelp = true)]
		public bool SuccintErrorDisplay = false;

		// Output file options
		//------------------------------------------------------------------
		public TargetType TargetFileType = TargetType.Exe;

		[Option("Display verbose messages", ShortForm = 'v', Name = "verbose", SecondLevelHelp = true)]
		public bool Verbose = false;

		[Option("Emit full debugging information", ShortForm = 'g', Name = "debug", VBCStyleBoolean = true)]
		public bool WantDebuggingSupport = false;

		[Option("Sets warning {level} (the highest is 4, the default)", Name = "wlevel", SecondLevelHelp = true)]
		public int WarningLevel = 4;

		[Option("Treat warnings as errors", Name = "warnaserror", SecondLevelHelp = true)]
		public bool WarningsAreErrors = false;

		public ArrayList Win32Icons = new ArrayList();

		public ArrayList Win32Resources = new ArrayList();

		public CommonCompilerOptions(string[] args = null, ErrorReporter reportError = null)
			: base(BuildDefaultContext(reportError), args)
		{
			PathsToSearchForLibraries.Add(Directory.GetCurrentDirectory());
		}

		[Option("List of directories to search for referenced assemblies. \t{path-list}:path,...", Name = "libpath", AlternateForm = "lib")]
		public string AddedLibPath { set { foreach (string path in value.Split(',')) PathsToSearchForLibraries.Add(path); } }

		[Option("Adds the specified file as a linked assembly resource. \t{details}:file[,id[,public|private]]", MaxOccurs = -1, Name = "linkresource", AlternateForm = "linkres")]
		public string AddedLinkresource { set { LinkedResources.Add(value); } }

		// input file options
		//------------------------------------------------------------------
		[Option("Imports all type information from files in the module-list. {module-list}:module,...", MaxOccurs = -1, Name = "addmodule")]
		public string AddedModule { set { foreach (string module in value.Split(',')) NetModulesToAdd.Add(module); } }

		[Option("References metadata from the specified assembly-list. \t{assembly-list}:assembly,...", MaxOccurs = -1, ShortForm = 'r', Name = "reference")]
		public string AddedReference { set { foreach (string assembly in value.Split(',')) AssembliesToReference.Add(assembly); } }

		//TODO: support -res:file[,id[,public|private]] what depends on changes at Mono.GetOptions
		[Option("Adds the specified file as an embedded assembly resource. \t{details}:file[,id[,public|private]]", MaxOccurs = -1, Name = "resource", AlternateForm = "res")]
		public string AddedResource { set { EmbeddedResources.Add(value); } }

		//		[Option("[NOT IMPLEMENTED YET]Specifies a Win32 icon {file} (.ico) for the default Win32 resources",
		//	MaxOccurs = -1, Name = "win32icon")]
		public string AddedWin32icon { set { Win32Icons.Add(value); } }

		//		[Option("[NOT IMPLEMENTED YET]Specifies a Win32 resource {file} (.res)",
		//	MaxOccurs = -1, Name = "win32resource")]
		public string AddedWin32resource { set { Win32Resources.Add(value); } }

		public virtual string[] AssembliesToReferenceSoftly
		{
			get
			{
				// For now the "default config" is hardcoded we can move this outside later
				return new string[] { "System", "System.Data", "System.Xml" };
			}
		}

		public bool BeQuiet { get { return DontShowBanner || SuccintErrorDisplay; } }

		[Option("Select codepage by {ID} (number, 'utf8' or 'reset') to process following source files", MaxOccurs = -1, Name = "codepage")]
		public string CurrentCodepage
		{
			set
			{
				switch (value.ToLower()) {
					case "reset":
						_currentEncoding = null;
						break;

					case "utf8":
					case "utf-8":
						_currentEncoding = Encoding.UTF8;
						break;

					default:
						try {
							_currentEncoding = Encoding.GetEncoding(int.Parse(value));
						} catch (NotSupportedException) {
							Context.ReportError(0, string.Format("Ignoring unsupported codepage number {0}.", value));
						} catch (Exception) {
							Context.ReportError(0, string.Format("Ignoring unsupported codepage ID {0}.", value));
						}
						break;
				}
			}
		}

		[Option("Emit full debugging information (default)", Name = "debug:full", SecondLevelHelp = true)]
		public bool debugfull
		{
			set
			{
				WantDebuggingSupport = value;
				FullDebugging = value;
				DebugSymbolsDbOnly = !value;
			}
		}

		[Option("Emit debug symbols file only", Name = "debug:pdbonly", SecondLevelHelp = true)]
		public bool debugpdbonly
		{
			set
			{
				WantDebuggingSupport = value;
				FullDebugging = !value;
				DebugSymbolsDbOnly = value;
			}
		}

		[Option("Declares global conditional compilation symbol(s). {symbol-list}:name=value,...", MaxOccurs = -1, ShortForm = 'd', Name = "define")]
		public string DefineSymbol
		{
			set
			{
				foreach (string item in value.Split(',')) {
					string[] dados = item.Split('=');
					if (dados.Length > 1)
						Defines.Add(dados[0], dados[1]);
					else
						Defines.Add(dados[0], "true");
				}
			}
		}

		[Option("Declare global Imports for listed namespaces. {import-list}:namespace,...", MaxOccurs = -1, Name = "imports")]
		public string ImportNamespaces
		{
			set
			{
				foreach (string importedNamespace in value.Split(','))
					Imports.Add(importedNamespace);
			}
		}

		public virtual bool NothingToCompile
		{
			get
			{
				if (SourceFilesToCompile.Count == 0) {
					if (!BeQuiet)
						DoHelp();
					return true;
				}
				if (!BeQuiet)
					ShowBanner();
				return false;
			}
		}

		// errors and warnings options
		//------------------------------------------------------------------
		[Option("Disable warnings", Name = "nowarn", SecondLevelHelp = true)]
		public bool NoWarnings { set { if (value) WarningLevel = 0; } }

		[Option("Specifies the output {file} name", ShortForm = 'o', Name = "out")]
		public string OutputFileName
		{
			set
			{
				_outputFileName = value;
			}
			get
			{
				if (_outputFileName == null) {
					int pos = _firstSourceFile.LastIndexOf(".");

					if (pos > 0)
						_outputFileName = _firstSourceFile.Substring(0, pos);
					else
						_outputFileName = _firstSourceFile;
					// TODO: what Codegen does here to get hid of this dependency
					//					string bname = CodeGen.Basename(outputFileName);
					//					if (bname.IndexOf(".") == -1)
					_outputFileName += _targetFileExtension;
				}
				return _outputFileName;
			}
		}

		[Option("Displays time stamps of various compiler events", Name = "timestamp", SecondLevelHelp = true)]
		public virtual bool PrintTimeStamps
		{
			set
			{
				_printTimeStamps = true;
				_last_time = DateTime.Now;
				DebugListOfArguments.Add("timestamp");
			}
		}

		// code generation options
		//------------------------------------------------------------------
		[Option("Remove integer checks. Default off.", SecondLevelHelp = true, VBCStyleBoolean = true)]
		public virtual bool removeintchecks { set { CheckedContext = !value; } }

		public int[] WarningsToIgnore { get { return (int[])_warningsToIgnore.ToArray(typeof(int)); } }

		public static OptionsContext BuildDefaultContext(ErrorReporter reportError)
		{
			return new OptionsContext()
			{
				BreakSingleDashManyLettersIntoManyOptions = false,
				DontSplitOnCommas = true,
				EndOptionProcessingWithDoubleDash = true,
				ParsingMode = OptionsParsingMode.Both,
				ReportError = reportError ?? OptionsContext.DefaultErrorReporter
			};
		}

		public void AdjustCodegenWhenTargetIsNetModule(AssemblyBuilder assemblyBuilder)
		{
			if (TargetFileType == TargetType.Module) {
				StartTime("Adjusting AssemblyBuilder for NetModule target");
				PropertyInfo module_only = typeof(AssemblyBuilder).GetProperty("IsModuleOnly", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (module_only == null)
					UnsupportedFeatureOnthisRuntime("/target:module");

				MethodInfo set_method = module_only.GetSetMethod(true);
				set_method.Invoke(assemblyBuilder, BindingFlags.Default, null, new object[] { true }, null);
				ShowTime("   Done");
			}
		}

		[ArgumentProcessor]
		public void DefaultArgumentProcessor(string fileName)
		{
			if (_firstSourceFile == null)
				_firstSourceFile = fileName;

			if (!_sourceFiles.Contains(fileName)) {
				SourceFilesToCompile.Add(new FileToCompile(fileName, _currentEncoding));
				_sourceFiles.Add(fileName, fileName);
			}
		}

		public void EmbedResources(AssemblyBuilder builder)
		{
			if (EmbeddedResources != null)
				foreach (string file in EmbeddedResources)
					builder.AddResourceFile(file, file); // TODO: deal with resource IDs
		}

		public bool LoadAddedNetModules(AssemblyBuilder assemblyBuilder, ModuleAdder adder)
		{
			int errors = 0;

			if (NetModulesToAdd.Count > 0) {
				StartTime("Loading added netmodules");

				MethodInfo adder_method = typeof(AssemblyBuilder).GetMethod("AddModule", BindingFlags.Instance | BindingFlags.NonPublic);
				if (adder_method == null)
					UnsupportedFeatureOnthisRuntime("/addmodule");

				foreach (string module in NetModulesToAdd)
					LoadModule(adder_method, assemblyBuilder, adder, module, ref errors);

				ShowTime("   Done");
			}

			return errors == 0;
		}

		/// <summary>
		///   Loads all assemblies referenced on the command line
		/// </summary>
		public bool LoadReferencedAssemblies(AssemblyAdder adder)
		{
			StartTime("Loading referenced assemblies");

			int errors = 0;
			int soft_errors = 0;

			// Load Core Library for default compilation
			if (!NoStandardLibraries)
				LoadAssembly(adder, "mscorlib", ref errors, false);

			foreach (string r in AssembliesToReference)
				LoadAssembly(adder, r, ref errors, false);

			if (!NoConfig)
				foreach (string r in AssembliesToReferenceSoftly)
					if (!(AssembliesToReference.Contains(r) || AssembliesToReference.Contains(r + ".dll")))
						LoadAssembly(adder, r, ref soft_errors, true);

			ShowTime("References loaded");
			return errors == 0;
		}

		//		[Option("[NOT IMPLEMENTED YET]Include all files in the current directory and subdirectories according to the {wildcard}", Name = "recurse")]
		public WhatToDoNext Recurse(string wildcard)
		{
			//AddFiles (DirName, true); // TODO wrong semantics
			return WhatToDoNext.GoAhead;
		}

		public bool ReferencePackage(string packageName)
		{
			if (packageName == "") {
				DoAbout();
				return false;
			}

			ProcessStartInfo pi = new ProcessStartInfo();
			pi.FileName = "pkg-config";
			pi.RedirectStandardOutput = true;
			pi.UseShellExecute = false;
			pi.Arguments = "--libs " + packageName;
			Process p = null;
			try {
				p = Process.Start(pi);
			} catch (Exception e) {
				Context.ReportError(0, "Couldn't run pkg-config: " + e.Message);
				return false;
			}

			if (p.StandardOutput == null) {
				Context.ReportError(0, "Specified package did not return any information");
			}
			string pkgout = p.StandardOutput.ReadToEnd();
			p.WaitForExit();
			if (p.ExitCode != 0) {
				Context.ReportError(0, "Error running pkg-config. Check the above output.");
				return false;
			}
			p.Close();

			if (pkgout != null) {
				string[] xargs = pkgout.Trim(new Char[] { ' ', '\n', '\r', '\t' }).
					Split(new Char[] { ' ', '\t' });
				foreach (string arg in xargs) {
					string[] zargs = arg.Split(':', '=');
					try {
						if (zargs.Length > 1)
							AddedReference = zargs[1];
						else
							AddedReference = arg;
					} catch (Exception e) {
						Context.ReportError(0, "Something wrong with argument (" + arg + ") in 'pkg-config --libs' output: " + e.Message);
						return false;
					}
				}
			}

			return true;
		}

		[Option("References packages listed. {packagelist}=package,...", MaxOccurs = -1, Name = "pkg")]
		public WhatToDoNext ReferenceSomePackage(string packageName)
		{
			return ReferencePackage(packageName) ? WhatToDoNext.GoAhead : WhatToDoNext.AbandonProgram;
		}

		[Option("Debugger {arguments}", Name = "debug-args", SecondLevelHelp = true)]
		public WhatToDoNext SetDebugArgs(string args)
		{
			DebugListOfArguments.AddRange(args.Split(','));
			return WhatToDoNext.GoAhead;
		}

		[Option("Ignores warning number {XXXX}", MaxOccurs = -1, Name = "ignorewarn", SecondLevelHelp = true)]
		public WhatToDoNext SetIgnoreWarning(int warningNumber)
		{
			_warningsToIgnore.Add(warningNumber);
			return WhatToDoNext.GoAhead;
		}

		[Option("Specifies the target {type} for the output file (exe [default], winexe, library, module)", ShortForm = 't', Name = "target")]
		public WhatToDoNext SetTarget(string type)
		{
			switch (type.ToLower()) {
				case "library":
					TargetFileType = TargetType.Library;
					_targetFileExtension = ".dll";
					break;

				case "exe":
					TargetFileType = TargetType.Exe;
					_targetFileExtension = ".exe";
					break;

				case "winexe":
					TargetFileType = TargetType.WinExe;
					_targetFileExtension = ".exe";
					break;

				case "module":
					TargetFileType = TargetType.Module;
					_targetFileExtension = ".netmodule";
					break;
			}
			return WhatToDoNext.GoAhead;
		}

		public void ShowTime(string msg)
		{
			if (!_printTimeStamps)
				return;

			DateTime now = DateTime.Now;
			TimeSpan span = now - _last_time;
			_last_time = now;

			WriteLine(
				"[{0:00}:{1:000}] {2}",
				(int)span.TotalSeconds, span.Milliseconds, msg);
		}

		public void StartTime(string msg)
		{
			if (!_printTimeStamps)
				return;

			_last_time = DateTime.Now;

			WriteLine("[*] {0}", msg);
		}

		public void UnsupportedFeatureOnthisRuntime(string feature)
		{
			Context.ReportError(0, string.Format("Cannot use {0} on this runtime: Try the Mono runtime instead.", feature));
			Environment.Exit(1);
		}

		private Encoding _currentEncoding = null;
		private string _firstSourceFile = null;

		//
		// Last time we took the time
		//
		private DateTime _last_time;

		private string _outputFileName = null;
		private bool _printTimeStamps = false;
		private Hashtable _sourceFiles = new Hashtable();
		private string _targetFileExtension = ".exe";
		private ArrayList _warningsToIgnore = new ArrayList();

		private bool AddFiles(string spec, bool recurse)
		{
			string path, pattern;

			SplitPathAndPattern(spec, out path, out pattern);
			if (pattern.IndexOf("*") == -1) {
				DefaultArgumentProcessor(spec);
				return true;
			}

			string[] files = null;
			try {
				files = Directory.GetFiles(path, pattern);
			} catch (System.IO.DirectoryNotFoundException) {
				Context.ReportError(2001, "Source file '" + spec + "' could not be found");
				return false;
			} catch (System.IO.IOException) {
				Context.ReportError(2001, "Source file '" + spec + "' could not be found");
				return false;
			}
			foreach (string f in files)
				DefaultArgumentProcessor(f);

			if (!recurse)
				return true;

			string[] dirs = null;

			try {
				dirs = Directory.GetDirectories(path);
			} catch {
			}

			foreach (string d in dirs) {
				// Don't include path in this string, as each
				// directory entry already does
				AddFiles(d + "/" + pattern, true);
			}

			return true;
		}

		private void LoadAssembly(AssemblyAdder adder, string assemblyName, ref int errors, bool soft)
		{
			Assembly a = null;
			string total_log = "";

			try {
				char[] path_chars = { '/', '\\' };

				if (assemblyName.IndexOfAny(path_chars) != -1)
					a = Assembly.LoadFrom(assemblyName);
				else {
					string ass = assemblyName;
					if (ass.EndsWith(".dll"))
						ass = assemblyName.Substring(0, assemblyName.Length - 4);
					a = Assembly.Load(ass);
				}
				adder(a);
				return;
			} catch (FileNotFoundException) {
				if (PathsToSearchForLibraries != null) {
					foreach (string dir in PathsToSearchForLibraries) {
						string full_path = Path.Combine(dir, assemblyName + ".dll");

						try {
							a = Assembly.LoadFrom(full_path);
							adder(a);
							return;
						} catch (FileNotFoundException ff) {
							total_log += ff.FusionLog;
							continue;
						}
					}
				}
				if (soft)
					return;

				Context.ReportError(6, "Can not find assembly '" + assemblyName + "'\nLog: " + total_log);
			} catch (BadImageFormatException f) {
				Context.ReportError(6, "Bad file format while loading assembly\nLog: " + f.FusionLog);
			} catch (FileLoadException f) {
				Context.ReportError(6, "File Load Exception: " + assemblyName + "\nLog: " + f.FusionLog);
			} catch (ArgumentNullException) {
				Context.ReportError(6, "Argument Null exception");
			}

			errors++;
		}

		private void LoadModule(MethodInfo adder_method, AssemblyBuilder assemblyBuilder, ModuleAdder adder, string module, ref int errors)
		{
			System.Reflection.Module m;
			string total_log = "";

			try {
				try {
					m = (System.Reflection.Module)adder_method.Invoke(assemblyBuilder, new object[] { module });
				} catch (TargetInvocationException ex) {
					throw ex.InnerException;
				}
				adder(m);
			} catch (FileNotFoundException) {
				foreach (string dir in PathsToSearchForLibraries) {
					string full_path = Path.Combine(dir, module);
					if (!module.EndsWith(".netmodule"))
						full_path += ".netmodule";

					try {
						try {
							m = (System.Reflection.Module)adder_method.Invoke(assemblyBuilder, new object[] { full_path });
						} catch (TargetInvocationException ex) {
							throw ex.InnerException;
						}
						adder(m);
						return;
					} catch (FileNotFoundException ff) {
						total_log += ff.FusionLog;
						continue;
					}
				}
				Context.ReportError(6, "Cannot find module `" + module + "'");
				WriteLine("Log: \n" + total_log);
			} catch (BadImageFormatException f) {
				Context.ReportError(6, "Cannot load module (bad file format)" + f.FusionLog);
			} catch (FileLoadException f) {
				Context.ReportError(6, "Cannot load module " + f.FusionLog);
			} catch (ArgumentNullException) {
				Context.ReportError(6, "Cannot load module (null argument)");
			}
			errors++;
		}

		//
		// Given a path specification, splits the path from the file/pattern
		//
		private void SplitPathAndPattern(string spec, out string path, out string pattern)
		{
			int p = spec.LastIndexOf("/");
			if (p != -1) {
				//
				// Windows does not like /file.cs, switch that to:
				// "\", "file.cs"
				//
				if (p == 0) {
					path = "\\";
					pattern = spec.Substring(1);
				} else {
					path = spec.Substring(0, p);
					pattern = spec.Substring(p + 1);
				}
				return;
			}

			p = spec.LastIndexOf("\\");
			if (p != -1) {
				path = spec.Substring(0, p);
				pattern = spec.Substring(p + 1);
				return;
			}

			path = ".";
			pattern = spec;
		}
	}

	public class CommonCompilerOptions2 : CommonCompilerOptions
	{
		[Option("Filealign internal blocks to the {blocksize} in bytes. Valid values are 512, 1024, 2048, 4096, and 8192.", Name = "filealign", SecondLevelHelp = true)]
		public int FileAlignBlockSize = 0;

		[Option("Generate documentation from xml commments.", Name = "doc", SecondLevelHelp = true, VBCStyleBoolean = true)]
		public bool GenerateXmlDocumentation = false;

		// 0 means use appropriate (not fixed) default
		[Option("Generate documentation from xml commments to an specific {file}.", Name = "docto", SecondLevelHelp = true)]
		public string GenerateXmlDocumentationToFileName = null;

		[Option("What {action} (prompt | send | none) should be done when an internal compiler error occurs.\tThe default is none what just prints the error data in the compiler output", Name = "errorreport", SecondLevelHelp = true)]
		public InternalCompilerErrorReportAction HowToReportErrors = InternalCompilerErrorReportAction.none;

		[Option("Specify target CPU platform {ID}. ID can be x86, Itanium, x64 (AMD 64bit) or anycpu (the default).", Name = "platform", SecondLevelHelp = true)]
		public string TargetPlatform;
	}
}