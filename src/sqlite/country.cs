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

/*
 * UNUSED BECAUSE USER WILL NOT DEFINE SPORTS BECAUSE THIS WILL MAKE PROBLEMS WITH GLOBAL SPORTS DEFINITION (NOT LOCAL) SPECIALLY IN SPORT TRANSLATIONS
 *
 * SPORTS ARE DEFINED IN constants.cs
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
	protected internal static void createTable()
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
		foreach(string myString in countries) {
			//put in db only english name
			string [] strFull = myString.Split(new char[] {':'});
			Insert(
					true,   		//dbconOpened
					strFull[3], 		//code
					strFull[1], 		//name (english)
					strFull[0]		//continent
			      );
		}
	}
	public static int Insert(bool dbconOpened, string code, string nameEnglish, string continent)
	{
		if(! dbconOpened)
			dbcon.Open();

		string myString = "INSERT INTO " + Constants.CountryTable + 
			//" (uniqueID, code, name, continent) VALUES (NULL, '" + code + "', '" + 
			//nameEnglish + "', '" + continent + "')";
			//fix bad chars (') :
			" (uniqueID, code, name, continent) VALUES (NULL, \"" + code + "\", \"" + 
			nameEnglish + "\", \"" + continent + "\")";

		dbcmd.CommandText = myString;
		dbcmd.ExecuteNonQuery();
		int myReturn = dbcon.LastInsertRowId;
		
		if(! dbconOpened)
			dbcon.Close();

		return myReturn;
	}

	/*
	public static string Select(int uniqueID)
	{
		dbcon.Open();
		
		dbcmd.CommandText = "SELECT * FROM " + Constants.CountryTable + " WHERE uniqueID == " + uniqueID;
		
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();
/*
		Sport mySport = new Sport(
				uniqueID,
				reader[1].ToString(), //name
				Util.IntToBool(Convert.ToInt32(reader[2])), //userDefined
				Util.IntToBool(Convert.ToInt32(reader[3])), //hasSpeciallities
				reader[4].ToString() //graphLink
				);
*/
	/*	
		reader.Close();
		dbcon.Close();
		return mySport;
	}
	*/

	/*
	public static int SelectID(string name)
	{
		//dbcon.Open();
		
		dbcmd.CommandText = "SELECT uniqueID FROM " + Constants.SportTable + " WHERE name == '" + name + "'";
		
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();
		int myID = Convert.ToInt32(reader[0]);
		reader.Close();
		
		//dbcon.Close();
		
		return myID;
	}
		
	public static string [] SelectAll() 
	{
		dbcon.Open();
		SqliteDataReader reader;
		ArrayList myArray = new ArrayList(2);
		int count = 0;

		dbcmd.CommandText = "SELECT * " +
			" FROM " + Constants.SportTable + " " +
			" ORDER BY name";
		
		dbcmd.ExecuteNonQuery();
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			Sport sport = new Sport(
					Convert.ToInt32(reader[0]), 
					Catalog.GetString(reader[1].ToString()), 
					Util.IntToBool(Convert.ToInt32(reader[2])),
					Util.IntToBool(Convert.ToInt32(reader[3])),
					reader[4].ToString() //graphLink
					);
			myArray.Add(sport.ToString());
			count ++;
		}
		reader.Close();
		dbcon.Close();

		string [] myReturn = new string[count];
		count = 0;
		foreach (string line in myArray) {
			myReturn [count++] = line;
		}

		return myReturn;
	}
*/

	/*
	 * the Catalog.GetString is only for having a translation that will be used on display sport name if available
	 * don't sportuserchecks here, doit in the database because sports will grow
	 * all this are obviously NOT user defined
	 * last string is for graphLink
	 */
	private static string [] countries = {
		//true or false means if it has speciallities
		Constants.CountryUndefined + ":" + Constants.CountryUndefined + ":" + Catalog.GetString(Constants.CountryUndefined) + ":" + Constants.CountryUndefined, //will be 1 (it's also written in Constants.CountryUndefinedID
		"Africa:" + "Algeria, People's Democratic Republic of:" + Catalog.GetString("Algeria, People's Democratic Republic of") + ":" + "DZA",
		"Africa:" + "Angola, Republic of:" + Catalog.GetString("Angola, Republic of") + ":" + "AGO",
		"Africa:" + "Benin, Republic of:" + Catalog.GetString("Benin, Republic of") + ":" + "BEN",
		"Africa:" + "Botswana, Republic of:" + Catalog.GetString("Botswana, Republic of") + ":" + "BWA",
		"Africa:" + "Burkina Faso:" + Catalog.GetString("Burkina Faso") + ":" + "BFA",
		"Africa:" + "Burundi, Republic of:" + Catalog.GetString("Burundi, Republic of") + ":" + "BDI",
		"Africa:" + "Cameroon, Republic of:" + Catalog.GetString("Cameroon, Republic of") + ":" + "CMR",
		"Africa:" + "Cape Verde, Republic of:" + Catalog.GetString("Cape Verde, Republic of") + ":" + "CPV",
		"Africa:" + "Central African Republic:" + Catalog.GetString("Central African Republic") + ":" + "CAF",
		"Africa:" + "Chad, Republic of:" + Catalog.GetString("Chad, Republic of") + ":" + "TCD",
		"Africa:" + "Comoros, Union of the:" + Catalog.GetString("Comoros, Union of the") + ":" + "COM",
		"Africa:" + "Congo, Democratic Republic of the:" + Catalog.GetString("Congo, Democratic Republic of the") + ":" + "COD",
		"Africa:" + "Congo, Republic of the:" + Catalog.GetString("Congo, Republic of the") + ":" + "COG",
		"Africa:" + "Cote d\'Ivoire, Republic of:" + Catalog.GetString("Cote d'Ivoire, Republic of") + ":" + "CIV",
		"Africa:" + "Djibouti, Republic of:" + Catalog.GetString("Djibouti, Republic of") + ":" + "DJI",
		"Africa:" + "Egypt, Arab Republic of:" + Catalog.GetString("Egypt, Arab Republic of") + ":" + "EGY",
		"Africa:" + "Equatorial Guinea, Republic of:" + Catalog.GetString("Equatorial Guinea, Republic of") + ":" + "GNQ",
		"Africa:" + "Eritrea, State of:" + Catalog.GetString("Eritrea, State of") + ":" + "ERI",
		"Africa:" + "Ethiopia, Federal Democratic Republic of:" + Catalog.GetString("Ethiopia, Federal Democratic Republic of") + ":" + "ETH",
		"Africa:" + "Gabon, Gabonese Republic:" + Catalog.GetString("Gabon, Gabonese Republic") + ":" + "GAB",
		"Africa:" + "Gambia, Republic of the:" + Catalog.GetString("Gambia, Republic of the") + ":" + "GMB",
		"Africa:" + "Ghana, Republic of:" + Catalog.GetString("Ghana, Republic of") + ":" + "GHA",
		"Africa:" + "Guinea-Bissau, Republic of:" + Catalog.GetString("Guinea-Bissau, Republic of") + ":" + "GNB",
		"Africa:" + "Guinea, Republic of:" + Catalog.GetString("Guinea, Republic of") + ":" + "GIN",
		"Africa:" + "Kenya, Republic of:" + Catalog.GetString("Kenya, Republic of") + ":" + "KEN",
		"Africa:" + "Lesotho, Kingdom of:" + Catalog.GetString("Lesotho, Kingdom of") + ":" + "LSO",
		"Africa:" + "Liberia, Republic of:" + Catalog.GetString("Liberia, Republic of") + ":" + "LBR",
		"Africa:" + "Libyan Arab Jamahiriya:" + Catalog.GetString("Libyan Arab Jamahiriya") + ":" + "LBY",
		"Africa:" + "Madagascar, Republic of:" + Catalog.GetString("Madagascar, Republic of") + ":" + "MDG",
		"Africa:" + "Malawi, Republic of:" + Catalog.GetString("Malawi, Republic of") + ":" + "MWI",
		"Africa:" + "Mali, Republic of:" + Catalog.GetString("Mali, Republic of") + ":" + "MLI",
		"Africa:" + "Mauritania, Islamic Republic of:" + Catalog.GetString("Mauritania, Islamic Republic of") + ":" + "MRT",
		"Africa:" + "Mauritius, Republic of:" + Catalog.GetString("Mauritius, Republic of") + ":" + "MUS",
		"Africa:" + "Mayotte:" + Catalog.GetString("Mayotte") + ":" + "MYT",
		"Africa:" + "Morocco, Kingdom of:" + Catalog.GetString("Morocco, Kingdom of") + ":" + "MAR",
		"Africa:" + "Mozambique, Republic of:" + Catalog.GetString("Mozambique, Republic of") + ":" + "MOZ",
		"Africa:" + "Namibia, Republic of:" + Catalog.GetString("Namibia, Republic of") + ":" + "NAM",
		"Africa:" + "Nigeria, Federal Republic of:" + Catalog.GetString("Nigeria, Federal Republic of") + ":" + "NGA",
		"Africa:" + "Niger, Republic of:" + Catalog.GetString("Niger, Republic of") + ":" + "NER",
		"Africa:" + "Reunion:" + Catalog.GetString("Reunion") + ":" + "REU",
		"Africa:" + "Rwanda, Republic of:" + Catalog.GetString("Rwanda, Republic of") + ":" + "RWA",
		"Africa:" + "Saint Helena:" + Catalog.GetString("Saint Helena") + ":" + "SHN",
		"Africa:" + "Sao Tome and Principe, Democratic Republic of:" + Catalog.GetString("Sao Tome and Principe, Democratic Republic of") + ":" + "STP",
		"Africa:" + "Senegal, Republic of:" + Catalog.GetString("Senegal, Republic of") + ":" + "SEN",
		"Africa:" + "Seychelles, Republic of:" + Catalog.GetString("Seychelles, Republic of") + ":" + "SYC",
		"Africa:" + "Sierra Leone, Republic of:" + Catalog.GetString("Sierra Leone, Republic of") + ":" + "SLE",
		"Africa:" + "Somalia, Somali Republic:" + Catalog.GetString("Somalia, Somali Republic") + ":" + "SOM",
		"Africa:" + "South Africa, Republic of:" + Catalog.GetString("South Africa, Republic of") + ":" + "ZAF",
		"Africa:" + "Sudan, Republic of:" + Catalog.GetString("Sudan, Republic of") + ":" + "SDN",
		"Africa:" + "Suriname, Republic of:" + Catalog.GetString("Suriname, Republic of") + ":" + "SUR",
		"Africa:" + "Swaziland, Kingdom of:" + Catalog.GetString("Swaziland, Kingdom of") + ":" + "SWZ",
		"Africa:" + "Tanzania, United Republic of:" + Catalog.GetString("Tanzania, United Republic of") + ":" + "TZA",
		"Africa:" + "Togo, Togolese Republic:" + Catalog.GetString("Togo, Togolese Republic") + ":" + "TGO",
		"Africa:" + "Tunisia, Tunisian Republic:" + Catalog.GetString("Tunisia, Tunisian Republic") + ":" + "TUN",
		"Africa:" + "Uganda, Republic of:" + Catalog.GetString("Uganda, Republic of") + ":" + "UGA",
		"Africa:" + "Western Sahara:" + Catalog.GetString("Western Sahara") + ":" + "ESH",
		"Africa:" + "Zambia, Republic of:" + Catalog.GetString("Zambia, Republic of") + ":" + "ZMB",
		"Africa:" + "Zimbabwe, Republic of:" + Catalog.GetString("Zimbabwe, Republic of") + ":" + "ZWE",
		"Antarctica:" + "Antarctica (the territory South of 60 deg S):" + Catalog.GetString("Antarctica (the territory South of 60 deg S)") + ":" + "ATA",
		"Antarctica:" + "Bouvet Island (Bouvetoya):" + Catalog.GetString("Bouvet Island (Bouvetoya)") + ":" + "BVT",
		"Antarctica:" + "French Southern Territories:" + Catalog.GetString("French Southern Territories") + ":" + "ATF",
		"Antarctica:" + "Heard Island and McDonald Islands:" + Catalog.GetString("Heard Island and McDonald Islands") + ":" + "HMD",
		"Antarctica:" + "South Georgia and the South Sandwich Islands:" + Catalog.GetString("South Georgia and the South Sandwich Islands") + ":" + "SGS",
		"Asia:" + "Afghanistan, Islamic Republic of:" + Catalog.GetString("Afghanistan, Islamic Republic of") + ":" + "AFG",
		"Asia:" + "Armenia, Republic of:" + Catalog.GetString("Armenia, Republic of") + ":" + "ARM",
		"Asia:" + "Azerbaijan, Republic of:" + Catalog.GetString("Azerbaijan, Republic of") + ":" + "AZE",
		"Asia:" + "Bahrain, Kingdom of:" + Catalog.GetString("Bahrain, Kingdom of") + ":" + "BHR",
		"Asia:" + "Bangladesh, People's Republic of:" + Catalog.GetString("Bangladesh, People's Republic of") + ":" + "BGD",
		"Asia:" + "Bhutan, Kingdom of:" + Catalog.GetString("Bhutan, Kingdom of") + ":" + "BTN",
		"Asia:" + "British Indian Ocean Territory (Chagos Archipelago):" + Catalog.GetString("British Indian Ocean Territory (Chagos Archipelago)") + ":" + "IOT",
		"Asia:" + "Brunei Darussalam:" + Catalog.GetString("Brunei Darussalam") + ":" + "BRN",
		"Asia:" + "Cambodia, Kingdom of:" + Catalog.GetString("Cambodia, Kingdom of") + ":" + "KHM",
		"Asia:" + "China, People's Republic of:" + Catalog.GetString("China, People's Republic of") + ":" + "CHN",
		"Asia:" + "Christmas Island:" + Catalog.GetString("Christmas Island") + ":" + "CXR",
		"Asia:" + "Cocos (Keeling) Islands:" + Catalog.GetString("Cocos (Keeling) Islands") + ":" + "CCK",
		"Asia:" + "Cyprus, Republic of:" + Catalog.GetString("Cyprus, Republic of") + ":" + "CYP",
		"Asia:" + "Georgia:" + Catalog.GetString("Georgia") + ":" + "GEO",
		"Asia:" + "Hong Kong, Special Administrative Region of China:" + Catalog.GetString("Hong Kong, Special Administrative Region of China") + ":" + "HKG",
		"Asia:" + "India, Republic of:" + Catalog.GetString("India, Republic of") + ":" + "IND",
		"Asia:" + "Indonesia, Republic of:" + Catalog.GetString("Indonesia, Republic of") + ":" + "IDN",
		"Asia:" + "Iran, Islamic Republic of:" + Catalog.GetString("Iran, Islamic Republic of") + ":" + "IRN",
		"Asia:" + "Iraq, Republic of:" + Catalog.GetString("Iraq, Republic of") + ":" + "IRQ",
		"Asia:" + "Israel, State of:" + Catalog.GetString("Israel, State of") + ":" + "ISR",
		"Asia:" + "Japan:" + Catalog.GetString("Japan") + ":" + "JPN",
		"Asia:" + "Jordan, Hashemite Kingdom of:" + Catalog.GetString("Jordan, Hashemite Kingdom of") + ":" + "JOR",
		"Asia:" + "Kazakhstan, Republic of:" + Catalog.GetString("Kazakhstan, Republic of") + ":" + "KAZ",
		"Asia:" + "Korea, Democratic People's Republic of:" + Catalog.GetString("Korea, Democratic People's Republic of") + ":" + "PRK",
		"Asia:" + "Korea, Republic of:" + Catalog.GetString("Korea, Republic of") + ":" + "KOR",
		"Asia:" + "Kuwait, State of:" + Catalog.GetString("Kuwait, State of") + ":" + "KWT",
		"Asia:" + "Kyrgyz Republic:" + Catalog.GetString("Kyrgyz Republic") + ":" + "KGZ",
		"Asia:" + "Lao People's Democratic Republic:" + Catalog.GetString("Lao People's Democratic Republic") + ":" + "LAO",
		"Asia:" + "Lebanon, Lebanese Republic:" + Catalog.GetString("Lebanon, Lebanese Republic") + ":" + "LBN",
		"Asia:" + "Macao, Special Administrative Region of China:" + Catalog.GetString("Macao, Special Administrative Region of China") + ":" + "MAC",
		"Asia:" + "Malaysia:" + Catalog.GetString("Malaysia") + ":" + "MYS",
		"Asia:" + "Maldives, Republic of:" + Catalog.GetString("Maldives, Republic of") + ":" + "MDV",
		"Asia:" + "Mongolia:" + Catalog.GetString("Mongolia") + ":" + "MNG",
		"Asia:" + "Myanmar, Union of:" + Catalog.GetString("Myanmar, Union of") + ":" + "MMR",
		"Asia:" + "Nepal, State of:" + Catalog.GetString("Nepal, State of") + ":" + "NPL",
		"Asia:" + "Oman, Sultanate of:" + Catalog.GetString("Oman, Sultanate of") + ":" + "OMN",
		"Asia:" + "Pakistan, Islamic Republic of:" + Catalog.GetString("Pakistan, Islamic Republic of") + ":" + "PAK",
		"Asia:" + "Palestinian Territory, Occupied:" + Catalog.GetString("Palestinian Territory, Occupied") + ":" + "PSE",
		"Asia:" + "Philippines, Republic of the:" + Catalog.GetString("Philippines, Republic of the") + ":" + "PHL",
		"Asia:" + "Qatar, State of:" + Catalog.GetString("Qatar, State of") + ":" + "QAT",
		"Asia:" + "Saudi Arabia, Kingdom of:" + Catalog.GetString("Saudi Arabia, Kingdom of") + ":" + "SAU",
		"Asia:" + "Singapore, Republic of:" + Catalog.GetString("Singapore, Republic of") + ":" + "SGP",
		"Asia:" + "Sri Lanka, Democratic Socialist Republic of:" + Catalog.GetString("Sri Lanka, Democratic Socialist Republic of") + ":" + "LKA",
		"Asia:" + "Syrian Arab Republic:" + Catalog.GetString("Syrian Arab Republic") + ":" + "SYR",
		"Asia:" + "Taiwan:" + Catalog.GetString("Taiwan") + ":" + "TWN",
		"Asia:" + "Tajikistan, Republic of:" + Catalog.GetString("Tajikistan, Republic of") + ":" + "TJK",
		"Asia:" + "Thailand, Kingdom of:" + Catalog.GetString("Thailand, Kingdom of") + ":" + "THA",
		"Asia:" + "Timor-Leste, Democratic Republic of:" + Catalog.GetString("Timor-Leste, Democratic Republic of") + ":" + "TLS",
		"Asia:" + "Turkey, Republic of:" + Catalog.GetString("Turkey, Republic of") + ":" + "TUR",
		"Asia:" + "Turkmenistan:" + Catalog.GetString("Turkmenistan") + ":" + "TKM",
		"Asia:" + "United Arab Emirates:" + Catalog.GetString("United Arab Emirates") + ":" + "ARE",
		"Asia:" + "Uzbekistan, Republic of:" + Catalog.GetString("Uzbekistan, Republic of") + ":" + "UZB",
		"Asia:" + "Vietnam, Socialist Republic of:" + Catalog.GetString("Vietnam, Socialist Republic of") + ":" + "VNM",
		"Asia:" + "Yemen:" + Catalog.GetString("Yemen") + ":" + "YEM",
		"Europe:" + "Åland Islands:" + Catalog.GetString("Åland Islands") + ":" + "ALA",
		"Europe:" + "Albania, Republic of:" + Catalog.GetString("Albania, Republic of") + ":" + "ALB",
		"Europe:" + "Andorra, Principality of:" + Catalog.GetString("Andorra, Principality of") + ":" + "AND",
		"Europe:" + "Austria, Republic of:" + Catalog.GetString("Austria, Republic of") + ":" + "AUT",
		"Europe:" + "Belarus, Republic of:" + Catalog.GetString("Belarus, Republic of") + ":" + "BLR",
		"Europe:" + "Belgium, Kingdom of:" + Catalog.GetString("Belgium, Kingdom of") + ":" + "BEL",
		"Europe:" + "Bosnia and Herzegovina:" + Catalog.GetString("Bosnia and Herzegovina") + ":" + "BIH",
		"Europe:" + "Bulgaria, Republic of:" + Catalog.GetString("Bulgaria, Republic of") + ":" + "BGR",
		"Europe:" + "Croatia, Republic of:" + Catalog.GetString("Croatia, Republic of") + ":" + "HRV",
		"Europe:" + "Czech Republic:" + Catalog.GetString("Czech Republic") + ":" + "CZE",
		"Europe:" + "Denmark, Kingdom of:" + Catalog.GetString("Denmark, Kingdom of") + ":" + "DNK",
		"Europe:" + "Estonia, Republic of:" + Catalog.GetString("Estonia, Republic of") + ":" + "EST",
		"Europe:" + "Faroe Islands:" + Catalog.GetString("Faroe Islands") + ":" + "FRO",
		"Europe:" + "Finland, Republic of:" + Catalog.GetString("Finland, Republic of") + ":" + "FIN",
		"Europe:" + "France, French Republic:" + Catalog.GetString("France, French Republic") + ":" + "FRA",
		"Europe:" + "Germany, Federal Republic of:" + Catalog.GetString("Germany, Federal Republic of") + ":" + "DEU",
		"Europe:" + "Gibraltar:" + Catalog.GetString("Gibraltar") + ":" + "GIB",
		"Europe:" + "Greece, Hellenic Republic:" + Catalog.GetString("Greece, Hellenic Republic") + ":" + "GRC",
		"Europe:" + "Guernsey, Bailiwick of:" + Catalog.GetString("Guernsey, Bailiwick of") + ":" + "GGY",
		"Europe:" + "Holy See (Vatican City State):" + Catalog.GetString("Holy See (Vatican City State)") + ":" + "VAT",
		"Europe:" + "Hungary, Republic of:" + Catalog.GetString("Hungary, Republic of") + ":" + "HUN",
		"Europe:" + "Iceland, Republic of:" + Catalog.GetString("Iceland, Republic of") + ":" + "ISL",
		"Europe:" + "Ireland:" + Catalog.GetString("Ireland") + ":" + "IRL",
		"Europe:" + "Isle of Man:" + Catalog.GetString("Isle of Man") + ":" + "IMN",
		"Europe:" + "Italy, Italian Republic:" + Catalog.GetString("Italy, Italian Republic") + ":" + "ITA",
		"Europe:" + "Jersey, Bailiwick of:" + Catalog.GetString("Jersey, Bailiwick of") + ":" + "JEY",
		"Europe:" + "Latvia, Republic of:" + Catalog.GetString("Latvia, Republic of") + ":" + "LVA",
		"Europe:" + "Liechtenstein, Principality of:" + Catalog.GetString("Liechtenstein, Principality of") + ":" + "LIE",
		"Europe:" + "Lithuania, Republic of:" + Catalog.GetString("Lithuania, Republic of") + ":" + "LTU",
		"Europe:" + "Luxembourg, Grand Duchy of:" + Catalog.GetString("Luxembourg, Grand Duchy of") + ":" + "LUX",
		"Europe:" + "Macedonia, the former Yugoslav Republic of:" + Catalog.GetString("Macedonia, the former Yugoslav Republic of") + ":" + "MKD",
		"Europe:" + "Malta, Republic of:" + Catalog.GetString("Malta, Republic of") + ":" + "MLT",
		"Europe:" + "Moldova, Republic of:" + Catalog.GetString("Moldova, Republic of") + ":" + "MDA",
		"Europe:" + "Monaco, Principality of:" + Catalog.GetString("Monaco, Principality of") + ":" + "MCO",
		"Europe:" + "Montenegro, Republic of:" + Catalog.GetString("Montenegro, Republic of") + ":" + "MNE",
		"Europe:" + "Netherlands, Kingdom of the:" + Catalog.GetString("Netherlands, Kingdom of the") + ":" + "NLD",
		"Europe:" + "Norway, Kingdom of:" + Catalog.GetString("Norway, Kingdom of") + ":" + "NOR",
		"Europe:" + "Poland, Republic of:" + Catalog.GetString("Poland, Republic of") + ":" + "POL",
		"Europe:" + "Portugal, Portuguese Republic:" + Catalog.GetString("Portugal, Portuguese Republic") + ":" + "PRT",
		"Europe:" + "Romania:" + Catalog.GetString("Romania") + ":" + "ROU",
		"Europe:" + "Russian Federation:" + Catalog.GetString("Russian Federation") + ":" + "RUS",
		"Europe:" + "San Marino, Republic of:" + Catalog.GetString("San Marino, Republic of") + ":" + "SMR",
		"Europe:" + "Serbia, Republic of:" + Catalog.GetString("Serbia, Republic of") + ":" + "SRB",
		"Europe:" + "Slovakia (Slovak Republic):" + Catalog.GetString("Slovakia (Slovak Republic)") + ":" + "SVK",
		"Europe:" + "Slovenia, Republic of:" + Catalog.GetString("Slovenia, Republic of") + ":" + "SVN",
		"Europe:" + "Spain, Kingdom of:" + Catalog.GetString("Spain, Kingdom of") + ":" + "ESP",
		"Europe:" + "Svalbard & Jan Mayen Islands:" + Catalog.GetString("Svalbard & Jan Mayen Islands") + ":" + "SJM",
		"Europe:" + "Sweden, Kingdom of:" + Catalog.GetString("Sweden, Kingdom of") + ":" + "SWE",
		"Europe:" + "Switzerland, Swiss Confederation:" + Catalog.GetString("Switzerland, Swiss Confederation") + ":" + "CHE",
		"Europe:" + "Ukraine:" + Catalog.GetString("Ukraine") + ":" + "UKR",
		"Europe:" + "United Kingdom of Great Britain & Northern Ireland:" + Catalog.GetString("United Kingdom of Great Britain & Northern Ireland") + ":" + "GBR",
		"North America:" + "Anguilla:" + Catalog.GetString("Anguilla") + ":" + "AIA",
		"North America:" + "Antigua and Barbuda:" + Catalog.GetString("Antigua and Barbuda") + ":" + "ATG",
		"North America:" + "Aruba:" + Catalog.GetString("Aruba") + ":" + "ABW",
		"North America:" + "Bahamas, Commonwealth of the:" + Catalog.GetString("Bahamas, Commonwealth of the") + ":" + "BHS",
		"North America:" + "Barbados:" + Catalog.GetString("Barbados") + ":" + "BRB",
		"North America:" + "Belize:" + Catalog.GetString("Belize") + ":" + "BLZ",
		"North America:" + "Bermuda:" + Catalog.GetString("Bermuda") + ":" + "BMU",
		"North America:" + "British Virgin Islands:" + Catalog.GetString("British Virgin Islands") + ":" + "VGB",
		"North America:" + "Canada:" + Catalog.GetString("Canada") + ":" + "CAN",
		"North America:" + "Cayman Islands:" + Catalog.GetString("Cayman Islands") + ":" + "CYM",
		"North America:" + "Costa Rica, Republic of:" + Catalog.GetString("Costa Rica, Republic of") + ":" + "CRI",
		"North America:" + "Cuba, Republic of:" + Catalog.GetString("Cuba, Republic of") + ":" + "CUB",
		"North America:" + "Dominica, Commonwealth of:" + Catalog.GetString("Dominica, Commonwealth of") + ":" + "DMA",
		"North America:" + "Dominican Republic:" + Catalog.GetString("Dominican Republic") + ":" + "DOM",
		"North America:" + "El Salvador, Republic of:" + Catalog.GetString("El Salvador, Republic of") + ":" + "SLV",
		"North America:" + "Greenland:" + Catalog.GetString("Greenland") + ":" + "GRL",
		"North America:" + "Grenada:" + Catalog.GetString("Grenada") + ":" + "GRD",
		"North America:" + "Guadeloupe:" + Catalog.GetString("Guadeloupe") + ":" + "GLP",
		"North America:" + "Guatemala, Republic of:" + Catalog.GetString("Guatemala, Republic of") + ":" + "GTM",
		"North America:" + "Haiti, Republic of:" + Catalog.GetString("Haiti, Republic of") + ":" + "HTI",
		"North America:" + "Honduras, Republic of:" + Catalog.GetString("Honduras, Republic of") + ":" + "HND",
		"North America:" + "Jamaica:" + Catalog.GetString("Jamaica") + ":" + "JAM",
		"North America:" + "Martinique:" + Catalog.GetString("Martinique") + ":" + "MTQ",
		"North America:" + "Mexico, United Mexican States:" + Catalog.GetString("Mexico, United Mexican States") + ":" + "MEX",
		"North America:" + "Montserrat:" + Catalog.GetString("Montserrat") + ":" + "MSR",
		"North America:" + "Netherlands Antilles:" + Catalog.GetString("Netherlands Antilles") + ":" + "ANT",
		"North America:" + "Nicaragua, Republic of:" + Catalog.GetString("Nicaragua, Republic of") + ":" + "NIC",
		"North America:" + "Panama, Republic of:" + Catalog.GetString("Panama, Republic of") + ":" + "PAN",
		"North America:" + "Puerto Rico, Commonwealth of:" + Catalog.GetString("Puerto Rico, Commonwealth of") + ":" + "PRI",
		"North America:" + "Saint Barthelemy:" + Catalog.GetString("Saint Barthelemy") + ":" + "BLM",
		"North America:" + "Saint Kitts and Nevis, Federation of:" + Catalog.GetString("Saint Kitts and Nevis, Federation of") + ":" + "KNA",
		"North America:" + "Saint Lucia:" + Catalog.GetString("Saint Lucia") + ":" + "LCA",
		"North America:" + "Saint Martin:" + Catalog.GetString("Saint Martin") + ":" + "MAF",
		"North America:" + "Saint Pierre and Miquelon:" + Catalog.GetString("Saint Pierre and Miquelon") + ":" + "SPM",
		"North America:" + "Saint Vincent and the Grenadines:" + Catalog.GetString("Saint Vincent and the Grenadines") + ":" + "VCT",
		"North America:" + "Trinidad and Tobago, Republic of:" + Catalog.GetString("Trinidad and Tobago, Republic of") + ":" + "TTO",
		"North America:" + "Turks and Caicos Islands:" + Catalog.GetString("Turks and Caicos Islands") + ":" + "TCA",
		"North America:" + "United States of America:" + Catalog.GetString("United States of America") + ":" + "USA",
		"North America:" + "United States Virgin Islands:" + Catalog.GetString("United States Virgin Islands") + ":" + "VIR",
		"Oceania:" + "American Samoa:" + Catalog.GetString("American Samoa") + ":" + "ASM",
		"Oceania:" + "Australia, Commonwealth of:" + Catalog.GetString("Australia, Commonwealth of") + ":" + "AUS",
		"Oceania:" + "Cook Islands:" + Catalog.GetString("Cook Islands") + ":" + "COK",
		"Oceania:" + "Fiji, Republic of the Fiji Islands:" + Catalog.GetString("Fiji, Republic of the Fiji Islands") + ":" + "FJI",
		"Oceania:" + "French Polynesia:" + Catalog.GetString("French Polynesia") + ":" + "PYF",
		"Oceania:" + "Guam:" + Catalog.GetString("Guam") + ":" + "GUM",
		"Oceania:" + "Kiribati, Republic of:" + Catalog.GetString("Kiribati, Republic of") + ":" + "KIR",
		"Oceania:" + "Marshall Islands, Republic of the:" + Catalog.GetString("Marshall Islands, Republic of the") + ":" + "MHL",
		"Oceania:" + "Micronesia, Federated States of:" + Catalog.GetString("Micronesia, Federated States of") + ":" + "FSM",
		"Oceania:" + "Nauru, Republic of:" + Catalog.GetString("Nauru, Republic of") + ":" + "NRU",
		"Oceania:" + "New Caledonia:" + Catalog.GetString("New Caledonia") + ":" + "NCL",
		"Oceania:" + "New Zealand:" + Catalog.GetString("New Zealand") + ":" + "NZL",
		"Oceania:" + "Niue:" + Catalog.GetString("Niue") + ":" + "NIU",
		"Oceania:" + "Norfolk Island:" + Catalog.GetString("Norfolk Island") + ":" + "NFK",
		"Oceania:" + "Northern Mariana Islands, Commonwealth of the:" + Catalog.GetString("Northern Mariana Islands, Commonwealth of the") + ":" + "MNP",
		"Oceania:" + "Palau, Republic of:" + Catalog.GetString("Palau, Republic of") + ":" + "PLW",
		"Oceania:" + "Papua New Guinea, Independent State of:" + Catalog.GetString("Papua New Guinea, Independent State of") + ":" + "PNG",
		"Oceania:" + "Pitcairn Islands:" + Catalog.GetString("Pitcairn Islands") + ":" + "PCN",
		"Oceania:" + "Samoa, Independent State of:" + Catalog.GetString("Samoa, Independent State of") + ":" + "WSM",
		"Oceania:" + "Solomon Islands:" + Catalog.GetString("Solomon Islands") + ":" + "SLB",
		"Oceania:" + "Tokelau:" + Catalog.GetString("Tokelau") + ":" + "TKL",
		"Oceania:" + "Tonga, Kingdom of:" + Catalog.GetString("Tonga, Kingdom of") + ":" + "TON",
		"Oceania:" + "Tuvalu:" + Catalog.GetString("Tuvalu") + ":" + "TUV",
		"Oceania:" + "United States Minor Outlying Islands:" + Catalog.GetString("United States Minor Outlying Islands") + ":" + "UMI",
		"Oceania:" + "Vanuatu, Republic of:" + Catalog.GetString("Vanuatu, Republic of") + ":" + "VUT",
		"Oceania:" + "Wallis and Futuna:" + Catalog.GetString("Wallis and Futuna") + ":" + "WLF",
		"South America:" + "Argentina, Argentine Republic:" + Catalog.GetString("Argentina, Argentine Republic") + ":" + "ARG",
		"South America:" + "Bolivia, Republic of:" + Catalog.GetString("Bolivia, Republic of") + ":" + "BOL",
		"South America:" + "Brazil, Federative Republic of:" + Catalog.GetString("Brazil, Federative Republic of") + ":" + "BRA",
		"South America:" + "Chile, Republic of:" + Catalog.GetString("Chile, Republic of") + ":" + "CHL",
		"South America:" + "Colombia, Republic of:" + Catalog.GetString("Colombia, Republic of") + ":" + "COL",
		"South America:" + "Ecuador, Republic of:" + Catalog.GetString("Ecuador, Republic of") + ":" + "ECU",
		"South America:" + "Falkland Islands (Malvinas):" + Catalog.GetString("Falkland Islands (Malvinas)") + ":" + "FLK",
		"South America:" + "French Guiana:" + Catalog.GetString("French Guiana") + ":" + "GUF",
		"South America:" + "Guyana, Co-operative Republic of:" + Catalog.GetString("Guyana, Co-operative Republic of") + ":" + "GUY",
		"South America:" + "Paraguay, Republic of:" + Catalog.GetString("Paraguay, Republic of") + ":" + "PRY",
		"South America:" + "Peru, Republic of:" + Catalog.GetString("Peru, Republic of") + ":" + "PER",
		"South America:" + "Uruguay, Eastern Republic of:" + Catalog.GetString("Uruguay, Eastern Republic of") + ":" + "URY",
		"South America:" + "Venezuela, Bolivarian Republic of:" + Catalog.GetString("Venezuela, Bolivarian Republic of") + ":" + "VEN",
		//add ALWAYS below
	};

	//dumb variables to translate countries
	private static string ctr1 = Catalog.GetString("Africa");
	private static string ctr2 = Catalog.GetString("Antarctica");
	private static string ctr3 = Catalog.GetString("Asia");
	private static string ctr4 = Catalog.GetString("Europe");
	private static string ctr5 = Catalog.GetString("North America");
	private static string ctr6 = Catalog.GetString("Oceania");
	private static string ctr7 = Catalog.GetString("South America");
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

	
	*/

}
