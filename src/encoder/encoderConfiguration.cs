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
 *  Copyright (C) 2004-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Mono.Unix;

public class EncoderConfigurationSQLObject
{
	public int uniqueID;
	public Constants.EncoderGI encoderGI;
	public bool active; //true or false. One true for each encoderGI (GRAVITATORY, INERTIAL)
	public string name;
	public EncoderConfiguration encoderConfiguration;
	public string description;

	public EncoderConfigurationSQLObject()
	{
		uniqueID = -1;
	}

	public EncoderConfigurationSQLObject(int uniqueID,
			Constants.EncoderGI encoderGI, bool active, string name,
			EncoderConfiguration encoderConfiguration,
			string description)
	{
		this.uniqueID = uniqueID;
		this.encoderGI = encoderGI;
		this.active = active;
		this.name = name;
		this.encoderConfiguration = encoderConfiguration;
		this.description = description;
	}

	//converts encoderConfiguration string from SQL
	public EncoderConfigurationSQLObject(int uniqueID,
			Constants.EncoderGI encoderGI, bool active, string name,
			string encoderConfigurationString,
			string description)
	{
		string [] strFull = encoderConfigurationString.Split(new char[] {':'});
		EncoderConfiguration econf = new EncoderConfiguration(
				(EncoderConfiguration.Names)
				Enum.Parse(typeof(EncoderConfiguration.Names), strFull[0]) );
		econf.ReadParamsFromSQL(strFull);

		this.uniqueID = uniqueID;
		this.encoderGI = encoderGI;
		this.active = active;
		this.name = name;
		this.encoderConfiguration = econf;
		this.description = description;
	}

	//imports from file
	public EncoderConfigurationSQLObject(string contents)
	{
		string line;
		using (StringReader reader = new StringReader (contents)) {
			do {
				line = reader.ReadLine ();

				if (line == null)
					break;
				if (line == "" || line[0] == '#')
					continue;

				string [] parts = line.Split(new char[] {'='});
				if(parts.Length != 2)
					continue;

				uniqueID = -1;
				if(parts[0] == "encoderGI")
				{
					if(Enum.IsDefined(typeof(Constants.EncoderGI), parts[1]))
						encoderGI = (Constants.EncoderGI) Enum.Parse(typeof(Constants.EncoderGI), parts[1]);
				}

				//active is not needed on import, because on import it's always marked as active
				else if(parts[0] == "active" && parts[1] != "")
					active = (parts[1] == "True");
				else if(parts[0] == "name" && parts[1] != "")
					name = parts[1];
				else if(parts[0] == "EncoderConfiguration")
				{
					string [] ecFull = parts[1].Split(new char[] {':'});
					if(Enum.IsDefined(typeof(EncoderConfiguration.Names), ecFull[0]))
					{
						//create object
						encoderConfiguration = new EncoderConfiguration(
								(EncoderConfiguration.Names)
								Enum.Parse(typeof(EncoderConfiguration.Names), ecFull[0]) );
						//assign the rest of params
						encoderConfiguration.ReadParamsFromSQL(ecFull);
					}
				}
				else if(parts[0] == "description" && parts[1] != "")
					description = parts[1];
			} while(true);
		}
	}

	public string ToSQLInsert()
	{
		 string idStr = uniqueID.ToString();
		 if(idStr == "-1")
			 idStr = "NULL";

		 return idStr +
			 ", '" + encoderGI.ToString() + "'" +
			 ", '" + active.ToString() + "'" +
			 ", '" + name + "'" +
			 ", '" + encoderConfiguration.ToStringOutput(EncoderConfiguration.Outputs.SQL) + "'" +
			 ", '" + description + "'" +
			 ", '', '', ''"; //future1, future2, future3
	}

