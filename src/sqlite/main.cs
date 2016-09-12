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
 * Copyright (C) 2004-2016   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.IO; //"File" things. TextWriter
using System.Collections; //ArrayList
using Mono.Data.Sqlite;
using System.Diagnostics; 	//for launching other process

using Mono.Unix;

public sealed class SqliteGeneral
{
	private static Sqlite m_sqlite;
	private static SqlitePreferences m_sqlitePreferences;
	private static SqliteJumpRj m_sqliteJumpRj;
	private static SqliteJump m_sqliteJump;
	private static SqliteRunInterval m_sqliteRunInterval;
	private static SqlitePerson m_sqlitePerson;
	private static SqliteExecuteAuto m_sqliteExecuteAuto;
	private static SqlitePersonSession m_sqlitePersonSession;
	private static SqliteRun m_sqliteRun;
	private static SqliteRunIntervalType m_sqliteRunIntervalType;
	private static SqliteRunType m_sqliteRunType;
	private static SqliteReactionTime m_sqliteReactionTime;
	private static SqlitePulse m_sqlitePulse;
	private static SqlitePulseType m_sqlitePulseType;
	private static SqliteMultiChronopic m_sqliteMultiChronopic;
	private static SqliteSport m_sqliteSport;
	private static SqliteSpeciallity m_sqliteSpeciallity;
	private static SqliteJumpType m_sqliteJumpType;
	private static SqliteSession m_sqliteSession;
	private static SqliteServer m_sqliteServer;
	private static SqliteServerSession m_sqliteServerSession;
	private static SqlitePersonSessionNotUpload m_sqlitePersonSessionNotUpload;
	private static SqliteEncoder m_sqliteEncoder;
	private static SqliteEvent m_sqliteEvent;
	private static SqliteCountry m_sqliteCountry;
	private static SqliteStat m_sqliteStat;
	private static SqlitePersonSessionOld m_sqlitePersonSessionOld;
	private static SqlitePersonOld m_sqlitePersonOld;
	private static SqliteSessionOld m_sqliteSessionOld;
	private static SqliteOldConvert m_sqliteOldConvert;

	public SqliteGeneral()
	{
		m_sqlite = new Sqlite();
		m_sqlite.Connect();
		m_sqlitePreferences = new SqlitePreferences();
		m_sqlitePreferences.Connect();
		m_sqliteJumpRj = new SqliteJumpRj();
		m_sqliteJump = new SqliteJump();
		m_sqliteRunInterval = new SqliteRunInterval();
		m_sqlitePerson = new SqlitePerson();
		m_sqliteExecuteAuto = new SqliteExecuteAuto();
		m_sqlitePersonSession = new SqlitePersonSession();
		m_sqliteRun = new SqliteRun();
		m_sqliteRunIntervalType = new SqliteRunIntervalType();
		m_sqliteRunType = new SqliteRunType();
		m_sqliteReactionTime = new SqliteReactionTime();
		m_sqlitePulse = new SqlitePulse();
		m_sqlitePulseType = new SqlitePulseType();
		m_sqliteMultiChronopic = new SqliteMultiChronopic();
		m_sqliteSport = new SqliteSport();
		m_sqliteSpeciallity = new SqliteSpeciallity();
		m_sqliteJumpType = new SqliteJumpType();
		m_sqliteSession = new SqliteSession();
		m_sqliteServer = new SqliteServer();
		m_sqliteServerSession = new SqliteServerSession();
		m_sqlitePersonSessionNotUpload = new SqlitePersonSessionNotUpload();
		m_sqliteEncoder = new SqliteEncoder();
		m_sqliteEvent = new SqliteEvent();
		m_sqliteCountry = new SqliteCountry();
		m_sqliteStat = new SqliteStat();
		m_sqlitePersonSessionOld = new SqlitePersonSessionOld();
		m_sqlitePersonOld = new SqlitePersonOld();
		m_sqliteSessionOld = new SqliteSessionOld();
		m_sqliteOldConvert = new SqliteOldConvert();
	}

	public static Sqlite Sqlite
	{
		get
		{
			return m_sqlite;
		}
	}

	public static SqlitePreferences SqlitePreferences
	{
		get
		{
			return m_sqlitePreferences;
		}
	}

	public static SqliteJumpRj SqliteJumpRj
	{
		get
		{
			return m_sqliteJumpRj;
		}
	}

	public static SqliteJump SqliteJump
	{
		get
		{
			return m_sqliteJump;
		}
	}

	public static SqliteRunInterval SqliteRunInterval
	{
		get
		{
			return m_sqliteRunInterval;
		}
	}

	public static SqlitePerson SqlitePerson
	{
		get
		{
			return m_sqlitePerson;
		}
	}

	public static SqliteExecuteAuto SqliteExecuteAuto
	{
		get
		{
			return m_sqliteExecuteAuto;
		}
	}

	public static SqlitePersonSession SqlitePersonSession
	{
		get
		{
			return m_sqlitePersonSession;
		}
	}

	public static SqliteRun SqliteRun
	{
		get
		{
			return m_sqliteRun;
		}
	}

	public static SqliteRunIntervalType SqliteRunIntervalType
	{
		get
		{
			return m_sqliteRunIntervalType;
		}
	}

	public static SqliteRunType SqliteRunType
	{
		get
		{
			return m_sqliteRunType;
		}
	}
	public static SqliteReactionTime SqliteReactionTime
	{

		get
		{
			return m_sqliteReactionTime;
		}
	}

	public static SqlitePulse SqlitePulse
	{
		get
		{
			return m_sqlitePulse;
		}
	}

	public static SqlitePulseType SqlitePulseType
	{
		get
		{
			return m_sqlitePulseType;
		}
	}

	public static SqliteMultiChronopic SqliteMultiChronopic
	{
		get
		{
			return m_sqliteMultiChronopic;
		}
	}

	public static SqliteSport SqliteSport
	{
		get
		{
			return m_sqliteSport;
		}
	}

	public static SqliteSpeciallity SqliteSpeciallity
	{
		get
		{
			return m_sqliteSpeciallity;
		}
	}

	public static SqliteJumpType SqliteJumpType
	{
		get
		{
			return m_sqliteJumpType;
		}
	}

	public static SqliteSession SqliteSession
	{
		get
		{
			return m_sqliteSession;
		}
	}

	public static SqliteServer SqliteServer
	{
		get
		{
			return m_sqliteServer;
		}
	}
	public static SqliteServerSession SqliteServerSession
	{
		get
		{
			return m_sqliteServerSession;
		}
	}
	public static SqlitePersonSessionNotUpload SqlitePersonSessionNotUpload
	{
		get
		{
			return m_sqlitePersonSessionNotUpload;
		}
	}
	public static SqliteEncoder SqliteEncoder
	{
		get
		{
			return m_sqliteEncoder;
		}
	}
	public static SqliteEvent SqliteEvent
	{
		get
		{
			return m_sqliteEvent;
		}
	}
	public static SqliteCountry SqliteCountry
	{
		get
		{
			return m_sqliteCountry;
		}
	}
	public static SqliteStat SqliteStat
	{
		get
		{
			return m_sqliteStat;
		}
	}
	public static SqlitePersonSessionOld SqlitePersonSessionOld
	{
		get
		{
			return m_sqlitePersonSessionOld;
		}
	}
	public static SqlitePersonOld SqlitePersonOld
	{
		get
		{
			return m_sqlitePersonOld;
		}
	}
	public static SqliteSessionOld SqliteSessionOld
	{
		get
		{
			return m_sqliteSessionOld;
		}
	}

	public static SqliteOldConvert SqliteOldConvert
	{
		get
		{
			return m_sqliteOldConvert;
		}
	}
}

public class Sqlite
{
	protected SqliteConnection dbcon;
	protected SqliteCommand dbcmd;

	//since we use installJammer (chronojump 0.7)	
	//database was on c:\.chronojump\ or in ~/.chronojump
	//now it's on installed dir, eg linux: ~/Chronojump/database
	private string home = Util.GetDatabaseDir();
	private string sqlFile = Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db";
	
	private string temp = Util.GetDatabaseTempDir();
	private string sqlFileTemp = Util.GetDatabaseTempDir() + Path.DirectorySeparatorChar + "chronojump.db";

	//http://www.mono-project.com/SQLite

	private string connectionString;
	private string connectionStringTemp;

	//test to try to open db in a dir with accents (latin)
	//string connectionString = "globalization requestEncoding=\"iso-8859-1\"; responseEncoding=\"iso-8859-1\"; fileEncoding=\"iso-8859-1\"; culture=\"es-ES\";version = 3; Data source = " + sqlFile;
	
	//create blank database
	bool creatingBlankDatabase = false;

	
	public enum Orders_by { DEFAULT, ID_DESC }

	//for db creation
	int creationRate;
	int creationTotal;

	//for db conversion
	string currentVersion = "0";
	
	int conversionRate;
	int conversionRateTotal;
	protected int conversionSubRate;
	protected int conversionSubRateTotal;

	public bool IsOpened = false;
	public bool SafeClose = true;

	/*
	 * Important, change this if there's any update to database
	 */
	string lastChronojumpDatabaseVersion = "1.32";

	public Sqlite() {
		connectionString = "version = 3; Data source = " + sqlFile;
		connectionStringTemp = "version = 3; Data source = " + sqlFileTemp;

		sqlFileServer = home + Path.DirectorySeparatorChar + "chronojump_server.db";
		connectionStringServer = "version = 3; Data source = " + sqlFileServer;
	}

	protected virtual void createTable(string tableName) {
	}
	
	//used by personSessionWeight
	protected virtual void createTable() {
	}
	
	~Sqlite() {}

	//these two methods are used by methods who want to leave the connection opened
	//because lots of similar transactions have to be done
	public void Open()
	{
		try {
			LogB.SQLon();
			dbcon.Open();
		} catch {
			LogB.SQL("-- catched --");

			Close();

			LogB.Warning(" going to open ");
			LogB.SQLon();
			dbcon.Open();
			
			LogB.SQL("-- end of catched --");
		}
		
		IsOpened = true;
	}
	public void Close()
	{
		LogB.SQLoff();
			
		if(SafeClose) {
			dbcmd.Dispose(); //this seems critical in multiple open/close SQL
		}

		dbcon.Close();
		
		if(SafeClose) {
			//GC.Collect(); don't need and very slow
			dbcmd = dbcon.CreateCommand();
		}
		
		IsOpened = false;
	}

	public bool Connect()
	{
		/*
	       splashMessage = "pre";
		needUpdateSplashMessage = true;
		Console.ReadLine();		
		*/

		LogB.SQL("home is: " + home);

		bool defaultDBLocation = true;

		dbcon = new SqliteConnection();

		/*
		 * the Open() helps to know it threre are problems with path and sqlite
		 * passing utf-8 or looking for invalid chars is not enough
		 * but, as Open creates a file (if it doesn't exist)
		 * we prefer to create a test file (test.db) instead of chronojump.db
		 */
		string sqlFileTest = home + Path.DirectorySeparatorChar + "test.db";
		string sqlFileTestTemp = temp + Path.DirectorySeparatorChar + "test.db";
		string connectionStringTest = "version = 3; Data source = " + sqlFileTest;
		string connectionStringTestTemp = "version = 3; Data source = " + sqlFileTestTemp;

		dbcon.ConnectionString = connectionStringTest;
		dbcmd = dbcon.CreateCommand();

		try {
			dbcon.Open();
		} catch {
			dbcon.Close();
			dbcon.ConnectionString = connectionStringTestTemp;
			dbcmd = dbcon.CreateCommand();
			dbcon.Open();
			defaultDBLocation = false;
		}
		dbcon.Close();
		
		
		if(defaultDBLocation) {
			dbcon.ConnectionString = connectionString;
			if (File.Exists(sqlFileTest)){
				File.Delete(sqlFileTest);
			}
		} else {
			dbcon.ConnectionString = connectionStringTemp;
			if (File.Exists(sqlFileTestTemp)){
				File.Delete(sqlFileTestTemp);
			}
		}
		dbcmd = dbcon.CreateCommand();

		/*
		LogB.SQL(string.Format("press3"));
	       	splashMessage = "post1";
		needUpdateSplashMessage = true;
		Console.ReadLine();		
		*/

		/*
		try{
			LogB.SQL(string.Format("Trying database in ... " + connectionString));

		//dbcon = new SqliteConnection();
			*/
		/*
			dbcon.ConnectionString = connectionString;
			//dbcon.ConnectionString = connectionStringTemp;
			dbcmd = dbcon.CreateCommand();
		} catch {
			try {
				LogB.SQL(string.Format("Trying database in ... " + connectionStringTemp));

		//dbcon = new SqliteConnection();
				dbcon.ConnectionString = connectionStringTemp;
				dbcmd = dbcon.CreateCommand();
			} catch { 
				LogB.SQL("Problems, exiting...\n");
				System.Console.Out.Close();
				Log.End();
				Log.Delete();
				Environment.Exit(1);
			}

		}

		*/
			
		return defaultDBLocation;
		
	}

	//only create blank DB
	public void ConnectBlank()
	{
		string sqlFileBlank = "chronojump_blank.db"; //copied on /chronojump-x.y/data installjammer will copy it to database
		string connectionStringBlank = "version = 3; Data source = " + sqlFileBlank;

		//delete blank file if exists
		if (File.Exists(sqlFileBlank)) {
			LogB.SQL("File blank exists, deleting...");
			File.Delete(sqlFileBlank);
		}

		dbcon = new SqliteConnection();
		dbcon.ConnectionString = connectionStringBlank;
		dbcmd = dbcon.CreateCommand();

		/*
		 * blankDB is created on Linux (create_release.sh) but
		 * it will work on windows.
		 * this bool allows to put COM? as default port
		 */

		creatingBlankDatabase = true; 
	}
	
	public void CreateDir()
	{
		LogB.SQL(connectionString);

		string applicationDataDir = UtilAll.GetApplicationDataDir();

		if(!Directory.Exists(applicationDataDir)) {
			LogB.SQL("creating dir 1...");
			Directory.CreateDirectory (applicationDataDir);
		}
		
		if(!Directory.Exists(home)) {
			LogB.SQL("creating dir 2...");
			Directory.CreateDirectory (home);
		}
		LogB.SQL("Dirs created.");
	}

	public void CreateFile()
	{
		LogB.SQL("creating file...");
		LogB.SQL(connectionString);
		
		//	if(!Directory.Exists(home)) {
		//		Directory.CreateDirectory (home);
		//	}

		//try {	
		dbcon.Open();
		/*
		   } catch {
		   dbcon.Close();
		   dbcon.ConnectionString = connectionStringTemp;
		   dbcmd = dbcon.CreateCommand();
		   dbcon.Open();
		   }
		   */
		dbcon.Close();
	}
	

