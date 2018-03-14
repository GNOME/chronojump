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
	[Widget] Gtk.Button button_force_sensor_analyze_load;
	[Widget] Gtk.Label label_force_sensor_analyze;
	[Widget] Gtk.Image image_force_sensor_graph;
	[Widget] Gtk.Viewport viewport_force_sensor_graph;
	[Widget] Gtk.Button button_force_sensor_image_save_rfd;

	[Widget] Gtk.SpinButton spin_force_duration_seconds;
	[Widget] Gtk.RadioButton radio_force_duration_seconds;

	//analyze options
	[Widget] Gtk.Notebook notebook_force_analyze;
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


	/*
	 * analyze options -------------------------->
	 */

	private void forceSensorAnalyzeOptionsSensitivity(bool s) //s for sensitive
	{
		button_force_sensor_analyze_options.Sensitive = s;
		button_force_sensor_analyze_load.Sensitive = s;

		main_menu.Sensitive = s;
		notebook_session_person.Sensitive = s;
		hbox_contacts_sup_capture_analyze_two_buttons.Sensitive = s;
		hbox_top_person.Sensitive = s;
		hbox_top_person_encoder.Sensitive = s;
	}

	private void on_button_force_sensor_analyze_options_clicked (object o, EventArgs args)
	{
		notebook_force_analyze.CurrentPage = 1;
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
				SqliteForceSensor.Update(true, rfd);
				rfdList[i] = rfd;
			}
			i ++;
		}

		ForceSensorImpulse newImpulse = getImpulseValue();
		if(newImpulse.Changed(impulse))
		{
			SqliteForceSensor.UpdateImpulse(true, newImpulse);
			impulse = newImpulse;
		}
		Sqlite.Close();

		// 2 change sensitivity of widgets

		notebook_force_analyze.CurrentPage = 0;
		forceSensorAnalyzeOptionsSensitivity(true);
	}

	private void initForceSensor ()
	{
		createForceCombos();
		setRFDValues();
		setImpulseValue();
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

	private void createForceCombos ()
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

		SqliteForceSensor.DeleteAll(true);
		SqliteForceSensor.InsertDefaultValues(true);

		rfdList = SqliteForceSensor.SelectAll(false);
		impulse = SqliteForceSensor.SelectImpulse(false);

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
}
