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
 * Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
//using Glade;
using GLib; //for Value
using System.Text; //StringBuilder
using Mono.Unix;


//object that will be uploaded to SessionUploadWindow
public class SessionUploadPersonData {
	public Person person;
	public Constants.UploadCodes personCode;
	public int jumpsU;
	public int jumpsE;
	public int jumpsS;
	public int jumpsRjU;
	public int jumpsRjE;
	public int jumpsRjS;
	public int runsU;
	public int runsE;
	public int runsS;
	public int runsIU;
	public int runsIE;
	public int runsIS;
	public int rtsU;
	public int rtsE;
	public int rtsS;
	public int pulsesU;
	public int pulsesE;
	public int pulsesS;
	public int mcsU;
	public int mcsE;
	public int mcsS;
	public string testTypes;
	public string sports;

	public SessionUploadPersonData () {
	}

	public SessionUploadPersonData (Person person, Constants.UploadCodes personCode, 
			int jumpsU, int jumpsE, int jumpsS, 
			int jumpsRjU, int jumpsRjE, int jumpsRjS, 
			int runsU, int runsE, int runsS, 
			int runsIU, int runsIE, int runsIS, 
			int rtsU, int rtsE, int rtsS, 
			int pulsesU, int pulsesE, int pulsesS,
			int mcsU, int mcsE, int mcsS,
			string testTypes, string sports) {
	}
}

public class SessionUploadWindow
{
	Gtk.Window session_upload;
	
	Gtk.TreeView treeview_persons;
	Gtk.TreeView treeview_jumps;
	Gtk.TreeView treeview_jumps_rj;
	Gtk.TreeView treeview_runs;
	Gtk.TreeView treeview_runs_i;
	Gtk.TreeView treeview_rts;
	Gtk.TreeView treeview_pulses;
	Gtk.TreeView treeview_mcs;
	Gtk.Label label_uploaded_test_types;
	Gtk.Label label_uploaded_sports;
	Gtk.Label label_thanks;
	Gtk.Button button_close;
	Gtk.ProgressBar pulsebar;
	Gtk.ProgressBar progressbar;

	private TreeStore store_persons;
	private TreeStore store_j;
	private TreeStore store_jr;
	private TreeStore store_r;
	private TreeStore store_ri;
	private TreeStore store_rts;
	private TreeStore store_pulses;
	private TreeStore store_mcs;


	static SessionUploadWindow SessionUploadWindowBox;
	
