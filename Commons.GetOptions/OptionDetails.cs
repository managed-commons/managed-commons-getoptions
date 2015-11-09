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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using static System.Console;
using static Commons.Translation.TranslationService;

namespace Commons.GetOptions
{
    public enum WhatToDoNext
    {
        AbandonProgram,
        GoAhead
    }

    internal enum OptionProcessingResult
    {
        NotThisOption,
        OptionAlone,
        OptionConsumedParameter
    }

    internal class OptionDetails : IComparable
    {
        public string AlternateForm;
        public bool BooleanOption;
        public bool DontSplitOnCommas;
        public bool Hidden;
        public string LongForm;
        public int MaxOccurs;
        public MemberInfo MemberInfo;
        public bool NeedsParameter;
        public OptionDetails NextAlternate = null;
        public int Occurs;// negative means there is no limit
        public object OptionBundle;
        public Type ParameterType;
        public string paramName = null;
        public OptionsParsingMode ParsingMode;
        public bool SecondLevelHelp;
        public string ShortDescription;
        public char ShortForm;
        public ArrayList Values;
        public bool VBCStyleBoolean;

        static OptionDetails()
        {
            RegisterTranslator(new Translator());
        }

        public OptionDetails(
            MemberInfo memberInfo,
            OptionAttribute option,
            object optionBundle,
            OptionsParsingMode parsingMode,
            bool dontSplitOnCommas
        )
        {
            ShortForm = option.ShortForm;
            ParsingMode = parsingMode;
            DontSplitOnCommas = dontSplitOnCommas;
            if (string.IsNullOrWhiteSpace(option.Name))
                LongForm = (ShortForm == default(char)) ? memberInfo.Name : string.Empty;
            else
                LongForm = option.Name;
            AlternateForm = option.AlternateForm;
            ShortDescription = ExtractParamName(option.TranslatedDescription);
            Occurs = 0;
            OptionBundle = optionBundle;
            BooleanOption = false;
            MemberInfo = memberInfo;
            NeedsParameter = false;
            Values = null;
            MaxOccurs = 1;
            VBCStyleBoolean = option.VBCStyleBoolean;
            SecondLevelHelp = option.SecondLevelHelp;
            Hidden = false; // TODO: check other attributes

            ParameterType = TypeOfMember(memberInfo);
            var NonDefaultMessage = string.Format("MaxOccurs set to non default value ({0}) for a [{1}] option", option.MaxOccurs, MemberInfo);
            if (ParameterType != null) {
                if (ParameterType.FullName != "System.Boolean") {
                    if (LongForm.IndexOf(':') >= 0)
                        throw new InvalidOperationException(string.Format("Options with an embedded colon (':') in their visible name must be boolean!!! [{0} isn't]", MemberInfo));

                    NeedsParameter = true;

                    if (option.MaxOccurs != 1) {
                        if (ParameterType.IsArray) {
                            Values = new ArrayList();
                            MaxOccurs = option.MaxOccurs;
                        } else {
                            if (MemberInfo is MethodInfo || MemberInfo is PropertyInfo)
                                MaxOccurs = option.MaxOccurs;
                            else
                                throw new InvalidOperationException(NonDefaultMessage);
                        }
                    }
                } else {
                    BooleanOption = true;
                    if (option.MaxOccurs != 1) {
                        if (MemberInfo is MethodInfo || MemberInfo is PropertyInfo)
                            MaxOccurs = option.MaxOccurs;
                        else
                            throw new InvalidOperationException(NonDefaultMessage);
                    }
                }
            }
        }

        public string DefaultForm
        {
            get
            {
                string shortPrefix = "-";
                string longPrefix = "--";
                if (ParsingMode == OptionsParsingMode.Windows) {
                    shortPrefix = "/";
                    longPrefix = "/";
                }
                return ShortForm != default(char) ? shortPrefix + ShortForm : longPrefix + LongForm;
            }
        }

        public string ParamName => paramName;

        public static void LinkAlternatesInsideList(List<OptionDetails> list)
        {
            var baseForms = new Hashtable(list.Count);
            foreach (OptionDetails option in list) {
                if (option.LongForm != null && option.LongForm.Trim().Length > 0) {
                    string[] parts = option.LongForm.Split(':');
                    if (parts.Length < 2) {
                        baseForms.Add(option.LongForm, option);
                    } else {
                        var baseForm = (OptionDetails)baseForms[parts[0]];
                        if (baseForm != null) {
                            // simple linked list
                            option.NextAlternate = baseForm.NextAlternate;
                            baseForm.NextAlternate = option;
                        }
                    }
                }
            }
        }