	public string ToFile()
	{
		return
			"#Case sensitive!\n" +
			"#Comments start with sharp sign\n" +
			"#Options are key/values with an = separating them\n" +
			"#DO NOT write comments in the same line than key/value pairs\n" +
			"#\n" +
			"#DO NOT WRITE SPACES JUST BEFORE OR AFTER THE '=' SIGN\n" +
			"#This work:\n" +
			"#name=My encoder config\n" +
			"#This doesn't work:\n" +
			"#name= My encoder config\n" +
			"#\n" +
			"#Whitelines are allowed\n" +
			"\nname=" + name + "\n" +
			"description=" + description + "\n" +
			"\n#encoderGI must be GRAVITATORY or INERTIAL\n" +
			"encoderGI=" + encoderGI.ToString() + "\n" +
			"\n#EncoderConfiguration if exists, this will be used and cannot be changed\n" +
"#name:d:D:anglePush:angleWeight:inertiaMachine:gearedDown:inertiaTotal:extraWeightN:extraWeightGrams:extraWeightLenght:list_d\n" +
"#list_d is list of anchorages in centimeters. each value separated by '_' . Decimal separator is '.'\n" +
			"EncoderConfiguration=" + encoderConfiguration.ToStringOutput(EncoderConfiguration.Outputs.SQL);
	}
}

public class EncoderConfiguration
{
	public enum Names { //this names are used on util.R and graph.R change there also if needed
		// ---- LINEAR ----
		LINEAR, LINEARINVERTED, LINEARINERTIAL, 
		WEIGHTEDMOVPULLEYLINEARONPERSON1, WEIGHTEDMOVPULLEYLINEARONPERSON1INV,
		WEIGHTEDMOVPULLEYLINEARONPERSON2, WEIGHTEDMOVPULLEYLINEARONPERSON2INV,
		WEIGHTEDMOVPULLEYONLINEARENCODER, 
		LINEARONPLANE, LINEARONPLANEWEIGHTDIFFANGLE, LINEARONPLANEWEIGHTDIFFANGLEMOVPULLEY,
		PNEUMATIC,
		// ---- ROTARY FRICTION ----
		ROTARYFRICTIONSIDE, ROTARYFRICTIONAXIS,
		WEIGHTEDMOVPULLEYROTARYFRICTION,
		ROTARYFRICTIONSIDEINERTIAL, ROTARYFRICTIONAXISINERTIAL,
		ROTARYFRICTIONSIDEINERTIALLATERAL, ROTARYFRICTIONAXISINERTIALLATERAL,
		ROTARYFRICTIONSIDEINERTIALMOVPULLEY, ROTARYFRICTIONAXISINERTIALMOVPULLEY,
		// ---- ROTARY AXIS ----
		ROTARYAXIS, WEIGHTEDMOVPULLEYROTARYAXIS,
		ROTARYAXISINERTIAL, ROTARYAXISINERTIALLATERAL, ROTARYAXISINERTIALMOVPULLEY, ROTARYAXISINERTIALLATERALMOVPULLEY
	}

	public Names name;
	public Constants.EncoderType type;
	public int position; //used to find values on the EncoderConfigurationList. Numeration changes on every encoder and on not inertial/inertial
	public string image;
	public string code;	//this code will be stored untranslated but will be translated just to be shown
	public string text;
	public bool has_d;	//axis
	public bool has_D;	//external disc or pulley
	public bool has_angle_push;
	public bool has_angle_weight;
	public bool has_inertia;
	public bool has_gearedDown;
	public bool rotaryFrictionOnAxis;
	public double d;	//axis 		//ATTENTION: this inertial param can be changed on main GUI
	public double D;	//external disc or pulley
	public int anglePush;
	public int angleWeight;
	
	public int inertiaMachine; //this is the inertia without the disc
	
	// see methods: GearedUpDisplay() SetGearedDownFromDisplay(string gearedUpStr) 
	public int gearedDown;	//demultiplication
	
	public int inertiaTotal; //this is the inertia used by R
	public int extraWeightN; //how much extra weights (inertia) //ATTENTION: this param can be changed on main GUI
	public int extraWeightGrams; //weight of each extra weight (inertia)
	public double extraWeightLength; //length from center to center (cm) (inertia)
	
	public List_d list_d;	//object managing a list of diameters depending on the anchorage position


	public string textDefault = Catalog.GetString("Linear encoder attached to a barbell.") + "\n" + 
		Catalog.GetString("Also common gym tests like jumps or chin-ups.");

	//this is the default values
	public EncoderConfiguration()
	{
		name = Names.LINEAR;
		type = Constants.EncoderType.LINEAR;
		position = 0;
		image = Constants.FileNameEncoderLinearFreeWeight;
		code = Constants.DefaultEncoderConfigurationCode;
		text = textDefault;

		setDefaultOptions();
	}

	// note: if this changes, change also in:
	// UtilEncoder.EncoderConfigurationList(enum encoderType)
	
