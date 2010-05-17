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
 * Copyright (C) 2008-2010   Xavier de Blas <xaviblas@gmail.com> 
 *
 */


#include "opencv/cv.h"
#include "opencv/highgui.h"
//#include <opencv/cxcore.h>
#include "stdio.h"
#include "math.h"
//#include<iostream>
//#include<fstream>
//#include<vector>
#include <string>
//using namespace std;


CvPoint findMiddle(CvPoint pt1, CvPoint pt2)
{
	return cvPoint((pt1.x+pt2.x)/2, (pt1.y+pt2.y)/2);
}

CvPoint findCenter(CvPoint pt1, CvPoint pt2)
{
	CvPoint center;
	center.x=(pt1.x + pt2.x)/2 +1;
	center.y=(pt1.y + pt2.y)/2 +1;
	return center;
}

bool connectedWithPoint(CvPoint pt, CvPoint sp)
{
	if     (sp.x == pt.x -1 && sp.y == pt.y) //left
		return true;
	else if(sp.x == pt.x -1 && sp.y == pt.y-1) //upLeft
		return true;
	else if(sp.x == pt.x    && sp.y == pt.y-1) //up
		return true;
	else if(sp.x == pt.x +1 && sp.y == pt.y-1) //upRight
		return true;
	else if(sp.x == pt.x +1 && sp.y == pt.y  ) //right
		return true;
	else if(sp.x == pt.x +1 && sp.y == pt.y+1) //downRight
		return true;
	else if(sp.x == pt.x    && sp.y == pt.y+1) //down
		return true;
	else if(sp.x == pt.x -1 && sp.y == pt.y+1) //downLeft
		return true;
	//		else if(sp.y == pt.y -1) //special attention
	//			return true;
	else
		return false;
}

CvRect seqRect(CvSeq *seq) {

	int minx = 1000;
	int miny = 1000;
	int maxx = 0;
	int maxy = 0;
	for( int i = 0; i < seq->total; i++ )
	{
		CvPoint pt = *CV_GET_SEQ_ELEM( CvPoint, seq, i ); //seqPoint
		if(pt.x < minx)
			minx = pt.x;
		if(pt.x > maxx)
			maxx = pt.x;
		if(pt.y < miny)
			miny = pt.y;
		if(pt.y > maxy)
			maxy = pt.y;
	}
	
	CvRect rect;

	rect.x = minx;
	rect.y = miny;
	rect.width = maxx-minx;
	rect.height = maxy-miny;

	return rect;
}


CvPoint findCorner(CvSeq* seq, bool first)
{
	int minx= 1000;
	int miny= 1000;
	int maxx= 0;
	int maxy= 0;
	CvPoint pt;
	if(first) {
		pt.x=minx; pt.y=miny;
	} else {
		pt.x=maxx; pt.y=maxy;
	}

	for( int i = 0; i < seq->total; i++ ) {
		CvPoint sp = *CV_GET_SEQ_ELEM( CvPoint, seq, i ); //seqPoint
		if(first) {
			if(sp.x < pt.x) 
				pt.x = sp.x;
			if(sp.y < pt.y) 
				pt.y = sp.y;
		} else {
			if(sp.x > pt.x)
				pt.x = sp.x;
			if(sp.y > pt.y)
				pt.y = sp.y;
		}
	}
	return pt;
}

/* at first photogramm where knee or foot is detected (it will not be too horizontal) find it's width and use all the time to fix kneex
 * at knee is called only done one time (because in max flexion, the back is line with the knee and there will be problems knowing knee width
 * at foot is called all the time
 */
int findWidth(IplImage* img, CvPoint point, bool goRight)
{
	CvMat *srcmat,src_stub;
	srcmat = cvGetMat(img,&src_stub);
	uchar *srcdata = srcmat->data.ptr;
	int width = img->width;

	int y=point.y;

	uchar *srcdataptr = srcdata + y*img->width;
	int countX = 0;

	if(goRight)
		for(int x=point.x+1; srcdataptr[x]; x++)
			countX ++;
	else
		for(int x=point.x-1; srcdataptr[x]; x--)
			countX ++;

	return countX;
}

double abs(double val)
{
	if(val<0)
		val *= -1;
	return val;
}

