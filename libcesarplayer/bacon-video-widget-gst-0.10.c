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

/* GStreamer Interfaces */
#include <gst/interfaces/xoverlay.h>
#include <gst/interfaces/navigation.h>
#include <gst/interfaces/colorbalance.h>
/* for detecting sources of errors */
#include <gst/video/gstvideosink.h>
#include <gst/video/video.h>
#include <gst/audio/gstbaseaudiosink.h>
/* for pretty multichannel strings */
#include <gst/audio/multichannel.h>


/* for missing decoder/demuxer detection */
#include <gst/pbutils/pbutils.h>

/* for the cover metadata info */
#include <gst/tag/tag.h>


/* system */
#include <time.h>
#include <string.h>
#include <stdio.h>
#include <math.h>

/* gtk+/gnome */
#include <gdk/gdk.h>
#if defined (GDK_WINDOWING_X11)
#include <gdk/gdkx.h>
#elif defined (GDK_WINDOWING_WIN32)
#include <gdk/gdkwin32.h>
#elif defined (GDK_WINDOWING_QUARTZ)
#include <gdk/gdkquartz.h>
#endif
#include <gtk/gtk.h>


#include <gio/gio.h>
#include <glib/gi18n.h>


//#include <gconf/gconf-client.h>

#include "bacon-video-widget.h"
#include "baconvideowidget-marshal.h"
#include "common.h"
#include "gstscreenshot.h"
#include "video-utils.h"

#define DEFAULT_HEIGHT 420
#define DEFAULT_WIDTH  315

