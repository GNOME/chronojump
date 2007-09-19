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
using Mono.Unix;
//using Mono.Data.SqliteClient;
//using System.Data.SqlClient;
using Mono.Data.Sqlite;
//using System.Data.SQLite;


class SqliteStat : Sqlite
{
	//sj, cmj, abk (no sj+)
	//"All jumps" (simple) is not managed here, is done in SjCmjAbkPlus
	public static ArrayList SjCmjAbk (string sessionString, bool multisession, string operationString, string jumpType, bool showSex, bool heightPreferred)
	{
		string ini = "";
		string end = "";
		if(operationString == "MAX") {
			ini = "MAX(";
			end = ")";
		} else if(operationString == "AVG") {
			ini = "AVG(";
			end = ")";
		}
		
		string orderByString = "ORDER BY ";
		string moreSelect = "";
		moreSelect = ini + "jump.tv" + end;
		
		string fromString = " FROM jump, person ";
		string jumpTypeString = " AND jump.type == '" + jumpType + "' ";

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
			//" FROM jump, person " +
			fromString +
			sessionString +
			//" AND jump.type == '" + jumpType + "' " +
			jumpTypeString +
			" AND jump.personID == person.uniqueID " +
			groupByString +
			orderByString + ini + "jump.tv" + end + " DESC ";

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		string showSexString = "";
		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			if(showSex) {
				showSexString = "." + reader[1].ToString() ;
			}
			
			if(multisession) {
				string returnSessionString = ":" + reader[2].ToString();
				string returnValueString = "";
				if(heightPreferred) {
					returnValueString = ":" + Util.GetHeightInCentimeters(
							Util.ChangeDecimalSeparator(reader[3].ToString()));
				} else {
					returnValueString = ":" + reader[3].ToString();
				}
				myArray.Add (reader[0].ToString() + showSexString +
						returnSessionString + 		//session
						returnValueString		//tv or heightofJump
					    );
			} else {
				//in simple session return: name, sex, height, TF
				myArray.Add (reader[0].ToString() + showSexString +
						":" + Util.GetHeightInCentimeters(
							Util.ChangeDecimalSeparator(reader[3].ToString()))
						+ ":" + Util.ChangeDecimalSeparator(reader[3].ToString())
					    );
			}
		}
		reader.Close();
		dbcon.Close();
		return myArray;
	}
	
	//sj+, cmj+, abk+
	//and "All jumps" (simple)
	public static ArrayList SjCmjAbkPlus (string sessionString, bool multisession, string operationString, string jumpType, bool showSex, bool heightPreferred)
	{
		string ini = "";
		string end = "";
		if(operationString == "MAX") {
			ini = "MAX(";
			end = ")";
		} else if(operationString == "AVG") {
			ini = "AVG(";
			end = ")";
		}
		
		string orderByString = "ORDER BY ";
		string moreSelect = "";
		moreSelect = ini + "jump.tv" + end + ", " + ini + "jump.weight" + end + ", person.weight";

		//manage allJumps
		string fromString = " FROM jump, person ";
		string jumpTypeString = " AND jump.type == '" + jumpType + "' ";
		if(jumpType == Catalog.GetString("All jumps")) {
			moreSelect = moreSelect + ", jump.type ";
			fromString = " FROM jump, person, jumpType ";
			jumpTypeString = " AND jumpType.startIn == 1 AND jump.Type == jumpType.name "; 
		}

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
			//" FROM jump, person " +
			fromString +
			sessionString +
			//" AND jump.type == '" + jumpType + "' " +
			jumpTypeString +
			" AND jump.personID == person.uniqueID " +
			groupByString +
			orderByString + ini + "jump.tv" + end + " DESC ";

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		string showSexString = "";
		string showJumpTypeString = "";
		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			if(showSex) {
				showSexString = "." + reader[1].ToString() ;
			}
			//manage allJumps (show jumpType beside name (and sex)) 
			//but only if it's not an AVG of different jumps
			if(jumpType == Catalog.GetString("All jumps") && operationString != "AVG") {
				showJumpTypeString = " (" + reader[6].ToString() + ")";
			}
			
			if(multisession) {
				string returnSessionString = ":" + reader[2].ToString();
				string returnValueString = "";
				if(heightPreferred) {
					returnValueString = ":" + Util.GetHeightInCentimeters(
							Util.ChangeDecimalSeparator(reader[3].ToString()));
				} else {
					returnValueString = ":" + Util.ChangeDecimalSeparator(reader[3].ToString());
				}
				myArray.Add (reader[0].ToString() + showSexString + showJumpTypeString +
						returnSessionString + 		//session
						returnValueString		//tv or heightofJump
					    );
			} else {
				//in simple session return: name, sex, height, TF, Fall
				myArray.Add (reader[0].ToString() + showSexString + showJumpTypeString +
						":" + Util.GetHeightInCentimeters(Util.ChangeDecimalSeparator(
								reader[3].ToString()))
						+ ":" + Util.ChangeDecimalSeparator(reader[3].ToString())
						+ ":" + convertWeight(
							Util.ChangeDecimalSeparator(reader[4].ToString()), 
							//Convert.ToInt32(reader[5].ToString()), weightPercent
							Convert.ToInt32(reader[5].ToString()), true
							)
					    );
			}
		}
		reader.Close();
		dbcon.Close();
		return myArray;
	}

	private static string convertWeight (string jumpW, int personW, bool percentDesired) {
		//if it was a nono weight jump, return 0
		if(jumpW.Length == 0) {
			return "0";
		}

		int i;
		bool percentFound = true;
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
			return (Convert.ToDouble(jumpW.Substring(0,i))*personW/100).ToString();
		} else if( ! percentFound && percentDesired) {
			//found Kg, but wanted percent
			return (Convert.ToDouble(jumpW.Substring(0,i))*100/personW).ToString();
		} else {
			return "ERROR";
		}
	}

	//dj index, Q index, ... (indexType)
	public static ArrayList DjIndexes (string indexType, string sessionString, bool multisession, string operationString, string jumpType, bool showSex)
	{
		string formula = "";
		if(indexType == "djIndex") {
			formula = "((tv-tc)*100/(tc*1.0))"; //*1.0 for having double division
		} else if (indexType == "indexQ") {
			formula = "(tv/(tc*1.0))"; //*1.0 for having double division
		}
		string ini = "";
		string end = "";
		if(operationString == "MAX") {
			ini = "MAX(";
			end = ")";
		} else if(operationString == "AVG") {
			ini = "AVG(";
			end = ")";
		}
		
		string orderByString = "ORDER BY ";
		string moreSelect = "";
		//moreSelect = ini + "((tv-tc)*100/tc)" + end + " AS dj_index, jump.tv, jump.tc, jump.fall";
		moreSelect = ini + formula + end + " AS myIndex, jump.tv, jump.tc, jump.fall";
		
		//manage allJumps
		string fromString = " FROM jump, person ";
		string jumpTypeString = " AND jump.type == '" + jumpType + "' ";
		if(jumpType == Catalog.GetString("All jumps")) {
			moreSelect = moreSelect + ", jump.type ";
			fromString = " FROM jump, person, jumpType ";
			jumpTypeString = " AND jumpType.startIn == 0 AND jump.Type == jumpType.name "; 
		}


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
			//" FROM jump, person " +
			fromString +
			sessionString +
			//" AND jump.type == '" + jumpType + "' " +
			jumpTypeString +
			" AND jump.personID == person.uniqueID " +
			groupByString +
			orderByString + " myIndex DESC, " + ini + "jump.tv" + end + " DESC ";

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		string showSexString = "";
		string showJumpTypeString = "";
		string returnSessionString = "";
		string returnHeightString = "";
		string returnTvString = "";
		string returnTcString = "";
		string returnFallString = "";
		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			if(showSex) {
				showSexString = "." + reader[1].ToString() ;
			}
			//manage allJumps (show jumpType beside name (and sex)) 
			//but only if it's not an AVG of different jumps
			if(jumpType == Catalog.GetString("All jumps") && operationString != "AVG") {
				showJumpTypeString = " (" + reader[7].ToString() + ")";
			}
			
			if(multisession) {
				returnSessionString = ":" + reader[2].ToString();
			} else {
				//in multisession we show only one column x session
				//in simplesession we show all
				
				returnHeightString = ":" + Util.GetHeightInCentimeters(
						Util.ChangeDecimalSeparator(reader[4].ToString()));	
				returnTvString = ":" + Util.ChangeDecimalSeparator(reader[4].ToString());
				returnTcString = ":" + Util.ChangeDecimalSeparator(reader[5].ToString());
				returnFallString = ":" + reader[6].ToString();
			}

			myArray.Add (reader[0].ToString() + showSexString + showJumpTypeString +
					returnSessionString + ":" + 		//session
					Util.ChangeDecimalSeparator(reader[3].ToString()) + //index
					returnHeightString + 			//height
					returnTvString + 			//tv
					returnTcString + 			//tc
					returnFallString			//fall
				    );
		}
		reader.Close();
		dbcon.Close();
		return myArray;
	}

	public static ArrayList RjIndex (string sessionString, bool multisession, string operationString, string jumpType, bool showSex)
	{
		string ini = "";
		string end = "";
		if(operationString == "MAX") {
			ini = "MAX(";
			end = ")";
		} else if(operationString == "AVG") {
			ini = "AVG(";
			end = ")";
		}
		
		string orderByString = "ORDER BY ";
		string moreSelect = "";
		moreSelect = ini + "((tvavg-tcavg)*100/(tcavg * 1.0))" + end + " AS rj_index, tvavg, tcavg, fall"; //*1.0 for having double division

		//manage allJumps
		string fromString = " FROM jumpRj, person ";
		string jumpTypeString = " AND jumpRj.type == '" + jumpType + "' ";
		if(jumpType == Catalog.GetString("All jumps")) {
			moreSelect = moreSelect + ", jumpRj.type ";
			fromString = " FROM jumpRj, person, jumpRjType ";
			jumpTypeString = " AND jumpRj.Type == jumpRjType.name "; 
		}

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
			//" FROM jumpRj, person " +
			fromString +
			sessionString +
			jumpTypeString +
			" AND jumpRj.personID == person.uniqueID " +
			groupByString +
			orderByString + " rj_index DESC, tvavg DESC ";

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		string showSexString = "";
		string showJumpTypeString = "";
		string returnSessionString = "";
		string returnTvString = "";
		string returnTcString = "";
		string returnFallString = "";
		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			if(showSex) {
				showSexString = "." + reader[1].ToString() ;
			}
			//manage allJumps (show jumpType beside name (and sex)) 
			//but only if it's not an AVG of different jumps
			if(jumpType == Catalog.GetString("All jumps") && operationString != "AVG") {
				showJumpTypeString = " (" + reader[7].ToString() + ")";
			}
			
			if(multisession) {
				returnSessionString = ":" + reader[2].ToString();
			} else {
				//in multisession we show only one column x session
				//in simplesession we show all
				//FIXME: convert this to an integer (with percent or kg, depending on bool percent)
				
				returnTvString = ":" + Util.ChangeDecimalSeparator(reader[4].ToString());
				returnTcString = ":" + Util.ChangeDecimalSeparator(reader[5].ToString());
				returnFallString = ":" + reader[6].ToString();
			}
			myArray.Add (reader[0].ToString() + showSexString + showJumpTypeString +
					returnSessionString + ":" + 		//session
					Util.ChangeDecimalSeparator(reader[3].ToString()) +			//index
					returnTvString + 			//tv
					returnTcString + 			//tc
					returnFallString			//fall
				    );
		}
		reader.Close();
		dbcon.Close();
		return myArray;
	}

	public static ArrayList RjPotencyBosco (string sessionString, bool multisession, string operationString, string jumpType, bool showSex)
	{
		string ini = "";
		string end = "";
		if(operationString == "MAX") {
			ini = "MAX(";
			end = ")";
		} else if(operationString == "AVG") {
			ini = "AVG(";
			end = ")";
		}
		
		string orderByString = "ORDER BY ";
		string moreSelect = "";
		moreSelect = ini + "9.81*9.81 * tvavg*jumps * time / ( 4.0 * jumps * (time - tvavg*jumps) )" + end + " AS potency, " + //*4.0 for having double division
			 " tvavg, tcavg, jumps, time, fall";

		//manage allJumps
		string fromString = " FROM jumpRj, person ";
		string jumpTypeString = " AND jumpRj.type == '" + jumpType + "' ";
		if(jumpType == Catalog.GetString("All jumps")) {
			moreSelect = moreSelect + ", jumpRj.type ";
			fromString = " FROM jumpRj, person, jumpRjType ";
			jumpTypeString = " AND jumpRj.Type == jumpRjType.name "; 
		}

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
			//" FROM jumpRj, person " +
			fromString +
			sessionString +
			jumpTypeString +
			" AND jumpRj.personID == person.uniqueID " +
			groupByString +
			orderByString + " potency DESC, tvavg DESC ";

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		string showSexString = "";
		string showJumpTypeString = "";
		string returnSessionString = "";
		string returnTvString = "";
		string returnTcString = "";
		string returnJumpsString = "";
		string returnTimeString = "";
		string returnFallString = "";
		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			if(showSex) {
				showSexString = "." + reader[1].ToString() ;
			}
			//manage allJumps (show jumpType beside name (and sex)) 
			//but only if it's not an AVG of different jumps
			if(jumpType == Catalog.GetString("All jumps") && operationString != "AVG") {
				showJumpTypeString = " (" + reader[9].ToString() + ")";
			}
			
			if(multisession) {
				returnSessionString = ":" + reader[2].ToString();
			} else {
				//in multisession we show only one column x session
				//in simplesession we show all
				//FIXME: convert this to an integer (with percent or kg, depending on bool percent)
				
				returnTvString = ":" + Util.ChangeDecimalSeparator(reader[4].ToString());
				returnTcString = ":" + Util.ChangeDecimalSeparator(reader[5].ToString());
				returnJumpsString = ":" + reader[6].ToString();
				returnTimeString = ":" + Util.ChangeDecimalSeparator(reader[7].ToString());
				returnFallString = ":" + reader[8].ToString();
			}
			myArray.Add (reader[0].ToString() + showSexString + showJumpTypeString +
					returnSessionString + ":" + 		//session
					Util.ChangeDecimalSeparator(reader[3].ToString()) +			//index
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


	//for rjEvolution (for know the name of columns)
	public static int ObtainMaxNumberOfJumps (string sessionString)
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT MAX(jumps) from jumpRj " + sessionString +
			"group by jumps order by jumps DESC limit 1";
			//this is done because if no jumps, and we don't write last line, it returns a blank line
			//and this crashes when converted to string
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		int myReturn = 0;

		bool found = false;
		if(reader.Read()) {
			found = true;
			myReturn = Convert.ToInt32(reader[0].ToString());
		}
		reader.Close();
		dbcon.Close();

		if (found) {
			return myReturn;
		} else {
			return 0;
		}
	}

	//convert the strings of TFs and TCs separated by '=' in
	//one string mixed and separated by ':' (starting by an ':')
	private static string combineTCsTFs(string TCs, string TFs, int maxJumps)
	{
		string [] TCFull = TCs.Split(new char[] {'='});
		string [] TFFull = TFs.Split(new char[] {'='});
		string myReturn = ""; 
		int i;
		for(i=0; i < TCFull.Length; i++) {
			myReturn = myReturn + ":" + TCFull[i];
			
			if(TFFull.Length > i) {
				myReturn = myReturn + ":" + TFFull[i];
			}
		}
		//fill the row with 0's equalling largest row
		for(int j=i; j < maxJumps; j++) {
			myReturn = myReturn + ":-:-";
		}
		return myReturn;
	}

	//maxJumps for make all the results of same length (fill it with '-'s)
	public static ArrayList RjEvolution (string sessionString, bool multisession, string operationString, string jumpType, bool showSex, int maxJumps)
	{
		string ini = "";
		string end = "";
		if(operationString == "MAX") {
			ini = "MAX(";
			end = ")";
		} else if(operationString == "AVG") {
			ini = "AVG(";
			end = ")";
		}
		
		string orderByString = "ORDER BY ";
		string moreSelect = "";
		moreSelect = ini + "((tvavg-tcavg)*100/(tcavg*1.0))" + end + " AS rj_index, tcString, tvString, fall"; //*1.0 for having double division

		//manage allJumps
		string fromString = " FROM jumpRj, person ";
		string jumpTypeString = " AND jumpRj.type == '" + jumpType + "' ";
		if(jumpType == Catalog.GetString("All jumps")) {
			moreSelect = moreSelect + ", jumpRj.type ";
			fromString = " FROM jumpRj, person, jumpRjType ";
			jumpTypeString = " AND jumpRj.Type == jumpRjType.name "; 
		}

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
			//" FROM jumpRj, person " +
			fromString +
			sessionString +
			jumpTypeString +
			" AND jumpRj.personID == person.uniqueID " +
			groupByString +
			orderByString + " rj_index DESC, tvavg DESC ";

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		string showSexString = "";
		string showJumpTypeString = "";
		string returnSessionString = "";
		string allTCsTFsCombined = "";
		string returnFallString = "";
		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			//Console.WriteLine(reader[3].ToString());
			if(showSex) {
				showSexString = "." + reader[1].ToString() ;
			}
			//manage allJumps (show jumpType beside name (and sex)) 
			//but only if it's not an AVG of different jumps
			if(jumpType == Catalog.GetString("All jumps") && operationString != "AVG") {
				showJumpTypeString = " (" + reader[7].ToString() + ")";
			}
			
			if(multisession) {
				returnSessionString = ":" + reader[2].ToString();
			} else {
				//in multisession we show only one column x session
				//in simplesession we show all
				
				//convert the strings of TFs and TCs separated by '=' in
				//one string mixed and separated by ':'
				allTCsTFsCombined = combineTCsTFs(
						Util.ChangeDecimalSeparator(reader[4].ToString()), 
						Util.ChangeDecimalSeparator(reader[5].ToString()), 
						maxJumps);
				
				returnFallString = ":" + reader[6].ToString();
			}
			myArray.Add (reader[0].ToString() + showSexString + showJumpTypeString +
					returnSessionString + ":" + 		//session
					Util.ChangeDecimalSeparator(reader[3].ToString()) +			//index
					returnFallString + 			//fall
					allTCsTFsCombined			//tc:tv:tc:tv...
				    );
		}
		reader.Close();
		dbcon.Close();
		return myArray;
	}

	public static ArrayList IeIub (string sessionString, bool multisession, string ini, string end, string jump1, string jump2, bool showSex)
	{
		//What's this? TODO: check old versions of this file
		/*
		string ini2 = "";
		if(ini == "MAX(") {
			ini2 = "MIN(";
		} else if (ini == "AVG("){
			ini2 = "AVG(";
		}
		*/
			
		string orderByString = "ORDER BY ";
		string moreSelect = ""; 
		
		//*1.0 for having double division
		if(ini == "MAX(") {
			//search MAX of two jumps, not max index!!
			moreSelect = " ( MAX(j1.tv) - MAX(j2.tv) )*100/(MAX(j2.tv)*1.0) AS myIndex, " +
				"MAX(j1.tv), MAX(j2.tv) ";
		} else if(ini == "AVG(") {
			moreSelect = " ( AVG(j1.tv) - AVG(j2.tv) )*100/(AVG(j2.tv)*1.0) AS myIndex, " +
				"AVG(j1.tv), AVG(j2.tv)";
		}

		//if we use AVG or MAX, then we have to group by the results
		//if there's more than one session, it sends the avg or max
		string groupByString = "";
		if (ini.Length > 0) {
			groupByString = " GROUP BY j1.personID, j1.sessionID ";
		}
		//if multisession, order by person.name, sessionID for being able to present results later
		if(multisession) {
			orderByString = orderByString + " person.name, j1.sessionID, ";
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
				showSexString = "." + reader[1].ToString() ;
			}
			if(multisession) {
				returnSessionString = ":" + reader[2].ToString();
			} else {
				//in multisession we show only one column x session
				//in simplesession we show all
				
				returnJump1String = ":" + Util.ChangeDecimalSeparator(reader[4].ToString());
				returnJump2String = ":" + Util.ChangeDecimalSeparator(reader[5].ToString());
			}
			myArray.Add (reader[0].ToString() + showSexString +
					returnSessionString + ":" + 		//session
					Util.ChangeDecimalSeparator(reader[3].ToString()) +			//index
					returnJump1String + 			//jump1
					returnJump2String  			//jump2
				    );
		}
		reader.Close();
		dbcon.Close();
		return myArray;
	}

	public static ArrayList Fv (string sessionString, bool multisession, string ini, string end, string jump1, string jump2, bool showSex)
	{
		string heightJump1 = " 100*4.9* (j1.tv/2.0) * (j1.tv/2.0) ";	//jump1 tv converted to height
		string heightJump2 = " 100*4.9* (j2.tv/2.0) * (j2.tv/2.0) ";	//jump2 tv converted to height
		
		string orderByString = "ORDER BY ";
		string moreSelect = "";
		if(ini == "MAX(") {
			//search MAX of two jumps, not max index!!
			moreSelect = " ( MAX(" + heightJump1 + ") )*100/(1.0*MAX(" + heightJump2 + ")) AS myIndex, " +
				"MAX(" + heightJump1 + "), MAX(" + heightJump2 + ") ";
		} else if(ini == "AVG(") {
			moreSelect = " ( AVG(" + heightJump1 + ") )*100/(1.0*AVG(" + heightJump2 + ")) AS myIndex, " +
				"AVG(" + heightJump1 + "), AVG(" + heightJump2 + ")";
		}

		//if we use AVG or MAX, then we have to group by the results
		//if there's more than one session, it sends the avg or max
		string groupByString = "";
		if (ini.Length > 0) {
			groupByString = " GROUP BY j1.personID, j1.sessionID ";
		}
		//if multisession, order by person.name, sessionID for being able to present results later
		if(multisession) {
			orderByString = orderByString + " person.name, j1.sessionID, ";
		}
		
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.name, person.sex, j1.sessionID, " + moreSelect +
			" FROM jump AS j1, jump AS j2, person " +
			sessionString +
			" AND j1.type == '" + jump1 + "' " +
			" AND j2.type == '" + jump2 + "' " +
			//weight of SJ+ jump is 100% or equals de person weight
			//the || is "the || concatenation operator which gives a string result." 
			//http://sqlite.org/lang_expr.html
			" AND (j1.weight == \"100%\" OR j1.weight == person.weight||'" + "Kg' ) " +
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
				showSexString = "." + reader[1].ToString() ;
			}
			if(multisession) {
				returnSessionString = ":" + reader[2].ToString();
			} else {
				//in multisession we show only one column x session
				//in simplesession we show all
				returnJump1String = ":" + Util.ChangeDecimalSeparator(reader[4].ToString());
				returnJump2String = ":" + Util.ChangeDecimalSeparator(reader[5].ToString());
			}
			myArray.Add (reader[0].ToString() + showSexString +
					returnSessionString + ":" + 		//session
					Util.ChangeDecimalSeparator(reader[3].ToString()) +			//index
					returnJump1String + 			//jump1
					returnJump2String  			//jump2
				    );
		}
		reader.Close();
		dbcon.Close();
		return myArray;
	}

	public static ArrayList CmjPlusPotency (string sessionString, bool multisession, string operationString, string jumpType, bool showSex, bool heightPreferred)
	{
		string ini = "";
		string end = "";
		if(operationString == "MAX") {
			ini = "MAX(";
			end = ")";
		//} else if(operationString == "AVG") {
		//	ini = "AVG(";
		//	end = ")";
		}
		
		string orderByString = "ORDER BY ";
		string moreSelect = "";
		//moreSelect = ini + "jump.tv" + end + ", " + ini + "jump.weight" + end + ", person.weight";
		//jump weight in Kg = jump weight in % * person.weight / 100
		//jump height in centimeters = 100*4.9* pow(jump.tv/2, 2)
		
		moreSelect = 
			ini + "(person.weight + jump.weight*person.weight/100.0) * 9.81" + end + " AS indexPart1, " + 
			ini + "2 * 9.81 * 4.9 * jump.tv/2.0 * jump.tv/2.0" + end + " AS indexPart2WithoutSqrt, " +	//m
			"person.weight, jump.weight*person.weight/100.0 AS extraWeight, 4.9 * 100 * jump.tv/2 * jump.tv/2.0"; 
		//divisor has to be .0 if not, double is bad calculated. Bug 478168
		//TODO: check if ini,end is needed here

		string fromString = " FROM jump, person ";
		string jumpTypeString = " AND jump.type == '" + jumpType + "' ";


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
			fromString +
			sessionString +
			jumpTypeString +
			" AND jump.personID == person.uniqueID " +
			groupByString +
			//orderByString + ini + "indexPart1 * indexPart2WithoutSqrt" + end + " DESC ";
			orderByString + "extraWeight";

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		string showSexString = "";
		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			if(showSex) {
				showSexString = "." + reader[1].ToString() ;
			}
			
			if(multisession) {
				string returnSessionString = ":" + reader[2].ToString();
				string returnValueString = "";
				returnValueString = ":" + 
					(
					 Convert.ToDouble(Util.ChangeDecimalSeparator(reader[3].ToString()))
					 * 
					 System.Math.Sqrt(Convert.ToDouble(Util.ChangeDecimalSeparator(reader[4].ToString())))
					).ToString();
				myArray.Add (reader[0].ToString() + showSexString +
						returnSessionString + 		//session
						returnValueString		//index
					    );
			} else {
				//in simple session return: name, sex, index, personweight, jumpweight, jumpheight
				bool showExtraWeightInName = true; //this is nice for graph
				string extraWeightString = "";
				if(showExtraWeightInName) {
					extraWeightString = "(" + Util.ChangeDecimalSeparator(reader[6].ToString()) + ")";//extra weight
				}
				myArray.Add (reader[0].ToString() + showSexString + extraWeightString +
						":" + 
						(
						 Convert.ToDouble(Util.ChangeDecimalSeparator(reader[3].ToString()))
						 * 
						 System.Math.Sqrt(Convert.ToDouble(Util.ChangeDecimalSeparator(reader[4].ToString())))
						).ToString()
						+ ":" + Util.ChangeDecimalSeparator(reader[5].ToString()) //person weight
						+ ":" + Util.ChangeDecimalSeparator(reader[6].ToString()) //extra weight
						+ ":" + Util.ChangeDecimalSeparator(reader[7].ToString()) //height
							);
			}
		}
		reader.Close();
		dbcon.Close();
		return myArray;
	}

	public static ArrayList GlobalNormal (string sessionString, string operation, bool sexSeparated, 
			int personID, bool heightPreferred)
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

				string heightString = Util.ChangeDecimalSeparator(reader[2].ToString());
				if(heightPreferred) {
					heightString = Util.GetHeightInCentimeters(Util.ChangeDecimalSeparator(reader[2].ToString()));
				}
				
				if (sexSeparated) {
					myArray.Add (reader[0].ToString() + "." + reader[3].ToString() + ":" +
							reader[1].ToString() + ":" + heightString
							);
				} else {
					myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() 
							+ ":" + heightString 
							);
				}
			}
		}
		
		reader.Close();
		dbcon.Close();
		
		return myArray;
	}

	//djindex, qindex, rjIndex, rjpotency(bosco)
	public static ArrayList GlobalOthers (string statName, string statFormulae, string jumpTable, string sessionString, string operation, bool sexSeparated, int personID)
	{

		//select all possible jumpTypes and put in jumpTypeString for SQL
		string jumpTypeString= " AND (";
		string [] myJumpTypes;
		if(jumpTable == "jump") {
			myJumpTypes = SqliteJumpType.SelectJumpTypes("", "TC", true);
		} else {
			myJumpTypes = SqliteJumpType.SelectJumpRjTypes("", true);
		}
		for(int i=0; i < myJumpTypes.Length ; i++) {
			if (i>0) {
				jumpTypeString += " OR ";
			}
			jumpTypeString += " type == '" + myJumpTypes[i] + "'";
		}
		jumpTypeString += " ) ";
	
		
		dbcon.Open();

		
		string personString = "";
		if(personID != -1) { 
			personString = " AND personID == " + personID;
		}
		
		if (sexSeparated) {
			//select the MAX or AVG index grouped by sex
			//returns 0-2 rows
			dbcmd.CommandText = "SELECT sessionID, type, " + operation + statFormulae + ", sex " + 
				" FROM " + jumpTable + ", person " +
				sessionString +	
				" AND personID == person.uniqueID" +
				//" AND type == '" + jumpType + "'" + personString +
				jumpTypeString +
				personString +
				" GROUP BY sessionID, type, person.sex " +
				" ORDER BY person.sex DESC, type, sessionID" ; 
		} else {
			//select the MAX or AVG index. 
			//returns 0-1 rows
			dbcmd.CommandText = "SELECT sessionID, type, " + operation + statFormulae +
				" FROM " + jumpTable + " " +
				sessionString +	
				//" AND type == '" + jumpType + "'" + personString +
				jumpTypeString +
				personString +
				//the following solves a problem
				//of sqlite, that returns an 
				//"empty line" when there are no
				//values to return in a index
				//calculation.
				//With the group by, 
				//if there are no values, 
				//it does not return any line
				" GROUP by sessionID, type " +
				" ORDER by type, sessionID";
		}

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);
		
		//returns always two columns
		while(reader.Read()) {
			if (sexSeparated) {
				myArray.Add (statName + " (" + reader[1].ToString()  + ")" + 	//stat name(jumptype)
						"." + reader[3].ToString() + ":" + 		//sex
						reader[0].ToString() + ":" + Util.ChangeDecimalSeparator(reader[2].ToString()) //session, value
						);
			} else {
				myArray.Add (statName + " (" + reader[1].ToString()  + ") " + ":" + 	//stat name (jumptype)
						reader[0].ToString() + ":" + Util.ChangeDecimalSeparator(reader[2].ToString())	//session, value
						);
			}
		}
		
		reader.Close();
		dbcon.Close();
		
		return myArray;
	}

	//IE, IUB, FV
	public static ArrayList GlobalIndexes (string statName, string jump1, string jump2, string sessionString, string operation, bool sexSeparated, int personID)
	{
		dbcon.Open();

		string moreSelect = "";
		string weightString = ""; //used by FV index
		
		if(statName == "FV") {
			string heightJump1 = " 100*4.9* (j1.tv/2.0) * (j1.tv/2.0) ";	//jump1 tv converted to height
			string heightJump2 = " 100*4.9* (j2.tv/2.0) * (j2.tv/2.0) ";	//jump2 tv converted to height
			if(operation == "MAX") {
				//search MAX of two jumps, not max index!!
				moreSelect = " ( MAX(" + heightJump1 + ") )*100/(1.0*MAX(" + heightJump2 + ")) AS myIndex, " +
					"MAX(" + heightJump1 + "), MAX(" + heightJump2 + ") ";
			} else if(operation == "AVG") {
				moreSelect = " ( AVG(" + heightJump1 + ") )*100/(1.0*AVG(" + heightJump2 + ")) AS myIndex, " +
					"AVG(" + heightJump1 + "), AVG(" + heightJump2 + ")";
			}
			weightString = " AND (j1.weight == \"100%\" OR j1.weight == person.weight||'" + "Kg' ) ";
		} else {	//IE, IUB
			if(operation == "MAX") {
				//search MAX of two jumps, not max index!!
				moreSelect = "( ( MAX(j1.tv) - MAX(j2.tv) )*100/(MAX(j2.tv)*1.0) ) AS myIndex ";
			} else if(operation == "AVG") {
				moreSelect = "( ( AVG(j1.tv) - AVG(j2.tv) )*100/(AVG(j2.tv)*1.0) ) AS myIndex ";
			}
		}
		
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
				weightString + 		//used by FV
				" AND j1.personID == person.uniqueID" +
				" AND j2.personID == person.uniqueID" +
				" AND j1.type == '" + jump1 + "'" + 
				" AND j2.type == '" + jump2 + "'" + 
				personString +
				" GROUP BY j1.sessionID, person.sex " +
				" ORDER BY person.sex DESC, j1.sessionID" ; 
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
				" GROUP by j1.sessionID" +
				" ORDER by j1.sessionID";
		}

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);
		
		//returns always two columns
		while(reader.Read()) {
			if (sexSeparated) {
				myArray.Add (statName +"." + reader[2].ToString() + ":" + reader[0].ToString() 
							+ ":" + Util.ChangeDecimalSeparator(reader[1].ToString()) );
			} else {
				myArray.Add (statName + ":" + reader[0].ToString() 
							+ ":" + Util.ChangeDecimalSeparator(reader[1].ToString())	);
			}
		}
		
		reader.Close();
		dbcon.Close();
		
		return myArray;
	}

}
