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
using System.Text; //StringBuilder
using Gtk;
using System.Collections; //ArrayList


//no weight
public class ReportSjCmjAbk : StatSjCmjAbk
{
	//string tableString;
	
	public ReportSjCmjAbk (ArrayList sessions, int newPrefsDigitsNumber, string jumpType, bool showSex, int statsJumpsType, int limit, bool heightPreferred) 
	{
		//make stats/main.cs CreateGraph and printData work for report
		toReport = true;
		
		this.dataColumns = 2;	//for simplesession
		this.jumpType = jumpType;
		this.limit = limit;
		this.heightPreferred = heightPreferred;
		
		if(sessions.Count > 1) {
			store = getStore(sessions.Count +3); //+3 (for jumper, the AVG horizontal and SD horizontal)
		} else {
			store = getStore(dataColumns +1); //jumper, height, TV
		}
		
		completeConstruction (treeview, sessions, newPrefsDigitsNumber, showSex, statsJumpsType);
		string [] columnsString = { Catalog.GetString("Jump"), 
			Catalog.GetString("Height"), Catalog.GetString("TV") };

		prepareHeaders(columnsString);
	}

	protected override void prepareHeaders(string [] columnsString) 
	{
		string myHeaderString = "";
		myHeaderString += "<p><TABLE BORDER=\"1\">\n";
		myHeaderString += "<TR><TH>" + Catalog.GetString(columnsString[0]) + "</TH>";
		
		if(sessions.Count > 1) {
			string [] stringFullResults;
			for (int i=0; i < sessions.Count ; i++) {
				stringFullResults = sessions[i].ToString().Split(new char[] {':'});
				
				myHeaderString += "<TH>" + stringFullResults[1] + "\n" + 
					stringFullResults[2] + "\n" + Catalog.GetString(columnsString[1]) + "</TH>"; //name, date, col name
			}
			//if multisession, add AVG and SD cols
			myHeaderString += "<TH>" + Catalog.GetString("AVG") + "</TH>";
			myHeaderString += "<TH>" + Catalog.GetString("SD") + "</TH>";
		} else {
			for(int i=1 ; i <= dataColumns ; i++) {
				myHeaderString += "<TH>" + columnsString[i] + "</TH>";
			}
		}
		myHeaderString += "</TR>\n";

		//copy to reportString variable
		reportString = myHeaderString;
		
		/*
		treeview.HeadersVisible=true;
		treeview.AppendColumn (Catalog.GetString(columnsString[0]), new CellRendererText(), "text", 0);

		int i;
		if(sessions.Count > 1) {
			string myHeaderString = "";
			string [] stringFullResults;
			for (i=0; i < sessions.Count ; i++) {
				//we need to know the name of the column: session
				stringFullResults = sessions[i].ToString().Split(new char[] {':'});
				myHeaderString = stringFullResults[1] + "\n" + 
					stringFullResults[2] + "\n" + Catalog.GetString(columnsString[1]); //name, date, col name
				treeview.AppendColumn (myHeaderString, new CellRendererText(), "text", i+1); 
			}
			//if multisession, add AVG and SD cols
			treeview.AppendColumn (Catalog.GetString("AVG"), new CellRendererText(), "text", i+1); 
			treeview.AppendColumn (Catalog.GetString("SD"), new CellRendererText(), "text", i+2);
		} else {
			treeview.AppendColumn (Catalog.GetString(columnsString[1]), new CellRendererText(), "text", 1); 
			//if there's only one session, add extra data columns if needed
			for(i=2 ; i <= dataColumns ; i++) {
				treeview.AppendColumn (columnsString[i], new CellRendererText(), "text", i);
			}
		}
		*/
	}


	/*
	protected override void printData(string [] statValues) 
	{
		reportString += "<TR>";
		for (int i=0; i < statValues.Length ; i++) {
			reportString += "<TD>" + statValues[i] + "</TD>";
		}
		reportString += "</TR>\n";
		//iter = store.AppendValues (statValues); 
	}
	*/

	public override string ToString() {
		return reportString + "</TABLE></p>\n";
	}

}