double getDistance(CvPoint p1, CvPoint p2)
{
	return sqrt( pow(p1.x-p2.x, 2) + pow(p1.y-p2.y, 2) );
}

double getDistance3D(CvPoint p1, CvPoint p2, int p1z, int p2z)
{
	return sqrt( pow(p1.x-p2.x, 2) + pow(p1.y-p2.y, 2) + pow(p1z-p2z, 2) );
}

int checkItsOk(int val, int min, int max)
{
	if(val < min)
		return min;
	else if(val > max)
		return max;
	return val;
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

//	if(Debug)
//		printf("upper(%.1f), lower(%.1f), big/little (%.2f)\n",upper, lower, big/(double)little);

	//if one is not n times or more bigger than the other
	//consider that if camera hides shoes and a bit more up, 2 times is risky in a maximal flexion
	//consider also that this data is previous to fixing
	double n = 2.5;
	if(big / (double)little < n)
		return true;
	else 
		return false;
}

bool pointIsNull(CvPoint pt) {
	CvPoint notFoundPoint;
	notFoundPoint.x = 0; notFoundPoint.y = 0;
	if(pt.x == notFoundPoint.x && pt.y == notFoundPoint.y) 
		return true;
	else 
		return false;
}

bool pointInside(CvPoint pt, CvPoint upLeft, CvPoint downRight ) {
	if(
			pt.x >= upLeft.x && pt.x <= downRight.x && 
			pt.y >= upLeft.y && pt.y <= downRight.y)
		return true;
	return false;
}

bool pointInsideRect(CvPoint pt, CvRect rect) {
	CvPoint upLeft; 
	upLeft.x = rect.x;
	upLeft.y = rect.y;
	CvPoint downRight; 
	downRight.x = rect.x + rect.width;
	downRight.y = rect.y + rect.height;

	return pointInside(pt, upLeft, downRight);
}

bool pointIsEqual(CvPoint pt1, CvPoint pt2 ) {
	if(pt1.x == pt2.x && pt1.y == pt2.y)
		return true;
	else
		return false;
}


double findAngle2D(CvPoint p1, CvPoint p2, CvPoint pa) //pa is the point at the angle
{
	CvPoint d1, d2;
	d1.x = p1.x - pa.x;
	d1.y = p1.y - pa.y;
	d2.x = p2.x - pa.x;
	d2.y = p2.y - pa.y;
	double dist1 = getDistance(p1, pa);
	double dist2 = getDistance(p2, pa);
	return (180.0/M_PI)*acos(((d1.x*d2.x + d1.y*d2.y))/(double)(dist1*dist2));
}

double findAngle3D(CvPoint p1, CvPoint p2, CvPoint pa, int p1z, int p2z, int paz) //pa is the point at the angle
{
	CvPoint d1, d2;
	d1.x = p1.x - pa.x;
	d1.y = p1.y - pa.y;
	int d1z = p1z - paz;
	d2.x = p2.x - pa.x;
	d2.y = p2.y - pa.y;
	int d2z = p2z - paz;
	double dist1 = getDistance3D(p1, pa, p1z, paz);
	double dist2 = getDistance3D(p2, pa, p2z, paz);
	return (180.0/M_PI)*acos(((d1.x*d2.x + d1.y*d2.y + d1z*d2z))/(double)(dist1*dist2));
}

double relError(double val1, double val2)
{
	if(val2-val1 == 0 || val2 == 0)
		return -1;
	else
		return (double) (val2-val1)/val2 *100;
}

int getMaxValue(CvSeq* seqGroups)
{
	int max = 0;
	for( int i = 0; i < seqGroups->total; i++ ) {
		int group = *CV_GET_SEQ_ELEM( int, seqGroups, i ); 
		if(group > max)
			max = group;
	}
	return max;
}
					
void fusionateGroups(CvSeq* seqGroups, int g1, int g2) {
	for( int i = 0; i < seqGroups->total; i++ ) {
		int found = *CV_GET_SEQ_ELEM( int, seqGroups, i );
		if(found == g2) {
			cvSeqRemove(seqGroups, i);
			cvSeqInsert(seqGroups, i, &g1);
		}
	}
}

