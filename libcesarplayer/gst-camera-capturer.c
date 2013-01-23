/* -*- Mode: C; indent-tabs-mode: t; c-basic-offset: 4; tab-width: 4 -*- */
/*
* Gstreamer DV capturer
* Copyright (C)  Andoni Morales Alastruey 2008 <ylatuya@gmail.com>
* 
* Gstreamer DV capturer is free software.
* 
* You may redistribute it and/or modify it under the terms of the
* GNU General Public License, as published by the Free Software
* Foundation; either version 2 of the License, or (at your option)
* any later version.
* 
* Gstreamer DV is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
* See the GNU General Public License for more details.
* 
* You should have received a copy of the GNU General Public License
* along with foob.  If not, write to:
*       The Free Software Foundation, Inc.,
*       51 Franklin Street, Fifth Floor
*       Boston, MA  02110-1301, USA.
*/

#include <string.h>
#include <stdio.h>

#include <gst/app/gstappsrc.h>
#include <gst/interfaces/xoverlay.h>
#include <gst/interfaces/propertyprobe.h>
#include <gst/gst.h>
#include <gst/video/video.h>

#include "gst-camera-capturer.h"
#include "gstscreenshot.h"
#include "common.h"
#include "video-utils.h"


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

GST_DEBUG_CATEGORY (_cesarplayer_gst_debug_cat);
#define GST_CAT_DEFAULT _cesarplayer_gst_debug_cat

/* Signals */
enum
{
  SIGNAL_ERROR,
  SIGNAL_EOS,
  SIGNAL_STATE_CHANGED,
  SIGNAL_DEVICE_CHANGE,
  LAST_SIGNAL
};

/* Properties */
enum
{
  PROP_0,
  PROP_OUTPUT_HEIGHT,
  PROP_OUTPUT_WIDTH,
  PROP_VIDEO_BITRATE,
  PROP_AUDIO_BITRATE,
  PROP_AUDIO_ENABLED,
  PROP_OUTPUT_FILE,
  PROP_DEVICE_ID,
};

struct GstCameraCapturerPrivate
{

  /*Encoding properties */
  gchar *output_file;
  gchar *device_id;
  guint output_height;
  guint output_width;
  guint audio_bitrate;
  guint video_bitrate;
  gboolean audio_enabled;
  VideoEncoderType video_encoder_type;
  AudioEncoderType audio_encoder_type;
  VideoMuxerType video_muxer_type;
  CaptureSourceType source_type;

  /*Video input info */
  gint video_width;             /* Movie width */
  gint video_height;            /* Movie height */
  const GValue *movie_par;      /* Movie pixel aspect ratio */
  gint video_width_pixels;      /* Scaled movie width */
  gint video_height_pixels;     /* Scaled movie height */
  gint video_fps_n;
  gint video_fps_d;
  gboolean media_has_video;
  gboolean media_has_audio;

  /* Snapshots */
  GstBuffer *last_buffer;

  /*GStreamer elements */
  GstElement *main_pipeline;
  GstElement *source_bin;
  GstElement *source_decoder_bin;
  GstElement *decoder_bin;
  GstElement *preview_bin;
  GstElement *encoder_bin;
  GstElement *source;
  GstElement *video_filter;
  GstElement *video_enc;
  GstElement *audio_enc;
  GstElement *muxer;
  GstElement *filesink;
  GstElement* video_appsrc;
  GstElement* audio_appsrc;
  const gchar *source_element_name;

  /* Recording */
  gboolean is_recording;
  gboolean closing_recording;
  gboolean video_needs_keyframe_sync;
  gboolean video_synced;
  GstClockTime accum_recorded_ts;
  GstClockTime last_accum_recorded_ts;
  GstClockTime current_recording_start_ts;
  GstClockTime last_video_buf_ts;
  GstClockTime last_audio_buf_ts;
  GMutex *recording_lock;

  /*Overlay */
  GstXOverlay *xoverlay;        /* protect with lock */
  guintptr window_handle;

  /*Videobox */
  gboolean logo_mode;
  GdkPixbuf *logo_pixbuf;
  gboolean expand_logo;

  /*GStreamer bus */
  GstBus *bus;
  gulong sig_bus_async;
  gulong sig_bus_sync;
};

static GtkWidgetClass *parent_class = NULL;

static GThread *gui_thread;

static int gcc_signals[LAST_SIGNAL] = { 0 };

static void gcc_error_msg (GstCameraCapturer * gcc, GstMessage * msg);
static void gcc_bus_message_cb (GstBus * bus, GstMessage * message,
    gpointer data);
static void gst_camera_capturer_get_property (GObject * object,
    guint property_id, GValue * value, GParamSpec * pspec);
static void gst_camera_capturer_set_property (GObject * object,
    guint property_id, const GValue * value, GParamSpec * pspec);
static void gcc_element_msg_sync (GstBus * bus, GstMessage * msg,
    gpointer data);
static int gcc_get_video_stream_info (GstCameraCapturer * gcc);

G_DEFINE_TYPE (GstCameraCapturer, gst_camera_capturer, GTK_TYPE_DRAWING_AREA);

/***********************************
*
*           GTK Widget
*
************************************/

static gboolean
gst_camera_capturer_configure_event (GtkWidget * widget,
    GdkEventConfigure * event, GstCameraCapturer * gcc)
{
  GstXOverlay *xoverlay = NULL;

  g_return_val_if_fail (gcc != NULL, FALSE);
  g_return_val_if_fail (GST_IS_CAMERA_CAPTURER (gcc), FALSE);

  xoverlay = gcc->priv->xoverlay;

  if (xoverlay != NULL && GST_IS_X_OVERLAY (xoverlay)) {
    gst_x_overlay_expose (xoverlay);
  }

  return FALSE;
}

static void
gst_camera_capturer_realize_event (GtkWidget * widget)
{
  GstCameraCapturer *gcc = GST_CAMERA_CAPTURER (widget);
  GdkWindow *window = gtk_widget_get_window (widget);

  if (!gdk_window_ensure_native (window))
    g_error ("Couldn't create native window needed for GstXOverlay!");

  /* Connect to configure event on the top level window */
  g_signal_connect (G_OBJECT (gtk_widget_get_toplevel (widget)),
      "configure-event", G_CALLBACK (gst_camera_capturer_configure_event), gcc);

  gcc->priv->window_handle = gst_get_window_handle (window);
}

static gboolean
gst_camera_capturer_expose_event (GtkWidget * widget, GdkEventExpose * event)
{
  GstCameraCapturer *gcc = GST_CAMERA_CAPTURER (widget);
  GstXOverlay *xoverlay;
  gboolean draw_logo;
  GdkWindow *win;

  if (event && event->count > 0)
    return TRUE;

  if (event == NULL)
    return TRUE;

  xoverlay = gcc->priv->xoverlay;
  if (xoverlay != NULL) {
    gst_object_ref (xoverlay);
    gst_set_window_handle (xoverlay, gcc->priv->window_handle);
  }

  win = gtk_widget_get_window (widget);

  /* if there's only audio and no visualisation, draw the logo as well */
  draw_logo = gcc->priv->media_has_audio && !gcc->priv->media_has_video;

  if (gcc->priv->logo_mode || draw_logo) {
    /* Start with a nice black canvas */
    gdk_draw_rectangle (win, gtk_widget_get_style (widget)->black_gc, TRUE, 0,
        0, widget->allocation.width, widget->allocation.height);

    if (gcc->priv->logo_pixbuf != NULL) {
      GdkPixbuf *frame;
      /*GdkPixbuf *drawing;*/
      guchar *pixels;
      int rowstride;
      gint width, height, alloc_width, alloc_height, logo_x, logo_y;
      gfloat ratio;

      /* Checking if allocated space is smaller than our logo */


      width = gdk_pixbuf_get_width (gcc->priv->logo_pixbuf);
      height = gdk_pixbuf_get_height (gcc->priv->logo_pixbuf);
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

      if (gcc->priv->expand_logo) { // && !gcc->priv->drawing_mode) {
        /* Scaling to available space */

        frame = gdk_pixbuf_new (GDK_COLORSPACE_RGB,
            FALSE, 8, widget->allocation.width, widget->allocation.height);

        gdk_pixbuf_composite (gcc->priv->logo_pixbuf,
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
          if (xoverlay != NULL)
            gst_object_unref (xoverlay);
          gdk_window_end_paint (win);
          return TRUE;
        }

        frame = gdk_pixbuf_scale_simple (gcc->priv->logo_pixbuf,
            width, height, GDK_INTERP_BILINEAR);
        gdk_draw_pixbuf (win, gtk_widget_get_style (widget)->fg_gc[0],
            frame, 0, 0, logo_x, logo_y, width, height,
            GDK_RGB_DITHER_NONE, 0, 0);

        /*if (gcc->priv->drawing_mode && bvw->priv->drawing_pixbuf != NULL) {*/
          /*drawing =*/
              /*gdk_pixbuf_scale_simple (gcc->priv->drawing_pixbuf, width,*/
              /*height, GDK_INTERP_BILINEAR);*/
          /*gdk_draw_pixbuf (win,*/
              /*gtk_widget_get_style (widget)->fg_gc[0],*/
              /*drawing, 0, 0, logo_x, logo_y, width,*/
              /*height, GDK_RGB_DITHER_NONE, 0, 0);*/
          /*g_object_unref (drawing);*/
        /*}*/

        g_object_unref (frame);
      }
    } else if (win) {
      /* No pixbuf, just draw a black background then */
      gdk_window_clear_area (win,
          0, 0, widget->allocation.width, widget->allocation.height);
    }
  } else {
    /* no logo, pass the expose to gst */
    if (xoverlay != NULL && GST_IS_X_OVERLAY (xoverlay)){
      gst_x_overlay_expose (xoverlay);
    }
    else {
      /* No xoverlay to expose yet */
      gdk_window_clear_area (win,
          0, 0, widget->allocation.width, widget->allocation.height);
    }
  }
  if (xoverlay != NULL)
    gst_object_unref (xoverlay);

  return TRUE;
}

