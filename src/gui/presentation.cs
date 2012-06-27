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


public partial class ChronoJumpWindow 
{
	//presentation
	[Widget] Gtk.ScrolledWindow scrolledwindow_presentation;
	[Widget] Gtk.Button button_presentation_fullscreen;
	[Widget] Gtk.Button button_presentation_restore_screen;

	static WebKit.WebView presentation;
	Thread presentationThread;

 
	private void presentationInit() {
		button_presentation_restore_screen.Sensitive = false;

		presentation = new WebKit.WebView();
		scrolledwindow_presentation.Add(presentation);
		
		loadInitialPresentation();

		presentation.ShowAll();
	}
	void on_button_presentation_screen_clicked (object o, EventArgs args) {
		Gtk.Button button = (Gtk.Button) o;
		main_menu.Visible =	( button != button_presentation_fullscreen);
		vbox_persons.Visible =	( button != button_presentation_fullscreen);
		notebook_sup.ShowTabs =	( button != button_presentation_fullscreen);
		button_presentation_fullscreen.Sensitive =	( button != button_presentation_fullscreen);
		button_presentation_restore_screen.Sensitive =	( button == button_presentation_fullscreen);
	}

	void on_button_presentation_reload_clicked (object o, EventArgs args) {
		loadInitialPresentation();
	}

	private void loadInitialPresentation(){
		//presentationOpenStatic("file:///home/xavier/Documents/academic/investigacio/tesi_chronojump/presentacio_tesi_defensa_blanquerna/shells/embedder_meu.html#file:///home/xavier/Documents/academic/investigacio/tesi_chronojump/presentacio_tesi_defensa_blanquerna/tesi_chronojump.html");
	}

	private static void presentationOpenStatic(string url) {
		presentation.Open(url);
	}
	
}

