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
 * Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic; //List<T>
using Gtk;
using Gdk;
//using Glade;
using Mono.Unix;


public class EncoderConfigurationWindow 
{
	// at glade ---->
	Gtk.Window encoder_configuration;
	Gtk.Image image_encoder_linear;
	Gtk.Image image_encoder_rotary_friction;
	Gtk.Image image_encoder_rotary_axis;
	Gtk.Image image_encoder_configuration;
	Gtk.Image image_encoder_calcule_im;
	Gtk.RadioButton radio_linear;
	Gtk.RadioButton radio_rotary_friction;
	Gtk.RadioButton radio_rotary_axis;

	Gtk.CheckButton check_rotary_friction_inertia_on_axis;
	
	Gtk.Button button_previous;
	Gtk.Button button_next;

	//to colorize
	Gtk.Label label_radio_linear;
	Gtk.Label label_radio_rotary_friction;
	Gtk.Label label_radio_rotary_axis;
	Gtk.Label label_check_rotary_friction_inertia_on_axis;
	Gtk.Label label_selected;
	Gtk.Label label_count;

	Gtk.TextView textview;

	//diameter when there's no inertia
	Gtk.Box hbox_d; 
	Gtk.SpinButton spin_d;

	//diameters whent there's inertia (is plural because there can be many anchorages)
	Gtk.Box vbox_d; 
	Gtk.Box hbox_list_d;
	Gtk.SpinButton spin_d_0;
	Gtk.SpinButton spin_d_1;
	Gtk.SpinButton spin_d_2;
	Gtk.SpinButton spin_d_3;
	Gtk.SpinButton spin_d_4;
	Gtk.SpinButton spin_d_5;
	Gtk.SpinButton spin_d_6;
	Gtk.SpinButton spin_d_7;
	Gtk.SpinButton spin_d_8;
	Gtk.SpinButton spin_d_9;


	Gtk.Box hbox_D;
	Gtk.Box hbox_angle_push;
	Gtk.Box hbox_angle_weight;
	Gtk.Box hbox_inertia;
	Gtk.Box hbox_inertia_mass;
	Gtk.Box hbox_inertia_length;
	Gtk.Box vbox_inertia_calcule;

	Gtk.SpinButton spin_D;
	Gtk.SpinButton spin_angle_push;
	Gtk.SpinButton spin_angle_weight;
	Gtk.SpinButton spin_inertia_machine;
	Gtk.SpinButton spin_inertia_mass; //mass of each of the extra load (weights)
	Gtk.SpinButton spin_inertia_length;
	
	Gtk.HBox hbox_gearedUp;
		
	Gtk.Box vbox_select_encoder;
	Gtk.Frame frame_notebook_side;
	Gtk.Notebook notebook_side;
	Gtk.TreeView treeview_select;
	Gtk.Button button_import;
	Gtk.Button button_export;
	Gtk.Button button_edit;
	Gtk.Button button_add;
	Gtk.Button button_duplicate;
	Gtk.Button button_delete;
	Gtk.Image image_import;
	Gtk.Image image_export;
	Gtk.Image image_capture_cancel;
	Gtk.Image image_cancel;
	Gtk.Image image_accept;
	Gtk.Image image_edit;
	Gtk.Image image_add;
	Gtk.Image image_duplicate;
	Gtk.Image image_delete;
	Gtk.Label label_side_action;
	Gtk.Label label_side_selected;

	Gtk.Notebook notebook_actions;
	Gtk.Entry entry_save_name;
	Gtk.Entry entry_save_description;
	//Gtk.Button button_cancel;
	Gtk.Button button_accept;

	Gtk.SpinButton spin_im_weight_calcule;
	Gtk.SpinButton spin_im_length_calcule;
	//Gtk.SpinButton spin_im_duration_calcule;
	Gtk.Label label_im_result_disc;
	Gtk.Label label_im_feedback;
	Gtk.Button button_encoder_capture_inertial_do;
	Gtk.Button button_encoder_capture_inertial_cancel;
	//Gtk.Button button_encoder_capture_inertial_finish;
	Gtk.Label label_capture_time;
	Gtk.Label label_im_calc_angle;
	Gtk.Label label_im_calc_oscillations;
	Gtk.Grid grid_angle_oscillations;
	Gtk.Box box_im_machine_result;

	Gtk.Box box_combo_d_num;
	Gtk.Box box_combo_gearedUp;

	Gtk.Button button_close;
	// <---- at glade
	

	Gtk.ComboBoxText combo_d_num;
	Gtk.ComboBoxText combo_gearedUp;

	static EncoderConfigurationWindow EncoderConfigurationWindowBox;
	
	ArrayList list;
	int listCurrent = 0; //current item on list
	Pixbuf pixbuf;

	Constants.EncoderGI encoderGI;

	private enum notebookPages { BUTTONS, EDIT_ADD_DUPLICATE, DELETE  };

	/*
	 * this params are used on inertial
	 * and must be retrieved when this window is closed
	 */
	string main_gui_anchorage_str;
	int main_gui_extraWeightN;


