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
 * Copyright (C) 2008-2010  Xavier de Blas <xaviblas@gmail.com> 
 *
 * version: 1.6 (Nov, 12, 2008)
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
 * if get lots of errors on playing like: 
 * "[swscaler @ 0x8d24260]No accelerated colorspace conversion found from yuv420p to bgr24."
 * execute:
 * ./kneeAngle "path to video file" 2> /dev/null
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
 *  -implement convexity defects (opencv book p.259) at findKneePointBack
 *  solve the problem with the cvCopy(frame_copy,result);
 *  	on blackAndMarkers, minimumFrame is the marked or the expected?
 *  -study kalman on openCV book (not interesting)... si, aplicar kalman enlloc de pointInside(previous)
 *
 *  need to do subpixel calculations for: thetaABD and thetaRealFlex, because the pixel unit is too big for this calculations
 *  eg: upLegMarkedDist: 170; upLegMarkedDistMax: 171.... 
 *  	produces kneeZetaSide of 18,46 and maybe with a htKneeMarked of only 3px (peson is almos full extended) 
 *  	resulting ABD is 80.
 *  	with subpixel, maybe the upLegMarkedDist is 170.4 and the Max is 170.6 this has kneeZetaSide of 8.25, and with htkneeMarked: 3px, ABD is: 70
 *  	as seen, maybe the problem is in the 3px. maybe this is not ABD, is ROT EXT, and is normal at this extension
 *  	
 *  	---------
 *  	maybe, getting closer to the camera (z axis) affects too much and space need to be calibrated before. 
 *  	Maybe this means that we only can use this uncalibrated data for seen angle
 *  	---------
 *
 *  	 
 *
 *  calibration:
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
		char *startMessage = new char[300];
		sprintf(startMessage, "\nkneeAngle HELP.\n\nProvide file location as a first argument...\nOptional: as 2nd argument provide a fraction of video to start at that frame, or a concrete frame.\nOptional: as 3rd argument provide mode you want to execute (avoiding main menu).\n\t%d: validation; %d: blackWithoutMarkers; %d: skinOnlyMarkers; %d: blackOnlyMarkers.\n\nEg: Start at frame 5375:\n\tkneeAngle myfile.mov 5375\nEg:start at 80 percent of video and directly as blackOnlyMarkers:\n\tkneeAngle myFile.mov .8 %d\n", 
				validation, blackWithoutMarkers, skinOnlyMarkers, blackOnlyMarkers, blackOnlyMarkers);
		cout<< startMessage <<endl;
		exit(1);
	}

	CvCapture* capture = NULL;

	char * fileName = argv[1];
//	printf("%s", fileName);
	capture = cvCaptureFromAVI(fileName);
	if(!capture)
	{
		exit(0);
	}
	
	int framesNumber = cvGetCaptureProperty(capture, CV_CAP_PROP_FRAME_COUNT);

	if(argc >= 3) {
		if(atof(argv[2]) < 1) 			
			startAt = framesNumber * atof(argv[2]);
		else 					
			startAt = atoi(argv[2]);	//start at selected frame
	}
	
	printf("Number of frames: %d\t Start at:%d\n\n", framesNumber, startAt);
	
	programMode = undefined;
	if(argc == 4) 
		programMode = atoi(argv[3]);


	//3D
	//printf("framesCount;hip.x;hip.y;knee.x;knee.y;toe.x;toe.y;angle seen;angle side;angle real\n");
	//not 3D but record thresholds
	//printf("framesCount;hip.x;hip.y;knee.x;knee.y;toe.x;toe.y;angle current; threshold, th.hip; th.knee; th.toe\n");

	
	/* initialization variables */
	IplImage *frame=0,*frame_copy=0,*gray=0,*segmented=0,*edge=0,*temp=0,*output=0;
	IplImage *outputTemp=0, *frame_copyTemp=0;
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
	
	cvNamedWindow("gui",1);
	//if programMode is not defined or is invalid, ask user
	if(programMode < validation || programMode > blackOnlyMarkers) {
		gui = cvLoadImage("kneeAngle_intro.png");
		cvShowImage("gui", gui);
		programMode = menu(gui, font);
	}
	
	if(programMode == skinOnlyMarkers) {
		usingContour = false;
		gui = cvLoadImage("kneeAngle_skin.png");
	}
	else if(programMode == blackOnlyMarkers || programMode == validation) {
		usingContour = true;
		gui = cvLoadImage("kneeAngle_black_contour.png");
	} 
	else
		gui = cvLoadImage("kneeAngle_black_without.png");

			
	imageGuiResult(gui, "Starting... please wait.", font);
	cvWaitKey(100); //to allow gui image be shown
	
	int kneeMinWidth = 0;

	int kneeWidthAtExtension = 0;
	int toeExtensionWidth = 0;
	
	bool foundAngle = false; //found angle on current frame
	bool foundAngleOneTime = false; //found angle at least one time on the video

	double knee2Hip,knee2Toe,hip2Toe;
	double thetaExpected, thetaMarked;
	string text,angle;
	double minThetaExpected = 360;
	double minThetaMarked = 360;
	double minThetaRealFlex = 360;
	char buffer[15];
					
	bool askForMaxFlexion = false; //of false, no ask, and no auto end before jump
	
	//validation files
	FILE *fheader; //contains max and mins values
	FILE *fdatapre; //each line: 'current box height; current angle'
	FILE *fdatapost; //each line: 'current box height percent; current angle' (percent comes from fheader)

	char header[] = "_header.txt";
	char txt[] = ".txt";
	char csv[] = ".csv";
	char fheaderName [strlen(fileName) + strlen(header)];
	char fdatapreName [strlen(fileName) + strlen(txt)];
	char fdatapostName [strlen(fileName) + strlen(csv)];

	if(programMode == validation) {
//		cvNamedWindow("Holes_on_contour",1);
//		cvNamedWindow("result",1);
		cvNamedWindow("threshold",1);

		//create fileNames
		strcpy(fheaderName, fileName);
		changeExtension(fheaderName, header);

		strcpy(fdatapreName, fileName);
		changeExtension(fdatapreName, txt);
		
		strcpy(fdatapostName, fileName);
		changeExtension(fdatapostName, csv);

		printf("mov file:%s\n",fileName);
		printf("header file:%s\n",fheaderName);
		printf("txt file:%s\n",fdatapreName);
		printf("csv file:%s\n",fdatapostName);

		if((fheader=fopen(fheaderName,"w"))==NULL){
			printf("Error, no se puede escribir en el fichero %s\n",fheaderName);
			fclose(fheader);
			exit(0);
		}
		if((fdatapre=fopen(fdatapreName,"w"))==NULL){
			printf("Error, no se puede escribir en el fichero %s\n",fdatapreName);
			fclose(fdatapre);
			exit(0);
		}
		if((fdatapost=fopen(fdatapostName,"w"))==NULL){
			printf("Error, no se puede escribir en el fichero %s\n",fdatapostName);
			fclose(fdatapost);
			exit(0);
		}
	} else if (programMode == skinOnlyMarkers || programMode == blackOnlyMarkers) {
		cvNamedWindow("threshold",1);
	}
	else if (programMode == blackWithoutMarkers)
		cvNamedWindow("result",1);
	
	

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
  
	double avgThetaDiff = 0;
	double avgThetaDiffPercent = 0;
	double avgKneeDistance = 0;
	double avgKneeBackDistance = 0;
	int framesDetected = 0;
	int framesCount = 0;
	//show a counting message every n frames:
	int framesCountShowMessage = 0;
	int framesCountShowMessageAt = 100;

	//to advance fast and really fast
	bool forward = false;
	int forwardSpeed = 50;
	bool fastForward= false; 
	int fastForwardSpeed = 200;
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
	//used for pressing 'l': last.  when a point is lost
	CvPoint hipOldWorked = pointToZero();
	CvPoint kneeOldWorked = pointToZero();
	CvPoint toeOldWorked = pointToZero();

	/*
	int upLegMarkedDist = 0;
	int upLegMarkedDistMax = 0;
	int downLegMarkedDist = 0;
	int downLegMarkedDistMax = 0;
	*/

	int threshold;
	
	//this is currently only used on blackOnlyMarkers to have a threshold to find the contour
	//(different than threshold for three points)
	int thresholdLargestContour; 

	int thresholdMax = 255;
	int thresholdInc = 1;
				
	IplImage* imgZoom;
	
	//threshold for the three specific points
	int thresholdROIH = -1;
	int thresholdROIK = -1;
	int thresholdROIT = -1;
	
	//to store threshold at min angle
	int thresholdAtMinAngle;
	int thresholdROIHAtMinAngle = -1;
	int thresholdROIKAtMinAngle = -1;
	int thresholdROITAtMinAngle = -1;
			
	//this helps at ROI to determine the area where threhold will be changed
	//useful if we have a white marker surrounded by white pant
	//then we can have main threshold low
	//then we can have main threshold ROI on that point high
	//then we can have main threshold ROI size low (butif marker moves a lot, then we will need to increase a bit)
	int thresholdROISizeH = 16; 
	int thresholdROISizeK = 16; 
	int thresholdROISizeT = 16;
	int thresholdROISizeInc = 2; //increment on each pulsation
	int thresholdROISizeMin = 8;

	int key;


	//programMode == validation || programMode == blackWithoutMarkers
	bool extensionDoIt = true;
	bool extensionCopyDone = false;
	CvPoint kneeMarkedAtExtension = pointToZero();
	CvPoint hipMarkedAtExtension = pointToZero();
	CvPoint hipPointBackAtExtension = pointToZero();

	//this contains data useful to validation: max and min Height and Width of all rectangles
	int validationRectHMax = 0;
	int validationRectHMin = 100000;
	int validationRectWMax = 0;
	int validationRectWMin = 100000;
	//angle at min Height of validation rectangle
	double validationRectHMinThetaMarked = 180;


	mouseClicked = undefined;	
	cvSetMouseCallback( "gui", on_mouse_gui, 0 );
		
	bool reloadFrame = false;
	int forcePause = false;
			

