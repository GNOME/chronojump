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


public class GraphRjEvolution : StatRjEvolution
{
	protected string operation;
	private Random myRand = new Random();
	private int countRows;

	public GraphRjEvolution (ArrayList sessions, int newPrefsDigitsNumber, bool showSex, int statsJumpsType, int limit) 
	{
		string sessionString = obtainSessionSqlString(sessions);
		//we need to know the reactive with more jumps for prepare columns
		maxJumps = SqliteStat.ObtainMaxNumberOfJumps(sessionString);
		
		this.dataColumns = maxJumps*2 + 2;	//for simplesession (index, (tv , tc)*jumps, fall)
		//this.dataColumns = 4; //for Simplesession (index, tv(avg), tc(avg), fall)
		this.valuesTransposed = new ArrayList(0);
	
		//two x jumper (row) (in printData we put values and increment size of this)
		valuesSchemaIndex = new ArrayList(0);
		colorSchema = new ArrayList (0);
		
		//in X axe, we print the number of jumps, not the jumperNames
		//this should be equal to the number of jumps
		jumperNames = new ArrayList(0);
		for(int i=0; i<maxJumps ; i++) {
			jumperNames.Add((i+1).ToString());
		}
		
		countRows = 0;
		
		this.jumpType = jumpType;
		this.limit = limit;
		
		completeConstruction (treeview, sessions, newPrefsDigitsNumber, showSex, statsJumpsType);
	
		if (statsJumpsType == 2) {
			this.operation = "MAX";
		} else {
			this.operation = "AVG";
		}

		this.windowTitle = Catalog.GetString("ChronoJump graph");
		this.graphTitle = "Rj Evolution " + operation + Catalog.GetString(" values chart of single session");
		
		
		resultCombined = false;
		resultIsIndex = false;
		labelLeft = Catalog.GetString("TC (secs.)");
		labelRight = Catalog.GetString("TV (secs.)");

		//make the X and the Y be equal
		//initial values for being replaced
		fixedLeftBottom = 1;
		fixedRightBottom = 1;
		fixedLeftTop = 0;
		fixedRightTop = 0;
	}

	protected override void printData (string [] statValues) 
	{
		int i = 0;
		//we need to save this transposed
		
		foreach (string myValue in statValues) 
		{
			if(i==0) {
				valuesTransposed.Add(myValue + " TC");
				valuesTransposed.Add(myValue + " TV");

				//one false, other true for having same color and different shape
				valuesSchemaIndex.Add ("false"); //TC
				valuesSchemaIndex.Add ("true"); //TV
				
				//FIXME: don't work, stats/main.cs expects a colorname
				int myR = myRand.Next(255);
				int myG = myRand.Next(255);
				int myB = myRand.Next(255);
				colorSchema.Add (Color.FromArgb(myR, myG, myB).ToString());	//TC
				colorSchema.Add ( (Color.FromArgb(myR, myG, myB)).ToString() );	//TV
			} else if(isTC(i)) {
				valuesTransposed[countRows*2] = valuesTransposed[countRows*2] + ":" + myValue;
		
				//make the X and the Y be equal
				if(myValue != "-") {
					if(Convert.ToDouble(myValue) < fixedLeftBottom ) { 
						fixedLeftBottom = (float) Convert.ToDouble(myValue); 
						fixedRightBottom = (float) Convert.ToDouble(myValue);
					}
					if(Convert.ToDouble(myValue) > fixedLeftTop ) { 
						fixedLeftTop = (float) Convert.ToDouble(myValue);
						fixedRightTop = (float) Convert.ToDouble(myValue);
					}
				}
			} else if(isTV(i)) {
				valuesTransposed[countRows*2 +1] = valuesTransposed[countRows*2 +1] + ":" + myValue;
				
				//make the X and the Y be equal
				if(myValue != "-") {
					if(Convert.ToDouble(myValue) < fixedLeftBottom ) { 
						fixedLeftBottom = (float) Convert.ToDouble(myValue); 
						fixedRightBottom = (float) Convert.ToDouble(myValue);
					}
					if(Convert.ToDouble(myValue) > fixedLeftTop ) { 
						fixedLeftTop = (float) Convert.ToDouble(myValue);
						fixedRightTop = (float) Convert.ToDouble(myValue);
					}
				}
			}
			i++;
		}

		countRows ++;
	}

	private bool isTC(int col) {
		for (int i=0; i < maxJumps ; i++) {
			if (i*2 +2 == col) { //TC cols: 2, 4, 6, 8, ...
				return true;
			}
		}
		return false;
	}
	
	private bool isTV(int col) {
		for (int i=0; i < maxJumps ; i++) {
			if (i*2 +3 == col) { //TV cols: 3, 5, 6, 9, ...
				return true;
			}
		}
		return false;
	}
}
