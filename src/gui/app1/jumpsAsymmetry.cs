/*
 * This file is part of ChronoJump
 *
 * Chronojump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * Chronojump is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 *    GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * Copyright (C) 2022   Xavier de Blas <xaviblas@gmail.com> 
 */


using System;
using System.Collections.Generic; //List
using Gtk;
using Glade;
using Mono.Unix;

public partial class ChronoJumpWindow 
{
	[Widget] Gtk.DrawingArea drawingarea_jumps_asymmetry;
	[Widget] Gtk.Image image_tab_jumps_asymmetry;
	[Widget] Gtk.Image image_jumps_asymmetry_save;
	[Widget] Gtk.Button button_jumps_asymmetry_save_image;

	[Widget] Gtk.RadioButton radio_jumps_asymmetry_bilateral;
	[Widget] Gtk.RadioButton radio_jumps_asymmetry_asymmetry;

	[Widget] Gtk.RadioButton radio_jumps_asymmetry_use_means;

	[Widget] Gtk.HBox hbox_combo_select_jumps_asymmetry_bilateral;
	[Widget] Gtk.ComboBox combo_select_jumps_asymmetry_bilateral;
	[Widget] Gtk.HBox hbox_combo_select_jumps_asymmetry_1;
	[Widget] Gtk.ComboBox combo_select_jumps_asymmetry_1;
	[Widget] Gtk.HBox hbox_combo_select_jumps_asymmetry_2;
	[Widget] Gtk.ComboBox combo_select_jumps_asymmetry_2;
	[Widget] Gtk.Label label_jumps_asymmetry_bilateral;

	//TODO: need max/avg controls

	JumpsAsymmetry jumpsAsymmetry;
	CjComboSelectJumps comboSelectJumpsAsymmetryBilateral;
	CjComboSelectJumps comboSelectJumpsAsymmetry1;
	CjComboSelectJumps comboSelectJumpsAsymmetry2;

	private void createComboSelectJumpsAsymmetry (bool create)
	{
		if(create)
		{
			comboSelectJumpsAsymmetryBilateral = new CjComboSelectJumps (combo_select_jumps_asymmetry_bilateral, hbox_combo_select_jumps_asymmetry_bilateral, false);
			combo_select_jumps_asymmetry_bilateral = comboSelectJumpsAsymmetryBilateral.Combo;
			combo_select_jumps_asymmetry_bilateral.Active = UtilGtk.ComboMakeActive (combo_select_jumps_asymmetry_bilateral, "CMJ");
			combo_select_jumps_asymmetry_bilateral.Changed += new EventHandler (on_combo_select_jumps_asymmetry_bilateral_changed);

			comboSelectJumpsAsymmetry1 = new CjComboSelectJumps (combo_select_jumps_asymmetry_1, hbox_combo_select_jumps_asymmetry_1, false);
			combo_select_jumps_asymmetry_1 = comboSelectJumpsAsymmetry1.Combo;
			combo_select_jumps_asymmetry_1.Active = UtilGtk.ComboMakeActive (combo_select_jumps_asymmetry_1, "slCMJleft");
			combo_select_jumps_asymmetry_1.Changed += new EventHandler (on_combo_select_jumps_asymmetry_1_changed);

			comboSelectJumpsAsymmetry2 = new CjComboSelectJumps (combo_select_jumps_asymmetry_2, hbox_combo_select_jumps_asymmetry_2, false);
			combo_select_jumps_asymmetry_2 = comboSelectJumpsAsymmetry2.Combo;
			combo_select_jumps_asymmetry_2.Active = UtilGtk.ComboMakeActive (combo_select_jumps_asymmetry_2, "slCMJright");
			combo_select_jumps_asymmetry_2.Changed += new EventHandler (on_combo_select_jumps_asymmetry_2_changed);
		} else {
			comboSelectJumpsAsymmetryBilateral.Fill();
			comboSelectJumpsAsymmetry1.Fill();
			comboSelectJumpsAsymmetry2.Fill();

			combo_select_jumps_asymmetry_bilateral = comboSelectJumpsAsymmetryBilateral.Combo;
			combo_select_jumps_asymmetry_1 = comboSelectJumpsAsymmetry1.Combo;
			combo_select_jumps_asymmetry_2 = comboSelectJumpsAsymmetry2.Combo;
		}

		combo_select_jumps_asymmetry_bilateral.Sensitive = true;
		combo_select_jumps_asymmetry_1.Sensitive = true;
		combo_select_jumps_asymmetry_2.Sensitive = true;
	}

