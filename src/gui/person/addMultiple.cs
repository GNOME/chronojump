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
 * Copyright (C) 2004-2022   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
using Gdk;
using Glade;
using System.Collections; //ArrayList
using System.Collections.Generic; //List
using Mono.Unix;

public class PersonAddMultipleTable
{
	public string name;
	public bool maleOrFemale;
	public double weight;
	public double height;
	public double legsLength;
	public double hipsHeight;

	public PersonAddMultipleTable (string name, bool maleOrFemale, double weight,
			double height, double legsLength, double hipsHeight)
	{
		this.name = name;
		this.maleOrFemale = maleOrFemale;
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
	
	[Widget] Gtk.Window person_multiple_infinite;
		
	[Widget] Gtk.Notebook notebook;
	[Widget] Gtk.Button button_cancel_or_back;
	
	[Widget] Gtk.RadioButton radio_csv;
	[Widget] Gtk.RadioButton radio_manually;
	[Widget] Gtk.Box hbox_manually;
	[Widget] Gtk.SpinButton spin_manually;
	
	[Widget] Gtk.Image image_csv_headers;
	[Widget] Gtk.Image image_csv_noheaders;
	[Widget] Gtk.Image image_load;
	[Widget] Gtk.Label label_csv;
	[Widget] Gtk.Label label_name;

	[Widget] Gtk.Button button_csv_prepare;
	
	[Widget] Gtk.Image image_name1;
	[Widget] Gtk.Image image_name2;

	[Widget] Gtk.CheckButton check_headers;
	[Widget] Gtk.CheckButton check_fullname_1_col;
	[Widget] Gtk.CheckButton check_person_height;
	[Widget] Gtk.CheckButton check_legsLength;
	[Widget] Gtk.CheckButton check_hipsHeight;

	[Widget] Gtk.HBox hbox_h1_h2_help;
	[Widget] Gtk.Label label_h1_legsLength;
	[Widget] Gtk.Label label_h2_hipsHeight;

	//[Widget] Gtk.Table table_example;
	//show/hide headers and make them bold
	[Widget] Gtk.Label label_t_fullname;
	[Widget] Gtk.Label label_t_name;
	[Widget] Gtk.Label label_t_surname;
	[Widget] Gtk.Label label_t_genre;
	[Widget] Gtk.Label label_t_weight;
	[Widget] Gtk.Label label_t_height;
	[Widget] Gtk.Label label_t_legsLength;
	[Widget] Gtk.Label label_t_hipsHeight;
	//show/hide hideable columns of June Carter
	[Widget] Gtk.Label label_t_fullname_june;
	[Widget] Gtk.Label label_t_name_june;
	[Widget] Gtk.Label label_t_surname_june;
	[Widget] Gtk.Label label_t_height_june;
	[Widget] Gtk.Label label_t_legsLength_june;
	[Widget] Gtk.Label label_t_hipsHeight_june;
	//show/hide hideable columns of Johnny Cash
	[Widget] Gtk.Label label_t_fullname_johnny;
	[Widget] Gtk.Label label_t_name_johnny;
	[Widget] Gtk.Label label_t_surname_johnny;
	[Widget] Gtk.Label label_t_height_johnny;
	[Widget] Gtk.Label label_t_legsLength_johnny;
	[Widget] Gtk.Label label_t_hipsHeight_johnny;

	[Widget] Gtk.TextView textview;

	private enum notebookPages { MAINOPTIONS, TABLEMANUALLY, LOADCSV };

	//use this to read/write table
	ArrayList entries;
	ArrayList radiosM;
	ArrayList radiosF;
	ArrayList spinsWeight;
	ArrayList spinsHeight;
	ArrayList spinsLegsLength;
	ArrayList spinsHipsHeight;
	
	int rows;
	
	//[Widget] Gtk.ScrolledWindow scrolledwindow;
	[Widget] Gtk.Table table_main;
	[Widget] Gtk.Label label_message;
	[Widget] Gtk.Label label_columns_order;
	
	[Widget] Gtk.Button button_accept;
	private Gtk.Button fakeButtonDone;
	
	static PersonAddMultipleWindow PersonAddMultipleWindowBox;

	private Person currentPerson;
	Session currentSession;
	char columnDelimiter;
	//int personsCreatedCount;


	PersonAddMultipleWindow (Gtk.Window parent, Session currentSession, char columnDelimiter)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "person_multiple_infinite.glade", "person_multiple_infinite", null);
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(person_multiple_infinite);

