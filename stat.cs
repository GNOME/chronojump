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
 *	StatIE
 *		StatIUB
 * 	StatGlobal
 * 		//StatGlobalInter
 *	//StatPersonInter
 *
 * ---------------------------------------
 */


public class Stat
{
	protected int sessionUniqueID;
	protected string sessionName;
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

	public string SessionName
	{
		get { 
			return sessionName;
		}
	}

	~Stat() {}

}


public class StatSjCmjAbk : Stat
{
	protected static Gtk.TreeViewColumn col0;
	protected static Gtk.TreeViewColumn col1;
	protected static Gtk.TreeViewColumn col2;
	
	//static GettextResourceManager catalog = new GettextResourceManager("chronojump");

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

		//col0 = treeview.AppendColumn (catalog.GetString("Pos."), new CellRendererText(), "text", count++);
		col0 = treeview.AppendColumn ( Catalog.GetString("Position"), new CellRendererText(), "text", count++);
		col1 = treeview.AppendColumn ( Catalog.GetString("Jumper"), new CellRendererText(), "text", count++); //person(sex)
		col2 = treeview.AppendColumn ( "TV", new CellRendererText(), "text", count++);
	}

	public override void RemoveHeaders() {
		treeview.RemoveColumn (col0);
		treeview.RemoveColumn (col1);
		treeview.RemoveColumn (col2);
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
		
		//Console.WriteLine("sumTv: {0}, totalNum: {1}, avgTv: {2}", sumTv, i , sumTv/i);
		//Console.WriteLine("sumSquaredTv: {0}", sumSquaredTv);
		//Console.WriteLine("square root of sumSquaredTv: {0}", System.Math.Sqrt(sumSquaredTv));
		printData ( ""  , "AVG", trimDecimals((sumTv/i).ToString()) );
		printData ( ""  , "SD", trimDecimals(
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
	protected static Gtk.TreeViewColumn col0;
	protected static Gtk.TreeViewColumn col1;
	protected static Gtk.TreeViewColumn col2;
	protected static Gtk.TreeViewColumn col3;
	protected static Gtk.TreeViewColumn col4;
	
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

		col0 = treeview.AppendColumn ( Catalog.GetString("Position"), new CellRendererText(), "text", count++);
		col1 = treeview.AppendColumn ( Catalog.GetString("Jumper"), new CellRendererText(), "text", count++); //person(sex)
		col2 = treeview.AppendColumn ( "TV", new CellRendererText(), "text", count++);
		col3 = treeview.AppendColumn ( "TC", new CellRendererText(), "text", count++);
		col4 = treeview.AppendColumn ( Catalog.GetString("Fall"), new CellRendererText(), "text", count++);
	}
	
	public override void RemoveHeaders() {
		treeview.RemoveColumn (col0);
		treeview.RemoveColumn (col1);
		treeview.RemoveColumn (col2);
		treeview.RemoveColumn (col3);
		treeview.RemoveColumn (col4);
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
		
		//Console.WriteLine("sumTv: {0}, totalNum: {1}, avgTv: {2}", sumTv, i , sumTv/i);
		printData ( ""  , "AVG", trimDecimals((sumTv/i).ToString()), 
				trimDecimals((sumTc/i).ToString()), trimDecimals((sumFall/i).ToString()) );
		printData ( ""  , "SD", 
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
	protected static Gtk.TreeViewColumn col0;
	protected static Gtk.TreeViewColumn col1;
	protected static Gtk.TreeViewColumn col2;
	protected static Gtk.TreeViewColumn col3;
	protected static Gtk.TreeViewColumn col4;
	protected static Gtk.TreeViewColumn col5;
	
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

		col0 = treeview.AppendColumn ( Catalog.GetString("Position"), new CellRendererText(), "text", count++);
		col1 = treeview.AppendColumn ( Catalog.GetString("Jumper"), new CellRendererText(), "text", count++); //person (sex)
		col2 = treeview.AppendColumn ( Catalog.GetString("Index %"), new CellRendererText(), "text", count++);
		col3 = treeview.AppendColumn ( "TV", new CellRendererText(), "text", count++);
		col4 = treeview.AppendColumn ( "TC", new CellRendererText(), "text", count++);
		col5 = treeview.AppendColumn ( Catalog.GetString("Fall"), new CellRendererText(), "text", count++);
	}

	public override void RemoveHeaders() {
		treeview.RemoveColumn (col0);
		treeview.RemoveColumn (col1);
		treeview.RemoveColumn (col2);
		treeview.RemoveColumn (col3);
		treeview.RemoveColumn (col4);
		treeview.RemoveColumn (col5);
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
		
		printData ( ""  , "AVG", trimDecimals((sumIndex/i).ToString()), 
				trimDecimals((sumTv/i).ToString()), trimDecimals((sumTc/i).ToString()), 
				trimDecimals((sumFall/i).ToString()) );
		printData ( ""  , "SD", 
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

		col0 = treeview.AppendColumn ( Catalog.GetString("Position"), new CellRendererText(), "text", count++);
		col1 = treeview.AppendColumn ( Catalog.GetString("Jumper"), new CellRendererText(), "text", count++); //person (sex)
		col2 = treeview.AppendColumn ( Catalog.GetString("Index %"), new CellRendererText(), "text", count++);
		col3 = treeview.AppendColumn ( Catalog.GetString("TV (AVG)"), new CellRendererText(), "text", count++);
		col4 = treeview.AppendColumn ( Catalog.GetString("TC (AVG)"), new CellRendererText(), "text", count++);
		col5 = treeview.AppendColumn ( Catalog.GetString("Fall"), new CellRendererText(), "text", count++);
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
		
		printData ( ""  , "AVG", trimDecimals((sumIndex/i).ToString()), 
				trimDecimals((sumTv/i).ToString()), trimDecimals((sumTc/i).ToString()), 
				trimDecimals((sumFall/i).ToString()) );
		printData ( ""  , "SD", 
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

public class StatIE : Stat
{
	protected string jump1Name;
	protected string jump2Name;
	protected string indexName;
	
	protected string indexNameString;
	protected string indexValueString;
	
	protected static Gtk.TreeViewColumn col0;
	protected static Gtk.TreeViewColumn col1;
	protected static Gtk.TreeViewColumn col2;
	protected static Gtk.TreeViewColumn col3;
	protected static Gtk.TreeViewColumn col4;

	
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

		col0 = treeview.AppendColumn ( Catalog.GetString("Position"), new CellRendererText(), "text", count++);
		col1 = treeview.AppendColumn ( Catalog.GetString("Jumper"), new CellRendererText(), "text", count++); //person (sex)
		col2 = treeview.AppendColumn ( Catalog.GetString("Index %"), new CellRendererText(), "text", count++);
		col3 = treeview.AppendColumn ("SJ TV", new CellRendererText(), "text", count++);
		col4 = treeview.AppendColumn ("CMJ TV", new CellRendererText(), "text", count++);
	}
	
	public override void RemoveHeaders() {
		treeview.RemoveColumn (col0);
		treeview.RemoveColumn (col1);
		treeview.RemoveColumn (col2);
		treeview.RemoveColumn (col3);
		treeview.RemoveColumn (col4);
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
		
		printData ( ""  , "AVG", trimDecimals((sumIndex/i).ToString()), 
				trimDecimals((sumJump1/i).ToString()), trimDecimals((sumJump2/i).ToString()) );
		printData ( ""  , "SD", 
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

		col0 = treeview.AppendColumn ( Catalog.GetString("Position"), new CellRendererText(), "text", count++);
		col1 = treeview.AppendColumn ( Catalog.GetString("Jumper"), new CellRendererText(), "text", count++);
		col2 = treeview.AppendColumn ( Catalog.GetString("Index %"), new CellRendererText(), "text", count++);
		col3 = treeview.AppendColumn ("CMJ TV", new CellRendererText(), "text", count++);
		col4 = treeview.AppendColumn ("ABK TV", new CellRendererText(), "text", count++);
	}
}

public class StatGlobal : Stat
{
	protected static Gtk.TreeViewColumn col0;
	protected static Gtk.TreeViewColumn col1;
	
	//if this is not present i have problems like (No overload for method `xxx' takes `0' arguments) with some inherited classes
	public StatGlobal () 
	{
		this.sessionName = "";
		this.sexSeparated = false;
		this.operation = "MAX";
	}

	public StatGlobal (Gtk.TreeView treeview, int sessionUniqueID, string sessionName, int newPrefsDigitsNumber, bool sexSeparated, bool max) 
	{
		completeConstruction (treeview, sessionUniqueID, sessionName, newPrefsDigitsNumber, sexSeparated);
		this.jumpType = jumpType;
		this.sexSeparated = sexSeparated;
		if (max) {
			this.operation = "MAX";
		} else {
			this.operation = "AVG";
		}
	}

	protected override TreeStore getStore ()
	{
		//statName, value
		TreeStore myStore = new TreeStore(typeof (string), typeof (string));
		return myStore;
	}
	
	protected override void createTreeView_stats () {
		treeview.HeadersVisible=true;
		int count =0;

		col0 = treeview.AppendColumn (Catalog.GetString("Jump"), new CellRendererText(), "text", count++);
		col1 = treeview.AppendColumn (Catalog.GetString("Value"), new CellRendererText(), "text", count++); 
	}

	public override void RemoveHeaders() {
		treeview.RemoveColumn (col0);
		treeview.RemoveColumn (col1);
	}

	public override void prepareData () 
	{
		ArrayList myArray = Sqlite.StatGlobalNormal(sessionUniqueID, operation, sexSeparated);
		ArrayList myArrayDj = Sqlite.StatGlobalDj(sessionUniqueID, operation, sexSeparated);
		ArrayList myArrayRj = Sqlite.StatGlobalRj(sessionUniqueID, operation, sexSeparated);
	
		string [] stringFullResults;
		string rowName = "";
		double sumValue = 0;
		double sumSquaredValue = 0;
		int i=0;
		
		for (i=0 ; i < myArray.Count ; i ++) {
			stringFullResults = myArray[i].ToString().Split(new char[] {':'});

			if(sexSeparated) {
				rowName = stringFullResults[0] + "(" + stringFullResults[2] + ")";
			} else {
				rowName = stringFullResults[0];
			}
			 
			printData ( rowName, trimDecimals (stringFullResults[1]) );
			sumValue += Convert.ToDouble(stringFullResults[1]);
			sumSquaredValue += makeSquare(stringFullResults[1]);
		}
		
		//AVG and SD here, because is not fine to compare
		//jumps with values like 0.5 with indexes
		//with values like 70
		printData ( "AVG", trimDecimals((sumValue/i).ToString()) );		
		printData ( "SD", trimDecimals(calculateSD(sumValue, sumSquaredValue, i)) );

		//print the DJ if exists
		if(myArrayDj.Count > 0) {
			if(sexSeparated) {
				stringFullResults = myArrayDj[0].ToString().Split(new char[] {':'});
				printData ( "DJ Index (" + stringFullResults[1] + ")", trimDecimals (stringFullResults[0]) );
				if(myArrayDj.Count > 1) {
					stringFullResults = myArrayDj[1].ToString().Split(new char[] {':'});
					printData ( "DJ Index (" + stringFullResults[1] + ")", trimDecimals (stringFullResults[0]) );
				}
			} else {
				stringFullResults = myArrayDj[0].ToString().Split(new char[] {':'});
				printData ( "DJ Index", trimDecimals (stringFullResults[0]) );
			}
		}
		
		//print the RJ if exists
		if(myArrayRj.Count > 0) {
			if(sexSeparated) {
				stringFullResults = myArrayRj[0].ToString().Split(new char[] {':'});
				printData ( "RJ Index (" + stringFullResults[1] + ")", trimDecimals (stringFullResults[0]) );
				if(myArrayDj.Count > 1) {
					stringFullResults = myArrayRj[1].ToString().Split(new char[] {':'});
					printData ( "RJ Index (" + stringFullResults[1] + ")", trimDecimals (stringFullResults[0]) );
				}
			} else {
				stringFullResults = myArrayRj[0].ToString().Split(new char[] {':'});
				printData ( "RJ Index", trimDecimals (stringFullResults[0]) );
			}
		}
				
	}

	protected void printData (string statName, string statValue) 
	{
			iter = store.AppendValues (statName, statValue); 
	}

	public override string ToString () 
	{
		string sexSeparatedString = "";
		if (this.sexSeparated) { sexSeparatedString = Catalog.GetString("sorted by sex"); }
		
		string inSession = Catalog.GetString(" in '") + sessionName + Catalog.GetString("' session ");
	
		if ( this.operation == "MAX" ) { 
			return Catalog.GetString("MAX values of some jumps and statistics") + inSession + sexSeparatedString + "."  ; 
		} else {
			return Catalog.GetString("AVG values of some jumps and statistics") + inSession + sexSeparatedString + "."  ; 
		}
	}
}


