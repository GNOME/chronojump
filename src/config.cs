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
 * Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO;
using System.IO.Ports;
using System.Collections.Generic;


public class Config
{
	public static bool SimulatedCapture; //readed from commandline

	//to avoid passing this info to all the windows and dialogs, just read it here
	public static bool UseSystemColor; //do nothing at all

	public static string LastDBFullPathStatic = ""; //works even with spaces in name
	/*
	   About LastDBFullPath and LastDBFullPathStatic:
	   At Read, DataDirStatic is assigned later to not be active on chronojump.cs,
	   start when gui is started, to not mess with runningFileName and others
	   LastDBFullPathStatic = parts[1]; //called from Util.GetLocalDataDir
	   */


	//use this bool because on Windows some File.Copy problems are not catched on gui/app1/encoder.cs checkFile
	//eg when writing to a file "owned" by another application
	public static bool ErrorInExport;

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
	public string RunScriptOnExit;


	//remember to change the fill method if this list grows
	public enum OpEnum {
		Compujump, CompujumpDjango, CompujumpServerURL, CompujumpStationID, CompujumpStationMode, //networks (main options)
		CompujumpHideTasksDone, CompujumpAdminID, CompujumpAdminEmail, //networks (other)
		CopyToCloudFullPath, CopyToCloudOnExit, ReadFromCloudMainPath, //cloud
		CanOpenExternalDB, ExternalDBDefaultPath, //externalDB
		LastDBFullPath, //cloud & externalDB
		SessionMode, FTDIalways, Raspberry, LowHeight, LowCPU, GuiTest, //other
		Exhibition, ExhibitionStationType, PlaySoundsFromFile //outdated or not working
	};

	//to display the configAll list nicely
	public static string OpEnum1stNetworksMain = OpEnum.Compujump.ToString ();
	public static string OpEnum1stNetworksOther = OpEnum.CompujumpHideTasksDone.ToString ();
	public static string OpEnum1stCloud = OpEnum.CopyToCloudFullPath.ToString ();
	public static string OpEnum1stExternalDB = OpEnum.CanOpenExternalDB.ToString ();
	public static string OpEnum1stCloudAndExternalDB = OpEnum.LastDBFullPath.ToString ();
	public static string OpEnum1stOther = OpEnum.SessionMode.ToString ();
	public static string OpEnum1stOutdated = OpEnum.Exhibition.ToString ();

	// networks (main options)
	public bool Compujump {
		get { return configList.GetBool (OpEnum.Compujump); }
	}
	public bool CompujumpDjango {
		get { return configList.GetBool (OpEnum.CompujumpDjango); }
	}
	public string CompujumpServerURL {
		get { return configList.GetString (OpEnum.CompujumpServerURL); }
	}
	public int CompujumpStationID {
		get { return configList.GetInt (OpEnum.CompujumpStationID); }
	}
	public Constants.Modes CompujumpStationMode {
		get { return (Constants.Modes) Enum.Parse (typeof (Constants.Modes),
				configList.GetEnum (OpEnum.CompujumpStationMode)); }
	}

	// networks (other)
	public bool CompujumpHideTaskDone {
		get { return configList.GetBool (OpEnum.CompujumpHideTasksDone); }
	}
	public int CompujumpAdminID {
		get { return configList.GetInt (OpEnum.CompujumpAdminID); }
	}
	public string CompujumpAdminEmail {
		get { return configList.GetString (OpEnum.CompujumpAdminEmail); }
	}

	// cloud
	public string CopyToCloudFullPath {
		get { return configList.GetString (OpEnum.CopyToCloudFullPath); }
	}
	public bool CopyToCloudOnExit {
		get { return configList.GetBool (OpEnum.CopyToCloudOnExit); }
	}
	public string ReadFromCloudMainPath {
		get { return configList.GetString (OpEnum.ReadFromCloudMainPath); }
	}

	// external DB
	public bool CanOpenExternalDB {
		get { return configList.GetBool (OpEnum.CanOpenExternalDB); }
	}
	public string ExternalDBDefaultPath {
		get { return configList.GetString (OpEnum.ExternalDBDefaultPath); }
	}

