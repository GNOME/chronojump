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
using System.Text; //StringBuilder
using Gtk;
using System.Collections; //ArrayList
//using Gnu.Gettext;

/* ------------ CLASS HERENCY MAP ---------
 *
 * Stat
 * 	StatSjCmjAbk	
 *	StatDjTV	
 *	StatDjIndex	
 *		StatRjIndex
 *	StatPotencyAguado
 *	StatIE
 *		StatIUB
 * 	StatGlobal	//suitable for global and for a unique jumper
 * 		//StatGlobalInter
 *	//StatPersonInter
 *
 * ---------------------------------------
 */


public class Stat
{
	protected int sessionUniqueID;

	protected string sessionName;
	protected ArrayList sessions;
	
	protected string jumpType;
	protected bool sexSeparated;
	protected string operation;
	protected int limit;

	protected TreeStore store;
	protected TreeIter iter;
	protected Gtk.TreeView treeview;

	protected static int prefsDigitsNumber;
	//protected static string manName = "M";
	//protected static string womanName = "F";

	//if this is not present i have problems like (No overload for method `xxx' takes `0' arguments) with some inherited classes
	public Stat () 
	{
		this.sessionName = "";
		this.sexSeparated = false;
		this.limit = 0;
	}

	public Stat (Gtk.TreeView treeview, int sessionUniqueID, string sessionName, int newPrefsDigitsNumber, bool sexSeparated) 
	{
		completeConstruction (treeview, sessionUniqueID, sessionName, newPrefsDigitsNumber, sexSeparated);
	}

	protected virtual void completeConstruction (Gtk.TreeView treeview, int sessionUniqueID, string sessionName, int newPrefsDigitsNumber, bool sexSeparated)
	{
		this.sessionUniqueID = sessionUniqueID;
		this.sessionName = sessionName;
		this.treeview = treeview;
		prefsDigitsNumber = newPrefsDigitsNumber;
		this.sexSeparated = sexSeparated;

		store = getStore();
		treeview.Model = store;

		createTreeView_stats();

		iter = new TreeIter();
	}

	protected virtual TreeStore getStore ()
	{
		TreeStore myStore = new TreeStore(typeof(string));
		return myStore;
	}
	
	protected virtual void createTreeView_stats () {
	}

	public virtual void RemoveHeaders() {
	}

	public virtual void prepareData () {
	}

	protected virtual void printData (string a, string b, string c) {
	}

	//public virtual string ObtainEnunciate () {
	//}
		
	protected static string trimDecimals (string time) {
		//the +2 is a workarround for not counting the two first characters: "0."
		//this will not work with the fall
		return time.Length > prefsDigitsNumber + 2 ? 
			time.Substring( 0, prefsDigitsNumber + 2 ) : 
				time;
	}

	protected static double makeSquare (string myValueStr) {
		double myDouble;
		myDouble = Convert.ToDouble(myValueStr);
		return myDouble * myDouble;
	}
	
	protected static string calculateSD(double sumValues, double sumSquaredValues, int count) {
		return (System.Math.Sqrt(
				sumSquaredValues -(sumValues*sumValues/count) / (count -1) )).ToString();
	}
	
	public virtual void RemoveColumns() {
		Gtk.TreeViewColumn [] myColumns = treeview.Columns;
		foreach (Gtk.TreeViewColumn column in myColumns) {
			treeview.RemoveColumn (column);
		}
	}


	public string SessionName
	{
		get { 
			return sessionName;
		}
	}
	
	public ArrayList Sessions
	{
		get { 
			return sessions;
		}
	}


	~Stat() {}

}


public class StatSjCmjAbk : Stat
{
	//if this is not present i have problems like (No overload for method `xxx' takes `0' arguments) with some inherited classes
	public StatSjCmjAbk () 
	{
		this.sessionName = "";
		this.sexSeparated = false;
		this.limit = 0;
		this.operation = "MAX";
	}

	public StatSjCmjAbk (Gtk.TreeView treeview, int sessionUniqueID, string sessionName, int newPrefsDigitsNumber, string jumpType, bool sexSeparated, bool max, int limit) 
	{
		completeConstruction (treeview, sessionUniqueID, sessionName, newPrefsDigitsNumber, sexSeparated);
		this.jumpType = jumpType;
		this.limit = limit;
		if (max) {
			this.operation = "MAX";
		} else {
			this.operation = "AVG";
		}
	}

	protected override TreeStore getStore ()
	{
		//statName, person (sex), tv
		TreeStore myStore = new TreeStore(typeof (string), typeof (string), typeof(string));
		return myStore;
	}
	
	protected override void createTreeView_stats () {
		treeview.HeadersVisible=true;
		int count =0;

		treeview.AppendColumn ( Catalog.GetString("Position"), new CellRendererText(), "text", count++);
		treeview.AppendColumn ( Catalog.GetString("Jumper"), new CellRendererText(), "text", count++); //person(sex)
		treeview.AppendColumn ( "TV", new CellRendererText(), "text", count++);
	}
	
	
	public override void prepareData () 
	{
		ArrayList myArray; 
		bool index = false;
		if (limit > 0) {
			//classify by jumps
			myArray = Sqlite.StatOneJumpJumps(sessionUniqueID, jumpType, index, sexSeparated, limit);
		} else {
			//classify by jumpers
			myArray = Sqlite.StatOneJumpJumpers(sessionUniqueID, jumpType, sexSeparated, operation);
		}
	
		string [] stringFullResults;
		int secondCount = 0;
		bool firstGirl = true;
		string myTv = "";
		double sumTv = 0;
		double sumSquaredTv = 0;
		int i=0;
		
		for (i=0 ; i < myArray.Count ; i ++) {
			stringFullResults = myArray[i].ToString().Split(new char[] {':'});

			if(firstGirl && sexSeparated && stringFullResults[2] == "F") {
				secondCount = i;
				firstGirl = false;
			}
			
			if (jumpType == "SJ+") {
				myTv = trimDecimals (stringFullResults[0]) + "(" + 
					stringFullResults[3] + ")" ; //weight
			} else {
				myTv = trimDecimals (stringFullResults[0]) ;
			}
			
			printData ( (i-secondCount+1).ToString() , 
					stringFullResults[1] + "(" + stringFullResults[2] + ")", myTv );

			sumTv += Convert.ToDouble(stringFullResults[0]);
			sumSquaredTv += makeSquare(stringFullResults[0]);
		}
		
		printData ( "", "", "");
		printData ( "AVG"  , "", trimDecimals((sumTv/i).ToString()) );
		printData ( "SD"  , "", trimDecimals(
					calculateSD(sumTv, sumSquaredTv, i)) );
	}

