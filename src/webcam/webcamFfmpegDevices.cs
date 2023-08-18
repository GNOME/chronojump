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
 *  Copyright (C) 2019-2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System.Collections.Generic; //List
using System.Diagnostics;
using System;
using System.IO;
using System.Text.RegularExpressions; //Regex


public class WebcamDevice
{
	//on mac it is returned a code and a device, like:
	//[0] FaceTime HD Camera 	//this is the webcam
	//[1] Capture screen 0 		//this is to screencapture
	//
	//on Linux it is returned a filename (it will be the code)
	//code == fullname
	//
	//on windows the code is the long name with strange chars: stored on SQL
	//the fullname is the readable name that is seen on preferences combo
	//
	//object is a device and fullname
	private string code;
	private string fullname;

	public WebcamDevice(string code, string fullname)
	{
		this.code = code;
		this.fullname = fullname;
	}

	public string Code
	{
		get { return code; }
	}

	public string Fullname
	{
		get { return fullname; }
	}
}

public class WebcamDeviceList
{
	private List<WebcamDevice> wd_list;
	public string Error;

	// constructors

	public WebcamDeviceList()
	{
		Error = "";
		wd_list = new List<WebcamDevice>();
	}

	public WebcamDeviceList(List<WebcamDevice> wd_list)
	{
		Error = "";
		this.wd_list = wd_list;
	}

	// public methods

	public void Add(WebcamDevice wd)
	{
		wd_list.Add(wd);
	}

	public int Count()
	{
		return wd_list.Count;
	}

	public List<string> GetCodes()
	{
		List<string> l = new List<string>();
		foreach(WebcamDevice wd in wd_list)
			l.Add(wd.Code);

		return l;
	}

	public List<string> GetFullnames()
	{
		List<string> l = new List<string>();
		foreach(WebcamDevice wd in wd_list)
			l.Add(wd.Fullname);

		return l;
	}

	public string GetCodeOfFullname(string fullname)
	{
		foreach(WebcamDevice wd in wd_list)
			if(wd.Fullname == fullname)
				return wd.Code;

		return "";
	}

	public string GetFullnameOfCode(string code)
	{
		foreach(WebcamDevice wd in wd_list)
			if(wd.Code == code)
				return wd.Fullname;

		return "";
	}
}

public abstract class WebcamFfmpegGetDevices
{
	//protected List<WebcamDevice> webcamDevice;
	protected WebcamDeviceList wd_list;

	//update codes and names lists
	public abstract WebcamDeviceList GetDevices();

	protected void initialize ()
	{
		LogB.Information(" called initialize ");
		wd_list = new WebcamDeviceList();
	}
}

public class WebcamFfmpegGetDevicesLinux : WebcamFfmpegGetDevices
{
	public WebcamFfmpegGetDevicesLinux()
	{
		initialize();
	}

	public override WebcamDeviceList GetDevices()
	{
		LogB.Information("GetDevices");
		LogB.Information(string.Format("wd_list is null: ", wd_list == null));
		string prefix = "/dev/";
		var dir = new DirectoryInfo(prefix);
		bool found = false;
		foreach(var file in dir.EnumerateFiles("video*"))
		{
			/*
			 * return 0, 1, ...
			 if( file.Name.Length > 5 && 				//there's something more than "video", like "video0" or "video1", ...
			 char.IsNumber(file.Name, 5) ) 		//and it's a number
			 list.Add(Convert.ToInt32(file.Name[5])); 			//0 or 1, or ...
			 */
			//return "/dev/video0", "/dev/video1", ...
			wd_list.Add(new WebcamDevice(
						prefix + file.Name,
						//prefix + file.Name + " (default camera)"));
						prefix + file.Name));
			found = true;
		}
		if(! found)
			wd_list.Error = Constants.CameraNotFoundStr();

		return wd_list;
	}
}

//Windows and mac share similar behaviour on ffmpeg get devices
public abstract class WebcamFfmpegGetDevicesWinMac : WebcamFfmpegGetDevices
{
	protected string executable;
	protected string videoDevString;
	protected string audioDevString;

	//windows specific
	protected bool doingName; //used only in Windows where device name and code come in two lines
	protected string deviceName;

	protected abstract List<string> createParameters();

	public override WebcamDeviceList GetDevices()
	{
		List<string> parameters = createParameters();

		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, parameters, true, true);

		LogB.Information("---- stdout: ----");
		LogB.Information(execute_result.stdout);
		LogB.Information("---- stderr: ----");
		LogB.Information(execute_result.stderr);
		LogB.Information("-----------------");

