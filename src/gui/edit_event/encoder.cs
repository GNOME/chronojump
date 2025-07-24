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
 * Copyright (C) 2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
using Mono.Unix;

public class EditEncoderWindow : EditEventWindow
{
	static EditEncoderWindow EditEncoderWindowBox;
	private Constants.Modes mode;
	private EncoderSQL eSQL;
	private List<EncoderExercise> encoderExercise_l;
	private EncoderConfigurationWindow encoder_configuration_win;
	protected Gtk.ComboBoxText combo_encoder_anchorage;

	//for inheritance
	protected EditEncoderWindow () {
	}

	public EditEncoderWindow (Gtk.Window parent)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "edit_event.glade", "edit_event", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "edit_event.glade", null);
		connectWidgetsEditEvent (builder);
		builder.Autoconnect (this);

		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(edit_event);
	}

	static public EditEncoderWindow Show (Gtk.Window parent, Event myEvent, Constants.Modes mode)
	{
		if (EditEncoderWindowBox == null) {
			EditEncoderWindowBox = new EditEncoderWindow (parent);
		}

		EditEncoderWindowBox.mode = mode;
		EditEncoderWindowBox.colorize();
		EditEncoderWindowBox.initializeValues();
		EditEncoderWindowBox.fillDialog (myEvent);
		EditEncoderWindowBox.edit_event.Show ();

		return EditEncoderWindowBox;
	}
	
	protected override void initializeSpecific ()
	{
		typeOfTest = Constants.TestTypes.ENCODER;
		showType = true;
		showDescription = true;

		combo_person_has_signal = true;
		combo_exercise_has_signal = true;

		// encoder
		label_encoder_exercise.Visible = true;
		button_encoder_select.Visible = true;
		box_encoder_selected.Visible = true;

		// eccon
		label_encoder_eccon_title.Visible = true;
		box_encoder_eccon.Visible = true;

		// laterality
		label_laterality.Visible = true;
		box_laterality.Visible = true;

		// mass-inertia
		label_encoder_exercise_mass.Visible = (mode == Constants.Modes.POWERGRAVITATORY);
		hbox_encoder_exercise_mass.Visible = (mode == Constants.Modes.POWERGRAVITATORY);
		label_encoder_exercise_inertia.Visible = (mode == Constants.Modes.POWERINERTIAL);
		box_encoder_exercise_inertia.Visible = (mode == Constants.Modes.POWERINERTIAL);

		// repetition
		label_encoder_rep_length.Visible = true;
		vbox_encoder_rep_length.Visible = true;
		label_encoder_rep_length_units.Visible = true;
	}

	protected override void fillDialogSpecific (Event myEvent)
	{
		eSQL = (EncoderSQL) myEvent;

		image_encoder_configuration.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_build_24.png");
		image_encoder_eccon_concentric.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "muscle-concentric.png");
		image_encoder_eccon_eccentric_concentric.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "muscle-excentric-concentric.png");
		createLateralityIcons ();

		image_extra_mass.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "extra-mass.png");
		image_encoder_inertial_weights.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "extra-mass.png");

		fillDialogSpecificEncoder ();
		fillDialogSpecificEccon ();
		fillDialogSpecificLaterality ();
		fillDialogSpecificMassInertia ();
		fillDialogSpecificReps ();
	}

	// called at start and when encoder_configuration_win is closed
	private void fillDialogSpecificEncoder ()
	{
		image_encoder_selected_type.Pixbuf = eSQL.encoderConfiguration.GetPixbuf;
		label_encoder_selected.Text = string.Format ("{0} ({1})", eSQL.encoderConfiguration.name, eSQL.encoderConfiguration.code);
	}

	private void fillDialogSpecificEccon ()
	{
		switch (eSQL.eccon)
		{
			case "c":
				radio_encoder_eccon_concentric.Active = true;
				break;
			default:
				radio_encoder_eccon_eccentric_concentric.Active = true;
				break;
		}
	}

	private void fillDialogSpecificLaterality ()
	{
		switch (eSQL.Laterality)
		{
			case "RL":
				radio_laterality_both.Active = true;
				break;
			case "L":
				radio_laterality_left.Active = true;
				break;
			case "R":
				radio_laterality_right.Active = true;
				break;
		}
	}

	private void fillDialogSpecificMassInertia ()
	{
		if (mode == Constants.Modes.POWERGRAVITATORY)
		{
			spin_encoder_extra_weight.Value = eSQL.extraWeightD;
			setDisplacedWeight (eSQL.extraWeightD);
		}
		else //if (mode == Constants.Modes.POWERINERTIAL)
		{
			createComboEncoderAnchorage ();

			// here we use the eSQL params.
			// Note this methods will read widgets when there are changes on related widgets
			spin_encoder_im_weights_n.Value = eSQL.encoderConfiguration.extraWeightN;

			label_encoder_im_total.Text = eSQL.encoderConfiguration.inertiaTotal.ToString();
			label_encoder_equivalent_mass.Text = Util.TrimDecimals (calculateEquivalentMass (eSQL.encoderConfiguration), 1);
		}
	}

	private int calculateInertiaTotalFromGui ()
	{
		EncoderConfiguration econf = eSQL.encoderConfiguration;
		econf.extraWeightN = Convert.ToInt32 (spin_encoder_im_weights_n.Value);

		return UtilEncoder.CalculeInertiaTotal (econf);
	}

	private double calculateEquivalentMass (EncoderConfiguration econf)
	{
		return UtilEncoder.CalculateEquivalentMass (econf);
	}
	private double calculateEquivalentMassFromGui ()
	{
		string anchorageStr = UtilGtk.ComboGetActive (combo_encoder_anchorage);
		if (! Util.IsNumber (anchorageStr, true))
			return 0;

		EncoderConfiguration econf = eSQL.encoderConfiguration;
		econf.d = Convert.ToDouble (anchorageStr);
		econf.inertiaTotal = calculateInertiaTotalFromGui ();

		return UtilEncoder.CalculateEquivalentMass (econf);
	}

	private void setDisplacedWeight (double extraWeight)
	{
		label_encoder_displaced_weight.Text = Util.TrimDecimals (calculeDisplacedWeight (extraWeight), pDN);
	}

	private int getExerciseDisplacedWeight ()
	{
		if (encoderExercise_l == null || encoderExercise_l.Count == 0)
			return -1;

		string exerciseName = UtilGtk.ComboGetActive (combo_eventType);
		foreach (EncoderExercise ex in encoderExercise_l)
			if (ex.Name == exerciseName)
				return ex.PercentBodyWeight;

		return -1;
	}

	private double calculeDisplacedWeight (double extraWeight)
	{
		int personID = getPersonIDFromCombo ();
		if (personID < 0)
			return 0;

		double personWeight = getPersonWeight (personID);
		if (personWeight == 0)
			return 0;

		int exerciseDisplacedWeight = getExerciseDisplacedWeight ();
		if (exerciseDisplacedWeight < 0)
			return 0;

		//from gui/app1/encoder.cs finMass (DISPLACED)
		return extraWeight + UtilAll.DivideSafe (personWeight * exerciseDisplacedWeight, 100.0);
	}

	private void fillDialogSpecificReps ()
	{
		if (mode == Constants.Modes.POWERGRAVITATORY)
		{
			label_encoder_rep_length.Text = Catalog.GetString ("Repetition\nminimal ROM");
			label_encoder_rep_length.TooltipText = Catalog.GetString ("Minimal Range of Movement");
			spin_encoder_rep_min_height_gravitatory.Visible = true;
			spin_encoder_rep_min_height_inertial.Visible = false;

			spin_encoder_rep_min_height_gravitatory.Value = eSQL.minHeight;
		} else //if (mode == Constants.Modes.POWERINERTIAL)
		{
			label_encoder_rep_length.Text = Catalog.GetString ("Repetition\nminimal length");
			label_encoder_rep_length.TooltipText = "";
			spin_encoder_rep_min_height_gravitatory.Visible = false;
			spin_encoder_rep_min_height_inertial.Visible = true;

			spin_encoder_rep_min_height_inertial.Value = eSQL.minHeight;
		}
	}

	protected override void on_button_encoder_select_clicked (object o, EventArgs args)
	{
		encoder_configuration_win = EncoderConfigurationWindow.View (
				Constants.GetEncoderGIByMode (mode),
				SqliteEncoderConfiguration.SelectActive (Constants.GetEncoderGIByMode (mode)),
				eSQL.encoderConfiguration.d.ToString (),
				eSQL.encoderConfiguration.extraWeightN, //used on inertial
				false); 	// allow to calcule IM on inertial

		encoder_configuration_win.Button_close.Clicked -= new EventHandler (on_encoder_configuration_win_closed);
		encoder_configuration_win.Button_close.Clicked += new EventHandler (on_encoder_configuration_win_closed);
	}
	protected override void on_encoder_configuration_win_closed (object o, EventArgs args)
	{
		eSQL.encoderConfiguration = encoder_configuration_win.GetAcceptedValues();
		fillDialogSpecificEncoder ();
	}

	protected override string [] findTypes (Event myEvent)
	{
		encoderExercise_l = SqliteEncoderExercise.SelectEncoderExercises (
				false, -1, false, Constants.GetEncoderGIByMode (mode));

		// get the exercise names and convert to string []
		return EncoderExercise.ListToString (encoderExercise_l);
	}

	protected override void on_combo_persons_changed (object o, EventArgs args)
	{
		setDisplacedWeight (spin_encoder_extra_weight.Value);
	}
	protected override void on_combo_eventType_changed (object o, EventArgs args)
	{
		setDisplacedWeight (spin_encoder_extra_weight.Value);
	}
	protected override void on_spin_encoder_extra_weight_value_changed (object o, EventArgs args)
	{
		setDisplacedWeight (spin_encoder_extra_weight.Value);
	}
	protected override void on_spin_encoder_im_weights_n_value_changed (object o, EventArgs args)
	{
		label_encoder_im_total.Text = calculateInertiaTotalFromGui ().ToString ();
		label_encoder_equivalent_mass.Text = Util.TrimDecimals (calculateEquivalentMassFromGui (), 1);
	}

	private void createComboEncoderAnchorage ()
	{
		combo_encoder_anchorage = new ComboBoxText();

		if (! eSQL.encoderConfiguration.list_d.IsEmpty())
		{
			UtilGtk.ComboUpdate (combo_encoder_anchorage, eSQL.encoderConfiguration.list_d.L);
			combo_encoder_anchorage.Active = UtilGtk.ComboMakeActive (
					combo_encoder_anchorage,
					eSQL.encoderConfiguration.d.ToString ()
					);
		}

		hbox_combo_encoder_anchorage.PackStart (combo_encoder_anchorage, false, true, 0);
		hbox_combo_encoder_anchorage.ShowAll ();

		combo_encoder_anchorage.Changed -= new EventHandler(on_combo_encoder_anchorage_changed );
		combo_encoder_anchorage.Changed += new EventHandler(on_combo_encoder_anchorage_changed );
	}

	private void on_combo_encoder_anchorage_changed (object o, EventArgs args)
	{
		label_encoder_equivalent_mass.Text = Util.TrimDecimals (calculateEquivalentMassFromGui (), 1);
	}

	protected override void updateSQL (int eventID, int personID, string description)
	{
		SqliteTests st = new SqliteEncoder ();
		st.UpdateFromEdit (eventID, personID, -1, description);
	}

	protected override void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditEncoderWindowBox.edit_event.Hide();
		EditEncoderWindowBox = null;
	}
	
	protected override void on_delete_event (object o, DeleteEventArgs args)
	{
		EditEncoderWindowBox.edit_event.Hide();
		EditEncoderWindowBox = null;
	}
	
	protected override void hideWindow() {
		EditEncoderWindowBox.edit_event.Hide();
		EditEncoderWindowBox = null;
	}
}