	EncoderConfigurationWindow ()
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "encoder_configuration.glade", "encoder_configuration", "chronojump");
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "encoder_configuration.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);
		
		//three encoder types	
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderTypeLinear);
		image_encoder_linear.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderTypeRotaryFriction);
		image_encoder_rotary_friction.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderTypeRotaryAxis);
		image_encoder_rotary_axis.Pixbuf = pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderCalculeIM);
		image_encoder_calcule_im.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_cancel.png");
		image_capture_cancel.Pixbuf = pixbuf;
		image_cancel.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_done_blue.png");
		image_accept.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_edit.png");
		image_edit.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_add.png");
		image_add.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_duplicate.png");
		image_duplicate.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_delete.png");
		image_delete.Pixbuf = pixbuf;

		label_side_action.Text = "";

		//put an icon to window
		UtilGtk.IconWindow(encoder_configuration);
	}

	static public EncoderConfigurationWindow View (
			Constants.EncoderGI encoderGI, EncoderConfigurationSQLObject econfSO,
			string anchorage_str, int extraWeightN)
	{
		if (EncoderConfigurationWindowBox == null) {
			EncoderConfigurationWindowBox = new EncoderConfigurationWindow ();
		}

		string title = Catalog.GetString("Select encoder configuration");
		if(encoderGI == Constants.EncoderGI.GRAVITATORY)
			title += " - " + Catalog.GetString("Gravitatory");
		else if(encoderGI == Constants.EncoderGI.INERTIAL)
			title += " - " + Catalog.GetString("Inertial");

		EncoderConfigurationWindowBox.encoder_configuration.Title = title;

		EncoderConfigurationWindowBox.createCombos ();

		EncoderConfigurationWindowBox.encoderGI = encoderGI;
		EncoderConfigurationWindowBox.updateGUIFromEncoderConfiguration(econfSO.encoderConfiguration);
		EncoderConfigurationWindowBox.main_gui_anchorage_str = anchorage_str;
		EncoderConfigurationWindowBox.main_gui_extraWeightN = extraWeightN;

		EncoderConfigurationWindowBox.createTreeView();
		EncoderConfigurationWindowBox.fillTreeView(
				SqliteEncoderConfiguration.Select(false, encoderGI, ""), //all
				econfSO);

		//A) side is hidden at start to ensure scr_treeview_select is scrolled and displays correctly the last row
		EncoderConfigurationWindowBox.frame_notebook_side.Visible = false;

		//B) done colors here because this win is created only once
		//if done in constructor will not be updated on another color change

		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.WindowColor(EncoderConfigurationWindowBox.encoder_configuration, Config.ColorBackground);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, EncoderConfigurationWindowBox.label_radio_linear);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, EncoderConfigurationWindowBox.label_radio_rotary_friction);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, EncoderConfigurationWindowBox.label_radio_rotary_axis);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, EncoderConfigurationWindowBox.label_check_rotary_friction_inertia_on_axis);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, EncoderConfigurationWindowBox.label_selected);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, EncoderConfigurationWindowBox.label_count);

			UtilGtk.WidgetColor (EncoderConfigurationWindowBox.vbox_select_encoder, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsWidget (Config.ColorBackgroundShiftedIsDark, EncoderConfigurationWindowBox.vbox_select_encoder);
			UtilGtk.WidgetColor (EncoderConfigurationWindowBox.frame_notebook_side, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsWidget (Config.ColorBackgroundShiftedIsDark, EncoderConfigurationWindowBox.frame_notebook_side);

		}

		EncoderConfigurationWindowBox.encoder_configuration.Show ();

		//C) side is shown now, after showing the window in order to be displayed correctly (see A)
		EncoderConfigurationWindowBox.frame_notebook_side.Visible = (EncoderConfigurationWindowBox.sideMode != sideModes.HIDDEN);
		EncoderConfigurationWindowBox.notebook_actions.Page = Convert.ToInt32(notebookPages.BUTTONS);

		return EncoderConfigurationWindowBox;
	}

	private void updateGUIFromEncoderConfiguration(EncoderConfiguration ec)
	{
		//activate default radiobutton
		if(ec.type == Constants.EncoderType.ROTARYFRICTION)
			radio_rotary_friction.Active = true;
		else if(ec.type == Constants.EncoderType.ROTARYAXIS)
			radio_rotary_axis.Active = true;
		else	//linear
			radio_linear.Active = true;

		check_rotary_friction_inertia_on_axis.Active = ec.rotaryFrictionOnAxis;

		initializeList(ec.type, ec.has_inertia, ec.rotaryFrictionOnAxis, ec.position);

		create_list_d_spinbutton();

		putValuesStoredPreviously(
				ec.d, ec.list_d, ec.D, ec.anglePush, ec.angleWeight,
				ec.inertiaMachine, ec.extraWeightGrams, ec.extraWeightLength,
				ec.has_gearedDown, ec.GearedUpDisplay());
	}

	private void on_radio_encoder_type_linear_toggled (object obj, EventArgs args) {
		if(radio_linear.Active)
			initializeList(Constants.EncoderType.LINEAR, 
					encoderGI == Constants.EncoderGI.INERTIAL, false, 0);
	}
	private void on_radio_encoder_type_rotary_friction_toggled (object obj, EventArgs args) {
		if(radio_rotary_friction.Active)
			initializeList(Constants.EncoderType.ROTARYFRICTION, 
					encoderGI == Constants.EncoderGI.INERTIAL,
					(encoderGI == Constants.EncoderGI.INERTIAL && check_rotary_friction_inertia_on_axis.Active),
					0);
	}
	private void on_radio_encoder_type_rotary_axis_toggled (object obj, EventArgs args) {
		if(radio_rotary_axis.Active)
			initializeList(Constants.EncoderType.ROTARYAXIS, 
					encoderGI == Constants.EncoderGI.INERTIAL, false, 0);
	}

	private void check_rotary_friction_inertia_on_axis_is_visible() {
		check_rotary_friction_inertia_on_axis.Visible = (radio_rotary_friction.Active && encoderGI == Constants.EncoderGI.INERTIAL);
	}
	
	private void on_check_rotary_friction_inertia_on_axis_toggled (object obj, EventArgs args) {
		initializeList(Constants.EncoderType.ROTARYFRICTION, true, check_rotary_friction_inertia_on_axis.Active, listCurrent);
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
		vbox_inertia_calcule.Visible = ec.has_inertia;
		
		hbox_gearedUp.Visible = ec.has_gearedDown;
		if(ec.has_gearedDown)
			combo_gearedUp.Active = UtilGtk.ComboMakeActive(combo_gearedUp, "2");
		
		label_count.Text = (listCurrent + 1).ToString() + " / " + list.Count.ToString();
	
		//hide inertia moment calculation options when change mode
		if(sideMode == sideModes.CAPTUREINERTIAL)
			showHideSide(sideModes.HIDDEN);
	}
	
	private void putValuesStoredPreviously(double d, List_d list_d, double D, int anglePush, int angleWeight,
			int inertia, int extraWeightGrams, double extraWeightLength,
			bool has_gearedDown, string gearedUpDisplay)
	{
		if(d != -1)
			spin_d.Value = d;
		if(! list_d.IsEmpty()) {
			//when there's 1 value in list_d, first value (0) in combo should be selected
			combo_d_num.Active = list_d.L.Count -1; //this will perform a reset on spinbuttons
		
			int i = 0;
			foreach(Gtk.SpinButton sp in hbox_list_d.Children)
				sp.Value = list_d.L[i ++];
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

	private void on_combo_d_num_changed (object o, EventArgs args)
	{
		//LogB.Information ("on_combo_d_num_changed: " + UtilGtk.ComboGetActive(combo_d_num));
		if (UtilGtk.ComboGetActive (combo_d_num) != "")
			reset_hbox_list_d (Convert.ToInt32 (UtilGtk.ComboGetActive (combo_d_num)));
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
	
	private List_d get_list_d()
	{
		List_d list_d = new List_d();
		double d = new double();
		foreach(Gtk.SpinButton sp in hbox_list_d.Children) {
			d = (double) sp.Value;
			list_d.Add(d);
		}
		return list_d;
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
		ec.list_d = new List_d();
		ec.D = -1;
		ec.anglePush = -1;
		ec.angleWeight = -1;
		ec.inertiaMachine = -1;
		ec.gearedDown = 1;
		ec.inertiaTotal = -1;
		ec.extraWeightN = 0;
		ec.extraWeightGrams = 0;
		ec.extraWeightLength = 1;
		
		if(ec.has_d) {
			if(ec.has_inertia)
			{
				ec.list_d = get_list_d();

				bool found = false;
				if(Util.IsNumber(main_gui_anchorage_str, true))
				{
					LogB.Information("main_gui_anchorage = " + main_gui_anchorage_str);
					double guiAnchorage = Convert.ToDouble(main_gui_anchorage_str);
					foreach(double d in ec.list_d.L) {
						LogB.Information("d = " + d.ToString());
						if(d == guiAnchorage) {
							ec.d = guiAnchorage;
							found = true;
							break;
						}
					}
				}

				if(! found)
					ec.d = ec.list_d.L[0];
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
			ec.extraWeightN = main_gui_extraWeightN;
			ec.extraWeightGrams = (int) spin_inertia_mass.Value;
			ec.extraWeightLength = (double) spin_inertia_length.Value;

			ec.inertiaTotal = UtilEncoder.CalculeInertiaTotal(ec);
		}

		if(ec.has_gearedDown) {
			ec.SetGearedDownFromDisplay(UtilGtk.ComboGetActive(combo_gearedUp));
		}

		return ec;
	}
	
	void on_button_encoder_capture_inertial_accuracy_clicked (object o, EventArgs args) {
		new DialogMessage(Constants.MessageTypes.WARNING, 
				Catalog.GetString("Calculation of dynamic variables like power in conical machines is not very accurate because current method is not using the variation of the cone diameter as a variable.") + "\n\n" +
				Catalog.GetString("Future versions will include a better way to calculate this. Sorry for the inconvenience."));
	}


	/*
	 * ------------------- side content stuff ----------------->
	 */

	private enum sideModes { HIDDEN, MANAGE, CAPTUREINERTIAL }
	private sideModes sideMode = sideModes.HIDDEN;

	void on_button_manage_show_clicked (object o, EventArgs args)
	{
		if(sideMode == sideModes.MANAGE)
			showHideSide(sideModes.HIDDEN);
		else
			showHideSide(sideModes.MANAGE);
	}
	void on_button_encoder_capture_inertial_show_clicked (object o, EventArgs args)
	{
		if(sideMode == sideModes.CAPTUREINERTIAL)
			showHideSide(sideModes.HIDDEN);
		else
			showHideSide(sideModes.CAPTUREINERTIAL);
	}

	int windowWidth;
	int windowHeight;
	void showHideSide (sideModes newSideMode)
	{
		/*
		 * Window size A
		 * Store window size just before showing side content store gui size.
		 */
		if(sideMode == sideModes.HIDDEN)
			encoder_configuration.GetSize(out windowWidth, out windowHeight);

		//update sideMode value
		sideMode = newSideMode;

		//change gui
		if(sideMode == sideModes.MANAGE)
			notebook_side.CurrentPage = 0;
		else if(sideMode == sideModes.CAPTUREINERTIAL)
			notebook_side.CurrentPage = 1;

		frame_notebook_side.Visible = (sideMode != sideModes.HIDDEN);

		button_encoder_capture_inertial_cancel.Sensitive = (sideMode != sideModes.CAPTUREINERTIAL);
		//button_encoder_capture_inertial_finish.Sensitive = (sideMode != sideModes.CAPTUREINERTIAL);

		/*
		 * Window size B
		 * Retrieve window size when side content is hided again
		 */
		if(sideMode == sideModes.HIDDEN)
			encoder_configuration.Resize(windowWidth, windowHeight);
	}

	/*
	 * <------------------- end of side content stuff -----------------
	 */


	/*
	 * <--------------- side content area / load-save ---->
	 */

	TreeStore store;
	int colName = 0;
	int colDescription = 1;

	private void createTreeView()
	{
		UtilGtk.RemoveColumns(treeview_select);

		treeview_select.HeadersVisible=true;
		int count = 0;
		treeview_select.AppendColumn (Catalog.GetString ("Name"), new CellRendererText(), "text", count++);
		treeview_select.AppendColumn (Catalog.GetString ("Description"), new CellRendererText(), "text", count++);

		treeview_select.Selection.Changed += onTVSelectionChanged;

		store = getStore();
		treeview_select.Model = store;

		Pixbuf pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameImport);
		image_import.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameExport);
		image_export.Pixbuf = pixbuf;
	}

	private void fillTreeView(List<EncoderConfigurationSQLObject> list, EncoderConfigurationSQLObject currentSO)
	{
		foreach (EncoderConfigurationSQLObject econfSO in list)
			store.AppendValues (new string[]{ econfSO.name, econfSO.description });

		UtilGtk.TreeviewSelectRowWithName(treeview_select, store, colName, currentSO.name, true);
	}

	private TreeStore getStore()
	{
		return new TreeStore(typeof (string), typeof (string));
	}

	private string getSelectedName ()
	{
		ITreeModel model;
		TreeIter iter;

		if (treeview_select.Selection.GetSelected(out model, out iter))
			return (string) model.GetValue (iter, colName);

		return "";
	}

	private string getSelectedDescription ()
	{
		ITreeModel model;
		TreeIter iter;

		if (treeview_select.Selection.GetSelected(out model, out iter))
			return (string) model.GetValue (iter, colDescription);

		return "";
	}

	private void onTVSelectionChanged (object o, EventArgs args)
	{
		string selectedName = getSelectedName();
		if(selectedName == "")
			return;

		label_side_selected.Text = "<b>" + selectedName + "</b>";
		label_side_selected.UseMarkup = true;

		List<EncoderConfigurationSQLObject> list = SqliteEncoderConfiguration.Select(false, encoderGI, selectedName);
		if(list != null && list.Count == 1)
		{
			EncoderConfigurationSQLObject econfSO = list[0];
			entry_save_name.Text = econfSO.name;
			entry_save_description.Text = econfSO.description;

			//mark all as unactive
			SqliteEncoderConfiguration.MarkAllAsUnactive(false, encoderGI);
			econfSO.active = true;
			//mark this as active
			SqliteEncoderConfiguration.Update(false, encoderGI, selectedName, econfSO);

			EncoderConfigurationWindowBox.updateGUIFromEncoderConfiguration(econfSO.encoderConfiguration);
		}
	}

	void createCombos ()
	{
		if (combo_d_num == null)
		{
			combo_d_num = UtilGtk.CreateComboBoxText (
					box_combo_d_num,
					new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" },
					"");

			combo_d_num.Changed += new EventHandler (on_combo_d_num_changed);
		}

		if (combo_gearedUp == null)
			combo_gearedUp = UtilGtk.CreateComboBoxText (
					box_combo_gearedUp,
					new List<string> { "4", "3", "2", "1/2", "1/3", "1/4" },
					"");
	}

	void on_entry_save_name_changed	(object o, EventArgs args)
	{
		entry_save_name.Text = Util.MakeValidSQL(entry_save_name.Text);

		button_accept.Sensitive = (entry_save_name.Text.ToString().Length > 0);

		//TODO: button delete sensitivity depends on being on the treeview
	}

	void on_entry_save_description_changed (object o, EventArgs args)
	{
		entry_save_description.Text = Util.MakeValidSQL(entry_save_description.Text);
	}

	void on_button_import_clicked (object o, EventArgs args)
	{
		Gtk.FileChooserDialog fc=
			new Gtk.FileChooserDialog(Catalog.GetString("Select file to import"),
					encoder_configuration,
					FileChooserAction.Open,
					Catalog.GetString("Cancel"),ResponseType.Cancel,
					Catalog.GetString("Accept"),ResponseType.Accept
					);

		fc.Filter = new FileFilter();
		fc.Filter.AddPattern("*.txt");

		if (fc.Run() == (int)ResponseType.Accept)
		{
			try {
				string contents = Util.ReadFile(fc.Filename, false);
				if (contents != null && contents != "")
				{
					EncoderConfigurationSQLObject econfSO = new EncoderConfigurationSQLObject(contents);
					if(econfSO.encoderGI != encoderGI)
					{
						if(encoderGI == Constants.EncoderGI.GRAVITATORY)
							new DialogMessage(Constants.MessageTypes.WARNING,
									Catalog.GetString("Chronojump is currently in gravitory mode.") + "\n" +
									Catalog.GetString("Selected configuration is inertial.") + "\n\n" +
									Catalog.GetString("If you still want to import it, change to inertial mode."));
						else if(encoderGI == Constants.EncoderGI.INERTIAL)
							new DialogMessage(Constants.MessageTypes.WARNING,
									Catalog.GetString("Chronojump is currently in inertial mode.") + "\n" +
									Catalog.GetString("Selected configuration is gravitatory.") + "\n\n" +
									Catalog.GetString("If you still want to import it, change to gravitatory mode."));
					}
					else if(econfSO.name != null && econfSO.name != "")
					{
						//add more suffixes until name is unique
						econfSO.name = SqliteEncoderConfiguration.IfNameExistsAddSuffix(econfSO.name, "_" + Catalog.GetString("copy"));

						SqliteEncoderConfiguration.MarkAllAsUnactive(false, encoderGI);
						econfSO.active = true;
						SqliteEncoderConfiguration.Insert(false, econfSO);

						store.AppendValues (new string[]{ econfSO.name, econfSO.description });
						UtilGtk.TreeviewSelectRowWithName(treeview_select, store, colName, econfSO.name, true);
					}
				}
			}
			catch {
				LogB.Warning("Catched! Configuration cannot be imported");
				new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Error importing data."));
			}
		}
		//Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
		fc.Destroy();
	}

	string exportFileName;
	void on_button_export_clicked (object o, EventArgs args)
	{
		Gtk.FileChooserDialog fc=
			new Gtk.FileChooserDialog(Catalog.GetString("Export to file"),
					encoder_configuration,
					FileChooserAction.Save,
					Catalog.GetString("Cancel"),ResponseType.Cancel,
					Catalog.GetString("Accept"),ResponseType.Accept
					);

		if (fc.Run() == (int)ResponseType.Accept)
		{
			exportFileName = fc.Filename;
			exportFileName = Util.AddTxtIfNeeded(exportFileName);

			try {
				if (File.Exists(exportFileName)) {
					LogB.Information(string.Format(
								"File {0} exists with attributes {1}, created at {2}",
								exportFileName,
								File.GetAttributes(exportFileName),
								File.GetCreationTime(exportFileName)));
					LogB.Information("Overwrite...");
					ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString(
								"Are you sure you want to overwrite file: "), "",
							exportFileName);

					confirmWin.Button_accept.Clicked +=
						new EventHandler(on_overwrite_file_export_accepted);
				} else {
					on_file_export_accepted();

					string myString = string.Format(Catalog.GetString("Saved to {0}"),
							exportFileName);
					new DialogMessage(Constants.MessageTypes.INFO, myString);
				}
			}
			catch {
				string myString = string.Format(
						Catalog.GetString("Cannot save file {0} "), exportFileName);
				new DialogMessage(Constants.MessageTypes.WARNING, myString);
			}
		}
		else {
			LogB.Information("cancelled");
			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Cancelled."));
			fc.Hide ();
			return ;
		}

		//Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
		fc.Destroy();

		return;
	}
	private void on_overwrite_file_export_accepted(object o, EventArgs args)
	{
		on_file_export_accepted();

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}
	private void on_file_export_accepted()
	{
		TextWriter writer = File.CreateText(exportFileName);

		EncoderConfiguration econfOnGUI = GetAcceptedValues();
		EncoderConfigurationSQLObject econfSO = new EncoderConfigurationSQLObject(-1,
				encoderGI, true, entry_save_name.Text.ToString(),
				econfOnGUI, entry_save_description.Text.ToString());
		writer.Write(econfSO.ToFile());

		writer.Close();
		((IDisposable)writer).Dispose();
	}

	//TODO: button_accept sensitive only when name != "" && != of the others
	void on_button_accept_clicked (object o, EventArgs args)
	{
		apply (true);
	}

	private void apply (bool updateGUI)
	{
		//useful fo update SQL
		string selectedOldName = getSelectedName();
		if(selectedOldName == "")
			return;

		string newName = entry_save_name.Text;
		if(newName != selectedOldName)
		{
			/*
			 * if name has changed, then check if newname already exists on database
			 * if exists add _copy recursively
			 */
			newName = SqliteEncoderConfiguration.IfNameExistsAddSuffix(newName, "_" + Catalog.GetString("copy"));
		}
		//update entry_save_name if needed
		if(newName != entry_save_name.Text)
			entry_save_name.Text = newName;

		//update SQL
		EncoderConfiguration econfOnGUI = GetAcceptedValues();
		EncoderConfigurationSQLObject econfSO = new EncoderConfigurationSQLObject(-1,
				encoderGI, true, newName,
				econfOnGUI, entry_save_description.Text.ToString());

		SqliteEncoderConfiguration.Update(false, encoderGI, selectedOldName, econfSO);

		if(updateGUI)
		{
			ITreeModel model;
			TreeIter iter = new TreeIter();
			treeview_select.Selection.GetSelected (out model, out iter);
			store.SetValue (iter, colName, newName);
			store.SetValue (iter, colDescription, entry_save_description.Text);
		}

		notebook_actions.Page = Convert.ToInt32(notebookPages.BUTTONS);
		sideActionGuiUnsensitive (false);
	}

	private void sideActionGuiUnsensitive (bool unsensitivize)
	{
		button_import.Sensitive = ! unsensitivize;
		button_export.Sensitive = ! unsensitivize;
		treeview_select.Sensitive = ! unsensitivize;
	}

	/* right actions */

	private void on_button_side_action_clicked (object o, EventArgs args)
	{
		sideActionGuiUnsensitive (true);

		if(o == (object) button_edit)
			on_button_edit_clicked (o, args);
		else if(o == (object) button_add)
			on_button_add_clicked (o, args);
		else if(o == (object) button_duplicate)
			on_button_duplicate_clicked (o, args);
		else if(o == (object) button_delete)
			on_button_delete_clicked (o, args);
	}

	private void on_button_edit_clicked (object o, EventArgs args)
	{
		notebook_actions.Page = Convert.ToInt32(notebookPages.EDIT_ADD_DUPLICATE);
		label_side_action.Text = Catalog.GetString("Edit selected");

		entry_save_name.Text = getSelectedName ();
		entry_save_description.Text = getSelectedDescription ();
	}

	private void on_button_add_clicked (object o, EventArgs args)
	{
		/*
		TODO: implement on the future
		need to put default config for linear or inertial,
		and change the gui, but if person cancels, then gui has to change again to new object

		notebook_actions.Page = Convert.ToInt32(notebookPages.EDIT_ADD_DUPLICATE);
		label_side_action.Text = Catalog.GetString("Add new");

		entry_save_name.Text = "";
		entry_save_description.Text = "";
		*/
	}

	private void on_button_duplicate_clicked (object o, EventArgs args)
	{
		string selectedName = getSelectedName();
		if(selectedName == "")
			return;

		notebook_actions.Page = Convert.ToInt32(notebookPages.EDIT_ADD_DUPLICATE);
		label_side_action.Text = Catalog.GetString("Duplicate");
		List<EncoderConfigurationSQLObject> list = SqliteEncoderConfiguration.Select(false, encoderGI, selectedName);
		if(list != null && list.Count == 1)
		{
			EncoderConfigurationSQLObject econfSO = list[0];
			econfSO.uniqueID = -1; //to be entered as null and not repeat the uniqueID

			//add a suffix
			econfSO.name += "_"  + Catalog.GetString("copy");
			//add more suffixes until name is unique
			econfSO.name = SqliteEncoderConfiguration.IfNameExistsAddSuffix(econfSO.name, "_" + Catalog.GetString("copy"));

			SqliteEncoderConfiguration.MarkAllAsUnactive(false, encoderGI);
			econfSO.active = true;
			SqliteEncoderConfiguration.Insert(false, econfSO);

			store.AppendValues (new string[]{ econfSO.name, econfSO.description });
			UtilGtk.TreeviewSelectRowWithName(treeview_select, store, colName, econfSO.name, true);
		}
	}

	void on_button_delete_clicked (object o, EventArgs args)
	{
		sideActionGuiUnsensitive (false);

		string selectedName = getSelectedName();
		if(selectedName == "")
			return;

		//notebook_actions.Page = Convert.ToInt32(notebookPages.DELETE);
		//label_side_action.Text = Catalog.GetString("Delete");
		if(UtilGtk.CountRows(store) <= 1) {
			new DialogMessage(Constants.MessageTypes.WARNING,
					Catalog.GetString("Sorry, cannot delete all rows.") + "\n");
			return;
		}

		//treeview
		UtilGtk.RemoveRow(treeview_select, store); 			//1 delete row
		UtilGtk.TreeviewSelectFirstRow(treeview_select, store, true); 	//2 selects another row (use first)

		//SQL.
		//note deleting by name should be only on one encoderGI
		SqliteEncoderConfiguration.Delete (false, encoderGI, selectedName);
	}

	private void on_button_cancel_clicked (object o, EventArgs args)
	{
		notebook_actions.Page = Convert.ToInt32(notebookPages.BUTTONS);
		sideActionGuiUnsensitive (false);
	}

	/*
	 * <--------------- end of side content area / load-save ----
	 */


	/*
	 * ------------------- side content area / capture inertial - ---->
	 */

	void on_button_encoder_capture_inertial_do_clicked (object o, EventArgs args) 
	{
		//signal is raised and managed in gui/encoder.cs
	}

	bool capturing = false;
	public void Button_encoder_capture_inertial_do_chronopic_ok () 
	{
		// 1) don't show vbox_Select_encoder and resize window

		int mainBoxWidth = vbox_select_encoder.Allocation.Width;

		encoder_configuration.GetSize(out windowWidth, out windowHeight);

		vbox_select_encoder.Visible = false;

		encoder_configuration.Resize(windowWidth - mainBoxWidth -15, windowHeight);
		//15 is glade separation between mainBox (vbox_select_encoder) and notebook_side

		// 2) modifiy oother widgets

		button_encoder_capture_inertial_do.Sensitive = false;

		//adapt capture, cancel and finish	
		//label_button_encoder_capture_inertial_do.Visible = false;
		button_encoder_capture_inertial_cancel.Sensitive = true;
		//button_encoder_capture_inertial_finish.Sensitive = true;

		grid_angle_oscillations.Visible = true;
		box_im_machine_result.Visible = false;

		label_im_feedback.Text = "<b>" + Catalog.GetString("Capturing") + "</b>";
		label_im_feedback.UseMarkup = true; 

		// 3) mark capturing starts

		capturing = true;
	}

	public void EncoderReaded(int sum, int oscillations)
	{
		double angle = 0;
		if(sum != 0)
			angle = (sum * 360.0) / 200;
		label_im_calc_angle.Text = Util.TrimDecimals(angle, 1);
		label_im_calc_oscillations.Text = oscillations.ToString();
	}

	public void Label_capture_time(int maxTime, int inactivityTime)
	{
		label_capture_time.Text =
			string.Format(Catalog.GetString("Max time: {0} s."), maxTime.ToString()) + "\n" +
			string.Format(Catalog.GetString("Ends at inactivity during {0} s."), inactivityTime.ToString());
		label_capture_time.Visible = true;
	}

	//if error, imResult: 0; message: is error message	
	//if ok, imResult: inertia moment; message: ""	
	public void Button_encoder_capture_inertial_do_ended (double imResult, string message) 
	{
		vbox_select_encoder.Visible = true;
		button_encoder_capture_inertial_do.Sensitive = true;
		
		//adapt capture, cancel and finish	
		//label_button_encoder_capture_inertial_do.Visible = true;
		button_encoder_capture_inertial_cancel.Sensitive = false;
		//button_encoder_capture_inertial_finish.Sensitive = false;
			
		label_capture_time.Visible = false;

		if(imResult == 0) {
			spin_inertia_machine.Value = imResult;
		} else {
			//label_im_result_disc.Text = Util.TrimDecimals(imResult, 2);
			//as int now
			label_im_result_disc.Text = Convert.ToInt32(imResult).ToString();
			spin_inertia_machine.Value = imResult;
			grid_angle_oscillations.Visible = false;
			box_im_machine_result.Visible = true;
		}

		label_im_feedback.Text = "<b>" + message + "</b>";
		label_im_feedback.UseMarkup = true;

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
	
	/*
	 * <--------------- end of side content area / capture inertial ----
	 */

	/*
	private void on_button_cancel_clicked (object o, EventArgs args)
	{
		EncoderConfigurationWindowBox.encoder_configuration.Hide();
		EncoderConfigurationWindowBox = null;
	}
	*/
	
	private void on_button_close_clicked (object o, EventArgs args)
	{
		apply(false);

		//managed on gui/encoder.cs
		EncoderConfigurationWindowBox.encoder_configuration.Hide();
	}
	
	protected void on_delete_event (object o, DeleteEventArgs args)
	{
		if(capturing)
			button_encoder_capture_inertial_cancel.Click();

		button_close.Click();

		args.RetVal = true;
	}

	public Button Button_close {
		get { return button_close; }
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
	
	public string Entry_save_name {
		get { return entry_save_name.Text; }
	}
	
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
		
	private void connectWidgets (Gtk.Builder builder)
	{
		encoder_configuration = (Gtk.Window) builder.GetObject ("encoder_configuration");
		image_encoder_linear = (Gtk.Image) builder.GetObject ("image_encoder_linear");
		image_encoder_rotary_friction = (Gtk.Image) builder.GetObject ("image_encoder_rotary_friction");
		image_encoder_rotary_axis = (Gtk.Image) builder.GetObject ("image_encoder_rotary_axis");
		image_encoder_configuration = (Gtk.Image) builder.GetObject ("image_encoder_configuration");
		image_encoder_calcule_im = (Gtk.Image) builder.GetObject ("image_encoder_calcule_im");
		radio_linear = (Gtk.RadioButton) builder.GetObject ("radio_linear");
		radio_rotary_friction = (Gtk.RadioButton) builder.GetObject ("radio_rotary_friction");
		radio_rotary_axis = (Gtk.RadioButton) builder.GetObject ("radio_rotary_axis");

		check_rotary_friction_inertia_on_axis = (Gtk.CheckButton) builder.GetObject ("check_rotary_friction_inertia_on_axis");

		button_previous = (Gtk.Button) builder.GetObject ("button_previous");
		button_next = (Gtk.Button) builder.GetObject ("button_next");

		//to colorize
		label_radio_linear = (Gtk.Label) builder.GetObject ("label_radio_linear");
		label_radio_rotary_friction = (Gtk.Label) builder.GetObject ("label_radio_rotary_friction");
		label_radio_rotary_axis = (Gtk.Label) builder.GetObject ("label_radio_rotary_axis");
		label_check_rotary_friction_inertia_on_axis = (Gtk.Label) builder.GetObject ("label_check_rotary_friction_inertia_on_axis");
		label_selected = (Gtk.Label) builder.GetObject ("label_selected");
		label_count = (Gtk.Label) builder.GetObject ("label_count");

		textview = (Gtk.TextView) builder.GetObject ("textview");

		//diameter when there's no inertia
		hbox_d = (Gtk.Box) builder.GetObject ("hbox_d"); 
		spin_d = (Gtk.SpinButton) builder.GetObject ("spin_d");

		//diameters whent there's inertia (is plural because there can be many anchorages)
		vbox_d = (Gtk.Box) builder.GetObject ("vbox_d"); 
		hbox_list_d = (Gtk.Box) builder.GetObject ("hbox_list_d");
		spin_d_0 = (Gtk.SpinButton) builder.GetObject ("spin_d_0");
		spin_d_1 = (Gtk.SpinButton) builder.GetObject ("spin_d_1");
		spin_d_2 = (Gtk.SpinButton) builder.GetObject ("spin_d_2");
		spin_d_3 = (Gtk.SpinButton) builder.GetObject ("spin_d_3");
		spin_d_4 = (Gtk.SpinButton) builder.GetObject ("spin_d_4");
		spin_d_5 = (Gtk.SpinButton) builder.GetObject ("spin_d_5");
		spin_d_6 = (Gtk.SpinButton) builder.GetObject ("spin_d_6");
		spin_d_7 = (Gtk.SpinButton) builder.GetObject ("spin_d_7");
		spin_d_8 = (Gtk.SpinButton) builder.GetObject ("spin_d_8");
		spin_d_9 = (Gtk.SpinButton) builder.GetObject ("spin_d_9");


		hbox_D = (Gtk.Box) builder.GetObject ("hbox_D");
		hbox_angle_push = (Gtk.Box) builder.GetObject ("hbox_angle_push");
		hbox_angle_weight = (Gtk.Box) builder.GetObject ("hbox_angle_weight");
		hbox_inertia = (Gtk.Box) builder.GetObject ("hbox_inertia");
		hbox_inertia_mass = (Gtk.Box) builder.GetObject ("hbox_inertia_mass");
		hbox_inertia_length = (Gtk.Box) builder.GetObject ("hbox_inertia_length");
		vbox_inertia_calcule = (Gtk.Box) builder.GetObject ("vbox_inertia_calcule");

		spin_D = (Gtk.SpinButton) builder.GetObject ("spin_D");
		spin_angle_push = (Gtk.SpinButton) builder.GetObject ("spin_angle_push");
		spin_angle_weight = (Gtk.SpinButton) builder.GetObject ("spin_angle_weight");
		spin_inertia_machine = (Gtk.SpinButton) builder.GetObject ("spin_inertia_machine");
		spin_inertia_mass = (Gtk.SpinButton) builder.GetObject ("spin_inertia_mass"); //mass of each of the extra load (weights)
		spin_inertia_length = (Gtk.SpinButton) builder.GetObject ("spin_inertia_length");

		hbox_gearedUp = (Gtk.HBox) builder.GetObject ("hbox_gearedUp");

		vbox_select_encoder = (Gtk.Box) builder.GetObject ("vbox_select_encoder");
		frame_notebook_side = (Gtk.Frame) builder.GetObject ("frame_notebook_side");
		notebook_side = (Gtk.Notebook) builder.GetObject ("notebook_side");
		treeview_select = (Gtk.TreeView) builder.GetObject ("treeview_select");
		button_import = (Gtk.Button) builder.GetObject ("button_import");
		button_export = (Gtk.Button) builder.GetObject ("button_export");
		button_edit = (Gtk.Button) builder.GetObject ("button_edit");
		button_add = (Gtk.Button) builder.GetObject ("button_add");
		button_duplicate = (Gtk.Button) builder.GetObject ("button_duplicate");
		button_delete = (Gtk.Button) builder.GetObject ("button_delete");
		image_import = (Gtk.Image) builder.GetObject ("image_import");
		image_export = (Gtk.Image) builder.GetObject ("image_export");
		image_capture_cancel = (Gtk.Image) builder.GetObject ("image_capture_cancel");
		image_cancel = (Gtk.Image) builder.GetObject ("image_cancel");
		image_accept = (Gtk.Image) builder.GetObject ("image_accept");
		image_edit = (Gtk.Image) builder.GetObject ("image_edit");
		image_add = (Gtk.Image) builder.GetObject ("image_add");
		image_duplicate = (Gtk.Image) builder.GetObject ("image_duplicate");
		image_delete = (Gtk.Image) builder.GetObject ("image_delete");
		label_side_action = (Gtk.Label) builder.GetObject ("label_side_action");
		label_side_selected = (Gtk.Label) builder.GetObject ("label_side_selected");

		notebook_actions = (Gtk.Notebook) builder.GetObject ("notebook_actions");
		entry_save_name = (Gtk.Entry) builder.GetObject ("entry_save_name");
		entry_save_description = (Gtk.Entry) builder.GetObject ("entry_save_description");
		//button_cancel = (Gtk.Button) builder.GetObject ("button_cancel");
		button_accept = (Gtk.Button) builder.GetObject ("button_accept");

		spin_im_weight_calcule = (Gtk.SpinButton) builder.GetObject ("spin_im_weight_calcule");
		spin_im_length_calcule = (Gtk.SpinButton) builder.GetObject ("spin_im_length_calcule");
		//spin_im_duration_calcule = (Gtk.SpinButton) builder.GetObject ("spin_im_duration_calcule");
		label_im_result_disc = (Gtk.Label) builder.GetObject ("label_im_result_disc");
		label_im_feedback = (Gtk.Label) builder.GetObject ("label_im_feedback");
		button_encoder_capture_inertial_do = (Gtk.Button) builder.GetObject ("button_encoder_capture_inertial_do");
		button_encoder_capture_inertial_cancel = (Gtk.Button) builder.GetObject ("button_encoder_capture_inertial_cancel");
		//button_encoder_capture_inertial_finish = (Gtk.Button) builder.GetObject ("button_encoder_capture_inertial_finish");
		label_capture_time = (Gtk.Label) builder.GetObject ("label_capture_time");
		label_im_calc_angle = (Gtk.Label) builder.GetObject ("label_im_calc_angle");
		label_im_calc_oscillations = (Gtk.Label) builder.GetObject ("label_im_calc_oscillations");
		grid_angle_oscillations = (Gtk.Grid) builder.GetObject ("grid_angle_oscillations");
		box_im_machine_result = (Gtk.Box) builder.GetObject ("box_im_machine_result");

		box_combo_d_num = (Gtk.Box) builder.GetObject ("box_combo_d_num");
		box_combo_gearedUp = (Gtk.Box) builder.GetObject ("box_combo_gearedUp");

		button_close = (Gtk.Button) builder.GetObject ("button_close");
	}
}
