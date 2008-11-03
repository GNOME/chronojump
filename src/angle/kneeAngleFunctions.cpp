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



#include "opencv/cv.h"
#include "opencv/highgui.h"
#include <opencv/cxcore.h>
#include "stdio.h"
#include "math.h"
#include<iostream>
#include<fstream>
#include<vector>
#include <string>


int findTotalArea(IplImage* img,CvRect roirect)
{
	int starty = roirect.y;
	int endy = starty + roirect.height;
	CvMat *srcmat,src_stub;
	srcmat = cvGetMat(img,&src_stub);
	uchar *srcdata = srcmat->data.ptr;
	int width = img->width;

	int count = 0;
	for(int y=starty;y<endy;y++)
	{
		uchar *srcdataptr = srcdata + y*width;
		for(int x=0; x < width; x++)
			if(srcdataptr[x] > 0)
				count ++;
	}
	
	return count;
}

/*
 * takes as input arguement the bounding rectangle of the largest contour and the image containing the bounding rectangle
 * Calculates the hip point
 * Hip point is the x coordinate of the white pixel having minimum x coordinate in the above bounding rectangle
 * Returns the coordinate of the hip point
 */
CvPoint findHipPoint(IplImage* img,CvRect roirect)
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

	//use to fount the lowest consecutive point (under the minx, miny)
	//then we will found the 1/2 height between  both
	int minx2 = img->width;
	int miny2 = -1;

	bool foundNow = false;
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
					foundNow = true;
				}
				else if(foundNow && x==minx) {
					minx2 = x;
					miny2 = y;
				} else
					foundNow = false;
		
				break;
			}
		}
	}
	
	pt.x = minx;
	if(miny2 != -1 && minx == minx2)
		pt.y = (miny2+miny)/2;
	else 
		pt.y = miny;

	return pt;
}

/*
 * takes as input arguement the bounding rectangle of the largest contour,the image containing the bounding rectangle and the y coordinate of the hip point
 * Calculates the knee point
 * Knee point is a white pixel below the hip point and having maximum x coordinate in the bounding box
 * Returns the coordinate of the knee point
 */
CvPoint findKneePointFront(IplImage *img,CvRect roirect,int starty)
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
	
	//use to fount the lowest consecutive point (under the maxx, maxy)
	//then we will found the 1/2 height between  both
	int maxx2 = 0;
	int maxy2 = -1;
	
	bool foundNow = false;
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
					foundNow = true;
				}
				else if(foundNow && x==maxx) {
					maxx2 = x;
					maxy2 = y;
				} else
					foundNow = false;
		
				break;
			}
		}
	}
	pt.x = maxx;
	if(maxy2 != -1 && maxx == maxx2)
		pt.y = (maxy2+maxy)/2;
	else 
		pt.y = maxy;

	return pt;
}

//hueco popliteo
CvPoint findKneePointBack(IplImage *img,CvRect roirect,int starty, int kneePointFrontX)
{
	CvPoint pt;
	pt.x = 0; pt.y = 0;

	/*
	 * this 8/10 makes no error when jump starts, 
	 * if not, on jump, the bottom of the pants can be taken as kneeBack
	 */
	int endy = roirect.y+roirect.height*8/10; 

	CvMat *srcmat,src_stub;
	srcmat = cvGetMat(img,&src_stub);
	uchar *srcdata = srcmat->data.ptr;
	int width = img->width;
	int maxx = 0;
	int maxy = 0;
	
	//use to fount the lowest consecutive point (under the maxx, maxy)
	//then we will found the 1/2 height between  both
	int maxx2 = 0;
	int maxy2 = -1;
	
	bool foundNow = false;
	for(int y=starty;y<endy;y++)
	{
		uchar *srcdataptr = srcdata + y*img->width;
		for(int x=0; x < kneePointFrontX; x++)
		{
			if(srcdataptr[x] > 0)
			{
				if(x>maxx)
				{
					maxx = x;
					maxy = y;
					foundNow = true;
				}
				else if(foundNow && x==maxx) {
					maxx2 = x;
					maxy2 = y;
				} else
					foundNow = false;
		
				break;
			}
		}
	}
	pt.x = maxx;
	if(maxy2 != -1 && maxx == maxx2)
		pt.y = (maxy2+maxy)/2;
	else 
		pt.y = maxy;

	return pt;
}


CvPoint kneePointInNearMiddleOfFrontAndBack(CvPoint kneePointBack, CvPoint kneePointFront, 
		int kneePointWidth, IplImage * frame_copy)
{
	CvPoint kneePointBackPrima;
	kneePointBackPrima.x = kneePointBack.x - kneePointFront.x;
	kneePointBackPrima.y = kneePointBack.y - kneePointFront.y;

	//don't use horizontal knee distance on each photogramme
	//kneePointBackPrima.x += getDistance(kneePoint, kneePointBack) * .6;
	//use it on first photogramme: kneePointWidth

	double kneeXConvertedRatio = (double) abs(kneePointBackPrima.x) / (kneePointWidth *.4);
	kneePointBackPrima.x /= kneeXConvertedRatio;
	kneePointBackPrima.y /= kneeXConvertedRatio;

	CvPoint kneePoint;
	kneePoint.x = kneePointBackPrima.x + kneePointFront.x;
	kneePoint.y = kneePointBackPrima.y + kneePointFront.y;

	kneePoint.x = checkItsOk(kneePoint.x, 0, frame_copy->width);
	kneePoint.y = checkItsOk(kneePoint.y, 0, frame_copy->height);

	return kneePoint;
}