		LogB.Information("pre");
		if(! execute_result.success)
		{
			LogB.Information("no success");
			if( ! executableExists())
			{
				LogB.Information(string.Format("File {0} does not exists, but note execuble can be on path", executable));
				wd_list.Error = Constants.FfmpegNotInstalledStr();
				return wd_list;
			}

			/*
			 * on Windows the -i dummy produces an error, so stderr exists and success is false
			 * on Mac the -i "" produces an error, so stderr exists and success is false
			 * stdout has the list of devices and stderr also
			 * check if in stdout there's the: videoDevString and if not exists, really we have an error
			 */
			if(execute_result.stdout != null && execute_result.stdout != "" &&
					execute_result.stdout.Contains(videoDevString))
			{
				LogB.Information("Calling parse with stdout");
				parse(execute_result.stdout);
				return wd_list;
			}

			if(execute_result.stderr != null && execute_result.stderr != "" &&
					execute_result.stderr.Contains(videoDevString))
			{
				LogB.Information("Calling parse with stderr");
				parse(execute_result.stderr);
				return wd_list;
			}

			wd_list.Error = Constants.CameraNotFoundStr();
		}
		else
			parse(execute_result.stdout);

		return wd_list;
	}

	protected void parse(string devicesOutput)
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

		bool started = false;
		doingName = true;
		foreach(string l in lines)
		{
			LogB.Information("line: " + l);

			//devices start after the videoDevString line
			if(! started)
			{
				if(l.Contains(videoDevString))
					started = true;

				continue;
			}

			parseMatch(l);

			//after the list of video devices comes the list of audio devices, skip it
			if(l.Contains(audioDevString))
				break;
		}
	}

	protected abstract void parseMatch(string l);

	protected bool executableExists()
	{
		return File.Exists(executable);
	}

}

public class WebcamFfmpegGetDevicesWindows : WebcamFfmpegGetDevicesWinMac
{
	public WebcamFfmpegGetDevicesWindows()
	{
		initialize();
		//if(System.Environment.Is64BitProcess)
			executable = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffmpeg.exe");
		//else
		//	executable = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/i386/ffmpeg.exe");
		videoDevString = "DirectShow video devices";
		audioDevString = "DirectShow audio devices";
	}

	protected override List<string> createParameters()
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

	protected override void parseMatch(string l)
	{
		LogB.Information(string.Format("parseMatch: {0}", l));
		//on windows, on each device, first line is the name and second line is the code
		foreach(Match match in Regex.Matches(l, "\"([^\"]*)\""))
		{
			LogB.Information(string.Format("\nmatch: {0}", match));
			//remove quotes from the match (at beginning and end) to add it in SQL
			string s = match.ToString().Substring(1, match.ToString().Length -2);

			if (doingName) {
				deviceName = s;
				LogB.Information(string.Format("deviceName: {0}", deviceName));
			} else {
				LogB.Information(string.Format("add match: code: {0} ; name: {1}", s, deviceName));
				wd_list.Add(new WebcamDevice(s, deviceName));
			}
			doingName = ! doingName;
		}
	}
}

public class WebcamFfmpegGetDevicesMac : WebcamFfmpegGetDevicesWinMac
{
	public WebcamFfmpegGetDevicesMac()
	{
		initialize();
		executable = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffmpeg");
		videoDevString = "AVFoundation video devices";
		audioDevString = "AVFoundation audio devices";
	}

	protected override List<string> createParameters()
	{
		//ffmpeg -f avfoundation -list_devices true -i ""
		List<string> parameters = new List<string>();

		int i = 0;
		parameters.Insert (i ++, "-f");
		parameters.Insert (i ++, "avfoundation");
		parameters.Insert (i ++, "-list_devices");
		parameters.Insert (i ++, "true");
		parameters.Insert (i ++, "-i");
		//parameters.Insert (i ++, "\"\"");
		parameters.Insert (i ++, "''");

		return parameters;
	}

	protected override void parseMatch(string l)
	{
		//ffmpeg 4.3 seems to return "indev"
		if( ! l.Contains("AVFoundation input device") && ! l.Contains("AVFoundation indev") )
			return;

		int firstBracketEnd = l.IndexOf(']');
		if(firstBracketEnd > 0 && l.Length > firstBracketEnd + 2)
		{
			string s = l.Substring(firstBracketEnd);
			int secondBracketStart = s.IndexOf('[');

			if(secondBracketStart > 0 && s.Length > secondBracketStart + 2)
			{
				s = s.Substring(secondBracketStart);
				LogB.Information("MAC matchPARSE: ***" + s + "***");

				//discard screen capture device on mac
				if(s.ToLower().Contains("capture screen"))
					return;

				wd_list.Add(new WebcamDevice(s[1].ToString(), s));
			}
		}
	}
}
