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
	protected bool showInitialSpeed;
	protected TreeStore store;
	protected Gtk.TreeView treeview;
	protected static int pDN; //prefsDigitsNumber;
	//protected static string allJumpsName = Catalog.GetString("All jumps");
	
	public TreeViewJumps ()
	{
	}
	
	public TreeViewJumps (Gtk.TreeView treeview, bool sortByType, bool showHeight, bool showInitialSpeed, int newPrefsDigitsNumber)
	{
		this.treeview = treeview;
		this.sortByType = sortByType;
		this.showHeight = showHeight;
		this.showInitialSpeed = showInitialSpeed;
		pDN = newPrefsDigitsNumber;

		string jumperName = Catalog.GetString("Jumper");
		string weightName = Catalog.GetString("Weight %");
		string fallName = Catalog.GetString("Fall");
		string heightName = Catalog.GetString("Height");
		string initialSpeedName = Catalog.GetString("Initial Speed");

		int colsNum = obtainJumpIDColumn() +1;
		store = getStore(colsNum); //because, jumpID is not show in last col
		string [] columnsString = { jumperName, "TC", "TV", weightName, fallName };
		treeview.Model = store;
		prepareHeaders(columnsString, heightName, initialSpeedName);
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
	
	protected virtual void prepareHeaders(string [] columnsString, string heightName, string initialSpeedName) 
	{
		treeview.HeadersVisible=true;
		int i=0;
		foreach(string myCol in columnsString) {
			treeview.AppendColumn (Catalog.GetString(myCol), new CellRendererText(), "text", i++);
		}
		if(showHeight) {
			treeview.AppendColumn (heightName, new CellRendererText(), "text", i++);
		}
		if(showInitialSpeed) {
			treeview.AppendColumn (initialSpeedName, new CellRendererText(), "text", i++);
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

		string [] jumpTypes = SqliteJumpType.SelectJumpTypes("", "", false);

		foreach (string jump in myJumps) {
			string [] myStringFull = jump.Split(new char[] {':'});

			//show always the names of jumpers ...
			if(tempJumper != myStringFull[0])
			{
				iter = store.AppendValues (myStringFull[0]);
				tempJumper = myStringFull[0];
			}

			//... but if we selected one type of jump and this it's not the type, don't show
			if(filter == Constants.AllJumpsName || filter == myStringFull[4]) {

				myType = myStringFull[4];
				string myFall = "";
				if (Util.HasFall(jumpTypes, myType)) {
					myFall = myStringFull[7] + "cm"; //fall
				} 
				string myWeight = "";
				if (Util.HasWeight(jumpTypes, myType)) {
					myWeight = myStringFull[8]; //weight
				}
		
				int colsNum = obtainJumpIDColumn() +1;
				string [] myData = new String [colsNum];
				int count = 0;
				myData[count++] = myType;
				myData[count++] = Util.TrimDecimals( myStringFull[6].ToString(), pDN );
				myData[count++] = Util.TrimDecimals( myStringFull[5].ToString(), pDN );
				myData[count++] = myWeight;
				myData[count++] = myFall;
				if(showHeight) {
					myData[count++] = Util.TrimDecimals( Util.GetHeightInCentimeters( myStringFull[5].ToString() ), pDN );
				}
				if(showInitialSpeed) {
					myData[count++] = Util.TrimDecimals( Util.GetInitialSpeed( myStringFull[5].ToString() ), pDN );
				}
				myData[count++] = myStringFull[1]; //jumpUniqueID (not shown) 

				store.AppendValues (iter, myData);
			}
		}	
	}
	
	public virtual void Add (string jumperName, Jump newJump)
	{
		TreeIter iter = new TreeIter();
		bool modelNotEmpty = treeview.Model.GetIterFirst ( out iter ) ;
		string iterJumperString;
		bool found = false;
		string [] jumpTypes = SqliteJumpType.SelectJumpTypes("", "", false);
	
		if(modelNotEmpty) {
			do {
				iterJumperString = ( treeview.Model.GetValue (iter, 0) ).ToString();
				if(iterJumperString == jumperName) {
					found = true;

					//expand the jumper
					treeview.ExpandToPath( treeview.Model.GetPath(iter) );

					string myFall = "";
					if (Util.HasFall(jumpTypes, newJump.Type)) {
						myFall = newJump.Fall + "cm";
					} 
					double myWeight = 0;
					if (Util.HasWeight(jumpTypes, newJump.Type)) {
						myWeight = newJump.Weight;
					}
			
					int colsNum = obtainJumpIDColumn() +1;
					string [] myData = new String [colsNum];
					int count = 0;
					myData[count++] = newJump.Type;
					myData[count++] = Util.TrimDecimals( newJump.Tc.ToString(), pDN );
					myData[count++] = Util.TrimDecimals( newJump.Tv.ToString(), pDN );
					myData[count++] = myWeight.ToString();
					myData[count++] = myFall;
					if(showHeight) {
						myData[count++] = Util.TrimDecimals( Util.GetHeightInCentimeters( newJump.Tv.ToString() ), pDN );
					}
					if(showInitialSpeed) {
						myData[count++] = Util.TrimDecimals( Util.GetInitialSpeed( newJump.Tv.ToString() ), pDN );
					}
					myData[count++] = newJump.UniqueID.ToString(); //jumpUniqueID (not shown) 
					Console.WriteLine(newJump.UniqueID.ToString()); //jumpUniqueID (not shown) 

					store.AppendValues (iter, myData);
				}
			} while (treeview.Model.IterNext (ref iter));
		}

		//if the jumper has not jumped in this session, it's name doesn't appear in the treeview
		//create the name, and write the jump
		if(! found) {
			iter = store.AppendValues (jumperName);
	
			string myFall = "";
			if (Util.HasFall(jumpTypes, newJump.Type)) {
				myFall = newJump.Fall + "cm";
			}
			double myWeight = 0;
			if (Util.HasWeight(jumpTypes, newJump.Type)) {
				myWeight = newJump.Weight;
			}
				
			int colsNum = obtainJumpIDColumn() +1;
			string [] myData = new String [colsNum];
			int count = 0;
			myData[count++] = newJump.Type;
			myData[count++] = Util.TrimDecimals( newJump.Tc.ToString(), pDN );
			myData[count++] = Util.TrimDecimals( newJump.Tv.ToString(), pDN );
			myData[count++] = myWeight.ToString();
			myData[count++] = myFall;
			if(showHeight) {
				myData[count++] = Util.TrimDecimals( Util.GetHeightInCentimeters( newJump.Tv.ToString() ), pDN );
			}
			if(showInitialSpeed) {
				myData[count++] = Util.TrimDecimals( Util.GetInitialSpeed( newJump.Tv.ToString() ), pDN );
			}
			myData[count++] = newJump.UniqueID.ToString(); //jumpUniqueID (not shown) 

			store.AppendValues (iter, myData);

			//expand the jumper
			treeview.ExpandToPath( treeview.Model.GetPath(iter) );
		}
	}

	protected int obtainJumpIDColumn () {
		int jumpIDColumn = 5;
		if (showHeight) {
			jumpIDColumn ++;
		}
		if (showInitialSpeed) {
			jumpIDColumn ++;
		}
		return jumpIDColumn;
	}
	
	public virtual void DelJump (int jumpID)
	{
		TreeIter iter = new TreeIter();
		treeview.Model.GetIterFirst ( out iter ) ;
		int jumpIDColumn = obtainJumpIDColumn();
	
		do {
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

	public int JumpSelectedID {
		get {
			TreeIter iter = new TreeIter();
			TreeModel myModel = treeview.Model;
			if (treeview.Selection.GetSelected (out myModel, out iter)) {
				if(showHeight) {
					return Convert.ToInt32 ( treeview.Model.GetValue(iter, obtainJumpIDColumn()) );
				} else {
					return Convert.ToInt32 ( treeview.Model.GetValue(iter, obtainJumpIDColumn()) );
				}
			} else {
				return 0;
			}
		}
	}
	
}

public class TreeViewJumpsRj : TreeViewJumps
{
	public TreeViewJumpsRj (Gtk.TreeView treeview, bool showHeight, bool showInitialSpeed, int newPrefsDigitsNumber)
	{
		this.treeview = treeview;
		this.showHeight = showHeight;
		this.showInitialSpeed = showInitialSpeed;
		pDN = newPrefsDigitsNumber;

		string jumperName = Catalog.GetString("Jumper");
		string weightName = Catalog.GetString("Weight %");
		string fallName = Catalog.GetString("Fall");
		string heightName = Catalog.GetString("Height");
		string initialSpeedName = Catalog.GetString("Initial Speed");
		
		int colsNum = obtainJumpIDColumn() +1;
		store = getStore(colsNum); //because, jumpID is not show in last col
		string [] columnsString = { jumperName, "TC", "TV", weightName, fallName };
		treeview.Model = store;
		prepareHeaders(columnsString, heightName, initialSpeedName);
	}
	
	public override void Fill(string [] myJumps, string filter)
	{
		TreeIter iter = new TreeIter();
		TreeIter iterDeep = new TreeIter(); //only for RJ

		string tempJumper = ":"; //one value that's not possible

		string myType ;
		string myTypeComplet ;
		string [] jumpTypes = SqliteJumpType.SelectJumpRjTypes("", false);
			
		foreach (string jump in myJumps) {
			string [] myStringFull = jump.Split(new char[] {':'});

			//show always the names of jumpers ...
			if(tempJumper != myStringFull[0])
			{
				iter = store.AppendValues (myStringFull[0]);
				tempJumper = myStringFull[0];
			}

			//... but if we selected one type of jump and this it's not the type, don't show
			if(filter == Constants.AllJumpsName || filter == myStringFull[4]) {
				myType = myStringFull[4];
				myTypeComplet = myType + "(" + myStringFull[16] + ") AVG: "; //limited

				string myFall = "";
				if (Util.HasFall(jumpTypes, myType)) {
					myFall = myStringFull[7] + "cm";
				}
				string myWeight = "";
				if (Util.HasWeight(jumpTypes, myType)) {
					myWeight = myStringFull[8];
				}

				
				int colsNum = obtainJumpIDColumn() +1;
				string [] myData = new String [colsNum];
				int count = 0;
				myData[count++] = myTypeComplet;
				myData[count++] = Util.TrimDecimals( myStringFull[11].ToString(), pDN );
				myData[count++] = Util.TrimDecimals( myStringFull[10].ToString(), pDN );
				myData[count++] = myWeight;
				myData[count++] = myFall;
				if(showHeight) {
					myData[count++] = Util.TrimDecimals( Util.GetHeightInCentimeters( myStringFull[10].ToString() ), pDN );
				}
				if(showInitialSpeed) {
					myData[count++] = Util.TrimDecimals( Util.GetInitialSpeed( myStringFull[10].ToString() ), pDN );
				}
				myData[count++] = myStringFull[1]; //jumpUniqueID (not shown) 

				iterDeep = store.AppendValues (iter, myData);

				//if it's an RJ, we should make a deeper tree with all the jumps
				//the info above it's average

				string [] rjTvs = myStringFull[12].Split(new char[] {'='});
				string [] rjTcs = myStringFull[13].Split(new char[] {'='});
				int countRows = 0;
				foreach (string myTv in rjTvs) 
				{
					string [] myData2 = new String [colsNum];
					count = 0;
					myData2[count++] = (countRows+1).ToString();
					myData2[count++] = Util.TrimDecimals( rjTcs[countRows], pDN );
					myData2[count++] = Util.TrimDecimals( myTv, pDN );
					myData2[count++] = "";
					myData2[count++] = "";
					if(showHeight) {
						myData2[count++] = Util.TrimDecimals( Util.GetHeightInCentimeters(myTv), pDN );
					}
					if(showInitialSpeed) {
						myData2[count++] = Util.TrimDecimals( Util.GetInitialSpeed(myTv), pDN );
					}
					myData2[count++] = myStringFull[1]; //jumpUniqueID (not shown) 

					store.AppendValues (iterDeep, myData2);

					
					countRows ++;
				}
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
		string [] jumpTypes = SqliteJumpType.SelectJumpRjTypes("", false);
		int colsNum = obtainJumpIDColumn() +1;
		
		if(modelNotEmpty) {
			do {
				iterJumperString = ( treeview.Model.GetValue (iter, 0) ).ToString();
				if(iterJumperString == jumperName) {
					found = true;
				
					string myFall = "";
					if (Util.HasFall(jumpTypes, newJump.Type)) {
						myFall = newJump.Fall + "cm";
					}
					double myWeight = 0;
					if (Util.HasWeight(jumpTypes, newJump.Type)) {
						myWeight = newJump.Weight;
					}

					//expand the jumper
					treeview.ExpandToPath( treeview.Model.GetPath(iter) );

					//if limited  value is "-1" comes from a "unlimited" repetitive jump, 
					//put jumps count as limitation value
					if(newJump.Limited == "-1J") { 
						newJump.Limited = Util.GetNumberOfJumps(newJump.TcString) + "J";
					}
					
					string myTypeComplet = newJump.Type + "(" + newJump.Limited + ") AVG: "; //limited
					
					string [] myData = new String [colsNum];
					int count = 0;
					myData[count++] = myTypeComplet;
					myData[count++] = Util.TrimDecimals( newJump.TcAvg.ToString(), pDN );
					myData[count++] = Util.TrimDecimals( newJump.TvAvg.ToString(), pDN );
					myData[count++] = myWeight.ToString();
					myData[count++] = myFall;
					if(showHeight) {
						myData[count++] = Util.TrimDecimals( Util.GetHeightInCentimeters( newJump.TvAvg.ToString() ), pDN );
					}
					if(showInitialSpeed) {
						myData[count++] = Util.TrimDecimals( Util.GetInitialSpeed( newJump.TvAvg.ToString() ), pDN );
					}
					myData[count++] = newJump.UniqueID.ToString(); //jumpUniqueID (not shown) 

					iterDeep = store.AppendValues (iter, myData);

					//fill the subjumps
					string [] myStringTv = newJump.TvString.Split(new char[] {'='});
					string [] myStringTc = newJump.TcString.Split(new char[] {'='});
					int countRows = 0;
					foreach (string myTv in myStringTv) {
						string [] myData2 = new String [colsNum];
						count = 0;
						myData2[count++] = (countRows+1).ToString();
						myData2[count++] = Util.TrimDecimals( myStringTc[countRows], pDN );
						myData2[count++] = Util.TrimDecimals( myTv, pDN );
						myData2[count++] = "";
						myData2[count++] = "";
						if(showHeight) {
							myData2[count++] = Util.TrimDecimals( Util.GetHeightInCentimeters(myTv), pDN );
						}
						if(showInitialSpeed) {
							myData2[count++] = Util.TrimDecimals( Util.GetInitialSpeed(myTv), pDN );
						}
						myData2[count++] = newJump.UniqueID.ToString(); //jumpUniqueID (not shown) 

						store.AppendValues (iterDeep, myData2);
							
						
						countRows ++;
					}
				}
			} while (treeview.Model.IterNext (ref iter));
		}

		//if the jumper has not jumped in this session, it's name doesn't appear in the treeview
		//create the name, and write the jump
		if(! found) {
			iter = store.AppendValues (jumperName);
			
			string myFall = "";
			if (Util.HasFall(jumpTypes, newJump.Type)) {
				myFall = newJump.Fall + "cm";
			}
			double myWeight = 0;
			if (Util.HasWeight(jumpTypes, newJump.Type)) {
				myWeight = newJump.Weight;
			}

			string myTypeComplet = newJump.Type + "(" + newJump.Limited + ") AVG: "; //limited
						
			string [] myData2 = new String [colsNum];
			int count = 0;
			myData2[count++] = myTypeComplet;
			myData2[count++] = Util.TrimDecimals( newJump.TcAvg.ToString(), pDN );
			myData2[count++] = Util.TrimDecimals( newJump.TvAvg.ToString(), pDN );
			myData2[count++] = myWeight.ToString();
			myData2[count++] = myFall;
			if(showHeight) {
				myData2[count++] = Util.TrimDecimals( Util.GetHeightInCentimeters(newJump.TvAvg.ToString()), pDN );
			}
			if(showInitialSpeed) {
				myData2[count++] = Util.TrimDecimals( Util.GetInitialSpeed(newJump.TvAvg.ToString()), pDN );
			}
			myData2[count++] = newJump.UniqueID.ToString(); //jumpUniqueID (not shown) 

			iterDeep = store.AppendValues (iter, myData2);
							
			//fill the subjumps
			string [] myStringTv = newJump.TvString.Split(new char[] {'='});
			string [] myStringTc = newJump.TcString.Split(new char[] {'='});
			int countRows = 0;
			foreach (string myTv in myStringTv) {
				myData2 = new String [colsNum];
				count = 0;
				myData2[count++] = (countRows+1).ToString();
				myData2[count++] = Util.TrimDecimals( myStringTc[countRows], pDN );
				myData2[count++] = Util.TrimDecimals( myTv, pDN );
				myData2[count++] = "";
				myData2[count++] = "";
				if(showHeight) {
					myData2[count++] = Util.TrimDecimals( Util.GetHeightInCentimeters(myTv), pDN );
				}
				if(showInitialSpeed) {
					myData2[count++] = Util.TrimDecimals( Util.GetInitialSpeed(myTv), pDN );
				}
				myData2[count++] = newJump.UniqueID.ToString(); //jumpUniqueID (not shown) 

				store.AppendValues (iterDeep, myData2);

				countRows ++;
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
