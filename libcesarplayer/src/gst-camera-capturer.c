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

#include <gst/interfaces/xoverlay.h>
#include <gst/interfaces/propertyprobe.h>
#include <gst/gst.h>
#include <gst/video/video.h>

#include "gst-camera-capturer.h"
#include "gstscreenshot.h"

/*Default video source*/
#if defined(OSTYPE_WINDOWS)
#define DVVIDEOSRC "dshowvideosrc"
#define RAWVIDEOSRC "dshowvideosrc"
#define AUDIOSRC "dshowaudiosrc"
#elif defined(OSTYPE_OS_X)
#define DVVIDEOSRC "osxvideosrc"
#define RAWVIDEOSRC "osxvideosrc"
#define AUDIOSRC "osxaudiosrc"
#elif defined(OSTYPE_LINUX)
#define DVVIDEOSRC "dv1394src"
#define RAWVIDEOSRC "gsettingsvideosrc"
#define AUDIOSRC "gsettingsaudiosrc"
#define RAWVIDEOSRC_GCONF "gconfvideosrc"
#define AUDIOSRC_GCONF "gconfaudiosrc"
#endif

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

#ifdef WIN32
#define DEFAULT_SOURCE_TYPE  GST_CAMERA_CAPTURE_SOURCE_TYPE_DSHOW
#else
#define DEFAULT_SOURCE_TYPE  GST_CAMERA_CAPTURE_SOURCE_TYPE_RAW
#endif

typedef enum
{
  GST_CAMERABIN_FLAG_SOURCE_RESIZE = (1 << 0),
  GST_CAMERABIN_FLAG_SOURCE_COLOR_CONVERSION = (1 << 1),
  GST_CAMERABIN_FLAG_VIEWFINDER_COLOR_CONVERSION = (1 << 2),
  GST_CAMERABIN_FLAG_VIEWFINDER_SCALE = (1 << 3),
  GST_CAMERABIN_FLAG_AUDIO_CONVERSION = (1 << 4),
  GST_CAMERABIN_FLAG_DISABLE_AUDIO = (1 << 5),
  GST_CAMERABIN_FLAG_IMAGE_COLOR_CONVERSION = (1 << 6)
} GstCameraBinFlags;

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
  guint output_fps_n;
  guint output_fps_d;
  guint audio_bitrate;
  guint video_bitrate;
  gboolean audio_enabled;
  VideoEncoderType video_encoder_type;
  AudioEncoderType audio_encoder_type;

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
  GstCameraCaptureSourceType source_type;

  /* Snapshots */
  GstBuffer *last_buffer;

  /*GStreamer elements */
  GstElement *main_pipeline;
  GstElement *camerabin;
  GstElement *videosrc;
  GstElement *device_source;
  GstElement *videofilter;
  GstElement *audiosrc;
  GstElement *videoenc;
  GstElement *audioenc;
  GstElement *videomux;

  /*Overlay */
  GstXOverlay *xoverlay;        /* protect with lock */
  guint interface_update_id;    /* protect with lock */
  GMutex *lock;

  /*Videobox */
  GdkWindow *video_window;
  gboolean logo_mode;
  GdkPixbuf *logo_pixbuf;
  float zoom;

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
static void gcc_update_interface_implementations (GstCameraCapturer * gcc);
static int gcc_get_video_stream_info (GstCameraCapturer * gcc);

G_DEFINE_TYPE (GstCameraCapturer, gst_camera_capturer, GTK_TYPE_EVENT_BOX);

static void
gst_camera_capturer_init (GstCameraCapturer * object)
{
  GstCameraCapturerPrivate *priv;
  object->priv = priv =
      G_TYPE_INSTANCE_GET_PRIVATE (object, GST_TYPE_CAMERA_CAPTURER,
      GstCameraCapturerPrivate);

  GTK_WIDGET_SET_FLAGS (GTK_WIDGET (object), GTK_CAN_FOCUS);
  GTK_WIDGET_UNSET_FLAGS (GTK_WIDGET (object), GTK_DOUBLE_BUFFERED);

  priv->zoom = 1.0;
  priv->output_height = 576;
  priv->output_width = 720;
  priv->output_fps_n = 25;
  priv->output_fps_d = 1;
  priv->audio_bitrate = 128;
  priv->video_bitrate = 5000;
  priv->last_buffer = NULL;
  priv->source_type = GST_CAMERA_CAPTURE_SOURCE_TYPE_NONE;

  priv->lock = g_mutex_new ();
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

  if (gcc->priv->interface_update_id) {
    g_source_remove (gcc->priv->interface_update_id);
    gcc->priv->interface_update_id = 0;
  }

  if (gcc->priv->last_buffer != NULL)
    gst_buffer_unref (gcc->priv->last_buffer);

  if (gcc->priv->main_pipeline != NULL
      && GST_IS_ELEMENT (gcc->priv->main_pipeline)) {
    gst_element_set_state (gcc->priv->main_pipeline, GST_STATE_NULL);
    gst_object_unref (gcc->priv->main_pipeline);
    gcc->priv->main_pipeline = NULL;
  }

  g_mutex_free (gcc->priv->lock);

  G_OBJECT_CLASS (parent_class)->finalize (object);
}

