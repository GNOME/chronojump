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
 * Copyright (C) 2004-2020   Xavier de Blas <xaviblas@gmail.com> 
 */


using System;
using Gtk;
using Glade;
using Mono.Unix;

public partial class ChronoJumpWindow 
{
	[Widget] Gtk.DrawingArea drawingarea_jumps_weight_fv_profile;
	[Widget] Gtk.Image image_tab_jumps_weight_fv_profile;
	[Widget] Gtk.Image image_jumps_weight_fv_profile_save;
	[Widget] Gtk.HBox hbox_combo_select_jumps_weight_fv_profile;
	[Widget] Gtk.ComboBox combo_select_jumps_weight_fv_profile;
	[Widget] Gtk.Button button_jumps_weight_fv_profile_save_image;
	[Widget] Gtk.CheckButton check_jumps_weight_fv_profile_only_best_in_weight;
	[Widget] Gtk.RadioButton radio_jumps_weight_fv_profile_show_full_graph;
	[Widget] Gtk.RadioButton radio_jumps_weight_fv_profile_zoom_to_points;

	JumpsWeightFVProfile jumpsWeightFVProfile;
	JumpsWeightFVProfileGraph jumpsWeightFVProfileGraph;
	CjComboSelectJumps comboSelectJumpsWeightFVProfile;

	// combo (start)
	private void createComboSelectJumpsWeightFVProfile(bool create)
	{
		/*
		if(create)
		{
			comboSelectJumpsWeightFVProfile = new CjComboSelectJumps(combo_select_jumps_weight_fv_profile, hbox_combo_select_jumps_weight_fv_profile, true);
			combo_select_jumps_weight_fv_profile = comboSelectJumpsWeightFVProfile.Combo;
			combo_select_jumps_weight_fv_profile.Changed += new EventHandler (on_combo_select_jumps_weight_fv_profile_changed);
		} else {
			comboSelectJumpsWeightFVProfile.Fill();
			combo_select_jumps_weight_fv_profile = comboSelectJumpsWeightFVProfile.Combo;
		}
		combo_select_jumps_weight_fv_profile.Sensitive = true;
		*/
	}
	private void on_combo_select_jumps_weight_fv_profile_changed(object o, EventArgs args)
	{
		/*
		ComboBox combo = o as ComboBox;
		if (o == null)
			return;

		jumpsWeightFVProfileDo(true);
		*/
	}
	// combo (end)

	private void jumpsWeightFVProfileDo (bool calculateData)
	{
		button_jumps_weight_fv_profile_save_image.Sensitive = false;

		if(currentPerson == null || currentPersonSession == null || currentSession == null ||
				drawingarea_jumps_weight_fv_profile == null || drawingarea_jumps_weight_fv_profile.GdkWindow == null) //it happens at start on click on analyze
			return;
		
		if(currentPersonSession.TrochanterToe == Constants.TrochanterToeUndefinedID || 
				currentPersonSession.TrochanterFloorOnFlexion == Constants.TrochanterFloorOnFlexionUndefinedID)
		{
			//constructor for showing blank screen with a message
			new JumpsWeightFVProfileGraph(drawingarea_jumps_weight_fv_profile, JumpsWeightFVProfileGraph.ErrorAtStart.NEEDLEGPARAMS, preferences.fontType.ToString());
					//currentPerson.Name, jumpType, currentSession.DateShort);

			return;
		}
		else if(currentPersonSession.TrochanterToe <= currentPersonSession.TrochanterFloorOnFlexion)
		{
			//constructor for showing blank screen with a message
			new JumpsWeightFVProfileGraph(drawingarea_jumps_weight_fv_profile, JumpsWeightFVProfileGraph.ErrorAtStart.BADLEGPARAMS, preferences.fontType.ToString());
					//currentPerson.Name, jumpType, currentSession.DateShort);

			return;
		}

		if(jumpsWeightFVProfile == null) {
			jumpsWeightFVProfile = new JumpsWeightFVProfile();
			calculateData = true;
		}

		//string jumpType = UtilGtk.ComboGetActive(combo_select_jumps_weight_fv_profile);
		//string jumpType = "SJl";

		if(calculateData)
			jumpsWeightFVProfile.Calculate(currentPerson.UniqueID, currentSession.UniqueID,
					currentPersonSession.Weight,
					currentPersonSession.TrochanterToe,
					currentPersonSession.TrochanterFloorOnFlexion,
					check_jumps_weight_fv_profile_only_best_in_weight.Active);

		if(jumpsWeightFVProfile.Point_l.Count == 0) {
			//constructor for showing blank screen with a message
			new JumpsWeightFVProfileGraph(drawingarea_jumps_weight_fv_profile, JumpsWeightFVProfileGraph.ErrorAtStart.NEEDJUMPS, preferences.fontType.ToString());
		} else if(jumpsWeightFVProfile.NeedMoreXData) {
			//constructor for showing blank screen with a message
			new JumpsWeightFVProfileGraph(drawingarea_jumps_weight_fv_profile, JumpsWeightFVProfileGraph.ErrorAtStart.NEEDJUMPSX, preferences.fontType.ToString());
		} else {
			//create the graph showing the points but showing also the error (if any)
			JumpsWeightFVProfileGraph.ErrorAtStart errorAtStart = JumpsWeightFVProfileGraph.ErrorAtStart.ALLOK;
			if(jumpsWeightFVProfile.F0 <= 0 && jumpsWeightFVProfile.V0 <= 0)
				errorAtStart = JumpsWeightFVProfileGraph.ErrorAtStart.F0ANDV0NOTPOSITIVE;
			else if (jumpsWeightFVProfile.F0 <= 0)
				errorAtStart = JumpsWeightFVProfileGraph.ErrorAtStart.F0NOTPOSITIVE;
			else if (jumpsWeightFVProfile.V0 <= 0)
				errorAtStart = JumpsWeightFVProfileGraph.ErrorAtStart.V0NOTPOSITIVE;

			//regular constructor
			jumpsWeightFVProfileGraph = new JumpsWeightFVProfileGraph(
					jumpsWeightFVProfile,
					drawingarea_jumps_weight_fv_profile,
					currentPerson.Name, //jumpType,
					currentSession.DateShort,
					radio_jumps_weight_fv_profile_show_full_graph.Active,
					errorAtStart);
			jumpsWeightFVProfileGraph.Do(preferences.fontType.ToString());

			button_jumps_weight_fv_profile_save_image.Sensitive = true;
		}
	}
	private void on_drawingarea_jumps_weight_fv_profile_expose_event (object o, ExposeEventArgs args) 
	{
		//needed to have mouse clicks at: on_drawingarea_jumps_weight_fv_profile_button_press_event ()
//		drawingarea_jumps_weight_fv_profile.AddEvents((int) (Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask));
		drawingarea_jumps_weight_fv_profile.AddEvents((int) Gdk.EventMask.ButtonPressMask);

		jumpsWeightFVProfileDo(false); //do not calculate data
		//data is calculated on switch page (at notebook_capture_analyze) or on change person
	}

