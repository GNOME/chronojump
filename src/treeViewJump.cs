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
	protected static int pDN; //prefsDigitsNumber;
	protected static string allJumpsName = "All jumps";
	
	public TreeViewJumps ()
	{
	}
	
	public TreeViewJumps (Gtk.TreeView treeview, bool sortByType, bool showHeight, int newPrefsDigitsNumber)
	{
		this.treeview = treeview;
		this.sortByType = sortByType;
		this.showHeight = showHeight;
		pDN = newPrefsDigitsNumber;

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

		string [] jumpTypes = SqliteJumpType.SelectJumpTypes("", false);
		
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
				if (Util.HasFall(jumpTypes, myType)) {
					myTypeComplet = myTypeComplet + "(" + myStringFull[7] + "cm)"; //fall
				} 
				if (Util.HasWeight(jumpTypes, myType)) {
					myTypeComplet = myTypeComplet + "(" + myStringFull[8] + ")"; //weight
				}
				
				if (showHeight) {
					store.AppendValues (iter,
						myTypeComplet,
						Util.TrimDecimals( myStringFull[5].ToString(), pDN ),
						Util.TrimDecimals( obtainHeight( myStringFull[5].ToString() ), pDN ),
						Util.TrimDecimals( myStringFull[6].ToString(), pDN )
						, myStringFull[1] //jumpUniqueID (not shown) 
						);
				} else {
					store.AppendValues (iter, 
						myTypeComplet, 
						Util.TrimDecimals( myStringFull[5].ToString(), pDN ),
						Util.TrimDecimals( myStringFull[6].ToString(), pDN )
						, myStringFull[1] //jumpUniqueID (not shown) 
						);
				}
			}
		}	
	}
	
	public virtual void Add (string jumperName, Jump newJump)
	{
		TreeIter iter = new TreeIter();
		bool modelNotEmpty = treeview.Model.GetIterFirst ( out iter ) ;
		string iterJumperString;
		string myTypeComplet ;
		bool found = false;
		string [] jumpTypes = SqliteJumpType.SelectJumpTypes("", false);
		
		if(modelNotEmpty) {
			do {
				iterJumperString = ( treeview.Model.GetValue (iter, 0) ).ToString();
				if(iterJumperString == jumperName) {
					found = true;

					//expand the jumper
					treeview.ExpandToPath( treeview.Model.GetPath(iter) );

					myTypeComplet = newJump.Type;
					//SJ+ weight and RJ limited, are in fall column
					if (Util.HasFall(jumpTypes, newJump.Type)) {
						myTypeComplet = myTypeComplet + "(" + newJump.Fall + "cm)"; //fall
					} 
					if (Util.HasWeight(jumpTypes, newJump.Type)) {
						myTypeComplet = myTypeComplet + "(" + newJump.Weight + ")"; //weight
					}
				
					if (showHeight) {
						store.AppendValues ( iter, myTypeComplet, 
								Util.TrimDecimals(newJump.Tv.ToString(), pDN), 
								Util.TrimDecimals( obtainHeight( newJump.Tv.ToString() ), pDN ),
								Util.TrimDecimals(newJump.Tc.ToString(), pDN), 
								newJump.UniqueID.ToString() );
					} else {
						store.AppendValues ( iter, myTypeComplet, 
								Util.TrimDecimals(newJump.Tv.ToString(), pDN), 
								Util.TrimDecimals(newJump.Tc.ToString(), pDN), 
								newJump.UniqueID.ToString() );
					}
				}
			} while (treeview.Model.IterNext (ref iter));
		}

		//if the jumper has not jumped in this session, it's name doesn't appear in the treeview
		//create the name, and write the jump
		if(! found) {
			iter = store.AppendValues (jumperName);
					
			myTypeComplet = newJump.Type;
			if (Util.HasFall(jumpTypes, newJump.Type)) {
				myTypeComplet = myTypeComplet + "(" + newJump.Fall + "cm)"; //fall
			}
			if (Util.HasWeight(jumpTypes, newJump.Type)) {
				myTypeComplet = myTypeComplet + "(" + newJump.Weight + ")"; //weight
			}
				
			if (showHeight) {
				store.AppendValues ( iter, myTypeComplet, 
						Util.TrimDecimals(newJump.Tv.ToString(), pDN), 
						Util.TrimDecimals( obtainHeight( newJump.Tv.ToString() ), pDN ),
						Util.TrimDecimals(newJump.Tc.ToString(), pDN), 
						newJump.UniqueID.ToString() );
			} else {
				store.AppendValues ( iter, myTypeComplet, 
						Util.TrimDecimals(newJump.Tv.ToString(), pDN), 
						Util.TrimDecimals(newJump.Tc.ToString(), pDN), 
						newJump.UniqueID.ToString() );
			}
			//expand the jumper
			treeview.ExpandToPath( treeview.Model.GetPath(iter) );
		}
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
		pDN = newPrefsDigitsNumber;

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
						Util.TrimDecimals( myStringFull[10].ToString(), pDN ), //tvAvg
						Util.TrimDecimals( obtainHeight( myStringFull[10].ToString() ), pDN ), //height(tvAvg)
						Util.TrimDecimals( myStringFull[11].ToString(), pDN ), //tcAvg
						myStringFull[1] //jumpUniqueID (not shown) 
						);
			} else {
				iterDeep = store.AppendValues (iter, 
						myTypeComplet, 
						Util.TrimDecimals( myStringFull[10].ToString(), pDN ), //tvAvg
						Util.TrimDecimals( myStringFull[11].ToString(), pDN ), //tcAvg
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
							Util.TrimDecimals(myTv, pDN), 
							Util.TrimDecimals(obtainHeight(myTv), pDN),
							Util.TrimDecimals(rjTcs[count], pDN), 
							myStringFull[1] //jumpUniqueID 
							);
				} else {
					store.AppendValues (iterDeep, 
							(count+1).ToString(), 
							Util.TrimDecimals(myTv, pDN), 
							Util.TrimDecimals(rjTcs[count], pDN),
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
		bool found = false;
		
		if(modelNotEmpty) {
			do {
				iterJumperString = ( treeview.Model.GetValue (iter, 0) ).ToString();
				if(iterJumperString == jumperName) {
					found = true;

					//expand the jumper
					treeview.ExpandToPath( treeview.Model.GetPath(iter) );

					string myTypeComplet = newJump.Type + "(" + newJump.Limited + ") AVG: "; //limited
					if (showHeight) {
						iterDeep = store.AppendValues ( iter, myTypeComplet, 
								Util.TrimDecimals(newJump.TvAvg.ToString(), pDN), 
								Util.TrimDecimals( obtainHeight( newJump.TvAvg.ToString() ), pDN ),
								Util.TrimDecimals(newJump.TcAvg.ToString(), pDN), 
								newJump.UniqueID.ToString() );
					} else {
						iterDeep = store.AppendValues ( iter, myTypeComplet, 
								Util.TrimDecimals(newJump.TvAvg.ToString(), pDN), 
								Util.TrimDecimals(newJump.TcAvg.ToString(), pDN), 
								newJump.UniqueID.ToString() );
					}

					//fill the subjumps
					string [] myStringTv = newJump.TvString.Split(new char[] {'='});
					string [] myStringTc = newJump.TcString.Split(new char[] {'='});
					int count = 0;
					foreach (string myTv in myStringTv) {
						if (showHeight) {
							store.AppendValues ( iterDeep, (count+1).ToString(), 
									Util.TrimDecimals( myTv, pDN ), 
									Util.TrimDecimals( obtainHeight( myTv ), pDN ),
									Util.TrimDecimals( myStringTc[count], pDN ), 
									newJump.UniqueID.ToString() );
						} else {
							store.AppendValues ( iterDeep, (count+1).ToString(), 
									Util.TrimDecimals( myTv, pDN ), 
									Util.TrimDecimals( myStringTc[count], pDN ), 
									newJump.UniqueID.ToString() );
						}
						count ++;
					}
				}
			} while (treeview.Model.IterNext (ref iter));
		}

		//if the jumper has not jumped in this session, it's name doesn't appear in the treeview
		//create the name, and write the jump
		if(! found) {
			iter = store.AppendValues (jumperName);
			
			string myTypeComplet = newJump.Type + "(" + newJump.Limited + ") AVG: "; //limited
			if (showHeight) {
				iterDeep = store.AppendValues ( iter, myTypeComplet, 
						Util.TrimDecimals(newJump.TvAvg.ToString(), pDN), 
						Util.TrimDecimals( obtainHeight( newJump.TvAvg.ToString() ), pDN ),
						Util.TrimDecimals(newJump.TcAvg.ToString(), pDN), 
						newJump.UniqueID.ToString() );
			} else {
				iterDeep = store.AppendValues ( iter, myTypeComplet, 
						Util.TrimDecimals(newJump.TvAvg.ToString(), pDN), 
						Util.TrimDecimals(newJump.TcAvg.ToString(), pDN), 
						newJump.UniqueID.ToString() );
			}

			//fill the subjumps
			string [] myStringTv = newJump.TvString.Split(new char[] {'='});
			string [] myStringTc = newJump.TcString.Split(new char[] {'='});
			int count = 0;
			foreach (string myTv in myStringTv) {
				if (showHeight) {
					store.AppendValues ( iterDeep, (count+1).ToString(), 
							Util.TrimDecimals( myTv, pDN ), 
							Util.TrimDecimals( obtainHeight( myTv ), pDN ),
							Util.TrimDecimals( myStringTc[count], pDN ), 
							newJump.UniqueID.ToString() );
				} else {
					store.AppendValues ( iterDeep, (count+1).ToString(), 
							Util.TrimDecimals( myTv, pDN ), 
							Util.TrimDecimals( myStringTc[count], pDN ), 
							newJump.UniqueID.ToString() );
				}
				count ++;
			}
		//expand the jumper
		treeview.ExpandToPath( treeview.Model.GetPath(iter) );
		}
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
