 /*GStreamer Video Editor Based On GNonlin
  * Copyright (C) 2007-2009 Andoni Morales Alastruey <ylatuya@gmail.com>
  *
  * This program is free software.
  *
  * You may redistribute it and/or modify it under the terms of the
  * GNU General Public License, as published by the Free Software
  * Foundation; either version 2 of the License, or (at your option)
  * any later version.
  *
  * Gstreamer Video Transcoderis distributed in the hope that it will be useful,
  * but WITHOUT ANY WARRANTY; without even the implied warranty of
  * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
  * See the GNU General Public License for more details.
  *
  * You should have received a copy of the GNU General Public License
  * along with foob.  If not, write to:
  *     The Free Software Foundation, Inc.,
  *     51 Franklin Street, Fifth Floor
  *     Boston, MA  02110-1301, USA.
  */

#include <string.h>
#include <stdio.h>
#include <gst/gst.h>
#include "gst-video-editor.h"


#define DEFAULT_VIDEO_ENCODER "vp8enc"
#define DEFAULT_AUDIO_ENCODER "vorbisenc"
#define DEFAULT_VIDEO_MUXER "matroskamux"
#define FONT_SIZE_FACTOR 0.05
#define LAME_CAPS "audio/x-raw-int, rate=44100, channels=2, endianness=1234, signed=true, width=16, depth=16"
#define VORBIS_CAPS "audio/x-raw-float, rate=44100, channels=2, endianness=1234, signed=true, width=32, depth=32"
#define FAAC_CAPS "audio/x-raw-int, rate=44100, channels=2, endianness=1234, signed=true, width=16, depth=16"
#define AC3_CAPS "audio/x-raw-int, rate=44100, channels=2, endianness=1234, signed=true, width=16, depth=16"

#define TIMEOUT 50

/* Signals */
enum
{
  SIGNAL_ERROR,
  SIGNAL_EOS,
  SIGNAL_PERCENT_COMPLETED,
  LAST_SIGNAL
};

/* Properties */
enum
{
  PROP_0,
  PROP_ENABLE_AUDIO,
  PROP_ENABLE_TITLE,
  PROP_VIDEO_BITRATE,
  PROP_AUDIO_BITRATE,
  PROP_HEIGHT,
  PROP_WIDTH,
  PROP_OUTPUT_FILE,
  PROP_TITLE_SIZE
};

struct GstVideoEditorPrivate
{
  gint segments;
  gint active_segment;
  gint64 *stop_times;
  GList *titles;
  GList *gnl_video_filesources;
  GList *gnl_audio_filesources;
  gint64 duration;

  /* Properties */
  gboolean audio_enabled;
  gboolean title_enabled;
  gchar *output_file;
  gint audio_bitrate;
  gint video_bitrate;
  gint width;
  gint height;
  gint title_size;

  /* Bins */
  GstElement *main_pipeline;
  GstElement *vencode_bin;
  GstElement *aencode_bin;

  /* Source */
  GstElement *gnl_video_composition;
  GstElement *gnl_audio_composition;

  /* Video */
  GstElement *identity;
  GstElement *ffmpegcolorspace;
  GstElement *videorate;
  GstElement *textoverlay;
  GstElement *videoscale;
  GstElement *capsfilter;
  GstElement *queue;
  GstElement *video_encoder;
  VideoEncoderType video_encoder_type;

  /* Audio */
  GstElement *audioidentity;
  GstElement *audioconvert;
  GstElement *audioresample;
  GstElement *audiocapsfilter;
  GstElement *audioqueue;
  GstElement *audioencoder;

  /* Sink */
  GstElement *muxer;
  GstElement *file_sink;

  GstBus *bus;
  gulong sig_bus_async;

  gint update_id;
};

static int gve_signals[LAST_SIGNAL] = { 0 };

static void gve_error_msg (GstVideoEditor * gve, GstMessage * msg);
static void new_decoded_pad_cb (GstElement * object, GstPad * arg0,
    gpointer user_data);
static void gve_bus_message_cb (GstBus * bus, GstMessage * message,
    gpointer data);
static void gst_video_editor_get_property (GObject * object,
    guint property_id, GValue * value, GParamSpec * pspec);
static void gst_video_editor_set_property (GObject * object,
    guint property_id, const GValue * value, GParamSpec * pspec);
static gboolean gve_query_timeout (GstVideoEditor * gve);
static void gve_apply_new_caps (GstVideoEditor * gve);
static void gve_apply_title_size (GstVideoEditor * gve);
static void gve_rewrite_headers (GstVideoEditor * gve);
G_DEFINE_TYPE (GstVideoEditor, gst_video_editor, G_TYPE_OBJECT);



/* =========================================== */
/*                                             */
/*      Class Initialization/Finalization      */
/*                                             */
/* =========================================== */

static void
gst_video_editor_init (GstVideoEditor * object)
{
  GstVideoEditorPrivate *priv;
  object->priv = priv =
      G_TYPE_INSTANCE_GET_PRIVATE (object, GST_TYPE_VIDEO_EDITOR,
      GstVideoEditorPrivate);

  priv->output_file = "new_video.avi";

  priv->audio_bitrate = 128000;
  priv->video_bitrate = 5000;
  priv->height = 540;
  priv->width = 720;
  priv->title_size = 20;
  priv->title_enabled = TRUE;
  priv->audio_enabled = TRUE;

  priv->duration = 0;
  priv->segments = 0;
  priv->gnl_video_filesources = NULL;
  priv->gnl_audio_filesources = NULL;
  priv->titles = NULL;
  priv->stop_times = (gint64 *) malloc (200 * sizeof (gint64));

  priv->update_id = 0;
}

static void
gst_video_editor_finalize (GObject * object)
{
  GstVideoEditor *gve = (GstVideoEditor *) object;

  if (gve->priv->bus) {
    /* make bus drop all messages to make sure none of our callbacks is ever
       called again (main loop might be run again to display error dialog) */
    gst_bus_set_flushing (gve->priv->bus, TRUE);

    if (gve->priv->sig_bus_async)
      g_signal_handler_disconnect (gve->priv->bus, gve->priv->sig_bus_async);
    gst_object_unref (gve->priv->bus);
    gve->priv->bus = NULL;
  }

  if (gve->priv->main_pipeline != NULL
      && GST_IS_ELEMENT (gve->priv->main_pipeline)) {
    gst_element_set_state (gve->priv->main_pipeline, GST_STATE_NULL);
    gst_object_unref (gve->priv->main_pipeline);
    gve->priv->main_pipeline = NULL;
  }

  g_free (gve->priv->output_file);
  g_list_free (gve->priv->gnl_video_filesources);
  g_list_free (gve->priv->gnl_audio_filesources);
  g_free (gve->priv->stop_times);
  g_list_free (gve->priv->titles);

  G_OBJECT_CLASS (gst_video_editor_parent_class)->finalize (object);
}


