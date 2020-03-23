/*
 * This file is part of ChronoJump
 *
 * Chronojump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * Chronojump is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 *    GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * Copyright (C) 2018   Xavier de Blas <xaviblas@gmail.com> 
 */

//this file has methods of ChronoJumpWindow related to manage menu

using System;
using Gtk;
//using Gdk;
using Glade;

//provar checkbuttons enlloc dels togglebuttons que igual no van be

public partial class ChronoJumpWindow
{
	[Widget] Gtk.Arrow arrow_menu_show_menu_up;
	[Widget] Gtk.Arrow arrow_menu_show_menu_down;
	[Widget] Gtk.Arrow arrow_menu_show_session_up;
	[Widget] Gtk.Arrow arrow_menu_show_session_down;
	[Widget] Gtk.Arrow arrow_menu_show_encoder_up;
	[Widget] Gtk.Arrow arrow_menu_show_encoder_down;
	[Widget] Gtk.Arrow arrow_menu_show_help_up;
	[Widget] Gtk.Arrow arrow_menu_show_help_down;
	[Widget] Gtk.Button button_show_modes;
	//[Widget] Gtk.HPaned hpaned_contacts_main;
	[Widget] Gtk.Viewport viewport_hpaned_contacts_main;
	[Widget] Gtk.Viewport viewport_start_modes;
	[Widget] Gtk.Viewport viewport_menu_top;
	[Widget] Gtk.Viewport viewport_menu;
	[Widget] Gtk.Viewport viewport_persons;
	[Widget] Gtk.EventBox eventbox_button_show_menu;
	[Widget] Gtk.EventBox eventbox_button_show_modes;
	[Widget] Gtk.EventBox eventbox_check_menu_session;
	[Widget] Gtk.EventBox eventbox_button_menu_preferences;
	[Widget] Gtk.EventBox eventbox_check_menu_encoder;
	[Widget] Gtk.EventBox eventbox_check_menu_help;
	[Widget] Gtk.EventBox eventbox_button_menu_exit;
	[Widget] Gtk.CheckButton check_menu_session;
	[Widget] Gtk.CheckButton check_menu_encoder;
	[Widget] Gtk.CheckButton check_menu_help;
	[Widget] Gtk.Image image_button_show_menu;
	[Widget] Gtk.Label label_button_show_menu;
	[Widget] Gtk.Image image_button_show_modes;
	[Widget] Gtk.Label label_button_show_modes;
	[Widget] Gtk.Image image_menu_folders;
	[Widget] Gtk.Label label_menu_folders;
	[Widget] Gtk.Image image_menu_preferences;
	[Widget] Gtk.Label label_menu_preferences;
	[Widget] Gtk.Image image_menu_help;
	[Widget] Gtk.Label label_menu_help;
	[Widget] Gtk.Image image_menu_help_documents;
	[Widget] Gtk.Label label_menu_help_documents;
	[Widget] Gtk.Image image_menu_help_accelerators;
	[Widget] Gtk.Label label_menu_help_accelerators;
	[Widget] Gtk.Image image_menu_help_about;
	[Widget] Gtk.Label label_menu_help_about;
	[Widget] Gtk.Image image_menu_quit;
	[Widget] Gtk.Label label_menu_quit;
	[Widget] Gtk.Alignment alignment_menu_session_options;
	[Widget] Gtk.Alignment alignment_menu_person_options;
	[Widget] Gtk.Alignment alignment_menu_encoder_options;
	[Widget] Gtk.Alignment alignment_menu_help_options;
	[Widget] Gtk.Image image_persons_new_2;
	[Widget] Gtk.Image image_persons_new_plus_2;
	[Widget] Gtk.Image image_persons_open_2;
	[Widget] Gtk.Image image_persons_open_plus_2;

