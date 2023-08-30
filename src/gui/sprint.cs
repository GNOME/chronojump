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
 * Copyright (C) 2017,2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO;
using System.Threading;
using Gtk;
//using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using Mono.Unix;


public partial class ChronoJumpWindow
{
	Gtk.Button button_sprint;
	Gtk.Viewport viewport_sprint;
	Gtk.Image image_sprint;
	Gtk.Button button_sprint_save_image;

	Gtk.HBox hbox_sprint_analyze_top_modes;
	Gtk.RadioButton radio_sprint_analyze_individual_current_session;
	Gtk.RadioButton radio_sprint_analyze_individual_all_sessions;
	Gtk.RadioButton radio_sprint_analyze_groupal_current_session;
	Gtk.Image image_sprint_analyze_individual_current_set;
	Gtk.Image image_sprint_analyze_individual_current_session;
	Gtk.Image image_sprint_analyze_individual_all_sessions;
	Gtk.Image image_sprint_analyze_groupal_current_session;
	Gtk.Notebook notebook_sprint_analyze_top;
	Gtk.TreeView treeview_sprint;
	Gtk.Button button_sprint_table_save;

	//export
	Gtk.Notebook notebook_sprint_export;
	Gtk.Label label_sprint_export_data;
	Gtk.CheckButton check_sprint_export_images;
	Gtk.HBox hbox_sprint_export_width_height;
	Gtk.SpinButton spinbutton_sprint_export_image_width;
	Gtk.SpinButton spinbutton_sprint_export_image_height;
	Gtk.Label label_sprint_export;
	Gtk.ProgressBar progressbar_sprint_export;
	Gtk.Label label_sprint_export_discarded;
	Gtk.Label label_sprint_export_result;
	Gtk.Button button_sprint_export_result_open;

	static SprintRGraph sprintRGraph;
	TreeStore storeSprint;

	private void createTreeView_runs_interval_sprint (Gtk.TreeView tv)
	{
		LogB.Information("SPRINT create START");
		UtilGtk.RemoveColumns(tv);
		button_sprint.Sensitive = false;
		image_sprint.Sensitive = false;
		button_sprint_save_image.Sensitive = false;
		button_sprint_table_save.Sensitive = false;

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
					SelectRunITypes.RunIntervalTypeDistancesString (lineSplit[4], runITypes) 	//distancesString
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
		ITreeModel model;
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
		ITreeModel model;
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
		if(operatingSystem == UtilAll.OperatingSystems.MACOSX &&
				! Util.FileExists(Constants.ROSX) )
		{
			showMacRInstallMessage ();
			return;
		}

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
		button_sprint_table_save.Sensitive = false;
		treeview_sprint = UtilGtk.RemoveColumns (treeview_sprint);

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

		//csv
		Thread.Sleep (250); //Wait a bit to ensure the csv is done
		string contents = Util.ReadFile (RunInterval.GetCSVResultsURL(), false);
		/*
		   maybe captured data was too low or two different than an sprint.
		   Then we have image but maybe we have no sprintResults.csv
		   Length < 10 is written because on a model too short R can just return ""
		   */
		if(contents == null || contents == "" || contents.Length < 10)
			return false;
		else {
			createTreeViewAnalyzeSprint (contents);

			button_sprint_table_save.Sensitive = true;
		}

		return true;
	}