/*
 * takes as input arguement the bounding rectangle of the largest contour,the image containing the bounding rectangle and the x and y coordinate of the knee point
 * Calculates the toe point
 * Toe point is a white pixel below the knee point and having minimum x coordinate
 * Returns the coordinate of the hip point
 */
CvPoint findToePoint(IplImage *img,CvRect roirect,int startx,int starty)
{
	CvPoint pt;
	pt.x = 0; pt.y = 0;
	
	
	/* if toe is in the image, is better to try to avoid it capturing above, if not then capture all
	 * maybe force user to capture without toe, or ask and put a boolean
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
 * imgC (image Contour)
 * imgH (image Holes)
 */
CvSeq* findHoles(IplImage *imgC, IplImage *imgH, IplImage *foundHoles, IplImage *imgMain, 
	CvRect roirect, CvPoint hipOld, CvPoint kneeOld, CvPoint toeOld)
{
	CvPoint pt;
	pt.x =0;pt.y=0;
	int starty = roirect.y;
	int endy = starty + roirect.height;
	
	CvMat *srcmatC,src_stubC;
	srcmatC = cvGetMat(imgC, &src_stubC);
	uchar *srcdataC = srcmatC->data.ptr;
	
	CvMat *srcmatH,src_stubH;
	srcmatH = cvGetMat(imgH, &src_stubH);
	uchar *srcdataH = srcmatH->data.ptr;

	int width = imgC->width;
	int minx = imgC->width;
	int miny = imgC->height;
	
	//stick storage
	CvMemStorage* storage = cvCreateMemStorage(0);

	cvZero(foundHoles);

	CvSeq* seqPoints = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), storage );
	CvSeq* seqGroups = cvCreateSeq( 0, sizeof(CvSeq), sizeof(0), storage );

	//put all hole points on seqAllHoles
	for(int y=starty;y<endy;y++)
	{
		uchar *srcdataptrC = srcdataC + y*imgC->width;
		uchar *srcdataptrH = srcdataH + y*imgH->width;
		for(int x=0;x<width;x++)
		{
			if(srcdataptrC[x] > 0)
			{
				if(srcdataptrH[x] == 0) {
					pt.x=x;
					pt.y=y;
					//cvCircle(foundHoles,pt,1, CV_RGB(128,128,128),1,8,0);

					cvSeqPush( seqPoints, &pt );
				}
			}
		}
	}

	//assign each point to a group (a hole)
	for( int i = 0; i < seqPoints->total; i++ ) {
		CvPoint pt = *CV_GET_SEQ_ELEM( CvPoint, seqPoints, i ); 
		int group = getGroup(i, pt, seqPoints,seqGroups);
		cvSeqPush( seqGroups, &group );
	}

	/*
	for( int i = 0; i < seqGroups->total; i++ ) {
		int group = *CV_GET_SEQ_ELEM( int, seqGroups, i ); 
		CvPoint pt = *CV_GET_SEQ_ELEM( CvPoint, seqPoints, i ); 
		printf("(%d,%d):%d ", pt.x, pt.y, group);
	}

	printf("\n");
*/

	CvPoint pt1; CvPoint pt2; CvPoint pt3;
 	
	CvSeq* seqHolesUpLeft = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), storage );
	CvSeq* seqHolesDownRight = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), storage );
	CvSeq* seqHolesCenter = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), storage );
	CvSeq* seqHolesSize = cvCreateSeq( 0, sizeof(CvSeq), sizeof(0), storage );
	


	for( int i = 0; i <= getMaxValue(seqGroups); i++ ) {
		int maxX = -1;
		int minX = 10000;
		int maxY = -1;
		int minY = 10000;
		int pointsInGroup = 0;
		for( int j = 0; j < seqGroups->total; j++ ) {
			int group = *CV_GET_SEQ_ELEM( int, seqGroups, j ); 
			if(group==i) {
				CvPoint pt = *CV_GET_SEQ_ELEM( CvPoint, seqPoints, j ); 
				if(pt.x > maxX)
					maxX = pt.x;
				if(pt.x < minX)
					minX = pt.x;
				if(pt.y > maxY)
					maxY = pt.y;
				if(pt.y < minY)
					minY = pt.y;
				pointsInGroup++;
			}
		}
		if(maxX != -1) { //ripoff empty groups (appear when there are fusions between groups)
			pt1.x = minX;
			pt1.y = minY;
			pt2.x = maxX;
			pt2.y = maxY;
			pt3 = findCenter(pt1, pt2);
			cvSeqPush( seqHolesUpLeft, &pt1 );
			cvSeqPush( seqHolesDownRight, &pt2 );
			cvSeqPush( seqHolesCenter, &pt3);
			cvSeqPush( seqHolesSize, &pointsInGroup );
			//printf("%d(%d,%d)-(%d,%d)-(%d,%d): %d\n", i, pt1.x, pt1.y, pt2.x, pt2.y, pt3.x, pt3.y, pointsInGroup);
		}
	}


	CvSeq* seqIsValidSize = cvCreateSeq( 0, sizeof(CvSeq), sizeof(0), storage ); //'1' if is valid

	int minSide = 6;
	int maxSize = 10000;
	int validValue = 1;
	int nonValidValue = 0;
	for( int i = 0; i < seqHolesUpLeft->total; i++ ) {
		CvPoint sp1 = *CV_GET_SEQ_ELEM( CvPoint, seqHolesUpLeft, i ); 
		CvPoint sp2 = *CV_GET_SEQ_ELEM( CvPoint, seqHolesDownRight, i ); 
		int size = *CV_GET_SEQ_ELEM( int, seqHolesSize, i ); 
		if(
				size >= minSide && //size is higher than minSide (obvious)
				size <= maxSize && //size is lowerr than maxSize (obvious)
				sp2.x-sp1.x > minSide && sp2.y-sp1.y > minSide && //every side is bigger or equal to minSide
				! (sp2.x-sp1.x > 3*(sp2.y-sp1.y)) && ! (3*(sp2.x-sp1.x) < (sp2.y-sp1.y)) //a side is not 3 times bigger than other (helps to delete shoes if appear)
		  ) {
			cvSeqPush( seqIsValidSize, &validValue);
		} else {
			cvSeqPush( seqIsValidSize, &nonValidValue );
		}
	}

	int sizeBig1 = 0;
	int sizeBig2 = 0;
	int sizeBig3 = 0;
	for( int i = 0; i < seqHolesSize->total; i++ ) {
		int validSize = *CV_GET_SEQ_ELEM( int, seqIsValidSize, i ); 
		int size = *CV_GET_SEQ_ELEM( int, seqHolesSize, i ); 
		if (validSize == 1) {
			if(size > sizeBig1) {
				sizeBig3 = sizeBig2;
				sizeBig2 = sizeBig1;
				sizeBig1 = size;
			} else if (size > sizeBig2) {
				sizeBig3 = sizeBig2;
				sizeBig2 = size;
			} else if (size > sizeBig3) {
				sizeBig3 = size;
			}
		}
	}
	
	CvPoint hipPoint;
	CvPoint kneePoint;
	CvPoint toePoint;
	hipPoint.x=0; kneePoint.x=0; toePoint.x=0;
	for( int i = 0; i < seqHolesSize->total; i++ ) 
	{
		int validSize = *CV_GET_SEQ_ELEM( int, seqIsValidSize, i ); 
		int size = *CV_GET_SEQ_ELEM( int, seqHolesSize, i ); 
		CvPoint sp1 = *CV_GET_SEQ_ELEM( CvPoint, seqHolesUpLeft, i ); 
		CvPoint sp2 = *CV_GET_SEQ_ELEM( CvPoint, seqHolesDownRight, i ); 
	
		bool validSure = false;

		//if size is valid
		if (validSize) {
			//if never found a point before, and this are the biggest points found
			if(pointIsNull(hipOld) && pointIsNull(kneeOld) && pointIsNull(toeOld)) {
				if(size == sizeBig1 || size == sizeBig2 || size == sizeBig3)
					validSure = true;
			} 
			//if found a point before, and this point is inside before point (ok at 300 fps)
			//a point is also ok, if we come from a user forward (then, there's not need to be inside old point)
			else if (pointInside(hipOld, sp1, sp2) || pointInside(kneeOld, sp1,sp2) || pointInside(toeOld, sp1,sp2))
				validSure = true;
		}

		if(validSure) {
			CvPoint center = *CV_GET_SEQ_ELEM( CvPoint, seqHolesCenter, i ); 
			if(hipPoint.x == 0) {
				cvRectangle(imgMain, 
						cvPoint(sp1.x-1,sp1.y-1),
						cvPoint(sp2.x+1, sp2.y+1),
						CV_RGB(0,255,0),1,1);
				hipPoint.x = center.x; hipPoint.y = center.y;
			} else if(kneePoint.x == 0) {
				cvRectangle(imgMain, 
						cvPoint(sp1.x-1,sp1.y-1),
						cvPoint(sp2.x+1, sp2.y+1),
						CV_RGB(0,255,0),1,1);
				kneePoint.x = center.x; kneePoint.y = center.y;
			} else {
				cvRectangle(imgMain, 
						cvPoint(sp1.x-1,sp1.y-1),
						cvPoint(sp2.x+1, sp2.y+1),
						CV_RGB(0,255,0),1,1);
				toePoint.x = center.x; toePoint.y = center.y;
			}
		} else {
			 //paint rectangles in not-valid (or not big) holes.
			cvRectangle(imgMain, 
					cvPoint(sp1.x-1,sp1.y-1),
					cvPoint(sp2.x+1, sp2.y+1),
					CV_RGB(0,255,255),1,1);
		}
	}

	if(kneePoint.x > 0) {
		if(hipPoint.x > 0)
			cvLine(imgMain,hipPoint,kneePoint,CV_RGB(0,255,0),1,1);
		if(toePoint.x > 0)
			cvLine(imgMain,toePoint,kneePoint,CV_RGB(0,255,0),1,1);
	}
	

	CvPoint notFoundPoint;
	notFoundPoint.x = 0; notFoundPoint.y = 0;
	
	CvSeq* seqHolesEnd = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), storage );
	
	if(hipPoint.x > 0) 
		cvSeqPush( seqHolesEnd, &hipPoint );
	else
		cvSeqPush( seqHolesEnd, &notFoundPoint );
	
	if(kneePoint.x > 0) 
		cvSeqPush( seqHolesEnd, &kneePoint );
	else
		cvSeqPush( seqHolesEnd, &notFoundPoint );

	if(toePoint.x > 0) 
		cvSeqPush( seqHolesEnd, &toePoint );
	else
		cvSeqPush( seqHolesEnd, &notFoundPoint );
	
	return seqHolesEnd;
}

