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
 *  Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.Text; //StringBuilder
using System.Threading;
using Mono.Unix;
using Gtk;
using Gdk;
using Glade;
using System.Net;
using System.Collections;

//test comment

public class Server
{
	public static bool CanI(string action, string clientVersion) {
		try {
			ChronojumpServer myServer = new ChronojumpServer();
			return myServer.CanINew(action, clientVersion);
		} catch {
			return false;
		}
	}

	/*
	private static string getIP() {
		string strHostName = "";
		strHostName = System.Net.Dns.GetHostName();
		IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);
		IPAddress[] addr = ipEntry.AddressList;
		return addr[addr.Length-1].ToString();
	}
	*/

	public static string Ping(bool doInsertion, string progName, string progVersion) {
		try {
			ChronojumpServer myServer = new ChronojumpServer();
			LogB.Information(myServer.ConnectDatabase());
		
			int evalSID = Convert.ToInt32(SqlitePreferences.Select("evaluatorServerID"));
			string machineID = SqlitePreferences.Select("machineID");

			ServerPing myPing = new ServerPing(evalSID, progName + " " + progVersion, UtilAll.GetOS(), 
					//getIP(), DateTime.Now); //evaluator IP, date
					machineID, DateTime.Now); //evaluator machineID, date

			//if !doIsertion nothing will be uploaded,
			//is ok for uploadPerson to know if server is online
			string versionAvailable = myServer.UploadPing(myPing, doInsertion);
			
			LogB.Information(myServer.DisConnectDatabase());
			return versionAvailable;
		} catch (Exception e){
			LogB.Information("Server Connection", e.Message);
			return Constants.ServerOffline;
		}
	}
	
	/* 
	 * server session update 
	 */

	static Thread thread;

	public static SessionUploadWindow sessionUploadWin;
	[Widget] static Gtk.Window app1;
	
	static Session currentSession;
	static string progName;
	static string progVersion;

	static bool serverSessionError;
	static bool needUpdateServerSession;
	static bool updatingServerSession;
	static SessionUploadPersonData sessionUploadPersonData;
	static int progressBarPersonsNum;
	static int countPersons;
			
	public static void InitializeSessionVariables(Gtk.Window mainApp, Session session, string programName, string programVersion) {
		app1 = mainApp;
		currentSession = session;
		progName = programName;
		progVersion = programVersion;

		serverSessionError = false;
		needUpdateServerSession = false;
		updatingServerSession = false;
		sessionUploadPersonData = new SessionUploadPersonData();
		countPersons = 0;
		progressBarPersonsNum = 0;
	}
			
	public static void ThreadStart() {
		thread = new Thread(new ThreadStart(on_server_upload_session_started));
		GLib.Idle.Add (new GLib.IdleHandler (pulseGTKServer));
		thread.Start(); 
	}
	
	private static bool pulseGTKServer ()
	{
		if(! thread.IsAlive) {
			sessionUploadWin.UploadFinished();
			LogB.Information("dying");
			return false;
		}

		if (serverSessionError) {
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Error uploading session to server"));
			return false;
		}

		//need to do this, if not it crashes because chronopicWin gets died by thread ending
		sessionUploadWin = SessionUploadWindow.Show(app1);
			
		if(countPersons == 0)
			sessionUploadWin.PulseProgressbar();

		//activity on pulsebar
		sessionUploadWin.UpdatePulsebar();

		if(needUpdateServerSession && !updatingServerSession) {
			//prevent that FillData is called again with same data
			updatingServerSession = true;

			//update progressBar
			sessionUploadWin.UpdateProgressbar(
					Util.DivideSafeFraction(++countPersons, progressBarPersonsNum));

			//fill data
			sessionUploadWin.FillData(sessionUploadPersonData);

			//not need to update until there's more data coming from the other thread
			updatingServerSession = false;
			needUpdateServerSession = false;
		}
		
		Thread.Sleep (50);
		LogB.Debug(thread.ThreadState.ToString());
		return true;
	}
	
