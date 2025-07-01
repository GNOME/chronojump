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
using System.Data;
using System.IO;
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using Mono.Unix;
#if MICROSOFT_DATA_SQLITE
using Microsoft.Data.Sqlite;
using SQLiteTransaction = Microsoft.Data.Sqlite.SqliteTransaction;
using SQLiteCommand = Microsoft.Data.Sqlite.SqliteCommand;
using SQLiteDataReader = Microsoft.Data.Sqlite.SqliteDataReader;
using SQLiteConnection = Microsoft.Data.Sqlite.SqliteConnection;
#else
using System.Data.SQLite;
using SQLiteTransaction = System.Data.SQLite.SQLiteTransaction;
using SQLiteCommand = System.Data.SQLite.SQLiteCommand;
using SQLiteDataReader = System.Data.SQLite.SQLiteDataReader;
using SQLiteConnection = System.Data.SQLite.SQLiteConnection;
#endif

class SqliteEncoder : SqliteTests
{
    private static string tableStatic = Constants.EncoderTable;
    private static int columns = 25;

    public SqliteEncoder()
    {
	    tableName = Constants.EncoderTable;
    }

    ~SqliteEncoder() { }

    /*
	 * create and initialize tables
	 */

    protected internal static void createTableEncoder()
    {
        dbcmd.CommandText =
            "CREATE TABLE " + Constants.EncoderTable + " ( " +
            "uniqueID INTEGER PRIMARY KEY, " +
            "personID INT, " +
            "sessionID INT, " +
            "exerciseID INT, " +
            "eccon TEXT, " +    //"c" or "ec"
            "laterality TEXT, " +   //"RL" "R" "L". stored in english
            "extraWeight TEXT, " +  //string
            "signalOrCurve TEXT, " + //"signal" or "curve"
            "filename TEXT, " +
            "url TEXT, " +      //URL of data of signals and curves. stored as relative
            "time INT, " +
            "minHeight INT, " +
            "description TEXT, " +
            "status TEXT, " +   //"active", "inactive"
            "videoURL TEXT, " + //URL of video of signals. stored as relative
            "encoderConfiguration TEXT, " + //text separated by ':'
                "future1 TEXT, " +  //Since 1.4.4 (DB 1.06) this stores last meanPower detected on a curve 
                                    //(as string with '.' because future1 was created as TEXT)
            "future2 TEXT, " +  //same as future1 but for meanSpeed
            "future3 TEXT, " +  //same as future1 but for meanForce
            "repCriteria TEXT, " +   //criteria of meanPower, meanSpeed, meanForce: ecc_con, ecc, con
	    "hasInertia INT NOT NULL DEFAULT 0, " +
	    "maxPower FLOAT, " +
	    "maxSpeed FLOAT, " +
	    "maxForce FLOAT, " +
	    "rangeAbs )";
        dbcmd.ExecuteNonQuery();
    }

    /*
	 * Encoder class methods
	 */

    public static int Insert(bool dbconOpened, EncoderSQL es)
    {
        if (!dbconOpened)
            Sqlite.Open();

	string uniqueIDStr = "NULL";
	if (es.UniqueID != -1)
		uniqueIDStr = es.UniqueID.ToString();

        dbcmd.CommandText = "INSERT INTO " + Constants.EncoderTable +
            " (uniqueID, personID, sessionID, exerciseID, eccon, laterality, extraWeight, " +
            "signalOrCurve, filename, url, time, minHeight, description, status, " +
            "videoURL, encoderConfiguration, future1, future2, future3, repCriteria, " +
	    "hasInertia, maxPower, maxSpeed, maxForce, rangeAbs)" +
            " VALUES (" + uniqueIDStr + ", " +
            es.PersonID + ", " + es.SessionID + ", " +
            es.exerciseID + ", '" + es.eccon + "', '" +
            es.LateralityToEnglish() + "', '" + Util.ConvertToPoint(es.extraWeight) + "', '" +
            es.signalOrCurve + "', '" + es.filename + "', '" +
            Util.MakeURLrelative(es.url) + "', " +
            es.time + ", " + es.minHeight + ", '" + es.Description +
            "', '" + es.status + "', '" +
            Util.MakeURLrelative(es.videoURL) + "', '" +
            es.encoderConfiguration.ToStringOutput(EncoderConfiguration.Outputs.SQL) + "', '" +
            Util.ConvertToPoint(es.meanPower) + "', '" + Util.ConvertToPoint(es.meanSpeed) + "', '" + Util.ConvertToPoint(es.meanForce) + "', '" +
            es.repCriteria.ToString() + "', " +
	    Util.BoolToInt (es.hasInertia) + ", " +
            Util.ConvertToPoint(es.maxPower) + ", " + Util.ConvertToPoint(es.maxSpeed) + ", " + Util.ConvertToPoint(es.maxForce) + ", " +
            Util.ConvertToPoint(es.rangeAbs) + ")";

        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();

        //int myLast = dbcon.LastInsertRowId;
        //http://stackoverflow.com/questions/4341178/getting-the-last-insert-id-with-sqlite-net-in-c
        string myString = @"select last_insert_rowid()";
        dbcmd.CommandText = myString;
        int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

        if (!dbconOpened)
            Sqlite.Close();

        return myLast;
    }

