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
using System.IO; 

public class PersonSelectWindow 
{
	[Widget] Gtk.Window person_select_window;
	[Widget] Gtk.Table table1;
	[Widget] Gtk.Button button_select;
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
	public Person SelectedPerson;
	public Gtk.Button FakeButtonAddPerson;
	public Gtk.Button FakeButtonLoadPerson;
	public Gtk.Button FakeButtonEditPerson;
	public Gtk.Button FakeButtonDeletePerson;
	public Gtk.Button FakeButtonDone;

	
	PersonSelectWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "person_select_window.glade", "person_select_window", "chronojump");
		gladeXML.Autoconnect(this);
		
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
		personButtonsSensitive(false);
		vbox_button_delete_confirm.Visible = false;
		
		for (int row_i = 0; row_i < rows; row_i ++) {
			for (int col_i = 0; col_i < cols; col_i ++) 
			{
				if(count >= persons.Count)
					return;
				
				Person p = (Person) persons[count ++];

				PersonPhotoButton ppb = new PersonPhotoButton(p);
				Gtk.Button b = ppb.CreateButton();
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
	
		int personID = PersonPhotoButton.GetPersonID(b);

		LogB.Information("UniqueID: " + personID.ToString());
				
		//TODO: now need to process the signal and close
		foreach(Person p in persons)
			if(p.UniqueID == personID) {
				SelectedPerson = p;
				label_selected_person_name.Text = p.Name;

				personButtonsSensitive(true);
			}
	}
	
	private void personButtonsSensitive(bool sensitive) {
		button_select.Sensitive = sensitive;
		button_edit.Sensitive = sensitive;
		button_delete.Sensitive = sensitive;
	}
	
	
	private void on_button_select_clicked (object o, EventArgs args) {
		FakeButtonDone.Click();
		close_window();
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
	
	//TODO: allow to close with ESC
	private void on_delete_event (object o, DeleteEventArgs args)
	{
		PersonSelectWindowBox.person_select_window.Hide();
		PersonSelectWindowBox = null;
	}
}

//used by PersonSelectWindow
public class PersonPhotoButton
{
	private Person p;

	public PersonPhotoButton (Person p) {
		this.p = p;
	}

	public Gtk.Button CreateButton () {
		Gtk.VBox vbox = new Gtk.VBox();

		Gtk.Image image = new Gtk.Image();
		string photoFile = Util.GetPhotoFileName(true, p.UniqueID);
		if(photoFile != "" && File.Exists(photoFile)) {
			try {
				Pixbuf pixbuf = new Pixbuf (photoFile); //from a file
				image.Pixbuf = pixbuf;
				image.Visible = true;
			}
			catch {
				LogB.Warning("catched while adding photo");
			}
		}

		Gtk.Label label_id = new Gtk.Label(p.UniqueID.ToString());
		label_id.Visible = false; //hide this to the user

		Gtk.Label label_name = new Gtk.Label(p.Name);
		label_name.Visible = true;

		vbox.PackStart(image);
		vbox.PackStart(label_id);
		vbox.PackEnd(label_name, false, false, 1);

		vbox.Show();

		Button b = new Button(vbox);
		b.WidthRequest=150;

		return b;
	}

	public static int GetPersonID (Gtk.Button b) 
	{
		//access the vbox
		Gtk.VBox box = (Gtk.VBox) b.Child;
		
		//access the memebers of vbox
		Array box_elements = box.Children;
		
		//access uniqueID	
		Gtk.Label l = (Gtk.Label) box_elements.GetValue(1); //the ID
		int personID = Convert.ToInt32(l.Text);

		//LogB.Information("UniqueID: " + l.Text.ToString());
		
		//access name
		/*
		l = (Gtk.Label) box_elements.GetValue(2); //the name
		LogB.Information("Name: " + l.Text.ToString());
		*/

		return personID;
	}
}

