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
 *  Copyright (C) 2018-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System.Collections.Generic; //List
using System.Diagnostics;
using System;
using System.IO;
using System.Text.RegularExpressions; //Regex
using Mono.Unix;

//note the stdout and stderr redirection to false is to fix problems with windows

public class WebcamFfmpeg : Webcam
{
	private UtilAll.OperatingSystems os;
	private int processID;
	private Action action;

	// constructor ----------------------------------

	public WebcamFfmpeg (Webcam.Action action, UtilAll.OperatingSystems os, string videoDevice,
			string videoDevicePixelFormat, string videoDeviceResolution, string videoDeviceFramerate)
	{
		this.action = action;
		this.os = os;
		this.videoDevice = videoDevice;
		this.videoDevicePixelFormat = videoDevicePixelFormat;
		this.videoDeviceResolution = videoDeviceResolution;
		this.videoDeviceFramerate = videoDeviceFramerate;

		if(action == Webcam.Action.CAPTURE)
			executable = GetExecutableCapture (os);
		else // PLAYPREVIEW || PLAYFILE
			executable = GetExecutablePlay (os);

		Running = false;
	}

	// public methods ----------------------------------

	public static string GetExecutableCapture (UtilAll.OperatingSystems os)
	{
		string e = "ffmpeg";
		if(os == UtilAll.OperatingSystems.WINDOWS)
		{
			//if(System.Environment.Is64BitProcess)
				e = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffmpeg.exe");
			//else
			//	e = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/i386/ffmpeg.exe"); //i386 binaries are no longer updated (and included)
		}
		if(os == UtilAll.OperatingSystems.MACOSX)
			e = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffmpeg");

		return e;
	}

	public static string GetExecutablePlay (UtilAll.OperatingSystems os)
	{
		string e = "ffplay";
		if(os == UtilAll.OperatingSystems.WINDOWS)
		{
			//if(System.Environment.Is64BitProcess)
				e = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffplay.exe");
			//else
			//	e = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/i386/ffplay.exe");
		}
		if(os == UtilAll.OperatingSystems.MACOSX)
			e = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffplay");

		return e;
	}

	public static string GetExecutableProbe (UtilAll.OperatingSystems os)
	{
		string e = "ffprobe";
		if(os == UtilAll.OperatingSystems.WINDOWS)
		{
			//if(System.Environment.Is64BitProcess)
				e = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffprobe.exe");
			//else
			//	e = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/i386/ffprobe.exe");
		}
		if(os == UtilAll.OperatingSystems.MACOSX)
			e = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffprobe");

		return e;
	}

	public override Result CapturePrepare (CaptureTypes captureType)
	{
		if(process != null)
			return new Result (false, "");

		return new Result (true, "");
	}

	public override Result PlayPreview ()
	{
		if(process != null)
			return new Result (false, "");

		//cannot play preview with camera recording
		if (ExecuteProcess.IsRunning3 (-1, GetExecutableCapture(os)))
			return new Result (false, "", "ffmpeg is already running");

		List<string> parameters = createParametersPlayPreview();

		process = new Process();
		bool success = ExecuteProcess.RunAtBackground (ref process, executable, parameters, true, false, true, false, false);
		if(! success)
		{
			process = null;
			return new Result (false, "", programFfplayNotInstalled);
		}

		streamWriter = process.StandardInput;
		Running = true;
		return new Result (true, "");
	}
	public override Result PlayPreviewNoBackground () //experimental
	{
		//cannot play preview with camera recording
		if (ExecuteProcess.IsRunning3 (-1, GetExecutableCapture(os)))
			return new Result (false, "", "ffmpeg is already running");

		List<string> parameters = createParametersPlayPreview();

		process = new Process();
		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, parameters, false, false);
		if(! execute_result.success)
		{
			return new Result (false, "", programFfplayNotInstalled);
		}

