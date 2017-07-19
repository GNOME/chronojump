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
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Gdk;
using Glade;

public class SplashWindow
{
	[Widget] Gtk.Window splash_window;
	[Widget] Gtk.Image image_logo;
	[Widget] Gtk.ProgressBar progressbarVersion;
	[Widget] Gtk.ProgressBar progressbarRate;
	[Widget] Gtk.ProgressBar progressbarSubRate;
	[Widget] Gtk.Label myLabel;
	[Widget] Gtk.Button button_close;

	[Widget] Gtk.Button fakeButtonClose;
	public bool FakeButtonCreated = false;
	
	static SplashWindow SplashWindowBox;

	public SplashWindow ()
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "splash_window.glade", "splash_window", "chronojump");
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(splash_window);

		FakeButtonClose = new Gtk.Button();
		FakeButtonCreated = true;

		hideAllProgressbars();

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
	
	public void UpdateLabel (string text) {
		myLabel.Text = text;
	}

	public void ShowButtonClose()
	{
		button_close.Show();
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

	private void on_delete_event (object o, DeleteEventArgs args) {
		splash_window.Destroy ();
	}

	public Button FakeButtonClose
	{
		set { fakeButtonClose = value; }
		get { return fakeButtonClose; }
	}

}
