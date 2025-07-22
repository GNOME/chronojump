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
 *  Copyright (C) 2004-2024   Xavier de Blas <xaviblas@gmail.com>
 */

using System;

public class EncoderExercise
{
	public int uniqueID;
	public string name;
	public int percentBodyWeight;
	public string ressistance;
	public string description;
	public double speed1RM;
	private Constants.EncoderGI type;

	public EncoderExercise() {
	}

	public EncoderExercise(string name) {
		this.name = name;
	}

	public EncoderExercise(int uniqueID, string name, int percentBodyWeight, 
			string ressistance, string description, double speed1RM, Constants.EncoderGI type)
	{
		this.uniqueID = uniqueID;
		this.name = name;
		this.percentBodyWeight = percentBodyWeight;
		this.ressistance = ressistance;
		this.description = description;
		this.speed1RM = speed1RM;
		this.type = type;
	}

	/*
	 * unused, on 1.9.1 all encoder exercises can be deleted
	public bool IsPredefined() {
		if(
				name == "Bench press" ||
				name == "Squat" ||
				name == "Free" ||
				name == "Jump" ||
				name == "Inclined plane" ||
				name == "Inclined plane BW" )
			return true;
		else 
			return false;
	}
	*/

	public override string ToString()
	{
		return uniqueID.ToString() + ": " + name + " (" + percentBodyWeight.ToString() + "%) " +
			ressistance + "," + description + "," + speed1RM.ToString() + "," + type.ToString();
	}

	public static string [] ListToString (List<EncoderExercise> ex_l)
	{
		string [] myString = new String[ex_l.Count];
		int i = 0;
		foreach (EncoderExercise ex in ex_l)
			myString[i++] = ex.Name;

		return myString;
	}

	public int UniqueID
	{
		get { return uniqueID; }
	}
	public string Name
	{
		get { return name; }
	}
	public int PercentBodyWeight
	{
		get { return percentBodyWeight; }
	}
	public string Ressistance
	{
		get { return ressistance; }
	}
	public string Description
	{
		get { return description; }
	}
	public double Speed1RM
	{
		get { return speed1RM; }
	}
	public Constants.EncoderGI Type
	{
		get { return type; }
	}

	~EncoderExercise() {}
}