static void
gst_video_editor_class_init (GstVideoEditorClass * klass)
{
  GObjectClass *object_class = G_OBJECT_CLASS (klass);

  object_class->finalize = gst_video_editor_finalize;

  g_type_class_add_private (object_class, sizeof (GstVideoEditorPrivate));

  /* GObject */
  object_class->set_property = gst_video_editor_set_property;
  object_class->get_property = gst_video_editor_get_property;
  object_class->finalize = gst_video_editor_finalize;

  /* Properties */
  g_object_class_install_property (object_class, PROP_ENABLE_AUDIO,
      g_param_spec_boolean ("enable-audio", NULL,
          NULL, TRUE, G_PARAM_READWRITE));

  g_object_class_install_property (object_class, PROP_ENABLE_TITLE,
      g_param_spec_boolean ("enable-title", NULL,
          NULL, TRUE, G_PARAM_READWRITE));

  g_object_class_install_property (object_class, PROP_VIDEO_BITRATE,
      g_param_spec_int ("video_bitrate", NULL,
          NULL, 100, G_MAXINT, 10000, G_PARAM_READWRITE));

  g_object_class_install_property (object_class, PROP_AUDIO_BITRATE,
      g_param_spec_int ("audio_bitrate", NULL,
          NULL, 12000, G_MAXINT, 128000, G_PARAM_READWRITE));

  g_object_class_install_property (object_class, PROP_HEIGHT,
      g_param_spec_int ("height", NULL, NULL,
          240, 1080, 480, G_PARAM_READWRITE));

  g_object_class_install_property (object_class, PROP_WIDTH,
      g_param_spec_int ("width", NULL, NULL, 320,
          1920, 720, G_PARAM_READWRITE));

  g_object_class_install_property (object_class, PROP_TITLE_SIZE,
      g_param_spec_int ("title-size", NULL, NULL,
          10, 100, 20, G_PARAM_READWRITE));

  g_object_class_install_property (object_class, PROP_OUTPUT_FILE,
      g_param_spec_string ("output_file", NULL, NULL, "", G_PARAM_READWRITE));

  /* Signals */
  gve_signals[SIGNAL_ERROR] =
      g_signal_new ("error",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (GstVideoEditorClass, error),
      NULL, NULL,
      g_cclosure_marshal_VOID__STRING, G_TYPE_NONE, 1, G_TYPE_STRING);

  gve_signals[SIGNAL_PERCENT_COMPLETED] =
      g_signal_new ("percent_completed",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (GstVideoEditorClass, percent_completed),
      NULL, NULL, g_cclosure_marshal_VOID__FLOAT, G_TYPE_NONE, 1, G_TYPE_FLOAT);
}

/* =========================================== */
/*                                             */
/*                Properties                   */
/*                                             */
/* =========================================== */

static void
gst_video_editor_set_enable_audio (GstVideoEditor * gve, gboolean audio_enabled)
{
  GstState cur_state;

  gst_element_get_state (gve->priv->main_pipeline, &cur_state, NULL, 0);

  if (cur_state > GST_STATE_READY) {
    GST_WARNING ("Could not enable/disable audio. Pipeline is playing");
    return;
  }

  if (!gve->priv->audio_enabled && audio_enabled) {
    /* Audio needs to be enabled and is disabled */
    gst_bin_add_many (GST_BIN (gve->priv->main_pipeline),
        gve->priv->gnl_audio_composition, gve->priv->aencode_bin, NULL);
    gst_element_link (gve->priv->aencode_bin, gve->priv->muxer);
    gst_element_set_state (gve->priv->gnl_audio_composition, cur_state);
    gst_element_set_state (gve->priv->aencode_bin, cur_state);
    gve_rewrite_headers (gve);
    gve->priv->audio_enabled = TRUE;
    GST_INFO ("Audio enabled");
  } else if (gve->priv->audio_enabled && !audio_enabled) {
    /* Audio is enabled and needs to be disabled) */
    gst_element_unlink_many (gve->priv->gnl_audio_composition,
        gve->priv->aencode_bin, gve->priv->muxer, NULL);
    gst_element_set_state (gve->priv->gnl_audio_composition, GST_STATE_NULL);
    gst_element_set_state (gve->priv->aencode_bin, GST_STATE_NULL);
    gst_object_ref (gve->priv->gnl_audio_composition);
    gst_object_ref (gve->priv->aencode_bin);
    gst_bin_remove_many (GST_BIN (gve->priv->main_pipeline),
        gve->priv->gnl_audio_composition, gve->priv->aencode_bin, NULL);
    gve_rewrite_headers (gve);
    gve->priv->audio_enabled = FALSE;
    GST_INFO ("Audio disabled");
  }
}

static void
gst_video_editor_set_enable_title (GstVideoEditor * gve, gboolean title_enabled)
{
  gve->priv->title_enabled = title_enabled;
  g_object_set (G_OBJECT (gve->priv->textoverlay), "silent",
      !gve->priv->title_enabled, NULL);
}

static void
gst_video_editor_set_video_bit_rate (GstVideoEditor * gve, gint bitrate)
{
  GstState cur_state;

  gve->priv->video_bitrate = bitrate;
  gst_element_get_state (gve->priv->video_encoder, &cur_state, NULL, 0);
  if (cur_state <= GST_STATE_READY) {
    if (gve->priv->video_encoder_type == VIDEO_ENCODER_THEORA ||
        gve->priv->video_encoder_type == VIDEO_ENCODER_H264)
      g_object_set (gve->priv->video_encoder, "bitrate", bitrate, NULL);
    else
      g_object_set (gve->priv->video_encoder, "bitrate", bitrate * 1000, NULL);
    GST_INFO_OBJECT (gve, "Encoding video bitrate changed to :%d (kbps)\n", bitrate);
  }
}

static void
gst_video_editor_set_audio_bit_rate (GstVideoEditor * gve, gint bitrate)
{
  GstState cur_state;

  gve->priv->audio_bitrate = bitrate;
  gst_element_get_state (gve->priv->audioencoder, &cur_state, NULL, 0);
  if (cur_state <= GST_STATE_READY) {
    g_object_set (gve->priv->audioencoder, "bitrate", bitrate, NULL);
    GST_INFO_OBJECT (gve, "Encoding audio bitrate changed to :%d (bps)\n", bitrate);
  }
}

static void
gst_video_editor_set_width (GstVideoEditor * gve, gint width)
{
  gve->priv->width = width;
  gve_apply_new_caps (gve);
}

static void
gst_video_editor_set_height (GstVideoEditor * gve, gint height)
{
  gve->priv->height = height;
  gve_apply_new_caps (gve);
}

static void
gst_video_editor_set_title_size (GstVideoEditor * gve, gint size)
{
  gve->priv->title_size = size;
  gve_apply_title_size (gve);
}

static void
gst_video_editor_set_output_file (GstVideoEditor * gve, const char *output_file)
{
  GstState cur_state;

  gve->priv->output_file = g_strdup (output_file);
  gst_element_get_state (gve->priv->file_sink, &cur_state, NULL, 0);
  if (cur_state <= GST_STATE_READY) {
    gst_element_set_state (gve->priv->file_sink, GST_STATE_NULL);
    g_object_set (gve->priv->file_sink, "location", gve->priv->output_file,
        NULL);
    GST_INFO_OBJECT (gve, "Ouput File changed to :%s\n", gve->priv->output_file);
  }
}

