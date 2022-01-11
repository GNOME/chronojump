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
 *  Copyright (C) 2004-2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections.Generic; //List<T>
using System.IO; //StringReader
using Mono.Unix;
	
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
		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, parameters, true, true);

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

		execute_result = ExecuteProcess.run (executable, parameters, true, true);
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

public class NetworksSendMail
{
	//filename is the image
	//public ExecuteProcess.Result execute_result;
	public string ErrorStr = "";

	public NetworksSendMail ()
	{
		ErrorStr = "";
	}

	public bool Send (string title, string filenameImage, string filenameCSV, string email)
	{
		//echo "See attached file" |mailx -s "$HOSTNAME Testing attachment" -A /home/chronojump/chronojump/images/calendar.png testing@chronojump.org

		List<string> parameters = new List<string>();
		parameters.Add("-s");
		//parameters.Add("\"myHostName: myTitle\"");
		parameters.Add("ChronojumpNetworks: " + title);
		parameters.Add("-A");
		parameters.Add(filenameImage);
		if(filenameCSV != "")
		{
			parameters.Add("-A");
			parameters.Add(filenameCSV);
		}
		parameters.Add(email);

		//note redirect output and error is false because if redirect input there are problems redirecting the others
		ExecuteProcess.Result execute_result = ExecuteProcess.run ("mail.mailutils", parameters, getBody(title), false, false);
		//TODO: decide if mail.mailutils (maybe debian) or mailutils (maybe manjaro), maybe this has to be on chronojump_config.txt
		//ExecuteProcess.Result execute_result = ExecuteProcess.run ("mailutils", parameters, getBody(title), false, false);
		if(! execute_result.success) {
			ErrorStr = "Need to install mail.mailutils";
		}

		return execute_result.success;
	}

	//currently on Spanish and Spanish date localization
	private string getBody(string s)
	{
		string [] strFull = s.Split(new char[] {'_'}); //player_YYY-M-D_exercise

		if(strFull.Length < 3)
			return "";

		string dateString = "";
		string [] strFullDate = strFull[1].Split(new char[] {'-'});
		if(strFullDate.Length == 3)
			dateString = string.Format("Fecha: Día: {0}, Mes: {1}, Año: {2}", strFullDate[2], strFullDate[1], strFullDate[0]);

		return string.Format("Gráfica del jugador: {0}\nTest: {1}\n{2}", strFull[0], strFull[2], dateString);
	}

	// another option will be use C-sharp methods, see:
	// https://stackoverflow.com/questions/2825950/sending-email-with-attachments-from-c-attachments-arrive-as-part-1-2-in-thunde
}

/*
 * on Networks to check if eth0 or wifi devices (interfaces) are on
 * read to see if they are "up":
 * /sys/class/net/eth0/operstate
 * /sys/class/net/wlan.../operstate
*/
public class NetworksCheckDevices
{
	private List<string> devicesUp;
	private string path = "/sys/class/net/";

	public NetworksCheckDevices ()
	{
		devicesUp = new List<string>();

		DirectoryInfo pathDirInfo = new DirectoryInfo(path);
		DirectoryInfo [] subdirs = pathDirInfo.GetDirectories();

		/*
		 * check eth0, wlan*
		 * but note our computers have interface enp2s0, wlp3s0
		 * they are valid: https://www.freedesktop.org/wiki/Software/systemd/PredictableNetworkInterfaceNames/
		 */
		foreach (DirectoryInfo dir in subdirs)
			if( ( dir.Name.StartsWith("eth") || dir.Name.StartsWith("en") || dir.Name.StartsWith("wl") ) && checkDevice(dir.Name))
				devicesUp.Add(dir.Name);
	}

	private bool checkDevice(string device)
	{
		string filename = path + device + "/operstate";
		if(File.Exists(filename))
		{
			List<string> l = Util.ReadFileAsStringList(filename);
			foreach(string str in l)
				if(str.Contains("up"))
					return true;
		}

		return false;
	}

	public override string ToString()
	{
		if(devicesUp.Count == 0)
			return Catalog.GetString("No active Internet devices.");
		else {
			string str = Catalog.GetString("Active Internet devices:");
			string sep = " ";
			foreach(string device in devicesUp)
			{
				str += sep + device;
				sep = ", ";
			}
			return str;
		}
	}
}
