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
 *  Copyright (C) 2004-2009   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Mono.Unix;

public class Constants
{
	//the strings created by Catalog cannot be const
	
	public static string [] Authors = {
		"Xavier de Blas (xaviblas@gmail.com)\n" + Catalog.GetString("Project leader and main developer."), 
		"Juan Gonzalez (http://www.iearobotics.com)\n" + Catalog.GetString("Skypic, Chronopic, connection between hardware and software."), 
		"Juan Fernando Pardo (juanfer@juanfer.com.ar)\n" + Catalog.GetString("Chronopic2 creation, Chronojump tester."), 
		"Ricardo Gómez (http://www.iearobotics.com)\n" + Catalog.GetString("Chronopic3 industrial prototype."),
		"Sharad Shankar (http://www.logicbrick.com)\n" + Catalog.GetString("OpenCV Detection of knee angle."), 
		"Onkar Nath Mishra (http://www.logicbrick.com)\n" + Catalog.GetString("OpenCV Detection of knee angle."),
		"Andoni Morales (http://ylatuya.es)\n" + Catalog.GetString("Installation support: Autotools, packaging, bundle.") 
	};
	
	public static string ChronojumpWebsite = "http://www.chronojump.org";
	
	//formulas
	public static string DjIndexFormula = "Dj Index (tv-tc)/tc *100)";
	public static string QIndexFormula = "Q index (tv/tc)";
	public static string DjIndexFormulaOnly = "(tv-tc)*100/(tc*1.0)"; //*1.0 for having double division
	public static string QIndexFormulaOnly = "tv/(tc*1.0)"; //*1.0 for having double division
	
	public const string FvIndexFormula = "F/V sj+(100%)/sj *100";
	public const string IeIndexFormula = "IE (cmj-sj)/sj *100";
	public const string IubIndexFormula = "IUB (abk-cmj)/cmj *100";

	//tests types
	public enum TestTypes { JUMP, JUMP_RJ, RUN, RUN_I, RT, PULSE, MULTICHRONOPIC }
	public static string JumpSimpleName = "Jump simple";
	public static string JumpReactiveName = "Jump reactive";
	public static string RunSimpleName = "Run simple";
	public static string RunIntervallicName = "Run interval";
	public static string ReactionTimeName = "Reaction Time";
	public static string PulseName = "Pulse";

	//sqlite tables
	//instead of typing the tableName directly (that can crash if it's bad written and it's not detected by compiler)
	//use the following consts, and if it's misspelled, compiler will know
	public const string PersonTable = "person";
	public const string SessionTable = "session";
	public const string PersonSessionTable = "personSession";
	public const string PersonSessionWeightTable = "personSessionWeight";
	public const string PersonNotUploadTable = "personSessionNotUpload"; 
	public const string SportTable = "sport";
	public const string SpeciallityTable = "speciallity";
	public const string PreferencesTable = "preferences";
	public const string CountryTable = "country";
	public const string ConvertTempTable = "convertTemp"; //for conversions
	//tests
	public const string JumpTable = "jump";
	public const string JumpRjTable = "jumpRj";
	public const string TempJumpRjTable = "tempJumpRj";
	public const string RunTable = "run";
	public const string RunIntervalTable = "runInterval";
	public const string TempRunIntervalTable = "tempRunInterval";
	public const string PulseTable = "pulse";
	public const string ReactionTimeTable = "reactionTime";
	public const string MultiChronopicTable = "multiChronopic";
	public const string TempMultiChronopicTable = "tempMultiChronopic"; //TODO

	//tests types
	public const string JumpTypeTable = "jumpType";
	public const string JumpRjTypeTable = "jumpRjType";
	public const string RunTypeTable = "runType";
	public const string RunIntervalTypeTable = "runIntervalType";
	public const string PulseTypeTable = "pulseType";
	public const string ReactionTimeTypeTable = "reactionTimeType";

	public const string UndefinedDefault = "Undefined";
	public const string Any = "Any";

	public const string M = "M";
	public const string F = "F";
	public const string Males = "Males";
	public const string Females = "Females";
	public const int AnyID = -1;
	public const int MaleID = 1;
	public const int FemaleID = 0;

