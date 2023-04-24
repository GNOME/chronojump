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
using System.Collections.Generic;

public partial class ChronoJumpWindow
{
	// at glade ---->
	Gtk.Box box_presentation;
	Gtk.Box box_combo_presentation;
	Gtk.Button button_presentation_left;
	Gtk.Button button_presentation_right;
	Gtk.Image image_presentation_left;
	Gtk.Image image_presentation_right;
	// <---- at glade

	Gtk.ComboBoxText combo_presentation;
	List<string> presentation_l;

	private void connectWidgetsPresentation (Gtk.Builder builder)
	{
		box_presentation = (Gtk.Box) builder.GetObject ("box_presentation");
		box_combo_presentation = (Gtk.Box) builder.GetObject ("box_combo_presentation");
		button_presentation_left = (Gtk.Button) builder.GetObject ("button_presentation_left");
		button_presentation_right = (Gtk.Button) builder.GetObject ("button_presentation_right");
		image_presentation_left = (Gtk.Image) builder.GetObject ("image_presentation_left");
		image_presentation_right = (Gtk.Image) builder.GetObject ("image_presentation_right");
	}

	private void presentationPrepare ()
	{
		//List<string> presentation_l = new List<string> () { "hola", "bon dia", "adéu" };
		presentation_l = Util.ReadFileAsStringList (Util.GetPresentationFileName());
		if (presentation_l == null)
			return;

		combo_presentation = UtilGtk.CreateComboBoxText (box_combo_presentation, presentation_l, presentation_l[0]);

		button_presentation_left.Sensitive = false;
		box_presentation.Visible = true;
	}

	private void on_button_presentation_left_clicked (object o, EventArgs args)
	{
		bool isFirst;
		combo_presentation = UtilGtk.ComboSelectPrevious (combo_presentation, out isFirst);

		button_presentation_left.Sensitive = ! isFirst;
		button_presentation_right.Sensitive = true;
	}

	private void on_button_presentation_right_clicked (object o, EventArgs args)
	{
		bool isLast;
		combo_presentation = UtilGtk.ComboSelectNext (combo_presentation, out isLast);

		button_presentation_left.Sensitive = true;
		button_presentation_right.Sensitive = ! isLast;
	}
}

/* Old webkit code used on my thesis presentation

//using System.IO;
//using System.Threading;
//using Mono.Unix;
//using Gdk;
//using Glade;
//using System.Collections;
//using WebKit;

public partial class ChronoJumpWindow 
{
	//presentation
	[Widget] Gtk.Box vbox_presentation;
	[Widget] Gtk.Image image_presentation_logo;
	[Widget] Gtk.Label label_presentation_current;

	//static WebKit.WebView presentation;

	bool presentationInitialized = false;

	void on_menuitem_presentation_activate (object o, EventArgs args) {
		//vbox_presentation.Visible = ! vbox_presentation.Visible;
	}

 
	private void presentationInit() {
		//button_presentation_restore_screen.Sensitive = false;

		 * needs webKit
		 *
		presentation = new WebKit.WebView();
		scrolledwindow_presentation.Add(presentation);
		
		loadInitialPresentation();

		presentation.ShowAll();
	
		presentationInitialized = true;
	}
	
	void on_button_presentation_screen_clicked (object o, EventArgs args) {
		Gtk.Button button = (Gtk.Button) o;

		vbox_persons.Visible =	( button != button_presentation_fullscreen);
		notebook_sup.ShowTabs =	( button != button_presentation_fullscreen);
		//button_presentation_fullscreen.Sensitive =	( button != button_presentation_fullscreen);
		//button_presentation_restore_screen.Sensitive =	( button == button_presentation_fullscreen);
	}

	void on_button_presentation_reload_clicked (object o, EventArgs args) 
	{
		if(! presentationInitialized)
			presentationInit();
		else
			loadInitialPresentation();
	}

	//TODO: in the future read the divs on the HTML
	int presentation_slide_current = 0;
	int presentation_slide_max = 10;

	void on_button_presentation_previous_clicked (object o, EventArgs args)
	{
		if(! presentationInitialized)
			presentationInit();
		else {
			presentation_slide_current --;
			presentationSlideChange();
		}
	}
	void on_button_presentation_next_clicked (object o, EventArgs args)
	{
		if(! presentationInitialized)
			presentationInit();
		else {
			presentation_slide_current ++;
			presentationSlideChange();
		}
	}
	void presentationSlideChange() 
	{
		if (presentation_slide_current < 0)
			presentation_slide_current = 0;
		else if (presentation_slide_current > presentation_slide_max)
			presentation_slide_current = presentation_slide_max;

		openPresentation();
		updatePresentationLabel();
	}

	void openPresentation() {
		//presentationOpenStatic("file:///home/...html#" + presentation_slide_current.ToString());
		string file = Path.Combine (Util.GetLocalDataDir (false) +
				Path.DirectorySeparatorChar + "Chronojump-Boscosystem.html");
		if(File.Exists(file))
			presentationOpenStatic("file://" + file + "#" + presentation_slide_current.ToString());
	}

	void updatePresentationLabel() {
		label_presentation_current.Text = 
			(presentation_slide_current +1).ToString() + " / " + 
			(presentation_slide_max +1).ToString();
	}
	
	private void loadInitialPresentation()
	{
		LogB.Information("Loading");
		
		presentation_slide_current = 0;
		openPresentation();
		updatePresentationLabel();
		
		LogB.Information("Loaded");
	}

	private static void presentationOpenStatic(string url) {
		 * needs WebKit
		 *
		presentation.Open(url);
	}
}
*/
