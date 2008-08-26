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
 * Sharad Shankar 
 * http://www.logicbrick.com/
 * 
 */


/*
 * Explanation:
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
 * compilation:
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
	int endy = starty + roirect.height*2/3;
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
	int endy = roirect.y+roirect.height*9/10;
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
	int endy = roirect.y+roirect.height*9/10;
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
	cvReleaseMemStorage(&storage);
	cvReleaseImage(&tempcopy);
	return maxrect;
}


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
	IplImage *frame=0,*frame_copy=0,*gray=0,*segmented=0,*edge=0,*temp=0,*output=0;
	IplImage *result=0;
	int minwidth = 0;
	bool findangle = false;
	double knee2Hip,knee2Foot,hip2Foot,theta;
	//CvFont font;
	string text,angle;
	double mintheta = 360;
	char buffer[15];
	cvNamedWindow("frame",1);
	for(;;)
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
        findangle = false;
        if(minwidth == 0)
        {
        	minwidth = kneePoint.x - hipPoint.x;
        }
        else
        {
        	if((double)(kneePoint.x- hipPoint.x) > 1.25*minwidth)
        		findangle = true;
        }

        //Increases the x coordinate of the hip point by 10
		//Decreases the x coordinate of the knee point and foot point by 10
        //Finds angle between Hip to Knee line and Knee to Toe line
        if(findangle)
        {
		    hipPoint.x +=10;
		    kneePoint.x -=10;
		    footPoint.x -=10;
    	    knee2Foot=sqrt((kneePoint.x-footPoint.x)*(kneePoint.x-footPoint.x)+(kneePoint.y-footPoint.y)*(kneePoint.y-footPoint.y));
    	    knee2Hip=sqrt((kneePoint.x-hipPoint.x)*(kneePoint.x-hipPoint.x)+(kneePoint.y-hipPoint.y)*(kneePoint.y-hipPoint.y));
    	    hip2Foot=sqrt((hipPoint.x-footPoint.x)*(hipPoint.x-footPoint.x)+(hipPoint.y-footPoint.y)*(hipPoint.y-footPoint.y));
		    theta = (180.0/M_PI)*acos(((knee2Foot*knee2Foot+knee2Hip*knee2Hip)-hip2Foot*hip2Foot)/(2*knee2Foot*knee2Hip));
		    cvLine(frame_copy,kneePoint,hipPoint,CV_RGB(0,0,255),1,1);
		    cvLine(frame_copy,kneePoint,footPoint,CV_RGB(0,0,255),1,1);
		    //Finds the minimum angle between Hip to Knee line and Knee to Toe line
		    if(theta < mintheta)
		    {
		    	mintheta = theta;
		    	cvCopy(frame_copy,result);
		    }

        }
        cvShowImage("frame",frame_copy);

        if(cvWaitKey(50)>0)
        {
    	    cvWaitKey(0);
        }

    }

	cvNamedWindow("Minimum Frame",1);

	char *name = new char[10];
	sprintf(name,"Minimum Angle= %f",mintheta);
	CvFont font;
	cvInitFont(&font, CV_FONT_HERSHEY_SIMPLEX, 0.7,0.5, 0.1);
	cvPutText(result, name,cvPoint(25,25),&font,cvScalar(0,0,255));

	cvShowImage("Minimum Frame", result);
	cvWaitKey(0);
	cvDestroyAllWindows();
	cvReleaseImage(&frame_copy);
	cvReleaseImage(&gray);
	cvReleaseImage(&segmented);
	cvReleaseImage(&edge);
	cvReleaseImage(&temp);
	cvReleaseImage(&output);
	cvReleaseImage(&result);
}


