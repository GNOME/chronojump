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
using System.Collections.Generic;
using LongoMatch.Video.Capturer;
using LongoMatch.Video.Common;
using Mono.Unix;

namespace LongoMatch.Video.Utils
{


	public class Device
	{
		public Device () {
			
		}

		/// <summary>
		/// Device Type among Video, Audio or DV (for dv cameras)
		/// </summary>
		public DeviceType DeviceType {
			get;
			set;
		}

		/// <summary>
		/// Device id, can be a human friendly name (for DirectShow devices), 
		/// the de device name (/dev/video0) or the GUID (dv1394src) 
		/// </summary>
		public string ID  {
			get;
			set;
		}
		
		/// <summary>
		/// The name of the gstreamer element property used to set the device
		/// </summary>
		
		public string IDProperty {
			get;
			set;
		}
		
		static public List<Device> ListVideoDevices (){
			List<Device> devicesList  = new List<Device>();
			
			/* Generate the list of devices and add the gconf one at the bottom
			 * so that DV sources are always selected before, at least on Linux, 
			 * since on Windows both raw an dv sources are listed from the same
			 * source element (dshowvideosrc) */
			foreach (string devName in GstCameraCapturer.VideoDevices){
				string idProp;
				
				if (Environment.OSVersion.Platform == PlatformID.Unix)
					idProp = Constants.DV1394SRC_PROP;
				else 
					idProp = Constants.DSHOWVIDEOSINK_PROP;
				
				devicesList.Add(new Device {
					ID = devName,
					IDProperty = idProp,
					DeviceType = DeviceType.DV});
			}
			if (Environment.OSVersion.Platform == PlatformID.Unix){
				devicesList.Add(new Device {
					ID = Catalog.GetString("Default device"),
					IDProperty = "",
					DeviceType = DeviceType.Video});
			}			
			return devicesList;
		}
	}
}
