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
 * Copyright (C) 2004-2012   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO; 
using Gtk;
using Gdk;
using Glade;
using System.Collections;
using System.Threading;
using Mono.Unix;


///http://archive09.linux.com/feature/154784


public partial class ChronoJumpWindow 
{
	//presentation
	[Widget] Gtk.ScrolledWindow scrolledwindow_presentation;
	[Widget] Gtk.Button button_presentation_fullscreen;
	[Widget] Gtk.Button button_presentation_restore_screen;
//	[Widget] Gtk.Button button_presentation_first;
//	[Widget] Gtk.Button button_presentation_previous;
//	[Widget] Gtk.Button button_presentation_next;
//	[Widget] Gtk.Button button_presentation_stop;
	[Widget] Gtk.Label label_presentation_num;

	static WebKit.WebView presentation;
	Thread presentationThread;
	bool presentationCancel;
	string presentationURL;

 
	private void presentationInit() {
		button_presentation_restore_screen.Sensitive = false;
//		button_presentation_stop.Sensitive = false;	

		presentation = new WebKit.WebView();
		scrolledwindow_presentation.Add(presentation);
		
		//presentation.Open("file:///home/xavier/informatica/progs_meus/mono/navegador-gtk-html/prova.html");
		presentationOpenStatic("http://www.chronojump.org");
		
		presentation.ShowAll();
	}

	private static void presentationOpenStatic(string url) {
		presentation.Open(url);
	}
	
	void on_button_presentation_screen_clicked (object o, EventArgs args) {
		Gtk.Button button = (Gtk.Button) o;
		main_menu.Visible =	( button != button_presentation_fullscreen);
		vbox_persons.Visible =	( button != button_presentation_fullscreen);
		notebook_sup.ShowTabs =	( button != button_presentation_fullscreen);
		button_presentation_fullscreen.Sensitive =	( button != button_presentation_fullscreen);
		button_presentation_restore_screen.Sensitive =	( button == button_presentation_fullscreen);
	}

	void on_button_presentation_first_clicked (object o, EventArgs args) {
	//	presentationOpenPre("http://www.chronojump.org");
	}

	void on_button_presentation_previous_clicked (object o, EventArgs args) {
	//	presentationOpenPre("http://www.xckd.org");
	}

	void on_button_presentation_next_clicked (object o, EventArgs args) {
	//	presentationOpenPre("http://www.gnome.org");
	}
	
	void on_button_presentation_stop_clicked (object o, EventArgs args) {
	//	presentationCancel = true;
		//presentation.Destroy();
	}

}

