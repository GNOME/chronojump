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

using System;
using System.IO;
using System.Collections.Generic; //List<T>
using System.Text.RegularExpressions; //Regex

/*
 * Denmark has:
 * ID number_initials of the participant_time period (pre, post, fu12, fu18).
 * but fu can be also in caps
 * and sometimes is found IDnumberinitialstimeperiod, and other times initialsIDnumbertimeperiod
 * sometimes separated by underscores and other times not
 *
 * 545-1318_fllu_FU18-2019-08-01_13-43-58.txt
 * 479-1318fllufu12-2019-03-19_10-10-30.txt
 * 265-knni1310pre-2017-10-20_12-51-48.txt
 *
 * so- remove until the first -
 * remove datetime.txt
 * find pre, post (caps or not), see if find a fu12 or fu18 (caps or not), and remove it
 * find number: should be code
 * the rest removing - or _ shoul be the name
 *
 * on the other computer must of the names have no moment (pre, post, ...)
 */


class ComputerDB
{
	public string city;
	public string computer;
	public string path;
	public string pathToFindMoments; //at barcelona we can find datetimes on 4 folders to know which moment
	//exercises, if one is not done on that computer: -1
	public int exBicepsCurlID; 	//0% bodyweight
	public int exJumpID; 		//100% bodyweight
	public int exSitToStandID; 	//100% bodyweight
	//do not analyze: shopping bag and object in shelf
	public string momentPreName;
	public string momentPostName;
	public string moment12Name;
	public string moment18Name;

	public ComputerDB(
			string city,
			string computer,
			string path,
			string pathToFindMoments,
			int exBicepsCurlID, int exJumpID, int exSitToStandID,
			string momentPreName, string momentPostName,
			string moment12Name, string moment18Name
			)
	{
		this.city = city;
		this.computer = computer;
		this.path = path;
		this.pathToFindMoments = pathToFindMoments;
		this.exBicepsCurlID = exBicepsCurlID;
		this.exJumpID = exJumpID;
		this.exSitToStandID = exSitToStandID;
		this.momentPreName = momentPreName;
		this.momentPostName = momentPostName;
		this.moment12Name = moment12Name;
		this.moment18Name = moment18Name;
	}

	public enum Moment { NONE, PRE, POST, M12, M18, MULTIPLE }
	public string FindMoment (string filename)
	{
		filename = filename.ToUpper();
		Moment m = Moment.NONE;

		Console.WriteLine("FindNotBarcelona filename: " + filename);
		if(filename.Contains(momentPreName))
		{
			if(m != Moment.NONE)
				m = Moment.MULTIPLE;
			else
				m = Moment.PRE;
		} else if(filename.Contains(momentPostName))
		{
			if(m != Moment.NONE)
				m = Moment.MULTIPLE;
			else
				m = Moment.POST;
		} else if(filename.Contains(moment12Name))
		{
			if(m != Moment.NONE)
				m = Moment.MULTIPLE;
			else
				m = Moment.M12;
		} else if(filename.Contains(moment18Name))
		{
			if(m != Moment.NONE)
				m = Moment.MULTIPLE;
			else
				m = Moment.M18;
		}

		//some cities did not use the PRE and they simply left blank, so default to PRE
		if(m == Moment.NONE)
			m = Moment.PRE;


		return m.ToString();
	}
}


class ComputerDBManage
{
	/*
	 * to know exercises of each DB:
	 * select * from encoderExercise;
	 * select exerciseID, count(*) from encoder group by exerciseID;
	 */
	string path = "/home/xavier/Documents/academic/investigacio/Encoder_SITLESS_nogit/carpetes-chronojump-senceres/"; //laptop
	//string path = "/home/xavier/Documents/sitless-no-git/"; //computer

