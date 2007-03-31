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
using Mono.Unix;


public class TreeViewPulses : TreeViewEvent
{

	public TreeViewPulses ()
	{
	}
	
	public TreeViewPulses (Gtk.TreeView treeview, int newPrefsDigitsNumber)
	{
		this.treeview = treeview;
		pDN = newPrefsDigitsNumber;

		treeviewHasTwoLevels = true;
		dataLineNamePosition = 0; //position of name in the data to be printed
		dataLineTypePosition = 4; //position of type in the data to be printed
		allEventsName = Constants.AllPulsesName;
		eventIDColumn = 4; //column where the uniqueID of event will be (and will be hidded)
		
		string jumperName = Catalog.GetString("Person");
		string timeName = Catalog.GetString("Time") + "\n(s)";
		string diffName = Catalog.GetString("Difference");
		string diffPercentName = Catalog.GetString("Difference") + "\n(%)";

		columnsString = new string[]{jumperName, timeName, diffName, diffPercentName};
		store = getStore(columnsString.Length +1); //+1 because, eventID is not show in last col
		treeview.Model = store;
		prepareHeaders(columnsString);
	}
	
	protected override System.Object getObjectFromString(string [] myStringOfData) {
		Pulse myPulse = new Pulse();
		myPulse.UniqueID = Convert.ToInt32(myStringOfData[1].ToString()); 
		myPulse.Type = myStringOfData[4].ToString();
		myPulse.FixedPulse = Convert.ToDouble(myStringOfData[5].ToString());
		myPulse.TimesString = myStringOfData[7].ToString();

		return myPulse;
	}

	protected override string [] getLineToStore(System.Object myObject)
	{
		Pulse newPulse = (Pulse)myObject;
		
		//if fixedPulse is not defined, comparate each pulse with the averave
		string myTypeComplet ="";
		if(newPulse.FixedPulse == -1) 
			myTypeComplet = newPulse.Type;
		else
			myTypeComplet = newPulse.Type + "(" + Util.TrimDecimals(newPulse.FixedPulse.ToString(), 3) + ")";
		
		
		string [] myData = new String [getColsNum()];
		int count = 0;
		myData[count++] = myTypeComplet;
		/*
		   myData[count++] = Util.TrimDecimals(Util.GetAverage(newPulse.TimesString).ToString(), pDN);
		myData[count++]	= "+/- " + Util.TrimDecimals( newPulse.GetErrorAverage(false).ToString(), pDN );
		myData[count++]	= "+/- " + Util.TrimDecimals( newPulse.GetErrorAverage(true).ToString(), pDN );
		*/
		myData[count++]	= "";
		myData[count++]	= "";
		myData[count++]	= "";
		myData[count++] = newPulse.UniqueID.ToString();
		return myData;
	}

	
	protected override string [] getSubLineToStore(System.Object myObject, int lineCount)
	{
		Pulse newPulse = (Pulse)myObject;

		//check the time
		string [] myStringFull = newPulse.TimesString.Split(new char[] {'='});
		string timeInterval = myStringFull[lineCount];


		//if fixedPulse is not defined, comparate each pulse with the averave
		double pulseToComparate = 0;
		if(newPulse.FixedPulse == -1) 
			pulseToComparate = Util.GetAverage(newPulse.TimesString);
		else
			pulseToComparate = newPulse.FixedPulse;

		
		double absoluteError = Convert.ToDouble(timeInterval) - pulseToComparate;
		double relativeError = absoluteError * 100 / pulseToComparate;
		
		
		//write line for treeview
		string [] myData = new String [getColsNum()];
		int count = 0;
		myData[count++] = (lineCount +1).ToString();
		myData[count++] = Util.TrimDecimals( timeInterval, pDN );
		myData[count++] = Util.TrimDecimals( absoluteError.ToString(), pDN );
		myData[count++] = Util.TrimDecimals( relativeError.ToString(), pDN );

		myData[count++] = newPulse.UniqueID.ToString(); 
		return myData;
	}
	
	protected override string [] printTotal(System.Object myObject, int cols) {
		Pulse newPulse = (Pulse)myObject;

		string [] myData = new String [getColsNum()];
		int count = 0;
		myData[count++] = Catalog.GetString("Total");
		myData[count++] = Util.TrimDecimals( Util.GetTotalTime(newPulse.TimesString).ToString(), pDN );
		myData[count++] = ""; 
		myData[count++] = "";
		myData[count++] = newPulse.UniqueID.ToString(); 
		
		return myData;
	}
	
	protected override string [] printAVG(System.Object myObject, int cols) {
		Pulse newPulse = (Pulse)myObject;

		string [] myData = new String [getColsNum()];
		int count = 0;
		myData[count++] = Catalog.GetString("AVG");
		myData[count++] = Util.TrimDecimals( Util.GetAverage(newPulse.TimesString).ToString(), pDN );
		myData[count++] = "";
		myData[count++] = "";
		myData[count++] = newPulse.UniqueID.ToString(); 
		
		return myData;
	}
	
	protected override string [] printSD(System.Object myObject, int cols) {
		Pulse newPulse = (Pulse)myObject;

		string [] myData = new String [getColsNum()];
		int count = 0;
		myData[count++] = Catalog.GetString("SD");
		myData[count++] = Util.TrimDecimals( Util.CalculateSD(
			Util.ChangeEqualForColon(newPulse.TimesString),
			Util.GetTotalTime(newPulse.TimesString),
			Util.GetNumberOfJumps(newPulse.TimesString, false)).ToString(),
				pDN );
		myData[count++] = "";
		myData[count++] = "";
		myData[count++] = newPulse.UniqueID.ToString(); 
		
		return myData;
	}
	
	
	protected override int getNumOfSubEvents(System.Object myObject)
	{
		Pulse newPulse = (Pulse)myObject;

		string [] myStringFull = newPulse.TimesString.Split(new char[] {'='});

		return myStringFull.Length; 
	}
	
}