		//manage window color
		if(! Config.UseSystemColor)
			UtilGtk.WindowColor(person_multiple_infinite, Config.ColorBackground);
	
		fakeButtonDone = new Gtk.Button();
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
		string s1 = Catalog.GetString ("To differentiate between male and female,\nuse the values 1/0, or m/f, or M/F on the genre column.") + "\n\n" +
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
						Catalog.GetString("Maybe this file is opened by an SpreadSheet software like Excel. Please, close that program.")
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
					bool maleOrFemale = true;
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
						else if (col == genreCol && (str == "0" || str == "f" || str == "F")) 	//female symbols
							maleOrFemale = false;
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
								Util.MakeValidSQL(fullname), maleOrFemale, weight,
								height, legsLength, hipsHeight);

						array.Add(pamt);
					}
					
					row ++;
					LogB.Debug("\n");
				}
			}

			file.Close(); 

			rows = array.Count;
			createEmptyTable (useHeightCol, useLegsLengthCol, useHipsHeightCol);
			fillTableFromCSV (array, useHeightCol, useLegsLengthCol, useHipsHeightCol);
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

		createEmptyTable (useHeightCol, useLegsLengthCol, useHipsHeightCol);
	}

	List<Gtk.Label> error_label_in_session_l;
	List<Gtk.Label> error_label_in_db_l;
	List<Gtk.CheckButton> error_check_use_stored_l;
	List<Gtk.Label> error_label_repeated_name_l;
	List<Gtk.Label> error_label_no_weight_l;
	Gtk.Label errorColumnLabel;

	List<PersonAddMultipleError> pame_l;

	void createEmptyTable (bool useHeightCol, bool useLegsLengthCol, bool useHipsHeightCol)
	{
		if (table_main != null && table_main.Children.Length > 0)
			UtilGtk.RemoveChildren (table_main);

		//initialize error lists
		error_label_in_session_l = new List<Gtk.Label>();
		error_label_in_db_l = new List<Gtk.Label>();
		error_check_use_stored_l = new List<Gtk.CheckButton>();
		error_label_repeated_name_l = new List<Gtk.Label>();
		error_label_no_weight_l = new List<Gtk.Label>();

		entries = new ArrayList();
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
	
		uint padding = 4;	

		int x = 1; //id col 0, errors col1, fullname col (2)
		table_main.Attach (errorColumnLabel, (uint) x, (uint) ++x, 0, 1,
				Gtk.AttachOptions.Shrink | Gtk.AttachOptions.Shrink , Gtk.AttachOptions.Shrink, padding, padding);
		table_main.Attach (nameLabel, (uint) x, (uint) ++x, 0, 1,
				Gtk.AttachOptions.Fill | Gtk.AttachOptions.Expand , Gtk.AttachOptions.Shrink, padding, padding);
		table_main.Attach (sexLabel, (uint) x, (uint) ++x, 0, 1,
				Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, padding, padding);
		table_main.Attach (weightLabel, (uint) x, (uint) ++x, 0, 1,
				Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, padding, padding);
		if (useHeightCol)
			table_main.Attach (heightLabel, (uint) x, (uint) ++x, 0, 1,
					Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, padding, padding);
		if (useLegsLengthCol)
			table_main.Attach (legsLengthLabel, (uint) x, (uint) ++x, 0, 1,
					Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, padding, padding);
		if (useHipsHeightCol)
			table_main.Attach (hipsHeightLabel, (uint) x, (uint) ++x, 0, 1,
					Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, padding, padding);

		for (int count=1; count <= rows; count ++)
		{
			x = 0; //id col 0, errors col1, fullname col (2)

			//id (count)
			Gtk.Label myLabel = new Gtk.Label((count).ToString());
			myLabel.Show();
			table_main.Attach (myLabel, (uint) x, (uint) ++x, (uint) count, (uint) count +1,
					Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, padding, padding);

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

			table_main.Attach (idError, (uint) x, (uint) ++x, (uint) count, (uint) count +1,
					Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, padding, padding);

			Gtk.Entry myEntry = new Gtk.Entry();
			table_main.Attach (myEntry, (uint) x, (uint) ++x, (uint) count, (uint) count +1,
					Gtk.AttachOptions.Fill | Gtk.AttachOptions.Expand , Gtk.AttachOptions.Shrink, padding, padding);
			myEntry.Changed += on_entry_changed;
			myEntry.Show();
			entries.Add(myEntry);

			
			Gtk.RadioButton myRadioM = new Gtk.RadioButton(Catalog.GetString(Constants.M));
			myRadioM.Show();
			radiosM.Add(myRadioM);
			
			Gtk.RadioButton myRadioF = new Gtk.RadioButton(myRadioM, Catalog.GetString(Constants.F));
			myRadioF.Show();
			radiosF.Add(myRadioF);
			
			Gtk.HBox sexBox = new HBox();
			sexBox.PackStart(myRadioM, false, false, 4);
			sexBox.PackStart(myRadioF, false, false, 4);
			sexBox.Show();
			table_main.Attach (sexBox, (uint) x, (uint) ++x, (uint) count, (uint) count +1,
					Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, padding, padding);


			Gtk.SpinButton mySpinWeight = new Gtk.SpinButton(0, 300, .1);
			table_main.Attach (mySpinWeight, (uint) x, (uint) ++x, (uint) count, (uint) count +1,
					Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, padding, padding);
			mySpinWeight.Show();
			spinsWeight.Add (mySpinWeight);

			if (useHeightCol)
			{
				Gtk.SpinButton mySpinHeight = new Gtk.SpinButton(0, 250, .1);
				table_main.Attach (mySpinHeight, (uint) x, (uint) ++x, (uint) count, (uint) count +1,
						Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, padding, padding);
				mySpinHeight.Show();
				spinsHeight.Add (mySpinHeight);
			}

			if (useLegsLengthCol)
			{
				Gtk.SpinButton mySpinLegsLength = new Gtk.SpinButton(0, 150, .1);
				table_main.Attach (mySpinLegsLength, (uint) x, (uint) ++x, (uint) count, (uint) count +1,
						Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, padding, padding);
				mySpinLegsLength.Show();
				spinsLegsLength.Add (mySpinLegsLength);
			}

			if (useHipsHeightCol)
			{
				Gtk.SpinButton mySpinHipsHeight = new Gtk.SpinButton(0, 150, .1);
				table_main.Attach (mySpinHipsHeight, (uint) x, (uint) ++x, (uint) count, (uint) count +1,
						Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, padding, padding);
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

		table_main.Show();
		table_main.Visible = true;
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

	void fillTableFromCSV (ArrayList array, bool useHeightCol, bool useLegsLengthCol, bool useHipsHeightCol)
	{
		int i = 0;
		foreach (PersonAddMultipleTable pamt in array)
		{
			((Gtk.Entry) entries[i]).Text = pamt.name;
			((Gtk.RadioButton) radiosM[i]).Active = pamt.maleOrFemale;
			((Gtk.RadioButton) radiosF[i]).Active = ! pamt.maleOrFemale;
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
					error_label_in_session_l[pame.id].Wrap = true;
				}
				else if (pame.errorType == PersonAddMultipleError.ErrorType.INDB)
				{
					error_label_in_db_l[pame.id].Visible = true;
					error_label_in_db_l[pame.id].Justify = Justification.Center;
					error_label_in_db_l[pame.id].Wrap = true;
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
				sex = Constants.F;
				if(((Gtk.RadioButton)radiosM[i]).Active) { sex = Constants.M; }

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
}

