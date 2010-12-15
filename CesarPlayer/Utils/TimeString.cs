// TimeString.cs
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

namespace LongoMatch.Video.Utils
{
	
	
	public class TimeString
	{
		
		public TimeString()
		{
		}
					
		public  static string SecondsToString (long time)
			{
				long _h, _m, _s;

				_h = (time / 3600);
				_m = ((time % 3600) / 60);
				_s = ((time % 3600) % 60);

				if (_h > 0)
					return String.Format ("{0}:{1}:{2}", _h, _m.ToString ("d2"), 
						_s.ToString ("d2"));
			
					return String.Format ("{0}:{1}", _m, _s.ToString ("d2"));
			}
		public  static string MSecondsToMSecondsString (long time)
			{
				long _h, _m, _s,_ms,_time;
				_time = time / 1000;
				_h = (_time / 3600);
				_m = ((_time % 3600) / 60);
				_s = ((_time % 3600) % 60);
				_ms = ((time % 3600000)%60000)%1000;
				
				if (_h > 0)
					return String.Format ("{0}:{1}:{2},{3}", _h, _m.ToString ("d2"), 
						_s.ToString ("d2"),_ms.ToString("d3"));
			
					return String.Format ("{0}:{1},{2}", _m, _s.ToString ("d2"),_ms.ToString("d3"));
			}
		public  static string MSecondsToSecondsString (long time)
			{
				long _h, _m, _s,_time;
				_time = time / 1000;
				_h = (_time / 3600);
				_m = ((_time % 3600) / 60);
				_s = ((_time % 3600) % 60);
				
				if (_h > 0)
					return String.Format ("{0}:{1}:{2}", _h, _m.ToString ("d2"), 
						_s.ToString ("d2"));
			
					return String.Format ("{0}:{1}", _m, _s.ToString ("d2"));
			}
		public static string FileName( string filename){
			return System.IO.Path.GetFileName(filename);
		}
	
		
	}
}
