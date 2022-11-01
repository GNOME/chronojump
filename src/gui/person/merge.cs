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
 * Copyright (C) 2022   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using System.Collections.Generic; //List

public partial class ChronoJumpWindow
{
	private void on_button_person_merge_clicked (object o, EventArgs args)
	{
		if (currentPerson == null || currentPersonSession == null)
			return;

		Person personToMerge = SqlitePerson.Select (false, 752);
		
		if (personToMerge == null)
			return;

		//person
		currentPerson.MergeWithAnotherGetConflicts (personToMerge);

		//personSession
		List<PersonSession> psCurrentPerson_l = SqlitePersonSession.SelectPersonSessionList (currentPerson.UniqueID, -1);
		List<PersonSession> psMergePerson_l = SqlitePersonSession.SelectPersonSessionList (752, -1);

		foreach (PersonSession psCurrentPerson in psCurrentPerson_l)
			foreach (PersonSession psMergePerson in psMergePerson_l)
			{
				if (psCurrentPerson.SessionID == psMergePerson.SessionID)
				{
					List<ClassVariance.Struct> psPropDiff_l = psCurrentPerson.MergeWithAnotherGetConflicts (psMergePerson);
					if (psPropDiff_l.Count > 0)
						foreach (ClassVariance.Struct cvs in psPropDiff_l)
							LogB.Information (cvs.ToString ());
				}
			}
	}
}
