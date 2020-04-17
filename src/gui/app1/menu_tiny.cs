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

//this file has methods of ChronoJumpWindow related to manage menu_tiny

using System;
using Gtk;
using Glade;

public partial class ChronoJumpWindow
{
	[Widget] Gtk.Alignment alignment_menu_tiny;
	[Widget] Gtk.Arrow arrow_menu_show_session_up1;
	[Widget] Gtk.Arrow arrow_menu_show_session_down1;
	[Widget] Gtk.Arrow arrow_menu_show_help_up1;
	[Widget] Gtk.Arrow arrow_menu_show_help_down1;
	[Widget] Gtk.Button button_show_modes1;
	[Widget] Gtk.VBox vbox_menu_tiny;
	[Widget] Gtk.EventBox eventbox_check_menu_session1;
	[Widget] Gtk.EventBox eventbox_button_menu_session_more1;
	[Widget] Gtk.EventBox eventbox_button_menu_session_new1;
	[Widget] Gtk.EventBox eventbox_button_menu_session_load1;
	[Widget] Gtk.EventBox eventbox_button_show_modes1;
	[Widget] Gtk.EventBox eventbox_button_menu_preferences1;
	[Widget] Gtk.EventBox eventbox_check_menu_help1;
	[Widget] Gtk.EventBox eventbox_button_menu_help_documents1;
	[Widget] Gtk.EventBox eventbox_button_menu_help_accelerators1;
	[Widget] Gtk.EventBox eventbox_button_menu_help_about1;
	[Widget] Gtk.EventBox eventbox_button_menu_exit1;
	[Widget] Gtk.EventBox eventbox_button_contacts_person_change;
	[Widget] Gtk.EventBox eventbox_button_encoder_person_change;
	[Widget] Gtk.CheckButton check_menu_session1;
	[Widget] Gtk.CheckButton check_menu_help1;
	[Widget] Gtk.VBox vbox_menu_session1;
	[Widget] Gtk.Alignment alignment_menu_session_options1;
	[Widget] Gtk.Alignment alignment_menu_person_options1;
	[Widget] Gtk.Alignment alignment_menu_help_options1;
	[Widget] Gtk.Button button_menu_session_more1;
	[Widget] Gtk.Button button_menu_preferences1;

	[Widget] Gtk.Image image_menu_folders1;
	[Widget] Gtk.Image image_session_new1;
	[Widget] Gtk.Image image_session_load1;
	[Widget] Gtk.Image image_session_more1;
	[Widget] Gtk.Image image_button_show_modes1;
	[Widget] Gtk.Image image_menu_preferences1;
	[Widget] Gtk.Image image_menu_help1;
	[Widget] Gtk.Image image_menu_help_documents1;
	[Widget] Gtk.Image image_menu_help_accelerators1;
	[Widget] Gtk.Image image_menu_help_about1;
	[Widget] Gtk.Image image_menu_quit1;

	[Widget] Gtk.Viewport viewport_image_logo_icon;


	private void menuTinyInitialize ()
	{
		menuTinySetColors();

		/*
		 * have it aligned with start of the notebook_sup
		 * +4 is alignment_contacts TopPadding
		 * +4 is vbox_contacts spacing
		 * (same for encoder)
		 */
		alignment_menu_tiny.TopPadding = (uint) radio_mode_contacts_capture.SizeRequest().Height + 4 + 4;

		//unselect menu_help if selected
		if(check_menu_help1.Active)
			check_menu_help1.Active = false;
		alignment_menu_help_options1.Visible = false;

		image_logo_contacts_transp.Visible = false;
		frame_logo_contacts.Visible = false;
		image_logo_encoder_transp.Visible = false;
		frame_logo_encoder.Visible = false;

		if(UtilGtk.ColorIsOkWithLogoTransparent (UtilGtk.ColorParse(preferences.colorBackgroundString)))
		{
			image_logo_icon_transp.Visible = true;
			frame_image_logo_icon.Visible = false;
		} else {
			image_logo_icon_transp.Visible = false;
			frame_image_logo_icon.Visible = true;
		}
	}

	private void menuTinySetColors ()
	{
		Gdk.Color color = UtilGtk.ColorParse(preferences.colorBackgroundString);

		UtilGtk.ViewportColor(viewport_hpaned_contacts_main, color);
		UtilGtk.ViewportColor(viewport_send_log, color);
		UtilGtk.ViewportColor(viewport_exit_confirm, color);
		UtilGtk.ViewportColor(viewport_rest_time_contacts, color);
		UtilGtk.ViewportColor(viewport_rest_time_encoder, color);
		UtilGtk.ViewportColor(viewport_image_logo_icon, UtilGtk.BLUE_CHRONOJUMP);

		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_show_modes1, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_check_menu_session1, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_preferences1, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_check_menu_help1, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_exit1, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);

		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_session_new1, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_session_load1, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_session_more1, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_help_documents1, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_help_accelerators1, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_help_about1, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);

		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_contacts_person_change, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_encoder_person_change, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
	}


	private void on_check_menu_session1_clicked (object o, EventArgs args)
	{
		menuShowVerticalArrow (check_menu_session1.Active, arrow_menu_show_session_up1, arrow_menu_show_session_down1);
		if(check_menu_session1.Active)
		{
			check_menu_help1.Active = false;
			alignment_menu_session_options1.Visible = true;

			alignment_menu_session_options1.Show();
		} else
			alignment_menu_session_options1.Visible = false;
	}

	private void on_check_menu_help1_clicked (object o, EventArgs args)
	{
		menuShowVerticalArrow (check_menu_help1.Active, arrow_menu_show_help_up1, arrow_menu_show_help_down1);
		if(check_menu_help1.Active)
		{
			check_menu_session1.Active = false;
			alignment_menu_help_options1.Visible = true;
		} else
			alignment_menu_help_options1.Visible = false;
	}

	private void on_button_show_modes1_clicked (object o, EventArgs args)
	{
		show_start_page();
		button_show_modes1.Sensitive = false;
	}

}