	public bool CheckTables(bool defaultDBLocation)
	{
		if(defaultDBLocation) {
			if (File.Exists(sqlFile)){
				return true;
			}
		} else {
			if (File.Exists(sqlFile)){
				//backup the database
				Util.BackupDirCreateIfNeeded();
				Util.BackupDatabase();
				LogB.SQL ("made a database backup"); //not compressed yet, it seems System.IO.Compression.DeflateStream and
				//System.IO.Compression.GZipStream are not in mono

				File.Move(Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db",
					Util.GetDatabaseTempDir() + Path.DirectorySeparatorChar + "chronojump.db");
				return true;
			}
		}
		return false;
	}


	public bool IsSqlite3() {
		if(sqlite3SelectWorks()){
			LogB.SQL("SQLITE3");
			SqliteGeneral.Sqlite.Close();
			return true;
		}
		else if(sqlite2SelectWorks()) {
			LogB.SQL("SQLITE2");
			SqliteGeneral.Sqlite.Close();
			//write sqlFile path on data/databasePath.txt
			//TODO
			//

			return false;
		}
		else {
			LogB.SQL("ERROR in sqlite detection");
			SqliteGeneral.Sqlite.Close();
			return false;
		}
	}
	private bool sqlite3SelectWorks() {
		try {
			SqliteGeneral.SqlitePreferences.Select("chronopicPort");
		} catch {
			/*
			try {
				SqliteGeneral.Sqlite.Close();
				if(File.Exists(Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db"))
					File.Move(Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db",
							Util.GetDatabaseTempDir() + Path.DirectorySeparatorChar + "chronojump.db");

				dbcon.ConnectionString = connectionStringTemp;
				dbcmd = dbcon.CreateCommand();
				SqliteGeneral.Sqlite.Open();
				SqliteGeneral.SqlitePreferences.Select("chronopicPort");
			} catch {
				SqliteGeneral.Sqlite.Close();
				if(File.Exists(Util.GetDatabaseTempDir() + Path.DirectorySeparatorChar + "chronojump.db"))
					File.Move(Util.GetDatabaseTempDir() + Path.DirectorySeparatorChar + "chronojump.db",
							Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db");

			*/
				return false;
			//}
		}
		return true;
	}
	private bool sqlite2SelectWorks() {
		/*
		 *it says:
		 Unhandled Exception: System.NotSupportedException: Only Sqlite Version 3 is supported at this time
		   at Mono.Data.SqliteGeneral.Sqlite.SqliteConnection.Open () [0x00000]
		 *
		SqliteGeneral.Sqlite.Close();
		connectionString = "version=2; URI=file:" + sqlFile;
		dbcon.ConnectionString = connectionString;
		SqliteGeneral.Sqlite.Open();
		try {
			SqliteGeneral.SqlitePreferences.Select("chronopicPort");
		} catch {
			return false;
		}
		*/
		return true;
	}


	public bool ConvertFromSqlite2To3() {
		/*
		 * 1 write the sqlite2 dumped data to an archive
		 * 2 copy db
		 * 3 create sqlite3 file from archive
		 */

		string sqlite2File = Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump-sqlite2.81.db";
		string sqliteDB = Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db";

		File.Copy(sqliteDB, sqlite2File, true);

		string myPath = "";
		string sqliteStr = "";
		//string sqlite3Str = "";
		string extension = "";
		try {
			if(UtilAll.IsWindows()) {
				myPath = Constants.UtilProgramsWindows;
				extension = Constants.ExtensionProgramsWindows;
				sqliteStr = "sqlite.exe";
				//sqlite3Str = "sqlite3.exe";
			}
			else {
				myPath = Constants.UtilProgramsLinux;
				extension = Constants.ExtensionProgramsLinux;
				sqliteStr = "sqlite-2.8.17.bin";
				//sqlite3Str = "sqlite3-3.5.0.bin";
			}

			if(File.Exists(myPath + Path.DirectorySeparatorChar + sqliteStr)) 
				LogB.SQL("exists1");
			if(File.Exists(sqlite2File)) 
				LogB.SQL("exists2");

			/*
			LogB.SQL("{0}-{1}", myPath + Path.DirectorySeparatorChar + sqliteStr , sqlite2File + " .dump");
			ProcessStartInfo ps = new ProcessStartInfo(myPath + Path.DirectorySeparatorChar + sqliteStr , sqlite2File + " .dump");

			ps.UseShellExecute = false;
			//ps.UseShellExecute = true;
			ps.RedirectStandardOutput = true;
			string output = "";
			using(Process p = Process.Start(ps)) {
				//TODO: this doesn't work on windows (it gets hanged)
				p.WaitForExit();
				output = p.StandardOutput.ReadToEnd();
			}
*/
			
			//write the path to chronojumpdb in a txt file (for convert_database.bat and .sh)
			TextWriter writer = File.CreateText(myPath + Path.DirectorySeparatorChar + "db_path.txt");
			string scriptsAreTwoDirsAhead = ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar;
			writer.WriteLine(scriptsAreTwoDirsAhead + Util.GetDatabaseDir());
			((IDisposable)writer).Dispose();
			
			LogB.SQL("Path written");

			Process p2 = Process.Start(myPath + Path.DirectorySeparatorChar + "convert_database." + extension);
			p2.WaitForExit();

			LogB.SQL("sqlite3 db created");
				
			File.Copy(myPath + Path.DirectorySeparatorChar + "tmp.db", sqliteDB, true ); //overwrite
		} catch {
			LogB.Error("PROBLEMS");
			return false;
		}

		LogB.SQL("done");
		return true;

	}

	//for splashWin text
	public string PrintConversionText() {
		double toReach = Convert.ToDouble(Util.ChangeDecimalSeparator(lastChronojumpDatabaseVersion));
		return currentVersion + "/" + toReach.ToString() + " " +
			conversionRate.ToString() + "/" + conversionRateTotal.ToString() + " " +
			conversionSubRate.ToString() + "/" + conversionSubRateTotal.ToString() + " ";
	}

	//for splashWin progressbars
	public double PrintCreation() {
		return Util.DivideSafeFraction(creationRate, creationTotal);
	}
	public double PrintConversionVersion() {
		return Util.DivideSafeFraction(
				Convert.ToDouble(Util.ChangeDecimalSeparator(currentVersion)), 
				Convert.ToDouble(Util.ChangeDecimalSeparator(lastChronojumpDatabaseVersion))
				);
	}
	public double PrintConversionRate() {
		return Util.DivideSafeFraction(conversionRate, conversionRateTotal);
	}
	public double PrintConversionSubRate() {
		return Util.DivideSafeFraction(conversionSubRate, conversionSubRateTotal);
	}

