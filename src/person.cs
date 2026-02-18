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
 *  Copyright (C) 2004-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Collections.Generic; //List
using Mono.Unix;


public class Person
{
	private int uniqueID;
	private string name;
	private string sex; // "-" (Unspecified), "M" (male) , "F" (female) (Constants.SexU, Constants.SexM, Constants.SexF)
	private DateTime dateBorn;
	private int race;
	private int countryID;
	private string description;
	private string future1; 	//rfid
	private string future2; 	//club ID, is an integer
	private int serverUniqueID; //not on server
	private string linkServerImage;
	private string nameFirst;
	private string nameLast;

	private string muuid; //machineID at the DB where is created (32 bits) ; uuid (32 bits).
	//It helps to know where a person was created.
	//This has been created when mobile app is being created and persons be moved between different mobiles and computers

	public Person() {
	}
	
	//used when we create a new person, then uniqueID is -1
	public Person(int uniqueID) {
		this.uniqueID = uniqueID;
	}

	//coming from compujump server, networks guest & RemotePerson
	public Person(bool insertPerson, int uniqueID, string name, string rfid, string image) //TODO:, string clubID)
	{
		this.uniqueID = uniqueID;
		this.name = name;
		this.sex = Constants.SexU;
		this.dateBorn = DateTime.Now;
		this.race = Constants.RaceUndefinedID;
		this.countryID = Constants.CountryUndefinedID; //1
		this.description = "";
		this.future1 = rfid;
		//TODO: this.future2 = clubID;
		this.serverUniqueID = Constants.ServerUndefinedID;
		this.linkServerImage = image;
		this.nameFirst = "";
		this.nameLast = "";
		this.muuid = "";

		/*
		 * Before insertion check that uniqueID exists locally
		 * can happen when there are rfid changes on server
		 */
		if(insertPerson)
			SqlitePerson.Insert(false,
					uniqueID.ToString(), name, sex, dateBorn, race, countryID,
					description, future1, future2, serverUniqueID, linkServerImage,
					nameFirst, nameLast, muuid);
	}

	//suitable when we load a person from the database for being the current Person
	//we know uniqueID
	//used also in class PersonSessionTransaction where we define the uniqueID 
	public Person(int uniqueID, string name, string sex, DateTime dateBorn, 
			int race, int countryID, string description,
			string future1, string future2, int serverUniqueID, string linkServerImage,
			string nameFirst, string nameLast, string muuid)
	{
		//needed by the return of gui/personAddModifyWindow
		name = Util.RemoveTildeAndColon(name);
		description = Util.RemoveTildeAndColon(description);

		this.uniqueID = uniqueID;
		this.sex = sex;
		this.name = name;
		this.dateBorn = dateBorn;
		this.race = race;
		this.countryID = countryID;
		this.description = description;
		this.future1 = future1;
		this.future2 = future2;
		this.serverUniqueID = serverUniqueID; //remember don't do this on server
		this.linkServerImage = linkServerImage;
		this.nameFirst = nameFirst;
		this.nameLast = nameLast;
		this.muuid = muuid;
	}

	//typical constructor
	//used when we create new person 
	//we don't know uniqueID
	public Person (string name, string sex, DateTime dateBorn, int race, int countryID, string description,
			string future1, string future2, int serverUniqueID, string linkServerImage,
			string nameFirst, string nameLast, string muuid, bool dbconOpened)
	{
		name = Util.RemoveTildeAndColon(name);
		description = Util.RemoveTildeAndColon(description);
		
		this.name = name;
		this.sex = sex;
		this.dateBorn = dateBorn;
		this.race = race;
		this.countryID = countryID;
		this.description = description;
		this.future1 = future1;
		this.future2 = future2;
		this.serverUniqueID = serverUniqueID; //remember don't do this on server
		this.linkServerImage = linkServerImage;
		this.nameFirst = nameFirst;
		this.nameLast = nameLast;
		this.muuid = muuid;

		//insert in the person table
		//when insert as person we don't know uniqueID
		uniqueID = -1;
		int insertedID = this.InsertAtDB(dbconOpened, Constants.PersonTable);

		//we need uniqueID for personSession
		uniqueID = insertedID;

		LogB.Information(this.ToString());
	}

