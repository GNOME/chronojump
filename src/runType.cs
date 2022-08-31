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
 *  Copyright (C) 2004-2022   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using Mono.Unix;

public class RunType : EventType 
{
	protected bool hasIntervals;
	protected double distance;
	protected bool tracksLimited;
	protected int fixedValue;
	protected bool unlimited;
	private string distancesString; //new at 0.8.1.5:
		       			//when distance is 0 or >0, distancesString it's ""
					//when distance is -1, distancesString is distance of each track, 
					//	eg: "7-5-9" for a runInterval with three tracks of 7, 5 and 9 meters each
					//	this is nice for agility tests
	 				//	and RSA: distancesString 8-4-R3-5   means: 8m, 4m, rest 3 seconds, 5m
					//		we know it's an RSA because there's an R in this variable

	public RunType() {
		type = Types.RUN;
	}
	
	public override bool FindIfIsPredefined() {
		string [] predefinedTests = {
			"Custom", "20m", "100m", "200m", "400m",
			"1000m", "2000m",
			"Agility-20Yard", "Agility-505",
			"Agility-Illinois", "Agility-Shuttle-Run" , "Agility-ZigZag",
			"Agility-T-Test", "Margaria",
			"byLaps", "byTime", "unlimited", 
			"20m10times", "7m30seconds", "20m endurance", 
			"MTGUG", "Gesell-DBT",
			"Agility-3L3R",
			"RSA 8-4-R3-5", 		//this is not used anymore (it was just a test)
			"RSA Aziz 2000 40, R30 x 8",
			"RSA Balsom 15, R30 x 40",
			"RSA Balsom 30, R30 x 20",
			"RSA Balsom 40, R30 x 15",
			"RSA Dawson 40, R24 x 6",
			"RSA Fitzsimons 40, R24 x 6",
			"RSA Gaitanos 6, R30 x 10",
			"RSA Hamilton 6, R30 x 10",
			"RSA RAST 35, R10 x 6",
			"RSA Mujica 15, R24 x 6",
			"RSA Wadley 20, R17 x 12",
			"RSA Wragg 34.2, R25 x 7"
		};

		foreach(string search in predefinedTests)
			if(this.name == search)
				return true;

		return false;
	}
	
