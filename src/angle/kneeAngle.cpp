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
 * version: 1.5.1 (Nov, 3, 2008)
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
 * for example: 
 * 
 * Ubuntu Hoary (8.04) with opencv 1.0
 * g++ -lcv -lcxcore -lhighgui -L/usr/local/lib kneeAngle.cpp -o kneeAngle
 * 
 * Ubuntu Intrepid (8.10) with opencv 1.1
 * g++ `pkg-config --cflags opencv` kneeAngle.cpp -o kneeAngle `pkg-config --libs opencv`
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


int menu(void);

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

int startAt = 1;
enum { blackAndMarkers = 0, blackOnlyMarkers = 1, skinOnlyMarkers = 2 };

int main(int argc,char **argv)
{
	if(argc < 2)
	{
		cout<<"Provide file location as a first argument...\noptional: provide start photogramme at 2nd argument."<<endl;
		exit(1);
	}

	CvCapture* capture = NULL;
	capture = cvCaptureFromAVI(argv[1]);
	if(!capture)
	{
		exit(0);
	}
	//int framesNumber = cvGetCaptureProperty(capture, CV_CAP_PROP_FRAME_COUNT);
	//printf("--%d--\n", framesNumber);

	
	int programMode = menu();

	/* initialization variables */
	IplImage *frame=0,*frame_copy=0,*gray=0,*segmented=0,*edge=0,*temp=0,*output=0;
	IplImage *segmentedValidationHoles=0;
	IplImage *foundHoles=0;
	IplImage *result=0;
	IplImage *resultStick=0;
	int minwidth = 0;
	
	bool foundAngle = false; //found angle on current frame
	bool foundAngleOneTime = false; //found angle at least one time on the video

	double knee2Hip,knee2Toe,hip2Toe;
	double thetaExpected, thetaMarked;
	string text,angle;
	double minThetaExpected = 360;
	double minThetaMarked = 360;
	char buffer[15];

	if(programMode == skinOnlyMarkers)
		cvNamedWindow("skinOutput",1);
	else {
		cvNamedWindow("holes",1);
		cvNamedWindow("result",1);
	}
	


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
	
	bool shouldEnd = false;

	//stick storage
	CvMemStorage* stickStorage = cvCreateMemStorage(0);
	CvSeq* hipSeq = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), stickStorage );
	CvSeq* kneeSeq = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), stickStorage );
	CvSeq* toeSeq = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), stickStorage );
  
	if(programMode == blackAndMarkers) {
		sprintf(label, "    fps a_black a_holes [   diff diff(%%)] kneeDif  a_supD  a_infD");
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
		
	CvPoint hipMarked = pointToZero();
	CvPoint kneeMarked = pointToZero();
	CvPoint toeMarked = pointToZero();
	CvPoint hipOld = pointToZero();
	CvPoint kneeOld = pointToZero();
	CvPoint toeOld = pointToZero();

	int upLegMarkedDist = 0;
	int upLegMarkedDistMax = 0;
	int downLegMarkedDist = 0;
	int downLegMarkedDistMax = 0;

	int threshold;
	int thresholdMax = 255;
	int thresholdInc = 1;

	int key;
	bool jumpedFrames = false;
	bool jumping = false;


	while(!shouldEnd) 
	{
		framesCount ++;
				
		/*
		 * 1
		 * GET FRAME AND FLOW CONTROL
		 */

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
		if(jumping) {
				hipOld = pointToZero();
				kneeOld = pointToZero();
				toeOld = pointToZero();
				jumping = false;
		}


		/* 
		 * 2
		 * CREATE IMAGES
		 */


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
			edge =	cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,1);
			temp = cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,1);
			output = cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,1);
			result = cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,3);
			resultStick = cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,3);
	
			if(programMode == skinOnlyMarkers)
				threshold = 150;
			else {
				cvCvtColor(frame_copy,gray,CV_BGR2GRAY);
				threshold = calculateThresholdStart(gray);
			}
		}

		cvSmooth(frame_copy,frame_copy,2,5,5);
		cvCvtColor(frame_copy,gray,CV_BGR2GRAY);
		CvRect maxrect;
			

		/*
		 * 3 
		 * FIND THREE MARKER POINTS
		 */


		if(programMode == skinOnlyMarkers) 
		{
			cvCvtColor(frame_copy,output,CV_BGR2GRAY);
			cvThreshold(gray, output, threshold, thresholdMax,CV_THRESH_BINARY_INV);
		
			/*
			hipOld = hipMarked;
			kneeOld = kneeMarked;
			toeOld = toeMarked;
			*/
				
			CvSeq* seqHolesEnd = findHolesSkin(output, frame_copy, hipMarked, kneeMarked, toeMarked);

			hipMarked = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 0); 
			kneeMarked = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 1 ); 
			toeMarked = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 2 ); 

			cvCircle(frame_copy,hipMarked,2, CV_RGB(128,128,128),1,8,0);
			cvCircle(frame_copy,kneeMarked,2, CV_RGB(128,128,128),1,8,0);
			cvCircle(frame_copy,toeMarked,2, CV_RGB(128,128,128),1,8,0);
			
			//if frame before nothing was detected (maybe first frame or after a forward or jump
			//OR a point in this frame was not detected
			if(
					(pointIsNull(hipOld) && pointIsNull(kneeOld) && pointIsNull(toeOld)) ||
					(pointIsNull(hipMarked) || pointIsNull(kneeMarked) || pointIsNull(toeMarked)) ) 
			{
				hipMouse = hipMarked;
				kneeMouse = kneeMarked;
				toeMouse = toeMarked;

				cvNamedWindow( "toClick", 1 );
				cvShowImage("toClick", frame_copy);
				mouseCanMark = true;
				cvSetMouseCallback( "toClick", on_mouse, 0 );
				
				cvCvtColor(frame_copy,output,CV_BGR2GRAY);
				cvThreshold(gray, output, threshold, thresholdMax,CV_THRESH_BINARY_INV);
				cvShowImage("skinOutput", output);
					
				printf("\nFrame: %d H(%d,%d), K(%d,%d), T(%d,%d)\n", framesCount, hipMarked.x, hipMarked.y, 
						kneeMarked.x, kneeMarked.y, toeMarked.x, toeMarked.y);


				if(pointIsNull(hipMarked) || pointIsNull(kneeMarked) || pointIsNull(toeMarked)) {
					printf("** Please mark:");
					if(pointIsNull(hipMarked))
						printf(" HIP ");
					if(pointIsNull(kneeMarked))
						printf(" KNEE ");
					if(pointIsNull(toeMarked))
						printf(" TOE ");
					printf("on 'toClick' window **\n");
				}
				
				printf("Press 'p' when done.\n");
				printf("Optionally:\n");
				printf("\tReMark hip 'h', knee 'k', toe 't'\n"); 
				printf("\tChange threshold: %d ('+' increase, '-' decrease)\n", threshold); 
				printf("\tOthers: Zoom 'z', forward 'f', Forward 'F', jump 'j', quit program 'q'\n\n"); 

				bool done = false;
				IplImage* imgZoom;
				do {
					cvCvtColor(frame_copy,output,CV_BGR2GRAY);
					cvThreshold(gray, output, threshold, thresholdMax,CV_THRESH_BINARY_INV);
					
					key = (char) cvWaitKey(0);
					if(key == 'p' )
						done  = true;
					else if(key == 'q') {
						done = true;
						shouldEnd = true;
					} else if(key == 'z') {
						if(zoomed) {
							cvDestroyWindow("zoomed");
							cvReleaseImage(&imgZoom);
							zoomed = false;
							cvSetMouseCallback( "toClick", on_mouse, 0 );
						} else {
							imgZoom = zoomImage(frame_copy);
							cvNamedWindow( "zoomed", 1 );
							cvShowImage("zoomed", imgZoom);
							zoomed = true;
							cvSetMouseCallback( "zoomed", on_mouse, 0 );
						}
					}
					else if(key == 'h') {
						forceMouseHip = true;
						printf("Remark Hip: ");
					} else if(key == 'k') {
						forceMouseKnee = true;
						printf("Remark Knee: ");
					} else if(key == 't') {
						forceMouseToe = true;
						printf("Remark Toe: ");
					} else if(key == '+')
						threshold ++;
					else if(key == '-')
						threshold --;
					else if (key == 'f') {// 'FORWARD'
						forward = true;
						printf("forwarding ...\n");
						done  = true;
					} else if (key == 'F') { // 'FORWARD SUPER'
						forwardSuper = true;
						done  = true;
						printf("super forwarding ...\n");
					}
					else if (key == 'j' || key == 'J') {
						//jump frames
						printf("current frame: %d. Jump to: ", framesCount);
						int jump;
						scanf("%d", &jump);

						//works with opencv1.1!!
						//now we can go back and forth
						cvSetCaptureProperty( capture, CV_CAP_PROP_POS_FRAMES, jump-1 );

						framesCount = jump;
						hipOld = pointToZero();
						kneeOld = pointToZero();
						toeOld = pointToZero();

						//when jumper is extending legs after maximal flexion, jump ends
						//this jumped makes it didn't work, avoiding ending a jump
						//when we have gone ('j') from a maximal flexion to the 1st frame again (extension)
						jumpedFrames = true;
						printf("jumping ...\n");
					}
					
					cvThreshold(gray, output, threshold, thresholdMax,CV_THRESH_BINARY_INV);

					sprintf(label, "frame: %d", framesCount);
					cvPutText(output, label, cvPoint(10,frame->height-40),&font,CV_RGB(0,0,0));

					sprintf(label, "threshold: %d", threshold);
					cvPutText(output, label, cvPoint(10,frame->height-20),&font,CV_RGB(0,0,0));
				
					cvShowImage("skinOutput", output);
				} while(! done);
				
				hipMarked = hipMouse;
				kneeMarked = kneeMouse;
				toeMarked = toeMouse;
						
				if(zoomed) {
					cvDestroyWindow("zoomed");
					cvReleaseImage(&imgZoom);
					zoomed = false;
				}
				mouseCanMark = false;
			}
			
		} 
		else {
			do {
				cvThreshold(gray,segmentedValidationHoles, threshold, thresholdMax,CV_THRESH_BINARY_INV);

				//create the largest contour image (stored on temp)
				cvThreshold(gray,segmented,threshold,thresholdMax,CV_THRESH_BINARY_INV);
				maxrect = findLargestContour(segmented, output, showContour);

				//search in output all the black places (pants) and 
				//see if there's a hole in that pixel on segmentedValidationHoles
				//if(programMode == blackAndMarkers || programMode == blackOnlyMarkers) {
					CvSeq* seqHolesEnd = findHoles(
							output, segmentedValidationHoles, foundHoles, frame_copy,  
							maxrect, hipOld, kneeOld, toeOld);

					hipMarked = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 0); 
					kneeMarked = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 1 ); 
					toeMarked = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 2 ); 
				//}
				threshold += thresholdInc;
			} while(
					(pointIsNull(hipMarked) || pointIsNull(kneeMarked) || pointIsNull(toeMarked))
					&& threshold < 100);

			threshold -= thresholdInc;
		}
			
		hipOld = hipMarked;
		kneeOld = kneeMarked;
		toeOld = toeMarked;


		CvPoint hipExpected;
		CvPoint kneeExpected;
		CvPoint toeExpected;
		//int area;

		/*
		 * 4
		 * FIND POINTS ON BLACKANDMARKERS
		 */

		if(programMode == blackAndMarkers) {
			CvPoint hipPointBack;
			CvPoint kneePointBack;
			CvPoint kneePointFront;

			hipPointBack = findHipPoint(output,maxrect);

			//provisionally ubicate hipPoint at horizontal 1/2
			hipExpected.x = hipPointBack.x + (findWidth(output, hipPointBack, true) /2);
			hipExpected.y = hipPointBack.y;

			//knee	
			kneePointFront = findKneePointFront(output,maxrect,hipPointBack.y);
			kneePointBack = findKneePointBack(output,maxrect,hipPointBack.y, kneePointFront.x); //hueco popliteo

			//toe
			CvPoint toeExpected = findToePoint(output,maxrect,kneePointFront.x,kneePointFront.y);


			foundAngle = false;
			if(minwidth == 0)
				minwidth = kneePointFront.x - hipPointBack.x;
			else {
				if((double)(kneePointFront.x- hipPointBack.x) > 1.15*minwidth 
						&&
						upperSimilarThanLower(hipExpected, kneePointFront, toeExpected)
						&& !pointIsNull(hipMarked) && !pointIsNull(kneeMarked) && 
						!pointIsNull(toeMarked)
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

				kneeExpected = kneePointInNearMiddleOfFrontAndBack(
						kneePointBack, kneePointFront, kneePointWidth, frame_copy);
				cvCircle(frame_copy,kneePointBack,2, CV_RGB(128,128,128),1,8,0);
				cvCircle(frame_copy,kneePointFront,2, CV_RGB(128,128,128),1,8,0);
				cvLine(frame_copy,kneePointFront,kneePointBack,CV_RGB(128,128,128),1,1);
				cvCircle(frame_copy,kneeExpected,3, CV_RGB(255,255,0),1,8,0);

				// ------------ toe stuff ----------

				/*
				 * don't find width of toe for each photogramme
				 * do it only at first, because if at any photogramme, as flexion advances, 
				 * knee pointfront is an isolate point at the right of the lowest part of the pants
				 * because the back part of kneepoint has gone up
				 */

				if(toePointWidth == -1) 
					toePointWidth = findWidth(output, toeExpected, false);
				cvCircle(frame_copy,toeExpected,2, CV_RGB(128,128,128),1,8,0);

				thetaExpected = findAngle2D(hipExpected, toeExpected, kneeExpected);

				//fix toeExpected.x at the 1/2 of the toe width
				//depending on kneeAngle
				toeExpected.x = fixToePointX(toeExpected.x, toePointWidth, thetaExpected);
				cvCircle(frame_copy,toeExpected,3, CV_RGB(255,0,0),1,8,0);


				// ------------ hip stuff ----------

				//fix hipExpected ...
				cvCircle(frame_copy,hipPointBack,2, CV_RGB(128,128,128),1,8,0);

				//... find at 3/2 of hip (surely under the hand) ...
				//thetaExpected = findAngle2D(hipExpected, toeExpected, kneeExpected);
				hipExpected = fixHipPoint1(output, hipPointBack.y, kneeExpected, thetaExpected);
				cvCircle(frame_copy,hipExpected,2, CV_RGB(128,128,128),1,8,0);

				//... cross first hippoint with the knee-hippoint line to find real hippoint
				hipExpected = fixHipPoint2(output, hipPointBack.y, kneeExpected, hipExpected);
				cvCircle(frame_copy,hipExpected,3, CV_RGB(255,0,0),1,8,0);


				// ------------ flexion angle ----------

				//find flexion angle
				thetaExpected = findAngle2D(hipExpected, toeExpected, kneeExpected);
				//double thetaBack = findAngle2D(hipExpected, toeExpected, kneePointBack);

				//draw 2 main lines
				cvLine(frame_copy,kneeExpected,hipExpected,CV_RGB(255,0,0),1,1);
				cvLine(frame_copy,kneeExpected,toeExpected,CV_RGB(255,0,0),1,1);

				cvSeqPush( hipSeq, &hipExpected );
				cvSeqPush( kneeSeq, &kneeExpected );
				cvSeqPush( toeSeq, &toeExpected );


				//find total area
				//area = findTotalArea(gray, maxrect);

				if(thetaExpected < minThetaExpected)
				{
					minThetaExpected = thetaExpected;
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


		/*
		 * 5
		 * PRINT MARKERS RELATED INFO AND DO CALCULATIONS LIKE ANGLE
		 */


		if(pointIsNull(hipMarked) || pointIsNull(kneeMarked) || pointIsNull(toeMarked))
			thetaMarked = -1;
		else {
			thetaMarked = findAngle2D(hipMarked, toeMarked, kneeMarked);
			if(thetaMarked < minThetaMarked) 
				minThetaMarked = thetaMarked;


			if(programMode == blackAndMarkers)
				cvRectangle(frame_copy,
						cvPoint(maxrect.x,maxrect.y),
						cvPoint(maxrect.x + maxrect.width, maxrect.y + maxrect.height),
						CV_RGB(255,0,0),1,1);


			/*
			//print frame variation of distances of leg
			//soon find abduction, and then real flexion angle
			*/
			upLegMarkedDist = getDistance(hipMarked, kneeMarked);
			if(upLegMarkedDist > upLegMarkedDistMax)
				upLegMarkedDistMax = upLegMarkedDist;
			downLegMarkedDist = getDistance(toeMarked, kneeMarked);
			if(downLegMarkedDist > downLegMarkedDistMax)
				downLegMarkedDistMax = downLegMarkedDist;

			CvPoint HT;
			HT.y = kneeMarked.y;
			double verticalKVersusHT = (kneeMarked.y - toeMarked.y) / (double) (hipMarked.y-toeMarked.y) ;
			HT.x = ((hipMarked.x - toeMarked.x) * verticalKVersusHT ) + toeMarked.x;

			double kneeZetaSide = sqrt( pow(upLegMarkedDistMax,2) - pow(upLegMarkedDist,2) );
			double htKneeMarked = getDistance (HT, kneeMarked);

			double thetaABD = (180.0/M_PI)*atan( kneeZetaSide / (double) htKneeMarked );

			double thetaRealFlex = findAngle3D(hipMarked, toeMarked, kneeMarked, 0, 0, -kneeZetaSide);


			if(programMode == skinOnlyMarkers) {
				printOnScreen(output, font, CV_RGB(0,0,0), 
						upLegMarkedDist + downLegMarkedDist, 
						relError(upLegMarkedDistMax + downLegMarkedDistMax, upLegMarkedDist + downLegMarkedDist),
						framesCount, threshold, thetaMarked, minThetaMarked);
				cvShowImage("toClick", frame_copy);
				cvShowImage("skinOutput",output);
			}

			printOnScreen(frame_copy, font, CV_RGB(255,255,255), 
					upLegMarkedDist + downLegMarkedDist, 
					relError(upLegMarkedDistMax + downLegMarkedDistMax, upLegMarkedDist + downLegMarkedDist),
					framesCount, threshold, thetaMarked, minThetaMarked);

			printOnScreenRight(frame_copy, font, CV_RGB(255,255,255), 
					upLegMarkedDist, downLegMarkedDist,  
					upLegMarkedDistMax, downLegMarkedDistMax,  
					kneeZetaSide, htKneeMarked, 
					thetaABD, thetaRealFlex);

			if(programMode == blackAndMarkers && foundAngle) {
				/*
				//print data
				double thetaSup = findAngle2D(hipExpected, cvPoint(0,kneeExpected.y), kneeExpected);
				double thetaMarkedSup = findAngle2D(hipMarked, cvPoint(0, kneeMarked.y), kneeMarked);

				double thetaInf = findAngle2D(cvPoint(0,kneeExpected.y), toeExpected, kneeExpected);
				double thetaMarkedInf = findAngle2D(cvPoint(0,kneeMarked.y), toeMarked, kneeMarked);

				printf("%7d %7.2f %7.2f [%7.2f %7.2f] %7.2f %7.2f %7.2f [%7.2f %7.2f] [%7.2f] [%7.2f]\n", framesCount, thetaExpected, thetaMarked, 
				thetaMarked-thetaExpected, relError(thetaExpected, thetaMarked), 
				getDistance(kneeExpected, kneeMarked), 
				thetaSup-thetaMarkedSup, thetaInf-thetaMarkedInf
				, getDistance(kneeMarked, hipMarked), getDistance(kneeExpected, hipExpected)
				, getDistance(kneeExpected, hipPointBack)
				, getDistance(kneePointBack, hipPointBack)
				//, area
				);

				avgThetaDiff += abs(thetaMarked-thetaExpected);
				avgThetaDiffPercent += abs(relError(thetaExpected, thetaMarked));
				avgKneeDistance += getDistance(kneePoint, kneeMarked);
				*/
				framesDetected ++;
			}


			if(programMode == blackAndMarkers || programMode == blackOnlyMarkers)
				cvShowImage("result",frame_copy);


			//Finds the minimum angle between Hip to Knee line and Knee to Toe line
			if(thetaMarked == minThetaMarked) {
//				if(programMode == blackOnlyMarkers || programMode == skinOnlyMarkers) {
					/*
					 * if only process validation points, then minimum angle should be obtained 
					 * by the validation points
					 */

					cvCopy(frame_copy,result);
					//lowestAngleFrame = hipSeq->total -1;
					lowestAngleFrame = framesCount;
//				}
			}

			//exit if we are going up and soon jumping.
			//toe will be lost
			//detected if minThetaMarked is littler than thetaMarked, when thetaMarked is big
			if(thetaMarked > 140 && 
					minThetaMarked +10 < thetaMarked &&
					! jumpedFrames)
			{
				printf("\ntm: %f, mtm: %f, frame: %d\n", thetaMarked, minThetaMarked, framesCount);
				shouldEnd = true;
			}
		}


		/* 
		 * 6
		 * WAIT FOR KEYS
		 */

		int myDelay = playDelay;
		//if(programMode != blackAndMarkers || foundAngle)
		if(foundAngle)
			myDelay = playDelayFoundAngle;

		key = (char) cvWaitKey(myDelay);
		if(key == 27 || key == 'q') // 'ESC'
			shouldEnd = true;
		else if (key == '+')  
			threshold += thresholdInc;
		else if (key == '-') 
			threshold -= thresholdInc;
		else if (key == 'f') { 
			forward = true;
			printf("forwarding ...\n");
		} else if (key == 'F') { 
			forwardSuper = true;
			printf("super forwarding ...\n");
		} else if (key == 'j' || key == 'J') {
			//jump frames
			printf("current frame: %d. Jump to: ", framesCount);
			int jump;
			scanf("%d", &jump);
		
			//works with opencv1.1!!
			//now we can go back and forth
			cvSetCaptureProperty( capture, CV_CAP_PROP_POS_FRAMES, jump-1 );
			
			framesCount = jump;

			//when jumper is extending legs after maximal flexion, jump ends
			//this jumped makes it didn't work, avoiding ending a jump
			//when we have gone ('j') from a maximal flexion to the 1st frame again (extension)
			jumpedFrames = true;
			
			//hlps to know when we jumped and we have to initialize hipOld, kneeOld, toOld
			jumping = true;
			printf("jumping ...\n");
		}
		else if (key == 'p')
		{
			//if paused, print "pause"
			sprintf(label,"Pause");
			if(programMode == skinOnlyMarkers) {
				cvPutText(output, label,cvPoint(10, 25),&font,cvScalar(128,128,128));
				cvShowImage("skinOutput",output);
			} else {
				cvPutText(frame_copy, label,cvPoint(10, 25),&font,cvScalar(0,0,255));
				cvShowImage("result",frame_copy);
			}

			bool done = false;
			do{
				key = (char) cvWaitKey(0);
				if(key == 27 || key == 'q') {
					shouldEnd = true;
					done = true;
				}
				else if(key =='p') { 
					threshold -= thresholdInc;
					done = true;
				}
				else if(key =='+' || key == '-') {  
					if(key =='+')  
						threshold += thresholdInc;
					else if(key =='-') 
						threshold -= thresholdInc;
		
					if(programMode == skinOnlyMarkers) {
						cvThreshold(gray, output, threshold, thresholdMax,CV_THRESH_BINARY_INV);
						sprintf(label,"Pause");
						cvPutText(output, label,cvPoint(10, 25),&font,cvScalar(128,128,128));
						sprintf(label, "frame: %d", framesCount);
						cvPutText(output, label, cvPoint(10,frame->height-40),&font,CV_RGB(0,0,0));
						sprintf(label, "threshold: %d", threshold);
						cvPutText(output, label, cvPoint(10,frame->height-20),&font,CV_RGB(0,0,0));
						cvShowImage("skinOutput", output);
					}
					else {		
						cvThreshold(gray,segmentedValidationHoles, threshold, thresholdMax,CV_THRESH_BINARY_INV);
						//create the largest contour image (stored on temp)
						cvThreshold(gray,segmented,threshold,thresholdMax,CV_THRESH_BINARY_INV);
						maxrect = findLargestContour(segmented, output, showContour);

						updateHolesWin(segmentedValidationHoles);
					}
				}

			} while (! done);
		}

		if(programMode == blackAndMarkers || programMode == blackOnlyMarkers) {
			updateHolesWin(segmentedValidationHoles);
		}

	}

	/*
	 * END OF MAIN LOOP
	 */

	if(programMode == blackAndMarkers && foundAngleOneTime) {

		avgThetaDiff = (double) avgThetaDiff / framesDetected;
		avgThetaDiffPercent = (double) avgThetaDiffPercent / framesDetected;
		avgKneeDistance = (double) avgKneeDistance / framesDetected;

		printf("\n[%f %f] %f\n", avgThetaDiff, avgThetaDiffPercent, avgKneeDistance);

		// do this on R
		/*
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
		*/
			
		cvNamedWindow("Minimum Frame",1);
		cvShowImage("Minimum Frame", result);

		printf("Minimum Frame\n");
		sprintf(label, "minblack minholes    diff diff(%%)");
		sprintf(label, "%8.2f %8.2f [%7.2f %7.2f]", minThetaExpected, minThetaMarked, 
				minThetaMarked-minThetaExpected, relError(minThetaExpected, minThetaMarked));
		printf("%s\n" ,label);

		cvWaitKey(0);
	}
	else {
		printf("*** Result ***\nMin angle: %.2f, lowest angle frame: %d\n", minThetaMarked, lowestAngleFrame);
		cvNamedWindow("Minimum Frame",1);
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


int menu() 
{
	/* initial menu */
	printf( "Use: \n"
			"\tq - quit the program\n"
			"\tb - change mode to black long pants and marker validation\n"
			"\tB - change mode to Black long pants (only markers)\n"
			"\ts - change mode to Skin (short pants) (only markers)\n"
			"\tj - Jump at selected frame\n"
			"\ti - Init the program\n"
	      );

	int key;
	int programMode = blackAndMarkers;
	char option;

	bool menuDone = false;
	do{
		printf("Current options:\n");
		if(programMode == blackAndMarkers)
			printf("\t- Black long pants and marker validation\n");
		else if(programMode == blackOnlyMarkers)
			printf("\t- Black long pants (only markers)\n");
		else
			printf("\t- Skin (short pants) (only markers)\n");
		printf("\t- start at frame: %d\n", startAt);

		scanf("%c", &option);
		switch( option ) {
			case 'q':
				printf("Exiting ...\n");
				menuDone = true;
			case 'b':
				programMode = blackAndMarkers;
				break;
			case 'B':
				programMode = blackOnlyMarkers;
				break;
			case 's':
				programMode = skinOnlyMarkers;
				break;
			case 'j':
				printf("Jump at frame: ");
				scanf("%d", &startAt);
				break;
			case 'i':
				printf("Starting...\n");
				menuDone = true;
				break;
		}
	} while (! menuDone);

	return programMode;
}
				
