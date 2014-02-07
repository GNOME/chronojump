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
using System.Collections;
using Gtk;
using Gdk;
using Glade;
using Mono.Unix;


public class EncoderConfigurationWindow {
	[Widget] Gtk.Window encoder_configuration;
	[Widget] Gtk.Image image_encoder_linear;
	[Widget] Gtk.Image image_encoder_rotary_friction;
	[Widget] Gtk.Image image_encoder_rotary_axis;
	[Widget] Gtk.Image image_encoder_configuration;
	[Widget] Gtk.Image image_encoder_calcule_im;
	[Widget] Gtk.RadioButton radio_linear;
	[Widget] Gtk.RadioButton radio_rotary_friction;
	[Widget] Gtk.RadioButton radio_rotary_axis;
	[Widget] Gtk.Label label_count;
	[Widget] Gtk.TextView textview;
	[Widget] Gtk.Box hbox_d;
	[Widget] Gtk.Box hbox_D;
	[Widget] Gtk.Box hbox_angle;
	[Widget] Gtk.Box hbox_inertia;
	[Widget] Gtk.Box hbox_inertia2;

	[Widget] Gtk.SpinButton spin_d;
	[Widget] Gtk.SpinButton spin_D;
	[Widget] Gtk.SpinButton spin_angle;
	[Widget] Gtk.SpinButton spin_inertia;
		
	[Widget] Gtk.Box vbox_select_encoder;
	[Widget] Gtk.VSeparator vseparator_im;
	[Widget] Gtk.Box vbox_calcule_im;
	[Widget] Gtk.SpinButton spin_im_weight;
	[Widget] Gtk.SpinButton spin_im_length;
	[Widget] Gtk.SpinButton spin_im_duration;
	[Widget] Gtk.Label label_im_progress;
	[Widget] Gtk.Button button_encoder_capture_inertial_do;

	[Widget] Gtk.Button button_accept;

	static EncoderConfigurationWindow EncoderConfigurationWindowBox;
	
	ArrayList list;
	int listCurrent = 0; //current item on list
	Pixbuf pixbuf;

	EncoderConfigurationWindow () {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "encoder_configuration", null);
		gladeXML.Autoconnect(this);
		