/* kalman */
	//CvKalman* kalman = cvCreateKalman(2,1,0);
	CvKalman* kalman = cvCreateKalman(2,2,0);
	CvMat* state = cvCreateMat(2,1,CV_32FC1);
	CvMat* process_noise = cvCreateMat(2,1,CV_32FC1);
	//CvMat* measurement = cvCreateMat(1,1,CV_32FC1);
	CvMat* measurement = cvCreateMat(2,1,CV_32FC1);
	CvPoint measurement_pt;
	cvZero( measurement );
	const float F[] = { 1, 1, 0, 1};
	memcpy(kalman->transition_matrix->data.fl, F, sizeof(F));

			CvRandState rng;
			cvRandInit(&rng,0,1,-1,CV_RAND_UNI);
			cvRandSetRange(&rng,0,0.1,0);
			rng.disttype = CV_RAND_NORMAL;
			cvRand(&rng, process_noise);			






	cvSetIdentity(kalman->measurement_matrix, 	cvRealScalar(1));
	cvSetIdentity(kalman->process_noise_cov, 	cvRealScalar(1e-5));
	cvSetIdentity(kalman->measurement_noise_cov, 	cvRealScalar(1e-1));
	cvSetIdentity(kalman->error_cov_post, 		cvRealScalar(1));

	CvPoint k0; k0.x=0; k0.y=0;