	public int InsertAtDB (bool dbconOpened, string tableName) {
		int myID = SqlitePerson.Insert(dbconOpened,  
				uniqueID.ToString(), name, sex, dateBorn, race, countryID,
				description, future1, future2, serverUniqueID, linkServerImage, nameFirst, nameLast, muuid);
		return myID;
	}

	public string IDAndName (string sep) {
		return uniqueID.ToString() + sep + name;
	}

	public override string ToString()
	{
		return "[uniqueID: " + uniqueID + "]" + name + ", " + ", " + sex + ", " + dateBorn.ToShortDateString() + ", " + description;
	}
	
	public string ToSQLInsertString()
	{
		return uniqueID.ToString() + ", '"  + name + "', '" + sex + "', '" + 
			UtilDate.ToDateSQL(dateBorn) + "', " + race + ", " + countryID + ", '" +
			description + "', '" + future1 + "', '" + future2 + "', " +
			serverUniqueID + ", '" + linkServerImage + "', '" +
			nameFirst + "', '" + nameLast + "', '" + muuid + "'";
	}

	public static string ExportHeader (char sep)
	{
		return string.Format ("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}",
				"ID", sep, "MUUID_m", sep, "MUUID_id", sep,
				"NameFirst", sep, "NameLast", sep, "Sex");
	}
	public string Export (char sep)
	{
		string muuidFix = muuid;
		if (sep == ';')
			muuidFix = muuid;
		return string.Format ("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}",
				uniqueID, sep, getMuuidMachine (), sep, getMuuidId (), sep,
				nameFirst, sep, nameLast, sep, sex);
	}
	

	public override bool Equals(object evalString)
	{
		return this.ToString() == evalString.ToString();
	}
	
	public override int GetHashCode()
	{
		return this.ToString().GetHashCode();
	}

	//personToMerge will be merged with currentPerson
	public List<ClassVariance.Struct> MergeWithAnotherGetConflicts (Person personToMerge)
	{
		List<ClassVariance> v_l = this.DetailedCompare (
				personToMerge, ClassCompare.Visibility.PUBLICANDPRIVATE);

		List<ClassVariance.Struct> propDiff_l = new List<ClassVariance.Struct> ();
		if (v_l.Count > 0)
		{
			LogB.Information ("Differences found between persons:");
			foreach (ClassVariance v in v_l)
			{
				//LogB.Information (v.ToString()); //debug
				//don't add the uniqueID, Obviously it is different
				if (v.Prop != "uniqueID")
					propDiff_l.Add (v.GetStruct ());
			}
		}

		return propDiff_l;
	}
	
	//some "set"s are needed. If not data of personSession does not arrive to the server
	
	public string Sex {
		get { return sex; } 
		set { sex = value; }
	}
	
	public DateTime DateBorn {
		get { return dateBorn; }
		set { dateBorn = value; }
	}
	
	public int Race {
		get { return race; }
		set { race = value; }
	}

	public int CountryID {
		get { return countryID; }
		set { countryID = value; }
	}

	public string Description {
		get { return description; }
		set { description = value; }
	}

	//rfid
	public string Future1 {
		get { return future1; }
		set { future1 = value; }
	}

	//clubID
	public string ClubID {
		get { return future2; }
		set { future2 = value; }
	}

	public int ServerUniqueID {
		get { return serverUniqueID; }
		set { serverUniqueID = value; }
	}

	public string LinkServerImage {
		get { return linkServerImage; }
	}


