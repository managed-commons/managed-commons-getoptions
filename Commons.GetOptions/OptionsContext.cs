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
using static System.Console;
using static Commons.Translation.TranslationService;

namespace Commons.GetOptions
{
    public delegate void ErrorReporter(int num, string msg);

    public class OptionsContext
    {
        public bool BreakSingleDashManyLettersIntoManyOptions = false;
        public bool DontSplitOnCommas = false;
        public bool EndOptionProcessingWithDoubleDash = true;
        public OptionsParsingMode ParsingMode = OptionsParsingMode.Both;
        public ErrorReporter ReportError = DefaultErrorReporter;

        public bool RunningOnWindows
        {
            get
            {
                var platform = Environment.OSVersion.Platform;
                return ((platform != PlatformID.Unix) && (platform != PlatformID.MacOSX));
            }
        }

        public static void DefaultErrorReporter(int number, string message)
        {
            if (number > 0)
                WriteLine(_Format("Error {0}: {1}", number, message));
            else
                WriteLine(TranslateAndFormat("Error: {0}", message));
        }

        public static string[] Exit(int exitCode)
        {
            Environment.Exit(exitCode);
            return null;
        }
    }
}