//	kalman->state_post = k0;
//	kalman->state_post.data.fl[0] = &k0;
/* /kalman */

	bool storeResultImage = false;

	while(!shouldEnd) 
	{
		/*
		 * 1
		 * GET FRAME AND FLOW CONTROL
		 */

		if( !reloadFrame)
			frame = cvQueryFrame(capture);
		reloadFrame = false;
		
		//when we go back, we doesn't always land where we want, this is safe:
		double capturedFrame = cvGetCaptureProperty( capture, CV_CAP_PROP_POS_FRAMES);
		framesCount = capturedFrame +1;

		if(!frame)
			break;
		if(startAt > framesCount) {
			//show a counting message every framesCountShowMessageAt, and continue
			framesCountShowMessage ++;
			if(framesCountShowMessage >= framesCountShowMessageAt) {
				eraseGuiResult(gui, true);
				sprintf(label, "frame: %d... (%d%%)", framesCount, 100*framesCount/startAt);
				imageGuiResult(gui, label, font);
				//printf("%s\n", label);
				cvWaitKey(25); //to allow gui image be shown
				framesCountShowMessage = 0;
			}
			continue;
		}
		if(forward || fastForward) {
			if(
					forward && (forwardCount < forwardSpeed) ||
					fastForward && (forwardCount < fastForwardSpeed)) {
				forwardCount ++;
				continue;
			} else {
				//end of forwarding
				forwardCount = 1;
				forward = false;
				fastForward = false;
				eraseGuiResult(gui, true);

				//mark that we are forwarding to the holesdetection
				//then it will not need to match a point with previous point
				//hipOld = pointToZero();
				//kneeOld = pointToZero();
				//toeOld = pointToZero();
			}
		}
		if(jumping || backward) {
				//hipOld = pointToZero();
				//kneeOld = pointToZero();
				//toeOld = pointToZero();
				jumping = false;
				backward = false;
				eraseGuiResult(gui, true);
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
			outputTemp = cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,1);
			result = cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,3);
			resultStick = cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,3);
			extension =	cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,3);
			extensionSeg =	cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,1);
			extensionSegHoles =	cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,1);

			if(programMode == skinOnlyMarkers) {
				cvCvtColor(frame_copy,gray,CV_BGR2GRAY);
				threshold = calculateThresholdStart(gray, false);
			}
			else if(programMode == blackOnlyMarkers || programMode == validation) {
				cvCvtColor(frame_copy,gray,CV_BGR2GRAY);
				threshold = calculateThresholdStart(gray, false);
				thresholdLargestContour = calculateThresholdStart(gray, true);
			}
			else {
				cvCvtColor(frame_copy,gray,CV_BGR2GRAY);
				threshold = calculateThresholdStart(gray, true);
			}
		}

		cvSmooth(frame_copy,frame_copy,2,5,5);
		cvCvtColor(frame_copy,gray,CV_BGR2GRAY);
		CvRect maxrect;

		/*
		 * 3 
		 * FIND THREE MARKER POINTS
		 */


		if(programMode == skinOnlyMarkers || programMode == blackOnlyMarkers || programMode == validation) 
		{

/* kalman */
			const CvMat* prediction = cvKalmanPredict(kalman, 0);
			CvPoint prediction_pt = cvPoint(
					cvRound(prediction->data.fl[0]), 
					cvRound(prediction->data.fl[1]));
/* /kalman */
	


			cvCvtColor(frame_copy,output,CV_BGR2GRAY);
			cvCvtColor(frame_copy,outputTemp,CV_BGR2GRAY);
			cvThreshold(gray, output, threshold, thresholdMax,CV_THRESH_BINARY_INV);


			if(thresholdROIH != -1 || thresholdROIK != -1 || thresholdROIT != -1)  
			{
				if(thresholdROIH != -1) {
					output = changeROIThreshold(gray, output, hipMarked, thresholdROIH, thresholdMax, thresholdROISizeH);
				}
				if(thresholdROIK != -1) {
					output = changeROIThreshold(gray, output, kneeMarked, thresholdROIK, thresholdMax, thresholdROISizeK);
				} 
				if(thresholdROIT != -1) {
					output = changeROIThreshold(gray, output, toeMarked, thresholdROIT, thresholdMax, thresholdROISizeT);
				}
			}


			//this segmented is to find the three holes
			cvThreshold(gray,segmented,threshold,thresholdMax,CV_THRESH_BINARY_INV);

			CvSeq* seqHolesEnd;

			if(programMode == skinOnlyMarkers) {
				seqHolesEnd = findHolesSkin(output, frame_copy, hipMarked, kneeMarked, toeMarked, font);
			}
			else { //if(programMode == blackOnlyMarkers || programMode == validation) 
				//this segmented is to find the contour (threshold is lot little)
				cvThreshold(gray,segmentedValidationHoles,thresholdLargestContour,thresholdMax,CV_THRESH_BINARY_INV);
				cvThreshold(gray,segmented,thresholdLargestContour,thresholdMax,CV_THRESH_BINARY_INV);

				//maxrect = findLargestContour(segmented, output, showContour);
				maxrect = findLargestContour(segmented, outputTemp, showContour);

				//search in output all the black places (pants) and 
				//see if there's a hole in that pixel on segmentedValidationHoles
				//but firsts do a copy because maybe it doesn't work
				if( !frame_copyTemp ) 
					frame_copyTemp = cvCreateImage( cvSize(frame->width,frame->height),IPL_DEPTH_8U, frame->nChannels );
				cvCopy(frame_copy,frame_copyTemp);
				seqHolesEnd = findHoles(
						outputTemp, segmented, foundHoles, frame_copy,  
						maxrect, hipOld, kneeOld, toeOld, font);

				//if hip or toe is touching a border of the image
				//then will not be included in largest contour
				//then use findHolesSkin to find points
				CvPoint myHip = pointToZero();
				CvPoint myKnee = pointToZero();
				CvPoint myToe = pointToZero();
				myHip = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 0); 
				myKnee = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 1); 
				myToe = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 2 ); 

				//validation uses always black contour
				//but black only markers can change to skin related if has problems with the contour
				if( programMode == validation || (! pointIsNull(myHip) && ! pointIsNull(myKnee) && ! pointIsNull(myToe))) {
					cvCopy(segmentedValidationHoles, output);
					if(! usingContour) {
						usingContour = true;
						gui = cvLoadImage("kneeAngle_black_contour.png");
					}
				}
				else {
					usingContour = false;
					gui = cvLoadImage("kneeAngle_black.png");
					cvCopy(frame_copyTemp,frame_copy);

//testing stuff
cvShowImage("threshold",output);
//cvShowImage("toClick", frame_copy);
imageGuiResult(gui, "going", font);
//printf("threshold :%d\n", threshold);
//printf("thresholdLC :%d\n", thresholdLargestContour);
//cvWaitKey(500); //to allow messages be shown
					seqHolesEnd = findHolesSkin(output, frame_copy, hipMarked, kneeMarked, toeMarked, font);
imageGuiResult(gui, "returned", font);
//cvWaitKey(500); //to allow gui image be shown
				}
			}
			cvShowImage("threshold", output);


			hipMarked = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 0); 
			kneeMarked = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 1 ); 
			toeMarked = *CV_GET_SEQ_ELEM( CvPoint, seqHolesEnd, 2 ); 
			
			

// kalman 
			measurement_pt = kneeMarked;

			//cvMatMulAdd(kalman->measurement_matrix, x_k,z_k,z_k);

//			crossPoint(frame_copy, cvPoint(measurement_pt.x -20, measurement_pt.y), YELLOW, BIG); //works
//			crossPoint(frame_copy, cvPoint(prediction_pt.x +20, prediction_pt.y), WHITE, BIG); //0,0
// /kalman 

			crossPoint(frame_copy, hipMarked, GREY, MID);
			crossPoint(frame_copy, kneeMarked, GREY, MID);
			crossPoint(frame_copy, toeMarked, GREY, MID);
			
				
			cvNamedWindow( "toClick", 1 );
			cvShowImage("toClick", frame_copy);


// kalman 
			cvKalmanCorrect(kalman, measurement);

			cvRandSetRange(&rng,0,sqrt(kalman->process_noise_cov->data.fl[0]),0);
			cvRand(&rng, process_noise);			
			cvMatMulAdd(kalman->transition_matrix, measurement, process_noise, measurement);