	public bool ConvertToLastChronojumpDBVersion() {
		LogB.SQL("SelectChronojumpProfile ()");

		//if(checkIfIsSqlite2())
		//	convertSqlite2To3();

		addChronopicPortNameIfNotExists();

		currentVersion = SqliteGeneral.SqlitePreferences.Select("databaseVersion");

		//LogB.SQL("lastDB: {0}", Convert.ToDouble(lastChronojumpDatabaseVersion));
		//LogB.SQL("currentVersion: {0}", Convert.ToDouble(currentVersion));

		bool returnSoftwareIsNew = true; //-1 if software is too old for database (moved db to other computer)
		if(
				Convert.ToDouble(Util.ChangeDecimalSeparator(lastChronojumpDatabaseVersion)) == 
				Convert.ToDouble(Util.ChangeDecimalSeparator(currentVersion)))
			LogB.SQL("Database is already latest version");
		else if(
				Convert.ToDouble(Util.ChangeDecimalSeparator(lastChronojumpDatabaseVersion)) < 
				Convert.ToDouble(Util.ChangeDecimalSeparator(currentVersion))) {
			LogB.SQL("User database newer than program, need to update software");
			returnSoftwareIsNew = false;
		} else {
			LogB.Warning("Old database, need to convert");
			LogB.Warning("db version: " + currentVersion);

			bool needToConvertPersonToSport = false;
			bool jumpFallAsDouble = false;
	 		bool runAndRunIntervalInitialSpeedAdded = false;
			bool addedRSA = false;

			SqliteJumpRj sqliteJumpRjObject = new SqliteJumpRj();
			SqliteRunInterval sqliteRunIntervalObject = new SqliteRunInterval();
			SqliteReactionTime sqliteReactionTimeObject = new SqliteReactionTime();
			SqlitePulse sqlitePulseObject = new SqlitePulse();
			SqliteMultiChronopic sqliteMultiChronopicObject = new SqliteMultiChronopic();
			SqlitePersonSessionOld sqlitePersonSessionOldObject = new SqlitePersonSessionOld();

			if(currentVersion == "0.41") {
				SqliteGeneral.Sqlite.Open();

				//SqliteGeneral.SqlitePulse.createTable(Constants.PulseTable);
				sqlitePulseObject.createTable(Constants.PulseTable);
				SqliteGeneral.SqlitePulseType.createTablePulseType();
				SqliteGeneral.SqlitePulseType.initializeTablePulseType();

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.42", true); 
				LogB.SQL("Converted DB to 0.42 (added pulse and pulseType tables)");

				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.42";
			}

			if(currentVersion == "0.42") {
				SqliteGeneral.Sqlite.Open();
				SqliteGeneral.SqlitePulseType.Insert ("Free:-1:-1:free PulseStep mode", true); 
				SqliteGeneral.SqlitePreferences.Insert ("language", "es-ES"); 
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.43", true); 
				LogB.SQL("Converted DB to 0.43 (added 'free' pulseType & language peference)");
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.43";
			}

			if(currentVersion == "0.43") {
				SqliteGeneral.Sqlite.Open();
				SqliteGeneral.SqlitePreferences.Insert ("showQIndex", "False"); 
				SqliteGeneral.SqlitePreferences.Insert ("showDjIndex", "False"); 
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.44", true); 
				LogB.SQL("Converted DB to 0.44 (added showQIndex, showDjIndex)");
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.44";
			}

			if(currentVersion == "0.44") {
				SqliteGeneral.Sqlite.Open();
				SqliteGeneral.SqlitePreferences.Insert ("allowFinishRjAfterTime", "True"); 
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.45", true); 
				LogB.SQL("Converted DB to 0.45 (added allowFinishRjAfterTime)");
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.45";
			}

			if(currentVersion == "0.45") {
				SqliteGeneral.Sqlite.Open();
				SqliteGeneral.SqliteJumpType.JumpTypeInsert ("Free:1:0:Free jump", true); 
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.46", true); 
				LogB.SQL("Added Free jump type");
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.46";
			}

			if(currentVersion == "0.46") {
				SqliteGeneral.Sqlite.Open();

				//SqliteGeneral.SqliteReactionTime.createTable(Constants.ReactionTimeTable);
				sqliteReactionTimeObject.createTable(Constants.ReactionTimeTable);

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.47", true); 
				LogB.SQL("Added reaction time table");
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.47";
			}

			if(currentVersion == "0.47") {
				SqliteGeneral.Sqlite.Open();

				//SqliteGeneral.SqliteJumpRj.createTable(Constants.TempJumpRjTable);
				sqliteJumpRjObject.createTable(Constants.TempJumpRjTable);
				//SqliteGeneral.SqliteRun.intervalCreateTable(Constants.TempRunIntervalTable);
				sqliteRunIntervalObject.createTable(Constants.TempRunIntervalTable);

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.48", true); 
				LogB.SQL("created tempJumpReactive and tempRunInterval tables");
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.48";
			}

			if(currentVersion == "0.48") {
				SqliteGeneral.Sqlite.Open();

				SqliteGeneral.SqliteJumpType.JumpTypeInsert ("Rocket:1:0:Rocket jump", true); 

				string [] iniRunTypes = {
					"Agility-20Yard:18.28:20Yard Agility test",
					"Agility-505:10:505 Agility test",
					"Agility-Illinois:60:Illinois Agility test",
					"Agility-Shuttle-Run:40:Shuttle Run Agility test",
					"Agility-ZigZag:17.6:ZigZag Agility test"
				};
				foreach(string myString in iniRunTypes) {
					string [] s = myString.Split(new char[] {':'});
					RunType type = new RunType();
					type.Name = s[0];
					type.Distance = Convert.ToDouble(s[1]);
					type.Description = s[2];
					SqliteGeneral.SqliteRunType.Insert(type, Constants.RunTypeTable, true);
				}
	

				SqliteGeneral.SqliteEvent.createGraphLinkTable();
				SqliteGeneral.SqliteRunType.AddGraphLinksRunSimpleAgility();	

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.49", true); 
				LogB.SQL("Added graphLinkTable, added Rocket jump and 5 agility tests: (20Yard, 505, Illinois, Shuttle-Run & ZigZag. Added graphs pof the 5 agility tests)");

				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.49";
			}

			if(currentVersion == "0.49") {
				SqliteGeneral.Sqlite.Open();
				SqliteGeneral.SqliteJumpType.Update ("SJ+", "SJl"); 
				SqliteGeneral.SqliteJumpType.Update ("CMJ+", "CJl"); 
				SqliteGeneral.SqliteJumpType.Update ("ABK+", "ABKl"); 
				SqliteGeneral.SqliteJump.ChangeWeightToL();
				SqliteGeneral.SqliteJumpType.AddGraphLinks();	
				SqliteGeneral.SqliteJumpType.AddGraphLinksRj();	
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.50", true); 
				LogB.SQL("changed SJ+ to SJl, same for CMJ+ and ABK+, added jump and jumpRj graph links");
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.50";
			}

			if(currentVersion == "0.50") {
				SqliteGeneral.Sqlite.Open();
				SqliteGeneral.SqliteRunType.AddGraphLinksRunSimple();	
				SqliteGeneral.SqliteRunIntervalType.AddGraphLinksRunInterval();	
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.51", true); 
				LogB.SQL("added graphLinks for run simple and interval");
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.51";
			}

			if(currentVersion == "0.51") {
				SqliteGeneral.Sqlite.Open();
				SqliteGeneral.SqliteJumpType.Update ("CJl", "CMJl"); 
				SqliteGeneral.SqliteEvent.GraphLinkInsert (Constants.JumpTable, "CMJl", "jump_cmj_l.png", true);
				SqliteGeneral.SqliteEvent.GraphLinkInsert (Constants.JumpTable, "ABKl", "jump_abk_l.png", true);
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.52", true); 
				LogB.SQL("added graphLinks for cmj_l and abk_l, fixed CMJl name");
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.52";
			}
			
			if(currentVersion == "0.52") {
				SqliteGeneral.Sqlite.Open();
				sqlitePersonSessionOldObject.createTable (); 
				SqliteGeneral.Sqlite.Close();
				
				//this needs the dbCon closed
				SqliteGeneral.SqlitePersonSessionOld.moveOldTableToNewTable (); 
				
				SqliteGeneral.Sqlite.Open();
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.53", true); 
				SqliteGeneral.Sqlite.Close();
				
				LogB.SQL("created weightSession table. Moved person weight data to weightSession table for each session that has performed");
				currentVersion = "0.53";
			}
			
			if(currentVersion == "0.53") {
				SqliteGeneral.Sqlite.Open();

				SqliteGeneral.SqliteSport.createTable();
				SqliteGeneral.SqliteSport.initialize();
				SqliteGeneral.SqliteSpeciallity.createTable();
				SqliteGeneral.SqliteSpeciallity.initialize();

				//SqlitePersonOld.convertTableToSportRelated (); 
				needToConvertPersonToSport = true;
				
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.54", true); 
				SqliteGeneral.Sqlite.Close();
				
				LogB.SQL("Created sport tables. Added sport data, speciallity and level of practice to person table");
				currentVersion = "0.54";
			}
			if(currentVersion == "0.54") {
				SqliteGeneral.Sqlite.Open();

				SqliteGeneral.SqliteSpeciallity.InsertUndefined(true);

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.55", true); 
				SqliteGeneral.Sqlite.Close();
				
				LogB.SQL("Added undefined to speciallity table");
				currentVersion = "0.55";
			}
			if(currentVersion == "0.55") {
				SqliteGeneral.Sqlite.Open();

				SqliteGeneral.SqliteSessionOld.convertTableAddingSportStuff();

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.56", true); 
				SqliteGeneral.Sqlite.Close();
				
				LogB.SQL("Added session default sport stuff into session table");
				currentVersion = "0.56";
			}
			if(currentVersion == "0.56") {
				SqliteGeneral.Sqlite.Open();

				//jump and jumpRj
				ArrayList arrayAngleAndSimulated = new ArrayList(1);
				arrayAngleAndSimulated.Add("-1"); //angle
				arrayAngleAndSimulated.Add("-1"); //simulated
				
				//run and runInterval
				ArrayList arraySimulatedAndInitialSpeed = new ArrayList(1);
				arraySimulatedAndInitialSpeed.Add("-1"); //simulated
				arraySimulatedAndInitialSpeed.Add("0"); //initial speed
				
				//others
				ArrayList arraySimulated = new ArrayList(1);
				arraySimulated.Add("-1"); //simulated


				conversionRateTotal = 9;
				conversionRate = 1;
				convertTables(new SqliteJump(), Constants.JumpTable, 9, arrayAngleAndSimulated, false);
				conversionRate ++;
				convertTables(new SqliteJumpRj(), Constants.JumpRjTable, 16, arrayAngleAndSimulated, false);
				conversionRate ++;
				convertTables(new SqliteRun(), Constants.RunTable, 7, arraySimulated, false);
				conversionRate ++;
				convertTables(new SqliteRunInterval(), Constants.RunIntervalTable, 11, arraySimulated, false);
				runAndRunIntervalInitialSpeedAdded = true;
				
				conversionRate ++;
				convertTables(new SqliteReactionTime(), Constants.ReactionTimeTable, 6, arraySimulated, false);
				conversionRate ++;
				convertTables(new SqlitePulse(), Constants.PulseTable, 8, arraySimulated, false);


				//reacreate temp tables for have also the simulated column
				conversionRate ++;
				SqliteGeneral.Sqlite.dropTable(Constants.TempJumpRjTable);
				sqliteJumpRjObject.createTable(Constants.TempJumpRjTable);
				SqliteGeneral.Sqlite.dropTable(Constants.TempRunIntervalTable);
				sqliteRunIntervalObject.createTable(Constants.TempRunIntervalTable);

				conversionRate ++;
				SqliteGeneral.SqliteCountry.createTable();
				SqliteGeneral.SqliteCountry.initialize();
				
				conversionRate ++;
				int columnsBefore = 10;
				bool putDescriptionInMiddle = false;
				ArrayList arrayPersonRaceCountryServerID = new ArrayList(1);
				if(needToConvertPersonToSport) {
					columnsBefore = 7;
					arrayPersonRaceCountryServerID.Add(Constants.SportUndefinedID.ToString());
					arrayPersonRaceCountryServerID.Add(Constants.SpeciallityUndefinedID.ToString());
					arrayPersonRaceCountryServerID.Add(Constants.LevelUndefinedID.ToString());
					putDescriptionInMiddle = true;
				}
				arrayPersonRaceCountryServerID.Add(Constants.RaceUndefinedID.ToString());
				arrayPersonRaceCountryServerID.Add(Constants.CountryUndefinedID.ToString());
				arrayPersonRaceCountryServerID.Add(Constants.ServerUndefinedID.ToString());
				convertTables(new SqlitePersonOld(), Constants.PersonOldTable, columnsBefore, arrayPersonRaceCountryServerID, putDescriptionInMiddle);

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.57", true); 
				SqliteGeneral.Sqlite.Close();
				
				LogB.SQL("Added simulated column to each event table on client. Added to person: race, country, serverUniqueID. Convert to sport related done here if needed. Added also run and runInterval initial speed");
				currentVersion = "0.57";
			}
			if(currentVersion == "0.57") {
				SqliteGeneral.Sqlite.Open();
		
				//check if "republic" is in country table
				if(SqliteGeneral.SqliteCountry.TableHasOldRepublicStuff()){
					conversionRateTotal = 4;
					conversionRate = 1;
					SqliteGeneral.Sqlite.dropTable(Constants.CountryTable);
					conversionRate ++;
					SqliteGeneral.SqliteCountry.createTable();
					conversionRate ++;
					SqliteGeneral.SqliteCountry.initialize();
					conversionRate ++;
					LogB.SQL("Countries without kingdom or republic (except when needed)");
				}
				
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.58", true); 
				SqliteGeneral.Sqlite.Close();
				
				currentVersion = "0.58";
			}

			if(currentVersion == "0.58") {
				SqliteGeneral.Sqlite.Open();
				conversionRateTotal = 2;
				conversionRate = 1;
				SqliteGeneral.SqlitePreferences.Insert ("showAngle", "False"); 
				alterTableColumn(new SqliteJump(), Constants.JumpTable, 11);

				//jump fall is also converted to double (don't need to do at conversion to 0.76)
				jumpFallAsDouble = true;

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.59", true); 
				LogB.SQL("Converted DB to 0.59 (added 'showAngle' to preferences, changed angle on jump to double)"); 
				conversionRate = 2;
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.59";
			}

			if(currentVersion == "0.59") {
				SqliteGeneral.Sqlite.Open();
				conversionRateTotal = 4;

				conversionRate = 1;
				SqliteGeneral.SqlitePreferences.Insert ("volumeOn", "True"); 
				SqliteGeneral.SqlitePreferences.Insert ("evaluatorServerID", "-1");

				conversionRate = 2;
			
				int columnsBefore = 8;
				ArrayList arrayServerID = new ArrayList(1);
				arrayServerID.Add(Constants.ServerUndefinedID.ToString());
				convertTables(new SqliteSession(), Constants.SessionTable, columnsBefore, arrayServerID, false);
				
				conversionRate = 3;
				SqliteGeneral.SqliteEvent.SimulatedConvertToNegative();

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.60", true); 
				LogB.SQL("Converted DB to 0.60 (added volumeOn and evaluatorServerID to preferences. session has now serverUniqueID. Simulated now are -1, because 0 is real and positive is serverUniqueID)"); 
				
				conversionRate = 4;
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.60";
			}

			if(currentVersion == "0.60") {
				SqliteGeneral.Sqlite.Open();
				conversionRateTotal = 3;
				conversionRate = 1;

				ArrayList arrayDS = new ArrayList(1);
				arrayDS.Add("-1"); //distancesString
				convertTables(new SqliteRunIntervalType(), Constants.RunIntervalTypeTable, 7, arrayDS, false);
				
				conversionRate = 2;

				//SqliteRunType.RunIntervalTypeInsert ("MTGUG:-1:true:3:false:Modified time Getup and Go test:1-7-19", true);
				RunType type = new RunType();
				type.Name = "MTGUG";
				type.Distance = -1;
				type.TracksLimited = true;
				type.FixedValue = 3;
				type.Unlimited = false;
				type.Description = "Modified time Getup and Go test";
				type.DistancesString = "1-7-19";
				SqliteGeneral.SqliteRunIntervalType.Insert(type, Constants.RunIntervalTypeTable, true);
				
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.61", true); 
				LogB.SQL("Converted DB to 0.61 added RunIntervalType distancesString (now we van have interval tests with different distances of tracks). Added MTGUG");
				
				conversionRate = 3;
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.61";
			}
			if(currentVersion == "0.61") {
				SqliteGeneral.Sqlite.Open();
				SqliteGeneral.SqliteJumpType.JumpRjTypeInsert ("RJ(hexagon):1:0:1:18:Reactive Jump on a hexagon until three full revolutions are done", true);
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.62", true); 
				LogB.SQL("Converted DB to 0.62 added hexagon");
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.62";
			}
			if(currentVersion == "0.62") {
				SqliteGeneral.Sqlite.Open();
				SqliteGeneral.SqlitePreferences.Insert ("versionAvailable", "");
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.63", true); 
				LogB.SQL("Converted DB to 0.63 (added 'versionAvailable' to preferences)"); 
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.63";
			}
			if(currentVersion == "0.63") {
				SqliteGeneral.Sqlite.Open();
				
				RunType type = new RunType();
				type.Name = "Margaria";
				type.Distance = 0;
				type.Description = "Margaria-Kalamen test";
				SqliteGeneral.SqliteRunType.Insert(type, Constants.RunTypeTable, true);

				SqliteGeneral.SqliteEvent.GraphLinkInsert (Constants.RunTable, "Margaria", "margaria.png", true);
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.64", true); 
				
				LogB.SQL("Converted DB to 0.64 (added margaria test)"); 
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.64";
			}
			if(currentVersion == "0.64") {
				SqliteGeneral.Sqlite.Open();
				
				SqliteServer sqliteServerObject = new SqliteServer();
				//user has also an evaluator table with a row (it's row)	
				sqliteServerObject.CreateEvaluatorTable();

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.65", true); 
				
				LogB.SQL("Converted DB to 0.65 (added Sevaluator on client)"); 
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.65";
			}
			if(currentVersion == "0.65") {
				SqliteGeneral.Sqlite.Open();
				//now runAnalysis is a multiChronopic event
				//SqliteGeneral.SqliteJumpType.JumpRjTypeInsert ("RunAnalysis:0:0:1:-1:Run between two photocells recording contact and flight times in contact platform/s. Until finish button is clicked.", true);

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.66", true); 
				
				//LogB.SQL("Converted DB to 0.66 (added RunAnalysis Reactive jump)"); 
				LogB.SQL("Converted DB to 0.66 (done nothing)"); 
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.66";
			}
			if(currentVersion == "0.66") {
				SqliteGeneral.Sqlite.Open();
				SqliteGeneral.SqliteJumpType.JumpTypeInsert ("TakeOff:0:0:Take off", true);
				SqliteGeneral.SqliteJumpType.JumpTypeInsert ("TakeOffWeight:0:0:Take off with weight", true);

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.67", true); 
				
				LogB.SQL("Converted DB to 0.67 (added TakeOff jumps)"); 
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.67";
			}
			if(currentVersion == "0.67") {
				SqliteGeneral.Sqlite.Open();
				sqliteMultiChronopicObject.createTable(Constants.MultiChronopicTable);

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.68", true); 
				
				LogB.SQL("Converted DB to 0.68 (added multiChronopic tests table)"); 
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.68";
			}
			if(currentVersion == "0.68") {
				SqliteGeneral.Sqlite.Open();
				
				RunType type = new RunType();
				type.Name = "Gesell-DBT";
				type.Distance = 2.5;
				type.Description = "Gesell Dynamic Balance Test";
				SqliteGeneral.SqliteRunType.Insert(type, Constants.RunTypeTable, true);

				SqliteGeneral.SqliteEvent.GraphLinkInsert (Constants.RunTable, "Gesell-DBT", "gesell_dbt.png", true);
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.69", true); 
				
				LogB.SQL("Converted DB to 0.69 (added Gesell-DBT test)"); 
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.69";
			}
			if(currentVersion == "0.69") {
				SqliteGeneral.Sqlite.Open();
				SqliteGeneral.SqlitePreferences.Insert ("showPower", "True"); 
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.70", true); 
				LogB.SQL("Converted DB to 0.70 (added showPower)");
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.70";
			}
			if(currentVersion == "0.70") {
				SqliteGeneral.Sqlite.Open();
				
				SqliteGeneral.SqlitePersonSessionNotUpload.CreateTable();

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.71", true); 
				
				LogB.SQL("Converted DB to 0.71 (created personNotUploadTable on client)"); 
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.71";
			}
			if(currentVersion == "0.71") {
				SqliteGeneral.Sqlite.Open();
				
				datesToYYYYMMDD();

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.72", true); 
				
				LogB.SQL("Converted DB to 0.72 (dates to YYYY-MM-DD)"); 
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.72";
			}
			if(currentVersion == "0.72") {
				SqliteGeneral.Sqlite.Open();
				
				deleteOrphanedPersonsOld();

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.73", true); 
				
				LogB.SQL("Converted DB to 0.73 (deleted orphaned persons (in person table but not in personSessionWeight table)"); 
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.73";
			}
			if(currentVersion == "0.73") {
				//dbcon open laters on mid convertDJinDJna()
				
				convertDJInDJna();

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.74", true); 
				
				LogB.SQL("Converted DB to 0.74 (All DJ converted to DJna)"); 
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.74";
			}
			if(currentVersion == "0.74") {
				conversionRateTotal = 3;
				conversionRate = 1;
				
				SqliteGeneral.Sqlite.Open();

				convertTables(new SqlitePersonOld(), Constants.PersonOldTable, 13, new ArrayList(), false);
				conversionRate++;
				
				convertTables(new SqlitePersonSessionOld(), Constants.PersonSessionOldWeightTable, 4, new ArrayList(), false);

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.75", true); 
				conversionRate++;
				
				LogB.SQL("Converted DB to 0.75 (person, and personSessionWeight have height and weight as double)"); 
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.75";
			}
			if(currentVersion == "0.75") {
				conversionRateTotal = 3;
				conversionRate = 1;
				SqliteGeneral.Sqlite.Open();

				if(!jumpFallAsDouble)
					alterTableColumn(new SqliteJump(), Constants.JumpTable, 11);
				
				conversionRate++;
				
				alterTableColumn(new SqliteJumpRj(), Constants.JumpRjTable, 18);
				
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.76", true); 
				conversionRate++;
				
				LogB.SQL("Converted DB to 0.76 (jump & jumpRj falls as double)"); 
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.76";
			}
			if(currentVersion == "0.76") {
				SqliteGeneral.Sqlite.Open();
				
				convertPersonAndPersonSessionTo77();
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.77", true); 
				LogB.SQL("Converted DB to 0.77 (person77, personSession77)"); 
				
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.77";
			}
			if(currentVersion == "0.77") {
				SqliteGeneral.Sqlite.Open();

				SqliteGeneral.SqliteJumpType.UpdateOther ("weight", Constants.TakeOffWeightName, "1"); 

				Random rnd = new Random();
				string machineID = rnd.Next().ToString();
				SqliteGeneral.SqlitePreferences.Insert ("machineID", machineID); 

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.78", true); 
				LogB.SQL("Converted DB to 0.78 (Added machineID to preferences, takeOffWeight has no weight in db conversions since 0.66)"); 

				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.78";
			}
			if(currentVersion == "0.78") {
				SqliteGeneral.Sqlite.Open();

				SqliteGeneral.SqlitePreferences.Insert ("multimediaStorage", Constants.MultimediaStorage.BYSESSION.ToString());

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.79", true); 
				LogB.SQL("Converted DB to 0.79 (Added multimediaStorage structure id)"); 

				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.79";
			}
			if(currentVersion == "0.79") {
				SqliteGeneral.Sqlite.Open();

	 			if(! runAndRunIntervalInitialSpeedAdded) {
					ArrayList myArray = new ArrayList(1);
					myArray.Add("0"); //initial speed
				
					conversionRateTotal = 3;
					conversionRate = 1;
					convertTables(new SqliteRun(), Constants.RunTable, 8, myArray, false);
					conversionRate ++;
					convertTables(new SqliteRunInterval(), Constants.RunIntervalTable, 12, myArray, false);
					conversionRate ++;
					LogB.SQL("Converted DB to 0.80 Added run and runInterval initial speed (if not done in 0.56 conversion)"); 
				}

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.80", true); 
				
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.80";
			}
			if(currentVersion == "0.80") {
				SqliteGeneral.Sqlite.Open();

				ArrayList myArray = new ArrayList(1);
				myArray.Add("0"); //initial speed
				
				conversionRateTotal = 2;
				conversionRate = 1;
				SqliteGeneral.Sqlite.dropTable(Constants.TempRunIntervalTable);
				sqliteRunIntervalObject.createTable(Constants.TempRunIntervalTable);
				conversionRate ++;
				LogB.SQL("Converted DB to 0.81 Added tempRunInterval initial speed"); 

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.81", true); 
				
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.81";
			}
			if(currentVersion == "0.81") {
				SqliteGeneral.Sqlite.Open();
				conversionRateTotal = 2;

				conversionRate = 1;
				SqliteGeneral.SqlitePreferences.Insert ("videoOn", "False"); 
				conversionRate = 2;
				LogB.SQL("Converted DB to 0.82 Added videoOn"); 

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.82", true); 
				
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.82";
			}
			if(currentVersion == "0.82") {
				SqliteGeneral.Sqlite.Open();
				conversionRateTotal = 2;
				
				conversionRate = 1;
				SqliteGeneral.SqliteEncoder.createTableEncoder();
				SqliteGeneral.SqliteEncoder.createTableEncoderExercise();
				SqliteGeneral.SqliteEncoder.initializeTableEncoderExercise();
				conversionRate = 2;
				LogB.SQL("Created encoder tables.");

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.83", true); 
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.83";
			}
			if(currentVersion == "0.83") {
				SqliteGeneral.Sqlite.Open();
				
				RunType type = new RunType();
				type.Name = "RSA 8-4-R3-5";
				type.Distance = -1;
				type.TracksLimited = true;
				type.FixedValue = 4;
				type.Unlimited = false;
				type.Description = "RSA testing";
				type.DistancesString = "8-4-R3-5";
				SqliteGeneral.SqliteRunIntervalType.Insert(type, Constants.RunIntervalTypeTable, true);

				LogB.SQL("Added 1st RSA test.");

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.84", true); 
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.84";
			}
			if(currentVersion == "0.84") {
				SqliteGeneral.Sqlite.Open();
				SqliteGeneral.SqliteJumpType.JumpTypeInsert ("slCMJ:1:0:Single-leg CMJ jump", true);

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.85", true); 
				
				LogB.SQL("Converted DB to 0.85 (added slCMJ jump)"); 
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.85";
			}
			if(currentVersion == "0.85") {
				SqliteGeneral.Sqlite.Open();
				LogB.SQL("Converted DB to 0.86 videoOn: TRUE"); 

				SqliteGeneral.SqlitePreferences.Update("videoOn", "True", true);
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.86", true); 
				
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.86";
			}
			if(currentVersion == "0.86") {
				SqliteGeneral.Sqlite.Open();
				LogB.SQL("Added run speed start preferences on sqlite"); 

				SqliteGeneral.SqlitePreferences.Insert ("runSpeedStartArrival", "True");
				SqliteGeneral.SqlitePreferences.Insert ("runISpeedStartArrival", "True");
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.87", true); 
				
				SqliteGeneral.Sqlite.Close();
				currentVersion = "0.87";
			}
			if(currentVersion == "0.87") {
				//delete runInterval type
				SqliteGeneral.SqliteRunIntervalType.Delete("RSA 8-4-R3-5");

				//delete all it's runs
				SqliteGeneral.Sqlite.Open();
				dbcmd.CommandText = "DELETE FROM " + Constants.RunIntervalTable +
					" WHERE type == \"RSA 8-4-R3-5\"";
				LogB.SQL(dbcmd.CommandText.ToString());
				dbcmd.ExecuteNonQuery();
				
				//add know RSAs
				SqliteGeneral.SqliteRunIntervalType.addRSA();
				addedRSA = true;

				LogB.SQL("Deleted fake RSA test and added known RSA tests.");
				
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.88", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "0.88";
			}
			if(currentVersion == "0.88") {
				SqliteGeneral.Sqlite.Open();
	
				SqliteGeneral.SqliteEncoder.addEncoderFreeExercise();
				
				LogB.SQL("Added encoder exercise: Free");
				
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.89", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "0.89";
			}
			if(currentVersion == "0.89") {
				SqliteGeneral.Sqlite.Open();
	
				SqliteGeneral.SqlitePreferences.Insert("encoderPropulsive", "True");
				SqliteGeneral.SqlitePreferences.Insert("encoderSmoothEccCon", "0.6");
				SqliteGeneral.SqlitePreferences.Insert("encoderSmoothCon", "0.7");
				LogB.SQL("Preferences added propulsive and encoder smooth");
				
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.90", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "0.90";
			}
			if(currentVersion == "0.90") {
				SqliteGeneral.Sqlite.Open();
				
				SqliteGeneral.SqliteEncoder.UpdateExercise(true, "Squat", "Squat", 100, "weight bar", "", "");	
				LogB.SQL("Encoder Squat 75% -> 100%");
				
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.91", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "0.91";
			}
			if(currentVersion == "0.91") {
				SqliteGeneral.Sqlite.Open();
				
				SqliteGeneral.SqlitePreferences.Insert("videoDevice", "0");
				LogB.SQL("Added videoDevice to preferences");
				
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.92", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "0.92";
			}
			if(currentVersion == "0.92") {
				SqliteGeneral.Sqlite.Open();
				
				SqliteGeneral.SqliteEncoder.UpdateExercise(true, "Bench press", "Bench press", 0, "weight bar", "","0.185");
				SqliteGeneral.SqliteEncoder.UpdateExercise(true, "Squat", "Squat", 100, "weight bar", "","0.31");
				LogB.SQL("Added speed1RM on encoder exercise");
				
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.93", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "0.93";
			}
			if(currentVersion == "0.93") {
				SqliteGeneral.Sqlite.Open();
				
				SqliteGeneral.SqliteEncoder.createTable1RM();
				LogB.SQL("Added encoder1RM table");
				
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.94", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "0.94";
			}
			if(currentVersion == "0.94") {
				SqliteGeneral.Sqlite.Open();
				
				SqliteGeneral.SqlitePreferences.Insert ("encoder1RMMethod", 
						Constants.Encoder1RMMethod.WEIGHTED2.ToString());
				LogB.SQL("Added encoder1RMMethod");
				
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.95", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "0.95";
			}
			if(currentVersion == "0.95") {
				SqliteGeneral.Sqlite.Open();
				
				Update(true, Constants.EncoderTable, "future3", "", Constants.EncoderConfigurationNames.LINEAR.ToString(), 
						"signalOrCurve", "signal");
				Update(true, Constants.EncoderTable, "future3", "0", Constants.EncoderConfigurationNames.LINEAR.ToString(), 
						"signalOrCurve", "signal");
				Update(true, Constants.EncoderTable, "future3", "1", Constants.EncoderConfigurationNames.LINEARINVERTED.ToString(),
						"signalOrCurve", "signal");

				LogB.SQL("Encoder signal future3 three modes");
				
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.96", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "0.96";
			}
			if(currentVersion == "0.96") {
				SqliteGeneral.Sqlite.Open();
				
				SqliteGeneral.SqlitePreferences.Insert ("inertialmomentum", "0.01");
				LogB.SQL("Added inertialmomentum in preferences");
				
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.97", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "0.97";
			}
			if(currentVersion == "0.97") {
				SqliteGeneral.Sqlite.Open();
				
				Update(true, Constants.EncoderTable, "laterality", "both", "RL", "", "");
				Update(true, Constants.EncoderTable, "laterality", "Both", "RL", "", "");
				Update(true, Constants.EncoderTable, "laterality", Catalog.GetString("Both"), "RL", "", "");
				Update(true, Constants.EncoderTable, "laterality", "right", "R", "", "");
				Update(true, Constants.EncoderTable, "laterality", "Right", "R", "", "");
				Update(true, Constants.EncoderTable, "laterality", Catalog.GetString("Right"), "R", "", "");
				Update(true, Constants.EncoderTable, "laterality", "left", "L", "", "");
				Update(true, Constants.EncoderTable, "laterality", "Left", "L", "", "");
				Update(true, Constants.EncoderTable, "laterality", Catalog.GetString("Left"), "L", "", "");
				LogB.SQL("Fixed encoder laterality");
				
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.98", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "0.98";
			}
			if(currentVersion == "0.98") {
				SqliteGeneral.Sqlite.Open();
		
				ArrayList array = SqliteGeneral.SqliteOldConvert.EncoderSelect098(true,-1,-1,-1,"all",false);
				
				conversionRateTotal = array.Count;
				
				dropTable(Constants.EncoderTable);
				
				//CAUTION: do like this and never do createTableEncoder,
				//because method will change in the future and will break updates
				SqliteGeneral.SqliteOldConvert.createTableEncoder99(); 
			
				int count = 1;	
				foreach( EncoderSQL098 es in array) {
					conversionRate = count;
				
					//do not use SqliteGeneral.SqliteEncoder.Insert because that method maybe changes in the future,
					//and here we need to do a conversion that works from 0.98 to 0.99
					dbcmd.CommandText = "INSERT INTO " + Constants.EncoderTable +  
						" (uniqueID, personID, sessionID, exerciseID, eccon, laterality, extraWeight, " + 
						"signalOrCurve, filename, url, time, minHeight, smooth, description, status, " +
						"videoURL, mode, inertiaMomentum, diameter, future1, future2, future3)" +
						" VALUES (" + es.uniqueID + ", " +
						es.personID + ", " + es.sessionID + ", " +
						es.exerciseID + ", \"" + es.eccon + "\", \"" +
						es.laterality + "\", \"" + es.extraWeight + "\", \"" +
						es.signalOrCurve + "\", \"" + es.filename + "\", \"" +
						es.url + "\", " + es.time + ", " + es.minHeight + ", " +
						Util.ConvertToPoint(es.smooth) + ", \"" + es.description + "\", \"" +
						es.future1 + "\", \"" + es.future2 + "\", \"LINEAR\", " + //status, videoURL, mode
						"0, 0, \"\", \"\", \"\")"; //inertiaMomentum, diameter, future1, 2, 3
					LogB.SQL(dbcmd.CommandText.ToString());
					dbcmd.ExecuteNonQuery();
					count ++;
				}	

				conversionRate = count;
				LogB.SQL("Encoder table improved");
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "0.99", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "0.99";
			}
			if(currentVersion == "0.99") {
				SqliteGeneral.Sqlite.Open();

				SqliteGeneral.SqliteEncoder.putEncoderExerciseAnglesAt90();
				SqliteGeneral.SqliteEncoder.addEncoderJumpExercise();
				SqliteGeneral.SqliteEncoder.addEncoderInclinedExercises();

				LogB.SQL("Added Free and inclinedExercises");
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.00", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "1.00";
			}
			if(currentVersion == "1.00") {
				SqliteGeneral.Sqlite.Open();
			
				SqliteGeneral.SqlitePreferences.Insert ("CSVExportDecimalSeparator", Util.GetDecimalSeparatorFromLocale());

				LogB.SQL("Added export to CSV configuration on preferences");
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.01", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "1.01";
			}
			if(currentVersion == "1.01") {
				SqliteGeneral.Sqlite.Open();
			
				RunType type = new RunType("Agility-T-Test");
				SqliteGeneral.SqliteRunType.Insert(type, Constants.RunTypeTable, true);
				type = new RunType("Agility-3L3R");
				SqliteGeneral.SqliteRunIntervalType.Insert(type, Constants.RunIntervalTypeTable, true);

				LogB.SQL("Added Agility Tests: Agility-T-Test, Agility-3l3R");
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.02", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "1.02";
			}
			if(currentVersion == "1.02") {
				SqliteGeneral.Sqlite.Open();
		
				DeleteFromName(true, Constants.EncoderExerciseTable, "Inclinated plane Custom");
				SqliteGeneral.SqliteEncoder.removeEncoderExerciseAngles();

				LogB.SQL("Updated encoder exercise, angle is now on encoder configuration");
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.03", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "1.03";
			}
			if(currentVersion == "1.03") {
				SqliteGeneral.Sqlite.Open();
		
				ArrayList array = SqliteGeneral.SqliteOldConvert.EncoderSelect103(true,-1,-1,-1,"all",false);
				
				conversionRateTotal = array.Count;
				
				dropTable(Constants.EncoderTable);
				
				//CAUTION: do like this and never do createTableEncoder,
				//because method will change in the future and will break updates
				SqliteGeneral.SqliteOldConvert.createTableEncoder104(); 
				
				//in this conversion put this as default for all SQL rows
				EncoderConfiguration econf = new EncoderConfiguration();
			
				int count = 1;	
				foreach(EncoderSQL103 es in array) {
					conversionRate = count;
				
					//do not use SqliteGeneral.SqliteEncoder.Insert because that method maybe changes in the future,
					//and here we need to do a conversion that works from 1.03 to 1.04
					dbcmd.CommandText = "INSERT INTO " + Constants.EncoderTable +  
						" (uniqueID, personID, sessionID, exerciseID, eccon, laterality, extraWeight, " + 
						"signalOrCurve, filename, url, time, minHeight, description, status, " +
						"videoURL, encoderConfiguration, future1, future2, future3)" +
						" VALUES (" + es.uniqueID + ", " +
						es.personID + ", " + es.sessionID + ", " +
						es.exerciseID + ", \"" + es.eccon + "\", \"" +
						es.laterality + "\", \"" + es.extraWeight + "\", \"" +
						es.signalOrCurve + "\", \"" + es.filename + "\", \"" +
						es.url + "\", " + es.time + ", " + es.minHeight + ", \"" + es.description + "\", \"" + 
						es.status + "\", \"" + es.videoURL + "\", \"" + 
						econf.ToStringOutput(EncoderConfiguration.Outputs.SQL) + "\", \"" + //in this conversion put this as default for all SQL rows.
						es.future1 + "\", \"" + es.future2 + "\", \"" + es.future3 + "\")";
					LogB.SQL(dbcmd.CommandText.ToString());
					dbcmd.ExecuteNonQuery();
					count ++;
				}	

				conversionRate = count;
				LogB.SQL("Encoder table improved");
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.04", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "1.04";
			}
			if(currentVersion == "1.04") {
				SqliteGeneral.Sqlite.Open();
				
				dbcmd.CommandText = "DELETE FROM " + Constants.EncoderTable + 
					" WHERE encoderConfiguration LIKE \"%INERTIAL%\" AND " +
					" signalOrCurve == \"curve\"";
				LogB.SQL(dbcmd.CommandText.ToString());
				dbcmd.ExecuteNonQuery();

				LogB.SQL("Removed inertial curves, because sign was not checked on 1.04 when saving curves");
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.05", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "1.05";
			}
			if(currentVersion == "1.05") {
				SqliteGeneral.Sqlite.Open();
		
				SqliteGeneral.SqliteEncoder.createTableEncoderSignalCurve();

				ArrayList signals = SqliteGeneral.SqliteEncoder.Select(true, -1, -1, -1, Constants.EncoderGI.ALL,
						-1, "signal", EncoderSQL.Eccons.ALL, false, false);
				ArrayList curves = SqliteGeneral.SqliteEncoder.Select(true, -1, -1, -1, Constants.EncoderGI.ALL,
						-1, "curve", EncoderSQL.Eccons.ALL, false, false);
				int signalID;
				conversionRateTotal = signals.Count;
				conversionRate = 1;
				//in 1.05 curves can be related to signals only by date
				foreach(EncoderSQL s in signals) {
					conversionRate ++;
					conversionSubRateTotal = curves.Count;
					conversionSubRate = 1;
							
					//needed to know if there are duplicates
					ArrayList curvesStored = new ArrayList();

					foreach(EncoderSQL c in curves) {
						conversionSubRate ++;
						if(s.GetDate(false) == c.GetDate(false) && s.eccon == c.eccon) {
							int msCentral = SqliteGeneral.SqliteEncoder.FindCurveInSignal(
									s.GetFullURL(false), c.GetFullURL(false));

							signalID = Convert.ToInt32(s.uniqueID);
							if(msCentral == -1)
								signalID = -1; //mark as an orphaned curve (without signal)

							/*
							 * about duplicated curves from 1.05 and before:
							 * There are two kind of duplicates, in both, eccon is the same, and they overlap.
							 *
							 * Overlapings situations:
							 * - they are saved two times (same msCentral), or
							 * - they are saved two times with different smoothing (different msCentral)
							 *
							 * from now on (1.06) there will be no more duplicates
							 * about the old duplicated curves, is the user problem,
							 * except the curves of the first kind, that we know exactly that they are duplicated
							 */
							
							//curves come sorted by UniqueID DESC (selected with orderIDascendent = false)
							//if does not exist: insert in encoderSignalCurve
							bool exists = false;
							foreach(int ms in curvesStored)
								if(ms == msCentral)
									exists = true;
							if(exists) {
								//delete this (newer will not be deleted)
								SqliteGeneral.Sqlite.Delete(true, 
										Constants.EncoderTable, Convert.ToInt32(c.uniqueID));
							} else {
								curvesStored.Add(msCentral);
								SqliteGeneral.SqliteEncoder.SignalCurveInsert(true, 
										signalID, Convert.ToInt32(c.uniqueID), msCentral);
							}
						}
					}
				}

				conversionSubRate ++;
				conversionRate ++;
				LogB.SQL("Curves are now linked to signals");
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.06", true); 
				
				SqliteGeneral.Sqlite.Close();
				currentVersion = "1.06";
			}
			if(currentVersion == "1.06") {
				SqliteGeneral.Sqlite.Open();
			
				Update(true, Constants.GraphLinkTable, "graphFileName", "jump_dj.png", "jump_dj_a.png",
						"eventName", "DJa");
				
				LogB.SQL("Added jump_dj_a.png");
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.07", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "1.07";
			}
			if(currentVersion == "1.07") {
				SqliteGeneral.Sqlite.Open();
			
				LogB.SQL("Added translate statistics graph option to preferences");
				
				SqliteGeneral.SqlitePreferences.Insert ("RGraphsTranslate", "True"); 
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.08", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "1.08";
			}
			if(currentVersion == "1.08") {
				SqliteGeneral.Sqlite.Open();
			
				LogB.SQL("Added option on preferences to useHeightsOnJumpIndexes (default) or not");
				
				SqliteGeneral.SqlitePreferences.Insert ("useHeightsOnJumpIndexes", "True"); 
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.09", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "1.09";
			}
			if(currentVersion == "1.09") {
				SqliteGeneral.Sqlite.Open();
			
				LogB.SQL("Added RSA RAST on runType");

				/*
				 * addRSA() contains RAST since 1.10
				 * database started at 1.10 or more contains RAST
				 * database started before 0.87 adds RAST on the addRSA() method
				 * satabase started after 0.87 adds RAST now
				 */
				if(! addedRSA) {
					RunType type = new RunType();
					type.Name = "RSA RAST 35, R10 x 6";
					type.Distance = -1;
					type.TracksLimited = true;
					type.FixedValue = 12;
					type.Unlimited = false;
					type.Description = "RSA RAST Test";
					type.DistancesString = "35-R10";
					
					SqliteGeneral.SqliteRunIntervalType.Insert(type, Constants.RunIntervalTypeTable, true);
					addedRSA = true;
				}
				
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.10", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "1.10";
			}
			if(currentVersion == "1.10") {
				SqliteGeneral.Sqlite.Open();
			
				LogB.SQL("Added option on autosave curves on capture (all/bestmeanpower/none)");
				
				SqliteGeneral.SqlitePreferences.Insert ("encoderAutoSaveCurve", Constants.EncoderAutoSaveCurve.BEST.ToString()); 
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.11", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "1.11";
			}
			if(currentVersion == "1.11") {
				SqliteGeneral.Sqlite.Open();
			
				LogB.SQL("URLs from absolute to relative)");
				
				SqliteGeneral.SqliteOldConvert.ConvertAbsolutePathsToRelative(); 
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.12", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "1.12";
			}
			if(currentVersion == "1.12") {
				SqliteGeneral.Sqlite.Open();
			
				LogB.SQL("Added ExecuteAuto table");
				
				SqliteGeneral.SqliteExecuteAuto.createTableExecuteAuto();
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.13", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "1.13";
			}
			if(currentVersion == "1.13") {
				SqliteGeneral.Sqlite.Open();
			
				LogB.SQL("slCMJ -> slCMJleft, slCMJright");

				SqliteGeneral.SqliteOldConvert.slCMJDivide();
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.14", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "1.14";
			}
			if(currentVersion == "1.14") {
				SqliteGeneral.Sqlite.Open();
			
				LogB.SQL("added Chronojump profile and bilateral profile");

				SqliteGeneral.SqliteExecuteAuto.addChronojumpProfileAndBilateral();
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.15", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "1.15";
			}
			if(currentVersion == "1.15") {
				SqliteGeneral.Sqlite.Open();
			
				LogB.SQL("Cyprus moved to Europe");

				Update(true, Constants.CountryTable, "continent", "Asia", "Europe", "code", "CYP"); 
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.16", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "1.16";
			}
			if(currentVersion == "1.16") {
				SqliteGeneral.Sqlite.Open();
			
				LogB.SQL("Deleting Max jump");

				Update(true, Constants.JumpTable, "type", "Max", "Free", "", ""); 
				DeleteFromName(true, Constants.JumpTypeTable, "Max");
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.17", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "1.17";
			}
			if(currentVersion == "1.17") {
				SqliteGeneral.Sqlite.Open();
			
				LogB.SQL("Deleted Negative runInterval runs (bug from last version)");

				SqliteGeneral.SqliteOldConvert.deleteNegativeRuns();
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.18", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "1.18";
			}
			if(currentVersion == "1.18") {
				LogB.SQL("Preferences deleted showHeight, added showStiffness");
				
				SqliteGeneral.Sqlite.Open();
				DeleteFromName(true, Constants.PreferencesTable, "showHeight");
				SqliteGeneral.SqlitePreferences.Insert ("showStiffness", "True"); 
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.19", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "1.19";
			}
			if(currentVersion == "1.19") {
				LogB.SQL("Preferences: added user email");
				
				SqliteGeneral.Sqlite.Open();
				SqliteGeneral.SqlitePreferences.Insert ("email", ""); 
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.20", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "1.20";
			}
			if(currentVersion == "1.20") {
				LogB.SQL("Fixing loosing of encoder videoURL after recalculate");
				
				SqliteGeneral.Sqlite.Open();
				
				SqliteGeneral.SqliteOldConvert.ConvertAbsolutePathsToRelative(); //videoURLs got absolute again
				SqliteGeneral.SqliteOldConvert.FixLostVideoURLAfterEncoderRecalculate();

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.21", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "1.21";
			}
			if(currentVersion == "1.21") {
				LogB.SQL("Encoder laterality in english again");
				
				SqliteGeneral.Sqlite.Open();
		
				if(Catalog.GetString("RL") != "RL")	
					Update(true, Constants.EncoderTable, "laterality", Catalog.GetString("RL"), "RL", "", "");
				
				if(Catalog.GetString("R") != "R")	
					Update(true, Constants.EncoderTable, "laterality", Catalog.GetString("R"), "R", "", "");
				
				if(Catalog.GetString("L") != "L")	
					Update(true, Constants.EncoderTable, "laterality", Catalog.GetString("L"), "L", "", "");

				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.22", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "1.22";
			}
			if(currentVersion == "1.22") {
				LogB.SQL("Added encoder configuration");
				
				SqliteGeneral.Sqlite.Open();
				SqliteGeneral.SqlitePreferences.Insert ("encoderConfiguration", new EncoderConfiguration().ToStringOutput(EncoderConfiguration.Outputs.SQL)); 
				SqliteGeneral.SqlitePreferences.Update ("databaseVersion", "1.23", true); 
				SqliteGeneral.Sqlite.Close();

				currentVersion = "1.23";
			}

			// ----------------------------------------------
			// IMPORTANT HERE IS DEFINED sqliteOpened == true
			// this is useful to not do more than 50 SQL open close
			// that crashes mac (Linux 100)
			// ----------------------------------------------
			LogB.SQL("Leaving Sqlite opened before DB updates");
			bool sqliteOpened = true;
	
			SqliteGeneral.Sqlite.Open(); //------------------------------------------------


			if(currentVersion == "1.23") {
				LogB.SQL("Delete runISpeedStartArrival and add 4 double contacts configs");

				DeleteFromName(true, Constants.PreferencesTable, "runISpeedStartArrival");
				SqliteGeneral.SqlitePreferences.Insert ("runDoubleContactsMode", Constants.DoubleContact.LAST.ToString()); 
				SqliteGeneral.SqlitePreferences.Insert ("runDoubleContactsMS", "1000");
				SqliteGeneral.SqlitePreferences.Insert ("runIDoubleContactsMode", Constants.DoubleContact.AVERAGE.ToString()); 
				SqliteGeneral.SqlitePreferences.Insert ("runIDoubleContactsMS", "1000");

				currentVersion = updateVersion("1.24");
			}
			if(currentVersion == "1.24") {
				LogB.SQL("Language defaults to (empty string), means detected");
				SqliteGeneral.SqlitePreferences.Update("language", "", true);
				
				currentVersion = updateVersion("1.25");
			}
			if(currentVersion == "1.25") {
				LogB.SQL("Changed Inclinated to Inclined");
				Update(true, Constants.EncoderExerciseTable, "name", "Inclinated plane", "Inclined plane", "", "");
				Update(true, Constants.EncoderExerciseTable, "name", "Inclinated plane BW", "Inclined plane BW", "", "");
				
				currentVersion = updateVersion("1.26");
			}
			if(currentVersion == "1.26") {
				LogB.SQL("Changing runDoubleContactsMS and runIDoubleContactsMS from 1000ms to 300ms");
				SqliteGeneral.SqlitePreferences.Update("runDoubleContactsMS", "300", true);
				SqliteGeneral.SqlitePreferences.Update("runIDoubleContactsMS", "300", true);
				
				currentVersion = updateVersion("1.27");
			}
			if(currentVersion == "1.27") {
				LogB.SQL("Changed encoderAutoSaveCurve BESTMEANPOWER to BEST");
				Update(true, Constants.PreferencesTable, "value", "BESTMEANPOWER", "BEST", "name", "encoderAutoSaveCurve");
				
				currentVersion = updateVersion("1.28");
			}
			if(currentVersion == "1.28") {
				LogB.SQL("Changed reaction time rows have reactionTime as default value");
				Update(true, Constants.ReactionTimeTable, "type", "", "reactionTime", "", "");
				
				currentVersion = updateVersion("1.29");
			}
			if(currentVersion == "1.29") {
				LogB.SQL("Added SIMULATED session");
	
				//add SIMULATED session if doesn't exists. Unique session where tests can be simulated.
				SqliteGeneral.SqliteSession.insertSimulatedSession();

				currentVersion = updateVersion("1.30");
			}
			if(currentVersion == "1.30") {
				LogB.SQL("Insert encoderCaptureCheckFullyExtended and ...Value at preferences");

				SqliteGeneral.SqlitePreferences.Insert ("encoderCaptureCheckFullyExtended", "True");
				SqliteGeneral.SqlitePreferences.Insert ("encoderCaptureCheckFullyExtendedValue", "4");

				currentVersion = updateVersion("1.31");
			}
			if(currentVersion == "1.31") {
				LogB.SQL("encoderCaptureOptionsWin -> preferences");

				SqliteGeneral.SqlitePreferences.Insert ("encoderCaptureTime", "60");
				SqliteGeneral.SqlitePreferences.Insert ("encoderCaptureInactivityEndTime", "3");
				SqliteGeneral.SqlitePreferences.Insert ("encoderCaptureMainVariable", Constants.EncoderVariablesCapture.MeanPower.ToString());
				SqliteGeneral.SqlitePreferences.Insert ("encoderCaptureMinHeightGravitatory", "20");
				SqliteGeneral.SqlitePreferences.Insert ("encoderCaptureMinHeightInertial", "5");
				SqliteGeneral.SqlitePreferences.Insert ("encoderShowStartAndDuration", "False");

				currentVersion = updateVersion("1.32");
			}



			// --- add more updates here
		

			
			// --- end of update, close DB

			LogB.SQL("Closing Sqlite after DB updates");
			sqliteOpened = false;

			SqliteGeneral.Sqlite.Close(); //------------------------------------------------
		}

		//if changes are made here, remember to change also in CreateTables()
		//remember to change also the databaseVersion below
		
		return returnSoftwareIsNew;
	}
	
