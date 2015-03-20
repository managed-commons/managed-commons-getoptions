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
using Commons.Reflection;
using _ = Commons.Translation.TranslationService;

namespace Commons
{
	public class AssemblyInformation
	{
		public AssemblyInformation(Assembly assembly)
		{
			ExeName = assembly.GetName().Name;
			Version = assembly.GetVersion();
			Title = assembly.GetAssemblyAttributeValueAsString<AssemblyTitleAttribute>(a => a.Title);
			Copyright = assembly.GetAssemblyAttributeValueAsString<AssemblyCopyrightAttribute>(a => a.Copyright);
			Description = assembly.GetAssemblyAttributeValueAsString<AssemblyDescriptionAttribute>(a => a.Description);
			AboutDetails = assembly.GetAssemblyAttributeAsString<AboutAttribute>();
			AdditionalInfo = assembly.GetAssemblyAttributeAsString<AdditionalInfoAttribute>();
			ReportBugsTo = assembly.GetAssemblyAttributeAsString<ReportBugsToAttribute>();
			License = assembly.GetAssemblyAttributeAsString<LicenseAttribute>();
			Authors = GetAuthors(assembly);
		}

		public static AssemblyInformation FromEntryAssembly { get { return new AssemblyInformation(Assembly.GetEntryAssembly()); } }

		public string AboutDetails { get; set; }

		public string AdditionalBannerInfo { get; set; }

		public string AdditionalInfo { get; set; }

		public IEnumerable<string> Authors { get; set; }

		public string Copyright { get; set; }

		public string Description { get; set; }

		public string ExeName { get; set; }

		public string License { get; set; }

		public string Product { get; set; }

		public string ReportBugsTo { get; set; }

		public string Title { get; set; }

		public string Version { get; set; }

		public AssemblyInformation WithDefaults
		{
			get
			{
				AboutDetails = AboutDetails ?? "Add a [assembly: Commons.About(\"Here goes the short about details\")] to your assembly";
				Copyright = Copyright ?? string.Format("Add a [assembly: AssemblyCopyright(\"(c){0:0000} Here goes the copyright holder name\")] to your assembly", DateTimeOffset.Now.Year);
				Description = Description ?? "Add a [assembly: AssemblyDescription(\"Here goes the short description\")] to your assembly";
				Title = Title ?? "Add a [assembly: AssemblyTitle(\"Here goes the application name\")] to your assembly";
				Product = Product ?? "Add a [assembly: AssemblyProduct(\"Here goes the product/parent project name\")] to your assembly";
				if (Authors == null) {
					var authors = new String[1];
					authors[0] = "Add [assembly: AssemblyCompany(\"Here goes the authors' names, separated by commas\")] to your assembly";
					Authors = authors;
				}
				return this;
			}
		}

		public void ShowAbout()
		{
			ShowTitleLines();
			Console.WriteLine(_.Translate(AboutDetails));
			Console.Write(_.Translate("Authors: "));
			Console.WriteLine(string.Join(", ", Authors));
		}

		public void ShowBanner()
		{
			Console.WriteLine(_.Translate(Title) + "  " + Version + " - " + Copyright);
			if (AdditionalBannerInfo != null)
				Console.WriteLine(AdditionalBannerInfo);
		}

		public void ShowFooter()
		{
			if (AdditionalInfo != null)
				Console.WriteLine("\n{0}", _.Translate(AdditionalInfo));
			if (ReportBugsTo != null)
				Console.WriteLine(_.Translate("\nPlease report bugs {0} <{1}>"),
								  (ReportBugsTo.IndexOf('@') > 0) ? _.Translate("to") : _.Translate("at"),
								  _.Translate(ReportBugsTo));
		}

		public void ShowTitleLines()
		{
			ShowBanner();
			Console.WriteLine(_.Translate(Description));
			if (!string.IsNullOrWhiteSpace(License))
				Console.WriteLine("\r\n" + _.Translate("License: ") + License);
			Console.WriteLine();
		}

		private static IEnumerable<string> GetAuthors(Assembly assembly)
		{
			string company = assembly.GetAssemblyAttributeValueAsString<AssemblyCompanyAttribute>(a => a.Company);
			if (!string.IsNullOrWhiteSpace(company))
				return company.Split(',').Select(s => s.Trim());
			return null;
		}
	}
}