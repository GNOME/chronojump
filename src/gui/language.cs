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
 * Copyright (C) 2004-2014   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Glade;
using Mono.Unix;

public class LanguageWindow
{
	[Widget] Gtk.Window language_window;
	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.Box hbox_combo_language;
	[Widget] Gtk.ComboBox combo_language;
	[Widget] Gtk.Label label_linux_restart;
	

	static LanguageWindow LanguageWindowBox;
	
	public LanguageWindow ()
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "language_window", "chronojump");
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(language_window);
	}

	static public LanguageWindow Show ()
	{
		if (LanguageWindowBox == null) {
			LanguageWindowBox = new LanguageWindow();
		}
		
		LanguageWindowBox.createComboLanguage();

		/*
		if(! UtilAll.IsWindows())
			LanguageWindowBox.label_linux_restart.Text =
				Catalog.GetString("On GNU/Linux you will need to restart Chronojump");
		*/


		LanguageWindowBox.language_window.Show ();
		
		return LanguageWindowBox;
	}
	
	//private void createComboLanguage(string myLanguage) {
	private void createComboLanguage() {
		combo_language = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_language, Util.GetLanguagesNames(), "");
		

		hbox_combo_language.PackStart(combo_language, false, false, 0);
		hbox_combo_language.ShowAll();

		combo_language.Active = UtilGtk.ComboMakeActive(Util.GetLanguagesNames(), Util.GetLanguageName(Constants.LanguageDefault));

		//if(UtilAll.IsWindows())
			combo_language.Sensitive = true;
		//else 
		//	combo_language.Sensitive = false;
	}
			
	//void on_language_destroy (object o, EventArgs args)
	void on_delete_event (object o, DeleteEventArgs args)
	{
		/*
		 * do nothing, cannot be deleted because:
		 * - windows decorations are not shown
		 * - and is not shown on taskBar and Paginator
		 *
		 * LanguageWindowBox.language_window.Hide();
		 * LanguageWindowBox = null;
		*/
	}
	

	protected void on_button_accept_clicked (object o, EventArgs args)
	{
		SqlitePreferences.Update("language", Util.GetLanguageCodeFromName(UtilGtk.ComboGetActive(LanguageWindowBox.combo_language)), false);

		LanguageWindowBox.language_window.Hide();
		LanguageWindowBox = null;
	}
	
	public Button ButtonAccept 
	{
		get {
			return button_accept;
		}
	}

	~LanguageWindow() {}
	
}

