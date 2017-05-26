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

	public DialogPersonPopup (int personID, string name, string rfid, string task)
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
		tb.Text =  task;
		textview_task.Buffer = tb;
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
