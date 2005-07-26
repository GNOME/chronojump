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
	protected string [] myJumpers;
	protected string [] myJumps;
	protected string [] myJumpsRj;
	protected Session mySession;
	protected TextWriter writer;
	protected static Gtk.Window app1;
	protected static Gnome.AppBar myAppbar;
	protected string fileName;

	public ExportSession() {
	}

	public ExportSession(Session mySession, Gtk.Window app1, Gnome.AppBar mainAppbar) 
	//public ExportSession(Session mySession, Gtk.Window app1) 
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
			myAppbar.Push ( Catalog.GetString ("Cannot export in file: ") + fileName );
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
	
	protected void getData() 
	{
		myJumpers = SqlitePersonSession.SelectCurrentSession(mySession.UniqueID);
		
		bool sortJumpsByType = false;
		if(sortJumpsByType) {
			myJumps= SqliteJump.SelectAllNormalJumps(mySession.UniqueID, "ordered_by_type"); //returns a string of values separated by ':'
		}
		else {
			myJumps= SqliteJump.SelectAllNormalJumps(mySession.UniqueID, "ordered_by_time"); //returns a string of values separated by ':'
		}

		myJumpsRj = SqliteJump.SelectAllRjJumps(mySession.UniqueID, "ordered_by_time");
	}
	
	protected virtual void printData ()
	{
		printHeader();
		printJumpers();
		printJumps();
		printJumpsRj();
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
		writer.WriteLine( Catalog.GetString ( "SessionID, Name, Place, Date, Comments" ) );
		writer.WriteLine( "{0}, {1}, {2}, {3}, {4}", mySession.UniqueID, mySession.Name, 
					mySession.Place, mySession.Date, mySession.Comments );
	}
	
	protected override void printJumpers()
	{
		writer.WriteLine( Catalog.GetString ( "\nJumpers" ) );
		foreach (string jumperString in myJumpers) {
			string [] myStr = jumperString.Split(new char[] {':'});
			
			writer.WriteLine ("{0}, {1}", 
					myStr[0], myStr[1] 	//person.id, person.name 
					);
		}
	}

	protected override void printJumps()
	{
		writer.WriteLine( Catalog.GetString ( "\nNormal Jumps" ) );
		writer.WriteLine( Catalog.GetString ( "\nJumper name, jump ID, Type, Tv, Tc, Fall, Weight, Description" ) );
		
		foreach (string jumpString in myJumps) {
			string [] myStr = jumpString.Split(new char[] {':'});
			
			writer.WriteLine ("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}", 
					myStr[0], myStr[1], 	//person.name, jump.uniqueID
					//myStr[2], myStr[3], 	//jump.personID, jump.sessionID
					myStr[4], myStr[5],	//jump.type, jump.tv
					myStr[6], myStr[7],	//jump.tc, jump.fall
					myStr[8], myStr[9]	//jump.weight, jump.description
					);
		}
	}

	protected override void printJumpsRj()
	{
		writer.WriteLine( Catalog.GetString ( "\nReactive Jumps" ) );
		
		foreach (string jump in myJumpsRj) {
			string [] myStr = jump.Split(new char[] {':'});

			writer.WriteLine( Catalog.GetString ( "\nJumper name, jump ID, jump Type, TV Max, TC Max, TV Avg, TC AVG, Fall, Weight, Jumps, Time, Limited, Description" ) );
			writer.WriteLine ("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}", 
					myStr[0], myStr[1], 	//person.name, jumpRj.uniqueID
					//myStr[2], myStr[3], 	//jumpRj.personID, jumpRj.sessionID
					myStr[4], myStr[5],	//jumpRj.type, jumpRj.tvMax
					myStr[6], myStr[10],	//jumpRj.tcMax, jumpRj.tvAvg
					myStr[11], myStr[7],	//jumpRj.tvAvg, jumpRj.Fall
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
	
	protected override void printFooter()
	{
		//xr.Flush();
		//xr.Close();
	}
	
	~ExportSessionXML() {}
}
