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
 * Copyright (C) 2018-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO;
using Gtk;
using Gdk;
//using Glade;
using System.Text; //StringBuilder
using System.Collections.Generic; //List<T>
using Mono.Unix;

//struct with relevant data used on various functions and threads
public partial class ChronoJumpWindow 
{
	// at glade ---->
	//analyze tab
	Gtk.RadioButton radio_force_sensor_analyze_individual_current_set;
	Gtk.RadioButton radio_force_sensor_analyze_individual_current_session;
	Gtk.RadioButton radio_force_sensor_analyze_individual_all_sessions;
	Gtk.RadioButton radio_force_sensor_analyze_groupal_current_session;
	Gtk.Image image_force_sensor_analyze_individual_current_set;
	Gtk.Image image_force_sensor_analyze_individual_current_session;
	Gtk.Image image_force_sensor_analyze_individual_all_sessions;
	Gtk.Image image_force_sensor_analyze_groupal_current_session;

	Gtk.Notebook notebook_force_sensor_analyze_top;
	Gtk.HBox hbox_force_general_analysis;
	Gtk.Button button_force_sensor_analyze_load;
	Gtk.Box box_buttons_force_sensor_analyze_load_unchained;
	Gtk.Button button_force_sensor_analyze_analyze;
	Gtk.Label label_force_sensor_analyze;
	Gtk.Image image_force_sensor_graph;
	Gtk.Viewport viewport_force_sensor_graph;
	Gtk.Button button_force_sensor_image_save_rfd_auto;
	Gtk.Button button_force_sensor_image_save_rfd_manual;
	Gtk.Button button_force_sensor_analyze_AB_save;
	Gtk.Button button_force_sensor_analyze_CD_save;
	Gtk.CheckButton check_force_sensor_ai_chained_hscales;
	Gtk.CheckButton check_force_sensor_ai_zoom;

	Gtk.RadioButton radio_force_rfd_search_optimized_ab;
	Gtk.RadioButton radio_force_rfd_use_ab_range;
	Gtk.SpinButton spin_force_duration_seconds;
	Gtk.RadioButton radio_force_duration_seconds;
	Gtk.HBox hbox_force_rfd_duration_percent;
	Gtk.RadioButton radio_force_rfd_duration_percent;
	Gtk.SpinButton spin_force_rfd_duration_percent;

	//analyze options
	Gtk.Notebook notebook_force_sensor_rfd_options;
	Gtk.HBox hbox_force_sensor_analyze_top_modes;
//	Gtk.HBox hbox_force_sensor_analyze_automatic_options;
//	Gtk.Notebook notebook_force_analyze_automatic;
	Gtk.Button button_force_sensor_analyze_options_close_and_analyze;
	Gtk.Label label_hscale_force_sensor_ai_a_pre_1s;
	Gtk.Label label_hscale_force_sensor_ai_a_post_1s;
	Gtk.Label label_hscale_force_sensor_ai_b_pre_1s;
	Gtk.Label label_hscale_force_sensor_ai_b_post_1s;
	Gtk.VBox vbox_force_rfd_duration_end;
	Gtk.Button button_force_sensor_analyze_options;
	Gtk.HBox hbox_force_1;
	Gtk.HBox hbox_force_2;
	Gtk.HBox hbox_force_3;
	Gtk.HBox hbox_force_4;
	Gtk.HBox hbox_force_impulse;
	Gtk.CheckButton check_force_1;
	Gtk.CheckButton check_force_2;
	Gtk.CheckButton check_force_3;
	Gtk.CheckButton check_force_4;
	Gtk.CheckButton check_force_impulse;
	Gtk.HBox hbox_force_1_at_ms;
	Gtk.HBox hbox_force_2_at_ms;
	Gtk.HBox hbox_force_3_at_ms;
	Gtk.HBox hbox_force_4_at_ms;
	Gtk.HBox hbox_force_1_at_percent;
	Gtk.HBox hbox_force_2_at_percent;
	Gtk.HBox hbox_force_3_at_percent;
	Gtk.HBox hbox_force_4_at_percent;
	Gtk.HBox hbox_force_impulse_until_percent;
	Gtk.HBox hbox_force_1_from_to;
	Gtk.HBox hbox_force_2_from_to;
	Gtk.HBox hbox_force_3_from_to;
	Gtk.HBox hbox_force_4_from_to;
	Gtk.HBox hbox_force_1_in_x_ms;
	Gtk.HBox hbox_force_2_in_x_ms;
	Gtk.HBox hbox_force_3_in_x_ms;
	Gtk.HBox hbox_force_4_in_x_ms;
	Gtk.SpinButton spinbutton_force_1_at_ms;
	Gtk.SpinButton spinbutton_force_2_at_ms;
	Gtk.SpinButton spinbutton_force_3_at_ms;
	Gtk.SpinButton spinbutton_force_4_at_ms;
	Gtk.HBox hbox_force_impulse_from_to;
	Gtk.SpinButton spinbutton_force_1_at_percent;
	Gtk.SpinButton spinbutton_force_2_at_percent;
	Gtk.SpinButton spinbutton_force_3_at_percent;
	Gtk.SpinButton spinbutton_force_4_at_percent;
	Gtk.SpinButton spinbutton_force_impulse_until_percent;
	Gtk.SpinButton spinbutton_force_1_from;
	Gtk.SpinButton spinbutton_force_2_from;
	Gtk.SpinButton spinbutton_force_3_from;
	Gtk.SpinButton spinbutton_force_4_from;
	Gtk.SpinButton spinbutton_force_impulse_from;
	Gtk.SpinButton spinbutton_force_1_to;
	Gtk.SpinButton spinbutton_force_2_to;
	Gtk.SpinButton spinbutton_force_3_to;
	Gtk.SpinButton spinbutton_force_4_to;
	Gtk.SpinButton spinbutton_force_impulse_to;
	Gtk.SpinButton spinbutton_force_1_in_x_ms;
	Gtk.SpinButton spinbutton_force_2_in_x_ms;
	Gtk.SpinButton spinbutton_force_3_in_x_ms;
	Gtk.SpinButton spinbutton_force_4_in_x_ms;

	Gtk.Button button_hscale_force_sensor_ai_a_first;
	Gtk.Button button_hscale_force_sensor_ai_a_pre;
	Gtk.Button button_hscale_force_sensor_ai_a_post;
	Gtk.Button button_hscale_force_sensor_ai_a_last;

	Gtk.Button button_hscale_force_sensor_ai_b_first;
	Gtk.Button button_hscale_force_sensor_ai_b_pre;
	Gtk.Button button_hscale_force_sensor_ai_b_post;
	Gtk.Button button_hscale_force_sensor_ai_b_last;

	Gtk.Notebook notebook_force_sensor_export;
	Gtk.Label label_force_sensor_export_data;
	Gtk.HBox hbox_force_sensor_export_images;
	Gtk.CheckButton check_force_sensor_export_images;
	Gtk.HBox hbox_force_sensor_export_width_height;
	Gtk.SpinButton spinbutton_force_sensor_export_image_width;
	Gtk.SpinButton spinbutton_force_sensor_export_image_height;
	Gtk.ProgressBar progressbar_force_sensor_export;
	Gtk.Label label_force_sensor_export_result;
	Gtk.Button button_force_sensor_export_result_open;

	Gtk.HBox hbox_force_sensor_analyze_ai_sliders_and_buttons;
	Gtk.Box box_force_sensor_analyze_magnitudes;
	Gtk.CheckButton check_force_sensor_analyze_show_distance;
	Gtk.CheckButton check_force_sensor_analyze_show_speed;
	Gtk.CheckButton check_force_sensor_analyze_show_power;
	Gtk.Image image_force_sensor_analyze_show_distance;
	Gtk.Image image_force_sensor_analyze_show_speed;
	Gtk.Image image_force_sensor_analyze_show_power;
	Gtk.DrawingArea force_sensor_ai_drawingarea_cairo;
	Gtk.Viewport viewport_force_sensor_analyze_hscales;
	Gtk.Viewport viewport_radio_force_sensor_ai_ab;
	Gtk.Viewport viewport_radio_force_sensor_ai_cd;
	Gtk.RadioButton radio_force_sensor_ai_ab;
	Gtk.RadioButton radio_force_sensor_ai_cd;
	Gtk.CheckButton check_force_sensor_ai_chained_load_link;
	Gtk.Image image_force_sensor_ai_chained_load_link;
	Gtk.Box box_force_sensor_ai_a;
	Gtk.Box box_force_sensor_ai_b;
	Gtk.Box box_force_sensor_ai_c;
	Gtk.Box box_force_sensor_ai_d;
	Gtk.Label label_force_sensor_ai_zoom_abcd;
	Gtk.HScale hscale_force_sensor_ai_a;
	Gtk.HScale hscale_force_sensor_ai_b;
	Gtk.HScale hscale_force_sensor_ai_c;
	Gtk.HScale hscale_force_sensor_ai_d;
	Gtk.TreeView treeview_force_sensor_ai_AB;
	Gtk.TreeView treeview_force_sensor_ai_CD;
	Gtk.TreeView treeview_force_sensor_ai_other;

	Gtk.ComboBoxText combo_force_1_function;
	Gtk.ComboBoxText combo_force_2_function;
	Gtk.ComboBoxText combo_force_3_function;
	Gtk.ComboBoxText combo_force_4_function;
	Gtk.ComboBoxText combo_force_impulse_function;
	Gtk.ComboBoxText combo_force_1_type;
	Gtk.ComboBoxText combo_force_2_type;
	Gtk.ComboBoxText combo_force_3_type;
	Gtk.ComboBoxText combo_force_4_type;
	Gtk.ComboBoxText combo_force_impulse_type;
	// <---- at glade


	private RepetitionMouseLimits fsAIRepetitionMouseLimits;
	private RepetitionMouseLimitsWithSamples fsAIRepetitionMouseLimitsCairo;

	private enum notebook_force_sensor_analyze_top_pages { CURRENTSETSIGNAL, CURRENTSETMODEL, CURRENTSESSION, AUTOMATICOPTIONS }
	/*
	 * analyze options -------------------------->
	 */

	private bool button_force_sensor_analyze_analyze_was_sensitive; //needed this temp variable
	private void forceSensorAnalyzeOptionsSensitivity(bool s) //s for sensitive. When show options frame is ! s
	{
		button_force_sensor_analyze_options.Sensitive = s;
		button_force_sensor_analyze_load.Sensitive = s;

		if(s)
			button_force_sensor_analyze_analyze.Sensitive = button_force_sensor_analyze_analyze_was_sensitive;
		else {
			button_force_sensor_analyze_analyze_was_sensitive =	button_force_sensor_analyze_analyze.Sensitive;
			button_force_sensor_analyze_analyze.Sensitive = false;
		}

		menus_and_mode_sensitive(s);
		hbox_contacts_sup_capture_analyze_two_buttons.Sensitive = s;
		hbox_top_person.Sensitive = s;
	}

	private int notebook_force_sensor_analyze_top_LastPage;
	private void on_button_force_sensor_analyze_options_clicked (object o, EventArgs args)
	{
		//store the notebook to return to same place
		notebook_force_sensor_analyze_top_LastPage = notebook_force_sensor_analyze_top.CurrentPage;
		notebook_force_sensor_analyze_top.CurrentPage = Convert.ToInt32(notebook_force_sensor_analyze_top_pages.AUTOMATICOPTIONS);

		hbox_force_sensor_analyze_top_modes.Sensitive = false;

		button_force_sensor_analyze_options_close_and_analyze.Visible = radio_force_sensor_analyze_individual_current_set.Active;

		forceSensorAnalyzeOptionsSensitivity(false);
	}

	private void on_button_force_sensor_analyze_options_close_clicked (object o, EventArgs args)
	{
		notebook_force_sensor_analyze_top.CurrentPage = notebook_force_sensor_analyze_top_LastPage;

		hbox_force_sensor_analyze_top_modes.Sensitive = true;

		// 1 change stuff on Sqlite if needed

		Sqlite.Open();
		List<ForceSensorRFD> newRFDList = getRFDValues();
		int i = 0;
		foreach(ForceSensorRFD rfd in newRFDList)
		{
			if(rfdList[i].Changed(rfd))
			{
				SqliteForceSensorRFD.Update(true, rfd);
				rfdList[i] = rfd;
			}
			i ++;
		}

		ForceSensorImpulse newImpulse = getImpulseValue();
		if(newImpulse.Changed(impulse))
		{
			SqliteForceSensorRFD.UpdateImpulse(true, newImpulse);
			impulse = newImpulse;
		}

		if(preferences.forceSensorStartEndOptimized != radio_force_rfd_search_optimized_ab.Active)
		{
			preferences.forceSensorStartEndOptimized = radio_force_rfd_search_optimized_ab.Active;
			SqlitePreferences.Update(SqlitePreferences.ForceSensorStartEndOptimized,
					radio_force_rfd_search_optimized_ab.Active.ToString(), true);
		}

		if(preferences.forceSensorMIFDurationMode == Preferences.ForceSensorMIFDurationModes.SECONDS &&
				radio_force_rfd_duration_percent.Active)
		{
			preferences.forceSensorMIFDurationMode = Preferences.ForceSensorMIFDurationModes.PERCENT;
			SqlitePreferences.Update(SqlitePreferences.ForceSensorMIFDurationMode,
					preferences.forceSensorMIFDurationMode.ToString(), true);
		}
		else if(preferences.forceSensorMIFDurationMode == Preferences.ForceSensorMIFDurationModes.PERCENT &&
				radio_force_duration_seconds.Active)
		{
			preferences.forceSensorMIFDurationMode = Preferences.ForceSensorMIFDurationModes.SECONDS;
			SqlitePreferences.Update(SqlitePreferences.ForceSensorMIFDurationMode,
					preferences.forceSensorMIFDurationMode.ToString(), true);
		}

		preferences.forceSensorMIFDurationSeconds = Preferences.PreferencesChange(
				true, SqlitePreferences.ForceSensorMIFDurationSeconds,
				preferences.forceSensorMIFDurationSeconds, spin_force_duration_seconds.Value);
		preferences.forceSensorMIFDurationPercent = Preferences.PreferencesChange(
				true, SqlitePreferences.ForceSensorMIFDurationPercent,
				preferences.forceSensorMIFDurationPercent, Convert.ToInt32(spin_force_rfd_duration_percent.Value));

		Sqlite.Close();

		// 2 change sensitivity of widgets

		forceSensorAnalyzeOptionsSensitivity(true);
	}

	private void on_button_force_sensor_analyze_options_close_and_analyze_clicked (object o, EventArgs args)
	{
		on_button_force_sensor_analyze_options_close_clicked (o, args);
		on_button_force_sensor_analyze_analyze_clicked (o, args);
	}

