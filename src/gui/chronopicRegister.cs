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

public class ChronopicRegisterWindowTypes
{
	public string SerialNumber;
	public string Port;
	public bool Unknown;
	public bool Contacts;
	public bool Encoder;

	public ChronopicRegisterWindowTypes (string serialNumber, string port, bool unknown, bool contacts, bool encoder)
	{
		this.SerialNumber = serialNumber;
		this.Port = port;
		this.Unknown = unknown;
		this.Contacts = contacts;
		this.Encoder = encoder;
	}

	public ChronopicRegisterWindowTypes (ChronopicRegisterPort crp)
	{
		this.SerialNumber = crp.SerialNumber;
		this.Port = crp.Port;

		Unknown = false;
		Contacts = false;
		Encoder = false;

		if(crp.Type == ChronopicRegisterPort.Types.UNKNOWN)
			Unknown = true;
		else if(crp.Type == ChronopicRegisterPort.Types.CONTACTS)
			Contacts = true;
		else
			Encoder = true;
	}
}


public class ChronopicRegisterWindow
{
	Gtk.Window chronopic_register_win;
	Gtk.VBox vbox_main;

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
		chronopic_register_win.AllowGrow = false;

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
	Gtk.ListStore listStoreAll;

	//based on: ~/informatica/progs_meus/mono/treemodel.cs
	private void createTreeView(List<ChronopicRegisterPort> list)
	{
		treeview = new Gtk.TreeView();

		// Create column , cell renderer and add the cell to the serialN column
		Gtk.TreeViewColumn serialNCol = new Gtk.TreeViewColumn ();
		serialNCol.Title = "Serial Number";
		Gtk.CellRendererText serialNCell = new Gtk.CellRendererText ();
		serialNCol.PackStart (serialNCell, true);

		// Create column , cell renderer and add the cell to the port column
		Gtk.TreeViewColumn portCol = new Gtk.TreeViewColumn ();
		portCol.Title = "Port";
		Gtk.CellRendererText portCell = new Gtk.CellRendererText ();
		portCol.PackStart (portCell, true);


		//-- cell renderer toggles

		Gtk.TreeViewColumn unknownCol = new Gtk.TreeViewColumn ();
		unknownCol.Title = "Unknown";
		Gtk.CellRendererToggle unknownCell = new Gtk.CellRendererToggle ();
		unknownCell.Activatable = true;
		unknownCell.Radio = true; 	//draw as radiobutton
		unknownCell.Toggled += new Gtk.ToggledHandler (unknownToggled);
		unknownCol.PackStart (unknownCell, true);

		Gtk.TreeViewColumn contactsCol = new Gtk.TreeViewColumn ();
		contactsCol.Title = "Contacts";
		Gtk.CellRendererToggle contactsCell = new Gtk.CellRendererToggle ();
		contactsCell.Activatable = true;
		contactsCell.Radio = true; 	//draw as radiobutton
		contactsCell.Toggled += new Gtk.ToggledHandler (contactsToggled);
		contactsCol.PackStart (contactsCell, true);

		Gtk.TreeViewColumn encoderCol = new Gtk.TreeViewColumn ();
		encoderCol.Title = "Encoder";
		Gtk.CellRendererToggle encoderCell = new Gtk.CellRendererToggle ();
		encoderCell.Activatable = true;
		encoderCell.Radio = true; 	//draw as radiobutton
		encoderCell.Toggled += new Gtk.ToggledHandler (encoderToggled);
		encoderCol.PackStart (encoderCell, true);

		//-- end of cell renderer toggles


		listStoreAll = new Gtk.ListStore (typeof (ChronopicRegisterWindowTypes));

		bool chronopicsFound = false;
		foreach(ChronopicRegisterPort crp in list) {
			if(crp.Port != "") {
				listStoreAll.AppendValues(new ChronopicRegisterWindowTypes(crp));
				chronopicsFound = true;
			}
		}

		serialNCol.SetCellDataFunc (serialNCell, new Gtk.TreeCellDataFunc (RenderSerialN));
		portCol.SetCellDataFunc (portCell, new Gtk.TreeCellDataFunc (RenderPort));
		unknownCol.SetCellDataFunc (unknownCell, new Gtk.TreeCellDataFunc (RenderUnknown));
		contactsCol.SetCellDataFunc (contactsCell, new Gtk.TreeCellDataFunc (RenderContacts));
		encoderCol.SetCellDataFunc (encoderCell, new Gtk.TreeCellDataFunc (RenderEncoder));

		treeview.Model = listStoreAll;

		// Add the columns to the TreeView
		treeview.AppendColumn (serialNCol);
		treeview.AppendColumn (portCol);
		treeview.AppendColumn (unknownCol);
		treeview.AppendColumn (contactsCol);
		treeview.AppendColumn (encoderCol);


		Gtk.Label label;
		if(chronopicsFound)
			label = new Gtk.Label("");
		else
			label = new Gtk.Label("Chronopic/s not found:\nConnect and reopen this window.");

		Gtk.VBox vboxTV = new Gtk.VBox(false, 8);
		vboxTV.Add(treeview);
		vboxTV.Add(label);

		vbox_main.Add(vboxTV);
	}


