//
// mcat.cs: Something similar to cat to exemplify using
//          Commons.GetOptions
//
// Author: Rafael Teixeira (monoman@gmail.com)
//
// (C) 2005 Rafael Teixeira
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
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;

using Commons.GetOptions;

/* our source of inspiration

cat --help gives:

Usage: cat [OPTION] [FILE]...
Concatenate FILE(s), or standard input, to standard output.

  -A, --show-all           equivalent to -vET
  -b, --number-nonblank    number nonblank output lines
  -e                       equivalent to -vE
  -E, --show-ends          display $ at end of each line
  -n, --number             number all output lines
  -s, --squeeze-blank      never more than one single blank line
  -t                       equivalent to -vT
  -T, --show-tabs          display TAB characters as ^I
  -u                       (ignored)
  -v, --show-nonprinting   use ^ and M- notation, except for LFD and TAB
      --help     display this help and exit
      --version  output version information and exit

With no FILE, or when FILE is -, read standard input.

Report bugs to <bug-coreutils@gnu.org>.
*/

[assembly: AssemblyTitle("mcat")]
[assembly: AssemblyCopyright("(c)2005-2007 Rafael Teixeira")]
[assembly: AssemblyDescription("Simulated cat-like program")]
[assembly: AssemblyVersion ("2.0.0.0")]

[assembly: Commons.About("Just a simulated cat to demonstrate Commons.GetOptions")]
[assembly: Commons.Author("Rafael Teixeira")]
[assembly: Commons.UsageComplement("[FILE]...\nConcatenate FILE(s), or standard input, to standard output.")]
[assembly: Commons.AdditionalInfo("With no FILE, or when FILE is -, read standard input.")]
[assembly: Commons.ReportBugsTo("rafaelteixeirabr@hotmail.com")]

public class CatLikeOptions : Options 
{	
	[Option("display TAB characters as ^I", 'T', "show-tabs")]
	public bool ShowTabs;

	[Option("display $ at end of each line", 'E', "show-ends")]
	public bool ShowLineEnds;
	
	[Option("use ^ and M- notation, except for LFD and TAB", 'v', "show-nonprinting")]
	public bool ShowNonPrinting;

	[Option("equivalent to -vE", 'e', null)]
	public bool ShowLineEndsAndNonPrinting { set { ShowLineEnds = ShowNonPrinting = value; } }
	
	[Option("equivalent to -vT", 't', null)]
	public bool ShowLineEndsAndTabs { set { ShowTabs = ShowNonPrinting = value; } }
	
	[Option("equivalent to -vET", 'A', "show-all")]
	public bool showAll { set { ShowTabs = ShowLineEnds = ShowNonPrinting = value; } }
	
	[Option("number nonblank output lines", 'b', "number-nonblank")]
	public bool NumberNonBlank;
	
	[Option("number all output lines", 'n', "number")]
	public bool NumberAllLines;
	
	[Option("never more than one single blank line", 's', "squeeze-blank")]
	public bool SqueezeBlankLines;
	
	[Option("(ignored)", 'u', null)]
	public bool Ignored;

	[Option("output version information and exit", "version")]
	public override WhatToDoNext DoAbout()
	{
		return base.DoAbout();
	}

	[Option("display this help and exit", "help")]
	public override WhatToDoNext DoHelp()
	{
		return base.DoHelp();
	}

	[KillInheritedOption]
	public override WhatToDoNext DoHelp2() { return WhatToDoNext.GoAhead; }

	[KillInheritedOption]
	public override WhatToDoNext DoUsage() { return WhatToDoNext.GoAhead; }

	public CatLikeOptions(string[] args) : base(args) {}
	
	protected override void InitializeOtherDefaults() 
	{
		ParsingMode = OptionsParsingMode.Both | OptionsParsingMode.GNU_DoubleDash;
		BreakSingleDashManyLettersIntoManyOptions = true; 
	}

}

public class Driver {

	public static int Main (string[] args)
	{
		CatLikeOptions options = new CatLikeOptions(args);
		
		Console.WriteLine(@"This is just a simulation of a cat-like program.

The command line options where processed by Commons.GetOptions and resulted as:

  ShowTabs = {0}
  ShowLineEnds = {1}
  ShowNonPrinting = {2}
  NumberNonBlank = {3}
  NumberAllLines = {4}
  SqueezeBlankLines = {5}
  
  RunningOnWindows = {6}

", 
			options.ShowTabs, options.ShowLineEnds, options.ShowNonPrinting,
			options.NumberNonBlank, options.NumberAllLines, options.SqueezeBlankLines, options.RunningOnWindows);
			
		if (options.GotNoArguments || options.FirstArgument == "-")
			Console.WriteLine("No arguments provided so cat would be copying stdin to stdout");
		else 
			Console.WriteLine("Would be copying these files to stdout: {0}", 
				String.Join(", ", options.RemainingArguments));
		Console.WriteLine("\nFollows help screen\n---------------------------------------------\n");
		options.DoHelp();
		return 0;
	}

}