// /kalman 



			//if frame before nothing was detected (maybe first frame or after a forward or jump
			//OR a point in this frame was not detected
			if(
					(pointIsNull(hipOld) && pointIsNull(kneeOld) && pointIsNull(toeOld)) ||
					(pointIsNull(hipMarked) || pointIsNull(kneeMarked) || pointIsNull(toeMarked)) ) 
			{
				// force a playPause and reload frame after
				// then all the code of key mouse interaction will be together at end of loop
				forcePause = true;
				reloadFrame = true;
			}

		} 
			
					
		//store old points that worked
		if(!pointIsNull(hipOld))
			hipOldWorked = hipOld;
		if(!pointIsNull(kneeOld))
			kneeOldWorked = kneeOld;
		if(!pointIsNull(toeOld))
			toeOldWorked = toeOld;

		hipOld = hipMarked;
		kneeOld = kneeMarked;
		toeOld = toeMarked;

		
		/*
		 * 4
		 * PRINT MARKERS RELATED INFO AND DO CALCULATIONS LIKE ANGLE
		 */


		if(programMode == skinOnlyMarkers || programMode == blackOnlyMarkers || programMode == validation) 
		{
			if(pointIsNull(hipMarked) || pointIsNull(kneeMarked) || pointIsNull(toeMarked))
				thetaMarked = -1;
			else {
				thetaMarked = findAngle2D(hipMarked, toeMarked, kneeMarked);
				
				//store minThetaMarked if not marked to reload (bad detection, or first frame)
				if(!reloadFrame && thetaMarked < minThetaMarked) {
					minThetaMarked = thetaMarked;
					cvCopy(frame_copy,result);
					storeResultImage = true;
					lowestAngleFrame = framesCount;

					//store thresholds	
					thresholdAtMinAngle = threshold;
					thresholdROIHAtMinAngle = thresholdROIH;
					thresholdROIKAtMinAngle = thresholdROIK;
					thresholdROITAtMinAngle = thresholdROIT;
				}


				if(programMode == validation) {
					cvRectangle(frame_copy,
							cvPoint(maxrect.x,maxrect.y),
							cvPoint(maxrect.x + maxrect.width, maxrect.y + maxrect.height),
							CV_RGB(255,0,0),1,1);

					//assign validationRect data if maxs or mins reached
					if(maxrect.height > validationRectHMax)
						validationRectHMax = maxrect.height;
					if(maxrect.height < validationRectHMin) {
						validationRectHMin = maxrect.height;
						//store angle at validationRectHMin
						//and see how differs from minimum angle
						validationRectHMinThetaMarked = thetaMarked;
					}

					if(maxrect.width > validationRectWMax)
						validationRectWMax = maxrect.width;
					if(maxrect.width < validationRectWMin)
						validationRectWMin = maxrect.width;
	
				}




				/*
				 * NOT doing 3D calculations now
				
				 
				CvPoint HT;
				HT.y = kneeMarked.y;
				HT.x = hipMarked.x;

				
				 upLegMarkedDist = getDistance(hipMarked, kneeMarked);
				if(upLegMarkedDist > upLegMarkedDistMax)
					upLegMarkedDistMax = upLegMarkedDist;
				downLegMarkedDist = getDistance(toeMarked, kneeMarked);
				if(downLegMarkedDist > downLegMarkedDistMax)
					downLegMarkedDistMax = downLegMarkedDist;
				
					double kneeZetaSide = sqrt( pow(upLegMarkedDistMax,2) - pow(upLegMarkedDist,2) );
				double htKneeMarked = getDistance (HT, kneeMarked);

				double thetaABD = (180.0/M_PI)*atan( (double) kneeZetaSide / htKneeMarked );

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
					cvShowImage("threshold",output);

					printf("%d;%d;%d;%d;%d;%d;%d;%.2f;%.2f;%.2f %d;%d %.2f;%.2f\n", 
							framesCount, 
							hipMarked.x, frame->height - hipMarked.y,
							kneeMarked.x, frame-> height - kneeMarked.y,
							toeMarked.x, frame->height - toeMarked.y,
							thetaMarked, thetaABD, thetaRealFlex,
							upLegMarkedDist, upLegMarkedDistMax, 
							kneeZetaSide, htKneeMarked);
				}

				printOnScreen(frame_copy, font, CV_RGB(255,255,255), labelsAtLeft,
						framesCount, threshold, 
						(double) upLegMarkedDist *100 /upLegMarkedDistMax, 
						(double) downLegMarkedDist *100 /downLegMarkedDistMax,
						thetaMarked, minThetaMarked,
						thetaABD, thetaRealFlex, minThetaRealFlex
					     );
				 */
			
				/*	
				printf("%d;%d;%d;%d;%d;%d;%d;%.2f;%d;%d;%d;%d\n", 
						framesCount, 
						hipMarked.x, frame->height - hipMarked.y,
						kneeMarked.x, frame-> height - kneeMarked.y,
						toeMarked.x, frame->height - toeMarked.y,
						thetaMarked,
						threshold, thresholdROIH, thresholdROIK, thresholdROIT
				      );
				      */
				
				printOnScreen(frame_copy, font, CV_RGB(255,255,255), labelsAtLeft,
						framesCount, 
						hipMarked.x, frame->height - hipMarked.y,
						kneeMarked.x, frame-> height - kneeMarked.y,
						toeMarked.x, frame->height - toeMarked.y,
						thetaMarked, minThetaMarked,
						threshold, thresholdROIH, thresholdROIK, thresholdROIT,
						thresholdROISizeH, thresholdROISizeK, thresholdROISizeT,
						thresholdLargestContour, usingContour);
			
				//this stores image with above printOnScreen data	
				if(storeResultImage) {
					cvCopy(frame_copy,result);
					storeResultImage = false;
				}


				/*
				   if( (programMode == validation || programMode == blackWithoutMarkers)
				   && foundAngle) {
				   */
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
				/*
				   framesDetected ++;
				   }
				   */


				/*
				   if(programMode == validation || programMode == blackWithoutMarkers)
				   cvShowImage("result",frame_copy);
				   */


				//Finds the minimum angle between Hip to Knee line and Knee to Toe line
				/*
				if(thetaRealFlex == minThetaRealFlex) {
					cvCopy(frame_copy,result);
					lowestAngleFrame = framesCount;
				}
				*/
				
				cvShowImage("toClick", frame_copy);
				//cvShowImage("threshold",output);
				
				//exit if we are going up and soon jumping.
				//toe will be lost
				//detected if minThetaMarked is littler than thetaMarked, when thetaMarked is big
				if(thetaMarked > 140 && 
						minThetaMarked +10 < thetaMarked &&
						askForMaxFlexion)
				{
					imageGuiResult(gui, "Min flex before. End?. 'y'es, 'n'o, 'N'ever", font);
					int option = optionAccept(true);	
					eraseGuiResult(gui, true);
					if(option==YES) {
						printf("\ntm: %f, mtm: %f, frame: %d\n", thetaMarked, minThetaMarked, framesCount);
						shouldEnd = true;
					} else if(option==NEVER)
						askForMaxFlexion = false;
				}
			}
			cvShowImage("threshold",output);
		}
				   

