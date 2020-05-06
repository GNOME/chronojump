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
 * Copyright (C) 2018-2020   Xavier de Blas <xaviblas@gmail.com> 
 */

//this file has methods of ChronoJumpWindow related to manage menu

using System;
using Gtk;
using Glade;
using System.Collections.Generic; //List

public partial class ChronoJumpWindow
{
	[Widget] Gtk.Alignment alignment_buttons_menu_and_persons;
	[Widget] Gtk.Arrow arrow_menu_show_session_up;
	[Widget] Gtk.Arrow arrow_menu_show_session_down;
	[Widget] Gtk.Arrow arrow_menu_show_help_up;
	[Widget] Gtk.Arrow arrow_menu_show_help_down;
	[Widget] Gtk.HBox hbox_radio_show_menu_and_persons;
	[Widget] Gtk.RadioButton radio_show_menu;
	[Widget] Gtk.RadioButton radio_show_persons;
	[Widget] Gtk.HPaned hpaned_contacts_main;
	[Widget] Gtk.Alignment alignment_viewport_menu_top;
	[Widget] Gtk.Viewport viewport_hpaned_contacts_main;
	[Widget] Gtk.Viewport viewport_send_log;
	[Widget] Gtk.Viewport viewport_exit_confirm;
	[Widget] Gtk.Viewport viewport_start_modes;
	[Widget] Gtk.Viewport viewport_menu_top;
	[Widget] Gtk.Viewport viewport_menu;
	[Widget] Gtk.Viewport viewport_persons;
	[Widget] Gtk.EventBox eventbox_radio_show_menu;
	[Widget] Gtk.EventBox eventbox_radio_show_persons;
	[Widget] Gtk.EventBox eventbox_button_show_modes;
	[Widget] Gtk.EventBox eventbox_check_menu_session;
	[Widget] Gtk.EventBox eventbox_button_menu_session_new;
	[Widget] Gtk.EventBox eventbox_button_menu_session_load;
	[Widget] Gtk.EventBox eventbox_button_menu_session_more;
	[Widget] Gtk.EventBox eventbox_button_menu_preferences;
	[Widget] Gtk.EventBox eventbox_check_menu_help;
	[Widget] Gtk.EventBox eventbox_button_menu_help_documents;
	[Widget] Gtk.EventBox eventbox_button_menu_help_accelerators;
	[Widget] Gtk.EventBox eventbox_button_menu_help_about;
	[Widget] Gtk.EventBox eventbox_button_menu_exit;
	[Widget] Gtk.CheckButton check_menu_session;
	[Widget] Gtk.CheckButton check_menu_help;
	[Widget] Gtk.VBox vbox_menu_session;
	[Widget] Gtk.Alignment alignment_menu_session_options;
	[Widget] Gtk.Alignment alignment_menu_person_options;
	[Widget] Gtk.Alignment alignment_menu_help_options;

	[Widget] Gtk.Button button_show_modes;
	[Widget] Gtk.Button button_session_edit;
	[Widget] Gtk.Button button_session_delete;
	[Widget] Gtk.Button button_session_export;

	[Widget] Gtk.Button button_menu_session_new;
	[Widget] Gtk.Button button_menu_session_load;
	[Widget] Gtk.Button button_menu_session_more;
	[Widget] Gtk.Button button_menu_preferences;
	[Widget] Gtk.Button button_menu_exit;
	[Widget] Gtk.Button button_menu_help_documents;
	[Widget] Gtk.Button button_menu_help_accelerators;
	[Widget] Gtk.Button button_menu_help_about;

	[Widget] Gtk.Image image_session_import;
	[Widget] Gtk.Image image_session_export;

	//menu icons
	[Widget] Gtk.Image image_radio_show_menu;
	[Widget] Gtk.Image image_radio_show_persons;
	[Widget] Gtk.Image image_menu_folders;
	[Widget] Gtk.Image image_session_new3;
	[Widget] Gtk.Image image_session_load2;
	[Widget] Gtk.Image image_session_more;
	[Widget] Gtk.Image image_session_edit2;
	[Widget] Gtk.Image image_button_show_modes;
	[Widget] Gtk.Image image_menu_preferences;
	[Widget] Gtk.Image image_menu_help;
	[Widget] Gtk.Image image_menu_help_documents;
	[Widget] Gtk.Image image_menu_help_accelerators;
	[Widget] Gtk.Image image_menu_help_about;
	[Widget] Gtk.Image image_menu_quit;

	private void initialize_menu_or_menu_tiny()
	{
		if(preferences.menuType == Preferences.MenuTypes.ICONS)
			menuTinyInitialize();
		else
			menuInitialize();
	}

	private void menu_and_menu_tiny_show_modes()
	{
		//need to change both because user can change PersonWinHide
		button_show_modes1.Sensitive = true;
		button_show_modes.Sensitive = true;
	}

	private void menus_sensitive(bool sensitive)
	{
		radio_show_menu.Sensitive = sensitive;
		viewport_menu.Sensitive = sensitive;
		vbox_menu_tiny_menu.Sensitive = sensitive;
	}