static void
gst_video_editor_set_property (GObject * object, guint property_id,
    const GValue * value, GParamSpec * pspec)
{
  GstVideoEditor *gve;

  gve = GST_VIDEO_EDITOR (object);

  switch (property_id) {
    case PROP_ENABLE_AUDIO:
      gst_video_editor_set_enable_audio (gve, g_value_get_boolean (value));
      break;
    case PROP_ENABLE_TITLE:
      gst_video_editor_set_enable_title (gve, g_value_get_boolean (value));
      break;
    case PROP_VIDEO_BITRATE:
      gst_video_editor_set_video_bit_rate (gve, g_value_get_int (value));
      break;
    case PROP_AUDIO_BITRATE:
      gst_video_editor_set_audio_bit_rate (gve, g_value_get_int (value));
      break;
    case PROP_WIDTH:
      gst_video_editor_set_width (gve, g_value_get_int (value));
      break;
    case PROP_HEIGHT:
      gst_video_editor_set_height (gve, g_value_get_int (value));
      break;
    case PROP_TITLE_SIZE:
      gst_video_editor_set_title_size (gve, g_value_get_int (value));
      break;
    case PROP_OUTPUT_FILE:
      gst_video_editor_set_output_file (gve, g_value_get_string (value));
      break;
    default:
      G_OBJECT_WARN_INVALID_PROPERTY_ID (object, property_id, pspec);
      break;
  }
}

static void
gst_video_editor_get_property (GObject * object, guint property_id,
    GValue * value, GParamSpec * pspec)
{
  GstVideoEditor *gve;

  gve = GST_VIDEO_EDITOR (object);

  switch (property_id) {
    case PROP_ENABLE_AUDIO:
      g_value_set_boolean (value, gve->priv->audio_enabled);
      break;
    case PROP_ENABLE_TITLE:
      g_value_set_boolean (value, gve->priv->title_enabled);
      break;
    case PROP_AUDIO_BITRATE:
      g_value_set_int (value, gve->priv->audio_bitrate);
      break;
    case PROP_VIDEO_BITRATE:
      g_value_set_int (value, gve->priv->video_bitrate);
      break;
    case PROP_WIDTH:
      g_value_set_int (value, gve->priv->width);
      break;
    case PROP_HEIGHT:
      g_value_set_int (value, gve->priv->height);
      break;
    case PROP_OUTPUT_FILE:
      g_value_set_string (value, gve->priv->output_file);
      break;
    default:
      G_OBJECT_WARN_INVALID_PROPERTY_ID (object, property_id, pspec);
      break;
  }
}

/* =========================================== */
/*                                             */
/*               Private Methods               */
/*                                             */
/* =========================================== */

static void
gve_rewrite_headers (GstVideoEditor * gve)
{
  gst_element_set_state (gve->priv->muxer, GST_STATE_NULL);
  gst_element_set_state (gve->priv->file_sink, GST_STATE_NULL);
  gst_element_set_state (gve->priv->file_sink, GST_STATE_READY);
  gst_element_set_state (gve->priv->muxer, GST_STATE_READY);
}

static void
gve_set_tick_timeout (GstVideoEditor * gve, guint msecs)
{
  g_return_if_fail (msecs > 0);

  GST_INFO ("adding tick timeout (at %ums)", msecs);
  gve->priv->update_id =
      g_timeout_add (msecs, (GSourceFunc) gve_query_timeout, gve);
}

static void
gve_apply_new_caps (GstVideoEditor * gve)
{
  GstCaps *caps;

  caps = gst_caps_new_simple ("video/x-raw-yuv",
      "width", G_TYPE_INT, gve->priv->width,
      "height", G_TYPE_INT, gve->priv->height,
      "pixel-aspect-ratio", GST_TYPE_FRACTION, 1, 1,
      "framerate", GST_TYPE_FRACTION, 25, 1, NULL);

  GST_INFO_OBJECT(gve, "Changed caps: %s", gst_caps_to_string(caps));
  g_object_set (G_OBJECT (gve->priv->capsfilter), "caps", caps, NULL);
  gst_caps_unref (caps);
}

static void
gve_apply_title_size (GstVideoEditor * gve)
{
  gchar *font;

  font = g_strdup_printf ("sans bold %d", gve->priv->title_size);
  g_object_set (G_OBJECT (gve->priv->textoverlay), "font-desc", font, NULL);
  g_free (font);
}

static void
gve_create_video_encode_bin (GstVideoEditor * gve)
{
  GstPad *sinkpad = NULL;
  GstPad *srcpad = NULL;

  if (gve->priv->vencode_bin != NULL)
    return;

  gve->priv->vencode_bin = gst_element_factory_make ("bin", "vencodebin");
  gve->priv->identity = gst_element_factory_make ("identity", "identity");
  gve->priv->ffmpegcolorspace =
      gst_element_factory_make ("ffmpegcolorspace", "ffmpegcolorspace");
  gve->priv->videorate = gst_element_factory_make ("videorate", "videorate");
  gve->priv->videoscale = gst_element_factory_make ("videoscale", "videoscale");
  gve->priv->capsfilter = gst_element_factory_make ("capsfilter", "capsfilter");
  gve->priv->textoverlay =
      gst_element_factory_make ("textoverlay", "textoverlay");
  gve->priv->queue = gst_element_factory_make ("queue", "video-encode-queue");
  gve->priv->video_encoder =
      gst_element_factory_make (DEFAULT_VIDEO_ENCODER, "video-encoder");

  g_object_set (G_OBJECT (gve->priv->identity), "single-segment", TRUE, NULL);
  g_object_set (G_OBJECT (gve->priv->textoverlay), "font-desc",
      "sans bold 20", "shaded-background", TRUE, "valignment", 2,
      "halignment", 2, NULL);
  g_object_set (G_OBJECT (gve->priv->videoscale), "add-borders", TRUE, NULL);
  g_object_set (G_OBJECT (gve->priv->video_encoder), "bitrate",
      gve->priv->video_bitrate, NULL);

  /*Add and link elements */
  gst_bin_add_many (GST_BIN (gve->priv->vencode_bin),
      gve->priv->identity,
      gve->priv->ffmpegcolorspace,
      gve->priv->videorate,
      gve->priv->videoscale,
      gve->priv->capsfilter,
      gve->priv->textoverlay, gve->priv->queue, gve->priv->video_encoder, NULL);
  gst_element_link_many (gve->priv->identity,
      gve->priv->ffmpegcolorspace,
      gve->priv->videoscale,
      gve->priv->videorate,
      gve->priv->capsfilter,
      gve->priv->textoverlay, gve->priv->queue, gve->priv->video_encoder, NULL);

  /*Create bin sink pad */
  sinkpad = gst_element_get_static_pad (gve->priv->identity, "sink");
  gst_pad_set_active (sinkpad, TRUE);
  gst_element_add_pad (GST_ELEMENT (gve->priv->vencode_bin),
      gst_ghost_pad_new ("sink", sinkpad));

  /*Creat bin src pad */
  srcpad = gst_element_get_static_pad (gve->priv->video_encoder, "src");
  gst_pad_set_active (srcpad, TRUE);
  gst_element_add_pad (GST_ELEMENT (gve->priv->vencode_bin),
      gst_ghost_pad_new ("src", srcpad));

  g_object_unref (srcpad);
  g_object_unref (sinkpad);
}

