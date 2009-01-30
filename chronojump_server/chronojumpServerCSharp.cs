//llibre mono (notebook) pag 206

using System; //for environment
using System.IO;
using System.Web.Services;
//using System.Web;

using System.Collections; //ArrayList

//[WebService(Namespace="http://80.32.81.197:8080/", //works to connect with pinux xen from client (from browser don't works)
[WebService(Namespace="http://localhost:8080/", //work to connect to corall development from client (from browser works only when online)
	Description="ChronojumpServer")]
public class ChronojumpServer {
	
	[WebMethod(Description="Conecta BBDD")]
	public string ConnectDatabase()
	{
		try {
			Sqlite.ConnectServer();
			return "Connected";
		} catch {
			return "Unnable to connect";
		}
	}
	
	[WebMethod(Description="Desconecta BBDD")]
	public string DisConnectDatabase()
	{
		try {
			Sqlite.DisConnect();
			return "Disconnected";
		} catch {
			return "Unnable to disconnect";
		}
	}

	[WebMethod(Description="Stats")]
	public ArrayList Stats()
	{
		ArrayList stats = SqliteServer.Stats();

		return stats;	
	}

	[WebMethod(Description="Upload a session")]
	public int UploadSession(ServerSession mySession)
	{
		Console.WriteLine(mySession.ToString());
	
		int id = mySession.InsertAtDB(false, Constants.SessionTable);
	
		return id; //uniqueID of person at server
	}
	
	[WebMethod(Description="Update session uploadingState")]
	public int UpdateSession(int sessionID, Constants.ServerSessionStates state)
	{
		SqliteServerSession.UpdateUploadingState(sessionID, state);
		
		return 1;
	}
	
	[WebMethod(Description="Upload an sport (user defined)")]
	public int UploadSport(Sport mySport)
	{
		int id = -1;
		//upload if doesn't exists (uploaded before by this or other evaluator)
		if(! Sqlite.Exists(Constants.SportTable, mySport.Name))
			id = mySport.InsertAtDB(false);
		
		return id; //uniqueID of sport at server
	}
	
	[WebMethod(Description="Upload a test type (user defined)")]
	public string UploadTestType(Constants.TestTypes testType, EventType type, int evalSID)
	{
		string typeServer = type.Name + "-" + evalSID.ToString();

		/*
		//upload if doesn't exists (uploaded before by this evaluator)
		//Console.WriteLine(typeServer + ":" + type.StartIn + ":" + type.HasWeight + ":" + type.Description);
		if(! Sqlite.Exists(Constants.JumpTypeTable, typeServer)) {
			//Console.WriteLine("Jump type doesn't exists");
			SqliteJumpType.JumpTypeInsert(
					typeServer + ":" + Util.BoolToInt(type.StartIn).ToString() + ":" + 
					Util.BoolToInt(type.HasWeight).ToString() + ":" + type.Description,
					false);
		}
		*/
		Console.WriteLine("XXXXXXXXXXXXXXXX");
		bool inserted = false;
		switch (testType) {
			case Constants.TestTypes.JUMP :
				JumpType jumpType = (JumpType)type;
				Console.WriteLine("JUMP" + typeServer + ":" + jumpType.StartIn + ":" + jumpType.HasWeight + ":" + jumpType.Description);
				if(! Sqlite.Exists(Constants.JumpTypeTable, typeServer)) {
					Console.WriteLine("YYYYYYYYYYYYYYYY");
					//Console.WriteLine("Jump type doesn't exists");
					SqliteJumpType.JumpTypeInsert(
							typeServer + ":" + Util.BoolToInt(jumpType.StartIn).ToString() + ":" + 
							Util.BoolToInt(jumpType.HasWeight).ToString() + ":" + jumpType.Description,
							false);
					inserted = true;
				}
				break;
			case Constants.TestTypes.JUMP_RJ :
				JumpType jumpTypeRj = (JumpType)type;
				Console.WriteLine("JUMP_RJ" + typeServer + ":" + jumpTypeRj.Description);
				if(! Sqlite.Exists(Constants.JumpRjTypeTable, typeServer)) {
					//Console.WriteLine("JumpRj type doesn't exists");
					SqliteJumpType.JumpRjTypeInsert(
							typeServer + ":" + Util.BoolToInt(jumpTypeRj.StartIn).ToString() + ":" + 
							Util.BoolToInt(jumpTypeRj.HasWeight).ToString() + ":" + 
							Util.BoolToInt(jumpTypeRj.JumpsLimited).ToString() + ":" + 
							jumpTypeRj.FixedValue.ToString() + ":" + 
							jumpTypeRj.Description,
							false);
					inserted = true;
				}
				break;
		}
					
		Console.WriteLine("zzzzzzzzzzzzzzzzzzzzzzzz");

		if(inserted)
			return typeServer;
		else
			return "-1";
	}

	
	[WebMethod(Description="Upload a person")]
	public int UploadPerson(Person myPerson, int sessionID)
	{
		//store person uniqueID
		int temp = myPerson.UniqueID;

		//change value for being inserted with new numeration in server
		myPerson.UniqueID = -1;
		
		//do insertion
		int id = myPerson.InsertAtDB(false, Constants.PersonTable);
		
		//roll back person unique id value
		myPerson.UniqueID = temp;

		Console.WriteLine("id at server: " + id);

		return id; //uniqueID of person at server
	}
	
