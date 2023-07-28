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
 * Copyright (C) 2020-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO;
using Gtk;
//using Glade;
using System.Collections.Generic; //List<T>
using Mono.Unix;


public partial class ChronoJumpWindow 
{
	// at glade ---->
	Gtk.CheckButton check_run_encoder_analyze_accel;
	Gtk.CheckButton check_run_encoder_analyze_force;
	Gtk.CheckButton check_run_encoder_analyze_power;
	Gtk.Button button_run_encoder_analyze_options_close_and_analyze;
	Gtk.Button button_run_encoder_analyze_analyze;
	Gtk.Button button_run_encoder_image_save;

	Gtk.RadioButton radio_run_encoder_analyze_individual_current_set;
	Gtk.RadioButton radio_run_encoder_analyze_individual_current_session;
	Gtk.RadioButton radio_run_encoder_analyze_individual_all_sessions;
	Gtk.RadioButton radio_run_encoder_analyze_groupal_current_session;
	Gtk.Image image_run_encoder_analyze_individual_current_set;
	Gtk.Image image_run_encoder_analyze_individual_current_session;
	Gtk.Image image_run_encoder_analyze_individual_all_sessions;
	Gtk.Image image_run_encoder_analyze_groupal_current_session;

	Gtk.HBox hbox_run_encoder_top;
	Gtk.HBox hbox_run_encoder_analyze_top_modes;

	//export
	Gtk.Notebook notebook_run_encoder_export;
	Gtk.Label label_run_encoder_export_data;
	Gtk.CheckButton check_run_encoder_export_images;
	Gtk.HBox hbox_run_encoder_export_width_height;
	Gtk.SpinButton spinbutton_run_encoder_export_image_width;
	Gtk.SpinButton spinbutton_run_encoder_export_image_height;
	Gtk.CheckButton check_run_encoder_export_instantaneous;
	Gtk.ProgressBar progressbar_run_encoder_export;
	Gtk.Label label_run_encoder_export_discarded;
	Gtk.Label label_run_encoder_export_result;
	Gtk.Button button_run_encoder_export_result_open;

	Gtk.Notebook notebook_run_encoder_analyze;
	Gtk.Notebook notebook_run_encoder_analyze_current_set;

	Gtk.ComboBoxText combo_run_encoder_analyze_accel;
	Gtk.ComboBoxText combo_run_encoder_analyze_force;
	Gtk.ComboBoxText combo_run_encoder_analyze_power;
	// <---- at glade


	private enum notebook_run_encoder_analyze_pages { CURRENTSET, CURRENTSESSION, OPTIONS }
	private enum notebook_run_encoder_analyze_current_set_pages { GRAPH, TABLE, TRIGGERS }

	TreeviewRAAnalyze tvRA_AB;
	TreeviewRAAnalyze tvRA_CD;

	private void runEncoderPrepareGraphAI ()
	{
		// 0. condition return if null
		if (cairoGraphRaceAnalyzerPoints_st_l == null || cairoGraphRaceAnalyzerPoints_st_l.Count == 0)
			return;

		// 1. get zoom values
		//int zoomFrameA = -1; //means no zoom
		int zoomFrameB = -1; //means no zoom

		//Gtk.HScale hsLeft = getHScaleABCD (true);
		Gtk.HScale hsRight = getHScaleABCD (false);

		//TODO: continue

		// 2. create raAI_AB, raAI_CD
		raAI_AB = new RaceAnalyzerAnalyzeInstant ("AB",
			cairoGraphRaceAnalyzerPoints_st_l);
		raAI_CD = new RaceAnalyzerAnalyzeInstant ("CD",
			cairoGraphRaceAnalyzerPoints_st_l);

		// 3. set hscales
		signalPrepareGraphAICont (raAI_AB.GetLength(), raAI_CD.GetLength(), zoomFrameB, hsRight);

		// 4. manage save buttons visibilities (TODO)
		//manage_force_sensor_ai_table_visibilities();
	}

	private void race_analyzer_analyze_instant_calculate_params_for_treeview (
			RaceAnalyzerAnalyzeInstant raAI, TreeviewRAAnalyze tvRA, bool isAB, int countA, int countB)
	{
		//LogB.Information (string.Format ("before CalculateRangeParams 0 with raAI.IdStr: {0}", raAI.IdStr));
		if (countA < 0 || countA > raAI.GetLength() -1 || countB < 0 || countB > raAI.GetLength() -1)
			return;

		//LogB.Information (string.Format ("before CalculateRangeParams 1 with fsAI.IdStr: {0}", fsAI.IdStr));
		//old method
		double timeA = raAI.GetTimeMS (countA);
		double timeB = raAI.GetTimeMS (countB);
		double speedA = raAI.GetSpeedAtCount (countA);
		double speedB = raAI.GetSpeedAtCount (countB);

		tvRA.TimeDiff = Math.Round (timeB - timeA, 1).ToString();
		tvRA.SpeedDiff = speedB - speedA;

		//TODO: avg, max
	}

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
	private void setRunEncoderAnalyzeWidgetsDo (LSqlEnTrans lsql, Gtk.CheckButton check, Gtk.ComboBoxText combo)
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

