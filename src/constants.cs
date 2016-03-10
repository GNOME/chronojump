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
 *  Copyright (C) 2004-2016   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Mono.Unix;
//do not use gtk or gdk because this class is used by the server
//see UtilGtk for eg color definitions

public class Constants
{
	//the strings created by Catalog cannot be const
	
	//public static string ReadmeTranslators = Catalog.GetString("Translator, there's a glossary that will help you in Chronojump translation:\n http://git.gnome.org/browse/chronojump/plain/glossary/chronojump_glossary_for_translators.html");
	
	public static string [] AuthorsCEO = {
		"Xavier de Blas Foix (info@chronojump.org)\n",
		"Josep Ma Padullés (jmpadulles@gmail.com)"	
	};
	public static string [] AuthorsSoftware = {
		"Xavier de Blas Foix (xaviblas@gmail.com)\n\t" + 
			Catalog.GetString("Main developer.") + "\n",
		"Andoni Morales Alastruey (http://ylatuya.es)\n\t" + 
			Catalog.GetString("Installation support: Autotools, packaging, bundle.") + "\n",
		"Carles Pina i Estany (http://pinux.info)\n\t" + 
			Catalog.GetString("Backend developer.")
	};
	public static string [] AuthorsChronopic = {
		"Teng Wei Hua (wadedang@gmail.com)\n\t" + Catalog.GetString("Translation of Firmware to C.") + "\n\t" +
			Catalog.GetString("New firmware features.") + " " + Catalog.GetString("Encoder hardware layer.") + "\n",
		"Juan Gonzalez Gómez (http://www.iearobotics.com)\n\t" + Catalog.GetString("Skypic, Chronopic, connection between hardware and software.") + "\n", 
		"Ferran Suárez Rodríguez (ferransuarez2@gmail.com)\n\t" + Catalog.GetString("Chronopic reaction time advanced implementation.") + "\n",
		"Ricardo Gómez González (http://www.iearobotics.com)\n\t" + Catalog.GetString("Chronopic3 industrial prototype.") + "\n",
		"Juan Fernando Pardo (juanfer@juanfer.com.ar)\n\t" + "Chronopic2."
	};
	public static string [] AuthorsDevices = {
		"Josep Ma Padullés (jmpadulles@gmail.com)\n",
		"Anna Padullés (hardware@chronojump.org)\n",
		"Xavier Padullés (testing@chronojump.org)\n",
		"Teng Wei Hua (wadedang@gmail.com)\n",
		"Xavier de Blas Foix (info@chronojump.org)\n",
		"Ferran Suárez Rodríguez (ferransuarez2@gmail.com)\n"
	};
	public static string [] AuthorsMath = {
		"Carlos J. Gil Bellosta (http://www.datanalytics.com)\n",
		"Aleix Ruiz de Villa (aleixrvr@gmail.com)\n",
		"Xavier Padullés (testing@chronojump.org)"
	};
	public static string [] AuthorsOpenCV = {
		"Sharad Shankar (http://www.logicbrick.com)\n", 
		"Onkar Nath Mishra (http://www.logicbrick.com)\n"
	};

	public static string [] Documenters = {
		"Xavier de Blas Foix (xaviblas@gmail.com)\n\t" +
			Catalog.GetString("Chronojump Manual author."),
		"Helena Olsson (hjolsson@gmail.com)\n\t" +
			Catalog.GetString("Chronojump Manual English translation."),
		"Xavier Padullés (testing@chronojump.org)",
	};
	
	public static string ChronojumpWebsite = "http://www.chronojump.org";
	
	//formulas
	public static string DjIndexFormula = "Dj Index: (tv-tc)/tc *100)";
	public static string QIndexFormula = "Q index: (tv/tc)";
	public static string DjPowerFormula = "Dj Power: mass*g*(fallHeight+1.226*(tv^2)) / (tc+tv)";
	public static string DjIndexFormulaOnly = "(tv-tc)*100/(tc*1.0)"; //*1.0 for having double division
	public static string QIndexFormulaOnly = "tv/(tc*1.0)"; //*1.0 for having double division
	public static string DjPowerFormulaOnly = PersonSessionTable + ".weight * 9.81 * (fall/100.0 + 1.226 * (tv*tv) ) / ((tc+tv)*1.0)";
	
	public static string ChronojumpProfile = Catalog.GetString("Chronojump profile");
	public const string FvIndexFormula = "F/V sj+(100%)/sj *100";
	public const string IeIndexFormula = "IE (cmj-sj)/sj *100";
	public const string IRnaIndexFormula = "IRna (djna-cmj)/cmj *100";
	public const string IRaIndexFormula = "IRa (dja-cmj)/cmj *100";
	
	public const string ArmsUseIndexFormula = "Arms Use Index (abk-cmj)/cmj *100";
	public const string ArmsUseIndexName = "Arms Use Index";

	public const string SubtractionBetweenTests = "Subtraction between tests";

	//tests types (dont' use character '-' will be used multimedia file names)
	public enum TestTypes { JUMP, JUMP_RJ, RUN, RUN_I, RT, PULSE, MULTICHRONOPIC, ENCODER }
	public static string JumpSimpleName = "Jump simple";
	public static string JumpReactiveName = "Jump reactive";
	public static string RunSimpleName = "Run simple";
	public static string RunIntervallicName = "Run interval";
	public static string ReactionTimeName = "Reaction Time";
	public static string PulseName = "Pulse";

