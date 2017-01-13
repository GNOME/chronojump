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
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */


using System;
using Gtk;

/*
 * Unused because moving doesn't look nice on big buttons, store here for the future
 *
public partial class ChronoJumpWindow 
{
	[Widget] Gtk.HBox hbox_start_test;
	[Widget] Gtk.Alignment alignment_start_test_l;
	[Widget] Gtk.Alignment alignment_start_test_m;
	[Widget] Gtk.Alignment alignment_start_test_r;
	[Widget] Gtk.Button button_start_test_l;
	[Widget] Gtk.Button button_start_test_m;
	[Widget] Gtk.Button button_start_test_r;

	MovingStartButton msb_l;	
	MovingStartButton msb_m;	
	MovingStartButton msb_r;	
	int msb_l_pos;
	int msb_m_pos;
	int msb_r_pos;
	bool timerStartTest_l;
	bool timerStartTest_m;
	bool timerStartTest_r;

	private void moveStartTestInitial() 
	{
		int emptySpace = hbox_start_test.Allocation.Width - 
			(button_start_test_l.Allocation.Width + button_start_test_m.Allocation.Width + button_start_test_r.Allocation.Width);
		int leftSpace = Convert.ToInt32(emptySpace / 4); //4: left, between 1-2, between 2-3, right
		msb_l_pos = leftSpace;
		msb_m_pos = leftSpace;
		msb_r_pos = leftSpace;
		alignment_start_test_l.SetPadding(0,0,Convert.ToUInt32(msb_l_pos),0);
		alignment_start_test_m.SetPadding(0,0,Convert.ToUInt32(msb_m_pos),0);
		alignment_start_test_r.SetPadding(0,0,Convert.ToUInt32(msb_r_pos),0);
	}
	
	private void on_alignment_start_l_clicked(object o, EventArgs args)
	{
		msb_m = new MovingStartButton(msb_m_pos, 
				//600, 
				hbox_start_test.Allocation.Width - button_start_test_m.Allocation.Width - button_start_test_r.Allocation.Width,
				MovingStartButton.Dirs.R);
		timerStartTest_l = true;
		GLib.Timeout.Add(1, new GLib.TimeoutHandler(OnTimerStartTest_l));
	}
	//moving m to right will move also r, make him move to left until they collide
	bool OnTimerStartTest_l()
	{
		if (! timerStartTest_l)
			return false;

		if(msb_m != null)
			msb_m.Next();
			
		if(msb_m.Moving) {
			msb_m_pos = msb_m.Pos;

			if( msb_r_pos - msb_m.Speed >= 1)
				msb_r_pos -= msb_m.Speed;
			else
				msb_r_pos = 1;

			alignment_start_test_m.SetPadding(0,0,Convert.ToUInt32(msb_m_pos),0);
			alignment_start_test_r.SetPadding(0,0,Convert.ToUInt32(msb_r_pos),0);

			LogB.Information("mpos: " + msb_m.Pos + "; mspeed: " + msb_m.Speed + "; rpos: " + msb_r_pos);
			return true;
		} else
			return false;
	}
	
	private void on_alignment_start_m_clicked(object o, EventArgs args)
	{
		msb_l = new MovingStartButton(msb_l_pos, 1, MovingStartButton.Dirs.L);
		timerStartTest_m = true;
		GLib.Timeout.Add(1, new GLib.TimeoutHandler(OnTimerStartTest_m));
	}
	//this will move l left, m right to maintain position, r right to the end
	bool OnTimerStartTest_m()
	{
		if (! timerStartTest_m)
			return false;

		if(msb_l != null)
			msb_l.Next();
			
		if(msb_l.Moving) {
			msb_l_pos = msb_l.Pos;

			msb_m_pos += msb_l.Speed;
			msb_r_pos += msb_l.Speed;

			alignment_start_test_l.SetPadding(0,0,Convert.ToUInt32(msb_l_pos),0);
			alignment_start_test_m.SetPadding(0,0,Convert.ToUInt32(msb_m_pos),0);
			alignment_start_test_r.SetPadding(0,0,Convert.ToUInt32(msb_r_pos),0);

			return true;
		} else
			return false;
	}
	
	private void on_alignment_start_r_clicked(object o, EventArgs args)
	{
		msb_m = new MovingStartButton(msb_m_pos, -200, MovingStartButton.Dirs.L);
		timerStartTest_r = true;
		GLib.Timeout.Add(1, new GLib.TimeoutHandler(OnTimerStartTest_r));
	}
	//moving m to left but when colliding with l, move it also
	bool OnTimerStartTest_r()
	{
		if (! timerStartTest_r)
			return false;

		if(msb_m != null)
			msb_m.Next();
			
		if(msb_m.Moving) {
			int msb_l_pos_old = msb_l_pos;
			int msb_m_pos_old = msb_m_pos;

			msb_m_pos = msb_m.Pos;

			if(msb_m_pos <= 1) {
				msb_m_pos = 1;
				msb_l_pos -= msb_m.Speed;
			}

			if(msb_l_pos <= 0) {
				timerStartTest_r = false;
				return false;
			}
			
			msb_r_pos += (msb_l_pos_old - msb_l_pos) + (msb_m_pos_old - msb_m_pos);
			
			alignment_start_test_l.SetPadding(0,0,Convert.ToUInt32(msb_l_pos),0);
			alignment_start_test_m.SetPadding(0,0,Convert.ToUInt32(msb_m_pos),0);
			alignment_start_test_r.SetPadding(0,0,Convert.ToUInt32(msb_r_pos),0);

			return true;
		} else
			return false;
	}
	
	private void on_alignment_start_reset_clicked(object o, EventArgs args)
	{
		moveStartTestInitial();
	}
}
*/
