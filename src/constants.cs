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
 *  Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Mono.Unix;
//do not use gtk or gdk because this class is used by the server
//see UtilGtk for eg color definitions

public class Constants
{
	//the strings created by Catalog cannot be const
	
	//public static string ReadmeTranslators = Catalog.GetString("Translator, there's a glossary that will help you in Chronojump translation:\n http://git.gnome.org/browse/chronojump/plain/glossary/chronojump_glossary_for_translators.html");

	public enum AuthorsEnum { SOFTWARE, NETWORKS, CHRONOPIC, SCIENTIFIC, MATH, DOCUMENTERS }
	public static string [] Authors(AuthorsEnum e)
	{
		if(e == AuthorsEnum.SOFTWARE)
			return new String []{
				"Xavier de Blas Foix <info@chronojump.org>, 2004-2022\n\t" +
					Catalog.GetString("Main developer."),
					"\nAndoni Morales Alastruey <http://ylatuya.es>, 2009-2020\n\t" +
						Catalog.GetString("Installation support: Autotools, packaging, bundle."),
					"\nXavier Padullés <testing@chronojump.org>, 2014-2022\n\t" +
						Catalog.GetString("R scripts."),
					"\nCarles Pina i Estany <http://pinux.info>, 2016-2017\n\t" +
						Catalog.GetString("Backend developer.") + " " + Catalog.GetString("Import sessions.")
			};
		else if(e == AuthorsEnum.NETWORKS)
			return new String []{
				"Arnau-Lenin Gols <arnau.lenin@sobtec.cat>, 2020-2021" +
					"\n\t" + Catalog.GetString("Full Stack developer."),
				"\nMax Ros Morejón <mros33@gmail.com>, 2019-2020" +
					"\n\t" + Catalog.GetString("Django developer."),
				"\nMarcos Venteo <mventeo@gmail.com>, 2017-2018" +
					"\n\t" + Catalog.GetString("Django migration."),
				"\nXavier de Blas Foix <info@chronojump.org>, 2016-2022" +
					"\n\t" + Catalog.GetString("Client-Server integration.") + " " + Catalog.GetString("First Flask iteration."),
				"\nCarles Pina i Estany <http://pinux.info>, 2016-2017" +
					"\n\t" + Catalog.GetString("Web services support.")
			};
		else if(e == AuthorsEnum.SCIENTIFIC)
			return new String []{
				"Josep Ma Padullés <jmpadulles@gmail.com>, 2005-2022" +
					"\n\t" + Catalog.GetString("Sports sciences") + ", " + Catalog.GetString("Engineering"),
				"\nXavier Padullés <testing@chronojump.org>, 2012-2022",
					"\n\t" + Catalog.GetString("Engineering"),
				"\nJosep Granell <josegranell2005@hotmail.com>, 2018-2022",
					"\n\t" + Catalog.GetString("Electronics"),
				"\nCarlos J. Gil Bellosta <http://www.datanalytics.com>, 2010-2012, 2018" +
					"\n\t" + Catalog.GetString("Maths and statistics"),
				"\nAleix Ruiz de Villa <aleixrvr@gmail.com>, 2013-2016" +
					"\n\t" + Catalog.GetString("Maths and statistics"),
			};
		/*
		else if(e == AuthorsEnum.OPENCV)
			return new String []{
				"Sharad Shankar <http://www.logicbrick.com>\n",
					"Onkar Nath Mishra <http://www.logicbrick.com>\n"
			};
			*/
		else if(e == AuthorsEnum.DOCUMENTERS)
			return new String []{
				"Xavier Padullés <testing@chronojump.org>, 2013-2020",
				"Xavier de Blas Foix <xaviblas@gmail.com>, 2004-2013",
				"Helena Olsson <hjolsson@gmail.com>, 2011\n\t" +
					Catalog.GetString("Chronojump Manual English translation.")
			};
		else if(e == AuthorsEnum.CHRONOPIC)
			return new String []{
				"Teng Wei Hua <wadedang@gmail.com>, 2011-2015" +
					"\n\t" + Catalog.GetString("Translation of Firmware to C.") + ", " +
					Catalog.GetString("New firmware features.") + ", " +
					Catalog.GetString("Encoder hardware layer."),
				"\nJuan Gonzalez Gómez <http://www.iearobotics.com>, 2004-2010\n\t" + Catalog.GetString("Skypic, Chronopic, connection between hardware and software."), //2010 he made the visit to the lab
				"\nRicardo Gómez González <http://www.iearobotics.com>, 2007\n\t" + Catalog.GetString("Chronopic3 industrial prototype."),
				"\nXavier de Blas Foix <info@chronojump.org>, 2015-2017\n\t" +
					Catalog.GetString("Chronopic validation.") + " " +
					Catalog.GetString("Chronopic reaction time advanced implementation.") + " " +
					Catalog.GetString("Other functionalities."),
				"\nXavier Padullés <testing@chronojump.org>, 2018\n\t" + Catalog.GetString("Improved timers."),
				"\nFerran Suárez Rodríguez <ferransuarez2@gmail.com>, 2015-2016\n\t" + Catalog.GetString("Chronopic reaction time advanced implementation."),
					//"Juan Fernando Pardo <juanfer@juanfer.com.ar>\n\t" + "Chronopic2."
			};
		else
			return new String []{""};
	}
	
	public static string ChronojumpWebsite = "http://www.chronojump.org";
	
	//formulas
	public static string DjIndexFormula = "Dj Index: (tv-tc)/tc *100)";
	public static string QIndexFormula = "Q index: (tv/tc)";
	public static string DjPowerFormula = "Dj Power: mass*g*(fallHeight+1.22625*(tv^2)) / tc";
	public static string DjIndexFormulaOnly = "(tv-tc)*100/(tc*1.0)"; //*1.0 for having double division
	public static string QIndexFormulaOnly = "tv/(tc*1.0)"; //*1.0 for having double division
	public static string DjPowerFormulaOnly = PersonSessionTable + ".weight * 9.81 * (fall/100.0 + 1.22625 * (tv*tv)) / tc *1.0";

	public static string ChronojumpProfileStr()
	{
		return Catalog.GetString("Chronojump profile");
	}
	public const string FvIndexFormula = "F/V sj+(100%)/sj *100";
	public const string IeIndexFormula = "IE (cmj-sj)/sj *100";
	public const string IRnaIndexFormula = "IRna (djna-cmj)/cmj *100";
	public const string IRaIndexFormula = "IRa (dja-cmj)/cmj *100";
	
	public const string ArmsUseIndexFormula = "Arms Use Index (abk-cmj)/cmj *100";
	public const string ArmsUseIndexName = "Arms Use Index";

	public const string SubtractionBetweenTests = "Subtraction between tests";

	//tests types (dont' use character '-' will be used multimedia file names)
	public enum TestTypes { JUMP, JUMP_RJ, RUN, RUN_I, FORCESENSOR, RT, PULSE, MULTICHRONOPIC, ENCODER, RACEANALYZER }
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
	public const string TagSessionTable = "tagSession";
	public const string SessionTagSessionTable = "sessionTagSession";
	public const string PersonNotUploadTable = "personSessionNotUpload"; 
	public const string SportTable = "sport";
	public const string SpeciallityTable = "speciallity";
	public const string PreferencesTable = "preferences";
	public const string CountryTable = "country";
	public const string ConvertTempTable = "convertTemp"; //for conversions
	public const string ChronopicRegisterTable = "chronopicRegister";
	public const string NewsTable = "news";

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
	public const string EncoderConfigurationTable = "encoderConfiguration";
	public const string EncoderSignalCurveTable = "encoderSignalCurve";
	public const string EncoderExerciseTable = "encoderExercise";
	public const string Encoder1RMTable = "encoder1RM";
	public const string ExecuteAutoTable = "executeAuto";
	public const string TriggerTable = "trigger";
	public const string ForceSensorTable = "forceSensor";
	public const string ForceSensorExerciseTable = "forceSensorExercise";
	public const string ForceSensorElasticBandTable = "forceSensorElasticBand";
	public const string ForceRFDTable = "forceRFD";
	public const string RunEncoderTable = "runEncoder";
	public const string RunEncoderExerciseTable = "runEncoderExercise";

