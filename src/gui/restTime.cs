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
 * Copyright (C) 2017-2020   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
using Glade;
using System.Collections.Generic; //List

/*
 * All this code is related to top restTimes shown on app1 top
 * when configChronojump.PersonWinHide
 */

public partial class ChronoJumpWindow 
{
	//contacts

	[Widget] Gtk.HBox hbox_contacts_rest_time_sofas;
	[Widget] Gtk.Image image_contacts_rest_time_dark_blue;
	[Widget] Gtk.Image image_contacts_rest_time_clear_yellow;
	[Widget] Gtk.ScrolledWindow scrolled_rest_time_contacts;
	[Widget] Gtk.Viewport viewport_rest_time_contacts;
	[Widget] Gtk.Table table_rest_time_contacts;
	[Widget] Gtk.Button button_scrolled_rest_time_contacts_left;
	[Widget] Gtk.Button button_scrolled_rest_time_contacts_right;

	[Widget] Gtk.Label label_contacts_rest_time_1_name;
	[Widget] Gtk.Label label_contacts_rest_time_2_name;
	[Widget] Gtk.Label label_contacts_rest_time_3_name;
	[Widget] Gtk.Label label_contacts_rest_time_4_name;
	[Widget] Gtk.Label label_contacts_rest_time_5_name;

	[Widget] Gtk.Label label_contacts_rest_time_1_time;
	[Widget] Gtk.Label label_contacts_rest_time_2_time;
	[Widget] Gtk.Label label_contacts_rest_time_3_time;
	[Widget] Gtk.Label label_contacts_rest_time_4_time;
	[Widget] Gtk.Label label_contacts_rest_time_5_time;

	List<Gtk.Label> labels_rest_time_contacts_names;
	List<Gtk.Label> labels_rest_time_contacts_times;

	//encoder

	[Widget] Gtk.HBox hbox_encoder_rest_time_sofas;
	[Widget] Gtk.Image image_encoder_rest_time_dark_blue;
	[Widget] Gtk.Image image_encoder_rest_time_clear_yellow;
	[Widget] Gtk.ScrolledWindow scrolled_rest_time_encoder;
	[Widget] Gtk.Viewport viewport_rest_time_encoder;
	[Widget] Gtk.Table table_rest_time_encoder;
	[Widget] Gtk.Button button_scrolled_rest_time_encoder_left;
	[Widget] Gtk.Button button_scrolled_rest_time_encoder_right;

	[Widget] Gtk.Label label_encoder_rest_time_1_name;
	[Widget] Gtk.Label label_encoder_rest_time_2_name;
	[Widget] Gtk.Label label_encoder_rest_time_3_name;
	[Widget] Gtk.Label label_encoder_rest_time_4_name;
	[Widget] Gtk.Label label_encoder_rest_time_5_name;

	[Widget] Gtk.Label label_encoder_rest_time_1_time;
	[Widget] Gtk.Label label_encoder_rest_time_2_time;
	[Widget] Gtk.Label label_encoder_rest_time_3_time;
	[Widget] Gtk.Label label_encoder_rest_time_4_time;
	[Widget] Gtk.Label label_encoder_rest_time_5_time;

	List<Gtk.Label> labels_rest_time_encoder_names;
	List<Gtk.Label> labels_rest_time_encoder_times;

