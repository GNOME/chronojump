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
 *  Copyright (C) 2016-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections; //ArrayList
using Gdk;
//using Glade;
using Gtk;
using Mono.Unix;


public abstract class OverviewWindow
{
	protected Gtk.Window overview_win;
	protected Gtk.TreeView treeview_sets;
	protected Gtk.TreeView treeview_reps;
	protected Gtk.Notebook notebook;
	protected Gtk.HBox hbox_radio_sets_repetitions;
	protected Gtk.RadioButton radio_sets;
	protected Gtk.RadioButton radio_reps;
	protected Gtk.Button button_select_this_person;

	//used by personIDAtStart, because we need to select the row after showing the window
	protected TreeStore storeSets;
	protected TreeStore storeReps; //note this is not used because right now we cannot have both treeviews selected

	protected enum treeviewType { SETS, REPS }
	protected int sessionID;
	protected int personIDAtStart;
	protected int selectedPersonID;

	protected void initialize()
	{
		setTitle();
		button_select_this_person.Sensitive = false;
		selectedPersonID = -1;
		createTreeViews();
	}

	protected virtual void setTitle()
	{
		overview_win.Title = getTitle();
	}
	protected abstract string getTitle();

	protected virtual void createTreeViews()
	{
		createAndFillTreeView(treeview_sets, treeviewType.SETS, selectData(treeviewType.SETS));
	}
	protected abstract ArrayList selectData(treeviewType type);


	protected void createAndFillTreeView(Gtk.TreeView tv, treeviewType tvType, ArrayList array)
	{
		createTreeView(tv, tvType);
		TreeStore store = getStore(tvType);
		tv.Model = store;

		if(tvType == treeviewType.SETS)
			storeSets = store;
		else if(tvType == treeviewType.REPS)
			storeReps = store;

		foreach (string [] line in array)
			store.AppendValues (line);

		tv.CursorChanged += on_treeview_cursor_changed;
		tv.RowActivated += on_row_double_clicked;
	}

	protected virtual void createTreeView(Gtk.TreeView tv, treeviewType type)
	{
		tv.HeadersVisible=true;
		int count = 0;

		//add invisible personID column
		Gtk.TreeViewColumn personIDCol = new Gtk.TreeViewColumn ("personId", new CellRendererText(), "text", count++);
		personIDCol.Visible = false;
		tv.AppendColumn(personIDCol);

		tv.AppendColumn (Catalog.GetString ("Person"), new CellRendererText(), "text", count++);
		tv.AppendColumn (Catalog.GetString ("Sex"), new CellRendererText(), "text", count++);
		tv.AppendColumn (Catalog.GetString ("Exercise"), new CellRendererText(), "text", count++);
		tv.AppendColumn (Catalog.GetString ("Sets"), new CellRendererText(), "text", count++);
	}

	protected void on_treeview_cursor_changed (object o, EventArgs args)
	{
		LogB.Information("on_treeview_cursor_changed");
		TreeIter iter = new TreeIter();

		if (o == (object) treeview_sets)
		{
			ITreeModel myModel = treeview_sets.Model;

			if (treeview_sets.Selection.GetSelected (out myModel, out iter))
			{
				button_select_this_person.Sensitive = true;
				string selected = ( treeview_sets.Model.GetValue (iter, 0) ).ToString();
				if(Util.IsNumber(selected, false))
					selectedPersonID = Convert.ToInt32(selected);
			}
		} else if (o == (object) treeview_reps)
		{
			ITreeModel myModel = treeview_reps.Model;

			if (treeview_reps.Selection.GetSelected (out myModel, out iter))
			{
				button_select_this_person.Sensitive = true;
				string selected = ( treeview_reps.Model.GetValue (iter, 0) ).ToString();
				if(Util.IsNumber(selected, false))
					selectedPersonID = Convert.ToInt32(selected);
			}
		}
	}

	//before being called, it is called two times: on_treeview_cursor_changed
	protected void on_row_double_clicked (object o, EventArgs args)
	{
		LogB.Information("on_row_double_clicked");

		if(selectedPersonID == -1)
			return;

		button_select_this_person.Click();
	}

	protected void selectRowWithID ()
	{
		if(personIDAtStart >= 0)
			UtilGtk.TreeviewSelectRowWithID(treeview_sets, storeSets, 0, personIDAtStart, true); //last boolean is 'scroll to row'
	}

