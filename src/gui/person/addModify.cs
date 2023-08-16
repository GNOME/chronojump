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
 * Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Gdk;
//using Glade;
using Mono.Unix;
using System.IO; 

public class PersonAddModifyWindow
{
	// at glade ---->
	Gtk.Window person_win;
	Gtk.RadioButton radio_metric;
	Gtk.RadioButton radio_imperial;
	Gtk.Entry entry1;
	Gtk.RadioButton radiobutton_sex_u;
	Gtk.RadioButton radiobutton_sex_m;
	Gtk.RadioButton radiobutton_sex_f;
	Gtk.Entry entry_club_id;
	Gtk.TextView textview_description;
	Gtk.TextView textview_ps_comments;

	Gtk.Frame frame_main;
	Gtk.Notebook notebook_main;
	Gtk.HButtonBox hbuttonbox_main;
	Gtk.HBox hbox_units;
	Gtk.VBox vbox_error;
	Gtk.Label label_error;
	Gtk.Button button_load_person;
	Gtk.Image image_load_person;

	Gtk.Image image_photo_from_file;
	Gtk.Image image_photo_preview;
	Gtk.Image image_photo_do;

	Gtk.Button button_delete_photo_file;
	Gtk.Image image_photo_delete;

	//Gtk.Button button_add_photo_file;
	//Gtk.Button button_take_photo_do;
	Gtk.HBox hbox_camera;
	
	Gtk.Label label_date;
	//Gtk.Button button_change_date;
	Gtk.Image image_calendar;

	Gtk.HBox hbox_weight_metric;
	Gtk.HBox hbox_weight_imperial;
	Gtk.SpinButton spinbutton_weight_metric;
	Gtk.SpinButton spinbutton_weight_imperial;

	Gtk.HBox hbox_height_metric;
	Gtk.HBox hbox_height_imperial;
	Gtk.SpinButton spinbutton_height_metric;
	Gtk.SpinButton spinbutton_height_imperial_feet;
	Gtk.SpinButton spinbutton_height_imperial_inches;

	Gtk.HBox hbox_leg_length_metric;
	Gtk.HBox hbox_leg_length_imperial;
	Gtk.SpinButton spinbutton_leg_length_metric;
	Gtk.SpinButton spinbutton_leg_length_imperial_feet;
	Gtk.SpinButton spinbutton_leg_length_imperial_inches;

	Gtk.HBox hbox_trochanter_floor_on_flexion_metric;
	Gtk.HBox hbox_trochanter_floor_on_flexion_imperial;
	Gtk.SpinButton spinbutton_trochanter_floor_on_flexion_metric;
	Gtk.SpinButton spinbutton_trochanter_floor_on_flexion_imperial_feet;
	Gtk.SpinButton spinbutton_trochanter_floor_on_flexion_imperial_inches;
	
	Gtk.Box hbox_combo_sports;
	Gtk.Label label_speciallity;
	Gtk.Box hbox_combo_speciallities;
	Gtk.Box hbox_combo_levels;
	
	Gtk.Box hbox_combo_continents;
	Gtk.Box hbox_combo_countries;
	
	Gtk.Image image_name;
	Gtk.Image image_weight;
	
	Gtk.Button button_zoom;
	Gtk.Image image_photo_mini;
	Gtk.Image image_zoom;

	Gtk.Button button_accept;
	// <---- at glade


	Gtk.ComboBoxText combo_sports;
	Gtk.ComboBoxText combo_speciallities;
	Gtk.ComboBoxText combo_levels;
	Gtk.ComboBoxText combo_continents;
	Gtk.ComboBoxText combo_countries;

	//used for connect ok gui/chronojump.cs, PersonRecuperate, PersonRecuperateFromOtherSession,this class, gui/convertWeight.cs
	private Gtk.Button fakeButtonAccept;
	private Gtk.Button fakeButtonCancel;
	
	static ConvertWeightWindow convertWeightWin;
	
	static PersonAddModifyWindow PersonAddModifyWindowBox;
	
	DialogCalendar myDialogCalendar;
	DateTime dateTime;
	Sport sport;
	string [] sports;
	string [] sportsTranslated;
	string [] speciallities;
	string [] speciallitiesTranslated;
	//String level;
	string [] levels;
	string [] continents;
	string [] continentsTranslated;
	string [] countries;
	string [] countriesTranslated;

	GenericWindow genericWin;

	bool adding;
	private bool descriptionChanging = false;
	private bool textviewpsChanging = false;

	private Person currentPerson;
	private Session currentSession;
	private string videoDevice;
	private string videoDevicePixelFormat;
	private string videoDeviceResolution;
	private string videoDeviceFramerate;
	private PersonSession currentPersonSession;
	private string sex = Constants.SexU;
	private double weightIniMetric;
	int pDN;
	//Gtk.CheckButton app1_checkbutton_video_contacts;
	
	private int serverUniqueID;

	//
	//if we are adding a person, currentPerson.UniqueID it's -1
	//if we are modifying a person, currentPerson.UniqueID is obviously it's ID
	//showCapturePhoto is false on raspberry to not use camera
	//PersonAddModifyWindow (Gtk.Window parent, Session currentSession, Person currentPerson, bool showCapturePhoto)
	PersonAddModifyWindow (Gtk.Window parent, Session currentSession, Person currentPerson)
	{
		//this strings will be used on merge persons code. Now are here just to be translated in the meantime.
		LogB.Information (Catalog.GetString ("Merge with another person"));
		LogB.Information (string.Format (Catalog.GetString ("Merge persons '{0}' with '{1}' in all sessions."), "xavi", "jordi") + "\n" +
			string.Format (Catalog.GetString ("All tests of person '{0}' will be assigned to person '{1}'."),
				"jordi", "xavi")); //Are you sure?
		LogB.Information (Catalog.GetString ("Please, choose person's parameters."));
		LogB.Information (string.Format (Catalog.GetString ("Please, choose person's parameters at session {0}."), "session"));

		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "person_win.glade", "person_win", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "person_win.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);
		
		//put an icon to window
		UtilGtk.IconWindow(person_win);

		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.WindowColor(person_win, Config.ColorBackground);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_error);

			UtilGtk.WidgetColor (frame_main, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsFrame (Config.ColorBackgroundShiftedIsDark, frame_main);

			UtilGtk.WidgetColor (notebook_main, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_main);
		}

		person_win.Parent = parent;
		this.currentSession = currentSession;
		this.currentPerson = currentPerson;

		if(currentPerson.UniqueID == -1)
			adding = true;
		else
			adding = false;
		
		createComboSports();
		createComboSpeciallities(-1);
		label_speciallity.Hide();
		combo_speciallities.Hide();
		createComboLevels();
		createComboContinents();
		createComboCountries();
		