/***********************************
*
*     Class, Object and Properties
*
************************************/

static void
gst_camera_capturer_init (GstCameraCapturer * object)
{
  GstCameraCapturerPrivate *priv;
  object->priv = priv =
      G_TYPE_INSTANCE_GET_PRIVATE (object, GST_TYPE_CAMERA_CAPTURER,
      GstCameraCapturerPrivate);

  GTK_WIDGET_SET_FLAGS (GTK_WIDGET (object), GTK_CAN_FOCUS);
  GTK_WIDGET_UNSET_FLAGS (GTK_WIDGET (object), GTK_DOUBLE_BUFFERED);

  priv->output_height = 480;
  priv->output_width = 640;
  priv->audio_bitrate = 128;
  priv->video_bitrate = 5000;
  priv->last_buffer = NULL;
  priv->expand_logo = TRUE;
  priv->current_recording_start_ts = GST_CLOCK_TIME_NONE;
  priv->accum_recorded_ts = GST_CLOCK_TIME_NONE;
  priv->last_accum_recorded_ts = GST_CLOCK_TIME_NONE;
  priv->last_video_buf_ts = GST_CLOCK_TIME_NONE;
  priv->last_audio_buf_ts = GST_CLOCK_TIME_NONE;
  priv->is_recording = FALSE;
  priv->recording_lock = g_mutex_new();

  priv->video_encoder_type = VIDEO_ENCODER_VP8;
  priv->audio_encoder_type = AUDIO_ENCODER_VORBIS;
  priv->video_muxer_type = VIDEO_MUXER_WEBM;
  priv->source_type = CAPTURE_SOURCE_TYPE_SYSTEM;

  gtk_widget_add_events (GTK_WIDGET (object),
      GDK_POINTER_MOTION_MASK | GDK_BUTTON_PRESS_MASK | GDK_BUTTON_RELEASE_MASK
      | GDK_KEY_PRESS_MASK | GDK_KEY_RELEASE_MASK);

  g_signal_connect (GTK_WIDGET (object), "realize",
      G_CALLBACK (gst_camera_capturer_realize_event), NULL);
}

void
gst_camera_capturer_finalize (GObject * object)
{
  GstCameraCapturer *gcc = (GstCameraCapturer *) object;

  GST_DEBUG_OBJECT (gcc, "Finalizing.");
  if (gcc->priv->bus) {
    /* make bus drop all messages to make sure none of our callbacks is ever
     * called again (main loop might be run again to display error dialog) */
    gst_bus_set_flushing (gcc->priv->bus, TRUE);

    if (gcc->priv->sig_bus_async)
      g_signal_handler_disconnect (gcc->priv->bus, gcc->priv->sig_bus_async);

    if (gcc->priv->sig_bus_sync)
      g_signal_handler_disconnect (gcc->priv->bus, gcc->priv->sig_bus_sync);

    gst_object_unref (gcc->priv->bus);
    gcc->priv->bus = NULL;
  }

  if (gcc->priv->output_file) {
    g_free (gcc->priv->output_file);
    gcc->priv->output_file = NULL;
  }

  if (gcc->priv->device_id) {
    g_free (gcc->priv->device_id);
    gcc->priv->device_id = NULL;
  }

  if (gcc->priv->logo_pixbuf) {
    g_object_unref (gcc->priv->logo_pixbuf);
    gcc->priv->logo_pixbuf = NULL;
  }

  if (gcc->priv->last_buffer != NULL)
    gst_buffer_unref (gcc->priv->last_buffer);

  if (gcc->priv->main_pipeline != NULL
      && GST_IS_ELEMENT (gcc->priv->main_pipeline)) {
    gst_element_set_state (gcc->priv->main_pipeline, GST_STATE_NULL);
    gst_object_unref (gcc->priv->main_pipeline);
    gcc->priv->main_pipeline = NULL;
  }

  G_OBJECT_CLASS (parent_class)->finalize (object);
}

static void
gst_camera_capturer_set_video_bit_rate (GstCameraCapturer * gcc, gint bitrate)
{
  gcc->priv->video_bitrate = bitrate;
  GST_INFO_OBJECT (gcc, "Changed video bitrate to: %d",
      gcc->priv->video_bitrate);
}

static void
gst_camera_capturer_set_audio_bit_rate (GstCameraCapturer * gcc, gint bitrate)
{

  gcc->priv->audio_bitrate = bitrate;
  GST_INFO_OBJECT (gcc, "Changed audio bitrate to: %d",
      gcc->priv->audio_bitrate);
}

static void
gst_camera_capturer_set_audio_enabled (GstCameraCapturer * gcc,
    gboolean enabled)
{
  gcc->priv->audio_enabled = enabled;
  GST_INFO_OBJECT (gcc, "Audio is %s", enabled ? "enabled": "disabled");
}

static void
gst_camera_capturer_set_output_file (GstCameraCapturer * gcc,
    const gchar * file)
{
  gcc->priv->output_file = g_strdup (file);
  GST_INFO_OBJECT (gcc, "Changed output filename to: %s", file);
}

static void
gst_camera_capturer_set_device_id (GstCameraCapturer * gcc,
    const gchar * device_id)
{
  gcc->priv->device_id = g_strdup (device_id);
  GST_INFO_OBJECT (gcc, "Changed device id/name to: %s", gcc->priv->device_id);
}

static void
gst_camera_capturer_set_property (GObject * object, guint property_id,
    const GValue * value, GParamSpec * pspec)
{
  GstCameraCapturer *gcc;

  gcc = GST_CAMERA_CAPTURER (object);

  switch (property_id) {
    case PROP_OUTPUT_HEIGHT:
      gcc->priv->output_height = g_value_get_uint (value);
      break;
    case PROP_OUTPUT_WIDTH:
      gcc->priv->output_width = g_value_get_uint (value);
      break;
    case PROP_VIDEO_BITRATE:
      gst_camera_capturer_set_video_bit_rate (gcc, g_value_get_uint (value));
      break;
    case PROP_AUDIO_BITRATE:
      gst_camera_capturer_set_audio_bit_rate (gcc, g_value_get_uint (value));
      break;
    case PROP_AUDIO_ENABLED:
      gst_camera_capturer_set_audio_enabled (gcc, g_value_get_boolean (value));
      break;
    case PROP_OUTPUT_FILE:
      gst_camera_capturer_set_output_file (gcc, g_value_get_string (value));
      break;
    case PROP_DEVICE_ID:
      gst_camera_capturer_set_device_id (gcc, g_value_get_string (value));
      break;
    default:
      G_OBJECT_WARN_INVALID_PROPERTY_ID (object, property_id, pspec);
      break;
  }
}

static void
gst_camera_capturer_get_property (GObject * object, guint property_id,
    GValue * value, GParamSpec * pspec)
{
  GstCameraCapturer *gcc;

  gcc = GST_CAMERA_CAPTURER (object);

  switch (property_id) {
    case PROP_OUTPUT_HEIGHT:
      g_value_set_uint (value, gcc->priv->output_height);
      break;
    case PROP_OUTPUT_WIDTH:
      g_value_set_uint (value, gcc->priv->output_width);
      break;
    case PROP_AUDIO_BITRATE:
      g_value_set_uint (value, gcc->priv->audio_bitrate);
      break;
    case PROP_VIDEO_BITRATE:
      g_value_set_uint (value, gcc->priv->video_bitrate);
      break;
    case PROP_AUDIO_ENABLED:
      g_value_set_boolean (value, gcc->priv->audio_enabled);
      break;
    case PROP_OUTPUT_FILE:
      g_value_set_string (value, gcc->priv->output_file);
      break;
    case PROP_DEVICE_ID:
      g_value_set_string (value, gcc->priv->device_id);
      break;
    default:
      G_OBJECT_WARN_INVALID_PROPERTY_ID (object, property_id, pspec);
      break;
  }
}

static void
gst_camera_capturer_class_init (GstCameraCapturerClass * klass)
{
  GObjectClass *object_class;
  GtkWidgetClass *widget_class;

  object_class = (GObjectClass *) klass;
  widget_class = (GtkWidgetClass *) klass;
  parent_class = g_type_class_peek_parent (klass);

  g_type_class_add_private (object_class, sizeof (GstCameraCapturerPrivate));

  /* GtkWidget */
  widget_class->expose_event = gst_camera_capturer_expose_event;

  /* GObject */
  object_class->set_property = gst_camera_capturer_set_property;
  object_class->get_property = gst_camera_capturer_get_property;
  object_class->finalize = gst_camera_capturer_finalize;

  /* Properties */
  g_object_class_install_property (object_class, PROP_OUTPUT_HEIGHT,
      g_param_spec_uint ("output_height", NULL,
          NULL, 0, 5600, 576, G_PARAM_READWRITE));
  g_object_class_install_property (object_class, PROP_OUTPUT_WIDTH,
      g_param_spec_uint ("output_width", NULL,
          NULL, 0, 5600, 720, G_PARAM_READWRITE));
  g_object_class_install_property (object_class, PROP_VIDEO_BITRATE,
      g_param_spec_uint ("video_bitrate", NULL,
          NULL, 100, G_MAXUINT, 1000, G_PARAM_READWRITE));
  g_object_class_install_property (object_class, PROP_AUDIO_BITRATE,
      g_param_spec_uint ("audio_bitrate", NULL,
          NULL, 12, G_MAXUINT, 128, G_PARAM_READWRITE));
  g_object_class_install_property (object_class, PROP_AUDIO_ENABLED,
      g_param_spec_boolean ("audio_enabled", NULL,
          NULL, FALSE, G_PARAM_READWRITE));
  g_object_class_install_property (object_class, PROP_OUTPUT_FILE,
      g_param_spec_string ("output_file", NULL,
          NULL, FALSE, G_PARAM_READWRITE));
  g_object_class_install_property (object_class, PROP_DEVICE_ID,
      g_param_spec_string ("device_id", NULL, NULL, FALSE, G_PARAM_READWRITE));

  /* Signals */
  gcc_signals[SIGNAL_ERROR] =
      g_signal_new ("error",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (GstCameraCapturerClass, error),
      NULL, NULL,
      g_cclosure_marshal_VOID__STRING, G_TYPE_NONE, 1, G_TYPE_STRING);

  gcc_signals[SIGNAL_EOS] =
      g_signal_new ("eos",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (GstCameraCapturerClass, eos),
      NULL, NULL, g_cclosure_marshal_VOID__VOID, G_TYPE_NONE, 0);

  gcc_signals[SIGNAL_DEVICE_CHANGE] =
      g_signal_new ("device-change",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (GstCameraCapturerClass, device_change),
      NULL, NULL, g_cclosure_marshal_VOID__INT, G_TYPE_NONE, 1, G_TYPE_INT);
}

