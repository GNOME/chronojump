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
		string exportString = Catalog.GetString ("Export session in " + formatFile + " format");
		FileSelection fs = new FileSelection (exportString);
		fs.SelectMultiple = false;

		//from: http://www.gnomebangalore.org/?q=node/view/467
		if ( (Gtk.ResponseType) fs.Run () != Gtk.ResponseType.Ok) {
			Console.WriteLine("cancelled");
			myAppbar.Push ( Catalog.GetString ("Cancelled") );
			fs.Hide ();
			return ;
		}

		fileName = fs.Filename;
		fs.Hide ();

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
		printHeader();
		printJumpers();
		printJumps();
		printJumpsRj();
		printRuns();
		printRunsInterval();
		printFooter();
	}

	protected virtual void printHeader()
	{
	}

	protected virtual void printJumpers()
	{
	}

	protected virtual void printJumps()
	{
	}

	protected virtual void printJumpsRj()
	{
	}
	
	protected virtual void printRuns()
	{
	}
	
	protected virtual void printRunsInterval()
	{
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
	
	protected override void printHeader()
	{
		//writer.WriteLine( Catalog.GetString ( "SessionID, Name, Place, Date, Comments" ) );
		writer.WriteLine( 
				Catalog.GetString ("SessionID") + ", " +
				Catalog.GetString ("Name") + ", " +
				Catalog.GetString ("Place") + ", " + 
				Catalog.GetString ("Date") + ", " + 
				Catalog.GetString ("Comments") );
		writer.WriteLine( "{0}, {1}, {2}, {3}, {4}", mySession.UniqueID, mySession.Name, 
					mySession.Place, mySession.Date, mySession.Comments );
	}
	
	protected override void printJumpers()
	{
		writer.WriteLine( "\n" + Catalog.GetString ( "Jumpers" ) );
		foreach (string jumperString in myPersons) {
			string [] myStr = jumperString.Split(new char[] {':'});
			
			writer.WriteLine ("{0}, {1}", 
					myStr[0], myStr[1] 	//person.id, person.name 
					);
		}
	}

	protected override void printJumps()
	{
		writer.WriteLine( "\n" + Catalog.GetString ( "Normal Jumps" ) );
		writer.WriteLine( "\n" + 
				Catalog.GetString("Jumper name") + ", " +
				Catalog.GetString("jump ID") + ", " + 
				Catalog.GetString("Type") + ", " + 
				"TV, " + 
				"TC, " + 
				Catalog.GetString("Fall") + ", " + 
				Catalog.GetString("Weight") + ", " + 
				Catalog.GetString("Height") + ", " +
				Catalog.GetString("Initial Speed") + ", " +
				Catalog.GetString("Description") );
		
		foreach (string jumpString in myJumps) {
			string [] myStr = jumpString.Split(new char[] {':'});
			
			writer.WriteLine ("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}", 
					myStr[0], myStr[1], 	//person.name, jump.uniqueID
					//myStr[2], myStr[3], 	//jump.personID, jump.sessionID
					myStr[4], myStr[5],	//jump.type, jump.tv
					myStr[6], myStr[7],	//jump.tc, jump.fall
					myStr[8],		//jump.weight,
					Util.GetHeightInCentimeters(myStr[5]), 
					Util.GetInitialSpeed(myStr[5]), 
					myStr[9]		//jump.description
					);
		}
	}

	protected override void printJumpsRj()
	{
		writer.WriteLine( "\n" + Catalog.GetString ( "Reactive Jumps" ) );
		
		foreach (string jump in myJumpsRj) {
			string [] myStr = jump.Split(new char[] {':'});

			writer.WriteLine( "\n" + 
					Catalog.GetString("Jumper name") + ", " + 
					Catalog.GetString("jump ID") + ", " + 
					Catalog.GetString("jump Type") + ", " + 
					Catalog.GetString("TC Max") + ", " + 
					Catalog.GetString("TV Max") + ", " + 
					Catalog.GetString("Max Height") + ", " +
					Catalog.GetString("Max Initial Speed") + ", " +
					Catalog.GetString("TC AVG") + ", " + 
					Catalog.GetString("TV AVG") + ", " + 
					Catalog.GetString("AVG Height") + ", " +
					Catalog.GetString("AVG Initial Speed") + ", " +
					Catalog.GetString("Fall") + ", " + 
					Catalog.GetString("Weight") + ", " + 
					Catalog.GetString("Jumps") + ", " + 
					Catalog.GetString("Time") + ", " + 
					Catalog.GetString("Limited") + ", " + 
					Catalog.GetString("Description" ) );
			writer.WriteLine ("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}", 
					myStr[0], myStr[1], 	//person.name, jumpRj.uniqueID
					//myStr[2], myStr[3], 	//jumpRj.personID, jumpRj.sessionID
					myStr[4], 		//jumpRj.type 
					myStr[6], 		//jumpRj.tcMax 
					myStr[5],		//jumpRj.tvMax
					Util.GetHeightInCentimeters(myStr[5]), 	//Max height
					Util.GetInitialSpeed(myStr[5]), 	//Max initial speed
					myStr[11], 		//jumpRj.tcAvg
					myStr[10],		//jumpRj.tvAvg
					Util.GetHeightInCentimeters(myStr[10]), //Avg height
					Util.GetInitialSpeed(myStr[10]), 	//Avg Initial speed
					myStr[7],	 	//jumpRj.Fall
					myStr[8], myStr[14],	//jumpRj.Weight, jumpRj.Jumps
					myStr[15], myStr[16],	//jumpRj.Time, jumpRj.Limited
					myStr[9]		//jumpRj.Description
					);
			
			//print tvString and tcString
			string [] tvString = myStr[12].Split(new char[] {'='});
			string [] tcString = myStr[13].Split(new char[] {'='});
			int count = 0;
			writer.WriteLine( Catalog.GetString ( "TV, TC" ) );
			foreach(string myTv in tvString) {
				writer.WriteLine("{0}, {1}", myTv, tcString[count]);
				count ++;
			}
		}
	}
	
	protected override void printRuns()
	{
		writer.WriteLine( "\n" + Catalog.GetString ( "Normal Runs" ) );
		writer.WriteLine( "\n" + 
				Catalog.GetString("Runner name") + ", " +
				Catalog.GetString("run ID") + ", " + 
				Catalog.GetString("Type") + ", " + 
				Catalog.GetString("Distance") + ", " + 
				Catalog.GetString("Time") + ", " + 
				Catalog.GetString("Speed") + ", " + 
				Catalog.GetString("Description") );
		
		foreach (string runString in myRuns) {
			string [] myStr = runString.Split(new char[] {':'});
			
			writer.WriteLine ("{0}, {1}, {2}, {3}, {4}, {5}", 
					myStr[0], myStr[1], 	//person.name, run.uniqueID
					myStr[4], myStr[5],	//run.type, run.distance
					myStr[6], Util.GetSpeed(myStr[5], myStr[6]),	//run.time, speed
					myStr[7]		//run.description
					);
		}
	}

	protected override void printRunsInterval()
	{
		writer.WriteLine( "\n" + Catalog.GetString ( "Interval Runs" ) );
		writer.WriteLine( "\n" + 
				Catalog.GetString("Runner name") + ", " +
				Catalog.GetString("run ID") + ", " + 
				Catalog.GetString("Type") + ", " + 
				Catalog.GetString("Distance total") + ", " + 
				Catalog.GetString("Time total") + ", " +
				Catalog.GetString("Average speed") + ", " +
				Catalog.GetString("Distance interval") + ", " + 
				Catalog.GetString("Tracks") + ", " + 
				Catalog.GetString("Limited") + ", " +
				Catalog.GetString("Description") );
		
		foreach (string runString in myRunsInterval) {
			string [] myStr = runString.Split(new char[] {':'});
			
			writer.WriteLine ("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}", 
					myStr[0], myStr[1], 	//person.name, run.uniqueID
					myStr[4], myStr[5],	//run.type, run.distancetotal
					myStr[6], 		//run.timetotal,
					Util.GetSpeed(myStr[5], myStr[6]),	//speed AVG
					myStr[7],	 	//run.distanceInterval
					myStr[9], myStr[11],	//tracks, limited
					myStr[10]		//description
					);
			
			//print intervalTimesString
			string [] timeString = myStr[8].Split(new char[] {'='});
			writer.WriteLine( Catalog.GetString ( "Interval speed, interval times" ) );
			foreach(string myTime in timeString) {
				writer.WriteLine("{0}, {1}", Util.GetSpeed(myStr[7], myTime), myTime);
			}
		}
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

	protected override void printHeader()
	{
	}

	protected override void printJumpers()
	{
	}

	protected override void printJumps()
	{
	}

	protected override void printJumpsRj()
	{
	}
	
	protected override void printRuns()
	{
	}

	protected override void printRunsInterval()
	{
	}
	
	protected override void printFooter()
	{
		//xr.Flush();
		//xr.Close();
	}
	
	~ExportSessionXML() {}
}