static void
gve_create_audio_encode_bin (GstVideoEditor * gve)
{
  GstPad *sinkpad = NULL;
  GstPad *srcpad = NULL;

  if (gve->priv->aencode_bin != NULL)
    return;

  gve->priv->aencode_bin = gst_element_factory_make ("bin", "aencodebin");
  gve->priv->audioidentity =
      gst_element_factory_make ("identity", "audio-identity");
  gve->priv->audioconvert =
      gst_element_factory_make ("audioconvert", "audioconvert");
  gve->priv->audioresample =
      gst_element_factory_make ("audioresample", "audioresample");
  gve->priv->audiocapsfilter =
      gst_element_factory_make ("capsfilter", "audiocapsfilter");
  gve->priv->audioqueue = gst_element_factory_make ("queue", "audio-queue");
  gve->priv->audioencoder =
      gst_element_factory_make (DEFAULT_AUDIO_ENCODER, "audio-encoder");


  g_object_set (G_OBJECT (gve->priv->audioidentity), "single-segment", TRUE,
      NULL);
  g_object_set (G_OBJECT (gve->priv->audiocapsfilter), "caps",
      gst_caps_from_string (VORBIS_CAPS), NULL);
  g_object_set (G_OBJECT (gve->priv->audioencoder), "bitrate",
      gve->priv->audio_bitrate, NULL);

  /*Add and link elements */
  gst_bin_add_many (GST_BIN (gve->priv->aencode_bin),
      gve->priv->audioidentity,
      gve->priv->audioconvert,
      gve->priv->audioresample,
      gve->priv->audiocapsfilter,
      gve->priv->audioqueue, gve->priv->audioencoder, NULL);

  gst_element_link_many (gve->priv->audioidentity,
      gve->priv->audioconvert,
      gve->priv->audioresample,
      gve->priv->audiocapsfilter,
      gve->priv->audioqueue, gve->priv->audioencoder, NULL);

  /*Create bin sink pad */
  sinkpad = gst_element_get_static_pad (gve->priv->audioidentity, "sink");
  gst_pad_set_active (sinkpad, TRUE);
  gst_element_add_pad (GST_ELEMENT (gve->priv->aencode_bin),
      gst_ghost_pad_new ("sink", sinkpad));

  /*Creat bin src pad */
  srcpad = gst_element_get_static_pad (gve->priv->audioencoder, "src");
  gst_pad_set_active (srcpad, TRUE);
  gst_element_add_pad (GST_ELEMENT (gve->priv->aencode_bin),
      gst_ghost_pad_new ("src", srcpad));

  g_object_unref (srcpad);
  g_object_unref (sinkpad);
}

GQuark
gst_video_editor_error_quark (void)
{
  static GQuark q;              /* 0 */

  if (G_UNLIKELY (q == 0)) {
    q = g_quark_from_static_string ("gve-error-quark");
  }
  return q;
}

/* =========================================== */
/*                                             */
/*                Callbacks                    */
/*                                             */
/* =========================================== */

static void
new_decoded_pad_cb (GstElement * object, GstPad * pad, gpointer user_data)
{
  GstCaps *caps = NULL;
  GstStructure *str = NULL;
  GstPad *videopad = NULL;
  GstPad *audiopad = NULL;
  GstVideoEditor *gve = NULL;

  g_return_if_fail (GST_IS_VIDEO_EDITOR (user_data));
  gve = GST_VIDEO_EDITOR (user_data);

  /* check media type */
  caps = gst_pad_get_caps (pad);
  str = gst_caps_get_structure (caps, 0);

  if (g_strrstr (gst_structure_get_name (str), "video")) {
    gst_element_set_state (gve->priv->vencode_bin, GST_STATE_PLAYING);
    videopad =
        gst_element_get_compatible_pad (gve->priv->vencode_bin, pad, NULL);
    /* only link once */
    if (GST_PAD_IS_LINKED (videopad)) {
      g_object_unref (videopad);
      gst_caps_unref (caps);
      return;
    }
    /* link 'n play */
    GST_INFO ("Found video stream...%" GST_PTR_FORMAT, caps);
    gst_pad_link (pad, videopad);
    g_object_unref (videopad);
  }

  else if (g_strrstr (gst_structure_get_name (str), "audio")) {
    gst_element_set_state (gve->priv->aencode_bin, GST_STATE_PLAYING);
    audiopad =
        gst_element_get_compatible_pad (gve->priv->aencode_bin, pad, NULL);
    /* only link once */
    if (GST_PAD_IS_LINKED (audiopad)) {
      g_object_unref (audiopad);
      gst_caps_unref (caps);
      return;
    }
    /* link 'n play */
    GST_INFO ("Found audio stream...%" GST_PTR_FORMAT, caps);
    gst_pad_link (pad, audiopad);
    g_object_unref (audiopad);
  }

  gst_caps_unref (caps);
}

static void
gve_bus_message_cb (GstBus * bus, GstMessage * message, gpointer data)
{
  GstVideoEditor *gve = (GstVideoEditor *) data;
  GstMessageType msg_type;

  g_return_if_fail (gve != NULL);
  g_return_if_fail (GST_IS_VIDEO_EDITOR (gve));

  msg_type = GST_MESSAGE_TYPE (message);

  switch (msg_type) {
    case GST_MESSAGE_ERROR:
      gve_error_msg (gve, message);
      if (gve->priv->main_pipeline)
        gst_element_set_state (gve->priv->main_pipeline, GST_STATE_READY);
      break;
    case GST_MESSAGE_WARNING:
      GST_WARNING ("Warning message: %" GST_PTR_FORMAT, message);
      break;

    case GST_MESSAGE_STATE_CHANGED:
    {
      GstState old_state, new_state;
      gchar *src_name;

      gst_message_parse_state_changed (message, &old_state, &new_state, NULL);

      if (old_state == new_state)
        break;

      /* we only care about playbin (pipeline) state changes */
      if (GST_MESSAGE_SRC (message) != GST_OBJECT (gve->priv->main_pipeline))
        break;

      src_name = gst_object_get_name (message->src);

      GST_INFO ("%s changed state from %s to %s", src_name,
          gst_element_state_get_name (old_state),
          gst_element_state_get_name (new_state));
      g_free (src_name);

      if (new_state == GST_STATE_PLAYING)
        gve_set_tick_timeout (gve, TIMEOUT);
      if (old_state == GST_STATE_PAUSED && new_state == GST_STATE_READY) {
        if (gve->priv->update_id > 0) {
          g_source_remove (gve->priv->update_id);
          gve->priv->update_id = 0;
        }
      }
      if (old_state == GST_STATE_NULL && new_state == GST_STATE_READY)
        GST_DEBUG_BIN_TO_DOT_FILE (GST_BIN (gve->priv->main_pipeline),
            GST_DEBUG_GRAPH_SHOW_ALL, "gst-camera-capturer-null-to-ready");
      if (old_state == GST_STATE_READY && new_state == GST_STATE_PAUSED)
        GST_DEBUG_BIN_TO_DOT_FILE (GST_BIN (gve->priv->main_pipeline),
            GST_DEBUG_GRAPH_SHOW_ALL, "gst-camera-capturer-ready-to-paused");
      break;
    }
    case GST_MESSAGE_EOS:
      if (gve->priv->update_id > 0) {
        g_source_remove (gve->priv->update_id);
        gve->priv->update_id = 0;
      }
      gst_element_set_state (gve->priv->main_pipeline, GST_STATE_NULL);
      g_signal_emit (gve, gve_signals[SIGNAL_PERCENT_COMPLETED], 0, (gfloat) 1);
      gve->priv->active_segment = 0;
      /* Close file sink properly */
      g_object_set (G_OBJECT (gve->priv->file_sink), "location", "", NULL);
      break;
    default:
      GST_LOG ("Unhandled message: %" GST_PTR_FORMAT, message);
      break;
  }
}

