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



/*
 * TODO:
 * -imprimeixi en arxiu xy de cada punt (6 columnes)
 *  -implement adaptative threshold on skin only markers, to allow hip (pant bended) to be detected without having a threshold too low or too large on knee and toe
 *  solve the problem with the cvCopy(frame_copy,result);
 *  	on blackAndMarkers, minimumFrame is the marked or the expected?
 * -on skin, when a point is lost, re-assign the other points depending on distance with previous frame marked points
 *  -study kalman on openCV book (not interesting)
 *  -calibration and undistortion (distorsions: radial, and tangential)
 *     radial doesn't exist on image, because the line dividing floor and wall is straight from left to right (radial will make distort on places far from the center)
 *     tangential (p376,377) seems also to not exist on used camera, but best to record a cheesboard or square object to check
 *     maybe we can use opencv to paint the "claqueta" corners, and the use it as a chessboard, the problem is that this image is not always fully seen, but it doesn't need to be seen in all persons
 *     but use calibration all the time is not nice, because we prefer to record best the person, and zoom in or out if necessary. if the camera has not considerable distorsion, is best to don't need to be all the time with the chessboard. maybe we can (in the software) test the camera one time to see its distorsion. A 398 diu que és millor que es vegi el chessboard des de diferents llocs, sinó la solució no serà bona. Així que millor oblidar-se de pintar la claqueta com a chessboard quan entra per la dreta. Pero sí que es podria calcular la distorsió de la càmera i provar un mateix salt (amb marcadors) amb undistort i sense. Val a dir també que els punts cercats de la imatge no són a les cantonades, així que la radial distort afectarà poc; està per veure la tangencial, tot i que és més cosa de cheap cameres.
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

#include "kneeAngleGlobal.cpp"
#include "kneeAngleUtil.cpp"
#include "kneeAngleFunctions.cpp"

using namespace std;

int menu(IplImage *, CvFont);

	
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
	/*
	int framesNumber = cvGetCaptureProperty(capture, CV_CAP_PROP_FRAME_COUNT);
	printf("--%d--\n", framesNumber);
	*/

	
	/* initialization variables */
	IplImage *frame=0,*frame_copy=0,*gray=0,*segmented=0,*edge=0,*temp=0,*output=0;
	IplImage *segmentedValidationHoles=0;
	IplImage *foundHoles=0;
	IplImage *result=0;
	IplImage *resultStick=0;
	IplImage *extension=0, *extensionSeg=0, *extensionSegHoles=0;
	IplImage *gui=0; //interact with user
	
	CvFont font;
	int fontLineType = CV_AA; // change it to 8 to see non-antialiased graphics
	double fontSize = .4;
	cvInitFont(&font, CV_FONT_HERSHEY_COMPLEX, fontSize, fontSize, 0.0, 1, fontLineType);
	
	gui = cvCreateImage(cvSize(500, 300), IPL_DEPTH_8U,3);
	cvNamedWindow("gui",1);
	int programMode = menu(gui, font);

	
	int minwidth = 0;
	
	bool foundAngle = false; //found angle on current frame
	bool foundAngleOneTime = false; //found angle at least one time on the video

	double knee2Hip,knee2Toe,hip2Toe;
	double thetaExpected, thetaMarked;
	string text,angle;
	double minThetaExpected = 360;
	double minThetaMarked = 360;
	double minThetaRealFlex = 360;
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
	
	bool shouldEnd = false;

	//stick storage
	CvMemStorage* stickStorage = cvCreateMemStorage(0);
	CvSeq* hipSeq = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), stickStorage );
	CvSeq* kneeSeq = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), stickStorage );
	CvSeq* toeSeq = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), stickStorage );
  
	/*
	if(programMode == blackAndMarkers) {
		sprintf(label, "    fps a_black a_holes [   diff diff(%%)] kneeDif  a_supD  a_infD");
		printf("%s\n" ,label);
	}
	*/

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
	int forwardCount = 1;
	bool backward = false;
	int backwardSpeed = 50;
	
	bool jumping = false;



	bool labelsAtLeft = true;
		
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


	//programMode == blackAndMarkers
	bool extensionCopyDone = false;
	CvPoint kneeMarkedAtExtension = pointToZero();
	CvPoint hipMarkedAtExtension = pointToZero();
	CvPoint hipPointBackAtExtension = pointToZero();
	
			
	while(!shouldEnd) 
	{
		/*
		 * 1
		 * GET FRAME AND FLOW CONTROL
		 */

		frame = cvQueryFrame(capture);
		
		//when we go back, we doesn't always land where we want, this is safe:
		double capturedFrame = cvGetCaptureProperty( capture, CV_CAP_PROP_POS_FRAMES);
		framesCount = capturedFrame +1;

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
				forwardCount = 1;
				forward = false;
				forwardSuper = false;

				//mark that we are forwarding to the holesdetection
				//then it will not need to match a point with previous point
				hipOld = pointToZero();
				kneeOld = pointToZero();
				toeOld = pointToZero();
			}
		}
		if(jumping || backward) {
				hipOld = pointToZero();
				kneeOld = pointToZero();
				toeOld = pointToZero();
				jumping = false;
				backward = false;
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
			segmentedValidationHoles  =	cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,1);
			foundHoles  =	cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,3);
			edge =	cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,1);
			temp = cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,1);
			output = cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,1);
			result = cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,3);
			resultStick = cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,3);
			extension =	cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,3);
			extensionSeg =	cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,1);
			extensionSegHoles =	cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,1);
	
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
				
			CvSeq* seqHolesEnd = findHolesSkin(output, frame_copy, hipMarked, kneeMarked, toeMarked, font);

			hipMarked = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 0); 
			kneeMarked = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 1 ); 
			toeMarked = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 2 ); 

			crossPoint(frame_copy, hipMarked, GREY, MID);
			crossPoint(frame_copy, kneeMarked, GREY, MID);
			crossPoint(frame_copy, toeMarked, GREY, MID);
			
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
					
				//printf("\nFrame: %d H(%d,%d), K(%d,%d), T(%d,%d)\n", framesCount, hipMarked.x, hipMarked.y, 
				//		kneeMarked.x, kneeMarked.y, toeMarked.x, toeMarked.y);


				/*
				if(pointIsNull(hipMarked) || pointIsNull(kneeMarked) || pointIsNull(toeMarked)) {
					sprintf(label, "Please mark");
					printf("** Please mark:");
					if(pointIsNull(hipMarked))
						printf(" HIP ");
					if(pointIsNull(kneeMarked))
						printf(" KNEE ");
					if(pointIsNull(toeMarked))
						printf(" TOE ");
					printf("on 'toClick' window **\n");
				}
				*/
			
				/*	
				printf("Mark: hip 'h', knee 'k', toe 't' on 'toClick' window if needed\n"); 
				printf("Change threshold: %d ('+' increase, '-' decrease)\n", threshold); 
				printf("Others: Zoom 'z', forward 'f', Forward 'F', jump 'j', quit program 'q'\n\n"); 
				printf("\tPress 'p' when done.\n");
				*/
				int row = 1;
				int step = 16;
				imagePrint(gui, cvPoint(10, step*row++), "Mark: hip 'h', knee 'k', toe 't' on 'toClick' window if needed", font, WHITE);
				imagePrint(gui, cvPoint(10, step*row++), "Change threshold: %d ('+' increase, '-' decrease)", font, WHITE);
				imagePrint(gui, cvPoint(10, step*row++), "Zoom 'z'", font, WHITE);
				imagePrint(gui, cvPoint(10, step*row++), "forward 'f', Forward 'F', jump 'j'", font, WHITE);
				imagePrint(gui, cvPoint(10, step*row++), "Quit program 'q'", font, WHITE);
				row ++;
				imagePrint(gui, cvPoint(10, step*row++), "Press 'p' when done.", font, WHITE);
				cvShowImage("gui", gui);

				bool done = false;
				IplImage* imgZoom;
					
				cvCvtColor(frame_copy,output,CV_BGR2GRAY);
				cvThreshold(gray, output, threshold, thresholdMax,CV_THRESH_BINARY_INV);
						
				bool thresholdChanged = false;
				do {
					key = (char) cvWaitKey(50);
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
					} else if(key == '+') {
						threshold ++;
						thresholdChanged = true;
					} else if(key == '-') {
						threshold --;
						if(threshold < 0)
							threshold = 0;
						thresholdChanged = true;
					} else if (key == 'f') {// 'FORWARD'
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

						printf("jumping ...\n");
						done = true;
					}
					
					if(thresholdChanged)  {
						cvThreshold(gray, output, threshold, thresholdMax,CV_THRESH_BINARY_INV);

						sprintf(label, "frame: %d", framesCount);
						imagePrint(output, cvPoint(10, frame->height-40), label, font, WHITE);

						sprintf(label, "threshold: %d", threshold);
						imagePrint(output, cvPoint(10, frame->height-20), label, font, WHITE);
				
						cvShowImage("skinOutput", output);
						thresholdChanged = false;
					}
				
					if(!pointIsEqual(hipMarked,hipMouse)) {
						crossPoint(frame_copy, hipMouse, MAGENTA, BIG);
						imagePrint(frame_copy, cvPoint(hipMouse.x -20, hipMouse.y), "H", font, MAGENTA);
						hipMarked = hipMouse;
						cvShowImage("toClick", frame_copy);
					}
					if(!pointIsEqual(kneeMarked,kneeMouse)) {
						crossPoint(frame_copy, kneeMouse, MAGENTA, BIG);
						imagePrint(frame_copy, cvPoint(kneeMouse.x -20, kneeMouse.y), "K", font, MAGENTA);
						kneeMarked = kneeMouse;
						cvShowImage("toClick", frame_copy);
					}
					if(!pointIsEqual(toeMarked,toeMouse)) {
						crossPoint(frame_copy, toeMouse, MAGENTA, BIG);
						imagePrint(frame_copy, cvPoint(toeMouse.x -20, toeMouse.y), "T", font, MAGENTA);
						toeMarked = toeMouse;
						cvShowImage("toClick", frame_copy);
					}
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
				eraseGuiWindow(gui);
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
				CvSeq* seqHolesEnd = findHoles(
						output, segmentedValidationHoles, foundHoles, frame_copy,  
						maxrect, hipOld, kneeOld, toeOld);

				hipMarked = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 0); 
				kneeMarked = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 1 ); 
				toeMarked = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 2 ); 
				
				threshold += thresholdInc;
			} while(
					(pointIsNull(hipMarked) || pointIsNull(kneeMarked) || pointIsNull(toeMarked))
					&& threshold < 100);

			threshold -= thresholdInc;
		}
			
		hipOld = hipMarked;
		kneeOld = kneeMarked;
		toeOld = toeMarked;

		/*
		 * 4
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


			upLegMarkedDist = getDistance(hipMarked, kneeMarked);
			if(upLegMarkedDist > upLegMarkedDistMax)
				upLegMarkedDistMax = upLegMarkedDist;
			downLegMarkedDist = getDistance(toeMarked, kneeMarked);
			if(downLegMarkedDist > downLegMarkedDistMax)
				downLegMarkedDistMax = downLegMarkedDist;

			CvPoint HT;
			HT.y = kneeMarked.y;
			HT.x = hipMarked.x;

			double kneeZetaSide = sqrt( pow(upLegMarkedDistMax,2) - pow(upLegMarkedDist,2) );
			double htKneeMarked = getDistance (HT, kneeMarked);

			double thetaABD = (180.0/M_PI)*atan( kneeZetaSide / (double) htKneeMarked );

			double thetaRealFlex = findAngle3D(hipMarked, toeMarked, kneeMarked, 0, 0, -kneeZetaSide);
			if(thetaRealFlex < minThetaRealFlex) 
				minThetaRealFlex = thetaRealFlex;


			if(programMode == skinOnlyMarkers) {
				printOnScreen(output, font, CV_RGB(0,0,0), labelsAtLeft,
						framesCount, threshold, 
						(double) upLegMarkedDist *100 /upLegMarkedDistMax, 
						(double) downLegMarkedDist *100 /downLegMarkedDistMax,
						thetaMarked, minThetaMarked,
						thetaABD, thetaRealFlex, minThetaRealFlex
						);
				cvShowImage("toClick", frame_copy);
				cvShowImage("skinOutput",output);
			}

			printOnScreen(frame_copy, font, CV_RGB(255,255,255), labelsAtLeft,
					framesCount, threshold, 
					(double) upLegMarkedDist *100 /upLegMarkedDistMax, 
					(double) downLegMarkedDist *100 /downLegMarkedDistMax,
					thetaMarked, minThetaMarked,
					thetaABD, thetaRealFlex, minThetaRealFlex
					);

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
			if(thetaRealFlex == minThetaRealFlex) {
				cvCopy(frame_copy,result);
				lowestAngleFrame = framesCount;
			}

			//exit if we are going up and soon jumping.
			//toe will be lost
			//detected if minThetaMarked is littler than thetaMarked, when thetaMarked is big
			if(thetaMarked > 140 && 
					minThetaMarked +10 < thetaMarked)
			{
				imageGuiAsk(gui, "Minimal flexion reached before. Ending. Accept?", "'n'o, 'y'es", font);
				int option = optionAccept(true);	
				eraseGuiAsk(gui);
				if(option==YES) {
					printf("\ntm: %f, mtm: %f, frame: %d\n", thetaMarked, minThetaMarked, framesCount);
					shouldEnd = true;
				}
			}
		}


		CvPoint hipExpected;
		CvPoint kneeExpected;
		CvPoint toeExpected;

		/*
		 * 5
		 * IF BLACKANDMARKERS MODE,
		 * FIND POINTS
		 */

		if(programMode == blackAndMarkers) 
		{
			CvPoint hipPointBack;
			CvPoint kneePointBack;
			CvPoint kneePointFront;

			hipPointBack = findHipPoint(output,maxrect);

			//provisionally ubicate hipPoint at horizontal 1/2
			hipExpected.x = hipPointBack.x + (findWidth(output, hipPointBack, true) /2);
			hipExpected.y = hipPointBack.y;

			//knee	
			kneePointFront = findKneePointFront(output,maxrect,hipPointBack.y, foundAngleOneTime);
			//hueco popliteo
			kneePointBack = findKneePointBack(output,maxrect, 
					hipPointBack.y, kneePointFront.x,
					foundAngleOneTime); 

			//toe
			CvPoint toeExpected = findToePoint(output,maxrect,kneePointFront.x,kneePointFront.y);


			foundAngle = false;
			if(minwidth == 0)
				minwidth = kneePointFront.x - hipPointBack.x;
			else {
				if((double)(kneePointFront.x- hipPointBack.x) > 1.15*minwidth && //or 1.05 or 1.15
						upperSimilarThanLower(hipExpected, kneePointFront, toeExpected)
						&& !pointIsNull(hipMarked) && !pointIsNull(kneeMarked) && 
						!pointIsNull(toeMarked))
				{
					if(foundAngleOneTime) {
						foundAngle = true;
					} 
					else if(extensionCopyDone) 
					{
						/* 
						 * first time, confirm we found knee ok (and angle)
						 * maybe is too early
						 */
						crossPoint(frame_copy, kneePointFront, GREY, MID);
						crossPoint(frame_copy, kneePointBack, GREY, MID);

						cvShowImage("result",frame_copy);
						imageGuiAsk(gui, "knee found. Accept?", "'n'o, 'y'es, 'f'orward, super'F'orward, 'b'ackward, 'q'uit", font);
					
						int option = optionAccept(false);	
						eraseGuiAsk(gui);

						if(option==YES) {
							printf("Accepted\n");
							foundAngle = true;
							foundAngleOneTime = true;

							CvRect rectExt = findKneeCenterAtExtension(extensionSeg, kneePointFront.y);
							
							//kneeRect height center will be in the middle of kneepointfront and back
							//the kneeRect.y should be (1/2 height below)
							rectExt.height = rectExt.width;
							rectExt.y = ( (kneePointFront.y + kneePointBack.y) /2 ) - (rectExt.height/2);

					
							//debug	
							//cvNamedWindow("Extension seg old",1);
							//cvShowImage("Extension seg old", extensionSeg);


							double kneeCenterExtX = rectExt.x + (double)(rectExt.width /2 );
							double kneeCenterExtY = rectExt.y + (double)(rectExt.height /2 );

							double kneeCenterExtXPercent = 100 * (double)(kneeCenterExtX - rectExt.x) / rectExt.width; //aprox 50%
							double kneeCenterExtYPercent = 100 * (double)(kneeCenterExtY - rectExt.y) / rectExt.height; //aprox 50%
							
							printf("kneeCenterExtX,Y: %.1f(%.1f%%) %.1f(%.1f%%)\n", 
									kneeCenterExtX, 
									kneeCenterExtXPercent, 
									kneeCenterExtY, 
									kneeCenterExtYPercent 
									);				//debug
							
							/*
							 * now print differences between:
							 * CvPoint(kneeCenterExtX, kneePointFront.y) and kneeMarkedAtEXtension
							 */
							printf("marked at ext: x: %d, y: %d\n", kneeMarkedAtExtension.x, kneeMarkedAtExtension.y); //debug

							//see the % of rectangle where kneeMarked is (at extension)
							double kneeMarkedXPercent = 100 * (double)(kneeMarkedAtExtension.x - rectExt.x) / rectExt.width;
							double kneeMarkedYPercent = 100 * (double)(kneeMarkedAtExtension.y - rectExt.y) / rectExt.height;


							cvRectangle(extension, 
									cvPoint(rectExt.x, rectExt.y), 
									cvPoint(rectExt.x + rectExt.width, rectExt.y + rectExt.height),
									CV_RGB(255,0,255),1,8,0
									);
							crossPoint(extension,cvPoint(rectExt.x + rectExt.width, kneePointFront.y), WHITE, MID);
							crossPoint(extension,cvPoint(rectExt.x, kneePointBack.y), WHITE, MID);



							printf("knee diff: x: %.1f (%.1f%%), y: %.1f (%.1f%%)\n", 
									kneeMarkedAtExtension.x - kneeCenterExtX,
									kneeMarkedXPercent - kneeCenterExtXPercent,
									kneeMarkedAtExtension.y - kneeCenterExtY,
									kneeMarkedYPercent - kneeCenterExtYPercent);	
							
							cvLine(extension,
									kneeMarkedAtExtension,
									cvPoint(kneeCenterExtX, kneeCenterExtY),
									WHITE,1,1);
							
							//hip
							double hipMarkedX = hipMarkedAtExtension.x - kneeCenterExtX;
							double hipMarkedXPercent = 100 * (double) hipMarkedX / rectExt.width;
							printf("hip diff: x: %.1f (%.1f%%)\n", 
									hipMarkedX,
									hipMarkedXPercent
									);
							cvLine(extension,
									hipMarkedAtExtension,
									cvPoint(kneeCenterExtX, hipMarkedAtExtension.y),
									CV_RGB(128,128,128),1,1);
							
							//back
							double backDistance = kneeCenterExtX - hipPointBackAtExtension.x;
							double backDistancePercent = 100 * (double) backDistance / rectExt.width ;
							printf("back width: %.1f (%.1f%%)\n", 
									backDistance, 
									backDistancePercent
									);
							cvLine(extension,
									hipPointBackAtExtension,
									cvPoint(kneeCenterExtX, hipPointBackAtExtension.y),
									CV_RGB(128,128,128),1,1);
							
							cvShowImage("Extension Frame", extension);
						} else {
							foundAngle = false;
							
							if(option==FORWARD) {
								forward = true;
								printf("forwarding ...\n");
							} else if(option==SUPERFORWARD) {
								forwardSuper = true;
								printf("super forwarding ...\n");
							} else if(option==BACKWARD) {
								backward = true;

								//try to go to previous (backwardspeed) frame
								cvSetCaptureProperty( capture, CV_CAP_PROP_POS_FRAMES, 
										framesCount - backwardSpeed -1 );
									
								printf("backwarding ...\n");
								continue;
							} else if(option==QUIT) {
								shouldEnd = true;
								printf("exiting ...\n");
							}
						}
					}
				}
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
				crossPoint(frame_copy, kneePointBack, GREY, MID);
				crossPoint(frame_copy, kneePointFront, GREY, MID);
				
				cvLine(frame_copy,kneePointFront,kneePointBack, GREY,1,1);
				crossPoint(frame_copy, kneeExpected, RED, BIG);


				int kneeWidth = kneePointFront.x - kneePointBack.x;
				int kneeMarkedPosX = kneeWidth - (kneeMarked.x - kneePointBack.x);
				double kneeMarkedPercentX = (double) kneeMarkedPosX * 100 / kneeWidth;
				int kneeHeight = kneeWidth; //kneeHeight can be 0, best to use kneeWidth for percent, is more stable
				int kneeHeightBoxDown = ( (kneePointFront.y + kneePointBack.y) /2 ) - (kneeHeight /2);
				int kneeMarkedPosY = kneeHeight - (kneeMarked.y - kneeHeightBoxDown);
				double kneeMarkedPercentY = (double) kneeMarkedPosY * 100 / kneeHeight;
				cvRectangle(frame_copy, 
						cvPoint(kneePointBack.x, kneeHeightBoxDown), 
						cvPoint(kneePointBack.x + kneeWidth, kneeHeightBoxDown + kneeHeight),
						CV_RGB(255,0,255),1,8,0);
				if(pointIsNull(hipMarked) || pointIsNull(kneeMarked) || pointIsNull(toeMarked))
					thetaMarked = -1;
				else
					thetaMarked = findAngle2D(hipMarked, toeMarked, kneeMarked);

				// ------------ toe stuff ----------

				/*
				 * don't find width of toe for each photogramme
				 * do it only at first, because if at any photogramme, as flexion advances, 
				 * knee pointfront is an isolate point at the right of the lowest part of the pants
				 * because the back part of kneepoint has gone up
				 */

				if(toePointWidth == -1) 
					toePointWidth = findWidth(output, toeExpected, false);
				crossPoint(frame_copy, toeExpected, GREY, MID);

				thetaExpected = findAngle2D(hipExpected, toeExpected, kneeExpected);
				
				
				//printf("%d;%.2f;%.2f;%.2f\n", framesCount, thetaMarked, kneeMarkedPercentX, kneeMarkedPercentY);
				double angleBack =findAngle2D(hipPointBack, cvPoint(toeExpected.x - toePointWidth, toeExpected.y), kneePointBack); 
				printf("%d;%.2f;%.2f;%.2f\n", framesCount, angleBack, kneeMarkedPercentX, kneeMarkedPercentY);
				cvLine(frame_copy,hipPointBack, kneePointBack,CV_RGB(255,0,0),1,1);
				cvLine(frame_copy,kneePointBack, cvPoint(toeExpected.x - toePointWidth, toeExpected.y),CV_RGB(255,0,0),1,1);


				//fix toeExpected.x at the 1/2 of the toe width
				//depending on kneeAngle
				toeExpected.x = fixToePointX(toeExpected.x, toePointWidth, thetaExpected);
				crossPoint(frame_copy, toeExpected, RED, BIG);


				// ------------ hip stuff ----------

				//fix hipExpected ...
				crossPoint(frame_copy, hipPointBack, GREY, MID);

				//... find at 3/2 of hip (surely under the hand) ...
				//thetaExpected = findAngle2D(hipExpected, toeExpected, kneeExpected);
				hipExpected = fixHipPoint1(output, hipPointBack.y, kneeExpected, thetaExpected);
				crossPoint(frame_copy, hipExpected, GREY, MID);

				//... cross first hippoint with the knee-hippoint line to find real hippoint
				hipExpected = fixHipPoint2(output, hipPointBack.y, kneeExpected, hipExpected);
				crossPoint(frame_copy, hipExpected, RED, BIG);


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

				/*
				 * if we never have found the angle, 
				 * and this maxrect is wider than previous maxrect (flexion started)
				 * then capture previous image to process knee at extension search
				 */
				if(! foundAngleOneTime && ! extensionCopyDone) {
					cvShowImage("result",frame_copy);
					imageGuiAsk(gui, "Extension copy. Accept?", "'n'o, 'y'es, 'f'orward, super'F'orward, 'b'ackward, 'q'uit", font);
					int option = optionAccept(false);
					eraseGuiAsk(gui);

					if(option==YES) {
						cvCopy(frame_copy, extension);
						
						//cvCopy(segmented, extensionSeg);
						cvCopy(output, extensionSeg);

						//cvCopy(segmentedValidationHoles, extensionSegHoles);
						//printf("\nhere: x: %d, y: %d\n", kneeMarked.x, kneeMarked.y);
						kneeMarkedAtExtension = kneeMarked;
						hipMarkedAtExtension = hipMarked;
						hipPointBackAtExtension = hipPointBack;
						
						cvNamedWindow("Extension Frame",1);
						cvShowImage("Extension Frame", extension);
						
						extensionCopyDone = true;
					} else {
						if(option==FORWARD) {
							forward = true;
							printf("forwarding ...\n");
						} else if(option==SUPERFORWARD) {
							forwardSuper = true;
							printf("super forwarding ...\n");
						} else if(option==BACKWARD) {
							backward = true;
							
							//try to go to previous (backwardspeed) frame
							cvSetCaptureProperty( capture, CV_CAP_PROP_POS_FRAMES, 
									framesCount - backwardSpeed -1 );
							
							printf("backwarding ...\n");
							continue;
						} else if(option==QUIT) {
							shouldEnd = true;
							printf("exiting ...\n");
						}
					}
				}
			}
				
			cvShowImage("result",frame_copy);
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
		else if (key == '-') { 
			threshold -= thresholdInc;
			if(threshold < 0)
				threshold = 0;
		} else if (key == 'l') 
			labelsAtLeft = ! labelsAtLeft;
		else if (key == 'r') { //reset legs length
			upLegMarkedDistMax = 0;
			downLegMarkedDistMax = 0;
		}
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

			//hlps to know when we jumped and we have to initialize hipOld, kneeOld, toOld
			jumping = true;
			printf("jumping ...\n");
		}
		else if (key == 'p')
		{
			imageGuiAsk(gui, "Pause", "'p', '+', '-'", font);
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
					else if(key =='-') {
						threshold -= thresholdInc;
						if(threshold < 0)
							threshold = 0;
					}
		
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
						eraseGuiResult(gui);
						cvThreshold(gray,segmentedValidationHoles, threshold, thresholdMax,CV_THRESH_BINARY_INV);
						//create the largest contour image (stored on temp)
						cvThreshold(gray,segmented,threshold,thresholdMax,CV_THRESH_BINARY_INV);
						maxrect = findLargestContour(segmented, output, showContour);

						updateHolesWin(segmentedValidationHoles);
						
						sprintf(label, "threshold: %d", threshold);
						imageGuiResult(gui, label, font);
					}
				}

			} while (! done);
					
			eraseGuiAsk(gui);
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
			
		cvNamedWindow("Minimum Frame",1);
		cvShowImage("Minimum Frame", result);

		/*
		printf("Minimum Frame\n");
		sprintf(label, "minblack minholes    diff diff(%%)");
		sprintf(label, "%8.2f %8.2f [%7.2f %7.2f]", minThetaExpected, minThetaMarked, 
				minThetaMarked-minThetaExpected, relError(minThetaExpected, minThetaMarked));
		printf("%s\n" ,label);
		*/

		cvWaitKey(0);
	}
	else {
		printf("*** Result ***\nMin angle: %.2f, lowest angle frame: %d\n", minThetaMarked, lowestAngleFrame);
		cvNamedWindow("Minimum Frame",1);
		cvShowImage("Minimum Frame", result);
		cvWaitKey(0);
	}

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

