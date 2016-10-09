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
 *  Copyright (C) 2016   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections.Generic; //List<T>
using Gtk;

public class ChronopicRegisterWindow
{
	Gtk.Window chronopic_register_win;
	Gtk.VBox vbox_main;
	Gtk.Table table;

	public ChronopicRegisterWindow(List<ChronopicRegisterPort> list)
	{
		createWindow();
		//put an icon to window
		UtilGtk.IconWindow(chronopic_register_win);

		createVBoxMain();
		createTreeView(list);
		createButton();

		chronopic_register_win.ShowAll();
	}


	private void createWindow()
	{
		chronopic_register_win = new Window ("Chronopic register");

		chronopic_register_win.DeleteEvent += on_delete_event;

		/* Sets the border width of the window. */
		chronopic_register_win.BorderWidth= 20;
	}

	private void createVBoxMain()
	{
		vbox_main = new Gtk.VBox(false, 20);
		chronopic_register_win.Add(vbox_main);
	}


	Gtk.TreeView treeview;
	Gtk.ListStore listStoreTypes;
	Gtk.ListStore listStoreAll;

	//based on: http://www.mono-project.com/docs/gui/gtksharp/widgets/treeview-tutorial/
	//and: http://stackoverflow.com/questions/12679688/updating-treeview-after-changing-cellrenderercombo-gtk
	private void createTreeView(List<ChronopicRegisterPort> list)
	{
		treeview = new Gtk.TreeView();

		// Create column , cell renderer and add the cell to the 1st column
		Gtk.TreeViewColumn serialNCol = new Gtk.TreeViewColumn ();
		serialNCol.Title = "Serial Number";
		Gtk.CellRendererText serialNCell = new Gtk.CellRendererText ();
		serialNCol.PackStart (serialNCell, true);

		//--------------------- combo start --------------
		Gtk.TreeViewColumn typeCol = new Gtk.TreeViewColumn ();
		typeCol.Title = "Type";
		Gtk.CellRendererCombo typeCell = new Gtk.CellRendererCombo ();
		typeCell.Editable = true;

		listStoreTypes = new Gtk.ListStore(typeof (string));
		int maxChars = 0;
		foreach(string s in Enum.GetNames(typeof(ChronopicRegisterPort.Types))) {
			listStoreTypes.AppendValues (s);
			if(s.Length > maxChars)
				maxChars = s.Length;
		}

		typeCell.Model = listStoreTypes;
		typeCell.WidthChars = maxChars + 10; //enough space to show the dropdown list button
		typeCell.TextColumn = 0;
		typeCell.Edited += comboChanged;

		typeCol.PackStart (typeCell, false);
		//--------------------- combo end --------------


		// Create column , cell renderer and add the cell to the 3rd column
		Gtk.TreeViewColumn portCol = new Gtk.TreeViewColumn ();
		portCol.Title = "Port";
		Gtk.CellRendererText portCell = new Gtk.CellRendererText ();
		portCol.PackStart (portCell, true);

		// Add the columns to the TreeView
		treeview.AppendColumn (serialNCol);
		treeview.AppendColumn (typeCol);
		treeview.AppendColumn (portCol);

		//Tell the Cell Renderers which items in the model to display
		serialNCol.AddAttribute (serialNCell, "text", 0);
		typeCol.AddAttribute (typeCell, "text", 1);
		portCol.AddAttribute (portCell, "text", 2);

		//listStoreAll = new Gtk.ListStore (typeof (string), typeof(Gtk.ComboBox), typeof(string));
		listStoreAll = new Gtk.ListStore (typeof (string), typeof(string), typeof(string));

		treeview.Model = listStoreAll;

		bool chronopicsFound = false;
		foreach(ChronopicRegisterPort crp in list) {
			if(crp.Port != "") {
				listStoreAll.AppendValues(crp.SerialNumber, crp.Type.ToString(), crp.Port);
				chronopicsFound = true;
			}
		}

		Gtk.Label label;
		if(chronopicsFound) {
			label = new Gtk.Label("To change values:\nClick on <b>Type</b> column and the press Enter.");
			label.UseMarkup = true;
		} else {
			label = new Gtk.Label("Chronopic/s not found:\nConnect and reopen this window.");
		}

		Gtk.VBox vboxTV = new Gtk.VBox(false, 8);
		vboxTV.Add(treeview);
		vboxTV.Add(label);

		vbox_main.Add(vboxTV);
	}

	/*
	   void comboChangedOld (object o, EditedArgs args)
	   {
	   TreeSelection selection = treeview.Selection;
	   TreeIter iter;
	   if (!selection.GetSelected (out iter))
	   return;

	   listStoreAll.SetValue (iter, 1, args.NewText);
	   }
	   */

	void comboChanged (object o, EditedArgs args)
	{
		Gtk.TreeIter iter;
		listStoreAll.GetIter (out iter, new Gtk.TreePath (args.Path));

		//update value on treeview
		listStoreAll.SetValue (iter, 1, args.NewText);

		//store on SQL
		string serialNumber = (string) listStoreAll.GetValue (iter, 0);
		ChronopicRegisterPort.Types type = (ChronopicRegisterPort.Types) Enum.Parse(
				typeof(ChronopicRegisterPort.Types), args.NewText);
		string port = (string) listStoreAll.GetValue (iter, 2);

		ChronopicRegisterPort crp = new ChronopicRegisterPort(serialNumber, type);
		crp.Port = port;

		SqliteChronopicRegister.Update(false, crp, type);
	}

	private void createButton()
	{
		Gtk.Button button = new Gtk.Button("Close");
		button.Clicked += new EventHandler(on_button_clicked);
		vbox_main.Add(button);
	}

	private void on_button_clicked(object o, EventArgs args)
	{
		/*
		 * TODO:
		 * trying to manage if a combobox is changed but focus is still there. "Edited" is not called.
		 TreeSelection selection = treeview.Selection;
		 TreeIter iter;
		 if (selection.GetSelected (out iter)) {
		 LogB.Information("SOMETHING SELECTED");
		 selection.UnselectIter(iter);
		 }
		 else
		 LogB.Information("NOTHING SELECTED");
		 */

		chronopic_register_win.Hide();
		chronopic_register_win = null;
	}

	private void on_delete_event (object o, DeleteEventArgs args)
	{
		LogB.Information("calling on_delete_event");

		args.RetVal = true;

		on_button_clicked(new object(), new EventArgs());
	}
}

