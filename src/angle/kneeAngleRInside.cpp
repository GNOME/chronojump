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
# Copyright (C) 2010   Xavier de Blas <xaviblas@gmail.com> , Carlos J. Gil Bellosta <cgb@datanalytics.com>
 *
 */


#include "stdio.h"
#include <sys/wait.h>
#include <string>


CvSeq* predictPointsRInside()
{
	/* TESTING:

	R.assign(hipXVector, "d1");
	R.assign(hipYVector, "d2");
	R.assign(kneeXVector, "d3");
	R.assign(kneeYVector, "d4");
	R.assign(toeXVector, "d5");
	R.assign(toeYVector, "d6");
	std::string prova =               // now access in R
		"cat('\nd1=', d1, '\n');"
		"cat('\nd2=', d2, '\n');"
		"cat('\nd3=', d3, '\n');"
		"cat('\nd4=', d4, '\n');"
		"cat('\nd5=', d5, '\n');"
		"cat('\nd6=', d6, '\n');";
	R.parseEvalQ(prova);
	*/

	CvMemStorage* storage = cvCreateMemStorage(0);
	CvSeq* seqPoints = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), storage );
	CvPoint hip;
	hip.x =0; hip.y=0;
	CvPoint knee;
	knee.x =0; knee.y=0;
	CvPoint toe;
	toe.x =0; toe.y=0;

	SEXP ans;
	std::string txt = "			\
		span = 30;			\
		threshold = 3;			\
		discard = 10;			\
						\
		maxX = 512;			\
		maxY = 384;			\
		#cat('(', length(x), '): ', x[length(x)]);		\
												\
		if( length(x) < discard || sd(x) == 0) {					\
			n<- x[ length( x ) ];							\
		} else {									\
			which.predict <- max( 1, length(x) - span):length(x);			\
			time.1 <- 1:length( which.predict );					\
			x.1    <- x[ which.predict ];						\
												\
			resid  <- x.1 - predict( lm( x.1 ~ time.1 ), data.frame( time.1 = time.1 ) );	\
			x.outlier <- abs( resid ) > threshold * sd( resid );			\
												\
			time.1   <- time.1[!x.outlier];						\
			x.1      <- x.1   [!x.outlier];						\
												\
			if( is.na( sd(x.1) ) ) {						\
				n<-  x[ length( x ) ];						\
			} else {								\
				n<- round (							\
				predict( lm( x.1 ~ time.1 ), newdata = data.frame( time.1 = length( time.1 ) + 1 ) ) 	\
				);								\
			}									\
		}				\
		";

	R.assign( txt, "txt"); 
	
	//HIP
	if(hipXVector.empty())
		hip.x = -1;
	else {
		R.assign(hipXVector, "x");
		R.parseEval(txt, ans);
		hip.x = Rcpp::as< int >(ans);
	}
	
	if(hipYVector.empty())
		hip.y = -1;
	else {
		R.assign(hipYVector, "x");
		R.parseEval(txt, ans);
		hip.y = Rcpp::as< int >(ans);
	}

	//KNEE
	if(kneeXVector.empty())
		knee.x = -1;
	else {
		R.assign(kneeXVector, "x");
		R.parseEval(txt, ans);
		knee.x = Rcpp::as< int >(ans);
	}

	if(kneeYVector.empty())
		knee.y = -1;
	else {
		R.assign(kneeYVector, "x");
		R.parseEval(txt, ans);
		knee.y = Rcpp::as< int >(ans);
	}

	//TOE
	if(toeXVector.empty())
		toe.x = -1;
	else {
		R.assign(toeXVector, "x");
		R.parseEval(txt, ans);
		toe.x = Rcpp::as< int >(ans);
	}

	if(toeYVector.empty())
		toe.y = -1;
	else {
		R.assign(toeYVector, "x");
		R.parseEval(txt, ans);
		toe.y = Rcpp::as< int >(ans);
	}
		
	//PASS to cvSeq	
	cvSeqPush( seqPoints, &hip );
	cvSeqPush( seqPoints, &knee );
	cvSeqPush( seqPoints, &toe );
	
	return seqPoints;
}