	// cloud & externalDB
	public string LastDBFullPath {
		get { return configList.GetString (OpEnum.LastDBFullPath); }
		set { configList.SetValue (OpEnum.LastDBFullPath.ToString (), value); }
	}

	// other
	public SessionModeEnum SessionMode {
		get { return (SessionModeEnum) Enum.Parse (typeof (SessionModeEnum),
				configList.GetEnum (OpEnum.SessionMode)); }
	}
	public bool FTDIalways {
		get {
			// 1 automatically check if is a chromeOS
			if (UtilAll.GetOSEnum () == UtilAll.OperatingSystems.LINUX && UtilAll.IsChromeOS ())
				return true;

			// 2 if not check this config option, but maybe will disappear one day
			return configList.GetBool (OpEnum.FTDIalways); }
	}
	public bool Raspberry {
		get { return configList.GetBool (OpEnum.Raspberry); }
	}
	public bool LowHeight {
		get { return configList.GetBool (OpEnum.LowHeight); }
	}
	public bool LowCPU {
		get { return configList.GetBool (OpEnum.LowCPU); }
	}
	public bool GuiTest {
		get { return configList.GetBool (OpEnum.GuiTest); }
	}

	// outdated or not working
	public bool Exhibition {
		get { return configList.GetBool (OpEnum.Exhibition); }
	}
	public ExhibitionTest.testTypes ExhibitionStationType
	{
		get { return (ExhibitionTest.testTypes) Enum.Parse (typeof (ExhibitionTest.testTypes),
				configList.GetEnum (OpEnum.ExhibitionStationType)); }
	}
	public bool PlaySoundsFromFile
	{
		get { return configList.GetBool (OpEnum.PlaySoundsFromFile); }
	}

	/*
	 * unused because the default serverURL chronojump.org is ok:
	 * public string ExhibitionServerURL = "";
	 * public int ExhibitionStationID = -1;
	 */

	//stored in DB
	private static Gdk.RGBA colorBackground;

	//not stored in DB (but incluced here to not have to calculate it all the time)
	private static bool colorBackgroundIsDark;
	private static Gdk.RGBA colorBackgroundShifted;
	private static bool colorBackgroundShiftedIsDark;

	private ConfigList configList;

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

		configList = new ConfigList ();
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

					configList.SetValue (parts[0], parts [1]); //TODO: parse parts[0] enum and pass it
				} while (true);
			}
		}
	}

	public string PrintAll ()
	{
		return configList.PrintAll ();
	}

	public string PrintDefined ()
	{
		return configList.PrintDefined ();
	}

	public static void SetColors (Gdk.RGBA color)
	{
		colorBackground = color;
		colorBackgroundIsDark = UtilGtk.ColorIsDark (color);
		colorBackgroundShifted = UtilGtk.GetColorShifted (color, ! colorBackgroundIsDark);
		colorBackgroundShiftedIsDark = UtilGtk.ColorIsDark (colorBackgroundShifted);
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

	public static Gdk.RGBA ColorBackground
	{
		get { return colorBackground; }
	}
	public static bool ColorBackgroundIsDark
	{
		get { return colorBackgroundIsDark; }
	}
	public static Gdk.RGBA ColorBackgroundShifted
	{
		get { return colorBackgroundShifted; }
	}
	public static bool ColorBackgroundShiftedIsDark
	{
		get { return colorBackgroundShiftedIsDark; }
	}

	~Config() {}
}

// this class contains the list of config options (can define any option and list all or defined)
public class ConfigList
{
	public List<ConfigOption> list;

	// constructor
	public ConfigList ()
	{
		create ();
		fill ();
	}

	// public methods
	public void SetValue (string name, string theValue)
	{
		foreach (ConfigOption co in list)
			if (name == co.Name && co.ValueCorrectForThisType (theValue))
				co.SetValue (theValue);
	}

	public int GetInt (Config.OpEnum name)
	{
		foreach (ConfigOption co in list)
			if (name.ToString () == co.Name)
			{
				if (co.Defined)
					return Convert.ToInt32 (co.ValuePrint ());
				else
					return Convert.ToInt32 (co.DefaultValue);
			}

		return -1;
	}

	public bool GetBool (Config.OpEnum name)
	{
		foreach (ConfigOption co in list)
			if (name.ToString () == co.Name)
			{
				if (co.Defined)
					return (co.ValuePrint ().ToString () == "True");
				else
					return (bool) co.DefaultValue;
			}

		return false;
	}

