UNIQUE_FILTER_PIPE = tr [:space:] \\n | sort | uniq
BUILD_DATA_DIR = $(top_builddir)/bin/share/$(PACKAGE)

SOURCES_BUILD = $(addprefix $(srcdir)/, $(SOURCES))
#SOURCES_BUILD += $(top_srcdir)/src/AssemblyInfo.cs

SUBST = ,
RESOURCES_D := $(foreach res,$(RESOURCES),$(firstword $(subst $(SUBST), ,$(strip $(res)))))
RESOURCES_DIST := $(addprefix $(srcdir)/, $(RESOURCES_D))
RESOURCES_EXPANDED = $(addprefix $(srcdir)/, $(RESOURCES))
RESOURCES_BUILD = $(foreach resource, $(RESOURCES_EXPANDED), \
	-resource:$(resource))

#INSTALL_ICONS = $(top_srcdir)/build/private-icon-theme-installer "$(mkinstalldirs)" "$(INSTALL_DATA)"
#THEME_ICONS_SOURCE = $(wildcard $(srcdir)/ThemeIcons/*/*/*.png) $(wildcard $(srcdir)/ThemeIcons/scalable/*/*.svg)
#THEME_ICONS_RELATIVE = $(subst $(srcdir)/ThemeIcons/, , $(THEME_ICONS_SOURCE))

ASSEMBLY_EXTENSION = $(strip $(patsubst library, dll, $(TARGET)))
ASSEMBLY_FILE = $(top_builddir)/bin/$(ASSEMBLY).$(ASSEMBLY_EXTENSION)

INSTALL_DIR_RESOLVED = $(firstword $(subst , $(DEFAULT_INSTALL_DIR), $(INSTALL_DIR)))

if ENABLE_TESTS
    LINK += " $(NUNIT_LIBS)"
    ENABLE_TESTS_FLAG = "-define:ENABLE_TESTS"
endif

FILTERED_LINK = $(shell echo "$(LINK)" | $(UNIQUE_FILTER_PIPE))
DEP_LINK = $(shell echo "$(LINK)" | $(UNIQUE_FILTER_PIPE) | sed s,-r:,,g | grep '$(top_builddir)/bin/')

OUTPUT_FILES = \
	$(ASSEMBLY_FILE) \
	$(ASSEMBLY_FILE).mdb \
	$(DLLCONFIG)

moduledir = $(INSTALL_DIR_RESOLVED)
module_SCRIPTS = $(OUTPUT_FILES)

@INTLTOOL_DESKTOP_RULE@
desktopdir = $(datadir)/applications
desktop_in_files = $(DESKTOP_FILE)
desktop_DATA = $(desktop_in_files:.desktop.in=.desktop)

imagesdir = @datadir@/@PACKAGE@/images
images_DATA = $(IMAGES)

logo_48dir = @datadir@/icons/hicolor/48x48/apps
logo_48_DATA = $(LOGO_48)

logodir = @datadir@/icons/hicolor/scalable/apps
logo_DATA = $(LOGO)

all: $(ASSEMBLY_FILE)

run: 
	@pushd $(top_builddir); \
	make run; \
	popd;

test:
	@pushd $(top_builddir)/tests; \
	make $(ASSEMBLY); \
	popd;

build-debug:
	@echo $(DEP_LINK)

$(ASSEMBLY_FILE).mdb: $(ASSEMBLY_FILE)

$(ASSEMBLY_FILE): $(SOURCES_BUILD) $(DEP_LINK)
	@mkdir -p $(top_builddir)/bin
	$(MCS) \
		$(GMCS_FLAGS) \
		$(ASSEMBLY_BUILD_FLAGS) \
		-nowarn:0278 -nowarn:0078 $$warn -unsafe \
		-define:HAVE_GTK -codepage:utf8 \
		-debug -target:$(TARGET) -out:$@ \
		$(BUILD_DEFINES) $(ENABLE_TESTS_FLAG) $(ENABLE_ATK_FLAG) \
		$(FILTERED_LINK) $(RESOURCES_BUILD) $(SOURCES_BUILD)
	@if [ ! -z "$(EXTRA_BUNDLE)" ]; then \
		cp $(EXTRA_BUNDLE) $(top_builddir)/bin; \
	fi;

#theme-icons: $(THEME_ICONS_SOURCE)
#	@$(INSTALL_ICONS) -il "$(BUILD_DATA_DIR)" "$(srcdir)" $(THEME_ICONS_RELATIVE)

install-data-hook: $(THEME_ICONS_SOURCE)
	@$(INSTALL_ICONS) -i "$(DESTDIR)$(pkgdatadir)" "$(srcdir)" $(THEME_ICONS_RELATIVE)
	$(EXTRA_INSTALL_DATA_HOOK)

uninstall-hook: $(THEME_ICONS_SOURCE)
	@$(INSTALL_ICONS) -u "$(DESTDIR)$(pkgdatadir)" "$(srcdir)" $(THEME_ICONS_RELATIVE)
	$(EXTRA_UNINSTALL_HOOK)

EXTRA_DIST = $(SOURCES_BUILD) $(RESOURCES_DIST) $(THEME_ICONS_SOURCE) $(IMAGES) $(LOGO) $(LOGO_48) $(desktop_in_files)

CLEANFILES = $(OUTPUT_FILES)
DISTCLEANFILES = *.pidb $(desktop_DATA)
MAINTAINERCLEANFILES = Makefile.in

