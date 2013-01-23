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
 * foob is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with foob.  If not, write to:
 *      The Free Software Foundation, Inc.,
 *      51 Franklin Street, Fifth Floor
 *      Boston, MA  02110-1301, USA.
 */

#ifndef _GST_REMUXER_H_
#define _GST_REMUXER_H_

#ifdef WIN32
#define EXPORT __declspec (dllexport)
#else
#define EXPORT
#endif

#include <gst/gst.h>
#include "common.h"

G_BEGIN_DECLS
#define GST_TYPE_REMUXER             (gst_remuxer_get_type ())
#define GST_REMUXER(obj)             (G_TYPE_CHECK_INSTANCE_CAST ((obj), GST_TYPE_REMUXER, GstRemuxer))
#define GST_REMUXER_CLASS(klass)     (G_TYPE_CHECK_CLASS_CAST ((klass), GST_TYPE_REMUXER, GstRemuxerClass))
#define GST_IS_REMUXER(obj)          (G_TYPE_CHECK_INSTANCE_TYPE ((obj), GST_TYPE_REMUXER))
#define GST_IS_REMUXER_CLASS(klass)  (G_TYPE_CHECK_CLASS_TYPE ((klass), GST_TYPE_REMUXER))
#define GST_REMUXER_GET_CLASS(obj)   (G_TYPE_INSTANCE_GET_CLASS ((obj), GST_TYPE_REMUXER, GstRemuxerClass))
#define GCC_ERROR gst_remuxer_error_quark ()
typedef struct _GstRemuxerClass GstRemuxerClass;
typedef struct _GstRemuxer GstRemuxer;
typedef struct GstRemuxerPrivate GstRemuxerPrivate;


struct _GstRemuxerClass
{
  GObjectClass parent_class;

  void (*percent_completed) (GstRemuxer * remuxer, gfloat *percent);
  void (*error) (GstRemuxer * remuxer, const char *message);
};

struct _GstRemuxer
{
  GObject parent;
  GstRemuxerPrivate *priv;
};

EXPORT GType gst_remuxer_get_type (void) G_GNUC_CONST;

EXPORT void gst_remuxer_init_backend (int *argc, char ***argv);
EXPORT GstRemuxer *gst_remuxer_new (gchar *in_filename, gchar *out_filename, GError ** err);
EXPORT void gst_remuxer_start (GstRemuxer * remuxer);
EXPORT void gst_remuxer_cancel (GstRemuxer * remuxer);

G_END_DECLS
#endif /* _GST_REMUXER_H_ */