	public string GetString (Config.OpEnum name)
	{
		foreach (ConfigOption co in list)
			if (name.ToString () == co.Name)
			{
				if (co.Defined)
					return (string) (co.ValuePrint ());
				else
					return (string) co.DefaultValue;
			}

		return "";
	}

	public string GetEnum (Config.OpEnum name) //get as string and parse on the caller
	{
		foreach (ConfigOption co in list)
		{
			if (name.ToString () == co.Name)
			{
				if (co.Defined)
				{
					//this does not work
					//return (string) (co.ValuePrint ()); //this does not work
					//this works
					string str = co.ValuePrint ().ToString ();
					return str;
				}
				else
				{
					//this does not work
					//return (string) co.DefaultValue;
					//this works
					string str = co.DefaultValue.ToString ();
					return str;
				}
			}
		}

		return "";
	}

	public string PrintAll ()
	{
		string str = "List of possible config options:";
		foreach (ConfigOption co in list)
		{
			if (co.Name == Config.OpEnum1stNetworksMain)
				str += "\n\nNetworks main:";
			else if (co.Name == Config.OpEnum1stNetworksOther)
				str += "\n\nNetworks other:";
			else if (co.Name == Config.OpEnum1stCloud)
				str += "\n\nCloud:";
			else if (co.Name == Config.OpEnum1stExternalDB)
				str += "\n\nExternalDB:";
			else if (co.Name == Config.OpEnum1stCloudAndExternalDB)
				str += "\n\nCloud & externalDB:";
			else if (co.Name == Config.OpEnum1stOther)
				str += "\n\nOther:";
			else if (co.Name == Config.OpEnum1stOutdated)
				str += "\n\nOutdated:";

			str += "\n" + co.ToString ();
		}
		str += "\n\nNote to define need to write option=theOption (without spaces) (paths without quotes (at least on Linux)) at chronojump_config.txt";
		str += "\n\nCanOpenExternalDB and ReadFromCloudMainPath only one can be active (if both are, cloud will be used). Because they do the same: show the database button, but on reading from cloud it will do a copy to tmp and operate with this copy.";
		str += "\n\nThis options are here and not in Sqlite DB because here are more easily changed on configure networks devices (just change a .txt),\n" +
			"also there is more convenient when related to some machines: eg. a LowHeight will display the gui in a way, but on change to its DB from other machine, we would not to see in LowHeight.";

		return str;
	}

	public string PrintDefined () //correctly found on chronojump_config.txt
	{
		string strTitle = "List of correctly defined config options:";
		string strValues = "";

		foreach (ConfigOption co in list)
			if (co.Defined)
				strValues += "\n" + co.PrintNameValue ();

		if (strValues == "")
			strValues = "\n(none)";

		return strTitle + strValues;
	}

	// private methods
	private void create ()
	{
		list = new List<ConfigOption> ();
	}

