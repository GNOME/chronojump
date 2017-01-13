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
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */


using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
//using Mono.Data.SqliteClient;
//using System.Data.SqlClient;
using Mono.Data.Sqlite;
//using System.Data.SQLite;
using Mono.Unix;


class SqliteCountry : Sqlite
{
	protected internal static new void createTable()
	 {
		dbcmd.CommandText = 
			"CREATE TABLE " + Constants.CountryTable + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"code TEXT, " +
			"continent TEXT )";
		dbcmd.ExecuteNonQuery();
	 }

	// intialize table
	protected internal static void initialize()
	{
		conversionSubRateTotal = countries.Length;
		conversionSubRate = 0;

		using(SqliteTransaction tr = dbcon.BeginTransaction())
		{
			using (SqliteCommand dbcmdTr = dbcon.CreateCommand())
			{
				dbcmdTr.Transaction = tr;
	
				foreach(string myString in countries) {
					//put in db only english name
					string [] strFull = myString.Split(new char[] {':'});
					Insert(
							true,   		//dbconOpened
							dbcmdTr,
							strFull[3], 		//code
							strFull[1], 		//name (english)
							strFull[0]		//continent
					      );
					conversionSubRate ++;
				}
			}
			tr.Commit();
		}
	}
	public static void Insert(bool dbconOpened, SqliteCommand mycmd, string code, string nameEnglish, string continent)
	{
		if(! dbconOpened)
			Sqlite.Open();

		string myString = "INSERT INTO " + Constants.CountryTable + 
			//" (uniqueID, code, name, continent) VALUES (NULL, '" + code + "', '" + 
			//nameEnglish + "', '" + continent + "')";
			//fix bad chars (') :
			" (uniqueID, code, name, continent) VALUES (NULL, \"" + code + "\", \"" + 
			nameEnglish + "\", \"" + continent + "\")";

		mycmd.CommandText = myString;
		LogB.SQL(mycmd.CommandText.ToString());
		mycmd.ExecuteNonQuery();

		/*
		//int myLast = dbcon.LastInsertRowId;
		//http://stackoverflow.com/questions/4341178/getting-the-last-insert-id-with-sqlite-net-in-c
		myString = @"select last_insert_rowid()";
		mycmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.
		*/

		if(! dbconOpened)
			Sqlite.Close();
	}

	public static string [] SelectCountriesOfAContinent(string continent, bool insertUndefined)
	{
		Sqlite.Open();
		
		dbcmd.CommandText = "SELECT uniqueID, name FROM " + Constants.CountryTable + " WHERE continent == \"" + continent + "\"";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		ArrayList myArray = new ArrayList(1);
		while(reader.Read()) 
			myArray.Add(reader[0].ToString() + ":" + 
					reader[1].ToString() + ":" + 
					Catalog.GetString(reader[1].ToString()));
		reader.Close();
		Sqlite.Close();

		int count = 0;
		string [] myReturn;
		if(insertUndefined) {
			myReturn = new string[myArray.Count +1];
			myReturn[count ++] = Constants.CountryUndefinedID + ":" +
				Constants.CountryUndefined + ":" +
				Catalog.GetString(Constants.CountryUndefined);
		} else
			myReturn = new string[myArray.Count];

		foreach (string line in myArray) 
			myReturn [count++] = line;
		
		return myReturn;
	}
	
	public static string [] Select(int uniqueID)
	{
		Sqlite.Open();
		
		dbcmd.CommandText = "SELECT * FROM " + Constants.CountryTable + " WHERE uniqueID == " + uniqueID;
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();
		
		string [] myReturn = new String [4];	
		myReturn[0] = reader[0].ToString(); //uniqueID
		myReturn[1] = reader[1].ToString(); //name
		myReturn[2] = reader[2].ToString(); //code
		myReturn[3] = reader[3].ToString(); //continent
		
		reader.Close();
		Sqlite.Close();
		return myReturn;
	}

	//useful to convert DB from 0.57 to 0.58 (strip republic and kingdom stuff)
	public static bool TableHasOldRepublicStuff() {
		dbcmd.CommandText = "SELECT name FROM " + Constants.CountryTable + " WHERE code == \"DZA\"";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();
		
		bool hasRepublicStuff;
		if(reader[0].ToString() == "Algeria") 
			hasRepublicStuff = false;
		else 
			hasRepublicStuff = true;

		LogB.SQL(reader[0].ToString() + " " + hasRepublicStuff);
		
		reader.Close();
		return hasRepublicStuff;
	}


