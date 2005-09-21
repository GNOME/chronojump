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
using System.IO; 	//TextWriter
using System.Xml;	//XmlTextWriter
using Gtk;		//FileSelection widget
using System.Collections; //ArrayList

public class ExportSession
{
	protected string [] myPersons;
	protected string [] myJumps;
	protected string [] myJumpsRj;
	protected string [] myRuns;
	protected string [] myRunsInterval;
	protected Session mySession;
	protected TextWriter writer;
	protected static Gtk.Window app1;
	protected static Gnome.AppBar myAppbar;
	protected string fileName;

	public ExportSession() {
	}

	public ExportSession(Session mySession, Gtk.Window app1, Gnome.AppBar mainAppbar) 
	{
		this.mySession = mySession;
		myAppbar = mainAppbar;
		
		checkFile("none");
	}

	protected void checkFile (string formatFile)
	{
		string exportString = "";
		if(formatFile == "report") {
			exportString = Catalog.GetString ("Save report in ");
		} else {
			exportString = Catalog.GetString ("Export session in " + formatFile + " format");
		}

			
		FileSelection fs = new FileSelection (exportString);
		fs.SelectMultiple = false;

		//from: http://www.gnomebangalore.org/?q=node/view/467
		if ( (Gtk.ResponseType) fs.Run () != Gtk.ResponseType.Ok) {
			Console.WriteLine("cancelled");
			//report does not currently send the appBar reference
			if(formatFile != "report") {
				myAppbar.Push ( Catalog.GetString ("Cancelled") );
			}
			fs.Hide ();
			return ;
		}

		fileName = fs.Filename;
		fs.Hide ();

		//add ".html" if needed, remember that on windows should be .htm
		fileName = addHtmlIfNeeded(fileName);

		try {
			if (File.Exists(fileName)) {
				Console.WriteLine("File {0} exists with attributes {1}, created at {2}", 
						fileName, File.GetAttributes(fileName), File.GetCreationTime(fileName));
				Console.WriteLine("Overwrite...");
				ConfirmWindow confirmWin = ConfirmWindow.Show(app1, Catalog.GetString("Are you sure you want to overwrite file: "), fileName);
				confirmWin.Button_accept.Clicked += new EventHandler(on_overwrite_file_accepted);
			} else {
				writer = File.CreateText(fileName);
				getData();
				printData();
				closeWriter();
			}
		} 
		catch {
			Console.WriteLine("cannot export to file: {0}", fileName);
			myAppbar.Push ( Catalog.GetString ("Cannot export to file: ") + fileName );
		}
		return;
	}

	private void on_overwrite_file_accepted(object o, EventArgs args)
	{
		writer = File.CreateText(fileName);
		getData();
		printData();
		closeWriter();
	}
		
	//remember on windows should be .htm
	private string addHtmlIfNeeded(string myFile)
	{
		int posOfDot = myFile.LastIndexOf('.');
		if (posOfDot == -1) {
			myFile += ".html";
		}
		return myFile;
	}
	
	protected virtual void getData() 
	{
		myPersons = SqlitePersonSession.SelectCurrentSession(mySession.UniqueID);
		myJumps= SqliteJump.SelectAllNormalJumps(mySession.UniqueID, "ordered_by_time");
		myJumpsRj = SqliteJump.SelectAllRjJumps(mySession.UniqueID, "ordered_by_time");
		myRuns= SqliteRun.SelectAllNormalRuns(mySession.UniqueID, "ordered_by_time");
		myRunsInterval = SqliteRun.SelectAllIntervalRuns(mySession.UniqueID, "ordered_by_time");
	}
	
	protected virtual void printData ()
	{
		printSessionInfo();
		printJumpers();
		printJumps();
		printJumpsRj();
		printRuns();
		printRunsInterval();
		printFooter();
	}

	protected virtual void writeData (ArrayList exportData) {
	}

