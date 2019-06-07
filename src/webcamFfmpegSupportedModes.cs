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
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections.Generic; //List<T>


public class WebcamFfmpegSupportedModes 
{
	private string modesStr;
	private string errorStr;

	//for mac and maybe windows, because in Linux it founds a default mode and it works
	public WebcamFfmpegSupportedModes ()
	{
		modesStr = "";
		errorStr = "";
	}

	public void GetModes(UtilAll.OperatingSystems os, string cameraCode)
	{
		if(os == UtilAll.OperatingSystems.LINUX)
			getModesLinux();
		else if(os == UtilAll.OperatingSystems.WINDOWS)
			getModesWindows(cameraCode);
		else if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.MACOSX)
			getModesMac(cameraCode);
	}

	private void getModesLinux()
	{
		List<string> parameters = new List<string>();
		parameters.Add("--list-formats-ext");
		ExecuteProcess.Result execute_result = ExecuteProcess.run ("v4l2-ctl", parameters, true, true);
		if(! execute_result.success) {
			errorStr = "Need to install v4l2-ctl (on v4l-utils) to know modes";
			return;
		}

		modesStr = execute_result.stdout;
	}

	private void getModesWindows(string cameraCode)
	{
		string executable = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffmpeg.exe");
		//ffmpeg -f dshow -list_options true -i video="USB 2.0 WebCamera"
		List<string> parameters = new List<string>();
		parameters.Add("-f");
		parameters.Add("dshow");
		parameters.Add("-list_options");
		parameters.Add("true");
		parameters.Add("-i");
		parameters.Add("video=" + cameraCode);
		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, parameters, true, true);

		//TODO: check if ffmpeg installed, but take care because right now this always gets error, so we need to not return
		/*
		   if(! execute_result.success) {
		   new DialogMessage("Chronojump - Modes of this webcam",
		   Constants.MessageTypes.WARNING, "Need to install ffmpeg");
		   return;
		   }
		   */

		//modesStr = execute_result.stdout;
		modesStr = execute_result.allOutput;
	}

	private void getModesMac(string cameraCode)
	{
		//select and impossible mode just to get an error on mac, this error will give us the "Supported modes"
		Webcam webcamPlay = new WebcamFfmpeg (Webcam.Action.PLAYPREVIEW, UtilAll.GetOSEnum(),
				cameraCode, "8000x8000", "8000");

		Webcam.Result result = webcamPlay.PlayPreviewNoBackgroundWantStdoutAndStderr();
		modesStr = result.output;
	}

	public string ErrorStr
	{
		get { return errorStr;  }
	}

	public string ModesStr
	{
		get { return modesStr;  }
	}
}
