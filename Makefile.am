EXTRA_DIST = m4 expansions.m4

ACLOCAL_AMFLAGS = -I m4

SUBDIRS = chronopic-firmware \
	libcesarplayer CesarPlayer src po manual

if OSTYPE_LINUX
 SUBDIRS += chronopic-tests chronojump_server
endif

if OSTYPE_WINDOWS
win32-bundle:
	make -C win32 bundle
endif

# Build ChangeLog from GIT history
ChangeLog:
	@if test -f $(top_srcdir)/.git/HEAD; then \
		git log --pretty=format:'%ad %an <%ae>%n%s ' --stat --after="Jul 01 23:47:57 2009" > $@; \
	fi

dist: ChangeLog

.PHONY: ChangeLog
