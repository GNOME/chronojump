EXTRA_DIST = m4 expansions.m4

ACLOCAL_AMFLAGS = -I m4

if ENABLE_DEBUG
 SUBDIRS = chronopic-firmware chronojump_server chronopic-tests \
 	src po manual chronopic-firmware libcesarplayer CesarPlayer
endif
if ENABLE_RELEASE
 SUBDIRS = chronopic-firmware chronojump_server chronopic-tests \
 	libcesarplayer CesarPlayer src po manual
endif

# Build ChangeLog from GIT history
ChangeLog:
	@if test -f $(top_srcdir)/.git/HEAD; then \
		git log --pretty=format:'%ad %an <%ae>%n%s ' --stat --after="Jul 01 23:47:57 2009" > $@; \
	fi

dist: ChangeLog

.PHONY: ChangeLog