	private void on_radio_jumps_asymmetry_bilateral_toggled (object o, EventArgs args)
	{
		combo_select_jumps_asymmetry_bilateral.Visible = true;
		label_jumps_asymmetry_bilateral.Visible = true;

		jumpsAsymmetryDo (true);
	}
	private void on_radio_jumps_asymmetry_asymmetry_toggled (object o, EventArgs args)
	{
		combo_select_jumps_asymmetry_bilateral.Visible = false;
		label_jumps_asymmetry_bilateral.Visible = false;

		jumpsAsymmetryDo (true);
	}

	private void on_radio_jumps_asymmetry_use_means_toggled (object o, EventArgs args)
	{
		jumpsAsymmetryDo (true);
	}
	private void on_radio_jumps_asymmetry_use_maximums_toggled (object o, EventArgs args)
	{
		jumpsAsymmetryDo (true);
	}

	private void on_combo_select_jumps_asymmetry_bilateral_changed (object o, EventArgs args)
	{
		jumpsAsymmetryDo (true);
	}
	private void on_combo_select_jumps_asymmetry_1_changed (object o, EventArgs args)
	{
		jumpsAsymmetryDo (true);
	}
	private void on_combo_select_jumps_asymmetry_2_changed (object o, EventArgs args)
	{
		jumpsAsymmetryDo (true);
	}

	JumpsAsymmetryGraph jumpsAsymmetryGraph;

	private void jumpsAsymmetryDo (bool calculateData)
	{
		button_jumps_asymmetry_save_image.Sensitive = false;

		if (currentPerson == null || currentSession == null ||
				drawingarea_jumps_asymmetry == null || drawingarea_jumps_asymmetry.GdkWindow == null) //it happens at start on click on analyze
		{
			button_jumps_asymmetry_save_image.Sensitive = false;
			return;
		}

		if (jumpsAsymmetry == null) {
			jumpsAsymmetry = new JumpsAsymmetry();
			calculateData = true;
		}

		string jumpBilateral = UtilGtk.ComboGetActive (combo_select_jumps_asymmetry_bilateral);
		string jumpAsymmetry1 = UtilGtk.ComboGetActive (combo_select_jumps_asymmetry_1);
		string jumpAsymmetry2 = UtilGtk.ComboGetActive (combo_select_jumps_asymmetry_2);

		if(calculateData)
			jumpsAsymmetry.Calculate (
					currentPerson.UniqueID, currentSession.UniqueID,
					radio_jumps_asymmetry_bilateral.Active,
					radio_jumps_asymmetry_use_means.Active,
					jumpBilateral,
					jumpAsymmetry1,
					jumpAsymmetry2
					);

		string index = Catalog.GetString ("Bilateral deficit");
		string formula = string.Format ("{0} - ({1} + {2})",
				jumpBilateral,
				jumpAsymmetry1,
				jumpAsymmetry2);

		if (radio_jumps_asymmetry_asymmetry.Active)
		{
			index = Catalog.GetString ("Asymmetry");
			string higherTranslated = Catalog.GetString ("higher");
			string lowerTranslated = Catalog.GetString ("lower");
			formula = Catalog.GetString ("Find daily higher of jumps:") + "\n" +
				string.Format ("{0}, {1}",
						jumpAsymmetry1,
						jumpAsymmetry2) + "\n" +
				string.Format ("100 * ({0} - {1}) / {0}",
						higherTranslated, lowerTranslated);
		}

		bool bars = false;
		if (jumpsAsymmetry.Jad_l.Count == 0)
		{
			if (bars) {
				//constructor for showing blank screen with a message
				CairoBars cb = new CairoBars1Series (drawingarea_jumps_asymmetry, CairoBars.Type.NORMAL, preferences.fontType.ToString(), "Need more data"); //TODO: change message
			} else {

				JumpsAsymmetryGraph.Error error = JumpsAsymmetryGraph.Error.NEEDJUMP;

				if (radio_jumps_asymmetry_bilateral.Active &&
						(jumpBilateral == jumpAsymmetry1 ||
						jumpBilateral == jumpAsymmetry2 ||
						jumpAsymmetry1 == jumpAsymmetry2))
					error = JumpsAsymmetryGraph.Error.REPEATEDJUMPS;
				else if (jumpAsymmetry1 == jumpAsymmetry2)
					error = JumpsAsymmetryGraph.Error.REPEATEDJUMPS;

				new JumpsAsymmetryGraph (drawingarea_jumps_asymmetry, 
						error, index, preferences.fontType.ToString());
			}

			button_jumps_asymmetry_save_image.Sensitive = false;
			return;
		} else {
			if (bars) {
				CairoBars cb = new CairoBars1Series (drawingarea_jumps_asymmetry, CairoBars.Type.NORMAL, false, true, false);
				cb.GraphInit (preferences.fontType.ToString (), false, false);

				List<PointF> point_l = new List<PointF>();
				List<string> names_l = new List<string>();
				int count = 1; //TODO: or 0 or start at max and go backwards?
				foreach (JumpsAsymmetryDay jad in jumpsAsymmetry.Jad_l)
				{
					point_l.Add (new PointF (count ++, jad.GetIndex ()));
					names_l.Add (count.ToString ());
				}

				cb.PassData1Serie (point_l,
						new List<Cairo.Color>(), names_l,
						-1, 14, 10, "hola");
				cb.GraphDo();
			} else {
				//regular constructor
				jumpsAsymmetryGraph = new JumpsAsymmetryGraph (
						jumpsAsymmetry.Point_l,
						jumpsAsymmetry.Dates_l,
						0,
						0,
						drawingarea_jumps_asymmetry,
						currentPerson.Name, index, formula,
						currentSession.DateShort, false, true);
				jumpsAsymmetryGraph.Do (preferences.fontType.ToString());
			}

			button_jumps_asymmetry_save_image.Sensitive = true;
		}
	}


