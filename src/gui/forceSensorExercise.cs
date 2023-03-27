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
 * Copyright (C) 2019-2023   Xavier de Blas <xaviblas@gmail.com>
 * Copyright (C) 2019  	Xavier Padullés <x.padulles@gmail.com>
 */

using System;
using Gdk; //Pixbuf
using Gtk;
//using Glade;
using GLib; //for Value
using System.Collections.Generic; //List<T>
using Mono.Unix;


public class ForceSensorExerciseWindow
{
	//general widgets
	Gtk.Window force_sensor_exercise;
	Gtk.Label label_header;
	Gtk.Box hbox_error;
	Gtk.Label label_error;
	Gtk.Entry entry_name;
	Gtk.Notebook notebook_main;
	Gtk.Notebook notebook_desc_examples;
	Gtk.RadioButton radio_desc_examples_desc;
	Gtk.RadioButton radio_desc_examples_examples;
	Gtk.Label label_help;
	Gtk.Label label_radio_desc_examples_desc;
	Gtk.Label label_radio_desc_examples_examples;
	Gtk.TextView textview_description;
	Gtk.TextView textview_examples;
	Gtk.Button button_back;
	Gtk.Button button_next;
	Gtk.Button button_accept;
	Gtk.Image image_back;
	Gtk.Image image_next;
	Gtk.Image image_cancel;

	//type tab
	Gtk.RadioButton radio_type_both;
	Gtk.RadioButton radio_type_isometric;
	Gtk.RadioButton radio_type_elastic;

	//force tab
	Gtk.Label label_force;
	Gtk.TextView textview_force_explanation;
	Gtk.RadioButton radio_force_sensor_raw;
	Gtk.RadioButton radio_force_resultant;
	Gtk.Button button_force_exerted_help;
	Gtk.Image image_force_exerted_help;

	//fixation tab
	Gtk.Label label_fixation;
	Gtk.TextView textview_fixation_explanation;
	// disabled, using the "type" variable
	//Gtk.RadioButton radio_fixation_elastic;
	//Gtk.RadioButton radio_fixation_not_elastic;

	//mass tab
	Gtk.Label label_mass;
	Gtk.TextView textview_mass_explanation;
	Gtk.RadioButton radio_mass_add;
	Gtk.RadioButton radio_mass_subtract;
	Gtk.RadioButton radio_mass_nothing;
	Gtk.HBox hbox_body_mass_add;
	Gtk.SpinButton spin_body_mass_add;

	//repetitions detect tab
	Gtk.Label label_detect_repetitions;
	Gtk.RadioButton radio_detect_repetitions_from_prefs;
	Gtk.RadioButton radio_detect_repetitions_custom;
	Gtk.HBox hbox_detect_repetitions_preferences;
	Gtk.HBox hbox_detect_repetitions_elastic;
	Gtk.HBox hbox_detect_repetitions_not_elastic;
	Gtk.Label label_repetitions_prefs_ecc_value;
	Gtk.Label label_repetitions_prefs_con_value;
	Gtk.Label label_repetitions_prefs_ecc_units;
	Gtk.Label label_repetitions_prefs_con_units;
	Gtk.SpinButton spin_force_sensor_elastic_ecc_min_displ;
	Gtk.SpinButton spin_force_sensor_elastic_con_min_displ;
	Gtk.SpinButton spin_force_sensor_not_elastic_ecc_min_force;
	Gtk.SpinButton spin_force_sensor_not_elastic_con_min_force;

	//repetitions show tab
	//Gtk.CheckButton check_show_ecc;
	Gtk.RadioButton radio_reps_show_concentric;
	Gtk.RadioButton radio_reps_show_both;
	Gtk.Alignment alignment_reps_show_both;
	Gtk.RadioButton radio_reps_show_both_together;
	Gtk.RadioButton radio_reps_show_both_separated;

	//other tab
	Gtk.Label label_other;
	Gtk.TextView textview_other_explanation;
	Gtk.SpinButton spin_angle;
	Gtk.Entry entry_description;

	//fake button
	Gtk.Button fakeButtonReadValues;

	public bool Success;
	private bool adding;
	private Constants.Modes chronojumpMode;
	private ForceSensorExercise.Types type;
	private ForceSensorExercise exercise;

	//values on preferences, useful to show them unsensitive if the radio_detect_repetitions_from_prefs.Active
	//on add and on edit exercise
	private double prefsForceSensorElasticEccMinDispl;
	private double prefsForceSensorElasticConMinDispl;
	private int prefsForceSensorNotElasticEccMinForce;
	private int prefsForceSensorNotElasticConMinForce;

	private enum modesEnum { EDIT, ADD }
	private modesEnum modeEnum;
	private enum Pages { TYPE, FORCE, FIXATION, MASS, REPSDETECT, REPSSHOW, OTHER }
	private enum Options { FORCE_SENSOR, FORCE_RESULTANT, FIXATION_ELASTIC, FIXATION_NOT_ELASTIC,
		MASS_ADD, MASS_SUBTRACT, MASS_NOTHING,
		REPETITIONS_PREFS, REPETITIONS_NO_PREFS, REPETITIONS_SHOW, OTHER }

	static ForceSensorExerciseWindow ForceSensorExerciseWindowBox;

	/*
	   public int uniqueID; 			//used on encoder & forceSensor edit exercise
	   public string nameUntranslated;		//used on encoder edit exercise
	   */

