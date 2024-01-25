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
	Gtk.RadioButton radio_signal_analyze_current_set;
	//Gtk.RadioButton radio_signal_analyze_export_csv;
	Gtk.Image image_force_sensor_analyze_individual_current_set;
	Gtk.Image image_force_sensor_analyze_individual_current_session;
	Gtk.Image image_force_sensor_analyze_individual_all_sessions;
	Gtk.Image image_force_sensor_analyze_groupal_current_session;

	Gtk.Notebook notebook_ai_top;
	Gtk.HBox hbox_force_general_analysis;
	Gtk.Button button_signal_analyze_load_ab;
	Gtk.Button button_signal_analyze_load_cd;
	Gtk.Button button_ai_move_cd_pre;
	Gtk.Button button_force_sensor_image_save_rfd_manual;
	Gtk.Button button_force_sensor_analyze_AB_save;
	Gtk.Button button_force_sensor_analyze_CD_save;

	Gtk.RadioButton radio_force_rfd_search_optimized_ab;
	Gtk.RadioButton radio_force_rfd_use_ab_range;
	Gtk.SpinButton spin_force_duration_seconds;
	Gtk.RadioButton radio_force_duration_seconds;
	Gtk.HBox hbox_force_rfd_duration_percent;
	Gtk.RadioButton radio_force_rfd_duration_percent;
	Gtk.SpinButton spin_force_rfd_duration_percent;

	//analyze options
	Gtk.Notebook notebook_force_sensor_rfd_options;
	Gtk.HBox hbox_ai_export_top_modes;
