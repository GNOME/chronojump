/*
 * gst-smart-video-scaler.c
 * Copyright (C) Andoni Morales Alastruey 2009 <ylatuya@gmail.com>
 * 
 * gst-smart-video-scaler.c is free software: you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by the
 * Free Software Foundation, either version 2 of the License, or
 * (at your option) any later version.
 * 
 * gst-smart-video-scaler.c is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along
 * with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

#ifndef _GST_SMART_VIDEO_SCALER_H_
#define _GST_SMART_VIDEO_SCALER_H_

#include <glib-object.h>
#include <gst/gst.h>

#ifdef WIN32
#define EXPORT __declspec (dllexport)
#else
#define EXPORT
#endif

G_BEGIN_DECLS
#define GST_TYPE_SMART_VIDEO_SCALER             (gst_smart_video_scaler_get_type ())
#define GST_SMART_VIDEO_SCALER(obj)             (G_TYPE_CHECK_INSTANCE_CAST ((obj), GST_TYPE_SMART_VIDEO_SCALER, GstSmartVideoScaler))
#define GST_SMART_VIDEO_SCALER_CLASS(klass)     (G_TYPE_CHECK_CLASS_CAST ((klass), GST_TYPE_SMART_VIDEO_SCALER, GstSmartVideoScalerClass))
#define GST_IS_SMART_VIDEO_SCALER(obj)          (G_TYPE_CHECK_INSTANCE_TYPE ((obj), GST_TYPE_SMART_VIDEO_SCALER))
#define GST_IS_SMART_VIDEO_SCALER_CLASS(klass)  (G_TYPE_CHECK_CLASS_TYPE ((klass), GST_TYPE_SMART_VIDEO_SCALER))
#define GST_SMART_VIDEO_SCALER_GET_CLASS(obj)   (G_TYPE_INSTANCE_GET_CLASS ((obj), GST_TYPE_SMART_VIDEO_SCALER, GstSmartVideoScalerClass))
typedef struct _GstSmartVideoScalerClass GstSmartVideoScalerClass;
typedef struct _GstSmartVideoScaler GstSmartVideoScaler;
typedef struct _GstSmartVideoScalerPrivate GstSmartVideoScalerPrivate;

struct _GstSmartVideoScalerClass
{
  GstBinClass parent_class;
};

struct _GstSmartVideoScaler
{
  GstBin parent_instance;
  GstSmartVideoScalerPrivate *priv;


};

EXPORT GType
gst_smart_video_scaler_get_type (void)
    G_GNUC_CONST;
     EXPORT GstSmartVideoScaler *gst_smart_video_scaler_new ();
     EXPORT void gst_smart_video_scaler_set_caps (GstSmartVideoScaler * gsvs,
    GstCaps * caps);

G_END_DECLS
#endif /* _GST_SMART_VIDEO_SCALER_H_ */
