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
 * Coded by (v.1.0):
 * Sharad Shankar & Onkar Nath Mishra
 * http://www.logicbrick.com/
 * 
 * Updated by:
 * Xavier de Blas 
 * xaviblas@gmail.com
 *
 * version: 1.1
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
using namespace std;

//config variables
bool showContour = true;
bool debug = false;
int playDelay = 5; //milliseconds between photogrammes wen playing. Used as a waitkey. 
			//not put values lower than 5 or the enter when executing will be the first pause
			//eg: 5 (fast) 1000 (one second each photogramme)
int playDelayFoundAngle = 150; //as above, but used when angle is found. 
				//Useful to see better the detected angle when something is detected
				//recommended values: 50 - 200


/*
 * takes as input arguement the bounding rectangle of the largest contour and the image containing the bounding rectangle
 * Calculates the hip point
 * Hip point is the x coordinate of the white pixel having minimum x coordinate in the above bounding rectangle
 * Returns the coordinate of the hip point
 */
CvPoint FindHipPoint(IplImage* img,CvRect roirect)
{
	CvPoint pt;
	pt.x =0;pt.y=0;
	int starty = roirect.y;
	int endy = starty + roirect.height*2/3; /* meu: why 2/3 */
	CvMat *srcmat,src_stub;
	srcmat = cvGetMat(img,&src_stub);
	uchar *srcdata = srcmat->data.ptr;
	int width = img->width;
	int minx = img->width;
	int miny = img->height;

	for(int y=starty;y<endy;y++)
	{
		uchar *srcdataptr = srcdata + y*img->width;
		for(int x=0;x<width;x++)
		{
			if(srcdataptr[x] > 0)
			{
				if(x<minx)
				{
					minx = x;
					miny = y;
				}
				break;
			}

		}
	}
	pt.x = minx;
	pt.y = miny;

	return pt;
}

/*
 * takes as input arguement the bounding rectangle of the largest contour,the image containing the bounding rectangle and the y coordinate of the hip point
 * Calculates the knee point
 * Knee point is a white pixel below the hip point and having maximum x coordinate in the bounding box
 * Returns the coordinate of the knee point
 */
CvPoint FindKneePoint(IplImage *img,CvRect roirect,int starty)
{
	CvPoint pt;
	pt.x = 0; pt.y = 0;
	
	//int endy = roirect.y+roirect.height*9/10; //this is ok if shoes or platform is shown in standup image
	int endy = roirect.y+roirect.height;

	CvMat *srcmat,src_stub;
	srcmat = cvGetMat(img,&src_stub);
	uchar *srcdata = srcmat->data.ptr;
	int width = img->width;
	int maxx = 0;
	int maxy = 0;
	for(int y=starty;y<endy;y++)
	{
		uchar *srcdataptr = srcdata + y*img->width;
		for(int x= width;x>0;x--)
		{
			if(srcdataptr[x] > 0)
			{
				if(x>maxx)
				{
					maxx = x;
					maxy = y;
				}
				break;
			}
		}
	}
	pt.x = maxx;
	pt.y = maxy;

	return pt;
}

/*
 * takes as input arguement the bounding rectangle of the largest contour,the image containing the bounding rectangle and the x and y coordinate of the knee point
 * Calculates the toe point
 * Toe point is a white pixel below the knee point and having minimum x coordinate
 * Returns the coordinate of the hip point
 */
CvPoint FindToePoint(IplImage *img,CvRect roirect,int startx,int starty)
{
	CvPoint pt;
	pt.x = 0; pt.y = 0;
	
	
	/* if foot is in the image, is better to try to avoid it capturing above, if not then capture all
	 * maybe force user to capture without foot, or ask and put a boolean
	 */

	//int endy = roirect.y+roirect.height*9/10; //this is ok if shoes or platform is shown in standup image
	int endy = roirect.y+roirect.height;
	

	CvMat *srcmat,src_stub;
	srcmat = cvGetMat(img,&src_stub);
	uchar *srcdata = srcmat->data.ptr;
	int width = img->width;
	int minx = img->width;
	int miny = img->height;
	for(int y=starty;y<endy;y++)
	{
		uchar *srcdataptr = srcdata + y*img->width;
		for(int x=width;x>0;x--)
		{
			if(srcdataptr[x] > 0)
			{
				if(x>startx)
					break;
				if(x<minx)
				{
					minx = x;
					miny = y;
				}
				break;
			}

		}
	}
	pt.x = minx;
	pt.y = miny;
	return pt;
}

