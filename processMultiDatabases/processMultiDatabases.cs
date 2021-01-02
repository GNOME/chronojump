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
 * Copyright (C) 2019-2020   Xavier de Blas <xaviblas@gmail.com> 
 */

/*
 * crashed at:
 * expression("1 Title=", "hola")
[1] "/tmp/chronojump-last-encoder-data.txt"
[1] "/tmp/chronojump-last-encoder-graph.png"
[1] "/tmp/chronojump-last-encoder-curves.txt"
[1] "/tmp/chronojump-encoder-status-"
[1] "/tmp/chronojump-special-data.txt"
Read 8713 items

Unhandled Exception:
System.IO.IOException: Write fault on path /home/xavier/informatica/progs_meus/chronojump/chronojump/processMultiDatabases/[Unknown]
  at System.IO.FileStream.WriteInternal (System.Byte[] src, System.Int32 offset, System.Int32 count) <0x7f96f2c927d0 + 0x001ba> in <d2ec5c92492f4d6ba8c422bdf574b786>:0
  at System.IO.FileStream.Write (System.Byte[] array, System.Int32 offset, System.Int32 count) <0x7f96f2c92680 + 0x000bd> in <d2ec5c92492f4d6ba8c422bdf574b786>:0
  at System.IO.StreamWriter.Flush (System.Boolean flushStream, System.Boolean flushEncoder) <0x7f96f2c615c0 + 0x000c4> in <d2ec5c92492f4d6ba8c422bdf574b786>:0
  at System.IO.StreamWriter.WriteSpan (System.ReadOnlySpan`1[T] buffer, System.Boolean appendNewLine) <0x7f96f2c61a30 + 0x001f5> in <d2ec5c92492f4d6ba8c422bdf574b786>:0
  at System.IO.StreamWriter.WriteLine (System.String value) <0x7f96f2c61de0 + 0x00178> in <d2ec5c92492f4d6ba8c422bdf574b786>:0
  at CallR.callRStart () [0x0019d] in <1c2b597e9ef34320b28fca7ab406049b>:0
  at CallR..ctor (EncoderStruct es) [0x0001a] in <1c2b597e9ef34320b28fca7ab406049b>:0
  at ProcessMultiDatabases.processCompDBEx (ComputerDB compDB, ComputerDB+ExerciseString exerciseString, System.Int32 exerciseID, System.Int32 percentBodyWeight) [0x00169] in <1c2b597e9ef34320b28fca7ab406049b>:0
  at ProcessMultiDatabases..ctor () [0x000a2] in <1c2b597e9ef34320b28fca7ab406049b>:0
  at ProcessMultiDatabases.Main (System.String[] args) [0x00000] in <1c2b597e9ef34320b28fca7ab406049b>:0
[ERROR] FATAL UNHANDLED EXCEPTION: System.IO.IOException: Write fault on path /home/xavier/informatica/progs_meus/chronojump/chronojump/processMultiDatabases/[Unknown]
  at System.IO.FileStream.WriteInternal (System.Byte[] src, System.Int32 offset, System.Int32 count) <0x7f96f2c927d0 + 0x001ba> in <d2ec5c92492f4d6ba8c422bdf574b786>:0
  at System.IO.FileStream.Write (System.Byte[] array, System.Int32 offset, System.Int32 count) <0x7f96f2c92680 + 0x000bd> in <d2ec5c92492f4d6ba8c422bdf574b786>:0
  at System.IO.StreamWriter.Flush (System.Boolean flushStream, System.Boolean flushEncoder) <0x7f96f2c615c0 + 0x000c4> in <d2ec5c92492f4d6ba8c422bdf574b786>:0
  at System.IO.StreamWriter.WriteSpan (System.ReadOnlySpan`1[T] buffer, System.Boolean appendNewLine) <0x7f96f2c61a30 + 0x001f5> in <d2ec5c92492f4d6ba8c422bdf574b786>:0
  at System.IO.StreamWriter.WriteLine (System.String value) <0x7f96f2c61de0 + 0x00178> in <d2ec5c92492f4d6ba8c422bdf574b786>:0
  at CallR.callRStart () [0x0019d] in <1c2b597e9ef34320b28fca7ab406049b>:0
  at CallR..ctor (EncoderStruct es) [0x0001a] in <1c2b597e9ef34320b28fca7ab406049b>:0
  at ProcessMultiDatabases.processCompDBEx (ComputerDB compDB, ComputerDB+ExerciseString exerciseString, System.Int32 exerciseID, System.Int32 percentBodyWeight) [0x00169] in <1c2b597e9ef34320b28fca7ab406049b>:0
  at ProcessMultiDatabases..ctor () [0x000a2] in <1c2b597e9ef34320b28fca7ab406049b>:0
  at ProcessMultiDatabases.Main (System.String[] args) [0x00000] in <1c2b597e9ef34320b28fca7ab406049b>:0

  maybe R has been closed and have to be opened again, see how is done in Chronojump
  */

