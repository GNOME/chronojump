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

//this file has methods of ChronoJumpWindow related to manage menu

using System;
using Gdk;
using Gtk;
//using Glade;
using System.Collections.Generic; //List

public partial class ChronoJumpWindow
{
	Gtk.Arrow arrow_menu_show_session_up;
	Gtk.Arrow arrow_menu_show_session_down;
	Gtk.HPaned hpaned_contacts_main;
	Gtk.Viewport viewport_exit_confirm;
	Gtk.HBox hbox_social_network_poll;
	//Gtk.Viewport viewport_start_modes;
	Gtk.EventBox eventbox_check_menu_session;
	Gtk.EventBox eventbox_button_menu_session_new;
	Gtk.EventBox eventbox_button_menu_session_load;
	Gtk.EventBox eventbox_button_menu_session_more;
	Gtk.EventBox eventbox_button_menu_preferences;
	Gtk.EventBox eventbox_button_menu_help;
	Gtk.EventBox eventbox_button_menu_news;
	Gtk.EventBox eventbox_button_menu_exit;
	Gtk.VBox vbox_person;
	Gtk.Arrow arrow_manage_persons_left;
	Gtk.Arrow arrow_manage_persons_right;
	Gtk.Image image_button_person_close;
	Gtk.EventBox eventbox_check_manage_persons;
	Gtk.EventBox eventbox_persons_up;
	Gtk.EventBox eventbox_persons_down;
	Gtk.Label label_current_database;
	Gtk.Label label_current_session;
	Gtk.Label label_current_person;

	Gtk.Image image_cloud;
	Gtk.Box box_above_frame_database;
	Gtk.Frame frame_database;
	Gtk.Label label_database_at_frame_database;
	Gtk.Button button_menu_database;

	Gtk.CheckButton check_menu_session;
	Gtk.CheckButton check_manage_persons;
	//Gtk.Button button_menu_help;
	Gtk.Button button_menu_news;
	Gtk.VButtonBox vbuttonbox_menu_session;
	//Gtk.Alignment alignment_menu_person_options;
	Gtk.Label label_session_at_frame_session;
	Gtk.Label label_persons_at_frame_persons;

	/*
	Gtk.Button button_menu_session_new;
	Gtk.Button button_menu_session_load;
	Gtk.Button button_menu_session_more;
	Gtk.Button button_menu_preferences;
	*/
	Gtk.Button button_menu_exit;
	/*
	Gtk.Button button_menu_help_documents;
	Gtk.Button button_menu_help_shortcuts;
	Gtk.Button button_menu_help_about;
	*/
	Gtk.Button button_menu_guiTest;
	Gtk.Box box_prefs_help_news_exit;

	//just to manage width
	Gtk.Image image_session_import;
	Gtk.Image image_session_import1;
	Gtk.Image image_session_export;

	//menu icons
	//Gtk.Image image_menu_folders;
	Gtk.Image image_menu_folders2;
	Gtk.Image image_session_new3;
	Gtk.Image image_session_load2;
	Gtk.Image image_session_more;
	Gtk.Image image_session_edit2;
	Gtk.Image image_menu_preferences;
	Gtk.Image image_persons_manage;
	Gtk.Image image_menu_help;
	Gtk.Image image_menu_help_documents;
	Gtk.Image image_menu_help_shortcuts;
	Gtk.Image image_menu_help_about;
	Gtk.Image image_menu_news;
	Gtk.Image image_menu_quit;

	//for vertical align
	Gtk.HBox hbox_above_frame_session;
	Gtk.Alignment alignment_vbox_session_load_or_import_select;

	private void initialize_menu_or_menu_tiny()
	{
		if(preferences.personWinHide)
			menuTinyInitialize();
		else
			menuInitialize();
	}

	private void menus_and_mode_sensitive(bool sensitive)
	{
		LogB.Information("menus_and_mode_sensitive: " + sensitive.ToString());

		vbox_menu_tiny_menu.Sensitive = sensitive;
		alignment_session_persons.Sensitive = sensitive;

		hbox_change_modes_contacts.Sensitive = sensitive;
		hbox_change_modes_encoder.Sensitive = sensitive;
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
		menus_and_mode_sensitive(danger);
	}

