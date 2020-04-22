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
 * Copyright (C) 2016-2017 Carles Pina
 * Copyright (C) 2016-2020 Xavier de Blas
 */

using System;
using System.Text;
using System.Collections;
using Mono.Unix;


public class Task
{
	public int Id;
	public char Type; //initially 'P'arametrized or 'F'ree. Now all are 'P'
	public int PersonId;
	public int StationId;
	public int ExerciseId;
	public string ExerciseName;
	public int Sets;
	public int Nreps;
	public float Load;
	public float Speed;
	public float PercentMaxSpeed;
	public string Laterality;
	public string Comment;

	public Task()
	{
		Id = -1;
		Comment = "";
	}

	public Task(int id, int personId, int stationId, int exerciseId, string exerciseName,
			int sets, int nreps, float load, float speed, float percentMaxSpeed,
			string laterality, string comment)
	{
		Type = 'P'; //parametrized

		Id = id;
		PersonId = personId;
		StationId = stationId;
		ExerciseId = exerciseId;
		ExerciseName = exerciseName;
		Sets = sets;
		Nreps = nreps;
		Load = load;
		Speed = speed;
		PercentMaxSpeed = percentMaxSpeed;
		Laterality = laterality;
		Comment = comment;
	}

	public override string ToString()
	{
		string sep = "";
		string str = "";
		if (Laterality == "R" || Laterality == "L")
		{
			string lateralityStr = Catalog.GetString("Right");
			if (Laterality == "L")
				lateralityStr = Catalog.GetString("Left");

			str += sep + lateralityStr;
			sep = "; ";
		}
		if (Load != -1)
		{
			str += sep + "Càrrega = " + Load.ToString() + " Kg";
			sep = "; ";
		}
		if (Sets != -1)
		{
			str += sep + "Series = " + Sets.ToString();
			sep = "; ";
		}
		if (Nreps != -1)
		{
			str += sep + "Repeticions = " + Nreps.ToString();
			sep = "; ";
		}
		if (Speed != -1)
		{
			str += sep + "Velocitat = " + Speed.ToString() + " m/s";
			sep = "; ";
		}
		if (PercentMaxSpeed != -1)
		{
			str += sep + "Velocitat = " + PercentMaxSpeed.ToString() + " %";
			sep = "; ";
		}
		if (Comment != "")
		{
			str += "\n" + Comment;
		}
		return ExerciseName + ": " + str;
	}
}

public class StationCount
{
	private string stationName;
	private int tasksCount;

	public StationCount()
	{
	}

	public StationCount(string name, int count)
	{
		stationName = name;
		tasksCount = count;
	}

	public override string ToString()
	{
		return stationName + " (" + tasksCount.ToString() + ")";
	}
}
