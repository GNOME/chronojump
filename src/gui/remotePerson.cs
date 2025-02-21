/*
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 *
 * Copyright (C) 2025  Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Threading;
using Gtk;

public partial class ChronoJumpWindow
{
	// at glade ---->
	//remote test next person
	Gtk.Box box_remote_person_next;
	Gtk.Button button_remote_person_next1;
	Gtk.Button button_remote_person_next2;
	Gtk.Button button_remote_person_next3;
	Gtk.Button button_remote_person_next4;
	Gtk.Button button_remote_person_next5;
	// <---- at glade

	//called on session changed or person changed
	//TODO: check as this is called two times
	private void remotePersonReadAndAssign ()
	{
		if (currentSession == null || currentPerson == null)
			return;

		remotePersonNext.ReadFile ();

		if (currentSession != null)
			remotePersonNext.AssignButtonsToPersonsNotInSession (currentSession.UniqueID);

		remotePersonNext.FakeButtonAdd.Clicked -= new EventHandler (on_remote_person_next_add);
		remotePersonNext.FakeButtonAdd.Clicked += new EventHandler (on_remote_person_next_add);
	}

	private void on_remote_person_next_add (object o, EventArgs args)
	{
		LogB.Information (string.Format ("Going to add person: {0}", remotePersonNext.ImportPerson));

		// TODO: take care if the DB is open, buttons should be unsensitive while capture
		currentPerson = new Person (
				remotePersonNext.ImportPerson, Constants.SexU, DateTime.MinValue,
				Constants.RaceUndefinedID, Constants.CountryUndefinedID, "",
				"", "", Constants.ServerUndefinedID, "", false);

		currentPersonSession = new PersonSession (
				currentPerson.UniqueID, currentSession.UniqueID,
				0, 0, //width, height
				Constants.SportUndefinedID,
				Constants.SpeciallityUndefinedID,
				Constants.LevelUndefinedID,
				"",     //comments
				Constants.TrochanterToeUndefinedID,
				Constants.TrochanterFloorOnFlexionUndefinedID,
				false); //dbconOpened

		label_person_change();
		personChanged();

		treeview_persons_storeReset();
		fillTreeView_persons();
		int rowToSelect = myTreeViewPersons.FindRow (currentPerson.UniqueID);
		if(rowToSelect != -1) {
			selectRowTreeView_persons (treeview_persons, rowToSelect);
			sensitiveGuiYesPerson ();
		}

		// remove the person from the buttons
		remotePersonNext.AssignButtonsToPersonsNotInSession (currentSession.UniqueID);
	}

	private void connectWidgetsRemotePerson (Gtk.Builder builder)
	{
		box_remote_person_next = (Gtk.Box) builder.GetObject ("box_remote_person_next");
		button_remote_person_next1 = (Gtk.Button) builder.GetObject ("button_remote_person_next1");
		button_remote_person_next2 = (Gtk.Button) builder.GetObject ("button_remote_person_next2");
		button_remote_person_next3 = (Gtk.Button) builder.GetObject ("button_remote_person_next3");
		button_remote_person_next4 = (Gtk.Button) builder.GetObject ("button_remote_person_next4");
		button_remote_person_next5 = (Gtk.Button) builder.GetObject ("button_remote_person_next5");
	}
}
