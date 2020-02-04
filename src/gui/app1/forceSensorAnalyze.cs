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
 * Copyright (C) 2018   Xavier de Blas <xaviblas@gmail.com>
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
	[Widget] Gtk.HBox hbox_force_sensor_analyze_modes;
	[Widget] Gtk.Button button_force_sensor_analyze_load;
	[Widget] Gtk.Button button_force_sensor_analyze_analyze;
	[Widget] Gtk.Label label_force_sensor_analyze;
	[Widget] Gtk.Image image_force_sensor_graph;
	[Widget] Gtk.Viewport viewport_force_sensor_graph;
	[Widget] Gtk.Button button_force_sensor_image_save_rfd_auto;
	[Widget] Gtk.Button button_force_sensor_image_save_rfd_manual;
	[Widget] Gtk.ScrolledWindow scrolledwindow_force_sensor_ai;
	[Widget] Gtk.Button button_force_sensor_analyze_AB_save;
	[Widget] Gtk.Button button_force_sensor_ai_zoom;

	[Widget] Gtk.SpinButton spin_force_duration_seconds;
	[Widget] Gtk.RadioButton radio_force_duration_seconds;
	[Widget] Gtk.HBox hbox_force_rfd_duration_percent;
	[Widget] Gtk.RadioButton radio_force_rfd_duration_percent;
	[Widget] Gtk.SpinButton spin_force_rfd_duration_percent;

	//analyze options
	[Widget] Gtk.Notebook notebook_force_sensor_analyze; //decide between automatic and manual
	[Widget] Gtk.RadioButton radiobutton_force_sensor_analyze_automatic;
	[Widget] Gtk.RadioButton radiobutton_force_sensor_analyze_manual;
	[Widget] Gtk.HBox hbox_force_sensor_analyze_automatic_options;
	[Widget] Gtk.Notebook notebook_force_analyze_automatic;
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

	[Widget] Gtk.VBox vbox_force_sensor_ai_impulse_variability_and_feedback;
	[Widget] Gtk.VBox vbox_force_sensor_ai_feedback;
	[Widget] Gtk.Label label_force_sensor_ai_impulse_values;
	[Widget] Gtk.Label label_force_sensor_ai_variability_values;
	[Widget] Gtk.Label label_force_sensor_ai_feedback_values;

	/*
	 * analyze options -------------------------->
	 */

	private bool button_force_sensor_analyze_analyze_was_sensitive; //needed this temp variable
	private void forceSensorAnalyzeOptionsSensitivity(bool s) //s for sensitive. When show options frame is ! s
	{
		button_force_sensor_analyze_options.Sensitive = s;
		button_force_sensor_analyze_load.Sensitive = s;
		hbox_force_sensor_analyze_modes.Sensitive = s;

		if(s)
			button_force_sensor_analyze_analyze.Sensitive = button_force_sensor_analyze_analyze_was_sensitive;
		else {
			button_force_sensor_analyze_analyze_was_sensitive =	button_force_sensor_analyze_analyze.Sensitive;
			button_force_sensor_analyze_analyze.Sensitive = false;
		}

		main_menu.Sensitive = s;
		notebook_session_person.Sensitive = s;
		hbox_contacts_sup_capture_analyze_two_buttons.Sensitive = s;
		hbox_top_person.Sensitive = s;
	}

	private void on_button_force_sensor_analyze_options_clicked (object o, EventArgs args)
	{
		notebook_force_analyze_automatic.CurrentPage = 1;
		forceSensorAnalyzeOptionsSensitivity(false);
	}

	private void on_button_force_sensor_analyze_options_close_clicked (object o, EventArgs args)
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

		notebook_force_analyze_automatic.CurrentPage = 0;
		forceSensorAnalyzeOptionsSensitivity(true);
	}

	private void on_button_force_sensor_analyze_options_close_and_analyze_clicked (object o, EventArgs args)
	{
		on_button_force_sensor_analyze_options_close_clicked (o, args);
		on_button_force_sensor_analyze_analyze_clicked (o, args);
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
		//TODO: assignar aquí lo que hi hagi de les preferencies: del radio actiu is els dos spinbuttons
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

	[Widget] Gtk.DrawingArea force_sensor_ai_drawingarea;
	[Widget] Gtk.HScale hscale_force_sensor_ai_a;
	[Widget] Gtk.HScale hscale_force_sensor_ai_b;
	[Widget] Gtk.HScale hscale_force_sensor_ai_ab;
	[Widget] Gtk.CheckButton checkbutton_force_sensor_ai_b;
	[Widget] Gtk.Label label_force_sensor_ai_time_a;
	[Widget] Gtk.Label label_force_sensor_ai_force_a;
	[Widget] Gtk.Label label_force_sensor_ai_rfd_a;
	[Widget] Gtk.Label label_force_sensor_ai_position_a;
	[Widget] Gtk.Label label_force_sensor_ai_speed_a;
	[Widget] Gtk.Label label_force_sensor_ai_accel_a;
	[Widget] Gtk.Label label_force_sensor_ai_power_a;
	[Widget] Gtk.HBox hbox_buttons_scale_force_sensor_ai_b;
	[Widget] Gtk.Label label_force_sensor_ai_b;
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
	[Widget] Gtk.Label label_force_sensor_ai_diff;
	[Widget] Gtk.Label label_force_sensor_ai_average;
	[Widget] Gtk.Label label_force_sensor_ai_max;
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

	ForceSensorAnalyzeInstant fsAI;

	private void on_radiobutton_force_sensor_analyze_automatic_toggled (object o, EventArgs args)
	{
		if(! radiobutton_force_sensor_analyze_automatic.Active)
			return;

		hbox_force_sensor_analyze_automatic_options.Visible = true;
		notebook_force_sensor_analyze.CurrentPage = 0;
	}
	bool force_sensor_ai_drawingareaShown = false;
	private void on_radiobutton_force_sensor_analyze_manual_toggled (object o, EventArgs args)
	{
		if(! radiobutton_force_sensor_analyze_manual.Active)
			return;

		hbox_force_sensor_analyze_automatic_options.Visible = false;
		notebook_force_sensor_analyze.CurrentPage = 1;
		force_sensor_ai_drawingareaShown = true;
		forceSensorDoGraphAI();
	}

	private void forceSensorDoGraphAI()
	{
		if(lastForceSensorFullPath == null || lastForceSensorFullPath == "")
			return;

		double zoomA = -1;
		double zoomB = -1;
		if(forceSensorZoomApplied && checkbutton_force_sensor_ai_b.Active &&
				Util.IsNumber(label_force_sensor_ai_time_a.Text, true) &&
				Util.IsNumber(label_force_sensor_ai_time_b.Text, true))
		{
			//invert hscales if needed
			int firstValue = Convert.ToInt32(hscale_force_sensor_ai_a.Value);
			int secondValue = Convert.ToInt32(hscale_force_sensor_ai_b.Value);

			if(firstValue > secondValue) {
				int temp = firstValue;
				firstValue = secondValue;
				secondValue = firstValue;
			}

			//-1 and +1 to have the points at the edges to calcule the RFDs
			zoomA = fsAI.GetTimeMS(firstValue -1) * 1000;
			zoomB = fsAI.GetTimeMS(secondValue +1) * 1000;

			//do not zoom if both are the same
			if(zoomA == zoomB)
			{
				zoomA = -1;
				zoomB = -1;
			}
		}

		//pass them as doubles
		double eccMinDispl;
		double conMinDispl;
		if(currentForceSensorExercise.Elastic)
		{
			eccMinDispl = preferences.forceSensorElasticEccMinDispl;
			conMinDispl = preferences.forceSensorElasticConMinDispl;
		} else {
			eccMinDispl = preferences.forceSensorNotElasticEccMinForce;
			conMinDispl = preferences.forceSensorNotElasticConMinForce;
		}

		fsAI = new ForceSensorAnalyzeInstant(
				lastForceSensorFullPath,
				force_sensor_ai_drawingarea.Allocation.Width,
				force_sensor_ai_drawingarea.Allocation.Height,
				zoomA, zoomB,
				currentForceSensorExercise, currentPersonSession.Weight,
				getForceSensorCaptureOptions(), currentForceSensor.Stiffness,
				eccMinDispl, conMinDispl);

		/*
		 * position the hscales on the left to avoid loading a csv
		 * with less data rows than current csv and having scales out of the graph
		//hscale_force_sensor_ai_a.ValuePos = Gtk.PositionType.Left; //doesn't work
		//hscale_force_sensor_ai_b.ValuePos = Gtk.PositionType.Left; //doesn't work
		*/
		hscale_force_sensor_ai_a.Value = 1;
		hscale_force_sensor_ai_b.Value = 1;
		force_sensor_last_ab = 1;

		forceSensorAIPlot();

		//ranges should have max value the number of the lines of csv file minus the header
		hscale_force_sensor_ai_a.SetRange(1, fsAI.GetLength() -2);
		hscale_force_sensor_ai_b.SetRange(1, fsAI.GetLength() -2);
		hscale_force_sensor_ai_ab.SetRange(1, fsAI.GetLength() -2);

		//on zoom put hscale B at the right
		if(zoomB >= 0)
			hscale_force_sensor_ai_b.Value = fsAI.GetLength() -1;

		//to update values
		on_hscale_force_sensor_ai_a_value_changed (new object (), new EventArgs ());

		manage_force_sensor_ai_table_visibilities();
	}

	Gdk.Colormap colormapForceAI;// = Gdk.Colormap.System;
	Gdk.Pixmap force_sensor_ai_pixmap = null;
	Gdk.GC pen_black_force_ai; 		//signal
	Gdk.GC pen_blue_force_ai; 		//RFD
	Gdk.GC pen_red_force_ai; 		//RFD max
	Gdk.GC pen_gray_discont_force_ai; 	//vertical lines
	Gdk.GC pen_yellow_force_ai; 		//0 force
	Gdk.GC pen_yellow_light_force_ai; 	//feedback rectangle on analyze to differentiate from yellow AB lines
	Gdk.GC pen_white_force_ai; 		//white box to ensure yellow text is not overlapped
	Gdk.GC pen_green_force_ai; 		//repetitions (vertical lines)

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
		colormapForceAI.AllocColor (ref UtilGtk.RED_PLOTS,true,true);
		colormapForceAI.AllocColor (ref UtilGtk.GRAY,true,true);
		colormapForceAI.AllocColor (ref UtilGtk.GREEN_PLOTS,true,true);
		bool success = colormapForceAI.AllocColor (ref UtilGtk.YELLOW,true,true);
		colormapForceAI.AllocColor (ref UtilGtk.YELLOW_LIGHT,true,true);
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
		pen_yellow_light_force_ai = new Gdk.GC(force_sensor_ai_drawingarea.GdkWindow);
		pen_white_force_ai = new Gdk.GC(force_sensor_ai_drawingarea.GdkWindow);
		pen_gray_discont_force_ai = new Gdk.GC(force_sensor_ai_drawingarea.GdkWindow);
		pen_green_force_ai = new Gdk.GC(force_sensor_ai_drawingarea.GdkWindow);

		pen_black_force_ai.Foreground = UtilGtk.BLACK;
		pen_blue_force_ai.Foreground = UtilGtk.BLUE_PLOTS;
		pen_red_force_ai.Foreground = UtilGtk.RED_PLOTS;
		pen_yellow_force_ai.Foreground = UtilGtk.YELLOW;
		pen_yellow_light_force_ai.Foreground = UtilGtk.YELLOW_LIGHT;
		pen_white_force_ai.Foreground = UtilGtk.WHITE;
		pen_gray_discont_force_ai.Foreground = UtilGtk.GRAY;
		pen_green_force_ai.Foreground = UtilGtk.GREEN_PLOTS;

		//pen_black_force_ai.SetLineAttributes (2, Gdk.LineStyle.Solid, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);
		//this makes the lines less spiky:
		//pen_black_force_ai.SetLineAttributes (1, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);
		pen_black_force_ai.SetLineAttributes (preferences.forceSensorGraphsLineWidth, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);

		pen_blue_force_ai.SetLineAttributes (1, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);
		pen_red_force_ai.SetLineAttributes (1, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);
		pen_yellow_force_ai.SetLineAttributes (2, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);
		pen_yellow_light_force_ai.SetLineAttributes (2, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);
		pen_white_force_ai.SetLineAttributes (1, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);
		pen_gray_discont_force_ai.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		pen_green_force_ai.SetLineAttributes (1, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);

		layout_force_ai_text = new Pango.Layout (force_sensor_ai_drawingarea.PangoContext);
		layout_force_ai_text.FontDescription = Pango.FontDescription.FromString ("Courier 10");
		layout_force_ai_text_big = new Pango.Layout (force_sensor_ai_drawingarea.PangoContext);
		layout_force_ai_text_big.FontDescription = Pango.FontDescription.FromString ("Courier 12");
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
			force_sensor_ai_pixmap.DrawLine(pen_gray_discont_force_ai,
					xPx, 4, xPx, force_sensor_ai_drawingarea.Allocation.Height - textHeight -4);
		else
			force_sensor_ai_pixmap.DrawLine(pen_gray_discont_force_ai,
					xPx, 4, xPx, force_sensor_ai_drawingarea.Allocation.Height - textHeight -4);
	}

	private void forcePaintAnalyzeGeneralHLine(int yForce, bool solid)
	{
		int yPx = fsAI.FscAIPoints.GetForceInPx(yForce);
		//draw horizontal line
		if(solid)
			force_sensor_ai_pixmap.DrawLine(pen_gray_discont_force_ai,
					fsAI.FscAIPoints.GetTimeInPx(0), yPx, force_sensor_ai_drawingarea.Allocation.Width, yPx);
		else
			force_sensor_ai_pixmap.DrawLine(pen_gray_discont_force_ai,
					fsAI.FscAIPoints.GetTimeInPx(0), yPx, force_sensor_ai_drawingarea.Allocation.Width, yPx);

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
		forceSensorDoGraphAI();

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

		Gdk.Rectangle area = args.Event.Area;

		//sometimes this is called when paint is finished
		//don't let this erase win
		if(force_sensor_ai_pixmap != null) {
			args.Event.Window.DrawDrawable(force_sensor_ai_drawingarea.Style.WhiteGC, force_sensor_ai_pixmap,
				area.X, area.Y,
				area.X, area.Y,
				area.Width, area.Height);
		}

		force_sensor_ai_allocationXOld = allocation.Width;
		LogB.Information("EXPOSE END");
	}


	private bool forceSensorZoomApplied;
	private void forceSensorZoomDefaultValues()
	{
		forceSensorZoomApplied = false;
		button_force_sensor_ai_zoom.Label = "Zoom [A-B]";
	}
	private int hscale_force_sensor_ai_a_BeforeZoom = 0;
	private int hscale_force_sensor_ai_a_AtZoom = 0;
	private int hscale_force_sensor_ai_b_BeforeZoom = 0;
	private int hscale_force_sensor_ai_b_AtZoom = 0;
	private void on_button_force_sensor_ai_zoom_clicked (object o, EventArgs args)
	{
		forceSensorZoomApplied = ! forceSensorZoomApplied;

		if(forceSensorZoomApplied)
		{
			button_force_sensor_ai_zoom.Label = "Zoom out";
			checkbutton_force_sensor_ai_b.Sensitive = false;
		} else {
			button_force_sensor_ai_zoom.Label = "Zoom [A-B]";
			checkbutton_force_sensor_ai_b.Sensitive = true;
		}

		//store hscale a to help return to position on unzoom
		if(forceSensorZoomApplied) {
			hscale_force_sensor_ai_a_BeforeZoom = Convert.ToInt32(hscale_force_sensor_ai_a.Value);
			hscale_force_sensor_ai_b_BeforeZoom = Convert.ToInt32(hscale_force_sensor_ai_b.Value);
		} else {
			hscale_force_sensor_ai_a_AtZoom = Convert.ToInt32(hscale_force_sensor_ai_a.Value);
			hscale_force_sensor_ai_b_AtZoom = Convert.ToInt32(hscale_force_sensor_ai_b.Value);
		}

		forceSensorDoGraphAI();

		if(! forceSensorZoomApplied)
		{
			hscale_force_sensor_ai_a.Value = hscale_force_sensor_ai_a_BeforeZoom + (hscale_force_sensor_ai_a_AtZoom -1);
			hscale_force_sensor_ai_b.Value = hscale_force_sensor_ai_a_BeforeZoom + (hscale_force_sensor_ai_b_AtZoom -1);
		}
	}

	private void forceSensorAnalyzeManualGraphDo(Rectangle allocation)
	{
		if(fsAI.GetLength() == 0)
			return;

		LogB.Information("forceSensorAnalyzeManualGraphDo() START");
		bool debug = false;

		button_force_sensor_image_save_rfd_manual.Sensitive = true;
		if(! forceSensorZoomApplied)
			checkbutton_force_sensor_ai_b.Sensitive = true;
		if(checkbutton_force_sensor_ai_b.Active)
			button_force_sensor_analyze_AB_save.Visible = true;

		//draw horizontal rectangle of feedback
		if(preferences.forceSensorCaptureFeedbackActive)
			forceSensorSignalPlotFeedbackRectangle(fsAI.FscAIPoints, force_sensor_ai_drawingarea, force_sensor_ai_pixmap, pen_yellow_light_force_ai);

		// 1) create paintPoints
		Gdk.Point [] paintPoints = new Gdk.Point[fsAI.FscAIPoints.Points.Count];
		for(int i = 0; i < fsAI.FscAIPoints.Points.Count; i ++)
			paintPoints[i] = fsAI.FscAIPoints.Points[i];

		forcePaintHVLines(ForceSensorGraphs.ANALYSIS_GENERAL, fsAI.FscAIPoints.ForceMax, fsAI.FscAIPoints.ForceMin, forceSensorValues.TimeLast, false);

		// 2) draw horizontal 0 line
		force_sensor_ai_pixmap.DrawLine(pen_gray_discont_force_ai,
				0, fsAI.GetPxAtForce(0), allocation.Width, fsAI.GetPxAtForce(0));

		// 3) paint points as line (can be done also with DrawPoints to debug)
		if(debug)
			force_sensor_ai_pixmap.DrawPoints(pen_black_force_ai, paintPoints);
		else
			force_sensor_ai_pixmap.DrawLines(pen_black_force_ai, paintPoints);

		// 4) create hscaleLower and higher values (A, B at the moment)
		int hscaleLower = Convert.ToInt32(hscale_force_sensor_ai_a.Value);
		int hscaleHigher = Convert.ToInt32(hscale_force_sensor_ai_b.Value);

		// 5) paint vertical yellow lines A, B and write letter
		int xposA = fsAI.GetXFromSampleCount(hscaleLower);
		force_sensor_ai_pixmap.DrawLine(pen_yellow_force_ai,
				xposA, 0, xposA, allocation.Height -20);

		layout_force_ai_text_big.SetMarkup("A");
		int textWidth = 1;
		int textHeight = 1;
		layout_force_ai_text_big.GetPixelSize(out textWidth, out textHeight);

		//draw white rectangle on the end to ensure A is shown
		Rectangle rect = new Rectangle(xposA - (textWidth -1), allocation.Height - textHeight, 2 * (textWidth -1), textHeight);
		force_sensor_ai_pixmap.DrawRectangle(pen_white_force_ai, true, rect);

		force_sensor_ai_pixmap.DrawLayout (pen_yellow_force_ai,
				xposA - textWidth/2, allocation.Height - textHeight,
				layout_force_ai_text_big);

		int xposB = 0;
		if(checkbutton_force_sensor_ai_b.Active && hscaleLower != hscaleHigher)
		{
			xposB = fsAI.GetXFromSampleCount(hscaleHigher);
			force_sensor_ai_pixmap.DrawLine(pen_yellow_force_ai,
					xposB, 0, xposB, allocation.Height -20);

			layout_force_ai_text_big.SetMarkup("B");
			textWidth = 1;
			textHeight = 1;
			layout_force_ai_text_big.GetPixelSize(out textWidth, out textHeight);

			//draw white rectangle on the end to ensure A is shown
			rect = new Rectangle(xposB - (textWidth -1), allocation.Height - textHeight, 2 * (textWidth -1), textHeight);
			force_sensor_ai_pixmap.DrawRectangle(pen_white_force_ai, true, rect);

			force_sensor_ai_pixmap.DrawLayout (pen_yellow_force_ai,
					xposB - textWidth/2, allocation.Height - textHeight,
					layout_force_ai_text_big);
		}

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

		// paint repetitions info (vertical line and number)
		int xposRepPrevious = 0;
		for(int i = 0; i < fsAI.ForceSensorRepetition_l.Count; i ++)
		{
			// paint vertical line for each rep
			int xposRep = fsAI.GetXFromSampleCount(fsAI.ForceSensorRepetition_l[i].posX);
			force_sensor_ai_pixmap.DrawLine(pen_green_force_ai,
					xposRep, 0, xposRep, allocation.Height -20);

			// write repetition count
			if(i > 0)
			{
				layout_force_ai_text.SetMarkup(i.ToString());
				textWidth = 1; textHeight = 1;
				layout_force_ai_text.GetPixelSize(out textWidth, out textHeight);
				force_sensor_ai_pixmap.DrawLayout (pen_green_force_ai,
						Convert.ToInt32((xposRepPrevious + xposRep)/2 - textWidth/2), 0,
						layout_force_ai_text);
			}

			xposRepPrevious = xposRep;
		}


		// 6) if only A calculate exit
		if(! checkbutton_force_sensor_ai_b.Active)
			return;

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
			//8) calculate and paint RFD
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
		LogB.Information("forceSensorAnalyzeManualGraphDo() END");
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
	bool updateForceSensorHScales = true;
	private void on_hscale_force_sensor_ai_a_value_changed (object o, EventArgs args)
	{
		if(fsAI == null || fsAI.GetLength() == 0)
			return;

		//do not allow A to be higher than B (fix multiple possible problems)
		if(checkbutton_force_sensor_ai_b.Active && hscale_force_sensor_ai_a.Value > hscale_force_sensor_ai_b.Value)
			hscale_force_sensor_ai_b.Value = hscale_force_sensor_ai_a.Value;

		int count = Convert.ToInt32(hscale_force_sensor_ai_a.Value);
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

		if(checkbutton_force_sensor_ai_b.Active)
		{
			force_sensor_analyze_instant_calculate_params();
			updateForceSensorHScales = false;
			hscale_force_sensor_ai_ab.Value = Convert.ToInt32(hscale_force_sensor_ai_a.Value + hscale_force_sensor_ai_b.Value) / 2;
			updateForceSensorHScales = true;
		}

		forceSensorAnalyzeGeneralButtonHscaleZoomSensitiveness();
		forceSensorAIChanged = true;
		force_sensor_ai_drawingarea.QueueDraw(); //will fire ExposeEvent
	}
	private void on_hscale_force_sensor_ai_b_value_changed (object o, EventArgs args)
	{
		if(fsAI == null || fsAI.GetLength() == 0)
			return;

		//do not allow B to be lower than A (fix multiple possible problems)
		if(hscale_force_sensor_ai_b.Value < hscale_force_sensor_ai_a.Value)
			hscale_force_sensor_ai_a.Value = hscale_force_sensor_ai_b.Value;

		int count = Convert.ToInt32(hscale_force_sensor_ai_b.Value);
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

		force_sensor_analyze_instant_calculate_params();
		updateForceSensorHScales = false;
		hscale_force_sensor_ai_ab.Value = Convert.ToInt32(hscale_force_sensor_ai_a.Value + hscale_force_sensor_ai_b.Value) / 2;
		updateForceSensorHScales = true;

		forceSensorAnalyzeGeneralButtonHscaleZoomSensitiveness();
		forceSensorAIChanged = true;
		force_sensor_ai_drawingarea.QueueDraw(); //will fire ExposeEvent
	}
	int force_sensor_last_ab = 1;
	private void on_hscale_force_sensor_ai_ab_value_changed (object o, EventArgs args)
	{
		if(fsAI == null || fsAI.GetLength() == 0)
			return;

		//avoid circular calls
		if(updateForceSensorHScales)
		{
			int difference = Convert.ToInt32(hscale_force_sensor_ai_ab.Value) - force_sensor_last_ab;
			int aValue = Convert.ToInt32(hscale_force_sensor_ai_a.Value);
			int bValue = Convert.ToInt32(hscale_force_sensor_ai_b.Value);
			//if a or b are at max, don't move the ab to the right
			//if a or b are at min, don't move the ab to the left
			if(
					( difference > 0 && aValue < fsAI.GetLength() -2 && bValue < fsAI.GetLength() -2 ) ||
					( difference < 0 && aValue > 1 && bValue > 1 ) )
			{
				//move a and b
				hscale_force_sensor_ai_a.Value += difference;
				hscale_force_sensor_ai_b.Value += difference;
			} else {
				//do not move ab (so also a and b will not be moved)
				updateForceSensorHScales = false;
				hscale_force_sensor_ai_ab.Value = force_sensor_last_ab;
				updateForceSensorHScales = true;
			}
		}

		force_sensor_last_ab = Convert.ToInt32(hscale_force_sensor_ai_ab.Value);
	}

	private void forceSensorAnalyzeGeneralButtonHscaleZoomSensitiveness()
	{
		//note ai_a can be working with ai_b or alone (depending on checkbutton_force_sensor_ai_b)

		button_hscale_force_sensor_ai_a_first.Sensitive = hscale_force_sensor_ai_a.Value > 1;
		button_hscale_force_sensor_ai_a_pre.Sensitive = hscale_force_sensor_ai_a.Value > 1;
		button_hscale_force_sensor_ai_b_first.Sensitive = hscale_force_sensor_ai_b.Value > 1;
		button_hscale_force_sensor_ai_b_pre.Sensitive = hscale_force_sensor_ai_b.Value > 1;

		button_hscale_force_sensor_ai_a_last.Sensitive = hscale_force_sensor_ai_a.Value < fsAI.GetLength() -2;
		button_hscale_force_sensor_ai_a_post.Sensitive = hscale_force_sensor_ai_a.Value < fsAI.GetLength() -2;
		button_hscale_force_sensor_ai_b_last.Sensitive = hscale_force_sensor_ai_b.Value < fsAI.GetLength() -2;
		button_hscale_force_sensor_ai_b_post.Sensitive = hscale_force_sensor_ai_b.Value < fsAI.GetLength() -2;

		button_force_sensor_ai_zoom.Sensitive = forceSensorZoomApplied || hscale_force_sensor_ai_a.Value != hscale_force_sensor_ai_b.Value;
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
		hscale_force_sensor_ai_b.Value = fsAI.GetLength() -2;
	}

	private void on_checkbutton_force_sensor_ai_b_toggled (object o, EventArgs args)
	{
		int count = Convert.ToInt32(hscale_force_sensor_ai_b.Value);
		label_force_sensor_ai_time_b.Text = Math.Round(fsAI.GetTimeMS(count), 1).ToString();
		label_force_sensor_ai_force_b.Text = Math.Round(fsAI.GetForceAtCount(count), 1).ToString();
		if(fsAI.CalculedElasticPSAP)
			label_force_sensor_ai_position_b.Text = Math.Round(fsAI.Position_l[count], 3).ToString();

		manage_force_sensor_ai_table_visibilities();

		if(fsAI != null)
			force_sensor_analyze_instant_calculate_params();

		forceSensorAIChanged = true; //to actually plot

		//this two help to make the table shrink when B is unchecked
		scrolledwindow_force_sensor_ai.Hide();
		scrolledwindow_force_sensor_ai.Show();

		force_sensor_ai_drawingarea.QueueDraw(); // -- refresh
	}

	private void manage_force_sensor_ai_table_visibilities()
	{
		bool visible = checkbutton_force_sensor_ai_b.Active;
		hscale_force_sensor_ai_b.Visible = visible;
		hbox_buttons_scale_force_sensor_ai_b.Visible = visible;
		hscale_force_sensor_ai_ab.Visible = visible;

		label_force_sensor_ai_b.Visible = visible;
		label_force_sensor_ai_diff.Visible = visible;
		label_force_sensor_ai_average.Visible = visible;
		label_force_sensor_ai_max.Visible = visible;
		label_force_sensor_ai_time_b.Visible = visible;
		label_force_sensor_ai_time_diff.Visible = visible;

		label_force_sensor_ai_force_b.Visible = visible;
		label_force_sensor_ai_force_diff.Visible = visible;
		label_force_sensor_ai_force_average.Visible = visible;
		label_force_sensor_ai_force_max.Visible = visible;

		label_force_sensor_ai_rfd_b.Visible = visible;
		label_force_sensor_ai_rfd_diff.Visible = visible;
		label_force_sensor_ai_rfd_average.Visible = visible;
		label_force_sensor_ai_rfd_max.Visible = visible;

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

		button_force_sensor_ai_zoom.Visible = visible;

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
		bool success = fsAI.CalculateRangeParams(countA, countB);
		if(success) {
			label_force_sensor_ai_time_diff.Text = Math.Round(timeB - timeA, 1).ToString();
			label_force_sensor_ai_force_diff.Text = Math.Round(forceB - forceA, 1).ToString();

			if(countA != countB) {
				label_force_sensor_ai_force_average.Text = Math.Round(fsAI.ForceAVG, 1).ToString();
				label_force_sensor_ai_force_max.Text = Math.Round(fsAI.ForceMAX, 1).ToString();
			} else {
				label_force_sensor_ai_force_average.Text = "";
				label_force_sensor_ai_force_max.Text = "";
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

		vbox_force_sensor_ai_impulse_variability_and_feedback.Visible = (countA != countB);

		if(countA != countB)
		{
			// 10) calculate impulse
			label_force_sensor_ai_impulse_values.Text = Math.Round(fsAI.CalculateImpulse(
						countA, countB), 1).ToString();

			// 11) calculate variability
			double variability = 0;
			double feedbackDiff = 0;
			int feedbackF = preferences.forceSensorCaptureFeedbackAt;

			fsAI.CalculateVariabilityAndAccuracy(countA, countB, feedbackF, out variability, out feedbackDiff);

			label_force_sensor_ai_variability_values.Text = Math.Round(variability, 3).ToString();

			// 12) calculate Accuracy (Feedback difference)
			if(preferences.forceSensorCaptureFeedbackActive && feedbackF > 0)
			{
				label_force_sensor_ai_feedback_values.Text = Math.Round(feedbackDiff, 3).ToString();
				vbox_force_sensor_ai_feedback.Visible = true;
			} else
				vbox_force_sensor_ai_feedback.Visible = false;
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
			new DialogMessage(Constants.MessageTypes.WARNING, "A nd B cannot be the same");
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
