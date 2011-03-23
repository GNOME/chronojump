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
 * Copyright (C) 2004-2011   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using Gtk;

//only to easy pass data
public class ExecutingGraphData
{
	public Gtk.Button Button_cancel;
	public Gtk.Button Button_finish;
	public Gtk.Label Label_sync_message;
	public Gtk.Label Label_message1;
	public Gtk.Label Label_message2;
	public Gtk.Label Label_event_value;
	public Gtk.Label Label_time_value;
	public Gtk.ProgressBar Progressbar_event;
	public Gtk.ProgressBar Progressbar_time;
	
	public ExecutingGraphData(
			Gtk.Button Button_cancel, Gtk.Button Button_finish, 
			Gtk.Label Label_sync_message, Gtk.Label Label_message1, Gtk.Label Label_message2,
			Gtk.Label Label_event_value, Gtk.Label Label_time_value,
			Gtk.ProgressBar Progressbar_event, Gtk.ProgressBar Progressbar_time) 
	{
		this.Button_cancel =  Button_cancel;
		this.Button_finish =  Button_finish;
		this.Label_sync_message =  Label_sync_message;
		this.Label_message1 =  Label_message1;
		this.Label_message2 =  Label_message2;
		this.Label_event_value =  Label_event_value;
		this.Label_time_value =  Label_time_value;
		this.Progressbar_event =  Progressbar_event;
		this.Progressbar_time =  Progressbar_time;
	}

	public ExecutingGraphData() {
	}
}	
