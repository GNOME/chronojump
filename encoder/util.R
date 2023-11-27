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


#extrema function is part of R EMD package
#It's included here to save time, because 'library("EMD")' is quite time consuming

#Caution: do not 'print, cat' stuff because (on captureR) it's readed from gui/encoder as results
#it can be printed safely to stderr. See end capture.R

#used in graph.R and capture.R
assignOptions <- function(options) {
	return(list(
		    File		= options[1],        
		    OutputGraph		= options[2],
		    OutputData1		= options[3],
		    EncoderRScriptsPath = options[4],
		    EncoderTempPath 	= options[5],
		    FeedbackFileBase 	= paste(options[5], "/chronojump-encoder-status-", sep=""),
		    SpecialData 	= paste(options[5], "/chronojump-special-data.txt", sep=""),
		    MinHeight		= as.numeric(options[6])*10, #from cm to mm
		    ExercisePercentBodyWeight = as.numeric(options[7]),
		    MassBody		= as.numeric(options[8]),
		    MassExtra		= as.numeric(options[9]),
		    Eccon		= options[10],
		    #in Analysis "cross", AnalysisVariables can be "Force;Speed;mean". 1st is Y, 2nd is X. "mean" can also be "max"
		    #Analysis "cross" can have a double XY plot, AnalysisVariables = "Speed,Power;Load;mean"
		    #	1st: Speed,power are Y (left and right), 2n: Load is X.
		    #In interssession, x can be date. In order to recicle paintCrossVariables, a 4th member is sent: "Date".
		    #	Power;Load;mean;Date will be an Power / Date (with Load as seriesName)
		    #	Speed;Load;mean;Date will be an Speed / Date (with Load as seriesName)
		    #	Force;Load;mean;Date will be an Force / Date (with Load as seriesName)
		    #
		    #Intersession Pmax(F0,V0) has not Date on X, so AnalusisVariables are:
		    # 	Pmax(F0,V0);Pmax(F0,V0);mean
		    #
		    #in Analysis "powerBars", AnalysisVariables can be:
		    #	"TimeToPeakPower;Range", or eg: "NoTimeToPeakPower;NoRange"
		    #
		    #in Analysis "single" or "side", AnalysisVariables can be:
		    #	"Position;Speed;Accel;Force;Power", or eg: "NoPosition;NoSpeed;NoAccel;Force;Power"
		    #
		    #in Analysis = "1RMAnyExercise"
		    #AnalysisVariables = "0.185;method". speed1RM = 0.185m/s
		    Analysis		= options[11],	
		    AnalysisVariables	= unlist(strsplit(options[12], "\\;")),
		    AnalysisOptions	= options[13],
		    #CheckFullyExtended	= as.numeric(options[14]),
		    CheckFullyExtended	= 0, 	#deactivated just after 1.7.0
		    EncoderConfigurationName =	options[15],	#just the name of the EncoderConfiguration	
		    diameter		= as.numeric(unlist(strsplit(options[16], "\\;"))), #comes in cm, will be converted to m. Since 1.5.1 can be different diameters separated by ;
		    #diameter    = getInertialDiametersPerTick(as.numeric(unlist(strsplit("1.5; 1.75; 2.65; 3.32; 3.95; 4.07; 4.28; 4.46; 4.54; 4.77; 4.96; 5.13; 5.3; 5.55", "\\;")))),
		    diameterExt		= as.numeric(options[17]),	#comes in cm, will be converted to m
		    anglePush 		= as.numeric(options[18]),
		    angleWeight 	= as.numeric(options[19]),
		    inertiaMomentum	= (as.numeric(options[20])/10000.0),	#comes in Kg*cm^2 eg: 100; convert it to Kg*m^2 eg: 0.010
		    gearedDown 		= readFromFile.gearedDown(as.numeric(options[21])),

		    SmoothingOneC	= as.numeric(options[22]),
		    Jump		= options[23],
		    Width		= as.numeric(options[24]),
		    Height		= as.numeric(options[25]),
		    DecimalSeparator	= options[26],
		    Title		= options[27],
		    OperatingSystem	= options[28],	#if this changes, change it also at call_graph.R
		    Debug		= options[31],
		    CrossValidate	= options[32],
		    TriggersCut 	= options[33],  #if TRUE ten cut by triggers, else use TriggersOnList (if any) only for vertical ablines on instaneous graphs
		    TriggersOnList	= as.numeric(unlist(strsplit(options[34], "\\;"))),

		    #Triggers:
		    #  at capture.R
		    #  		if triggers are used to cut: TriggersCut == TRUE, TriggersOnList == 1
		    #  		if not use triggers: TriggersCut == FALSE, TriggersOnList == -1 #but nothing will be plotted on capture
		    #  at graph.R
		    # 		if triggers are used to cut: TriggersCut == TRUE, TriggersOnList == xxx, yyy, zzz, ...
		    # 		... but if there are not enough triggers, then just plot
		    #  		if not use triggers: TriggersCut == FALSE, TriggersOnList == xxx, yyy, zzz, ...

		    #Unassigned here:
		    #	englishWords [29]
		    #	translatedWords [30]
		    SeparateSessionInDays = options[35],
		    AnalysisMode 	 = options[36],
		    InertialGraphX	 = options[37]
		    ))
}

cutByTriggers <- function(op)
{
	if(op$TriggersCut == TRUE && op$TriggersOnList != -1)
		return(TRUE);

	return(FALSE);
}


#gearedDown comes as:
#4 and should be converted to 4
#-4 and should be converted to 1/4 : 0.25
#more info at GearedUpDisplay() on EncoderConfiguration C# class

readFromFile.gearedDown <- function(gd) {
	if(gd > 0)
		return(gd)

	return( abs( 1 / gd ) )
}



extrema <- function(y, ndata = length(y), ndatam1 = ndata - 1) {

	minindex <- maxindex <- NULL; nextreme <- 0; cross <- NULL; ncross <- 0 

	z1 <- sign(diff(y))
	index1 <- seq(1, ndatam1)[z1 != 0]; z1 <- z1[z1 != 0]  

	if (!(is.null(index1) || all(z1==1) || all(z1==-1))) {

		index1 <- index1[c(z1[-length(z1)] != z1[-1], FALSE)] + 1 
		z1 <- z1[c(z1[-length(z1)] != z1[-1], FALSE)]  

		nextreme <- length(index1)

		if(nextreme >= 2)
			for(i in 1:(nextreme-1)) {
				tmpindex <- index1[i]:(index1[i+1]-1)
				if(z1[i] > 0) {
					tmpindex <- tmpindex[y[index1[i]] == y[tmpindex]]
					maxindex <- rbind(maxindex, c(min(tmpindex), max(tmpindex)))
				} else {
					tmpindex <- tmpindex[y[index1[i]] == y[tmpindex]]
					minindex <- rbind(minindex, c(min(tmpindex), max(tmpindex)))
				}     
			} 

		tmpindex <- index1[nextreme]:ndatam1  
		if(z1[nextreme] > 0) {
			tmpindex <- tmpindex[y[index1[nextreme]] == y[tmpindex]]
			maxindex <- rbind(maxindex, c(min(tmpindex), max(tmpindex)))
		} else {
			tmpindex <- tmpindex[y[index1[nextreme]] == y[tmpindex]]
			minindex <- rbind(minindex, c(min(tmpindex), max(tmpindex)))
		}  

		### Finding the index of zero crossing  

		if (!(all(sign(y) >= 0) || all(sign(y) <= 0) || all(sign(y) == 0))) {
			index1 <- c(1, index1)
			for (i in 1:nextreme) {
				if (y[index1[i]] == 0) {
					tmp <- c(index1[i]:index1[i+1])[y[index1[i]:index1[i+1]] == 0]
					cross <- rbind(cross, c(min(tmp), max(tmp)))                 
				} else
					if (y[index1[i]] * y[index1[i+1]] < 0) {
						tmp <- min(c(index1[i]:index1[i+1])[y[index1[i]] * y[index1[i]:index1[i+1]] <= 0])
						if (y[tmp] == 0) {
							tmp <- c(tmp:index1[i+1])[y[tmp:index1[i+1]] == 0]
							cross <- rbind(cross, c(min(tmp), max(tmp))) 
						} else 
							cross <- rbind(cross, c(tmp-1, tmp)) 
					}
			}
			#if (y[ndata] == 0) {
			#    tmp <- c(index1[nextreme+1]:ndata)[y[index1[nextreme+1]:ndata] == 0]
			#    cross <- rbind(cross, c(min(tmp), max(tmp)))         
			#} else
			if (any(y[index1[nextreme+1]] * y[index1[nextreme+1]:ndata] <= 0)) {
				tmp <- min(c(index1[nextreme+1]:ndata)[y[index1[nextreme+1]] * y[index1[nextreme+1]:ndata] <= 0])
				if (y[tmp] == 0) {
					tmp <- c(tmp:ndata)[y[tmp:ndata] == 0]
					cross <- rbind(cross, c(min(tmp), max(tmp))) 
				} else
					cross <- rbind(cross, c(tmp-1, tmp))
			}
			ncross <- nrow(cross)        
		}
	}

	#extrema cross working:
	#a=c(-3,-2,-1,0,1,2,1)
	#extrema(a)$cross
	#     [,1] [,2]
	#[1,]    4    4
	#extrema(a)$ncross
	#[1] 1
	#
	#But, if there's no change of direction, cross does not find anything:
	#a=c(-3,-2,-1,0,1,2)
	#extrema(a)$cross
	#NULL
	#extrema(a)$ncross
	#[1] 0
	#
	#then find a cross in this situation. Find just one cross.
	if(ncross == 0)	{
		positiveAtStart = (y[1] >= 0)
		for(i in 1:length(y)) {
			if( (y[i] >= 0) != positiveAtStart) { #if there's a sign change
				cross = rbind(cross, c(i-1,i))
				ncross = 1
				break
			}
		}
	}

	list(minindex=minindex, maxindex=maxindex, nextreme=nextreme, cross=cross, ncross=ncross)
}


