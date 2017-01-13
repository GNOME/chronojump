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

public class ReactionTimeType : EventType 
{
	public ReactionTimeType() {
		type = Types.REACTIONTIME;
	}
	
	public ReactionTimeType(string name) 
	{
		type = Types.REACTIONTIME;
		this.name = name;
		description = "";
		longDescription = ""; 

		if(name == "reactionTime")
			imageFileName = "reaction_time.png";
		else if(name == "Discriminative")
			imageFileName = "reaction_time_discriminative.png";
		else if(name == "anticipation")
			imageFileName = "reaction_time.png";	//TODO
		else if(name == "flickr")
			imageFileName = "reaction_time.png";	//TODO
	}
	
}

