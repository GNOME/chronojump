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
		string orderByString = "";
		string moreSelect = "";
		moreSelect = ini + "jump.tv" + end;

		//if we use AVG or MAX, then we have to group by the results
		//if there's more than one session, it sends the avg or max
		string groupByString = "";
		if (ini.Length > 0) {
			groupByString = " GROUP BY jump.personID, jump.sessionID ";
		}
		
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.name, person.sex, sessionID, " + moreSelect +
			" FROM jump, person " +
			sessionString +
			" AND jump.type == '" + jumpType + "' " +
			" AND jump.personID == person.uniqueID " +
			groupByString +
			" ORDER BY person.name, sessionID, " + orderByString + " jump.tv DESC ";

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
		string orderByString = "";
		string moreSelect = "";
		moreSelect = ini + "jump.tv" + end + ", jump.weight, person.weight";

		//if we use AVG or MAX, then we have to group by the results
		//if there's more than one session, it sends the avg or max, no more columns
		//there's no chance of mixing tv and weight of different jumps in multisessions because only tv is returned
		string groupByString = "";
		if (ini.Length > 0) {
			groupByString = " GROUP BY jump.personID, jump.sessionID ";
		}
		
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.name, person.sex, sessionID, " + moreSelect +
			" FROM jump, person " +
			sessionString +
			" AND jump.type == '" + jumpType + "' " +
			" AND jump.personID == person.uniqueID " +
			groupByString +
			" ORDER BY person.name, sessionID, " + orderByString + " jump.tv DESC ";

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

	//dj
	public static ArrayList Dj (string sessionString, bool multisession, string ini, string end, bool showSex)
	{
		string orderByString = "";
		string moreSelect = "";
		moreSelect = ini + "jump.tv" + end + ", jump.tc, jump.fall";

		//if we use AVG or MAX, then we have to group by the results
		//if there's more than one session, it sends the avg or max
		string groupByString = "";
		if (ini.Length > 0) {
			groupByString = " GROUP BY jump.personID, jump.sessionID ";
		}
		
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.name, person.sex, sessionID, " + moreSelect +
			" FROM jump, person " +
			sessionString +
			" AND jump.type == 'DJ' " +
			" AND jump.personID == person.uniqueID " +
			groupByString +
			" ORDER BY person.name, sessionID, " + orderByString + " jump.tv DESC ";

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
	/*
	private static string getSexSqlString (string sex)
	{
		string sexSqlString = "";
		if (sex == "M") {
			sexSqlString = " AND person.sex == 'M' " ;
		} else if (sex == "F") {
			sexSqlString = " AND person.sex == 'F' " ;
		}

		return sexSqlString;
	}
	*/
	
	/*
	//Only for DJ, for solving a problem that tv and tc doesn't match in some options
	public static ArrayList StatOneJumpJumpersDj (int sessionID, bool index, bool sexSeparated, string operation)
	{
		string orderByString = "";
		if (sexSeparated) {
			orderByString = "person.sex DESC, ";
		}
		
		string selectString = "";
		
		dbcon.Open();
		//0 always tv
		//1 always person
		//2 always sex
		if (operation == "AVG") {
			if (index) {
				selectString = " AVG(jump.tv) AS jump_tv, person.name, person.sex, AVG(jump.tc) AS jump_tc, AVG(jump.fall), AVG(100*((jump.tv-jump.tc)/jump.tc)) AS jump_index ";
				orderByString = orderByString + "jump_index DESC ";
			} else {
				selectString = " AVG(jump.tv) AS jump_tv, person.name, person.sex, AVG(jump.tc) AS jump_tc, AVG(jump.fall) ";
				orderByString = orderByString + "jump_tv DESC ";
			}
			dbcmd.CommandText = "SELECT " + selectString + 
				" FROM jump, person " +
				" WHERE jump.type == 'DJ' " +
				" AND jump.sessionID == " + sessionID + 
				" AND jump.personID == person.uniqueID " +
				" GROUP BY person.uniqueID " +
				" ORDER BY " + orderByString ; 
		} else {
			//operation == MAX 
			//not "GROUP BY" because it doens't match the tv with the tc
			if (index) {
				selectString = " jump.tv, person.name, person.sex, jump.tc, jump.fall, 100*(jump.tv-jump.tc)/jump.tc AS jump_index ";
				orderByString = orderByString + "jump_index DESC ";
			} else {
				selectString = " jump.tv, person.name, person.sex, jump.tc, jump.fall  ";
				orderByString = orderByString + "jump.tv DESC ";
			}
			dbcmd.CommandText = "SELECT " + selectString +
				" FROM jump, person " +
				" WHERE jump.type == 'DJ' " +
				" AND jump.sessionID == " + sessionID + 
				" AND jump.personID == person.uniqueID " +
				" ORDER BY " + orderByString ; 
		}

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);
		ArrayList arrayJumpers = new ArrayList(2);
		
		while(reader.Read()) { 	
			//filter all except the first value of each person
			if ( ! (operation == "MAX" && foundInArray(reader[1].ToString(), arrayJumpers) ) ) 
			{
				if(index) {
					myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
							reader[2].ToString() + ":" + reader[3].ToString() + ":" +
							reader[4].ToString() + ":" + reader[5].ToString() );
				} else {
					myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
							reader[2].ToString() + ":" + reader[3].ToString() + ":" +
							reader[4].ToString() );
				}
			arrayJumpers.Add(reader[1].ToString());
			}
		}
		
		reader.Close();
		dbcon.Close();
		
		return myArray;
	}

	//Only for RJ, for solving a problem that tv and tc doesn't match in some options
	public static ArrayList StatOneJumpJumpersRj (int sessionID, bool sexSeparated, string operation)
	{
		string orderByString = "";
		if (sexSeparated) {
			orderByString = "person.sex DESC, ";
		}
		
		string selectString = "";
		
		dbcon.Open();
		//0 always tv
		//1 always person
		//2 always sex
		if (operation == "AVG") {
			selectString = " AVG(tvavg) AS jump_tv, person.name, person.sex, AVG(tcavg) AS jump_tc, AVG(fall), AVG(100*((tvavg-tcavg)/tcavg)) AS jump_index ";
			orderByString = orderByString + "jump_index DESC ";
			dbcmd.CommandText = "SELECT " + selectString + 
				" FROM jumpRj, person " +
				" WHERE type == 'RJ' " +
				" AND jumpRj.sessionID == " + sessionID + 
				" AND jumpRj.personID == person.uniqueID " +
				" GROUP BY person.uniqueID " +
				" ORDER BY " + orderByString ; 
		} else {
			selectString = " tvavg, person.name, person.sex, tcavg, fall, 100*(tvavg-tcavg)/tcavg AS jump_index ";
			orderByString = orderByString + "jump_index DESC ";
			dbcmd.CommandText = "SELECT " + selectString +
				" FROM jumpRj, person " +
				" WHERE type == 'RJ' " +
				" AND jumpRj.sessionID == " + sessionID + 
				" AND jumpRj.personID == person.uniqueID " +
				" ORDER BY " + orderByString ; 
		}

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);
		ArrayList arrayJumpers = new ArrayList(2);
		
		while(reader.Read()) { 	
			//filter all except the first value of each person
			if ( ! (operation == "MAX" && foundInArray(reader[1].ToString(), arrayJumpers) ) ) 
			{
				myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
						reader[2].ToString() + ":" + reader[3].ToString() + ":" +
						reader[4].ToString() + ":" + reader[5].ToString() );
				arrayJumpers.Add(reader[1].ToString());
			}
		}
		
		reader.Close();
		dbcon.Close();
		
		return myArray;
	}

	public static ArrayList StatOneJumpJumpersRjPotencyAguado (int sessionID, bool sexSeparated, string operation)
	{
		string orderByString = "";
		if (sexSeparated) {
			orderByString = "person.sex DESC, ";
		}
		
		string selectString = "";
		
		dbcon.Open();
		
		if (operation == "AVG") {
			selectString = " person.name, person.sex, AVG(jumpRj.jumps), AVG(jumpRj.time), AVG(jumpRj.tvAvg), " +
				"AVG(9.81*9.81 * tvavg*jumps * time / ( 4 * jumps * (time - tvavg*jumps) ) ) AS potency ";
			orderByString = orderByString + "potency DESC ";
			dbcmd.CommandText = "SELECT " + selectString + 
				" FROM jumpRj, person " +
				" WHERE type == 'RJ' " +
				" AND jumpRj.sessionID == " + sessionID + 
				" AND jumpRj.personID == person.uniqueID " +
				" GROUP BY person.uniqueID " +
				" ORDER BY " + orderByString ; 
		} else {
			selectString = " person.name, person.sex, jumpRj.jumps, jumpRj.time, jumpRj.tvAvg, " +
				"(9.81*9.81 * tvavg*jumps * time / ( 4 * jumps * (time - tvavg*jumps) ) ) AS potency ";
			orderByString = orderByString + "potency DESC ";
			dbcmd.CommandText = "SELECT " + selectString +
				" FROM jumpRj, person " +
				" WHERE type == 'RJ' " +
				" AND jumpRj.sessionID == " + sessionID + 
				" AND jumpRj.personID == person.uniqueID " +
				" ORDER BY " + orderByString ; 
		}

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);
		ArrayList arrayJumpers = new ArrayList(2);
		
		while(reader.Read()) { 	
			//filter all except the first value of each person
			if ( ! (operation == "MAX" && foundInArray(reader[0].ToString(), arrayJumpers) ) ) 
			{
				myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
						reader[2].ToString() + ":" + reader[3].ToString() + ":" +
						reader[4].ToString() + ":" + reader[5].ToString() );
				arrayJumpers.Add(reader[0].ToString());
			}
		}
		
		reader.Close();
		dbcon.Close();
		
		return myArray;
	}

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

	//for SJ, SJ+, CMJ, ABK, DJ
	//public static ArrayList StatOneJumpJumps (int sessionID, string jumpType, bool index, bool sexSeparated, int limit)
	//public static ArrayList StatNormalJumps (int sessionID, string jumpType, bool index, bool showSex)
	/*
	public static ArrayList StatNormalJumps (string sessionString, string ini, string end, string jumpType, bool index, bool showSex)
	{
		string orderByString = "";
			
		string moreSelect = "";
		if (jumpType == "DJ") {
			if (index) {
				moreSelect = ini + "100*(jump.tv-jump.tc)/jump.tc" + end + " AS jump_index, " + ini + "jump.fall" + end;
				orderByString = "jump_index DESC, ";
			} else {
				moreSelect = ini + "jump.tv" + end + ", " + ini + "jump.tc" + end + ", " + ini + "jump.fall" + end;
			}
		}
		else if (jumpType == "SJ+") {
			moreSelect = ini + "jump.tv" + end + ", " + ini + "jump.weight" + end;
		}
		else {
			moreSelect = ini + "jump.tv" + end;
		}

		//if we use AVG or MAX, then we have to group by the results
		//if there's more than one session, it sends the avg or max
		string groupByString = "";
		if (ini.Length > 0) {
			groupByString = " GROUP BY jump.personID, jump.sessionID ";
		}
		
		dbcon.Open();
		//0 always tv
		//1 always person
		//2 always sex
		dbcmd.CommandText = "SELECT person.name, person.sex, sessionID, " + moreSelect +
			" FROM jump, person " +
			sessionString +
			" AND jump.type == '" + jumpType + "' " +
			" AND jump.personID == person.uniqueID " +
			groupByString +
			" ORDER BY person.name, sessionID, " + orderByString + " jump.tv DESC ";

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		string showSexString = "";
		

		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			if(showSex) {
				showSexString = " (" + reader[1].ToString() + ")";
			}

			if (jumpType == "DJ") {
				if(index) {
					myArray.Add (reader[0].ToString() + showSexString + ":" +
						reader[2].ToString() + ":" + 	//session
						reader[3].ToString() 		//index
						//+ ":" +	reader[4].ToString() 	//fall
						);
				} else {
					myArray.Add (reader[0].ToString() + showSexString + ":" +
						reader[2].ToString() + ":" +		//session
						reader[3].ToString() 			//tv
						//+ ":" + reader[4].ToString() + ", " + //tc, fall
						//reader[5].ToString() 
						);
				}
			}
			else if (jumpType == "SJ+") {
				myArray.Add (reader[0].ToString() + showSexString + ":" +
						reader[2].ToString() + ":" + 		//session
						reader[3].ToString() 			//tv
						//+ ":" reader[4].ToString() 		//weight
						);
			} else {
				myArray.Add (reader[0].ToString() + showSexString + ":" +
						reader[2].ToString() + ":" + 		//session
						reader[3].ToString() 			//tv
						//+ ":" + ""  				//extra col
						);
			}
		}
		
		reader.Close();
		dbcon.Close();
		
		return myArray;
	}
	*/	

	/*
	public static ArrayList StatRjJumps (int sessionID, bool sexSeparated, int limit)
	{
		string orderByString = "";
		if (sexSeparated) {
			orderByString = "person.sex DESC, ";
		}
		
		if (limit > 0) {
			//if we use jumps and limit, don't separate by sex, because it's difficult to show 
			//the same proportion of man and woman without doing all this two times
			orderByString = "";
		}
			
		string moreSelect = "";
		moreSelect = ", tcavg, fall, (100*(tvavg-tcavg)/tcavg) AS jumpRj_index " ;
		orderByString = orderByString + "jumpRj_index DESC ";

		
		dbcon.Open();
		//0 always tv
		//1 always person
		//2 always sex
		dbcmd.CommandText = "SELECT tvavg, person.name, person.sex " + moreSelect +
			" FROM jumpRj, person " +
			" WHERE type == 'RJ' " +
			" AND jumpRj.sessionID == " + sessionID + 
			" AND jumpRj.personID == person.uniqueID " +
			" ORDER BY " + orderByString +
			" LIMIT " + limit ; 

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
					reader[2].ToString() + ":" + reader[3].ToString() + ":" +
					reader[4].ToString() + ":" + reader[5].ToString() );
		}
		
		reader.Close();
		dbcon.Close();
		
		return myArray;
	}

	public static ArrayList StatRjPotencyAguadoJumps (int sessionID, bool sexSeparated, int limit)
	{
		string orderByString = "";
		if (sexSeparated) {
			orderByString = "person.sex DESC, ";
		}
		
		if (limit > 0) {
			//if we use jumps and limit, don't separate by sex, because it's difficult to show 
			//the same proportion of man and woman without doing all this two times
			orderByString = "";
		}
			
		orderByString = orderByString + "potency DESC ";

		
		dbcon.Open();
		//0 always tv
		//1 always person
		//2 always sex
		dbcmd.CommandText = "SELECT person.name, person.sex, jumpRj.jumps, jumpRj.time, jumpRj.tvAvg" +
			", (9.81*9.81 * tvavg*jumps * time / ( 4 * jumps * (time - tvavg*jumps) ) ) AS potency " +
			" FROM jumpRj, person " +
			" WHERE type == 'RJ' " +
			" AND jumpRj.sessionID == " + sessionID + 
			" AND jumpRj.personID == person.uniqueID " +
			" ORDER BY " + orderByString +
			" LIMIT " + limit ; 

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
					reader[2].ToString() + ":" + reader[3].ToString() + ":" +
					reader[4].ToString() + ":" + reader[5].ToString() );
		}
		
		reader.Close();
		dbcon.Close();
		
		return myArray;
	}

	
	//IE (Elasticity index) relationship between SJ and CMJ
	//IUB (Using of arms index) relationship between CMJ and ABK
	//this method gets all SJ or CMJ or ABK of all persons in a session
	public static ArrayList StatClassificationIeIub (int sessionID, string indexType, string operation, bool sexSeparated)
	{
		//string indexTypeString = "";
		string jump1Name = "";
		string jump2Name = "";
		if (indexType == "IE") {
			//indexTypeString = " AND ( jump.type == 'SJ' OR jump.type == 'CMJ' ) " ;
			jump1Name = "SJ";
			jump2Name = "CMJ";
		} else { //IUB
			//indexTypeString = " AND ( jump.type == 'CMJ' OR jump.type == 'ABK' ) " ;
			jump1Name = "CMJ";
			jump2Name = "ABK";
		}

		string sexSeparatedString = "";
		if (sexSeparated) {
			sexSeparatedString = "person.sex DESC, ";
		}
		
		dbcon.Open();
		
		dbcmd.CommandText = "SELECT person.name, " +
			"((" + operation + "(j2.tv) - " + operation + "(j1.tv))*100/" + operation + "(j1.tv)) AS myindex, " +
			operation + "(j1.tv), " + operation + "(j2.tv), person.sex " +
			" FROM person, jump AS j1, jump AS j2 " +
			" WHERE j1.type == '" + jump1Name + "' " +
			" AND j2.type == '" + jump2Name + "' " +
			" AND person.uniqueID == j1.personID " +
			" AND j1.personID == j2.personID " +
			" AND j1.sessionID == " + sessionID +
			" AND j2.sessionID == " + sessionID +
			" GROUP BY person.uniqueID " +
			" ORDER BY " + sexSeparatedString + " myIndex DESC";
			
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			if(reader[2].ToString() == "0") {
				myArray.Add (reader[0].ToString() + ":" + "DIV" + ":" +
						reader[2].ToString() + ":" + reader[3].ToString() + ":" +
						reader[4].ToString());
			} else {
				myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
						reader[2].ToString() + ":" + reader[3].ToString() + ":" +
						reader[4].ToString());
			}
		}
		
		reader.Close();
		dbcon.Close();
		
		return myArray;
	}

*/	
}