	private void on_button_force_sensor_analyze_analyze_clicked (object o, EventArgs args)
	{
		notebook_force_sensor_analyze_top.CurrentPage = Convert.ToInt32(notebook_force_sensor_analyze_top_pages.CURRENTSETMODEL);

		if(! Util.FileExists(lastForceSensorFullPath))
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileNotFoundStr());
			return;
		}

		if(lastForceSensorFullPath != null && lastForceSensorFullPath != "")
			forceSensorCopyTempAndDoGraphs(forceSensorGraphsEnum.RFD);
	}

	private void on_button_force_sensor_analyze_back_to_signal_clicked (object o, EventArgs args)
	{
		notebook_force_sensor_analyze_top.CurrentPage = Convert.ToInt32(notebook_force_sensor_analyze_top_pages.CURRENTSETSIGNAL);
	}

	private void on_radio_force_rfd_search_optimized_ab_toggled (object o, EventArgs args)
	{
		vbox_force_rfd_duration_end.Sensitive = true;
	}
	private void on_radio_force_rfd_use_ab_range_toggled (object o, EventArgs args)
	{
		vbox_force_rfd_duration_end.Sensitive = false;
	}

	private void check_force_visibilities()
	{
		hbox_force_1.Visible = (check_force_1.Active);
		hbox_force_2.Visible = (check_force_2.Active);
		hbox_force_3.Visible = (check_force_3.Active);
		hbox_force_4.Visible = (check_force_4.Active);
		hbox_force_impulse.Visible = (check_force_impulse.Active);
	}

	private void on_check_force_clicked (object o, EventArgs args)
	{
		check_force_visibilities();
	}

	private void createForceAnalyzeCombos ()
	{
		/*
		 * usually we have an hbox on glade, we create a combo here and we attach
		 * this technique is used also on createRunEncoderAnalyzeCombos ()
		 * the combo is in glade, without elements, but the elements is in bold because it has been edited, but is blank
		 * you can see in the app1.glade:
		 * <property name="items"/>
		 */

		UtilGtk.ComboUpdate(combo_force_1_function, ForceSensorRFD.FunctionsArray(true), "");
		UtilGtk.ComboUpdate(combo_force_2_function, ForceSensorRFD.FunctionsArray(true), "");
		UtilGtk.ComboUpdate(combo_force_3_function, ForceSensorRFD.FunctionsArray(true), "");
		UtilGtk.ComboUpdate(combo_force_4_function, ForceSensorRFD.FunctionsArray(true), "");
		UtilGtk.ComboUpdate(combo_force_impulse_function, ForceSensorImpulse.FunctionsArray(true), "");

		UtilGtk.ComboUpdate(combo_force_1_type, ForceSensorRFD.TypesArray(true), "");
		UtilGtk.ComboUpdate(combo_force_2_type, ForceSensorRFD.TypesArray(true), "");
		UtilGtk.ComboUpdate(combo_force_3_type, ForceSensorRFD.TypesArray(true), "");
		UtilGtk.ComboUpdate(combo_force_4_type, ForceSensorRFD.TypesArray(true), "");
		UtilGtk.ComboUpdate(combo_force_impulse_type, ForceSensorImpulse.TypesArrayImpulse(true), "");
	}

	private void on_combo_force_type_changed (object o, EventArgs args)
	{
		Gtk.ComboBoxText combo = o as ComboBoxText;
		if (o == null)
			return;

		if(combo == combo_force_1_type)
			combo_force_visibility(
					UtilGtk.ComboGetActive(combo_force_1_type),
					hbox_force_1_at_ms,
					hbox_force_1_at_percent,
					hbox_force_1_from_to,
					hbox_force_1_in_x_ms);
		else if(combo == combo_force_2_type)
			combo_force_visibility(
					UtilGtk.ComboGetActive(combo_force_2_type),
					hbox_force_2_at_ms,
					hbox_force_2_at_percent,
					hbox_force_2_from_to,
					hbox_force_2_in_x_ms);
		else if(combo == combo_force_3_type)
			combo_force_visibility(
					UtilGtk.ComboGetActive(combo_force_3_type),
					hbox_force_3_at_ms,
					hbox_force_3_at_percent,
					hbox_force_3_from_to,
					hbox_force_3_in_x_ms);
		else if(combo == combo_force_4_type)
			combo_force_visibility(
					UtilGtk.ComboGetActive(combo_force_4_type),
					hbox_force_4_at_ms,
					hbox_force_4_at_percent,
					hbox_force_4_from_to,
					hbox_force_4_in_x_ms);
		else if(combo == combo_force_impulse_type)
			combo_force_impulse_visibility(
					UtilGtk.ComboGetActive(combo_force_impulse_type),
					hbox_force_impulse_until_percent,
					hbox_force_impulse_from_to);
	}

	private void combo_force_visibility (string selected, Gtk.HBox at_ms, Gtk.HBox at_percent, Gtk.HBox from_to, Gtk.HBox in_x_ms)
	{
		//valid for active == "" and active == "RFD max"
		at_ms.Visible = false;
		from_to.Visible = false;
		at_percent.Visible = false;
		in_x_ms.Visible = false;

		if(selected == Catalog.GetString(ForceSensorRFD.Type_INSTANTANEOUS_name))
			at_ms.Visible = true;
		else if(selected == Catalog.GetString(ForceSensorRFD.Type_AVERAGE_name))
			from_to.Visible = true;
		else if(selected == Catalog.GetString(ForceSensorRFD.Type_PERCENT_F_MAX_name))
			at_percent.Visible = true;
		else if(selected == Catalog.GetString(ForceSensorRFD.Type_BEST_AVG_RFD_IN_X_MS_name))
			in_x_ms.Visible = true;
	}
	private void combo_force_impulse_visibility (string selected, Gtk.HBox until_percent, Gtk.HBox from_to)
	{
		until_percent.Visible = false;
		from_to.Visible = false;

		if(selected == Catalog.GetString(ForceSensorImpulse.Type_IMP_UNTIL_PERCENT_F_MAX_name))
			until_percent.Visible = true;
		else if(selected == Catalog.GetString(ForceSensorImpulse.Type_IMP_RANGE_name))
			from_to.Visible = true;
	}

	private void setForceDurationRadios()
	{
		if(preferences.forceSensorStartEndOptimized)
		{
			radio_force_rfd_search_optimized_ab.Active = true;
			vbox_force_rfd_duration_end.Sensitive = true;
		} else {
			radio_force_rfd_use_ab_range.Active = true;
			vbox_force_rfd_duration_end.Sensitive = false;
		}

		if(preferences.forceSensorMIFDurationMode == Preferences.ForceSensorMIFDurationModes.SECONDS)
			radio_force_duration_seconds.Active = true;
		else //(preferences.forceSensorMIFDurationMode == Preferences.ForceSensorMIFDurationModes.PERCENT)
			radio_force_rfd_duration_percent.Active = true;

		//to show/hides spinbuttons
		on_radio_force_rfd_duration_toggled (new object (), new EventArgs ());

		spin_force_duration_seconds.Value = preferences.forceSensorMIFDurationSeconds;
		spin_force_rfd_duration_percent.Value = preferences.forceSensorMIFDurationPercent;
	}

	private void setRFDValues ()
	{
		setRFDValue (rfdList[0], check_force_1, combo_force_1_function, combo_force_1_type,
				hbox_force_1_at_ms, spinbutton_force_1_at_ms,
				hbox_force_1_at_percent, spinbutton_force_1_at_percent,
				hbox_force_1_from_to, spinbutton_force_1_from, spinbutton_force_1_to,
				hbox_force_1_in_x_ms, spinbutton_force_1_in_x_ms);

		setRFDValue (rfdList[1], check_force_2, combo_force_2_function, combo_force_2_type,
				hbox_force_2_at_ms, spinbutton_force_2_at_ms,
				hbox_force_2_at_percent, spinbutton_force_2_at_percent,
				hbox_force_2_from_to, spinbutton_force_2_from, spinbutton_force_2_to,
				hbox_force_2_in_x_ms, spinbutton_force_2_in_x_ms);

		setRFDValue (rfdList[2], check_force_3, combo_force_3_function, combo_force_3_type,
				hbox_force_3_at_ms, spinbutton_force_3_at_ms,
				hbox_force_3_at_percent, spinbutton_force_3_at_percent,
				hbox_force_3_from_to, spinbutton_force_3_from, spinbutton_force_3_to,
				hbox_force_3_in_x_ms, spinbutton_force_3_in_x_ms);

		setRFDValue (rfdList[3], check_force_4, combo_force_4_function, combo_force_4_type,
				hbox_force_4_at_ms, spinbutton_force_4_at_ms,
				hbox_force_4_at_percent, spinbutton_force_4_at_percent,
				hbox_force_4_from_to, spinbutton_force_4_from, spinbutton_force_4_to,
				hbox_force_4_in_x_ms, spinbutton_force_4_in_x_ms);
	}

	private void setRFDValue (ForceSensorRFD rfd, Gtk.CheckButton check, Gtk.ComboBoxText combo_force_function, Gtk.ComboBoxText combo_force_type,
			Gtk.HBox hbox_force_at_ms, Gtk.SpinButton spinbutton_force_at_ms,
			Gtk.HBox hbox_force_at_percent, Gtk.SpinButton spinbutton_force_at_percent,
			Gtk.HBox hbox_force_from_to, Gtk.SpinButton spinbutton_force_from, Gtk.SpinButton spinbutton_force_to,
			Gtk.HBox hbox_force_in_x_ms, Gtk.SpinButton spinbutton_force_in_x_ms)
	{
		check.Active = rfd.active;

		combo_force_function.Active = UtilGtk.ComboMakeActive(combo_force_function, rfd.FunctionPrint(true));
		combo_force_type.Active = UtilGtk.ComboMakeActive(combo_force_type, rfd.TypePrint(true));

		hbox_force_at_ms.Visible = false;
		hbox_force_at_percent.Visible = false;
		hbox_force_from_to.Visible = false;
		hbox_force_in_x_ms.Visible = false;

		if(rfd.type == ForceSensorRFD.Types.INSTANTANEOUS)
		{
			hbox_force_at_ms.Visible = true;
			spinbutton_force_at_ms.Value = rfd.num1;
		}
		else if(rfd.type == ForceSensorRFD.Types.AVERAGE)
		{
			hbox_force_from_to.Visible = true;
			spinbutton_force_from.Value = rfd.num1;
			spinbutton_force_to.Value = rfd.num2;
		}
		else if(rfd.type == ForceSensorRFD.Types.PERCENT_F_MAX)
		{
			hbox_force_at_percent.Visible = true;
			spinbutton_force_at_percent.Value = rfd.num1;
		}
		else if(rfd.type == ForceSensorRFD.Types.BEST_AVG_RFD_IN_X_MS)
		{
			hbox_force_in_x_ms.Visible = true;
			spinbutton_force_in_x_ms.Value = rfd.num1;
		}
	}

	private List<ForceSensorRFD> getRFDValues ()
	{
		List<ForceSensorRFD> l = new List<ForceSensorRFD>();
		l.Add(getRFDValue("RFD1", check_force_1, combo_force_1_function, combo_force_1_type,
					spinbutton_force_1_at_ms, spinbutton_force_1_at_percent,
					spinbutton_force_1_from, spinbutton_force_1_to,
					spinbutton_force_1_in_x_ms));
		l.Add(getRFDValue("RFD2", check_force_2, combo_force_2_function, combo_force_2_type,
					spinbutton_force_2_at_ms, spinbutton_force_2_at_percent,
					spinbutton_force_2_from, spinbutton_force_2_to,
					spinbutton_force_2_in_x_ms));
		l.Add(getRFDValue("RFD3", check_force_3, combo_force_3_function, combo_force_3_type,
					spinbutton_force_3_at_ms, spinbutton_force_3_at_percent,
					spinbutton_force_3_from, spinbutton_force_3_to,
					spinbutton_force_3_in_x_ms));
		l.Add(getRFDValue("RFD4", check_force_4, combo_force_4_function, combo_force_4_type,
					spinbutton_force_4_at_ms, spinbutton_force_4_at_percent,
					spinbutton_force_4_from, spinbutton_force_4_to,
					spinbutton_force_4_in_x_ms));
		return l;
	}
	private ForceSensorRFD getRFDValue (string code, Gtk.CheckButton check, Gtk.ComboBoxText combo_force_function, Gtk.ComboBoxText combo_force_type,
			Gtk.SpinButton spinbutton_force_at_ms, Gtk.SpinButton spinbutton_force_at_percent,
			Gtk.SpinButton spinbutton_force_from, Gtk.SpinButton spinbutton_force_to,
			Gtk.SpinButton spinbutton_force_in_x_ms)

	{
		bool active = check.Active;
		int num1 = -1;
		int num2 = -1;

		ForceSensorRFD.Functions function;
		if(UtilGtk.ComboGetActive(combo_force_function) == ForceSensorRFD.Function_RAW_name)
			function = ForceSensorRFD.Functions.RAW;
		else //(UtilGtk.ComboGetActive(combo_force_function) == ForceSensorRFD.Function_FITTED_name)
			function = ForceSensorRFD.Functions.FITTED;

		ForceSensorRFD.Types type;
		string typeStr = UtilGtk.ComboGetActive(combo_force_type);
		if (typeStr == Catalog.GetString (ForceSensorRFD.Type_INSTANTANEOUS_name))
		{
			num1 = Convert.ToInt32 (spinbutton_force_at_ms.Value);
			type = ForceSensorRFD.Types.INSTANTANEOUS;
		}
		else if (typeStr == Catalog.GetString (ForceSensorRFD.Type_AVERAGE_name))
		{
			num1 = Convert.ToInt32 (spinbutton_force_from.Value);
			num2 = Convert.ToInt32 (spinbutton_force_to.Value);
			type = ForceSensorRFD.Types.AVERAGE;
		}
		else if (typeStr == Catalog.GetString (ForceSensorRFD.Type_PERCENT_F_MAX_name))
		{
			num1 = Convert.ToInt32 (spinbutton_force_at_percent.Value);
			type = ForceSensorRFD.Types.PERCENT_F_MAX;
		}
		else if (typeStr == Catalog.GetString (ForceSensorRFD.Type_RFD_MAX_name))
			type = ForceSensorRFD.Types.RFD_MAX;
		else //if (typeStr == Catalog.GetString (ForceSensorRFD.Type_BEST_AVG_RFD_IN_X_MS_name))
		{
			num1 = Convert.ToInt32 (spinbutton_force_in_x_ms.Value);
			type = ForceSensorRFD.Types.BEST_AVG_RFD_IN_X_MS;
		}

		return new ForceSensorRFD (code, active, function, type, num1, num2);
	}

	private void setImpulseValue ()
	{
		check_force_impulse.Active = impulse.active;

		combo_force_impulse_function.Active = UtilGtk.ComboMakeActive(combo_force_impulse_function, impulse.FunctionPrint(true));
		combo_force_impulse_type.Active = UtilGtk.ComboMakeActive(combo_force_impulse_type, impulse.TypePrint(true));

		hbox_force_impulse_until_percent.Visible = false;
		hbox_force_impulse_from_to.Visible = false;

		if(impulse.type == ForceSensorImpulse.Types.IMP_UNTIL_PERCENT_F_MAX)
		{
			hbox_force_impulse_until_percent.Visible = true;
			//num1 is 0
			spinbutton_force_impulse_until_percent.Value = impulse.num2;
		}
		else if(impulse.type == ForceSensorImpulse.Types.IMP_RANGE)
		{
			hbox_force_impulse_from_to.Visible = true;
			spinbutton_force_impulse_from.Value = impulse.num1;
			spinbutton_force_impulse_to.Value = impulse.num2;
		}
	}
	private ForceSensorImpulse getImpulseValue ()
	{
		bool active = check_force_impulse.Active;
		int num1 = -1;
		int num2 = -1;

		ForceSensorImpulse.Functions function;
		if(UtilGtk.ComboGetActive(combo_force_impulse_function) == ForceSensorImpulse.Function_RAW_name)
			function = ForceSensorImpulse.Functions.RAW;
		else //(UtilGtk.ComboGetActive(combo_force_impulse_function) == ForceSensorImpulse.Function_FITTED_name)
			function = ForceSensorImpulse.Functions.FITTED;

		ForceSensorImpulse.Types type;
		string typeStr = UtilGtk.ComboGetActive(combo_force_impulse_type);

		if(typeStr == Catalog.GetString(ForceSensorImpulse.Type_IMP_UNTIL_PERCENT_F_MAX_name))
		{
			num1 = 0;
			num2 = Convert.ToInt32(spinbutton_force_impulse_until_percent.Value);
			type = ForceSensorImpulse.Types.IMP_UNTIL_PERCENT_F_MAX;
		}
		else // if(typeStr == Catalog.GetString(ForceSensorImpulse.Type_IMP_RANGE_name))
		{
			num1 = Convert.ToInt32(spinbutton_force_impulse_from.Value);
			num2 = Convert.ToInt32(spinbutton_force_impulse_to.Value);
			type = ForceSensorImpulse.Types.IMP_RANGE;
		}

		return new ForceSensorImpulse(active, function, type, num1, num2);
	}

	private void setForceSensorAnalyzeABSliderIncrements()
	{
		label_hscale_force_sensor_ai_a_pre_1s.Text = string.Format("{0}s", preferences.forceSensorAnalyzeABSliderIncrement);
		label_hscale_force_sensor_ai_a_post_1s.Text = string.Format("{0}s", preferences.forceSensorAnalyzeABSliderIncrement);
		label_hscale_force_sensor_ai_b_pre_1s.Text = string.Format("{0}s", preferences.forceSensorAnalyzeABSliderIncrement);
		label_hscale_force_sensor_ai_b_post_1s.Text = string.Format("{0}s", preferences.forceSensorAnalyzeABSliderIncrement);
	}
	private void setForceSensorAnalyzeMaxAVGInWindow()
	{
		tvFS_other.MaxAvgInWindowName = string.Format("Max AVG Force in {0} s",
				preferences.forceSensorAnalyzeMaxAVGInWindow);
	}

	private void on_button_force_rfd_default_clicked (object o, EventArgs args)
	{
		Sqlite.Open();

		SqliteForceSensorRFD.DeleteAll(true);
		SqliteForceSensorRFD.InsertDefaultValues(true);

		rfdList = SqliteForceSensorRFD.SelectAll(false);
		impulse = SqliteForceSensorRFD.SelectImpulse(false);

		setRFDValues();
		setImpulseValue();

		Sqlite.Close();
	}

	/*
	 * <------------------------ end of analyze options
	 */

	public List<ForceSensorRFD> GetRFDList
	{
		get { return rfdList;  }
	}

	public ForceSensorImpulse GetImpulse
	{
		get { return impulse;  }
	}


	ForceSensorAnalyzeInstant fsAI_AB;
	ForceSensorAnalyzeInstant fsAI_CD;

	/*
	private void on_radiobutton_force_sensor_analyze_automatic_toggled (object o, EventArgs args)
	{
		if(! radiobutton_force_sensor_analyze_automatic.Active)
			return;

//		hbox_force_sensor_analyze_automatic_options.Visible = true;
		notebook_force_sensor_analyze.CurrentPage = Convert.ToInt32(notebook_force_sensor_analyze_pages.AUTOMATIC);
	}
	*/

	//move to export gui file

	private void on_check_force_sensor_export_images_toggled (object o, EventArgs args)
	{
		hbox_force_sensor_export_width_height.Visible = check_force_sensor_export_images.Active;

		//also hide the label and the open button
		label_force_sensor_export_result.Text = "";
		button_force_sensor_export_result_open.Visible = false;
	}

	private void on_radio_force_sensor_analyze_individual_current_set_toggled (object o, EventArgs args)
	{
		button_force_sensor_analyze_load.Visible = true;
		notebook_force_sensor_analyze_top.CurrentPage = Convert.ToInt32(notebook_force_sensor_analyze_top_pages.CURRENTSETSIGNAL);
		label_force_sensor_export_result.Text = "";
		button_force_sensor_export_result_open.Visible = false;
		button_force_sensor_analyze_analyze.Visible = true;
	}
	private void on_radio_force_sensor_analyze_individual_session_current_or_all_toggled (object o, EventArgs args)
	{
		button_force_sensor_analyze_load.Visible = false;

		if(currentPerson != null)
			label_force_sensor_export_data.Text = currentPerson.Name;
		else
			label_force_sensor_export_data.Text = "";

		notebook_force_sensor_analyze_top.CurrentPage = Convert.ToInt32(notebook_force_sensor_analyze_top_pages.CURRENTSESSION);
		label_force_sensor_export_result.Text = "";
		button_force_sensor_export_result_open.Visible = false;
		button_force_sensor_analyze_analyze.Visible = false;
	}
	private void on_radio_force_sensor_analyze_groupal_current_session_toggled (object o, EventArgs args)
	{
		button_force_sensor_analyze_load.Visible = false;

		label_force_sensor_export_data.Text = currentSession.Name;

		notebook_force_sensor_analyze_top.CurrentPage = Convert.ToInt32(notebook_force_sensor_analyze_top_pages.CURRENTSESSION);
		label_force_sensor_export_result.Text = "";
		button_force_sensor_export_result_open.Visible = false;
		button_force_sensor_analyze_analyze.Visible = false;
	}

	//everything except the current set
	private void on_button_force_sensor_export_not_set_clicked (object o, EventArgs args)
	{
		// 1) check if all sessions
		if(radio_force_sensor_analyze_individual_all_sessions.Active)
		{
			if(currentPerson == null)
				return;

			button_force_sensor_export_session (currentPerson.UniqueID, -1);
			return;
		}

		// 2) current session (individual or groupal)
		if(currentSession == null)
			return;

		if (radio_force_sensor_analyze_individual_current_session.Active)
		{
			if(currentPerson == null)
				return;

			button_force_sensor_export_session (currentPerson.UniqueID, currentSession.UniqueID);
		}
		else if (radio_force_sensor_analyze_groupal_current_session.Active)
		{
			button_force_sensor_export_session (-1, currentSession.UniqueID);
		}
	}

	ForceSensorExport forceSensorExport;
	private void button_force_sensor_export_session (int personID, int sessionID)
	{
		double duration = -1;
		if(radio_force_duration_seconds.Active)
			duration = Convert.ToDouble(spin_force_duration_seconds.Value);

		label_force_sensor_export_result.Text = "";
		button_force_sensor_export_result_open.Visible = false;
		forceSensorButtonsSensitive(false);
		hbox_force_sensor_analyze_top_modes.Sensitive = false;
		button_force_sensor_analyze_options.Sensitive = false;
		hbox_force_sensor_export_images.Sensitive = false;

		//store new width/height if changed
		Sqlite.Open();
		preferences.exportGraphWidth = Preferences.PreferencesChange(
				true,
				SqlitePreferences.ExportGraphWidth,
				preferences.exportGraphWidth, Convert.ToInt32(spinbutton_force_sensor_export_image_width.Value));
		preferences.exportGraphHeight = Preferences.PreferencesChange(
				true,
				SqlitePreferences.ExportGraphHeight,
				preferences.exportGraphHeight, Convert.ToInt32(spinbutton_force_sensor_export_image_height.Value));
		Sqlite.Close();

		//change also spinbuttons of export sprint and runEncoder
		spinbutton_sprint_export_image_width.Value = spinbutton_force_sensor_export_image_width.Value;
		spinbutton_sprint_export_image_height.Value = spinbutton_force_sensor_export_image_height.Value;

		spinbutton_run_encoder_export_image_width.Value = spinbutton_force_sensor_export_image_width.Value;
		spinbutton_run_encoder_export_image_height.Value = spinbutton_force_sensor_export_image_height.Value;


		forceSensorExport = new ForceSensorExport (
				current_mode,
				notebook_force_sensor_export,
				progressbar_force_sensor_export,
				label_force_sensor_export_result,
				check_force_sensor_export_images.Active,
				Convert.ToInt32(spinbutton_force_sensor_export_image_width.Value),
				Convert.ToInt32(spinbutton_force_sensor_export_image_height.Value),
				UtilAll.IsWindows(), personID, sessionID,
				rfdList, impulse,//getImpulseValue(),
				duration, Convert.ToInt32(spin_force_rfd_duration_percent.Value),
				preferences.forceSensorElasticEccMinDispl,
				preferences.forceSensorNotElasticEccMinForce,
				preferences.forceSensorElasticConMinDispl,
				preferences.forceSensorNotElasticConMinForce,
				preferences.forceSensorStartEndOptimized,
				preferences.CSVExportDecimalSeparatorChar, 	//decimalIsPointAtExport (write)
				preferences.forceSensorAnalyzeMaxAVGInWindow
				);

		forceSensorExport.Button_done.Clicked -= new EventHandler(force_sensor_export_done);
		forceSensorExport.Button_done.Clicked += new EventHandler(force_sensor_export_done);

		bool selectedFile = false;
		if(check_force_sensor_export_images.Active)
		{
			if(personID == -1)
				selectedFile = checkFolder (Constants.CheckFileOp.FORCESENSOR_EXPORT_GROUPAL_CURRENT_SESSION_YES_IMAGES);
			else if (sessionID == -1)
				selectedFile = checkFolder (Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_ALL_SESSIONS_YES_IMAGES);
			else
				selectedFile = checkFolder (Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_CURRENT_SESSION_YES_IMAGES);
		} else {
			if(personID == -1)
				selectedFile = checkFile (Constants.CheckFileOp.FORCESENSOR_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES);
			else if (sessionID == -1)
				selectedFile = checkFile (Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES);
			else
				selectedFile = checkFile (Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES);
		}

		//restore the gui if cancelled
		if(! selectedFile) {
			forceSensorButtonsSensitive(true);
			hbox_force_sensor_analyze_top_modes.Sensitive = true;
			button_force_sensor_analyze_options.Sensitive = true;
			hbox_force_sensor_export_images.Sensitive = true;
		}
	}
	private void on_button_force_sensor_export_file_selected (string selectedFileName)
	{
		forceSensorExport.Start(selectedFileName); //file or folder
	}

	private void on_button_force_sensor_export_cancel_clicked (object o, EventArgs args)
	{
		forceSensorExport.Cancel();
	}

	private void force_sensor_export_done (object o, EventArgs args)
	{
		forceSensorExport.Button_done.Clicked -= new EventHandler(force_sensor_export_done);

		forceSensorButtonsSensitive(true);
		hbox_force_sensor_analyze_top_modes.Sensitive = true;
		button_force_sensor_analyze_options.Sensitive = true;
		hbox_force_sensor_export_images.Sensitive = true;

		if(forceSensorExport != null && forceSensorExport.AllOk)
			button_force_sensor_export_result_open.Visible = true;
	}

	private void on_button_force_sensor_export_result_open_clicked (object o, EventArgs args)
	{
		if(forceSensorExport == null || forceSensorExport.ExportURL == "")
		{
			new DialogMessage(Constants.MessageTypes.WARNING,
					Constants.DirectoryCannotOpenStr());
			return;
		}

		if(! Util.OpenURL (forceSensorExport.ExportURL))
			new DialogMessage(Constants.MessageTypes.WARNING,
					Constants.DirectoryCannotOpenStr() + "\n\n" + forceSensorExport.ExportURL);
	}

	private void forceSensorDoGraphAI(bool windowResizedAndZoom) //TODO: note true is never sent
	{
		if(lastForceSensorFullPath == null || lastForceSensorFullPath == "")
			return;

		int zoomFrameA = -1; //means no zoom
		int zoomFrameB = -1; //means no zoom

		Gtk.HScale hsLeft = getHScaleABCD (true);
		Gtk.HScale hsRight = getHScaleABCD (false);

		if(windowResizedAndZoom)
		{
			if (radio_force_sensor_ai_ab.Active)
			{
				//like this works but cannot calculate the RFD of A,B
				zoomFrameA = hscale_force_sensor_ai_a_BeforeZoom;
				zoomFrameB = hscale_force_sensor_ai_b_BeforeZoom;
				//zoomFrameA = hscale_force_sensor_ai_a_BeforeZoom -1;
				//zoomFrameB = hscale_force_sensor_ai_b_BeforeZoom +1;
			} else
			{
				zoomFrameA = hscale_force_sensor_ai_c_BeforeZoom;
				zoomFrameB = hscale_force_sensor_ai_d_BeforeZoom;
			}
		}
		else if ( forceSensorZoomApplied && (
				(radio_force_sensor_ai_ab.Active &&
				Util.IsNumber (tvFS_AB.TimeStart, true) &&
				Util.IsNumber (tvFS_AB.TimeEnd, true)) ||
				(! radio_force_sensor_ai_ab.Active &&
				Util.IsNumber (tvFS_CD.TimeStart, true) &&
				Util.IsNumber (tvFS_CD.TimeEnd, true)) ))
		{
			//invert hscales if needed
			int firstValue = Convert.ToInt32 (hsLeft.Value);
			int secondValue = Convert.ToInt32 (hsRight.Value);
			//LogB.Information(string.Format("firstValue: {0}, secondValue: {1}", firstValue, secondValue));

			//note that is almost impossible in the ui, but just in case...
			if(firstValue > secondValue) {
				int temp = firstValue;
				firstValue = secondValue;
				secondValue = temp;
			}

			//-1 and +1 to have the points at the edges to calcule the RFDs
			//like this works but cannot calculate the RFD of A,B
			zoomFrameA = firstValue;
			zoomFrameB = secondValue;;
			//zoomFrameA = firstValue -1;
			//zoomFrameB = secondValue +1;

			//do not zoom if both are the same, or the diff is just on pixel
			if(Math.Abs(zoomFrameA - zoomFrameB) <= 1)
			{
				zoomFrameA = -1;
				zoomFrameB = -1;
			}
		}

		//pass them as doubles
		double eccMinDispl = currentForceSensorExercise.GetEccOrConMinMaybePreferences(true,
				preferences.forceSensorElasticEccMinDispl,
				preferences.forceSensorNotElasticEccMinForce);
		double conMinDispl = currentForceSensorExercise.GetEccOrConMinMaybePreferences(false,
				preferences.forceSensorElasticConMinDispl,
				preferences.forceSensorNotElasticConMinForce);
		LogB.Information(string.Format("eccMinDispl: {0}, conMinDispl: {1}", eccMinDispl, conMinDispl));

		//LogB.Information(string.Format("creating fsAI with zoomFrameA: {0}, zoomFrameB: {1}", zoomFrameA, zoomFrameB));
		fsAI_AB = new ForceSensorAnalyzeInstant(
				"AB",
				lastForceSensorFullPath,
				zoomFrameA, zoomFrameB,
				currentForceSensorExercise, currentPersonSession.Weight,
				getForceSensorCaptureOptions(), currentForceSensor.Stiffness,
				eccMinDispl, conMinDispl
				);
		fsAI_CD = new ForceSensorAnalyzeInstant(
				"CD",
				lastForceSensorFullPath,
				zoomFrameA, zoomFrameB, //TODO: check zoomz for CD
				currentForceSensorExercise, currentPersonSession.Weight,
				getForceSensorCaptureOptions(), currentForceSensor.Stiffness,
				eccMinDispl, conMinDispl
				);
		//LogB.Information("created fsAI");
		//LogB.Information(string.Format("fsAI.GetLength: {0}", fsAI.GetLength()));

		/*
		 * position the hscales on the left to avoid loading a csv
		 * with less data rows than current csv and having scales out of the graph
		//hscale_force_sensor_ai_a.ValuePos = Gtk.PositionType.Left; //doesn't work
		//hscale_force_sensor_ai_b.ValuePos = Gtk.PositionType.Left; //doesn't work
		*/
		hscale_force_sensor_ai_a.Value = 0;
		hscale_force_sensor_ai_b.Value = 0;
		hscale_force_sensor_ai_c.Value = 0;
		hscale_force_sensor_ai_d.Value = 0;

		//ranges should have max value the number of the lines of csv file minus the header
		//this applies to the four hscales
		hscale_force_sensor_ai_a.SetRange (0, fsAI_AB.GetLength() -1);
		hscale_force_sensor_ai_b.SetRange (0, fsAI_AB.GetLength() -1);
		hscale_force_sensor_ai_c.SetRange (0, fsAI_CD.GetLength() -1);
		hscale_force_sensor_ai_d.SetRange (0, fsAI_CD.GetLength() -1);
		//set them to 0, because if not is set to 1 by a GTK error
		hscale_force_sensor_ai_a.Value = 0;
		hscale_force_sensor_ai_b.Value = 0;
		hscale_force_sensor_ai_c.Value = 0;
		hscale_force_sensor_ai_d.Value = 0;

		//LogB.Information(string.Format("hscale_force_sensor_ai_time_a,b,ab ranges: 0, {0}", fsAI.GetLength() -1));

		//on zoom put hscale B at the right
		if(zoomFrameB >= 0)
		{
			hsRight.Value = fsAI_AB.GetLength() -1; //TODO: fix for D
			LogB.Information ("hsRight.Value: " + hsRight.Value.ToString ());
			LogB.Information ("hscale_force_sensor_ai_b.Value: " + hscale_force_sensor_ai_b.Value.ToString ());
		}

		//to update values
		if (radio_force_sensor_ai_ab.Active)
			on_hscale_force_sensor_ai_value_changed (hscale_force_sensor_ai_a, new EventArgs ());
		else
			on_hscale_force_sensor_ai_value_changed (hscale_force_sensor_ai_c, new EventArgs ());

		manage_force_sensor_ai_table_visibilities();
	}

	private void on_check_force_sensor_analyze_show_magnitudes (object o, EventArgs args)
	{
		if (fsMagnitudesSignalsNoFollow)
			return;

		force_sensor_ai_drawingarea_cairo.QueueDraw ();

		//sync with capture magnitudes
		fsMagnitudesSignalsNoFollow = true;

		if (check_force_sensor_analyze_show_distance.Active != check_force_sensor_capture_show_distance.Active)
			check_force_sensor_capture_show_distance.Active = check_force_sensor_analyze_show_distance.Active;
		if (check_force_sensor_analyze_show_speed.Active != check_force_sensor_capture_show_speed.Active)
			check_force_sensor_capture_show_speed.Active = check_force_sensor_analyze_show_speed.Active;
		if (check_force_sensor_analyze_show_power.Active != check_force_sensor_capture_show_power.Active)
			check_force_sensor_capture_show_power.Active = check_force_sensor_analyze_show_power.Active;

		fsMagnitudesSignalsNoFollow = false;
	}

	CairoGraphForceSensorAI cairoGraphForceSensorAI;
	public void on_force_sensor_ai_drawingarea_cairo_draw (object o, Gtk.DrawnArgs args)
	{
		updateForceSensorAICairo (true);
	}

	private void updateForceSensorAICairo (bool forceRedraw)
	{
		if (cairoGraphForceSensorAI == null)
			cairoGraphForceSensorAI = new CairoGraphForceSensorAI (
					force_sensor_ai_drawingarea_cairo, "title");

		int rectangleN = 0;
		int rectangleRange = 0;
		if(preferences.forceSensorCaptureFeedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.RECTANGLE)
		{
			rectangleN = preferences.forceSensorCaptureFeedbackAt;
			rectangleRange = preferences.forceSensorCaptureFeedbackRange;
		}

		if (spCairoFE == null)
			spCairoFE = new SignalPointsCairoForceElastic ();

		List<GetMaxAvgInWindow> gmaiw_l = new List<GetMaxAvgInWindow> ();
		List<GetBestRFDInWindow> briw_l = new List<GetBestRFDInWindow> ();
		List<ForceSensorRepetition> reps_l = new List<ForceSensorRepetition> ();

		List<ForceSensorAnalyzeInstant> fsAI_l = new List <ForceSensorAnalyzeInstant>() { fsAI_AB, fsAI_CD };
		int count = 0;
		foreach (ForceSensorAnalyzeInstant fsAI in fsAI_l)
		{
			if (fsAI != null)
			{
				if (fsAI.Gmaiw != null && fsAI.Gmaiw.Error == "")
					gmaiw_l.Add (fsAI.Gmaiw);
				else
					gmaiw_l.Add (new GetMaxAvgInWindow ());

				//TODO: think on List of reps_l for when we have two different samples
				if (count == 0)
				{
					reps_l = fsAI.ForceSensorRepetition_l;
					if(forceSensorZoomApplied)
						reps_l = forceSensorRepetition_lZoomAppliedCairo;
				}

				if (fsAI.Briw == null)
					briw_l.Add (new GetBestRFDInWindow (new List<PointF>(), 0, 0, 1));
				else
					briw_l.Add (fsAI.Briw);
			}
			else {
				gmaiw_l.Add (new GetMaxAvgInWindow ());
				briw_l.Add (new GetBestRFDInWindow (new List<PointF>(), 0, 0, 1));
			}

			count ++;
		}

		int hscaleABSampleStart = Convert.ToInt32 (hscale_force_sensor_ai_a.Value);
		int hscaleABSampleEnd = Convert.ToInt32 (hscale_force_sensor_ai_b.Value);
		int hscaleCDSampleStart = Convert.ToInt32 (hscale_force_sensor_ai_c.Value);
		int hscaleCDSampleEnd = Convert.ToInt32 (hscale_force_sensor_ai_d.Value);

		// no need of copy because this graph is done on load or at end of capture (points_list does not grow in other thread
		// spCairoFESend is not a copy, is just to choose between zoomed or not
		SignalPointsCairoForceElastic spCairoFESend;
		if (forceSensorZoomApplied)
			spCairoFESend = spCairoFEZoom;
		else
			spCairoFESend = spCairoFE;

		double tempMinY = PointF.GetMinY (spCairoFESend.Force_l);
		double tempMaxY = PointF.GetMaxY (spCairoFESend.Force_l);
		int minY = Convert.ToInt32 (tempMinY -.05 *(tempMaxY - tempMinY));
		int maxY = Convert.ToInt32 (tempMinY +.05 *(tempMaxY - tempMinY));
		if (spCairoFESend.Displ_l != null && spCairoFESend.Displ_l.Count > 0)
		{
			minY = 0;
			maxY = 0;
		}

		fsAIRepetitionMouseLimitsCairo = cairoGraphForceSensorAI.DoSendingList (
				preferences.fontType.ToString(),
				spCairoFESend, //now send this, in the future send List of this to superpose curves
				check_force_sensor_analyze_show_distance.Active,
				check_force_sensor_analyze_show_speed.Active,
				check_force_sensor_analyze_show_power.Active,
				minY, maxY,
				rectangleN, rectangleRange,
				briw_l,
				triggerListForceSensor,
				hscaleABSampleStart, hscaleABSampleEnd,
				hscaleCDSampleStart, hscaleCDSampleEnd,
				forceSensorZoomApplied,
				gmaiw_l,
				currentForceSensorExercise, reps_l,
				forceRedraw, CairoXY.PlotTypes.LINES);
	}

	private void on_force_sensor_ai_drawingarea_cairo_button_press_event (object o, ButtonPressEventArgs args)
	{
		//LogB.Information(string.Format("Mouse X: {0}; Mouse Y: {1}", args.Event.X, args.Event.Y));

		//if zoomed: unzoom and return
		if(forceSensorZoomApplied)
		{
			check_force_sensor_ai_zoom.Click();
			return;
		}

		//if list exists, select the repetition
		if (fsAIRepetitionMouseLimitsCairo == null)
			return;

		int repetition = fsAIFindBarInPixelCairo (args.Event.X);
		LogB.Information("Repetition: " + repetition.ToString());
		if(repetition < 0)
			return;

		if (radio_force_sensor_ai_ab.Active)
		{
			hscale_force_sensor_ai_a.Value = fsAIRepetitionMouseLimitsCairo.GetSampleStartOfARep (repetition);
			hscale_force_sensor_ai_b.Value = fsAIRepetitionMouseLimitsCairo.GetSampleEndOfARep (repetition);
		} else
		{
			hscale_force_sensor_ai_c.Value = fsAIRepetitionMouseLimitsCairo.GetSampleStartOfARep (repetition);
			hscale_force_sensor_ai_d.Value = fsAIRepetitionMouseLimitsCairo.GetSampleEndOfARep (repetition);
		}
	}

	private bool forceSensorZoomApplied;
	private List<ForceSensorRepetition> forceSensorRepetition_lZoomAppliedCairo;
	private void forceSensorZoomDefaultValues()
	{
		forceSensorZoomApplied = false;
		check_force_sensor_ai_zoom.Active = false;
	}

	private int hscale_force_sensor_ai_a_BeforeZoom = 0;
	private int hscale_force_sensor_ai_a_AtZoom = 0;
	private int hscale_force_sensor_ai_b_BeforeZoom = 0;
	private int hscale_force_sensor_ai_b_AtZoom = 0;
	private int hscale_force_sensor_ai_c_BeforeZoom = 0;
	private int hscale_force_sensor_ai_c_AtZoom = 0;
	private int hscale_force_sensor_ai_d_BeforeZoom = 0;
	private int hscale_force_sensor_ai_d_AtZoom = 0;

	//private double hscale_force_sensor_ai_a_BeforeZoomTimeMS = 0; //to calculate triggers

	private void on_check_force_sensor_ai_zoom_clicked (object o, EventArgs args)
	{
		ForceSensorAnalyzeInstant fsAI = getCorrectfsAI ();

		if(fsAI == null || fsAI.GetLength() == 0)
			return;

		if(check_force_sensor_ai_zoom.Active)
		{
			forceSensorZoomApplied = true;

			//store hscale a to help return to position on unzoom
			hscale_force_sensor_ai_a_BeforeZoom = Convert.ToInt32 (hscale_force_sensor_ai_a.Value);
			hscale_force_sensor_ai_b_BeforeZoom = Convert.ToInt32 (hscale_force_sensor_ai_b.Value);
			hscale_force_sensor_ai_c_BeforeZoom = Convert.ToInt32 (hscale_force_sensor_ai_c.Value);
			hscale_force_sensor_ai_d_BeforeZoom = Convert.ToInt32 (hscale_force_sensor_ai_d.Value);

			if (radio_force_sensor_ai_ab.Active)
				spCairoFEZoom = new SignalPointsCairoForceElastic (spCairoFE,
						hscale_force_sensor_ai_a_BeforeZoom, hscale_force_sensor_ai_b_BeforeZoom);
			else
				spCairoFEZoom = new SignalPointsCairoForceElastic (spCairoFE,
						hscale_force_sensor_ai_c_BeforeZoom, hscale_force_sensor_ai_d_BeforeZoom);

			//cairo
			forceSensorRepetition_lZoomAppliedCairo = new List<ForceSensorRepetition> ();
			for (int r = 0; r < fsAI.ForceSensorRepetition_l.Count; r ++)
			{
				// don't do like this until delete non-cairo because this changes the fsAI.ForceSensorRepetition_l values and non-cairo is not displayed correctly
				//ForceSensorRepetition fsr = fsAI.ForceSensorRepetition_l[r];
				// do this:
				ForceSensorRepetition fsr = fsAI.ForceSensorRepetition_l[r].Clone();

				fsr.sampleStart -= hscale_force_sensor_ai_a_BeforeZoom;
				fsr.sampleEnd -= hscale_force_sensor_ai_a_BeforeZoom;

				forceSensorRepetition_lZoomAppliedCairo.Add (fsr);
			}

			forceSensorDoGraphAI(false);

			image_force_sensor_ai_zoom.Visible = false;
			image_force_sensor_ai_zoom_out.Visible = true;

		} else {
			forceSensorZoomApplied = false;

			if (radio_force_sensor_ai_ab.Active)
			{
				hscale_force_sensor_ai_a_AtZoom = Convert.ToInt32 (hscale_force_sensor_ai_a.Value);
				hscale_force_sensor_ai_b_AtZoom = Convert.ToInt32 (hscale_force_sensor_ai_b.Value);
			} else {
				hscale_force_sensor_ai_c_AtZoom = Convert.ToInt32 (hscale_force_sensor_ai_c.Value);
				hscale_force_sensor_ai_d_AtZoom = Convert.ToInt32 (hscale_force_sensor_ai_d.Value);
			}

			forceSensorDoGraphAI(false);

			if (radio_force_sensor_ai_ab.Active)
			{
				// set hscales a,b to value before + value at zoom (because user maybe changed it on zoom)
				hscale_force_sensor_ai_a.Value = hscale_force_sensor_ai_a_BeforeZoom +
					(hscale_force_sensor_ai_a_AtZoom);
				hscale_force_sensor_ai_b.Value = hscale_force_sensor_ai_a_BeforeZoom +
					(hscale_force_sensor_ai_b_AtZoom);

				// set hscales c,d at same value before zoom
				hscale_force_sensor_ai_c.Value = hscale_force_sensor_ai_c_BeforeZoom;
				hscale_force_sensor_ai_d.Value = hscale_force_sensor_ai_d_BeforeZoom;
			} else {
				hscale_force_sensor_ai_a.Value = hscale_force_sensor_ai_a_BeforeZoom;
				hscale_force_sensor_ai_b.Value = hscale_force_sensor_ai_b_BeforeZoom;

				hscale_force_sensor_ai_c.Value = hscale_force_sensor_ai_c_BeforeZoom +
					(hscale_force_sensor_ai_c_AtZoom);
				hscale_force_sensor_ai_d.Value = hscale_force_sensor_ai_c_BeforeZoom +
					(hscale_force_sensor_ai_d_AtZoom);
			}

			image_force_sensor_ai_zoom.Visible = true;
			image_force_sensor_ai_zoom_out.Visible = false;
		}
	}

	private void forceSensorAnalyzeManualGraphDo(Rectangle allocation)
	{
	}

	private int fsAIFindBarInPixel (double pixel)
	{
		if(fsAIRepetitionMouseLimits == null)
			return -1;

		return fsAIRepetitionMouseLimits.FindBarInPixel(pixel);
	}
	private int fsAIFindBarInPixelCairo (double pixel)
	{
		if(fsAIRepetitionMouseLimitsCairo == null)
			return -1;

		return fsAIRepetitionMouseLimitsCairo.FindBarInPixel (pixel);
	}

	private void on_radio_force_sensor_ai_abcd_toggled (object o, EventArgs args)
	{
		if (o == null || ! ((Gtk.RadioButton) o).Active)
			return;

		if ((Gtk.RadioButton) o == radio_force_sensor_ai_ab)
		{
			box_force_sensor_ai_a.Visible = true;
			box_force_sensor_ai_b.Visible = true;
			box_force_sensor_ai_c.Visible = false;
			box_force_sensor_ai_d.Visible = false;
			label_force_sensor_ai_zoom_abcd.Text = "[A-B]";
			UtilGtk.ViewportColor (viewport_force_sensor_analyze_hscales, UtilGtk.Colors.YELLOW_LIGHT);
		}
		else if ((Gtk.RadioButton) o == radio_force_sensor_ai_cd)
		{
			box_force_sensor_ai_a.Visible = false;
			box_force_sensor_ai_b.Visible = false;
			box_force_sensor_ai_c.Visible = true;
			box_force_sensor_ai_d.Visible = true;
			label_force_sensor_ai_zoom_abcd.Text = "[C-D]";
			UtilGtk.ViewportColor (viewport_force_sensor_analyze_hscales, UtilGtk.Colors.GREEN_LIGHT);
		}

		forceSensorAnalyzeGeneralButtonHscaleZoomSensitiveness();
	}

	private Gtk.HScale getHScaleABCD (bool left)
	{
		if (radio_force_sensor_ai_ab.Active)
		{
			if (left)
				return hscale_force_sensor_ai_a;
			else
				return hscale_force_sensor_ai_b;
		} else
		{
			if (left)
				return hscale_force_sensor_ai_c;
			else
				return hscale_force_sensor_ai_d;
		}
	}

	private ForceSensorAnalyzeInstant getCorrectfsAI ()
	{
		if (radio_force_sensor_ai_ab.Active)
			return fsAI_AB;
		else
			return fsAI_CD;
	}

	private void on_check_force_sensor_ai_chained_load_link_clicked (object o, EventArgs args)
	{
		if (check_force_sensor_ai_chained_load_link.Active)
		{
			image_force_sensor_ai_chained_load_link.Pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "link_vertical.png");
			button_force_sensor_analyze_load.Visible = true;
			box_buttons_force_sensor_analyze_load_unchained.Visible = false;
		} else {
			image_force_sensor_ai_chained_load_link.Pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "link_vertical_off.png");
			button_force_sensor_analyze_load.Visible = false;
			box_buttons_force_sensor_analyze_load_unchained.Visible = true;
		}
	}

	bool forceSensorHScalesDoNotFollow = false;
	//to know change of slider in order to apply on the other slider if chained
	int force_sensor_last_a = 1;
	int force_sensor_last_b = 1;
	int force_sensor_last_c = 1;
	int force_sensor_last_d = 1;

	private void on_hscale_force_sensor_ai_value_changed (object o, EventArgs args)
	{
		LogB.Information ("on_hscale_force_sensor_ai_value_changed");
		ForceSensorAnalyzeInstant fsAI = getCorrectfsAI ();

		if(fsAI == null || fsAI.GetLength() == 0)
			return;
		Gtk.HScale hs = (Gtk.HScale) o;

		hscale_force_sensor_ai_value_changed_do (fsAI, hs);
	}

	//can be convinient to call it directly
	private void hscale_force_sensor_ai_value_changed_do (ForceSensorAnalyzeInstant fsAI, HScale hs)
	{
		// 1. set some variables to make this function work for the four hscales
		bool isAB; //false AC
		bool isLeft; //A or C
		Gtk.HScale hsRelated ; //if A then B, if D then C
		int last;
		string hscaleToDebug;
		TreeviewFSAnalyze tvFS;

		if (hs == hscale_force_sensor_ai_a)
		{
			tvFS = tvFS_AB;
			isAB = true; //false AC
			isLeft = true; //A or C
			hsRelated = hscale_force_sensor_ai_b; //if A then B, if D then C
			last = force_sensor_last_a;
			hscaleToDebug = "--- hscale_a ---";
		}
		else if (hs == hscale_force_sensor_ai_b)
		{
			tvFS = tvFS_AB;
			isAB = true;
			isLeft = false;
			hsRelated = hscale_force_sensor_ai_a;
			last = force_sensor_last_b;
			hscaleToDebug = "--- hscale_b ---";
		}
		else if (hs == hscale_force_sensor_ai_c)
		{
			tvFS = tvFS_CD;
			isAB = false;
			isLeft = true;
			hsRelated = hscale_force_sensor_ai_d;
			last = force_sensor_last_c;
			hscaleToDebug = "--- hscale_c ---";
		}
		else //if (hs == hscale_force_sensor_ai_d)
		{
			tvFS = tvFS_CD;
			isAB = false;
			isLeft = false;
			hsRelated = hscale_force_sensor_ai_c;
			last = force_sensor_last_d;
			hscaleToDebug = "--- hscale_d ---";
		}

		int diffWithPrevious = Convert.ToInt32 (hs.Value) - last;

		//if chained and moving a to the right makes b higher than 1, do not move
		if (check_force_sensor_ai_chained_hscales.Active)
		{
			if (
					(isAB && isLeft && Convert.ToInt32 (hsRelated.Value) + diffWithPrevious >= fsAI.GetLength() -1) ||
					(isAB && ! isLeft && Convert.ToInt32 (hsRelated.Value) + diffWithPrevious <= 0) ||
					(! isAB && isLeft && Convert.ToInt32 (hsRelated.Value) + diffWithPrevious >= fsAI.GetLength() -1) ||
					(! isAB && ! isLeft && Convert.ToInt32 (hsRelated.Value) + diffWithPrevious <= 0) )
			{
				hs.Value = last;
				return;
			}
		}
		//LogB.Information (string.Format ("on_hscale_force_sensor_ai_value_changed {0} 2", hscaleToDebug));

		//do not allow A or C to be higher than B or D (fix multiple possible problems)
		if (isLeft && hs.Value > hsRelated.Value)
			hsRelated.Value = hs.Value;
		else if (! isLeft && hs.Value < hsRelated.Value)
			hs.Value = hsRelated.Value;

		//fix possible boundaries problem that could happen when there is really few data
		int count = Convert.ToInt32 (hs.Value);
		int countRelated = Convert.ToInt32 (hsRelated.Value);
		if (
				(count > 0 && count > fsAI.GetLength() -1) ||
				(countRelated > 0 && countRelated > fsAI.GetLength() -1) )
		{
			LogB.Information (string.Format ("hscale_force_sensor outside of boundaries (isAB: {0}, isLeft: {1}, count: {2}, countRelated: {3}, fsAI.GetLength (): {4})",
						isAB, isLeft, count, countRelated, fsAI.GetLength ()));
			return;
		}
		//LogB.Information (string.Format ("on_hscale_force_sensor_ai_value_changed {0} 3", hscaleToDebug));

		int countLeft = count;
		int countRight = countRelated;
		if (! isLeft)
		{
			countLeft = countRelated;
			countRight = count;
		}

		string rfd = "";
		if(count > 0 && count < fsAI.GetLength() -1)
			rfd = Math.Round(fsAI.CalculateRFD(count -1, count +1), 1).ToString();

		string position = "";
		string speed = "";
		string accel = "";
		string power = "";
		LogB.Information ("at hscales fsAI.CalculedElasticPSAP: " + fsAI.CalculedElasticPSAP.ToString ());
		if(fsAI.CalculedElasticPSAP)
		{
			position = Math.Round (fsAI.Position_l[count], 3).ToString();
			speed = Math.Round (fsAI.Speed_l[count], 3).ToString();
			accel = Math.Round (fsAI.Accel_l[count], 3).ToString();
			power = Math.Round (fsAI.Power_l[count], 3).ToString();
		}

		tvFS.ResetTreeview ();
		tvFS.PassRow1or2 (isLeft, Math.Round(fsAI.GetTimeMS(count), 1).ToString(), fsAI.GetForceAtCount (count), rfd);
		if (current_mode == Constants.Modes.FORCESENSORELASTIC)
			tvFS.PassRow1or2Elastic (isLeft, position, speed, accel, power);

		//LogB.Information (string.Format ("on_hscale_force_sensor_ai_value_changed {0} 4", hscaleToDebug));
		//LogB.Information (string.Format ("on_hscale_force_sensor_ai_value_changed {0} 5", hscaleToDebug));
		// update force_sensor_last_a, ...
		if (hs == hscale_force_sensor_ai_a)
			force_sensor_last_a = Convert.ToInt32 (hs.Value);
		else if (hs == hscale_force_sensor_ai_b)
			force_sensor_last_b = Convert.ToInt32 (hs.Value);
		else if (hs == hscale_force_sensor_ai_c)
			force_sensor_last_c = Convert.ToInt32 (hs.Value);
		else if (hs == hscale_force_sensor_ai_d)
			force_sensor_last_d = Convert.ToInt32 (hs.Value);

		//LogB.Information (string.Format ("on_hscale_force_sensor_ai_value_changed {0} 6", hscaleToDebug));
		if (forceSensorHScalesDoNotFollow)
		{
			forceSensorHScalesDoNotFollow = false;
			tvFS.ResetTreeview (); //To avoid duplicated rows on chained A,B
			tvFS.FillTreeview ();

			return;
		}

		//LogB.Information (string.Format ("on_hscale_force_sensor_ai_value_changed {0} 7", hscaleToDebug));
		//if chained move also the related
		if (check_force_sensor_ai_chained_hscales.Active)
		{
			forceSensorHScalesDoNotFollow = true;
			hsRelated.Value = hsRelated.Value + diffWithPrevious;
			forceSensorHScalesDoNotFollow = false;
		}

		//need to do both to ensure at unzoom params are calculated for AB and CD
		force_sensor_analyze_instant_calculate_params (fsAI_AB, tvFS_AB, true,
				Convert.ToInt32 (hscale_force_sensor_ai_a.Value),
				Convert.ToInt32 (hscale_force_sensor_ai_b.Value));
		force_sensor_analyze_instant_calculate_params (fsAI_CD, tvFS_CD, false,
				Convert.ToInt32 (hscale_force_sensor_ai_c.Value),
				Convert.ToInt32 (hscale_force_sensor_ai_d.Value));

		tvFS.ResetTreeview (); //To avoid duplicated rows on chained A,B
		tvFS.FillTreeview ();

		forceSensorAnalyzeGeneralButtonHscaleZoomSensitiveness();
		force_sensor_ai_drawingarea_cairo.QueueDraw(); //will fire ExposeEvent
		LogB.Information (string.Format ("on_hscale_force_sensor_ai_value_changed {0} 8", hscaleToDebug));
	}

	private void on_check_force_sensor_ai_chained_hscales_clicked (object o, EventArgs args)
	{
		image_force_sensor_ai_chained_hscales_link.Visible = check_force_sensor_ai_chained_hscales.Active;
		image_force_sensor_ai_chained_hscales_link_off.Visible = ! check_force_sensor_ai_chained_hscales.Active;
	}

	private void forceSensorAnalyzeGeneralButtonHscaleZoomSensitiveness()
	{
		Gtk.HScale hsLeft = getHScaleABCD (true);
		Gtk.HScale hsRight = getHScaleABCD (false);

		button_hscale_force_sensor_ai_a_first.Sensitive = hsLeft.Value > 0;
		button_hscale_force_sensor_ai_a_pre.Sensitive = hsLeft.Value > 0;
		button_hscale_force_sensor_ai_b_first.Sensitive = hsRight.Value > 0;
		button_hscale_force_sensor_ai_b_pre.Sensitive = hsRight.Value > 0;

		ForceSensorAnalyzeInstant fsAI = getCorrectfsAI ();
		button_hscale_force_sensor_ai_a_last.Sensitive = (fsAI != null && hsLeft.Value < fsAI.GetLength() -1);
		button_hscale_force_sensor_ai_a_post.Sensitive = (fsAI != null && hsLeft.Value < fsAI.GetLength() -1);
		button_hscale_force_sensor_ai_b_last.Sensitive = (fsAI != null && hsRight.Value < fsAI.GetLength() -1);
		button_hscale_force_sensor_ai_b_post.Sensitive = (fsAI != null && hsRight.Value < fsAI.GetLength() -1);

		//diff have to be more than one pixel
		check_force_sensor_ai_zoom.Sensitive = (Math.Abs(hsLeft.Value - hsRight.Value) > 1);
	}

	private void on_button_hscale_force_sensor_ai_a_first_clicked (object o, EventArgs args)
	{
		getHScaleABCD (true).Value = 0;
	}
	private void on_button_hscale_force_sensor_ai_a_pre_clicked (object o, EventArgs args)
	{
		HScale hs = getHScaleABCD (true);
		hs.Value -= 1;
	}
	private void on_button_hscale_force_sensor_ai_a_post_clicked (object o, EventArgs args)
	{
		HScale hs = getHScaleABCD (true);
		hs.Value += 1;
	}
	private void on_button_hscale_force_sensor_ai_a_last_clicked (object o, EventArgs args)
	{
		ForceSensorAnalyzeInstant fsAI = getCorrectfsAI ();
		if(fsAI == null || fsAI.GetLength() < 2)
			return;

		getHScaleABCD (true).Value = fsAI.GetLength() -1;
	}

	private void on_button_hscale_force_sensor_ai_b_first_clicked (object o, EventArgs args)
	{
		getHScaleABCD (false).Value = 0;
	}
	private void on_button_hscale_force_sensor_ai_b_pre_clicked (object o, EventArgs args)
	{
		HScale hs = getHScaleABCD (false);
		hs.Value -= 1;
	}
	private void on_button_hscale_force_sensor_ai_b_post_clicked (object o, EventArgs args)
	{
		HScale hs = getHScaleABCD (false);
		hs.Value += 1;
	}

	private void on_button_hscale_force_sensor_ai_b_last_clicked (object o, EventArgs args)
	{
		ForceSensorAnalyzeInstant fsAI = getCorrectfsAI ();
		if(fsAI == null || fsAI.GetLength() < 2)
			return;

		getHScaleABCD (false).Value = fsAI.GetLength() -1;
	}

	private void on_button_hscale_force_sensor_ai_a_pre_1s_clicked (object o, EventArgs args)
	{
		ForceSensorAnalyzeInstant fsAI = getCorrectfsAI ();
		if(fsAI == null || fsAI.GetLength() == 0)
			return;

		Gtk.HScale hs = getHScaleABCD (true);
		int startA = Convert.ToInt32 (hs.Value);
		double startAMs = fsAI.GetTimeMS(startA);
		for(int i = startA; i > 0; i --)
		{
			if(startAMs - fsAI.GetTimeMS(i) >= preferences.forceSensorAnalyzeABSliderIncrement * 1000)
			{
				//hscale_force_sensor_ai_a.Value += i - startA; is the sample where condition is done,
				//but maybe the sample before that condition is more close to 1s than this
				if(MathUtil.PassedSampleIsCloserToCriteria (
						startAMs - fsAI.GetTimeMS(i), startAMs - fsAI.GetTimeMS(i+1),
						preferences.forceSensorAnalyzeABSliderIncrement * 1000))
					hs.Value += (i - startA);
				else
					hs.Value += (i+1 - startA);

				return;
			}
		}
	}
	private void on_button_hscale_force_sensor_ai_a_post_1s_clicked (object o, EventArgs args)
	{
		ForceSensorAnalyzeInstant fsAI = getCorrectfsAI ();
		if(fsAI == null || fsAI.GetLength() == 0)
			return;

		Gtk.HScale hs = getHScaleABCD (true);
		int startA = Convert.ToInt32(hs.Value);
		double startAMs = fsAI.GetTimeMS(startA);
		for(int i = startA; i < fsAI.GetLength() -1; i ++)
		{
			if(fsAI.GetTimeMS(i) - startAMs >= preferences.forceSensorAnalyzeABSliderIncrement * 1000)
			{
				//hscale_force_sensor_ai_a.Value += i - startA;
				if(MathUtil.PassedSampleIsCloserToCriteria (
						fsAI.GetTimeMS(i) - startAMs, fsAI.GetTimeMS(i-1) - startAMs,
						preferences.forceSensorAnalyzeABSliderIncrement * 1000))
					hs.Value += (i - startA);
				else
					hs.Value += (i-1 - startA);

				return;
			}
		}
	}

	private void on_button_hscale_force_sensor_ai_b_pre_1s_clicked (object o, EventArgs args)
	{
		ForceSensorAnalyzeInstant fsAI = getCorrectfsAI ();
		if(fsAI == null || fsAI.GetLength() == 0)
			return;

		Gtk.HScale hs = getHScaleABCD (false);
		int startB = Convert.ToInt32(hs.Value);
		double startBMs = fsAI.GetTimeMS(startB);
		for(int i = startB; i > 0; i --)
		{
			if(startBMs - fsAI.GetTimeMS(i) >= preferences.forceSensorAnalyzeABSliderIncrement * 1000)
			{
				//hscale_force_sensor_ai_b.Value += i - startB;
				if(MathUtil.PassedSampleIsCloserToCriteria (
						startBMs - fsAI.GetTimeMS(i), startBMs - fsAI.GetTimeMS(i+1),
						preferences.forceSensorAnalyzeABSliderIncrement * 1000))
					hs.Value += (i - startB);
				else
					hs.Value += (i+1 - startB);

				return;
			}
		}
	}
	private void on_button_hscale_force_sensor_ai_b_post_1s_clicked (object o, EventArgs args)
	{
		ForceSensorAnalyzeInstant fsAI = getCorrectfsAI ();
		if(fsAI == null || fsAI.GetLength() == 0)
			return;

		Gtk.HScale hs = getHScaleABCD (false);
		int startB = Convert.ToInt32(hs.Value);
		double startBMs = fsAI.GetTimeMS(startB);
		for(int i = startB; i < fsAI.GetLength() -1; i ++)
		{
			if(fsAI.GetTimeMS(i) - startBMs >= preferences.forceSensorAnalyzeABSliderIncrement * 1000)
			{
				//hscale_force_sensor_ai_b.Value += i - startB;
				if(MathUtil.PassedSampleIsCloserToCriteria (
						fsAI.GetTimeMS(i) - startBMs, fsAI.GetTimeMS(i-1) - startBMs,
						preferences.forceSensorAnalyzeABSliderIncrement * 1000))
					hs.Value += (i - startB);
				else
					hs.Value += (i-1 - startB);

				return;
			}
		}
	}

	private void manage_force_sensor_ai_table_visibilities()
	{
		bool visible = true;//checkbutton_force_sensor_ai_b.Active;

		ForceSensorAnalyzeInstant fsAI = getCorrectfsAI ();
		bool visibleElastic = (visible && fsAI.CalculedElasticPSAP);

		if (visible && canDoForceSensorAnalyzeAB ())
			button_force_sensor_analyze_AB_save.Visible = true;
		else
			button_force_sensor_analyze_AB_save.Visible = false;

		if (visible && canDoForceSensorAnalyzeCD ())
			button_force_sensor_analyze_CD_save.Visible = true;
		else
			button_force_sensor_analyze_CD_save.Visible = false;
	}

	private void force_sensor_analyze_instant_calculate_params (
			ForceSensorAnalyzeInstant fsAI, TreeviewFSAnalyze tvFS,	bool isAB, int countA, int countB)
	{
		//LogB.Information (string.Format ("before CalculateRangeParams 0 with fsAI.IdStr: {0}", fsAI.IdStr));
		if (countA < 0 || countA > fsAI.GetLength() -1 || countB < 0 || countB > fsAI.GetLength() -1)
			return;

		//LogB.Information (string.Format ("before CalculateRangeParams 1 with fsAI.IdStr: {0}", fsAI.IdStr));
		//old method
		double timeA = fsAI.GetTimeMS(countA);
		double timeB = fsAI.GetTimeMS(countB);
		double forceA = fsAI.GetForceAtCount(countA);
		double forceB = fsAI.GetForceAtCount(countB);
		bool success = fsAI.CalculateRangeParams(countA, countB, preferences.forceSensorAnalyzeMaxAVGInWindow);
		if(success) {
			tvFS.TimeDiff = Math.Round(timeB - timeA, 1).ToString();
			tvFS.ForceDiff = forceB - forceA;

			if(countA != countB) {
				tvFS.ForceAvg = Math.Round(fsAI.ForceAVG, 1).ToString();
				tvFS.ForceMax = Math.Round(fsAI.ForceMAX, 1).ToString();

				if(fsAI.Gmaiw.Error == "")
					tvFS_other.SetMaxAvgInWindow (Math.Round(fsAI.Gmaiw.Max, 1).ToString(), isAB);
				else
					tvFS_other.SetMaxAvgInWindow ("----", isAB);

				if(fsAI.Briw.Error == "")
					tvFS_other.SetBestRFDInWindow (Math.Round(fsAI.Briw.Max, 1).ToString(), isAB);
				else
					tvFS_other.SetBestRFDInWindow ("----", isAB);

				tvFS_other.ShowColumn (true, isAB);
			} else {
				tvFS.ForceAvg = "";
				tvFS.ForceMax = "";
				tvFS_other.ShowColumn (false, isAB);
			}
		} else {
			tvFS.TimeDiff = "";
			tvFS.ForceDiff = 0;
			tvFS_other.ShowColumn (false, isAB);
		}

		//LogB.Information (string.Format ("after CalculateRangeParams fsAI.IdStr: {0}, fsAI.Briw: {1}",
		//			fsAI.IdStr, fsAI.Briw));

		/*
		//new method
		LogB.Information ("fsAI timeA: " + timeA.ToString());
		LogB.Information ("p_l timeA: " + p_l[countA].X.ToString());

		double msA = p_l[countA].X / 1000.0;
		double msB = p_l[countB].X / 1000.0;
		forceA = p_l[countA].Y;
		forceB = p_l[countB].Y;

		ForceCalculateRange fcr;
		if (cairoGraphForceSensorSignalPointsDispl_l != null && cairoGraphForceSensorSignalPointsDispl_l.Count > 0)
			fcr = new ForceCalculateRange (p_l,
					cairoGraphForceSensorSignalPointsSpeed_l,
					cairoGraphForceSensorSignalPointsAccel_l,
					cairoGraphForceSensorSignalPointsPower_l,
					countA, countB, preferences.forceSensorAnalyzeMaxAVGInWindow);
		else
			fcr = new ForceCalculateRange (p_l, countA, countB, preferences.forceSensorAnalyzeMaxAVGInWindow);

		LogB.Information ("ai_time_diff: " + Math.Round(msB - msA, 1).ToString());
		LogB.Information ("ai_force_diff: " + Math.Round(forceB - forceA, 1).ToString());

		if(countA != countB) {
			//label_force_sensor_ai_force_average.Text = Math.Round(fsAI.ForceAVG, 1).ToString();
			//abel_force_sensor_ai_force_max.Text = Math.Round(fsAI.ForceMAX, 1).ToString();
			LogB.Information (string.Format ("ForceCalculateRange AVG: {0}, MAX: {1}", fcr.ForceAVG, fcr.ForceMAX));

			if (fcr.Gmaiw.Error == "")
			{
				//label_force_sensor_ai_max_avg_in_window_values.Text = Math.Round (fcr.Gmaiw.AvgMax, 1).ToString();
				LogB.Information ("InWindow: " + Math.Round (fcr.Gmaiw.AvgMax, 1).ToString());
			}
			else
			{
				//label_force_sensor_ai_max_avg_in_window_values.Text = "----";
				LogB.Information ("InWindow: ----");
			}
		} else {
			label_force_sensor_ai_force_average.Text = "";
			label_force_sensor_ai_force_max.Text = "";
			label_force_sensor_ai_max_avg_in_window_values.Text = "";
		}
		//end of new method
		*/

		if(fsAI.CalculedElasticPSAP && success) //success is always true
		{
			double positionA = fsAI.Position_l[countA];
			double positionB = fsAI.Position_l[countB];

			//LogB.Information ("fcr position_diff = " + Math.Round (cairoGraphForceSensorSignalPointsDispl_l[countB].Y - cairoGraphForceSensorSignalPointsDispl_l[countA].Y, 3).ToString());

			double speedA = fsAI.Speed_l[countA];
			double speedB = fsAI.Speed_l[countB];
			//LogB.Information ("fcr speed_diff = " + Math.Round (cairoGraphForceSensorSignalPointsSpeed_l[countB].Y - cairoGraphForceSensorSignalPointsSpeed_l[countA].Y, 3).ToString());

			if(countA != countB) {
				tvFS.PassElasticAvgs (
						Math.Round(fsAI.SpeedAVG, 3).ToString(),
						Math.Round(fsAI.AccelAVG, 3).ToString(),
						Math.Round(fsAI.PowerAVG, 3).ToString());
				tvFS.PassElasticMaxs (
						Math.Round(fsAI.SpeedMAX, 3).ToString(),
						Math.Round(fsAI.AccelMAX, 3).ToString(),
						Math.Round(fsAI.PowerMAX, 3).ToString());
			} else {
				tvFS.PassElasticAvgs ("", "", "");
				tvFS.PassElasticMaxs ("", "", "");
			}

			double accelA = fsAI.Accel_l[countA];
			double accelB = fsAI.Accel_l[countB];
			double powerA = fsAI.Power_l[countA];
			double powerB = fsAI.Power_l[countB];

			tvFS.PassElasticDiffs (
					Math.Round(positionB - positionA, 3).ToString(),
					Math.Round(speedB - speedA, 3).ToString(),
					Math.Round(accelB - accelA, 3).ToString(),
					Math.Round(powerB - powerA, 3).ToString());
		}
		else
			tvFS.PassElasticDiffs ("", "", "", "");

		/*
		//print the repetitions stuff
		LogB.Information("Printing repetitions:");
		foreach(ForceSensorRepetition fsr in fsAI.ForceSensorRepetition_l)
			LogB.Information(fsr.ToString());
		*/

		double rfdA = 0;
		double rfdB = 0;
		bool rfdADefined = false;
		bool rfdBDefined = false;
		if(countA > 0 && countA < fsAI.GetLength() -1)
		{
			rfdA = Math.Round(fsAI.CalculateRFD(countA -1, countA +1), 1);
			rfdADefined = true;
		}

		if(countB > 0 && countB < fsAI.GetLength() -1)
		{
			rfdB = Math.Round(fsAI.CalculateRFD(countB -1, countB +1), 1);
			rfdBDefined = true;
		}

		/* no need (I think) because it is already on on_hscale_force_sensor_ai_value_changed
		if(rfdADefined)
			label_force_sensor_ai_rfd_a.Text = rfdA.ToString();
		else
			label_force_sensor_ai_rfd_a.Text = "";

		if(rfdBDefined)
			label_force_sensor_ai_rfd_b.Text = rfdB.ToString();
		else
			label_force_sensor_ai_rfd_b.Text = "";
			*/

		if(rfdADefined && rfdBDefined && countA != countB)
		{
			// 0) invert counts if needed
			if(countA > countB)
			{
				int temp = countA;
				countA = countB;
				countB = temp;
			}

			// 1) diff
			tvFS.RfdDiff = rfdB - rfdA;

			// 2) Average:
			tvFS.RfdAvg = Math.Round(fsAI.CalculateRFD(countA, countB), 1).ToString();

			// 3) max
			fsAI.CalculateMaxRFDInRange(countA, countB);

			//LogB.Information(string.Format("fsAI.LastRFDMax: {0}", fsAI.LastRFDMax));
			//LogB.Information(string.Format("fsAI.LastRFDMaxCount: {0}", fsAI.LastRFDMaxCount));

			tvFS.RfdMax = Math.Round(fsAI.LastRFDMax, 1).ToString();
		} else {
			tvFS.RfdDiff = 0;
			tvFS.RfdAvg = "";
			tvFS.RfdMax = "";
		}

		tvFS_other.VariabilityMethod = preferences.forceSensorVariabilityMethod.ToString ();

		if(countA != countB)
		{
			// 10) calculate impulse
			//label_force_sensor_ai_impulse_values.Text = Math.Round (ForceCalcs.GetImpulse (
			//			p_l, countA, countB), 1).ToString();
			//	again this should be on fsAI:
			tvFS_other.SetImpulse (Math.Round (fsAI.CalculateImpulse (countA, countB), 1).ToString(), isAB);

			// 11) calculate variability
			int feedbackF = preferences.forceSensorCaptureFeedbackAt;

			fsAI.CalculateVariabilityAndAccuracy (countA, countB, feedbackF,
					preferences.forceSensorVariabilityMethod, preferences.forceSensorVariabilityLag);
			LogB.Information (string.Format ("vaa variability: {0}, feedbackDiff: {1}", fsAI.Vaa.Variability, fsAI.Vaa.FeedbackDiff));

			tvFS_other.SetVariability (Math.Round (fsAI.Vaa.Variability, 3).ToString(), isAB);

			// 12) calculate Accuracy (Feedback difference)
			//if(preferences.forceSensorCaptureFeedbackActive && feedbackF > 0)
			if(preferences.forceSensorCaptureFeedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.RECTANGLE && feedbackF > 0)
				tvFS_other.SetFeedback (Math.Round (fsAI.Vaa.FeedbackDiff, 3).ToString(), isAB);
			else
				tvFS_other.SetFeedback ("", isAB);
		}

		setForceSensorAnalyzeMaxAVGInWindow();
		tvFS_other.ResetTreeview ();
		tvFS_other.Fill ();
	}

	private bool canDoForceSensorAnalyzeAB()
	{
		return (Util.FileExists(lastForceSensorFullPath) && Util.IsNumber (tvFS_AB.TimeDiff, true));
	}
	private bool canDoForceSensorAnalyzeCD()
	{
		return (Util.FileExists(lastForceSensorFullPath) && Util.IsNumber (tvFS_CD.TimeDiff, true));
	}

	private void on_button_force_sensor_analyze_AB_save_clicked (object o, EventArgs args)
	{
		if (tvFS_AB.TimeStart == tvFS_AB.TimeEnd)
		{
			new DialogMessage (Constants.MessageTypes.WARNING, "A and B cannot be the same");
			return;
		}

		if (canDoForceSensorAnalyzeAB ())
			checkFile (Constants.CheckFileOp.FORCESENSOR_ANALYZE_SAVE_AB);
		else
			new DialogMessage (Constants.MessageTypes.WARNING, Constants.FileNotFoundStr());
	}
	private void on_button_force_sensor_analyze_CD_save_clicked (object o, EventArgs args)
	{
		if (tvFS_CD.TimeStart == tvFS_CD.TimeEnd)
		{
			new DialogMessage (Constants.MessageTypes.WARNING, "C and D cannot be the same");
			return;
		}

		if (canDoForceSensorAnalyzeCD ())
			checkFile (Constants.CheckFileOp.FORCESENSOR_ANALYZE_SAVE_CD);
		else
			new DialogMessage (Constants.MessageTypes.WARNING, Constants.FileNotFoundStr());
	}

	private void on_button_force_sensor_save_AB_file_selected (string selectedFileName)
	{
		fsAI_AB.ExportToCSV(
				getLowestForceSensorScale (hscale_force_sensor_ai_a, hscale_force_sensor_ai_b),
				getHighestForceSensorScale (hscale_force_sensor_ai_a, hscale_force_sensor_ai_b),
				selectedFileName, preferences.CSVExportDecimalSeparator);
	}
	private void on_button_force_sensor_save_CD_file_selected (string selectedFileName)
	{
		fsAI_CD.ExportToCSV(
				getLowestForceSensorScale (hscale_force_sensor_ai_c, hscale_force_sensor_ai_d),
				getHighestForceSensorScale (hscale_force_sensor_ai_c, hscale_force_sensor_ai_d),
				selectedFileName, preferences.CSVExportDecimalSeparator);
	}

	private int getLowestForceSensorScale (Gtk.HScale hsLeft, Gtk.HScale hsRight)
	{
		if(Convert.ToInt32(hsLeft.Value) <= Convert.ToInt32(hsRight.Value))
			return Convert.ToInt32(hsLeft.Value);
		else
			return Convert.ToInt32(hsRight.Value);
	}
	private int getHighestForceSensorScale (Gtk.HScale hsLeft, Gtk.HScale hsRight)
	{
		if(Convert.ToInt32(hsLeft.Value) <= Convert.ToInt32(hsRight.Value))
			return Convert.ToInt32(hsRight.Value);
		else
			return Convert.ToInt32(hsLeft.Value);
	}

	private void connectWidgetsForceSensorAnalyze (Gtk.Builder builder)
	{
		//analyze tab
		radio_force_sensor_analyze_individual_current_set = (Gtk.RadioButton) builder.GetObject ("radio_force_sensor_analyze_individual_current_set");
		radio_force_sensor_analyze_individual_current_session = (Gtk.RadioButton) builder.GetObject ("radio_force_sensor_analyze_individual_current_session");
		radio_force_sensor_analyze_individual_all_sessions = (Gtk.RadioButton) builder.GetObject ("radio_force_sensor_analyze_individual_all_sessions");
		radio_force_sensor_analyze_groupal_current_session = (Gtk.RadioButton) builder.GetObject ("radio_force_sensor_analyze_groupal_current_session");
		image_force_sensor_analyze_individual_current_set = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_individual_current_set");
		image_force_sensor_analyze_individual_current_session = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_individual_current_session");
		image_force_sensor_analyze_individual_all_sessions = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_individual_all_sessions");
		image_force_sensor_analyze_groupal_current_session = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_groupal_current_session");

		notebook_force_sensor_analyze_top = (Gtk.Notebook) builder.GetObject ("notebook_force_sensor_analyze_top");
		hbox_force_general_analysis = (Gtk.HBox) builder.GetObject ("hbox_force_general_analysis");
		button_force_sensor_analyze_load = (Gtk.Button) builder.GetObject ("button_force_sensor_analyze_load");
		box_buttons_force_sensor_analyze_load_unchained = (Gtk.Box) builder.GetObject ("box_buttons_force_sensor_analyze_load_unchained");
		button_force_sensor_analyze_analyze = (Gtk.Button) builder.GetObject ("button_force_sensor_analyze_analyze");
		label_force_sensor_analyze = (Gtk.Label) builder.GetObject ("label_force_sensor_analyze");
		image_force_sensor_graph = (Gtk.Image) builder.GetObject ("image_force_sensor_graph");
		viewport_force_sensor_graph = (Gtk.Viewport) builder.GetObject ("viewport_force_sensor_graph");
		button_force_sensor_image_save_rfd_auto = (Gtk.Button) builder.GetObject ("button_force_sensor_image_save_rfd_auto");
		button_force_sensor_image_save_rfd_manual = (Gtk.Button) builder.GetObject ("button_force_sensor_image_save_rfd_manual");
		button_force_sensor_analyze_AB_save = (Gtk.Button) builder.GetObject ("button_force_sensor_analyze_AB_save");
		button_force_sensor_analyze_CD_save = (Gtk.Button) builder.GetObject ("button_force_sensor_analyze_CD_save");
		check_force_sensor_ai_chained_hscales = (Gtk.CheckButton) builder.GetObject ("check_force_sensor_ai_chained_hscales");
		check_force_sensor_ai_zoom = (Gtk.CheckButton) builder.GetObject ("check_force_sensor_ai_zoom");

		radio_force_rfd_search_optimized_ab = (Gtk.RadioButton) builder.GetObject ("radio_force_rfd_search_optimized_ab");
		radio_force_rfd_use_ab_range = (Gtk.RadioButton) builder.GetObject ("radio_force_rfd_use_ab_range");
		spin_force_duration_seconds = (Gtk.SpinButton) builder.GetObject ("spin_force_duration_seconds");
		radio_force_duration_seconds = (Gtk.RadioButton) builder.GetObject ("radio_force_duration_seconds");
		hbox_force_rfd_duration_percent = (Gtk.HBox) builder.GetObject ("hbox_force_rfd_duration_percent");
		radio_force_rfd_duration_percent = (Gtk.RadioButton) builder.GetObject ("radio_force_rfd_duration_percent");
		spin_force_rfd_duration_percent = (Gtk.SpinButton) builder.GetObject ("spin_force_rfd_duration_percent");

		//analyze options
		notebook_force_sensor_rfd_options = (Gtk.Notebook) builder.GetObject ("notebook_force_sensor_rfd_options");
		hbox_force_sensor_analyze_top_modes = (Gtk.HBox) builder.GetObject ("hbox_force_sensor_analyze_top_modes");
		//	hbox_force_sensor_analyze_automatic_options = (Gtk.HBox) builder.GetObject ("hbox_force_sensor_analyze_automatic_options");
		//	notebook_force_analyze_automatic = (Gtk.Notebook) builder.GetObject ("notebook_force_analyze_automatic");
		button_force_sensor_analyze_options_close_and_analyze = (Gtk.Button) builder.GetObject ("button_force_sensor_analyze_options_close_and_analyze");
		label_hscale_force_sensor_ai_a_pre_1s = (Gtk.Label) builder.GetObject ("label_hscale_force_sensor_ai_a_pre_1s");
		label_hscale_force_sensor_ai_a_post_1s = (Gtk.Label) builder.GetObject ("label_hscale_force_sensor_ai_a_post_1s");
		label_hscale_force_sensor_ai_b_pre_1s = (Gtk.Label) builder.GetObject ("label_hscale_force_sensor_ai_b_pre_1s");
		label_hscale_force_sensor_ai_b_post_1s = (Gtk.Label) builder.GetObject ("label_hscale_force_sensor_ai_b_post_1s");
		vbox_force_rfd_duration_end = (Gtk.VBox) builder.GetObject ("vbox_force_rfd_duration_end");
		button_force_sensor_analyze_options = (Gtk.Button) builder.GetObject ("button_force_sensor_analyze_options");
		hbox_force_1 = (Gtk.HBox) builder.GetObject ("hbox_force_1");
		hbox_force_2 = (Gtk.HBox) builder.GetObject ("hbox_force_2");
		hbox_force_3 = (Gtk.HBox) builder.GetObject ("hbox_force_3");
		hbox_force_4 = (Gtk.HBox) builder.GetObject ("hbox_force_4");
		hbox_force_impulse = (Gtk.HBox) builder.GetObject ("hbox_force_impulse");
		check_force_1 = (Gtk.CheckButton) builder.GetObject ("check_force_1");
		check_force_2 = (Gtk.CheckButton) builder.GetObject ("check_force_2");
		check_force_3 = (Gtk.CheckButton) builder.GetObject ("check_force_3");
		check_force_4 = (Gtk.CheckButton) builder.GetObject ("check_force_4");
		check_force_impulse = (Gtk.CheckButton) builder.GetObject ("check_force_impulse");
		hbox_force_1_at_ms = (Gtk.HBox) builder.GetObject ("hbox_force_1_at_ms");
		hbox_force_2_at_ms = (Gtk.HBox) builder.GetObject ("hbox_force_2_at_ms");
		hbox_force_3_at_ms = (Gtk.HBox) builder.GetObject ("hbox_force_3_at_ms");
		hbox_force_4_at_ms = (Gtk.HBox) builder.GetObject ("hbox_force_4_at_ms");
		hbox_force_1_at_percent = (Gtk.HBox) builder.GetObject ("hbox_force_1_at_percent");
		hbox_force_2_at_percent = (Gtk.HBox) builder.GetObject ("hbox_force_2_at_percent");
		hbox_force_3_at_percent = (Gtk.HBox) builder.GetObject ("hbox_force_3_at_percent");
		hbox_force_4_at_percent = (Gtk.HBox) builder.GetObject ("hbox_force_4_at_percent");
		hbox_force_impulse_until_percent = (Gtk.HBox) builder.GetObject ("hbox_force_impulse_until_percent");
		hbox_force_1_from_to = (Gtk.HBox) builder.GetObject ("hbox_force_1_from_to");
		hbox_force_2_from_to = (Gtk.HBox) builder.GetObject ("hbox_force_2_from_to");
		hbox_force_3_from_to = (Gtk.HBox) builder.GetObject ("hbox_force_3_from_to");
		hbox_force_4_from_to = (Gtk.HBox) builder.GetObject ("hbox_force_4_from_to");
		hbox_force_1_in_x_ms = (Gtk.HBox) builder.GetObject ("hbox_force_1_in_x_ms");
		hbox_force_2_in_x_ms = (Gtk.HBox) builder.GetObject ("hbox_force_2_in_x_ms");
		hbox_force_3_in_x_ms = (Gtk.HBox) builder.GetObject ("hbox_force_3_in_x_ms");
		hbox_force_4_in_x_ms = (Gtk.HBox) builder.GetObject ("hbox_force_4_in_x_ms");
		spinbutton_force_1_at_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_1_at_ms");
		spinbutton_force_2_at_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_2_at_ms");
		spinbutton_force_3_at_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_3_at_ms");
		spinbutton_force_4_at_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_4_at_ms");
		hbox_force_impulse_from_to = (Gtk.HBox) builder.GetObject ("hbox_force_impulse_from_to");
		spinbutton_force_1_at_percent = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_1_at_percent");
		spinbutton_force_2_at_percent = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_2_at_percent");
		spinbutton_force_3_at_percent = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_3_at_percent");
		spinbutton_force_4_at_percent = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_4_at_percent");
		spinbutton_force_impulse_until_percent = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_impulse_until_percent");
		spinbutton_force_1_from = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_1_from");
		spinbutton_force_2_from = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_2_from");
		spinbutton_force_3_from = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_3_from");
		spinbutton_force_4_from = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_4_from");
		spinbutton_force_impulse_from = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_impulse_from");
		spinbutton_force_1_to = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_1_to");
		spinbutton_force_2_to = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_2_to");
		spinbutton_force_3_to = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_3_to");
		spinbutton_force_4_to = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_4_to");
		spinbutton_force_impulse_to = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_impulse_to");
		spinbutton_force_1_in_x_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_1_in_x_ms");
		spinbutton_force_2_in_x_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_2_in_x_ms");
		spinbutton_force_3_in_x_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_3_in_x_ms");
		spinbutton_force_4_in_x_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_4_in_x_ms");

		button_hscale_force_sensor_ai_a_first = (Gtk.Button) builder.GetObject ("button_hscale_force_sensor_ai_a_first");
		button_hscale_force_sensor_ai_a_pre = (Gtk.Button) builder.GetObject ("button_hscale_force_sensor_ai_a_pre");
		button_hscale_force_sensor_ai_a_post = (Gtk.Button) builder.GetObject ("button_hscale_force_sensor_ai_a_post");
		button_hscale_force_sensor_ai_a_last = (Gtk.Button) builder.GetObject ("button_hscale_force_sensor_ai_a_last");

		button_hscale_force_sensor_ai_b_first = (Gtk.Button) builder.GetObject ("button_hscale_force_sensor_ai_b_first");
		button_hscale_force_sensor_ai_b_pre = (Gtk.Button) builder.GetObject ("button_hscale_force_sensor_ai_b_pre");
		button_hscale_force_sensor_ai_b_post = (Gtk.Button) builder.GetObject ("button_hscale_force_sensor_ai_b_post");
		button_hscale_force_sensor_ai_b_last = (Gtk.Button) builder.GetObject ("button_hscale_force_sensor_ai_b_last");

		notebook_force_sensor_export = (Gtk.Notebook) builder.GetObject ("notebook_force_sensor_export");
		label_force_sensor_export_data = (Gtk.Label) builder.GetObject ("label_force_sensor_export_data");
		hbox_force_sensor_export_images = (Gtk.HBox) builder.GetObject ("hbox_force_sensor_export_images");
		check_force_sensor_export_images = (Gtk.CheckButton) builder.GetObject ("check_force_sensor_export_images");
		hbox_force_sensor_export_width_height = (Gtk.HBox) builder.GetObject ("hbox_force_sensor_export_width_height");
		spinbutton_force_sensor_export_image_width = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_sensor_export_image_width");
		spinbutton_force_sensor_export_image_height = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_sensor_export_image_height");
		progressbar_force_sensor_export = (Gtk.ProgressBar) builder.GetObject ("progressbar_force_sensor_export");
		label_force_sensor_export_result = (Gtk.Label) builder.GetObject ("label_force_sensor_export_result");
		button_force_sensor_export_result_open = (Gtk.Button) builder.GetObject ("button_force_sensor_export_result_open");

		hbox_force_sensor_analyze_ai_sliders_and_buttons = (Gtk.HBox) builder.GetObject ("hbox_force_sensor_analyze_ai_sliders_and_buttons");
		force_sensor_ai_drawingarea_cairo = (Gtk.DrawingArea) builder.GetObject ("force_sensor_ai_drawingarea_cairo");
		box_force_sensor_analyze_magnitudes = (Gtk.Box) builder.GetObject ("box_force_sensor_analyze_magnitudes");
		check_force_sensor_analyze_show_distance = (Gtk.CheckButton) builder.GetObject ("check_force_sensor_analyze_show_distance");
		check_force_sensor_analyze_show_speed = (Gtk.CheckButton) builder.GetObject ("check_force_sensor_analyze_show_speed");
		check_force_sensor_analyze_show_power = (Gtk.CheckButton) builder.GetObject ("check_force_sensor_analyze_show_power");
		image_force_sensor_analyze_show_distance = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_show_distance");
		image_force_sensor_analyze_show_speed = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_show_speed");
		image_force_sensor_analyze_show_power = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_show_power");
		viewport_force_sensor_analyze_hscales = (Gtk.Viewport) builder.GetObject ("viewport_force_sensor_analyze_hscales");
		viewport_radio_force_sensor_ai_ab = (Gtk.Viewport) builder.GetObject ("viewport_radio_force_sensor_ai_ab");
		viewport_radio_force_sensor_ai_cd = (Gtk.Viewport) builder.GetObject ("viewport_radio_force_sensor_ai_cd");
		radio_force_sensor_ai_ab = (Gtk.RadioButton) builder.GetObject ("radio_force_sensor_ai_ab");
		radio_force_sensor_ai_cd = (Gtk.RadioButton) builder.GetObject ("radio_force_sensor_ai_cd");
		check_force_sensor_ai_chained_load_link = (Gtk.CheckButton) builder.GetObject ("check_force_sensor_ai_chained_load_link");
		image_force_sensor_ai_chained_load_link = (Gtk.Image) builder.GetObject ("image_force_sensor_ai_chained_load_link");
		box_force_sensor_ai_a = (Gtk.Box) builder.GetObject ("box_force_sensor_ai_a");
		box_force_sensor_ai_b = (Gtk.Box) builder.GetObject ("box_force_sensor_ai_b");
		box_force_sensor_ai_c = (Gtk.Box) builder.GetObject ("box_force_sensor_ai_c");
		box_force_sensor_ai_d = (Gtk.Box) builder.GetObject ("box_force_sensor_ai_d");
		label_force_sensor_ai_zoom_abcd = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_zoom_abcd");
		hscale_force_sensor_ai_a = (Gtk.HScale) builder.GetObject ("hscale_force_sensor_ai_a");
		hscale_force_sensor_ai_b = (Gtk.HScale) builder.GetObject ("hscale_force_sensor_ai_b");
		hscale_force_sensor_ai_c = (Gtk.HScale) builder.GetObject ("hscale_force_sensor_ai_c");
		hscale_force_sensor_ai_d = (Gtk.HScale) builder.GetObject ("hscale_force_sensor_ai_d");
		treeview_force_sensor_ai_AB = (Gtk.TreeView) builder.GetObject ("treeview_force_sensor_ai_AB");
		treeview_force_sensor_ai_CD = (Gtk.TreeView) builder.GetObject ("treeview_force_sensor_ai_CD");
		treeview_force_sensor_ai_other = (Gtk.TreeView) builder.GetObject ("treeview_force_sensor_ai_other");

		combo_force_1_function = (Gtk.ComboBoxText) builder.GetObject ("combo_force_1_function");
		combo_force_2_function = (Gtk.ComboBoxText) builder.GetObject ("combo_force_2_function");
		combo_force_3_function = (Gtk.ComboBoxText) builder.GetObject ("combo_force_3_function");
		combo_force_4_function = (Gtk.ComboBoxText) builder.GetObject ("combo_force_4_function");
		combo_force_impulse_function = (Gtk.ComboBoxText) builder.GetObject ("combo_force_impulse_function");
		combo_force_1_type = (Gtk.ComboBoxText) builder.GetObject ("combo_force_1_type");
		combo_force_2_type = (Gtk.ComboBoxText) builder.GetObject ("combo_force_2_type");
		combo_force_3_type = (Gtk.ComboBoxText) builder.GetObject ("combo_force_3_type");
		combo_force_4_type = (Gtk.ComboBoxText) builder.GetObject ("combo_force_4_type");
		combo_force_impulse_type = (Gtk.ComboBoxText) builder.GetObject ("combo_force_impulse_type");
	}
}


