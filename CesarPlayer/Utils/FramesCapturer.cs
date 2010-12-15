// FramesCapturer.cs
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
using LongoMatch.Video.Utils;
using LongoMatch.Video;
using Gdk;
using Gtk;
using System.Threading;
using LongoMatch.Video.Common;

namespace LongoMatch.Video.Utils
{
	
	
	public class FramesSeriesCapturer
	{
		IFramesCapturer capturer;
		long start;
		long stop;
		uint interval;
		int totalFrames;
		string seriesName;
		string outputDir;
		bool cancel;
		private const int THUMBNAIL_MAX_HEIGHT=250;
		private const int THUMBNAIL_MAX_WIDTH=300;
		
		public event FramesProgressHandler Progress;
		
		public FramesSeriesCapturer(string videoFile,long start, long stop, uint interval, string outputDir)
		{
			MultimediaFactory mf= new MultimediaFactory();
			this.capturer=mf.getFramesCapturer();
			this.capturer.Open(videoFile);
			this.start= start;
			this.stop = stop;
			this.interval = interval;
			this.outputDir = outputDir;
			this.seriesName = System.IO.Path.GetFileName(outputDir);			
			this.totalFrames = (int)Math.Floor((double)((stop - start ) / interval))+1;
		}
		
		public void Cancel(){
			cancel = true;	
		}
		
		public void Start(){
			Thread thread = new Thread(new ThreadStart(CaptureFrames));
			thread.Start();
		}
		
		public void CaptureFrames(){		
			long pos;
			Pixbuf frame;
			Pixbuf scaledFrame=null;
			int i = 0;			
						
			System.IO.Directory.CreateDirectory(outputDir);	
			
			pos = start;			
			if (Progress != null)					
						Application.Invoke(delegate {Progress(0,totalFrames,null);});
			while (pos <= stop){	
				if (!cancel){					
					capturer.SeekTime(pos,true);	
					capturer.Pause();
					frame = capturer.GetCurrentFrame();				
					if (frame != null) {
						frame.Save(System.IO.Path.Combine(outputDir,seriesName+"_" + i +".png"),"png");
						int h = frame.Height;
						int w = frame.Width;
						double rate = (double)w/(double)h;
						if (h>w)
							scaledFrame = frame.ScaleSimple((int)(THUMBNAIL_MAX_HEIGHT*rate),THUMBNAIL_MAX_HEIGHT,InterpType.Bilinear);
						else
							scaledFrame = frame.ScaleSimple(THUMBNAIL_MAX_WIDTH,(int)(THUMBNAIL_MAX_WIDTH/rate),InterpType.Bilinear);
						frame.Dispose();				
					}
					
					if (Progress != null)					
						Application.Invoke(delegate {Progress(i+1,totalFrames,scaledFrame);});
					pos += interval;
					i++;
				}
				else {
					System.IO.Directory.Delete(outputDir,true);	
					cancel=false;
					break;
				}
			}
		}
	}
}