	private static void on_server_upload_session_started () 
	{
		int evalSID = Convert.ToInt32(SqlitePreferences.Select("evaluatorServerID"));

		try {	
			ChronojumpServer myServer = new ChronojumpServer();
			LogB.Information(myServer.ConnectDatabase());
		
			int state = (int) Constants.ServerSessionStates.UPLOADINGSESSION;
			//create ServerSession based on Session currentSession
			ServerSession serverSession = new ServerSession(currentSession, evalSID, progName + " " + progVersion, 
					UtilAll.GetOS(), DateTime.Now, state); 

			//if uploading session for first time
			if(currentSession.ServerUniqueID == Constants.ServerUndefinedID) 
			{
				//upload ServerSession
				int idAtServer = myServer.UploadSession(serverSession);

				//update session currentSession (serverUniqueID) on client database
				currentSession.ServerUniqueID = idAtServer;
				SqliteSession.UpdateServerUniqueID(currentSession.UniqueID, currentSession.ServerUniqueID);
			}

			state = (int) Constants.ServerSessionStates.UPLOADINGDATA;
			myServer.UpdateSession(currentSession.ServerUniqueID, state); 

			sessionUploadPersonData.testTypes = "";
			string testTypesSeparator = "";
			sessionUploadPersonData.sports = "";
			string sportsSeparator = "";

			//upload persons (updating also person.serverUniqueID locally)
			ArrayList persons = SqlitePersonSession.SelectCurrentSessionPersons(
					serverSession.UniqueID,
					false); //means: do not returnPersonAndPSlist
			
			Constants.UploadCodes uCode;
			ArrayList notToUpload = SqlitePersonSessionNotUpload.SelectAll(currentSession.UniqueID);
			
			//store in variable for updating progressBar from other thread
			progressBarPersonsNum = persons.Count - notToUpload.Count;

			foreach(Person p in persons) {
				Person person = p;

				//do not continue with this person if has been banned to upload
				if(Util.FoundInArrayList(notToUpload, person.UniqueID.ToString())) 
					continue;

				PersonSession ps = SqlitePersonSession.Select(person.UniqueID, currentSession.UniqueID);  

				//check person if exists
				if(person.ServerUniqueID != Constants.ServerUndefinedID) 
					uCode = Constants.UploadCodes.EXISTS;
				else {
					uCode = Constants.UploadCodes.OK;

					person = serverUploadPerson(myServer, person, serverSession.UniqueID);
				}
					
				//if sport is user defined, upload it
				//and when upload the person, do it with new sportID
				Sport sport = SqliteSport.Select(false, ps.SportID);
				//but record old sport ID because locally will be a change in serverUniqueID
				//(with slite update)
				//but local sport has not to be changed
				int sportUserDefinedLocal = -1;

				if(sport.UserDefined) {
					sportUserDefinedLocal = sport.UniqueID;

					//this will be uploaded
					int newSport = myServer.UploadSport(sport);
					if(newSport != -1) {
						ps.SportID = newSport;
						sessionUploadPersonData.sports += sportsSeparator + sport.Name;
						sportsSeparator = ", ";
					}
				}

				//a person can be in the database for one session, 
				//but maybe now we add jumps from another session and we should add an entry at personsession
				serverUploadPersonSessionIfNeeded(myServer, person.ServerUniqueID, 
						currentSession.ServerUniqueID, ps, sportUserDefinedLocal);

				//other thread updates the gui:
				sessionUploadPersonData.person = person;
				sessionUploadPersonData.personCode = uCode;

				//upload jumps
				int countU = 0;					
				int countE = 0;					
				int countS = 0;					

				string [] jumps = SqliteJump.SelectJumps (false, currentSession.UniqueID, person.UniqueID, "", "",
						Sqlite.Orders_by.DEFAULT, 0);
				Sqlite.Open();
				foreach(string myJump in jumps) {
					string [] js = myJump.Split(new char[] {':'});
					//select jump
					Jump test = SqliteJump.SelectJumpData(Convert.ToInt32(js[1]), true); //uniqueID
					//fix it to server person, session keys
					test.PersonID = person.ServerUniqueID;
					test.SessionID = currentSession.ServerUniqueID;

					//if test is not simulated and has not been uploaded,
					//see if it's type is not predefined and is not in the database
					//then upload it first
					if(test.Simulated == 0) {
						//upload jumpType if is user defined and doesn't exists in server database
						//JumpType type = new JumpType(test.Type);
						JumpType type = SqliteJumpType.SelectAndReturnJumpType(test.Type, true);
						if( ! type.IsPredefined) {
							//Console.WriteLine("USER DEFINED TEST: " + test.Type);
							//
							//this uploads the new type, as it's user created, it will be like this
							//eg: for user defined jumpType: "supra" of evaluatorServerID: 9
							//at server will be "supra-9"
							//then two problems get solved:
							//1.- every evaluator that uploads a type will have a different name 
							//than other evaluator uploading a type that is named the same but could be different 
							//(one can think that "supra" is another thing
							//2- when the same evaluator upload some supra's, only a new type is created
					
							//test.Type = myServer.UploadJumpType(type, evalSID);
							//int testType = (int) Constants.TestTypes.JUMP;
							//string insertedType = myServer.UploadTestType(Constants.TestTypes.JUMP, type, evalSID);
							//string insertedType = myServer.UploadTestType(testType, type, evalSID);
							string insertedType = myServer.UploadJumpType(type, evalSID);
							if(insertedType != "-1") {
								//record type in test (with the "-7" if it's done by evaluator 7)
								test.Type = insertedType;

								//show user uploaded type (without the "-7")
								sessionUploadPersonData.testTypes += testTypesSeparator + type.Name;
								testTypesSeparator = ", ";
							}
					
							//test.Type in the server will have the correct name "supra-9" 
						} 
					}

					//upload... (if not because of simulated or uploaded before, report also the user)
					uCode = serverUploadTest(myServer, Constants.TestTypes.JUMP, Constants.JumpTable, test);

					if(uCode == Constants.UploadCodes.OK)
						countU ++;
					else if(uCode == Constants.UploadCodes.EXISTS)
						countE ++;
					else //SIMULATED
						countS ++;
				}
				Sqlite.Close();

				//other thread updates the gui:
				sessionUploadPersonData.jumpsU = countU;
				sessionUploadPersonData.jumpsE = countE;
				sessionUploadPersonData.jumpsS = countS;

				//upload jumpsRj
				countU = 0;					
				countE = 0;					
				countS = 0;					

				string [] jumpsRj = SqliteJumpRj.SelectJumps (false, currentSession.UniqueID, person.UniqueID, "", "");
				Sqlite.Open();
				foreach(string myJump in jumpsRj) {
					string [] js = myJump.Split(new char[] {':'});
					//select jump
					JumpRj test = SqliteJumpRj.SelectJumpData(Constants.JumpRjTable, Convert.ToInt32(js[1]), false, true); //uniqueID
					//fix it to server person, session keys
					test.PersonID = person.ServerUniqueID;
					test.SessionID = currentSession.ServerUniqueID;
					
					if(test.Simulated == 0) {
						JumpType type = SqliteJumpType.SelectAndReturnJumpRjType(test.Type, true);
						if( ! type.IsPredefined) {
							string insertedType = myServer.UploadJumpRjType(type, evalSID);
							if(insertedType != "-1") {
								test.Type = insertedType;
								sessionUploadPersonData.testTypes += testTypesSeparator + type.Name;
								testTypesSeparator = ", ";
							}
						} 
					}
					
					//upload...
					uCode = serverUploadTest(myServer, Constants.TestTypes.JUMP_RJ, Constants.JumpRjTable, test);

					if(uCode == Constants.UploadCodes.OK)
						countU ++;
					else if(uCode == Constants.UploadCodes.EXISTS)
						countE ++;
					else //SIMULATED
						countS ++;
				}
				Sqlite.Close();

				//other thread updates the gui:
				sessionUploadPersonData.jumpsRjU = countU;
				sessionUploadPersonData.jumpsRjE = countE;
				sessionUploadPersonData.jumpsRjS = countS;

				//upload runs
				countU = 0;					
				countE = 0;					
				countS = 0;					

				string [] runs = SqliteRun.SelectRuns (false, currentSession.UniqueID, person.UniqueID, "",
						Sqlite.Orders_by.DEFAULT, 0);

				Sqlite.Open();
				foreach(string myRun in runs) {
					string [] js = myRun.Split(new char[] {':'});
					//select run
					Run test = SqliteRun.SelectRunData(Convert.ToInt32(js[1]), true); //uniqueID
					//fix it to server person, session keys
					test.PersonID = person.ServerUniqueID;
					test.SessionID = currentSession.ServerUniqueID;

					if(test.Simulated == 0) {
						RunType type = SqliteRunType.SelectAndReturnRunType(test.Type, true);
						if( ! type.IsPredefined) {
							string insertedType = myServer.UploadRunType(type, evalSID);
							if(insertedType != "-1") {
								test.Type = insertedType;
								sessionUploadPersonData.testTypes += testTypesSeparator + type.Name;
								testTypesSeparator = ", ";
							}
						}
					}

					//upload...
					uCode = serverUploadTest(myServer, Constants.TestTypes.RUN, Constants.RunTable, test);

					if(uCode == Constants.UploadCodes.OK)
						countU ++;
					else if(uCode == Constants.UploadCodes.EXISTS)
						countE ++;
					else //SIMULATED
						countS ++;
				}
				Sqlite.Close();

				//other thread updates the gui:
				sessionUploadPersonData.runsU = countU;
				sessionUploadPersonData.runsE = countE;
				sessionUploadPersonData.runsS = countS;

				//upload runs intervallic
				countU = 0;					
				countE = 0;					
				countS = 0;					

				string [] runsI = SqliteRunInterval.SelectRuns (false, currentSession.UniqueID, person.UniqueID, "");
				Sqlite.Open();
				foreach(string myRun in runsI) {
					string [] js = myRun.Split(new char[] {':'});
					//select run
					RunInterval test = SqliteRunInterval.SelectRunData(Constants.RunIntervalTable, Convert.ToInt32(js[1]), false, true); //uniqueID
					//fix it to server person, session keys
					test.PersonID = person.ServerUniqueID;
					test.SessionID = currentSession.ServerUniqueID;
					
					if(test.Simulated == 0) {
						RunType type = SqliteRunIntervalType.SelectAndReturnRunIntervalType(test.Type, true);
						if( ! type.IsPredefined) {
							string insertedType = myServer.UploadRunIntervalType(type, evalSID);
							if(insertedType != "-1") {
								test.Type = insertedType;
								sessionUploadPersonData.testTypes += testTypesSeparator + type.Name;
								testTypesSeparator = ", ";
							}
						} 
					}
					//upload...
					uCode = serverUploadTest(myServer, Constants.TestTypes.RUN_I, Constants.RunIntervalTable, test);

					if(uCode == Constants.UploadCodes.OK)
						countU ++;
					else if(uCode == Constants.UploadCodes.EXISTS)
						countE ++;
					else //SIMULATED
						countS ++;
				}
				Sqlite.Close();

				//other thread updates the gui:
				sessionUploadPersonData.runsIU = countU;
				sessionUploadPersonData.runsIE = countE;
				sessionUploadPersonData.runsIS = countS;

				//upload reaction times
				countU = 0;					
				countE = 0;					
				countS = 0;					

				string [] rts = SqliteReactionTime.SelectReactionTimes(false, currentSession.UniqueID, person.UniqueID, "",
						Sqlite.Orders_by.DEFAULT, -1);

				Sqlite.Open();
				foreach(string myRt in rts) {
					string [] js = myRt.Split(new char[] {':'});
					//select rt
					ReactionTime test = SqliteReactionTime.SelectReactionTimeData(Convert.ToInt32(js[1]), true); //uniqueID
					//fix it to server person, session keys
					test.PersonID = person.ServerUniqueID;
					test.SessionID = currentSession.ServerUniqueID;
					//upload...
					uCode = serverUploadTest(myServer, Constants.TestTypes.RT, Constants.ReactionTimeTable, test);

					if(uCode == Constants.UploadCodes.OK)
						countU ++;
					else if(uCode == Constants.UploadCodes.EXISTS)
						countE ++;
					else //SIMULATED
						countS ++;
				}
				Sqlite.Close();

				//other thread updates the gui:
				sessionUploadPersonData.rtsU = countU;
				sessionUploadPersonData.rtsE = countE;
				sessionUploadPersonData.rtsS = countS;

				//upload pulses
				countU = 0;					
				countE = 0;					
				countS = 0;					

				string [] pulses = SqlitePulse.SelectPulses(false, currentSession.UniqueID, person.UniqueID);
				Sqlite.Open();
				foreach(string myPulse in pulses) {
					string [] js = myPulse.Split(new char[] {':'});
					//select pulse
					Pulse test = SqlitePulse.SelectPulseData(Convert.ToInt32(js[1]), true); //uniqueID
					//fix it to server person, session keys
					test.PersonID = person.ServerUniqueID;
					test.SessionID = currentSession.ServerUniqueID;
					//upload...
					uCode = serverUploadTest(myServer, Constants.TestTypes.PULSE, Constants.PulseTable, test);

					if(uCode == Constants.UploadCodes.OK)
						countU ++;
					else if(uCode == Constants.UploadCodes.EXISTS)
						countE ++;
					else //SIMULATED
						countS ++;
				}
				Sqlite.Close();

				//other thread updates the gui:
				sessionUploadPersonData.pulsesU = countU;
				sessionUploadPersonData.pulsesE = countE;
				sessionUploadPersonData.pulsesS = countS;

				//upload multiChronopic
				countU = 0;					
				countE = 0;					
				countS = 0;					

				string [] mcs = SqliteMultiChronopic.SelectTests(false, currentSession.UniqueID, person.UniqueID);
				Sqlite.Open();
				foreach(string mc in mcs) {
					string [] js = mc.Split(new char[] {':'});
					//select mc
					MultiChronopic test = SqliteMultiChronopic.SelectMultiChronopicData(Convert.ToInt32(js[1]), true); //uniqueID
					//fix it to server person, session keys
					test.PersonID = person.ServerUniqueID;
					test.SessionID = currentSession.ServerUniqueID;
					//upload...
					uCode = serverUploadTest(myServer, Constants.TestTypes.MULTICHRONOPIC, Constants.MultiChronopicTable, test);

					if(uCode == Constants.UploadCodes.OK)
						countU ++;
					else if(uCode == Constants.UploadCodes.EXISTS)
						countE ++;
					else //SIMULATED
						countS ++;
				}
				Sqlite.Close();

				//other thread updates the gui:
				sessionUploadPersonData.mcsU = countU;
				sessionUploadPersonData.mcsE = countE;
				sessionUploadPersonData.mcsS = countS;

				needUpdateServerSession = true;
				while(needUpdateServerSession) {
					//wait until data is printed on the other thread
				}

			}
								
			state = (int) Constants.ServerSessionStates.DONE;
			//myServer.UpdateSession(currentSession.ServerUniqueID, (ServerSessionStates)  Constants.ServerSessionStates.DONE); 
			myServer.UpdateSession(currentSession.ServerUniqueID, state); 

			LogB.Information(myServer.DisConnectDatabase());
		} catch {
			//other thread updates the gui:
			serverSessionError = true;
		}
	}
	
	
	//upload a person
	private static Person serverUploadPerson(ChronojumpServer myServer, Person person, int serverSessionID) 
	{
		int idAtServer = myServer.UploadPerson(person, serverSessionID);

		//update person (serverUniqueID) on client database
		person.ServerUniqueID = idAtServer;

		SqlitePerson.Update(person);

		return person;
	}

