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
 * Copyright (C) 2018-2021   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO;
using Gtk;
using Gdk;
using Glade;
using System.Text; //StringBuilder
using System.Collections.Generic; //List<T>
using Mono.Unix;

//struct with relevant data used on various functions and threads
public partial class ChronoJumpWindow 
{
	//analyze tab
	[Widget] Gtk.RadioButton radio_force_sensor_analyze_individual_current_set;
	[Widget] Gtk.RadioButton radio_force_sensor_analyze_individual_current_session;
	[Widget] Gtk.RadioButton radio_force_sensor_analyze_individual_all_sessions;
	[Widget] Gtk.RadioButton radio_force_sensor_analyze_groupal_current_session;
	[Widget] Gtk.Image image_force_sensor_analyze_individual_current_set;
	[Widget] Gtk.Image image_force_sensor_analyze_individual_current_session;
	[Widget] Gtk.Image image_force_sensor_analyze_individual_all_sessions;
	[Widget] Gtk.Image image_force_sensor_analyze_groupal_current_session;

	[Widget] Gtk.Notebook notebook_force_sensor_analyze_top;
	[Widget] Gtk.HBox hbox_force_general_analysis;
	[Widget] Gtk.Button button_force_sensor_analyze_load;
	[Widget] Gtk.Button button_force_sensor_analyze_analyze;
	[Widget] Gtk.Label label_force_sensor_analyze;
	[Widget] Gtk.Image image_force_sensor_graph;
	[Widget] Gtk.Viewport viewport_force_sensor_graph;
	[Widget] Gtk.Button button_force_sensor_image_save_rfd_auto;
	[Widget] Gtk.Button button_force_sensor_image_save_rfd_manual;
	[Widget] Gtk.ScrolledWindow scrolledwindow_force_sensor_ai;
	[Widget] Gtk.Button button_force_sensor_analyze_AB_save;
	[Widget] Gtk.CheckButton check_force_sensor_ai_chained;
	[Widget] Gtk.CheckButton check_force_sensor_ai_zoom;

	[Widget] Gtk.RadioButton radio_force_rfd_search_optimized_ab;
	[Widget] Gtk.RadioButton radio_force_rfd_use_ab_range;
	[Widget] Gtk.SpinButton spin_force_duration_seconds;
	[Widget] Gtk.RadioButton radio_force_duration_seconds;
	[Widget] Gtk.HBox hbox_force_rfd_duration_percent;
	[Widget] Gtk.RadioButton radio_force_rfd_duration_percent;
	[Widget] Gtk.SpinButton spin_force_rfd_duration_percent;

	//analyze options
	[Widget] Gtk.HBox hbox_force_sensor_analyze_top_modes;
//	[Widget] Gtk.HBox hbox_force_sensor_analyze_automatic_options;
//	[Widget] Gtk.Notebook notebook_force_analyze_automatic;
	[Widget] Gtk.Button button_force_sensor_analyze_options_close_and_analyze;
	[Widget] Gtk.Label label_hscale_force_sensor_ai_a_pre_1s;
	[Widget] Gtk.Label label_hscale_force_sensor_ai_a_post_1s;
	[Widget] Gtk.Label label_hscale_force_sensor_ai_b_pre_1s;
	[Widget] Gtk.Label label_hscale_force_sensor_ai_b_post_1s;
	[Widget] Gtk.VBox vbox_force_rfd_duration_end;
	[Widget] Gtk.Button button_force_sensor_analyze_options;
	[Widget] Gtk.HBox hbox_force_1;
	[Widget] Gtk.HBox hbox_force_2;
	[Widget] Gtk.HBox hbox_force_3;
	[Widget] Gtk.HBox hbox_force_4;
	[Widget] Gtk.HBox hbox_force_impulse;
	[Widget] Gtk.CheckButton check_force_1;
	[Widget] Gtk.CheckButton check_force_2;
	[Widget] Gtk.CheckButton check_force_3;
	[Widget] Gtk.CheckButton check_force_4;
	[Widget] Gtk.CheckButton check_force_impulse;
	[Widget] Gtk.ComboBox combo_force_1_function;
	[Widget] Gtk.ComboBox combo_force_2_function;
	[Widget] Gtk.ComboBox combo_force_3_function;
	[Widget] Gtk.ComboBox combo_force_4_function;
	[Widget] Gtk.ComboBox combo_force_impulse_function;
	[Widget] Gtk.ComboBox combo_force_1_type;
	[Widget] Gtk.ComboBox combo_force_2_type;
	[Widget] Gtk.ComboBox combo_force_3_type;
	[Widget] Gtk.ComboBox combo_force_4_type;
	[Widget] Gtk.ComboBox combo_force_impulse_type;
	[Widget] Gtk.HBox hbox_force_1_at_ms;
	[Widget] Gtk.HBox hbox_force_2_at_ms;
	[Widget] Gtk.HBox hbox_force_3_at_ms;
	[Widget] Gtk.HBox hbox_force_4_at_ms;
	[Widget] Gtk.HBox hbox_force_1_at_percent;
	[Widget] Gtk.HBox hbox_force_2_at_percent;
	[Widget] Gtk.HBox hbox_force_3_at_percent;
	[Widget] Gtk.HBox hbox_force_4_at_percent;
	[Widget] Gtk.HBox hbox_force_impulse_until_percent;
	[Widget] Gtk.HBox hbox_force_1_from_to;
	[Widget] Gtk.HBox hbox_force_2_from_to;
	[Widget] Gtk.HBox hbox_force_3_from_to;
	[Widget] Gtk.HBox hbox_force_4_from_to;
	[Widget] Gtk.HBox hbox_force_impulse_from_to;
	[Widget] Gtk.SpinButton spinbutton_force_1_at_ms;
	[Widget] Gtk.SpinButton spinbutton_force_2_at_ms;
	[Widget] Gtk.SpinButton spinbutton_force_3_at_ms;
	[Widget] Gtk.SpinButton spinbutton_force_4_at_ms;
	[Widget] Gtk.SpinButton spinbutton_force_1_at_percent;
	[Widget] Gtk.SpinButton spinbutton_force_2_at_percent;
	[Widget] Gtk.SpinButton spinbutton_force_3_at_percent;
	[Widget] Gtk.SpinButton spinbutton_force_4_at_percent;
	[Widget] Gtk.SpinButton spinbutton_force_impulse_until_percent;
	[Widget] Gtk.SpinButton spinbutton_force_1_from;
	[Widget] Gtk.SpinButton spinbutton_force_2_from;
	[Widget] Gtk.SpinButton spinbutton_force_3_from;
	[Widget] Gtk.SpinButton spinbutton_force_4_from;
	[Widget] Gtk.SpinButton spinbutton_force_impulse_from;
	[Widget] Gtk.SpinButton spinbutton_force_1_to;
	[Widget] Gtk.SpinButton spinbutton_force_2_to;
	[Widget] Gtk.SpinButton spinbutton_force_3_to;
	[Widget] Gtk.SpinButton spinbutton_force_4_to;
	[Widget] Gtk.SpinButton spinbutton_force_impulse_to;

	[Widget] Gtk.Button button_hscale_force_sensor_ai_a_first;
	[Widget] Gtk.Button button_hscale_force_sensor_ai_a_pre;
	[Widget] Gtk.Button button_hscale_force_sensor_ai_a_post;
	[Widget] Gtk.Button button_hscale_force_sensor_ai_a_last;

	[Widget] Gtk.Button button_hscale_force_sensor_ai_b_first;
	[Widget] Gtk.Button button_hscale_force_sensor_ai_b_pre;
	[Widget] Gtk.Button button_hscale_force_sensor_ai_b_post;
	[Widget] Gtk.Button button_hscale_force_sensor_ai_b_last;

	[Widget] Gtk.HBox hbox_force_sensor_ai_position;
	[Widget] Gtk.HBox hbox_force_sensor_ai_speed;
	[Widget] Gtk.HBox hbox_force_sensor_ai_accel;
	[Widget] Gtk.HBox hbox_force_sensor_ai_power;

	[Widget] Gtk.Table table_force_sensor_ai_impulse_variability_and_feedback;
	[Widget] Gtk.Label label_force_sensor_ai_feedback;
	[Widget] Gtk.HBox hbox_force_sensor_ai_feedback;
	[Widget] Gtk.Label label_force_sensor_ai_impulse_values;
	[Widget] Gtk.Label label_force_sensor_ai_variability_values;
	[Widget] Gtk.Label label_force_sensor_ai_feedback_values;
	[Widget] Gtk.Label label_force_sensor_ai_variability_method;
	[Widget] Gtk.Label label_force_sensor_ai_variability_units;
	[Widget] Gtk.Label label_force_sensor_ai_max_avg_in_window;

