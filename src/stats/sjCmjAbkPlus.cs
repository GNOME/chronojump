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
using Gtk;
using System.Collections; //ArrayList


//no sj+
public class StatSjCmjAbkPlus : Stat
{
	protected bool percent; //show weight in simplesession as a percent of body weight or kg
	
	//if this is not present i have problems like (No overload for method `xxx' takes `0' arguments) with some inherited classes
	public StatSjCmjAbkPlus () 
	{
		this.sessionName = "";
		this.showSex = false;
		this.statsJumpsType = 0;
		this.limit = 0;
	}

	public StatSjCmjAbkPlus (Gtk.TreeView treeview, ArrayList sessions, int newPrefsDigitsNumber, string jumpType, bool showSex, int statsJumpsType, int limit, bool percent) 
	{
		this.sessionName = "nothing";
		this.dataColumns = 2;	//for simplesession (tv, weight)
		this.jumpType = jumpType;
		this.limit = limit;
		this.percent = percent;
		
		if(sessions.Count > 1) {
			store = getStore(sessions.Count +3); //+3 (for jumper, the AVG horizontal and SD horizontal)
		} else {
			store = getStore(dataColumns +1); //jumper, TV, weight (this col has characters '%' and 'Kg') solved in sqlite
		}
		
		treeview.Model = store;

		completeConstruction (treeview, sessions, newPrefsDigitsNumber, showSex, statsJumpsType);
		string [] columnsString = { "Jumper", "TV", "Weight %" };
		if(! percent) {
			columnsString[2] = "Weight Kg";
		}
		prepareHeaders(columnsString);
	}

	
	public override void prepareData() 
	{
		string sessionString = obtainSessionSqlString(sessions);
		bool multisession = false;
		if(sessions.Count > 1) {
			multisession = true;
		}

		if(statsJumpsType == 3) { //avg of each jumper
			if(multisession) {
				processDataMultiSession ( 
						SqliteStat.SjCmjAbkPlus(sessionString, multisession, 
							"AVG(", ")", jumpType, showSex, percent), 
						true, sessions.Count);
			} else {
				processDataSimpleSession ( cleanDontWanted (
							SqliteStat.SjCmjAbkPlus(sessionString, multisession, 
								"AVG(", ")", jumpType, showSex, percent), 
							statsJumpsType, limit),
						true, dataColumns);
			}
		} else {
			//if more than on session, show only the avg or max of each jump/jumper
			if(multisession) {
				processDataMultiSession ( SqliteStat.SjCmjAbkPlus(sessionString, multisession, 
							"MAX(", ")", jumpType, showSex, percent),  
						true, sessions.Count);
			} else {
				processDataSimpleSession ( cleanDontWanted (
							SqliteStat.SjCmjAbkPlus(sessionString, multisession, 
								"", "", jumpType, showSex, percent), 
							statsJumpsType, limit),
						true, dataColumns);
			}
		}
	}
		
	public override string ToString () 
	{
		/*
		string operationString = "";
		//if ( this.operation == "MAX" ) { 
		if ( statsJumpsType == 3 ) { 
			operationString =  Catalog.GetString ("by AVG flight time values "); 
		}
		else { operationString =  Catalog.GetString ("by MAX flight time values "); 
		}

		string inJump =  Catalog.GetString (" in ") + jumpType +  Catalog.GetString (" jump ");
		string inSession =  Catalog.GetString (" in '") + sessionName +  Catalog.GetString ("' session ");

		string valuesString = "";
		if ( this.limit != -1 ) {
			valuesString = this.limit.ToString();
		}
			
			
			return  Catalog.GetString ("Selection of the ") + valuesString + 
				 Catalog.GetString (" MAX values of flight time ") + inJump + inSession + "."; 
		}
		*/
		return "pending";
	}

}