static void
gve_error_msg (GstVideoEditor * gve, GstMessage * msg)
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
    g_signal_emit (gve, gve_signals[SIGNAL_ERROR], 0, err->message);
    g_error_free (err);
  }
  g_free (dbg);
}

static gboolean
gve_query_timeout (GstVideoEditor * gve)
{
  GstFormat fmt = GST_FORMAT_TIME;
  gint64 pos = -1;
  gchar *title;
  gint64 stop_time = gve->priv->stop_times[gve->priv->active_segment];

  if (gst_element_query_position (gve->priv->main_pipeline, &fmt, &pos)) {
    if (pos != -1 && fmt == GST_FORMAT_TIME) {
      g_signal_emit (gve,
          gve_signals[SIGNAL_PERCENT_COMPLETED],
          0, (float) pos / (float) gve->priv->duration);
    }
  } else {
    GST_INFO ("could not get position");
  }

  if (gst_element_query_position (gve->priv->video_encoder, &fmt, &pos)) {
    if (stop_time - pos <= 0) {
      gve->priv->active_segment++;
      title =
          (gchar *) g_list_nth_data (gve->priv->titles,
          gve->priv->active_segment);
      g_object_set (G_OBJECT (gve->priv->textoverlay), "text", title, NULL);
    }
  }

  return TRUE;
}

/* =========================================== */
/*                                             */
/*              Public Methods                 */
/*                                             */
/* =========================================== */


void
gst_video_editor_add_segment (GstVideoEditor * gve, gchar * file,
    gint64 start, gint64 duration, gdouble rate,
    gchar * title, gboolean hasAudio)
{
  GstState cur_state;
  GstElement *gnl_filesource = NULL;
  GstElement *audiotestsrc = NULL;
  GstCaps *filter = NULL;
  gchar *element_name = "";
  gint64 final_duration;

  g_return_if_fail (GST_IS_VIDEO_EDITOR (gve));

  gst_element_get_state (gve->priv->main_pipeline, &cur_state, NULL, 0);
  if (cur_state > GST_STATE_READY) {
    GST_WARNING ("Segments can only be added for a state <= GST_STATE_READY");
    return;
  }

  start = GST_MSECOND * start;
  duration = GST_MSECOND * duration;
  final_duration = duration / rate;

  /* Video */
  filter = gst_caps_from_string ("video/x-raw-rgb;video/x-raw-yuv");
  element_name = g_strdup_printf ("gnlvideofilesource%d", gve->priv->segments);
  gnl_filesource = gst_element_factory_make ("gnlfilesource", element_name);
  g_object_set (G_OBJECT (gnl_filesource), "location", file,
      "media-start", start,
      "media-duration", duration,
      "start", gve->priv->duration,
      "duration", final_duration, "caps", filter, NULL);
  if (gve->priv->segments == 0) {
    g_object_set (G_OBJECT (gve->priv->textoverlay), "text", title, NULL);
  }
  gst_bin_add (GST_BIN (gve->priv->gnl_video_composition), gnl_filesource);
  gve->priv->gnl_video_filesources =
      g_list_append (gve->priv->gnl_video_filesources, gnl_filesource);

  /* Audio */
  if (hasAudio && rate == 1) {
    element_name =
        g_strdup_printf ("gnlaudiofilesource%d", gve->priv->segments);
    gnl_filesource = gst_element_factory_make ("gnlfilesource", element_name);
    g_object_set (G_OBJECT (gnl_filesource), "location", file, NULL);
  } else {
    /* If the file doesn't contain audio, something must be playing */
    /* We use an audiotestsrc mutted and with a low priority */
    element_name =
        g_strdup_printf ("gnlaudiofakesource%d", gve->priv->segments);
    gnl_filesource = gst_element_factory_make ("gnlsource", element_name);
    element_name = g_strdup_printf ("audiotestsource%d", gve->priv->segments);
    audiotestsrc = gst_element_factory_make ("audiotestsrc", element_name);
    g_object_set (G_OBJECT (audiotestsrc), "volume", (double) 0, NULL);
    gst_bin_add (GST_BIN (gnl_filesource), audiotestsrc);
  }
  filter = gst_caps_from_string ("audio/x-raw-float;audio/x-raw-int");
  g_object_set (G_OBJECT (gnl_filesource),
      "media-start", start,
      "media-duration", duration,
      "start", gve->priv->duration,
      "duration", final_duration, "caps", filter, NULL);
  gst_bin_add (GST_BIN (gve->priv->gnl_audio_composition), gnl_filesource);
  gve->priv->gnl_audio_filesources =
      g_list_append (gve->priv->gnl_audio_filesources, gnl_filesource);

  GST_INFO ("New segment: start={%" GST_TIME_FORMAT "} duration={%"
      GST_TIME_FORMAT "} ", GST_TIME_ARGS (gve->priv->duration),
      GST_TIME_ARGS (final_duration));

  gve->priv->duration += final_duration;
  gve->priv->segments++;

  gve->priv->titles = g_list_append (gve->priv->titles, title);
  gve->priv->stop_times[gve->priv->segments - 1] = gve->priv->duration;

  g_free (element_name);
}