        int IComparable.CompareTo(object other) => string.Compare(Key, ((OptionDetails)other).Key, StringComparison.Ordinal);

        public OptionProcessingResult ProcessArgument(string arg, string nextArg)
        {
            if (IsAlternate(arg + ":" + nextArg))
                return OptionProcessingResult.NotThisOption;

            if (IsThisOption(arg)) {
                if (!NeedsParameter) {
                    if (VBCStyleBoolean && arg.EndsWith("-", StringComparison.Ordinal))
                        DoIt(false);
                    else
                        DoIt(true);
                    return OptionProcessingResult.OptionAlone;
                }
                DoIt(nextArg);
                return OptionProcessingResult.OptionConsumedParameter;
            }

            if (IsThisOption(arg + ":" + nextArg)) {
                DoIt(true);
                return OptionProcessingResult.OptionConsumedParameter;
            }

            return OptionProcessingResult.NotThisOption;
        }

        public override string ToString()
        {
            if (optionHelp == null) {
                string shortPrefix;
                string longPrefix;
                bool hasLongForm = (LongForm != null && LongForm != string.Empty);
                if (ParsingMode == OptionsParsingMode.Windows) {
                    shortPrefix = "/";
                    longPrefix = "/";
                } else {
                    shortPrefix = "-";
                    longPrefix = "--";
                }
                optionHelp = "  ";
                optionHelp += (ShortForm != default(char)) ? shortPrefix + ShortForm + " " : "   ";
                optionHelp += hasLongForm ? longPrefix + LongForm : "";
                if (NeedsParameter) {
                    if (hasLongForm)
                        optionHelp += ":";
                    optionHelp += ParamName;
                } else if (BooleanOption && VBCStyleBoolean) {
                    optionHelp += "[+|-]";
                }
                optionHelp += "\t";
                if (AlternateForm != string.Empty && AlternateForm != null)
                    optionHelp += _("Also ") + shortPrefix + AlternateForm + (NeedsParameter ? (":" + ParamName) : "") + ". ";
                optionHelp += ShortDescription;
            }
            return optionHelp;
        }

        public void TransferValues()
        {
            if (Values != null) {
                if (MemberInfo is FieldInfo) {
                    ((FieldInfo)MemberInfo).SetValue(OptionBundle, Values.ToArray(ParameterType.GetElementType()));
                    return;
                }

                if (MemberInfo is PropertyInfo) {
                    ((PropertyInfo)MemberInfo).SetValue(OptionBundle, Values.ToArray(ParameterType.GetElementType()), null);
                    return;
                }

                if ((WhatToDoNext)((MethodInfo)MemberInfo).Invoke(OptionBundle, new object[] { Values.ToArray(ParameterType.GetElementType()) }) == WhatToDoNext.AbandonProgram)
                    System.Environment.Exit(1);
            }
        }

        internal string Key => (ShortForm == default(char)) ? LongForm : ShortForm + " " + LongForm;
        private string optionHelp = null;
        bool AddingOneMoreExceedsMaxOccurs => HowManyBeforeExceedingMaxOccurs(1) < 1;

        static System.Type TypeOfMember(MemberInfo memberInfo)
        {
            if ((memberInfo.MemberType == MemberTypes.Field && memberInfo is FieldInfo))
                return ((FieldInfo)memberInfo).FieldType;

            if ((memberInfo.MemberType == MemberTypes.Property && memberInfo is PropertyInfo))
                return ((PropertyInfo)memberInfo).PropertyType;

            if ((memberInfo.MemberType == MemberTypes.Method && memberInfo is MethodInfo)) {
                if (((MethodInfo)memberInfo).ReturnType.FullName != typeof(WhatToDoNext).FullName)
                    throw new NotSupportedException("Option method must return '" + typeof(WhatToDoNext).FullName + "'");
                ParameterInfo[] parameters = ((MethodInfo)memberInfo).GetParameters();
                return (parameters == null) || (parameters.Length == 0) ? null : parameters[0].ParameterType;
            }

            throw new NotSupportedException("'" + memberInfo.MemberType + "' memberType is not supported");
        }