	protected void on_radio_sets_toggled (object o, EventArgs args)
	{
		if(radio_sets.Active)
			notebook.CurrentPage = 0;

		//unselect to have no confusion on which person is selected by button_select_this_person
		//if there are different selections on both treeviews
		treeview_reps.Selection.UnselectAll();
		treeview_sets.Selection.UnselectAll();
		button_select_this_person.Sensitive = false;
	}

	protected void on_radio_reps_toggled (object o, EventArgs args)
	{
		if(radio_reps.Active)
			notebook.CurrentPage = 1;

		//unselect to have no confusion on which person is selected by button_select_this_person
		//if there are different selections on both treeviews
		treeview_reps.Selection.UnselectAll();
		treeview_sets.Selection.UnselectAll();
		button_select_this_person.Sensitive = false;
	}

	protected virtual TreeStore getStore(treeviewType type)
	{
		return new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string), typeof (string)); //personID (hidden), person name, sex, exercise, sets
	}

	public void HideAndNull ()
	{
		on_button_close_clicked (new object (), new EventArgs ());
	}
	protected virtual void on_button_close_clicked (object o, EventArgs args)
	{
	}

	public Button Button_select_this_person
	{
		set { button_select_this_person = value; }
		get { return button_select_this_person;  }
	}
	public int SelectedPersonID
	{
		get { return selectedPersonID; }
	}
	
	protected void connectWidgets (Gtk.Builder builder)
	{
		overview_win = (Gtk.Window) builder.GetObject ("overview_win");
		treeview_sets = (Gtk.TreeView) builder.GetObject ("treeview_sets");
		treeview_reps = (Gtk.TreeView) builder.GetObject ("treeview_reps");
		notebook = (Gtk.Notebook) builder.GetObject ("notebook");
		hbox_radio_sets_repetitions = (Gtk.HBox) builder.GetObject ("hbox_radio_sets_repetitions");
		radio_sets = (Gtk.RadioButton) builder.GetObject ("radio_sets");
		radio_reps = (Gtk.RadioButton) builder.GetObject ("radio_reps");
		button_select_this_person = (Gtk.Button) builder.GetObject ("button_select_this_person");
	}

}

public class EncoderOverviewWindow : OverviewWindow
{
	static EncoderOverviewWindow EncoderOverviewWindowBox;
	private Constants.EncoderGI encoderGI;

	public EncoderOverviewWindow(Gtk.Window parent)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "overview.glade", "overview_win", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "overview.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);

		overview_win.Parent = parent;

		//put an icon to window
		UtilGtk.IconWindow(overview_win);

