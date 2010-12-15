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

 /* This code is a port to C of the PiTiVi's smartvideoscaler */

#include "gst-smart-video-scaler.h"
#include <glib/gprintf.h>

G_DEFINE_TYPE (GstSmartVideoScaler, gst_smart_video_scaler, GST_TYPE_BIN);


struct _GstSmartVideoScalerPrivate
{
  gint widthin;
  gint heightin;
  GValue *parin;
  GValue *darin;

  GstCaps *capsout;
  gint widthout;
  gint heightout;
  GValue *parout;
  GValue *darout;

  GstElement *videoscale;
  GstElement *capsfilter;
  GstElement *videobox;

  GstPad *sink_pad;
  GstPad *src_pad;
};

static void
gst_smart_video_scaler_init (GstSmartVideoScaler * object)
{
  GstSmartVideoScalerPrivate *priv;
  object->priv = priv =
      G_TYPE_INSTANCE_GET_PRIVATE (object, GST_TYPE_SMART_VIDEO_SCALER,
      GstSmartVideoScalerPrivate);

  priv->parin = g_new0 (GValue, 1);
  g_value_init (priv->parin, GST_TYPE_FRACTION);
  gst_value_set_fraction (priv->parin, 1, 1);

  priv->parout = g_new0 (GValue, 1);
  g_value_init (priv->parout, GST_TYPE_FRACTION);
  gst_value_set_fraction (priv->parout, 1, 1);

  priv->darin = g_new0 (GValue, 1);
  g_value_init (priv->darin, GST_TYPE_FRACTION);
  gst_value_set_fraction (priv->darin, 1, 1);

  priv->darout = g_new0 (GValue, 1);
  g_value_init (priv->darout, GST_TYPE_FRACTION);
  gst_value_set_fraction (priv->darout, 1, 1);

  priv->widthin = -1;
  priv->widthout = -1;
  priv->heightin = -1;
  priv->heightout = -1;
}

static void
gst_smart_video_scaler_finalize (GObject * object)
{
  GstSmartVideoScaler *gsvs = (GstSmartVideoScaler *) object;

  if (gsvs != NULL) {
    gst_element_set_state (GST_ELEMENT (gsvs), GST_STATE_NULL);
    gst_object_unref (gsvs);
    gsvs->priv->videoscale = NULL;
    gsvs->priv->videobox = NULL;
    gsvs->priv->capsfilter = NULL;
  }

  g_free (gsvs->priv->parin);
  g_free (gsvs->priv->parout);
  g_free (gsvs->priv->darin);
  g_free (gsvs->priv->darout);

  G_OBJECT_CLASS (gst_smart_video_scaler_parent_class)->finalize (object);
}

static void
gst_smart_video_scaler_class_init (GstSmartVideoScalerClass * klass)
{
  GObjectClass *object_class = G_OBJECT_CLASS (klass);
  //GstBinClass* parent_class = GST_BIN_CLASS (klass);

  g_type_class_add_private (object_class, sizeof (GstSmartVideoScalerPrivate));

  object_class->finalize = gst_smart_video_scaler_finalize;
}

