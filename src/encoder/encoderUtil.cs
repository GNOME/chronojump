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


/*
 * class that manages list of diameters on encoderConfiguration
 * read_list_d returns a List<double>. when reading a "" value from SQL is usually converted to 0 here
 * and then a list_d Add will add a new value
 * control list_d values with this class
 * if there are no diameters, list_d has one value: 0
 */
public class List_d
{
	private List<double> l;

	//default constructor
	public List_d()
	{
		l = new List<double>();
		l.Add(0);
	}
	//constructor with a default value
	public List_d(double d)
	{
		l = new List<double>();
		l.Add(d);
	}

	//list_d contains the different diameters (anchorages). They are stored as '_'
	public void ReadFromSQL(string listFromSQL)
	{
		l = new List<double>();
		string [] strFull = listFromSQL.Split(new char[] {'_'});
		foreach (string s in strFull) {
			double d = Convert.ToDouble(Util.ChangeDecimalSeparator(s));
			l.Add(d);
		}

		if(l.Count == 0)
			l.Add(0);
	}

	public void Add(double d)
	{
		if(l.Count == 1 && l[0] == 0)
			l[0] = d;
		else
			l.Add(d);
	}

	public override string ToString()
	{
		string str = "";
		string sep = "";
		foreach(double d in l) {
			str += sep + Util.ConvertToPoint(d);
			sep = "_";
		}

		if(str == "")
			str = "0";

		return str;
	}

	public bool IsEmpty()
	{
		if(l == null || l.Count == 0 || (l.Count == 1 && l[0] == 0) )
			return true;

		return false;
	}

	public List<double> L
	{
		get { return l; }
	}

}


//for objects coming from R that have "x1 x2 y1 y2" like usr or par
public class Rx1y2 
{
	public double x1;
	public double x2;
	public double y1;
	public double y2;

	public Rx1y2 (string s) {
		string [] sFull = s.Split(new char[] {' '});
		x1 = Convert.ToDouble(Util.ChangeDecimalSeparator(sFull[0]));
		x2 = Convert.ToDouble(Util.ChangeDecimalSeparator(sFull[1]));
		y1 = Convert.ToDouble(Util.ChangeDecimalSeparator(sFull[2]));
		y2 = Convert.ToDouble(Util.ChangeDecimalSeparator(sFull[3]));
	}
}
