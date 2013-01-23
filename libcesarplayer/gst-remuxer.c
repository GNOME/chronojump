/* -*- Mode: C; indent-tabs-mode: t; c-basic-offset: 4; tab-width: 4 -*- */
/*
* Gstreamer Remuxer
* Copyright (C)  Andoni Morales Alastruey 2012 <ylatuya@gmail.com>
*
* LongoMatch is free software.
*
* You may redistribute it and/or modify it under the terms of the
* GNU General Public License, as published by the Free Software
* Foundation; either version 2 of the License, or (at your option)
* any later version.
*
* LongoMatch is distributed in the hope that it will be useful,
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

#include <gst/gst.h>

#include "gst-remuxer.h"

GST_DEBUG_CATEGORY (_remuxer_gst_debug_cat);
#define GST_CAT_DEFAULT _remuxer_gst_debug_cat

/* Signals */
enum
{
  SIGNAL_ERROR,
  SIGNAL_PERCENT,
  LAST_SIGNAL
};

struct GstRemuxerPrivate
{

  /*Encoding properties */
  gchar *input_file;
  gchar *output_file;
  VideoMuxerType video_muxer_type;

  /* Remuxer */
  GstClockTime last_video_buf_ts;
  GstClockTime last_audio_buf_ts;
  gboolean audio_linked;
  gboolean video_linked;

  /*GStreamer elements */
  GstElement *main_pipeline;

  /*GStreamer bus */
  GstBus *bus;
  gulong sig_bus_async;
};

static GObject *parent_class = NULL;

static int remuxer_signals[LAST_SIGNAL] = { 0 };

static void remuxer_error_msg (GstRemuxer * remuxer, GstMessage * msg);
static void remuxer_bus_message_cb (GstBus * bus, GstMessage * message,
    gpointer data);

G_DEFINE_TYPE (GstRemuxer, gst_remuxer, G_TYPE_OBJECT);

/***********************************
*
*     Class, Object and Properties
*
************************************/

static void
gst_remuxer_init (GstRemuxer * object)
{
  GstRemuxerPrivate *priv;
  object->priv = priv =
      G_TYPE_INSTANCE_GET_PRIVATE (object, GST_TYPE_REMUXER,
      GstRemuxerPrivate);

  priv->input_file = NULL;
  priv->output_file = NULL;
  priv->last_video_buf_ts = GST_CLOCK_TIME_NONE;
  priv->last_audio_buf_ts = GST_CLOCK_TIME_NONE;
  priv->video_muxer_type = VIDEO_MUXER_WEBM;
  priv->audio_linked = FALSE;
  priv->video_linked = FALSE;
}

void
gst_remuxer_finalize (GObject * object)
{
  GstRemuxer *remuxer = (GstRemuxer *) object;

  GST_DEBUG_OBJECT (remuxer, "Finalizing.");
  if (remuxer->priv->bus) {
    /* make bus drop all messages to make sure none of our callbacks is ever
     * called again (main loop might be run again to display error dialog) */
    gst_bus_set_flushing (remuxer->priv->bus, TRUE);

    if (remuxer->priv->sig_bus_async)
      g_signal_handler_disconnect (remuxer->priv->bus, remuxer->priv->sig_bus_async);

    gst_object_unref (remuxer->priv->bus);
    remuxer->priv->bus = NULL;
  }

  if (remuxer->priv->input_file) {
    g_free (remuxer->priv->input_file);
    remuxer->priv->input_file = NULL;
  }

  if (remuxer->priv->output_file) {
    g_free (remuxer->priv->output_file);
    remuxer->priv->output_file = NULL;
  }

  if (remuxer->priv->main_pipeline != NULL
      && GST_IS_ELEMENT (remuxer->priv->main_pipeline)) {
    gst_element_set_state (remuxer->priv->main_pipeline,
        GST_STATE_NULL);
    gst_object_unref (remuxer->priv->main_pipeline);
    remuxer->priv->main_pipeline = NULL;
  }

  G_OBJECT_CLASS (parent_class)->finalize (object);
}