findTakeOff <- function(forceConcentric, maxSpeedTInConcentric) 
{
	#this can be a problem because some people does an strange countermovement at start of concentric movement
	#this people moves arms down and legs go up
	#at this moment can be a force == 0, and with this method can be detected as takeoff
	#if this happens, takeoff will be very early and detected jump will be very high
	#takeoff = min(which(force[concentric]<=0)) + length_eccentric + length_isometric

	#then: find the force == 0 in concentric that is closer to max speed time

	#------- example ------
	#force=c(2000,1800,1600,1000,400,100,-10,-25,-5,150,400,600,200,11,-20,-60,-120,-40,5,150)
	#maxSpeedT=17

	#df=data.frame(force<0,force,abs(1:length(force)-maxSpeedT))
	#colnames(df)=c("belowZero","force","dist")

	#df2 = subset(df,subset=df$belowZero)
	#> df2
	#	   belowZero force dist
	#	7       TRUE   -10   10
	#	8       TRUE   -25    9
	#	9       TRUE    -5    8
	#	15      TRUE   -20    2
	#	16      TRUE   -60    1
	#	17      TRUE  -120    0
	#	18      TRUE   -40    1

	#min(which(df2$dist == min(df2$dist)))
	#[1] 6
	#------- end of example ------

	#1 create df dataFrame with forceData and it's distance to maxSpeedT
	df=data.frame(forceConcentric < 0, forceConcentric, abs(1:length(forceConcentric)-maxSpeedTInConcentric))
	colnames(df)=c("belowZero","force","dist")

	#2 create df2 with only the rows where force is below or equal zero
	df2 = subset(df,subset=df$belowZero)

	#3 find takeoff as the df2 row with less distance to maxSpeedT
	df2row = min(which(df2$dist == min(df2$dist)))
	takeoff = as.integer(rownames(df2)[df2row])

	return(takeoff)
}

#find the absolute distance reached on ec curve
findDistanceAbsoluteEC <- function(position)
{
	#	-
	#	 \       /
	#	  \     /
	#	   \   /
	#	    \_/
	#	A    B   C

	#1st, find B value
	minB = min(position)
	minBms = min(which(position == minB))
	maxA = max(position[1:minBms])
	maxC = max(position[minBms:length(position)])

	return ( (maxA - minB) + (maxC - minB) )
}

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


	speed = c(diff(positionSpline$y)[1],diff(positionSpline$y))
	speedSpline <- smooth.spline( 1:length(speed), speed, spar=0) #don't do smoothing, just convert speed to an spline

	return (speedSpline)
}

#same smoothing than getSpeed
getPositionSmoothed <- function(displacement, smoothing) {
	#no change affected by encoderConfiguration
	
	#use position because this does not make erronously change the initial and end of the curve
	#do spline with displacement is buggy because data is too similar -2, -1, 0, 1, 2, ...
	position <- cumsum(displacement)

	spar <- smoothing
	if(CROSSVALIDATESMOOTH)
		spar <- cvParCall(1:length(displacement), displacement)

	positionSpline <- smooth.spline( 1:length(position), position, spar=spar)

	return (positionSpline$y)
}


getAccelerationSafe <- function(speed) {
	#x vector should contain at least 4 different values
	if(length(speed) >= 4)
		return(getAcceleration(speed))
	else
		return(list(y=rep(0,length(speed))))
}
getAcceleration <- function(speed) {
	#no change affected by encoderConfiguration
	return (predict( speed, deriv=1 ))
}

smoothSplineSafe <- function(a,b) 
{
	out <- tryCatch(
			{
				smooth.spline(a,b)
			},        
			error=function(cond) {
				message(cond)
				return(NULL)
			}
			)
	return (out)
}
smoothSplineWeightedSafe <- function(a, b, weights)
{
	out <- tryCatch(
			{
				smooth.spline(a, b, w=weights)
			},
			error=function(cond) {
				message(cond)
				return(NULL)
			}
			)
	return (out)
}

#gearedDown is positive, normally 2
#this is not used on inertial machines
getMass <- function(mass, gearedDown, angle) {
	if(mass == 0)
		return (0)

	#default value of angle is 90 degrees. If is not selected, it's -1
	if(angle == -1)
		angle = 90

	return ( ( mass / gearedDown ) * sin( angle * pi / 180 ) )
}

# 2023 getStableConcentricStart && reduceCurveByPredictStartEnd
#
# The initial / final zeros affect a lot to the power. reduceCurveBySpeed depends on this initial zeros, and some parts of the program like analysis set and analysis session sometimes have different number of zeros affecting the result.
# This functions deletes initial zeros (as we do not know if movement started)
# And calculates where the initial zero should be from a regression from right to left
# And reconstruct the displacement
# Check tests/fixEccConCutOnNotSingleFile/getCurveStartEnd.R
# Check tests/fixEccConCutOnNotSingleFile/fixEccConCutOnNotSingleFile.R

#######################################
#  getStableConcentricStart ----> #####
#######################################

# i is the position we are searching if has n zeros at left
hasNZerosAtLeft <- function (displacement, i, n)
{
	if (i <= n)
		return (FALSE)

	for (j in seq(i-1, i-n))
		if (displacement[j] != 0)
			return (FALSE)

	return (TRUE)
}

zerosAtLeft <- function (displacement, i)
{
	zeros <- 0
	if (i == 1)
		return (0)

	for (j in seq(i-1, 1))
	{
		if (displacement [j] == 0)
			zeros <- zeros +1
		else
			return (zeros)
	}

	return (zeros)
}

# line ___ it is 1 mm below the line ------
#                            con
#                              /
#                             t
#                            /
#                        ---s
#                  -----s
#      -----------S
#-----s
# this function finds s that has at least 30 ms of stability at left
# t is the minHeight needed for being a repetition
# Considers also that from s,S to top has to be >= minHeight
# in the graph s,S have 30 zeros or more at left
# S is the point below t that has more zeros at left
#
# for an exemple of this working do:
# d <- scan("curve1.txt", sep=",")
# d <- d[!is.na(d)]
# plot (cumsum (d), type="l")
# abline (v = getStableConcentricStart(d, 20))
# abline (v = reduceCurveByPredictStartEnd (d, "c", 20)$startPos, col="red")
getStableConcentricStart <- function (displacement, minHeight)
{
	#displacementDebug <<- displacement #TODO just to debug, comment this
	position <- cumsum (displacement)

	if (max (position) < minHeight)
		return (1)

	t <- min (which (position >= minHeight))

	nZerosAtLeft <- 30
	if (t - nZerosAtLeft <= 1)
		return (1)

	storedSample <- -1
	for (j in seq (t, nZerosAtLeft))
		if (position[j] < position[t] &&
		    hasNZerosAtLeft (displacement, j, nZerosAtLeft) &&
		    max(position) - position[j] >= minHeight)
		{
			if (storedSample < 0)
			{
				storedSample <- j
				#storedHeight <- position[j]
				storedNZerosAtLeft <- zerosAtLeft (displacement, j)
			} else
			{
				zerosAtLeft = zerosAtLeft (displacement, j)
				if (zerosAtLeft > storedNZerosAtLeft)
				{
					storedSample <- j
					#storedHeight <- position[j]
					storedNZerosAtLeft <- zerosAtLeft
				}
			}
		}

	if (storedSample > 0)
		return (storedSample)

	return (1)
}

# reverse vertically and getStableConcentricStart
# to find A, convert, find B, A == B
# FROM 0,0,0,-1,-1,-2, ...  TO: 0,0,0,1,1,2, ...
#    FROM                TO
#                         ----
#                        /
#                       /
# 0 -A-\            -B-/
#       \
#        \
#         ----
getStableEccentricStart <- function (displacement, minHeight)
{
	return (getStableConcentricStart (-1 * displacement, minHeight))
}