int getGroup(int pointPos, CvPoint pt, CvSeq* seqPoints, CvSeq* seqGroups)
{
	//search in previous points
	int group = -1;
	for( int i = 0; i < pointPos; i++ ) {
		CvPoint sp = *CV_GET_SEQ_ELEM( CvPoint, seqPoints, i ); 
		if(connectedWithPoint(pt, sp)) { //if is connected with 
			int tempGroup = *CV_GET_SEQ_ELEM( int, seqGroups, i );
			if(group != -1 && //a group connected with that point was found
					tempGroup != group) //that group was not the same group we just found now
			{ 
				//it was connected with another group, let's fusionate
				fusionateGroups(seqGroups, tempGroup, group);
			}
			group = *CV_GET_SEQ_ELEM( int, seqGroups, i );
		}
	}

	if(pointPos == 0) //return 0 is the first point
		return 0;
	else if(group == -1) //if not found, return a new group
		return getMaxValue(seqGroups) +1;
	else
		return group; //return group found
}

CvPoint pointToZero() {
	CvPoint point;
	point.x=0; point.y=0; 
	return point;
}

int optionAccept(bool onlyYesNo) {
	int key;
	if(onlyYesNo) {
		do {
			key = (char) cvWaitKey(0);
		} while(key != 'y' && key != 'n' && key != 'N');
	} else {
		do {
			key = (char) cvWaitKey(0);
		} while(key != 'n' && key != 'y' && key != 'f' && key != 'F' && key != 'b' && key != 'q');
	}

	if(key == 'y') 
		return YES;
	else if(key == 'n') 
		return NO;
	else if(key == 'N') 
		return NEVER;
	else if(key == 'f') 
		return FORWARD;
	else if(key == 'F') 
		return FASTFORWARD;
	else if(key == 'b') 
		return BACKWARD;
	else if(key == 'q') 
		return QUIT;
}

void crossPoint(IplImage * img, CvPoint point, CvScalar color, int sizeEnum) {
	int size;
	if(sizeEnum == SMALL)
		size = 6;
	else if(sizeEnum == MID)
		size = 10;
	else // if(sizeEnum == BIG)
		size = 14;

	cvLine(img,
			cvPoint(point.x - size/2, point.y),
			cvPoint(point.x + size/2, point.y),
			color,1,1);
	cvLine(img,
			cvPoint(point.x, point.y - size/2),
			cvPoint(point.x, point.y + size/2),
			color,1,1);
}

void imagePrint(IplImage *img, CvPoint point, const char *label, CvFont font, CvScalar color) {
	cvPutText(img, label, point, &font, color);
}

void imageGuiAsk(IplImage *gui, const char *labelQuestion, const char * labelOptions, CvFont font) {
	/*
	imagePrint(gui, cvPoint(25, gui->height-60), labelQuestion, font, WHITE);
	imagePrint(gui, cvPoint(25, gui->height-40), labelOptions, font, WHITE);
	cvShowImage("gui", gui);
	*/
}

void eraseGuiAsk(IplImage * gui) {
	/*
	cvRectangle(gui, cvPoint(0, gui->height-70), cvPoint(gui->width, gui->height), CV_RGB(0,0,0),CV_FILLED,8,0);
	cvShowImage("gui", gui);
	*/
}

void eraseGuiResult(IplImage * gui, bool updateImage) {
	cvRectangle(gui, cvPoint(158, 239), cvPoint(448,264), LIGHT,CV_FILLED,8,0);
	if(updateImage)
		cvShowImage("gui", gui);
}

void imageGuiResult(IplImage *gui, const char *label, CvFont font) {
	eraseGuiResult(gui, false);
	imagePrint(gui, cvPoint(160, 252), label, font, BLACK);
	cvShowImage("gui", gui);
}


void toggleGuiMark(IplImage *gui, int togglePoint) {
	if(togglePoint == TOGGLEHIP) 
		crossPoint(gui, cvPoint(165+(25/2), 114+(24/2)), MAGENTA, BIG);
	else if(togglePoint == TOGGLEKNEE) 
		crossPoint(gui, cvPoint(235+(25/2), 114+(24/2)), MAGENTA, BIG);
	else if(togglePoint == TOGGLETOE) 
		crossPoint(gui, cvPoint(308+(25/2), 114+(24/2)), MAGENTA, BIG);
	else //if(togglePoint == ZOOM) 
		crossPoint(gui, cvPoint(447+(25/2), 114+(24/2)), MAGENTA, BIG);
	cvShowImage("gui", gui);
}

