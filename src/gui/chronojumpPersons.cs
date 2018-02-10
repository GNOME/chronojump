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
 * Copyright (C) 2018   Xavier de Blas <xaviblas@gmail.com> 
 */

//this file has methods of ChronoJumpWindow related to manage persons

using System;
using Gtk;
using Glade;

public partial class ChronoJumpWindow
{
	[Widget] Gtk.CheckMenuItem menuitem_view_persons_on_top;
	[Widget] Gtk.CheckMenuItem menuitem_view_persons_show_photo;

	private void on_menuitem_view_persons_on_top_toggled (object o, EventArgs args)
	{
		bool personsOnTop = menuitem_view_persons_on_top.Active;
		LogB.Information("Toggled: " + personsOnTop.ToString());

		SqlitePreferences.Update("personWinHide", personsOnTop.ToString(), false);
		preferences.personWinHide = personsOnTop;
		showPersonsOnTop(personsOnTop);
	}

	private void showPersonsOnTop (bool onTop)
	{
		notebook_session_person.Visible = ! onTop;
		hbox_top_person.Visible = onTop;
		hbox_top_person_encoder.Visible = onTop;

		//show photo option sensitive only when ! onTop
		menuitem_view_persons_show_photo.Sensitive = ! onTop;
	}


	private void on_menuitem_view_persons_show_photo_toggled (object o, EventArgs args)
	{
		bool showPhoto = menuitem_view_persons_show_photo.Active;

		SqlitePreferences.Update("personPhoto", showPhoto.ToString(), false);
		preferences.personPhoto = showPhoto;
		showPersonPhoto(showPhoto);
	}

	private void showPersonPhoto (bool showPhoto)
	{
		if(! menuitem_view_persons_on_top.Active)
		{
			hbox_persons_bottom_photo.Visible = showPhoto;
			hbox_persons_bottom_no_photo.Visible = ! showPhoto;
		}
	}
}
