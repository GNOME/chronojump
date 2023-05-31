# 
#  This file is part of ChronoJump
# 
#  ChronoJump is free software; you can redistribute it and/or modify
#   it under the terms of the GNU General Public License as published by
#    the Free Software Foundation; either version 2 of the License, or   
#     (at your option) any later version.
#     
#  ChronoJump is distributed in the hope that it will be useful,
#   but WITHOUT ANY WARRANTY; without even the implied warranty of
#    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
#     GNU General Public License for more details.
# 
#  You should have received a copy of the GNU General Public License
#   along with this program; if not, write to the Free Software
#    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
# 
#   Copyright (C) 2014-2023  	Xavier de Blas <xaviblas@gmail.com>
#   Copyright (C) 2014-2020   	Xavier Padullés <x.padulles@gmail.com>
# 



#getSpeedButterworth <- function (displacement, smoothing)
#{
#	#library("signal")
#	#bf <- butter(2, 1/50, type="low");
#	bf <- butter(2, 1/24, type="low");
#	x <- 1:length(displacement);
#	b <- filter(bf, displacement);
#	#plot(x, b, lwd = 5, col = "green", type="l")
#
#	speedSpline <- smooth.spline(x, b, spar=0) #don't do smoothing, just convert speed to an spline
#}
#
## getSpeed using weights on initial and final zeros
#getSpeedSplineUsingWeights <- function(displacement, smoothing)
#{
#	if(length(displacement) < 4)
#		return(list(y=rep(0,length(displacement))))
#
#	spar <- smoothing
#	firstInitialNonZero <- min(which(displacement != 0)) #take care on boundaries
#	lastFinalNonZero <- max(which(displacement != 0)) #take care on boundaries
#	#w <- c(rep(.01,firstInitialNonZero -1), rep(1,lastFinalNonZero-firstInitialNonZero+1), rep(.01,length(displacement)-lastFinalNonZero))
#	#w <- c(rep(4,firstInitialNonZero -1), rep(5,lastFinalNonZero-firstInitialNonZero+1), rep(1,length(displacement)-lastFinalNonZero))
#
#	w <- c(rep(.25,firstInitialNonZero -1), rep(1,lastFinalNonZero-firstInitialNonZero+1), rep(.25,length(displacement)-lastFinalNonZero))
#	#w[1] = 1
#	#w[length(w)] = 1
#
#	position <- cumsum(displacement)
#	positionSpline <- smooth.spline(x=1:length(position), y=position, w=w, spar=spar)
#	#positionSpline <- smooth.spline(x=1:length(position), y=position, w=w, all.knots=TRUE, spar=spar)
#
#	speed <- c(diff(positionSpline$y)[1],diff(positionSpline$y))
#	speedSpline <- smooth.spline( 1:length(speed), speed, spar=0) #don't do smoothing, just convert speed to an spline
#}
#
## getSped deleting initial and final zeros except 1st and last
## not nice, too high spikes on the extremes
#getSpeedSplineTrimmingZerosInitialAndEnd <- function(displacement, smoothing)
#{
#	if(length(displacement) < 4)
#		return(list(y=rep(0,length(displacement))))
#
#	firstInitialNonZero = min(which(displacement != 0))
#	lastFinalNonZero = max(which(displacement != 0))
#
#	dAbsolute = cumsum(displacement)
#
#	x=1:length(displacement)
#	xcut=c(1,x[firstInitialNonZero:lastFinalNonZero],length(displacement))
#	ycut=c(0,dAbsolute[firstInitialNonZero:lastFinalNonZero],dAbsolute[length(dAbsolute)])
#
#	positionSpline <- smooth.spline(xcut, ycut, spar=.7)
#
#	speed = c(diff(positionSpline$y)[1],diff(positionSpline$y))
#	speedSpline <- smooth.spline( 1:length(speed), speed, spar=0) #don't do smoothing, just convert speed to an spline
#}