/***********************************
*
*           GStreamer
*
************************************/

void
gst_camera_capturer_init_backend (int *argc, char ***argv)
{
  gst_init (argc, argv);
}

GQuark
gst_camera_capturer_error_quark (void)
{
  static GQuark q;              /* 0 */

  if (G_UNLIKELY (q == 0)) {
    q = g_quark_from_static_string ("gcc-error-quark");
  }
  return q;
}

gboolean
gst_camera_capture_videosrc_buffer_probe (GstPad * pad, GstBuffer * buf,
    gpointer data)
{
  GstCameraCapturer *gcc = GST_CAMERA_CAPTURER (data);

  if (gcc->priv->last_buffer) {
    gst_buffer_unref (gcc->priv->last_buffer);
    gcc->priv->last_buffer = NULL;
  }

  gst_buffer_ref (buf);
  gcc->priv->last_buffer = buf;

  return TRUE;
}

static void
gst_camera_capturer_update_device_id (GstCameraCapturer *gcc)
{
  const gchar *prop_name;

  if (!g_strcmp0 (gcc->priv->source_element_name, "dv1394src"))
    prop_name = "guid";
  else if (!g_strcmp0 (gcc->priv->source_element_name, "v4l2src"))
    prop_name = "device";
  else if (!g_strcmp0 (gcc->priv->source_element_name, "gsettingsvideosrc"))
    prop_name = NULL;
  else
    prop_name = "device-name";

  if (prop_name)
    g_object_set(gcc->priv->source, prop_name, gcc->priv->device_id, NULL);
}

static void
cb_new_pad (GstElement * element, GstPad * pad, GstCameraCapturer *gcc)
{
  GstCaps *caps;
  const gchar *mime;
  GstElement *sink = NULL;
  GstPad *epad;
  GstBin *bin = GST_BIN(gcc->priv->source_decoder_bin);

  caps = gst_pad_get_caps_reffed (pad);
  mime = gst_structure_get_name (gst_caps_get_structure (caps, 0));
  if (g_strrstr (mime, "video")) {
    sink = gst_bin_get_by_name (bin, "video-pad");
  }
  if (g_strrstr (mime, "audio") && gcc->priv->audio_enabled) {
    sink = gst_bin_get_by_name (bin, "audio-pad");
  }

  if (sink != NULL) {
    epad = gst_element_get_static_pad(sink, "sink");
    gst_pad_link (pad, epad);
    gst_object_unref(epad);
    gst_object_unref (sink);
  }
  gst_caps_unref(caps);
}

static void
gst_camera_capturer_create_encoder_bin (GstCameraCapturer *gcc)
{
  GstElement *colorspace, *videoscale;
  GstCaps *caps;
  GstPad *v_sink_pad;
  gchar *caps_str;

  GST_INFO_OBJECT (gcc, "Creating encoder bin");
  gcc->priv->encoder_bin = gst_bin_new ("encoder_bin");

  colorspace = gst_element_factory_make("ffmpegcolorspace", NULL);
  videoscale = gst_element_factory_make("videoscale", NULL);
  gcc->priv->video_filter = gst_element_factory_make("capsfilter", NULL);
  gcc->priv->filesink = gst_element_factory_make("filesink", NULL);

  /* Set caps for the encoding resolution */
  if (gcc->priv->output_width != 0 && gcc->priv->output_height != 0) {
    caps_str = g_strdup_printf("video/x-raw-yuv, width=%d, height=%d",
        gcc->priv->output_width, gcc->priv->output_height);
    caps = gst_caps_from_string(caps_str);
    g_object_set(gcc->priv->video_filter, "caps", caps, NULL);
    gst_caps_unref(caps);
    g_free(caps_str);
  }

  gst_bin_add_many(GST_BIN(gcc->priv->encoder_bin), videoscale,
      colorspace, gcc->priv->video_filter, gcc->priv->video_enc,
      gcc->priv->muxer, gcc->priv->filesink, NULL);

  gst_element_link_many(videoscale, colorspace, gcc->priv->video_filter,
      gcc->priv->video_enc, gcc->priv->muxer, NULL);
  gst_element_link(gcc->priv->muxer, gcc->priv->filesink);

  g_object_set (gcc->priv->filesink, "location", gcc->priv->output_file, NULL);

  /* Create ghost pads */
  v_sink_pad = gst_element_get_static_pad (videoscale, "sink");
  gst_element_add_pad (gcc->priv->encoder_bin, gst_ghost_pad_new ("video", v_sink_pad));
  gst_object_unref (GST_OBJECT (v_sink_pad));

  if (gcc->priv->audio_enabled)
  {
    GstElement *audioconvert, *audioresample;
    GstPad *a_sink_pad;

    audioconvert = gst_element_factory_make("audioconvert", NULL);
    audioresample = gst_element_factory_make("audioresample", NULL);

    gst_bin_add_many(GST_BIN(gcc->priv->encoder_bin), audioconvert, audioresample,
        audioresample, gcc->priv->audio_enc, NULL);

    gst_element_link_many(audioconvert, audioresample, gcc->priv->audio_enc,
        gcc->priv->muxer, NULL);

    a_sink_pad = gst_element_get_static_pad (audioconvert, "sink");
    gst_element_add_pad (gcc->priv->encoder_bin, gst_ghost_pad_new ("audio", a_sink_pad));
    gst_object_unref (GST_OBJECT (a_sink_pad));
  }

  GST_INFO_OBJECT (gcc, "Encoder bin created successfully");
}

static void
gst_camera_capturer_create_remuxer_bin (GstCameraCapturer *gcc)
{
  GstElement *muxer;
  GstPad *v_sink_pad;

  GST_INFO_OBJECT (gcc, "Creating remuxer bin");
  gcc->priv->encoder_bin = gst_bin_new ("encoder_bin");
  muxer = gst_element_factory_make("qtmux", NULL);
  gcc->priv->filesink = gst_element_factory_make("filesink", NULL);
  g_object_set (gcc->priv->filesink, "location", gcc->priv->output_file, NULL);

  gst_bin_add_many(GST_BIN(gcc->priv->encoder_bin), muxer, gcc->priv->filesink, NULL);
  gst_element_link(muxer, gcc->priv->filesink);

  /* Create ghost pads */
  v_sink_pad = gst_element_get_request_pad (muxer, "video_%d");
  gst_element_add_pad (gcc->priv->encoder_bin, gst_ghost_pad_new ("video", v_sink_pad));
  gst_object_unref (v_sink_pad);

  if (gcc->priv->audio_enabled) {
    GstPad *a_sink_pad;

    /* Create ghost pads */
    a_sink_pad = gst_element_get_request_pad (muxer, "audio_%d");
    gst_element_add_pad (gcc->priv->encoder_bin, gst_ghost_pad_new ("audio", a_sink_pad));
    gst_object_unref (GST_OBJECT (v_sink_pad));
  }
}

static GstElement *
gst_camera_capturer_prepare_raw_source (GstCameraCapturer *gcc)
{
  GstElement *bin, *v_identity;
  GstPad *video_pad, *src_pad;

  GST_INFO_OBJECT (gcc, "Creating raw source");

  gcc->priv->video_needs_keyframe_sync = FALSE;

  gcc->priv->source_decoder_bin = gst_bin_new ("decoder");
  bin = gcc->priv->source_decoder_bin;
  v_identity = gst_element_factory_make ("identity", NULL);

  gst_bin_add_many (GST_BIN (bin), v_identity, NULL);

  /* add ghostpad */
  video_pad = gst_element_get_static_pad (v_identity, "src");
  gst_element_add_pad (bin, gst_ghost_pad_new ("video", video_pad));
  gst_object_unref (GST_OBJECT (video_pad));
  src_pad = gst_element_get_static_pad (v_identity, "sink");
  gst_element_add_pad (bin, gst_ghost_pad_new ("sink", src_pad));
  gst_object_unref (GST_OBJECT (src_pad));

  gst_camera_capturer_create_encoder_bin(gcc);

  return bin;
}