	private string updateVersion(string newVersion) {
		SqliteGeneral.SqlitePreferences.Update ("databaseVersion", newVersion, true); 
		return newVersion;
	}

	public bool ChangeDjToDJna() {
		string v = SqliteGeneral.SqlitePreferences.Select("databaseVersion");
		LogB.SQL(Convert.ToDouble(Util.ChangeDecimalSeparator(v)).ToString());
		if(Convert.ToDouble(Util.ChangeDecimalSeparator(v)) < Convert.ToDouble(Util.ChangeDecimalSeparator("0.74")))
			return true;
		return false;
	}

	private void addChronopicPortNameIfNotExists() {
		string myPort = SqliteGeneral.SqlitePreferences.Select("chronopicPort");
		if(myPort == "0") {
			//if doesn't exist (for any reason, like old database)
			SqliteGeneral.Sqlite.Open();
			if(UtilAll.IsWindows() || creatingBlankDatabase)
				SqliteGeneral.SqlitePreferences.Insert ("chronopicPort", Constants.ChronopicDefaultPortWindows);
			else
				SqliteGeneral.SqlitePreferences.Insert ("chronopicPort", Constants.ChronopicDefaultPortLinux);
			SqliteGeneral.Sqlite.Close();
			
			LogB.SQL("Added Chronopic port");
		}
	}
	