/*
 * this function is realy similiar to findHoles
 * try to do only a function
 */
CvSeq* findHolesSkin(IplImage *imgThresh, IplImage *imgColor, CvPoint hipOld, CvPoint kneeOld, CvPoint toeOld)
{
	CvPoint pt;
	pt.x =0;pt.y=0;
	
	CvMat *srcmat,src_stub;
	srcmat = cvGetMat(imgThresh, &src_stub);
	uchar *srcdata = srcmat->data.ptr;
	
	int width = imgThresh->width;
	int minx = imgThresh->width;
	int endy = imgThresh->height;
	
	//stick storage
	CvMemStorage* storage = cvCreateMemStorage(0);

	CvSeq* seqPoints = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), storage );
	CvSeq* seqGroups = cvCreateSeq( 0, sizeof(CvSeq), sizeof(0), storage );

//exit(0); //desp
	//put all hole points on seqAllHoles
	for(int y=0;y<endy;y++)
	{
		uchar *srcdataptr = srcdata + y*imgThresh->width;
		for(int x=0;x<width;x++)
		{
			if(srcdataptr[x] == 0)
			{
				pt.x=x; 
				pt.y=y;
				//cvCircle(foundHoles,pt,1, CV_RGB(128,128,128),1,8,0);
				cvSeqPush( seqPoints, &pt );
			}
		}
	}

	//assign each point to a group (a hole)
	for( int i = 0; i < seqPoints->total; i++ ) {
		CvPoint pt = *CV_GET_SEQ_ELEM( CvPoint, seqPoints, i ); 
		int group = getGroup(i, pt, seqPoints,seqGroups);
		cvSeqPush( seqGroups, &group );
	}
	

	CvPoint pt1; CvPoint pt2; CvPoint pt3;
 	
	CvSeq* seqHolesUpLeft = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), storage );
	CvSeq* seqHolesDownRight = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), storage );
	CvSeq* seqHolesCenter = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), storage );
	CvSeq* seqHolesSize = cvCreateSeq( 0, sizeof(CvSeq), sizeof(0), storage );
	


	for( int i = 0; i <= getMaxValue(seqGroups); i++ ) {
		int maxX = -1;
		int minX = 10000;
		int maxY = -1;
		int minY = 10000;
		int pointsInGroup = 0;
		for( int j = 0; j < seqGroups->total; j++ ) {
			int group = *CV_GET_SEQ_ELEM( int, seqGroups, j ); 
			if(group==i) {
				CvPoint pt = *CV_GET_SEQ_ELEM( CvPoint, seqPoints, j ); 
				if(pt.x > maxX)
					maxX = pt.x;
				if(pt.x < minX)
					minX = pt.x;
				if(pt.y > maxY)
					maxY = pt.y;
				if(pt.y < minY)
					minY = pt.y;
				pointsInGroup++;
			}
		}
		if(maxX != -1) { //ripoff empty groups (appear when there are fusions between groups)
			pt1.x = minX;
			pt1.y = minY;
			pt2.x = maxX;
			pt2.y = maxY;
			pt3 = findCenter(pt1, pt2);
			cvSeqPush( seqHolesUpLeft, &pt1 );
			cvSeqPush( seqHolesDownRight, &pt2 );
			cvSeqPush( seqHolesCenter, &pt3);
			cvSeqPush( seqHolesSize, &pointsInGroup );
			//printf("%d(%d,%d)-(%d,%d)-(%d,%d): %d\n", i, pt1.x, pt1.y, pt2.x, pt2.y, pt3.x, pt3.y, pointsInGroup);
		}
	}
	
	CvSeq* seqIsValidSize = cvCreateSeq( 0, sizeof(CvSeq), sizeof(0), storage ); //'1' if is valid

	int minSide = 6;
	int maxSize = 200;
	int validValue = 1;
	int nonValidValue = 0;
	for( int i = 0; i < seqHolesUpLeft->total; i++ ) {
		CvPoint sp1 = *CV_GET_SEQ_ELEM( CvPoint, seqHolesUpLeft, i ); 
		CvPoint sp2 = *CV_GET_SEQ_ELEM( CvPoint, seqHolesDownRight, i ); 
		int size = *CV_GET_SEQ_ELEM( int, seqHolesSize, i ); 
				
		if(
				size >= minSide && //size is higher than minSide (obvious)
				size <= maxSize	&& //size is lowerr than maxSize (obvious)
		//		sp2.x-sp1.x > minSide && sp2.y-sp1.y > minSide && //every side is bigger or equal to minSide
				! (sp2.x-sp1.x > 3*(sp2.y-sp1.y)) && ! (3*(sp2.x-sp1.x) < (sp2.y-sp1.y)) //a side is not 3 times bigger than other (helps to delete shoes if appear)
		  ) {
			cvSeqPush( seqIsValidSize, &validValue);
		} else {
			cvSeqPush( seqIsValidSize, &nonValidValue );
		}
	}

	int sizeBig1 = 0;
	int sizeBig2 = 0;
	int sizeBig3 = 0;
	for( int i = 0; i < seqHolesSize->total; i++ ) {
		int validSize = *CV_GET_SEQ_ELEM( int, seqIsValidSize, i ); 
		int size = *CV_GET_SEQ_ELEM( int, seqHolesSize, i ); 
		if (validSize == 1) {
			if(size > sizeBig1) {
				sizeBig3 = sizeBig2;
				sizeBig2 = sizeBig1;
				sizeBig1 = size;
			} else if (size > sizeBig2) {
				sizeBig3 = sizeBig2;
				sizeBig2 = size;
			} else if (size > sizeBig3) {
				sizeBig3 = size;
			}
		}
	}
	
	CvPoint hipPoint;
	CvPoint kneePoint;
	CvPoint toePoint;
	hipPoint.x=0; kneePoint.x=0; toePoint.x=0;

	for( int i = 0; i < seqHolesSize->total; i++ ) 
	{
		int validSize = *CV_GET_SEQ_ELEM( int, seqIsValidSize, i ); 
		int size = *CV_GET_SEQ_ELEM( int, seqHolesSize, i ); 
		CvPoint sp1 = *CV_GET_SEQ_ELEM( CvPoint, seqHolesUpLeft, i ); 
		CvPoint sp2 = *CV_GET_SEQ_ELEM( CvPoint, seqHolesDownRight, i ); 

		bool validSure = false;
		CvPoint center; 

		CvScalar color = CV_RGB(128, 128, 128 ); //paint rectangles in not-valid (or not big) holes.

		//if size is valid
		if (validSize) {
			center = *CV_GET_SEQ_ELEM( CvPoint, seqHolesCenter, i ); 
			//if never found a point before, and this are the biggest points found
			if(pointIsNull(hipOld) && pointIsNull(kneeOld) && pointIsNull(toeOld)) {
				if(size == sizeBig1 || size == sizeBig2 || size == sizeBig3) {
					validSure = true;

					if(hipPoint.x == 0) {
						hipPoint.x = center.x; 
						hipPoint.y = center.y;
					color = CV_RGB(255, 0, 0 );

					} else if(kneePoint.x == 0) {
						kneePoint.x = center.x; 
						kneePoint.y = center.y;
						color = CV_RGB(0, 255, 0 );

					} else {
						toePoint.x = center.x; 
						toePoint.y = center.y;
						color = CV_RGB(0, 0, 255 );

					}
				}
			}
			//if found a point before, and this point is inside before point (ok at 300 fps)
			//a point is also ok, if we come from a user forward (then, there's not need to be inside old point)
			else {
				validSure = true;
				if(pointInside(hipOld, sp1, sp2)) {
					hipPoint.x = center.x; 
					hipPoint.y = center.y;
					color = CV_RGB(255, 0, 0 );

				} else if(pointInside(kneeOld, sp1,sp2)) {
					kneePoint.x = center.x; 
					kneePoint.y = center.y;
					color = CV_RGB(0, 255, 0 );

				} else if(pointInside(toeOld, sp1,sp2)) {
					toePoint.x = center.x; 
					toePoint.y = center.y;
					color = CV_RGB(0, 0, 255);

				}else 
					validSure = false;
			}
		}

		cvRectangle(imgThresh, 
				cvPoint(sp1.x-1,sp1.y-1),
				cvPoint(sp2.x+1, sp2.y+1),
				color,1,1);
		cvRectangle(imgColor, 
				cvPoint(sp1.x-1,sp1.y-1),
				cvPoint(sp2.x+1, sp2.y+1),
				color,1,1);
	}

	if(kneePoint.x > 0) {
		if(hipPoint.x > 0) {
			cvLine(imgThresh,hipPoint,kneePoint,CV_RGB(0,255,0),1,1);
			cvLine(imgColor,hipPoint,kneePoint,CV_RGB(0,255,0),1,1);
		} if(toePoint.x > 0) {
			cvLine(imgThresh,toePoint,kneePoint,CV_RGB(0,255,0),1,1);
			cvLine(imgColor,toePoint,kneePoint,CV_RGB(0,255,0),1,1);
		}
	}

	CvPoint notFoundPoint;
	notFoundPoint.x = 0; notFoundPoint.y = 0;

	CvSeq* seqHolesEnd = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), storage );

	if(hipPoint.x > 0) 
		cvSeqPush( seqHolesEnd, &hipPoint );
	else
		cvSeqPush( seqHolesEnd, &notFoundPoint );

	if(kneePoint.x > 0) 
		cvSeqPush( seqHolesEnd, &kneePoint );
	else
		cvSeqPush( seqHolesEnd, &notFoundPoint );

	if(toePoint.x > 0) 
		cvSeqPush( seqHolesEnd, &toePoint );
	else
		cvSeqPush( seqHolesEnd, &notFoundPoint );

	return seqHolesEnd;
}


