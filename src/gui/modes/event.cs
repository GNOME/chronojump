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
using Gdk;
using Gtk;
//using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Collections.Generic; //List<>
using System.IO;
using System.Threading;
using Mono.Unix;


//--------------------------------------------------------
//---------------- event_more widget ---------------------
//--------------------------------------------------------

public class EventMoreWindow 
{
	protected Gtk.Notebook notebook;
	protected Gtk.TreeView treeview_more;
	protected Gtk.Button button_accept;
	protected Gtk.Button button_delete_type;
	protected Gtk.Button button_cancel;
	protected Gtk.Button button_close;
	protected Gtk.Button button_close1;
	protected Gtk.Label label_delete_confirm;
	protected Gtk.Label label_delete_confirm_name;
	protected Gtk.Label label_delete_cannot;
	protected Gtk.Image image_delete;
	protected Gtk.Image image_delete1;

	protected Gtk.Window parent;

	protected enum notebookPages { TESTS, DELETECONFIRM, DELETECANNOT };

	protected TreeStore store;

	protected string selectedEventType;
	protected string selectedEventName;
	protected string selectedDescription;
	public Gtk.Button button_selected;
	public Gtk.Button button_deleted_test; //just to send a signal
	
	protected bool testOrDelete; //are we going to do a test or to delete a test type (test is true)
	protected string [] typesTranslated;

	public EventMoreWindow () {
	}

	public EventMoreWindow (Gtk.Window parent, bool testOrDelete)
	{
		//name, startIn, weight, description
		store = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string));

		initializeThings();
	}

	protected void initializeThings() 
	{
		button_selected = new Gtk.Button();
		button_deleted_test = new Gtk.Button();
		
		createTreeView(treeview_more);

		treeview_more.Model = store;
		fillTreeView(treeview_more,store);

		//when executing test: show accept and cancel
		button_accept.Visible = testOrDelete;
		button_cancel.Visible = testOrDelete;
		//when deleting test type: show delete type and close
		button_delete_type.Visible = ! testOrDelete;
		button_close.Visible = ! testOrDelete;

		Pixbuf pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "stock_delete.png");
		image_delete.Pixbuf = pixbuf;
		image_delete1.Pixbuf = pixbuf;

		button_accept.Sensitive = false;
		button_delete_type.Sensitive = false;
		 
		treeview_more.Selection.Changed += onSelectionEntry;
	}


	//if eventType is predefined, it will have a translation on src/evenType or derivated class
	//this is useful if user changed language
	protected string getDescriptionLocalised(EventType myType, string descriptionFromDb) {
	if(myType.IsPredefined)
		return myType.Description;
	else
		return descriptionFromDb;
	}


	protected virtual void createTreeView (Gtk.TreeView tv) {
	}
	
	protected virtual void fillTreeView (Gtk.TreeView tv, TreeStore store) 
	{
	}

	/*
	 * when a row is selected...
	 * -put selected value in selected* variables
	 * -update graph image test on main window
	 */
	protected virtual void onSelectionEntry (object o, EventArgs args)
	{
	}
	
	protected virtual void on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
	}
	
	void on_button_delete_type_clicked (object o, EventArgs args)
	{
		List<Session> session_l = SqliteSession.SelectAll(false, Sqlite.Orders_by.DEFAULT);
		string [] tests = findTestTypesInSessions();

		//this will be much better doing a select distinct(session) instead of using SelectJumps or Runs
		ArrayList sessionValuesArray = new ArrayList();
		foreach(string t in tests)
		{
			string [] tFull = t.Split(new char[] {':'});
			if(! Util.IsNumber(tFull[3], false))
				continue;

			int sessionID = Convert.ToInt32(tFull[3]);
			foreach(Session s in session_l)
				if(s.UniqueID == sessionID)
					Util.AddToArrayListIfNotExist(sessionValuesArray,
							string.Format("  ({0}) {1}", s.DateShort, s.Name));
		}

		//if exist tell user to edit or delete them
		if(tests.Length > 0)
		{
			notebook.Page = Convert.ToInt32(notebookPages.DELETECANNOT);
			label_delete_cannot.Text = Catalog.GetString("There are tests of that type on database on sessions:") + "\n\n" +
					Util.ArrayListToSingleString(sessionValuesArray, "\n") + "\n\n" +
					Catalog.GetString("please first edit or delete them.");
		} else {
			notebook.Page = Convert.ToInt32(notebookPages.DELETECONFIRM);
			label_delete_confirm.Text = Catalog.GetString("Are you sure you want to delete this test type?");
			label_delete_confirm_name.Text = "<b>" + selectedEventName + "</b>";
			label_delete_confirm_name.UseMarkup = true;
		}
	}
	

	protected virtual void deleteTestLine() {
	}
	
	protected void on_button_delete_confirm_cancel_clicked (object o, EventArgs args)
	{
		notebook.Page = Convert.ToInt32(notebookPages.TESTS);
	}

	protected void on_button_delete_confirm_accept_clicked (object o, EventArgs args)
	{
		deleteTestLine();

		button_deleted_test.Click();

		ITreeModel model;
		TreeIter iter;
		if (treeview_more.Selection.GetSelected (out model, out iter)) 
			store.Remove(ref iter);

		button_delete_type.Sensitive = false;
		notebook.Page = Convert.ToInt32(notebookPages.TESTS);
	}

	///this should be abstract
	protected virtual string [] findTestTypesInSessions() {
		string [] nothing = new String[0];
		return nothing;
	}
	
	//fired when something is selected for drawing on imageTest
	public Button Button_selected
	{
		get { return button_selected; }
	}

	public Button Button_deleted_test
	{
		get { return button_deleted_test; }
	}

	public Button Button_accept {
		set { button_accept = value; }
		get { return button_accept; }
	}
	
	public Button Button_cancel {
		set { button_cancel = value; }
		get { return button_cancel; }
	}

	public string SelectedEventName
	{
		set { selectedEventName = value; }
		get { return selectedEventName; }
	}
	
	public string SelectedDescription {
		get { return selectedDescription; }
	}

	protected void connectWidgetsEventMore (Gtk.Builder builder)
	{
		notebook = (Gtk.Notebook) builder.GetObject ("notebook");
		treeview_more = (Gtk.TreeView) builder.GetObject ("treeview_more");
		button_accept = (Gtk.Button) builder.GetObject ("button_accept");
		button_delete_type = (Gtk.Button) builder.GetObject ("button_delete_type");
		button_cancel = (Gtk.Button) builder.GetObject ("button_cancel");
		button_close = (Gtk.Button) builder.GetObject ("button_close");
		button_close1 = (Gtk.Button) builder.GetObject ("button_close1");
		label_delete_confirm = (Gtk.Label) builder.GetObject ("label_delete_confirm");
		label_delete_confirm_name = (Gtk.Label) builder.GetObject ("label_delete_confirm_name");
		label_delete_cannot = (Gtk.Label) builder.GetObject ("label_delete_cannot");
		image_delete = (Gtk.Image) builder.GetObject ("image_delete");
		image_delete1 = (Gtk.Image) builder.GetObject ("image_delete1");
	}
}