	public EncoderConfiguration(Names name)
	{
		this.name = name;
		setDefaultOptions();

		// ---- LINEAR ----
		// ---- not inertial
		if(name == Names.LINEAR) {
			type = Constants.EncoderType.LINEAR;
			position = 0;
			image = Constants.FileNameEncoderLinearFreeWeight;
			code = Constants.DefaultEncoderConfigurationCode;
			text = textDefault;
		}
		else if(name == Names.LINEARINVERTED) {
			type = Constants.EncoderType.LINEAR;
			position = 1;
			image =Constants.FileNameEncoderLinearFreeWeightInv;
			code = "Linear inv - barbell";
			text = Catalog.GetString("Linear encoder inverted attached to a barbell.");
		}
		else if(name == Names.WEIGHTEDMOVPULLEYLINEARONPERSON1) {
			type = Constants.EncoderType.LINEAR;
			position = 2;
			image = Constants.FileNameEncoderWeightedMovPulleyOnPerson1;
			code = "Linear - barbell - moving pulley";
			text = Catalog.GetString("Linear encoder attached to a barbell.") + " " + 
				Catalog.GetString("Barbell is connected to a weighted moving pulley.") 
				+ " " + Catalog.GetString("Mass is geared down by 2."); 
		
			gearedDown = 2;
		}
		else if(name == Names.WEIGHTEDMOVPULLEYLINEARONPERSON1INV) {
			type = Constants.EncoderType.LINEAR;
			position = 3;
			image = Constants.FileNameEncoderWeightedMovPulleyOnPerson1Inv;
			code = "Linear inv - barbell - moving pulley";
			text = Catalog.GetString("Linear encoder inverted attached to a barbell.") + " " + 
				Catalog.GetString("Barbell is connected to a weighted moving pulley.")
				+ " " + Catalog.GetString("Mass is geared down by 2."); 
		
			gearedDown = 2;
		}
		else if(name == Names.WEIGHTEDMOVPULLEYLINEARONPERSON2) {
			type = Constants.EncoderType.LINEAR;
			position = 4;
			image = Constants.FileNameEncoderWeightedMovPulleyOnPerson2;
			code = "Linear - barbell - pulley - moving pulley";
			text = Catalog.GetString("Linear encoder attached to a barbell.") + " " + 
				Catalog.GetString("Barbell is connected to a fixed pulley that is connected to a weighted moving pulley.")
				+ " " + Catalog.GetString("Mass is geared down by 2."); 
		
			gearedDown = 2;
		}
		else if(name == Names.WEIGHTEDMOVPULLEYLINEARONPERSON2INV) {
			type = Constants.EncoderType.LINEAR;
			position = 5;
			image = Constants.FileNameEncoderWeightedMovPulleyOnPerson2Inv;
			code = "Linear inv - barbell - pulley - moving pulley";
			text = Catalog.GetString("Linear encoder inverted attached to a barbell.") + " " + 
				Catalog.GetString("Barbell is connected to a fixed pulley that is connected to a weighted moving pulley.")
				+ " " + Catalog.GetString("Mass is geared down by 2."); 
		
			gearedDown = 2;
		}
		else if(name == Names.WEIGHTEDMOVPULLEYONLINEARENCODER) {
			type = Constants.EncoderType.LINEAR;
			position = 6;
			image = Constants.FileNameEncoderWeightedMovPulleyOnLinearEncoder;
			code = "Linear - moving pulley";
			text = Catalog.GetString("Linear encoder attached to a weighted moving pulley.")
				+ " " + Catalog.GetString("Mass is geared down by 2."); 
		
			gearedDown = 2;
		}
		else if(name == Names.LINEARONPLANE) {
			type = Constants.EncoderType.LINEAR;
			position = 7;
			image = Constants.FileNameEncoderLinearOnPlane;
			code = "Linear - inclined plane";
			text = Catalog.GetString("Linear encoder on a inclined plane.") + "\n" + 
				Catalog.GetString("Suitable also for horizontal movement. Just set a 0 push angle.");
			
			has_angle_push = true;
			has_angle_weight = false;
		}
		else if(name == Names.LINEARONPLANEWEIGHTDIFFANGLE) {
			type = Constants.EncoderType.LINEAR;
			position = 8;
			image = Constants.FileNameEncoderLinearOnPlaneWeightDiffAngle;
			code = "Linear - inclined plane different angle";
			text = Catalog.GetString("Linear encoder on a inclined plane moving a weight in different angle.") + "\n" +
				Catalog.GetString("Suitable also for horizontal movement. Just set a 0 push angle.");
			
			has_angle_push = true;
			has_angle_weight = true;
		}
		else if(name == Names.LINEARONPLANEWEIGHTDIFFANGLEMOVPULLEY) {
			type = Constants.EncoderType.LINEAR;
			position = 9;
			image = Constants.FileNameEncoderLinearOnPlaneWeightDiffAngleMovPulley;
			code = "Linear - inclined plane different angle - moving pulley";
			text = Catalog.GetString("Linear encoder on a inclined plane moving a weight in different angle.") + "\n" +
				Catalog.GetString("Suitable also for horizontal movement. Just set a 0 push angle.") + "\n" +
				Catalog.GetString("Force demultiplier refers to the times the rope comes in and comes out from the moving pulley attached to the extra load.") +
				" " + Catalog.GetString("In the example image demultiplier is 2, hence multiplier is 1/2.");

			has_angle_push = true;
			has_angle_weight = true;
			has_gearedDown = true;
		}
		if(name == Names.PNEUMATIC) {
			type = Constants.EncoderType.LINEAR;
			position = 10;
			image = Constants.FileNameEncoderLinearPneumatic;
			code = "Linear - Pneumatic machine";
			text = "Linear encoder connected to a pneumatic machine";

			has_angle_push = true;
		}
		// ---- inertial
		else if(name == Names.LINEARINERTIAL) {
			type = Constants.EncoderType.LINEAR;
			position = 0;
			image = Constants.FileNameEncoderLinearInertial;
			code = "Linear - inertial machine";
			text = Catalog.GetString("Linear encoder on inertia machine.") + "\n" + 
				Catalog.GetString("Configuration NOT Recommended! Please use a rotary encoder.") + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled");
			
			has_d = true;
			has_inertia = true;
		}
		// ---- ROTARY FRICTION ----
		// ---- not inertial
		else if(name == Names.ROTARYFRICTIONSIDE) {
			type = Constants.EncoderType.ROTARYFRICTION;
			position = 0;
			image = Constants.FileNameEncoderFrictionSide;
			code = "Rotary friction - pulley";
			text = Catalog.GetString("Rotary friction encoder on pulley.");
		}
		else if(name == Names.ROTARYFRICTIONAXIS) {
			type = Constants.EncoderType.ROTARYFRICTION;
			position = 1;
			image = Constants.FileNameEncoderFrictionAxis;
			code = "Rotary friction - pulley axis";
			text = Catalog.GetString("Rotary friction encoder on pulley axis.");

			has_d = true;
			has_D = true;
		}
		else if(name == Names.WEIGHTEDMOVPULLEYROTARYFRICTION) {
			type = Constants.EncoderType.ROTARYFRICTION;
			position = 2;
			image = Constants.FileNameEncoderFrictionWithMovPulley;
			code = "Rotary friction - moving pulley";
			text = Catalog.GetString("Rotary friction encoder on weighted moving pulley.");
		}
		// ---- inertial
		// ---- rotary friction not on axis
		else if(name == Names.ROTARYFRICTIONSIDEINERTIAL) {
			type = Constants.EncoderType.ROTARYFRICTION;
			position = 0;
			image = Constants.FileNameEncoderFrictionSideInertial;
			code = "Rotary friction - inertial machine side";
			text = Catalog.GetString("Rotary friction encoder on inertial machine side.") + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled");

			has_d = true;
			has_D = true;
			has_inertia = true;
		}
		else if(name == Names.ROTARYFRICTIONSIDEINERTIALLATERAL) {
			type = Constants.EncoderType.ROTARYFRICTION;
			position = 1;
			image = Constants.FileNameEncoderFrictionSideInertialLateral;
			code = "Rotary friction - inertial machine side - horizontal movement";
			text = Catalog.GetString("Rotary friction encoder on inertial machine when person is moving horizontally.") + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled");

			has_d = true;
			has_D = true;
			has_inertia = true;
		}
		else if(name == Names.ROTARYFRICTIONSIDEINERTIALMOVPULLEY) {
			type = Constants.EncoderType.ROTARYFRICTION;
			position = 2;
			image = Constants.FileNameEncoderFrictionSideInertialMovPulley;
			code = "Rotary friction - inertial machine side geared up";
			text = Catalog.GetString("Rotary friction encoder on inertial machine geared up.") + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled") + "\n" + 
				Catalog.GetString("Inertial machine rolls twice faster than body."); 

			has_d = true;
			has_D = true;
			has_inertia = true;
			has_gearedDown = true;
		}

		// ---- rotary friction on axis
		else if(name == Names.ROTARYFRICTIONAXISINERTIAL) {
			type = Constants.EncoderType.ROTARYFRICTION;
			position = 0;
			image = Constants.FileNameEncoderFrictionAxisInertial;
			code = "Rotary friction axis - inertial machine axis";
			text = Catalog.GetString("Rotary friction encoder on inertial machine axis.") + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled");

			has_d = true;
			has_inertia = true;
			rotaryFrictionOnAxis = true;
		}
		else if(name == Names.ROTARYFRICTIONAXISINERTIALLATERAL) {
			type = Constants.EncoderType.ROTARYFRICTION;
			position = 1;
			image = Constants.FileNameEncoderFrictionAxisInertialLateral;
			code = "Rotary friction - inertial machine axis - horizontal movement";
			text = Catalog.GetString("Rotary friction encoder on inertial machine when person is moving horizontally.") + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled");

			has_d = true;
			has_inertia = true;
			rotaryFrictionOnAxis = true;
		}
		else if(name == Names.ROTARYFRICTIONAXISINERTIALMOVPULLEY) {
			type = Constants.EncoderType.ROTARYFRICTION;
			position = 2;
			image = Constants.FileNameEncoderFrictionAxisInertialMovPulley;
			code = "Rotary friction - inertial machine axis geared up";
			text = Catalog.GetString("Rotary friction encoder on inertial machine geared up.") + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled") + "\n" + 
				Catalog.GetString("Inertial machine rolls twice faster than body."); 

			has_d = true;
			has_inertia = true;
			rotaryFrictionOnAxis = true;
			has_gearedDown = true;
		}

		// ---- ROTARY AXIS ----
		// ---- not inertial
		else if(name == Names.ROTARYAXIS) {
			type = Constants.EncoderType.ROTARYAXIS;
			position = 0;
			image = Constants.FileNameEncoderRotaryAxisOnAxis;
			code = "Rotary axis - pulley axis";
			text = Catalog.GetString("Rotary axis encoder on pulley axis.");

			has_D = true;
		}
		else if(name == Names.WEIGHTEDMOVPULLEYROTARYAXIS) {
			type = Constants.EncoderType.ROTARYAXIS;
			position = 1;
			image = Constants.FileNameEncoderAxisWithMovPulley;
			code = "Rotary axis - moving pulley";
			text = Catalog.GetString("Rotary axis encoder on weighted moving pulley.")
				+ " " + Catalog.GetString("Mass is geared down by 2."); 
			
			gearedDown = 2;
		}
		// ---- inertial
		else if(name == Names.ROTARYAXISINERTIAL) {
			type = Constants.EncoderType.ROTARYAXIS;
			position = 0;
			image = Constants.FileNameEncoderAxisInertial;
			code = "Rotary axis - inertial machine";
			text = Catalog.GetString("Rotary axis encoder on inertial machine.") + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled");

			has_d = true;
			has_inertia = true;
		}
		else if(name == Names.ROTARYAXISINERTIALLATERAL) {
			type = Constants.EncoderType.ROTARYAXIS;
			position = 1;
			image = Constants.FileNameEncoderAxisInertialLateral;
			code = "Rotary axis - inertial machine - horizontal movement";
			text = Catalog.GetString("Rotary axis encoder on inertial machine when person is moving horizontally.") + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled");

			has_d = true;
			has_inertia = true;
		}
		else if(name == Names.ROTARYAXISINERTIALMOVPULLEY) {
			type = Constants.EncoderType.ROTARYAXIS;
			position = 2;
			image = Constants.FileNameEncoderAxisInertialMovPulley;
			code = "Rotary axis - inertial machine geared up";
			text = Catalog.GetString("Rotary axis encoder on inertial machine geared up.") + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled") + "\n" + 
				Catalog.GetString("Inertial machine rolls twice faster than body."); 

			has_d = true;
			has_inertia = true;
			has_gearedDown = true;
		}
		else if(name == Names.ROTARYAXISINERTIALLATERALMOVPULLEY) {
			type = Constants.EncoderType.ROTARYAXIS;
			position = 3;
			image = Constants.FileNameEncoderAxisInertialMovPulleyLateral;
			code = "Rotary axis - inertial machine - horizontal movement - geared up";
			text = Catalog.GetString("Rotary axis encoder on inertial machine geared up when person is moving horizontally.") + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled") + "\n" +
				Catalog.GetString("Inertial machine rolls twice faster than body.");

			has_d = true;
			has_inertia = true;
			has_gearedDown = true;
		}
	}

