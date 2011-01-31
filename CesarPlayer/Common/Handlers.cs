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
using Gdk;

namespace LongoMatch.Video.Common
{
	
	public delegate void PlayListSegmentDoneHandler ();
	public delegate void SegmentClosedHandler();
	public delegate void SegmentDoneHandler();
	public delegate void SeekEventHandler(long pos);
	public delegate void VolumeChangedHandler (double level);
	public delegate void NextButtonClickedHandler ();
	public delegate void PrevButtonClickedHandler ();
	public delegate void ProgressHandler (float progress);
	public delegate void FramesProgressHandler (int actual, int total,Pixbuf frame);
	public delegate void DrawFrameHandler (int time);
	public delegate void EllpasedTimeHandler (int ellapsedTime);	
	
	
	public delegate void ErrorHandler(object o, ErrorArgs args);
	public delegate void PercentCompletedHandler(object o, PercentCompletedArgs args);
	public delegate void StateChangeHandler(object o, StateChangeArgs args);
	public delegate void TickHandler(object o, TickArgs args);
	public delegate void DeviceChangeHandler(object o, DeviceChangeArgs args);
	public delegate void NewSnapshotHandler(Pixbuf snapshot);
	
	
	public class ErrorArgs : GLib.SignalArgs {
		public string Message{
			get {
				return (string) Args[0];
			}
		}

	}	

	public class PercentCompletedArgs : GLib.SignalArgs {
		public float Percent{
			get {
				return (float) Args[0];
			}
		}

	}

	public class StateChangeArgs : GLib.SignalArgs {
		public bool Playing{
			get {
				return (bool) Args[0];
			}
		}
	}	

	public class TickArgs : GLib.SignalArgs {
		public long CurrentTime{
			get {
				return (long) Args[0];
			}
		}

		public long StreamLength{
			get {
				return (long) Args[1];
			}
		}

		public float CurrentPosition{
			get {
				return (float) Args[2];
			}
		}

		public bool Seekable{
			get {
				return (bool) Args[3];
			}
		}
	}	

	public class DeviceChangeArgs : GLib.SignalArgs {
		public int DeviceChange{
			get {
				return (int) Args[0];
			}
		}

	}	
}
