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
 *  Copyright (C) 2016-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections; //ArrayList
//using System.Collections.Generic; //List<T>
using Gdk;
using Glade;
using Gtk;
using Mono.Unix;


public class EncoderOverviewWindow
{
	static EncoderOverviewWindow EncoderOverviewWindowBox;
	
	[Widget] Gtk.Window encoder_overview_win;
	[Widget] Gtk.TreeView treeview_sets;
	[Widget] Gtk.TreeView treeview_reps;
	
	private enum treeviewType { SETS, REPS }


	public EncoderOverviewWindow(Gtk.Window parent, Constants.EncoderGI encoderGI, int sessionID)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "encoder_overview.glade", "encoder_overview_win", null);

		gladeXML.Autoconnect(this);
		encoder_overview_win.Parent = parent;

		if(encoderGI == Constants.EncoderGI.GRAVITATORY)
			encoder_overview_win.Title = Catalog.GetString("Encoder Overview") + " - " + Catalog.GetString("Gravitatory");
		else if(encoderGI == Constants.EncoderGI.INERTIAL)
			encoder_overview_win.Title = Catalog.GetString("Encoder Overview") + " - " + Catalog.GetString("Inertial");

		//put an icon to window
		UtilGtk.IconWindow(encoder_overview_win);

		createAndFillTreeView(treeview_sets, treeviewType.SETS, encoderGI,
				SqliteEncoder.SelectSessionOverviewSets(false, encoderGI, sessionID));
		createAndFillTreeView(treeview_reps, treeviewType.REPS, encoderGI,
				SqliteEncoder.SelectSessionOverviewReps(false, encoderGI, sessionID));

		/*
		createTreeView(treeview_sets, treeviewType.SETS, encoderGI);
		TreeStore storeSets = getStore(treeviewType.SETS, encoderGI);
		treeview_sets.Model = storeSets;
		ArrayList dataSets = SqliteEncoder.SelectSessionOverviewSets(false, encoderGI, sessionID);

		foreach (string [] line in dataSets)
			storeSets.AppendValues (line);

		createTreeView(treeview_reps, treeviewType.REPS, encoderGI);
		TreeStore storeReps = getStore(treeviewType.REPS, encoderGI);
		treeview_reps.Model = storeReps;
		ArrayList dataReps = SqliteEncoder.SelectSessionOverviewReps(false, encoderGI, sessionID);

		foreach (string [] line in dataReps)
			storeReps.AppendValues (line);
			*/
	}
		
	private void createAndFillTreeView(Gtk.TreeView tv, treeviewType tvType, Constants.EncoderGI encoderGI, ArrayList array)
	{
		createTreeView(tv, tvType, encoderGI);
		TreeStore store = getStore(tvType, encoderGI);
		tv.Model = store;

		foreach (string [] line in array)
			store.AppendValues (line);
	}

	static public EncoderOverviewWindow Show (Gtk.Window parent, Constants.EncoderGI encoderGI, int sessionID)
	{
		if (EncoderOverviewWindowBox == null)
			EncoderOverviewWindowBox = new EncoderOverviewWindow (parent, encoderGI, sessionID);

		EncoderOverviewWindowBox.encoder_overview_win.Show ();
		
		return EncoderOverviewWindowBox;
	}


	private void createTreeView(
			Gtk.TreeView tv, treeviewType type, Constants.EncoderGI encoderGI)
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

	private TreeStore getStore(treeviewType type, Constants.EncoderGI encoderGI)
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
		EncoderOverviewWindowBox.encoder_overview_win.Hide();
		EncoderOverviewWindowBox = null;
	}
	
	void on_delete_event (object o, DeleteEventArgs args)
	{
		EncoderOverviewWindowBox.encoder_overview_win.Hide();
		EncoderOverviewWindowBox = null;
	}

}

