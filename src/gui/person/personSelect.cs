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
using System.IO; 

public class PersonSelectWindow 
{
	[Widget] Gtk.Window person_select_window;
	[Widget] Gtk.Notebook notebook;
	[Widget] Gtk.ScrolledWindow scrolled1;
	[Widget] Gtk.Viewport viewport1;
	[Widget] Gtk.Viewport viewport_person_name;
	[Widget] Gtk.Table table1;
	[Widget] Gtk.Button button_edit;
	[Widget] Gtk.Button button_show_all_events;
	[Widget] Gtk.Button button_delete;
	[Widget] Gtk.VBox vbox_button_delete_confirm;
	[Widget] Gtk.Label label_selected_person_name;
	[Widget] Gtk.Label label_confirm;
	[Widget] Gtk.Button button_manage_persons;
	[Widget] Gtk.Image image_manage_persons;
	[Widget] Gtk.Image image_person_new;
	[Widget] Gtk.Image image_persons_new_plus;
	[Widget] Gtk.Image image_person_load;
	[Widget] Gtk.Image image_persons_open_plus;
	[Widget] Gtk.Image image_person_edit;
	[Widget] Gtk.Image image_all_persons_events;
	[Widget] Gtk.Image image_person_delete;
	[Widget] Gtk.Image image_manage_persons_cancel;
	[Widget] Gtk.VBox vbox_corner_controls;
	[Widget] Gtk.Image image_close;
	[Widget] Gtk.Label label_manage_persons;
	[Widget] Gtk.Label label_delete_person;
	[Widget] Gtk.CheckButton check_show_images;
	
	static PersonSelectWindow PersonSelectWindowBox;
	
	private ArrayList persons;
	private int selectedFirstClickPersonID; //contains the uniqueID of person selected on first button click
	public Person SelectedPerson;
	public Gtk.Button FakeButtonAddPerson;
	public Gtk.Button FakeButtonAddPersonMultiple;
	public Gtk.Button FakeButtonLoadPerson;
	public Gtk.Button FakeButtonLoadPersonMultiple;
	public Gtk.Button FakeButtonEditPerson;
	public Gtk.Button FakeButtonPersonShowAllEvents;
	public Gtk.Button FakeButtonDeletePerson;
	public Gtk.Button FakeButtonShowImages;
	public Gtk.Button FakeButtonHideImages;
	public Gtk.Button FakeButtonDone;

	private List<PersonPhotoButton> list_ppb;

	private bool showImages;
	private uint columns;
	