	protected override void printData (string row, string person, string tv) 
	{
			iter = store.AppendValues (row, person, tv); 
	}
	
	public override string ToString () 
	{
		string operationString = "";
		if ( this.operation == "MAX" ) { 
			operationString =  Catalog.GetString ("by MAX flight time values "); 
		}
		else { operationString =  Catalog.GetString ("by AVG flight time values "); }

		string sexSeparatedString = "";
		if (this.sexSeparated) { sexSeparatedString =  Catalog.GetString ("sorted by sex"); }
	
		string inJump =  Catalog.GetString (" in ") + jumpType +  Catalog.GetString (" jump ");
		string inSession =  Catalog.GetString (" in '") + sessionName +  Catalog.GetString ("' session ");
		
		if ( this.limit == 0 ) { 
			return  Catalog.GetString ("Rank of jumpers ") + operationString + 
				inJump + inSession + sexSeparatedString + "."; 
		}
		else { 
			return  Catalog.GetString ("Selection of the ") + this.limit + 
				 Catalog.GetString (" MAX values of flight time ") + inJump + inSession + "."; 
		}
	}

}


public class StatDjTv : Stat
{
	
	public StatDjTv () 
	{
		this.sessionName = "";
		this.sexSeparated = false;
		this.limit = 0;
		this.operation = "MAX";
	}
	
	public StatDjTv (Gtk.TreeView treeview, int sessionUniqueID, string sessionName, int newPrefsDigitsNumber, bool sexSeparated, bool max, int limit) 
	{
		completeConstruction (treeview, sessionUniqueID, sessionName, newPrefsDigitsNumber, sexSeparated);
		this.limit = limit;
		if (max) {
			this.operation = "MAX";
		} else {
			this.operation = "AVG";
		}
	}
	
	protected override TreeStore getStore ()
	{
		//statName, person(sex), tv, tc, fall
		TreeStore myStore = new TreeStore(typeof (string), typeof (string), typeof(string), typeof (string), typeof(string));
		return myStore;
	}
	
	protected override void createTreeView_stats () {
		treeview.HeadersVisible=true;
		int count =0;

		treeview.AppendColumn ( Catalog.GetString("Position"), new CellRendererText(), "text", count++);
		treeview.AppendColumn ( Catalog.GetString("Jumper"), new CellRendererText(), "text", count++); //person(sex)
		treeview.AppendColumn ( "TV", new CellRendererText(), "text", count++);
		treeview.AppendColumn ( "TC", new CellRendererText(), "text", count++);
		treeview.AppendColumn ( Catalog.GetString("Fall"), new CellRendererText(), "text", count++);
	}

	public override void prepareData () 
	{
		string jumpType = "DJ";

		//ArrayList myArray = Sqlite.StatClassificationOneJump(sessionUniqueID, jumpType, sexSeparated);
		ArrayList myArray; 
		bool index = false;
		if (limit > 0) {
			//classify by jumps
			myArray = Sqlite.StatOneJumpJumps(sessionUniqueID, jumpType, index, sexSeparated, limit);
		} else {
			//classify by jumpers
			myArray = Sqlite.StatOneJumpJumpersDj(sessionUniqueID, index, sexSeparated, operation);
		}
	
		
		int secondCount = 0;
		bool firstGirl = true;
		string myTv = "";
		string [] stringFullResults;
		double sumTv = 0;
		double sumTc = 0;
		double sumFall = 0;
		double sumSquaredTv = 0;
		double sumSquaredTc = 0;
		double sumSquaredFall = 0;
		int i=0;
		
		for (i=0 ; i < myArray.Count ; i ++) {
			stringFullResults = myArray[i].ToString().Split(new char[] {':'});

			if(firstGirl && sexSeparated && stringFullResults[2] == "F") {
				secondCount = i;
				firstGirl = false;
			}
			
			printData ( (i-secondCount+1).ToString() , 
					stringFullResults[1] + "(" + stringFullResults[2] + ")", 
					trimDecimals (stringFullResults[0]), 
					trimDecimals(stringFullResults[3]), 
					trimDecimals (stringFullResults[4])
					);
			
			sumTv += Convert.ToDouble(stringFullResults[0]);
			sumTc += Convert.ToDouble(stringFullResults[3]);
			sumFall += Convert.ToDouble(stringFullResults[4]);
			sumSquaredTv += makeSquare(stringFullResults[0]);
			sumSquaredTc += makeSquare(stringFullResults[3]);
			sumSquaredFall += makeSquare(stringFullResults[4]);
		}
		
		printData ( "", "", "", "", "");
		printData ( "AVG"  , "", trimDecimals((sumTv/i).ToString()), 
				trimDecimals((sumTc/i).ToString()), trimDecimals((sumFall/i).ToString()) );
		printData ( "SD"  , "", 
				trimDecimals(calculateSD(sumTv, sumSquaredTv, i)),
				trimDecimals(calculateSD(sumTc, sumSquaredTc, i)),
				trimDecimals(calculateSD(sumFall, sumSquaredFall, i)) );
	}
			
	//protected override void printData (string row, string person, string tv, string tc, string fall) 
	protected void printData (string row, string person, string tv, string tc, string fall) 
	{
			iter = store.AppendValues (row, person, tv, tc, fall); 
	}

