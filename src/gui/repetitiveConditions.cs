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
 * Xavier de Blas: 
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
 */

using System;
using Gtk;
using Gdk;
using Glade;
using Mono.Unix;

public class RepetitiveConditionsWindow 
{
	[Widget] Gtk.Window repetitive_conditions;
//	[Widget] Gtk.ScrolledWindow scrolled_conditions;

	/* jumps */	
	[Widget] Gtk.Table table_jump_conditions;
	[Widget] Gtk.CheckButton checkbutton_jump_tf_tc_best;
	[Widget] Gtk.CheckButton checkbutton_jump_tf_tc_worst;

	[Widget] Gtk.CheckButton checkbutton_tf_greater;
	[Widget] Gtk.CheckButton checkbutton_tf_lower;
	[Widget] Gtk.CheckButton checkbutton_tc_greater;
	[Widget] Gtk.CheckButton checkbutton_tc_lower;
	[Widget] Gtk.CheckButton checkbutton_tf_tc_greater;
	[Widget] Gtk.CheckButton checkbutton_tf_tc_lower;
	
	[Widget] Gtk.SpinButton spinbutton_tf_greater;
	[Widget] Gtk.SpinButton spinbutton_tf_lower;
	[Widget] Gtk.SpinButton spinbutton_tc_greater;
	[Widget] Gtk.SpinButton spinbutton_tc_lower;
	[Widget] Gtk.SpinButton spinbutton_tf_tc_greater;
	[Widget] Gtk.SpinButton spinbutton_tf_tc_lower;

	/* runs */	
	[Widget] Gtk.Table table_run_conditions;
	[Widget] Gtk.CheckButton checkbutton_run_time_best;
	[Widget] Gtk.CheckButton checkbutton_run_time_worst;
	
	[Widget] Gtk.CheckButton checkbutton_time_greater;
	[Widget] Gtk.CheckButton checkbutton_time_lower;

	[Widget] Gtk.SpinButton spinbutton_time_greater;
	[Widget] Gtk.SpinButton spinbutton_time_lower;

	/* bell tests*/	
	[Widget] Gtk.RadioButton radiobutton_test_good;
	[Widget] Gtk.RadioButton radiobutton_test_bad;

	[Widget] Gtk.Button button_test;
	[Widget] Gtk.Button button_close;

	//bells good (green)
	[Widget] Gtk.Image image_repetitive_best_tf_tc;
	[Widget] Gtk.Image image_repetitive_best_time;
	[Widget] Gtk.Image image_repetitive_tf_greater;
	[Widget] Gtk.Image image_repetitive_tc_lower;
	[Widget] Gtk.Image image_repetitive_tf_tc_greater;
	[Widget] Gtk.Image image_repetitive_time_lower;
	[Widget] Gtk.Image image_repetitive_test_good;
	//bells bad (red)
	[Widget] Gtk.Image image_repetitive_worst_tf_tc;
	[Widget] Gtk.Image image_repetitive_worst_time;
	[Widget] Gtk.Image image_repetitive_tf_lower;
	[Widget] Gtk.Image image_repetitive_tc_greater;
	[Widget] Gtk.Image image_repetitive_tf_tc_lower;
	[Widget] Gtk.Image image_repetitive_time_greater;
	[Widget] Gtk.Image image_repetitive_test_bad;

	//static bool volumeOn;
	bool volumeOn;
	
	static RepetitiveConditionsWindow RepetitiveConditionsWindowBox;
		
	RepetitiveConditionsWindow () {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "repetitive_conditions", null);
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(repetitive_conditions);
		
