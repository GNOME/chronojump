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
	Gtk.Button button_force_sensor_analyze_analyze;
	Gtk.Label label_force_sensor_analyze;
	Gtk.Image image_force_sensor_graph;
	Gtk.Viewport viewport_force_sensor_graph;
	Gtk.Button button_force_sensor_image_save_rfd_auto;
	Gtk.Button button_force_sensor_image_save_rfd_manual;
	//Gtk.ScrolledWindow scrolledwindow_force_sensor_ai;
	Gtk.Button button_force_sensor_analyze_AB_save;
	Gtk.CheckButton check_force_sensor_ai_chained;
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

	Gtk.HBox hbox_force_sensor_ai_position;
	Gtk.HBox hbox_force_sensor_ai_speed;
	Gtk.HBox hbox_force_sensor_ai_accel;
	Gtk.HBox hbox_force_sensor_ai_power;

	Gtk.Table table_force_sensor_ai_impulse_variability_and_feedback;
	Gtk.Label label_force_sensor_ai_feedback;
	Gtk.HBox hbox_force_sensor_ai_feedback;
	Gtk.Label label_force_sensor_ai_impulse_values;
	Gtk.Label label_force_sensor_ai_variability_values;
	Gtk.Label label_force_sensor_ai_feedback_values;
	Gtk.Label label_force_sensor_ai_variability_method;
	Gtk.Label label_force_sensor_ai_variability_units;
	Gtk.Label label_force_sensor_ai_max_avg_in_window;

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
	Gtk.DrawingArea force_sensor_ai_drawingarea_cairo;
	Gtk.HScale hscale_force_sensor_ai_a;
	Gtk.HScale hscale_force_sensor_ai_b;
	Gtk.Label label_force_sensor_ai_time_a;
	Gtk.Label label_force_sensor_ai_force_a;
	Gtk.Label label_force_sensor_ai_rfd_a;
	Gtk.Label label_force_sensor_ai_position_a;
	Gtk.Label label_force_sensor_ai_speed_a;
	Gtk.Label label_force_sensor_ai_accel_a;
	Gtk.Label label_force_sensor_ai_power_a;
	//Gtk.HBox hbox_buttons_scale_force_sensor_ai_b;
	Gtk.Label label_force_sensor_ai_position_b;
	Gtk.Label label_force_sensor_ai_position_diff;
	Gtk.Label label_force_sensor_ai_speed_b;
	Gtk.Label label_force_sensor_ai_speed_diff;
	Gtk.Label label_force_sensor_ai_speed_average;
	Gtk.Label label_force_sensor_ai_speed_max;
	Gtk.Label label_force_sensor_ai_accel_b;
	Gtk.Label label_force_sensor_ai_accel_diff;
	Gtk.Label label_force_sensor_ai_accel_average;
	Gtk.Label label_force_sensor_ai_accel_max;
	Gtk.Label label_force_sensor_ai_power_b;
	Gtk.Label label_force_sensor_ai_power_diff;
	Gtk.Label label_force_sensor_ai_power_average;
	Gtk.Label label_force_sensor_ai_power_max;
	Gtk.Label label_force_sensor_ai_time_b;
	Gtk.Label label_force_sensor_ai_time_diff;
	Gtk.Label label_force_sensor_ai_force_b;
	Gtk.Label label_force_sensor_ai_force_diff;
	Gtk.Label label_force_sensor_ai_force_average;
	Gtk.Label label_force_sensor_ai_force_max;
	Gtk.Label label_force_sensor_ai_rfd_b;
	Gtk.Label label_force_sensor_ai_rfd_diff;
	Gtk.Label label_force_sensor_ai_rfd_average;
	Gtk.Label label_force_sensor_ai_rfd_max;
	Gtk.Label label_force_sensor_ai_max_avg_in_window_values;

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
		label_force_sensor_ai_max_avg_in_window.Text = string.Format("Max AVG Force in {0} s",
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


	ForceSensorAnalyzeInstant fsAI;

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
	}
	private void on_radio_force_sensor_analyze_groupal_current_session_toggled (object o, EventArgs args)
	{
		button_force_sensor_analyze_load.Visible = false;

		label_force_sensor_export_data.Text = currentSession.Name;

		notebook_force_sensor_analyze_top.CurrentPage = Convert.ToInt32(notebook_force_sensor_analyze_top_pages.CURRENTSESSION);
		label_force_sensor_export_result.Text = "";
		button_force_sensor_export_result_open.Visible = false;
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

	private void forceSensorDoGraphAI(bool windowResizedAndZoom)
	{
		if(lastForceSensorFullPath == null || lastForceSensorFullPath == "")
			return;

		int zoomFrameA = -1; //means no zoom
		int zoomFrameB = -1; //means no zoom

		if(windowResizedAndZoom)
		{
			//like this works but cannot calculate the RFD of A,B
			zoomFrameA = hscale_force_sensor_ai_a_BeforeZoom;
			zoomFrameB = hscale_force_sensor_ai_b_BeforeZoom;
			//zoomFrameA = hscale_force_sensor_ai_a_BeforeZoom -1;
			//zoomFrameB = hscale_force_sensor_ai_b_BeforeZoom +1;
		}
		else if(forceSensorZoomApplied &&
				Util.IsNumber(label_force_sensor_ai_time_a.Text, true) &&
				Util.IsNumber(label_force_sensor_ai_time_b.Text, true))
		{
			//invert hscales if needed
			int firstValue = Convert.ToInt32(hscale_force_sensor_ai_a.Value);
			int secondValue = Convert.ToInt32(hscale_force_sensor_ai_b.Value);
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
		fsAI = new ForceSensorAnalyzeInstant(
				lastForceSensorFullPath,
				zoomFrameA, zoomFrameB,
				currentForceSensorExercise, currentPersonSession.Weight,
				getForceSensorCaptureOptions(), currentForceSensor.Stiffness,
				eccMinDispl, conMinDispl
				);
		//LogB.Information("created fsAI");
		LogB.Information(string.Format("fsAI.GetLength: {0}", fsAI.GetLength()));

		/*
		 * position the hscales on the left to avoid loading a csv
		 * with less data rows than current csv and having scales out of the graph
		//hscale_force_sensor_ai_a.ValuePos = Gtk.PositionType.Left; //doesn't work
		//hscale_force_sensor_ai_b.ValuePos = Gtk.PositionType.Left; //doesn't work
		*/
		hscale_force_sensor_ai_a.Value = 0;
		hscale_force_sensor_ai_b.Value = 0;

		//ranges should have max value the number of the lines of csv file minus the header
		hscale_force_sensor_ai_a.SetRange(0, fsAI.GetLength() -1);
		hscale_force_sensor_ai_b.SetRange(0, fsAI.GetLength() -1);
		//set them to 0, because if not is set to 1 by a GTK error
		hscale_force_sensor_ai_a.Value = 0;
		hscale_force_sensor_ai_b.Value = 0;

		LogB.Information(string.Format("hscale_force_sensor_ai_time_a,b,ab ranges: 0, {0}", fsAI.GetLength() -1));

		//on zoom put hscale B at the right
		if(zoomFrameB >= 0)
			hscale_force_sensor_ai_b.Value = fsAI.GetLength() -1;

		//to update values
		on_hscale_force_sensor_ai_a_value_changed (new object (), new EventArgs ());

		manage_force_sensor_ai_table_visibilities();
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

		if (cairoGraphForceSensorSignalPoints_l == null)
			cairoGraphForceSensorSignalPoints_l = new List<PointF> ();
		if (cairoGraphForceSensorSignalPointsDispl_l == null)
			cairoGraphForceSensorSignalPointsDispl_l = new List<PointF> ();
		if (cairoGraphForceSensorSignalPointsSpeed_l == null)
			cairoGraphForceSensorSignalPointsSpeed_l = new List<PointF> ();
		if (cairoGraphForceSensorSignalPointsAccel_l == null)
			cairoGraphForceSensorSignalPointsAccel_l = new List<PointF> ();
		if (cairoGraphForceSensorSignalPointsPower_l == null)
			cairoGraphForceSensorSignalPointsPower_l = new List<PointF> ();

		/* no need of copy because this graph is done on load or at end of capture (points_list does not grow in other thread
		int pointsToCopy = cairoGraphForceSensorSignalPoints_l.Count;
		List<PointF> cairoGraphForceSensorSignalPoints_l_copy = new List<PointF>();
		for (int i = 0; i < pointsToCopy; i ++)
			cairoGraphForceSensorSignalPoints_l_copy.Add (cairoGraphForceSensorSignalPoints_l[i]);
		same for trigger
		*/


		int fMaxAvgSampleStart = -1;
		int fMaxAvgSampleEnd = -1;
		double fsMaxAvgForce = -1;
		List<ForceSensorRepetition> reps_l = new List<ForceSensorRepetition> ();
		if (fsAI != null)
		{
			if (fsAI.Gmaiw.Error == "")
			{
				fMaxAvgSampleStart = fsAI.Gmaiw.MaxSampleStart;
				fMaxAvgSampleEnd = fsAI.Gmaiw.MaxSampleEnd;
				fsMaxAvgForce = fsAI.Gmaiw.Max;
			}

			reps_l = fsAI.ForceSensorRepetition_l;
			if(forceSensorZoomApplied)
				reps_l = forceSensorRepetition_lZoomAppliedCairo;
		}

		int hscaleSampleStart = Convert.ToInt32 (hscale_force_sensor_ai_a.Value);
		int hscaleSampleEnd = Convert.ToInt32 (hscale_force_sensor_ai_b.Value);

		List<PointF> sendPoints_l = cairoGraphForceSensorSignalPoints_l;
		List<PointF> sendPointsDispl_l = cairoGraphForceSensorSignalPointsDispl_l;
		List<PointF> sendPointsSpeed_l = cairoGraphForceSensorSignalPointsSpeed_l;
		List<PointF> sendPointsPower_l = cairoGraphForceSensorSignalPointsPower_l;
		if(forceSensorZoomApplied)
		{
			sendPoints_l = cairoGraphForceSensorSignalPointsZoomed_l;
			sendPointsDispl_l = cairoGraphForceSensorSignalPointsDisplZoomed_l;
			sendPointsSpeed_l = cairoGraphForceSensorSignalPointsSpeedZoomed_l;
			sendPointsPower_l = cairoGraphForceSensorSignalPointsPowerZoomed_l;
		}

		//minimum Y display from -50 to 50
		int minY = -50;
		int maxY = +50;
		if (cairoGraphForceSensorSignalPointsDispl_l.Count > 0)
		{
			minY = 0;
			maxY = 0;
		}

		fsAIRepetitionMouseLimitsCairo = cairoGraphForceSensorAI.DoSendingList (
				preferences.fontType.ToString(),
				sendPoints_l,
				sendPointsDispl_l, sendPointsSpeed_l, sendPointsPower_l,
				minY, maxY,
				rectangleN, rectangleRange,
				triggerListForceSensor,
				hscaleSampleStart, hscaleSampleEnd, forceSensorZoomApplied,
				fMaxAvgSampleStart, fMaxAvgSampleEnd, fsMaxAvgForce,
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

		hscale_force_sensor_ai_a.Value = fsAIRepetitionMouseLimitsCairo.GetSampleStartOfARep (repetition);
		hscale_force_sensor_ai_b.Value = fsAIRepetitionMouseLimitsCairo.GetSampleEndOfARep (repetition);
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

	private double hscale_force_sensor_ai_a_BeforeZoomTimeMS = 0; //to calculate triggers

	static List<PointF> cairoGraphForceSensorSignalPointsZoomed_l;
	static List<PointF> cairoGraphForceSensorSignalPointsDisplZoomed_l;
	static List<PointF> cairoGraphForceSensorSignalPointsSpeedZoomed_l;
	static List<PointF> cairoGraphForceSensorSignalPointsAccelZoomed_l;
	static List<PointF> cairoGraphForceSensorSignalPointsPowerZoomed_l;
	private void on_check_force_sensor_ai_zoom_clicked (object o, EventArgs args)
	{
		if(fsAI == null || fsAI.GetLength() == 0)
			return;

		if(check_force_sensor_ai_zoom.Active)
		{
			forceSensorZoomApplied = true;

			//store hscale a to help return to position on unzoom
			hscale_force_sensor_ai_a_BeforeZoom = Convert.ToInt32(hscale_force_sensor_ai_a.Value);
			hscale_force_sensor_ai_b_BeforeZoom = Convert.ToInt32(hscale_force_sensor_ai_b.Value);


			cairoGraphForceSensorSignalPointsZoomed_l = new List<PointF> ();
			cairoGraphForceSensorSignalPointsDisplZoomed_l = new List<PointF> ();
			cairoGraphForceSensorSignalPointsSpeedZoomed_l = new List<PointF> ();
			cairoGraphForceSensorSignalPointsAccelZoomed_l = new List<PointF> ();
			cairoGraphForceSensorSignalPointsPowerZoomed_l = new List<PointF> ();
			for (int i = hscale_force_sensor_ai_a_BeforeZoom; i <= hscale_force_sensor_ai_b_BeforeZoom; i ++)
			{
				cairoGraphForceSensorSignalPointsZoomed_l.Add (cairoGraphForceSensorSignalPoints_l[i]);

				if (cairoGraphForceSensorSignalPointsDispl_l != null && cairoGraphForceSensorSignalPointsDispl_l.Count > 0)
					cairoGraphForceSensorSignalPointsDisplZoomed_l.Add (cairoGraphForceSensorSignalPointsDispl_l[i]);
				if (cairoGraphForceSensorSignalPointsSpeed_l != null && cairoGraphForceSensorSignalPointsSpeed_l.Count > 0)
					cairoGraphForceSensorSignalPointsSpeedZoomed_l.Add (cairoGraphForceSensorSignalPointsSpeed_l[i]);
				if (cairoGraphForceSensorSignalPointsAccel_l != null && cairoGraphForceSensorSignalPointsAccel_l.Count > 0)
					cairoGraphForceSensorSignalPointsAccelZoomed_l.Add (cairoGraphForceSensorSignalPointsAccel_l[i]);
				if (cairoGraphForceSensorSignalPointsPower_l != null && cairoGraphForceSensorSignalPointsPower_l.Count > 0)
					cairoGraphForceSensorSignalPointsPowerZoomed_l.Add (cairoGraphForceSensorSignalPointsPower_l[i]);
			}

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

			hscale_force_sensor_ai_a_AtZoom = Convert.ToInt32(hscale_force_sensor_ai_a.Value);
			hscale_force_sensor_ai_b_AtZoom = Convert.ToInt32(hscale_force_sensor_ai_b.Value);

			forceSensorDoGraphAI(false);

			hscale_force_sensor_ai_a.Value = hscale_force_sensor_ai_a_BeforeZoom + (hscale_force_sensor_ai_a_AtZoom);
			hscale_force_sensor_ai_b.Value = hscale_force_sensor_ai_a_BeforeZoom + (hscale_force_sensor_ai_b_AtZoom);

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


	bool forceSensorHScalesDoNotFollow = false;
	//to know change of slider in order to apply on the other slider if chained
	int force_sensor_last_a = 1;
	int force_sensor_last_b = 1;

	private void on_hscale_force_sensor_ai_a_value_changed (object o, EventArgs args)
	{
		if(fsAI == null || fsAI.GetLength() == 0)
			return;

		int diffWithPrevious = Convert.ToInt32(hscale_force_sensor_ai_a.Value) - force_sensor_last_a;

		//if chained and moving a to the right makes b higher than 1, do not move
		if(check_force_sensor_ai_chained.Active &&
				Convert.ToInt32(hscale_force_sensor_ai_b.Value) + diffWithPrevious >= fsAI.GetLength() -1)
		{
			hscale_force_sensor_ai_a.Value = force_sensor_last_a;
			return;
		}

		//do not allow A to be higher than B (fix multiple possible problems)
		if(hscale_force_sensor_ai_a.Value > hscale_force_sensor_ai_b.Value)
			hscale_force_sensor_ai_b.Value = hscale_force_sensor_ai_a.Value;

		int count = Convert.ToInt32(hscale_force_sensor_ai_a.Value);
		if(count > 0 && count > fsAI.GetLength() -1)
		{
			//it could happen when there is really little data
			LogB.Information("hscale_force_sensor_ai_a outside of boundaries");
			return;
		}

		label_force_sensor_ai_time_a.Text = Math.Round(fsAI.GetTimeMS(count), 1).ToString();
		label_force_sensor_ai_force_a.Text = Math.Round(fsAI.GetForceAtCount(count), 1).ToString();

		if(fsAI.CalculedElasticPSAP)
		{
			label_force_sensor_ai_position_a.Text = Math.Round(fsAI.Position_l[count], 3).ToString();
			label_force_sensor_ai_speed_a.Text = Math.Round(fsAI.Speed_l[count], 3).ToString();
			label_force_sensor_ai_accel_a.Text = Math.Round(fsAI.Accel_l[count], 3).ToString();
			label_force_sensor_ai_power_a.Text = Math.Round(fsAI.Power_l[count], 3).ToString();
		} else {
			label_force_sensor_ai_position_a.Text = "";
			label_force_sensor_ai_speed_a.Text = "";
			label_force_sensor_ai_accel_a.Text = "";
			label_force_sensor_ai_power_a.Text = "";
		}

		if(count > 0 && count < fsAI.GetLength() -1)
			label_force_sensor_ai_rfd_a.Text = Math.Round(fsAI.CalculateRFD(count -1, count +1), 1).ToString();
		else
			label_force_sensor_ai_rfd_a.Text = "";

		force_sensor_last_a = Convert.ToInt32(hscale_force_sensor_ai_a.Value);

		if(forceSensorHScalesDoNotFollow)
		{
			forceSensorHScalesDoNotFollow = false;
			return;
		}

		//if chained move also B
		if(check_force_sensor_ai_chained.Active)
		{
			forceSensorHScalesDoNotFollow = true;
			hscale_force_sensor_ai_b.Value = hscale_force_sensor_ai_b.Value + diffWithPrevious;
			forceSensorHScalesDoNotFollow = false;
		}

		force_sensor_analyze_instant_calculate_params();

		forceSensorAnalyzeGeneralButtonHscaleZoomSensitiveness();
		force_sensor_ai_drawingarea_cairo.QueueDraw(); //will fire ExposeEvent
	}
	private void on_hscale_force_sensor_ai_b_value_changed (object o, EventArgs args)
	{
		if(fsAI == null || fsAI.GetLength() == 0)
			return;

		int diffWithPrevious = Convert.ToInt32(hscale_force_sensor_ai_b.Value) - force_sensor_last_b;

		//if chained and moving b to the left makes a lower than 0, do not move
		if(check_force_sensor_ai_chained.Active &&
				Convert.ToInt32(hscale_force_sensor_ai_a.Value) + diffWithPrevious <= 0)
		{
			hscale_force_sensor_ai_b.Value = force_sensor_last_b;
			return;
		}

		//do not allow B to be lower than A (fix multiple possible problems)
		if(hscale_force_sensor_ai_b.Value < hscale_force_sensor_ai_a.Value)
			hscale_force_sensor_ai_a.Value = hscale_force_sensor_ai_b.Value;

		int count = Convert.ToInt32(hscale_force_sensor_ai_b.Value);
		if(count > 0 && count > fsAI.GetLength() -1)
		{
			//it could happen when there is really little data
			LogB.Information("hscale_force_sensor_ai_b outside of boundaries");
			return;
		}

		label_force_sensor_ai_time_b.Text = Math.Round(fsAI.GetTimeMS(count), 1).ToString();
		label_force_sensor_ai_force_b.Text = Math.Round(fsAI.GetForceAtCount(count), 1).ToString();

		if(fsAI.CalculedElasticPSAP)
		{
			label_force_sensor_ai_position_b.Text = Math.Round(fsAI.Position_l[count], 3).ToString();
			label_force_sensor_ai_speed_b.Text = Math.Round(fsAI.Speed_l[count], 3).ToString();
			label_force_sensor_ai_accel_b.Text = Math.Round(fsAI.Accel_l[count], 3).ToString();
			label_force_sensor_ai_power_b.Text = Math.Round(fsAI.Power_l[count], 3).ToString();
		} else {
			label_force_sensor_ai_position_b.Text = "";
			label_force_sensor_ai_speed_b.Text = "";
			label_force_sensor_ai_accel_b.Text = "";
			label_force_sensor_ai_power_b.Text = "";
		}

		if(count > 0 && count < fsAI.GetLength() -1)
			label_force_sensor_ai_rfd_b.Text = Math.Round(fsAI.CalculateRFD(count -1, count +1), 1).ToString();
		else
			label_force_sensor_ai_rfd_b.Text = "";

		force_sensor_last_b = Convert.ToInt32(hscale_force_sensor_ai_b.Value);

		if(forceSensorHScalesDoNotFollow)
		{
			forceSensorHScalesDoNotFollow = false;
			return;
		}

		//if chained move also A
		if(check_force_sensor_ai_chained.Active)
		{
			forceSensorHScalesDoNotFollow = true;
			hscale_force_sensor_ai_a.Value = hscale_force_sensor_ai_a.Value + diffWithPrevious;
			forceSensorHScalesDoNotFollow = false;
		}

		force_sensor_analyze_instant_calculate_params();

		forceSensorAnalyzeGeneralButtonHscaleZoomSensitiveness();
		force_sensor_ai_drawingarea_cairo.QueueDraw(); //will fire ExposeEvent
	}

	private void on_check_force_sensor_ai_chained_clicked (object o, EventArgs args)
	{
		image_force_sensor_ai_chained_link.Visible = check_force_sensor_ai_chained.Active;
		image_force_sensor_ai_chained_link_off.Visible = ! check_force_sensor_ai_chained.Active;
	}

	private void forceSensorAnalyzeGeneralButtonHscaleZoomSensitiveness()
	{
		button_hscale_force_sensor_ai_a_first.Sensitive = hscale_force_sensor_ai_a.Value > 0;
		button_hscale_force_sensor_ai_a_pre.Sensitive = hscale_force_sensor_ai_a.Value > 0;
		button_hscale_force_sensor_ai_b_first.Sensitive = hscale_force_sensor_ai_b.Value > 0;
		button_hscale_force_sensor_ai_b_pre.Sensitive = hscale_force_sensor_ai_b.Value > 0;

		button_hscale_force_sensor_ai_a_last.Sensitive = hscale_force_sensor_ai_a.Value < fsAI.GetLength() -1;
		button_hscale_force_sensor_ai_a_post.Sensitive = hscale_force_sensor_ai_a.Value < fsAI.GetLength() -1;
		button_hscale_force_sensor_ai_b_last.Sensitive = hscale_force_sensor_ai_b.Value < fsAI.GetLength() -1;
		button_hscale_force_sensor_ai_b_post.Sensitive = hscale_force_sensor_ai_b.Value < fsAI.GetLength() -1;

		//diff have to be more than one pixel
		check_force_sensor_ai_zoom.Sensitive = (Math.Abs(hscale_force_sensor_ai_a.Value - hscale_force_sensor_ai_b.Value) > 1);
	}

	private void on_button_hscale_force_sensor_ai_a_first_clicked (object o, EventArgs args)
	{
		hscale_force_sensor_ai_a.Value = 0;
	}
	private void on_button_hscale_force_sensor_ai_a_pre_clicked (object o, EventArgs args)
	{
		hscale_force_sensor_ai_a.Value -= 1;
	}
	private void on_button_hscale_force_sensor_ai_a_post_clicked (object o, EventArgs args)
	{
		hscale_force_sensor_ai_a.Value += 1;
	}
	private void on_button_hscale_force_sensor_ai_a_last_clicked (object o, EventArgs args)
	{
		if(fsAI == null || fsAI.GetLength() < 2)
			return;

		hscale_force_sensor_ai_a.Value = fsAI.GetLength() -1;
	}

	private void on_button_hscale_force_sensor_ai_b_first_clicked (object o, EventArgs args)
	{
		hscale_force_sensor_ai_b.Value = 0;
	}
	private void on_button_hscale_force_sensor_ai_b_pre_clicked (object o, EventArgs args)
	{
		hscale_force_sensor_ai_b.Value -= 1;
	}
	private void on_button_hscale_force_sensor_ai_b_post_clicked (object o, EventArgs args)
	{
		hscale_force_sensor_ai_b.Value += 1;
	}

	private void on_button_hscale_force_sensor_ai_b_last_clicked (object o, EventArgs args)
	{
		if(fsAI == null || fsAI.GetLength() < 2)
			return;

		hscale_force_sensor_ai_b.Value = fsAI.GetLength() -1;
	}

	private void on_button_hscale_force_sensor_ai_a_pre_1s_clicked (object o, EventArgs args)
	{
		if(fsAI == null || fsAI.GetLength() == 0)
			return;

		int startA = Convert.ToInt32(hscale_force_sensor_ai_a.Value);
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
					hscale_force_sensor_ai_a.Value += (i - startA);
				else
					hscale_force_sensor_ai_a.Value += (i+1 - startA);

				return;
			}
		}
	}
	private void on_button_hscale_force_sensor_ai_a_post_1s_clicked (object o, EventArgs args)
	{
		if(fsAI == null || fsAI.GetLength() == 0)
			return;

		int startA = Convert.ToInt32(hscale_force_sensor_ai_a.Value);
		double startAMs = fsAI.GetTimeMS(startA);
		for(int i = startA; i < fsAI.GetLength() -1; i ++)
		{
			if(fsAI.GetTimeMS(i) - startAMs >= preferences.forceSensorAnalyzeABSliderIncrement * 1000)
			{
				//hscale_force_sensor_ai_a.Value += i - startA;
				if(MathUtil.PassedSampleIsCloserToCriteria (
						fsAI.GetTimeMS(i) - startAMs, fsAI.GetTimeMS(i-1) - startAMs,
						preferences.forceSensorAnalyzeABSliderIncrement * 1000))
					hscale_force_sensor_ai_a.Value += (i - startA);
				else
					hscale_force_sensor_ai_a.Value += (i-1 - startA);

				return;
			}
		}
	}

	private void on_button_hscale_force_sensor_ai_b_pre_1s_clicked (object o, EventArgs args)
	{
		if(fsAI == null || fsAI.GetLength() == 0)
			return;

		int startB = Convert.ToInt32(hscale_force_sensor_ai_b.Value);
		double startBMs = fsAI.GetTimeMS(startB);
		for(int i = startB; i > 0; i --)
		{
			if(startBMs - fsAI.GetTimeMS(i) >= preferences.forceSensorAnalyzeABSliderIncrement * 1000)
			{
				//hscale_force_sensor_ai_b.Value += i - startB;
				if(MathUtil.PassedSampleIsCloserToCriteria (
						startBMs - fsAI.GetTimeMS(i), startBMs - fsAI.GetTimeMS(i+1),
						preferences.forceSensorAnalyzeABSliderIncrement * 1000))
					hscale_force_sensor_ai_b.Value += (i - startB);
				else
					hscale_force_sensor_ai_b.Value += (i+1 - startB);

				return;
			}
		}
	}
	private void on_button_hscale_force_sensor_ai_b_post_1s_clicked (object o, EventArgs args)
	{
		if(fsAI == null || fsAI.GetLength() == 0)
			return;

		int startB = Convert.ToInt32(hscale_force_sensor_ai_b.Value);
		double startBMs = fsAI.GetTimeMS(startB);
		for(int i = startB; i < fsAI.GetLength() -1; i ++)
		{
			if(fsAI.GetTimeMS(i) - startBMs >= preferences.forceSensorAnalyzeABSliderIncrement * 1000)
			{
				//hscale_force_sensor_ai_b.Value += i - startB;
				if(MathUtil.PassedSampleIsCloserToCriteria (
						fsAI.GetTimeMS(i) - startBMs, fsAI.GetTimeMS(i-1) - startBMs,
						preferences.forceSensorAnalyzeABSliderIncrement * 1000))
					hscale_force_sensor_ai_b.Value += (i - startB);
				else
					hscale_force_sensor_ai_b.Value += (i-1 - startB);

				return;
			}
		}
	}

	private void manage_force_sensor_ai_table_visibilities()
	{
		bool visible = true;//checkbutton_force_sensor_ai_b.Active;

		bool visibleElastic = (visible && fsAI.CalculedElasticPSAP);

		hbox_force_sensor_ai_position.Visible = visibleElastic;
		hbox_force_sensor_ai_speed.Visible = visibleElastic;
		hbox_force_sensor_ai_accel.Visible = visibleElastic;
		hbox_force_sensor_ai_power.Visible = visibleElastic;

		label_force_sensor_ai_position_b.Visible = visibleElastic;
		label_force_sensor_ai_position_diff.Visible = visibleElastic;
		label_force_sensor_ai_speed_b.Visible = visibleElastic;
		label_force_sensor_ai_speed_diff.Visible = visibleElastic;
		label_force_sensor_ai_speed_average.Visible = visibleElastic;
		label_force_sensor_ai_speed_max.Visible = visibleElastic;

		label_force_sensor_ai_accel_b.Visible = visibleElastic;
		label_force_sensor_ai_accel_diff.Visible = visibleElastic;
		label_force_sensor_ai_accel_average.Visible = visibleElastic;
		label_force_sensor_ai_accel_max.Visible = visibleElastic;

		label_force_sensor_ai_power_b.Visible = visibleElastic;
		label_force_sensor_ai_power_diff.Visible = visibleElastic;
		label_force_sensor_ai_power_average.Visible = visibleElastic;
		label_force_sensor_ai_power_max.Visible = visibleElastic;

		if(visible && canDoForceSensorAnalyzeAB())
			button_force_sensor_analyze_AB_save.Visible = true;
		else
			button_force_sensor_analyze_AB_save.Visible = false;
	}

	private void force_sensor_analyze_instant_calculate_params()
	{
		//List<PointF> p_l = cairoGraphForceSensorSignalPoints_l; //to have shorter code
		int countA = Convert.ToInt32 (hscale_force_sensor_ai_a.Value);
		int countB = Convert.ToInt32 (hscale_force_sensor_ai_b.Value);

		//avoid problems of GTK misreading of hscale on a notebook change or load file
		if (countA < 0 || countA > fsAI.GetLength() -1 || countB < 0 || countB > fsAI.GetLength() -1)
			return;


		//old method
		double timeA = fsAI.GetTimeMS(countA);
		double timeB = fsAI.GetTimeMS(countB);
		double forceA = fsAI.GetForceAtCount(countA);
		double forceB = fsAI.GetForceAtCount(countB);
		bool success = fsAI.CalculateRangeParams(countA, countB, preferences.forceSensorAnalyzeMaxAVGInWindow);
		if(success) {
			label_force_sensor_ai_time_diff.Text = Math.Round(timeB - timeA, 1).ToString();
			label_force_sensor_ai_force_diff.Text = Math.Round(forceB - forceA, 1).ToString();

			if(countA != countB) {
				label_force_sensor_ai_force_average.Text = Math.Round(fsAI.ForceAVG, 1).ToString();
				label_force_sensor_ai_force_max.Text = Math.Round(fsAI.ForceMAX, 1).ToString();

				if(fsAI.Gmaiw.Error == "")
					label_force_sensor_ai_max_avg_in_window_values.Text = Math.Round(fsAI.Gmaiw.Max, 1).ToString();
				else
					label_force_sensor_ai_max_avg_in_window_values.Text = "----";
			} else {
				label_force_sensor_ai_force_average.Text = "";
				label_force_sensor_ai_force_max.Text = "";
				label_force_sensor_ai_max_avg_in_window_values.Text = "";
			}
		}

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
			label_force_sensor_ai_position_diff.Text = Math.Round(positionB - positionA, 3).ToString();

			//LogB.Information ("fcr position_diff = " + Math.Round (cairoGraphForceSensorSignalPointsDispl_l[countB].Y - cairoGraphForceSensorSignalPointsDispl_l[countA].Y, 3).ToString());

			double speedA = fsAI.Speed_l[countA];
			double speedB = fsAI.Speed_l[countB];
			label_force_sensor_ai_speed_diff.Text = Math.Round(speedB - speedA, 3).ToString();
			//LogB.Information ("fcr speed_diff = " + Math.Round (cairoGraphForceSensorSignalPointsSpeed_l[countB].Y - cairoGraphForceSensorSignalPointsSpeed_l[countA].Y, 3).ToString());

			if(countA != countB) {
				label_force_sensor_ai_speed_average.Text = Math.Round(fsAI.SpeedAVG, 3).ToString();
				label_force_sensor_ai_speed_max.Text = Math.Round(fsAI.SpeedMAX, 3).ToString();
				label_force_sensor_ai_accel_average.Text = Math.Round(fsAI.AccelAVG, 3).ToString();
				label_force_sensor_ai_accel_max.Text = Math.Round(fsAI.AccelMAX, 3).ToString();
				label_force_sensor_ai_power_average.Text = Math.Round(fsAI.PowerAVG, 3).ToString();
				label_force_sensor_ai_power_max.Text = Math.Round(fsAI.PowerMAX, 3).ToString();
			} else {
				label_force_sensor_ai_speed_average.Text = "";
				label_force_sensor_ai_speed_max.Text = "";
				label_force_sensor_ai_accel_average.Text = "";
				label_force_sensor_ai_accel_max.Text = "";
				label_force_sensor_ai_power_average.Text = "";
				label_force_sensor_ai_power_max.Text = "";
			}

			double accelA = fsAI.Accel_l[countA];
			double accelB = fsAI.Accel_l[countB];
			label_force_sensor_ai_accel_diff.Text = Math.Round(accelB - accelA, 3).ToString();

			double powerA = fsAI.Power_l[countA];
			double powerB = fsAI.Power_l[countB];
			label_force_sensor_ai_power_diff.Text = Math.Round(powerB - powerA, 3).ToString();
		}

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

		if(rfdADefined)
			label_force_sensor_ai_rfd_a.Text = rfdA.ToString();
		else
			label_force_sensor_ai_rfd_a.Text = "";

		if(rfdBDefined)
			label_force_sensor_ai_rfd_b.Text = rfdB.ToString();
		else
			label_force_sensor_ai_rfd_b.Text = "";

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
			label_force_sensor_ai_rfd_diff.Text = Math.Round(rfdB - rfdA, 1).ToString();

			// 2) Average:
			label_force_sensor_ai_rfd_average.Text = Math.Round(fsAI.CalculateRFD(countA, countB), 1).ToString();

			// 3) max
			fsAI.CalculateMaxRFDInRange(countA, countB);

			//LogB.Information(string.Format("fsAI.LastRFDMax: {0}", fsAI.LastRFDMax));
			//LogB.Information(string.Format("fsAI.LastRFDMaxCount: {0}", fsAI.LastRFDMaxCount));

			label_force_sensor_ai_rfd_max.Text = Math.Round(fsAI.LastRFDMax, 1).ToString();
		} else {
			label_force_sensor_ai_rfd_diff.Text = "0";
			label_force_sensor_ai_rfd_average.Text = "";
			label_force_sensor_ai_rfd_max.Text = "";
		}

		table_force_sensor_ai_impulse_variability_and_feedback.Visible = (countA != countB);

		if(preferences.forceSensorVariabilityMethod == Preferences.VariabilityMethodEnum.RMSSD)
			label_force_sensor_ai_variability_method.Text = "RMSSD";
		else if(preferences.forceSensorVariabilityMethod == Preferences.VariabilityMethodEnum.CVRMSSD)
			label_force_sensor_ai_variability_method.Text = "cvRMSSD";
		else
			label_force_sensor_ai_variability_method.Text = "Old method";

		if(preferences.forceSensorVariabilityMethod == Preferences.VariabilityMethodEnum.CVRMSSD)
			label_force_sensor_ai_variability_units.Text = "%";
		else
			label_force_sensor_ai_variability_units.Text = "N";

		if(countA != countB)
		{
			// 10) calculate impulse
			//label_force_sensor_ai_impulse_values.Text = Math.Round (ForceCalcs.GetImpulse (
			//			p_l, countA, countB), 1).ToString();
			//	again this should be on fsAI:
			label_force_sensor_ai_impulse_values.Text = Math.Round (fsAI.CalculateImpulse (
						countA, countB), 1).ToString();

			// 11) calculate variability
			int feedbackF = preferences.forceSensorCaptureFeedbackAt;

			fsAI.CalculateVariabilityAndAccuracy (countA, countB, feedbackF,
					preferences.forceSensorVariabilityMethod, preferences.forceSensorVariabilityLag);
			LogB.Information (string.Format ("vaa variability: {0}, feedbackDiff: {1}", fsAI.Vaa.Variability, fsAI.Vaa.FeedbackDiff));

			label_force_sensor_ai_variability_values.Text = Math.Round (fsAI.Vaa.Variability, 3).ToString();

			// 12) calculate Accuracy (Feedback difference)
			//if(preferences.forceSensorCaptureFeedbackActive && feedbackF > 0)
			if(preferences.forceSensorCaptureFeedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.RECTANGLE && feedbackF > 0)
			{
				label_force_sensor_ai_feedback_values.Text = Math.Round (fsAI.Vaa.FeedbackDiff, 3).ToString();
				label_force_sensor_ai_feedback.Visible = true;
				hbox_force_sensor_ai_feedback.Visible = true;
			} else {
				label_force_sensor_ai_feedback.Visible = false;
				hbox_force_sensor_ai_feedback.Visible = false;
			}
		}
	}

	private bool canDoForceSensorAnalyzeAB()
	{
		return (Util.FileExists(lastForceSensorFullPath) &&
				label_force_sensor_ai_time_diff.Visible &&
				label_force_sensor_ai_time_diff.Text != null &&
				Util.IsNumber(label_force_sensor_ai_time_diff.Text, true) );
	}

	private void on_button_force_sensor_analyze_AB_save_clicked (object o, EventArgs args)
	{
		if (label_force_sensor_ai_time_a.Text == label_force_sensor_ai_time_b.Text)
		{
			new DialogMessage(Constants.MessageTypes.WARNING, "A and B cannot be the same");
			return;
		}

		if (canDoForceSensorAnalyzeAB())
			checkFile(Constants.CheckFileOp.FORCESENSOR_ANALYZE_SAVE_AB);
		else {
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileNotFoundStr());
			return;
		}
	}
	void on_button_force_sensor_save_AB_file_selected (string selectedFileName)
	{
		fsAI.ExportToCSV(getLowestForceSensorABScale(), getHighestForceSensorABScale(),
				selectedFileName, preferences.CSVExportDecimalSeparator);
	}

	private int getLowestForceSensorABScale()
	{
		if(Convert.ToInt32(hscale_force_sensor_ai_a.Value) <= Convert.ToInt32(hscale_force_sensor_ai_b.Value))
			return Convert.ToInt32(hscale_force_sensor_ai_a.Value);
		else
			return Convert.ToInt32(hscale_force_sensor_ai_b.Value);
	}
	private int getHighestForceSensorABScale()
	{
		if(Convert.ToInt32(hscale_force_sensor_ai_a.Value) <= Convert.ToInt32(hscale_force_sensor_ai_b.Value))
			return Convert.ToInt32(hscale_force_sensor_ai_b.Value);
		else
			return Convert.ToInt32(hscale_force_sensor_ai_a.Value);
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
		button_force_sensor_analyze_analyze = (Gtk.Button) builder.GetObject ("button_force_sensor_analyze_analyze");
		label_force_sensor_analyze = (Gtk.Label) builder.GetObject ("label_force_sensor_analyze");
		image_force_sensor_graph = (Gtk.Image) builder.GetObject ("image_force_sensor_graph");
		viewport_force_sensor_graph = (Gtk.Viewport) builder.GetObject ("viewport_force_sensor_graph");
		button_force_sensor_image_save_rfd_auto = (Gtk.Button) builder.GetObject ("button_force_sensor_image_save_rfd_auto");
		button_force_sensor_image_save_rfd_manual = (Gtk.Button) builder.GetObject ("button_force_sensor_image_save_rfd_manual");
		//scrolledwindow_force_sensor_ai = (Gtk.ScrolledWindow) builder.GetObject ("scrolledwindow_force_sensor_ai");
		button_force_sensor_analyze_AB_save = (Gtk.Button) builder.GetObject ("button_force_sensor_analyze_AB_save");
		check_force_sensor_ai_chained = (Gtk.CheckButton) builder.GetObject ("check_force_sensor_ai_chained");
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

		hbox_force_sensor_ai_position = (Gtk.HBox) builder.GetObject ("hbox_force_sensor_ai_position");
		hbox_force_sensor_ai_speed = (Gtk.HBox) builder.GetObject ("hbox_force_sensor_ai_speed");
		hbox_force_sensor_ai_accel = (Gtk.HBox) builder.GetObject ("hbox_force_sensor_ai_accel");
		hbox_force_sensor_ai_power = (Gtk.HBox) builder.GetObject ("hbox_force_sensor_ai_power");

		table_force_sensor_ai_impulse_variability_and_feedback = (Gtk.Table) builder.GetObject ("table_force_sensor_ai_impulse_variability_and_feedback");
		label_force_sensor_ai_feedback = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_feedback");
		hbox_force_sensor_ai_feedback = (Gtk.HBox) builder.GetObject ("hbox_force_sensor_ai_feedback");
		label_force_sensor_ai_impulse_values = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_impulse_values");
		label_force_sensor_ai_variability_values = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_variability_values");
		label_force_sensor_ai_feedback_values = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_feedback_values");
		label_force_sensor_ai_variability_method = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_variability_method");
		label_force_sensor_ai_variability_units = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_variability_units");
		label_force_sensor_ai_max_avg_in_window = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_max_avg_in_window");

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
		hscale_force_sensor_ai_a = (Gtk.HScale) builder.GetObject ("hscale_force_sensor_ai_a");
		hscale_force_sensor_ai_b = (Gtk.HScale) builder.GetObject ("hscale_force_sensor_ai_b");
		label_force_sensor_ai_time_a = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_time_a");
		label_force_sensor_ai_force_a = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_force_a");
		label_force_sensor_ai_rfd_a = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_rfd_a");
		label_force_sensor_ai_position_a = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_position_a");
		label_force_sensor_ai_speed_a = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_speed_a");
		label_force_sensor_ai_accel_a = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_accel_a");
		label_force_sensor_ai_power_a = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_power_a");
		//hbox_buttons_scale_force_sensor_ai_b = (Gtk.HBox) builder.GetObject ("hbox_buttons_scale_force_sensor_ai_b");
		label_force_sensor_ai_position_b = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_position_b");
		label_force_sensor_ai_position_diff = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_position_diff");
		label_force_sensor_ai_speed_b = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_speed_b");
		label_force_sensor_ai_speed_diff = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_speed_diff");
		label_force_sensor_ai_speed_average = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_speed_average");
		label_force_sensor_ai_speed_max = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_speed_max");
		label_force_sensor_ai_accel_b = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_accel_b");
		label_force_sensor_ai_accel_diff = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_accel_diff");
		label_force_sensor_ai_accel_average = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_accel_average");
		label_force_sensor_ai_accel_max = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_accel_max");
		label_force_sensor_ai_power_b = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_power_b");
		label_force_sensor_ai_power_diff = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_power_diff");
		label_force_sensor_ai_power_average = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_power_average");
		label_force_sensor_ai_power_max = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_power_max");
		label_force_sensor_ai_time_b = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_time_b");
		label_force_sensor_ai_time_diff = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_time_diff");
		label_force_sensor_ai_force_b = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_force_b");
		label_force_sensor_ai_force_diff = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_force_diff");
		label_force_sensor_ai_force_average = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_force_average");
		label_force_sensor_ai_force_max = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_force_max");
		label_force_sensor_ai_rfd_b = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_rfd_b");
		label_force_sensor_ai_rfd_diff = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_rfd_diff");
		label_force_sensor_ai_rfd_average = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_rfd_average");
		label_force_sensor_ai_rfd_max = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_rfd_max");
		label_force_sensor_ai_max_avg_in_window_values = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_max_avg_in_window_values");

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
