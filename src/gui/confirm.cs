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
 * Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
//using Glade;
//using Gnome;
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using Mono.Unix;


public class ConfirmWindowJumpRun
{
	Gtk.Window confirm_window;
	Gtk.Label label1;
	Gtk.Label label_question;
	Gtk.Button button_accept;

	static ConfirmWindowJumpRun ConfirmWindowJumpRunBox;
	
	public ConfirmWindowJumpRun (string text1, string question)
	{
		//Setup (text, table, uniqueID);
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "confirm_window.glade", "confirm_window", "chronojump");
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "confirm_window.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);
		

		confirm_window.Title = "Chronojump - " + Catalog.GetString("Confirm");
		//put an icon to window
		UtilGtk.IconWindow(confirm_window);

		if(! Config.UseSystemColor)
		{
			UtilGtk.WindowColor(confirm_window, Config.ColorBackground);
			UtilGtk.ContrastLabelsLabel(UtilGtk.ColorIsDark(Config.ColorBackground), label1);
			UtilGtk.ContrastLabelsLabel(UtilGtk.ColorIsDark(Config.ColorBackground), label_question);
		}
		
		label1.Text = text1;
		if(question == "")
			label_question.Hide();
		else
			label_question.Text = question;
	}

	static public ConfirmWindowJumpRun Show (string text1, string question)
	{
		if (ConfirmWindowJumpRunBox == null) {
			ConfirmWindowJumpRunBox = new ConfirmWindowJumpRun(text1, question);
		}
		ConfirmWindowJumpRunBox.confirm_window.Show ();
		
		return ConfirmWindowJumpRunBox;
	}
	
	protected void on_button_cancel_clicked (object o, EventArgs args)
	{
		ConfirmWindowJumpRunBox.confirm_window.Hide();
		ConfirmWindowJumpRunBox = null;
	}

	protected void on_delete_selected_jump_delete_event (object o, DeleteEventArgs args)
	{
		ConfirmWindowJumpRunBox.confirm_window.Hide();
		ConfirmWindowJumpRunBox = null;
	}

	protected void on_button_accept_clicked (object o, EventArgs args)
	{
		ConfirmWindowJumpRunBox.confirm_window.Hide();
		ConfirmWindowJumpRunBox = null;
	}
	
	public Button Button_accept 
	{
		set { button_accept = value; }
		get { return button_accept; }
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		confirm_window = (Gtk.Window) builder.GetObject ("confirm_window");
		label1 = (Gtk.Label) builder.GetObject ("label1");
		label_question = (Gtk.Label) builder.GetObject ("label_question");
		button_accept = (Gtk.Button) builder.GetObject ("button_accept");
	}

	~ConfirmWindowJumpRun() {}
	
}


public class ConfirmWindow
{
	Gtk.Window confirm_window;
	Gtk.Label label1;
	Gtk.Label label_link;
	Gtk.Label label_question;
	Gtk.Button button_accept;
	Gtk.Button button_cancel;

	static ConfirmWindow ConfirmWindowBox;
	
	public ConfirmWindow (string text1, string link, string question)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "confirm_window.glade", "confirm_window", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "confirm_window.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);
		
		confirm_window.Title = "Chronojump - " + Catalog.GetString("Confirm");

		//put an icon to window
		UtilGtk.IconWindow(confirm_window);

		if(! Config.UseSystemColor)
		{
			UtilGtk.WindowColor(confirm_window, Config.ColorBackground);
			UtilGtk.ContrastLabelsLabel(UtilGtk.ColorIsDark(Config.ColorBackground), label1);
			UtilGtk.ContrastLabelsLabel(UtilGtk.ColorIsDark(Config.ColorBackground), label_link);
			UtilGtk.ContrastLabelsLabel(UtilGtk.ColorIsDark(Config.ColorBackground), label_question);
		}
		
		label1.Text = text1;
		label1.UseMarkup = true;

		if(link != "") {
			label_link.Text = link;
			label_link.UseMarkup = true;
		} else
			label_link.Hide();

		if(question != "") {
			label_question.Text = question;
			label_question.UseMarkup = true;
		} else
			label_question.Hide();

	}

	static public ConfirmWindow Show (string text1, string link, string question)
	{
		if (ConfirmWindowBox == null) {
			ConfirmWindowBox = new ConfirmWindow(text1, link, question);
		}
		ConfirmWindowBox.confirm_window.Show ();
		
		return ConfirmWindowBox;
	}
	
	protected void on_button_cancel_clicked (object o, EventArgs args)
	{
		ConfirmWindowBox.confirm_window.Hide();
		ConfirmWindowBox = null;
	}
	
	protected void on_delete_selected_jump_delete_event (object o, DeleteEventArgs args)
	{
		//ConfirmWindowBox.confirm_window.Hide();
		//ConfirmWindowBox = null;

		//better raise this signal, and then can be controlled equal if it's cancel or delete
		button_cancel.Click();
	}
	

	protected void on_button_accept_clicked (object o, EventArgs args)
	{
		ConfirmWindowBox.confirm_window.Hide();
		ConfirmWindowBox = null;
	}
	
	public Button Button_accept 
	{
		set { button_accept = value; }
		get { return button_accept; }
	}

	public Button Button_cancel 
	{
		set { button_cancel = value; }
		get { return button_cancel; }
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		confirm_window = (Gtk.Window) builder.GetObject ("confirm_window");
		label1 = (Gtk.Label) builder.GetObject ("label1");
		label_link = (Gtk.Label) builder.GetObject ("label_link");
		label_question = (Gtk.Label) builder.GetObject ("label_question");
		button_accept = (Gtk.Button) builder.GetObject ("button_accept");
		button_cancel = (Gtk.Button) builder.GetObject ("button_cancel");
	}

	~ConfirmWindow() {}
	
}

