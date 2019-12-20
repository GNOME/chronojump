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
//dist min: sit to stand: 30. study the different dist of each rep is we have a min rep of 3 cm

using System;
using System.IO; //"File" things. TextWriter. Path
using System.Collections.Generic; //List<T>

class ProcessMultiDatabases
{
	private bool debug = false; //on debug just 5 sets of each compDB-exercise are used
	private int distMin = 20; //distMinSitToStand = 20;
	private Sqlite sqlite;
	TextWriter writer;

	//hardcoded stuff (search 'hardcoded' on):
	//callR.cs
	//utilEncoder.cs

	// <---- end of configuration variables


	public static void Main(string[] args)
	{
		new ProcessMultiDatabases();
	}

	public ProcessMultiDatabases()
	{
		sqlite = new Sqlite();
		ComputerDBManage compDBManage = new ComputerDBManage();

		writer = File.CreateText("/tmp/chronojump-processMultiEncoder.csv");
		writer.WriteLine("city,exercise,person,sex,moment,rep,series,exercise,massBody,massExtra,start,width,height,meanSpeed,maxSpeed,maxSpeedT,meanPower,peakPower,peakPowerT,RPD,meanForce,maxForce,maxForceT,RFD,workJ,impulse,laterality,inertiaM");

		foreach(ComputerDB compDB in compDBManage.list)
		{
			sqlite.CreateConnection(compDB.path);
			sqlite.Open();

			if(compDB.exBicepsCurlID != -1)
				processCompDBEx(compDB, ComputerDB.ExerciseString.BICEPSCURL, compDB.exBicepsCurlID, 0);

			if(compDB.exJumpID != -1)
				processCompDBEx(compDB, ComputerDB.ExerciseString.JUMP, compDB.exJumpID, 100);

			if(compDB.exSitToStandID != -1)
				processCompDBEx(compDB, ComputerDB.ExerciseString.SITTOSTAND, compDB.exSitToStandID, 100);

			sqlite.Close();
		}
		writer.Close();
		((IDisposable)writer).Dispose();

		Console.WriteLine("processMultiDatabases done!");
	}

	private void processCompDBEx (ComputerDB compDB, ComputerDB.ExerciseString exerciseString, int exerciseID, int percentBodyWeight)
	{
		List<EncoderSQL> list = sqlite.SelectEncoder (exerciseID);

		int count = 0;
		foreach(EncoderSQL eSQL in list)
		{
			Console.WriteLine(string.Format("progress: {0}/{1} - ", count, list.Count) + eSQL.ToString());

			Console.WriteLine("copying file: " + compDB.path + "/../" + eSQL.url + "/" + eSQL.filename);
			if(! UtilEncoder.CopyEncoderDataToTemp(compDB.path + "/../" + eSQL.url, eSQL.filename))
			{
				Console.WriteLine("Could not copy file, discarding");
				continue;
			}

			Person person = sqlite.SelectPerson (eSQL.personID);
			double personWeight = sqlite.SelectPersonWeight(eSQL.personID);

			EncoderParams ep = new EncoderParams(
					distMin, //preferences.EncoderCaptureMinHeight(encoderConfigurationCurrent.has_inertia),
					percentBodyWeight, //getExercisePercentBodyWeightFromComboCapture (),
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

			Console.WriteLine("Calling R... ");
			new CallR(es);
			Console.WriteLine("CallR done!");

			//output file is: /tmp/chronojump-last-encoder-curves.txt
			//1st two lines are:
			//,series,exercise,massBody,massExtra,start,width,height,meanSpeed,maxSpeed,maxSpeedT,meanPower,peakPower,peakPowerT,pp_ppt,meanForce,maxForce,maxForceT,maxForce_maxForceT,workJ,impulse,laterality,inertiaM
			//1,1,exerciseName,57.9,0,971,498,755,1.51152519574657,3.20590883396153,307,1694.08480044046,4423.25671303514,243,18202.7025227784,1108.13701938937,1754.09683966977,232,7560.7622399559,1980.64161525193,365.685216398493,,-1e-04

			//now we have to parse it to fill the big file
			string filename = "/tmp/chronojump-last-encoder-curves.txt";
			List<string> lines = Util.ReadFileAsStringList(filename);
			bool firstLine = true;
			foreach(string line in lines)
			{
				if(firstLine)
					firstLine = false;
				else {
					string line2 = string.Format("{0},{1},{2},{3},{4}", compDB.name, exerciseString, person.Name, person.Sex,  "(moment)") + line;
					//note personID is not correct because persons sometimes where evaluated on different chronojump machines
					//for this reason has been changed to personName, we suppose is the same on different machines

					writer.WriteLine(line2);
					writer.Flush();
				}
			}

			count ++;

			if(debug && count >= 5)
				break;

			System.Threading.Thread.Sleep(100); //rest a bit
		}

		Console.WriteLine("processDatabase done!");
	}
}
