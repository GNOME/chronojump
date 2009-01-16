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

using System;
using System.Data;
using System.Text; //StringBuilder

using Mono.Unix;

public class ServerPing
{
	private int uniqueID;
	private int evaluatorID;
	private string ip;
	private string date;

	//only initializing
	//needed by the webservice
	public ServerPing() {
	}

	public ServerPing(int evaluatorID, string ip, string date) {
		this.evaluatorID = evaluatorID;
		this.ip = ip;
		this.date = date;
	}

	public int InsertAtDB(bool dbconOpened){
		int myID = SqliteServer.InsertPing(dbconOpened, evaluatorID, ip, date);
		return myID;
	}	

	public override string ToString() {
		return "ID: " + uniqueID + "; evaluatorID: " + evaluatorID + "; IP: " + ip + "; date: " + date;
	}
	
	public int UniqueID {
		get { return uniqueID; }
	}

	//the set's are for the server
	//"Private, internal, and protected members do not get serialized.  
	//If the accessor is not specific, it is private by default (and will not get serialized)."
	
	public int EvaluatorID {
		get { return evaluatorID; }
		set { evaluatorID = value; }
	}

	public string IP {
		get { return ip; }
		set { ip = value; }
	}

	public string Date {
		get { return date; }
		set { date = value; }
	}

}