/*
 * takes input argument as the gray form of input frame and a temp image
 * Returns the bounding rectangle of the contour having maximum height
 * Draws the bounding rectangle of the largest contour on temp
 */

CvRect findLargestContour(IplImage* img,IplImage* temp)
{
	CvContourScanner scanner;
	CvSeq *src_contour;
	IplImage *tempcopy = cvCreateImage(cvGetSize(img),IPL_DEPTH_8U,1);
	cvCopy(img,tempcopy);
	CvMemStorage* storage = cvCreateMemStorage(0);
	scanner = cvStartFindContours(img,storage,sizeof(CvContour),CV_RETR_EXTERNAL,CV_CHAIN_APPROX_SIMPLE);
	cvZero(temp);
	CvRect maxrect;
	maxrect.x=0;maxrect.y=0;maxrect.width=0;maxrect.height=0;
	while((src_contour = cvFindNextContour(scanner))!= 0)
	{
		CvRect rect = ((CvContour*)src_contour)->rect;
		if((rect.height) < (img->height)/3)
			continue;
		if(rect.height > maxrect.height)
		{
			maxrect.x = rect.x;
			maxrect.y =	rect.y;
			maxrect.width = rect.width;
			maxrect.height = rect.height;
		}
		cvDrawContours(temp,src_contour,cvScalarAll(255),cvScalarAll(255),0,-1);
	}

	//show temp image (contour) little
	if(showContour) {
		cvNamedWindow("temp",1);
		cvResize(temp, temp, CV_INTER_LINEAR);

		double scale = 4;
		IplImage* tempSmall = cvCreateImage( cvSize( cvRound (img->width/scale), cvRound (img->height/scale)), 8, 1 );
		cvResize( temp, tempSmall, CV_INTER_LINEAR );

		cvShowImage("temp", tempSmall);
	}
	
	cvReleaseMemStorage(&storage);
	cvReleaseImage(&tempcopy);
	return maxrect;
}

CvPoint FixHipPoint1(IplImage* img, CvPoint hip, CvPoint knee)
{
	CvPoint ptHK;
	ptHK.x =0;ptHK.y=0;
	CvMat *srcmat,src_stub;
	srcmat = cvGetMat(img,&src_stub);
	uchar *srcdata = srcmat->data.ptr;
	int width = img->width;

	//find at 3/2 of hip (surely under the hand)
	int y=hip.y*.66 + knee.y*.33;

	uchar *srcdataptr = srcdata + y*img->width;
	int startX = 0;
	int countX = 0;
	bool found = false;
	for(int x=0;x<width;x++)
	{
		if(srcdataptr[x] > 0)
		{
			if(!found) {
				startX = x;
				countX = x;
				found = true;
			}
			countX ++;
		}
	}
	ptHK.x = (startX + countX) /2;
	ptHK.y = y;
	
	return ptHK;
}