	public void CreateTables(bool server)
	{
		SqliteGeneral.Sqlite.Open();

		creationTotal = 14;
		creationRate = 1;

		SqliteServer sqliteServerObject = new SqliteServer();
		//user has also an evaluator table with a row (it's row)	
		sqliteServerObject.CreateEvaluatorTable();
		
		if(server) {
			sqliteServerObject.CreatePingTable();
			
			SqliteServerSession sqliteSessionObject = new SqliteServerSession();
			sqliteSessionObject.createTable(Constants.SessionTable);
		} else {
			SqliteSession sqliteSessionObject = new SqliteSession();
			sqliteSessionObject.createTable(Constants.SessionTable);
			
			//add SIMULATED session if doesn't exists. Unique session where tests can be simulated.
			SqliteGeneral.SqliteSession.insertSimulatedSession();
			
			SqliteGeneral.SqlitePersonSessionNotUpload.CreateTable();
			creationRate ++;
		}
		

		SqlitePerson sqlitePersonObject = new SqlitePerson();
		sqlitePersonObject.createTable(Constants.PersonTable);

		//graphLinkTable
		SqliteGeneral.SqliteEvent.createGraphLinkTable();
		creationRate ++;
		
		//jumps
		SqliteJump sqliteJumpObject = new SqliteJump();
		SqliteJumpRj sqliteJumpRjObject = new SqliteJumpRj();
		sqliteJumpObject.createTable(Constants.JumpTable);
		sqliteJumpRjObject.createTable(Constants.JumpRjTable);
		sqliteJumpRjObject.createTable(Constants.TempJumpRjTable);

		//jump Types
		creationRate ++;
		SqliteGeneral.SqliteJumpType.createTableJumpType();
		SqliteGeneral.SqliteJumpType.createTableJumpRjType();
		SqliteGeneral.SqliteJumpType.initializeTableJumpType();
		SqliteGeneral.SqliteJumpType.initializeTableJumpRjType();
		
		//runs
		creationRate ++;
		SqliteRun sqliteRunObject = new SqliteRun();
		SqliteRunInterval sqliteRunIntervalObject = new SqliteRunInterval();
		sqliteRunObject.createTable(Constants.RunTable);
		sqliteRunIntervalObject.createTable(Constants.RunIntervalTable);
		sqliteRunIntervalObject.createTable(Constants.TempRunIntervalTable);
		
		//run Types
		creationRate ++;
		SqliteRunType sqliteRunTypeObject = new SqliteRunType();
		sqliteRunTypeObject.createTable(Constants.RunTypeTable);
		SqliteGeneral.SqliteRunType.initializeTable();

		SqliteRunIntervalType sqliteRunIntervalTypeObject = new SqliteRunIntervalType();
		sqliteRunIntervalTypeObject.createTable(Constants.RunIntervalTypeTable);
		SqliteGeneral.SqliteRunIntervalType.initializeTable();
		
		//reactionTimes
		creationRate ++;
		SqliteReactionTime sqliteReactionTimeObject = new SqliteReactionTime();
		sqliteReactionTimeObject.createTable(Constants.ReactionTimeTable);
		
		//pulses and pulseTypes
		creationRate ++;
		SqlitePulse sqlitePulseObject = new SqlitePulse();
		sqlitePulseObject.createTable(Constants.PulseTable);
		SqliteGeneral.SqlitePulseType.createTablePulseType();
		SqliteGeneral.SqlitePulseType.initializeTablePulseType();
		
		//multiChronopic tests		
		creationRate ++;
		SqliteMultiChronopic sqliteMultiChronopicObject = new SqliteMultiChronopic();
		sqliteMultiChronopicObject.createTable(Constants.MultiChronopicTable);
	
		//encoder	
		creationRate ++;
		SqliteGeneral.SqliteEncoder.createTableEncoder();
		SqliteGeneral.SqliteEncoder.createTableEncoderSignalCurve();
		SqliteGeneral.SqliteEncoder.createTableEncoderExercise();
		SqliteGeneral.SqliteEncoder.initializeTableEncoderExercise();
		SqliteGeneral.SqliteEncoder.createTable1RM();

		//sports
		creationRate ++;
		SqliteGeneral.SqliteSport.createTable();
		SqliteGeneral.SqliteSport.initialize();
		SqliteGeneral.SqliteSpeciallity.createTable();
		SqliteGeneral.SqliteSpeciallity.initialize();
		SqliteGeneral.SqliteSpeciallity.InsertUndefined(true);
				
		creationRate ++;
		SqlitePersonSession sqlitePersonSessionObject = new SqlitePersonSession();
		sqlitePersonSessionObject.createTable(Constants.PersonSessionTable);
		
		creationRate ++;
		SqliteGeneral.SqlitePreferences.createTable();
		SqliteGeneral.SqlitePreferences.initializeTable(lastChronojumpDatabaseVersion, creatingBlankDatabase);
		
		creationRate ++;
		SqliteGeneral.SqliteCountry.createTable();
		SqliteGeneral.SqliteCountry.initialize();
				
		SqliteGeneral.SqliteExecuteAuto.createTableExecuteAuto();
		SqliteGeneral.SqliteExecuteAuto.addChronojumpProfileAndBilateral();
		
		//changes [from - to - desc]
		//1.31 - 1.32 Converted DB to 1.32 encoderCaptureOptionsWin -> preferences
		//1.30 - 1.31 Converted DB to 1.31 Insert encoderCaptureCheckFullyExtended and ...Value at preferences
		//1.29 - 1.30 Converted DB to 1.30 Added SIMULATED session
		//1.28 - 1.29 Converted DB to 1.29 Changed reaction time rows have reactionTime as default value
		//1.27 - 1.28 Converted DB to 1.28 Changed encoderAutoSaveCurve BESTMEANPOWER to BEST
		//1.26 - 1.27 Converted DB to 1.27 Changing runDoubleContactsMS and runIDoubleContactsMS from 1000ms to 300ms
		//1.25 - 1.26 Converted DB to 1.26 Changed Inclinated to Inclined
		//1.24 - 1.25 Converted DB to 1.25 Language defaults to (empty string), means detected
		//1.23 - 1.24 Converted DB to 1.24 Delete runISpeedStartArrival and add 4 double contacts configs
		//1.22 - 1.23 Converted DB to 1.23 Added encoder configuration
		//1.21 - 1.22 Converted DB to 1.22 Encoder laterality in english again
		//1.20 - 1.21 Converted DB to 1.21 Fixing loosing of encoder videoURL after recalculate
		//1.19 - 1.20 Converted DB to 1.20 Preferences: added user email
		//1.18 - 1.19 Converted DB to 1.19 Preferences deleted showHeight, added showStiffness
		//1.17 - 1.18 Converted DB to 1.18 deleted Negative runInterval runs (bug from last version)
		//1.16 - 1.17 Converted DB to 1.17 Deleted Max jump (we already have "Free")
		//1.15 - 1.16 Converted DB to 1.16 Cyprus moved to Europe
		//1.14 - 1.15 Converted DB to 1.15 added Chronojump profile and bilateral profile
		//1.13 - 1.14 Converted DB to 1.14 slCMJ -> slCMJleft, slCMJright
		//1.12 - 1.13 Converted DB to 1.13 Added ExecuteAuto table
		//1.11 - 1.12 Converted DB to 1.12 URLs from absolute to relative
		//1.10 - 1.11 Converted DB to 1.11 Added option on autosave curves on capture (all/bestmeanpower/none)
		//1.09 - 1.10 Converted DB to 1.10 Added RSA RAST on runType
		//1.08 - 1.09 Converted DB to 1.09 Added option on preferences to useHeightsOnJumpIndexes (default) or not
		//1.07 - 1.08 Converted DB to 1.08 Added translate statistics graph option to preferences
		//1.06 - 1.07 Converted DB to 1.07 Added jump_dj_a.png
		//1.05 - 1.06 Converted DB to 1.06 Curves are now linked to signals
		//1.04 - 1.05 Converted DB to 1.05 Removed inertial curves, because sign was not checked on 1.04 when saving curves
		//1.03 - 1.04 Converted DB to 1.04 Encoder table improved
		//1.02 - 1.03 Converted DB to 1.03 Updated encoder exercise, angle is now on encoder configuration
		//1.01 - 1.02 Converted DB to 1.02 Added Agility Tests: Agility-T-Test, Agility-3L3R
		//1.00 - 1.01 Converted DB to 1.01 Added export to CSV configuration on preferences
		//0.99 - 1.00 Converted DB to 1.00 Encoder added Free and Inclined Exercises
		//0.98 - 0.99 Converted DB to 0.99 Encoder table improved 
		//0.97 - 0.98 Converted DB to 0.98 Fixed encoder laterality
		//0.96 - 0.97 Converted DB to 0.97 Added inertialmomentum in preferences
		//0.95 - 0.96 Converted DB to 0.96 Encoder signal future3 three modes
		//0.94 - 0.95 Converted DB to 0.95 Added encoder1RMMethod
		//0.93 - 0.94 Converted DB to 0.94 Added encoder1RM table
		//0.92 - 0.93 Converted DB to 0.93 Added speed1RM on encoder exercise
		//0.91 - 0.92 Converted DB to 0.92 Added videoDevice to preferences
		//0.90 - 0.91 Converted DB to 0.91 Encoder Squat 75% -> 100%
		//0.89 - 0.90 Converted DB to 0.90 Preferences added propulsive and encoder smooth
		//0.88 - 0.89 Converted DB to 0.89 Added encoder exercise: Free
		//0.87 - 0.88 Converted DB to 0.88 Deleted fake RSA test and added known RSA tests
		//0.86 - 0.87 Converted DB to 0.87 Added run speed start preferences on sqlite
		//0.85 - 0.86 Converted DB to 0.86 videoOn: TRUE
		//0.84 - 0.85 Converted DB to 0.85 Added slCMJ jump 
		//0.83 - 0.84 Converted DB to 0.84 Added first RSA test 
		//0.82 - 0.83 Converted DB to 0.83 Created encoder table
		//0.81 - 0.82 Converted DB to 0.82 Added videoOn 
		//0.80 - 0.81 Converted DB to 0.81 Added tempRunInterval initial speed
		//0.79 - 0.80 Converted DB to 0.80 Added run and runInterval initial speed (if not done in 0.56 conversion)
		//0.78 - 0.79 Converted DB to 0.79 (Added multimediaStorage structure id)
		//0.77 - 0.78 Converted DB to 0.78 (Added machineID to preferences, takeOffWeight has no weight in db conversions since 0.66)
		//0.76 - 0.77 Converted DB to 0.77 (person77, personSession77)
		//0.75 - 0.76 Converted DB to 0.76 (jump & jumpRj falls as double)
		//0.74 - 0.75 Converted DB to 0.75 (person, and personSessionWeight have height and weight as double)
		//0.73 - 0.74 Converted DB to 0.74 (All DJ converted to DJna)
		//0.72 - 0.73 Converted DB to 0.73 (deleted orphaned persons (in person table but not in personSessionWeight table))
		//0.71 - 0.72 dates to YYYY-MM-DD
		//0.70 - 0.71 created personNotUploadTable on client
		//0.69 - 0.70 added showPower to preferences
		//0.68 - 0.69 added Gesell-DBT test
		//0.67 - 0.68 added multiChronopic tests table
		//0.66 - 0.67 added TakeOff jumps 
		//0.65 - 0.66 added done nothing 
		//0.64 - 0.65 added Sevaluator on client
		//0.63 - 0.64 added margaria test
		//0.62 - 0.63 added 'versionAvailable' to preferences
		//0.61 - 0.62 added hexagon (jumpRj test)
		//0.60 - 0.61 added RunIntervalType distancesString (now we van have interval tests with different distances of tracks). Added MTGUG
		//0.59 - 0.60 added volumeOn and evaluatorServerID to preferences. Session has now serverUniqueID. Simulated now are -1, because 0 is real and positive is serverUniqueID
		//0.58 - 0.59 Added 'showAngle' to preferences, changed angle on jump to double
		//0.57 - 0.58 Countries without kingdom or republic (except when needed)
		//0.56 - 0.57 Added simulated column to each event table on client. person: race, country, serverID. Convert to sport related done here if needed. Added also run and runInterval initial speed);
		//0.55 - 0.56 Added session default sport stuff into session table
		//0.54 - 0.55 Added undefined to speciallity table
		//0.53 - 0.54 created sport tables. Added sport data, speciallity and level of practice to person table
		//0.52 - 0.53 added table weightSession, moved person weight data to weightSession table for each session that has performed
		//0.51 - 0.52 added graphLinks for cmj_l and abk_l. Fixed CMJ_l name
		//0.50 - 0.51 added graphLinks for run simple and interval
		//0.49 - 0.50: changed SJ+ to SJl, same for CMJ+ and ABK+, added jump and jumpRj graph links
		//0.48 - 0.49: added graphLinkTable, added rocket jump and 5 agility tests: (20Yard, 505, Illinois, Shuttle-Run & ZigZag). Added graphs pof the 5 agility tests
		//0.47 - 0.48: added tempJumpReactive and tempRunInterval tables
		//0.46 - 0.47: added reactionTime table
		//0.45 - 0.46: added "Free" jump type
		//0.44 - 0.45: added allowFinishRjAfterTime
		//0.43 - 0.44: added showQIndex and showDjIndex
		//0.42 - 0.43: added 'free' pulseType & language preference
		//0.41 - 0.42: added pulse and pulseType tables
		//0.4 - 0.41: jump, jumpRj weight is double (always a percent)
		

		SqliteGeneral.Sqlite.Close();
		creationRate ++;
	}

