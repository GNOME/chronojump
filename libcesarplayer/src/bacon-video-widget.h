/*
 * Copyright (C) 2001,2002,2003,2004,2005 Bastien Nocera <hadess@hadess.net>
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
 * permission are above and beyond the permissions granted by the GPL license
 * Totem is covered by.
 *
 * Monday 7th February 2005: Christian Schaller: Add excemption clause.
 * See license_change file for details.
 *
 */

#ifndef HAVE_BACON_VIDEO_WIDGET_H
#define HAVE_BACON_VIDEO_WIDGET_H

#ifdef WIN32
#define EXPORT __declspec (dllexport)
#else
#define EXPORT
#endif


#include <gtk/gtkbox.h>
#include <gst/gst.h>
/* for optical disc enumeration type */
//#include "totem-disc.h"

G_BEGIN_DECLS
#define BACON_TYPE_VIDEO_WIDGET		     (bacon_video_widget_get_type ())
#define BACON_VIDEO_WIDGET(obj)              (G_TYPE_CHECK_INSTANCE_CAST ((obj), bacon_video_widget_get_type (), BaconVideoWidget))
#define BACON_VIDEO_WIDGET_CLASS(klass)      (G_TYPE_CHECK_CLASS_CAST ((klass), bacon_video_widget_get_type (), BaconVideoWidgetClass))
#define BACON_IS_VIDEO_WIDGET(obj)           (G_TYPE_CHECK_INSTANCE_TYPE (obj, bacon_video_widget_get_type ()))
#define BACON_IS_VIDEO_WIDGET_CLASS(klass)   (G_CHECK_INSTANCE_GET_CLASS ((klass), bacon_video_widget_get_type ()))
#define BVW_ERROR bacon_video_widget_error_quark ()
typedef struct BaconVideoWidgetPrivate BaconVideoWidgetPrivate;


typedef struct
{
  GtkEventBox parent;
  BaconVideoWidgetPrivate *priv;
} BaconVideoWidget;

typedef struct
{
  GtkEventBoxClass parent_class;

  void (*error) (BaconVideoWidget * bvw, const char *message);
  void (*eos) (BaconVideoWidget * bvw);
  void (*got_metadata) (BaconVideoWidget * bvw);
  void (*segment_done) (BaconVideoWidget * bvw);
  void (*got_redirect) (BaconVideoWidget * bvw, const char *mrl);
  void (*title_change) (BaconVideoWidget * bvw, const char *title);
  void (*channels_change) (BaconVideoWidget * bvw);
  void (*tick) (BaconVideoWidget * bvw, gint64 current_time,
      gint64 stream_length, float current_position, gboolean seekable);
  void (*buffering) (BaconVideoWidget * bvw, guint progress);
  void (*state_change) (BaconVideoWidget * bvw, gboolean playing);
  void (*got_duration) (BaconVideoWidget * bvw);
  void (*ready_to_seek) (BaconVideoWidget * bvw);
} BaconVideoWidgetClass;


EXPORT GQuark
bacon_video_widget_error_quark (void)
    G_GNUC_CONST;
     EXPORT GType bacon_video_widget_get_type (void) G_GNUC_CONST;
     EXPORT GOptionGroup *bacon_video_widget_get_option_group (void);
/* This can be used if the app does not use popt */
     EXPORT void bacon_video_widget_init_backend (int *argc, char ***argv);

/**
 * BvwUseType:
 * @BVW_USE_TYPE_VIDEO: fully-featured with video, audio, capture and metadata support
 * @BVW_USE_TYPE_AUDIO: audio and metadata support
 * @BVW_USE_TYPE_CAPTURE: capture support only
 * @BVW_USE_TYPE_METADATA: metadata support only
 *
 * The purpose for which a #BaconVideoWidget will be used, as specified to
 * bacon_video_widget_new(). This determines which features will be enabled
 * in the created widget.
 **/
     typedef enum
     {
       BVW_USE_TYPE_VIDEO,
       BVW_USE_TYPE_AUDIO,
       BVW_USE_TYPE_CAPTURE,
       BVW_USE_TYPE_METADATA
     } BvwUseType;

     EXPORT GtkWidget *bacon_video_widget_new (int width, int height,
    BvwUseType type, GError ** error);

     EXPORT char *bacon_video_widget_get_backend_name (BaconVideoWidget * bvw);