#define is_error(e, d, c) \
  (e->domain == GST_##d##_ERROR && \
   e->code == GST_##d##_ERROR_##c)

/* Signals */
enum
{
  SIGNAL_ERROR,
  SIGNAL_EOS,
  SIGNAL_SEGMENT_DONE,
  SIGNAL_REDIRECT,
  SIGNAL_TITLE_CHANGE,
  SIGNAL_CHANNELS_CHANGE,
  SIGNAL_TICK,
  SIGNAL_GOT_METADATA,
  SIGNAL_BUFFERING,
  SIGNAL_MISSING_PLUGINS,
  SIGNAL_STATE_CHANGE,
  SIGNAL_GOT_DURATION,
  SIGNAL_READY_TO_SEEK,
  LAST_SIGNAL
};

/* Properties */
enum
{
  PROP_0,
  PROP_LOGO_MODE,
  PROP_EXPAND_LOGO,
  PROP_POSITION,
  PROP_CURRENT_TIME,
  PROP_STREAM_LENGTH,
  PROP_PLAYING,
  PROP_SEEKABLE,
  PROP_SHOW_CURSOR,
  PROP_MEDIADEV,
  PROP_VOLUME
};

/* GstPlayFlags flags from playbin2 */
typedef enum
{
  GST_PLAY_FLAGS_VIDEO = 0x01,
  GST_PLAY_FLAGS_AUDIO = 0x02,
  GST_PLAY_FLAGS_TEXT = 0x04,
  GST_PLAY_FLAGS_VIS = 0x08,
  GST_PLAY_FLAGS_SOFT_VOLUME = 0x10,
  GST_PLAY_FLAGS_NATIVE_AUDIO = 0x20,
  GST_PLAY_FLAGS_NATIVE_VIDEO = 0x40
} GstPlayFlags;


struct BaconVideoWidgetPrivate
{
  BvwAspectRatio ratio_type;

  char *mrl;

  GstElement *play;
  GstElement *video_sink;
  GstXOverlay *xoverlay;        /* protect with lock */
  GstColorBalance *balance;     /* protect with lock */
  GstNavigation *navigation;    /* protect with lock */
  guint interface_update_id;    /* protect with lock */
  GMutex *lock;

  guint update_id;

  GdkPixbuf *logo_pixbuf;
  GdkPixbuf *drawing_pixbuf;

  gboolean media_has_video;
  gboolean media_has_audio;
  gint seekable;                /* -1 = don't know, FALSE = no */
  gint64 stream_length;
  gint64 current_time_nanos;
  gint64 current_time;
  gfloat current_position;
  gboolean is_live;

  GstTagList *tagcache;
  GstTagList *audiotags;
  GstTagList *videotags;

  gboolean got_redirect;

  GtkWidget *video_da;
  GtkWidget *logo_da;
  guintptr window_handle;
  GdkCursor *cursor;


  /* Other stuff */
  gboolean logo_mode;
  gboolean drawing_mode;
  gboolean expand_logo;
  gboolean cursor_shown;
  gboolean fullscreen_mode;
  gboolean auto_resize;
  gboolean uses_fakesink;

  gint video_width;             /* Movie width */
  gint video_height;            /* Movie height */
  gboolean window_resized;      /* Whether the window has already been resized
                                   for this media */
  const GValue *movie_par;      /* Movie pixel aspect ratio */
  gint video_width_pixels;      /* Scaled movie width */
  gint video_height_pixels;     /* Scaled movie height */
  gint video_fps_n;
  gint video_fps_d;

  gdouble zoom;

  GstElement *audio_capsfilter;

  BvwAudioOutType speakersetup;
  gint connection_speed;

  gchar *media_device;


  GstMessageType ignore_messages_mask;

  GstBus *bus;
  gulong sig_bus_sync;
  gulong sig_bus_async;

  BvwUseType use_type;

  gint eos_id;

  /* state we want to be in, as opposed to actual pipeline state
   * which may change asynchronously or during buffering */
  GstState target_state;
  gboolean buffering;

  /* for easy codec installation */
  GList *missing_plugins;       /* GList of GstMessages */
  gboolean plugin_install_in_progress;

};

static void bacon_video_widget_set_property (GObject * object,
    guint property_id, const GValue * value, GParamSpec * pspec);
static void bacon_video_widget_get_property (GObject * object,
    guint property_id, GValue * value, GParamSpec * pspec);

static void bacon_video_widget_finalize (GObject * object);
static void bvw_process_pending_tag_messages (BaconVideoWidget * bvw);
static void bvw_stop_play_pipeline (BaconVideoWidget * bvw);
static GError *bvw_error_from_gst_error (BaconVideoWidget * bvw,
    GstMessage * m);



static GtkWidgetClass *parent_class = NULL;

static GThread *gui_thread;

static int bvw_signals[LAST_SIGNAL] = { 0 };


typedef gchar *(*MsgToStrFunc) (GstMessage * msg);

static gchar **
bvw_get_missing_plugins_foo (const GList * missing_plugins, MsgToStrFunc func)
{
  GPtrArray *arr = g_ptr_array_new ();

  while (missing_plugins != NULL) {
    g_ptr_array_add (arr, func (GST_MESSAGE (missing_plugins->data)));
    missing_plugins = missing_plugins->next;
  }
  g_ptr_array_add (arr, NULL);
  return (gchar **) g_ptr_array_free (arr, FALSE);
}

static gchar **
bvw_get_missing_plugins_details (const GList * missing_plugins)
{
  return bvw_get_missing_plugins_foo (missing_plugins,
      gst_missing_plugin_message_get_installer_detail);
}

static gchar **
bvw_get_missing_plugins_descriptions (const GList * missing_plugins)
{
  return bvw_get_missing_plugins_foo (missing_plugins,
      gst_missing_plugin_message_get_description);
}

static void
bvw_clear_missing_plugins_messages (BaconVideoWidget * bvw)
{
  g_list_foreach (bvw->priv->missing_plugins,
      (GFunc) gst_mini_object_unref, NULL);
  g_list_free (bvw->priv->missing_plugins);
  bvw->priv->missing_plugins = NULL;
}

static void
bvw_check_if_video_decoder_is_missing (BaconVideoWidget * bvw)
{
  GList *l;

  if (bvw->priv->media_has_video || bvw->priv->missing_plugins == NULL)
    return;

  for (l = bvw->priv->missing_plugins; l != NULL; l = l->next) {
    GstMessage *msg = GST_MESSAGE (l->data);
    gchar *d, *f;

    if ((d = gst_missing_plugin_message_get_installer_detail (msg))) {
      if ((f = strstr (d, "|decoder-")) && strstr (f, "video")) {
        GError *err;

        /* create a fake GStreamer error so we get a nice warning message */
        err = g_error_new (GST_CORE_ERROR, GST_CORE_ERROR_MISSING_PLUGIN, "x");
        msg = gst_message_new_error (GST_OBJECT (bvw->priv->play), err, NULL);
        g_error_free (err);
        err = bvw_error_from_gst_error (bvw, msg);
        gst_message_unref (msg);
        g_signal_emit (bvw, bvw_signals[SIGNAL_ERROR], 0, err->message);
        g_error_free (err);
        g_free (d);
        break;
      }
      g_free (d);
    }
  }
}

static void
bvw_error_msg (BaconVideoWidget * bvw, GstMessage * msg)
{
  GError *err = NULL;
  gchar *dbg = NULL;

  GST_DEBUG_BIN_TO_DOT_FILE (GST_BIN_CAST (bvw->priv->play),
      GST_DEBUG_GRAPH_SHOW_ALL ^
      GST_DEBUG_GRAPH_SHOW_NON_DEFAULT_PARAMS, "totem-error");

  gst_message_parse_error (msg, &err, &dbg);
  if (err) {
    GST_ERROR ("message = %s", GST_STR_NULL (err->message));
    GST_ERROR ("domain  = %d (%s)", err->domain,
        GST_STR_NULL (g_quark_to_string (err->domain)));
    GST_ERROR ("code    = %d", err->code);
    GST_ERROR ("debug   = %s", GST_STR_NULL (dbg));
    GST_ERROR ("source  = %" GST_PTR_FORMAT, msg->src);
    GST_ERROR ("uri     = %s", GST_STR_NULL (bvw->priv->mrl));

    g_message ("Error: %s\n%s\n", GST_STR_NULL (err->message),
        GST_STR_NULL (dbg));

    g_error_free (err);
  }
  g_free (dbg);
}

static void
bacon_video_widget_realize_event (GtkWidget * widget, BaconVideoWidget *bvw)
{
  GdkWindow *window = gtk_widget_get_window (widget);

  if (!gdk_window_ensure_native (window))
    g_error ("Couldn't create native window needed for GstXOverlay!");

  bvw->priv->window_handle = gst_get_window_handle (window);
}

static gboolean
bacon_video_widget_logo_expose_event (GtkWidget * widget, GdkEventExpose * event,
    BaconVideoWidget *bvw)
{
  gboolean draw_logo;
  GdkWindow *win;

  if (event && event->count > 0)
    return TRUE;

  if (event == NULL)
    return TRUE;

  g_mutex_lock (bvw->priv->lock);

  /* if there's only audio and no visualisation, draw the logo as well */
  draw_logo = bvw->priv->media_has_audio && !bvw->priv->media_has_video;

  if (!bvw->priv->logo_mode && !draw_logo)
    goto exit;

  win = gtk_widget_get_window (bvw->priv->logo_da);

  /* Start with a nice black canvas */
  gdk_draw_rectangle (win, gtk_widget_get_style (widget)->black_gc, TRUE, 0,
      0, widget->allocation.width, widget->allocation.height);

  if (bvw->priv->logo_pixbuf != NULL) {
    GdkPixbuf *frame;
    GdkPixbuf *drawing;
    guchar *pixels;
    int rowstride;
    gint width, height, alloc_width, alloc_height, logo_x, logo_y;
    gfloat ratio;

    /* Checking if allocated space is smaller than our logo */


    width = gdk_pixbuf_get_width (bvw->priv->logo_pixbuf);
    height = gdk_pixbuf_get_height (bvw->priv->logo_pixbuf);
    alloc_width = widget->allocation.width;
    alloc_height = widget->allocation.height;

    if ((gfloat) alloc_width / width > (gfloat) alloc_height / height) {
      ratio = (gfloat) alloc_height / height;
    } else {
      ratio = (gfloat) alloc_width / width;
    }

    width *= ratio;
    height *= ratio;

    logo_x = (alloc_width / 2) - (width / 2);
    logo_y = (alloc_height / 2) - (height / 2);


    /* Drawing our frame */

    if (bvw->priv->expand_logo && !bvw->priv->drawing_mode) {
      /* Scaling to available space */

      frame = gdk_pixbuf_new (GDK_COLORSPACE_RGB,
          FALSE, 8, widget->allocation.width, widget->allocation.height);

      gdk_pixbuf_composite (bvw->priv->logo_pixbuf,
          frame,
          0, 0,
          alloc_width, alloc_height,
          logo_x, logo_y, ratio, ratio, GDK_INTERP_BILINEAR, 255);

      rowstride = gdk_pixbuf_get_rowstride (frame);

      pixels = gdk_pixbuf_get_pixels (frame) +
          rowstride * event->area.y + event->area.x * 3;

      gdk_draw_rgb_image_dithalign (widget->window,
          widget->style->black_gc,
          event->area.x, event->area.y,
          event->area.width,
          event->area.height,
          GDK_RGB_DITHER_NORMAL, pixels,
          rowstride, event->area.x, event->area.y);

      g_object_unref (frame);
    } else {
      if (width <= 1 || height <= 1) {
        gdk_window_end_paint (win);
        goto exit;
      }

      frame = gdk_pixbuf_scale_simple (bvw->priv->logo_pixbuf,
          width, height, GDK_INTERP_BILINEAR);
      gdk_draw_pixbuf (win, gtk_widget_get_style (widget)->fg_gc[0],
          frame, 0, 0, logo_x, logo_y, width, height,
          GDK_RGB_DITHER_NONE, 0, 0);

      if (bvw->priv->drawing_mode && bvw->priv->drawing_pixbuf != NULL) {
        drawing =
            gdk_pixbuf_scale_simple (bvw->priv->drawing_pixbuf, width,
            height, GDK_INTERP_BILINEAR);
        gdk_draw_pixbuf (win,
            gtk_widget_get_style (widget)->fg_gc[0],
            drawing, 0, 0, logo_x, logo_y, width,
            height, GDK_RGB_DITHER_NONE, 0, 0);
        g_object_unref (drawing);
      }

      g_object_unref (frame);
    }
  } else if (win) {
    /* No pixbuf, just draw a black background then */
    gdk_window_clear_area (win,
        0, 0, widget->allocation.width, widget->allocation.height);
  }

exit:
  g_mutex_unlock (bvw->priv->lock);
  return TRUE;
}

static gboolean
bacon_video_widget_video_expose_event (GtkWidget * widget, GdkEventExpose * event,
    BaconVideoWidget *bvw)
{
  GstXOverlay *xoverlay;
  GdkWindow *win;

  if (event && event->count > 0)
    return TRUE;

  if (event == NULL)
    return TRUE;

  g_mutex_lock (bvw->priv->lock);

  xoverlay = bvw->priv->xoverlay;
  if (xoverlay != NULL) {
    gst_object_ref (xoverlay);
    gst_set_window_handle (xoverlay, bvw->priv->window_handle);
  }

  if (bvw->priv->logo_mode) {
#if !defined (GDK_WINDOWING_QUARTZ)
    g_mutex_unlock (bvw->priv->lock);
    bacon_video_widget_logo_expose_event (widget, event, bvw);
    g_mutex_lock (bvw->priv->lock);
#endif
    goto exit;
  }

  /* no logo, pass the expose to gst */
  if (xoverlay != NULL && GST_IS_X_OVERLAY (xoverlay)){
    gst_x_overlay_expose (xoverlay);
  }
  else {
    /* No xoverlay to expose yet */
    win = gtk_widget_get_window (bvw->priv->video_da);
    gdk_window_clear_area (win,
      0, 0, widget->allocation.width, widget->allocation.height);
  }

exit:

  if (xoverlay != NULL)
    gst_object_unref (xoverlay);

  g_mutex_unlock (bvw->priv->lock);
  return TRUE;
}

static gboolean
bvw_boolean_handled_accumulator (GSignalInvocationHint * ihint,
    GValue * return_accu, const GValue * handler_return, gpointer foobar)
{
  gboolean continue_emission;
  gboolean signal_handled;

  signal_handled = g_value_get_boolean (handler_return);
  g_value_set_boolean (return_accu, signal_handled);
  continue_emission = !signal_handled;

  return continue_emission;
}

static void
bacon_video_widget_class_init (BaconVideoWidgetClass * klass)
{
  GObjectClass *object_class;

  object_class = (GObjectClass *) klass;

  parent_class = g_type_class_peek_parent (klass);

  g_type_class_add_private (object_class, sizeof (BaconVideoWidgetPrivate));

  /* GObject */
  object_class->set_property = bacon_video_widget_set_property;
  object_class->get_property = bacon_video_widget_get_property;
  object_class->finalize = bacon_video_widget_finalize;

  /* Properties */
  g_object_class_install_property (object_class, PROP_LOGO_MODE,
      g_param_spec_boolean ("logo_mode", NULL, NULL, FALSE, G_PARAM_READWRITE));
  g_object_class_install_property (object_class, PROP_EXPAND_LOGO,
      g_param_spec_boolean ("expand_logo", NULL,
          NULL, TRUE, G_PARAM_READWRITE));
  g_object_class_install_property (object_class, PROP_POSITION,
      g_param_spec_int ("position", NULL, NULL,
          0, G_MAXINT, 0, G_PARAM_READABLE));
  g_object_class_install_property (object_class, PROP_STREAM_LENGTH,
      g_param_spec_int64 ("stream_length", NULL,
          NULL, 0, G_MAXINT64, 0, G_PARAM_READABLE));
  g_object_class_install_property (object_class, PROP_PLAYING,
      g_param_spec_boolean ("playing", NULL, NULL, FALSE, G_PARAM_READABLE));
  g_object_class_install_property (object_class, PROP_SEEKABLE,
      g_param_spec_boolean ("seekable", NULL, NULL, FALSE, G_PARAM_READABLE));
  g_object_class_install_property (object_class, PROP_VOLUME,
      g_param_spec_int ("volume", NULL, NULL, 0, 100, 0, G_PARAM_READABLE));
  g_object_class_install_property (object_class, PROP_SHOW_CURSOR,
      g_param_spec_boolean ("showcursor", NULL,
          NULL, FALSE, G_PARAM_READWRITE));
  g_object_class_install_property (object_class, PROP_MEDIADEV,
      g_param_spec_string ("mediadev", NULL, NULL, FALSE, G_PARAM_READWRITE));


  /* Signals */
  bvw_signals[SIGNAL_ERROR] =
      g_signal_new ("error",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (BaconVideoWidgetClass, error),
      NULL, NULL,
      g_cclosure_marshal_VOID__STRING, G_TYPE_NONE, 1, G_TYPE_STRING);

  bvw_signals[SIGNAL_EOS] =
      g_signal_new ("eos",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (BaconVideoWidgetClass, eos),
      NULL, NULL, g_cclosure_marshal_VOID__VOID, G_TYPE_NONE, 0);

  bvw_signals[SIGNAL_SEGMENT_DONE] =
      g_signal_new ("segment_done",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (BaconVideoWidgetClass, segment_done),
      NULL, NULL, g_cclosure_marshal_VOID__VOID, G_TYPE_NONE, 0);

  bvw_signals[SIGNAL_READY_TO_SEEK] =
      g_signal_new ("ready_to_seek",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (BaconVideoWidgetClass, ready_to_seek),
      NULL, NULL, g_cclosure_marshal_VOID__VOID, G_TYPE_NONE, 0);

  bvw_signals[SIGNAL_GOT_DURATION] =
      g_signal_new ("got_duration",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (BaconVideoWidgetClass, got_duration),
      NULL, NULL, g_cclosure_marshal_VOID__VOID, G_TYPE_NONE, 0);

  bvw_signals[SIGNAL_GOT_METADATA] =
      g_signal_new ("got-metadata",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (BaconVideoWidgetClass, got_metadata),
      NULL, NULL, g_cclosure_marshal_VOID__VOID, G_TYPE_NONE, 0);

  bvw_signals[SIGNAL_REDIRECT] =
      g_signal_new ("got-redirect",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (BaconVideoWidgetClass, got_redirect),
      NULL, NULL, g_cclosure_marshal_VOID__STRING,
      G_TYPE_NONE, 1, G_TYPE_STRING);

  bvw_signals[SIGNAL_TITLE_CHANGE] =
      g_signal_new ("title-change",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (BaconVideoWidgetClass, title_change),
      NULL, NULL,
      g_cclosure_marshal_VOID__STRING, G_TYPE_NONE, 1, G_TYPE_STRING);

  bvw_signals[SIGNAL_CHANNELS_CHANGE] =
      g_signal_new ("channels-change",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (BaconVideoWidgetClass, channels_change),
      NULL, NULL, g_cclosure_marshal_VOID__VOID, G_TYPE_NONE, 0);

  bvw_signals[SIGNAL_TICK] =
      g_signal_new ("tick",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (BaconVideoWidgetClass, tick),
      NULL, NULL,
      baconvideowidget_marshal_VOID__INT64_INT64_FLOAT_BOOLEAN,
      G_TYPE_NONE, 4, G_TYPE_INT64, G_TYPE_INT64, G_TYPE_FLOAT, G_TYPE_BOOLEAN);

  bvw_signals[SIGNAL_BUFFERING] =
      g_signal_new ("buffering",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (BaconVideoWidgetClass, buffering),
      NULL, NULL, g_cclosure_marshal_VOID__INT, G_TYPE_NONE, 1, G_TYPE_INT);

  bvw_signals[SIGNAL_STATE_CHANGE] =
      g_signal_new ("state_change",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (BaconVideoWidgetClass, state_change),
      NULL, NULL,
      g_cclosure_marshal_VOID__BOOLEAN, G_TYPE_NONE, 1, G_TYPE_BOOLEAN);

  /* missing plugins signal:
   *  - string array: details of missing plugins for libgimme-codec
   *  - string array: details of missing plugins (human-readable strings)
   *  - bool: if we managed to start playing something even without those plugins
   *  return value: callback must return TRUE to indicate that it took some
   *                action, FALSE will be interpreted as no action taken
   */
  bvw_signals[SIGNAL_MISSING_PLUGINS] = g_signal_new ("missing-plugins", G_TYPE_FROM_CLASS (object_class), G_SIGNAL_RUN_LAST, 0,        /* signal is enough, we don't need a vfunc */
      bvw_boolean_handled_accumulator,
      NULL,
      baconvideowidget_marshal_BOOLEAN__BOXED_BOXED_BOOLEAN,
      G_TYPE_BOOLEAN, 3, G_TYPE_STRV, G_TYPE_STRV, G_TYPE_BOOLEAN);
}

static void
bacon_video_widget_init (BaconVideoWidget * bvw)
{
  BaconVideoWidgetPrivate *priv;

  GTK_WIDGET_SET_FLAGS (GTK_WIDGET (bvw), GTK_CAN_FOCUS);

  bvw->priv = priv =
      G_TYPE_INSTANCE_GET_PRIVATE (bvw, BACON_TYPE_VIDEO_WIDGET,
      BaconVideoWidgetPrivate);

  priv->update_id = 0;
  priv->tagcache = NULL;
  priv->audiotags = NULL;
  priv->videotags = NULL;
  priv->zoom = 1.0;
  priv->lock = g_mutex_new ();

  bvw->priv->missing_plugins = NULL;
  bvw->priv->plugin_install_in_progress = FALSE;

  bvw->priv->video_da = gtk_drawing_area_new ();
  gtk_box_pack_start (GTK_BOX (bvw), bvw->priv->video_da, TRUE, TRUE, 0);
  gtk_widget_show (bvw->priv->video_da);
  GTK_WIDGET_UNSET_FLAGS (GTK_WIDGET (bvw->priv->video_da), GTK_DOUBLE_BUFFERED);

#if defined (GDK_WINDOWING_QUARTZ)
  bvw->priv->logo_da = gtk_drawing_area_new ();
  gtk_box_pack_start (GTK_BOX (bvw), bvw->priv->logo_da, TRUE, TRUE, 0);
  gtk_widget_show (bvw->priv->logo_da);

  g_signal_connect (GTK_WIDGET (bvw->priv->logo_da), "expose-event",
      G_CALLBACK (bacon_video_widget_logo_expose_event), bvw);
#else
  bvw->priv->logo_da = bvw->priv->video_da;
#endif

  gtk_widget_add_events (GTK_WIDGET (bvw->priv->video_da),
      GDK_POINTER_MOTION_MASK | GDK_BUTTON_PRESS_MASK | GDK_BUTTON_RELEASE_MASK
      | GDK_KEY_PRESS_MASK | GDK_KEY_RELEASE_MASK);

  g_signal_connect (GTK_WIDGET (bvw->priv->video_da), "realize",
      G_CALLBACK (bacon_video_widget_realize_event), bvw);

  g_signal_connect (GTK_WIDGET (bvw->priv->video_da), "expose-event",
      G_CALLBACK (bacon_video_widget_video_expose_event), bvw);

  bacon_video_widget_set_logo_mode (bvw, TRUE);
}

static gboolean bvw_query_timeout (BaconVideoWidget * bvw);
static void parse_stream_info (BaconVideoWidget * bvw);

static void
bvw_update_stream_info (BaconVideoWidget * bvw)
{
  parse_stream_info (bvw);

  /* if we're not interactive, we want to announce metadata
   * only later when we can be sure we got it all */
  if (bvw->priv->use_type == BVW_USE_TYPE_VIDEO ||
      bvw->priv->use_type == BVW_USE_TYPE_AUDIO) {
    g_signal_emit (bvw, bvw_signals[SIGNAL_GOT_METADATA], 0, NULL);
    g_signal_emit (bvw, bvw_signals[SIGNAL_CHANNELS_CHANGE], 0);
  }
}

static void
bvw_handle_application_message (BaconVideoWidget * bvw, GstMessage * msg)
{
  const gchar *msg_name;

  msg_name = gst_structure_get_name (msg->structure);
  g_return_if_fail (msg_name != NULL);

  GST_DEBUG ("Handling application message: %" GST_PTR_FORMAT, msg->structure);

  if (strcmp (msg_name, "stream-changed") == 0) {
    bvw_update_stream_info (bvw);
  } else {
    GST_DEBUG ("Unhandled application message %s", msg_name);
  }
}

static void
bvw_handle_element_message (BaconVideoWidget * bvw, GstMessage * msg)
{
  const gchar *type_name = NULL;
  gchar *src_name;

  src_name = gst_object_get_name (msg->src);
  if (msg->structure)
    type_name = gst_structure_get_name (msg->structure);

  GST_DEBUG ("from %s: %" GST_PTR_FORMAT, src_name, msg->structure);

  if (type_name == NULL)
    goto unhandled;

  if (strcmp (type_name, "redirect") == 0) {
    const gchar *new_location;

    new_location = gst_structure_get_string (msg->structure, "new-location");
    GST_DEBUG ("Got redirect to '%s'", GST_STR_NULL (new_location));

    if (new_location && *new_location) {
      g_signal_emit (bvw, bvw_signals[SIGNAL_REDIRECT], 0, new_location);
      goto done;
    }
  } else if (strcmp (type_name, "progress") == 0) {
    /* this is similar to buffering messages, but shouldn't affect pipeline
     * state; qtdemux emits those when headers are after movie data and
     * it is in streaming mode and has to receive all the movie data first */
    if (!bvw->priv->buffering) {
      gint percent = 0;

      if (gst_structure_get_int (msg->structure, "percent", &percent))
        g_signal_emit (bvw, bvw_signals[SIGNAL_BUFFERING], 0, percent);
    }
    goto done;
  } else if (strcmp (type_name, "prepare-xwindow-id") == 0 ||
      strcmp (type_name, "have-xwindow-id") == 0) {
    /* we handle these synchronously or want to ignore them */
    goto done;
  } else if (gst_is_missing_plugin_message (msg)) {
    bvw->priv->missing_plugins =
        g_list_prepend (bvw->priv->missing_plugins, gst_message_ref (msg));
    goto done;
  } else {
#if 0
    GstNavigationMessageType nav_msg_type =
        gst_navigation_message_get_type (msg);

    switch (nav_msg_type) {
      case GST_NAVIGATION_MESSAGE_MOUSE_OVER:
      {
        gint active;
        if (!gst_navigation_message_parse_mouse_over (msg, &active))
          break;
        if (active) {
          if (bvw->priv->cursor == NULL) {
            bvw->priv->cursor = gdk_cursor_new (GDK_HAND2);
          }
        } else {
          if (bvw->priv->cursor != NULL) {
            gdk_cursor_unref (bvw->priv->cursor);
            bvw->priv->cursor = NULL;
          }
        }
        gdk_window_set_cursor (gtk_widget_get_window (GTK_WIDGET (bvw)),
            bvw->priv->cursor);
        break;
      }
      default:
        break;
    }
#endif
  }

unhandled:
  GST_WARNING ("Unhandled element message %s from %s: %" GST_PTR_FORMAT,
      GST_STR_NULL (type_name), GST_STR_NULL (src_name), msg);

done:
  g_free (src_name);
}

/* This is a hack to avoid doing poll_for_state_change() indirectly
 * from the bus message callback (via EOS => totem => close => wait for ready)
 * and deadlocking there. We need something like a
 * gst_bus_set_auto_flushing(bus, FALSE) ... */
static gboolean
bvw_signal_eos_delayed (gpointer user_data)
{
  BaconVideoWidget *bvw = BACON_VIDEO_WIDGET (user_data);
  g_signal_emit (bvw, bvw_signals[SIGNAL_EOS], 0, NULL);
  bvw->priv->eos_id = 0;
  return FALSE;
}

static void
bvw_reconfigure_tick_timeout (BaconVideoWidget * bvw, guint msecs)
{
  if (bvw->priv->update_id != 0) {
    GST_INFO ("removing tick timeout");
    g_source_remove (bvw->priv->update_id);
    bvw->priv->update_id = 0;
  }
  if (msecs > 0) {
    GST_INFO ("adding tick timeout (at %ums)", msecs);
    bvw->priv->update_id =
        g_timeout_add (msecs, (GSourceFunc) bvw_query_timeout, bvw);
  }
}

/* returns TRUE if the error/signal has been handled and should be ignored */
static gboolean
bvw_emit_missing_plugins_signal (BaconVideoWidget * bvw, gboolean prerolled)
{
  gboolean handled = FALSE;
  gchar **descriptions, **details;

  details = bvw_get_missing_plugins_details (bvw->priv->missing_plugins);
  descriptions =
      bvw_get_missing_plugins_descriptions (bvw->priv->missing_plugins);

  GST_LOG ("emitting missing-plugins signal (prerolled=%d)", prerolled);

  g_signal_emit (bvw, bvw_signals[SIGNAL_MISSING_PLUGINS], 0,
      details, descriptions, prerolled, &handled);
  GST_DEBUG ("missing-plugins signal was %shandled", (handled) ? "" : "not ");

  g_strfreev (descriptions);
  g_strfreev (details);

  if (handled) {
    bvw->priv->plugin_install_in_progress = TRUE;
    bvw_clear_missing_plugins_messages (bvw);
  }

  /* if it wasn't handled, we might need the list of missing messages again
   * later to create a proper error message with details of what's missing */

  return handled;
}


/* returns TRUE if the error has been handled and should be ignored */
static gboolean
bvw_check_missing_plugins_error (BaconVideoWidget * bvw, GstMessage * err_msg)
{
  gboolean error_src_is_playbin;
  gboolean ret = FALSE;
  GError *err = NULL;

  if (bvw->priv->missing_plugins == NULL) {
    GST_DEBUG ("no missing-plugin messages");
    return FALSE;
  }

  gst_message_parse_error (err_msg, &err, NULL);

  error_src_is_playbin = (err_msg->src == GST_OBJECT_CAST (bvw->priv->play));

  /* If we get a WRONG_TYPE error from playbin itself it's most likely because
   * there is a subtitle stream we can decode, but no video stream to overlay
   * it on. Since there were missing-plugins messages, we'll assume this is
   * because we cannot decode the video stream (this should probably be fixed
   * in playbin, but for now we'll work around it here) */
  if (is_error (err, CORE, MISSING_PLUGIN) ||
      is_error (err, STREAM, CODEC_NOT_FOUND) ||
      (is_error (err, STREAM, WRONG_TYPE) && error_src_is_playbin)) {
    ret = bvw_emit_missing_plugins_signal (bvw, FALSE);
    if (ret) {
      /* If it was handled, stop playback to make sure we're not processing any
       * other error messages that might also be on the bus */
      bacon_video_widget_stop (bvw);
    }
  } else {
    GST_DEBUG ("not an error code we are looking for, doing nothing");
  }

  g_error_free (err);
  return ret;
}

/* returns TRUE if the error/signal has been handled and should be ignored */
static gboolean
bvw_check_missing_plugins_on_preroll (BaconVideoWidget * bvw)
{
  if (bvw->priv->missing_plugins == NULL) {
    GST_DEBUG ("no missing-plugin messages");
    return FALSE;
  }

  return bvw_emit_missing_plugins_signal (bvw, TRUE);
}

static void
bvw_bus_message_cb (GstBus * bus, GstMessage * message, gpointer data)
{
  BaconVideoWidget *bvw = (BaconVideoWidget *) data;
  GstMessageType msg_type;

  g_return_if_fail (bvw != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));

  msg_type = GST_MESSAGE_TYPE (message);

  /* somebody else is handling the message, probably in poll_for_state_change */
  if (bvw->priv->ignore_messages_mask & msg_type) {
    GST_LOG ("Ignoring %s message from element %" GST_PTR_FORMAT
        " as requested: %" GST_PTR_FORMAT,
        GST_MESSAGE_TYPE_NAME (message), message->src, message);
    return;
  }

  if (msg_type != GST_MESSAGE_STATE_CHANGED) {
    gchar *src_name = gst_object_get_name (message->src);
    GST_LOG ("Handling %s message from element %s",
        gst_message_type_get_name (msg_type), src_name);
    g_free (src_name);
  }

  switch (msg_type) {
    case GST_MESSAGE_ERROR:
    {
      bvw_error_msg (bvw, message);

      if (!bvw_check_missing_plugins_error (bvw, message)) {
        GError *error;

        error = bvw_error_from_gst_error (bvw, message);

        bvw->priv->target_state = GST_STATE_NULL;
        if (bvw->priv->play)
          gst_element_set_state (bvw->priv->play, GST_STATE_NULL);

        bvw->priv->buffering = FALSE;

        g_signal_emit (bvw, bvw_signals[SIGNAL_ERROR], 0,
            error->message, TRUE, FALSE);

        g_error_free (error);
      }
      break;
    }
    case GST_MESSAGE_WARNING:
    {
      GST_WARNING ("Warning message: %" GST_PTR_FORMAT, message);
      break;
    }
    case GST_MESSAGE_TAG:
    {
      GstTagList *tag_list, *result;
      GstElementFactory *f;

      gst_message_parse_tag (message, &tag_list);

      GST_DEBUG ("Tags: %" GST_PTR_FORMAT, tag_list);

      /* all tags (replace previous tags, title/artist/etc. might change
       * in the middle of a stream, e.g. with radio streams) */
      result = gst_tag_list_merge (bvw->priv->tagcache, tag_list,
          GST_TAG_MERGE_REPLACE);
      if (bvw->priv->tagcache)
        gst_tag_list_free (bvw->priv->tagcache);
      bvw->priv->tagcache = result;

      /* media-type-specific tags */
      if (GST_IS_ELEMENT (message->src) &&
          (f = gst_element_get_factory (GST_ELEMENT (message->src)))) {
        const gchar *klass = gst_element_factory_get_klass (f);
        GstTagList **cache = NULL;

        if (g_strrstr (klass, "Video")) {
          cache = &bvw->priv->videotags;
        } else if (g_strrstr (klass, "Audio")) {
          cache = &bvw->priv->audiotags;
        }

        if (cache) {
          result = gst_tag_list_merge (*cache, tag_list, GST_TAG_MERGE_REPLACE);
          if (*cache)
            gst_tag_list_free (*cache);
          *cache = result;
        }
      }

      /* clean up */
      gst_tag_list_free (tag_list);

      /* if we're not interactive, we want to announce metadata
       * only later when we can be sure we got it all */
      if (bvw->priv->use_type == BVW_USE_TYPE_VIDEO ||
          bvw->priv->use_type == BVW_USE_TYPE_AUDIO) {
        /* If we updated metadata and we have a new title, send it
         * using TITLE_CHANGE, so that the UI knows it has a new
         * streaming title */
        GValue value = { 0, };

        g_signal_emit (bvw, bvw_signals[SIGNAL_GOT_METADATA], 0);

        bacon_video_widget_get_metadata (bvw, BVW_INFO_TITLE, &value);
        if (g_value_get_string (&value))
          g_signal_emit (bvw, bvw_signals[SIGNAL_TITLE_CHANGE], 0,
              g_value_get_string (&value));
        g_value_unset (&value);
      }
      break;
    }
    case GST_MESSAGE_EOS:
      GST_DEBUG ("EOS message");
      /* update slider one last time */
      bvw_query_timeout (bvw);
      if (bvw->priv->eos_id == 0)
        bvw->priv->eos_id = g_idle_add (bvw_signal_eos_delayed, bvw);
      break;
    case GST_MESSAGE_BUFFERING:
    {
      gint percent = 0;

      /* FIXME: use gst_message_parse_buffering() once core 0.10.11 is out */
      gst_structure_get_int (message->structure, "buffer-percent", &percent);
      g_signal_emit (bvw, bvw_signals[SIGNAL_BUFFERING], 0, percent);

      if (percent >= 100) {
        /* a 100% message means buffering is done */
        bvw->priv->buffering = FALSE;
        /* if the desired state is playing, go back */
        if (bvw->priv->target_state == GST_STATE_PLAYING) {
          GST_DEBUG ("Buffering done, setting pipeline back to PLAYING");
          gst_element_set_state (bvw->priv->play, GST_STATE_PLAYING);
        } else {
          GST_DEBUG ("Buffering done, keeping pipeline PAUSED");
        }
      } else if (bvw->priv->buffering == FALSE &&
          bvw->priv->target_state == GST_STATE_PLAYING) {
        GstState cur_state;

        gst_element_get_state (bvw->priv->play, &cur_state, NULL, 0);
        if (cur_state == GST_STATE_PLAYING) {
          GST_DEBUG ("Buffering ... temporarily pausing playback");
          gst_element_set_state (bvw->priv->play, GST_STATE_PAUSED);
        } else {
          GST_DEBUG ("Buffering ... prerolling, not doing anything");
        }
        bvw->priv->buffering = TRUE;
      } else {
        GST_LOG ("Buffering ... %d", percent);
      }
      break;
    }
    case GST_MESSAGE_APPLICATION:
    {
      bvw_handle_application_message (bvw, message);
      break;
    }
    case GST_MESSAGE_STATE_CHANGED:
    {
      GstState old_state, new_state;
      gchar *src_name;

      gst_message_parse_state_changed (message, &old_state, &new_state, NULL);

      if (old_state == new_state)
        break;

      /* we only care about playbin (pipeline) state changes */
      if (GST_MESSAGE_SRC (message) != GST_OBJECT (bvw->priv->play))
        break;

      src_name = gst_object_get_name (message->src);
      GST_DEBUG ("%s changed state from %s to %s", src_name,
          gst_element_state_get_name (old_state),
          gst_element_state_get_name (new_state));
      g_free (src_name);

      /* now do stuff */
      if (new_state <= GST_STATE_PAUSED) {
        bvw_query_timeout (bvw);
        bvw_reconfigure_tick_timeout (bvw, 0);
        g_signal_emit (bvw, bvw_signals[SIGNAL_STATE_CHANGE], 0, FALSE);

      } else if (new_state == GST_STATE_PAUSED) {
        bvw_reconfigure_tick_timeout (bvw, 500);
        g_signal_emit (bvw, bvw_signals[SIGNAL_STATE_CHANGE], 0, FALSE);

      } else if (new_state > GST_STATE_PAUSED) {
        bvw_reconfigure_tick_timeout (bvw, 200);
        g_signal_emit (bvw, bvw_signals[SIGNAL_STATE_CHANGE], 0, TRUE);
      }

      if (old_state == GST_STATE_READY && new_state == GST_STATE_PAUSED) {
        GST_DEBUG_BIN_TO_DOT_FILE (GST_BIN_CAST (bvw->priv->play),
            GST_DEBUG_GRAPH_SHOW_ALL ^
            GST_DEBUG_GRAPH_SHOW_NON_DEFAULT_PARAMS, "totem-prerolled");
        bvw->priv->stream_length = 0;
        if (bacon_video_widget_get_stream_length (bvw) == 0) {
          GST_DEBUG ("Failed to query duration in PAUSED state?!");
        }
        bvw_update_stream_info (bvw);
        if (!bvw_check_missing_plugins_on_preroll (bvw)) {
          /* show a non-fatal warning message if we can't decode the video */
          bvw_check_if_video_decoder_is_missing (bvw);
        }
        g_signal_emit (bvw, bvw_signals[SIGNAL_READY_TO_SEEK], 0, FALSE);

      } else if (old_state == GST_STATE_PAUSED && new_state == GST_STATE_READY) {
        bvw->priv->media_has_video = FALSE;
        bvw->priv->media_has_audio = FALSE;

        /* clean metadata cache */
        if (bvw->priv->tagcache) {
          gst_tag_list_free (bvw->priv->tagcache);
          bvw->priv->tagcache = NULL;
        }
        if (bvw->priv->audiotags) {
          gst_tag_list_free (bvw->priv->audiotags);
          bvw->priv->audiotags = NULL;
        }
        if (bvw->priv->videotags) {
          gst_tag_list_free (bvw->priv->videotags);
          bvw->priv->videotags = NULL;
        }

        bvw->priv->video_width = 0;
        bvw->priv->video_height = 0;
      }
      break;
    }
    case GST_MESSAGE_ELEMENT:
    {
      bvw_handle_element_message (bvw, message);
      break;
    }

    case GST_MESSAGE_DURATION:
    {
      /* force _get_stream_length() to do new duration query */
      /*bvw->priv->stream_length = 0;
         if (bacon_video_widget_get_stream_length (bvw) == 0)
         {
         GST_DEBUG ("Failed to query duration after DURATION message?!");
         }
         break; */
    }

    case GST_MESSAGE_CLOCK_PROVIDE:
    case GST_MESSAGE_CLOCK_LOST:
    case GST_MESSAGE_NEW_CLOCK:
    case GST_MESSAGE_STATE_DIRTY:
      break;

    default:
      GST_LOG ("Unhandled message: %" GST_PTR_FORMAT, message);
      break;
  }
}