	private void on_drawingarea_jumps_asymmetry_button_press_event (object o, ButtonPressEventArgs args)
	{
		//if there is no data and nothing to show, nothing to press, and also this is null
		// without jumpsAsymmetry.Point_l.Count == 0 condition, it will show graph/data on mouse (with data of previous person/graph)
		if(jumpsAsymmetryGraph == null || jumpsAsymmetry.Point_l.Count == 0)
			return;

		LogB.Information("Button press done!");

		//redo the graph to delete previous rectangles of previous mouse clicks
		jumpsAsymmetryGraph.PassMouseXY (args.Event.X, args.Event.Y);
		jumpsAsymmetryGraph.Do (preferences.fontType.ToString());
	}
	private void on_drawingarea_jumps_asymmetry_expose_event (object o, ExposeEventArgs args)
	{
		jumpsAsymmetryDo (false); //do not calculate data
		//data is calculated on switch page (at notebook_capture_analyze) or on change person
	}

	private void on_button_jumps_asymmetry_save_image_clicked (object o, EventArgs args)
	{
		if (radio_jumps_asymmetry_bilateral.Active)
			checkFile (Constants.CheckFileOp.JUMPS_ASYMMETRY_BILATERAL_SAVE_IMAGE);
		else
			checkFile (Constants.CheckFileOp.JUMPS_ASYMMETRY_ASYMMETRY_SAVE_IMAGE);
	}

	private void on_button_jumps_asymmetry_save_image_selected (string destination)
	{
		if(drawingarea_jumps_asymmetry == null)
			return;

		Gdk.Pixbuf pixbuf = Gdk.Pixbuf.FromDrawable(drawingarea_jumps_asymmetry.GdkWindow, Gdk.Colormap.System,
				0, 0, 0, 0,
				UtilGtk.WidgetWidth(drawingarea_jumps_asymmetry),
				UtilGtk.WidgetHeight(drawingarea_jumps_asymmetry) );

		LogB.Information("Saving");
		pixbuf.Save(destination,"png");
	}
	private void on_overwrite_file_jumps_asymmetry_save_image_accepted (object o, EventArgs args)
	{
		on_button_jumps_asymmetry_save_image_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}

}