		putNonStandardIcons();
	}

	static public RepetitiveConditionsWindow Create ()
	{
		if (RepetitiveConditionsWindowBox == null) {
			RepetitiveConditionsWindowBox = new RepetitiveConditionsWindow (); 
		}
	
		//don't show until View is called
		RepetitiveConditionsWindowBox.repetitive_conditions.Hide ();
		
		return RepetitiveConditionsWindowBox;
	}
	
	public void View (bool showJumps, bool volumeOn) {
		//this.volumeOn = volumeOn;

		//when user "deleted_event" the window
		if (RepetitiveConditionsWindowBox == null) {
			RepetitiveConditionsWindowBox = new RepetitiveConditionsWindow (); 
		}
		RepetitiveConditionsWindowBox.showWidgets(showJumps);
		RepetitiveConditionsWindowBox.repetitive_conditions.Show ();
		RepetitiveConditionsWindowBox.volumeOn = volumeOn;
	}

	void showWidgets(bool showJumps) {
		if(showJumps) {
			table_jump_conditions.Show();
			checkbutton_jump_tf_tc_best.Show();
			checkbutton_jump_tf_tc_worst.Show();
			table_run_conditions.Hide();
			checkbutton_run_time_best.Hide();
			checkbutton_run_time_worst.Hide();
//			scrolled_conditions	
		} else {
			table_run_conditions.Show();
			checkbutton_run_time_best.Show();
			checkbutton_run_time_worst.Show();
			table_jump_conditions.Hide();
			checkbutton_jump_tf_tc_best.Hide();
			checkbutton_jump_tf_tc_worst.Hide();
		}
	}

	private void putNonStandardIcons() {
		Pixbuf pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_green.png");
		image_repetitive_best_tf_tc.Pixbuf = pixbuf;
		image_repetitive_best_time.Pixbuf = pixbuf;
		image_repetitive_tf_greater.Pixbuf = pixbuf;
		image_repetitive_tc_lower.Pixbuf = pixbuf;
		image_repetitive_tf_tc_greater.Pixbuf = pixbuf;
		image_repetitive_time_lower.Pixbuf = pixbuf;
		image_repetitive_test_good.Pixbuf = pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_red.png");
		image_repetitive_worst_tf_tc.Pixbuf = pixbuf;
		image_repetitive_worst_time.Pixbuf = pixbuf;
		image_repetitive_tf_lower.Pixbuf = pixbuf;
		image_repetitive_tc_greater.Pixbuf = pixbuf;
		image_repetitive_tf_tc_lower.Pixbuf = pixbuf;
		image_repetitive_time_greater.Pixbuf = pixbuf;
		image_repetitive_test_bad.Pixbuf = pixbuf;
	}

	void on_button_test_clicked (object o, EventArgs args)
	{
		if(volumeOn) {
			if (radiobutton_test_good.Active) 
				Util.PlaySound(Constants.SoundTypes.GOOD, true);
			else
				Util.PlaySound(Constants.SoundTypes.BAD, true);
		} else
			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("You need to activate sounds in main window (bottom)"));

	}

	void on_button_close_clicked (object o, EventArgs args)
	{
		RepetitiveConditionsWindowBox.repetitive_conditions.Hide();
//		RepetitiveConditionsWindowBox = null;
	}

	void on_delete_event (object o, DeleteEventArgs args)
	{
		RepetitiveConditionsWindowBox.repetitive_conditions.Hide();
		RepetitiveConditionsWindowBox = null;
	}

	public bool VolumeOn {
		set { volumeOn = value; }
	}

	/* JUMPS */
	public bool TfTcBest {
		get { return checkbutton_jump_tf_tc_best.Active; }
	}
	public bool TfTcWorst {
		get { return checkbutton_jump_tf_tc_worst.Active; }
	}


	public bool TfGreater {
		get { return checkbutton_tf_greater.Active; }
	}

	public bool TfLower {
		get { return checkbutton_tf_lower.Active; }
	}

	public bool TcGreater {
		get { return checkbutton_tc_greater.Active; }
	}

	public bool TcLower {
		get { return checkbutton_tc_lower.Active; }
	}

	public bool TfTcGreater {
		get { return checkbutton_tf_tc_greater.Active; }
	}

	public bool TfTcLower {
		get { return checkbutton_tf_tc_lower.Active; }
	}


	public double TfGreaterValue {
		get { return Convert.ToDouble(spinbutton_tf_greater.Value); }
	}

	public double TfLowerValue {
		get { return Convert.ToDouble(spinbutton_tf_lower.Value); }
	}

	public double TcGreaterValue {
		get { return Convert.ToDouble(spinbutton_tc_greater.Value); }
	}

	public double TcLowerValue {
		get { return Convert.ToDouble(spinbutton_tc_lower.Value); }
	}

	public double TfTcGreaterValue {
		get { return Convert.ToDouble(spinbutton_tf_tc_greater.Value); }
	}

	public double TfTcLowerValue {
		get { return Convert.ToDouble(spinbutton_tf_tc_lower.Value); }
	}

	/* RUNS */
	public bool RunTimeBest {
		get { return checkbutton_run_time_best.Active; }
	}
	public bool RunTimeWorst {
		get { return checkbutton_run_time_worst.Active; }
	}

	public bool RunTimeGreater {
		get { return checkbutton_time_greater.Active; }
	}

	public bool RunTimeLower {
		get { return checkbutton_time_lower.Active; }
	}

	public double RunTimeGreaterValue {
		get { return Convert.ToDouble(spinbutton_time_greater.Value); }
	}

	public double RunTimeLowerValue {
		get { return Convert.ToDouble(spinbutton_time_lower.Value); }
	}

}

