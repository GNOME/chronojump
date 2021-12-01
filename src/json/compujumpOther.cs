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
 * Copyright (C) 2016-2021 Xavier de Blas
 */

using System;
using System.Json;
using System.Text;
using System.Collections;
using System.Collections.Generic; //List
using System.Text.RegularExpressions; //Regex
using Mono.Unix;

public abstract class Task
{
	public int Id;
	public int PersonId;
	public string Comment;

	protected void initialize ()
	{
		Id = -1;
		Comment = "";
	}

	public override string ToString()
	{
		return "";
	}
}

public class TaskEncoder : Task
{
	public int ExerciseId;
	public string ExerciseName;
	public int Sets;
	public int Nreps;
	public float Load;
	public float Speed;
	public float PercentMaxSpeed;
	public string Laterality;

	public TaskEncoder ()
	{
		initialize ();
	}

	public TaskEncoder (int id, int personId, int exerciseId, string exerciseName,
			int sets, int nreps, float load, float speed, float percentMaxSpeed,
			string laterality, string comment)
	{
		Id = id;
		PersonId = personId;
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

	public TaskEncoder (JsonValue jsonTask)
	{
		Id = jsonTask ["id"];
		PersonId = jsonTask ["player_id"];
		ExerciseId = jsonTask ["exercise_id"];
		ExerciseName = jsonTask ["exercise_name"];

		if(jsonTask ["duration"] == null) //fix if duration is not defined
			Nreps = -1;
		else {
			string NrepsStr = jsonTask ["duration"]; //eg: "15 Repetitions"
			Match match = Regex.Match(NrepsStr, @"(\d+) Repetitions");
			//LogB.Information(string.Format("NrepsStr: {0}, match.Groups.Count: {1}, match.Groups[1].Value: {2}",
			//	NrepsStr, match.Groups.Count, match.Groups[1].Value));
			if(match.Groups.Count == 2 && Util.IsNumber(match.Groups[1].Value, false))
				Nreps = Convert.ToInt32(match.Groups[1].Value);
		}

		Laterality = jsonTask ["laterality"];
		Sets = jsonTask ["sets"];

		//measurable_info
		Load = jsonTask ["measurable_info"]["load"];
		Speed = jsonTask ["measurable_info"]["speed"];
		PercentMaxSpeed = jsonTask ["measurable_info"]["percent_max_speed"];

		Comment = jsonTask ["comment"];
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

public class TaskDeserialize
{
	public TaskDeserialize()
	{
	}

	public List<Task> DeserializeTaskEncoder (string responseFromServer)
	{
		List<Task> tasks = new List<Task>();

		JsonValue jsonTasks = JsonValue.Parse (responseFromServer);
		foreach (JsonValue jsonTask in jsonTasks) {
			tasks.Add(new TaskEncoder(jsonTask));
		}
		return tasks;
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