	private void on_check_jumps_weight_fv_profile_only_best_in_weight_clicked (object o, EventArgs args)
	{
		jumpsWeightFVProfileDo (true);

		SqlitePreferences.Update(SqlitePreferences.JumpsFVProfileOnlyBestInWeight,
				check_jumps_weight_fv_profile_only_best_in_weight.Active, false);
	}

	private void on_radio_jumps_weight_fv_profile_show_full_graph_toggled (object o, EventArgs args)
	{
		if(radio_jumps_weight_fv_profile_show_full_graph.Active)
		{
			jumpsWeightFVProfileDo (false);

			SqlitePreferences.Update(SqlitePreferences.JumpsFVProfileShowFullGraph,
					radio_jumps_weight_fv_profile_show_full_graph.Active, false);
		}
	}
	private void on_radio_jumps_weight_fv_profile_zoom_to_points_toggled (object o, EventArgs args)
	{
		if(radio_jumps_weight_fv_profile_zoom_to_points.Active)
		{
			jumpsWeightFVProfileDo (false);

			SqlitePreferences.Update(SqlitePreferences.JumpsFVProfileShowFullGraph,
					radio_jumps_weight_fv_profile_show_full_graph.Active, false);
		}
	}

	private void on_drawingarea_jumps_weight_fv_profile_button_press_event (object o, ButtonPressEventArgs args)
	{
		//if there is no data and nothing to show, nothing to press, and also this is null
		if(jumpsWeightFVProfileGraph == null)
			return;

		LogB.Information("Button press done!");

		//redo the graph to delete previous rectangles of previous mouse clicks
		jumpsWeightFVProfileGraph.Do(preferences.fontType.ToString());
		LogB.Information(string.Format("Mouse X: {0}; Mouse Y: {1}", args.Event.X, args.Event.Y));
		//LogB.Information(string.Format("Real X: {0}; Real Y: {1}",
		//			jumpsWeightFVProfileGraph.CalculateAndWriteRealXY(args.Event.X, args.Event.Y)));
		jumpsWeightFVProfileGraph.CalculateAndWriteRealXY(args.Event.X, args.Event.Y);
	}

	private void on_button_jumps_weight_fv_profile_save_image_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.JUMPS_WEIGHT_FV_PROFILE_SAVE_IMAGE);
	}

	private void on_button_jumps_weight_fv_profile_save_image_selected (string destination)
	{
		if(drawingarea_jumps_weight_fv_profile == null)
			return;

		Gdk.Pixbuf pixbuf = Gdk.Pixbuf.FromDrawable(drawingarea_jumps_weight_fv_profile.GdkWindow, Gdk.Colormap.System,
				0, 0, 0, 0,
				UtilGtk.WidgetWidth(drawingarea_jumps_weight_fv_profile),
				UtilGtk.WidgetHeight(drawingarea_jumps_weight_fv_profile) );

		LogB.Information("Saving");
		pixbuf.Save(destination,"png");
	}
	private void on_overwrite_file_jumps_weight_fv_profile_save_image_accepted (object o, EventArgs args)
	{
		on_button_jumps_weight_fv_profile_save_image_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}

}