	private void RenderSerialN (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		ChronopicRegisterWindowTypes crwt = (ChronopicRegisterWindowTypes) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = crwt.SerialNumber;
	}

	private void RenderPort (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		ChronopicRegisterWindowTypes crwt = (ChronopicRegisterWindowTypes) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererText).Text = crwt.Port;
	}

	private void RenderUnknown (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		ChronopicRegisterWindowTypes crwt = (ChronopicRegisterWindowTypes) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererToggle).Active = crwt.Unknown;
	}

	private void RenderContacts (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		ChronopicRegisterWindowTypes crwt = (ChronopicRegisterWindowTypes) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererToggle).Active = crwt.Contacts;
	}

	private void RenderEncoder (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		ChronopicRegisterWindowTypes crwt = (ChronopicRegisterWindowTypes) model.GetValue (iter, 0);
		(cell as Gtk.CellRendererToggle).Active = crwt.Encoder;
	}



	private void unknownToggled (object sender, Gtk.ToggledArgs args)
	{
		Gtk.TreeIter iter;
		listStoreAll.GetIter (out iter, new Gtk.TreePath (args.Path));

		ChronopicRegisterWindowTypes crwt = (ChronopicRegisterWindowTypes) listStoreAll.GetValue (iter, 0);

		if(! crwt.Unknown) {
			crwt.Unknown = true;
			crwt.Contacts = false;
			crwt.Encoder = false;
		}

		//store on SQL
		SqliteChronopicRegister.Update(false,
				new ChronopicRegisterPort(crwt.SerialNumber, ChronopicRegisterPort.Types.UNKNOWN),
				ChronopicRegisterPort.Types.UNKNOWN);
	}

	private void contactsToggled (object sender, Gtk.ToggledArgs args)
	{
		Gtk.TreeIter iter;
		listStoreAll.GetIter (out iter, new Gtk.TreePath (args.Path));

		ChronopicRegisterWindowTypes crwt = (ChronopicRegisterWindowTypes) listStoreAll.GetValue (iter, 0);

		if(! crwt.Contacts) {
			crwt.Unknown = false;
			crwt.Contacts = true;
			crwt.Encoder = false;
		}

		//store on SQL
		SqliteChronopicRegister.Update(false,
				new ChronopicRegisterPort(crwt.SerialNumber, ChronopicRegisterPort.Types.CONTACTS),
				ChronopicRegisterPort.Types.CONTACTS);
	}

	private void encoderToggled (object sender, Gtk.ToggledArgs args)
	{
		Gtk.TreeIter iter;
		listStoreAll.GetIter (out iter, new Gtk.TreePath (args.Path));

		ChronopicRegisterWindowTypes crwt = (ChronopicRegisterWindowTypes) listStoreAll.GetValue (iter, 0);

		if(! crwt.Encoder) {
			crwt.Unknown = false;
			crwt.Contacts = false;
			crwt.Encoder = true;
		}

		//store on SQL
		SqliteChronopicRegister.Update(false,
				new ChronopicRegisterPort(crwt.SerialNumber, ChronopicRegisterPort.Types.ENCODER),
				ChronopicRegisterPort.Types.ENCODER);
	}

	private void createButton()
	{
		Gtk.Button button = new Gtk.Button("Close");
		button.Clicked += new EventHandler(on_button_clicked);

		//button can be called clicking Escape key
		Gtk.AccelGroup ag = new Gtk.AccelGroup ();
		chronopic_register_win.AddAccelGroup (ag);

		button.AddAccelerator
			("activate", ag, new Gtk.AccelKey
			 (Gdk.Key.Escape, Gdk.ModifierType.None,
			  Gtk.AccelFlags.Visible));

		Gtk.HButtonBox hbox = new Gtk.HButtonBox ();
		hbox.Add(button);

		vbox_main.Add(hbox);
	}

	private void on_button_clicked(object o, EventArgs args)
	{
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

