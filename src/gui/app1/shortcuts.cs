/*
 * This file is part of ChronoJump
 *
 * Chronojump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * Chronojump is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 *    GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * Copyright (C) 2020-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
//using Glade;
using Mono.Unix;

public partial class ChronoJumpWindow
{
	//Gtk.Button button_edit_current_person;
	Gtk.Button button_edit_current_person_h;
	Gtk.Button button_encoder_exercise;
	Gtk.Button button_encoder_exercise_close_and_capture;

	private string kCtrl = "Ctrl";
	private string kEnter = "Enter";

	private void addShortcutsToTooltips(bool isMac)
	{
		/*
		 * on 2.0 mac will also use ctrl until we find the way to use command
		 *
		if(isMac) {
			kCtrl = "Command";
			kEnter = "Command";
		}
		*/

		string space = Catalog.GetString("Space");
		string up = Catalog.GetString("Up");
		string down = Catalog.GetString("Down");

		//persons
		button_edit_current_person_h.TooltipText += string.Format(" ({0}+p)", kCtrl);
		button_persons_up.TooltipText += string.Format(" ({0}+{1})", kCtrl, up);
		button_persons_down.TooltipText += string.Format(" ({0}+{1})", kCtrl, down);

		//contacts
		button_contacts_exercise.TooltipText += string.Format( "({0}+t)", kCtrl);
		button_execute_test.TooltipText += string.Format(" ({0}+{1})", kCtrl, space);
		button_contacts_exercise_close_and_capture.TooltipText += string.Format("({0}+{1})", kCtrl, space);
		button_contacts_exercise_close_and_recalculate.TooltipText += string.Format(" ({0}+r)", kCtrl);
		event_execute_button_finish.TooltipText += string.Format(" ({0})", kEnter);
		event_execute_button_cancel.TooltipText += " (Escape)";
		button_contacts_delete_selected.TooltipText += string.Format(" ({0}+d)", kCtrl);
		button_contacts_capture_session_overview.TooltipText += string.Format(" ({0}+o)", kCtrl);
		button_contacts_capture_load.TooltipText += string.Format(" ({0}+l)", kCtrl);
		//button_signal_analyze_load_ab.TooltipText += string.Format(" ({0}+l)", kCtrl);
		//button_signal_analyze_load_cd.TooltipText += string.Format(" ({0}+l)", kCtrl);
		button_video_play_this_test_contacts.TooltipText += string.Format(" ({0}+v)", kCtrl);

		//encoder
		button_encoder_exercise.TooltipText += string.Format( "({0}+t)", kCtrl);
		button_encoder_capture.TooltipText += string.Format(" ({0}+{1})", kCtrl, space);
		button_encoder_exercise_close_and_capture.TooltipText += string.Format("({0}+{1})", kCtrl, space);
		button_encoder_exercise_close_and_recalculate.TooltipText += string.Format(" ({0}+r)", kCtrl);
		button_encoder_capture_finish.TooltipText += string.Format(" ({0})", kEnter);
		button_encoder_capture_cancel.TooltipText += " (Escape)";
		button_encoder_capture_session_overview.TooltipText += string.Format(" ({0}+o)", kCtrl);
		button_encoder_load_signal.TooltipText += string.Format(" ({0}+l)", kCtrl);
		button_encoder_load_signal_at_analyze.TooltipText += string.Format(" ({0}+l)", kCtrl);
		button_video_play_this_test_encoder.TooltipText += string.Format(" ({0}+v)", kCtrl);
	}

	private void connectWidgetsShortcuts (Gtk.Builder builder)
	{
		//button_edit_current_person = (Gtk.Button) builder.GetObject ("button_edit_current_person");
		button_edit_current_person_h = (Gtk.Button) builder.GetObject ("button_edit_current_person_h");
		button_encoder_exercise = (Gtk.Button) builder.GetObject ("button_encoder_exercise");
		button_encoder_exercise_close_and_capture = (Gtk.Button) builder.GetObject ("button_encoder_exercise_close_and_capture");
	}
}