	private void setDefaultOptions()
	{
		has_d = false;
		has_D = false;
		has_angle_push = false;
		has_angle_weight = false;
		has_inertia = false;
		has_gearedDown = false; //gearedDown can be changed by user
		rotaryFrictionOnAxis = false;
		d = -1;
		D = -1;
		anglePush = -1;
		angleWeight = -1;
		inertiaMachine = -1;
		gearedDown = 1;
		inertiaTotal = -1;
		extraWeightN = 0;
		extraWeightGrams = 0;
		extraWeightLength = 1;
		list_d = new List_d();
	}

	public void SetInertialDefaultOptions()
	{
		//after creating Names.ROTARYAXISINERTIAL
		inertiaMachine = 900;
		d = 5;
		list_d = new List_d(d);
		inertiaTotal = UtilEncoder.CalculeInertiaTotal(this);
	}

	public bool Equals(EncoderConfiguration other)
	{
		return (this.ToStringOutput(Outputs.SQL) == other.ToStringOutput(Outputs.SQL));
	}

	public void ReadParamsFromSQL (string [] strFull) 
	{
		//adds other params
		this.d = 	   Convert.ToDouble(Util.ChangeDecimalSeparator(strFull[1]));
		this.D = 	   Convert.ToDouble(Util.ChangeDecimalSeparator(strFull[2]));
		this.anglePush =   Convert.ToInt32(strFull[3]);
		this.angleWeight = Convert.ToInt32(strFull[4]);
		this.inertiaMachine = 	Convert.ToInt32(strFull[5]);
		this.gearedDown =  Convert.ToInt32(strFull[6]);
		this.inertiaTotal = 	Convert.ToInt32(strFull[7]);
		this.extraWeightN = 	Convert.ToInt32(strFull[8]);
		this.extraWeightGrams = Convert.ToInt32(strFull[9]);
		this.extraWeightLength = Convert.ToDouble(Util.ChangeDecimalSeparator(strFull[10]));

		//check needed when updating DB to 1.36
		if(strFull.Length == 12)
			this.list_d.ReadFromSQL(strFull[11]);
	}

