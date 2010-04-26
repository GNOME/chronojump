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


SEXP ansP;
std::string txtP = "";

void createPredictFunction()
{
	//don't put comments '#'
	txtP = "span = 30;	\
		threshold = 3;	\
		discard = 10;	\
				\
		if( length(x) < discard || sd(x) == 0) {			\
			n<- x[ length( x ) ];					\
		} else {							\
			which.predict <- max( 1, length(x) - span):length(x);	\
			time.1 <- 1:length( which.predict );			\
			x.1    <- x[ which.predict ];				\
										\
			resid  <- x.1 - predict( lm( x.1 ~ time.1 ), data.frame( time.1 = time.1 ) );	\
			x.outlier <- abs( resid ) > threshold * sd( resid );	\
										\
			time.1   <- time.1[!x.outlier];				\
			x.1      <- x.1   [!x.outlier];				\
										\
			if( is.na( sd(x.1) ) ) {				\
				n<-  x[ length( x ) ];				\
			} else {						\
				n<- round (					\
				predict( lm( x.1 ~ time.1 ), newdata = data.frame( time.1 = length( time.1 ) + 1 ) ) 	\
				);						\
			}							\
		}		\
		";

	R.assign( txtP, "txtP"); 
}

int predictDo(std::vector<int> vect) {
	if( ! vect.empty() ) {
		R.assign(vect, "x");
		R.parseEval(txtP, ansP);
		return(Rcpp::as< int >(ansP));
	} else return 0;
}

CvSeq* predictPoints() {
	if(txtP=="") 
		createPredictFunction();
	
	CvMemStorage* storage = cvCreateMemStorage(0);
	CvSeq* seqPoints = cvCreateSeq( CV_SEQ_KIND_GENERIC|CV_32SC2, sizeof(CvSeq), sizeof(CvPoint), storage );
	CvPoint hip;
	CvPoint knee;
	CvPoint toe;
	
	hip.x =  predictDo(hipXVector);
	hip.y =  predictDo(hipYVector);
	knee.x = predictDo(kneeXVector);
	knee.y = predictDo(kneeYVector);
	toe.x =  predictDo(toeXVector);
	toe.y =  predictDo(toeYVector);
	
	//PASS to cvSeq	
	cvSeqPush( seqPoints, &hip );
	cvSeqPush( seqPoints, &knee );
	cvSeqPush( seqPoints, &toe );
	
	return seqPoints;
}