public abstract class TreeviewFSAbstract
{
	protected TreeStore store;
	protected Gtk.TreeView tv;
	protected string [] columnsString;

	protected void createTreeview ()
	{
		tv = UtilGtk.RemoveColumns (tv);
		columnsString = setColumnsString ();
		store = UtilGtk.GetStore (columnsString.Length);
		tv.Model = store;
		prepareHeaders (columnsString);
		tv.HeadersClickable = false;
	}

	protected virtual string [] setColumnsString ()
	{
		return new string [] {};
	}

	protected void prepareHeaders(string [] columnsString)
	{
		tv.HeadersVisible = true;
		int i = 0;
		foreach (string myCol in columnsString)
			UtilGtk.CreateCols (tv, store, myCol, i++, true);
	}

	public void ResetTreeview ()
	{
		if (tv != null)
			tv = UtilGtk.RemoveColumns (tv);

		createTreeview ();
	}

}

// 1 for AB and another for CD:
// tvFS_AB for treeview_force_sensor_ai_AB
// tvFS_CD for treeview_force_sensor_ai_CD
public class TreeviewFSAnalyze : TreeviewFSAbstract
{
	//row 1
	protected string letterStart;
	protected string timeStart;
	protected double forceStart;
	protected string rfdStart;

	//row 2
	protected string letterEnd;
	protected string timeEnd;
	protected double forceEnd;
	protected string rfdEnd;
	private string positionEnd;
	private string speedEnd;
	private string accelEnd;
	private string powerEnd;

