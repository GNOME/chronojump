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
#   Copyright (C) 2014-2015  	Xavier de Blas <xaviblas@gmail.com> 
#   Copyright (C) 2014-2015   	Xavier Padullés <x.padulles@gmail.com>
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
		    #
		    #in Analysis "powerBars", AnalysisVariables can be:
		    #	"TimeToPeakPower;Range", or eg: "NoTimeToPeakPower;NoRange"
		    #
		    #in Analysis "single" or "side", AnalysisVariables can be:
		    #	"Speed;Accel;Force;Power", or eg: "NoSpeed;NoAccel;Force;Power"
		    #
		    #in Analysis = "1RMAnyExercise"
		    #AnalysisVariables = "0.185;method". speed1RM = 0.185m/s
		    Analysis		= options[11],	
		    AnalysisVariables	= unlist(strsplit(options[12], "\\;")),
		    AnalysisOptions	= options[13],
		    EncoderConfigurationName =	options[14],	#just the name of the EncoderConfiguration	
		    diameter		= as.numeric(unlist(strsplit(options[15], "\\;"))), #comes in cm, will be converted to m. Since 1.5.1 can be different diameters separated by ;
		    #diameter    = getInertialDiametersPerTick(as.numeric(unlist(strsplit("1.5; 1.75; 2.65; 3.32; 3.95; 4.07; 4.28; 4.46; 4.54; 4.77; 4.96; 5.13; 5.3; 5.55", "\\;")))),
		    diameterExt		= as.numeric(options[16]),	#comes in cm, will be converted to m
		    anglePush 		= as.numeric(options[17]),
		    angleWeight 	= as.numeric(options[18]),
		    inertiaMomentum	= (as.numeric(options[19])/10000.0),	#comes in Kg*cm^2 eg: 100; convert it to Kg*m^2 eg: 0.010
		    gearedDown 		= readFromFile.gearedDown(as.numeric(options[20])),

		    SmoothingOneC	= as.numeric(options[21]),
		    Jump		= options[22],
		    Width		= as.numeric(options[23]),
		    Height		= as.numeric(options[24]),
		    DecimalSeparator	= options[25],
		    Title		= options[26],
		    OperatingSystem	= options[27]#,	#if this changes, change it also at start of this R file
		    #IMPORTANT, if this grows, change the readLines value on getOptionsFromFile
		    ))
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

	#print("df2")
	#print(df2)

	#3 find takeoff as the df2 row with less distance to maxSpeedT
	df2row = min(which(df2$dist == min(df2$dist)))
	takeoff = as.integer(rownames(df2)[df2row])

	#print(c("takeoff",takeoff))

	return(takeoff)
}
	

getSpeedSafe <- function(displacement, smoothing) {
	#x vector should contain at least 4 different values
	if(length(displacement) >= 4)
		return(getSpeed(displacement, smoothing))
	else
		return(list(y=rep(0,length(displacement))))
}
#unused since 1.6.0 because first and last displacement values make change too much the initial and end curve
#getSpeedOld <- function(displacement, smoothing) {
#	#no change affected by encoderConfiguration
#	return (smooth.spline( 1:length(displacement), displacement, spar=smoothing))
#}
getSpeed <- function(displacement, smoothing) {
	#no change affected by encoderConfiguration
	
	#use position because this does not make erronously change the initial and end of the curve
	#do spline with displacement is buggy because data is too similar -2, -1, 0, 1, 2, ...
	position <- cumsum(displacement)
	positionSpline <- smooth.spline( 1:length(position), position, spar=smoothing)
	
	#get speed converting from position to displacement, but repeat first value because it's missing on the diff process
	#do not hijack spline like this because this works for speed, but then acceleration has big error
	#speedSpline = positionSpline
	#speedSpline$y = c(diff(positionSpline$y)[1],diff(positionSpline$y))

	speed = c(diff(positionSpline$y)[1],diff(positionSpline$y))
	speedSpline <- smooth.spline( 1:length(speed), speed, spar=0) #don't do smoothing, just convert speed to an spline

	return (speedSpline)
}