	/*
	 * Until 2.5.3, name is not separated in first and last, name contains everything.
	 *
	 * Continue using name variable (it means fullname) to not have to change lots of code
	 *
	 * On the migration nameFirst is name, and nameLast is ""
	 *
	 * When nameFirst or nameLast is updated, on SQL name it is also updated. So there are 3 columns.
	 * This solves problems and has much easier SQL queries.
	 *
	 * First we thought on having Name autogenerated from nameFirst and nameLast SQL columns,
	 * but this forces to do things like:
	 * coalesce(person77.name, '') || ' ' || coalesce(person77.nameLast, '') AS person_fullname
	 * and this will return:
	 * |Dídac Black |  	this person has the firstName: "Dídac Black", and lastName ""
	 * |Joan Guiu|		this person hast the firstName: "Joan", and lastName "Guiu"
	 * Note the unwanted space after Dídac Black
	 */
	public string Name {
		get { return name; }
		set { name = value; }
	}

	// used op updating Name from widgets
	public static string GetNameFromFirstAndLast (string nameFirst, string nameLast)
	{
		if (nameFirst != "" && nameLast != "")
			return nameFirst + " " + nameLast;
		else if (nameFirst == "" && nameLast == "")
			return "";
		else if (nameLast == "")
			return nameFirst;
		else //if (nameFirst == "")
			return nameLast;
	}

	public static string CreateMuuidFromMachineID (string machineID)
	{
		Random rnd = new Random();
		return string.Format ("{0};{1}", machineID, rnd.NextInt64()); //this will generate a machineID between 0 and 9223372036854775807 (Int64.MaxValue)
	}

	// gets the first part of muuid
	private string getMuuidMachine ()
	{
		string [] sFull = muuid.Split(new char[] {';'});
		if (sFull.Length != 2)
			return muuid;

		return sFull[0];
	}
	// gets the second part of muuid
	private string getMuuidId ()
	{
		string [] sFull = muuid.Split(new char[] {';'});
		if (sFull.Length != 2)
			return muuid;

		return sFull[1];
	}

	public string NameFirst {
		get { return nameFirst; }
		set { nameFirst = value; }
	}
	// last name
	public string NameLast {
		get { return nameLast; }
		set { nameLast = value; }
	}

	/* using name, Name (for historical reasons)
	 * so this is not used
	public string FullName {
		get { return name + " " + nameLast; }
	}
	*/

	public string Muuid {
		get { return muuid; }
		set { muuid = value; }
	}

	public int UniqueID {
		get { return uniqueID; }
		set { uniqueID = value; }
	}

	public string DateLong {
		get { return dateBorn.ToLongDateString(); }
	}
	
	public string DateShort {
		get { return dateBorn.ToShortDateString(); }
	}
	
	
	~Person() {}
	   
}

public class PersonsExport
{
	private string destination;
	private char colDelim;
	private List<Person> person_l;
	private enum doneEnumType { NOPERSONS, CANNOTCOPY, SUCCESS };
	private doneEnumType doneEnum;

	// constructor
	public PersonsExport (int sessionID, string destination, char colDelim)
	{
		this.destination = destination;
		this.colDelim = colDelim;

		person_l = SqlitePersonSession.SelectCurrentSessionPersonsAsList (false, sessionID);
	}

	public bool Do ()
	{
		if (person_l.Count == 0)
		{
			doneEnum = doneEnumType.NOPERSONS;
			return false;
		}

		TextWriter writer;
		try {
			writer = File.CreateText(destination);
		} catch {
			LogB.Information("Couldn't create file: " + destination);
			doneEnum = doneEnumType.CANNOTCOPY;
			return false;
		}

		writer.WriteLine (Person.ExportHeader (colDelim));
		foreach (Person p in person_l)
			writer.WriteLine (p.Export (colDelim));

		writer.Close ();

		doneEnum = doneEnumType.SUCCESS;
		return true;
	}

	public string DoneMessage ()
	{
		if (doneEnum == doneEnumType.NOPERSONS)
			return Catalog.GetString ("No persons to export.");
		else if (doneEnum == doneEnumType.CANNOTCOPY)
			return string.Format (Catalog.GetString ("Cannot export to file {0} "), destination);
		else if (doneEnum == doneEnumType.SUCCESS)
			return Catalog.GetString ("Exported to:");

		return "";
	}

	public string DoneOkURL {
		get { return destination; }
	}
}

//useful when you just want to know all of the data of a person in this session
public class PersonAndPS
{
	public Person p;
	public PersonSession ps;
	