# reverse horizontally, getStableConcentricStart, and then horizontally reverse the value again
# and vertically to have the - as +
# to find A, convert, find B, A = length - B
# FROM 0,0,0,-1,-1,-2, ...  TO: 0,0,0,1,1,2, ...
#    FROM                TO
#                         --
#                        /
#                       /
# 0 --\            -B--/
#      \
#       \
#        -A--
getStableEccentricEnd <- function (displacement, minHeight)
{
	return (length (displacement) - getStableConcentricStart (-1 * rev (displacement), minHeight))
}

# reverse horizontally, getStableConcentricStart
# to find A, convert (simply reverse horizontally) , find B, A = length - B
#    FROM                 TO
#          -A-             ----
#         /               /
#        /               /
# 0 ----/            -B-/
getStableConcentricEnd <- function (displacement, minHeight)
{
	return (length (displacement) - getStableConcentricStart (rev (displacement), minHeight))
}

#######################################
#  <---- getStableConcentricStart #####
#######################################


#########################################
# reduceCurveByPredictStartEnd ----> ####
#########################################

getParabole <- function(x, y)
{
	fit = lm(y ~ x + I(x^2))

        coef.a <- fit$coefficient[3] #the quadratic component
        coef.b <- fit$coefficient[2]
        coef.c <- fit$coefficient[1]

	#print (c("coefs a,b,c", coef.a, coef.b, coef.c))
	return (list (
		      a = as.numeric (coef.a),
		      b = as.numeric (coef.b),
		      c = as.numeric (coef.c)
		      ))
}
getXatY <- function (x, y, yDesired)
{
	coefs_l <- getParabole (x,y)
	a <- coefs_l$a
	b <- coefs_l$b
	c <- coefs_l$c

	# generic
	# y = ax2 + bx + c
	# x <- (-b +- sqrt(b2 -4ac)) / 2a
	# 
	# yDesired = ax2 + bx + c
	# 0 = ax2 + bx + c - yDesired
	# C = c - yDesired
	# x <- (-b +- sqrt(b2 -4aC)) / 2a

	C <- c - yDesired

	#return ( (-b + sqrt (b^2 - 4 * a * C)) / (2 * a) ) #this will be at right (we do not want it)
	return ( (-b - sqrt (b^2 - 4 * a * C)) / (2 * a) )
}

predictNeededZerosAtLeft <- function (displacement)
{
	# 1 find the first 3 values
	firstThreeNonZeroPos <- head (which (displacement [2:length(displacement)] != 0), n = 3)

	# if there are less than 3 values, just return the number of initial zeros (the same will be used)
	if (length (firstThreeNonZeroPos) < 3)
		return (firstThreeNonZeroPos[1])

	position <- cumsum (displacement)

	# 2 try to find the x at min (position) -1
	xAtDesiredY <- getXatY (firstThreeNonZeroPos, cumsum(position)[firstThreeNonZeroPos], min(position) -1)

	# if is.nan, the parabole does not pass by the point, we can increase the number of values or just return the num of initial zeros
	if (is.nan (xAtDesiredY) || xAtDesiredY < 0)
		return (firstThreeNonZeroPos[1])

	# 3 detected num of initial zeros
	return (round (xAtDesiredY, 0))
}


# This should work for all eccons, check tests/fixEccConCutOnNotSingleFile/getCurveStartEnd.R
# and the example on getStableConcentricStart
reduceCurveByPredictStartEnd <- function (displacement, eccon, minHeight)
{
	print ("reduceCurveByPredictStartEnd start")
	displacementLengthStored <- length (displacement)

	# 1 cut by getStableConcentricStart, getStableEccentricStart
	startByStability <- 1
	endByStability <- length (displacement)

	if (eccon == "c")
	{
		startByStability <- getStableConcentricStart (displacement, minHeight)
		endByStability <- getStableConcentricEnd (displacement, minHeight)
	}
	else if (eccon == "e")
	{
		startByStability <- getStableEccentricStart (displacement, minHeight)
		endByStability <- getStableEccentricEnd (displacement, minHeight)
	}
	else if (eccon == "ec")
	{
		startByStability <- getStableEccentricStart (displacement, minHeight)
		endByStability <- getStableConcentricEnd (displacement, minHeight)
	}

	displacement <- displacement [startByStability:endByStability]

	# 2 delete initial/final zeros
	firstInitialNonZero <- min(which(displacement != 0))
	lastFinalNonZero <- max(which(displacement != 0))

	if (is.infinite (firstInitialNonZero))
		firstInitialNonZero <- 1
	if (is.infinite (lastFinalNonZero))
		lastFinalNonZero <- length(displacement)

	if (firstInitialNonZero >= lastFinalNonZero)
		return (list (
			      curve = displacement,
			      startPos = 1,
			      endPos = length (displacement)
			      ))

	displacement <- displacement[firstInitialNonZero:lastFinalNonZero]

	zerosAtLeft <- 0
	zerosAtRight <- 0

	if (eccon == "c")
	{
		zerosAtLeft <- predictNeededZerosAtLeft (displacement)
		zerosAtRight <- predictNeededZerosAtLeft (rev (displacement))
	}
	else if (eccon == "e")
	{
		zerosAtLeft <- predictNeededZerosAtLeft (-1 * displacement)
		zerosAtRight <- predictNeededZerosAtLeft (rev (-1 * displacement))
	}
	else if (eccon == "ec")
	{
		zerosAtLeft <- predictNeededZerosAtLeft (-1 * displacement)
		zerosAtRight <- predictNeededZerosAtLeft (rev (displacement))
	}

	print (c("zerosAtLeft, zerosAtRight", zerosAtLeft, zerosAtRight))

	startPos <- startByStability + firstInitialNonZero-1 - zerosAtLeft
	endPos <- startByStability + firstInitialNonZero-1 + lastFinalNonZero + zerosAtRight

	#if the displacement is all 0s then startPos is na. For this reason there are is.na checks
	if (is.na (startPos) || is.na (endPos))
		return (list (
			      curve = displacement,
			      startPos = 1,
			      endPos = length (displacement)
			      ))

	if (startPos < 1)
		startPos <- 1

	if (endPos < 1)
		endPos <- 1
	if (endPos > displacementLengthStored)
		endPos <- displacementLengthStored

	print ("reduceCurveByPredictStartEnd end")
	# 4 return the reconstructed curve
	#print (paste ("start moved to: ", startByStability + (firstInitialNonZero -1) - zerosAtLeft))
	return (list (
		      curve = c(rep(0, zerosAtLeft), displacement, rep(0, zerosAtRight)),
		      startPos = startPos,
		      endPos = endPos
		    ))
}

#########################################
# <---- reduceCurveByPredictStartEnd ####
#########################################


#for graph.R, if single file all repetitions have Roptions, but if not, each one has different values
#for capture.R pass singleFile as TRUE, curves as NULL, and i as NULL
assignRepOptions <- function(
			      singleFile, curves, i,
			      massBody, massExtra, eccon, exPercentBodyWeight, 
			      econfName, diameter, diameterExt, 
			      anglePush, angleWeight, inertiaM, gearedDown,
			      laterality) 
{
	if(singleFile) {
		return(list(
			      massBody = massBody, 
			      massExtra = massExtra, 
			      eccon = eccon, 
			      exPercentBodyWeight = exPercentBodyWeight, 
			      
			      econfName = econfName, 
			      diameter = diameter, 
			      diameterExt = diameterExt, 
			      anglePush = anglePush, 
			      angleWeight = angleWeight, 
			      inertiaM = inertiaM, 
			      gearedDown = gearedDown,
			      laterality = laterality
			    ))
	} else {
		return(list(
			      massBody = curves[i,5], 
			      massExtra = curves[i,6], 
			      eccon = curves[i,8], 
			      exPercentBodyWeight = curves[i,10], 
			      
			      econfName = curves[i,11], 
			      diameter = curves[i,12], 
			      diameterExt = curves[i,13], 
			      anglePush = curves[i,14], 
			      angleWeight = curves[i,15], 
			      inertiaM = curves[i,16], 
			      gearedDown = curves[i,17],
			      laterality = curves[i,18] 
			    ))
	}
}