public partial class ChronoJumpWindow
{
	private void on_edit_selected_encoder_clicked (object o, EventArgs args)
	{
		//notebooks_change(2); see "notebooks_change sqlite problem"
		LogB.Information("Edit selected encoder");
		//1.- check that there's a line selected
		//2.- check that this line is a encoder and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		int selectedID = treeViewResultsSession.EventSelectedID;
		if (selectedID < 0)
			return;

		//3.- obtain the data of the selected encoder
		EncoderSQL encoder = SqliteEncoder.SelectData (selectedID, false );
		eventOldPerson = encoder.PersonID;

		//4.- edit this test
		editEncoderWin = EditEncoderWindow.Show (app1, encoder, current_mode);
		editEncoderWin.Fake_button_finished.Clicked += new EventHandler (on_edit_selected_encoder_finished);
	}

	private void on_edit_selected_encoder_finished (object o, EventArgs args)
	{
		LogB.Information("edit selected encoder finished");
		EncoderSQL encoder = SqliteEncoder.SelectData (treeViewResultsSession.EventSelectedID, false);

		//if person changed, fill treeview again, if not, only update it's line
		if (eventOldPerson == encoder.PersonID)
		{
			//encoder.ExerciseName = SqliteTests.SelectExerciseNameInOtherTable (false, encoder.ExerciseID, Constants.EncoderExerciseTable);
			treeViewResultsSession.Update (encoder);
		}  else
			pre_fillTreeView_resultsSession ();

		updateGraphEncoderSessionBars ();
	}

}
