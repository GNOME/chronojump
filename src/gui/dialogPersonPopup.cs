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
 * Copyright (C) 2016-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO; 
using System.Collections.Generic; //List
using Gtk;
using Gdk;
//using Glade;

public class DialogPersonPopup
{
	Gtk.Dialog dialog_person_popup;
	Gtk.Label label_name;
	Gtk.Image image_person;
	Gtk.Image image_person_logout;
	Gtk.Image image_new_tasks_other_stations;
	Gtk.CheckButton checkbutton_autologout;
	Gtk.Image image_close;
	Gtk.Label label_rfid;
	Gtk.VBox vbox_tasks_parametrized;

	Gtk.Label label_network_devices;
	Gtk.Label label_server_connected;
	Gtk.Image image_server_connected_yes;
	Gtk.Image image_server_connected_no;
	Gtk.Label label_other_stations;

	private List<Task> list_tasks_fixed; //This list has "R,L" separated
	private List<Gtk.Button> list_buttons_start;
	private List<Gtk.Button> list_buttons_done;
	private List<int> list_buttons_done_id; //this has right id to mark task (also R,L) done
	private List<Gtk.HBox> list_hboxs_row; //to unsensitive when done!

	private Task taskActive;
	public Button Fake_button_start_task;
	public Button Fake_button_person_logout;
	public Button Fake_button_person_autologout_changed;
	public bool Visible;
	public bool Autologout;
	private bool compujumpDjango;

	public DialogPersonPopup (int personID, string name, string rfid,
			List<Task> tasks, List<StationCount> stationsCount,
			string networkDevices, bool serverConnected, bool Autologout,
			bool compujumpDjango, bool compujumpHideTaskDone)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "dialog_person_popup.glade", "dialog_person_popup", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "dialog_person_popup.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);

		//put an icon to window
		UtilGtk.IconWindow(dialog_person_popup);

		//manage window color
		if(! Config.UseSystemColor)
			UtilGtk.DialogColor(dialog_person_popup, Config.ColorBackground);

		Visible = true;
		this.Autologout = Autologout;
		this.compujumpDjango = compujumpDjango;

		//1) Show top of the window widgets
		label_name.Text = "<b>" + name + "</b>";
		label_name.UseMarkup = true;
		label_rfid.Text = rfid;

