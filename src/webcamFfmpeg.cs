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
using System.Text.RegularExpressions; //Regex

//note the stdout and stderr redirection to false is to fix problems with windows

public class WebcamFfmpeg : Webcam
{
	private UtilAll.OperatingSystems os;
	private int processID;

	// constructor ----------------------------------

	public WebcamFfmpeg (Webcam.Action action, UtilAll.OperatingSystems os, string videoDevice)
	{
		this.os = os;
		this.videoDevice = videoDevice;

		if(action == Webcam.Action.CAPTURE)
		{
			executable = "ffmpeg";
			if(os == UtilAll.OperatingSystems.WINDOWS)
				executable = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffmpeg.exe");
		}
		else // PLAYPREVIEW || PLAYFILE
		{
			executable = "ffplay";
			if(os == UtilAll.OperatingSystems.WINDOWS)
				executable = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffplay.exe");
		}

		Running = false;
	}

	// public methods ----------------------------------

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

		List<string> parameters = createParametersPlayPreview();

		process = new Process();
		bool success = ExecuteProcess.RunAtBackground (ref process, executable, parameters, true, false, true, false, false);
		if(! success)
		{
			process = null;
			return new Result (false, "", programFfmpegNotInstalled);
		}

		streamWriter = process.StandardInput;
		Running = true;
		return new Result (true, "");
	}
	public override Result PlayPreviewNoBackground () //experimental
	{
		List<string> parameters = createParametersPlayPreview();

		process = new Process();
		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, parameters, false, false);
		if(! execute_result.success)
		{
			return new Result (false, "", programFfmpegNotInstalled);
		}

		return new Result (true, "");
	}

	//snapshot in 2 seconds
	public override bool Snapshot ()
	{
		executable = "ffmpeg";
		List<string> parameters = createParametersSnapshot();

		process = new Process();
		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, parameters, false, false);
		return execute_result.success;
	}



	public override Result PlayFile (string filename)
	{
		if(process != null || filename == "")
			return new Result (false, "");

		List<string> parameters = createParametersPlayFile (filename);

		process = new Process();
		bool success = ExecuteProcess.RunAtBackground (ref process, executable, parameters, true, false, false, false, false);
		if(! success)
		{
			process = null;
			return new Result (false, "", programFfmpegNotInstalled);
		}

		Running = true;
		return new Result (true, "");
	}

	public override Result VideoCaptureStart()
	{
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
		else 	//windows
			parameters.Insert (i ++, "dshow");

		if(os == UtilAll.OperatingSystems.LINUX)
			parameters.Insert (i ++, videoDevice);
		else
			parameters.Insert (i ++, "video=" + videoDevice);

		parameters.Insert (i++, "-exitonkeydown");
		parameters.Insert (i++, "-exitonmousedown");
		parameters.Insert (i++, "-window_title");
		parameters.Insert (i++, "Preview. Press any key to exit.");
		return parameters;
	}

	//ffmpeg -f v4l2 -s 400x400 -i /dev/video0 -ss 0:0:2 -frames 1 /tmp/out.jpg
	private List<string> createParametersSnapshot()
	{
		// ffplay /dev/video0
		List<string> parameters = new List<string>();
		int i=0;

		parameters.Insert (i ++, "-f");
		if(os == UtilAll.OperatingSystems.LINUX)
			parameters.Insert (i ++, "v4l2");
		else 	//windows
			parameters.Insert (i ++, "dshow");

		parameters.Insert (i ++, "-s");
		parameters.Insert (i ++, "400x400");

		parameters.Insert (i ++, "-i");
		if(os == UtilAll.OperatingSystems.LINUX)
			parameters.Insert (i ++, videoDevice);
		else
			parameters.Insert (i ++, "video=" + videoDevice);

		parameters.Insert (i ++, "-ss");
		parameters.Insert (i ++, "0:0:2");
		parameters.Insert (i ++, "-frames");
		parameters.Insert (i ++, "1");
		parameters.Insert (i ++, Util.GetWebcamPhotoTempFileNamePost(videoDeviceToFilename()));
		parameters.Insert (i ++, "-y"); //overwrite

		return parameters;
	}

	private List<string> createParametersPlayFile(string filename)
	{
		// ffplay out.mp4
		List<string> parameters = new List<string>();
		parameters.Insert (0, filename);
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
		else //windows
			parameters.Insert (i ++, "dshow");

		parameters.Insert (i ++, "-framerate");
		parameters.Insert (i ++, "30");
		parameters.Insert (i ++, "-video_size");
		parameters.Insert (i ++, "640x480");

		if(os == UtilAll.OperatingSystems.LINUX) {
			parameters.Insert (i ++, "-input_format");
			parameters.Insert (i ++, "mjpeg");
		}

		parameters.Insert (i ++, "-i");
		if(os == UtilAll.OperatingSystems.LINUX)
			parameters.Insert (i ++, videoDevice);
		else //windows
			parameters.Insert (i ++, "video=" + videoDevice);

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
		else //windows
			parameters.Insert (i ++, "dshow");

		parameters.Insert (i ++, "-i");
		if(os == UtilAll.OperatingSystems.LINUX)
			parameters.Insert (i ++, videoDevice);
		else //windows
			parameters.Insert (i ++, "video=" + videoDevice);

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


	public override Result VideoCaptureEnd()
	{
		//on ffmpeg capture ends on exit: 'q' done at ExitAndFinish()
		return new Result (true, "");
	}

        //can pass a -1 uniqueID if test is cancelled
	public override Result ExitAndFinish (int sessionID, Constants.TestTypes testType, int testID, bool moveTempFiles)
	{
		ExitCamera();

		if(! moveTempFiles)
			return new Result (true, "");

		//Copy the video to expected place
		//but only if the test has not been cancelled
		if(testID != -1)
			if (! Util.CopyTempVideo(sessionID, testType, testID))
				return new Result (false, "", Constants.FileCopyProblem);

		//Delete temp video
		deleteTempFiles();

		return new Result (true, "");
	}

	public override void ExitCamera()
	{
		LogB.Information("Exit camera");
		LogB.Information("streamWriter is null: " + (streamWriter == null).ToString());
		try {
			streamWriter.Write('q');
			streamWriter.Flush(); //seems is not needed
		} catch {
			//maybe capturer process (could be a window) has been closed by user
			process = null;
			Running = false;
			return;
		}

                Console.WriteLine("closing ...");
                process.Close();
                Console.WriteLine("done!");

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


public static class WebcamFfmpegGetDevicesWindows
{
	public static List<string> GetDevices()
	{
		string executable = "ffmpeg";
		if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.WINDOWS)
			executable = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffmpeg.exe");

		List<string> parameters = createParameters();

		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, parameters, true, true);

		LogB.Information("---- stdout: ----");
		LogB.Information(execute_result.stdout);
		LogB.Information("---- stderr: ----");
		LogB.Information(execute_result.stderr);
		LogB.Information("-----------------");

		if(! execute_result.success)
		{
			LogB.Information("WebcamFfmpegGetDevicesWindows error: " + execute_result.stderr);

			/*
			 * on Windows the -i dummy produces an error, so stderr exists and success is false
			 * stdout has the list of devices and stderr also
			 * check if in stdout there's the: "DirectShow video devices" string and if not exists, really we have an error
			 */
			if(execute_result.stdout != null && execute_result.stdout != "" &&
					execute_result.stdout.Contains("DirectShow video devices"))
			{
				LogB.Information("Calling parse with stdout");
				return parse(execute_result.stdout);
			}

			if(execute_result.stderr != null && execute_result.stderr != "" &&
					execute_result.stderr.Contains("DirectShow video devices"))
			{
				LogB.Information("Calling parse with stderr");
				return parse(execute_result.stderr);
			}

			return new List<string>();
		}
		else
			return parse(execute_result.stdout);
	}

	private static List<string> createParameters()
	{
		//ffmpeg -list_devices true -f dshow -i dummy
		List<string> parameters = new List<string>();

		int i = 0;
		parameters.Insert (i ++, "-list_devices");
		parameters.Insert (i ++, "true");
		parameters.Insert (i ++, "-f");
		parameters.Insert (i ++, "dshow");
		parameters.Insert (i ++, "-i");
		parameters.Insert (i ++, "dummy");

		return parameters;
	}

	private static List<string> parse(string devicesOutput)
	{
		LogB.Information("Called parse");

		/*
		 * break the big string in \n strings
		 * https://stackoverflow.com/a/1547483
		 */
		string[] lines = devicesOutput.Split(
				new[] { Environment.NewLine },
				StringSplitOptions.None
				);

		List<string> parsedList = new List<string>();
		foreach(string l in lines)
		{
			LogB.Information("line: " + l);
			foreach(Match match in Regex.Matches(l, "\"([^\"]*)\""))
			{
				//remove quotes from the match (at beginning and end) to add it in SQL
				string s = match.ToString().Substring(1, match.ToString().Length -2);

				LogB.Information("add match: " + s);
				parsedList.Add(s);
			}

			//after the list of video devices comes the list of audio devices, skip it
			if(l.Contains("DirectShow audio devices"))
				break;
		}

		return parsedList;
	}
}
