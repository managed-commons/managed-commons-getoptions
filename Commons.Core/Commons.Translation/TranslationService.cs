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
using System.Reflection;
using System.Threading;

namespace Commons.Translation
{
	public static class TranslationService
	{
		public static void Register(ITranslator translator)
		{
			if (translator == null)
				throw new ArgumentNullException(nameof(translator));
			var context = Assembly.GetCallingAssembly().FullName;
			_chain = new TranslatorInChain(context, translator, _chain);
		}

		public static string Translate(string textToTranslate)
		{
			var context = Assembly.GetCallingAssembly().FullName;
			return InnerTranslate(Thread.CurrentThread.CurrentCulture.Name, textToTranslate, context);
		}

		public static string Translate(string locale, string textToTranslate)
		{
			if (locale == null)
				throw new ArgumentNullException(nameof(locale));
			var context = Assembly.GetCallingAssembly().FullName;
			return InnerTranslate(locale, textToTranslate, context);
		}

		public static string TranslateAndFormat(string textToTranslate, params object[] args)
		{
			var context = Assembly.GetCallingAssembly().FullName;
			return string.Format(
				InnerTranslate(Thread.CurrentThread.CurrentCulture.Name, textToTranslate, context),
				args);
		}

		public static string TranslateAndFormat(string locale, string textToTranslate, params object[] args)
		{
			if (locale == null)
				throw new ArgumentNullException(nameof(locale));
			var context = Assembly.GetCallingAssembly().FullName;
			return string.Format(
				InnerTranslate(locale, textToTranslate, context),
				args);
		}

		public static string TranslatePlural(string singular, string plural, int quantity)
		{
			var context = Assembly.GetCallingAssembly().FullName;
			return InnerTranslatePlural(Thread.CurrentThread.CurrentCulture.Name, singular, plural, quantity, context);
		}

		public static string TranslatePlural(string locale, string singular, string plural, int quantity)
		{
			if (locale == null)
				throw new ArgumentNullException(nameof(locale));
			var context = Assembly.GetCallingAssembly().FullName;
			return InnerTranslatePlural(locale, singular, plural, quantity, context);
		}

		private static TranslatorInChain _chain = null;

		private static string FindAndTranslate(Func<TranslatorInChain, string> use, string context)
		{
			var translator = _chain;
			while (translator != null && (context == "*" || translator.Context == context)) {
				var result = use(translator);
				if (result != null)
					return result;
				translator = translator.Next;
			}
			return (context != "*") ? FindAndTranslate(use, "*") : null;
		}

		private static string InnerTranslate(string locale, string textToTranslate, string context)
		{
			if (textToTranslate == null)
				throw new ArgumentNullException(nameof(textToTranslate));
			return FindAndTranslate((tr) => tr.Translate(locale, textToTranslate), context)
				?? textToTranslate;
		}

		private static string InnerTranslatePlural(string locale, string singular, string plural, int quantity, string context)
		{
			if (singular == null)
				throw new ArgumentNullException(nameof(singular));
			if (plural == null)
				throw new ArgumentNullException(nameof(plural));
			if (quantity < 1)
				throw new ArgumentOutOfRangeException(nameof(quantity));
			return FindAndTranslate((tr) => tr.TranslatePlural(locale, singular, plural, quantity), context)
				?? ((quantity == 1) ? singular : plural);
		}

		private class TranslatorInChain : ITranslator
		{
			public readonly string Context;
			public readonly TranslatorInChain Next;

			public TranslatorInChain(string context, ITranslator translator, TranslatorInChain next)
			{
				Context = context;
				_translator = translator;
				Next = next;
			}

			public string Translate(string locale, string textToTranslate)
			{
				return _translator.Translate(locale, textToTranslate);
			}

			public string TranslatePlural(string locale, string singular, string plural, int quantity)
			{
				return _translator.TranslatePlural(locale, singular, plural, quantity);
			}

			private readonly ITranslator _translator;
		}
	}
}