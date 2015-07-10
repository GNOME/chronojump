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
 * Copyright (C) 2004-2015   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections;
using Gtk;
using Gdk;
using Glade;
using Mono.Unix;


public class EncoderConfigurationWindow 
{
	[Widget] Gtk.Window encoder_configuration;
	[Widget] Gtk.Image image_encoder_linear;
	[Widget] Gtk.Image image_encoder_rotary_friction;
	[Widget] Gtk.Image image_encoder_rotary_axis;
	[Widget] Gtk.Image image_encoder_configuration;
	[Widget] Gtk.Image image_encoder_calcule_im;
	[Widget] Gtk.RadioButton radio_linear;
	[Widget] Gtk.RadioButton radio_rotary_friction;
	[Widget] Gtk.RadioButton radio_rotary_axis;
	
	[Widget] Gtk.RadioButton radio_gravity;
	[Widget] Gtk.RadioButton radio_inertia;
		
	[Widget] Gtk.CheckButton check_rotary_friction_inertia_on_axis;
	[Widget] Gtk.HBox hbox_encoder_types;
	[Widget] Gtk.Alignment alignment_options;
	
	[Widget] Gtk.Button button_previous;
	[Widget] Gtk.Button button_next;
	[Widget] Gtk.Label label_count;

	[Widget] Gtk.TextView textview;
	[Widget] Gtk.Box hbox_d;
	[Widget] Gtk.Box hbox_D;
	[Widget] Gtk.Box hbox_angle_push;
	[Widget] Gtk.Box hbox_angle_weight;
	[Widget] Gtk.Box hbox_inertia;
	[Widget] Gtk.Box hbox_inertia_mass;
	[Widget] Gtk.Box hbox_inertia_length;
	[Widget] Gtk.Box vbox_inertia_calcule;

	[Widget] Gtk.SpinButton spin_d;
	[Widget] Gtk.SpinButton spin_D;
	[Widget] Gtk.SpinButton spin_angle_push;
	[Widget] Gtk.SpinButton spin_angle_weight;
	[Widget] Gtk.SpinButton spin_inertia_machine;
	[Widget] Gtk.SpinButton spin_inertia_mass; //mass of each of the extra load (weights)
	[Widget] Gtk.SpinButton spin_inertia_length;
		
	[Widget] Gtk.Box vbox_select_encoder;
	[Widget] Gtk.VSeparator vseparator_im;
	[Widget] Gtk.Box vbox_calcule_im;
	[Widget] Gtk.SpinButton spin_im_weight_calcule;
	[Widget] Gtk.SpinButton spin_im_length_calcule;
	//[Widget] Gtk.SpinButton spin_im_duration_calcule;
	[Widget] Gtk.Label label_im_result_disc;
	[Widget] Gtk.Label label_im_result_weights;
	[Widget] Gtk.Label label_im_result_total;
	[Widget] Gtk.Label label_im_feedback;
	[Widget] Gtk.Button button_encoder_capture_inertial_do;
	[Widget] Gtk.Button button_encoder_capture_inertial_cancel;
	//[Widget] Gtk.Button button_encoder_capture_inertial_finish;
	[Widget] Gtk.Label label_button_encoder_capture_inertial_do;

	[Widget] Gtk.Button button_accept;

	static EncoderConfigurationWindow EncoderConfigurationWindowBox;
	
	ArrayList list;
	int listCurrent = 0; //current item on list
	Pixbuf pixbuf;
	bool definedInConfig;

