/*
 * Copyright (C) 2010 Andoni Morales Alastruey <ylatuya@gmail.com>
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
 */

#ifndef __LIBCESARPLAYER_COMMON_H__
#define __LIBCESARPLAYER_COMMON_H__

/* Default video/audio sinks */
#if defined(OSTYPE_WINDOWS)
#define DEFAULT_VIDEO_SINK "d3dvideosink"
#define BACKUP_VIDEO_SINK "autovideosink"
#elif defined(OSTYPE_OS_X)
#define DEFAULT_VIDEO_SINK "osxvideosink"
#define BACKUP_VIDEO_SINK "autovideosink"
#elif defined(OSTYPE_LINUX)
#define DEFAULT_VIDEO_SINK "gsettingsvideosink"
#define BACKUP_VIDEO_SINK "autovideosink"
#endif

/*Default video/audio source*/
#if defined(OSTYPE_WINDOWS)
#define DVVIDEOSRC "dshowvideosrc"
#define SYSVIDEOSRC "dshowvideosrc"
#define AUDIOSRC "dshowaudiosrc"
#elif defined(OSTYPE_OS_X)
#define DVVIDEOSRC "osxvideosrc"
#define SYSVIDEOSRC "osxvideosrc"
#define AUDIOSRC "osxaudiosrc"
#elif defined(OSTYPE_LINUX)
#define DVVIDEOSRC "dv1394src"
#define SYSVIDEOSRC "gsettingsvideosrc"
#define AUDIOSRC "gsettingsaudiosrc"
#endif

/**
 * Error:
 * @GST_ERROR_AUDIO_PLUGIN: Error loading audio output plugin or device.
 * @GST_ERROR_NO_PLUGIN_FOR_FILE: A required GStreamer plugin or xine feature is missing.
 * @GST_ERROR_VIDEO_PLUGIN: Error loading video output plugin or device.
 * @GST_ERROR_AUDIO_BUSY: Audio output device is busy.
 * @GST_ERROR_BROKEN_FILE: The movie file is broken and cannot be decoded.
 * @GST_ERROR_FILE_GENERIC: A generic error for problems with movie files.
 * @GST_ERROR_FILE_PERMISSION: Permission was refused to access the stream, or authentication was required.
 * @GST_ERROR_FILE_ENCRYPTED: The stream is encrypted and cannot be played.
 * @GST_ERROR_FILE_NOT_FOUND: The stream cannot be found.
 * @GST_ERROR_DVD_ENCRYPTED: The DVD is encrypted and libdvdcss is not installed.
 * @GST_ERROR_INVALID_DEVICE: The device given in an MRL (e.g. DVD drive or DVB tuner) did not exist.
 * @GST_ERROR_DEVICE_BUSY: The device was busy.
 * @GST_ERROR_UNKNOWN_HOST: The host for a given stream could not be resolved.
 * @GST_ERROR_NETWORK_UNREACHABLE: The host for a given stream could not be reached.
 * @GST_ERROR_CONNECTION_REFUSED: The server for a given stream refused the connection.
 * @GST_ERROR_INVALID_LOCATION: An MRL was malformed, or CDDB playback was attempted (which is now unsupported).
 * @GST_ERROR_GENERIC: A generic error occurred.
 * @GST_ERROR_CODEC_NOT_HANDLED: The audio or video codec required by the stream is not supported.
 * @GST_ERROR_AUDIO_ONLY: An audio-only stream could not be played due to missing audio output support.
 * @GST_ERROR_CANNOT_CAPTURE: Error determining frame capture support for a video with bacon_video_widget_can_get_frames().
 * @GST_ERROR_READ_ERROR: A generic error for problems reading streams.
 * @GST_ERROR_PLUGIN_LOAD: A library or plugin could not be loaded.
 * @GST_ERROR_EMPTY_FILE: A movie file was empty.
 *
 **/
typedef enum
{
  /* Plugins */
  GST_ERROR_AUDIO_PLUGIN,
  GST_ERROR_NO_PLUGIN_FOR_FILE,
  GST_ERROR_VIDEO_PLUGIN,
  GST_ERROR_AUDIO_BUSY,
  /* File */
  GST_ERROR_BROKEN_FILE,
  GST_ERROR_FILE_GENERIC,
  GST_ERROR_FILE_PERMISSION,
  GST_ERROR_FILE_ENCRYPTED,
  GST_ERROR_FILE_NOT_FOUND,
  /* Devices */
  GST_ERROR_DVD_ENCRYPTED,
  GST_ERROR_INVALID_DEVICE,
  GST_ERROR_DEVICE_BUSY,
  /* Network */
  GST_ERROR_UNKNOWN_HOST,
  GST_ERROR_NETWORK_UNREACHABLE,
  GST_ERROR_CONNECTION_REFUSED,
  /* Generic */
  GST_ERROR_INVALID_LOCATION,
  GST_ERROR_GENERIC,
  GST_ERROR_CODEC_NOT_HANDLED,
  GST_ERROR_AUDIO_ONLY,
  GST_ERROR_CANNOT_CAPTURE,
  GST_ERROR_READ_ERROR,
  GST_ERROR_PLUGIN_LOAD,
  GST_ERROR_EMPTY_FILE
} Error;


typedef enum
{
  VIDEO_ENCODER_MPEG4,
  VIDEO_ENCODER_XVID,
  VIDEO_ENCODER_THEORA,
  VIDEO_ENCODER_H264,
  VIDEO_ENCODER_MPEG2,
  VIDEO_ENCODER_VP8
} VideoEncoderType;

typedef enum
{
  AUDIO_ENCODER_MP3,
  AUDIO_ENCODER_AAC,
  AUDIO_ENCODER_VORBIS
} AudioEncoderType;

typedef enum
{
  VIDEO_MUXER_AVI,
  VIDEO_MUXER_MP4,
  VIDEO_MUXER_MATROSKA,
  VIDEO_MUXER_OGG,
  VIDEO_MUXER_MPEG_PS,
  VIDEO_MUXER_WEBM
} VideoMuxerType;

typedef enum
{
  CAPTURE_SOURCE_TYPE_NONE = 0,
  CAPTURE_SOURCE_TYPE_DV = 1,
  CAPTURE_SOURCE_TYPE_SYSTEM = 2,
} CaptureSourceType;

#endif
