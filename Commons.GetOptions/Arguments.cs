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

using System.Collections.Generic;

namespace Commons.GetOptions
{
    public class Arguments : List<string>
    {
        public Arguments(IEnumerable<string> list) : base(list)
        {
        }

        public string FifthArgument => (Count > 4) ? this[4] : null;

        public string FirstArgument => (Count > 0) ? this[0] : null;

        public string FourthArgument => (Count > 3) ? this[3] : null;

        public bool GotNoArguments => Count == 0;

        public string SecondArgument => (Count > 1) ? this[1] : null;

        public string ThirdArgument => (Count > 2) ? this[2] : null;
    }
}
