EXTRA_DIST = win32/chronojump_innosetup.iss.in win32/Makefile.in glade/chronojump.glade src/chronojump-importer/chronojump_importer.py
CLEANFILES = win32/Makefile win32/chronojump_innosetup.iss

ACLOCAL_AMFLAGS = -I build/m4 -I build/m4/shave -I build/m4/shamrock

SUBDIRS = build chronopic-firmware \
	 encoder libcesarplayer CesarPlayer src po manual chronojump_server

if OSTYPE_LINUX
SUBDIRS += chronopic-tests chronojump_server
endif

if OSTYPE_WINDOWS
win32-installer:
	make -C win32 installer
endif

# Build ChangeLog from GIT history
ChangeLog:
	@if test -f $(top_srcdir)/.git/HEAD; then \
		git log --pretty=format:'%ad %an <%ae>%n%s ' --stat --after="Jul 01 23:47:57 2009" > $@; \
	fi

dist: ChangeLog

.PHONY: ChangeLog