    //normal Update call dbcmd will be used	
    public static void Update(bool dbconOpened, EncoderSQL es)
    {
        update(dbconOpened, es, dbcmd);
    }
    //Transaction Update call dbcmdTr will be used	
    public static void Update(bool dbconOpened, EncoderSQL es, SQLiteCommand dbcmdTr)
    {
        update(dbconOpened, es, dbcmdTr);
    }
    private static void update(bool dbconOpened, EncoderSQL es, SQLiteCommand mycmd)
    {
        if (!dbconOpened)
            Sqlite.Open();

	string uniqueIDStr = "NULL";
	if (es.UniqueID != -1)
		uniqueIDStr = es.UniqueID.ToString();

        mycmd.CommandText = "UPDATE " + Constants.EncoderTable + " SET " +
                " personID = " + es.PersonID +
                ", sessionID = " + es.SessionID +
                ", exerciseID = " + es.exerciseID +
                ", eccon = '" + es.eccon +
                "', laterality = '" + es.LateralityToEnglish() +
                "', extraWeight = '" + Util.ConvertToPoint(es.extraWeight) +
                "', signalOrCurve = '" + es.signalOrCurve +
                "', filename = '" + es.filename +
                "', url = '" + Util.MakeURLrelative(es.url) +
                "', time = " + es.time +
                ", minHeight = " + es.minHeight +
                ", description = '" + es.Description +
                "', status = '" + es.status +
                "', videoURL = '" + Util.MakeURLrelative(es.videoURL) +
                "', encoderConfiguration = '" + es.encoderConfiguration.ToStringOutput(EncoderConfiguration.Outputs.SQL) +
                "', future1 = '" + Util.ConvertToPoint(es.meanPower) +
                "', future2 = '" + Util.ConvertToPoint(es.meanSpeed) +
                "', future3 = '" + Util.ConvertToPoint(es.meanForce) +
                "', repCriteria = '" + es.repCriteria.ToString() +
		"', hasInertia = " + Util.BoolToInt (es.hasInertia) +
		", maxPower = " + Util.ConvertToPoint (es.maxPower) +
		", maxSpeed = " + Util.ConvertToPoint (es.maxSpeed) +
		", maxForce = " + Util.ConvertToPoint (es.maxForce) +
		", rangeAbs = " + Util.ConvertToPoint (es.rangeAbs) +
                " WHERE uniqueID = " + uniqueIDStr;

        LogB.SQL(mycmd.CommandText.ToString());
        mycmd.ExecuteNonQuery();

        if (!dbconOpened)
            Sqlite.Close();
    }

    // used on encoder to update related curves
    protected override void updateSpecific (int signalID, int personID)
    {
	    ArrayList array = SqliteEncoderSignalCurve.SelectSignalCurve (true, signalID, -1, -1, -1);
	    foreach (EncoderSignalCurve esc in array)
	    {
		    dbcmd.CommandText = "UPDATE " + tableName +
			    " SET personID = " + personID +
			    " WHERE uniqueID = " + esc.curveID;

		    LogB.SQL(dbcmd.CommandText.ToString());
		    dbcmd.ExecuteNonQuery();
	    }
    }

    // on encoder comments is named: description
    public override void UpdateComments (int uniqueID, string comments)
    {
	    Sqlite.Open();
	    dbcmd.CommandText = "UPDATE " + tableName +
		    " SET description = '" + comments + "'" +
		    " WHERE uniqueID = " + uniqueID ;

	    LogB.SQL(dbcmd.CommandText.ToString());
	    dbcmd.ExecuteNonQuery();
	    Sqlite.Close();
    }

    public static int UpdateTransaction(ArrayList data, string[] checkboxes)
    {
        int count = 0;
        int countActive = 0;

        LogB.SQL("Starting transaction");
        Sqlite.Open();

        using (SQLiteTransaction tr = dbcon.BeginTransaction())
        {
            using (SQLiteCommand dbcmdTr = dbcon.CreateCommand())
            {
                dbcmdTr.Transaction = tr;

                foreach (EncoderSQL eSQL in data)
                {
                    if (count < checkboxes.Length && eSQL.status != checkboxes[count])
                    {
                        eSQL.status = checkboxes[count];

                        SqliteEncoder.Update(true, eSQL, dbcmdTr);
                    }

                    count++;

                    if (eSQL.status == "active")
                        countActive++;
                }
            }
            tr.Commit();
        }

        Sqlite.Close();
        LogB.SQL("Ended transaction");
        return countActive;
    }

