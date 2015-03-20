// Commons.Core
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

namespace Commons.Translation
{
	public class DictionaryTranslator : ITranslator
	{
		public void AddLocale(string locale, Dictionary<string, string> dictionary)
		{
			if (_.ContainsKey(locale))
				throw new ArgumentException("Locale already added!!!", nameof(locale));
			_[locale] = dictionary;
			if (locale.Contains("-"))
				locale = locale.Split('-')[0];
			if (_.ContainsKey(locale))
				_[locale] = dictionary;
		}

		public string Translate(string locale, string textToTranslate)
		{
			return InnerTranslate(locale, textToTranslate);
		}

		public string TranslatePlural(string locale, string singular, string plural, int quantity)
		{
			return quantity == 1 ? InnerTranslate(locale, singular) : InnerTranslate(locale, plural);
		}

		private readonly Dictionary<string, Dictionary<string, string>> _ = new Dictionary<string, Dictionary<string, string>>();

		private string InnerTranslate(string locale, string textToTranslate)
		{
			if (_.ContainsKey(locale)) {
				var dic = _[locale];
				if (dic.ContainsKey(textToTranslate))
					return dic[textToTranslate];
			}
			if (locale.Contains("-"))
				return InnerTranslate(locale.Split('-')[0], textToTranslate);
			return null;
		}
	}
}