#unused since 1.6.0 because first and last displacement values make change too much the initial and end curve
#getSpeedOld <- function(displacement, smoothing) {
#	#no change affected by encoderConfiguration
#	return (smooth.spline( 1:length(displacement), displacement, spar=smoothing))
#}
getSpeed <- function(displacement, smoothing)
{
	#x vector should contain at least 4 different values (at positionSpline)
	if (length (unique (cumsum (displacement))) < 4)
		return(list(y=rep(0,length(displacement))))

	#no change affected by encoderConfiguration
	
	#use position because this does not make erronously change the initial and end of the curve
	#do spline with displacement is buggy because data is too similar -2, -1, 0, 1, 2, ...
	position <- cumsum(displacement)
	
	spar <- smoothing
	if(CROSSVALIDATESMOOTH)
		spar <- cvParCall(1:length(position), position)

	positionSpline <- smooth.spline( 1:length(position), position, spar=spar)

	#using cv or using smooth.Pspline are much more agressive than using our traditional spar=0.7
	#positionSpline <- smooth.spline( 1:length(position), position, cv=TRUE)
	#positionSpline <- smooth.spline( 1:length(position), position, cv=FALSE)
	#library("pspline")
	#positionSpline <- smooth.Pspline(1:length(position), position, method=3)
	#positionSpline <- smooth.Pspline(1:length(position), position, method=4)
	
	#get speed converting from position to displacement, but repeat first value because it's missing on the diff process
	#do not hijack spline like this because this works for speed, but then acceleration has big error
	#speedSpline = positionSpline
	#speedSpline$y = c(diff(positionSpline$y)[1],diff(positionSpline$y))

	speed = c(diff(positionSpline$y)[1],diff(positionSpline$y))
	speedSpline <- smooth.spline( 1:length(speed), speed, spar=0) #don't do smoothing, just convert speed to an spline

	#aixi dona una diferència de 9 al test de mastercede a encoder/tests
	#[1] 2073.435
	#[1] 2082.263

	return (speedSpline)
}


#########################################
# reduceCurveByPredictStartEnd ----> ####
#########################################
#
# discarded method because spline sometimes gives strange curvature changes like:
#       /
#      /
#   /\/
#  /
#--
#
#getPositionSplineLeft <- function (position)
#{
#	if (length(unique(position)) < 4)
#		return (NULL)
#
#	x <- 1:length(position)
##	weights <- 1-(position/40)
##	print ("position/20")
##	print (position/20)
#	weights <- rep (1, length(position))
#	weights[weights <= 0.05] <- 0.05
#	print ("weights")
#	print (weights)
#
#	return (smoothSplineWeightedSafe (x, position, weights))
#}
# get spline by decreasing weights
#getPositionSplineRight <- function (position)
#{
#	if (length(unique(position)) < 4)
#		return (NULL)
#
#	x <- 1:length(position)
#	posTemp = abs(position-(max(position)))
#	weights <- 1 - posTemp/20
#	weights[weights <= 0.05] <- 0.05
#
#	return (smoothSplineWeightedSafe (x, position, weights))
#}

## predict left start
#predictNeededZerosAtLeft <- function (positionSplineLeft)
#{
#	xPredict <- seq(from=-50, to=0, length.out=400) #get x from (0) to 50 left
#	prediction <- predict(positionSplineLeft, x = xPredict)
#	print ("prediction")
#	print (prediction)
#	pos <- max(which(abs(prediction$y) == min(abs(prediction$y))))
#
#	return (c(prediction$x[pos], prediction$y[pos]))
#}
#
## predict right end
#predictNeededZerosAtRight <- function (positionSplineRight, position, maxx)
#{
#	xPredict <- seq(from=maxx, to=maxx+50, length.out=400) #get x from max(x) to 50 right
#	prediction <- predict(positionSplineRight, x = xPredict)
#	predictionYStored <- prediction$y
#	prediction$y <- abs(prediction$y - (max(position)+1))
#	pos <- min(which(abs(prediction$y) == min(abs(prediction$y))))
#
#	return (c(prediction$x[pos], predictionYStored[pos]))
#}


# do not this because trying to find this:
#                /
#               /
#           __ /
#          .
#
# what we find this this:
#                /
#          __   /
#            \./
#
#predictNeededZerosAtLeftIterating <- function (displacement)
#{
#	position <- cumsum (displacement)
#
#	for (i in 3:20)
#	{
#		# 1 find the first i values
#		firstNonZeroPos <- head (which (displacement [2:length(displacement)] != 0), n = i)
#
#		# if there are less than i values, just return the number of initial zeros (the same will be used)
#		if (length (firstNonZeroPos) < i)
#			return (firstNonZeroPos[1])
#
#		# 2 try to find the x at min (position) -1
#		xAtDesiredY <- getXatY (firstNonZeroPos, position[firstNonZeroPos], min(position) -1)
#
#		# if is.nan, the parabole does not pass by the point, we can increase the number of values or just return the num of initial zeros
#		if (! is.nan (xAtDesiredY))
#		{
#			print (paste ("found", xAtDesiredY, "for i =", i))
#			return (xAtDesiredY)
#		}
#	}
#
#	return (firstNonZeroPos[1])
#}

