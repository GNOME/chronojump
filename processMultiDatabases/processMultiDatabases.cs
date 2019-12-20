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
 * Copyright (C) 2019   Xavier de Blas <xaviblas@gmail.com> 
 */

//TODO:
//manage how to integrate with age that is on different database
//moment: read the moment on the filename on processed files matching with date_time. problem on barcelona1 baseline: need to put 1 before all dates
//- barcelona check on which folder we have that date-time
//- rest of the cities: read the code
//ecc-con: on sit to stand
//
//do not analyze: shopping bag and object in shelf
//
//dist min: sit to stand: 30. study the different dist of each rep is we have a min rep of 3 cm
//


using System;
using System.IO; //"File" things. TextWriter. Path
using System.Collections.Generic; //List<T>
using Mono.Data.Sqlite;

class ProcessMultiDatabases
{
	// start of configuration variables ---->
	//
	//barcelona1
	private string barcelona1Path = "/home/xavier/Documents/academic/investigacio/Encoder_SITLESS/carpetes-chronojump-senceres/barcelona/wetransfer-8ba4dd/Encoder_Copies_17_07_2019/database";
//	private int barcelona1ExJump = ?;
	private int barcelona1ExSitToStand = 7;
	private int barcelona1BicepsCurl = 8;
//	private int barcelona1ShoppingBag = 9;
//	private int barcelona1ObjectInShelf = 10;
	private int distMinSitToStand = 20;

	//current
	private string currentDBPath;
	private string currentFilenamePre;
	private string currentExerciseString;
	private int currentExercise;
	private int currentPercentWeight;

	//hardcoded stuff (search 'hardcoded' on):
	//callR.cs
	//utilEncoder.cs

	// <---- end of configuration variables

	private string database = "chronojump.db";
        private SqliteConnection dbcon;
	private SqliteCommand dbcmd;

	public static void Main(string[] args)
	{
		new ProcessMultiDatabases();
	}

	private void configure()
	{
		currentDBPath = barcelona1Path;
		currentFilenamePre = "barcelona1";
		currentExerciseString = "SITTOSTAND";
		currentExercise = barcelona1ExSitToStand;
		currentPercentWeight = 100;
		/*
		currentExerciseString = "BICEPSCURL";
		currentExercise = barcelona1BicepsCurl;
		currentPercentWeight = 0;
		*/
		/*
		currentExerciseString = "SHOPPINGBAG";
		currentExercise = barcelona1ShoppingBag;
		currentPercentWeight = 0;
		*/
	}

	public ProcessMultiDatabases()
	{
		configure();
		sqliteCreateConnection();
		sqliteOpen();

		processDatabase();

		sqliteClose();
		Console.WriteLine("processMultiDatabases done!");
	}

	private void processDatabase()
	{
		List<EncoderSQL> list = SelectEncoder (currentExercise);

		TextWriter writer = File.CreateText("/tmp/" + currentFilenamePre + "-" + currentExerciseString + ".csv");
		writer.WriteLine("city,exercise,person,sex,moment,rep,series,exercise,massBody,massExtra,start,width,height,meanSpeed,maxSpeed,maxSpeedT,meanPower,peakPower,peakPowerT,RPD,meanForce,maxForce,maxForceT,RFD,workJ,impulse,laterality,inertiaM");

		int count = 0;
		foreach(EncoderSQL eSQL in list)
		{
			Console.WriteLine(string.Format("progress: {0}/{1} - ", count, list.Count) + eSQL.ToString());
			Person person = SelectPerson (eSQL.personID);
			double personWeight = SelectPersonWeight(eSQL.personID);

			EncoderParams ep = new EncoderParams(
					20, //preferences.EncoderCaptureMinHeight(encoderConfigurationCurrent.has_inertia), 
					currentPercentWeight, //TODO: change this value depending on exercise //getExercisePercentBodyWeightFromComboCapture (),
					Util.ConvertToPoint(personWeight), // Util.ConvertToPoint(findMass(Constants.MassType.BODY)),
					Util.ConvertToPoint(eSQL.extraWeight), //Util.ConvertToPoint(findMass(Constants.MassType.EXTRA)),
					"c", //findEccon(true),                                        //force ecS (ecc-conc separated)
					"curves", //is the same than "curvesAC". was: analysis,
					"none",                         //analysisVariables (not needed in create curves). Cannot be blank
					"p", //analysisOptions,
					true, //preferences.encoderCaptureCheckFullyExtended,
					4, //preferences.encoderCaptureCheckFullyExtendedValue,
					"LINEAR", //encoderConfigurationCurrent,
					"0.7", //Util.ConvertToPoint(preferences.encoderSmoothCon),      //R decimal: '.'
					0,                      //curve is not used here
					600, 600, //image_encoder_width, image_encoder_height,
					"," //preferences.CSVExportDecimalSeparator 
					);

			EncoderStruct es = new EncoderStruct(
					UtilEncoder.GetEncoderDataTempFileName(),
					UtilEncoder.GetEncoderGraphTempFileName(),
					UtilEncoder.GetEncoderCurvesTempFileName(),
					UtilEncoder.GetEncoderScriptsPathWithoutLastSep(),
					UtilEncoder.GetEncoderTempPathWithoutLastSep(),
					ep);

			Console.WriteLine("copying file: " + currentDBPath + "/../" + eSQL.url + "/" + eSQL.filename);
			UtilEncoder.CopyEncoderDataToTemp(currentDBPath + "/../" + eSQL.url, eSQL.filename);
			Console.WriteLine("copying file done. Calling R... ");

			new CallR(es);
			Console.WriteLine("CallR done!");

			//output file is: /tmp/chronojump-last-encoder-curves.txt
			//1st two lines are:
			//,series,exercise,massBody,massExtra,start,width,height,meanSpeed,maxSpeed,maxSpeedT,meanPower,peakPower,peakPowerT,pp_ppt,meanForce,maxForce,maxForceT,maxForce_maxForceT,workJ,impulse,laterality,inertiaM
			//1,1,exerciseName,57.9,0,971,498,755,1.51152519574657,3.20590883396153,307,1694.08480044046,4423.25671303514,243,18202.7025227784,1108.13701938937,1754.09683966977,232,7560.7622399559,1980.64161525193,365.685216398493,,-1e-04

			//
			//now we have to parse it to fill the big file
			string filename = "/tmp/chronojump-last-encoder-curves.txt";
			List<string> lines = Util.ReadFileAsStringList(filename);
			bool firstLine = true;
			foreach(string line in lines)
			{
				if(firstLine)
					firstLine = false;
				else {
					string line2 = "BARCELONA," + currentExerciseString + "," + person.Name + "," + person.Sex + ",(moment)," + line;
					//TODO: note this personID is not correct because persons sometimes where evaluated on different chronojump machines
					//for this reason has been changed to personName, we suppose is the same on different machines
					writer.WriteLine(line2);
					writer.Flush();
				}
			}

			count ++;
			/*
			if(count >= 15)
				break;
				*/

			System.Threading.Thread.Sleep(200); //rest a bit
		}

		writer.Close();
		((IDisposable)writer).Dispose();
		Console.WriteLine("processDatabase done!");
	}

