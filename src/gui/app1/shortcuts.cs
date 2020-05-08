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
 * Copyright (C) 2004-2020   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
using Mono.Unix;

public partial class ChronoJumpWindow 
{
	private void on_shortcuts_clicked (object o, EventArgs args)
	{
		new DialogMessage(
				Catalog.GetString("Use these keys in order to work faster."),
				Constants.MessageTypes.NONE,  //NONE because window is vertically big and the INFO icon looks weird there
				Catalog.GetString("Persons") + ":\n" +
				"\t<tt><b>CTRL+p</b></tt> " + Catalog.GetString("Edit selected person") + "\n" +
				"\t<tt><b>CTRL+" + Catalog.GetString("CURSOR_UP") + "</b></tt> " + Catalog.GetString("Select previous person") + "\n" +
				"\t<tt><b>CTRL+" + Catalog.GetString("CURSOR_DOWN") + "</b></tt> " + Catalog.GetString("Select next person") + "\n" +

				"\n" + Catalog.GetString("Capture tests") + ":\n" +
				"\t<tt><b>CTRL+Space</b></tt> " + Catalog.GetString("Execute test") + "\n" +
				"\t<tt><b>Enter</b></tt> " + Catalog.GetString("Finish test") + "\n" +
				"\t<tt><b>Escape</b></tt> " + Catalog.GetString("Cancel test") + "\n" +

				"\n" + Catalog.GetString("Jumps") + "/" +  Catalog.GetString("Races") + ". " +
				Catalog.GetString("On capture tab:") + "\n" +
				"\t<tt><b>CTRL+v</b></tt> " + Catalog.GetString("Play video of this test") + " " + Catalog.GetString("(if available)")+ "\n" +
				"\t<tt><b>CTRL+d</b></tt> " + Catalog.GetString("Delete this test") + "\n" +

				"\n" + Catalog.GetString("Jumps") + "/" +  Catalog.GetString("Races") + ". " +
				Catalog.GetString("On analyze tab:") + "\n" +
				"\t<tt><b>z</b></tt> " + Catalog.GetString("Zoom change") + "\n" +
				"\t<tt><b>CTRL+v</b></tt> " + Catalog.GetString("Play video of selected test") + " " + Catalog.GetString("(if available)")+ "\n" +
				"\t<tt><b>e</b></tt> " + Catalog.GetString("Edit selected test") + "\n" +
				"\t<tt><b>d</b></tt> " + Catalog.GetString("Delete selected test") + "\n" +
				"\t<tt><b>r</b></tt> " + Catalog.GetString("Repair selected test") + " " + Catalog.GetString("(if available)") + "\n" +

				"\n" + Catalog.GetString("On encoder capture:") + "\n" +
				"\t<tt><b>+</b></tt> " + Catalog.GetString("Add weight") + "\n" +
				"\t<tt><b>-</b></tt> " + Catalog.GetString("Remove weight") + "\n" +

				"\n<tt><b>Escape</b></tt>\n" +
				"\t" + Catalog.GetString("Close any window") + "\n" +
				"\t" + Catalog.GetString("Open menu")
				);
	}
}