static GstElement *
gst_camera_capturer_prepare_dv_source (GstCameraCapturer *gcc)
{
  GstElement *bin, *decodebin, *deinterlacer;
  GstPad *video_pad, *src_pad;

  GST_INFO_OBJECT (gcc, "Creating dv source");

  gcc->priv->video_needs_keyframe_sync = FALSE;

  gcc->priv->source_decoder_bin = gst_bin_new ("decoder");
  bin = gcc->priv->source_decoder_bin;
  decodebin = gst_element_factory_make ("decodebin2", NULL);
  deinterlacer = gst_element_factory_make ("ffdeinterlace", "video-pad");

  gst_bin_add_many (GST_BIN (bin), decodebin, deinterlacer, NULL);

  /* add ghostpad */
  video_pad = gst_element_get_static_pad (deinterlacer, "src");
  gst_element_add_pad (bin, gst_ghost_pad_new ("video", video_pad));
  gst_object_unref (GST_OBJECT (video_pad));
  src_pad = gst_element_get_static_pad (decodebin, "sink");
  gst_element_add_pad (bin, gst_ghost_pad_new ("sink", src_pad));
  gst_object_unref (GST_OBJECT (src_pad));

  if (gcc->priv->audio_enabled) {
    GstElement *audio;
    GstPad *audio_pad;

    audio = gst_element_factory_make ("identity", "audio-pad");

    gst_bin_add_many (GST_BIN (bin), audio, NULL);

    /* add ghostpad */
    audio_pad = gst_element_get_static_pad (audio, "src");
    gst_element_add_pad (bin, gst_ghost_pad_new ("audio", audio_pad));
    gst_object_unref (GST_OBJECT (audio_pad));
  }

  g_signal_connect (decodebin, "pad-added", G_CALLBACK (cb_new_pad), gcc);

  gst_camera_capturer_create_encoder_bin(gcc);

  return bin;
}

static GstElement *
gst_camera_capturer_prepare_mpegts_source (GstCameraCapturer *gcc)
{
  GstElement *bin, *demuxer,  *video, *video_parser;
  GstPad *video_pad, *src_pad;

  GST_INFO_OBJECT (gcc, "Creating mpegts source");

  gcc->priv->video_needs_keyframe_sync = TRUE;
  gcc->priv->video_synced = FALSE;

  /* We don't want to reencode, only remux */
  gcc->priv->source_decoder_bin = gst_bin_new ("decoder");
  bin = gcc->priv->source_decoder_bin;
  demuxer = gst_element_factory_make ("mpegtsdemux", NULL);
  video_parser = gst_element_factory_make ("h264parse", "video-pad");
  video = gst_element_factory_make ("capsfilter", NULL);
  g_object_set(video, "caps", gst_caps_from_string("video/x-h264, stream-format=avc, alignment=au"), NULL);

  gst_bin_add_many (GST_BIN (bin), demuxer, video_parser, video, NULL);
  gst_element_link(video_parser, video);

  /* add ghostpad */
  video_pad = gst_element_get_static_pad (video, "src");
  gst_element_add_pad (bin, gst_ghost_pad_new ("video", video_pad));
  gst_object_unref (GST_OBJECT (video_pad));
  src_pad = gst_element_get_static_pad (demuxer, "sink");
  gst_element_add_pad (bin, gst_ghost_pad_new ("sink", src_pad));
  gst_object_unref (GST_OBJECT (src_pad));

  if (gcc->priv->audio_enabled) {
    GstElement *audio;
    GstPad *audio_pad;

    audio = gst_element_factory_make ("identity", "audio-pad");

    gst_bin_add_many (GST_BIN (bin), audio, NULL);

    /* add ghostpad */
    audio_pad = gst_element_get_static_pad (audio, "src");
    gst_element_add_pad (bin, gst_ghost_pad_new ("audio", audio_pad));
    gst_object_unref (GST_OBJECT (audio_pad));
  }

  g_signal_connect (demuxer, "pad-added", G_CALLBACK (cb_new_pad), gcc);

  gst_camera_capturer_create_remuxer_bin(gcc);

  return bin;
}

static gboolean
gst_camera_capturer_encoding_retimestamper (GstCameraCapturer *gcc,
    GstBuffer *prev_buf, gboolean is_video)
{
  GstClockTime buf_ts, new_buf_ts, duration;
  GstBuffer *enc_buf;

  g_mutex_lock(gcc->priv->recording_lock);

  if (!gcc->priv->is_recording) {
    /* Drop buffers if we are not recording */
    GST_LOG_OBJECT (gcc, "Dropping buffer on %s pad", is_video ? "video": "audio");
    goto done;
  }

  /* If we are just remuxing, drop everything until we see a keyframe */
  if (gcc->priv->video_needs_keyframe_sync && !gcc->priv->video_synced) {
    if (is_video && !GST_BUFFER_FLAG_IS_SET(prev_buf, GST_BUFFER_FLAG_DELTA_UNIT)) {
      gcc->priv->video_synced = TRUE;
    } else {
      GST_LOG_OBJECT (gcc, "Waiting for a keyframe, "
          "dropping buffer on %s pad", is_video ? "video": "audio");
      goto done;
    }
  }

  enc_buf = gst_buffer_create_sub (prev_buf, 0, GST_BUFFER_SIZE(prev_buf));
  buf_ts = GST_BUFFER_TIMESTAMP (prev_buf);
  duration = GST_BUFFER_DURATION (prev_buf);
  if (duration == GST_CLOCK_TIME_NONE)
    duration = 0;

  /* Check if it's the first buffer after starting or restarting the capture
   * and update the timestamps accordingly */
  if (G_UNLIKELY(gcc->priv->current_recording_start_ts == GST_CLOCK_TIME_NONE)) {
    gcc->priv->current_recording_start_ts = buf_ts;
    gcc->priv->last_accum_recorded_ts = gcc->priv->accum_recorded_ts;
    GST_INFO_OBJECT (gcc, "Starting recording at %" GST_TIME_FORMAT,
        GST_TIME_ARGS(gcc->priv->last_accum_recorded_ts));
  }

  /* Clip buffers that are not in the segment */
  if (buf_ts < gcc->priv->current_recording_start_ts) {
    GST_WARNING_OBJECT (gcc, "Discarding buffer out of segment");
    goto done;
  }

  if (buf_ts != GST_CLOCK_TIME_NONE) {
    /* Get the buffer timestamp with respect of the encoding time and not
     * the playing time for a continous stream in the encoders input */
    new_buf_ts = buf_ts - gcc->priv->current_recording_start_ts + gcc->priv->last_accum_recorded_ts;

    /* Store the last timestamp seen on this pad */
    if (is_video)
      gcc->priv->last_video_buf_ts = new_buf_ts;
    else
      gcc->priv->last_audio_buf_ts = new_buf_ts;

    /* Update the highest encoded timestamp */
    if (new_buf_ts + duration > gcc->priv->accum_recorded_ts)
      gcc->priv->accum_recorded_ts = new_buf_ts + duration;
  } else {
    /* h264parse only sets the timestamp on the first buffer if a frame is
     * split in several ones. Other parsers might do the same. We only set
     * the last timestamp seen on the pad */
    if (is_video)
      new_buf_ts = gcc->priv->last_video_buf_ts;
    else
      new_buf_ts = gcc->priv->last_audio_buf_ts;
  }

  GST_BUFFER_TIMESTAMP (enc_buf) = new_buf_ts;

  GST_LOG_OBJECT(gcc, "Pushing %s frame to the encoder in ts:% " GST_TIME_FORMAT
      " out ts: %" GST_TIME_FORMAT, is_video ? "video": "audio",
      GST_TIME_ARGS(buf_ts), GST_TIME_ARGS(new_buf_ts));

  if (is_video)
    gst_app_src_push_buffer(GST_APP_SRC(gcc->priv->video_appsrc), enc_buf);
  else
    gst_app_src_push_buffer(GST_APP_SRC(gcc->priv->audio_appsrc), enc_buf);

done:
  {
    g_mutex_unlock(gcc->priv->recording_lock);
    return TRUE;
  }
}

static gboolean
gst_camera_capturer_audio_encoding_probe (GstPad *pad, GstBuffer *buf,
    GstCameraCapturer *gcc)
{
  return gst_camera_capturer_encoding_retimestamper(gcc, buf, FALSE);
}

static gboolean
gst_camera_capturer_video_encoding_probe (GstPad *pad, GstBuffer *buf,
    GstCameraCapturer *gcc)
{
  return gst_camera_capturer_encoding_retimestamper(gcc, buf, TRUE);
}

