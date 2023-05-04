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
#   Copyright (C) 2023  	Xavier de Blas <xaviblas@gmail.com>
# 

#this code is implemented in util.R reduceCurveByPredictStartEnd ()

# get spline by decreasing weights
getPositionSplineLeft <- function (position)
{
	x <- 1:length(position)
	weights <- 1-(position/20)
	weights[weights <= 0.05] <- 0.05

	return (smooth.spline (x, position, w=weights))
}
# get spline by decreasing weights
getPositionSplineRight <- function (position)
{
	x <- 1:length(position)
	posTemp = abs(position-(max(position)))
	weights <- 1 - posTemp/20
	weights[weights <= 0.05] <- 0.05

	return (smooth.spline (x, position, w=weights))
}

# predict left start
predictNeededZerosAtLeft <- function (positionSplineLeft)
{
	xPre <- seq(from=-50, to=0, length.out=400)
	prediction <- predict(positionSplineLeft, x = xPre)
	#print (prediction)
	pos <- max(which(abs(prediction$y) == min(abs(prediction$y))))
	#xAtYZero <- prediction$x[pos]

	print ("needed zeros at left: ")
	print (round (abs(prediction$x[pos]), 0))

	return (c(prediction$x[pos], prediction$y[pos]))
}

# predict right end
predictNeededZerosAtRight <- function (positionSplineRight, position, maxx)
{
	xPre <- seq(from=maxx, to=maxx+50, length.out=400)
	prediction <- predict(positionSplineRight, x = xPre)

	predictionYStored <- prediction$y
	prediction$y <- abs(prediction$y - (max(position)+1))
	print ("prediction at right")
	#print (prediction$y)
	pos <- min(which(abs(prediction$y) == min(abs(prediction$y))))
	#xAtYMaxPlus1 <- prediction$x[pos]

	print ("needed zeros at right: ")
	print (round (prediction$x[pos], 0) - length(position))

	return (c(prediction$x[pos], predictionYStored[pos]))
}

process <- function (displacement, title, imageTitle, graph, startIsCon)
{
	#TODO: note here the getStableConcentricStart is not being applied, is just a test done before adding that function
	#fixEccConCutOnNotSingleFile.R is more updated implementing both

	firstInitialNonZero <- min(which(displacement != 0))
	lastFinalNonZero <- max(which(displacement != 0))

	disCut <- displacement[firstInitialNonZero:lastFinalNonZero]

	position <- cumsum (disCut)

	x <- 1:length(position)
	positionSplineCvT <- smooth.spline (x, position, cv=TRUE)
	positionSplineCvF <- smooth.spline (x, position, cv=FALSE)

	positionSplineLeft <- getPositionSplineLeft (position)
	positionSplineRight <- getPositionSplineRight (position)

	print (title)
	pointCrossY0 <- predictNeededZerosAtLeft (positionSplineLeft)
	zerosAtLeft <- abs(round(pointCrossY0[1], 0))

	pointCrossMaxYPlus1 <- predictNeededZerosAtRight (positionSplineRight, position, max(x))
	zerosAtRight <- round(pointCrossMaxYPlus1[1], 0) - length(position)
	print ("pointCrossMaxYPlus1")
	print (pointCrossMaxYPlus1)

	if (graph)
	{
		#graph
		png (imageTitle, width=1920, height=1080)
		par(mfrow=c(1,3))

		#plot left side
		yAt10 = min (which (position == 10)) #TODO: take care as this can be NA
		xlimStart = c(-zerosAtLeft, x[yAt10])
		ylimStart = c(0,10)
		if (! startIsCon)
		{
			yAt10Lower = min (which (position == position[1]-10)) #TODO: take care as this can be NA
			xlimStart = c(-zerosAtLeft, x[yAt10Lower])
			ylimStart = c(position[1]-10,position[1]+1)
		}
		plot (position, main="Start", xlab="Time (ms)", ylab="Position (mm)", xlim=xlimStart, ylim=ylimStart)
		lines (positionSplineCvT, col="green")
		lines (positionSplineCvF, col="blue")
		lines (positionSplineLeft, col="red")
		points (pointCrossY0[1], pointCrossY0[2], col="red")

		#plot right side
		yAtTopMinus10 = max (which (position == max(position) -10)) #TODO: take care as this can be NA
		xlimEnd = c(x[yAtTopMinus10], max(x)+zerosAtRight)
		ylimEnd = c(max(position)-10,max(position)+1)
		plot (position, main="End", xlab="Time (ms)", ylab="Position (mm)", xlim=xlimEnd, ylim=ylimEnd)
		lines (positionSplineCvT, col="green")
		lines (positionSplineCvF, col="blue")
		lines (positionSplineRight, col="red")
		points (pointCrossMaxYPlus1[1], pointCrossMaxYPlus1[2], col="red")

		#plot full displacement
		plot (position, main="Position", xlab="Time (ms)", ylab="Position (mm)")
		rect (xlimStart[1], ylimStart[1], xlimStart[2], ylimStart[2], border="red")
		rect (xlimEnd[1], ylimEnd[1], xlimEnd[2], ylimEnd[2], border="red")

		dev.off()
	}
}

#this data is from mastercede session, is the same repetition but initial and final zeros are slightly different
serie <- scan("serie.txt", skip=5) #concentric
sessio <- scan("sessio.txt", skip=5) #concentric
process (serie, "serie", "getCurveStartEnd_serie.png", TRUE, TRUE)
process (sessio, "sessio", "getCurveStartEnd_sessio.png", TRUE, TRUE)

#this is an ecc/con curve, try the ecc part
curve1 <- scan("curve1.txt", sep=",")
curve1 <- curve1[!is.na(curve1)]
curve1Position <- cumsum (curve1)
eccConChange <- mean(which (curve1Position == min(curve1Position)))
curve1Ecc <- curve1[1:eccConChange]
#as it is an ecc just reverse vertically for being plotted nicely (but calcs are the same)
curve1EccReversed <- -1 * curve1Ecc
process (curve1EccReversed, "curve1Ecc reversed", "getCurveStartEnd_curve1Ecc_reversed.png", TRUE, TRUE)

#this is an ecc/con curve, try the full ecc/con
curve1 <- scan("curve1.txt", sep=",")
curve1 <- curve1[!is.na(curve1)]
process (curve1, "curve1EccCon", "getCurveStartEnd_curve1EccCon.png", TRUE, FALSE)