	private void on_button_sprint_table_save_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.RUNS_SPRINT_SAVE_TABLE);
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

	private void on_button_runs_sprint_save_table_selected (string destination)
	{
		try {
			//this overwrites if needed
			TextWriter writer = File.CreateText(destination);

			string sep = " ";
			if (preferences.CSVExportDecimalSeparator == "COMMA")
				sep = ";";
			else
				sep = ",";

			string contents = Util.ReadFile(RunInterval.GetCSVResultsURL(), false);

			//write header
			writer.WriteLine(Util.RemoveNewLine(Util.StringArrayToString(
							getTreeviewSprintHeaders (contents), sep), true));

			SprintCSV csv = readSprintCSVContents (contents);

			writer.WriteLine (csv.ToCSV (preferences.CSVExportDecimalSeparator));

			writer.Flush();
			writer.Close();
			((IDisposable)writer).Dispose();
		} catch {
			string myString = string.Format(
					Catalog.GetString("Cannot save file {0} "), destination);
			new DialogMessage(Constants.MessageTypes.WARNING, myString);
		}
	}

	private void on_overwrite_file_runs_sprint_save_table_accepted(object o, EventArgs args)
	{
		on_button_runs_sprint_save_table_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}

	//note this is almost the same than runEncoder
	private void createTreeViewAnalyzeSprint (string contents)
	{
		// 1) read the contents of the CSV
		SprintCSV csv = readSprintCSVContents (contents);

		// 2) Add the columns to the treeview
		string [] columnsString = getTreeviewSprintHeaders (contents);
		int count = 0;
		foreach(string column in columnsString)
			treeview_sprint.AppendColumn (column, new CellRendererText(), "text", count++);

		// 3) Add the TreeStore
		Type [] types = new Type [columnsString.Length];
		for (int i=0; i < columnsString.Length; i++) {
			types[i] = typeof (string);
		}
		TreeStore store = new TreeStore(types);

		store.AppendValues (csv.ToTreeView());

		// 4) Assing model to store and other tweaks
		treeview_sprint.Model = store;
		treeview_sprint.Selection.Mode = SelectionMode.None;
                treeview_sprint.HeadersVisible=true;
	}

	//note this is almost the same than runEncoder
	private string [] getTreeviewSprintHeaders (string contents)
        {
		// 1) check how many dist columns we should add
		List<string> dist_l = new List<string> ();
		using (StringReader reader = new StringReader (contents))
		{
			string line = reader.ReadLine ();      //headers
			LogB.Information(line);
			if (line != null)
			{
				string [] cells = line.Split(new char[] {';'});
				dist_l = new List<string> ();
				for (int i = 26; i < cells.Length; i ++) //Attention!: take care with this 26 if in the future add more columns before dist/times
				{
					//each string comes as "X0Y25.5m_Speed" convert to 0-25.5 m\nSpeed or 0-25,5 m/nSpeed
					string temp = Util.RemoveChar (cells[i], '"', false);
					temp = Util.RemoveChar (temp, 'X', false);
					temp = Util.ChangeChars (temp, "Y", "-");
					temp = Util.ChangeDecimalSeparator (temp);
					temp = Util.ChangeChars (temp, "_", "\n");

					dist_l.Add (temp);
				}
			}
		}

		// 2) prepare the headers
                string [] headers = {
			"Mass\n\n(Kg)", "Height\n\n(m)", "Temperature\n\n(ºC)",
			"V (wind)\n\n(m/s)", "Ka\n\n", "K\nfitted\n(s^-1)",
			"Vmax\nfitted\n(m/s)", "Amax\nfitted\n(m/s^2)", "Fmax\nfitted\n(N)",
			"Fmax\nrel fitted\n(N/Kg)", "Sfv\nfitted\n", "Sfv\nrel fitted\n",
			"Sfv\nlm\n", "Sfv\nrel lm\n", "Pmax\nfitted\n(W)",
			"Pmax\nrel fitted\n(W/Kg)", "Time to pmax\nfitted\n(s)", "F0\n\n(N)",
			"F0\nrel\n(N/Kg)", "V0\n\n(m/s)", "Pmax\nlm\n(W)",
			"Pmax\nrel lm\n(W/Kg)"
		};

		// 3) add the dists to the headers
		headers = Util.AddToArrayString (headers, dist_l);

		return headers;
	}

	//note this is almost the same than runEncoder
	//right now it only returns one line
	private SprintCSV readSprintCSVContents (string contents)
	{
		SprintCSV csv = new SprintCSV();
		string line;
		using (StringReader reader = new StringReader (contents))
		{
			line = reader.ReadLine ();      //headers
			do {
				line = reader.ReadLine ();
				LogB.Information(line);
				if (line == null)
					break;

				string [] cells = line.Split(new char[] {';'});

				// get the times (total columns can be different each time)
				List<double> time_l = new List<double> ();
				for (int i = 26; i < cells.Length; i ++) //Attention! take care with this 26 if in the future add more columns before dist/times
					time_l.Add (Convert.ToDouble (cells[i]));

				csv = new SprintCSV (
						Convert.ToDouble(cells[0]), Convert.ToDouble(cells[1]), Convert.ToInt32(cells[2]),
						Convert.ToDouble(cells[3]), Convert.ToDouble(cells[4]), Convert.ToDouble(cells[5]),
						Convert.ToDouble(cells[6]), Convert.ToDouble(cells[7]), Convert.ToDouble(cells[8]),
						Convert.ToDouble(cells[9]), Convert.ToDouble(cells[10]), Convert.ToDouble(cells[11]),
						Convert.ToDouble(cells[12]), Convert.ToDouble(cells[13]), Convert.ToDouble(cells[14]),
						Convert.ToDouble(cells[15]), Convert.ToDouble(cells[16]), Convert.ToDouble(cells[17]),
						Convert.ToDouble(cells[18]), Convert.ToDouble(cells[19]), Convert.ToDouble(cells[20]),
						Convert.ToDouble(cells[21]),
						Convert.ToDouble(cells[22]), Convert.ToDouble(cells[23]), //vmax raw, amax raw //both unused
						Convert.ToDouble(cells[24]), Convert.ToDouble(cells[25]), //fmax raw, pmax raw //both unused
						time_l
						);
			} while(true);
		}

		return csv;
	}

	//move to export gui file

	private void on_check_sprint_export_images_toggled (object o, EventArgs args)
	{
		hbox_sprint_export_width_height.Visible = check_sprint_export_images.Active;

		//also hide the label and the open button
		label_sprint_export_discarded.Text = "";
		label_sprint_export_result.Text = "";
		button_sprint_export_result_open.Visible = false;
	}

	private void on_radio_sprint_analyze_individual_current_set_toggled (object o, EventArgs args)
	{
		notebook_sprint_analyze_top.CurrentPage = 0;

		label_sprint_export_discarded.Text = "";
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

		label_sprint_export_discarded.Text = "";
		label_sprint_export_result.Text = "";
		button_sprint_export_result_open.Visible = false;
	}

	private void on_radio_sprint_analyze_groupal_current_session_toggled (object o, EventArgs args)
	{
		notebook_sprint_analyze_top.CurrentPage = 1;

		label_sprint_export_data.Text = currentSession.Name;

		label_sprint_export_discarded.Text = "";
		label_sprint_export_result.Text = "";
		button_sprint_export_result_open.Visible = false;
	}

	private void on_button_sprint_export_not_set_clicked (object o, EventArgs args)
	{
		// 1) avoid exporting to R on mac if R is not installed
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

		// 2) check if all sessions
		if(radio_sprint_analyze_individual_all_sessions.Active)
		{
			if(currentPerson == null)
				return;

			button_sprint_export_session (currentPerson.UniqueID, -1);
			return;
		}

		// 3) current session (individual or groupal)
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

		label_sprint_export_discarded.Text = "";
		label_sprint_export_result.Text = "";
		button_sprint_export_result_open.Visible = false;

		//store new width/height if changed
		Sqlite.Open();
		preferences.exportGraphWidth = Preferences.PreferencesChange(
				true, SqlitePreferences.ExportGraphWidth,
				preferences.exportGraphWidth, Convert.ToInt32(spinbutton_sprint_export_image_width.Value));
		preferences.exportGraphHeight = Preferences.PreferencesChange(
				true, SqlitePreferences.ExportGraphHeight,
				preferences.exportGraphHeight, Convert.ToInt32(spinbutton_sprint_export_image_height.Value));
		Sqlite.Close();

		//change also spinbuttons of export forceSensor and runEncoder
		spinbutton_ai_export_image_width.Value = spinbutton_sprint_export_image_width.Value;
		spinbutton_ai_export_image_height.Value = spinbutton_sprint_export_image_height.Value;

		sprintExport = new SprintExport(
				notebook_sprint_export,
				label_sprint_export, progressbar_sprint_export,
				label_sprint_export_discarded,
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

	private void connectWidgetsSprint (Gtk.Builder builder)
	{
		button_sprint = (Gtk.Button) builder.GetObject ("button_sprint");
		viewport_sprint = (Gtk.Viewport) builder.GetObject ("viewport_sprint");
		image_sprint = (Gtk.Image) builder.GetObject ("image_sprint");
		button_sprint_save_image = (Gtk.Button) builder.GetObject ("button_sprint_save_image");

		hbox_sprint_analyze_top_modes = (Gtk.HBox) builder.GetObject ("hbox_sprint_analyze_top_modes");
		radio_sprint_analyze_individual_current_session = (Gtk.RadioButton) builder.GetObject ("radio_sprint_analyze_individual_current_session");
		radio_sprint_analyze_individual_all_sessions = (Gtk.RadioButton) builder.GetObject ("radio_sprint_analyze_individual_all_sessions");
		radio_sprint_analyze_groupal_current_session = (Gtk.RadioButton) builder.GetObject ("radio_sprint_analyze_groupal_current_session");
		image_sprint_analyze_individual_current_set = (Gtk.Image) builder.GetObject ("image_sprint_analyze_individual_current_set");
		image_sprint_analyze_individual_current_session = (Gtk.Image) builder.GetObject ("image_sprint_analyze_individual_current_session");
		image_sprint_analyze_individual_all_sessions = (Gtk.Image) builder.GetObject ("image_sprint_analyze_individual_all_sessions");
		image_sprint_analyze_groupal_current_session = (Gtk.Image) builder.GetObject ("image_sprint_analyze_groupal_current_session");
		notebook_sprint_analyze_top = (Gtk.Notebook) builder.GetObject ("notebook_sprint_analyze_top");
		treeview_sprint = (Gtk.TreeView) builder.GetObject ("treeview_sprint");
		button_sprint_table_save = (Gtk.Button) builder.GetObject ("button_sprint_table_save");

		//export
		notebook_sprint_export = (Gtk.Notebook) builder.GetObject ("notebook_sprint_export");
		label_sprint_export_data = (Gtk.Label) builder.GetObject ("label_sprint_export_data");
		check_sprint_export_images = (Gtk.CheckButton) builder.GetObject ("check_sprint_export_images");
		hbox_sprint_export_width_height = (Gtk.HBox) builder.GetObject ("hbox_sprint_export_width_height");
		spinbutton_sprint_export_image_width = (Gtk.SpinButton) builder.GetObject ("spinbutton_sprint_export_image_width");
		spinbutton_sprint_export_image_height = (Gtk.SpinButton) builder.GetObject ("spinbutton_sprint_export_image_height");
		label_sprint_export = (Gtk.Label) builder.GetObject ("label_sprint_export");
		progressbar_sprint_export = (Gtk.ProgressBar) builder.GetObject ("progressbar_sprint_export");
		label_sprint_export_discarded = (Gtk.Label) builder.GetObject ("label_sprint_export_discarded");
		label_sprint_export_result = (Gtk.Label) builder.GetObject ("label_sprint_export_result");
		button_sprint_export_result_open = (Gtk.Button) builder.GetObject ("button_sprint_export_result_open");
	}
}