#go here with every single repetition
#repOp has the options for this repetition
#repOp$eccon="c" one time each repetitions
#repOp$eccon="ec" one time each repetition
#repOp$eccon="ecS" means ecSeparated. two times each repetition: one for "e", one for "c"
#kinematicsF <- function(displacement, massBody, massExtra, exercisePercentBodyWeight,
#			encoderConfigurationName,diameter,diameterExt,anglePush,angleWeight,inertiaMomentum,gearedDown,
#			smoothingOneEC, smoothingOneC, g, eccon, isPropulsive)
#eccModesStartOnGround is default mode for calculations on e, ec starting when person lands,
#	but on single analysis instant array, disable it in order to see the full repetition
#	also think what to do on export CSV
kinematicsF <- function(displacement, repOp, smoothingOneEC, smoothingOneC, g, isPropulsive, eccModesStartOnGround, minHeight)
{
	print("at kinematicsF")

	smoothing = 0
	if(repOp$eccon == "c" || repOp$eccon == "e")
		smoothing = smoothingOneC
	else
		smoothing = smoothingOneEC
	

	print(c("repOp$eccon, smoothing:", repOp$eccon, smoothing))

	speed <- getSpeed(displacement, smoothing)
	
	accel <- getAccelerationSafe(speed)
	
	#speed comes in mm/ms when derivate to accel its mm/ms^2 to convert it to m/s^2 need to *1000 because it's quadratic
	accel$y <- accel$y * 1000 

	eccentric = 0
	concentric = 0
	propulsiveEnd = 0

	#search propulsiveEnd
	if(isPropulsive) {
		if(repOp$eccon=="c")
		{
			concentric=1:length(displacement)
			
			maxSpeedT <- min(which(speed$y == max(speed$y)))
			maxSpeedTInConcentric = maxSpeedT
			
			propulsiveEnd = findPropulsiveEnd(accel$y, concentric, maxSpeedTInConcentric,
							  repOp$econfName, repOp$anglePush, repOp$angleWeight, 
							  repOp$massBody, repOp$massExtra, repOp$exPercentBodyWeight)
			print ("propulsiveEnd");
			print (propulsiveEnd);
		}
		else if(repOp$eccon=="ec")
		{
			phases_l <- findECPhases (displacement, minHeight)
			eccentric <- phases_l$eccentric
			isometric <- phases_l$isometric
			concentric <- phases_l$concentric
	
			#temporary fix problem of found MinSpeedEnd at right

			#if(eccentric == 0 && isometric == 0 && concentric == 0)
			#condition "if (eccentric == 0)" fails when is a vector on recent R versions, so check first the length
			if(length(eccentric) == 1 && eccentric == 0 &&
					length(isometric) == 1 && isometric == 0 &&
					length(concentric) == 1 && concentric == 0)
				propulsiveEnd = length(displacement)
			else {
				maxSpeedT <- min(which(speed$y == max(speed$y)))
				maxSpeedTInConcentric = maxSpeedT - (length(eccentric) + length(isometric))

				propulsiveEnd = length(eccentric) + length(isometric) + 
						findPropulsiveEnd(accel$y, concentric, maxSpeedTInConcentric, 
								  repOp$econfName, repOp$anglePush, repOp$angleWeight, 
								  repOp$massBody, repOp$massExtra, repOp$exPercentBodyWeight)
			}
		} else if(repOp$eccon=="e") {
			#not repOp$eccon="e" because not propulsive calculations on eccentric
		} else { #ecS
			#print("WARNING ECS\n\n\n\n\n")
		}

		# concentric can be 1 sample longer than accel$y, so put it at length accel$y
		# in order to not have concenctric force ending on a NA value and its mean will be NA
		if (propulsiveEnd > length (accel$y))
			propulsiveEnd = length (accel$y)
	}

	dynamics = getDynamics(repOp$econfName,
			speed$y, accel$y, repOp$massBody, repOp$massExtra, repOp$exPercentBodyWeight, 
			repOp$gearedDown, repOp$anglePush, repOp$angleWeight,
			displacement, repOp$diameter, repOp$inertiaM, smoothing)
	mass = dynamics$mass
	force = dynamics$force
	power = dynamics$power

	print ("dynamics$mass")
	print (dynamics$mass)
	#print ("dynamics$force")
	#print (dynamics$force)
	#print ("dynamics$power")
	#print (dynamics$power)
	print ("length (dynamics$force)")
	print (length (dynamics$force))

	start <- 1
	end <- length(speed$y)

	print ("end")
	print (end)

	#on "e", "ec", start on ground
	if(eccModesStartOnGround && ! isInertial(repOp$econfName) && 
	   (repOp$eccon == "e" || repOp$eccon == "ec")) {
		#if eccentric is undefined, find it
		if(length(eccentric == 1) && eccentric == 0) {
			if(repOp$eccon == "e")
				eccentric = 1:length(displacement)
			else {  #(repOp$eccon=="ec")
				phases_l <- findECPhases (displacement, minHeight)
				eccentric <- phases_l$eccentric
			}
		}
		
		weight = mass * g
		if(length(which(force[eccentric] <= 0)) > 0) {
			landing = max(which(force[eccentric]<=0))
			start = landing
		}
	}

	if( isPropulsive && ( repOp$eccon== "c" || repOp$eccon == "ec" ) )
		end <- propulsiveEnd

	print ("end2") #here is the bug, because propulsive phase is being aplied and should not be applied on inertial !!!!!!!!
	print (end)

	#as acceleration can oscillate, start at the eccentric part where there are not negative values
	if(repOp$inertiaM > 0 && (repOp$eccon == "e" || repOp$eccon == "ec")) 
	{
		if(repOp$eccon=="e") {
			eccentric=1:length(displacement)
		}
		
		#if there is eccentric data and there are negative values
	   	if(length(eccentric) > 0 && min(accel$y[eccentric]) < 0)
		{ 
			#deactivated:
			#start = max(which(accel$y[eccentric] < 0)) +1
			#print("------------ start -----------")
			#print(start)
		}
	}

	#print(c("kinematicsF start end",start,end))
	#write("kinematicsF speed length and mean,", stderr())
	#write(length(speed$y[start:end]), stderr())
	#write(mean(speed$y[start:end]), stderr())
	write("kinematicsF displacement length, displ length", stderr())
	write(length(displacement), stderr())
	write(length(displacement[start:end]), stderr())

	speedyangular = 0
	if(isInertial(repOp$econfName))
		speedyangular = dynamics$angleSpeed[start:end]

	return(list(
		    displ = displacement[start:end], 
		    speedy = speed$y[start:end], 
		    accely = accel$y[start:end], 
		    force = force[start:end], 
		    power = power[start:end], 
		    mass = mass,
		    speedyangular = speedyangular
		    ))
}

# 2023 code using reduceCurveByPredictStartEnd
# returning values eccentric[1] and concentric[length(concentric)] can be uses to know start and end of full ec
findECPhases <- function (displacement, minHeight)
{
	eccentric <- 0
	isometric <- 0
	concentric <- 0

	position <- cumsum (displacement)
	changeEccCon <- mean (which (position == min (position)))

	ecc_l <- reduceCurveByPredictStartEnd (displacement[1:changeEccCon],
					       "e", minHeight)
	con_l <- reduceCurveByPredictStartEnd (displacement[changeEccCon:length(displacement)],
					       "c", minHeight)

	eccentric <- ecc_l$startPos:ecc_l$endPos
	concentric <- (changeEccCon + con_l$startPos -1):(changeEccCon + con_l$endPos -1)
	if (ecc_l$endPos < changeEccCon || con_l$startPos > changeEccCon)
		isometric <- ecc_l$endPos:(changeEccCon + con_l$startPos -1)

	return (list (
		eccentric = eccentric,
		isometric = isometric,
		concentric = concentric))
}

findPropulsiveEnd <- function(accel, concentric, maxSpeedTInConcentric,
			     encoderConfigurationName, anglePush, angleWeight, 
			     massBody, massExtra, exercisePercentBodyWeight) {

	propulsiveEndsAt <- -g

	if(encoderConfigurationName == "LINEARONPLANE") {
		#propulsive phase ends at: -g*sin(alfa)
		propulsiveEndsAt <- -g * sin(anglePush * pi / 180)
	} else if(encoderConfigurationName == "LINEARONPLANEWEIGHTDIFFANGLE") {
		massBodyUsed <- getMassBodyByExercise(massBody, exercisePercentBodyWeight)
		#propulsive phase ends at: g * [massBodyUsed*sin(anglePush) + massExtra*sin(angleWeight)] / (massBodyUsed + massExtra)
		propulsiveEndsAt <- -g * (massBodyUsed * sin(anglePush * pi / 180) + massExtra * sin(angleWeight * pi / 180)) / (massBodyUsed + massExtra)
	}

	if(length(which(accel[concentric] <= propulsiveEndsAt)) > 0) {
		#this:
		#	propulsiveEnd = min(which(accel[concentric] <= -g))
		#can be a problem because some people does an strange countermovement at start of concentric movement
		#this people moves arms down and legs go up
		#at this moment acceleration can be lower than -g
		#if this happens, propulsiveEnd will be very early and detected jump will be very high
		#is exactly the same problem than findTakeOff, see that method for further help
		#another option can be using extrema

		accelCon = accel[concentric]
		df=data.frame(accelCon <= propulsiveEndsAt, accelCon, abs(1:length(accelCon)-maxSpeedTInConcentric))
		colnames(df)=c("belowG","accel","dist")
		df2 = subset(df,subset=df$belowG)
	
		df2row = min(which(df2$dist == min(df2$dist)))
		propulsiveEnd = as.integer(rownames(df2)[df2row])
	}
	else
		propulsiveEnd = length(concentric)

	return (propulsiveEnd)
}