	//row 3
	protected string timeDiff;
	protected double forceDiff;
	protected double rfdDiff;

	//row 4
	protected string forceAvg;
	protected string rfdAvg;

	//row 5
	protected string forceMax;
	protected string rfdMax;

	public TreeviewFSAnalyze () //needed to inherit
	{
	}

	public TreeviewFSAnalyze (Gtk.TreeView tv, string letterStart, string letterEnd)
	{
		this.tv = tv;
		this.letterStart = letterStart;
		this.letterEnd = letterEnd;


		createTreeview ();
	}

	protected override string [] setColumnsString ()
	{
		return new String [] {
			"",
			Catalog.GetString ("Time") + " (ms)",
			Catalog.GetString ("Force") + " (N)",
			"RFD" + " (N/s)"
		};
	}

	//some are string because it is easier to know if missing data, because doble could be 0.00000001 ...
	public void PassRow1or2 (bool isLeft, string time, double force, string rfd)
	{
		if (isLeft)
		{
			this.timeStart = time;
			this.forceStart = force;
			this.rfdStart = rfd;
		} else {
			this.timeEnd = time;
			this.forceEnd = force;
			this.rfdEnd = rfd;
		}
	}

	public virtual void PassRow1or2Elastic (bool isLeft, string position, string speed, string accel, string power)
	{
	}

