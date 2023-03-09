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
using System.IO;
using Gtk;
using Gdk;
//using Glade;
using Mono.Unix;

public class SplashWindow
{
	Gtk.Window splash_window;
	Gtk.Image image_logo;
	Gtk.ProgressBar progressbarVersion;
	Gtk.ProgressBar progressbarRate;
	Gtk.ProgressBar progressbarSubRate;
	Gtk.Label myLabel;
	Gtk.Button button_close;

	Gtk.Button button_open_database_folder;
	Gtk.Button button_open_docs_folder;

	Gtk.Button fakeButtonClose;


	public bool FakeButtonCreated = false;
	//string progVersion = "";

	static SplashWindow SplashWindowBox;

	public SplashWindow ()
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "splash_window.glade", "splash_window", "chronojump");
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "splash_window.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);
		
		//put an icon to window
		UtilGtk.IconWindow(splash_window);

		FakeButtonClose = new Gtk.Button();
		FakeButtonCreated = true;

		hideAllProgressbars();

		//hidden always excepted when called to be shown (see below)
		button_open_database_folder.Hide();
		button_open_docs_folder.Hide();

		//put logo image
		Pixbuf pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameLogo320);
		image_logo.Pixbuf = pixbuf;
	}

	static public SplashWindow Show ()
	{
		if (SplashWindowBox == null) {
			SplashWindowBox = new SplashWindow();
		}
		SplashWindowBox.splash_window.Show ();

		return SplashWindowBox;
	}

	private void hideAllProgressbars() 
	{
		progressbarVersion.Hide();
		progressbarRate.Hide();
		progressbarSubRate.Hide();
	}

	public void ShowProgressbar(string option) {
		if(option == "creating") {
			progressbarVersion.Show();
			progressbarSubRate.Show();
		} else if (option == "updating") {
			progressbarVersion.Show();
			progressbarRate.Show();
			progressbarSubRate.Show();
		}
	}

	public void UpdateProgressbar (string pbString, double fraction) {
		if(fraction < 0)
			fraction = 0;
		else if(fraction > 1)
			fraction = 1;

		if(pbString == "version")
			progressbarVersion.Fraction = fraction;
		else if(pbString == "rate")
			progressbarRate.Fraction = fraction;
		else 
			progressbarSubRate.Fraction = fraction;
	}
	
	public void UpdateLabel (string text)
	{
		myLabel.Text = text;
		myLabel.UseMarkup = true;
	}

	public void Show_button_open_database_folder () {
		button_open_database_folder.Show();
	}
	private void on_button_open_database_folder_clicked (object o, EventArgs args)
	{
		string database_url = Util.GetDatabaseDir() + System.IO.Path.DirectorySeparatorChar  + "chronojump.db";
		string database_temp_url = Util.GetDatabaseTempDir() + System.IO.Path.DirectorySeparatorChar  + "chronojump.db";

		System.IO.FileInfo file1 = new System.IO.FileInfo(database_url); //potser cal una arrobar abans (a windows)
		System.IO.FileInfo file2 = new System.IO.FileInfo(database_temp_url); //potser cal una arrobar abans (a windows)

		if(! file1.Exists && ! file2.Exists)
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.DatabaseNotFoundStr());

		string dir = "";
		if(file1.Exists)
			dir = Util.GetDatabaseDir();
		else if(file2.Exists)
			dir = Util.GetDatabaseTempDir();

		if(! Util.OpenURL (dir))
			new DialogMessage(Constants.MessageTypes.WARNING,
					Catalog.GetString("Error. Cannot open directory.") + "\n\n" + dir);
	}

	public void Show_button_open_docs_folder () {
		button_open_docs_folder.Show();
	}
	private void on_button_open_docs_folder_clicked (object o, EventArgs args)
	{
		if(! Util.OpenURL (Path.GetFullPath(Util.GetManualDir())))
			new DialogMessage(Constants.MessageTypes.WARNING,
					"Sorry, folder does not exist." + "\n\n" +
					Path.GetFullPath(Util.GetManualDir())
					);
	}

	public void ShowButtonClose()
	{
		button_close.Show();
	}

	public void Button_close_label (string str) {
		button_close.Label = str;
	}

	protected void on_button_close_clicked (object o, EventArgs args)
	{
		fakeButtonClose.Click();
		Hide();
	}

	public void Destroy () {
		//it seem on some machines (MacOSX) splash_window maybe is Destroyed previously because on_delete_event it's called
		//Destroy here if it has not been destroyed
		if(SplashWindowBox.splash_window == null)
			LogB.Information("splash_window is null. Do nothing.");
		else {
			LogB.Warning("splash_window is not null. Destroying now...");
			
			SplashWindowBox.splash_window.Destroy ();
			
			LogB.Information("Destroyed!");
		}
	}

	//on 1.5.0 on mac, does not crash on splashwin but sometimes it does not disappear the splash	
	//hide splashWin
	//TODO: rewrite all the splashWin flow here and on chronojump.cs
	static public void Hide () {
		LogB.Debug("splash_window hide start ");
		if(SplashWindowBox.splash_window != null) {
			LogB.Debug("splash_window hide != null");

			SplashWindowBox.splash_window.Visible = false;
			
			LogB.Debug("splash_window hide hidden");
		}
		LogB.Debug("splash_window hide end");
	}

	/*
	public string ProgVersion {
		set { grogVersion = value; }
	}
	*/

	private void on_delete_event (object o, DeleteEventArgs args) {
		splash_window.Destroy ();
	}

	public Button Button_close {
		get { return button_close; }
	}

	public Button FakeButtonClose
	{
		set { fakeButtonClose = value; }
		get { return fakeButtonClose; }
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		splash_window = (Gtk.Window) builder.GetObject ("splash_window");
		image_logo = (Gtk.Image) builder.GetObject ("image_logo");
		progressbarVersion = (Gtk.ProgressBar) builder.GetObject ("progressbarVersion");
		progressbarRate = (Gtk.ProgressBar) builder.GetObject ("progressbarRate");
		progressbarSubRate = (Gtk.ProgressBar) builder.GetObject ("progressbarSubRate");
		myLabel = (Gtk.Label) builder.GetObject ("myLabel");
		button_close = (Gtk.Button) builder.GetObject ("button_close");

		button_open_database_folder = (Gtk.Button) builder.GetObject ("button_open_database_folder");
		button_open_docs_folder = (Gtk.Button) builder.GetObject ("button_open_docs_folder");

		fakeButtonClose = (Gtk.Button) builder.GetObject ("fakeButtonClose");
	}
}
