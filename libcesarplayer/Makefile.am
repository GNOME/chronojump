## Process this file with automake to produce Makefile.in


AM_CPPFLAGS = \
	-DPACKAGE_SRC_DIR=\""$(srcdir)"\" \
	-DPACKAGE_DATA_DIR=\""$(datadir)"\" \
	$(CESARPLAYER_CFLAGS)

if OSTYPE_OS_X
AM_CPPFLAGS += \
	-x objective-c
endif

AM_CFLAGS =\
	 -Wall\
	 -g

BVWMARSHALFILES = baconvideowidget-marshal.c baconvideowidget-marshal.h
GLIB_GENMARSHAL=`pkg-config --variable=glib_genmarshal glib-2.0`
BUILT_SOURCES = $(BVWMARSHALFILES)

baconvideowidget-marshal.h: baconvideowidget-marshal.list
	( $(GLIB_GENMARSHAL) --prefix=baconvideowidget_marshal $(srcdir)/baconvideowidget-marshal.list --header > baconvideowidget-marshal.h )
baconvideowidget-marshal.c: baconvideowidget-marshal.h
	( $(GLIB_GENMARSHAL) --prefix=baconvideowidget_marshal $(srcdir)/baconvideowidget-marshal.list --body --header > baconvideowidget-marshal.c )


pkglib_LTLIBRARIES = \
	libcesarplayer.la

libcesarplayer_la_SOURCES = \
	$(BVWMARSHALFILES) \
	common.h\
	bacon-video-widget.h\
	bacon-video-widget-gst-0.10.c\
	gstscreenshot.c \
	gstscreenshot.h \
	gst-camera-capturer.c\
	gst-camera-capturer.h\
	gst-remuxer.c\
	gst-remuxer.h\
	gst-video-editor.c\
	gst-video-editor.h\
	video-utils.c\
	video-utils.h\
	macros.h

libcesarplayer_la_LDFLAGS = \
	$(CESARPLAYER_LIBS)

if OSTYPE_WINDOWS
libcesarplayer_la_LDFLAGS += -no-undefined
endif
	
CLEANFILES = $(BUILT_SOURCES)
	
EXTRA_DIST = \
	baconvideowidget-marshal.list

