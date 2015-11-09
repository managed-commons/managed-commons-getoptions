using System.Collections.Generic;
using Commons.Translation;

namespace Commons.GetOptions
{
    internal class AppTranslator : DictionaryTranslator
    {
        public AppTranslator()
        {
            AddLocale("pt", new Dictionary<string, string>
            {
                ["Command {0} executed!"] = "Comando {0} executado!",
                ["Exercises Commons.GetOptions features"] = "Exercita algumas funcionalidades da Commons.GetOptions",
                ["First mock command"] = "Primeiro falso comando",
                ["Fourth mock command"] = "Quarto falso comando",
                ["Just some bonking for testing..."] = "Só uma brincadeira para testes...",
                ["Only gamma command needs this set!"] = "Só o comando gamma precisa deste argumento!",
                ["Second mock command"] = "Segundo falso comando",
                ["Should have been gamma corrected!!!"] = "Deve ter o gamma corrigido!!!",
                ["Third mock command"] = "Terceiro falso comando"
            });
        }
    }
}
