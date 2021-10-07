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
 * Copyright (C) 2017,2021   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO;
using Gtk;
using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using Mono.Unix;


public partial class ChronoJumpWindow
{
	[Widget] Gtk.Button button_sprint;
	[Widget] Gtk.Viewport viewport_sprint;
	[Widget] Gtk.Image image_sprint;
	[Widget] Gtk.Button button_sprint_save_image;

	[Widget] Gtk.HBox hbox_sprint_analyze_top_modes;
	[Widget] Gtk.RadioButton radio_sprint_analyze_individual_current_session;
	[Widget] Gtk.RadioButton radio_sprint_analyze_individual_all_sessions;
	[Widget] Gtk.RadioButton radio_sprint_analyze_groupal_current_session;
	[Widget] Gtk.Image image_sprint_analyze_individual_current_set;
	[Widget] Gtk.Image image_sprint_analyze_individual_current_session;
	[Widget] Gtk.Image image_sprint_analyze_individual_all_sessions;
	[Widget] Gtk.Image image_sprint_analyze_groupal_current_session;
	[Widget] Gtk.Notebook notebook_sprint_analyze_top;

	//export
	[Widget] Gtk.Notebook notebook_sprint_export;
	[Widget] Gtk.Label label_sprint_export_data;
	[Widget] Gtk.CheckButton check_sprint_export_images;
	[Widget] Gtk.HBox hbox_sprint_export_width_height;
	[Widget] Gtk.SpinButton spinbutton_sprint_export_image_width;
	[Widget] Gtk.SpinButton spinbutton_sprint_export_image_height;
	[Widget] Gtk.ProgressBar progressbar_sprint_export;
	[Widget] Gtk.Label label_sprint_export_result;
	[Widget] Gtk.Button button_sprint_export_result_open;

	static SprintRGraph sprintRGraph;
	TreeStore storeSprint;

	private void createTreeView_runs_interval_sprint (Gtk.TreeView tv)
	{
		LogB.Information("SPRINT create START");
		UtilGtk.RemoveColumns(tv);
		button_sprint.Sensitive = false;
		image_sprint.Sensitive = false;
		button_sprint_save_image.Sensitive = false;

		tv.HeadersVisible=true;

		int count = 0;
		tv.AppendColumn (Catalog.GetString("Type"), new CellRendererText(), "text", count++);
		tv.AppendColumn ("ID", new CellRendererText(), "text", count++);
		tv.AppendColumn (Catalog.GetString("Distances"), new CellRendererText(), "text", count++);
		tv.AppendColumn (Catalog.GetString("Split times"), new CellRendererText(), "text", count++);
		tv.AppendColumn (Catalog.GetString("Total time"), new CellRendererText(), "text", count++);

		storeSprint = new TreeStore(
				typeof (string), typeof (string), typeof (string),
				typeof (string), typeof (string));
		tv.Model = storeSprint;

		if (currentSession == null || currentPerson == null)
		      return;
		
		tv.Selection.Changed -= onTreeviewSprintSelectionEntry;
		tv.Selection.Changed += onTreeviewSprintSelectionEntry;

		List<object> runITypes = SqliteRunIntervalType.SelectRunIntervalTypesNew("", false);
		string [] runsArray = SqliteRunInterval.SelectRunsSA (
				false, currentSession.UniqueID, currentPerson.UniqueID, "");

		foreach (string line in runsArray)
		{
			//[] lineSplit has run params
			string [] lineSplit = line.Split(new char[] {':'});

			//get intervalTimes
			string intervalTimesString = lineSplit[8];

			string positions = RunInterval.GetSprintPositions(
					Convert.ToDouble(lineSplit[7]), //distanceInterval. == -1 means variable distances
					intervalTimesString,
					SelectRunITypes.RunIntervalTypeDistances(lineSplit[4], runITypes) 	//distancesString
					);
			if(positions == "")
				continue;

			string splitTimes = RunInterval.GetSplitTimes(intervalTimesString, preferences.digitsNumber);

			string [] lineParams = { 
				lineSplit[4],
				lineSplit[1],
				positions,
				splitTimes,
				Util.TrimDecimals(lineSplit[6], preferences.digitsNumber)
			};
			storeSprint.AppendValues (lineParams);
		}
		LogB.Information("SPRINT create END");
	}

