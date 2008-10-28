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
 * version: 1.4 (Oct, 28, 2008)
 *
 */

/*
 * This is suitable to detect minimum angle on the flexion previous to a CMJ jump
 * check samples working:
 * http://vimeo.com/album/28658
 *
 * more info here:
 * http://mail.gnome.org/archives/chronojump-list/2008-July/msg00005.html
 */

/*
 * CONSTRAINTS:
 * 
 * -Person have to be "looking" to the right of the camera
 * -Camera view area will be (having jumper stand up):
 *   below: the toe preferrably has not to be shown
 *   above: the top part of the image will be the right hand (fully included)
 * -Black trousers should be use. Rest of the clothes should not be black.
 * -White background is highly recommended
 * -Initially user should stand straight for 1-2 frames so that some values can be set by the program. 
 *
 */


/*
 * EXPLANATION:
 *
 * The incoming image is converted into Gray form of image and a fixed low level threshold 
 * is applied to  the grayscale image.
 *
 * Now the call to the function *findLargestContour* finds the bounding rectangle
 * of the largest connected contour in the thresholded image.This function returns
 * the bounding rectangle of the largest contours and draws the largest contour on a temporay image.
 *
 * Now the call to the function *findHipPoint* find the x coordinate of the pixel fulfilling
 * following criteria in the image containing maximum bounding rectangle :
 * 
 * 1.white pixel with minimum x coordinate
 * This gives the coordinate of the Hip of the person.
 *
 * Now the call to the function *findKneePoint* takes as argument the bounding rectangle
 * of the largest  connected contours and the y coordinate of the hippoint calculated in the *findHipPoint*.
 * The function returns the x coordinate of the white pixel having maximum x coordinate below the hip point. 
 * The white pixel with largest x coordinate below the hip point gives the coordinate of the Knee point of the leg.
 *
 * Now the call to the function *findToePoint* searches for the white pixels below the knee point
 * and having minimum x coordinate.This gives the coordinate of the toe point.
 *
 * Minimum Angle Calculation: 
 * The x coordinate of the hip point is increased by 10 and the x coordinate of the knee point
 * and toe point is decreased by 10 to get the exact hip,knee and toe points.
 * The angle between hip to knee line and knee to toe line is calculated for all the input frames
 * and the minimum value of these angles give the minimum angle.
 */

/*
 * COMPILATION:
 *
 * g++ -lcv -lcxcore -lhighgui -L(path to opencv library) kneeAngle.cpp -o kneeAngle
 *
 * for example
 * 
 * g++ -lcv -lcxcore -lhighgui -L/usr/local/lib kneeAngle.cpp -o kneeAngle
 * 
 * command to run the file
 * ./kneeAngle "path to video file"
 *
 */

/*
 * Installing OpenCV on Ubuntu
 *
 * http://dircweb.king.ac.uk/reason/opencv_cvs.php
 */



#include "opencv/cv.h"
#include "opencv/highgui.h"
#include <opencv/cxcore.h>
#include "stdio.h"
#include "math.h"
#include<iostream>
#include<fstream>
#include<vector>
#include <string>

#include "kneeAngleUtil.cpp"
#include "kneeAngleFunctions.cpp"

using namespace std;

//config variables
bool showContour = true;
bool debug = false;
int playDelay = 5; //milliseconds between photogrammes wen playing. Used as a waitkey. 
			//not put values lower than 5 or the enter when executing will be the first pause
			//eg: 5 (fast) 1000 (one second each photogramme)
//int playDelayFoundAngle = 150; //as above, but used when angle is found. 
int playDelayFoundAngle = 50; //as above, but used when angle is found. 
				//Useful to see better the detected angle when something is detected
				//recommended values: 50 - 200

//used for validating
enum { markerColorBlue = 0, markerColorRed = 1, markerColorGreen = 2, markerColorYellow = 3 };


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

//if true, don't analyse the pants angle, only the validation points on black pants
bool onlyValidationBlack = false;

bool onlyValidationSkin = false;


