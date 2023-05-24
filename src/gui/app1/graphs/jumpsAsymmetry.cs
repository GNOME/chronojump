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
 * Copyright (C) 2023   Xavier de Blas <xaviblas@gmail.com> 
 */


using System;
using System.Collections.Generic; //List
using Gtk;
//using Glade;
using Mono.Unix;

public partial class ChronoJumpWindow 
{
	// at glade ---->
	Gtk.DrawingArea drawingarea_jumps_asymmetry;
	Gtk.Image image_tab_jumps_asymmetry;
	Gtk.Image image_jumps_asymmetry_save;
	Gtk.Button button_jumps_asymmetry_save_image;

	Gtk.RadioButton radio_jumps_asymmetry_bilateral;
	Gtk.RadioButton radio_jumps_asymmetry_asymmetry;

	Gtk.RadioButton radio_jumps_asymmetry_use_means;

	Gtk.HBox hbox_combo_select_jumps_asymmetry_bilateral;
	Gtk.HBox hbox_combo_select_jumps_asymmetry_1;
	Gtk.HBox hbox_combo_select_jumps_asymmetry_2;
	Gtk.Label label_jumps_asymmetry_bilateral;
	// <---- at glade

	//TODO: need max/avg controls

	JumpsAsymmetry jumpsAsymmetry;
	CjComboSelectJumps comboSelectJumpsAsymmetryBilateral;
	CjComboSelectJumps comboSelectJumpsAsymmetry1;
	CjComboSelectJumps comboSelectJumpsAsymmetry2;
	
	Gtk.ComboBoxText combo_select_jumps_asymmetry_bilateral;
	Gtk.ComboBoxText combo_select_jumps_asymmetry_1;
	Gtk.ComboBoxText combo_select_jumps_asymmetry_2;

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

