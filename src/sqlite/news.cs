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
 * Copyright (C) 2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
//using System.Data;
using System.Collections;
using System.Collections.Generic; //List<T>
using Mono.Data.Sqlite;

class SqliteNews : Sqlite
{
	private static string table = Constants.NewsTable;

	public SqliteNews() {
	}
	
	~SqliteNews() {}
	
	/*
	 * create and initialize tables
	 */

	//server table is created like this:
	//create table news (code INT NOT NULL AUTO_INCREMENT PRIMARY KEY, category INT, version INT, titleEn char(255), titleEs char(255), linkEn char(255), linkEs char(255), descriptionEn char(255), descriptionEs char(255), linkServerImage char(255));
	//INSERT INTO news VALUES (NULL, 1, 0, "Race Analyzer kit", "Kit Analizador Carreras", "https://chronojump.org/product-category/races/race_analyzer_kit/", "https://chronojump.org/es/categoria-producto/carreras/kit_analizador_carreras/", "Device that allows you to analyze and graph races from the time obtained every 3cm of race.\nThe mathematical model implemented by Chronojump allows you to graph speeds, accelerations, forces and instantaneous powers.", "Dispositivo que permite analizar y graficar carreras a partir del tiempo obtenido cada 3cm.\nEl modelo matemático implementado por Chronojump le permite graficar velocidades, aceleraciones, fuerzas y potencias instantáneas.", "https://chronojump.org/wp-content/uploads/2020/12/Race-Analyzer_new_products_400w.jpeg");


	protected internal static void createTable ()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + table + " ( " +
			"code INT, " +
			"category INT, " +
			"version INT, " +
			"viewed INT, " +
			"titleEn TEXT, " +
			"titleEs TEXT, " +
			"linkEn TEXT, " +
			"linkEs TEXT, " +
			"descriptionEn TEXT, " +
			"descriptionEs TEXT, " +
			"linkServerImage TEXT )";
		dbcmd.ExecuteNonQuery();
	}

	public static int Insert (bool dbconOpened, string insertString)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "INSERT INTO " + table +
				" (code, category, version, viewed, titleEn, titleEs, linkEn, linkEs, descriptionEn, descriptionEs, linkServerImage)" +
				" VALUES (" + insertString + ")";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		string myString = @"select last_insert_rowid()";
		dbcmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		closeIfNeeded(dbconOpened);

		return myLast;
	}

	//code -1 (select all)
	public static List<News> Select (bool dbconOpened, int code)
	{
		openIfNeeded(dbconOpened);

		string codeStr = "";
		if(code != -1)
			codeStr = " WHERE code = " + code;

		dbcmd.CommandText = "SELECT * FROM " + table + codeStr + " ORDER BY code DESC";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		List<News> news_l = new List<News>();

		while(reader.Read())
			news_l.Add(new News (
					Convert.ToInt32(reader[0].ToString()),	//code
					Convert.ToInt32(reader[1].ToString()),	//category
					Convert.ToInt32(reader[2].ToString()),	//version
					Util.IntToBool(Convert.ToInt32(reader[3].ToString())),	//viewed
					reader[4].ToString(), 			//titleEn
					reader[5].ToString(), 			//titleEs
					reader[6].ToString(), 			//linkEn (web)
					reader[7].ToString(), 			//linkEs (web)
					reader[8].ToString(), 			//descriptionEn
					reader[9].ToString(), 			//descriptionEs
					reader[10].ToString()			//linkServerImage
					));

		reader.Close();
		closeIfNeeded(dbconOpened);
		return news_l;
	}
}

