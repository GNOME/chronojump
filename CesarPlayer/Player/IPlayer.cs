// IPlayer.cs
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
using Gtk;
using Gdk;
using LongoMatch.Video.Common;


namespace LongoMatch.Video.Player
	      
{
	public interface IPlayer
	{
	

		// Events
		
	
		event         ErrorHandler Error;
		event         System.EventHandler Eos;
		event         StateChangeHandler StateChange;
		event         TickHandler Tick;
		event         System.EventHandler GotDuration;
		event         System.EventHandler SegmentDone;
		event         System.EventHandler ReadyToSeek;
		
		
		long StreamLength{
			get;
		}
		
		
		
		long CurrentTime{
			get;
			
		}
		
		double Position{
			get;
			set;
			
		}
		
		bool LogoMode {
			get;
			set;
		}
		
		bool DrawingMode {
			set;
		}
		
		Pixbuf DrawingPixbuf {
			set;
		}
		
		bool ExpandLogo{
			get;
			set;
		}
		
		double Volume{
			get;
			set;
		}		
		
		bool Playing {
			get;
		}
			
		string Logo {
			set;
		}
		
		Pixbuf LogoPixbuf{
			set;
		}
		
		long AccurateCurrentTime{
			get;
		}
		
		bool SeekTime(long time,float rate, bool accurate);
		
		bool Play();
		
		bool Open(string mrl);
		
		bool SetRate(float rate);
		
		bool SetRateInSegment(float rate, long stopTime);
		
		
		
		void TogglePlay();
		
		void Pause();
		
		void Stop();
				
		void Close();
		
		
		void Dispose();
		
		bool SegmentSeek(long start, long stop,float rate);
		
		bool SeekInSegment(long pos,float rate);
		
		bool NewFileSeek(long start, long stop,float rate);
			
		bool SegmentStartUpdate(long start,float rate);
		
		bool SegmentStopUpdate(long stop,float rate);
		
		bool SeekToNextFrame(float rate,bool in_segment);
		
		bool SeekToPreviousFrame(float rate,bool in_segment);
		
		Pixbuf GetCurrentFrame(int outwidth, int outheight);
		
		Pixbuf GetCurrentFrame();
		
		void CancelProgramedStop();
		
	}
}
