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
 * Copyright (C) 2020   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO;
using Gtk;
using Glade;
using System.Collections.Generic; //List<T>
using Mono.Unix;


public partial class ChronoJumpWindow 
{
	[Widget] Gtk.Notebook notebook_run_encoder_analyze_or_options;
	[Widget] Gtk.CheckButton check_run_encoder_analyze_accel;
	[Widget] Gtk.CheckButton check_run_encoder_analyze_force;
	[Widget] Gtk.CheckButton check_run_encoder_analyze_power;
	[Widget] Gtk.ComboBox combo_run_encoder_analyze_accel;
	[Widget] Gtk.ComboBox combo_run_encoder_analyze_force;
	[Widget] Gtk.ComboBox combo_run_encoder_analyze_power;
	[Widget] Gtk.Button button_run_encoder_analyze_options;
	[Widget] Gtk.Button button_run_encoder_analyze_analyze;
	[Widget] Gtk.Button button_run_encoder_image_save;


	private void createRunEncoderAnalyzeCombos ()
	{
		/*
		 * usually we have an hbox on glade, we create a combo here and we attach
		 * this technique is the same than createForceAnalyzeCombos ()
		 * the combo is in glade, without elements, but the elements is in bold because it has been edited, but is blank
		 * you can see in the app1.glade:
		 * <property name="items"/>
		 */

		//do not put the "NO" in the combo, the NO is the checkbox
		//note accel, force, power have the same options
		List<string> optionsWithoutNo = new List<string>();
		for(int i = 1; i < Preferences.runEncoderAnalyzeAccel.L_trans.Count; i ++)
			optionsWithoutNo.Add(Preferences.runEncoderAnalyzeAccel.L_trans[i]);

		UtilGtk.ComboUpdate(combo_run_encoder_analyze_accel, optionsWithoutNo);
		UtilGtk.ComboUpdate(combo_run_encoder_analyze_force, optionsWithoutNo);
		UtilGtk.ComboUpdate(combo_run_encoder_analyze_power, optionsWithoutNo);
		combo_run_encoder_analyze_accel.Active = Preferences.runEncoderAnalyzeAccel.SqlCurrent;
		combo_run_encoder_analyze_force.Active = Preferences.runEncoderAnalyzeForce.SqlCurrent;
		combo_run_encoder_analyze_power.Active = Preferences.runEncoderAnalyzePower.SqlCurrent;
	}

	private void setRunEncoderAnalyzeWidgets()
	{
		setRunEncoderAnalyzeWidgetsDo (Preferences.runEncoderAnalyzeAccel,
				check_run_encoder_analyze_accel, combo_run_encoder_analyze_accel);
		setRunEncoderAnalyzeWidgetsDo (Preferences.runEncoderAnalyzeForce,
				check_run_encoder_analyze_force, combo_run_encoder_analyze_force);
		setRunEncoderAnalyzeWidgetsDo (Preferences.runEncoderAnalyzePower,
				check_run_encoder_analyze_power, combo_run_encoder_analyze_power);
	}
	private void setRunEncoderAnalyzeWidgetsDo (LSqlEnTrans lsql, Gtk.CheckButton check, Gtk.ComboBox combo)
	{
		if(lsql.SqlCurrentName == Preferences.runEncoderAnalyzeAFPSqlNO)
		{
			check.Active = false;
			combo.Visible = false;
		} else {
			check.Active = true;
			combo.Visible = true;
		}
		combo.Active = UtilGtk.ComboMakeActive(combo, lsql.TranslatedCurrent);
	}

	private void on_check_run_encoder_analyze_accel_clicked (object o, EventArgs args)
	{
		combo_run_encoder_analyze_accel.Visible = (check_run_encoder_analyze_accel.Active);
	}
	private void on_check_run_encoder_analyze_force_clicked (object o, EventArgs args)
	{
		combo_run_encoder_analyze_force.Visible = (check_run_encoder_analyze_force.Active);
	}
	private void on_check_run_encoder_analyze_power_clicked (object o, EventArgs args)
	{
		combo_run_encoder_analyze_power.Visible = (check_run_encoder_analyze_power.Active);
	}

	private void on_button_run_encoder_analyze_options_clicked (object o, EventArgs args)
	{
		notebook_run_encoder_analyze_or_options.CurrentPage = 1;
		runEncoderButtonsSensitive(false);
	}
	private void on_button_run_encoder_analyze_options_close_clicked (object o, EventArgs args)
	{
		// 1 change stuff on Sqlite if needed

		Sqlite.Open();

		//accel
		if( ! check_run_encoder_analyze_accel.Active )
			Preferences.runEncoderAnalyzeAccel.SetCurrentFromSQL(Preferences.runEncoderAnalyzeAFPSqlNO);
		else
			Preferences.runEncoderAnalyzeAccel.SetCurrentFromComboTranslated(
				UtilGtk.ComboGetActive(combo_run_encoder_analyze_accel));

		SqlitePreferences.Update(Preferences.runEncoderAnalyzeAccel.Name, Preferences.runEncoderAnalyzeAccel.SqlCurrentName, true);

		//force
		if( ! check_run_encoder_analyze_force.Active )
			Preferences.runEncoderAnalyzeForce.SetCurrentFromSQL(Preferences.runEncoderAnalyzeAFPSqlNO);
		else
			Preferences.runEncoderAnalyzeForce.SetCurrentFromComboTranslated(
				UtilGtk.ComboGetActive(combo_run_encoder_analyze_force));

		SqlitePreferences.Update(Preferences.runEncoderAnalyzeForce.Name, Preferences.runEncoderAnalyzeForce.SqlCurrentName, true);

		//power
		if( ! check_run_encoder_analyze_power.Active )
			Preferences.runEncoderAnalyzePower.SetCurrentFromSQL(Preferences.runEncoderAnalyzeAFPSqlNO);
		else
			Preferences.runEncoderAnalyzePower.SetCurrentFromComboTranslated(
				UtilGtk.ComboGetActive(combo_run_encoder_analyze_power));

		SqlitePreferences.Update(Preferences.runEncoderAnalyzePower.Name, Preferences.runEncoderAnalyzePower.SqlCurrentName, true);

		Sqlite.Close();

		// 2 change sensitivity of widgets

		notebook_run_encoder_analyze_or_options.CurrentPage = 0;
		runEncoderButtonsSensitive(true);
	}

	private void on_button_run_encoder_analyze_options_close_and_analyze_clicked (object o, EventArgs args)
	{
		on_button_run_encoder_analyze_options_close_clicked (o, args);
		on_button_run_encoder_analyze_analyze_clicked (o, args);
	}

	private void on_button_run_encoder_analyze_analyze_clicked (object o, EventArgs args)
	{
		if(! Util.FileExists(lastRunEncoderFullPath))
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileNotFoundStr());
			return;
		}

		if(lastRunEncoderFullPath != null && lastRunEncoderFullPath != "")
			raceEncoderCopyTempAndDoGraphs();
	}

	private void on_button_run_encoder_image_save_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.RUNENCODER_SAVE_IMAGE);
	}

	private void on_button_run_encoder_image_save_selected (string destination)
	{
		try {
			File.Copy(UtilEncoder.GetSprintEncoderImage(), destination, true);
		} catch {
			string myString = string.Format(
					Catalog.GetString("Cannot save file {0} "), destination);
			new DialogMessage(Constants.MessageTypes.WARNING, myString);
		}
	}

	private void on_overwrite_file_runencoder_image_save_accepted(object o, EventArgs args)
	{
		on_button_run_encoder_image_save_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}

}