void
gst_video_editor_add_image_segment (GstVideoEditor * gve, gchar * file,
    guint64 start, gint64 duration, gchar * title)
{
  GstState cur_state;
  GstElement *gnl_filesource = NULL;
  GstElement *imagesourcebin = NULL;
  GstElement *filesource = NULL;
  GstElement *decoder = NULL;
  GstElement *colorspace = NULL;
  GstElement *imagefreeze = NULL;
  GstElement *audiotestsrc = NULL;
  GstCaps *filter = NULL;
  gchar *element_name = NULL;
  gchar *desc = NULL;

  g_return_if_fail (GST_IS_VIDEO_EDITOR (gve));

  gst_element_get_state (gve->priv->main_pipeline, &cur_state, NULL, 0);
  if (cur_state > GST_STATE_READY) {
    GST_WARNING ("Segments can only be added for a state <= GST_STATE_READY");
    return;
  }

  duration = duration * GST_MSECOND;
  start = start * GST_MSECOND;

  /* Video */
  /* gnlsource */
  filter = gst_caps_from_string ("video/x-raw-rgb;video/x-raw-yuv");
  element_name = g_strdup_printf ("gnlvideofilesource%d", gve->priv->segments);
  gnl_filesource = gst_element_factory_make ("gnlsource", element_name);
  g_object_set (G_OBJECT (gnl_filesource),
      "media-start", start,
      "media-duration", duration,
      "start", gve->priv->duration,
      "duration", duration, "caps", filter, NULL);
  g_free(element_name);
  /* filesrc ! pngdec ! ffmpegcolorspace ! imagefreeze */
  desc = g_strdup_printf("filesrc location=%s ! pngdec ! videoscale ! ffmpegcolorspace ! video/x-raw-rgb, pixel-aspect-ratio=1/1 ! imagefreeze  ", file);
  imagesourcebin = gst_parse_bin_from_description(desc, TRUE, NULL);
  g_free(desc);
  gst_bin_add (GST_BIN (gnl_filesource), imagesourcebin);
  gst_bin_add (GST_BIN (gve->priv->gnl_video_composition), gnl_filesource);
  gve->priv->gnl_video_filesources =
      g_list_append (gve->priv->gnl_video_filesources, gnl_filesource);

  /* Audio */
  element_name =
      g_strdup_printf ("gnlaudiofakesource%d", gve->priv->segments);
  gnl_filesource = gst_element_factory_make ("gnlsource", element_name);
  g_free (element_name);
  element_name = g_strdup_printf ("audiotestsource%d", gve->priv->segments);
  audiotestsrc = gst_element_factory_make ("audiotestsrc", element_name);
  g_free (element_name);
  g_object_set (G_OBJECT (audiotestsrc), "volume", (double) 0, NULL);
  gst_bin_add (GST_BIN (gnl_filesource), audiotestsrc);
  filter = gst_caps_from_string ("audio/x-raw-float;audio/x-raw-int");
  g_object_set (G_OBJECT (gnl_filesource),
      "media-start", start,
      "media-duration", duration,
      "start", gve->priv->duration,
      "duration", duration, "caps", filter, NULL);
  gst_bin_add (GST_BIN (gve->priv->gnl_audio_composition), gnl_filesource);
  gve->priv->gnl_audio_filesources =
      g_list_append (gve->priv->gnl_audio_filesources, gnl_filesource);

  GST_INFO ("New segment: start={%" GST_TIME_FORMAT "} duration={%"
      GST_TIME_FORMAT "} ", GST_TIME_ARGS (gve->priv->duration),
      GST_TIME_ARGS (duration));

  gve->priv->duration += duration;
  gve->priv->segments++;

  gve->priv->titles = g_list_append (gve->priv->titles, title);
  gve->priv->stop_times[gve->priv->segments - 1] = gve->priv->duration;

}

void
gst_video_editor_clear_segments_list (GstVideoEditor * gve)
{
  GList *tmp = NULL;

  g_return_if_fail (GST_IS_VIDEO_EDITOR (gve));

  GST_INFO_OBJECT (gve, "Clearing list of segments");

  gst_video_editor_cancel (gve);

  tmp = gve->priv->gnl_video_filesources;

  for (; tmp; tmp = g_list_next (tmp)) {
    GstElement *object = (GstElement *) tmp->data;
    if (object)
      gst_element_set_state (object, GST_STATE_NULL);
    gst_bin_remove (GST_BIN (gve->priv->gnl_video_composition), object);
  }

  tmp = gve->priv->gnl_audio_filesources;

  for (; tmp; tmp = g_list_next (tmp)) {
    GstElement *object = (GstElement *) tmp->data;
    if (object)
      gst_element_set_state (object, GST_STATE_NULL);
    gst_bin_remove (GST_BIN (gve->priv->gnl_audio_composition), object);
  }

  g_list_free (tmp);
  g_list_free (gve->priv->gnl_video_filesources);
  g_list_free (gve->priv->gnl_audio_filesources);
  g_free (gve->priv->stop_times);
  g_list_free (gve->priv->titles);

  gve->priv->gnl_video_filesources = NULL;
  gve->priv->gnl_audio_filesources = NULL;
  gve->priv->stop_times = (gint64 *) malloc (200 * sizeof (gint64));
  gve->priv->titles = NULL;

  gve->priv->duration = 0;
  gve->priv->active_segment = 0;
}


void
gst_video_editor_set_video_encoder (GstVideoEditor * gve, gchar ** err,
    VideoEncoderType codec)
{
  GstElement *encoder = NULL;
  GstState cur_state;
  GstPad *srcpad;
  GstPad *oldsrcpad;
  gchar *encoder_name = "";
  gchar *error;

  g_return_if_fail (GST_IS_VIDEO_EDITOR (gve));

  gst_element_get_state (gve->priv->main_pipeline, &cur_state, NULL, 0);

  if (cur_state > GST_STATE_READY)
    goto wrong_state;

  switch (codec) {
    case VIDEO_ENCODER_H264:
      encoder_name = "x264enc";
      encoder = gst_element_factory_make (encoder_name, encoder_name);
      g_object_set (G_OBJECT (encoder), "pass", 17, NULL);       //Variable Bitrate-Pass 1
      g_object_set (G_OBJECT (encoder), "speed-preset", 4, NULL);//"Faster" preset
      break;
    case VIDEO_ENCODER_MPEG4:
      encoder_name = "xvidenc";
      encoder = gst_element_factory_make (encoder_name, encoder_name);
      g_object_set (G_OBJECT (encoder), "pass", 1, NULL);       //Variable Bitrate-Pass 1
      break;
    case VIDEO_ENCODER_XVID:
      encoder_name = "ffenc_mpeg4";
      encoder = gst_element_factory_make (encoder_name, encoder_name);
      g_object_set (G_OBJECT (encoder), "pass", 512, NULL);     //Variable Bitrate-Pass 1
      break;
    case VIDEO_ENCODER_MPEG2:
      encoder_name = "mpeg2enc";
      encoder = gst_element_factory_make (encoder_name, encoder_name);
      g_object_set (G_OBJECT (encoder), "format", 9, NULL);     //DVD compilant
      g_object_set (G_OBJECT (encoder), "framerate", 3, NULL);  //25 FPS (PAL/SECAM)    
      break;
    case VIDEO_ENCODER_THEORA:
      encoder_name = "theoraenc";
      encoder = gst_element_factory_make (encoder_name, encoder_name);
      break;
    case VIDEO_ENCODER_VP8:
      encoder_name = "vp8enc";
      encoder = gst_element_factory_make (encoder_name, encoder_name);
      g_object_set (G_OBJECT (encoder), "speed", 1, NULL);
      g_object_set (G_OBJECT (encoder), "threads", 4, NULL);
      break;
  }

  if (!encoder)
    goto no_encoder;

  GST_INFO_OBJECT(gve, "Changing video encoder: %s", encoder_name);

  if (!g_strcmp0
      (gst_element_get_name (gve->priv->video_encoder), encoder_name))
    goto same_encoder;

  gve->priv->video_encoder_type = codec;

  /*Remove old encoder element */
  gst_element_unlink (gve->priv->queue, gve->priv->video_encoder);
  gst_element_unlink (gve->priv->vencode_bin, gve->priv->muxer);
  gst_element_set_state (gve->priv->video_encoder, GST_STATE_NULL);
  gst_bin_remove (GST_BIN (gve->priv->vencode_bin), gve->priv->video_encoder);

  /*Add new encoder element */
  gve->priv->video_encoder = encoder;
  if (codec == VIDEO_ENCODER_THEORA || codec == VIDEO_ENCODER_H264)
    g_object_set (G_OBJECT (gve->priv->video_encoder), "bitrate",
        gve->priv->video_bitrate, NULL);
  else
    g_object_set (G_OBJECT (gve->priv->video_encoder), "bitrate",
        gve->priv->video_bitrate * 1000, NULL);
  
  /*Add first to the encoder bin */
  gst_bin_add (GST_BIN (gve->priv->vencode_bin), gve->priv->video_encoder);
  gst_element_link (gve->priv->queue, gve->priv->video_encoder);
  /*Remove old encoder bin's src pad */
  oldsrcpad = gst_element_get_static_pad (gve->priv->vencode_bin, "src");
  gst_pad_set_active (oldsrcpad, FALSE);
  gst_element_remove_pad (gve->priv->vencode_bin, oldsrcpad);
  /*Create new encoder bin's src pad */
  srcpad = gst_element_get_static_pad (gve->priv->video_encoder, "src");
  gst_pad_set_active (srcpad, TRUE);
  gst_element_add_pad (gve->priv->vencode_bin,
      gst_ghost_pad_new ("src", srcpad));
  gst_element_link (gve->priv->vencode_bin, gve->priv->muxer);

  gve_rewrite_headers (gve);
  return;

wrong_state:
  {
    GST_WARNING
        ("The video encoder cannot be changed for a state <= GST_STATE_READY");
    return;
  }
no_encoder:
  {
    error =
        g_strdup_printf
        ("The %s encoder element is not avalaible. Check your GStreamer installation",
        encoder_name);
    GST_ERROR (error);
    *err = g_strdup (error);
    g_free (error);
    return;
  }
same_encoder:
  {
    GST_WARNING
        ("The video encoder is not changed because it is already in use.");
    gst_object_unref (encoder);
    return;
  }
}

