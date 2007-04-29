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

public class EventType 
{
	public enum Types {
		JUMP, RUN, PULSE, REACTIONTIME
	}

	protected Types type; //jump, run, reactionTime, pulse
	
	protected string name;
	protected bool isPredefined;
	protected string imageFileName;
	protected string description;

	public EventType() {
	}

	public Types Type
	{
		get { return type; }
	}
	
	public string Name
	{
		get { return name; }
		set { name = value; }
	}
	
	public string ImageFileName
	{
		get { return imageFileName; }
	}
	
	public string Description
	{
		get { return description; }
	}
}

