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
 *  Copyright (C) 2016-2020   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections; //ArrayList
using Gdk;
using Glade;
using Gtk;
using Mono.Unix;


public abstract class OverviewWindow
{
	[Widget] protected Gtk.Window overview_win;
	[Widget] protected Gtk.TreeView treeview_sets;
	[Widget] protected Gtk.TreeView treeview_reps;
	[Widget] protected Gtk.Notebook notebook;
	[Widget] protected Gtk.HBox hbox_radio_sets_repetitions;
	[Widget] protected Gtk.RadioButton radio_sets;
	[Widget] protected Gtk.RadioButton radio_reps;
	[Widget] protected Gtk.Button button_select_this_person;
	
	protected enum treeviewType { SETS, REPS }
	protected int sessionID;
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

		foreach (string [] line in array)
			store.AppendValues (line);

		tv.CursorChanged += on_treeview_cursor_changed;
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
		TreeIter iter = new TreeIter();

		if (o == (object) treeview_sets)
		{
			TreeModel myModel = treeview_sets.Model;

			if (treeview_sets.Selection.GetSelected (out myModel, out iter))
			{
				button_select_this_person.Sensitive = true;
				string selected = ( treeview_sets.Model.GetValue (iter, 0) ).ToString();
				if(Util.IsNumber(selected, false))
					selectedPersonID = Convert.ToInt32(selected);
			}
		} else if (o == (object) treeview_reps)
		{
			TreeModel myModel = treeview_reps.Model;

			if (treeview_reps.Selection.GetSelected (out myModel, out iter))
			{
				button_select_this_person.Sensitive = true;
				string selected = ( treeview_reps.Model.GetValue (iter, 0) ).ToString();
				if(Util.IsNumber(selected, false))
					selectedPersonID = Convert.ToInt32(selected);
			}
		}
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

}

public class EncoderOverviewWindow : OverviewWindow
{
	static EncoderOverviewWindow EncoderOverviewWindowBox;
	private Constants.EncoderGI encoderGI;

	public EncoderOverviewWindow(Gtk.Window parent)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "overview.glade", "overview_win", null);
		gladeXML.Autoconnect(this);

		overview_win.Parent = parent;

		//put an icon to window
		UtilGtk.IconWindow(overview_win);

		//manage window color
		if(! Config.UseSystemColor)
			UtilGtk.WindowColor(overview_win, Config.ColorBackground);
	}

	static public EncoderOverviewWindow Show (Gtk.Window parent, Constants.EncoderGI encoderGI, int sessionID)
	{
		if (EncoderOverviewWindowBox == null)
			EncoderOverviewWindowBox = new EncoderOverviewWindow (parent);

		EncoderOverviewWindowBox.encoderGI = encoderGI;
		EncoderOverviewWindowBox.sessionID = sessionID;

		EncoderOverviewWindowBox.initialize();
		EncoderOverviewWindowBox.hbox_radio_sets_repetitions.Visible = true;

		EncoderOverviewWindowBox.overview_win.Show ();
		
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
			return SqliteEncoder.SelectSessionOverviewSets(false, encoderGI, sessionID);
		else
			return SqliteEncoder.SelectSessionOverviewReps(false, encoderGI, sessionID);
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

			tv.AppendColumn (Catalog.GetString ("Power"), new CellRendererText(), "text", count++);
		}
	}

	protected override TreeStore getStore(treeviewType type)
	{
		TreeStore s;
		if(type == treeviewType.SETS)
		{
			if(encoderGI == Constants.EncoderGI.GRAVITATORY)
				s = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string), typeof (string), typeof (string), typeof(string)); //personID (hidden), person name, sex, encoderConfiguration, exercise, displaced mass, sets
			else
				s = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string), typeof (string), typeof (string)); //personID (hidden), person name, sex, encoderConfiguration, exercise, sets
		} else {
			if(encoderGI == Constants.EncoderGI.GRAVITATORY)
				s = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string), typeof (string), typeof (string), typeof(string)); //personID (hidden), person name, sex, encoderConfiguration, exercise, extra mass, power
			else
				s = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string), typeof (string), typeof (string)); //personID (hidden), person name, sex, encoderConfiguration, exercise, power
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

	public ForceSensorOverviewWindow(Gtk.Window parent)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "overview.glade", "overview_win", null);
		gladeXML.Autoconnect(this);

		overview_win.Parent = parent;

		//put an icon to window
		UtilGtk.IconWindow(overview_win);

		//manage window color
		if(! Config.UseSystemColor)
			UtilGtk.WindowColor(overview_win, Config.ColorBackground);
	}

	static public ForceSensorOverviewWindow Show (Gtk.Window parent, int sessionID)
	{
		if (ForceSensorOverviewWindowBox == null)
			ForceSensorOverviewWindowBox = new ForceSensorOverviewWindow (parent);

		ForceSensorOverviewWindowBox.sessionID = sessionID;

		ForceSensorOverviewWindowBox.initialize();

		ForceSensorOverviewWindowBox.hbox_radio_sets_repetitions.Visible = false;;
		ForceSensorOverviewWindowBox.notebook.GetNthPage(1).Hide();

		ForceSensorOverviewWindowBox.overview_win.Show ();

		return ForceSensorOverviewWindowBox;
	}

	protected override string getTitle()
	{
		return Catalog.GetString("Force sensor overview");
	}

	protected override ArrayList selectData(treeviewType type)
	{
		return SqliteForceSensor.SelectSessionOverviewSets(false, sessionID);
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
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "overview.glade", "overview_win", null);
		gladeXML.Autoconnect(this);

		overview_win.Parent = parent;

		//put an icon to window
		UtilGtk.IconWindow(overview_win);

		//manage window color
		if(! Config.UseSystemColor)
			UtilGtk.WindowColor(overview_win, Config.ColorBackground);
	}

	static public RunEncoderOverviewWindow Show (Gtk.Window parent, int sessionID)
	{
		if (RunEncoderOverviewWindowBox == null)
			RunEncoderOverviewWindowBox = new RunEncoderOverviewWindow (parent);

		RunEncoderOverviewWindowBox.sessionID = sessionID;

		RunEncoderOverviewWindowBox.initialize();

		RunEncoderOverviewWindowBox.hbox_radio_sets_repetitions.Visible = false;;
		RunEncoderOverviewWindowBox.notebook.GetNthPage(1).Hide();

		RunEncoderOverviewWindowBox.overview_win.Show ();

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