    /*
	   SqliteEncoder.Select
	   pass uniqueID value and then will return one record. do like this:
	   EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(false, myUniqueID, 0, 0, 0, "", EncoderSQL.Eccons.ALL, false, true)[0];

	   WARNING because SqliteEncoder.Select may not return nothing, and then cannot be assigned to eSQL
	   see: delete_encoder_curve(bool dbconOpened, int uniqueID)
		and: manageCurvesOfThisSignal

	   don't care for the 0, 0, 0  because selection will be based on the myUniqueID and only one row will be returned
	   or
	   pass uniqueID==-1 and personID, sessionID, signalOrCurve values, and will return some records
	   personID can be -1 to get all on that session
	   sessionID can be -1 to get all sessions
	   exerciseID can be -1 to get all exercises
	   signalOrCurve can be "all"

	   orderIDascendent is good for all the situations except when we want to convert from 1.05 to 1.06
	   in that conversion, we want first the last ones, and later the previous
	   (to delete them if they are old copies)

	   orderRepsByPosInSet uses encoderSignalCurve. encoder reps uniqueIDs are not correctly ordered by set,
	   eg if you save only the best (maybe the 4th), will have uniqueID 1, and then if you save it all,
	   then they will be saved as 2, 3, (4 not saved becuase it is already one), 4, 5, ... So 4th in order will be 1
	   orderRepsByPosInSet fixes this problem. this is used eg. in analyze session to sort them correctly
	   but note it Select will only work ok for curves
	 */
    // default, returns an ArrayList
    public static ArrayList Select (
		    bool dbconOpened, int uniqueID, int personID, int sessionID, Constants.EncoderGI encoderGI,
		    int exerciseID, string signalOrCurve, EncoderSQL.Eccons ecconSelect, string lateralityEnglish,
		    bool onlyActive, bool orderIDascendent,
		    bool orderRepsByPosInSet) // Attention! note this only selects curves
    {
	    openIfNeeded (dbconOpened);

	    Sqlite.Orders_by orderBy = Sqlite.Orders_by.ID_DESC;
	    if (orderIDascendent)
		    orderBy = Sqlite.Orders_by.ID_ASC;

	    selectDo (dbconOpened, uniqueID, personID, sessionID, encoderGI,
			    exerciseID, signalOrCurve, ecconSelect, lateralityEnglish,
			    onlyActive, orderBy,
			    orderRepsByPosInSet); // Attention! note this only selects curves

	    SQLiteDataReader reader;
	    reader = dbcmd.ExecuteReader();

	    ArrayList array = new ArrayList(1);
	    while (reader.Read())
	    {
		    EncoderSQL eSQL = getEncoderSQL (reader, encoderGI);
		    if (eSQL != null)
			    array.Add(eSQL);
	    }

	    reader.Close();
	    closeIfNeeded (dbconOpened);

	    return array;
    }

    // returns a List<EncoderSQL>
    // includes also:
    // limit 0 means no limit (limit negative is the last results)
    // personNameInComment
    public static List<EncoderSQL> SelectList (
		    bool dbconOpened, int uniqueID, int personID, int sessionID, Constants.EncoderGI encoderGI,
		    int exerciseID, string signalOrCurve, EncoderSQL.Eccons ecconSelect, string lateralityEnglish,
		    bool onlyActive,
		    Orders_by order,
		    bool orderRepsByPosInSet, 	// Attention! note this only selects curves
		    int limit, bool personNameInComment)
    {
	    openIfNeeded (dbconOpened);

	    //for personNameInComment
	    List<Person> person_l =
		    SqlitePersonSession.SelectCurrentSessionPersonsAsList (true, sessionID);

	    selectDo (dbconOpened, uniqueID, personID, sessionID, encoderGI,
			    exerciseID, signalOrCurve, ecconSelect, lateralityEnglish,
			    onlyActive, order,
			    orderRepsByPosInSet); // Attention! note this only selects curves

	    SQLiteDataReader reader;
	    reader = dbcmd.ExecuteReader();

	    List<EncoderSQL> eSQL_l = new List<EncoderSQL> ();
	    while (reader.Read())
	    {
		    EncoderSQL eSQL = getEncoderSQL (reader, encoderGI);

		    if (eSQL != null)
		    {
			    if (personNameInComment)
				    foreach (Person person in person_l)
					    if (person.UniqueID == eSQL.PersonID)
						    eSQL.Description = person.Name;

			    eSQL_l.Add (eSQL);
		    }
	    }

	    reader.Close();
	    closeIfNeeded (dbconOpened);

	    //get last values on negative limit
	    if (limit < 0 && eSQL_l.Count + limit >= 0)
		    eSQL_l = eSQL_l.GetRange (eSQL_l.Count + limit, -1 * limit);

	    return eSQL_l;
    }

    private static void selectDo (
		    bool dbconOpened, int uniqueID, int personID, int sessionID, Constants.EncoderGI encoderGI,
		    int exerciseID, string signalOrCurve, EncoderSQL.Eccons ecconSelect, string lateralityEnglish,
		    bool onlyActive, Orders_by order,
		    bool orderRepsByPosInSet) // Attention! note this only selects curves
    {

        string encT = Constants.EncoderTable;
        string encSCT = Constants.EncoderSignalCurveTable;
        string encExT = Constants.EncoderExerciseTable;

	// on best is best repetitition, do not need to group them by sets and all the related complexity
	if (order == Orders_by.BEST)
		orderRepsByPosInSet = false;
	//order == Orders_by.BEST2 is weight and then orderRepsByPosInSet is true

        string andString = "";
        string personIDStr = "";
        if (personID != -1)
        {
            personIDStr = " personID = " + personID;
            andString = " AND ";
        }

        string sessionIDStr = "";
        if (sessionID != -1)
        {
            sessionIDStr = andString + " sessionID = " + sessionID;
            andString = " AND ";
        }

        string exerciseIDStr = "";
        if (exerciseID != -1)
        {
            exerciseIDStr = andString + " exerciseID = " + exerciseID;
            andString = " AND ";
        }

        string lateralityEnglishStr = "";
        if (lateralityEnglish != "")
        {
            lateralityEnglishStr = andString + " laterality = '" + lateralityEnglish + "'";
            andString = " AND ";
        }

        string selectStr = "";
        if (uniqueID != -1)
            selectStr = encT + ".uniqueID = " + uniqueID;
        else
        {
            if (signalOrCurve == "all")
                selectStr = personIDStr + sessionIDStr + exerciseIDStr + lateralityEnglishStr;
            else
                selectStr = personIDStr + sessionIDStr + exerciseIDStr + lateralityEnglishStr + andString + " signalOrCurve = '" + signalOrCurve + "'";

            if (ecconSelect != EncoderSQL.Eccons.ALL)
                selectStr += andString + encT + ".eccon = '" + EncoderSQL.Eccons.ecS.ToString() + "'";
        }

        string fromString = " FROM " + encT + ", " + encExT;
        if (orderRepsByPosInSet)
            fromString += ", " + encSCT;

        //ensure andString is defined if selectStr is != "" (bug on 2.1.2 release)
        if (selectStr != "")
            andString = " AND ";

        string onlyActiveString = "";
        if (onlyActive)
        {
            onlyActiveString = andString + encT + ".status = 'active' ";
            andString = " AND ";
        }

        string orderRepsByPosInSetAndStr = "";
        if (orderRepsByPosInSet)
        {
            orderRepsByPosInSetAndStr = andString + encT + ".uniqueID = " +
                encSCT + ".curveID ";
            //andString = " AND ";
        }

        string orderRepsByPosInSetOrderStr = "";
        if (orderRepsByPosInSet)
            orderRepsByPosInSetOrderStr = encSCT + ".mscentral, ";

	string orderByStr = "";
	if (order == Orders_by.BEST)
		orderByStr = string.Format ( " ORDER BY {0}.future1 ", tableStatic); // meanPower
	else if (order == Orders_by.BEST2) //weight (and on the same weight, order by each set
		orderByStr = string.Format ( " ORDER BY {0}.extraWeight, " +
				"substr(filename,-23,19), " + //'filename,-23,19' has the date of capture signal
				orderRepsByPosInSetOrderStr +
				"uniqueID ", tableStatic);
	else {
		orderByStr =
			" ORDER BY substr(filename,-23,19), " + //'filename,-23,19' has the date of capture signal
			orderRepsByPosInSetOrderStr +
			"uniqueID ";
		if (order == Orders_by.ID_DESC)
			orderByStr += " DESC";
	}

        dbcmd.CommandText = "SELECT " +
            encT + ".*, " + encExT + ".name " +
            fromString +
            " WHERE " + selectStr +
	    andString + encT + ".exerciseID = " +
	    encExT + ".uniqueID " +
	    onlyActiveString + orderRepsByPosInSetAndStr +
	    orderByStr;

        LogB.SQL(dbcmd.CommandText.ToString());
    }