int menu(IplImage * gui, CvFont font) 
{
	/* initial menu */

	int row = 1;
	int step = 16;
	char *label = new char[150];
	imagePrint(gui, cvPoint(10, step*row++), "Use:", font, WHITE);
	imagePrint(gui, cvPoint(20, step*row++), "b - change mode to black long pants and marker validation", font, WHITE);
	imagePrint(gui, cvPoint(20, step*row++), "B - change mode to Black long pants (only markers)", font, WHITE);
	imagePrint(gui, cvPoint(20, step*row++), "s - change mode to Skin (short pants) (only markers)", font, WHITE);
	//imagePrint(gui, cvPoint(20, step*row++), "j - Jump at selected frame", font, WHITE);
	imagePrint(gui, cvPoint(20, step*row++), "i - Init the program", font, WHITE);
	imagePrint(gui, cvPoint(20, step*row++), "q - quit program", font, WHITE);
		
	row ++;
	imagePrint(gui, cvPoint(10, step*row++), "Current option:", font, WHITE);
	row ++;

	int key;
	int programMode = blackAndMarkers;
	int startAt = 1;
	char option;

	bool menuDone = false;
	do{
		//delete
		cvRectangle(gui, cvPoint(10, step*(row-1)), cvPoint(gui->width, gui->height), CV_RGB(0,0,0),CV_FILLED,8,0);

		if(programMode == blackAndMarkers)
			imagePrint(gui, cvPoint(20, step*row++), "Black long pants and marker validation", font, WHITE);
		else if(programMode == blackOnlyMarkers)
			imagePrint(gui, cvPoint(20, step*row++), "Black long pants (only markers)", font, WHITE);
		else
			imagePrint(gui, cvPoint(20, step*row++), "Skin (short pants) (only markers)", font, WHITE);

		//sprintf(label, "start at frame: %d", startAt);
		//imagePrint(gui, cvPoint(20, step*row++), label, font);
				
		// create a toolbar 
		//cvCreateTrackbar("start at", "gui", &startAt, 100, on_trackbar);

		cvShowImage("gui", gui);

		key = (char) cvWaitKey(0);
		switch( key ) {
			case 'q':
				printf("Exiting ...\n");
				exit(0);
			case 'b':
				programMode = blackAndMarkers;
				break;
			case 'B':
				programMode = blackOnlyMarkers;
				break;
			case 's':
				programMode = skinOnlyMarkers;
				break;
				/*
			case 'j':
				row -= 2;
				sprintf(label, "Jump at frame: %d", startAt);
				imagePrint(gui, cvPoint(20, step*row++), label, font);
				imagePrint(gui, cvPoint(20, step*row++), "press '+', '-', '*', Skin (short pants) (only markers)", font);
				//scanf("%d", &startAt);
				// create a toolbar 
				cvCreateTrackbar(gui, "start at", &startAt, 100, on_trackbar);
				break;
				*/
			case 'i':
				printf("Starting...\n");
				menuDone = true;
				break;
		}
		row --;
	} while (! menuDone);
	eraseGuiWindow(gui);

	return programMode;
}
				