	private void fill ()
	{
		// 1 prepare the enum strings
		string constantsModesEnumStr = "";
		string sep = "";
		foreach (Constants.Modes m in Enum.GetValues(typeof(Constants.Modes)))
		{
			constantsModesEnumStr += string.Format ("{0}'{1}'", sep, m);
			sep = ", ";
		}

		// 2 fill the list
		// networks (main options)
		list.Add (new ConfigOptionBool (Config.OpEnum.Compujump,
					"Is this a Networks station?"));
		list.Add (new ConfigOptionBool (Config.OpEnum.CompujumpDjango,
					"Is this a Networks station using Django?"));
		list.Add (new ConfigOptionString  (Config.OpEnum.CompujumpServerURL,
					"At Networks, the URL of the server (take care if http or https)."));
		list.Add (new ConfigOptionInt (Config.OpEnum.CompujumpStationID,
					"At Networks, the ID of a capture station."));
		list.Add (new ConfigOptionEnum (Config.OpEnum.CompujumpStationMode,
					string.Format ("At Networks, the Mode of a capture station, can be any of: {0} (except the UNDEFINED)", constantsModesEnumStr),
					ConfigOptionEnum.WhichEnum.Constants_Modes));

		// networks (other)
		list.Add (new ConfigOptionBool (Config.OpEnum.CompujumpHideTasksDone,
					"At Networks, whena task is done hide it on the client."));
		list.Add (new ConfigOptionInt (Config.OpEnum.CompujumpAdminID,
					"At Networks, the ID of admin station."));
		list.Add (new ConfigOptionString (Config.OpEnum.CompujumpAdminEmail,
					"At Networks, email of admin station (to send email of the graph, maybe does not work with current code)."));

		// cloud
		list.Add (new ConfigOptionString (Config.OpEnum.CopyToCloudFullPath,
					"The path where all the data will be copied (uncompressed) to be synced with the cloud service."));
		list.Add (new ConfigOptionBool (Config.OpEnum.CopyToCloudOnExit,
					"If CopyToCloudFullPath is defined, then on Chronojump exit the copy will be done automatically."));
		list.Add (new ConfigOptionString (Config.OpEnum.ReadFromCloudMainPath,
					"The path to open cloud DBs (each one will be copied to temp on open). If this active, CanOpenExternalDB will be discarded."));

		// externalDB
		list.Add (new ConfigOptionBool (Config.OpEnum.CanOpenExternalDB,
					"A choose DB button will be visible and will check: ExternalDBDefaultPath, LastDBFullPath"));
		list.Add (new ConfigOptionString (Config.OpEnum.ExternalDBDefaultPath,
					"On chronojump-networks admin to replace GetLocalDataDir (), think if Import has to be disabled. Works even with spaces on name."));

		// cloud & externalDB
		list.Add (new ConfigOptionString (Config.OpEnum.LastDBFullPath,
					"Last path used, Chronojump will open it automatically if not empty and (ReadFromCloudMainPath or CanOpenExternalDB)."));

		// other
		list.Add (new ConfigOptionEnum (Config.OpEnum.SessionMode,
					"STANDARD (default), or UNIQUE or MONTHLY",
					ConfigOptionEnum.WhichEnum.Config_SessionModeEnum));
		list.Add (new ConfigOptionBool (Config.OpEnum.FTDIalways,
					"For ChromeOS where we ID_VENDOR is not returned on call udevadm"));
		list.Add (new ConfigOptionBool (Config.OpEnum.Raspberry,
					"Some graphical configs for Raspberry (not really updated). Raspberry: small screens, could be networks or not. They are usually disconnected by cable removal, so do not show send log at start"));
		list.Add (new ConfigOptionBool (Config.OpEnum.LowHeight,
					"Devices with less than 500 px vertical, like Odroid Go Super"));
		list.Add (new ConfigOptionBool (Config.OpEnum.LowCPU,
					"Workaround to not show realtime graph on force sensor capture (until its optimized)"));
		list.Add (new ConfigOptionBool (Config.OpEnum.GuiTest,
					"To perform tests with the GUI (untested with current code)."));

		// outdated or not working
		list.Add (new ConfigOptionBool (Config.OpEnum.Exhibition,
					"To shows like YOMO. Does not have rfid capture, user autologout management, and automatic configuration of gui. Maybe does not work ok with current code."));
		list.Add (new ConfigOptionEnum (Config.OpEnum.ExhibitionStationType,
					"JUMP, RUN, INERTIAL, FORCE_ROPE, FORCE_SHOT",
					ConfigOptionEnum.WhichEnum.ExhibitionTest_testTypes));
		list.Add (new ConfigOptionBool (Config.OpEnum.PlaySoundsFromFile,
					"For an spectacle with encoder. surely does not work ok with current code."));
	}
}

//this class is the abstract of any of the options
public abstract class ConfigOption
{
	protected string name;
	protected object theValue;
	protected object defaultValue;
	protected string explanation;
	protected string typeStr;
	protected bool defined;

	// public methods
	public abstract bool ValueCorrectForThisType (string theValue);

	public abstract void SetValue (string theValue);

	public abstract object ValuePrint ();

	public override string ToString ()
	{
		return (string.Format ("- {0}: {1} ({2}, default: {3})", name, explanation, typeStr, defaultValue));
	}

	public string PrintNameValue ()
	{
		return (string.Format ("- {0}: {1}", name, theValue));
	}