pafGenerate <- function(eccon, kinematics, massBody, massExtra, laterality, inertiaMomentum, diameter, gearedDown)
{
	meanSpeed <- mean(kinematics$speedy)

	#max speed and max speed time can be at eccentric or concentric
	maxSpeed <- max(abs(kinematics$speedy))
	maxSpeedT <- min(which(abs(kinematics$speedy) == maxSpeed))

	meanPower <- mean(kinematics$power)
	
	if(eccon != "c") {
		meanSpeed <- mean(abs(kinematics$speedy))
		meanPower <- mean(abs(kinematics$power))
	}

	peakPower <- max(abs(kinematics$power))
	peakPowerT <- min(which(abs(kinematics$power) == peakPower))
	pp_ppt <- peakPower / (peakPowerT/1000)	# ms->s

	meanForce <- mean(kinematics$force)
	maxForce <- max(abs(kinematics$force))
	maxForceT <- min(which(abs(kinematics$force) == maxForce))
	maxForce_maxForceT <- maxForce / (maxForceT/1000)	# ms->s

	#work calculation
	#calculating with mean force has the problem that the displacement is not always the same
	#so is not correct and has to be (at each ms): force * displ
	#workJ <- meanForce * max(abs(kinematics$displ)) 	#F * displ # TODO: maybe calculate by phases with a distance always positive

	#work by difference of energy (Joules)
	#W = m[g(hf - hi) + 1/2 (vf^2 - Vi^2)] + 1/2 I(wf^2 - wi^2)
	#                                      <--  on inertial -->
	#hi, hf: height initial/final
	#vi, vf: speed initial/final
	#wi, wf: angular speed initial/final
	lastValue = length(kinematics$displ)
	workJ <- kinematics$mass * ( g * (
					  abs(cumsum(kinematics$displ))[length(cumsum(kinematics$displ))] / 1000 #position displaced in m
					  ) + .5*((kinematics$speedy[lastValue])^2 - (kinematics$speedy[1])^2) )
	if(inertiaMomentum > 0) #if is not inertial Roptions has a -1
		workJ = workJ + .5 * inertiaMomentum * abs( (kinematics$speedyangular[lastValue])^2 - (kinematics$speedyangular[1]^2) )

	#print ("workJ decomposed:")
	#print (workJ)
	#print (kinematics$mass)
	#print (g)
	#print (abs(cumsum(kinematics$displ))[length(cumsum(kinematics$displ))] / 1000 ) #position displaced in m
	#print ((kinematics$speedy[lastValue])^2)
	#print ((kinematics$speedy[1])^2)

	#impulse
	impulse <- meanForce * length(kinematics$displ)/1000 	#F * time in s

	print("meanForce: ")
	print(meanForce)
#	print ("kinematics$force:")
#	print (kinematics$force)
	print("displ: ")
	print(length(kinematics$displ)/1000)

	#here paf is generated
	#mass is not used by pafGenerate, but used by Kg/W (loadVSPower)
	#meanForce and maxForce are not used by pafGenerate, but used by F/S (forceVSSpeed)
	mass = kinematics$mass
	#names will be used on graph.R writeCurves

	equivalentMass = calculateEquivalentMass(inertiaMomentum, gearedDown, diameter)

	return(data.frame(
			  meanSpeed, maxSpeed, maxSpeedT,
			  meanPower, peakPower, peakPowerT, pp_ppt,
			  meanForce, maxForce, maxForceT, maxForce_maxForceT,
			  mass, massBody, massExtra,		#kinematics$mass is Load
			  workJ, impulse,
			  laterality, inertiaMomentum, diameter,
			  equivalentMass
			  ))
}

calculateEquivalentMass <- function(inertiaMomentum, gearedDown, diameter)
{
	if(inertiaMomentum > 0 && gearedDown > 0 && diameter > 0)
		return (10000 * inertiaMomentum * (1/gearedDown) / (( diameter / 2 )^2) )
	else
		return (0)
}

isInertial <- function(encoderConfigurationName) {
	if(encoderConfigurationName == "LINEARINERTIAL" ||
	   encoderConfigurationName == "ROTARYFRICTIONSIDEINERTIAL" ||
	   encoderConfigurationName == "ROTARYFRICTIONAXISINERTIAL" ||
	   encoderConfigurationName == "ROTARYFRICTIONSIDEINERTIALLATERAL" || 
	   encoderConfigurationName == "ROTARYFRICTIONAXISINERTIALLATERAL" ||
	   encoderConfigurationName == "ROTARYFRICTIONSIDEINERTIALMOVPULLEY" || 
	   encoderConfigurationName == "ROTARYFRICTIONAXISINERTIALMOVPULLEY" ||
	   encoderConfigurationName == "ROTARYAXISINERTIAL" ||
	   encoderConfigurationName == "ROTARYAXISINERTIALLATERAL" || 
	   encoderConfigurationName == "ROTARYAXISINERTIALMOVPULLEY" ||
	   encoderConfigurationName == "ROTARYAXISINERTIALLATERALMOVPULLEY"
	   )
		return(TRUE)
	else
		return(FALSE)
}

getMassBodyByExercise <- function(massBody, exercisePercentBodyWeight) {
  return (massBody * exercisePercentBodyWeight / 100.0)
}

getDynamics <- function(encoderConfigurationName,
			speed, accel, massBody, massExtra, exercisePercentBodyWeight, gearedDown, anglePush, angleWeight,
			displacement, diameter, inertiaMomentum, smoothing)
{
	#debugParameters(listN(encoderConfigurationName,
	#		     speed, accel, massBody, massExtra, exercisePercentBodyWeight, gearedDown, anglePush, angleWeight,
	#		     displacement, diameter, inertiaMomentum, smoothing), "getDynamics")
	

	massBody = getMassBodyByExercise(massBody,exercisePercentBodyWeight)

	if(
	   encoderConfigurationName == "WEIGHTEDMOVPULLEYLINEARONPERSON1" ||
	   encoderConfigurationName == "WEIGHTEDMOVPULLEYLINEARONPERSON1INV" ||
	   encoderConfigurationName == "WEIGHTEDMOVPULLEYLINEARONPERSON2" ||
	   encoderConfigurationName == "WEIGHTEDMOVPULLEYLINEARONPERSON2INV" ||
	   encoderConfigurationName == "WEIGHTEDMOVPULLEYROTARYFRICTION" ||
	   encoderConfigurationName == "WEIGHTEDMOVPULLEYROTARYAXIS")
	{
		massExtra = getMass(massExtra, gearedDown, anglePush)
	} 

	massTotal = massBody + massExtra

	if(isInertial(encoderConfigurationName))
		return (getDynamicsInertial(encoderConfigurationName, displacement, diameter, massTotal, inertiaMomentum, gearedDown, smoothing))
	else 
		return (getDynamicsNotInertial (encoderConfigurationName, speed, accel, 
						massBody, massExtra, massTotal, 
						exercisePercentBodyWeight, gearedDown, anglePush, angleWeight))
}

#mass extra can be connected to body or connected to a pulley depending on encoderConfiguration
getDynamicsNotInertial <- function(encoderConfigurationName, speed, accel, 
                                   massBody, massExtra, massTotal,
                                   exercisePercentBodyWeight, gearedDown, anglePush, angleWeight) 
{ 
        force = NULL
        if(encoderConfigurationName == "LINEARONPLANEWEIGHTDIFFANGLE") {
                force <- massBody*(accel + g*sin(anglePush * pi / 180)) + massExtra*(accel + g*sin(angleWeight * pi / 180))
        } else if(encoderConfigurationName == "LINEARONPLANEWEIGHTDIFFANGLEMOVPULLEY") {
                force <- massBody*(accel + g*sin(anglePush * pi / 180)) + massExtra*(g*sin(angleWeight * pi / 180) + accel) / gearedDown
        } else if(encoderConfigurationName == "LINEARONPLANE"){
                force <- (massBody + massExtra)*(accel + g*sin(anglePush * pi / 180))
        } else if(encoderConfigurationName == "PNEUMATIC"){
                force  = massExtra * g + massBody * (accel + g*sin(anglePush * pi / 180))         #The force generated by the machine is supposed to be constant
        } else {
                force <- massTotal*(accel+g)	#g:9.81 (used when movement is against gravity)
        }
        power <- force*speed
        return(list(mass=massTotal, force=force, power=power))
}