//	Gtk.HBox hbox_force_sensor_analyze_automatic_options;
//	Gtk.Notebook notebook_force_analyze_automatic;
	/*
	Gtk.HBox hbox_force_show_1;
	Gtk.HBox hbox_force_show_2;
	Gtk.HBox hbox_force_show_3;
	Gtk.HBox hbox_force_show_4;
	Gtk.HBox hbox_force_show_5;
	Gtk.HBox hbox_force_show_6;
	Gtk.HBox hbox_force_show_7;
	Gtk.HBox hbox_force_show_8;
	Gtk.HBox hbox_force_show_9;
	Gtk.HBox hbox_force_show_10;
	*/
	Gtk.VBox vbox_force_rfd_duration_end;
	Gtk.HBox hbox_force_1;
	Gtk.HBox hbox_force_2;
	Gtk.HBox hbox_force_3;
	Gtk.HBox hbox_force_4;
	Gtk.HBox hbox_force_5;
	Gtk.HBox hbox_force_6;
	Gtk.HBox hbox_force_7;
	Gtk.HBox hbox_force_8;
	Gtk.HBox hbox_force_9;
	Gtk.HBox hbox_force_10;
	Gtk.HBox hbox_force_impulse;
	Gtk.CheckButton check_force_1;
	Gtk.CheckButton check_force_2;
	Gtk.CheckButton check_force_3;
	Gtk.CheckButton check_force_4;
	Gtk.CheckButton check_force_5;
	Gtk.CheckButton check_force_6;
	Gtk.CheckButton check_force_7;
	Gtk.CheckButton check_force_8;
	Gtk.CheckButton check_force_9;
	Gtk.CheckButton check_force_10;
	Gtk.CheckButton check_force_impulse;
	Gtk.HBox hbox_force_1_at_ms;
	Gtk.HBox hbox_force_2_at_ms;
	Gtk.HBox hbox_force_3_at_ms;
	Gtk.HBox hbox_force_4_at_ms;
	Gtk.HBox hbox_force_5_at_ms;
	Gtk.HBox hbox_force_6_at_ms;
	Gtk.HBox hbox_force_7_at_ms;
	Gtk.HBox hbox_force_8_at_ms;
	Gtk.HBox hbox_force_9_at_ms;
	Gtk.HBox hbox_force_10_at_ms;
	Gtk.HBox hbox_force_1_at_percent;
	Gtk.HBox hbox_force_2_at_percent;
	Gtk.HBox hbox_force_3_at_percent;
	Gtk.HBox hbox_force_4_at_percent;
	Gtk.HBox hbox_force_5_at_percent;
	Gtk.HBox hbox_force_6_at_percent;
	Gtk.HBox hbox_force_7_at_percent;
	Gtk.HBox hbox_force_8_at_percent;
	Gtk.HBox hbox_force_9_at_percent;
	Gtk.HBox hbox_force_10_at_percent;
	Gtk.HBox hbox_force_impulse_until_percent;
	Gtk.HBox hbox_force_1_from_to;
	Gtk.HBox hbox_force_2_from_to;
	Gtk.HBox hbox_force_3_from_to;
	Gtk.HBox hbox_force_4_from_to;
	Gtk.HBox hbox_force_5_from_to;
	Gtk.HBox hbox_force_6_from_to;
	Gtk.HBox hbox_force_7_from_to;
	Gtk.HBox hbox_force_8_from_to;
	Gtk.HBox hbox_force_9_from_to;
	Gtk.HBox hbox_force_10_from_to;
	Gtk.HBox hbox_force_1_in_x_ms;
	Gtk.HBox hbox_force_2_in_x_ms;
	Gtk.HBox hbox_force_3_in_x_ms;
	Gtk.HBox hbox_force_4_in_x_ms;
	Gtk.HBox hbox_force_5_in_x_ms;
	Gtk.HBox hbox_force_6_in_x_ms;
	Gtk.HBox hbox_force_7_in_x_ms;
	Gtk.HBox hbox_force_8_in_x_ms;
	Gtk.HBox hbox_force_9_in_x_ms;
	Gtk.HBox hbox_force_10_in_x_ms;
	Gtk.SpinButton spinbutton_force_1_at_ms;
	Gtk.SpinButton spinbutton_force_2_at_ms;
	Gtk.SpinButton spinbutton_force_3_at_ms;
	Gtk.SpinButton spinbutton_force_4_at_ms;
	Gtk.SpinButton spinbutton_force_5_at_ms;
	Gtk.SpinButton spinbutton_force_6_at_ms;
	Gtk.SpinButton spinbutton_force_7_at_ms;
	Gtk.SpinButton spinbutton_force_8_at_ms;
	Gtk.SpinButton spinbutton_force_9_at_ms;
	Gtk.SpinButton spinbutton_force_10_at_ms;
	Gtk.HBox hbox_force_impulse_from_to;
	Gtk.SpinButton spinbutton_force_1_at_percent;
	Gtk.SpinButton spinbutton_force_2_at_percent;
	Gtk.SpinButton spinbutton_force_3_at_percent;
	Gtk.SpinButton spinbutton_force_4_at_percent;
	Gtk.SpinButton spinbutton_force_5_at_percent;
	Gtk.SpinButton spinbutton_force_6_at_percent;
	Gtk.SpinButton spinbutton_force_7_at_percent;
	Gtk.SpinButton spinbutton_force_8_at_percent;
	Gtk.SpinButton spinbutton_force_9_at_percent;
	Gtk.SpinButton spinbutton_force_10_at_percent;
	Gtk.SpinButton spinbutton_force_impulse_until_percent;
	Gtk.SpinButton spinbutton_force_1_from;
	Gtk.SpinButton spinbutton_force_2_from;
	Gtk.SpinButton spinbutton_force_3_from;
	Gtk.SpinButton spinbutton_force_4_from;
	Gtk.SpinButton spinbutton_force_5_from;
	Gtk.SpinButton spinbutton_force_6_from;
	Gtk.SpinButton spinbutton_force_7_from;
	Gtk.SpinButton spinbutton_force_8_from;
	Gtk.SpinButton spinbutton_force_9_from;
	Gtk.SpinButton spinbutton_force_10_from;
	Gtk.SpinButton spinbutton_force_impulse_from;
	Gtk.SpinButton spinbutton_force_1_to;
	Gtk.SpinButton spinbutton_force_2_to;
	Gtk.SpinButton spinbutton_force_3_to;
	Gtk.SpinButton spinbutton_force_4_to;
	Gtk.SpinButton spinbutton_force_5_to;
	Gtk.SpinButton spinbutton_force_6_to;
	Gtk.SpinButton spinbutton_force_7_to;
	Gtk.SpinButton spinbutton_force_8_to;
	Gtk.SpinButton spinbutton_force_9_to;
	Gtk.SpinButton spinbutton_force_10_to;
	Gtk.SpinButton spinbutton_force_impulse_to;
	Gtk.SpinButton spinbutton_force_1_in_x_ms;
	Gtk.SpinButton spinbutton_force_2_in_x_ms;
	Gtk.SpinButton spinbutton_force_3_in_x_ms;
	Gtk.SpinButton spinbutton_force_4_in_x_ms;
	Gtk.SpinButton spinbutton_force_5_in_x_ms;
	Gtk.SpinButton spinbutton_force_6_in_x_ms;
	Gtk.SpinButton spinbutton_force_7_in_x_ms;
	Gtk.SpinButton spinbutton_force_8_in_x_ms;
	Gtk.SpinButton spinbutton_force_9_in_x_ms;
	Gtk.SpinButton spinbutton_force_10_in_x_ms;

	Gtk.HBox hbox_force_sensor_analyze_ai_sliders_and_buttons;
	Gtk.Box box_force_sensor_analyze_magnitudes;
	Gtk.CheckButton check_force_sensor_analyze_show_distance;
	Gtk.CheckButton check_force_sensor_analyze_show_speed;
	Gtk.CheckButton check_force_sensor_analyze_show_power;
	Gtk.Image image_force_sensor_analyze_show_distance;
	Gtk.Image image_force_sensor_analyze_show_speed;
	Gtk.Image image_force_sensor_analyze_show_power;
	Gtk.DrawingArea ai_drawingarea_cairo;
	Gtk.Box box_force_sensor_ai_a;
	Gtk.Box box_force_sensor_ai_b;
	Gtk.Box box_force_sensor_ai_c;
	Gtk.Box box_force_sensor_ai_d;
	Gtk.Label label_force_sensor_ai_zoom_abcd;
	Gtk.TreeView treeview_ai_AB;
	Gtk.TreeView treeview_ai_CD;
	Gtk.TreeView treeview_force_sensor_ai_other;

	Gtk.ComboBoxText combo_force_1_function;
	Gtk.ComboBoxText combo_force_2_function;
	Gtk.ComboBoxText combo_force_3_function;
	Gtk.ComboBoxText combo_force_4_function;
	Gtk.ComboBoxText combo_force_5_function;
	Gtk.ComboBoxText combo_force_6_function;
	Gtk.ComboBoxText combo_force_7_function;
	Gtk.ComboBoxText combo_force_8_function;
	Gtk.ComboBoxText combo_force_9_function;
	Gtk.ComboBoxText combo_force_10_function;
	Gtk.ComboBoxText combo_force_impulse_function;
	Gtk.ComboBoxText combo_force_1_type;
	Gtk.ComboBoxText combo_force_2_type;
	Gtk.ComboBoxText combo_force_3_type;
	Gtk.ComboBoxText combo_force_4_type;
	Gtk.ComboBoxText combo_force_5_type;
	Gtk.ComboBoxText combo_force_6_type;
	Gtk.ComboBoxText combo_force_7_type;
	Gtk.ComboBoxText combo_force_8_type;
	Gtk.ComboBoxText combo_force_9_type;
	Gtk.ComboBoxText combo_force_10_type;
	Gtk.ComboBoxText combo_force_impulse_type;
	// <---- at glade


	private RepetitionMouseLimits fsAIRepetitionMouseLimits;
	private RepetitionMouseLimitsWithSamples fsAIRepetitionMouseLimitsCairo;
	private List<ForceSensorRepetition> rep_lZoomAppliedCairo;

	private enum notebook_ai_top_pages { CURRENTSETSIGNAL, CURRENTSETMODEL, CURRENTSESSION, AUTOMATICOPTIONS }

	private string signalSuperpose2SetsCDPersonName = "";

	/*
	 * analyze options -------------------------->
	 */

	private void forceSensorAnalyzeOptionsSensitivity(bool s) //s for sensitive. When show options frame is ! s
	{
		button_ai_model_options.Sensitive = s;
		button_signal_analyze_load_ab.Sensitive = s;
		button_signal_analyze_load_cd.Sensitive = s;
		if (! s)
			button_ai_move_cd_pre.Sensitive = false;
		else
			button_ai_move_cd_pre_set_sensitivity ();

		if(s)
			button_ai_model.Sensitive = button_ai_model_was_sensitive;
		else {
			button_ai_model_was_sensitive = button_ai_model.Sensitive;
			button_ai_model.Sensitive = false;
		}

		menus_and_mode_sensitive(s);
		hbox_contacts_sup_capture_analyze_two_buttons.Sensitive = s;
		hbox_top_person.Sensitive = s;
	}

	private void on_button_force_sensor_ai_model_options_clicked ()
	{
		forceSensorAnalyzeOptionsSensitivity(false);
	}

	private void on_button_force_sensor_ai_model_options_close_clicked ()
	{
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

	private void on_button_force_sensor_analyze_model_clicked (object o, EventArgs args)
	{
		notebook_ai_top.CurrentPage = Convert.ToInt32(notebook_ai_top_pages.CURRENTSETMODEL);

		if(! Util.FileExists(lastForceSensorFullPath))
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileNotFoundStr());
			return;
		}

		if(lastForceSensorFullPath != null && lastForceSensorFullPath != "")
			forceSensorCopyTempAndDoGraphs(forceSensorGraphsEnum.RFD);
	}

	private void on_button_ai_model_back_to_signal_clicked (object o, EventArgs args)
	{
		notebook_ai_top.CurrentPage = Convert.ToInt32(notebook_ai_top_pages.CURRENTSETSIGNAL);
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
		hbox_force_5.Visible = (check_force_5.Active);
		hbox_force_6.Visible = (check_force_6.Active);
		hbox_force_7.Visible = (check_force_7.Active);
		hbox_force_8.Visible = (check_force_8.Active);
		hbox_force_9.Visible = (check_force_9.Active);
		hbox_force_10.Visible = (check_force_10.Active);
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
		UtilGtk.ComboUpdate(combo_force_5_function, ForceSensorRFD.FunctionsArray(true), "");
		UtilGtk.ComboUpdate(combo_force_6_function, ForceSensorRFD.FunctionsArray(true), "");
		UtilGtk.ComboUpdate(combo_force_7_function, ForceSensorRFD.FunctionsArray(true), "");
		UtilGtk.ComboUpdate(combo_force_8_function, ForceSensorRFD.FunctionsArray(true), "");
		UtilGtk.ComboUpdate(combo_force_9_function, ForceSensorRFD.FunctionsArray(true), "");
		UtilGtk.ComboUpdate(combo_force_10_function, ForceSensorRFD.FunctionsArray(true), "");
		UtilGtk.ComboUpdate(combo_force_impulse_function, ForceSensorImpulse.FunctionsArray(true), "");

		UtilGtk.ComboUpdate(combo_force_1_type, ForceSensorRFD.TypesArray(true), "");
		UtilGtk.ComboUpdate(combo_force_2_type, ForceSensorRFD.TypesArray(true), "");
		UtilGtk.ComboUpdate(combo_force_3_type, ForceSensorRFD.TypesArray(true), "");
		UtilGtk.ComboUpdate(combo_force_4_type, ForceSensorRFD.TypesArray(true), "");
		UtilGtk.ComboUpdate(combo_force_5_type, ForceSensorRFD.TypesArray(true), "");
		UtilGtk.ComboUpdate(combo_force_6_type, ForceSensorRFD.TypesArray(true), "");
		UtilGtk.ComboUpdate(combo_force_7_type, ForceSensorRFD.TypesArray(true), "");
		UtilGtk.ComboUpdate(combo_force_8_type, ForceSensorRFD.TypesArray(true), "");
		UtilGtk.ComboUpdate(combo_force_9_type, ForceSensorRFD.TypesArray(true), "");
		UtilGtk.ComboUpdate(combo_force_10_type, ForceSensorRFD.TypesArray(true), "");
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
					hbox_force_1_at_ms, hbox_force_1_at_percent, hbox_force_1_from_to, hbox_force_1_in_x_ms);
		else if(combo == combo_force_2_type)
			combo_force_visibility(
					UtilGtk.ComboGetActive(combo_force_2_type),
					hbox_force_2_at_ms, hbox_force_2_at_percent, hbox_force_2_from_to, hbox_force_2_in_x_ms);
		else if(combo == combo_force_3_type)
			combo_force_visibility(
					UtilGtk.ComboGetActive(combo_force_3_type),
					hbox_force_3_at_ms, hbox_force_3_at_percent, hbox_force_3_from_to, hbox_force_3_in_x_ms);
		else if(combo == combo_force_4_type)
			combo_force_visibility(
					UtilGtk.ComboGetActive(combo_force_4_type),
					hbox_force_4_at_ms, hbox_force_4_at_percent, hbox_force_4_from_to, hbox_force_4_in_x_ms);
		else if(combo == combo_force_5_type)
			combo_force_visibility(
					UtilGtk.ComboGetActive(combo_force_5_type),
					hbox_force_5_at_ms, hbox_force_5_at_percent, hbox_force_5_from_to, hbox_force_5_in_x_ms);
		else if(combo == combo_force_6_type)
			combo_force_visibility(
					UtilGtk.ComboGetActive(combo_force_6_type),
					hbox_force_6_at_ms, hbox_force_6_at_percent, hbox_force_6_from_to, hbox_force_6_in_x_ms);
		else if(combo == combo_force_7_type)
			combo_force_visibility(
					UtilGtk.ComboGetActive(combo_force_7_type),
					hbox_force_7_at_ms, hbox_force_7_at_percent, hbox_force_7_from_to, hbox_force_7_in_x_ms);
		else if(combo == combo_force_8_type)
			combo_force_visibility(
					UtilGtk.ComboGetActive(combo_force_8_type),
					hbox_force_8_at_ms, hbox_force_8_at_percent, hbox_force_8_from_to, hbox_force_8_in_x_ms);
		else if(combo == combo_force_9_type)
			combo_force_visibility(
					UtilGtk.ComboGetActive(combo_force_9_type),
					hbox_force_9_at_ms, hbox_force_9_at_percent, hbox_force_9_from_to, hbox_force_9_in_x_ms);
		else if(combo == combo_force_10_type)
			combo_force_visibility(
					UtilGtk.ComboGetActive(combo_force_10_type),
					hbox_force_10_at_ms, hbox_force_10_at_percent, hbox_force_10_from_to, hbox_force_10_in_x_ms);
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

		setRFDValue (rfdList[4], check_force_5, combo_force_5_function, combo_force_5_type,
				hbox_force_5_at_ms, spinbutton_force_5_at_ms,
				hbox_force_5_at_percent, spinbutton_force_5_at_percent,
				hbox_force_5_from_to, spinbutton_force_5_from, spinbutton_force_5_to,
				hbox_force_5_in_x_ms, spinbutton_force_5_in_x_ms);

		setRFDValue (rfdList[5], check_force_6, combo_force_6_function, combo_force_6_type,
				hbox_force_6_at_ms, spinbutton_force_6_at_ms,
				hbox_force_6_at_percent, spinbutton_force_6_at_percent,
				hbox_force_6_from_to, spinbutton_force_6_from, spinbutton_force_6_to,
				hbox_force_6_in_x_ms, spinbutton_force_6_in_x_ms);

		setRFDValue (rfdList[6], check_force_7, combo_force_7_function, combo_force_7_type,
				hbox_force_7_at_ms, spinbutton_force_7_at_ms,
				hbox_force_7_at_percent, spinbutton_force_7_at_percent,
				hbox_force_7_from_to, spinbutton_force_7_from, spinbutton_force_7_to,
				hbox_force_7_in_x_ms, spinbutton_force_7_in_x_ms);

		setRFDValue (rfdList[7], check_force_8, combo_force_8_function, combo_force_8_type,
				hbox_force_8_at_ms, spinbutton_force_8_at_ms,
				hbox_force_8_at_percent, spinbutton_force_8_at_percent,
				hbox_force_8_from_to, spinbutton_force_8_from, spinbutton_force_8_to,
				hbox_force_8_in_x_ms, spinbutton_force_8_in_x_ms);

		setRFDValue (rfdList[8], check_force_9, combo_force_9_function, combo_force_9_type,
				hbox_force_9_at_ms, spinbutton_force_9_at_ms,
				hbox_force_9_at_percent, spinbutton_force_9_at_percent,
				hbox_force_9_from_to, spinbutton_force_9_from, spinbutton_force_9_to,
				hbox_force_9_in_x_ms, spinbutton_force_9_in_x_ms);

		setRFDValue (rfdList[9], check_force_10, combo_force_10_function, combo_force_10_type,
				hbox_force_10_at_ms, spinbutton_force_10_at_ms,
				hbox_force_10_at_percent, spinbutton_force_10_at_percent,
				hbox_force_10_from_to, spinbutton_force_10_from, spinbutton_force_10_to,
				hbox_force_10_in_x_ms, spinbutton_force_10_in_x_ms);
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
		l.Add(getRFDValue("RFD5", check_force_5, combo_force_5_function, combo_force_5_type,
					spinbutton_force_5_at_ms, spinbutton_force_5_at_percent,
					spinbutton_force_5_from, spinbutton_force_5_to,
					spinbutton_force_5_in_x_ms));
		l.Add(getRFDValue("RFD6", check_force_6, combo_force_6_function, combo_force_6_type,
					spinbutton_force_6_at_ms, spinbutton_force_6_at_percent,
					spinbutton_force_6_from, spinbutton_force_6_to,
					spinbutton_force_6_in_x_ms));
		l.Add(getRFDValue("RFD7", check_force_7, combo_force_7_function, combo_force_7_type,
					spinbutton_force_7_at_ms, spinbutton_force_7_at_percent,
					spinbutton_force_7_from, spinbutton_force_7_to,
					spinbutton_force_7_in_x_ms));
		l.Add(getRFDValue("RFD8", check_force_8, combo_force_8_function, combo_force_8_type,
					spinbutton_force_8_at_ms, spinbutton_force_8_at_percent,
					spinbutton_force_8_from, spinbutton_force_8_to,
					spinbutton_force_8_in_x_ms));
		l.Add(getRFDValue("RFD9", check_force_9, combo_force_9_function, combo_force_9_type,
					spinbutton_force_9_at_ms, spinbutton_force_9_at_percent,
					spinbutton_force_9_from, spinbutton_force_9_to,
					spinbutton_force_9_in_x_ms));
		l.Add(getRFDValue("RFD10", check_force_10, combo_force_10_function, combo_force_10_type,
					spinbutton_force_10_at_ms, spinbutton_force_10_at_percent,
					spinbutton_force_10_from, spinbutton_force_10_to,
					spinbutton_force_10_in_x_ms));
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
		label_hscale_ai_a_pre_1s.Text = string.Format("{0}s", preferences.forceSensorAnalyzeABSliderIncrement);
		label_hscale_ai_a_post_1s.Text = string.Format("{0}s", preferences.forceSensorAnalyzeABSliderIncrement);
		label_hscale_ai_b_pre_1s.Text = string.Format("{0}s", preferences.forceSensorAnalyzeABSliderIncrement);
		label_hscale_ai_b_post_1s.Text = string.Format("{0}s", preferences.forceSensorAnalyzeABSliderIncrement);
	}
	private void setForceSensorAnalyzeMaxAVGInWindow()
	{
		tvFS_other.MaxAvgInWindowName = string.Format("Max AVG Force in {0} s",
				Util.TrimDecimals (preferences.forceSensorAnalyzeMaxAVGInWindow, 1));
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
	RaceAnalyzerAnalyzeInstant raAI_AB;
	RaceAnalyzerAnalyzeInstant raAI_CD;

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

	private void on_check_ai_export_images_toggled (object o, EventArgs args)
	{
		hbox_ai_export_width_height.Visible = check_ai_export_images.Active;

		//also hide the label and the open button
		label_ai_export_result.Text = "";
		button_ai_export_result_open.Visible = false;
	}

	private void on_radio_signal_analyze_current_set_toggled (object o, EventArgs args)
	{
		notebook_ai_top.CurrentPage = Convert.ToInt32(notebook_ai_top_pages.CURRENTSETSIGNAL);
		label_ai_export_result.Text = "";
		button_ai_export_result_open.Visible = false;
	}
	private void on_radio_signal_analyze_export_csv_toggled (object o, EventArgs args)
	{
		notebook_ai_top.CurrentPage = Convert.ToInt32(notebook_ai_top_pages.CURRENTSESSION);
		label_ai_export_result.Text = "";
		button_ai_export_result_open.Visible = false;
	}

	private void on_radio_force_sensor_export_session_current_toggled (object o, EventArgs args)
	{
		if(currentPerson != null)
			label_ai_export_person.Text = currentPerson.Name;
		else
			label_ai_export_person.Text = "";

		label_ai_export_session.Text = currentSession.Name;

		label_ai_export_result.Text = "";
		button_ai_export_result_open.Visible = false;
	}
	private void on_radio_force_sensor_export_session_all_toggled (object o, EventArgs args)
	{
		if(currentPerson != null)
			label_ai_export_person.Text = currentPerson.Name;
		else
			label_ai_export_person.Text = "";

		label_ai_export_session.Text = Catalog.GetString ("All");

		label_ai_export_result.Text = "";
		button_ai_export_result_open.Visible = false;
	}
	private void on_radio_force_sensor_analyze_export_groupal_toggled (object o, EventArgs args)
	{
		label_ai_export_person.Text = Catalog.GetString ("All");
		label_ai_export_session.Text = currentSession.Name;

		label_ai_export_result.Text = "";
		button_ai_export_result_open.Visible = false;
	}

	//everything except the current set
	private void on_button_force_sensor_export_not_set_clicked (object o, EventArgs args)
	{
		// 1) check if all sessions
		if (radio_ai_export_individual_all_sessions.Active)
		{
			if(currentPerson == null)
				return;

			button_force_sensor_export_session (currentPerson.UniqueID, -1);
			return;
		}

		// 2) current session (individual or groupal)
		if (currentSession == null)
			return;

		if (radio_ai_export_individual_current_session.Active)
		{
			if(currentPerson == null)
				return;

			button_force_sensor_export_session (currentPerson.UniqueID, currentSession.UniqueID);
		}
		else if (radio_ai_export_groupal_current_session.Active)
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

		label_ai_export_result.Text = "";
		button_ai_export_result_open.Visible = false;
		forceSensorButtonsSensitive(false);
		hbox_ai_export_top_modes.Sensitive = false;
		button_ai_model_options.Sensitive = false;
		hbox_ai_export_images.Sensitive = false;

		//store new width/height if changed
		Sqlite.Open();
		preferences.exportGraphWidth = Preferences.PreferencesChange(
				true,
				SqlitePreferences.ExportGraphWidth,
				preferences.exportGraphWidth, Convert.ToInt32(spinbutton_ai_export_image_width.Value));
		preferences.exportGraphHeight = Preferences.PreferencesChange(
				true,
				SqlitePreferences.ExportGraphHeight,
				preferences.exportGraphHeight, Convert.ToInt32(spinbutton_ai_export_image_height.Value));
		Sqlite.Close();

		//change also spinbuttons of export sprint and runEncoder
		spinbutton_sprint_export_image_width.Value = spinbutton_ai_export_image_width.Value;
		spinbutton_sprint_export_image_height.Value = spinbutton_ai_export_image_height.Value;

		forceSensorExport = new ForceSensorExport (
				current_mode,
				notebook_ai_export,
				label_ai_export, progressbar_ai_export,
				label_ai_export_result,
				check_ai_export_images.Active,
				Convert.ToInt32(spinbutton_ai_export_image_width.Value),
				Convert.ToInt32(spinbutton_ai_export_image_height.Value),
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
		if(check_ai_export_images.Active)
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
			hbox_ai_export_top_modes.Sensitive = true;
			button_ai_model_options.Sensitive = true;
			hbox_ai_export_images.Sensitive = true;
		}
	}
	private void on_button_force_sensor_export_file_selected (string selectedFileName)
	{
		forceSensorExport.Start(selectedFileName); //file or folder
	}

	private void force_sensor_export_done (object o, EventArgs args)
	{
		forceSensorExport.Button_done.Clicked -= new EventHandler(force_sensor_export_done);

		forceSensorButtonsSensitive(true);
		hbox_ai_export_top_modes.Sensitive = true;
		button_ai_model_options.Sensitive = true;
		hbox_ai_export_images.Sensitive = true;

		if(forceSensorExport != null && forceSensorExport.AllOk)
			button_ai_export_result_open.Visible = true;
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

	private void forceSensorPrepareGraphAI ()
	{
		// 0. condition return if null
		if(lastForceSensorFullPath == null || lastForceSensorFullPath == "")
			return;

		Gtk.HScale hsLeft = getHScaleABCD (true);
		Gtk.HScale hsRight = getHScaleABCD (false);

		// 1. get zoom values
		int zoomFrameA = -1; //means no zoom
		int zoomFrameB = -1; //means no zoom

		if (AiVars.zoomApplied)
		{
			if (radio_ai_ab.Active)
				getAiZoomStartEnd (tvFS_AB.TimeStart, tvFS_AB.TimeEnd, hsLeft, hsRight,
						ref zoomFrameA, ref zoomFrameB);
			else
				getAiZoomStartEnd (tvFS_CD.TimeStart, tvFS_CD.TimeEnd, hsLeft, hsRight,
						ref zoomFrameA, ref zoomFrameB);
		}

		// 2. create fsAI_AB, fsAI_CD
		//pass them as doubles
		double eccMinDispl = currentForceSensorExercise.GetEccOrConMinMaybePreferences(true,
				preferences.forceSensorElasticEccMinDispl,
				preferences.forceSensorNotElasticEccMinForce);
		double conMinDispl = currentForceSensorExercise.GetEccOrConMinMaybePreferences(false,
				preferences.forceSensorElasticConMinDispl,
				preferences.forceSensorNotElasticConMinForce);
		LogB.Information(string.Format("eccMinDispl: {0}, conMinDispl: {1}", eccMinDispl, conMinDispl));

		//LogB.Information ("lastForceSensorFullPath", lastForceSensorFullPath);
		//LogB.Information(string.Format("creating fsAI with zoomFrameA: {0}, zoomFrameB: {1}", zoomFrameA, zoomFrameB));
		fsAI_AB = new ForceSensorAnalyzeInstant(
				"AB",
				lastForceSensorFullPath,
				zoomFrameA, zoomFrameB,
				currentForceSensorExercise, currentPersonSession.Weight,
				getForceSensorCaptureOptions(), currentForceSensor.Stiffness,
				eccMinDispl, conMinDispl
				);

		string fullPath_cd = lastForceSensorFullPath;
		ForceSensorExercise exercise_cd = currentForceSensorExercise;
		if (radio_ai_2sets.Active &&
				lastForceSensorFullPath_CD != null &&
				lastForceSensorFullPath_CD != "")
		{
			fullPath_cd = lastForceSensorFullPath_CD;
			exercise_cd = currentForceSensorExercise_CD;
			//TODO: CaptureOptions, Stiffness, also personSession.Weight if compare between persons
		}


		//LogB.Information ("fullPath_cd", fullPath_cd);
		fsAI_CD = new ForceSensorAnalyzeInstant(
				"CD",
				fullPath_cd,
				zoomFrameA, zoomFrameB,
				exercise_cd, currentPersonSession.Weight,
				getForceSensorCaptureOptions(), currentForceSensor.Stiffness,
				eccMinDispl, conMinDispl
				);
		//LogB.Information("created fsAI");
		//LogB.Information(string.Format("fsAI.GetLength: {0}", fsAI.GetLength()));

		// 3. set hscales
		signalPrepareGraphAICont (fsAI_AB.GetLength(), fsAI_CD.GetLength(), zoomFrameB, hsRight);

		// 4. manage save buttons visibilities
		manage_force_sensor_ai_table_visibilities();
	}

	private void on_check_force_sensor_analyze_show_magnitudes (object o, EventArgs args)
	{
		if (fsMagnitudesSignalsNoFollow)
			return;

		ai_drawingarea_cairo.QueueDraw ();

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
	public void on_ai_drawingarea_cairo_draw (object o, Gtk.DrawnArgs args)
	{
		if (Constants.ModeIsFORCESENSOR (current_mode))
			updateForceSensorAICairo (true);
		else //if (current_mode == Constants.Modes.RUNSENCODER)
			updateRaceAnalyzerCaptureSpeedTime (true);
	}

	private void updateForceSensorAICairo (bool forceRedraw)
	{
		// 1. create cairoGraphForceSensorAI & spCairoFE if needed
		if (cairoGraphForceSensorAI == null)
			cairoGraphForceSensorAI = new CairoGraphForceSensorAI (
					ai_drawingarea_cairo, "title");

		if (spCairoFE == null)
			spCairoFE = new SignalPointsCairoForceElastic ();


		// 2. get rectangle values if needed
		int rectangleN = 0;
		int rectangleRange = 0;
		if(preferences.forceSensorCaptureFeedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.RECTANGLE)
		{
			rectangleN = preferences.forceSensorCaptureFeedbackAt;
			rectangleRange = preferences.forceSensorCaptureFeedbackRange;
		}

		// 3. get gmaiw_l, briw_l, reps_l for both fsAI
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

				// display reps for set1 or for set2
				if (
						(count == 0 && ! (radio_ai_2sets.Active && radio_ai_cd.Active)) ||
						(count == 1 && radio_ai_2sets.Active && radio_ai_cd.Active) )
				{
					reps_l = fsAI.ForceSensorRepetition_l;
					if(AiVars.zoomApplied)
						reps_l = rep_lZoomAppliedCairo;
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

		// 4. get other data needed
		int hscaleABSampleStart = Convert.ToInt32 (hscale_ai_a.Value);
		int hscaleABSampleEnd = Convert.ToInt32 (hscale_ai_b.Value);
		int hscaleCDSampleStart = Convert.ToInt32 (hscale_ai_c.Value);
		int hscaleCDSampleEnd = Convert.ToInt32 (hscale_ai_d.Value);

		// no need of copy because this graph is done on load or at end of capture (points_list does not grow in other thread
		// spCairoFESend is not a copy, is just to choose between zoomed or not
		SignalPointsCairoForceElastic spCairoFESend;

		if (AiVars.zoomApplied)
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

		// 5. get distinct CD data && subtitleWithSetsInfo if needed
		SignalPointsCairoForceElastic spCairoFESend_CD = null;
		List<string> subtitleWithSetsInfo_l = new List<string> ();
		if (radio_ai_2sets.Active)
		{
			spCairoFESend_CD = spCairoFE_CD;
			if (AiVars.zoomApplied)
				spCairoFESend_CD = spCairoFEZoom_CD;

			if (currentForceSensor != null && currentForceSensorExercise != null &&
					currentForceSensor_CD != null && currentForceSensorExercise_CD != null)
			{
				string abPersonName = "";
				string cdPersonName = "";
				if (signalSuperpose2SetsCDPersonName != "")
				{
					abPersonName = currentPerson.Name + ", ";
					cdPersonName = signalSuperpose2SetsCDPersonName + ", ";
				}

				subtitleWithSetsInfo_l.Add (string.Format ("AB: {0}{1}, {2}, {3}",
							abPersonName,
							currentForceSensorExercise.Name,
							currentForceSensor.Laterality,
							currentForceSensor.DateTimePublic));

				subtitleWithSetsInfo_l.Add (string.Format ("CD: {0}{1}, {2}, {3}",
							cdPersonName,
							currentForceSensorExercise_CD.Name,
							currentForceSensor_CD.Laterality,
							currentForceSensor_CD.DateTimePublic));
			}
		}

		// 6. draw the cairo graph
		fsAIRepetitionMouseLimitsCairo = cairoGraphForceSensorAI.DoSendingList (
				preferences.fontType.ToString(),
				spCairoFESend, spCairoFESend_CD,
				subtitleWithSetsInfo_l, radio_ai_cd.Active,
				check_force_sensor_analyze_show_distance.Active,
				check_force_sensor_analyze_show_speed.Active,
				check_force_sensor_analyze_show_power.Active,
				minY, maxY,
				rectangleN, rectangleRange,
				briw_l,
				triggerListForceSensor,
				hscaleABSampleStart, hscaleABSampleEnd,
				hscaleCDSampleStart, hscaleCDSampleEnd,
				AiVars.zoomApplied,
				gmaiw_l,
				currentForceSensorExercise, reps_l,
				forceRedraw, CairoXY.PlotTypes.LINES);
	}

	private void on_ai_drawingarea_cairo_button_press_event (object o, ButtonPressEventArgs args)
	{
		//LogB.Information(string.Format("Mouse X: {0}; Mouse Y: {1}", args.Event.X, args.Event.Y));

		//if zoomed: unzoom and return
		if(AiVars.zoomApplied)
		{
			check_ai_zoom.Click();
			return;
		}

		//if list exists, select the repetition
		if (fsAIRepetitionMouseLimitsCairo == null)
			return;

		int repetition = fsAIFindBarInPixelCairo (args.Event.X, args.Event.Y);
		LogB.Information("Repetition: " + repetition.ToString());
		if(repetition < 0)
			return;

		if (radio_ai_ab.Active)
		{
			hscale_ai_a.Value = fsAIRepetitionMouseLimitsCairo.GetSampleStartOfARep (repetition);
			hscale_ai_b.Value = fsAIRepetitionMouseLimitsCairo.GetSampleEndOfARep (repetition);
		} else
		{
			hscale_ai_c.Value = fsAIRepetitionMouseLimitsCairo.GetSampleStartOfARep (repetition);
			hscale_ai_d.Value = fsAIRepetitionMouseLimitsCairo.GetSampleEndOfARep (repetition);
		}
	}

	private void forceSensorAnalyzeManualGraphDo(Rectangle allocation)
	{
	}

	private int fsAIFindBarInPixel (double px, double py)
	{
		if(fsAIRepetitionMouseLimits == null)
			return -1;

		return fsAIRepetitionMouseLimits.FindBarInPixel (px, py);
	}
	private int fsAIFindBarInPixelCairo (double px, double py)
	{
		if(fsAIRepetitionMouseLimitsCairo == null)
			return -1;

		return fsAIRepetitionMouseLimitsCairo.FindBarInPixel (px, py);
	}


	private void manage_force_sensor_ai_table_visibilities()
	{
		bool visible = true;//checkbutton_force_sensor_ai_b.Active;

		//ForceSensorAnalyzeInstant fsAI = getCorrectAI ();
		//bool visibleElastic = (visible && fsAI.CalculedElasticPSAP);

		if (visible && canDoForceSensorAnalyzeAB ())
			button_force_sensor_analyze_AB_save.Visible = true;
		else
			button_force_sensor_analyze_AB_save.Visible = false;

		if (visible && canDoForceSensorAnalyzeCD ())
			button_force_sensor_analyze_CD_save.Visible = true;
		else
			button_force_sensor_analyze_CD_save.Visible = false;
	}

	private void force_sensor_analyze_instant_calculate_params_for_treeview (
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

		/* no need (I think) because it is already on on_hscale_ai_value_changed
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
				getLowestForceSensorScale (hscale_ai_a, hscale_ai_b),
				getHighestForceSensorScale (hscale_ai_a, hscale_ai_b),
				selectedFileName, preferences.CSVExportDecimalSeparator);
	}
	private void on_button_force_sensor_save_CD_file_selected (string selectedFileName)
	{
		fsAI_CD.ExportToCSV(
				getLowestForceSensorScale (hscale_ai_c, hscale_ai_d),
				getHighestForceSensorScale (hscale_ai_c, hscale_ai_d),
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
		radio_signal_analyze_current_set = (Gtk.RadioButton) builder.GetObject ("radio_signal_analyze_current_set");
		//radio_signal_analyze_export_csv = (Gtk.RadioButton) builder.GetObject ("radio_signal_analyze_export_csv");
		image_force_sensor_analyze_individual_current_set = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_individual_current_set");
		image_force_sensor_analyze_individual_current_session = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_individual_current_session");
		image_force_sensor_analyze_individual_all_sessions = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_individual_all_sessions");
		image_force_sensor_analyze_groupal_current_session = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_groupal_current_session");

		notebook_ai_top = (Gtk.Notebook) builder.GetObject ("notebook_ai_top");
		hbox_force_general_analysis = (Gtk.HBox) builder.GetObject ("hbox_force_general_analysis");
		button_signal_analyze_load_ab = (Gtk.Button) builder.GetObject ("button_signal_analyze_load_ab");
		button_signal_analyze_load_cd = (Gtk.Button) builder.GetObject ("button_signal_analyze_load_cd");
		button_ai_move_cd_pre = (Gtk.Button) builder.GetObject ("button_ai_move_cd_pre");
		button_force_sensor_image_save_rfd_manual = (Gtk.Button) builder.GetObject ("button_force_sensor_image_save_rfd_manual");
		button_force_sensor_analyze_AB_save = (Gtk.Button) builder.GetObject ("button_force_sensor_analyze_AB_save");
		button_force_sensor_analyze_CD_save = (Gtk.Button) builder.GetObject ("button_force_sensor_analyze_CD_save");

		radio_force_rfd_search_optimized_ab = (Gtk.RadioButton) builder.GetObject ("radio_force_rfd_search_optimized_ab");
		radio_force_rfd_use_ab_range = (Gtk.RadioButton) builder.GetObject ("radio_force_rfd_use_ab_range");
		spin_force_duration_seconds = (Gtk.SpinButton) builder.GetObject ("spin_force_duration_seconds");
		radio_force_duration_seconds = (Gtk.RadioButton) builder.GetObject ("radio_force_duration_seconds");
		hbox_force_rfd_duration_percent = (Gtk.HBox) builder.GetObject ("hbox_force_rfd_duration_percent");
		radio_force_rfd_duration_percent = (Gtk.RadioButton) builder.GetObject ("radio_force_rfd_duration_percent");
		spin_force_rfd_duration_percent = (Gtk.SpinButton) builder.GetObject ("spin_force_rfd_duration_percent");

		//analyze options
		notebook_force_sensor_rfd_options = (Gtk.Notebook) builder.GetObject ("notebook_force_sensor_rfd_options");
		hbox_ai_export_top_modes = (Gtk.HBox) builder.GetObject ("hbox_ai_export_top_modes");
		//	hbox_force_sensor_analyze_automatic_options = (Gtk.HBox) builder.GetObject ("hbox_force_sensor_analyze_automatic_options");
		//	notebook_force_analyze_automatic = (Gtk.Notebook) builder.GetObject ("notebook_force_analyze_automatic");
		/*
		hbox_force_show_1 = (Gtk.HBox) builder.GetObject ("hbox_force_show_1");
		hbox_force_show_2 = (Gtk.HBox) builder.GetObject ("hbox_force_show_2");
		hbox_force_show_3 = (Gtk.HBox) builder.GetObject ("hbox_force_show_3");
		hbox_force_show_4 = (Gtk.HBox) builder.GetObject ("hbox_force_show_4");
		hbox_force_show_5 = (Gtk.HBox) builder.GetObject ("hbox_force_show_5");
		hbox_force_show_6 = (Gtk.HBox) builder.GetObject ("hbox_force_show_6");
		hbox_force_show_7 = (Gtk.HBox) builder.GetObject ("hbox_force_show_7");
		hbox_force_show_8 = (Gtk.HBox) builder.GetObject ("hbox_force_show_8");
		hbox_force_show_9 = (Gtk.HBox) builder.GetObject ("hbox_force_show_9");
		hbox_force_show_10 = (Gtk.HBox) builder.GetObject ("hbox_force_show_10");
		*/
		vbox_force_rfd_duration_end = (Gtk.VBox) builder.GetObject ("vbox_force_rfd_duration_end");
		hbox_force_1 = (Gtk.HBox) builder.GetObject ("hbox_force_1");
		hbox_force_2 = (Gtk.HBox) builder.GetObject ("hbox_force_2");
		hbox_force_3 = (Gtk.HBox) builder.GetObject ("hbox_force_3");
		hbox_force_4 = (Gtk.HBox) builder.GetObject ("hbox_force_4");
		hbox_force_5 = (Gtk.HBox) builder.GetObject ("hbox_force_5");
		hbox_force_6 = (Gtk.HBox) builder.GetObject ("hbox_force_6");
		hbox_force_7 = (Gtk.HBox) builder.GetObject ("hbox_force_7");
		hbox_force_8 = (Gtk.HBox) builder.GetObject ("hbox_force_8");
		hbox_force_9 = (Gtk.HBox) builder.GetObject ("hbox_force_9");
		hbox_force_10 = (Gtk.HBox) builder.GetObject ("hbox_force_10");
		hbox_force_impulse = (Gtk.HBox) builder.GetObject ("hbox_force_impulse");
		check_force_1 = (Gtk.CheckButton) builder.GetObject ("check_force_1");
		check_force_2 = (Gtk.CheckButton) builder.GetObject ("check_force_2");
		check_force_3 = (Gtk.CheckButton) builder.GetObject ("check_force_3");
		check_force_4 = (Gtk.CheckButton) builder.GetObject ("check_force_4");
		check_force_5 = (Gtk.CheckButton) builder.GetObject ("check_force_5");
		check_force_6 = (Gtk.CheckButton) builder.GetObject ("check_force_6");
		check_force_7 = (Gtk.CheckButton) builder.GetObject ("check_force_7");
		check_force_8 = (Gtk.CheckButton) builder.GetObject ("check_force_8");
		check_force_9 = (Gtk.CheckButton) builder.GetObject ("check_force_9");
		check_force_10 = (Gtk.CheckButton) builder.GetObject ("check_force_10");
		check_force_impulse = (Gtk.CheckButton) builder.GetObject ("check_force_impulse");
		hbox_force_1_at_ms = (Gtk.HBox) builder.GetObject ("hbox_force_1_at_ms");
		hbox_force_2_at_ms = (Gtk.HBox) builder.GetObject ("hbox_force_2_at_ms");
		hbox_force_3_at_ms = (Gtk.HBox) builder.GetObject ("hbox_force_3_at_ms");
		hbox_force_4_at_ms = (Gtk.HBox) builder.GetObject ("hbox_force_4_at_ms");
		hbox_force_5_at_ms = (Gtk.HBox) builder.GetObject ("hbox_force_5_at_ms");
		hbox_force_6_at_ms = (Gtk.HBox) builder.GetObject ("hbox_force_6_at_ms");
		hbox_force_7_at_ms = (Gtk.HBox) builder.GetObject ("hbox_force_7_at_ms");
		hbox_force_8_at_ms = (Gtk.HBox) builder.GetObject ("hbox_force_8_at_ms");
		hbox_force_9_at_ms = (Gtk.HBox) builder.GetObject ("hbox_force_9_at_ms");
		hbox_force_10_at_ms = (Gtk.HBox) builder.GetObject ("hbox_force_10_at_ms");
		hbox_force_1_at_percent = (Gtk.HBox) builder.GetObject ("hbox_force_1_at_percent");
		hbox_force_2_at_percent = (Gtk.HBox) builder.GetObject ("hbox_force_2_at_percent");
		hbox_force_3_at_percent = (Gtk.HBox) builder.GetObject ("hbox_force_3_at_percent");
		hbox_force_4_at_percent = (Gtk.HBox) builder.GetObject ("hbox_force_4_at_percent");
		hbox_force_5_at_percent = (Gtk.HBox) builder.GetObject ("hbox_force_5_at_percent");
		hbox_force_6_at_percent = (Gtk.HBox) builder.GetObject ("hbox_force_6_at_percent");
		hbox_force_7_at_percent = (Gtk.HBox) builder.GetObject ("hbox_force_7_at_percent");
		hbox_force_8_at_percent = (Gtk.HBox) builder.GetObject ("hbox_force_8_at_percent");
		hbox_force_9_at_percent = (Gtk.HBox) builder.GetObject ("hbox_force_9_at_percent");
		hbox_force_10_at_percent = (Gtk.HBox) builder.GetObject ("hbox_force_10_at_percent");
		hbox_force_impulse_until_percent = (Gtk.HBox) builder.GetObject ("hbox_force_impulse_until_percent");
		hbox_force_1_from_to = (Gtk.HBox) builder.GetObject ("hbox_force_1_from_to");
		hbox_force_2_from_to = (Gtk.HBox) builder.GetObject ("hbox_force_2_from_to");
		hbox_force_3_from_to = (Gtk.HBox) builder.GetObject ("hbox_force_3_from_to");
		hbox_force_4_from_to = (Gtk.HBox) builder.GetObject ("hbox_force_4_from_to");
		hbox_force_5_from_to = (Gtk.HBox) builder.GetObject ("hbox_force_5_from_to");
		hbox_force_6_from_to = (Gtk.HBox) builder.GetObject ("hbox_force_6_from_to");
		hbox_force_7_from_to = (Gtk.HBox) builder.GetObject ("hbox_force_7_from_to");
		hbox_force_8_from_to = (Gtk.HBox) builder.GetObject ("hbox_force_8_from_to");
		hbox_force_9_from_to = (Gtk.HBox) builder.GetObject ("hbox_force_9_from_to");
		hbox_force_10_from_to = (Gtk.HBox) builder.GetObject ("hbox_force_10_from_to");
		hbox_force_1_in_x_ms = (Gtk.HBox) builder.GetObject ("hbox_force_1_in_x_ms");
		hbox_force_2_in_x_ms = (Gtk.HBox) builder.GetObject ("hbox_force_2_in_x_ms");
		hbox_force_3_in_x_ms = (Gtk.HBox) builder.GetObject ("hbox_force_3_in_x_ms");
		hbox_force_4_in_x_ms = (Gtk.HBox) builder.GetObject ("hbox_force_4_in_x_ms");
		hbox_force_5_in_x_ms = (Gtk.HBox) builder.GetObject ("hbox_force_5_in_x_ms");
		hbox_force_6_in_x_ms = (Gtk.HBox) builder.GetObject ("hbox_force_6_in_x_ms");
		hbox_force_7_in_x_ms = (Gtk.HBox) builder.GetObject ("hbox_force_7_in_x_ms");
		hbox_force_8_in_x_ms = (Gtk.HBox) builder.GetObject ("hbox_force_8_in_x_ms");
		hbox_force_9_in_x_ms = (Gtk.HBox) builder.GetObject ("hbox_force_9_in_x_ms");
		hbox_force_10_in_x_ms = (Gtk.HBox) builder.GetObject ("hbox_force_10_in_x_ms");
		spinbutton_force_1_at_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_1_at_ms");
		spinbutton_force_2_at_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_2_at_ms");
		spinbutton_force_3_at_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_3_at_ms");
		spinbutton_force_4_at_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_4_at_ms");
		spinbutton_force_5_at_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_5_at_ms");
		spinbutton_force_6_at_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_6_at_ms");
		spinbutton_force_7_at_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_7_at_ms");
		spinbutton_force_8_at_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_8_at_ms");
		spinbutton_force_9_at_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_9_at_ms");
		spinbutton_force_10_at_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_10_at_ms");
		hbox_force_impulse_from_to = (Gtk.HBox) builder.GetObject ("hbox_force_impulse_from_to");
		spinbutton_force_1_at_percent = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_1_at_percent");
		spinbutton_force_2_at_percent = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_2_at_percent");
		spinbutton_force_3_at_percent = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_3_at_percent");
		spinbutton_force_4_at_percent = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_4_at_percent");
		spinbutton_force_5_at_percent = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_5_at_percent");
		spinbutton_force_6_at_percent = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_6_at_percent");
		spinbutton_force_7_at_percent = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_7_at_percent");
		spinbutton_force_8_at_percent = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_8_at_percent");
		spinbutton_force_9_at_percent = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_9_at_percent");
		spinbutton_force_10_at_percent = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_10_at_percent");
		spinbutton_force_impulse_until_percent = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_impulse_until_percent");
		spinbutton_force_1_from = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_1_from");
		spinbutton_force_2_from = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_2_from");
		spinbutton_force_3_from = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_3_from");
		spinbutton_force_4_from = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_4_from");
		spinbutton_force_5_from = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_5_from");
		spinbutton_force_6_from = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_6_from");
		spinbutton_force_7_from = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_7_from");
		spinbutton_force_8_from = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_8_from");
		spinbutton_force_9_from = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_9_from");
		spinbutton_force_10_from = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_10_from");
		spinbutton_force_impulse_from = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_impulse_from");
		spinbutton_force_1_to = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_1_to");
		spinbutton_force_2_to = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_2_to");
		spinbutton_force_3_to = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_3_to");
		spinbutton_force_4_to = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_4_to");
		spinbutton_force_5_to = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_5_to");
		spinbutton_force_6_to = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_6_to");
		spinbutton_force_7_to = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_7_to");
		spinbutton_force_8_to = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_8_to");
		spinbutton_force_9_to = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_9_to");
		spinbutton_force_10_to = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_10_to");
		spinbutton_force_impulse_to = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_impulse_to");
		spinbutton_force_1_in_x_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_1_in_x_ms");
		spinbutton_force_2_in_x_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_2_in_x_ms");
		spinbutton_force_3_in_x_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_3_in_x_ms");
		spinbutton_force_4_in_x_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_4_in_x_ms");
		spinbutton_force_5_in_x_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_5_in_x_ms");
		spinbutton_force_6_in_x_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_6_in_x_ms");
		spinbutton_force_7_in_x_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_7_in_x_ms");
		spinbutton_force_8_in_x_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_8_in_x_ms");
		spinbutton_force_9_in_x_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_9_in_x_ms");
		spinbutton_force_10_in_x_ms = (Gtk.SpinButton) builder.GetObject ("spinbutton_force_10_in_x_ms");

		hbox_force_sensor_analyze_ai_sliders_and_buttons = (Gtk.HBox) builder.GetObject ("hbox_force_sensor_analyze_ai_sliders_and_buttons");
		ai_drawingarea_cairo = (Gtk.DrawingArea) builder.GetObject ("ai_drawingarea_cairo");
		box_force_sensor_analyze_magnitudes = (Gtk.Box) builder.GetObject ("box_force_sensor_analyze_magnitudes");
		check_force_sensor_analyze_show_distance = (Gtk.CheckButton) builder.GetObject ("check_force_sensor_analyze_show_distance");
		check_force_sensor_analyze_show_speed = (Gtk.CheckButton) builder.GetObject ("check_force_sensor_analyze_show_speed");
		check_force_sensor_analyze_show_power = (Gtk.CheckButton) builder.GetObject ("check_force_sensor_analyze_show_power");
		image_force_sensor_analyze_show_distance = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_show_distance");
		image_force_sensor_analyze_show_speed = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_show_speed");
		image_force_sensor_analyze_show_power = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_show_power");
		box_force_sensor_ai_a = (Gtk.Box) builder.GetObject ("box_force_sensor_ai_a");
		box_force_sensor_ai_b = (Gtk.Box) builder.GetObject ("box_force_sensor_ai_b");
		box_force_sensor_ai_c = (Gtk.Box) builder.GetObject ("box_force_sensor_ai_c");
		box_force_sensor_ai_d = (Gtk.Box) builder.GetObject ("box_force_sensor_ai_d");
		label_force_sensor_ai_zoom_abcd = (Gtk.Label) builder.GetObject ("label_force_sensor_ai_zoom_abcd");
		treeview_ai_AB = (Gtk.TreeView) builder.GetObject ("treeview_ai_AB");
		treeview_ai_CD = (Gtk.TreeView) builder.GetObject ("treeview_ai_CD");
		treeview_force_sensor_ai_other = (Gtk.TreeView) builder.GetObject ("treeview_force_sensor_ai_other");

		combo_force_1_function = (Gtk.ComboBoxText) builder.GetObject ("combo_force_1_function");
		combo_force_2_function = (Gtk.ComboBoxText) builder.GetObject ("combo_force_2_function");
		combo_force_3_function = (Gtk.ComboBoxText) builder.GetObject ("combo_force_3_function");
		combo_force_4_function = (Gtk.ComboBoxText) builder.GetObject ("combo_force_4_function");
		combo_force_5_function = (Gtk.ComboBoxText) builder.GetObject ("combo_force_5_function");
		combo_force_6_function = (Gtk.ComboBoxText) builder.GetObject ("combo_force_6_function");
		combo_force_7_function = (Gtk.ComboBoxText) builder.GetObject ("combo_force_7_function");
		combo_force_8_function = (Gtk.ComboBoxText) builder.GetObject ("combo_force_8_function");
		combo_force_9_function = (Gtk.ComboBoxText) builder.GetObject ("combo_force_9_function");
		combo_force_10_function = (Gtk.ComboBoxText) builder.GetObject ("combo_force_10_function");
		combo_force_impulse_function = (Gtk.ComboBoxText) builder.GetObject ("combo_force_impulse_function");
		combo_force_1_type = (Gtk.ComboBoxText) builder.GetObject ("combo_force_1_type");
		combo_force_2_type = (Gtk.ComboBoxText) builder.GetObject ("combo_force_2_type");
		combo_force_3_type = (Gtk.ComboBoxText) builder.GetObject ("combo_force_3_type");
		combo_force_4_type = (Gtk.ComboBoxText) builder.GetObject ("combo_force_4_type");
		combo_force_5_type = (Gtk.ComboBoxText) builder.GetObject ("combo_force_5_type");
		combo_force_6_type = (Gtk.ComboBoxText) builder.GetObject ("combo_force_6_type");
		combo_force_7_type = (Gtk.ComboBoxText) builder.GetObject ("combo_force_7_type");
		combo_force_8_type = (Gtk.ComboBoxText) builder.GetObject ("combo_force_8_type");
		combo_force_9_type = (Gtk.ComboBoxText) builder.GetObject ("combo_force_9_type");
		combo_force_10_type = (Gtk.ComboBoxText) builder.GetObject ("combo_force_10_type");
		combo_force_impulse_type = (Gtk.ComboBoxText) builder.GetObject ("combo_force_impulse_type");
	}
}


// 1 for AB and another for CD:
// tvFS_AB for treeview_ai_AB
// tvFS_CD for treeview_ai_CD
public class TreeviewFSAnalyze : TreeviewS2Abstract
{
	//row 1
	protected string letterStart;
	protected double forceStart;
	protected string rfdStart;

	//row 2
	protected string letterEnd;
	protected double forceEnd;
	protected string rfdEnd;

	//row 3
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

	public override void PassForceAndRFD1or2 (bool isLeft, double force, string rfd)
	{
		if (isLeft)
		{
			this.forceStart = force;
			this.rfdStart = rfd;
		} else {
			this.forceEnd = force;
			this.rfdEnd = rfd;
		}
	}

	protected override string [] getTreeviewStr ()
	{
		return new String [4];
	}

	protected override string [] fillTreeViewStart (string [] str, int i)
	{
		str[i++] = letterStart;
		str[i++] = timeStart;
		str[i++] = Math.Round (forceStart, 1).ToString ();
		str[i++] = rfdStart;
		return fillTreeViewStartElastic (str, i);
	}

	protected override string [] fillTreeViewEnd (string [] str, int i)
	{
		str[i++] = letterEnd;
		str[i++] = timeEnd;
		str[i++] = Math.Round (forceEnd, 1).ToString ();
		str[i++] = rfdEnd;
		return fillTreeViewEndElastic (str, i);
	}

	protected override string [] fillTreeViewDiff (string [] str, int i)
	{
		str[i++] = Catalog.GetString ("Difference");
		str[i++] = timeDiff;
		str[i++] = Math.Round (forceDiff, 1).ToString ();
		str[i++] = Math.Round (rfdDiff, 1).ToString ();
		return fillTreeViewDiffElastic (str, i);
	}

	protected override string [] fillTreeViewAvg (string [] str, int i)
	{
		str[i++] = Catalog.GetString ("Average");
		str[i++] = ""; // no time avg
		str[i++] = forceAvg;
		str[i++] = rfdAvg;
		return fillTreeViewAvgElastic (str, i);
	}

	protected override string [] fillTreeViewMax (string [] str, int i)
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

public class TreeviewFSAnalyzeOther : TreeviewSAbstract
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

		if (
				(feedbackAB != null && feedbackAB != "") ||
				(feedbackCD != null && feedbackCD != "") )
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
