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
 * Xavier de Blas: 
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
 */

using System;
using System.Data;
using Gtk;
using System.Collections; //ArrayList


public class TreeViewJumps
{
	protected bool sortByType;
	protected bool showHeight;
	protected TreeStore store;
	protected Gtk.TreeView treeview;
	protected static int prefsDigitsNumber;
	protected static string allJumpsName = "All jumps";
	
	public TreeViewJumps ()
	{
	}
	
	public TreeViewJumps (Gtk.TreeView treeview, bool sortByType, bool showHeight, int newPrefsDigitsNumber)
	{
		this.treeview = treeview;
		this.sortByType = sortByType;
		this.showHeight = showHeight;
		prefsDigitsNumber = newPrefsDigitsNumber;

		if(showHeight) {
			store = getStore(5); //because, jumpID is not show in last col
			string [] columnsString = { "Jumper", "TV", "Height", "TC" };
			treeview.Model = store;
			prepareHeaders(columnsString);
		} else {
			store = getStore(4); //because, jumpID is not show in last col
			string [] columnsString = { "Jumper", "TV", "TC" };
			treeview.Model = store;
			prepareHeaders(columnsString);
		}
	}
	
	protected TreeStore getStore (int columns)
	{
		//prepares the TreeStore for required columns
		Type [] types = new Type [columns];
		for (int i=0; i < columns; i++) {
			types[i] = typeof (string);
		}
		TreeStore myStore = new TreeStore(types);
		return myStore;
	}
	
	protected virtual void prepareHeaders(string [] columnsString) 
	{
		treeview.HeadersVisible=true;
		int i=0;
		foreach(string myCol in columnsString) {
			treeview.AppendColumn (Catalog.GetString(myCol), new CellRendererText(), "text", i++);
		}
	}
	
	public virtual void RemoveColumns() {
		Gtk.TreeViewColumn [] myColumns = treeview.Columns;
		foreach (Gtk.TreeViewColumn column in myColumns) {
			treeview.RemoveColumn (column);
		}
	}

	public virtual void Fill(string [] myJumps, string filter)
	{
		TreeIter iter = new TreeIter();

		string tempJumper = ":"; //one value that's not possible
	
		string myType ;
		string myTypeComplet ;
		
		foreach (string jump in myJumps) {
			string [] myStringFull = jump.Split(new char[] {':'});

			//show always the names of jumpers ...
			if(tempJumper != myStringFull[0])
			{
				iter = store.AppendValues (myStringFull[0]);
				tempJumper = myStringFull[0];
			}

			//... but if we selected one type of jump and this it's not the type, don't show
			if(filter == allJumpsName || filter == myStringFull[4]) {

				myType = myStringFull[4];
				myTypeComplet = myType;
				//SJ+ weight and RJ limited, are in fall column
				if (myType == "DJ") {
					myTypeComplet = myType + "(" + myStringFull[7] + ")"; //fall
				} else if (myType == "SJ+") {
					myTypeComplet = myType + "(" + myStringFull[8] + ")"; //weight
				}
				
				if (showHeight) {
					store.AppendValues (iter,
						myTypeComplet,
						trimDecimals( myStringFull[5].ToString() ),
						trimDecimals( obtainHeight( myStringFull[5].ToString() ) ),
						trimDecimals( myStringFull[6].ToString() )
						, myStringFull[1] //jumpUniqueID (not shown) 
						);
				} else {
					store.AppendValues (iter, 
						myTypeComplet, 
						trimDecimals( myStringFull[5].ToString() ),
						trimDecimals( myStringFull[6].ToString() )
						, myStringFull[1] //jumpUniqueID (not shown) 
						);
				}
			}
		}	
	}
	
