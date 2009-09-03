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
 * Copyright (C) 2004-2009   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList

using System.Threading;
using Mono.Unix;



//--------------------------------------------------------
//---------------- EDIT MULTI CHRONOPIC WIDGET -----------
//--------------------------------------------------------

public class EditMultiChronopicWindow : EditEventWindow
{
	static EditMultiChronopicWindow EditMultiChronopicWindowBox;

	EditMultiChronopicWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "edit_event", null);
		gladeXML.Autoconnect(this);
		this.parent 	= parent;
		
		//put an icon to window
		UtilGtk.IconWindow(edit_event);
	
		eventBigTypeString = Catalog.GetString("multi chronopic");
		headerShowDecimal = false;
	}

	static new public EditMultiChronopicWindow Show (Gtk.Window parent, Event myEvent, int pDN)
	{
		if (EditMultiChronopicWindowBox == null) {
			EditMultiChronopicWindowBox = new EditMultiChronopicWindow (parent);
		}

		EditMultiChronopicWindowBox.pDN = pDN;
		
		EditMultiChronopicWindowBox.initializeValues();
		
		if(myEvent.Type == Constants.RunAnalysisName)
			EditMultiChronopicWindowBox.showDistance = true;
		else {
			EditMultiChronopicWindowBox.showDistance = false;
			EditMultiChronopicWindowBox.entryDistance = ""; //instead of the "0" in gui/event.cs
		}

		EditMultiChronopicWindowBox.fillDialog (myEvent);

		EditMultiChronopicWindowBox.edit_event.Show ();

		return EditMultiChronopicWindowBox;
	}
	
	protected override void initializeValues () {
		headerShowDecimal = false;
		showType = false;
		showTv = false;
		showTc= false;
		showFall = false;

		//showDistance (defined above depending on myEvent)
		distanceCanBeDecimal = false;

		showTime = false;
		showSpeed = false;
		showWeight = false;
		showLimited = false;
		showMistakes = false;
	}

	protected override void fillDistance(Event myEvent) {
		MultiChronopic mc = (MultiChronopic) myEvent;
		entryDistance = mc.Vars.ToString(); //distance
		entry_distance_value.Text = Util.TrimDecimals(entryDistance, pDN);
		entry_distance_value.Sensitive = true;
	}
	
	protected override void updateEvent(int eventID, int personID, string description) {

		SqliteMultiChronopic.Update(eventID, personID, entryDistance, description);
	}

	protected override void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditMultiChronopicWindowBox.edit_event.Hide();
		EditMultiChronopicWindowBox = null;
	}
	
	protected override void on_delete_event (object o, DeleteEventArgs args)
	{
		EditMultiChronopicWindowBox.edit_event.Hide();
		EditMultiChronopicWindowBox = null;
	}
	
	protected override void hideWindow() {
		EditMultiChronopicWindowBox.edit_event.Hide();
		EditMultiChronopicWindowBox = null;
	}

}




