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


public class StatRjEvolution : Stat
{
	protected int maxJumps;
	protected string [] columnsString;

	//if this is not present i have problems like (No overload for method `xxx' takes `0' arguments) with some inherited classes
	public StatRjEvolution () 
	{
		this.showSex = false;
		this.statsJumpsType = 0;
		this.limit = 0;
	}

	public StatRjEvolution (Gtk.TreeView treeview, ArrayList sessions, int newPrefsDigitsNumber, bool showSex, int statsJumpsType, int limit) 
	{
		string sessionString = obtainSessionSqlString(sessions);

		//we need to know the reactive with more jumps for prepare columns
		maxJumps = SqliteStat.ObtainMaxNumberOfJumps(sessionString);
		
		this.dataColumns = maxJumps*2 + 2;	//for simplesession (index, (tv , tc)*jumps, fall)
		this.limit = limit;

		//only simplesession
		store = getStore(dataColumns +1); //jumper, datacolumns 
		string [] columnsString = new String[dataColumns +1];
		columnsString[0] = Catalog.GetString("Jumper");
		columnsString[1] = Catalog.GetString("Index");
		int i;
		for(i=0; i < maxJumps; i++) {
			columnsString[i*2 +2] = Catalog.GetString("TC") + (i+1).ToString(); //cols: 2, 4, 6, ...
			columnsString[i*2 +2 +1] = Catalog.GetString("TV") + (i+1).ToString(); //cols: 3, 5, 7, ...
		}
		columnsString[i*2 +2] = Catalog.GetString("Fall");
		
		treeview.Model = store;

		completeConstruction (treeview, sessions, newPrefsDigitsNumber, showSex, statsJumpsType);

		Console.WriteLine("maxjumps: {0}, datacolumns: {1}", maxJumps, dataColumns);
		prepareHeaders(columnsString);
	}
	
	public override void PrepareData() 
	{
		string sessionString = obtainSessionSqlString(sessions);
		//only simplesession
		bool multisession = false;

		//we send maxJumps for make all the results of same length (fill it with '-'s)
		if(statsJumpsType == 3) { //avg of each jumper
			processDataSimpleSession ( cleanDontWanted (
						SqliteStat.RjEvolution(sessionString, multisession, "AVG(", ")", showSex, maxJumps), 
						statsJumpsType, limit),
					false, dataColumns); //don't print AVG and SD at end of row (has no sense)
		} else {
			processDataSimpleSession ( cleanDontWanted (
						SqliteStat.RjEvolution(sessionString, multisession, "", "", showSex, maxJumps), 
						statsJumpsType, limit),
					false, dataColumns); //don't print AVG and SD at end of row (has no sense)
		}
	}
		
	public override string ToString () 
	{
		return "pending";
	}
}