	[Widget] Gtk.Notebook notebook_force_sensor_export;
	[Widget] Gtk.Label label_force_sensor_export_data;
	[Widget] Gtk.HBox hbox_force_sensor_export_images;
	[Widget] Gtk.CheckButton check_force_sensor_export_images;
	[Widget] Gtk.HBox hbox_force_sensor_export_width_height;
	[Widget] Gtk.SpinButton spinbutton_force_sensor_export_image_width;
	[Widget] Gtk.SpinButton spinbutton_force_sensor_export_image_height;
	[Widget] Gtk.ProgressBar progressbar_force_sensor_export;
	[Widget] Gtk.Label label_force_sensor_export_result;
	[Widget] Gtk.Button button_force_sensor_export_result_open;

	private RepetitionMouseLimits fsAIRepetitionMouseLimits;

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
				SqlitePreferences.ForceSensorMIFDurationSeconds,
				preferences.forceSensorMIFDurationSeconds, spin_force_duration_seconds.Value);
		preferences.forceSensorMIFDurationPercent = Preferences.PreferencesChange(
				SqlitePreferences.ForceSensorMIFDurationPercent,
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
		Gtk.ComboBox combo = o as ComboBox;
		if (o == null)
			return;

		if(combo == combo_force_1_type)
			combo_force_visibility(
					UtilGtk.ComboGetActive(combo_force_1_type),
					hbox_force_1_at_ms,
					hbox_force_1_at_percent,
					hbox_force_1_from_to);
		else if(combo == combo_force_2_type)
			combo_force_visibility(
					UtilGtk.ComboGetActive(combo_force_2_type),
					hbox_force_2_at_ms,
					hbox_force_2_at_percent,
					hbox_force_2_from_to);
		else if(combo == combo_force_3_type)
			combo_force_visibility(
					UtilGtk.ComboGetActive(combo_force_3_type),
					hbox_force_3_at_ms,
					hbox_force_3_at_percent,
					hbox_force_3_from_to);
		else if(combo == combo_force_4_type)
			combo_force_visibility(
					UtilGtk.ComboGetActive(combo_force_4_type),
					hbox_force_4_at_ms,
					hbox_force_4_at_percent,
					hbox_force_4_from_to);
		else if(combo == combo_force_impulse_type)
			combo_force_impulse_visibility(
					UtilGtk.ComboGetActive(combo_force_impulse_type),
					hbox_force_impulse_until_percent,
					hbox_force_impulse_from_to);
	}

	private void combo_force_visibility (string selected, Gtk.HBox at_ms, Gtk.HBox at_percent, Gtk.HBox from_to)
	{
		//valid for active == "" and active == "RFD max"
		at_ms.Visible = false;
		from_to.Visible = false;
		at_percent.Visible = false;

		if(selected == Catalog.GetString(ForceSensorRFD.Type_INSTANTANEOUS_name))
		{
			at_ms.Visible = true;
		}
		else if(selected == Catalog.GetString(ForceSensorRFD.Type_AVERAGE_name))
		{
			from_to.Visible = true;
		}
		else if(selected == Catalog.GetString(ForceSensorRFD.Type_PERCENT_F_MAX_name))
		{
			at_percent.Visible = true;
		}
	}
	private void combo_force_impulse_visibility (string selected, Gtk.HBox until_percent, Gtk.HBox from_to)
	{
		until_percent.Visible = false;
		from_to.Visible = false;

		if(selected == Catalog.GetString(ForceSensorImpulse.Type_IMP_UNTIL_PERCENT_F_MAX_name))
		{
			until_percent.Visible = true;
		}
		else if(selected == Catalog.GetString(ForceSensorImpulse.Type_IMP_RANGE_name))
		{
			from_to.Visible = true;
		}
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
		setRFDValue(rfdList[0], check_force_1, combo_force_1_function, combo_force_1_type,
				hbox_force_1_at_ms, spinbutton_force_1_at_ms,
				hbox_force_1_at_percent, spinbutton_force_1_at_percent,
				hbox_force_1_from_to, spinbutton_force_1_from, spinbutton_force_1_to);

		setRFDValue(rfdList[1], check_force_2, combo_force_2_function, combo_force_2_type,
				hbox_force_2_at_ms, spinbutton_force_2_at_ms,
				hbox_force_2_at_percent, spinbutton_force_2_at_percent,
				hbox_force_2_from_to, spinbutton_force_2_from, spinbutton_force_2_to);

		setRFDValue(rfdList[2], check_force_3, combo_force_3_function, combo_force_3_type,
				hbox_force_3_at_ms, spinbutton_force_3_at_ms,
				hbox_force_3_at_percent, spinbutton_force_3_at_percent,
				hbox_force_3_from_to, spinbutton_force_3_from, spinbutton_force_3_to);

		setRFDValue(rfdList[3], check_force_4, combo_force_4_function, combo_force_4_type,
				hbox_force_4_at_ms, spinbutton_force_4_at_ms,
				hbox_force_4_at_percent, spinbutton_force_4_at_percent,
				hbox_force_4_from_to, spinbutton_force_4_from, spinbutton_force_4_to);
	}

	private void setRFDValue(ForceSensorRFD rfd, Gtk.CheckButton check, Gtk.ComboBox combo_force_function, Gtk.ComboBox combo_force_type,
			Gtk.HBox hbox_force_at_ms, Gtk.SpinButton spinbutton_force_at_ms,
			Gtk.HBox hbox_force_at_percent, Gtk.SpinButton spinbutton_force_at_percent,
			Gtk.HBox hbox_force_from_to, Gtk.SpinButton spinbutton_force_from, Gtk.SpinButton spinbutton_force_to)
	{
		check.Active = rfd.active;

		combo_force_function.Active = UtilGtk.ComboMakeActive(combo_force_function, rfd.FunctionPrint(true));
		combo_force_type.Active = UtilGtk.ComboMakeActive(combo_force_type, rfd.TypePrint(true));

		hbox_force_at_ms.Visible = false;
		hbox_force_at_percent.Visible = false;
		hbox_force_from_to.Visible = false;

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
	}

	private List<ForceSensorRFD> getRFDValues()
	{
		List<ForceSensorRFD> l = new List<ForceSensorRFD>();
		l.Add(getRFDValue("RFD1", check_force_1, combo_force_1_function, combo_force_1_type,
					spinbutton_force_1_at_ms, spinbutton_force_1_at_percent,
					spinbutton_force_1_from, spinbutton_force_1_to));
		l.Add(getRFDValue("RFD2", check_force_2, combo_force_2_function, combo_force_2_type,
					spinbutton_force_2_at_ms, spinbutton_force_2_at_percent,
					spinbutton_force_2_from, spinbutton_force_2_to));
		l.Add(getRFDValue("RFD3", check_force_3, combo_force_3_function, combo_force_3_type,
					spinbutton_force_3_at_ms, spinbutton_force_3_at_percent,
					spinbutton_force_3_from, spinbutton_force_3_to));
		l.Add(getRFDValue("RFD4", check_force_4, combo_force_4_function, combo_force_4_type,
					spinbutton_force_4_at_ms, spinbutton_force_4_at_percent,
					spinbutton_force_4_from, spinbutton_force_4_to));
		return l;
	}
	private ForceSensorRFD getRFDValue(string code, Gtk.CheckButton check, Gtk.ComboBox combo_force_function, Gtk.ComboBox combo_force_type,
			Gtk.SpinButton spinbutton_force_at_ms, Gtk.SpinButton spinbutton_force_at_percent,
			Gtk.SpinButton spinbutton_force_from, Gtk.SpinButton spinbutton_force_to)
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
		if(typeStr == Catalog.GetString(ForceSensorRFD.Type_INSTANTANEOUS_name))
		{
			num1 = Convert.ToInt32(spinbutton_force_at_ms.Value);
			type = ForceSensorRFD.Types.INSTANTANEOUS;
		}
		else if(typeStr == Catalog.GetString(ForceSensorRFD.Type_AVERAGE_name))
		{
			num1 = Convert.ToInt32(spinbutton_force_from.Value);
			num2 = Convert.ToInt32(spinbutton_force_to.Value);
			type = ForceSensorRFD.Types.AVERAGE;
		}
		else if(typeStr == Catalog.GetString(ForceSensorRFD.Type_PERCENT_F_MAX_name))
		{
			num1 = Convert.ToInt32(spinbutton_force_at_percent.Value);
			type = ForceSensorRFD.Types.PERCENT_F_MAX;
		}
		else // (typeStr == Catalog.GetString(ForceSensorRFD.Type_RFD_MAX_name))
			type = ForceSensorRFD.Types.RFD_MAX;

		return new ForceSensorRFD(code, active, function, type, num1, num2);
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

	[Widget] Gtk.HBox hbox_force_sensor_analyze_ai_sliders_and_buttons;
	[Widget] Gtk.DrawingArea force_sensor_ai_drawingarea;
	[Widget] Gtk.HScale hscale_force_sensor_ai_a;
	[Widget] Gtk.HScale hscale_force_sensor_ai_b;
	[Widget] Gtk.Label label_force_sensor_ai_time_a;
	[Widget] Gtk.Label label_force_sensor_ai_force_a;
	[Widget] Gtk.Label label_force_sensor_ai_rfd_a;
	[Widget] Gtk.Label label_force_sensor_ai_position_a;
	[Widget] Gtk.Label label_force_sensor_ai_speed_a;
	[Widget] Gtk.Label label_force_sensor_ai_accel_a;
	[Widget] Gtk.Label label_force_sensor_ai_power_a;
	[Widget] Gtk.HBox hbox_buttons_scale_force_sensor_ai_b;
	[Widget] Gtk.Label label_force_sensor_ai_position_b;
	[Widget] Gtk.Label label_force_sensor_ai_position_diff;
	[Widget] Gtk.Label label_force_sensor_ai_speed_b;
	[Widget] Gtk.Label label_force_sensor_ai_speed_diff;
	[Widget] Gtk.Label label_force_sensor_ai_speed_average;
	[Widget] Gtk.Label label_force_sensor_ai_speed_max;
	[Widget] Gtk.Label label_force_sensor_ai_accel_b;
	[Widget] Gtk.Label label_force_sensor_ai_accel_diff;
	[Widget] Gtk.Label label_force_sensor_ai_accel_average;
	[Widget] Gtk.Label label_force_sensor_ai_accel_max;
	[Widget] Gtk.Label label_force_sensor_ai_power_b;
	[Widget] Gtk.Label label_force_sensor_ai_power_diff;
	[Widget] Gtk.Label label_force_sensor_ai_power_average;
	[Widget] Gtk.Label label_force_sensor_ai_power_max;
	[Widget] Gtk.Label label_force_sensor_ai_time_b;
	[Widget] Gtk.Label label_force_sensor_ai_time_diff;
	[Widget] Gtk.Label label_force_sensor_ai_force_b;
	[Widget] Gtk.Label label_force_sensor_ai_force_diff;
	[Widget] Gtk.Label label_force_sensor_ai_force_average;
	[Widget] Gtk.Label label_force_sensor_ai_force_max;
	[Widget] Gtk.Label label_force_sensor_ai_rfd_b;
	[Widget] Gtk.Label label_force_sensor_ai_rfd_diff;
	[Widget] Gtk.Label label_force_sensor_ai_rfd_average;
	[Widget] Gtk.Label label_force_sensor_ai_rfd_max;
	[Widget] Gtk.Label label_force_sensor_ai_max_avg_in_window_values;

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

	bool force_sensor_ai_drawingareaShown = false;

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
				SqlitePreferences.ExportGraphWidth,
				preferences.exportGraphWidth, Convert.ToInt32(spinbutton_force_sensor_export_image_width.Value));
		preferences.exportGraphHeight = Preferences.PreferencesChange(
				SqlitePreferences.ExportGraphHeight,
				preferences.exportGraphHeight, Convert.ToInt32(spinbutton_force_sensor_export_image_height.Value));
		Sqlite.Close();

		//change also spinbuttons of export sprint and runEncoder
		spinbutton_sprint_export_image_width.Value = spinbutton_force_sensor_export_image_width.Value;
		spinbutton_sprint_export_image_height.Value = spinbutton_force_sensor_export_image_height.Value;

		spinbutton_run_encoder_export_image_width.Value = spinbutton_force_sensor_export_image_width.Value;
		spinbutton_run_encoder_export_image_height.Value = spinbutton_force_sensor_export_image_height.Value;


		forceSensorExport = new ForceSensorExport (
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
			zoomFrameA = hscale_force_sensor_ai_a_BeforeZoom -1;
			zoomFrameB = hscale_force_sensor_ai_b_BeforeZoom +1;
		}
		else if(forceSensorZoomApplied &&
				Util.IsNumber(label_force_sensor_ai_time_a.Text, true) &&
				Util.IsNumber(label_force_sensor_ai_time_b.Text, true))
		{
			//invert hscales if needed
			int firstValue = Convert.ToInt32(hscale_force_sensor_ai_a.Value);
			int secondValue = Convert.ToInt32(hscale_force_sensor_ai_b.Value);
			//LogB.Information(string.Format("firstValue: {0}, secondValue: {1}", firstValue, secondValue));

			if(firstValue > secondValue) {
				int temp = firstValue;
				firstValue = secondValue;
				secondValue = firstValue;
			}

			//-1 and +1 to have the points at the edges to calcule the RFDs
			zoomFrameA = firstValue -1;
			zoomFrameB = secondValue +1;

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
				force_sensor_ai_drawingarea.Allocation.Width,
				force_sensor_ai_drawingarea.Allocation.Height,
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
		hscale_force_sensor_ai_a.Value = 1;
		hscale_force_sensor_ai_b.Value = 1;

		forceSensorAIPlot();

		//ranges should have max value the number of the lines of csv file minus the header
		hscale_force_sensor_ai_a.SetRange(1, fsAI.GetLength() -2);
		hscale_force_sensor_ai_b.SetRange(1, fsAI.GetLength() -2);
		LogB.Information(string.Format("hscale_force_sensor_ai_time_a,b,ab ranges: 1, {0}", fsAI.GetLength() -2));

		//on zoom put hscale B at the right
		if(zoomFrameB >= 0)
			hscale_force_sensor_ai_b.Value = fsAI.GetLength() -2;

		//to update values
		on_hscale_force_sensor_ai_a_value_changed (new object (), new EventArgs ());

		manage_force_sensor_ai_table_visibilities();
	}

	Gdk.Colormap colormapForceAI;// = Gdk.Colormap.System;
	Gdk.Pixmap force_sensor_ai_pixmap = null;
	Gdk.GC pen_black_force_ai; 		//signal
	Gdk.GC pen_red_force_ai; 		//RFD max
	Gdk.GC pen_gray_cont_force_ai; 		//vertical lines
	Gdk.GC pen_gray_discont_force_ai; 	//vertical lines
	Gdk.GC pen_yellow_force_ai; 		//0 force
	//Gdk.GC pen_yellow_light_force_ai; 	//feedback rectangle on analyze to differentiate from yellow AB lines
	Gdk.GC pen_blue_force_ai; 		//RFD and repetitions stuff
	Gdk.GC pen_blue_bold_force_ai; 		//repetitions stuff
	Gdk.GC pen_blue_discont_force_ai; 	//repetitions stuff
	Gdk.GC pen_blue_light_force_ai; 	//feedback rectangle on analyze to differentiate from yellow AB lines, and repetitions stuff
	Gdk.GC pen_white_force_ai; 		//white box to ensure yellow text is not overlapped
	Gdk.GC pen_green_force_ai; 		//repetitions (vertical lines) //now is trigger on
	//Gdk.GC pen_green_bold_force_ai; 	//repetitions signal
	Gdk.GC pen_green_discont_force_ai; 	//repetitions max and min

	private void forceSensorAIPlot()
	{
		//UtilGtk.ErasePaint(force_sensor_ai_drawingarea, force_sensor_ai_pixmap);

		LogB.Information(
				"forceSensorAIPlot() " +
				(pen_black_force_ai == null).ToString() +
				(colormapForceAI == null).ToString() +
				(force_sensor_ai_drawingarea == null).ToString() +
				(force_sensor_ai_pixmap == null).ToString());

		if(force_sensor_ai_pixmap == null || pen_black_force_ai == null)
			force_ai_graphs_init();

		forceSensorAIChanged = true; //to actually plot
		force_sensor_ai_drawingarea.QueueDraw(); // -- refresh
	}

	Pango.Layout layout_force_ai_text;
	Pango.Layout layout_force_ai_text_big;
	private void force_ai_graphs_init()
	{
		colormapForceAI = Gdk.Colormap.System;
		colormapForceAI.AllocColor (ref UtilGtk.BLACK,true,true);
		colormapForceAI.AllocColor (ref UtilGtk.BLUE_PLOTS,true,true);
		colormapForceAI.AllocColor (ref UtilGtk.BLUE_LIGHT,true,true);
		colormapForceAI.AllocColor (ref UtilGtk.RED_PLOTS,true,true);
		colormapForceAI.AllocColor (ref UtilGtk.GRAY,true,true);
		colormapForceAI.AllocColor (ref UtilGtk.GREEN_PLOTS,true,true);
		bool success = colormapForceAI.AllocColor (ref UtilGtk.YELLOW,true,true);
		colormapForceAI.AllocColor (ref UtilGtk.YELLOW_LIGHT,true,true);
		colormapForceAI.AllocColor (ref UtilGtk.LIGHT_BLUE_PLOTS,true,true);
		LogB.Information("Yellow success!: " + success.ToString()); //sempre dona success

		colormapForceAI.AllocColor (ref UtilGtk.WHITE,true,true);

		pen_black_force_ai = new Gdk.GC(force_sensor_ai_drawingarea.GdkWindow);
		//potser llegir els valors de la Gdk.GC
		try{
			LogB.Information("Gdk.GC screen: " + pen_black_force_ai.Screen.ToString());
		} catch { LogB.Information("CATCHED at screen"); }

		pen_blue_force_ai = new Gdk.GC(force_sensor_ai_drawingarea.GdkWindow);
		pen_red_force_ai = new Gdk.GC(force_sensor_ai_drawingarea.GdkWindow);
		pen_yellow_force_ai = new Gdk.GC(force_sensor_ai_drawingarea.GdkWindow);
		//pen_yellow_light_force_ai = new Gdk.GC(force_sensor_ai_drawingarea.GdkWindow);
		pen_blue_force_ai = new Gdk.GC(force_sensor_ai_drawingarea.GdkWindow);
		pen_blue_bold_force_ai = new Gdk.GC(force_sensor_ai_drawingarea.GdkWindow);
		pen_blue_discont_force_ai = new Gdk.GC(force_sensor_ai_drawingarea.GdkWindow);
		pen_blue_light_force_ai = new Gdk.GC(force_sensor_ai_drawingarea.GdkWindow);
		pen_white_force_ai = new Gdk.GC(force_sensor_ai_drawingarea.GdkWindow);
		pen_gray_cont_force_ai = new Gdk.GC(force_sensor_ai_drawingarea.GdkWindow);
		pen_gray_discont_force_ai = new Gdk.GC(force_sensor_ai_drawingarea.GdkWindow);
		pen_green_force_ai = new Gdk.GC(force_sensor_ai_drawingarea.GdkWindow);
		//pen_green_bold_force_ai = new Gdk.GC(force_sensor_ai_drawingarea.GdkWindow);
		pen_green_discont_force_ai = new Gdk.GC(force_sensor_ai_drawingarea.GdkWindow);

		pen_black_force_ai.Foreground = UtilGtk.BLACK;
		pen_blue_force_ai.Foreground = UtilGtk.BLUE_PLOTS;
		pen_red_force_ai.Foreground = UtilGtk.RED_PLOTS;
		pen_yellow_force_ai.Foreground = UtilGtk.YELLOW;
		//pen_yellow_light_force_ai.Foreground = UtilGtk.YELLOW_LIGHT;
		pen_blue_discont_force_ai.Foreground = UtilGtk.BLUE_LIGHT;
		pen_blue_bold_force_ai.Foreground = UtilGtk.BLUE_LIGHT;
		pen_blue_force_ai.Foreground = UtilGtk.BLUE_LIGHT;
		pen_blue_light_force_ai.Foreground = UtilGtk.LIGHT_BLUE_PLOTS;
		pen_white_force_ai.Foreground = UtilGtk.WHITE;
		pen_gray_cont_force_ai.Foreground = UtilGtk.GRAY;
		pen_gray_discont_force_ai.Foreground = UtilGtk.GRAY;
		pen_green_force_ai.Foreground = UtilGtk.GREEN_PLOTS;
		//pen_green_bold_force_ai.Foreground = UtilGtk.GREEN_PLOTS;
		pen_green_discont_force_ai.Foreground = UtilGtk.GREEN_PLOTS;

		//pen_black_force_ai.SetLineAttributes (2, Gdk.LineStyle.Solid, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);
		//this makes the lines less spiky:
		//pen_black_force_ai.SetLineAttributes (1, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);
		pen_black_force_ai.SetLineAttributes (preferences.forceSensorGraphsLineWidth, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);

		pen_blue_force_ai.SetLineAttributes (1, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);
		pen_red_force_ai.SetLineAttributes (1, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);
		pen_yellow_force_ai.SetLineAttributes (2, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);
		//pen_yellow_light_force_ai.SetLineAttributes (2, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);
		pen_blue_force_ai.SetLineAttributes (1, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);
		pen_blue_bold_force_ai.SetLineAttributes (2, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);
		pen_blue_discont_force_ai.SetLineAttributes (1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		pen_blue_light_force_ai.SetLineAttributes (1, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);
		pen_white_force_ai.SetLineAttributes (1, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);
		pen_gray_cont_force_ai.SetLineAttributes(1, Gdk.LineStyle.Solid, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		pen_gray_discont_force_ai.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		pen_green_force_ai.SetLineAttributes (1, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);
		//pen_green_bold_force_ai.SetLineAttributes (2, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);
		pen_green_discont_force_ai.SetLineAttributes (1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);

		layout_force_ai_text = new Pango.Layout (force_sensor_ai_drawingarea.PangoContext);
		layout_force_ai_text.FontDescription = Pango.FontDescription.FromString (preferences.GetFontTypeWithSize(10));
		layout_force_ai_text_big = new Pango.Layout (force_sensor_ai_drawingarea.PangoContext);
		layout_force_ai_text_big.FontDescription = Pango.FontDescription.FromString (preferences.GetFontTypeWithSize(12));
	}

	private void forcePaintAnalyzeGeneralTimeValue(int time, bool solid)
	{
		int xPx = fsAI.FscAIPoints.GetTimeInPx(1000000 * time);

		layout_force_ai_text.SetMarkup(time.ToString() + "s");
		int textWidth = 1;
		int textHeight = 1;
		layout_force_ai_text.GetPixelSize(out textWidth, out textHeight);
		force_sensor_ai_pixmap.DrawLayout (pen_gray_discont_force_ai,
				xPx - textWidth/2, force_sensor_ai_drawingarea.Allocation.Height - textHeight, layout_force_ai_text);

		//draw vertical line
		if(solid)
		{
			layout_force_ai_text.SetMarkup("Force (N)");
			layout_force_ai_text.GetPixelSize(out textWidth, out textHeight);
			force_sensor_ai_pixmap.DrawLayout (pen_gray_cont_force_ai,
					xPx - textWidth/2, 6, layout_force_ai_text);

			force_sensor_ai_pixmap.DrawLine(pen_gray_cont_force_ai,
					xPx, textHeight +6, xPx, force_sensor_ai_drawingarea.Allocation.Height - textHeight -6);
		} else
			force_sensor_ai_pixmap.DrawLine(pen_gray_discont_force_ai,
					xPx, textHeight +6, xPx, force_sensor_ai_drawingarea.Allocation.Height - textHeight -6);
	}

	private void forcePaintAnalyzeGeneralHLine(int yForce, bool solid)
	{
		int yPx = fsAI.FscAIPoints.GetForceInPx(yForce);
		//draw horizontal line

		int xPxEnd = fsAI.FscAIPoints.GetTimeInPx(fsAI.FscAIPoints.GetLastTime());

		if(solid)
			force_sensor_ai_pixmap.DrawLine(pen_gray_cont_force_ai,
					fsAI.FscAIPoints.GetTimeInPx(0), yPx, xPxEnd, yPx);
		else
			force_sensor_ai_pixmap.DrawLine(pen_gray_discont_force_ai,
					fsAI.FscAIPoints.GetTimeInPx(0), yPx, xPxEnd, yPx);

		layout_force_ai_text.SetMarkup(yForce.ToString());
		int textWidth = 1;
		int textHeight = 1;
		layout_force_ai_text.GetPixelSize(out textWidth, out textHeight);
		force_sensor_ai_pixmap.DrawLayout (pen_gray_discont_force_ai,
				fsAI.FscAIPoints.GetTimeInPx(0) - textWidth -4, yPx - textHeight/2, layout_force_ai_text);
	}


	int force_sensor_ai_allocationXOld;
	bool force_sensor_ai_sizeChanged;
	public void on_force_sensor_ai_drawingarea_configure_event (object o, ConfigureEventArgs args)
	{
		LogB.Information("CONFIGURE force_sensor_ai_drawingarea_exposeai START");
		if(force_sensor_ai_drawingarea == null)
			return;

		force_sensor_ai_drawingareaShown = true;

		//taking care of have the BeforeZoom hscales if user reescales window on a zoomed graph
		forceSensorDoGraphAI(forceSensorZoomApplied && hscale_force_sensor_ai_b_BeforeZoom > 0);

		Gdk.EventConfigure ev = args.Event;
		Gdk.Window window = ev.Window;

		Gdk.Rectangle allocation = force_sensor_ai_drawingarea.Allocation;

		if(force_sensor_ai_pixmap == null || force_sensor_ai_sizeChanged ||
				allocation.Width != force_sensor_ai_allocationXOld ||
				forceSensorAIChanged)
		{
			force_sensor_ai_pixmap = new Gdk.Pixmap (window, allocation.Width, allocation.Height, -1);

			UtilGtk.ErasePaint(force_sensor_ai_drawingarea, force_sensor_ai_pixmap);
			if(fsAI != null)
			{
				fsAI.RedoGraph(allocation.Width, allocation.Height);
				forceSensorAnalyzeManualGraphDo(allocation);
			}

			force_sensor_ai_sizeChanged = false;
		}

		force_sensor_ai_allocationXOld = allocation.Width;
		LogB.Information("CONFIGURE force_sensor_ai_drawingarea_exposeai END");
	}
	public void on_force_sensor_ai_drawingarea_expose_event (object o, ExposeEventArgs args)
	{
		LogB.Information("EXPOSE force_sensor_ai_drawingarea_expose START");
		if(force_sensor_ai_drawingarea == null)
			return;

		//needed to have mouse clicks button_press_event ()
//		force_sensor_ai_drawingarea.AddEvents((int) (Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask));
		force_sensor_ai_drawingarea.AddEvents((int) Gdk.EventMask.ButtonPressMask);

		/* in some mono installations, configure_event is not called, but expose_event yes.
		 * Do here the initialization
		 */
		Gdk.Rectangle allocation = force_sensor_ai_drawingarea.Allocation;
		//LogB.Information(string.Format("width changed?: {0}, {1}", allocation.Width, force_sensor_ai_allocationXOld));

		if(force_sensor_ai_pixmap == null || force_sensor_ai_sizeChanged ||
				allocation.Width != force_sensor_ai_allocationXOld ||
				forceSensorAIChanged)
		{
			if(forceSensorAIChanged)
				forceSensorAIChanged = false;

			force_sensor_ai_pixmap = new Gdk.Pixmap (force_sensor_ai_drawingarea.GdkWindow,
					allocation.Width, allocation.Height, -1);

			UtilGtk.ErasePaint(force_sensor_ai_drawingarea, force_sensor_ai_pixmap);
			if(fsAI != null)
				forceSensorAnalyzeManualGraphDo(allocation);

			force_sensor_ai_sizeChanged = false;
		}


		Gdk.Rectangle rect_area = args.Event.Area;

		//sometimes this is called when paint is finished
		//don't let this erase win
		//here is were actually is drawn
		if(force_sensor_ai_pixmap != null) {
			args.Event.Window.DrawDrawable(force_sensor_ai_drawingarea.Style.WhiteGC, force_sensor_ai_pixmap,
				rect_area.X, rect_area.Y,
				rect_area.X, rect_area.Y,
				rect_area.Width, rect_area.Height);
		}

		if(fsAI != null)
		{
			CairoUtil.PaintVerticalLinesAndRectangle (
					force_sensor_ai_drawingarea,
					fsAI.GetXFromSampleCount(Convert.ToInt32(hscale_force_sensor_ai_a.Value)),
					fsAI.GetXFromSampleCount(Convert.ToInt32(hscale_force_sensor_ai_b.Value)),
					true, //paint the second line and rectangle (if a != b)
					15, 0); // top/bottom of the rectangle (top is greater than at encoder to acomodate the repetition green text), bottom 0 is ok.
		
			if(fsAI.ForceMaxAvgInWindowError == "")
			{
				int yPx = fsAI.FscAIPoints.GetForceInPx(fsAI.ForceMaxAvgInWindow);

				CairoUtil.PaintSegment(force_sensor_ai_drawingarea,
						new Cairo.Color(0/256.0, 200/256.0, 0/256.0),
						fsAI.GetXFromSampleCount(fsAI.ForceMaxAvgInWindowSampleStart), yPx,
						fsAI.GetXFromSampleCount(fsAI.ForceMaxAvgInWindowSampleEnd), yPx);
				CairoUtil.PaintSegment(force_sensor_ai_drawingarea,
						new Cairo.Color(0/256.0, 200/256.0, 0/256.0),
						fsAI.GetXFromSampleCount(fsAI.ForceMaxAvgInWindowSampleStart), yPx-10,
						fsAI.GetXFromSampleCount(fsAI.ForceMaxAvgInWindowSampleStart), yPx+10);
				CairoUtil.PaintSegment(force_sensor_ai_drawingarea,
						new Cairo.Color(0/256.0, 200/256.0, 0/256.0),
						fsAI.GetXFromSampleCount(fsAI.ForceMaxAvgInWindowSampleEnd), yPx-10,
						fsAI.GetXFromSampleCount(fsAI.ForceMaxAvgInWindowSampleEnd), yPx+10);
			}
		}

		force_sensor_ai_allocationXOld = allocation.Width;

		LogB.Information("EXPOSE END");
	}

	private void on_force_sensor_ai_drawingarea_button_press_event (object o, ButtonPressEventArgs args)
	{
		//LogB.Information(string.Format("Mouse X: {0}; Mouse Y: {1}", args.Event.X, args.Event.Y));

		//if zoomed: unzoom and return
		if(forceSensorZoomApplied)
		{
			check_force_sensor_ai_zoom.Click();
			return;
		}

		//if list exists, select the repetition
		if(fsAIRepetitionMouseLimits != null)
		{
			int repetition = fsAIFindBarInPixel(args.Event.X);
			LogB.Information("Repetition: " + repetition.ToString());
			if(repetition >= 0)
			{
				double start = fsAIRepetitionMouseLimits.GetStartOfARep(repetition);
				if(start < 0)
					start = 0; //just a precaution
				double end = fsAIRepetitionMouseLimits.GetEndOfARep(repetition);
				if(end >= fsAI.GetLength() -1)
					end -= 1; //just a precaution
				LogB.Information(string.Format("start px: {0}, end px: {1}", start, end));

				//find the hscale value for this x
				//TODO: move this to forceSensor.cs
				bool startFound = false;
				bool endFound = false;
				for(int i = 0; i < fsAI.GetLength(); i++)
				{
					int xposHere = fsAI.GetXFromSampleCount(i);
					//LogB.Information(string.Format("xposHere: {0} px, startFound: {1}, endFound: {2}", xposHere, startFound, endFound));

					//with >= to solve problems of doubles
					if(! startFound && xposHere >= start)
					{
						hscale_force_sensor_ai_a.Value = i;
						//LogB.Information(string.Format("start2 sample: {0}", i));
						startFound = true;
					}

					if(! endFound && xposHere >= end)
					{
						hscale_force_sensor_ai_b.Value = i;
						//LogB.Information(string.Format("end2 sample: {0}", i));
						endFound = true;
					}
				}

				/*
				 * right now click on sets hscales but does not zoom
				 * because zoom on elastic is not working ok
				 *
				//LogB.Information("call zoom start -->");
				if(startFound && endFound)
					button_force_sensor_ai_zoom.Click();
				//LogB.Information("<-- call zoom end");
				 */
			}
		}
	}

	private bool forceSensorZoomApplied;
	private List<ForceSensorRepetition> forceSensorRepetition_lZoomApplied;
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

			//to calculate triggers
			hscale_force_sensor_ai_a_BeforeZoomTimeMS = fsAI.GetTimeMS(hscale_force_sensor_ai_a_BeforeZoom -1);

			forceSensorRepetition_lZoomApplied = fsAI.ForceSensorRepetition_l;

			forceSensorDoGraphAI(false);

			image_force_sensor_ai_zoom.Visible = false;
			image_force_sensor_ai_zoom_out.Visible = true;

		} else {
			forceSensorZoomApplied = false;

			hscale_force_sensor_ai_a_AtZoom = Convert.ToInt32(hscale_force_sensor_ai_a.Value);
			hscale_force_sensor_ai_b_AtZoom = Convert.ToInt32(hscale_force_sensor_ai_b.Value);

			forceSensorDoGraphAI(false);

			hscale_force_sensor_ai_a.Value = hscale_force_sensor_ai_a_BeforeZoom + (hscale_force_sensor_ai_a_AtZoom -1);
			hscale_force_sensor_ai_b.Value = hscale_force_sensor_ai_a_BeforeZoom + (hscale_force_sensor_ai_b_AtZoom -1);

			image_force_sensor_ai_zoom.Visible = true;
			image_force_sensor_ai_zoom_out.Visible = false;
		}
	}

	private void forceSensorAnalyzeManualGraphDo(Rectangle allocation)
	{
		if(fsAI.GetLength() == 0)
			return;

		LogB.Information("forceSensorAnalyzeManualGraphDo() START");
		fsAIRepetitionMouseLimits = new RepetitionMouseLimits();
		bool debug = false;

		button_force_sensor_image_save_rfd_manual.Sensitive = true;

		int xPxStart = fsAI.FscAIPoints.GetTimeInPx(0);
		int xPxEnd = fsAI.FscAIPoints.GetTimeInPx(fsAI.FscAIPoints.GetLastTime());

		//draw horizontal rectangle of feedback
		//if(preferences.forceSensorCaptureFeedbackActive)
		if(preferences.forceSensorCaptureFeedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.RECTANGLE)
			forceSensorSignalPlotFeedbackRectangle(fsAI.FscAIPoints, xPxEnd,
					force_sensor_ai_drawingarea, force_sensor_ai_pixmap, pen_blue_light_force_ai);

		// 1) create paintPoints
		//LogB.Information(string.Format("forceSensorAnalyzeManualGraphDo(): fsAI.FscAIPoints.Points.Count: {0}", fsAI.FscAIPoints.Points.Count));
		Gdk.Point [] paintPoints = new Gdk.Point[fsAI.FscAIPoints.Points.Count];
		for(int i = 0; i < fsAI.FscAIPoints.Points.Count; i ++)
			paintPoints[i] = fsAI.FscAIPoints.Points[i];

		forcePaintHVLines(ForceSensorGraphs.ANALYSIS_GENERAL, fsAI.FscAIPoints.ForceMax, fsAI.FscAIPoints.ForceMin, fsAI.FscAIPoints.GetLastTime(), false);

		int textWidth = 1;
		int textHeight = 1;

		//this is nice in order to save the image having the most relevant of the set
		//note max force is of all the set, BUT maxRFD is of last calculation done (moving the A or B) in order to update fsAI.LastRFDMax
		//so decide if use maxForce also of the AB interval
		bool writeForceAndRFD = true;
		if(writeForceAndRFD)
		{
			if(label_force_sensor_ai_force_max.Text != "")
			{
				layout_force_ai_text.SetMarkup("Max force: " + label_force_sensor_ai_force_max.Text);
				layout_force_ai_text.GetPixelSize(out textWidth, out textHeight);
				force_sensor_ai_pixmap.DrawLayout (pen_black_force_ai,
						xPxEnd - textWidth, 2, layout_force_ai_text);
			}

			if(label_force_sensor_ai_rfd_max.Text != "")
			{
				layout_force_ai_text.SetMarkup("Max RFD: " + label_force_sensor_ai_rfd_max.Text);
				layout_force_ai_text.GetPixelSize(out textWidth, out textHeight);
				force_sensor_ai_pixmap.DrawLayout (pen_red_force_ai,
						xPxEnd - textWidth, 2 + textHeight, layout_force_ai_text);
			}
		}

		// 2b) draw horizontal 0 line on elastic, and Y right axis
		if(fsAI.CalculedElasticPSAP)
		{
			int yPx = fsAI.FscAIPoints.GetForceInPx(0);

			layout_force_ai_text.SetMarkup("Displ (m)");
			layout_force_ai_text.GetPixelSize(out textWidth, out textHeight);
			force_sensor_ai_pixmap.DrawLayout (pen_blue_force_ai,
					xPxEnd - textWidth/2, 0, layout_force_ai_text);

			//vertical Y right axis
			force_sensor_ai_pixmap.DrawLine(pen_blue_force_ai,
					xPxEnd, textHeight, xPxEnd, force_sensor_ai_drawingarea.Allocation.Height - textHeight -6);
					//xPxEnd, textHeight, xPxEnd, yPx);

			//horizontal distance 0 line
			force_sensor_ai_pixmap.DrawLine(pen_blue_force_ai,
					xPxStart, fsAI.GetPxAtDispl(0), xPxEnd, fsAI.GetPxAtDispl(0));
			
			//print 0
			layout_force_ai_text.SetMarkup("0");
			layout_force_ai_text.GetPixelSize(out textWidth, out textHeight);
			force_sensor_ai_pixmap.DrawLayout (pen_blue_force_ai,
					xPxEnd +2, fsAI.GetPxAtDispl(0) - textHeight/2, layout_force_ai_text);

			//max (if it fits)
			if(fsAI.GetPxAtDispl(0) - fsAI.GetPxAtDispl(fsAI.FscAIPointsDispl.ForceMax) > textHeight)
			{
				//horizontal distance max line
				force_sensor_ai_pixmap.DrawLine(pen_blue_discont_force_ai,
						xPxStart, fsAI.GetPxAtDispl(fsAI.FscAIPointsDispl.ForceMax), xPxEnd, fsAI.GetPxAtDispl(fsAI.FscAIPointsDispl.ForceMax));

				//print max value
				layout_force_ai_text.SetMarkup(Util.TrimDecimals(fsAI.FscAIPointsDispl.ForceMax,2));
				layout_force_ai_text.GetPixelSize(out textWidth, out textHeight);
				force_sensor_ai_pixmap.DrawLayout (pen_blue_force_ai,
						xPxEnd +2, fsAI.GetPxAtDispl(fsAI.FscAIPointsDispl.ForceMax) - textHeight/2, layout_force_ai_text);
			}

			//min (if it fits)
			if(fsAI.GetPxAtDispl(fsAI.FscAIPointsDispl.ForceMin) < 0 && fsAI.GetPxAtDispl(fsAI.FscAIPointsDispl.ForceMin) - fsAI.GetPxAtDispl(0) > textHeight)
			{
				//horizontal distance min line
				force_sensor_ai_pixmap.DrawLine(pen_blue_discont_force_ai,
						xPxStart, fsAI.GetPxAtDispl(fsAI.FscAIPointsDispl.ForceMin), xPxEnd, fsAI.GetPxAtDispl(fsAI.FscAIPointsDispl.ForceMin));

				//print max value
				layout_force_ai_text.SetMarkup(Util.TrimDecimals(fsAI.FscAIPointsDispl.ForceMin,2));
				layout_force_ai_text.GetPixelSize(out textWidth, out textHeight);
				force_sensor_ai_pixmap.DrawLayout (pen_blue_force_ai,
						xPxEnd +2, fsAI.GetPxAtDispl(fsAI.FscAIPointsDispl.ForceMin) - textHeight/2, layout_force_ai_text);
			}
		} else
		{
			//we just need to set textHeight
			layout_force_ai_text.SetMarkup("a");
			layout_force_ai_text.GetPixelSize(out textWidth, out textHeight);
		}

		// 3) paint points as line (can be done also with DrawPoints to debug)
		if(debug)
			force_sensor_ai_pixmap.DrawPoints(pen_black_force_ai, paintPoints);
		else
			force_sensor_ai_pixmap.DrawLines(pen_black_force_ai, paintPoints);

		// 3b) create and paint points displ on elastic
		if(fsAI.CalculedElasticPSAP)
		{
			Gdk.Point [] paintPointsDispl = new Gdk.Point[fsAI.FscAIPointsDispl.Points.Count];
			for(int i = 0; i < fsAI.FscAIPointsDispl.Points.Count; i ++)
				paintPointsDispl[i] = fsAI.FscAIPointsDispl.Points[i];

			if(debug)
				force_sensor_ai_pixmap.DrawPoints(pen_blue_bold_force_ai, paintPointsDispl);
			else
				force_sensor_ai_pixmap.DrawLines(pen_blue_bold_force_ai, paintPointsDispl);

			LogB.Information(string.Format("fsAI.FscAIPoints.Points.Count: {0}, fsAI.FscAIPointsDispl.Points.Count: {1}",
						fsAI.FscAIPoints.Points.Count, fsAI.FscAIPointsDispl.Points.Count));
		}

		// 4) create hscaleLower and higher values (A, B at the moment)
		int hscaleLower = Convert.ToInt32(hscale_force_sensor_ai_a.Value);
		int hscaleHigher = Convert.ToInt32(hscale_force_sensor_ai_b.Value);

		int xposA = fsAI.GetXFromSampleCount(hscaleLower);
		int xposB = 0;
		if(hscaleLower != hscaleHigher)
			xposB = fsAI.GetXFromSampleCount(hscaleHigher);

		if(fsAI.CalculedElasticPSAP)
		{
			hbox_force_sensor_ai_position.Visible = true;
			hbox_force_sensor_ai_speed.Visible = true;
			hbox_force_sensor_ai_accel.Visible = true;
			hbox_force_sensor_ai_power.Visible = true;
		} else {
			hbox_force_sensor_ai_position.Visible = false;
			hbox_force_sensor_ai_speed.Visible = false;
			hbox_force_sensor_ai_accel.Visible = false;
			hbox_force_sensor_ai_power.Visible = false;
		}

		// 6) paint repetitions info (vertical line and number)
		List<ForceSensorRepetition> reps_l = fsAI.ForceSensorRepetition_l;
		if(forceSensorZoomApplied)
			reps_l = forceSensorRepetition_lZoomApplied;

		int xposRepStart = 0;
		int xposRepEnd = 0;
		int j = 0;
		for(j = 0; j < reps_l.Count; j ++)
		{
			LogB.Information(string.Format("repetition: {0}", reps_l[j]));
			int sampleStart = reps_l[j].sampleStart;
			int sampleEnd = reps_l[j].sampleEnd;
			if(forceSensorZoomApplied)
			{
				sampleStart -= hscale_force_sensor_ai_a_BeforeZoom -1;
				sampleEnd -= hscale_force_sensor_ai_a_BeforeZoom -1;

				//LogB.Information(string.Format("reps_l[j].sampleEnd: {0}, hscale_force_sensor_ai_b_BeforeZoom: {1}",
				//			reps_l[j].sampleEnd, hscale_force_sensor_ai_b_BeforeZoom));

				if(reps_l[j].sampleEnd -1 > hscale_force_sensor_ai_b_BeforeZoom)
				{
					if(sampleStart >= 0) //precaution
						xposRepStart = fsAI.GetXFromSampleCount(sampleStart);
					xposRepEnd = fsAI.GetXFromSampleCount(
							hscale_force_sensor_ai_b_BeforeZoom - hscale_force_sensor_ai_a_BeforeZoom -1);

					break;
				}
			}

			// paint vertical line for each rep
			if(sampleStart >= 0) {
				xposRepStart = fsAI.GetXFromSampleCount(sampleStart);
				//no need to graph two green lines together if rep starts just on previous rep ends
				if(xposRepStart > xposRepEnd)
					force_sensor_ai_pixmap.DrawLine(pen_blue_force_ai,
							xposRepStart, textHeight +6, xposRepStart, allocation.Height - textHeight -6);
			}
			if(sampleEnd >= 0) {
				xposRepEnd = fsAI.GetXFromSampleCount(sampleEnd);
				force_sensor_ai_pixmap.DrawLine(pen_blue_force_ai,
						xposRepEnd, textHeight +6, xposRepEnd, allocation.Height -textHeight -6);
				//LogB.Information(string.Format("repetition paint, j: {0}, xposRepEnd: {1}", j, xposRepEnd));
			}

			if(sampleEnd >= 0)
			{
				if(sampleStart >= 0)
				{
					forceSensorWriteRepetitionCode (j, reps_l[j].TypeShort(), xposRepStart, xposRepEnd, true, true);

					if(! forceSensorZoomApplied)
						fsAIRepetitionMouseLimits.Add(xposRepStart, xposRepEnd);
				} else {
					//write the rep and arrow, but just if there is enough space
					if(xposRepEnd - fsAI.GetXFromSampleCount(0) > 30)
						forceSensorWriteRepetitionCode (j, reps_l[j].TypeShort(),
								//fsAI.GetXFromSampleCount(hscale_force_sensor_ai_a_BeforeZoom),
								fsAI.GetXFromSampleCount(0),
								xposRepEnd, false, true);
				}
			}
		}

		//show the start vertical line and the number of last repetition (when obviously no new rep will make writting it)
		//but only if zoomed and that repetition exists (has an end)
		if(forceSensorZoomApplied && j >= 0 && j < reps_l.Count)
		{
			//write the vertical start line
			force_sensor_ai_pixmap.DrawLine(pen_blue_force_ai,
					xposRepStart, textHeight +6, xposRepStart, allocation.Height - textHeight -6);

			//write last repetition count
			if(xposRepEnd - xposRepStart > 30)
				forceSensorWriteRepetitionCode (j, reps_l[j].TypeShort(), xposRepStart, xposRepEnd, true, false);
		}

		/*
		 * 7) Invert AB if needed to paint correctly blue and red lines
		 * making it work also when B is higher than A
		 */
		if(hscaleLower > hscaleHigher)
		{
			hscaleLower = Convert.ToInt32(hscale_force_sensor_ai_b.Value);
			hscaleHigher = Convert.ToInt32(hscale_force_sensor_ai_a.Value);
			int temp = xposA;
			xposA = xposB;
			xposB = temp;
		}

		if(hscaleHigher != hscaleLower)
		{
			// 8) calculate and paint RFD
			double forceA = fsAI.GetForceAtCount(hscaleLower);
			double forceB = fsAI.GetForceAtCount(hscaleHigher);

			force_sensor_ai_pixmap.DrawLine(pen_blue_force_ai,
					xposA, fsAI.GetPxAtForce(forceA),
					xposB, fsAI.GetPxAtForce(forceB));

			// 9) calculate and paint max RFD (circle and line)
			//value of count that produce the max RFD (between the previous and next value)

			if(hscaleLower <= 0 || hscaleHigher >= fsAI.GetLength() -1)
				return;

			int countRFDMax = fsAI.LastRFDMaxCount;

			int rfdX = fsAI.GetXFromSampleCount(countRFDMax);
			int rfdY = fsAI.GetPxAtForce(fsAI.GetForceAtCount(countRFDMax));

			// draw a circle of 12 points width/length, move it 6 points top/left to have it centered
			force_sensor_ai_pixmap.DrawArc(pen_red_force_ai, false,
					rfdX -6, rfdY -6,
					12, 12, 90 * 64, 360 * 64);

			// plot tangent line
			if(countRFDMax -1 >= 0 && countRFDMax +1 < fsAI.GetLength() -1)
			{
				//calculate line
				int lineXStart; int lineXEnd;
				int lineYStart; int lineYEnd;
				fsAI.CalculateRFDTangentLine(countRFDMax, out lineXStart, out lineXEnd, out lineYStart, out lineYEnd);
				force_sensor_ai_pixmap.DrawLine(pen_red_force_ai, lineXStart, lineYStart, lineXEnd, lineYEnd);

				if(debug)
					plotRFDLineDebugConstruction(countRFDMax);
			}
		}

		// triggers (also on zoom)
		int firstCount = 0;
		double firstMs = fsAI.GetTimeMS(firstCount);
		int xFirstPos = fsAI.GetXFromSampleCount(firstCount);
		//LogB.Information(string.Format("no zoom: firstCount: {0}, firstMs: {1}, xFirstPos: {2}", firstCount, firstMs, xFirstPos));

		int lastCount = fsAI.GetLength() -1;
		double lastMs = fsAI.GetTimeMS(lastCount);
		int xLastPos = fsAI.GetXFromSampleCount(lastCount);
		//LogB.Information(string.Format("no zoom: lastCount: {0}, lastMs: {1}, xLastPos: {2}", lastCount, lastMs, xLastPos));

		foreach(Trigger trigger in triggerListForceSensor.GetList())
		{
			//write the vertical start line
			//int tempX = fsAI.GetXFromSampleCount(trigger.Ms); not because is not a count

			double triggerPercent1 = UtilAll.DivideSafe(trigger.Ms, lastMs-firstMs);
			if(forceSensorZoomApplied)
			{
				//on zoom, 0 seconds has displaced, correct trigger.Ms according to that displacement
				triggerPercent1 = UtilAll.DivideSafe(
						trigger.Ms -hscale_force_sensor_ai_a_BeforeZoomTimeMS,
						lastMs-firstMs);
				if(triggerPercent1 < 0 || triggerPercent1 > 1)
					continue;
			}

			int xtrigger = Convert.ToInt32(triggerPercent1 * (xLastPos - xFirstPos)) + xFirstPos;

			Gdk.GC myPen = pen_green_force_ai; //in
			if(! trigger.InOut)
				myPen = pen_red_force_ai; //out

			force_sensor_ai_pixmap.DrawLine(myPen,
					xtrigger, textHeight +6, xtrigger, allocation.Height - textHeight -6);
		}

		LogB.Information("forceSensorAnalyzeManualGraphDo() END");
	}

	private void forceSensorWriteRepetitionCode (int number, string type, int xposRepStart, int xposRepEnd, bool endsAtLeft, bool endsAtRight)
	{
//		LogB.Information(string.Format("at forceSensorWriteRepetitionNumber with (rep+1): {0}, endsAtLeft: {1}, endsAtRight: {2}", rep +1, endsAtLeft, endsAtRight));
//		layout_force_ai_text.SetMarkup((rep+1).ToString());

		//if(! currentForceSensorExercise.EccReps)
		if(currentForceSensorExercise.RepetitionsShow == ForceSensorExercise.RepetitionsShowTypes.CONCENTRIC ||
				currentForceSensorExercise.RepetitionsShow == ForceSensorExercise.RepetitionsShowTypes.BOTHTOGETHER)
			layout_force_ai_text.SetMarkup((number+1).ToString());
		else
			layout_force_ai_text.SetMarkup(string.Format("{0}{1}", Math.Ceiling((number +1)/2.0), type));

		int textWidth = 1; int textHeight = 1;
		layout_force_ai_text.GetPixelSize(out textWidth, out textHeight);

		int xposNumber = Convert.ToInt32((xposRepStart + xposRepEnd)/2 - textWidth/2);

		force_sensor_ai_pixmap.DrawLayout (pen_blue_force_ai,
				xposNumber, 6, layout_force_ai_text);

		//if it does not fit, do not plot the horizontal lines + arrows
		if(xposNumber - xposRepStart < 16)
			return;

		//draw arrow to the left
		UtilGtk.DrawHorizontalLine(force_sensor_ai_pixmap, pen_blue_force_ai, xposRepStart, xposNumber, 6 + textHeight/2,
			6, endsAtLeft, false, 4);
		UtilGtk.DrawHorizontalLine(force_sensor_ai_pixmap, pen_blue_force_ai, xposNumber + textWidth, xposRepEnd, 6 + textHeight/2,
			6, false, endsAtRight, 4);
	}

	private int fsAIFindBarInPixel (double pixel)
	{
		if(fsAIRepetitionMouseLimits == null)
			return -1;

		return fsAIRepetitionMouseLimits.FindBarInPixel(pixel);
	}

	private void plotRFDLineDebugConstruction(int countRFDMax)
	{
		/*
		 * debug plotting points before and after RFD
		 * draw a circle of 6 points width/length, move it 3 points top/left to have it centered
		 */
		int debugPointsBeforeRFD = 4;
		int debugPointsAfterRFD = 4;
		for(int i = countRFDMax - debugPointsBeforeRFD; i <= countRFDMax + debugPointsAfterRFD; i ++)
		{
			if(i < 0 || i > fsAI.GetLength() -1)
				continue;

			int segXDebug = fsAI.GetXFromSampleCount(i);
			int segYDebug = fsAI.GetPxAtForce(fsAI.GetForceAtCount(i));
			force_sensor_ai_pixmap.DrawArc(pen_black_force_ai, false,
					segXDebug -3, segYDebug -3,
					6, 6, 90 * 64, 360 * 64);
		}
	}

	bool forceSensorAIChanged = false;
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
		if(count > 0 && count >= fsAI.GetLength() -1)
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
		forceSensorAIChanged = true;
		force_sensor_ai_drawingarea.QueueDraw(); //will fire ExposeEvent
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
		if(count > 0 && count >= fsAI.GetLength() -1)
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
		forceSensorAIChanged = true;
		force_sensor_ai_drawingarea.QueueDraw(); //will fire ExposeEvent
	}

	private void on_check_force_sensor_ai_chained_clicked (object o, EventArgs args)
	{
		image_force_sensor_ai_chained_link.Visible = check_force_sensor_ai_chained.Active;
		image_force_sensor_ai_chained_link_off.Visible = ! check_force_sensor_ai_chained.Active;
	}

	private void forceSensorAnalyzeGeneralButtonHscaleZoomSensitiveness()
	{
		button_hscale_force_sensor_ai_a_first.Sensitive = hscale_force_sensor_ai_a.Value > 1;
		button_hscale_force_sensor_ai_a_pre.Sensitive = hscale_force_sensor_ai_a.Value > 1;
		button_hscale_force_sensor_ai_b_first.Sensitive = hscale_force_sensor_ai_b.Value > 1;
		button_hscale_force_sensor_ai_b_pre.Sensitive = hscale_force_sensor_ai_b.Value > 1;

		button_hscale_force_sensor_ai_a_last.Sensitive = hscale_force_sensor_ai_a.Value < fsAI.GetLength() -2;
		button_hscale_force_sensor_ai_a_post.Sensitive = hscale_force_sensor_ai_a.Value < fsAI.GetLength() -2;
		button_hscale_force_sensor_ai_b_last.Sensitive = hscale_force_sensor_ai_b.Value < fsAI.GetLength() -2;
		button_hscale_force_sensor_ai_b_post.Sensitive = hscale_force_sensor_ai_b.Value < fsAI.GetLength() -2;

		//diff have to be more than one pixel
		check_force_sensor_ai_zoom.Sensitive = (Math.Abs(hscale_force_sensor_ai_a.Value - hscale_force_sensor_ai_b.Value) > 1);
	}

	private void on_button_hscale_force_sensor_ai_a_first_clicked (object o, EventArgs args)
	{
		hscale_force_sensor_ai_a.Value = 1;
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

		hscale_force_sensor_ai_a.Value = fsAI.GetLength() -2;
	}

	private void on_button_hscale_force_sensor_ai_b_first_clicked (object o, EventArgs args)
	{
		hscale_force_sensor_ai_b.Value = 1;
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

		hscale_force_sensor_ai_b.Value = fsAI.GetLength() -2;
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
		int countA = Convert.ToInt32(hscale_force_sensor_ai_a.Value);
		int countB = Convert.ToInt32(hscale_force_sensor_ai_b.Value);

		//avoid problems of GTK misreading of hscale on a notebook change or load file
		if(countA < 0 || countA > fsAI.GetLength() -1 || countB < 0 || countB > fsAI.GetLength() -1)
			return;

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

				if(fsAI.ForceMaxAvgInWindowError == "")
					label_force_sensor_ai_max_avg_in_window_values.Text = Math.Round(fsAI.ForceMaxAvgInWindow, 1).ToString();
				else
					label_force_sensor_ai_max_avg_in_window_values.Text = "----";
			} else {
				label_force_sensor_ai_force_average.Text = "";
				label_force_sensor_ai_force_max.Text = "";
				label_force_sensor_ai_max_avg_in_window_values.Text = "";
			}
		}

		if(fsAI.CalculedElasticPSAP && success)
		{
			double positionA = fsAI.Position_l[countA];
			double positionB = fsAI.Position_l[countB];
			label_force_sensor_ai_position_diff.Text = Math.Round(positionB - positionA, 3).ToString();

			double speedA = fsAI.Speed_l[countA];
			double speedB = fsAI.Speed_l[countB];
			label_force_sensor_ai_speed_diff.Text = Math.Round(speedB - speedA, 3).ToString();
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
			label_force_sensor_ai_impulse_values.Text = Math.Round(fsAI.CalculateImpulse(
						countA, countB), 1).ToString();

			// 11) calculate variability
			double variability = 0;
			double feedbackDiff = 0;
			int feedbackF = preferences.forceSensorCaptureFeedbackAt;

			fsAI.CalculateVariabilityAndAccuracy(countA, countB, feedbackF, out variability, out feedbackDiff,
					preferences.forceSensorVariabilityMethod);

			label_force_sensor_ai_variability_values.Text = Math.Round(variability, 3).ToString();

			// 12) calculate Accuracy (Feedback difference)
			//if(preferences.forceSensorCaptureFeedbackActive && feedbackF > 0)
			if(preferences.forceSensorCaptureFeedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.RECTANGLE && feedbackF > 0)
			{
				label_force_sensor_ai_feedback_values.Text = Math.Round(feedbackDiff, 3).ToString();
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

}
