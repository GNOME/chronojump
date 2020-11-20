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
 * Copyright (C) 2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO; 
using Gtk;
using Gdk;
using Glade;
using System.Collections;
using System.Collections.Generic; //List<T>
using Mono.Unix;

//adapted from src/gui/encoderSelectRepetitions.cs
public class TagSessionSelect
{
	private ArrayList data;
	private ArrayList dataPrint;
	private static GenericWindow genericWin;
	private string [] columnsString;
        private ArrayList bigArray;
	private string [] checkboxes;

        //passed variables

	public void Do() {
                getData();
                createBigArray();
                nullifyGenericWindow();
                createGenericWindow();
        }

	private void getData()
        {
		data = SqliteTagSession.Select(false, -1);
        }

	private void createBigArray()
	{
		dataPrint = new ArrayList();
		checkboxes = new string[data.Count]; //to store active or inactive status of tags
		int count = 0;
		foreach(TagSession tagS in data) {
			checkboxes[count++] = "inactive";
			dataPrint.Add(tagS.ToStringArray());
		}

		columnsString = new string[] {
			"ID",
			Catalog.GetString("Name"),
			Catalog.GetString("Color"),
			Catalog.GetString("Comments")
		};

		bigArray = new ArrayList();
		ArrayList a1 = new ArrayList();

		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.TREEVIEW); a1.Add(true); a1.Add("");
		bigArray.Add(a1);
	}

	private void nullifyGenericWindow()
	{
		if(genericWin != null && ! genericWin.GenericWindowBoxIsNull())
			genericWin.HideAndNull();
	}

	private void createGenericWindow()
	{
                genericWin = GenericWindow.Show(Catalog.GetString("Tags"), false,       //don't show now
                                "", bigArray);

		genericWin.SetTreeview(columnsString, true, dataPrint, new ArrayList(), GenericWindow.EditActions.NONE, false);

		//TODO: continue...
	}


       
}
