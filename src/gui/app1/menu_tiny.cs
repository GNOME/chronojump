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
 * Copyright (C) 2018-2023   Xavier de Blas <xaviblas@gmail.com> 
 */

//this file has methods of ChronoJumpWindow related to manage menu_tiny

using System;
using Gdk;
using Gtk;
//using Glade;
//using System.Collections.Generic; //List

public partial class ChronoJumpWindow
{
	Gtk.Alignment alignment_menu_tiny;
	Gtk.Arrow arrow_menu_show_session_up1;
	Gtk.Arrow arrow_menu_show_session_down1;
	Gtk.VBox vbox_menu_tiny;
	Gtk.VBox vbox_menu_tiny_menu; //really the menu (without the logos at the bottom)
	Gtk.EventBox eventbox_button_menu_database;
	Gtk.EventBox eventbox_check_menu_session1;
	Gtk.EventBox eventbox_button_menu_session_more1;
	Gtk.EventBox eventbox_button_menu_session_new1;
	Gtk.EventBox eventbox_button_menu_session_load1;
	Gtk.EventBox eventbox_button_menu_preferences1;
	Gtk.EventBox eventbox_button_menu_help1;
	Gtk.EventBox eventbox_button_menu_news1;
	Gtk.EventBox eventbox_button_menu_exit1;
	Gtk.EventBox eventbox_button_contacts_person_change;
	Gtk.EventBox eventbox_button_encoder_person_change;
	Gtk.EventBox eventbox_button_networks_contacts_guest;
	Gtk.EventBox eventbox_button_networks_encoder_guest;
	Gtk.CheckButton check_menu_session1;
	Gtk.Button button_menu_preferences1;
	Gtk.Button button_menu_news1;
	Gtk.VBox vbox_menu_session1;
	Gtk.Alignment alignment_menu_session_options1;

	Gtk.Image image_menu_folders1;
	Gtk.Image image_session_new1;
	Gtk.Image image_session_load1;
	Gtk.Image image_session_more1;
	Gtk.Image image_menu_preferences1;
	Gtk.Image image_menu_help1;
	Gtk.Image image_menu_news1;
	Gtk.Image image_menu_quit1;
	Gtk.Button button_menu_guiTest1;

	Gtk.Viewport viewport_image_logo_icon;
	//Gtk.HBox hbox_top_contacts;

	private void menuTinyInitialize ()
	{
		menuTinySetColors();

		/*
		 * have it aligned with start of the notebook_sup
		 * +4 is alignment_contacts TopPadding
		 * +4 is vbox_contacts spacing
		 * (same for encoder)
		 */
		/*
		List <int> l = new List<int>();
		l.Add(hbox_top_contacts.SizeRequest().Height);
		l.Add(hbox_encoder_sup_capture_analyze.SizeRequest().Height);
		int maxHeight = getMenuButtonsMaxWidthOrHeight(l);

		alignment_menu_tiny.TopPadding = (uint) maxHeight + 4;
		*/

		//doing with Allocation, but wait 1s to have correctly allocated widgets in order to vertical align menu
		//of course it will be much better using a table, but I do not want to risk in moving all big notebooks to a table cell

		GLib.Timeout.Add(200, new GLib.TimeoutHandler(menuTinyTopAlign));

		image_logo_contacts_transp.Visible = false;
		image_logo_contacts.Visible = false;
		image_logo_encoder_transp.Visible = false;
		image_logo_encoder.Visible = false;
		//TODO: check this
		fullscreen_image_logo.Visible = false;
		fullscreen_image_logo_transp.Visible = false;

		if(! Config.UseSystemColor && UtilGtk.ColorIsOkWithLogoTransparent (UtilGtk.ColorParse(preferences.colorBackgroundString)))
		{
			image_logo_icon_transp.Visible = true;
			frame_image_logo_icon.Visible = false;
		} else {
			image_logo_icon_transp.Visible = false;
			frame_image_logo_icon.Visible = true;
		}
	}

	private bool menuTinyTopAlign()
	{
		/*
		sadly this is not a correct info on Chronojump starts: says 52 when it should be 48
		but called in this Timeout it works as a charm

		LogB.Information("notebook_capture_analyze.Allocation.Y = " + notebook_capture_analyze.Allocation.Y.ToString());
		LogB.Information("hbox_top_contacts.Allocation.Y = " + hbox_top_contacts.Allocation.Y.ToString());
		LogB.Information("hbox_top_contacts.Allocation.Height = " + hbox_top_contacts.Allocation.Height.ToString());
		LogB.Information("button_show_modes_contacts.Allocation.Height = " + button_show_modes_contacts.Allocation.Height.ToString());
		LogB.Information("hbox_contacts_sup_capture_analyze_two_buttons.Allocation.Height = " + hbox_contacts_sup_capture_analyze_two_buttons.Allocation.Height.ToString());
		LogB.Information("hbox_top_person.Allocation.Height = " + hbox_top_person.Allocation.Height.ToString());
		LogB.Information("hbox_other.Allocation.Height = " + hbox_other.Allocation.Height.ToString());
		LogB.Information("hbox_top_contacts.Allocation.Height = " + hbox_top_contacts.Allocation.Height.ToString());

		//LogB.Information("notebook_capture_analyze.Allocation.Y = " + notebook_capture_analyze.Allocation.Y.ToString());
		//LogB.Information("notebook_encoder_sup.Allocation.Y = " + notebook_encoder_sup.Allocation.Y.ToString());
		*/

		alignment_menu_tiny.TopPadding = (uint) notebook_capture_analyze.Allocation.Y;

		return false; //do not call this again
	}