	public bool Exists(bool dbconOpened, string tableName, string findName)
	{
		if(!dbconOpened)
			SqliteGeneral.Sqlite.Open();

		dbcmd.CommandText = "SELECT uniqueID FROM " + tableName + 
			" WHERE LOWER(name) == LOWER(\"" + findName + "\")" ;
		LogB.SQL(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		bool exists = new bool();
		exists = false;
		
		if (reader.Read()) {
			exists = true;
		}
		LogB.SQL(string.Format("name exists = {0}", exists.ToString()));

		reader.Close();
		if(!dbconOpened)
			SqliteGeneral.Sqlite.Close();

		return exists;
	}

	public string SQLBuildQueryString (string tableName, string test, string variable,
			int sex, string ageInterval,
			int countryID, int sportID, int speciallityID, int levelID, int evaluatorID)
	{
		string tp = Constants.PersonTable;
		string tps = Constants.PersonSessionTable;

		string strSelect = "SELECT COUNT(" + variable + "), AVG(" + variable + ")";
		string strFrom   = " FROM " + tableName;
		string strWhere  = " WHERE " + tableName + ".type = \"" + test + "\"";

		string strSex = "";
		if(sex == Constants.MaleID) 
			strSex = " AND " + tp + ".sex == \"" + Constants.M + "\"";
		else if (sex == Constants.FemaleID) 
			strSex = " AND " + tp + ".sex == \"" + Constants.F + "\"";

		string strAge = "";
		string strEval = "";
		string strSession = "";
		if(ageInterval != "" || evaluatorID != Constants.AnyID) {
			strFrom += ", session";
			if(ageInterval != "") {
				string [] strFull = ageInterval.Split(new char[] {':'});
				strAge = " AND (julianday(session.date) - julianday(" + tp + ".dateBorn))/365.25 " + 
					strFull[0] + " " + strFull[1];
				if(strFull.Length == 4)
					strAge += " AND (julianday(session.date) - julianday(" + tp + ".dateBorn))/365.25 " + 
						strFull[2] + " " + strFull[3];
			}
			if(evaluatorID != Constants.AnyID) 
				strEval = " AND session.evaluatorID == " + evaluatorID;
			
			strSession = " AND " + tableName + ".sessionID = session.uniqueID";
		}

		string strCountry = "";
		if(countryID != Constants.CountryUndefinedID) 
			strCountry = " AND " + tp + ".countryID == " + countryID;

		string strSport = "";
		if(sportID != Constants.SportUndefinedID) 
			strSport = " AND " + tps + ".sportID == " + sportID;

		string strSpeciallity = "";
		if(speciallityID != Constants.SpeciallityUndefinedID) 
			strSpeciallity = " AND " + tps + ".speciallityID == " + speciallityID;
		
		string strLevel = "";
		if(levelID != Constants.LevelUndefinedID) 
			strLevel = " AND " + tps + ".practice == " + levelID;

		string strLast = "";
		if(strSex.Length > 0 || strAge.Length > 0 || 
				strCountry.Length > 0 || strSport.Length > 0 || strSpeciallity.Length > 0 || strLevel.Length > 0) {
			strFrom += ", " + tp + ", " + tps;
			strLast = " AND " + tableName + ".personID == " + tp + ".uniqueID" +
				" AND " + tp + ".uniqueID == " + tps + ".personID";
		}	
		return strSelect + strFrom + strWhere + strSex + strAge
			+ strCountry + strSport + strSpeciallity + strLevel + strEval + strSession + strLast;
	}


	/* 
	 * temp data stuff
	 */
	public int TempDataExists(string tableName)
	{
		//tableName can be tempJumpRj or tempRunInterval
		
		SqliteGeneral.Sqlite.Open();
		dbcmd.CommandText = "SELECT MAX(uniqueID) FROM " + tableName;
		LogB.SQL(dbcmd.CommandText.ToString());
		
		//SqliteDataReader reader;
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		int exists = 0;
		
		if (reader.Read()) {
			//sqlite3 returns a line (without data) if there's no data. Converting to int the line makes chronojump crash
			try {
				exists = Convert.ToInt32(reader[0]);
			} catch { exists = 0; }
		}
		LogB.SQL(string.Format("exists = {0}", exists.ToString()));
		reader.Close();
		SqliteGeneral.Sqlite.Close();

		return exists;
	}

	public void DeleteTempEvents(string tableName)
	{
		//tableName can be tempJumpRj or tempRunInterval

		SqliteGeneral.Sqlite.Open();
		//dbcmd.CommandText = "Delete FROM tempJumpRj";
		dbcmd.CommandText = "DELETE FROM " + tableName;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		SqliteGeneral.Sqlite.Close();
	}

	public void dropTable(string tableName) {
		dbcmd.CommandText = "DROP TABLE " + tableName;
		dbcmd.ExecuteNonQuery();
	}
				
	protected void convertPersonAndPersonSessionTo77() {
		//create person77
		SqlitePerson sqlitePersonObject = new SqlitePerson();
		sqlitePersonObject.createTable(Constants.PersonTable);
		
		//create personSession77
		SqlitePersonSession sqlitePersonSessionObject = new SqlitePersonSession();
		sqlitePersonSessionObject.createTable(Constants.PersonSessionTable);

		//select all personOld data
		SqlitePersonOld sqlitePersonOldObject = new SqlitePersonOld();
		ArrayList personsOld = sqlitePersonOldObject.SelectAllPersons();

		conversionRateTotal = personsOld.Count;
		conversionRate = 1;
		foreach (PersonOld pOld in personsOld) {
			Person p = new Person(
				       pOld.UniqueID,
				       pOld.Name,
				       pOld.Sex,
				       pOld.DateBorn,
				       pOld.Race,
				       pOld.CountryID,
				       pOld.Description,
				       pOld.ServerUniqueID
				       );
			p.InsertAtDB(true, Constants.PersonTable);
		
			//select all personSessionOld data of this person
			SqlitePersonSessionOld sqlitePersonSessionOldObject = new SqlitePersonSessionOld();
			ArrayList personSessionsOld = sqlitePersonSessionOldObject.SelectAllPersonSessionsOfAPerson(p.UniqueID);
			conversionSubRateTotal = personSessionsOld.Count;
			conversionSubRate = 1;
			foreach (PersonSessionOld psOld in personSessionsOld) {
				PersonSession ps = new PersonSession(
						psOld.UniqueID,
						psOld.PersonID,
						psOld.SessionID,
						pOld.Height,
						psOld.Weight,
						pOld.SportID,
						pOld.SpeciallityID,
						pOld.Practice,
						"" 		//comments
						);
				ps.InsertAtDB(true, Constants.PersonSessionTable);
				conversionSubRate ++;
			}
			conversionRate ++;
		}
		
		//drop old tables
		SqliteGeneral.Sqlite.dropTable(Constants.PersonOldTable);
		SqliteGeneral.Sqlite.dropTable(Constants.PersonSessionOldWeightTable);

	}



	//to convert to sqlite 0.72
	protected internal void datesToYYYYMMDD()
	{
		conversionRateTotal = 4;
		conversionRate = 1;
		conversionSubRateTotal = -1;

		/* person table */

		dbcmd.CommandText = "SELECT uniqueID, dateBorn FROM person";
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		ArrayList myArray = new ArrayList(1);
		while(reader.Read()) 
			myArray.Add (reader[0].ToString() + ":" + reader[1].ToString());

		reader.Close();

		/*
		   convert dates from D/M/Y
		   to YYYY-MM-DD
		   */
		conversionSubRateTotal = myArray.Count;
		conversionSubRate = 1;
		foreach(string str in myArray) {
			string [] id_date = str.Split(new char[] {':'});
			DateTime dt = UtilDate.FromSql(id_date[1]);
			dbcmd.CommandText = "UPDATE person set dateBorn = \"" + UtilDate.ToSql(dt) +
				"\" WHERE uniqueID = " + id_date[0];
			LogB.SQL(dbcmd.CommandText.ToString());
			dbcmd.ExecuteNonQuery();
			conversionSubRate ++;
		}

		conversionRate ++;

		/* session table */

		dbcmd.CommandText = "SELECT uniqueID, date FROM session";
		reader = dbcmd.ExecuteReader();
		myArray = new ArrayList(1);
		while(reader.Read()) 
			myArray.Add (reader[0].ToString() + ":" + reader[1].ToString());

		reader.Close();

		/*
		   convert dates from D/M/Y
		   to YYYY-MM-DD
		   */
		conversionSubRateTotal = myArray.Count;
		conversionSubRate = 1;
		foreach(string str in myArray) {
			string [] id_date = str.Split(new char[] {':'});
			DateTime dt = UtilDate.FromSql(id_date[1]);
			dbcmd.CommandText = "UPDATE session set date = \"" + UtilDate.ToSql(dt) +
				"\" WHERE uniqueID = " + id_date[0];
			LogB.SQL(dbcmd.CommandText.ToString());
			dbcmd.ExecuteNonQuery();
			conversionSubRate ++;
		}

		conversionRate ++;
		
		/* SEvaluator table */

		dbcmd.CommandText = "SELECT uniqueID, dateBorn FROM SEvaluator";
		reader = dbcmd.ExecuteReader();
		myArray = new ArrayList(1);
		while(reader.Read()) 
			myArray.Add (reader[0].ToString() + ":" + reader[1].ToString());

		reader.Close();

		/*
		   convert dates from D/M/Y
		   to YYYY-MM-DD
		   */
		conversionSubRateTotal = myArray.Count;
		conversionSubRate = 1;
		foreach(string str in myArray) {
			string [] id_date = str.Split(new char[] {':'});
			DateTime dt = UtilDate.FromSql(id_date[1]);
			dbcmd.CommandText = "UPDATE SEvaluator set dateBorn = \"" + UtilDate.ToSql(dt) +
				"\" WHERE uniqueID = " + id_date[0];
			LogB.SQL(dbcmd.CommandText.ToString());
			dbcmd.ExecuteNonQuery();
			conversionSubRate ++;
		}
		conversionRate ++;
	}

	//used to delete persons (if needed) when a session is deleted. See SqliteGeneral.SqliteSession.DeleteAllStuff
	protected internal void deleteOrphanedPersons()
	{
		dbcmd.CommandText = "SELECT uniqueID FROM " + Constants.PersonTable;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		ArrayList myArray = new ArrayList(1);

		while(reader.Read())
			myArray.Add (Convert.ToInt32(reader[0]));
		reader.Close();

		foreach(int personID in myArray) {
			//if person is not in other sessions, delete it from DB
			if(! SqliteGeneral.SqlitePersonSession.PersonExistsInPS(true, personID))
				Delete(true, Constants.PersonTable, personID);
		}
	}
				
	//used to delete persons (if needed) when a session is deleted. See SqliteGeneral.SqliteSession.DeleteAllStuff
	//also used to convert to sqlite 0.73
	//this is old method (before .77), now use above method
	protected internal void deleteOrphanedPersonsOld()
	{
		dbcmd.CommandText = "SELECT uniqueID FROM " + Constants.PersonOldTable;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		ArrayList myArray = new ArrayList(1);

		while(reader.Read())
			myArray.Add (Convert.ToInt32(reader[0]));
		reader.Close();

		foreach(int personID in myArray) {
			//if person is not in other sessions, delete it from DB
			if(! SqliteGeneral.SqlitePersonSessionOld.PersonExistsInPS(personID))
				SqliteGeneral.SqlitePersonOld.Delete(personID);
		}
	}
				
	//used to convert to sqlite 0.74
	protected internal void convertDJInDJna()
	{
		//Dja exists in DB? (user defined)
		if(Exists(false, Constants.JumpTypeTable, "DJa")) {
			string [] names = { "DJa-user", "DJa-user2", "DJa-user3", "DJa-user4" }; //sorry, we cannot check all the names in the world, ok, yes, i know, we can, but it's ok like this
			bool success = false;
			foreach(string name in names) {
				if(!Exists(false, Constants.JumpTypeTable, name)) {
					success = true;
					dbcmd.CommandText = "UPDATE jump SET type = \"" + name + "\" WHERE type == \"DJa\"";
					LogB.SQL(dbcmd.CommandText.ToString());
					dbcmd.ExecuteNonQuery();
				}
				if(success) 
					break;
			}
		}
		
		//Djna exists in DB? (user defined)
		if(Exists(false, Constants.JumpTypeTable, "DJna")) {
			string [] names = { "DJna-user", "DJna-user2", "DJna-user3", "DJna-user4" }; //sorry, we cannot check all the names in the world, ok, yes, i know, we can, but it's ok like this
			bool success = false;
			foreach(string name in names) {
				if(!Exists(false, Constants.JumpTypeTable, name)) {
					success = true;
					dbcmd.CommandText = "UPDATE jump SET type = \"" + name + "\" WHERE type == \"DJna\"";
					LogB.SQL(dbcmd.CommandText.ToString());
					dbcmd.ExecuteNonQuery();
				}
				if(success) 
					break;
			}
		}

		//no opened before because Exists is for closed dbcon
		SqliteGeneral.Sqlite.Open();

		//create new jump types
		SqliteGeneral.SqliteJumpType.JumpTypeInsert ("DJa:0:0:DJ jump using arms", true); 
		SqliteGeneral.SqliteJumpType.JumpTypeInsert ("DJna:0:0:DJ jump without using arms", true); 
		
		//add auto-converted on description
		dbcmd.CommandText = "UPDATE jump SET description = description || \" Auto-converted from DJ\" WHERE type == \"DJ\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		//conversion
		dbcmd.CommandText = "UPDATE jump SET type = \"DJna\" WHERE type == \"DJ\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		//delete DJ
		SqliteGeneral.SqliteJumpType.Delete(Constants.JumpTypeTable, "DJ", true);
	}



	/*
	 * The problem of this method is that uses class constructors: person, jump, ...
	 * and if the sqlite version is updated from a really old version
	 * maybe the object has to be converted from really older class to old, and then to new class (two conversions)
	 * and this can have problems in the class construction
	 * The best seem to have a boolean that indicates if certain conversion has done before
	 * (see bool runAndRunIntervalInitialSpeedAdded)
	 */
	protected internal void convertTables(Sqlite sqliteObject, string tableName, int columnsBefore, ArrayList columnsToAdd, bool putDescriptionInMiddle) 
	{
		conversionSubRate = 1;
		conversionSubRateTotal = -1; //unknown yet


		//2st create convert temp table
		sqliteObject.createTable(Constants.ConvertTempTable);

		//2nd copy all data from desired table to temp table (in event tables, adding the simulated column)
		ArrayList myArray = new ArrayList(2);
		dbcmd.CommandText = "SELECT * " + 
			"FROM " + tableName + " ORDER BY uniqueID"; 
		LogB.SQL(dbcmd.CommandText.ToString());
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		while(reader.Read()) {
			string [] myReaderStr = new String[columnsBefore + columnsToAdd.Count];
			int i;
			for (i=0; i < columnsBefore; i ++) 
				myReaderStr[i] = reader[i].ToString();
		
			foreach (string myStr in columnsToAdd) 
				myReaderStr[i++] = myStr;

			if (putDescriptionInMiddle) {
				//string [] strFull = changePos.Split(new char[] {':'});
				//int row1 = Convert.ToInt32(strFull[0]);
				//int row2 = Convert.ToInt32(strFull[1]);
				string desc = myReaderStr[6];
				myReaderStr[6] = myReaderStr[7];
				myReaderStr[7] = myReaderStr[8];
				myReaderStr[8] = myReaderStr[9];
				myReaderStr[9] = desc;
			}

			if(tableName == Constants.PersonOldTable) {	
				PersonOld myPerson =  new PersonOld(myReaderStr);
				myArray.Add(myPerson);
			} else if(tableName == Constants.SessionTable) {	
				Session mySession = new Session(myReaderStr);
				myArray.Add(mySession);
			} else if(tableName == Constants.RunIntervalTypeTable) {	
				RunType myType = new RunType(myReaderStr, true); //interval
				myArray.Add(myType);
			} else if(tableName == Constants.PersonSessionOldWeightTable) {	
				PersonSessionOld myPS = new PersonSessionOld(myReaderStr);
				myArray.Add(myPS);
			} else {
				Event myEvent =  new Event();	
				switch (tableName) {
					case Constants.JumpTable:
						myEvent = new Jump(myReaderStr);
						break;
					case Constants.JumpRjTable:
						myEvent = new JumpRj(myReaderStr);
						break;
					case Constants.RunTable:
						myEvent = new Run(myReaderStr);
						break;
					case Constants.RunIntervalTable:
						myEvent = new RunInterval(myReaderStr);
						break;
					case Constants.ReactionTimeTable:
						myEvent = new ReactionTime(myReaderStr);
						break;
					case Constants.PulseTable:
						myEvent = new Pulse(myReaderStr);
						break;
				}
				myArray.Add(myEvent);
			}
		}
		reader.Close();

LogB.SQL("1" + tableName);

		conversionSubRateTotal = myArray.Count * 2;

		if(tableName == Constants.PersonOldTable) {	
			foreach (PersonOld myPerson in myArray) {
				myPerson.InsertAtDB(true, Constants.ConvertTempTable);
				conversionSubRate ++;
			}
		} else if(tableName == Constants.SessionTable) {	
			foreach (Session mySession in myArray) {
				mySession.InsertAtDB(true, Constants.ConvertTempTable);
				conversionSubRate ++;
			}
		} else if(tableName == Constants.RunIntervalTypeTable) {	
			foreach (RunType type in myArray) {
				type.InsertAtDB(true, Constants.ConvertTempTable, true); //last true is for interval
				conversionSubRate ++;
			}
		} else if(tableName == Constants.PersonSessionOldWeightTable) {	
			foreach (PersonSessionOld ps in myArray) {
				ps.InsertAtDB(true, Constants.ConvertTempTable);
				conversionSubRate ++;
			}
		} else {
			foreach (Event myEvent in myArray) {
				myEvent.InsertAtDB(true, Constants.ConvertTempTable);
				conversionSubRate ++;
			}
		}
		
LogB.SQL("2" + tableName);
		//3rd drop desired table
		SqliteGeneral.Sqlite.dropTable(tableName);

LogB.SQL("3" + tableName);
		//4d create desired table (now with new columns)
		sqliteObject.createTable(tableName);


LogB.SQL("4" + tableName);

		//5th insert data in desired table
		if(tableName == Constants.PersonOldTable) {	
			foreach (PersonOld myPerson in myArray) {
				myPerson.InsertAtDB(true, tableName);
				conversionSubRate ++;
			}
		} else if(tableName == Constants.SessionTable) {	
			foreach (Session mySession in myArray) {
				mySession.InsertAtDB(true, tableName);
				conversionSubRate ++;
			}
		} else if(tableName == Constants.RunIntervalTypeTable) {	
			foreach (RunType type in myArray) {
				type.InsertAtDB(true, tableName, true); //last true is for interval
				conversionSubRate ++;
			}
		} else if(tableName == Constants.PersonSessionOldWeightTable) {	
			foreach (PersonSessionOld ps in myArray) {
				ps.InsertAtDB(true, tableName);
				conversionSubRate ++;
			}
		} else {
			foreach (Event myEvent in myArray) {
				myEvent.InsertAtDB(true, tableName);
				conversionSubRate ++;
			}
		}

LogB.SQL("5" + tableName);
		//6th drop temp table
		SqliteGeneral.Sqlite.dropTable(Constants.ConvertTempTable);
	}
	
	/*
	 * useful to do a conversion from an int to a double
	 * used on jump.angle
	 * we done on sqlite/jump.cs:
	 * on createTable change "angle INT" to "angle FLOAT"
	 * then call this alterTableColumn
	 *
	 * but CAUTION: doing this, all the float data is converted to .0
	 * eg: 27.35 will be 27.0
	 *     -1 will be -1.0
	 *
	 * if we don't use this, and we have created a column as int, and introduce floats or doubles, 
	 * we can insert ok the float or doubles, but on select we will have ints
	 */
	protected internal void alterTableColumn(Sqlite sqliteObject, string tableName, int columns) 
	{
		conversionSubRate = 1;
		conversionSubRateTotal = -1; //unknown yet

		//2st create convert temp table
		sqliteObject.createTable(Constants.ConvertTempTable);

		//2nd copy all data from desired table to temp table adding the simulated column
		ArrayList myArray = new ArrayList(2);
		dbcmd.CommandText = "SELECT * " + 
			"FROM " + tableName + " ORDER BY uniqueID"; 
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		LogB.SQL(dbcmd.CommandText.ToString());

		while(reader.Read()) {
			string [] myReaderStr = new String[columns];
			for (int i=0; i < columns; i ++) 
				myReaderStr[i] = reader[i].ToString();
		
			Event myEvent =  new Event();	
			switch (tableName) {
				case Constants.JumpTable:
					myEvent = new Jump(myReaderStr);
					break;
				case Constants.JumpRjTable:
					myEvent = new JumpRj(myReaderStr);
					break;
				case Constants.RunTable:
					myEvent = new Run(myReaderStr);
					break;
				case Constants.RunIntervalTable:
					myEvent = new RunInterval(myReaderStr);
					break;
				case Constants.ReactionTimeTable:
					myEvent = new ReactionTime(myReaderStr);
					break;
				case Constants.PulseTable:
					myEvent = new Pulse(myReaderStr);
					break;
			}
			myArray.Add(myEvent);
		}
		reader.Close();

		conversionSubRateTotal = myArray.Count * 2;

		foreach (Event myEvent in myArray) {
			myEvent.InsertAtDB(true, Constants.ConvertTempTable);
			conversionSubRate ++;
		}

		//3rd drop desired table
		SqliteGeneral.Sqlite.dropTable(tableName);

		//4d create desired table (now with new columns)
		sqliteObject.createTable(tableName);

		//5th insert data in desired table
		foreach (Event myEvent in myArray) {
			myEvent.InsertAtDB(true, tableName);
			conversionSubRate ++;
		}

		//6th drop temp table
		SqliteGeneral.Sqlite.dropTable(Constants.ConvertTempTable);
	}

	protected string [] DataReaderToStringArray (SqliteDataReader reader, int columns) {
		string [] myReaderStr = new String[columns];
		for (int i=0; i < columns; i ++)
			myReaderStr[i] = reader[i].ToString();
		return myReaderStr;
	}

	protected IDNameList fillIDNameList(string selectStr) 
	{
		//select personID and jump type 'SJ' mean
		dbcmd.CommandText = selectStr;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader = dbcmd.ExecuteReader();

		IDNameList list = new IDNameList();
		while(reader.Read()) {
			IDName idname = new IDName( Convert.ToInt32(reader[0].ToString()),
						reader[1].ToString());
			LogB.SQL(idname.ToString());
			
			list.Add(new IDName( Convert.ToInt32(reader[0].ToString()),
						reader[1].ToString() ));
		}
		reader.Close();
		return list;
	}

	protected IDDoubleList fillIDDoubleList(string selectStr) 
	{
		//select personID and jump type 'SJ' mean
		dbcmd.CommandText = selectStr;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader = dbcmd.ExecuteReader();

		IDDoubleList list = new IDDoubleList();
		while(reader.Read()) {
			list.Add(new IDDouble( Convert.ToInt32(reader[0].ToString()),
						Convert.ToDouble(Util.ChangeDecimalSeparator(reader[1].ToString())) ));
		}
		reader.Close();
		return list;
	}


	/* methods for different classes */
	
	protected double selectDouble (string sqlSelect) 
	{
		dbcmd.CommandText = sqlSelect;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		double result = 0;
		if(reader.Read())
			result = Convert.ToDouble(Util.ChangeDecimalSeparator(reader[0].ToString()));

		reader.Close();
		
		return result;
	}


	public int Max (string tableName, string column, bool dbconOpened)
	{
		if(!dbconOpened)
			SqliteGeneral.Sqlite.Open();

		dbcmd.CommandText = "SELECT MAX(" + column + ") FROM " + tableName ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		int myReturn = 0;
		//caution because it there are no values, a whiteline or NULL can be returned and can fail in this Convert.ToInt32
		if(reader.Read()) 
			myReturn = Convert.ToInt32(reader[0].ToString());
		reader.Close();

		if(!dbconOpened)
			SqliteGeneral.Sqlite.Close();
		return myReturn;
	}

	public int Count (string tableName, bool dbconOpened)
	{
		if(!dbconOpened)
			SqliteGeneral.Sqlite.Open();

		dbcmd.CommandText = "SELECT COUNT(*) FROM " + tableName ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		int myReturn = 0;
		if(reader.Read()) 
			myReturn = Convert.ToInt32(reader[0].ToString());
		reader.Close();

		if(!dbconOpened)
			SqliteGeneral.Sqlite.Close();
		return myReturn;
	}

	public int CountCondition (string tableName, bool dbconOpened, string condition, string operand, string myValue) {
		if(!dbconOpened)
			SqliteGeneral.Sqlite.Open();

		dbcmd.CommandText = "SELECT COUNT(*) FROM " + tableName +
			" WHERE " + condition + " " + operand + " " + myValue;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		int myReturn = 0;
		if(reader.Read()) 
			myReturn = Convert.ToInt32(reader[0].ToString());
		reader.Close();

		if(!dbconOpened)
			SqliteGeneral.Sqlite.Close();
		return myReturn;
	}

	//if we want to use the condition2 but not the searchValue, leave this as ""
	public void Update(
			bool dbconOpened, string tableName, string columnName, 
			string searchValue, string newValue, 
			string columnNameCondition2, string searchValueCondition2)
	{
		if( ! dbconOpened)
			SqliteGeneral.Sqlite.Open();
		
		bool whereDone = false;
		string cond1 = "";
		if(searchValue != "") {
			cond1 = " WHERE " + columnName + " == \"" + searchValue + "\"";
			whereDone = true;
		}

		string cond2 = "";
		if(columnNameCondition2 != "" && searchValueCondition2 != "") 
		{
			string cond2Pre = "";
			if(whereDone)
				cond2Pre = " AND ";
			else
				cond2Pre = " WHERE ";

			cond2 = cond2Pre + columnNameCondition2 + " == \"" + searchValueCondition2 + "\""; 
		}

		dbcmd.CommandText = "UPDATE " + tableName +
			" SET " + columnName + " = \"" + newValue + "\"" +  
			cond1 +
			cond2
			;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		if( ! dbconOpened)
			SqliteGeneral.Sqlite.Close();
	}

	public void Delete(bool dbconOpened, string tableName, int uniqueID)
	{
		if( ! dbconOpened)
			SqliteGeneral.Sqlite.Open();

		dbcmd.CommandText = "DELETE FROM " + tableName +
			" WHERE uniqueID == " + uniqueID.ToString();
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		if( ! dbconOpened)
			SqliteGeneral.Sqlite.Close();
	}
	
	public void DeleteSelectingField(bool dbconOpened, string tableName, string fieldName, string id)
	{
		if( ! dbconOpened)
			SqliteGeneral.Sqlite.Open();

		dbcmd.CommandText = "DELETE FROM " + tableName +
			" WHERE " + fieldName + " == " + id;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		if( ! dbconOpened)
			SqliteGeneral.Sqlite.Close();
	}


	public void DeleteFromName(bool dbconOpened, string tableName, string name)
	{
		if( ! dbconOpened)
			SqliteGeneral.Sqlite.Open();

		dbcmd.CommandText = "DELETE FROM " + tableName +
			" WHERE name == \"" + name + "\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		if( ! dbconOpened)
			SqliteGeneral.Sqlite.Close();
	}

	public void DeleteFromAnInt(bool dbconOpened, string tableName, string colName, int id)
	{
		if( ! dbconOpened)
			SqliteGeneral.Sqlite.Open();

		dbcmd.CommandText = "DELETE FROM " + tableName +
			" WHERE " + colName + " == " + id;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		if( ! dbconOpened)
			SqliteGeneral.Sqlite.Close();
	}


	/* end of methods for different classes */
	
	/* 
	 * SERVER STUFF
	 */
	
	public string sqlFileServer;
	public string connectionStringServer;
	
	public bool CheckFileServer(){
		if (File.Exists(sqlFileServer))
			return true;
		else
			return false;
	}
	
	public void ConnectServer()
	{
		dbcon = new SqliteConnection();
		dbcon.ConnectionString = connectionStringServer;
		dbcmd = dbcon.CreateCommand();
	}
	
	public bool DisConnect() {
		try {
			SqliteGeneral.Sqlite.Close();
		} catch {
			return false;
		}
		return true;
	}


}
