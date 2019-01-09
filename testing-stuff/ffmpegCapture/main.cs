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

class FfmpegCapture
{
	private UtilAll.OperatingSystems os;
	private string captureExecutable = "ffmpeg";
        private StreamWriter streamWriter;
	private	Process process;
	private int processID;
	protected static internal string programFfmpegNotInstalled =
		string.Format("Error. {0} is not installed.", "ffmpeg");
	public bool Running;
        private string videoDevice;




	/*
	public void WebcamFfmpeg (UtilAll.OperatingSystems os, string videoDevice)
	{
		this.os = os;
		this.videoDevice = videoDevice;

		captureExecutable = "ffmpeg";
		if(os == UtilAll.OperatingSystems.WINDOWS)
			captureExecutable = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffmpeg.exe");

		Running = false;
	}
	*/

	/*
	 * constructor for Play

	public WebcamFfmpeg ()
	{
	}
	 */

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

        public enum CaptureTypes { PHOTO, VIDEO }

	public Result CapturePrepare (CaptureTypes captureType)
	{
		if(process != null)
			return new Result (false, "");

		return new Result (true, "");
	}

	public Result Play(string filename)
	{
		//only implemented on mplayer
		return new Result (true, "");
	}
	public bool Snapshot()
	{
		//only implemented on mplayer
		return true;
	}


	public static void Main(string[] args)
	{
		new FfmpegCapture(args);
	}

	public FfmpegCapture(string[] args)
	{
		if(args.Length != 1) {
			Console.WriteLine("Need to pass the videoDevice");
			return;
		}

                os = UtilAll.GetOSEnum();
		videoDevice = args[0];

		if(os == UtilAll.OperatingSystems.WINDOWS)
			captureExecutable = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffmpeg.exe");

		process = new Process();
		List<string> parameters = createParametersOnlyCapture();
		//List<string> parameters = createParametersCaptureAndDelayedView();
		bool success = ExecuteProcess.RunAtBackground (ref process, captureExecutable, parameters,
				true, false, true); //createNoWindow, useShellExecute, redirectInput
		if(! success)
		{
			streamWriter = null;
			process = null;
			//return new Result (false, "", programFfmpegNotInstalled);
			return;
		}

		processID = process.Id;
		streamWriter = process.StandardInput;
		Running = true;

		Console.WriteLine("Recording 5 seconds ...");
		for(int countA = 4; countA >= 0; countA --)
		{
			System.Threading.Thread.Sleep(1000);
			Console.WriteLine(countA.ToString());
		}
	
		int sessionID = 0;
		Constants.TestTypes testType = Constants.TestTypes.RUN;
		int testID = 1;
		ExitAndFinish (sessionID, testType, testID);

		Console.WriteLine("Recorded, copied, and deleted ok. Now we are going to play it");

		PlayFile(Util.GetVideoFileName(sessionID, testType, testID));

		//return new Result (true, "");
	}

       public Result PlayFile (string filename)
        {
		string executable = "ffplay";
		if(os == UtilAll.OperatingSystems.WINDOWS)
			executable = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffplay.exe");

                if(process != null || filename == "")
                        return new Result (false, "");

                List<string> parameters = createParametersPlayFile (filename);

                process = new Process();
                bool success = ExecuteProcess.RunAtBackground (ref process, executable, parameters, false, false, false);
                if(! success)
                {
                        process = null;
                        return new Result (false, "", programFfmpegNotInstalled);
                }

                Running = true;
                return new Result (true, "");
        }


	//TODO: on Windows or old machines adjust rtbufsize parameter, on our tests we have problems with default 3041280
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
			parameters.Insert (i ++, "\"" + videoDevice + "\"");
		else //windows
			parameters.Insert (i ++, "video=\"" + videoDevice + "\"");

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