static void
got_video_size (BaconVideoWidget * bvw)
{
  GstMessage *msg;

  g_return_if_fail (bvw != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));

  msg = gst_message_new_application (GST_OBJECT (bvw->priv->play),
      gst_structure_new ("video-size", "width",
          G_TYPE_INT,
          bvw->priv->video_width, "height",
          G_TYPE_INT, bvw->priv->video_height, NULL));
  gst_element_post_message (bvw->priv->play, msg);
}

static void
got_time_tick (GstElement * play, gint64 time_nanos, BaconVideoWidget * bvw)
{
  gboolean seekable;

  g_return_if_fail (bvw != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));

  bvw->priv->current_time = (gint64) time_nanos / GST_MSECOND;

  if (bvw->priv->stream_length == 0) {
    bvw->priv->current_position = 0;
  } else {
    bvw->priv->current_position =
        (gdouble) bvw->priv->current_time / bvw->priv->stream_length;
  }

  if (bvw->priv->stream_length == 0) {
    seekable = bacon_video_widget_is_seekable (bvw);
  } else {
    if (bvw->priv->seekable == -1)
      g_object_notify (G_OBJECT (bvw), "seekable");
    seekable = TRUE;
  }

  bvw->priv->is_live = (bvw->priv->stream_length == 0);

/*
  GST_INFO ("%" GST_TIME_FORMAT ",%" GST_TIME_FORMAT " %s",
      GST_TIME_ARGS (bvw->priv->current_time),
      GST_TIME_ARGS (bvw->priv->stream_length),
      (seekable) ? "TRUE" : "FALSE"); 
*/

  g_signal_emit (bvw, bvw_signals[SIGNAL_TICK], 0,
      bvw->priv->current_time, bvw->priv->stream_length,
      bvw->priv->current_position, seekable);
}

static void
bvw_set_device_on_element (BaconVideoWidget * bvw, GstElement * element)
{
  if (bvw->priv->media_device == NULL)
    return;

  if (g_object_class_find_property (G_OBJECT_GET_CLASS (element), "device")) {
    GST_DEBUG ("Setting device to '%s'", bvw->priv->media_device);
    g_object_set (element, "device", bvw->priv->media_device, NULL);
  }
}

static void
playbin_source_notify_cb (GObject * play, GParamSpec * p,
    BaconVideoWidget * bvw)
{
  GObject *source = NULL;

  /* CHECKME: do we really need these taglist frees here (tpm)? */
  if (bvw->priv->tagcache) {
    gst_tag_list_free (bvw->priv->tagcache);
    bvw->priv->tagcache = NULL;
  }
  if (bvw->priv->audiotags) {
    gst_tag_list_free (bvw->priv->audiotags);
    bvw->priv->audiotags = NULL;
  }
  if (bvw->priv->videotags) {
    gst_tag_list_free (bvw->priv->videotags);
    bvw->priv->videotags = NULL;
  }

  g_object_get (play, "source", &source, NULL);

  if (source) {
    GST_DEBUG ("Got source of type %s", G_OBJECT_TYPE_NAME (source));
    bvw_set_device_on_element (bvw, GST_ELEMENT (source));
    g_object_unref (source);
  }
}

static gboolean
bvw_query_timeout (BaconVideoWidget * bvw)
{
  GstFormat fmt = GST_FORMAT_TIME;
  gint64 prev_len = -1;
  gint64 pos = -1, len = -1;

  /* check length/pos of stream */
  prev_len = bvw->priv->stream_length;
  if (gst_element_query_duration (bvw->priv->play, &fmt, &len)) {
    if (len != -1 && fmt == GST_FORMAT_TIME) {
      bvw->priv->stream_length = len / GST_MSECOND;
      if (bvw->priv->stream_length != prev_len) {
        g_signal_emit (bvw, bvw_signals[SIGNAL_GOT_METADATA], 0, NULL);
      }
    }
  } else {
    GST_INFO ("could not get duration");
  }

  if (gst_element_query_position (bvw->priv->play, &fmt, &pos)) {
    if (pos != -1 && fmt == GST_FORMAT_TIME) {
      got_time_tick (GST_ELEMENT (bvw->priv->play), pos, bvw);
    }
  } else {
    GST_INFO ("could not get position");
  }

  return TRUE;
}

static void
caps_set (GObject * obj, GParamSpec * pspec, BaconVideoWidget * bvw)
{
  GstPad *pad = GST_PAD (obj);
  GstStructure *s;
  GstCaps *caps;

  if (!(caps = gst_pad_get_negotiated_caps (pad)))
    return;

  /* Get video decoder caps */
  s = gst_caps_get_structure (caps, 0);
  if (s) {
    /* We need at least width/height and framerate */
    if (!
        (gst_structure_get_fraction
            (s, "framerate", &bvw->priv->video_fps_n, &bvw->priv->video_fps_d)
            && gst_structure_get_int (s, "width", &bvw->priv->video_width)
            && gst_structure_get_int (s, "height", &bvw->priv->video_height)))
      return;

    /* Get the movie PAR if available */
    bvw->priv->movie_par = gst_structure_get_value (s, "pixel-aspect-ratio");

    /* Now set for real */
    bacon_video_widget_set_aspect_ratio (bvw, bvw->priv->ratio_type);
  }

  gst_caps_unref (caps);
}

static void
parse_stream_info (BaconVideoWidget * bvw)
{
  GstPad *videopad = NULL;
  gint n_audio, n_video;

  g_object_get (G_OBJECT (bvw->priv->play), "n-audio", &n_audio,
      "n-video", &n_video, NULL);

  bvw->priv->media_has_video = FALSE;
  if (n_video > 0) {
    gint i;

    bvw->priv->media_has_video = TRUE;
    for (i = 0; i < n_video && videopad == NULL; i++)
      g_signal_emit_by_name (bvw->priv->play, "get-video-pad", i, &videopad);
  }

  bvw->priv->media_has_audio = FALSE;
  if (n_audio > 0) {
    bvw->priv->media_has_audio = TRUE;
    if (!bvw->priv->media_has_video) {
      /*gint flags;*/

      /*g_object_get (bvw->priv->play, "flags", &flags, NULL);*/

      /*gdk_window_hide (bvw->priv->video_window);*/
      /*GTK_WIDGET_SET_FLAGS (GTK_WIDGET (bvw), GTK_DOUBLE_BUFFERED);*/
      /*flags &= ~GST_PLAY_FLAGS_VIS;*/

      /*g_object_set (bvw->priv->play, "flags", flags, NULL);*/
    }
  }

  if (videopad) {
    GstCaps *caps;

    if ((caps = gst_pad_get_negotiated_caps (videopad))) {
      caps_set (G_OBJECT (videopad), NULL, bvw);
      gst_caps_unref (caps);
    }
    g_signal_connect (videopad, "notify::caps", G_CALLBACK (caps_set), bvw);
    gst_object_unref (videopad);
  }
}

static void
playbin_stream_changed_cb (GstElement * obj, gpointer data)
{
  BaconVideoWidget *bvw = BACON_VIDEO_WIDGET (data);
  GstMessage *msg;

  /* we're being called from the streaming thread, so don't do anything here */
  GST_LOG ("streams have changed");
  msg = gst_message_new_application (GST_OBJECT (bvw->priv->play),
      gst_structure_new ("stream-changed", NULL));
  gst_element_post_message (bvw->priv->play, msg);
}

static void
bacon_video_widget_finalize (GObject * object)
{
  BaconVideoWidget *bvw = (BaconVideoWidget *) object;

  GST_INFO ("finalizing");

  if (bvw->priv->bus) {
    /* make bus drop all messages to make sure none of our callbacks is ever
     * called again (main loop might be run again to display error dialog) */
    gst_bus_set_flushing (bvw->priv->bus, TRUE);

    if (bvw->priv->sig_bus_sync)
      g_signal_handler_disconnect (bvw->priv->bus, bvw->priv->sig_bus_sync);

    if (bvw->priv->sig_bus_async)
      g_signal_handler_disconnect (bvw->priv->bus, bvw->priv->sig_bus_async);

    gst_object_unref (bvw->priv->bus);
    bvw->priv->bus = NULL;
  }

  g_free (bvw->priv->media_device);
  bvw->priv->media_device = NULL;

  g_free (bvw->priv->mrl);
  bvw->priv->mrl = NULL;

  if (bvw->priv->play != NULL && GST_IS_ELEMENT (bvw->priv->play)) {
    gst_element_set_state (bvw->priv->play, GST_STATE_NULL);
    gst_object_unref (bvw->priv->play);
    bvw->priv->play = NULL;
  }

  if (bvw->priv->update_id) {
    g_source_remove (bvw->priv->update_id);
    bvw->priv->update_id = 0;
  }

  if (bvw->priv->interface_update_id) {
    g_source_remove (bvw->priv->interface_update_id);
    bvw->priv->interface_update_id = 0;
  }

  if (bvw->priv->tagcache) {
    gst_tag_list_free (bvw->priv->tagcache);
    bvw->priv->tagcache = NULL;
  }
  if (bvw->priv->audiotags) {
    gst_tag_list_free (bvw->priv->audiotags);
    bvw->priv->audiotags = NULL;
  }
  if (bvw->priv->videotags) {
    gst_tag_list_free (bvw->priv->videotags);
    bvw->priv->videotags = NULL;
  }

  if (bvw->priv->cursor != NULL) {
    gdk_cursor_unref (bvw->priv->cursor);
    bvw->priv->cursor = NULL;
  }

  if (bvw->priv->eos_id != 0)
    g_source_remove (bvw->priv->eos_id);

  g_mutex_free (bvw->priv->lock);

  G_OBJECT_CLASS (parent_class)->finalize (object);
}

static void
bacon_video_widget_set_property (GObject * object, guint property_id,
    const GValue * value, GParamSpec * pspec)
{
  BaconVideoWidget *bvw;

  bvw = BACON_VIDEO_WIDGET (object);

  switch (property_id) {
    case PROP_LOGO_MODE:
      bacon_video_widget_set_logo_mode (bvw, g_value_get_boolean (value));
      break;
    case PROP_EXPAND_LOGO:
      bvw->priv->expand_logo = g_value_get_boolean (value);
      break;
    case PROP_SHOW_CURSOR:
      bacon_video_widget_set_show_cursor (bvw, g_value_get_boolean (value));
      break;
    case PROP_VOLUME:
      bacon_video_widget_set_volume (bvw, g_value_get_double (value));
      break;

    default:
      G_OBJECT_WARN_INVALID_PROPERTY_ID (object, property_id, pspec);
      break;
  }
}

static void
bacon_video_widget_get_property (GObject * object, guint property_id,
    GValue * value, GParamSpec * pspec)
{
  BaconVideoWidget *bvw;

  bvw = BACON_VIDEO_WIDGET (object);

  switch (property_id) {
    case PROP_LOGO_MODE:
      g_value_set_boolean (value, bacon_video_widget_get_logo_mode (bvw));
      break;
    case PROP_EXPAND_LOGO:
      g_value_set_boolean (value, bvw->priv->expand_logo);
      break;
    case PROP_POSITION:
      g_value_set_int64 (value, bacon_video_widget_get_position (bvw));
      break;
    case PROP_STREAM_LENGTH:
      g_value_set_int64 (value, bacon_video_widget_get_stream_length (bvw));
      break;
    case PROP_PLAYING:
      g_value_set_boolean (value, bacon_video_widget_is_playing (bvw));
      break;
    case PROP_SEEKABLE:
      g_value_set_boolean (value, bacon_video_widget_is_seekable (bvw));
      break;
    case PROP_SHOW_CURSOR:
      g_value_set_boolean (value, bacon_video_widget_get_show_cursor (bvw));
      break;
    case PROP_VOLUME:
      g_value_set_int (value, bacon_video_widget_get_volume (bvw));
      break;
    default:
      G_OBJECT_WARN_INVALID_PROPERTY_ID (object, property_id, pspec);
      break;
  }
}

/* ============================================================= */
/*                                                               */
/*                       Public Methods                          */
/*                                                               */
/* ============================================================= */


/**
 * bacon_video_widget_get_backend_name:
 * @bvw: a #BaconVideoWidget
 *
 * Returns the name string for @bvw. For the GStreamer backend, it is the output
 * of gst_version_string(). *
 * Return value: the backend's name; free with g_free()
 **/
char *
bacon_video_widget_get_backend_name (BaconVideoWidget * bvw)
{
  return gst_version_string ();
}

/**
 * bacon_video_widget_get_subtitle:
 * @bvw: a #BaconVideoWidget
 *
 * Returns the index of the current subtitles.
 *
 * If the widget is not playing, %-2 will be returned. If no subtitles are
 * being used, %-1 is returned.
 *
 * Return value: the subtitle index
 **/
int
bacon_video_widget_get_subtitle (BaconVideoWidget * bvw)
{
  int subtitle = -1;
  gint flags;

  g_return_val_if_fail (bvw != NULL, -2);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), -2);
  g_return_val_if_fail (bvw->priv->play != NULL, -2);

  g_object_get (bvw->priv->play, "flags", &flags, NULL);

  if ((flags & GST_PLAY_FLAGS_TEXT) == 0)
    return -2;

  g_object_get (G_OBJECT (bvw->priv->play), "current-text", &subtitle, NULL);

  return subtitle;
}

/**
 * bacon_video_widget_set_subtitle:
 * @bvw: a #BaconVideoWidget
 * @subtitle: a subtitle index
 *
 * Sets the subtitle index for @bvw. If @subtitle is %-1, no subtitles will
 * be used.
 **/
void
bacon_video_widget_set_subtitle (BaconVideoWidget * bvw, int subtitle)
{
  gint flags;

  g_return_if_fail (bvw != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));
  g_return_if_fail (bvw->priv->play != NULL);

  g_object_get (bvw->priv->play, "flags", &flags, NULL);

  if (subtitle == -2) {
    flags &= ~GST_PLAY_FLAGS_TEXT;
    subtitle = -1;
  } else {
    flags |= GST_PLAY_FLAGS_TEXT;
  }

  g_object_set (bvw->priv->play, "flags", flags, "current-text", subtitle,
      NULL);
}

/**
 * bacon_video_widget_has_next_track:
 * @bvw: a #BaconVideoWidget
 *
 * Determines whether there is another track after the current one, typically
 * as a chapter on a DVD.
 *
 * Return value: %TRUE if there is another track, %FALSE otherwise
 **/
gboolean
bacon_video_widget_has_next_track (BaconVideoWidget * bvw)
{
  //FIXME
  return TRUE;
}

/**
 * bacon_video_widget_has_previous_track:
 * @bvw: a #BaconVideoWidget
 *
 * Determines whether there is another track before the current one, typically
 * as a chapter on a DVD.
 *
 * Return value: %TRUE if there is another track, %FALSE otherwise
 **/
gboolean
bacon_video_widget_has_previous_track (BaconVideoWidget * bvw)
{
  //FIXME
  return TRUE;
}

static GList *
get_lang_list_for_type (BaconVideoWidget * bvw, const gchar * type_name)
{
  GList *ret = NULL;
  gint num = 0;

  if (g_str_equal (type_name, "AUDIO")) {
    gint i, n;

    g_object_get (G_OBJECT (bvw->priv->play), "n-audio", &n, NULL);
    if (n == 0)
      return NULL;

    for (i = 0; i < n; i++) {
      GstTagList *tags = NULL;

      g_signal_emit_by_name (G_OBJECT (bvw->priv->play), "get-audio-tags",
          i, &tags);

      if (tags) {
        gchar *lc = NULL, *cd = NULL;

        gst_tag_list_get_string (tags, GST_TAG_LANGUAGE_CODE, &lc);
        gst_tag_list_get_string (tags, GST_TAG_CODEC, &lc);

        if (lc) {
          ret = g_list_prepend (ret, lc);
          g_free (cd);
        } else if (cd) {
          ret = g_list_prepend (ret, cd);
        } else {
          ret =
              g_list_prepend (ret, g_strdup_printf ("%s %d", type_name, num++));
        }
        gst_tag_list_free (tags);
      } else {
        ret = g_list_prepend (ret, g_strdup_printf ("%s %d", type_name, num++));
      }
    }
  } else if (g_str_equal (type_name, "TEXT")) {
    gint i, n = 0;

    g_object_get (G_OBJECT (bvw->priv->play), "n-text", &n, NULL);
    if (n == 0)
      return NULL;

    for (i = 0; i < n; i++) {
      GstTagList *tags = NULL;

      g_signal_emit_by_name (G_OBJECT (bvw->priv->play), "get-text-tags",
          i, &tags);

      if (tags) {
        gchar *lc = NULL, *cd = NULL;

        gst_tag_list_get_string (tags, GST_TAG_LANGUAGE_CODE, &lc);
        gst_tag_list_get_string (tags, GST_TAG_CODEC, &lc);

        if (lc) {
          ret = g_list_prepend (ret, lc);
          g_free (cd);
        } else if (cd) {
          ret = g_list_prepend (ret, cd);
        } else {
          ret =
              g_list_prepend (ret, g_strdup_printf ("%s %d", type_name, num++));
        }
        gst_tag_list_free (tags);
      } else {
        ret = g_list_prepend (ret, g_strdup_printf ("%s %d", type_name, num++));
      }
    }
  } else {
    g_critical ("Invalid stream type '%s'", type_name);
    return NULL;
  }

  return g_list_reverse (ret);
}

/**
 * bacon_video_widget_get_subtitles:
 * @bvw: a #BaconVideoWidget
 *
 * Returns a list of subtitle tags, each in the form <literal>TEXT <replaceable>x</replaceable></literal>,
 * where <replaceable>x</replaceable> is the subtitle index.
 *
 * Return value: a #GList of subtitle tags, or %NULL; free each element with g_free() and the list with g_list_free()
 **/
GList *
bacon_video_widget_get_subtitles (BaconVideoWidget * bvw)
{
  GList *list;

  g_return_val_if_fail (bvw != NULL, NULL);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), NULL);
  g_return_val_if_fail (bvw->priv->play != NULL, NULL);

  list = get_lang_list_for_type (bvw, "TEXT");

  return list;
}

/**
 * bacon_video_widget_get_languages:
 * @bvw: a #BaconVideoWidget
 *
 * Returns a list of audio language tags, each in the form <literal>AUDIO <replaceable>x</replaceable></literal>,
 * where <replaceable>x</replaceable> is the language index.
 *
 * Return value: a #GList of audio language tags, or %NULL; free each element with g_free() and the list with g_list_free()
 **/
GList *
bacon_video_widget_get_languages (BaconVideoWidget * bvw)
{
  g_return_val_if_fail (bvw != NULL, NULL);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), NULL);
  g_return_val_if_fail (bvw->priv->play != NULL, NULL);

  return get_lang_list_for_type (bvw, "AUDIO");
}

