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

public class Encoder1RM
{
	public int uniqueID;
	public int personID;
	public int sessionID;
	public DateTime date;
	public int exerciseID;
	public double load1RM;
	
	public string personName;
	public string exerciseName;
	
	public Encoder1RM() {
	}

	public Encoder1RM(int uniqueID, int personID, int sessionID, DateTime date, int exerciseID, double load1RM)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.date = date;
		this.exerciseID = exerciseID;
		this.load1RM = load1RM;
	}

	public Encoder1RM(int uniqueID, int personID, int sessionID, DateTime date, int exerciseID, double load1RM, string personName, string exerciseName)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.date = date;
		this.exerciseID = exerciseID;
		this.load1RM = load1RM;
		this.personName = personName;
		this.exerciseName = exerciseName;
	}

	public string [] ToStringArray() {
		string [] s = { uniqueID.ToString(), load1RM.ToString() };
		return s;
	}
	
	public string [] ToStringArray2() {
		string [] s = { uniqueID.ToString(), personName, exerciseName, load1RM.ToString(), date.ToShortDateString() };
		return s;
	}


	~Encoder1RM() {}
}
