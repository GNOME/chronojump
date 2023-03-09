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
 * Copyright (C) 2019-2023   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
//using Glade;

public partial class ChronoJumpWindow 
{
	Gtk.Frame frame_exhibition;
	Gtk.SpinButton spin_exhibition_school;
	Gtk.SpinButton spin_exhibition_group;
	Gtk.SpinButton spin_exhibition_id;

	private void exhibitionGuiAtStart(ExhibitionTest.testTypes exhibitionStationType)
	{
		if(exhibitionStationType == ExhibitionTest.testTypes.JUMP)
			on_button_selector_start_jumps_simple_clicked (new object (), new EventArgs());
		else if(exhibitionStationType == ExhibitionTest.testTypes.RUN)
			on_button_selector_start_runs_simple_clicked (new object (), new EventArgs());
		else if(exhibitionStationType == ExhibitionTest.testTypes.INERTIAL)
			on_button_selector_start_encoder_inertial_clicked (new object (), new EventArgs());
		else if(exhibitionStationType == ExhibitionTest.testTypes.FORCE_ROPE ||
				exhibitionStationType == ExhibitionTest.testTypes.FORCE_SHOT)
			on_button_selector_start_force_sensor_isometric_clicked (new object (), new EventArgs());

		frame_exhibition.Visible = true;
		frame_persons.Sensitive = true;
		frame_persons_top.Visible = false;
		spin_exhibition_school.Value = 0; //need to assign an inital value (if not it shows blank value)
		spin_exhibition_group.Value = 0;

		//button_persons_up.SetSizeRequest (45,10);
		//button_persons_down.SetSizeRequest (45,10);
	}

	//---- spin_exhibition_school stuff

	private void on_button_exhibition_school_left10_clicked (object o, EventArgs args)
	{
		exhibitionSchoolChange(-10);
	}
	private void on_button_exhibition_school_left_clicked (object o, EventArgs args)
	{
		exhibitionSchoolChange(-1);
	}
	private void on_button_exhibition_school_right10_clicked (object o, EventArgs args)
	{
		exhibitionSchoolChange(+10);
	}
	private void on_button_exhibition_school_right_clicked (object o, EventArgs args)
	{
		exhibitionSchoolChange(+1);
	}

	void exhibitionSchoolChange(int change)
	{
		double newValue = spin_exhibition_school.Value + change;

		double min, max;
		spin_exhibition_school.GetRange(out min, out max);
		if(newValue < min)
			spin_exhibition_school.Value = min;
		else if(newValue > max)
			spin_exhibition_school.Value = max;
		else
			spin_exhibition_school.Value = newValue;
	}

	//---- end of spin_exhibition_school stuff

	//---- spin_exhibition_group stuff

	private void on_button_exhibition_group_left10_clicked (object o, EventArgs args)
	{
		exhibitionGroupChange(-10);
	}
	private void on_button_exhibition_group_left_clicked (object o, EventArgs args)
	{
		exhibitionGroupChange(-1);
	}
	private void on_button_exhibition_group_right10_clicked (object o, EventArgs args)
	{
		exhibitionGroupChange(+10);
	}
	private void on_button_exhibition_group_right_clicked (object o, EventArgs args)
	{
		exhibitionGroupChange(+1);
	}

	void exhibitionGroupChange(int change)
	{
		double newValue = spin_exhibition_group.Value + change;

		double min, max;
		spin_exhibition_group.GetRange(out min, out max);
		if(newValue < min)
			spin_exhibition_group.Value = min;
		else if(newValue > max)
			spin_exhibition_group.Value = max;
		else
			spin_exhibition_group.Value = newValue;
	}

	//---- end of spin_exhibition_group stuff

	//---- spin_exhibition_id stuff

	private void on_button_exhibition_id_left10_clicked (object o, EventArgs args)
	{
		exhibitionIdChange(-10);
	}
	private void on_button_exhibition_id_left_clicked (object o, EventArgs args)
	{
		exhibitionIdChange(-1);
	}
	private void on_button_exhibition_id_right10_clicked (object o, EventArgs args)
	{
		exhibitionIdChange(+10);
	}
	private void on_button_exhibition_id_right_clicked (object o, EventArgs args)
	{
		exhibitionIdChange(+1);
	}

	void exhibitionIdChange(int change)
	{
		double newValue = spin_exhibition_id.Value + change;

		double min, max;
		spin_exhibition_id.GetRange(out min, out max);
		if(newValue < min)
			spin_exhibition_id.Value = min;
		else if(newValue > max)
			spin_exhibition_id.Value = max;
		else
			spin_exhibition_id.Value = newValue;
	}

	//---- end of spin_exhibition_group stuff
	private void on_button_exhibition_select_clicked (object o, EventArgs args)
	{
		//select session
		string newSessionName = string.Format("{0}-{1}", spin_exhibition_school.Value, spin_exhibition_group.Value);
		if(currentSession == null || currentSession.Name != newSessionName)
		{
			currentSession = SqliteSession.SelectByName(newSessionName);
			on_load_session_accepted();
			sensitiveGuiYesSession();
		}

		//select person
		int rowToSelect = myTreeViewPersons.FindRow(Convert.ToInt32(spin_exhibition_id.Value));
		if(rowToSelect != -1) {
			selectRowTreeView_persons(treeview_persons, rowToSelect);
			sensitiveGuiYesPerson();
		}
	}

	//read from the widgets
	//read testTypes and result
	private ExhibitionTest getExhibitionTestFromGui(ExhibitionTest.testTypes tt, double result)
	{
		return new ExhibitionTest(Convert.ToInt32(spin_exhibition_school.Value),
				Convert.ToInt32(spin_exhibition_group.Value),
				currentPerson.UniqueID, tt, result);
	}


	private void connectWidgetsExhibition (Gtk.Builder builder)
	{
		frame_exhibition = (Gtk.Frame) builder.GetObject ("frame_exhibition");
		spin_exhibition_school = (Gtk.SpinButton) builder.GetObject ("spin_exhibition_school");
		spin_exhibition_group = (Gtk.SpinButton) builder.GetObject ("spin_exhibition_group");
		spin_exhibition_id = (Gtk.SpinButton) builder.GetObject ("spin_exhibition_id");
	}
}
