/*
 * This file is part of ChronoJump
 *
 * ChronoJump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * ChronoJump is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 *    GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Collections.Generic; //List
using System.Diagnostics; 	//for detect OS
using System.IO; 		//for detect OS
//using Cairo;
//using Gdk;

//this class tries to be a space for methods that are used in different classes
public class UtilMultimedia
{
	/*
	 * VIDEO
	 */

	public static List<string> GetVideoDevices ()
	{
		/*
		 * TODO: reimplement this with ffmpeg
		 *
		List<LongoMatch.Video.Utils.Device> devices = LongoMatch.Video.Utils.Device.ListVideoDevices();
		string [] devicesStr = new String[devices.Count];
		int count = 0;
		LogB.Information("Searching video devices");
		foreach(LongoMatch.Video.Utils.Device dev in devices) {
			devicesStr[count++] = dev.ID.ToString();
			LogB.Information(dev.ID.ToString());
		}
		return devicesStr;
		*/


		//on Linux search for video0, video1, ...
		if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.LINUX)
			return GetVideoDevicesLinux();
		else if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.WINDOWS)
			return GetVideoDevicesWindows();
		else
			return new List<string>();
	}

	public static List<string> GetVideoDevicesLinux ()
	{
		List<string> list = new List<string>();
		string prefix = "/dev/";
		var dir = new DirectoryInfo(prefix);
		foreach(var file in dir.EnumerateFiles("video*"))
			/*
			 * return 0, 1, ...
			 if( file.Name.Length > 5 && 				//there's something more than "video", like "video0" or "video1", ...
			 char.IsNumber(file.Name, 5) ) 		//and it's a number
			 list.Add(Convert.ToInt32(file.Name[5])); 			//0 or 1, or ...
			 */
			//return "/dev/video0", "/dev/video1", ...
			list.Add(prefix + file.Name);

		return list;
	}

	public static List<string> GetVideoDevicesWindows ()
	{
		return WebcamFfmpegGetDevicesWindows.GetDevices();
	}

	/*
	 * IMAGES
	 */

	public enum ImageTypes { UNKNOWN, PNG, JPEG }
	public static ImageTypes GetImageType(string filename)
	{
		if(filename.ToLower().EndsWith("jpeg") || filename.ToLower().EndsWith("jpg"))
			return ImageTypes.JPEG;
		else if(filename.ToLower().EndsWith("png"))
			return ImageTypes.PNG;

		return ImageTypes.UNKNOWN;
	}
}
