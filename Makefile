# Commons.GetOptions makefile
#
# (c)2007 Rafael 'Monoman' Teixeira
#
# Permission is hereby granted, free of charge, to any person obtaining
# a copy of this software and associated documentation files (the
# "Software"), to deal in the Software without restriction, including
# without limitation the rights to use, copy, modify, merge, publish,
# distribute, sublicense, and/or sell copies of the Software, and to
# permit persons to whom the Software is furnished to do so, subject to
# the following conditions:
# 
# The above copyright notice and this permission notice shall be
# included in all copies or substantial portions of the Software.
# 
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
# EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
# MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
# NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
# LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
# OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
# WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#

NAME = Commons.GetOptions

MANAGED_LIBS_NAMES =

CSFILES = \
$(srcdir)/Assembly/AssemblyInfo.cs \
$(srcdir)/Commons/AuthorAttribute.cs \
$(srcdir)/Commons/ReportBugsToAttribute.cs \
$(srcdir)/Commons/IsPartOfPackageAttribute.cs \
$(srcdir)/Commons/UsageComplementAttribute.cs \
$(srcdir)/Commons/AboutAttribute.cs \
$(srcdir)/Commons/AdditionalInfoAttribute.cs \
$(srcdir)/Commons.GetOptions.Useful.Compilers/CommonCompilerOptions.cs \
$(srcdir)/Commons.GetOptions/OptionsParsingMode.cs \
$(srcdir)/Commons.GetOptions/OptionDetails.cs \
$(srcdir)/Commons.GetOptions/KillInheritedOptionAttribute.cs \
$(srcdir)/Commons.GetOptions/OptionAttribute.cs \
$(srcdir)/Commons.GetOptions/Options.cs \
$(srcdir)/Commons.GetOptions/OptionList.cs \
$(srcdir)/Commons.GetOptions/ArgumentProcessorAttribute.cs

TESTTARGET_CSFILES =

EXTRA_DISTFILES = \
	GetOptTest/AssemblyInfo.cs	\
	GetOptTest/GetOptTester.cs \
	Samples/Makefile \
	Samples/mcat.cs

include rules.make