	private void menu_initialize_colors()
	{
		//Gdk.Color color = UtilGtk.YELLOW;
		//Gdk.Color color = UtilGtk.BLUE_CHRONOJUMP;

		Gdk.Color color = UtilGtk.ColorParse(preferences.colorBackgroundString);

		UtilGtk.ViewportColor(viewport_hpaned_contacts_main, color);
		UtilGtk.ViewportColor(viewport_menu_top, color);
		UtilGtk.ViewportColor(viewport_menu, color);
		//UtilGtk.ViewportColor(viewport_menu, UtilGtk.BLUE_CLEAR2);
		//UtilGtk.ViewportColor(viewport_menu, UtilGtk.YELLOW);
		//UtilGtk.ViewportColor(viewport_menu, UtilGtk.GRAY_LIGHT);
		//UtilGtk.ViewportColor(viewport_persons, UtilGtk.BLUE_CLEAR2);

		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_show_menu, UtilGtk.YELLOW);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_show_modes, UtilGtk.YELLOW);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_check_menu_session, UtilGtk.YELLOW);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_preferences, UtilGtk.YELLOW);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_check_menu_encoder, UtilGtk.YELLOW);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_check_menu_help, UtilGtk.YELLOW);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_exit, UtilGtk.YELLOW);
	}

	private void menuShowVerticalArrow (bool selected, Gtk.Arrow a_up, Gtk.Arrow a_down)
	{
		a_up.Visible = selected;
		a_down.Visible = ! selected;
	}

	private void on_button_show_menu_clicked (object o, EventArgs args)
	{
		if(! viewport_menu.Visible)
		{
			if(check_menu_session.Active)
				check_menu_session.Active = false;
			if(check_menu_help.Active)
				check_menu_help.Active = false;
			if(check_menu_encoder.Active)
				check_menu_encoder.Active = false;
		}

		viewport_menu.Visible = ! viewport_menu.Visible;
		menuShowVerticalArrow (viewport_menu.Visible, arrow_menu_show_menu_up, arrow_menu_show_menu_down);

		if(viewport_menu.Visible)
			viewport_persons.Visible = false;
		else
			viewport_persons.Visible = (currentSession != null);
		//hpaned_contacts_main.Show();
	}

	private void on_check_menu_session_clicked (object o, EventArgs args)
	{
		menuShowVerticalArrow (check_menu_session.Active, arrow_menu_show_session_up, arrow_menu_show_session_down);
		if(check_menu_session.Active)
		{
			check_menu_encoder.Active = false;
			check_menu_help.Active = false;
			alignment_menu_session_options.Visible = true;
			alignment_menu_session_options.Show();
		} else
			alignment_menu_session_options.Visible = false;
	}

	private void on_check_menu_encoder_clicked (object o, EventArgs args)
	{
		menuShowVerticalArrow (check_menu_encoder.Active, arrow_menu_show_encoder_up, arrow_menu_show_encoder_down);
		if(check_menu_encoder.Active)
		{
			check_menu_session.Active = false;
			check_menu_help.Active = false;
			alignment_menu_encoder_options.Visible = true;
		} else
			alignment_menu_encoder_options.Visible = false;
	}
	private void on_check_menu_help_clicked (object o, EventArgs args)
	{
		menuShowVerticalArrow (check_menu_help.Active, arrow_menu_show_help_up, arrow_menu_show_help_down);
		if(check_menu_help.Active)
		{
			check_menu_encoder.Active = false;
			check_menu_session.Active = false;
			alignment_menu_help_options.Visible = true;
		} else
			alignment_menu_help_options.Visible = false;
	}

	private void on_button_show_modes_clicked (object o, EventArgs args)
	{
		show_start_page();
		button_show_modes.Sensitive = false;

		/*
		//to care about viewport_menu_top being lower width allocated and a bit hidden by hpaned_contacts_main
		LogB.Information("viewport_menu_top.Allocation.Width: " + viewport_menu_top.Allocation.Width.ToString());
		LogB.Information("viewport_menu_top.SizeRequest.Width: " + viewport_menu_top.SizeRequest().Width.ToString());
		*/
	}

}


/*
public class ColorGuiManage
{
	List<ColorGui> colorGui_l;

	public ColorGuiManage()
	{
		List<ColorGui> colorGui_l = new List<colorGui>();
		List.Add(new ColorGui("Chronojump Yellow", "0xff,0xcc,0x01")); 
		List.Add(new ColorGui("Chronojump Blue", "0x0e,0x1e,0x46")); 
	}

	public Gdk.Color GetColor(string english)
	{
	}
}

public class ColorGui
{
	private string colorEnglish;
	private string colorHTML;

	public ColorGui(string english, string html)
	{
		this.colorEnglish = english;
		this.colorHTML = html;
	}

	public Gdk.Color ColorHTML
	{
		get { return new Gdk.Color(colorHTML; }
	}
}
*/