static void
gsvs_compute_and_set_values (GstSmartVideoScaler * gsvs)
{
  guint mayor, minor, micro, nano;
  gint left, right, bottom, top;
  gchar astr[100];
  gfloat fdarin, fdarout, fparin, fparout;
  GstCaps *caps;

  /*Calculate the new values to set on capsfilter and videobox. */
  if (gsvs->priv->widthin == -1 || gsvs->priv->heightin == -1
      || gsvs->priv->widthout == -1 || gsvs->priv->heightout == -1) {
    /* FIXME : should we reset videobox/capsfilter properties here ? */
    GST_ERROR
        ("We don't have input and output caps, we can't calculate videobox values");
    return;
  }

  fdarin =
      (gfloat) gst_value_get_fraction_numerator (gsvs->priv->darin) /
      (gfloat) gst_value_get_fraction_denominator (gsvs->priv->darin);
  fdarout =
      (gfloat) gst_value_get_fraction_numerator (gsvs->priv->darout) /
      (gfloat) gst_value_get_fraction_denominator (gsvs->priv->darout);
  fparin =
      (gfloat) gst_value_get_fraction_numerator (gsvs->priv->parin) /
      (gfloat) gst_value_get_fraction_denominator (gsvs->priv->parin);
  fparout =
      (gfloat) gst_value_get_fraction_numerator (gsvs->priv->parout) /
      (gfloat) gst_value_get_fraction_denominator (gsvs->priv->parout);
  GST_INFO ("incoming width/height/PAR/DAR : %d/%d/%f/%f",
      gsvs->priv->widthin, gsvs->priv->heightin, fparin, fdarin);
  GST_INFO ("outgoing width/height/PAR/DAR : %d/%d/%f/%f",
      gsvs->priv->widthout, gsvs->priv->heightout, fparout, fdarout);

  /* for core <= 0.10.22 we always set caps != any, see 574805 for the
     details */

  gst_version (&mayor, &minor, &micro, &nano);
  if (fdarin == fdarout && (mayor >= 0 && minor >= 10 && micro >= 23)) {
    GST_INFO
        ("We have same input and output caps, resetting capsfilter and videobox settings");
    /* same DAR, set inputcaps on capsfilter, reset videobox values */
    caps = gst_caps_new_any ();
    left = 0;
    right = 0;
    top = 0;
    bottom = 0;
  } else {
    gint par_d, par_n, dar_d, dar_n;
    gint extra;
    gchar scaps[200];

    par_n = gst_value_get_fraction_numerator (gsvs->priv->parout);
    par_d = gst_value_get_fraction_denominator (gsvs->priv->parout);
    dar_n = gst_value_get_fraction_numerator (gsvs->priv->darin);
    dar_d = gst_value_get_fraction_denominator (gsvs->priv->darin);

    if (fdarin > fdarout) {
      gint newheight;

      GST_INFO
          ("incoming DAR is greater that ougoing DAR. Adding top/bottom borders");
      /* width, PAR stays the same as output
         calculate newheight = (PAR * widthout) / DAR */
      newheight = (par_n * gsvs->priv->widthout * dar_d) / (par_d * dar_n);
      GST_INFO ("newheight should be %d", newheight);
      extra = gsvs->priv->heightout - newheight;
      top = extra / 2;
      bottom = extra - top;     /* compensate for odd extra */
      left = right = 0;
      /*calculate filter caps */
      g_sprintf (astr, "width=%d,height=%d", gsvs->priv->widthout, newheight);
    } else {
      gint newwidth;

      GST_INFO
          ("incoming DAR is smaller than outgoing DAR. Adding left/right borders");
      /* height, PAR stays the same as output
         calculate newwidth = (DAR * heightout) / PAR */
      newwidth = (dar_n * gsvs->priv->heightout * par_n) / (dar_d * par_n);
      GST_INFO ("newwidth should be %d", newwidth);
      extra = gsvs->priv->widthout - newwidth;
      left = extra / 2;
      right = extra - left;     /* compensate for odd extra */
      top = bottom = 0;
      /* calculate filter caps */
      g_sprintf (astr, "width=%d,height=%d", newwidth, gsvs->priv->heightout);
    }
    g_sprintf (scaps, "video/x-raw-yuv,%s;video/x-raw-rgb,%s", astr, astr);
    caps = gst_caps_from_string (scaps);
  }

  /* set properties on elements */
  GST_INFO ("About to set left/right/top/bottom : %d/%d/%d/%d", -left, -right,
      -top, -bottom);
  g_object_set (G_OBJECT (gsvs->priv->videobox), "left", -left, NULL);
  g_object_set (G_OBJECT (gsvs->priv->videobox), "right", -right, NULL);
  g_object_set (G_OBJECT (gsvs->priv->videobox), "top", -top, NULL);
  g_object_set (G_OBJECT (gsvs->priv->videobox), "bottom", -bottom, NULL);
  GST_INFO ("Settings filter caps %s", gst_caps_to_string (caps));
  g_object_set (G_OBJECT (gsvs->priv->capsfilter), "caps", caps, NULL);
  gst_caps_unref (caps);
}

