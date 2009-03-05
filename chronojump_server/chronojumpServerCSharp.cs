//llibre mono (notebook) pag 206

using System; //for environment
using System.IO;
using System.Web.Services;
//using System.Web;
using System.Web;


//[WebService(Namespace="http://localhost:8080/", //work to connect to corall development from client (from browser works only when online)
//[WebService(Namespace="http://80.32.81.197:8080/", //works to connect with pinux xen from client (from browser don't works) WORKS FROM CLIENT
//[WebService(Namespace="http://server.chronojump.org:8080/", //works to connect with pinux xen from client (from browser don't works) WORKS FROM CLIENT (important: needed the last '/')
[WebService(Namespace="http://server.chronojump.org/", //works to connect with pinux xen from client (from browser don't works) WORKS FROM CLIENT (important: needed the last '/')
	Description="ChronojumpServer")]
[Serializable]
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
	
	[WebMethod(Description="Check actions that client can do depending on it's version)")]
	public bool CanI(string action, double clientVersion)
	{
		if(action == Constants.ServerActionUploadSession && clientVersion >= 0.8)
			return true;
		else if(action == Constants.ServerActionStats && clientVersion >= 0.8)
			return true;

		return false;
	}


	[WebMethod(Description="Stats")]
	public string [] Stats()
	{
		string [] stats = SqliteServer.Stats();

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
	public int UpdateSession(int sessionID, int  state)
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

/*	
	[WebMethod(Description="Upload a test type (user defined)")]
	//public string UploadTestType(Constants.TestTypes testType, EventType type, int evalSID)
	public string UploadTestType(int testType, EventType type, int evalSID)
	{
		string typeServer = type.Name + "-" + evalSID.ToString();

		Console.WriteLine("XXXXXXXXXXXXXXXX");
		bool inserted = false;
		switch (testType) {
			case (int) Constants.TestTypes.JUMP :
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
			case (int) Constants.TestTypes.JUMP_RJ :
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
	*/

	[WebMethod(Description="Upload a jump type (user defined)")]
	public string UploadJumpType(JumpType type, int evalSID)
	{
		string typeServer = type.Name + "-" + evalSID.ToString();
				
		Console.WriteLine("JUMP" + typeServer + ":" + type.StartIn + ":" + type.HasWeight + ":" + type.Description);
		if(! Sqlite.Exists(Constants.JumpTypeTable, typeServer)) {
			SqliteJumpType.JumpTypeInsert(
					typeServer + ":" + Util.BoolToInt(type.StartIn).ToString() + ":" + 
					Util.BoolToInt(type.HasWeight).ToString() + ":" + type.Description,
					false);
			return typeServer;
		}
		return "-1";
	}

	[WebMethod(Description="Upload a jumpRj type (user defined)")]
	public string UploadJumpRjType(JumpType type, int evalSID)
	{
		string typeServer = type.Name + "-" + evalSID.ToString();
				
		Console.WriteLine("JUMP_RJ" + typeServer + ":" + type.Description);
		if(! Sqlite.Exists(Constants.JumpRjTypeTable, typeServer)) {
			SqliteJumpType.JumpRjTypeInsert(
					typeServer + ":" + Util.BoolToInt(type.StartIn).ToString() + ":" + 
					Util.BoolToInt(type.HasWeight).ToString() + ":" + 
					Util.BoolToInt(type.JumpsLimited).ToString() + ":" + 
					type.FixedValue.ToString() + ":" + 
					type.Description,
					false);
			return typeServer;
		}
		return "-1";
	}

	[WebMethod(Description="Upload a run type (user defined)")]
	public string UploadRunType(RunType type, int evalSID)
	{
		string typeServer = type.Name + "-" + evalSID.ToString();
				
		Console.WriteLine("RUN" + typeServer + ":" + type.Distance + ":" + type.Description);
		if(! Sqlite.Exists(Constants.RunTypeTable, typeServer)) {
			type.Name = typeServer;
			SqliteRunType.Insert(type, Constants.RunTypeTable, false);
			return typeServer;
		}
		return "-1";
	}

	[WebMethod(Description="Upload a run interval type (user defined)")]
	public string UploadRunIntervalType(RunType type, int evalSID)
	{
		string typeServer = type.Name + "-" + evalSID.ToString();
				
		Console.WriteLine("RUN_I" + typeServer + ":" + type.Distance + ":" + type.Description);
		if(! Sqlite.Exists(Constants.RunIntervalTypeTable, typeServer)) {
			type.Name = typeServer;
			SqliteRunIntervalType.Insert(type, Constants.RunIntervalTypeTable, false);
			return typeServer;
		}
		return "-1";
	}

	
	[WebMethod(Description="Upload a person")]
	public int UploadPerson(Person myPerson, int sessionID)
	{
		//store person uniqueID
		int temp = myPerson.UniqueID;

		//change value for being inserted with new numeration in server
		myPerson.UniqueID = -1;
		
		//hidden person.Name
		myPerson.Name = "";
		
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
	public string UploadPing(ServerPing myPing, bool doInsertion)
	{
		//problemes getting user ip:
		//when it works it should be assigned to myPing.IP
		//string a = Request.UserHostName;
		//Console.WriteLine(System.Web.HttpRequest.UserHostAdress);

		Console.WriteLine("ping string: " + myPing.ToString());

		//!doInsertion is a way to know if server is connected
		//but without inserting nothing
		//is ok before uploading a session

		//Console.WriteLine("IP: " + System.Web.HttpRequest.UserHostAddress.ToString());


		if(doInsertion)
			myPing.InsertAtDB(false);
			
		return SqlitePreferences.Select("versionAvailable");
	}

	[WebMethod(Description="Upload an evaluator")]
	public string UploadEvaluator(ServerEvaluator myEval)
	{
		Console.WriteLine("upload. eval string: " + myEval.ToString());

		string idCode;
		Random rnd = new Random();  
		string password = myEval.Name + rnd.Next().ToString();
		string hashed = BCrypt.HashPassword(password, BCrypt.GenerateSalt(10));

		//insert the password in the server and the hash in the client
		myEval.Code = password;

		int id = myEval.InsertAtDB(false); //do insertion

		return id.ToString() + ":" + hashed;
	}

	[WebMethod(Description="Edit an evaluator")]
	public bool EditEvaluator(ServerEvaluator clientEval, int evalSID)
	{
		Console.WriteLine("edit. eval string: " + clientEval.ToString());

		ServerEvaluator serverEval = SqliteServer.SelectEvaluator(evalSID);

		//serveEval.Code is password
		//clientEval.Code is hash
		bool matches = BCrypt.CheckPassword(serverEval.Code, clientEval.Code);
		if(matches) {
			//put the uniqueID that corresponds in server
			clientEval.UniqueID = evalSID;

			//put the pass code instead of the client password hash
			clientEval.Code = serverEval.Code;

			clientEval.Update(false); //do update
			return true;
		}
			
		return false;
	}


/*
	[WebMethod(Description="Upload a test")]
	//public int UploadTest (Event myTest, Constants.TestTypes type, string tableName)
	public int UploadTest (Event myTest, int type, string tableName)
	{
		//store event uniqueID
		int temp = myTest.UniqueID;

		//change value for being inserted with new numeration in server
		myTest.UniqueID = -1;

		//insert
		int id = 0;
		switch (type) {
			case (int) Constants.TestTypes.JUMP :
				Jump jump = (Jump)myTest;
				id = jump.InsertAtDB(false, tableName);
				break;
			case (int) Constants.TestTypes.JUMP_RJ :
				JumpRj jumpRj = (JumpRj)myTest;
				id = jumpRj.InsertAtDB(false, tableName);
				break;
			case (int) Constants.TestTypes.RUN :
				Run run = (Run)myTest;
				id = run.InsertAtDB(false, tableName);
				break;
			case (int) Constants.TestTypes.RUN_I :
				RunInterval runI = (RunInterval)myTest;
				id = runI.InsertAtDB(false, tableName);
				break;
			case (int) Constants.TestTypes.RT :
				ReactionTime rt = (ReactionTime)myTest;
				id = rt.InsertAtDB(false, tableName);
				break;
			case (int) Constants.TestTypes.PULSE :
				Pulse pulse = (Pulse)myTest;
				id = pulse.InsertAtDB(false, tableName);
				break;
		}

		//roll back person unique id value
		myTest.UniqueID = temp;

		return id;
	}
	*/

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

	[WebMethod(Description="Upload a jumpRj")]
	public int UploadJumpRj (JumpRj myTest)
	{
		int temp = myTest.UniqueID;
		myTest.UniqueID = -1;
		int id = myTest.InsertAtDB(false, Constants.JumpRjTable);
		myTest.UniqueID = temp;
		return id; //uniqueID of person at server
	}

	[WebMethod(Description="Upload a run")]
	public int UploadRun (Run myTest)
	{
		int temp = myTest.UniqueID;
		myTest.UniqueID = -1;
		int id = myTest.InsertAtDB(false, Constants.RunTable);
		myTest.UniqueID = temp;
		return id; //uniqueID of person at server
	}
	
	[WebMethod(Description="Upload a run interval")]
	public int UploadRunI (RunInterval myTest)
	{
		int temp = myTest.UniqueID;
		myTest.UniqueID = -1;
		int id = myTest.InsertAtDB(false, Constants.RunIntervalTable);
		myTest.UniqueID = temp;
		return id; //uniqueID of person at server
	}
	
	[WebMethod(Description="Upload a reaction time")]
	public int UploadRT (ReactionTime myTest)
	{
		int temp = myTest.UniqueID;
		myTest.UniqueID = -1;
		int id = myTest.InsertAtDB(false, Constants.ReactionTimeTable);
		myTest.UniqueID = temp;
		return id; //uniqueID of person at server
	}
	
	[WebMethod(Description="Upload a pulse")]
	public int UploadPulse (Pulse myTest)
	{
		int temp = myTest.UniqueID;
		myTest.UniqueID = -1;
		int id = myTest.InsertAtDB(false, Constants.PulseTable);
		myTest.UniqueID = temp;
		return id; //uniqueID of person at server
	}

	
	[WebMethod(Description="List directory files (only as a sample)")]
	public string [] ListDirectory(string path) {
		return Directory.GetFileSystemEntries(path);
	}

	
}
