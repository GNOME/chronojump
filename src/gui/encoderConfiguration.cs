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
 * Copyright (C) 2004-2016   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections;
using System.Collections.Generic; //List<T>
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

	//diameter when there's no inertia
	[Widget] Gtk.Box hbox_d; 
	[Widget] Gtk.SpinButton spin_d;

	//diameters whent there's inertia (is plural because there can be many anchorages)
	[Widget] Gtk.Box vbox_d; 
	[Widget] Gtk.Box hbox_list_d;
	[Widget] Gtk.ComboBox combo_d_num;
	[Widget] Gtk.SpinButton spin_d_0;
	[Widget] Gtk.SpinButton spin_d_1;
	[Widget] Gtk.SpinButton spin_d_2;
	[Widget] Gtk.SpinButton spin_d_3;
	[Widget] Gtk.SpinButton spin_d_4;
	[Widget] Gtk.SpinButton spin_d_5;
	[Widget] Gtk.SpinButton spin_d_6;
	[Widget] Gtk.SpinButton spin_d_7;
	[Widget] Gtk.SpinButton spin_d_8;
	[Widget] Gtk.SpinButton spin_d_9;


	[Widget] Gtk.Box hbox_D;
	[Widget] Gtk.Box hbox_angle_push;
	[Widget] Gtk.Box hbox_angle_weight;
	[Widget] Gtk.Box hbox_inertia;
	[Widget] Gtk.Box hbox_inertia_mass;
	[Widget] Gtk.Box hbox_inertia_length;
	[Widget] Gtk.Box vbox_inertia_calcule;

	[Widget] Gtk.SpinButton spin_D;
	[Widget] Gtk.SpinButton spin_angle_push;
	[Widget] Gtk.SpinButton spin_angle_weight;
	[Widget] Gtk.SpinButton spin_inertia_machine;
	[Widget] Gtk.SpinButton spin_inertia_mass; //mass of each of the extra load (weights)
	[Widget] Gtk.SpinButton spin_inertia_length;
	
	[Widget] Gtk.HBox hbox_gearedUp;
	[Widget] Gtk.ComboBox combo_gearedUp;
		
	[Widget] Gtk.Box vbox_select_encoder;
	[Widget] Gtk.VSeparator vseparator_im;
	[Widget] Gtk.Box vbox_calcule_im;
	[Widget] Gtk.SpinButton spin_im_weight_calcule;
	[Widget] Gtk.SpinButton spin_im_length_calcule;
	//[Widget] Gtk.SpinButton spin_im_duration_calcule;
	[Widget] Gtk.Label label_im_result_disc;
	[Widget] Gtk.Label label_im_feedback;
	[Widget] Gtk.Button button_encoder_capture_inertial_do;
	[Widget] Gtk.Button button_encoder_capture_inertial_cancel;
	//[Widget] Gtk.Button button_encoder_capture_inertial_finish;

	[Widget] Gtk.Button button_accept;

	static EncoderConfigurationWindow EncoderConfigurationWindowBox;
	
	ArrayList list;
	int listCurrent = 0; //current item on list
	Pixbuf pixbuf;
	bool definedInConfig;

	EncoderConfigurationWindow (bool definedInConfig) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "encoder_configuration.glade", "encoder_configuration", "chronojump");
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
	
	static public EncoderConfigurationWindow View (bool gravitatory, EncoderConfiguration ec, bool definedInConfig) 
	{
		/*
		 * if we are on gravitatory but ec is inertial, then put definedInConfig as false
		 * and create a new ec that suits
		 */
		if(ec.has_inertia == gravitatory) {
			definedInConfig = false;
			if(gravitatory)
				ec = new EncoderConfiguration(); //LINEAR, not inertial
			else {
				ec = new EncoderConfiguration(Constants.EncoderConfigurationNames.ROTARYAXISINERTIAL);
				ec.SetInertialDefaultOptions();
			}
		}

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
	
		EncoderConfigurationWindowBox.create_list_d_spinbutton();
		
		EncoderConfigurationWindowBox.putValuesStoredPreviously(
				ec.d, ec.list_d, ec.D, ec.anglePush, ec.angleWeight, 
				ec.inertiaMachine, ec.extraWeightGrams, ec.extraWeightLength, 
				ec.has_gearedDown, ec.GearedUpDisplay());
		

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
		
		hbox_d.Visible = (ec.has_d && ! ec.has_inertia);
		vbox_d.Visible = (ec.has_d && ec.has_inertia);

		hbox_D.Visible = ec.has_D;
		hbox_angle_push.Visible = ec.has_angle_push;
		hbox_angle_weight.Visible = ec.has_angle_weight;
		hbox_inertia.Visible = ec.has_inertia;
		hbox_inertia_mass.Visible = ec.has_inertia;
		hbox_inertia_length.Visible = ec.has_inertia;
		vbox_inertia_calcule.Visible = (ec.has_inertia && ! definedInConfig);
		
		hbox_gearedUp.Visible = ec.has_gearedDown;
		if(ec.has_gearedDown)
			combo_gearedUp.Active = UtilGtk.ComboMakeActive(combo_gearedUp, "2");
		
		label_count.Text = (listCurrent + 1).ToString() + " / " + list.Count.ToString();
	
		//hide inertia moment calculation options when change mode
		if(show_calcule_im)
			on_button_encoder_capture_inertial_show_clicked (new object(), new EventArgs());
	}
	
	private void putValuesStoredPreviously(double d, List<double> list_d, double D, int anglePush, int angleWeight, 
			int inertia, int extraWeightGrams, double extraWeightLength, 
			bool has_gearedDown, string gearedUpDisplay) 
	{
		if(d != -1)
			spin_d.Value = d;
		if(list_d != null && list_d.Count > 0) {
			//when there's 1 value in list_d, first value (0) in combo should be selected
			combo_d_num.Active = list_d.Count -1; //this will perform a reset on spinbuttons
		
			int i = 0;
			foreach(Gtk.SpinButton sp in hbox_list_d.Children)
				sp.Value = list_d[i ++];
		}

		if(D != -1)
			spin_D.Value = D;
		if(anglePush != -1)
			spin_angle_push.Value = anglePush;
		if(angleWeight != -1)
			spin_angle_weight.Value = angleWeight;
		if(inertia != -1)
			spin_inertia_machine.Value = inertia;
		if(has_gearedDown)
			combo_gearedUp.Active = UtilGtk.ComboMakeActive(combo_gearedUp, gearedUpDisplay);

		spin_inertia_mass.Value = extraWeightGrams;
		spin_inertia_length.Value = extraWeightLength;
	}
	
	private void create_list_d_spinbutton () {
		/*
		 * adjustment definition:
		 * initial value
		 * minimum value
		 * maximum value
		 * increment for a single step (e.g. one click on an arrow)
		 * increment for a page-up or page-down keypress
		 * page size (using any value other than 0 for this parameter is deprecated) 
		 * 
		 * spinbutton creation (adjustment, climb rate, decimals)
		 */
		spin_d_0 = new Gtk.SpinButton(new Adjustment(4, .5f, 80.0f, .01f, 10.0f, 0f), 1, 2);
		spin_d_1 = new Gtk.SpinButton(new Adjustment(4, .5f, 80.0f, .01f, 10.0f, 0f), 1, 2);
		spin_d_2 = new Gtk.SpinButton(new Adjustment(4, .5f, 80.0f, .01f, 10.0f, 0f), 1, 2);
		spin_d_3 = new Gtk.SpinButton(new Adjustment(4, .5f, 80.0f, .01f, 10.0f, 0f), 1, 2);
		spin_d_4 = new Gtk.SpinButton(new Adjustment(4, .5f, 80.0f, .01f, 10.0f, 0f), 1, 2);
		spin_d_5 = new Gtk.SpinButton(new Adjustment(4, .5f, 80.0f, .01f, 10.0f, 0f), 1, 2);
		spin_d_6 = new Gtk.SpinButton(new Adjustment(4, .5f, 80.0f, .01f, 10.0f, 0f), 1, 2);
		spin_d_7 = new Gtk.SpinButton(new Adjustment(4, .5f, 80.0f, .01f, 10.0f, 0f), 1, 2);
		spin_d_8 = new Gtk.SpinButton(new Adjustment(4, .5f, 80.0f, .01f, 10.0f, 0f), 1, 2);
		spin_d_9 = new Gtk.SpinButton(new Adjustment(4, .5f, 80.0f, .01f, 10.0f, 0f), 1, 2);

		combo_d_num.Active = 0;
		
		reset_hbox_list_d (1);
	}

	private void on_combo_d_num_changed (object o, EventArgs args) {
		reset_hbox_list_d(Convert.ToInt32(UtilGtk.ComboGetActive(combo_d_num)));
	}
	
	void reset_hbox_list_d (int colsNum) 
	{
		foreach(Gtk.SpinButton sp in hbox_list_d.Children)
			hbox_list_d.Remove(sp);

		for (int i = 0; i < colsNum; i ++) {
			switch(i) {
				case 0: 
					hbox_list_d.PackStart(spin_d_0, false, false, 0);
					break;
				case 1: 
					hbox_list_d.PackStart(spin_d_1, false, false, 0);
					break;
				case 2: 
					hbox_list_d.PackStart(spin_d_2, false, false, 0);
					break;
				case 3: 
					hbox_list_d.PackStart(spin_d_3, false, false, 0);
					break;
				case 4: 
					hbox_list_d.PackStart(spin_d_4, false, false, 0);
					break;
				case 5: 
					hbox_list_d.PackStart(spin_d_5, false, false, 0);
					break;
				case 6: 
					hbox_list_d.PackStart(spin_d_6, false, false, 0);
					break;
				case 7: 
					hbox_list_d.PackStart(spin_d_7, false, false, 0);
					break;
				case 8: 
					hbox_list_d.PackStart(spin_d_8, false, false, 0);
					break;
				case 9: 
					hbox_list_d.PackStart(spin_d_9, false, false, 0);
					break;
			}
		}
		hbox_list_d.ShowAll();
	}
	
	private List<double> get_list_d	() {
		List<double> l = new List<double>(); 
		double d = new double();
		foreach(Gtk.SpinButton sp in hbox_list_d.Children) {
			d = (double) sp.Value;
			l.Add(d);
		}
		return l;
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
		ec.list_d = new List<double>(); 
		ec.D = -1;
		ec.anglePush = -1;
		ec.angleWeight = -1;
		ec.inertiaMachine = -1;
		
		if(ec.has_d) {
			if(ec.has_inertia) {
				ec.list_d = get_list_d();
				ec.d = ec.list_d[0]; //selected value is the first
			}
			else
				ec.d = (double) spin_d.Value; 
		}

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

		if(ec.has_gearedDown) {
			ec.SetGearedDownFromDisplay(UtilGtk.ComboGetActive(combo_gearedUp));
		}

		return ec;
	}
	
	void on_button_encoder_capture_inertial_accuracy_clicked (object o, EventArgs args) {
		new DialogMessage(Constants.MessageTypes.WARNING, 
				Catalog.GetString("Calculation of dynamic variables like power in conical machines is not very accurate because current method is not using the variation of the cone diameter as a variable.") + "\n\n" +
				Catalog.GetString("Future versions will include a better way to calcule this. Sorry for the inconvenience."));
	}
	
	bool show_calcule_im = false;
	int windowWidth;
	int windowHeight;
	void on_button_encoder_capture_inertial_show_clicked (object o, EventArgs args) 
	{
		/*
		 * Window size A
		 * Store window size just before showing side content store gui size.
		 */
		if(! show_calcule_im)
			encoder_configuration.GetSize(out windowWidth, out windowHeight);

		//invert show_calcule_im value
		show_calcule_im = ! show_calcule_im;

		//change gui
		vseparator_im.Visible = show_calcule_im;
		vbox_calcule_im.Visible = show_calcule_im;

		button_encoder_capture_inertial_cancel.Sensitive = ! show_calcule_im;
		//button_encoder_capture_inertial_finish.Sensitive = ! show_calcule_im;

		/*
		 * Window size B
		 * Retrieve window size when side content is hided again
		 */
		if(! show_calcule_im)
			encoder_configuration.Resize(windowWidth, windowHeight);
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
