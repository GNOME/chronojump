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
 * Copyright (C) 2014   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections; //ArrayList

public class ExecuteAuto {
	public enum ModeTypes { BY_PERSONS, BY_TESTS, BY_SERIES }
	private ModeTypes mode;
	
	public int serieID;	//only in BY_SERIES (in BY_PERSONS or BY_TESTS, value is -1)
	public int personUniqueID;
	public string personName;
	public int testUniqueID;
	public string testEName;	//english name
	public string testTrName;	//translated name
	
	public ExecuteAuto(int serieID, int personUniqueID, string personName, 
			int testUniqueID, string testEName, string testTrName) {
		this.serieID = serieID;
		this.personUniqueID = personUniqueID;
		this.personName = personName;
		this.testUniqueID = testUniqueID;
		this.testEName = testEName;
		this.testTrName = testTrName;
	}

	public string [] AsStringArray() {
		if(serieID == -1)
			return new string [] { personName, testTrName };
		else
			return new string [] { serieID.ToString(), personName, testTrName };
	}
	
	public static ArrayList CreateOrder(ModeTypes mode, ArrayList persons, 
			ArrayList comboSerie1Array, ArrayList comboSerie2Array, ArrayList comboSerie3Array) 
	{
	        ArrayList orderArray = new ArrayList(persons.Count);

		if(mode == ModeTypes.BY_PERSONS)
			foreach(Person p in persons)
				foreach(TrCombo tc in comboSerie1Array)
					orderArray.Add(new ExecuteAuto(-1, p.UniqueID, p.Name, tc.id, tc.eName, tc.trName));
		else if(mode == ModeTypes.BY_TESTS)
			foreach(TrCombo tc in comboSerie1Array)
				foreach(Person p in persons)
					orderArray.Add(new ExecuteAuto(-1, p.UniqueID, p.Name, tc.id, tc.eName, tc.trName));
		else {
			//by series
			foreach(Person p in persons)
				foreach(TrCombo tc in comboSerie1Array)
					orderArray.Add(new ExecuteAuto(1, p.UniqueID, p.Name, tc.id, tc.eName, tc.trName));

			foreach(Person p in persons)
				foreach(TrCombo tc in comboSerie2Array)
					orderArray.Add(new ExecuteAuto(2, p.UniqueID, p.Name, tc.id, tc.eName, tc.trName));

			foreach(Person p in persons)
				foreach(TrCombo tc in comboSerie3Array)
					orderArray.Add(new ExecuteAuto(3, p.UniqueID, p.Name, tc.id, tc.eName, tc.trName));
		}

		return orderArray;
	}
	
	//position is needed because person will be moved to the end (but only from current pos to the end, not previous)
	public static ArrayList SkipPerson(ArrayList orderArray, int position, Person p)
	{
		ArrayList returnArray = new ArrayList();
		ArrayList skippedArray = new ArrayList();
		int count = 0;
		foreach (ExecuteAuto ea in orderArray) {
			if(count < position || ea.personUniqueID != p.UniqueID)
				returnArray.Add(ea);
			else
				skippedArray.Add(ea);
			count ++;
		}
		
		foreach (ExecuteAuto ea in skippedArray) {
			returnArray.Add(ea);
		}

		return returnArray;
	}

	//position is needed because person will be removed from current pos to the end (not previous)
	public static ArrayList RemovePerson(ArrayList orderArray, int position, Person p)
	{
		ArrayList returnArray = new ArrayList();
		int count = 0;
		foreach (ExecuteAuto ea in orderArray) {
			if(count < position || ea.personUniqueID != p.UniqueID)
				returnArray.Add(ea);
			count ++;
		}
		return returnArray;
	}

	~ExecuteAuto() {}	
}

