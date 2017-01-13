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

using Mono.Unix;

public class ServerPing
{
	private int uniqueID;
	private int evaluatorID;
	private string cjVersion;
	private string osVersion;
	private string ip;
	private DateTime date;

	//only initializing
	//needed by the webservice
	public ServerPing() {
	}

	public ServerPing(int evaluatorID, string cjVersion, string osVersion, string ip, DateTime date) {
		this.evaluatorID = evaluatorID;
		this.cjVersion = cjVersion;
		this.osVersion = osVersion;
		this.ip = ip;
		this.date = date;
	}

	public int InsertAtDB(bool dbconOpened){
		int myID = SqliteServer.InsertPing(dbconOpened, evaluatorID, cjVersion, osVersion, ip, date);
		return myID;
	}	

	public override string ToString() {
		return Catalog.GetString("Uploaded") + "\nID: " + uniqueID + "\nEvaluatorID: " + evaluatorID + 
			"\nChronojump Version: " + cjVersion + "\nOS Version: " + osVersion +
			"\nIP: " + ip + "\nDate: " + date.ToString();
	}
	
	public int UniqueID {
		get { return uniqueID; }
		set { uniqueID = value; }
	}

	//the set's are for the server
	//"Private, internal, and protected members do not get serialized.  
	//If the accessor is not specific, it is private by default (and will not get serialized)."
	
	public int EvaluatorID {
		get { return evaluatorID; }
		set { evaluatorID = value; }
	}

	public string CJVersion {
		get { return cjVersion; }
		set { cjVersion = value; }
	}

	public string OSVersion {
		get { return osVersion; }
		set { osVersion = value; }
	}

	public string IP {
		get { return ip; }
		set { ip = value; }
	}

	public DateTime Date {
		get { return date; }
		set { date = value; }
	}

}