int main(int argc,char **argv)
{
	if(argc < 2)
	{
		cout<<"Provide file location as a first argument...\noptional: provide start photogramme at 2nd argument."<<endl;
		exit(1);
	}

	//printf("argc: %d\n",argc);
	int startAt = -1;
	if(argc >= 3)
		startAt = atoi(argv[2]);
	if(argc == 4 && *argv[3] == 'b') {
		onlyValidationBlack = true;
		printf("only validation black!!\n");
	}
	if(argc == 4 && *argv[3] == 's') {
		onlyValidationSkin = true;
		printf("only validation skin!!\n");
	}

	CvCapture* capture = NULL;
	capture = cvCaptureFromAVI(argv[1]);
	if(!capture)
	{
		exit(0);
	}

	//int framesNumber = cvGetCaptureProperty(capture, CV_CAP_PROP_FRAME_COUNT);
	//printf("--%d--\n", framesNumber);
			
	IplImage *frame=0,*frame_copy=0,*gray=0,*segmented=0,*edge=0,*temp=0,*output=0;
	IplImage *segmentedValidationHoles=0;
	IplImage *foundHoles=0;
	//IplImage *foundHolesContour=0;
	IplImage *result=0;
	IplImage *resultStick=0;
	int minwidth = 0;
	
	bool foundAngle = false; //found angle on current frame
	bool foundAngleOneTime = false; //found angle at least one time on the video

	double knee2Hip,knee2Toe,hip2Toe;
	double theta, thetaHoles;
	string text,angle;
	double minTheta = 360;
	double minThetaHoles = 360;
	char buffer[15];
	cvNamedWindow("result",1);

//	cvNamedWindow("gray",1);//meu
	cvNamedWindow("holes",1); //meu
//	cvNamedWindow("foundHoles",1); //meu
//	cvNamedWindow("output",1);

	int kneePointWidth = -1;
	int toePointWidth = -1;
		
	//to make lines at resultPointsLines
	CvPoint notFoundPoint;
	notFoundPoint.x = 0; notFoundPoint.y = 0;
	int lowestAngleFrame = 0;


	char *label = new char[150];
	CvFont font;
	int fontLineType = CV_AA; // change it to 8 to see non-antialiased graphics
	double fontSize = .4;
	cvInitFont(&font, CV_FONT_HERSHEY_COMPLEX, fontSize, fontSize, 0.0, 1, fontLineType);
	
	char key;
	bool shouldEnd = false;

	//stick storage
	CvMemStorage* stickStorage = cvCreateMemStorage(0);
	CvSeq* hipSeq = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), stickStorage );
	CvSeq* kneeSeq = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), stickStorage );
	CvSeq* toeSeq = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), stickStorage );
  
	if(! onlyValidationBlack && ! onlyValidationSkin) {
		sprintf(label, "    fps a_black a_holes [   diff diff(%)] kneeDif  a_supD  a_infD");
		printf("%s\n" ,label);
	}

	double avgThetaDiff = 0;
	double avgThetaDiffPercent = 0;
	double avgKneeDistance = 0;
	double avgKneeBackDistance = 0;
	int framesDetected = 0;
	int framesCount = 0;

	//to advance fast and really fast
	bool forward = false;
	int forwardSpeed = 50;
	bool forwardSuper = false; 
	int forwardSuperSpeed = 200;
	int forwardCount = 0;
		
	CvPoint hipHolesPoint = pointToZero();
	CvPoint kneeHolesPoint = pointToZero();
	CvPoint toeHolesPoint = pointToZero();
	CvPoint hipOld = pointToZero();
	CvPoint kneeOld = pointToZero();
	CvPoint toeOld = pointToZero();

	int legHolesDist = 0;
	int legHolesDistMax = 0;

	int brightness = -1;

	while(!shouldEnd) 
	{
		framesCount ++;
				
		frame = cvQueryFrame(capture);
		if(!frame)
			break;
		if(startAt > framesCount) {
			continue;
		}
		if(forward || forwardSuper) {
			if(
					forward && (forwardCount < forwardSpeed) ||
					forwardSuper && (forwardCount < forwardSuperSpeed)) {
				forwardCount ++;
				continue;
			} else {
				//end of forwarding
				forwardCount = 0;
				forward = false;
				forwardSuper = false;

				//mark that we are forwarding to the holesdetection
				//then it will not need to match a point with previous point
				hipOld = pointToZero();
				kneeOld = pointToZero();
				toeOld = pointToZero();
			}
		}

		if( !frame_copy ) 
			frame_copy = cvCreateImage( cvSize(frame->width,frame->height),IPL_DEPTH_8U, frame->nChannels );
		
		if( frame->origin == IPL_ORIGIN_TL )
			cvCopy( frame, frame_copy, 0 );
		else
			cvFlip( frame, frame_copy, 0 );

		if(!gray)
		{
			gray = 		cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,1);
			segmented =	cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,1);
			segmentedValidationHoles  =	cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,1); //meu
			foundHoles  =	cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,3); //meu
			//foundHolesContour  =	cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,1); //meu
			edge =	cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,1);
			temp = cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,1);
			output = cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,1);
			result = cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,3);
			resultStick = cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,3);
		}

		cvSmooth(frame_copy,frame_copy,2,5,5);
		cvCvtColor(frame_copy,gray,CV_BGR2GRAY);

		// detect brightness
		if(brightness == -1) {
			brightness = calculateBrightness(gray);
			printf("brightness: %d\n", brightness);
		}

		//created image like the contour but with holes and more (stores on segmentedValidationHoles)
		//recommended 25,255
		//high threshold (40-60) detects more black things (useful to not confuse a hole when is close to the border)
		//low threshold (5-10) detects less black things
		//if the image is bright, a hight threshold is perfect to nice detect the shapes without detecting shadows
		//int threshold = 35;
		//put threshold min depending on brightnesss
		//eg: nora: brightness 85 -> threshold 30
		//eg: 44_lluis_puerta_salt6_m.MOV: brightness 73 -> threshold 10
	
		//adjust better, because:
		//12_carles_tejedor_salt3_m.MOV
		//is bright:0, but if put thresh to 1 then detects bad. and to 10 is ok
		//
		//on the other side, 2_roger_miralles_salt3_m.MOV
		//is really dark and it needs a thresh of 1 to work
		//
		//also it could be nice to have a thresh that detects three objects
		//another option, could be to have the thresh 10 as really minimum and darker images are unsupported!
		int threshold;
		int briMax = 85;
		int briMin = 65;
