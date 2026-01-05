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


public class RepairJumpRjWindow 
{
	Gtk.Window repair_sub_event;
	Gtk.Box hbox_notes_and_totaltime;
	Gtk.Label label_header;
	Gtk.Label label_totaltime_value;
	Gtk.TreeView treeview_subevents;
	Gtk.Button button_accept;
	Gtk.Button button_add_before;
	Gtk.Button button_add_after;
	Gtk.Button button_delete;
	Gtk.TextView textview1;

	private TreeStore store;
	static RepairJumpRjWindow RepairJumpRjWindowBox;
	//int pDN;

	JumpType jumpType;
	JumpRj jumpRj; //used on button_accept
	

	RepairJumpRjWindow (Gtk.Window parent, JumpRj myJump, int pDN)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "repair_sub_event.glade", "repair_sub_event", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "repair_sub_event.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);
	
		//put an icon to window
		UtilGtk.IconWindow(repair_sub_event);
	
		repair_sub_event.Parent = parent;
		this.jumpRj = myJump;

		//this.pDN = pDN;

		repair_sub_event.Title = Catalog.GetString("Repair reactive jump");
		label_header.Text = Constants.GetRepairWindowMessage ();

		jumpType = SqliteJumpType.SelectAndReturnJumpRjType(myJump.Type, false);
		
		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = createTextForTextView(jumpType);
		textview1.Buffer = tb;
		
		createTreeView(treeview_subevents);
		//count, tc, tv
		store = new TreeStore(typeof (string), typeof (string), typeof(string));
		treeview_subevents.Model = store;
		fillTreeView (treeview_subevents, store, myJump, pDN);
	
		button_add_before.Sensitive = false;
		button_add_after.Sensitive = false;
		button_delete.Sensitive = false;
		
		label_totaltime_value.Text = getTotalTime().ToString() + " " + Catalog.GetString("seconds");
		