		//manage window color
		if(! Config.UseSystemColor)
			UtilGtk.WindowColor (overview_win, Config.ColorBackground);
	}

	//if personIDAtStart == -1, there is not currentPerson
	static public EncoderOverviewWindow Show (Gtk.Window parent, Constants.EncoderGI encoderGI, int sessionID, int personIDAtStart)
	{
		if (EncoderOverviewWindowBox == null)
			EncoderOverviewWindowBox = new EncoderOverviewWindow (parent);

		EncoderOverviewWindowBox.encoderGI = encoderGI;
		EncoderOverviewWindowBox.sessionID = sessionID;
		EncoderOverviewWindowBox.personIDAtStart = personIDAtStart;

		EncoderOverviewWindowBox.initialize();
		EncoderOverviewWindowBox.hbox_radio_sets_repetitions.Visible = true;

		EncoderOverviewWindowBox.overview_win.Show ();

		//done after Show, to ensure the selected row is shown
		EncoderOverviewWindowBox.selectRowWithID();
		
		return EncoderOverviewWindowBox;
	}

	protected override string getTitle()
	{
		string title = Catalog.GetString("Encoder Overview") + " - " + Catalog.GetString("Gravitatory");
		if(encoderGI == Constants.EncoderGI.INERTIAL)
			title = Catalog.GetString("Encoder Overview") + " - " + Catalog.GetString("Inertial");

		return title;
	}

	protected override void createTreeViews()
	{
		createAndFillTreeView(treeview_sets, treeviewType.SETS, selectData(treeviewType.SETS));
		createAndFillTreeView(treeview_reps, treeviewType.REPS, selectData(treeviewType.REPS));
	}

	protected override ArrayList selectData(treeviewType type)
	{
		if(type == treeviewType.SETS)
			return SqliteEncoder.SelectSessionOverviewSets (false, encoderGI, sessionID);
		else
			return SqliteEncoder.SelectSessionOverviewReps (false, encoderGI, sessionID);
	}

	protected override void createTreeView(Gtk.TreeView tv, treeviewType type)
	{
		tv.HeadersVisible=true;
		int count = 0;

		//add invisible personID column
		Gtk.TreeViewColumn personIDCol = new Gtk.TreeViewColumn ("personId", new CellRendererText(), "text", count++);
		personIDCol.Visible = false;
		tv.AppendColumn(personIDCol);

		if(type == treeviewType.SETS)
		{
			tv.AppendColumn (Catalog.GetString ("Person"), new CellRendererText(), "text", count++);
			tv.AppendColumn (Catalog.GetString ("Sex"), new CellRendererText(), "text", count++);
			tv.AppendColumn (Catalog.GetString ("Encoder configuration"), new CellRendererText(), "text", count++);
			tv.AppendColumn (Catalog.GetString ("Exercise"), new CellRendererText(), "text", count++);
			if(encoderGI == Constants.EncoderGI.GRAVITATORY)
				tv.AppendColumn (Catalog.GetString ("Displaced mass"), new CellRendererText(), "text", count++);

			tv.AppendColumn (Catalog.GetString ("Sets"), new CellRendererText(), "text", count++);
		} else {
			tv.AppendColumn (Catalog.GetString ("Person"), new CellRendererText(), "text", count++);
			tv.AppendColumn (Catalog.GetString ("Sex"), new CellRendererText(), "text", count++);
			tv.AppendColumn (Catalog.GetString ("Encoder configuration"), new CellRendererText(), "text", count++);
			tv.AppendColumn (Catalog.GetString ("Exercise"), new CellRendererText(), "text", count++);
			if(encoderGI == Constants.EncoderGI.GRAVITATORY)
				tv.AppendColumn (Catalog.GetString ("Extra mass"), new CellRendererText(), "text", count++);

			tv.AppendColumn (Catalog.GetString ("Contraction"), new CellRendererText(), "text", count++);
			tv.AppendColumn (Catalog.GetString ("Power"), new CellRendererText(), "text", count++);
			tv.AppendColumn (Catalog.GetString ("Speed"), new CellRendererText(), "text", count++);
			tv.AppendColumn (Catalog.GetString ("Force"), new CellRendererText(), "text", count++);
			tv.AppendColumn (Catalog.GetString ("Save criteria on eccentric-concentric"), new CellRendererText(), "text", count++);
		}
	}

	protected override TreeStore getStore(treeviewType type)
	{
		TreeStore s;
		if(type == treeviewType.SETS)
		{
			if(encoderGI == Constants.EncoderGI.GRAVITATORY)
				s = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string), typeof (string), typeof (string), typeof (string));
				//personID (hidden), person name, sex, encoderConfiguration, exercise, displaced mass, sets
			else
				s = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string), typeof (string), typeof (string));
				//personID (hidden), person name, sex, encoderConfiguration, exercise, sets
		} else {
			if(encoderGI == Constants.EncoderGI.GRAVITATORY)
				s = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string), typeof (string), typeof (string), typeof (string), typeof (string), typeof (string), typeof (string), typeof (string));
				//personID (hidden), person name, sex, encoderConfiguration, exercise, extra mass, contraction, power, speed, force, repCriteria
			else
				s = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string), typeof (string), typeof (string), typeof (string), typeof (string), typeof (string), typeof (string));
				//personID (hidden), person name, sex, encoderConfiguration, exercise, contraction, power, speed, force, repCriteria
		}

		return s;
	}

	protected override void on_button_close_clicked (object o, EventArgs args)
	{
		EncoderOverviewWindowBox.overview_win.Hide();
		EncoderOverviewWindowBox = null;
	}
	
	void on_delete_event (object o, DeleteEventArgs args)
	{
		EncoderOverviewWindowBox.overview_win.Hide();
		EncoderOverviewWindowBox = null;
	}
}

public class ForceSensorOverviewWindow : OverviewWindow
{
	static ForceSensorOverviewWindow ForceSensorOverviewWindowBox;
	private Constants.Modes chronojumpMode;

