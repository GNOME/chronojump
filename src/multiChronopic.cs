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

public class MultiChronopic : Event 
{
	private string cp1InStr;
	private string cp1OutStr;
	private string cp2InStr;
	private string cp2OutStr;
	private string cp3InStr;
	private string cp3OutStr;
	private string cp4InStr;
	private string cp4OutStr;

	public MultiChronopic() {
	}

	//after inserting database (SQL)
	public MultiChronopic(int uniqueID, int personID, int sessionID, 
			string cp1InStr, string cp1OutStr,
			string cp2InStr, string cp2OutStr,
			string cp3InStr, string cp3OutStr,
			string cp4InStr, string cp4OutStr,
			string description, int simulated)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
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
				"default", //type
				cp1InStr, cp1OutStr,
				cp2InStr, cp2OutStr,
				cp3InStr, cp3OutStr,
				cp4InStr, cp4OutStr,
				description, simulated);
	}

	~MultiChronopic() {}
}