static void
gsvs_get_value_from_caps (GstCaps * caps, gboolean force, gint * width,
    gint * height, GValue ** par, GValue ** dar)
{
  GstStructure *str = NULL;
  const GValue *tmp_par = NULL;
  gint num, denom;

  *width = -1;
  *height = -1;
  gst_value_set_fraction (*par, 1, 1);
  gst_value_set_fraction (*dar, 1, 1);

  if (force || (caps && gst_caps_is_fixed (caps))) {
    str = gst_caps_get_structure (caps, 0);
    gst_structure_get_int (str, "width", &(*width));
    gst_structure_get_int (str, "height", &(*height));

    if (g_strrstr (gst_structure_get_name (str), "pixel-aspect-ratio")) {
      tmp_par = gst_structure_get_value (str, "pixel-aspect-ratio");
      gst_value_set_fraction (*par,
          gst_value_get_fraction_numerator (tmp_par),
          gst_value_get_fraction_denominator (tmp_par));
    }
    num = gst_value_get_fraction_numerator (*par);
    denom = gst_value_get_fraction_denominator (*par);
    gst_value_set_fraction (*dar, *width * num, *height * denom);
  }
}

static gboolean
gsvs_sink_set_caps (GstPad * pad, GstCaps * caps)
{
  GstSmartVideoScaler *gsvs =
      GST_SMART_VIDEO_SCALER (gst_element_get_parent
      (gst_pad_get_parent (pad)));
  GstPad *videoscale_pad = NULL;

  videoscale_pad = gst_element_get_static_pad (GST_ELEMENT (gsvs), "sink");

  if (!gst_pad_set_caps (videoscale_pad, caps))
    return FALSE;

  gsvs_get_value_from_caps (caps, FALSE, &gsvs->priv->widthin,
      &gsvs->priv->heightin, &gsvs->priv->parin, &gsvs->priv->darin);
  gsvs_compute_and_set_values (gsvs);

  return TRUE;
}

void
gst_smart_video_scaler_set_caps (GstSmartVideoScaler * gsvs, GstCaps * caps)
{
  g_return_if_fail (GST_IS_SMART_VIDEO_SCALER (gsvs));

  gsvs_get_value_from_caps (caps, FALSE, &gsvs->priv->widthout,
      &gsvs->priv->heightout, &gsvs->priv->parout, &gsvs->priv->darout);

}

GstSmartVideoScaler *
gst_smart_video_scaler_new ()
{
  GstSmartVideoScaler *gsvs = NULL;
  GstPad *sink_pad;
  GstPad *src_pad;

  gsvs = g_object_new (GST_TYPE_SMART_VIDEO_SCALER, NULL);

  /*Create bin elements */
  gsvs->priv->videoscale =
      gst_element_factory_make ("videoscale", "smart-videoscale");
  g_object_set (G_OBJECT (gsvs->priv->videoscale), "method", 1, NULL);
  gsvs->priv->capsfilter =
      gst_element_factory_make ("capsfilter", "smart-capsfilter");
  gsvs->priv->videobox =
      gst_element_factory_make ("videobox", "smart-videobox");

  /*Add and link elements */
  gst_bin_add_many (GST_BIN (gsvs), gsvs->priv->videoscale,
      gsvs->priv->capsfilter, gsvs->priv->videobox, NULL);
  gst_element_link_many (gsvs->priv->videoscale, gsvs->priv->capsfilter,
      gsvs->priv->videobox, NULL);

  /*Create bin sink pad */
  sink_pad = gst_element_get_static_pad (gsvs->priv->videoscale, "sink");
  gst_pad_set_active (sink_pad, TRUE);
  gst_element_add_pad (GST_ELEMENT (gsvs),
      gst_ghost_pad_new ("sink", sink_pad));

  /*Creat bin src pad */
  src_pad = gst_element_get_static_pad (gsvs->priv->videobox, "src");
  gst_pad_set_active (src_pad, TRUE);
  gst_element_add_pad (GST_ELEMENT (gsvs), gst_ghost_pad_new ("src", src_pad));

  gst_pad_set_setcaps_function (sink_pad, gsvs_sink_set_caps);

  return gsvs;
}