	PersonSelectWindow (Gtk.Window parent, bool raspberry, bool lowHeight)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "person_select_window.glade", "person_select_window", "chronojump");
		gladeXML.Autoconnect(this);
		
		int slidebarSize = 20;
		if(raspberry)
			slidebarSize = 40;

		scrolled1.WidthRequest = 150 * 4 + 8 * 2 + 12 * 2 + slidebarSize; //150 is button width, 8 is padding left and right (4+4), 12 the left and right of scrolled1
		//if showImages (2 columns) will be 308*2 + 4 * 2 + 12 * 2

		int rowsShown = 3;
		if(lowHeight)
			rowsShown = 2;

		//there's no side slidebar for going horizontal, but the last +10 is to have a bit of space for the widget
		scrolled1.HeightRequest = 170 * rowsShown + 8 * 2 + 12 * 2; //170 is button height, 8 is padding top botton (4+4), 12 the top and bottom of scrolled1

		//put an icon to window
		UtilGtk.IconWindow(person_select_window);

		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.WindowColor(person_select_window, Config.ColorBackground);
			//UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_confirm);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_manage_persons);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_delete_person);
			UtilGtk.ContrastLabelsVBox(Config.ColorBackgroundIsDark, vbox_corner_controls);
		}

		label_manage_persons.Text = "<b>" + label_manage_persons.Text + "</b>";
		label_manage_persons.UseMarkup = true;
		label_delete_person.Text = "<b>" + label_delete_person.Text + "</b>";
		label_delete_person.UseMarkup = true;

		person_select_window.Parent = parent;
		
		FakeButtonAddPerson = new Gtk.Button();
		FakeButtonAddPersonMultiple = new Gtk.Button();
		FakeButtonLoadPerson = new Gtk.Button();
		FakeButtonLoadPersonMultiple = new Gtk.Button();
		FakeButtonEditPerson = new Gtk.Button();
		FakeButtonPersonShowAllEvents = new Gtk.Button();
		FakeButtonDeletePerson = new Gtk.Button();
		FakeButtonShowImages = new Gtk.Button();
		FakeButtonHideImages = new Gtk.Button();
		FakeButtonDone = new Gtk.Button();

		Pixbuf pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person_pin.png");
		image_manage_persons.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person_add.png");
		image_person_new.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_group_add.png");
		image_persons_new_plus.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person_outline.png");
		image_person_load.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_group_outline.png");
		image_persons_open_plus.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_edit.png");
		image_person_edit.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_visibility.png");
		image_all_persons_events.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_delete.png");
		image_person_delete.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_cancel.png");
		image_manage_persons_cancel.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_close.png");
		image_close.Pixbuf = pixbuf;
	}
	
	static public PersonSelectWindow Show (Gtk.Window parent, ArrayList persons, Person currentPerson, Gdk.Color colorBackground, bool raspberry, bool lowHeight, bool showImages)
	{
		if (PersonSelectWindowBox == null) {
			PersonSelectWindowBox = new PersonSelectWindow (parent, raspberry, lowHeight);
		}

		PersonSelectWindowBox.persons = persons;
		PersonSelectWindowBox.SelectedPerson = currentPerson;
		PersonSelectWindowBox.viewport_person_name_show_paint();

		PersonSelectWindowBox.notebook.CurrentPage = 0;
		PersonSelectWindowBox.button_manage_persons.Sensitive = true;
		PersonSelectWindowBox.vbox_corner_controls.Sensitive = true;

		PersonSelectWindowBox.showImages = showImages;
		if(showImages)
		{
			PersonSelectWindowBox.check_show_images.Active = true;
			PersonSelectWindowBox.columns = 4;
		} else {
			PersonSelectWindowBox.check_show_images.Active = false;
			PersonSelectWindowBox.columns = 2;
		}

		PersonSelectWindowBox.createTable();

		PersonSelectWindowBox.person_select_window.Show ();

		return PersonSelectWindowBox;
	}

	//from main gui to not change SelectedPerson
	public void Update(ArrayList persons)
	{
		Update(persons, SelectedPerson);
	}
	//from main gui to change SelectedPerson or from this class
	public void Update(ArrayList persons, Person currentPerson)
	{
		this.persons = persons;
		SelectedPerson = currentPerson;
		viewport_person_name_show_paint();

		notebook.CurrentPage = 0;

		LogB.Debug("Removing table");
		table1.Visible = false;
		removeTable();

		LogB.Debug("Recreating table");
		createTable();
		table1.Visible = true;
		table1.Sensitive = true;
		button_manage_persons.Sensitive = true;
		vbox_corner_controls.Sensitive = true;

		if(currentPerson != null)
			assignPersonSelectedStuff(currentPerson);

		if(! person_select_window.Visible)
			person_select_window.Visible = true;
	}

	private void on_check_show_images_toggled (object o, EventArgs args)
	{
		if(check_show_images.Active)
			FakeButtonShowImages.Click();
		else
			FakeButtonHideImages.Click();

		if(check_show_images.Active)
			columns = 4;
		else
			columns = 2;

		Update(persons);
	}

	private void removeTable() 
	{
		Array buttons = table1.Children;
		foreach(Gtk.Button b in buttons)
			table1.Remove(b);
	}

	private void createTable() 
	{
		LogB.Debug("Persons count" + persons.Count.ToString());
		uint padding = 4;	
		uint cols = columns; //each row has 4 columns
		uint rows = Convert.ToUInt32(Math.Floor(persons.Count / (1.0 * cols) ) +1);
		int count = 0;

		if(SelectedPerson == null)
		{
			selectedFirstClickPersonID = -1;
			label_selected_person_name.Text = "";
		}
		else {
			selectedFirstClickPersonID = SelectedPerson.UniqueID;
			label_selected_person_name.Text = "<b>" + SelectedPerson.Name + "</b>";
			label_selected_person_name.UseMarkup = true;
		}

		personButtonsSensitive(false);
		//vbox_button_delete_confirm.Visible = false;
		list_ppb = new List<PersonPhotoButton>();

		for (int row_i = 0; row_i < rows; row_i ++)
		{
			for (int col_i = 0; col_i < cols; col_i ++)
			{
				if(count >= persons.Count)
					return;
				
				Person p = (Person) persons[count ++];

				PersonPhotoButton ppb = new PersonPhotoButton(p.UniqueID, p.Name, check_show_images.Active); //creates the button

				//select currentPerson
				if(selectedFirstClickPersonID != -1 && selectedFirstClickPersonID == p.UniqueID)
				{
					ppb.Select(true);
					assignPersonSelectedStuff(p);
				}

				list_ppb.Add(ppb);
				Gtk.Button b = ppb.Button;

				b.Show();
			
				b.Clicked += new EventHandler(on_button_portrait_clicked);
				b.CanFocus=true;
				
				table1.Attach (b, (uint) col_i, (uint) col_i +1, (uint) row_i, (uint) row_i +1, 
						Gtk.AttachOptions.Fill, 
						Gtk.AttachOptions.Fill, 
						padding, padding);
			}
		}

		table1.ShowAll();
	}
	
	private void on_button_portrait_clicked (object o, EventArgs args)
	{
		LogB.Information("Clicked");

		//access the button
		Button b = (Button) o;

		foreach(PersonPhotoButton ppb in list_ppb)
		{
			if(ppb.Button == b)
			{
				if(ppb.PersonID == selectedFirstClickPersonID)
				{
					close_window();
					return;
				}

				ppb.Select(true);

				foreach(Person p in persons)
					if(p.UniqueID == ppb.PersonID)
						assignPersonSelectedStuff(p);
			}
			else if(ppb.Selected)
				ppb.Select(false);
		}
	}

	private void viewport_person_name_show_paint()
	{
		if(SelectedPerson == null)
			viewport_person_name.Visible = false;
		else {
			UtilGtk.ViewportColor(viewport_person_name, UtilGtk.YELLOW);
			viewport_person_name.Visible = true;
		}
	}

	private void assignPersonSelectedStuff(Person p)
	{
		SelectedPerson = p;
		selectedFirstClickPersonID = p.UniqueID;

		label_selected_person_name.Text = "<b>" + p.Name + "</b>";
		label_selected_person_name.UseMarkup = true;
		personButtonsSensitive(true);
	}

	private void personButtonsSensitive(bool sensitive)
	{
		button_edit.Sensitive = sensitive;
		button_show_all_events.Sensitive = sensitive;
		button_delete.Sensitive = sensitive;
	}

	private void on_button_manage_persons_clicked (object o, EventArgs args)
	{
		notebook.CurrentPage = 1;

		personButtonsSensitive (false);
		button_manage_persons.Sensitive = false;
		vbox_corner_controls.Sensitive = false;
	}

	private void on_button_manage_persons_cancel_clicked (object o, EventArgs args)
	{
		notebook.CurrentPage = 0;

		personButtonsSensitive (SelectedPerson != null);
		button_manage_persons.Sensitive = true;
		vbox_corner_controls.Sensitive = true;
	}

	private void on_button_add_clicked (object o, EventArgs args)
	{
		person_select_window.Visible = false;
		FakeButtonAddPerson.Click();
	}
	private void on_button_load_clicked (object o, EventArgs args) {
		person_select_window.Visible = false;
		FakeButtonLoadPerson.Click();
	}
	private void on_button_person_add_multiple_clicked (object o, EventArgs args)
	{
		person_select_window.Visible = false;
		FakeButtonAddPersonMultiple.Click();
	}
	private void on_button_recuperate_persons_from_session_clicked (object o, EventArgs args)
	{
		person_select_window.Visible = false;
		FakeButtonLoadPersonMultiple.Click();
	}

	private void on_button_edit_clicked (object o, EventArgs args) {
		person_select_window.Visible = false;
		FakeButtonEditPerson.Click();
	}

	private void on_button_show_all_events_clicked (object o, EventArgs args) {
		person_select_window.Visible = false;
		FakeButtonPersonShowAllEvents.Click();
	}

	private void on_button_up_clicked (object o, EventArgs args)
	{
		//vertical_scroll_change(viewport1.Vadjustment.Value - viewport1.Vadjustment.PageSize); //this is not very usable, better go row by row

		//row by row
		vertical_scroll_change(viewport1.Vadjustment.Value - 170 + 8); //a row + its padding (another solution could be viewport height / rows)
	}

	private void on_button_down_clicked (object o, EventArgs args)
	{
		//vertical_scroll_change(viewport1.Vadjustment.Value + viewport1.Vadjustment.PageSize); //this is not very usable, better go row by row

		//row by row
		vertical_scroll_change(viewport1.Vadjustment.Value + 170 + 8); //a row + its padding
	}

	private void vertical_scroll_change (double newValue)
	{
		int min = 0;
		double max = viewport1.Vadjustment.Upper - viewport1.Vadjustment.PageSize;

		if(newValue < min)
			newValue = min;
		else if(newValue > max)
			newValue = max;

		viewport1.Vadjustment.Value = newValue;
	}

	//delete person stuff	
	private void on_button_delete_clicked (object o, EventArgs args) {
		button_delete_confirm_focus(true, false);
	}
	private void on_button_delete_no_clicked (object o, EventArgs args) {
		button_delete_confirm_focus(false, true);
	}
	private void on_button_delete_yes_clicked (object o, EventArgs args) {
		FakeButtonDeletePerson.Click();
	}
	private void button_delete_confirm_focus(bool doFocus, bool sensitivePersonButtons)
	{
		if(doFocus)
			notebook.CurrentPage = 2;
		else
			notebook.CurrentPage = 0;

		//vbox_button_delete_confirm.Visible = doFocus;
		table1.Sensitive = ! doFocus;
		button_manage_persons.Sensitive = ! doFocus;
		vbox_corner_controls.Sensitive = ! doFocus;

		personButtonsSensitive(sensitivePersonButtons);
	}
	//end of delete person stuff	


	private void close_window()
	{
		FakeButtonDone.Click(); //change person on main gui

		PersonSelectWindowBox.person_select_window.Hide();
		PersonSelectWindowBox = null;
	}
	
	//ESC is enabled
	private void on_button_close_clicked (object o, EventArgs args)
	{
		close_window();
	}
	
	private void on_delete_event (object o, DeleteEventArgs args)
	{
		args.RetVal = true;
		close_window();
	}
}