/* Actions */
     EXPORT gboolean bacon_video_widget_open (BaconVideoWidget * bvw,
    const char *mrl, const char *subtitle_uri, GError ** error);
     EXPORT gboolean bacon_video_widget_play (BaconVideoWidget * bvw);
     EXPORT void bacon_video_widget_pause (BaconVideoWidget * bvw);
     EXPORT gboolean bacon_video_widget_is_playing (BaconVideoWidget * bvw);

/* Seeking and length */
     EXPORT gboolean bacon_video_widget_is_seekable (BaconVideoWidget * bvw);
     EXPORT gboolean bacon_video_widget_seek (BaconVideoWidget * bvw,
    gdouble position, gfloat rate);
     EXPORT gboolean bacon_video_widget_seek_time (BaconVideoWidget * bvw,
    gint64 time, gfloat rate, gboolean accurate);
     EXPORT gboolean bacon_video_widget_segment_seek (BaconVideoWidget * bvw,
    gint64 start, gint64 stop, gfloat rate);
     EXPORT gboolean bacon_video_widget_seek_in_segment (BaconVideoWidget *
    bvw, gint64 pos, gfloat rate);
     EXPORT gboolean bacon_video_widget_seek_to_next_frame (BaconVideoWidget *
    bvw, gfloat rate, gboolean in_segment);
     EXPORT gboolean
         bacon_video_widget_seek_to_previous_frame (BaconVideoWidget * bvw,
    gfloat rate, gboolean in_segment);
     EXPORT gboolean bacon_video_widget_segment_stop_update (BaconVideoWidget
    * bvw, gint64 stop, gfloat rate);
     EXPORT gboolean bacon_video_widget_segment_start_update (BaconVideoWidget
    * bvw, gint64 start, gfloat rate);
     EXPORT gboolean bacon_video_widget_new_file_seek (BaconVideoWidget * bvw,
    gint64 start, gint64 stop, gfloat rate);
     EXPORT gboolean bacon_video_widget_can_direct_seek (BaconVideoWidget *
    bvw);
     EXPORT double bacon_video_widget_get_position (BaconVideoWidget * bvw);
     EXPORT gint64 bacon_video_widget_get_current_time (BaconVideoWidget * bvw);
     EXPORT gint64 bacon_video_widget_get_stream_length (BaconVideoWidget *
    bvw);
     EXPORT gint64
         bacon_video_widget_get_accurate_current_time (BaconVideoWidget * bvw);
     EXPORT gboolean bacon_video_widget_set_rate (BaconVideoWidget * bvw,
    gfloat rate);
     EXPORT gboolean bacon_video_widget_set_rate_in_segment (BaconVideoWidget
    * bvw, gfloat rate, gint64 stop);



     EXPORT void bacon_video_widget_stop (BaconVideoWidget * bvw);
     EXPORT void bacon_video_widget_close (BaconVideoWidget * bvw);

/* Audio volume */
     EXPORT gboolean bacon_video_widget_can_set_volume (BaconVideoWidget * bvw);
     EXPORT void bacon_video_widget_set_volume (BaconVideoWidget * bvw,
    double volume);
     EXPORT double bacon_video_widget_get_volume (BaconVideoWidget * bvw);

/*Drawings Overlay*/
     EXPORT void bacon_video_widget_set_drawing_pixbuf (BaconVideoWidget *
    bvw, GdkPixbuf * drawing);
     EXPORT void bacon_video_widget_set_drawing_mode (BaconVideoWidget * bvw,
    gboolean drawing_mode);


