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
 *  Copyright (C) 2016-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections.Generic; //List<T>

//TODO: new code should use Timespan to make operations with minutes, seconds ...
public class LastTestTime
{
	private int personID;
	private string personName;
	private DateTime time;

	//constructor
	public LastTestTime(int pID, string pName)
	{
		this.personID = pID;
		this.personName = pName;
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

	const string maxTimeString = "+60'";

	public string RestedTime
	{
		get {
			TimeSpan ts = DateTime.Now.Subtract(time);
			if(ts.TotalMinutes >= 60)
				return maxTimeString;

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

	//better a double because an int on > 30" marks as 1 minute
	public double GetTotalMinutes()
	{
		TimeSpan ts = DateTime.Now.Subtract(time);
		return ts.TotalMinutes;
	}

	public static int GetSeconds(string restedTime)
	{
		if(restedTime == null || restedTime == "")
			return 0;
		if(restedTime == maxTimeString)
			return 3600;

		int seconds = 0;
		try {
			//comes 02'10" 	Substring(0,2) will read minutes 	Substring(3,2) will read seconds
			seconds = ( 60 * Convert.ToInt32(restedTime.Substring(0,2)) ) + Convert.ToInt32(restedTime.Substring(3,2));
		} catch {
			return 0;
		}
		return seconds;
	}

	public int PersonID
	{
		get { return personID; } 
	}

	public string PersonName
	{
		get { return personName; }
	}
}

public class RestTime
{
	private List<LastTestTime> listAll;
	private List<LastTestTime> listLastMin; //for Compujump, list of last 20' on the top

	public RestTime()
	{
		//initialize list when Chronojump starts
		listAll = new List<LastTestTime>();
		listLastMin = new List<LastTestTime>();
	}
	
	public string RestedTime(int personID)
	{
		foreach(LastTestTime ltt in listAll)
			if(ltt.PersonID == personID)
				return ltt.RestedTime;
		
		return "";
	}

	public bool CompujumpPersonNeedLogout(int personID)
	{
		foreach(LastTestTime ltt in listAll)
			if(ltt.PersonID == personID)
			{
				if(LastTestTime.GetSeconds(ltt.RestedTime) > 180) //3 min
					return true;
				else
					return false;
			}

		//person has not done any test. DateTime.Now.Subtract(currentPersonCompujumpLoginTime).TotalMinutes will decide
		return true;
	}

	public void AddOrModify(int personID, string personName, bool print)
	{
		//listAll
		if(exists(listAll, personID))
			modifyRestTime(listAll, personID);
		else
			addRestTime(listAll, personID, personName);

		if(print)
			foreach(LastTestTime ltt in listAll)
				LogB.Information(ltt.ToString());

		//listLastMin
		if(exists(listLastMin, personID))
			modifyRestTime(listLastMin, personID);
		else {
			//add but have only five values
			if(listLastMin.Count == 5)
			{
				int secondsMax = 0;
				int seconds;
				int highestTimePos = 0;
				for (int i = listLastMin.Count - 1; i >= 0; i--)
				{
					seconds = LastTestTime.GetSeconds(listLastMin[i].RestedTime);
					if(seconds > secondsMax)
					{
						secondsMax = seconds;
						highestTimePos = i;
					}
				}
				listLastMin.RemoveAt(highestTimePos);
			}

			//add the new value
			addRestTime(listLastMin, personID, personName);
		}
	}

	private bool exists(List<LastTestTime> l, int personID)
	{
		foreach(LastTestTime ltt in l)
			if(ltt.PersonID == personID)
				return true;
		
		return false;
	}
	
	private void addRestTime(List<LastTestTime> l, int personID, string personName)
	{
		l.Add(new LastTestTime(personID, personName));
	}
	
	private void modifyRestTime(List<LastTestTime> l, int personID)
	{
		foreach(LastTestTime ltt in l)
			if(ltt.PersonID == personID)
				ltt.Update();
	}

	public List<LastTestTime> LastMinList()
	{
		//remove reverse in order to not hung the program on removing while iteratin
		for (int i = listLastMin.Count - 1; i >= 0; i--)
		{
			if(listLastMin[i].GetTotalMinutes() > 10 )//10 minutes
				listLastMin.RemoveAt(i);
		}

		return listLastMin;
	}
}