	public virtual void Add (string jumperName, Jump newJump)
	{
		TreeIter iter = new TreeIter();
		TreeIter iterDeep = new TreeIter();
		bool modelNotEmpty = treeview.Model.GetIterFirst ( out iter ) ;
		string iterJumperString;
		
		do {
			iterJumperString = ( treeview.Model.GetValue (iter, 0) ).ToString();
			if(iterJumperString == jumperName) {
				//expand the jumper
				treeview.ExpandToPath( treeview.Model.GetPath(iter) );
			
				if (showHeight) {
					iterDeep = store.AppendValues ( iter, newJump.Type, 
							trimDecimals(newJump.Tv.ToString()), 
							trimDecimals( obtainHeight( newJump.Tv.ToString() ) ),
							trimDecimals(newJump.Tc.ToString()), 
							newJump.UniqueID.ToString() );
				} else {
					iterDeep = store.AppendValues ( iter, newJump.Type, 
							trimDecimals(newJump.Tv.ToString()), 
							trimDecimals(newJump.Tc.ToString()), 
							newJump.UniqueID.ToString() );
				}
			}
		} while (treeview.Model.IterNext (ref iter));
	}
	
	public virtual void DelJump (int jumpID)
	{
		TreeIter iter = new TreeIter();
		treeview.Model.GetIterFirst ( out iter ) ;
		string iterJumperString;
		int jumpIDColumn = 3;
		if (showHeight) {
			jumpIDColumn = 4;
		}
		
		do {
			iterJumperString = ( treeview.Model.GetValue (iter, 0) ).ToString();
			if( treeview.Model.IterHasChild(iter) ) {
				treeview.Model.IterChildren (out iter, iter);
				do {
					int iterJumpID =  Convert.ToInt32 ( treeview.Model.GetValue (iter, jumpIDColumn) );
					//Console.WriteLine("iterJumpID {0}, jumpID{1}", iterJumpID, jumpID);
					if(iterJumpID == jumpID) {
						store.Remove(ref iter);
						return;
					}
				} while (treeview.Model.IterNext (ref iter));
				treeview.Model.IterParent (out iter, iter);
			}
		} while (treeview.Model.IterNext (ref iter));
	}
	
	protected static string trimDecimals (string time) {
		//the +2 is a workarround for not counting the two first characters: "0."
		//this will not work with the fall

		return time.Length > prefsDigitsNumber + 2 ? 
			time.Substring( 0, prefsDigitsNumber + 2 ) : 
				time;
	}
	
	protected static string obtainHeight (string time) {
		// s = 4.9 * (tv/2)exp2
		double myValue = 4.9 * ( Convert.ToDouble(time) / 2 ) * (Convert.ToDouble(time) / 2 ) ;

		return myValue.ToString();
	}

	public int JumpSelectedID {
		get {
			TreeIter iter = new TreeIter();
			TreeModel myModel = treeview.Model;
			if (treeview.Selection.GetSelected (out myModel, out iter)) {
				if(showHeight) {
					return Convert.ToInt32 ( treeview.Model.GetValue(iter, 4) );
				} else {
					return Convert.ToInt32 ( treeview.Model.GetValue(iter, 3) );
				}
			} else {
				return 0;
			}
		}
	}
	
}

public class TreeViewJumpsRj : TreeViewJumps
{
	public TreeViewJumpsRj (Gtk.TreeView treeview, bool showHeight, int newPrefsDigitsNumber)
	{
		this.treeview = treeview;
		this.showHeight = showHeight;
		prefsDigitsNumber = newPrefsDigitsNumber;

		if(showHeight) {
			store = getStore(5); //because, jumpID is not show in last col
			string [] columnsString = { "Jumper", "TV", "Height", "TC" };
			treeview.Model = store;
			prepareHeaders(columnsString);
		} else {
			store = getStore(4); //because, jumpID is not show in last col
			string [] columnsString = { "Jumper", "TV", "TC" };
			treeview.Model = store;
			prepareHeaders(columnsString);
		}
	}
	
