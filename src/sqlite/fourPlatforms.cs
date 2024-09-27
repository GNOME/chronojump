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
 * Copyright (C) 2024   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
//using System.Data;
using System.Collections;
using System.Collections.Generic; //List<T>
//using System.IO; //DirectoryInfo
using System.Data.SQLite;
//using System.Text.RegularExpressions; //Regex

class SqliteFourPlatforms : Sqlite
{
	private static string table = Constants.FourPlatformsTable;

	public SqliteFourPlatforms() {
	}

	~SqliteFourPlatforms() {}

	/*
	 * create and initialize tables
	 */

	protected internal static new void createTable()
	{
		dbcmd.CommandText =
			"CREATE TABLE " + table + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"exerciseID INT, " + //right now all will be exercise 0, until we have a clear idea of what exercises could be done and how can affect measurements
			"b0_1 TEXT, " +  //on
			"b0_0 TEXT, " +  //off
			"b1_1 TEXT, " +
			"b1_0 TEXT, " +
			"b2_1 TEXT, " +
			"b2_0 TEXT, " +
			"b3_1 TEXT, " +
			"b3_0 TEXT, " +
			"datetime TEXT, " + 	//2019-07-11_15-01-44
			"comments TEXT, " +
			"videoURL TEXT, " +	//URL of video of signals. stored as relative
			"totalTime INT)";	//needed to sync with video. If we press finish when there are no pulsees we cannot sync. If we use totalTime we can sync.
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

	public static int Insert (bool dbconOpened, string insertString)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "INSERT INTO " + table +
				" (uniqueID, personID, sessionID, exerciseID, dateTime, " + 
				" b0_1, b0_0, b1_1, b1_0, b2_1, b2_0, b3_1, b3_0, " +
				" comments, videoURL, totalTime)" +
				" VALUES " + insertString;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		string myString = @"select last_insert_rowid()";
		dbcmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		closeIfNeeded(dbconOpened);

		return myLast;
	}

	public static void Update (bool dbconOpened, string updateString)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "UPDATE " + table + " SET " + updateString;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}

	public static void UpdateComments (bool dbconOpened, int uniqueID, string comments)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "UPDATE " + table + " SET comments = '" + comments + "'" +
			" WHERE uniqueID = " + uniqueID;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}
}

//using fourPlatforms to store simple jumps
class SqliteFourPlatformsJumpsSimple : Sqlite
{
	private static string table = Constants.JumpTable;
	private bool hasFall;

	public SqliteFourPlatformsJumpsSimple (bool hasFall)
	{
		this.hasFall = hasFall;
	}

	//public int Insert (int personID, int sessionID,
	public void Insert (List<IDName> person_l, int sessionID,
			string jumpType, List<List<double>> off_ll, List<List<double>> on_ll, double firstFall,
			double weight, string description, int angle, bool simulated,
			string datetimeStr)
	{
		Sqlite.Open();
		using(SQLiteTransaction tr = dbcon.BeginTransaction())
		{
			using (SQLiteCommand dbcmdTr = dbcon.CreateCommand())
			{
				dbcmdTr.Transaction = tr;
				for (int p = 0; p < 4; p ++)
				{
					if (person_l[p].UniqueID < 0)
						continue;

					double tf = 0;
					double tc = 0;
					double fall = 0;

					bool firstJump = true; //to use firstFall
					double thisOnTime = 0;
					double lastOnTime = 0;
					double tfLast = 0; //used to calculate fall

					for (int j = 0; j < on_ll[p].Count; j ++)
					{
						//LogB.Information (string.Format ("on {0}, onTime {1} ", j, on_ll[p][j]));
						bool found = false;
						int k = 0;
						for (k = off_ll[p].Count -1; k >= 0; k --)
						{
							//LogB.Information (string.Format ("off {0}, offTime {1} ", k, off_ll[p][k]));
							if (off_ll[p][k] < on_ll[p][j])
							{
								//LogB.Information ("found!");
								tf = on_ll[p][j] - off_ll[p][k];
								thisOnTime = on_ll[p][j];

								found = true;
								break;
							}
						}

						if (found && hasFall)
						{
							if (firstJump)
							{
								firstJump = false;

								//do not accept a jump with tc when started in
								if (off_ll[p][0] < on_ll[p][0])
								{
									lastOnTime = thisOnTime;
									tfLast = tf;
									continue;
								}

								fall = firstFall;
							} else
								fall = Util.GetHeightInCentimeters (tfLast);

							tc = off_ll[p][k] - lastOnTime;
						}

						if (found)
						{
							SqliteJump.InsertDo (true, table,
									"-1", person_l[p].UniqueID, sessionID, jumpType,
									tf, tc, fall, weight, "", -1, 0, datetimeStr,
									dbcmdTr);

							lastOnTime = thisOnTime;
							tfLast = tf;
						}
					}
				}
			}
			tr.Commit();
		}
		Sqlite.Close();
	}

	~SqliteFourPlatformsJumpsSimple() {}
}