//used by PersonSelectWindow
public class PersonPhotoButton
{
	private int personID;
	private string personName;
	private Gtk.Button button;
	public bool Selected;

	//constructors -------------------------------

	/*
	//undefined
	public PersonPhotoButton ()
	{
		personID = -1;
		personName = "";
		button = new Gtk.Button();
		Selected = false;
	}
	*/

	//used to create button
	public PersonPhotoButton (int id, string name, bool showImage)
	{
		personID = id;
		personName = name;

		createButton(showImage);
		Selected = false;
	}

	//used to get button on clicking
	public PersonPhotoButton (Gtk.Button button)
	{
		this.button = button;
		assignPersonData();
	}

	//public methods -------------------------------

	public void Select (bool select)
	{
		Array box_elements = getButtonBoxElements(button);

		//image
		Gtk.Image image = (Gtk.Image) box_elements.GetValue(0); //the image

		Gtk.Viewport viewport = (Gtk.Viewport) box_elements.GetValue(2); //the name

		if(select)
			UtilGtk.ViewportColor(viewport, UtilGtk.YELLOW);
		else
			UtilGtk.ViewportColorDefault(viewport);

		Selected = select;
	}


	//private methods -------------------------------

	private void assignPersonData ()
	{
		Array box_elements = getButtonBoxElements(button);

		//access uniqueID
		Gtk.Label l = (Gtk.Label) box_elements.GetValue(1); //the ID
		personID = Convert.ToInt32(l.Text);
		//LogB.Information("UniqueID: " + l.Text.ToString());

		//access name
		Gtk.Viewport v = (Gtk.Viewport) box_elements.GetValue(2); //the name
		l = (Gtk.Label) v.Child; //the name
		personName = l.Text;
		//LogB.Information("Name: " + l.Text.ToString());
	}

