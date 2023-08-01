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
using Gdk;
//using Glade;
using GLib; //for Value
using System.Collections; //ArrayList
using Mono.Unix;


public class ReportWindow
{
	Gtk.Window report_window;
	Gtk.TreeView treeview1;
//	Gtk.Frame frame_general;
//	Gtk.Frame frame_statistics;
	Gtk.VBox vbox_general;
	Gtk.HBox hbox_statistics;

	Gtk.Label label_header;
	Gtk.Label label_general;
	Gtk.Label label_statistics;

	Gtk.CheckButton cb_session_data;
	Gtk.CheckButton cb_jumpers;
	Gtk.CheckButton cb_jumps_simple;
	Gtk.CheckButton cb_jumps_reactive;
	Gtk.CheckButton cb_jumps_reactive_with_subjumps;
	Gtk.CheckButton cb_runs_simple;
	Gtk.CheckButton cb_runs_interval;
	Gtk.CheckButton cb_runs_interval_with_subruns;
	//Gtk.CheckButton cb_reaction_times;
	//Gtk.CheckButton cb_pulses;
	Gtk.Image image_report_win_graph;
	Gtk.Image image_report_win_report;
	Gtk.Image image_report_delete;
	
	Gtk.VButtonBox right_buttons;
	
	GenericWindow genericWin;

	TreeStore store;
	bool selected;

	static ReportWindow ReportWindowBox;

	//private int sessionID;

	Report report;

	
	private ReportWindow () {
	}

