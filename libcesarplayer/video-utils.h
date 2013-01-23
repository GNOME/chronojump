/* 
 * Copyright (C) 2003-2007 the GStreamer project
 *      Julien Moutte <julien@moutte.net>
 *      Ronald Bultje <rbultje@ronald.bitfreak.net>
 * Copyright (C) 2005-2008 Tim-Philipp Müller <tim centricular net>
 * Copyright (C) 2009 Sebastian Dröge <sebastian.droege@collabora.co.uk>
 * Copyright (C) 2009  Andoni Morales Alastruey <ylatuya@gmail.com> 
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
 *
 * The Totem project hereby grant permission for non-gpl compatible GStreamer
 * plugins to be used and distributed together with GStreamer and Totem. This
 * permission is above and beyond the permissions granted by the GPL license
 * Totem is covered by.
 *
 */

#include <gst/gst.h>
#include <gst/interfaces/xoverlay.h>
#include <gdk/gdk.h>
#if defined (GDK_WINDOWING_X11)
#include <gdk/gdkx.h>
#elif defined (GDK_WINDOWING_WIN32)
#include <gdk/gdkwin32.h>
#elif defined (GDK_WINDOWING_QUARTZ)
#include <gdk/gdkquartz.h>
#endif
#include <gtk/gtk.h>


#ifdef WIN32
#define EXPORT __declspec (dllexport)
#else
#define EXPORT
#endif

#define TOTEM_OBJECT_HAS_SIGNAL(obj, name) (g_signal_lookup (name, g_type_from_name (G_OBJECT_TYPE_NAME (obj))) != 0)

void totem_gdk_window_set_invisible_cursor (GdkWindow * window);
void totem_gdk_window_set_waiting_cursor (GdkWindow * window);

gboolean totem_display_is_local (void);

char *totem_time_to_string (gint64 msecs);
gint64 totem_string_to_time (const char *time_string);
char *totem_time_to_string_text (gint64 msecs);

void totem_widget_set_preferred_size (GtkWidget * widget, gint width,
    gint height);
gboolean totem_ratio_fits_screen (GdkWindow * window, int video_width,
    int video_height, gfloat ratio);

void init_backend (int argc, char **argv);
guintptr gst_get_window_handle (GdkWindow *window);
void gst_set_window_handle (GstXOverlay *overlay, guintptr window_handle);
void init_debug();