void
gst_video_editor_set_audio_encoder (GstVideoEditor * gve, gchar ** err,
    AudioEncoderType codec)
{
  GstElement *encoder = NULL;
  GstState cur_state;
  GstPad *srcpad;
  GstPad *oldsrcpad;
  gchar *encoder_name = "";
  gchar *error;

  g_return_if_fail (GST_IS_VIDEO_EDITOR (gve));

  gst_element_get_state (gve->priv->main_pipeline, &cur_state, NULL, 0);

  if (cur_state > GST_STATE_READY)
    goto wrong_state;

  switch (codec) {
    case AUDIO_ENCODER_AAC:
      encoder_name = "faac";
      encoder = gst_element_factory_make (encoder_name, encoder_name);
      g_object_set (G_OBJECT (gve->priv->audiocapsfilter), "caps",
          gst_caps_from_string (FAAC_CAPS), NULL);
      break;
    case AUDIO_ENCODER_MP3:
      encoder_name = "lame";
      encoder = gst_element_factory_make (encoder_name, encoder_name);
      g_object_set (G_OBJECT (encoder), "vbr", 4, NULL);        //Variable Bitrate
      g_object_set (G_OBJECT (gve->priv->audiocapsfilter), "caps",
          gst_caps_from_string (LAME_CAPS), NULL);
      break;
    case AUDIO_ENCODER_VORBIS:
      encoder_name = "vorbisenc";
      encoder = gst_element_factory_make (encoder_name, encoder_name);
      g_object_set (G_OBJECT (gve->priv->audiocapsfilter), "caps",
          gst_caps_from_string (VORBIS_CAPS), NULL);
      break;
    default:
      gst_video_editor_set_enable_audio (gve, FALSE);
      break;
  }
  if (!encoder)
    goto no_encoder;

  GST_INFO_OBJECT(gve, "Changing audio encoder: %s", encoder_name);

  if (!g_strcmp0 (gst_element_get_name (gve->priv->audioencoder), encoder_name))
    goto same_encoder;

  /*Remove old encoder element */
  gst_element_unlink (gve->priv->audioqueue, gve->priv->audioencoder);
  if (gve->priv->audio_enabled)
    gst_element_unlink (gve->priv->aencode_bin, gve->priv->muxer);
  gst_element_set_state (gve->priv->audioencoder, GST_STATE_NULL);
  gst_bin_remove (GST_BIN (gve->priv->aencode_bin), gve->priv->audioencoder);

  /*Add new encoder element */
  gve->priv->audioencoder = encoder;
  if (codec == AUDIO_ENCODER_MP3)
    g_object_set (G_OBJECT (gve->priv->audioencoder), "bitrate",
        gve->priv->audio_bitrate / 1000, NULL);
  else
    g_object_set (G_OBJECT (gve->priv->audioencoder), "bitrate",
        gve->priv->audio_bitrate, NULL);
  /*Add first to the encoder bin */
  gst_bin_add (GST_BIN (gve->priv->aencode_bin), gve->priv->audioencoder);
  gst_element_link (gve->priv->audioqueue, gve->priv->audioencoder);
  /*Remove old encoder bin's src pad */
  oldsrcpad = gst_element_get_static_pad (gve->priv->aencode_bin, "src");
  gst_pad_set_active (oldsrcpad, FALSE);
  gst_element_remove_pad (gve->priv->aencode_bin, oldsrcpad);
  /*Create new encoder bin's src pad */
  srcpad = gst_element_get_static_pad (gve->priv->audioencoder, "src");
  gst_pad_set_active (srcpad, TRUE);
  gst_element_add_pad (gve->priv->aencode_bin,
      gst_ghost_pad_new ("src", srcpad));
  if (gve->priv->audio_enabled)
    gst_element_link (gve->priv->aencode_bin, gve->priv->muxer);
  gve_rewrite_headers (gve);
  return;

wrong_state:
  {
    GST_WARNING
        ("The audio encoder cannot be changed for a state <= GST_STATE_READY");
    return;
  }
no_encoder:
  {
    error =
        g_strdup_printf
        ("The %s encoder element is not avalaible. Check your GStreamer installation",
        encoder_name);
    GST_ERROR (error);
    *err = g_strdup (error);
    g_free (error);
    return;
  }
same_encoder:
  {
    GST_WARNING
        ("The audio encoder is not changed because it is already in use.");
    gst_object_unref (encoder);
    return;
  }
}

