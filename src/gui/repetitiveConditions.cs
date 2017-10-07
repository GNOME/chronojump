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
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
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
	
	[Widget] Gtk.Frame frame_conditions;

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
	[Widget] Gtk.Frame frame_encoder_automatic_conditions;
	[Widget] Gtk.RadioButton radio_encoder_relative_to_set;
	[Widget] Gtk.RadioButton radio_encoder_relative_to_historical;
	[Widget] Gtk.HBox hbox_combo_encoder_variable_automatic;
	[Widget] Gtk.ComboBox combo_encoder_variable_automatic;
	[Widget] Gtk.CheckButton checkbutton_encoder_automatic_greater;
	[Widget] Gtk.CheckButton checkbutton_encoder_automatic_lower;
	[Widget] Gtk.SpinButton spinbutton_encoder_automatic_greater;
	[Widget] Gtk.SpinButton spinbutton_encoder_automatic_lower;

	[Widget] Gtk.VBox vbox_encoder_manual;
	[Widget] Gtk.CheckButton checkbutton_encoder_show_manual_feedback;
	[Widget] Gtk.Notebook notebook_encoder_conditions;
	[Widget] Gtk.CheckButton checkbutton_encoder_height_higher;
	[Widget] Gtk.CheckButton checkbutton_encoder_height_lower;
	[Widget] Gtk.CheckButton checkbutton_encoder_mean_speed_higher;
	[Widget] Gtk.CheckButton checkbutton_encoder_max_speed_higher;
	[Widget] Gtk.CheckButton checkbutton_encoder_mean_speed_lower;
	[Widget] Gtk.CheckButton checkbutton_encoder_max_speed_lower;
	[Widget] Gtk.CheckButton checkbutton_encoder_mean_force_higher;
	[Widget] Gtk.CheckButton checkbutton_encoder_max_force_higher;
	[Widget] Gtk.CheckButton checkbutton_encoder_mean_force_lower;
	[Widget] Gtk.CheckButton checkbutton_encoder_max_force_lower;
	[Widget] Gtk.CheckButton checkbutton_encoder_power_higher;
	[Widget] Gtk.CheckButton checkbutton_encoder_peakpower_higher;
	[Widget] Gtk.CheckButton checkbutton_encoder_power_lower;
	[Widget] Gtk.CheckButton checkbutton_encoder_peakpower_lower;
	[Widget] Gtk.SpinButton spinbutton_encoder_height_higher;
	[Widget] Gtk.SpinButton spinbutton_encoder_height_lower;
	[Widget] Gtk.SpinButton spinbutton_encoder_mean_speed_higher;
	[Widget] Gtk.SpinButton spinbutton_encoder_max_speed_higher;
	[Widget] Gtk.SpinButton spinbutton_encoder_mean_speed_lower;
	[Widget] Gtk.SpinButton spinbutton_encoder_max_speed_lower;
	[Widget] Gtk.SpinButton spinbutton_encoder_mean_force_higher;
	[Widget] Gtk.SpinButton spinbutton_encoder_max_force_higher;
	[Widget] Gtk.SpinButton spinbutton_encoder_mean_force_lower;
	[Widget] Gtk.SpinButton spinbutton_encoder_max_force_lower;
	[Widget] Gtk.SpinButton spinbutton_encoder_power_higher;
	[Widget] Gtk.SpinButton spinbutton_encoder_peakpower_higher;
	[Widget] Gtk.SpinButton spinbutton_encoder_power_lower;
	[Widget] Gtk.SpinButton spinbutton_encoder_peakpower_lower;
	[Widget] Gtk.CheckButton checkbutton_inertial_discard_first_three;


	[Widget] Gtk.Button button_test_good;
	[Widget] Gtk.Label label_test_sound_result;
	[Widget] Gtk.Button button_close;

	//bells good (green)
	[Widget] Gtk.Image image_repetitive_best_tf_tc;
	[Widget] Gtk.Image image_repetitive_best_time;
	[Widget] Gtk.Image image_repetitive_height_greater;
	[Widget] Gtk.Image image_repetitive_tf_greater;
	[Widget] Gtk.Image image_repetitive_tc_lower;
	[Widget] Gtk.Image image_repetitive_tf_tc_greater;
	[Widget] Gtk.Image image_repetitive_time_lower;
	[Widget] Gtk.Image image_repetitive_encoder_automatic_greater;
	[Widget] Gtk.Image image_encoder_height_higher;
	[Widget] Gtk.Image image_encoder_mean_speed_higher;
	[Widget] Gtk.Image image_encoder_max_speed_higher;
	[Widget] Gtk.Image image_encoder_mean_force_higher;
	[Widget] Gtk.Image image_encoder_max_force_higher;
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
	[Widget] Gtk.Image image_repetitive_encoder_automatic_lower;
	[Widget] Gtk.Image image_encoder_height_lower;
	[Widget] Gtk.Image image_encoder_mean_speed_lower;
	[Widget] Gtk.Image image_encoder_max_speed_lower;
	[Widget] Gtk.Image image_encoder_mean_force_lower;
	[Widget] Gtk.Image image_encoder_max_force_lower;
	[Widget] Gtk.Image image_encoder_power_lower;
	[Widget] Gtk.Image image_encoder_peakpower_lower;
	[Widget] Gtk.Image image_repetitive_test_bad;
	
	public Gtk.Button FakeButtonClose;

	//static bool volumeOn;
	bool volumeOn;
	public Preferences.GstreamerTypes gstreamer;

	private double bestSetValue;
	
	static RepetitiveConditionsWindow RepetitiveConditionsWindowBox;
		
	RepetitiveConditionsWindow () {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "repetitive_conditions.glade", "repetitive_conditions", "chronojump");
		gladeXML.Autoconnect(this);
		
		//don't show until View is called
		repetitive_conditions.Hide ();

		//put an icon to window
		UtilGtk.IconWindow(repetitive_conditions);
		
		FakeButtonClose = new Gtk.Button();
		
		createComboEncoderAutomaticVariable();

		bestSetValue = 0;
		notebook_encoder_conditions.CurrentPage = 3; //power

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
	
	public void View (Constants.BellModes bellMode, bool volumeOn, Preferences.GstreamerTypes gstreamer)
	{
		//when user "deleted_event" the window
		if (RepetitiveConditionsWindowBox == null) {
			RepetitiveConditionsWindowBox = new RepetitiveConditionsWindow (); 
		}
		RepetitiveConditionsWindowBox.showWidgets(bellMode);
		RepetitiveConditionsWindowBox.repetitive_conditions.Show ();
		RepetitiveConditionsWindowBox.volumeOn = volumeOn;
		RepetitiveConditionsWindowBox.gstreamer = gstreamer;
	}

	void showWidgets(Constants.BellModes bellMode)
	{
		frame_best_and_worst.Hide();
		frame_conditions.Hide();
		hbox_jump_best_worst.Hide();
		hbox_run_best_worst.Hide();
		hbox_jump_conditions.Hide();
		hbox_run_conditions.Hide();
		frame_encoder_automatic_conditions.Hide();
		vbox_encoder_manual.Hide();
		notebook_encoder_conditions.Hide();
		checkbutton_inertial_discard_first_three.Hide();

		if(bellMode == Constants.BellModes.JUMPS) {
			frame_best_and_worst.Show();
			hbox_jump_best_worst.Show();
			hbox_jump_conditions.Show();
			frame_conditions.Show();
		} else if(bellMode == Constants.BellModes.RUNS) {
			frame_best_and_worst.Show();
			hbox_run_best_worst.Show();
			hbox_run_conditions.Show();
			frame_conditions.Show();
		} else { //encoder (grav and inertial)
			frame_encoder_automatic_conditions.Show();
			vbox_encoder_manual.Show();
			if(checkbutton_encoder_show_manual_feedback.Active)
				notebook_encoder_conditions.Show();

			if(bellMode == Constants.BellModes.ENCODERINERTIAL)
				checkbutton_inertial_discard_first_three.Show();
		}

		label_test_sound_result.Text = "";
	}
		
	private void createComboEncoderAutomaticVariable()
	{
		combo_encoder_variable_automatic = ComboBox.NewText ();

		comboEncoderAutomaticVariableFillThisSet();

		hbox_combo_encoder_variable_automatic.PackStart(combo_encoder_variable_automatic, false, false, 0);
		hbox_combo_encoder_variable_automatic.ShowAll();
		combo_encoder_variable_automatic.Sensitive = true;
	}
	//all values
	private void comboEncoderAutomaticVariableFillThisSet()
	{
		string [] values = { Constants.MeanSpeed, Constants.MaxSpeed, Constants.MeanForce, Constants.MaxForce, Constants.MeanPower, Constants.PeakPower };

		UtilGtk.ComboUpdate(combo_encoder_variable_automatic, values, "");
		combo_encoder_variable_automatic.Active = UtilGtk.ComboMakeActive(combo_encoder_variable_automatic, "Mean power");
	}
	//currently only power
	private void comboEncoderAutomaticVariableFillHistorical()
	{
		string [] values = { Constants.MeanPower };

		UtilGtk.ComboUpdate(combo_encoder_variable_automatic, values, "");
		combo_encoder_variable_automatic.Active = UtilGtk.ComboMakeActive(combo_encoder_variable_automatic, "Mean power");
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
		image_repetitive_encoder_automatic_greater.Pixbuf = pixbuf;
		image_encoder_height_higher.Pixbuf = pixbuf;
		image_encoder_mean_speed_higher.Pixbuf = pixbuf;
		image_encoder_max_speed_higher.Pixbuf = pixbuf;
		image_encoder_mean_force_higher.Pixbuf = pixbuf;
		image_encoder_max_force_higher.Pixbuf = pixbuf;
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
		image_repetitive_encoder_automatic_lower.Pixbuf = pixbuf;
		image_encoder_height_lower.Pixbuf = pixbuf;
		image_encoder_mean_speed_lower.Pixbuf = pixbuf;
		image_encoder_max_speed_lower.Pixbuf = pixbuf;
		image_encoder_mean_force_lower.Pixbuf = pixbuf;
		image_encoder_max_force_lower.Pixbuf = pixbuf;
		image_encoder_power_lower.Pixbuf = pixbuf;
		image_encoder_peakpower_lower.Pixbuf = pixbuf;
		image_repetitive_test_bad.Pixbuf = pixbuf;
	}

	void on_button_test_clicked (object o, EventArgs args)
	{
		if(volumeOn)
		{
			Util.TestSound = true;

			label_test_sound_result.Text = "";
			Util.SoundCodes sc;
			if (o == button_test_good) 
				sc = Util.PlaySound(Constants.SoundTypes.GOOD, true, gstreamer);
			else //button_test_bad
				sc = Util.PlaySound(Constants.SoundTypes.BAD, true, gstreamer);

			if(sc == Util.SoundCodes.OK)
				label_test_sound_result.Text = Catalog.GetString("Sound working");
			else
				label_test_sound_result.Text = Catalog.GetString("Sound not working");

			Util.TestSound = false;
		} else
			new DialogMessage(Constants.MessageTypes.INFO, 
					Catalog.GetString("You need to activate sounds in preferences / multimedia."));

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

	public bool FeedbackActive (Constants.BellModes bellMode)
	{
		if(bellMode == Constants.BellModes.JUMPS)
		{
			if(checkbutton_height_greater.Active || checkbutton_height_lower.Active ||
					checkbutton_tf_greater.Active || checkbutton_tf_lower.Active ||
					checkbutton_tc_lower.Active || checkbutton_tc_greater.Active ||
					checkbutton_tf_tc_greater.Active || checkbutton_tf_tc_lower.Active)
				return true;
		}
		else if(bellMode == Constants.BellModes.RUNS)
		{
			if(checkbutton_time_lower.Active || checkbutton_time_greater.Active)
				return true;
		}
		else { //encoder (grav and inertial)
			if(checkbutton_encoder_automatic_greater.Active || checkbutton_encoder_automatic_lower.Active ||
					checkbutton_encoder_height_higher.Active || checkbutton_encoder_height_lower.Active ||
					checkbutton_encoder_mean_speed_higher.Active || checkbutton_encoder_mean_speed_lower.Active ||
					checkbutton_encoder_max_speed_higher.Active || checkbutton_encoder_max_speed_lower.Active ||
					checkbutton_encoder_mean_force_higher.Active || checkbutton_encoder_mean_force_lower.Active ||
					checkbutton_encoder_max_force_higher.Active || checkbutton_encoder_max_force_lower.Active ||
					checkbutton_encoder_power_higher.Active || checkbutton_encoder_power_lower.Active ||
					checkbutton_encoder_peakpower_higher.Active || checkbutton_encoder_peakpower_lower.Active)
				return true;
		}

		return false;
	}

	public bool VolumeOn {
		set { volumeOn = value; }
	}
	public Preferences.GstreamerTypes Gstreamer {
		set { gstreamer = value; }
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

	void on_radio_encoder_relative_to_toggled (object o, EventArgs args)
	{
		if(radio_encoder_relative_to_set.Active)
			comboEncoderAutomaticVariableFillThisSet();
		else
			comboEncoderAutomaticVariableFillHistorical();
	}

	void on_spinbutton_encoder_automatic_greater_value_changed (object o, EventArgs args) {
		checkbutton_encoder_automatic_greater.Active = true;
	}
	void on_spinbutton_encoder_automatic_lower_value_changed (object o, EventArgs args) {
		checkbutton_encoder_automatic_lower.Active = true;
	}
			
	void on_checkbutton_encoder_show_manual_feedback_toggled (object o, EventArgs args) {
		if(checkbutton_encoder_show_manual_feedback.Active)
			notebook_encoder_conditions.Show();
		else
			notebook_encoder_conditions.Hide();
	}
	
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
	
	void on_spinbutton_encoder_mean_force_higher_value_changed (object o, EventArgs args) {
		checkbutton_encoder_mean_force_higher.Active = true;
	}
	void on_spinbutton_encoder_mean_force_lower_value_changed (object o, EventArgs args) {
		checkbutton_encoder_mean_force_lower.Active = true;
	}
	
	void on_spinbutton_encoder_max_force_higher_value_changed (object o, EventArgs args) {
		checkbutton_encoder_max_force_higher.Active = true;
	}
	void on_spinbutton_encoder_max_force_lower_value_changed (object o, EventArgs args) {
		checkbutton_encoder_max_force_lower.Active = true;
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

			
	public void ResetBestSetValue() {
	       bestSetValue = 0;	
	}

	public void UpdateBestSetValue(EncoderCurve curve) 
	{
		string autoVar = encoderAutomaticVariable;
		if(encoderAutomaticHigher || encoderAutomaticLower) 
		{
			if(autoVar == Constants.MeanSpeed)
				UpdateBestSetValue(curve.MeanSpeedD);
			else if(autoVar == Constants.MaxSpeed)
				UpdateBestSetValue(curve.MaxSpeedD);
			else if(autoVar == Constants.MeanPower)
				UpdateBestSetValue(curve.MeanPowerD);
			else if(autoVar == Constants.PeakPower)
				UpdateBestSetValue(curve.PeakPowerD);
			else if(autoVar == Constants.MeanForce)
				UpdateBestSetValue(curve.MeanForceD);
			else if(autoVar == Constants.MaxForce)
				UpdateBestSetValue(curve.MaxForceD);
		}
	}
	public void UpdateBestSetValue(double d) {
		if(d > bestSetValue)
			bestSetValue = d;
	}
		
	//called from gui/encoderTreeviews.cs
	public string AssignColorAutomatic(EncoderCurve curve, string variable)
	{
		if(encoderAutomaticVariable != variable)
			return UtilGtk.ColorNothing;

		double currentValue = curve.GetParameter(variable);

		return AssignColorAutomatic(currentValue);
	}
	//called from gui/encoder.cs plotCurvesGraphDoPlot
	public string AssignColorAutomatic(double currentValue)
	{
		if(encoderAutomaticHigher && currentValue > bestSetValue * encoderAutomaticHigherValue / 100)
			return UtilGtk.ColorGood;
		else if (encoderAutomaticLower && currentValue < bestSetValue * encoderAutomaticLowerValue/ 100)
			return UtilGtk.ColorBad;

		return UtilGtk.ColorNothing;
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
	//automatic

	private string encoderAutomaticVariable {
		get { return UtilGtk.ComboGetActive(combo_encoder_variable_automatic); }
	}

	private bool encoderAutomaticHigher {
		get { return checkbutton_encoder_automatic_greater.Active; }
	}
	private int encoderAutomaticHigherValue {
		get { return Convert.ToInt32(spinbutton_encoder_automatic_greater.Value); }
	}
	private bool encoderAutomaticLower {
		get { return checkbutton_encoder_automatic_lower.Active; }
	}
	private int encoderAutomaticLowerValue {
		get { return Convert.ToInt32(spinbutton_encoder_automatic_lower.Value); }
	}

	public bool EncoderRelativeToSet {
		get { return radio_encoder_relative_to_set.Active; }
	}

	public double GetMainVariableHigher(string mainVariable) 
	{
		if(mainVariable == Constants.MeanSpeed && EncoderMeanSpeedHigher)
			return EncoderMeanSpeedHigherValue;
		else if(mainVariable == Constants.MaxSpeed && EncoderMaxSpeedHigher)
			return EncoderMaxSpeedHigherValue;
		else if(mainVariable == Constants.MeanForce && EncoderMeanForceHigher)
			return EncoderMeanForceHigherValue;
		else if(mainVariable == Constants.MaxForce && EncoderMaxForceHigher)
			return EncoderMaxForceHigherValue;
		else if(mainVariable == Constants.MeanPower && EncoderPowerHigher)
			return EncoderPowerHigherValue;
		else if(mainVariable == Constants.PeakPower && EncoderPeakPowerHigher)
			return EncoderPeakPowerHigherValue;

		return -1;
	}

	public double GetMainVariableLower(string mainVariable) 
	{
		if(mainVariable == Constants.MeanSpeed && EncoderMeanSpeedLower)
			return EncoderMeanSpeedLowerValue;
		else if(mainVariable == Constants.MaxSpeed && EncoderMaxSpeedLower)
			return EncoderMaxSpeedLowerValue;
		else if(mainVariable == Constants.MeanForce && EncoderMeanForceLower)
			return EncoderMeanForceLowerValue;
		else if(mainVariable == Constants.MaxForce && EncoderMaxForceLower)
			return EncoderMaxForceLowerValue;
		else if(mainVariable == Constants.MeanPower && EncoderPowerLower)
			return EncoderPowerLowerValue;
		else if(mainVariable == Constants.PeakPower && EncoderPeakPowerLower)
			return EncoderPeakPowerLowerValue;
			
		return -1;
	}

	public int Notebook_encoder_conditions_page {
		set { notebook_encoder_conditions.CurrentPage = value; }
	}

	public bool Encoder_show_manual_feedback {
		set { checkbutton_encoder_show_manual_feedback.Active = value; }
	}

	//height
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

	//speed
	public bool EncoderMeanSpeedHigher {
		get { return checkbutton_encoder_mean_speed_higher.Active; }
		set { checkbutton_encoder_mean_speed_higher.Active = value; } //used on Compujump
	}
	public double EncoderMeanSpeedHigherValue {
		get { return Convert.ToDouble(spinbutton_encoder_mean_speed_higher.Value); }
		set { spinbutton_encoder_mean_speed_higher.Value = value; } //used on Compujump
	}
	
	public bool EncoderMeanSpeedLower {
		get { return checkbutton_encoder_mean_speed_lower.Active; }
		set { checkbutton_encoder_mean_speed_lower.Active = value; } //used on Compujump
	}
	public double EncoderMeanSpeedLowerValue {
		get { return Convert.ToDouble(spinbutton_encoder_mean_speed_lower.Value); }
		set { spinbutton_encoder_mean_speed_lower.Value = value; } //used on Compujump
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

	//force
	public bool EncoderMeanForceHigher {
		get { return checkbutton_encoder_mean_force_higher.Active; }
	}
	public double EncoderMeanForceHigherValue {
		get { return Convert.ToDouble(spinbutton_encoder_mean_force_higher.Value); }
	}
	
	public bool EncoderMeanForceLower {
		get { return checkbutton_encoder_mean_force_lower.Active; }
	}
	public double EncoderMeanForceLowerValue {
		get { return Convert.ToDouble(spinbutton_encoder_mean_force_lower.Value); }
	}

	public bool EncoderMaxForceHigher {
		get { return checkbutton_encoder_max_force_higher.Active; }
	}
	public double EncoderMaxForceHigherValue {
		get { return Convert.ToDouble(spinbutton_encoder_max_force_higher.Value); }
	}
	
	public bool EncoderMaxForceLower {
		get { return checkbutton_encoder_max_force_lower.Active; }
	}
	public double EncoderMaxForceLowerValue {
		get { return Convert.ToDouble(spinbutton_encoder_max_force_lower.Value); }
	}

	//power & peakPower
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
	
	public bool EncoderInertialDiscardFirstThree {
		get { return checkbutton_inertial_discard_first_three.Active; }
	}

}