	public const string LastJumpSimpleTypeParamsTable = "lastJumpSimpleTypeParams";
	public const string LastJumpRjTypeParamsTable = "lastJumpRjTypeParams";

	// Dummy variables that exists for translating purposes
	// pragma warning is to avoid warnings of "defined and not used" for these variables.
#pragma warning disable 0414
	private static string dumbVariableForTranslatingAny = Catalog.GetString("Any");
	private static string dumbVariableForTranslatingSportUndefined = Catalog.GetString("--Undefined");
	private static string dumbVariableForTranslatingSportAny = Catalog.GetString("--Any");
	private static string dumbVariableForTranslatingSportNone = Catalog.GetString("-None");
	private static string dumbVariableForTranslatingSpeciallityUndefined = Catalog.GetString("Undefined");
	private static string dumbVariableForTranslatingLevelUndefined = Catalog.GetString("Undefined");
	private static string dumbVariableForTranslatingLevelSedentary = Catalog.GetString("Sedentary/Occasional practice");
	private static string dumbVariableForTranslatingExerciseTranslatedBenchPress = Catalog.GetString("Bench press");
	private static string dumbVariableForTranslatingExerciseTranslatedSquat = Catalog.GetString("Squat");

	// The next two variables got moved from src/execute/event.cs from EventExecute::runATouchPlatform() and
	// EventExecute::RunANoStrides() and I'm not sure that are used. Left it here for now to avoid any regressions.
	private static string dumbVariableForTranslatingWarningMessageTouchAtend = Catalog.GetString("Always remember to touch platform at ending. If you don't do it, Chronojump will crash at next execution.");
	private static string dumbVariableForTranslatingWarningMessageRunAnalysisNotValid = Catalog.GetString("This Run Analysis is not valid because there are no strides.");
#pragma warning restore 0414

	//tests types
	public const string JumpTypeTable = "jumpType";
	public const string JumpRjTypeTable = "jumpRjType";
	public const string RunTypeTable = "runType";
	public const string RunIntervalTypeTable = "runIntervalType";
	public const string PulseTypeTable = "pulseType";
	public const string ReactionTimeTypeTable = "reactionTimeType";
	public const string GraphLinkTable = "graphLinkTable";

	//json temp tables
	public const string UploadEncoderDataTempTable = "uploadEncoderDataTemp";
	public const string UploadSprintDataTempTable = "uploadSprintDataTemp";
	public const string UploadExhibitionTestTempTable = "uploadExhibitionTestTemp";

	public const string UndefinedDefault = "Undefined";
	public const string Any = "Any";

	public const string SexU = "U";
	public const string SexM = "M";
	public const string SexF = "F";
	public const string Males = "Males";
	public const string Females = "Females";
	public const int SexUID = -2;
	public const int SexMID = 1;
	public const int SexFID = 0;
	public const int AnyID = -1; //not only related to sex

	//person & personSession stuff
	public const string Name = "name";
	public const string Height = "height";
	public const string Weight = "weight";
	
	//simulated tests and SIMULATED session
	public const string SessionSimulatedName = "SIMULATED"; //Do NOT translate this 
	public static string SessionProtectedStr()
	{
		return Catalog.GetString("Sorry, this session cannot be modified."); //SIMULATED session
	}
	public static string SimulatedTestsNotAllowedStr()
	{
		return Catalog.GetString("Chronopic is disconnected.") + "\n\n" +
			string.Format(Catalog.GetString("If you want to simulate tests, use {0} session."), "SIMULATED");
	}

	//Compujump strings
	public static string RFIDDisconnectedMessage()
	{
		return Catalog.GetString("RFID has been disconnected!") + "\n\n" +
			Catalog.GetString("Please, connect it and restart Chronojump.");
	}
	public static string RFIDNotInServerMessage()
	{
		return Catalog.GetString("This RFID is not registered on server.");
	}
	public static string ServerDisconnectedMessage()
	{
		return Catalog.GetString("Cannot connect with server!");
	}

	//server
	public const string ServerPingTable = "SPing"; 
	public const string ServerEvaluatorTable = "SEvaluator"; 
	public const string IPUnknown = "Unknown"; 
	public static int ServerUndefinedID = -1;
	public static string ServerOnlineStr()
	{
		return Catalog.GetString("Server is connected.");
	}
	public static string ServerOfflineStr()
	{
		return Catalog.GetString("Sorry, server is currently offline. Try later.") + "\n" +
			Catalog.GetString("Or maybe you are not connected to the Internet or your firewall is restricting connections");
	}
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
	public static string [] Devices ()
	{
		return new string[] {
			UndefinedDefault + ":" + Catalog.GetString(UndefinedDefault),
					 DeviceContactSteel + ":" + Catalog.GetString(DeviceContactSteel),
					 DeviceContactCircuit + ":" + Catalog.GetString(DeviceContactCircuit),
					 DeviceInfrared + ":" + Catalog.GetString(DeviceInfrared),
					 "Other" + ":" + Catalog.GetString("Other"),
		};
	}
	
	public static string MultiChronopicName = "MultiChronopic";
	public static string RunAnalysisName = "RunAnalysis"; //Josep Ma Padullés test
	public static string TakeOffName = "TakeOff"; //translate (take off?)
	public static string TakeOffWeightName = "TakeOffWeight"; //translate (take off?)

	public static string SoftwareUpdated = "Your software is updated!";
	public static string SoftwareUpdatedStr()
	{
		return Catalog.GetString("Your software is updated!");
	}
	public static string SoftwareNeedUpdateStr()
	{
		return string.Format(Catalog.GetString("Update software at {0}"), "www.chronojump.org");
	}
	public static string SoftwareNewerThanPublisedStr()
	{
		return Catalog.GetString("Your software is more updated than last published version.") + "\n" + Catalog.GetString("Please, don't update!");
	}

	public static string GetSpreadsheetString(string CSVExportDecimalSeparator)
	{
		string sep = ";";
		if(CSVExportDecimalSeparator != "COMMA")
			sep = ",";

		return "\n\n" + Catalog.GetString("Importing from your spreadsheet (LibreOffice, R, MS Excel, …)") + "\n" +
			Catalog.GetString("Remember the separator character is:") + " <b>" + sep + "</b>" + "\n\n" +
			Catalog.GetString("This can be changed at preferences.");
	}