static void
gst_camera_capturer_apply_resolution (GstCameraCapturer * gcc)
{
  GST_INFO_OBJECT (gcc, "Changed video resolution to %dx%d@%d/%dfps",
      gcc->priv->output_width, gcc->priv->output_height,
      gcc->priv->output_fps_n, gcc->priv->output_fps_d);

  g_signal_emit_by_name (G_OBJECT (gcc->priv->camerabin),
      "set-video-resolution-fps", gcc->priv->output_width,
      gcc->priv->output_height, gcc->priv->output_fps_n,
      gcc->priv->output_fps_d);
}

static void
gst_camera_capturer_set_video_bit_rate (GstCameraCapturer * gcc, gint bitrate)
{
  gcc->priv->video_bitrate = bitrate;
  if (gcc->priv->video_encoder_type == VIDEO_ENCODER_MPEG4 ||
      gcc->priv->video_encoder_type == VIDEO_ENCODER_XVID)
    g_object_set (gcc->priv->videoenc, "bitrate", bitrate * 1000, NULL);
  else
    g_object_set (gcc->priv->videoenc, "bitrate", gcc->priv->video_bitrate,
        NULL);
  GST_INFO_OBJECT (gcc, "Changed video bitrate to :\n%d",
      gcc->priv->video_bitrate);
}

static void
gst_camera_capturer_set_audio_bit_rate (GstCameraCapturer * gcc, gint bitrate)
{

  gcc->priv->audio_bitrate = bitrate;
  if (gcc->priv->audio_encoder_type == AUDIO_ENCODER_MP3)
    g_object_set (gcc->priv->audioenc, "bitrate", bitrate, NULL);
  else
    g_object_set (gcc->priv->audioenc, "bitrate", 1000 * bitrate, NULL);
  GST_INFO_OBJECT (gcc, "Changed audio bitrate to :\n%d",
      gcc->priv->audio_bitrate);

}

static void
gst_camera_capturer_set_audio_enabled (GstCameraCapturer * gcc,
    gboolean enabled)
{
  gint flags;
  gcc->priv->audio_enabled = enabled;

  g_object_get (gcc->priv->main_pipeline, "flags", &flags, NULL);
  if (!enabled) {
    flags &= ~GST_CAMERABIN_FLAG_DISABLE_AUDIO;
    GST_INFO_OBJECT (gcc, "Audio disabled");
  } else {
    flags |= GST_CAMERABIN_FLAG_DISABLE_AUDIO;
    GST_INFO_OBJECT (gcc, "Audio enabled");
  }
}

static void
gst_camera_capturer_set_output_file (GstCameraCapturer * gcc,
    const gchar * file)
{
  gcc->priv->output_file = g_strdup (file);
  g_object_set (gcc->priv->camerabin, "filename", file, NULL);
  GST_INFO_OBJECT (gcc, "Changed output filename to :\n%s", file);

}

static void
gst_camera_capturer_set_device_id (GstCameraCapturer * gcc,
    const gchar * device_id)
{
  gcc->priv->device_id = g_strdup (device_id);
  GST_INFO_OBJECT (gcc, "Changed device id/name to :\n%s", device_id);
}

/***********************************
*           
*           GTK Widget
*
************************************/

static void
gst_camera_capturer_size_request (GtkWidget * widget,
    GtkRequisition * requisition)
{
  requisition->width = 320;
  requisition->height = 240;
}