	public ForceSensorOverviewWindow(Gtk.Window parent)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "overview.glade", "overview_win", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "overview.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);

		overview_win.Parent = parent;

		//put an icon to window
		UtilGtk.IconWindow(overview_win);

		//manage window color
		if(! Config.UseSystemColor)
			UtilGtk.WindowColor (overview_win, Config.ColorBackground);
	}

	//if personIDAtStart == -1, there is not currentPerson
	static public ForceSensorOverviewWindow Show (Gtk.Window parent, int sessionID, int personIDAtStart, Constants.Modes chronojumpMode)
	{
		if (ForceSensorOverviewWindowBox == null)
			ForceSensorOverviewWindowBox = new ForceSensorOverviewWindow (parent);

		ForceSensorOverviewWindowBox.sessionID = sessionID;
		ForceSensorOverviewWindowBox.personIDAtStart = personIDAtStart;
		ForceSensorOverviewWindowBox.chronojumpMode = chronojumpMode;

		ForceSensorOverviewWindowBox.initialize();

		ForceSensorOverviewWindowBox.hbox_radio_sets_repetitions.Visible = false;;
		ForceSensorOverviewWindowBox.notebook.GetNthPage(1).Hide();

		ForceSensorOverviewWindowBox.overview_win.Show ();

		//done after Show, to ensure the selected row is shown
		ForceSensorOverviewWindowBox.selectRowWithID();

		return ForceSensorOverviewWindowBox;
	}

	protected override string getTitle()
	{
		return Catalog.GetString("Force sensor overview");
	}

	protected override ArrayList selectData(treeviewType type)
	{
		return SqliteForceSensor.SelectSessionOverviewSets(false, sessionID, chronojumpMode);
	}

	protected override void on_button_close_clicked (object o, EventArgs args)
	{
		ForceSensorOverviewWindowBox.overview_win.Hide();
		ForceSensorOverviewWindowBox = null;
	}

	void on_delete_event (object o, DeleteEventArgs args)
	{
		ForceSensorOverviewWindowBox.overview_win.Hide();
		ForceSensorOverviewWindowBox = null;
	}
}

public class RunEncoderOverviewWindow : OverviewWindow
{
	static RunEncoderOverviewWindow RunEncoderOverviewWindowBox;

	public RunEncoderOverviewWindow(Gtk.Window parent)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "overview.glade", "overview_win", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "overview.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);

		overview_win.Parent = parent;

		//put an icon to window
		UtilGtk.IconWindow(overview_win);

		//manage window color
		if(! Config.UseSystemColor)
			UtilGtk.WindowColor (overview_win, Config.ColorBackground);
	}

	//if personIDAtStart == -1, there is not currentPerson
	static public RunEncoderOverviewWindow Show (Gtk.Window parent, int sessionID, int personIDAtStart)
	{
		if (RunEncoderOverviewWindowBox == null)
			RunEncoderOverviewWindowBox = new RunEncoderOverviewWindow (parent);

		RunEncoderOverviewWindowBox.sessionID = sessionID;
		RunEncoderOverviewWindowBox.personIDAtStart = personIDAtStart;

		RunEncoderOverviewWindowBox.initialize();

		RunEncoderOverviewWindowBox.hbox_radio_sets_repetitions.Visible = false;;
		RunEncoderOverviewWindowBox.notebook.GetNthPage(1).Hide();

		RunEncoderOverviewWindowBox.overview_win.Show ();

		//done after Show, to ensure the selected row is shown
		RunEncoderOverviewWindowBox.selectRowWithID();

		return RunEncoderOverviewWindowBox;
	}

	protected override string getTitle()
	{
		return Catalog.GetString("Race analyzer overview");
	}

	protected override ArrayList selectData(treeviewType type)
	{
		return SqliteRunEncoder.SelectSessionOverviewSets(false, sessionID);
	}

	protected override void on_button_close_clicked (object o, EventArgs args)
	{
		RunEncoderOverviewWindowBox.overview_win.Hide();
		RunEncoderOverviewWindowBox = null;
	}

	void on_delete_event (object o, DeleteEventArgs args)
	{
		RunEncoderOverviewWindowBox.overview_win.Hide();
		RunEncoderOverviewWindowBox = null;
	}
}
