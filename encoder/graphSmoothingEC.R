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
#   Copyright (C) 2004-2017  	Xavier de Blas <xaviblas@gmail.com> 
#   Copyright (C) 2014-2017   	Xavier Padullés <x.padulles@gmail.com>
# 

findSmoothingsECGetPowerNI <- function(speed)
{
	acceleration <- getAcceleration(speed)
	acceleration$y <- acceleration$y * 1000
	force <- 50 * (acceleration$y + 9.81) #Do always with 50Kg right now. TODO: See if there's a need of real mass value
	power <- force * speed$y
	return(power)
}
findSmoothingsECGetPowerI <- function(displacement, encoderConfigurationName, diameter, massTotal, inertiaMomentum, gearedDown, smoothing)
{
	dynamics = getDynamicsInertial(encoderConfigurationName, displacement, diameter, massTotal, inertiaMomentum, gearedDown, smoothing)
	return(dynamics$power)
}
#splinesPredictGraph <- function(x,y)
#{
#	smodel = smooth.spline(y,x)
#	plot(x,y)
#	for (i in seq(y[1], y[length(y)], length.out=30)) {
#		predicted <- predict(smodel,i)
#		points(predicted$y, predicted$x, col="red")
#	}
#	return(smodel)
#
#}
findSmoothingsECYPoints <- function(eccentric.concentric, conStart, conEnd, x, maxPowerConAtCon,
		       encoderConfigurationName, diameter, massTotal, inertiaMomentum, gearedDown)
{
	y <- NULL 
	count <- 1
	for(j in x)
	{
		speed <- getSpeed(eccentric.concentric, j)
		smoothingOneEC = j

		powerTemp = NULL
		if(! isInertial(encoderConfigurationName) )
			powerTemp <- findSmoothingsECGetPowerNI(speed)
		else
			powerTemp <- findSmoothingsECGetPowerI(eccentric.concentric, encoderConfigurationName, 
							       diameter, massTotal, inertiaMomentum, gearedDown, smoothingOneEC)
		
		#find the max power in concentric phase of this ecc-con movement
		maxPowerConAtFullrep <- max(powerTemp[conStart:conEnd])
		y[count] = maxPowerConAtFullrep

		#write("kkYPoints", stderr())
		#write(paste("smoothingOneEC, maxPowerConAtFullrep, maxPowerConAtCon, diff", 
		#	    smoothingOneEC,
		#	    round(maxPowerConAtFullrep,2), 
		#	    round(maxPowerConAtCon,2),
		#	    round( (maxPowerConAtFullrep - maxPowerConAtCon),2 ))
		#, stderr())
		
		count <- count +1
	}

	return(y)
}
				


#called on "ec" and "ce" to have a smoothingOneEC for every curve
#this smoothingOneEC has produce same speeds than smoothing "c"