	// protected methods
	protected void init (string name, string explanation)
	{
		this.name = name;
		this.explanation = explanation;
		this.defined = false;
	}

	// accessors
	public string Name {
		get { return name; }
	}

	public object TheValue {
		get { return theValue; }
	}

	public object DefaultValue {
		get { return defaultValue; }
	}

	public bool Defined {
		get { return defined; }
	}
}

public class ConfigOptionInt : ConfigOption
{
	// constructor
	public ConfigOptionInt (Config.OpEnum name, string explanation)
	{
		init (name.ToString (), explanation);
		this.typeStr = "INT";
		this.defaultValue = -1;
	}

	//public overriden
	public override bool ValueCorrectForThisType (string theValue)
	{
		return (Util.IsNumber (theValue, false));
	}

	public override void SetValue (string theValue)
	{
		this.theValue = Convert.ToInt32 (theValue);
		defined = true;
	}

	public override object ValuePrint ()
	{
		return (object) Convert.ToInt32 (theValue);
	}
}

public class ConfigOptionBool : ConfigOption
{
	// constructor
	public ConfigOptionBool (Config.OpEnum name, string explanation)
	{
		init (name.ToString (), explanation);
		this.typeStr = "BOOL";
		this.theValue = false;
		this.defaultValue = false;
	}

	//public overriden
	public override bool ValueCorrectForThisType (string theValue)
	{
		return (theValue == "TRUE" || theValue == "FALSE");
	}

	public override void SetValue (string theValue)
	{
		this.theValue = (theValue == "TRUE");
		defined = true;
	}

	public override object ValuePrint ()
	{
		return (object) theValue.ToString();
	}
}

public class ConfigOptionString : ConfigOption
{
	// constructor
	public ConfigOptionString (Config.OpEnum name, string explanation)
	{
		init (name.ToString (), explanation);
		this.typeStr = "STRING";
		this.defaultValue = "";
	}

	//public overriden
	public override bool ValueCorrectForThisType (string theValue)
	{
		return true;
	}

	public override void SetValue (string theValue)
	{
		this.theValue = theValue;
		defined = true;
	}

	public override object ValuePrint ()
	{
		return (object) theValue;
	}
}

public class ConfigOptionEnum : ConfigOption
{
	public enum WhichEnum { Config_SessionModeEnum, Constants_Modes, ExhibitionTest_testTypes }
	private WhichEnum which;

	// constructor
	public ConfigOptionEnum (Config.OpEnum name, string explanation, WhichEnum which)
	{
		init (name.ToString (), explanation);
		this.typeStr = "ENUM";

		if (which == WhichEnum.Config_SessionModeEnum)
			this.defaultValue = Config.SessionModeEnum.STANDARD;
		else if (which == WhichEnum.Constants_Modes)
			this.defaultValue = Constants.Modes.UNDEFINED;
		else if (which == WhichEnum.ExhibitionTest_testTypes)
			this.defaultValue = ExhibitionTest.testTypes.JUMP;

		this.which = which;
	}

	//public overriden
	public override bool ValueCorrectForThisType (string theValue)
	{
		if (which == WhichEnum.Config_SessionModeEnum)
			return Enum.IsDefined (typeof(Config.SessionModeEnum), theValue);
		else if (which == WhichEnum.Constants_Modes)
			return Enum.IsDefined (typeof(Constants.Modes), theValue);
		else if (which == WhichEnum.ExhibitionTest_testTypes)
			return Enum.IsDefined (typeof(ExhibitionTest.testTypes), theValue);

		return false;
	}

	public override void SetValue (string theValue)
	{
		if (which == WhichEnum.Config_SessionModeEnum)
			this.theValue = (Config.SessionModeEnum) Enum.Parse(typeof(Config.SessionModeEnum), theValue);
		else if (which == WhichEnum.Constants_Modes)
			this.theValue = (Constants.Modes) Enum.Parse(typeof(Constants.Modes), theValue);
		else if (which == WhichEnum.ExhibitionTest_testTypes)
			this.theValue = (ExhibitionTest.testTypes) Enum.Parse(typeof(ExhibitionTest.testTypes), theValue);

		defined = true;
	}

	public override object ValuePrint ()
	{
		return (object) theValue.ToString ();
	}
}