	public override string ToString () 
	{
		string operationString = "";
		if ( this.operation == "MAX" ) { 
			operationString =  Catalog.GetString ("by MAX flight time values "); 
		}
		else { operationString =  Catalog.GetString ("by AVG flight time values "); }

		string sexSeparatedString = "";
		if (this.sexSeparated) { sexSeparatedString =  Catalog.GetString ("sorted by sex"); }
	
		string inJump =  Catalog.GetString (" in ") + jumpType +  Catalog.GetString (" jump ");
		string inSession =  Catalog.GetString (" in '") + sessionName +  Catalog.GetString ("' session ");
		
		if ( this.limit == 0 ) { 
			return  Catalog.GetString ("Rank of jumpers ") + operationString + inJump + inSession + sexSeparatedString + "."; 
		}
		else { 
			return  Catalog.GetString ("Selection of the ") + this.limit + 
				 Catalog.GetString (" MAX values of flight time ") + inJump + inSession + "."; 
		}
	}

}


public class StatDjIndex : Stat
{
	
	//if this is not present i have problems like (No overload for method `xxx' takes `0' arguments) with some inherited classes
	public StatDjIndex () 
	{
		//this.sessionName = "unnamed";
		this.sessionName = "";
		this.sexSeparated = false;
		this.limit = 0;
		this.operation = "MAX";
	}
	
	public StatDjIndex (Gtk.TreeView treeview, int sessionUniqueID, string sessionName, int newPrefsDigitsNumber, bool sexSeparated, bool max, int limit) 
	{
		completeConstruction (treeview, sessionUniqueID, sessionName, newPrefsDigitsNumber, sexSeparated);
		this.jumpType = jumpType;
		this.limit = limit;
		if (max) {
			this.operation = "MAX";
		} else {
			this.operation = "AVG";
		}
	}

	protected override TreeStore getStore ()
	{
		//statName, person (sex), index, tv, tc, fall
		TreeStore myStore = new TreeStore(typeof (string), typeof (string), typeof (string), typeof(string), typeof (string), typeof(string));
		return myStore;
	}
	
	protected override void createTreeView_stats () {
		treeview.HeadersVisible=true;
		int count =0;

		treeview.AppendColumn ( Catalog.GetString("Position"), new CellRendererText(), "text", count++);
		treeview.AppendColumn ( Catalog.GetString("Jumper"), new CellRendererText(), "text", count++); //person (sex)
		treeview.AppendColumn ( Catalog.GetString("Index %"), new CellRendererText(), "text", count++);
		treeview.AppendColumn ( "TV", new CellRendererText(), "text", count++);
		treeview.AppendColumn ( "TC", new CellRendererText(), "text", count++);
		treeview.AppendColumn ( Catalog.GetString("Fall"), new CellRendererText(), "text", count++);
	}

	public override void prepareData () 
	{
		string jumpType = "DJ";

		ArrayList myArray; 
		bool index = true;
		if (limit > 0) {
			//classify by jumps
			myArray = Sqlite.StatOneJumpJumps(sessionUniqueID, jumpType, index, sexSeparated, limit);
		} else {
			//classify by jumpers
			myArray = Sqlite.StatOneJumpJumpersDj(sessionUniqueID, index, sexSeparated, operation);
		}

		int secondCount = 0;
		bool firstGirl = true;
		string myTv = "";
		string [] stringFullResults;
		double sumIndex = 0;
		double sumTv = 0;
		double sumTc = 0;
		double sumFall = 0;
		double sumSquaredIndex = 0;
		double sumSquaredTv = 0;
		double sumSquaredTc = 0;
		double sumSquaredFall = 0;
		int i=0;
		
		for (i=0 ; i < myArray.Count ; i ++) {
			stringFullResults = myArray[i].ToString().Split(new char[] {':'});

			if(firstGirl && sexSeparated && stringFullResults[2] == "F") {
				secondCount = i;
				firstGirl = false;
			}

			printData ( (i-secondCount+1).ToString() , 
					stringFullResults[1] + "(" + stringFullResults[2] + ")", 
					//trimDecimals ( 
					//	calculateIndex (stringFullResults[0], stringFullResults[3]) ), //index
					trimDecimals (stringFullResults[5]), //index
					trimDecimals (stringFullResults[0]), //tv
					trimDecimals (stringFullResults[3]), //tc
					trimDecimals (stringFullResults[4]) //fall
					);
			
			sumIndex += Convert.ToDouble(stringFullResults[5]); //index
			sumTv += Convert.ToDouble(stringFullResults[0]);
			sumTc += Convert.ToDouble(stringFullResults[3]);
			sumFall += Convert.ToDouble(stringFullResults[4]);
			sumSquaredIndex += makeSquare(stringFullResults[5]);
			sumSquaredTv += makeSquare(stringFullResults[0]);
			sumSquaredTc += makeSquare(stringFullResults[3]);
			sumSquaredFall += makeSquare(stringFullResults[4]);

		}
		
		printData ( "", "", "", "", "", "");
		printData ( "AVG"  , "", trimDecimals((sumIndex/i).ToString()), 
				trimDecimals((sumTv/i).ToString()), trimDecimals((sumTc/i).ToString()), 
				trimDecimals((sumFall/i).ToString()) );
		printData ( "SD"  , "", 
				trimDecimals(calculateSD(sumIndex, sumSquaredIndex, i)),
				trimDecimals(calculateSD(sumTv, sumSquaredTv, i)),
				trimDecimals(calculateSD(sumTc, sumSquaredTc, i)),
				trimDecimals(calculateSD(sumFall, sumSquaredFall, i)) );
	}

	//protected override void printData (string row, string person, string index, string tv, string tc, string fall) 
	protected void printData (string row, string person, string index, string tv, string tc, string fall) 
	{
			iter = store.AppendValues (row, person, index, tv, tc, fall); 
	}
	
	protected virtual string calculateIndex (string tv, string tc) 
	{
		return ( ( Convert.ToDouble(tv) - Convert.ToDouble(tc) ) * 100 / Convert.ToDouble(tc) ).ToString();
	}
	
