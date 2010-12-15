// MediaFile.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using Mono.Unix;
using Gdk;
using LongoMatch.Video;
using LongoMatch.Video.Player;
using LongoMatch.Video.Common;

namespace LongoMatch.Video.Utils
{
	
	[Serializable]
	
	public class MediaFile
	{
		
		string filePath;
		long length; // In MSeconds
		ushort fps;
		bool hasAudio;
		bool hasVideo;
		string videoCodec;
		string audioCodec;
		uint videoHeight;
		uint videoWidth;
	
		
		public MediaFile(){}
		
		public MediaFile(string filePath,
		                 long length,
		                 ushort fps,
		                 bool hasAudio, 
		                 bool hasVideo, 
		                 string videoCodec, 
		                 string audioCodec, 
		                 uint videoWidth, 
		                 uint videoHeight)
		{
			this.filePath = filePath;
			this.length = length;
			this.hasAudio = hasAudio;
			this.hasVideo = hasVideo;
			this.videoCodec = videoCodec;
			this.audioCodec = audioCodec;
			this.videoHeight = videoHeight;
			this.videoWidth = videoWidth;
			if (fps == 0)
					//For audio Files
					this.fps=25;
				else
					this.fps = fps;
			
		}
		
		public string FilePath{
			get {return this.filePath;}
			set {this.filePath = value;}
		}
		
		public long Length{
			get {return this.length;}
			set {this.length = value;}
		}
		
		public bool HasVideo{
			get { return this.hasVideo;}
			set{this.hasVideo = value;}
		}
		
		public bool HasAudio{
			get { return this.hasAudio;}
			set{this.hasAudio = value;}
		}
		
		public string VideoCodec{
			get {return this.videoCodec;}
			set {this.videoCodec = value;}
		}
		
		public string AudioCodec{
			get {return this.audioCodec;}
			set {this.audioCodec = value;}
			}
		
		public uint VideoWidth{
			get {return this.videoWidth;}
			set {this.videoWidth= value;}			
		}
		
		public uint VideoHeight{
			get {return this.videoHeight;}
			set {this.videoHeight= value;}			
		}
		
		public ushort Fps{
			get {return this.fps;}
			set {
				if (value == 0)
					//For audio Files
					this.fps=25;
				else
					this.fps = value;}
		}
		
		public uint GetFrames(){
			return (uint) (Fps*Length/1000);
		}
		
		
			
		public static MediaFile GetMediaFile(string filePath){
			int duration, fps=0, height=0, width=0;			
			bool hasVideo, hasAudio;
			string audioCodec = "", videoCodec = "";
			MultimediaFactory factory;
			IMetadataReader reader = null;
			
			try{
				factory =  new MultimediaFactory();
				reader = factory.getMetadataReader();
				reader.Open(filePath);
				duration = (int)reader.GetMetadata(MetadataType.Duration);						
				hasVideo = (bool) reader.GetMetadata(MetadataType.HasVideo);
				hasAudio = (bool) reader.GetMetadata(MetadataType.HasAudio);
				if (hasAudio)
					audioCodec = (string) reader.GetMetadata(MetadataType.AudioEncoderType);					
				if (hasVideo){
					videoCodec = (string) reader.GetMetadata(MetadataType.VideoEncoderType);	
					fps = (int) reader.GetMetadata(MetadataType.Fps);
				}			
				height = (int) reader.GetMetadata(MetadataType.DimensionX);
				width = (int) reader.GetMetadata (MetadataType.DimensionY);
								
				return new MediaFile(filePath,duration*1000,(ushort)fps,
				                     hasAudio,hasVideo,videoCodec,audioCodec,
				                     (uint)height,(uint)width);
		
			}
			catch (GLib.GException ex){
			    throw new Exception (Catalog.GetString("Invalid video file:")+"\n"+ex.Message);
			}
			finally {
				reader.Close();	
				reader.Dispose();
			}
		}
	
		
	}
}
