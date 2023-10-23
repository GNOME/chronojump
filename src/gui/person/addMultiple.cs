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
using System.Collections; //ArrayList
using System.Collections.Generic; //List
using Mono.Unix;

public class PersonAddMultipleTable
{
	public string name;
	public string sex;
	public double weight;
	public double height;
	public double legsLength;
	public double hipsHeight;

	public PersonAddMultipleTable (string name, string sex, double weight,
			double height, double legsLength, double hipsHeight)
	{
		this.name = name;
		this.sex = sex;
		this.weight = weight;
		this.height = height;
		this.legsLength = legsLength;
		this.hipsHeight = hipsHeight;
	}

	~PersonAddMultipleTable() {}
}


// on current implementation PersonAddMultipleWindow, an id will have just 1 ErrorType
public class PersonAddMultipleError
{
	public enum ErrorType { INSESSION, INDB, REPEATEDNAME, NOWEIGHT }

	public int id; //starts at 0
	public string nameOptional; //needed at repeated
	public ErrorType errorType;

	public PersonAddMultipleError (int id, string nameOptional, ErrorType errorType)
	{
		this.id = id;
		this.nameOptional = nameOptional;
		this.errorType = errorType;
	}

	public static bool ExistErrorLike (List<PersonAddMultipleError> pame_l, string nameOptional, ErrorType errorType)
	{
		foreach (PersonAddMultipleError pame in pame_l)
			if (pame.errorType == errorType && pame.nameOptional == nameOptional)
				return true;

		return false;
	}
}

//new persons multiple (infinite)
public class PersonAddMultipleWindow
{
	Gtk.Window person_multiple_infinite;
		
	Gtk.Notebook notebook;
	Gtk.Button button_cancel_or_back;
	
	Gtk.RadioButton radio_csv;
	Gtk.RadioButton radio_manually;
	Gtk.Box hbox_manually;
	Gtk.SpinButton spin_manually;
	
	Gtk.Image image_csv_headers;
	Gtk.Image image_csv_noheaders;
	Gtk.Image image_load;
	Gtk.Label label_csv;
	Gtk.Label label_name;

	Gtk.Button button_csv_prepare;
	
	Gtk.Image image_name1;
	Gtk.Image image_name2;

	Gtk.CheckButton check_headers;
	Gtk.CheckButton check_fullname_1_col;
	Gtk.CheckButton check_person_height;
	Gtk.CheckButton check_legsLength;
	Gtk.CheckButton check_hipsHeight;

	Gtk.HBox hbox_h1_h2_help;
	Gtk.Label label_h1_legsLength;
	Gtk.Label label_h2_hipsHeight;

	//show/hide headers and make them bold
	Gtk.Label label_t_fullname;
	Gtk.Label label_t_name;
	Gtk.Label label_t_surname;
	Gtk.Label label_t_genre;
	Gtk.Label label_t_weight;
	Gtk.Label label_t_height;
	Gtk.Label label_t_legsLength;
	Gtk.Label label_t_hipsHeight;
	//show/hide hideable columns of June Carter
	Gtk.Label label_t_fullname_june;
	Gtk.Label label_t_name_june;
	Gtk.Label label_t_surname_june;
	Gtk.Label label_t_height_june;
	Gtk.Label label_t_legsLength_june;
	Gtk.Label label_t_hipsHeight_june;
	//show/hide hideable columns of Johnny Cash
	Gtk.Label label_t_fullname_johnny;
	Gtk.Label label_t_name_johnny;
	Gtk.Label label_t_surname_johnny;
	Gtk.Label label_t_height_johnny;
	Gtk.Label label_t_legsLength_johnny;
	Gtk.Label label_t_hipsHeight_johnny;

	Gtk.TextView textview;
	//Gtk.ScrolledWindow scrolledwindow;
	Gtk.Grid grid_main;
	Gtk.Label label_message;
	Gtk.Label label_columns_order;
	
	Gtk.Button button_accept;

	private enum notebookPages { MAINOPTIONS, TABLEMANUALLY, LOADCSV };

	//use this to read/write table
	ArrayList entries;
	ArrayList radiosU;
	ArrayList radiosM;
	ArrayList radiosF;
	ArrayList spinsWeight;
	ArrayList spinsHeight;
	ArrayList spinsLegsLength;
	ArrayList spinsHipsHeight;

	
	int rows;
	
	private Gtk.Button fakeButtonDone;
	private Gtk.Button fakeButtonCancel;
	
	static PersonAddMultipleWindow PersonAddMultipleWindowBox;

	private Person currentPerson;
	Session currentSession;
	char columnDelimiter;
	//int personsCreatedCount;


	PersonAddMultipleWindow (Gtk.Window parent, Session currentSession, char columnDelimiter)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "person_multiple_infinite.glade", "person_multiple_infinite", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "person_multiple_infinite.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);
		
		//put an icon to window
		UtilGtk.IconWindow(person_multiple_infinite);

		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.WindowColor(person_multiple_infinite, Config.ColorBackground);

			UtilGtk.WidgetColor (notebook, Config.ColorBackgroundShifted);