#diameter: diameter of the axis (where string is wrapped)
#angle: angle (rotation of disc) in radians
#angleSpeed: speed of angle
#angleAccel: acceleration of angle
#encoderConfiguration:
#  LINEARINERTIAL Linear encoder on inertial machine (rolled on axis)
#  ROTARYFRICTIONSIDEINERTIAL Rotary friction encoder connected to inertial machine on the side of the disc
#  ROTARYFRICTIONAXISINERTIAL Rotary friction encoder connected to inertial machine on the axis
#  ROTARYAXISINERTIAL Rotary axis encoder  connected to inertial machine on the axis
#  ROTARYAXISINERTIALMOVPULLEY Rotari axis encoder connected to inertial machine on the axis and the subject pulling from a moving pulley
#  ROTARYAXISINERTIALLATERALMOVPULLEY Rotari axis encoder connected to inertial machine on the axis and the subject pulling from a moving pulley
#       and performing and horizontal movement

getDynamicsInertial <- function(encoderConfigurationName, displacement, diameter, mass, inertiaMomentum, gearedDown, smoothing)
{
	speed = getSpeed(displacement, smoothing) #mm/ms == m/s 

	# accel will be:
	accel = getAcceleration(speed) 

	accel$y = accel$y * 1000 # mm/ms² -> m/s²
	
	#use the values
	speed = speed$y
	accel = accel$y
	
	#----------------------
	#let's work with SI now
	#----------------------

	position.m = abs(cumsum(displacement)) / 1000 #m
	diameter.m = diameter / 100 #cm -> m
    
	angleSpeed = 0
	forceDisc = 0
	forceBody = 0
	powerDisc = 0
	powerBody = 0
  
  if(encoderConfigurationName == "ROTARYAXISINERTIAL" ||
       encoderConfigurationName == "ROTARYFRICTIONSIDEINERTIAL" ||
       encoderConfigurationName == "ROTARYFRICTIONAXISINERTIAL" ||
       encoderConfigurationName == "LINEARINERTIAL"){
    angle = position.m * 2 / diameter.m
    angleSpeed = speed * 2 / diameter.m
    angleAccel = accel * 2 / diameter.m
    anglePush = 90 #TODO: send from C#

    forceDisc = abs(inertiaMomentum * angleAccel) * (2 / diameter.m) 
    forceBody = mass * (abs(accel) + g * sin(anglePush * pi / 180))
    powerDisc = abs((inertiaMomentum * angleAccel) * angleSpeed)
    powerBody = abs(mass * (accel + g * sin(anglePush * pi / 180)) * speed)

  } else if(encoderConfigurationName == "ROTARYAXISINERTIALMOVPULLEY" ||
              encoderConfigurationName == "ROTARYFRICTIONAXISINERTIALMOVPULLEY" ||
              encoderConfigurationName == "ROTARYFRICTIONSIDEINERTIALMOVPULLEY"){
    #With a moving pulley, the displacement of the body is half of the displacement of the rope in the inertial machine side
    #So, the multiplier is 4 instead of 2 to get the rotational kinematics.
    angle = position.m * 2 / gearedDown / diameter.m
    angleSpeed = speed * 2 / gearedDown / diameter.m
    angleAccel = accel * 2 / gearedDown / diameter.m
    anglePush = 90 #TODO: send from C#
    #The configuration covers horizontal, vertical and inclined movements
    #If the movement is vertical g*sin(alpha) = g
    #If the movement is horizontal g*sin(alpha) = 0

    forceDisc = abs(inertiaMomentum * angleAccel) * (2 / diameter.m)
    forceBody = mass * (abs(accel) + g * sin(anglePush * pi / 180))
    powerDisc = abs((inertiaMomentum * angleAccel) * angleSpeed)
    powerBody = abs(mass * (accel + g * sin(anglePush * pi / 180)) * speed)


  } else if(encoderConfigurationName == "ROTARYAXISINERTIALLATERAL" ||
              encoderConfigurationName == "ROTARYFRICTIONAXISINERTIALLATERAL" ||
              encoderConfigurationName == "ROTARYFRICTIONSIDEINERTIALLATERAL" ||
              encoderConfigurationName == "ROTARYAXISINERTIALLATERALMOVPULLEY"){
    angle = position.m * 2 / gearedDown / diameter.m
    angleSpeed = speed * 2 / gearedDown / diameter.m
    angleAccel = accel * 2 / gearedDown / diameter.m
    anglePush = 0 #TODO: send from C#
    
    forceDisc = abs(inertiaMomentum * angleAccel) * (2 / diameter.m)
    forceBody = mass * abs(accel)
    powerDisc = abs((inertiaMomentum * angleAccel) * angleSpeed)
    powerBody = abs(mass * accel * speed)

  }

  force = (forceDisc / gearedDown) + forceBody
  power = powerDisc + powerBody

  loopsMax = diameter.m * max(angle)
  loopsAblines = seq(from=0, to=loopsMax, by=diameter.m*pi)

	return(list(displacement=displacement, mass=mass, force=force, power=power, 
		    loopsAblines=loopsAblines, angleSpeed=angleSpeed,
		    forceDisc=forceDisc, forceBody=forceBody, powerDisc=powerDisc, powerBody=powerBody))
}



#in signals and curves, need to do conversions (invert, diameter)
getDisplacement <- function(capturing, encoderConfigurationName, displacement, diameter, diameterExt, gearedDown) {
	#no change
	#WEIGHTEDMOVPULLEYLINEARONPERSON1, WEIGHTEDMOVPULLEYLINEARONPERSON1INV,
	#WEIGHTEDMOVPULLEYLINEARONPERSON2, WEIGHTEDMOVPULLEYLINEARONPERSON2INV,
	#LINEARONPLANE
	#ROTARYFRICTIONSIDE
	#WEIGHTEDMOVPULLEYROTARYFRICTION
  #ROTARYAXISINERTIALMOVPULLEY

  if( ! capturing && (
	   encoderConfigurationName == "LINEARINVERTED" ||
	   encoderConfigurationName == "WEIGHTEDMOVPULLEYLINEARONPERSON1INV" ||
	   encoderConfigurationName == "WEIGHTEDMOVPULLEYLINEARONPERSON2INV") )
	  #On inverted modes the direction of the displacement is changed
  {
	  displacement = -displacement
  } else if(encoderConfigurationName == "WEIGHTEDMOVPULLEYONLINEARENCODER") 
  {
	  #On geared down machines the displacement of the subject is multiplied by gearedDown
	  #default is: gearedDown = 2. Future maybe this will be a parameter
	  displacement = displacement * 2
  } else if(encoderConfigurationName == "LINEARONPLANEWEIGHTDIFFANGLEMOVPULLEY")
  {
    displacement = displacement * gearedDown
  } else if(encoderConfigurationName == "ROTARYFRICTIONAXIS") 
  {
	  #On rotary friction axis the displacement of the subject is proportional to the axis diameter
	  #and inversely proportional to the diameter where the encoder is coupled
	  displacement = displacement * diameter / diameterExt
  } else if(encoderConfigurationName == "ROTARYAXIS" || 
	    encoderConfigurationName == "WEIGHTEDMOVPULLEYROTARYAXIS") 
  {
	  #On rotary encoders attached to fixed pulleys next to subjects (see config 1 and 3 in interface),
	  #the displacement of the subject is anlge * radius
	  ticksRotaryEncoder = 200 #our rotary axis encoder sends 200 ticks per revolution
	  #The angle rotated by the pulley is (ticks / ticksRotaryEncoder) * 2 * pi
	  #The radium in mm is diameter * 1000 / 2
	  displacement = ( displacement / ticksRotaryEncoder ) * pi * ( diameter * 1000 )
  }
		
	return(displacement)
}

#This function converts angular information from rotary encoder to linear information like linear encoder
#This is NOT the displacement of the person because con-ec phases roll in the same direction
#This is solved by the function getDisplacementInertialBody
getDisplacementInertial <- function(displacement, encoderConfigurationName, diameter, diameterExt, gearedDown)
{
	write("at getDisplacementInertial", stderr())
	
	#scanned displacement is ticks of rotary axis encoder
	#now convert it to mm of body displacement
	if(encoderConfigurationName == "ROTARYAXISINERTIAL" ||
	     encoderConfigurationName == "ROTARYAXISINERTIALLATERAL" ||
	     encoderConfigurationName == "ROTARYAXISINERTIALMOVPULLEY" ||
	     encoderConfigurationName == "ROTARYAXISINERTIALLATERALMOVPULLEY") {
	        ticksRotaryEncoder = 200 #our rotary axis encoder send 200 ticks per revolution
	        
	        #Number of revolutions that the flywheel rotates every millisecond
	        revolutionsPerMs = displacement / ticksRotaryEncoder # One revolution every ticksRotaryEncoder ticks
	        
	        #The person is gearedDown from the machine point of view 
	        #If force multiplier is 2 (gearedDown = 0.5) the displacement of the body is 
	        #half the the displacement at the perimeter of the axis
	        displacement = revolutionsPerMs * pi * diameter * 10 * gearedDown # Revolutions * perimeter * gearedDown  and converted cm -> mm
	        
	} else if(encoderConfigurationName == "ROTARYFRICTIONSIDEINERTIAL" ||
                encoderConfigurationName == "ROTARYFRICTIONSIDEINERTIALLATERAL" ||
                encoderConfigurationName == "ROTARYFRICTIONSIDEINERTIALMOVPULLEY"){
	        displacement = displacement * diameter * gearedDown / diameterExt #displacement of the axis
	        
	} else if(encoderConfigurationName == "ROTARYFRICTIONAXISINERTIALMOVPULLEY"){
	        #If force multiplier is 2 (gearedDown = 0.5) the displacement of the body is 
	        #half the the displacement at the perimeter of the axis
	        displacement = displacement * gearedDown
	  
	}
	
	return (displacement)
}


