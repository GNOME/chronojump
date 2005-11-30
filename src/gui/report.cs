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
using Gtk;
using Glade;
using Gnome;
using GLib; //for Value
using System.Collections; //ArrayList


public class ReportWindow {
	
	
	TreeStore store;
	bool selected;

	[Widget] Gtk.Window report_window;
	[Widget] Gtk.TreeView treeview1;
	
	[Widget] Gtk.CheckButton cb_session_data;
	[Widget] Gtk.CheckButton cb_jumpers;
	[Widget] Gtk.CheckButton cb_jumps_simple;
	[Widget] Gtk.CheckButton cb_jumps_reactive;
	[Widget] Gtk.CheckButton cb_jumps_reactive_with_subjumps;
	[Widget] Gtk.CheckButton cb_runs_simple;
	[Widget] Gtk.CheckButton cb_runs_interval;
	[Widget] Gtk.CheckButton cb_runs_interval_with_subruns;
	
	static ReportWindow ReportWindowBox;
	
	protected Gtk.Window parent;

	//protected int sessionID;

	Report report;

	
	protected ReportWindow () {
	}

	ReportWindow (Gtk.Window parent, Report report ) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "report_window", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;

		this.report = report;
	
		//treeview
		createTreeView(treeview1);
		store = new TreeStore( typeof (string), typeof (string), typeof (string), 
				typeof (string), typeof (string), typeof (string), typeof (string)
				);
		treeview1.Model = store;
	}

	//if it's created
	static public ReportWindow Show (Gtk.Window parent, Report report)
	{
		if (ReportWindowBox == null) {
			ReportWindowBox = new ReportWindow (parent, report);
			
			//checkboxes
			ReportWindowBox.loadCheckBoxes();

			ReportWindowBox.fillTreeView();

			ReportWindowBox.report_window.Show ();
		}
		else {
			//update all widget only if it's hidden
			if(! ReportWindowBox.report_window.Visible)
				{
				//checkboxes
				ReportWindowBox.loadCheckBoxes();

				ReportWindowBox.fillTreeView();

				ReportWindowBox.report_window.Show ();
			}
		}
		
		return ReportWindowBox;
	}
	
	protected void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;
		tv.AppendColumn ( Catalog.GetString("Type"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Subtype"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Apply to"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Session/s"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Show jumps"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Show sex"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Checked rows"), new CellRendererText(), "text", count++);
	}

	void loadCheckBoxes () 
	{
		if(report.ShowCurrentSessionData) { 	cb_session_data.Active = true; } 
		if(report.ShowCurrentSessionJumpers) {	cb_jumpers.Active = true; } 
		if(report.ShowSimpleJumps) { 		cb_jumps_simple.Active = true; } 
		if(report.ShowReactiveJumps) { 	cb_jumps_reactive.Active = true; } 
		if(report.ShowReactiveJumpsWithSubjumps) { 	cb_jumps_reactive_with_subjumps.Active = true; } 
		if(report.ShowSimpleRuns) { 		cb_runs_simple.Active = true; } 
		if(report.ShowIntervalRuns) { 	cb_runs_interval.Active = true; } 
		if(report.ShowIntervalRunsWithSubruns) { 	cb_runs_interval_with_subruns.Active = true; } 
	}
	
	void fillTreeView () 
	{
		//delete rows
		store.Clear();
		
		for (int i=0; i < report.StatisticsData.Count ; i++) {
			//string [] myStringFull = report.StatisticsData[i].ToString().Split(new char[] {'\n'});
			string [] myStringFull = report.StatisticsData[i].ToString().Split(new char[] {'\t'});

			store.AppendValues (
					myStringFull[0],	//type
					myStringFull[1],	//subtype
					myStringFull[2],	//applyTo
					myStringFull[3],	//sessionString
					myStringFull[4],	//showJumps
					myStringFull[5],	//showSex
					myStringFull[6]		//markedRows
					);
		}

	}

	string arrayToString(ArrayList myArrayList) {
		string myString = "";
		for (int i=0; i < myArrayList.Count ; i++) {
			if(i>0) {
				myString += ":";
			}
			string [] myStrFull = myArrayList[i].ToString().Split(new char[] {':'});
			myString += myStrFull[0];
		}
		return myString;
	}
	
	//comes from stats window
	//public void Add(string type, string subtype, string applyTo, string sessionString, string showJumps, string showSex)
	public void Add(string type, string subtype, string applyTo, ArrayList sendSelectedSessions, 
			string showJumps, string showSex, ArrayList markedRows)
	{
		string sessionsAsAString = arrayToString(sendSelectedSessions);
		string markedRowsAsAString = arrayToString(markedRows);
		
		store.AppendValues (
				type, 
				subtype, 
				applyTo, 
				sessionsAsAString, 
				showJumps, 
				showSex,
				markedRowsAsAString 
				);
		
		//show report window if it's not shown
		report_window.Show ();
	}
	
	protected virtual void on_cb_jumps_reactive_clicked (object o, EventArgs args) {
		if(cb_jumps_reactive.Active) {
			cb_jumps_reactive_with_subjumps.Show();
		} else {
			cb_jumps_reactive_with_subjumps.Hide();
		}
	}

	protected virtual void on_cb_runs_interval_clicked (object o, EventArgs args) {
		if(cb_runs_interval.Active) {
			cb_runs_interval_with_subruns.Show();
		} else {
			cb_runs_interval_with_subruns.Hide();
		}
	}

	protected virtual void on_treeview_cursor_changed (object o, EventArgs args)
	{
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;
		selected = false;

		// you get the iter and the model if something is selected
		if (tv.Selection.GetSelected (out model, out iter)) {
			selected = true;
		}
	}
	
	protected virtual void on_button_up_clicked (object o, EventArgs args) {
		if(selected)
		{
			TreeModel model;
			TreeIter iter_pre; //iter_old
			TreeIter iter_post; //iter new_ordered
			TreePath path;

			if (treeview1.Selection.GetSelected (out model, out iter_pre)) {
				path=store.GetPath(iter_pre);
				path.Prev();
				bool notFirst = store.GetIter(out iter_post, path);
				
				if(notFirst) {
					store.MoveBefore (iter_pre, iter_post);
				}
			}
		}
	}
	
	protected virtual void on_button_down_clicked (object o, EventArgs args) {
		if(selected)
		{
			TreeModel model;
			TreeIter iter_pre; //iter_old
			TreeIter iter_post; //iter new_ordered
			TreePath path;

			if (treeview1.Selection.GetSelected (out model, out iter_pre)) {
				path=store.GetPath(iter_pre);
				path.Next();
				bool notLast = store.GetIter(out iter_post, path);
				
				if(notLast) {
					store.MoveAfter (iter_pre, iter_post);
				}
			}
		}
	}
	
	protected virtual void on_button_delete_clicked (object o, EventArgs args) {
		if(selected)
		{
			TreeModel model;
			TreeIter iter1; 

			if (treeview1.Selection.GetSelected (out model, out iter1)) {
				store.Remove(ref iter1);
			}
		}
	}

	private void recordData()
	{
		//myReport.SessionID = sessionID;
		
		//checkboxes
		if(cb_session_data.Active) { report.ShowCurrentSessionData = true;  } 
		else { report.ShowCurrentSessionData = false; }

		if(cb_jumpers.Active) { report.ShowCurrentSessionJumpers = true;  } 
		else { report.ShowCurrentSessionJumpers = false; }

		if(cb_jumps_simple.Active) { report.ShowSimpleJumps = true;  } 
		else { report.ShowSimpleJumps = false; }

		if(cb_jumps_reactive.Active) { report.ShowReactiveJumps = true;  } 
		else { report.ShowReactiveJumps = false; }

		if(cb_jumps_reactive_with_subjumps.Active) { report.ShowReactiveJumpsWithSubjumps = true;  } 
		else { report.ShowReactiveJumpsWithSubjumps = false; }

		if(cb_runs_simple.Active) { report.ShowSimpleRuns = true;  } 
		else { report.ShowSimpleRuns = false; }

		if(cb_runs_interval.Active) { report.ShowIntervalRuns = true;  } 
		else { report.ShowIntervalRuns = false; }

		if(cb_runs_interval_with_subruns.Active) { report.ShowIntervalRunsWithSubruns = true;  } 
		else { report.ShowIntervalRunsWithSubruns = false; }

		//treeview
		TreeIter myIter = new TreeIter ();
		bool iterOk = true;
	
		ArrayList arrayToRecord = new ArrayList (1);

		for (int count=0 ; iterOk; count ++) {
			if (count == 0) {
				iterOk = store.GetIterFirst (out myIter);
			}
			else {
				iterOk = store.IterNext (ref myIter); 
			}
			
			if (iterOk) {
				arrayToRecord.Add ( 
					(string) treeview1.Model.GetValue (myIter, 0) + "\t" +	//type
					(string) treeview1.Model.GetValue (myIter, 1) + "\t" +	//subtype
					(string) treeview1.Model.GetValue (myIter, 2) + "\t" +	//apply to
					(string) treeview1.Model.GetValue (myIter, 3) + "\t" +	//sessionString
					(string) treeview1.Model.GetValue (myIter, 4) + "\t" +	//showJumps
					(string) treeview1.Model.GetValue (myIter, 5) + "\t" +  //showSex
					(string) treeview1.Model.GetValue (myIter, 6) 		//markedRowsString
					);
			}
		}

		report.StatisticsData = arrayToRecord;

	}
	
	protected virtual void on_button_close_clicked (object o, EventArgs args)
	{
		recordData();
		
		ReportWindowBox.report_window.Hide();
		//ReportWindowBox = null;
	}
	
	protected virtual void on_delete_event (object o, EventArgs args)
	{
		recordData();
		
		ReportWindowBox.report_window.Hide();
		//ReportWindowBox = null;
	}
	
	protected virtual void on_button_make_report_clicked (object o, EventArgs args)
	{
		recordData();
	
		report.PrepareFile();
	}
}

