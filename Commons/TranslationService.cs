using System;
using System.Collections.Generic;
using System.Linq;

namespace Commons
{
	public static class TranslationService
	{
		public static TranslatePlural PluralTranslator;

		public static Translate Translator;

		public static string Translate(string textToTranslate)
		{
			return (Translator == null) ? textToTranslate : Translator(textToTranslate);
		}

		public static string TranslatePlural(string singular, string plural, int quantity)
		{
			return (PluralTranslator == null) ?
					(quantity == 1 ? singular : plural) :
					PluralTranslator(singular, plural, quantity);
		}
	}

	public delegate string Translate(string textToTranslate);

	public delegate string TranslatePlural(string singular, string plural, int quantity);
}