	public virtual void PassElasticDiffs (string position, string speed, string accel, string power)
	{
	}
	public virtual void PassElasticAvgs (string speed, string accel, string power)
	{
	}
	public virtual void PassElasticMaxs (string speed, string accel, string power)
	{
	}

	protected virtual string [] getTreeviewStr ()
	{
		return new String [4];
	}

	public virtual void FillTreeview ()
	{
		string [] str = getTreeviewStr ();
		store.AppendValues (fillTreeViewStart (str, 0));
		store.AppendValues (fillTreeViewEnd (str, 0));
		store.AppendValues (fillTreeViewDiff (str, 0));
		store.AppendValues (fillTreeViewAvg (str, 0));
		store.AppendValues (fillTreeViewMax (str, 0));
	}

	private string [] fillTreeViewStart (string [] str, int i)
	{
		str[i++] = letterStart;
		str[i++] = timeStart;
		str[i++] = Math.Round (forceStart, 1).ToString ();
		str[i++] = rfdStart;
		return fillTreeViewStartElastic (str, i);
	}

	private string [] fillTreeViewEnd (string [] str, int i)
	{
		str[i++] = letterEnd;
		str[i++] = timeEnd;
		str[i++] = Math.Round (forceEnd, 1).ToString ();
		str[i++] = rfdEnd;
		return fillTreeViewEndElastic (str, i);
	}

