/*
 * Gstreamer Video Editor
 * Copyright (C) 2007-2009 Andoni Morales Alastruey <ylatuya@gmail.com>
 * 
 * Gstreamer Video Editor is free software.
 * 
 * You may redistribute it and/or modify it under the terms of the
 * GNU General Public License, as published by the Free Software
 * Foundation; either version 2 of the License, or (at your option)
 * any later version.
 * 
 * foob is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with foob.  If not, write to:
 * 	The Free Software Foundation, Inc.,
 * 	51 Franklin Street, Fifth Floor
 * 	Boston, MA  02110-1301, USA.
 */

#ifndef _GST_VIDEO_EDITOR_H_
#define _GST_VIDEO_EDITOR_H_

#ifdef WIN32
#define EXPORT __declspec (dllexport)
#else
#define EXPORT
#endif

#include <glib-object.h>
#include <gtk/gtk.h>
#include <gdk/gdk.h>

#include "common.h"

G_BEGIN_DECLS
#define GST_TYPE_VIDEO_EDITOR             (gst_video_editor_get_type ())
#define GST_VIDEO_EDITOR(obj)             (G_TYPE_CHECK_INSTANCE_CAST ((obj), GST_TYPE_VIDEO_EDITOR, GstVideoEditor))
#define GST_VIDEO_EDITOR_CLASS(klass)     (G_TYPE_CHECK_CLASS_CAST ((klass), GST_TYPE_VIDEO_EDITOR, GstVideoEditorClass))
#define GST_IS_VIDEO_EDITOR(obj)          (G_TYPE_CHECK_INSTANCE_TYPE ((obj), GST_TYPE_VIDEO_EDITOR))
#define GST_IS_VIDEO_EDITOR_CLASS(klass)  (G_TYPE_CHECK_CLASS_TYPE ((klass), GST_TYPE_VIDEO_EDITOR))
#define GST_VIDEO_EDITOR_GET_CLASS(obj)   (G_TYPE_INSTANCE_GET_CLASS ((obj), GST_TYPE_VIDEO_EDITOR, GstVideoEditorClass))
#define GVC_ERROR gst_video_editor_error_quark ()
typedef struct _GstVideoEditorClass GstVideoEditorClass;
typedef struct _GstVideoEditor GstVideoEditor;
typedef struct GstVideoEditorPrivate GstVideoEditorPrivate;


struct _GstVideoEditorClass
{
  GtkHBoxClass parent_class;

  void (*error) (GstVideoEditor * gve, const char *message);
  void (*percent_completed) (GstVideoEditor * gve, float percent);
};

struct _GstVideoEditor
{
  GtkHBox parent_instance;
  GstVideoEditorPrivate *priv;
};

EXPORT GType gst_video_editor_get_type (void) G_GNUC_CONST;


EXPORT void gst_video_editor_init_backend (int *argc, char ***argv);
EXPORT GstVideoEditor *gst_video_editor_new (GError ** err);
EXPORT void gst_video_editor_start (GstVideoEditor * gve);
EXPORT void gst_video_editor_cancel (GstVideoEditor * gve);
EXPORT void gst_video_editor_set_video_encoder (GstVideoEditor * gve,
    gchar ** err, VideoEncoderType codec);
EXPORT void gst_video_editor_set_audio_encoder (GstVideoEditor * gve,
    gchar ** err, AudioEncoderType codec);
EXPORT void gst_video_editor_set_video_muxer (GstVideoEditor * gve,
    gchar ** err, VideoMuxerType codec);
EXPORT void gst_video_editor_clear_segments_list (GstVideoEditor * gve);
EXPORT void gst_video_editor_add_segment (GstVideoEditor * gve,
    gchar * file, gint64 start,
    gint64 duration, gdouble rate, gchar * title, gboolean hasAudio);
EXPORT void gst_video_editor_add_image_segment (GstVideoEditor * gve, gchar * file,
    guint64 start, gint64 duration, gchar * title);
G_END_DECLS
#endif /* _GST_VIDEO_EDITOR_H_ */
