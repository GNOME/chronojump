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
 * imgC (image Contour)
 * imgH (image Holes)
 */
CvSeq* findHoles(IplImage *imgC, IplImage *imgH, IplImage *foundHoles, CvRect roirect, IplImage *imgMain)
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
	CvSeq* seq0 = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), storage );
	CvSeq* seq1 = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), storage );
	CvSeq* seq2 = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), storage );
	CvSeq* seq3 = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), storage );
	CvSeq* seq4 = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), storage );
	CvSeq* seq5 = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), storage );
	CvSeq* seq6 = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), storage );
	CvSeq* seq7 = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), storage );
	CvSeq* seq8 = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), storage );
	CvSeq* seq9 = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), storage );

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
					pt.x =x;pt.y=y;
					cvCircle(foundHoles,pt,1, CV_RGB(128,128,128),1,8,0);

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
		//find three biggest seqs
		/*	
		 * seq0, seq1, ... amb els punts
		 * seqHolesUpLeft un seq amb els 10 punts de dalt a l'esquerra de cada hole 
		 * seqHolesDownRight un seq amb els 10 punts de baix a la dreta de cada hole
		 * seqHolesCenter un seq amb els 10 punts centre de cada hole
		 * seqHolesSize un seq amb les mides de cada hole (en nombre de punts)
		 * que miri la 3a mes gran a seqHolesize (sempre i quan la seva horiz i vertical sigui d'uns valors minims (per no agafar linies rectes llargues)), fer-ho restant el downRight dels upleft per ax i y
		 * que assigni hip al seqHolesCenter que aparegui en primera posició dels 3 que són prou grans
		 * que assigni knee al seqHolesCenter que aparegui en segona posició dels 3 que són prou grans
		 * que assigni toe al seqHolesCenter que aparegui en tercera posició dels 3 que són prou grans
		 * que pinti un cercle a cada un dels tres punts i faci l'angle...
		 *
		 * finalment, agafar la imatge gray (la del histograma i pintar de negre tots els punts que estiguin blancs dels seq0..seq1.. triats com a hip, knee, toe)
		 * -----------------------
		 *
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


	CvSeq* seqIsValid = cvCreateSeq( 0, sizeof(CvSeq), sizeof(0), storage ); //'1' if is valid

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
			cvSeqPush( seqIsValid, &validValue);
		} else {
			cvSeqPush( seqIsValid, &nonValidValue );
		}
	}

	int sizeBig1 = 0;
	int sizeBig2 = 0;
	int sizeBig3 = 0;
	for( int i = 0; i < seqHolesSize->total; i++ ) {
		int valid = *CV_GET_SEQ_ELEM( int, seqIsValid, i ); 
		int size = *CV_GET_SEQ_ELEM( int, seqHolesSize, i ); 
		if (valid==1) {
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
	for( int i = 0; i < seqHolesSize->total; i++ ) {
		int valid = *CV_GET_SEQ_ELEM( int, seqIsValid, i ); 
		int size = *CV_GET_SEQ_ELEM( int, seqHolesSize, i ); 
		if (valid && (size == sizeBig1 || size == sizeBig2 || size == sizeBig3)) {
			CvPoint sp1 = *CV_GET_SEQ_ELEM( CvPoint, seqHolesUpLeft, i ); 
			CvPoint sp2 = *CV_GET_SEQ_ELEM( CvPoint, seqHolesDownRight, i ); 
			CvPoint center = *CV_GET_SEQ_ELEM( CvPoint, seqHolesCenter, i ); 
			if(hipPoint.x == 0) {
				cvRectangle(imgMain, 
						cvPoint(sp1.x-1,sp1.y-1),
						cvPoint(sp2.x+1, sp2.y+1),
						CV_RGB(255,0,0),1,1);
				hipPoint.x = center.x; hipPoint.y = center.y;
				cvCircle(imgMain,center,1, CV_RGB(255,255,0),CV_FILLED,8,0);
			} else if(kneePoint.x == 0) {
				cvRectangle(imgMain, 
						cvPoint(sp1.x-1,sp1.y-1),
						cvPoint(sp2.x+1, sp2.y+1),
						CV_RGB(0,255,0),1,1);
				kneePoint.x = center.x; kneePoint.y = center.y;
				cvCircle(imgMain,center,1, CV_RGB(255,255,0),CV_FILLED,8,0);
			} else {
				cvRectangle(imgMain, 
						cvPoint(sp1.x-1,sp1.y-1),
						cvPoint(sp2.x+1, sp2.y+1),
						CV_RGB(0,0,255),1,1);
				toePoint.x = center.x; toePoint.y = center.y;
				cvCircle(imgMain,center,1, CV_RGB(255,255,0),CV_FILLED,8,0);
			}
		}
	}

	if(kneePoint.x > 0) {
		if(hipPoint.x > 0)
			cvLine(imgMain,hipPoint,kneePoint,CV_RGB(128,128,128),1,1);
		if(toePoint.x > 0)
			cvLine(imgMain,toePoint,kneePoint,CV_RGB(128,128,128),1,1);
	}
	
	/* Test:
	 * paint rectangles in all detected holes.
	 */