	public List<ComputerDB> list;
	public ComputerDBManage()
	{
		//atencio pq a barcelona1 hi ha:
		///home/xavier/Documents/academic/investigacio/Encoder_SITLESS_nogit/carpetes-chronojump-senceres/barcelona/wetransfer-8ba4dd/Encoder_Copies_17_07_2019/database/chronojump.db (604 Kb)
		//	i
		///home/xavier/Documents/academic/investigacio/Encoder_SITLESS_nogit/carpetes-chronojump-senceres/barcelona/wetransfer-8ba4dd/Encoder_Copies_17_07_2019/database/chronojump/database/chronojump.db (186 Kb)

		list = new List<ComputerDB>();
		list.Add(new ComputerDB(
				"barcelona",
				"barcelona1", 
				path + "barcelona/wetransfer-8ba4dd/Encoder_Copies_17_07_2019/database",
				"/home/xavier/Documents/academic/investigacio/Encoder_SITLESS_nogit/arxius-processats-per-ells/barcelona",
				8, -1, 7,
				"", "", "", ""));
		list.Add(new ComputerDB(
				"barcelona",
				"barcelona2", 
				path + "barcelona/wetransfer-8ba4dd/Encoder_Copies_17_07_2019/Darrera_còpia_pc_prèstec/chronojump/database",
				"/home/xavier/Documents/academic/investigacio/Encoder_SITLESS_nogit/arxius-processats-per-ells/barcelona",
				8, 4, 7,
				"", "", "", ""));
		/*
		//belfast is ok except the double exercise on biceps
		list.Add(new ComputerDB(
				"belfast",
				"belfast",
				path + "Belfast_chronojump/chronojump/database",
				"",
				12, 14, 15, 	//note: belfast has biceps curl 12 (2kg), and 13 (4kg)
				"PRE", "PI", "12M", "18M"));
		//to process onlh the 4Kg biceps on Belfast
		list.Add(new ComputerDB(
				"belfast",
				"belfast",
				path + "Belfast_chronojump/chronojump/database",
				"",
				13, -1, -1, 	//note: belfast has biceps curl 12 (2kg), and 13 (4kg)
				"PRE", "PI", "12M", "18M"));
		list.Add(new ComputerDB(
				"denmark",
				"denmark1",
				path + "denmark/wetransfer-08b800/Chronojump Backup 09.10.2019 - HP - FINAL - DK site/database",
				"",
				8, 9, 7,
				"PRE", "POST", "FU12", "FU18"));
		list.Add(new ComputerDB(
				"denmark",
				"denmark2",
				path + "denmark/wetransfer-08b800/Chronojump Backup 09.10.2019 - Lenovo - FINAL - DK site/database",
				"",
				8, 11, 7,
				"PRE", "POST", "FU12", "FU18"));
		*/
		/*
		list.Add(new ComputerDB(
				"ulm",
				"ulm1",
				path + "Encoder_Ulm/Laptop1_Chronojump_für Maria_Nov2019/chronojump/database",
				"",
				8,4,7,
				"PRE", "A2", "A3", "A4")); //in caps because comparison is done in caps
		*/
		/*
		list.Add(new ComputerDB(
				"ulm",
				"ulm2",
				path + "Encoder_Ulm/Laptop2_Chronojump_für Maria_Nov2019/database",
				"",
				8,9,7,  //note: they have also jumps on 4
				"PRE", "A2", "A3", "A4"));
		list.Add(new ComputerDB(
				"ulm",
				"ulm3",
				path + "Encoder_Ulm/Laptop3_Chronojump_für Maria_Nov2019/database",
				"",
				8,4,7,
				"PRE", "A2", "A3", "A4"));
		*/
	}
}

static class ComputerDBMomentByProcessedFiles
{
	public static string FindOnBarcelona(string location, string filename)
	{
		//1 parse date of filename
		Console.WriteLine("FindMoment for filename: " + filename);
		string searchedDatetime = getFilenameDatetime(filename);
		if(searchedDatetime == "")
			return "(moment)";

		//2 search date on all folders
		int foundCount = 0;
		string moment = "NOTFOUND:" + searchedDatetime;
		DirectoryInfo [] dirArray = new DirectoryInfo(location).GetDirectories();
                foreach (DirectoryInfo dir in dirArray)
		{
	                foreach (FileInfo file in dir.GetFiles())
			{
				//Console.WriteLine("filename: {0}, date: {1}", file.Name, getFilenameDatetime(file.Name));
				if(getFilenameDatetime(file.Name) == searchedDatetime)
				{
					Console.WriteLine("FOUND at folder: {0}", dir.Name);
					moment = dir.Name;
					foundCount ++;
				}
			}
		}

		if(foundCount >= 2)
		{
			Console.WriteLine(string.Format("FOUND {0} times!!!", foundCount));
			return "DUPLICATED";
		}
		return moment;
	}
	//this function is exclusive from processMultiDatabases code
	private static string getFilenameDatetime(string filename)
	{
		Match match = Regex.Match(filename, @"(\d+-\d+-\d+_\d+-\d+-\d+)");
		if(match.Groups.Count == 2)
			return match.Value;

		return "";
	}

}