	[WebMethod(Description="Upload person session if needed")]
	public int UploadPersonSessionIfNeeded(int personServerID, int sessionServerID, int weight)
	{
		if(!SqlitePersonSession.PersonSelectExistsInSession(personServerID, sessionServerID)) {
			Console.WriteLine("personSession needed");
			SqlitePersonSession.Insert (personServerID, sessionServerID, weight);
			Console.WriteLine("done");
			return 1; //unused
		} else 
			Console.WriteLine("personSession NOT needed");
		return 0; //unused
	}
	
	[WebMethod(Description="Upload a ping")]
	public int UploadPing(ServerPing myPing, bool doInsertion)
	{
		//problemes getting user ip:
		//when it works it should be assigned to myPing.IP
		//string a = Request.UserHostName;
		//Console.WriteLine(System.Web.HttpRequest.UserHostAdress);

		Console.WriteLine("ping string: " + myPing.ToString());

		//!doInsertion is a way to know if server is connected
		//but without inserting nothing
		//is ok before uploading a session

		if(doInsertion) {
			int id = myPing.InsertAtDB(false);
			return id;
		} else
			return -1;
	}

	[WebMethod(Description="Upload a evaluator")]
	public int UploadEvaluator(ServerEvaluator myEval)
	{
		Console.WriteLine("eval string: " + myEval.ToString());

		//do insertion
		int id = myEval.InsertAtDB(false);
		
		return id;
	}
	
	[WebMethod(Description="Upload an array")]
	public int UploadArray (ArrayList array)
	{
		//funciona
		//foreach(int num in array)
		//	Console.Write(num.ToString() + "\t");
		/*
		//funciona
		foreach(string str in array)
			Console.Write(str + "\t");
			*/
		//funciona
		foreach(Event myEvent in array)
			Console.WriteLine(myEvent.Prova() + "\t");
		//no funciona
		//foreach(Jump jump in array)
		//	Console.WriteLine(jump.Prova() + "\t");
		
		return 1;
	}

	[WebMethod(Description="Upload a test")]
	public int UploadTest (Event myTest, Constants.TestTypes type, string tableName)
	{
		//store event uniqueID
		int temp = myTest.UniqueID;

		//change value for being inserted with new numeration in server
		myTest.UniqueID = -1;

		//insert
		int id = 0;
		switch (type) {
			case Constants.TestTypes.JUMP :
				Jump jump = (Jump)myTest;
				id = jump.InsertAtDB(false, tableName);
				break;
			case Constants.TestTypes.JUMP_RJ :
				JumpRj jumpRj = (JumpRj)myTest;
				id = jumpRj.InsertAtDB(false, tableName);
				break;
			case Constants.TestTypes.RUN :
				Run run = (Run)myTest;
				id = run.InsertAtDB(false, tableName);
				break;
			case Constants.TestTypes.RUN_I :
				RunInterval runI = (RunInterval)myTest;
				id = runI.InsertAtDB(false, tableName);
				break;
			case Constants.TestTypes.RT :
				ReactionTime rt = (ReactionTime)myTest;
				id = rt.InsertAtDB(false, tableName);
				break;
			case Constants.TestTypes.PULSE :
				Pulse pulse = (Pulse)myTest;
				id = pulse.InsertAtDB(false, tableName);
				break;
		}

		//roll back person unique id value
		myTest.UniqueID = temp;

		return id;
	}

	/*
	[WebMethod(Description="Upload a jump")]
	public int UploadJump (Jump myTest)
	{
		//store event uniqueID
		int temp = myTest.UniqueID;

		//change value for being inserted with new numeration in server
		myTest.UniqueID = -1;

		//insert
		int id = myTest.InsertAtDB(false, Constants.JumpTable);
		
		//roll back person unique id value
		myTest.UniqueID = temp;

		return id; //uniqueID of person at server
	}
	*/

	[WebMethod(Description="Upload a jumpRj")]
	public int UploadJumpRj (JumpRj myTest)
	{
		/*
		//store event uniqueID
		int temp = myTest.UniqueID;

		//change value for being inserted with new numeration in server
		myTest.UniqueID = -1;

		//insert
		int id = myTest.InsertAtDB(false, Constants.JumpRjTable);
		
		//roll back person unique id value
		myTest.UniqueID = temp;

		return id; //uniqueID of person at server
		*/
		return 1;
	}