    private static EncoderSQL getEncoderSQL (SQLiteDataReader reader, Constants.EncoderGI encoderGI)
    {
	    // TODO: in the future use hasInertia (see SessionTestsByPerson) ---->
            string[] strFull = reader[15].ToString().Split(new char[] { ':' });
            EncoderConfiguration econf = new EncoderConfiguration(
                (EncoderConfiguration.Names)
                Enum.Parse(typeof(EncoderConfiguration.Names), strFull[0]));
            econf.ReadParamsFromSQL(strFull);

            //if encoderGI != ALL discard non wanted repetitions
            if (encoderGI == Constants.EncoderGI.GRAVITATORY && econf.has_inertia)
                return null;
            else if (encoderGI == Constants.EncoderGI.INERTIAL && !econf.has_inertia)
                return null;
	    // <----

            //if there's no video, will be "".
            //if there's video, will be with full path
            string videoURL = "";
            if (reader[14].ToString() != "")
                videoURL = Util.MakeURLabsolute(FixOSpath(reader[14].ToString()));

	    //LogB.SQL(econf.ToString(":", true));
	    EncoderSQL eSQL = new EncoderSQL (
			    Convert.ToInt32(reader[0].ToString()),  //uniqueID
			    Convert.ToInt32(reader[1].ToString()),  //personID
			    Convert.ToInt32(reader[2].ToString()),  //sessionID
			    Convert.ToInt32(reader[3].ToString()),  //exerciseID
			    reader[4].ToString(),           //eccon
			    Catalog.GetString(reader[5].ToString()),//laterality
			    Util.ChangeDecimalSeparator(reader[6].ToString()),  //extraWeight
			    reader[7].ToString(),           //signalOrCurve
			    reader[8].ToString(),           //filename
			    Util.MakeURLabsolute(FixOSpath(reader[9].ToString())),  //url
			    Convert.ToInt32(reader[10].ToString()), //time
			    Convert.ToInt32(reader[11].ToString()), //minHeight
			    reader[12].ToString(),          //description
			    reader[13].ToString(),          //status
			    videoURL,               //videoURL
			    econf,                  //encoderConfiguration
			    Util.ChangeDecimalSeparator(reader[16].ToString()), //future1 (meanPower on curves)
			    Util.ChangeDecimalSeparator(reader[17].ToString()), //future2 (meanSpeed on curves)
			    Util.ChangeDecimalSeparator(reader[18].ToString()), //future3 (meanForce on curves)
			    (Preferences.EncoderRepetitionCriteria)Enum.Parse(
				    typeof(Preferences.EncoderRepetitionCriteria), reader[19].ToString()),
			    Util.IntToBool (Convert.ToInt32 (reader[20].ToString())),  //hasInertia
			    Convert.ToDouble (Util.CDS (reader[21].ToString())), //maxPower
			    Convert.ToDouble (Util.CDS (reader[22].ToString())), //maxSpeed
			    Convert.ToDouble (Util.CDS (reader[23].ToString())), //maxForce
			    Convert.ToDouble (Util.CDS (reader[24].ToString())), //rangeAbs
			    reader[25].ToString()           //EncoderExercise.name
				    );

	    return eSQL;
    }