	public enum Outputs { ROPTIONS, RCSV, SQL, SQLECWINCOMPARE}
	/*
	 * SQLECWINCOMPARE is to know it two encoderConfigurations on encoderConfigurationWindow are the same
	 * because two inertial params change outside that window
	 */
	
	public string ToStringOutput(Outputs o) 
	{
		//for R and SQL		
		string str_d = Util.ConvertToPoint(d);
		string str_D = Util.ConvertToPoint(D);
		
		string sep = "";

		if(o == Outputs.ROPTIONS) {
			sep = "\n";
			return 
				"#name" + sep + 	name + sep + 
				"#str_d" + sep + 	str_d + sep + 
				"#str_D" + sep + 	str_D + sep + 
				"#anglePush" + sep + 	anglePush.ToString() + sep + 
				"#angleWeight" + sep + 	angleWeight.ToString() + sep +
				"#inertiaTotal" + sep + inertiaTotal.ToString() + sep + 
				"#gearedDown" + sep + 	gearedDown.ToString()
				;
		}
		else if (o == Outputs.RCSV) { //not single curve
			sep = ",";
			//do not need inertiaMachine, extraWeightN, extraWeightGrams, extraWeightLength (unneded for the R calculations)
			return 
				name + sep + 
				str_d + sep + 
				str_D + sep + 
				anglePush.ToString() + sep + 
				angleWeight.ToString() + sep +
				inertiaTotal.ToString() + sep + 
				gearedDown.ToString()
				;
		}
		else { //(o == Outputs.SQL || o == OUTPUTS.SQLECWINCOMPARE)
			sep = ":";

			string my_str_d = str_d;
			string my_str_extraWeightN = extraWeightN.ToString();

			if(o == Outputs.SQLECWINCOMPARE)
			{
				//this inertial params can be changed on main GUI
				my_str_d = "%";
				my_str_extraWeightN  = "%";
			}
			return 
				name + sep + 
				my_str_d + sep +
				str_D + sep + 
				anglePush.ToString() + sep + 
				angleWeight.ToString() + sep +
				inertiaMachine.ToString() + sep + 
				gearedDown.ToString() + sep + 
				inertiaTotal.ToString() + sep + 
				my_str_extraWeightN + sep +
				extraWeightGrams.ToString() + sep +
				extraWeightLength.ToString() + sep +
				list_d.ToString();
		}
	}

