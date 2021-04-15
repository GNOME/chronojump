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
 * Copyright (C) 2004-2020   Xavier de Blas <xaviblas@gmail.com> 
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

	public PersonAddMultipleTable(string name, bool maleOrFemale, double weight) {
		this.name = name;
		this.maleOrFemale = maleOrFemale;
		this.weight = weight;
	}

	~PersonAddMultipleTable() {}
}

//new persons multiple (infinite)
public class PersonAddMultipleWindow
{
	
	[Widget] Gtk.Window person_multiple_infinite;
		
	[Widget] Gtk.VBox vbox_top;
	[Widget] Gtk.Notebook notebook;
	
	[Widget] Gtk.RadioButton radio_csv;
	[Widget] Gtk.RadioButton radio_manually;
	[Widget] Gtk.Box hbox_manually;
	[Widget] Gtk.SpinButton spin_manually;
	
	[Widget] Gtk.Image image_csv_headers;
	[Widget] Gtk.Image image_csv_noheaders;
	[Widget] Gtk.Image image_load;
	[Widget] Gtk.Label label_csv;
	[Widget] Gtk.Label label_name;

	[Widget] Gtk.Button button_manually_create;
	[Widget] Gtk.Table table_headers_1_column;
	[Widget] Gtk.Table table_no_headers_1_column;
	[Widget] Gtk.Table table_headers_2_columns;
	[Widget] Gtk.Table table_no_headers_2_columns;
	
	[Widget] Gtk.Image image_name1;
	[Widget] Gtk.Image image_name2;

	[Widget] Gtk.CheckButton check_headers;
	[Widget] Gtk.CheckButton check_name_1_column;

	[Widget] Gtk.Label label_column_separator;

	//use this to read/write table
	ArrayList entries;
	ArrayList radiosM;
	ArrayList radiosF;
	ArrayList spins;
	
	int rows;
	
	//[Widget] Gtk.ScrolledWindow scrolledwindow;
	[Widget] Gtk.Table table_main;
	[Widget] Gtk.Label label_message;
	
	[Widget] Gtk.Button button_accept;
	
	static PersonAddMultipleWindow PersonAddMultipleWindowBox;

	private Person currentPerson;
	Session currentSession;
	char columnDelimiter;
	int personsCreatedCount;
	string errorExistsString;
	string errorWeightString;
	string errorRepeatedEntryString;

	
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
		PersonAddMultipleWindowBox.tablesVisibility ();

		PersonAddMultipleWindowBox.person_multiple_infinite.Show ();
		