    //used on EncoderSelectRepetitionsIndividualAllSessions
    //exerciseID can be -1 to get all exercises
    public static ArrayList SelectCompareIntersession(bool dbconOpened, Constants.EncoderGI encoderGI,
            int exerciseID, string lateralityCode, int personID)
    {
        if (!dbconOpened)
            Sqlite.Open();

        string exerciseIDStr = "";
        if (exerciseID != -1)
            exerciseIDStr = "encoder.exerciseID = " + exerciseID + " AND ";

        string lateralityCodeStr = "";
        if (lateralityCode != "")
            lateralityCodeStr = "laterality = '" + lateralityCode + "' AND ";

        //returns a row for each session where there are active or inactive
        dbcmd.CommandText =
            "SELECT encoder.sessionID, session.name, session.date, encoder.extraWeight, " +
            " SUM(CASE WHEN encoder.status = 'active' THEN 1 END) as active, " +
            " SUM(CASE WHEN encoder.status = 'inactive' THEN 1 END) as inactive," +
            " encoder.encoderConfiguration " +
            " FROM encoder, session, person77 " +
            " WHERE " +
            exerciseIDStr + lateralityCodeStr +
            " encoder.personID = " + personID + " AND signalOrCurve = 'curve' AND " +
            " encoder.personID = person77.uniqueID AND encoder.sessionID = session.uniqueID " +
            " GROUP BY encoder.sessionID, encoder.extraWeight ORDER BY encoder.sessionID, encoder.extraWeight, encoder.status";

        LogB.SQL(dbcmd.CommandText.ToString());

        SQLiteDataReader reader;
        reader = dbcmd.ExecuteReader();

        ArrayList array = new ArrayList();
        EncoderPersonCurvesInDB encPS = new EncoderPersonCurvesInDB();
        /*
		 * eg.
		 * sessID|sess name|date|extraWe|a|i (a: active, i: inactive)
		 * 20|Encoder tests|2012-12-10|7|3|
		 * 20|Encoder tests|2012-12-10|0||9
		 * 20|Encoder tests|2012-12-10|10||34
		 * 20|Encoder tests|2012-12-10|58||1
		 * 20|Encoder tests|2012-12-10|61||1
		 * 26|sessio-proves|2013-07-08|10|5|36
		 * 30|proves encoder|2013-11-08|0|2|
		 * 30|proves encoder|2013-11-08|100|5|
		 * 
		 * convert to:
		 *
		 * sessID|sess name|date|a|i|reps*weights	(a: active, i: inactive)
		 * 20|Encoder tests|2012-12-10|3|45|3*7 9*0 34*10 1*58 1*61 (but sorted)
		 *
		 */
        int sessIDDoing = -1; //of this sessionID
        int sessIDThisRow = -1; //of this SQL row
        List<EncoderPersonCurvesInDBDeep> lDeep = new List<EncoderPersonCurvesInDBDeep>();
        bool firstSession = true;
        int activeThisRow;
        int inactiveThisRow;
        int activeThisSession = 0;
        int inactiveThisSession = 0;

        while (reader.Read())
        {
	    // TODO: in the future use hasInertia (see SessionTestsByPerson) ---->
            //discard if != encoderGI
            string[] strFull = reader[6].ToString().Split(new char[] { ':' });
            EncoderConfiguration econf = new EncoderConfiguration(
                (EncoderConfiguration.Names)
                Enum.Parse(typeof(EncoderConfiguration.Names), strFull[0]));

            //if encoderGI != ALL discard non wanted repetitions
            if (encoderGI == Constants.EncoderGI.GRAVITATORY && econf.has_inertia)
                continue;
            else if (encoderGI == Constants.EncoderGI.INERTIAL && !econf.has_inertia)
                continue;
	    // <----

            //1 get sessionID of this row
            sessIDThisRow = Convert.ToInt32(reader[0].ToString());

            //2 get active an inactive curves of this row
            activeThisRow = 0;
            string activeStr = reader[4].ToString();
            if (Util.IsNumber(activeStr, false))
                activeThisRow = Convert.ToInt32(activeStr);

            inactiveThisRow = 0;
            string inactiveStr = reader[5].ToString();
            if (Util.IsNumber(inactiveStr, false))
                inactiveThisRow = Convert.ToInt32(inactiveStr);

            //3 if session of this row is different than previous row
            if (sessIDThisRow != sessIDDoing)
            {
                sessIDDoing = sessIDThisRow;

                if (!firstSession)
                {
                    //if is not first session (means we have processed a session before)
                    //update encPS with the lDeep and then add to array
                    encPS.lDeep = lDeep;
                    encPS.countActive = activeThisSession;
                    encPS.countAll = activeThisSession + inactiveThisSession;
                    array.Add(encPS);
                }

                firstSession = false;

                //create new EncoderPersonCurvesInDB
                encPS = new EncoderPersonCurvesInDB(
                        personID,
                        Convert.ToInt32(reader[0].ToString()),  //sessionID
                        reader[1].ToString(),           //sessionName
                        reader[2].ToString());          //sessionDate

                activeThisSession = 0;
                inactiveThisSession = 0;
                //empty lDeep
                lDeep = new List<EncoderPersonCurvesInDBDeep>();
            }
            //4 add deep info: (weight, all reps)
            EncoderPersonCurvesInDBDeep deep = new EncoderPersonCurvesInDBDeep(
                    Convert.ToDouble(Util.ChangeDecimalSeparator(reader[3].ToString())), activeThisRow + inactiveThisRow);
            //add to lDeep
            lDeep.Add(deep);

            activeThisSession += activeThisRow;
            inactiveThisSession += inactiveThisRow;
        }

        //store last row in array (once we are out the while)
        if (!firstSession)
        {
            //if is not first session (means we have processed a session before)
            //update encPS with the lDeep and then add to array
            encPS.lDeep = lDeep;
            encPS.countActive = activeThisSession;
            encPS.countAll = activeThisSession + inactiveThisSession;
            array.Add(encPS);
        }

        reader.Close();
        if (!dbconOpened)
            Sqlite.Close();

        return array;
    }

