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


public class GraphIeIub : StatIeIub
{
	protected string operation;

	public GraphIeIub (ArrayList sessions, string indexType, int newPrefsDigitsNumber, bool showSex, int statsJumpsType, int limit) 
	{
		//by1Values = new ArrayList(2); 
		by100Values = new ArrayList(2);
		this.dataColumns = 3; //for Simplesession (index, jump1, jump2)
		this.valuesTransposed = new ArrayList(2);
	
		valuesSchemaIndex = new ArrayList(dataColumns);
		valuesSchemaIndex.Add ("true"); //Index
		valuesSchemaIndex.Add ("false"); //jump1
		valuesSchemaIndex.Add ("false"); //jump2

		colorSchema = new ArrayList (dataColumns);
		colorSchema.Add ("Red");		//Index
		colorSchema.Add ("LightBlue");		//jump1
		colorSchema.Add ("LightGreen");		//jump2

		jumperNames = new ArrayList(2);
		
		this.jumpType = jumpType;
		this.limit = limit;
		
		this.indexType = indexType; //"IE" or "IUB"
		if(indexType == "IE") {
			jump1="CMJ";
			jump2="SJ";
		} else { //IUB
			jump1="ABK";
			jump2="CMJ";
		}
		columnsString[0] = "Jumper";
		columnsString[1] = indexType;
		columnsString[2] = jump1;
		columnsString[3] = jump2;
		
		completeConstruction (treeview, sessions, newPrefsDigitsNumber, showSex, statsJumpsType);
	
		if (statsJumpsType == 2) {
			this.operation = "MAX";
		} else {
			this.operation = "AVG";
		}

		this.windowTitle = Catalog.GetString("ChronoJump graph");
		if(sessions.Count > 1) {
			this.graphTitle = indexType + " " + operation + Catalog.GetString(" values chart of multiple sessions");
		} else {
			this.graphTitle = indexType + " " + operation + Catalog.GetString(" values chart of single session");
			
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
		labelLeft = Catalog.GetString("seconds");
		labelRight = Catalog.GetString("Index(%)");
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