	public override string ToString () 
	{
		string indexString = "100*(TV-TC)/TC ";
			
		string operationString = "";
		if ( this.operation == "MAX" ) { 
			operationString = Catalog.GetString("by MAX values of index: ") + indexString ; 
		}
		else { operationString = Catalog.GetString("by AVG values of index: ") + indexString ; }

		string sexSeparatedString = "";
		if (this.sexSeparated) { sexSeparatedString = Catalog.GetString("sorted by sex"); }

		string inJump = Catalog.GetString(" in ") + jumpType + Catalog.GetString(" jump ");
		string inSession = Catalog.GetString(" in '") + sessionName + Catalog.GetString("' session ");
		
		if ( this.limit == 0 ) { 
			return Catalog.GetString("Rank of jumpers ") + operationString + inJump + inSession + sexSeparatedString + "."; 
		}
		else { 
			return Catalog.GetString("Selection of the ") + this.limit + 
				Catalog.GetString(" MAX values of flight time ") + inJump + inSession + "."; 
		}
	}
}


//StatRjIndex class heredates from StatDjIndex the following methods:
//protected override TreeStore getStore ()
//protected void printData (string row, string person, string index, string tv, string tc, string fall)
//in fact, it's not a very useful class herency

public class StatRjIndex : StatDjIndex
{
	
	//if this is not present i have problems like (No overload for method `xxx' takes `0' arguments) with some inherited classes
	public StatRjIndex () 
	{
		//this.sessionName = "unnamed";
		this.sessionName = "";
		this.sexSeparated = false;
		this.limit = 0;
		this.operation = "MAX";
	}
	
	public StatRjIndex (Gtk.TreeView treeview, int sessionUniqueID, string sessionName, int newPrefsDigitsNumber, bool sexSeparated, bool max, int limit) 
	{
		completeConstruction (treeview, sessionUniqueID, sessionName, newPrefsDigitsNumber, sexSeparated);
		this.jumpType = jumpType;
		this.limit = limit;
		if (max) {
			this.operation = "MAX";
		} else {
			this.operation = "AVG";
		}
	}

	
	protected override void createTreeView_stats () {
		treeview.HeadersVisible=true;
		int count =0;

		treeview.AppendColumn ( Catalog.GetString("Position"), new CellRendererText(), "text", count++);
		treeview.AppendColumn ( Catalog.GetString("Jumper"), new CellRendererText(), "text", count++); //person (sex)
		treeview.AppendColumn ( Catalog.GetString("Index %"), new CellRendererText(), "text", count++);
		treeview.AppendColumn ( Catalog.GetString("TV (AVG)"), new CellRendererText(), "text", count++);
		treeview.AppendColumn ( Catalog.GetString("TC (AVG)"), new CellRendererText(), "text", count++);
		treeview.AppendColumn ( Catalog.GetString("Fall"), new CellRendererText(), "text", count++);
	}
	
	public override void prepareData () 
	{
		string jumpType = "RJ";

		ArrayList myArray; 
		bool index = true;
		if (limit > 0) {
			//classify by jumps
			myArray = Sqlite.StatRjJumps(sessionUniqueID, sexSeparated, limit);
		} else {
			//classify by jumpers
			myArray = Sqlite.StatOneJumpJumpersRj(sessionUniqueID, sexSeparated, operation);
		}

		int secondCount = 0;
		bool firstGirl = true;
		string myTv = "";
		string [] stringFullResults;
		double sumIndex = 0;
		double sumTv = 0;
		double sumTc = 0;
		double sumFall = 0;
		double sumSquaredIndex = 0;
		double sumSquaredTv = 0;
		double sumSquaredTc = 0;
		double sumSquaredFall = 0;
		int i=0;
		
		for (i=0 ; i < myArray.Count ; i ++) {
			stringFullResults = myArray[i].ToString().Split(new char[] {':'});

			if(firstGirl && sexSeparated && stringFullResults[2] == "F") {
				secondCount = i;
				firstGirl = false;
			}

			printData ( (i-secondCount+1).ToString() , 
					stringFullResults[1] + "(" + stringFullResults[2] + ")", 
					//trimDecimals ( 
					//	calculateIndex (stringFullResults[0], stringFullResults[3]) ), //index
					trimDecimals (stringFullResults[5]), //index
					trimDecimals (stringFullResults[0]), //tv
					trimDecimals (stringFullResults[3]), //tc
					trimDecimals (stringFullResults[4]) //fall
					);
			
			//sumIndex += Convert.ToDouble(calculateIndex (stringFullResults[0], stringFullResults[3]) ); //index
			sumIndex += Convert.ToDouble(stringFullResults[5]);
			sumTv += Convert.ToDouble(stringFullResults[0]);
			sumTc += Convert.ToDouble(stringFullResults[3]);
			sumFall += Convert.ToDouble(stringFullResults[4]);
			sumSquaredIndex += makeSquare(stringFullResults[5]);
			sumSquaredTv += makeSquare(stringFullResults[0]);
			sumSquaredTc += makeSquare(stringFullResults[3]);
			sumSquaredFall += makeSquare(stringFullResults[4]);
		}
		
		printData ( "", "", "", "", "", "");
		printData ( "AVG"  , "", trimDecimals((sumIndex/i).ToString()), 
				trimDecimals((sumTv/i).ToString()), trimDecimals((sumTc/i).ToString()), 
				trimDecimals((sumFall/i).ToString()) );
		printData ( "SD"  , "", 
				trimDecimals(calculateSD(sumIndex, sumSquaredIndex, i)),
				trimDecimals(calculateSD(sumTv, sumSquaredTv, i)),
				trimDecimals(calculateSD(sumTc, sumSquaredTc, i)),
				trimDecimals(calculateSD(sumFall, sumSquaredFall, i)) );
	}

	
	public override string ToString () 
	{
		string indexString = "100*(TV-TC)/TC ";
			
		string operationString = "";
		if ( this.operation == "MAX" ) { 
			operationString = Catalog.GetString("by MAX values of index: ") + indexString ; 
		}
		else { operationString = Catalog.GetString("by AVG values of index: ") + indexString ; }

		string sexSeparatedString = "";
		if (this.sexSeparated) { sexSeparatedString = Catalog.GetString("sorted by sex"); }
	
		string bySubjumps = Catalog.GetString(" using AVG of its subjumps ");
		string inJump = Catalog.GetString(" in ") + jumpType + Catalog.GetString(" jump ");
		string inSession = Catalog.GetString(" in '") + sessionName + Catalog.GetString("' session ");
		
		if ( this.limit == 0 ) { 
			return Catalog.GetString("Rank of jumpers ") + operationString + + bySubjumps + inSession + sexSeparatedString + "."; 
		}
		else { 
			return Catalog.GetString("Selection of the ") + this.limit + 
				Catalog.GetString(" MAX values of flight time ") + inJump + bySubjumps + inSession + "."; 
		}
	}
}


