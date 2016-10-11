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
 * Copyright (C) 2004-2016   Xavier de Blas <xaviblas@gmail.com> 
 */


using System;
using Gtk;
using Gdk;
using Glade;
using System.IO.Ports;
using Mono.Unix;
using System.IO; //"File" things
using System.Collections; //ArrayList
using System.Collections.Generic; //List
using System.Threading;

public partial class ChronoJumpWindow 
{
		
	/* ---------------------------------------------------------
	 * ----------------  SERVER CALLS --------------------------
	 *  --------------------------------------------------------
	 */

	/* 
	 * SERVER CALLBACKS
	 */

	// upload session and it's persons (callback)
	private void on_server_upload_session_pre (object o, EventArgs args) {
		/*
		//evaluator stuff
		//Server.ServerUploadEvaluator();
		string evalMessage = "";
		int evalSID = Convert.ToInt32(SqlitePreferences.Select("evaluatorServerID"));
		if(evalSID == Constants.ServerUndefinedID) 
			evalMessage = Catalog.GetString("Please, first fill evaluator data.");
		else 
			evalMessage = Catalog.GetString("Please, first check evaluator data is ok.");
		
//		appbar2.Push ( 1, evalMessage );
		
		server_evaluator_data_and_after_upload_session();
		*/
	}

	private bool connectedAndCanI (string serverAction) {
		string versionAvailable = Server.Ping(false, "", ""); //false: don't do insertion
		if(versionAvailable != Constants.ServerOffline) { //false: don't do insertion
			if(Server.CanI(serverAction, progVersion))
				return true;
			else
				new DialogMessage(Constants.MessageTypes.WARNING, 
						Catalog.GetString("Your version of Chronojump is too old for this.") + "\n\n" + 
						Catalog.GetString("Please, update to new version: ") + versionAvailable + "\n");
		} else 
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.ServerOffline);