	private List<string> createParametersPlayFile(string filename)
	{
		// ffplay out.mp4
		List<string> parameters = new List<string>();
		parameters.Insert (0, filename);
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


	public Result ExitAndFinish (int sessionID, Constants.TestTypes testType, int testID)
	{
		ExitCamera(); //this works

		//testing this now

		//Copy the video to expected place
		if (! Util.CopyTempVideo(sessionID, testType, testID))
			return new Result (false, "", Constants.FileCopyProblem);

		//Delete temp video
		deleteTempFiles();

		return new Result (true, "");
	}

	public void ExitCamera()
	{
		Console.WriteLine("Exit camera");
		Console.WriteLine("streamWriter is null: " + (streamWriter == null).ToString());
		try {
			streamWriter.Write('q'); //encara que hi hagi el process.Close() de sota, no es tanca la càmera sense aquest 'q'
			//streamWriter.Flush();
//			streamWriter.WriteLine("\x3");
			//streamWriter.Flush();
			//process.StandardInput.WriteLine("\x3");
//			streamWriter.WriteLine("q\n");
		} catch {
			//maybe Mplayer window has been closed by user
			process = null;
			Running = false;
			return;
		}

		Console.WriteLine("closing ...");
		process.Close();

		//this does not work
		//Console.WriteLine("waiting for exit ...");
		//process.WaitForExit();

		Console.WriteLine("done!");

		//amb el process.Close(); tanca i tanca be pero es perden fotogrames inicials i finals, vaja que no tanca massa be, no estic segur de si realment enviar la q serveix de algo, vaig a fer-ho sense la q
		//si que tanca be pq ara he provat sense enviar el q, nomes amb el process.Close() i la camera es queda oberta

		/*
		//System.Threading.Thread.Sleep(500);
		//better check if process still exists to later copy the video
		do {
			Console.WriteLine("waiting 100 ms to end Ffmpeg");
			System.Threading.Thread.Sleep(100);
		} while(ExecuteProcess.IsRunning2(process, captureExecutable));
		*/

		do {
			Console.WriteLine("waiting 100 ms to end Ffmpeg");
			System.Threading.Thread.Sleep(100);
                } while(ExecuteProcess.IsRunning3(processID, "ffmpeg")); //note on Linux and Windows we need to check ffmpeg and not ffmpeg.exe


		streamWriter = null;
		process = null;
		Running = false;
	}

	/*
	 * protected methods
	 */

	protected void deleteTempFiles()
	{
		Console.WriteLine("Deleting temp video");
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

                ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, parameters);

                Console.WriteLine("---- stdout: ----");
                Console.WriteLine(execute_result.stdout);
                Console.WriteLine("---- stderr: ----");
                Console.WriteLine(execute_result.stderr);
                Console.WriteLine("-----------------");

                if(! execute_result.success)
                {
                        Console.WriteLine("WebcamFfmpegGetDevicesWindows error: " + execute_result.stderr);

                        /*
                         * on Windows the -i dummy produces an error, so stderr exists and success is false
                         * stdout has the list of devices and stderr also
                         * check if in stdout there's the: "DirectShow video devices" string and if not exists, really we have an error
                         */
                        if(execute_result.stdout != null && execute_result.stdout != "" &&
                                        execute_result.stdout.Contains("DirectShow video devices"))
                        {
                                Console.WriteLine("Calling parse with stdout");
                                return parse(execute_result.stdout);
                        }

                        if(execute_result.stderr != null && execute_result.stderr != "" &&
                                        execute_result.stderr.Contains("DirectShow video devices"))
                        {
                                Console.WriteLine("Calling parse with stderr");
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
                Console.WriteLine("Called parse");

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
                        Console.WriteLine("line: " + l);
                        foreach(Match match in Regex.Matches(l, "\"([^\"]*)\""))
                        {
                                //remove quotes from the match (at beginning and end) to add it in SQL
                                string s = match.ToString().Substring(1, match.ToString().Length -2);

                                Console.WriteLine("add match: " + s);
                                parsedList.Add(s);
                        }

                        //after the list of video devices comes the list of audio devices, skip it
                        if(l.Contains("DirectShow audio devices"))
                                break;
                }

                return parsedList;
        }
}

