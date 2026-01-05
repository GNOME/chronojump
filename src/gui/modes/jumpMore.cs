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
 * Copyright (C) 2004-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
//using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList

using System.Threading;
using Mono.Unix;


public class JumpsMoreWindow : EventMoreWindow
{
	Gtk.Window jumps_runs_more;
	static JumpsMoreWindow JumpsMoreWindowBox;
	private bool selectedStartIn;
	private bool selectedExtraWeight;

	public JumpsMoreWindow (Gtk.Window parent, bool testOrDelete)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "jumps_runs_more.glade", "jumps_runs_more", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "jumps_runs_more.glade", null);
		connectWidgetsEventMore (builder);
		jumps_runs_more = (Gtk.Window) builder.GetObject ("jumps_runs_more");
		builder.Autoconnect (this);

		this.parent = parent;
		this.testOrDelete = testOrDelete;
		
		if(!testOrDelete)
			jumps_runs_more.Title = Catalog.GetString("Delete test type defined by user");
		
		//put an icon to window
		UtilGtk.IconWindow(jumps_runs_more);

		//manage window color
		if(! Config.UseSystemColor)
			UtilGtk.WindowColor(jumps_runs_more, Config.ColorBackground);

		selectedEventType = EventType.Types.JUMP.ToString();
		
		//name, startIn, weight, description
		store = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string));

		initializeThings();
	}
	
	static public JumpsMoreWindow Show (Gtk.Window parent, bool testOrDelete)
	{
		if (JumpsMoreWindowBox == null) {
			JumpsMoreWindowBox = new JumpsMoreWindow (parent, testOrDelete);
		}
		JumpsMoreWindowBox.jumps_runs_more.Show ();
		
		return JumpsMoreWindowBox;
	}
	
	protected override void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;
		
		tv.AppendColumn ( Catalog.GetString ("Name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Start inside"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Extra weight"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Description"), new CellRendererText(), "text", count++);
	}
	
	protected override void fillTreeView (Gtk.TreeView tv, TreeStore store) 
	{
		//select data without inserting an "all jumps", without filter, and not obtain only name of jump
		string [] myJumpTypes = SqliteJumpType.SelectJumpTypes(false, "", "", false);

		//remove typesTranslated
		typesTranslated = new String [myJumpTypes.Length];
		int count = 0;

		foreach (string myType in myJumpTypes) {
			string [] myStringFull = myType.Split(new char[] {':'});
			if(myStringFull[2] == "1") {
				myStringFull[2] = Catalog.GetString("Yes");
			} else {
				myStringFull[2] = Catalog.GetString("No");
			}
			if(myStringFull[3] == "1") {
				myStringFull[3] = Catalog.GetString("Yes");
			} else {
				myStringFull[3] = Catalog.GetString("No");
			}

			JumpType tempType = new JumpType (myStringFull[1]);
			string description  = getDescriptionLocalised(tempType, myStringFull[4]);

			//if we are going to execute: show all types
			//if we are going to delete: show user defined types
			if(testOrDelete || ! tempType.IsPredefined)
				store.AppendValues (
						//myStringFull[0], //don't display de uniqueID
						Catalog.GetString(myStringFull[1]),	//name (translated)
						myStringFull[2], 	//startIn
						myStringFull[3], 	//weight
						description
						);

			//create typesTranslated
			typesTranslated [count++] = myStringFull[1] + ":" + Catalog.GetString(myStringFull[1]);
		}	
	}

	protected override void onSelectionEntry (object o, EventArgs args) {
		ITreeModel model;
		TreeIter iter;
		selectedEventName = "-1";
		selectedStartIn = false;
		selectedExtraWeight = false;
		selectedDescription = "";

		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			string translatedName = (string) model.GetValue (iter, 0);
			//get name in english
			selectedEventName = Util.FindOnArray(':', 1, 0, translatedName, typesTranslated);
			
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Yes") ) {
				selectedStartIn = true;
			}
			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Yes") ) {
				selectedExtraWeight = true;
			}
			selectedDescription = (string) model.GetValue (iter, 3);

			if(testOrDelete) {
				button_accept.Sensitive = true;
				//update graph image test on main window
				button_selected.Click();
			} else
				button_delete_type.Sensitive = true;
		}
	}
	
	protected override void on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
		//return if we are to delete a test
		if(!testOrDelete)
			return;

		TreeView tv = (TreeView) o;
		ITreeModel model;
		TreeIter iter;

		if (tv.Selection.GetSelected (out model, out iter)) {
			string translatedName = (string) model.GetValue (iter, 0);
			//get name in english
			selectedEventName = Util.FindOnArray(':', 1, 0, translatedName, typesTranslated);
			
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Yes") ) {
				selectedStartIn = true;
			}
			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Yes") ) {
				selectedExtraWeight = true;
			}
			selectedDescription = (string) model.GetValue (iter, 3);

			//activate on_button_accept_clicked()
			button_accept.Activate();
		}
	}
	
	protected override void deleteTestLine() {
		SqliteJumpType.Delete(Constants.JumpTypeTable, selectedEventName, false);
		
		//delete from typesTranslated
		string row = Util.FindOnArray(':',0, -1, selectedEventName, typesTranslated);
		LogB.Information("row " + row);
		typesTranslated = Util.DeleteString(typesTranslated, row);
	}

	protected override string [] findTestTypesInSessions() {
		return SqliteJump.SelectJumpsSA (false, -1, -1, "", selectedEventName,
				Sqlite.Orders_by.DEFAULT, 0);
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		JumpsMoreWindowBox.jumps_runs_more.Hide();
		JumpsMoreWindowBox = null;
	}
	
	void on_jumps_runs_more_delete_event (object o, DeleteEventArgs args)
	{
		//raise signal
		button_cancel.Click();

		//JumpsMoreWindowBox.jumps_runs_more.Hide();
		//JumpsMoreWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		JumpsMoreWindowBox.jumps_runs_more.Hide();
	}
	
	//when a jump is done using jumpsMoreWindow, the accept doesn't destroy this instance, because 
	//later we need data from it.
	//This is used for destroying, then if a new jump type is added, it will be shown at first time clicking "more" button
	public void Destroy() {		
		JumpsMoreWindowBox = null;
	}
	
	public bool SelectedStartIn 
	{
		get {
			return selectedStartIn;
		}
	}
	
	public bool SelectedExtraWeight 
	{
		get {
			return selectedExtraWeight;
		}
	}
}
