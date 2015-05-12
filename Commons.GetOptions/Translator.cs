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