CvPoint FixHipPoint2(IplImage* img, int hipFirstY, CvPoint knee, CvPoint ptHK)
{
			
	/* this was hippoint in 1/3 of the leg (close to the hip but just below the hand)
	 * now do a line from knee to this hippoint and cross this line with first hippoint and x axe
	 * we will have the first hippoint but centered on the hip (without problems with the hand)
	 */

	CvPoint kneePrima;
	kneePrima.x = knee.x - ptHK.x; 
	kneePrima.y = knee.y - ptHK.y; 

	/*
	 * y = (kneePrima.y / kneePrima.x) * x + d
	 * x = (kneePrima.x / kneePrima.y) * y - d
	 * d = -x +(kneePrima.x / kneePrima.y) * y
	 */

	double d = -knee.x + ( (kneePrima.x / (double)kneePrima.y) * knee.y);

	/*
	 * x = (kneePrima.x / kneePrima.y) * y - d
	 */
	 
	CvPoint HCenter;
	HCenter.x =0; 
	HCenter.y = hipFirstY;

	HCenter.x = ( (kneePrima.x / (double)kneePrima.y) * HCenter.y ) - d;

	if(debug) {
		printf("hipy(%d) ",hipFirstY);
		printf("knee(%d,%d) ",knee.x, knee.y);
		printf("ptHK(%d,%d) ",ptHK.x, ptHK.y);
		printf("kneePrima(%d,%d) ",kneePrima.x, kneePrima.y);
		printf("HCenter(%d,%d) ",HCenter.x, HCenter.y);
		printf("kneePrima x/y:%.2f ", kneePrima.x / (double)kneePrima.y);
		printf("d:%.1f", d);
		printf("\n");
	}

	return HCenter;
}

/* at first photogramm where knee or foot is detected (it will not be too horizontal) find it's width and use all the time to fix kneex
 * at knee is called only done one time (because in max flexion, the back is line with the knee and there will be problems knowing knee width
 * at foot is called all the time
 */
int FindWidth(IplImage* img, CvPoint kneeOrFoot)
{
	CvMat *srcmat,src_stub;
	srcmat = cvGetMat(img,&src_stub);
	uchar *srcdata = srcmat->data.ptr;
	int width = img->width;

	int y=kneeOrFoot.y;

	uchar *srcdataptr = srcdata + y*img->width;
	int countX = 0;
	for(int x=kneeOrFoot.x-1;srcdataptr[x];x--)
	{
		countX ++;
	}

	return countX;;
}

double getDistance(CvPoint p1, CvPoint p2)
{
	return sqrt((p1.x-p2.x)*(p1.x-p2.x)+(p1.y-p2.y)*(p1.y-p2.y));
}

bool upperSimilarThanLower(CvPoint hipPoint, CvPoint kneePoint, CvPoint footPoint)
{
	double upper = getDistance(kneePoint, hipPoint); 
	double lower = getDistance(kneePoint, footPoint);
	double big = 0; 
	double little = 0;
	
	if(upper > lower) {
		big = upper;
		little = lower;
	} else {
		big = lower;
		little = upper;
	}

	if(debug)
		printf("upper(%.1f), lower(%.1f), big/little (%.2f)\n",upper, lower, big/(double)little);

	//if one is not n times or more bigger than the other
	//consider that if camera hides shoes and a bit more up, 2 times is risky in a maximal flexion
	//consider also that this data is previous to fixing
	double n = 2.5;
	if(big / (double)little < n)
		return true;
	else 
		return false;
}