/*results of moment are:
 * > d=read.csv("chronojump-processMultiEncoder.csv")
 * > table(d$moment)

                       12M_3                        18M_4
                        1760                         1382
                    Baseline                        End_2
                        6236                          584
NOTFOUND:2015-11-27_13-35-27 NOTFOUND:2015-11-27_13-38-49
                           5                            1
NOTFOUND:2015-11-27_13-45-37 NOTFOUND:2015-12-03_08-28-14
                           6                            3
NOTFOUND:2015-12-18_16-07-48 NOTFOUND:2015-12-18_16-08-19
                           3                            3
NOTFOUND:2015-12-18_16-08-33 NOTFOUND:2016-01-11_18-15-47
                           3                            3
NOTFOUND:2018-02-06_12-23-01 NOTFOUND:2018-02-15_10-44-45
                           3                            3
NOTFOUND:2018-03-09_09-50-31 NOTFOUND:2018-03-12_10-00-07
                           2                            4
NOTFOUND:2018-07-19_09-04-43 NOTFOUND:2019-03-26_09-58-30
                           3                            3
*/


//TODO:
//manage how to integrate with age that is on different database
//moment: read the moment on the filename on processed files matching with date_time. problem on barcelona1 baseline: need to put 1 before all dates
//- barcelona check on which folder we have that date-time
//- rest of the cities: read the code
//ecc-con: on sit to stand
//dist min: sit to stand: 30. study the different dist of each rep is we have a min rep of 3 cm
//- need to manage the extra weight on each exercise (biceps curl at belfast is 2 or 4 Kg). but no problem, is stored on the eSQL

using System;
using System.IO; //"File" things. TextWriter. Path
using System.Collections.Generic; //List<T>

class ProcessMultiDatabases
{
	private bool debug = false; //on debug just 5 sets of each compDB-exercise are used
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
		ExerciseManage exManage = new ExerciseManage();

		writer = File.CreateText("/tmp/chronojump-processMultiEncoder.csv");
		//writer = File.CreateText("/home/xavier/chronojump-processMultiEncoder.csv");
		writer.WriteLine("city,computer,person,personCode,sex,exercise,moment,rep,series,exercise,massBody,massExtra,start,width,height,meanSpeed,maxSpeed,maxSpeedT,meanPower,peakPower,peakPowerT,RPD,meanForce,maxForce,maxForceT,RFD,workJ,impulse,laterality,inertiaM,gearedDownREMOVECOL,lateralityREMOVECOL");

		foreach(ComputerDB compDB in compDBManage.list)
		{
			sqlite.CreateConnection(compDB.path);
			sqlite.Open();

			if(compDB.exBicepsCurlID != -1)
				processCompDBEx(compDB, exManage.GetExercise(Exercise.Names.BICEPSCURL), compDB.exBicepsCurlID, 0);

			/*
			if(compDB.exJumpID != -1)
				processCompDBEx(compDB, exManage.GetExercise(Exercise.Names.JUMP), compDB.exJumpID, 100);

			if(compDB.exSitToStandID != -1)
				processCompDBEx(compDB, exManage.GetExercise(Exercise.Names.SITTOSTAND), compDB.exSitToStandID, 100);
				*/

			sqlite.Close();
		}
		writer.Close();
		((IDisposable)writer).Dispose();

		Console.WriteLine("processMultiDatabases done!\n\nAttention!\nLast two columns have to be REMOVED: gearedDown, laterality for compatibility with previous sitless results\n");
	}

	private void processCompDBEx (ComputerDB compDB, Exercise exercise, int exerciseID, int percentBodyWeight)
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
					exercise.distMin, //preferences.EncoderCaptureMinHeight(encoderConfigurationCurrent.has_inertia),
					percentBodyWeight, //getExercisePercentBodyWeightFromComboCapture (),
					Util.ConvertToPoint(personWeight), // Util.ConvertToPoint(findMass(Constants.MassType.BODY)),
					Util.ConvertToPoint(eSQL.extraWeight), //Util.ConvertToPoint(findMass(Constants.MassType.EXTRA)),
					exercise.contraction.ToString(), //findEccon(true),
					"curvesProcessMultiDB", //"curves" is the same than "curvesAC". was: analysis. Note curvesProcessMultiDB is like curves but without making the graph
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

			string moment = "(moment)";
			if(compDB.pathToFindMoments != "")
				moment = ComputerDBMomentByProcessedFiles.FindOnBarcelona(compDB.pathToFindMoments, eSQL.filename);
			else {
				moment = compDB.FindMoment(eSQL.filename);
			}

			//now we have to parse it to fill the big file
			string filename = "/tmp/chronojump-last-encoder-curves.txt";
			List<string> reps = Util.ReadFileAsStringList(filename);
			bool firstRep = true;
			foreach(string rep in reps)
			{
				if(firstRep)
					firstRep = false;
				else {
					string repToWriter = string.Format("{0},{1},{2},{3},{4},{5},{6},",
							compDB.city, compDB.computer, person.Name, person.FindPersonCode(compDB.city), person.Sex, exercise.name, moment) + rep;
					//note personID is not correct because persons sometimes where evaluated on different chronojump machines
					//for this reason has been changed to personName, we suppose is the same on different machines
					//person.FindPersonCode() should be the code of that person on all the computers of a given city

					writer.WriteLine(repToWriter);
					writer.Flush();
				}
			}

			count ++;

			if(debug && count >= 5)
				break;

			//System.Threading.Thread.Sleep(200);
			System.Threading.Thread.Sleep(50);
			/*
			 * rest a bit 100 gave me one problem:
			 * System.IO.IOException: Write fault on path /home/xavier/informatica/progs_meus/chronojump/chronojump/processMultiDatabases/[Unknown]
			 * trying 200
			 */
		}

		Console.WriteLine("processDatabase done!");
	}
}