static void
get_media_size (GstCameraCapturer * gcc, gint * width, gint * height)
{
  if (gcc->priv->logo_mode) {
    if (gcc->priv->logo_pixbuf) {
      *width = gdk_pixbuf_get_width (gcc->priv->logo_pixbuf);
      *height = gdk_pixbuf_get_height (gcc->priv->logo_pixbuf);
    } else {
      *width = 0;
      *height = 0;
    }
  } else {

    GValue *disp_par = NULL;
    guint movie_par_n, movie_par_d, disp_par_n, disp_par_d, num, den;

    /* Create and init the fraction value */
    disp_par = g_new0 (GValue, 1);
    g_value_init (disp_par, GST_TYPE_FRACTION);

    /* Square pixel is our default */
    gst_value_set_fraction (disp_par, 1, 1);

    /* Now try getting display's pixel aspect ratio */
    if (gcc->priv->xoverlay) {
      GObjectClass *klass;
      GParamSpec *pspec;

      klass = G_OBJECT_GET_CLASS (gcc->priv->xoverlay);
      pspec = g_object_class_find_property (klass, "pixel-aspect-ratio");

      if (pspec != NULL) {
        GValue disp_par_prop = { 0, };

        g_value_init (&disp_par_prop, pspec->value_type);
        g_object_get_property (G_OBJECT (gcc->priv->xoverlay),
            "pixel-aspect-ratio", &disp_par_prop);

        if (!g_value_transform (&disp_par_prop, disp_par)) {
          GST_WARNING ("Transform failed, assuming pixel-aspect-ratio = 1/1");
          gst_value_set_fraction (disp_par, 1, 1);
        }

        g_value_unset (&disp_par_prop);
      }
    }

    disp_par_n = gst_value_get_fraction_numerator (disp_par);
    disp_par_d = gst_value_get_fraction_denominator (disp_par);

    GST_DEBUG_OBJECT (gcc, "display PAR is %d/%d", disp_par_n, disp_par_d);

    /* Use the movie pixel aspect ratio if any */
    if (gcc->priv->movie_par) {
      movie_par_n = gst_value_get_fraction_numerator (gcc->priv->movie_par);
      movie_par_d = gst_value_get_fraction_denominator (gcc->priv->movie_par);
    } else {
      /* Square pixels */
      movie_par_n = 1;
      movie_par_d = 1;
    }

    GST_DEBUG_OBJECT (gcc, "movie PAR is %d/%d", movie_par_n, movie_par_d);

    if (gcc->priv->video_width == 0 || gcc->priv->video_height == 0) {
      GST_DEBUG_OBJECT (gcc, "width and/or height 0, assuming 1/1 ratio");
      num = 1;
      den = 1;
    } else if (!gst_video_calculate_display_ratio (&num, &den,
            gcc->priv->video_width,
            gcc->priv->video_height,
            movie_par_n, movie_par_d, disp_par_n, disp_par_d)) {
      GST_WARNING ("overflow calculating display aspect ratio!");
      num = 1;                  /* FIXME: what values to use here? */
      den = 1;
    }

    GST_DEBUG_OBJECT (gcc, "calculated scaling ratio %d/%d for video %dx%d",
        num, den, gcc->priv->video_width, gcc->priv->video_height);

    /* now find a width x height that respects this display ratio.
     * prefer those that have one of w/h the same as the incoming video
     * using wd / hd = num / den */

    /* start with same height, because of interlaced video */
    /* check hd / den is an integer scale factor, and scale wd with the PAR */
    if (gcc->priv->video_height % den == 0) {
      GST_DEBUG_OBJECT (gcc, "keeping video height");
      gcc->priv->video_width_pixels =
          (guint) gst_util_uint64_scale (gcc->priv->video_height, num, den);
      gcc->priv->video_height_pixels = gcc->priv->video_height;
    } else if (gcc->priv->video_width % num == 0) {
      GST_DEBUG_OBJECT (gcc, "keeping video width");
      gcc->priv->video_width_pixels = gcc->priv->video_width;
      gcc->priv->video_height_pixels =
          (guint) gst_util_uint64_scale (gcc->priv->video_width, den, num);
    } else {
      GST_DEBUG_OBJECT (gcc, "approximating while keeping video height");
      gcc->priv->video_width_pixels =
          (guint) gst_util_uint64_scale (gcc->priv->video_height, num, den);
      gcc->priv->video_height_pixels = gcc->priv->video_height;
    }
    GST_DEBUG_OBJECT (gcc, "scaling to %dx%d", gcc->priv->video_width_pixels,
        gcc->priv->video_height_pixels);

    *width = gcc->priv->video_width_pixels;
    *height = gcc->priv->video_height_pixels;

    /* Free the PAR fraction */
    g_value_unset (disp_par);
    g_free (disp_par);

  }
}

static void
resize_video_window (GstCameraCapturer * gcc)
{
  const GtkAllocation *allocation;
  gfloat width, height, ratio, x, y;
  int w, h;

  g_return_if_fail (gcc != NULL);
  g_return_if_fail (GST_IS_CAMERA_CAPTURER (gcc));

  allocation = &GTK_WIDGET (gcc)->allocation;

  get_media_size (gcc, &w, &h);

  if (!w || !h) {
    w = allocation->width;
    h = allocation->height;
  }
  width = w;
  height = h;

  /* calculate ratio for fitting video into the available space */
  if ((gfloat) allocation->width / width > (gfloat) allocation->height / height) {
    ratio = (gfloat) allocation->height / height;
  } else {
    ratio = (gfloat) allocation->width / width;
  }

  /* apply zoom factor */
  ratio = ratio * gcc->priv->zoom;

  width *= ratio;
  height *= ratio;
  x = (allocation->width - width) / 2;
  y = (allocation->height - height) / 2;

  gdk_window_move_resize (gcc->priv->video_window, x, y, width, height);
  gtk_widget_queue_draw (GTK_WIDGET (gcc));
}