	private void menuInitialize ()
	{
		menuSetTextAndIcons();
		menuSetColors();

		//LogB.Information("hpaned MinPosition: " + hpaned_contacts_main.MinPosition.ToString());

		//unselect menu_help if selected
		if(check_menu_help.Active)
			check_menu_help.Active = false;
		alignment_menu_help_options.Visible = false;

		/*
		//do 1 and then 2 to ensure menu is shrinked after changing to icons
		//1
		hpaned_contacts_main = new Gtk.HPaned();
		hpaned_contacts_main.Pack1(alignment_viewport_menu_top, false, false);
		hpaned_contacts_main.Pack2(notebook_sup, true, false);
		hpaned_contacts_main.Show();
		*/

		//2 (1 seems not needed)
		//this is done to ensure hidden buttons will be shown (because also submenu items seems to have Allocation=1)
		//if we need it, pass also the other buttons but without the +16
		List <int> l = new List<int>();

		//menus
		l.Add(check_menu_session.SizeRequest().Width);
		l.Add(button_show_modes.SizeRequest().Width);
		l.Add(check_menu_help.SizeRequest().Width);
		l.Add(button_menu_exit.SizeRequest().Width);
		l.Add(button_menu_preferences.SizeRequest().Width);

		//submenus (16 is the horizontal separation of the submenu)
		l.Add(button_menu_session_new.SizeRequest().Width + 16);
		l.Add(button_menu_session_load.SizeRequest().Width + 16);
		l.Add(button_menu_session_more.SizeRequest().Width + 16);
		l.Add(button_menu_help_documents.SizeRequest().Width + 16);
		l.Add(button_menu_help_accelerators.SizeRequest().Width + 16);
		l.Add(button_menu_help_about.SizeRequest().Width + 16);

		int maxWidth = getMenuButtonsMaxWidth(l) + 4 + 6; //4, 6 are alignments spaces.

		/*
		LogB.Information(string.Format("viewport_persons: {0}", viewport_persons.SizeRequest().Width));
		LogB.Information(string.Format("frame_persons: {0}", frame_persons.SizeRequest().Width));
		LogB.Information(string.Format("frame_exhibition: {0}", frame_exhibition.SizeRequest().Width));
		LogB.Information(string.Format("frame_persons_top: {0}", frame_persons_top.SizeRequest().Width));
		LogB.Information(string.Format("treeview_persons: {0}", treeview_persons.SizeRequest().Width));
		LogB.Information(string.Format("vbox_persons_bottom: {0}", vbox_persons_bottom.SizeRequest().Width));
		LogB.Information(string.Format("hbox_persons_bottom_no_photo: {0}", hbox_persons_bottom_no_photo.SizeRequest().Width));
		LogB.Information(string.Format("hbox_persons_bottom_photo: {0}", hbox_persons_bottom_photo.SizeRequest().Width));
		LogB.Information(string.Format("image_current_person: {0}", image_current_person.SizeRequest().Width));
		LogB.Information(string.Format("button_image_current_person_zoom: {0}", button_image_current_person_zoom.SizeRequest().Width));
		LogB.Information(string.Format("hbox_rest_time: {0}", hbox_rest_time.SizeRequest().Width));
		*/

		if(viewport_persons.SizeRequest().Width +4 +6 > maxWidth)
			maxWidth = viewport_persons.SizeRequest().Width +4 + 6; //+4 due to alignment_person, +6 to alignment_viewport_menu_top
		//if(frame_persons.SizeRequest().Width > maxWidth)
		//	maxWidth = frame_persons.SizeRequest().Width;

		viewport_menu_top.SetSizeRequest(maxWidth, -1); //-1 is height

		if(UtilGtk.ColorIsOkWithLogoTransparent (UtilGtk.ColorParse(preferences.colorBackgroundString)))
		{
			frame_logo_contacts.Visible = false;
			image_logo_contacts_transp.Visible = true;
			frame_logo_encoder.Visible = false;
			image_logo_encoder_transp.Visible = true;
			radio_show_menu_and_persons_adjust_height(true);
		} else {
			frame_logo_contacts.Visible = true;
			image_logo_contacts_transp.Visible = false;
			frame_logo_encoder.Visible = true;
			image_logo_encoder_transp.Visible = false;
			radio_show_menu_and_persons_adjust_height(false);
		}
	}

	private void radio_show_menu_and_persons_adjust_height(bool toTransparentImage)
	{
		if(toTransparentImage)
			hbox_radio_show_menu_and_persons.SetSizeRequest
				(-1, image_logo_contacts_transp.SizeRequest().Height);
		else
			hbox_radio_show_menu_and_persons.SetSizeRequest
				(-1, frame_logo_contacts.SizeRequest().Height);
	}

