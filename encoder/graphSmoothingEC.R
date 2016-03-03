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
#   Copyright (C) 2004-2015  	Xavier de Blas <xaviblas@gmail.com> 
#   Copyright (C) 2014-2015   	Xavier Padullés <x.padulles@gmail.com>
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
findSmoothingsEC <- function(singleFile, displacement, curves, eccon, smoothingOneC,
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
#0 find concentric
				eccentric.concentric = displacement[curves[i,1]:curves[i,2]]

				#get the position
				position=cumsum(displacement[curves[i,1]:curves[i,2]])

				#analyze the "c" phase
				#Note dividing phases can be done using the speed,
				#but there's no need of this small difference here 
				conStart = 0
				conEnd = 0
				if(eccon=="ec") {
					conStart = mean(which(position == min(position)))
					conEnd = length(position) -1
					#the -1 is because the line below: "concentric=" will fail in curves[i,1]+end
					#and will add an NA
				} else { #(eccon=="ce")
					conStart = 0
					conEnd = mean(which(position == max(position)))
				}

				
				#1 get max power concentric at concentric phase with current smoothing

				speed <- getSpeed(concentric, smoothingOneC)

				#assign values from Roptions.txt (singleFile), or from curves
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
			
				powerTemp = NULL	
				if(! isInertial(myEncoderConfigurationName) )
					powerTemp <- findSmoothingsECGetPowerNI(speed)
				else
					powerTemp <- findSmoothingsECGetPowerI(concentric, myEncoderConfigurationName,
									       myDiameter, 100, myInertiaMomentum, myGearedDown, smoothingOneC)
				
				maxPowerConAtCon <- max(powerTemp)

				#2 get max power concentric (y) at eccentric-concentric phase with current smoothing of an interval of possible smoothings (x)
				
				x <- seq(from = as.numeric(smoothingOneC), 
						    to = as.numeric(smoothingOneC)/2, 
						    length.out = 5)
				y <- findSmoothingsECYPoints(eccentric.concentric, conStart, conEnd, x, maxPowerConAtCon, 
						myEncoderConfigurationName, myDiameter, 100, myInertiaMomentum, myGearedDown)
				#write(paste("x, y", x, y), stderr())


				smodel <- smooth.spline(y,x)
				smoothingOneEC <- predict(smodel, maxPowerConAtCon)$y
				write(paste("smoothingOneEC", smoothingOneEC), stderr())
					
				#check how it worked

				speed <- getSpeed(eccentric.concentric, smoothingOneEC)
				
				if(! isInertial(myEncoderConfigurationName) )
					powerTemp <- findSmoothingsECGetPowerNI(speed)
				else
					powerTemp <- findSmoothingsECGetPowerI( eccentric.concentric, myEncoderConfigurationName,
									       myDiameter, 100, myInertiaMomentum, myGearedDown, smoothingOneEC)
				
				#find the max power in concentric phase of this ecc-con movement
				maxPowerConAtFullrep <- max(powerTemp[conStart:conEnd])

				#3 check if first aproximation is OK

				#write(paste("MID i, smoothingOneEC, maxPowerConAtFullrep, maxPowerConAtCon, diff", 
				#	    i, smoothingOneEC,
				#	    round(maxPowerConAtFullrep,2), 
				#	    round(maxPowerConAtCon,2),
				#	    round( (maxPowerConAtFullrep - maxPowerConAtCon),2 ))
				#, stderr())

				#4 create new x values closer

				#eg 
				#x:   .7,     .6125,     .525,     .4375,     .35
				#y: 1156, 1190     , 1340    , 1736     , 2354
				#lowerValue ald it's lowerPos are reffered to the x vector. 1 means the first (0.7)
				#A) if we find the x for an y = 1900, x should be between .4375 (lowerValue) and .35 (upperValue)
				#B) if we find the x for an y = 2500, x should be between .35 (lowerValue) and (right of .35) (upperValue)
				#C) if we find the x for an y = 1000, x should be between (left of .7) (lowerValue) and .7 (upperValue)

				xUpperValue = NULL
				xLowerValue = NULL

				upperPos <- min(which(y > maxPowerConAtCon)) #A: 5, C:1
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


				#5 get max power concentric (y) at eccentric-concentric phase with current smoothing of an interval of possible smoothings (x)
				
				x <- seq(
					 from = xUpperValue, 
					 to = xLowerValue,
					 length.out = 5)
				y <- findSmoothingsECYPoints(eccentric.concentric, conStart, conEnd, x, maxPowerConAtCon, 
						myEncoderConfigurationName, myDiameter, 100, myInertiaMomentum, myGearedDown)
				
				
				smodel <- smooth.spline(y,x)
				smoothingOneEC <- predict(smodel, maxPowerConAtCon)$y
					
				#6 check if aproximation is OK
				
				if(! isInertial(myEncoderConfigurationName) )
					powerTemp <- findSmoothingsECGetPowerNI(speed)
				else
					powerTemp <- findSmoothingsECGetPowerI( eccentric.concentric, myEncoderConfigurationName,
									       myDiameter, 100, myInertiaMomentum, myGearedDown, smoothingOneEC)
				
				#find the max power in concentric phase of this ecc-con movement
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