	private void menuInitialize ()
	{
		menuSetColors();

		//LogB.Information("hpaned MinPosition: " + hpaned_contacts_main.MinPosition.ToString());

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
		/* commented at gtk3
		List <int> l = new List<int>();

		//menus
		l.Add(check_menu_session.SizeRequest().Width);
		l.Add(button_menu_help.SizeRequest().Width);
		l.Add(button_menu_news.SizeRequest().Width);
		l.Add(button_menu_exit.SizeRequest().Width);
		l.Add(button_menu_preferences.SizeRequest().Width);

		//submenus (16 is the horizontal separation of the submenu)
		l.Add(button_menu_session_new.SizeRequest().Width + 16);
		l.Add(button_menu_session_load.SizeRequest().Width + 16);
		l.Add(button_menu_session_more.SizeRequest().Width + 16);
		l.Add(button_menu_help_documents.SizeRequest().Width + 16);
		l.Add(button_menu_help_shortcuts.SizeRequest().Width + 16);
		l.Add(button_menu_help_about.SizeRequest().Width + 16);
		*/

		//int maxWidth = getMenuButtonsMaxWidth(l) + 4 + 6; //4, 6 are alignments spaces.

		/*
		LogB.Information(string.Format("viewport_persons: {0}", viewport_persons.SizeRequest().Width));
		LogB.Information(string.Format("frame_persons: {0}", frame_persons.SizeRequest().Width));
		LogB.Information(string.Format("frame_exhibition: {0}", frame_exhibition.SizeRequest().Width));
		LogB.Information(string.Format("frame_persons_top: {0}", frame_persons_top.SizeRequest().Width));
		LogB.Information(string.Format("treeview_persons: {0}", treeview_persons.SizeRequest().Width));
		LogB.Information(string.Format("vbox_persons_bottom: {0}", vbox_persons_bottom.SizeRequest().Width));
		LogB.Information(string.Format("hbox_persons_bottom_photo: {0}", hbox_persons_bottom_photo.SizeRequest().Width));
		LogB.Information(string.Format("image_current_person: {0}", image_current_person.SizeRequest().Width));
		LogB.Information(string.Format("hbox_rest_time: {0}", hbox_rest_time.SizeRequest().Width));
		*/

//		if(viewport_persons.SizeRequest().Width +4 +6 > maxWidth)
//			maxWidth = viewport_persons.SizeRequest().Width +4 + 6; //+4 due to alignment_person, +6 to alignment_viewport_menu_top
		//if(frame_persons.SizeRequest().Width > maxWidth)
		//	maxWidth = frame_persons.SizeRequest().Width;

//		viewport_menu_top.SetSizeRequest(maxWidth, -1); //-1 is height

		if(! Config.UseSystemColor && UtilGtk.ColorIsOkWithLogoTransparent (UtilGtk.ColorParse(preferences.colorBackgroundString)))
		{
			image_logo_contacts.Visible = false;
			image_logo_contacts_transp.Visible = true;
			image_logo_encoder.Visible = false;
			image_logo_encoder_transp.Visible = true;
			fullscreen_image_logo.Visible = false;
			fullscreen_image_logo_transp.Visible = true;
//			radio_show_menu_and_persons_adjust_height(true);
		} else {
			image_logo_contacts.Visible = true;
			image_logo_contacts_transp.Visible = false;
			image_logo_encoder.Visible = true;
			image_logo_encoder_transp.Visible = false;
			fullscreen_image_logo.Visible = true;
			fullscreen_image_logo_transp.Visible = false;
//			radio_show_menu_and_persons_adjust_height(false);
		}

		GLib.Timeout.Add(200, new GLib.TimeoutHandler(menuTopAlign));
	}