	[WebMethod(Description="Upload a run")]
	public int UploadRun (Run myTest)
	{
		/*
		//store event uniqueID
		int temp = myTest.UniqueID;

		//change value for being inserted with new numeration in server
		myTest.UniqueID = -1;

		//insert
		int id = myTest.InsertAtDB(false, Constants.RunTable);
		
		//roll back person unique id value
		myTest.UniqueID = temp;
		
		return id; //uniqueID of person at server
		*/

		return 1;
	}
	
	
	[WebMethod(Description="Upload a run interval")]
	public int UploadRunI (RunInterval myTest)
	{
		/*
		//store event uniqueID
		int temp = myTest.UniqueID;

		//change value for being inserted with new numeration in server
		myTest.UniqueID = -1;

		//insert
		int id = myTest.InsertAtDB(false, Constants.RunIntervalTable);
		
		//roll back person unique id value
		myTest.UniqueID = temp;

		return id; //uniqueID of person at server
		*/
		return 1;
	}
	
	/*
	[WebMethod(Description="Upload a reaction time")]
	public int UploadRT (ReactionTime myTest)
	{
		//store event uniqueID
		int temp = myTest.UniqueID;

		//change value for being inserted with new numeration in server
		myTest.UniqueID = -1;

		//insert
		int id = myTest.InsertAtDB(false, Constants.ReactionTimeTable);
		
		//roll back person unique id value
		myTest.UniqueID = temp;

		return id; //uniqueID of person at server
	}
	
	[WebMethod(Description="Upload a pulse")]
	public int UploadPulse (Pulse myTest)
	{
		//store event uniqueID
		int temp = myTest.UniqueID;

		//change value for being inserted with new numeration in server
		myTest.UniqueID = -1;

		//insert
		int id = myTest.InsertAtDB(false, Constants.PulseTable);
		
		//roll back person unique id value
		myTest.UniqueID = temp;

		return id; //uniqueID of person at server
	}
	*/	

	[WebMethod(Description="hola")]
	public int Hola(string text, int id) {
		Console.WriteLine(text + " hola " + id.ToString());
		return 1;
	}
	
	/*	
	[WebMethod(Description="hola2")]
	public int Hola2(string text, Jump jump) {
		Console.WriteLine(text + " hola2" + jump.UniqueID.ToString() + " " + jump.Description + "/" + jump.Tv.ToString() );
		return 1;
	}
	*/

/*	
	[WebMethod(Description="hola3")]
	public int Hola3(string text, Event jumpi) {
		Jump jump2 = (Jump)jumpi;
		Console.WriteLine(text + " hola3" + jump2.UniqueID.ToString() + " " + jump2.Description + "/" + jump2.Tv.ToString() );
		return 1;
	}
	*/

	[WebMethod(Description="hola5")]
	public int Hola5(string text, Event myTest, Constants.TestTypes type) {
		switch (type) {
			case Constants.TestTypes.JUMP :
				Jump jump = (Jump)myTest;
				Console.WriteLine(text + " hola5 jump" + jump.UniqueID.ToString() + " " + jump.Description + "/" + jump.Tv.ToString() );
				break;
			case Constants.TestTypes.JUMP_RJ :
				JumpRj jumpRj = (JumpRj)myTest;
				Console.WriteLine(text + " hola5 jumpRj" + jumpRj.UniqueID.ToString() + " " + jumpRj.Description + "/" + jumpRj.TvString );
				break;
			case Constants.TestTypes.RUN :
				Run run = (Run)myTest;
				Console.WriteLine(text + " hola5 run" + run.UniqueID.ToString() + " " + run.Description + "/" + run.Time.ToString() );
				break;
			case Constants.TestTypes.RUN_I :
				RunInterval runI = (RunInterval)myTest;
				Console.WriteLine(text + " hola5 runI" + runI.UniqueID.ToString() + " " + runI.Description + "/" + runI.TimeTotal.ToString() );
				break;
				/*
			case Constants.TestTypes.RT :
				ReactionTime rt = (ReactionTime)myTest;
				id = rt.InsertAtDB(false, tableName);
				break;
			case Constants.TestTypes.PULSE :
				Pulse pulse = (Pulse)myTest;
				id = pulse.InsertAtDB(false, tableName);
				break;
				*/
		}

		return 1;
	}
	
	
	
	[WebMethod(Description="List directory files (only as a sample)")]
	public string [] ListDirectory(string path) {
		return Directory.GetFileSystemEntries(path);
	}

	/*
	[WebMethod(Description="Select person name")]
	public string SelectPersonName(int personID)
	{
		return SqlitePerson.SelectJumperName(personID);	
	}
	*/	

	/*
	[WebMethod(Description="See all persons")]
	public ArrayList SelectAllPersons()
	{
		return SqlitePerson.SelectAllPersons();	
	}

	[WebMethod(Description="Select events from all persons")]
	public ArrayList SelectAllPersonEvents(int personID) {
		return SqlitePerson.SelectAllPersonEvents(personID);	
	}
	*/
	
}