	public static string JumpsProfileNeededJumpsStr()
	{
		return Catalog.GetString("Please, perform the needed jumps marked in red above.");
	}

/*	OLD, check this
	public static string PotencyLewisCMJFormula = Catalog.GetString("Peak Power")+ " CMJ (Lewis) " +
		"(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")*9.81*" +
		"SQRT(2*9,81* " + Catalog.GetString("height") + "(m))";
*/
	public static string PotencyLewisFormulaShortStr()
	{
		return Catalog.GetString("Peak power") + " (Lewis, 1974) " + Catalog.GetString("(Watts)");
	}
	public static string PotencyLewisFormulaStr()
	{
		return PotencyLewisFormulaShortStr() + "\n" +
			"(SQRT(4,9)*9,8*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ") * SQRT(" + Catalog.GetString("height") + "(m)))";
	}
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
	
	public static string PotencyHarmanFormulaShortStr()
	{
		return Catalog.GetString("Peak power") + " (Harman, 1991)";
	}
	public static string PotencyHarmanFormulaStr()
	{
		return PotencyHarmanFormulaShortStr() + "\n" +
			"(61.9*" + Catalog.GetString("height") + "(cm))" +
			"+ (36*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -1822";
	}

	public static string PotencySayersSJFormulaShortStr()
	{
		return Catalog.GetString("Peak power") + " SJ (Sayers, 1999)";
	}
	public static string PotencySayersSJFormulaStr()
	{
		return PotencySayersSJFormulaShortStr() + "\n" +
			"(60.7*" + Catalog.GetString("height") + "(cm))" +
			"+ (45.3*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -2055";
	}

	public static string PotencySayersCMJFormulaShortStr()
	{
		return Catalog.GetString("Peak power") + " CMJ (Sayers, 1999)";
	}
	public static string PotencySayersCMJFormulaStr()
	{
		return PotencySayersCMJFormulaShortStr() + "\n" +
			"(51.9*" + Catalog.GetString("height") + "(cm))" +
			"+ (48.9*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -2007";
	}

	//http://www.ncbi.nlm.nih.gov/pubmed/14658372	
	public static string PotencyShettyFormulaShortStr()
	{
		return Catalog.GetString("Peak power") + " (Shetty, 2002)";
	}
	public static string PotencyShettyFormulaStr()
	{
		return PotencyShettyFormulaShortStr() + "\n" +
			"(1925.72*" + Catalog.GetString("height") + "(m))" +
			"+ (14.74*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -666.3";
	}

	public static string PotencyCanavanFormulaShortStr()
	{
		return Catalog.GetString("Peak power") + " (Canavan, 2004)";
	}
	public static string PotencyCanavanFormulaStr()
	{
		return PotencyCanavanFormulaShortStr() + "\n" +
			"(65.1*" + Catalog.GetString("height") + "(cm))" +
			"+ (25.8*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -1413.1";
	}

	/*
	   public static string PotencyBahamondeFormula = Catalog.GetString("Peak power") + " (Bahamonde, 2005) \n" +
	   "(78.5*" + Catalog.GetString("height") + "(cm))" +
	   "+ (60.6*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") +
	   ")) -(15.3*" + Catalog.GetString("height") + "(cm)) -1413.1";
	   */ //what is this height?

	public static string PotencyLaraMaleApplicantsSCFormulaShortStr()
	{
		return Catalog.GetString("Peak power") + " (Lara, 2006, m)";
	}
	public static string PotencyLaraMaleApplicantsSCFormulaStr()
	{
		return PotencyLaraMaleApplicantsSCFormulaShortStr() +
			" (" + Catalog.GetString("Male applicants to a Faculty of Sport Sciences") + ") \n" +
			"(62.5*" + Catalog.GetString("height") + "(cm))" +
			"+ (50.3*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -2184.7";
	}

	public static string PotencyLaraFemaleEliteVoleiFormulaShortStr()
	{
		return Catalog.GetString("Peak power") + " (Lara, 2006, fev)";
	}
	public static string PotencyLaraFemaleEliteVoleiFormulaStr()
	{
		return PotencyLaraFemaleEliteVoleiFormulaShortStr() +
			" (" + Catalog.GetString("Female elite volleyball") + ") \n" +
			"(83.1*" + Catalog.GetString("height") + "(cm))" +
			"+ (42*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -2488";
	}
	
	public static string PotencyLaraFemaleMediumVoleiFormulaShortStr()
	{
		return Catalog.GetString("Peak power") + " (Lara, 2006, fmv)";
	}
	public static string PotencyLaraFemaleMediumVoleiFormulaStr()
	{
		return PotencyLaraFemaleMediumVoleiFormulaShortStr() +
			" (" + Catalog.GetString("Female medium volleyball") + ") \n" +
			"(53.6*" + Catalog.GetString("height") + "(cm))" +
			"+ (67.5*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -2624.1";
	}

	public static string PotencyLaraFemaleSCStudentsFormulaShortStr()
	{
		return Catalog.GetString("Peak power") + " (Lara, 2006, fsc)";
	}
	public static string PotencyLaraFemaleSCStudentsFormulaStr()
	{
		return PotencyLaraFemaleSCStudentsFormulaShortStr() +
			" (" + Catalog.GetString("Peak power") + " (Lara, 2006, " +
			Catalog.GetString("Female sports sciences students") + ") \n" +
			"(56.7*" + Catalog.GetString("height") + "(cm))" +
			"+ (47.2*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -1772.6";
	}
	
	public static string PotencyLaraFemaleSedentaryFormulaShortStr()
	{
		return Catalog.GetString("Peak power") + " (Lara, 2006, fu)";
	}
	public static string PotencyLaraFemaleSedentaryFormulaStr()
	{
		return PotencyLaraFemaleSedentaryFormulaShortStr() +
			" (" + Catalog.GetString("Peak power") + " (Lara, 2006, " + Catalog.GetString("Female university students") + ") \n" +
			"(68.2*" + Catalog.GetString("height") + "(cm))" +
			"+ (40.8*(" + Catalog.GetString("body weight") + "+" + Catalog.GetString("extra weight") + ")) -1731.1";
	}
	
	public static string RJPotencyBoscoFormula = "Peak Power" + " (Bosco)" + " 9.81^2*TF*TT / (4*jumps*(TT-TF))";
	public static string RJPotencyBoscoFormulaOnly = "9.81*9.81 * tvavg*jumps * time / ( 4.0 * jumps * (time - tvavg*jumps) )"; //*4.0 for having double division
	public static string RJPotencyBoscoNameStr()
	{
		return Catalog.GetString("Peak Power") + " (Bosco)";
	}

	public static string RjIndexName = "Rj Index";
	public static string RjIndexFormulaOnly = "(tvavg-tcavg)*100/(tcavg * 1.0)";
	public static string QIndexName = "Q Index";
	public static string RjIndexOnlyFormula = "(tf-tc)/tc *100)";
	public static string QIndexOnlyFormula = "(tf/tc)";
	public static string RJAVGSDRjIndexName = "Reactive AVG SD" + " (" + RjIndexName + ")";
	public static string RJAVGSDQIndexName = "Reactive AVG SD" + " (" + QIndexName + ")";


	//global stat types
	public static string TypeSessionSummaryStr()
	{
		return Catalog.GetString("Session summary");
	}
	public static string TypeJumperSummaryStr()
	{
		return Catalog.GetString("Jumper summary");
	}
	public static string TypeJumpsSimpleStr()
	{
		return Catalog.GetString("Simple");
	}
	public static string TypeJumpsSimpleWithTCStr()
	{
		return Catalog.GetString("Simple with TC");
	}
	public static string TypeJumpsReactiveStr()
	{
		return Catalog.GetString("Jumps: Reactive");
	}
	public static string TypeRunsSimpleStr()
	{
		return Catalog.GetString("Races: Simple");
	}

	public static string TypeRunsIntervallicStr()
	{
		return Catalog.GetString("Races: Intervallic");
	}

	public static string jumpsTranslatedStr ()
	{
		return Catalog.GetString ("jumps");
	}
	public static string tracksTranslatedStr ()
	{
		return Catalog.GetString ("tracks");
	}

	//strings
	public static string AllJumpsNameStr()
	{
	       return Catalog.GetString("See all jumps");
	}
	public static string AllRunsNameStr()
	{
		return Catalog.GetString("See all races");
	}
	public static string AllPulsesNameStr()
	{
		return Catalog.GetString("See all pulses");
	}

	//fileNames
	//public static string FileNameLogo = "chronojump-boscosystem_white_bg.png";
	//public static string FileNameLogo320 = "chronojump-boscosystem_320.png";
	public static string FileNameLogoTransparent = "chronojump-logo-transparent.png";
	public static string FileNameLogo = "chronojump-logo-2013.png";
	public static string FileNameLogo320 = "chronojump-logo-2013_320.png";
	public static string FileNameLogoTransparent40h = "chronojump-logo-transparent-40h.png";
	public static string FileNameCSS = "report_web_style.css";
	public static string FileNameIcon = "chronojump_icon.png";
	public static string FileNameIconTransp = "chronojump_icon_transp.png";
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
	public static string GraphPaletteBlackStr()
	{
		//return Catalog.GetString("black only");
		//not show translated because on app1 creation, this is written in English
		//and then on check if is the selected then some params change
		//and we need to ensure that no space arrives to R
		return "black";
	}
	public static string [] GraphPalettes = { GraphPaletteBlackStr(), GraphPaletteGray, "rainbow",
		"topo.colors", "heat.colors", "terrain.colors", "cm.colors"};


	public static string FileNameZoomInIcon = "zoom_in.png";
	public static string FileNameZoomInWithTextIcon = "zoom_in_with_text.png";
	public static string FileNameNew1 = "gtk-new-1.png";
	public static string FileNameNewPlus = "gtk-new-plus.png";
	public static string FileNameOpen = "gtk-open.png";
	public static string FileNameOpen1 = "gtk-open-1.png";
	public static string FileNameOpenPlus = "gtk-open-plus.png";

	public static string FileNameImport = "import.png";
	public static string FileNameImportYellow = "import_yellow.png";
	public static string FileNameExport = "export.png";

	public static string FileNameCSVHeadersIcon = "import-csv-headers.png";
	public static string FileNameCSVNoHeadersIcon = "import-csv-noheaders.png";
	public static string FileNameCSVName1Icon = "import-csv-name-1-column.png";
	public static string FileNameCSVName2Icon = "import-csv-name-2-columns.png";

	public static string FileNameEncoderGravitatory = "stock_down.png";
	public static string FileNameEncoderInertial = "stock_inertial.png";
	public static string FileNamePulse = "pulse_menu.png";
	public static string FileNameForceSensorElastic = "force_sensor_elastic.png";
	public static string FileNameForceSensorNotElastic = "force_sensor_not_elastic.png";
	public static string FileNameMultiChronopic = "multichronopic_menu.png";
	
	public static string FileNameJumpsFallCalculate = "dj-from-in.png";
	public static string FileNameJumpsFallPredefined = "dj-from-out.png";
	
	public static string FileNameChronopic = "chronopic.png";
	public static string FileNameChronopic1 = "chronopic1.jpg";
	public static string FileNameChronopic2 = "chronopic2.jpg";
	public static string FileNameChronopic3 = "chronopic3.jpg";
	public static string FileNameContactPlatformSteel = "plataforma_contactos.jpg";
	public static string FileNameContactPlatformModular = "modular_platform_with_chronopic.jpg";
	public static string FileNameInfrared = "infrared.jpg";
	
	public static string FileNameEncoderAnalyzeIndividualCurrentSetIcon = "encoder-analyze-individual-current-set.png";
	public static string FileNameEncoderAnalyzeIndividualCurrentSessionIcon = "encoder-analyze-individual-current-session.png";
	public static string FileNameEncoderAnalyzeIndividualAllSessionsIcon = "encoder-analyze-individual-all-sessions.png";
	public static string FileNameEncoderAnalyzeGroupalCurrentSessionIcon = "encoder-analyze-groupal-current-session.png";

	public static string FileNameEncoderAnalyzePowerbarsIcon = "encoder-analyze-powerbars.png";
	public static string FileNameEncoderAnalyzeCrossIcon = "encoder-analyze-cross.png";
	public static string FileNameEncoderAnalyze1RMIcon = "encoder-analyze-1RM.png";
	public static string FileNameEncoderAnalyzeInstantaneousIcon = "encoder-analyze-instantaneous.png";
	public static string FileNameEncoderAnalyzeSingleIcon = "encoder-analyze-single.png";
	public static string FileNameEncoderAnalyzeSideIcon = "encoder-analyze-side.png";
	public static string FileNameEncoderAnalyzeSuperposeIcon = "encoder-analyze-superpose.png";
	public static string FileNameEncoderAnalyzeAllSetIcon = "encoder-analyze-all-set.png";
	public static string FileNameEncoderAnalyzeNmpIcon = "encoder-analyze-nmp.png";
	
	public static string FileNameEncoderAnalyzeEcconTogetherIcon = "encoder-analyze-eccon-together.png";
	public static string FileNameEncoderAnalyzeEcconSeparatedIcon = "encoder-analyze-eccon-separated.png";
	
	public static string FileNameEncoderAnalyzePositionIcon = "encoder-analyze-position.png";
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
	public static string FileNameSelectorEncoderGravitatory = "chronojump-encoder-small.png";
	public static string FileNameSelectorEncoderInertial = "chronojump-inertial.png";

	public static string FileNameLog = "log_chronojump.txt";
	public static string FileNameLogOld = "log_chronojump_old.txt";
	
	public static string FileNameConfig = "chronojump_config.txt";
	public static string FileNamePresentation = "presentation.txt";
	
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


	public static string PortNamesWindowsStr()
	{
		return string.Format(Catalog.GetString("Typical serial and USB-serial ports on Windows:") +
				"\n\t<i>COM1\tCOM2</i>\n\n" + 
				Catalog.GetString("Also, these are possible:") + 
				"\t<i>COM3 ... COM27</i>");
	}


	public static string PortNamesLinuxStr()
	{
		return string.Format(Catalog.GetString("Typical serial ports on GNU/Linux:") +
				"\t<i>/dev/ttyS0\t/dev/ttyS1</i>\n" +
				Catalog.GetString("Typical USB-serial ports on GNU/Linux:") +
				"\t<i>/dev/ttyUSB0\t/dev/ttyUSB1</i>" + "\n" + 
				Catalog.GetString("If you use Chronopic3, you will have an USB-serial port.")
				);
	}

	public static string FoundSerialPortsString()
	{
		return Catalog.GetString("Serial ports found:");
	}
	public static string FoundUSBSerialPortsString()
	{
		return Catalog.GetString("USB-serial ports found:");
	}
	public static string NotFoundUSBSerialPortsString()
	{
		return Catalog.GetString("Not found any USB-serial ports.") + " " + Catalog.GetString("Is Chronopic connected?");
	}

	public static string ChronopicDetectingStr()
	{
		return Catalog.GetString("Detecting …");
	}
	public static string ChronopicNeedTouchStr()
	{
		return Catalog.GetString("Touch device.");
	}

	public static string FindDriverNeedStr()
	{
		return Catalog.GetString("Chronopic driver has to be installed.");
	}

	public static string FindDriverWindowsStr()
	{
		return Catalog.GetString("If you have problems connecting with Chronopic, ensure you have the <b>driver</b> installed at 'Windows Start Menu / Chronojump / Install Chronopic driver'.");
	}
	public static string FindDriverOthersStr()
	{
		return Catalog.GetString("Check Chronojump software website.");
	}
	public static string VideoNothingCapturedStr()
	{
		return Catalog.GetString("Error. Nothing has been captured.");
	}

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
	public static int SportNoneID = 2;
	public static string SportNone = "-None";

	public static int SpeciallityUndefinedID = -1;
	public static string SpeciallityUndefined = "Undefined"; 

	public static int RaceUndefinedID = -1;

	public static int CountryUndefinedID = 1;
	public static string CountryUndefined = "Undefined"; 
	public static string ContinentUndefined = "Undefined"; 
	public static string [] ContinentsStr()
	{
		return new string [] {
			ContinentUndefined + ":" + Catalog.GetString(ContinentUndefined),
					   "Africa" + ":" + Catalog.GetString("Africa"),
					   "Antarctica" + ":" + Catalog.GetString("Antarctica"),
					   "Asia" + ":" + Catalog.GetString("Asia"),
					   "Europe" + ":" + Catalog.GetString("Europe"),
					   "North America" + ":" + Catalog.GetString("North America"),
					   "Oceania" + ":" + Catalog.GetString("Oceania"),
					   "South America" + ":" + Catalog.GetString("South America"),
		};
	}

	public static int Simulated = -1; 
	public static string SimulatedMessage()
	{
		return Catalog.GetString("Tests are <b>simulated</b> until Chronopic is connected.");
	}
	public static string SimulatedTreeviewStr()
	{
		return " (" + Catalog.GetString("Simulated") + ")";
	}

	public static string ChronopicOneStr()
	{
		return Catalog.GetString("All tests available except MultiChronopic.");
	}
	public static string ChronopicMoreStr()
	{
		return Catalog.GetString("All tests available.");
	}

	public static string DefaultString()
	{
		return Catalog.GetString("Default");
	}
	
	//levels of sport practice
	//int will go into person database
	//string will be shown in user language
	public static int LevelUndefinedID = -1;
	public static string LevelUndefined = "Undefined"; 
	public static int LevelSedentaryID = 0; 
	public static string LevelSedentary = "Sedentary/Occasional practice"; 
	public static string [] LevelsStr()
	{
		return new string [] {
			LevelUndefinedID.ToString() + ":" + Catalog.GetString(LevelUndefined),
				LevelSedentaryID.ToString() + ":" + Catalog.GetString(LevelSedentary),
				"1:" + Catalog.GetString("Regular practice"),
				"2:" + Catalog.GetString("Competition"),
				"3:" + Catalog.GetString("Elite"),
		};
	}

	public static double TrochanterToeUndefinedID = 0;
	public static double TrochanterFloorOnFlexionUndefinedID = 0;

	public static string [] SplashMessages = {
		"Initializing",			//0
		"Checking database",		//1
		"Creating database",		//2
		"Making database backup",	//3
		"Updating database",		//4
		"Connecting to server",		//5
		"Preparing main Window",	//6
		"Loading preferences",		//7
		"Creating widgets", 		//8
		"Creating encoder widgets", 	//9
		"Starting main window", 	//10
	};

	public static string NotEnoughDataStr ()
	{
		return Catalog.GetString ("Not enough data.");
	}
	public static string DatabaseNotFoundStr()
	{
		return Catalog.GetString("Error. Cannot find database.");
	}
	public static string WebsiteNotFoundStr()
	{
		return Catalog.GetString("Sorry, cannot open website."); //note this message is also in Glade
	}
	public static string DirectoryCannotOpenStr()
	{
		return Catalog.GetString("Error. Cannot open directory.");
	}
	public static string FileNotFoundStr()
	{
		return Catalog.GetString("Error. File not found.");
	}
	public static string FileEmptyStr()
	{
		return Catalog.GetString("Error. File is empty.");
	}
	public static string FileCopyProblemStr()
	{
		return Catalog.GetString("Error. Cannot copy file.");
	}
	public static string FileCannotSaveStr()
	{
		return Catalog.GetString("Error. File cannot be saved.");
	}
	public static string GraphCannot(string test)
	{
		return string.Format(Catalog.GetString("Sorry, this graph cannot be done with test {0}."), test);
	}
	public static string FileCannotOpenedMaybeSpreadsheetOpened  ()
	{
		return Catalog.GetString ("Maybe this file is opened by an SpreadSheet software like Excel. Please, close that program.");
	}

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
		WARNING, INFO, HELP, INSPECT, NONE
	}

	public static string NoStr()
	{
		return Catalog.GetString("No");
	}
	public static string YesStr()
	{
		return Catalog.GetString("Yes");
	}
	public static string InStr()
	{
		return Catalog.GetString("In");
	}
	public static string OutStr()
	{
		return Catalog.GetString("Out");
	}

	//it's important they are two chars long
	//public static string EqualThanCode = "= ";
	public static string LowerThanCode = "< ";
	//public static string HigherThanCode = "> ";
	//public static string LowerOrEqualThanCode = "<=";
	public static string HigherOrEqualThanCode = ">=";

	//heightmetric contains 2 spins
	public enum GenericWindowShow {
		ENTRY, ENTRY2, ENTRY3, SPININT, SPININT2, SPININT3, SPINDOUBLE, HBOXSPINDOUBLE2, 
		HEIGHTMETRIC, CHECK1, COMBO, COMBOALLNONESELECTED, BUTTONMIDDLE, GRIDPERSONSESSION,
		LABELBEFORETEXTVIEWTREEVIEW, TEXTVIEW, TREEVIEW
	}


	public static string HelpPowerStr()
	{
		return Catalog.GetString("On jumps results tab, power is calculated depending on jump type:") +
			"\n\n" +
			//Catalog.GetString("Jumps with TC & TF: Bosco Relative Power (W/Kg)") +
			//"\n" +
			//Catalog.GetString("P = 24.6 * (Total time + Flight time) / Contact time") +
			Catalog.GetString("Jumps with TC and TF:") + " " + Catalog.GetString("Developed by Chronojump team") +
			"\n" +
			Catalog.GetString("Calculate the potential energies on fall and after the jump.") + "\n" +
			Catalog.GetString("Divide them by time during force is applied.") +
			"\n\n" +
			//P = mass * g * ( fallHeight + 1.22625 * Math.Pow(tf,2) ) / (Double)tt;
			"P = " + Catalog.GetString("mass") + " * g * ( " +
			Catalog.GetString("falling height") + " + 1.22625 * " + Catalog.GetString("flight time") + " ^ 2 ) / " +
			Catalog.GetString("contact time") +
			"\n\n" +
			Catalog.GetString("Jumps without TC: Lewis Peak Power 1974 (W)") +
			"\n\n" +
			Catalog.GetString("P = SQRT(4.9) * 9.8 * (body weight+extra weight) * SQRT(jump height in meters)") +
			"\n\n" +
			Catalog.GetString("If you want to use other formulas, go to Analyze.");
	}

	public static string HelpStiffnessStr()
	{
		return "M: " + Catalog.GetString("Mass") + "\n" +
			"Tc: " + Catalog.GetString("Contact Time") + "\n" +
			"Tf: " + Catalog.GetString("Flight Time") + "\n\n" +
			Catalog.GetString("See:") + "\n" +
			"Dalleau, G; Belli, A; Viale, F; Lacour, JR; and Bourdin, M. (2004). " +
			"A simple method for field measurements of leg stiffness in hopping. " +
			"Int J Sports Med 25: 170–176";
	}

	public static string GetRepairWindowMessage ()
	{
		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;
		return string.Format(Catalog.GetString("Use this window to repair this test.\nDouble clic any cell to edit it (decimal separator: '{0}')"), localeInfo.NumberDecimalSeparator) +
			"\n" + Catalog.GetString ("Press enter or tab after editing a cell.");
	}

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
		PHOTO, PHOTOPNG, VIDEO
	}
	//public const string ExtensionVideo = ".avi";
	public const string ExtensionVideo = ".mp4";
	public const string ExtensionPhoto = ".jpg";
	public const string ExtensionPhotoPng = ".png"; //used for Cairo resized images
	public const string SmallPhotoDir = "small";
	public static string MultimediaFileNoExists = Catalog.GetString("Sorry, this multimedia file does not exists.");
	public static string PhotoTemp = "chronojump-last-photo";
	public static string PhotoSmallTemp = "chronojump-last-photo-small";
	public static string VideoTemp = "chronojump-last-video";
	
	public static string RunStartInitialSpeedYesStr()
	{
		return Catalog.GetString("Running start. Started with initial speed.");
	}
	public static string RunStartInitialSpeedNoStr()
	{
		return Catalog.GetString("Standing start. Started without initial speed.");
	}

	public static string CameraNotFoundStr()
	{
		return Catalog.GetString("Sorry, no cameras found.");
	}
	public static string FfmpegNotInstalledStr()
	{
		return "Software ffmpeg is not installed, please check instructions on chronojump website (software page)";
	}

	public enum BellModes {
		JUMPS, RUNS, ENCODERGRAVITATORY, ENCODERINERTIAL, FORCESENSOR, RUNSENCODER
	}

	public enum Modes {
		UNDEFINED,
		JUMPSSIMPLE, JUMPSREACTIVE,
		RUNSSIMPLE, RUNSINTERVALLIC, RUNSENCODER,
		POWERGRAVITATORY, POWERINERTIAL,
		FORCESENSORISOMETRIC, FORCESENSORELASTIC,
		RT, OTHER } //OTHER can be: Multichronopic, Pulse

	//to shorten code on the rest of the software
	public static bool ModeIsENCODER (Modes mode)
	{
		return (mode == Modes.POWERGRAVITATORY || mode == Modes.POWERINERTIAL);
	}
	public static bool ModeIsFORCESENSOR (Modes mode)
	{
		return (mode == Modes.FORCESENSORISOMETRIC || mode == Modes.FORCESENSORELASTIC);
	}

	public static string ModeFolder (Constants.Modes mode)
	{
		if(mode == Constants.Modes.RUNSENCODER)
			return "raceAnalyzer";
		else if (ModeIsENCODER (mode))
			return "encoder";
		else if(ModeIsFORCESENSOR (mode))
			return "forceSensor";
		else
			return ""; //should never happen
	}

	public static string ModePrint (Constants.Modes mode)
	{
		if(mode == Constants.Modes.JUMPSSIMPLE)
			return Catalog.GetString("Jumps simple");
		else if(mode == Constants.Modes.JUMPSREACTIVE)
			return Catalog.GetString("Jumps multiple");
		else if(mode == Constants.Modes.RUNSSIMPLE)
			return Catalog.GetString("Races simple");
		else if(mode == Constants.Modes.RUNSINTERVALLIC)
			return Catalog.GetString("Races intervallic");
		else if(mode == Constants.Modes.RUNSENCODER)
			return Catalog.GetString("Race analyzer");
		else if(mode == Constants.Modes.POWERGRAVITATORY)
			return Catalog.GetString("Encoder (gravitatory)");
		else if(mode == Constants.Modes.POWERINERTIAL)
			return Catalog.GetString("Encoder (inertial)");
		else if(mode == Constants.Modes.FORCESENSORISOMETRIC)
			return Catalog.GetString("Isometric");
		else if(mode == Constants.Modes.FORCESENSORELASTIC)
			return Catalog.GetString("Elastic");
		else if(mode == Constants.Modes.RT)
			return Catalog.GetString("Reaction time");
		else if(mode == Constants.Modes.OTHER)
			return Catalog.GetString("Other");
		else
			return ""; //should never happen
	}

	//used on detect device
	public static string ModeIcon (Constants.Modes mode)
	{
		if (mode == Constants.Modes.JUMPSSIMPLE)
			return "image_jump_simple.png";
		else if (mode == Constants.Modes.JUMPSREACTIVE)
			return "image_jump_reactive.png";
		else if (mode == Constants.Modes.RUNSSIMPLE)
			return "image_run_simple.png";
		else if (mode == Constants.Modes.RUNSINTERVALLIC)
			return "image_run_multiple.png";
		else if (mode == Constants.Modes.RUNSENCODER)
			return "race_encoder_icon.png";
		else if (mode == Constants.Modes.POWERGRAVITATORY)
			return "image_weight.png";
		else if (mode == Constants.Modes.POWERINERTIAL)
			return "image_inertia.png";
		else if (mode == Constants.Modes.FORCESENSORISOMETRIC)
			return "isometric.png";
		else if (mode == Constants.Modes.FORCESENSORELASTIC)
			return "elastic.png";
		else
			return "image_jump_simple"; //to not crash if there is any future mode
	}

	public static string All = "All";
	public static string None = "None";
	public static string Invert = "Invert";
	public static string Selected = "Selected";

	public static string FileNameArrowForward = "arrow_forward.png";
	public static string FileNameArrowBackward = "arrow_backward.png";
	public static string FileNameArrowForwardEmphasis = "arrow_forward_emphasis.png";

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
	public static string RScriptOSX = "/Library/Frameworks/R.framework/Resources/Rscript";

	public static string RmacDownload = "https://cran-archive.r-project.org/bin/macosx/el-capitan/base/R-3.6.3.nn.pkg";
	
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

	public enum EncoderGI {ALL, GRAVITATORY, INERTIAL}
	public enum CheckFileOp
	{
		JUMPS_SIMPLE_CAPTURE_SAVE_IMAGE, JUMPS_REACTIVE_CAPTURE_SAVE_IMAGE,
		JUMPS_PROFILE_SAVE_IMAGE, JUMPS_DJ_OPTIMAL_FALL_SAVE_IMAGE,
		JUMPS_ASYMMETRY_BILATERAL_SAVE_IMAGE, JUMPS_ASYMMETRY_ASYMMETRY_SAVE_IMAGE,
		JUMPS_WEIGHT_FV_PROFILE_SAVE_IMAGE, JUMPS_EVOLUTION_SAVE_IMAGE,
		JUMPS_RJ_FATIGUE_SAVE_IMAGE,
		RUNS_SIMPLE_CAPTURE_SAVE_IMAGE, RUNS_INTERVALLIC_CAPTURE_SAVE_IMAGE,
		RUNS_EVOLUTION_SAVE_IMAGE,
		RUNS_SPRINT_SAVE_IMAGE, RUNS_SPRINT_SAVE_TABLE,
		RUNS_SPRINT_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES,
		RUNS_SPRINT_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES,
		RUNS_SPRINT_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES,
		RUNS_SPRINT_EXPORT_INDIVIDUAL_CURRENT_SESSION_YES_IMAGES,
		RUNS_SPRINT_EXPORT_INDIVIDUAL_ALL_SESSIONS_YES_IMAGES,
		RUNS_SPRINT_EXPORT_GROUPAL_CURRENT_SESSION_YES_IMAGES,
		ENCODER_CAPTURE_EXPORT_ALL, ENCODER_CAPTURE_SAVE_IMAGE,
		ENCODER_ANALYZE_SAVE_IMAGE, ENCODER_ANALYZE_SAVE_IMAGE_CURRENT_SESSION,
		ENCODER_ANALYZE_SEND_IMAGE, //like save image but just defines the name exportFileName to be sended
		ENCODER_ANALYZE_SAVE_AB, ENCODER_ANALYZE_SAVE_TABLE, ENCODER_ANALYZE_SAVE_TABLE_CURRENT_SESSION,
		FORCESENSOR_SAVE_IMAGE_SIGNAL, FORCESENSOR_SAVE_IMAGE_MODEL,
		FORCESENSOR_SAVE_IMAGE_RFD_MANUAL, FORCESENSOR_ANALYZE_SAVE_AB, FORCESENSOR_ANALYZE_SAVE_CD,
		FORCESENSOR_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES,
		FORCESENSOR_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES,
		FORCESENSOR_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES,
		FORCESENSOR_EXPORT_INDIVIDUAL_CURRENT_SESSION_YES_IMAGES,
		FORCESENSOR_EXPORT_INDIVIDUAL_ALL_SESSIONS_YES_IMAGES,
		FORCESENSOR_EXPORT_GROUPAL_CURRENT_SESSION_YES_IMAGES,
		RUNENCODER_CAPTURE_SAVE_IMAGE,
		RUNENCODER_ANALYZE_SAVE_IMAGE_MODEL, RUNENCODER_ANALYZE_SAVE_TABLE,
		RUNENCODER_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES,
		RUNENCODER_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES,
		RUNENCODER_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES,
		RUNENCODER_EXPORT_INDIVIDUAL_CURRENT_SESSION_YES_IMAGES,
		RUNENCODER_EXPORT_INDIVIDUAL_ALL_SESSIONS_YES_IMAGES,
		RUNENCODER_EXPORT_GROUPAL_CURRENT_SESSION_YES_IMAGES,
	}

	public static double EncoderErrorCode = -1;
	
	public static string FileNameEncoderImagePending = "encoder-image-pending.png";
	
	//three encoder types
	public static string FileNameEncoderTypeLinear = "encoder-linear.png";
	public static string FileNameEncoderTypeRotaryFriction = "encoder-rotary-friction.png";
	public static string FileNameEncoderTypeRotaryAxis = "encoder-rotary-axis.png";
	
	//encoder configurations
	//linear
	public static string FileNameEncoderLinearFreeWeight = "encoder-l-free-weight.png";
	public static string FileNameEncoderLinearFreeWeightInv = "encoder-l-free-weight-inv.png";
	public static string FileNameEncoderLinearInertial = "encoder-l-inertial.png";
	public static string FileNameEncoderWeightedMovPulleyOnPerson1 = "encoder-l-on-person-weighted-moving-pulley1.png";
	public static string FileNameEncoderWeightedMovPulleyOnPerson1Inv = "encoder-l-inv-on-person-weighted-moving-pulley1.png";
	public static string FileNameEncoderWeightedMovPulleyOnPerson2 = "encoder-l-on-person-weighted-moving-pulley2.png";
	public static string FileNameEncoderWeightedMovPulleyOnPerson2Inv = "encoder-l-inv-on-person-weighted-moving-pulley2.png";
	public static string FileNameEncoderWeightedMovPulleyOnLinearEncoder = "encoder-l-on-weighted-moving-pulley.png";
	public static string FileNameEncoderLinearOnPlane = "encoder-l-inclined-plane.png";
	public static string FileNameEncoderLinearOnPlaneWeightDiffAngle = "encoder-l-inclined-plane-weight-diff-angle.png";
	public static string FileNameEncoderLinearOnPlaneWeightDiffAngleMovPulley = "encoder-l-inclined-plane-weight-diff-angle-mov-pulley.png";
	public static string FileNameEncoderLinearPneumatic = "encoder-l-pneumatic.png";

	//rotary friction
	public static string FileNameEncoderFrictionSide = "encoder-rf-pulley.png";
	public static string FileNameEncoderFrictionAxis = "encoder-rf-pulley-axis.png";
	public static string FileNameEncoderFrictionSideInertial = "encoder-rf-side-inertial.png";
	public static string FileNameEncoderFrictionAxisInertial = "encoder-rf-axis-inertial.png";
	public static string FileNameEncoderFrictionSideInertialLateral = "encoder-rf-side-inertial-lateral.png";
	public static string FileNameEncoderFrictionAxisInertialLateral = "encoder-rf-axis-inertial-lateral.png";
	public static string FileNameEncoderFrictionSideInertialMovPulley = "encoder-rf-side-inertial-mov-pulley.png";
	public static string FileNameEncoderFrictionAxisInertialMovPulley = "encoder-rf-axis-inertial-mov-pulley.png";
	public static string FileNameEncoderFrictionWithMovPulley = "encoder-rf-on-fixed-pulley-with-weighted-moving-pulley.png";

	//rotary axis
	public static string FileNameEncoderRotaryAxisOnAxis = "encoder-ra-pulley-axis.png";
	public static string FileNameEncoderAxisInertial = "encoder-ra-inertial.png";
	public static string FileNameEncoderAxisInertialLateral = "encoder-ra-inertial-lateral.png";
	public static string FileNameEncoderAxisInertialMovPulley = "encoder-ra-inertial-mov-pulley.png";
	public static string FileNameEncoderAxisInertialMovPulleyLateral = "encoder-ra-inertial-mov-pulley-lateral.png";
	public static string FileNameEncoderAxisWithMovPulley = "encoder-ra-on-fixed-pulley-with-weighted-moving-pulley.png";
	
	public static string FileNameEncoderCalculeIM = "encoder-calcule-im.png";	

	public enum EncoderConfigurationNames { //this names are used on util.R and graph.R change there also if needed
		// ---- LINEAR ----
		LINEAR, LINEARINVERTED, LINEARINERTIAL, 
		WEIGHTEDMOVPULLEYLINEARONPERSON1, WEIGHTEDMOVPULLEYLINEARONPERSON1INV,
		WEIGHTEDMOVPULLEYLINEARONPERSON2, WEIGHTEDMOVPULLEYLINEARONPERSON2INV,
		WEIGHTEDMOVPULLEYONLINEARENCODER, 
		LINEARONPLANE, LINEARONPLANEWEIGHTDIFFANGLE, LINEARONPLANEWEIGHTDIFFANGLEMOVPULLEY,
		PNEUMATIC,
		// ---- ROTARY FRICTION ----
		ROTARYFRICTIONSIDE, ROTARYFRICTIONAXIS,
		WEIGHTEDMOVPULLEYROTARYFRICTION,
		ROTARYFRICTIONSIDEINERTIAL, ROTARYFRICTIONAXISINERTIAL,
		ROTARYFRICTIONSIDEINERTIALLATERAL, ROTARYFRICTIONAXISINERTIALLATERAL,
		ROTARYFRICTIONSIDEINERTIALMOVPULLEY, ROTARYFRICTIONAXISINERTIALMOVPULLEY,
		// ---- ROTARY AXIS ----
		ROTARYAXIS, WEIGHTEDMOVPULLEYROTARYAXIS,
		ROTARYAXISINERTIAL, ROTARYAXISINERTIALLATERAL, ROTARYAXISINERTIALMOVPULLEY, ROTARYAXISINERTIALLATERALMOVPULLEY
	}

	public static string DefaultEncoderConfigurationCode = "Linear - barbell";
	
	public enum EncoderType {
		LINEAR, ROTARYFRICTION, ROTARYAXIS
	}	

	public const string Start = "Start";
	public const string Duration = "Duration";
	public const string Range = "Range";
	public const string RangeAbsolute = "RangeAbsolute";
	public const string MeanSpeed = "Mean speed";
	public const string MaxSpeed = "Max speed";
	public const string MeanForce = "Mean force";
	public const string MaxForce = "Max force";
	public const string MeanPower = "Mean power";
	public const string PeakPower = "Peak power";
	public const string WorkJ = "WorkJ";
	public const string Impulse = "Impulse";
	public static string [] EncoderVariablesCaptureList = {
		RangeAbsolute, MeanSpeed, MaxSpeed, MeanForce, MaxForce, MeanPower, PeakPower, WorkJ, Impulse
	};
	public enum EncoderVariablesCapture {
		RangeAbsolute, MeanSpeed, MaxSpeed, MeanForce, MaxForce, MeanPower, PeakPower, WorkJ, Impulse
	}
	public static string GetEncoderVariablesCapture(EncoderVariablesCapture enumVariable) {
		switch(enumVariable) {
			case EncoderVariablesCapture.RangeAbsolute:
				return RangeAbsolute;
			case EncoderVariablesCapture.MeanSpeed:
				return MeanSpeed;
			case EncoderVariablesCapture.MaxSpeed:
				return MaxSpeed;
			case EncoderVariablesCapture.MeanForce:
				return MeanForce;
			case EncoderVariablesCapture.MaxForce:
				return MaxForce;
			case EncoderVariablesCapture.MeanPower:
				return MeanPower;
			case EncoderVariablesCapture.PeakPower:
				return PeakPower;
			case EncoderVariablesCapture.WorkJ:
				return WorkJ;
			case EncoderVariablesCapture.Impulse:
				return Impulse;
		}
		return MeanPower;
	}
	public static EncoderVariablesCapture SetEncoderVariablesCapture(string v) {
		switch(v) {
			case RangeAbsolute:
				return EncoderVariablesCapture.RangeAbsolute;
			case MeanSpeed:
				return EncoderVariablesCapture.MeanSpeed;
			case MaxSpeed:
				return EncoderVariablesCapture.MaxSpeed;
			case MeanForce:
				return EncoderVariablesCapture.MeanForce;
			case MaxForce:
				return EncoderVariablesCapture.MaxForce;
			case MeanPower:
				return EncoderVariablesCapture.MeanPower;
			case PeakPower:
				return EncoderVariablesCapture.PeakPower;
			case WorkJ:
				return EncoderVariablesCapture.WorkJ;
			case Impulse:
				return EncoderVariablesCapture.Impulse;
		}
		return EncoderVariablesCapture.MeanPower;
	}
		
	public enum Encoder1RMMethod { NONWEIGHTED, WEIGHTED, WEIGHTED2, WEIGHTED3 }
	//public enum ContextMenu { NONE, EDITDELETE, DELETE }

	//on glade/app1 using the same names
	public const string ForceSensorLateralityBoth = "Both";
	public const string ForceSensorLateralityLeft = "Left";
	public const string ForceSensorLateralityRight = "Right";

	public enum EncoderAutoSaveCurve { BEST, BESTN, BESTNCONSECUTIVE, ALL, FROM4TOPENULTIMATE, NONE }
	public static string [] EncoderAutoSaveCurvesStrings = { "Best", "Best n", "Best n consecutive", "All", "All but last", "None" };
	public static string GetEncoderAutoSaveCurvesStrings (EncoderAutoSaveCurve easc)
	{
		if(easc == EncoderAutoSaveCurve.BEST)
			return EncoderAutoSaveCurvesStrings[0];
		else if(easc == EncoderAutoSaveCurve.BESTN)
			return EncoderAutoSaveCurvesStrings[1];
		else if(easc == EncoderAutoSaveCurve.BESTNCONSECUTIVE)
			return EncoderAutoSaveCurvesStrings[2];
		else if(easc == EncoderAutoSaveCurve.ALL)
			return EncoderAutoSaveCurvesStrings[3];
		else if(easc == EncoderAutoSaveCurve.FROM4TOPENULTIMATE)
			return EncoderAutoSaveCurvesStrings[4];
		else //(easc == EncoderAutoSaveCurve.NONE
			return EncoderAutoSaveCurvesStrings[5];
	}
	public static EncoderAutoSaveCurve GetEncoderAutoSaveCurvesEnum (string str)
	{
		if(str == "Best")
			return EncoderAutoSaveCurve.BEST;
		else if(str == "Best n")
			return EncoderAutoSaveCurve.BESTN;
		else if(str == "Best n consecutive")
			return EncoderAutoSaveCurve.BESTNCONSECUTIVE;
		else if(str == "All")
			return EncoderAutoSaveCurve.ALL;
		else if(str == "All but last")
			return EncoderAutoSaveCurve.FROM4TOPENULTIMATE;
		else //(str == "None")
			return EncoderAutoSaveCurve.NONE;
	}

	//BESTN means: "bests" (cannot say this in English)
	//note last mode not need to be 4 because DB 1.63 introduces the config of this value

	//BIGGEST_TC will be the default mode.
	// - at END of each track: track ends before the biggest TC (just before the trunk arrives)
	// - at START of each track:
	// 	- if starting on contact:
	// 		time starts when leaving it. Usually will be with the biggest part (trunk),
	// 		and maybe later there's a foot, so leaving the contact on the first time is the beginning.
	// 	- on speed start, if there are double contacts,
	// 		- if "race starts at arriving at platform", then race will start before big tc (trunk)
	// 		- else, race will start after big tc (trunk)
	public enum DoubleContact {
		NONE, FIRST, AVERAGE, LAST, BIGGEST_TC
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
		"Concentric mean speed on squat 1RM =",
		"Estimated percentual load =",
		"Adapted from",
		"Mean speed in concentric propulsive phase",
		"Sorry, no repetitions matched your criteria.",
		"Need at least three jumps", 	//used on set analysis (has the whole set and finds ecc and cons)
		"Need at least three eccentric-concentric jumps", //used on session analysis, needs saved reps as ecc-con
		"Laterality", "RL", "R", "L",
		"Inertia M.",
		"Maximum mean power using the F-V profile",
		"Mean power parabole using the Power-Load data",
		"Propulsive",
		"Non propulsive",
		"Impulse"
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
		Catalog.GetString("Concentric mean speed on bench press 1RM ="),
		Catalog.GetString("Concentric mean speed on squat 1RM ="),
		Catalog.GetString("Estimated percentual load ="),
		Catalog.GetString("Adapted from"),
		Catalog.GetString("Mean speed in concentric propulsive phase"),
		Catalog.GetString("Sorry, no repetitions matched your criteria."),
		Catalog.GetString("Need at least three jumps"),
		Catalog.GetString("Need at least three eccentric-concentric jumps"),
		Catalog.GetString("Laterality"), Catalog.GetString("RL"), Catalog.GetString("R"), Catalog.GetString("L"),
		Catalog.GetString("Inertia M."),
		Catalog.GetString("Maximum mean power using the F-V profile"),
		Catalog.GetString("Mean power parabole using the Power-Load data"),
		Catalog.GetString("Propulsive"),
		Catalog.GetString("Non propulsive"),
		Catalog.GetString("Impulse")
	};
}