/**
 * bacon_video_widget_get_language:
 * @bvw: a #BaconVideoWidget
 *
 * Returns the index of the current audio language.
 *
 * If the widget is not playing, or the default language is in use, %-2 will be returned.
 *
 * Return value: the audio language index
 **/
int
bacon_video_widget_get_language (BaconVideoWidget * bvw)
{
  int language = -1;

  g_return_val_if_fail (bvw != NULL, -2);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), -2);
  g_return_val_if_fail (bvw->priv->play != NULL, -2);

  g_object_get (G_OBJECT (bvw->priv->play), "current-audio", &language, NULL);

  if (language == -1)
    language = -2;

  return language;
}

/**
 * bacon_video_widget_set_language:
 * @bvw: a #BaconVideoWidget
 * @language: an audio language index
 *
 * Sets the audio language index for @bvw. If @language is %-1, the default language will
 * be used.
 **/
void
bacon_video_widget_set_language (BaconVideoWidget * bvw, int language)
{
  g_return_if_fail (bvw != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));
  g_return_if_fail (bvw->priv->play != NULL);

  if (language == -1)
    language = 0;
  else if (language == -2)
    language = -1;

  GST_DEBUG ("setting language to %d", language);

  g_object_set (bvw->priv->play, "current-audio", language, NULL);

  g_object_get (bvw->priv->play, "current-audio", &language, NULL);
  GST_DEBUG ("current-audio now: %d", language);

  /* so it updates its metadata for the newly-selected stream */
  g_signal_emit (bvw, bvw_signals[SIGNAL_GOT_METADATA], 0, NULL);
  g_signal_emit (bvw, bvw_signals[SIGNAL_CHANNELS_CHANGE], 0);
}

static guint
connection_speed_enum_to_kbps (gint speed)
{
  static const guint conv_table[] = { 14400, 19200, 28800, 33600, 34400, 56000,
    112000, 256000, 384000, 512000, 1536000, 10752000
  };

  g_return_val_if_fail (speed >= 0
      && (guint) speed < G_N_ELEMENTS (conv_table), 0);

  /* must round up so that the correct streams are chosen and not ignored
   * due to rounding errors when doing kbps <=> bps */
  return (conv_table[speed] / 1000) +
      (((conv_table[speed] % 1000) != 0) ? 1 : 0);
}

/**
 * bacon_video_widget_get_connection_speed:
 * @bvw: a #BaconVideoWidget
 *
 * Returns the current connection speed, where %0 is the lowest speed
 * and %11 is the highest.
 *
 * Return value: the connection speed index
 **/
int
bacon_video_widget_get_connection_speed (BaconVideoWidget * bvw)
{
  g_return_val_if_fail (bvw != NULL, 0);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), 0);

  return bvw->priv->connection_speed;
}

/**
 * bacon_video_widget_set_connection_speed:
 * @bvw: a #BaconVideoWidget
 * @speed: the connection speed index
 *
 * Sets the connection speed from the given @speed index, where %0 is the lowest speed
 * and %11 is the highest.
 **/
void
bacon_video_widget_set_connection_speed (BaconVideoWidget * bvw, int speed)
{
  g_return_if_fail (bvw != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));

  if (bvw->priv->connection_speed != speed) {
    bvw->priv->connection_speed = speed;
  }

  if (bvw->priv->play != NULL &&
      g_object_class_find_property (G_OBJECT_GET_CLASS (bvw->priv->play),
          "connection-speed")) {
    guint kbps = connection_speed_enum_to_kbps (speed);

    GST_LOG ("Setting connection speed %d (= %d kbps)", speed, kbps);
    g_object_set (bvw->priv->play, "connection-speed", kbps, NULL);
  }
}


static gint
get_num_audio_channels (BaconVideoWidget * bvw)
{
  gint channels;

  switch (bvw->priv->speakersetup) {
    case BVW_AUDIO_SOUND_STEREO:
      channels = 2;
      break;
    case BVW_AUDIO_SOUND_CHANNEL4:
      channels = 4;
      break;
    case BVW_AUDIO_SOUND_CHANNEL5:
      channels = 5;
      break;
    case BVW_AUDIO_SOUND_CHANNEL41:
      /* so alsa has this as 5.1, but empty center speaker. We don't really
       * do that yet. ;-). So we'll take the placebo approach. */
    case BVW_AUDIO_SOUND_CHANNEL51:
      channels = 6;
      break;
    case BVW_AUDIO_SOUND_AC3PASSTHRU:
    default:
      g_return_val_if_reached (-1);
  }

  return channels;
}

static GstCaps *
fixate_to_num (const GstCaps * in_caps, gint channels)
{
  gint n, count;
  GstStructure *s;
  const GValue *v;
  GstCaps *out_caps;

  out_caps = gst_caps_copy (in_caps);

  count = gst_caps_get_size (out_caps);
  for (n = 0; n < count; n++) {
    s = gst_caps_get_structure (out_caps, n);
    v = gst_structure_get_value (s, "channels");
    if (!v)
      continue;

    /* get channel count (or list of ~) */
    gst_structure_fixate_field_nearest_int (s, "channels", channels);
  }

  return out_caps;
}

static void
set_audio_filter (BaconVideoWidget * bvw)
{
  gint channels;
  GstCaps *caps, *res;
  GstPad *pad;

  /* reset old */
  g_object_set (bvw->priv->audio_capsfilter, "caps", NULL, NULL);

  /* construct possible caps to filter down to our chosen caps */
  /* Start with what the audio sink supports, but limit the allowed
   * channel count to our speaker output configuration */
  pad = gst_element_get_pad (bvw->priv->audio_capsfilter, "src");
  caps = gst_pad_peer_get_caps (pad);
  gst_object_unref (pad);

  if ((channels = get_num_audio_channels (bvw)) == -1)
    return;

  res = fixate_to_num (caps, channels);
  gst_caps_unref (caps);

  /* set */
  if (res && gst_caps_is_empty (res)) {
    gst_caps_unref (res);
    res = NULL;
  }
  g_object_set (bvw->priv->audio_capsfilter, "caps", res, NULL);

  if (res) {
    gst_caps_unref (res);
  }

  /* reset */
  pad = gst_element_get_pad (bvw->priv->audio_capsfilter, "src");
  gst_pad_set_caps (pad, NULL);
  gst_object_unref (pad);
}

/**
 * bacon_video_widget_get_audio_out_type:
 * @bvw: a #BaconVideoWidget
 *
 * Returns the current audio output type (e.g. how many speaker channels)
 * from #BaconVideoWidgetAudioOutType.
 *
 * Return value: the audio output type, or %-1
 **/
BvwAudioOutType
bacon_video_widget_get_audio_out_type (BaconVideoWidget * bvw)
{
  g_return_val_if_fail (bvw != NULL, -1);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), -1);

  return bvw->priv->speakersetup;
}

/**
 * bacon_video_widget_set_audio_out_type:
 * @bvw: a #BaconVideoWidget
 * @type: the new audio output type
 *
 * Sets the audio output type (number of speaker channels) in the video widget,
 * and stores it in GConf.
 *
 * Return value: %TRUE on success, %FALSE otherwise
 **/
gboolean
bacon_video_widget_set_audio_out_type (BaconVideoWidget * bvw,
    BvwAudioOutType type)
{
  g_return_val_if_fail (bvw != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), FALSE);

  if (type == bvw->priv->speakersetup)
    return FALSE;
  else if (type == BVW_AUDIO_SOUND_AC3PASSTHRU)
    return FALSE;

  bvw->priv->speakersetup = type;


  set_audio_filter (bvw);

  return FALSE;
}






/* =========================================== */
/*                                             */
/*               Play/Pause, Stop              */
/*                                             */
/* =========================================== */

static GError *
bvw_error_from_gst_error (BaconVideoWidget * bvw, GstMessage * err_msg)
{
  const gchar *src_typename;
  GError *ret = NULL;
  GError *e = NULL;

  GST_LOG ("resolving error message %" GST_PTR_FORMAT, err_msg);

  src_typename = (err_msg->src) ? G_OBJECT_TYPE_NAME (err_msg->src) : NULL;

  gst_message_parse_error (err_msg, &e, NULL);

  if (is_error (e, RESOURCE, NOT_FOUND) || is_error (e, RESOURCE, OPEN_READ)) {
#if 0
    if (strchr (mrl, ':') &&
        (g_str_has_prefix (mrl, "dvd") ||
            g_str_has_prefix (mrl, "cd") || g_str_has_prefix (mrl, "vcd"))) {
      ret = g_error_new_literal (BVW_ERROR, GST_ERROR_INVALID_DEVICE,
          e->message);
    } else {
#endif
      if (e->code == GST_RESOURCE_ERROR_NOT_FOUND) {
        if (GST_IS_BASE_AUDIO_SINK (err_msg->src)) {
          ret =
              g_error_new_literal (BVW_ERROR, GST_ERROR_AUDIO_PLUGIN,
              _
              ("The requested audio output was not found. "
                  "Please select another audio output in the Multimedia "
                  "Systems Selector."));
        } else {
          ret =
              g_error_new_literal (BVW_ERROR, GST_ERROR_FILE_NOT_FOUND,
              _("Location not found."));
        }
      } else {
        ret = g_error_new_literal (BVW_ERROR, GST_ERROR_FILE_PERMISSION,
            _("Could not open location; "
                "you might not have permission to open the file."));
      }
#if 0
    }
#endif
  } else if (is_error (e, RESOURCE, BUSY)) {
    if (GST_IS_VIDEO_SINK (err_msg->src)) {
      /* a somewhat evil check, but hey.. */
      ret = g_error_new_literal (BVW_ERROR,
          GST_ERROR_VIDEO_PLUGIN,
          _
          ("The video output is in use by another application. "
              "Please close other video applications, or select "
              "another video output in the Multimedia Systems Selector."));
    } else if (GST_IS_BASE_AUDIO_SINK (err_msg->src)) {
      ret = g_error_new_literal (BVW_ERROR,
          GST_ERROR_AUDIO_BUSY,
          _
          ("The audio output is in use by another application. "
              "Please select another audio output in the Multimedia Systems Selector. "
              "You may want to consider using a sound server."));
    }
  } else if (e->domain == GST_RESOURCE_ERROR) {
    ret = g_error_new_literal (BVW_ERROR, GST_ERROR_FILE_GENERIC, e->message);
  } else if (is_error (e, CORE, MISSING_PLUGIN) ||
      is_error (e, STREAM, CODEC_NOT_FOUND)) {
    if (bvw->priv->missing_plugins != NULL) {
      gchar **descs, *msg = NULL;
      guint num;

      descs = bvw_get_missing_plugins_descriptions (bvw->priv->missing_plugins);
      num = g_list_length (bvw->priv->missing_plugins);

      if (is_error (e, CORE, MISSING_PLUGIN)) {
        /* should be exactly one missing thing (source or converter) */
        msg =
            g_strdup_printf (_
            ("The playback of this movie requires a %s "
                "plugin which is not installed."), descs[0]);
      } else {
        gchar *desc_list;

        desc_list = g_strjoinv ("\n", descs);
        msg = g_strdup_printf (ngettext (_("The playback of this movie "
                    "requires a %s plugin which is not installed."),
                _("The playback "
                    "of this movie requires the following decoders which are not "
                    "installed:\n\n%s"), num),
            (num == 1) ? descs[0] : desc_list);
        g_free (desc_list);
      }
      ret = g_error_new_literal (BVW_ERROR, GST_ERROR_CODEC_NOT_HANDLED, msg);
      g_free (msg);
      g_strfreev (descs);
    } else {
      GST_LOG ("no missing plugin messages, posting generic error");
      ret = g_error_new_literal (BVW_ERROR, GST_ERROR_CODEC_NOT_HANDLED,
          e->message);
    }
  } else if (is_error (e, STREAM, WRONG_TYPE) ||
      is_error (e, STREAM, NOT_IMPLEMENTED)) {
    if (src_typename) {
      ret = g_error_new (BVW_ERROR, GST_ERROR_CODEC_NOT_HANDLED, "%s: %s",
          src_typename, e->message);
    } else {
      ret = g_error_new_literal (BVW_ERROR, GST_ERROR_CODEC_NOT_HANDLED,
          e->message);
    }
  } else if (is_error (e, STREAM, FAILED) &&
      src_typename && strncmp (src_typename, "GstTypeFind", 11) == 0) {
    ret = g_error_new_literal (BVW_ERROR, GST_ERROR_READ_ERROR,
        _("Cannot play this file over the network. "
            "Try downloading it to disk first."));
  } else {
    /* generic error, no code; take message */
    ret = g_error_new_literal (BVW_ERROR, GST_ERROR_GENERIC, e->message);
  }
  g_error_free (e);
  bvw_clear_missing_plugins_messages (bvw);

  return ret;
}


static gboolean
poll_for_state_change_full (BaconVideoWidget * bvw, GstElement * element,
    GstState state, GstMessage ** err_msg, gint64 timeout)
{
  GstBus *bus;
  GstMessageType events, saved_events;

  g_assert (err_msg != NULL);

  bus = gst_element_get_bus (element);

  events = GST_MESSAGE_STATE_CHANGED | GST_MESSAGE_ERROR | GST_MESSAGE_EOS;

  saved_events = bvw->priv->ignore_messages_mask;

  if (element != NULL && element == bvw->priv->play) {
    /* we do want the main handler to process state changed messages for
     * playbin as well, otherwise it won't hook up the timeout etc. */
    bvw->priv->ignore_messages_mask |= (events ^ GST_MESSAGE_STATE_CHANGED);
  } else {
    bvw->priv->ignore_messages_mask |= events;
  }

  while (TRUE) {
    GstMessage *message;
    GstElement *src;

    message = gst_bus_poll (bus, events, timeout);

    if (!message)
      goto timed_out;

    src = (GstElement *) GST_MESSAGE_SRC (message);

    switch (GST_MESSAGE_TYPE (message)) {
      case GST_MESSAGE_STATE_CHANGED:
      {
        GstState old, new, pending;

        if (src == element) {
          gst_message_parse_state_changed (message, &old, &new, &pending);
          if (new == state) {
            gst_message_unref (message);
            goto success;
          }
        }
        break;
      }
      case GST_MESSAGE_ERROR:
      {
        bvw_error_msg (bvw, message);
        *err_msg = message;
        message = NULL;
        goto error;
        break;
      }
      case GST_MESSAGE_EOS:
      {
        GError *e = NULL;

        gst_message_unref (message);
        e = g_error_new_literal (BVW_ERROR, GST_ERROR_FILE_GENERIC,
            _("Media file could not be played."));
        *err_msg =
            gst_message_new_error (GST_OBJECT (bvw->priv->play), e, NULL);
        g_error_free (e);
        goto error;
        break;
      }
      default:
        g_assert_not_reached ();
        break;
    }

    gst_message_unref (message);
  }

  g_assert_not_reached ();

success:
  /* state change succeeded */
  GST_DEBUG ("state change to %s succeeded",
      gst_element_state_get_name (state));
  bvw->priv->ignore_messages_mask = saved_events;
  return TRUE;

timed_out:
  /* it's taking a long time to open -- just tell totem it was ok, this allows
   * the user to stop the loading process with the normal stop button */
  GST_DEBUG ("state change to %s timed out, returning success and handling "
      "errors asynchronously", gst_element_state_get_name (state));
  bvw->priv->ignore_messages_mask = saved_events;
  return TRUE;

error:
  GST_DEBUG ("error while waiting for state change to %s: %" GST_PTR_FORMAT,
      gst_element_state_get_name (state), *err_msg);
  /* already set *err_msg */
  bvw->priv->ignore_messages_mask = saved_events;
  return FALSE;
}

/**
 * bacon_video_widget_open:
 * @bvw: a #BaconVideoWidget
 * @mrl: an MRL
 * @subtitle_uri: the URI of a subtitle file, or %NULL
 * @error: a #GError, or %NULL
 *
 * Opens the given @mrl in @bvw for playing. If @subtitle_uri is not %NULL, the given
 * subtitle file is also loaded. Alternatively, the subtitle URI can be passed in @mrl
 * by adding it after <literal>#subtitle:</literal>. For example:
 * <literal>http://example.com/video.mpg#subtitle:/home/user/subtitle.ass</literal>.
 *
 * If there was a filesystem error, a %ERROR_GENERIC error will be returned. Otherwise,
 * more specific #BvwError errors will be returned.
 *
 * On success, the MRL is loaded and waiting to be played with bacon_video_widget_play().
 *
 * Return value: %TRUE on success, %FALSE otherwise
 **/

gboolean
bacon_video_widget_open (BaconVideoWidget * bvw,
    const gchar * mrl, const gchar * subtitle_uri, GError ** error)
{

  GstMessage *err_msg = NULL;
  GFile *file;
  gboolean ret;
  char *path;

  g_return_val_if_fail (bvw != NULL, FALSE);
  g_return_val_if_fail (mrl != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), FALSE);
  g_return_val_if_fail (bvw->priv->play != NULL, FALSE);


  /* So we aren't closed yet... */
  if (bvw->priv->mrl) {
    bacon_video_widget_close (bvw);
  }

  GST_DEBUG ("mrl = %s", GST_STR_NULL (mrl));
  GST_DEBUG ("subtitle_uri = %s", GST_STR_NULL (subtitle_uri));

  /* this allows non-URI type of files in the thumbnailer and so on */
  file = g_file_new_for_commandline_arg (mrl);


  /* Only use the URI when FUSE isn't available for a file */
  path = g_file_get_path (file);
  if (path) {
    bvw->priv->mrl = g_filename_to_uri (path, NULL, NULL);
    g_free (path);
  } else {
    bvw->priv->mrl = g_strdup (mrl);
  }

  g_object_unref (file);

  if (g_str_has_prefix (mrl, "icy:") != FALSE) {
    /* Handle "icy://" URLs from QuickTime */
    g_free (bvw->priv->mrl);
    bvw->priv->mrl = g_strdup_printf ("http:%s", mrl + 4);
  } else if (g_str_has_prefix (mrl, "icyx:") != FALSE) {
    /* Handle "icyx://" URLs from Orban/Coding Technologies AAC/aacPlus Player */
    g_free (bvw->priv->mrl);
    bvw->priv->mrl = g_strdup_printf ("http:%s", mrl + 5);
  } else if (g_str_has_prefix (mrl, "dvd:///")) {
    /* this allows to play backups of dvds */
    g_free (bvw->priv->mrl);
    bvw->priv->mrl = g_strdup ("dvd://");
    g_free (bvw->priv->media_device);
    bvw->priv->media_device = g_strdup (mrl + strlen ("dvd://"));
  } else if (g_str_has_prefix (mrl, "vcd:///")) {
    /* this allows to play backups of vcds */
    g_free (bvw->priv->mrl);
    bvw->priv->mrl = g_strdup ("vcd://");
    g_free (bvw->priv->media_device);
    bvw->priv->media_device = g_strdup (mrl + strlen ("vcd://"));
  }

  bvw->priv->got_redirect = FALSE;
  bvw->priv->media_has_video = FALSE;
  bvw->priv->media_has_audio = FALSE;
  bvw->priv->stream_length = 0;
  bvw->priv->ignore_messages_mask = 0;

  if (g_strrstr (bvw->priv->mrl, "#subtitle:")) {
    gchar **uris;
    gchar *subtitle_uri;

    uris = g_strsplit (bvw->priv->mrl, "#subtitle:", 2);
    /* Try to fix subtitle uri if needed */
    if (uris[1][0] == '/') {
      subtitle_uri = g_strdup_printf ("file://%s", uris[1]);
    } else {
      if (strchr (uris[1], ':')) {
        subtitle_uri = g_strdup (uris[1]);
      } else {
        gchar *cur_dir = g_get_current_dir ();
        if (!cur_dir) {
          g_set_error_literal (error, BVW_ERROR, GST_ERROR_GENERIC,
              _("Failed to retrieve working directory"));
          return FALSE;
        }
        subtitle_uri = g_strdup_printf ("file://%s/%s", cur_dir, uris[1]);
        g_free (cur_dir);
      }
    }
    g_object_set (bvw->priv->play, "uri", bvw->priv->mrl,
        "suburi", subtitle_uri, NULL);
    g_free (subtitle_uri);
    g_strfreev (uris);
  } else {

    g_object_set (bvw->priv->play, "uri", bvw->priv->mrl, NULL);
  }

  bvw->priv->seekable = -1;
  bvw->priv->target_state = GST_STATE_PAUSED;
  bvw_clear_missing_plugins_messages (bvw);

  gst_element_set_state (bvw->priv->play, GST_STATE_PAUSED);

  if (bvw->priv->use_type == BVW_USE_TYPE_AUDIO ||
      bvw->priv->use_type == BVW_USE_TYPE_VIDEO) {
    GST_INFO ("normal playback, handling all errors asynchroneously");
    ret = TRUE;
  } else {
    /* used as thumbnailer or metadata extractor for properties dialog. In
     * this case, wait for any state change to really finish and process any
     * pending tag messages, so that the information is available right away */
    GST_INFO ("waiting for state changed to PAUSED to complete");
    ret = poll_for_state_change_full (bvw, bvw->priv->play,
        GST_STATE_PAUSED, &err_msg, -1);

    bvw_process_pending_tag_messages (bvw);
    bacon_video_widget_get_stream_length (bvw);
    GST_INFO ("stream length = %u", bvw->priv->stream_length);

    /* even in case of an error (e.g. no decoders installed) we might still
     * have useful metadata (like codec types, duration, etc.) */
    g_signal_emit (bvw, bvw_signals[SIGNAL_GOT_METADATA], 0, NULL);
  }

  if (ret) {
    g_signal_emit (bvw, bvw_signals[SIGNAL_CHANNELS_CHANGE], 0);
  } else {
    GST_INFO ("Error on open: %" GST_PTR_FORMAT, err_msg);
    if (bvw_check_missing_plugins_error (bvw, err_msg)) {
      /* totem will try to start playing, so ignore all messages on the bus */
      bvw->priv->ignore_messages_mask |= GST_MESSAGE_ERROR;
      GST_LOG ("missing plugins handled, ignoring error and returning TRUE");
      gst_message_unref (err_msg);
      err_msg = NULL;
      ret = TRUE;
    } else {
      bvw->priv->ignore_messages_mask |= GST_MESSAGE_ERROR;
      bvw_stop_play_pipeline (bvw);
      g_free (bvw->priv->mrl);
      bvw->priv->mrl = NULL;
    }
  }

  /* When opening a new media we want to redraw ourselves */
  gtk_widget_queue_draw (GTK_WIDGET (bvw));

  if (err_msg != NULL) {
    if (error) {
      *error = bvw_error_from_gst_error (bvw, err_msg);

    } else {
      GST_WARNING ("Got error, but caller is not collecting error details!");
    }
    gst_message_unref (err_msg);
  }


  return ret;
}

