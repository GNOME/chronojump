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

using NPlot.Gtk;
using NPlot;
using System.Drawing;
using System.Drawing.Imaging;


public class GraphSjCmjAbk : StatSjCmjAbk
{
	
	public GraphSjCmjAbk (ArrayList sessions, int newPrefsDigitsNumber, string jumpType, bool showSex, int statsJumpsType, int limit) 
	{
		by1Values = new ArrayList(2); 
		by100Values = new ArrayList(2);
		this.dataColumns = 1; //for Simplesession
		this.jumpType = jumpType;
		this.limit = limit;
		
		completeConstruction (treeview, sessions, newPrefsDigitsNumber, showSex, statsJumpsType);
	
		/*
		if (statsJumpsType == 2) {
			this.operation = "MAX";
		} else {
			this.operation = "AVG";
		}
		*/

		this.windowTitle = Catalog.GetString("ChronoJump graph");
		if(sessions.Count > 1) {
			this.graphTitle = jumpType + Catalog.GetString(" values chart of multiple sessions");
		} else {
			this.graphTitle = jumpType + Catalog.GetString(" values chart of single session");
		}
		plotIndexes = false;
	}

	protected override void printData (string [] statValues) 
	{
		//add jump to by1Values (as a string separated by ':')
		string myReturn = "";
		int i=0;
		foreach (string myValue in statValues) {
			if(i == 0) {
				//don't plot AVG and SD rows
				if( myValue == Catalog.GetString("AVG") || myValue == Catalog.GetString("SD") ) {
					return;
				}
		
				//this separates "ABK" from "ABK (M)", "ABK (F)", and also "ABK"
				string [] myValueWithSex = myValue.ToString().Split(new char[] {' '});
				string myValue2 = myValueWithSex[0];
			}
			if(i > 0) {
				myReturn = myReturn + ":";
			}
			myReturn = myReturn + myValue;
			i++;
		}
		by1Values.Add(myReturn);
	}
}