	//server	
	public const string ServerPingTable = "SPing"; 
	public const string ServerEvaluatorTable = "SEvaluator"; 
	public const string IPUnknown = "Unknown"; 
	public static int ServerUndefinedID = -1;
	public static string ServerOnline = Catalog.GetString("Server is connected.");
	public static string ServerOffline = Catalog.GetString("Sorry, server is currently offline. Try later.") + "\n" + 
		Catalog.GetString("Or maybe you are not connected to the Internet or your firewall is restricting connections");
	public enum ServerSessionStates {
		NOTHING, UPLOADINGSESSION, UPLOADINGDATA, DONE
	}
	public enum UploadCodes {
		EXISTS, SIMULATED, OK
	}
	public const string ServerActionUploadSession = "uploadSession"; 
	public const string ServerActionStats = "stats"; 
	public const string ServerActionQuery = "query"; 
	
	public const string ChronometerCp1 = "Chronopic1";
	public const string ChronometerCp2 = "Chronopic2";
	public const string ChronometerCp3 = "Chronopic3";
	public static string [] Chronometers = {
		UndefinedDefault, 
		ChronometerCp1,
		ChronometerCp2,
		ChronometerCp3,
	};
	public const string DeviceContactSteel = "Contact platform (steel)";
	public const string DeviceContactCircuit = "Contact platform (circuit board)";
	public const string DeviceInfrared = "Infrared";
	public static string [] Devices = {
		UndefinedDefault + ":" + Catalog.GetString(UndefinedDefault), 
		DeviceContactSteel + ":" + Catalog.GetString(DeviceContactSteel),
		DeviceContactCircuit + ":" + Catalog.GetString(DeviceContactCircuit),
		DeviceInfrared + ":" + Catalog.GetString(DeviceInfrared),
		"Other" + ":" + Catalog.GetString("Other"),
	};
	
	
	public static string MultiChronopicName = "MultiChronopic";
	public static string RunAnalysisName = "RunAnalysis"; //Josep Ma Padullés test
	public static string TakeOffName = "TakeOff"; //translate (take off?)
	public static string TakeOffWeightName = "TakeOffWeight"; //translate (take off?)



/*	OLD, check this
	public static string PotencyLewisCMJFormula = Catalog.GetString("Peak Power")+ " CMJ (Lewis) " +
		"(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")*9.81*" +
		"SQRT(2*9,81* " + Catalog.GetString("height") + "(m))";
*/
	public static string PotencyLewisFormula = Catalog.GetString("Peak power")+ " (Lewis, 1974) \n" +
		"(SQRT(4,9)*9,8*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ") * SQRT(" + Catalog.GetString("height") + "(m)))";
	
	public static string PotencyHarmanFormula = Catalog.GetString("Peak power") + " (Harman, 1991) \n" +
		"(61.9*" + Catalog.GetString("height") + "(cm))" +
	        "+ (36*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -1822";
	
	public static string PotencySayersSJFormula = Catalog.GetString("Peak power") + " SJ (Sayers, 1999) \n" +
		"(60.7*" + Catalog.GetString("height") + "(cm))" +
	        "+ (45.3*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -2055";
	
	public static string PotencySayersCMJFormula = Catalog.GetString("Peak power") + " CMJ (Sayers, 1999) \n" +
		"(51.9*" + Catalog.GetString("height") + "(cm))" +
	        "+ (48.9*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -2007";
	
	public static string PotencyShettyFormula = Catalog.GetString("Peak power") + " (Shetty, 2002) \n" +
		"(1925.72*" + Catalog.GetString("height") + "(cm))" +
	        "+ (14.74*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -66.3";
	
	public static string PotencyCanavanFormula = Catalog.GetString("Peak power") + " (Canavan, 2004) \n" +
		"(65.1*" + Catalog.GetString("height") + "(cm))" +
	        "+ (25.8*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -1413.1";
	
	/*
	public static string PotencyBahamondeFormula = Catalog.GetString("Peak power") + " (Bahamonde, 2005) \n" +
		"(78.5*" + Catalog.GetString("height") + "(cm))" +
	        "+ (60.6*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + 
		")) -(15.3*" + Catalog.GetString("height") + "(cm)) -1413.1";
	*/ //what is this height?
	
	public static string PotencyLaraMaleApplicantsSCFormula = Catalog.GetString("Peak power") + " (Lara, 2006, " + Catalog.GetString("Male applicants to a Faculty of Sport Sciencies") + ") \n" +
		"(62.5*" + Catalog.GetString("height") + "(cm))" +
	        "+ (50.3*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -2184.7";
	
	public static string PotencyLaraFemaleEliteVoleiFormula = Catalog.GetString("Peak power") + " (Lara, 2006, " + Catalog.GetString("Female elite volleybol") + ") \n" +
		"(83.1*" + Catalog.GetString("height") + "(cm))" +
	        "+ (42*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -2488";
	
	public static string PotencyLaraFemaleMediumVoleiFormula = Catalog.GetString("Peak power") + " (Lara, 2006, " + Catalog.GetString("Female medium volleybol") + ") \n" +
		"(53.6*" + Catalog.GetString("height") + "(cm))" +
	        "+ (67.5*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -2624.1";
	
	public static string PotencyLaraFemaleSCStudentsFormula = Catalog.GetString("Peak power") + " (Lara, 2006, " + Catalog.GetString("Female sports sciencies students") + ") \n" +
		"(56.7*" + Catalog.GetString("height") + "(cm))" +
	        "+ (47.2*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -1772.6";
	
	public static string PotencyLaraFemaleSedentaryFormula = Catalog.GetString("Peak power") + " (Lara, 2006, " + Catalog.GetString("Female university students") + ") \n" +
		"(68.2*" + Catalog.GetString("height") + "(cm))" +
	        "+ (40.8*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -1731.1";
	
	public static string RJPotencyBoscoFormula = "Peak Power" + " (Bosco)" + "\n 9.81^2*TF*TT / (4*jumps*(TT-TF))";
	public static string RJPotencyBoscoFormulaOnly = "9.81*9.81 * tvavg*jumps * time / ( 4.0 * jumps * (time - tvavg*jumps) )"; //*4.0 for having double division
	public static string RJPotencyBoscoName = Catalog.GetString("Peak Power") + " (Bosco)";

	public static string RjIndexName = "Rj Index";
	public static string RjIndexFormulaOnly = "(tvavg-tcavg)*100/(tcavg * 1.0)";
	public static string QIndexName = "Q Index";
	public static string RjIndexOnlyFormula = "(tf-tc)/tc *100)";
	public static string QIndexOnlyFormula = "(tf/tc)";
	public static string RJAVGSDRjIndexName = "Reactive AVG SD" + " (" + RjIndexName + ")";
	public static string RJAVGSDQIndexName = "Reactive AVG SD" + " (" + QIndexName + ")";


	//global stat types
	public static string TypeSessionSummary = Catalog.GetString("Session summary");
	public static string TypeJumperSummary = Catalog.GetString("Jumper summary");
	public static string TypeJumpsSimple = Catalog.GetString("Jumps: Simple");
	public static string TypeJumpsSimpleWithTC = Catalog.GetString("Jumps: Simple with TC");
	public static string TypeJumpsReactive = Catalog.GetString("Jumps: Reactive");

	//strings
	public static string AllJumpsName = Catalog.GetString("See all jumps");
	public static string AllRunsName = Catalog.GetString("See all runs");
	public static string AllPulsesName = Catalog.GetString("See all pulses");

	//fileNames
	public static string FileNameLogo = "chronojump_logo.png";
	public static string FileNameLogo320 = "chronojump_320.png";
	public static string FileNameCSS = "report_web_style.css";
	public static string FileNameIcon = "chronojump_icon.png";
	public static string FileNameIconGraph = "chronojump_icon_graph.png";
	public static string FileNameVersion = "version.txt";
	
	public static string FileNameRGraph = "r-graph.png";
	public static string FileNameRScript = "r-graph.txt";
	public static string GraphTypeBoxplot = "Boxplot";
	public static string GraphTypeBarplot = "Barplot";
	public static string GraphTypeLines = "Lines";
	public static string GraphTypeXY = "XY";
	public static string GraphTypeDotchart = "Dotchart";
	public static string GraphTypeStripchart = "Stripchart";
	public static string [] GraphTypes = { GraphTypeBoxplot, GraphTypeBarplot, GraphTypeLines, 
		GraphTypeXY, GraphTypeDotchart, GraphTypeStripchart };
	public static string GraphPaletteGray = "gray.colors";
	public static string [] GraphPalettes = { GraphPaletteGray, "rainbow", 
		"topo.colors", "heat.colors", "terrain.colors", "cm.colors" };


	public static string FileNameZoomFitIcon = "gtk-zoom-fit.png";
	//public static string FileNameZoomOutIcon = "gtk-zoom-out.png";
	public static string FileNameZoomInIcon = "gtk-zoom-in.png";
	public static string FileNameZoomInWithTextIcon = "gtk-zoom-in-with-text.png";
	
	public static string FileNameChronopic1 = "chronopic1.jpg";
	public static string FileNameChronopic2 = "chronopic2.jpg";
	public static string FileNameChronopic3 = "chronopic3.jpg";
	public static string FileNameContactPlatformSteel = "plataforma_contactos.jpg";
	public static string FileNameContactPlatformModular = "modular_platform_with_chronopic.jpg";
	public static string FileNameInfrared = "infrared.jpg";
	
	//30 colors defined
	public static string [] Colors = {
		"Blue", "Coral", "Cyan", "Gray", "Green", "Pink", "Salmon", "Yellow",
		"DarkBlue", "DarkCyan", "DarkGoldenrod", "DarkGray", 
		"DarkGreen", "DarkMagenta", "DarkSalmon",
		"LightBlue", "LightCoral", "LightCyan", "LightGoldenrodYellow", 
		"LightGray", "LightGreen", "LightPink", "LightSalmon", "LightYellow", 
		"MediumBlue", "MediumOrchid", "MediumPurple", "MediumTurquoise", "MediumVioletRed", "YellowGreen" 
	};
	
	//for windows, on linux it takes language ok from the locale
	public static string LanguageDefault = "en-GB:English (United Kingdom)";
	public static string [] Languages = {
		"ca-ES:Catalan", 
		"zh-CN:Chinese", 
		LanguageDefault, 
		"dz-BT:Dzongkha",
		"fi-FI:Finnish", 
		"fr-FR:French", 
		"oc-OC:Occitan",
		"pt-BR:Portuguese (Brazil)", 
		"pt-PT:Portuguese (Portugal)", 
		"es-ES:Spanish (Spain)", 
		"sv-SE:Swedish", 
		"vi-VN:Vietnamese", 
	};

	/* *** ATTENTIOn ***: dz-BT deactivated on Windows compilation...
	 * in the next release, do it better
	 */

	//TODO: add:
	//ar (when there ara not fuzzy lines)
	//see in both langs how to write as xx_XX
	

	public static string PortNamesWindows = 
		string.Format(Catalog.GetString("Typical serial and USB-serial ports on Windows:") + 
				"\n\t<i>COM1\tCOM2</i>\n\n" + 
				Catalog.GetString("Also, these are possible:") + 
				"\t<i>COM3 ... COM27</i>");
	
	public static string PortNamesLinux = 
		string.Format(Catalog.GetString("Typical serial serial ports on GNU/Linux:") + 
				"\t<i>/dev/ttyS0\t/dev/ttyS1</i>\n" +
				Catalog.GetString("Typical USB-serial ports on GNU/Linux:") +
				"\t<i>/dev/ttyUSB0\t/dev/ttyUSB1</i>" + "\n" + 
				Catalog.GetString("If you use Chronopic3, you will have an USB-serial port.")
				);

	public static string FoundSerialPortsString = Catalog.GetString("Serial ports found:"); 
	public static string FoundUSBSerialPortsString = Catalog.GetString("USB-serial ports found:"); 
	public static string NotFoundUSBSerialPortsString = Catalog.GetString("Not found any USB-serial ports.") + " " + Catalog.GetString("Is Chronopic connected?"); 

