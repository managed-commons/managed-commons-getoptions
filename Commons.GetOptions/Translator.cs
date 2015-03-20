using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commons.Translation;

namespace Commons.GetOptions
{
	internal class Translator : DictionaryTranslator
	{
		public Translator()
		{
			AddLocale("pt", new Dictionary<string, string>()
			{
				["\nPlease report bugs {0} <{1}>"] = "\nPor favor, reporte bugs {0} <{1}>",
				["Also "] = "Também ",
				["at"] = "em",
				["Authors: "] = "Autores: ",
				["Commands:"] = "Comandos:",
				["Display version and licensing information"] = "Mostra a versão e informações da licença de uso",
				["Error {0}: {1}"] = "Erro {0}: {1}",
				["Error: {0}"] = "Erro: {0}",
				["Exception: {0}"] = "Exceção: {0}",
				["Invalid argument: '{0}'"] = "Argumento inválido: '{0}'",
				["License: "] = "Licença: ",
				["Option {0} can be used at most {1} times. Ignoring extras..."] = "Opção {0} pode ser usada no máximo {1} vezes. Ignorando os usos extras...",
				["Options:"] = "Opções:",
				["PARAM"] = "PARÂMETRO",
				["Show this help list"] = "Mostra este texto de ajuda",
				["The value '{0}' is not convertible to the appropriate type '{1}' for the {2} option (reason '{3}')"] = "O valor '{0}' não é conversível para o tipo '{1}' apropriado para a opção {2} (razão '{3}')",
				["to"] = "para"
			});
		}
	}
}