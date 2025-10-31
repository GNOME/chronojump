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

//related to all reps, not only active
public class EncoderPersonCurvesInDBDeep
{
	public double extraWeight;
	public int count; //this count is all reps (not only active)

	public EncoderPersonCurvesInDBDeep(double w, int c) {
		this.extraWeight = w;
		this.count = c;
	}

	public override string ToString() {
		return count.ToString() + "*" + extraWeight.ToString();// + "kg";
	}
}
public class EncoderPersonCurvesInDB
{
	public int personID;
	public int sessionID;
	public string sessionName;
	public string sessionDate;
	public int countActive;
	public int countAll;
	public List<EncoderPersonCurvesInDBDeep> lDeep;
	
	public EncoderPersonCurvesInDB() {
	}
	public EncoderPersonCurvesInDB(int personID, int sessionID, string sessionName, string sessionDate) 
	{
		this.personID =		personID;
		this.sessionID = 	sessionID;
		this.sessionName = 	sessionName;
		this.sessionDate = 	sessionDate;
	}

	public string [] ToStringArray(bool deep) {
		string [] s;

		//the "" will be for the checkbox on genericWin
		if(deep) {
			s = new string[]{ sessionID.ToString(), "", sessionName, sessionDate,
				//countActive.ToString(), countAll.ToString()
				countAll.ToString(), DeepPrint()
			};
		} else {
			s = new string[]{ sessionID.ToString(), "", sessionName, sessionDate,
				//countActive.ToString(), countAll.ToString()
				countAll.ToString()
			};
		}

		return s;
	}

	private string DeepPrint() {
		string s = "";
		string sep = "";
		foreach(EncoderPersonCurvesInDBDeep e in lDeep) {
			s += sep + e.ToString();
			sep = " ";
		}
		return s;
	}
}
