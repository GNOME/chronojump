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
 *  Copyright (C) 2016-2019   Xavier de Blas <xaviblas@gmail.com>
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
	
	protected enum treeviewType { SETS, REPS }
	protected int sessionID;

	protected void initialize()
	{
		setTitle();
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
	}


	protected abstract void createTreeView(Gtk.TreeView tv, treeviewType type);

	protected abstract TreeStore getStore(treeviewType type);
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
	}

	static public EncoderOverviewWindow Show (Gtk.Window parent, Constants.EncoderGI encoderGI, int sessionID)
	{
		if (EncoderOverviewWindowBox == null)
			EncoderOverviewWindowBox = new EncoderOverviewWindow (parent);

		EncoderOverviewWindowBox.encoderGI = encoderGI;
		EncoderOverviewWindowBox.sessionID = sessionID;

		EncoderOverviewWindowBox.initialize();

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

		if(type == treeviewType.SETS)
		{
			tv.AppendColumn (Catalog.GetString ("Person"), new CellRendererText(), "text", count++);
			tv.AppendColumn (Catalog.GetString ("Sex"), new CellRendererText(), "text", count++);
			tv.AppendColumn (Catalog.GetString ("Exercise"), new CellRendererText(), "text", count++);
			if(encoderGI == Constants.EncoderGI.GRAVITATORY)
				tv.AppendColumn (Catalog.GetString ("Displaced mass"), new CellRendererText(), "text", count++);

			tv.AppendColumn (Catalog.GetString ("Sets"), new CellRendererText(), "text", count++);
		} else {
			tv.AppendColumn (Catalog.GetString ("Person"), new CellRendererText(), "text", count++);
			tv.AppendColumn (Catalog.GetString ("Sex"), new CellRendererText(), "text", count++);
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
				s = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string), typeof(string)); //person, sex, exercise, displaced mass, sets
			else
				s = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string)); //person, sex, exercise, sets
		} else {
			if(encoderGI == Constants.EncoderGI.GRAVITATORY)
				s = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string), typeof(string)); //person, sex, exercise, extra mass, power
			else
				s = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string)); //person, sex, exercise, power
		}

		return s;
	}

	void on_button_close_clicked (object o, EventArgs args)
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