void
gst_video_editor_set_video_muxer (GstVideoEditor * gve, gchar ** err,
    VideoMuxerType muxerType)
{
  GstElement *muxer = NULL;
  GstState cur_state;
  gchar *muxer_name = "";
  gchar *error;

  g_return_if_fail (GST_IS_VIDEO_EDITOR (gve));

  gst_element_get_state (gve->priv->main_pipeline, &cur_state, NULL, 0);

  if (cur_state > GST_STATE_READY)
    goto wrong_state;

  switch (muxerType) {
    case VIDEO_MUXER_MATROSKA:
      muxer_name = "matroskamux";
      muxer = gst_element_factory_make ("matroskamux", muxer_name);
      break;
    case VIDEO_MUXER_AVI:
      muxer_name = "avimux";
      muxer = gst_element_factory_make ("avimux", muxer_name);
      break;
    case VIDEO_MUXER_OGG:
      muxer_name = "oggmux";
      muxer = gst_element_factory_make ("oggmux", muxer_name);
      break;
    case VIDEO_MUXER_MP4:
      muxer_name = "qtmux";
      muxer = gst_element_factory_make ("qtmux", muxer_name);
      break;
    case VIDEO_MUXER_MPEG_PS:
      muxer_name = "ffmux_dvd";
      //We don't want to mux anything yet as ffmux_dvd is buggy
      //FIXME: Until we don't have audio save the mpeg-ps stream without mux.
      muxer = gst_element_factory_make ("ffmux_dvd", muxer_name);
      break;
    case VIDEO_MUXER_WEBM:
      muxer_name = "webmmux";
      muxer = gst_element_factory_make ("webmmux", muxer_name);
      break;
  }

  if (!muxer)
    goto no_muxer;

  GST_INFO_OBJECT(gve, "Changing muxer: %s", muxer_name);

  if (!g_strcmp0 (gst_element_get_name (gve->priv->muxer), muxer_name))
    goto same_muxer;

  gst_element_unlink (gve->priv->vencode_bin, gve->priv->muxer);
  if (gve->priv->audio_enabled)
    gst_element_unlink (gve->priv->aencode_bin, gve->priv->muxer);
  gst_element_unlink (gve->priv->muxer, gve->priv->file_sink);
  gst_element_set_state (gve->priv->muxer, GST_STATE_NULL);
  gst_bin_remove (GST_BIN (gve->priv->main_pipeline), gve->priv->muxer);
  gst_bin_add (GST_BIN (gve->priv->main_pipeline), muxer);
  gst_element_link_many (gve->priv->vencode_bin, muxer,
      gve->priv->file_sink, NULL);
  if (gve->priv->audio_enabled)
    gst_element_link (gve->priv->aencode_bin, muxer);
  gve->priv->muxer = muxer;
  gve_rewrite_headers (gve);
  return;

wrong_state:
  {
    GST_WARNING
        ("The video muxer cannot be changed for a state <= GST_STATE_READY");
    return;
  }
no_muxer:
  {
    error =
        g_strdup_printf
        ("The %s muxer element is not avalaible. Check your GStreamer installation",
        muxer_name);
    GST_ERROR (error);
    *err = g_strdup (error);
    g_free (error);
    return;
  }
same_muxer:
  {
    GST_WARNING
        ("Not changing the video muxer as the new one is the same in use.");
    gst_object_unref (muxer);
    return;
  }
}

void
gst_video_editor_start (GstVideoEditor * gve)
{
  g_return_if_fail (GST_IS_VIDEO_EDITOR (gve));

  GST_INFO_OBJECT(gve, "Starting");
  gst_element_set_state (gve->priv->main_pipeline, GST_STATE_PLAYING);
  g_signal_emit (gve, gve_signals[SIGNAL_PERCENT_COMPLETED], 0, (gfloat) 0);
}

void
gst_video_editor_cancel (GstVideoEditor * gve)
{
  g_return_if_fail (GST_IS_VIDEO_EDITOR (gve));

  GST_INFO_OBJECT(gve, "Cancelling");
  if (gve->priv->update_id > 0) {
    g_source_remove (gve->priv->update_id);
    gve->priv->update_id = 0;
  }
  gst_element_set_state (gve->priv->main_pipeline, GST_STATE_NULL);
  g_signal_emit (gve, gve_signals[SIGNAL_PERCENT_COMPLETED], 0, (gfloat) - 1);
}

void
gst_video_editor_init_backend (int *argc, char ***argv)
{
  gst_init (argc, argv);
}

GstVideoEditor *
gst_video_editor_new (GError ** err)
{
  GstVideoEditor *gve = NULL;

  gve = g_object_new (GST_TYPE_VIDEO_EDITOR, NULL);

  gve->priv->main_pipeline = gst_pipeline_new ("main_pipeline");

  if (!gve->priv->main_pipeline) {
    g_set_error (err, GVC_ERROR, GST_ERROR_PLUGIN_LOAD,
        ("Failed to create a GStreamer Bin. "
            "Please check your GStreamer installation."));
    g_object_ref_sink (gve);
    g_object_unref (gve);
    return NULL;
  }

  /* Create elements */
  gve->priv->gnl_video_composition =
      gst_element_factory_make ("gnlcomposition", "gnl-video-composition");
  gve->priv->gnl_audio_composition =
      gst_element_factory_make ("gnlcomposition", "gnl-audio-composition");
  if (!gve->priv->gnl_video_composition || !gve->priv->gnl_audio_composition) {
    g_set_error (err, GVC_ERROR, GST_ERROR_PLUGIN_LOAD,
        ("Failed to create a Gnonlin element. "
            "Please check your GStreamer installation."));
    g_object_ref_sink (gve);
    g_object_unref (gve);
    return NULL;
  }

  gve->priv->muxer =
      gst_element_factory_make (DEFAULT_VIDEO_MUXER, "videomuxer");
  gve->priv->file_sink = gst_element_factory_make ("filesink", "filesink");
  gve_create_video_encode_bin (gve);
  gve_create_audio_encode_bin (gve);

  /* Set elements properties */
  g_object_set (G_OBJECT (gve->priv->file_sink), "location",
      gve->priv->output_file, NULL);

  /* Link elements */
  gst_bin_add_many (GST_BIN (gve->priv->main_pipeline),
      gve->priv->gnl_video_composition,
      gve->priv->gnl_audio_composition,
      gve->priv->vencode_bin,
      gve->priv->aencode_bin, gve->priv->muxer, gve->priv->file_sink, NULL);

  gst_element_link_many (gve->priv->vencode_bin,
      gve->priv->muxer, gve->priv->file_sink, NULL);
  gst_element_link (gve->priv->aencode_bin, gve->priv->muxer);

  /*Connect bus signals */
  /*Wait for a "new-decoded-pad" message to link the composition with
     the encoder tail */
  gve->priv->bus = gst_element_get_bus (GST_ELEMENT (gve->priv->main_pipeline));
  g_signal_connect (gve->priv->gnl_video_composition, "pad-added",
      G_CALLBACK (new_decoded_pad_cb), gve);
  g_signal_connect (gve->priv->gnl_audio_composition, "pad-added",
      G_CALLBACK (new_decoded_pad_cb), gve);

  gst_bus_add_signal_watch (gve->priv->bus);
  gve->priv->sig_bus_async = g_signal_connect (gve->priv->bus, "message",
      G_CALLBACK (gve_bus_message_cb), gve);

  gst_element_set_state (gve->priv->main_pipeline, GST_STATE_READY);

  return gve;
}
