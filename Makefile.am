EXTRA_DIST = build/m4 expansions.m4

ACLOCAL_AMFLAGS = -I build/m4 -I build/m4/shave -I build/m4/shamrock

SUBDIRS = chronopic-firmware \
	encoder libcesarplayer CesarPlayer rdotnet src po manual chronojump_server

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
