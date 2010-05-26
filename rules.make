srcdir = .
MCS = gmcs
CSFLAGS =  -debug -target:library -codepage:utf8 -nowarn:169

TESTDIR = $(srcdir)/Test
TESTRESULTDIR = $(TESTDIR)
DOCSDIR = $(srcdir)/docs

TOOLSDIR = $(srcdir)/tools

BUILDDIR = $(srcdir)/bin
LIBDIR = $(srcdir)/lib
XMLDOCSDIR = $(srcdir)/xmldocs

TARGET_NAME = $(NAME).dll
TEST_TARGET_NAME = $(NAME)_Test.dll

TARGET = $(BUILDDIR)/$(TARGET_NAME)
TESTTARGET = $(BUILDDIR)/$(TEST_TARGET_NAME)

PROJECT_LIBS = $(addsuffix .dll, $(addprefix $(BUILDDIR)/, $(PROJECT_LIBS_NAMES)))
MANAGED_LIBS = $(addsuffix .dll, $(addprefix $(LIBDIR)/, $(MANAGED_LIBS_NAMES)))

LIBS = $(PROJECT_LIBS) $(MANAGED_LIBS)

TEST_MANAGED_LIB = $(LIBDIR)/nunit.framework.dll
TESTRESULT = $(TESTRESULTDIR)/$(TESTTARGET_NAME).results

FORMS_DIR = $(srcdir)/$(FORMS_NAMESPACE)/
FORMS_FILENAME_BASES = $(addprefix $(FORMS_DIR), $(FORMS_NAMES))
FORMS_GLADES = $(addsuffix .glade, $(FORMS_FILENAME_BASES))
FORMS_SOURCES = $(addsuffix .cs, $(FORMS_FILENAME_BASES))
FORMS_BASES = $(addsuffix -Form.cs, $(FORMS_FILENAME_BASES))

PKG_REFERENCES_BUILD = $(addprefix -pkg:, $(PKG_REFERENCES))

REFERENCES = \
	$(addprefix -r:, $(LIBS) $(GAC_LIBS))

RESOURCES = \
	$(FORMS_GLADES) \
	$(EXTERNAL_RESOURCES)

RESOURCES_BUILD = $(foreach res,$(RESOURCES), $(addprefix -resource:,$(res)),$(notdir $(res)))

NUNITCONSOLE = mono $(TOOLSDIR)/nunit-console.exe

XMLDOCSINDEX = $(XMLDOCSDIR)/index.xml
DOCSINDEX = $(DOCSDIR)/index.html
DOCTEMPLATE = $(TOOLSDIR)/monodoctemplate.xsl

all: builddir $(TARGET)

builddir: 
	@mkdir -p $(BUILDDIR)
	@list='$(MANAGED_LIBS)'; for lib in $$list; do cp -f $$lib $$lib.mdb $(BUILDDIR); done

$(FORMS_BASES): $(FORMS_GLADES)
	@list='$(FORMS_GLADES)'; for form in $$list; do $(TOOLSDIR)/gladebfb "$$form" $(FORMS_NAMESPACE); done
	
TARGET_CSFILES = \
	$(CSFILES) \
	$(FORMS_BASES) \
	$(FORMS_SOURCES)

$(TARGET): $(TARGET_CSFILES) $(RESOURCES) $(LIBS)
	$(MCS) -out:$@ $(CSFLAGS) $(REFERENCES) $(PKG_REFERENCES_BUILD) $(RESOURCES_BUILD) $(TARGET_CSFILES)
	@find . -name '*~' -print -exec rm '{}' ';'
	@find . -name '*.glade.bak' -print -exec rm '{}' ';'

$(TESTTARGET): $(TESTTARGET_CSFILES) all
	cp $(TEST_MANAGED_LIB) $(BUILDDIR)
	$(MCS) -out:$@ $(CSFLAGS)  $(REFERENCES) $(PKG_REFERENCES_BUILD) -r:$(TARGET) -r:$(TEST_MANAGED_LIB) $(TESTTARGET_CSFILES)

html: $(DOCSINDEX)

test: $(TESTTARGET)

run-test: test
	cd `dirname $(TESTTARGET)` && $(NUNITCONSOLE) /output=$(TESTRESULT) `basename $(TESTTARGET)`

clean:
	rm -rf $(CLEANFILES) $(DOCSDIR)
	
$(XMLDOCSINDEX): $(TARGET)
	mkdir -p $(XMLDOCSDIR)
	monodocer -assembly $(TARGET) -path $(XMLDOCSDIR)
	
$(DOCSINDEX): $(XMLDOCSINDEX) $(DOCTEMPLATE)
	mkdir -p $(DOCSDIR)
	monodocs2html -template $(DOCTEMPLATE) -source $(XMLDOCSDIR) -dest $(DOCSDIR)

EXTRA_DIST = \
	ChangeLog \
	$(CSFILES) \
	$(FORMS_SOURCES) \
	$(RESOURCES)

CLEANFILES += \
	$(TARGET) \
	$(TESTTARGET) \
	$(FORMS_BASES)
	
	
