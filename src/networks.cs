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
 *  Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections.Generic; //List<T>
using System.IO; //StringReader
	
public class Networks
{
	private List<int> UID;

	public Networks() {
	}

	public void Test() {
		UID = new List<int>{0,0,0,0};
		LogB.Debug(UIDToInt().ToString());

		UID = new List<int>{255,255,255,255};
		LogB.Debug(UIDToInt().ToString());
	}

	private uint UIDToInt ()
	{
		uint total = 0; //total has to be an uint
		int count = 0;
		//maybe has to be done backwards (from 3 to 0)
		foreach(int uid in UID) {
			total += Convert.ToUInt32(
					uid *
					Math.Pow(Convert.ToDouble(256), Convert.ToDouble(count))
					);
			count ++;
		}

		return (total);
	}

	public static void WakeUpRaspberryIfNeeded()
	{
		string executable = "xset";
		List<string> parameters = new List<string>();

		parameters.Insert (0, "-q");
		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, parameters);

		bool on = screenIsOn(execute_result.stdout);
		LogB.Information("Screen is on?" + on.ToString());

		if(! on) {
			//xset -display :0 dpms force on
			parameters = new List<string>();
			parameters.Insert (0, "-display");
			parameters.Insert (1, ":0");
			parameters.Insert (2, "dpms");
			parameters.Insert (3, "force");
			parameters.Insert (4, "on");
		}

		execute_result = ExecuteProcess.run (executable, parameters);
		LogB.Information("Result = " + execute_result.stdout);
	}

	private static bool screenIsOn(string contents)
	{
		LogB.Information(contents);
		string line;
		using (StringReader reader = new StringReader (contents)) {
			do {
				line = reader.ReadLine ();

				if (line == null)
					break;

				if(line.StartsWith("  Monitor is On"))
					return true;
				else if(line.StartsWith("  Monitor is in Standby"))
					return false;
				else if(line.StartsWith("  Monitor is Off"))
					return false;
			} while(true);
		}
		return true; //by default screen is on. If detection is wrong user can touch screen
	}
}
