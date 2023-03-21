EXTRA_DIST = win32/chronojump_innosetup.iss.in \
	     win32/Makefile.in glade/*.glade \
	     src/chronojump-importer/chronojump_importer.py \
	     src/chronojump-importer/test-delete.py \
	     binariesMac/ffplay \
	     binariesMac/7zz

CLEANFILES = win32/Makefile win32/chronojump_innosetup.iss

ACLOCAL_AMFLAGS = -I build/m4 -I build/m4/shave -I build/m4/shamrock

#SUBDIRS = build chronopic-firmware \
#	  encoder r-scripts data src po manual
SUBDIRS = build encoder r-scripts src po manual

#if OSTYPE_LINUX
#SUBDIRS += chronopic-tests
#endif

if OSTYPE_WINDOWS
win32-installer: all install
	make -C win32 installer
endif

# Build ChangeLog from GIT history
ChangeLog:
	@if test -f $(top_srcdir)/.git/HEAD; then \
		git log --pretty=format:'%ad %an <%ae>%n%s ' --stat --after="Jul 01 23:47:57 2009" > $@; \
	fi

dist: ChangeLog

if OSTYPE_OS_X
SUBDIRS += binariesMac
export CHRONOJUMP_VERSION = @PACKAGE_VERSION@
export CHRONOJUMP_COMMIT = origin/@GIT_BRANCH@
CERBERO_COMMAND = /Users/chronojump/cerbero/cerbero-uninstalled -c /Users/chronojump/cerbero/cerbero-extras/chronojump.cbc
osx-installer: all install
	$(CERBERO_COMMAND) buildone chronojump
	$(CERBERO_COMMAND) package chronojump
endif


.PHONY: ChangeLog