static void
gst_remuxer_class_init (GstRemuxerClass * klass)
{
  GObjectClass *object_class;

  object_class = (GObjectClass *) klass;
  parent_class = g_type_class_peek_parent (klass);

  g_type_class_add_private (object_class, sizeof (GstRemuxerPrivate));

  object_class->finalize = gst_remuxer_finalize;

  /* Signals */
  remuxer_signals[SIGNAL_ERROR] =
      g_signal_new ("error",
        G_TYPE_FROM_CLASS (object_class),
        G_SIGNAL_RUN_LAST,
        G_STRUCT_OFFSET (GstRemuxerClass, error),
        NULL, NULL,
        g_cclosure_marshal_VOID__STRING, G_TYPE_NONE, 1, G_TYPE_STRING);

  remuxer_signals[SIGNAL_PERCENT] =
      g_signal_new ("percent_completed",
        G_TYPE_FROM_CLASS (object_class),
        G_SIGNAL_RUN_LAST,
        G_STRUCT_OFFSET (GstRemuxerClass, percent_completed),
        NULL, NULL, g_cclosure_marshal_VOID__FLOAT, G_TYPE_NONE, 1, G_TYPE_FLOAT);
}

/***********************************
*
*           GStreamer
*
************************************/

void
gst_remuxer_init_backend (int *argc, char ***argv)
{
  gst_init (argc, argv);
}

GQuark
gst_remuxer_error_quark (void)
{
  static GQuark q;              /* 0 */

  if (G_UNLIKELY (q == 0)) {
    q = g_quark_from_static_string ("remuxer-error-quark");
  }
  return q;
}

static GstElement *
gst_remuxer_create_video_muxer (GstRemuxer * remuxer,
    VideoMuxerType type)
{
  GstElement *muxer;

  g_return_val_if_fail (remuxer != NULL, FALSE);
  g_return_val_if_fail (GST_IS_REMUXER (remuxer), FALSE);

  switch (type) {
    case VIDEO_MUXER_OGG:
      muxer = gst_element_factory_make ("oggmux", "muxer");
      break;
    case VIDEO_MUXER_AVI:
      muxer = gst_element_factory_make ("avimux", "muxer");
      break;
    case VIDEO_MUXER_MATROSKA:
      muxer = gst_element_factory_make ("matroskamux", "muxer");
      break;
    case VIDEO_MUXER_MP4:
      muxer = gst_element_factory_make ("qtmux", "muxer");
      break;
    case VIDEO_MUXER_WEBM:
    default:
      muxer = gst_element_factory_make ("webmmux", "muxer");
      break;
  }

  return muxer;
}

static gboolean
gst_remuxer_fix_video_ts (GstPad *pad, GstBuffer *buf, GstRemuxer *remuxer)
{
  if (GST_BUFFER_TIMESTAMP (buf) == GST_CLOCK_TIME_NONE) {
    GST_BUFFER_TIMESTAMP (buf) = remuxer->priv->last_video_buf_ts;
  } else {
    remuxer->priv->last_video_buf_ts = GST_BUFFER_TIMESTAMP (buf);
  }
  return TRUE;
}