	private string [] fillTreeViewDiff (string [] str, int i)
	{
		str[i++] = Catalog.GetString ("Difference");
		str[i++] = timeDiff;
		str[i++] = Math.Round (forceDiff, 1).ToString ();
		str[i++] = Math.Round (rfdDiff, 1).ToString ();
		return fillTreeViewDiffElastic (str, i);
	}

	private string [] fillTreeViewAvg (string [] str, int i)
	{
		str[i++] = Catalog.GetString ("Average");
		str[i++] = ""; // no time avg
		str[i++] = forceAvg;
		str[i++] = rfdAvg;
		return fillTreeViewAvgElastic (str, i);
	}

	private string [] fillTreeViewMax (string [] str, int i)
	{
		str[i++] = Catalog.GetString ("Maximum");
		str[i++] = ""; // no time max
		str[i++] = forceMax;
		str[i++] = rfdMax;
		return fillTreeViewMaxElastic (str, i);
	}

	protected virtual string [] fillTreeViewStartElastic (string [] str, int i)
	{
		return str;
	}
	protected virtual string [] fillTreeViewEndElastic (string [] str, int i)
	{
		return str;
	}
	protected virtual string [] fillTreeViewDiffElastic (string [] str, int i)
	{
		return str;
	}
	protected virtual string [] fillTreeViewAvgElastic (string [] str, int i)
	{
		return str;
	}
	protected virtual string [] fillTreeViewMaxElastic (string [] str, int i)
	{
		return str;
	}

