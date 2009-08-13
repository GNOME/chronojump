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

bin_PROGRAMS=test-accuracy
test_accuracy_SOURCES=test-precision.c
test_accuracy_LDADD=libchronopic.la

##########Test Jumps##########

bin_PROGRAMS+=test-jumps
test_jumps_SOURCES=test-saltos.c
test_jumps_LDADD=libchronopic.la

##########Test Stream##########

bin_PROGRAMS+=test-stream
test_stream_SOURCES=test-tramas.c termansi.h termansi.c
test_stream_LDADD=libchronopic.la








