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

	//stored in DB
	public static Gdk.RGBA ColorBackground;

	//not stored in DB (but incluced here to not have to calculate it all the time)
	public static bool ColorBackgroundIsDark;
	public static Gdk.RGBA ColorBackgroundShifted;
	public static bool ColorBackgroundShiftedIsDark;

	public static string LastDBFullPathStatic = ""; //works even with spaces in name

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
	public bool CanOpenExternalDB;
	public string ExternalDBDefaultPath = ""; //on chronojump-networks admin to replace GetLocalDataDir (), think if Import has to be disabled
	public string LastDBFullPath = ""; //on chronojump-networks admin to replace GetLocalDataDir (), think if Import has to be disabled

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
					else if(parts[0] == "CanOpenExternalDB" && Util.StringToBool(parts[1]))
						CanOpenExternalDB = true;
					else if(parts[0] == "ExternalDBDefaultPath" && parts[1] != "")
						ExternalDBDefaultPath = parts[1]; //works even with spaces on name
					else if(parts[0] == "LastDBFullPath" && parts[1] != "")
					{
						LastDBFullPath = parts[1]; //works even with spaces on name
						/*
						   DataDirStatic is assigned later to not be active on chronojump.cs,
						   start when gui is started, to not mess with runningFileName and others
						LastDBFullPathStatic = parts[1]; //called from Util.GetLocalDataDir
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

	//adapted from: http://stackoverflow.com/a/2401873
	//useDefaultConfigFile is the default. It ensures no use the config of another database
	public void UpdateFieldEnsuringDefaultConfigFile (string field, string text)
	{
		string storedLastDBFullPathStatic = Config.LastDBFullPathStatic;
		Config.LastDBFullPathStatic = "";

		UpdateField (field, text);

		Config.LastDBFullPathStatic = storedLastDBFullPathStatic;
	}
	public void UpdateField (string field, string text)
	{
		string tempfile = Path.GetTempFileName ();
		string configFile = Util.GetConfigFileName ();
		LogB.Information( string.Format ("Config.UpdateField tempfile: {0}, configFile: {1}, field: {2}, text: {3}",
					tempfile, configFile, field, text));
		
		if(! File.Exists (configFile)) {
			try {
				using (var writer = new StreamWriter (tempfile))
				{
					writer.WriteLine (field + "=" + text);
				}
				File.Copy (tempfile, configFile, true);
			} catch {
				LogB.Warning ("Cannot write at Config.UpdateField");
			}
		} else {
			try {
				using (var writer = new StreamWriter(tempfile))
					using (var reader = new StreamReader (configFile))
					{
						bool found = false;
						while (! reader.EndOfStream)
						{
							string line = reader.ReadLine ();
							if (line != "" && line[0] != '#') 
							{
								string [] parts = line.Split (new char[] {'='});
								if (parts.Length == 2 && parts[0] == field)
								{
									line = field + "=" + text;
									found = true;
								}
							}
							writer.WriteLine (line);
						}

						//if not found it adds the command
						if (! found)
							writer.WriteLine (field + "=" + text);
					}
				File.Copy (tempfile, configFile, true);
			} catch {
				LogB.Warning ("Cannot write at Config.UpdateField");
			}
		}
	}

	public override string ToString() 
	{
		return (string.Format (
					"Compujump = {0}, CompujumpDjango = {1}, CompujumpHideTaskDone = {2}, CompujumpServerURL = {3}, " +
					"CompujumpStationID = {4}, CompujumpAdminID = {5}, compujumpAdminEmail = {6}, CompujumpStationMode = {7}, " +
					"SessionMode = {8}, PlaySoundsFromFile = {9}, Exhibition = {10}, Raspberry = {11}, " +
					"LowHeight = {12}, LowCPU = {13}, GuiTest = {14}, CanOpenExternalDB = {15}, " +
					"ExternalDBDefaultPath = {16}, LastDBFullPath = {17}",
					Compujump, CompujumpDjango, CompujumpHideTaskDone, CompujumpServerURL,
					CompujumpStationID, CompujumpAdminID, CompujumpAdminEmail, CompujumpStationMode,
					SessionMode, PlaySoundsFromFile, Exhibition, Raspberry,
					LowHeight, LowCPU, GuiTest, CanOpenExternalDB,
					ExternalDBDefaultPath, LastDBFullPath));
	}

	~Config() {}
}
