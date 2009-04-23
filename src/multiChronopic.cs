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
 * Xavier de Blas: 
 */

using System;
using System.Data;
using System.Collections; //ArrayList

public class MultiChronopic : Event 
{
	private int cp1StartedIn;
	private int cp2StartedIn;
	private int cp3StartedIn;
	private int cp4StartedIn;
	private string cp1InStr;
	private string cp1OutStr;
	private string cp2InStr;
	private string cp2OutStr;
	private string cp3InStr;
	private string cp3OutStr;
	private string cp4InStr;
	private string cp4OutStr;

	private ArrayList array;
	private bool arrayDone;

	public MultiChronopic() {
	}

	//after inserting database (SQL)
	public MultiChronopic(int uniqueID, int personID, int sessionID, string type, 
			int cp1StartedIn, int cp2StartedIn, int cp3StartedIn, int cp4StartedIn,
			string cp1InStr, string cp1OutStr,
			string cp2InStr, string cp2OutStr,
			string cp3InStr, string cp3OutStr,
			string cp4InStr, string cp4OutStr,
			string description, int simulated)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.cp1StartedIn = cp1StartedIn;
		this.cp2StartedIn = cp2StartedIn;
		this.cp3StartedIn = cp3StartedIn;
		this.cp4StartedIn = cp4StartedIn;
		this.cp1InStr = cp1InStr;
		this.cp1OutStr = cp1OutStr;
		this.cp2InStr = cp2InStr;
		this.cp2OutStr = cp2OutStr;
		this.cp3InStr = cp3InStr;
		this.cp3OutStr = cp3OutStr;
		this.cp4InStr = cp4InStr;
		this.cp4OutStr = cp4OutStr;
		this.description = description;
		this.simulated = simulated;
	
