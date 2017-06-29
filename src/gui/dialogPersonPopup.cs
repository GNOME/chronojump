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
 * Copyright (C) 2016-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO; 
using System.Collections.Generic; //List
using Gtk;
using Gdk;
using Glade;

public class DialogPersonPopup
{
	[Widget] Gtk.Dialog dialog_person_popup;
	[Widget] Gtk.Label label_name;
	[Widget] Gtk.Image image_person;
	[Widget] Gtk.Image image_close;
	[Widget] Gtk.Label label_rfid;
	[Widget] Gtk.Frame frame_tasks_parametrized;
	[Widget] Gtk.VBox vbox_tasks_parametrized;

	private List<Task> list_tasks_fixed; //This list has "R,L" separated
	private List<Gtk.Button> list_buttons_start;
	private List<Gtk.Button> list_buttons_done;
	private List<int> list_buttons_done_id; //this has right id to mark task (also R,L) done
	private List<Gtk.HBox> list_hboxs_row; //to unsensitive when done!

	private Task taskActive;
	public Button Fake_button_start_task;

	public DialogPersonPopup (int personID, string name, string rfid, List<Task> tasks)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "dialog_person_popup.glade", "dialog_person_popup", null);
		gladeXML.Autoconnect(this);

		//put an icon to window
		UtilGtk.IconWindow(dialog_person_popup);

		label_name.Text = "<b>" + name + "</b>";
		label_name.UseMarkup = true;
		label_rfid.Text = rfid;

		Pixbuf pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_close.png");
		image_close.Pixbuf = pixbuf;

		string photoFile = Util.GetPhotoFileName(false, personID);
		if(File.Exists(photoFile))
		{
			try {
				pixbuf = new Pixbuf (photoFile); //from a file
				image_person.Pixbuf = pixbuf;
			} catch {
				//on windows there are problem using the fileNames that are not on temp
				string tempFileName = Path.Combine(Path.GetTempPath(), Constants.PhotoSmallTemp +
						Util.GetMultimediaExtension(Constants.MultimediaItems.PHOTO));
				File.Copy(photoFile, tempFileName, true);
				pixbuf = new Pixbuf (tempFileName);
				image_person.Pixbuf = pixbuf;
			}
		}

		list_tasks_fixed = new List<Task>();
		list_hboxs_row = new List<Gtk.HBox>();
		list_buttons_start = new List<Gtk.Button>();
		list_buttons_done = new List<Gtk.Button>();
		list_buttons_done_id = new List<int>();
		taskActive = new Task();
		Fake_button_start_task = new Gtk.Button();

		bool task_parametrized_exist = false;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_capture.png");

		Gtk.HBox hboxRow;
		foreach(Task t in tasks)
		{
			hboxRow = new Gtk.HBox(false, 10);

			if(t.Laterality == "R,L")
			{
				Gtk.VBox vboxRL = new Gtk.VBox(false, 4);

				Task taskCopy = new Task(t.Id, t.PersonId, t.StationId, t.ExerciseId, t.ExerciseName,
					t.Sets, t.Nreps, t.Load, t.Speed, t.PercentMaxSpeed,
					"R", t.Comment);
				Gtk.HBox hboxStartAndLabel = createHBoxStartAndLabel(taskCopy, pixbuf);
				vboxRL.PackStart(hboxStartAndLabel, false, false, 0);

				taskCopy = new Task(t.Id, t.PersonId, t.StationId, t.ExerciseId, t.ExerciseName,
					t.Sets, t.Nreps, t.Load, t.Speed, t.PercentMaxSpeed,
					"L", t.Comment);
				hboxStartAndLabel = createHBoxStartAndLabel(taskCopy, pixbuf);
				vboxRL.PackStart(hboxStartAndLabel, false, false, 0);

				Gtk.VSeparator vsep = new Gtk.VSeparator();
				hboxRow.PackStart(vsep, false, false, 0);
				hboxRow.PackStart(vboxRL, false, false, 0);
			} else {
				Gtk.HBox hboxStartAndLabel = createHBoxStartAndLabel(t, pixbuf);
				hboxRow.PackStart(hboxStartAndLabel, false, false, 0);
			}

			task_parametrized_exist = true;

			//create button_done (shared on R,L)
			Gtk.Button button_done = new Gtk.Button("Fet!");
			button_done.Clicked += new EventHandler(button_done_clicked);
			hboxRow.PackEnd(button_done, false, false, 0);
			list_buttons_done.Add(button_done);
			list_buttons_done_id.Add(t.Id);
			list_hboxs_row.Add(hboxRow);

			vbox_tasks_parametrized.PackStart(hboxRow, false, false, 0);
		}

		if(task_parametrized_exist)
			vbox_tasks_parametrized.ShowAll();
		else
			frame_tasks_parametrized.Visible = false;
	}

	private Gtk.HBox createHBoxStartAndLabel(Task t, Pixbuf pixbuf)
	{
		Gtk.Label l = new Gtk.Label(t.ToString());
		HBox hbox = new Gtk.HBox(false, 10);
		Button button_start;

		Gtk.Image image = new Gtk.Image();
		image.Pixbuf = pixbuf;

		button_start = new Gtk.Button(image);
		button_start.Clicked += new EventHandler(button_start_clicked);

		hbox.PackStart(button_start, false, false, 0);
		hbox.PackStart(l, false, false, 0);

		list_tasks_fixed.Add(t);
		LogB.Information("createBoxStart....");
		LogB.Information(t.ToString());
		list_buttons_start.Add(button_start);

		return hbox;
	}

	private void button_start_clicked(object o, EventArgs args)
	{
		Button buttonClicked = o as Button;
		if (o == null)
			return;

		int count = 0;
		foreach(Gtk.Button button in list_buttons_start)
		{
			if(button == buttonClicked)
			{
				LogB.Information("Clicked button start: " + count.ToString());

				taskActive = list_tasks_fixed[count];
				Fake_button_start_task.Click();

				return;
			}
			count ++;
		}
	}

	private void button_done_clicked(object o, EventArgs args)
	{
		Button buttonClicked = o as Button;
		if (o == null)
			return;

		int count = 0;
		foreach(Gtk.Button button in list_buttons_done)
		{
			if(button == buttonClicked)
			{
				LogB.Information("Clicked button done: " + count.ToString());

				Json json = new Json();
				json.UpdateTask(list_buttons_done_id[count], 1);

				//button.Sensitive = false;
				//list_buttons_start[count].Sensitive = false;
				//list_labels[count].Sensitive = false;
				list_hboxs_row[count].Sensitive = false;

				return;
			}
			count ++;
		}
	}

	public void on_button_close_clicked (object obj, EventArgs args)
	{
		dialog_person_popup.Destroy ();
	}

	private void on_delete_event (object o, DeleteEventArgs args)
	{
		dialog_person_popup.Destroy ();
	}

	//call this if a new person put his rfid code before showing it's data
	public void DestroyDialog ()
	{
		LogB.Information("Destroying dialogPersonPopup");
		dialog_person_popup.Destroy ();
		LogB.Information("Destroyed dialogPersonPopup");
	}

	public Task TaskActive
	{
		get { return taskActive; }
	}
}