	//predefined values
	public RunType(string name) {
		type = Types.RUN;
		this.name = name;

		unlimited = false;	//default value
		imageFileName = "";
		distancesString = "";
		
		//if this changes, sqlite/runType.cs initialize tables should change
		//
		//no interval
		if(name == "Custom") {
			hasIntervals 	= false; 
			distance 	= 0;
			tracksLimited 	= false;
			fixedValue 	= 0;
			description	= Catalog.GetString("Variable distance running");
			imageFileName = "run_simple.png";
		} else if(name == "20m") {
			hasIntervals 	= false; 
			distance 	= 20;
			tracksLimited 	= false;
			fixedValue 	= 0;
			description	= Catalog.GetString("Run 20 meters");
			imageFileName = "run_simple.png";
		} else if(name == "100m") {
			hasIntervals 	= false; 
			distance 	= 100;
			tracksLimited 	= false;
			fixedValue 	= 0;
			description	= Catalog.GetString("Run 100 meters");
			imageFileName = "run_simple.png";
		} else if(name == "200m") {
			hasIntervals 	= false; 
			distance 	= 200;
			tracksLimited 	= false;
			fixedValue 	= 0;
			description	= Catalog.GetString("Run 200 meters");
			imageFileName = "run_simple.png";
		} else if(name == "400m") {
			hasIntervals 	= false; 
			distance 	= 400;
			tracksLimited 	= false;
			fixedValue 	= 0;
			description	= Catalog.GetString("Run 400 meters");
			imageFileName = "run_simple.png";
		} else if(name == "1000m") {
			hasIntervals 	= false; 
			distance 	= 1000;
			tracksLimited 	= false;
			fixedValue 	= 0;
			description	= Catalog.GetString("Run 1000 meters");
			imageFileName = "run_simple.png";
		} else if(name == "2000m") {
			hasIntervals 	= false; 
			distance 	= 2000;
			tracksLimited 	= false;
			fixedValue 	= 0;
			description	= Catalog.GetString("Run 2000 meters");
			imageFileName = "run_simple.png";
		} //balance
		else if(name == "Gesell-DBT") {
			hasIntervals 	= false; 
			distance 	= 2.5;
			tracksLimited 	= false;
			fixedValue 	= 0;
			imageFileName = "gesell_dbt.png";
			description	= "Gesell Dynamic Balance Test";
			longDescription	= 
				"<b>" + Catalog.GetString("Note on measurement") + "</b>: \n" +
				Catalog.GetString("Measured time will be the time between two platforms") + "\n\n" +
				"<b>" + Catalog.GetString("Short description") + "</b>: \n" +
			       Catalog.GetString("Subjects had to walk over the bar as fast as possible.") + "\n" +
			       Catalog.GetString("From one platform to another without falling down.") + "\n" +
			       Catalog.GetString("If they touched the ground they had to continue.") + "\n" +
			       Catalog.GetString("The hands were on their waist.") + "\n" +
			       Catalog.GetString("Without shoes.") + "\n" +
			       Catalog.GetString("Every ground contact is penalized with 2 seconds.") + "\n" +
			       Catalog.GetString("The best of 2 attempts were recorded.") + "\n\n" +
				
			       "<b>" + Catalog.GetString("Gesell's Bar") + "</b>: " +
			       Catalog.GetString("Length: 2.5 m.") + "\n" +
			       Catalog.GetString("Wide: 4 cm.") + "\n\n" +

				"<b>" + Catalog.GetString("Protocol") + "</b>: " + 
				Catalog.GetString("CONDITIONS: ") + "\n" +
				"- " + Catalog.GetString("Without shoes.") + "\n" +
				"- " + Catalog.GetString("Hands on their waist.") + "\n" +
				"- " + Catalog.GetString("In front of a wall in order to avoid distractions.") + "\n\n" +
				Catalog.GetString("INSTRUCTIONS AND DEMONSTRATION: ") + "\n" +
				"- " + Catalog.GetString("You have to walk on this bar as fast as possible \'like this\', if you touch the ground just continue.") + "\n" +
				Catalog.GetString("\'Like this\' means normal, with a foot in front of the other, not side by side.") + "\n\n" +
				
				Catalog.GetString("SCORE: ") + "\n" +
				"- " + Catalog.GetString("Time will start since first platform is touched, and will stop when second platform is reached.") + "\n" +
				"- " + Catalog.GetString("Every ground contact is penalized with 2 seconds.") + "\n" +
			        "- " + Catalog.GetString("The best of 2 attempts will be recorded.") + "\n\n" +

				"<b>" + Catalog.GetString("Reference:") + "</b>\n" +
				"Cabedo, J. (2005) L'evolució de l'equilibri durant el cicle vital. En Liceu Psicològic. Accessible a http://www.liceupsicologic.org/tesis/Tesi. Pep Cabedo.pdf (Consulted Apr 30 2009).";
			/*
				"<b>" + Catalog.GetString("Abstract:") + "</b>\n" +
	    			"http://www.revista-apunts.com/apunts.php?id_pagina=7&id_post=837&lang=en";
				*/
		} //agility
		else if(name == "Agility-20Yard") {
			hasIntervals 	= false; 
			distance 	= 18.28;
			tracksLimited 	= false;
			fixedValue 	= 0;
			imageFileName = "agility_20yard.png";
			description	= Catalog.GetString("20Yard Agility test");
			longDescription	= 
				Catalog.GetString("This test is part of a battery for the USA Women's Soccer Team. The NFL use a very similar test for the NFL Combine Testing, the 20 yard shuttle.") + "\n\n" +

				"<b>" + Catalog.GetString("Purpose") + "</b>: " +
			       Catalog.GetString("The 20 yard agility race is a simple measure of an athlete’s ability to accelerate, decelerate, change direction, and to accelerate again.") + "\n\n" +

				"<b>" + Catalog.GetString("Description") + " / " + Catalog.GetString("Procedure") + "</b>: " +
			       Catalog.GetString("Set up three marker cones in a straight line, exactly five yards apart - cones B, A(center) and C. At each cone place a line across using marking tape. The timer is positioned at the level of the center A cone, facing the athlete. The athlete straddles the center cone A with feet an equal distance apart and parallel to the line of cones. When ready, the athlete runs to cone B (touching the line with either foot), turns and accelerates to cone C (touching the line), and finishes by accelerating through the line at cone A. The stopwatch is started on the first movement of the athlete and stops the watch when the athlete’s torso crosses the center line.") + "\n\n" + 

				"<b>" + Catalog.GetString("Scoring") + "</b>: " + 
				Catalog.GetString("Record the best time of two trials.") + "\n\n" +
				"<b>" + Catalog.GetString("Comments") + "</b>: " + 
				Catalog.GetString("Encourage athletes to accelerate through the finish line to maximize their result.") + "\n\n" +
				"http://www.topendsports.com/testing/tests/20yard-agility.htm" + "\n" +
	    			Catalog.GetString("Cited with permission.");
		} 
		else if(name == "Agility-505") {
			hasIntervals 	= false; 
			distance 	= 10;
			tracksLimited 	= false;
			fixedValue 	= 0;
			imageFileName = "agility_505.png";
			description	= Catalog.GetString("505 Agility test");
			longDescription	= 
				"<b>" + Catalog.GetString("Procedure") + "</b>: " +
				Catalog.GetString("Markers are set up 5 and 15 meters from a line marked on the ground. The athlete runs from the 15 meter marker towards the line (run in distance to build up speed) and through the 5 m markers, turns on the line and runs back through the 5 m markers. The time is recorded from when the athletes first runs through the 5 meter marker, and stopped when they return through these markers (that is, the time taken to cover the 5 m up and back distance - 10 m total). The best of two trails is recorded. The turning ability on each leg should be tested. The subject should be encouraged to not overstep the line by too much, as this will increase their time.") + "\n\n" +
				
				"<b>" + Catalog.GetString("Comments") + "</b>: " + 
				Catalog.GetString("This is a test of 180 degree turning ability. This ability may not be applicable to some sports.") + "\n\n" +
				"http://www.topendsports.com/testing/tests/505.htm" + "\n" +
	    			Catalog.GetString("Cited with permission.");
		} 
		else if(name == "Agility-Illinois") {
			hasIntervals 	= false; 
			distance 	= 60;
			tracksLimited 	= false;
			fixedValue 	= 0;
			imageFileName = "agility_illinois.png";
			description	= Catalog.GetString("Illinois Agility test");
			longDescription = 
				"<b>" + Catalog.GetString("Description") + "</b>: " +
				Catalog.GetString("The length of the course is 10 meters and the width (distance between the start and finish points) is 5 meters. Four cones are used to mark the start, finish and the two turning points. Another four cones are placed down the center an equal distance apart. Each cone in the center is spaced 3.3 meters apart.") + "\n\n" +
				
				"<b>" + Catalog.GetString("Procedure") + "</b>: " +
				Catalog.GetString("Subjects should lie on their front (head to the start line) and hands by their shoulders. On the 'Go' command the stopwatch is started, and the athlete gets up as quickly as possible and runs around the course in the direction indicated, without knocking the cones over, to the finish line, at which the timing is stopped.") + "\n\n" +
				
				"<b>" + Catalog.GetString("Results") + "</b>: " +
				Catalog.GetString("The table below gives some rating scores (in seconds) for the test") + "\n" +
				"<b>" + Catalog.GetString("Rating") + "</b>\t<b>" + Catalog.GetString("Males") + "</b>\t<b>" + Catalog.GetString("Females") + "</b>\n" +
				Catalog.GetString("Excellent") + "\t" + "&lt; 15.2" + "\t" + "&lt; 17.0" + "\n" +
				Catalog.GetString("Good") + "    \t" + "16.1-15.2" + "\t" + "17.9-17.0" + "\n" +
				Catalog.GetString("Average") + "\t" + "18.1-16.2" + "\t" + "21.7-18.0" + "\n" +
				Catalog.GetString("Fair") + "    \t" + "18.3-18.2" + "\t" + "23.0-21.8" + "\n" +
				Catalog.GetString("Poor") + "    \t" + "&gt; 18.3" + "\t" + "&gt; 23.0" + "\n" + "\n" +

				"<b>" + Catalog.GetString("Advantages") + "</b>: " +
				Catalog.GetString("This is a simple test to administer, requiring little equipment. Can test players ability to turn in different directions, and different angles.") + "\n\n" + 
				
				"<b>" + Catalog.GetString("Disadvantages") + "</b>: " +
				Catalog.GetString("Choice of footwear and surface of area can effect times greatly. Results can be subject to timing inconsistencies, which may be overcome by using timing gates. Cannot distinguish between left and right turning ability.") + "\n\n" +
				
				"<b>" + Catalog.GetString("Variations") + "</b>: " +
				Catalog.GetString("The starting and finishing sides can be swapped, so that turning direction is changed.") + "\n\n" +
				"http://www.topendsports.com/testing/tests/illinois.htm" + "\n" +
	    			Catalog.GetString("Cited with permission.");

		} 
		else if(name == "Agility-Shuttle-Run") {
			hasIntervals 	= false; 
			distance 	= 40;
			tracksLimited 	= false;
			fixedValue 	= 0;
			imageFileName = "agility_shuttle.png";
			description	= Catalog.GetString("Shuttle Run Agility test");
			longDescription = 
				Catalog.GetString("This test describes the procedures as used in the President's Challenge Fitness Awards. The variations listed give other ways to also perform this test.") + "\n\n" +

				"<b>" + Catalog.GetString("Purpose") + "</b>: " +
				Catalog.GetString("This is a test of speed and agility, important in many sports.") + "\n\n" +

				"<b>" + Catalog.GetString("Description") + " / " + Catalog.GetString("Procedure") + "</b>: " +
				Catalog.GetString("This test requires the person to run back and forth between two parallel lines as fast as possible. Set up two lines of cones 30 feet apart or use line markings, and place two blocks of wood or a similar object behind one of the lines. Starting at the line opposite the blocks, on the signal 'Ready? Go!' the participant runs to the other line, picks up a block and returns to place it behind the starting line, then returns to pick up the second block, then runs with it back across the line.") + "\n\n" +


				"<b>" + Catalog.GetString("Scoring") + "</b>: " + 
				Catalog.GetString("Two or more trails may be performed, and the quickest time is recorded. Results are recorded to the nearest tenth of a second.") + "\n\n" +

				"<b>" + Catalog.GetString("Variations") + " / " + Catalog.GetString("Modifications") + "</b>: " +
				Catalog.GetString("The test procedure can be varied by changing the number of shuttles performed, the distance between turns (some use 10 meters rather than 30 feet) and by removing the need for the person pick up and return objects from the turning points.") + "\n\n" +


				"<b>" + Catalog.GetString("Advantages") + "</b>: " +
				Catalog.GetString("This test can be conducted on large groups relatively quickly with minimal equipment required.") + "\n\n" +
				
				"<b>" + Catalog.GetString("Comments") + "</b>: " + 
				Catalog.GetString("The blocks should be placed at the line, not thrown across them. Also make sure the participants run through the finish line to maximize their score.") + "\n\n" + 
				"http://www.topendsports.com/testing/tests/shuttle.htm" + "\n" +
	    			Catalog.GetString("Cited with permission.");

		} 
		else if(name == "Agility-ZigZag") {
			hasIntervals 	= false; 
			distance 	= 17.6;
			tracksLimited 	= false;
			fixedValue 	= 0;
			imageFileName = "agility_zigzag.png";
			description	= Catalog.GetString("ZigZag Agility test");
			longDescription =
				"<b>" + Catalog.GetString("Description") + " / " + Catalog.GetString("Procedure") + "</b>: " +
				Catalog.GetString("Similar to the Shuttle Run test, this test requires the athlete to run a course in the shortest possible time. A standard zig zag course is with four cones placed on the corners of a rectangle 10 by 16 feet, with one more cone placed in the centre. If the cones are labelled 1 to 4 around the rectangle going along the longer side first, and the centre cone is C, the test begins at 1, then to C, 2, 3, C, 4, then back to 1.") + "\n\n" + 

				"<b>" + Catalog.GetString("Modifications") + "</b>: " + 
				Catalog.GetString("This test procedure can be modified by changing the distance between cones, and the number of circuits performed.") + "\n\n" + 

				"<b>" + Catalog.GetString("Comments") + "</b>: " + 
				Catalog.GetString("The total distance run should not be too great so that fatigue does not become a factor.")  + "\n\n" + 

				"http://www.topendsports.com/testing/tests/zigzag.htm" + "\n" +
	    			Catalog.GetString("Cited with permission.");
		} 
		else if(name == "Agility-T-Test") {
			hasIntervals 	= false; 
			distance 	= 36;
			tracksLimited 	= false;
			fixedValue 	= 0;
			imageFileName = "agility_t_test.png";
			description	= "T Test";
			longDescription = "";
		} 
		else if(name == "Margaria") {
			hasIntervals 	= false; 
			distance 	= 0; //0:will ask user... refered to the vertical distance between third and nineth stair
			tracksLimited 	= false;
			fixedValue 	= 0;
			imageFileName = "margaria.png";
			description	= Catalog.GetString("Margaria-Kalamen");
			longDescription	= ""; //TODO
		} //interval
		else if(name == "byLaps") {
			hasIntervals 	= true; 
			distance 	= 0;
			tracksLimited 	= true;
			fixedValue 	= 0;
			description	= Catalog.GetString("Run n laps x distance");
			imageFileName = "run_interval.png";
		} else if(name == "byTime") {
			hasIntervals 	= true; 
			distance 	= 0;
			tracksLimited 	= false;
			fixedValue 	= 0;
			description	= Catalog.GetString("Make max laps in n seconds");
			imageFileName = "run_interval.png";
		} else if(name == "unlimited") {
			hasIntervals 	= true; 
			distance 	= 0;
			tracksLimited 	= false;	//limited by time
			fixedValue 	= 0;
			unlimited 	= true;
			description	= Catalog.GetString("Continue running in n distance");
			imageFileName = "run_interval.png";
		} else if(name == "20m10times") {
			hasIntervals 	= true; 
			distance 	= 20;
			tracksLimited 	= true;
			fixedValue 	= 10;
			description	= Catalog.GetString("Run 10 times a 20m distance");
			imageFileName = "run_interval.png";
		} else if(name == "7m30seconds") {
			hasIntervals 	= true; 
			distance 	= 7;
			tracksLimited 	= false;
			fixedValue 	= 30;
			description	= Catalog.GetString("Make max laps in 30 seconds");
			imageFileName = "run_interval.png";
		} else if(name == "20m endurance") {
			hasIntervals 	= true; 
			distance 	= 20;
			tracksLimited 	= false;
			fixedValue 	= 0;
			unlimited 	= true;
			description	= Catalog.GetString("Continue running in 20m distance");
			imageFileName = "run_interval.png";
		} else if(name == "MTGUG") {
			hasIntervals 	= true; 
			distance 	= -1;
			tracksLimited 	= true;
			fixedValue 	= 3;
			unlimited 	= false;
			description	= Catalog.GetString("Modified time Getup and Go test");
			imageFileName = "mtgug.png";
			distancesString = "1-7-19";	//this intervallic race has different distance for each lap
			longDescription = Catalog.GetString("The instructions given to perform the test were as follows: \"Sit down with your back resting on the back of the chair and with your two arms resting on your legs. When you hear the word 'go', stand up without using your arms, kick the ball in front of you as hard as you possibly can, using the instep of the foot you feel the safest. Then walk at your normal pace while counting backwards from 15 to 0 out loud. Turn around back the cone, without touching it, and go back to your seat, stepping into the circles, trying not to touch any of them. Finally, sit down again, trying not to use your arms\".") +
				"\n\n" +
				Catalog.GetString("The stopwatches were activated on the word 'go' and the button that saved the time intervals was pressed also after the following stages: when the subject stood up and kicked the ball; when the ball passed the 8 m line; and when the subject returned to the seated position in the same chair (42 cm height from the seat to the ground). The total time needed to perform the test provided a quantitative evaluation of performance. A qualitative evaluation was performed by the completion of an AQ. This AQ assesses 6 items with a Likert scale from 0 to 3, where 0 is the equivalent to needing help in order to perform the task, and 3 is equivalent to performing the task unaided with no mistakes. The maximum points that can be attained are 18. The items assessed were: (1) standing up from the chair, (2) kicking the ball, (3) walking whilst counting backwards from 15 to 0, (4) walking around the cone, (5) walking whilst stepping into the circles, and (6) sitting back down again.") + "\n\n" + 
				"<b>" + Catalog.GetString("Assessment questionnaire") + "</b>\n" +
				Catalog.GetString("Once the test finishes proceed to edit and you will be able to complete the assessment questionnaire.") +
				"\n\n" +
				"<b>" + Catalog.GetString("Reference:") + "</b>\n" + 
				"\nGiné-Garriga, M., Guerra, M., Marí-Dell’Olmo, M., Martin, C., & Unnithan, V.B. (2008). Sensitivity of a modified version of the ‘Timed Get Up and Go’ Test to predict fall risk in the elderly: a pilot study. Archives of Gerontology and Geriatrics, doi:10.1016/j.archger.2008.08.014. \n" +
				"<b>" + Catalog.GetString("Abstract:") + "</b>\n" +
				"http://linkinghub.elsevier.com/retrieve/pii/S0167494308001763";

		} else if(name == "Agility-3L3R") {
			hasIntervals 	= true; 
			distance 	= -1;
			tracksLimited 	= true;
			fixedValue 	= 2;
			unlimited 	= false;
			description	= Catalog.GetString("Turn left three times and turn right three times");
			imageFileName = "agility_3l3r.png";
			distancesString = "24.14-24.14";	//this intervallic run has different distance for each track
			longDescription = "";
		} else if(name == "RSA 8-4-R3-5") { 	//this is not used anymore (it was just a test)
			hasIntervals 	= true; 
			distance 	= -1;
			tracksLimited 	= true;
			fixedValue 	= 4;
			unlimited 	= false;
			description	= "RSA testing";
			imageFileName = "run_interval.png";
			distancesString = "8-4-R3-5";	//this intervallic run has different distance for each track
		} else if(
				name == "RSA Aziz 2000 40, R30 x 8" ||
				name == "RSA Balsom 15, R30 x 40" ||
				name == "RSA Balsom 30, R30 x 20" ||
				name == "RSA Balsom 40, R30 x 15" ||
				name == "RSA Dawson 40, R24 x 6" ||
				name == "RSA Fitzsimons 40, R24 x 6" ||
				name == "RSA Gaitanos 6, R30 x 10" ||
				name == "RSA Hamilton 6, R30 x 10" ||
				name == "RSA RAST 35, R10 x 6" ||
				name == "RSA Mujica 15, R24 x 6" ||
				name == "RSA Wadley 20, R17 x 12" ||
				name == "RSA Wragg 34.2, R25 x 7")
		{
			hasIntervals 	= true;
			distance 	= -1;
			tracksLimited 	= true;
			unlimited 	= false;

			if(name == "RSA Aziz 2000 40, R30 x 8") {
				fixedValue 	= 16;
				distancesString = "40-R30";
			}
			else if (name == "RSA Balsom 15, R30 x 40") {
				fixedValue 	= 80;
				distancesString = "15-R30";
			}
			else if (name == "RSA Balsom 30, R30 x 20") {
				fixedValue 	= 40;
				distancesString = "30-R30";
			}
			else if (name == "RSA Balsom 40, R30 x 15") {
				fixedValue 	= 30;
				distancesString = "40-R30";
			}
			else if (name == "RSA Dawson 40, R24 x 6") {
				fixedValue 	= 12;
				distancesString = "40-R24";
			}
			else if (name == "RSA Fitzsimons 40, R24 x 6") {
				fixedValue 	= 12;
				distancesString = "40-R24";
			}
			else if (name == "RSA Gaitanos 6, R30 x 10") {
				fixedValue 	= 20;
				distancesString = "6-R30";
			}
			else if (name == "RSA Hamilton 6, R30 x 10") {
				fixedValue 	= 20;
				distancesString = "6-R30";
			}
			else if (name == "RSA RAST 35, R10 x 6") {
				fixedValue 	= 12;
				distancesString = "35-R10";
			}
			else if (name == "RSA Mujica 15, R24 x 6") {
				fixedValue 	= 12;
				distancesString = "15-R24";
			}
			else if (name == "RSA Wadley 20, R17 x 12") {
				fixedValue 	= 24;
				distancesString = "20-R17";
			}
			else if (name == "RSA Wragg 34.2, R25 x 7") {
				fixedValue 	= 14;
				distancesString = "34.2-R25";
			}
		}

		isPredefined = FindIfIsPredefined();
	}
	
	
	public RunType(string name, bool hasIntervals, double distance, 
			bool tracksLimited, int fixedValue, bool unlimited, string description, string distancesString, string imageFileName)
	{
		type = Types.RUN;
		this.name 	= name;
		this.hasIntervals 	= hasIntervals;
		this.distance 	= distance;
		this.tracksLimited = tracksLimited;
		this.fixedValue = fixedValue;
		this.unlimited = unlimited;
		this.description = description;
		this.distancesString = distancesString;
		this.imageFileName = imageFileName;
		
		isPredefined = FindIfIsPredefined();
	}
	