/*
 * takes input argument as the gray form of input frame and a temp image
 * Returns the bounding rectangle of the contour having maximum height
 * Draws the bounding rectangle of the largest contour on temp
 */

CvRect findLargestContour(IplImage* img,IplImage* temp, bool showContour)
{
	CvContourScanner scanner;
	CvSeq *src_contour;
	IplImage *tempcopy = cvCreateImage(cvGetSize(img),IPL_DEPTH_8U,1);
	cvCopy(img,tempcopy);
	CvMemStorage* storage = cvCreateMemStorage(0);
	scanner = cvStartFindContours(img,storage,sizeof(CvContour),CV_RETR_EXTERNAL,CV_CHAIN_APPROX_SIMPLE);
	//scanner = cvStartFindContours(img,storage,sizeof(CvContour),CV_RETR_EXTERNAL,CV_CHAIN_APPROX_TC89_L1); //nothing
	//scanner = cvStartFindContours(img,storage,sizeof(CvContour),CV_RETR_LIST,CV_CHAIN_APPROX_TC89_L1); //nothing
	//scanner = cvStartFindContours(img,storage,sizeof(CvContour),CV_RETR_EXTERNAL,CV_CHAIN_CODE); //nothing
	//scanner = cvStartFindContours(img,storage,sizeof(CvChain),CV_RETR_EXTERNAL,CV_CHAIN_CODE); //segmentation fault
	//scanner = cvStartFindContours(img,storage,sizeof(CvChain),CV_RETR_LIST,CV_CHAIN_CODE); //segmentation fault
	//scanner = cvStartFindContours(img,storage,sizeof(CvContour),CV_RETR_LIST,CV_CHAIN_CODE); //nothing
	//scanner = cvStartFindContours(img,storage,sizeof(CvContour),CV_RETR_EXTERNAL,CV_CHAIN_APPROX_NONE); //== simple
	//scanner = cvStartFindContours(img,storage,sizeof(CvContour),CV_RETR_EXTERNAL,CV_CHAIN_APPROX_TC89_KCOS); //== simple
	//scanner = cvStartFindContours(img,storage,sizeof(CvContour),CV_RETR_LIST,CV_LINK_RUNS);
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
		cvNamedWindow("contour",1);
		cvResize(temp, temp, CV_INTER_LINEAR);

		double scale = 4;
		//double scale = 1;
		IplImage* tempSmall = cvCreateImage( cvSize( cvRound (img->width/scale), cvRound (img->height/scale)), 8, 1 );
		cvResize( temp, tempSmall, CV_INTER_LINEAR );

		cvShowImage("contour", tempSmall);
	}
	
	cvReleaseMemStorage(&storage);
	cvReleaseImage(&tempcopy);
	return maxrect;
}