	//filter is not used
	public override void Fill(string [] myJumps, string filter)
	{
		TreeIter iter = new TreeIter();
		TreeIter iterDeep = new TreeIter(); //only for RJ

		string tempJumper = ":"; //one value that's not possible

		string myType ;
		string myTypeComplet ;
			
		foreach (string jump in myJumps) {
			string [] myStringFull = jump.Split(new char[] {':'});

			//show always the names of jumpers ...
			if(tempJumper != myStringFull[0])
			{
				iter = store.AppendValues (myStringFull[0]);
				tempJumper = myStringFull[0];
			}


			myType = myStringFull[4];
			myTypeComplet = myType + "(" + myStringFull[16] + ") AVG: "; //limited

			if (showHeight) {
				iterDeep = store.AppendValues (iter,
						myTypeComplet,
						trimDecimals( myStringFull[10].ToString() ), //tvAvg
						trimDecimals( obtainHeight( myStringFull[10].ToString() ) ), //height(tvAvg)
						trimDecimals( myStringFull[11].ToString() ), //tcAvg
						myStringFull[1] //jumpUniqueID (not shown) 
						);
			} else {
				iterDeep = store.AppendValues (iter, 
						myTypeComplet, 
						trimDecimals( myStringFull[10].ToString() ), //tvAvg
						trimDecimals( myStringFull[11].ToString() ), //tcAvg
						myStringFull[1] //jumpUniqueID (not shown) 
						);
			}
			//if it's an RJ, we should make a deeper tree with all the jumps
			//the info above it's average

			string [] rjTvs = myStringFull[12].Split(new char[] {'='});
			string [] rjTcs = myStringFull[13].Split(new char[] {'='});
			int count = 0;
			foreach (string myTv in rjTvs) 
			{
				if (showHeight) {
					store.AppendValues (iterDeep, 
							(count+1).ToString(), 
							trimDecimals(myTv), 
							trimDecimals(obtainHeight(myTv)),
							trimDecimals(rjTcs[count]), 
							myStringFull[1] //jumpUniqueID 
							);
				} else {
					store.AppendValues (iterDeep, 
							(count+1).ToString(), 
							trimDecimals(myTv), 
							trimDecimals(rjTcs[count]),
							myStringFull[1] //jumpUniqueID 
							);
				}
				count ++;
			}
		}
	}
	
	public virtual void Add (string jumperName, JumpRj newJump)
	{
		TreeIter iter = new TreeIter();
		TreeIter iterDeep = new TreeIter();
		bool modelNotEmpty = treeview.Model.GetIterFirst ( out iter ) ;
		string iterJumperString;
		
		do {
			iterJumperString = ( treeview.Model.GetValue (iter, 0) ).ToString();
			if(iterJumperString == jumperName) {
				//expand the jumper
				treeview.ExpandToPath( treeview.Model.GetPath(iter) );
			
				string myTypeComplet = newJump.Type + "(" + newJump.Limited + ") AVG: "; //limited
				if (showHeight) {
					iterDeep = store.AppendValues ( iter, myTypeComplet, 
							trimDecimals(newJump.TvAvg.ToString()), 
							trimDecimals( obtainHeight( newJump.TvAvg.ToString() ) ),
							trimDecimals(newJump.TcAvg.ToString()), 
							newJump.UniqueID.ToString() );
				} else {
					iterDeep = store.AppendValues ( iter, myTypeComplet, 
							trimDecimals(newJump.TvAvg.ToString()), 
							trimDecimals(newJump.TcAvg.ToString()), 
							newJump.UniqueID.ToString() );
				}

				//fill the subjumps
				string [] myStringTv = newJump.TvString.Split(new char[] {'='});
				string [] myStringTc = newJump.TcString.Split(new char[] {'='});
				int count = 0;
				foreach (string myTv in myStringTv) {
					if (showHeight) {
						store.AppendValues ( iterDeep, (count+1).ToString(), 
								trimDecimals( myTv ), 
								trimDecimals( obtainHeight( myTv ) ),
								trimDecimals( myStringTc[count] ), 
								newJump.UniqueID.ToString() );
					} else {
						store.AppendValues ( iterDeep, (count+1).ToString(), 
								trimDecimals( myTv ), 
								trimDecimals( myStringTc[count] ), 
								newJump.UniqueID.ToString() );
					}
					count ++;
				}
			}
		} while (treeview.Model.IterNext (ref iter));
	}
	

	public virtual void ExpandOptimal()
	{
		TreeIter iter = new TreeIter();
		bool tvExists = treeview.Model.GetIterFirst ( out iter ) ; //returns false if empty
	
		if (tvExists) {
			do {
				treeview.ExpandToPath( treeview.Model.GetPath(iter) );
			} while (treeview.Model.IterNext (ref iter));
		}
	}
	
}
