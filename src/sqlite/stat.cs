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
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
 */

using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
using Mono.Data.SqliteClient;
using System.Data.SqlClient;


class SqliteStat : Sqlite
{
	//sj, cmj, abk (no sj+)
	public static ArrayList SjCmjAbk (string sessionString, bool multisession, string ini, string end, string jumpType, bool showSex)
	{
		string orderByString = "ORDER BY ";
		string moreSelect = "";
		moreSelect = ini + "jump.tv" + end;

		//if we use AVG or MAX, then we have to group by the results
		//if there's more than one session, it sends the avg or max
		string groupByString = "";
		if (ini.Length > 0) {
			groupByString = " GROUP BY jump.personID, jump.sessionID ";
		}
		//if multisession, order by person.name, sessionID for being able to present results later
		if(multisession) {
			orderByString = orderByString + "person.name, sessionID, ";
		}
		
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.name, person.sex, sessionID, " + moreSelect +
			" FROM jump, person " +
			sessionString +
			" AND jump.type == '" + jumpType + "' " +
			" AND jump.personID == person.uniqueID " +
			groupByString +
			orderByString + ini + "jump.tv" + end + " DESC ";

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		string showSexString = "";
		string returnSessionString = "";
		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			if(showSex) {
				showSexString = " (" + reader[1].ToString() + ")";
			}
			if(multisession) {
				returnSessionString = ":" + reader[2].ToString();
			}
			myArray.Add (reader[0].ToString() + showSexString +
					returnSessionString + ":" + 		//session
					reader[3].ToString() 			//tv
				    );
		}
		reader.Close();
		dbcon.Close();
		return myArray;
	}
	
	//sj+, cmj+, abk+
	public static ArrayList SjCmjAbkPlus (string sessionString, bool multisession, string ini, string end, string jumpType, bool showSex, bool weightPercent)
	{
		string orderByString = "ORDER BY ";
		string moreSelect = "";
		moreSelect = ini + "jump.tv" + end + ", jump.weight, person.weight";

		//if we use AVG or MAX, then we have to group by the results
		//if there's more than one session, it sends the avg or max, no more columns
		//there's no chance of mixing tv and weight of different jumps in multisessions because only tv is returned
		string groupByString = "";
		if (ini.Length > 0) {
			groupByString = " GROUP BY jump.personID, jump.sessionID ";
		}
		//if multisession, order by person.name, sessionID for being able to present results later
		if(multisession) {
			orderByString = orderByString + "person.name, sessionID, ";
		}
		
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.name, person.sex, sessionID, " + moreSelect +
			" FROM jump, person " +
			sessionString +
			" AND jump.type == '" + jumpType + "' " +
			" AND jump.personID == person.uniqueID " +
			groupByString +
			orderByString + ini + "jump.tv" + end + " DESC ";

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		string showSexString = "";
		string returnSessionString = "";
		string returnWeightString = "";
		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			if(showSex) {
				showSexString = " (" + reader[1].ToString() + ")";
			}
			if(multisession) {
				returnSessionString = ":" + reader[2].ToString();
			} else {
				//in multisession we show only one column x session
				//in simplesession we show all
				//FIXME: convert this to an integer (with percent or kg, depending on bool percent)
				
				returnWeightString = ":" + convertWeight(
						reader[4].ToString(), Convert.ToInt32(reader[5].ToString()), weightPercent
						);
			}
			myArray.Add (reader[0].ToString() + showSexString +
					returnSessionString + ":" + 		//session
					reader[3].ToString() +			//tv
					returnWeightString 			//weight
				    );
		}
		reader.Close();
		dbcon.Close();
		return myArray;
	}

	private static string convertWeight (string jumpW, int personW, bool percentDesired) {
		int i;
		bool percentFound;
		for (i=0 ; i< jumpW.Length ; i ++) {
			if (jumpW[i] == '%') {
				percentFound = true;
				break;
			} else if (jumpW[i] == 'K') {
				percentFound = false;
				break;
			}
		}
		if(percentFound == percentDesired) {
			//(found a percent, and wanted percent) or (found kg and wanted kg)
			return jumpW.Substring(0,i);
		} else if(percentFound && ! percentDesired) {
			//found a percent, but we wanted Kg
			return (Convert.ToInt32(jumpW.Substring(0,i))*personW/100).ToString();
		} else if( ! percentFound && percentDesired) {
			//found Kg, but wanted percent
			return (Convert.ToInt32(jumpW.Substring(0,i))*100/personW).ToString();
		} else {
			return "ERROR";
		}
	}

	//dj (not index)
	public static ArrayList Dj (string sessionString, bool multisession, string ini, string end, bool showSex)
	{
		string orderByString = "ORDER BY ";
		string moreSelect = "";
		moreSelect = ini + "jump.tv" + end + ", jump.tc, jump.fall";

		//if we use AVG or MAX, then we have to group by the results
		//if there's more than one session, it sends the avg or max
		string groupByString = "";
		if (ini.Length > 0) {
			groupByString = " GROUP BY jump.personID, jump.sessionID ";
		}
		//if multisession, order by person.name, sessionID for being able to present results later
		if(multisession) {
			orderByString = orderByString + "person.name, sessionID, ";
		}
		
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.name, person.sex, sessionID, " + moreSelect +
			" FROM jump, person " +
			sessionString +
			" AND jump.type == 'DJ' " +
			" AND jump.personID == person.uniqueID " +
			groupByString +
			orderByString + ini + "jump.tv" + end + " DESC ";

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		string showSexString = "";
		string returnSessionString = "";
		string returnTcString = "";
		string returnFallString = "";
		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			if(showSex) {
				showSexString = " (" + reader[1].ToString() + ")";
			}
			if(multisession) {
				returnSessionString = ":" + reader[2].ToString();
			} else {
				//in multisession we show only one column x session
				//in simplesession we show all
				//FIXME: convert this to an integer (with percent or kg, depending on bool percent)
				
				returnTcString = ":" + reader[4].ToString();
				returnFallString = ":" + reader[5].ToString();
			}
			myArray.Add (reader[0].ToString() + showSexString +
					returnSessionString + ":" + 		//session
					reader[3].ToString() +			//tv
					returnTcString + 			//tc
					returnFallString			//fall
				    );
		}
		reader.Close();
		dbcon.Close();
		return myArray;
	}

	//dj index 
	public static ArrayList DjIndex (string sessionString, bool multisession, string ini, string end, bool showSex)
	{
		string orderByString = "ORDER BY ";
		string moreSelect = "";
		moreSelect = ini + "((tv-tc)*100/tc)" + end + " AS dj_index, jump.tv, jump.tc, jump.fall";

		//if we use AVG or MAX, then we have to group by the results
		//if there's more than one session, it sends the avg or max
		string groupByString = "";
		if (ini.Length > 0) {
			groupByString = " GROUP BY jump.personID, jump.sessionID ";
		}
		//if multisession, order by person.name, sessionID for being able to present results later
		if(multisession) {
			orderByString = orderByString + "person.name, sessionID, ";
		}
		
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.name, person.sex, sessionID, " + moreSelect +
			" FROM jump, person " +
			sessionString +
			" AND jump.type == 'DJ' " +
			" AND jump.personID == person.uniqueID " +
			groupByString +
			orderByString + " dj_index DESC, " + ini + "jump.tv" + end + " DESC ";

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		string showSexString = "";
		string returnSessionString = "";
		string returnTvString = "";
		string returnTcString = "";
		string returnFallString = "";
		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			if(showSex) {
				showSexString = " (" + reader[1].ToString() + ")";
			}
			if(multisession) {
				returnSessionString = ":" + reader[2].ToString();
			} else {
				//in multisession we show only one column x session
				//in simplesession we show all
				//FIXME: convert this to an integer (with percent or kg, depending on bool percent)
				
				returnTvString = ":" + reader[4].ToString();
				returnTcString = ":" + reader[5].ToString();
				returnFallString = ":" + reader[6].ToString();
			}
			myArray.Add (reader[0].ToString() + showSexString +
					returnSessionString + ":" + 		//session
					reader[3].ToString() +			//index
					returnTvString + 			//tv
					returnTcString + 			//tc
					returnFallString			//fall
				    );
		}
		reader.Close();
		dbcon.Close();
		return myArray;
	}

	//rj index 
	public static ArrayList rjIndex (string sessionString, bool multisession, string ini, string end, bool showSex)
	{
		string orderByString = "ORDER BY ";
		string moreSelect = "";
		moreSelect = ini + "((tvavg-tcavg)*100/tcavg)" + end + " AS rj_index, tvavg, tcavg, fall";

		//if we use AVG or MAX, then we have to group by the results
		//if there's more than one session, it sends the avg or max
		string groupByString = "";
		if (ini.Length > 0) {
			groupByString = " GROUP BY jumpRj.personID, jumpRj.sessionID ";
		}
		//if multisession, order by person.name, sessionID for being able to present results later
		if(multisession) {
			orderByString = orderByString + "person.name, jumpRj.sessionID, ";
		}
		
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.name, person.sex, sessionID, " + moreSelect +
			" FROM jumpRj, person " +
			sessionString +
			" AND type == 'RJ' " +
			" AND jumpRj.personID == person.uniqueID " +
			groupByString +
			orderByString + " rj_index DESC, tvavg DESC ";

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		string showSexString = "";
		string returnSessionString = "";
		string returnTvString = "";
		string returnTcString = "";
		string returnFallString = "";
		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			if(showSex) {
				showSexString = " (" + reader[1].ToString() + ")";
			}
			if(multisession) {
				returnSessionString = ":" + reader[2].ToString();
			} else {
				//in multisession we show only one column x session
				//in simplesession we show all
				//FIXME: convert this to an integer (with percent or kg, depending on bool percent)
				
				returnTvString = ":" + reader[4].ToString();
				returnTcString = ":" + reader[5].ToString();
				returnFallString = ":" + reader[6].ToString();
			}
			myArray.Add (reader[0].ToString() + showSexString +
					returnSessionString + ":" + 		//session
					reader[3].ToString() +			//index
					returnTvString + 			//tv
					returnTcString + 			//tc
					returnFallString			//fall
				    );
		}
		reader.Close();
		dbcon.Close();
		return myArray;
	}

	//rj index 
	public static ArrayList rjPotencyAguado (string sessionString, bool multisession, string ini, string end, bool showSex)
	{
		string orderByString = "ORDER BY ";
		string moreSelect = "";
		moreSelect = ini + "9.81*9.81 * tvavg*jumps * time / ( 4 * jumps * (time - tvavg*jumps) )" + end + " AS potency, " +
			 " tvavg, tcavg, jumps, time, fall";

		//if we use AVG or MAX, then we have to group by the results
		//if there's more than one session, it sends the avg or max
		string groupByString = "";
		if (ini.Length > 0) {
			groupByString = " GROUP BY jumpRj.personID, jumpRj.sessionID ";
		}
		//if multisession, order by person.name, sessionID for being able to present results later
		if(multisession) {
			orderByString = orderByString + "person.name, jumpRj.sessionID, ";
		}
		
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.name, person.sex, sessionID, " + moreSelect +
			" FROM jumpRj, person " +
			sessionString +
			" AND type == 'RJ' " +
			" AND jumpRj.personID == person.uniqueID " +
			groupByString +
			orderByString + " potency DESC, tvavg DESC ";

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		string showSexString = "";
		string returnSessionString = "";
		string returnTvString = "";
		string returnTcString = "";
		string returnJumpsString = "";
		string returnTimeString = "";
		string returnFallString = "";
		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			if(showSex) {
				showSexString = " (" + reader[1].ToString() + ")";
			}
			if(multisession) {
				returnSessionString = ":" + reader[2].ToString();
			} else {
				//in multisession we show only one column x session
				//in simplesession we show all
				//FIXME: convert this to an integer (with percent or kg, depending on bool percent)
				
				returnTvString = ":" + reader[4].ToString();
				returnTcString = ":" + reader[5].ToString();
				returnJumpsString = ":" + reader[6].ToString();
				returnTimeString = ":" + reader[7].ToString();
				returnFallString = ":" + reader[8].ToString();
			}
			myArray.Add (reader[0].ToString() + showSexString +
					returnSessionString + ":" + 		//session
					reader[3].ToString() +			//index
					returnTvString + 			//tv
					returnTcString + 			//tc
					returnJumpsString +			//jumps
					returnTimeString +			//time
					returnFallString			//fall
				    );
		}
		reader.Close();
		dbcon.Close();
		return myArray;
	}

	public static ArrayList IeIub (string sessionString, bool multisession, string ini, string end, string jump1, string jump2, bool showSex)
	{
		//if we search AVG, show AVG of index, and jump1 and jump2
		//if we search MAX, show MAX of index, MAX of jump1 and MIN of jump2
		string ini2 = "";
		if(ini == "MAX(") {
			ini2 = "MIN(";
		} else if (ini == "AVG("){
			ini2 = "AVG(";
		}
			
		string orderByString = "ORDER BY ";
		string moreSelect = "";
		//moreSelect = "((" + ini + "j1.tv" + end + "-" + ini + "j2.tv" + end + ")*100/" + ini + "j2.tv" + end + ") AS myIndex, " +
			//ini + "j1.tv" + end + ", " + ini + "j2.tv" + end;
		moreSelect = ini + "(j1.tv-j2.tv)*100/j2.tv" + end + " AS myIndex, " +
			ini + "j1.tv" + end + ", " + ini2 + "j2.tv" + end;

		//if we use AVG or MAX, then we have to group by the results
		//if there's more than one session, it sends the avg or max
		string groupByString = "";
		if (ini.Length > 0) {
			groupByString = " GROUP BY j1.personID, j1.sessionID ";
		}
		//if multisession, order by person.name, sessionID for being able to present results later
		if(multisession) {
			orderByString = orderByString + "person.name, j1.sessionID, ";
		}
		
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.name, person.sex, j1.sessionID, " + moreSelect +
			" FROM jump AS j1, jump AS j2, person " +
			sessionString +
			" AND j1.type == '" + jump1 + "' " +
			" AND j2.type == '" + jump2 + "' " +
			" AND j1.personID == person.uniqueID " +
			" AND j2.personID == person.uniqueID " +
			groupByString +
			orderByString + " myIndex DESC ";

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		string showSexString = "";
		string returnSessionString = "";
		string returnJump1String = "";
		string returnJump2String = "";
		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			if(showSex) {
				showSexString = " (" + reader[1].ToString() + ")";
			}
			if(multisession) {
				returnSessionString = ":" + reader[2].ToString();
			} else {
				//in multisession we show only one column x session
				//in simplesession we show all
				//FIXME: convert this to an integer (with percent or kg, depending on bool percent)
				
				returnJump1String = ":" + reader[4].ToString();
				returnJump2String = ":" + reader[5].ToString();
			}
			myArray.Add (reader[0].ToString() + showSexString +
					returnSessionString + ":" + 		//session
					reader[3].ToString() +			//index
					returnJump1String + 			//jump1
					returnJump2String  			//jump2
				    );
		}
		reader.Close();
		dbcon.Close();
		return myArray;
	}

	public static ArrayList GlobalNormal (string sessionString, string operation, bool sexSeparated, int personID)
	{
		dbcon.Open();
		
		string personString = "";
		if(personID != -1) { 
			personString = " AND jump.personID == " + personID;
		}
		
		if (sexSeparated) {
			dbcmd.CommandText = "SELECT type, sessionID, " + operation + "(tv), sex" + 
				" FROM jump, person " +
				sessionString + 
				" AND jump.personID == person.uniqueID" + personString +
				" GROUP BY jump.type, sessionID, person.sex " +
				" ORDER BY jump.type, person.sex DESC, sessionID" ; 
		} else {
		
			dbcmd.CommandText = "SELECT type, sessionID, " + operation + "(tv) " +
				" FROM jump " +
				sessionString + personString +
				" GROUP BY type, sessionID " +
				" ORDER BY type, sessionID " ; 
		}

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		//returns always two columns
		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			if (reader[0].ToString() != "DJ") {
				if (sexSeparated) {
					myArray.Add (reader[0].ToString() + " (" + reader[3].ToString() + "):" +
							reader[1].ToString() + ":" + reader[2].ToString()
							);
				} else {
					myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() 
							+ ":" + reader[2].ToString() 
							);
				}
			}
		}
		
		reader.Close();
		dbcon.Close();
		
		return myArray;
	}

	public static ArrayList GlobalOthers (string statName, string statFormulae, string jumpTable, string jumpType, string sessionString, string operation, bool sexSeparated, int personID)
	{
		dbcon.Open();
		
		string personString = "";
		if(personID != -1) { 
			personString = " AND personID == " + personID;
		}
		
		if (sexSeparated) {
			//select the MAX or AVG index grouped by sex
			//returns 0-2 rows
			dbcmd.CommandText = "SELECT sessionID, " + operation + statFormulae + ", sex " + 
				" FROM " + jumpTable + ", person " +
				sessionString +	
				" AND personID == person.uniqueID" +
				" AND type == '" + jumpType + "'" + personString +
				" GROUP BY sessionID, person.sex " +
				" ORDER BY person.sex DESC, sessionID" ; 
		} else {
			//select the MAX or AVG index. 
			//returns 0-1 rows
			dbcmd.CommandText = "SELECT sessionID, " + operation + statFormulae +
				" FROM " + jumpTable + " " +
				sessionString +	
				" AND type == '" + jumpType + "'" + personString +
				//the following solves a problem
				//of sqlite, that returns an 
				//"empty line" when there are no
				//values to return in a index
				//calculation.
				//With the group by, 
				//if there are no values, 
				//it does not return any line
				" GROUP by sessionID, " +
				" ORDER by sessionID";
		}

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);
		
		//returns always two columns
		while(reader.Read()) {
			if (sexSeparated) {
				myArray.Add (statName +" (" + reader[2].ToString() + "):" + reader[0].ToString() 
							+ ":" + reader[1].ToString() );
			} else {
				myArray.Add (statName + ":" + reader[0].ToString() 
							+ ":" + reader[1].ToString()	);
			}
		}
		
		reader.Close();
		dbcon.Close();
		
		return myArray;
	}

	public static ArrayList GlobalIndexes (string statName, string jump1, string jump2, string sessionString, string operation, bool sexSeparated, int personID)
	{
		dbcon.Open();

		
				string moreSelect = "";
		//moreSelect = "(( " + operation + "(j1.tv)-" + operation + "(j2.tv) )*100/" + operation + "(j2.tv) ) AS myIndex, " +
			//operation + "(j1.tv), " + operation + "(j2.tv), sex";
		moreSelect = operation + "( ( j1.tv-j2.tv )*100/j2.tv ) AS myIndex ";
			//operation + "(j1.tv), " + operation2 + "(j2.tv), sex";
		
		string personString = "";
		if(personID != -1) { 
			personString = " AND j1.personID == " + personID + " AND j2.personID == " + personID + " ";
		}
		
		if (sexSeparated) {
			//select the MAX or AVG index grouped by sex
			//returns 0-2 rows
			dbcmd.CommandText = "SELECT j1.sessionID, " + moreSelect + ", person.sex " +
				" FROM jump AS j1, jump AS j2, person " +
				sessionString +	
				" AND j1.personID == person.uniqueID" +
				" AND j2.personID == person.uniqueID" +
				" AND j1.type == '" + jump1 + "'" + 
				" AND j2.type == '" + jump2 + "'" + 
				personString +
				" GROUP BY j1.sessionID, person.sex " +
				" ORDER BY person.sex DESC, j1.sessionID, myIndex DESC" ; 
		} else {
			//select the MAX or AVG index. 
			//returns 0-1 rows
			dbcmd.CommandText = "SELECT j1.sessionID, " + moreSelect +
				" FROM jump AS j1, jump AS j2, person " +
				sessionString +	
				" AND j1.personID == person.uniqueID" +
				" AND j2.personID == person.uniqueID" +
				" AND j1.type == '" + jump1 + "'" + 
				" AND j2.type == '" + jump2 + "'" + 
				personString +
				//the following solves a problem
				//of sqlite, that returns an 
				//"empty line" when there are no
				//values to return in a index
				//calculation.
				//With the group by, 
				//if there are no values, 
				//it does not return any line
				" GROUP by j1.sessionID, " +
				" ORDER by j1.sessionID, myIndex DESC";
		}

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);
		
		//returns always two columns
		while(reader.Read()) {
			if (sexSeparated) {
				myArray.Add (statName +" (" + reader[2].ToString() + "):" + reader[0].ToString() 
							+ ":" + reader[1].ToString() );
			} else {
				myArray.Add (statName + ":" + reader[0].ToString() 
							+ ":" + reader[1].ToString()	);
			}
		}
		
		reader.Close();
		dbcon.Close();
		
		return myArray;
	}

	/*
	static bool foundInArray (string searching, ArrayList myArray) 
	{
		for (int i=0; i< myArray.Count ; i ++) {
			if (searching == myArray[i].ToString()) {
				return true;
			}
		}
		return false;
	}
	*/

}
