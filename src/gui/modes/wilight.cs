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
//using Glade;
//using System.Text; //StringBuilder
using Mono.Unix;

//--------------------------------------------------------
//---------------- EDIT RUN WIDGET -----------------------
//--------------------------------------------------------

public class EditWilightWindow : EditEventWindow
{
	static EditWilightWindow EditWilightWindowBox;

	//for inheritance
	protected EditWilightWindow () {
	}

	public EditWilightWindow (Gtk.Window parent)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "edit_event.glade", "edit_event", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "edit_event.glade", null);
		connectWidgetsEditEvent (builder);
		builder.Autoconnect (this);

		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(edit_event);
	
//TODO:
//		eventBigTypeString = Catalog.GetString("race");
	}

	static public EditWilightWindow Show (Gtk.Window parent, Event myEvent)
	{
		if (EditWilightWindowBox == null) {
			EditWilightWindowBox = new EditWilightWindow (parent);
		}

		EditWilightWindowBox.colorize();
		EditWilightWindowBox.initializeValues();
		EditWilightWindowBox.fillDialog (myEvent);
		EditWilightWindowBox.edit_event.Show ();

		return EditWilightWindowBox;
	}
	
	protected override void initializeValues ()
	{
		typeOfTest = Constants.TestTypes.WILIGHT;
		showType = false; //TODO: in the future change this
		showRunStart = false;
		showTv = false;
		showTc= false;
		showFall = false;
		showDistance = false;
		distanceCanBeDecimal = true;
		showTime = false;
		showSpeed = false;
		showWeight = false;
		showLimited = false;
		showMistakes = false;
		showVideo = false;
		showDescription = false;
	}

	protected override string [] findTypes (Event myEvent)
	{
		//TODO
		return new string []{};
	}
	
	private void on_combo_eventType_changed (object o, EventArgs args)
	{
		//TODO:
	}

	protected override void updateEvent (int eventID, int personID, string description)
	{
		SqliteWilight.Update (eventID,
				//UtilGtk.ComboGetActive(combo_eventType),
				personID);
	}

	protected override void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditWilightWindowBox.edit_event.Hide();
		EditWilightWindowBox = null;
	}
	
	protected override void on_delete_event (object o, DeleteEventArgs args)
	{
		EditWilightWindowBox.edit_event.Hide();
		EditWilightWindowBox = null;
	}
	
	protected override void hideWindow() {
		EditWilightWindowBox.edit_event.Hide();
		EditWilightWindowBox = null;
	}
}
	

