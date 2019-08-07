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
 *  Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
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
	private string vars; //distance at runAnalysis (unused currently on multiChronopic default)

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
			string vars, string description, int simulated)
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
		this.vars = vars;
		this.description = description;
		this.simulated = simulated;
	
		arrayDone = false;
	}
	
	//used to select a event at SqliteMultiChronopic.SelectMultiChronopicData and at Sqlite.convertTables
	//Util.ConvertToPointIfNeeded is used because multichronopic data is recorded by mistake as ',' instead of '.' on database
	public MultiChronopic(string [] eventString) {
		this.uniqueID = Convert.ToInt32(eventString[0]);
		this.personID = Convert.ToInt32(eventString[1]);
		this.sessionID = Convert.ToInt32(eventString[2]);
		this.type = eventString[3].ToString();
		this.cp1StartedIn = Convert.ToInt32(eventString[4]);
		this.cp2StartedIn = Convert.ToInt32(eventString[5]);
		this.cp3StartedIn = Convert.ToInt32(eventString[6]);
		this.cp4StartedIn = Convert.ToInt32(eventString[7]);
		this.cp1InStr  = Util.ConvertToPointIfNeeded(eventString[8]);
		this.cp1OutStr = Util.ConvertToPointIfNeeded(eventString[9]);
		this.cp2InStr  = Util.ConvertToPointIfNeeded(eventString[10]);
		this.cp2OutStr = Util.ConvertToPointIfNeeded(eventString[11]);
		this.cp3InStr  = Util.ConvertToPointIfNeeded(eventString[12]);
		this.cp3OutStr = Util.ConvertToPointIfNeeded(eventString[13]);
		this.cp4InStr  = Util.ConvertToPointIfNeeded(eventString[14]);
		this.cp4OutStr = Util.ConvertToPointIfNeeded(eventString[15]);
		this.vars = eventString[16];
		this.description = eventString[17];
		this.simulated = Convert.ToInt32(eventString[18]);
	}


	public override int InsertAtDB (bool dbconOpened, string tableName) {
		return SqliteMultiChronopic.Insert(dbconOpened, tableName, 
				uniqueID.ToString(), 
				personID, sessionID, 
				type, 
				cp1StartedIn, cp2StartedIn, cp3StartedIn, cp4StartedIn,
				cp1InStr, cp1OutStr,
				cp2InStr, cp2OutStr,
				cp3InStr, cp3OutStr,
				cp4InStr, cp4OutStr,
				vars, description, simulated);
	}

	public ArrayList AsArrayList(int pDN) 
	{
		if(arrayDone)
			return array;

		if(type == Constants.RunAnalysisName)
			return asArrayListRunA(pDN);
		else
			return asArrayListDefault(pDN);
	}
		
	public double GetTimeRunA() {
		string [] cp1InFull = this.Cp1InStr.Split(new char[] {'='});
		string [] cp1OutFull = this.Cp1OutStr.Split(new char[] {'='});

		return Convert.ToDouble(cp1InFull[0]) + Convert.ToDouble(cp1OutFull[1]);
	}
	
	public double GetAVGSpeedRunA() {
		double runADistanceMeters = Convert.ToDouble(vars) / 100;
		return runADistanceMeters / GetTimeRunA();
	}
	
	public ArrayList asArrayListRunA(int pDN) 
	{
		ArrayList returnArray = new ArrayList(1);
		
		string [] returnLine = new String[20];
	
		string [] cp2InFull = this.Cp2InStr.Split(new char[] {'='});
		string [] cp2OutFull = this.Cp2OutStr.Split(new char[] {'='});

		double avgSpeed = GetAVGSpeedRunA();
						
		for(int lineCount=0; lineCount < cp2InFull.Length; lineCount++) {
			int count=0;
			double tc = Convert.ToDouble(cp2InFull[lineCount]);
			double tf = Convert.ToDouble(cp2OutFull[lineCount]);
			double totalTime = tc + tf;
			double freq = 1 / totalTime; 
			double width = avgSpeed / freq; 
			double height = 1.22 * System.Math.Pow (tf, 2);
			double angle = System.Math.Atan(System.Math.Sqrt(2 * 9.81 * height) / avgSpeed ) * 180 / System.Math.PI;
			returnLine[count++] = (lineCount+1).ToString();
			returnLine[count++] = Util.TrimDecimals(tc, pDN);
			returnLine[count++] = Util.TrimDecimals(tf, pDN);
			returnLine[count++] = Util.TrimDecimals(totalTime, pDN);
			returnLine[count++] = Util.TrimDecimals(freq, pDN);
			returnLine[count++] = Util.TrimDecimals(width, pDN);
			returnLine[count++] = Util.TrimDecimals(height, pDN);
			returnLine[count++] = Util.TrimDecimals(angle, pDN);

			for(int i=0; i < 10; i++)
				returnLine[count++] = "";

			returnLine[count++] = ""; //description column (unused because this array if for eg. treeview subLines)
			returnLine[count++] = "-1"; //mark to non select here, select first line 
			returnArray.Add(Util.StringArrayToString(returnLine, ":"));
		}
		array = returnArray;
		arrayDone = true;
		return array;
	}
		
		
	public ArrayList asArrayListDefault(int pDN) 
	{
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
						
		double rt1InRecorded = 0;
		double rt1OutRecorded = 0;
		double rt2InRecorded = 0;
		double rt2OutRecorded = 0;
		double rt3InRecorded = 0;
		double rt3OutRecorded = 0;
		double rt4InRecorded = 0;
		double rt4OutRecorded = 0;
		
		double iibefore = -1;
		double oobefore = -1;

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

				int pos=0;
				double thisTime = 0;
				string thisState = Constants.OutStr();
				iibefore = -1;
				oobefore = -1;
				pos = nextCp -1;

				if(nextCp == 1) 
				{
					thisTime = cp1NextTime - cp1Sum;
					cp1Sum += thisTime;
					runningTime = cp1Sum;
					if(cp1NextIn) {
						cp1InCount ++;
						if( ! (rt1OutRecorded == 0 && Util.IntToBool(this.cp1StartedIn)) )
							oobefore = runningTime - rt1OutRecorded; //runningTime minus runningtime recorded at last out
						rt1OutRecorded = runningTime;
					} else {
						cp1OutCount ++;
						if( ! (rt1InRecorded == 0 && ! Util.IntToBool(this.cp1StartedIn)) )
							iibefore = runningTime - rt1InRecorded; //runningTime minus runningtime recorded at last in
						rt1InRecorded = runningTime;
					}
					cp1NextIn = ! cp1NextIn;
					thisState = Util.BoolToInOut(cp1NextIn);
				} 
				else if(nextCp == 2) 
				{
					thisTime = cp2NextTime - cp2Sum;
					cp2Sum += thisTime;
					runningTime = cp2Sum;
					if(cp2NextIn) {
						cp2InCount ++;
						if( ! (rt2OutRecorded == 0 && Util.IntToBool(this.cp2StartedIn)) )
							oobefore = runningTime - rt2OutRecorded; //runningTime minus runningtime recorded at last out
						rt2OutRecorded = runningTime;
					} else {
						cp2OutCount ++;
						if( ! (rt2InRecorded == 0 && ! Util.IntToBool(this.cp2StartedIn)) )
							iibefore = runningTime - rt2InRecorded; //runningTime minus runningtime recorded at last in
						rt2InRecorded = runningTime;
					}
					cp2NextIn = ! cp2NextIn;
					thisState = Util.BoolToInOut(cp2NextIn);
				} 
				else if(nextCp == 3) 
				{
					thisTime = cp3NextTime - cp3Sum;
					cp3Sum += thisTime;
					runningTime = cp3Sum;
					if(cp3NextIn) {
						cp3InCount ++;
						if( ! (rt3OutRecorded == 0 && Util.IntToBool(this.cp3StartedIn)) )
							oobefore = runningTime - rt3OutRecorded; //runningTime minus runningtime recorded at last out
						rt3OutRecorded = runningTime;
					} else {
						cp3OutCount ++;
						if( ! (rt3InRecorded == 0 && ! Util.IntToBool(this.cp3StartedIn)) )
							iibefore = runningTime - rt3InRecorded; //runningTime minus runningtime recorded at last in
						rt3InRecorded = runningTime;
					}
					cp3NextIn = ! cp3NextIn;
					thisState = Util.BoolToInOut(cp3NextIn);
				} 
				else if(nextCp == 4) 
				{
					thisTime = cp4NextTime - cp4Sum;
					cp4Sum += thisTime;
					runningTime = cp4Sum;
					if(cp4NextIn) {
						cp4InCount ++;
						if( ! (rt4OutRecorded == 0 && Util.IntToBool(this.cp4StartedIn)) )
							oobefore = runningTime - rt4OutRecorded; //runningTime minus runningtime recorded at last out
						rt4OutRecorded = runningTime;
					} else {
						cp4OutCount ++;
						if( ! (rt4InRecorded == 0 && ! Util.IntToBool(this.cp4StartedIn)) )
							iibefore = runningTime - rt4InRecorded; //runningTime minus runningtime recorded at last in
						rt4InRecorded = runningTime;
					}
					cp4NextIn = ! cp4NextIn;
					thisState = Util.BoolToInOut(cp4NextIn);
				}


				int count=0;
				returnLine[count++] = (++lineCount).ToString();
				returnLine[count++] = Util.TrimDecimals(runningTime.ToString(), pDN);

				for(int i=0; i<16; i++) {
					if(i==pos)
						returnLine[count++] = thisState;
					else if(i == (pos +4))
						returnLine[count++] = Util.TrimDecimals(thisTime.ToString(), pDN);
					else if(i == (pos +8) && iibefore != -1)
						returnLine[count++] = Util.TrimDecimals(iibefore.ToString(), pDN);
					else if(i == (pos +12) && oobefore != -1)
						returnLine[count++] = Util.TrimDecimals(oobefore.ToString(), pDN);
					else
						returnLine[count++] = "";
				}

				returnLine[count++] = ""; //description column (unused because this array if for eg. treeview subLines)
				returnLine[count++] = "-1"; //mark to non select here, select first line 
				returnArray.Add(Util.StringArrayToString(returnLine, ":"));
			}
		}
		array = returnArray;
		arrayDone = true;
		return array;
	}
	
	public string [] Statistics(bool averageOrSD, int pDN) { //if averageOrSD is false, then SD
		ArrayList array = this.AsArrayList(pDN);
		string cp1iiStr = "";
		string cp2iiStr = "";
		string cp3iiStr = "";
		string cp4iiStr = "";
		string cp1ooStr = "";
		string cp2ooStr = "";
		string cp3ooStr = "";
		string cp4ooStr = "";
		string sep1ii = "";
		string sep1oo = "";
		string sep2ii = "";
		string sep2oo = "";
		string sep3ii = "";
		string sep3oo = "";
		string sep4ii = "";
		string sep4oo = "";
		int col = 10; //in-in cp1

		foreach(string str in array) {
			string [] strFull = str.Split(new char[] {':'});
			if(strFull[col] != "") {
				cp1iiStr += sep1ii + strFull[col];
				sep1ii = "=";
			}
			if(strFull[col+1] != "") {
				cp2iiStr += sep2ii + strFull[col+1];
				sep2ii = "=";
			}
			if(strFull[col+2] != "") {
				cp3iiStr += sep3ii + strFull[col+2];
				sep3ii = "=";
			}
			if(strFull[col+3] != "") {
				cp4iiStr += sep4ii + strFull[col+3];
				sep4ii = "=";
			}
			if(strFull[col+4] != "") {
				cp1ooStr += sep1oo + strFull[col+4];
				sep1oo = "=";
			}
			if(strFull[col+5] != "") {
				cp2ooStr += sep2oo + strFull[col+5];
				sep2oo = "=";
			}
			if(strFull[col+6] != "") {
				cp3ooStr += sep3oo + strFull[col+6];
				sep3oo = "=";
			}
			if(strFull[col+7] != "") {
				cp4ooStr += sep4oo + strFull[col+7];
				sep4oo = "=";
			}
		}

		string [] myData = new String [8];
		int count = 0;
		myData[count++] = getIndex(averageOrSD, cp1iiStr);
		myData[count++] = getIndex(averageOrSD, cp2iiStr);
		myData[count++] = getIndex(averageOrSD, cp3iiStr);
		myData[count++] = getIndex(averageOrSD, cp4iiStr);
		
		myData[count++] = getIndex(averageOrSD, cp1ooStr);
		myData[count++] = getIndex(averageOrSD, cp2ooStr);
		myData[count++] = getIndex(averageOrSD, cp3ooStr);
		myData[count] = getIndex(averageOrSD, cp4ooStr);

		return myData;
	}

	private string getIndex(bool averageOrSD, string str) {
		if(averageOrSD)
			return Util.GetAverage(str).ToString();
		else 
			return Util.CalculateSD(
					Util.ChangeEqualForColon(str),
					Util.GetTotalTime(str),
					Util.GetNumberOfJumps(str, false)).ToString();
	}

	public int CPs() {
		if(cp3InStr == "" && cp3OutStr == "")
			return 2;
		else if(cp4InStr == "" && cp4OutStr == "")
			return 3;
		else
			return 4;
	}

	public string GetCPsString () {
		string cpsStr = "";
		string sep = "";
		if(this.cp1InStr.Length + this.cp1OutStr.Length > 0) {
			cpsStr += sep + "1";
			sep = ", ";
		}
		if(this.cp2InStr.Length + this.cp2OutStr.Length > 0) {
			cpsStr += sep + "2";
			sep = ", ";
		}
		if(CPs() >= 3 && this.cp3InStr.Length + this.cp3OutStr.Length > 0) {
			cpsStr += sep + "3";
			sep = ", ";
		}
		if(CPs() == 4 && this.cp4InStr.Length + this.cp4OutStr.Length > 0) {
			cpsStr += sep + "4";
			sep = ", ";
		}
		return cpsStr;
	}

	/*
	   we pass maxCPs because treeviewMultiChronopic will use maxCPs in session (from sqliteMultiChronopic)
	   but exportSession will use mc.CPs (cps of this multiChronopic)
	   */
	public string [] DeleteCols(string [] s1, int maxCPs, bool deleteSubRowId) {
		if(type == Constants.RunAnalysisName)
			return deleteColsRunA(s1, maxCPs, deleteSubRowId);
		else
			return deleteColsDefault(s1, maxCPs, deleteSubRowId);
	}

	//export session passes a string instead of a stringArray
	public string DeleteCols(string s1, int maxCPs, bool deleteSubRowId) {
		string [] strArr = s1.Split(new char[] {':'});
		strArr = DeleteCols(strArr, maxCPs, deleteSubRowId);
		return Util.StringArrayToString(strArr, ":");
	}
	

	private string [] deleteColsRunA(string [] s1, int maxCPs, bool deleteSubRowId) {
		/*
		   deleteSubRowId is reffered to the "-1" at end of each row in treeview
		   this is not used in exportSession, then this bools allows to delete it
		   */

		if(deleteSubRowId)
			s1[19] = "";

		if(maxCPs == 2) {
			string [] s2 = new String[11+1];
			for(int i=0, count=0; i < s1.Length; i++) {
				if(i < 10 || i > 17)
					s2[count++] = s1[i];
			}
			return s2;
		}
		else if(maxCPs == 3) {  
			string [] s2 = new String[15+1];
			for(int i=0, count=0; i < s1.Length; i++) {
				if(i < 14 || i > 17)
					s2[count++] = s1[i];
			}
			return s2;
		}
		else //maxCPs == 4
			return s1;
	}
	

	private string [] deleteColsDefault(string [] s1, int maxCPs, bool deleteSubRowId) {
		/*
		   deleteSubRowId is reffered to the "-1" at end of each row in treeview
		   this is not used in exportSession, then this bools allows to delete it
		   */

		if(deleteSubRowId)
			s1[19] = "";

		if(maxCPs == 2) {
			string [] s2 = new String[11+1];
			for(int i=0, count=0; i < s1.Length; i++) {
				if(i != 4 && i != 5 && i != 8 && i != 9 && i != 12 && i != 13 && i != 16 && i != 17)
					s2[count++] = s1[i];
			}
			return s2;
		}
		else if(maxCPs == 3) {
			string [] s2 = new String[15+1];
			for(int i=0, count=0; i < s1.Length; i++) {
				if(i != 5 && i != 9 && i != 13 && i != 17)
					s2[count++] = s1[i];
			}
			return s2;
		}
		else //maxCPs == 4
			return s1;
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
	
	public string Vars {
		get { return vars; }
		set { vars = value; }
	}
	
	

	~MultiChronopic() {}
}
