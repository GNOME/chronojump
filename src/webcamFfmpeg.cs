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
	private Action action;

	// constructor ----------------------------------

	public WebcamFfmpeg (Webcam.Action action, UtilAll.OperatingSystems os, string videoDevice, string videoDeviceResolution, string videoDeviceFramerate)
	{
		this.action = action;
		this.os = os;
		this.videoDevice = videoDevice;
		this.videoDeviceResolution = videoDeviceResolution;
		this.videoDeviceFramerate = videoDeviceFramerate;

		if(action == Webcam.Action.CAPTURE)
		{
			executable = "ffmpeg";
			if(os == UtilAll.OperatingSystems.WINDOWS)
				executable = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffmpeg.exe");
			if(os == UtilAll.OperatingSystems.MACOSX)
				executable = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffmpeg");
		}
		else // PLAYPREVIEW || PLAYFILE
		{
			executable = "ffplay";
			if(os == UtilAll.OperatingSystems.WINDOWS)
				executable = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffplay.exe");
			if(os == UtilAll.OperatingSystems.MACOSX)
				executable = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffplay");
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
			return new Result (false, "", programFfplayNotInstalled);
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

		string parsed = parseSupportedModes(execute_result.allOutput);
		//use this to test the parsing method
		//string parsed = parseSupportedModes(parseSupportedModesTestString);

		if(! execute_result.success)
		{
			return new Result (false, parsed);
		}

		return new Result (true, parsed);
	}

	//snapshot in 2 seconds
	public override bool Snapshot ()
	{
		executable = "ffmpeg";
		if(os == UtilAll.OperatingSystems.WINDOWS)
			executable = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffmpeg.exe");
		if(os == UtilAll.OperatingSystems.MACOSX)
			executable = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffmpeg");

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
			return new Result (false, "", programFfplayNotInstalled);
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
		else if (os == UtilAll.OperatingSystems.WINDOWS)
			parameters.Insert (i ++, "dshow");
		else 	//mac
			parameters.Insert (i ++, "avfoundation");

		parameters.Insert (i ++, "-s");
		parameters.Insert (i ++, "400x400");

		parameters.Insert (i ++, "-i");
		if(os == UtilAll.OperatingSystems.LINUX)
			parameters.Insert (i ++, videoDevice);
		else if (os == UtilAll.OperatingSystems.WINDOWS)
			parameters.Insert (i ++, "video=" + videoDevice);
		else	//mac
			parameters.Insert (i ++, videoDevice);

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
		else if (os == UtilAll.OperatingSystems.WINDOWS)
			parameters.Insert (i ++, "dshow");
		else 	//mac
			parameters.Insert (i ++, "avfoundation");

		parameters.Insert (i ++, "-framerate");
		if(videoDeviceFramerate != "" && Util.IsNumber(videoDeviceFramerate, false))
			parameters.Insert (i ++, videoDeviceFramerate);
		else
			parameters.Insert (i ++, "30");

		parameters.Insert (i ++, "-video_size");
		if(videoDeviceResolution != "")
			parameters.Insert (i ++, videoDeviceResolution);
		else
			parameters.Insert (i ++, "640x480");

		if(os == UtilAll.OperatingSystems.LINUX) {
			parameters.Insert (i ++, "-input_format");
			parameters.Insert (i ++, "mjpeg");
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

	private string parseSupportedModes(string allOutput)
	{
		string parsedAll = "Resolution    Framerate\n";

		/*
		 * break the big string in \n strings
		 * https://stackoverflow.com/a/1547483
		 */
		string[] lines = allOutput.Split(
				new[] { Environment.NewLine },
				StringSplitOptions.None
				);

		bool started = false;
		foreach(string l in lines)
		{
			LogB.Information("line: " + l);

			//devices start after the videoDevString line
			if(! started)
			{
				if(l.Contains("Supported modes"))
					started = true;

				continue;
			}

			string parsedLine = parseSupportedMode(l);
			if(parsedLine != "")
				parsedAll += parsedLine + "\n";

			//after the list of video devices comes the list of audio devices, skip it
			if(l.Contains("Input/output"))
				break;
		}
		return parsedAll;
	}
	private string parseSupportedMode(string l) //TODO: currently only for mac
	{
		if(! l.Contains("avfoundation"))
			return "";

		//parse this:
		//	[avfoundation @ 0x7f849a8be800]   1280x720@[23.999981 23.999981]fps
		//use: https://regex101.com/r/lZ5mN8/50
		// 	(\d+)x(\d+)@\[(\d+).(\d+)\s+

		Match match = Regex.Match(l, @"(\d+)x(\d+)@\[(\d+).(\d+)\s+");

		//TODO: use these lines
		//LogB.Information("match group count: ", match.Groups.Count.ToString());
		//if(match.Groups.Count != 5) //first is all match, second is the first int (width), last one is the decimals of the resolution
		//	return "";
		LogB.Information("match group count is 5?", (match.Groups.Count == 5).ToString());
		LogB.Information("match group count is -5?", (match.Groups.Count == -5).ToString());

		return string.Format("{0}x{1}    {2}.{3}", //resolution    framerate
				match.Groups[1].Value, match.Groups[2].Value,
				match.Groups[3].Value, match.Groups[4].Value);
	}

	// test ParseSupportModes
	private string parseSupportedModesTestString = @"Supported modes:
[avfoundation @ 0x7f849a8be800]   160x120@[29.970000 29.970000]fps
[avfoundation @ 0x7f849a8be800]   160x120@[25.000000 25.000000]fps
[avfoundation @ 0x7f849a8be800]   160x120@[23.999981 23.999981]fps
[avfoundation @ 0x7f849a8be800]   160x120@[14.999993 14.999993]fps
[avfoundation @ 0x7f849a8be800]   176x144@[29.970000 29.970000]fps
[avfoundation @ 0x7f849a8be800]   176x144@[25.000000 25.000000]fps
[avfoundation @ 0x7f849a8be800]   176x144@[23.999981 23.999981]fps
[avfoundation @ 0x7f849a8be800]   176x144@[14.999993 14.999993]fps
[avfoundation @ 0x7f849a8be800]   320x240@[29.970000 29.970000]fps
[avfoundation @ 0x7f849a8be800]   320x240@[25.000000 25.000000]fps
[avfoundation @ 0x7f849a8be800]   320x240@[23.999981 23.999981]fps
[avfoundation @ 0x7f849a8be800]   320x240@[14.999993 14.999993]fps
[avfoundation @ 0x7f849a8be800]   352x288@[29.970000 29.970000]fps
[avfoundation @ 0x7f849a8be800]   352x288@[25.000000 25.000000]fps
[avfoundation @ 0x7f849a8be800]   352x288@[23.999981 23.999981]fps
[avfoundation @ 0x7f849a8be800]   352x288@[14.999993 14.999993]fps
[avfoundation @ 0x7f849a8be800]   640x480@[29.970000 29.970000]fps
[avfoundation @ 0x7f849a8be800]   640x480@[25.000000 25.000000]fps
[avfoundation @ 0x7f849a8be800]   640x480@[23.999981 23.999981]fps
[avfoundation @ 0x7f849a8be800]   640x480@[14.999993 14.999993]fps
[avfoundation @ 0x7f849a8be800]   960x540@[29.970000 29.970000]fps
[avfoundation @ 0x7f849a8be800]   960x540@[25.000000 25.000000]fps
[avfoundation @ 0x7f849a8be800]   960x540@[23.999981 23.999981]fps
[avfoundation @ 0x7f849a8be800]   960x540@[14.999993 14.999993]fps
[avfoundation @ 0x7f849a8be800]   1024x576@[29.970000 29.970000]fps
[avfoundation @ 0x7f849a8be800]   1024x576@[25.000000 25.000000]fps
[avfoundation @ 0x7f849a8be800]   1024x576@[23.999981 23.999981]fps
[avfoundation @ 0x7f849a8be800]   1024x576@[14.999993 14.999993]fps
[avfoundation @ 0x7f849a8be800]   1280x720@[29.970000 29.970000]fps
[avfoundation @ 0x7f849a8be800]   1280x720@[25.000000 25.000000]fps
[avfoundation @ 0x7f849a8be800]   1280x720@[23.999981 23.999981]fps
[avfoundation @ 0x7f849a8be800]   1280x720@[14.999993 14.999993]fps
0: Input/output error";


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
