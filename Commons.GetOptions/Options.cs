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

namespace Commons.GetOptions
{
    public class Options
    {
#pragma warning disable RECS0021 // Warns about calls to virtual member functions occuring in the constructor

        public Options(OptionsContext context)
        {
            Context = context;
            InitializeOtherDefaults();
            OptionParser = new OptionList(this, context);
            OptionParser.AdditionalBannerInfo = AdditionalBannerInfo;
        }

#pragma warning restore RECS0021 // Warns about calls to virtual member functions occuring in the constructor

        [Option("Display version and licensing information", ShortForm = 'V', Name = "version")]
        public virtual WhatToDoNext DoAbout() => OptionParser.DoAbout();

        [Option("Show this help list", ShortForm = '?', Name = "help")]
        public virtual WhatToDoNext DoHelp() => OptionParser.DoHelp();

        public Arguments ProcessArgs(string[] args, Func<int, string[]> exitFunc) => OptionParser.ProcessArgs(args, exitFunc);

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

        protected virtual string AdditionalBannerInfo => null;

        protected virtual void InitializeOtherDefaults()
        {
        }
    }
}
