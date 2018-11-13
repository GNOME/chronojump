/*

17 setembre 2018

ffmpeg builds on windows only work on Windows 7 and above
info en directshow per windows:
https://trac.ffmpeg.org/wiki/DirectShow
amb el del xavi padu el ffplay funciona pero el ffmpeg capture sembla que no va pq cal tocar el rtbufsize
sí que va, tot i que va millor amv un -rtbufsize 702000k
el problema es que el winwows media player no ho reprodueix, pero el ffplay sí (i el vlc també)

*****
superbo capture i play amb el play amb una mica de lag:
https://bbs.archlinux.org/viewtopic.php?id=225376
$ ffmpeg -f v4l2 -i /dev/video0 -map 0 -c:v libx264 -f tee "output.mkv|[f=nut]pipe:" | ffplay pipe:
*****

provant ara multicamera amb:
https://superuser.com/questions/1262690/capture-multiple-camera-in-sync
$ ffmpeg -rtbufsize 1M -r 30 -i /dev/video0 -rtbufsize 1M -r 30 -i /dev/video1 -framerate 30 -map 0 -y output1_1.mp4 -framerate 30 -map 1 -y output2_1.mp4
*/


//provant altre cop lo de la captura amb ffmpeg i ara va millor al parar, potser és per updates del kernel o pq els fps son més els de la camera:
//ffmpeg -f v4l2 -framerate 30 -video_size 640x480 -input_format mjpeg -i /dev/video0 out.mp4
//ffmpeg -f v4l2 -framerate 30 -video_size 640x480 -input_format mjpeg -i /dev/video0 out.mkv
//
//provar que hi hagi opcio de fer-ho amb visio al moment: mplayer
//o amb previsio mplayer i després captura ffmpeg sense visio

/* manera xul.la de fer tot amb mplayer i ffmpeg i alhora pero cal coses al kernel:
 * https://unix.stackexchange.com/questions/343832/how-to-read-a-webcam-that-is-already-used-by-a-background-capture
 * $ sudo modprobe v4l2loopback devices=1
 * si falla:
 * $ sudo apt-get install v4l2loopback-dkms
 * $ ffmpeg -f video4linux2  -i /dev/video0 -codec copy -f v4l2 /dev/video1
 * llavors a una terminal es pot fer:
 * $ mplayer -tv driver=v4l2:gain=1:width=400:height=400:device=/dev/video1:fps=30:outfmt=rgb16 tv://
 * i a l'altra:
 * $ ffmpeg -y -f v4l2 -r 25 -i /dev/video1 out.mp4
 el que no entenc és pq al final agafem les dos /dev/video1
 be, aixo es el que diu la web

 de fet es pot passar de mplayer i usar sempre ffmpeg i ffplay (els dos al paquet ffmpeg) per a veure mentre capturem:
 ffplay -f video4linux2 -video_size 400x400 -i /dev/video1
 i per a veure el video despres:
 ffplay out.mp4

 una altra solucio seria usant gstreamer camerabin2 que ho fa tot, pero està unstable:
 https://www.freedesktop.org/software/gstreamer-sdk/data/docs/2012.5/gst-plugins-bad-plugins-0.10/gst-plugins-bad-plugins-camerabin2.html

 tema diferents sistemes operatius:
 http://trac.ffmpeg.org/wiki/Capture/Webcam

 */

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
	protected string executable = "";


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
	private UtilAll.OperatingSystems os;
	//TODO: implement an List<T> of objects containing webcam and video device

	public WebcamManage()
	{
		os = UtilAll.GetOSEnum();
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
		w = new WebcamFfmpeg (os, videoDevice);
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