		arrayDone = false;
	}

	/*
	//used to select a event at SqliteReactionTime.SelectReactionTimeData and at Sqlite.convertTables
	public MultiChronopic(string [] eventString) {
		this.uniqueID = Convert.ToInt32(eventString[0]);
		this.personID = Convert.ToInt32(eventString[1]);
		this.sessionID = Convert.ToInt32(eventString[2]);
		//this.type = eventString[3].ToString();
		this.time = Convert.ToDouble(Util.ChangeDecimalSeparator(eventString[4]));
		this.description = eventString[5].ToString();
		this.simulated = Convert.ToInt32(eventString[6]);
	}
	*/

	public override int InsertAtDB (bool dbconOpened, string tableName) {
		return SqliteMultiChronopic.Insert(dbconOpened, tableName, 
				uniqueID.ToString(), 
				personID, sessionID, 
				"", //type
				cp1StartedIn, cp2StartedIn, cp3StartedIn, cp4StartedIn,
				cp1InStr, cp1OutStr,
				cp2InStr, cp2OutStr,
				cp3InStr, cp3OutStr,
				cp4InStr, cp4OutStr,
				description, simulated);
	}

	public ArrayList AsArrayList() 
	{
		if(arrayDone)
			return array;
		
		//write line for treeview
		//string [] myData = new String [getColsNum()];
		ArrayList returnArray = new ArrayList(1);
		string [] returnLine = new String[20];

		string [] cp1InFull = this.Cp1InStr.Split(new char[] {'='});
		string [] cp1OutFull = this.Cp1OutStr.Split(new char[] {'='});
		string [] cp2InFull = this.Cp2InStr.Split(new char[] {'='});
		string [] cp2OutFull = this.Cp2OutStr.Split(new char[] {'='});
		string [] cp3InFull = this.Cp3InStr.Split(new char[] {'='});
		string [] cp3OutFull = this.Cp3OutStr.Split(new char[] {'='});
		string [] cp4InFull = this.Cp4InStr.Split(new char[] {'='});
		string [] cp4OutFull = this.Cp4OutStr.Split(new char[] {'='});

		bool ended = false;
		bool cp1NextIn = Util.IntToBool(this.Cp1StartedIn);
		bool cp2NextIn = Util.IntToBool(this.Cp2StartedIn);
		bool cp3NextIn = Util.IntToBool(this.Cp3StartedIn);
		bool cp4NextIn = Util.IntToBool(this.Cp4StartedIn);
		double cp1NextTime;
		double cp2NextTime;
		double cp3NextTime;
		double cp4NextTime;
		double runningTime = 0;

		int cp1InCount = 0;
		int cp1OutCount = 0;
		int cp2InCount = 0;
		int cp2OutCount = 0;
		int cp3InCount = 0;
		int cp3OutCount = 0;
		int cp4InCount = 0;
		int cp4OutCount = 0;
		double cp1Sum = 0;
		double cp2Sum = 0;
		double cp3Sum = 0;
		double cp4Sum = 0;

		int lineCount = 0;
		while(! ended) {
			int nextCp = -1;

			/*
			   need the last && mcCp1InStr.Length>0
			   because if string it's empty cp1InFull will be created with lenght 1
			 */ 
			if(cp1NextIn && cp1InFull.Length > cp1InCount && this.Cp1InStr.Length>0)  
				cp1NextTime = Convert.ToDouble(cp1InFull[cp1InCount]) + cp1Sum;
			else if(! cp1NextIn && cp1OutFull.Length > cp1OutCount && this.Cp1OutStr.Length>0) 
				cp1NextTime = Convert.ToDouble(cp1OutFull[cp1OutCount]) + cp1Sum;
			else 
				cp1NextTime = 99999;


			if(cp2NextIn && cp2InFull.Length > cp2InCount && this.Cp2InStr.Length>0) 
				cp2NextTime = Convert.ToDouble(cp2InFull[cp2InCount]) + cp2Sum;
			else if(! cp2NextIn && cp2OutFull.Length > cp2OutCount && this.Cp2OutStr.Length>0) 
				cp2NextTime = Convert.ToDouble(cp2OutFull[cp2OutCount]) + cp2Sum;
			else
				cp2NextTime = 99999;

			if(cp3NextIn && cp3InFull.Length > cp3InCount && this.Cp3InStr.Length>0) 
				cp3NextTime = Convert.ToDouble(cp3InFull[cp3InCount]) + cp3Sum;
			else if(! cp3NextIn && cp3OutFull.Length > cp3OutCount && this.Cp3OutStr.Length>0) 
				cp3NextTime = Convert.ToDouble(cp3OutFull[cp3OutCount]) + cp3Sum;
			else
				cp3NextTime = 99999;

			if(cp4NextIn && cp4InFull.Length > cp4InCount && this.Cp4InStr.Length>0) 
				cp4NextTime = Convert.ToDouble(cp4InFull[cp4InCount]) + cp4Sum;
			else if(! cp4NextIn && cp4OutFull.Length > cp4OutCount && this.Cp4OutStr.Length>0) 
				cp4NextTime = Convert.ToDouble(cp4OutFull[cp4OutCount]) + cp4Sum;
			else
				cp4NextTime = 99999;


			if(cp1NextTime == 99999 && cp2NextTime == 99999 && cp3NextTime == 99999 && cp4NextTime == 99999)
				ended = true;
			else {
				if(cp1NextTime <= cp2NextTime) {
					if(cp1NextTime <= cp3NextTime) {
						if(cp1NextTime <= cp4NextTime) 
							nextCp = 1;
						else 
							nextCp = 4;
					} else {
						if(cp3NextTime <= cp4NextTime) 
							nextCp = 3;
						else 
							nextCp = 4;
					}
				} else {
					if(cp2NextTime <= cp3NextTime) {
						if(cp2NextTime <= cp4NextTime) 
							nextCp = 2;
						else 
							nextCp = 4;
					} else {
						if(cp3NextTime <= cp4NextTime) 
							nextCp = 3;
						else 
							nextCp = 4;
					}
				}

				Console.WriteLine("NEXTCP: " + nextCp);

				int pos=0;
				double thisTime = 0;
				string thisState = Constants.Out;
				if(nextCp == 1) {
					pos = 0;
					if(cp1NextIn)
						cp1InCount ++;
					else
						cp1OutCount ++;
					cp1NextIn = ! cp1NextIn;
					thisTime = cp1NextTime - cp1Sum;;
					cp1Sum += thisTime;
					runningTime = cp1Sum;
					thisState = Util.BoolToInOut(cp1NextIn);
				} else if(nextCp == 2) {
					pos = 1;
					if(cp2NextIn)
						cp2InCount ++;
					else
						cp2OutCount ++;
					cp2NextIn = ! cp2NextIn;
					thisTime = cp2NextTime - cp2Sum;
					cp2Sum += thisTime;
					runningTime = cp2Sum;
					thisState = Util.BoolToInOut(cp2NextIn);
				} else if(nextCp == 3) {
					pos = 2;
					if(cp3NextIn)
						cp3InCount ++;
					else
						cp3OutCount ++;
					cp3NextIn = ! cp3NextIn;
					thisTime = cp3NextTime - cp3Sum;
					cp3Sum += thisTime;
					runningTime = cp3Sum;
					thisState = Util.BoolToInOut(cp3NextIn);
				} else if(nextCp == 4) {
					pos = 3;
					if(cp4NextIn)
						cp4InCount ++;
					else
						cp4OutCount ++;
					cp4NextIn = ! cp4NextIn;
					thisTime = cp4NextTime - cp4Sum;
					cp4Sum += thisTime;
					runningTime = cp4Sum;
					thisState = Util.BoolToInOut(cp4NextIn);
				}


				int count=0;
				returnLine[count++] = (lineCount++).ToString();
				returnLine[count++] = runningTime.ToString();

				for(int i=0; i<8; i++) {
					if(i==pos)
						returnLine[count++] = thisState;
					else if(i == (pos +4))
						returnLine[count++] = thisTime.ToString();
					else
						returnLine[count++] = "";
				}


				for(int i=0; i<8;i++)
					returnLine[count++] = "";

				returnLine[count++] = ""; //description column
				returnLine[count++] = "-1"; //mark to non select here, select first line 
				returnArray.Add(Util.StringArrayToString(returnLine, ":"));
			}
		}
		array = returnArray;
		arrayDone = true;
		return array;
	}
	
	public int Cp1StartedIn {
		get { return cp1StartedIn; }
		set { cp1StartedIn = value; }
	}
	public int Cp2StartedIn {
		get { return cp2StartedIn; }
		set { cp2StartedIn = value; }
	}
	public int Cp3StartedIn {
		get { return cp3StartedIn; }
		set { cp3StartedIn = value; }
	}
	public int Cp4StartedIn {
		get { return cp4StartedIn; }
		set { cp4StartedIn = value; }
	}
	
	public string Cp1InStr {
		get { return cp1InStr; }
		set { cp1InStr = value; }
	}
	public string Cp1OutStr {
		get { return cp1OutStr; }
		set { cp1OutStr = value; }
	}
	
	public string Cp2InStr {
		get { return cp2InStr; }
		set { cp2InStr = value; }
	}
	public string Cp2OutStr {
		get { return cp2OutStr; }
		set { cp2OutStr = value; }
	}

	public string Cp3InStr {
		get { return cp3InStr; }
		set { cp3InStr = value; }
	}
	public string Cp3OutStr {
		get { return cp3OutStr; }
		set { cp3OutStr = value; }
	}
	
	public string Cp4InStr {
		get { return cp4InStr; }
		set { cp4InStr = value; }
	}
	public string Cp4OutStr {
		get { return cp4OutStr; }
		set { cp4OutStr = value; }
	}
	
	

	~MultiChronopic() {}
}