	private void createButton (bool showImage)
	{
		Gtk.VBox vbox = new Gtk.VBox();

		Gtk.Image image = new Gtk.Image();
		addUserPhotoIfExists(image);
		if(showImage)
			image.HeightRequest = 150;
		image.Visible = showImage;

		Gtk.Label label_select = new Gtk.Label("Select !");
		label_select.Visible = false; //hide this to the user until button is clicked first time

		Gtk.Label label_id = new Gtk.Label(personID.ToString());
		label_id.Visible = false; //hide this to the user

		Gtk.Viewport viewport = new Gtk.Viewport();
		UtilGtk.ViewportColorDefault(viewport);
		Gtk.Label label_name = new Gtk.Label(personName);
		label_name.LineWrap = true;

		if(showImage)
			label_name.WidthRequest = 140; //same width as label_selected_person_name, to ensure contents are correctly shown on both
		else
			label_name.WidthRequest = 298;

		label_name.HeightRequest = -1;
		label_name.LineWrapMode = Pango.WrapMode.Word;
		label_name.Visible = true;
		label_name.Show();
		viewport.Add(label_name);
		viewport.Show();

		vbox.PackStart(image); 				//0
		vbox.PackStart(label_id); 			//1
		vbox.PackEnd(viewport, false, false, 1); 	//2 (contains label_name)

		vbox.Show();

		button = new Button(vbox);

		if(showImage)
		{
			button.WidthRequest = 150; //commenting this will make columns longer with enough space for labels
			button.HeightRequest = 170; //commenting this will make the rows be of different height depending on image and lines of text
		}
		else
			button.WidthRequest = 308; //commenting this will make columns longer with enough space for labels
	}

