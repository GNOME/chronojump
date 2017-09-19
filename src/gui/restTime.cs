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
 * Copyright (C) 2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using Gtk;
using Glade;
using System.Collections.Generic; //List

/*
 * All this code is related to top restTimes shown on app1 top
 * when configChronojump.PersonWinHide
 */

public partial class ChronoJumpWindow 
{
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

	List<Gtk.Label> labels_rest_time_contacts_names;
	List<Gtk.Label> labels_rest_time_contacts_times;
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
	}
	private void updateTopRestTimesEncoder()
	{
		labels_rest_time_encoder_clean();
		List<LastTestTime> listLastMin = restTime.LastMinList();
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
	}
}