    /*
     * used on encoder treeviewResultsSession
     * use this intead of the selectSSAray in order to have all the sets & reps linked
     * this will return a llist like this
     *
     * list with: { eSQL set of person 1 set 1, eSQL rep 1, eSQL rep 2, eSQL rep 3, ...}
     * list with: { eSQL set of person 1 set 2, eSQL rep 1, eSQL rep 2, eSQL rep 3, ...}
     * list with: { eSQL set of person 1 set 3, eSQL rep 1, eSQL rep 2, eSQL rep 3, ...}
     * list with: { eSQL set of person 2 set 1, eSQL rep 1, eSQL rep 2, eSQL rep 3, ...}
     * ...
     * ORDER BY here is very important to match all
     */
    public List<List<EncoderSQL>> SelectSetsAndRepsLList (
		    bool dbconOpened,
		    int personID, int sessionID, Constants.EncoderGI encoderGI, int exerciseID,
		    int signalID) //used after capture on treeviewResultsSession.Add () to get just that signal and repetitions in same format that Fill. On Fill just use -1 here
    {
        openIfNeeded (dbconOpened);

	List<List<EncoderSQL>> eSQL_ll = new List<List<EncoderSQL>> ();

	// 1 prepare the variables
	string tp = Constants.PersonTable;
        string encExT = Constants.EncoderExerciseTable;

	string filterPersonString = "";
	if(personID != -1)
		filterPersonString = string.Format(" AND {0}.uniqueID = {1}", tp, personID);

	string filterSessionString = "";
	if(sessionID != -1)
		filterSessionString = string.Format(" AND {0}.sessionID = {1}", tableName, sessionID);

        string filterExerciseString = "";
        if (exerciseID != -1)
            filterExerciseString = string.Format (" AND {0}.exerciseID = {1}", tableName, exerciseID);

	string filterSignalString = "";
	if (signalID >= 0)
		filterSignalString = string.Format (" AND {0}.uniqueID = {1}", tableName, signalID);

	// 1 select the sets
        dbcmd.CommandText = string.Format ("SELECT {0}.*, {1}.name, {2}.name ", tableName, encExT, tp) +
			string.Format(" FROM {0}, {1}, {2} ", tableName, encExT, tp) +
			string.Format(" WHERE {0}.uniqueID = {1}.personID", tp, tableName) +
			string.Format(" AND {0}.exerciseID = {1}.uniqueID", tableName, encExT) +
			filterPersonString + filterSessionString +
			filterExerciseString + filterSignalString +
			" AND signalOrCurve = 'signal' " +
			string.Format(" ORDER BY upper({0}.name), {1}.uniqueID ASC", tp, tableName);
	LogB.SQL(dbcmd.CommandText.ToString());

	dbcmd.ExecuteNonQuery();
	SQLiteDataReader reader;
	reader = dbcmd.ExecuteReader();

	List<int> signalID_l = new List<int> ();  //just to have operations faster on assign repetitions

	while (reader.Read())
	{
		EncoderSQL eSQL = getEncoderSQL (reader, encoderGI);
		if (eSQL == null)
			continue;

		eSQL.PersonNameSet = reader[(columns +1)].ToString ();

		List<EncoderSQL> eSQL_l = new List<EncoderSQL> (); // create eSQL_l list for this set
		eSQL_l.Add (eSQL); 				// add the set
		eSQL_ll.Add (eSQL_l);				// add the list to eSQL_ll
		signalID_l.Add (eSQL.UniqueID);
	}
        reader.Close();
	if (eSQL_ll.Count == 0)
		return eSQL_ll;

	/*
	// debug
	LogB.Information (string.Format ("List at end of 1, count: {0}", eSQL_ll.Count));
	foreach (List<EncoderSQL> eSQL_l in eSQL_ll)
		LogB.Information (((EncoderSQL) eSQL_l[0]).ToString ());
	foreach (int id in signalID_l)
		LogB.Information (id.ToString ());
	*/

	// 2 select the reps (getting also the EncoderSignalCurve.signalID to link with the sets
        string encSCT = Constants.EncoderSignalCurveTable;

	filterSignalString = "";
	if (signalID >= 0)
		filterSignalString = string.Format (" AND {0}.signalID = {1}", encSCT, signalID);

        dbcmd.CommandText = string.Format ("SELECT {0}.*, {1}.name, {2}.name, {3}.signalID, {3}.msCentral", tableName, encExT, tp, encSCT) +
			string.Format(" FROM {0}, {1}, {2}, {3} ", tableName, encExT, tp, encSCT) +
			string.Format(" WHERE {0}.uniqueID = {1}.personID", tp, tableName) +
			string.Format(" AND {0}.exerciseID = {1}.uniqueID", tableName, encExT) +
			string.Format(" AND {0}.uniqueID = {1}.curveID", tableName, encSCT) +
			filterPersonString + filterSessionString +
			filterExerciseString + filterSignalString +
			" AND signalOrCurve = 'curve' " +
			string.Format(" ORDER BY {0}.signalID ASC, {0}.msCentral ", encSCT);
	LogB.SQL(dbcmd.CommandText.ToString());

	dbcmd.ExecuteNonQuery();
	reader = dbcmd.ExecuteReader();

	EncoderSignalCurve escOld = new EncoderSignalCurve (-1, -1, -1, -1);
	while (reader.Read())
	{
		EncoderSQL eSQL = getEncoderSQL (reader, encoderGI);
		if (eSQL == null)
			continue;

		//LogB.Information (eSQL.ToString ());
		eSQL.PersonNameSet = reader[(columns +1)].ToString ();
		int signalIDofThisRep = Convert.ToInt32 (reader[(columns +2)].ToString ());

		// for some reason, some EncoderSignalCurve records are repeated on DB. Find why and fix. Meanwhile discard them here.
		EncoderSignalCurve esc = new EncoderSignalCurve (-1, signalIDofThisRep, eSQL.UniqueID,
				Convert.ToInt32 (reader[(columns +3)].ToString ()));
		if (esc.Equals (escOld))
			continue;

		for (int i = 0 ; i < signalID_l.Count ; i ++)
			if (signalIDofThisRep == signalID_l[i])
			{
				(eSQL_ll[i]).Add (eSQL);
				escOld = new EncoderSignalCurve (-1, esc.signalID, esc.curveID, esc.msCentral);
			}
	}
        reader.Close();

	closeIfNeeded (dbconOpened);

	return eSQL_ll;
    }