/**
 * bacon_video_widget_play:
 * @bvw: a #BaconVideoWidget
 * @error: a #GError, or %NULL
 *
 * Plays the currently-loaded video in @bvw.
 *
 * Errors from the GStreamer backend will be returned asynchronously via the
 * #BaconVideoWidget::error signal, even if this function returns %TRUE.
 *
 * Return value: %TRUE on success, %FALSE otherwise
 **/
gboolean
bacon_video_widget_play (BaconVideoWidget * bvw)
{

  GstState cur_state;

  g_return_val_if_fail (bvw != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (bvw->priv->play), FALSE);
  g_return_val_if_fail (bvw->priv->mrl != NULL, FALSE);

  bvw->priv->target_state = GST_STATE_PLAYING;

  /* no need to actually go into PLAYING in capture/metadata mode (esp.
   * not with sinks that don't sync to the clock), we'll get everything
   * we need by prerolling the pipeline, and that is done in _open() */
  if (bvw->priv->use_type == BVW_USE_TYPE_CAPTURE ||
      bvw->priv->use_type == BVW_USE_TYPE_METADATA) {
    return TRUE;
  }

  /* just lie and do nothing in this case */
  gst_element_get_state (bvw->priv->play, &cur_state, NULL, 0);
  if (bvw->priv->plugin_install_in_progress && cur_state != GST_STATE_PAUSED) {
    GST_INFO ("plugin install in progress and nothing to play, doing nothing");
    return TRUE;
  }

  GST_INFO ("play");
  gst_element_set_state (bvw->priv->play, GST_STATE_PLAYING);

  /* will handle all errors asynchroneously */
  return TRUE;
}

/**
 * bacon_video_widget_can_direct_seek:
 * @bvw: a #BaconVideoWidget
 *
 * Determines whether direct seeking is possible for the current stream.
 *
 * Return value: %TRUE if direct seeking is possible, %FALSE otherwise
 **/
gboolean
bacon_video_widget_can_direct_seek (BaconVideoWidget * bvw)
{
  g_return_val_if_fail (bvw != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (bvw->priv->play), FALSE);

  if (bvw->priv->mrl == NULL)
    return FALSE;

  /* (instant seeking only make sense with video,
   * hence no cdda:// here) */
  if (g_str_has_prefix (bvw->priv->mrl, "file://") ||
      g_str_has_prefix (bvw->priv->mrl, "dvd:/") ||
      g_str_has_prefix (bvw->priv->mrl, "vcd:/"))
    return TRUE;

  return FALSE;
}

//If we want to seek throug a seekbar we want speed, so we use the KEY_UNIT flag
//Sometimes accurate position is requested so we use the ACCURATE flag
gboolean
bacon_video_widget_seek_time (BaconVideoWidget * bvw, gint64 time,
    gfloat rate, gboolean accurate)
{


  g_return_val_if_fail (bvw != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (bvw->priv->play), FALSE);

  GST_LOG ("Seeking to %" GST_TIME_FORMAT, GST_TIME_ARGS (time * GST_MSECOND));

  if (time > bvw->priv->stream_length
      && bvw->priv->stream_length > 0
      && !g_str_has_prefix (bvw->priv->mrl, "dvd:")
      && !g_str_has_prefix (bvw->priv->mrl, "vcd:")) {
    if (bvw->priv->eos_id == 0)
      bvw->priv->eos_id = g_idle_add (bvw_signal_eos_delayed, bvw);
    return TRUE;
  }


  if (accurate) {
    got_time_tick (bvw->priv->play, time * GST_MSECOND, bvw);
    gst_element_seek (bvw->priv->play, rate,
        GST_FORMAT_TIME,
        GST_SEEK_FLAG_FLUSH | GST_SEEK_FLAG_ACCURATE,
        GST_SEEK_TYPE_SET, time * GST_MSECOND,
        GST_SEEK_TYPE_NONE, GST_CLOCK_TIME_NONE);
  } else {
    /* Emit a time tick of where we are going, we are paused */
    got_time_tick (bvw->priv->play, time * GST_MSECOND, bvw);
    gst_element_seek (bvw->priv->play, rate,
        GST_FORMAT_TIME,
        GST_SEEK_FLAG_FLUSH | GST_SEEK_FLAG_KEY_UNIT,
        GST_SEEK_TYPE_SET, time * GST_MSECOND,
        GST_SEEK_TYPE_NONE, GST_CLOCK_TIME_NONE);
  }
  return TRUE;
}





gboolean
bacon_video_widget_seek (BaconVideoWidget * bvw, gdouble position, gfloat rate)
{

  gint64 seek_time, length_nanos;

  g_return_val_if_fail (bvw != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (bvw->priv->play), FALSE);

  length_nanos = (gint64) (bvw->priv->stream_length * GST_MSECOND);
  seek_time = (gint64) (length_nanos * position);

  GST_LOG ("Seeking to %3.2f%% %" GST_TIME_FORMAT, position,
      GST_TIME_ARGS (seek_time));

  return bacon_video_widget_seek_time (bvw, seek_time / GST_MSECOND, rate,
      FALSE);
}

gboolean
bacon_video_widget_seek_in_segment (BaconVideoWidget * bvw, gint64 pos,
    gfloat rate)
{

  g_return_val_if_fail (bvw != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (bvw->priv->play), FALSE);

  GST_LOG ("Segment seeking from %" GST_TIME_FORMAT,
      GST_TIME_ARGS (pos * GST_MSECOND));

  if (pos > bvw->priv->stream_length
      && bvw->priv->stream_length > 0
      && !g_str_has_prefix (bvw->priv->mrl, "dvd:")
      && !g_str_has_prefix (bvw->priv->mrl, "vcd:")) {
    if (bvw->priv->eos_id == 0)
      bvw->priv->eos_id = g_idle_add (bvw_signal_eos_delayed, bvw);
    return TRUE;
  }

  got_time_tick (bvw->priv->play, pos * GST_MSECOND, bvw);
  gst_element_seek (bvw->priv->play, rate,
      GST_FORMAT_TIME,
      GST_SEEK_FLAG_FLUSH | GST_SEEK_FLAG_SEGMENT |
      GST_SEEK_FLAG_ACCURATE, GST_SEEK_TYPE_SET,
      pos * GST_MSECOND, GST_SEEK_TYPE_NONE, GST_CLOCK_TIME_NONE);

  return TRUE;
}

gboolean
bacon_video_widget_set_rate_in_segment (BaconVideoWidget * bvw, gfloat rate,
    gint64 stop)
{
  guint64 pos;

  g_return_val_if_fail (bvw != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (bvw->priv->play), FALSE);

  pos = bacon_video_widget_get_accurate_current_time (bvw);
  if (pos == 0)
    return FALSE;

  gst_element_seek (bvw->priv->play, rate,
      GST_FORMAT_TIME,
      GST_SEEK_FLAG_FLUSH | GST_SEEK_FLAG_ACCURATE |
      GST_SEEK_FLAG_SEGMENT, GST_SEEK_TYPE_SET,
      pos * GST_MSECOND, GST_SEEK_TYPE_SET, stop * GST_MSECOND);

  return TRUE;
}

gboolean
bacon_video_widget_set_rate (BaconVideoWidget * bvw, gfloat rate)
{
  guint64 pos;

  g_return_val_if_fail (bvw != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (bvw->priv->play), FALSE);

  pos = bacon_video_widget_get_accurate_current_time (bvw);
  if (pos == 0)
    return FALSE;

  gst_element_seek (bvw->priv->play, rate,
      GST_FORMAT_TIME,
      GST_SEEK_FLAG_FLUSH | GST_SEEK_FLAG_ACCURATE,
      GST_SEEK_TYPE_SET,
      pos * GST_MSECOND, GST_SEEK_TYPE_NONE, GST_CLOCK_TIME_NONE);

  return TRUE;
}


gboolean
bacon_video_widget_new_file_seek (BaconVideoWidget * bvw, gint64 start,
    gint64 stop, gfloat rate)
{

  g_return_val_if_fail (bvw != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (bvw->priv->play), FALSE);

  GST_LOG ("Segment seeking from %" GST_TIME_FORMAT,
      GST_TIME_ARGS (start * GST_MSECOND));

  if (start > bvw->priv->stream_length
      && bvw->priv->stream_length > 0
      && !g_str_has_prefix (bvw->priv->mrl, "dvd:")
      && !g_str_has_prefix (bvw->priv->mrl, "vcd:")) {
    if (bvw->priv->eos_id == 0)
      bvw->priv->eos_id = g_idle_add (bvw_signal_eos_delayed, bvw);
    return TRUE;
  }


  GST_LOG ("Segment seeking from %" GST_TIME_FORMAT,
      GST_TIME_ARGS (start * GST_MSECOND));

  //FIXME Needs to wait until GST_STATE_PAUSED
  gst_element_get_state (bvw->priv->play, NULL, NULL, 0);

  got_time_tick (bvw->priv->play, start * GST_MSECOND, bvw);
  gst_element_seek (bvw->priv->play, rate,
      GST_FORMAT_TIME,
      GST_SEEK_FLAG_FLUSH | GST_SEEK_FLAG_SEGMENT |
      GST_SEEK_FLAG_ACCURATE, GST_SEEK_TYPE_SET,
      start * GST_MSECOND, GST_SEEK_TYPE_SET, stop * GST_MSECOND);
  gst_element_set_state (bvw->priv->play, GST_STATE_PLAYING);

  return TRUE;
}

gboolean
bacon_video_widget_segment_seek (BaconVideoWidget * bvw, gint64 start,
    gint64 stop, gfloat rate)
{

  g_return_val_if_fail (bvw != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (bvw->priv->play), FALSE);

  GST_LOG ("Segment seeking from %" GST_TIME_FORMAT,
      GST_TIME_ARGS (start * GST_MSECOND));

  got_time_tick (bvw->priv->play, start * GST_MSECOND, bvw);
  gst_element_seek (bvw->priv->play, rate,
      GST_FORMAT_TIME,
      GST_SEEK_FLAG_FLUSH | GST_SEEK_FLAG_SEGMENT |
      GST_SEEK_FLAG_ACCURATE, GST_SEEK_TYPE_SET,
      start * GST_MSECOND, GST_SEEK_TYPE_SET, stop * GST_MSECOND);

  return TRUE;
}

gboolean
bacon_video_widget_seek_to_next_frame (BaconVideoWidget * bvw, gfloat rate,
    gboolean in_segment)
{
  gint64 pos = -1;
  gboolean ret;

  g_return_val_if_fail (bvw != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (bvw->priv->play), FALSE);

  gst_element_send_event(bvw->priv->video_sink,
      gst_event_new_step (GST_FORMAT_BUFFERS, 1, 1.0, TRUE, FALSE));

  pos = bacon_video_widget_get_accurate_current_time (bvw);
  got_time_tick (GST_ELEMENT (bvw->priv->play), pos * GST_MSECOND, bvw);

  gst_x_overlay_expose (bvw->priv->xoverlay);

  return ret;
}

gboolean
bacon_video_widget_seek_to_previous_frame (BaconVideoWidget * bvw,
    gfloat rate, gboolean in_segment)
{
  gint fps;
  gint64 pos;
  gint64 final_pos;
  guint8 seek_flags;
  gboolean ret;

  g_return_val_if_fail (bvw != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (bvw->priv->play), FALSE);


  //Round framerate to the nearest integer
  fps = (bvw->priv->video_fps_n + bvw->priv->video_fps_d / 2) /
      bvw->priv->video_fps_d;
  pos = bacon_video_widget_get_accurate_current_time (bvw);
  final_pos = pos * GST_MSECOND - 1 * GST_SECOND / fps;

  if (pos == 0)
    return FALSE;

  if (bacon_video_widget_is_playing (bvw))
    bacon_video_widget_pause (bvw);

  seek_flags = GST_SEEK_FLAG_FLUSH | GST_SEEK_FLAG_ACCURATE;
  if (in_segment)
    seek_flags = seek_flags | GST_SEEK_FLAG_SEGMENT;
  ret = gst_element_seek (bvw->priv->play, rate,
      GST_FORMAT_TIME, seek_flags, GST_SEEK_TYPE_SET,
      final_pos, GST_SEEK_TYPE_NONE, GST_CLOCK_TIME_NONE);
  gst_x_overlay_expose (bvw->priv->xoverlay);

  got_time_tick (GST_ELEMENT (bvw->priv->play), pos * GST_MSECOND, bvw);

  return ret;
}

gboolean
bacon_video_widget_segment_stop_update (BaconVideoWidget * bvw, gint64 stop,
    gfloat rate)
{
  g_return_val_if_fail (bvw != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (bvw->priv->play), FALSE);

  gst_element_seek (bvw->priv->play, rate,
      GST_FORMAT_TIME,
      GST_SEEK_FLAG_FLUSH | GST_SEEK_FLAG_SEGMENT |
      GST_SEEK_FLAG_ACCURATE, GST_SEEK_TYPE_SET,
      stop * GST_MSECOND - 1, GST_SEEK_TYPE_SET, stop * GST_MSECOND);

  if (bacon_video_widget_is_playing (bvw))
    bacon_video_widget_pause (bvw);

  gst_x_overlay_expose (bvw->priv->xoverlay);

  return TRUE;
}

gboolean
bacon_video_widget_segment_start_update (BaconVideoWidget * bvw, gint64 start,
    gfloat rate)
{
  g_return_val_if_fail (bvw != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (bvw->priv->play), FALSE);

  gst_element_seek (bvw->priv->play, rate,
      GST_FORMAT_TIME,
      GST_SEEK_FLAG_FLUSH | GST_SEEK_FLAG_SEGMENT |
      GST_SEEK_FLAG_ACCURATE, GST_SEEK_TYPE_SET,
      start * GST_MSECOND, GST_SEEK_TYPE_NONE, GST_CLOCK_TIME_NONE);

  if (bacon_video_widget_is_playing (bvw))
    bacon_video_widget_pause (bvw);

  gst_x_overlay_expose (bvw->priv->xoverlay);

  return TRUE;
}


static void
bvw_stop_play_pipeline (BaconVideoWidget * bvw)
{
  GstState cur_state;

  gst_element_get_state (bvw->priv->play, &cur_state, NULL, 0);
  if (cur_state > GST_STATE_READY) {
    GstMessage *msg;
    GstBus *bus;

    GST_INFO ("stopping");
    gst_element_set_state (bvw->priv->play, GST_STATE_READY);

    /* process all remaining state-change messages so everything gets
     * cleaned up properly (before the state change to NULL flushes them) */
    GST_INFO ("processing pending state-change messages");
    bus = gst_element_get_bus (bvw->priv->play);
    while ((msg = gst_bus_poll (bus, GST_MESSAGE_STATE_CHANGED, 0))) {
      gst_bus_async_signal_func (bus, msg, NULL);
      gst_message_unref (msg);
    }
    gst_object_unref (bus);
  }

  gst_element_set_state (bvw->priv->play, GST_STATE_NULL);
  bvw->priv->target_state = GST_STATE_NULL;
  bvw->priv->buffering = FALSE;
  bvw->priv->plugin_install_in_progress = FALSE;
  bvw->priv->ignore_messages_mask = 0;
  GST_INFO ("stopped");
}

/**
 * bacon_video_widget_stop:
 * @bvw: a #BaconVideoWidget
 *
 * Stops playing the current stream and resets to the first position in the stream.
 **/
void
bacon_video_widget_stop (BaconVideoWidget * bvw)
{
  g_return_if_fail (bvw != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));
  g_return_if_fail (GST_IS_ELEMENT (bvw->priv->play));

  GST_LOG ("Stopping");
  bvw_stop_play_pipeline (bvw);

  /* Reset position to 0 when stopping */
  got_time_tick (GST_ELEMENT (bvw->priv->play), 0, bvw);
}


/**
 * bacon_video_widget_close:
 * @bvw: a #BaconVideoWidget
 *
 * Closes the current stream and frees the resources associated with it.
 **/
void
bacon_video_widget_close (BaconVideoWidget * bvw)
{
  g_return_if_fail (bvw != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));
  g_return_if_fail (GST_IS_ELEMENT (bvw->priv->play));

  GST_LOG ("Closing");
  bvw_stop_play_pipeline (bvw);

  g_free (bvw->priv->mrl);
  bvw->priv->mrl = NULL;
  bvw->priv->is_live = FALSE;
  bvw->priv->window_resized = FALSE;

  g_object_notify (G_OBJECT (bvw), "seekable");
  g_signal_emit (bvw, bvw_signals[SIGNAL_CHANNELS_CHANGE], 0);
  got_time_tick (GST_ELEMENT (bvw->priv->play), 0, bvw);
}


void
bacon_video_widget_redraw_last_frame (BaconVideoWidget * bvw)
{
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));
  g_return_if_fail (bvw->priv->xoverlay != NULL);

  if (!bvw->priv->logo_mode && !bacon_video_widget_is_playing (bvw)) {
    gst_x_overlay_expose (bvw->priv->xoverlay);
  }
}

#if 0
static void
bvw_do_navigation_command (BaconVideoWidget * bvw, GstNavigationCommand command)
{
  GstNavigation *nav = bvw_get_navigation_iface (bvw);
  if (nav == NULL)
    return;

  gst_navigation_send_command (nav, command);
  gst_object_unref (GST_OBJECT (nav));
}

/**
 * bacon_video_widget_dvd_event:
 * @bvw: a #BaconVideoWidget
 * @type: the type of DVD event to issue
 *
 * Issues a DVD navigation event to the video widget, such as one to skip to the
 * next chapter, or navigate to the DVD title menu.
 *
 * This is a no-op if the current stream is not navigable.
 **/