#on op$Analysis=="single", singleCurveNum is the curve that has to be analysed.
#On the rest of op$Analysis, singleCurveNum is -1 meaning "All"
findSmoothingsEC <- function(singleFile, displacement, curves, singleCurveNum, eccon, smoothingOneC, minHeight,
			     singleFileEncoderConfigurationName, singleFileDiameter, singleFileInertiaMomentum, singleFileGearedDown)
{
	ptm <- as.vector(proc.time()[3])
	write("time start", stderr())
	write(ptm, stderr())

	smoothings = NULL
	n=length(curves[,1])

	#if not "ec" or "ce" just have a value of 0 every curve,
	#no problem, this value will not be used
	#is just to not make crash other parts of the software like reduceCurveBySpeed
	if(eccon != "ec" && eccon != "ce") {
		for(i in 1:n) {
			smoothings[i] = 0
		}
	} else {
		#on every curve...
		for(i in 1:n) {
			#maybe the global eccon == "ec" or "ce" but the individual eccon of each curve is "c", then just do the same as above
			if( (singleFile && eccon == "c") || (! singleFile && curves[i,8] == "c") )
				smoothings[i] = 0
			else {
				#on op$Analysis=="single", only analyse one curve
				if(singleCurveNum > 0 && i != singleCurveNum) {
					smoothings[i] = 0
					next
				}

				# 0 find concentric
				eccentric.concentric = displacement[curves[i,1]:curves[i,2]]

				# 1 get the position
				position = cumsum (eccentric.concentric)

				#2 analyze the "c" phase
				changeEccCon <- mean(which(position == min(position)))
				ecc_l <- reduceCurveByPredictStartEnd (eccentric.concentric[1:changeEccCon],
								       "e", minHeight)
				con_l <- reduceCurveByPredictStartEnd (eccentric.concentric[changeEccCon:length(eccentric.concentric)],
								       "c", minHeight)

				conStart <- changeEccCon + con_l$startPos -1
				conEnd <- changeEccCon + con_l$endPos -1

				concentric=displacement[(curves[i,1] -1 +conStart):(curves[i,1] -1 +conEnd)]
				#note that eccentric.concentric could also be reduced to have data more similar to final ec graph

				# 3 get max power concentric at concentric phase with current smoothing
				if (length (unique (cumsum (concentric))) < 4)
				{
					smoothings[i] = smoothingOneC
					next
				}
				speed <- getSpeed(concentric, smoothingOneC)

				# 3.1 assign values from Roptions.txt (singleFile), or from curves
				myEncoderConfigurationName = singleFileEncoderConfigurationName;
				myDiameter = singleFileDiameter;
				myInertiaMomentum = singleFileInertiaMomentum;
				myGearedDown = singleFileGearedDown;
				if(! singleFile) {
					myEncoderConfigurationName = curves[i,11];
					myDiameter = curves[i,12];
					myInertiaMomentum = curves[i,16];
					myGearedDown = curves[i,17];
				}
			
				# 3.2
				powerTemp = NULL	
				if(! isInertial(myEncoderConfigurationName) )
					powerTemp <- findSmoothingsECGetPowerNI(speed)
				else
					powerTemp <- findSmoothingsECGetPowerI(concentric, myEncoderConfigurationName,
									       myDiameter, 100, myInertiaMomentum, myGearedDown, smoothingOneC)
				
				maxPowerConAtCon <- max(powerTemp)

				# 4 get max power concentric (y) at eccentric-concentric phase with current smoothing of an interval of possible smoothings (x)

				x <- seq(from = as.numeric(smoothingOneC), 
						    to = as.numeric(smoothingOneC)/4, 
						    length.out = 8)
				y <- findSmoothingsECYPoints(eccentric.concentric, conStart, conEnd, x, maxPowerConAtCon,
						myEncoderConfigurationName, myDiameter, 100, myInertiaMomentum, myGearedDown)
				write(paste("findSmoothinsECYPoints: x, y", x, y), stderr())

				#write("smooth.spline x (a)", stderr())	
				#write(x, stderr())
				#write("smooth.spline y (a)", stderr())	
				#write(y, stderr())
			
				#if less than 4 unique x or y cannot smooth spline. Just use smoothingOneC as default value
				if(length(unique(x)) < 4 || length(unique(y)) < 4) {
					smoothings[i] = smoothingOneC
					next
				}

				#smodel <- smooth.spline(y,x)
				smodel <- smoothSplineSafe(y,x)
				if(is.null(smodel)) {
					smoothings[i] = smoothingOneC
					next
				}

				smoothingOneEC <- predict(smodel, maxPowerConAtCon)$y
				write(paste("smoothingOneEC", smoothingOneEC), stderr())
					
				# 4.1 check how it worked

				if(length(unique(eccentric.concentric)) < 4 ) {
					smoothings[i] = smoothingOneEC
					next
				}
				speed <- getSpeed(eccentric.concentric, smoothingOneEC)
				
				if(! isInertial(myEncoderConfigurationName) )
					powerTemp <- findSmoothingsECGetPowerNI(speed)
				else
					powerTemp <- findSmoothingsECGetPowerI( eccentric.concentric, myEncoderConfigurationName,
									       myDiameter, 100, myInertiaMomentum, myGearedDown, smoothingOneEC)
				
				#find the max power in concentric phase of this ecc-con movement
				maxPowerConAtFullrep <- max(powerTemp[conStart:conEnd])

				# 5 check if first aproximation is OK

				#write(paste("MID i, smoothingOneEC, maxPowerConAtFullrep, maxPowerConAtCon, diff", 
				#	    i, smoothingOneEC,
				#	    round(maxPowerConAtFullrep,2), 
				#	    round(maxPowerConAtCon,2),
				#	    round( (maxPowerConAtFullrep - maxPowerConAtCon),2 ))
				#, stderr())

				# 6 create new x values closer

				temp.list <- findXValuesClose(x, y, maxPowerConAtCon)
				xUpperValue <- temp.list[[1]]
				xLowerValue <- temp.list[[2]]

				# 7 get max power concentric (y) at eccentric-concentric phase with current smoothing of an interval of possible smoothings (x)
				
				x <- seq(
					 from = xUpperValue, 
					 to = xLowerValue,
					 length.out = 8)
				y <- findSmoothingsECYPoints(eccentric.concentric, conStart, conEnd, x, maxPowerConAtCon,
						myEncoderConfigurationName, myDiameter, 100, myInertiaMomentum, myGearedDown)
				
			
				#write("smooth.spline x (b)", stderr())
				#write(x, stderr())
				#write("smooth.spline y (b)", stderr())	
				#write(y, stderr())
				
				#if less than 4 unique x or y cannot smooth spline. Just use recent calculed smoothingOneEC as default value
				if(length(unique(x)) < 4 || length(unique(y)) < 4) {
					smoothings[i] = smoothingOneEC
					next
				}

				#smodel <- smooth.spline(y,x)
				smodel <- smoothSplineSafe(y,x)
				if(is.null(smodel)) {
					smoothings[i] = smoothingOneEC
					next
				}
				smoothingOneEC <- predict(smodel, maxPowerConAtCon)$y
					
				# 8 check if aproximation is OK
				
				if(! isInertial(myEncoderConfigurationName) )
					powerTemp <- findSmoothingsECGetPowerNI(speed)
				else
					powerTemp <- findSmoothingsECGetPowerI( eccentric.concentric, myEncoderConfigurationName,
									       myDiameter, 100, myInertiaMomentum, myGearedDown, smoothingOneEC)
				
				# 9 find the max power in concentric phase of this ecc-con movement
				maxPowerConAtFullrep <- max(powerTemp[conStart:conEnd])
				
				write(paste("FINAL smooth EC: i, smoothingOneEC, maxPowerConAtFullrep, maxPowerConAtCon, diff", 
					    i, smoothingOneEC,
					    round(maxPowerConAtFullrep,2), 
					    round(maxPowerConAtCon,2),
					    round( (maxPowerConAtFullrep - maxPowerConAtCon),2 ))
				, stderr())

				#use smoothingOneEC
				smoothings[i] = smoothingOneEC
			}
		}
	}
	
	write("time end", stderr())
	write(as.vector(proc.time()[3]) - ptm, stderr())
		
	return(smoothings)
}