	//default constructor
	public PersonAndPS(Person p, PersonSession ps) {
		this.p = p;
		this.ps = ps;
	}

	public override string ToString ()
	{
		return string.Format ("Person: {0};\nPersonSession: {1}", p, ps);
	}

	~PersonAndPS() {}
}
public static class PersonAndPSUtil
{
	public static int Find(ArrayList papsArray, int personID) 
	{
		int count = 0;
		foreach(PersonAndPS paps in papsArray) {
			if(paps.p.UniqueID == personID)
				return count;
			count ++;
		}

		return -1;
	}

	// just to debug
	public static void CompareAtImportPrintStr (ArrayList papsDB_a, ArrayList papsIS_a)
	{
		//this will be faster if ArrayList are sorted by name (to lower), and when that name passed, continue

		string conflictsStr = "";
		foreach (PersonAndPS papsIS in papsIS_a)
			foreach(PersonAndPS papsDB in papsDB_a)
				if (papsIS.p.Name.ToLower () == papsDB.p.Name.ToLower ())
					conflictsStr += "\n" + string.Format ("{0} - {1}; {2}; {3}",
							papsIS.p.Name,
							papsDB.p.Name, papsDB.p.UniqueID, papsDB.ps.SessionID);

		if (conflictsStr != "")
			LogB.Information ("Possible conflicts previous to import:" +
					"\nPerson name on importing session - person name on DB, ID on DB, session on DB" +
					conflictsStr);
		else
			LogB.Information ("No name conflicts previous to import");
	}

	// papsDB: currentDB; papsIS: Importing Session
	// TODO: need to send here a List<session> to be able to have lot more content on sessions (not only id)
	public static List<PersonImportConflict> CompareAtImport (ArrayList papsDB_a, ArrayList papsIS_a, List<Session> sessionsDB_l)
	{
		//this will be faster if ArrayList are sorted by name (to lower), and when that name passed, continue
		List<PersonImportConflict> pic_l = new List<PersonImportConflict> ();

		foreach (PersonAndPS papsIS in papsIS_a)
		{
			PersonImportConflict pic = null;
			foreach(PersonAndPS papsDB in papsDB_a)
				if (papsIS.p.Name.ToLower () == papsDB.p.Name.ToLower ())
				{
					Session session = null;
					foreach (Session s in sessionsDB_l)
						if (papsDB.ps.SessionID == s.UniqueID)
						{
							session = s;
							break;
						}

					if (pic == null)
						pic = new PersonImportConflict (papsIS.p.Name, papsDB.p.UniqueID, session);
					else
						pic.AddSession (session);
				}
			if (pic != null)
				pic_l.Add (pic);
		}

		return pic_l;
	}

}

public class PersonImportConflict
{
	private string nameImporting;
	private int idAtLocalDB;

	private List<Session> sessionsAtLocalDB_l;

	public PersonImportConflict (string nameImporting, int idAtLocalDB, Session sessionAtLocalDB)
	{
		this.nameImporting = nameImporting;
		this.idAtLocalDB = idAtLocalDB;

		sessionsAtLocalDB_l = new List<Session> ();
		sessionsAtLocalDB_l.Add (sessionAtLocalDB);
	}

	public void AddSession (Session sessionAtLocalDB)
	{
		sessionsAtLocalDB_l.Add (sessionAtLocalDB);
	}

	public override string ToString ()
	{
		string str = string.Format("nameImporting: {0}, idAtDb: {1}, at sessions:",
				nameImporting, idAtLocalDB);
		foreach (Session s in sessionsAtLocalDB_l)
				str += "\n" + s.ToString ();

		return str;
	}

	public string NameImporting {
		get { return nameImporting; }
	}

	public string SessionsAtLocalDB {
		get {
			string str = "";
			string sep = "";
			foreach (Session s in sessionsAtLocalDB_l)
			{
				str += sep + string.Format ("[{0}] {1}", s.DateShort, s.Name);
				sep = "\n";
			}

			return str;
		}
	}
}
