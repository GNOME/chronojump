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
 *  Copyright (C) 2019   Xavier de Blas <xaviblas@gmail.com> 
 */

using System.Collections.Generic; //List
using System.Diagnostics;
using System;
using System.IO;
using System.Text.RegularExpressions; //Regex


public abstract class WebcamFfmpegGetDevices
{
	public abstract List<string> GetDevices();

	protected abstract List<string> createParameters();

	protected abstract List<string> parse(string devicesOutput);
}

public class WebcamFfmpegGetDevicesLinux : WebcamFfmpegGetDevices
{
	public WebcamFfmpegGetDevicesLinux()
	{
	}

	public override List<string> GetDevices()
	{
		List<string> list = new List<string>();
		string prefix = "/dev/";
		var dir = new DirectoryInfo(prefix);
		foreach(var file in dir.EnumerateFiles("video*"))
			/*
			 * return 0, 1, ...
			 if( file.Name.Length > 5 && 				//there's something more than "video", like "video0" or "video1", ...
			 char.IsNumber(file.Name, 5) ) 		//and it's a number
			 list.Add(Convert.ToInt32(file.Name[5])); 			//0 or 1, or ...
			 */
			//return "/dev/video0", "/dev/video1", ...
			list.Add(prefix + file.Name);

		return list;
	}

	protected override List<string> createParameters()
	{
		return new List<string>();
	}

	protected override List<string> parse(string devicesOutput)
	{
		return new List<string>();
	}
}


public class WebcamFfmpegGetDevicesWindows : WebcamFfmpegGetDevices
{
	public WebcamFfmpegGetDevicesWindows()
	{
	}

	public override List<string> GetDevices()
	{
		string executable = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffmpeg.exe");
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

	protected override List<string> parse(string devicesOutput)
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

public class WebcamFfmpegGetDevicesMac : WebcamFfmpegGetDevices
{
	public WebcamFfmpegGetDevicesMac()
	{
	}

	public override List<string> GetDevices()
	{
		string executable = "ffmpeg";
		List<string> parameters = createParameters();

		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, parameters, true, true);

		LogB.Information("---- stdout: ----");
		LogB.Information(execute_result.stdout);
		LogB.Information("---- stderr: ----");
		LogB.Information(execute_result.stderr);
		LogB.Information("-----------------");

		if(! execute_result.success)
		{
			/*
			LogB.Information("WebcamFfmpegGetDevicesMac stdout: " + execute_result.stdout);
			LogB.Information("WebcamFfmpegGetDevicesMac error: " + execute_result.stderr);
			*/

			return new List<string>();
		}
		else
			return parse(execute_result.stdout);
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
		parameters.Insert (i ++, "\"\"");

		return parameters;
	}

	protected override List<string> parse(string devicesOutput)
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
			parsedList.Add(l);
		}

		return parsedList;
	}
}
