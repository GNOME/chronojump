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
using Mono.Unix;

/// <summary>
/// </summary>
public class Platform {

	private bool state; // false (out the platform) , true (inside the platform) 
	private string lastChange;
	private string stream;

	public Platform() 
	{
	}

	public void Initialize()
	{
	}

	public void Close()
	{
	}

	public void ReadStream()
	{
		if(stream[1] == "0") { state = false; }
		else if (stream[1] == "1") { state = true; }
		else { 
			LogB.Error(Catalog.GetString("Error, state '{0}' non valid"), stream[1]);
		}
	}

	public override string ToString()
	{
		return Catalog.GetString("State: {0}, lastChange {1}", state.ToString(), lastChange);
	}
		
	public bool State 
	{
		get
		{
			return platform;
		}
	}
}