    public void TestSelectSetsAndRepsLList (bool dbconOpened,
		    int personID, int sessionID, Constants.EncoderGI encoderGI, int exerciseID, int signalID)
    {
	    List<List<EncoderSQL>> eSQL_ll = SelectSetsAndRepsLList (dbconOpened,
			    personID, sessionID, encoderGI, exerciseID, signalID);

	    int l0count = 0;
	    foreach (List<EncoderSQL> eSQL_l in eSQL_ll)
	    {
		    int l1count = 0;
		    foreach (EncoderSQL eSQL in eSQL_l)
		    {
			    LogB.Information (string.Format ("l0count: {0}, l1count: {1}, eSQL: {2}", l0count, l1count, eSQL));
			    l1count ++;
		    }
		    l0count ++;
	    }
    }

    public static EncoderSQL SelectData (int uniqueID, bool dbconOpened)
    {
	    EncoderSQL eSQL = new EncoderSQL (selectTestData (uniqueID, dbconOpened, tableStatic, columns));

	    //get also the exerciseName
	    ArrayList ex_array = SqliteEncoderExercise.SelectEncoderExercises (
			    dbconOpened, eSQL.exerciseID, true, Constants.EncoderGI.ALL);
	    if (ex_array.Count > 0)
		    eSQL.exerciseName = ((EncoderExercise) ex_array[0]).Name;

	    return eSQL;
    }

    public static ArrayList SelectSessionOverviewSets(bool dbconOpened, Constants.EncoderGI encoderGI, int sessionID)
    {
        if (!dbconOpened)
            Sqlite.Open();

        dbcmd.CommandText =
            "SELECT person77.uniqueID, person77.name, person77.sex, encoder.encoderConfiguration, encoderExercise.name, (personSession77.weight * encoderExercise.percentBodyWeight/100) + encoder.extraWeight, COUNT(*)" +
            " FROM person77, personSession77, encoderExercise, encoder" +
            " WHERE person77.uniqueID = encoder.personID AND personSession77.personID = encoder.personID AND personSession77.sessionID = encoder.sessionID AND encoderExercise.uniqueID=encoder.exerciseID AND signalOrCurve = 'signal' AND encoder.sessionID = " + sessionID +
            " GROUP BY encoder.personID, encoderConfiguration, exerciseID, extraWeight" +
            " ORDER BY person77.name";

        LogB.SQL(dbcmd.CommandText.ToString());

        SQLiteDataReader reader;
        reader = dbcmd.ExecuteReader();

        ArrayList array = new ArrayList();
        while (reader.Read())
        {
	    // TODO: in the future use hasInertia (see SessionTestsByPerson) ---->
            //discard if != encoderGI
            string[] strFull = reader[3].ToString().Split(new char[] { ':' });
            EncoderConfiguration econf = new EncoderConfiguration(
                (EncoderConfiguration.Names)
                Enum.Parse(typeof(EncoderConfiguration.Names), strFull[0]));

            //if encoderGI != ALL discard non wanted repetitions
            if (encoderGI == Constants.EncoderGI.GRAVITATORY && econf.has_inertia)
                continue;
            else if (encoderGI == Constants.EncoderGI.INERTIAL && !econf.has_inertia)
                continue;
	    // <----

            if (encoderGI == Constants.EncoderGI.GRAVITATORY)
            {
                string[] s = {
                    reader[0].ToString(), 	//person uniqueID
					reader[1].ToString(), 	//person name
					reader[2].ToString(), 	//person sex
					econf.ToStringPretty(), //encoder configuration
					reader[4].ToString(), 	//encoder exercise name
					reader[5].ToString(),	//displaced mass (includes percentBodyeight)
					reader[6].ToString()	//sets count
				};
                array.Add(s);
            }
            else
            {
                string[] s = {
                    reader[0].ToString(), 	//person uniqueID
					reader[1].ToString(), 	//person name
					reader[2].ToString(), 	//person sex
					econf.ToStringPretty(),	//encoder configuration
					reader[4].ToString(), 	//encoder exercise name
					reader[6].ToString()	//sets count
				};
                array.Add(s);
            }
        }

        reader.Close();
        if (!dbconOpened)
            Sqlite.Close();

        return array;
    }

