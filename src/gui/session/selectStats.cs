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
using System.Collections; //ArrayList
using Mono.Unix;

public class SessionSelectStatsWindow
{
	Gtk.Window stats_select_sessions;
	Gtk.TreeView treeview1;
	Gtk.TreeView treeview2;
	Gtk.Button button_accept;

	private TreeStore store1;
	private TreeStore store2;
	static SessionSelectStatsWindow SessionSelectStatsWindowBox;
	
	private ArrayList arrayOfSelectedSessions;
	
	SessionSelectStatsWindow (Gtk.Window parent, ArrayList oldSelectedSessions)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "stats_select_sessions.glade", "stats_select_sessions", "chronojump");
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "stats_select_sessions.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);

		stats_select_sessions.Parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(stats_select_sessions);
	
		createTreeView(treeview1);
		store1 = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string), typeof (string) );
		treeview1.Model = store1;
		fillTreeView(treeview1,store1);
		
		createTreeView(treeview2);
		store2 = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string), typeof (string) );
		treeview2.Model = store2;
		
		processOldSelectedSessions(treeview1, store1, store2, oldSelectedSessions);
	}
	
	static public SessionSelectStatsWindow Show (Gtk.Window parent, ArrayList oldSelectedSessions)
	{
		if (SessionSelectStatsWindowBox == null) {
			SessionSelectStatsWindowBox = new SessionSelectStatsWindow (parent, oldSelectedSessions);
		}
		SessionSelectStatsWindowBox.stats_select_sessions.Show ();
		return SessionSelectStatsWindowBox;
	}
	
	private void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;
		
		tv.AppendColumn ( Catalog.GetString ("Number"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Place"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Date"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Comments"), new CellRendererText(), "text", count++);
	}
	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store) 
	{
		bool commentsDisable = false;
		int sessionIdDisable = -1; //don't disable any session (-1 as uniqueID is impossible)
		string [] mySessions = 
			SqliteSession.SelectAllSessionsSimple(commentsDisable, sessionIdDisable);

		foreach (string session in mySessions) {
			string [] myStringFull = session.Split(new char[] {':'});

			store.AppendValues (myStringFull[0], myStringFull[1], 
					myStringFull[2], myStringFull[3], 
					myStringFull[4]		//description of session
					);
		}	

	}
	
	//oldSelectedSessions is an ArrayList with three cols (values of the old selectedSessions)
	//now, find iters corresponding to each of this sessions and put in the selected treeview, and delete from the unselected treeview
	private void processOldSelectedSessions (Gtk.TreeView treeview1, TreeStore store1, TreeStore store2, ArrayList oldSelectedSessions) {
		TreeIter iter1 = new TreeIter();
		string [] strIter = {"", "", "", "", ""};
		
		for (int i=0; i < oldSelectedSessions.Count ; i ++) {
			string [] str = oldSelectedSessions[i].ToString().Split(new char[] {':'});
			findRowForIter(treeview1, store1, out iter1, Convert.ToInt32(str[0]));

			for (int j=0; j < 5; j ++) {
				strIter [j] = (string) treeview1.Model.GetValue (iter1, j);
			}

			//print values
			store2.AppendValues (strIter[0], strIter[1], strIter[2], strIter[3], strIter[4]);

			//delete iter1
			store1.Remove(ref iter1);
		}
	}
	
	void on_button_select_clicked (object o, EventArgs args)
	{
		ITreeModel model; 
		TreeIter iter1; //iter of the first treeview
		TreeIter iter2; //iter of second treeview. Used for search the row on we are going to insert
		TreeIter iter3; //new iter in first treeview
		int i;
		string [] str = {"", "", "", "", ""};

		if (treeview1.Selection.GetSelected (out model, out iter1)) {
			for (i=0; i < 5; i ++) {
				str[i] = (string) model.GetValue (iter1, i);
			}

			//create iter3
			iter3 = store2.AppendValues (str[0], str[1], str[2], str[3], str[4]);

			//move it where it has to be
			findRowForIter(treeview2, store2, out iter2, Convert.ToInt32(str[0]));
			store2.MoveBefore (iter3, iter2);
		
			//delete iter1
			store1.Remove(ref iter1);
		}
	}
		
	void on_button_unselect_clicked (object o, EventArgs args)
	{
		ITreeModel model; 
		TreeIter iter1; //iter of first treeview. Used for search the row on we are going to insert
		TreeIter iter2; //iter of the second treeview
		TreeIter iter3; //new iter in first treeview
		int i;
		string [] str = {"", "", "", "", ""};

		if (treeview2.Selection.GetSelected (out model, out iter2)) {
			for (i=0; i < 5; i ++) {
				str[i] = (string) model.GetValue (iter2, i);
			}

			//create iter3
			iter3 = store1.AppendValues (str[0], str[1], str[2], str[3], str[4]);

			//move it where it has to be
			findRowForIter(treeview1, store1, out iter1, Convert.ToInt32(str[0]));
			store1.MoveBefore (iter3, iter1);
		
			//delete iter2
			store2.Remove(ref iter2);
		}
	}

	void findRowForIter (TreeView myTreeview, TreeStore myStore, out TreeIter myIter, int searchedPosition) 
	{
		int position;
		bool firstLap = true;

		myStore.GetIterFirst (out myIter);
		position = Convert.ToInt32( (string) myTreeview.Model.GetValue (myIter, 0) );

		do {
			if ( ! firstLap) {
				myStore.IterNext (ref myIter);
			}
			position = Convert.ToInt32( (string) myTreeview.Model.GetValue (myIter, 0) );
			firstLap = false;
		} while (position < searchedPosition );
	}
		
	void on_button_all_clicked (object o, EventArgs args)
	{
		//delete existing rows in treeview1
		store1.Clear();
		//also in treeview2 (for not having repeated rows)
		store2.Clear();
		
		//put all the values it treeview2 (from the sql)
		fillTreeView(treeview2,store2);
	}
		
	void on_button_none_clicked (object o, EventArgs args)
	{
		//delete existing rows in treeview2
		store2.Clear();
		//also in treeview1 (for not having repeated rows)
		store1.Clear();
		
		//put all the values it treeview1 (from the sql)
		fillTreeView(treeview1,store1);
	}
		
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		SessionSelectStatsWindowBox.stats_select_sessions.Hide();
		SessionSelectStatsWindowBox = null;
	}
	
	void on_delete_event (object o, DeleteEventArgs args)
	{
		SessionSelectStatsWindowBox.stats_select_sessions.Hide();
		SessionSelectStatsWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		prepareSelected (treeview2, store2);
		SessionSelectStatsWindowBox.stats_select_sessions.Hide();
		SessionSelectStatsWindowBox = null;
	}
	
	void prepareSelected (TreeView myTreeview, TreeStore myStore) 
	{
		TreeIter myIter = new TreeIter ();
		bool iterOk = true;
	
		arrayOfSelectedSessions = new ArrayList (2);

		for (int count=0 ; iterOk; count ++) {
			if (count == 0) {
				iterOk = myStore.GetIterFirst (out myIter);
			}
			else {
				iterOk = myStore.IterNext (ref myIter); 
			}
			
			if (iterOk) {
				arrayOfSelectedSessions.Add ( 
					(string) myTreeview.Model.GetValue (myIter, 0) + ":" +	//id
					(string) myTreeview.Model.GetValue (myIter, 1) + ":" +	//name
					(string) myTreeview.Model.GetValue (myIter, 3) 		//date (forget place)
					);
				LogB.Information(arrayOfSelectedSessions[count].ToString());
			}
		} 
	}
		
	public Button Button_accept 
	{
		set {
			button_accept = value;	
		}
		get {
			return button_accept;
		}
	}
	
	public ArrayList ArrayOfSelectedSessions 
	{
		get {
			if (arrayOfSelectedSessions.Count > 0) {
				return arrayOfSelectedSessions;
			} else {
				arrayOfSelectedSessions.Add("-1");
				return arrayOfSelectedSessions;
			}
		}
	}
	
	private void connectWidgets (Gtk.Builder builder)
	{
		stats_select_sessions = (Gtk.Window) builder.GetObject ("stats_select_sessions");
		treeview1 = (Gtk.TreeView) builder.GetObject ("treeview1");
		treeview2 = (Gtk.TreeView) builder.GetObject ("treeview2");
		button_accept = (Gtk.Button) builder.GetObject ("button_accept");
	}
}