void
bacon_video_widget_dvd_event (BaconVideoWidget * bvw, BvwDVDEvent type)
{
  g_return_if_fail (bvw != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));
  g_return_if_fail (GST_IS_ELEMENT (bvw->priv->play));

  switch (type) {
    case BVW_DVD_ROOT_MENU:
      bvw_do_navigation_command (bvw, GST_NAVIGATION_COMMAND_DVD_MENU);
      break;
    case BVW_DVD_TITLE_MENU:
      bvw_do_navigation_command (bvw, GST_NAVIGATION_COMMAND_DVD_TITLE_MENU);
      break;
    case BVW_DVD_SUBPICTURE_MENU:
      bvw_do_navigation_command (bvw,
          GST_NAVIGATION_COMMAND_DVD_SUBPICTURE_MENU);
      break;
    case BVW_DVD_AUDIO_MENU:
      bvw_do_navigation_command (bvw, GST_NAVIGATION_COMMAND_DVD_AUDIO_MENU);
      break;
    case BVW_DVD_ANGLE_MENU:
      bvw_do_navigation_command (bvw, GST_NAVIGATION_COMMAND_DVD_ANGLE_MENU);
      break;
    case BVW_DVD_CHAPTER_MENU:
      bvw_do_navigation_command (bvw, GST_NAVIGATION_COMMAND_DVD_CHAPTER_MENU);
      break;
    case BVW_DVD_NEXT_ANGLE:
      bvw_do_navigation_command (bvw, GST_NAVIGATION_COMMAND_NEXT_ANGLE);
      break;
    case BVW_DVD_PREV_ANGLE:
      bvw_do_navigation_command (bvw, GST_NAVIGATION_COMMAND_PREV_ANGLE);
      break;
    case BVW_DVD_ROOT_MENU_UP:
      bvw_do_navigation_command (bvw, GST_NAVIGATION_COMMAND_UP);
      break;
    case BVW_DVD_ROOT_MENU_DOWN:
      bvw_do_navigation_command (bvw, GST_NAVIGATION_COMMAND_DOWN);
      break;
    case BVW_DVD_ROOT_MENU_LEFT:
      bvw_do_navigation_command (bvw, GST_NAVIGATION_COMMAND_LEFT);
      break;
    case BVW_DVD_ROOT_MENU_RIGHT:
      bvw_do_navigation_command (bvw, GST_NAVIGATION_COMMAND_RIGHT);
      break;
    case BVW_DVD_ROOT_MENU_SELECT:
      bvw_do_navigation_command (bvw, GST_NAVIGATION_COMMAND_ACTIVATE);
      break;
    case BVW_DVD_NEXT_CHAPTER:
    case BVW_DVD_PREV_CHAPTER:
    case BVW_DVD_NEXT_TITLE:
    case BVW_DVD_PREV_TITLE:
    {
      const gchar *fmt_name;
      GstFormat fmt;
      gint64 val;
      gint dir;

      if (type == BVW_DVD_NEXT_CHAPTER || type == BVW_DVD_NEXT_TITLE)
        dir = 1;
      else
        dir = -1;

      if (type == BVW_DVD_NEXT_CHAPTER || type == BVW_DVD_PREV_CHAPTER)
        fmt_name = "chapter";
      else if (type == BVW_DVD_NEXT_TITLE || type == BVW_DVD_PREV_TITLE)
        fmt_name = "title";
      else
        fmt_name = "angle";

      fmt = gst_format_get_by_nick (fmt_name);
      if (gst_element_query_position (bvw->priv->play, &fmt, &val)) {
        GST_DEBUG ("current %s is: %" G_GINT64_FORMAT, fmt_name, val);
        val += dir;
        GST_DEBUG ("seeking to %s: %" G_GINT64_FORMAT, val);
        gst_element_seek (bvw->priv->play, 1.0, fmt, GST_SEEK_FLAG_FLUSH,
            GST_SEEK_TYPE_SET, val, GST_SEEK_TYPE_NONE, 0);
      } else {
        GST_DEBUG ("failed to query position (%s)", fmt_name);
      }
      break;
    }
    default:
      GST_WARNING ("unhandled type %d", type);
      break;
  }
}
#endif

/**
 * bacon_video_widget_set_logo:
 * @bvw: a #BaconVideoWidget
 * @filename: the logo filename
 *
 * Sets the logo displayed on the video widget when no stream is loaded.
 **/
void
bacon_video_widget_set_logo (BaconVideoWidget * bvw, gchar * filename)
{
  GError *error = NULL;

  g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));
  g_return_if_fail (filename != NULL);

  if (bvw->priv->logo_pixbuf != NULL)
    g_object_unref (bvw->priv->logo_pixbuf);

  bvw->priv->logo_pixbuf = gdk_pixbuf_new_from_file (filename, &error);

  if (error) {
    g_warning ("An error occurred trying to open logo %s: %s",
        filename, error->message);
    g_error_free (error);
  }
}

/**
 * bacon_video_widget_set_logo_pixbuf:
 * @bvw: a #BaconVideoWidget
 * @logo: the logo #GdkPixbuf
 *
 * Sets the logo displayed on the video widget when no stream is loaded,
 * by passing in a #GdkPixbuf directly. @logo is reffed, so can be unreffed
 * once this function call is complete.
 **/
void
bacon_video_widget_set_logo_pixbuf (BaconVideoWidget * bvw, GdkPixbuf * logo)
{
  g_return_if_fail (bvw != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));
  g_return_if_fail (logo != NULL);

  if (bvw->priv->logo_pixbuf != NULL)
    g_object_unref (bvw->priv->logo_pixbuf);

  g_object_ref (logo);
  bvw->priv->logo_pixbuf = logo;
}

/**
 * bacon_video_widget_set_logo_mode:
 * @bvw: a #BaconVideoWidget
 * @logo_mode: %TRUE to display the logo, %FALSE otherwise
 *
 * Sets whether to display a logo set with @bacon_video_widget_set_logo when
 * no stream is loaded. If @logo_mode is %FALSE, nothing will be displayed
 * and the video widget will take up no space. Otherwise, the logo will be
 * displayed and will requisition a corresponding amount of space.
 **/
void
bacon_video_widget_set_logo_mode (BaconVideoWidget * bvw, gboolean logo_mode)
{
  BaconVideoWidgetPrivate *priv;

  g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));
  priv = bvw->priv;

  logo_mode = logo_mode != FALSE;

  g_mutex_lock (bvw->priv->lock);

  if (priv->logo_mode != logo_mode) {
    priv->logo_mode = logo_mode;

    if (logo_mode) {
#if defined (GDK_WINDOWING_QUARTZ)
      gtk_widget_show (priv->logo_da);
      gtk_widget_hide (priv->video_da);
#else
      GTK_WIDGET_SET_FLAGS (GTK_WIDGET (bvw->priv->video_da), GTK_DOUBLE_BUFFERED);
#endif
    } else {
#if defined (GDK_WINDOWING_QUARTZ)
      gtk_widget_show (priv->video_da);
      gtk_widget_hide (priv->logo_da);
#else
      GTK_WIDGET_UNSET_FLAGS (GTK_WIDGET (bvw->priv->video_da), GTK_DOUBLE_BUFFERED);
#endif
    }

    g_mutex_unlock (bvw->priv->lock);

    g_object_notify (G_OBJECT (bvw), "logo_mode");
    g_object_notify (G_OBJECT (bvw), "seekable");

    /* Queue a redraw of the widget */
    gtk_widget_queue_draw (GTK_WIDGET (bvw));
  } else {
    g_mutex_unlock (bvw->priv->lock);
  }
}

/**
 * bacon_video_widget_get_logo_mode
 * @bvw: a #BaconVideoWidget
 *
 * Gets whether the logo is displayed when no stream is loaded.
 *
 * Return value: %TRUE if the logo is displayed, %FALSE otherwise
 **/
gboolean
bacon_video_widget_get_logo_mode (BaconVideoWidget * bvw)
{
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), FALSE);

  return bvw->priv->logo_mode;
}

void
bacon_video_widget_set_drawing_pixbuf (BaconVideoWidget * bvw,
    GdkPixbuf * drawing)
{
  g_return_if_fail (bvw != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));
  g_return_if_fail (drawing != NULL);

  if (bvw->priv->drawing_pixbuf != NULL)
    g_object_unref (bvw->priv->drawing_pixbuf);

  g_object_ref (drawing);
  bvw->priv->drawing_pixbuf = drawing;
}

void
bacon_video_widget_set_drawing_mode (BaconVideoWidget * bvw,
    gboolean drawing_mode)
{
  g_return_if_fail (bvw != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));

  bvw->priv->drawing_mode = drawing_mode;
}

/**
 * bacon_video_widget_pause:
 * @bvw: a #BaconVideoWidget
 *
 * Pauses the current stream in the video widget.
 *
 * If a live stream is being played, playback is stopped entirely.
 **/
void
bacon_video_widget_pause (BaconVideoWidget * bvw)
{
  g_return_if_fail (bvw != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));
  g_return_if_fail (GST_IS_ELEMENT (bvw->priv->play));
  g_return_if_fail (bvw->priv->mrl != NULL);

  if (bvw->priv->is_live != FALSE) {
    GST_LOG ("Stopping because we have a live stream");
    bacon_video_widget_stop (bvw);
    return;
  }

  GST_LOG ("Pausing");
  gst_element_set_state (GST_ELEMENT (bvw->priv->play), GST_STATE_PAUSED);
  bvw->priv->target_state = GST_STATE_PAUSED;
}

/**
 * bacon_video_widget_set_subtitle_font:
 * @bvw: a #BaconVideoWidget
 * @font: a font description string
 *
 * Sets the font size and style in which to display subtitles.
 *
 * @font is a Pango font description string, as understood by
 * pango_font_description_from_string().
 **/
void
bacon_video_widget_set_subtitle_font (BaconVideoWidget * bvw,
    const gchar * font)
{
  g_return_if_fail (bvw != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));
  g_return_if_fail (GST_IS_ELEMENT (bvw->priv->play));

  if (!g_object_class_find_property
      (G_OBJECT_GET_CLASS (bvw->priv->play), "subtitle-font-desc"))
    return;
  g_object_set (bvw->priv->play, "subtitle-font-desc", font, NULL);
}

/**
 * bacon_video_widget_set_subtitle_encoding:
 * @bvw: a #BaconVideoWidget
 * @encoding: an encoding system
 *
 * Sets the encoding system for the subtitles, so that they can be decoded
 * properly.
 **/
void
bacon_video_widget_set_subtitle_encoding (BaconVideoWidget * bvw,
    const char *encoding)
{
  g_return_if_fail (bvw != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));
  g_return_if_fail (GST_IS_ELEMENT (bvw->priv->play));

  if (!g_object_class_find_property
      (G_OBJECT_GET_CLASS (bvw->priv->play), "subtitle-encoding"))
    return;
  g_object_set (bvw->priv->play, "subtitle-encoding", encoding, NULL);
}

/**
 * bacon_video_widget_can_set_volume:
 * @bvw: a #BaconVideoWidget
 *
 * Returns whether the volume level can be set, given the current settings.
 *
 * The volume cannot be set if the audio output type is set to
 * %BVW_AUDIO_SOUND_AC3PASSTHRU.
 *
 * Return value: %TRUE if the volume can be set, %FALSE otherwise
 **/
gboolean
bacon_video_widget_can_set_volume (BaconVideoWidget * bvw)
{
  g_return_val_if_fail (bvw != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (bvw->priv->play), FALSE);

  if (bvw->priv->speakersetup == BVW_AUDIO_SOUND_AC3PASSTHRU)
    return FALSE;

  return !bvw->priv->uses_fakesink;
}

/**
 * bacon_video_widget_set_volume:
 * @bvw: a #BaconVideoWidget
 * @volume: the new volume level, as a percentage between %0 and %1
 *
 * Sets the volume level of the stream as a percentage between %0 and %1.
 *
 * If bacon_video_widget_can_set_volume() returns %FALSE, this is a no-op.
 **/
void
bacon_video_widget_set_volume (BaconVideoWidget * bvw, double volume)
{
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));
  g_return_if_fail (GST_IS_ELEMENT (bvw->priv->play));

  if (bacon_video_widget_can_set_volume (bvw) != FALSE) {
    volume = CLAMP (volume, 0.0, 1.0);
    g_object_set (bvw->priv->play, "volume", (gdouble) volume, NULL);
    g_object_notify (G_OBJECT (bvw), "volume");
  }
}

/**
 * bacon_video_widget_get_volume:
 * @bvw: a #BaconVideoWidget
 *
 * Returns the current volume level, as a percentage between %0 and %1.
 *
 * Return value: the volume as a percentage between %0 and %1
 **/
double
bacon_video_widget_get_volume (BaconVideoWidget * bvw)
{
  double vol;

  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), 0.0);
  g_return_val_if_fail (GST_IS_ELEMENT (bvw->priv->play), 0.0);

  g_object_get (G_OBJECT (bvw->priv->play), "volume", &vol, NULL);

  return vol;
}

/**
 * bacon_video_widget_set_fullscreen:
 * @bvw: a #BaconVideoWidget
 * @fullscreen: %TRUE to go fullscreen, %FALSE otherwise
 *
 * Sets whether the widget renders the stream in fullscreen mode.
 *
 * Fullscreen rendering is done only when possible, as xvidmode is required.
 **/
/*void*/
/*bacon_video_widget_set_fullscreen (BaconVideoWidget * bvw, gboolean fullscreen)*/
/*{*/
  /*gboolean have_xvidmode;*/

  /*g_return_if_fail (bvw != NULL);*/
  /*g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));*/

  /*g_object_get (G_OBJECT (bvw->priv->bacon_resize),*/
      /*"have-xvidmode", &have_xvidmode, NULL);*/

  /*if (have_xvidmode == FALSE)*/
    /*return;*/

  /*bvw->priv->fullscreen_mode = fullscreen;*/

  /*if (fullscreen == FALSE) {*/
    /*bacon_resize_restore (bvw->priv->bacon_resize);*/
    /*[> Turn fullscreen on when we have xvidmode <]*/
  /*} else if (have_xvidmode != FALSE) {*/
    /*bacon_resize_resize (bvw->priv->bacon_resize);*/
  /*}*/
/*}*/


/**
 * bacon_video_widget_set_show_cursor:
 * @bvw: a #BaconVideoWidget
 * @show_cursor: %TRUE to show the cursor, %FALSE otherwise
 *
 * Sets whether the cursor should be shown when it is over the video
 * widget. If @show_cursor is %FALSE, the cursor will be invisible
 * when it is moved over the video widget.
 **/
void
bacon_video_widget_set_show_cursor (BaconVideoWidget * bvw,
    gboolean show_cursor)
{
  GdkWindow *window;

  g_return_if_fail (bvw != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));

  bvw->priv->cursor_shown = show_cursor;
  window = gtk_widget_get_window (GTK_WIDGET (bvw));

  if (!window) {
    return;
  }

  if (show_cursor == FALSE) {
    totem_gdk_window_set_invisible_cursor (window);
  } else {
    gdk_window_set_cursor (window, bvw->priv->cursor);
  }
}

/**
 * bacon_video_widget_get_show_cursor:
 * @bvw: a #BaconVideoWidget
 *
 * Returns whether the cursor is shown when it is over the video widget.
 *
 * Return value: %TRUE if the cursor is shown, %FALSE otherwise
 **/
gboolean
bacon_video_widget_get_show_cursor (BaconVideoWidget * bvw)
{
  g_return_val_if_fail (bvw != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), FALSE);

  return bvw->priv->cursor_shown;
}

/**
 * bacon_video_widget_get_auto_resize:
 * @bvw: a #BaconVideoWidget
 *
 * Returns whether the widget will automatically resize to fit videos.
 *
 * Return value: %TRUE if the widget will resize, %FALSE otherwise
 **/
gboolean
bacon_video_widget_get_auto_resize (BaconVideoWidget * bvw)
{
  g_return_val_if_fail (bvw != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), FALSE);

  return bvw->priv->auto_resize;
}

/**
 * bacon_video_widget_set_auto_resize:
 * @bvw: a #BaconVideoWidget
 * @auto_resize: %TRUE to automatically resize for new videos, %FALSE otherwise
 *
 * Sets whether the widget should automatically resize to fit to new videos when
 * they are loaded. Changes to this will take effect when the next media file is
 * loaded.
 **/
void
bacon_video_widget_set_auto_resize (BaconVideoWidget * bvw,
    gboolean auto_resize)
{
  g_return_if_fail (bvw != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));

  bvw->priv->auto_resize = auto_resize;

  /* this will take effect when the next media file loads */
}

/**
 * bacon_video_widget_set_aspect_ratio:
 * @bvw: a #BaconVideoWidget
 * @ratio: the new aspect ratio
 *
 * Sets the aspect ratio used by the widget, from #BaconVideoWidgetAspectRatio.
 *
 * Changes to this take effect immediately.
 **/
void
bacon_video_widget_set_aspect_ratio (BaconVideoWidget * bvw,
    BvwAspectRatio ratio)
{
  g_return_if_fail (bvw != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));

  bvw->priv->ratio_type = ratio;
  got_video_size (bvw);
}

/**
 * bacon_video_widget_get_aspect_ratio:
 * @bvw: a #BaconVideoWidget
 *
 * Returns the current aspect ratio used by the widget, from
 * #BaconVideoWidgetAspectRatio.
 *
 * Return value: the aspect ratio
 **/
BvwAspectRatio
bacon_video_widget_get_aspect_ratio (BaconVideoWidget * bvw)
{
  g_return_val_if_fail (bvw != NULL, 0);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), 0);

  return bvw->priv->ratio_type;
}

/**
 * bacon_video_widget_get_position:
 * @bvw: a #BaconVideoWidget
 *
 * Returns the current position in the stream, as a value between
 * %0 and %1.
 *
 * Return value: the current position, or %-1
 **/
double
bacon_video_widget_get_position (BaconVideoWidget * bvw)
{
  g_return_val_if_fail (bvw != NULL, -1);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), -1);
  return bvw->priv->current_position;
}

/**
 * bacon_video_widget_get_current_time:
 * @bvw: a #BaconVideoWidget
 *
 * Returns the current position in the stream, as the time (in milliseconds)
 * since the beginning of the stream.
 *
 * Return value: time since the beginning of the stream, in milliseconds, or %-1
 **/
gint64
bacon_video_widget_get_current_time (BaconVideoWidget * bvw)
{
  g_return_val_if_fail (bvw != NULL, -1);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), -1);
  return bvw->priv->current_time;
}

/**
 * bacon_video_widget_get_accurate_current_time:
 * @bvw: a #BaconVideoWidget
 *
 * Returns the current position in the stream, as the time (in milliseconds)
 * since the beginning of the stream.
 *
 * Return value: time since the beginning of the stream querying directly to the pipeline, in milliseconds, or %-1
 **/
gint64
bacon_video_widget_get_accurate_current_time (BaconVideoWidget * bvw)
{
  GstFormat fmt;
  gint64 pos;

  g_return_val_if_fail (bvw != NULL, -1);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), -1);

  fmt = GST_FORMAT_TIME;
  pos = -1;

  gst_element_query_position (bvw->priv->play, &fmt, &pos);

  return pos / GST_MSECOND;

}



/**
 * bacon_video_widget_get_stream_length:
 * @bvw: a #BaconVideoWidget
 *
 * Returns the total length of the stream, in milliseconds.
 *
 * Return value: the stream length, in milliseconds, or %-1
 **/
gint64
bacon_video_widget_get_stream_length (BaconVideoWidget * bvw)
{
  g_return_val_if_fail (bvw != NULL, -1);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), -1);

  if (bvw->priv->stream_length == 0 && bvw->priv->play != NULL) {
    GstFormat fmt = GST_FORMAT_TIME;
    gint64 len = -1;

    if (gst_element_query_duration (bvw->priv->play, &fmt, &len)
        && len != -1) {
      bvw->priv->stream_length = len / GST_MSECOND;
    }
  }

  return bvw->priv->stream_length;
}

/**
 * bacon_video_widget_is_playing:
 * @bvw: a #BaconVideoWidget
 *
 * Returns whether the widget is currently playing a stream.
 *
 * Return value: %TRUE if a stream is playing, %FALSE otherwise
 **/
gboolean
bacon_video_widget_is_playing (BaconVideoWidget * bvw)
{
  gboolean ret;

  g_return_val_if_fail (bvw != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (bvw->priv->play), FALSE);

  ret = (bvw->priv->target_state == GST_STATE_PLAYING);
  GST_LOG ("%splaying", (ret) ? "" : "not ");

  return ret;
}

/**
 * bacon_video_widget_is_seekable:
 * @bvw: a #BaconVideoWidget
 *
 * Returns whether seeking is possible in the current stream.
 *
 * If no stream is loaded, %FALSE is returned.
 *
 * Return value: %TRUE if the stream is seekable, %FALSE otherwise
 **/