    public static ArrayList SelectSessionOverviewReps(bool dbconOpened, Constants.EncoderGI encoderGI, int sessionID)
    {
        if (!dbconOpened)
            Sqlite.Open();

        dbcmd.CommandText =
            "SELECT person77.uniqueID, person77.name, person77.sex, encoder.encoderConfiguration, encoderExercise.name, " +
            "encoder.extraWeight, encoder.eccon, encoder.future1, encoder.future2, encoder.future3, encoder.repCriteria " +
            "FROM person77, encoderExercise, encoder " +
            "WHERE sessionID = " + sessionID.ToString() +
                " AND signalOrCurve = 'curve' " +
            " AND person77.uniqueID = encoder.personID " +
            " AND encoderExercise.uniqueID = encoder.exerciseID " +
            " ORDER BY person77.name";

        LogB.SQL(dbcmd.CommandText.ToString());

        SQLiteDataReader reader;
        reader = dbcmd.ExecuteReader();

        ArrayList array = new ArrayList();
        while (reader.Read())
        {
	    // TODO: in the future use hasInertia (see SessionTestsByPerson) ---->
            //discard if != encoderGI
            string[] strFull = reader[3].ToString().Split(new char[] { ':' });
            EncoderConfiguration econf = new EncoderConfiguration(
                (EncoderConfiguration.Names)
                Enum.Parse(typeof(EncoderConfiguration.Names), strFull[0]));

            //if encoderGI != ALL discard non wanted repetitions
            if (encoderGI == Constants.EncoderGI.GRAVITATORY && econf.has_inertia)
                continue;
            else if (encoderGI == Constants.EncoderGI.INERTIAL && !econf.has_inertia)
                continue;
	    // <----

            string repCriteria = "";
            if (reader[6].ToString() != "c")
            {
                if (reader[10].ToString() == Preferences.EncoderRepetitionCriteria.ECC_CON.ToString())
                    repCriteria = Catalog.GetString("Eccentric-concentric");
                else if (reader[10].ToString() == Preferences.EncoderRepetitionCriteria.ECC.ToString())
                    repCriteria = Catalog.GetString("Eccentric");
                else if (reader[10].ToString() == Preferences.EncoderRepetitionCriteria.CON.ToString())
                    repCriteria = Catalog.GetString("Concentric");
            }

            if (encoderGI == Constants.EncoderGI.GRAVITATORY)
            {
                string[] s = {
                    reader[0].ToString(), 	//person uniqueID
					reader[1].ToString(), 	//person name
					reader[2].ToString(), 	//person sex
					econf.ToStringPretty(), //encoder configuration
					reader[4].ToString(), 	//encoder exercise name
					Util.ChangeDecimalSeparator(reader[5].ToString()),	//extra mass
					EncoderSQL.EcconLong(reader[6].ToString()),
                    Util.ChangeDecimalSeparator (reader[7].ToString()),	//power
					Util.ChangeDecimalSeparator (reader[8].ToString()),	//speed
					Util.ChangeDecimalSeparator (reader[9].ToString()),	//force
					repCriteria
                };
                array.Add(s);
            }
            else
            {
                string[] s = {
                    reader[0].ToString(), 	//person uniqueID
					reader[1].ToString(), 	//person name
					reader[2].ToString(), 	//person sex
					econf.ToStringPretty(), //encoder configuration
					reader[4].ToString(), 	//encoder exercise name
					EncoderSQL.EcconLong(reader[6].ToString()),
                    Util.ChangeDecimalSeparator (reader[7].ToString()),	//power
					Util.ChangeDecimalSeparator (reader[8].ToString()),	//speed
					Util.ChangeDecimalSeparator (reader[9].ToString()),	//force
					repCriteria
                };
                array.Add(s);
            }
        }

        reader.Close();
        if (!dbconOpened)
            Sqlite.Close();

        return array;
    }



    /* 
	 * database conversions	
	 */

    //convert from DB 1.05 to 1.06
    //1.06 have curves connected to signals
    //as curves detection on every signal can change depending on smoothing, minimal_height, ...
    //1.06 needs to know where the curve is located in the signal
    //starting ms is not reliable because changes with smoothing
    //use central millisecond.
    //
    //this method will find where the central millisecond of a curve is located in a signal
    //and this will be stored in 1.06 in new EncoderSignalCurve table
    //signalID,curveID,contraction(c,ecS,ceS),msCentral
    //encoder table will continue with signals and curves because we don't want to break things now
    //
    //as explained, following method is only used in conversions from 1.05 to 1.06
    //newly saved curves in 1.06 will write msCentral in EncoderSignalCurve table without needing this method
    public static int FindCurveInSignal(string signalFile, string curveFile)
    {
        int[] signalInts = Util.ReadFileAsInts(signalFile);
        /*	
		LogB.SQL("found INTS");
		for(int i=0; i < signalInts.Length; i ++)
			Log.Write(signalInts[i] + " ");
		*/

        int[] curveInts = Util.ReadFileAsInts(curveFile);
        /*
		LogB.SQL("found INTS");
		for(int i=0; i < curveInts.Length; i ++)
			Log.Write(curveInts[i] + " ");
		*/

        int c;
        for (int s = 0; s < signalInts.Length; s++)
        {
            for (c = 0; c < curveInts.Length && (s + c < signalInts.Length); c++)
                if (signalInts[s + c] != curveInts[c])
                    break;

            if (c == curveInts.Length)
            {
                //LogB.SQL("Start at: " + s);
                //LogB.SQL("Middle at: " + s + Convert.ToInt32(c / 2));
                return s + Convert.ToInt32(c / 2);
            }
        }

        return -1;
    }
}