//	public static System.Media.SystemSound SoundCanStart = System.Media.SystemSounds.Question; 
//	public static System.Media.SystemSounds SoundGood = System.Media.SystemSounds.Exclamation; 
//	public static System.Media.SystemSounds SoundBad = System.Media.SystemSounds.Beep; 
	public enum SoundTypes {
		CAN_START, GOOD, BAD
	}
	//public soundTypes SoundType;

	//public static string GladeWindows = "chronojump.glade.";
	//public static string ImagesWindows = "chronojump.images.";
	//public static string ImagesMiniWindows = "chronojump.images.mini.";
	public static string GladeWindows = "";
	public static string ImagesWindows = "";
	public static string ImagesMiniWindows = "mini/";
	public static string GladeLinux = "";
	public static string ImagesLinux = "";
	public static string ImagesMiniLinux = "mini/";
	
	public static string UtilProgramsWindows = "utils\\windows";
	public static string UtilProgramsLinux = "utils/linux";
	
	public static string ExtensionProgramsLinux = "sh";
	public static string ExtensionProgramsWindows = "bat";

	public static int SportUndefinedID = 1;
	public static string SportUndefined = "--Undefined";
	public static string SportAny = "--Any";
	private static string dumbVariableForTranslatingSportUndefined = Catalog.GetString("--Undefined");
	private static string dumbVariableForTranslatingSportAny = Catalog.GetString("--Any");
	public static int SportNoneID = 2;
	public static string SportNone = "-None";
	private static string dumbVariableForTranslatingSportNone = Catalog.GetString("-None");

	public static int SpeciallityUndefinedID = -1;
	public static string SpeciallityUndefined = "Undefined"; 
	private static string dumbVariableForTranslatingSpeciallityUndefined = Catalog.GetString("Undefined");
	
	public static int RaceUndefinedID = -1;

	public static int CountryUndefinedID = 1;
	public static string CountryUndefined = "Undefined"; 
	public static string ContinentUndefined = "Undefined"; 
	public static string [] Continents = {
		ContinentUndefined + ":" + Catalog.GetString(ContinentUndefined), 
		"Africa" + ":" + Catalog.GetString("Africa"),
		"Antarctica" + ":" + Catalog.GetString("Antarctica"),
		"Asia" + ":" + Catalog.GetString("Asia"),
		"Europe" + ":" + Catalog.GetString("Europe"),
		"North America" + ":" + Catalog.GetString("North America"),
		"Oceania" + ":" + Catalog.GetString("Oceania"),
		"South America" + ":" + Catalog.GetString("South America"),
	};
	
	public static int Simulated = -1; 
	
	//levels of sport practice
	//int will go into person database
	//string will be shown in user language
	public static int LevelUndefinedID = -1;
	public static string LevelUndefined = "Undefined"; 
	private static string dumbVariableForTranslatingLevelUndefined = Catalog.GetString("Undefined");
	public static int LevelSedentaryID = 0; 
	public static string LevelSedentary = "Sedentary/Ocasional practice"; 
	private static string dumbVariableForTranslatingLevelSedentary = Catalog.GetString("Sedentary/Ocasional practice");
	public static string [] Levels = {
		LevelUndefinedID.ToString() + ":" + Catalog.GetString(LevelUndefined), 
		LevelSedentaryID.ToString() + ":" + Catalog.GetString(LevelSedentary), 
		"1:" + Catalog.GetString("Regular practice"), 
		"2:" + Catalog.GetString("Competition"), 
		"3:" + Catalog.GetString("Elite"), 
	};

	public static string [] SplashMessages = {
		Catalog.GetString("Initializing"),		//0
		Catalog.GetString("Checking database"),		//1
		Catalog.GetString("Creating database"),		//2
		Catalog.GetString("Making database backup"),	//3
		Catalog.GetString("Updating database"),		//4
		Catalog.GetString("Check for new version"),	//5
		Catalog.GetString("Preparing main Window"),	//6
	};

	public static string ChronopicDefaultPortWindows = "COM?";
	public static string ChronopicDefaultPortLinux = "/dev/ttyUSB?";
	
	public static string [] ComboPortLinuxOptions = {
		"/dev/ttyUSB?", 
		"/dev/ttyUSB0", 
		"/dev/ttyUSB1", 
		"/dev/ttyUSB2", 
		"/dev/ttyUSB3", 
		"/dev/ttyS0", 
		"/dev/ttyS1", 
		"/dev/ttyS2", 
		"/dev/ttyS3", 
	};
		
	

	//for dialog windows
	public enum MessageTypes {
		WARNING, INFO, HELP
	}

	public static string No = Catalog.GetString("No");
	public static string Yes = Catalog.GetString("Yes");

	public static string In = Catalog.GetString("In");
	public static string Out = Catalog.GetString("Out");

	//it's important they are two chars long
	//public static string EqualThanCode = "= ";
	public static string LowerThanCode = "< ";
	//public static string HigherThanCode = "> ";
	//public static string LowerOrEqualThanCode = "<=";
	public static string HigherOrEqualThanCode = ">=";
	
	public const string PrefVersionAvailable = "versionAvailable";

}
