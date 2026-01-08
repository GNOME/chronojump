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
 * Copyright (C) 2022  Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using System.Collections.Generic; //List


/*
   This is written by capture thread and readed by GTK thread.
   Manages the icon shown on jumping.
   */

public class JumpChangeImage
{
	public enum Types { NONE, AIR, LAND }
	private Types last;
	private Types current;

	//constructor, don't show any image
	public JumpChangeImage()
	{
		last = Types.NONE;
		current = Types.NONE;
	}

	public bool ShouldBeChanged()
	{
		//LogB.Information (string.Format ("ShouldBeChanged, current: {0}, last: {1}", current, last));
		if(current == last)
			return false;

		last = current;
		return true;
	}

	//accesssor: get/change current image
	public Types Current {
		get { return current; }
		set { current = value; }
	}
}


/*
   This is written by capture thread and readed by GTK thread.
   Manages the icon shown on running.
   Image is a person RUNNING or a PHOTOCELL being shown (when cross it).
   Also on wireless, show the number of the photocell
   */

public class RunChangeImage
{
	public enum Types { NONE, RUNNING, PHOTOCELL }
	private Types last;
	private Types current;
	private int photocell; //0 is a valid value

	//constructor, don't show any image
	public RunChangeImage()
	{
		last = Types.NONE;
		current = Types.NONE;
		photocell = -1;
	}

	public bool ShouldBeChanged()
	{
		if(current == last)
			return false;

		last = current;
		return true;
	}

	//accesssor: get/change current image
	public Types Current {
		get { return current; }
		set { current = value; }
	}

	public int Photocell {
		get { return photocell; }
		set { photocell = value; }
	}

}