	/*
	 * the Catalog.GetString is only for having a translation that will be used on display sport name if available
	 * don't sportuserchecks here, doit in the database because sports will grow
	 * all this are obviously NOT user defined
	 * last string is for graphLink
	 */
	private static string [] countries = {
		//true or false means if it has speciallities
		Constants.CountryUndefined + ":" + Constants.CountryUndefined + ":" + Catalog.GetString(Constants.CountryUndefined) + ":" + Constants.CountryUndefined, //will be 1 (it's also written in Constants.CountryUndefinedID
		"Africa:" + "Algeria:" + Catalog.GetString("Algeria") + ":" + "DZA",
		"Africa:" + "Angola:" + Catalog.GetString("Angola") + ":" + "AGO",
		"Africa:" + "Benin:" + Catalog.GetString("Benin") + ":" + "BEN",
		"Africa:" + "Botswana:" + Catalog.GetString("Botswana") + ":" + "BWA",
		"Africa:" + "Burkina Faso:" + Catalog.GetString("Burkina Faso") + ":" + "BFA",
		"Africa:" + "Burundi:" + Catalog.GetString("Burundi") + ":" + "BDI",
		"Africa:" + "Cameroon:" + Catalog.GetString("Cameroon") + ":" + "CMR",
		"Africa:" + "Cape Verde:" + Catalog.GetString("Cape Verde") + ":" + "CPV",
		"Africa:" + "Central African Republic:" + Catalog.GetString("Central African Republic") + ":" + "CAF",
		"Africa:" + "Chad:" + Catalog.GetString("Chad") + ":" + "TCD",
		"Africa:" + "Comoros:" + Catalog.GetString("Comoros") + ":" + "COM",
		"Africa:" + "Congo, Democratic Republic of the:" + Catalog.GetString("Congo, Democratic Republic of the") + ":" + "COD",
		"Africa:" + "Congo, Republic of the:" + Catalog.GetString("Congo, Republic of the") + ":" + "COG",
		"Africa:" + "Cote d\'Ivoire:" + Catalog.GetString("Cote d'Ivoire") + ":" + "CIV",
		"Africa:" + "Djibouti:" + Catalog.GetString("Djibouti") + ":" + "DJI",
		"Africa:" + "Egypt:" + Catalog.GetString("Egypt") + ":" + "EGY",
		"Africa:" + "Equatorial Guinea:" + Catalog.GetString("Equatorial Guinea") + ":" + "GNQ",
		"Africa:" + "Eritrea:" + Catalog.GetString("Eritrea") + ":" + "ERI",
		"Africa:" + "Ethiopia:" + Catalog.GetString("Ethiopia") + ":" + "ETH",
		"Africa:" + "Gabon:" + Catalog.GetString("Gabon") + ":" + "GAB",
		"Africa:" + "Gambia:" + Catalog.GetString("Gambia") + ":" + "GMB",
		"Africa:" + "Ghana:" + Catalog.GetString("Ghana") + ":" + "GHA",
		"Africa:" + "Guinea-Bissau:" + Catalog.GetString("Guinea-Bissau") + ":" + "GNB",
		"Africa:" + "Guinea:" + Catalog.GetString("Guinea") + ":" + "GIN",
		"Africa:" + "Kenya:" + Catalog.GetString("Kenya") + ":" + "KEN",
		"Africa:" + "Lesotho:" + Catalog.GetString("Lesotho") + ":" + "LSO",
		"Africa:" + "Liberia:" + Catalog.GetString("Liberia") + ":" + "LBR",
		"Africa:" + "Libyan Arab Jamahiriya:" + Catalog.GetString("Libyan Arab Jamahiriya") + ":" + "LBY",
		"Africa:" + "Madagascar:" + Catalog.GetString("Madagascar") + ":" + "MDG",
		"Africa:" + "Malawi:" + Catalog.GetString("Malawi") + ":" + "MWI",
		"Africa:" + "Mali:" + Catalog.GetString("Mali") + ":" + "MLI",
		"Africa:" + "Mauritania:" + Catalog.GetString("Mauritania") + ":" + "MRT",
		"Africa:" + "Mauritius:" + Catalog.GetString("Mauritius") + ":" + "MUS",
		"Africa:" + "Mayotte:" + Catalog.GetString("Mayotte") + ":" + "MYT",
		"Africa:" + "Morocco:" + Catalog.GetString("Morocco") + ":" + "MAR",
		"Africa:" + "Mozambique:" + Catalog.GetString("Mozambique") + ":" + "MOZ",
		"Africa:" + "Namibia:" + Catalog.GetString("Namibia") + ":" + "NAM",
		"Africa:" + "Nigeria:" + Catalog.GetString("Nigeria") + ":" + "NGA",
		"Africa:" + "Niger:" + Catalog.GetString("Niger") + ":" + "NER",
		"Africa:" + "Reunion:" + Catalog.GetString("Reunion") + ":" + "REU",
		"Africa:" + "Rwanda:" + Catalog.GetString("Rwanda") + ":" + "RWA",
		"Africa:" + "Saint Helena:" + Catalog.GetString("Saint Helena") + ":" + "SHN",
		"Africa:" + "Sao Tome and Principe:" + Catalog.GetString("Sao Tome and Principe") + ":" + "STP",
		"Africa:" + "Senegal:" + Catalog.GetString("Senegal") + ":" + "SEN",
		"Africa:" + "Seychelles:" + Catalog.GetString("Seychelles") + ":" + "SYC",
		"Africa:" + "Sierra Leone:" + Catalog.GetString("Sierra Leone") + ":" + "SLE",
		"Africa:" + "Somalia:" + Catalog.GetString("Somalia") + ":" + "SOM",
		"Africa:" + "South Africa:" + Catalog.GetString("South Africa") + ":" + "ZAF",
		"Africa:" + "Sudan:" + Catalog.GetString("Sudan") + ":" + "SDN",
		"Africa:" + "Suriname:" + Catalog.GetString("Suriname") + ":" + "SUR",
		"Africa:" + "Swaziland:" + Catalog.GetString("Swaziland") + ":" + "SWZ",
		"Africa:" + "Tanzania:" + Catalog.GetString("Tanzania") + ":" + "TZA",
		"Africa:" + "Togo:" + Catalog.GetString("Togo") + ":" + "TGO",
		"Africa:" + "Tunisia:" + Catalog.GetString("Tunisia") + ":" + "TUN",
		"Africa:" + "Uganda:" + Catalog.GetString("Uganda") + ":" + "UGA",
		"Africa:" + "Western Sahara:" + Catalog.GetString("Western Sahara") + ":" + "ESH",
		"Africa:" + "Zambia:" + Catalog.GetString("Zambia") + ":" + "ZMB",
		"Africa:" + "Zimbabwe:" + Catalog.GetString("Zimbabwe") + ":" + "ZWE",
		"Antarctica:" + "Antarctica (the territory South of 60 deg S):" + Catalog.GetString("Antarctica (the territory South of 60 deg S)") + ":" + "ATA",
		"Antarctica:" + "Bouvet Island (Bouvetoya):" + Catalog.GetString("Bouvet Island (Bouvetoya)") + ":" + "BVT",
		"Antarctica:" + "French Southern Territories:" + Catalog.GetString("French Southern Territories") + ":" + "ATF",
		"Antarctica:" + "Heard Island and McDonald Islands:" + Catalog.GetString("Heard Island and McDonald Islands") + ":" + "HMD",
		"Antarctica:" + "South Georgia and the South Sandwich Islands:" + Catalog.GetString("South Georgia and the South Sandwich Islands") + ":" + "SGS",
		"Asia:" + "Afghanistan:" + Catalog.GetString("Afghanistan") + ":" + "AFG",
		"Asia:" + "Armenia:" + Catalog.GetString("Armenia") + ":" + "ARM",
		"Asia:" + "Azerbaijan:" + Catalog.GetString("Azerbaijan") + ":" + "AZE",
		"Asia:" + "Bahrain:" + Catalog.GetString("Bahrain") + ":" + "BHR",
		"Asia:" + "Bangladesh:" + Catalog.GetString("Bangladesh") + ":" + "BGD",
		"Asia:" + "Bhutan:" + Catalog.GetString("Bhutan") + ":" + "BTN",
		"Asia:" + "British Indian Ocean Territory (Chagos Archipelago):" + Catalog.GetString("British Indian Ocean Territory (Chagos Archipelago)") + ":" + "IOT",
		"Asia:" + "Brunei Darussalam:" + Catalog.GetString("Brunei Darussalam") + ":" + "BRN",
		"Asia:" + "Cambodia:" + Catalog.GetString("Cambodia") + ":" + "KHM",
		"Asia:" + "China:" + Catalog.GetString("China") + ":" + "CHN",
		"Asia:" + "Christmas Island:" + Catalog.GetString("Christmas Island") + ":" + "CXR",
		"Asia:" + "Cocos (Keeling) Islands:" + Catalog.GetString("Cocos (Keeling) Islands") + ":" + "CCK",
		"Asia:" + "Georgia:" + Catalog.GetString("Georgia") + ":" + "GEO",
		"Asia:" + "Hong Kong:" + Catalog.GetString("Hong Kong") + ":" + "HKG",
		"Asia:" + "India:" + Catalog.GetString("India") + ":" + "IND",
		"Asia:" + "Indonesia:" + Catalog.GetString("Indonesia") + ":" + "IDN",
		"Asia:" + "Iran:" + Catalog.GetString("Iran") + ":" + "IRN",
		"Asia:" + "Iraq:" + Catalog.GetString("Iraq") + ":" + "IRQ",
		"Asia:" + "Israel:" + Catalog.GetString("Israel") + ":" + "ISR",
		"Asia:" + "Japan:" + Catalog.GetString("Japan") + ":" + "JPN",
		"Asia:" + "Jordan:" + Catalog.GetString("Jordan") + ":" + "JOR",
		"Asia:" + "Kazakhstan:" + Catalog.GetString("Kazakhstan") + ":" + "KAZ",
		"Asia:" + "Korea, Democratic People's Republic of:" + Catalog.GetString("Korea, Democratic People's Republic of") + ":" + "PRK",
		"Asia:" + "Korea, Republic of:" + Catalog.GetString("Korea, Republic of") + ":" + "KOR",
		"Asia:" + "Kuwait:" + Catalog.GetString("Kuwait") + ":" + "KWT",
		"Asia:" + "Kyrgyz Republic:" + Catalog.GetString("Kyrgyz Republic") + ":" + "KGZ",
		"Asia:" + "Lao People's Democratic Republic:" + Catalog.GetString("Lao People's Democratic Republic") + ":" + "LAO",
		"Asia:" + "Lebanon:" + Catalog.GetString("Lebanon") + ":" + "LBN",
		"Asia:" + "Macao:" + Catalog.GetString("Macao") + ":" + "MAC",
		"Asia:" + "Malaysia:" + Catalog.GetString("Malaysia") + ":" + "MYS",
		"Asia:" + "Maldives:" + Catalog.GetString("Maldives") + ":" + "MDV",
		"Asia:" + "Mongolia:" + Catalog.GetString("Mongolia") + ":" + "MNG",
		"Asia:" + "Myanmar:" + Catalog.GetString("Myanmar") + ":" + "MMR",
		"Asia:" + "Nepal:" + Catalog.GetString("Nepal") + ":" + "NPL",
		"Asia:" + "Oman:" + Catalog.GetString("Oman") + ":" + "OMN",
		"Asia:" + "Pakistan:" + Catalog.GetString("Pakistan") + ":" + "PAK",
		"Asia:" + "Palestinian Territory:" + Catalog.GetString("Palestinian Territory") + ":" + "PSE",
		"Asia:" + "Philippines:" + Catalog.GetString("Philippines") + ":" + "PHL",
		"Asia:" + "Qatar:" + Catalog.GetString("Qatar") + ":" + "QAT",
		"Asia:" + "Saudi Arabia:" + Catalog.GetString("Saudi Arabia") + ":" + "SAU",
		"Asia:" + "Singapore:" + Catalog.GetString("Singapore") + ":" + "SGP",
		"Asia:" + "Sri Lanka:" + Catalog.GetString("Sri Lanka") + ":" + "LKA",
		"Asia:" + "Syrian Arab Republic:" + Catalog.GetString("Syrian Arab Republic") + ":" + "SYR",
		"Asia:" + "Taiwan:" + Catalog.GetString("Taiwan") + ":" + "TWN",
		"Asia:" + "Tajikistan:" + Catalog.GetString("Tajikistan") + ":" + "TJK",
		"Asia:" + "Thailand:" + Catalog.GetString("Thailand") + ":" + "THA",
		"Asia:" + "Timor-Leste:" + Catalog.GetString("Timor-Leste") + ":" + "TLS",
		"Asia:" + "Turkey:" + Catalog.GetString("Turkey") + ":" + "TUR",
		"Asia:" + "Turkmenistan:" + Catalog.GetString("Turkmenistan") + ":" + "TKM",
		"Asia:" + "United Arab Emirates:" + Catalog.GetString("United Arab Emirates") + ":" + "ARE",
		"Asia:" + "Uzbekistan:" + Catalog.GetString("Uzbekistan") + ":" + "UZB",
		"Asia:" + "Vietnam:" + Catalog.GetString("Vietnam") + ":" + "VNM",
		"Asia:" + "Yemen:" + Catalog.GetString("Yemen") + ":" + "YEM",
		"Europe:" + "Åland Islands:" + Catalog.GetString("Åland Islands") + ":" + "ALA",
		"Europe:" + "Albania:" + Catalog.GetString("Albania") + ":" + "ALB",
		"Europe:" + "Andorra:" + Catalog.GetString("Andorra") + ":" + "AND",
		"Europe:" + "Austria:" + Catalog.GetString("Austria") + ":" + "AUT",
		"Europe:" + "Belarus:" + Catalog.GetString("Belarus") + ":" + "BLR",
		"Europe:" + "Belgium:" + Catalog.GetString("Belgium") + ":" + "BEL",
		"Europe:" + "Bosnia and Herzegovina:" + Catalog.GetString("Bosnia and Herzegovina") + ":" + "BIH",
		"Europe:" + "Bulgaria:" + Catalog.GetString("Bulgaria") + ":" + "BGR",
		"Europe:" + "Croatia:" + Catalog.GetString("Croatia") + ":" + "HRV",
		"Europe:" + "Cyprus:" + Catalog.GetString("Cyprus") + ":" + "CYP",
		"Europe:" + "Czech Republic:" + Catalog.GetString("Czech Republic") + ":" + "CZE",
		"Europe:" + "Denmark:" + Catalog.GetString("Denmark") + ":" + "DNK",
		"Europe:" + "Estonia:" + Catalog.GetString("Estonia") + ":" + "EST",
		"Europe:" + "Faroe Islands:" + Catalog.GetString("Faroe Islands") + ":" + "FRO",
		"Europe:" + "Finland:" + Catalog.GetString("Finland") + ":" + "FIN",
		"Europe:" + "France:" + Catalog.GetString("France") + ":" + "FRA",
		"Europe:" + "Germany:" + Catalog.GetString("Germany") + ":" + "DEU",
		"Europe:" + "Gibraltar:" + Catalog.GetString("Gibraltar") + ":" + "GIB",
		"Europe:" + "Greece:" + Catalog.GetString("Greece") + ":" + "GRC",
		"Europe:" + "Guernsey:" + Catalog.GetString("Guernsey") + ":" + "GGY",
		"Europe:" + "Holy See (Vatican City State):" + Catalog.GetString("Holy See (Vatican City State)") + ":" + "VAT",
		"Europe:" + "Hungary:" + Catalog.GetString("Hungary") + ":" + "HUN",
		"Europe:" + "Iceland:" + Catalog.GetString("Iceland") + ":" + "ISL",
		"Europe:" + "Ireland:" + Catalog.GetString("Ireland") + ":" + "IRL",
		"Europe:" + "Isle of Man:" + Catalog.GetString("Isle of Man") + ":" + "IMN",
		"Europe:" + "Italy:" + Catalog.GetString("Italy") + ":" + "ITA",
		"Europe:" + "Jersey:" + Catalog.GetString("Jersey") + ":" + "JEY",
		"Europe:" + "Latvia:" + Catalog.GetString("Latvia") + ":" + "LVA",
		"Europe:" + "Liechtenstein:" + Catalog.GetString("Liechtenstein") + ":" + "LIE",
		"Europe:" + "Lithuania:" + Catalog.GetString("Lithuania") + ":" + "LTU",
		"Europe:" + "Luxembourg:" + Catalog.GetString("Luxembourg") + ":" + "LUX",
		"Europe:" + "Macedonia:" + Catalog.GetString("Macedonia") + ":" + "MKD",
		"Europe:" + "Malta:" + Catalog.GetString("Malta") + ":" + "MLT",
		"Europe:" + "Moldova:" + Catalog.GetString("Moldova") + ":" + "MDA",
		"Europe:" + "Monaco:" + Catalog.GetString("Monaco") + ":" + "MCO",
		"Europe:" + "Montenegro:" + Catalog.GetString("Montenegro") + ":" + "MNE",
		"Europe:" + "Netherlands:" + Catalog.GetString("Netherlands") + ":" + "NLD",
		"Europe:" + "Norway:" + Catalog.GetString("Norway") + ":" + "NOR",
		"Europe:" + "Poland:" + Catalog.GetString("Poland") + ":" + "POL",
		"Europe:" + "Portugal:" + Catalog.GetString("Portugal") + ":" + "PRT",
		"Europe:" + "Romania:" + Catalog.GetString("Romania") + ":" + "ROU",
		"Europe:" + "Russian Federation:" + Catalog.GetString("Russian Federation") + ":" + "RUS",
		"Europe:" + "San Marino:" + Catalog.GetString("San Marino") + ":" + "SMR",
		"Europe:" + "Serbia:" + Catalog.GetString("Serbia") + ":" + "SRB",
		"Europe:" + "Slovakia (Slovak Republic):" + Catalog.GetString("Slovakia (Slovak Republic)") + ":" + "SVK",
		"Europe:" + "Slovenia:" + Catalog.GetString("Slovenia") + ":" + "SVN",
		"Europe:" + "Spain:" + Catalog.GetString("Spain") + ":" + "ESP",
		"Europe:" + "Svalbard & Jan Mayen Islands:" + Catalog.GetString("Svalbard & Jan Mayen Islands") + ":" + "SJM",
		"Europe:" + "Sweden:" + Catalog.GetString("Sweden") + ":" + "SWE",
		"Europe:" + "Switzerland:" + Catalog.GetString("Switzerland") + ":" + "CHE",
		"Europe:" + "Ukraine:" + Catalog.GetString("Ukraine") + ":" + "UKR",
		"Europe:" + "United Kingdom of Great Britain & Northern Ireland:" + Catalog.GetString("United Kingdom of Great Britain & Northern Ireland") + ":" + "GBR",
		"North America:" + "Anguilla:" + Catalog.GetString("Anguilla") + ":" + "AIA",
		"North America:" + "Antigua and Barbuda:" + Catalog.GetString("Antigua and Barbuda") + ":" + "ATG",
		"North America:" + "Aruba:" + Catalog.GetString("Aruba") + ":" + "ABW",
		"North America:" + "Bahamas:" + Catalog.GetString("Bahamas") + ":" + "BHS",
		"North America:" + "Barbados:" + Catalog.GetString("Barbados") + ":" + "BRB",
		"North America:" + "Belize:" + Catalog.GetString("Belize") + ":" + "BLZ",
		"North America:" + "Bermuda:" + Catalog.GetString("Bermuda") + ":" + "BMU",
		"North America:" + "British Virgin Islands:" + Catalog.GetString("British Virgin Islands") + ":" + "VGB",
		"North America:" + "Canada:" + Catalog.GetString("Canada") + ":" + "CAN",
		"North America:" + "Cayman Islands:" + Catalog.GetString("Cayman Islands") + ":" + "CYM",
		"North America:" + "Costa Rica:" + Catalog.GetString("Costa Rica") + ":" + "CRI",
		"North America:" + "Cuba:" + Catalog.GetString("Cuba") + ":" + "CUB",
		"North America:" + "Dominica, Commonwealth of:" + Catalog.GetString("Dominica, Commonwealth of") + ":" + "DMA",
		"North America:" + "Dominican Republic:" + Catalog.GetString("Dominican Republic") + ":" + "DOM",
		"North America:" + "El Salvador:" + Catalog.GetString("El Salvador") + ":" + "SLV",
		"North America:" + "Greenland:" + Catalog.GetString("Greenland") + ":" + "GRL",
		"North America:" + "Grenada:" + Catalog.GetString("Grenada") + ":" + "GRD",
		"North America:" + "Guadeloupe:" + Catalog.GetString("Guadeloupe") + ":" + "GLP",
		"North America:" + "Guatemala:" + Catalog.GetString("Guatemala") + ":" + "GTM",
		"North America:" + "Haiti:" + Catalog.GetString("Haiti") + ":" + "HTI",
		"North America:" + "Honduras:" + Catalog.GetString("Honduras") + ":" + "HND",
		"North America:" + "Jamaica:" + Catalog.GetString("Jamaica") + ":" + "JAM",
		"North America:" + "Martinique:" + Catalog.GetString("Martinique") + ":" + "MTQ",
		"North America:" + "Mexico:" + Catalog.GetString("Mexico") + ":" + "MEX",
		"North America:" + "Montserrat:" + Catalog.GetString("Montserrat") + ":" + "MSR",
		"North America:" + "Netherlands Antilles:" + Catalog.GetString("Netherlands Antilles") + ":" + "ANT",
		"North America:" + "Nicaragua:" + Catalog.GetString("Nicaragua") + ":" + "NIC",
		"North America:" + "Panama:" + Catalog.GetString("Panama") + ":" + "PAN",
		"North America:" + "Puerto Rico:" + Catalog.GetString("Puerto Rico") + ":" + "PRI",
		"North America:" + "Saint Barthelemy:" + Catalog.GetString("Saint Barthelemy") + ":" + "BLM",
		"North America:" + "Saint Kitts and Nevis:" + Catalog.GetString("Saint Kitts and Nevis") + ":" + "KNA",
		"North America:" + "Saint Lucia:" + Catalog.GetString("Saint Lucia") + ":" + "LCA",
		"North America:" + "Saint Martin:" + Catalog.GetString("Saint Martin") + ":" + "MAF",
		"North America:" + "Saint Pierre and Miquelon:" + Catalog.GetString("Saint Pierre and Miquelon") + ":" + "SPM",
		"North America:" + "Saint Vincent and the Grenadines:" + Catalog.GetString("Saint Vincent and the Grenadines") + ":" + "VCT",
		"North America:" + "Trinidad and Tobago:" + Catalog.GetString("Trinidad and Tobago") + ":" + "TTO",
		"North America:" + "Turks and Caicos Islands:" + Catalog.GetString("Turks and Caicos Islands") + ":" + "TCA",
		"North America:" + "United States of America:" + Catalog.GetString("United States of America") + ":" + "USA",
		"North America:" + "United States Virgin Islands:" + Catalog.GetString("United States Virgin Islands") + ":" + "VIR",
		"Oceania:" + "American Samoa:" + Catalog.GetString("American Samoa") + ":" + "ASM",
		"Oceania:" + "Australia:" + Catalog.GetString("Australia") + ":" + "AUS",
		"Oceania:" + "Cook Islands:" + Catalog.GetString("Cook Islands") + ":" + "COK",
		"Oceania:" + "Fiji:" + Catalog.GetString("Fiji") + ":" + "FJI",
		"Oceania:" + "French Polynesia:" + Catalog.GetString("French Polynesia") + ":" + "PYF",
		"Oceania:" + "Guam:" + Catalog.GetString("Guam") + ":" + "GUM",
		"Oceania:" + "Kiribati:" + Catalog.GetString("Kiribati") + ":" + "KIR",
		"Oceania:" + "Marshall Islands:" + Catalog.GetString("Marshall Islands") + ":" + "MHL",
		"Oceania:" + "Micronesia:" + Catalog.GetString("Micronesia") + ":" + "FSM",
		"Oceania:" + "Nauru:" + Catalog.GetString("Nauru") + ":" + "NRU",
		"Oceania:" + "New Caledonia:" + Catalog.GetString("New Caledonia") + ":" + "NCL",
		"Oceania:" + "New Zealand:" + Catalog.GetString("New Zealand") + ":" + "NZL",
		"Oceania:" + "Niue:" + Catalog.GetString("Niue") + ":" + "NIU",
		"Oceania:" + "Norfolk Island:" + Catalog.GetString("Norfolk Island") + ":" + "NFK",
		"Oceania:" + "Northern Mariana Islands:" + Catalog.GetString("Northern Mariana Islands") + ":" + "MNP",
		"Oceania:" + "Palau:" + Catalog.GetString("Palau") + ":" + "PLW",
		"Oceania:" + "Papua New Guinea:" + Catalog.GetString("Papua New Guinea") + ":" + "PNG",
		"Oceania:" + "Pitcairn Islands:" + Catalog.GetString("Pitcairn Islands") + ":" + "PCN",
		"Oceania:" + "Samoa:" + Catalog.GetString("Samoa") + ":" + "WSM",
		"Oceania:" + "Solomon Islands:" + Catalog.GetString("Solomon Islands") + ":" + "SLB",
		"Oceania:" + "Tokelau:" + Catalog.GetString("Tokelau") + ":" + "TKL",
		"Oceania:" + "Tonga:" + Catalog.GetString("Tonga") + ":" + "TON",
		"Oceania:" + "Tuvalu:" + Catalog.GetString("Tuvalu") + ":" + "TUV",
		"Oceania:" + "United States Minor Outlying Islands:" + Catalog.GetString("United States Minor Outlying Islands") + ":" + "UMI",
		"Oceania:" + "Vanuatu:" + Catalog.GetString("Vanuatu") + ":" + "VUT",
		"Oceania:" + "Wallis and Futuna:" + Catalog.GetString("Wallis and Futuna") + ":" + "WLF",
		"South America:" + "Argentina:" + Catalog.GetString("Argentina") + ":" + "ARG",
		"South America:" + "Bolivia:" + Catalog.GetString("Bolivia") + ":" + "BOL",
		"South America:" + "Brazil:" + Catalog.GetString("Brazil") + ":" + "BRA",
		"South America:" + "Chile:" + Catalog.GetString("Chile") + ":" + "CHL",
		"South America:" + "Colombia:" + Catalog.GetString("Colombia") + ":" + "COL",
		"South America:" + "Ecuador:" + Catalog.GetString("Ecuador") + ":" + "ECU",
		"South America:" + "Falkland Islands (Malvinas):" + Catalog.GetString("Falkland Islands (Malvinas)") + ":" + "FLK",
		"South America:" + "French Guiana:" + Catalog.GetString("French Guiana") + ":" + "GUF",
		"South America:" + "Guyana:" + Catalog.GetString("Guyana") + ":" + "GUY",
		"South America:" + "Paraguay:" + Catalog.GetString("Paraguay") + ":" + "PRY",
		"South America:" + "Peru:" + Catalog.GetString("Peru") + ":" + "PER",
		"South America:" + "Uruguay:" + Catalog.GetString("Uruguay") + ":" + "URY",
		"South America:" + "Venezuela:" + Catalog.GetString("Venezuela") + ":" + "VEN",

		//add ALWAYS below
	
	};

