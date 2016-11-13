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
 *  Copyright (C) 2016   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections.Generic; //List<T>

public class LastTestTime
{
	private int personID;
	private DateTime time;

	//constructor
	public LastTestTime(int pID)
	{
		this.personID = pID;
		this.time = DateTime.Now;
	}
	
	public void Update()
	{
		time = DateTime.Now;
	}

	public override string ToString()
	{
		return personID.ToString() + ":" + time.ToString();
	}
	
	public string RestedTime
	{
		get {
			TimeSpan ts = DateTime.Now.Subtract(time);
			if(ts.TotalMinutes >= 60)
				return "+60'"; //TODO: check if this is correctly sorted on treeview persons

			//add a 0 if values are <10 to order them correctly on treeview persons
			int m = ts.Minutes;
			int s = ts.Seconds;
			string mStr = m.ToString();
			string sStr = s.ToString();
			if(m < 10)
				mStr = "0" + mStr;
			if(s < 10)
				sStr = "0" + sStr;

			return string.Format("{0}'{1}\"", mStr, sStr);
		}
	}

	public int PersonID
	{
		get { return personID; } 
	}
}

public class RestTime
{
	private List<LastTestTime> list;

	public RestTime()
	{
		//initialize list when Chronojump starts
		list = new List<LastTestTime>();
	}
	
	public string RestedTime(int personID)
	{
		foreach(LastTestTime ltt in list)
			if(ltt.PersonID == personID)
				return ltt.RestedTime;
		
		return "";
	}
	
	public void AddOrModify(int personID, bool print)
	{
		if(exists(personID))
			modifyRestTime(personID);
		else
			addRestTime(personID);

		if(print)
			foreach(LastTestTime ltt in list)
				LogB.Information(ltt.ToString());
	}

	private bool exists(int personID)
	{
		foreach(LastTestTime ltt in list)
			if(ltt.PersonID == personID)
				return true;
		
		return false;
	}
	
	private void addRestTime(int personID)
	{
		list.Add(new LastTestTime(personID));
	}
	
	private void modifyRestTime(int personID)
	{
		foreach(LastTestTime ltt in list)
			if(ltt.PersonID == personID)
				ltt.Update();
	}
}
