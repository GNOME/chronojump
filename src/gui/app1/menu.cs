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
	[Widget] Gtk.Label label_button_show_menu;
	[Widget] Gtk.Arrow arrow_menu_show_menu_up;
	[Widget] Gtk.Arrow arrow_menu_show_menu_down;
	[Widget] Gtk.Arrow arrow_menu_show_session_up;
	[Widget] Gtk.Arrow arrow_menu_show_session_down;
	[Widget] Gtk.Arrow arrow_menu_show_help_up;
	[Widget] Gtk.Arrow arrow_menu_show_help_down;
	[Widget] Gtk.Button button_modes;
	//[Widget] Gtk.HPaned hpaned_contacts_main;
	[Widget] Gtk.Viewport viewport_hpaned_contacts_main;
	[Widget] Gtk.Viewport viewport_start_modes;
	[Widget] Gtk.Viewport viewport_menu_top;
	[Widget] Gtk.Viewport viewport_menu;
	[Widget] Gtk.Viewport viewport_persons;
	[Widget] Gtk.CheckButton check_menu_session;
	[Widget] Gtk.CheckButton check_menu_help;
	[Widget] Gtk.Alignment alignment_menu_session_options;
	[Widget] Gtk.Alignment alignment_menu_person_options;
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
	}

	private void on_button_show_menu_clicked (object o, EventArgs args)
	{
		if(! viewport_menu.Visible)
		{
			if(check_menu_session.Active)
				check_menu_session.Active = false;
			if(check_menu_help.Active)
				check_menu_help.Active = false;
		}

		viewport_menu.Visible = ! viewport_menu.Visible;
		if(viewport_menu.Visible) {
			viewport_persons.Visible = false;
			arrow_menu_show_menu_up.Visible = true;
			arrow_menu_show_menu_down.Visible = false;
		} else {
			viewport_persons.Visible = (currentSession != null);
			arrow_menu_show_menu_up.Visible = false;
			arrow_menu_show_menu_down.Visible = true;
		}
		//hpaned_contacts_main.Show();
	}

	private void on_check_menu_session_clicked (object o, EventArgs args)
	{
		if(check_menu_session.Active)
		{
			check_menu_help.Active = false;
			alignment_menu_session_options.Visible = true;
			alignment_menu_session_options.Show();
			arrow_menu_show_session_up.Visible = true;
			arrow_menu_show_session_down.Visible = false;
		} else {
			alignment_menu_session_options.Visible = false;
			arrow_menu_show_session_up.Visible = false;
			arrow_menu_show_session_down.Visible = true;
		}
	}

	private void on_check_menu_help_clicked (object o, EventArgs args)
	{
		if(check_menu_help.Active)
		{
			check_menu_session.Active = false;
			alignment_menu_help_options.Visible = true;
			arrow_menu_show_help_up.Visible = true;
			arrow_menu_show_help_down.Visible = false;
		} else {
			alignment_menu_help_options.Visible = false;
			arrow_menu_show_help_up.Visible = false;
			arrow_menu_show_help_down.Visible = true;
		}
	}

	private void on_button_modes_clicked (object o, EventArgs args)
	{
		show_start_page();
		button_modes.Sensitive = false;

		/*
		//to care about viewport_menu_top being lower width allocated and a bit hidden by hpaned_contacts_main
		LogB.Information("viewport_menu_top.Allocation.Width: " + viewport_menu_top.Allocation.Width.ToString());
		LogB.Information("viewport_menu_top.SizeRequest.Width: " + viewport_menu_top.SizeRequest().Width.ToString());
		*/
	}

}
