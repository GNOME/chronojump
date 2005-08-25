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


public class StatRjPotencyBosco : Stat
{
	protected string [] columnsString = { 
		Catalog.GetString("Jumper"), 
		Catalog.GetString("Index"), 
		Catalog.GetString("TV(AVG)"), 
		Catalog.GetString("TC(AVG)"), 
		Catalog.GetString("Jumps"), 
		Catalog.GetString("Time"), 
		Catalog.GetString("Fall") };

	//if this is not present i have problems like (No overload for method `xxx' takes `0' arguments) with some inherited classes
	public StatRjPotencyBosco () 
	{
		this.showSex = false;
		this.statsJumpsType = 0;
		this.limit = 0;
	}

	public StatRjPotencyBosco (Gtk.TreeView treeview, ArrayList sessions, int newPrefsDigitsNumber, string jumpType, bool showSex, int statsJumpsType, int limit) 
	{
		this.dataColumns = 6;	//for simplesession (index, tv (avg), tc (avg), jumps, time, fall)
		this.jumpType = jumpType;
		this.limit = limit;
		
		if(sessions.Count > 1) {
			store = getStore(sessions.Count +3); //+3 (for jumper, the AVG horizontal and SD horizontal)
		} else {
			store = getStore(dataColumns +1); //jumper, index, tv(avg), tc(avg), jumps, time, fall
		}
		
		treeview.Model = store;

		completeConstruction (treeview, sessions, newPrefsDigitsNumber, showSex, statsJumpsType);
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
				string operation = "AVG";
				processDataMultiSession ( 
						SqliteStat.RjPotencyBosco(sessionString, multisession, 
							//"AVG(", ")", showSex), 
							operation, jumpType, showSex), 
						true, sessions.Count);
			} else {
				string operation = "AVG";
				processDataSimpleSession ( cleanDontWanted (
							SqliteStat.RjPotencyBosco(sessionString, multisession, 
								//"AVG(", ")", showSex), 
								operation, jumpType, showSex), 
							statsJumpsType, limit),
						true, dataColumns);
			}
		} else {
			//if more than on session, show only the avg or max of each jump/jumper
			if(multisession) {
				string operation = "MAX";
				processDataMultiSession ( SqliteStat.RjPotencyBosco(sessionString, multisession, 
							//"MAX(", ")", showSex),  
							operation, jumpType, showSex), 
						true, sessions.Count);
			} else {
				string operation = ""; //no need of "MAX", there's an order by (index) desc
							//and clenaDontWanted will do his work
				processDataSimpleSession ( cleanDontWanted (
							SqliteStat.RjPotencyBosco(sessionString, multisession, 
								//"", "", showSex), 
								operation, jumpType, showSex), 
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


