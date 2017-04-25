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
 * Copyright (C) 2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Glade;
using System.Collections.Generic; //List<T>


public partial class ChronoJumpWindow
{
	[Widget] Gtk.TextView textview_encoder_analyze_triggers;
	TriggerList triggerList;

	private void showTriggersAndTab()
	{
		triggerList.Print();
		if(triggerList.Count() > 0)
		{
			//fill textview
			TextBuffer tb1 = new TextBuffer (new TextTagTable());
			tb1.Text = triggerList.ToString();
			textview_encoder_analyze_triggers.Buffer = tb1;
		}

		if(radio_encoder_analyze_individual_current_set.Active && triggerList.Count() > 0)
			showTriggerTab(true);
		else
			showTriggerTab(false);
	}
	
	private void showTriggerTab(bool show)
	{
		if(show)
			notebook_analyze_results.GetNthPage(2).Show();
		else
			notebook_analyze_results.GetNthPage(2).Hide();
	}
}