	private static void serverUploadPersonSessionIfNeeded(ChronojumpServer myServer, 
			int personServerID, int sessionServerID, PersonSession ps, int sportUserDefinedLocal)
	{
		//when update locally, don't put the user defined sport id at server
		if(sportUserDefinedLocal != -1)
			ps.SportID = sportUserDefinedLocal;

		ps.UniqueID = -1;
		ps.PersonID = personServerID;
		ps.SessionID = sessionServerID;

		myServer.UploadPersonSessionIfNeeded(ps);
	}

	//upload a test
	private static Constants.UploadCodes serverUploadTest(ChronojumpServer myServer, Constants.TestTypes type, string tableName, Event myTest) 
	{
		Constants.UploadCodes uCode;

		if(myTest.Simulated == Constants.Simulated) {
			//Test is simulated, don't upload
			uCode = Constants.UploadCodes.SIMULATED;
		} else if(myTest.Simulated > 0) {
			//Test is already uploaded, don't upload
			uCode = Constants.UploadCodes.EXISTS;
		} else {
			int idAtServer = -1;
			switch (type) {
				case Constants.TestTypes.JUMP :
					Jump jump = (Jump)myTest;
					idAtServer = myServer.UploadJump(jump);
					break;
				case Constants.TestTypes.JUMP_RJ :
					JumpRj jumpRj = (JumpRj)myTest;
					idAtServer = myServer.UploadJumpRj(jumpRj);
					break;
				case Constants.TestTypes.RUN :
					Run run = (Run)myTest;
					idAtServer = myServer.UploadRun(run);
					break;
				case Constants.TestTypes.RUN_I :
					RunInterval runI = (RunInterval)myTest;
					idAtServer = myServer.UploadRunI(runI);
					break;
				case Constants.TestTypes.RT :
					ReactionTime rt = (ReactionTime)myTest;
					idAtServer = myServer.UploadRT(rt);
					break;
				case Constants.TestTypes.PULSE :
					Pulse pulse = (Pulse)myTest;
					idAtServer = myServer.UploadPulse(pulse);
					break;
				case Constants.TestTypes.MULTICHRONOPIC :
					MultiChronopic mc = (MultiChronopic)myTest;
					idAtServer = myServer.UploadMultiChronopic(mc);
					break;
			}

			
			//update test (simulated) on client database
			myTest.Simulated = idAtServer;
			SqliteEvent.UpdateSimulated(true, tableName, myTest.UniqueID, idAtServer);
			
			uCode = Constants.UploadCodes.OK;
		}
		return uCode;
	}