gboolean
bacon_video_widget_is_seekable (BaconVideoWidget * bvw)
{
  gboolean res;
  gint old_seekable;

  g_return_val_if_fail (bvw != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (bvw->priv->play), FALSE);

  if (bvw->priv->mrl == NULL)
    return FALSE;

  old_seekable = bvw->priv->seekable;

  if (bvw->priv->seekable == -1) {
    GstQuery *query;

    query = gst_query_new_seeking (GST_FORMAT_TIME);
    if (gst_element_query (bvw->priv->play, query)) {
      gst_query_parse_seeking (query, NULL, &res, NULL, NULL);
      bvw->priv->seekable = (res) ? 1 : 0;
    } else {
      GST_DEBUG ("seeking query failed");
    }
    gst_query_unref (query);
  }

  if (bvw->priv->seekable != -1) {
    res = (bvw->priv->seekable != 0);
    goto done;
  }

  /* try to guess from duration (this is very unreliable though) */
  if (bvw->priv->stream_length == 0) {
    res = (bacon_video_widget_get_stream_length (bvw) > 0);
  } else {
    res = (bvw->priv->stream_length > 0);
  }

done:

  if (old_seekable != bvw->priv->seekable)
    g_object_notify (G_OBJECT (bvw), "seekable");

  GST_DEBUG ("stream is%s seekable", (res) ? "" : " not");
  return res;
}


static struct _metadata_map_info
{
  BvwMetadataType type;
  const gchar *str;
} metadata_str_map[] = {
  {
  BVW_INFO_TITLE, "title"}, {
  BVW_INFO_ARTIST, "artist"}, {
  BVW_INFO_YEAR, "year"}, {
  BVW_INFO_COMMENT, "comment"}, {
  BVW_INFO_ALBUM, "album"}, {
  BVW_INFO_DURATION, "duration"}, {
  BVW_INFO_TRACK_NUMBER, "track-number"}, {
  BVW_INFO_HAS_VIDEO, "has-video"}, {
  BVW_INFO_DIMENSION_X, "dimension-x"}, {
  BVW_INFO_DIMENSION_Y, "dimension-y"}, {
  BVW_INFO_VIDEO_BITRATE, "video-bitrate"}, {
  BVW_INFO_VIDEO_CODEC, "video-codec"}, {
  BVW_INFO_FPS, "fps"}, {
  BVW_INFO_HAS_AUDIO, "has-audio"}, {
  BVW_INFO_AUDIO_BITRATE, "audio-bitrate"}, {
  BVW_INFO_AUDIO_CODEC, "audio-codec"}, {
  BVW_INFO_AUDIO_SAMPLE_RATE, "samplerate"}, {
  BVW_INFO_AUDIO_CHANNELS, "channels"}, {
  BVW_INFO_PAR, "pixel-aspect-ratio"}
};

static const gchar *
get_metadata_type_name (BvwMetadataType type)
{
  guint i;
  for (i = 0; i < G_N_ELEMENTS (metadata_str_map); ++i) {
    if (metadata_str_map[i].type == type)
      return metadata_str_map[i].str;
  }
  return "unknown";
}

static gint
bvw_get_current_stream_num (BaconVideoWidget * bvw, const gchar * stream_type)
{
  gchar *lower, *cur_prop_str;
  gint stream_num = -1;

  if (bvw->priv->play == NULL)
    return stream_num;

  lower = g_ascii_strdown (stream_type, -1);
  cur_prop_str = g_strconcat ("current-", lower, NULL);
  g_object_get (bvw->priv->play, cur_prop_str, &stream_num, NULL);
  g_free (cur_prop_str);
  g_free (lower);

  GST_LOG ("current %s stream: %d", stream_type, stream_num);
  return stream_num;
}

static GstTagList *
bvw_get_tags_of_current_stream (BaconVideoWidget * bvw,
    const gchar * stream_type)
{
  GstTagList *tags = NULL;
  gint stream_num = -1;
  gchar *lower, *cur_sig_str;

  stream_num = bvw_get_current_stream_num (bvw, stream_type);
  if (stream_num < 0)
    return NULL;

  lower = g_ascii_strdown (stream_type, -1);
  cur_sig_str = g_strconcat ("get-", lower, "-tags", NULL);
  g_signal_emit_by_name (bvw->priv->play, cur_sig_str, stream_num, &tags);
  g_free (cur_sig_str);
  g_free (lower);

  GST_LOG ("current %s stream tags %" GST_PTR_FORMAT, stream_type, tags);
  return tags;
}

static GstCaps *
bvw_get_caps_of_current_stream (BaconVideoWidget * bvw,
    const gchar * stream_type)
{
  GstCaps *caps = NULL;
  gint stream_num = -1;
  GstPad *current;
  gchar *lower, *cur_sig_str;

  stream_num = bvw_get_current_stream_num (bvw, stream_type);
  if (stream_num < 0)
    return NULL;

  lower = g_ascii_strdown (stream_type, -1);
  cur_sig_str = g_strconcat ("get-", lower, "-pad", NULL);
  g_signal_emit_by_name (bvw->priv->play, cur_sig_str, stream_num, &current);
  g_free (cur_sig_str);
  g_free (lower);

  if (current != NULL) {
    caps = gst_pad_get_negotiated_caps (current);
    gst_object_unref (current);
  }
  GST_LOG ("current %s stream caps: %" GST_PTR_FORMAT, stream_type, caps);
  return caps;
}

static gboolean
audio_caps_have_LFE (GstStructure * s)
{
  GstAudioChannelPosition *positions;
  gint i, channels;

  if (!gst_structure_get_value (s, "channel-positions") ||
      !gst_structure_get_int (s, "channels", &channels)) {
    return FALSE;
  }

  positions = gst_audio_get_channel_positions (s);
  if (positions == NULL)
    return FALSE;

  for (i = 0; i < channels; ++i) {
    if (positions[i] == GST_AUDIO_CHANNEL_POSITION_LFE) {
      g_free (positions);
      return TRUE;
    }
  }

  g_free (positions);
  return FALSE;
}

static void
bacon_video_widget_get_metadata_string (BaconVideoWidget * bvw,
    BvwMetadataType type, GValue * value)
{
  char *string = NULL;
  gboolean res = FALSE;

  g_value_init (value, G_TYPE_STRING);

  if (bvw->priv->play == NULL) {
    g_value_set_string (value, NULL);
    return;
  }

  switch (type) {
    case BVW_INFO_TITLE:
      if (bvw->priv->tagcache != NULL) {
        res = gst_tag_list_get_string_index (bvw->priv->tagcache,
            GST_TAG_TITLE, 0, &string);
      }
      break;
    case BVW_INFO_ARTIST:
      if (bvw->priv->tagcache != NULL) {
        res = gst_tag_list_get_string_index (bvw->priv->tagcache,
            GST_TAG_ARTIST, 0, &string);
      }
      break;
    case BVW_INFO_YEAR:
      if (bvw->priv->tagcache != NULL) {
        GDate *date;

        if ((res = gst_tag_list_get_date (bvw->priv->tagcache,
                    GST_TAG_DATE, &date))) {
          string = g_strdup_printf ("%d", g_date_get_year (date));
          g_date_free (date);
        }
      }
      break;
    case BVW_INFO_COMMENT:
      if (bvw->priv->tagcache != NULL) {
        res = gst_tag_list_get_string_index (bvw->priv->tagcache,
            GST_TAG_COMMENT, 0, &string);
      }
      break;
    case BVW_INFO_ALBUM:
      if (bvw->priv->tagcache != NULL) {
        res = gst_tag_list_get_string_index (bvw->priv->tagcache,
            GST_TAG_ALBUM, 0, &string);
      }
      break;
    case BVW_INFO_VIDEO_CODEC:
    {
      GstTagList *tags;

      /* try to get this from the stream info first */
      if ((tags = bvw_get_tags_of_current_stream (bvw, "video"))) {
        res = gst_tag_list_get_string (tags, GST_TAG_CODEC, &string);
        gst_tag_list_free (tags);
      }

      /* if that didn't work, try the aggregated tags */
      if (!res && bvw->priv->tagcache != NULL) {
        res = gst_tag_list_get_string (bvw->priv->tagcache,
            GST_TAG_VIDEO_CODEC, &string);
      }
      break;
    }
    case BVW_INFO_AUDIO_CODEC:
    {
      GstTagList *tags;

      /* try to get this from the stream info first */
      if ((tags = bvw_get_tags_of_current_stream (bvw, "audio"))) {
        res = gst_tag_list_get_string (tags, GST_TAG_CODEC, &string);
        gst_tag_list_free (tags);
      }

      /* if that didn't work, try the aggregated tags */
      if (!res && bvw->priv->tagcache != NULL) {
        res = gst_tag_list_get_string (bvw->priv->tagcache,
            GST_TAG_AUDIO_CODEC, &string);
      }
      break;
    }
    case BVW_INFO_AUDIO_CHANNELS:
    {
      GstStructure *s;
      GstCaps *caps;

      caps = bvw_get_caps_of_current_stream (bvw, "audio");
      if (caps) {
        gint channels = 0;

        s = gst_caps_get_structure (caps, 0);
        if ((res = gst_structure_get_int (s, "channels", &channels))) {
          /* FIXME: do something more sophisticated - but what? */
          if (channels > 2 && audio_caps_have_LFE (s)) {
            string = g_strdup_printf ("%s %d.1", _("Surround"), channels - 1);
          } else if (channels == 1) {
            string = g_strdup (_("Mono"));
          } else if (channels == 2) {
            string = g_strdup (_("Stereo"));
          } else {
            string = g_strdup_printf ("%d", channels);
          }
        }
        gst_caps_unref (caps);
      }
      break;
    }
    default:
      g_assert_not_reached ();
  }

  /* Remove line feeds */
  if (string && strstr (string, "\n") != NULL)
    g_strdelimit (string, "\n", ' ');

  if (res && string && g_utf8_validate (string, -1, NULL)) {
    g_value_take_string (value, string);
    GST_DEBUG ("%s = '%s'", get_metadata_type_name (type), string);
  } else {
    g_value_set_string (value, NULL);
    g_free (string);
  }

  return;
}

static void
bacon_video_widget_get_metadata_double (BaconVideoWidget * bvw,
    BvwMetadataType type, GValue * value)
{
  gdouble metadata_double = 0;

  g_value_init (value, G_TYPE_DOUBLE);

  if (bvw->priv->play == NULL) {
    g_value_set_double (value, 0);
    return;
  }

  switch (type) {
    case BVW_INFO_PAR:
    {
      int movie_par_n = gst_value_get_fraction_numerator (bvw->priv->movie_par);
      int movie_par_d = gst_value_get_fraction_denominator (bvw->priv->movie_par);
      metadata_double = (gdouble) movie_par_n / (gdouble) movie_par_d;
      break;
    }
    default:
      g_assert_not_reached();
  }

  g_value_set_double (value, metadata_double);
  GST_DEBUG ("%s = %f", get_metadata_type_name (type), metadata_double);

  return;
}

static void
bacon_video_widget_get_metadata_int (BaconVideoWidget * bvw,
    BvwMetadataType type, GValue * value)
{
  int integer = 0;

  g_value_init (value, G_TYPE_INT);

  if (bvw->priv->play == NULL) {
    g_value_set_int (value, 0);
    return;
  }

  switch (type) {
    case BVW_INFO_DURATION:
      integer = bacon_video_widget_get_stream_length (bvw) / 1000;
      break;
    case BVW_INFO_TRACK_NUMBER:
      if (bvw->priv->tagcache == NULL)
        break;
      if (!gst_tag_list_get_uint (bvw->priv->tagcache,
              GST_TAG_TRACK_NUMBER, (guint *) & integer))
        integer = 0;
      break;
    case BVW_INFO_DIMENSION_X:
      integer = bvw->priv->video_width;
      break;
    case BVW_INFO_DIMENSION_Y:
      integer = bvw->priv->video_height;
      break;
    case BVW_INFO_FPS:
      if (bvw->priv->video_fps_d > 0) {
        /* Round up/down to the nearest integer framerate */
        integer = (bvw->priv->video_fps_n + bvw->priv->video_fps_d / 2) /
            bvw->priv->video_fps_d;
      } else
        integer = 0;
      break;
    case BVW_INFO_AUDIO_BITRATE:
      if (bvw->priv->audiotags == NULL)
        break;
      if (gst_tag_list_get_uint (bvw->priv->audiotags, GST_TAG_BITRATE,
              (guint *) & integer) ||
          gst_tag_list_get_uint (bvw->priv->audiotags,
              GST_TAG_NOMINAL_BITRATE, (guint *) & integer)) {
        integer /= 1000;
      }
      break;
    case BVW_INFO_VIDEO_BITRATE:
      if (bvw->priv->videotags == NULL)
        break;
      if (gst_tag_list_get_uint (bvw->priv->videotags, GST_TAG_BITRATE,
              (guint *) & integer) ||
          gst_tag_list_get_uint (bvw->priv->videotags,
              GST_TAG_NOMINAL_BITRATE, (guint *) & integer)) {
        integer /= 1000;
      }
      break;
    case BVW_INFO_AUDIO_SAMPLE_RATE:
    {
      GstStructure *s;
      GstCaps *caps;

      caps = bvw_get_caps_of_current_stream (bvw, "audio");
      if (caps) {
        s = gst_caps_get_structure (caps, 0);
        gst_structure_get_int (s, "rate", &integer);
        gst_caps_unref (caps);
      }
      break;
    }
    default:
      g_assert_not_reached ();
  }

  g_value_set_int (value, integer);
  GST_DEBUG ("%s = %d", get_metadata_type_name (type), integer);

  return;
}

static void
bacon_video_widget_get_metadata_bool (BaconVideoWidget * bvw,
    BvwMetadataType type, GValue * value)
{
  gboolean boolean = FALSE;

  g_value_init (value, G_TYPE_BOOLEAN);

  if (bvw->priv->play == NULL) {
    g_value_set_boolean (value, FALSE);
    return;
  }

  GST_DEBUG ("tagcache  = %" GST_PTR_FORMAT, bvw->priv->tagcache);
  GST_DEBUG ("videotags = %" GST_PTR_FORMAT, bvw->priv->videotags);
  GST_DEBUG ("audiotags = %" GST_PTR_FORMAT, bvw->priv->audiotags);

  switch (type) {
    case BVW_INFO_HAS_VIDEO:
      boolean = bvw->priv->media_has_video;
      /* if properties dialog, show the metadata we
       * have even if we cannot decode the stream */
      if (!boolean && bvw->priv->use_type == BVW_USE_TYPE_METADATA &&
          bvw->priv->tagcache != NULL &&
          gst_structure_has_field ((GstStructure *) bvw->priv->tagcache,
              GST_TAG_VIDEO_CODEC)) {
        boolean = TRUE;
      }
      break;
    case BVW_INFO_HAS_AUDIO:
      boolean = bvw->priv->media_has_audio;
      /* if properties dialog, show the metadata we
       * have even if we cannot decode the stream */
      if (!boolean && bvw->priv->use_type == BVW_USE_TYPE_METADATA &&
          bvw->priv->tagcache != NULL &&
          gst_structure_has_field ((GstStructure *) bvw->priv->tagcache,
              GST_TAG_AUDIO_CODEC)) {
        boolean = TRUE;
      }
      break;
    default:
      g_assert_not_reached ();
  }

  g_value_set_boolean (value, boolean);
  GST_DEBUG ("%s = %s", get_metadata_type_name (type),
      (boolean) ? "yes" : "no");

  return;
}

static void
bvw_process_pending_tag_messages (BaconVideoWidget * bvw)
{
  GstMessageType events;
  GstMessage *msg;
  GstBus *bus;

  /* process any pending tag messages on the bus NOW, so we can get to
   * the information without/before giving control back to the main loop */

  /* application message is for stream-info */
  events = GST_MESSAGE_TAG | GST_MESSAGE_DURATION | GST_MESSAGE_APPLICATION;
  bus = gst_element_get_bus (bvw->priv->play);
  while ((msg = gst_bus_poll (bus, events, 0))) {
    gst_bus_async_signal_func (bus, msg, NULL);
  }
  gst_object_unref (bus);
}

static GdkPixbuf *
bacon_video_widget_get_metadata_pixbuf (BaconVideoWidget * bvw,
    GstBuffer * buffer)
{
  GdkPixbufLoader *loader;
  GdkPixbuf *pixbuf;

  loader = gdk_pixbuf_loader_new ();
  if (!gdk_pixbuf_loader_write (loader, buffer->data, buffer->size, NULL)) {
    g_object_unref (loader);
    return NULL;
  }
  if (!gdk_pixbuf_loader_close (loader, NULL)) {
    g_object_unref (loader);
    return NULL;
  }

  pixbuf = gdk_pixbuf_loader_get_pixbuf (loader);
  if (pixbuf)
    g_object_ref (pixbuf);
  g_object_unref (loader);
  return pixbuf;
}

static const GValue *
bacon_video_widget_get_best_image (BaconVideoWidget * bvw)
{
  const GValue *cover_value = NULL;
  guint i;

  for (i = 0;; i++) {
    const GValue *value;
    GstBuffer *buffer;
    GstStructure *caps_struct;
    int type;

    value = gst_tag_list_get_value_index (bvw->priv->tagcache,
        GST_TAG_IMAGE, i);
    if (value == NULL)
      break;

    buffer = gst_value_get_buffer (value);

    caps_struct = gst_caps_get_structure (buffer->caps, 0);
    gst_structure_get_enum (caps_struct,
        "image-type", GST_TYPE_TAG_IMAGE_TYPE, &type);
    if (type == GST_TAG_IMAGE_TYPE_UNDEFINED) {
      if (cover_value == NULL)
        cover_value = value;
    } else if (type == GST_TAG_IMAGE_TYPE_FRONT_COVER) {
      cover_value = value;
      break;
    }
  }

  return cover_value;
}

/**
 * bacon_video_widget_get_metadata:
 * @bvw: a #BaconVideoWidget
 * @type: the type of metadata to return
 * @value: a #GValue
 *
 * Provides metadata of the given @type about the current stream in @value.
 *
 * Free the #GValue with g_value_unset().
 **/
void
bacon_video_widget_get_metadata (BaconVideoWidget * bvw,
    BvwMetadataType type, GValue * value)
{
  g_return_if_fail (bvw != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (bvw));
  g_return_if_fail (GST_IS_ELEMENT (bvw->priv->play));

  switch (type) {
    case BVW_INFO_TITLE:
    case BVW_INFO_ARTIST:
    case BVW_INFO_YEAR:
    case BVW_INFO_COMMENT:
    case BVW_INFO_ALBUM:
    case BVW_INFO_VIDEO_CODEC:
      bacon_video_widget_get_metadata_string (bvw, type, value);
      break;
    case BVW_INFO_AUDIO_CODEC:
      bacon_video_widget_get_metadata_string (bvw, type, value);
      break;
    case BVW_INFO_AUDIO_CHANNELS:
      bacon_video_widget_get_metadata_string (bvw, type, value);
      break;
    case BVW_INFO_DURATION:
      bacon_video_widget_get_metadata_int (bvw, type, value);
      break;
    case BVW_INFO_DIMENSION_X:
      bacon_video_widget_get_metadata_int (bvw, type, value);
      break;
    case BVW_INFO_DIMENSION_Y:
      bacon_video_widget_get_metadata_int (bvw, type, value);
      break;
    case BVW_INFO_FPS:
      bacon_video_widget_get_metadata_int (bvw, type, value);
      break;
    case BVW_INFO_AUDIO_BITRATE:
      bacon_video_widget_get_metadata_int (bvw, type, value);
      break;
    case BVW_INFO_VIDEO_BITRATE:
      bacon_video_widget_get_metadata_int (bvw, type, value);
      break;
    case BVW_INFO_TRACK_NUMBER:
    case BVW_INFO_AUDIO_SAMPLE_RATE:
      bacon_video_widget_get_metadata_int (bvw, type, value);
      break;
    case BVW_INFO_HAS_VIDEO:
      bacon_video_widget_get_metadata_bool (bvw, type, value);
      break;
    case BVW_INFO_HAS_AUDIO:
      bacon_video_widget_get_metadata_bool (bvw, type, value);
      break;
    case BVW_INFO_COVER:
    {
      const GValue *cover_value;

      g_value_init (value, G_TYPE_OBJECT);

      if (bvw->priv->tagcache == NULL)
        break;
      cover_value = bacon_video_widget_get_best_image (bvw);
      if (!cover_value) {
        cover_value = gst_tag_list_get_value_index (bvw->priv->tagcache,
            GST_TAG_PREVIEW_IMAGE, 0);
      }
      if (cover_value) {
        GstBuffer *buffer;
        GdkPixbuf *pixbuf;

        buffer = gst_value_get_buffer (cover_value);
        pixbuf = bacon_video_widget_get_metadata_pixbuf (bvw, buffer);
        if (pixbuf)
          g_value_take_object (value, pixbuf);
      }
    }
    case BVW_INFO_PAR:
      bacon_video_widget_get_metadata_double (bvw, type, value);
      break;
    default:
      g_return_if_reached ();
  }

  return;
}