	public void addTreeView_runs_interval_sprint (RunInterval runI, RunType runIType)
	{
		LogB.Information("SPRINT add START");
		if(storeSprint == null)
		{
			createTreeView_runs_interval_sprint (treeview_runs_interval_sprint);
			return;
		}

		string positions = RunInterval.GetSprintPositions(
				runI.DistanceInterval, 		//distanceInterval. == -1 means variable distances
				runI.IntervalTimesString,
				runIType.DistancesString 	//distancesString
				);
		if(positions == "")
			return;

		TreeIter iter = new TreeIter();
		bool iterOk = storeSprint.GetIterFirst(out iter);
		if(! iterOk)
			iter = new TreeIter();

		iter = storeSprint.AppendValues (
				runI.Type,
				runI.UniqueID.ToString(),
				positions,
				RunInterval.GetSplitTimes(runI.IntervalTimesString, preferences.digitsNumber),
				Util.TrimDecimals(runI.TimeTotal, preferences.digitsNumber)
				);

		//scroll treeview if needed
		TreePath path = storeSprint.GetPath (iter);
		treeview_runs_interval_sprint.ScrollToCell (path, null, true, 0, 0);
		LogB.Information("SPRINT add END");
	}

	private void onTreeviewSprintSelectionEntry (object o, EventArgs args)
	{
		TreeModel model;
		TreeIter iter;

		// you get the iter and the model if something is selected
		if (((TreeSelection)o).GetSelected(out model, out iter))
		{
			//only allow sprint calculation when there are three tracks
			string splitTimes = (string) model.GetValue(iter, 3);
			if(splitTimes.Split(new char[] {';'}).Length >= 3)
				button_sprint.Sensitive = true;
			else
				button_sprint.Sensitive = false;
		}
		else
			button_sprint.Sensitive = false;
	}

	public static bool GetSelectedSprint (Gtk.TreeView tv)
	{
		TreeModel model;
		TreeIter iter;

		if (tv.Selection.GetSelected (out model, out iter))
		{
			 string positions = (string) model.GetValue(iter, 2);
			 positions = Util.ChangeChars(positions, ",", ".");
			 positions = "0;" + positions;

			 string splitTimes = (string) model.GetValue(iter, 3);
			 splitTimes = Util.ChangeChars(splitTimes, ",", ".");
			 splitTimes = "0;" + splitTimes;

			 sprintRGraph = new SprintRGraph (
					 positions,
					 splitTimes,
					 currentPersonSession.Weight, //TODO: can be more if extra weight
					 currentPersonSession.Height,
					 currentPerson.Name,
					 25);
			return true;
		}
		return false;
	}


	private void on_button_sprint_clicked (object o, EventArgs args)
	{
		if(! GetSelectedSprint(treeview_runs_interval_sprint))
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Error"));
			return;
		}