//"POTENCY (Aguado) 9.81^2*TV*TT / (4*jumps*(TT-TV))")
public class StatPotencyAguado : Stat
{
	
	public StatPotencyAguado (Gtk.TreeView treeview, int sessionUniqueID, string sessionName, int newPrefsDigitsNumber, bool sexSeparated, bool max, int limit) 
	{
		completeConstruction (treeview, sessionUniqueID, sessionName, newPrefsDigitsNumber, sexSeparated);
		this.jumpType = jumpType;
		this.limit = limit;
		if (max) {
			this.operation = "MAX";
		} else {
			this.operation = "AVG";
		}
	}

	protected override TreeStore getStore ()
	{
		//position, jumper (sex), index, tv, tt, jumps
		TreeStore myStore = new TreeStore(typeof (string), typeof (string), typeof (string), typeof(string), typeof (string), typeof(string));
		return myStore;
	}
	
	protected override void createTreeView_stats () {
		treeview.HeadersVisible=true;
		int count =0;

		treeview.AppendColumn ( Catalog.GetString("Position"), new CellRendererText(), "text", count++);
		treeview.AppendColumn ( Catalog.GetString("Jumper"), new CellRendererText(), "text", count++); //person (sex)
		treeview.AppendColumn ( Catalog.GetString("Potency"), new CellRendererText(), "text", count++);
		treeview.AppendColumn ( "Total TV", new CellRendererText(), "text", count++);
		treeview.AppendColumn ( "Total Time", new CellRendererText(), "text", count++);
		treeview.AppendColumn ( Catalog.GetString("Jumps"), new CellRendererText(), "text", count++);
	}

	public override void prepareData () 
	{
		ArrayList myArray; 
		bool index = true;
		if (limit > 0) {
			//classify by jumps
			myArray = Sqlite.StatRjPotencyAguadoJumps(sessionUniqueID, sexSeparated, limit);
		} else {
			//classify by jumpers
			myArray = Sqlite.StatOneJumpJumpersRjPotencyAguado(sessionUniqueID, sexSeparated, operation);
		}

		int secondCount = 0;
		bool firstGirl = true;
		string myTv = "";
		string [] stringFullResults;
		double totalTv = 0;
		double sumPotency = 0;
		double sumTotalTv = 0;
		double sumTotalTime = 0;
		double sumJumps = 0;
		double sumSquaredPotency = 0;
		double sumSquaredTotalTv = 0;
		double sumSquaredTotalTime = 0;
		double sumSquaredJumps = 0;
		
		int i=0;
		
		for (i=0 ; i < myArray.Count ; i ++) {
			stringFullResults = myArray[i].ToString().Split(new char[] {':'});

			totalTv =  Convert.ToDouble(stringFullResults[2]) * //number of jumps 
									//(double because if we are searching AVG, can return double values
				Convert.ToDouble(stringFullResults[4]); //tvAvg

			if(firstGirl && sexSeparated && stringFullResults[1] == "F") {
				secondCount = i;
				firstGirl = false;
			}

			printData ( (i-secondCount+1).ToString() , 
					stringFullResults[0] + "(" + stringFullResults[1] + ")", 
					trimDecimals (stringFullResults[5]), //potency
					trimDecimals ( totalTv.ToString() ), //total tv 
					trimDecimals (stringFullResults[3]), //total time
					trimDecimals (stringFullResults[2]) //number of jumps
					);

			sumPotency += Convert.ToDouble(stringFullResults[5]);
			sumTotalTv += Convert.ToDouble(totalTv.ToString());
			sumTotalTime += Convert.ToDouble(stringFullResults[3]);
			sumJumps += Convert.ToDouble(stringFullResults[2]);
			sumSquaredPotency += makeSquare(stringFullResults[5]);
			sumSquaredTotalTv += makeSquare(totalTv.ToString());
			sumSquaredTotalTime += makeSquare(stringFullResults[3]);
			sumSquaredJumps += makeSquare(stringFullResults[2]);
		}

		printData ( "", "", "", "", "", "");
		printData ( "AVG"  , "", trimDecimals((sumPotency/i).ToString()), 
				trimDecimals((sumTotalTv/i).ToString()), trimDecimals((sumTotalTime/i).ToString()), 
				trimDecimals((sumJumps/i).ToString()) );
		printData ( "SD"  , "", 
				trimDecimals(calculateSD(sumPotency, sumSquaredPotency, i)),
				trimDecimals(calculateSD(sumTotalTv, sumSquaredTotalTv, i)),
				trimDecimals(calculateSD(sumTotalTime, sumSquaredTotalTime, i)),
				trimDecimals(calculateSD(sumJumps, sumSquaredJumps, i)) );
	}