/* Properties */
     EXPORT void bacon_video_widget_set_logo (BaconVideoWidget * bvw,
    char *filename);
     EXPORT void bacon_video_widget_set_logo_pixbuf (BaconVideoWidget * bvw,
    GdkPixbuf * logo);
     EXPORT void bacon_video_widget_set_logo_mode (BaconVideoWidget * bvw,
    gboolean logo_mode);
     EXPORT gboolean bacon_video_widget_get_logo_mode (BaconVideoWidget * bvw);

     EXPORT void bacon_video_widget_set_fullscreen (BaconVideoWidget * bvw,
    gboolean fullscreen);

     EXPORT void bacon_video_widget_set_show_cursor (BaconVideoWidget * bvw,
    gboolean show_cursor);
     EXPORT gboolean bacon_video_widget_get_show_cursor (BaconVideoWidget *
    bvw);

     EXPORT gboolean bacon_video_widget_get_auto_resize (BaconVideoWidget *
    bvw);
     EXPORT void bacon_video_widget_set_auto_resize (BaconVideoWidget * bvw,
    gboolean auto_resize);

     EXPORT void bacon_video_widget_set_connection_speed (BaconVideoWidget *
    bvw, int speed);
     EXPORT int bacon_video_widget_get_connection_speed (BaconVideoWidget *
    bvw);

     EXPORT void bacon_video_widget_set_subtitle_font (BaconVideoWidget * bvw,
    const char *font);
     EXPORT void bacon_video_widget_set_subtitle_encoding (BaconVideoWidget *
    bvw, const char *encoding);

/* Metadata */
/**
 * BvwMetadataType:
 * @BVW_INFO_TITLE: the stream's title
 * @BVW_INFO_ARTIST: the artist who created the work
 * @BVW_INFO_YEAR: the year in which the work was created
 * @BVW_INFO_COMMENT: a comment attached to the stream
 * @BVW_INFO_ALBUM: the album in which the work was released
 * @BVW_INFO_DURATION: the stream's duration, in seconds
 * @BVW_INFO_TRACK_NUMBER: the track number of the work on the album
 * @BVW_INFO_COVER: a #GdkPixbuf of the cover artwork
 * @BVW_INFO_HAS_VIDEO: whether the stream has video
 * @BVW_INFO_DIMENSION_X: the video's width, in pixels
 * @BVW_INFO_DIMENSION_Y: the video's height, in pixels
 * @BVW_INFO_VIDEO_BITRATE: the video's bitrate, in kilobits per second
 * @BVW_INFO_VIDEO_CODEC: the video's codec
 * @BVW_INFO_FPS: the number of frames per second in the video
 * @BVW_INFO_HAS_AUDIO: whether the stream has audio
 * @BVW_INFO_AUDIO_BITRATE: the audio's bitrate, in kilobits per second
 * @BVW_INFO_AUDIO_CODEC: the audio's codec
 * @BVW_INFO_AUDIO_SAMPLE_RATE: the audio sample rate, in bits per second
 * @BVW_INFO_AUDIO_CHANNELS: a string describing the number of audio channels in the stream
 *
 * The different metadata available for querying from a #BaconVideoWidget
 * stream with bacon_video_widget_get_metadata().
 **/
     typedef enum
     {
       BVW_INFO_TITLE,
       BVW_INFO_ARTIST,
       BVW_INFO_YEAR,
       BVW_INFO_COMMENT,
       BVW_INFO_ALBUM,
       BVW_INFO_DURATION,
       BVW_INFO_TRACK_NUMBER,
       BVW_INFO_COVER,
       /* Video */
       BVW_INFO_HAS_VIDEO,
       BVW_INFO_DIMENSION_X,
       BVW_INFO_DIMENSION_Y,
       BVW_INFO_VIDEO_BITRATE,
       BVW_INFO_VIDEO_CODEC,
       BVW_INFO_FPS,
       /* Audio */
       BVW_INFO_HAS_AUDIO,
       BVW_INFO_AUDIO_BITRATE,
       BVW_INFO_AUDIO_CODEC,
       BVW_INFO_AUDIO_SAMPLE_RATE,
       BVW_INFO_AUDIO_CHANNELS,
       /* Added later */
       BVW_INFO_PAR,
     } BvwMetadataType;

     EXPORT void bacon_video_widget_get_metadata (BaconVideoWidget * bvw,
    BvwMetadataType type, GValue * value);