			UtilGtk.ContrastLabelsWidget (Config.ColorBackgroundShiftedIsDark, notebook);
		}
	
		fakeButtonDone = new Gtk.Button();
		fakeButtonCancel = new Gtk.Button();
		person_multiple_infinite.Parent = parent;
		this.currentSession = currentSession;
		this.columnDelimiter = columnDelimiter;
	}
	
	static public PersonAddMultipleWindow Show (Gtk.Window parent, Session currentSession, char columnDelimiter)
	{
		if (PersonAddMultipleWindowBox == null) {
			PersonAddMultipleWindowBox = new PersonAddMultipleWindow (parent, currentSession, columnDelimiter);
		}
		
		PersonAddMultipleWindowBox.putNonStandardIcons ();
		PersonAddMultipleWindowBox.tableHeaderBold ();
		PersonAddMultipleWindowBox.textviewUpdate ();
		PersonAddMultipleWindowBox.tableLabelsVisibility ();
		PersonAddMultipleWindowBox.button_cancel_or_back.Label = Catalog.GetString ("Cancel");

		PersonAddMultipleWindowBox.person_multiple_infinite.Show ();
		
		return PersonAddMultipleWindowBox;
	}
	
	void on_button_cancel_or_back_clicked (object o, EventArgs args)
	{
		if (notebook.CurrentPage == Convert.ToInt32 (notebookPages.MAINOPTIONS))
		{
			fakeButtonCancel.Click (); //managed if persons in top win

			PersonAddMultipleWindowBox.person_multiple_infinite.Hide();
			PersonAddMultipleWindowBox = null;
		} else {
			notebook.CurrentPage = Convert.ToInt32 (notebookPages.MAINOPTIONS);
			PersonAddMultipleWindowBox.button_cancel_or_back.Label = Catalog.GetString ("Cancel");
			PersonAddMultipleWindowBox.button_accept.Sensitive = false;
		}
	}
	
	void on_delete_event (object o, DeleteEventArgs args)
	{
		fakeButtonCancel.Click (); //managed if persons in top win

		PersonAddMultipleWindowBox.person_multiple_infinite.Hide();
		PersonAddMultipleWindowBox = null;
	}
		
	void putNonStandardIcons ()
	{
		Pixbuf pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameCSVHeadersIcon);
		image_csv_headers.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameCSVNoHeadersIcon);
		image_csv_noheaders.Pixbuf = pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameCSVName1Icon);
		image_name1.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameCSVName2Icon);
		image_name2.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "folder_open.png");
		image_load.Pixbuf = pixbuf;

		label_csv.Text = Catalog.GetString("CSV file has headers");
		label_name.Text = Catalog.GetString("Full name in one column");
	}
	
	void tableHeaderBold ()
	{
		label_t_fullname.Text = "<b>" + label_t_fullname.Text + "</b>";
		label_t_name.Text = "<b>" + label_t_name.Text + "</b>";
		label_t_surname.Text = "<b>" + label_t_surname.Text + "</b>";
		label_t_genre.Text = "<b>" + label_t_genre.Text + "</b>";
		label_t_weight.Text = "<b>" + label_t_weight.Text + "</b>";
		label_t_height.Text = "<b>" + label_t_height.Text + "</b>";
		label_t_legsLength.Text = "<b>" + label_t_legsLength.Text + "</b>";
		label_t_hipsHeight.Text = "<b>" + label_t_hipsHeight.Text + "</b>";

		label_t_fullname.UseMarkup = true;
		label_t_name.UseMarkup = true;
		label_t_surname.UseMarkup = true;
		label_t_genre.UseMarkup = true;
		label_t_weight.UseMarkup = true;
		label_t_height.UseMarkup = true;
		label_t_legsLength.UseMarkup = true;
		label_t_hipsHeight.UseMarkup = true;
	}

	void tableLabelsVisibility ()
	{
		// 1) fullname vs name surname
		label_t_fullname.Visible = (check_headers.Active && check_fullname_1_col.Active);
		label_t_name.Visible = (check_headers.Active && ! check_fullname_1_col.Active);
		label_t_surname.Visible = (check_headers.Active && ! check_fullname_1_col.Active);
		label_t_fullname_june.Visible = check_fullname_1_col.Active;
		label_t_name_june.Visible = ! check_fullname_1_col.Active;
		label_t_surname_june.Visible = ! check_fullname_1_col.Active;
		label_t_fullname_johnny.Visible = check_fullname_1_col.Active;
		label_t_name_johnny.Visible = ! check_fullname_1_col.Active;
		label_t_surname_johnny.Visible = ! check_fullname_1_col.Active;

		// 2) Genre, Weight row headers
		label_t_genre.Visible = check_headers.Active;
		label_t_weight.Visible = check_headers.Active;

		// 3) Height
		label_t_height.Visible = (check_headers.Active && check_person_height.Active);
		label_t_height_june.Visible = check_person_height.Active;
		label_t_height_johnny.Visible = check_person_height.Active;

		// 4) legsLength
		label_t_legsLength.Visible = (check_headers.Active && check_legsLength.Active);
		label_t_legsLength_june.Visible = check_legsLength.Active;
		label_t_legsLength_johnny.Visible = check_legsLength.Active;

		// 5) hipsHeight
		label_t_hipsHeight.Visible = (check_headers.Active && check_hipsHeight.Active);
		label_t_hipsHeight_june.Visible = check_hipsHeight.Active;
		label_t_hipsHeight_johnny.Visible = check_hipsHeight.Active;

		button_accept.Sensitive = false;
	}

	void on_check_headers_toggled (object obj, EventArgs args)
	{
		if(check_headers.Active) {
			image_csv_headers.Visible = true;
			image_csv_noheaders.Visible = false;
			label_csv.Text = Catalog.GetString("CSV file has headers");
			label_columns_order.Visible = true;

		} else {
			image_csv_headers.Visible = false;
			image_csv_noheaders.Visible = true;
			label_csv.Text = Catalog.GetString("CSV file does not have headers");
			label_columns_order.Visible = false;
		}

		tableLabelsVisibility();
	}

	private void textviewUpdate ()
	{
		string s1 = Catalog.GetString ("To differentiate between male and female,\nuse any of these on the sex column:") +
			"\n - " + Catalog.GetString ("Unspecified/Unknown:") + " (U, u, -)" +
			"\n - " + Catalog.GetString ("Male:") + " (M, m, 1)" +
			"\n - " + Catalog.GetString ("Female:") + " (F, f, 0)" +
			"\n\n" +
			Catalog.GetString ("Save the spreadsheet as CSV (Comma Separated Values).");
		string s2 = string.Format (Catalog.GetString(
					"Expected column separator character is '{0}'"), columnDelimiter) +
			"\n" + Catalog.GetString("You can change this on Preferences / Language.");

		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = s1 + "\n\n" + s2;
		textview.Buffer = tb;
	}

	void on_check_fullname_1_col_toggled (object obj, EventArgs args)
	{
		if(check_fullname_1_col.Active) {
			image_name1.Visible = true;
			image_name2.Visible = false;
			label_name.Text = Catalog.GetString("Full name in one column");
		} else {
			image_name1.Visible = false;
			image_name2.Visible = true;
			label_name.Text = Catalog.GetString("Full name in two columns");
		}
		
		tableLabelsVisibility();
	}

	private void on_check_person_other_variables_toggled (object o, EventArgs args)
	{
		tableLabelsVisibility();
	}
	
	void on_radio_csv_toggled (object obj, EventArgs args)
	{
		if (radio_csv.Active)
		{
			button_csv_prepare.Sensitive = true;
			hbox_manually.Sensitive = false;
		}
	}
	void on_radio_manually_toggled (object obj, EventArgs args)
	{
		if (radio_manually.Active)
		{
			button_csv_prepare.Sensitive = false;
			hbox_manually.Sensitive = true;
		}
	}

	private void on_button_csv_prepare_clicked (object obj, EventArgs args)
	{
		button_cancel_or_back.Label = Catalog.GetString ("Back");
		notebook.CurrentPage = Convert.ToInt32 (notebookPages.LOADCSV);
	}
		
	void on_button_csv_load_clicked (object obj, EventArgs args) 
	{
		Gtk.FileChooserDialog fc=
			new Gtk.FileChooserDialog(Catalog.GetString("Select CSV file"),
					null,
					FileChooserAction.Open,
					Catalog.GetString("Cancel"),ResponseType.Cancel,
					Catalog.GetString("Load"),ResponseType.Accept
					);

		fc.Filter = new FileFilter();
		fc.Filter.AddPattern("*.csv");
		fc.Filter.AddPattern("*.CSV");

		ArrayList array = new ArrayList();
		if (fc.Run() == (int)ResponseType.Accept) 
		{
			LogB.Warning("Opening CSV...");
			System.IO.FileStream file;
			try {
				file = System.IO.File.OpenRead(fc.Filename); 
			} catch {
				LogB.Warning("Catched, maybe is used by another program");
				new DialogMessage(Constants.MessageTypes.WARNING, 
						Constants.FileCannotSaveStr() + "\n\n" +
						Constants.FileCannotOpenedMaybeSpreadsheetOpened ()
						);
				fc.Destroy();
				return;
			}

			bool headersActive = check_headers.Active;
			bool name1Col = check_fullname_1_col.Active;
			bool useHeightCol = check_person_height.Active;
			bool useLegsLengthCol = check_legsLength.Active;
			bool useHipsHeightCol = check_hipsHeight.Active;

			List<string> columns = new List<string>();
			using (var reader = new CsvFileReader(fc.Filename))
			{
				reader.ChangeDelimiter(columnDelimiter);
				int genreCol = 1;
				int weightCol = 2;
				int heightCol = 3;
				int legsLengthCol = 4;
				int hipsHeightCol = 5;

				if (! name1Col)
				{
					genreCol ++;
					weightCol ++;
					heightCol ++;
					legsLengthCol ++;
					hipsHeightCol ++;
				}
				if (! useHeightCol)
				{
					legsLengthCol --;
					hipsHeightCol --;
				}
				if (! useLegsLengthCol)
				{
					hipsHeightCol --;
				}

				int row = 0;
				while (reader.ReadRow(columns))
				{
					string fullname = "";
					string onlyname = "";
					string sex = Constants.SexU;
					double weight = 0;
					double height = 0;
					double legsLength = 0;
					double hipsHeight = 0;
					bool errorsReading = false;

					int col = 0;
					foreach (string str in columns)
					{
						//if headers are active do not process first row
						//do not process this first row because weight can be a string
						if(headersActive && row == 0)
							continue;
						
						LogB.Debug(":" + str);

						if(col == 0) {
							if(name1Col)
								fullname = str;
							else
								onlyname = str;
						}
						else if(col == 1 && ! name1Col)
							fullname = onlyname + " " + str;
						else if (col == genreCol && (str == "-" || str == Constants.SexU.ToLower() || str == Constants.SexU))
							sex = Constants.SexU;
						else if (col == genreCol && (str == "1" || str == Constants.SexM.ToLower() || str == Constants.SexM))
							sex = Constants.SexM;
						else if (col == genreCol && (str == "0" || str == Constants.SexF.ToLower() || str == Constants.SexF))
							sex = Constants.SexF;
						else if (col == weightCol)
						{
							try {
								weight = Convert.ToDouble(Util.ChangeDecimalSeparator(str));
								LogB.Information("wwwww weight" + weight.ToString());
							} catch {
								errorsReading = true;
							}
						}
						else if (useHeightCol && col == heightCol)
						{
							try {
								height = Convert.ToDouble(Util.ChangeDecimalSeparator(str));
							} catch {
								errorsReading = true;
							}
						}
						else if (useLegsLengthCol && col == legsLengthCol)
						{
							try {
								legsLength = Convert.ToDouble(Util.ChangeDecimalSeparator(str));
							} catch {
								errorsReading = true;
							}
						}
						else if (useHipsHeightCol && col == hipsHeightCol)
						{
							try {
								hipsHeight = Convert.ToDouble(Util.ChangeDecimalSeparator(str));
							} catch {
								errorsReading = true;
							}
						}

						if (errorsReading)
						{
							string message = Catalog.GetString("Error importing data.");
							if( ! check_headers.Active && row == 0)
								message += "\n" + Catalog.GetString("Seems there's a header row and you have not marked it.");

							new DialogMessage(Constants.MessageTypes.WARNING, message);

							file.Close();
							//Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
							fc.Destroy();

							return;
						}

						col ++;
					}
					//if headers are active do not add first row
					if( ! (headersActive && row == 0) ) {
						PersonAddMultipleTable pamt = new PersonAddMultipleTable (
								Util.MakeValidSQL(fullname), sex, weight,
								height, legsLength, hipsHeight);

						array.Add(pamt);
					}
					
					row ++;
					LogB.Debug("\n");
				}
			}

			file.Close(); 

			rows = array.Count;
			createEmptyGrid (useHeightCol, useLegsLengthCol, useHipsHeightCol);
			fillGridFromCSV (array, useHeightCol, useLegsLengthCol, useHipsHeightCol);

			if(! Config.UseSystemColor)
				UtilGtk.ContrastLabelsWidget (Config.ColorBackgroundShiftedIsDark, grid_main);
		} 

		//Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
		fc.Destroy();
	}

	void on_button_manually_create_clicked (object obj, EventArgs args) 
	{
		button_cancel_or_back.Label = Catalog.GetString ("Back");

		rows = Convert.ToInt32(spin_manually.Value);

		bool useHeightCol = check_person_height.Active;
		bool useLegsLengthCol = check_legsLength.Active;
		bool useHipsHeightCol = check_hipsHeight.Active;

		createEmptyGrid (useHeightCol, useLegsLengthCol, useHipsHeightCol);
		if(! Config.UseSystemColor)
			UtilGtk.ContrastLabelsWidget (Config.ColorBackgroundShiftedIsDark, grid_main);
	}

	List<Gtk.Label> error_label_in_session_l;
	List<Gtk.Label> error_label_in_db_l;
	List<Gtk.CheckButton> error_check_use_stored_l;
	List<Gtk.Label> error_label_repeated_name_l;
	List<Gtk.Label> error_label_no_weight_l;
	Gtk.Label errorColumnLabel;

	List<PersonAddMultipleError> pame_l;

	void createEmptyGrid (bool useHeightCol, bool useLegsLengthCol, bool useHipsHeightCol)
	{
		if (grid_main != null && grid_main.Children.Length > 0)
			UtilGtk.RemoveChildren (grid_main);

		//initialize error lists
		error_label_in_session_l = new List<Gtk.Label>();
		error_label_in_db_l = new List<Gtk.Label>();
		error_check_use_stored_l = new List<Gtk.CheckButton>();
		error_label_repeated_name_l = new List<Gtk.Label>();
		error_label_no_weight_l = new List<Gtk.Label>();

		entries = new ArrayList();
		radiosU = new ArrayList();
		radiosM = new ArrayList();
		radiosF = new ArrayList();
		spinsWeight = new ArrayList();
		spinsHeight = new ArrayList();
		spinsLegsLength = new ArrayList();
		spinsHipsHeight = new ArrayList();

		errorColumnLabel = new Gtk.Label("<b>" + Catalog.GetString("Error") + "</b>");
		Gtk.Label nameLabel = new Gtk.Label("<b>" + Catalog.GetString("Full name") + "</b>");
		Gtk.Label sexLabel = new Gtk.Label("<b>" + Catalog.GetString("Sex") + "</b>");
		Gtk.Label weightLabel = new Gtk.Label("<b>" + Catalog.GetString("Weight") +
				"</b> (" + Catalog.GetString("Kg") + ")" );
		Gtk.Label heightLabel = new Gtk.Label("<b>" + Catalog.GetString("Height") +
				"</b> (" + Catalog.GetString("cm") + ")" );
		Gtk.Label legsLengthLabel = new Gtk.Label("<b>h1</b> (" + Catalog.GetString("cm") + ")" );
		Gtk.Label hipsHeightLabel = new Gtk.Label("<b>h2</b> (" + Catalog.GetString("cm") + ")" );
		
		errorColumnLabel.UseMarkup = true;
		nameLabel.UseMarkup = true;
		sexLabel.UseMarkup = true;
		weightLabel.UseMarkup = true;
		if (useHeightCol)
			heightLabel.UseMarkup = true;
		if (useLegsLengthCol)
			legsLengthLabel.UseMarkup = true;
		if (useHipsHeightCol)
			hipsHeightLabel.UseMarkup = true;

		nameLabel.Xalign = 0;
		sexLabel.Xalign = 0;
		weightLabel.Xalign = 0;
		heightLabel.Xalign = 0;
		legsLengthLabel.Xalign = 0;
		hipsHeightLabel.Xalign = 0;
		
		errorColumnLabel.Hide();
		nameLabel.Show();
		sexLabel.Show();
		weightLabel.Show();
		if (useHeightCol)
			heightLabel.Show();
		if (useLegsLengthCol)
			legsLengthLabel.Show();
		if (useHipsHeightCol)
			hipsHeightLabel.Show();
	
		grid_main.ColumnSpacing = 4;
		grid_main.RowSpacing = 4;

		int x = 1; //id col 0, errors col1, fullname col (2)
		grid_main.Attach (errorColumnLabel, x++, 0, 1, 1);
		grid_main.Attach (nameLabel, x++, 0, 1, 1);
		grid_main.Attach (sexLabel, x++, 0, 1, 1);
		grid_main.Attach (weightLabel, x++, 0, 1, 1);
		if (useHeightCol)
			grid_main.Attach (heightLabel, x++, 0, 1, 1);
		if (useLegsLengthCol)
			grid_main.Attach (legsLengthLabel, x++, 0, 1, 1);
		if (useHipsHeightCol)
			grid_main.Attach (hipsHeightLabel, x++, 0, 1, 1);

		for (int count=1; count <= rows; count ++)
		{
			x = 0; //id col 0, errors col1, fullname col (2)

			//id (count)
			Gtk.Label myLabel = new Gtk.Label((count).ToString());
			myLabel.Show();
			grid_main.Attach (myLabel, x++, count, 1, 1);

			//errors
			Gtk.Label error_label_in_session = new Gtk.Label (Catalog.GetString ("Name already\nin session"));
			Gtk.Label error_label_in_db = new Gtk.Label (Catalog.GetString ("Name already\nin database"));
			Gtk.CheckButton error_check_use_stored = new Gtk.CheckButton ("Use stored person");
			Gtk.Label error_label_repeated_name = new Gtk.Label (Catalog.GetString ("Repeated name"));
			Gtk.Label error_label_no_weight = new Gtk.Label (Catalog.GetString ("No weight"));

			error_label_in_session_l.Add (error_label_in_session);
			error_label_in_db_l.Add (error_label_in_db);
			error_check_use_stored_l.Add (error_check_use_stored);
			error_label_repeated_name_l.Add (error_label_repeated_name);
			error_label_no_weight_l.Add (error_label_no_weight);

			error_label_in_session.Visible = false;
			error_label_in_db.Visible = false;
			error_check_use_stored.Visible = false;
			error_label_repeated_name.Visible = false;
			error_label_no_weight.Visible = false;

			Gtk.HBox idError = new HBox();
			/* structure of HBox idError:

			            |  in_session  | in_db   |
			   repeated | ---------------------- | no weight
			            | error_check_use_stored |

			   it will be shown 1 of the 4 error types
			   if in_session or in_db then show the checkbox
			*/

			idError.PackStart (error_label_repeated_name, false, false, 0);

			Gtk.HBox idErrorExistsH = new HBox();
			idErrorExistsH.PackStart (error_label_in_session, true, true, 0);
			idErrorExistsH.PackStart (error_label_in_db, true, true, 0);
			idErrorExistsH.Show();
			Gtk.VBox idErrorExistsV = new VBox();
			idErrorExistsV.PackStart (idErrorExistsH, true, true, 0);
			idErrorExistsV.PackStart (error_check_use_stored, false, false, 0);
			idErrorExistsV.Show();
			idError.PackStart (idErrorExistsV, false, false, 0);

			idError.PackStart (error_label_no_weight, false, false, 0);
			idError.Show();

			grid_main.Attach (idError, x++, count, 1, 1);

			Gtk.Entry myEntry = new Gtk.Entry();
			grid_main.Attach (myEntry, x++, count, 1, 1);
			myEntry.Changed += on_entry_changed;
			myEntry.Show();
			entries.Add(myEntry);

			Gtk.RadioButton myRadioU = new Gtk.RadioButton (Catalog.GetString (Constants.SexU));
			myRadioU.Show ();
			radiosU.Add (myRadioU);
			
			Gtk.RadioButton myRadioM = new Gtk.RadioButton (myRadioU, Catalog.GetString (Constants.SexM));
			myRadioM.Show ();
			radiosM.Add (myRadioM);
			
			Gtk.RadioButton myRadioF = new Gtk.RadioButton (myRadioU, Catalog.GetString (Constants.SexF));
			myRadioF.Show ();
			radiosF.Add (myRadioF);
			
			Gtk.HBox sexBox = new HBox();
			sexBox.PackStart(myRadioU, false, false, 2);
			sexBox.PackStart(myRadioM, false, false, 2);
			sexBox.PackStart(myRadioF, false, false, 2);
			sexBox.Show();
			grid_main.Attach (sexBox, x++, count, 1, 1);


			Gtk.SpinButton mySpinWeight = new Gtk.SpinButton(0, 300, .1);
			grid_main.Attach (mySpinWeight, x++, count, 1, 1);
			mySpinWeight.Show();
			spinsWeight.Add (mySpinWeight);

			if (useHeightCol)
			{
				Gtk.SpinButton mySpinHeight = new Gtk.SpinButton(0, 250, .1);
				grid_main.Attach (mySpinHeight, x++, count, 1, 1);
				mySpinHeight.Show();
				spinsHeight.Add (mySpinHeight);
			}

			if (useLegsLengthCol)
			{
				Gtk.SpinButton mySpinLegsLength = new Gtk.SpinButton(0, 150, .1);
				grid_main.Attach (mySpinLegsLength, x++, count, 1, 1);
				mySpinLegsLength.Show();
				spinsLegsLength.Add (mySpinLegsLength);
			}

			if (useHipsHeightCol)
			{
				Gtk.SpinButton mySpinHipsHeight = new Gtk.SpinButton(0, 150, .1);
				grid_main.Attach (mySpinHipsHeight, x++, count, 1, 1);
				mySpinHipsHeight.Show();
				spinsHipsHeight.Add (mySpinHipsHeight);
			}
		}

		string sportStuffString = "";
		if(currentSession.PersonsSportID != Constants.SportUndefinedID)
			sportStuffString += Catalog.GetString("Sport") + ":<i>" + Catalog.GetString(SqliteSport.Select(false, currentSession.PersonsSportID).Name) + "</i>.";
		if(currentSession.PersonsSpeciallityID != Constants.SpeciallityUndefinedID)
			sportStuffString += " " + Catalog.GetString("Specialty") + ":<i>" + SqliteSpeciallity.Select(false, currentSession.PersonsSpeciallityID) + "</i>.";
		if(currentSession.PersonsPractice != Constants.LevelUndefinedID)
			sportStuffString += " " + Catalog.GetString("Level") + ":<i>" + Util.FindLevelName(currentSession.PersonsPractice) + "</i>.";

		if(sportStuffString.Length > 0)
		{
			sportStuffString = Catalog.GetString("Persons will be created with default session values") + 
				":\n" + sportStuffString;
			label_message.Text = sportStuffString;
			label_message.UseMarkup = true;
			label_message.Visible = true;
		}

		if (useLegsLengthCol || useHipsHeightCol) 	//show the h1, h2
		{
			hbox_h1_h2_help.Visible = true;
			if (useLegsLengthCol)
			{
				label_h1_legsLength.Text = "<b>h1</b>: " + Catalog.GetString ("Leg length");
				label_h1_legsLength.UseMarkup = true;
			}
			if (useHipsHeightCol)
			{
				label_h2_hipsHeight.Text = "<b>h2</b>: " + Catalog.GetString ("Hips height on SJ flexion");
				label_h2_hipsHeight.UseMarkup = true;
			}
		} else
			hbox_h1_h2_help.Visible = false;

		grid_main.Show();
		grid_main.Visible = true;
		notebook.CurrentPage = Convert.ToInt32 (notebookPages.TABLEMANUALLY);

		button_accept.Sensitive = true;
	}

	void on_entry_changed (object o, EventArgs args)
	{
		Gtk.Entry entry = o as Gtk.Entry;
		if (o == null)
			return;

		entry.Text = Util.MakeValidSQL(entry.Text);
	}

	void fillGridFromCSV (ArrayList array, bool useHeightCol, bool useLegsLengthCol, bool useHipsHeightCol)
	{
		int i = 0;
		foreach (PersonAddMultipleTable pamt in array)
		{
			((Gtk.Entry) entries[i]).Text = pamt.name;
			((Gtk.RadioButton) radiosU[i]).Active = (pamt.sex == Constants.SexU);
			((Gtk.RadioButton) radiosM[i]).Active = (pamt.sex == Constants.SexM);
			((Gtk.RadioButton) radiosF[i]).Active = (pamt.sex == Constants.SexF);
			LogB.Information("going to weight");
			((Gtk.SpinButton) spinsWeight[i]).Value = pamt.weight;

			if (useHeightCol)
			{
				LogB.Information("going to height");
				((Gtk.SpinButton) spinsHeight[i]).Value = pamt.height;
			}
			if (useLegsLengthCol)
			{
				LogB.Information("going to h1");
				((Gtk.SpinButton) spinsLegsLength[i]).Value = pamt.legsLength;
			}
			if (useHipsHeightCol)
			{
				LogB.Information("going to h2");
				((Gtk.SpinButton) spinsHipsHeight[i]).Value = pamt.hipsHeight;
				LogB.Information("h2 done");
			}
			i ++;
		}
	}

	void on_button_accept_clicked (object o, EventArgs args)
	{
		pame_l = new List<PersonAddMultipleError>();
		foreach (Gtk.Label l in error_label_in_session_l)
			l.Visible = false;
		foreach (Gtk.Label l in error_label_in_db_l)
			l.Visible = false;
		foreach (Gtk.CheckButton cb in error_check_use_stored_l)
			cb.Visible = false;
		foreach (Gtk.Label l in error_label_repeated_name_l)
			l.Visible = false;
		foreach (Gtk.Label l in error_label_no_weight_l)
			l.Visible = false;

		//personsCreatedCount = 0;

		checkAllEntriesAreDifferent();

		Sqlite.Open();
		for (int i = 0; i < rows; i ++) 
			checkEntries(i, ((Gtk.Entry)entries[i]).Text.ToString(), (int) ((Gtk.SpinButton) spinsWeight[i]).Value);
		Sqlite.Close();

		bool errors = false;
		errorColumnLabel.Visible = false;

		foreach (PersonAddMultipleError pame in pame_l)
		{
			if (pame.errorType == PersonAddMultipleError.ErrorType.REPEATEDNAME)
			{
				error_label_repeated_name_l[pame.id].Visible = true;
				errors = true;
			}
			else if (pame.errorType == PersonAddMultipleError.ErrorType.INSESSION ||
					pame.errorType == PersonAddMultipleError.ErrorType.INDB)
			{
				if (pame.errorType == PersonAddMultipleError.ErrorType.INSESSION)
				{
					error_label_in_session_l[pame.id].Visible = true;
					error_label_in_session_l[pame.id].Justify = Justification.Center;
					error_label_in_session_l[pame.id].Wrap = false;
				}
				else if (pame.errorType == PersonAddMultipleError.ErrorType.INDB)
				{
					error_label_in_db_l[pame.id].Visible = true;
					error_label_in_db_l[pame.id].Justify = Justification.Center;
					error_label_in_db_l[pame.id].Wrap = false;
				}

				error_check_use_stored_l[pame.id].Visible = true;
				if ( ! error_check_use_stored_l[pame.id].Active)
					errors = true;
			}
			else if (pame.errorType == PersonAddMultipleError.ErrorType.NOWEIGHT)
			{
				error_label_no_weight_l[pame.id].Visible = true;
				errors = true;
			}
		}

		if (errors)
			errorColumnLabel.Visible = true;
		else {
			processAllNonBlankRows();
			fakeButtonDone.Click();

			PersonAddMultipleWindowBox.person_multiple_infinite.Hide();
			PersonAddMultipleWindowBox = null;
		}
	}

	//do not need to check height, legsLength, hipsHeight, can be 0
	private void checkEntries (int count, string name, double weight)
	{
		if(name.Length > 0)
		{
			//1st check that name is not repeated. If repeated, 1st solve this
			if (PersonAddMultipleError.ExistErrorLike (pame_l,
						Util.RemoveTilde (name), PersonAddMultipleError.ErrorType.REPEATEDNAME))
				return;

			bool personExists = Sqlite.Exists (true, Constants.PersonTable, Util.RemoveTilde(name));

			if (personExists)
			{
				bool inThisSession = false;
				List<Person> p_l = SqlitePersonSession.SelectCurrentSessionPersonsAsList (
						true, currentSession.UniqueID);
				foreach (Person p in p_l)
					if (name == p.Name)
					{
						inThisSession = true;
						break;
					}

				if (inThisSession)
					pame_l.Add (new PersonAddMultipleError (count, "", PersonAddMultipleError.ErrorType.INSESSION));
				else
					pame_l.Add (new PersonAddMultipleError (count, "", PersonAddMultipleError.ErrorType.INDB));
			}
			else if (Convert.ToInt32(weight) == 0) // "else if" to only complain on no weight when person is new
				pame_l.Add (new PersonAddMultipleError (count, "", PersonAddMultipleError.ErrorType.NOWEIGHT));
		}
	}
		
	void checkAllEntriesAreDifferent()
	{
		ArrayList newNames= new ArrayList();
		for (int i = 0; i < rows; i ++) 
			newNames.Add(((Gtk.Entry)entries[i]).Text.ToString());

		for(int i=0; i < rows; i++)
		{
			bool repeated = false;
			if(Util.RemoveTilde(newNames[i].ToString()).Length > 0)
			{
				int j;
				for (j=i+1; j < newNames.Count && ! repeated; j++)
				{
					if( Util.RemoveTilde(newNames[i].ToString()) == Util.RemoveTilde(newNames[j].ToString()) )
						repeated = true;
				}
				if(repeated)
				{
					// a) if the first time it appeared is not on pame_l yet, include it now
					if ( ! PersonAddMultipleError.ExistErrorLike (pame_l,
								Util.RemoveTilde (newNames[i].ToString ()),
								PersonAddMultipleError.ErrorType.REPEATEDNAME))
						pame_l.Add (new PersonAddMultipleError (
									j-1, Util.RemoveTilde (newNames[i].ToString()), PersonAddMultipleError.ErrorType.REPEATEDNAME));

					// b) then add current row
					pame_l.Add (new PersonAddMultipleError (
								i, Util.RemoveTilde (newNames[i].ToString()), PersonAddMultipleError.ErrorType.REPEATEDNAME));
				}
			}
		}
	}
	
	//inserts all the rows where name is not blank
	//all this names doesn't match with other in the database, and the weights are > 0 ( checked in checkEntries() )
	void processAllNonBlankRows ()
	{
		int pID;
		int countPersons = Sqlite.Count(Constants.PersonTable, false);
		if(countPersons == 0)
			pID = 1;
		else {
			//Sqlite.Max will return NULL if there are no values, for this reason we use the Sqlite.Count before
			int maxPUniqueID = Sqlite.Max(Constants.PersonTable, "uniqueID", false);
			pID = maxPUniqueID + 1;
		}

		int psID;
		int countPersonSessions = Sqlite.Count(Constants.PersonSessionTable, false);
		if(countPersonSessions == 0)
			psID = 1;
		else {
			//Sqlite.Max will return NULL if there are no values, for this reason we use the Sqlite.Count before
			int maxPSUniqueID = Sqlite.Max(Constants.PersonSessionTable, "uniqueID", false);
			psID = maxPSUniqueID + 1;
		}
		
		string sex = "";
		double weight = 0;
		double height = 0;
		double legsLength = 0;
		double hipsHeight = 0;
				
		List <Person> persons = new List<Person>();
		List <PersonSession> personSessions = new List<PersonSession>();

		DateTime dateTime = DateTime.MinValue;

		//the last is the first for having the first value inserted as currentPerson
		for (int i = rows -1; i >= 0; i --) 
			if(((Gtk.Entry)entries[i]).Text.ToString().Length > 0) 
			{
				sex = Constants.SexU;
				if (((Gtk.RadioButton) radiosM[i]).Active)
					sex = Constants.SexM;
				else if (((Gtk.RadioButton) radiosF[i]).Active)
					sex = Constants.SexF;

				PersonSession psExisting = new PersonSession ();
				bool createPerson = true;
				if (error_check_use_stored_l[i].Visible && error_check_use_stored_l[i].Active)
				{
					//do not create person, just load it (create personSession below)
					currentPerson = SqlitePerson.SelectByName (false,
							Util.RemoveTilde (((Gtk.Entry)entries[i]).Text.ToString()));
					psExisting = SqlitePersonSession.Select (currentPerson.UniqueID, -1); //if sessionID == -1 we search data in last sessionID
					createPerson = false;
				} else {
					currentPerson = new Person(
							pID ++,
							((Gtk.Entry)entries[i]).Text.ToString(), //name
							sex,
							dateTime,
							Constants.RaceUndefinedID,
							Constants.CountryUndefinedID,
							"", "", "", 		//description, future1: rfid, future2: clubID
							Constants.ServerUndefinedID,
							""			//linkServerImage
							);

					persons.Add (currentPerson);
				}

				weight = (double) ((Gtk.SpinButton) spinsWeight[i]).Value;

				height = 0;
				if (check_person_height.Active)
					height = (double) ((Gtk.SpinButton) spinsHeight[i]).Value;
				if (! createPerson && height == 0 && psExisting.Height > 0)
					height = psExisting.Height;

				legsLength = 0;
				if (check_legsLength.Active)
					legsLength = (double) ((Gtk.SpinButton) spinsLegsLength[i]).Value;
				if (! createPerson && legsLength == 0 && psExisting.TrochanterToe > 0)
					legsLength = psExisting.TrochanterToe;

				hipsHeight = 0;
				if (check_hipsHeight.Active)
					hipsHeight = (double) ((Gtk.SpinButton) spinsHipsHeight[i]).Value;
				if (! createPerson && hipsHeight == 0 && psExisting.TrochanterFloorOnFlexion > 0)
					hipsHeight = psExisting.TrochanterFloorOnFlexion;

				int sportID = currentSession.PersonsSportID;
				if (! createPerson && sportID == Constants.SportUndefinedID &&
						psExisting.SportID > Constants.SportUndefinedID)
					sportID = psExisting.SportID;

				int speciallityID = currentSession.PersonsSpeciallityID;
				if (! createPerson && speciallityID == Constants.SpeciallityUndefinedID &&
						psExisting.SpeciallityID > Constants.SpeciallityUndefinedID)
					speciallityID = psExisting.SpeciallityID;

				int practice = currentSession.PersonsPractice;
				if (! createPerson && practice == Constants.LevelUndefinedID &&
						psExisting.Practice > Constants.LevelUndefinedID)
					practice = psExisting.Practice;

				PersonSession ps = new PersonSession (
						psID ++,
						currentPerson.UniqueID, currentSession.UniqueID,
						height, weight,
						sportID, speciallityID,	practice,
						"", 			//comments
						legsLength, hipsHeight
						);

				if (! createPerson && error_label_in_session_l[i].Visible)
				{
					//if it is on session not need to create the personSession, just update it
					//get the personSession on this session
					psID = SqlitePersonSession.Select (false, currentPerson.UniqueID, currentSession.UniqueID).UniqueID;
					if (psID > 0)
					{
						ps.UniqueID = psID;
						SqlitePersonSession.Update (false, ps); //update
					}
				} else
					personSessions.Add (ps);

				//personsCreatedCount ++;
			}
	
		//do the transaction	
		new SqlitePersonSessionTransaction (persons, personSessions);
	}

	public Button FakeButtonDone
	{
		get { return fakeButtonDone; }
	}

	public Button FakeButtonCancel
	{
		get { return fakeButtonCancel; }
	}

	public Person CurrentPerson
	{
		get { return currentPerson; }
	}

	/*
	public int PersonsCreatedCount 
	{
		get { return personsCreatedCount; }
	}
	*/

	private void connectWidgets (Gtk.Builder builder)
	{
		person_multiple_infinite = (Gtk.Window) builder.GetObject ("person_multiple_infinite");

		notebook = (Gtk.Notebook) builder.GetObject ("notebook");
		button_cancel_or_back = (Gtk.Button) builder.GetObject ("button_cancel_or_back");

		radio_csv = (Gtk.RadioButton) builder.GetObject ("radio_csv");
		radio_manually = (Gtk.RadioButton) builder.GetObject ("radio_manually");
		hbox_manually = (Gtk.Box) builder.GetObject ("hbox_manually");
		spin_manually = (Gtk.SpinButton) builder.GetObject ("spin_manually");

		image_csv_headers = (Gtk.Image) builder.GetObject ("image_csv_headers");
		image_csv_noheaders = (Gtk.Image) builder.GetObject ("image_csv_noheaders");
		image_load = (Gtk.Image) builder.GetObject ("image_load");
		label_csv = (Gtk.Label) builder.GetObject ("label_csv");
		label_name = (Gtk.Label) builder.GetObject ("label_name");

		button_csv_prepare = (Gtk.Button) builder.GetObject ("button_csv_prepare");

		image_name1 = (Gtk.Image) builder.GetObject ("image_name1");
		image_name2 = (Gtk.Image) builder.GetObject ("image_name2");

		check_headers = (Gtk.CheckButton) builder.GetObject ("check_headers");
		check_fullname_1_col = (Gtk.CheckButton) builder.GetObject ("check_fullname_1_col");
		check_person_height = (Gtk.CheckButton) builder.GetObject ("check_person_height");
		check_legsLength = (Gtk.CheckButton) builder.GetObject ("check_legsLength");
		check_hipsHeight = (Gtk.CheckButton) builder.GetObject ("check_hipsHeight");

		hbox_h1_h2_help = (Gtk.HBox) builder.GetObject ("hbox_h1_h2_help");
		label_h1_legsLength = (Gtk.Label) builder.GetObject ("label_h1_legsLength");
		label_h2_hipsHeight = (Gtk.Label) builder.GetObject ("label_h2_hipsHeight");

		//show/hide headers and make them bold
		label_t_fullname = (Gtk.Label) builder.GetObject ("label_t_fullname");
		label_t_name = (Gtk.Label) builder.GetObject ("label_t_name");
		label_t_surname = (Gtk.Label) builder.GetObject ("label_t_surname");
		label_t_genre = (Gtk.Label) builder.GetObject ("label_t_genre");
		label_t_weight = (Gtk.Label) builder.GetObject ("label_t_weight");
		label_t_height = (Gtk.Label) builder.GetObject ("label_t_height");
		label_t_legsLength = (Gtk.Label) builder.GetObject ("label_t_legsLength");
		label_t_hipsHeight = (Gtk.Label) builder.GetObject ("label_t_hipsHeight");
		//show/hide hideable columns of June Carter
		label_t_fullname_june = (Gtk.Label) builder.GetObject ("label_t_fullname_june");
		label_t_name_june = (Gtk.Label) builder.GetObject ("label_t_name_june");
		label_t_surname_june = (Gtk.Label) builder.GetObject ("label_t_surname_june");
		label_t_height_june = (Gtk.Label) builder.GetObject ("label_t_height_june");
		label_t_legsLength_june = (Gtk.Label) builder.GetObject ("label_t_legsLength_june");
		label_t_hipsHeight_june = (Gtk.Label) builder.GetObject ("label_t_hipsHeight_june");
		//show/hide hideable columns of Johnny Cash
		label_t_fullname_johnny = (Gtk.Label) builder.GetObject ("label_t_fullname_johnny");
		label_t_name_johnny = (Gtk.Label) builder.GetObject ("label_t_name_johnny");
		label_t_surname_johnny = (Gtk.Label) builder.GetObject ("label_t_surname_johnny");
		label_t_height_johnny = (Gtk.Label) builder.GetObject ("label_t_height_johnny");
		label_t_legsLength_johnny = (Gtk.Label) builder.GetObject ("label_t_legsLength_johnny");
		label_t_hipsHeight_johnny = (Gtk.Label) builder.GetObject ("label_t_hipsHeight_johnny");

		textview = (Gtk.TextView) builder.GetObject ("textview");

		//scrolledwindow = (Gtk.ScrolledWindow) builder.GetObject ("scrolledwindow");
		grid_main = (Gtk.Grid) builder.GetObject ("grid_main");
		label_message = (Gtk.Label) builder.GetObject ("label_message");
		label_columns_order = (Gtk.Label) builder.GetObject ("label_columns_order");

		button_accept = (Gtk.Button) builder.GetObject ("button_accept");
	}
}

