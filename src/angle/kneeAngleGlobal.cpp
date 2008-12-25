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
 * Initally coded by (v.1.0):
 * Sharad Shankar & Onkar Nath Mishra
 * http://www.logicbrick.com/
 * 
 * Updated by:
 * Xavier de Blas 
 * xaviblas@gmail.com
 *
 *
 */

//config variables
bool showContour = true;
bool debug = false;
int playDelay = 10; //milliseconds between photogrammes wen playing. Used as a waitkey.
//not put values lower than 5 or the enter when executing will be the first pause
//eg: 5 (fast) 1000 (one second each photogramme)
//int playDelayFoundAngle = 150; //as above, but used when angle is found.
int playDelayFoundAngle = 50; //as above, but used when angle is found.
//Useful to see better the detected angle when something is detected
//recommended values: 50 - 200



/* recommended:
	   showAtLinesPoints = true
	   ...DiffPoints = true
	   ...SamePoints = true
	   ...OnlyStartMinEnd = true;
	   */

bool showStickThePoints = true;
bool showStickTheLinesBetweenSamePoints = true;
bool showStickTheLinesBetweenDifferentPoints = true;
bool showStickOnlyStartMinEnd = true;
bool mixStickWithMinAngleWindow = true;

int startAt = 1;


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

//used on menu gui and programMode
//currently validation and blackWithoutMarkers are synonymous (until statistical anylisys is not done)
/*
 * validation uses markers and black pants to try to find relationship between both
 * blackWithoutMarkers uses only black pants and finds the place where the markers should be
 *    (when validation study for lots of people isdone)
 * skinOnlyMarkers uses markers to find three points and angle (easiest)
 */
enum { quit = -2, undefined = -1, validation = 0, blackWithoutMarkers = 1, skinOnlyMarkers = 2}; 

//used on gui
enum { 
	QUIT = -2,
	UNDEFINED = -1, 
	YES = 0, NO = 1, NEVER = 2, 
	PLAYPAUSE = 3, FORWARD = 4, FASTFORWARD = 5, BACKWARD = 6,
	HIPMARK = 7, KNEEMARK = 8, TOEMARK = 9, ZOOM = 10,
	THIPMORE = 11, THIPLESS = 12, 
	TKNEEMORE = 13, TKNEELESS = 14, 
	TTOEMORE = 15, TTOELESS = 16, 
	TGLOBALMORE = 17, TGLOBALLESS = 18
}; 

enum { TOGGLENOTHING = -1, TOGGLEHIP = 0, TOGGLEKNEE = 1, TOGGLETOE = 2};

CvPoint markedMouse;
int forceMouseMark;
int mouseClicked = undefined;
bool mouseMultiplier = false; //using shift key

bool zoomed = false;
double zoomScale = 2; 