        void DoIt(bool setValue)
        {
            if (AddingOneMoreExceedsMaxOccurs)
                return;

            if (MemberInfo is FieldInfo) {
                ((FieldInfo)MemberInfo).SetValue(OptionBundle, setValue);
                return;
            }
            if (MemberInfo is PropertyInfo) {
                ((PropertyInfo)MemberInfo).SetValue(OptionBundle, setValue, null);
                return;
            }
            if ((WhatToDoNext)((MethodInfo)MemberInfo).Invoke(OptionBundle, null) == WhatToDoNext.AbandonProgram)
                System.Environment.Exit(1);
        }

        void DoIt(string parameterValue)
        {
            if (parameterValue == null)
                parameterValue = "";

            string[] parameterValues;

            if (DontSplitOnCommas || MaxOccurs == 1)
                parameterValues = new string[] { parameterValue };
            else
                parameterValues = parameterValue.Split(',');

            int waitingToBeProcessed = HowManyBeforeExceedingMaxOccurs(parameterValues.Length);

            foreach (string parameter in parameterValues) {
                if (waitingToBeProcessed-- <= 0)
                    break;

                object convertedParameter = null;

                if (Values != null && parameter != null) {
                    try {
                        convertedParameter = Convert.ChangeType(parameter, ParameterType.GetElementType());
                    } catch (Exception ex) {
                        ReportBadConversion(parameter, ex);
                    }
                    Values.Add(convertedParameter);
                    continue;
                }

                if (parameter != null) {
                    try {
                        convertedParameter = Convert.ChangeType(parameter, ParameterType);
                    } catch (Exception ex) {
                        ReportBadConversion(parameter, ex);
                        continue;
                    }
                }

                if (MemberInfo is FieldInfo) {
                    ((FieldInfo)MemberInfo).SetValue(OptionBundle, convertedParameter);
                    continue;
                }

                if (MemberInfo is PropertyInfo) {
                    ((PropertyInfo)MemberInfo).SetValue(OptionBundle, convertedParameter, null);
                    continue;
                }

                if ((WhatToDoNext)((MethodInfo)MemberInfo).Invoke(OptionBundle, new object[] { convertedParameter }) == WhatToDoNext.AbandonProgram)
                    System.Environment.Exit(1);
            }
        }

        string ExtractParamName(string shortDescription)
        {
            int whereBegins = shortDescription.IndexOf("{", StringComparison.Ordinal);
            if (whereBegins < 0)
                paramName = _("PARAM");
            else {
                int whereEnds = shortDescription.IndexOf("}", StringComparison.Ordinal);
                if (whereEnds < whereBegins)
                    whereEnds = shortDescription.Length + 1;

                paramName = shortDescription.Substring(whereBegins + 1, whereEnds - whereBegins - 1);
                shortDescription =
                    shortDescription.Substring(0, whereBegins) +
                paramName +
                shortDescription.Substring(whereEnds + 1);
            }
            return shortDescription;
        }

        int HowManyBeforeExceedingMaxOccurs(int howMany)
        {
            if (MaxOccurs > 0 && (Occurs + howMany) > MaxOccurs) {
                Error.WriteLine(TranslateAndFormat("Option {0} can be used at most {1} times. Ignoring extras...", LongForm, MaxOccurs));
                howMany = MaxOccurs - Occurs;
            }
            Occurs += howMany;
            return howMany;
        }

        bool IsAlternate(string compoundArg)
        {
            OptionDetails next = NextAlternate;
            while (next != null) {
                if (next.IsThisOption(compoundArg))
                    return true;
                next = next.NextAlternate;
            }
            return false;
        }

        bool IsThisOption(string arg)
        {
            if (arg != null && arg != string.Empty) {
                arg = arg.TrimStart('-', '/');
                if (VBCStyleBoolean)
                    arg = arg.TrimEnd('-', '+');
                return (MatchShortForm(arg) || arg == LongForm || arg == AlternateForm);
            }
            return false;
        }

        bool MatchShortForm(string arg) => arg.Length == 1 && arg[0] == ShortForm;

        void ReportBadConversion(string parameter, Exception ex)
        {
            WriteLine(TranslateAndFormat("The value '{0}' is not convertible to the appropriate type '{1}' for the {2} option (reason '{3}')", parameter, ParameterType.Name, DefaultForm, ex.Message));
        }
    }
}
