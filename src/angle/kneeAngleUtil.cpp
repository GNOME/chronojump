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
using namespace std;

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

//	if(debug)
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

double findAngle(CvPoint p1, CvPoint p2, CvPoint pc) //pc is center point
{
	CvPoint d1, d2;
	d1.x = p1.x - pc.x;
	d1.y = p1.y - pc.y;
	d2.x = p2.x - pc.x;
	d2.y = p2.y - pc.y;
	double dist1 = getDistance(p1, pc);
	double dist2 = getDistance(p2, pc);
	return (180.0/M_PI)*acos(((d1.x*d2.x + d1.y*d2.y))/(double)(dist1*dist2));
}

double relError(double val1, double val2)
{
	if(val2-val1 == 0 || val2 == 0)
		return -1;
	else
		return (val2-val1)/val2 *100;
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