//point will be more at right if there's more flexion
int fixToePointX(int toeX, int toeWidth, double kneeAngle)
{
			
//	toePoint.x -= toePointWidth /2;

	//point will be more at right if there's more flexion
	double mult;
	double maxRight = .7; //maximum right
	double valueAtExtension = .6;
	if(kneeAngle >= 180)
		mult = valueAtExtension;
	else if(kneeAngle < 90)
		mult = maxRight;
	else {
		double temp = maxRight - valueAtExtension;
		double sum = ((180/kneeAngle) -1) *temp;
		mult = valueAtExtension + sum;
//		printf("%f-%f-%f  ", kneeAngle, sum, mult);
	}
	
	//ptHK.x = (startX + countX) /2;
	int startX = toeX - toeWidth; 
	int endX = toeX; 
	toeX = startX + (endX-startX)*mult;
//	printf("%d-%d-%d\n", startX, endX, ptHK.x);

//	ptHK.y = y;
	
	return toeX;
}

CvPoint fixHipPoint1(IplImage* img, int hipY, CvPoint knee, double kneeAngle)
{
	CvPoint ptHK;
	ptHK.x =0;ptHK.y=0;
	CvMat *srcmat,src_stub;
	srcmat = cvGetMat(img,&src_stub);
	uchar *srcdata = srcmat->data.ptr;
	int width = img->width;

	//find at 3/2 of hip (surely under the hand)
	int y=hipY*.66 + knee.y*.33;

	uchar *srcdataptr = srcdata + y*img->width;
	int startX = 0;
	int endX = 0;
	bool found = false;

	for(int x=0;x<width;x++)
	{
		if(srcdataptr[x] > 0)
		{
			if(!found) {
				startX = x;
				endX = x;
				found = true;
			}
			endX ++;
		}
	}

	//point will be more at right if there's more flexion
	double mult;
	double valueAtExtension = .6;
	double maxRight = .7; //maximum right
	if(kneeAngle >= 180)
		mult = valueAtExtension;
	else if(kneeAngle < 90)
		mult = maxRight;
	else {
		double temp = maxRight - valueAtExtension; 
		double sum = ((180/kneeAngle) -1) *temp;
		mult = valueAtExtension + sum;
		//printf("%f-%f-%f  ", kneeAngle, sum, mult);
	}
	
	//ptHK.x = (startX + countX) /2;
	ptHK.x = startX + (endX-startX)*mult;
	//printf("%d-%d-%d\n", startX, endX, ptHK.x);

	ptHK.y = y;
	
	return ptHK;
}

