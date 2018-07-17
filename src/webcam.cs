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


class Webcam
{
	public bool Running;

	private Process process;
	private StreamWriter streamWriter;

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
	 * constructor
	 */

	public Webcam()
	{
		Running = false;
	}

	/*
	 * public methods
	 */

	public Result MplayerCapture()
	{
		if(process != null)
			return new Result (false, "");

		string tempFile = Util.GetMplayerPhotoTempFileNamePost();
		Util.FileDelete(tempFile);

		//TODO: check /dev/video0 or video1 or nothing
		string executable = "mplayer";
		List<string> parameters = new List<string>();
		//-noborder -nosound -tv driver=v4l2:gain=1:width=400:height=400:device=/dev/video0:fps=10:outfmt=rgb16 tv:// -vf screenshot=/tmp/chronojump-last-photo
		parameters.Insert (0, "-noborder"); //on X11 can be: title "Chronojump"". -noborder makes no accept 's', or 'q'
		parameters.Insert (1, "-nosound");
		parameters.Insert (2, "-tv");
		parameters.Insert (3, "driver=v4l2:gain=1:width=400:height=400:device=/dev/video0:fps=10:outfmt=rgb16");
		parameters.Insert (4, "tv://");
		parameters.Insert (5, "-vf");
		parameters.Insert (6, "screenshot=" + Util.GetMplayerPhotoTempFileNamePre());

		process = new Process();
		bool success = ExecuteProcess.RunAtBackground (process, executable, parameters, true); //redirectInput
		if(! success)
		{
			streamWriter = null;
			process = null;
			return new Result (false, "", Constants.MplayerNotInstalled);
		}

		streamWriter = process.StandardInput;

		Running = true;
		return new Result (true, "");
	}

	public Result MplayerPlay(string filename)
	{
		if(process != null || filename == "")
			return new Result (false, "");

		string executable = "mplayer";
		List<string> parameters = new List<string>();
		//-noborder -nosound -tv driver=v4l2:gain=1:width=400:height=400:device=/dev/video0:fps=10:outfmt=rgb16 tv:// -vf screenshot=/tmp/ChronojumpPhoto
		parameters.Insert (0, filename);
		//parameters.Insert (0, "-noborder"); //on X11 can be: title "Chronojump"". -noborder makes no accept 's', or 'q'

		process = new Process();
		bool success = ExecuteProcess.RunAtBackground (process, executable, parameters, false);
		if(! success)
		{
			process = null;
			return new Result (false, "", Constants.MplayerNotInstalled);
		}

		Running = true;
		return new Result (true, "");
	}


	public bool Snapshot()
	{
		if(process == null || streamWriter == null)
			return false;

		bool exitAtFirstSnapshot = true;

		if(! snapshotDo())
			return false;

		if(exitAtFirstSnapshot)
		{
			streamWriter.Flush();
			System.Threading.Thread.Sleep(100);
			ExitCamera();
		}
		return true;
	}

	public bool RecordStart()
	{
		if(process == null || streamWriter == null)
			return false;

		if(! recordStartOrEndDo())
			return false;

		return true;
	}

	public bool RecordEnd(int sessionID, Constants.TestTypes testType, int testID)
	{
		if(process == null || streamWriter == null)
			return false;

		//System.Threading.Thread.Sleep(2000); //TODO: play with this to see if cut better video at end
		if(! recordStartOrEndDo())
			return false;

		ExitCamera();

		//Convert video to the name and format expected
		if(! convertImagesToVideo())
			return false;

		//Copy the video to expected place
		if (! Util.CopyTempVideo(sessionID, testType, testID))
			return false;

		//Delete temp photos and video
		Util.DeleteTempPhotosAndVideo();

		return true;
	}

	public void ExitCamera()
	{
		try {
			streamWriter.Write('q');
			streamWriter.Flush();
		} catch {
			//maybe Mplayer window has been closed by user
			process = null;
			Running = false;
			return;
		}
		System.Threading.Thread.Sleep(100);

		streamWriter = null;
		process = null;
		Running = false;
	}

	/*
	 * private methods
	 */

	private bool snapshotDo()
	{
		try {
			streamWriter.Write('s');
		} catch {
			//maybe Mplayer window has been closed by user
			return false;
		}
		return true;
	}

	private bool recordStartOrEndDo()
	{
		try {
			streamWriter.Write('S');
		} catch {
			//maybe Mplayer window has been closed by user
			return false;
		}
		return true;
	}

	private bool convertImagesToVideo()
	{
		string executable = "ffmpeg";
		List<string> parameters = new List<string>();
		//ffmpeg -framerate 20 -y -i chronojump-last-photo%04d.png output.mp4
		parameters.Insert (0, "-framerate");
		parameters.Insert (1, "20");
		parameters.Insert (2, "-y"); //force overwrite without asking
		parameters.Insert (3, "-i"); //input files
		parameters.Insert (4, Util.GetMplayerPhotoTempFileNamePre() + "%04d.png");
		parameters.Insert (5, Util.GetVideoTempFileName());

		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, parameters);
		return execute_result.success;
	}

}