	/*
	/* howto countryList 

	   used this list:
	http://en.wikipedia.org/wiki/List_of_countries_by_continent_(data_file)

	with vim:
	1 selected all
	2 substitution

	:'<,'>s/\(..\) \(..\) \(...\) \(...\) \(.*$\)/\1:\5:\3
	eg.     from:   AF AO AGO 024 Angola, Republic of
        	to:     AF:Angola, Republic of:AGO

	3 !sort

	4
	:%s/AF:/Africa:
	:%s/AN:/Antarctica:
	:%s/AS:/Asia:
	:%s/EU:/Europe:
	:%s/OC:/Oceania:
	:%s/NA:/North America:
	:%s/SA:/South America:

	5 convert to usable by chronojump
	:%s/\([^:]*\):\([^:]*\):\([^:]*\)/"\1:" + "\2:" + Catalog.GetString("\2") + ":" + "\3"

	6 fix bad chars
	:%s/'/\\'/g (this doesn't work)
	solved, on insertion instead of doing:
		'" + code + "'
	we use:
		\"" + code + "\" 

	7 strip of republic, kingdom, ...
	:'<,'>s/\(.*\),\(.*\):\(.*\),\(.*\))\(.*$\)/\1:\3")\5/

	except: congo, korea, Dominica

	1.16 Cyprus has been moved from Asia to Europe	
	*/

}
