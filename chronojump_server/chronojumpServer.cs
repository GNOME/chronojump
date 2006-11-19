//llibre mono (notebook) pag 206

using System; //for environment
using System.IO;
using System.Web.Services;

using System.Collections; //ArrayList
using Mono.Data.SqliteClient;
using System.Data.SqlClient;

[WebService(Namespace="http://www.sampleFakeWeb.org/",
	Description="ChronojumpServer")]
public class ChronojumpServer {
	
	[WebMethod(Description="Conecta BBDD")]
	public string ConnectDatabase()
	{
		try {
			Sqlite.Connect();
			return "Connected";
		} catch {
			return "Unnable to connect";
		}
	}

	[WebMethod(Description="Select person name")]
	public string SelectPersonName(int personID)
	{
		//Sqlite.Connect();	
		return SqlitePerson.SelectJumperName(personID);	
	}
	
	[WebMethod(Description="See all persons")]
	//public ArrayList SelectAllPersons()
	public string[] SelectAllPersons()
	{
		//Sqlite.Connect();	
		return SqlitePerson.SelectAllPersonsRecuperable("name", -1, -1, "");	
	}

	[WebMethod(Description="Select events from all persons")]
	public ArrayList SelectAllPersonEvents(int personID) {
		//Sqlite.Connect();	
		return SqlitePerson.SelectAllPersonEvents(personID);	
	}
	
	[WebMethod(Description="List directory files (only as a sample)")]
	public string [] ListDirectory(string path) {
		return Directory.GetFileSystemEntries(path);
	}

}