static void
gst_camera_capturer_create_decoder_bin (GstCameraCapturer *gcc, GstElement *decoder_bin)
{
  /*    decoder --> video_preview_queue
   *            |
   *            --> audio_preview_queue
   *
   *            video_appsrc   --> video_queue
   *            audio_appsrc   --> audio_queue
   */

  GstElement *v_queue, *v_prev_queue;
  GstPad *v_dec_pad, *v_queue_pad, *v_prev_queue_pad;
  GstPad *dec_sink_pad;

  GST_INFO_OBJECT(gcc, "Creating decoder bin");
  /* Create elements */
  gcc->priv->decoder_bin = gst_bin_new("decoder_bin");
  v_queue = gst_element_factory_make("queue2", "video-queue");
  gcc->priv->video_appsrc = gst_element_factory_make("appsrc", "video-appsrc");
  v_prev_queue = gst_element_factory_make("queue2", "video-preview-queue");

  g_object_set(v_queue, "max-size-time", 1 * GST_SECOND, NULL);
  g_object_set(v_prev_queue, "max-size-bytes", 0,  NULL);

  gst_bin_add_many(GST_BIN(gcc->priv->decoder_bin), decoder_bin, v_queue,
      gcc->priv->video_appsrc, v_prev_queue, NULL);

  /* link decoder to the preview-queue */
  v_dec_pad = gst_element_get_static_pad(decoder_bin, "video");
  v_prev_queue_pad = gst_element_get_static_pad(v_prev_queue, "sink");
  gst_pad_link(v_dec_pad, v_prev_queue_pad);
  gst_object_unref(v_dec_pad);
  gst_object_unref(v_prev_queue_pad);

  /* Link appsrc */
  gst_element_link (gcc->priv->video_appsrc, v_queue);

  /* Create ghost pads */
  v_queue_pad = gst_element_get_static_pad(v_queue, "src");
  v_prev_queue_pad = gst_element_get_static_pad(v_prev_queue, "src");
  dec_sink_pad = gst_element_get_static_pad(decoder_bin, "sink");
  gst_element_add_pad (gcc->priv->decoder_bin, gst_ghost_pad_new ("video", v_queue_pad));
  gst_element_add_pad (gcc->priv->decoder_bin, gst_ghost_pad_new ("video_preview", v_prev_queue_pad));
  gst_element_add_pad (gcc->priv->decoder_bin, gst_ghost_pad_new ("sink", dec_sink_pad));
  gst_object_unref(v_queue_pad);
  gst_object_unref(v_prev_queue_pad);
  gst_object_unref(dec_sink_pad);

  /* Add pad probes for the encoding branch */
  v_prev_queue_pad = gst_element_get_static_pad(v_prev_queue, "src");
  gst_pad_add_buffer_probe(v_prev_queue_pad, (GCallback) gst_camera_capturer_video_encoding_probe, gcc);
  gst_object_unref(v_prev_queue_pad);

  if (gcc->priv->audio_enabled) {
    GstElement *a_queue, *a_prev_queue;
    GstPad *a_dec_pad, *a_queue_pad, *a_prev_queue_pad;

    /* Create elements */
    gcc->priv->audio_appsrc = gst_element_factory_make("appsrc", "video-appsrc");
    a_queue = gst_element_factory_make("queue2", "audio-queue");
    a_prev_queue = gst_element_factory_make("queue2", "audio-preview-queue");

    g_object_set(a_queue, "max-size-time", 1 * GST_SECOND,  NULL);

    gst_bin_add_many(GST_BIN(gcc->priv->decoder_bin), gcc->priv->audio_appsrc, a_queue,
        a_prev_queue, NULL);

    /* Link appsrc to the queue */
    gst_element_link(gcc->priv->audio_appsrc, a_queue);

    /* link decoder to the queue */
    a_dec_pad = gst_element_get_static_pad(decoder_bin, "audio");
    a_prev_queue_pad = gst_element_get_static_pad(a_prev_queue, "sink");
    gst_pad_link(a_dec_pad, a_prev_queue_pad);
    gst_object_unref(a_dec_pad);
    gst_object_unref(a_prev_queue_pad);

    /* Create ghost pads */
    a_queue_pad = gst_element_get_static_pad(a_queue, "src");
    a_prev_queue_pad = gst_element_get_static_pad(a_prev_queue, "src");
    gst_element_add_pad (gcc->priv->decoder_bin, gst_ghost_pad_new ("audio", a_queue_pad));
    gst_element_add_pad (gcc->priv->decoder_bin, gst_ghost_pad_new ("audio_preview", a_prev_queue_pad));
    gst_object_unref(a_queue_pad);
    gst_object_unref(a_prev_queue_pad);

    /* Add pad probes for the encoding branch */
    a_prev_queue_pad = gst_element_get_static_pad(a_prev_queue, "src");
    gst_pad_add_buffer_probe(a_prev_queue_pad, (GCallback) gst_camera_capturer_audio_encoding_probe, gcc);
    gst_object_unref(a_prev_queue_pad);
  }
}

static void
gst_camera_capturer_link_encoder_bin (GstCameraCapturer *gcc)
{
  GstPad *v_dec_pad, *v_enc_pad;

  GST_INFO_OBJECT(gcc, "Linking encoder bin");

  gst_bin_add(GST_BIN(gcc->priv->main_pipeline), gcc->priv->encoder_bin);

  v_dec_pad = gst_element_get_static_pad(gcc->priv->decoder_bin, "video");
  v_enc_pad = gst_element_get_static_pad(gcc->priv->encoder_bin, "video");
  gst_pad_link(v_dec_pad, v_enc_pad);
  gst_object_unref(v_dec_pad);
  gst_object_unref(v_enc_pad);

  if (gcc->priv->audio_enabled) {
    GstPad *a_dec_pad, *a_enc_pad;

    a_dec_pad = gst_element_get_static_pad(gcc->priv->decoder_bin, "audio");
    a_enc_pad = gst_element_get_static_pad(gcc->priv->encoder_bin, "audio");
    gst_pad_link(a_dec_pad, a_enc_pad);
    gst_object_unref(a_dec_pad);
    gst_object_unref(a_enc_pad);
  }

  gst_element_set_state(gcc->priv->encoder_bin, GST_STATE_PLAYING);
}

static void
gst_camera_capturer_link_preview (GstCameraCapturer *gcc)
{
  GstPad *v_dec_prev_pad, *v_prev_pad;

  GST_INFO_OBJECT(gcc, "Linking preview bin");

  gst_bin_add(GST_BIN(gcc->priv->main_pipeline), gcc->priv->decoder_bin);

  gst_element_link(gcc->priv->source_bin, gcc->priv->decoder_bin);

  v_dec_prev_pad = gst_element_get_static_pad(gcc->priv->decoder_bin, "video_preview");
  v_prev_pad = gst_element_get_static_pad(gcc->priv->preview_bin, "video");

  gst_pad_link(v_dec_prev_pad, v_prev_pad);

  gst_object_unref(v_dec_prev_pad);
  gst_object_unref(v_prev_pad);

  if (gcc->priv->audio_enabled) {
    GstPad *a_dec_prev_pad, *a_prev_pad;

    a_dec_prev_pad = gst_element_get_static_pad(gcc->priv->decoder_bin, "audio_preview");
    a_prev_pad = gst_element_get_static_pad(gcc->priv->preview_bin, "audio");

    gst_pad_link(a_dec_prev_pad, a_prev_pad);

    gst_object_unref(a_dec_prev_pad);
    gst_object_unref(a_prev_pad);
  }
  gst_element_set_state(gcc->priv->decoder_bin, GST_STATE_PLAYING);
}

static gboolean
cb_last_buffer (GstPad *pad, GstBuffer *buf, GstCameraCapturer *gcc){
  if (buf != NULL) {
    if (gcc->priv->last_buffer != NULL)
      gst_buffer_unref(buf);
    gst_buffer_ref(buf);
    gcc->priv->last_buffer = buf;
  }
  return TRUE;
}

static void
cb_new_prev_pad (GstElement * element, GstPad * pad, GstElement *bin)
{
  GstPad *sink_pad;

  sink_pad = gst_element_get_static_pad(bin, "sink");
  gst_pad_link(pad, sink_pad);
  gst_object_unref(sink_pad);
}

static void
gst_camera_capturer_create_preview(GstCameraCapturer *gcc)
{
  GstElement *v_decoder, *video_bin;
  GstPad *video_pad;

  v_decoder = gst_element_factory_make("decodebin2", "preview-decoder");

  video_bin = gst_parse_bin_from_description(
      "videoscale ! ffmpegcolorspace ! autovideosink name=videosink", TRUE, NULL);

  gcc->priv->preview_bin = gst_bin_new("preview_bin");
  gst_bin_add_many (GST_BIN(gcc->priv->preview_bin), v_decoder, video_bin, NULL);

  g_signal_connect (v_decoder, "pad-added", G_CALLBACK (cb_new_prev_pad), video_bin);

  video_pad = gst_element_get_static_pad(video_bin, "sink");
  gst_pad_add_buffer_probe (video_pad, (GCallback) cb_last_buffer, gcc);
  gst_object_unref(video_pad);

  /* Create ghost pads */
  video_pad = gst_element_get_static_pad (v_decoder, "sink");
  gst_element_add_pad (gcc->priv->preview_bin, gst_ghost_pad_new ("video", video_pad));
  gst_object_unref (GST_OBJECT (video_pad));

  if (gcc->priv->audio_enabled) {
    GstElement *a_decoder, *audio_bin;
    GstPad *audio_pad;

    a_decoder = gst_element_factory_make("decodebin2", NULL);

    audio_bin = gst_parse_bin_from_description(
        "audioconvert ! audioresample ! autoaudiosink name=audiosink", TRUE, NULL);

    gst_bin_add_many (GST_BIN(gcc->priv->preview_bin), a_decoder, audio_bin, NULL);

    g_signal_connect (a_decoder, "pad-added", G_CALLBACK (cb_new_prev_pad), audio_bin);

    /* Create ghost pads */
    audio_pad = gst_element_get_static_pad (a_decoder, "sink");
    gst_element_add_pad (gcc->priv->preview_bin, gst_ghost_pad_new ("audio", audio_pad));
    gst_object_unref (GST_OBJECT (audio_pad));
  }

  gst_bin_add(GST_BIN(gcc->priv->main_pipeline), gcc->priv->preview_bin);
  gst_element_set_state(gcc->priv->preview_bin, GST_STATE_PLAYING);
}

static gboolean
gst_camera_capturer_have_type_cb (GstElement *typefind, guint prob,
    GstCaps *caps, GstCameraCapturer *gcc)
{
  GstCaps *media_caps;
  GstElement *decoder_bin = NULL;

  GST_INFO_OBJECT (gcc, "Found type with caps %s", gst_caps_to_string(caps));

  /* Check for DV streams */
  media_caps = gst_caps_from_string("video/x-dv, systemstream=true");

  if (gst_caps_can_intersect(caps, media_caps)) {
    decoder_bin = gst_camera_capturer_prepare_dv_source(gcc);
    gst_caps_unref(media_caps);
  }

  /* Check for MPEG-TS streams */
  media_caps = gst_caps_from_string("video/mpegts");
  if (gst_caps_can_intersect(caps, media_caps)) {
    decoder_bin = gst_camera_capturer_prepare_mpegts_source(gcc);
    gst_caps_unref(media_caps);
  }

  /* Check for Raw streams */
  media_caps = gst_caps_from_string ("video/x-raw-rgb; video/x-raw-yuv");
  if (gst_caps_can_intersect(caps, media_caps)) {
    gcc->priv->audio_enabled = FALSE;
    decoder_bin = gst_camera_capturer_prepare_raw_source(gcc);
    gst_caps_unref(media_caps);
  }

  if (decoder_bin != NULL) {
    gst_camera_capturer_create_decoder_bin(gcc, decoder_bin);
    gst_camera_capturer_create_preview(gcc);

    gst_camera_capturer_link_preview(gcc);
    gst_element_set_state(gcc->priv->main_pipeline, GST_STATE_PLAYING);
  } else {
    /* FIXME: post error */
  }
  return TRUE;
}