	//used to select a runType at Sqlite.convertTables
	public RunType(string [] str, bool interval)
	{
		if(interval) {
			this.uniqueID = Convert.ToInt32(str[0]);
			this.name = str[1];
			this.distance = Convert.ToDouble(Util.ChangeDecimalSeparator(str[2]));
			this.tracksLimited = Util.IntToBool(Convert.ToInt32(str[3]));
			this.fixedValue = Convert.ToInt32(str[4]);
			this.unlimited = Util.IntToBool(Convert.ToInt32(str[5]));
			this.description = str[6].ToString();
		} else {
			this.uniqueID = Convert.ToInt32(str[0]);
			this.name = str[1];
			this.distance = Convert.ToDouble(Util.ChangeDecimalSeparator(str[2]));
			this.description = str[3].ToString();
		}
	}


	//used by Sqlite.convertTables
	//public override int InsertAtDB (bool dbconOpened, string tableName, bool interval) {
	public int InsertAtDB (bool dbconOpened, string tableName, bool interval) {
		if(interval)
			/*
			return SqliteRunIntervalType.Insert(dbconOpened, tableName,
					name, distance, tracksLimited, fixedValue,
					unlimited, description);
					*/
			return SqliteRunIntervalType.Insert(this, tableName, dbconOpened);
		else
			/*
			return SqliteRunType.Insert(dbconOpened, tableName, 
					name, distance, description);
					*/
			return SqliteRunType.Insert(this, tableName, dbconOpened);
	}