#same smoothing than getSped
getPositionSmoothed <- function(displacement, smoothing) {
	#no change affected by encoderConfiguration
	
	#use position because this does not make erronously change the initial and end of the curve
	#do spline with displacement is buggy because data is too similar -2, -1, 0, 1, 2, ...
	position <- cumsum(displacement)
	positionSpline <- smooth.spline( 1:length(position), position, spar=smoothing)
	
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


#used in alls eccons
reduceCurveBySpeed <- function(eccon, row, startT, startH, displacement, smoothingOneC) 
{
	#In 1.4.0 and before, we use smoothingOneEC on "ec", "ce"
	#but the problem is findSmoothingsEC has problems knowing the smoothingEC when users stays stand up lot of time before jump.
        #is better to reduceCurveBySpeed first in order to remove the not-moving phase
	#and then do findSmoothingsEC
	#for this reason, following code is commented, and it's only used smoothingOneC	

	#smoothing = smoothingOneEC
	#if(eccon == "c" || eccon == "ecS" || eccon == "ceS")
	#	smoothing = smoothingOneC
		
	smoothing = smoothingOneC

	speed <- getSpeed(displacement, smoothing)
	
	speed.ext=extrema(speed$y)

	#in order to reduce curve by speed, we search the cross of speed (in 0m/s)
        #before and after the peak value, but in "ec" and "ce" there are two peak values:
	#
	#speeds:
	#
	#ec         2
	#\         / \
	# \       /   \
	#-----------------
	#   \   /       \
	#    \ /         \
	#     1     
	#
	#ce   1
	#    / \         /
	#   /   \       /
	#-----------------
	# /       \   /
	#/         \ /
	#           2
	#
	#then we need two times: time1, time2 to search cross speed 0 before and after them

	time1 = 0
	time2 = 0
	if(eccon=="ec") {
		time1 = min(which(speed$y == min(speed$y)))
		time2 = max(which(speed$y == max(speed$y)))
	} else if(eccon=="ce") {
		time1 = min(which(speed$y == max(speed$y)))
		time2 = max(which(speed$y == min(speed$y)))
	} else { #c
		speed$y=abs(speed$y)
		time1 = min(which(speed$y == max(speed$y)))
		time2 = max(which(speed$y == max(speed$y)))
	}
	
	#write("time1, time2",stderr())
	#write(c(time1,time2),stderr())

	#now that times are defined we can work in ABS for all the curves
	speed$y=abs(speed$y)

	#left adjust
	#find the speed.ext$cross at left of max speed
	x.ini = 0 #good to declare here
	ext.cross.len = length(speed.ext$cross[,2])
	if(ext.cross.len == 0)
		x.ini = 0
	else if(ext.cross.len == 1) {
		if(speed.ext$cross[,2] < time1) 
			x.ini = speed.ext$cross[,2]
	} else { 
		for(i in speed.ext$cross[,2]) 
			if(i < time1) 
				x.ini = i
	}

	#right adjust
	#find the speed.ext$cross at right of max speed
	x.end = length(displacement) #good to declare here
	#ext.cross.len = length(speed.ext$cross[,2])
	if(ext.cross.len == 0)
		x.end = length(displacement)
	else if(ext.cross.len == 1) {
		if(speed.ext$cross[,2] > time2) 
			x.end = speed.ext$cross[,2]
	} else { 
		for(i in rev(speed.ext$cross[,2])) 
			if(i > time2) 
				x.end = i
	}

	#print(speed.ext$cross[,2])
	#print(ext.cross.len)
	#print(c("time1,time2",time1,time2))
	#print(c("x.ini x.end",x.ini,x.end))

	#to know the new startH
	#calculate displacement from original start to the new: x.ini
	startH.old = startH
	startH = startH + sum(displacement[1:x.ini])
	#print(c("old startH:", startH.old, "; new startH:", startH))

	return(c(startT + x.ini, startT + x.end, startH))
}


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



#go here with every single curve
#repOp has the options for this repetition
#repOp$eccon="c" one time each curve
#repOp$eccon="ec" one time each curve
#repOp$eccon="ecS" means ecSeparated. two times each curve: one for "e", one for "c"
#kinematicsF <- function(displacement, massBody, massExtra, exercisePercentBodyWeight,
#			encoderConfigurationName,diameter,diameterExt,anglePush,angleWeight,inertiaMomentum,gearedDown,
#			smoothingOneEC, smoothingOneC, g, eccon, isPropulsive)
#eccModesStartOnGround is default mode for calculations on e, ec starting when person lands,
#	but on single analysis instant array, disable it in order to see the full repetition
#	also think what to do on export CSV
kinematicsF <- function(displacement, repOp, smoothingOneEC, smoothingOneC, g, isPropulsive, eccModesStartOnGround)
{
	print("at kinematicsF")

	smoothing = 0
	if(repOp$eccon == "c" || repOp$eccon == "e")
		smoothing = smoothingOneC
	else
		smoothing = smoothingOneEC
	

	print(c("repOp$eccon, smoothing:", repOp$eccon, smoothing))

	speed <- getSpeedSafe(displacement, smoothing)
	
	accel <- getAccelerationSafe(speed)
	
	#print(c(" ms",round(mean(speed$y),5)," ma",round(mean(accel$y),5)))
	#print(c(" Ms",round(max(speed$y),5)," Ma",round(max(accel$y),5)))
	#print(c(" |ms|",round(mean(abs(speed$y)),5)," |ma|:",round(mean(abs(accel$y)),5)))
	#print(c(" |Ms|",round(max(abs(speed$y)),5)," |Ma|",round(max(abs(accel$y)),5)))

	#speed comes in mm/ms when derivate to accel its mm/ms^2 to convert it to m/s^2 need to *1000 because it's quadratic
	accel$y <- accel$y * 1000 

	eccentric = 0
	concentric = 0
	propulsiveEnd = 0

	#print(c("at kinematicsF eccon: ", repOp$eccon, " length(displacement): ",length(displacement)))

	#search propulsiveEnd
	if(isPropulsive) {
		if(repOp$eccon=="c") {
			concentric=1:length(displacement)
			
			maxSpeedT <- min(which(speed$y == max(speed$y)))
			maxSpeedTInConcentric = maxSpeedT
			
			propulsiveEnd = findPropulsiveEnd(accel$y, concentric, maxSpeedTInConcentric,
							  repOp$econfName, repOp$anglePush, repOp$angleWeight, 
							  repOp$massBody, repOp$massExtra, repOp$exPercentBodyWeight)
		} else if(repOp$eccon=="ec") {
			phases=findECPhases(displacement,speed$y)
			eccentric = phases$eccentric
			isometric = phases$isometric
			concentric = phases$concentric
	
			#temporary fix problem of found MinSpeedEnd at right
			if(eccentric == 0 && isometric == 0 && concentric == 0)
				propulsiveEnd = length(displacement)
			else {
				maxSpeedT <- min(which(speed$y == max(speed$y)))
				maxSpeedTInConcentric = maxSpeedT - (length(eccentric) + length(isometric))

				propulsiveEnd = length(eccentric) + length(isometric) + 
						findPropulsiveEnd(accel$y, concentric, maxSpeedTInConcentric, 
								  repOp$econfName, repOp$anglePush, repOp$angleWeight, 
								  repOp$massBody, repOp$massExtra, repOp$exPercentBodyWeight)
				#print(c("lengths: ", length(eccentric), length(isometric), findPropulsiveEnd(accel$y,concentric), propulsiveEnd))
			}
		} else if(repOp$eccon=="e") {
			#not repOp$eccon="e" because not propulsive calculations on eccentric
		} else { #ecS
			#print("WARNING ECS\n\n\n\n\n")
		}
	}

	dynamics = getDynamics(repOp$econfName,
			speed$y, accel$y, repOp$massBody, repOp$massExtra, repOp$exPercentBodyWeight, 
			repOp$gearedDown, repOp$anglePush, repOp$angleWeight,
			displacement, repOp$diameter, repOp$inertiaM, smoothing)
	mass = dynamics$mass
	force = dynamics$force
	power = dynamics$power

	start <- 1
	end <- length(speed$y)


	#on "e", "ec", start on ground
	if(eccModesStartOnGround && ! isInertial(repOp$econfName) && 
	   (repOp$eccon == "e" || repOp$eccon == "ec")) {
		#if eccentric is undefined, find it
		if(eccentric == 0) {
			if(repOp$eccon == "e")
				eccentric = 1:length(displacement)
			else {  #(repOp$eccon=="ec")
				phases=findECPhases(displacement,speed$y)
				eccentric = phases$eccentric
			}
		}
		
		weight = mass * g
		if(length(which(force[eccentric] <= weight)) > 0) {
			landing = max(which(force[eccentric]<=weight))
			start = landing
		}
	}

	if( isPropulsive && ( repOp$eccon== "c" || repOp$eccon == "ec" ) )
		end <- propulsiveEnd

	#as acceleration can oscillate, start at the eccentric part where there are not negative values
	#print(c(inertiaMomentum, repOp$eccon, length(eccentric), min(accel$y[eccentric])))
	if(repOp$inertiaM > 0 && (repOp$eccon == "e" || repOp$eccon == "ec")) 
	{
		if(repOp$eccon=="e") {
			eccentric=1:length(displacement)
		}
		
		#if there is eccentric data and there are negative values
	   	if(length(eccentric) > 0 && min(accel$y[eccentric]) < 0)
		{ 
			#print(c("length(eccentric)",length(eccentric)))
			#print("min accel y eccentric ++++++++++++")
			#print(c("min(accel$y[eccentric])",min(accel$y[eccentric])))
	
			#print(c("accel$y[eccentric]", accel$y[eccentric]))
			#print("displacement eccentric")
			#print(displacement[eccentric])



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

	return(list(
		    displ = displacement[start:end], 
		    speedy = speed$y[start:end], 
		    accely = accel$y[start:end], 
		    force = force[start:end], 
		    power = power[start:end], 
		    mass = mass))
}

findECPhases <- function(displacement,speed) {
	speed.ext=extrema(speed)
	#print(speed.ext)
	#print(speed)
	
	#In all the extrema minindex values, search which range (row) has the min values,
	#and in this range search last value
	#print("searchMinSpeedEnd")
	searchMinSpeedEnd = max(which(speed == min(speed)))
	#print(searchMinSpeedEnd)
	
	#In all the extrema maxindex values, search which range (row) has the max values,
	#and in this range search first value
	#print("searchMaxSpeedIni")
	searchMaxSpeedIni = min(which(speed == max(speed)))
	#print(searchMaxSpeedIni)
	
	#find the cross between both
	#print("speed.ext-Cross")
	#print(speed.ext$cross[,1])
	#print("search min cross: crossMinRow")

	crossMinRow=which(speed.ext$cross[,1] > searchMinSpeedEnd & speed.ext$cross[,1] < searchMaxSpeedIni)
	#print(crossMinRow)
			
	
	eccentric = 0
	isometric = 0
	concentric = 0

	#temporary fix problem of found MinSpeedEnd at right
	if(searchMinSpeedEnd > searchMaxSpeedIni)
		return(list(
			    eccentric=0,
			    isometric=0,
			    concentric=0))


	isometricUse = TRUE

	#print("at findECPhases")

	if(isometricUse) {
		eccentric=1:min(speed.ext$cross[crossMinRow,1])
		isometric=min(speed.ext$cross[crossMinRow,1]+1):max(speed.ext$cross[crossMinRow,2])
		concentric=max(speed.ext$cross[crossMinRow,2]+1):length(displacement)
	} else {
		eccentric=1:mean(speed.ext$cross[crossMinRow,1])
		#isometric=mean(speed.ext$cross[crossMinRow,1]+1):mean(speed.ext$cross[crossMinRow,2])
		concentric=mean(speed.ext$cross[crossMinRow,2]+1):length(displacement)
	}
	return(list(
		eccentric=eccentric,
		isometric=isometric,
		concentric=concentric))
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
		
		#propulsive phase ends at: [massBodyUsed*sin(anglePush) + massExtra*sin(angleWeight)] / (massBodyUsed + massExtra)
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

pafGenerate <- function(eccon, kinematics, massBody, massExtra, laterality, inertiaMomentum) {
	#print("speed$y")
	#print(kinematics$speedy)

	meanSpeed <- mean(kinematics$speedy)

	#max speed and max speed time can be at eccentric or concentric
	maxSpeed <- max(abs(kinematics$speedy))
	maxSpeedT <- min(which(abs(kinematics$speedy) == maxSpeed))

	meanPower <- mean(kinematics$power)
	
	if(eccon != "c") {
		meanSpeed <- mean(abs(kinematics$speedy))
		meanPower <- mean(abs(kinematics$power))
	}

	#print(c("eccon meanPowerSigned meanPowerABS",eccon, mean(kinematics$power), mean(abs(kinematics$power))))
	#print("kinematics$power")
	#print(abs(kinematics$power))

	peakPower <- max(abs(kinematics$power))
	peakPowerT <- min(which(abs(kinematics$power) == peakPower))
	
	#print(which(abs(kinematics$power) == peakPower))

	pp_ppt <- peakPower / (peakPowerT/1000)	# ms->s
	meanForce <- mean(kinematics$force)
	maxForce <- max(abs(kinematics$force))
	maxForceT <- min(which(abs(kinematics$force) == maxForce))

	#here paf is generated
	#mass is not used by pafGenerate, but used by Kg/W (loadVSPower)
	#meanForce and maxForce are not used by pafGenerate, but used by F/S (forceVSSpeed)
	mass = kinematics$mass
	#names will be used on graph.R writeCurves
	return(data.frame(
			  meanSpeed, maxSpeed, maxSpeedT,
			  meanPower, peakPower, peakPowerT, pp_ppt,
			  meanForce, maxForce, maxForceT,
			  mass, massBody, massExtra,		#kinematics$mass is Load
			  laterality, inertiaMomentum)) 
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
	   encoderConfigurationName == "ROTARYAXISINERTIALMOVPULLEY"
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
  massBody = getMassBodyByExercise(massBody,exercisePercentBodyWeight)
  
  if(
    encoderConfigurationName == "WEIGHTEDMOVPULLEYLINEARONPERSON1" ||
      encoderConfigurationName == "WEIGHTEDMOVPULLEYLINEARONPERSON1INV" ||
      encoderConfigurationName == "WEIGHTEDMOVPULLEYLINEARONPERSON2" ||
      encoderConfigurationName == "WEIGHTEDMOVPULLEYLINEARONPERSON2INV" ||
      encoderConfigurationName == "WEIGHTEDMOVPULLEYROTARYFRICTION" ||
      encoderConfigurationName == "WEIGHTEDMOVPULLEYROTARYAXIS" ) 
  {
    massExtra = getMass(massExtra, gearedDown, anglePush)
  } 
  
  massTotal = massBody + massExtra
  
  if(isInertial(encoderConfigurationName))
    return (getDynamicsInertial(encoderConfigurationName, displacement, diameter, massTotal, inertiaMomentum, smoothing))
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
    force <- massBody*(accel + g*sin(anglePush * pi / 180)) + massExtra*(g*sin(angleWeight * pi / 180) + accel)
  } else if(encoderConfigurationName == "LINEARONPLANE"){
    force <- (massBody + massExtra)*(accel + g*sin(anglePush * pi / 180))
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

getDynamicsInertial <- function(encoderConfigurationName, displacement, diameter, mass, inertiaMomentum, smoothing)
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
    forceBody = mass * (accel + g * sin(anglePush * pi / 180))
    powerDisc = abs((inertiaMomentum * angleAccel) * angleSpeed)
    powerBody = abs(mass * (accel + g * sin(anglePush * pi / 180)) * speed)

  } else if(encoderConfigurationName == "ROTARYAXISINERTIALMOVPULLEY" ||
              encoderConfigurationName == "ROTARYFRICTIONAXISINERTIALMOVPULLEY" ||
              encoderConfigurationName == "ROTARYFRICTIONSIDEINERTIALMOVPULLEY"){
    #With a moving pulley, the displacement of the body is half of the displacement of the rope in the inertial machine side
    #So, the multiplier is 4 instead of 2 to get the rotational kinematics.
    angle = position.m * 4 / diameter.m
    angleSpeed = speed * 4 / diameter.m
    angleAccel = accel * 4 / diameter.m
    anglePush = 90 #TODO: send from C#
    #The configuration covers horizontal, vertical and inclined movements
    #If the movement is vertical g*sin(alpha) = g
    #If the movement is horizontal g*sin(alpha) = 0

    forceDisc = abs(inertiaMomentum * angleAccel) * (2 / diameter.m)
    forceBody = mass * (accel + g * sin(anglePush * pi / 180))
    powerDisc = abs((inertiaMomentum * angleAccel) * angleSpeed)
    powerBody = abs(mass * (accel + g * sin(anglePush * pi / 180)) * speed)


  } else if(encoderConfigurationName == "ROTARYAXISINERTIALLATERAL" ||
              encoderConfigurationName == "ROTARYFRICTIONAXISINERTIALLATERAL" ||
              encoderConfigurationName == "ROTARYFRICTIONSIDEINERTIALLATERAL"){
    angle = position.m * 2 / diameter.m
    angleSpeed = speed * 2 / diameter.m
    angleAccel = accel * 2 / diameter.m
    anglePush = 0 #TODO: send from C#
    
    forceDisc = abs(inertiaMomentum * angleAccel) * (2 / diameter.m)
    forceBody = mass * accel
    powerDisc = abs((inertiaMomentum * angleAccel) * angleSpeed)
    powerBody = abs(mass * accel * speed)

  }

  force = forceDisc + forceBody
  power = powerDisc + powerBody

  loopsMax = diameter.m * max(angle)
  loopsAblines = seq(from=0, to=loopsMax, by=diameter.m*pi)

	return(list(displacement=displacement, mass=mass, force=force, power=power, 
		    loopsAblines=loopsAblines, angleSpeed=angleSpeed,
		    forceDisc=forceDisc, forceBody=forceBody, powerDisc=powerDisc, powerBody=powerBody))
}



#in signals and curves, need to do conversions (invert, diameter)
getDisplacement <- function(encoderConfigurationName, displacement, diameter, diameterExt) {
	#no change
	#WEIGHTEDMOVPULLEYLINEARONPERSON1, WEIGHTEDMOVPULLEYLINEARONPERSON1INV,
	#WEIGHTEDMOVPULLEYLINEARONPERSON2, WEIGHTEDMOVPULLEYLINEARONPERSON2INV,
	#LINEARONPLANE
	#ROTARYFRICTIONSIDE
	#WEIGHTEDMOVPULLEYROTARYFRICTION
  #ROTARYAXISINERTIALMOVPULLEY

  if(
	   encoderConfigurationName == "LINEARINVERTED" ||
	   encoderConfigurationName == "WEIGHTEDMOVPULLEYLINEARONPERSON1INV" ||
	   encoderConfigurationName == "WEIGHTEDMOVPULLEYLINEARONPERSON2INV") 
	  #On inverted modes the direction of the displacement is changed
  {
	  displacement = -displacement
  } else if(encoderConfigurationName == "WEIGHTEDMOVPULLEYONLINEARENCODER") 
  {
	  #On geared down machines the displacement of the subject is multiplied by gearedDown
	  #default is: gearedDown = 2. Future maybe this will be a parameter
	  displacement = displacement * 2
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
	     encoderConfigurationName == "ROTARYAXISINERTIALLATERAL") {
		displacementMeters = displacement / 1000 #mm -> m
		diameterMeters = diameter / 100 #cm -> m
		ticksRotaryEncoder = 200 #our rotary axis encoder send 200 ticks per turn
		#angle in radians
		angle = cumsum(displacementMeters * 1000) * 2 * pi / ticksRotaryEncoder
		position = angle * diameterMeters / 2
		position = position * 1000	#m -> mm
		#this is to make "inverted cumsum"
		displacement = diff(position) #this displacement is going to be used now
		displacement = c(displacement[1],displacement) #this is to recuperate the lost 1st value in the diff operation
	} else if(encoderConfigurationName == "ROTARYFRICTIONSIDEINERTIAL" ||
              encoderConfigurationName == "ROTARYFRICTIONSIDEINERTIALLATERAL"){
	  displacement = displacement * diameter / diameterExt #displacement of the axis
	} else if(encoderConfigurationName == "ROTARYFRICTIONAXISINERTIALMOVPULLEY"){
	  displacement = displacement / gearedDown #if gearedDown = 2 : Half the displacement of the axis
	} else if(encoderConfigurationName == "ROTARYFRICTIONSIDEINERTIALMOVPULLEY"){
	  displacement = displacement * diameter /(gearedDown * diameterExt) #if gearedDown = 2 : Half the displacement of the axis
	} else if(encoderConfigurationName == "ROTARYAXISINERTIALMOVPULLEY"){
	  displacementMeters = displacement / 1000 #mm -> m
	  diameterMeters = diameter / 100 #cm -> m
	  ticksRotaryEncoder = 200 #our rotary axis encoder send 200 ticks per turn
	  #angle in radians
	  angle = cumsum(displacementMeters * 1000) * 2 * pi / ticksRotaryEncoder
	  position = angle * diameterMeters / 2
	  position = position * 1000 / gearedDown 	#m -> mm	if gearedDown = 2 : Half the displacement of the axis
	  #this is to make "inverted cumsum"
	  displacement = diff(position) #this displacement is going to be used now
	  displacement = c(displacement[1],displacement) #this is to recuperate the lost 1st value in the diff operation
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
#To divide the con-ecc in two phases (because for the encoder is only one phsae because it rotates in the same direction), we need to know the positionAtStart
getDisplacementInertialBody <- function(positionStart, displacement, draw, title) 
{
	position=cumsum(displacement)
	position.ext=extrema(position)

	#print("at findCurvesInertial")
	#print(position.ext)

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
			position.ext=extrema(position)
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
		     lty=2,col=col) 

		abline(h=0, lty=2, col="gray")
	
		lines((1:length(position))/1000,positionPerson/10,lty=1,lwd=2)

		title(title, cex.main=1, font.main=1)
		mtext(paste(translateToPrint("time"),"(s)"),side=1,adj=1,line=-1)
		mtext(paste(translateToPrint("displacement"),"(cm)"),side=2,adj=1,line=-1)
	}
	return(displacementPerson)
}


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