	//just to show on a treeview	
	public string ToStringPretty() {
		string sep = "; ";

		string str_d = "";
		if(d != -1)
			str_d = sep + "d=" + d.ToString();

		string str_D = "";
		if(D != -1)
			str_D = sep + "D=" + D.ToString();

		string str_anglePush = "";
		if(anglePush != -1)
			str_anglePush = sep + "push angle=" + anglePush.ToString();

		string str_angleWeight = "";
		if(angleWeight != -1)
			str_angleWeight = sep + "weight angle=" + angleWeight.ToString();

		string str_inertia = "";
		if(has_inertia && inertiaTotal != -1)
			str_inertia = sep + "inertia total=" + inertiaTotal.ToString();

		string str_gearedDown = "";
		if(gearedDown != 1)	//1 is the default
			str_gearedDown = sep + "geared down=" + gearedDown.ToString();

		return code + str_d + str_D + str_anglePush + str_angleWeight + str_inertia + str_gearedDown;
	}

	// while capture to show correctly on screen and to send the correct concentric cut to R
	public bool IsInverted ()
	{
		if (name == Names.LINEARINVERTED ||
				name == Names.WEIGHTEDMOVPULLEYLINEARONPERSON1INV ||
				name == Names.WEIGHTEDMOVPULLEYLINEARONPERSON2INV)
			return true;

		return false;
	}

