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
	//exercises, if one is not done on that computer: -1
	public int exBicepsCurlID; 	//0% bodyweight
	public int exJumpID; 		//100% bodyweight
	public int exSitToStandID; 	//100% bodyweight
	//do not analyze: shopping bag and object in shelf

	public enum ExerciseString { BICEPSCURL, JUMP, SITTOSTAND };
	public ComputerDB(string name, string path, int exBicepsCurlID, int exJumpID, int exSitToStandID)
	{
		this.name = name;
		this.path = path;
		this.exBicepsCurlID = exBicepsCurlID;
		this.exJumpID = exJumpID;
		this.exSitToStandID = exSitToStandID;
	}
}

class ComputerDBManage
{
	public List<ComputerDB> list;
	public ComputerDBManage()
	{
		list = new List<ComputerDB>();
		list.Add(new ComputerDB(
				"barcelona1", 
				"/home/xavier/Documents/academic/investigacio/Encoder_SITLESS/carpetes-chronojump-senceres/barcelona/wetransfer-8ba4dd/Encoder_Copies_17_07_2019/database",
				8, -1, 7));
		list.Add(new ComputerDB(
				"barcelona2", 
				"/home/xavier/Documents/academic/investigacio/Encoder_SITLESS/carpetes-chronojump-senceres/barcelona/wetransfer-8ba4dd/Encoder_Copies_17_07_2019/Darrera_còpia_pc_prèstec/chronojump/database",
				8, 4, 7));
	}
}