static gboolean
gst_camera_capturer_create_video_source (GstCameraCapturer * gcc,
    CaptureSourceType type, GError ** err)
{
  GstElement *typefind;
  const gchar *source_desc = "";
  gchar *source_str;

  g_return_val_if_fail (gcc != NULL, FALSE);
  g_return_val_if_fail (GST_IS_CAMERA_CAPTURER (gcc), FALSE);

  switch (type) {
    case CAPTURE_SOURCE_TYPE_DV:
      GST_INFO_OBJECT(gcc, "Creating dv video source");
      source_desc = DVVIDEOSRC;
      gcc->priv->source_element_name = source_desc;
      break;
    case CAPTURE_SOURCE_TYPE_SYSTEM:
      GST_INFO_OBJECT(gcc, "Creating system video source");
      source_desc = SYSVIDEOSRC;
      gcc->priv->source_element_name = source_desc;
      //source_desc = "filesrc location=/home/andoni/test.ts";
      break;
    default:
      g_assert_not_reached();
  }

  /* HACK: dshowvideosrc's device must be set before linking the element
   * since the device is set in getcaps and can't be changed later */
  if (!g_strcmp0 (gcc->priv->source_element_name, "dshowvideosrc"))
    source_str = g_strdup_printf("%s device-name=\"%s\" name=source ! typefind name=typefind",
        source_desc, gcc->priv->device_id);
  else
    source_str = g_strdup_printf("%s name=source ! typefind name=typefind", source_desc);
  GST_INFO_OBJECT(gcc, "Created video source %s", source_str);
  gcc->priv->source_bin = gst_parse_bin_from_description(source_str, TRUE, NULL);
  g_free(source_str);
  if (!gcc->priv->source_bin) {
    g_set_error (err,
        GCC_ERROR,
        GST_ERROR_PLUGIN_LOAD,
        "Failed to create the %s element. "
        "Please check your GStreamer installation.", source_desc);
    return FALSE;
  }

  gcc->priv->source = gst_bin_get_by_name (GST_BIN(gcc->priv->source_bin), "source");
  typefind = gst_bin_get_by_name (GST_BIN(gcc->priv->source_bin), "typefind");
  g_signal_connect (typefind, "have-type",
      G_CALLBACK (gst_camera_capturer_have_type_cb), gcc);

  gst_camera_capturer_update_device_id(gcc);

  GST_INFO_OBJECT(gcc, "Created video source %s", source_desc);

  gst_object_unref (gcc->priv->source);
  gst_object_unref (typefind);

  return TRUE;
}

static gboolean
gst_camera_capturer_create_video_encoder (GstCameraCapturer * gcc,
    VideoEncoderType type, GError ** err)
{
  gchar *name = NULL;

  g_return_val_if_fail (gcc != NULL, FALSE);
  g_return_val_if_fail (GST_IS_CAMERA_CAPTURER (gcc), FALSE);

  switch (type) {
    case VIDEO_ENCODER_MPEG4:
      gcc->priv->video_enc =
          gst_element_factory_make ("ffenc_mpeg4", "video-encoder");
      g_object_set (gcc->priv->video_enc, "pass", 512,
          "max-key-interval", -1, NULL);
      name = "FFmpeg mpeg4 video encoder";
      break;

    case VIDEO_ENCODER_XVID:
      gcc->priv->video_enc =
          gst_element_factory_make ("xvidenc", "video-encoder");
      g_object_set (gcc->priv->video_enc, "pass", 1,
          "profile", 146, "max-key-interval", -1, NULL);
      name = "Xvid video encoder";
      break;

    case VIDEO_ENCODER_H264:
      gcc->priv->video_enc =
          gst_element_factory_make ("x264enc", "video-encoder");
      g_object_set (gcc->priv->video_enc, "key-int-max", 25, "pass", 17,
          "speed-preset", 3, NULL);
      name = "X264 video encoder";
      break;

    case VIDEO_ENCODER_THEORA:
      gcc->priv->video_enc =
          gst_element_factory_make ("theoraenc", "video-encoder");
      g_object_set (gcc->priv->video_enc, "keyframe-auto", FALSE,
          "keyframe-force", 25, NULL);
      name = "Theora video encoder";
      break;

    case VIDEO_ENCODER_VP8:
    default:
      gcc->priv->video_enc =
          gst_element_factory_make ("vp8enc", "video-encoder");
      g_object_set (gcc->priv->video_enc, "speed", 2, "threads", 8,
          "max-keyframe-distance", 25, NULL);
      name = "VP8 video encoder";
      break;

  }
  if (!gcc->priv->video_enc) {
    g_set_error (err,
        GCC_ERROR,
        GST_ERROR_PLUGIN_LOAD,
        "Failed to create the %s element. "
        "Please check your GStreamer installation.", name);
    return FALSE;
  }

  if (gcc->priv->video_encoder_type == VIDEO_ENCODER_MPEG4 ||
      gcc->priv->video_encoder_type == VIDEO_ENCODER_XVID)
    g_object_set (gcc->priv->video_enc, "bitrate", gcc->priv->video_bitrate * 1000, NULL);
  else
    g_object_set (gcc->priv->video_enc, "bitrate", gcc->priv->video_bitrate,
        NULL);

  GST_INFO_OBJECT(gcc, "Video encoder %s created", name);
  gcc->priv->video_encoder_type = type;
  return TRUE;
}

static gboolean
gst_camera_capturer_create_audio_encoder (GstCameraCapturer * gcc,
    AudioEncoderType type, GError ** err)
{
  gchar *name = NULL;

  g_return_val_if_fail (gcc != NULL, FALSE);
  g_return_val_if_fail (GST_IS_CAMERA_CAPTURER (gcc), FALSE);

  switch (type) {
    case AUDIO_ENCODER_MP3:
      gcc->priv->audio_enc =
          gst_element_factory_make ("lamemp3enc", "audio-encoder");
      g_object_set (gcc->priv->audio_enc, "target", 0, NULL);
      name = "Mp3 audio encoder";
      break;

    case AUDIO_ENCODER_AAC:
      gcc->priv->audio_enc = gst_element_factory_make ("faac", "audio-encoder");
      name = "AAC audio encoder";
      break;

    case AUDIO_ENCODER_VORBIS:
    default:
      gcc->priv->audio_enc =
          gst_element_factory_make ("vorbisenc", "audio-encoder");
      name = "Vorbis audio encoder";
      break;
  }

  if (!gcc->priv->audio_enc) {
    g_set_error (err,
        GCC_ERROR,
        GST_ERROR_PLUGIN_LOAD,
        "Failed to create the %s element. "
        "Please check your GStreamer installation.", name);
    return FALSE;
  }

  if (gcc->priv->audio_encoder_type == AUDIO_ENCODER_MP3)
    g_object_set (gcc->priv->audio_enc, "bitrate", gcc->priv->audio_bitrate, NULL);
  else
    g_object_set (gcc->priv->audio_enc, "bitrate", 1000 * gcc->priv->audio_bitrate, NULL);

  GST_INFO_OBJECT(gcc, "Audio encoder %s created", name);

  gcc->priv->audio_encoder_type = type;
  return TRUE;
}

static gboolean
gst_camera_capturer_create_video_muxer (GstCameraCapturer * gcc,
    VideoMuxerType type, GError ** err)
{
  gchar *name = NULL;

  g_return_val_if_fail (gcc != NULL, FALSE);
  g_return_val_if_fail (GST_IS_CAMERA_CAPTURER (gcc), FALSE);

  switch (type) {
    case VIDEO_MUXER_OGG:
      name = "OGG muxer";
      gcc->priv->muxer = gst_element_factory_make ("oggmux", "video-muxer");
      break;
    case VIDEO_MUXER_AVI:
      name = "AVI muxer";
      gcc->priv->muxer = gst_element_factory_make ("avimux", "video-muxer");
      break;
    case VIDEO_MUXER_MATROSKA:
      name = "Matroska muxer";
      gcc->priv->muxer =
          gst_element_factory_make ("matroskamux", "video-muxer");
      break;
    case VIDEO_MUXER_MP4:
      name = "MP4 muxer";
      gcc->priv->muxer = gst_element_factory_make ("qtmux", "video-muxer");
      break;
    case VIDEO_MUXER_WEBM:
    default:
      name = "WebM muxer";
      gcc->priv->muxer = gst_element_factory_make ("webmmux", "video-muxer");
      break;
  }

  if (!gcc->priv->muxer) {
    g_set_error (err,
        GCC_ERROR,
        GST_ERROR_PLUGIN_LOAD,
        "Failed to create the %s element. "
        "Please check your GStreamer installation.", name);
  }

  GST_INFO_OBJECT(gcc, "Muxer %s created", name);
  gcc->priv->video_muxer_type = type;
  return TRUE;
}