	// ---- sqlite main methods ----

	private void CreateAndOpen()
	{
		sqliteCreateConnection();
		sqliteOpen();
	}

	private void sqliteCreateConnection()
	{
		dbcon = new SqliteConnection ();
	        string sqlFile = currentDBPath + Path.DirectorySeparatorChar + database;
		Console.WriteLine(sqlFile);
		dbcon.ConnectionString = "version = 3; Data source = " + sqlFile;
		dbcmd = dbcon.CreateCommand();
	}
	private void sqliteOpen()
	{
		dbcon.Open();
	}
	private void sqliteClose()
	{
		dbcon.Close();
	}

	public Person SelectPerson (int uniqueID)
	{
		dbcmd.CommandText = "SELECT * FROM person77 WHERE uniqueID = " + uniqueID;
		Console.WriteLine(dbcmd.CommandText.ToString());

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		Person p = new Person(-1);
		if(reader.Read()) {
			p = new Person(
					Convert.ToInt32(reader[0].ToString()), //uniqueID
					reader[1].ToString(),                   //name
					reader[2].ToString(),                   //sex
					UtilDate.FromSql(reader[3].ToString()),//dateBorn
					Convert.ToInt32(reader[4].ToString()), //race
					Convert.ToInt32(reader[5].ToString()), //countryID
					reader[6].ToString(),                   //description
					reader[7].ToString(),                   //future1: rfid
					reader[8].ToString(),                   //future2: clubID
					Convert.ToInt32(reader[9].ToString()) //serverUniqueID
				      );
		}
		reader.Close();

		return p;
	}

	private double SelectPersonWeight (int personID)
	{
		dbcmd.CommandText = "SELECT weight FROM personSession77 WHERE personID = " + personID;
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		double myReturn = 0;
                if(reader.Read()) {
                        myReturn = Convert.ToDouble(Util.ChangeDecimalSeparator(reader[0].ToString()));
                }
                reader.Close();

                return myReturn;
	}

	//TODO: will need session to process this by sessions or compare with filenames
	private List<EncoderSQL> SelectEncoder (int exerciseID)
        {
		dbcmd.CommandText = "SELECT * FROM encoder WHERE signalOrCurve = 'signal' AND exerciseID = " + exerciseID;
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		List<EncoderSQL> list = new List<EncoderSQL>();
		while(reader.Read())
		{
			EncoderSQL eSQL = new EncoderSQL (
					reader[0].ToString(),                   //uniqueID
					Convert.ToInt32(reader[1].ToString()),  //personID      
					Convert.ToInt32(reader[2].ToString()),  //sessionID
					Convert.ToInt32(reader[3].ToString()),  //exerciseID
					reader[4].ToString(),                   //eccon
					reader[5].ToString(),//laterality
					Util.ChangeDecimalSeparator(reader[6].ToString()),      //extraWeight
					reader[7].ToString(),                   //signalOrCurve
					reader[8].ToString(),                   //filename
					fixOSpath(reader[9].ToString()), //Util.MakeURLabsolute(fixOSpath(reader[9].ToString())),  //url
					Convert.ToInt32(reader[10].ToString()), //time
					Convert.ToInt32(reader[11].ToString()), //minHeight
					reader[12].ToString(),                  //description
					reader[13].ToString(),                  //status
					reader[14].ToString(), //videoURL,                               //videoURL
					reader[15].ToString(), //econf,                                  //encoderConfiguration
					Util.ChangeDecimalSeparator(reader[16].ToString()),     //future1 (meanPower on curves)
					reader[17].ToString(),                  //future2
					reader[18].ToString()//,                  //future3
					//reader[19].ToString()                   //EncoderExercise.name
						);
			list.Add (eSQL);
		}
		reader.Close();
		return list;
	}

	private static string fixOSpath(string url) {
		//if(UtilAll.IsWindows())
		//	return url.Replace("/","\\");
		//else
			return url.Replace("\\","/");
	}

	
	// ---- end of sqlite main methods ----

}