	protected virtual void writeData (string exportData) {
	}

	
	protected virtual void printSessionInfo()
	{
		ArrayList myData = new ArrayList(2);
		myData.Add( "\n" + 
				Catalog.GetString ("SessionID") + ":" +
				Catalog.GetString ("Name") + ":" +
				Catalog.GetString ("Place") + ":" + 
				Catalog.GetString ("Date") + ":" + 
				Catalog.GetString ("Comments") );
		myData.Add ( mySession.UniqueID + ":" + mySession.Name + ":" +
					mySession.Place + ":" + mySession.Date + ":" + mySession.Comments );
		writeData(myData);
		writeData("VERTICAL-SPACE");
	}

	protected virtual void printJumpers()
	{
		ArrayList myData = new ArrayList(1);
		myData.Add ( "\n" + Catalog.GetString ("ID") + ":" + Catalog.GetString ("Name"));
		foreach (string jumperString in myPersons) {
			string [] myStr = jumperString.Split(new char[] {':'});
			
			myData.Add(myStr[0] + ":" + myStr[1]); 	//person.id, person.name 
		}
		writeData(myData);
		writeData("VERTICAL-SPACE");
	}

	protected void printJumps()
	{
		int dec=4; //decimals
		
		if(myJumps.Length > 0) {
			ArrayList myData = new ArrayList(1);
			myData.Add( "\n" + 
					Catalog.GetString("Jumper name") + ":" +
					Catalog.GetString("jump ID") + ":" + 
					Catalog.GetString("Type") + ":" + 
					"TC:" + 
					"TV:" + 
					Catalog.GetString("Fall") + ":" + 
					Catalog.GetString("Weight") + ":" + 
					Catalog.GetString("Height") + ":" +
					Catalog.GetString("Initial Speed") + ":" +
					Catalog.GetString("Description") );

			foreach (string jumpString in myJumps) {
				string [] myStr = jumpString.Split(new char[] {':'});

				myData.Add (	
						myStr[0] + ":" +  myStr[1] + ":" +  	//person.name, jump.uniqueID
						//myStr[2] + ":" +  myStr[3] + ":" +  	//jump.personID, jump.sessionID
						myStr[4] + ":" +  Util.TrimDecimals(myStr[6], dec) + ":" + 	//jump.type, jump.tc
						Util.TrimDecimals(myStr[5], dec) + ":" +  myStr[7] + ":" + 	//jump.tv, jump.fall
						myStr[8] + ":" + 		//jump.weight,
						Util.TrimDecimals(Util.GetHeightInCentimeters(myStr[5]), dec) + ":" +  
						Util.TrimDecimals(Util.GetInitialSpeed(myStr[5]), dec) + ":" +  
						myStr[9]		//jump.description
					   );
			}
			writeData(myData);
			writeData("VERTICAL-SPACE");
		}
	}