static gboolean
gst_remuxer_pad_added_cb (GstElement *demuxer, GstPad *pad,
    GstRemuxer *remuxer)
{
  GstElement *muxer, *queue;
  GstElement *parser = NULL;
  GstPad *muxer_pad, *queue_sink_pad, *queue_src_pad;
  GstCaps *caps, *parser_caps = NULL;
  const GstStructure *s;
  gchar *muxer_pad_name = NULL;
  const gchar *mime;
  gboolean is_video;

  caps = gst_pad_get_caps_reffed (pad);
  s = gst_caps_get_structure (caps, 0);
  mime = gst_structure_get_name (s);
  GST_INFO_OBJECT (remuxer, "Found mime type: %s", mime);

  if (g_strrstr (mime, "video") && !remuxer->priv->video_linked) {
    muxer_pad_name = "video_%d";
    if (g_strrstr (mime, "video/x-h264")) {
      GstPad *parser_pad;

      parser = gst_element_factory_make("h264parse", "video-parser");
      parser_caps = gst_caps_from_string("video/x-h264, stream-format=avc, alignment=au");

      parser_pad = gst_element_get_static_pad (parser, "src");
      gst_pad_add_buffer_probe (parser_pad, (GCallback)gst_remuxer_fix_video_ts, remuxer);
    }
    is_video = TRUE;
  } else if (g_strrstr (mime, "audio") && !remuxer->priv->audio_linked) {
    muxer_pad_name = "audio_%d";
    if (g_strrstr (mime, "audio/mpeg")) {
      gint version;
      gst_structure_get_int (s, "mpegversion", &version);
      if (version == 4) {
        /* FIXME: aacparse doesn't seem to support adts to raw conversion */
        //parser = gst_element_factory_make ("aacparse", NULL);
        parser = gst_parse_bin_from_description ("faad ! faac", TRUE, NULL);
      } else if (version == 3) {
        parser = gst_element_factory_make("mp3parse", "audio-parser");
      } else {
        parser = gst_element_factory_make("mpegaudioparse", "audio-parser");
      }
    } else if (g_strrstr (mime, "audio/x-eac3")) {
      parser = gst_element_factory_make ("ac3parse", NULL);
    } else if (g_strrstr (mime, "audio/x-ac3")) {
      parser = gst_element_factory_make ("ac3parse", NULL);
    }
    is_video = FALSE;
  }

  if (muxer_pad_name == NULL) {
    gst_caps_unref (caps);
    return TRUE;
  }

  muxer = gst_bin_get_by_name (GST_BIN(remuxer->priv->main_pipeline), "muxer");
  if (parser != NULL) {
    gst_bin_add (GST_BIN(remuxer->priv->main_pipeline), parser);
    gst_element_set_state (parser, GST_STATE_PLAYING);
    if (parser_caps) {
      gst_element_link_filtered (parser, muxer, parser_caps);
      gst_caps_unref (parser_caps);
    } else {
      gst_element_link (parser, muxer);
    }
    muxer_pad = gst_element_get_static_pad (parser, "sink");
  } else {
    muxer_pad = gst_element_get_compatible_pad (muxer, pad, caps);
  }

  if (!muxer_pad) {
    gchar *msg;

    msg = g_strdup_printf ("File with %s type %s is not supported",
        is_video ? "video" : "audio", mime);
    g_signal_emit (remuxer, remuxer_signals[SIGNAL_ERROR], 0, msg);
    g_free (msg);
    gst_remuxer_cancel (remuxer);
    return TRUE;
  } else {
    if (is_video)
      remuxer->priv->video_linked = TRUE;
    else
      remuxer->priv->audio_linked = TRUE;
  }
  queue = gst_element_factory_make ("queue2", NULL);
  gst_bin_add (GST_BIN(remuxer->priv->main_pipeline), queue);
  gst_element_set_state (queue, GST_STATE_PLAYING);
  queue_sink_pad = gst_element_get_static_pad (queue, "sink");
  queue_src_pad = gst_element_get_static_pad (queue, "src");

  gst_pad_link (pad, queue_sink_pad);
  gst_pad_link (queue_src_pad, muxer_pad);

  gst_object_unref (muxer);
  gst_object_unref (queue_sink_pad);
  gst_object_unref (queue_src_pad);

  gst_caps_unref (caps);
  return TRUE;
}

static gboolean
gst_remuxer_have_type_cb (GstElement *typefind, guint prob,
    GstCaps *caps, GstRemuxer *remuxer)
{
  GstElement *demuxer = NULL;
  GstElement *parser = NULL;
  GstStructure *structure;
  const gchar *mime;

  structure = gst_caps_get_structure (caps, 0);
  mime = gst_structure_get_name (structure);
  GST_INFO_OBJECT (remuxer, "Found mime type: %s", mime);

  if (g_strrstr (mime, "video/mpegts")) {
    GST_INFO_OBJECT (remuxer, "Using tsdemux as demuxer");
    demuxer = gst_element_factory_make ("tsdemux", NULL);
  } else if (g_strrstr (mime, "video/x-ms-asf")) {
    GST_INFO_OBJECT (remuxer, "Using asfdemux as demuxer");
    demuxer = gst_element_factory_make ("asfdemux", NULL);
  } else if (g_strrstr (mime, "video/mpeg")) {
    gboolean sysstream;

    if (gst_structure_get_boolean (structure, "systemstream", &sysstream) &&
        !sysstream) {
      GST_INFO_OBJECT (remuxer, "Using mpegvideoparse as demuxer");
      parser = gst_element_factory_make ("mpegvideoparse", NULL);
    } else {
      GST_INFO_OBJECT (remuxer, "Using mpegpsdemux as demuxer");
      demuxer = gst_element_factory_make ("mpegpsdemux", NULL);
    }
  }

  if (demuxer) {
    gst_bin_add (GST_BIN(remuxer->priv->main_pipeline), demuxer);
    gst_element_link (typefind, demuxer);
    g_signal_connect (demuxer, "pad-added",
        G_CALLBACK (gst_remuxer_pad_added_cb), remuxer);
  } else if (parser) {
    GstPad *pad;
    gst_bin_add (GST_BIN(remuxer->priv->main_pipeline), parser);
    gst_element_link (typefind, parser);
    pad = gst_element_get_static_pad (parser, "src");
    gst_remuxer_pad_added_cb (parser, pad, remuxer);
  } else {
    gchar *msg;

    msg = g_strdup_printf ("File with mime type %s is not supported", mime);
    g_signal_emit (remuxer, remuxer_signals[SIGNAL_ERROR], 0, msg);
    g_free (msg);
    gst_remuxer_cancel (remuxer);
  }

  return TRUE;
}

