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


public class GraphRjIndex : StatRjIndex
{
	protected string operation;

	public GraphRjIndex (ArrayList sessions, int newPrefsDigitsNumber, bool showSex, int statsJumpsType, int limit) 
	{
		//by1Values = new ArrayList(2); 
		by100Values = new ArrayList(2);
		this.dataColumns = 4; //for Simplesession (index, tv(avg), tc(avg), fall)
		this.valuesTransposed = new ArrayList(2);
	
		valuesSchemaIndex = new ArrayList(dataColumns);
		valuesSchemaIndex.Add ("true"); //Index
		valuesSchemaIndex.Add ("false"); //TV
		valuesSchemaIndex.Add ("false"); //TC
		valuesSchemaIndex.Add ("true"); //fall

		colorSchema = new ArrayList (dataColumns);
		colorSchema.Add ("Red");		//Index
		colorSchema.Add ("LightBlue");		//TV
		colorSchema.Add ("LightGreen");		//TC
		colorSchema.Add ("Chocolate");		//fall

		jumperNames = new ArrayList(2);
		
		this.jumpType = jumpType;
		this.limit = limit;
		
		completeConstruction (treeview, sessions, newPrefsDigitsNumber, showSex, statsJumpsType);
	
		if (statsJumpsType == 2) {
			this.operation = "MAX";
		} else {
			this.operation = "AVG";
		}

		this.windowTitle = Catalog.GetString("ChronoJump graph");
		if(sessions.Count > 1) {
			this.graphTitle = "Rj Index " + operation + Catalog.GetString(" values chart of multiple sessions");
		} else {
			this.graphTitle = "Rj Index " + operation + Catalog.GetString(" values chart of single session");
		
			//initialize valuesTransposed (with one row x each column name, except the first)
			bool firstValue = true;
			foreach (string myCol in columnsString) {
				if (! firstValue) {
					valuesTransposed.Add(myCol);
				}
				firstValue = false;
			}
		}
		resultCombined = false;
		resultIsIndex = true;
		labelLeft = Catalog.GetString("TV, TC (seconds)");
		labelRight = Catalog.GetString("Index(%), fall(cm)");
	}

	protected override void printData (string [] statValues) 
	{
		if(sessions.Count == 1) {
			int i = 0;
			//we need to save this transposed
			foreach (string myValue in statValues) 
			{
				if(i == 0) {
					//don't plot AVG and SD rows
					if( myValue == Catalog.GetString("AVG") || myValue == Catalog.GetString("SD") ) {
						return;
					}
					jumperNames.Add(myValue);
				} else {
					valuesTransposed[i-1] = valuesTransposed[i-1] + ":" + myValue;
				}
				i++;
			}
		} else {
			//add jump to by100Values (as a string separated by ':')
			string myReturn = "";
			int i=0;
			foreach (string myValue in statValues) {
				if(i == 0) {
					//don't plot AVG and SD rows
					if( myValue == Catalog.GetString("AVG") || myValue == Catalog.GetString("SD") ) {
						return;
					}
				}
				if(i > 0) {
					myReturn = myReturn + ":";
				}
				myReturn = myReturn + myValue;
				i++;
			}
			by100Values.Add(myReturn);
		}
	}
}