	protected void printJumpsRj()
	{
		int dec=4; //decimals
		
		foreach (string jump in myJumpsRj) {
			ArrayList myData = new ArrayList(1);

			myData.Add( "\n" + 
					Catalog.GetString("Jumper name") + ":" + 
					Catalog.GetString("jump ID") + ":" + 
					Catalog.GetString("jump Type") + ":" + 
					Catalog.GetString("TC Max") + ":" + 
					Catalog.GetString("TV Max") + ":" + 
					Catalog.GetString("Max Height") + ":" +
					Catalog.GetString("Max Initial Speed") + ":" +
					Catalog.GetString("TC AVG") + ":" + 
					Catalog.GetString("TV AVG") + ":" + 
					Catalog.GetString("AVG Height") + ":" +
					Catalog.GetString("AVG Initial Speed") + ":" +
					Catalog.GetString("Fall") + ":" + 
					Catalog.GetString("Weight") + ":" + 
					Catalog.GetString("Jumps") + ":" + 
					Catalog.GetString("Time") + ":" + 
					Catalog.GetString("Limited") + ":" + 
					Catalog.GetString("Description" )
				  );
			
			string [] myStr = jump.Split(new char[] {':'});
			myData.Add ( "\n" + 
					myStr[0] + ":" +  myStr[1] + ":" +  	//person.name, jumpRj.uniqueID
					//myStr[2] + ":" +  myStr[3] + ":" +  	//jumpRj.personID, jumpRj.sessionID
					myStr[4] + ":" +  		//jumpRj.type 
					Util.TrimDecimals(myStr[6], dec) + ":" +  		//jumpRj.tcMax 
					Util.TrimDecimals(myStr[5], dec) + ":" + 		//jumpRj.tvMax
					Util.TrimDecimals(Util.GetHeightInCentimeters(myStr[5]), dec) + ":" +  	//Max height
					Util.TrimDecimals(Util.GetInitialSpeed(myStr[5]), dec) + ":" +  	//Max initial speed
					Util.TrimDecimals(myStr[11], dec) + ":" +  		//jumpRj.tcAvg
					Util.TrimDecimals(myStr[10], dec) + ":" + 		//jumpRj.tvAvg
					Util.TrimDecimals(Util.GetHeightInCentimeters(myStr[10]), dec) + ":" +  //Avg height
					Util.TrimDecimals(Util.GetInitialSpeed(myStr[10]), dec) + ":" +  	//Avg Initial speed
					myStr[7] + ":" + 	 	//jumpRj.Fall
					myStr[8] + ":" +  myStr[14] + ":" + 	//jumpRj.Weight, jumpRj.Jumps
					Util.TrimDecimals(myStr[15], dec) + ":" +  myStr[16] + ":" + 	//jumpRj.Time, jumpRj.Limited
					myStr[9]		//jumpRj.Description
					);
			
			writeData(myData);
			myData = new ArrayList(1);
			//print tvString and tcString
			string [] tvString = myStr[12].Split(new char[] {'='});
			string [] tcString = myStr[13].Split(new char[] {'='});
			int count = 0;
			myData.Add( Catalog.GetString("Count") + ":TC:TV" );
			foreach(string myTv in tvString) {
				myData.Add((count+1).ToString() + ":" + 
						Util.TrimDecimals(tcString[count], dec) + ":" + 
						Util.TrimDecimals(myTv, dec));
				count ++;
			}
			writeData(myData);
			writeData("VERTICAL-SPACE");
		}
	}
	
	protected void printRuns()
	{
		int dec=4; //decimals
		
		if(myRuns.Length > 0) {
			ArrayList myData = new ArrayList(1);
			myData.Add( "\n" + 
					Catalog.GetString("Runner name") + ":" +
					Catalog.GetString("run ID") + ":" + 
					Catalog.GetString("Type") + ":" + 
					Catalog.GetString("Distance") + ":" + 
					Catalog.GetString("Time") + ":" + 
					Catalog.GetString("Speed") + ":" + 
					Catalog.GetString("Description") );

			foreach (string runString in myRuns) {
				string [] myStr = runString.Split(new char[] {':'});

				myData.Add (
						myStr[0] + ":" +  myStr[1] + ":" +  	//person.name, run.uniqueID
						myStr[4] + ":" +  myStr[5] + ":" + 	//run.type, run.distance
						Util.TrimDecimals(myStr[6], dec) + ":" +  	//run.time
						Util.TrimDecimals(Util.GetSpeed(myStr[5], myStr[6]), dec) + ":" + //speed
						myStr[7]		//run.description
					   );
			}
			writeData(myData);
			writeData("VERTICAL-SPACE");
		}
	}
	
