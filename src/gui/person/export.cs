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
	Gtk.ButtonBox buttonbox_person_export_feedback;
	Gtk.Label label_persons_export_csv_delim;
        // <---- at glade

	private enum notebook_persons_export_pages { MAIN, EXPORT }
	private PersonsExport personsExport;

	private void on_button_person_export_clicked (object o, EventArgs args)
	{
		notebook_persons_export.CurrentPage = Convert.ToInt32 (notebook_persons_export_pages.EXPORT);

		persons_export_feedback_message.Text = "";
		persons_export_feedback_ok_url.Text = "";
		persons_export_feedback_ok_url.TooltipText = "";
		label_persons_export_csv_delim.Text = Constants.GetSpreadsheetString (false, preferences.CSVExportDecimalSeparator);
		label_persons_export_csv_delim.UseMarkup = true;
		buttonbox_person_export_feedback.Visible = false;
	}

	private void on_button_persons_export_clicked (object o, EventArgs args)
	{
		if (radio_export_persons_current_session.Active)
			checkFile (Constants.CheckFileOp.PERSONS_EXPORT_THIS_SESSION);
		else
			checkFile (Constants.CheckFileOp.PERSONS_EXPORT_ANY_SESSION);
	}

	private void on_overwrite_file_persons_export_this_session_accepted (object o, EventArgs args)
	{
		on_persons_export_this_session_selected (exportFileName);
	}
	private void on_overwrite_file_persons_export_all_sessions_accepted (object o, EventArgs args)
	{
		on_persons_export_all_sessions_selected (exportFileName);
	}

	private void on_persons_export_this_session_selected (string destination)
	{
		persons_export_do (currentSession.UniqueID, destination);
	}
	private void on_persons_export_all_sessions_selected (string destination)
	{
		persons_export_do (-1, destination);
	}

	private void persons_export_do (int sessionID, string destination)
	{
		persons_export_feedback_message.Text = "";
		persons_export_feedback_ok_url.Text = "";
		persons_export_feedback_ok_url.TooltipText = "";
		buttonbox_person_export_feedback.Visible = false;

		personsExport = new PersonsExport (sessionID, destination, preferences.CSVColumnDelimiter);
		bool success = personsExport.Do ();

		// labels
		persons_export_feedback_message.Text = personsExport.DoneMessage ();
		string url = "";
		if (success)
			url = personsExport.DoneOkURL;

		persons_export_feedback_ok_url.Text = url;
		persons_export_feedback_ok_url.TooltipText = url;
		buttonbox_person_export_feedback.Visible = success;
	}

	private void on_button_persons_export_feedback_open_file_clicked (object o, EventArgs args)
	{
		if (personsExport == null || personsExport.DoneOkURL == "")
			return;

		if (! Util.OpenURL (personsExport.DoneOkURL))
			new DialogMessage (Constants.MessageTypes.WARNING,
					Constants.DirectoryCannotOpenStr() + "\n\n" + personsExport.DoneOkURL);
	}

	private void on_button_persons_export_feedback_open_folder_clicked (object o, EventArgs args)
	{
		if (personsExport == null || personsExport.DoneOkURL == "" || Path.GetDirectoryName (personsExport.DoneOkURL) == "")
			return;

		string path = Path.GetDirectoryName (personsExport.DoneOkURL);
		if (! Util.OpenURL (path))
			new DialogMessage (Constants.MessageTypes.WARNING,
					Constants.DirectoryCannotOpenStr() + "\n\n" + path);
	}

	private void connectWidgetsPersonsExport (Gtk.Builder builder)
	{
		radio_export_persons_current_session = (Gtk.RadioButton) builder.GetObject ("radio_export_persons_current_session");
		label_persons_export_destination = (Gtk.Label) builder.GetObject ("label_persons_export_destination");
		box_persons_export_feedback = (Gtk.Box) builder.GetObject ("box_persons_export_feedback");
		persons_export_feedback_message = (Gtk.Label) builder.GetObject ("persons_export_feedback_message");
		persons_export_feedback_ok_url = (Gtk.Label) builder.GetObject ("persons_export_feedback_ok_url");
		buttonbox_person_export_feedback = (Gtk.ButtonBox) builder.GetObject ("buttonbox_person_export_feedback");
		label_persons_export_csv_delim = (Gtk.Label) builder.GetObject ("label_persons_export_csv_delim");
	}
}