	SessionUploadWindow (Gtk.Window parent)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "session_upload.glade", "session_upload", "chronojump");
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "session_upload.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);

		session_upload.Parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(session_upload);
		
		createTreeViews(
				treeview_persons,
				treeview_jumps,
				treeview_jumps_rj,
				treeview_runs,
				treeview_runs_i,
				treeview_rts,
				treeview_pulses,
				treeview_mcs
				);

		store_persons 	= new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string) );
		store_j  	= new TreeStore(typeof (string), typeof (string), typeof (string) );
		store_jr 	= new TreeStore(typeof (string), typeof (string), typeof (string) );
		store_r  	= new TreeStore(typeof (string), typeof (string), typeof (string) );
		store_ri	= new TreeStore(typeof (string), typeof (string), typeof (string) );
		store_rts	= new TreeStore(typeof (string), typeof (string), typeof (string) );
		store_pulses	 = new TreeStore(typeof (string), typeof (string), typeof (string) );
		store_mcs	 = new TreeStore(typeof (string), typeof (string), typeof (string) );

		treeview_persons.Model = 	store_persons;
		treeview_jumps.Model = 		store_j;
		treeview_jumps_rj.Model = 	store_jr;
		treeview_runs.Model = 		store_r;
		treeview_runs_i.Model = 	store_ri;
		treeview_rts.Model = 		store_rts;
		treeview_pulses.Model = 	store_pulses;
		treeview_mcs.Model = 		store_mcs;

		label_thanks.Hide();
		button_close.Sensitive = false;

	}
	
	static public SessionUploadWindow Show (Gtk.Window parent)
	{
		if (SessionUploadWindowBox == null) {
			SessionUploadWindowBox = new SessionUploadWindow (parent);
		}
		SessionUploadWindowBox.session_upload.Show ();
		
		return SessionUploadWindowBox;
	}
	
	private void createTreeViews (Gtk.TreeView treeview_persons, Gtk.TreeView treeview_jumps, Gtk.TreeView treeview_jumps_rj, 
			Gtk.TreeView treeview_runs, Gtk.TreeView treeview_runs_i, Gtk.TreeView treeview_rts, 
			Gtk.TreeView treeview_pulses, Gtk.TreeView treeview_mcs) 
	{
		String [] personCols = {"ID", Catalog.GetString("Name"), "U", "E"};
		String [] testCols = {"U", "E", "S"};
		createTreeView(treeview_persons, personCols, 3);
		createTreeView(treeview_jumps, testCols, 1);
		createTreeView(treeview_jumps_rj, testCols, 1);
		createTreeView(treeview_runs, testCols, 1);
		createTreeView(treeview_runs_i, testCols, 1);
		createTreeView(treeview_rts, testCols, 1);
		createTreeView(treeview_pulses, testCols, 1);
		createTreeView(treeview_mcs, testCols, 1);
	}

	private void createTreeView (Gtk.TreeView tv, String [] cols, int existsPos) {
		int count = 0;
		foreach(string col in cols) {
			//paint in red the non uploaded cols
			if(count >= existsPos) {
				CellRendererText crt1 = new CellRendererText();
				crt1.Foreground = "red";
				//crt1.Background = "blue";
				tv.AppendColumn ( col, crt1, "text", count++);
			} else 
				tv.AppendColumn ( col, new CellRendererText(), "text", count++);
		}
	}

	public void FillData (SessionUploadPersonData p) {
		fillPerson (p.person, p.personCode);
		fillTest (Constants.TestTypes.JUMP, 	p.jumpsU, p.jumpsE, p.jumpsS);
		fillTest (Constants.TestTypes.JUMP_RJ,	p.jumpsRjU, p.jumpsRjE, p.jumpsRjS);
		fillTest (Constants.TestTypes.RUN,	p.runsU, p.runsE, p.runsS);
		fillTest (Constants.TestTypes.RUN_I,	p.runsIU, p.runsIE, p.runsIS);
		fillTest (Constants.TestTypes.RT,	p.rtsU, p.rtsE, p.rtsS);
		fillTest (Constants.TestTypes.PULSE,	p.pulsesU, p.pulsesE, p.pulsesS);
		fillTest (Constants.TestTypes.MULTICHRONOPIC,	p.mcsU, p.mcsE, p.mcsS);

		if(p.testTypes.Length > 0) {
			label_uploaded_test_types.Text = "<b>" + Catalog.GetString("Uploaded test type") + "</b>: " + p.testTypes;
			label_uploaded_test_types.UseMarkup = true; 
		}
		
		if(p.sports.Length > 0) {
			label_uploaded_sports.Text = "<b>" + Catalog.GetString("Uploaded sport") + "</b>: " + p.sports;
			label_uploaded_sports.UseMarkup = true; 
		}
	}

	private void fillPerson (Person person, Constants.UploadCodes uCode) {
		string okCol = "";
		string existCol = "";
		if(uCode == Constants.UploadCodes.OK) 
			okCol = "*";
		else
			existCol = "*";

		store_persons.AppendValues (person.UniqueID.ToString(), person.Name, okCol, existCol);
	}

	private void fillTest (Constants.TestTypes type, int countU, int countE, int countS) {
		string u = countU.ToString();
		string e = countE.ToString();
		string s = countS.ToString();
		if(u=="0")
			u = "";
		if(e=="0")
			e = "";
		if(s=="0")
			s = "";

		switch (type) {
			case Constants.TestTypes.JUMP:
				store_j.AppendValues (u, e, s);
				break;
			case Constants.TestTypes.JUMP_RJ:
				store_jr.AppendValues (u, e, s);
				break;
			case Constants.TestTypes.RUN:
				store_r.AppendValues (u, e, s);
				break;
			case Constants.TestTypes.RUN_I:
				store_ri.AppendValues (u, e, s);
				break;
			case Constants.TestTypes.RT:
				store_rts.AppendValues (u, e, s);
				break;
			case Constants.TestTypes.PULSE:
				store_pulses.AppendValues (u, e, s);
				break;
			case Constants.TestTypes.MULTICHRONOPIC:
				store_mcs.AppendValues (u, e, s);
				break;
		}
	}

	public void PulseProgressbar () {
		progressbar.Pulse();
	}

	public void UpdatePulsebar () {
		pulsebar.Pulse();
	}

	public void UpdateProgressbar (double fraction) {
		if(fraction < 0)
			fraction = 0;
		else if(fraction > 1)
			fraction = 1;

		progressbar.Fraction = fraction;
	}
		
	public void UploadFinished() {
		pulsebar.Fraction =1;
		label_thanks.Show();
		button_close.Sensitive = true;
	}


	void on_button_close_clicked (object o, EventArgs args)
	{
		SessionUploadWindowBox.session_upload.Hide();
		SessionUploadWindowBox = null;
	}
	
	void on_session_upload_delete_event (object o, DeleteEventArgs args)
	{
		SessionUploadWindowBox.session_upload.Hide();
		SessionUploadWindowBox = null;
	}
	
	private void connectWidgets (Gtk.Builder builder)
	{
		treeview_persons = (Gtk.TreeView) builder.GetObject ("treeview_persons");
		treeview_jumps = (Gtk.TreeView) builder.GetObject ("treeview_jumps");
		treeview_jumps_rj = (Gtk.TreeView) builder.GetObject ("treeview_jumps_rj");
		treeview_runs = (Gtk.TreeView) builder.GetObject ("treeview_runs");
		treeview_runs_i = (Gtk.TreeView) builder.GetObject ("treeview_runs_i");
		treeview_rts = (Gtk.TreeView) builder.GetObject ("treeview_rts");
		treeview_pulses = (Gtk.TreeView) builder.GetObject ("treeview_pulses");
		treeview_mcs = (Gtk.TreeView) builder.GetObject ("treeview_mcs");
		label_uploaded_test_types = (Gtk.Label) builder.GetObject ("label_uploaded_test_types");
		label_uploaded_sports = (Gtk.Label) builder.GetObject ("label_uploaded_sports");
		label_thanks = (Gtk.Label) builder.GetObject ("label_thanks");
		button_close = (Gtk.Button) builder.GetObject ("button_close");
		pulsebar = (Gtk.ProgressBar) builder.GetObject ("pulsebar");
		progressbar = (Gtk.ProgressBar) builder.GetObject ("progressbar");
	}
}