void eraseGuiMark(IplImage *gui, int togglePoint) {
	if(togglePoint == TOGGLEHIP) 
		cvRectangle(gui, cvPoint(165+1, 114+1), cvPoint(165+1 + 25-1, 114+1 + 24-1), WHITE,CV_FILLED,8,0);
	else if(togglePoint == TOGGLEKNEE) 
		cvRectangle(gui, cvPoint(235+1, 114+1), cvPoint(235+1 + 25-1, 114+1 + 24-1), WHITE,CV_FILLED,8,0);
	else if(togglePoint == TOGGLETOE) 
		cvRectangle(gui, cvPoint(308+1, 114+1), cvPoint(308+1 + 25-1, 114+1 + 24-1), WHITE,CV_FILLED,8,0);
	else //if(togglePoint == ZOOM) 
		cvRectangle(gui, cvPoint(447+1, 114+1), cvPoint(447+1 + 25-1, 114+1 + 24-1), WHITE,CV_FILLED,8,0);
	cvShowImage("gui", gui);
}

void eraseGuiWindow(IplImage * gui) {
	cvRectangle(gui, cvPoint(0, 0), cvPoint(gui->width, gui->height), CV_RGB(0,0,0),CV_FILLED,8,0);
	cvShowImage("gui", gui);
}

void showScaledImage(IplImage * img, const char *title) {
	cvNamedWindow(title, 1);
	//double scale = 4;
	double scale = 2;
	IplImage* small = cvCreateImage( cvSize( cvRound (img->width/scale), cvRound (img->height/scale)), 8, img->nChannels );
	cvResize( img, small, CV_INTER_LINEAR );
	cvShowImage(title, small);
}


IplImage* changeROIThreshold(IplImage* gray, IplImage* output, CvPoint p, int threshold, int thresholdMax, int pointSize)
{
	CvRect rect;
					
	rect.x=p.x - pointSize/2; rect.y=p.y - pointSize/2;
	rect.width=pointSize; rect.height=pointSize;

	cvSetImageROI(gray, rect);
	cvSetImageROI(output, rect);
	cvThreshold(gray, output, threshold, thresholdMax, CV_THRESH_BINARY_INV);
	cvResetImageROI(gray);
	cvResetImageROI(output);

	return output;
}

//char * changeExtension(char fileName[], char newExt[])
void changeExtension(char fileName[], char newExt[])
{
	int lastPoint = strlen(fileName) -1;
	do {
		//printf("%c", fileName[lastPoint]);
	} while (fileName[lastPoint --] != '.');

	char fileName2 [strlen(fileName)];
	int i;
	for(i=0; i <= lastPoint; i ++)
		fileName2[i]=fileName[i];
	fileName2[i] = '\0';

	strcat(fileName2, newExt);
	strcpy(fileName, fileName2);

//	return fileName;
}

int findMaxInVector(std::vector<int> vect, int posStart, int posEnd) {
	int max = -1;
	for(int i=posStart; i <= posEnd ; i++)
		if(vect[i] > max)
			max = vect[i];
	return max;
}

double findMaxInVector(std::vector<double> vect, int posStart, int posEnd) {
	double max = -1;
	for(int i=posStart; i <= posEnd ; i++)
		if(vect[i] > max)
			max = vect[i];
	return max;
}


int findMinInVector(std::vector<int> vect) {
	int min = 100000;
	for(int i=0; i < vect.size() ; i++)
		if(vect[i] < min)
			min = vect[i];
	return min;
}


int findLastPositionInVector(std::vector<int> vect, int searched) {
	int lastFound = 0;
	for(int i=0; i < vect.size() ; i++)
		if(vect[i] == searched)
			lastFound = i;
	return lastFound;
}

char* stringToUpper(char* str) {
	for (int i=0; i < strlen(str); i++)
		str[i] = toupper(str[i]);
	return str;
}