		return PersonAddMultipleWindowBox;
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		PersonAddMultipleWindowBox.person_multiple_infinite.Hide();
		PersonAddMultipleWindowBox = null;
	}
	
	void on_delete_event (object o, DeleteEventArgs args)
	{
		PersonAddMultipleWindowBox.person_multiple_infinite.Hide();
		PersonAddMultipleWindowBox = null;
	}
		
	void putNonStandardIcons() {
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

		label_column_separator.Text = string.Format(Catalog.GetString(
					"Expected column separator character is '<b>{0}</b>'"), columnDelimiter) +
			"\n" + Catalog.GetString("You can change this on Preferences / Language.");
		label_column_separator.UseMarkup = true;

		//label_csv_help.Text =
			//"<b>" + Catalog.GetString("Import persons from an spreadsheet. Eg. Excel, LibreOffice, Google Drive.") + "</b>\n\n" +
			//Catalog.GetString("Open the spreadsheet with the persons data to be added.") + "\n" +
		//	Catalog.GetString("Spreadsheet structure need to have this structure:");
		//label_csv_help.UseMarkup = true;
	}
	
	void tablesVisibility() {
		table_headers_1_column.Visible = false;
		table_no_headers_1_column.Visible = false;
		table_headers_2_columns.Visible = false;
		table_no_headers_2_columns.Visible = false;
		
		if(check_headers.Active) {
			if(check_name_1_column.Active)
				table_headers_1_column.Visible = true;
			else
				table_headers_2_columns.Visible = true;
		} else {
			if(check_name_1_column.Active)
				table_no_headers_1_column.Visible = true;
			else
				table_no_headers_2_columns.Visible = true;
		}
		
		button_accept.Sensitive = false;
	}

	void on_check_headers_toggled (object obj, EventArgs args)
	{
		if(check_headers.Active) {
			image_csv_headers.Visible = true;
			image_csv_noheaders.Visible = false;
			label_csv.Text = Catalog.GetString("CSV file has headers");

		} else {
			image_csv_headers.Visible = false;
			image_csv_noheaders.Visible = true;
			label_csv.Text = Catalog.GetString("CSV file does not have headers");
		}

		tablesVisibility();
	}

	void on_check_name_1_column_toggled (object obj, EventArgs args)
	{
		if(check_name_1_column.Active) {
			image_name1.Visible = true;
			image_name2.Visible = false;
			label_name.Text = Catalog.GetString("Full name in one column");
		} else {
			image_name1.Visible = false;
			image_name2.Visible = true;
			label_name.Text = Catalog.GetString("Full name in two columns");
		}
		
		tablesVisibility();
	}
	
	void on_radio_csv_toggled (object obj, EventArgs args)
	{
		if(radio_csv.Active)
			notebook.CurrentPage = 1;
	}
	void on_radio_manually_toggled (object obj, EventArgs args)
	{
		if(radio_manually.Active)
			notebook.CurrentPage = 0;
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

			List<string> columns = new List<string>();
			using (var reader = new CsvFileReader(fc.Filename))
			{
				reader.ChangeDelimiter(columnDelimiter);
				bool headersActive = check_headers.Active;
				bool name1Column = check_name_1_column.Active;
				int row = 0;
				while (reader.ReadRow(columns))
				{
					string fullname = "";
					string onlyname = "";
					bool maleOrFemale = true;
					double weight = 0;
					int col = 0;
					foreach(string str in columns) {
						//if headers are active do not process first row
						//do not process this first row because weight can be a string
						if(headersActive && row == 0)
							continue;
						
						LogB.Debug(":" + str);

						if(col == 0) {
							if(name1Column)
								fullname = str;
							else
								onlyname = str;
						}
						else if(col == 1 && ! name1Column)
							fullname = onlyname + " " + str;
						else if( (col == 1 && name1Column) || (col == 2 && ! name1Column) ) {
							//female symbols
							if(str == "0" || str == "f" || str == "F")
								maleOrFemale = false;
						}
						else if( (col == 2 && name1Column) || (col == 3 && ! name1Column) ) {
							try {
								weight = Convert.ToDouble(Util.ChangeDecimalSeparator(str));
							} catch {
								string message = Catalog.GetString("Error importing data.");
								if( ! check_headers.Active && row == 0)
									message += "\n" + Catalog.GetString("Seems there's a header row and you have not marked it.");

								new DialogMessage(Constants.MessageTypes.WARNING, message);

								file.Close(); 
								//Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
								fc.Destroy();

								return;
							}
						}
						col ++;
					}
					//if headers are active do not add first row
					if( ! (headersActive && row == 0) ) {
						PersonAddMultipleTable pamt = new PersonAddMultipleTable(Util.MakeValidSQL(fullname), maleOrFemale, weight);
						array.Add(pamt);
					}
					
					row ++;
					LogB.Debug("\n");
				}
			}

			file.Close(); 

			rows = array.Count;
			createEmptyTable();
			fillTableFromCSV(array);
		} 

		//Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
		fc.Destroy();
	}

	void on_button_manually_create_clicked (object obj, EventArgs args) 
	{
		button_manually_create.Sensitive = false;

		rows = Convert.ToInt32(spin_manually.Value);

		createEmptyTable();
	}

	void createEmptyTable()
	{
		vbox_top.Visible = false;
		hbox_manually.Visible = false;

		entries = new ArrayList();
		radiosM = new ArrayList();
		radiosF = new ArrayList();
		spins = new ArrayList();

		Gtk.Label nameLabel = new Gtk.Label("<b>" + Catalog.GetString("Full name") + "</b>");
		Gtk.Label sexLabel = new Gtk.Label("<b>" + Catalog.GetString("Sex") + "</b>");
		Gtk.Label weightLabel = new Gtk.Label("<b>" + Catalog.GetString("Weight") +
			"</b>(" + Catalog.GetString("Kg") + ")" );
		
		nameLabel.UseMarkup = true;
		sexLabel.UseMarkup = true;
		weightLabel.UseMarkup = true;

		nameLabel.Xalign = 0;
		sexLabel.Xalign = 0;
		weightLabel.Xalign = 0;
		
		weightLabel.Show();
		nameLabel.Show();
		sexLabel.Show();
	
		uint padding = 4;	

		table_main.Attach (nameLabel, (uint) 1, (uint) 2, 0, 1, 
				Gtk.AttachOptions.Fill | Gtk.AttachOptions.Expand , Gtk.AttachOptions.Shrink, padding, padding);
		table_main.Attach (sexLabel, (uint) 2, (uint) 3, 0, 1, 
				Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, padding, padding);
		table_main.Attach (weightLabel, (uint) 3, (uint) 4, 0, 1, 
				Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, padding, padding);

		for (int count=1; count <= rows; count ++) {
			Gtk.Label myLabel = new Gtk.Label((count).ToString());
			table_main.Attach (myLabel, (uint) 0, (uint) 1, (uint) count, (uint) count +1, 
					Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, padding, padding);
			myLabel.Show();
			//labels.Add(myLabel);

			Gtk.Entry myEntry = new Gtk.Entry();
			table_main.Attach (myEntry, (uint) 1, (uint) 2, (uint) count, (uint) count +1, 
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
			table_main.Attach (sexBox, (uint) 2, (uint) 3, (uint) count, (uint) count +1, 
					Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, padding, padding);


			Gtk.SpinButton mySpin = new Gtk.SpinButton(0, 300, .1);
			table_main.Attach (mySpin, (uint) 3, (uint) 4, (uint) count, (uint) count +1, 
					Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, padding, padding);
			mySpin.Show();
			spins.Add(mySpin);
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

		table_main.Show();
		table_main.Visible = true;
		notebook.CurrentPage = 0;

		button_accept.Sensitive = true;
	}

	void on_entry_changed (object o, EventArgs args)
	{
		Gtk.Entry entry = o as Gtk.Entry;
		if (o == null)
			return;

		entry.Text = Util.MakeValidSQL(entry.Text);
	}

	void fillTableFromCSV(ArrayList array) {
		int i = 0;
		foreach(PersonAddMultipleTable pamt in array) {
			((Gtk.Entry)entries[i]).Text = pamt.name;
			((Gtk.RadioButton)radiosM[i]).Active = pamt.maleOrFemale;
			((Gtk.RadioButton)radiosF[i]).Active = ! pamt.maleOrFemale;
			((Gtk.SpinButton)spins[i]).Value = pamt.weight;
			i++;
		}
	}

	void on_button_accept_clicked (object o, EventArgs args)
	{
		errorExistsString = "";
		errorWeightString = "";
		errorRepeatedEntryString = "";
		personsCreatedCount = 0;

		Sqlite.Open();
		for (int i = 0; i < rows; i ++) 
			checkEntries(i, ((Gtk.Entry)entries[i]).Text.ToString(), (int) ((Gtk.SpinButton)spins[i]).Value);
		Sqlite.Close();
	
		checkAllEntriesAreDifferent();

		string combinedErrorString = "";
		combinedErrorString = readErrorStrings();
		
		if (combinedErrorString.Length > 0) {
			ErrorWindow.Show(combinedErrorString);
		} else {
			processAllNonBlankRows();
		
			PersonAddMultipleWindowBox.person_multiple_infinite.Hide();
			PersonAddMultipleWindowBox = null;
		}
	}
		
	private void checkEntries(int count, string name, double weight) {
		if(name.Length > 0) {
			bool personExists = Sqlite.Exists (true, Constants.PersonTable, Util.RemoveTilde(name));
			if(personExists) {
				errorExistsString += "[" + (count+1) + "] " + name + "\n";
			}
			if(Convert.ToInt32(weight) == 0) {
				errorWeightString += "[" + (count+1) + "] " + name + "\n";
			}
		}
	}
		
	void checkAllEntriesAreDifferent() {
		ArrayList newNames= new ArrayList();
		for (int i = 0; i < rows; i ++) 
			newNames.Add(((Gtk.Entry)entries[i]).Text.ToString());

		for(int i=0; i < rows; i++) {
			bool repeated = false;
			if(Util.RemoveTilde(newNames[i].ToString()).Length > 0) {
				int j;
				for(j=i+1; j < newNames.Count && !repeated; j++) {
					if( Util.RemoveTilde(newNames[i].ToString()) == Util.RemoveTilde(newNames[j].ToString()) ) {
						repeated = true;
					}
				}
				if(repeated) {
					errorRepeatedEntryString += string.Format("[{0}] {1} - [{2}] {3}\n",
							i+1, newNames[i].ToString(), j, newNames[j-1].ToString());
				}
			}
		}
	}
	
	string readErrorStrings() {
		if (errorExistsString.Length > 0) {
			errorExistsString = "ERROR This person(s) exists in the database:\n" + errorExistsString;
		}
		if (errorWeightString.Length > 0) {
			errorWeightString = "\nERROR weight of this person(s) cannot be 0:\n" + errorWeightString;
		}
		if (errorRepeatedEntryString.Length > 0) {
			errorRepeatedEntryString = "\nERROR this names are repeated:\n" + errorRepeatedEntryString;
		}
		
		return errorExistsString + errorWeightString + errorRepeatedEntryString;
	}

	//inserts all the rows where name is not blank
	//all this names doesn't match with other in the database, and the weights are > 0 ( checked in checkEntries() )
	void processAllNonBlankRows() 
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
				
		List <Person> persons = new List<Person>();
		List <PersonSession> personSessions = new List<PersonSession>();

		DateTime dateTime = DateTime.MinValue;

		//the last is the first for having the first value inserted as currentPerson
		for (int i = rows -1; i >= 0; i --) 
			if(((Gtk.Entry)entries[i]).Text.ToString().Length > 0) 
			{
				sex = Constants.F;
				if(((Gtk.RadioButton)radiosM[i]).Active) { sex = Constants.M; }

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
				
				persons.Add(currentPerson);
						
				weight = (double) ((Gtk.SpinButton)spins[i]).Value;
				personSessions.Add(new PersonSession(
							psID ++,
							currentPerson.UniqueID, currentSession.UniqueID, 
							0, weight, 		//height, weight	
							currentSession.PersonsSportID,
							currentSession.PersonsSpeciallityID,
							currentSession.PersonsPractice,
							"", 			//comments
							Constants.TrochanterToeUndefinedID,
							Constants.TrochanterFloorOnFlexionUndefinedID
							)
						);

				personsCreatedCount ++;
			}
	
		//do the transaction	
		new SqlitePersonSessionTransaction(persons, personSessions);
	}

	public Button Button_accept 
	{
		set {
			button_accept = value;	
		}
		get {
			return button_accept;
		}
	}

	public int PersonsCreatedCount 
	{
		get { return personsCreatedCount; }
	}
	
	public Person CurrentPerson 
	{
		get {
			return currentPerson;
		}
	}

}