	protected void printData (string row, string person, string potency, string totalTv, string totalTime, string jumpsNumber) 
	{
			iter = store.AppendValues (row, person, potency, totalTv, totalTime, jumpsNumber); 
	}
	
	
	public override string ToString () 
	{
		string indexString = "POTENCY (Aguado) 9.81^2*TV*TT / (4*jumps*(TT-TV))";
			
		string operationString = "";
		if ( this.operation == "MAX" ) { 
			operationString = Catalog.GetString("by MAX values of index: ") + indexString ; 
		}
		else { operationString = Catalog.GetString("by AVG values of index: ") + indexString ; }

		string sexSeparatedString = "";
		if (this.sexSeparated) { sexSeparatedString = Catalog.GetString("sorted by sex"); }

		string inJump = Catalog.GetString(" in ") + jumpType + Catalog.GetString(" jump ");
		string inSession = Catalog.GetString(" in '") + sessionName + Catalog.GetString("' session ");
		
		if ( this.limit == 0 ) { 
			return Catalog.GetString("Rank of jumpers ") + operationString + inJump + inSession + sexSeparatedString + "."; 
		}
		else { 
			return Catalog.GetString("Selection of the ") + this.limit + 
				Catalog.GetString(" MAX values of flight time ") + inJump + inSession + "."; 
		}
	}
}

public class StatIE : Stat
{
	protected string jump1Name;
	protected string jump2Name;
	protected string indexName;
	
	protected string indexNameString;
	protected string indexValueString;
	
	
	//if this is not present i have problems like (No overload for method `xxx' takes `0' arguments) with some inherited classes
	public StatIE () 
	{
		this.sessionName = "";
		this.sexSeparated = false;
		this.operation = "MAX";
	}

	public StatIE (Gtk.TreeView treeview, int sessionUniqueID, string sessionName, int newPrefsDigitsNumber, bool sexSeparated, bool max) 
	{
		completeConstruction (treeview, sessionUniqueID, sessionName, newPrefsDigitsNumber, sexSeparated);
	
		jump1Name = "SJ";
		jump2Name = "CMJ";
		indexName = "IE";
		indexNameString =  Catalog.GetString ("Elasticity Index");
		indexValueString = "100*(CMJ-SJ)/SJ" ;
		if (max) {
			this.operation = "MAX";
		} else {
			this.operation = "AVG";
		}
	}

	protected override TreeStore getStore ()
	{
		//statCount, person (sex), IE, sj tv, cmj tv
		TreeStore myStore = new TreeStore(typeof (string), typeof (string), typeof(string), typeof (string), typeof(string));
		return myStore;
	}
	
	protected override void createTreeView_stats () {
		treeview.HeadersVisible=true;
		int count =0;

		treeview.AppendColumn ( Catalog.GetString("Position"), new CellRendererText(), "text", count++);
		treeview.AppendColumn ( Catalog.GetString("Jumper"), new CellRendererText(), "text", count++); //person (sex)
		treeview.AppendColumn ( Catalog.GetString("Index %"), new CellRendererText(), "text", count++);
		treeview.AppendColumn ("SJ TV", new CellRendererText(), "text", count++);
		treeview.AppendColumn ("CMJ TV", new CellRendererText(), "text", count++);
	}
	
	public override void prepareData () 
	{
		ArrayList myArray = Sqlite.StatClassificationIeIub(sessionUniqueID, indexName, operation, sexSeparated);

		int secondCount = 0;
		bool firstGirl = true;
		string myTv = "";
		string [] stringFullResults;
		double sumIndex = 0;
		double sumJump1 = 0;
		double sumJump2 = 0;
		double sumSquaredIndex = 0;
		double sumSquaredJump1 = 0;
		double sumSquaredJump2 = 0;
		int i=0;
		
		for (i=0 ; i < myArray.Count ; i ++) {
			stringFullResults = myArray[i].ToString().Split(new char[] {':'});

			if(firstGirl && sexSeparated && stringFullResults[4] == "F") {
				secondCount = i;
				firstGirl = false;
			}

			printData ( (i-secondCount+1).ToString() , 
					stringFullResults[0] + "(" + stringFullResults[4] + ")", //person (sex)
					trimDecimals (stringFullResults[1]), //index
					trimDecimals (stringFullResults[2]), //jump1
					trimDecimals (stringFullResults[3]) //jump2
					);

			sumIndex += Convert.ToDouble(stringFullResults[1]);
			sumJump1 += Convert.ToDouble(stringFullResults[2]);
			sumJump2 += Convert.ToDouble(stringFullResults[3]);
			sumSquaredIndex += makeSquare(stringFullResults[1]);
			sumSquaredJump1 += makeSquare(stringFullResults[2]);
			sumSquaredJump2 += makeSquare(stringFullResults[3]);
		}
		
		printData ( "", "", "", "", "");
		printData ( "AVG"  , "", trimDecimals((sumIndex/i).ToString()), 
				trimDecimals((sumJump1/i).ToString()), trimDecimals((sumJump2/i).ToString()) );
		printData ( "SD"  , "", 
				trimDecimals(calculateSD(sumIndex, sumSquaredIndex, i)),
				trimDecimals(calculateSD(sumJump1, sumSquaredJump1, i)),
				trimDecimals(calculateSD(sumJump2, sumSquaredJump2, i)) );
	}

	
	protected void printData (string row, string person, string index, string sj, string cmj) 
	{
			iter = store.AppendValues (row, person, index, sj, cmj); 
	}

	public override string ToString () 
	{
		string operationString = "";
		if ( this.operation == "MAX" ) { 
			operationString = Catalog.GetString("by MAX values of index '") + indexNameString + "': " + indexValueString;
		}
		else { operationString = Catalog.GetString("by AVG values of index '") + indexNameString + "': " + indexValueString ; }

		string sexSeparatedString = "";
		if (this.sexSeparated) { sexSeparatedString = Catalog.GetString("sorted by sex"); }
	
		string inSession = Catalog.GetString(" in '") + sessionName + Catalog.GetString("' session ");

		if ( this.limit == 0 ) { 
			return Catalog.GetString("Rank of jumpers") + operationString + inSession + sexSeparatedString + "."; 
		}
		else { 
			return Catalog.GetString("Selection of the ") + this.limit + 
				Catalog.GetString(" MAX values of index '") + indexNameString + "': " + indexValueString + inSession + "."; 
		}
	}
}


