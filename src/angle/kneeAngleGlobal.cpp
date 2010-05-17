/*
 * This file is part of ChronoJump
 *
 * Chronojump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * Chronojump is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 *    GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * Copyright (C) 2008   Sharad Shankar & Onkar Nath Mishra http://www.logicbrick.com/
 * Copyright (C) 2008-2010   Xavier de Blas <xaviblas@gmail.com> 
 *
 */


//-------------------- global config variables. Change values at kneeAngleOptions.txt ------------
bool ShowContour;
bool Debug;
bool UsePrediction;	//unneded at 300 fps
int PlayDelay; //milliseconds between photogrammes wen playing. Used as a waitkey.
//not put values lower than 5 or the enter when executing will be the first pause
//eg: 5 (fast) 1000 (one second each photogramme)
int PlayDelayFoundAngle; //as above, but used when angle is found.
//Useful to see better the detected angle when something is detected
//recommended values: 50 - 200

double ZoomScale; 

//--------------------- global constants (enums) ----------------------------------------------


CvScalar WHITE = CV_RGB(255,255,255);
CvScalar BLACK = CV_RGB(0 ,0 , 0);
CvScalar RED = CV_RGB(255, 0, 0);
CvScalar GREEN = CV_RGB(0 ,255, 0);
CvScalar BLUE = CV_RGB(0 ,0 ,255);
CvScalar GREY = CV_RGB(128,128,128);
CvScalar YELLOW = CV_RGB(255,255, 0);
CvScalar MAGENTA = CV_RGB(255, 0,255);
CvScalar CYAN = CV_RGB( 0,255,255); 
CvScalar LIGHT = CV_RGB( 247,247,247); 

enum { SMALL = 1, MID = 2, BIG = 3 }; 

//used on menu gui and ProgramMode
//currently validation and blackWithoutMarkers are synonymous (until statistical anylisys is not done)
/*
 * validation uses markers and black pants to try to find relationship between both
 * blackWithoutMarkers uses only black pants and finds the place where the markers should be
 *    (when validation study for lots of people isdone)
 * skinOnlyMarkers uses markers to find three points and angle (easiest)
 */
//NOTE: if this changes, change also in kneeangle.cpp menu
enum { quit = -1, undefined = 0, validation = 1, blackWithoutMarkers = 2, skinOnlyMarkers = 3}; 

//used on gui
enum { 
	QUIT = -1,
	UNDEFINED = 0, 
	YES = 1, NO = 2, NEVER = 3, 
	PLAYPAUSE = 4, FORWARDONE = 5, FORWARD = 6, FASTFORWARD = 7, BACKWARD = 8,
	HIPMARK = 9, KNEEMARK = 10, TOEMARK = 11, ZOOM = 12,
	THIPMORE = 13, THIPLESS = 14, 
	TKNEEMORE = 15, TKNEELESS = 16, 
	TTOEMORE = 17, TTOELESS = 18, 
	TGLOBALMORE = 19, TGLOBALLESS = 20,
	SHIPMORE = 21, SHIPLESS = 22, 
	SKNEEMORE = 23, SKNEELESS = 24, 
	STOEMORE = 25, STOELESS = 26,
	TCONTOURMORE = 27, TCONTOURLESS = 28,
	BACKTOCONTOUR = 29,
	MINIMUMFRAMEVIEW = 30, MINIMUMFRAMEDELETE = 31
}; 

enum { TOGGLENOTHING = -1, TOGGLEHIP = 0, TOGGLEKNEE = 1, TOGGLETOE = 2};

//--------------------- global variables that change in program execution --------------------- 

int ProgramMode;

int StartAt = 1;

//black only markers will try to use contour
//and controls will be only threshold + -
//but if there's any problem with contour or the toe or hip go outside contour,
//then UsingContour will be false and it will be used same method than skinOnlyMarkers
//interface will change also
//difference with skinOnlyMarkers is that user can return to: UsingContour and play with threshold
//if is not successuful (three points are not found in contour) the UsingContour will be false again
bool UsingContour;

CvPoint MarkedMouse;
int ForceMouseMark;
int MouseClicked = undefined;
bool MouseMultiplier = false; //using shift key
bool MouseControl = false; //using CTRL key

bool Zoomed = false;

//predictions and other R statistics using
RInside R = RInside();		// create an embedded R instance 