static void
gst_camera_capturer_size_allocate (GtkWidget * widget,
    GtkAllocation * allocation)
{
  GstCameraCapturer *gcc = GST_CAMERA_CAPTURER (widget);

  g_return_if_fail (widget != NULL);
  g_return_if_fail (GST_IS_CAMERA_CAPTURER (widget));

  widget->allocation = *allocation;

  if (GTK_WIDGET_REALIZED (widget)) {
    gdk_window_move_resize (gtk_widget_get_window (widget),
        allocation->x, allocation->y, allocation->width, allocation->height);
    resize_video_window (gcc);
  }
}

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
gst_camera_capturer_realize (GtkWidget * widget)
{
  GstCameraCapturer *gcc = GST_CAMERA_CAPTURER (widget);
  GdkWindowAttr attributes;
  gint attributes_mask, w, h;
  GdkColor colour;
  GdkWindow *window;
  GdkEventMask event_mask;

  event_mask = gtk_widget_get_events (widget)
      | GDK_POINTER_MOTION_MASK | GDK_KEY_PRESS_MASK;
  gtk_widget_set_events (widget, event_mask);

  GTK_WIDGET_CLASS (parent_class)->realize (widget);

  window = gtk_widget_get_window (widget);

  /* Creating our video window */
  attributes.window_type = GDK_WINDOW_CHILD;
  attributes.x = 0;
  attributes.y = 0;
  attributes.width = widget->allocation.width;
  attributes.height = widget->allocation.height;
  attributes.wclass = GDK_INPUT_OUTPUT;
  attributes.event_mask = gtk_widget_get_events (widget);
  attributes.event_mask |= GDK_EXPOSURE_MASK |
      GDK_POINTER_MOTION_MASK | GDK_BUTTON_PRESS_MASK | GDK_KEY_PRESS_MASK;
  attributes_mask = GDK_WA_X | GDK_WA_Y;

  gcc->priv->video_window = gdk_window_new (window,
      &attributes, attributes_mask);
  gdk_window_set_user_data (gcc->priv->video_window, widget);

  gdk_color_parse ("black", &colour);
  gdk_colormap_alloc_color (gtk_widget_get_colormap (widget),
      &colour, TRUE, TRUE);
  gdk_window_set_background (window, &colour);
  gtk_widget_set_style (widget,
      gtk_style_attach (gtk_widget_get_style (widget), window));

  GTK_WIDGET_SET_FLAGS (widget, GTK_REALIZED);

  /* Connect to configure event on the top level window */
  g_signal_connect (G_OBJECT (widget), "configure-event",
      G_CALLBACK (gst_camera_capturer_configure_event), gcc);

  /* nice hack to show the logo fullsize, while still being resizable */
  get_media_size (GST_CAMERA_CAPTURER (widget), &w, &h);
}

static void
gst_camera_capturer_unrealize (GtkWidget * widget)
{
  GstCameraCapturer *gcc = GST_CAMERA_CAPTURER (widget);

  gdk_window_set_user_data (gcc->priv->video_window, NULL);
  gdk_window_destroy (gcc->priv->video_window);
  gcc->priv->video_window = NULL;

  GTK_WIDGET_CLASS (parent_class)->unrealize (widget);
}

static void
gst_camera_capturer_show (GtkWidget * widget)
{
  GstCameraCapturer *gcc = GST_CAMERA_CAPTURER (widget);
  GdkWindow *window;

  window = gtk_widget_get_window (widget);
  if (window)
    gdk_window_show (window);
  if (gcc->priv->video_window)
    gdk_window_show (gcc->priv->video_window);

  if (GTK_WIDGET_CLASS (parent_class)->show)
    GTK_WIDGET_CLASS (parent_class)->show (widget);
}

