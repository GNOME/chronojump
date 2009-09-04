EXTRA_DIST =

##########libchronopic.so##########

AM_CPPFLAGS = \
	-DPACKAGE_SRC_DIR=\""$(srcdir)"\" \
	-DPACKAGE_DATA_DIR=\""$(datadir)"\" 

AM_CFLAGS =\
	 -Wall\
	 -g

pkglib_LTLIBRARIES = \
	libchronopic.la

libchronopic_la_SOURCES = \
	chronopic.h\
	chronopic.c


##########Test Accuracy##########

bin_PROGRAMS=chronojump-test-accuracy
chronojump_test_accuracy_SOURCES=test-precision.c
chronojump_test_accuracy_LDADD=libchronopic.la

##########Test Jumps##########

bin_PROGRAMS+=chronojump-test-jumps
chronojump_test_jumps_SOURCES=test-saltos.c
chronojump_test_jumps_LDADD=libchronopic.la

##########Test Stream##########

bin_PROGRAMS+=chronojump-test-stream
chronojump_test_stream_SOURCES=test-tramas.c termansi.h termansi.c
chronojump_test_stream_LDADD=libchronopic.la