	public string TimeStart {
		get { return timeStart; }
	}
	public string TimeEnd {
		get { return timeEnd; }
	}

	// this accessors help to pass variables once are calculated on force_sensor_analyze_instant_calculate_params
	public string TimeDiff {
		get { return timeDiff; }
		set { timeDiff = value; }
	}
	public double ForceDiff {
		set { forceDiff = value; }
	}
	public double RfdDiff {
		set { rfdDiff = value; }
	}

	public string ForceAvg {
		set { forceAvg = value; }
	}
	public string RfdAvg {
		set { rfdAvg = value; }
	}

	public string ForceMax {
		set { forceMax = value; }
	}
	public string RfdMax {
		set { rfdMax = value; }
	}
}

//TODO: move this to another file once the new Windows compilation is working
public class TreeviewFSAnalyzeElastic : TreeviewFSAnalyze
{
	//row 1
	private string positionStart;
	private string speedStart;
	private string accelStart;
	private string powerStart;

	//row 2
	private string positionEnd;
	private string speedEnd;
	private string accelEnd;
	private string powerEnd;

	//row 3
	private string positionDiff;
	private string speedDiff;
	private string accelDiff;
	private string powerDiff;

	//row 4
	private string speedAvg;
	private string accelAvg;
	private string powerAvg;

