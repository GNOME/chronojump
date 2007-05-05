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
 * Xavier de Blas: 
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
 */

using System;
using Gtk;
using Glade;

public class DialogCalendar
{
	[Widget] Gtk.Dialog dialog_calendar;
	[Widget] Gtk.Calendar calendar1;
	
	private DateTime myDateTime;
	
	//for raise a signal and manage it on guis/session.cs (and other places)
	protected Gtk.Button fakeButtonDateChanged;

	public DialogCalendar (string calendarTitle)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "dialog_calendar", null);
		gladeXML.Autoconnect(this);
		
		dialog_calendar.Title = calendarTitle; 

		fakeButtonDateChanged = new Button();
	}
				
	void on_calendar1_day_selected (object obj, EventArgs args)
	{
		Calendar activatedCalendar = (Calendar) obj;

		//Console.WriteLine ( activatedCalendar.GetDate().ToLongDateString());
		myDateTime = activatedCalendar.Date;

		//raise a signal
		fakeButtonDateChanged.Click();
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
}