	private Array getButtonBoxElements (Gtk.Button button)
	{
		//access the vbox
		Gtk.VBox box = (Gtk.VBox) button.Child;

		/*
		LogB.Information("printing children");
		foreach(Gtk.Widget w in box.Children)
			LogB.Information(w.ToString());
		*/

		//access the members of vbox
		return box.Children;
	}

	private void addUserPhotoIfExists (Gtk.Image image)
	{
		string photoFile = Util.UserPhotoURL(true, personID);

		if(photoFile != "" && File.Exists(photoFile))
			assignPhotoToPixbuf(image, true, photoFile);
		else {
			if(Config.ColorBackgroundIsDark)
				assignPhotoToPixbuf(image, false, Util.GetImagePath(false) + "image_no_photo_yellow.png");
			else
				assignPhotoToPixbuf(image, false, Util.GetImagePath(false) + "image_no_photo.png");
		}
	}

	private void assignPhotoToPixbuf (Gtk.Image image, bool fromFile, string photoFile)
	{
		Pixbuf pixbuf;
		try {
			if(fromFile)
				pixbuf = new Pixbuf (photoFile); //from a file
			else
				pixbuf = new Pixbuf (null, photoFile); //from assemblies

			image.Pixbuf = pixbuf;
		}
		catch {
			LogB.Warning("catched while assigning image: " + photoFile);
		}
	}

	//------------------ accessors

	public int PersonID
	{
		get { return personID; }
	}

	public Gtk.Button Button
	{
		get { return button; }
	}
}

