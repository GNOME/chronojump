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

using System.Diagnostics;
using System;
using System.IO;
using Mono.Unix;

//todo separate in different classes (inherited)
public abstract class Webcam
{
	//messages
	protected static internal string programMplayerNotInstalled =
		string.Format(Catalog.GetString("Error. {0} is not installed."), "mplayer");
	protected static internal string programFfmpegNotInstalled =
		string.Format(Catalog.GetString("Error. {0} is not installed."), "ffmpeg");
	protected static internal string programMplayerClosed =
		string.Format(Catalog.GetString("Error. {0} has been closed."), "mplayer");
	protected static internal string programMplayerCannotSave =
		string.Format(Catalog.GetString("Error. {0} cannot save video."), "mplayer");

	public bool Running;

	protected Process process;
	protected string videoDevice;
	protected StreamWriter streamWriter;
	protected string captureExecutable = "";


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
	 * protected methods
	 */

	// convert /dev/video0 to _dev_video0
	protected string videoDeviceToFilename()
	{
		return Util.ChangeChars(videoDevice, "/", "_");
	}

	protected abstract void deleteTempFiles();
}

/*
 * this class contains select which webcam class should be used and how many cameras
 */
public class WebcamManage
{
	Webcam webcam;
	Webcam webcam2;
	//TODO: implement an List<T> of objects containing webcam and video device

	public WebcamManage()
	{
	}

	// 1 camera
	public Webcam.Result RecordPrepare (string videoDevice)
	{
		return recordPrepareDo (ref webcam, videoDevice);
	}
	// 2 cameras
	public Webcam.Result RecordPrepare (string videoDevice, string videoDevice2)
	{
		Webcam.Result result1 = recordPrepareDo (ref webcam, videoDevice);
		Webcam.Result result2 = recordPrepareDo (ref webcam2, videoDevice2);

		return new Webcam.Result (
			result1.success && result2.success,
			result1.output + result2.output,
			result1.error + result2.error
			);
	}
	private Webcam.Result recordPrepareDo (ref Webcam w, string videoDevice)
	{
		if(videoDevice == "" || videoDevice == "0")
		{
			new DialogMessage(Constants.MessageTypes.WARNING, "Video device is not configured. Check Preferences / Multimedia.");
			return new Webcam.Result (false, "");
		}

		//w = new Webcam(preferences.videoDevice);
		LogB.Information("wRS at gui chronojump.cs 0, videoDevice: " + videoDevice);

		//w = new WebcamMplayer (videoDevice);
		w = new WebcamFfmpeg (videoDevice);
		Webcam.Result result = w.CapturePrepare (Webcam.CaptureTypes.VIDEO);

		LogB.Information("wRS at gui chronojump.cs 1, videoDevice: " + videoDevice);
		return result;
	}

	public void RecordStart (int ncams)
	{
		recordStartDo (ref webcam);
		if(ncams > 1)
			recordStartDo (ref webcam2);
	}
	private void recordStartDo (ref Webcam webcam)
	{
		webcam.VideoCaptureStart();
	}

	public Webcam.Result RecordEnd(int ncam)
	{
		if(ncam == 1)
		{
			if(! webcam.Running)
				return new Webcam.Result (false, "");

			return recordEndDo (ref webcam);
		} else //(ncam == 2)
		{
			if(! webcam2.Running)
				return new Webcam.Result (false, "");

			return recordEndDo (ref webcam2);
		}
	}
	private Webcam.Result recordEndDo (ref Webcam webcam)
	{
		LogB.Information("webcamRecordEnd call 0");
		Webcam.Result result = webcam.VideoCaptureEnd ();

		LogB.Information("webcamRecordEnd call 1");
		if(! result.success)
			return result;

		LogB.Information("webcamRecordEnd call 2");
		return result;
	}

	public Webcam.Result ExitAndFinish (int ncam, int sessionID, Constants.TestTypes testType, int testID)
	{
		if(ncam == 1)
			return exitAndFinishDo (ref webcam, sessionID, testType, testID);
		else //ncam == 2
			return exitAndFinishDo (ref webcam2, sessionID, testType, testID);
	}

	private Webcam.Result exitAndFinishDo (ref Webcam webcam, int sessionID, Constants.TestTypes testType, int testID)
	{
		return webcam.ExitAndFinish (sessionID, testType, testID);
	}
}