	private void menuTinySetColors ()
	{
		if(! Config.UseSystemColor)
		{
			RGBA color = UtilGtk.ColorParse (preferences.colorBackgroundString);

			UtilGtk.WindowColor (app1, color);
			UtilGtk.ViewportColor (viewport_exit_confirm, color);
			UtilGtk.ViewportColor (viewport_rest_time_contacts, color);
			UtilGtk.ViewportColor (viewport_rest_time_encoder, color);

			UtilGtk.WidgetColor (vbox_send_log, Config.ColorBackground);

			UtilGtk.WidgetColor (frame_send_log, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsFrame (Config.ColorBackgroundShiftedIsDark, frame_send_log);

			UtilGtk.WidgetColor (hbox_social_network_poll, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsWidget (Config.ColorBackgroundShiftedIsDark, hbox_social_network_poll);
		}

		UtilGtk.ViewportColor (viewport_image_logo_icon, UtilGtk.Colors.BLUE_CHRONOJUMP);

		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_database,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_check_menu_session1,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_preferences1,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_help1,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_news1,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_exit1,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);

		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_session_new1,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_session_load1,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_session_more1,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);

		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_contacts_person_change,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_encoder_person_change,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_networks_contacts_guest,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_networks_encoder_guest,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
	}


	private void on_check_menu_session1_clicked (object o, EventArgs args)
	{
		menuShowVerticalArrow (check_menu_session1.Active, arrow_menu_show_session_up1, arrow_menu_show_session_down1);
		if(check_menu_session1.Active)
		{
			alignment_menu_session_options1.Visible = true;
			alignment_menu_session_options1.Show();
		} else
			alignment_menu_session_options1.Visible = false;
	}

	private void connectWidgetsMenuTiny (Gtk.Builder builder)
	{
		alignment_menu_tiny = (Gtk.Alignment) builder.GetObject ("alignment_menu_tiny");
		arrow_menu_show_session_up1 = (Gtk.Arrow) builder.GetObject ("arrow_menu_show_session_up1");
		arrow_menu_show_session_down1 = (Gtk.Arrow) builder.GetObject ("arrow_menu_show_session_down1");
		vbox_menu_tiny = (Gtk.VBox) builder.GetObject ("vbox_menu_tiny");
		vbox_menu_tiny_menu = (Gtk.VBox) builder.GetObject ("vbox_menu_tiny_menu"); //really the menu (without the logos at the bottom)
		eventbox_button_menu_database = (Gtk.EventBox) builder.GetObject ("eventbox_button_menu_database");
		eventbox_check_menu_session1 = (Gtk.EventBox) builder.GetObject ("eventbox_check_menu_session1");
		eventbox_button_menu_session_more1 = (Gtk.EventBox) builder.GetObject ("eventbox_button_menu_session_more1");
		eventbox_button_menu_session_new1 = (Gtk.EventBox) builder.GetObject ("eventbox_button_menu_session_new1");
		eventbox_button_menu_session_load1 = (Gtk.EventBox) builder.GetObject ("eventbox_button_menu_session_load1");
		eventbox_button_menu_preferences1 = (Gtk.EventBox) builder.GetObject ("eventbox_button_menu_preferences1");
		eventbox_button_menu_help1 = (Gtk.EventBox) builder.GetObject ("eventbox_button_menu_help1");
		eventbox_button_menu_news1 = (Gtk.EventBox) builder.GetObject ("eventbox_button_menu_news1");
		eventbox_button_menu_exit1 = (Gtk.EventBox) builder.GetObject ("eventbox_button_menu_exit1");
		eventbox_button_contacts_person_change = (Gtk.EventBox) builder.GetObject ("eventbox_button_contacts_person_change");
		eventbox_button_encoder_person_change = (Gtk.EventBox) builder.GetObject ("eventbox_button_encoder_person_change");
		eventbox_button_networks_contacts_guest = (Gtk.EventBox) builder.GetObject ("eventbox_button_networks_contacts_guest");
		eventbox_button_networks_encoder_guest = (Gtk.EventBox) builder.GetObject ("eventbox_button_networks_encoder_guest");
		check_menu_session1 = (Gtk.CheckButton) builder.GetObject ("check_menu_session1");
		button_menu_preferences1 = (Gtk.Button) builder.GetObject ("button_menu_preferences1");
		button_menu_news1 = (Gtk.Button) builder.GetObject ("button_menu_news1");
		vbox_menu_session1 = (Gtk.VBox) builder.GetObject ("vbox_menu_session1");
		alignment_menu_session_options1 = (Gtk.Alignment) builder.GetObject ("alignment_menu_session_options1");

		image_menu_folders1 = (Gtk.Image) builder.GetObject ("image_menu_folders1");
		image_session_new1 = (Gtk.Image) builder.GetObject ("image_session_new1");
		image_session_load1 = (Gtk.Image) builder.GetObject ("image_session_load1");
		image_session_more1 = (Gtk.Image) builder.GetObject ("image_session_more1");
		image_menu_preferences1 = (Gtk.Image) builder.GetObject ("image_menu_preferences1");
		image_menu_help1 = (Gtk.Image) builder.GetObject ("image_menu_help1");
		image_menu_news1 = (Gtk.Image) builder.GetObject ("image_menu_news1");
		image_menu_quit1 = (Gtk.Image) builder.GetObject ("image_menu_quit1");
		button_menu_guiTest1 = (Gtk.Button) builder.GetObject ("button_menu_guiTest1");

		viewport_image_logo_icon = (Gtk.Viewport) builder.GetObject ("viewport_image_logo_icon");
		//hbox_top_contacts = (Gtk.HBox) builder.GetObject ("hbox_top_contacts");
	}
}