	//sqlite tables
	//instead of typing the tableName directly (that can crash if it's bad written and it's not detected by compiler)
	//use the following consts, and if it's misspelled, compiler will know
	public const string PersonTable = "person77";
	public const string PersonSessionTable = "personSession77";
	
	public const string PersonOldTable = "person";
	public const string PersonSessionOldTable = "personSession"; //old table, used before db 0.53
	public const string PersonSessionOldWeightTable = "personSessionWeight"; //old table, used from db 0.53 to 0.76

	public const string SessionTable = "session";
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
	public const string EncoderTable = "encoder";
	public const string EncoderSignalCurveTable = "encoderSignalCurve";
	public const string EncoderExerciseTable = "encoderExercise";
	public const string Encoder1RMTable = "encoder1RM";
	public const string ExecuteAutoTable = "executeAuto";

	//tests types
	public const string JumpTypeTable = "jumpType";
	public const string JumpRjTypeTable = "jumpRjType";
	public const string RunTypeTable = "runType";
	public const string RunIntervalTypeTable = "runIntervalType";
	public const string PulseTypeTable = "pulseType";
	public const string ReactionTimeTypeTable = "reactionTimeType";
	public const string GraphLinkTable = "graphLinkTable";

	public const string UndefinedDefault = "Undefined";
	public const string Any = "Any";
	private static string dumbVariableForTranslatingAny = Catalog.GetString("Any");

	public const string M = "M";
	public const string F = "F";
	public const string Males = "Males";
	public const string Females = "Females";
	public const int AnyID = -1;
	public const int MaleID = 1;
	public const int FemaleID = 0;

	//person & personSession stuff
	public const string Name = "name";
	public const string Height = "height";
	public const string Weight = "weight";

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