	private bool button_run_encoder_analyze_analyze_was_sensitive; //needed this temp variable
	private int notebook_run_encoder_analyze_coming_page; //needed this temp variable
	private void on_button_run_encoder_analyze_options_clicked (object o, EventArgs args)
	{
		//only show close_and_analyze button on current set tab
		button_run_encoder_analyze_options_close_and_analyze.Visible =
			(notebook_run_encoder_analyze.CurrentPage == Convert.ToInt32(notebook_run_encoder_analyze_pages.CURRENTSET));

		notebook_run_encoder_analyze_coming_page = Convert.ToInt32(notebook_run_encoder_analyze.CurrentPage);
		notebook_run_encoder_analyze.CurrentPage = Convert.ToInt32(notebook_run_encoder_analyze_pages.OPTIONS);

		hbox_run_encoder_analyze_top_modes.Sensitive = false;
		runEncoderButtonsSensitive(false);

		button_run_encoder_analyze_analyze_was_sensitive = button_run_encoder_analyze_analyze.Sensitive;
		button_run_encoder_analyze_analyze.Sensitive = false;
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

		notebook_run_encoder_analyze.CurrentPage = notebook_run_encoder_analyze_coming_page;
		runEncoderButtonsSensitive(true);
		button_run_encoder_analyze_analyze.Sensitive = button_run_encoder_analyze_analyze_was_sensitive;
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

		if(operatingSystem == UtilAll.OperatingSystems.MACOSX &&
				! Util.FileExists(Constants.ROSX) )
		{
			showMacRInstallMessage ();
			return;
		}
		else if (operatingSystem == UtilAll.OperatingSystems.LINUX &&
				! ExecuteProcess.InstalledOnLinux ("R"))
		{
			showLinuxRInstallMessage ();
			return;
		}

		if(lastRunEncoderFullPath != null && lastRunEncoderFullPath != "")
			raceEncoderCopyToTempAndDoRGraph();

		//move from triggers tab (if we are there) to graph tab
		if(notebook_run_encoder_analyze_current_set.CurrentPage ==
				Convert.ToInt32(notebook_run_encoder_analyze_current_set_pages.TRIGGERS))
			notebook_run_encoder_analyze_current_set.CurrentPage =
				Convert.ToInt32(notebook_run_encoder_analyze_current_set_pages.GRAPH);
	}