/* Screenshot functions */

/**
 * bacon_video_widget_can_get_frames:
 * @bvw: a #BaconVideoWidget
 * @error: a #GError, or %NULL
 *
 * Determines whether individual frames from the current stream can
 * be returned using bacon_video_widget_get_current_frame().
 *
 * Frames cannot be returned for audio-only streams, unless visualisations
 * are enabled.
 *
 * Return value: %TRUE if frames can be captured, %FALSE otherwise
 **/
gboolean
bacon_video_widget_can_get_frames (BaconVideoWidget * bvw, GError ** error)
{
  g_return_val_if_fail (bvw != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (bvw->priv->play), FALSE);

  /* check for version */
  if (!g_object_class_find_property
      (G_OBJECT_GET_CLASS (bvw->priv->play), "frame")) {
    g_set_error_literal (error, BVW_ERROR, GST_ERROR_GENERIC,
        _("Too old version of GStreamer installed."));
    return FALSE;
  }

  /* check for video */
  if (!bvw->priv->media_has_video) {
    g_set_error_literal (error, BVW_ERROR, GST_ERROR_GENERIC,
        _("Media contains no supported video streams."));
    return FALSE;
  }

  return TRUE;
}

static void
destroy_pixbuf (guchar * pix, gpointer data)
{
  gst_buffer_unref (GST_BUFFER (data));
}

void
bacon_video_widget_unref_pixbuf (GdkPixbuf * pixbuf)
{
  g_object_unref (pixbuf);
}

/**
 * bacon_video_widget_get_current_frame:
 * @bvw: a #BaconVideoWidget
 *
 * Returns a #GdkPixbuf containing the current frame from the playing
 * stream. This will wait for any pending seeks to complete before
 * capturing the frame.
 *
 * Return value: the current frame, or %NULL; unref with g_object_unref()
 **/
GdkPixbuf *
bacon_video_widget_get_current_frame (BaconVideoWidget * bvw)
{
  GstStructure *s;
  GstBuffer *buf = NULL;
  GdkPixbuf *pixbuf;
  GstCaps *to_caps;
  gint outwidth = 0;
  gint outheight = 0;

  g_return_val_if_fail (bvw != NULL, NULL);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (bvw), NULL);
  g_return_val_if_fail (GST_IS_ELEMENT (bvw->priv->play), NULL);

  /*[> when used as thumbnailer, wait for pending seeks to complete <] */
  /*if (bvw->priv->use_type == BVW_USE_TYPE_CAPTURE) */
  /*{ */
  /*gst_element_get_state (bvw->priv->play, NULL, NULL, -1); */
  /*} */
  gst_element_get_state (bvw->priv->play, NULL, NULL, -1);

  /* no video info */
  if (!bvw->priv->video_width || !bvw->priv->video_height) {
    GST_DEBUG ("Could not take screenshot: %s", "no video info");
    g_warning ("Could not take screenshot: %s", "no video info");
    return NULL;
  }

  /* get frame */
  g_object_get (bvw->priv->play, "frame", &buf, NULL);

  if (!buf) {
    GST_DEBUG ("Could not take screenshot: %s", "no last video frame");
    g_warning ("Could not take screenshot: %s", "no last video frame");
    return NULL;
  }

  if (GST_BUFFER_CAPS (buf) == NULL) {
    GST_DEBUG ("Could not take screenshot: %s", "no caps on buffer");
    g_warning ("Could not take screenshot: %s", "no caps on buffer");
    return NULL;
  }

  /* convert to our desired format (RGB24) */
  to_caps = gst_caps_new_simple ("video/x-raw-rgb",
      "bpp", G_TYPE_INT, 24, "depth", G_TYPE_INT, 24,
      /* Note: we don't ask for a specific width/height here, so that
       * videoscale can adjust dimensions from a non-1/1 pixel aspect
       * ratio to a 1/1 pixel-aspect-ratio */
      "pixel-aspect-ratio", GST_TYPE_FRACTION, 1,
      1, "endianness", G_TYPE_INT, G_BIG_ENDIAN,
      "red_mask", G_TYPE_INT, 0xff0000,
      "green_mask", G_TYPE_INT, 0x00ff00,
      "blue_mask", G_TYPE_INT, 0x0000ff, NULL);

  if (bvw->priv->video_fps_n > 0 && bvw->priv->video_fps_d > 0) {
    gst_caps_set_simple (to_caps, "framerate", GST_TYPE_FRACTION,
        bvw->priv->video_fps_n, bvw->priv->video_fps_d, NULL);
  }

  GST_DEBUG ("frame caps: %" GST_PTR_FORMAT, GST_BUFFER_CAPS (buf));
  GST_DEBUG ("pixbuf caps: %" GST_PTR_FORMAT, to_caps);

  /* bvw_frame_conv_convert () takes ownership of the buffer passed */
  buf = bvw_frame_conv_convert (buf, to_caps);

  gst_caps_unref (to_caps);

  if (!buf) {
    GST_DEBUG ("Could not take screenshot: %s", "conversion failed");
    g_warning ("Could not take screenshot: %s", "conversion failed");
    return NULL;
  }

  if (!GST_BUFFER_CAPS (buf)) {
    GST_DEBUG ("Could not take screenshot: %s", "no caps on output buffer");
    g_warning ("Could not take screenshot: %s", "no caps on output buffer");
    return NULL;
  }

  s = gst_caps_get_structure (GST_BUFFER_CAPS (buf), 0);
  gst_structure_get_int (s, "width", &outwidth);
  gst_structure_get_int (s, "height", &outheight);
  g_return_val_if_fail (outwidth > 0 && outheight > 0, NULL);

  /* create pixbuf from that - use our own destroy function */
  pixbuf = gdk_pixbuf_new_from_data (GST_BUFFER_DATA (buf),
      GDK_COLORSPACE_RGB, FALSE, 8, outwidth,
      outheight, GST_ROUND_UP_4 (outwidth * 3), destroy_pixbuf, buf);

  if (!pixbuf) {
    GST_DEBUG ("Could not take screenshot: %s", "could not create pixbuf");
    g_warning ("Could not take screenshot: %s", "could not create pixbuf");
    gst_buffer_unref (buf);
  }

  return pixbuf;
}


/* =========================================== */
/*                                             */
/*          Widget typing & Creation           */
/*                                             */
/* =========================================== */

G_DEFINE_TYPE (BaconVideoWidget, bacon_video_widget, GTK_TYPE_HBOX)
/* applications must use exactly one of bacon_video_widget_get_option_group()
 * OR bacon_video_widget_init_backend(), but not both */
/**
 * bacon_video_widget_get_option_group:
 *
 * Returns the #GOptionGroup containing command-line options for
 * #BaconVideoWidget.
 *
 * Applications must call either this or bacon_video_widget_init_backend() exactly
 * once; but not both.
 *
 * Return value: a #GOptionGroup giving command-line options for #BaconVideoWidget
 **/
     GOptionGroup *bacon_video_widget_get_option_group (void)
{
  return gst_init_get_option_group ();
}

/**
 * bacon_video_widget_init_backend:
 * @argc: pointer to application's argc
 * @argv: pointer to application's argv
 *
 * Initialises #BaconVideoWidget's GStreamer backend. If this fails
 * for the GStreamer backend, your application will be terminated.
 *
 * Applications must call either this or bacon_video_widget_get_option_group() exactly
 * once; but not both.
 **/
void
bacon_video_widget_init_backend (int *argc, char ***argv)
{
  gst_init (argc, argv);
}

GQuark
bacon_video_widget_error_quark (void)
{
  static GQuark q;              /* 0 */

  if (G_UNLIKELY (q == 0)) {
    q = g_quark_from_static_string ("bvw-error-quark");
  }
  return q;
}

static void
bvw_element_msg_sync (GstBus * bus, GstMessage * msg, gpointer data)
{

  BaconVideoWidget *bvw = BACON_VIDEO_WIDGET (data);

  g_assert (msg->type == GST_MESSAGE_ELEMENT);

  if (msg->structure == NULL)
    return;

  /* This only gets sent if we haven't set an ID yet. This is our last
   * chance to set it before the video sink will create its own window */
  if (gst_structure_has_name (msg->structure, "prepare-xwindow-id")) {
    GstObject *sender = GST_MESSAGE_SRC (msg);

    GST_INFO ("Handling sync prepare-xwindow-id message");

    g_mutex_lock (bvw->priv->lock);

    if (bvw->priv->xoverlay != NULL) {
      gst_object_unref (bvw->priv->xoverlay);
    }

    if (sender && GST_IS_X_OVERLAY (sender))
      bvw->priv->xoverlay = GST_X_OVERLAY (gst_object_ref (sender));

    g_return_if_fail (bvw->priv->xoverlay != NULL);
    g_return_if_fail (bvw->priv->window_handle != 0);

    g_object_set (GST_ELEMENT (bvw->priv->xoverlay), "force-aspect-ratio", TRUE, NULL);
    gst_set_window_handle(bvw->priv->xoverlay, bvw->priv->window_handle);
    g_mutex_unlock (bvw->priv->lock);
  }
}

/**
 * bacon_video_widget_new:
 * @width: initial or expected video width, in pixels, or %-1
 * @height: initial or expected video height, in pixels, or %-1
 * @type: the widget's use type
 * @error: a #GError, or %NULL
 *
 * Creates a new #BaconVideoWidget for the purpose specified in @type.
 *
 * If @type is %BVW_USE_TYPE_VIDEO, the #BaconVideoWidget will be fully-featured; other
 * values of @type will enable less functionality on the widget, which will come with
 * corresponding decreases in the size of its memory footprint.
 *
 * @width and @height give the initial or expected video height. Set them to %-1 if the
 * video size is unknown. For small videos, #BaconVideoWidget will be configured differently.
 *
 * A #BvwError will be returned on error.
 *
 * Return value: a new #BaconVideoWidget, or %NULL; destroy with gtk_widget_destroy()
 **/
GtkWidget *
bacon_video_widget_new (int width, int height, BvwUseType type, GError ** err)
{

  BaconVideoWidget *bvw;
  GstElement *audio_sink = NULL, *video_sink = NULL;
  gchar *version_str;

  version_str = gst_version_string ();
  GST_INFO ("Initialised %s", version_str);
  g_free (version_str);

  gst_pb_utils_init ();

  bvw = g_object_new (bacon_video_widget_get_type (), NULL);

  /* show the gui. */
  gtk_widget_show_all (GTK_WIDGET(bvw));

  bacon_video_widget_set_logo_mode (bvw, TRUE);

  bvw->priv->use_type = type;

  GST_INFO ("use_type = %d", type);

  bvw->priv->play = gst_element_factory_make ("playbin2", "play");
  if (!bvw->priv->play) {

    g_set_error (err, BVW_ERROR, GST_ERROR_PLUGIN_LOAD,
        _("Failed to create a GStreamer play object. "
            "Please check your GStreamer installation."));
    g_object_ref_sink (bvw);
    g_object_unref (bvw);
    return NULL;
  }

  bvw->priv->bus = gst_element_get_bus (bvw->priv->play);
  gst_bus_add_signal_watch (bvw->priv->bus);

  bvw->priv->sig_bus_async =
      g_signal_connect (bvw->priv->bus, "message",
      G_CALLBACK (bvw_bus_message_cb), bvw);

  bvw->priv->speakersetup = BVW_AUDIO_SOUND_STEREO;
  bvw->priv->media_device = g_strdup ("/dev/dvd");
  bvw->priv->ratio_type = BVW_RATIO_AUTO;

  bvw->priv->cursor_shown = TRUE;
  bvw->priv->logo_mode = FALSE;
  bvw->priv->auto_resize = FALSE;


  if (type == BVW_USE_TYPE_VIDEO || type == BVW_USE_TYPE_AUDIO) {

    audio_sink = gst_element_factory_make ("autoaudiosink", "audio-sink");

    if (audio_sink == NULL) {
      g_warning ("Could not create element 'autoaudiosink'");
    }
  } else {
    audio_sink = gst_element_factory_make ("fakesink", "audio-fake-sink");
  }

  if (type == BVW_USE_TYPE_VIDEO) {
    video_sink = gst_element_factory_make (DEFAULT_VIDEO_SINK, "video-sink");

    if (video_sink == NULL) {
      g_warning ("Could not create element '%s'", DEFAULT_VIDEO_SINK);
      /* Try to fallback on ximagesink */
      video_sink = gst_element_factory_make ("ximagesink", "video-sink");
    }

  } else {
    video_sink = gst_element_factory_make ("fakesink", "video-fake-sink");
    if (video_sink)
      g_object_set (video_sink, "sync", TRUE, NULL);
  }

  if (video_sink) {
    GstStateChangeReturn ret;

    /* need to set bus explicitly as it's not in a bin yet and
     * poll_for_state_change() needs one to catch error messages */
    gst_element_set_bus (video_sink, bvw->priv->bus);
    /* state change NULL => READY should always be synchronous */
    ret = gst_element_set_state (video_sink, GST_STATE_READY);
    if (ret == GST_STATE_CHANGE_FAILURE) {
      /* Drop this video sink */
      gst_element_set_state (video_sink, GST_STATE_NULL);
      gst_object_unref (video_sink);
      /* Try again with autovideosink */
      video_sink = gst_element_factory_make (BACKUP_VIDEO_SINK, "video-sink");
      gst_element_set_bus (video_sink, bvw->priv->bus);
      ret = gst_element_set_state (video_sink, GST_STATE_READY);
      if (ret == GST_STATE_CHANGE_FAILURE) {
        GstMessage *err_msg;

        err_msg = gst_bus_poll (bvw->priv->bus, GST_MESSAGE_ERROR, 0);
        if (err_msg == NULL) {
          g_warning ("Should have gotten an error message, please file a bug.");
          g_set_error (err, BVW_ERROR, GST_ERROR_VIDEO_PLUGIN,
              _
              ("Failed to open video output. It may not be available. "
                  "Please select another video output in the Multimedia "
                  "Systems Selector."));
        } else if (err_msg) {
          *err = bvw_error_from_gst_error (bvw, err_msg);
          gst_message_unref (err_msg);
        }
        goto sink_error;
      }
    } else {
      bvw->priv->video_sink = video_sink;
    }
  } else {
    g_set_error (err, BVW_ERROR, GST_ERROR_VIDEO_PLUGIN,
        _("Could not find the video output. "
            "You may need to install additional GStreamer plugins, "
            "or select another video output in the Multimedia Systems "
            "Selector."));
    goto sink_error;
  }

  if (audio_sink) {
    GstStateChangeReturn ret;
    GstBus *bus;

    /* need to set bus explicitly as it's not in a bin yet and
     * we need one to catch error messages */
    bus = gst_bus_new ();
    gst_element_set_bus (audio_sink, bus);

    /* state change NULL => READY should always be synchronous */
    ret = gst_element_set_state (audio_sink, GST_STATE_READY);
    gst_element_set_bus (audio_sink, NULL);

    if (ret == GST_STATE_CHANGE_FAILURE) {
      /* doesn't work, drop this audio sink */
      gst_element_set_state (audio_sink, GST_STATE_NULL);
      gst_object_unref (audio_sink);
      audio_sink = NULL;
      /* Hopefully, fakesink should always work */
      if (type != BVW_USE_TYPE_AUDIO)
        audio_sink = gst_element_factory_make ("fakesink", "audio-sink");
      if (audio_sink == NULL) {
        GstMessage *err_msg;

        err_msg = gst_bus_poll (bus, GST_MESSAGE_ERROR, 0);
        if (err_msg == NULL) {
          g_warning ("Should have gotten an error message, please file a bug.");
          g_set_error (err, BVW_ERROR, GST_ERROR_AUDIO_PLUGIN,
              _
              ("Failed to open audio output. You may not have "
                  "permission to open the sound device, or the sound "
                  "server may not be running. "
                  "Please select another audio output in the Multimedia "
                  "Systems Selector."));
        } else if (err) {
          *err = bvw_error_from_gst_error (bvw, err_msg);
          gst_message_unref (err_msg);
        }
        gst_object_unref (bus);
        goto sink_error;
      }
      /* make fakesink sync to the clock like a real sink */
      g_object_set (audio_sink, "sync", TRUE, NULL);
      GST_INFO ("audio sink doesn't work, using fakesink instead");
      bvw->priv->uses_fakesink = TRUE;
    }
    gst_object_unref (bus);
  } else {
    g_set_error (err, BVW_ERROR, GST_ERROR_AUDIO_PLUGIN,
        _("Could not find the audio output. "
            "You may need to install additional GStreamer plugins, or "
            "select another audio output in the Multimedia Systems "
            "Selector."));
    goto sink_error;
  }

  /* set back to NULL to close device again in order to avoid interrupts
   * being generated after startup while there's nothing to play yet */
  gst_element_set_state (audio_sink, GST_STATE_NULL);

  do {
    GstElement *bin;
    GstPad *pad;

    bvw->priv->audio_capsfilter =
        gst_element_factory_make ("capsfilter", "audiofilter");
    bin = gst_bin_new ("audiosinkbin");
    gst_bin_add_many (GST_BIN (bin), bvw->priv->audio_capsfilter,
        audio_sink, NULL);
    gst_element_link_pads (bvw->priv->audio_capsfilter, "src",
        audio_sink, "sink");

    pad = gst_element_get_pad (bvw->priv->audio_capsfilter, "sink");
    gst_element_add_pad (bin, gst_ghost_pad_new ("sink", pad));
    gst_object_unref (pad);

    audio_sink = bin;
  }
  while (0);


  /* now tell playbin */
  g_object_set (bvw->priv->play, "video-sink", video_sink, NULL);
  g_object_set (bvw->priv->play, "audio-sink", audio_sink, NULL);

  g_signal_connect (bvw->priv->play, "notify::source",
      G_CALLBACK (playbin_source_notify_cb), bvw);
  g_signal_connect (bvw->priv->play, "video-changed",
      G_CALLBACK (playbin_stream_changed_cb), bvw);
  g_signal_connect (bvw->priv->play, "audio-changed",
      G_CALLBACK (playbin_stream_changed_cb), bvw);
  g_signal_connect (bvw->priv->play, "text-changed",
      G_CALLBACK (playbin_stream_changed_cb), bvw);

  /* assume we're always called from the main Gtk+ GUI thread */
  gui_thread = g_thread_self ();

  if (type == BVW_USE_TYPE_VIDEO) {
    GstStateChangeReturn ret;

    /* wait for video sink to finish changing to READY state,
     * otherwise we won't be able to detect the colorbalance interface */
    ret = gst_element_get_state (video_sink, NULL, NULL, 5 * GST_SECOND);

    if (ret != GST_STATE_CHANGE_SUCCESS) {
      GST_WARNING ("Timeout setting videosink to READY");
      g_set_error (err, BVW_ERROR, GST_ERROR_VIDEO_PLUGIN,
          _
          ("Failed to open video output. It may not be available. "
              "Please select another video output in the Multimedia Systems Selector."));
      return NULL;
    }

  }

  /* we want to catch "prepare-xwindow-id" element messages synchronously */
  gst_bus_set_sync_handler (bvw->priv->bus, gst_bus_sync_signal_handler, bvw);

  bvw->priv->sig_bus_sync =
      g_signal_connect (bvw->priv->bus, "sync-message::element",
      G_CALLBACK (bvw_element_msg_sync), bvw);

  return GTK_WIDGET (bvw);

  /* errors */
sink_error:
  {
    if (video_sink) {
      gst_element_set_state (video_sink, GST_STATE_NULL);
      gst_object_unref (video_sink);
    }
    if (audio_sink) {
      gst_element_set_state (audio_sink, GST_STATE_NULL);
      gst_object_unref (audio_sink);
    }

    g_object_ref (bvw);
    g_object_ref_sink (G_OBJECT (bvw));
    g_object_unref (bvw);

    return NULL;
  }
}