	EncoderConfigurationWindow (bool definedInConfig) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "encoder_configuration", "chronojump");
		gladeXML.Autoconnect(this);
		
		this.definedInConfig = definedInConfig;
		
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
	
	static public EncoderConfigurationWindow View (EncoderConfiguration ec, bool definedInConfig) {
		if (EncoderConfigurationWindowBox == null) {
			EncoderConfigurationWindowBox = new EncoderConfigurationWindow (definedInConfig);
		}

		//activate default radiobutton
		if(ec.type == Constants.EncoderType.ROTARYFRICTION)
			EncoderConfigurationWindowBox.radio_rotary_friction.Active = true;
		else if(ec.type == Constants.EncoderType.ROTARYAXIS)
			EncoderConfigurationWindowBox.radio_rotary_axis.Active = true;
		else	//linear
			EncoderConfigurationWindowBox.radio_linear.Active = true;

		if(! ec.has_inertia)
			EncoderConfigurationWindowBox.radio_gravity.Active = true;
		else
			EncoderConfigurationWindowBox.radio_inertia.Active = true;
		
		EncoderConfigurationWindowBox.check_rotary_friction_inertia_on_axis.Active = ec.rotaryFrictionOnAxis;


		EncoderConfigurationWindowBox.initializeList(ec.type, ec.has_inertia, ec.rotaryFrictionOnAxis, ec.position);
		
		EncoderConfigurationWindowBox.putValuesStoredPreviously(
				ec.d, ec.D, ec.anglePush, ec.angleWeight, 
				ec.inertiaMachine, ec.extraWeightGrams, ec.extraWeightLength);
		

		//id definedInConfig then only few things can change
		if(definedInConfig) {
			EncoderConfigurationWindowBox.hbox_encoder_types.Visible = false;
			EncoderConfigurationWindowBox.check_rotary_friction_inertia_on_axis.Visible = false;
			EncoderConfigurationWindowBox.alignment_options.Visible = false;
			EncoderConfigurationWindowBox.vbox_inertia_calcule.Visible = false;
		}
	
		EncoderConfigurationWindowBox.encoder_configuration.Show ();
		return EncoderConfigurationWindowBox;
	}
	
	private void on_radio_encoder_type_linear_toggled (object obj, EventArgs args) {
		if(radio_linear.Active)
			initializeList(Constants.EncoderType.LINEAR, 
					radio_inertia.Active, false, 0);
	}
	private void on_radio_encoder_type_rotary_friction_toggled (object obj, EventArgs args) {
		if(radio_rotary_friction.Active)
			initializeList(Constants.EncoderType.ROTARYFRICTION, 
					radio_inertia.Active, 
					(radio_inertia.Active && check_rotary_friction_inertia_on_axis.Active), 
					0);
	}
	private void on_radio_encoder_type_rotary_axis_toggled (object obj, EventArgs args) {
		if(radio_rotary_axis.Active)
			initializeList(Constants.EncoderType.ROTARYAXIS, 
					radio_inertia.Active, false, 0);
	}
	
	private void on_radio_gravity_toggled (object obj, EventArgs args) {
		if(radio_gravity.Active) {
			if(radio_linear.Active)
				initializeList(Constants.EncoderType.LINEAR, false, false, 0);
			else if(radio_rotary_friction.Active)
				initializeList(Constants.EncoderType.ROTARYFRICTION, false, false, 0);
			else //(radio_rotary_axis.Active)
				initializeList(Constants.EncoderType.ROTARYAXIS, false, false, 0);
		}
	}
	private void on_radio_inertia_toggled (object obj, EventArgs args) {
		if(radio_inertia.Active) {
			if(radio_linear.Active)
				initializeList(Constants.EncoderType.LINEAR, true, false, 0);
			else if(radio_rotary_friction.Active)
				initializeList(Constants.EncoderType.ROTARYFRICTION, true, check_rotary_friction_inertia_on_axis.Active,  0);
			else //(radio_rotary_axis.Active)
				initializeList(Constants.EncoderType.ROTARYAXIS, true, false, 0);
		}
	}

	private void check_rotary_friction_inertia_on_axis_is_visible() {
		check_rotary_friction_inertia_on_axis.Visible = (radio_rotary_friction.Active && ! radio_gravity.Active);
	}
	
	private void on_check_rotary_friction_inertia_on_axis_toggled (object obj, EventArgs args) {
		on_radio_inertia_toggled(obj, args);
	}
	

	private void initializeList(Constants.EncoderType type, bool inertial, bool rotaryFrictionOnAxis, int position) 
	{
		check_rotary_friction_inertia_on_axis_is_visible();

		list = UtilEncoder.EncoderConfigurationList(type, inertial, rotaryFrictionOnAxis);

		listCurrent = position; //current item on list
		
		buttons_previous_next_sensitive();
		selectedModeChanged();
	}
	
	private void on_button_previous_clicked (object o, EventArgs args) {
		listCurrent --;
		if(listCurrent < 0)
			listCurrent = 0;
		
		buttons_previous_next_sensitive();
		selectedModeChanged();
	}

	private void on_button_next_clicked (object o, EventArgs args) {
		listCurrent ++;
		if(listCurrent > list.Count -1)
			listCurrent = list.Count -1;
		
		buttons_previous_next_sensitive();
		selectedModeChanged();
	}

	private void buttons_previous_next_sensitive() {
		button_previous.Sensitive = (listCurrent > 0);
		button_next.Sensitive = (listCurrent < list.Count -1);
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
		hbox_angle_push.Visible = ec.has_angle_push;
		hbox_angle_weight.Visible = ec.has_angle_weight;
		hbox_inertia.Visible = ec.has_inertia;
		hbox_inertia_mass.Visible = ec.has_inertia;
		hbox_inertia_length.Visible = ec.has_inertia;
		vbox_inertia_calcule.Visible = (ec.has_inertia && ! definedInConfig);
		
		label_count.Text = (listCurrent + 1).ToString() + " / " + list.Count.ToString();
	
		//hide inertia moment calculation options when change mode
		if(show_calcule_im)
			on_button_encoder_capture_inertial_show_clicked (new object(), new EventArgs());
	}
	
	private void putValuesStoredPreviously(double d, double D, int anglePush, int angleWeight, 
			int inertia, int extraWeightGrams, double extraWeightLength) 
	{
		if(d != -1)
			spin_d.Value = d;
		if(D != -1)
			spin_D.Value = D;
		if(anglePush != -1)
			spin_angle_push.Value = anglePush;
		if(angleWeight != -1)
			spin_angle_weight.Value = angleWeight;
		if(inertia != -1)
			spin_inertia_machine.Value = inertia;
			
		spin_inertia_mass.Value = extraWeightGrams;
		spin_inertia_length.Value = extraWeightLength;
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
		
		ec.d = -1;
		ec.D = -1;
		ec.anglePush = -1;
		ec.angleWeight = -1;
		ec.inertiaMachine = -1;
		
		if(ec.has_d)
			ec.d = (double) spin_d.Value; 

		if(ec.has_D)
			ec.D = (double) spin_D.Value; 

		if(ec.has_angle_push)
			ec.anglePush = (int) spin_angle_push.Value; 

		if(ec.has_angle_weight)
			ec.angleWeight = (int) spin_angle_weight.Value; 

		if(ec.has_inertia) {
			ec.inertiaMachine = (int) spin_inertia_machine.Value; 
			ec.inertiaTotal = (int) spin_inertia_machine.Value; 
			ec.extraWeightGrams = (int) spin_inertia_mass.Value;
			ec.extraWeightLength = (double) spin_inertia_length.Value;
		}

		return ec;
	}
	
	void on_button_encoder_capture_inertial_accuracy_clicked (object o, EventArgs args) {
		new DialogMessage(Constants.MessageTypes.WARNING, 
				Catalog.GetString("Calculation of dynamic variables like power in conical machines is not very accurate because current method is not using the variation of the cone diameter as a variable.") + "\n\n" +
				Catalog.GetString("Future versions will include a better way to calcule this. Sorry for the inconvenience."));
	}
	
	bool show_calcule_im = false;
	void on_button_encoder_capture_inertial_show_clicked (object o, EventArgs args) 
	{
		show_calcule_im = ! show_calcule_im;
		vseparator_im.Visible = show_calcule_im;
		vbox_calcule_im.Visible = show_calcule_im;

		button_encoder_capture_inertial_cancel.Sensitive = ! show_calcule_im;
		//button_encoder_capture_inertial_finish.Sensitive = ! show_calcule_im;
	}
	
	void on_button_encoder_capture_inertial_do_clicked (object o, EventArgs args) 
	{
		//signal is raised and managed in gui/encoder.cs
	}

	bool capturing = false;
	public void Button_encoder_capture_inertial_do_chronopic_ok () 
	{
		vbox_select_encoder.Visible = false;
		vseparator_im.Visible = false;
		button_encoder_capture_inertial_do.Sensitive = false;

		//adapt capture, cancel and finish	
		//label_button_encoder_capture_inertial_do.Visible = false;
		button_encoder_capture_inertial_cancel.Sensitive = true;
		//button_encoder_capture_inertial_finish.Sensitive = true;
		
		label_im_feedback.Text = "<b>" + Catalog.GetString("Capturing") + "</b>";
		label_im_feedback.UseMarkup = true; 
		capturing = true;
	}

	//if error, imResult: 0; message: is error message	
	//if ok, imResult: inertia moment; message: ""	
	public void Button_encoder_capture_inertial_do_ended (double imResult, string message) 
	{
		vbox_select_encoder.Visible = true;
		vseparator_im.Visible = true;
		button_encoder_capture_inertial_do.Sensitive = true;
		
		//adapt capture, cancel and finish	
		//label_button_encoder_capture_inertial_do.Visible = true;
		button_encoder_capture_inertial_cancel.Sensitive = false;
		//button_encoder_capture_inertial_finish.Sensitive = false;
			
		if(imResult == 0) {
			label_im_feedback.Text = "<b>" + message + "</b>";
			label_im_feedback.UseMarkup = true; 
			spin_inertia_machine.Value = imResult;
		} else {
			//label_im_result_disc.Text = Util.TrimDecimals(imResult, 2);
			//as int now
			label_im_result_disc.Text = Convert.ToInt32(imResult).ToString();
			spin_inertia_machine.Value = imResult;
			label_im_feedback.Text = "";
		}
		capturing = false;
	}
	
	void on_button_encoder_capture_inertial_cancel_clicked (object o, EventArgs args) {
		//signal is raised and managed in gui/encoder.cs
		label_im_feedback.Text = "<b>" + Catalog.GetString("Cancelled") + "</b>";
		label_im_feedback.UseMarkup = true; 
		capturing = false;
	}
	/*
	void on_button_encoder_capture_inertial_finish_clicked (object o, EventArgs args) {
		//signal is raised and managed in gui/encoder.cs
	}
	*/
	
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
	
		if(capturing)
			button_encoder_capture_inertial_cancel.Click();
			
		EncoderConfigurationWindowBox.encoder_configuration.Hide();
		EncoderConfigurationWindowBox = null;
	}

	public Button Button_accept {
		get { return button_accept; }
	}
	
	public Button Button_encoder_capture_inertial_do {
		get { return button_encoder_capture_inertial_do; }
	}
	public Button Button_encoder_capture_inertial_cancel {
		get { return button_encoder_capture_inertial_cancel; }
	}
	//public Button Button_encoder_capture_inertial_finish {
	//	get { return button_encoder_capture_inertial_finish; }
	//}
	
	
	public double Spin_im_weight {
		get { return spin_im_weight_calcule.Value; }
	}
	
	public double Spin_im_length {
		get { return spin_im_length_calcule.Value; }
	}
	
	public int Spin_im_duration {
		//get { return (int) spin_im_duration_calcule.Value; }
		//
		//do 60 seconds and it will end automatically when ended
		get { return 60; }
	}
		
}
