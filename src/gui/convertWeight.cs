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
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using Mono.Unix;

public class ConvertWeightWindow 
{
	Gtk.Window convert_weight;
	Gtk.TreeView treeview1;
	Gtk.Label label_old_weight_value;
	Gtk.Label label_new_weight_value;
	Gtk.Button button_accept;
	Gtk.Button button_cancel;

	static ConvertWeightWindow ConvertWeightWindowBox;
	TreeStore store;
	double oldPersonWeight;
	double newPersonWeight;
	string [] jumpsSimple;
	string [] jumpsReactive;
	int columnBool1 = 6;
	int columnBool2 = 8;
	string simpleString;
	string reactiveString;
	
	ConvertWeightWindow (double oldPersonWeight, double newPersonWeight, string [] jumpsSimple, string [] jumpsReactive) {
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "convert_weight.glade", "convert_weight", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "convert_weight.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);
		
		//put an icon to window
		UtilGtk.IconWindow(convert_weight);

		this.oldPersonWeight = oldPersonWeight;
		this.newPersonWeight = newPersonWeight;
		this.jumpsSimple = jumpsSimple;
		this.jumpsReactive = jumpsReactive;
					
		simpleString = Catalog.GetString("Simple");
		reactiveString = Catalog.GetString("Reactive");
	
		createTreeViewWithCheckboxes(treeview1);
		
		store = new TreeStore( 
				typeof (string), //uniqueID
				typeof (string), //simple or reactive
				typeof (string), //jumpType
				typeof (string), //tf 
				typeof (string), //tc 
				/* following eg of a subject of 70Kg 
				 * that has done a jump with an extra of 70Kg
				 * and after (in same session) changes person weight to 80
				 */
				typeof (string), //weight % + weight kg (old) (eg: 100%-70Kg)
				typeof (bool), //mark new option 1
				typeof (string), //weight % + weight kg (new option1) (eg: 100%-80Kg)
				typeof (bool), //mark new option 2
				typeof (string) //weight % + weight kg (new option2) (eg: 87%-70Kg)
				);
		treeview1.Model = store;
		
		fillTreeView( treeview1, store );
	}

	static public ConvertWeightWindow Show (
			double oldPersonWeight, double newPersonWeight, string [] jumpsSimple, string [] jumpsReactive)
	{
		if (ConvertWeightWindowBox == null) {
			ConvertWeightWindowBox = 
				new ConvertWeightWindow (oldPersonWeight, newPersonWeight, jumpsSimple, jumpsReactive);
		}
	
		ConvertWeightWindowBox.label_old_weight_value.Text = oldPersonWeight.ToString() + " Kg";
		ConvertWeightWindowBox.label_new_weight_value.Text = newPersonWeight.ToString() + " Kg";

		ConvertWeightWindowBox.convert_weight.Show ();
		
		return ConvertWeightWindowBox;
	}