		return new Result (true, "");
	}
	//used to know "Supported modes" on mac
	public override Result PlayPreviewNoBackgroundWantStdoutAndStderr() //experimental
	{
		List<string> parameters = createParametersPlayPreview();

		process = new Process();
		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, parameters, true, true);
		//LogB.Information("Stdout: ", execute_result.stdout);
		//LogB.Information("Stderr: ", execute_result.stderr);
		LogB.Information("allOutput: ", execute_result.allOutput);

		if(! execute_result.success)
			return new Result (false, execute_result.allOutput);

		return new Result (true, execute_result.allOutput);
	}

	//snapshot in 2 seconds
	public override bool Snapshot ()
	{
		executable = "ffmpeg";
		if(os == UtilAll.OperatingSystems.WINDOWS)
		{
			//if(System.Environment.Is64BitProcess)
				executable = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffmpeg.exe");
			//else
			//	executable = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/i386/ffmpeg.exe");
		}
		if(os == UtilAll.OperatingSystems.MACOSX)
			executable = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffmpeg");

		List<string> parameters = createParametersSnapshot(true); //force size

		process = new Process();
		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, parameters, false, false);

		//on a Windows tablet cannot change size on snapshot, so if snapshot is not successful, do it again wihout forcing size
		if(os == UtilAll.OperatingSystems.WINDOWS && ! execute_result.success)
		{
			parameters = createParametersSnapshot(false);
			execute_result = ExecuteProcess.run (executable, parameters, false, false);
		}

		return execute_result.success;
	}

	public override double FindVideoDuration (string filename)
	{
		// ffprobe -v 0 -show_entries format=duration -of compact=p=0:nk=1 filename
		if(filename == "")
			return -1;

		executable = GetExecutableProbe (os);

		List<string> parameters = new List<string> { "-v", "0", "-show_entries", "format=duration", "-of", "compact=p=0:nk=1", filename };
		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, parameters, true, false);
		if(! execute_result.success)
			return -1;

		LogB.Information ("execute_result stdout:" + execute_result.stdout);

		string stdoutConverted = Util.ChangeDecimalSeparator (execute_result.stdout);
		if (Util.IsNumber (stdoutConverted, true))
			return Convert.ToDouble (stdoutConverted);

		return 0;
	}

	public override int FindVideoFrames (string filename)
	{
		// ffprobe -v 0 -count_frames -show_entries stream=nb_read_frames FORCESENSOR-3141.mp4
		if(filename == "")
			return -1;

		executable = GetExecutableProbe (os);

		List<string> parameters = new List<string> { "-v", "0", "-count_frames", "-show_entries", "stream=nb_read_frames", filename };

		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, parameters, true, false);
		if(! execute_result.success)
			return -1;

		LogB.Information ("execute_result stdout:" + execute_result.stdout);

		return ffprobeMatchFrames (execute_result.stdout);
	}

	public override Result PlayFile (string filename)
	{
		if(process != null || filename == "")
			return new Result (false, "");

		List<string> parameters = createParametersPlayFile (filename);

		process = new Process();

		//ffplay returns the stats play time in stderr
		process.ErrorDataReceived += new DataReceivedEventHandler (stderrHandler);

		bool success = ExecuteProcess.RunAtBackground (ref process, executable, parameters, true, false, false, false, true);
		if(! success)
		{
			process = null;
			return new Result (false, "", programFfplayNotInstalled);
		}

		process.BeginErrorReadLine();

		Running = true;
		return new Result (true, "");
	}

	private void stderrHandler (object sendingProcess, DataReceivedEventArgs line)
	{
		if (line.Data != null && line.Data.Length > 0)
		{
			//LogB.Information (string.Format ("ffplay time: {0}", ffplayMatchTime (line.Data)));
			playVideoGetSecond = ffplayMatchTime (line.Data);
		} else 
			playVideoGetSecond = -1;
	}
	private double ffplayMatchTime (string l)
	{
		Match match = Regex.Match(l, @"(\d+).(\d+) M-V");

		//LogB.Information("fps match group count is 3?", (match.Groups.Count == 3).ToString());
		if(match.Groups.Count == 3)
		{
			string str = string.Format ("{0}.{1}", match.Groups[1].Value, match.Groups[2].Value);
			str = Util.ChangeDecimalSeparator (str);
			if (Util.IsNumber (str, true))
				return Convert.ToDouble (str);
		}

		return 0;
	}

	private int ffprobeMatchFrames (string l)
	{
		Match match = Regex.Match(l, @"nb_read_frames=(\d+)");

		//LogB.Information("frames match group count is 2?", (match.Groups.Count == 2).ToString());
		if(match.Groups.Count == 2)
		{
			string str = string.Format ("{0}", match.Groups[1].Value);
			if (Util.IsNumber (str, false))
				return Convert.ToInt32 (str);
		}

		return 0;
	}

	public override Result VideoCaptureStart()
	{
		/*
		 * TODO: apply also this, but right now we will just unsensitive:
		 * execute test (contacts and encoder)
		 * and other camera related buttons
		 * is complicated because button_execute_test.Sensitive = true happens at different places
		 * also having this code will help to prevent problems of ffmpeg working all the time and filling users HD
		 *
		 * TODO: do some check related to ffmpeg capture time length
		 */
		//check if process is running from a previous call:
		if(ExecuteProcess.IsRunning3 (-1, executable))
			return new Result (false, "", "ffmpeg is already running");

		//Delete temp video if exists
		deleteTempFiles();

		process = new Process();
		List<string> parameters = createParametersOnlyCapture();
		//List<string> parameters = createParametersCaptureAndDelayedView();
		bool success = ExecuteProcess.RunAtBackground (ref process, executable, parameters, true, false, true, false, false); //redirectInput, not output, not stderr
		if(! success)
		{
			streamWriter = null;
			process = null;
			return new Result (false, "", programFfmpegNotInstalled);
		}

		processID = process.Id;
		streamWriter = process.StandardInput;
		Running = true;

		return new Result (true, "");
	}

	// private methods ----------------------------------

	private List<string> createParametersPlayPreview()
	{
		// ffplay /dev/video0
		List<string> parameters = new List<string>();
		int i=0;

		parameters.Insert (i ++, "-f");
		if(os == UtilAll.OperatingSystems.LINUX)
			parameters.Insert (i ++, "v4l2");
		else if (os == UtilAll.OperatingSystems.WINDOWS)
			parameters.Insert (i ++, "dshow");
		else 	//mac
			parameters.Insert (i ++, "avfoundation");

		parameters.Insert (i ++, "-framerate");
		if(videoDeviceFramerate != "")
			parameters.Insert (i ++, videoDeviceFramerate);
		else
			parameters.Insert (i ++, "30");

		parameters.Insert (i ++, "-video_size");
		if(videoDeviceResolution != "")
			parameters.Insert (i ++, videoDeviceResolution);
		else
			parameters.Insert (i ++, "640x480");

		if(videoDevicePixelFormat != "")
		{
			if(os == UtilAll.OperatingSystems.LINUX) {
				parameters.Insert (i ++, "-input_format");
				parameters.Insert (i ++, videoDevicePixelFormat);
			} else if(os == UtilAll.OperatingSystems.WINDOWS) {
				parameters.Insert (i ++, "-pixel_format");
				parameters.Insert (i ++, videoDevicePixelFormat);
			}
		}

		if(os == UtilAll.OperatingSystems.LINUX)
			parameters.Insert (i ++, videoDevice);
		else if (os == UtilAll.OperatingSystems.WINDOWS)
			parameters.Insert (i ++, "video=" + videoDevice);
		else {	//mac
			parameters.Insert (i ++, "-i");
			parameters.Insert (i ++, videoDevice);
		}

		parameters.Insert (i++, "-exitonkeydown");
		parameters.Insert (i++, "-exitonmousedown");
		parameters.Insert (i++, "-window_title");
		parameters.Insert (i++, Catalog.GetString("Preview. Press any key to exit."));
		return parameters;
	}

	//ffmpeg -f v4l2 -s 400x400 -i /dev/video0 -ss 0:0:2 -frames 1 /tmp/out.jpg
	private List<string> createParametersSnapshot(bool forceSize)
	{
		// ffplay /dev/video0
		List<string> parameters = new List<string>();
		int i=0;

		parameters.Insert (i ++, "-f");
		if(os == UtilAll.OperatingSystems.LINUX)
			parameters.Insert (i ++, "v4l2");
		else if (os == UtilAll.OperatingSystems.WINDOWS)
			parameters.Insert (i ++, "dshow");
		else 	//mac
			parameters.Insert (i ++, "avfoundation");

		if(forceSize)
		{
			parameters.Insert (i ++, "-s");
			parameters.Insert (i ++, "400x400");
		}

		parameters.Insert (i ++, "-i");
		if(os == UtilAll.OperatingSystems.LINUX)
			parameters.Insert (i ++, videoDevice);
		else if (os == UtilAll.OperatingSystems.WINDOWS)
			parameters.Insert (i ++, "video=" + videoDevice);
		else	//mac
			parameters.Insert (i ++, videoDevice);

		//on Linux, at least at developer laptop, -ss is not working
		if(os != UtilAll.OperatingSystems.LINUX)
		{
			parameters.Insert (i ++, "-ss");
			parameters.Insert (i ++, "0:0:2");
		}

		parameters.Insert (i ++, "-frames");
		parameters.Insert (i ++, "1");
		parameters.Insert (i ++, Util.GetWebcamPhotoTempFileNamePost());
		parameters.Insert (i ++, "-y"); //overwrite

		return parameters;
	}

	private List<string> createParametersPlayFile(string filename)
	{
		// ffplay -autoexit out.mp4
		// experimental on Linux to show pts time (not length percentage)
		// ffplay -vf "drawtext=text='%{pts\:hms}':box=1:x=(w-tw)/2:y=h-(2*lh)" -autoexit out.mp4

		List<string> parameters = new List<string>();
		int i=0;

		if(os == UtilAll.OperatingSystems.LINUX) { //TODO: check if this works on Mac and Windows
			parameters.Insert (i++, "-vf");
			parameters.Insert (i++, "[in]drawtext=text='%{pts\\:hms}':box=1:x=(w-tw)/2:y=h-(3*lh), drawtext=text='pause\\: p, space; next frame\\: s; -+10s\\: left/right':box=1:x=(w-tw)/2:y=h-(lh)[out]");
		}

		parameters.Insert (i ++, "-autoexit");
		parameters.Insert (i ++, filename);

		/*
		 * these parameters allow to have:
		 * 4.70 M-V:  0.034 fd=   0 aq=    0KB vq=  148KB sq=    0B f=0/0
		 * being 4.70 the time in seconds with two decimals
		 * https://gitlab.gnome.org/GNOME/chronojump/-/issues/29
		 */
		parameters.Insert (i ++, "-hide_banner");
		parameters.Insert (i ++, "-loglevel");
		parameters.Insert (i ++, "8");
		parameters.Insert (i ++, "-stats");

		return parameters;
	}

	private List<string> createParametersOnlyCapture()
	{
		// ffmpeg -y -f v4l2 -framerate 30 -video_size 640x480 -input_format mjpeg -i /dev/video0 out.mp4
		List<string> parameters = new List<string>();

		int i = 0;
		parameters.Insert (i ++, "-y"); //overwrite

		parameters.Insert (i ++, "-f");
		if(os == UtilAll.OperatingSystems.LINUX)
			parameters.Insert (i ++, "v4l2");
		else if (os == UtilAll.OperatingSystems.WINDOWS)
			parameters.Insert (i ++, "dshow");
		else 	//mac
			parameters.Insert (i ++, "avfoundation");

		parameters.Insert (i ++, "-framerate");
		//on mac and linux framerate comes on all languages with . as decimal separator
		//if(videoDeviceFramerate != "" && Util.IsNumber(videoDeviceFramerate, false))
		if(videoDeviceFramerate != "")
			parameters.Insert (i ++, videoDeviceFramerate);
		else
			parameters.Insert (i ++, "30");

		parameters.Insert (i ++, "-video_size");
		if(videoDeviceResolution != "")
			parameters.Insert (i ++, videoDeviceResolution);
		else
			parameters.Insert (i ++, "640x480");

		if(videoDevicePixelFormat != "")
		{
			if(os == UtilAll.OperatingSystems.LINUX) {
				parameters.Insert (i ++, "-input_format");
				parameters.Insert (i ++, videoDevicePixelFormat);
			} else if(os == UtilAll.OperatingSystems.WINDOWS) {
				parameters.Insert (i ++, "-pixel_format");
				parameters.Insert (i ++, videoDevicePixelFormat);
			}
		}

		parameters.Insert (i ++, "-i");
		if(os == UtilAll.OperatingSystems.LINUX)
			parameters.Insert (i ++, videoDevice);
		else if (os == UtilAll.OperatingSystems.WINDOWS)
			parameters.Insert (i ++, "video=" + videoDevice);
		else 	//mac
			parameters.Insert (i ++, videoDevice);

		parameters.Insert (i ++, Util.GetVideoTempFileName());

		return parameters;
	}

	//Care: press q two times, one for each process on the tee
	//or only one on the ffplay process
	private List<string> createParametersCaptureAndDelayedView()
	{
		//ffmpeg -y -f v4l2 -i /dev/video0 -map 0 -c:v libx264 -f tee "output.mkv|[f=nut]pipe:" | ffplay pipe:
		List<string> parameters = new List<string>();

		int i = 0;
		parameters.Insert (i ++, "-y"); //overwrite

		parameters.Insert (i ++, "-f");
		if(os == UtilAll.OperatingSystems.LINUX)
			parameters.Insert (i ++, "v4l2");
		else if (os == UtilAll.OperatingSystems.WINDOWS)
			parameters.Insert (i ++, "dshow");
		else 	//mac
			parameters.Insert (i ++, "avfoundation");

		parameters.Insert (i ++, "-i");
		if(os == UtilAll.OperatingSystems.LINUX)
			parameters.Insert (i ++, videoDevice);
		else if (os == UtilAll.OperatingSystems.WINDOWS)
			parameters.Insert (i ++, "video=" + videoDevice);
		else	//mac
			parameters.Insert (i ++, videoDevice);

		parameters.Insert (i ++, "-map");
		parameters.Insert (i ++, "0");
		parameters.Insert (i ++, "-c:v");
		parameters.Insert (i ++, "libx264");
		parameters.Insert (i ++, "-f");
		parameters.Insert (i ++, "tee");

		//TODO: Think on the \" for windows and maybe also for other OSes
		parameters.Insert (i ++, "'" + Util.GetVideoTempFileName() + "|[f=nut]pipe:'");
		parameters.Insert (i ++, "|");
		parameters.Insert (i ++, "ffplay");
		parameters.Insert (i ++, "pipe:");

		return parameters;
	}

	/*
	 * there are problems calling the process with the "|"
	 * better call a shell script like this:
	 * ffmpeg_capture_and_play.sh
	 *
	 * #!/bin/bash
	 * ffmpeg -y -f v4l2 -i /dev/video0 -map 0 -c:v libx264 -f tee "/tmp/chronojump-last-video.mp4|[f=nut]pipe:" | ffplay pipe:
	 */


	/* unused on ffmpeg
	public override Result VideoCaptureEnd()
	{
		//on ffmpeg capture ends on exit: 'q' done at ExitAndFinish()
		return new Result (true, "");
	}
	*/

        //can pass a -1 uniqueID if test is cancelled
	public override Result SaveFile (int sessionID, Constants.TestTypes testType, int testID, bool moveTempFiles)
	{
		if(! moveTempFiles)
			return new Result (true, "");

		//Copy the video to expected place
		//but only if the test has not been cancelled
		if(testID != -1)
			if (! Util.CopyTempVideo(sessionID, testType, testID))
				return new Result (false, "", Constants.FileCopyProblemStr());

		//Delete temp video
		deleteTempFiles();

		return new Result (true, "");
	}

	public override void RecordingStop ()
	{
		LogB.Information("RecordingStop (was: Exit camera)");
		LogB.Information("streamWriter is null: " + (streamWriter == null).ToString());
		LogB.Information("Action: " + action.ToString());

		if(action == Action.PLAYPREVIEW || action == Action.PLAYFILE)
		{
			LogB.Information("killing ...");
			try {
				process.Kill();
			}
			catch {
				LogB.Information("catched!");
			}
			LogB.Information("done!");
		} else
		{ //action == Action.CAPTURE
			try {
				streamWriter.Write('q');
				streamWriter.Flush(); //seems is not needed
			} catch {
				//maybe capturer process (could be a window) has been closed by user
				process = null;
				Running = false;
				return;
			}

			LogB.Information("closing ...");
			process.Close();
			LogB.Information("done!");
		}

		if (action == Action.CAPTURE)
		{
			/*
			 * above process.Close() will end the process
			 * without using this file copied from /tmp maybe is not finished, so a bad ended file is copied to .local/share/Chronojump/multimedia/video
			 */

			bool exitBucle = false;
			do {
				LogB.Information("waiting 100 ms to tmp capture file being unlocked");
				System.Threading.Thread.Sleep(100);

				if (! File.Exists(Util.GetVideoTempFileName())) //PlayPreview does not have tmp file
					exitBucle = true;
				else if( ! ExecuteProcess.IsFileLocked(new System.IO.FileInfo(Util.GetVideoTempFileName())) ) //we are capturing, wait file is not locked
					exitBucle = true;
			} while(! exitBucle);

			do {
				LogB.Information("waiting 100 ms to end Ffmpeg");
				System.Threading.Thread.Sleep(100);
			} while(ExecuteProcess.IsRunning3(processID, "ffmpeg")); //note on Linux and Windows we need to check ffmpeg and not ffmpeg.exe
		}

		streamWriter = null;
		process = null;
		Running = false;
	}



	/*
	 * protected methods
	 */

	protected override void deleteTempFiles()
	{
		LogB.Information("Deleting temp video");
		if(File.Exists(Util.GetVideoTempFileName()))
			File.Delete(Util.GetVideoTempFileName());
	}
}