	public static string SpreadsheetString = "\n\n" + Catalog.GetString("When import from your spreadsheet (OpenOffice, R, MS Excel, ...)\nremember the separator character is semicolon <b>;</b>, or comma <b>,</b>.") + 
			"\n\n" + Catalog.GetString("This can be changed on preferences.");

/*	OLD, check this
	public static string PotencyLewisCMJFormula = Catalog.GetString("Peak Power")+ " CMJ (Lewis) " +
		"(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")*9.81*" +
		"SQRT(2*9,81* " + Catalog.GetString("height") + "(m))";
*/
	public static string PotencyLewisFormulaShort = Catalog.GetString("Peak power") + " (Lewis, 1974) " +
		Catalog.GetString("(Watts)");
	public static string PotencyLewisFormula = PotencyLewisFormulaShort + "\n" +
		"(SQRT(4,9)*9,8*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ") * SQRT(" + Catalog.GetString("height") + "(m)))";
	//see: http://www.dtic.mil/cgi-bin/GetTRDoc?AD=ADA218194&Location=U2&doc=GetTRDoc.pdf
	//Estimation of human power output from maximal vertical jump and body mass
	//1988
	//Everett A. Harman, Michael T. Rosenstein, Peter N.
	//Frykman, Richard M. Rosenstein and William J.
	//Kraemer
	//The Lewis formula, and nomogram based on the formula, have
	//become widely used among coaches, physical educators, and researchers to
	//estimate power output during the vertical jump-and-reach test (1,8).
	//According to the formula,
	//POWERkg.. = SQRT(4.9) .WEIGHTkg.SQRT(JUMP-REACH SCOREm), (1)
	//The formula and nomogram appear to have been first published in
	//1974 in a book on interval training by Fox and Mathews (4). The only
	//reference provided for the formula was a note stating "Courtesy, Office of
	//Naval Research". The formula and nomogram were popularized in the
	//1976 and 1981 editions of the widely used exercise physiology textbook by
	//Fox and Mathews (3,7), and have been more recently published in a book
	//on tests and measurements for physical educators (5).
	//A phone conversation with Dr. Mathews revealed that he developed
	//the formula and nomogram in conjunction with his student, Mr. Lewis.
	//Development of the nomogram was funded in part by the Office of Naval
	//Research.
	//An obvious problem with the formula is that it does not use
	//standard units. Power should be measured in watts, which are
	//newton-meters per second. Kilograms are units of mass, not weight or
	//force. The following adjusted version of the formula includes the multiplier
	//9.8 (the acceleration of gravity in m/sec2), which converts kilograms to
	//newtons, yielding power in watts (N.m/s).
	//POWERw = (SQRT(4.9))(9.8)(BODY MASSkg)(SQRT(JUMP-REACH SCOREm)) (2)
	
	public static string PotencyHarmanFormulaShort = Catalog.GetString("Peak power") + " (Harman, 1991)";
	public static string PotencyHarmanFormula = PotencyHarmanFormulaShort + "\n" +
		"(61.9*" + Catalog.GetString("height") + "(cm))" +
	        "+ (36*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -1822";
	
	public static string PotencySayersSJFormulaShort = Catalog.GetString("Peak power") + " SJ (Sayers, 1999)";
	public static string PotencySayersSJFormula = PotencySayersSJFormulaShort + "\n" +
		"(60.7*" + Catalog.GetString("height") + "(cm))" +
	        "+ (45.3*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -2055";
	
	public static string PotencySayersCMJFormulaShort = Catalog.GetString("Peak power") + " CMJ (Sayers, 1999)";
	public static string PotencySayersCMJFormula = PotencySayersCMJFormulaShort + "\n" +
		"(51.9*" + Catalog.GetString("height") + "(cm))" +
	        "+ (48.9*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -2007";

	//http://www.ncbi.nlm.nih.gov/pubmed/14658372	
	public static string PotencyShettyFormulaShort = Catalog.GetString("Peak power") + " (Shetty, 2002)";
	public static string PotencyShettyFormula = PotencyShettyFormulaShort + "\n" +
		"(1925.72*" + Catalog.GetString("height") + "(m))" +
	        "+ (14.74*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -666.3";
	
	public static string PotencyCanavanFormulaShort = Catalog.GetString("Peak power") + " (Canavan, 2004)";
	public static string PotencyCanavanFormula = PotencyCanavanFormulaShort + "\n" +
		"(65.1*" + Catalog.GetString("height") + "(cm))" +
	        "+ (25.8*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -1413.1";
	
	/*
	public static string PotencyBahamondeFormula = Catalog.GetString("Peak power") + " (Bahamonde, 2005) \n" +
		"(78.5*" + Catalog.GetString("height") + "(cm))" +
	        "+ (60.6*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + 
		")) -(15.3*" + Catalog.GetString("height") + "(cm)) -1413.1";
	*/ //what is this height?
	
	public static string PotencyLaraMaleApplicantsSCFormulaShort = 
		Catalog.GetString("Peak power") + " (Lara, 2006, m)";
	public static string PotencyLaraMaleApplicantsSCFormula = PotencyLaraMaleApplicantsSCFormulaShort + 
		" (" + Catalog.GetString("Male applicants to a Faculty of Sport Sciences") + ") \n" +
		"(62.5*" + Catalog.GetString("height") + "(cm))" +
	        "+ (50.3*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -2184.7";
	
	public static string PotencyLaraFemaleEliteVoleiFormulaShort = 
		Catalog.GetString("Peak power") + " (Lara, 2006, fev)"; 
	public static string PotencyLaraFemaleEliteVoleiFormula = PotencyLaraFemaleEliteVoleiFormulaShort + 
		" (" + Catalog.GetString("Female elite volleyball") + ") \n" +
		"(83.1*" + Catalog.GetString("height") + "(cm))" +
	        "+ (42*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -2488";
	
	public static string PotencyLaraFemaleMediumVoleiFormulaShort = 
		Catalog.GetString("Peak power") + " (Lara, 2006, fmv)";
	public static string PotencyLaraFemaleMediumVoleiFormula = PotencyLaraFemaleMediumVoleiFormulaShort +
		" (" + Catalog.GetString("Female medium volleyball") + ") \n" +
		"(53.6*" + Catalog.GetString("height") + "(cm))" +
	        "+ (67.5*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -2624.1";
	
	public static string PotencyLaraFemaleSCStudentsFormulaShort = 
		Catalog.GetString("Peak power") + " (Lara, 2006, fsc)";
	public static string PotencyLaraFemaleSCStudentsFormula = PotencyLaraFemaleSCStudentsFormulaShort +
		" (" + Catalog.GetString("Peak power") + " (Lara, 2006, " + 
		Catalog.GetString("Female sports sciences students") + ") \n" +
		"(56.7*" + Catalog.GetString("height") + "(cm))" +
	        "+ (47.2*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -1772.6";
	
	public static string PotencyLaraFemaleSedentaryFormulaShort = Catalog.GetString("Peak power") + " (Lara, 2006, fu)";
	public static string PotencyLaraFemaleSedentaryFormula = PotencyLaraFemaleSedentaryFormulaShort +
		" (" + Catalog.GetString("Peak power") + " (Lara, 2006, " + Catalog.GetString("Female university students") + ") \n" +
		"(68.2*" + Catalog.GetString("height") + "(cm))" +
	        "+ (40.8*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -1731.1";
	
	public static string RJPotencyBoscoFormula = "Peak Power" + " (Bosco)" + " 9.81^2*TF*TT / (4*jumps*(TT-TF))";
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
	public static string TypeJumpsSimple = Catalog.GetString("Simple");
	public static string TypeJumpsSimpleWithTC = Catalog.GetString("Simple with TC");
	public static string TypeJumpsReactive = Catalog.GetString("Jumps: Reactive");
	public static string TypeRunsSimple = Catalog.GetString("Runs: Simple");
	public static string TypeRunsIntervallic = Catalog.GetString("Runs: Intervallic");

	//strings
	public static string AllJumpsName = Catalog.GetString("See all jumps");
	public static string AllRunsName = Catalog.GetString("See all runs");
	public static string AllPulsesName = Catalog.GetString("See all pulses");

	//fileNames
	//public static string FileNameLogo = "chronojump-boscosystem_white_bg.png";
	//public static string FileNameLogo320 = "chronojump-boscosystem_320.png";
	public static string FileNameLogo = "chronojump-logo-2013.png";
	public static string FileNameLogo320 = "chronojump-logo-2013_320.png";
	public static string FileNameCSS = "report_web_style.css";
	public static string FileNameIcon = "chronojump_icon.png";
	public static string FileNameIconGraph = "chronojump_icon_graph.png";
	public static string FileNameVersion = "version.txt";
	
	public static string FileNameRGraph = "r-graph.png";
	public static string FileNameRScript = "r-graph.txt";
	public static string GraphTypeBoxplot = "Boxplot";
	public static string GraphTypeBarplot = "Barplot";
	public static string GraphTypeHistogram = "Histogram";
	public static string GraphTypeLines = "Lines";
	public static string GraphTypeXY = "Dispersion";
	public static string GraphTypeDotchart = "Dotchart";
	public static string GraphTypeStripchart = "Stripchart";
	public static string [] GraphTypes = { GraphTypeBoxplot, GraphTypeHistogram, GraphTypeBarplot, GraphTypeLines, 
		GraphTypeXY, GraphTypeDotchart, GraphTypeStripchart };
	public static string [] GraphTypesMultisession = { GraphTypeBarplot, GraphTypeLines };
	public static string GraphPaletteGray = "gray.colors";
	public static string GraphPaletteBlack = Catalog.GetString("black only");
	public static string [] GraphPalettes = { GraphPaletteBlack, GraphPaletteGray, "rainbow", 
		"topo.colors", "heat.colors", "terrain.colors", "cm.colors"};


	public static string FileNameZoomFitIcon = "gtk-zoom-fit.png";
	//public static string FileNameZoomOutIcon = "gtk-zoom-out.png";
	public static string FileNameZoomInIcon = "gtk-zoom-in.png";
	public static string FileNameZoomInWithTextIcon = "gtk-zoom-in-with-text.png";
	public static string FileNameNew1 = "gtk-new-1.png";
	public static string FileNameNewPlus = "gtk-new-plus.png";
	public static string FileNameOpen = "gtk-open.png";
	public static string FileNameOpen1 = "gtk-open-1.png";
	public static string FileNameOpenPlus = "gtk-open-plus.png";

	public static string FileNameCSVHeadersIcon = "import-csv-headers.png";
	public static string FileNameCSVNoHeadersIcon = "import-csv-noheaders.png";
	public static string FileNameCSVName1Icon = "import-csv-name-1-column.png";
	public static string FileNameCSVName2Icon = "import-csv-name-2-columns.png";

	public static string FileNameJumps = "stock_up.png";
	public static string FileNameJumpsRJ = "stock_up_down.png";
	public static string FileNameRuns = "stock_right.png";
	public static string FileNameRunsInterval = "stock_right_left.png";
	public static string FileNameReactionTime = "reaction_time_menu.png";
	public static string FileNamePulse = "pulse_menu.png";
	public static string FileNameMultiChronopic = "multichronopic_menu.png";
	
	public static string FileNameJumpsFallCalculate = "dj-from-in.png";
	public static string FileNameJumpsFallPredefined = "dj-from-out.png";
	
	public static string FileNameChronopic1 = "chronopic1.jpg";
	public static string FileNameChronopic2 = "chronopic2.jpg";
	public static string FileNameChronopic3 = "chronopic3.jpg";
	public static string FileNameContactPlatformSteel = "plataforma_contactos.jpg";
	public static string FileNameContactPlatformModular = "modular_platform_with_chronopic.jpg";
	public static string FileNameInfrared = "infrared.jpg";
	
	public static string FileNameEncoderAnalyzeCurrentSignalIcon = "encoder-analyze-current-signal.png";
	public static string FileNameEncoderAnalyzeSavedCurvesIcon = "encoder-analyze-saved-curves.png";

	public static string FileNameEncoderAnalyzePowerbarsIcon = "encoder-analyze-powerbars.png";
	public static string FileNameEncoderAnalyzeCrossIcon = "encoder-analyze-cross.png";
	public static string FileNameEncoderAnalyze1RMIcon = "encoder-analyze-1RM.png";
	public static string FileNameEncoderAnalyzeSideIcon = "encoder-analyze-side.png";
	public static string FileNameEncoderAnalyzeSingleIcon = "encoder-analyze-single.png";
	public static string FileNameEncoderAnalyzeNmpIcon = "encoder-analyze-nmp.png";
	
	public static string FileNameEncoderAnalyzeEcconTogetherIcon = "encoder-analyze-eccon-together.png";
	public static string FileNameEncoderAnalyzeEcconSeparatedIcon = "encoder-analyze-eccon-separated.png";
	
	public static string FileNameEncoderAnalyzeSpeedIcon = "encoder-analyze-speed.png";
	public static string FileNameEncoderAnalyzeAccelIcon = "encoder-analyze-accel.png";
	public static string FileNameEncoderAnalyzeForceIcon = "encoder-analyze-force.png";
	public static string FileNameEncoderAnalyzePowerIcon = "encoder-analyze-power.png";
	
	public static string FileNameEncoderAnalyzeMeanIcon = "encoder-analyze-mean.png";
	public static string FileNameEncoderAnalyzeMaxIcon = "encoder-analyze-max.png";
	public static string FileNameEncoderAnalyzeRangeIcon = "encoder-analyze-range.png";
	public static string FileNameEncoderAnalyzeTimeToPPIcon = "encoder-analyze-time-to-pp.png";
	public static string FileNameEncoderInertialInstructions = "inertial-start.png";
	
	public static string FileNameAutoPersonSkipIcon = "auto-person-skip.png";
	public static string FileNameAutoPersonRemoveIcon = "auto-person-remove.png";
	
	public static string FileNameSelectorJumps = "chronojump-jumps-small.png";
	public static string FileNameSelectorRuns = "chronojump-runs-small.png";
	public static string FileNameSelectorEncoder = "chronojump-encoder-small.jpg";

	public static string FileNameLog = "log_chronojump.txt";
	public static string FileNameLogOld = "log_chronojump_old.txt";
	
	public static string FileNameConfig = "chronojump_config.txt";
	
	//30 colors defined
	//see als UtilGtk that's not used by the server
	public static string [] Colors = {
		"Blue", "Coral", "Cyan", "Gray", "Green", "Pink", "Salmon", "Yellow",
		"DarkBlue", "DarkCyan", "DarkGoldenrod", "DarkGray", 
		"DarkGreen", "DarkMagenta", "DarkSalmon",
		"LightBlue", "LightCoral", "LightCyan", "LightGoldenrodYellow", 
		"LightGray", "LightGreen", "LightPink", "LightSalmon", "LightYellow", 
		"MediumBlue", "MediumOrchid", "MediumPurple", "MediumTurquoise", "MediumVioletRed", "YellowGreen" 
	};


	public static string PortNamesWindows = 
		string.Format(Catalog.GetString("Typical serial and USB-serial ports on Windows:") + 
				"\n\t<i>COM1\tCOM2</i>\n\n" + 
				Catalog.GetString("Also, these are possible:") + 
				"\t<i>COM3 ... COM27</i>");
	
	public static string PortNamesLinux = 
		string.Format(Catalog.GetString("Typical serial ports on GNU/Linux:") + 
				"\t<i>/dev/ttyS0\t/dev/ttyS1</i>\n" +
				Catalog.GetString("Typical USB-serial ports on GNU/Linux:") +
				"\t<i>/dev/ttyUSB0\t/dev/ttyUSB1</i>" + "\n" + 
				Catalog.GetString("If you use Chronopic3, you will have an USB-serial port.")
				);

	public static string FoundSerialPortsString = Catalog.GetString("Serial ports found:"); 
	public static string FoundUSBSerialPortsString = Catalog.GetString("USB-serial ports found:"); 
	public static string NotFoundUSBSerialPortsString = Catalog.GetString("Not found any USB-serial ports.") + " " + Catalog.GetString("Is Chronopic connected?"); 

	public static string ChronopicDetecting = Catalog.GetString("Detecting ...");
	public static string ChronopicNeedTouch = Catalog.GetString("Touch device.");

	public static string FindDriverNeed = Catalog.GetString("Chronopic driver has to be installed.");
	public static string FindDriverWindows = Catalog.GetString("If you have problems connecting with Chronopic, ensure you have the <b>driver</b> installed at 'Windows Start Menu / Chronojump / Install Chronopic driver'."); 
	public static string FindDriverOthers = Catalog.GetString("Check Chronojump software website.");

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
	public static string SimulatedMessage = Catalog.GetString("Tests are <b>simulated</b> until Chronopic is connected.");
	public static string SimulatedTreeview = " (" + Catalog.GetString("Simulated") + ")"; 
	
	public static string ChronopicOne = Catalog.GetString("All tests available except MultiChronopic.");
	public static string ChronopicMore = Catalog.GetString("All tests available.");

	
	//levels of sport practice
	//int will go into person database
	//string will be shown in user language
	public static int LevelUndefinedID = -1;
	public static string LevelUndefined = "Undefined"; 
	private static string dumbVariableForTranslatingLevelUndefined = Catalog.GetString("Undefined");
	public static int LevelSedentaryID = 0; 
	public static string LevelSedentary = "Sedentary/Occasional practice"; 
	private static string dumbVariableForTranslatingLevelSedentary = Catalog.GetString("Sedentary/Occasional practice");
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
		Catalog.GetString("Connecting to server"),	//5
		Catalog.GetString("Preparing main Window"),	//6
	};
	
	public static string DatabaseNotFound = Catalog.GetString("Error. Cannot find database.");
	public static string DirectoryCannotOpen = Catalog.GetString("Error. Cannot open directory.");
	public static string FileNotFound = Catalog.GetString("Error. File not found.");
	public static string FileCopyProblem = Catalog.GetString("Error. Cannot copy file.");

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

	//heightmetric contains 2 spins
	public enum GenericWindowShow {
		ENTRY, ENTRY2, ENTRY3, SPININT, SPININT2, SPININT3, SPINDOUBLE, HBOXSPINDOUBLE2, 
		HEIGHTMETRIC, COMBO, COMBOALLNONESELECTED, TEXTVIEW, TREEVIEW
	}


	public static string HelpPower =
		Catalog.GetString("On jumps results tab, power is calculated depending on jump type:") + 
		"\n\n" +
		//Catalog.GetString("Jumps with TC & TF: Bosco Relative Power (W/Kg)") + 
		//"\n" +
		//Catalog.GetString("P = 24.6 * (Total time + Flight time) / Contact time") + 
		Catalog.GetString("Jumps with TC and TF:") + " " + Catalog.GetString("Developed by Chronojump team") + 
		"\n" +
		Catalog.GetString("Calcule the potential energies on fall and after the jump.") + "\n" + 
		Catalog.GetString("Divide them by time during force is applied.") +
		"\n\n" +
		//P = mass * g * ( fallHeight + 1.226 * Math.Pow(tf,2) ) / (Double)tt;
		"P = " + Catalog.GetString("mass") + " * g * ( " + 
		Catalog.GetString("falling height") + " + 1.226 * " + Catalog.GetString("flight time") + " ^ 2 ) / " + 
		Catalog.GetString("contact time") +
		"\n\n" +
		Catalog.GetString("Jumps without TC: Lewis Peak Power 1974 (W)") + 
		"\n\n" +
		Catalog.GetString("P = SQRT(4.9) * 9.8 * (body weight+extra weight) * SQRT(jump height in meters)") + 
		"\n\n" +
		Catalog.GetString("If you want to use other formulas, go to Analyze.");
	
	public static string HelpStiffness =
		"M: " + Catalog.GetString("Mass") + "\n" +
		"Tc: " + Catalog.GetString("Contact Time") + "\n" +
		"Tf: " + Catalog.GetString("Flight Time") + "\n\n" +
		Catalog.GetString("See:") + "\n" +
		"Dalleau, G; Belli, A; Viale, F; Lacour, JR; and Bourdin, M. (2004). " + 
		"A simple method for field measurements of leg stiffness in hopping. " +
		"Int J Sports Med 25: 170–176";
	
	public const string PrefVersionAvailable = "versionAvailable";
	
	/*
	MultimediaStorage
	BYSESSION samples:
	Chronojump/multimedia/videos/sessionID/JUMP-jumpID
	Chronojump/multimedia/photos/sessionID/RUN_I-runIntervalID
	//see Constants.TestTypes 
	*/
	public enum MultimediaStorage {
		BYSESSION
	}
	
	public enum MultimediaItems {
		PHOTO, VIDEO
	}
	public const string ExtensionVideo = ".avi";
	public const string ExtensionPhoto = ".jpg";
	public const string SmallPhotoDir = "small";
	public static string MultimediaFileNoExists = Catalog.GetString("Sorry, this multimedia file does not exists.");
	public static string PhotoTemp = "chronojump-last-photo";
	public static string PhotoSmallTemp = "chronojump-last-photo-small";
	public static string VideoTemp = "chronojump-last-video";
	
	public static string RunStartInitialSpeedYes = Catalog.GetString("Running start. Started with initial speed.");
	public static string RunStartInitialSpeedNo = Catalog.GetString("Standing start. Started without initial speed.");
	
	public static string CameraNotFound = Catalog.GetString("Sorry, no cameras found.");
	
	public enum BellModes {
		JUMPS, RUNS, ENCODER
	}

	public static string All = "All";
	public static string None = "None";
	public static string Invert = "Invert";
	public static string Selected = "Selected";

	/*
	 * encoder storage
	 * chronojump / encoder / sessionID / data
	 * chronojump / encoder / sessionID / graphs
	 */
	
	/*
	 * The installer as from R 3.2.2 puts links to R and Rscript
	 * in /usr/bin (Mavericks, Yosemite) or /usr/local/bin (El Capitan and later).
	 * If these are missing, you can run directly the versions in /Library/Frameworks/R.framework/Resources/.
	 * https://cran.r-project.org/doc/manuals/r-devel/R-admin.pdf
	 */
	public static string ROSX = "/Library/Frameworks/R.framework/Resources/R";
	public static string RScriptOSX = "/Library/Frameworks/R.framework/Resources/RScript";
	
	//public static string EncoderScriptCapturePythonLinux = "pyserial_pyper.py";
	//public static string EncoderScriptCapturePythonWindows = "pyserial_pyper_windows.exe";
	public static string EncoderScriptCallCaptureNoRDotNet = "call_capture.R";
	public static string EncoderScriptCaptureNoRDotNet = "capture.R";
	public static string EncoderScriptCallGraph = "call_graph.R";
	public static string EncoderScriptGraph = "graph.R";
	public static string EncoderScriptInertiaMomentum = "inertia-momentum.R";
	public static string EncoderScriptUtilR = "util.R";
	public static string EncoderScriptNeuromuscularProfile = "neuromuscularProfile.R";
	//no longer used:
	//public static string EncoderScriptGraphCall = 
		//"/home/xavier/informatica/progs_meus/chronojump/chronojump/encoder/call_graph.py";

	public static string EncoderCaptureTemp = "chronojump-captured"; //will be "chronojump-captured-000.txt", 001 ... 999
	public static string EncoderDataTemp = "chronojump-last-encoder-data.txt";
	public static string EncoderCurvesTemp = "chronojump-last-encoder-curves.txt";
	public static string EncoderAnalyzeTableTemp = "chronojump-last-encoder-analyze-table.txt";
	public static string EncoderGraphTemp = "chronojump-last-encoder-graph.png";
	
	//this file will have a ...1.txt ...2.txt ... we check now the first part of the file
	public static string EncoderStatusTempBase = "chronojump-encoder-status-";
	
	public static string EncoderExportTemp = "chronojump-export.csv";
	public static string EncoderSpecialDataTemp = "chronojump-special-data.txt"; //variable;result (eg. "1RM;82.78")
	public static string EncoderInstantDataTemp = "chronojump-analysis-instant.csv";

	//note next has 40 chars, and its used also in encoder/graph.R to detect how a file will be treated
	//if this name changes, change it in encoder/graph.R
	public static string EncoderGraphInputMulti = "chronojump-encoder-graph-input-multi.csv"; 

	public static string Concentric = "Concentric";
	public static string EccentricConcentric = "Eccentric-concentric";
	//public static string ConcentricEccentric = "Concentric-eccentric";

	public enum EncoderCheckFileOp { CAPTURE_EXPORT_ALL, ANALYZE_SAVE_IMAGE, ANALYZE_SAVE_AB, ANALYZE_SAVE_TABLE}

	public static double EncoderErrorCode = -1;
	
	public static string FileNameEncoderImagePending = "encoder-image-pending.png";
	
	//three encoder types
	public static string FileNameEncoderTypeLinear = "encoder-linear.png";
	public static string FileNameEncoderTypeRotaryFriction = "encoder-rotary-friction.png";
	public static string FileNameEncoderTypeRotaryAxis = "encoder-rotary-axis.png";
	
	//encoder configurations
	//linear
	public static string FileNameEncoderLinearFreeWeight = "encoder-linear-free-weight.png";
	public static string FileNameEncoderLinearFreeWeightInv = "encoder-linear-free-weight-inv.png";
	public static string FileNameEncoderLinearInertial = "encoder-linear-inertial.png";
	public static string FileNameEncoderWeightedMovPulleyOnPerson1 = "encoder-linear-on-person-weighted-moving-pulley1.png";
	public static string FileNameEncoderWeightedMovPulleyOnPerson1Inv = "encoder-linear-inv-on-person-weighted-moving-pulley1.png";
	public static string FileNameEncoderWeightedMovPulleyOnPerson2 = "encoder-linear-on-person-weighted-moving-pulley2.png";
	public static string FileNameEncoderWeightedMovPulleyOnPerson2Inv = "encoder-linear-inv-on-person-weighted-moving-pulley2.png";
	public static string FileNameEncoderWeightedMovPulleyOnLinearEncoder = "encoder-linear-on-weighted-moving-pulley.png";
	public static string FileNameEncoderLinearOnPlane = "encoder-linear-inclined-plane.png";	
	public static string FileNameEncoderLinearOnPlaneWeightDiffAngle = "encoder-linear-inclined-plane-weight-diff-angle.png";	

	//rotary friction
	public static string FileNameEncoderFrictionSide = "encoder-rotary-friction-pulley.png";
	public static string FileNameEncoderFrictionAxis = "encoder-rotary-friction-pulley-axis.png";
	public static string FileNameEncoderFrictionSideInertial = "encoder-rotary-friction-side-inertial.png";
	public static string FileNameEncoderFrictionAxisInertial = "encoder-rotary-friction-axis-inertial.png";
	public static string FileNameEncoderFrictionSideInertialLateral = "encoder-rotary-friction-side-inertial-lateral.png";
	public static string FileNameEncoderFrictionAxisInertialLateral = "encoder-rotary-friction-axis-inertial-lateral.png";
	public static string FileNameEncoderFrictionSideInertialMovPulley = "encoder-rotary-friction-side-inertial-mov-pulley.png";
	public static string FileNameEncoderFrictionAxisInertialMovPulley = "encoder-rotary-friction-axis-inertial-mov-pulley.png";
	public static string FileNameEncoderFrictionWithMovPulley = "encoder-rotary-friction-on-fixed-pulley-with-weighted-moving-pulley.png";

	//rotary axis
	public static string FileNameEncoderRotaryAxisOnAxis = "encoder-rotary-axis-pulley-axis.png";
	public static string FileNameEncoderAxisInertial = "encoder-rotary-axis-inertial.png";
	public static string FileNameEncoderAxisInertialLateral = "encoder-rotary-axis-inertial-lateral.png";
	public static string FileNameEncoderAxisInertialMovPulley = "encoder-rotary-axis-inertial-mov-pulley.png";
	public static string FileNameEncoderAxisWithMovPulley = "encoder-rotary-axis-on-fixed-pulley-with-weighted-moving-pulley.png";
	
	public static string FileNameEncoderCalculeIM = "encoder-calcule-im.png";	

	public enum EncoderConfigurationNames { //this names are used on graph.R change there also if needed
		// ---- LINEAR ----
		LINEAR, LINEARINVERTED, LINEARINERTIAL, 
		WEIGHTEDMOVPULLEYLINEARONPERSON1, WEIGHTEDMOVPULLEYLINEARONPERSON1INV,
		WEIGHTEDMOVPULLEYLINEARONPERSON2, WEIGHTEDMOVPULLEYLINEARONPERSON2INV,
		WEIGHTEDMOVPULLEYONLINEARENCODER, 
		LINEARONPLANE, LINEARONPLANEWEIGHTDIFFANGLE, 
		// ---- ROTARY FRICTION ----
		ROTARYFRICTIONSIDE, ROTARYFRICTIONAXIS,
		WEIGHTEDMOVPULLEYROTARYFRICTION,
		ROTARYFRICTIONSIDEINERTIAL, ROTARYFRICTIONAXISINERTIAL,
		ROTARYFRICTIONSIDEINERTIALLATERAL, ROTARYFRICTIONAXISINERTIALLATERAL,
		ROTARYFRICTIONSIDEINERTIALMOVPULLEY, ROTARYFRICTIONAXISINERTIALMOVPULLEY,
		// ---- ROTARY AXIS ----
		ROTARYAXIS, WEIGHTEDMOVPULLEYROTARYAXIS,
		ROTARYAXISINERTIAL, ROTARYAXISINERTIALLATERAL, ROTARYAXISINERTIALMOVPULLEY
	}

	public static string DefaultEncoderConfigurationCode = "Linear - barbell";
	
	public enum EncoderType {
		LINEAR, ROTARYFRICTION, ROTARYAXIS
	}	

	public const string MeanSpeed = "Mean speed";
	public const string MaxSpeed = "Max speed";
	public const string MeanForce = "Mean force";
	public const string MaxForce = "Max force";
	public const string MeanPower = "Mean power";
	public const string PeakPower = "Peak power";
		
	public enum Encoder1RMMethod { NONWEIGHTED, WEIGHTED, WEIGHTED2, WEIGHTED3 }
	public enum ContextMenu { NONE, EDITDELETE, DELETE }
	
	public enum EncoderAutoSaveCurve { ALL, NONE, BEST }

	public enum DoubleContact {
		NONE, FIRST, AVERAGE, LAST
	}
	
	//DISPLACED means: total
	public enum MassType {
		BODY, EXTRA, DISPLACED
	}	
	
	public enum Status { ERROR, UNSTARTED, OK}	
	
	/*
	 * Attention: this will be separated by ';', then no ';' sign can be here
	 * No "\n" can be here also
	 * check that this list has same elements than below list
	 */
	public static string [] EncoderEnglishWords = {
		"jump",
		"body speed",
		"speed",
		"Speed",
		"Accel.",
		"Force",
		"Power",
		"Average Power",
		"Peak Power",
		"Distance",
		"Time to Peak Power",
		"time",
		"Range",
		"distance",
		"Weight",
		"Mass",
		"eccentric",
		"concentric",
		"land",
		"air",
		"jump height",
		"Repetition",
		"Not enough data.",
		"Encoder is not connected.", 
		"prediction",
		"Concentric mean speed on bench press 1RM =",
		"Estimated percentual load =",
		"Adapted from",
		"Mean speed in concentric propulsive phase",
		"Sorry, no repetitions matched your criteria.",
		"Need at least three jumps",
		"Laterality", "RL", "R", "L",
		"Inertia M."
	};
	/*
	 * written here in order to be translated
	 * Attention: this will be separated by ';', then no ';' sign can be here
	 * No "\n" can be here also
	 * No "'" can be here also. eg d'inèrcia on catalan
	 * if translators add one, it will be converted to ','
	 * if translators add a "\n", it will be converted to " "
	 * if translators add a "'", it will be converted to ' '
	 *
	 * check that this list has same elements than above list
	 */
	public static string [] EncoderTranslatedWords = {
		Catalog.GetString("jump"),
		Catalog.GetString("body speed"),
		Catalog.GetString("speed"),
		Catalog.GetString("Speed"),
		Catalog.GetString("Accel."),
		Catalog.GetString("Force"),
		Catalog.GetString("Power"),
		Catalog.GetString("Average Power"),
		Catalog.GetString("Peak Power"),
		Catalog.GetString("Distance"),
		Catalog.GetString("Time to Peak Power"),
		Catalog.GetString("time"),
		Catalog.GetString("Range"),
		Catalog.GetString("distance"),
		Catalog.GetString("Weight"),
		Catalog.GetString("Mass"),
		Catalog.GetString("eccentric"),
		Catalog.GetString("concentric"),
		Catalog.GetString("land"),
		Catalog.GetString("air"),
		Catalog.GetString("jump height"),
		Catalog.GetString("Repetition"),
		Catalog.GetString("Not enough data."),
		Catalog.GetString("Encoder is not connected."),
		Catalog.GetString("prediction"),
		Catalog.GetString("Concentric mean speed on bench press 1RM is"),
		Catalog.GetString("Estimated percentual load ="),
		Catalog.GetString("Adapted from"),
		Catalog.GetString("Mean speed in concentric propulsive phase"),
		Catalog.GetString("Sorry, no repetitions matched your criteria."),
		Catalog.GetString("Need at least three jumps"),
		Catalog.GetString("Laterality"), Catalog.GetString("RL"), Catalog.GetString("R"), Catalog.GetString("L"),
		Catalog.GetString("Inertia M.")
	};
}