/* Picture settings */
/**
 * BvwVideoProperty:
 * @BVW_VIDEO_BRIGHTNESS: the video brightness
 * @BVW_VIDEO_CONTRAST: the video contrast
 * @BVW_VIDEO_SATURATION: the video saturation
 * @BVW_VIDEO_HUE: the video hue
 *
 * The video properties queryable with bacon_video_widget_get_video_property(),
 * and settable with bacon_video_widget_set_video_property().
 **/
     typedef enum
     {
       BVW_VIDEO_BRIGHTNESS,
       BVW_VIDEO_CONTRAST,
       BVW_VIDEO_SATURATION,
       BVW_VIDEO_HUE
     } BvwVideoProperty;

/**
 * BvwAspectRatio:
 * @BVW_RATIO_AUTO: automatic
 * @BVW_RATIO_SQUARE: square (1:1)
 * @BVW_RATIO_FOURBYTHREE: four-by-three (4:3)
 * @BVW_RATIO_ANAMORPHIC: anamorphic (16:9)
 * @BVW_RATIO_DVB: DVB (20:9)
 *
 * The pixel aspect ratios available in which to display videos using
 * @bacon_video_widget_set_aspect_ratio().
 **/
     typedef enum
     {
       BVW_RATIO_AUTO = 0,
       BVW_RATIO_SQUARE = 1,
       BVW_RATIO_FOURBYTHREE = 2,
       BVW_RATIO_ANAMORPHIC = 3,
       BVW_RATIO_DVB = 4
     } BvwAspectRatio;

     EXPORT gboolean bacon_video_widget_can_deinterlace (BaconVideoWidget *
    bvw);
     EXPORT void bacon_video_widget_set_deinterlacing (BaconVideoWidget * bvw,
    gboolean deinterlace);
     EXPORT gboolean bacon_video_widget_get_deinterlacing (BaconVideoWidget *
    bvw);

     EXPORT void bacon_video_widget_set_aspect_ratio (BaconVideoWidget * bvw,
    BvwAspectRatio ratio);
     EXPORT BvwAspectRatio bacon_video_widget_get_aspect_ratio
         (BaconVideoWidget * bvw);

     EXPORT void bacon_video_widget_set_scale_ratio (BaconVideoWidget * bvw,
    float ratio);

     EXPORT void bacon_video_widget_set_zoom (BaconVideoWidget * bvw,
    double zoom);
     EXPORT double bacon_video_widget_get_zoom (BaconVideoWidget * bvw);

     EXPORT int bacon_video_widget_get_video_property (BaconVideoWidget * bvw,
    BvwVideoProperty type);
     EXPORT void bacon_video_widget_set_video_property (BaconVideoWidget *
    bvw, BvwVideoProperty type, int value);