	public static void ServerUploadEvaluator () {
		try {
			ChronojumpServer myServer = new ChronojumpServer();
			LogB.Information(myServer.ConnectDatabase());
			
			ServerEvaluator myEval = SqliteServer.SelectEvaluator(1);

			bool success = false;
			int evalSID = Convert.ToInt32(SqlitePreferences.Select("evaluatorServerID"));
			if(evalSID == Constants.ServerUndefinedID) {
				string idCode = myServer.UploadEvaluator(myEval);
				myEval.Code = Util.FetchName(idCode);

				myEval.Update(false);

				evalSID = Util.FetchID(idCode);
				SqlitePreferences.Update("evaluatorServerID", evalSID.ToString(), false);
				success = true;
			} else 
				success = myServer.EditEvaluator(myEval, evalSID);
				
			if(success)
				new DialogMessage(Constants.MessageTypes.INFO, 
						string.Format(Catalog.GetString("Successfully Uploaded evaluator with ID: {0}"), evalSID));
			else
				new DialogMessage(Constants.MessageTypes.WARNING, 
						string.Format(Catalog.GetString("Evaluator {0} has not been correctly uploaded. Maybe codes doesn't match."), evalSID));
			
			LogB.Information(myServer.DisConnectDatabase());
		} catch {
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.ServerOffline);
		}
	}
	
}
