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
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
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
	[Widget] Gtk.Viewport viewport1;
	[Widget] Gtk.Table table1;
	[Widget] Gtk.Button button_edit;
	[Widget] Gtk.Button button_delete;
	[Widget] Gtk.VBox vbox_button_delete_confirm;
	[Widget] Gtk.Label label_selected_person_name;
	[Widget] Gtk.Button button_add;
	[Widget] Gtk.Button button_load;
	[Widget] Gtk.Image image_person_new;
	[Widget] Gtk.Image image_person_load;
	
	static PersonSelectWindow PersonSelectWindowBox;
	
	private ArrayList persons;
	private int selectedFirstClickPersonID; //contains the uniqueID of person selected on first button click
	public Person SelectedPerson;
	public Gtk.Button FakeButtonAddPerson;
	public Gtk.Button FakeButtonLoadPerson;
	public Gtk.Button FakeButtonEditPerson;
	public Gtk.Button FakeButtonDeletePerson;
	public Gtk.Button FakeButtonDone;

	private List<PersonPhotoButton> list_ppb;

	
	PersonSelectWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "person_select_window.glade", "person_select_window", "chronojump");
		gladeXML.Autoconnect(this);
		
		viewport1.HeightRequest = 170 * 3 + 8 * 2 + 4 * 2; //170 is button height, 8 is padding top botton (4+4), 4 the top and bottom of viewport1

		//put an icon to window
		UtilGtk.IconWindow(person_select_window);
		person_select_window.Parent = parent;
		
		FakeButtonAddPerson = new Gtk.Button();
		FakeButtonLoadPerson = new Gtk.Button();
		FakeButtonEditPerson = new Gtk.Button();
		FakeButtonDeletePerson = new Gtk.Button();
		FakeButtonDone = new Gtk.Button();

		Pixbuf pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person_add.png");
		image_person_new.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person_outline.png");
		image_person_load.Pixbuf = pixbuf;
	}
	
	static public PersonSelectWindow Show (Gtk.Window parent, ArrayList persons)
	{
		if (PersonSelectWindowBox == null) {
			PersonSelectWindowBox = new PersonSelectWindow (parent);
		}

		PersonSelectWindowBox.persons = persons;
		
		PersonSelectWindowBox.createTable();
		
		PersonSelectWindowBox.person_select_window.Show ();
		
		return PersonSelectWindowBox;
	}

	public void Update(ArrayList persons) {
		this.persons = persons;
		
		LogB.Debug("Removing table");
		table1.Visible = false;
		removeTable();

		LogB.Debug("Recreating table");
		createTable();
		table1.Visible = true;
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
		uint cols = 4; //each row has 4 columns
		uint rows = Convert.ToUInt32(Math.Floor(persons.Count / (1.0 * cols) ) +1);
		int count = 0;
		
		label_selected_person_name.Text = "";
		SelectedPerson = null;
		selectedFirstClickPersonID = -1;
		personButtonsSensitive(false);
		vbox_button_delete_confirm.Visible = false;
		list_ppb = new List<PersonPhotoButton>();

		for (int row_i = 0; row_i < rows; row_i ++) {
			for (int col_i = 0; col_i < cols; col_i ++) 
			{
				if(count >= persons.Count)
					return;
				
				Person p = (Person) persons[count ++];

				PersonPhotoButton ppb = new PersonPhotoButton(p.UniqueID, p.Name); //creates the button
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
					FakeButtonDone.Click();
					close_window();
					return;
				}

				ppb.Select(true);

				foreach(Person p in persons)
					if(p.UniqueID == ppb.PersonID)
					{
						SelectedPerson = p;
						label_selected_person_name.Text = "<b>" + p.Name + "</b>";
						label_selected_person_name.UseMarkup = true;
						personButtonsSensitive(true);
						selectedFirstClickPersonID = p.UniqueID;
					}
			}
			else if(ppb.Selected)
				ppb.Select(false);
		}
	}
	
	private void personButtonsSensitive(bool sensitive)
	{
		button_edit.Sensitive = sensitive;
		button_delete.Sensitive = sensitive;
	}
	
	private void on_button_add_clicked (object o, EventArgs args) {
		FakeButtonAddPerson.Click();
	}
	private void on_button_load_clicked (object o, EventArgs args) {
		FakeButtonLoadPerson.Click();
	}
	private void on_button_edit_clicked (object o, EventArgs args) {
		FakeButtonEditPerson.Click();
	}

	private void on_button_up_clicked (object o, EventArgs args)
	{
		vertical_scroll_change(viewport1.Vadjustment.Value - viewport1.Vadjustment.PageSize);
	}

	private void on_button_down_clicked (object o, EventArgs args)
	{
		vertical_scroll_change(viewport1.Vadjustment.Value + viewport1.Vadjustment.PageSize);
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
		Button_delete_confirm_focus(true, false);
	}
	private void on_button_delete_no_clicked (object o, EventArgs args) {
		Button_delete_confirm_focus(false, true);
	}
	private void on_button_delete_yes_clicked (object o, EventArgs args) {
		FakeButtonDeletePerson.Click();
	}
	public void Button_delete_confirm_focus(bool doFocus, bool sensitivePersonButtons)
	{
		vbox_button_delete_confirm.Visible = doFocus;
		table1.Sensitive = ! doFocus;
		button_add.Sensitive = ! doFocus;
		button_load.Sensitive = ! doFocus;
		
		personButtonsSensitive(sensitivePersonButtons);
	}
	//end of delete person stuff	


	private void close_window() {	
		PersonSelectWindowBox.person_select_window.Hide();
		PersonSelectWindowBox = null;
	}
	
	//ESC is enabled
	private void on_button_cancel_clicked (object o, EventArgs args) {
		close_window();
	}
	
	private void on_delete_event (object o, DeleteEventArgs args)
	{
		PersonSelectWindowBox.person_select_window.Hide();
		PersonSelectWindowBox = null;
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
	public PersonPhotoButton (int id, string name)
	{
		personID = id;
		personName = name;

		createButton();
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

		if(select)
			assignPhotoToPixbuf(image, false, Util.GetImagePath(false) + "image_selected.png");
		else
			addUserPhotoIfExists(image);

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
		l = (Gtk.Label) box_elements.GetValue(2); //the name
		personName = l.Text;
		//LogB.Information("Name: " + l.Text.ToString());
	}

	private void createButton ()
	{
		Gtk.VBox vbox = new Gtk.VBox();

		Gtk.Image image = new Gtk.Image();
		addUserPhotoIfExists(image);
		image.HeightRequest = 150;
		image.Visible = true;

		Gtk.Label label_select = new Gtk.Label("Select !");
		label_select.Visible = false; //hide this to the user until button is clicked first time

		Gtk.Label label_id = new Gtk.Label(personID.ToString());
		label_id.Visible = false; //hide this to the user

		Gtk.Label label_name = new Gtk.Label(personName);
		label_name.Visible = true;

		vbox.PackStart(image); 				//0
		vbox.PackStart(label_id); 			//1
		vbox.PackEnd(label_name, false, false, 1); 	//2

		vbox.Show();

		button = new Button(vbox);
		button.WidthRequest = 150;
		button.HeightRequest = 170;
	}

	private Array getButtonBoxElements (Gtk.Button b)
	{
		//access the vbox
		Gtk.VBox box = (Gtk.VBox) b.Child;

		/*
		LogB.Information("printing children");
		foreach(Gtk.Widget w in box.Children)
			LogB.Information(w.ToString());
		*/

		//access the memebers of vbox
		return box.Children;
	}

	private void addUserPhotoIfExists (Gtk.Image image)
	{
		string photoFile = Util.UserPhotoURL(true, personID);

		if(photoFile != "" && File.Exists(photoFile))
			assignPhotoToPixbuf(image, true, photoFile);
		else
			assignPhotoToPixbuf(image, false, Util.GetImagePath(false) + "image_no_photo.png");
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