/* DVD functions */
/**
 * BvwDVDEvent:
 * @BVW_DVD_ROOT_MENU: root menu
 * @BVW_DVD_TITLE_MENU: title menu
 * @BVW_DVD_SUBPICTURE_MENU: subpicture menu (if available)
 * @BVW_DVD_AUDIO_MENU: audio menu (if available)
 * @BVW_DVD_ANGLE_MENU: angle menu (if available)
 * @BVW_DVD_CHAPTER_MENU: chapter menu
 * @BVW_DVD_NEXT_CHAPTER: the next chapter
 * @BVW_DVD_PREV_CHAPTER: the previous chapter
 * @BVW_DVD_NEXT_TITLE: the next title in the current chapter
 * @BVW_DVD_PREV_TITLE: the previous title in the current chapter
 * @BVW_DVD_NEXT_ANGLE: the next angle
 * @BVW_DVD_PREV_ANGLE: the previous angle
 * @BVW_DVD_ROOT_MENU_UP: go up in the menu
 * @BVW_DVD_ROOT_MENU_DOWN: go down in the menu
 * @BVW_DVD_ROOT_MENU_LEFT: go left in the menu
 * @BVW_DVD_ROOT_MENU_RIGHT: go right in the menu
 * @BVW_DVD_ROOT_MENU_SELECT: select the current menu entry
 *
 * The DVD navigation actions available to fire as DVD events to
 * the #BaconVideoWidget.
 **/
     typedef enum
     {
       BVW_DVD_ROOT_MENU,
       BVW_DVD_TITLE_MENU,
       BVW_DVD_SUBPICTURE_MENU,
       BVW_DVD_AUDIO_MENU,
       BVW_DVD_ANGLE_MENU,
       BVW_DVD_CHAPTER_MENU,
       BVW_DVD_NEXT_CHAPTER,
       BVW_DVD_PREV_CHAPTER,
       BVW_DVD_NEXT_TITLE,
       BVW_DVD_PREV_TITLE,
       BVW_DVD_NEXT_ANGLE,
       BVW_DVD_PREV_ANGLE,
       BVW_DVD_ROOT_MENU_UP,
       BVW_DVD_ROOT_MENU_DOWN,
       BVW_DVD_ROOT_MENU_LEFT,
       BVW_DVD_ROOT_MENU_RIGHT,
       BVW_DVD_ROOT_MENU_SELECT
     } BvwDVDEvent;

     EXPORT void bacon_video_widget_dvd_event (BaconVideoWidget * bvw,
    BvwDVDEvent type);
     EXPORT GList *bacon_video_widget_get_languages (BaconVideoWidget * bvw);
     EXPORT int bacon_video_widget_get_language (BaconVideoWidget * bvw);
     EXPORT void bacon_video_widget_set_language (BaconVideoWidget * bvw,
    int language);

     EXPORT GList *bacon_video_widget_get_subtitles (BaconVideoWidget * bvw);
     EXPORT int bacon_video_widget_get_subtitle (BaconVideoWidget * bvw);
     EXPORT void bacon_video_widget_set_subtitle (BaconVideoWidget * bvw,
    int subtitle);

     EXPORT gboolean bacon_video_widget_has_next_track (BaconVideoWidget * bvw);
     EXPORT gboolean bacon_video_widget_has_previous_track (BaconVideoWidget *
    bvw);

/* Screenshot functions */
     EXPORT gboolean bacon_video_widget_can_get_frames (BaconVideoWidget *
    bvw, GError ** error);
     EXPORT GdkPixbuf *bacon_video_widget_get_current_frame (BaconVideoWidget
    * bvw);
     EXPORT void bacon_video_widget_unref_pixbuf (GdkPixbuf * pixbuf);

/* Audio-out functions */
/**
 * BvwAudioOutType:
 * @BVW_AUDIO_SOUND_STEREO: stereo output
 * @BVW_AUDIO_SOUND_4CHANNEL: 4-channel output
 * @BVW_AUDIO_SOUND_41CHANNEL: 4.1-channel output
 * @BVW_AUDIO_SOUND_5CHANNEL: 5-channel output
 * @BVW_AUDIO_SOUND_51CHANNEL: 5.1-channel output
 * @BVW_AUDIO_SOUND_AC3PASSTHRU: AC3 passthrough output
 *
 * The audio output types available for use with bacon_video_widget_set_audio_out_type().
 **/
     typedef enum
     {
       BVW_AUDIO_SOUND_STEREO,
       BVW_AUDIO_SOUND_CHANNEL4,
       BVW_AUDIO_SOUND_CHANNEL41,
       BVW_AUDIO_SOUND_CHANNEL5,
       BVW_AUDIO_SOUND_CHANNEL51,
       BVW_AUDIO_SOUND_AC3PASSTHRU
     } BvwAudioOutType;

     EXPORT BvwAudioOutType bacon_video_widget_get_audio_out_type
         (BaconVideoWidget * bvw);
     EXPORT gboolean bacon_video_widget_set_audio_out_type (BaconVideoWidget *
    bvw, BvwAudioOutType type);

G_END_DECLS
#endif /* HAVE_BACON_VIDEO_WIDGET_H */