		jumpsAsymmetryCalculate ();
		drawingarea_jumps_asymmetry.QueueDraw ();
	}
	private void on_radio_jumps_asymmetry_asymmetry_toggled (object o, EventArgs args)
	{
		combo_select_jumps_asymmetry_bilateral.Visible = false;
		label_jumps_asymmetry_bilateral.Visible = false;

		jumpsAsymmetryCalculate ();
		drawingarea_jumps_asymmetry.QueueDraw ();
	}

	private void on_radio_jumps_asymmetry_use_means_toggled (object o, EventArgs args)
	{
		jumpsAsymmetryCalculate ();
		drawingarea_jumps_asymmetry.QueueDraw ();
	}
	private void on_radio_jumps_asymmetry_use_maximums_toggled (object o, EventArgs args)
	{
		jumpsAsymmetryCalculate ();
		drawingarea_jumps_asymmetry.QueueDraw ();
	}

	private void on_combo_select_jumps_asymmetry_bilateral_changed (object o, EventArgs args)
	{
		jumpsAsymmetryCalculate ();
		drawingarea_jumps_asymmetry.QueueDraw ();
	}
	private void on_combo_select_jumps_asymmetry_1_changed (object o, EventArgs args)
	{
		jumpsAsymmetryCalculate ();
		drawingarea_jumps_asymmetry.QueueDraw ();
	}
	private void on_combo_select_jumps_asymmetry_2_changed (object o, EventArgs args)
	{
		jumpsAsymmetryCalculate ();
		drawingarea_jumps_asymmetry.QueueDraw ();
	}

	JumpsAsymmetryGraph jumpsAsymmetryGraph;

	private void jumpsAsymmetryCalculate ()
	{
		if (currentPerson == null || currentSession == null ||
				drawingarea_jumps_asymmetry == null || drawingarea_jumps_asymmetry.Window == null) //it happens at start on click on analyze
			return;

		if (jumpsAsymmetry == null)
			jumpsAsymmetry = new JumpsAsymmetry();

		jumpsAsymmetry.MouseReset ();
		jumpsAsymmetry.Calculate (
				currentPerson.UniqueID, currentSession.UniqueID,
				radio_jumps_asymmetry_bilateral.Active,
				radio_jumps_asymmetry_use_means.Active,
				comboSelectJumpsAsymmetryBilateral.GetSelectedNameEnglish (),
				comboSelectJumpsAsymmetry1.GetSelectedNameEnglish (),
				comboSelectJumpsAsymmetry2.GetSelectedNameEnglish ()
				);
	}

	//called just by QueueDraw
	private void jumpsAsymmetryPlot ()
	{
		button_jumps_asymmetry_save_image.Sensitive = false;

		if (currentPerson == null || currentSession == null ||
				drawingarea_jumps_asymmetry == null || drawingarea_jumps_asymmetry.Window == null) //it happens at start on click on analyze
		{
			button_jumps_asymmetry_save_image.Sensitive = false;
			return;
		}

		if (jumpsAsymmetry == null)
			jumpsAsymmetryCalculate ();

		string jumpBilateral = comboSelectJumpsAsymmetryBilateral.GetSelectedNameEnglish ();
		string jumpAsymmetry1 = comboSelectJumpsAsymmetry1.GetSelectedNameEnglish ();
		string jumpAsymmetry2 = comboSelectJumpsAsymmetry2.GetSelectedNameEnglish ();

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
				new CairoBars1Series (drawingarea_jumps_asymmetry, CairoBars.Type.NORMAL, preferences.fontType.ToString(), "Need more data"); //TODO: change message
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
						-1, 14, 10, "hola", new List<int> (), new List<int> ());
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
						currentSession.DateShort, false, true,
						jumpsAsymmetry.MouseX,
						jumpsAsymmetry.MouseY);
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
		if (jumpsAsymmetry != null)
			jumpsAsymmetry.MouseSet (args.Event.X, args.Event.Y);

		drawingarea_jumps_asymmetry.QueueDraw ();
	}
	private void on_drawingarea_jumps_asymmetry_draw (object o, Gtk.DrawnArgs args)
	{
		jumpsAsymmetryPlot ();
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

		LogB.Information("Saving");
		CairoUtil.GetScreenshotFromDrawingArea (drawingarea_jumps_asymmetry, destination);
	}
	private void on_overwrite_file_jumps_asymmetry_save_image_accepted (object o, EventArgs args)
	{
		on_button_jumps_asymmetry_save_image_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}

	private void connectWidgetsJumpAsymmetry (Gtk.Builder builder)
	{
		drawingarea_jumps_asymmetry = (Gtk.DrawingArea) builder.GetObject ("drawingarea_jumps_asymmetry");
		image_tab_jumps_asymmetry = (Gtk.Image) builder.GetObject ("image_tab_jumps_asymmetry");
		image_jumps_asymmetry_save = (Gtk.Image) builder.GetObject ("image_jumps_asymmetry_save");
		button_jumps_asymmetry_save_image = (Gtk.Button) builder.GetObject ("button_jumps_asymmetry_save_image");

		radio_jumps_asymmetry_bilateral = (Gtk.RadioButton) builder.GetObject ("radio_jumps_asymmetry_bilateral");
		radio_jumps_asymmetry_asymmetry = (Gtk.RadioButton) builder.GetObject ("radio_jumps_asymmetry_asymmetry");

		radio_jumps_asymmetry_use_means = (Gtk.RadioButton) builder.GetObject ("radio_jumps_asymmetry_use_means");

		hbox_combo_select_jumps_asymmetry_bilateral = (Gtk.HBox) builder.GetObject ("hbox_combo_select_jumps_asymmetry_bilateral");
		hbox_combo_select_jumps_asymmetry_1 = (Gtk.HBox) builder.GetObject ("hbox_combo_select_jumps_asymmetry_1");
		hbox_combo_select_jumps_asymmetry_2 = (Gtk.HBox) builder.GetObject ("hbox_combo_select_jumps_asymmetry_2");
		label_jumps_asymmetry_bilateral = (Gtk.Label) builder.GetObject ("label_jumps_asymmetry_bilateral");
	}
}