//		int briZero = 50;
		int thresMax = 30;
		int thresMin = 10;
		if(brightness >= briMax) 
			threshold = thresMax;
//		else if(brightness <= briZero)
//			threshold = 10;
		else if(brightness <= briMin)
			threshold = thresMin;
		else
			threshold = brightness - briMin + thresMin;

		int thresholdMax = 255;

		CvRect maxrect;
		int thresholdInc = 1;
			
		//at first photogram if onlyValidationSkin, ask user for the points
		//don't do like this, go to another method and have the flow there
		//also the same for onlyValidationBlack
		if(onlyValidationSkin) {
			if(hipSkin.x == 0  ) {
				cvNamedWindow( "toClick", 1 );
				cvShowImage("toClick", gray);
				cvSetMouseCallback( "toClick", on_mouse, 0 );
				printf("Please, mark hip point, then knee, then toe, and finally press any key\n"); 
				int c = cvWaitKey(0);
				printf("hip: (%d,%d), knee: (%d,%d), toe:(%d,%d)\n", 
						hipSkin.x, hipSkin.y, kneeSkin.x, kneeSkin.y, toeSkin.x, toeSkin.y);

				cvNamedWindow( "HolesBri", 1 );
				int thresholdSkin = 150;
				bool done = false;
				do {
					cvThreshold(gray,segmentedValidationHoles, thresholdSkin, thresholdMax,CV_THRESH_BINARY_INV);
					cvShowImage("HolesBri", segmentedValidationHoles);
					printf("threshold: %d ('+' increase, '-' decrease, 'q' done)\n", thresholdSkin); 
					key = (char) cvWaitKey(0);
					if(key == 'q' || key == 'Q' )
						done  = true;
					else if(key == '+')
						thresholdSkin ++;
					else if(key == '-')
						thresholdSkin --;
				} while(! done);

				//backward doesn't work:
				//cvSetCaptureProperty( capture, CV_CAP_PROP_POS_FRAMES, 600.0 );
				//cvSetCaptureProperty( capture, CV_CAP_PROP_POS_AVI_RATIO, .7 );
				//http://opencv.willowgarage.com/wiki/HighGui


			} else {
				cvThreshold(gray,segmentedValidationHoles, 150, thresholdMax,CV_THRESH_BINARY_INV);
				cvCircle(segmentedValidationHoles,hipSkin,2, CV_RGB(255,0,0),1,8,0);
				cvCircle(segmentedValidationHoles,kneeSkin,2, CV_RGB(255,0,0),1,8,0);
				cvCircle(segmentedValidationHoles,toeSkin,2, CV_RGB(255,0,0),1,8,0);
				cvShowImage("HolesBri", segmentedValidationHoles);
				cvWaitKey(10);
			}
				
			continue;			
			//exit(0);
		}

		do {
			cvThreshold(gray,segmentedValidationHoles, threshold, thresholdMax,CV_THRESH_BINARY_INV);

			//create the largest contour image (stored on temp)
			cvThreshold(gray,segmented,threshold,thresholdMax,CV_THRESH_BINARY_INV);
			maxrect = findLargestContour(segmented, output, showContour);

			//search in output all the black places (pants) and 
			//see if there's a hole in that pixel on segmentedValidationHoles
			if(onlyValidationSkin)
				;
			else {
				CvSeq* seqHolesEnd = findHoles(
						output, segmentedValidationHoles, foundHoles, frame_copy,  
						maxrect, hipOld, kneeOld, toeOld);
			
				hipHolesPoint = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 0); 
				kneeHolesPoint = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 1 ); 
				toeHolesPoint = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 2 ); 
			}
			threshold += thresholdInc;
		} while(
				(pointIsNull(hipHolesPoint) || pointIsNull(kneeHolesPoint) || pointIsNull(toeHolesPoint))
				&& threshold < 100);

		threshold -= thresholdInc;
		hipOld = hipHolesPoint;
		kneeOld = kneeHolesPoint;
		toeOld = toeHolesPoint;


		CvPoint hipPointBack;
		CvPoint hipPoint;
		CvPoint kneePointBack;
		CvPoint kneePointFront;
		CvPoint kneePoint;
		CvPoint toePoint;
		//int area;


		if( ! onlyValidationBlack) {
			hipPointBack = findHipPoint(output,maxrect);

			//provisionally ubicate hipPoint at horizontal 1/2
			CvPoint hipPoint;
			hipPoint.x = hipPointBack.x + (findWidth(output, hipPointBack, true) /2);
			hipPoint.y = hipPointBack.y;

			//knee	
			kneePointFront = findKneePointFront(output,maxrect,hipPointBack.y);
			kneePointBack = findKneePointBack(output,maxrect,hipPointBack.y, kneePointFront.x); //hueco popliteo

			//toe
			CvPoint toePoint = findToePoint(output,maxrect,kneePointFront.x,kneePointFront.y);


			foundAngle = false;
			if(minwidth == 0)
				minwidth = kneePointFront.x - hipPointBack.x;
			else {
				if((double)(kneePointFront.x- hipPointBack.x) > 1.15*minwidth 
						&&
						upperSimilarThanLower(hipPoint, kneePointFront, toePoint)
						&& !pointIsNull(hipHolesPoint) && !pointIsNull(kneeHolesPoint) && 
						!pointIsNull(toeHolesPoint)
				  )
					/* get lower this 1.25 because now we use mid leg to solve the hand problem and width is lower*/
					/*1.25 again, because we use hip y again*/
					foundAngle = true;
				foundAngleOneTime = true;
			}

			//Finds angle between Hip to Knee line and Knee to Toe line
			if(foundAngle)
			{
				// ------------ knee stuff ----------

				//find width of knee, only one time and will be used for all the photogrammes
				if(kneePointWidth == -1) 
					kneePointWidth = findWidth(output, kneePointFront, false);

				kneePoint = kneePointInNearMiddleOfFrontAndBack(
						kneePointBack, kneePointFront, kneePointWidth, frame_copy);
				cvCircle(frame_copy,kneePointBack,2, CV_RGB(128,128,128),1,8,0);
				cvCircle(frame_copy,kneePointFront,2, CV_RGB(128,128,128),1,8,0);
				cvLine(frame_copy,kneePointFront,kneePointBack,CV_RGB(128,128,128),1,1);
				cvCircle(frame_copy,kneePoint,3, CV_RGB(255,255,0),1,8,0);

				// ------------ toe stuff ----------

				/*
				 * don't find width of toe for each photogramme
				 * do it only at first, because if at any photogramme, as flexion advances, 
				 * knee pointfront is an isolate point at the right of the lowest part of the pants
				 * because the back part of kneepoint has gone up
				 */

				if(toePointWidth == -1) 
					toePointWidth = findWidth(output, toePoint, false);
				cvCircle(frame_copy,toePoint,2, CV_RGB(128,128,128),1,8,0);

				theta = findAngle(hipPoint, toePoint, kneePoint);

				//fix toepoint.x at the 1/2 of the toe width
				//depending on kneeAngle
				toePoint.x = fixToePointX(toePoint.x, toePointWidth, theta);
				cvCircle(frame_copy,toePoint,3, CV_RGB(255,0,0),1,8,0);


				// ------------ hip stuff ----------

				//fix hipPoint ...
				cvCircle(frame_copy,hipPointBack,2, CV_RGB(128,128,128),1,8,0);

				//... find at 3/2 of hip (surely under the hand) ...
				//theta = findAngle(hipPoint, toePoint, kneePoint);
				hipPoint = fixHipPoint1(output, hipPointBack.y, kneePoint, theta);
				cvCircle(frame_copy,hipPoint,2, CV_RGB(128,128,128),1,8,0);

				//... cross first hippoint with the knee-hippoint line to find real hippoint
				hipPoint = fixHipPoint2(output, hipPointBack.y, kneePoint, hipPoint);
				cvCircle(frame_copy,hipPoint,3, CV_RGB(255,0,0),1,8,0);


				// ------------ flexion angle ----------

				//find flexion angle
				theta = findAngle(hipPoint, toePoint, kneePoint);
				//double thetaBack = findAngle(hipPoint, toePoint, kneePointBack);

				//draw 2 main lines
				cvLine(frame_copy,kneePoint,hipPoint,CV_RGB(255,0,0),1,1);
				cvLine(frame_copy,kneePoint,toePoint,CV_RGB(255,0,0),1,1);

				cvSeqPush( hipSeq, &hipPoint );
				cvSeqPush( kneeSeq, &kneePoint );
				cvSeqPush( toeSeq, &toePoint );


				//find total area
				//area = findTotalArea(gray, maxrect);

				if(theta < minTheta)
				{
					minTheta = theta;
					cvCopy(frame_copy,result);
					lowestAngleFrame = framesCount;
				}
			}
			else {
				//angle not found
				cvSeqPush( hipSeq, &notFoundPoint );
				cvSeqPush( kneeSeq, &notFoundPoint );
				cvSeqPush( toeSeq, &notFoundPoint );
			}
		}


		if(pointIsNull(hipHolesPoint) || pointIsNull(kneeHolesPoint) || pointIsNull(toeHolesPoint))
				thetaHoles = -1;
		else {
			thetaHoles = findAngle(hipHolesPoint, toeHolesPoint, kneeHolesPoint);
			if(thetaHoles < minThetaHoles) 
				minThetaHoles = thetaHoles;


			cvRectangle(frame_copy,
					cvPoint(maxrect.x,maxrect.y),
					cvPoint(maxrect.x + maxrect.width, maxrect.y + maxrect.height),
					CV_RGB(255,0,0),1,1);

			//print frame variation of distances of leg
			//soon find abduction, and then real flexion angle
			legHolesDist = getDistance(hipHolesPoint, kneeHolesPoint) + getDistance(toeHolesPoint, kneeHolesPoint);
			if(legHolesDist > legHolesDistMax)
				legHolesDistMax = legHolesDist;

			sprintf(label, "legSize: %d(%.1f\%)", legHolesDist, relError(legHolesDistMax, legHolesDist));
			cvPutText(frame_copy, label, cvPoint(frame->width-150, frame->height-20),&font,CV_RGB(255,255,255));

				
			sprintf(label, "frame: %d", framesCount);
			cvPutText(frame_copy, label, cvPoint(10,frame->height-80),&font,CV_RGB(255,255,255));

			//print holes angle detection data
			sprintf(label, "threshold: %d", threshold);
			cvPutText(frame_copy, label, cvPoint(10,frame->height-60),&font,CV_RGB(255,255,255));
			sprintf(label, "H angle: %.2f", thetaHoles);
			cvPutText(frame_copy, label, cvPoint(10,frame->height-40),&font,CV_RGB(255,255,255));
			sprintf(label, "min H angle: %.2f", minThetaHoles);
			cvPutText(frame_copy, label, cvPoint(10,frame->height-20),&font,CV_RGB(255,255,255));

			if(foundAngle) {
				//print data
				double thetaSup = findAngle(hipPoint, cvPoint(0,kneePoint.y), kneePoint);
				double thetaHolesSup = findAngle(hipHolesPoint, cvPoint(0, kneeHolesPoint.y), kneeHolesPoint);

				double thetaInf = findAngle(cvPoint(0,kneePoint.y), toePoint, kneePoint);
				double thetaHolesInf = findAngle(cvPoint(0,kneeHolesPoint.y), toeHolesPoint, kneeHolesPoint);

				printf("%7d %7.2f %7.2f [%7.2f %7.2f] %7.2f %7.2f %7.2f [%7.2f %7.2f] [%7.2f] [%7.2f]\n", framesCount, theta, thetaHoles, 
						thetaHoles-theta, relError(theta, thetaHoles), 
						getDistance(kneePoint, kneeHolesPoint), 
						thetaSup-thetaHolesSup, thetaInf-thetaHolesInf
						, getDistance(kneeHolesPoint, hipHolesPoint), getDistance(kneePoint, hipPoint)
						, getDistance(kneePoint, hipPointBack)
						, getDistance(kneePointBack, hipPointBack)
						//, area
				      );

				avgThetaDiff += abs(thetaHoles-theta);
				avgThetaDiffPercent += abs(relError(theta, thetaHoles));
				avgKneeDistance += getDistance(kneePoint, kneeHolesPoint);
				framesDetected ++;
			}

			cvShowImage("result",frame_copy);


			//Finds the minimum angle between Hip to Knee line and Knee to Toe line
			if(thetaHoles == minThetaHoles) {
				if(onlyValidationBlack) {
					/*
					 * if only process validation points, then minimum angle should be obtained 
					 * by the validation points
					 */
				
					cvCopy(frame_copy,result);
					//lowestAngleFrame = hipSeq->total -1;
					lowestAngleFrame = framesCount;
				}
			}

			//exit if we are going up and soon jumping.
			//toe will be lost
			//detected if minThetaHoles is littler than thetaHoles, when thetaHoles is big
			if(thetaHoles > 140 && minThetaHoles +10 < thetaHoles)
				shouldEnd = true;
		}


		/* wait key for pause
		 * if ESC, q, Q then exit
		 */

		int myDelay = playDelay;
		if(foundAngle)
			myDelay = playDelayFoundAngle;

		key = (char) cvWaitKey(myDelay);
		if(key == 27 || key == 'q' || key == 'Q' ) // 'ESC'
			shouldEnd = true;
		else if(key == 'f') // 'FORWARD'
			forward = true;
		else if(key =='F') // 'FORWARD SUPER'
			forwardSuper = true;
		else if (key >0)
		{
			//if paused, print "pause"
			sprintf(label,"Pause");
			cvPutText(frame_copy, label,cvPoint(frame->width -100,25),&font,cvScalar(0,0,255));
			cvShowImage("result",frame_copy);

			key = (char) cvWaitKey(0);
			if(key == 27 || key == 'q' || key == 'Q' ) // 'ESC'
				shouldEnd = true;
		}

		//cvShowImage("gray",gray);

		//cvShowImage("segmentedValidationHoles",segmentedValidationHoles);
		double scale = 4;
		IplImage* tempSmall = cvCreateImage( cvSize( cvRound (segmentedValidationHoles->width/scale), cvRound (segmentedValidationHoles->height/scale)), 8, 1 );
		cvResize( segmentedValidationHoles, tempSmall, CV_INTER_LINEAR );
		cvShowImage("holes",tempSmall);


		//cvShowImage("foundHoles",foundHoles);
		//cvShowImage("output",output);

	}

	if(onlyValidationBlack) {
		printf("*** Result ***\nMin angle: %.2f, lowest angle frame: %d\n", minThetaHoles, lowestAngleFrame);
		cvNamedWindow("Minimum Frame",1);
		cvShowImage("Minimum Frame", result);
		cvWaitKey(0);
	}
	else if(foundAngleOneTime) {

		avgThetaDiff = (double) avgThetaDiff / framesDetected;
		avgThetaDiffPercent = (double) avgThetaDiffPercent / framesDetected;
		avgKneeDistance = (double) avgKneeDistance / framesDetected;

		printf("\n[%f %f] %f\n", avgThetaDiff, avgThetaDiffPercent, avgKneeDistance);

		if(showStickThePoints || 
				showStickTheLinesBetweenDifferentPoints ||
				showStickTheLinesBetweenSamePoints) {

			//remove unfound points at end (useful to paint end point)
			CvPoint hipLast = *CV_GET_SEQ_ELEM( CvPoint, hipSeq, hipSeq->total-1);
			do {
				if(pointIsNull(hipLast)) {
					cvSeqPop( hipSeq );
					cvSeqPop( kneeSeq );
					cvSeqPop( toeSeq );
				}

				hipLast = *CV_GET_SEQ_ELEM( CvPoint, hipSeq, hipSeq->total-1);
			} while(pointIsNull(hipLast) );

			if(mixStickWithMinAngleWindow) {
				paintStick(result, lowestAngleFrame, hipSeq, kneeSeq, toeSeq, 
						showStickThePoints, showStickTheLinesBetweenDifferentPoints,
						showStickTheLinesBetweenSamePoints, showStickOnlyStartMinEnd, font);
				cvNamedWindow("Minimum Frame",1);
				cvShowImage("Minimum Frame", result);
			} else {
				paintStick(resultStick, lowestAngleFrame, hipSeq, kneeSeq, toeSeq, 
						showStickThePoints, showStickTheLinesBetweenDifferentPoints,
						showStickTheLinesBetweenSamePoints, showStickOnlyStartMinEnd, font);
				cvNamedWindow("Stick Figure",1);
				cvShowImage("Stick Figure", resultStick);
			}
		}

		printf("Minimum Frame\n");
		sprintf(label, "minblack minholes    diff diff(%)");
		sprintf(label, "%8.2f %8.2f [%7.2f %7.2f]", minTheta, minThetaHoles, 
				minThetaHoles-minTheta, relError(minTheta, minThetaHoles));
		printf("%s\n" ,label);

		cvWaitKey(0);
	}

	/* show all windows*/	
	/*
	   cvNamedWindow("gray",1);
	   cvShowImage("gray", gray);
	   cvNamedWindow("segmented",1);
	   cvShowImage("segmented", segmented);
	   cvNamedWindow("edge",1);
	   cvShowImage("edge", edge);
	   cvNamedWindow("temp",1);
	   cvShowImage("temp", temp);
	   cvNamedWindow("output",1);
	   cvShowImage("output", output);
	   */


	cvClearMemStorage( stickStorage );

	cvDestroyAllWindows();
	cvReleaseImage(&frame_copy);
	cvReleaseImage(&gray);
	cvReleaseImage(&segmented);
	cvReleaseImage(&segmentedValidationHoles);
	cvReleaseImage(&foundHoles);
	cvReleaseImage(&edge);
	cvReleaseImage(&temp);
	cvReleaseImage(&output);
	cvReleaseImage(&result);
	if(!mixStickWithMinAngleWindow)
		cvReleaseImage(&resultStick);
}