static void
gst_camera_capturer_hide (GtkWidget * widget)
{
  GstCameraCapturer *gcc = GST_CAMERA_CAPTURER (widget);
  GdkWindow *window;

  window = gtk_widget_get_window (widget);
  if (window)
    gdk_window_hide (window);
  if (gcc->priv->video_window)
    gdk_window_hide (gcc->priv->video_window);

  if (GTK_WIDGET_CLASS (parent_class)->hide)
    GTK_WIDGET_CLASS (parent_class)->hide (widget);
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

  g_mutex_lock (gcc->priv->lock);
  xoverlay = gcc->priv->xoverlay;
  if (xoverlay == NULL) {
    gcc_update_interface_implementations (gcc);
    resize_video_window (gcc);
    xoverlay = gcc->priv->xoverlay;
  }
  if (xoverlay != NULL)
    gst_object_ref (xoverlay);

  g_mutex_unlock (gcc->priv->lock);

  if (xoverlay != NULL && GST_IS_X_OVERLAY (xoverlay)) {
    gdk_window_show (gcc->priv->video_window);
    gst_set_window_handle (gcc->priv->xoverlay,gcc->priv->video_window);
  }

  /* Start with a nice black canvas */
  win = gtk_widget_get_window (widget);
  gdk_draw_rectangle (win, gtk_widget_get_style (widget)->black_gc, TRUE, 0,
      0, widget->allocation.width, widget->allocation.height);

  /* if there's only audio and no visualisation, draw the logo as well */
  draw_logo = gcc->priv->media_has_audio && !gcc->priv->media_has_video;

  if (gcc->priv->logo_mode || draw_logo) {
    if (gcc->priv->logo_pixbuf != NULL) {
      GdkPixbuf *frame;
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
    } else if (win) {
      /* No pixbuf, just draw a black background then */
      gdk_window_clear_area (win,
          0, 0, widget->allocation.width, widget->allocation.height);
    }
  } else {
    /* no logo, pass the expose to gst */
    if (xoverlay != NULL && GST_IS_X_OVERLAY (xoverlay)) {
      gst_x_overlay_expose (xoverlay);
    } else {
      /* No xoverlay to expose yet */
      gdk_window_clear_area (win,
          0, 0, widget->allocation.width, widget->allocation.height);
    }
  }
  if (xoverlay != NULL)
    gst_object_unref (xoverlay);

  return TRUE;
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
      gst_camera_capturer_apply_resolution (gcc);
      break;
    case PROP_OUTPUT_WIDTH:
      gcc->priv->output_width = g_value_get_uint (value);
      gst_camera_capturer_apply_resolution (gcc);
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
  widget_class->size_request = gst_camera_capturer_size_request;
  widget_class->size_allocate = gst_camera_capturer_size_allocate;
  widget_class->realize = gst_camera_capturer_realize;
  widget_class->unrealize = gst_camera_capturer_unrealize;
  widget_class->show = gst_camera_capturer_show;
  widget_class->hide = gst_camera_capturer_hide;
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
cb_new_pad (GstElement * element, GstPad * pad, gpointer data)
{
  GstCaps *caps;
  const gchar *mime;
  GstElement *sink;
  GstBin *bin = GST_BIN (data);

  caps = gst_pad_get_caps (pad);
  mime = gst_structure_get_name (gst_caps_get_structure (caps, 0));
  if (g_strrstr (mime, "video")) {
    sink = gst_bin_get_by_name (bin, "source_video_sink");
    gst_pad_link (pad, gst_element_get_pad (sink, "sink"));
  }
  if (g_strrstr (mime, "audio")) {
    /* Not implemented yet */
  }
}

/* On linux GStreamer packages provided by distributions might still have the
 * dv1394src clock bug and the dvdemuxer buffers duration bug. That's why we
 * can't use decodebin2 and we need to force the use of ffdemux_dv */
static GstElement *
gst_camera_capture_create_dv1394_source_bin (GstCameraCapturer * gcc)
{
  GstElement *bin;
  GstElement *demuxer;
  GstElement *queue1;
  GstElement *decoder;
  GstElement *queue2;
  GstElement *deinterlacer;
  GstElement *colorspace;
  GstElement *videorate;
  GstElement *videoscale;
  GstPad *src_pad;

  bin = gst_bin_new ("videosource");
  gcc->priv->device_source =
      gst_element_factory_make (DVVIDEOSRC, "source_device");
  demuxer = gst_element_factory_make ("ffdemux_dv", NULL);
  queue1 = gst_element_factory_make ("queue", "source_video_sink");
  decoder = gst_element_factory_make ("ffdec_dvvideo", NULL);
  queue2 = gst_element_factory_make ("queue", NULL);
  deinterlacer = gst_element_factory_make ("ffdeinterlace", NULL);
  videorate = gst_element_factory_make ("videorate", NULL);
  colorspace = gst_element_factory_make ("ffmpegcolorspace", NULL);
  videoscale = gst_element_factory_make ("videoscale", NULL);

  /* this property needs to be set before linking the element, where the device
   * id configured in get_caps() */
  g_object_set (G_OBJECT (gcc->priv->device_source), "guid",
      g_ascii_strtoull (gcc->priv->device_id, NULL, 0), NULL);

  gst_bin_add_many (GST_BIN (bin), gcc->priv->device_source, demuxer, queue1,
      decoder, queue2, deinterlacer, colorspace, videorate, videoscale, NULL);
  gst_element_link (gcc->priv->device_source, demuxer);
  gst_element_link_many (queue1, decoder, queue2, deinterlacer, videorate,
      colorspace, videoscale, NULL);

  g_signal_connect (demuxer, "pad-added", G_CALLBACK (cb_new_pad), bin);

  /* add ghostpad */
  src_pad = gst_element_get_static_pad (videoscale, "src");
  gst_element_add_pad (bin, gst_ghost_pad_new ("src", src_pad));
  gst_object_unref (GST_OBJECT (src_pad));

  return bin;
}

static GstElement *
gst_camera_capture_create_dshow_source_bin (GstCameraCapturer * gcc)
{
  GstElement *bin;
  GstElement *decoder;
  GstElement *deinterlacer;
  GstElement *colorspace;
  GstElement *videorate;
  GstElement *videoscale;
  GstPad *src_pad;
  GstCaps *source_caps;

  bin = gst_bin_new ("videosource");
  gcc->priv->device_source =
      gst_element_factory_make (DVVIDEOSRC, "source_device");
  decoder = gst_element_factory_make ("decodebin2", NULL);
  colorspace = gst_element_factory_make ("ffmpegcolorspace",
      "source_video_sink");
  deinterlacer = gst_element_factory_make ("ffdeinterlace", NULL);
  videorate = gst_element_factory_make ("videorate", NULL);
  videoscale = gst_element_factory_make ("videoscale", NULL);

  /* this property needs to be set before linking the element, where the device
   * id configured in get_caps() */
  g_object_set (G_OBJECT (gcc->priv->device_source), "device-name",
      gcc->priv->device_id, NULL);

  gst_bin_add_many (GST_BIN (bin), gcc->priv->device_source, decoder,
      colorspace, deinterlacer, videorate, videoscale, NULL);
  source_caps =
      gst_caps_from_string ("video/x-dv, systemstream=true;"
      "video/x-raw-rgb; video/x-raw-yuv");
  gst_element_link_filtered (gcc->priv->device_source, decoder, source_caps);
  gst_element_link_many (colorspace, deinterlacer, videorate, videoscale, NULL);

  g_signal_connect (decoder, "pad-added", G_CALLBACK (cb_new_pad), bin);

  /* add ghostpad */
  src_pad = gst_element_get_static_pad (videoscale, "src");
  gst_element_add_pad (bin, gst_ghost_pad_new ("src", src_pad));
  gst_object_unref (GST_OBJECT (src_pad));

  return bin;
}

gboolean
gst_camera_capturer_set_source (GstCameraCapturer * gcc,
    GstCameraCaptureSourceType source_type, GError ** err)
{
  GstPad *videosrcpad;

  g_return_val_if_fail (gcc != NULL, FALSE);
  g_return_val_if_fail (GST_IS_CAMERA_CAPTURER (gcc), FALSE);

  if (gcc->priv->source_type == source_type)
    return TRUE;
  gcc->priv->source_type = source_type;

  switch (gcc->priv->source_type) {
    case GST_CAMERA_CAPTURE_SOURCE_TYPE_DV:
    {
      gcc->priv->videosrc = gst_camera_capture_create_dv1394_source_bin (gcc);
      /*gcc->priv->audiosrc = gcc->priv->videosrc; */
      break;
    }
    case GST_CAMERA_CAPTURE_SOURCE_TYPE_DSHOW:
    {
      gcc->priv->videosrc = gst_camera_capture_create_dshow_source_bin (gcc);
      /*gcc->priv->audiosrc = gcc->priv->videosrc; */
      break;
    }
    case GST_CAMERA_CAPTURE_SOURCE_TYPE_RAW:
    default:
    {
      gchar *videosrc = RAWVIDEOSRC;

#if defined(OSTYPE_WINDOWS)
      GstElementFactory *fact = gst_element_factory_find(RAWVIDEOSRC);
      if (fact == NULL)
        videosrc = RAWVIDEOSRC_GCONF;
      else
        gst_object_unref (fact);
#endif

      gchar *bin =
          g_strdup_printf ("%s name=device_source ! videorate ! "
          "ffmpegcolorspace ! videoscale", videosrc);
      gcc->priv->videosrc = gst_parse_bin_from_description (bin, TRUE, err);
      gcc->priv->device_source =
          gst_bin_get_by_name (GST_BIN (gcc->priv->videosrc), "device_source");
      gcc->priv->audiosrc = gst_element_factory_make (AUDIOSRC, "audiosource");
      break;
    }
  }
  if (*err) {
    GST_ERROR_OBJECT (gcc, "Error changing source: %s", (*err)->message);
    return FALSE;
  }

  g_object_set (gcc->priv->camerabin, "video-source", gcc->priv->videosrc,
      NULL);

  /* Install pad probe to store the last buffer */
  videosrcpad = gst_element_get_pad (gcc->priv->videosrc, "src");
  gst_pad_add_buffer_probe (videosrcpad,
      G_CALLBACK (gst_camera_capture_videosrc_buffer_probe), gcc);
  return TRUE;
}

GstCameraCapturer *
gst_camera_capturer_new (gchar * filename, GError ** err)
{
  GstCameraCapturer *gcc = NULL;
  gchar *plugin;
  gint flags = 0;

  gcc = g_object_new (GST_TYPE_CAMERA_CAPTURER, NULL);

  gcc->priv->main_pipeline = gst_pipeline_new ("main_pipeline");

  if (!gcc->priv->main_pipeline) {
    plugin = "pipeline";
    goto missing_plugin;
  }

  /* Setup */
  GST_INFO_OBJECT (gcc, "Initializing camerabin");
  gcc->priv->camerabin = gst_element_factory_make ("camerabin", "camerabin");
  gst_bin_add (GST_BIN (gcc->priv->main_pipeline), gcc->priv->camerabin);
  if (!gcc->priv->camerabin) {
    plugin = "camerabin";
    goto missing_plugin;
  }
  GST_INFO_OBJECT (gcc, "Setting capture mode to \"video\"");
  g_object_set (gcc->priv->camerabin, "mode", 1, NULL);


  GST_INFO_OBJECT (gcc, "Disabling audio");
  flags = GST_CAMERABIN_FLAG_DISABLE_AUDIO;
#ifdef WIN32
  flags |= GST_CAMERABIN_FLAG_VIEWFINDER_COLOR_CONVERSION;
#endif
  g_object_set (gcc->priv->camerabin, "flags", flags, NULL);

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
    g_set_error (err, GCC_ERROR, GST_ERROR_PLUGIN_LOAD,
        ("Failed to create a GStreamer element. "
            "The element \"%s\" is missing. "
            "Please check your GStreamer installation."), plugin);
    g_object_ref_sink (gcc);
    g_object_unref (gcc);
    return NULL;
  }
}

