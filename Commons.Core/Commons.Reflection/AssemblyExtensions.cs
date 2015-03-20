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
using System.Linq;
using System.Reflection;

namespace Commons.Reflection
{
	public static class AssemblyExtensions
	{
		public static string GetAssemblyAttributeAsString<T>(this Assembly assembly) where T : Attribute
		{
			return assembly.GetAssemblyAttributeValueAsString<T>(a => a.ToString());
		}

		public static object[] GetAssemblyAttributes<T>(this Assembly assembly)
		{
			return (assembly == null) ? null : assembly.GetCustomAttributes(typeof(T), false);
		}

		public static TReturn GetAssemblyAttributeValue<T, TReturn>(this Assembly assembly, Func<T, TReturn> getter) where T : Attribute
		{
			object[] result = assembly.GetAssemblyAttributes<T>();

			if ((result != null) && (result.Length > 0) && (result[0] is T))
				return getter((T)result[0]);
			return default(TReturn);
		}

		public static string GetAssemblyAttributeValueAsString<T>(this Assembly assembly, Func<T, string> getter) where T : Attribute
		{
			return assembly.GetAssemblyAttributeValue<T, string>(getter);
		}

		public static string GetVersion(this Assembly assembly, int size = 3)
		{
			if (assembly == null)
				return string.Empty;
			var version = assembly.GetAssemblyAttributeValueAsString<AssemblyInformationalVersionAttribute>(a => a.InformationalVersion);
			if (string.IsNullOrWhiteSpace(version))
				version = assembly.GetName().Version.ToString(size);
			return version;
		}
	}
}