//		if(programMode == validation || programMode == blackWithoutMarkers)
//      			cvShowImage("result",frame_copy);


		CvPoint hipExpected;
		CvPoint kneeExpected;
		CvPoint toeExpected;

		/*
		 * 5 a
		 * IF BLACKANDMARKERS MODE, FIND POINTS
		 * UNUSED NOW BECAUSE WE ARE USING ONLY RECTANGLE
		 */

		/*
		if(programMode == validation || programMode == blackWithoutMarkers) 
		{
			CvPoint hipPointBack;
			CvPoint kneePointBack;
			CvPoint kneePointFront;

//			cvWaitKey(0);  aqui:

			hipPointBack = findHipPoint(output,maxrect);

//			cvWaitKey(0);  abans

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
			int toeMinWidth;
			if(kneeWidthAtExtension == 0)
				toeMinWidth = findWidth(output, kneePointFront, false) / 2;
			else
				toeMinWidth = kneeWidthAtExtension / 2;
			CvPoint toeExpected = findToePoint(output,maxrect,kneePointFront.x,kneePointFront.y, toeMinWidth);
			
			
			crossPoint(frame_copy, hipPointBack, RED, MID);
			crossPoint(frame_copy, hipExpected, BLUE, MID);
			crossPoint(frame_copy, kneePointFront, GREY, MID);
			crossPoint(frame_copy, kneePointBack, GREY, MID);
			crossPoint(frame_copy, toeExpected, GREEN, MID);
			cvShowImage("result",frame_copy);


			foundAngle = false;
			if(kneeMinWidth == 0)
				kneeMinWidth = kneePointFront.x - hipPointBack.x;
			else {
				if((double)(kneePointFront.x- hipPointBack.x) > 1.15*kneeMinWidth && //or 1.05 or 1.15
						upperSimilarThanLower(hipExpected, kneePointFront, toeExpected)
				  )
						//this is for validation
						//&& !pointIsNull(hipMarked) && !pointIsNull(kneeMarked) && 
						//!pointIsNull(toeMarked))
				{
					if(foundAngleOneTime) {
						foundAngle = true;
					} 
					else if(extensionDoIt && extensionCopyDone) 
					{
						// 
						// first time, confirm we found knee ok (and angle)
						// maybe is too early
						//
						crossPoint(frame_copy, kneePointFront, GREY, MID);
						crossPoint(frame_copy, kneePointBack, GREY, MID);

						cvShowImage("result",frame_copy);
						imageGuiResult(gui, "knee found. Accept? 'n'o, 'y'es", font);
					
						int option = optionAccept(false);	
						eraseGuiResult(gui, true);

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
				

							if(validation) {	
								//
								// now print differences between:
								// CvPoint(kneeCenterExtX, kneePointFront.y) and kneeMarkedAtEXtension
								//
								printf("marked at ext: x: %d, y: %d\n", kneeMarkedAtExtension.x, kneeMarkedAtExtension.y); //debug

								//see the % of rectangle where kneeMarked is (at extension)
								double kneeMarkedXPercent = 100 * (double)(kneeMarkedAtExtension.x - rectExt.x) / rectExt.width;
								double kneeMarkedYPercent = 100 * (double)(kneeMarkedAtExtension.y - rectExt.y) / rectExt.height;
							
								cvLine(extension,
										kneeMarkedAtExtension,
										cvPoint(kneeCenterExtX, kneeCenterExtY),
										WHITE,1,1);
								
								printf("knee diff: x: %.1f (%.1f%%), y: %.1f (%.1f%%)\n", 
										kneeMarkedAtExtension.x - kneeCenterExtX,
										kneeMarkedXPercent - kneeCenterExtXPercent,
										kneeMarkedAtExtension.y - kneeCenterExtY,
										kneeMarkedYPercent - kneeCenterExtYPercent);	


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
							}
							
							cvRectangle(extension, 
									cvPoint(rectExt.x, rectExt.y), 
									cvPoint(rectExt.x + rectExt.width, rectExt.y + rectExt.height),
									MAGENTA,1,8,0
									);
							crossPoint(extension,cvPoint(rectExt.x + rectExt.width, kneePointFront.y), WHITE, MID);
							crossPoint(extension,cvPoint(rectExt.x, kneePointBack.y), WHITE, MID);

							
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
							
							showScaledImage(extension, "Extension frame");
						} else {
							foundAngle = false;
							
							if(option==FORWARD) {
								forward = true;
								printf("forwarding ...\n");
							} else if(option==FASTFORWARD) {
								fastForward = true;
								printf("fastforwarding ...\n");
							} else if(option==BACKWARD) {
								backward = true;

								//try to go to previous (backwardspeed) frame
								cvSetCaptureProperty( capture, CV_CAP_PROP_POS_FRAMES, 
										framesCount - backwardSpeed -1 );
									
								imageGuiResult(gui, "Backwarding...", font);
								continue;
							} else if(option==QUIT) {
								shouldEnd = true;
								printf("exiting ...\n");
							}
						}

						//if extensionDoIt == false, nothing of above is done
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
				int kneeHeight = kneeWidth; //kneeHeight can be 0, best to use kneeWidth for percent, is more stable
				int kneeHeightBoxDown = ( (kneePointFront.y + kneePointBack.y) /2 ) - (kneeHeight /2);
				
				cvRectangle(frame_copy, 
						cvPoint(kneePointBack.x, kneeHeightBoxDown), 
						cvPoint(kneePointBack.x + kneeWidth, kneeHeightBoxDown + kneeHeight),
						MAGENTA,1,8,0);

				if(validation) {
					int kneeMarkedPosX = kneeWidth - (kneeMarked.x - kneePointBack.x);
					double kneeMarkedPercentX = (double) kneeMarkedPosX * 100 / kneeWidth;
					int kneeMarkedPosY = kneeHeight - (kneeMarked.y - kneeHeightBoxDown);
					double kneeMarkedPercentY = (double) kneeMarkedPosY * 100 / kneeHeight;
					
				}

				//
				//if(pointIsNull(hipMarked) || pointIsNull(kneeMarked) || pointIsNull(toeMarked))
				//	thetaMarked = -1;
				//else
				//	thetaMarked = findAngle2D(hipMarked, toeMarked, kneeMarked);
				//	

				// ------------ toe stuff ----------

				//
				// don't find width of toe for each photogramme
				// do it only at first, because if at any photogramme, as flexion advances, 
				// knee pointfront is an isolate point at the right of the lowest part of the pants
				// because the back part of kneepoint has gone up
				//

				//if(toePointWidth == -1) 
					toePointWidth = findWidth(output, toeExpected, false);
				crossPoint(frame_copy, toeExpected, GREY, MID);

				thetaExpected = findAngle2D(hipExpected, toeExpected, kneeExpected);
				
				
				//printf("%d;%.2f;%.2f;%.2f\n", framesCount, thetaMarked, kneeMarkedPercentX, kneeMarkedPercentY);
				double angleBack =findAngle2D(hipPointBack, cvPoint(toeExpected.x - toePointWidth, toeExpected.y), kneePointBack); 
				//printf("%d;%.2f;%.2f;%.2f\n", framesCount, angleBack, kneeMarkedPercentX, kneeMarkedPercentY);
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

				//
				// if we never have found the angle, 
				// and this maxrect is wider than previous maxrect (flexion started)
				// then capture previous image to process knee at extension search
				//
				if(extensionDoIt && ! foundAngleOneTime && ! extensionCopyDone) {
					cvShowImage("result",frame_copy);
					imageGuiResult(gui, "Extension copy. Accept? 'n'o, 'y'es", font);
					int option = optionAccept(false);
					eraseGuiResult(gui, true);

					if(option==YES) {
						cvCopy(frame_copy, extension);
						
						//cvCopy(segmented, extensionSeg);
						cvCopy(output, extensionSeg);

						//cvCopy(segmentedValidationHoles, extensionSegHoles);
						//printf("\nhere: x: %d, y: %d\n", kneeMarked.x, kneeMarked.y);
						if(validation) {
							kneeMarkedAtExtension = kneeMarked;
							hipMarkedAtExtension = hipMarked;
						}
							
						hipPointBackAtExtension = hipPointBack;

						kneeWidthAtExtension = findWidth(output, kneePointFront, false);

						showScaledImage(extension, "Extension frame");
						
						extensionCopyDone = true;
					} else {
						if(option==FORWARD) {
							forward = true;
							printf("forwarding ...\n");
						} else if(option==FASTFORWARD) {
							fastForward = true;
							printf("fastforwarding ...\n");
						} else if(option==BACKWARD) {
							backward = true;
							
							//try to go to previous (backwardspeed) frame
							cvSetCaptureProperty( capture, CV_CAP_PROP_POS_FRAMES, 
									framesCount - backwardSpeed -1 );
							
							imageGuiResult(gui, "Backwarding...", font);
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
		*/
		
		/*
		 * 5 b
		 * IF BLACKANDMARKERS MODE, FIND RECTANGLE
		 */

		if(programMode == validation || programMode == blackWithoutMarkers) 
		{
			//print height of rectangle and thetaMarked
			fprintf(fdatapre, "%d;%f\n", maxrect.height, thetaMarked);
		}