void
gst_camera_capturer_run (GstCameraCapturer * gcc)
{
  GError *err = NULL;

  g_return_if_fail (gcc != NULL);
  g_return_if_fail (GST_IS_CAMERA_CAPTURER (gcc));

  /* the source needs to be created before the 'device-is' is set
   * because dshowsrcwrapper can't change the device-name after
   * it has been linked for the first time */
  if (!gcc->priv->videosrc)
    gst_camera_capturer_set_source (gcc, gcc->priv->source_type, &err);
  gst_element_set_state (gcc->priv->main_pipeline, GST_STATE_PLAYING);
}

void
gst_camera_capturer_close (GstCameraCapturer * gcc)
{
  g_return_if_fail (gcc != NULL);
  g_return_if_fail (GST_IS_CAMERA_CAPTURER (gcc));

  gst_element_set_state (gcc->priv->main_pipeline, GST_STATE_NULL);
}

void
gst_camera_capturer_start (GstCameraCapturer * gcc)
{
  g_return_if_fail (gcc != NULL);
  g_return_if_fail (GST_IS_CAMERA_CAPTURER (gcc));

  g_signal_emit_by_name (G_OBJECT (gcc->priv->camerabin), "capture-start", 0,
      0);
}

void
gst_camera_capturer_toggle_pause (GstCameraCapturer * gcc)
{
  g_return_if_fail (gcc != NULL);
  g_return_if_fail (GST_IS_CAMERA_CAPTURER (gcc));

  g_signal_emit_by_name (G_OBJECT (gcc->priv->camerabin), "capture-pause", 0,
      0);
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
  gst_element_set_state(gcc->priv->device_source, GST_STATE_NULL);
#endif
  g_signal_emit_by_name (G_OBJECT (gcc->priv->camerabin), "capture-stop", 0, 0);
}