#Converts from mm of displacement on the encoder
#to mm of displacement of the person	

#separate phases using initial height of full extended person
#here comes a signal: (singleFile)
#it can show graph of the disc rotation and the person movement
	
#positionStart is the height at the start of the curve. It's used only on realtime capture.
#displacementPerson has to be adjusted for every repetition using the positionStart relative to the start of the movement
#Eg, at start of the capture position is always 0, then goes down (first eccentric phase), and then starts con-ecc, con-ecc, con-ecc, ...
#To divide the con-ecc in two phases (because for the encoder is only one phase because it rotates in the same direction), we need to know the positionAtStart
getDisplacementInertialBody <- function(positionStart, displacement, draw, title) 
{
	position=cumsum(displacement)
	position.ext=extrema(position)

	#print("at findCurvesInertial")

	#do if extrema(position)$nextreme == 0... then do not use extrema
	#TODO: check if started backwards on realtime capture (extrema is null)
	#firstDownPhaseTime = 1
	#downHeight = 0
	if( position.ext$nextreme > 0 && ! is.null(position.ext$minindex) && ! is.null(position.ext$maxindex) ) {
		#Fix if disc goes wrong direction at start
		#we know this because we found a maxindex before than a minindex, and
		#this maxindex is not an small movement, at least is 20% of the movement
		if(
		   ( position.ext$maxindex[1] < position.ext$minindex[1] ) &&
		   ( position[position.ext$maxindex[1]] >= ( max(abs(position)) *.2 ) ) ) {
			displacement = displacement * -1
			position=cumsum(displacement)
			#position.ext=extrema(position)
		}

		#unused
		#firstDownPhaseTime = position.ext$minindex[1]
		#downHeight = abs(position[1] - position[firstDownPhaseTime])
	}
	
	#This is the main part.
	#Converts from mm of displacement on the encoder
	#to mm of displacement of the person	
	positionPerson = cumsum(displacement)
	positionPerson = positionPerson + positionStart
	positionPerson = abs(positionPerson)*-1

	#this is to make "inverted cumsum"
	#this displacement when 'cumsum' into position is the reality,
	#but if we use it it will seem a VERY high acceleration in the beginning (between the fist and second value)
	#because it will be: eg:  -1100, 0, 0, 0, 0, 1, ...
	#don't use it
	#displacementPerson = c(positionStart,diff(positionPerson))
	#better have it starting with 0 and then speed calculations... will be correct
	#displacementPerson = c(positionPerson[1],diff(positionPerson))
	#displacementPerson = diff(positionPerson)
	#displacementPerson = c(displacementPerson[1],displacementPerson) #this is to recuperate the lost 1st value in the diff operation

	#Important: In this case, as written aboce, the first 0 should be a 0, don't touch it!
	displacementPerson = c(0,diff(positionPerson))

	#write(displacementPerson,stderr())
	
	if(draw) {
		col="black"
		plot((1:length(position))/1000			#ms -> s
		     ,position/10,				#mm -> cm
		     type="l",
		     xlim=c(1,length(position))/1000,		#ms -> s
		     ylim=c(min(positionPerson)/10,max(position/10)),
		     xlab="",ylab="",axes=T,
		     lty=1,col=col)

		abline(h=0, lty=2, col="gray")
	
		lines((1:length(position))/1000,positionPerson/10,lty=1,lwd=2)

		title(title, cex.main=1, font.main=1)
		
		#disabled because translateToPrint is on graph.R (not called by capture.R)
		#mtext(paste(translateToPrint("time"),"(s)"),side=1,adj=1,line=-1)
		#mtext(paste(translateToPrint("displacement"),"(cm)"),side=2,adj=1,line=-1)
		mtext("time (s)", side=1, adj=1, line=-1)
		mtext("displacement (cm)", side=2, adj=1, line=-1)
	}
	return(displacementPerson)
}

#DISABLED just after 1.7.0
#used when user captures without string fully extended
#signal is the information coming from the encoder, graph is to debug
#see codeExplained/image detect-and-fix-inertial-string-not-fully-extended.png
#fixInertialSignalIfNotFullyExtended <- function(signal, checkRevolutions, saveFile, specialDataFile, graph)
#{
#	write("at fixInertialSignalIfNotFullyExtended", stderr())
#	angle <- cumsum(signal) #360 degrees every 200 ticks
#
#	maximums <- extrema(angle)$maxindex[,1]
#	minimums <- extrema(angle)$minindex[,1]
#	maximumsCopy <- maximums #store this value
#	minimumsCopy <- minimums #store this value
#
#	#if we have more than 2 max & mins, remove the first and last value
#	if(length(maximums) > 2 & length(minimums) > 2)
#	{
#		#if there's any max extrema value negative (remove it), same for positive min values
#		maximums.temp = NULL
#		minimums.temp = NULL
#		for( i in maximums )
#			if(angle[i] > 0)
#				maximums.temp <- c(maximums.temp, i)
#		for( i in minimums )
#			if(angle[i] < 0)
#				minimums.temp <- c(minimums.temp, i)
#
#		maximums <- maximums.temp
#		minimums <- minimums.temp
#
#		if(length(maximums) < 1 | length(minimums) < 1)
#			return(signal)
#
#
#		#remove the first value of the maximums OR minimums (just the first one of both)
#		if(maximums[1] < minimums[1])
#			maximums <- maximums[-1]
#		else
#			minimums <- minimums[-1]
#
#		if(length(maximums) < 1 | length(minimums) < 1)
#			return(signal)
#
#
#		#remove the last value of the maximums OR minimums (just the last one of both)
#		if(maximums[length(maximums)] > minimums[length(minimums)])
#			maximums <- maximums[-length(maximums)]
#		else
#			minimums <- minimums[-length(minimums)]
#
#	}
#
#	#return if no data
#	if(length(maximums) < 1 | length(minimums) < 1)
#		return(signal)
#
#	#ensure both maximums and minimums have same length
#	while(length(maximums) != length(minimums))
#	{
#		if(length(maximums) > length(minimums))
#			maximums <- maximums[-length(maximums)]
#		else if(length(maximums) < length(minimums))
#			minimums <- minimums[-length(minimums)]
#	}
#
#	meanByExtrema <- mean(c(angle[maximums], angle[minimums]))
#	angleCorrected <- angle - meanByExtrema
#
#	#remove the initial part of the signal. Remove from ms 1 to when angleCorrected crosses 0
#	angleCorrectedCrossZero = extrema(angleCorrected)$cross[1,1]
#
#	if(graph) {
#		par(mfrow=c(1,2))
#
#		#1st graph (left)
#		plot(angle, type="l", lty=2, xlab="time", ylab="Angle", main="String NOT fully extended",
#		     ylim=c(min(c(angle[minimums], -angle[maximums])) - abs(meanByExtrema), max(c(angle[maximums], -angle[minimums])) + abs(meanByExtrema)))
#		lines(abs(angle)*-1, lwd=2)
#		points(maximumsCopy, angle[maximumsCopy], col="black", cex=1)
#		points(minimumsCopy, angle[minimumsCopy], col="black", cex=1)
#		points(maximums, angle[maximums], col="green", cex=3)
#		points(minimums, angle[minimums], col="green", cex=3)
#		abline(h = meanByExtrema, col="red")
#		text(x = 0, y = meanByExtrema, labels = round(meanByExtrema,2), adj=0)
#
#		#2nd graph (right)
#		plot(angleCorrected, type="l", lty=2, xlab="time", ylab="angle", main="Corrected set",
#		     ylim=c(min(c(angle[minimums], -angle[maximums])) - 2*abs(meanByExtrema), max(c(angle[maximums], -angle[minimums]))))
#		lines(abs(angleCorrected)*-1, lwd=2)
#		abline(v=c(angleCorrectedCrossZero, length(angleCorrected)), col="green")
#		mtext("Start", at=angleCorrectedCrossZero, side=3, col="green")
#		mtext("EnfixInertialSignalIfNotFullyExtendedd", at=length(angleCorrected), side=3, col="green")
#
#		par(mfrow=c(1,1))
#	}
#
#	#define new signal only if the error in extended string is more than 4 revolutions
#	#(this value can be changed from gui)
#	if( abs(meanByExtrema) > (checkRevolutions * 200) ) {
#		#write(signal, file="/tmp/old.txt", ncolumns=length(signal), sep=", ")
#		signal <- signal[angleCorrectedCrossZero:length(signal)]
#
#		write("SIGNAL CORRECTED", specialDataFile)
#
#		#write to file and return displacement to be used
#		write(signal, file=saveFile, ncolumns=length(signal), sep=", ")
#	}
#
#	return(signal)
#}



