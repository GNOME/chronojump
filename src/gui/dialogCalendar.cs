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
//using Glade;

public class DialogCalendar
{
	Gtk.Dialog dialog_calendar;
	Gtk.Calendar calendar1;
	
	private DateTime myDateTime;
	private bool signalsActive;
	
	//for raise a signal and manage it on guis/session.cs (and other places)
	protected Gtk.Button fakeButtonDateChanged;

	public DialogCalendar (string calendarTitle, DateTime dateInitial)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "dialog_calendar.glade", "dialog_calendar", "chronojump");
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "dialog_calendar.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);
	
		signalsActive = false;
		
		//put an icon to window
		UtilGtk.IconWindow(dialog_calendar);
		
		dialog_calendar.Title = calendarTitle; 

		calendar1.Date = dateInitial;

		fakeButtonDateChanged = new Button();
		
		signalsActive = true;
	}
				
	void on_calendar1_day_selected (object obj, EventArgs args)
	{
		/* 
		   when dialog starts, calendar1.Date = dateInitial changes date
		   and raises this and it's too early
		   */
		if(! signalsActive)
			return;

		try {
			Calendar activatedCalendar = (Calendar) obj;
			myDateTime = activatedCalendar.Date;

			//raise a signal
			fakeButtonDateChanged.Click();
		}
		catch {
			/* 
			   when dialog starts, calendar1.Date = dateInitial changes date
			   and raises this and it's too early
			   */
		}
	}


	public void on_button_close_clicked (object obj, EventArgs args) {
		dialog_calendar.Destroy ();
	}

	private void on_delete_event (object o, DeleteEventArgs args) {
		dialog_calendar.Destroy ();
	}
	
	public Gtk.Button FakeButtonDateChanged
	{
		get { return	fakeButtonDateChanged; }
	}
	
	public DateTime MyDateTime
	{
		get { return	myDateTime; }
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		dialog_calendar = (Gtk.Dialog) builder.GetObject ("dialog_calendar");
		calendar1 = (Gtk.Calendar) builder.GetObject ("calendar1");
	}
}