gboolean
gst_camera_capturer_set_video_encoder (GstCameraCapturer * gcc,
    VideoEncoderType type, GError ** err)
{
  gchar *name = NULL;

  g_return_val_if_fail (gcc != NULL, FALSE);
  g_return_val_if_fail (GST_IS_CAMERA_CAPTURER (gcc), FALSE);

  switch (type) {
    case VIDEO_ENCODER_MPEG4:
      gcc->priv->videoenc =
          gst_element_factory_make ("ffenc_mpeg4", "video-encoder");
      g_object_set (gcc->priv->videoenc, "pass", 512,
          "max-key-interval", -1, NULL);
      name = "FFmpeg mpeg4 video encoder";
      break;

    case VIDEO_ENCODER_XVID:
      gcc->priv->videoenc =
          gst_element_factory_make ("xvidenc", "video-encoder");
      g_object_set (gcc->priv->videoenc, "pass", 1,
          "profile", 146, "max-key-interval", -1, NULL);
      name = "Xvid video encoder";
      break;

    case VIDEO_ENCODER_H264:
      gcc->priv->videoenc =
          gst_element_factory_make ("x264enc", "video-encoder");
      g_object_set (gcc->priv->videoenc, "key-int-max", 25, "pass", 17,
          "speed-preset", 3, NULL);
      name = "X264 video encoder";
      break;

    case VIDEO_ENCODER_THEORA:
      gcc->priv->videoenc =
          gst_element_factory_make ("theoraenc", "video-encoder");
      g_object_set (gcc->priv->videoenc, "keyframe-auto", FALSE,
          "keyframe-force", 25, NULL);
      name = "Theora video encoder";
      break;

    case VIDEO_ENCODER_VP8:
    default:
      gcc->priv->videoenc =
          gst_element_factory_make ("vp8enc", "video-encoder");
      g_object_set (gcc->priv->videoenc, "speed", 2, "threads", 8,
          "max-keyframe-distance", 25, NULL);
      name = "VP8 video encoder";
      break;

  }
  if (!gcc->priv->videoenc) {
    g_set_error (err,
        GCC_ERROR,
        GST_ERROR_PLUGIN_LOAD,
        "Failed to create the %s element. "
        "Please check your GStreamer installation.", name);
  } else {
    g_object_set (gcc->priv->camerabin, "video-encoder", gcc->priv->videoenc,
        NULL);
    gcc->priv->video_encoder_type = type;
  }
  return TRUE;
}

gboolean
gst_camera_capturer_set_audio_encoder (GstCameraCapturer * gcc,
    AudioEncoderType type, GError ** err)
{
  gchar *name = NULL;

  g_return_val_if_fail (gcc != NULL, FALSE);
  g_return_val_if_fail (GST_IS_CAMERA_CAPTURER (gcc), FALSE);

  switch (type) {
    case AUDIO_ENCODER_MP3:
      gcc->priv->audioenc =
          gst_element_factory_make ("lamemp3enc", "audio-encoder");
      g_object_set (gcc->priv->audioenc, "target", 0, NULL);
      name = "Mp3 audio encoder";
      break;

    case AUDIO_ENCODER_AAC:
      gcc->priv->audioenc = gst_element_factory_make ("faac", "audio-encoder");
      name = "AAC audio encoder";
      break;

    case AUDIO_ENCODER_VORBIS:
    default:
      gcc->priv->audioenc =
          gst_element_factory_make ("vorbisenc", "audio-encoder");
      name = "Vorbis audio encoder";
      break;
  }

  if (!gcc->priv->audioenc) {
    g_set_error (err,
        GCC_ERROR,
        GST_ERROR_PLUGIN_LOAD,
        "Failed to create the %s element. "
        "Please check your GStreamer installation.", name);
  } else {
    g_object_set (gcc->priv->camerabin, "audio-encoder", gcc->priv->audioenc,
        NULL);
    gcc->priv->audio_encoder_type = type;
  }

  return TRUE;
}

