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
using System.Collections.Generic; //List<T>

class Exercise
{
	public enum Names { BICEPSCURL, JUMP, SITTOSTAND }
	public enum Contractions { c, ec, ecS }

	public Names name;
	public int distMin;
	public Contractions contraction;

	public Exercise (Names name, int distMin, Contractions contraction)
	{
		this.name = name;
		this.distMin = distMin;
		this.contraction = contraction;
	}
}

class ExerciseManage
{
	public List<Exercise> list;
	public ExerciseManage()
	{
		list = new List<Exercise>();
		list.Add(new Exercise(
					Exercise.Names.BICEPSCURL,
					10,
					Exercise.Contractions.c));

		list.Add(new Exercise(Exercise.Names.JUMP,
					5,
					Exercise.Contractions.c));

		list.Add(new Exercise(Exercise.Names.SITTOSTAND,
					10,
					Exercise.Contractions.c)); //better value c as there's no control on ecc execution
	}

	public Exercise GetExercise(Exercise.Names name)
	{
		foreach(Exercise ex in list)
			if(ex.name == name)
				return ex;

		//default if strange error
		return list[0];
	}
}
