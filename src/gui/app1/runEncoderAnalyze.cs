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
	[Widget] Gtk.Notebook notebook_run_encoder_analyze;
	[Widget] Gtk.CheckButton check_run_encoder_analyze_accel;
	[Widget] Gtk.CheckButton check_run_encoder_analyze_force;
	[Widget] Gtk.CheckButton check_run_encoder_analyze_power;
	[Widget] Gtk.ComboBox combo_run_encoder_analyze_accel;
	[Widget] Gtk.ComboBox combo_run_encoder_analyze_force;
	[Widget] Gtk.ComboBox combo_run_encoder_analyze_power;
	[Widget] Gtk.Button button_run_encoder_analyze_options_close_and_analyze;
	[Widget] Gtk.Button button_run_encoder_analyze_analyze;
	[Widget] Gtk.Button button_run_encoder_image_save;

	[Widget] Gtk.RadioButton radio_run_encoder_analyze_individual_current_set;
	[Widget] Gtk.RadioButton radio_run_encoder_analyze_individual_current_session;
	[Widget] Gtk.RadioButton radio_run_encoder_analyze_groupal_current_session;
	[Widget] Gtk.Image image_run_encoder_analyze_individual_current_set;
	[Widget] Gtk.Image image_run_encoder_analyze_individual_current_session;
	[Widget] Gtk.Image image_run_encoder_analyze_groupal_current_session;
	[Widget] Gtk.HBox hbox_run_encoder_analyze_top_modes;

	//export
	[Widget] Gtk.Label label_run_encoder_export_data;
	[Widget] Gtk.CheckButton check_run_encoder_export_images;
	[Widget] Gtk.ProgressBar progressbar_run_encoder_export;
	[Widget] Gtk.Label label_run_encoder_export_result;

	private enum notebook_run_encoder_analyze_pages { CURRENTSET, CURRENTSESSION, OPTIONS }

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
		notebook_run_encoder_analyze.CurrentPage = Convert.ToInt32(notebook_run_encoder_analyze_pages.OPTIONS);
		hbox_run_encoder_analyze_top_modes.Sensitive = false;
		runEncoderButtonsSensitive(false);
	}
	private void on_button_run_encoder_analyze_options_close_clicked (object o, EventArgs args)
	{
		hbox_run_encoder_analyze_top_modes.Sensitive = true;

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

		notebook_run_encoder_analyze.CurrentPage = Convert.ToInt32(notebook_run_encoder_analyze_pages.CURRENTSET);
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

	private void on_button_raceAnalyzer_save_table_file_selected (string destination)
	{
		try {
			//this overwrites if needed
			TextWriter writer = File.CreateText(destination);

			string sep = " ";
			if (preferences.CSVExportDecimalSeparator == "COMMA")
				sep = ";";
			else
				sep = ",";

			//write header
			writer.WriteLine(Util.RemoveNewLine(Util.StringArrayToString(
							getTreeviewRaceAnalyzerHeaders(), sep), false));

			string contents = Util.ReadFile(RunEncoder.GetCSVResultsFileName(), false);
			RunEncoderCSV recsv = readRunEncoderCSVContents(contents);

			writer.WriteLine(recsv.ToCSV(preferences.CSVExportDecimalSeparator));
/*

			//write curves rows
			ArrayList array = getTreeViewCurves(encoderAnalyzeListStore);

			foreach (EncoderCurve ec in array)
			{
				writer.WriteLine(ec.ToCSV(false, preferences.CSVExportDecimalSeparator, preferences.encoderWorkKcal, phase));
			}
			*/

			writer.Flush();
			writer.Close();
			((IDisposable)writer).Dispose();
		} catch {
			string myString = string.Format(
					Catalog.GetString("Cannot save file {0} "), destination);
			new DialogMessage(Constants.MessageTypes.WARNING, myString);
		}
	}

	private void on_overwrite_file_raceAnalyzer_save_table_accepted(object o, EventArgs args)
	{
		on_button_raceAnalyzer_save_table_file_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}


	//move to export gui file

	private void on_radio_run_encoder_analyze_individual_current_set_toggled (object o, EventArgs args)
	{
		button_run_encoder_analyze_load.Visible = true;
		button_run_encoder_analyze_analyze.Visible = true;

		notebook_run_encoder_analyze.CurrentPage = Convert.ToInt32(notebook_run_encoder_analyze_pages.CURRENTSET);
		label_run_encoder_export_result.Text = "";
	}
	private void on_radio_run_encoder_analyze_individual_current_session_toggled (object o, EventArgs args)
	{
		button_run_encoder_analyze_load.Visible = false;
		button_run_encoder_analyze_analyze.Visible = false;

		if(currentPerson != null)
			label_run_encoder_export_data.Text = currentPerson.Name;
		else
			label_run_encoder_export_data.Text = "";

		notebook_run_encoder_analyze.CurrentPage = Convert.ToInt32(notebook_run_encoder_analyze_pages.CURRENTSESSION);
		label_run_encoder_export_result.Text = "";
	}
	private void on_radio_run_encoder_analyze_groupal_current_session_toggled (object o, EventArgs args)
	{
		button_run_encoder_analyze_load.Visible = false;
		button_run_encoder_analyze_analyze.Visible = false;

		label_run_encoder_export_data.Text = currentSession.Name;

		notebook_run_encoder_analyze.CurrentPage = Convert.ToInt32(notebook_run_encoder_analyze_pages.CURRENTSESSION);
		label_run_encoder_export_result.Text = "";
	}

	private void on_button_run_encoder_export_current_session_clicked (object o, EventArgs args)
	{
		if(currentSession == null)
			return;

		if (radio_run_encoder_analyze_individual_current_session.Active)
		{
			if(currentPerson == null)
				return;

			button_run_encoder_export_session (currentPerson.UniqueID);
		}
		else if (radio_run_encoder_analyze_groupal_current_session.Active)
		{
			button_run_encoder_export_session (-1);
		}
	}

	RunEncoderExport runEncoderExport;
	private void button_run_encoder_export_session (int personID)
	{
		runEncoderExport = new RunEncoderExport (
				true, //includeImages 	//TODO
				UtilAll.IsWindows(),
				-1, 			//all persons
				currentSession.UniqueID,
				preferences.runEncoderMinAccel,
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.RAWACCEL),
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.FITTEDACCEL),
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.RAWFORCE),
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.FITTEDFORCE),
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.RAWPOWER),
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.FITTEDPOWER),
				preferences.CSVExportDecimalSeparatorChar      //decimalIsPointAtExport (write)
				);

		//runEncoderExport.Start(selectedFileName); //file or folder
		runEncoderExport.Start("runEncoderExport.csv"); //file or folder

		//TODO: continue with cancel stuff, ...
	}
}