		Pixbuf pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_close.png");
		image_close.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person_logout.png");
		image_person_logout.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "new.png");
		image_new_tasks_other_stations.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "gtk-apply.png");
		image_server_connected_yes.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "gtk-cancel.png");
		image_server_connected_no.Pixbuf = pixbuf;

		label_network_devices.Text = networkDevices;
		if(serverConnected)
		{
			label_server_connected.Text = "Server is connected";
			image_server_connected_yes.Visible = true;
			image_server_connected_no.Visible = false;
		} else {
			label_server_connected.Text = Constants.ServerDisconnectedMessage();
			image_server_connected_yes.Visible = false;
			image_server_connected_no.Visible = true;
		}

		//string photoFile = Util.GetPhotoFileName(false, personID);
		string photoFile = Util.UserPhotoURL(false, personID);
		if(File.Exists(photoFile))
		{
			try {
				pixbuf = new Pixbuf (photoFile); //from a file
				image_person.Pixbuf = pixbuf;
			} catch {
				string extension = Util.GetMultimediaExtension(photoFile);
				//on windows there are problem using the fileNames that are not on temp
				string tempFileName = Path.Combine(Path.GetTempPath(),
						Constants.PhotoSmallTemp + extension);
				File.Copy(photoFile, tempFileName, true);
				pixbuf = new Pixbuf (tempFileName);
				image_person.Pixbuf = pixbuf;
			}
		}

		//2) Show tasks stuff
		list_tasks_fixed = new List<Task>();
		list_hboxs_row = new List<Gtk.HBox>();
		list_buttons_start = new List<Gtk.Button>();
		list_buttons_done = new List<Gtk.Button>();
		list_buttons_done_id = new List<int>();
		taskActive = null;
		Fake_button_start_task = new Gtk.Button();
		Fake_button_person_logout = new Gtk.Button();
		Fake_button_person_autologout_changed = new Gtk.Button();

		checkbutton_autologout.Active = Autologout;

		bool task_parametrized_exist = false;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_capture_big.png");

		Gtk.HBox hboxRow;
		foreach(Task tAbstract in tasks)
		{
			//TODO: cast depending on type, not all them encoder ;)
			//if POWERGRAVITATORY || POWERINERTIAL
			TaskEncoder t = (TaskEncoder) tAbstract;

			hboxRow = new Gtk.HBox(false, 10);

			if(t.Laterality == "R,L")
			{
				Gtk.VBox vboxRL = new Gtk.VBox(false, 4);

				TaskEncoder taskCopy = new TaskEncoder (t.Id, t.PersonId, t.ExerciseId, t.ExerciseName,
					t.Sets, t.Nreps, t.Load, t.Speed, t.PercentMaxSpeed,
					"R", t.Comment);
				Gtk.HBox hboxStartAndLabel = createHBoxStartAndLabel(taskCopy, pixbuf);
				vboxRL.PackStart(hboxStartAndLabel, false, false, 0);

				taskCopy = new TaskEncoder (t.Id, t.PersonId, t.ExerciseId, t.ExerciseName,
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
			if(! compujumpHideTaskDone)
			{
				Gtk.Button button_done = new Gtk.Button("Fet!");
				button_done.Clicked += new EventHandler(button_done_clicked);
				hboxRow.PackEnd(button_done, false, false, 0);
				list_buttons_done.Add(button_done);
				list_buttons_done_id.Add(t.Id);
			}

			list_hboxs_row.Add(hboxRow);
			vbox_tasks_parametrized.PackStart(hboxRow, false, false, 0);
		}

		if(! task_parametrized_exist)
		{
			Gtk.Label label = new Gtk.Label("There are no pending tasks on this station.");
			vbox_tasks_parametrized.PackStart(label, false, false, 0);
		}

		vbox_tasks_parametrized.ShowAll();

		//3) Show other stations tasks
		string sep = "";
		string stationsString = "";
		foreach(StationCount sc in stationsCount)
		{
			stationsString += sep + sc.ToString();
			sep = ", ";
		}

		if(stationsString == "")
			label_other_stations.Text = "There are no tasks at other stations";
		else
		{
			//label_other_stations.Text = "There are task at this stations:" + "\n\n" + stationsString;
			label_other_stations.Text = stationsString;
			image_new_tasks_other_stations.Visible = true;
		}

		dialog_person_popup.Show();
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

				JsonCompujump json = new JsonCompujump(compujumpDjango);
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

	public void on_button_person_logout_clicked (object obj, EventArgs args)
	{
		Fake_button_person_logout.Click();
	}

	private void on_checkbutton_autologout_toggled (object o, EventArgs args)
	{
		Autologout = checkbutton_autologout.Active;
		Fake_button_person_autologout_changed.Click();
	}

	public void on_button_close_clicked (object obj, EventArgs args)
	{
		Visible = false;
		dialog_person_popup.Destroy ();
	}

	private void on_delete_event (object o, DeleteEventArgs args)
	{
		Visible = false;
		dialog_person_popup.Destroy ();
	}

	//call this if a new person put his rfid code before showing it's data
	public void DestroyDialog ()
	{
		LogB.Information("Destroying dialogPersonPopup");

		Visible = false;
		dialog_person_popup.Destroy ();

		LogB.Information("Destroyed dialogPersonPopup");
	}

	public Task TaskActive
	{
		get { return taskActive; }
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		dialog_person_popup = (Gtk.Dialog) builder.GetObject ("dialog_person_popup");
		label_name = (Gtk.Label) builder.GetObject ("label_name");
		image_person = (Gtk.Image) builder.GetObject ("image_person");
		image_person_logout = (Gtk.Image) builder.GetObject ("image_person_logout");
		image_new_tasks_other_stations = (Gtk.Image) builder.GetObject ("image_new_tasks_other_stations");
		checkbutton_autologout = (Gtk.CheckButton) builder.GetObject ("checkbutton_autologout");
		image_close = (Gtk.Image) builder.GetObject ("image_close");
		label_rfid = (Gtk.Label) builder.GetObject ("label_rfid");
		vbox_tasks_parametrized = (Gtk.VBox) builder.GetObject ("vbox_tasks_parametrized");
		label_network_devices = (Gtk.Label) builder.GetObject ("label_network_devices");
		label_server_connected = (Gtk.Label) builder.GetObject ("label_server_connected");
		image_server_connected_yes = (Gtk.Image) builder.GetObject ("image_server_connected_yes");
		image_server_connected_no = (Gtk.Image) builder.GetObject ("image_server_connected_no");
		label_other_stations = (Gtk.Label) builder.GetObject ("label_other_stations");
	}
}
