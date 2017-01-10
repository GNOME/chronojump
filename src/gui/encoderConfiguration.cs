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

	[Widget] Gtk.CheckButton check_rotary_friction_inertia_on_axis;
	
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
	[Widget] Gtk.Notebook notebook_side;
	[Widget] Gtk.TreeView treeview_select;
	[Widget] Gtk.Image image_delete;

	[Widget] Gtk.Entry entry_save_name;
	[Widget] Gtk.Entry entry_save_description;
	[Widget] Gtk.Button button_apply;

	[Widget] Gtk.SpinButton spin_im_weight_calcule;
	[Widget] Gtk.SpinButton spin_im_length_calcule;
	//[Widget] Gtk.SpinButton spin_im_duration_calcule;
	[Widget] Gtk.Label label_im_result_disc;
	[Widget] Gtk.Label label_im_feedback;
	[Widget] Gtk.Button button_encoder_capture_inertial_do;
	[Widget] Gtk.Button button_encoder_capture_inertial_cancel;
	//[Widget] Gtk.Button button_encoder_capture_inertial_finish;

	[Widget] Gtk.Button button_close;

	static EncoderConfigurationWindow EncoderConfigurationWindowBox;
	
	ArrayList list;
	int listCurrent = 0; //current item on list
	Pixbuf pixbuf;

	Constants.EncoderGI encoderGI;

	/*
	 * this params are used on inertial
	 * and must be retrieved when this window is closed
	 */
	string main_gui_anchorage_str;
	int main_gui_extraWeightN;


	EncoderConfigurationWindow () {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "encoder_configuration.glade", "encoder_configuration", "chronojump");
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

	static public EncoderConfigurationWindow View (
			Constants.EncoderGI encoderGI, EncoderConfigurationSQLObject econfSO,
			string anchorage_str, int extraWeightN)
	{
		if (EncoderConfigurationWindowBox == null) {
			EncoderConfigurationWindowBox = new EncoderConfigurationWindow ();
		}

		EncoderConfigurationWindowBox.encoderGI = encoderGI;
		EncoderConfigurationWindowBox.updateGUIFromEncoderConfiguration(econfSO.encoderConfiguration);
		EncoderConfigurationWindowBox.main_gui_anchorage_str = anchorage_str;
		EncoderConfigurationWindowBox.main_gui_extraWeightN = extraWeightN;

		EncoderConfigurationWindowBox.createTreeView();
		EncoderConfigurationWindowBox.fillTreeView(
				SqliteEncoderConfiguration.Select(false, encoderGI, ""), //all
				econfSO);

		//A) side is hidden at start to ensure scr_treeview_select is scrolled and displays correctly the last row
		EncoderConfigurationWindowBox.notebook_side.Visible = false;

		EncoderConfigurationWindowBox.encoder_configuration.Show ();

		//B) side is shown now, after showing the window in order to be displayed correctly (see A)
		EncoderConfigurationWindowBox.notebook_side.Visible = (EncoderConfigurationWindowBox.sideMode != sideModes.HIDDEN);

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
		initializeList(Constants.EncoderType.ROTARYFRICTION, true, check_rotary_friction_inertia_on_axis.Active, 0);
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
			if(ec.has_inertia)
			{
				ec.list_d = get_list_d();

				bool found = false;
				if(Util.IsNumber(main_gui_anchorage_str, true))
				{
					LogB.Information("main_gui_anchorage = " + main_gui_anchorage_str);
					double guiAnchorage = Convert.ToDouble(main_gui_anchorage_str);
					foreach(double d in ec.list_d) {
						LogB.Information("d = " + d.ToString());
						if(d == guiAnchorage) {
							ec.d = guiAnchorage;
							found = true;
							break;
						}
					}
				}

				if(! found)
					ec.d = ec.list_d[0];
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
			ec.extraWeightN = main_gui_extraWeightN;
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

		notebook_side.Visible = (sideMode != sideModes.HIDDEN);

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

		Pixbuf pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_delete.png");
		image_delete.Pixbuf = pixbuf;
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

	private string getSelectedName()
	{
		TreeModel model;
		TreeIter iter;

		if (treeview_select.Selection.GetSelected(out model, out iter))
			return (string) model.GetValue (iter, colName);

		return "";
	}

	private void onTVSelectionChanged (object o, EventArgs args)
	{
		string selectedName = getSelectedName();
		if(selectedName == "")
			return;

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

	void on_entry_save_name_changed	(object o, EventArgs args)
	{
		button_apply.Sensitive = (entry_save_name.Text.ToString().Length > 0);

		//TODO: button delete sensitivity depends on being on the treeview
	}

	void on_entry_save_description_changed (object o, EventArgs args)
	{
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
					if(econfSO.name != null && econfSO.name != "")
					{
						//add more suffixes until name is unique
						econfSO.name = SqliteEncoderConfiguration.IfNameExistsAddSuffix(econfSO.name, Catalog.GetString("copy"));

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

	//TODO: button_apply sensitive only when name != "" && != of the others
	void on_button_apply_clicked (object o, EventArgs args)
	{
		apply(true);
	}

	private void apply(bool updateGUI)
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
			newName = SqliteEncoderConfiguration.IfNameExistsAddSuffix(newName, Catalog.GetString("copy"));
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
			TreeModel model;
			TreeIter iter = new TreeIter();
			treeview_select.Selection.GetSelected (out model, out iter);
			store.SetValue (iter, colName, newName);
			store.SetValue (iter, colDescription, entry_save_description.Text);
		}
	}

	void on_button_duplicate_clicked (object o, EventArgs args)
	{
		string selectedName = getSelectedName();
		if(selectedName == "")
			return;

		List<EncoderConfigurationSQLObject> list = SqliteEncoderConfiguration.Select(false, encoderGI, selectedName);
		if(list != null && list.Count == 1)
		{
			EncoderConfigurationSQLObject econfSO = list[0];
			econfSO.uniqueID = -1; //to be entered as null and not repeat the uniqueID

			//add a suffix
			econfSO.name += "_"  + Catalog.GetString("copy");
			//add more suffixes until name is unique
			econfSO.name = SqliteEncoderConfiguration.IfNameExistsAddSuffix(econfSO.name, Catalog.GetString("copy"));

			SqliteEncoderConfiguration.MarkAllAsUnactive(false, encoderGI);
			econfSO.active = true;
			SqliteEncoderConfiguration.Insert(false, econfSO);

			store.AppendValues (new string[]{ econfSO.name, econfSO.description });
			UtilGtk.TreeviewSelectRowWithName(treeview_select, store, colName, econfSO.name, true);
		}
	}

	void on_button_delete_clicked (object o, EventArgs args)
	{
		string selectedName = getSelectedName();
		if(selectedName == "")
			return;

		if(UtilGtk.CountRows(store) <= 1) {
			new DialogMessage(Constants.MessageTypes.WARNING,
					Catalog.GetString("Sorry, cannot delete all rows.") + "\n");
			return;
		}

		//treeview
		UtilGtk.RemoveRow(treeview_select, store); 			//1 delete row
		UtilGtk.TreeviewSelectFirstRow(treeview_select, store, true); 	//2 selects another row (use first)

		//SQL
		Sqlite.DeleteFromName(false, Constants.EncoderConfigurationTable, "name", selectedName);
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
		vbox_select_encoder.Visible = false;
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
		
}