#Read a double vector indicating the initial diameter of every loop of the rope
#plus the final diameter of the last loop and returns a dataframe with the radius
#correspending to the total number of ticks of the encoder
#This can be run only once per machine

# Example of input of the sequence of the loop and diameter of the loop
# We use diameters but in the next step we convert to radii
# d_vector <- c(1.5, 1.5, 1.5, 1.5, 2, 2.5, 2.7, 2.9, 2.95, 3)
getInertialDiametersPerTick <- function(d_vector)
{
  #If only one diameter is returned, we assume that the diameter is constant
  #and only a double is returned
  if (length(d_vector) == 1){
    return(d_vector)
  }
  
  # Numerating the loops of the rope
  d <- matrix(c(seq(from=0, to=(length(d_vector) -1), by=1), d_vector), ncol=2)
  
  # Converting the number of the loop to ticks of the encoder
  d[,1] <- d[,1]*200
  
  # Adding an extra point at the begining of the diameters matrix to match better the first point
  x1 <- d[1,1]
  y1 <- d[1,2]
  x2 <- d[2,1]
  y2 <- d[2,2]
  lambda <- 200
  x0 <- x1 - lambda
  y0 <- y1 - lambda*(y2 - y1)/(x2 - x1)
  p0 <- matrix(c(x0, y0), ncol=2)
  d <- rbind(p0, d)
  
  # Adding an extra point at the end of the diameters matrix to match better the last point
  last <- length(d[,1])
  x1 <- d[(last - 1),1]
  y1 <- d[(last - 1),2]
  x2 <- d[last,1]
  y2 <- d[last,2]
  lambda <- 200
  xFinal <- x2 + lambda
  yFinal <- y2 + lambda*(y2 - y1)/(x2 - x1)
  pFinal <- matrix(c(xFinal, yFinal), ncol=2)
  d <- rbind(d, pFinal)
  
  
  # Linear interpolation of the radius across the lenght of the measurement of the diameters
  #d.approx <- approx(x=d[,1], y=d[,2], seq(from=1, to=d[length(d[,1]),1]))
  d.smoothed <- smooth.spline(d, spar=0.4)
  d.approx <- predict(d.smoothed, 0:d[length(d[,1]), 1],0)
  return(d.approx$y)
}
#Returns the instant diameter every milisecond, depending on the displacement of the movement
getInertialDiametersPerMs <- function(displacement, diametersPerTick)
{
  if (length(diametersPerTick) == 1) {
    return(diametersPerTick)
  }
  
  diameter <- diametersPerTick[abs(cumsum(displacement)) + 1]
  return(diameter)
}

#----------- Begin spar with crossvalidation (Aleix Ruiz de Villa) -------------

cvParCall <- function(x, y) {
	parRange <- seq(0.1, 1, 0.05)
	cvResults <- cvPar(x, y, parRange)

	## Agafem la spar que te un error més petit
	#print( paste('The best spar is: ', parRange[ which.min(cvResults) ] ) )
	write("CrossValidation:", stderr())
	write(parRange[which.min(cvResults)], stderr())
	return (parRange[which.min(cvResults)])
}
cvPar <- function(x, y, parRange, cvProp = 0.8) {

	n <- length(x)
	nTrain <- floor( cvProp * n )
	nTest <- n - nTrain

	cvParError <- vector(length=length(parRange))
	for( parPos in 1:length(parRange) ){
		selectedPar <- parRange[ parPos ]

		## Separem la mostra en train i test
		indexTrain <- sample(1:n, nTrain)
		indexTest <- (1:n)[-indexTrain]
		xTrain <- x[ indexTrain ]
		xTest <- x[ indexTest ]
		yTrain <- y[ indexTrain ]
		yTest <- y[ indexTest ]

		## Fem servir les dades del train per a construir els splines
		splineModel <- smooth.spline(xTrain, yTrain, spar = selectedPar)

		## Fem la prediccio sobre la mostra test
		predictedBySpline <- predict(splineModel, xTest)
		# plot(xTest, predictedBySpline$y, col = 'blue')
		# lines(xTest, yTest, col = 'green', type = 'p')

		## Calculem els errors
		predictionError <- sqrt( mean( (predictedBySpline$y - yTest)^2 ) )
		cvParError[ parPos ] <- predictionError
	}

	return(cvParError)
}

#----------- end spar with crossvalidation -------------

last <- function (vect)
{
	return (vect[length (vect)])
}

printLHT <- function (vect, name)
{
	print (paste(name, "length, head & tail:"))
	print (length(vect))
	print (head(vect))
	print (tail(vect))
}

#----------- Begin debug file output -------------
#http://stackoverflow.com/a/34996874
write_list <- function (outlist, outfile,append=FALSE) {
	for (i in 1:length(outlist)) {
		if (i==1) app=append else app=TRUE
		if (is.character(outlist[[i]]) || is.numeric(outlist[[i]])) write(paste(outlist[[i]],collapse = " "),outfile, append=app)
		else
			if (is.data.frame(outlist[[i]])) write.table(outlist[[i]],outfile, append=app, quote=FALSE, sep="\t")
			else
				if (is.POSIXlt(outlist[[i]])) write (as.character(outlist[[i]]),outfile, append=app)
				else
					if  (is.list(outlist[[i]])) write_list(outlist = outlist[[i]], outfile, append = TRUE)

	}
}

debugParameters <- function (parameterList, currentFunction) 
{
	if(is.null(DEBUG) || DEBUG == FALSE || is.null(DebugFileName) || DebugFileName == "")
		return()

        write(paste("\n\n[Parameters of the", currentFunction, "function are]:\n"), DebugFileName, append=TRUE)
        
        #based on http://stackoverflow.com/a/34996874
        for (i in 1:length(parameterList)) {
                if ( (is.character(parameterList[[i]]) || is.numeric(parameterList[[i]])) &&
                    ! is.data.frame(parameterList[[i]]) && ! is.matrix(parameterList[[i]]) ) {
			writedebugParameters(parameterList, i)
                        write(paste(parameterList[[i]],collapse = " "), DebugFileName, append=TRUE)
                }
                if (is.data.frame(parameterList[[i]])) {
			writedebugParameters(parameterList, i)
                        write.table(parameterList[[i]], DebugFileName, append=TRUE, quote=FALSE, sep="\t", col.names = TRUE)
                }
                else if (is.POSIXlt(parameterList[[i]])) {
			writedebugParameters(parameterList, i)
                        write(as.character(parameterList[[i]]), DebugFileName, append=TRUE)
                }
                else if (is.logical(parameterList[[i]])) {
			writedebugParameters(parameterList, i)
                        write(as.character(parameterList[[i]]), DebugFileName, append=TRUE)
                }
                else if  (is.list(parameterList[[i]])) {
			writedebugParameters(parameterList, i)
                        write_list(parameterList[[i]], DebugFileName, append = TRUE)
                }
                else if (is.matrix(parameterList[[i]])) {
			writedebugParameters(parameterList, i)
                        write.table(parameterList[[i]], DebugFileName, append=TRUE, quote=FALSE, col.names = TRUE)
                }
        }
}

writedebugParameters <- function(parameterList, i) {
	write(paste("---------", names(parameterList)[i], ":", "---------", sep = ""), DebugFileName, append=TRUE) 
}

is.POSIXlt <- function (ts) {
        isPOS=c("POSIXlt", "POSIXt")
        identical (class(ts),isPOS)
}

test_debugParameters <- function() {
        numVariable = c(1,2,3,4)
        charVariable = c("Hello", "how", "are", "you")
        dataframeVariable = as.data.frame(matrix(c(10,20,40,50), ncol = 2))
        matrixVariable = matrix(c(100,200,400,500), ncol = 2)
	booleanVariable = TRUE

	l = list(numVariable = numVariable,
		 charVariable = charVariable,
		 dataframeVariable = dataframeVariable,
		 matrixVariable = matrixVariable,
		 booleanVariable = booleanVariable)
        
        debugParameters(l, "test_debugParameters")
}

#create a list assigning names
#http://stackoverflow.com/a/21059868
listN <- function(...){
	anonList <- list(...)
	names(anonList) <- as.character(substitute(list(...)))[-1]
	anonList
}

#----------- End debug file output -------------