	public override string ToString() {
		if(hasIntervals)
			return string.Format("RunType: Interval, uniqueID: {0}, name: {1}, distance: {2}, " +
					"tracksLimited: {3}, fixedValue: {4}, unlimited: {5}, description: {6}",
					uniqueID, name, distance,
					tracksLimited, fixedValue, unlimited, description);
		else
			return string.Format("RunType: Simple, uniqueID: {0}, name: {1}, " +
					"distance: {2}, description: {3}",
					uniqueID, name, distance, description);
	}

	public double Distance
	{
		get {
			/*
			if(isPredefined) {
				return distance; 
			} else {
				return SqliteRunType.Distance(name);
			}
			*/
				return distance; 
		}
		set { distance = value; }
	}
	
	public bool HasIntervals {
		get { return hasIntervals; }
		set { hasIntervals = value; }
	}
	
	public bool TracksLimited {
		get { return tracksLimited; }
		set { tracksLimited = value; }
	}
	
	public int FixedValue {
		get { return fixedValue; }
		set { fixedValue = value; }
	}
	
	public bool Unlimited {
		get { return unlimited; }
		set { unlimited = value; }
	}

	public string DistancesString {
		get { return distancesString; }
		set { distancesString = value; }
	}

	// converts "5-R24" into "5m" for displaying
	// converts "5-5-10" into "5m 5m 10m" for displaying
	public static string DistancesStringAsMeters (string distancesString)
	{
		if (distancesString.Contains ("R")) //RSA
			return Util.RemoveFromChar (distancesString, '-') + "m";
		else
			return Util.ChangeChars (distancesString, "-", "m ") + "m";
	}

	public bool IsRSA {
		get { return distancesString.LastIndexOf("R") != -1; }
	}
}