int main(int argc,char **argv)
{
	//TODO:
	//add args for debug, record to file, speed, fondAngle value, ...
	//currently this variables are global and defined at the top of the file
	
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
	IplImage *frame=0,*frame_copy=0,*gray=0,*segmented=0,*edge=0,*temp=0,*output=0;
	IplImage *result=0;
	int minwidth = 0;
	
	bool foundAngle = false; //found angle on current frame
	bool foundAngleOneTime = false; //found angle at least one time on the video

	double knee2Hip,knee2Foot,hip2Foot,theta;
	string text,angle;
	double mintheta = 360;
	char buffer[15];
	cvNamedWindow("frame",1);

	int kneePointWidth = -1;
	int footPointWidth;

	char *label = new char[30];
	CvFont font;
	int fontLineType = CV_AA; // change it to 8 to see non-antialiased graphics
	cvInitFont(&font, CV_FONT_HERSHEY_COMPLEX, .7, .7, 0.0, 1, fontLineType);
	
	char key;
	bool shouldEnd = false;

	//for(;;)
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
			edge =	cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,1);
			temp = cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,1);
			output = cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,1);
			result = cvCreateImage(cvGetSize(frame),IPL_DEPTH_8U,3);
		}

		cvSmooth(frame_copy,frame_copy,2,5,5);
		cvCvtColor(frame_copy,gray,CV_BGR2GRAY);
		cvThreshold(gray,segmented,30,255,CV_THRESH_BINARY_INV);
		CvRect maxrect = findLargestContour(segmented,output);
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
			
			//fix kneepoint.x at the right 1/3 of the knee width
			kneePoint.x -= kneePointWidth /3;
			cvCircle(frame_copy,kneePoint,3, CV_RGB(255,0,0),1,8,0);


			//fix hipPoint ...
			int hipPointFirstY = hipPoint.y;
			cvCircle(frame_copy,hipPoint,2, CV_RGB(255,0,255),1,8,0);
			
			//... find at 3/2 of hip (surely under the hand) ...
			hipPoint = FixHipPoint1(output, hipPoint, kneePoint);
			cvCircle(frame_copy,hipPoint,2, CV_RGB(255,0,255),1,8,0);

			//... cross first hippoint with the knee-hippoint line to find real hippoint
			hipPoint = FixHipPoint2(output, hipPointFirstY, kneePoint, hipPoint);
			cvCircle(frame_copy,hipPoint,3, CV_RGB(255,0,0),1,8,0);
			
			//find width of knee, only one time and will be used for all the prhotogrammes
			if(kneePointWidth == -1) {
				kneePointWidth = FindWidth(output, kneePoint);
			}

			//find width of foot for each photogramme
			footPointWidth = FindWidth(output, footPoint);
			cvCircle(frame_copy,footPoint,2, CV_RGB(255,0,255),1,8,0);
			
			//fix footpoint.x at the 1/2 of the foot width
			footPoint.x -= footPointWidth /2;
			cvCircle(frame_copy,footPoint,3, CV_RGB(255,0,0),1,8,0);

			//draw 2 main lines
			knee2Foot = getDistance(kneePoint, footPoint);
			knee2Hip = getDistance(kneePoint, hipPoint);
			hip2Foot = getDistance(hipPoint, footPoint);
			theta = (180.0/M_PI)*acos(((knee2Foot*knee2Foot+knee2Hip*knee2Hip)-hip2Foot*hip2Foot)/(2*knee2Foot*knee2Hip));
			cvLine(frame_copy,kneePoint,hipPoint,CV_RGB(0,0,255),1,1);
			cvLine(frame_copy,kneePoint,footPoint,CV_RGB(0,0,255),1,1);
			//Finds the minimum angle between Hip to Knee line and Knee to Toe line
			if(theta < mintheta)
			{
				mintheta = theta;
				cvCopy(frame_copy,result);
			}

			cvRectangle(frame_copy,
					cvPoint(maxrect.x,maxrect.y),
					cvPoint(maxrect.x + maxrect.width, maxrect.y + maxrect.height),
					CV_RGB(255,0,0),1,1);

			//print angles
			sprintf(label, "current: %.1f", theta);
			cvPutText(frame_copy, label, cvPoint(5,frame->height /2),&font,cvScalar(0,0,255));
			sprintf(label,     "min:     %.1f", mintheta);
			cvPutText(frame_copy, label, cvPoint(5,frame->height /2 +30),&font,cvScalar(0,0,255));
		}


		cvShowImage("frame",frame_copy);

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
			cvShowImage("frame",frame_copy);

			key = (char) cvWaitKey(0);
			if(key == 27 || key == 'q' || key == 'Q' ) // 'ESC'
				shouldEnd = true;
		}

	}

	if(foundAngleOneTime) {
		cvNamedWindow("Minimum Frame",1);

		sprintf(label,"Minimum Angle= %.3f",mintheta);
		cvPutText(result, label,cvPoint(25,25),&font,cvScalar(0,0,255));

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


	cvDestroyAllWindows();
	cvReleaseImage(&frame_copy);
	cvReleaseImage(&gray);
	cvReleaseImage(&segmented);
	cvReleaseImage(&edge);
	cvReleaseImage(&temp);
	cvReleaseImage(&output);
	cvReleaseImage(&result);
}