//		cvWaitKey(0);

		/* 
		 * 6
		 * WAIT FOR KEYS OR MANAGE MOUSE INTERACTION
		 */

		int myDelay = playDelay;
		if(foundAngle)
			myDelay = playDelayFoundAngle;

		key = (char) cvWaitKey(myDelay);

//		printf("mc: %d ", mouseClicked);  

		if(mouseClicked == quit || key == 27 || key == 'q') // 'ESC'
			shouldEnd = true;

		if (mouseClicked == FORWARD) { 
			forward = true;
			imageGuiResult(gui, "Forwarding...", font);
			cvWaitKey(50); //to print above message
		} else if (mouseClicked == FASTFORWARD) { 
			fastForward = true;
			imageGuiResult(gui, "FastForwarding...", font);
			cvWaitKey(50); //to print above message
		} else if (mouseClicked == BACKWARD) {
			backward = true;

			//try to go to previous (backwardspeed) frame
			cvSetCaptureProperty( capture, CV_CAP_PROP_POS_FRAMES, 
					framesCount - backwardSpeed -1 );

			imageGuiResult(gui, "Backwarding...", font);
			cvWaitKey(50); //to print above message
		//} else if (key == 'j' || key == 'J') {
			/*
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
			imageGuiResult(gui, "Jumping...", font);
			*/
		//}
		} 
		//threshold of large contour
		else if( mouseClicked == TCONTOURMORE || mouseClicked == TCONTOURLESS) 
			forcePause = true;
		else if(mouseClicked == BACKTOCONTOUR) 
			forcePause = true;
		//thresholds of points
		else if(mouseClicked == TGLOBALMORE || mouseClicked == TGLOBALLESS ||
			mouseClicked == THIPMORE || mouseClicked == THIPLESS ||
			mouseClicked == TKNEEMORE || mouseClicked == TKNEELESS ||
			mouseClicked == TTOEMORE || mouseClicked == TTOELESS ||
			mouseClicked == TGLOBALMORE || mouseClicked == TGLOBALLESS ||
			mouseClicked == SHIPMORE || mouseClicked == SHIPLESS ||
			mouseClicked == SKNEEMORE || mouseClicked == SKNEELESS ||
			mouseClicked == STOEMORE || mouseClicked == STOELESS)
		{
			//if a threshold button is pushed, force a pause
			forcePause = true;
		}
		else if(key == 'v') {
			forcePause = true;
			mouseClicked = MINIMUMFRAMEVIEW;
		}
		else if(key == 'd') {
			forcePause = true;
			mouseClicked = MINIMUMFRAMEDELETE;
		}
		
		//pause is also forced on skin if a marker is not ok
		if (mouseClicked == PLAYPAUSE || key == 'p' || forcePause) 
		{
			if(!forcePause)
				mouseClicked = UNDEFINED;  
			if(key == 'p')
				key = NULL;

			forcePause = false;

			imageGuiResult(gui, "Paused.", font);
			//int thresholdROIChanged = TOGGLENOTHING;
			bool thresholdROIChanged = false;
					
			int mult;

			bool done = false;
			do {
				key = (char) cvWaitKey(50);
				//cvWaitKey(50);
			
				if(mouseMultiplier)
					mult = 10;
				else
					mult = 1;

				if (mouseClicked == PLAYPAUSE || key == 'p') 
					done = true;
		
				else if(mouseClicked == quit || key == 27 || key == 'q') {
					done = true;
					shouldEnd = true;
				}
				
				else if(mouseClicked == BACKWARD) { 
					backward = true;
					//try to go to previous (backwardspeed) frame
					cvSetCaptureProperty( capture, CV_CAP_PROP_POS_FRAMES, 
						framesCount - backwardSpeed -1 );

					imageGuiResult(gui, "Backwarding...", font);
					done = true;
				}
			
				else if(mouseClicked == FORWARDONE) { 
					forward = true;
					forwardCount = forwardSpeed -1; //this makes advance only one frame
					imageGuiResult(gui, "Forward one", font);
					done = true;
				}

				else if(mouseClicked == FORWARD) { 
					forward = true;
					imageGuiResult(gui, "Forwarding...", font);
					done = true;
				}

				else if(mouseClicked == FASTFORWARD) { 
					fastForward = true;
					imageGuiResult(gui, "FastForwarding...", font);
					done = true;
				}
				
				else if(mouseClicked == MINIMUMFRAMEVIEW || key == 'v') {
					cvShowImage("Minimum Frame", result);
					imageGuiResult(gui, "Shown minimum frame. Paused.", font);
				}
				else if(mouseClicked == MINIMUMFRAMEDELETE || key == 'd') { 
					minThetaMarked = 360;
					cvShowImage("Minimum Frame", result);
					imageGuiResult(gui, "Deleted stored angle. Paused.", font);
				}
		
				
				else if(mouseClicked == ZOOM || key == 'z') {
					if(zoomed) {
						eraseGuiMark(gui, ZOOM);
						cvDestroyWindow("zoomed");
						cvReleaseImage(&imgZoom);
						zoomed = false;
						cvSetMouseCallback( "toClick", on_mouse_mark_point, 0 );
					} else {
						toggleGuiMark(gui, ZOOM);
						imgZoom = zoomImage(frame_copy);
						cvNamedWindow( "zoomed", 1 );
						cvShowImage("zoomed", imgZoom);
						zoomed = true;
						cvSetMouseCallback( "zoomed", on_mouse_mark_point, 0 );
					}
					mouseClicked = UNDEFINED;  
				}

				else if(mouseClicked == THIPMORE || mouseClicked == THIPLESS) {
					if(pointIsNull(hipMarked)) {
						//force mark first
						mouseClicked = HIPMARK;
					} else {
						if(thresholdROIH == -1) //undefined
							thresholdROIH = threshold;

						if(mouseClicked == THIPMORE) 
							thresholdROIH += thresholdInc * mult;
						else {
							thresholdROIH -= thresholdInc * mult;
							if(thresholdROIH < 0)
								thresholdROIH = 0;
						}
						thresholdROIChanged = true;
						mouseClicked = UNDEFINED;  
					}
				}

				else if(mouseClicked == TKNEEMORE || mouseClicked == TKNEELESS) {
					if(pointIsNull(kneeMarked)) {
						//force mark first
						mouseClicked = KNEEMARK;
					} else {
						if(thresholdROIK == -1) //undefined
							thresholdROIK = threshold;

						if(mouseClicked == TKNEEMORE) 
							thresholdROIK += thresholdInc * mult;
						else {
							thresholdROIK -= thresholdInc * mult;
							if(thresholdROIK < 0)
								thresholdROIK = 0;
						}
						thresholdROIChanged = true;
						mouseClicked = UNDEFINED;  
					}
				}

				else if(mouseClicked == TTOEMORE || mouseClicked == TTOELESS) {
					if(pointIsNull(toeMarked)) {
						//force mark first
						mouseClicked = TOEMARK;
					} else {
						if(thresholdROIT == -1) //undefined
							thresholdROIT = threshold;

						if(mouseClicked == TTOEMORE) 
							thresholdROIT += thresholdInc * mult;
						else {
							thresholdROIT -= thresholdInc * mult;
							if(thresholdROIT < 0)
								thresholdROIT = 0;
						}
						thresholdROIChanged = true;
						mouseClicked = UNDEFINED;  
					}
				}

				else if(mouseClicked == SHIPMORE || mouseClicked == SHIPLESS) {
					if(pointIsNull(hipMarked)) {
						//force mark first
						mouseClicked = HIPMARK;
					} else {
						if(mouseClicked == SHIPMORE) 
							thresholdROISizeH += thresholdROISizeInc;
						else {
							thresholdROISizeH -= thresholdROISizeInc;
							if(thresholdROISizeH < thresholdROISizeMin)
								thresholdROISizeH = thresholdROISizeMin;
						}
						thresholdROIChanged = true;
						mouseClicked = UNDEFINED;  
					}
				}
				
				else if(mouseClicked == SKNEEMORE || mouseClicked == SKNEELESS) {
					if(pointIsNull(kneeMarked)) {
						//force mark first
						mouseClicked = KNEEMARK;
					} else {
						if(mouseClicked == SKNEEMORE) 
							thresholdROISizeK += thresholdROISizeInc;
						else {
							thresholdROISizeK -= thresholdROISizeInc;
							if(thresholdROISizeK < thresholdROISizeMin)
								thresholdROISizeK = thresholdROISizeMin;
						}
						thresholdROIChanged = true;
						mouseClicked = UNDEFINED;  
					}
				}
				
				else if(mouseClicked == STOEMORE || mouseClicked == STOELESS) {
					if(pointIsNull(toeMarked)) {
						//force mark first
						mouseClicked = TOEMARK;
					} else {
						if(mouseClicked == STOEMORE) 
							thresholdROISizeT += thresholdROISizeInc;
						else {
							thresholdROISizeT -= thresholdROISizeInc;
							if(thresholdROISizeT < thresholdROISizeMin)
								thresholdROISizeT = thresholdROISizeMin;
						}
						thresholdROIChanged = true;
						mouseClicked = UNDEFINED;  
					}
				}
		
				//if we are in blackOnlyMarkers but we cannot find three points in contour
				//then we are in usingContour = false
				//a mode like skinOnlyMarkers, but will return to contour if find points on contour next frame
				//or if userwanted to play with threshold:
				else if(mouseClicked == BACKTOCONTOUR) {
					usingContour = true;
					gui = cvLoadImage("kneeAngle_black_contour.png");
					cvShowImage("gui", gui);
					mouseClicked = UNDEFINED;  

					cvThreshold(gray,segmentedValidationHoles,thresholdLargestContour,thresholdMax,CV_THRESH_BINARY_INV);
					cvThreshold(gray,segmented,thresholdLargestContour,thresholdMax,CV_THRESH_BINARY_INV);

					maxrect = findLargestContour(segmented, outputTemp, showContour);
					//frame_copyTemp = cvCreateImage( cvSize(frame->width,frame->height),IPL_DEPTH_8U, frame->nChannels );
					findHoles(
							outputTemp, segmented, foundHoles, frame_copyTemp,  
							maxrect, hipOld, kneeOld, toeOld, font);

					cvCopy(segmentedValidationHoles, output);
					cvShowImage("threshold", output);
				}

				else if( mouseClicked == TCONTOURMORE || mouseClicked == TCONTOURLESS) {
					if(mouseClicked == TCONTOURMORE)
						thresholdLargestContour ++;
					else
						thresholdLargestContour --;
						
					eraseGuiResult(gui, true);
					sprintf(label, "Threshold: %d", thresholdLargestContour);
					imageGuiResult(gui, label, font);

					cvThreshold(gray,segmentedValidationHoles,thresholdLargestContour,thresholdMax,CV_THRESH_BINARY_INV);
					cvCopy(segmentedValidationHoles, output);
					cvShowImage("threshold", output);
					mouseClicked = UNDEFINED;  
				}
				
				else if (mouseClicked == TGLOBALMORE || mouseClicked == TGLOBALLESS || thresholdROIChanged) {  
					if (mouseClicked == TGLOBALMORE)  
						threshold += thresholdInc * mult;
					else if (mouseClicked == TGLOBALLESS) {
						threshold -= thresholdInc * mult;
						if(threshold < 0)
							threshold = 0;
					}
					mouseClicked = UNDEFINED;  
					mouseMultiplier = false;
		
					if(programMode == skinOnlyMarkers || programMode == blackOnlyMarkers || programMode == validation) {
						sprintf(label, "Threshold: %d (%d,%d,%d) (%d,%d,%d)", 
								threshold, 
								thresholdROIH, thresholdROIK, thresholdROIT, 
								thresholdROISizeH, thresholdROISizeK, thresholdROISizeT);
						imageGuiResult(gui, label, font);

						cvThreshold(gray, output, threshold, thresholdMax,CV_THRESH_BINARY_INV);
						
						if(thresholdROIH != -1)
							output = changeROIThreshold(gray, output, hipMarked, 
									thresholdROIH, thresholdMax, thresholdROISizeH);
						if(thresholdROIK != -1)
							output = changeROIThreshold(gray, output, kneeMarked, 
									thresholdROIK, thresholdMax, thresholdROISizeK);
						if(thresholdROIT != -1)
							output= changeROIThreshold(gray, output, toeMarked, 
									thresholdROIT, thresholdMax, thresholdROISizeT);

						cvShowImage("threshold", output);
					}
					else {		
						cvThreshold(gray,segmentedValidationHoles, threshold, thresholdMax,CV_THRESH_BINARY_INV);
						//create the largest contour image (stored on temp)
						cvThreshold(gray,segmented,threshold,thresholdMax,CV_THRESH_BINARY_INV);
						maxrect = findLargestContour(segmented, output, showContour);

						if(validation)
							updateHolesWin(segmentedValidationHoles);
						
						sprintf(label, "threshold: %d", threshold);
						imageGuiResult(gui, label, font);
					}
						
					thresholdROIChanged = false;
				}
					
				//remark bad found or unfound markers	
				if(
						mouseClicked == HIPMARK || key == 'h' ||
						mouseClicked == KNEEMARK || key == 'k' ||
						mouseClicked == TOEMARK || key == 't'
						) {
						
					int myMark = TOGGLENOTHING ;
					const char * Id = "";
					CvPoint markedBefore;
					
					if(mouseClicked == HIPMARK || key == 'h' ) {
						myMark = TOGGLEHIP;
						markedBefore = hipMarked;
						markedMouse = hipMarked;
						Id = "H";
						imageGuiResult(gui, "Mark Hip on toClick window. Or 'l': last", font);
					} else if(mouseClicked == KNEEMARK || key == 'k' ) {
						myMark = TOGGLEKNEE;
						markedBefore = kneeMarked;
						markedMouse = kneeMarked;
						Id = "K";
						imageGuiResult(gui, "Mark Knee on toClick window. Or 'l': last", font);
					} else {
						myMark = TOGGLETOE;
						markedBefore = toeMarked;
						markedMouse = toeMarked;
						Id = "T";
						imageGuiResult(gui, "Mark Toe on toClick window. Or 'l': last", font);
					}
						
					toggleGuiMark(gui, myMark);
					forceMouseMark = myMark;
			
					cvSetMouseCallback( "toClick", on_mouse_mark_point, 0 );
					mouseClicked = UNDEFINED;  
					bool doneMarking = false;
					bool cancelled = false;
					do {
						key = (char) cvWaitKey(playDelay);
						if(!pointIsEqual(markedBefore, markedMouse)) {
							crossPoint(frame_copy, markedMouse, MAGENTA, BIG);
							imagePrint(frame_copy, cvPoint(markedMouse.x -20, markedMouse.y), Id, font, MAGENTA);
							cvShowImage("toClick", frame_copy);
							if(zoomed) {
								crossPoint(imgZoom, cvPoint(zoomScale * markedMouse.x, zoomScale * markedMouse.y), MAGENTA, BIG);
								imagePrint(imgZoom, cvPoint(zoomScale * (markedMouse.x -20), zoomScale * markedMouse.y), Id, font, MAGENTA);
								cvShowImage("zoomed", imgZoom);
							}
							doneMarking = true;
						}
				
						else if(mouseClicked == ZOOM || key == 'z') {
							if(zoomed) {
								eraseGuiMark(gui, ZOOM);
								cvDestroyWindow("zoomed");
								cvReleaseImage(&imgZoom);
								zoomed = false;
								cvSetMouseCallback( "toClick", on_mouse_mark_point, 0 );
							} else {
								toggleGuiMark(gui, ZOOM);
								imgZoom = zoomImage(frame_copy);
								cvNamedWindow( "zoomed", 1 );
								cvShowImage("zoomed", imgZoom);
								zoomed = true;
								cvSetMouseCallback( "zoomed", on_mouse_mark_point, 0 );
							}
							mouseClicked = UNDEFINED;  
						}
						//if user want to put mark on last point it was (and then play with threshold)
						else if(key == 'l') {
							if(myMark == TOGGLEHIP) 
								markedMouse = hipOldWorked;
							if(myMark == TOGGLEKNEE) 
								markedMouse = kneeOldWorked;
							if(myMark == TOGGLETOE) 
								markedMouse = toeOldWorked;
							imageGuiResult(gui, "Last marked. Now adjust threshold", font);
							cvWaitKey(500); //to print above message
						}
					
						else if(
								mouseClicked == HIPMARK || key == 'h' ||
								mouseClicked == KNEEMARK || key == 'k' ||
								mouseClicked == TOEMARK || key == 't'
						       ) {
							cancelled = true;
						}
					} while(!doneMarking && !cancelled);
					
					eraseGuiMark(gui, myMark);
					
					bool unZoomAfterClick = true;
					if(unZoomAfterClick && zoomed) {
						eraseGuiMark(gui, ZOOM);
						cvDestroyWindow("zoomed");
						cvReleaseImage(&imgZoom);
						zoomed = false;
						cvSetMouseCallback( "toClick", on_mouse_mark_point, 0 );
					}
					
					if(!cancelled) {
						if(myMark == TOGGLEHIP) {
							hipMarked = markedMouse;
							hipOld = markedMouse;
						} else if(myMark == TOGGLEKNEE) {
							kneeMarked = markedMouse;
							kneeOld = markedMouse;
						} else { 
							toeMarked = markedMouse;
							toeOld = markedMouse;
						}
					}

					eraseGuiResult(gui, true);
					forceMouseMark = TOGGLENOTHING;
					mouseClicked = UNDEFINED;  
				}

			} while (! done);
					
			eraseGuiResult(gui, true);
				
		}