	public ForceSensorExerciseWindow (string title, string textHeader)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "force_sensor_exercise.glade", "force_sensor_exercise", "chronojump");
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "force_sensor_exercise.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);

		//put an icon to window
		UtilGtk.IconWindow(force_sensor_exercise);

		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.WindowColor(force_sensor_exercise, Config.ColorBackground);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_header);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_help);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_error);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_radio_desc_examples_desc);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_radio_desc_examples_examples);

			UtilGtk.WidgetColor (notebook_main, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_main);
		}

		force_sensor_exercise.Resizable = false;
		setTitle(title);
		label_header.Text = textHeader;
		fakeButtonReadValues = new Gtk.Button();

		initializeGuiAtCreation();

		//HideOnAccept = true;
		//DestroyOnAccept = false;
	}

	static public ForceSensorExerciseWindow ShowEdit (Constants.Modes chronojumpMode,
			string title, string textHeader, ForceSensorExercise exercise,
			double prefsForceSensorElasticEccMinDispl, double prefsForceSensorElasticConMinDispl,
			int prefsForceSensorNotElasticEccMinForce, int prefsForceSensorNotElasticConMinForce)
	{
		if (ForceSensorExerciseWindowBox == null) {
			ForceSensorExerciseWindowBox = new ForceSensorExerciseWindow(title, textHeader);
		} else {
			ForceSensorExerciseWindowBox.setTitle(title);
			ForceSensorExerciseWindowBox.label_header.Text = textHeader;
		}

		ForceSensorExerciseWindowBox.adding = false;
		ForceSensorExerciseWindowBox.chronojumpMode = chronojumpMode;
		ForceSensorExerciseWindowBox.type = exercise.Type; //do not change the type here, if it's both will continue being both
		ForceSensorExerciseWindowBox.Success = false;
		ForceSensorExerciseWindowBox.modeEnum = modesEnum.EDIT;
		ForceSensorExerciseWindowBox.exercise = exercise;
		ForceSensorExerciseWindowBox.exerciseToGUI();

		ForceSensorExerciseWindowBox.prefsForceSensorElasticEccMinDispl = prefsForceSensorElasticEccMinDispl;
		ForceSensorExerciseWindowBox.prefsForceSensorElasticConMinDispl = prefsForceSensorElasticConMinDispl;
		ForceSensorExerciseWindowBox.prefsForceSensorNotElasticEccMinForce = prefsForceSensorNotElasticEccMinForce;
		ForceSensorExerciseWindowBox.prefsForceSensorNotElasticConMinForce = prefsForceSensorNotElasticConMinForce;

		//on edit, if elastic, pass elastic params from exercise, and not elastic from preferences. Opposite on not elastic
		bool repsFromPrefs = false;
		if(exercise.ComputeAsElastic)
		{
			//to avoid put -1 (in fact is 1, minimum value) on spinbuttons
			double em = exercise.EccMin;
			if(em < 0) {
				em = prefsForceSensorElasticEccMinDispl;
				repsFromPrefs = true;
			}

			double cm = exercise.ConMin;
			if(cm < 0) {
				cm = prefsForceSensorElasticConMinDispl;
				repsFromPrefs = true;
			}

			ForceSensorExerciseWindowBox.repetitionsToGUI(
					exercise.RepetitionsShow, repsFromPrefs,
					em, cm,
					prefsForceSensorNotElasticEccMinForce, prefsForceSensorNotElasticConMinForce);
		} else {
			//to avoid put -1 (in fact is 1, minimum value) on spinbuttons
			double em = exercise.EccMin;
			if(em < 0) {
				em = prefsForceSensorNotElasticEccMinForce;
				repsFromPrefs = true;
			}

			double cm = exercise.ConMin;
			if(cm < 0) {
				cm = prefsForceSensorNotElasticConMinForce;
				repsFromPrefs = true;
			}

			ForceSensorExerciseWindowBox.repetitionsToGUI(
					exercise.RepetitionsShow, repsFromPrefs,
					prefsForceSensorElasticEccMinDispl, prefsForceSensorElasticConMinDispl,
					Convert.ToInt32(em), Convert.ToInt32(cm));
		}

		ForceSensorExerciseWindowBox.initializeGuiAtShow ();
		ForceSensorExerciseWindowBox.force_sensor_exercise.Show ();

		return ForceSensorExerciseWindowBox;
	}

	static public ForceSensorExerciseWindow ShowAdd (Constants.Modes chronojumpMode,
			string title, string textHeader,
			double prefsForceSensorElasticEccMinDispl, double prefsForceSensorElasticConMinDispl,
			int prefsForceSensorNotElasticEccMinForce, int prefsForceSensorNotElasticConMinForce)
	{
		if (ForceSensorExerciseWindowBox == null) {
			ForceSensorExerciseWindowBox = new ForceSensorExerciseWindow(title, textHeader);
		} else {
			ForceSensorExerciseWindowBox.setTitle(title);
			ForceSensorExerciseWindowBox.label_header.Text = textHeader;
		}

		ForceSensorExerciseWindowBox.adding = true;
		ForceSensorExerciseWindowBox.chronojumpMode = chronojumpMode;
		if(chronojumpMode == Constants.Modes.FORCESENSORISOMETRIC)
			ForceSensorExerciseWindowBox.type = ForceSensorExercise.Types.ISOMETRIC;
		else //if(chronojumpMode == Constants.Modes.FORCESENSORELASTIC)
			ForceSensorExerciseWindowBox.type = ForceSensorExercise.Types.ELASTIC;

		ForceSensorExerciseWindowBox.Success = false;
		ForceSensorExerciseWindowBox.modeEnum = modesEnum.ADD;
		ForceSensorExerciseWindowBox.exercise = null;

		ForceSensorExerciseWindowBox.prefsForceSensorElasticEccMinDispl = prefsForceSensorElasticEccMinDispl;
		ForceSensorExerciseWindowBox.prefsForceSensorElasticConMinDispl = prefsForceSensorElasticConMinDispl;
		ForceSensorExerciseWindowBox.prefsForceSensorNotElasticEccMinForce = prefsForceSensorNotElasticEccMinForce;
		ForceSensorExerciseWindowBox.prefsForceSensorNotElasticConMinForce = prefsForceSensorNotElasticConMinForce;

		ForceSensorExerciseWindowBox.repetitionsToGUI(
				ForceSensorExercise.RepetitionsShowTypes.BOTHTOGETHER, true, //repsFromPrefs
				prefsForceSensorElasticEccMinDispl, prefsForceSensorElasticConMinDispl,
				prefsForceSensorNotElasticEccMinForce, prefsForceSensorNotElasticConMinForce);

		ForceSensorExerciseWindowBox.initializeGuiAtShow ();
		ForceSensorExerciseWindowBox.force_sensor_exercise.Show ();

		return ForceSensorExerciseWindowBox;
	}

	private void setTitle(string title)
	{
		if(title != "")
			force_sensor_exercise.Title = "Chronojump - " + title;
	}

	private void initializeGuiAtCreation()
	{
		// 1. show title label at each notebook_main page on bold
		label_force.Text = "<b>" + label_force.Text + "</b>";
		label_fixation.Text = "<b>" + label_fixation.Text + "</b>";
		label_mass.Text = "<b>" + label_mass.Text + "</b>";
		label_detect_repetitions.Text = "<b>" + label_detect_repetitions.Text + "</b>";
		label_other.Text = "<b>" + label_other.Text + "</b>";

		label_force.UseMarkup = true;
		label_fixation.UseMarkup = true;
		label_mass.UseMarkup = true;
		label_detect_repetitions.UseMarkup = true;
		label_other.UseMarkup = true;

		// 2. textviews of explanations of each page
		textview_force_explanation.Buffer.Text = getTopExplanations(Pages.FORCE);
		textview_fixation_explanation.Buffer.Text = getTopExplanations(Pages.FIXATION);
		textview_mass_explanation.Buffer.Text = getTopExplanations(Pages.MASS);
		// done below textview_other_explanation.Buffer.Text = getTopExplanations(Pages.OTHER);

		// 3. icons
		image_cancel.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_cancel.png");
		image_next.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "arrow_forward.png");
		image_back.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "arrow_back.png");
		image_force_exerted_help.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_info.png");
	}

	private void initializeGuiAtShow ()
	{
		Pages p = Pages.FORCE;
		if (! adding && exercise.Type == ForceSensorExercise.Types.BOTH)
		{
			p = Pages.TYPE;
			radio_type_both.Active = true;
		}

		managePage (p);
		ForceSensorExerciseWindowBox.notebook_main.CurrentPage = Convert.ToInt32 (p);

		//on edit spin_body_mass_add.Value is set at exerciseToGUI(), but on adding, set at 100
		if(adding)
			spin_body_mass_add.Value = 100;
	}

	private void exerciseToGUI()
	{
		entry_name.Text = exercise.Name;

		if(exercise.ForceResultant)
		{
			radio_force_resultant.Active = true;
			button_force_exerted_help.Sensitive = true;
		} else {
			radio_force_sensor_raw.Active = true;
			button_force_exerted_help.Sensitive = false;
		}

		if(exercise.PercentBodyWeight > 0 && ! exercise.TareBeforeCaptureOnExerciseEdit)
			radio_mass_add.Active = true;

		else if(exercise.PercentBodyWeight == 0 && exercise.TareBeforeCaptureOnExerciseEdit)
			radio_mass_subtract.Active = true;
		else
			radio_mass_nothing.Active = true;

		spin_body_mass_add.Value = exercise.PercentBodyWeight;
		spin_angle.Value = exercise.AngleDefault;
		entry_description.Text = exercise.Description;
	}

	private void repetitionsToGUI(
			ForceSensorExercise.RepetitionsShowTypes repetitionsShow, bool repsFromPrefs,
			double forceSensorElasticEccMinDispl, double forceSensorElasticConMinDispl,
			int forceSensorNotElasticEccMinForce, int forceSensorNotElasticConMinForce)
	{
		//reps detect tab
		if(repsFromPrefs)
			radio_detect_repetitions_from_prefs.Active = true;
		else
			radio_detect_repetitions_custom.Active =true;

		spin_force_sensor_elastic_ecc_min_displ.Value = forceSensorElasticEccMinDispl;
		spin_force_sensor_elastic_con_min_displ.Value = forceSensorElasticConMinDispl;
		spin_force_sensor_not_elastic_ecc_min_force.Value = forceSensorNotElasticEccMinForce;
		spin_force_sensor_not_elastic_con_min_force.Value = forceSensorNotElasticConMinForce;


		//reps show tab
		if(repetitionsShow == ForceSensorExercise.RepetitionsShowTypes.CONCENTRIC)
		{
			radio_reps_show_concentric.Active = true;
			alignment_reps_show_both.Visible = false;
		} else
		{
			radio_reps_show_both.Active = true;
			alignment_reps_show_both.Visible = true;

			if(repetitionsShow == ForceSensorExercise.RepetitionsShowTypes.BOTHTOGETHER)
				radio_reps_show_both_together.Active = true;
			else
				radio_reps_show_both_separated.Active = true;
		}
	}

	private string getTopExplanations (Pages p)
	{
		string str;
		if(p == Pages.FORCE)
			str = Catalog.GetString("In some cases the force registered by the Force Sensor is not directly the force that the person is exerting.");
		else if(p == Pages.FIXATION)
			str = Catalog.GetString("How the force is transmitted to the sensor");
		else if(p == Pages.MASS)
			str = Catalog.GetString("Depending on the exercise and configuration of the test, the total mass (mass of the person and the extra load) can affect the sensor measuring. Select how to manage this effect.");
		else if(p == Pages.REPSDETECT)
			str = ""; //not shown
		else if(p == Pages.REPSSHOW)
			str = ""; //not shown
		else { //if(p == Pages.OTHER)
			if(radio_force_resultant.Active && radio_mass_add.Active)
				str = Catalog.GetString("In current exercise configuration, it is necessary to enter the angle in which the sensor is measuring.");
			else
				str = Catalog.GetString("In current exercise configuration, angle is merely descriptive (not used in calculations).");
		}

		return str;
	}

	private string getDescription (Options o)
	{
		string str;
		if(o == Options.FORCE_SENSOR)
			str = Catalog.GetString("When you are interested only in the force transmitted to the force sensor. This option do NOT take into account the effect of the weight or the acceleration of a mass.");
		else if(o == Options.FORCE_RESULTANT)
			str = Catalog.GetString("When you want the resultant of all the forces exerted by the person.") + " " +
				Catalog.GetString("Chronojump needs to know the involved mass (usually person's mass) and the angle.") + " " +
				Catalog.GetString("This option allows to take into account the effect of the weight or the acceleration of a mass.") + " " +
				Catalog.GetString("Projection of exerted force is calculated as the sum of forces projected in the direction defined by the exercise.");
		else if(o == Options.FIXATION_ELASTIC)
			str = Catalog.GetString("If, exerting a force, some element is significantly elongated it means that you are using elastic elements. Knowing the characteristics of the elastic elements allows to calculate positions, velocities and accelerations during the exercise");
		else if(o == Options.FIXATION_NOT_ELASTIC)
			str = Catalog.GetString("If exerting a force there's no significant elongation you are using not elastic elements.");
		else if(o == Options.MASS_ADD)
			str = Catalog.GetString("When the mass doesn't affect the sensor data but it must be added to it.");
		else if(o == Options.MASS_SUBTRACT)
			str = Catalog.GetString("In some cases the weight of the mass is supported by the sensor but it is not a force that the subject is exerting. In this case, the sensor will be tared before starting the test.");
		else if(o == Options.MASS_NOTHING)
			str = Catalog.GetString("In some cases the weight is transmitted to the sensor and it is also supported by the measured limb. If the effect of the mass is not significant, use this option also.") +
				" " + Catalog.GetString("This configuration is the same as raw data.");
		else if(o == Options.REPETITIONS_PREFS)
			str = Catalog.GetString("If user changes values on preferences, these values will automatically change.");
		else if(o == Options.REPETITIONS_NO_PREFS)
			str = Catalog.GetString("These values will be used to detect repetitions.");
		else if(o == Options.REPETITIONS_SHOW)
			str = Catalog.GetString("Detected repetitions will show only the phase in which the force increases or also where decreases.");
		else //if(o == Options.OTHER)
			str = Catalog.GetString("0 means horizontally") + "\n" +
				Catalog.GetString("90 means vertically with the person above the sensor") + "\n" +
				Catalog.GetString("-90 means vertically with the person below the sensor");

		return str;
	}

	private string getExample (Options o)
	{
		string str;
		if(o == Options.FORCE_SENSOR)
		{
			if (chronojumpMode == Constants.Modes.FORCESENSORISOMETRIC)
				str = "- " + Catalog.GetString("Isometric Leg Extension.");
			else //if (chronojumpMode == Constants.Modes.FORCESENSORELASTIC)
				str = "- " + Catalog.GetString("Upper limb movements against a rubber if the displaced mass is considered insignificant.");
		} else if(o == Options.FORCE_RESULTANT)
			str = "1.- " + Catalog.GetString("Vertical:") + " " + Catalog.GetString("Squat with the force sensor fixed between the floor and the body.") +
				"\n2.- " + Catalog.GetString("Horizontal:") + " " + Catalog.GetString("Movements where a significant mass is accelerated.") +
				"\n3.- " + Catalog.GetString("Diagonal movement:") + " " + Catalog.GetString("Lateral squat.");
		else if(o == Options.FIXATION_ELASTIC)
			str =  Catalog.GetString("Rubber bands, springs, flexible material …");
		else if(o == Options.FIXATION_NOT_ELASTIC)
			str = "- " + Catalog.GetString("In an isometric squat with the force sensor fixed between the floor and the body, increasing the mass don't affect the measure of the sensor because the weight is supported by the lower limbs, not the sensor.");
		else if(o == Options.MASS_ADD)
			str = "- " + Catalog.GetString("In an isometric squat with the force sensor fixed between the floor and the body, increasing the mass don't affect the measure of the sensor because the weight is supported by the lower limbs, not the sensor.");
		else if(o == Options.MASS_SUBTRACT)
			str = Catalog.GetString("Hamstring test where the heel of the person is suspended in a cinch attached to the sensor. The weight of the leg is affecting the measure of the force transmitted to the sensor but this is not a force exerted by the subject.");
		else if(o == Options.MASS_NOTHING)
			str = "1.- " + Catalog.GetString("Nordic hamstring. In a Nordic hamstring with the sensor attached to the ankle, the weight affects the values of the sensor but this weight is supported by the hamstrings we are measuring.") +
				"\n2.- " + Catalog.GetString("Pulling on a TRX. Pulling from a TRX implies overcome the body weight. This body weight is also measured by the sensor.");
		//else if(o == Options.REPSDETECT)
		//	str = ""; //not shown
		//else if(o == Options.REPSSHOW)
		//	str = ""; //not shown
		else //if(o == Options.OTHER)
			str = "";

		return str;
	}

	private void set_notebook_desc_example_labels(Options o)
	{
		string str;

		if(o == Options.FORCE_SENSOR)
			str = Catalog.GetString("Raw data");
		else if(o == Options.FORCE_RESULTANT)
			str = Catalog.GetString("Projection of exerted force");
		else if(o == Options.FIXATION_ELASTIC)
			str = Catalog.GetString("Elastic");
		else if(o == Options.FIXATION_NOT_ELASTIC)
			str = Catalog.GetString("Not Elastic");
		else if(o == Options.MASS_ADD)
			str = Catalog.GetString("Add mass");
		else if(o == Options.MASS_SUBTRACT)
			str = Catalog.GetString("Subtract mass");
		else if(o == Options.MASS_NOTHING)
			str = Catalog.GetString("Mass is included");
		else if(o == Options.REPETITIONS_PREFS)
			str = Catalog.GetString("Repetitions according to preferences");
		else if(o == Options.REPETITIONS_NO_PREFS)
			str = Catalog.GetString("Repetitions using custom values");
		else if(o == Options.REPETITIONS_SHOW)
			str = Catalog.GetString("Show repetitions");
		else //if(o == Options.OTHER)
			str = Catalog.GetString("Angle explanation");

		label_radio_desc_examples_desc.Text = str;
		label_radio_desc_examples_examples.Text = Catalog.GetString("Examples of:") + " " + str;
	}

	private void managePage(int i)
	{
		//convert to int to enum
		Pages p = (Pages)Enum.ToObject(typeof(Pages) , i);
		managePage(p);
	}
	private void managePage(Pages p)
	{
		string desc;
		string ex;

		//default for most of the pages
		button_next.Visible = true;
		button_accept.Visible = false;
		button_back.Sensitive = true;
		radio_desc_examples_examples.Show();
		notebook_desc_examples.GetNthPage(1).Show();

		if(p == Pages.TYPE)
		{
			desc = "";
			ex = "";
			button_back.Sensitive = false;
		}
		else if(p == Pages.FORCE)
		{
			if (adding || exercise.Type != ForceSensorExercise.Types.BOTH)
				button_back.Sensitive = false;

			if(radio_force_sensor_raw.Active) {
				desc = getDescription(Options.FORCE_SENSOR);
				ex = getExample(Options.FORCE_SENSOR);
				set_notebook_desc_example_labels(Options.FORCE_SENSOR);
				button_force_exerted_help.Sensitive = false;
			} else {
				desc = getDescription(Options.FORCE_RESULTANT);
				ex = getExample(Options.FORCE_RESULTANT);
				set_notebook_desc_example_labels(Options.FORCE_RESULTANT);
				button_force_exerted_help.Sensitive = true;
			}
		}
		else if(p == Pages.FIXATION)
		{
			desc = "";
			ex = "";
		}
		else if(p == Pages.MASS)
		{
			if(radio_mass_add.Active) {
				desc = getDescription(Options.MASS_ADD);
				ex = getExample(Options.MASS_ADD);
				set_notebook_desc_example_labels(Options.MASS_ADD);
			} else if(radio_mass_subtract.Active) {
				desc = getDescription(Options.MASS_SUBTRACT);
				ex = getExample(Options.MASS_SUBTRACT);
				set_notebook_desc_example_labels(Options.MASS_SUBTRACT);
			} else { // (radio_mass_nothing.Active)
				desc = getDescription(Options.MASS_NOTHING);
				ex = getExample(Options.MASS_NOTHING);
				set_notebook_desc_example_labels(Options.MASS_NOTHING);
			}
			hbox_body_mass_add.Sensitive = radio_mass_add.Active;
		}
		else if(p == Pages.REPSDETECT)
		{
			if(radio_force_sensor_raw.Active || chronojumpMode == Constants.Modes.FORCESENSORISOMETRIC)
			{
				label_repetitions_prefs_ecc_value.Text = prefsForceSensorNotElasticEccMinForce.ToString();
				label_repetitions_prefs_con_value.Text = prefsForceSensorNotElasticConMinForce.ToString();
				label_repetitions_prefs_ecc_units.Text = "N";
				label_repetitions_prefs_con_units.Text = "N";
			} else {
				label_repetitions_prefs_ecc_value.Text = Util.TrimDecimals(prefsForceSensorElasticEccMinDispl, 1);
				label_repetitions_prefs_con_value.Text = Util.TrimDecimals(prefsForceSensorElasticConMinDispl, 1);
				label_repetitions_prefs_ecc_units.Text = "m";
				label_repetitions_prefs_con_units.Text = "m";
			}


			//visibilities
			if(radio_force_sensor_raw.Active || chronojumpMode == Constants.Modes.FORCESENSORISOMETRIC)
			{
				hbox_detect_repetitions_not_elastic.Visible = true;
				hbox_detect_repetitions_elastic.Visible = false;
			} else {
				hbox_detect_repetitions_not_elastic.Visible = false;
				hbox_detect_repetitions_elastic.Visible = true;
			}

			//sensitiveness
			if(radio_detect_repetitions_from_prefs.Active)
			{
				hbox_detect_repetitions_preferences.Sensitive = true;
				hbox_detect_repetitions_elastic.Sensitive = false;
				hbox_detect_repetitions_not_elastic.Sensitive = false;
			} else {
				hbox_detect_repetitions_preferences.Sensitive = false;
				hbox_detect_repetitions_elastic.Sensitive = true;
				hbox_detect_repetitions_not_elastic.Sensitive = true;
			}

			Options o = Options.REPETITIONS_PREFS;
			if(! radio_detect_repetitions_from_prefs.Active)
				o = Options.REPETITIONS_NO_PREFS;

			desc = getDescription(o);
			ex = getExample(o);
			set_notebook_desc_example_labels(o);

			radio_desc_examples_desc.Active = true;
			radio_desc_examples_examples.Hide();
			//notebook_desc_examples.CurrentPage = 0;
			notebook_desc_examples.GetNthPage(1).Hide();
		}
		else if(p == Pages.REPSSHOW)
		{
			if(radio_reps_show_concentric.Active)
				alignment_reps_show_both.Visible = false;
			else if(radio_reps_show_both.Active)
				alignment_reps_show_both.Visible = true;

			desc = getDescription(Options.REPETITIONS_SHOW);
			ex = getExample(Options.REPETITIONS_SHOW);
			set_notebook_desc_example_labels(Options.REPETITIONS_SHOW);

			radio_desc_examples_desc.Active = true;
			radio_desc_examples_examples.Hide();
			notebook_desc_examples.GetNthPage(1).Hide();
		}
		else // if(p == Pages.OTHER)
		{
			button_next.Visible = false;
			button_accept.Visible = true;
			textview_other_explanation.Buffer.Text = getTopExplanations(Pages.OTHER);

			desc = getDescription(Options.OTHER);
			ex = getExample(Options.OTHER);
			set_notebook_desc_example_labels(Options.OTHER);

			radio_desc_examples_desc.Active = true;
			radio_desc_examples_examples.Hide();
			//notebook_desc_examples.CurrentPage = 0;
			notebook_desc_examples.GetNthPage(1).Hide();
		}

		textview_description.Buffer.Text = desc;
		textview_examples.Buffer.Text = ex;
	}

	private void on_radio_desc_examples_desc_toggled (object o, EventArgs args)
	{
		notebook_desc_examples.CurrentPage = 0;
	}
	private void on_radio_desc_examples_examples_toggled (object o, EventArgs args)
	{
		notebook_desc_examples.CurrentPage = 1;
	}

	private void on_button_next_clicked (object o, EventArgs args)
	{
		// do not ask about elastic. Since 2.2.2 depends on mode
		if (notebook_main.CurrentPage == Convert.ToInt32 (Pages.FORCE))
		{
			if (radio_force_sensor_raw.Active)
				notebook_main.CurrentPage = Convert.ToInt32 (Pages.REPSDETECT);
			else //if (radio_force_resultant.Active)
				notebook_main.CurrentPage = Convert.ToInt32 (Pages.MASS);
		} else if (notebook_main.CurrentPage < Convert.ToInt32 (Pages.OTHER))
			notebook_main.CurrentPage ++;
		else
			return;

		managePage(notebook_main.CurrentPage);
	}
	private void on_button_back_clicked (object o, EventArgs args)
	{
		// do not ask about elastic. Since 2.2.2 depends on mode
		if (notebook_main.CurrentPage == Convert.ToInt32 (Pages.REPSDETECT))
		{
			if (radio_force_sensor_raw.Active)
				notebook_main.CurrentPage = Convert.ToInt32 (Pages.FORCE);
			else //if (radio_force_resultant.Active)
				notebook_main.CurrentPage = Convert.ToInt32 (Pages.MASS);
		} else if (notebook_main.CurrentPage == Convert.ToInt32 (Pages.MASS))
			notebook_main.CurrentPage = Convert.ToInt32 (Pages.FORCE);
		else if (notebook_main.CurrentPage > Convert.ToInt32 (Pages.FORCE))
			notebook_main.CurrentPage --;
		else if (notebook_main.CurrentPage == Convert.ToInt32 (Pages.FORCE) &&
				! adding && exercise.Type == ForceSensorExercise.Types.BOTH)
			notebook_main.CurrentPage --;
		else
			return;

		managePage(notebook_main.CurrentPage);
	}

	private void on_radio_force_toggled (object o, EventArgs args)
	{
		managePage(Pages.FORCE);
	}
	private void on_radio_fixation_toggled (object o, EventArgs args)
	{
		managePage(Pages.FIXATION);
	}
	private void on_radio_mass_toggled (object o, EventArgs args)
	{
		managePage(Pages.MASS);
	}

	private void on_radio_detect_repetitions_toggled (object o, EventArgs args)
	{
		managePage(Pages.REPSDETECT);
	}

	/*
	private void on_check_show_ecc_toggled (object o, EventArgs args)
	{
		managePage(Pages.REPETITIONS);
	}
	*/
	private void on_radio_reps_show_concentric_toggled (object o, EventArgs args)
	{
		managePage(Pages.REPSSHOW);
	}
	private void on_radio_reps_show_both_toggled (object o, EventArgs args)
	{
		managePage(Pages.REPSSHOW);
	}

	private void on_button_force_exerted_help_clicked (object o, EventArgs args)
	{
		new DialogImageTest (
				Catalog.GetString("Projection of exerted force"),
				Util.GetImagePath(false) + "force_exerted_projected.png",
				DialogImageTest.ArchiveType.ASSEMBLY,
				"", -1, -1
				);
	}

	private void on_entry_name_changed (object o, EventArgs args)
	{
		Gtk.Entry entry = o as Gtk.Entry;
		if (o == null)
			return;

		entry_name.Text = Util.MakeValidSQL(entry.Text);
		entry_name.Text = Util.RemoveBackSlash (entry_name.Text);
		entry_name.Text = Util.RemoveSlash (entry_name.Text);
	}
	private void on_entry_description_changed (object o, EventArgs args)
	{
		Gtk.Entry entry = o as Gtk.Entry;
		if (o == null)
			return;

		entry_description.Text = Util.MakeValidSQL(entry.Text);
	}

	private void on_button_accept_clicked (object o, EventArgs args)
	{
		string name = entry_name.Text;

		if(name == "")
		{
			label_error.Text = Catalog.GetString("Error: Missing name of exercise.");
			hbox_error.Show();
			return;
		}
		else if (modeEnum == modesEnum.ADD && Sqlite.Exists(false, Constants.ForceSensorExerciseTable, name))
		{
			//if we add, check that this name does not exists
			label_error.Text = string.Format(Catalog.GetString("Error: An exercise named '{0}' already exists."), name);
			hbox_error.Show();
			return;
		}
		else if (modeEnum == modesEnum.EDIT)
		{
			//if we edit, check that this name does not exists (on other exercise, on current editing exercise is obviously fine)
			int getIdOfThis = Sqlite.ExistsAndGetUniqueID(false, Constants.ForceSensorExerciseTable, name); //if not exists will be -1
			if(getIdOfThis != -1 && getIdOfThis != exercise.UniqueID)
			{
				label_error.Text = string.Format(Catalog.GetString("Error: An exercise named '{0}' already exists."), name);
				hbox_error.Show();
				return;
			}
		}

		double eccMin = -1;
		double conMin = -1;
		if(! radio_detect_repetitions_from_prefs.Active)
		{
			if (radio_force_sensor_raw.Active || chronojumpMode == Constants.Modes.FORCESENSORISOMETRIC) {
				eccMin = spin_force_sensor_not_elastic_ecc_min_force.Value;
				conMin = spin_force_sensor_not_elastic_con_min_force.Value;
			} else {
				eccMin = spin_force_sensor_elastic_ecc_min_displ.Value;
				conMin = spin_force_sensor_elastic_con_min_displ.Value;
			}
		}

		//compare exercise (opening window) with exerciseNew (changes maybe done)

		//only store percentBodyWeight at SQL if radio_mass_add is active
		int percentBodyWeight = 0;
		if(radio_mass_add.Active && Convert.ToInt32(spin_body_mass_add.Value) > 0)
			percentBodyWeight = Convert.ToInt32(spin_body_mass_add.Value);

		int myID = -1;
		if(modeEnum == modesEnum.EDIT)
			myID = exercise.UniqueID;

		ForceSensorExercise.RepetitionsShowTypes repetitionsShow;
		if(radio_reps_show_concentric.Active)
			repetitionsShow = ForceSensorExercise.RepetitionsShowTypes.CONCENTRIC;
		else if(radio_reps_show_both.Active && radio_reps_show_both_together.Active)
			repetitionsShow = ForceSensorExercise.RepetitionsShowTypes.BOTHTOGETHER;
		else //if(radio_reps_show_both.Active && radio_reps_show_both_separated.Active)
			repetitionsShow = ForceSensorExercise.RepetitionsShowTypes.BOTHSEPARATED;

		ForceSensorExercise.Types typeNew = type;
		if (! adding && exercise.Type == ForceSensorExercise.Types.BOTH)
		{
			if (radio_type_isometric.Active)
				typeNew = ForceSensorExercise.Types.ISOMETRIC;
			else if (radio_type_elastic.Active)
				typeNew = ForceSensorExercise.Types.ELASTIC;
		}

		ForceSensorExercise exerciseTemp = new ForceSensorExercise(
				myID, entry_name.Text,
				percentBodyWeight,
				"", //resistance (unused, now merged on description)
				Convert.ToInt32(spin_angle.Value),
				entry_description.Text,
				radio_mass_subtract.Active, 	//tareBeforeCapture
				radio_force_resultant.Active,
				typeNew,
				repetitionsShow, eccMin, conMin);

		if(modeEnum == modesEnum.ADD)
		{
			exercise = exerciseTemp;
			SqliteForceSensorExercise.Insert(false, exercise);
			Success = true;
		} else {
			//we are editing the exercise
			if(exercise.Changed(exerciseTemp))
			{
				exercise = exerciseTemp;
				SqliteForceSensorExercise.Update(false, exercise);
				Success = true;
			}
		}

		//"exercise" will be read by reading "Exercise"
		fakeButtonReadValues.Click();
	}

	public Button FakeButtonReadValues {
		//set { fakeButtonReadValues = value; }
		get { return fakeButtonReadValues; }
	}

	public ForceSensorExercise Exercise
	{
		get { return exercise; }
	}

	public void HideAndNull()
	{
		ForceSensorExerciseWindowBox.force_sensor_exercise.Hide();
		ForceSensorExerciseWindowBox = null;
	}

	void on_button_cancel_clicked (object o, EventArgs args)
	{
		ForceSensorExerciseWindowBox.force_sensor_exercise.Hide();
		ForceSensorExerciseWindowBox = null;
	}

	private void on_delete_event (object o, DeleteEventArgs args)
	{
		LogB.Information("calling on_delete_event at ForceSensorExerciseWindow");

		args.RetVal = true;

		ForceSensorExerciseWindowBox.force_sensor_exercise.Hide();
		ForceSensorExerciseWindowBox = null;
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		//general widgets
		force_sensor_exercise = (Gtk.Window) builder.GetObject ("force_sensor_exercise");
		label_header = (Gtk.Label) builder.GetObject ("label_header");
		hbox_error = (Gtk.Box) builder.GetObject ("hbox_error");
		label_error = (Gtk.Label) builder.GetObject ("label_error");
		entry_name = (Gtk.Entry) builder.GetObject ("entry_name");
		notebook_main = (Gtk.Notebook) builder.GetObject ("notebook_main");
		notebook_desc_examples = (Gtk.Notebook) builder.GetObject ("notebook_desc_examples");
		radio_desc_examples_desc = (Gtk.RadioButton) builder.GetObject ("radio_desc_examples_desc");
		radio_desc_examples_examples = (Gtk.RadioButton) builder.GetObject ("radio_desc_examples_examples");
		label_help = (Gtk.Label) builder.GetObject ("label_help");
		label_radio_desc_examples_desc = (Gtk.Label) builder.GetObject ("label_radio_desc_examples_desc");
		label_radio_desc_examples_examples = (Gtk.Label) builder.GetObject ("label_radio_desc_examples_examples");
		textview_description = (Gtk.TextView) builder.GetObject ("textview_description");
		textview_examples = (Gtk.TextView) builder.GetObject ("textview_examples");
		button_back = (Gtk.Button) builder.GetObject ("button_back");
		button_next = (Gtk.Button) builder.GetObject ("button_next");
		button_accept = (Gtk.Button) builder.GetObject ("button_accept");
		image_back = (Gtk.Image) builder.GetObject ("image_back");
		image_next = (Gtk.Image) builder.GetObject ("image_next");
		image_cancel = (Gtk.Image) builder.GetObject ("image_cancel");

		//type tab
		radio_type_both = (Gtk.RadioButton) builder.GetObject ("radio_type_both");
		radio_type_isometric = (Gtk.RadioButton) builder.GetObject ("radio_type_isometric");
		radio_type_elastic = (Gtk.RadioButton) builder.GetObject ("radio_type_elastic");

		//force tab
		label_force = (Gtk.Label) builder.GetObject ("label_force");
		textview_force_explanation = (Gtk.TextView) builder.GetObject ("textview_force_explanation");
		radio_force_sensor_raw = (Gtk.RadioButton) builder.GetObject ("radio_force_sensor_raw");
		radio_force_resultant = (Gtk.RadioButton) builder.GetObject ("radio_force_resultant");
		button_force_exerted_help = (Gtk.Button) builder.GetObject ("button_force_exerted_help");
		image_force_exerted_help = (Gtk.Image) builder.GetObject ("image_force_exerted_help");

		//fixation tab
		label_fixation = (Gtk.Label) builder.GetObject ("label_fixation");
		textview_fixation_explanation = (Gtk.TextView) builder.GetObject ("textview_fixation_explanation");
		// disabled, using the "type" variable
		//radio_fixation_elastic = (Gtk.RadioButton) builder.GetObject ("radio_fixation_elastic");
		//radio_fixation_not_elastic = (Gtk.RadioButton) builder.GetObject ("radio_fixation_not_elastic");

		//mass tab
		label_mass = (Gtk.Label) builder.GetObject ("label_mass");
		textview_mass_explanation = (Gtk.TextView) builder.GetObject ("textview_mass_explanation");
		radio_mass_add = (Gtk.RadioButton) builder.GetObject ("radio_mass_add");
		radio_mass_subtract = (Gtk.RadioButton) builder.GetObject ("radio_mass_subtract");
		radio_mass_nothing = (Gtk.RadioButton) builder.GetObject ("radio_mass_nothing");
		hbox_body_mass_add = (Gtk.HBox) builder.GetObject ("hbox_body_mass_add");
		spin_body_mass_add = (Gtk.SpinButton) builder.GetObject ("spin_body_mass_add");

		//repetitions detect tab
		label_detect_repetitions = (Gtk.Label) builder.GetObject ("label_detect_repetitions");
		radio_detect_repetitions_from_prefs = (Gtk.RadioButton) builder.GetObject ("radio_detect_repetitions_from_prefs");
		radio_detect_repetitions_custom = (Gtk.RadioButton) builder.GetObject ("radio_detect_repetitions_custom");
		hbox_detect_repetitions_preferences = (Gtk.HBox) builder.GetObject ("hbox_detect_repetitions_preferences");
		hbox_detect_repetitions_elastic = (Gtk.HBox) builder.GetObject ("hbox_detect_repetitions_elastic");
		hbox_detect_repetitions_not_elastic = (Gtk.HBox) builder.GetObject ("hbox_detect_repetitions_not_elastic");
		label_repetitions_prefs_ecc_value = (Gtk.Label) builder.GetObject ("label_repetitions_prefs_ecc_value");
		label_repetitions_prefs_con_value = (Gtk.Label) builder.GetObject ("label_repetitions_prefs_con_value");
		label_repetitions_prefs_ecc_units = (Gtk.Label) builder.GetObject ("label_repetitions_prefs_ecc_units");
		label_repetitions_prefs_con_units = (Gtk.Label) builder.GetObject ("label_repetitions_prefs_con_units");
		spin_force_sensor_elastic_ecc_min_displ = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_elastic_ecc_min_displ");
		spin_force_sensor_elastic_con_min_displ = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_elastic_con_min_displ");
		spin_force_sensor_not_elastic_ecc_min_force = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_not_elastic_ecc_min_force");
		spin_force_sensor_not_elastic_con_min_force = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_not_elastic_con_min_force");

		//repetitions show tab
		//check_show_ecc = (Gtk.CheckButton) builder.GetObject ("check_show_ecc");
		radio_reps_show_concentric = (Gtk.RadioButton) builder.GetObject ("radio_reps_show_concentric");
		radio_reps_show_both = (Gtk.RadioButton) builder.GetObject ("radio_reps_show_both");
		alignment_reps_show_both = (Gtk.Alignment) builder.GetObject ("alignment_reps_show_both");
		radio_reps_show_both_together = (Gtk.RadioButton) builder.GetObject ("radio_reps_show_both_together");
		radio_reps_show_both_separated = (Gtk.RadioButton) builder.GetObject ("radio_reps_show_both_separated");

		//other tab
		label_other = (Gtk.Label) builder.GetObject ("label_other");
		textview_other_explanation = (Gtk.TextView) builder.GetObject ("textview_other_explanation");
		spin_angle = (Gtk.SpinButton) builder.GetObject ("spin_angle");
		entry_description = (Gtk.Entry) builder.GetObject ("entry_description");

		//fake button
		fakeButtonReadValues = (Gtk.Button) builder.GetObject ("fakeButtonReadValues");
	}

	~ForceSensorExerciseWindow() {}
}