public class StatIUB : StatIE
{
	
	//if this is not present i have problems like (No overload for method `xxx' takes `0' arguments) with some inherited classes
	//public StatIUB () 
	//{
	//	this.sessionName = "unnamed";
		//this.sessionName = "";
		//this.sexSeparated = false;
		//this.operation = "MAX";
	//}

	
	public StatIUB (Gtk.TreeView treeview, int sessionUniqueID, string sessionName, int newPrefsDigitsNumber, bool sexSeparated, bool max) 
	{
		completeConstruction (treeview, sessionUniqueID, sessionName, newPrefsDigitsNumber, sexSeparated);
		
		jump1Name = "CMJ";
		jump2Name = "ABK";
		indexName = "IUB";
		indexNameString = Catalog.GetString("Arms using Index");
		indexValueString = "100*(ABK-CMJ)/CMJ" ;
		if (max) {
			this.operation = "MAX";
		} else {
			this.operation = "AVG";
		}
	}

	//i tried to inherit this with no result, i had problems with the columns names
	protected override void createTreeView_stats () {
		treeview.HeadersVisible=true;
		int count =0;

		treeview.AppendColumn ( Catalog.GetString("Position"), new CellRendererText(), "text", count++);
		treeview.AppendColumn ( Catalog.GetString("Jumper"), new CellRendererText(), "text", count++);
		treeview.AppendColumn ( Catalog.GetString("Index %"), new CellRendererText(), "text", count++);
		treeview.AppendColumn ("CMJ TV", new CellRendererText(), "text", count++);
		treeview.AppendColumn ("ABK TV", new CellRendererText(), "text", count++);
	}
}

public class StatGlobal : Stat
{
	protected int personID;
	protected string personName;
	//protected ArrayList sessions;
	
	//if this is not present i have problems like (No overload for method `xxx' takes `0' arguments) with some inherited classes
	/*
	public StatGlobal () 
	{
		this.sessionName = "";
		this.sexSeparated = false;
		this.operation = "MAX";
	}
	*/

	//public StatGlobal (Gtk.TreeView treeview, int sessionUniqueID, string sessionName, int personID, string personName, int newPrefsDigitsNumber, bool sexSeparated, bool max) 
	public StatGlobal (Gtk.TreeView treeview, ArrayList sessions, int personID, string personName, int newPrefsDigitsNumber, bool sexSeparated, bool max) 
	{
		completeConstruction2 (treeview, sessions, personID, personName, newPrefsDigitsNumber, sexSeparated);
		this.jumpType = jumpType;
		this.sexSeparated = sexSeparated;
		if (max) {
			this.operation = "MAX";
		} else {
			this.operation = "AVG";
		}
	}

	protected void completeConstruction2 (Gtk.TreeView treeview, ArrayList sessions, int personID, string personName, int newPrefsDigitsNumber, bool sexSeparated)
	{
		//this.sessionUniqueID = sessionUniqueID;
		
		//FIXME: change this value, it only serves for knowing it there's a previous stat and removing it
		this.sessionName = "global multi";
		
		
		this.treeview = treeview;
		prefsDigitsNumber = newPrefsDigitsNumber;
		this.sexSeparated = sexSeparated;
		this.personID = personID;
		this.personName = personName;
		this.sessions = sessions;

		//store = getStore2(sessions.Count +1);
		if(sessions.Count > 1) {
			store = getStore2(sessions.Count +3); //+3 (for the statName, the AVG horizontal and SD horizontal
		} else {
			store = getStore2(sessions.Count +1);
		}
		treeview.Model = store;

		iter = new TreeIter();
	}

	protected TreeStore getStore2 (int columns)
	{
		//prepares the TreeStore for de required columns
		Type [] types = new Type [columns];
		for (int i=0; i < columns; i++) {
			types[i] = typeof (string);
		}
		TreeStore myStore = new TreeStore(types);
		return myStore;
	}
	
	protected string obtainSessionSqlString(ArrayList sessions)
	{
		string newStr = "WHERE (";
		for (int i=0; i < sessions.Count; i++) {
			string [] stringFullResults = sessions[i].ToString().Split(new char[] {':'});
			newStr = newStr + " sessionID == " + stringFullResults[0];
			if (i+1 < sessions.Count) {
				newStr = newStr + " OR ";
			}
		}
		newStr = newStr + ") ";
		return newStr;		
	}
	
	public override void prepareData() 
	{
		treeview.HeadersVisible=true;
		treeview.AppendColumn (Catalog.GetString("Jump"), new CellRendererText(), "text", 0);

		string headerString = "";
		string [] stringFullResults;
		
		for (int i=0; i < sessions.Count ; i++) {
			//we need to know the name of the column: session
			stringFullResults = sessions[i].ToString().Split(new char[] {':'});
			headerString = stringFullResults[1] + "\n" + stringFullResults[2]; //name, date
			//Console.WriteLine("headerString: {0}", headerString);
			treeview.AppendColumn (Catalog.GetString(headerString), new CellRendererText(), "text", i+1); 
		}
		if(sessions.Count > 1) {
			treeview.AppendColumn (Catalog.GetString("AVG"), new CellRendererText(), "text", sessions.Count +1); 
			treeview.AppendColumn (Catalog.GetString("SD"), new CellRendererText(), "text", sessions.Count +2); 
		}

		string sessionString = obtainSessionSqlString(sessions);
				
		//last value: number of sessions
		processData ( Sqlite.StatGlobalNormal(sessionString, operation, sexSeparated, personID), 
				true, sessions.Count );
		processData ( Sqlite.StatGlobalOthers("DjIndex", "(100*((tv-tc)/tc))", "jump", "DJ", 
					sessionString, operation, sexSeparated, personID),
				false, sessions.Count );
		processData ( Sqlite.StatGlobalOthers("RjIndex", "(100*((tvavg-tcavg)/tcavg))", "jumpRj", "RJ", 
					sessionString, operation, sexSeparated, personID),
				false, sessions.Count );
		processData ( Sqlite.StatGlobalOthers("RjPotency", 
					"(9.81*9.81 * tvavg*jumps * time / (4*jumps*(time - tvavg*jumps)) )", "jumpRj", "RJ", 
					sessionString, operation, sexSeparated, personID),
				false, sessions.Count );
	}
	
