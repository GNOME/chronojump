/*
 * This file is part of ChronoJump
 *
 * Chronojump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * Chronojump is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 *    GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * Copyright (C) 2004-2022   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO;
using System.IO.Ports;


public class Config
{
	//to avoid passing this info to all the windows and dialogs, just read it here
	public static bool UseSystemColor; //do nothing at all
	public static Gdk.Color ColorBackground;
	public static bool ColorBackgroundIsDark;
	public static string DataDirStatic = ""; //works even with spaces in name

	public enum SessionModeEnum { STANDARD, UNIQUE, MONTHLY }

	public Preferences.MaximizedTypes Maximized;
	public bool CustomButtons;
	//public bool UseVideo;
	public bool OnlyEncoderGravitatory;
	public bool OnlyEncoderInertial;
	public EncoderCaptureDisplay EncoderCaptureShowOnlyBars;
	public bool EncoderUpdateTreeViewWhileCapturing = true; //user can change the 3 show checkboxes, so have it true to be updated
	public bool PersonWinHide;
	public bool EncoderAnalyzeHide;
	public SessionModeEnum SessionMode;
	public bool Compujump;
	public bool CompujumpDjango;
	public string CompujumpServerURL = "";
	public bool CompujumpHideTaskDone = false;

	public int CompujumpStationID = -1;
	public int CompujumpAdminID = -1; //undefined
	public string CompujumpAdminEmail = ""; //undefined
	public Constants.Modes CompujumpStationMode = Constants.Modes.UNDEFINED;
	public string RunScriptOnExit;
	public bool PlaySoundsFromFile;

	public bool Exhibition; //like YOMO. does not have rfid capture, user autologout management, and automatic configuration of gui
	public ExhibitionTest.testTypes ExhibitionStationType;
	public bool Raspberry;
	public bool LowHeight; //devices with less than 500 px vertical, like Odroid Go Super
	public bool LowCPU; //workaround to not show realtime graph on force sensor capture (until its optimized)
	public bool GuiTest;
	public bool CanOpenExternalDataDir;
	public string DataDir = ""; //on chronojump-networks admin to replace GetLocalDataDir (), think if Import has to be disabled

	/*
	 * unused because the default serverURL chronojump.org is ok:
	 * public string ExhibitionServerURL = "";
	 * public int ExhibitionStationID = -1;
	 */

	public Config()
	{
		/*
		Maximized = Preferences.MaximizedTypes.NO;
		CustomButtons = false;
		UseVideo = true;
		
		OnlyEncoderGravitatory = false;
		OnlyEncoderInertial = false;
		EncoderCaptureShowOnlyBars = false;
		EncoderUpdateTreeViewWhileCapturing = true;
		PersonWinHide = false;
		EncoderAnalyzeHide = false;
		SessionMode = SessionModeEnum.STANDARD;
		Compujump = false;
		RunScriptOnExit = "";
		*/
	}

	public void Read()
	{
		string contents = Util.ReadFile(Util.GetConfigFileName(), false);
		if (contents != null && contents != "") 
		{
			string line;
			using (StringReader reader = new StringReader (contents)) {
				do {
					line = reader.ReadLine ();

					if (line == null)
						break;
					if (line == "" || line[0] == '#')
						continue;

					string [] parts = line.Split(new char[] {'='});
					if(parts.Length != 2)
						continue;

					if(parts[0] == "Compujump" && Util.StringToBool(parts[1])) //Compujump is related to networks (usually the big screens)
						Compujump = true;
					else if(parts[0] == "CompujumpDjango" && Util.StringToBool(parts[1]))
						CompujumpDjango = true;
					else if(parts[0] == "CompujumpHideTaskDone" && Util.StringToBool(parts[1]))
						CompujumpHideTaskDone = true;
					else if(parts[0] == "CompujumpServerURL" && parts[1] != "")
						CompujumpServerURL = parts[1];
					else if(parts[0] == "CompujumpStationID" && parts[1] != "" && Util.IsNumber(parts[1], false))
						CompujumpStationID = Convert.ToInt32(parts[1]);
					else if(parts[0] == "CompujumpAdminID" && parts[1] != "" && Util.IsNumber(parts[1], false))
						CompujumpAdminID = Convert.ToInt32(parts[1]);
					else if(parts[0] == "CompujumpAdminEmail" && parts[1] != "")
						CompujumpAdminEmail = parts[1];
					else if(parts[0] == "CompujumpStationMode" && Enum.IsDefined(typeof(Constants.Modes), parts[1]))
						CompujumpStationMode = (Constants.Modes)
							Enum.Parse(typeof(Constants.Modes), parts[1]);
					else if(parts[0] == "SessionMode" && Enum.IsDefined(typeof(SessionModeEnum), parts[1]))
						SessionMode = (SessionModeEnum) 
							Enum.Parse(typeof(SessionModeEnum), parts[1]);
					else if(parts[0] == "PlaySoundsFromFile" && Util.StringToBool(parts[1]))
						PlaySoundsFromFile = true;
					else if(parts[0] == "Exhibition" && Util.StringToBool(parts[1]))
						Exhibition = true;
					else if(parts[0] == "ExhibitionStationType" && Enum.IsDefined(typeof(ExhibitionTest.testTypes), parts[1]))
						ExhibitionStationType = (ExhibitionTest.testTypes)
							Enum.Parse(typeof(ExhibitionTest.testTypes), parts[1]);
					/*
					else if(parts[0] == "ExhibitionServerURL" && parts[1] != "")
						ExhibitionServerURL = parts[1];
					else if(parts[0] == "ExhibitionStationID" && parts[1] != "" && Util.IsNumber(parts[1], false))
						ExhibitionStationID = Convert.ToInt32(parts[1]);
						*/
					else if(parts[0] == "Raspberry" && Util.StringToBool(parts[1])) //Raspberry: small screens, could be networks or not. They are usually disconnected by cable removal, so do not show send log at start
						Raspberry = true;
					else if(parts[0] == "LowHeight" && Util.StringToBool(parts[1]))
						LowHeight = true;
					else if(parts[0] == "LowCPU" && Util.StringToBool(parts[1]))
						LowCPU = true;
					else if(parts[0] == "GuiTest" && Util.StringToBool(parts[1]))
						GuiTest = true;
					else if(parts[0] == "CanOpenExternalDataDir" && Util.StringToBool(parts[1]))
						CanOpenExternalDataDir = true;
					else if(parts[0] == "DataDir" && parts[1] != "")
					{
						DataDir = parts[1]; //works even with spaces on name
						/*
						   DataDirStatic is assigned later to not be active on chronojump.cs,
						   start when gui is started, to not mess with runningFileName and others
						DataDirStatic = parts[1]; //called from Util.GetLocalDataDir
						 */
					}
				} while(true);
			}
		}
	}

	//p is currentPerson
	public bool CompujumpUserIsAdmin(Person p)
	{
		LogB.Information("CompujumpUserIsAdmin ? (person)");
		LogB.Information(string.Format("{0}, {1}", p.UniqueID, CompujumpAdminID));
		LogB.Information(string.Format("{0}, {1}, {2}", p != null, Compujump, p.UniqueID == CompujumpAdminID));

		return (p != null && Compujump && p.UniqueID == CompujumpAdminID);
	}
	public bool CompujumpUserIsAdmin(int pID)
	{
		LogB.Information("CompujumpUserIsAdmin ? (int)");
		return (Compujump && pID == CompujumpAdminID);
	}

	/*
	public static void UpdateField(string field, string text)
	{
		//adapted from
		//http://stackoverflow.com/a/2401873
				
		string tempfile = Path.GetTempFileName();

		LogB.Information("UpdateField, field: " + field + ", text: " + text);
		
		if(! File.Exists(Util.GetConfigFileName())) {
			try {
				using (var writer = new StreamWriter(tempfile))
				{
					writer.WriteLine(field + "=" + text);
				}
				File.Copy(tempfile, Util.GetConfigFileName(), true);
			} catch {
				LogB.Warning("Cannot write at Config.UpdateField");
			}
		} else {
			try {
				using (var writer = new StreamWriter(tempfile))
					using (var reader = new StreamReader(Util.GetConfigFileName()))
					{
						while (! reader.EndOfStream) {
							string line = reader.ReadLine();
							if (line != "" && line[0] != '#') 
							{
								string [] parts = line.Split(new char[] {'='});
								if(parts.Length == 2 && parts[0] == field)
									line = field + "=" + text;
							}

							writer.WriteLine(line);
						}
					}
				File.Copy(tempfile, Util.GetConfigFileName(), true);
			} catch {
				LogB.Warning("Cannot write at Config.UpdateField");
			}
		}
	}

	public override string ToString() 
	{
		return(
				"Maximized = " + Maximized.ToString() + "\n" +
				"CustomButtons = " + CustomButtons.ToString() + "\n" +
				"UseVideo = " + UseVideo.ToString() + "\n" +
				"OnlyEncoderGravitatory = " + OnlyEncoderGravitatory.ToString() + "\n" +
				"OnlyEncoderInertial = " + OnlyEncoderInertial.ToString() + "\n" +
				"EncoderCaptureShowOnlyBars = " + EncoderCaptureShowOnlyBars.ToString() + "\n" +
				"EncoderUpdateTreeViewWhileCapturing = " + EncoderUpdateTreeViewWhileCapturing.ToString() + "\n" +
				"PersonWinHide = " + PersonWinHide.ToString() + "\n" +
				"EncoderAnalyzeHide = " + EncoderAnalyzeHide.ToString() + "\n" +
				"SessionMode = " + SessionMode.ToString() + "\n" +
				"Compujump = " + Compujump.ToString() + "\n" +
				"RunScriptOnExit = " + RunScriptOnExit.ToString() + "\n"
		      );
	}
	*/

	~Config() {}
}
