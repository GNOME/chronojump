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
	Gtk.Box box_result;
	Gtk.Box box_problems;
	Gtk.Grid grid_iqa;
	Gtk.Label label_progress;
	Gtk.Image image;
	Gtk.Label label_instructions;
	Gtk.Label label_question;
	Gtk.Label label_all_ok;
	Gtk.Label label_contact_reason;
	Gtk.Button button_yes;
	Gtk.Button button_no;
	Gtk.Image image_yes;
	Gtk.Image image_no;
	Gtk.Button button_back;
	Gtk.Image image_back;
	Gtk.Button button_close;
	Gtk.Image image_close;

	// <---- at glade
	int currentPage = 0;
	
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
			UtilGtk.ContrastLabelsBox (Config.ColorBackgroundIsDark, box_result);
			UtilGtk.ContrastLabelsGrid (Config.ColorBackgroundIsDark, grid_iqa);
		}

		chronopic_test.Parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow (chronopic_test);
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
		image_yes.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "thumb_up.png");
		image_no.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "thumb_down.png");
		image_back.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "arrow_back.png");
		image_close.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_close.png");

		currentPage = 0;
		changePage (currentPage);
	}

	private void changePage (int page)
	{
		currentPage = page;
		button_yes.Sensitive = true;
		button_back.Sensitive = true;

		switch (page)
		{
			case 0:
				label_progress.Text = "1 / 5";
				image.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "chronopic_ledOn_300.jpg");
				label_instructions.Text = Catalog.GetString ("Remove the RCA cable. Then, check LED D1.");
				label_question.Text = Catalog.GetString ("Is the green LED D1 on?");
				button_back.Sensitive = false;
				break;
			case 1:
				label_progress.Text = "2 / 5";
				image.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "chronopic_testLed_300.jpg");
				label_instructions.Text = Catalog.GetString ("Press test button.");
				label_question.Text = Catalog.GetString ("When test button is pressed, is the green LED D1 off?");
				break;
			case 2:
				label_progress.Text = "3 / 5";
				image.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "rca_test_300.jpg");
				label_instructions.Text = Catalog.GetString ("Connect RCA cable.") + "\n" +
					Catalog.GetString ("Touch the tip and teeth of RCA with a metallic object (like a key).");
				label_question.Text = Catalog.GetString ("Is the green LED D1 off?");
				break;
			case 3:
				label_progress.Text = "4 / 5";
				image.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "platform_connect_300.jpg");
				label_instructions.Text = Catalog.GetString ("Connect Chronopic with the platform using the RCA.") + "\n" + 
					Catalog.GetString ("Do not stand on the platform.");
				label_question.Text = Catalog.GetString ("Is the green LED D1 on?");
				break;
			case 4:
				label_progress.Text = "5 / 5";
				image.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "platform_stand_300.jpg");
				label_instructions.Text = Catalog.GetString ("Stand on the platform.");
				label_question.Text = Catalog.GetString ("Is the green LED D1 off?");
				break;
		}
	}

	private void on_button_yes_clicked (object o, EventArgs args)
	{
		button_no.Sensitive = true;

		if (currentPage == 4)
		{
			box_result.Visible = true;
			box_problems.Visible = false;
			label_all_ok.Visible = true;
			button_yes.Sensitive = false;
		}
		else
		{
			box_result.Visible = false;
			box_problems.Visible = false;
			label_all_ok.Visible = false;
			changePage (currentPage + 1);
		}
	}
	
	private void on_button_no_clicked (object o, EventArgs args)
	{
		box_result.Visible = true;
		box_problems.Visible = true;
		label_all_ok.Visible = false;
		button_no.Sensitive = false;

		switch (currentPage)
		{
			case 0:
				label_contact_reason.Text = "Chronopic RCA cable removed. Led D1 is off.";
				break;
			case 1:
				label_contact_reason.Text = "Chronopic test button pressed. Led D1 is on.";
				break;
			case 2:
				label_contact_reason.Text = "RCA cable is not working. Try to change it.";
				break;
			case 3:
				label_contact_reason.Text = "Connected to platform without any weight. Led D1 is off.";
				break;
			case 4:
				label_contact_reason.Text = "Standing on platform. Led D1 is on.";
				break;
		}
	}

	private void on_button_back_clicked (object o, EventArgs args)
	{
		// impossible because is not sensitive, but just in case
		if (currentPage == 0)
			return;

		button_no.Sensitive = true;
		changePage (currentPage - 1);
		box_result.Visible = false;
		//box_problems.Visible = false;
		label_all_ok.Visible = false;
	}

	public void Close()
	{
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
		box_result = (Gtk.Box) builder.GetObject ("box_result");
		box_problems = (Gtk.Box) builder.GetObject ("box_problems");
		grid_iqa = (Gtk.Grid) builder.GetObject ("grid_iqa");
		label_progress = (Gtk.Label) builder.GetObject ("label_progress");
		image = (Gtk.Image) builder.GetObject ("image");
		label_instructions = (Gtk.Label) builder.GetObject ("label_instructions");
		label_question = (Gtk.Label) builder.GetObject ("label_question");
		label_all_ok = (Gtk.Label) builder.GetObject ("label_all_ok");
		label_contact_reason = (Gtk.Label) builder.GetObject ("label_contact_reason");
		button_yes = (Gtk.Button) builder.GetObject ("button_yes");
		button_no = (Gtk.Button) builder.GetObject ("button_no");
		image_yes = (Gtk.Image) builder.GetObject ("image_yes");
		image_no = (Gtk.Image) builder.GetObject ("image_no");
		button_back = (Gtk.Button) builder.GetObject ("button_back");
		image_back = (Gtk.Image) builder.GetObject ("image_back");
		button_close = (Gtk.Button) builder.GetObject ("button_close");
		image_close = (Gtk.Image) builder.GetObject ("image_close");
	}
}


