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
	[Widget] Gtk.TextView textview_task;
	[Widget] Gtk.VBox vbox_tasks;

	private List<Gtk.CheckButton> list_checks;
	private List<int> list_tasks_id;

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

		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = "";
		foreach(Task t in tasks)
			tb.Text += t.ToString() + "\n";

		textview_task.Buffer = tb;

		list_checks = new List<Gtk.CheckButton>();
		list_tasks_id = new List<int>();
		foreach(Task t in tasks)
		{
			CheckButton check = new Gtk.CheckButton(t.ToString());
			check.Toggled += on_task_toggled;
			check.Active = true;
			vbox_tasks.Add(check);

			list_checks.Add(check);
			list_tasks_id.Add(t.Id);
		}
		vbox_tasks.ShowAll();
	}

	private void on_task_toggled(object o, EventArgs args)
	{
		CheckButton checkToggled = o as CheckButton;
		if (o == null)
			return;

		int count = 0;
		foreach(Gtk.CheckButton check in list_checks)
		{
			if(check == checkToggled)
			{
				LogB.Information("check toggled row: " + count.ToString());
				LogB.Information(check.Active.ToString());

				Json json = new Json();
				/*
				 * pass negative check because on this dialog "active" means to do task
				 * on server the boolean is called "done" and means the opposite
				 */
				json.UpdateTask(list_tasks_id[count], Util.BoolToInt(! check.Active));
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
		LogB.Information("Destroying dialogPersonPupopu");
		dialog_person_popup.Destroy ();
		LogB.Information("Destroyed dialogPersonPupopu");
	}
}