		treeview_subevents.Selection.Changed += onSelectionEntry;
	}
	
	static public RepairJumpRjWindow Show (Gtk.Window parent, JumpRj myJump, int pDN)
	{
		//LogB.Information(myJump);
		if (RepairJumpRjWindowBox == null) {
			RepairJumpRjWindowBox = new RepairJumpRjWindow (parent, myJump, pDN);
		}
		
		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.WindowColor(RepairJumpRjWindowBox.repair_sub_event, Config.ColorBackground);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, RepairJumpRjWindowBox.label_header);
			UtilGtk.ContrastLabelsBox(Config.ColorBackgroundIsDark, RepairJumpRjWindowBox.hbox_notes_and_totaltime);
		}

		RepairJumpRjWindowBox.repair_sub_event.Show ();

		return RepairJumpRjWindowBox;
	}
	
	private string createTextForTextView (JumpType myJumpType) {
		string jumpTypeString = string.Format(Catalog.GetString(
					"JumpType: {0}."), myJumpType.Name);

		//if it's a jump type that starts in, then don't allow first TC be different than -1
		string startString = "";
		if(myJumpType.StartIn) {
			startString = string.Format(Catalog.GetString("\nThis jump type starts inside, the first time should be a flight time."));
		}

		string fixedString = "";
		if(myJumpType.FixedValue > 0) {
			if(myJumpType.JumpsLimited) {
				//if it's a jump type jumpsLimited with a fixed value, then don't allow the creation of more jumps, and respect the -1 at last TF if found
				fixedString = "\n" + string.Format(
						Catalog.GetPluralString(
							"This jump type is fixed to one jump.",
							"This jump type is fixed to {0} jumps.",
							Convert.ToInt32(myJumpType.FixedValue)),
						myJumpType.FixedValue) + " " +
					Catalog.GetString("You cannot add more.");
			} else {
				//if it's a jump type timeLimited with a fixed value, then complain when the total time is higher
				fixedString = "\n" + string.Format(
						Catalog.GetPluralString(
							"This jump type is fixed to one second.",
							"This jump type is fixed to {0} seconds.",
							Convert.ToInt32(myJumpType.FixedValue)),
						myJumpType.FixedValue) + " " +
					Catalog.GetString("You cannot add more.");
			}
		}
		return jumpTypeString + startString + fixedString;
	}

	
	private void createTreeView (Gtk.TreeView myTreeView) {
		myTreeView.HeadersVisible=true;
		int count = 0;

		myTreeView.AppendColumn ( Catalog.GetString ("Count"), new CellRendererText(), "text", count++);

		Gtk.TreeViewColumn tcColumn = new Gtk.TreeViewColumn ();
		tcColumn.Title = Catalog.GetString("TC");
		Gtk.CellRendererText tcCell = new Gtk.CellRendererText ();
		tcCell.Editable = true;
		tcCell.Edited += tcCellEdited;
		tcColumn.PackStart (tcCell, true);
		tcColumn.AddAttribute(tcCell, "text", count ++);
		myTreeView.AppendColumn ( tcColumn );
		
		Gtk.TreeViewColumn tvColumn = new Gtk.TreeViewColumn ();
		tvColumn.Title = Catalog.GetString("TF");
		Gtk.CellRendererText tvCell = new Gtk.CellRendererText ();
		tvCell.Editable = true;
		tvCell.Edited += tvCellEdited;
		tvColumn.PackStart (tvCell, true);
		tvColumn.AddAttribute(tvCell, "text", count ++);
		myTreeView.AppendColumn ( tvColumn );
	}
	
	private void tcCellEdited (object o, Gtk.EditedArgs args)
	{
		Gtk.TreeIter iter;
		store.GetIter (out iter, new Gtk.TreePath (args.Path));
		if(Util.IsNumber(args.NewText, true) && (string) treeview_subevents.Model.GetValue(iter,1) != "-1") {
			//if it's limited by fixed value of seconds
			//and new seconds are bigger than allowed, return
			if(jumpType.FixedValue > 0 && ! jumpType.JumpsLimited &&
					getTotalTime() //current total time in treeview
					- Convert.ToDouble((string) treeview_subevents.Model.GetValue(iter,1)) //-old cell
					+ Convert.ToDouble(args.NewText) //+new cell
					> jumpType.FixedValue) {	//bigger than allowed
				return;
			} else {
				store.SetValue(iter, 1, args.NewText);

				//update the totaltime label
				label_totaltime_value.Text = getTotalTime().ToString() + " " + Catalog.GetString("seconds");
			}
		}
		
		//if is not number or if it was -1, the old data will remain
	}

	private void tvCellEdited (object o, Gtk.EditedArgs args)
	{
		Gtk.TreeIter iter;
		store.GetIter (out iter, new Gtk.TreePath (args.Path));
		if(Util.IsNumber(args.NewText, true) && (string) treeview_subevents.Model.GetValue(iter,2) != "-1") {
			//if it's limited by fixed value of seconds
			//and new seconds are bigger than allowed, return
			if(jumpType.FixedValue > 0 && ! jumpType.JumpsLimited &&
					getTotalTime() //current total time in treeview
					- Convert.ToDouble((string) treeview_subevents.Model.GetValue(iter,2)) //-old cell
					+ Convert.ToDouble(args.NewText) //+new cell
					> jumpType.FixedValue) {	//bigger than allowed
				return;
			} else {
				store.SetValue(iter, 2, args.NewText);
				
				//update the totaltime label
				label_totaltime_value.Text = getTotalTime().ToString() + " " + Catalog.GetString("seconds");
			}
		}
		//if is not number or if it was -1, the old data will remain
	}

	private double getTotalTime() {
		TreeIter myIter;
		double totalTime = 0;
		bool iterOk = store.GetIterFirst (out myIter);
		string stringTc = "";
		string stringTv = "";
		if(iterOk) {
			do {
				//be cautious because when there's no value (like first tc in a jump that starts in,
				//it's stored as "-1", but it's shown as "-"
				stringTc = (string) treeview_subevents.Model.GetValue(myIter, 1);
				if(stringTc != "-" && stringTc != "-1") 
					totalTime += Convert.ToDouble(stringTc);
				
				stringTv = (string) treeview_subevents.Model.GetValue(myIter, 2);
				if(stringTv != "-" && stringTv != "-1") 
					totalTime += Convert.ToDouble(stringTv);
				
			} while (store.IterNext (ref myIter));
		}
		return totalTime;
	}
	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store, JumpRj myJump, int pDN)
	{
		if(myJump.TcString.Length > 0 && myJump.TvString.Length > 0) {
			string [] tcArray = myJump.TcString.Split(new char[] {'='});
			string [] tvArray = myJump.TvString.Split(new char[] {'='});

			int count = 0;
			foreach (string myTc in tcArray) {
				string myTv;
				if(tvArray.Length >= count)
					myTv = Util.TrimDecimals(tvArray[count], pDN);
				else
					myTv = "";

				store.AppendValues ( (count+1).ToString(), Util.TrimDecimals(myTc, pDN), myTv );

				count ++;
			}
		}
	}

	void onSelectionEntry (object o, EventArgs args) {
		ITreeModel model;
		TreeIter iter;
		
		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			button_add_before.Sensitive = true;
			button_add_after.Sensitive = true;
			button_delete.Sensitive = true;

			//don't allow to add a row before the first if first row has a -1 in 'TC'
			//also don't allow deleting
			if((string) model.GetValue (iter, 1) == "-1") {
				button_add_before.Sensitive = false;
				button_delete.Sensitive = false;
			}

			//don't allow to add a row after the last if it has a -1
			//also don't allow deleting
			//the only -1 in flight time can be in the last row
			if((string) model.GetValue (iter, 2) == "-1") {
				button_add_after.Sensitive = false;
				button_delete.Sensitive = false;
			}
			
			//don't allow to add a row before or after 
			//if the jump type is fixed to n jumps and we reached n
			if(jumpType.FixedValue > 0 && jumpType.JumpsLimited) {
				int lastRow = 0;
				do {
					lastRow = Convert.ToInt32 ((string) model.GetValue (iter, 0));
				} while (store.IterNext (ref iter));

				//don't allow if max rows reached
				if(lastRow == jumpType.FixedValue ||
						( lastRow == jumpType.FixedValue +1 && jumpType.StartIn) ) {
					button_add_before.Sensitive = false;
					button_add_after.Sensitive = false;
				}
			}
		}
	}

	void on_button_add_before_clicked (object o, EventArgs args) {
		ITreeModel model; 
		TreeIter iter; 
		if (treeview_subevents.Selection.GetSelected (out model, out iter)) {
			int position = Convert.ToInt32( (string) model.GetValue (iter, 0) ) -1; //count starts at '0'
			iter = store.InsertNode(position);
			store.SetValue(iter, 1, "0");
			store.SetValue(iter, 2, "0");
			putRowNumbers(store);
		}
	}
	
	void on_button_add_after_clicked (object o, EventArgs args) {
		ITreeModel model; 
		TreeIter iter; 
		if (treeview_subevents.Selection.GetSelected (out model, out iter)) {
			int position = Convert.ToInt32( (string) model.GetValue (iter, 0) ); //count starts at '0'
			iter = store.InsertNode(position);
			store.SetValue(iter, 1, "0");
			store.SetValue(iter, 2, "0");
			putRowNumbers(store);
		}
	}
	
	private void putRowNumbers(TreeStore myStore) {
		TreeIter myIter;
		bool iterOk = myStore.GetIterFirst (out myIter);
		if(iterOk) {
			int count = 1;
			do {
				store.SetValue(myIter, 0, (count++).ToString());
			} while (myStore.IterNext (ref myIter));
		}
	}
		
	void on_button_delete_clicked (object o, EventArgs args) {
		ITreeModel model; 
		TreeIter iter; 
		if (treeview_subevents.Selection.GetSelected (out model, out iter)) {
			store.Remove(ref iter);
			putRowNumbers(store);
		
			label_totaltime_value.Text = getTotalTime().ToString() + " " + Catalog.GetString("seconds");

			button_add_before.Sensitive = false;
			button_add_after.Sensitive = false;
			button_delete.Sensitive = false;
		}
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		//foreach all lines... extrac tcString and tvString
		TreeIter myIter;
		string tcString = "";
		string tvString = "";
		
		bool iterOk = store.GetIterFirst (out myIter);
		if(iterOk) {
			string equal= ""; //first iteration should not appear '='
			do {
				tcString = tcString + equal + (string) treeview_subevents.Model.GetValue (myIter, 1);
				tvString = tvString + equal + (string) treeview_subevents.Model.GetValue (myIter, 2);
				equal = "=";
			} while (store.IterNext (ref myIter));
		}
		
		jumpRj.TvString = tvString;	
		jumpRj.TcString = tcString;	
		jumpRj.Jumps = Util.GetNumberOfJumps(tvString, false);
		jumpRj.Time = Util.GetTotalTime(tcString, tvString);

		//calculate other variables needed for jumpRj creation
		
		if(jumpType.FixedValue > 0) {
			//if this jumpType has a fixed value of jumps or time, limitstring has not changed
			if(jumpType.JumpsLimited) {
				jumpRj.Limited = jumpType.FixedValue.ToString() + "J";
			} else {
				jumpRj.Limited = jumpType.FixedValue.ToString() + "T";
			}
		} else {
			//else limitstring should be calculated
			if(jumpType.JumpsLimited) {
				jumpRj.Limited = jumpRj.Jumps.ToString() + "J";
			} else {
				jumpRj.Limited = Util.GetTotalTime(tcString, tvString) + "T";
			}
		}

		//save it deleting the old first for having the same uniqueID
		Sqlite.Delete(false, Constants.JumpRjTable, jumpRj.UniqueID);
		jumpRj.InsertAtDB(false, Constants.JumpRjTable); 
		/*
		SqliteJump.InsertRj("jumpRj", jumpRj.UniqueID.ToString(), jumpRj.PersonID, jumpRj.SessionID, 
				jumpRj.Type, Util.GetMax(tvString), Util.GetMax(tcString), 
				jumpRj.Fall, jumpRj.Weight, jumpRj.Description,
				Util.GetAverage(tvString), Util.GetAverage(tcString),
				tvString, tcString,
				jumps, Util.GetTotalTime(tcString, tvString), limitString
				);
				*/

		//close the window
		RepairJumpRjWindowBox.repair_sub_event.Hide();
		RepairJumpRjWindowBox = null;
	}

	void on_button_cancel_clicked (object o, EventArgs args)
	{
		RepairJumpRjWindowBox.repair_sub_event.Hide();
		RepairJumpRjWindowBox = null;
	}
	
	void on_delete_event (object o, DeleteEventArgs args)
	{
		RepairJumpRjWindowBox.repair_sub_event.Hide();
		RepairJumpRjWindowBox = null;
	}
	
	public Button Button_accept 
	{
		set { button_accept = value;	}
		get { return button_accept;	}
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		repair_sub_event = (Gtk.Window) builder.GetObject ("repair_sub_event");
		hbox_notes_and_totaltime = (Gtk.Box) builder.GetObject ("hbox_notes_and_totaltime");
		label_header = (Gtk.Label) builder.GetObject ("label_header");
		label_totaltime_value = (Gtk.Label) builder.GetObject ("label_totaltime_value");
		treeview_subevents = (Gtk.TreeView) builder.GetObject ("treeview_subevents");
		button_accept = (Gtk.Button) builder.GetObject ("button_accept");
		button_add_before = (Gtk.Button) builder.GetObject ("button_add_before");
		button_add_after = (Gtk.Button) builder.GetObject ("button_add_after");
		button_delete = (Gtk.Button) builder.GetObject ("button_delete");
		textview1 = (Gtk.TextView) builder.GetObject ("textview1");
	}
}