static void
gst_camera_capturer_initialize (GstCameraCapturer *gcc)
{
  GError *err= NULL;

  GST_INFO_OBJECT (gcc, "Initializing encoders");
  if (!gst_camera_capturer_create_video_encoder(gcc,
        gcc->priv->video_encoder_type, &err))
    goto missing_plugin;
  if (!gst_camera_capturer_create_audio_encoder(gcc,
        gcc->priv->audio_encoder_type, &err))
    goto missing_plugin;
  if (!gst_camera_capturer_create_video_muxer(gcc,
        gcc->priv->video_muxer_type, &err))
    goto missing_plugin;

  GST_INFO_OBJECT (gcc, "Initializing source");
  if (!gst_camera_capturer_create_video_source(gcc,
        gcc->priv->source_type, &err))
    goto missing_plugin;

  /* add the source element */
  gst_bin_add(GST_BIN(gcc->priv->main_pipeline), gcc->priv->source_bin);
  return;

missing_plugin:
    g_signal_emit (gcc, gcc_signals[SIGNAL_ERROR], 0, err->message);
    g_error_free (err);
}

static void
gcc_encoder_send_event (GstCameraCapturer *gcc, GstEvent *event)
{
  GstPad *video_pad, *audio_pad;

  if (gcc->priv->audio_enabled) {
    gst_event_ref(event);
    audio_pad = gst_element_get_static_pad(gcc->priv->encoder_bin, "audio");
    gst_pad_send_event(audio_pad, event);
    gst_object_unref(audio_pad);
  }

  video_pad = gst_element_get_static_pad(gcc->priv->encoder_bin, "video");
  gst_pad_send_event(video_pad, event);
  gst_object_unref(video_pad);

}

static void
gcc_bus_message_cb (GstBus * bus, GstMessage * message, gpointer data)
{
  GstCameraCapturer *gcc = (GstCameraCapturer *) data;
  GstMessageType msg_type;

  g_return_if_fail (gcc != NULL);
  g_return_if_fail (GST_IS_CAMERA_CAPTURER (gcc));

  msg_type = GST_MESSAGE_TYPE (message);

  switch (msg_type) {
    case GST_MESSAGE_ERROR:
    {
      if (gcc->priv->main_pipeline) {
        gst_camera_capturer_stop (gcc);
        gst_camera_capturer_close (gcc);
        gst_element_set_state (gcc->priv->main_pipeline, GST_STATE_NULL);
      }
      gcc_error_msg (gcc, message);
      break;
    }

    case GST_MESSAGE_WARNING:
    {
      GST_WARNING ("Warning message: %" GST_PTR_FORMAT, message);
      break;
    }

    case GST_MESSAGE_EOS:
    {
      GST_INFO_OBJECT (gcc, "EOS message");
      g_signal_emit (gcc, gcc_signals[SIGNAL_EOS], 0);
      break;
    }

    case GST_MESSAGE_STATE_CHANGED:
    {
      GstState old_state, new_state;

      gst_message_parse_state_changed (message, &old_state, &new_state, NULL);

      if (old_state == new_state)
        break;

      /* we only care about playbin (pipeline) state changes */
      if (GST_MESSAGE_SRC (message) != GST_OBJECT (gcc->priv->main_pipeline))
        break;

      if (old_state == GST_STATE_PAUSED && new_state == GST_STATE_PLAYING) {
        gcc_get_video_stream_info (gcc);
      }
    }

    case GST_MESSAGE_ELEMENT:
    {
      const GstStructure *s;
      gint device_change = 0;

      /* We only care about messages sent by the device source */
      if (GST_MESSAGE_SRC (message) != GST_OBJECT (gcc->priv->source))
        break;

      s = gst_message_get_structure (message);
      /* check if it's bus reset message and it contains the
       * 'current-device-change' field */
      if (g_strcmp0 (gst_structure_get_name (s), "ieee1394-bus-reset"))
        break;
      if (!gst_structure_has_field (s, "current-device-change"))
        break;


      /* emit a signal if the device was connected or disconnected */
      gst_structure_get_int (s, "current-device-change", &device_change);

      if (device_change != 0)
        g_signal_emit (gcc, gcc_signals[SIGNAL_DEVICE_CHANGE], 0,
            device_change);
      break;
    }

    default:
      GST_LOG ("Unhandled message: %" GST_PTR_FORMAT, message);
      break;
  }
}

static void
gcc_error_msg (GstCameraCapturer * gcc, GstMessage * msg)
{
  GError *err = NULL;
  gchar *dbg = NULL;

  gst_message_parse_error (msg, &err, &dbg);
  if (err) {
    GST_ERROR ("message = %s", GST_STR_NULL (err->message));
    GST_ERROR ("domain  = %d (%s)", err->domain,
        GST_STR_NULL (g_quark_to_string (err->domain)));
    GST_ERROR ("code    = %d", err->code);
    GST_ERROR ("debug   = %s", GST_STR_NULL (dbg));
    GST_ERROR ("source  = %" GST_PTR_FORMAT, msg->src);


    g_message ("Error: %s\n%s\n", GST_STR_NULL (err->message),
        GST_STR_NULL (dbg));
    g_signal_emit (gcc, gcc_signals[SIGNAL_ERROR], 0, err->message);
    g_error_free (err);
  }
  g_free (dbg);
}

static void
gcc_element_msg_sync (GstBus * bus, GstMessage * msg, gpointer data)
{
  GstCameraCapturer *gcc = GST_CAMERA_CAPTURER (data);

  g_assert (msg->type == GST_MESSAGE_ELEMENT);

  if (msg->structure == NULL)
    return;

  /* This only gets sent if we haven't set an ID yet. This is our last
   * chance to set it before the video sink will create its own window */
  if (gst_structure_has_name (msg->structure, "prepare-xwindow-id")) {

    if (gcc->priv->xoverlay == NULL) {
      GstObject *sender = GST_MESSAGE_SRC (msg);
      if (sender && GST_IS_X_OVERLAY (sender))
        gcc->priv->xoverlay = GST_X_OVERLAY (gst_object_ref (sender));
    }

    g_return_if_fail (gcc->priv->xoverlay != NULL);
    g_return_if_fail (gcc->priv->window_handle != 0);

    g_object_set (GST_ELEMENT (gcc->priv->xoverlay), "force-aspect-ratio", TRUE, NULL);
    gst_set_window_handle (gcc->priv->xoverlay, gcc->priv->window_handle);
    gtk_widget_queue_draw (GTK_WIDGET(gcc));
  }
}

static int
gcc_get_video_stream_info (GstCameraCapturer * gcc)
{
  GstPad *sourcepad;
  GstCaps *caps;
  GstStructure *s;

  sourcepad = gst_element_get_pad (gcc->priv->source_bin, "src");
  caps = gst_pad_get_negotiated_caps (sourcepad);

  if (!(caps)) {
    GST_WARNING_OBJECT (gcc, "Could not get stream info");
    return -1;
  }

  /* Get the source caps */
  s = gst_caps_get_structure (caps, 0);
  if (s) {
    /* We need at least width/height and framerate */
    if (!
        (gst_structure_get_fraction
            (s, "framerate", &gcc->priv->video_fps_n, &gcc->priv->video_fps_d)
            && gst_structure_get_int (s, "width", &gcc->priv->video_width)
            && gst_structure_get_int (s, "height", &gcc->priv->video_height)))
      return -1;
    /* Get the source PAR if available */
    gcc->priv->movie_par = gst_structure_get_value (s, "pixel-aspect-ratio");
  }
  return 1;
}

/*****************************************************
 *
 *             Device Probe
 *
 * **************************************************/

GList *
gst_camera_capturer_enum_devices (gchar * device_name)
{
  GstElement *device;
  GstPropertyProbe *probe;
  GValueArray *va;
  gchar *prop_name;
  GList *list = NULL;
  guint i = 0;

  device = gst_element_factory_make (device_name, "source");
  if (!device || !GST_IS_PROPERTY_PROBE (device))
    goto finish;
  gst_element_set_state (device, GST_STATE_READY);
  gst_element_get_state (device, NULL, NULL, 5 * GST_SECOND);
  probe = GST_PROPERTY_PROBE (device);

  if (!g_strcmp0 (device_name, "dv1394src"))
    prop_name = "guid";
  else if (!g_strcmp0 (device_name, "v4l2src") ||
      !g_strcmp0 (device_name, "osxvideosrc"))
    prop_name = "device";
  else
    prop_name = "device-name";

  va = gst_property_probe_get_values_name (probe, prop_name);
  if (!va)
    goto finish;

  for (i = 0; i < va->n_values; ++i) {
    GValue *v = g_value_array_get_nth (va, i);
    GValue valstr = { 0, };

    g_value_init (&valstr, G_TYPE_STRING);
    if (!g_value_transform (v, &valstr))
      continue;
    list = g_list_append (list, g_value_dup_string (&valstr));
    g_value_unset (&valstr);
  }
  g_value_array_free (va);

finish:
  {
    gst_element_set_state (device, GST_STATE_NULL);
    gst_object_unref (GST_OBJECT (device));
    return list;
  }
}

GList *
gst_camera_capturer_enum_video_devices (void)
{
  return gst_camera_capturer_enum_devices (DVVIDEOSRC);
}

GList *
gst_camera_capturer_enum_audio_devices (void)
{
  return gst_camera_capturer_enum_devices (AUDIOSRC);
}

/*******************************************
 *
 *         Public methods
 *
 * ****************************************/

void
gst_camera_capturer_run (GstCameraCapturer * gcc)
{
  g_return_if_fail (gcc != NULL);
  g_return_if_fail (GST_IS_CAMERA_CAPTURER (gcc));

  gst_camera_capturer_initialize (gcc);
  gst_element_set_state (gcc->priv->main_pipeline, GST_STATE_PLAYING);
}

void
gst_camera_capturer_close (GstCameraCapturer * gcc)
{
  g_return_if_fail (gcc != NULL);
  g_return_if_fail (GST_IS_CAMERA_CAPTURER (gcc));

  gst_element_set_state (gcc->priv->main_pipeline, GST_STATE_NULL);
  gst_element_get_state (gcc->priv->main_pipeline, NULL, NULL, -1);
}

