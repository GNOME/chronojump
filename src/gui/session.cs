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
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using Mono.Unix;


public class SessionAddWindow {
	
	[Widget] Gtk.Window session_add_edit;
	[Widget] Gtk.Entry entry_name;
	[Widget] Gtk.Entry entry_place;
	
	[Widget] Gtk.Label label_date;
	[Widget] Gtk.Button button_change_date;
	
	[Widget] Gtk.TextView textview;
	[Widget] Gtk.Button button_accept;
	
	ErrorWindow errorWin;

	DialogCalendar myDialogCalendar;
	DateTime dateTime;
	
	private Session currentSession;
	
	static SessionAddWindow SessionAddWindowBox;
	Gtk.Window parent;
	
	
	SessionAddWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "session_add_edit", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(session_add_edit);
	
		button_accept.Sensitive = false;
		dateTime = DateTime.Today;
		label_date.Text = dateTime.ToLongDateString();
			
		session_add_edit.Title = Catalog.GetString("New Session");
	}
	
	static public SessionAddWindow Show (Gtk.Window parent)
	{
		if (SessionAddWindowBox == null) {
			SessionAddWindowBox = new SessionAddWindow (parent);
		}
		SessionAddWindowBox.session_add_edit.Show ();

		return SessionAddWindowBox;
	}
	
	void on_entries_required_changed (object o, EventArgs args)
	{
		if(entry_name.Text.ToString().Length > 0) {
			button_accept.Sensitive = true;
		}
		else {
			button_accept.Sensitive = false;
		}
	}
		
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		SessionAddWindowBox.session_add_edit.Hide();
		SessionAddWindowBox = null;
	}
	
	void on_session_add_edit_delete_event (object o, DeleteEventArgs args)
	{
		SessionAddWindowBox.session_add_edit.Hide();
		SessionAddWindowBox = null;
	}

	
	void on_button_change_date_clicked (object o, EventArgs args)
	{
		myDialogCalendar = new DialogCalendar(Catalog.GetString("Select session date"));
		myDialogCalendar.FakeButtonDateChanged.Clicked += new EventHandler(on_calendar_changed);
	}

	void on_calendar_changed (object obj, EventArgs args)
	{
		dateTime = myDialogCalendar.MyDateTime;
		label_date.Text = dateTime.ToLongDateString();
	}

	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		string nowDate = dateTime.Day.ToString() + "/" + dateTime.Month.ToString() + "/" +
			dateTime.Year.ToString();
		
		bool sessionExists = Sqlite.Exists (Constants.SessionTable, Util.RemoveTilde(entry_name.Text));
		if(sessionExists) {
			string myString = string.Format(Catalog.GetString("Session: '{0}' exists. Please, use another name"), Util.RemoveTildeAndColonAndDot(entry_name.Text) );
			Console.WriteLine (myString);
			errorWin = ErrorWindow.Show(myString);

		} else {
			currentSession = new Session (entry_name.Text, entry_place.Text, nowDate, textview.Buffer.Text);
			SessionAddWindowBox.session_add_edit.Hide();
			SessionAddWindowBox = null;
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

	public Session CurrentSession 
	{
		get {
			return currentSession;
		}
	}

}

public class SessionEditWindow
{

	[Widget] Gtk.Window session_add_edit; 
	[Widget] Gtk.Entry entry_name;
	[Widget] Gtk.Entry entry_place;
	
	[Widget] Gtk.Label label_date;
	[Widget] Gtk.Button button_change_date;

	[Widget] Gtk.TextView textview;
	[Widget] Gtk.Button button_accept;
	
	ErrorWindow errorWin;

	DialogCalendar myDialogCalendar;
	DateTime dateTime;
	
	private Session currentSession;
	
	static SessionEditWindow SessionEditWindowBox;
	Gtk.Window parent;
	
	
	SessionEditWindow (Gtk.Window parent, Session currentSession) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "session_add_edit", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(session_add_edit);
	
		this.currentSession = currentSession;
		button_accept.Sensitive = false;
		
		session_add_edit.Title = Catalog.GetString("Session Edit");

		dateTime = Util.DateAsDateTime(currentSession.Date);

		entry_name.Text = currentSession.Name;
		entry_place.Text = currentSession.Place;
		