	ReportWindow (Gtk.Window parent, Report report )
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "report_window.glade", "report_window", "chronojump");
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "report_window.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);

		report_window.Parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(report_window);

		this.report = report;
	
		//treeview
		createTreeView(treeview1);
		store = new TreeStore( typeof (string), typeof (string), typeof (string), 
				typeof (string), typeof (string), typeof (string), typeof (string), 
				typeof (string), typeof (string)
				);
		treeview1.Model = store;
		
		treeview1.Selection.Changed += onSelectionEntry;
	
		putNonStandardIcons();
	}
	
	

	//if it's created
	static public ReportWindow Show (Gtk.Window parent, Report report)
	{
		if (ReportWindowBox == null) {
			ReportWindowBox = new ReportWindow (parent, report);	
			
			//checkboxes
			ReportWindowBox.loadCheckBoxes();

			ReportWindowBox.FillTreeView();

			ReportWindowBox.report_window.Show ();
		}
		else {
			//update all widget only if it's hidden
			if(! ReportWindowBox.report_window.Visible)
				{
				//checkboxes
				ReportWindowBox.loadCheckBoxes();

				ReportWindowBox.FillTreeView();

				ReportWindowBox.report_window.Show ();
			}
		}

		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.WindowColor(ReportWindowBox.report_window, Config.ColorBackground);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, ReportWindowBox.label_header);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, ReportWindowBox.label_general);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, ReportWindowBox.label_statistics);

			/*
			UtilGtk.WidgetColor (ReportWindowBox.frame_general, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsFrame (Config.ColorBackgroundShiftedIsDark, ReportWindowBox.frame_general);

			UtilGtk.WidgetColor (ReportWindowBox.frame_statistics, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsFrame (Config.ColorBackgroundShiftedIsDark, ReportWindowBox.frame_statistics);
			*/
			
			UtilGtk.WidgetColor (ReportWindowBox.vbox_general, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsVBox (Config.ColorBackgroundShiftedIsDark, ReportWindowBox.vbox_general);
			UtilGtk.WidgetColor (ReportWindowBox.hbox_statistics, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsHBox (Config.ColorBackgroundShiftedIsDark, ReportWindowBox.hbox_statistics);
		}

		return ReportWindowBox;
	}

	private void putNonStandardIcons() {
		Pixbuf pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "gpm-statistics.png");
		image_report_win_graph.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "report_view.png");
		image_report_win_report.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_delete.png");
		image_report_delete.Pixbuf = pixbuf;
	}
	
	private void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;
		tv.AppendColumn ( Catalog.GetString("Type"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Subtype"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Apply to"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Session/s"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Show jumps"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Show sex"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Checked rows"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Graph Options"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Comment"), new CellRendererText(), "text", count++);
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
		//if(report.ShowReactionTimes) { 		cb_reaction_times.Active = true; } 
		//if(report.ShowPulses) { 		cb_pulses.Active = true; } 
	}
	
	public void FillTreeView () 
	{
		//delete rows
		store.Clear();
		
		for (int i=0; i < report.StatisticsData.Count ; i++) {
			string [] myStringFull = report.StatisticsData[i].ToString().Split(new char[] {'\t'});

			store.AppendValues (
					myStringFull[0],	//type
					myStringFull[1],	//subtype
					myStringFull[2],	//applyTo
					myStringFull[3],	//sessionString
					myStringFull[4],	//showJumps
					myStringFull[5],	//showSex
					myStringFull[6],	//markedRows
					myStringFull[7],	//graphROptions
					myStringFull[8]		//comment
					);
		}
			
		right_buttons.Sensitive = false;
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
	public void Add(string type, string subtype, string applyTo, ArrayList sendSelectedSessions, 
			string showJumps, string showSex, ArrayList markedRows, GraphROptions gro)
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
				markedRowsAsAString,
				gro.ToString(),
				""		//comment
				);
		
		//show report window if it's not shown
		report_window.Show ();
	}
	
	private void on_cb_jumps_reactive_clicked (object o, EventArgs args) {
		if(cb_jumps_reactive.Active) {
			cb_jumps_reactive_with_subjumps.Show();
		} else {
			cb_jumps_reactive_with_subjumps.Hide();
		}
	}

	private void on_cb_runs_interval_clicked (object o, EventArgs args) {
		if(cb_runs_interval.Active) {
			cb_runs_interval_with_subruns.Show();
		} else {
			cb_runs_interval_with_subruns.Hide();
		}
	}

	private void onSelectionEntry (object o, EventArgs args)
	{
		//TreeView tv = (TreeView) o;
		ITreeModel model;
		TreeIter iter;
		selected = false;

		// you get the iter and the model if something is selected
		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			selected = true;
			right_buttons.Sensitive = true;
		}
	}
	
	private void on_button_up_clicked (object o, EventArgs args) {
		if(selected)
		{
			ITreeModel model;
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
	
	private void on_button_down_clicked (object o, EventArgs args) {
		if(selected)
		{
			ITreeModel model;
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
	
	private void on_button_graph_clicked (object o, EventArgs args) {
		if(selected)
		{
			ITreeModel model;
			TreeIter iter1; 

			if (treeview1.Selection.GetSelected (out model, out iter1)) {
				string str=getRow(iter1);
				string [] statRow = str.ToString().Split(new char[] {'\t'});
			
				string subType;
				int rj_evolution_mark_consecutives = report.GetRjEvolutionMarkConsecutives(statRow[1], out subType);
			
				ArrayList sendSelectedSessions = report.GetSelectedSessions(statRow[3]);

				Gtk.TreeView treeviewFake = new Gtk.TreeView(); //not needed for graph
						
				bool showSex = Util.StringToBool(statRow[5]);
			
				int limit = -1;
				int statsJumpsType = report.GetStatsJumpTypeAndLimit(statRow[4], out limit);

				ArrayList arrayListMarkedRows = Util.StringToArrayList(statRow[6], ':');

				GraphROptions graphROptions = new GraphROptions(statRow[7]);

				StatType myStatType = new StatType(
						statRow[0], 		//statisticType
						subType,
						statRow[2], 		//statisticApplyTo,
						treeviewFake, 
						sendSelectedSessions, 
						showSex,
						statsJumpsType, 
						limit, 	
						arrayListMarkedRows,
						rj_evolution_mark_consecutives,
						graphROptions,
						true,	//graph
						false,  //always false in this class
						report.preferences
						);
				myStatType.ChooseStat();
			}
		
		}
	}
	
	private void on_button_add_comment_clicked (object o, EventArgs args) {
		//new DialogMessage(Constants.MessageTypes.INFO, "not implemented yet");

		//see if there's any comment
		string comment = "";
		if(selected)
		{
			ITreeModel model;
			TreeIter iter1; 

			if (treeview1.Selection.GetSelected (out model, out iter1)) {
				string str=getRow(iter1);
				string [] statRow = str.ToString().Split(new char[] {'\t'});
				comment = statRow[8];
			}
			
			genericWin = GenericWindow.Show(Catalog.GetString("Add comment"),
					Catalog.GetString("Comment this statistic"),
					Constants.GenericWindowShow.TEXTVIEW, true);
			genericWin.SetTextview(comment);
			genericWin.Button_accept.Clicked += new EventHandler(on_comment_add_accepted);
		}
	}
	
	private void on_comment_add_accepted (object o, EventArgs args) {
		genericWin.Button_accept.Clicked -= new EventHandler(on_comment_add_accepted);
		string comment = genericWin.TextviewSelected;
		
		if(selected)
		{
			ITreeModel model;
			TreeIter iter1; 

			if (treeview1.Selection.GetSelected (out model, out iter1)) {
				store.SetValue (iter1, 8, comment);
			}
		}
	}

	private void on_button_delete_clicked (object o, EventArgs args) {
		if(selected)
		{
			ITreeModel model;
			TreeIter iter1; 

			if (treeview1.Selection.GetSelected (out model, out iter1)) {
				store.Remove(ref iter1);
				right_buttons.Sensitive = false;
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

		/*
		if(cb_reaction_times.Active) { report.ShowReactionTimes = true;  } 
		else { report.ShowReactionTimes = false; }

		if(cb_pulses.Active) { report.ShowPulses = true;  } 
		else { report.ShowPulses = false; }
		*/

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
			
			if (iterOk) 
				arrayToRecord.Add (getRow(myIter));
		}

		report.StatisticsData = arrayToRecord;

	}

	private string getRow(TreeIter myIter) {
		return	(string) treeview1.Model.GetValue (myIter, 0) + "\t" +	//type
			(string) treeview1.Model.GetValue (myIter, 1) + "\t" +	//subtype
			(string) treeview1.Model.GetValue (myIter, 2) + "\t" +	//apply to
			(string) treeview1.Model.GetValue (myIter, 3) + "\t" +	//sessionString
			(string) treeview1.Model.GetValue (myIter, 4) + "\t" +	//showJumps
			(string) treeview1.Model.GetValue (myIter, 5) + "\t" +  //showSex
			(string) treeview1.Model.GetValue (myIter, 6) + "\t" +	//markedRowsString
			(string) treeview1.Model.GetValue (myIter, 7) + "\t" + 	//GraphROptions
			(string) treeview1.Model.GetValue (myIter, 8) 		//Comment
			;
	}
	
	private void on_button_close_clicked (object o, EventArgs args)
	{
		recordData();
		
		ReportWindowBox.report_window.Hide();
		//ReportWindowBox = null;
	}
	
	private void on_delete_event (object o, DeleteEventArgs args)
	{
		recordData();
		
		ReportWindowBox.report_window.Hide();
		ReportWindowBox = null;
	}
	
	private void on_button_make_report_clicked (object o, EventArgs args)
	{
		recordData();
	
		report.PrepareFile();
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		report_window = (Gtk.Window) builder.GetObject ("report_window");
		treeview1 = (Gtk.TreeView) builder.GetObject ("treeview1");
//		frame_general = (Gtk.Frame) builder.GetObject ("frame_general");
//		frame_statistics = (Gtk.Frame) builder.GetObject ("frame_statistics");
		vbox_general = (Gtk.VBox) builder.GetObject ("vbox_general");
		hbox_statistics = (Gtk.HBox) builder.GetObject ("hbox_statistics");

		label_header = (Gtk.Label) builder.GetObject ("label_header");
		label_general = (Gtk.Label) builder.GetObject ("label_general");
		label_statistics = (Gtk.Label) builder.GetObject ("label_statistics");

		cb_session_data = (Gtk.CheckButton) builder.GetObject ("cb_session_data");
		cb_jumpers = (Gtk.CheckButton) builder.GetObject ("cb_jumpers");
		cb_jumps_simple = (Gtk.CheckButton) builder.GetObject ("cb_jumps_simple");
		cb_jumps_reactive = (Gtk.CheckButton) builder.GetObject ("cb_jumps_reactive");
		cb_jumps_reactive_with_subjumps = (Gtk.CheckButton) builder.GetObject ("cb_jumps_reactive_with_subjumps");
		cb_runs_simple = (Gtk.CheckButton) builder.GetObject ("cb_runs_simple");
		cb_runs_interval = (Gtk.CheckButton) builder.GetObject ("cb_runs_interval");
		cb_runs_interval_with_subruns = (Gtk.CheckButton) builder.GetObject ("cb_runs_interval_with_subruns");
		//cb_reaction_times = (Gtk.CheckButton) builder.GetObject ("cb_reaction_times");
		//cb_pulses = (Gtk.CheckButton) builder.GetObject ("cb_pulses");
		image_report_win_graph = (Gtk.Image) builder.GetObject ("image_report_win_graph");
		image_report_win_report = (Gtk.Image) builder.GetObject ("image_report_win_report");
		image_report_delete = (Gtk.Image) builder.GetObject ("image_report_delete");

		right_buttons = (Gtk.VButtonBox) builder.GetObject ("right_buttons");
	}
}