		on_button_sprint_do ();
	}

	private bool on_button_sprint_do ()
	{
		button_sprint_save_image.Sensitive = false;
		if(currentPersonSession.Weight == 0)
		{
			new DialogMessage(Constants.MessageTypes.WARNING,
					Catalog.GetString("Error, weight of the person cannot be 0"));
			return false;
		}

		if(currentPersonSession.Height == 0)
		{
			new DialogMessage(Constants.MessageTypes.WARNING,
					Catalog.GetString("Error, height of the person cannot be 0"));
			return false;
		}

		if(! sprintRGraph.IsDataOk())
		{
			new DialogMessage(Constants.MessageTypes.WARNING,
					Catalog.GetString("This data does not seem a sprint.") + "\n\n" +
					sprintRGraph.ErrorMessage);
			return false;
		}

		Util.FileDelete(UtilEncoder.GetSprintImage());

		image_sprint.Sensitive = false;

		bool success = sprintRGraph.CallR(
				viewport_sprint.Allocation.Width -5,
				viewport_sprint.Allocation.Height -5,
				true); //singleOrMultiple

		if(! success)
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("This data does not seem a sprint."));
			return false;
		}

		while ( ! Util.FileReadable(UtilEncoder.GetSprintImage()));

		image_sprint = UtilGtk.OpenImageSafe(
				UtilEncoder.GetSprintImage(),
				image_sprint);
		image_sprint.Sensitive = true;
		button_sprint_save_image.Sensitive = true;
		return true;
	}

	private void on_button_sprint_save_image_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.RUNS_SPRINT_SAVE_IMAGE);
	}

	private void on_button_runs_sprint_save_image_selected (string destination)
	{
		try {
			File.Copy(UtilEncoder.GetSprintImage(), destination, true);
		} catch {
			string myString = string.Format(
					Catalog.GetString("Cannot save file {0} "), destination);
			new DialogMessage(Constants.MessageTypes.WARNING, myString);
		}
	}

	private void on_overwrite_file_runs_sprint_save_image_accepted (object o, EventArgs args)
	{
		on_button_runs_sprint_save_image_selected(exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}


	//move to export gui file

	private void on_check_sprint_export_images_toggled (object o, EventArgs args)
	{
		hbox_sprint_export_width_height.Visible = check_sprint_export_images.Active;

		//also hide the label and the open button
		label_sprint_export_result.Text = "";
		button_sprint_export_result_open.Visible = false;
	}

	private void on_radio_sprint_analyze_individual_current_set_toggled (object o, EventArgs args)
	{
		notebook_sprint_analyze_top.CurrentPage = 0;

		label_sprint_export_result.Text = "";
		button_sprint_export_result_open.Visible = false;
	}

	private void on_radio_sprint_analyze_individual_session_current_or_all_toggled (object o, EventArgs args)
	{
		notebook_sprint_analyze_top.CurrentPage = 1;

		if(currentPerson != null)
			label_sprint_export_data.Text = currentPerson.Name;
		else
			label_sprint_export_data.Text = "";

		label_sprint_export_result.Text = "";
		button_sprint_export_result_open.Visible = false;
	}

	private void on_radio_sprint_analyze_groupal_current_session_toggled (object o, EventArgs args)
	{
		notebook_sprint_analyze_top.CurrentPage = 1;

		label_sprint_export_data.Text = currentSession.Name;

		label_sprint_export_result.Text = "";
		button_sprint_export_result_open.Visible = false;
	}

	private void on_button_sprint_export_not_set_clicked (object o, EventArgs args)
	{
		// 1) check if all sessions
		if(radio_sprint_analyze_individual_all_sessions.Active)
		{
			if(currentPerson == null)
				return;

			button_sprint_export_session (currentPerson.UniqueID, -1);
			return;
		}

		// 2) current session (individual or groupal)
		if(currentSession == null)
			return;

		if (radio_sprint_analyze_individual_current_session.Active)
		{
			if(currentPerson == null)
				return;

			button_sprint_export_session (currentPerson.UniqueID, currentSession.UniqueID);
		}
		else if (radio_sprint_analyze_groupal_current_session.Active)
		{
			button_sprint_export_session (-1, currentSession.UniqueID);
		}
	}

	SprintExport sprintExport;
	private void button_sprint_export_session (int personID, int sessionID)
	{
		//continue based on: private void button_run_encoder_export_session (int personID)
		//TODO: sensitive stuff (false)

		label_sprint_export_result.Text = "";
		button_sprint_export_result_open.Visible = false;

		//store new width/height if changed
		Sqlite.Open();
		preferences.exportGraphWidth = Preferences.PreferencesChange(
				SqlitePreferences.ExportGraphWidth,
				preferences.exportGraphWidth, Convert.ToInt32(spinbutton_sprint_export_image_width.Value));
		preferences.exportGraphHeight = Preferences.PreferencesChange(
				SqlitePreferences.ExportGraphHeight,
				preferences.exportGraphHeight, Convert.ToInt32(spinbutton_sprint_export_image_height.Value));
		Sqlite.Close();

		//change also spinbuttons of export forceSensor and runEncoder
		spinbutton_force_sensor_export_image_width.Value = spinbutton_sprint_export_image_width.Value;
		spinbutton_force_sensor_export_image_height.Value = spinbutton_sprint_export_image_height.Value;

		spinbutton_run_encoder_export_image_width.Value = spinbutton_sprint_export_image_width.Value;
		spinbutton_run_encoder_export_image_height.Value = spinbutton_sprint_export_image_height.Value;


		sprintExport = new SprintExport(
				notebook_sprint_export,
				progressbar_sprint_export,
				label_sprint_export_result,
				check_sprint_export_images.Active,
				Convert.ToInt32(spinbutton_sprint_export_image_width.Value),
				Convert.ToInt32(spinbutton_sprint_export_image_height.Value),
				UtilAll.IsWindows(),
				personID, sessionID,
				preferences.CSVExportDecimalSeparatorChar,      //decimalIsPointAtExport (write)
				preferences.digitsNumber);

		sprintExport.Button_done.Clicked -= new EventHandler(sprint_export_done);
		sprintExport.Button_done.Clicked += new EventHandler(sprint_export_done);

		bool selectedFile = false;
		if(check_sprint_export_images.Active)
		{
			if(personID == -1)
				selectedFile = checkFolder (Constants.CheckFileOp.RUNS_SPRINT_EXPORT_GROUPAL_CURRENT_SESSION_YES_IMAGES);
			else if (sessionID == -1)
				selectedFile = checkFolder (Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_ALL_SESSIONS_YES_IMAGES);
			else
				selectedFile = checkFolder (Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_CURRENT_SESSION_YES_IMAGES);
		} else {
			if(personID == -1)
				selectedFile = checkFile (Constants.CheckFileOp.RUNS_SPRINT_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES);
			else if (sessionID == -1)
				selectedFile = checkFile (Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES);
			else
				selectedFile = checkFile (Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES);
		}

		//restore the gui if cancelled
		if(! selectedFile) {
			//TODO: sensitive stuff (true)
		}
	}
	private void on_button_sprint_export_file_selected (string selectedFileName)
	{
		//sprintExport.Start("/tmp/prova_sprintExport.csv");
		sprintExport.Start(selectedFileName); //file or folder
	}

	private void on_button_sprint_export_cancel_clicked (object o, EventArgs args)
	{
		sprintExport.Cancel();
	}

	private void sprint_export_done (object o, EventArgs args)
	{
		sprintExport.Button_done.Clicked -= new EventHandler(sprint_export_done);

//		sprintButtonsSensitive(true);
		hbox_sprint_analyze_top_modes.Sensitive = true;

		if(sprintExport != null && sprintExport.AllOk)
			button_sprint_export_result_open.Visible = true;
	}

	private void on_button_sprint_export_result_open_clicked (object o, EventArgs args)
	{
		if(sprintExport == null || sprintExport.ExportURL == "")
		{
			new DialogMessage(Constants.MessageTypes.WARNING,
					Constants.DirectoryCannotOpenStr());
			return;
		}

		if(! Util.OpenURL (sprintExport.ExportURL))
			new DialogMessage(Constants.MessageTypes.WARNING,
					Constants.DirectoryCannotOpenStr() + "\n\n" + sprintExport.ExportURL);
	}

}