smoothAllSetYPoints <- function(smooth.seq, displacement, 
				  econfName, massBody, massExtra, exPercentBodyWeight, 
				  gearedDown, anglePush, angleWeight,
				  diameter, inertiaMomentum)
{
	y <- NULL 
	count <- 1
	for (i in smooth.seq) {
		#print(c("i",i))
		speed <- getSpeed(displacement, i)
		accel <- getAccelerationSafe(speed)
		#speed comes in mm/ms when derivate to accel its mm/ms^2 to convert it to m/s^2 need to *1000 because it's quadratic
		accel$y <- accel$y * 1000 

		dynamics <- getDynamics (econfName,
					 speed$y, accel$y, 
					 massBody, massExtra, exPercentBodyWeight, 
					 gearedDown, anglePush, angleWeight,
					 displacement, diameter, 
					 inertiaMomentum, i	#smoothing
					 )
		y[count] = max(dynamics$power)
		count <- count +1
	}
	return(y)
}

#Attention: x should be from high to low!
#eg 
#x:   .7,     .6125,     .525,     .4375,     .35
#y: 1156, 1190     , 1340    , 1736     , 2354
#lowerValue ald it's lowerPos are reffered to the x vector. 1 means the first (0.7)
#A) if we find the x for an y = 1900, x should be between .4375 (lowerValue) and .35 (upperValue)
#B) if we find the x for an y = 2500, x should be between .35 (lowerValue) and (right of .35) (upperValue)
#C) if we find the x for an y = 1000, x should be between (left of .7) (lowerValue) and .7 (upperValue)

findXValuesClose <- function(x, y, y.searched)
{
	xUpperValue = NULL
	xLowerValue = NULL

	upperPos <- min(which(y > y.searched)) #A: 5, C:1
	if(is.infinite(upperPos)) {	
		xUpperValue <- x[length(x)] - (x[length(x) -1] - x[length(x)])	#B: .35 - (.4375-.35) = .2625
		xLowerValue <- x[length(x)]					#B: .35
	}
	else {
		xUpperValue <- x[upperPos]	#A: .35
		lowerPos <- upperPos -1

		if(lowerPos >= 1)
			xLowerValue <- x[lowerPos]	#A: .4375
		else
			xLowerValue <- x[1] + (x[1] - x[2]) #C: .7 + (.7-.6125) = .7875
	}
	return(list(xUpperValue, xLowerValue))
}