gboolean
gst_camera_capturer_set_video_muxer (GstCameraCapturer * gcc,
    VideoMuxerType type, GError ** err)
{
  gchar *name = NULL;

  g_return_val_if_fail (gcc != NULL, FALSE);
  g_return_val_if_fail (GST_IS_CAMERA_CAPTURER (gcc), FALSE);

  switch (type) {
    case VIDEO_MUXER_OGG:
      name = "OGG muxer";
      gcc->priv->videomux = gst_element_factory_make ("oggmux", "video-muxer");
      break;
    case VIDEO_MUXER_AVI:
      name = "AVI muxer";
      gcc->priv->videomux = gst_element_factory_make ("avimux", "video-muxer");
      break;
    case VIDEO_MUXER_MATROSKA:
      name = "Matroska muxer";
      gcc->priv->videomux =
          gst_element_factory_make ("matroskamux", "video-muxer");
      break;
    case VIDEO_MUXER_MP4:
      name = "MP4 muxer";
      gcc->priv->videomux = gst_element_factory_make ("qtmux", "video-muxer");
      break;
    case VIDEO_MUXER_WEBM:
    default:
      name = "WebM muxer";
      gcc->priv->videomux = gst_element_factory_make ("webmmux", "video-muxer");
      break;
  }

  if (!gcc->priv->videomux) {
    g_set_error (err,
        GCC_ERROR,
        GST_ERROR_PLUGIN_LOAD,
        "Failed to create the %s element. "
        "Please check your GStreamer installation.", name);
  } else {
    g_object_set (gcc->priv->camerabin, "video-muxer", gcc->priv->videomux,
        NULL);
  }

  return TRUE;
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
        resize_video_window (gcc);
        gtk_widget_queue_draw (GTK_WIDGET (gcc));
      }
    }

    case GST_MESSAGE_ELEMENT:
    {
      const GstStructure *s;
      gint device_change = 0;

      /* We only care about messages sent by the device source */
      if (GST_MESSAGE_SRC (message) != GST_OBJECT (gcc->priv->device_source))
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

static gboolean
gcc_update_interfaces_delayed (GstCameraCapturer * gcc)
{
  GST_DEBUG_OBJECT (gcc, "Delayed updating interface implementations");
  g_mutex_lock (gcc->priv->lock);
  gcc_update_interface_implementations (gcc);
  gcc->priv->interface_update_id = 0;
  g_mutex_unlock (gcc->priv->lock);

  return FALSE;
}

static void
gcc_update_interface_implementations (GstCameraCapturer * gcc)
{

  GstElement *element = NULL;

  if (g_thread_self () != gui_thread) {
    if (gcc->priv->interface_update_id)
      g_source_remove (gcc->priv->interface_update_id);
    gcc->priv->interface_update_id =
        g_idle_add ((GSourceFunc) gcc_update_interfaces_delayed, gcc);
    return;
  }

  GST_INFO_OBJECT (gcc, "Retrieving xoverlay from bin ...");
  element = gst_bin_get_by_interface (GST_BIN (gcc->priv->camerabin),
      GST_TYPE_X_OVERLAY);

  if (GST_IS_X_OVERLAY (element)) {
    gcc->priv->xoverlay = GST_X_OVERLAY (element);
  } else {
    gcc->priv->xoverlay = NULL;
  }
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
    g_mutex_lock (gcc->priv->lock);
    gcc_update_interface_implementations (gcc);
    g_mutex_unlock (gcc->priv->lock);

    if (gcc->priv->xoverlay == NULL) {
      GstObject *sender = GST_MESSAGE_SRC (msg);
      if (sender && GST_IS_X_OVERLAY (sender))
        gcc->priv->xoverlay = GST_X_OVERLAY (gst_object_ref (sender));
    }

    g_return_if_fail (gcc->priv->xoverlay != NULL);
    g_return_if_fail (gcc->priv->video_window != NULL);

    gst_set_window_handle (gcc->priv->xoverlay,gcc->priv->video_window);
  }
}

static int
gcc_get_video_stream_info (GstCameraCapturer * gcc)
{
  GstPad *sourcepad;
  GstCaps *caps;
  GstStructure *s;

  sourcepad = gst_element_get_pad (gcc->priv->videosrc, "src");
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
  else if (!g_strcmp0 (device_name, "v4l2src"))
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

gboolean
gst_camera_capturer_can_get_frames (GstCameraCapturer * gcc, GError ** error)
{
  g_return_val_if_fail (gcc != NULL, FALSE);
  g_return_val_if_fail (GST_IS_CAMERA_CAPTURER (gcc), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (gcc->priv->camerabin), FALSE);

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
  g_return_val_if_fail (GST_IS_ELEMENT (gcc->priv->camerabin), NULL);

  gst_element_get_state (gcc->priv->camerabin, NULL, NULL, -1);

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