CvPoint fixHipPoint2(IplImage* img, int hipY, CvPoint knee, CvPoint ptHK)
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
	HCenter.y = hipY;

	HCenter.x = ( (kneePrima.x / (double)kneePrima.y) * HCenter.y ) - d;

	/*
	if(debug) {
		printf("hipy(%d) ",hipY);
		printf("knee(%d,%d) ",knee.x, knee.y);
		printf("ptHK(%d,%d) ",ptHK.x, ptHK.y);
		printf("kneePrima(%d,%d) ",kneePrima.x, kneePrima.y);
		printf("HCenter(%d,%d) ",HCenter.x, HCenter.y);
		printf("kneePrima x/y:%.2f ", kneePrima.x / (double)kneePrima.y);
		printf("d:%.1f", d);
		printf("\n");
	}
	*/

	return HCenter;
}

/* paints stick figure at end */
void paintStick(IplImage *img, int lowestAngleFrame, CvSeq *hipSeq, CvSeq* kneeSeq, CvSeq *toeSeq, 
		bool showPoints, bool showLinesDiff, bool showLinesSame, bool onlyStartMinEnd, CvFont font) {
	
	//colors for start, end points, and up, down lines
	CvScalar startColor = CV_RGB(0,0,255); //start blue
	CvScalar minAngleColor = CV_RGB(255,0,0); //min angle red
	CvScalar endColor = CV_RGB(0,255,0); //end green
	CvScalar connectedWithPreviousColor = CV_RGB(255,255,0); //yellow
	CvScalar unConnectedWithPreviousColor = CV_RGB(128,128,128); //grey
	CvScalar currentColor;
	int size = 0;
	
	bool lastFound = false;
	bool neverFound = true;

	for( int i = 0; i < hipSeq->total; i++ )
	{
		CvPoint hip = *CV_GET_SEQ_ELEM( CvPoint, hipSeq, i );
		CvPoint knee = *CV_GET_SEQ_ELEM( CvPoint, kneeSeq, i );
		CvPoint toe = *CV_GET_SEQ_ELEM( CvPoint, toeSeq, i );

		//if this point was found
		if(!pointIsNull(hip)) {

			//colors are different depending on phase
			if(i < lowestAngleFrame)
				currentColor = startColor;
			else if(i > lowestAngleFrame)
				currentColor = endColor;
			else
				currentColor = minAngleColor;

			//size of some points is bigger, also decide if paint angle lines
			bool paintAngleLines = true;
			if(neverFound || i == lowestAngleFrame || i == hipSeq->total -1 )
				size = 3;
			else {
				size = 1;
				if(onlyStartMinEnd)
					paintAngleLines = false;
			}

			if(showPoints) {
				cvCircle(img,knee,size, currentColor,1,8,0);
				cvCircle(img,hip,size, currentColor,1,8,0);
				cvCircle(img,toe,size, currentColor,1,8,0);
			}
			if(showLinesDiff && paintAngleLines) {
				cvLine(img,knee,hip,currentColor,1,1);
				cvLine(img,knee,toe,currentColor,1,1);
			}
			if(showLinesSame) {
				if(i>0) {
					CvPoint hipOld = *CV_GET_SEQ_ELEM( CvPoint, hipSeq, i-1);
					CvPoint kneeOld = *CV_GET_SEQ_ELEM( CvPoint, kneeSeq, i-1);
					CvPoint toeOld = *CV_GET_SEQ_ELEM( CvPoint, toeSeq, i-1);

					//only paint line if previous point was found
					if(!pointIsNull(hipOld)) {
						cvLine(img, hip, hipOld, connectedWithPreviousColor,1,1);
						cvLine(img, knee, kneeOld, connectedWithPreviousColor,1,1);
						cvLine(img, toe, toeOld, connectedWithPreviousColor,1,1);
					}
				}
			}
			lastFound = true;
			neverFound = false;
		} else 
			lastFound = false;
	}
	
	//print text	
	char *label = new char[10];
	sprintf(label,"First");
	cvPutText(img, label,cvPoint(20, 20),&font,startColor);
	sprintf(label,"Min");
	cvPutText(img, label,cvPoint(20, 40),&font,minAngleColor);
	sprintf(label,"Last");
	cvPutText(img, label,cvPoint(20, 60),&font,endColor);
}