	private void menuSetTextAndIcons ()
	{
		//icons
		image_menu_folders.Visible = preferences.menuType != Preferences.MenuTypes.TEXT;
		image_session_new3.Visible = preferences.menuType != Preferences.MenuTypes.TEXT;
		image_session_load2.Visible = preferences.menuType != Preferences.MenuTypes.TEXT;
		image_session_edit2.Visible = preferences.menuType != Preferences.MenuTypes.TEXT;
		image_button_show_modes.Visible = preferences.menuType != Preferences.MenuTypes.TEXT;
		image_menu_preferences.Visible = preferences.menuType != Preferences.MenuTypes.TEXT;
		image_menu_help.Visible = preferences.menuType != Preferences.MenuTypes.TEXT;
		image_menu_help_documents.Visible = preferences.menuType != Preferences.MenuTypes.TEXT;
		image_menu_help_accelerators.Visible = preferences.menuType != Preferences.MenuTypes.TEXT;
		image_menu_help_about.Visible = preferences.menuType != Preferences.MenuTypes.TEXT;
		image_menu_quit.Visible = preferences.menuType != Preferences.MenuTypes.TEXT;
	}

	private void menuSetColors ()
	{
		//Gdk.Color color = UtilGtk.YELLOW;
		//Gdk.Color color = UtilGtk.BLUE_CHRONOJUMP;
		//Gdk.Color color = //#FFE891 //this is nice

		Gdk.Color color = UtilGtk.ColorParse(preferences.colorBackgroundString);

		UtilGtk.ViewportColor(viewport_hpaned_contacts_main, color);
		UtilGtk.ViewportColor(viewport_send_log, color);
		UtilGtk.ViewportColor(viewport_exit_confirm, color);
		UtilGtk.ViewportColor(viewport_menu_top, color);
		UtilGtk.ViewportColor(viewport_menu, color);
		//UtilGtk.ViewportColor(viewport_menu, UtilGtk.BLUE_CLEAR2);
		//UtilGtk.ViewportColor(viewport_menu, UtilGtk.YELLOW);
		//UtilGtk.ViewportColor(viewport_menu, UtilGtk.GRAY_LIGHT);
		//UtilGtk.ViewportColor(viewport_persons, UtilGtk.BLUE_CLEAR2);

		UtilGtk.EventBoxColorBackgroundActive (eventbox_radio_show_menu, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_radio_show_persons, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_show_modes, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_check_menu_session, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_preferences, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_check_menu_help, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_exit, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);

		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_session_new, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_session_load, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_session_more, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_help_documents, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_help_accelerators, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_help_about, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
	}

	private void menuShowVerticalArrow (bool selected, Gtk.Arrow a_up, Gtk.Arrow a_down)
	{
		a_up.Visible = selected;
		a_down.Visible = ! selected;
	}

	private void on_radio_show_menu_toggled (object o, EventArgs args)
	{
		if(check_menu_help.Active)
			check_menu_help.Active = false;

		viewport_menu.Visible = true;
		viewport_persons.Visible = false;
	}
	private void on_radio_show_persons_toggled (object o, EventArgs args)
	{
		viewport_menu.Visible = false;
		viewport_persons.Visible = (currentSession != null);
	}

	private void on_check_menu_session_clicked (object o, EventArgs args)
	{
		menuShowVerticalArrow (check_menu_session.Active, arrow_menu_show_session_up, arrow_menu_show_session_down);
		if(check_menu_session.Active)
		{
			check_menu_help.Active = false;
			alignment_menu_session_options.Visible = true;

			alignment_menu_session_options.Show();
		} else
			alignment_menu_session_options.Visible = false;
	}

	private void on_check_menu_help_clicked (object o, EventArgs args)
	{
		menuShowVerticalArrow (check_menu_help.Active, arrow_menu_show_help_up, arrow_menu_show_help_down);
		if(check_menu_help.Active)
		{
			check_menu_session.Active = false;
			alignment_menu_help_options.Visible = true;
		} else
			alignment_menu_help_options.Visible = false;
	}

	private int getMenuButtonsMaxWidth(List<int> l)
	{
		int max = 0;
		foreach(int i in l)
			if(i > max)
				max = i;

		return max;
	}

	private void on_button_menu_session_more_clicked (object o, EventArgs args)
	{
		menus_sensitive_import_not_danger(false);

		//store which page we are on notebook_sup, except if we clicked on "more" from the session tab
		if(notebook_sup.CurrentPage != Convert.ToInt32(notebook_sup_pages.SESSION))
			app1s_notebook_sup_entered_from = notebook_sup.CurrentPage;

		notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.SESSION);
		app1s_notebook.CurrentPage = Convert.ToInt32(app1s_PAGE_MODES);
	}

	/* if import started we need to reload at the end
	 * we will reload if there's a cancel there
	 * dangerous situation is when treeview of sessions have been loaded
	 * and then we exit, eg. with back and then cancel
	 * so that cancel has a reloadSession()
	 * but ensure user will not be able to manage database in other way by clicking session or preferences
	 *
	 * also is nice to have the menu_more unsensitive
	 */
	private void menus_sensitive_import_not_danger(bool danger)
	{
		alignment_buttons_menu_and_persons.Sensitive = danger;
		vbox_menu_session.Sensitive = danger;
		vbox_menu_session1.Sensitive = danger;
		button_menu_preferences.Sensitive = danger;
		button_menu_preferences1.Sensitive = danger;
	}

	private void on_button_show_modes_clicked (object o, EventArgs args)
	{
		show_start_page();
		button_show_modes.Sensitive = false;
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
