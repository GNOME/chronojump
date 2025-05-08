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
 * Copyright (C) 2025   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Gdk;
//using Glade;
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using Mono.Unix;

public class ChronopicTestWindow
{
	// at glade ---->
	Gtk.Window chronopic_test;
	Gtk.Notebook notebook;
	Gtk.Button button_close;
	Gtk.Box box_result;
	Gtk.Label label_contact_reason;
	Gtk.Image image_close;

	//1st tab
	Gtk.Image image_ledOn;
	Gtk.Button button_ledOn_yes;
	Gtk.Button button_ledOn_no;

	//2nd tab
	Gtk.Image image_testLed;
	Gtk.Button button_testLed_yes;
	Gtk.Button button_testLed_no;

	//3rd tab
	Gtk.Image image_rca;
	Gtk.Button button_rca_yes;
	Gtk.Button button_rca_no;

	// <---- at glade
	
	static ChronopicTestWindow ChronopicTestWindowBox;

	//public Gtk.Button FakeButtonAccept; //to return orderedData
	
	public ChronopicTestWindow (Gtk.Window parent)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronopic_test.glade", "execute_auto", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "chronopic_test.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);

		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.WindowColor (chronopic_test, Config.ColorBackground);
			UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundIsDark, notebook);
			UtilGtk.ContrastLabelsBox (Config.ColorBackgroundIsDark, box_result);
		}

		chronopic_test.Parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow (chronopic_test);
		
		//FakeButtonAccept = new Gtk.Button();
	}

	static public ChronopicTestWindow Show (Gtk.Window parent)
	{
		if (ChronopicTestWindowBox == null) {
			ChronopicTestWindowBox = new ChronopicTestWindow (parent);
		}	

		ChronopicTestWindowBox.initialize();

		ChronopicTestWindowBox.chronopic_test.Show ();

		return ChronopicTestWindowBox;
	}
	
	private void initialize()
	{
		image_close.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_close.png");

		notebook.CurrentPage = 0;

		Pixbuf pixbuf;
		
		pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "chronopic_ledOn_300.jpg");
		image_ledOn.Pixbuf = pixbuf;
		pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "chronopic_testLed_300.jpg");
		image_testLed.Pixbuf = pixbuf;
		pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "rca_test_300.jpg");
		image_rca.Pixbuf = pixbuf;
	}

	// yes/no butrons
	// 1st tab
	private void on_button_ledOn_yes_clicked (object o, EventArgs args)
	{
		notebook.CurrentPage = 1;
	}
	private void on_button_ledOn_no_clicked (object o, EventArgs args)
	{
		label_contact_reason.Text = "Chronopic RCA cable removed. Led D1 is Off.";
		box_result.Visible = true;
		notebook.Sensitive = false;
	}

	// 2nd tab
	private void on_button_testLed_yes_clicked (object o, EventArgs args)
	{
		notebook.CurrentPage = 2;
	}
	private void on_button_testLed_no_clicked (object o, EventArgs args)
	{
		label_contact_reason.Text = "Chronopic test button pressed, RCA is on.";
		box_result.Visible = true;
		notebook.Sensitive = false;
	}

	// 3rd tab
	private void on_button_rca_yes_clicked (object o, EventArgs args)
	{
		new DialogMessage (Constants.MessageTypes.INFO, "TODO");
	}
	private void on_button_rca_no_clicked (object o, EventArgs args)
	{
		label_contact_reason.Text = "RCA cable is not working. Try to change it.";
		box_result.Visible = true;
		notebook.Sensitive = false;
	}


	public void Close() {
		on_button_close_clicked (new object (), new EventArgs ());
	}

	private void on_button_close_clicked (object o, EventArgs args)
	{
		ChronopicTestWindowBox.chronopic_test.Hide();
		ChronopicTestWindowBox = null;
	}
	
	private void on_delete_event (object o, DeleteEventArgs args)
	{
		ChronopicTestWindowBox.chronopic_test.Hide();
		ChronopicTestWindowBox = null;
	}
	
	private void connectWidgets (Gtk.Builder builder)
	{
		chronopic_test = (Gtk.Window) builder.GetObject ("chronopic_test");
		notebook = (Gtk.Notebook) builder.GetObject ("notebook");
		button_close = (Gtk.Button) builder.GetObject ("button_close");
		box_result = (Gtk.Box) builder.GetObject ("box_result");
		label_contact_reason = (Gtk.Label) builder.GetObject ("label_contact_reason");
		image_close = (Gtk.Image) builder.GetObject ("image_close");

		//1st tab
		image_ledOn = (Gtk.Image) builder.GetObject ("image_ledOn");
		button_ledOn_yes = (Gtk.Button) builder.GetObject ("button_ledOn_yes");
		button_ledOn_no = (Gtk.Button) builder.GetObject ("button_ledOn_no");

		//2nd tab
		image_testLed = (Gtk.Image) builder.GetObject ("image_testLed");
		button_testLed_yes = (Gtk.Button) builder.GetObject ("button_testLed_yes");
		button_testLed_no = (Gtk.Button) builder.GetObject ("button_testLed_no");

		//3rd tab
		image_rca = (Gtk.Image) builder.GetObject ("image_rca");
		button_rca_yes = (Gtk.Button) builder.GetObject ("button_rca_yes");
		button_rca_no = (Gtk.Button) builder.GetObject ("button_rca_no");
	}
}