	private void on_button_raceAnalyzer_table_save_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.RUNENCODER_ANALYZE_SAVE_TABLE);
	}

	private void on_button_run_encoder_image_save_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.RUNENCODER_ANALYZE_SAVE_IMAGE);
	}

	private void on_button_run_encoder_analyze_image_save_selected (string destination)
	{
		try {
			File.Copy(UtilEncoder.GetSprintEncoderImage(), destination, true);
		} catch {
			string myString = string.Format(
					Catalog.GetString("Cannot save file {0} "), destination);
			new DialogMessage(Constants.MessageTypes.WARNING, myString);
		}
	}

	private void on_overwrite_file_runencoder_analyze_image_save_accepted(object o, EventArgs args)
	{
		on_button_run_encoder_analyze_image_save_selected (exportFileName);

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

			string contents = Util.ReadFile(RunEncoder.GetCSVResultsURL(), false);

			//write header
			writer.WriteLine(Util.RemoveNewLine(Util.StringArrayToString(
							getTreeviewRaceAnalyzerHeaders(contents), sep), true));

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

	private void on_check_run_encoder_export_images_toggled (object o, EventArgs args)
	{
		hbox_run_encoder_export_width_height.Visible = check_run_encoder_export_images.Active;

		//also hide the label and the open button
		label_run_encoder_export_discarded.Text = "";
		label_run_encoder_export_result.Text = "";
		button_run_encoder_export_result_open.Visible = false;
	}

	private void on_radio_run_encoder_analyze_individual_current_set_toggled (object o, EventArgs args)
	{
		button_run_encoder_analyze_load.Visible = true;
		button_run_encoder_analyze_analyze.Visible = true;

		notebook_run_encoder_analyze.CurrentPage = Convert.ToInt32(notebook_run_encoder_analyze_pages.CURRENTSET);
		label_run_encoder_export_discarded.Text = "";
		label_run_encoder_export_result.Text = "";
		button_run_encoder_export_result_open.Visible = false;
	}
	private void on_radio_run_encoder_analyze_individual_session_current_or_all_toggled (object o, EventArgs args)
	{
		button_run_encoder_analyze_load.Visible = false;
		button_run_encoder_analyze_analyze.Visible = false;

		if(currentPerson != null)
			label_run_encoder_export_data.Text = currentPerson.Name;
		else
			label_run_encoder_export_data.Text = "";

		notebook_run_encoder_analyze.CurrentPage = Convert.ToInt32(notebook_run_encoder_analyze_pages.CURRENTSESSION);
		label_run_encoder_export_discarded.Text = "";
		label_run_encoder_export_result.Text = "";
		button_run_encoder_export_result_open.Visible = false;
	}
	private void on_radio_run_encoder_analyze_groupal_current_session_toggled (object o, EventArgs args)
	{
		button_run_encoder_analyze_load.Visible = false;
		button_run_encoder_analyze_analyze.Visible = false;

		label_run_encoder_export_data.Text = currentSession.Name;

		notebook_run_encoder_analyze.CurrentPage = Convert.ToInt32(notebook_run_encoder_analyze_pages.CURRENTSESSION);
		label_run_encoder_export_discarded.Text = "";
		label_run_encoder_export_result.Text = "";
		button_run_encoder_export_result_open.Visible = false;
	}

	private void on_button_run_encoder_export_not_set_clicked (object o, EventArgs args)
	{
		if(operatingSystem == UtilAll.OperatingSystems.MACOSX &&
				! Util.FileExists(Constants.ROSX) )
		{
			showMacRInstallMessage ();
			return;
		}

		// 1) check if all sessions
		if(radio_run_encoder_analyze_individual_all_sessions.Active)
		{
			if(currentPerson == null)
				return;

			button_run_encoder_export_session (currentPerson.UniqueID, -1);
			return;
		}

		// 2) current session (individual or groupal)
		if(currentSession == null)
			return;

		if (radio_run_encoder_analyze_individual_current_session.Active)
		{
			if(currentPerson == null)
				return;

			button_run_encoder_export_session (currentPerson.UniqueID, currentSession.UniqueID);
		}
		else if (radio_run_encoder_analyze_groupal_current_session.Active)
		{
			button_run_encoder_export_session (-1, currentSession.UniqueID);
		}
	}

	RunEncoderExport runEncoderExport;
	private void button_run_encoder_export_session (int personID, int sessionID)
	{
		label_run_encoder_export_discarded.Text = "";
		label_run_encoder_export_result.Text = "";
		button_run_encoder_export_result_open.Visible = false;
		runEncoderButtonsSensitive(false);
		hbox_run_encoder_top.Sensitive = false;

		//store new width/height if changed
		Sqlite.Open();
		preferences.exportGraphWidth = Preferences.PreferencesChange(
				true, SqlitePreferences.ExportGraphWidth,
				preferences.exportGraphWidth, Convert.ToInt32(spinbutton_run_encoder_export_image_width.Value));
		preferences.exportGraphHeight = Preferences.PreferencesChange(
				true, SqlitePreferences.ExportGraphHeight,
				preferences.exportGraphHeight, Convert.ToInt32(spinbutton_run_encoder_export_image_height.Value));
		Sqlite.Close();

		//change also spinbuttons of export sprint and forceSensor
		spinbutton_sprint_export_image_width.Value = spinbutton_run_encoder_export_image_width.Value;
		spinbutton_sprint_export_image_height.Value = spinbutton_run_encoder_export_image_height.Value;

		spinbutton_force_sensor_export_image_width.Value = spinbutton_run_encoder_export_image_width.Value;
		spinbutton_force_sensor_export_image_height.Value = spinbutton_run_encoder_export_image_height.Value;


		runEncoderExport = new RunEncoderExport (
				notebook_run_encoder_export,
				progressbar_run_encoder_export,
				label_run_encoder_export_discarded,
				label_run_encoder_export_result,
				check_run_encoder_export_images.Active,
				Convert.ToInt32(spinbutton_run_encoder_export_image_width.Value),
				Convert.ToInt32(spinbutton_run_encoder_export_image_height.Value),
				check_run_encoder_export_instantaneous.Active,
				UtilAll.IsWindows(),
				personID, sessionID,
				preferences.runEncoderMinAccel,
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.RAWACCEL),
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.FITTEDACCEL),
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.RAWFORCE),
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.FITTEDFORCE),
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.RAWPOWER),
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.FITTEDPOWER),
				preferences.CSVExportDecimalSeparatorChar      //decimalIsPointAtExport (write)
				);

		runEncoderExport.Button_done.Clicked -= new EventHandler(run_encoder_export_done);
		runEncoderExport.Button_done.Clicked += new EventHandler(run_encoder_export_done);

		bool selectedFile = false;
		if(check_run_encoder_export_images.Active || check_run_encoder_export_instantaneous.Active) 	//export folder
		{
			if(personID == -1)
				selectedFile = checkFolder (Constants.CheckFileOp.RUNENCODER_EXPORT_GROUPAL_CURRENT_SESSION_YES_IMAGES);
			else if (sessionID == -1)
				selectedFile = checkFolder (Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_ALL_SESSIONS_YES_IMAGES);
			else
				selectedFile = checkFolder (Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_CURRENT_SESSION_YES_IMAGES);
		} else { 											//export file
			if(personID == -1)
				selectedFile = checkFile (Constants.CheckFileOp.RUNENCODER_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES);
			else if (sessionID == -1)
				selectedFile = checkFile (Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES);
			else
				selectedFile = checkFile (Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES);
		}

		//restore the gui if cancelled
		if(! selectedFile) {
			runEncoderButtonsSensitive(true);
			hbox_run_encoder_top.Sensitive = true;
		}
	}
	private void on_button_run_encoder_export_file_selected (string selectedFileName)
	{
		//runEncoderExport.Start("runEncoderExport.csv");
		runEncoderExport.Start(selectedFileName); //file or folder
	}

	private void on_button_run_encoder_export_cancel_clicked (object o, EventArgs args)
	{
		runEncoderExport.Cancel();
	}

	private void run_encoder_export_done (object o, EventArgs args)
	{
		runEncoderExport.Button_done.Clicked -= new EventHandler(run_encoder_export_done);

		runEncoderButtonsSensitive(true);
		hbox_run_encoder_top.Sensitive = true;

		if(runEncoderExport != null && runEncoderExport.AllOk)
			button_run_encoder_export_result_open.Visible = true;
	}

	private void on_button_run_encoder_export_result_open_clicked (object o, EventArgs args)
	{
		if(runEncoderExport == null || runEncoderExport.ExportURL == "")
		{
			new DialogMessage(Constants.MessageTypes.WARNING,
					Constants.DirectoryCannotOpenStr());
			return;
		}

		if(! Util.OpenURL (runEncoderExport.ExportURL))
			new DialogMessage(Constants.MessageTypes.WARNING,
					Constants.DirectoryCannotOpenStr() + "\n\n" + runEncoderExport.ExportURL);
	}

	private void connectWidgetsRunEncoderAnalyze (Gtk.Builder builder)
	{
		check_run_encoder_analyze_accel = (Gtk.CheckButton) builder.GetObject ("check_run_encoder_analyze_accel");
		check_run_encoder_analyze_force = (Gtk.CheckButton) builder.GetObject ("check_run_encoder_analyze_force");
		check_run_encoder_analyze_power = (Gtk.CheckButton) builder.GetObject ("check_run_encoder_analyze_power");
		button_run_encoder_analyze_options_close_and_analyze = (Gtk.Button) builder.GetObject ("button_run_encoder_analyze_options_close_and_analyze");
		button_run_encoder_analyze_analyze = (Gtk.Button) builder.GetObject ("button_run_encoder_analyze_analyze");
		button_run_encoder_image_save = (Gtk.Button) builder.GetObject ("button_run_encoder_image_save");

		radio_run_encoder_analyze_individual_current_set = (Gtk.RadioButton) builder.GetObject ("radio_run_encoder_analyze_individual_current_set");
		radio_run_encoder_analyze_individual_current_session = (Gtk.RadioButton) builder.GetObject ("radio_run_encoder_analyze_individual_current_session");
		radio_run_encoder_analyze_individual_all_sessions = (Gtk.RadioButton) builder.GetObject ("radio_run_encoder_analyze_individual_all_sessions");
		radio_run_encoder_analyze_groupal_current_session = (Gtk.RadioButton) builder.GetObject ("radio_run_encoder_analyze_groupal_current_session");
		image_run_encoder_analyze_individual_current_set = (Gtk.Image) builder.GetObject ("image_run_encoder_analyze_individual_current_set");
		image_run_encoder_analyze_individual_current_session = (Gtk.Image) builder.GetObject ("image_run_encoder_analyze_individual_current_session");
		image_run_encoder_analyze_individual_all_sessions = (Gtk.Image) builder.GetObject ("image_run_encoder_analyze_individual_all_sessions");
		image_run_encoder_analyze_groupal_current_session = (Gtk.Image) builder.GetObject ("image_run_encoder_analyze_groupal_current_session");

		hbox_run_encoder_top = (Gtk.HBox) builder.GetObject ("hbox_run_encoder_top");
		hbox_run_encoder_analyze_top_modes = (Gtk.HBox) builder.GetObject ("hbox_run_encoder_analyze_top_modes");

		//export
		notebook_run_encoder_export = (Gtk.Notebook) builder.GetObject ("notebook_run_encoder_export");
		label_run_encoder_export_data = (Gtk.Label) builder.GetObject ("label_run_encoder_export_data");
		check_run_encoder_export_images = (Gtk.CheckButton) builder.GetObject ("check_run_encoder_export_images");
		hbox_run_encoder_export_width_height = (Gtk.HBox) builder.GetObject ("hbox_run_encoder_export_width_height");
		spinbutton_run_encoder_export_image_width = (Gtk.SpinButton) builder.GetObject ("spinbutton_run_encoder_export_image_width");
		spinbutton_run_encoder_export_image_height = (Gtk.SpinButton) builder.GetObject ("spinbutton_run_encoder_export_image_height");
		check_run_encoder_export_instantaneous = (Gtk.CheckButton) builder.GetObject ("check_run_encoder_export_instantaneous");
		progressbar_run_encoder_export = (Gtk.ProgressBar) builder.GetObject ("progressbar_run_encoder_export");
		label_run_encoder_export_discarded = (Gtk.Label) builder.GetObject ("label_run_encoder_export_discarded");
		label_run_encoder_export_result = (Gtk.Label) builder.GetObject ("label_run_encoder_export_result");
		button_run_encoder_export_result_open = (Gtk.Button) builder.GetObject ("button_run_encoder_export_result_open");

		notebook_run_encoder_analyze = (Gtk.Notebook) builder.GetObject ("notebook_run_encoder_analyze");
		notebook_run_encoder_analyze_current_set = (Gtk.Notebook) builder.GetObject ("notebook_run_encoder_analyze_current_set");

		combo_run_encoder_analyze_accel = (Gtk.ComboBoxText) builder.GetObject ("combo_run_encoder_analyze_accel");
		combo_run_encoder_analyze_force = (Gtk.ComboBoxText) builder.GetObject ("combo_run_encoder_analyze_force");
		combo_run_encoder_analyze_power = (Gtk.ComboBoxText) builder.GetObject ("combo_run_encoder_analyze_power");
	}
}

