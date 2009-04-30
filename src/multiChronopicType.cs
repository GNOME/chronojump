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
 */

using System;
using System.Data;

public class MultiChronopicType : EventType 
{
	/*
	   if false, a type doesn't need sync
	   if true, it can be synced or not depending on checkbox active by user
	   */
	bool syncNeeded;

	public MultiChronopicType() {
		type = Types.MULTICHRONOPIC;
	}
	
	//predefined values
	public MultiChronopicType(string name) {
		type = Types.MULTICHRONOPIC;
		this.name = name;
		
		//if this changes, sqlite/pulseType.cs initialize table should change
		if(name == "multiChronopic") {
			syncNeeded = true;
			imageFileName = "multiChronopic.png";
			description = "";
			longDescription = ""; 

		} else if(name == "runAnalysis") {
			syncNeeded = false;
			imageFileName = "run_analysis.png";
			description = "";
			longDescription = ""; 
		}
	}

	public bool SyncNeeded {
		get { return syncNeeded; }
	}
}

