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

using System;
using System.Collections.Generic; //List<T>

class ComputerDB
{
	public string name;
	public string path;
	public string pathToFindMoments; //at barcelona we can find datetimes on 4 folders to know which moment
	//exercises, if one is not done on that computer: -1
	public int exBicepsCurlID; 	//0% bodyweight
	public int exJumpID; 		//100% bodyweight
	public int exSitToStandID; 	//100% bodyweight
	//do not analyze: shopping bag and object in shelf

	public enum ExerciseString { BICEPSCURL, JUMP, SITTOSTAND };
	public ComputerDB(string name,
			string path,
			string pathToFindMoments,
			int exBicepsCurlID, int exJumpID, int exSitToStandID)
	{
		this.name = name;
		this.path = path;
		this.pathToFindMoments = pathToFindMoments;
		this.exBicepsCurlID = exBicepsCurlID;
		this.exJumpID = exJumpID;
		this.exSitToStandID = exSitToStandID;
	}
}

class ComputerDBManage
{
	/*
	 * to know exercises of each DB:
	 * select * from encoderExercise;
	 * select exerciseID, count(*) from encoder group by exerciseID;
	 */

	public List<ComputerDB> list;
	public ComputerDBManage()
	{
		list = new List<ComputerDB>();
		list.Add(new ComputerDB(
				"barcelona1", 
				"/home/xavier/Documents/academic/investigacio/Encoder_SITLESS/carpetes-chronojump-senceres/barcelona/wetransfer-8ba4dd/Encoder_Copies_17_07_2019/database",
				"/home/xavier/Documents/academic/investigacio/Encoder_SITLESS/arxius-processats-per-ells/barcelona",
				8, -1, 7));
		list.Add(new ComputerDB(
				"barcelona2", 
				"/home/xavier/Documents/academic/investigacio/Encoder_SITLESS/carpetes-chronojump-senceres/barcelona/wetransfer-8ba4dd/Encoder_Copies_17_07_2019/Darrera_còpia_pc_prèstec/chronojump/database",
				"/home/xavier/Documents/academic/investigacio/Encoder_SITLESS/arxius-processats-per-ells/barcelona",
				8, 4, 7));
		list.Add(new ComputerDB(
				"belfast",
				"/home/xavier/Documents/academic/investigacio/Encoder_SITLESS/carpetes-chronojump-senceres/Belfast_chronojump/chronojump/database",
				"",
				12, 14, 15)); //note: belfast has biceps curl 12 (2kg), and 13 (4kg)
		list.Add(new ComputerDB(
				"denmark1",
				"/home/xavier/Documents/academic/investigacio/Encoder_SITLESS/carpetes-chronojump-senceres/denmark/wetransfer-08b800/Chronojump Backup 09.10.2019 - HP - FINAL - DK site/database",
				"",
				8, 9, 7));
		list.Add(new ComputerDB(
				"denmark2",
				"/home/xavier/Documents/academic/investigacio/Encoder_SITLESS/carpetes-chronojump-senceres/denmark/wetransfer-08b800/Chronojump Backup 09.10.2019 - Lenovo - FINAL - DK site/database",
				"",
				8, 11, 7));
		list.Add(new ComputerDB(
				"ulm1",
				"/home/xavier/Documents/academic/investigacio/Encoder_SITLESS/carpetes-chronojump-senceres/Encoder_Ulm/Laptop1_Chronojump_für Maria_Nov2019/chronojump/database",
				"",
				8,4,7));
		list.Add(new ComputerDB(
				"ulm2",
				"/home/xavier/Documents/academic/investigacio/Encoder_SITLESS/carpetes-chronojump-senceres/Encoder_Ulm/Laptop2_Chronojump_für Maria_Nov2019/database",
				"",
				8,9,7)); //note: they have also jumps on 4
		list.Add(new ComputerDB(
				"ulm3",
				"/home/xavier/Documents/academic/investigacio/Encoder_SITLESS/carpetes-chronojump-senceres/Encoder_Ulm/Laptop3_Chronojump_für Maria_Nov2019/database",
				"",
				8,4,7));
	}
}