	private bool menuTopAlign()
	{
		uint alignTop = (uint) (notebook_capture_analyze.Allocation.Y
				//-hbox_above_frame_session.SizeRequest().Height);
				- UtilGtk.WidgetHeight (hbox_above_frame_session));

		alignment_session_persons.TopPadding = alignTop;
		alignment_vbox_session_load_or_import_select.TopPadding = alignTop;

		return false;
	}

	/*
	   unused on 2.1.3 but maybe use it for menu session
	private void radio_show_menu_and_persons_adjust_height(bool toTransparentImage)
	{
		if(toTransparentImage)
			hbox_radio_show_menu_and_persons.SetSizeRequest
				(-1, image_logo_contacts_transp.SizeRequest().Height);
		else
			hbox_radio_show_menu_and_persons.SetSizeRequest
				(-1, image_logo_contacts.SizeRequest().Height);
	}
	*/

	private void menuSetColors ()
	{
		//Gdk.Color color = UtilGtk.YELLOW;
		//Gdk.Color color = UtilGtk.BLUE_CHRONOJUMP;
		//Gdk.Color color = //#FFE891 //this is nice

		if(! Config.UseSystemColor)
		{
			RGBA color = UtilGtk.ColorParse (preferences.colorBackgroundString);

			UtilGtk.WindowColor (app1, color);
			UtilGtk.ViewportColor (viewport_exit_confirm, color);

			UtilGtk.WidgetColor (vbox_send_log, Config.ColorBackground);

			UtilGtk.WidgetColor (frame_send_log, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsFrame (Config.ColorBackgroundShiftedIsDark, frame_send_log);

			UtilGtk.WidgetColor (hbox_social_network_poll, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsWidget (Config.ColorBackgroundShiftedIsDark, hbox_social_network_poll);
		}

		UtilGtk.EventBoxColorBackgroundActive (eventbox_check_menu_session,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_session_new,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_session_load,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_session_more,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_preferences,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_help,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_news,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_menu_exit,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_check_manage_persons,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_persons_up,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_persons_down,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
	}

	private void menuShowVerticalArrow (bool selected, Gtk.Arrow a_up, Gtk.Arrow a_down)
	{
		a_up.Visible = selected;
		a_down.Visible = ! selected;
	}

	private void on_check_manage_persons_clicked (object o, EventArgs args)
	{
		if (check_manage_persons.Active)
		{
			app1s_notebook_sup_entered_from = notebook_sup.CurrentPage;
			notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.PERSON);

			//do not allow to use session buttons to not confuse the button_close actions
			vbuttonbox_menu_session.Sensitive = false;
			box_prefs_help_news_exit.Sensitive = false;

			arrow_manage_persons_left.Visible = true;
			arrow_manage_persons_right.Visible = false;
		} else {
			notebook_sup.CurrentPage = app1s_notebook_sup_entered_from;

			vbuttonbox_menu_session.Sensitive = true;
			box_prefs_help_news_exit.Sensitive = true;

			arrow_manage_persons_left.Visible = false;
			arrow_manage_persons_right.Visible = true;;
		}
	}
	private void on_button_person_close_clicked (object o, EventArgs args)
	{
		check_manage_persons.Click ();
	}

	private void on_check_menu_session_clicked (object o, EventArgs args)
	{
		menuShowVerticalArrow (check_menu_session.Active, arrow_menu_show_session_up, arrow_menu_show_session_down);
		vbuttonbox_menu_session.Visible = check_menu_session.Active;

		//hide the person photo if anything is unfolded
		if(preferences.personPhoto)
			vbox_persons_bottom.Visible = ! check_menu_session.Active;

		//scroll it, but wait a bit before to be all the things at place
		if(myTreeViewPersons != null)
			GLib.Timeout.Add(50, new GLib.TimeoutHandler(scrollTreeviewPersons));
	}

	private bool scrollTreeviewPersons ()
	{
		if(myTreeViewPersons != null) 		//extra check
			myTreeViewPersons.ScrollToSelectedRow ();

		return false;
	}

	private void on_button_menu_help_clicked (object o, EventArgs args)
	{
		menus_and_mode_sensitive(false);
		app1s_notebook_sup_entered_from = notebook_sup.CurrentPage;
		notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.HELP);
	}
	private void on_button_help_close_clicked (object o, EventArgs args)
	{
		menus_and_mode_sensitive(true);
		notebook_sup.CurrentPage = app1s_notebook_sup_entered_from;
	}

	private void on_button_menu_news_clicked (object o, EventArgs args)
	{
		newsGetThreadPrepare();
	}
	private void on_button_news_close_clicked (object o, EventArgs args)
	{
		menus_and_mode_sensitive(true);
		notebook_sup.CurrentPage = app1s_notebook_sup_entered_from;

		//put default news store icon because window has been opened (and hopefully seen)
		Pixbuf pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_store_blue.png");
		image_menu_news.Pixbuf = pixbuf;
		image_menu_news1.Pixbuf = pixbuf;
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
		app1s_label_session_set_name();
		//do not allow to export session SIMULATED because it could also not be imported
		button_menu_session_export.Sensitive = (currentSession != null && currentSession.Name != Constants.SessionSimulatedName);
	}

	private void connectWidgetsMenu (Gtk.Builder builder)
	{
		arrow_menu_show_session_up = (Gtk.Arrow) builder.GetObject ("arrow_menu_show_session_up");
		arrow_menu_show_session_down = (Gtk.Arrow) builder.GetObject ("arrow_menu_show_session_down");
		hpaned_contacts_main = (Gtk.HPaned) builder.GetObject ("hpaned_contacts_main");
		viewport_exit_confirm = (Gtk.Viewport) builder.GetObject ("viewport_exit_confirm");
		hbox_social_network_poll = (Gtk.HBox) builder.GetObject ("hbox_social_network_poll");
		//viewport_start_modes = (Gtk.Viewport) builder.GetObject ("viewport_start_modes");
		eventbox_check_menu_session = (Gtk.EventBox) builder.GetObject ("eventbox_check_menu_session");
		eventbox_button_menu_session_new = (Gtk.EventBox) builder.GetObject ("eventbox_button_menu_session_new");
		eventbox_button_menu_session_load = (Gtk.EventBox) builder.GetObject ("eventbox_button_menu_session_load");
		eventbox_button_menu_session_more = (Gtk.EventBox) builder.GetObject ("eventbox_button_menu_session_more");
		eventbox_button_menu_preferences = (Gtk.EventBox) builder.GetObject ("eventbox_button_menu_preferences");
		eventbox_button_menu_help = (Gtk.EventBox) builder.GetObject ("eventbox_button_menu_help");
		eventbox_button_menu_news = (Gtk.EventBox) builder.GetObject ("eventbox_button_menu_news");
		eventbox_button_menu_exit = (Gtk.EventBox) builder.GetObject ("eventbox_button_menu_exit");
		vbox_person = (Gtk.VBox) builder.GetObject ("vbox_person");
		arrow_manage_persons_left = (Gtk.Arrow) builder.GetObject ("arrow_manage_persons_left");
		arrow_manage_persons_right = (Gtk.Arrow) builder.GetObject ("arrow_manage_persons_right");
		image_button_person_close = (Gtk.Image) builder.GetObject ("image_button_person_close");
		eventbox_check_manage_persons = (Gtk.EventBox) builder.GetObject ("eventbox_check_manage_persons");
		eventbox_persons_up = (Gtk.EventBox) builder.GetObject ("eventbox_persons_up");
		eventbox_persons_down = (Gtk.EventBox) builder.GetObject ("eventbox_persons_down");
		label_current_database = (Gtk.Label) builder.GetObject ("label_current_database");
		label_current_session = (Gtk.Label) builder.GetObject ("label_current_session");
		label_current_person = (Gtk.Label) builder.GetObject ("label_current_person");

		image_cloud = (Gtk.Image) builder.GetObject ("image_cloud");
		box_above_frame_database = (Gtk.Box) builder.GetObject ("box_above_frame_database");
		frame_database = (Gtk.Frame) builder.GetObject ("frame_database");
		label_database_at_frame_database = (Gtk.Label) builder.GetObject ("label_database_at_frame_database");
		button_menu_database = (Gtk.Button) builder.GetObject ("button_menu_database");

		check_menu_session = (Gtk.CheckButton) builder.GetObject ("check_menu_session");
		check_manage_persons = (Gtk.CheckButton) builder.GetObject ("check_manage_persons");
		//button_menu_help = (Gtk.Button) builder.GetObject ("button_menu_help");
		button_menu_news = (Gtk.Button) builder.GetObject ("button_menu_news");
		vbuttonbox_menu_session = (Gtk.VButtonBox) builder.GetObject ("vbuttonbox_menu_session");
		//alignment_menu_person_options = (Gtk.Alignment) builder.GetObject ("alignment_menu_person_options");
		label_session_at_frame_session = (Gtk.Label) builder.GetObject ("label_session_at_frame_session");
		label_persons_at_frame_persons = (Gtk.Label) builder.GetObject ("label_persons_at_frame_persons");

		/*
		button_menu_session_new = (Gtk.Button) builder.GetObject ("button_menu_session_new");
		button_menu_session_load = (Gtk.Button) builder.GetObject ("button_menu_session_load");
		button_menu_session_more = (Gtk.Button) builder.GetObject ("button_menu_session_more");
		button_menu_preferences = (Gtk.Button) builder.GetObject ("button_menu_preferences");
		*/
		button_menu_exit = (Gtk.Button) builder.GetObject ("button_menu_exit");
		/*
		button_menu_help_documents = (Gtk.Button) builder.GetObject ("button_menu_help_documents");
		button_menu_help_shortcuts = (Gtk.Button) builder.GetObject ("button_menu_help_shortcuts");
		button_menu_help_about = (Gtk.Button) builder.GetObject ("button_menu_help_about");
		*/
		button_menu_guiTest = (Gtk.Button) builder.GetObject ("button_menu_guiTest");
		box_prefs_help_news_exit = (Gtk.Box) builder.GetObject ("box_prefs_help_news_exit");

		//just to manage width
		image_session_import = (Gtk.Image) builder.GetObject ("image_session_import");
		image_session_import1 = (Gtk.Image) builder.GetObject ("image_session_import1");
		image_session_export = (Gtk.Image) builder.GetObject ("image_session_export");

		//menu icons
		//image_menu_folders = (Gtk.Image) builder.GetObject ("image_menu_folders");
		image_menu_folders2 = (Gtk.Image) builder.GetObject ("image_menu_folders2");
		image_session_new3 = (Gtk.Image) builder.GetObject ("image_session_new3");
		image_session_load2 = (Gtk.Image) builder.GetObject ("image_session_load2");
		image_session_more = (Gtk.Image) builder.GetObject ("image_session_more");
		image_session_edit2 = (Gtk.Image) builder.GetObject ("image_session_edit2");
		image_menu_preferences = (Gtk.Image) builder.GetObject ("image_menu_preferences");
		image_persons_manage = (Gtk.Image) builder.GetObject ("image_persons_manage");
		image_menu_help = (Gtk.Image) builder.GetObject ("image_menu_help");
		image_menu_help_documents = (Gtk.Image) builder.GetObject ("image_menu_help_documents");
		image_menu_help_shortcuts = (Gtk.Image) builder.GetObject ("image_menu_help_shortcuts");
		image_menu_help_about = (Gtk.Image) builder.GetObject ("image_menu_help_about");
		image_menu_news = (Gtk.Image) builder.GetObject ("image_menu_news");
		image_menu_quit = (Gtk.Image) builder.GetObject ("image_menu_quit");

		//for vertical align
		hbox_above_frame_session = (Gtk.HBox) builder.GetObject ("hbox_above_frame_session");
		alignment_vbox_session_load_or_import_select = (Gtk.Alignment) builder.GetObject ("alignment_vbox_session_load_or_import_select");
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
