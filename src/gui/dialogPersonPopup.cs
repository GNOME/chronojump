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
	[Widget] Gtk.Image image;
	[Widget] Gtk.Label label_rfid;
	[Widget] Gtk.Frame frame_tasks_parametrized;
	[Widget] Gtk.Frame frame_tasks_free;
	[Widget] Gtk.VBox vbox_tasks_parametrized;
	[Widget] Gtk.VBox vbox_tasks_free;

	private List<Gtk.CheckButton> list_checks;
	private List<Gtk.Button> list_buttons;
	private List<int> list_tasks_id;
	private List<int> list_buttons_id;

	private List<Task> list_tasks;
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
		list_tasks = tasks;

		string photoFile = Util.GetPhotoFileName(false, personID);
		if(File.Exists(photoFile)) {
			try {
				Pixbuf pixbuf = new Pixbuf (photoFile); //from a file
				image.Pixbuf = pixbuf;
			} catch {
				//on windows there are problem using the fileNames that are not on temp
				string tempFileName = Path.Combine(Path.GetTempPath(), Constants.PhotoSmallTemp +
						Util.GetMultimediaExtension(Constants.MultimediaItems.PHOTO));
				File.Copy(photoFile, tempFileName, true);
				Pixbuf pixbuf = new Pixbuf (tempFileName);
				image.Pixbuf = pixbuf;
			}
		}

		list_checks = new List<Gtk.CheckButton>();
		list_buttons = new List<Gtk.Button>();
		list_tasks_id = new List<int>();
		list_buttons_id = new List<int>();
		taskActive = new Task();
		Fake_button_start_task = new Gtk.Button();

		bool task_parametrized_exist = false;
		bool task_free_exist = false;
		foreach(Task t in tasks)
		{
			Gtk.Label l = new Gtk.Label(t.ToString());
			HBox hbox = new Gtk.HBox(false, 12);
			Button button_do;

			if(t.Type == 'P')
			{
				button_do = new Gtk.Button("Inicia");
				button_do.Clicked += new EventHandler(button_clicked);
				hbox.PackStart(button_do, false, false, 0);
				hbox.PackStart(l, false, false, 0);
				vbox_tasks_parametrized.PackStart(hbox, false, false, 0);
				task_parametrized_exist = true;
			} else // 'F'
			{
				button_do = new Gtk.Button("Fet!");
				button_do.Clicked += new EventHandler(button_clicked);
				hbox.PackStart(l, false, false, 0);
				hbox.PackStart(button_do, false, false, 0);
				vbox_tasks_free.PackStart(hbox, false, false, 0);
				task_free_exist = true;
			}

			list_buttons.Add(button_do);
			list_tasks_id.Add(t.Id);
			list_buttons_id.Add(t.Id);
		}
		if(task_parametrized_exist)
			vbox_tasks_parametrized.ShowAll();
		else
			frame_tasks_parametrized.Visible = false;

		if(task_free_exist)
			vbox_tasks_free.ShowAll();
		else
			frame_tasks_free.Visible = false;
	}

	private void button_clicked(object o, EventArgs args)
	{
		Button buttonClicked = o as Button;
		if (o == null)
			return;

		int count = 0;
		foreach(Gtk.Button button in list_buttons)
		{
			if(button == buttonClicked)
			{
				LogB.Information("Clicked button" + count.ToString());
				if(list_tasks[count].Type == 'P')
				{
					taskActive = list_tasks[count];
					Fake_button_start_task.Click();
				} else { // 'F'
					Json json = new Json();
					json.UpdateTask(list_tasks_id[count], 1);
					button.Sensitive = false;
				}
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