	/*
	 * IMPORTANT: on GUI is gearedDown is shown as UP (for clarity: 4, 3, 2, 1, 1/2, 1/3, 1/4)
	 * on C#, R, SQL we use "gearedDown" for historical reasons. So a conversion is done on displaying data to user
	 * gearedDown is stored as integer on database and is converted to this gearedUp for GUI
	 * R will do another conversion and will use the double
	 *   4   ,    3    ,  2  , 1/2, 1/3, 1/4		#gearedUp string (GUI)
	 *  -4   ,   -3    , -2  ,   2,   3,   4		#gearedDown
	 *   0.25,    0.333,  0.5,   2,   3,   4		#gearedDown on R (see readFromFile.gearedDown() on util.cs)
	 */
	public string GearedUpDisplay() 
	{
		switch(gearedDown) {
			case -4:
				return "4";
			case -3:
				return "3";
			case -2:
				return "2";
			case 2:
				return "1/2";
			case 3:
				return "1/3";
			case 4:
				return "1/4";
			default:
				return "2";
		}
	}
	public void SetGearedDownFromDisplay(string gearedUpStr) 
	{
		switch(gearedUpStr) {
			case "4":
				gearedDown = -4;
				break;
			case "3":
				gearedDown = -3;
				break;
			case "2":
				gearedDown = -2;
				break;
			case "1/2":
				gearedDown = 2;
				break;
			case "1/3":
				gearedDown = 3;
				break;
			case "1/4":
				gearedDown = 4;
				break;
			default:
				gearedDown = -2;
				break;
		}
	}
	//same as util.R readFromFile.gearedDown (to have same formulas than R on encoderLikeR methods
	public double gearedDownLikeR
	{
		get {
			if (gearedDown > 0)
				return gearedDown;

			return Math.Abs ( 1 / (1.0 * gearedDown) );
		}
	}
	public double inertiaTotalLikeR
	{
		get { return inertiaTotal / 10000.0; } //comes in Kg*cm^2 eg: 100; convert it to Kg*m^2 eg: 0.010
	}

}
