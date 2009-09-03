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
 * Copyright (C) 2004-2009   Xavier de Blas <xaviblas@gmail.com> 
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
	[Widget] Gtk.Button button_cancel;

	
	public Gtk.Button fakeButtonCancel;
	public bool FakeButtonCreated = false;
	
	static SplashWindow SplashWindowBox;

	public SplashWindow ()
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "splash_window", null);
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(splash_window);

		fakeButtonCancel = new Gtk.Button();
		FakeButtonCreated = true;

		CancelButtonShow(false);
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
	
	/* 
	 * cancel	
	 * for SPing
	 */
	public void CancelButtonShow(bool show) {
		if(show)
			button_cancel.Show();
		else
			button_cancel.Hide();
	}
	
	protected void on_button_cancel_clicked (object o, EventArgs args)
	{
		fakeButtonCancel.Click();
	}
	
	public Button FakeButtonCancel 
	{
		set { fakeButtonCancel = value; }
		get { return fakeButtonCancel; }
	}

			

	public void Destroy () {
		SplashWindowBox.splash_window.Destroy ();
	}

	private void on_delete_event (object o, DeleteEventArgs args) {
		splash_window.Destroy ();
	}
}