	protected void printRunsInterval()
	{
		int dec=4; //decimals
		
		foreach (string runString in myRunsInterval) {
			ArrayList myData = new ArrayList(1);

			myData.Add( "\n" + 
					Catalog.GetString("Runner name") + ":" +
					Catalog.GetString("run ID") + ":" + 
					Catalog.GetString("Type") + ":" + 
					Catalog.GetString("Distance total") + ":" + 
					Catalog.GetString("Time total") + ":" +
					Catalog.GetString("Average speed") + ":" +
					Catalog.GetString("Distance interval") + ":" + 
					Catalog.GetString("Tracks") + ":" + 
					Catalog.GetString("Limited") + ":" +
					Catalog.GetString("Description") );


			string [] myStr = runString.Split(new char[] {':'});
			myData.Add (
					myStr[0] + ":" +  myStr[1] + ":" +  	//person.name, run.uniqueID
					myStr[4] + ":" +  Util.TrimDecimals(myStr[5], dec) + ":" + 	//run.type, run.distancetotal
					Util.TrimDecimals(myStr[6], dec) + ":" +  		//run.timetotal
					Util.TrimDecimals(Util.GetSpeed(myStr[5], myStr[6]), dec) + ":" + 	//speed AVG
					myStr[7] + ":" + 	 	//run.distanceInterval
					myStr[9] + ":" +  myStr[11] + ":" + 	//tracks, limited
					myStr[10]		//description
				   );
			writeData(myData);

			myData = new ArrayList(1);
			//print intervalTimesString
			string [] timeString = myStr[8].Split(new char[] {'='});
			myData.Add( Catalog.GetString ("Count") + ":" + 
					Catalog.GetString ("Interval speed") + ":" + 
					Catalog.GetString("interval times") );
			int count = 1;
			foreach(string myTime in timeString) {
				myData.Add((count++).ToString() + ":" + 
						Util.TrimDecimals(Util.GetSpeed(myStr[7], myTime), dec) + ":" + 
						Util.TrimDecimals(myTime, dec)
						);
			}
			writeData(myData);
			writeData("VERTICAL-SPACE");
		}
	}
	
	protected virtual void printFooter()
	{
	}
	
	protected void closeWriter ()
	{
		((IDisposable)writer).Dispose();
	}
	
	~ExportSession() {}
}


public class ExportSessionCSV : ExportSession 
{
	
	public ExportSessionCSV(Session mySession, Gtk.Window app1, Gnome.AppBar mainAppbar) 
	//public ExportCSV(Session mySession, Gtk.Window app1) 
	{
		this.mySession = mySession;
		myAppbar = mainAppbar;
		checkFile("CSV");
	}

	protected override void writeData (ArrayList exportData) {
		for(int i=0; i < exportData.Count ; i++) {
			exportData[i] = exportData[i].ToString().Replace(":", ", ");
			writer.WriteLine( exportData[i] );
		}
	}
	
	protected override void writeData (string exportData) {
		//do nothing
	}

	protected override void printFooter()
	{
		Console.WriteLine( "Correctly exported" );
		myAppbar.Push ( Catalog.GetString ("Exported to file: ") + fileName );
	}
	
	~ExportSessionCSV() {}
}

public class ExportSessionXML : ExportSession 
{
	private XmlTextWriter xr;
		
	public ExportSessionXML(Session mySession, Gtk.Window app1, Gnome.AppBar mainAppbar) 
	//public ExportXML(Session mySession, Gtk.Window app1) 
	{
		this.mySession = mySession;
		//this.app1 = app1;
		myAppbar = mainAppbar;
		
		//xr = new XmlTextWriter(fileExport, null);
		//xr.Formatting = Formatting.Indented;
		//xr.Indentation = 4;
		
		checkFile("XML");
	}
	

	//public void printData(string [] myJumps)
	//{
	//	Console.WriteLine("print data export XML");
		/*

		xr.WriteStartDocument();
		xr.WriteComment("Exported File:");

		//for (int i=0; i < persons.NextIndex; i++)
		foreach (string jump in myJumps) {
		{
			xr.WriteStartElement(jump[i]);

			//put all this in session.cs 
			//and person.cs and jump.cs
			//do it as myPerson.ExportXML()

			xr.WriteElementString("Name", persons[i].Name);
			xr.WriteElementString("dateBorn", persons[i].DateBorn);
			//xr.WriteElementString("level", persons[i].Level);
			xr.WriteElementString("description", persons[i].Description);

			xr.WriteEndElement();
		}

		Console.WriteLine("Saved as: {0}", FilePersons);
		*/
	//}

	protected override void printFooter()
	{
		//xr.Flush();
		//xr.Close();
	}
	
	~ExportSessionXML() {}
}