		label_date.Text = currentSession.DateLong;
		
		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = currentSession.Comments;
		textview.Buffer = tb;
	}
	
	static public SessionEditWindow Show (Gtk.Window parent, Session currentSession)
	{
		if (SessionEditWindowBox == null) {
			SessionEditWindowBox = new SessionEditWindow (parent, currentSession);
		}
		SessionEditWindowBox.session_add_edit.Show ();

		return SessionEditWindowBox;
	}

	
	void on_entries_required_changed (object o, EventArgs args)
	{
		if(entry_name.Text.ToString().Length > 0) {
			button_accept.Sensitive = true;
		}
		else {
			button_accept.Sensitive = false;
		}
	}
		
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		SessionEditWindowBox.session_add_edit.Hide();
		SessionEditWindowBox = null;
	}
	
	void on_session_add_edit_delete_event (object o, DeleteEventArgs args)
	{
		SessionEditWindowBox.session_add_edit.Hide();
		SessionEditWindowBox = null;
	}

	
	void on_button_change_date_clicked (object o, EventArgs args)
	{
		myDialogCalendar = new DialogCalendar(Catalog.GetString("Select session date"));
		myDialogCalendar.FakeButtonDateChanged.Clicked += new EventHandler(on_calendar_changed);
	}

	void on_calendar_changed (object obj, EventArgs args)
	{
		dateTime = myDialogCalendar.MyDateTime;
		label_date.Text = dateTime.ToLongDateString();
	}


	void on_button_accept_clicked (object o, EventArgs args)
	{
		//check if new name of session exists (is owned by other session),
		//but all is ok if the name is the same as the old name
		bool sessionExists = Sqlite.Exists (Constants.SessionTable, Util.RemoveTilde(entry_name.Text));
		if(sessionExists && Util.RemoveTilde(entry_name.Text) != currentSession.Name ) {
			string myString = string.Format(Catalog.GetString("Session: '{0}' exists. Please, use another name"), Util.RemoveTildeAndColonAndDot(entry_name.Text) );
			Console.WriteLine (myString);
			errorWin = ErrorWindow.Show(myString);

		} else {
			string nowDate = dateTime.Day.ToString() + "/" + dateTime.Month.ToString() + "/" +
				dateTime.Year.ToString();
			
			currentSession.Name = entry_name.Text.ToString();
			currentSession.Place = entry_place.Text.ToString(); 
			currentSession.Date = nowDate;
			currentSession.Comments = textview.Buffer.Text;
			
			SqliteSession.Edit(currentSession.UniqueID, currentSession.Name, 
					currentSession.Place, currentSession.Date, currentSession.Comments);

			SessionEditWindowBox.session_add_edit.Hide();
			SessionEditWindowBox = null;
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

	public Session CurrentSession 
	{
		get {
			return currentSession;
		}
	}

}


public class SessionLoadWindow {
	
	[Widget] Gtk.Window session_load;
	
	private TreeStore store;
	private string selected;
	[Widget] Gtk.TreeView treeview_session_load;
	[Widget] Gtk.Button button_accept;

	static SessionLoadWindow SessionLoadWindowBox;
	Gtk.Window parent;
	
	private Session currentSession;
	
	SessionLoadWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "session_load", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(session_load);
		
		createTreeView(treeview_session_load);
		store = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string), 
				typeof (string), typeof (string), typeof (string), typeof (string), typeof(string), 
				typeof (string), typeof (string), typeof (string) );
		treeview_session_load.Model = store;
		fillTreeView(treeview_session_load,store);

		button_accept.Sensitive = false;

		treeview_session_load.Selection.Changed += onSelectionEntry;
	}
	
	static public SessionLoadWindow Show (Gtk.Window parent)
	{
		if (SessionLoadWindowBox == null) {
			SessionLoadWindowBox = new SessionLoadWindow (parent);
		}
		SessionLoadWindowBox.session_load.Show ();
		
		return SessionLoadWindowBox;
	}
	
	private void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;
		
		tv.AppendColumn ( Catalog.GetString ("Number"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Place"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Date"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Persons"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Jumps simple"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Jumps reactive"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Runs simple"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Runs interval"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Reaction time"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Pulses"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Comments"), new CellRendererText(), "text", count++);
	}
	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store) 
	{
		string [] mySessions = SqliteSession.SelectAllSessions(); //returns a string of values separated by ':'
		foreach (string session in mySessions) {
			string [] myStringFull = session.Split(new char[] {':'});

			
			store.AppendValues (myStringFull[0], myStringFull[1], 
					myStringFull[2], 
					Util.DateAsDateTime(myStringFull[3]).ToShortDateString(),
					myStringFull[5],	//number of jumpers x session
					myStringFull[6],	//number of jumps x session
					myStringFull[7],	//number of jumpsRj x session
					myStringFull[8], 	//number of runs x session
					myStringFull[9], 	//number of runsInterval x session
					myStringFull[10], 	//number of reaction times x session
					myStringFull[11], 	//number of pulses x session
					myStringFull[4]		//description of session
					);
		}	

	}
	
	private void onSelectionEntry (object o, EventArgs args)
	{
		TreeModel model;
		TreeIter iter;
		selected = "-1";

		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			selected = (string)model.GetValue (iter, 0);
			button_accept.Sensitive = true;
		}
		Console.WriteLine (selected);
	}
	
	void on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;

		if (tv.Selection.GetSelected (out model, out iter)) {
			//put selection in selected
			selected = (string) model.GetValue (iter, 0);

			//activate on_button_accept_clicked()
			button_accept.Activate();
		}
	}
	void on_button_accept_clicked (object o, EventArgs args)
	{
		if(selected != "-1")
		{
			currentSession = SqliteSession.Select (selected);
			SessionLoadWindowBox.session_load.Hide();
			SessionLoadWindowBox = null;
		}
	}
	
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		SessionLoadWindowBox.session_load.Hide();
		SessionLoadWindowBox = null;
	}
	
	void on_session_load_delete_event (object o, DeleteEventArgs args)
	{
		SessionLoadWindowBox.session_load.Hide();
		SessionLoadWindowBox = null;
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
	
	public Session CurrentSession 
	{
		get {
			return currentSession;
		}
	}

}

public class SessionSelectStatsWindow {
	
	[Widget] Gtk.Window stats_select_sessions;
	
	private TreeStore store1;
	private TreeStore store2;
	[Widget] Gtk.TreeView treeview1;
	[Widget] Gtk.TreeView treeview2;
	[Widget] Gtk.Button button_accept;

	static SessionSelectStatsWindow SessionSelectStatsWindowBox;
	Gtk.Window parent;
	
	private ArrayList arrayOfSelectedSessions;
	
	SessionSelectStatsWindow (Gtk.Window parent, ArrayList oldSelectedSessions) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "stats_select_sessions", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
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
		TreeModel model; 
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
		TreeModel model; 
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
				Console.WriteLine(arrayOfSelectedSessions[count]);
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
	
}