int calculateBrightness(IplImage* img)
{
	int starty = 0;
	int endy = img->height;
	int width = img->width;
	
	IplImage * detectBrightness = cvCreateImage(cvGetSize(img),IPL_DEPTH_8U,1);
	cvThreshold(img, detectBrightness, 67, 255, CV_THRESH_BINARY_INV);
	
	CvMat *srcmat,src_stub;
	srcmat = cvGetMat(detectBrightness,&src_stub);
	uchar *srcdata = srcmat->data.ptr;

	int countBlack = 0;
	int countWhite = 0;
	for(int y=starty;y<endy;y++)
	{
		uchar *srcdataptr = srcdata + y*width;
		for(int x=0; x < width; x++)
			if(srcdataptr[x] == 0)
				countBlack ++;
			else
				countWhite ++;
	}
	
	//cvNamedWindow("detectBrightness");
	//cvShowImage("detectBrightness", detectBrightness);

	if(countBlack == 0)
		return 0;
	else if(countWhite == 0)
		return 100;
	else 
		return (int) 100 * countBlack/(countWhite + countBlack);
}

int calculateThresholdStart(IplImage * gray)
{
	int brightness = calculateBrightness(gray);
	printf("brightness: %d\n", brightness);

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
	int thresholdStart;
	int briMax = 85;
	int briMin = 65;
	//int briZero = 50;
	int thresMax = 30;
	int thresMin = 10;

	if(brightness >= briMax) 
		thresholdStart = thresMax;
	//else if(brightness <= briZero)
	//	thresholdStart = 10;
	else if(brightness <= briMin)
		thresholdStart = thresMin;
	else
		thresholdStart = brightness - briMin + thresMin;

	return thresholdStart;
}