	//row 5
	private string speedMax;
	private string accelMax;
	private string powerMax;

	public TreeviewFSAnalyzeElastic (Gtk.TreeView tv, string letterStart, string letterEnd)
	{
		this.tv = tv;
		this.letterStart = letterStart;
		this.letterEnd = letterEnd;

		createTreeview ();
	}

	protected override string [] setColumnsString ()
	{
		//elastic has units below to not loose a lot of horizontal space
		return new String [] {
			"",
			Catalog.GetString ("Time") + "\n(ms)",
			Catalog.GetString ("Force") + "\n(N)",
			"RFD" + "\n(N/s)",
			Catalog.GetString ("Position") + "\n(m)",
			Catalog.GetString ("Speed") + "\n(m/s)",
			Catalog.GetString ("Acceleration") + "\n(m/s^2)",
			Catalog.GetString ("Power") + "\n(W)"
		};
	}

	//some are string because it is easier to know if missing data, because doble could be 0.00000001 ...
	public override void PassRow1or2Elastic (bool isLeft, string position, string speed, string accel, string power)
	{
		if (isLeft)
		{
			this.positionStart = position;
			this.speedStart = speed;
			this.accelStart = accel;
			this.powerStart = power;
		} else {
			this.positionEnd = position;
			this.speedEnd = speed;
			this.accelEnd = accel;
			this.powerEnd = power;
		}
	}

	public override void PassElasticDiffs (string position, string speed, string accel, string power)
	{
		this.positionDiff = position;
		this.speedDiff = speed;
		this.accelDiff = accel;
		this.powerDiff = power;
	}

	public override void PassElasticAvgs (string speed, string accel, string power)
	{
		this.speedAvg = speed;
		this.accelAvg = accel;
		this.powerAvg = power;
	}

	public override void PassElasticMaxs (string speed, string accel, string power)
	{
		this.speedMax = speed;
		this.accelMax = accel;
		this.powerMax = power;
	}

	protected override string [] getTreeviewStr ()
	{
		return new String [8];
	}

	protected override string [] fillTreeViewStartElastic (string [] str, int i)
	{
		str[i++] = positionStart;
		str[i++] = speedStart;
		str[i++] = accelStart;
		str[i++] = powerStart;
		return str;
	}
	protected override string [] fillTreeViewEndElastic (string [] str, int i)
	{
		str[i++] = positionEnd;
		str[i++] = speedEnd;
		str[i++] = accelEnd;
		str[i++] = powerEnd;
		return str;
	}
	protected override string [] fillTreeViewDiffElastic (string [] str, int i)
	{
		str[i++] = positionDiff;
		str[i++] = speedDiff;
		str[i++] = accelDiff;
		str[i++] = powerDiff;
		return str;
	}
	protected override string [] fillTreeViewAvgElastic (string [] str, int i)
	{
		str[i++] = ""; // no position average
		str[i++] = speedAvg;
		str[i++] = accelAvg;
		str[i++] = powerAvg;
		return str;
	}
	protected override string [] fillTreeViewMaxElastic (string [] str, int i)
	{
		str[i++] = ""; // no position max
		str[i++] = speedMax;
		str[i++] = accelMax;
		str[i++] = powerMax;
		return str;
	}
}

public class TreeviewFSAnalyzeOther : TreeviewFSAbstract
{
	private bool showColumnAB;
	private bool showColumnCD;

	//values
	private string impulseAB;
	private string impulseCD;

	private string variabilityAB;
	private string variabilityCD;

	private string feedbackAB;
	private string feedbackCD;

	private string maxAvgInWindowAB;
	private string maxAvgInWindowCD;

	private string bestRFDInWindowAB;
	private string bestRFDInWindowCD;

	//1st column names
	private string variabilityMethod;
	private string variabilityUnits;

	private string maxAvgInWindowName;

	public TreeviewFSAnalyzeOther (Gtk.TreeView tv)
	{
		this.tv = tv;

		if (!showColumnAB && ! showColumnCD)
		{
			tv.Visible = false;
			return;
		}

		createTreeview ();
	}

	protected override string [] setColumnsString ()
	{
		if (showColumnAB && showColumnCD)
			return new String [] { "", "A-B", "C-D" };
		else if (showColumnAB)
			return new String [] { "", "A-B", };
		else if (showColumnCD)
			return new String [] { "", "C-D" };
		else
			return new String [] { "" };
	}

	public void Fill ()
	{
		if (! showColumnAB && ! showColumnCD)
		{
			tv.Visible = false;
			return;
		}

		store.AppendValues (fillImpulse ());
		store.AppendValues (fillVariability ());
		store.AppendValues (fillFeedback ());
		store.AppendValues (fillMaxAvgInWindow ());
		store.AppendValues (fillBestRFDInWindow ());

		tv.Visible = true;
	}

	private string [] fillImpulse ()
	{
		if (showColumnAB && showColumnCD)
			return new String [] { Catalog.GetString ("Impulse"), impulseAB + " N*s", impulseCD + " N*s" };
		else if (showColumnAB)
			return new String [] { Catalog.GetString ("Impulse"), impulseAB + " N*s" };
		else //if (showColumnCD)
			return new String [] { Catalog.GetString ("Impulse"), impulseCD + " N*s" };
	}

	private string [] fillVariability ()
	{
		if (showColumnAB && showColumnCD)
			return new String [] {
				Catalog.GetString ("Variability") + " (" + variabilityMethod + ")",
					variabilityAB + " " + variabilityUnits,
					variabilityCD + " " + variabilityUnits,
			};
		else if (showColumnAB)
			return new String [] {
				Catalog.GetString ("Variability") + " (" + variabilityMethod + ")",
					variabilityAB + " " + variabilityUnits
			};
		else //if (showColumnCD)
			return new String [] {
				Catalog.GetString ("Variability") + " (" + variabilityMethod + ")",
					variabilityCD + " " + variabilityUnits
			};
	}

	private string [] fillFeedback ()
	{
		if (showColumnAB && showColumnCD)
			return new String [] { Catalog.GetString ("Feedback"), feedbackAB + " N", feedbackCD + " N" };
		else if (showColumnAB)
			return new String [] { Catalog.GetString ("Feedback"), feedbackAB + " N"};
		else //if (showColumnCD)
			return new String [] { Catalog.GetString ("Feedback"), feedbackCD + " N" };
	}

	private string [] fillMaxAvgInWindow ()
	{
		if (showColumnAB && showColumnCD)
			return new String [] { maxAvgInWindowName, maxAvgInWindowAB + " N", maxAvgInWindowCD + " N" };
		else if (showColumnAB)
			return new String [] { maxAvgInWindowName, maxAvgInWindowAB + " N" };
		else //if (showColumnCD)
			return new String [] { maxAvgInWindowName, maxAvgInWindowCD + " N" };
	}

	private string [] fillBestRFDInWindow ()
	{
		if (showColumnAB && showColumnCD)
			return new String [] { Catalog.GetString ("Best RFD") + " (50 ms avg)",
				bestRFDInWindowAB + " N/s", bestRFDInWindowCD + " N/s",
			};
		else if (showColumnAB)
			return new String [] { Catalog.GetString ("Best RFD") + " (50 ms avg)",
				bestRFDInWindowAB + " N/s"
			};
		else //if (showColumnCD)
			return new String [] { Catalog.GetString ("Best RFD") + " (50 ms avg)",
				bestRFDInWindowCD + " N/s"
			};
	}

	public void ShowColumn (bool show, bool ab)
	{
		if (ab)
			showColumnAB = show;
		else
			showColumnCD = show;
	}

	//values

	public void SetImpulse (string s, bool ab)
	{
		if (ab)
			impulseAB = s;
		else
			impulseCD = s;
	}

	public void SetVariability (string s, bool ab)
	{
		if (ab)
			variabilityAB = s;
		else
			variabilityCD = s;
	}

	public void SetFeedback (string s, bool ab)
	{
		if (ab)
			feedbackAB = s;
		else
			feedbackCD = s;
	}

	public void SetMaxAvgInWindow (string s, bool ab)
	{
		if (ab)
			maxAvgInWindowAB = s;
		else
			maxAvgInWindowCD = s;
	}

	public void SetBestRFDInWindow (string s, bool ab)
	{
		if (ab)
			bestRFDInWindowAB = s;
		else
			bestRFDInWindowCD = s;
	}

	public string VariabilityMethod {
		set {
			variabilityMethod = value;
			if (value == Preferences.VariabilityMethodEnum.CVRMSSD.ToString () || value == Preferences.VariabilityMethodEnum.CV.ToString ())
				variabilityUnits = "%";
			else
				variabilityUnits = "N";
		}
	}

	//1st column names
	public string MaxAvgInWindowName {
		set { maxAvgInWindowName = value; }
	}
}