		//three encoder types	
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderTypeLinear);
		image_encoder_linear.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderTypeRotaryFriction);
		image_encoder_rotary_friction.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderTypeRotaryAxis);
		image_encoder_rotary_axis.Pixbuf = pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderCalculeIM);
		image_encoder_calcule_im.Pixbuf = pixbuf;

		//put an icon to window
		UtilGtk.IconWindow(encoder_configuration);
	}
	
	static public EncoderConfigurationWindow View (EncoderConfiguration ec) {
		if (EncoderConfigurationWindowBox == null) {
			EncoderConfigurationWindowBox = new EncoderConfigurationWindow ();
		}
		
		//activate default radiobutton
		if(ec.type == Constants.EncoderType.ROTARYFRICTION)
			EncoderConfigurationWindowBox.radio_rotary_friction.Active = true;
		else if(ec.type == Constants.EncoderType.ROTARYAXIS)
			EncoderConfigurationWindowBox.radio_rotary_axis.Active = true;
		else	//linear
			EncoderConfigurationWindowBox.radio_linear.Active = true;


		EncoderConfigurationWindowBox.initializeList(ec.type, ec.position);
		
		EncoderConfigurationWindowBox.putValuesStoredPreviously(ec.d, ec.D, ec.angle, ec.inertia);
	
		EncoderConfigurationWindowBox.encoder_configuration.Show ();
		return EncoderConfigurationWindowBox;
	}
	
	private void on_radio_encoder_type_linear_toggled (object obj, EventArgs args) {
		if(radio_linear.Active)
			initializeList(Constants.EncoderType.LINEAR, 0);
	}
	private void on_radio_encoder_type_rotary_friction_toggled (object obj, EventArgs args) {
		if(radio_rotary_friction.Active)
			initializeList(Constants.EncoderType.ROTARYFRICTION, 0);
	}
	private void on_radio_encoder_type_rotary_axis_toggled (object obj, EventArgs args) {
		if(radio_rotary_axis.Active)
			initializeList(Constants.EncoderType.ROTARYAXIS, 0);
	}
	
	private void initializeList(Constants.EncoderType type, int position) {
		list = UtilEncoder.EncoderConfigurationList(type);
		listCurrent = position; //current item on list
		
		selectedModeChanged();
	}
	
	private void on_button_previous_clicked (object o, EventArgs args) {
		listCurrent --;
		if(listCurrent < 0)
			listCurrent = list.Count -1;
		
		selectedModeChanged();
	}

	private void on_button_next_clicked (object o, EventArgs args) {
		listCurrent ++;
		if(listCurrent > list.Count -1)
			listCurrent = 0;

		selectedModeChanged();
	}

	private void selectedModeChanged() {
		EncoderConfiguration ec = (EncoderConfiguration) list[listCurrent];
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + ec.image);
		image_encoder_configuration.Pixbuf = pixbuf;
			
		TextBuffer tb1 = new TextBuffer (new TextTagTable());
		tb1.Text = "[" + ec.code + "]\n" + ec.text;
		textview.Buffer = tb1;
		
		hbox_d.Visible = ec.has_d;
		hbox_D.Visible = ec.has_D;
		hbox_angle.Visible = ec.has_angle;
		hbox_inertia.Visible = ec.has_inertia;
		hbox_inertia2.Visible = ec.has_inertia;
		
		label_count.Text = (listCurrent + 1).ToString() + " / " + list.Count.ToString();
	}
	
	private void putValuesStoredPreviously(double d, double D, int angle, int inertia) {
		if(d != -1)
			spin_d.Value = d;
		if(D != -1)
			spin_D.Value = D;
		if(angle != -1)
			spin_angle.Value = angle;
		if(inertia != -1)
			spin_inertia.Value = inertia;
	}
	
	/*
	 * Use this to retrieve values after accept
	 * do not use to know current encoder configuration
	 * because that is stored in gui/encoder as
	 * encoderConfigurationCurrent
	 */
	public EncoderConfiguration GetAcceptedValues() 
	{
		EncoderConfiguration ec = (EncoderConfiguration) list[listCurrent];
		ec.d = (double) spin_d.Value; 
		ec.D = (double) spin_D.Value; 
		ec.angle = (int) spin_angle.Value; 
		ec.inertia = (int) spin_inertia.Value; 

		return ec;
	}
	
	bool show_calcule_im = false;
	void on_button_encoder_capture_inertial_show_clicked (object o, EventArgs args) 
	{
		show_calcule_im = ! show_calcule_im;
		vseparator_im.Visible = show_calcule_im;
		vbox_calcule_im.Visible = show_calcule_im;
	}
	
	void on_button_encoder_capture_inertial_do_clicked (object o, EventArgs args) 
	{
		vbox_select_encoder.Visible = false;
		vseparator_im.Visible = false;
		button_encoder_capture_inertial_do.Sensitive = false;

		//signal is raised and managed in gui/encoder.cs
	}
	
	public void Button_encoder_capture_inertial_do_ended (double imResult) 
	{
		vbox_select_encoder.Visible = true;
		vseparator_im.Visible = true;
		button_encoder_capture_inertial_do.Sensitive = true;
			
		if(imResult != Constants.EncoderErrorCode) {
			label_im_progress.Text = 
				imResult.ToString() + " Kg*cm^2";
			spin_inertia.Value = imResult;
		}
		else 
			label_im_progress.Text = "Error capturing"; 
	}
	
	private void on_button_cancel_clicked (object o, EventArgs args)
	{
		EncoderConfigurationWindowBox.encoder_configuration.Hide();
		EncoderConfigurationWindowBox = null;
	}
	
	private void on_button_accept_clicked (object o, EventArgs args)
	{
		EncoderConfigurationWindowBox.encoder_configuration.Hide();
	}
	
	protected void on_delete_event (object o, DeleteEventArgs args)
	{
		args.RetVal = true;
			
		EncoderConfigurationWindowBox.encoder_configuration.Hide();
		EncoderConfigurationWindowBox = null;
	}

	public Button Button_accept {
		get { return button_accept; }
	}
	
	public Button Button_encoder_capture_inertial_do {
		get { return button_encoder_capture_inertial_do; }
	}
	
	public string Label_im_progress_text {
		set { label_im_progress.Text = value; }
	}
	
	public double Spin_im_weight {
		get { return spin_im_weight.Value; }
	}
	
	public double Spin_im_length {
		get { return spin_im_length.Value; }
	}
	
	public int Spin_im_duration {
		get { return (int) spin_im_duration.Value; }
	}
		
}