		Pixbuf pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "calendar.png"); //from asssembly
		image_calendar.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "portrait_zoom.png");
		image_zoom.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person_outline.png");
		image_load_person.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_delete.png");
		image_photo_delete.Pixbuf = pixbuf;

		image_photo_from_file.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_attachment.png");
		image_photo_preview.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_photo_preview.png");
		image_photo_do.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_photo_do.png");

		//delete a -1.png or -1.jpg added before on a new user where "accept" button was not pressed and window was closed
		deleteOldPhotosIfAny(-1);

		string photoFile = Util.UserPhotoURL(true, currentPerson.UniqueID);
		if(photoFile != "") {
			try {
				pixbuf = new Pixbuf (photoFile); //from a file
				image_photo_mini.Pixbuf = pixbuf;
			} catch {
				//on windows there are problem using the fileNames that are not on temp
				string tempFileName = Path.Combine(Path.GetTempPath(), Constants.PhotoSmallTemp +
						Util.GetMultimediaExtension(Constants.MultimediaItems.PHOTO));
				File.Copy(photoFile, tempFileName, true);
				pixbuf = new Pixbuf (tempFileName);
				image_photo_mini.Pixbuf = pixbuf;
			}

			button_delete_photo_file.Sensitive = true;
		}
		else {
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_no_photo.png");
			image_photo_mini.Pixbuf = pixbuf;
			button_delete_photo_file.Sensitive = false;
		}

		//show zoom button only if big image exists
		string photoBigFile = Util.UserPhotoURL(false, currentPerson.UniqueID);
		if(photoBigFile != "")
		{
			button_zoom.Sensitive = true;
			//button_add_photo_file.Label = Catalog.GetString("Change photo");
		}
		else
			button_zoom.Sensitive = false;

		fakeButtonAccept = new Gtk.Button();
		fakeButtonCancel = new Gtk.Button();

		entry1.CanFocus = true;
		entry1.IsFocus = true;

		if(adding) {
			person_win.Title = Catalog.GetString ("New person");
			//button_accept.Sensitive = false;
		} else 
			person_win.Title = Catalog.GetString ("Edit person");

		person_win.Show();
	}

	private void on_radio_metric_imperial_toggled (object o, EventArgs args)
	{
		if(radio_metric.Active)
		{
			spinbutton_weight_metric.Value = Util.ConvertPoundsToKg (spinbutton_weight_imperial.Value);
			spinbutton_height_metric.Value = Util.ConvertFeetInchesToCm (
					Convert.ToInt32(spinbutton_height_imperial_feet.Value),
					spinbutton_height_imperial_inches.Value);
			spinbutton_leg_length_metric.Value = Util.ConvertFeetInchesToCm (
					Convert.ToInt32(spinbutton_leg_length_imperial_feet.Value),
					spinbutton_leg_length_imperial_inches.Value);
			spinbutton_trochanter_floor_on_flexion_metric.Value = Util.ConvertFeetInchesToCm (
					Convert.ToInt32(spinbutton_trochanter_floor_on_flexion_imperial_feet.Value),
					spinbutton_trochanter_floor_on_flexion_imperial_inches.Value);

			hbox_weight_metric.Visible = true;
			hbox_weight_imperial.Visible = false;
			hbox_height_metric.Visible = true;
			hbox_height_imperial.Visible = false;
			hbox_leg_length_metric.Visible = true;
			hbox_leg_length_imperial.Visible = false;
			hbox_trochanter_floor_on_flexion_metric.Visible = true;
			hbox_trochanter_floor_on_flexion_imperial.Visible = false;
		}
		else if(radio_imperial.Active)
		{
			spinbutton_weight_imperial.Value = Util.ConvertKgToPounds (spinbutton_weight_metric.Value);

			int feet = 0;
			double inches = 0;
			Util.ConvertCmToFeetInches (spinbutton_height_metric.Value, out feet, out inches);
			spinbutton_height_imperial_feet.Value = feet;
			spinbutton_height_imperial_inches.Value = inches;

			Util.ConvertCmToFeetInches (spinbutton_leg_length_metric.Value, out feet, out inches);
			spinbutton_leg_length_imperial_feet.Value = feet;
			spinbutton_leg_length_imperial_inches.Value = inches;

			Util.ConvertCmToFeetInches (spinbutton_trochanter_floor_on_flexion_metric.Value, out feet, out inches);
			spinbutton_trochanter_floor_on_flexion_imperial_feet.Value = feet;
			spinbutton_trochanter_floor_on_flexion_imperial_inches.Value = inches;

			hbox_weight_metric.Visible = false;
			hbox_weight_imperial.Visible = true;
			hbox_height_metric.Visible = false;
			hbox_height_imperial.Visible = true;
			hbox_leg_length_metric.Visible = false;
			hbox_leg_length_imperial.Visible = true;
			hbox_trochanter_floor_on_flexion_metric.Visible = false;
			hbox_trochanter_floor_on_flexion_imperial.Visible = true;
		}
	}

	void on_button_zoom_clicked (object o, EventArgs args) {
		string tempFileName = Path.Combine(Path.GetTempPath(), Constants.PhotoTemp +
				Util.GetMultimediaExtension(Constants.MultimediaItems.PHOTO));
		if(! adding) {
			//on windows there are problem using the fileNames that are not on temp
			string fileName = Util.UserPhotoURL(false, currentPerson.UniqueID);
			File.Copy(fileName, tempFileName, true);
		}

		new DialogImageTest(currentPerson.Name, tempFileName, DialogImageTest.ArchiveType.FILE);
	}

	/*
	 * used when:
	 * 1.- adding a photo to delete a possible duplicate, eg user 231 has 231.png and now will add 231.jpg
	 * 2.- start this window to delete a -1.png or -1.jpg added before on a new user where "accept" button was not pressed and window was closed
	 */
	private void deleteOldPhotosIfAny(int uniqueID)
	{
		LogB.Information("deleteOldPhotosIfAny: " + uniqueID.ToString());
		string file = Util.UserPhotoURL(false, uniqueID); //default
		if(file != "")
			Util.FileDelete(file);

		file  = Util.UserPhotoURL(true, uniqueID); //small
		if(file != "")
			Util.FileDelete(file);
	}

	//user wants to delete his photo
	private void deletePhoto (int uniqueID)
	{
		LogB.Information("deletePhoto: " + uniqueID.ToString());
		string file = Util.UserPhotoURL(false, uniqueID); //default
		if(file != "")
			Util.FileDelete(file);

		file  = Util.UserPhotoURL(true, uniqueID); //small
		if(file != "")
			Util.FileDelete(file);
	}

	void on_button_add_photo_file_clicked (object o, EventArgs args)
	{
		Gtk.FileChooserDialog fc=
			new Gtk.FileChooserDialog(Catalog.GetString("Select file"),
					person_win,
					FileChooserAction.Open,
					Catalog.GetString("Cancel"),ResponseType.Cancel,
					Catalog.GetString("Accept"),ResponseType.Accept
					);

		fc.Filter = new FileFilter();
		fc.Filter.AddPattern("*.png");
		fc.Filter.AddPattern("*.PNG");
		fc.Filter.AddPattern("*.jpg");
		fc.Filter.AddPattern("*.JPG");
		fc.Filter.AddPattern("*.jpeg");
		fc.Filter.AddPattern("*.JPEG");

		if (fc.Run() == (int)ResponseType.Accept)
		{
			bool originalCopySuccess = false;
			try {
				deleteOldPhotosIfAny(currentPerson.UniqueID);
				if(UtilMultimedia.GetImageType(fc.Filename) == UtilMultimedia.ImageTypes.JPEG)
				{
					File.Copy(fc.Filename, Util.GetPhotoFileName(false, currentPerson.UniqueID), true); //overwrite
					originalCopySuccess = true;
				}
				else if(UtilMultimedia.GetImageType(fc.Filename) == UtilMultimedia.ImageTypes.PNG)
				{
					File.Copy(fc.Filename, Util.GetPhotoPngFileName(false, currentPerson.UniqueID), true); //overwrite
					originalCopySuccess = true;
				}
			}
			catch {
				LogB.Warning("Catched! photo cannot be added");
				new DialogMessage(Constants.MessageTypes.WARNING, string.Format(
							Catalog.GetString("Cannot save file {0} "), fc.Filename));
			}

			if(originalCopySuccess)
			{
				//mini will be always png from now on (after 1.7.1-213)
				string filenameMini = Util.GetPhotoPngFileName(true, currentPerson.UniqueID);
				bool miniSuccess = UtilMultimedia.LoadAndResizeImage(fc.Filename, filenameMini, 150, -1); //-1: maintain aspect ratio
				if(miniSuccess)
					showMiniPhoto(filenameMini);
			}
		}
		//Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
		fc.Destroy();
	}

	private void showMiniPhoto(string filenameMini)
	{
		Pixbuf pixbuf = new Pixbuf (filenameMini);
		image_photo_mini.Pixbuf = pixbuf;
		//button_add_photo_file.Label = Catalog.GetString("Change photo");
		button_zoom.Sensitive = true;
	}

	//Gtk.Window capturerWindow;
	Webcam webcam;
	//CapturerBin capturer;
	void on_button_take_photo_preview_camera_clicked (object o, EventArgs args)
	{
		// A) end if it's running
		if(webcam != null && webcam.Running)
		{
			webcam.RecordingStop();
			//return;
		}

		// B) start if it's not running
		//webcam = new WebcamMplayer (videoDevice);
		//Webcam.Result result = webcam.CapturePrepare (Webcam.CaptureTypes.PHOTO);
		//constructor for playpreview
		webcam = new WebcamFfmpeg (Webcam.Action.PLAYPREVIEW, UtilAll.GetOSEnum(), videoDevice, videoDevicePixelFormat, videoDeviceResolution, videoDeviceFramerate);
		//Webcam.Result result = webcam.PlayPreviewNoBackground ();
		Webcam.Result result = webcam.PlayPreview ();

		if (! result.success)
		{
			LogB.Debug ("Webcam Ffmpeg error: ", result.error);
			new DialogMessage (Constants.MessageTypes.WARNING, result.error);
			return;
		}
	}
	void on_button_take_photo_do_clicked (object o, EventArgs args)
	{
		if(webcam == null)
			webcam = new WebcamFfmpeg (Webcam.Action.PLAYPREVIEW, UtilAll.GetOSEnum(), videoDevice, videoDevicePixelFormat, videoDeviceResolution, videoDeviceFramerate);
		else if(webcam != null && webcam.Running)
		{
			webcam.RecordingStop();
		}

		if(webcam.Snapshot())
		{
			File.Copy(Util.GetWebcamPhotoTempFileNamePost(),
					Util.GetPhotoPngFileName(false, currentPerson.UniqueID), true); //overwrite

			string filenameMini = Util.GetPhotoPngFileName(true, currentPerson.UniqueID);
			bool miniSuccess = UtilMultimedia.LoadAndResizeImage(
					Util.GetPhotoPngFileName(false, currentPerson.UniqueID),
					filenameMini, 150, -1); //-1: maintain aspect ratio
			if(miniSuccess)
				showMiniPhoto(filenameMini);
		}
		button_delete_photo_file.Sensitive = true;
	}

	private void on_button_delete_photo_file_clicked (object o, EventArgs args)
	{
		deletePhoto (currentPerson.UniqueID);

		button_delete_photo_file.Sensitive = false;

		Pixbuf pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_no_photo.png");
		image_photo_mini.Pixbuf = pixbuf;
		button_zoom.Sensitive = false;
	}

	void on_entries_required_changed (object o, EventArgs args)
	{
		entry1.Text = Util.MakeValidSQL(entry1.Text);

		if(entry1.Text.ToString().Length > 0)
			image_name.Hide();
		else {
			image_name.Show();
		}

		if(radio_metric.Active && (double) spinbutton_weight_metric.Value > 0)
			image_weight.Hide();
		else if(! radio_metric.Active && (double) spinbutton_weight_imperial.Value > 0)
			image_weight.Hide();
		else {
			image_weight.Show();
		}
	
		/*		
		if(dateTime != DateTime.MinValue)
			image_date.Hide();
		else {
			image_date.Show();
			allOk = false;
		}
		*/

		//countries is not required to create a person here, but will be required for server
		//&& 
		//UtilGtk.ComboGetActive(combo_continents) != Catalog.GetString(Constants.ContinentUndefined) &&
		//UtilGtk.ComboGetActive(combo_countries) != Catalog.GetString(Constants.CountryUndefined)
			
		/*
		if(allOk)
			button_accept.Sensitive = true;
		else
			button_accept.Sensitive = false;
		*/
		/*
		Always true because there's problems detecting the spinbutton change (when inserting data directly on entry)
		and there's an error message after if there's missing data	
		*/
		button_accept.Sensitive = true;
	}

	void on_radiobutton_sex_u_toggled (object o, EventArgs args)
	{
		sex = Constants.SexU;
	}
	
	void on_radiobutton_sex_m_toggled (object o, EventArgs args)
	{
		sex = Constants.SexM;
	}
	
	void on_radiobutton_sex_f_toggled (object o, EventArgs args)
	{
		sex = Constants.SexF;
	}

	static public PersonAddModifyWindow Show (Gtk.Window parent, 
			Session mySession, Person currentPerson, int pDN, 
			//Gtk.CheckButton app1_checkbutton_video, bool showCapturePhoto,
			//Gtk.CheckButton app1_checkbutton_video_contacts,
			string videoDevice, string videoDevicePixelFormat, string videoDeviceResolution, string videoDeviceFramerate,
			bool compujump, bool metric)
	{
		if (PersonAddModifyWindowBox == null) {
			//PersonAddModifyWindowBox = new PersonAddModifyWindow (parent, mySession, currentPerson, showCapturePhoto);
			PersonAddModifyWindowBox = new PersonAddModifyWindow (parent, mySession, currentPerson);
		}

		PersonAddModifyWindowBox.pDN = pDN;
		///PersonAddModifyWindowBox.app1_checkbutton_video_contacts = app1_checkbutton_video_contacts;
		PersonAddModifyWindowBox.videoDevice = videoDevice;
		PersonAddModifyWindowBox.videoDevicePixelFormat = videoDevicePixelFormat;
		PersonAddModifyWindowBox.videoDeviceResolution = videoDeviceResolution;
		PersonAddModifyWindowBox.videoDeviceFramerate = videoDeviceFramerate;
		//do not allow camera on compujump
		PersonAddModifyWindowBox.hbox_camera.Visible = ! compujump;

		PersonAddModifyWindowBox.person_win.Show ();

		PersonAddModifyWindowBox.fillDialog (metric);
		
		return PersonAddModifyWindowBox;
	}
	
	static public void MakeVisible () {
		PersonAddModifyWindowBox.person_win.Show();
	}


	private void createComboSports() {
		combo_sports = new ComboBoxText ();
		sports = SqliteSport.SelectAll();
			
		//create sports translated, only with translated stuff
		sportsTranslated = new String[sports.Length];
		int i = 0;
		foreach(string row in sports) {
			string [] myStrFull = row.Split(new char[] {':'});
			sportsTranslated[i++] = myStrFull[2];
			}
		
		//sort array (except second row)
		System.Array.Sort(sportsTranslated, 2, sportsTranslated.Length-2);
		
		UtilGtk.ComboUpdate(combo_sports, sportsTranslated, "");
		combo_sports.Active = UtilGtk.ComboMakeActive(sportsTranslated, 
				Catalog.GetString(Constants.SportUndefined));
	
		combo_sports.Changed += new EventHandler (on_combo_sports_changed);

		hbox_combo_sports.PackStart(combo_sports, true, true, 0);
		hbox_combo_sports.ShowAll();
		combo_sports.Sensitive = true;
	}
	
	private void createComboSpeciallities(int sportID) {
		combo_speciallities = new ComboBoxText ();
		speciallities = SqliteSpeciallity.SelectAll(true, sportID); //show undefined, filter by sport
		
		//create speciallities translated, only with translated stuff
		speciallitiesTranslated = new String[speciallities.Length];
		int i = 0;
		foreach(string row in speciallities) {
			string [] myStrFull = row.Split(new char[] {':'});
			speciallitiesTranslated[i++] = myStrFull[2];
			}
		
		//sort array (except first row)
		System.Array.Sort(speciallities, 1, speciallities.Length-1);

		UtilGtk.ComboUpdate(combo_speciallities, speciallitiesTranslated, "");
		combo_speciallities.Active = UtilGtk.ComboMakeActive(speciallitiesTranslated, 
				Catalog.GetString(Constants.SpeciallityUndefined));

		combo_speciallities.Changed += new EventHandler (on_combo_speciallities_changed);

		hbox_combo_speciallities.PackStart(combo_speciallities, true, true, 0);
		hbox_combo_speciallities.ShowAll();
		combo_speciallities.Sensitive = true;
	}
	
	private void createComboLevels() {
		combo_levels = new ComboBoxText ();
		levels = Constants.LevelsStr();
		
		UtilGtk.ComboUpdate(combo_levels, levels, "");
		combo_levels.Active = UtilGtk.ComboMakeActive(levels, 
				Constants.LevelUndefinedID.ToString() + ":" + 
				Catalog.GetString(Constants.LevelUndefined));

		combo_levels.Changed += new EventHandler (on_combo_levels_changed);

		hbox_combo_levels.PackStart(combo_levels, true, true, 0);
		hbox_combo_levels.ShowAll();
		combo_levels.Sensitive = false; //level is shown when sport is not "undefined" and not "none"
	}
		
	private void createComboContinents() {
		combo_continents = new ComboBoxText ();
		continents = Constants.ContinentsStr();

		//create continentsTranslated, only with translated stuff
		continentsTranslated = new String[Constants.ContinentsStr().Length];
		int i = 0;
		foreach(string continent in continents) 
			continentsTranslated[i++] = Util.FetchName(continent);

		UtilGtk.ComboUpdate(combo_continents, continentsTranslated, "");
		combo_continents.Active = UtilGtk.ComboMakeActive(continentsTranslated, 
				Catalog.GetString(Constants.ContinentUndefined));

		combo_continents.Changed += new EventHandler (on_combo_continents_changed);

		hbox_combo_continents.PackStart(combo_continents, true, true, 0);
		hbox_combo_continents.ShowAll();
		combo_continents.Sensitive = true;
	}

	private void createComboCountries() {
		combo_countries = new ComboBoxText ();

		countries = new String[1];
		//record countries with id:english name:translatedName
		countries [0] = Constants.CountryUndefinedID + ":" + Constants.CountryUndefined + ":" + Catalog.GetString(Constants.CountryUndefined);

		string [] myCountries = new String[1];
		myCountries [0] = Catalog.GetString(Constants.CountryUndefined);
		UtilGtk.ComboUpdate(combo_countries, myCountries, "");
		combo_countries.Active = UtilGtk.ComboMakeActive(myCountries, 
				Catalog.GetString(Constants.CountryUndefined));
		
		//create countriesTranslated, only with translated stuff
		countriesTranslated = new String[1];

		
		combo_countries.Changed += new EventHandler (on_combo_countries_changed);

		hbox_combo_countries.PackStart(combo_countries, true, true, 0);
		hbox_combo_countries.ShowAll();
		combo_countries.Sensitive = false;
	}

	private void fillDialog (bool metric)
	{
		int mySportID;
		int mySpeciallityID;
		int myLevelID;
		if(adding) {
			//now dateTime is undefined until user changes it
			dateTime = DateTime.MinValue;
			label_date.Text = Catalog.GetString("Undefined");

			mySportID = currentSession.PersonsSportID;
			mySpeciallityID = currentSession.PersonsSpeciallityID;
			myLevelID = currentSession.PersonsPractice;
		} else {
			if(metric) {
				hbox_weight_metric.Visible = true;
				hbox_weight_imperial.Visible = false;
				hbox_height_metric.Visible = true;
				hbox_height_imperial.Visible = false;
				hbox_leg_length_metric.Visible = true;
				hbox_leg_length_imperial.Visible = false;
				hbox_trochanter_floor_on_flexion_metric.Visible = true;
				hbox_trochanter_floor_on_flexion_imperial.Visible = false;

				radio_metric.Active = true;
			}
			else {
				hbox_weight_metric.Visible = false;
				hbox_weight_imperial.Visible = true;
				hbox_height_metric.Visible = false;
				hbox_height_imperial.Visible = true;
				hbox_leg_length_metric.Visible = false;
				hbox_leg_length_imperial.Visible = true;
				hbox_trochanter_floor_on_flexion_metric.Visible = false;
				hbox_trochanter_floor_on_flexion_imperial.Visible = true;

				radio_imperial.Active = true;
			}

			//PERSON STUFF
			entry1.Text = currentPerson.Name;
			entry_club_id.Text = currentPerson.Future2;
			if (currentPerson.Sex == Constants.SexU)
				radiobutton_sex_u.Active = true;
			else if (currentPerson.Sex == Constants.SexM)
				radiobutton_sex_m.Active = true;
			else 	//(currentPerson.Sex == Constants.SexF)
				radiobutton_sex_f.Active = true;

			dateTime = currentPerson.DateBorn;
			if(dateTime == DateTime.MinValue)
				label_date.Text = Catalog.GetString("Undefined");
			else
				label_date.Text = dateTime.ToLongDateString();

			//country stuff
			if(currentPerson.CountryID != Constants.CountryUndefinedID) {
				string [] countryString = SqliteCountry.Select(currentPerson.CountryID);
			
				combo_continents.Active = UtilGtk.ComboMakeActive(continentsTranslated, 
						Catalog.GetString(countryString[3]));
				
				combo_countries.Active = UtilGtk.ComboMakeActive(countriesTranslated, 
						Catalog.GetString(countryString[1]));
			}

			TextBuffer tb1 = new TextBuffer (new TextTagTable());
			tb1.Text = currentPerson.Description;
			textview_description.Buffer = tb1;
			
			serverUniqueID = currentPerson.ServerUniqueID;
			

			//PERSONSESSION STUFF
			PersonSession myPS = SqlitePersonSession.Select(currentPerson.UniqueID, currentSession.UniqueID);

			if(metric) {
				spinbutton_weight_metric.Value = myPS.Weight;
				spinbutton_height_metric.Value = myPS.Height;
				spinbutton_leg_length_metric.Value = myPS.TrochanterToe; //future1: altura trochanter - punta del pie en extension
				spinbutton_trochanter_floor_on_flexion_metric.Value = myPS.TrochanterFloorOnFlexion; //future2: altura trochanter - suelo en flexión
			} else {
				spinbutton_weight_imperial.Value = Util.ConvertKgToPounds(myPS.Weight);

				int feet = 0;
				double inches = 0;
				Util.ConvertCmToFeetInches (myPS.Height, out feet, out inches);
				spinbutton_height_imperial_feet.Value = feet;
				spinbutton_height_imperial_inches.Value = inches;

				Util.ConvertCmToFeetInches (myPS.TrochanterToe, out feet, out inches);
				spinbutton_leg_length_imperial_feet.Value = feet;
				spinbutton_leg_length_imperial_inches.Value = inches;

				Util.ConvertCmToFeetInches (myPS.TrochanterFloorOnFlexion, out feet, out inches);
				spinbutton_trochanter_floor_on_flexion_imperial_feet.Value = feet;
				spinbutton_trochanter_floor_on_flexion_imperial_inches.Value = inches;
			}


			weightIniMetric = myPS.Weight; //store for tracking if changes
		
			mySportID = myPS.SportID;
			mySpeciallityID = myPS.SpeciallityID;
			myLevelID = myPS.Practice;

			TextBuffer tb2 = new TextBuffer (new TextTagTable());
			tb2.Text = myPS.Comments;
			textview_ps_comments.Buffer = tb2;
		}

		textview_description.Buffer.Changed += new EventHandler(descriptionChanged);
		descriptionChanging = false;
		textview_ps_comments.Buffer.Changed += new EventHandler(textviewpsChanged);
		textviewpsChanging = false;

		sport = SqliteSport.Select(false, mySportID);
		combo_sports.Active = UtilGtk.ComboMakeActive(sportsTranslated, sport.ToString());

		combo_speciallities.Active = UtilGtk.ComboMakeActive(speciallitiesTranslated, SqliteSpeciallity.Select(false, mySpeciallityID));

		combo_levels.Active = UtilGtk.ComboMakeActive(levels, myLevelID + ":" + Util.FindLevelName(myLevelID));
		
	}

	private void descriptionChanged(object o,EventArgs args)
	{
		if(descriptionChanging)
			return;

		descriptionChanging = true;

		TextBuffer tb = o as TextBuffer;
		if (o == null)
			return;

		tb.Text = Util.MakeValidSQL(tb.Text);
		descriptionChanging = false;
	}
	private void textviewpsChanged(object o,EventArgs args)
	{
		if(textviewpsChanging)
			return;

		textviewpsChanging = true;

		TextBuffer tb = o as TextBuffer;
		if (o == null)
			return;

		tb.Text = Util.MakeValidSQL(tb.Text);
		textviewpsChanging = false;
	}


	void on_button_calendar_clicked (object o, EventArgs args)
	{
		DateTime dt = dateTime;
		if(dt == DateTime.MinValue)
			dt = DateTime.Now;
		myDialogCalendar = new DialogCalendar(Catalog.GetString("Select session date"), dt);
		myDialogCalendar.FakeButtonDateChanged.Clicked += new EventHandler(on_calendar_changed);
	}

	void on_calendar_changed (object obj, EventArgs args)
	{
		dateTime = myDialogCalendar.MyDateTime;
		label_date.Text = dateTime.ToLongDateString();
		on_entries_required_changed(new object(), new EventArgs());
	}

	private void on_combo_sports_changed(object o, EventArgs args) {
		if (o == null)
			return;

		//LogB.Information("changed");
		try {
			int sportID = Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_sports), sports));
			sport = SqliteSport.Select(false, sportID);

			if(Catalog.GetString(sport.Name) == Catalog.GetString(Constants.SportUndefined)) {
				//if sport is undefined, level should be undefined, and unsensitive
				try { 
					combo_levels.Active = UtilGtk.ComboMakeActive(levels, 
							Constants.LevelUndefinedID.ToString() + ":" + 
							Catalog.GetString(Constants.LevelUndefined));
					combo_levels.Sensitive = false;
					combo_speciallities.Active = UtilGtk.ComboMakeActive(speciallitiesTranslated, 
							Catalog.GetString(Constants.SpeciallityUndefined));
					label_speciallity.Hide();
					combo_speciallities.Hide();
				}
				catch { LogB.Warning("do later"); }
			} else if(Catalog.GetString(sport.Name) == Catalog.GetString(Constants.SportNone)) {
				//if sport is none, level should be sedentary and unsensitive
				try { 
					combo_levels.Active = UtilGtk.ComboMakeActive(levels, 
							Constants.LevelSedentaryID.ToString() + ":" + 
							Catalog.GetString(Constants.LevelSedentary));
					combo_levels.Sensitive = false;

					combo_speciallities.Active = UtilGtk.ComboMakeActive(speciallitiesTranslated, 
							Catalog.GetString(Constants.SpeciallityUndefined));

					label_speciallity.Hide();
					combo_speciallities.Hide();
				}
				catch { LogB.Warning("do later"); }
			} else {
				//sport is not undefined and not none

				//if level is "sedentary", then change level to "undefined"
				if(UtilGtk.ComboGetActive(combo_levels) ==
						Constants.LevelSedentaryID.ToString() + ":" + 
					       	Catalog.GetString(Constants.LevelSedentary)) {
					combo_levels.Active = UtilGtk.ComboMakeActive(levels,
							Constants.LevelUndefinedID.ToString() + ":" + 
						       	Catalog.GetString(Constants.LevelUndefined));
				}

				//show level
				combo_levels.Sensitive = true;
		
				if(sport.HasSpeciallities) {
					combo_speciallities.Destroy();
					createComboSpeciallities(sport.UniqueID);
					label_speciallity.Show();
					combo_speciallities.Show();
				} else {
					LogB.Information("hide");
					combo_speciallities.Active = UtilGtk.ComboMakeActive(speciallitiesTranslated,
						       	Catalog.GetString(Constants.SpeciallityUndefined));
					label_speciallity.Hide();
					combo_speciallities.Hide();
				}
			}
		} catch { 
			//LogB.Warning("do later");
		}

		on_entries_required_changed(new object(), new EventArgs());
		LogB.Information(sport.ToString());
	}
	
	private void on_combo_speciallities_changed(object o, EventArgs args) {
		LogB.Information("changed speciallities");
		on_entries_required_changed(new object(), new EventArgs());
	}

	private void on_combo_levels_changed(object o, EventArgs args) {
		//string myText = UtilGtk.ComboGetActive(combo_sports);
		LogB.Information("changed levels");
		on_entries_required_changed(new object(), new EventArgs());
		//level = UtilGtk.ComboGetActive(combo_levels);
				
		//if it's sedentary, put sport to none
		/*
		 * Now undone because sedentary has renamed to "sedentary/Occasional practice"
		if(UtilGtk.ComboGetActive(combo_levels) == "0:" + Catalog.GetString(Constants.LevelSedentary))
			combo_sports.Active = UtilGtk.ComboMakeActive(sports, "2:" + Catalog.GetString(Constants.SportNone));
		*/
	}
	
	private void on_combo_continents_changed(object o, EventArgs args) {
		//LogB.Information("Changed");

		if(UtilGtk.ComboGetActive(combo_continents) == Catalog.GetString(Constants.ContinentUndefined)) {
			countries [0] = Constants.CountryUndefinedID + ":" + 
				Constants.CountryUndefined + ":" + Catalog.GetString(Constants.CountryUndefined);
			countriesTranslated = new String[1];
			countriesTranslated [0] = Catalog.GetString(Constants.CountryUndefined);
			combo_countries.Sensitive = false;
		}
		else {
			//get the active continent
			string continentEnglish = Util.FindOnArray(':', 1, 0, UtilGtk.ComboGetActive(combo_continents), continents); 
			countries = SqliteCountry.SelectCountriesOfAContinent(continentEnglish, true); //put undefined first

			//create countries translated, only with translated stuff
			countriesTranslated = new String[countries.Length];
			int i = 0;
			foreach(string row in countries) {
				string [] myStrFull = row.Split(new char[] {':'});
				countriesTranslated[i++] = myStrFull[2];
			}
		}
		//sort array (except first row)
		System.Array.Sort(countriesTranslated, 1, countriesTranslated.Length-1);

		UtilGtk.ComboUpdate(combo_countries, countriesTranslated, "");
		combo_countries.Active = UtilGtk.ComboMakeActive(countriesTranslated, 
				Catalog.GetString(Constants.CountryUndefined));

		combo_countries.Sensitive = true;

		on_entries_required_changed(new object(), new EventArgs());
	}
	
	private void on_combo_countries_changed(object o, EventArgs args) {
		//define country is not needed to accept person
		//on_entries_required_changed(new object(), new EventArgs());
	}
	
	void on_button_sport_add_clicked (object o, EventArgs args)
	{
		LogB.Information("sport add clicked");
		genericWin = GenericWindow.Show(Catalog.GetString("Add sport"),
				Catalog.GetString("Add new sport to database"),
				Constants.GenericWindowShow.ENTRY, true);
		genericWin.Button_accept.Clicked += new EventHandler(on_sport_add_accepted);
	}

	private void on_sport_add_accepted (object o, EventArgs args)
	{
		genericWin.Button_accept.Clicked -= new EventHandler(on_sport_add_accepted);

		string newSportName = genericWin.EntrySelected;
		if(Sqlite.Exists(false, Constants.SportTable, newSportName) ||
				newSportName == Catalog.GetString(Constants.SportUndefined) || //let's save problems
				newSportName == Catalog.GetString(Constants.SportNone)		//let's save problems
				)
				new DialogMessage(Constants.MessageTypes.WARNING, string.Format(
							Catalog.GetString("Sorry, this sport '{0}' already exists in database"), 
							newSportName));
		else {
			int myID = SqliteSport.Insert(false, "-1", newSportName, true, //dbconOpened, , userDefined
					false, "");	//hasSpeciallities, graphLink 

			Sport mySport = new Sport(myID, newSportName, true, 
					false, "");	//hasSpeciallities, graphLink 
			sports = SqliteSport.SelectAll();
			//create sports translated, only with translated stuff
			sportsTranslated = new String[sports.Length];
			int i = 0;
			foreach(string row in sports) {
				string [] myStrFull = row.Split(new char[] {':'});
				sportsTranslated[i++] = myStrFull[2];
				}
		
			//sort array (except second row)
			System.Array.Sort(sportsTranslated, 2, sportsTranslated.Length-2);

			UtilGtk.ComboUpdate(combo_sports, sportsTranslated, mySport.ToString());
			combo_sports.Active = UtilGtk.ComboMakeActive(sportsTranslated, mySport.ToString());
			//on_combo_sports_changed(combo_sports, new EventArgs());
		}
	}
		

	Person loadingPerson;

	void on_button_accept_clicked (object o, EventArgs args)
	{
		string errorMessage = "";

		//Check if person name exists and weight is > 0
		string personName = Util.MakeValidSQLAndFileName(entry1.Text);

		if(personName == "")
			errorMessage += "\n" + Catalog.GetString("Please, write the name of the person.");

		if(radio_metric.Active && (double) spinbutton_weight_metric.Value == 0)
			errorMessage += "\n" + Catalog.GetString("Please, complete the weight of the person.");
		if(! radio_metric.Active && (double) spinbutton_weight_imperial.Value == 0)
			errorMessage += "\n" + Catalog.GetString("Please, complete the weight of the person.");

		if(errorMessage.Length > 0)
		{
			label_error.Text = errorMessage;
			showErrorMessage(true, false);
			return;
		}

		bool personExists;
		bool personLoadable = false; // this person exists but is not in this session
		if(adding)
		{
			personExists = Sqlite.Exists (false, Constants.PersonTable, personName);
			if(personExists)
			{
				Person p = SqlitePerson.Select(false, "WHERE LOWER(name) = LOWER(\"" + personName + "\")");
				if(p.UniqueID != -1)
				{
					personLoadable = ! SqlitePersonSession.PersonSelectExistsInSession(
							false, p.UniqueID, currentSession.UniqueID);
					loadingPerson = p;
				}
			}
		}
		else
			personExists = SqlitePerson.ExistsAndItsNotMe (currentPerson.UniqueID, personName);

		if(personExists)
		{
			errorMessage += string.Format(Catalog.GetString("Person: '{0}' exists. Please, use another name."),
					Util.RemoveTildeAndColonAndDot(personName) );
			if(adding && personLoadable)
				errorMessage += "\n" + Catalog.GetString("Or load this person from another session using this button:");
		}
		else {
			//if weight has changed
			if(! adding && (
						(radio_metric.Active && (double) spinbutton_weight_metric.Value != weightIniMetric) ||
						(! radio_metric.Active && Util.ConvertPoundsToKg(spinbutton_weight_imperial.Value) != weightIniMetric)
				      ) ) {
				//see if this person has done jumps with weight
				string [] myJumpsSimple = SqliteJump.SelectJumpsSA (false, currentSession.UniqueID, currentPerson.UniqueID, "withWeight", "",
						Sqlite.Orders_by.DEFAULT, 0);
				string [] myJumpsReactive = SqliteJumpRj.SelectJumpsSA (false, currentSession.UniqueID, currentPerson.UniqueID, "withWeight", "");

				if(myJumpsSimple.Length > 0 || myJumpsReactive.Length > 0) {
					//create the convertWeight Window
					if(radio_metric.Active)
						convertWeightWin = ConvertWeightWindow.Show(
								weightIniMetric, (double) spinbutton_weight_metric.Value,
								myJumpsSimple, myJumpsReactive);
					else
						convertWeightWin = ConvertWeightWindow.Show(
								weightIniMetric, Util.ConvertPoundsToKg(spinbutton_weight_imperial.Value),
								myJumpsSimple, myJumpsReactive);
					convertWeightWin.Button_accept.Clicked += new EventHandler(on_convertWeightWin_accepted);
					convertWeightWin.Button_cancel.Clicked += new EventHandler(on_convertWeightWin_cancelled);
				} else 
					recordChanges();
				
			} else 
				recordChanges();
			
		}

		if(errorMessage.Length > 0)
		{
			label_error.Text = errorMessage;
			//show error message, show button_load_person if adding, personExists and not in this session
			showErrorMessage(true, adding && personExists && personLoadable);
		}
	}

	private void showErrorMessage(bool show, bool showLoadPerson)
	{
		vbox_error.Visible = show;
		frame_main.Visible = ! show;
		hbox_units.Visible = ! show;
		hbuttonbox_main.Visible = ! show;

		button_load_person.Visible = showLoadPerson;
	}

	private void on_button_load_person_clicked (object o, EventArgs args)
	{
		currentPerson = loadingPerson;
		PersonAddModifyWindowBox.person_win.Hide();
		PersonAddModifyWindowBox = null;

		PersonSession myPS = SqlitePersonSession.Select(false, currentPerson.UniqueID, -1);

		LogB.Information("Going to insert personSession");
		//this inserts in DB
		currentPersonSession = new PersonSession (
				currentPerson.UniqueID, currentSession.UniqueID,
				myPS.Height, myPS.Weight,
				myPS.SportID, myPS.SpeciallityID,
				myPS.Practice,
				myPS.Comments,
				myPS.TrochanterToe,
				myPS.TrochanterFloorOnFlexion,
				false); //dbconOpened

		fakeButtonAccept.Click();
	}

	private void on_button_error_go_back_clicked (object o, EventArgs args)
	{
		showErrorMessage(false, false);
	}

	void on_convertWeightWin_accepted (object o, EventArgs args) {
		recordChanges();
	}

	void on_convertWeightWin_cancelled (object o, EventArgs args) {
		//do nothing (wait if user whants to cancel the personModify o change another thing)
	}

	private void recordChanges() {
		//separate by '/' for not confusing with the ':' separation between the other values
		//string dateFull = dateTime.Day.ToString() + "/" + dateTime.Month.ToString() + "/" +
		//	dateTime.Year.ToString();
		
		double weight = 0;
		double height = 0;
		double legLength = 0;
		double trochanterFloorOnFlexion;
		if(radio_metric.Active) {
			weight = (double) spinbutton_weight_metric.Value;
			height = (double) spinbutton_height_metric.Value;
			legLength = (double) spinbutton_leg_length_metric.Value;
			trochanterFloorOnFlexion = (double) spinbutton_trochanter_floor_on_flexion_metric.Value;
		} else {
			weight = Util.ConvertPoundsToKg(spinbutton_weight_imperial.Value);
			height = Util.ConvertFeetInchesToCm(
					Convert.ToInt32(spinbutton_height_imperial_feet.Value),
					spinbutton_height_imperial_inches.Value);
			legLength = Util.ConvertFeetInchesToCm(
					Convert.ToInt32(spinbutton_leg_length_imperial_feet.Value),
					spinbutton_leg_length_imperial_inches.Value);
			trochanterFloorOnFlexion = Util.ConvertFeetInchesToCm(
					Convert.ToInt32(spinbutton_trochanter_floor_on_flexion_imperial_feet.Value),
					spinbutton_trochanter_floor_on_flexion_imperial_inches.Value);
		}

		//convert margarias (it's power is calculated using weight and it's written on description)
		string [] myMargarias = SqliteRun.SelectRunsSA (false, currentSession.UniqueID, currentPerson.UniqueID, "Margaria",
				Sqlite.Orders_by.DEFAULT, -1);

		foreach(string myStr in myMargarias) {
			string [] margaria = myStr.Split(new char[] {':'});
			Run mRun = SqliteRun.SelectRunData(Convert.ToInt32(margaria[1]), false);
			double distanceMeters = mRun.Distance / 1000;
			mRun.Description = "P = " + Util.TrimDecimals ( (weight * 9.8 * distanceMeters / mRun.Time).ToString(), pDN) + " (Watts)";
			SqliteRun.Update(mRun.UniqueID, mRun.Type, mRun.Distance, mRun.Time.ToString(), mRun.PersonID, mRun.Description);
		}

		string personName = Util.MakeValidSQLAndFileName(entry1.Text);
		string clubID = Util.MakeValidSQL(entry_club_id.Text);

		if(adding) {
			//here we add rows in the database
			LogB.Information("Going to insert person");
			currentPerson = new Person (personName, sex, dateTime,
					Constants.RaceUndefinedID,
					Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_countries), countries)),
					textview_description.Buffer.Text,
					"", clubID, //future1: rfid; future2: clubID
					Constants.ServerUndefinedID, "", false); //dbconOpened
					
			LogB.Information("Going to insert personSession");
			currentPersonSession = new PersonSession (
					currentPerson.UniqueID, currentSession.UniqueID, 
					height, (double) weight,
					sport.UniqueID, 
					Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_speciallities), speciallities)),
					Util.FetchID(UtilGtk.ComboGetActive(combo_levels)),
					textview_ps_comments.Buffer.Text,
					legLength,
					trochanterFloorOnFlexion,
					false); //dbconOpened
			LogB.Information("inserted both");

			//if we added photo while creating, filename is -1.png or -1.png, change name
			string photo = Util.UserPhotoURL(false, -1);
			if(photo != "")
			{
				if(UtilMultimedia.GetImageType(photo) == UtilMultimedia.ImageTypes.JPEG)
					File.Move(photo, Util.GetPhotoFileName(false, currentPerson.UniqueID));
				else if(UtilMultimedia.GetImageType(photo) == UtilMultimedia.ImageTypes.PNG)
					File.Move(photo, Util.GetPhotoPngFileName(false, currentPerson.UniqueID));
			}
			photo = Util.UserPhotoURL(true, -1);
			if(photo != "")
			{
				if(UtilMultimedia.GetImageType(photo) == UtilMultimedia.ImageTypes.JPEG)
					File.Move(photo, Util.GetPhotoFileName(true, currentPerson.UniqueID));
				else if(UtilMultimedia.GetImageType(photo) == UtilMultimedia.ImageTypes.PNG)
					File.Move(photo, Util.GetPhotoPngFileName(true, currentPerson.UniqueID));
			}

		} else {
			//here we update rows in the database
			currentPerson = new Person (currentPerson.UniqueID, personName, sex, dateTime,
					Constants.RaceUndefinedID,
					Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_countries), countries)),
					textview_description.Buffer.Text,
					"", clubID, //future1: rfid; future2: clubID
					serverUniqueID, currentPerson.LinkServerImage);
			SqlitePerson.Update (currentPerson); 
		
			//we only need to update personSession
			//1.- search uniqueID
			PersonSession ps = SqlitePersonSession.Select(currentPerson.UniqueID, currentSession.UniqueID);

			//2.- create new instance
			currentPersonSession = new PersonSession (
					ps.UniqueID,
					currentPerson.UniqueID, currentSession.UniqueID, 
					height, (double) weight,
					sport.UniqueID, 
					Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_speciallities), speciallities)),
					Util.FetchID(UtilGtk.ComboGetActive(combo_levels)),
					textview_ps_comments.Buffer.Text,
					legLength,
					trochanterFloorOnFlexion);

			//3.- update in database
			SqlitePersonSession.Update (false, currentPersonSession);
		}

		if(webcam != null && webcam.Running)
			webcam.RecordingStop();

		PersonAddModifyWindowBox.person_win.Hide();
		PersonAddModifyWindowBox = null;

		fakeButtonAccept.Click();
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		fakeButtonCancel.Click (); //managed if persons in top win

		if(webcam != null && webcam.Running)
			webcam.RecordingStop();

		PersonAddModifyWindowBox.person_win.Hide();
		PersonAddModifyWindowBox = null;
	}
	
	//void on_person_modify_delete_event (object o, EventArgs args)
	void on_person_win_delete_event (object o, DeleteEventArgs args)
	{
		fakeButtonCancel.Click (); //managed if persons in top win

		if(webcam != null && webcam.Running)
			webcam.RecordingStop();

		PersonAddModifyWindowBox.person_win.Hide();
		PersonAddModifyWindowBox = null;
	}

	public void Destroy() {
		//PersonAddModifyWindowBox.person_win.Destroy();
	}

	public Button FakeButtonAccept 
	{
		set { fakeButtonAccept = value; }
		get { return fakeButtonAccept; }
	}
	
	public Button FakeButtonCancel
	{
		get { return fakeButtonCancel; }
	}

	public Person CurrentPerson {
		get { return currentPerson; }
	}
	
	public PersonSession CurrentPersonSession {
		get { return currentPersonSession; }
	}

	public Preferences.UnitsEnum Units {
		get {
			if(radio_metric.Active)
				return Preferences.UnitsEnum.METRIC;
			else
				return Preferences.UnitsEnum.IMPERIAL;
		}
	}
	
	private void connectWidgets (Gtk.Builder builder)
	{
		person_win = (Gtk.Window) builder.GetObject ("person_win");
		radio_metric = (Gtk.RadioButton) builder.GetObject ("radio_metric");
		radio_imperial = (Gtk.RadioButton) builder.GetObject ("radio_imperial");
		entry1 = (Gtk.Entry) builder.GetObject ("entry1");
		radiobutton_sex_u = (Gtk.RadioButton) builder.GetObject ("radiobutton_sex_u");
		radiobutton_sex_m = (Gtk.RadioButton) builder.GetObject ("radiobutton_sex_m");
		radiobutton_sex_f = (Gtk.RadioButton) builder.GetObject ("radiobutton_sex_f");
		entry_club_id = (Gtk.Entry) builder.GetObject ("entry_club_id");
		textview_description = (Gtk.TextView) builder.GetObject ("textview_description");
		textview_ps_comments = (Gtk.TextView) builder.GetObject ("textview_ps_comments");

		frame_main = (Gtk.Frame) builder.GetObject ("frame_main");
		notebook_main = (Gtk.Notebook) builder.GetObject ("notebook_main");
		hbuttonbox_main = (Gtk.HButtonBox) builder.GetObject ("hbuttonbox_main");
		hbox_units = (Gtk.HBox) builder.GetObject ("hbox_units");
		vbox_error = (Gtk.VBox) builder.GetObject ("vbox_error");
		label_error = (Gtk.Label) builder.GetObject ("label_error");
		button_load_person = (Gtk.Button) builder.GetObject ("button_load_person");
		image_load_person = (Gtk.Image) builder.GetObject ("image_load_person");

		image_photo_from_file = (Gtk.Image) builder.GetObject ("image_photo_from_file");
		image_photo_preview = (Gtk.Image) builder.GetObject ("image_photo_preview");
		image_photo_do = (Gtk.Image) builder.GetObject ("image_photo_do");

		button_delete_photo_file = (Gtk.Button) builder.GetObject ("button_delete_photo_file");
		image_photo_delete = (Gtk.Image) builder.GetObject ("image_photo_delete");

		//button_add_photo_file = (Gtk.Button) builder.GetObject ("button_add_photo_file");
		//button_take_photo_do = (Gtk.Button) builder.GetObject ("button_take_photo_do");
		hbox_camera = (Gtk.HBox) builder.GetObject ("hbox_camera");

		label_date = (Gtk.Label) builder.GetObject ("label_date");
		//button_change_date = (Gtk.Button) builder.GetObject ("button_change_date");
		image_calendar = (Gtk.Image) builder.GetObject ("image_calendar");

		hbox_weight_metric = (Gtk.HBox) builder.GetObject ("hbox_weight_metric");
		hbox_weight_imperial = (Gtk.HBox) builder.GetObject ("hbox_weight_imperial");
		spinbutton_weight_metric = (Gtk.SpinButton) builder.GetObject ("spinbutton_weight_metric");
		spinbutton_weight_imperial = (Gtk.SpinButton) builder.GetObject ("spinbutton_weight_imperial");

		hbox_height_metric = (Gtk.HBox) builder.GetObject ("hbox_height_metric");
		hbox_height_imperial = (Gtk.HBox) builder.GetObject ("hbox_height_imperial");
		spinbutton_height_metric = (Gtk.SpinButton) builder.GetObject ("spinbutton_height_metric");
		spinbutton_height_imperial_feet = (Gtk.SpinButton) builder.GetObject ("spinbutton_height_imperial_feet");
		spinbutton_height_imperial_inches = (Gtk.SpinButton) builder.GetObject ("spinbutton_height_imperial_inches");

		hbox_leg_length_metric = (Gtk.HBox) builder.GetObject ("hbox_leg_length_metric");
		hbox_leg_length_imperial = (Gtk.HBox) builder.GetObject ("hbox_leg_length_imperial");
		spinbutton_leg_length_metric = (Gtk.SpinButton) builder.GetObject ("spinbutton_leg_length_metric");
		spinbutton_leg_length_imperial_feet = (Gtk.SpinButton) builder.GetObject ("spinbutton_leg_length_imperial_feet");
		spinbutton_leg_length_imperial_inches = (Gtk.SpinButton) builder.GetObject ("spinbutton_leg_length_imperial_inches");

		hbox_trochanter_floor_on_flexion_metric = (Gtk.HBox) builder.GetObject ("hbox_trochanter_floor_on_flexion_metric");
		hbox_trochanter_floor_on_flexion_imperial = (Gtk.HBox) builder.GetObject ("hbox_trochanter_floor_on_flexion_imperial");
		spinbutton_trochanter_floor_on_flexion_metric = (Gtk.SpinButton) builder.GetObject ("spinbutton_trochanter_floor_on_flexion_metric");
		spinbutton_trochanter_floor_on_flexion_imperial_feet = (Gtk.SpinButton) builder.GetObject ("spinbutton_trochanter_floor_on_flexion_imperial_feet");
		spinbutton_trochanter_floor_on_flexion_imperial_inches = (Gtk.SpinButton) builder.GetObject ("spinbutton_trochanter_floor_on_flexion_imperial_inches");

		hbox_combo_sports = (Gtk.Box) builder.GetObject ("hbox_combo_sports");
		label_speciallity = (Gtk.Label) builder.GetObject ("label_speciallity");
		hbox_combo_speciallities = (Gtk.Box) builder.GetObject ("hbox_combo_speciallities");
		hbox_combo_levels = (Gtk.Box) builder.GetObject ("hbox_combo_levels");

		hbox_combo_continents = (Gtk.Box) builder.GetObject ("hbox_combo_continents");
		hbox_combo_countries = (Gtk.Box) builder.GetObject ("hbox_combo_countries");

		image_name = (Gtk.Image) builder.GetObject ("image_name");
		image_weight = (Gtk.Image) builder.GetObject ("image_weight");

		button_zoom = (Gtk.Button) builder.GetObject ("button_zoom");
		image_photo_mini = (Gtk.Image) builder.GetObject ("image_photo_mini");
		image_zoom = (Gtk.Image) builder.GetObject ("image_zoom");

		button_accept = (Gtk.Button) builder.GetObject ("button_accept");
	}
}