		return false;
	}

	private void on_menuitem_server_stats (object o, EventArgs args) {
		/*
		if(connectedAndCanI(Constants.ServerActionStats)) {
			ChronojumpServer myServer = new ChronojumpServer();
			LogB.SQL(myServer.ConnectDatabase());

			string [] statsServer = myServer.Stats();

			LogB.SQL(myServer.DisConnectDatabase());

			string [] statsMine = SqliteServer.StatsMine();

			new DialogServerStats(statsServer, statsMine);
		}
		*/
	}
	
	private void on_menuitem_server_query (object o, EventArgs args) {
		/*
		if(connectedAndCanI(Constants.ServerActionQuery)) {
			ChronojumpServer myServer = new ChronojumpServer();
			QueryServerWindow.Show(
					preferences.digitsNumber,
					myServer.SelectEvaluators(true)
					);
		}
		*/
	}
	
	private void on_server_ping (object o, EventArgs args) {
		/*
		string str = Server.Ping(false, progName, progVersion); //don't do insertion (will show versionAvailable)
		//show online or offline (not the next version of client available)
		if(str != Constants.ServerOffline)
			str = Catalog.GetString(Constants.ServerOnline);
		new DialogMessage(Constants.MessageTypes.INFO, str);
		*/
	}

	/*	
	bool uploadSessionAfter;

	//called when after that has to continue with upload session
	private void server_evaluator_data_and_after_upload_session() {
//		appbar2.Push ( 1, "" );
		uploadSessionAfter = true;
		server_evaluator_data (); 
	}

	//called when only has to be created/updated the evaluator (not update session)
	//private void on_menuitem_server_evaluator_data_only (object o, EventArgs args) {
	//	uploadSessionAfter = false;
	//	server_evaluator_data (); 
	//}
	
	
	private void server_evaluator_data () {
		ServerEvaluator myEval = SqliteServer.SelectEvaluator(1); 
		evalWin = EvaluatorWindow.Show(myEval);
		evalWin.FakeButtonAccept.Clicked += new EventHandler(on_evaluator_done);
	}

	private void on_evaluator_done (object o, EventArgs args) {
		if(evalWin.Changed) {
			string versionAvailable = Server.Ping(false, "", ""); //false: don't do insertion
			if(versionAvailable != Constants.ServerOffline) { //false: don't do insertion
				ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Do you want to upload evaluator data now?"), "", "");
				confirmWin.Button_accept.Clicked += new EventHandler(on_evaluator_upload_accepted);
			} else 
				new DialogMessage(Constants.MessageTypes.WARNING, 
						Catalog.GetString("Currently cannot upload.") + "\n\n" + Constants.ServerOffline);
		}
		else
			if(uploadSessionAfter)
				select_persons_to_discard ();

	}

	private void on_evaluator_upload_accepted (object o, EventArgs args) {
		Server.ServerUploadEvaluator();
		if(uploadSessionAfter)
			select_persons_to_discard ();
	}

	private void select_persons_to_discard () {
		personNotUploadWin = PersonNotUploadWindow.Show(app1, currentSession.UniqueID);
		personNotUploadWin.FakeButtonDone.Clicked += new EventHandler(on_select_persons_to_discard_done);
	}
	
	private void on_select_persons_to_discard_done (object o, EventArgs args) {
		server_upload_session();
	}
	*/

	private void on_menuitem_goto_server_website (object o, EventArgs args) {
		/*
		if(UtilAll.IsWindows())
			new DialogMessage(Constants.MessageTypes.INFO, 
					"http://www.chronojump.org/server.html" + "\n" + 
					"http://www.chronojump.org/server_es.html");
		else
			System.Diagnostics.Process.Start(Constants.ChronojumpWebsite + System.IO.Path.DirectorySeparatorChar + "server.html");
		*/
	}

	/* 
	 * SERVER CODE
	 */

	/*
	private bool checkPersonsMissingData() 
	{
		ArrayList impossibleWeight = new ArrayList(1);
		ArrayList undefinedCountry = new ArrayList(1); //country is required for server
		ArrayList undefinedSport = new ArrayList(1);
		
		ArrayList notToUpload = SqlitePersonSessionNotUpload.SelectAll(currentSession.UniqueID);
		ArrayList persons = SqlitePersonSession.SelectCurrentSessionPersons(
				currentSession.UniqueID,
				false); //means: do not returnPersonAndPSlist

		foreach (Person person in persons) 
		{
			if(! Util.FoundInArrayList(notToUpload, person.UniqueID.ToString())) 
			{
				//TODO: this is not needed if true at SqlitePersonSession.SelectCurrentSessionPersons
				PersonSession ps = SqlitePersonSession.Select(person.UniqueID, currentSession.UniqueID);
				if(ps.Weight <= 10 || ps.Weight >= 300)
					impossibleWeight.Add(person);
				if(person.CountryID == Constants.CountryUndefinedID)
					undefinedCountry.Add(person);
				if(ps.SportID == Constants.SportUndefinedID)
					undefinedSport.Add(person);
				//speciallity and level not required, because person gui obligates to select them when sport is selected
			}
		}

		string weightString = "";
		string countryString = "";
		string sportString = "";
		
		int maxPeopleFail = 7;

		if(impossibleWeight.Count > 0) {
			weightString += "\n\n" + Catalog.GetString("<b>Weight</b> of the following persons is not ok:") + "\n";
			string separator = "";
			int count=0;
			foreach(Person person in impossibleWeight) {
				weightString += separator + person.Name;
				separator = ", ";
				if(++count >= maxPeopleFail) {
					weightString += "...";
					break;
				}
			}
		}

		if(undefinedCountry.Count > 0) {
			countryString += "\n\n" + Catalog.GetString("<b>Country</b> of the following persons is undefined:") + "\n";
			string separator = "";
			int count=0;
			foreach(Person person in undefinedCountry) {
				countryString += separator + person.Name;
				separator = ", ";
				if(++count >= maxPeopleFail) {
					countryString += "...";
					break;
				}
			}
		}

		if(undefinedSport.Count > 0) {
			sportString += "\n\n" + Catalog.GetString("<b>Sport</b> of the following persons is undefined:") + "\n";
			string separator = "";
			int count=0;
			foreach(Person person in undefinedSport) {
				sportString += separator + person.Name;
				separator = ", ";
				if(++count >= maxPeopleFail) {
					sportString += "...";
					break;
				}
			}
		}

		if(weightString.Length > 0 || countryString.Length > 0 || sportString.Length > 0) {
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Please, fix this before uploading:") +
						weightString + countryString + sportString + "\n\n" + 
						Catalog.GetString("Or when upload session again, mark these persons as not to be uploaded.")
						);
			return true; //data is missing
		}
		else
			return false; //data is ok

	}
			
	private void server_upload_session () 
	{
		int evalSID = Convert.ToInt32(SqlitePreferences.Select("evaluatorServerID"));
		if(evalSID != Constants.ServerUndefinedID) {
			if(!checkPersonsMissingData()) {
				string message1 = ""; 
				if(currentSession.ServerUniqueID == Constants.ServerUndefinedID) 
					message1 =  
							Catalog.GetString("Session will be uploaded to server.") + "\n" +  
							Catalog.GetString("Names, date of birth and descriptions of persons will be hidden.") + "\n\n" + 
							Catalog.GetString("You can upload again this session if you add more data or persons.");
				else
					message1 =  
							Catalog.GetString("Session has been uploaded to server before.") + "\n" +  
							Catalog.GetString("Uploading new data.");

				message1 += "\n\n" + Catalog.GetString("All the uploaded data will be licensed as:") + 
						"\n<b>" + Catalog.GetString("Creative Commons Attribution 3.0") + "</b>";


				ConfirmWindow confirmWin = ConfirmWindow.Show(message1, 
							"<u>http://creativecommons.org/licenses/by/3.0/</u>", //label_link
							Catalog.GetString("Are you sure you want to upload this session to server?"));
				confirmWin.Button_accept.Clicked += new EventHandler(on_server_upload_session_accepted);
			}
		}
	}


	private void on_server_upload_session_accepted (object o, EventArgs args) 
	{
		if(connectedAndCanI(Constants.ServerActionUploadSession)) {
			Server.InitializeSessionVariables(app1, currentSession, progName, progVersion);
			Server.ThreadStart();
		}
	}
	*/

}
