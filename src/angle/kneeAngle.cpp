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
 * version: 1.3 (Oct, 3, 2008)
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
 *   below: the foot preferrably has not to be shown
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
 * Now the call to the function *FindHipPoint* find the x coordinate of the pixel fulfilling
 * following criteria in the image containing maximum bounding rectangle :
 * 
 * 1.white pixel with minimum x coordinate
 * This gives the coordinate of the Hip of the person.
 *
 * Now the call to the function *FindKneePoint* takes as argument the bounding rectangle
 * of the largest  connected contours and the y coordinate of the hippoint calculated in the *FindHipPoint*.
 * The function returns the x coordinate of the white pixel having maximum x coordinate below the hip point. 
 * The white pixel with largest x coordinate below the hip point gives the coordinate of the Knee point of the leg.
 *
 * Now the call to the function *FindToePoint* searches for the white pixels below the knee point
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



int main(int argc,char **argv)
{
	if(argc < 2)
	{
		cout<<"Provide file location as a first argument..."<<endl;
		exit(1);
	}
	CvCapture* capture = NULL;
	capture = cvCaptureFromAVI(argv[1]);
	if(!capture)
	{
		exit(0);
	}

	int framesNumber = cvGetCaptureProperty(capture, CV_CAP_PROP_FRAME_COUNT);
	printf("--%d--\n", framesNumber);

	IplImage *frame=0,*frame_copy=0,*gray=0,*segmented=0,*edge=0,*temp=0,*output=0;
	IplImage *segmentedValidationHoles=0;
	IplImage *foundHoles=0;
	//IplImage *foundHolesContour=0;
	IplImage *result=0;
	IplImage *resultStick=0;
	int minwidth = 0;
	
	bool foundAngle = false; //found angle on current frame
	bool foundAngleOneTime = false; //found angle at least one time on the video

	double knee2Hip,knee2Foot,hip2Foot;
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
	int footPointWidth;
		
	//to make lines at resultPointsLines
	CvPoint notFoundPoint;
	notFoundPoint.x = 0; notFoundPoint.y = 0;
	int lowestAngleFrame = 0;


	char *label = new char[30];
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
	CvSeq* footSeq = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), stickStorage );


	while(!shouldEnd) 
	{

		frame = cvQueryFrame(capture);
		if(!frame)
			break;
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

		//created image like the contour but with holes and more (stores on segmentedValidationHoles)
		cvThreshold(gray,segmentedValidationHoles,30,255,CV_THRESH_BINARY_INV);

		//create the largest contour image (stored on temp)
		cvThreshold(gray,segmented,30,255,CV_THRESH_BINARY_INV);
		CvRect maxrect = findLargestContour(segmented,output, showContour);


		//search in output all the black places (pants) and 
		//see if there's a hole in that pixel on segmentedValidationHoles
		CvSeq* seqHolesEnd = findHoles(output, segmentedValidationHoles, foundHoles, maxrect, frame_copy);
		CvPoint hipHolesPoint = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 0); 
		CvPoint kneeHolesPoint = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 1 ); 
		CvPoint toeHolesPoint = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 2 ); 


		CvPoint hipPoint = FindHipPoint(output,maxrect);
		CvPoint kneePoint = FindKneePoint(output,maxrect,hipPoint.y);
		CvPoint footPoint = FindToePoint(output,maxrect,kneePoint.x,kneePoint.y);

		foundAngle = false;
		if(minwidth == 0)
		{
			minwidth = kneePoint.x - hipPoint.x;
		}
		else
		{
			if((double)(kneePoint.x- hipPoint.x) > 1.15*minwidth 
					&&
					upperSimilarThanLower(hipPoint, kneePoint, footPoint)
					)
				/* get lower this 1.25 because now we use mid leg to solve the hand problem and width is lower*/
				/*1.25 again, because we use hip y again*/
				foundAngle = true;
				foundAngleOneTime = true;
		}

		//Finds angle between Hip to Knee line and Knee to Toe line
		if(foundAngle)
		{
			cvCircle(frame_copy,kneePoint,2, CV_RGB(255,0,255),1,8,0);
			
			//find width of knee, only one time and will be used for all the photogrammes
			if(kneePointWidth == -1) {
				kneePointWidth = FindWidth(output, kneePoint);
			}

			//fix kneepoint.x at the right 1/2 of the knee width
			kneePoint.x -= kneePointWidth /2;
			cvCircle(frame_copy,kneePoint,3, CV_RGB(0,255,0),1,8,0);


			//find width of foot for each photogramme
			footPointWidth = FindWidth(output, footPoint);
			cvCircle(frame_copy,footPoint,2, CV_RGB(255,0,255),1,8,0);
			
			//fix footpoint.x at the 1/2 of the foot width
			footPoint.x -= footPointWidth /2;
			cvCircle(frame_copy,footPoint,3, CV_RGB(0,0,255),1,8,0);

			
			//fix hipPoint ...
			int hipPointFirstY = hipPoint.y;
			cvCircle(frame_copy,hipPoint,2, CV_RGB(255,0,255),1,8,0);
			
			//... find at 3/2 of hip (surely under the hand) ...
			theta = findAngle(hipPoint, footPoint, kneePoint);
			hipPoint = FixHipPoint1(output, hipPoint, kneePoint, theta);
			cvCircle(frame_copy,hipPoint,2, CV_RGB(255,0,255),1,8,0);

			//... cross first hippoint with the knee-hippoint line to find real hippoint
			hipPoint = FixHipPoint2(output, hipPointFirstY, kneePoint, hipPoint);
			cvCircle(frame_copy,hipPoint,3, CV_RGB(255,0,0),1,8,0);
			

			//find flexion angle
			theta = findAngle(hipPoint, footPoint, kneePoint);
			thetaHoles = findAngle(hipHolesPoint, toeHolesPoint, kneeHolesPoint);
			
			//draw 2 main lines
			cvLine(frame_copy,kneePoint,hipPoint,CV_RGB(128,128,128),1,1);
			cvLine(frame_copy,kneePoint,footPoint,CV_RGB(128,128,128),1,1);
			
			cvSeqPush( hipSeq, &hipPoint );
			cvSeqPush( kneeSeq, &kneePoint );
			cvSeqPush( footSeq, &footPoint );

			/*draw line of knee distance between holes validation and black detection
			 */

			if(kneeHolesPoint.x > 0) 
				cvLine(frame_copy,kneeHolesPoint, kneePoint,CV_RGB(225,225,0),1,1);
			if(hipHolesPoint.x > 0) 
				cvLine(frame_copy,hipHolesPoint, hipPoint,CV_RGB(225,225,0),1,1);
			if(toeHolesPoint.x > 0) 
				cvLine(frame_copy,toeHolesPoint, footPoint,CV_RGB(225,225,0),1,1);

			/*
			 * draw perpendicular line (min distance)
			 if(kneeHolesPoint.x > 0 && hipHolesPoint > 0) { 

			 CvPoint hipHolesPrima;
			 hipHolesPrima.x = hipHolesPoint.x - kneeHolesPoint.x; 
			 hipHolesPrima.y = hipHolesPoint.y - kneeHolesPoint.y; 

			 CvPoint hipPrima;
			 hipPrima.x = hipPoint.x - kneeHolesPoint.x; 
			 hipPrima.y = hipPoint.y - kneeHolesPoint.y; 

			//find angle on knee	
			int hip2KneeHoles = getDistance(hipPoint, kneeHolesPoint);
			int hipHoles2KneeHoles = getDistance(hipHolesPoint, kneeHolesPoint);
			double theta2 = (180.0/M_PI)*acos(((hipHolesPrima.x*hipPrima.x + hipHolesPrima.y*hipPrima.y))/(double)(hip2KneeHoles*hipHoles2KneeHoles));
			int d1 = hip2KneeHoles * sin((M_PI/180.0)*theta2);
			int d2 = hip2KneeHoles * cos((M_PI/180.0)*theta2);


			//angle of kneeHoles-hipHoles with horizontal
			double theta3 = (180.0/M_PI)*acos(((hipHolesPrima.x*hipHolesPrima.x + hipHolesPrima.y*0))/(double)(hipHoles2KneeHoles*hipHolesPrima.x));
			CvPoint crossPrima;
			crossPrima.y = hipHoles2KneeHoles * sin((M_PI/180.0)*theta3);
			crossPrima.x = hipHoles2KneeHoles * cos((M_PI/180.0)*theta3);
			CvPoint cross;
			cross.y = kneeHolesPoint.y + crossPrima.y;
			cross.x = kneeHolesPoint.x + crossPrima.x;
			cvCircle(frame_copy,cross,3, CV_RGB(128,128,128),1,8,0);




			sprintf(label, "a3: %f", theta3);
			cvPutText(frame_copy, label, 
			cvPoint((hipHolesPoint.x+hipPoint.x)/2, 40),
			&font,cvScalar(0,0,255));

			sprintf(label, "h2kh: %d, hh2kh: %d", hip2KneeHoles, hipHoles2KneeHoles);
			cvPutText(frame_copy, label, 
			cvPoint((hipHolesPoint.x+hipPoint.x)/2, 20),
			&font,cvScalar(0,0,255));
			sprintf(label, "a: %f, d1: %d, d2: %d", theta2, d1, d2);
			cvPutText(frame_copy, label, 
			cvPoint((hipHolesPoint.x+hipPoint.x)/2, (hipHolesPoint.y+hipPoint.y)/2),
			&font,cvScalar(0,0,255));
			}
			*/




			//Finds the minimum angle between Hip to Knee line and Knee to Toe line
			if(thetaHoles < minThetaHoles)
			{
				minThetaHoles = thetaHoles;
			}
			if(theta < minTheta)
			{
				minTheta = theta;
				cvCopy(frame_copy,result);
				lowestAngleFrame = hipSeq->total -1;
			}


			cvRectangle(frame_copy,
					cvPoint(maxrect.x,maxrect.y),
					cvPoint(maxrect.x + maxrect.width, maxrect.y + maxrect.height),
					CV_RGB(255,0,0),1,1);

			//print angles
			sprintf(label, "        black  holes  diff diff(%)");
			cvPutText(frame_copy, label, cvPoint(5,frame->height *.7),&font,cvScalar(0,0,255));
			sprintf(label, "current: %.1f %.1f %.1f %.1f", theta, thetaHoles, 
					thetaHoles-theta, relError(theta, thetaHoles));
			cvPutText(frame_copy, label, cvPoint(5,frame->height *.7 +20),&font,cvScalar(0,0,255));
			sprintf(label, "min:     %.1f %.1f %.1f %.1f", minTheta, minThetaHoles, 
					minThetaHoles-minTheta, relError(minTheta, minThetaHoles));
			cvPutText(frame_copy, label, cvPoint(5,frame->height *.7 +40),&font,cvScalar(0,0,255));
		} else{
			cvSeqPush( hipSeq, &notFoundPoint );
			cvSeqPush( kneeSeq, &notFoundPoint );
			cvSeqPush( footSeq, &notFoundPoint );
		}
			
		cvShowImage("result",frame_copy);

		/* wait key for pause
		 * if ESC, q, Q then exit
		 */

		int myDelay = playDelay;
		if(foundAngle)
			myDelay = playDelayFoundAngle;
		
		key = (char) cvWaitKey(myDelay);
		if(key == 27 || key == 'q' || key == 'Q' ) // 'ESC'
			shouldEnd = true;
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
	
	if(foundAngleOneTime) {


		if(showStickThePoints || 
				showStickTheLinesBetweenDifferentPoints ||
				showStickTheLinesBetweenSamePoints) {
			
			//remove unfound points at end (useful to paint end point)
			CvPoint hipLast = *CV_GET_SEQ_ELEM( CvPoint, hipSeq, hipSeq->total-1);
			do {
				if(pointIsNull(hipLast)) {
					cvSeqPop( hipSeq );
					cvSeqPop( kneeSeq );
					cvSeqPop( footSeq );
				}
			
				hipLast = *CV_GET_SEQ_ELEM( CvPoint, hipSeq, hipSeq->total-1);
			} while(pointIsNull(hipLast) );

			if(mixStickWithMinAngleWindow)
				paintStick(result, lowestAngleFrame, hipSeq, kneeSeq, footSeq, 
						showStickThePoints, showStickTheLinesBetweenDifferentPoints,
						showStickTheLinesBetweenSamePoints, showStickOnlyStartMinEnd, font);
			else {
				paintStick(resultStick, lowestAngleFrame, hipSeq, kneeSeq, footSeq, 
						showStickThePoints, showStickTheLinesBetweenDifferentPoints,
						showStickTheLinesBetweenSamePoints, showStickOnlyStartMinEnd, font);
				cvNamedWindow("Stick Figure",1);
				cvShowImage("Stick Figure", resultStick);
			}
		}
		
		cvNamedWindow("Minimum Frame",1);
		sprintf(label, "        black  holes  diff diff(%)");
		cvPutText(result, label, cvPoint(5,frame->height *.7),&font,cvScalar(0,0,255));
		sprintf(label, "%.1f %.1f %.1f %.1f", minTheta, minThetaHoles, 
					minThetaHoles-minTheta, relError(minTheta, minThetaHoles));
		cvPutText(result, label, cvPoint(5,frame->height *.7 +20),&font,cvScalar(0,0,255));
		cvShowImage("Minimum Frame", result);
		
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