double zoomScale = 2;

IplImage * zoomImage(IplImage *img) {
	IplImage* imgZoom = cvCreateImage( cvSize( cvRound (img->width*zoomScale), 
				cvRound (img->height*zoomScale)), 8, 3 );
	cvResize( img, imgZoom, CV_INTER_LINEAR );
	return imgZoom;
}

CvPoint hipMouse;
CvPoint kneeMouse;
CvPoint toeMouse;

bool forceMouseHip = false;
bool forceMouseKnee = false;
bool forceMouseToe = false;

bool zoomed = false;

bool mouseCanMark = false;

void on_mouse( int event, int x, int y, int flags, void* param )
{
	if(! mouseCanMark)
		return;

	if(zoomed) {
		x = x / zoomScale;
		y = y / zoomScale;
	}

	switch( event ) {
		case CV_EVENT_LBUTTONDOWN:
			{
				if(forceMouseHip) 
				{
					hipMouse = cvPoint(x,y);
					forceMouseHip = false;
					printf("H x:%d, y:%d\n", x, y);
				} 
				else if(forceMouseKnee) 
				{
					kneeMouse = cvPoint(x,y);
					forceMouseKnee = false;
					printf("K x:%d, y:%d\n", x, y);
				} 
				else if(forceMouseToe) 
				{
					toeMouse = cvPoint(x,y);
					forceMouseToe = false;
					printf("T x:%d, y:%d\n", x, y);
				} 
				else if(hipMouse.x == 0)
				{
					hipMouse = cvPoint(x,y);
					printf("H x:%d, y:%d\n", x, y);
				}
				else if(kneeMouse.x == 0)
				{
					kneeMouse = cvPoint(x,y);
					printf("K x:%d, y:%d\n", x, y);
				}
				else 
				{
					toeMouse = cvPoint(x,y);
					printf("T x:%d, y:%d\n", x, y);
				}
			}
			break;
	}
}

void updateHolesWin(IplImage *segmentedValidationHoles) {
	double scale = 4;
	IplImage* tempSmall = cvCreateImage( cvSize( cvRound (segmentedValidationHoles->width/scale), 
				cvRound (segmentedValidationHoles->height/scale)), 8, 1 );
	cvResize( segmentedValidationHoles, tempSmall, CV_INTER_LINEAR );
	cvShowImage("holes",tempSmall);
}

void printOnScreen(IplImage * img, CvFont font, CvScalar color, int legMarkedDist, double legError, int framesCount, 
		int threshold, double thetaMarked, double minThetaMarked)
{
	char *label = new char[150];
	int width = img->width;
	int height = img->height;
				
	sprintf(label, "legSize: %d(%.1f%%)", legMarkedDist, legError);
	cvPutText(img, label, cvPoint(10, height-100),&font,color);

	sprintf(label, "M angle obs: %.2fº", thetaMarked);
	cvPutText(img, label, cvPoint(10,height-80),&font,color);
	
	sprintf(label, "min M angle obs: %.2fº", minThetaMarked);
	cvPutText(img, label, cvPoint(10,height-60),&font,color);
	
	sprintf(label, "frame: %d", framesCount);
	cvPutText(img, label, cvPoint(10,height-40),&font,color);

	sprintf(label, "threshold: %d", threshold);
	cvPutText(img, label, cvPoint(10,height-20),&font,color);
	
}
			
void printOnScreenRight(IplImage * img, CvFont font, CvScalar color, 
					double upLegMarkedDist, double downLegMarkedDist,  
					double upLegMarkedDistMax, double downLegMarkedDistMax,  
					double kneeZetaSide, double htKneeMarked, 
					double thetaABD, double thetaRealFlex)
{
	char *label = new char[150];
	int width = img->width;
	int height = img->height;
				
	sprintf(label, "legUp/Down: %.1f/%.1f", upLegMarkedDist, downLegMarkedDist);
	cvPutText(img, label, cvPoint(width-200, height-120),&font,color);

	sprintf(label, "legMaxUp/Down: %.1f/%.1f", upLegMarkedDistMax, downLegMarkedDistMax);
	cvPutText(img, label, cvPoint(width-200, height-100),&font,color);

	sprintf(label, "kneeZetaSide: %.1f", kneeZetaSide);
	cvPutText(img, label, cvPoint(width-200, height-80),&font,color);

	sprintf(label, "htKneeMarked: %.2f", htKneeMarked);
	cvPutText(img, label, cvPoint(width-200, height-40),&font,color);

	sprintf(label, "sideMove: %.2fº", thetaABD);
	cvPutText(img, label, cvPoint(width-200, height-60),&font,color);

	sprintf(label, "real Flex: %.2fº", thetaRealFlex);
	cvPutText(img, label, cvPoint(width-200, height-20),&font,color);
}

