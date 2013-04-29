// 
//  Copyright (C) 2010 Andoni Morales Alastruey
// 
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
// 

using System;

namespace LongoMatch.Video.Common
{
	
	
	public enum Error {
		AudioPlugin,
		NoPluginForFile,
		VideoPlugin,
		AudioBusy,
		BrokenFile,
		FileGeneric,
		FilePermission,
		FileEncrypted,
		FileNotFound,
		DvdEncrypted,
		InvalidDevice,
		UnknownHost,
		NetworkUnreachable,
		ConnectionRefused,
		UnvalidLocation,
		Generic,
		CodecNotHandled,
		AudioOnly,
		CannotCapture,
		ReadError,
		PluginLoad,
		EmptyFile,
	}	
	
	public enum VideoEncoderType {
		Mpeg4,
		Xvid,
		Theora,
		H264,
		Mpeg2,
		VP8,
	}
	
	public enum AudioEncoderType {
		Mp3,
		Aac,
		Vorbis,
	}
	
	public enum VideoMuxerType {
		Avi,
		Mp4, 
		Matroska,
		Ogg,
		MpegPS,
		WebM,
	}	
	
	public enum CapturerType{
		Fake,
		Live,
		Snapshot,
	}	

	public enum VideoFormat {
		PORTABLE=0,
		VGA=1,
		TV=2,
		HD720p=3,
		HD1080p=4
	}
	
	public enum VideoQuality {
		Low = 1000,
		Normal = 3000,
		Good = 5000,
		Extra = 7000,
	}
	
	public enum AudioQuality
	{
		Low = 32000,
		Normal = 64000,
		Good = 128000,
		Extra = 256000,
		copy,
	}
	
	public enum PlayerUseType {
		Video,
		Audio,
		Capture,
		Metadata,
	}
	
	public enum VideoProperty {
		Brightness,
		Contrast,
		Saturation,
		Hue,
	}	
	
	public enum AspectRatio {
		Auto,
		Square,
		Fourbythree,
		Anamorphic,
		Dvb,
	}	
	
	public enum AudioOutType {
		Stereo,
		Channel4,
		Channel41,
		Channel5,
		Channel51,
		Ac3passthru,
	}
	
	public enum MetadataType {
		Title,
		Artist,
		Year,
		Comment,
		Album,
		Duration,
		TrackNumber,
		Cover,
		HasVideo,
		DimensionX,
		DimensionY,
		VideoBitrate,
		VideoEncoderType,
		Fps,
		HasAudio,
		AudioBitrate,
		AudioEncoderType,
		AudioSampleRate,
		AudioChannels,
	}
	
	public enum DeviceType {
		Video,
		Audio,
		DV
	}
	
	public enum CaptureSourceType {
		None,
		DV,
		System,
		URI,
	}
}
