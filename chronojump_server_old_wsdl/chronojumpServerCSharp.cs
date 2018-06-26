/*
 * This file is part of ChronoJump
 *
 * Chronojump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * Chronojump is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 *    GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * Copyright (C) 2004-2009   Xavier de Blas <xaviblas@gmail.com> 
 */




using System; //for environment
using System.IO;
using System.Web.Services;
//using System.Web;
using System.Web;
using System.Collections; //ArrayList

using System.Net; //getIP stuff

//[WebService(Namespace="http://localhost:8080/", //work to connect to corall development from client (from browser works only when online)
//[WebService(Namespace="http://80.32.81.197:8080/", //works to connect with pinux xen from client (from browser don't works) WORKS FROM CLIENT
//[WebService(Namespace="http://server.chronojump.org:8080/", //works to connect with pinux xen from client (from browser don't works) WORKS FROM CLIENT (important: needed the last '/')
//[WebService(Namespace="http://server.chronojump.org/", //works to connect with pinux xen from client (from browser don't works) WORKS FROM CLIENT (important: needed the last '/')
[WebService(Namespace="http://server.chronojump.org:8080/", //2013 server
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
	public bool CanINew(string action, string clientVersion)
	{
		Version cv = new Version(clientVersion);
		if(action == Constants.ServerActionUploadSession && cv >= new Version(0,9,3))
			return true;
		else if(action == Constants.ServerActionStats && cv >= new Version(0,8,18))
			return true;
		else if(action == Constants.ServerActionQuery && cv >= new Version(0,8,18))
			return true;

		return false;
	}

/* note this is old*/
	[WebMethod(Description="Check actions that client can do depending on it's version)")]
	public bool CanI(string action, double clientVersion)
	{
		//comes something like 0.898
		//ONLY used on 0.8.9.7, 0.8.9.8
		Version cv;
		if(clientVersion == 0.897)
			cv = new Version(0,8,9,7);
		else if(clientVersion == 0.898)
			cv = new Version(0,8,9,8);
		else 
			return false; //"for if the flyes"

		if(action == Constants.ServerActionUploadSession && cv >= new Version(0,9,3))
			return true;
		else if(action == Constants.ServerActionStats && cv >= new Version(0,8))
			return true;
		else if(action == Constants.ServerActionQuery && cv >= new Version(0,8,9,6))
			return true;

		return false;
	}

	[WebMethod(Description="Query")]
	public string Query(string tableName, string test, string variable,
			int sex, string ageInterval,
			int countryID, int sportID, int speciallityID, int levelID, int evaluatorID)
	{
		string str = Sqlite.SQLBuildQueryString(tableName, test, variable,
				sex, ageInterval, 
				countryID, sportID, speciallityID, levelID, evaluatorID
				);

		return SqliteServer.Query(str);
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
	
		try {
			File.Create("need-to-update-r-graphs");
		} catch {
			//file exists and cannot be overwritten
		}
	
		return id; //uniqueID of session at server
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
		if(! Sqlite.Exists(false, Constants.SportTable, mySport.Name))
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
				if(! Sqlite.Exists(false, Constants.JumpTypeTable, typeServer)) {
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
				if(! Sqlite.Exists(false, Constants.JumpRjTypeTable, typeServer)) {
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
		if(! Sqlite.Exists(false, Constants.JumpTypeTable, typeServer)) {
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
		if(! Sqlite.Exists(false, Constants.JumpRjTypeTable, typeServer)) {
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
		if(! Sqlite.Exists(false, Constants.RunTypeTable, typeServer)) {
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
		if(! Sqlite.Exists(false, Constants.RunIntervalTypeTable, typeServer)) {
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
		
		//hidden person.Name and comments
		myPerson.Name = "";
		myPerson.Description = "";
		
		//do insertion
		int id = myPerson.InsertAtDB(false, Constants.PersonTable);
		
		//roll back person unique id value
		myPerson.UniqueID = temp;

		Console.WriteLine("id at server: " + id);

		return id; //uniqueID of person at server
	}
	
	[WebMethod(Description="Upload person session if needed")]
	public int UploadPersonSessionIfNeeded(PersonSession ps)
	{
		if(!SqlitePersonSession.PersonSelectExistsInSession(ps.PersonID, ps.SessionID)) {
			Console.WriteLine("personSession needed");
			Console.WriteLine(ps.ToString());
			ps.InsertAtDB(false, Constants.PersonSessionTable);
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
	

		string strHostName = "";
		strHostName = System.Net.Dns.GetHostName();
		IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);
		IPAddress[] addr = ipEntry.AddressList;
		string ip = addr[addr.Length-1].ToString();
		
		Console.WriteLine("ip: " + ip);




		//!doInsertion is a way to know if server is connected
		//but without inserting nothing
		//is ok before uploading a session

		//Console.WriteLine("IP: " + System.Web.HttpRequest.UserHostAddress.ToString());
		//Console.WriteLine("IP: " + System.Net.HttpRequest.UserHostAddress.ToString());
		//Console.WriteLine("IP: " + System.Net.Request.UserHostAddress.ToString());
		//Console.WriteLine("IP: " + HttpContext.Current.Request.UserHostAddress);
		//Console.WriteLine("IP context : " + this.Context.Request.UserHostAddress);
		//Console.WriteLine("IP : " + this.Request.UserHostAddress);
		
		//Console.WriteLine("IP : " + System.Net.HttpListenerRequest.UserHostAddress.ToString());
		//System.Net.HttpListenerRequest req = new System.Net.HttpListenerRequest();
		//Console.WriteLine("IP : " + req.UserHostAddress.ToString());

//		System.Net.HttpListenerRequest req;
		//string a = System.Net.HttpListenerRequest.UserHostAddress;
//		string a = req.UserHostAddress;
			
		//string a = this.System.Net.HttpListenerRequest.UserHostAddress;

//		System.Net.HttpListenerRequest request = new HttpListenerRequest (String.Empty, "http://localhost/", String.Empty);
//		string a = request.UserHostAddress;

		//http://lists.ximian.com/pipermail/mono-list/2007-January/033998.html

		/*
		HttpListener listener = new HttpListener();

		listener.AuthenticationSchemeSelectorDelegate += delegate{
			Console.WriteLine("Asking for authentication scheme");
			return AuthenticationSchemes.Basic;
		};



		listener.Start();
		HttpListenerContext context = listener.GetContext();
		HttpListenerRequest request = context.Request;
		Console.WriteLine("IP req: " + request.UserHostAddress);
		*/



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

	[WebMethod(Description="Select evaluators")]
	public string [] SelectEvaluators(bool addAnyString)
	{
		Console.WriteLine("select evaluators");

		return SqliteServer.SelectEvaluators(addAnyString);
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

	[WebMethod(Description="Upload a multiChronopic")]
	public int UploadMultiChronopic (MultiChronopic myTest)
	{
		int temp = myTest.UniqueID;
		myTest.UniqueID = -1;
		int id = myTest.InsertAtDB(false, Constants.MultiChronopicTable);
		myTest.UniqueID = temp;
		return id; //uniqueID of person at server
	}

	
	[WebMethod(Description="List directory files (only as a sample)")]
	public string [] ListDirectory(string path) {
		return Directory.GetFileSystemEntries(path);
	}

	
}
