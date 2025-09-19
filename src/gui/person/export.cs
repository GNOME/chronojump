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
 * Copyright (C) 2025   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Mono.Unix;
using System.Collections.Generic; //List

public partial class ChronoJumpWindow
{
        // at glade ---->                       
	Gtk.RadioButton radio_export_persons_current_session;
	Gtk.Label label_persons_export_destination;
	Gtk.Box box_persons_export_feedback;
	Gtk.Label persons_export_feedback_message;
	Gtk.Label persons_export_feedback_ok_url;
        // <---- at glade

	private enum notebook_persons_export_pages { MAIN, EXPORT }

	private void on_button_person_export_clicked (object o, EventArgs args)
	{
		notebook_persons_export.CurrentPage = Convert.ToInt32 (notebook_persons_export_pages.EXPORT);
	}

	private void on_button_persons_export_clicked (object o, EventArgs args)
	{
		if (radio_export_persons_current_session.Active)
			checkFile (Constants.CheckFileOp.PERSONS_EXPORT_THIS_SESSION);
		else
			checkFile (Constants.CheckFileOp.PERSONS_EXPORT_ANY_SESSION);
	}

	private void on_persons_export_this_session_selected (string destination)
	{
		persons_export_do (currentSession.UniqueID, destination);
	}
	private void on_persons_export_all_sessions_selected (string destination)
	{
		persons_export_do (-1, destination);
	}

	private  void persons_export_do (int sessionID, string destination)
	{
		persons_export_feedback_ok_url.Text = "";
		persons_export_feedback_ok_url.TooltipText = "";

		PersonsExport pe = new PersonsExport (sessionID, destination, preferences.CSVColumnDelimiter);
		bool success = pe.Do ();

		// labels
		persons_export_feedback_message.Text = pe.DoneMessage ();
		string url = "";
		if (success)
			url = pe.DoneOkURL ();

		persons_export_feedback_ok_url.Text = url;
		persons_export_feedback_ok_url.TooltipText = url;

		// TODO: if success show open file & open folder
	}

	private void on_overwrite_file_persons_export_this_session_accepted (object o, EventArgs args)
	{
		on_persons_export_this_session_selected (exportFileName);
	}
	private void on_overwrite_file_persons_export_all_sessions_accepted (object o, EventArgs args)
	{
		on_persons_export_all_sessions_selected (exportFileName);
	}

	private void connectWidgetsPersonsExport (Gtk.Builder builder)
	{
		radio_export_persons_current_session = (Gtk.RadioButton) builder.GetObject ("radio_export_persons_current_session");
		label_persons_export_destination = (Gtk.Label) builder.GetObject ("label_persons_export_destination");
		box_persons_export_feedback = (Gtk.Box) builder.GetObject ("box_persons_export_feedback");
		persons_export_feedback_message = (Gtk.Label) builder.GetObject ("persons_export_feedback_message");
		persons_export_feedback_ok_url = (Gtk.Label) builder.GetObject ("persons_export_feedback_ok_url");
	}
}