	//prepared for multisession
	public void processData (ArrayList arrayFromSql, bool makeAVGSD, int sessionsNum) 
	{
		string [] rowFromSql = new string [sessionsNum +1];
		double [] sumValue = new double [sessionsNum +1];
		double [] sumSquaredValue = new double [sessionsNum +1];
		string [] sendRow = new string [sessionsNum +1];
		int [] countRows = new int [sessions.Count +1]; //count the number of valid cells (rows) for make the AVG
		int i;
		
		//initialize values
		for(int j=1; j< sessionsNum+1 ; j++) {
			sendRow[j] = "-";
			sumValue[j] = 0;
			sumSquaredValue[j] = 0;
			countRows[j] = 0;
		}
		string oldStat = "-1";
	
		//process all SQL results line x line
		for (i=0 ; i < arrayFromSql.Count ; i ++) {
			rowFromSql = arrayFromSql[i].ToString().Split(new char[] {':'});

			if (rowFromSql[0] != oldStat) {
				//print the values, except on the first iteration
				if (i>0) {
					printData( calculateRowAVGSD(sendRow) );
				}
				
				//process another stat
				sendRow[0] = rowFromSql[0]; //first value to send (the name of stat)
				for(int j=1; j< sessionsNum+1 ; j++) {
					sendRow[j] = "-";
				}
			}

			for (int j=0; j < sessions.Count ; j++) {
				string [] str = sessions[j].ToString().Split(new char[] {':'});
				if(rowFromSql[1] == str[0]) { //if matches the session num
					sendRow[j+1] = trimDecimals(rowFromSql[2]); //put value from sql in the desired pos of sendRow

					if(makeAVGSD) {
						sumValue[j+1] += Convert.ToDouble(rowFromSql[2]);
						sumSquaredValue[j+1] += makeSquare(rowFromSql[2]);
						countRows[j+1] ++;
					}
				}
			}
			oldStat = sendRow[0];
		}
					
		printData( calculateRowAVGSD(sendRow) );

		if(makeAVGSD) {
			//printData accepts two cols: name, values (values separated again by ':')
			string [] sendAVG = new string [sessions.Count +1];
			string [] sendSD = new string [sessions.Count +1];
			
			sendAVG[0] = Catalog.GetString("AVG");
			sendSD[0] =  Catalog.GetString("SD");
			
			for (int j=1; j <= sessions.Count; j++) {
				if(countRows[j] > 0) {
					sendAVG[j] = trimDecimals( (sumValue[j] /countRows[j]).ToString() );
					if(countRows[j] > 1) {
						sendSD[j] = trimDecimals( calculateSD(sumValue[j], sumSquaredValue[j], countRows[j]) );
					} else {
						sendSD[j] = "-";
					}
					//Console.WriteLine("sumValue: {0}, sumSquaredValue: {1}, countRows[j]: {2}, j: {3}", 
					//		sumValue[j], sumSquaredValue[j], countRows[j], j);
				} else {
					sendAVG[j] = "-";
					sendSD[j] = "-";
				}
			}
			printData( calculateRowAVGSD(sendAVG) );
			printData( calculateRowAVGSD(sendSD) );
		}
			
	}

	//returns a row with it's AVG and SD in last two columns
	protected string [] calculateRowAVGSD(string [] rowData) 
	{
		string [] rowReturn = new String[sessions.Count +3];
		int count =0;
		double sumValue = 0;
		double sumSquaredValue = 0;
	
		if(sessions.Count > 1) {
			int i=0;
			for (i=0; i < sessions.Count + 1; i++) {
				rowReturn[i] = rowData[i];
				if(i>0 && rowReturn[i] != "-") { //first column is text
					count++;
					sumValue += Convert.ToDouble(rowReturn[i]); 
					sumSquaredValue += makeSquare(rowReturn[i]); 
				}
			}
			if(count > 0) {
				rowReturn[i] = trimDecimals( (sumValue /count).ToString() );
				if(count > 1) {
					rowReturn[i+1] = trimDecimals( calculateSD(sumValue, sumSquaredValue, count) );
				} else {
					rowReturn[i+1] = "-";
				}
			} else {
				rowReturn[i] = "-";
				rowReturn[i+1] = "-";
			}
					
			return rowReturn;
		} else {
			return rowData;
		}
	}
		
	protected void printData (string [] statValues) 
	{
			iter = store.AppendValues (statValues); 
	}

	public override string ToString () 
	{
		string personString = "";
		if (personID != -1) {
			personString = Catalog.GetString(" of '") + personName + Catalog.GetString("' jumper");
		}
			
		string sexSeparatedString = "";
		if (this.sexSeparated) { sexSeparatedString = " " + Catalog.GetString("sorted by sex"); }
		
		string inSessions = Catalog.GetString(" in sessions: ");
		for (int i=0; i < sessions.Count ;i++) {
			string [] str = sessions[i].ToString().Split(new char[] {':'});
			inSessions = inSessions + "'" + str[1] + "' (" + str[2] + ")";
			if(i + 1 < sessions.Count) {
				inSessions = inSessions + ", ";
			}
		}
	
		if ( this.operation == "MAX" ) { 
			return Catalog.GetString("MAX values of some jumps and statistics") + personString + inSessions + sexSeparatedString + "."  ; 
		} else {
			return Catalog.GetString("AVG values of some jumps and statistics") + personString + inSessions + sexSeparatedString + "."  ; 
		}
	}
	
}