//		if(programMode == validation) 
//			updateHolesWin(segmentedValidationHoles);
		
		
		mouseClicked = UNDEFINED;  

	}

	/*
	 * END OF MAIN LOOP
	 */
		
	imageGuiResult(gui, "Press 'q' to exit.", font);

	//if( (programMode == validation || programMode == blackWithoutMarkers) && foundAngleOneTime) 
	if(programMode == validation || programMode == blackWithoutMarkers) 
	{
		//relative to geometric points:
		//avgThetaDiff = (double) avgThetaDiff / framesDetected;
		//avgThetaDiffPercent = (double) avgThetaDiffPercent / framesDetected;
		//avgKneeDistance = (double) avgKneeDistance / framesDetected;
			
		cvNamedWindow("Minimum Frame",1);
		cvShowImage("Minimum Frame", result);

		/*
		printf("Minimum Frame\n");
		sprintf(label, "minblack minholes    diff diff(%%)");
		sprintf(label, "%8.2f %8.2f [%7.2f %7.2f]", minThetaExpected, minThetaMarked, 
				minThetaMarked-minThetaExpected, relError(minThetaExpected, minThetaMarked));
		printf("%s\n" ,label);
		*/
	

		fclose(fdatapre);
		
		fprintf(fheader, "BoxHMax;BoxHMin;BoxWMax;BoxWMin;BoxHMaxWMin;AngleBoxHMin;AngleMin\n%d;%d;%d;%d;%f;%f;%f",
			validationRectHMax, validationRectHMin, validationRectWMax, validationRectWMin,
			(double) validationRectHMax / validationRectWMin, validationRectHMinThetaMarked, minThetaMarked);
		fclose(fheader);

		//copy fdatapre in fdatapost but converting box height in %
		if((fdatapre=fopen(fdatapreName,"r")) == NULL){
			printf("Error, no se puede leer: %s\n",fdatapreName);
			fclose(fdatapre);
			exit(0);
		}

		bool fileEnd = false;
		int endChar;
		int i=0;
		float height;
		float angle;
		while(!fileEnd) {
			fscanf(fdatapre,"%f;%f\n",&height, &angle);
			fprintf(fdatapost, "%f;%f\n", 100 * height / validationRectHMax, angle);
			endChar = getc(fdatapre);
			if(endChar == EOF) 
				fileEnd = true;
			else
				ungetc(endChar, fdatapre);

			//do not continue if we copied frame with minimum angle (only store 'going-down' phase)
//			if(angle == minThetaMarked)
//				fileEnd = true;
		}

		fclose(fdatapre);
		fclose(fdatapost);

	}
	else {
		//printf("*** Result ***\nMin angle: %.2f, lowest angle frame: %d\n", minThetaMarked, lowestAngleFrame);
		cvNamedWindow("Minimum Frame",1);
		cvShowImage("Minimum Frame", result);
		printf("MIN: %d;%.2f\n", lowestAngleFrame, minThetaMarked);
	}
	

	do {
		key =  (char) cvWaitKey(0);
	} while (key != 'q' && key != 'Q');
					
			

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
	int key = NULL;

	cvSetMouseCallback( "gui", on_mouse_gui_menu, 0 );

	do {
		key = (char) cvWaitKey(100);
		switch ( key ) {
			case 27: mouseClicked = quit; 			break; //27: ESC
			case 'q': mouseClicked = quit; 			break;
			case '1': mouseClicked = validation; 		break;
			case '2': mouseClicked = blackWithoutMarkers; 	break;
			case '3': mouseClicked = skinOnlyMarkers; 	break;
			case '4': mouseClicked = blackOnlyMarkers; 	break;
		}
	} while (mouseClicked == undefined);

	if(mouseClicked == quit)
		exit(0);

	eraseGuiWindow(gui);
	
	return mouseClicked;
}
				
