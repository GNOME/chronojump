/*
 * This file is part of ChronoJump
 *
 * ChronoJump is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or   
 * (at your option) any later version.
 *    
 * ChronoJump is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Copyright (C) 2018   Xavier de Blas <xaviblas@gmail.com> 
 */

using System.Collections.Generic; //List
using System.Diagnostics;
using System;
using System.IO;

//todo separate in different classes (inherited)
public abstract class Webcam
{
	public bool Running;

	protected Process process;
	protected string videoDevice;

	// Result struct holds the output, error and success operations. It's used to pass
	// errors from different layers (e.g. executing Python scripts) to the UI layer
	public struct Result
	{
		public bool success;
		public string output;
		public string error;

		public Result(bool success, string output, string error = "")
		{
			this.success = success;
			this.output = output;
			this.error = error;
		}
	}

	/*
	 * constructor for capture
	 */

	public Webcam(string videoDevice)
	{
		this.videoDevice = videoDevice;
		Running = false;
	}

	/*
	 * constructor for play
	 */

	public Webcam()
	{
	}

	/*
	 * public methods
	 */

	public enum CaptureTypes { PHOTO, VIDEO }

	public abstract Result CapturePrepare (CaptureTypes captureType);

	public abstract Result Play(string filename);

	public abstract bool Snapshot();

	public abstract Result VideoCaptureStart();

	//short process, to do end capture (good if there's more than one camera to end capture all at same time)
	public abstract Result VideoCaptureEnd();

	public abstract Result ExitAndFinish (int sessionID, Constants.TestTypes testType, int testID);

	public abstract void ExitCamera();

	/*
	 * private methods
	 */

	// convert /dev/video0 to _dev_video0
	protected string videoDeviceToFilename()
	{
		return Util.ChangeChars(videoDevice, "/", "_");
	}

}
