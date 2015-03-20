using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Translation
{
	public interface ITranslator
	{
		string Translate(string locale, string textToTranslate);

		string TranslatePlural(string locale, string singular, string plural, int quantity);
	}
}