void
gst_camera_capturer_start (GstCameraCapturer * gcc)
{
  g_return_if_fail (gcc != NULL);
  g_return_if_fail (GST_IS_CAMERA_CAPTURER (gcc));

  GST_INFO_OBJECT(gcc, "Started capture");
  g_mutex_lock(gcc->priv->recording_lock);
  if (!gcc->priv->is_recording && gcc->priv->accum_recorded_ts == GST_CLOCK_TIME_NONE) {
    gcc->priv->accum_recorded_ts = 0;
    gcc->priv->is_recording = TRUE;
    gst_camera_capturer_link_encoder_bin (gcc);
  }
  g_mutex_unlock(gcc->priv->recording_lock);
}

void
gst_camera_capturer_toggle_pause (GstCameraCapturer * gcc)
{
  g_return_if_fail (gcc != NULL);
  g_return_if_fail (GST_IS_CAMERA_CAPTURER (gcc));

  g_mutex_lock(gcc->priv->recording_lock);
  if (!gcc->priv->is_recording) {
    gcc->priv->current_recording_start_ts = GST_CLOCK_TIME_NONE;
    gcc->priv->is_recording = TRUE;
  } else {
    gcc->priv->is_recording = FALSE;
    gcc->priv->video_synced = FALSE;
  }
  g_mutex_unlock(gcc->priv->recording_lock);

  GST_INFO_OBJECT(gcc, "Capture state changed to %s", gcc->priv->is_recording ? "recording": "paused");
}

void
gst_camera_capturer_set_source (GstCameraCapturer * gcc, CaptureSourceType source)
{
  g_return_if_fail (gcc != NULL);
  g_return_if_fail (GST_IS_CAMERA_CAPTURER (gcc));

  gcc->priv->source_type = source;
}

void
gst_camera_capturer_set_video_encoder (GstCameraCapturer * gcc, VideoEncoderType encoder)
{
  g_return_if_fail (gcc != NULL);
  g_return_if_fail (GST_IS_CAMERA_CAPTURER (gcc));

  gcc->priv->video_encoder_type = encoder;
}

void
gst_camera_capturer_set_audio_encoder (GstCameraCapturer * gcc, AudioEncoderType encoder)
{
  g_return_if_fail (gcc != NULL);
  g_return_if_fail (GST_IS_CAMERA_CAPTURER (gcc));

  gcc->priv->audio_encoder_type = encoder;
}

void
gst_camera_capturer_set_video_muxer (GstCameraCapturer * gcc, VideoMuxerType muxer)
{
  g_return_if_fail (gcc != NULL);
  g_return_if_fail (GST_IS_CAMERA_CAPTURER (gcc));

  gcc->priv->video_muxer_type = muxer;
}

gboolean
gst_camera_capturer_can_get_frames (GstCameraCapturer * gcc, GError ** error)
{
  g_return_val_if_fail (gcc != NULL, FALSE);
  g_return_val_if_fail (GST_IS_CAMERA_CAPTURER (gcc), FALSE);

  /* check for video */
  if (!gcc->priv->media_has_video) {
    g_set_error_literal (error, GCC_ERROR, GST_ERROR_GENERIC,
        "Media contains no supported video streams.");
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
gst_camera_capturer_unref_pixbuf (GdkPixbuf * pixbuf)
{
  g_object_unref (pixbuf);
}

GdkPixbuf *
gst_camera_capturer_get_current_frame (GstCameraCapturer * gcc)
{
  GstStructure *s;
  GdkPixbuf *pixbuf;
  GstBuffer *last_buffer;
  GstBuffer *buf;
  GstCaps *to_caps;
  gint outwidth = 0;
  gint outheight = 0;

  g_return_val_if_fail (gcc != NULL, NULL);
  g_return_val_if_fail (GST_IS_CAMERA_CAPTURER (gcc), NULL);

  gst_element_get_state (gcc->priv->main_pipeline, NULL, NULL, -1);

  /* no video info */
  if (!gcc->priv->video_width || !gcc->priv->video_height) {
    GST_DEBUG_OBJECT (gcc, "Could not take screenshot: %s", "no video info");
    g_warning ("Could not take screenshot: %s", "no video info");
    return NULL;
  }

  /* get frame */
  last_buffer = gcc->priv->last_buffer;
  gst_buffer_ref (last_buffer);

  if (!last_buffer) {
    GST_DEBUG_OBJECT (gcc, "Could not take screenshot: %s",
        "no last video frame");
    g_warning ("Could not take screenshot: %s", "no last video frame");
    return NULL;
  }

  if (GST_BUFFER_CAPS (last_buffer) == NULL) {
    GST_DEBUG_OBJECT (gcc, "Could not take screenshot: %s",
        "no caps on buffer");
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

  if (gcc->priv->video_fps_n > 0 && gcc->priv->video_fps_d > 0) {
    gst_caps_set_simple (to_caps, "framerate", GST_TYPE_FRACTION,
        gcc->priv->video_fps_n, gcc->priv->video_fps_d, NULL);
  }

  GST_DEBUG_OBJECT (gcc, "frame caps: %" GST_PTR_FORMAT,
      GST_BUFFER_CAPS (gcc->priv->last_buffer));
  GST_DEBUG_OBJECT (gcc, "pixbuf caps: %" GST_PTR_FORMAT, to_caps);

  /* bvw_frame_conv_convert () takes ownership of the buffer passed */
  buf = bvw_frame_conv_convert (last_buffer, to_caps);

  gst_caps_unref (to_caps);
  gst_buffer_unref (last_buffer);

  if (!buf) {
    GST_DEBUG_OBJECT (gcc, "Could not take screenshot: %s",
        "conversion failed");
    g_warning ("Could not take screenshot: %s", "conversion failed");
    return NULL;
  }

  if (!GST_BUFFER_CAPS (buf)) {
    GST_DEBUG_OBJECT (gcc, "Could not take screenshot: %s",
        "no caps on output buffer");
    g_warning ("Could not take screenshot: %s", "no caps on output buffer");
    return NULL;
  }

  s = gst_caps_get_structure (GST_BUFFER_CAPS (buf), 0);
  gst_structure_get_int (s, "width", &outwidth);
  gst_structure_get_int (s, "height", &outheight);
  g_return_val_if_fail (outwidth > 0 && outheight > 0, NULL);

  /* create pixbuf from that - we don't want to use the gstreamer's buffer
   * because the GTK# bindings won't call the destroy funtion */
  pixbuf = gdk_pixbuf_new_from_data (GST_BUFFER_DATA (buf),
      GDK_COLORSPACE_RGB, FALSE, 8, outwidth,
      outheight, GST_ROUND_UP_4 (outwidth * 3), destroy_pixbuf, buf);

  if (!pixbuf) {
    GST_DEBUG_OBJECT (gcc, "Could not take screenshot: %s",
        "could not create pixbuf");
    g_warning ("Could not take screenshot: %s", "could not create pixbuf");
  }

  return pixbuf;
}


void
gst_camera_capturer_stop (GstCameraCapturer * gcc)
{
  g_return_if_fail (gcc != NULL);
  g_return_if_fail (GST_IS_CAMERA_CAPTURER (gcc));

#ifdef WIN32
  //On windows we can't handle device disconnections until dshowvideosrc
  //supports it. When a device is disconnected, the source is locked
  //in ::create(), blocking the streaming thread. We need to change its
  //state to null, this way camerabin doesn't block in ::do_stop().
  gst_element_set_state(gcc->priv->source, GST_STATE_NULL);
#endif

  GST_INFO_OBJECT(gcc, "Closing capture");
  g_mutex_lock(gcc->priv->recording_lock);
  gcc->priv->closing_recording = TRUE;
  gcc->priv->is_recording = FALSE;
  g_mutex_unlock(gcc->priv->recording_lock);

  gcc_encoder_send_event(gcc, gst_event_new_eos());
}

GstCameraCapturer *
gst_camera_capturer_new (gchar * filename, GError ** err)
{
  GstCameraCapturer *gcc = NULL;

#ifndef GST_DISABLE_GST_INFO
  if (_cesarplayer_gst_debug_cat == NULL) {
    GST_DEBUG_CATEGORY_INIT (_cesarplayer_gst_debug_cat, "longomatch", 0,
        "LongoMatch GStreamer Backend");
  }
#endif

  gcc = g_object_new (GST_TYPE_CAMERA_CAPTURER, NULL);

  gcc->priv->main_pipeline = gst_pipeline_new ("main_pipeline");

  if (!gcc->priv->main_pipeline) {
    g_set_error (err,
        GCC_ERROR,
        GST_ERROR_PLUGIN_LOAD,
        "Failed to create the pipeline element. "
        "Please check your GStreamer installation.");
    goto missing_plugin;
  }

  /* assume we're always called from the main Gtk+ GUI thread */
  gui_thread = g_thread_self ();

  /*Connect bus signals */
  GST_INFO_OBJECT (gcc, "Connecting bus signals");
  gcc->priv->bus = gst_element_get_bus (GST_ELEMENT (gcc->priv->main_pipeline));
  gst_bus_add_signal_watch (gcc->priv->bus);
  gcc->priv->sig_bus_async =
      g_signal_connect (gcc->priv->bus, "message",
      G_CALLBACK (gcc_bus_message_cb), gcc);

  /* we want to catch "prepare-xwindow-id" element messages synchronously */
  gst_bus_set_sync_handler (gcc->priv->bus, gst_bus_sync_signal_handler, gcc);

  gcc->priv->sig_bus_sync =
      g_signal_connect (gcc->priv->bus, "sync-message::element",
      G_CALLBACK (gcc_element_msg_sync), gcc);

  return gcc;

/* Missing plugin */
missing_plugin:
  {
    g_object_ref_sink (gcc);
    g_object_unref (gcc);
    return NULL;
  }
}
