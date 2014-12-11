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
 * Copyright (C) 2004-2014   Xavier de Blas <xaviblas@gmail.com> 
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

	[Widget] Gtk.Frame frame_best_and_worst;
	[Widget] Gtk.Box hbox_jump_best_worst;
	[Widget] Gtk.Box hbox_run_best_worst;

	/* jumps */	
	[Widget] Gtk.Box hbox_jump_conditions;
	[Widget] Gtk.CheckButton checkbutton_jump_tf_tc_best;
	[Widget] Gtk.CheckButton checkbutton_jump_tf_tc_worst;

	[Widget] Gtk.CheckButton checkbutton_height_greater;
	[Widget] Gtk.CheckButton checkbutton_height_lower;
	[Widget] Gtk.CheckButton checkbutton_tf_greater;
	[Widget] Gtk.CheckButton checkbutton_tf_lower;
	[Widget] Gtk.CheckButton checkbutton_tc_greater;
	[Widget] Gtk.CheckButton checkbutton_tc_lower;
	[Widget] Gtk.CheckButton checkbutton_tf_tc_greater;
	[Widget] Gtk.CheckButton checkbutton_tf_tc_lower;
	
	[Widget] Gtk.SpinButton spinbutton_height_greater;
	[Widget] Gtk.SpinButton spinbutton_height_lower;
	[Widget] Gtk.SpinButton spinbutton_tf_greater;
	[Widget] Gtk.SpinButton spinbutton_tf_lower;
	[Widget] Gtk.SpinButton spinbutton_tc_greater;
	[Widget] Gtk.SpinButton spinbutton_tc_lower;
	[Widget] Gtk.SpinButton spinbutton_tf_tc_greater;
	[Widget] Gtk.SpinButton spinbutton_tf_tc_lower;

	/* runs */	
	[Widget] Gtk.Box hbox_run_conditions;
	[Widget] Gtk.CheckButton checkbutton_run_time_best;
	[Widget] Gtk.CheckButton checkbutton_run_time_worst;
	
	[Widget] Gtk.CheckButton checkbutton_time_greater;
	[Widget] Gtk.CheckButton checkbutton_time_lower;

	[Widget] Gtk.SpinButton spinbutton_time_greater;
	[Widget] Gtk.SpinButton spinbutton_time_lower;

	/* encoder */
	[Widget] Gtk.Box hbox_encoder_conditions;
	[Widget] Gtk.CheckButton checkbutton_encoder_height_higher;
	[Widget] Gtk.CheckButton checkbutton_encoder_mean_speed_higher;
	[Widget] Gtk.CheckButton checkbutton_encoder_max_speed_higher;
	[Widget] Gtk.CheckButton checkbutton_encoder_height_lower;
	[Widget] Gtk.CheckButton checkbutton_encoder_mean_speed_lower;
	[Widget] Gtk.CheckButton checkbutton_encoder_max_speed_lower;
	[Widget] Gtk.SpinButton spinbutton_encoder_height_higher;
	[Widget] Gtk.SpinButton spinbutton_encoder_mean_speed_higher;
	[Widget] Gtk.SpinButton spinbutton_encoder_max_speed_higher;
	[Widget] Gtk.SpinButton spinbutton_encoder_height_lower;
	[Widget] Gtk.SpinButton spinbutton_encoder_mean_speed_lower;
	[Widget] Gtk.SpinButton spinbutton_encoder_max_speed_lower;
	[Widget] Gtk.CheckButton checkbutton_encoder_power_higher;
	[Widget] Gtk.CheckButton checkbutton_encoder_peakpower_higher;
	[Widget] Gtk.CheckButton checkbutton_encoder_power_lower;
	[Widget] Gtk.CheckButton checkbutton_encoder_peakpower_lower;
	[Widget] Gtk.SpinButton spinbutton_encoder_power_higher;
	[Widget] Gtk.SpinButton spinbutton_encoder_peakpower_higher;
	[Widget] Gtk.SpinButton spinbutton_encoder_power_lower;
	[Widget] Gtk.SpinButton spinbutton_encoder_peakpower_lower;


	/* bell tests*/	
	[Widget] Gtk.RadioButton radiobutton_test_good;
	[Widget] Gtk.RadioButton radiobutton_test_bad;

	[Widget] Gtk.Button button_test;
	[Widget] Gtk.Button button_close;

	//bells good (green)
	[Widget] Gtk.Image image_repetitive_best_tf_tc;
	[Widget] Gtk.Image image_repetitive_best_time;
	[Widget] Gtk.Image image_repetitive_height_greater;
	[Widget] Gtk.Image image_repetitive_tf_greater;
	[Widget] Gtk.Image image_repetitive_tc_lower;
	[Widget] Gtk.Image image_repetitive_tf_tc_greater;
	[Widget] Gtk.Image image_repetitive_time_lower;
	[Widget] Gtk.Image image_encoder_height_higher;
	[Widget] Gtk.Image image_encoder_mean_speed_higher;
	[Widget] Gtk.Image image_encoder_max_speed_higher;
	[Widget] Gtk.Image image_encoder_power_higher;
	[Widget] Gtk.Image image_encoder_peakpower_higher;
	[Widget] Gtk.Image image_repetitive_test_good;
	//bells bad (red)
	[Widget] Gtk.Image image_repetitive_worst_tf_tc;
	[Widget] Gtk.Image image_repetitive_worst_time;
	[Widget] Gtk.Image image_repetitive_height_lower;
	[Widget] Gtk.Image image_repetitive_tf_lower;
	[Widget] Gtk.Image image_repetitive_tc_greater;
	[Widget] Gtk.Image image_repetitive_tf_tc_lower;
	[Widget] Gtk.Image image_repetitive_time_greater;
	[Widget] Gtk.Image image_encoder_height_lower;
	[Widget] Gtk.Image image_encoder_mean_speed_lower;
	[Widget] Gtk.Image image_encoder_max_speed_lower;
	[Widget] Gtk.Image image_encoder_power_lower;
	[Widget] Gtk.Image image_encoder_peakpower_lower;
	[Widget] Gtk.Image image_repetitive_test_bad;
	
	public Gtk.Button FakeButtonClose;

	//static bool volumeOn;
	bool volumeOn;
	
	static RepetitiveConditionsWindow RepetitiveConditionsWindowBox;
		
	RepetitiveConditionsWindow () {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "repetitive_conditions", "chronojump");
		gladeXML.Autoconnect(this);
		
		//don't show until View is called
		repetitive_conditions.Hide ();

		//put an icon to window
		UtilGtk.IconWindow(repetitive_conditions);
		
		FakeButtonClose = new Gtk.Button();
		
		putNonStandardIcons();
	}

	static public RepetitiveConditionsWindow Create ()
	{
		if (RepetitiveConditionsWindowBox == null) {
			RepetitiveConditionsWindowBox = new RepetitiveConditionsWindow (); 
		}
	
		//don't show until View is called
		//RepetitiveConditionsWindowBox.repetitive_conditions.Hide ();
		
		return RepetitiveConditionsWindowBox;
	}
	
	public void View (Constants.BellModes bellMode, bool volumeOn) {
		//this.volumeOn = volumeOn;

		//when user "deleted_event" the window
		if (RepetitiveConditionsWindowBox == null) {
			RepetitiveConditionsWindowBox = new RepetitiveConditionsWindow (); 
		}
		RepetitiveConditionsWindowBox.showWidgets(bellMode);
		RepetitiveConditionsWindowBox.repetitive_conditions.Show ();
		RepetitiveConditionsWindowBox.volumeOn = volumeOn;
	}

	void showWidgets(Constants.BellModes bellMode) {
		frame_best_and_worst.Hide();
		hbox_jump_best_worst.Hide();
		hbox_run_best_worst.Hide();
		hbox_jump_conditions.Hide();
		hbox_run_conditions.Hide();
		hbox_encoder_conditions.Hide();

		if(bellMode == Constants.BellModes.JUMPS) {
			frame_best_and_worst.Show();
			hbox_jump_best_worst.Show();
			hbox_jump_conditions.Show();
		} else if(bellMode == Constants.BellModes.RUNS) {
			frame_best_and_worst.Show();
			hbox_run_best_worst.Show();
			hbox_run_conditions.Show();
		} else { //encoder
			hbox_encoder_conditions.Show();
		}
	}

	private void putNonStandardIcons() {
		Pixbuf pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_green.png");
		image_repetitive_best_tf_tc.Pixbuf = pixbuf;
		image_repetitive_best_time.Pixbuf = pixbuf;
		image_repetitive_height_greater.Pixbuf = pixbuf;
		image_repetitive_tf_greater.Pixbuf = pixbuf;
		image_repetitive_tc_lower.Pixbuf = pixbuf;
		image_repetitive_tf_tc_greater.Pixbuf = pixbuf;
		image_repetitive_time_lower.Pixbuf = pixbuf;
		image_encoder_height_higher.Pixbuf = pixbuf;
		image_encoder_mean_speed_higher.Pixbuf = pixbuf;
		image_encoder_max_speed_higher.Pixbuf = pixbuf;
		image_encoder_power_higher.Pixbuf = pixbuf;
		image_encoder_peakpower_higher.Pixbuf = pixbuf;
		image_repetitive_test_good.Pixbuf = pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_red.png");
		image_repetitive_worst_tf_tc.Pixbuf = pixbuf;
		image_repetitive_worst_time.Pixbuf = pixbuf;
		image_repetitive_height_lower.Pixbuf = pixbuf;
		image_repetitive_tf_lower.Pixbuf = pixbuf;
		image_repetitive_tc_greater.Pixbuf = pixbuf;
		image_repetitive_tf_tc_lower.Pixbuf = pixbuf;
		image_repetitive_time_greater.Pixbuf = pixbuf;
		image_encoder_height_lower.Pixbuf = pixbuf;
		image_encoder_mean_speed_lower.Pixbuf = pixbuf;
		image_encoder_max_speed_lower.Pixbuf = pixbuf;
		image_encoder_power_lower.Pixbuf = pixbuf;
		image_encoder_peakpower_lower.Pixbuf = pixbuf;
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
			new DialogMessage(Constants.MessageTypes.INFO, 
					Catalog.GetString("You need to activate sounds in main window") + 
					" (" + Catalog.GetString("top right") + ")");

	}

	void on_button_close_clicked (object o, EventArgs args)
	{
		RepetitiveConditionsWindowBox.repetitive_conditions.Hide();
		FakeButtonClose.Click();
//		RepetitiveConditionsWindowBox = null;
	}

	void on_delete_event (object o, DeleteEventArgs args)
	{
		//RepetitiveConditionsWindowBox.repetitive_conditions.Hide();
		//RepetitiveConditionsWindowBox = null;
		
		button_close.Click();
		args.RetVal = true;
	}

	public bool VolumeOn {
		set { volumeOn = value; }
	}

	/* Auto.mark checkbox if spinbutton is changed */
	
	/* jumps */
	void on_spinbutton_height_greater_value_changed (object o, EventArgs args) {
		checkbutton_height_greater.Active = true;
	}
	void on_spinbutton_height_lower_value_changed (object o, EventArgs args) {
		checkbutton_height_lower.Active = true;
	}

	void on_spinbutton_tf_greater_value_changed (object o, EventArgs args) {
		checkbutton_tf_greater.Active = true;
	}
	void on_spinbutton_tf_lower_value_changed (object o, EventArgs args) {
		checkbutton_tf_lower.Active = true;
	}

	void on_spinbutton_tc_greater_value_changed (object o, EventArgs args) {
		checkbutton_tc_greater.Active = true;
	}
	void on_spinbutton_tc_lower_value_changed (object o, EventArgs args) {
		checkbutton_tc_lower.Active = true;
	}

	void on_spinbutton_tf_tc_greater_value_changed (object o, EventArgs args) {
		checkbutton_tf_tc_greater.Active = true;
	}
	void on_spinbutton_tf_tc_lower_value_changed (object o, EventArgs args) {
		checkbutton_tf_tc_lower.Active = true;
	}

	/*runs*/
	void on_spinbutton_time_greater_value_changed (object o, EventArgs args) {
		checkbutton_time_greater.Active = true;
	}
	void on_spinbutton_time_lower_value_changed (object o, EventArgs args) {
		checkbutton_time_lower.Active = true;
	}

	/* encoder */
	void on_spinbutton_encoder_height_higher_value_changed (object o, EventArgs args) {
		checkbutton_encoder_height_higher.Active = true;
	}
	void on_spinbutton_encoder_height_lower_value_changed (object o, EventArgs args) {
		checkbutton_encoder_height_lower.Active = true;
	}
	
	void on_spinbutton_encoder_mean_speed_higher_value_changed (object o, EventArgs args) {
		checkbutton_encoder_mean_speed_higher.Active = true;
	}
	void on_spinbutton_encoder_mean_speed_lower_value_changed (object o, EventArgs args) {
		checkbutton_encoder_mean_speed_lower.Active = true;
	}
	
	void on_spinbutton_encoder_max_speed_higher_value_changed (object o, EventArgs args) {
		checkbutton_encoder_max_speed_higher.Active = true;
	}
	void on_spinbutton_encoder_max_speed_lower_value_changed (object o, EventArgs args) {
		checkbutton_encoder_max_speed_lower.Active = true;
	}
	
	void on_spinbutton_encoder_power_higher_value_changed (object o, EventArgs args) {
		checkbutton_encoder_power_higher.Active = true;
	}
	void on_spinbutton_encoder_power_lower_value_changed (object o, EventArgs args) {
		checkbutton_encoder_power_lower.Active = true;
	}

	void on_spinbutton_encoder_peakpower_higher_value_changed (object o, EventArgs args) {
		checkbutton_encoder_peakpower_higher.Active = true;
	}
	void on_spinbutton_encoder_peakpower_lower_value_changed (object o, EventArgs args) {
		checkbutton_encoder_peakpower_lower.Active = true;
	}


	/* JUMPS */
	public bool TfTcBest {
		get { return checkbutton_jump_tf_tc_best.Active; }
	}
	public bool TfTcWorst {
		get { return checkbutton_jump_tf_tc_worst.Active; }
	}

	public bool HeightGreater {
		get { return checkbutton_height_greater.Active; }
	}
	public bool HeightLower {
		get { return checkbutton_height_lower.Active; }
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

	public double HeightGreaterValue {
		get { return Convert.ToDouble(spinbutton_height_greater.Value); }
	}
	public double HeightLowerValue {
		get { return Convert.ToDouble(spinbutton_height_lower.Value); }
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

	/* ENCODER */

	public bool EncoderHeightHigher {
		get { return checkbutton_encoder_height_higher.Active; }
	}
	public double EncoderHeightHigherValue {
		get { return Convert.ToDouble(spinbutton_encoder_height_higher.Value); }
	}
	
	public bool EncoderHeightLower {
		get { return checkbutton_encoder_height_lower.Active; }
	}
	public double EncoderHeightLowerValue {
		get { return Convert.ToDouble(spinbutton_encoder_height_lower.Value); }
	}

	public bool EncoderMeanSpeedHigher {
		get { return checkbutton_encoder_mean_speed_higher.Active; }
	}
	public double EncoderMeanSpeedHigherValue {
		get { return Convert.ToDouble(spinbutton_encoder_mean_speed_higher.Value); }
	}
	
	public bool EncoderMeanSpeedLower {
		get { return checkbutton_encoder_mean_speed_lower.Active; }
	}
	public double EncoderMeanSpeedLowerValue {
		get { return Convert.ToDouble(spinbutton_encoder_mean_speed_lower.Value); }
	}

	public bool EncoderMaxSpeedHigher {
		get { return checkbutton_encoder_max_speed_higher.Active; }
	}
	public double EncoderMaxSpeedHigherValue {
		get { return Convert.ToDouble(spinbutton_encoder_max_speed_higher.Value); }
	}
	
	public bool EncoderMaxSpeedLower {
		get { return checkbutton_encoder_max_speed_lower.Active; }
	}
	public double EncoderMaxSpeedLowerValue {
		get { return Convert.ToDouble(spinbutton_encoder_max_speed_lower.Value); }
	}

	public bool EncoderPowerHigher {
		get { return checkbutton_encoder_power_higher.Active; }
	}
	public int EncoderPowerHigherValue {
		get { return Convert.ToInt32(spinbutton_encoder_power_higher.Value); }
	}
	
	public bool EncoderPowerLower {
		get { return checkbutton_encoder_power_lower.Active; }
	}
	public int EncoderPowerLowerValue {
		get { return Convert.ToInt32(spinbutton_encoder_power_lower.Value); }
	}

	public bool EncoderPeakPowerHigher {
		get { return checkbutton_encoder_peakpower_higher.Active; }
	}
	public int EncoderPeakPowerHigherValue {
		get { return Convert.ToInt32(spinbutton_encoder_peakpower_higher.Value); }
	}

	public bool EncoderPeakPowerLower {
		get { return checkbutton_encoder_peakpower_lower.Active; }
	}
	public int EncoderPeakPowerLowerValue {
		get { return Convert.ToInt32(spinbutton_encoder_peakpower_lower.Value); }
	}

}

