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

public class DialogServerStats
{
	[Widget] Gtk.Dialog dialog_server_stats;

	[Widget] Gtk.Label label_evaluators;
	
	[Widget] Gtk.Label label_sessions_server;
	[Widget] Gtk.Label label_sessions_you;
	
	[Widget] Gtk.Label label_persons_server;
	[Widget] Gtk.Label label_persons_you;
	
	[Widget] Gtk.Label label_jumps_server;
	[Widget] Gtk.Label label_jumps_you;
	[Widget] Gtk.Label label_jumps_rj_server;
	[Widget] Gtk.Label label_jumps_rj_you;
	[Widget] Gtk.Label label_runs_server;
	[Widget] Gtk.Label label_runs_you;
	[Widget] Gtk.Label label_runs_i_server;
	[Widget] Gtk.Label label_runs_i_you;
	[Widget] Gtk.Label label_rt_server;
	[Widget] Gtk.Label label_rt_you;
	[Widget] Gtk.Label label_pulses_server;
	[Widget] Gtk.Label label_pulses_you;
	[Widget] Gtk.Label label_multichronopic_server;
	[Widget] Gtk.Label label_multichronopic_you;

	[Widget] Gtk.Label label_date;

	public DialogServerStats (string [] statsServer, string [] statsMine)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "dialog_server_stats.glade", "dialog_server_stats", "chronojump");
		gladeXML.Autoconnect(this);
	

		//put an icon to window
		UtilGtk.IconWindow(dialog_server_stats);

		foreach(string strWithText in statsServer) {
			string [] s = strWithText.Split(new char[] {':'});
			switch (s[0]) {
				case "Evaluators":
					label_evaluators.Text = s[1];
				break;
				case "Sessions":
					label_sessions_server.Text 	= s[1];
				break;
				case "Persons":
					label_persons_server.Text 	= s[1];
				break;
				case "Jumps":
					label_jumps_server.Text 	= s[1];
				break;
				case "JumpsRj":
					label_jumps_rj_server.Text 	= s[1];
				break;
				case "Runs":
					label_runs_server.Text 		= s[1];
				break;
				case "RunsInterval":
					label_runs_i_server.Text 	= s[1];
				break;
				case "ReactionTimes":
					label_rt_server.Text 		= s[1];
				break;
				case "Pulses":
					label_pulses_server.Text 	= s[1];
				break;
				case "MultiChronopic":
					label_multichronopic_server.Text= s[1];
				break;
				default:
					//do nothing
				break;
			}
		}

		foreach(string strWithText in statsMine) {
			string [] s = strWithText.Split(new char[] {':'});
			switch (s[0]) {
				case "Sessions":
					label_sessions_you.Text = s[1];
				break;
				case "Persons":
					label_persons_you.Text 	= s[1];
				break;
				case "Jumps":
					label_jumps_you.Text 	= s[1];
				break;
				case "JumpsRj":
					label_jumps_rj_you.Text = s[1];
				break;
				case "Runs":
					label_runs_you.Text 	= s[1];
				break;
				case "RunsInterval":
					label_runs_i_you.Text 	= s[1];
				break;
				case "ReactionTimes":
					label_rt_you.Text 	= s[1];
				break;
				case "Pulses":
					label_pulses_you.Text 	= s[1];
				break;
				case "MultiChronopic":
					label_multichronopic_you.Text= s[1];
				break;
				default:
					//do nothing
				break;
			}
		}
		
		label_date.Text = DateTime.Now.ToString();
	}


	public void on_button_close_clicked (object obj, EventArgs args) {
		dialog_server_stats.Destroy ();
	}

	private void on_delete_event (object o, DeleteEventArgs args) {
		dialog_server_stats.Destroy ();
	}
	
}