	protected void createTreeViewWithCheckboxes (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;
		tv.AppendColumn ( Catalog.GetString("ID"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( 
				Catalog.GetString("Simple") + " " +
				Catalog.GetString("or") + " " +
				Catalog.GetString("Reactive")
				, new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Type"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( 
				Catalog.GetString("TF") 
				/*
				+ "\n" + 
				Catalog.GetString("TF") + "(" + 
				Catalog.GetString("AVG") + ")"
				*/
				, new CellRendererText(), "text", count++);
		tv.AppendColumn ( 
				Catalog.GetString("TC") 
				/*
				+ "\n" + 
				Catalog.GetString("TC") + "(" + 
				Catalog.GetString("AVG") + ")"
				*/
				, new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Old weight"), new CellRendererText(), "text", count++);

		CellRendererToggle crt = new CellRendererToggle();
		crt.Visible = true;
		crt.Activatable = true;
		crt.Active = true;
		crt.Toggled += ItemToggled1;

		TreeViewColumn column = new TreeViewColumn ("", crt, "active", count);
		column.Clickable = true;
		tv.InsertColumn (column, count++);

		tv.AppendColumn ( Catalog.GetString("New weight\noption 1"), new CellRendererText(), "text", count++);

		CellRendererToggle crt2 = new CellRendererToggle();
		crt2.Visible = true;
		crt2.Activatable = true;
		crt2.Active = true;
		crt2.Toggled += ItemToggled2;

		column = new TreeViewColumn ("", crt2, "active", count);
		column.Clickable = true;
		tv.InsertColumn (column, count++);

		tv.AppendColumn ( Catalog.GetString("New weight\noption 2"), new CellRendererText(), "text", count++);
	}
	
	void ItemToggled1(object o, ToggledArgs args) {
		ItemToggled(columnBool1, columnBool2, o, args);
	}

	void ItemToggled2(object o, ToggledArgs args) {
		ItemToggled(columnBool2, columnBool1, o, args);
	}
	
	void ItemToggled(int columnThis, int columnOther, object o, ToggledArgs args) {
		TreeIter iter;
		if (store.GetIter (out iter, new TreePath(args.Path))) 
		{
			bool val = (bool) store.GetValue (iter, columnThis);
			LogB.Information (string.Format("toggled {0} with value {1}", args.Path, !val));

			if(args.Path == "0") {
				if (store.GetIterFirst(out iter)) {
					val = (bool) store.GetValue (iter, columnThis);
					store.SetValue (iter, columnThis, !val);
					store.SetValue (iter, columnOther, val);
					while ( store.IterNext(ref iter) ){
						store.SetValue (iter, columnThis, !val);
						store.SetValue (iter, columnOther, val);
					}
				}
			} else {
				store.SetValue (iter, columnThis, !val);
				store.SetValue (iter, columnOther, val);
				//usnelect "all" checkboxes
				store.GetIterFirst(out iter);
				store.SetValue (iter, columnThis, false);
				store.SetValue (iter, columnOther, false);
			}
		}
	}

	private string createStringCalculatingKgs (double personWeightKg, double jumpWeightPercent) {
		return jumpWeightPercent + "% " + 
			Convert.ToDouble(Util.WeightFromPercentToKg(jumpWeightPercent, personWeightKg)).ToString()
			+ "Kg";
	}

	private string createStringCalculatingPercent (double oldPersonWeightKg, double newPersonWeightKg, double jumpWeightPercent) {
		double jumpInKg = Util.WeightFromPercentToKg(jumpWeightPercent, oldPersonWeightKg);
		double jumpPercentToNewPersonWeight = Convert.ToDouble(Util.WeightFromKgToPercent(jumpInKg, newPersonWeightKg));
		return jumpPercentToNewPersonWeight + "% " + jumpInKg + "Kg";
	}

	protected void fillTreeView (Gtk.TreeView tv, TreeStore store) 
	{
		//add a string for first row (for checking or unchecking all)
		store.AppendValues ( "", "", "", "", "", "", true, "", false, "");
		
		foreach (string jump in jumpsSimple) {
			string [] myStringFull = jump.Split(new char[] {':'});
			store.AppendValues (
					myStringFull[1], //uniqueID
					simpleString,
					myStringFull[4], //type
					myStringFull[5], //tf
					myStringFull[6], //tf
					createStringCalculatingKgs(oldPersonWeight, Convert.ToDouble(Util.ChangeDecimalSeparator(myStringFull[8]))), //old weight
					true,
					createStringCalculatingKgs(newPersonWeight, Convert.ToDouble(Util.ChangeDecimalSeparator(myStringFull[8]))), //new weight 1
					false,
					createStringCalculatingPercent(oldPersonWeight, newPersonWeight, Convert.ToDouble(Util.ChangeDecimalSeparator(myStringFull[8]))) //new weight 2
					);
		}

		foreach (string jump in jumpsReactive) {
			string [] myStringFull = jump.Split(new char[] {':'});
			store.AppendValues (
					myStringFull[1], //uniqueID
					reactiveString,
					myStringFull[4], //type
					myStringFull[10], //tf (AVG)
					myStringFull[11], //tf (AVG)
					createStringCalculatingKgs(oldPersonWeight, Convert.ToDouble(Util.ChangeDecimalSeparator(myStringFull[8]))), //old weight
					true,
					createStringCalculatingKgs(newPersonWeight, Convert.ToDouble(Util.ChangeDecimalSeparator(myStringFull[8]))), //new weight 1
					false,
					createStringCalculatingPercent(oldPersonWeight, newPersonWeight, Convert.ToDouble(Util.ChangeDecimalSeparator(myStringFull[8]))) //new weight 2
					);
		}

		 
	}

	protected void on_button_cancel_clicked (object o, EventArgs args)
	{
		ConvertWeightWindowBox.convert_weight.Hide();
		ConvertWeightWindowBox = null;
	}
	
	protected void on_delete_event (object o, DeleteEventArgs args)
	{
		ConvertWeightWindowBox.convert_weight.Hide();
		ConvertWeightWindowBox = null;
	}
	
	protected void on_button_accept_clicked (object o, EventArgs args)
	{
		Gtk.TreeIter iter;
		
		int jumpID;
		bool option1;
		if (store.GetIterFirst(out iter)) {
			//don't catch 0 value
			while ( store.IterNext(ref iter) ){
				option1 = (bool) store.GetValue (iter, columnBool1);

				//only change in database if option is 2
				//because option 1 leaves the same percent and changes Kg (and database is in %)
				if(! option1) {
					//find the jumpID
					jumpID = Convert.ToInt32( treeview1.Model.GetValue(iter, 0) );

					//find weight (100% 80Kg)
					string weightString = (string) store.GetValue (iter, columnBool2 +1 );

					//find percent (it's before the '%' sign)
					string [] myStringFull = weightString.Split(new char[] {'%'});
					double percent = Convert.ToDouble(myStringFull[0]);

					//update DB
					//see if it's reactive
					string tableName = "jump";
					if ( (string) treeview1.Model.GetValue(iter, 1) == reactiveString )
						tableName = "jumpRj";

					SqliteJump.UpdateWeight(tableName, jumpID, percent);
				}
			}
		}

		ConvertWeightWindowBox.convert_weight.Hide();
		ConvertWeightWindowBox = null;
	}
	
	public Button Button_accept 
	{
		get { return button_accept; }
	}
	
	public Button Button_cancel 
	{
		get { return button_cancel; }
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		convert_weight = (Gtk.Window) builder.GetObject ("convert_weight");
		treeview1 = (Gtk.TreeView) builder.GetObject ("treeview1");
		label_old_weight_value = (Gtk.Label) builder.GetObject ("label_old_weight_value");
		label_new_weight_value = (Gtk.Label) builder.GetObject ("label_new_weight_value");
		button_accept = (Gtk.Button) builder.GetObject ("button_accept");
		button_cancel = (Gtk.Button) builder.GetObject ("button_cancel");
	}
}
