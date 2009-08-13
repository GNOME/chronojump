EXTRA_DIST = 	build/data/linux_dlls/NPlot.dll\
  		build/data/linux_dlls/NPlot.Gtk.dll\
		build/data/linux_dlls/NPlot.dll.config\
		build/data/linux_dlls/NPlot.Gtk.dll.config

ACLOCAL_AMFLAGS = -I m4

if ENABLE_DEBUG
 SUBDIRS =  chronojump_server chronopic-tests  src po manual
endif
if ENABLE_RELEASE
 SUBDIRS = chronojump_server chronopic-tests  src po manual
endif