	private void initializeRestTimeLabels()
	{
		labels_rest_time_contacts_names = new List<Gtk.Label>();
		labels_rest_time_contacts_times = new List<Gtk.Label>();
		labels_rest_time_encoder_names = new List<Gtk.Label>();
		labels_rest_time_encoder_times = new List<Gtk.Label>();

		labels_rest_time_contacts_names.Add(label_contacts_rest_time_1_name);
		labels_rest_time_contacts_names.Add(label_contacts_rest_time_2_name);
		labels_rest_time_contacts_names.Add(label_contacts_rest_time_3_name);
		labels_rest_time_contacts_names.Add(label_contacts_rest_time_4_name);
		labels_rest_time_contacts_names.Add(label_contacts_rest_time_5_name);

		labels_rest_time_contacts_times.Add(label_contacts_rest_time_1_time);
		labels_rest_time_contacts_times.Add(label_contacts_rest_time_2_time);
		labels_rest_time_contacts_times.Add(label_contacts_rest_time_3_time);
		labels_rest_time_contacts_times.Add(label_contacts_rest_time_4_time);
		labels_rest_time_contacts_times.Add(label_contacts_rest_time_5_time);

		labels_rest_time_encoder_names.Add(label_encoder_rest_time_1_name);
		labels_rest_time_encoder_names.Add(label_encoder_rest_time_2_name);
		labels_rest_time_encoder_names.Add(label_encoder_rest_time_3_name);
		labels_rest_time_encoder_names.Add(label_encoder_rest_time_4_name);
		labels_rest_time_encoder_names.Add(label_encoder_rest_time_5_name);

		labels_rest_time_encoder_times.Add(label_encoder_rest_time_1_time);
		labels_rest_time_encoder_times.Add(label_encoder_rest_time_2_time);
		labels_rest_time_encoder_times.Add(label_encoder_rest_time_3_time);
		labels_rest_time_encoder_times.Add(label_encoder_rest_time_4_time);
		labels_rest_time_encoder_times.Add(label_encoder_rest_time_5_time);
	}

	private void labels_rest_time_contacts_clean()
	{
		for(int i=0; i < 5; i ++) {
			((Gtk.Label) labels_rest_time_contacts_names[i]).Text = "";
			((Gtk.Label) labels_rest_time_contacts_times[i]).Text = "";
		}
	}
	private void labels_rest_time_encoder_clean()
	{
		for(int i=0; i < 5; i ++) {
			((Gtk.Label) labels_rest_time_encoder_names[i]).Text = "";
			((Gtk.Label) labels_rest_time_encoder_times[i]).Text = "";
		}
	}

