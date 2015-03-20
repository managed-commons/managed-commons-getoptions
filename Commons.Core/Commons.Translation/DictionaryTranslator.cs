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