public class TreeviewRAAnalyze : TreeviewS2Abstract
{
	//row 1
	protected string letterStart;
	protected double speedStart;

	//row 2
	protected string letterEnd;
	protected double speedEnd;

	//row 3
	protected double speedDiff;

	//row 4
	protected string speedAvg;

	//row 5
	protected string speedMax;

	public TreeviewRAAnalyze (Gtk.TreeView tv, string letterStart, string letterEnd)
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
			Catalog.GetString ("Speed") + " (m/s)"
		};
	}

	public override void PassSpeed1or2 (bool isLeft, double speed)
	{
		if (isLeft)
			this.speedStart = speed;
		else
			this.speedEnd = speed;
	}

	protected override string [] getTreeviewStr ()
	{
		return new String [3];
	}

	protected override string [] fillTreeViewStart (string [] str, int i)
	{
		str[i++] = letterStart;
		str[i++] = timeStart;
		str[i++] = Math.Round (speedStart, 3).ToString ();
		return str;
	}

	protected override string [] fillTreeViewEnd (string [] str, int i)
	{
		str[i++] = letterEnd;
		str[i++] = timeEnd;
		str[i++] = Math.Round (speedEnd, 3).ToString ();
		return str;
	}

	protected override string [] fillTreeViewDiff (string [] str, int i)
	{
		str[i++] = Catalog.GetString ("Difference");
		str[i++] = timeDiff;
		str[i++] = Math.Round (speedDiff, 3).ToString ();
		return str;
	}

	protected override string [] fillTreeViewAvg (string [] str, int i)
	{
		str[i++] = Catalog.GetString ("Average");
		str[i++] = ""; // no time avg
		str[i++] = speedAvg;
		return str;
	}

	protected override string [] fillTreeViewMax (string [] str, int i)
	{
		str[i++] = Catalog.GetString ("Maximum");
		str[i++] = ""; // no time max
		str[i++] = speedMax;
		return str;
	}

	public double SpeedDiff {
		set { speedDiff = value; }
	}
}