	private void updateTopRestTimesContacts()
	{
		labels_rest_time_contacts_clean();
		List<LastTestTime> listLastMin = restTime.LastMinList();
		//hbox_contacts_rest_time_sofas.Visible = (listLastMin.Count > 0);
		int count = 0;
		foreach(LastTestTime ltt in listLastMin)
		{
			if(count < 5) //only 5 values
			{
				((Gtk.Label) labels_rest_time_contacts_names[count]).Text = ltt.PersonName;
				((Gtk.Label) labels_rest_time_contacts_times[count]).Text = ltt.RestedTime;
				count ++;
			}
		}

		//as scrollbar is not usable on tactile screens, and this top rest time are thought for tactile screens
		//show left/right buttons if content is bigger than scrollbar
		if(scrolled_rest_time_contacts.Hadjustment.Upper > scrolled_rest_time_contacts.Hadjustment.PageSize)
		{
			button_scrolled_rest_time_contacts_left.Visible = true;
			button_scrolled_rest_time_contacts_right.Visible = true;

			//make left arrow sensitive if we are not on totally left
			button_scrolled_rest_time_contacts_left.Sensitive =
				(scrolled_rest_time_contacts.Hadjustment.Value > scrolled_rest_time_contacts.Hadjustment.Lower);

			//make right arrow sensitive if we are not on totally right
			button_scrolled_rest_time_contacts_right.Sensitive =
				//(scrolled_rest_time_contacts.Hadjustment.Value < scrolled_rest_time_contacts.Hadjustment.Upper);
				( scrolled_rest_time_contacts.Hadjustment.Value <
				  (scrolled_rest_time_contacts.Hadjustment.Upper - scrolled_rest_time_contacts.Hadjustment.PageSize) );

			hbox_contacts_rest_time_sofas.Visible = false;

		} else {
			button_scrolled_rest_time_contacts_left.Visible = false;
			button_scrolled_rest_time_contacts_right.Visible = false;

			hbox_contacts_rest_time_sofas.Visible = (listLastMin.Count > 0);
		}
	}
	private void updateTopRestTimesEncoder()
	{
		labels_rest_time_encoder_clean();
		List<LastTestTime> listLastMin = restTime.LastMinList();
		//hbox_encoder_rest_time_sofas.Visible = (listLastMin.Count > 0);
		int count = 0;
		foreach(LastTestTime ltt in listLastMin)
		{
			if(count < 5) //only 5 values
			{
				((Gtk.Label) labels_rest_time_encoder_names[count]).Text = ltt.PersonName;
				((Gtk.Label) labels_rest_time_encoder_times[count]).Text = ltt.RestedTime;
				count ++;
			}
		}

		//as scrollbar is not usable on tactile screens, and this top rest time are thought for tactile screens
		//show left/right buttons if content is bigger than scrollbar
		if(scrolled_rest_time_encoder.Hadjustment.Upper > scrolled_rest_time_encoder.Hadjustment.PageSize)
		{
			button_scrolled_rest_time_encoder_left.Visible = true;
			button_scrolled_rest_time_encoder_right.Visible = true;

			//make left arrow sensitive if we are not on totally left
			button_scrolled_rest_time_encoder_left.Sensitive =
				(scrolled_rest_time_encoder.Hadjustment.Value > scrolled_rest_time_encoder.Hadjustment.Lower);

			//make right arrow sensitive if we are not on totally right
			button_scrolled_rest_time_encoder_right.Sensitive =
				//(scrolled_rest_time_encoder.Hadjustment.Value < scrolled_rest_time_encoder.Hadjustment.Upper);
				( scrolled_rest_time_encoder.Hadjustment.Value <
				  (scrolled_rest_time_encoder.Hadjustment.Upper - scrolled_rest_time_encoder.Hadjustment.PageSize) );

			hbox_encoder_rest_time_sofas.Visible = false;

		} else {
			button_scrolled_rest_time_encoder_left.Visible = false;
			button_scrolled_rest_time_encoder_right.Visible = false;

			hbox_encoder_rest_time_sofas.Visible = (listLastMin.Count > 0);
		}
	}

	// left/right buttons

	private void on_scrolled_rest_time_contacts_right (object o, EventArgs args)
	{
		//scrolled_rest_time_contacts.Hadjustment.Value = scrolled_rest_time_contacts.Hadjustment.Upper; //go to the end
		scrolled_rest_time_contacts.Hadjustment.Value += scrolled_rest_time_contacts.Hadjustment.PageSize; //one page to the right
		updateTopRestTimesContacts(); //make the update because if not it looks weird
	}
	private void on_scrolled_rest_time_contacts_left (object o, EventArgs args)
	{
		//scrolled_rest_time_contacts.Hadjustment.Value = scrolled_rest_time_contacts.Hadjustment.Lower; //go to the beginning
		scrolled_rest_time_contacts.Hadjustment.Value -= scrolled_rest_time_contacts.Hadjustment.PageSize; //one page to the right
		updateTopRestTimesContacts(); //make the update because if not it looks weird
	}
	private void on_scrolled_rest_time_encoder_right (object o, EventArgs args)
	{
		//scrolled_rest_time_encoder.Hadjustment.Value = scrolled_rest_time_encoder.Hadjustment.Upper; //go to the end
		scrolled_rest_time_encoder.Hadjustment.Value += scrolled_rest_time_encoder.Hadjustment.PageSize; //one page to the right
		updateTopRestTimesEncoder(); //make the update because if not it looks weird
	}
	private void on_scrolled_rest_time_encoder_left (object o, EventArgs args)
	{
		//scrolled_rest_time_encoder.Hadjustment.Value = scrolled_rest_time_encoder.Hadjustment.Lower; //go to the beginning
		scrolled_rest_time_encoder.Hadjustment.Value -= scrolled_rest_time_encoder.Hadjustment.PageSize; //one page to the right
		updateTopRestTimesEncoder(); //make the update because if not it looks weird
	}
}

