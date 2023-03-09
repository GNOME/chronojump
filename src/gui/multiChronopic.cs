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
 * Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
//using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList

using System.Threading;
using Mono.Unix;



public partial class ChronoJumpWindow 
{
	Gtk.RadioButton extra_window_radio_multichronopic_start;
	Gtk.RadioButton extra_window_radio_multichronopic_run_analysis;

	Gtk.Label extra_window_label_multichronopic_need_two;
	Gtk.Button button_run_analysis_help;

	Gtk.CheckButton extra_window_check_multichronopic_sync;
	Gtk.CheckButton extra_window_check_multichronopic_delete_first;

	//run analysis
	Gtk.HBox extra_window_hbox_run_analysis_total_distance;
	Gtk.SpinButton extra_window_spin_run_analysis_distance;
	
	int extra_window_multichronopic_distance = 1000; //1000cm: 10m

	private void on_extra_window_multichronopic_test_changed(object o, EventArgs args)
	{
		if(extra_window_radio_multichronopic_start.Active) 
			currentMultiChronopicType = new MultiChronopicType(Constants.MultiChronopicName);
		else if(extra_window_radio_multichronopic_run_analysis.Active) 
			currentMultiChronopicType = new MultiChronopicType(Constants.RunAnalysisName);

		sensitiveLastTestButtons(false);

		/*
		 *	disabled on 1.6.3
		if(chronopicWin.NumConnected() < 2) 
			extra_window_multichronopic_can_do(false);
		else
			extra_window_multichronopic_can_do(true);
			*/
		
		extra_window_multichronopic_initialize(currentMultiChronopicType);
	}

	private void extra_window_multichronopic_initialize(MultiChronopicType myMultiChronopicType) 
	{
		currentEventType = myMultiChronopicType;
		changeTestImage(EventType.Types.MULTICHRONOPIC.ToString(), 
				myMultiChronopicType.Name, myMultiChronopicType.ImageFileName);

		extra_window_spin_run_analysis_distance.Value = extra_window_multichronopic_distance;
		bool showSyncAndDeleteFirst = false;
		bool showRunDistance = false;
		if(myMultiChronopicType.Name == Constants.MultiChronopicName) {
			showSyncAndDeleteFirst = true;
			setLabelContactsExerciseSelected(Catalog.GetString("Multi Chronopic"));
		} else if(myMultiChronopicType.Name == Constants.RunAnalysisName) {
			showRunDistance = true;
			setLabelContactsExerciseSelected(Catalog.GetString("Race analysis"));
		}
		extra_window_multichronopic_showSyncAndDeleteFirst(showSyncAndDeleteFirst);
		extra_window_multichronopic_showRunDistance(showRunDistance);
	}


	private void extra_window_multichronopic_can_do(bool can_do) {
		//if there are no persons, cannot show this stuff
		if( ! myTreeViewPersons.IsThereAnyRecord() )
			can_do = false;

		button_execute_test.Sensitive = can_do;
		extra_window_label_multichronopic_need_two.Visible = ! can_do;
		
		extra_window_check_multichronopic_sync.Sensitive = can_do;
		extra_window_check_multichronopic_delete_first.Sensitive = can_do;
		extra_window_hbox_run_analysis_total_distance.Sensitive = can_do;
	}

	private void extra_window_multichronopic_showSyncAndDeleteFirst(bool show) {
		extra_window_check_multichronopic_sync.Visible = show;
		extra_window_check_multichronopic_delete_first.Visible = show;
	}
	private void extra_window_multichronopic_showRunDistance(bool show) {
		extra_window_hbox_run_analysis_total_distance.Visible = show;
		button_run_analysis_help.Visible = show;
	}

	private void on_button_run_analysis_help_clicked (object o, EventArgs args) {
		new DialogMessage(Constants.MessageTypes.INFO, 
				Catalog.GetString("First Chronopic should be connected to photocells.\nSecond Chronopic to platforms.") + "\n");
	}


	private void connectWidgetsMultiChronopic (Gtk.Builder builder)
	{
		extra_window_radio_multichronopic_start = (Gtk.RadioButton) builder.GetObject ("extra_window_radio_multichronopic_start");
		extra_window_radio_multichronopic_run_analysis = (Gtk.RadioButton) builder.GetObject ("extra_window_radio_multichronopic_run_analysis");

		extra_window_label_multichronopic_need_two = (Gtk.Label) builder.GetObject ("extra_window_label_multichronopic_need_two");
		button_run_analysis_help = (Gtk.Button) builder.GetObject ("button_run_analysis_help");

		extra_window_check_multichronopic_sync = (Gtk.CheckButton) builder.GetObject ("extra_window_check_multichronopic_sync");
		extra_window_check_multichronopic_delete_first = (Gtk.CheckButton) builder.GetObject ("extra_window_check_multichronopic_delete_first");

		//run analysis
		extra_window_hbox_run_analysis_total_distance = (Gtk.HBox) builder.GetObject ("extra_window_hbox_run_analysis_total_distance");
		extra_window_spin_run_analysis_distance = (Gtk.SpinButton) builder.GetObject ("extra_window_spin_run_analysis_distance");
	}
}

//--------------------------------------------------------
//---------------- EDIT MULTI CHRONOPIC WIDGET -----------
//--------------------------------------------------------

public class EditMultiChronopicWindow : EditEventWindow
{
	static EditMultiChronopicWindow EditMultiChronopicWindowBox;

	EditMultiChronopicWindow (Gtk.Window parent)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "edit_event.glade", "edit_event", "chronojump");
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "edit_event.glade", null);
		connectWidgetsEditEvent (builder);
		builder.Autoconnect (this);

		this.parent = parent;
		
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
		typeOfTest = Constants.TestTypes.MULTICHRONOPIC;
		headerShowDecimal = false;
		showType = false;
		showRunStart = false;
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




