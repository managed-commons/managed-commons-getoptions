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
using System.Collections.Generic;
using System.Linq;

namespace Commons.GetOptions
{
	public class Options
	{
		public string[] RemainingArguments;

		public Options(OptionsContext context, string[] args = null)
		{
			InitializeOtherDefaults();
			Context = context;
			OptionParser = new OptionList(this, context);
			OptionParser.AdditionalBannerInfo = AdditionalBannerInfo;
			if (args != null)
				ProcessArgs(args, OptionsContext.Exit);
		}

		public string FifthArgument { get { return (RemainingArguments.Length > 4) ? RemainingArguments[4] : null; } }

		public string FirstArgument { get { return (RemainingArguments.Length > 0) ? RemainingArguments[0] : null; } }

		public string FourthArgument { get { return (RemainingArguments.Length > 3) ? RemainingArguments[3] : null; } }

		public bool GotNoArguments { get { return RemainingArguments.Length == 0; } }

		public string SecondArgument { get { return (RemainingArguments.Length > 1) ? RemainingArguments[1] : null; } }

		public string ThirdArgument { get { return (RemainingArguments.Length > 2) ? RemainingArguments[2] : null; } }

		[Option("Display version and licensing information", ShortForm = 'V', Name = "version")]
		public virtual WhatToDoNext DoAbout()
		{
			return OptionParser.DoAbout();
		}

		[Option("Show this help list", ShortForm = '?', Name = "help")]
		public virtual WhatToDoNext DoHelp()
		{
			return OptionParser.DoHelp();
		}

		public void DoUsage()
		{
			OptionParser.DoUsage();
		}

		public String[] ProcessArgs(string[] args, Func<int, string[]> exitFunc)
		{
			return (RemainingArguments = OptionParser.ProcessArgs(args, exitFunc));
		}

		public void Reset()
		{
			OptionParser.Reset();
		}

		public void ShowBanner()
		{
			OptionParser.ShowBanner();
		}

		protected readonly OptionsContext Context;
		protected readonly OptionList OptionParser;

		protected virtual string AdditionalBannerInfo { get { return null; } }

		protected virtual void InitializeOtherDefaults()
		{
		}
	}
}