/*
	for( int i = 0; i < seqHolesSize->total; i++ ) {
			CvPoint sp1 = *CV_GET_SEQ_ELEM( CvPoint, seqHolesUpLeft, i ); 
			CvPoint sp2 = *CV_GET_SEQ_ELEM( CvPoint, seqHolesDownRight, i ); 
			int radious = (sp2.y - sp1.y) /2;
			if(radious < 1)
				radious = 1;
			cvRectangle(imgMain, sp1,sp2, CV_RGB(0,255,255),1,8,0);
	}
	*/
	//end of test
	
	CvSeq* seqHolesEnd = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), storage );
	cvSeqPush( seqHolesEnd, &hipPoint );
	cvSeqPush( seqHolesEnd, &kneePoint );
	cvSeqPush( seqHolesEnd, &toePoint );
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
		IplImage* tempSmall = cvCreateImage( cvSize( cvRound (img->width/scale), cvRound (img->height/scale)), 8, 1 );
		cvResize( temp, tempSmall, CV_INTER_LINEAR );

		cvShowImage("contour", tempSmall);
	}
	
	cvReleaseMemStorage(&storage);
	cvReleaseImage(&tempcopy);
	return maxrect;
}


CvPoint FixHipPoint1(IplImage* img, CvPoint hip, CvPoint knee, double angle)
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
	double maxRight = .7; //maximum right
	if(angle >= 180)
		mult = .5;
	else if(angle < 90)
		mult = maxRight;
	else {
		double temp = maxRight -.5; //if 5, then will be added to 
		double sum = ((180/angle) -1) *temp;
		mult = .5 + sum;
		printf("%f-%f-%f  ", angle, sum, mult);
	}
	
	//ptHK.x = (startX + countX) /2;
	ptHK.x = startX + (endX-startX)*mult;
	printf("%d-%d-%d\n", startX, endX, ptHK.x);

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

	/*
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
	*/

	return HCenter;
}

/* paints stick figure at end */
void paintStick(IplImage *img, int lowestAngleFrame, CvSeq *hipSeq, CvSeq* kneeSeq, CvSeq *footSeq, 
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
		CvPoint foot = *CV_GET_SEQ_ELEM( CvPoint, footSeq, i );

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
				cvCircle(img,foot,size, currentColor,1,8,0);
			}
			if(showLinesDiff && paintAngleLines) {
				cvLine(img,knee,hip,currentColor,1,1);
				cvLine(img,knee,foot,currentColor,1,1);
			}
			if(showLinesSame) {
				if(i>0) {
					CvPoint hipOld = *CV_GET_SEQ_ELEM( CvPoint, hipSeq, i-1);
					CvPoint kneeOld = *CV_GET_SEQ_ELEM( CvPoint, kneeSeq, i-1);
					CvPoint footOld = *CV_GET_SEQ_ELEM( CvPoint, footSeq, i-1);

					//only paint line if previous point was found
					if(!pointIsNull(hipOld)) {
						cvLine(img, hip, hipOld, connectedWithPreviousColor,1,1);
						cvLine(img, knee, kneeOld, connectedWithPreviousColor,1,1);
						cvLine(img, foot, footOld, connectedWithPreviousColor,1,1);
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
