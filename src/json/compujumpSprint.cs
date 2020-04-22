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
using System.Collections.Generic; //List<T>


public class UploadSprintDataObject
{
	public int uniqueId; //used for SQL load and delete
	public int personId;
	public string sprintPositions;
	public List<double> splitTimesL;
	public double k;
	public double vmax;
	public double amax;
	public double fmax;
	public double pmax;

	public UploadSprintDataObject (int uniqueId, int personId, string sprintPositions, List<double> splitTimesL,
			double k, double vmax, double amax, double fmax, double pmax)
	{
		this.uniqueId = uniqueId;
		this.personId = personId;
		this.sprintPositions = sprintPositions;
		this.splitTimesL = splitTimesL;
		this.k = k;
		this.vmax = vmax;
		this.amax = amax;
		this.fmax = fmax;
		this.pmax = pmax;
	}

	public string ToSQLInsertString ()
	{
		return
			"NULL, " +
			personId.ToString() + ", " +
			"\"" + sprintPositions + "\", " +
			"\"" + splitTimesLToString() + "\", " +
			Util.ConvertToPoint(k) + ", " +
			Util.ConvertToPoint(vmax) + ", " +
			Util.ConvertToPoint(amax) + ", " +
			Util.ConvertToPoint(fmax) + ", " +
			Util.ConvertToPoint(pmax) + ")";
	}

	public static List<double> SplitTimesStringToList(string sqlSelectSplitTimes)
	{
		List<double> l = new List<double>();
		if(sqlSelectSplitTimes == null || sqlSelectSplitTimes == "")
			return l;

		string [] myStringFull = sqlSelectSplitTimes.Split(new char[] {';'});
		foreach (string time in myStringFull)
			l.Add(Convert.ToDouble(Util.ChangeDecimalSeparator(time)));

		return l;
	}

	private string splitTimesLToString()
	{
		string str = "";
		string sep = "";
		foreach(double d in splitTimesL)
		{
			str += sep + Util.ConvertToPoint(d);
			sep = ";";
		}

		return str;
	}
}

