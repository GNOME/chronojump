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


public class StatDj : Stat
{
	//if this is not present i have problems like (No overload for method `xxx' takes `0' arguments) with some inherited classes
	public StatDj () 
	{
		this.sessionName = "";
		this.showSex = false;
		this.statsJumpsType = 0;
		this.limit = 0;
	}

	public StatDj (Gtk.TreeView treeview, ArrayList sessions, int newPrefsDigitsNumber, bool showSex, int statsJumpsType, int limit) 
	{
		this.sessionName = "nothing";
		this.dataColumns = 3;	//for simplesession (tv, tc, fall)
		this.limit = limit;
		
		if(sessions.Count > 1) {
			store = getStore(sessions.Count +3); //+3 (for jumper, the AVG horizontal and SD horizontal)
		} else {
			store = getStore(dataColumns +1); //jumper, tv, tc, fall
		}
		
		treeview.Model = store;

		completeConstruction (treeview, sessions, newPrefsDigitsNumber, showSex, statsJumpsType);
		string [] columnsString = { "Jumper", "TV", "TC", "Fall" };
		prepareHeaders(columnsString);
	}
	
	public override void PrepareData() 
	{
		string sessionString = obtainSessionSqlString(sessions);
		bool multisession = false;
		if(sessions.Count > 1) {
			multisession = true;
		}

		if(statsJumpsType == 3) { //avg of each jumper
			if(multisession) {
				processDataMultiSession ( 
						SqliteStat.Dj(sessionString, multisession, "AVG(", ")", showSex), 
						true, sessions.Count);
			} else {
				processDataSimpleSession ( cleanDontWanted (
							SqliteStat.Dj(sessionString, multisession, "AVG(", ")", showSex), 
							statsJumpsType, limit),
						true, dataColumns);
			}
		} else {
			//if more than on session, show only the avg or max of each jump/jumper
			if(multisession) {
				processDataMultiSession ( SqliteStat.Dj(sessionString, multisession, "MAX(", ")", showSex),  
						true, sessions.Count);
			} else {
				processDataSimpleSession ( cleanDontWanted (
							SqliteStat.Dj(sessionString, multisession, "", "", showSex), 
							statsJumpsType, limit),
						true, dataColumns);
			}
		}
	}
		
	public override string ToString () 
	{
		return "pending";
	}
}