static void
gst_remuxer_initialize (GstRemuxer *remuxer)
{
  GstElement *filesrc, *typefind, *muxer, *filesink;

  GST_INFO_OBJECT (remuxer, "Initializing pipeline");

  /* Create elements */
  remuxer->priv->main_pipeline = gst_pipeline_new ("pipeline");

  filesrc = gst_element_factory_make("filesrc", "source");
  typefind = gst_element_factory_make("typefind", "typefind");
  muxer = gst_remuxer_create_video_muxer (remuxer, remuxer->priv->video_muxer_type);
  filesink = gst_element_factory_make("filesink", "sink");

  /* Set properties */
  g_object_set (filesrc, "location", remuxer->priv->input_file, NULL);
  g_object_set (filesink, "location", remuxer->priv->output_file, NULL);

  /* Add elements to the bin */
  gst_bin_add_many(GST_BIN(remuxer->priv->main_pipeline), filesrc, typefind,
      muxer, filesink, NULL);
  gst_element_link(filesrc, typefind);
  gst_element_link(muxer, filesink);

  g_signal_connect (typefind, "have-type",
      G_CALLBACK (gst_remuxer_have_type_cb), remuxer);
}


static void
remuxer_bus_message_cb (GstBus * bus, GstMessage * message, gpointer data)
{
  GstRemuxer *remuxer = (GstRemuxer *) data;
  GstMessageType msg_type;

  g_return_if_fail (remuxer != NULL);
  g_return_if_fail (GST_IS_REMUXER (remuxer));

  msg_type = GST_MESSAGE_TYPE (message);

  switch (msg_type) {
    case GST_MESSAGE_ERROR:
    {
      if (remuxer->priv->main_pipeline) {
        gst_remuxer_cancel (remuxer);
      }
      remuxer_error_msg (remuxer, message);
      break;
    }

    case GST_MESSAGE_WARNING:
    {
      GST_WARNING ("Warning message: %" GST_PTR_FORMAT, message);
      break;
    }

    case GST_MESSAGE_EOS:
    {
      GST_INFO_OBJECT (remuxer, "EOS message");
      g_signal_emit (remuxer, remuxer_signals[SIGNAL_PERCENT], 0, (gfloat) 1);
      break;
    }

    default:
      GST_LOG ("Unhandled message: %" GST_PTR_FORMAT, message);
      break;
  }
}

static void
remuxer_error_msg (GstRemuxer * remuxer, GstMessage * msg)
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
    g_signal_emit (remuxer, remuxer_signals[SIGNAL_ERROR], 0, err->message);
    g_error_free (err);
  }
  g_free (dbg);
}

/*******************************************
 *
 *         Public methods
 *
 * ****************************************/

void
gst_remuxer_start (GstRemuxer * remuxer)
{
  g_return_if_fail (remuxer != NULL);
  g_return_if_fail (GST_IS_REMUXER (remuxer));

  gst_element_set_state (remuxer->priv->main_pipeline, GST_STATE_PLAYING);
}

void
gst_remuxer_cancel (GstRemuxer * remuxer)
{
  g_return_if_fail (remuxer != NULL);
  g_return_if_fail (GST_IS_REMUXER (remuxer));

  gst_element_set_state (remuxer->priv->main_pipeline, GST_STATE_NULL);
  gst_element_get_state (remuxer->priv->main_pipeline, NULL, NULL, -1);
}

GstRemuxer *
gst_remuxer_new (gchar * input_file, gchar *output_file, GError ** err)
{
  GstRemuxer *remuxer = NULL;

#ifndef GST_DISABLE_GST_INFO
  if (_remuxer_gst_debug_cat == NULL) {
    GST_DEBUG_CATEGORY_INIT (_remuxer_gst_debug_cat, "longomatch", 0,
        "LongoMatch GStreamer Backend");
  }
#endif

  remuxer = g_object_new (GST_TYPE_REMUXER, NULL);

  remuxer->priv->input_file = input_file;
  remuxer->priv->output_file = output_file;
  remuxer->priv->video_muxer_type = VIDEO_MUXER_MATROSKA;

  gst_remuxer_initialize (remuxer);

  /*Connect bus signals */
  GST_INFO_OBJECT (remuxer, "Connecting bus signals");
  remuxer->priv->bus = gst_element_get_bus (remuxer->priv->main_pipeline);
  gst_bus_add_signal_watch (remuxer->priv->bus);
  remuxer->priv->sig_bus_async =
      g_signal_connect (remuxer->priv->bus, "message",
      G_CALLBACK (remuxer_bus_message_cb), remuxer);

  return remuxer;
}
