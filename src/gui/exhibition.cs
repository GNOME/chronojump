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
 * Copyright (C) 2019   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Glade;

public partial class ChronoJumpWindow 
{
	[Widget] Gtk.Frame frame_exhibition;
	[Widget] Gtk.SpinButton spin_exhibition_school;
	[Widget] Gtk.SpinButton spin_exhibition_group;
	[Widget] Gtk.SpinButton spin_exhibition_id;

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
			on_button_selector_start_force_sensor_clicked (new object (), new EventArgs());

		menuitem_open_session.Sensitive = false; //do not allow menu open to work (it could be really slow)
		frame_exhibition.Visible = true;
		notebook_session_person.CurrentPage = 1;
		frame_persons.Sensitive = true;
		frame_persons_top.Visible = false;
		spin_exhibition_school.Value = 0; //need to assign an inital value (if not it shows blank value)
		spin_exhibition_group.Value = 0;

		button_persons_up.SetSizeRequest (45,10);
		button_persons_down.SetSizeRequest (45,10);
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

	//---- json upload

	private void uploadExhibitionTest(ExhibitionTest.testTypes tt, double result)
	{
		Json js = new Json();
		ExhibitionTest et = new ExhibitionTest(Convert.ToInt32(spin_exhibition_school.Value),
				Convert.ToInt32(spin_exhibition_group.Value),
				currentPerson.UniqueID, tt, result);

		if( ! js.UploadExhibitionTest (et))
		{
			LogB.Error(js.ResultMessage);
			SqliteJson.InsertTempExhibitionTest(false, et); //insert only if could'nt be uploaded
		}
	}

}
