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

#TODO: current BUGS
#peakpowerTime is not working ok at con-ecc
#paint "eccentric","concentric" labels are not ok at con-ecc


#----------------------------------
#Explanation of the code. Workflow:
#----------------------------------
#Define some variables, read options file and define more variables
#at end of file: call to loadLibraries(OperatingSystem)
#at end of file: call to doProcess(options)
#doProcess:
  #assign variables reading options
  #process curves:
    #if(! singleFile) reads "chronojump-encoder-graph-input-multi.csv", then read each file and define curves using files
    #if(singleFile) define curves using findCurves function
  #if analysis is single: paint 
  #if analysis is side: kinematicRanges will call kinematicsF to know common axes (max and mins) and the call to paint 
  #using curves and powerBars, paf table will be created. This will be used always, because writeCurves (on a file) is always true
  #if(Analysis=="exportCSV") data will be exported to CSV file
#----------------------------------

#TODO: bug: 705214 - Encoders selection and management
#Change notebook to have a third horizontal tab
#
#Inertial machines has to be always using the axis:
#1. if it's rotatory (friction): in contact with axis
#2. if it's rotatory (axis): connected to axis
#3. if it's linear (string): rolled at the axis
#
#in all need to provide the inertia momentum, body weight and extra weight (if
#									   any)
#
#in the 2. and 3. need to provide the diameter of the axis 
#
#In all the cases, need to convert data using fixRawdataInertial
#because we need to know the change of direction
#
#also preselect con-ecc when inertial machine is selected. Put in the same tab
#
#Need also to save encoder type and load encoder type. This has to be useful to
#have data introduced always and have different encoders.
#
#

debug = FALSE

#concentric, eccentric-concentric, repetitions of eccentric-concentric
#currently only used "c" and "ec". no need of ec-rep because c and ec are repetitive
#"ecS" is like ec but eccentric and concentric phases are separated, used in findCurves, this is good for treeview to know power... on the 2 phases
eccons=c("c","ec","ecS","ce","ceS") 

g = 9.81

colSpeed="springgreen3"; colForce="blue2"; colPower="tomato2"	#colors
#colSpeed="black"; colForce="black"; colPower="black"		#black & white
cols=c(colSpeed,colForce,colPower); lty=rep(1,3)	


#translateToPrint An expression is returned andcan only be printed. Don't do operations
#Important: An expression is returned because is the best way to ahndle unicode on windows
#take care not to do operations with this. Just print it
translateToPrint <- function(englishWord) {
	if(length(Translated[which(English == englishWord)]) == 0)
		return (englishWord) #not found, return english word
	else {
		myWord = Translated[which(English == englishWord)]
	
		#Needed conversion for Windows:
		#unicoded titles arrive here like this "\\", convert to "\", as this is difficult, do like this:
		#http://stackoverflow.com/a/17787736
		myWord = parse(text = paste0("'", myWord, "'"))
	
		return(myWord)
	}
}

translateVector <- function(englishVector) {
	translatedVector <- englishVector
	for(i in 1:length(englishVector)) {
		translatedVector[i] <- translateToPrint(englishVector[i])
	}

	return(translatedVector)
}


# This function converts top curve into bottom curve
#
#          /\
#         /  \        /
# _     B/    \C     /
#  \    /      \    /D
#  A\  /        \  /
#    \/          \/
#
# _     b        
#  \    /\    /\    /\
#   \  /  \  /  \  /  \
#    \/    \/    \/
#    ab    bc
#
# eg: in to curve, 'B' is a movement of disc rolling to right (or left),
# but in this movement, person has gone up/down. Up phase is ab-b. Down phase is b-bc.
# his function produces this transformation
# all displacement will be negative because we start on the top
#fixRawdataInertial <- function(displacement) {
#	#do not do this:
#	#position=cumsum(displacement)
#	#displacement[which(position >= 0)] = displacement[which(position >= 0)]*-1
#	
#	#do this: work with cumsum, do ABS on cumsum, then *-1
#	#then to obtain displacement again just do diff (and add first number)
#
#	position = abs(cumsum(displacement))*-1
#
#	#this is to make "inverted cumsum"
#	displacement = c(0,diff(position))
#
#	print(displacement)
#
#	return(displacement)
#}

#don't do this, because on inertial machines string will be rolled to machine and not connected to the body
#fixRawdataLI <- function(displacement) {
#	position = cumsum(displacement)
#	meanMax=mean(which(position == max(position)))
#
#	#this is to make "inverted cumsum"
#	displacement = c(0,diff(position))
#	
#	displacement[meanMax:length(displacement)] = displacement[meanMax:length(displacement)] * -1
#
#	return(displacement)
#}

#this is equal to runEncoderCaptureCsharp()
#but note getDisplacement hapens before this function, so no need getDisplacement here
#also don't need byteReadedRaw, and encoderReadedRaw. encoderReaded is 'displacement' here
findCurvesNew <- function(displacement, eccon, isInertial, min_height, draw, title) 
{
	#---- 1) declare variables ----
	
	byteReaded = NULL
					
	position = cumsum(displacement)

	sum = 0

	directionChangePeriod = 25
	directionChangeCount = 0
	directionNow = 1
	directionLastMSecond = 1
	directionCompleted = -1
	previousFrameChange = 0
	previousEnd = 1
	lastNonZero = 0

	heightAtCurveStart = 0
	heightAccumulated = 0

	inertialCaptureDirectionChecked = FALSE
	inertialCaptureDirectionInverted = FALSE
		
	capturingFirstPhase = TRUE

	startCurrent = NULL
	endCurrent = NULL

	startStored = 0
	endStored= 0
	startHStored = 0

	#curve we are about to store is concentric
	directionToStoreIsCon = NULL #bool
	#last stored curve is concentric. It's important on 'ec' to store first 'e', next 'c' ...
	directionStoredIsCon = NULL #bool
		
	#               3
	#              / \
	#             /   B
	#            /     \
	# --1       /
	#    \     /
	#     \   A
	#      \2/
	#
	# At B, we evaluate if 2-3 curve is concentric.
	# directionToStoreIsCon == TRUE


	#               3               5
	#              / \             /
	#             /   B           /
	#            /     \         /
	# --1       /       \       /
	#    \     /         \     /
	#     \   A           \   C
	#      \2/             \4/
	#
	# At C, we evaluate if 3-4 curve is eccentric.
	# directionToStoreIsCon == FALSE
	# Also we see that last stored curve is opposite:
	# directionStoredIsCon == TRUE
	#


	row = 1

	count = 1

	#---- 2) start ----

	while(count <= length(displacement)) {

	      byteReaded = displacement[count]

	      #TODO: need this?	
	      #if(inertialCaptureDirectionInverted)
	      #	byteReadedRaw = -1 * byteReadedRaw

	      sum = sum + byteReaded

	      #the inertial change of direction is not needed here because runEncoderCaptureCsharp() stores data already 'changed'
	      #so no change here is needed

	      #if string goes up or down, store the direction
	      if(byteReaded != 0)
		      directionNow = byteReaded / ( 1.0 * abs(byteReaded) ) #1 (up) or -1 (down)

	      #if we don't have changed the direction, store the last non-zero that we can find
	      if(directionChangeCount == 0 && directionNow == directionLastMSecond) {
		      if(byteReaded != 0)
			      lastNonZero = count
	      }

	      #if it's different than the last direction, mark the start of change
	      if(directionNow != directionLastMSecond) {
		      directionLastMSecond = directionNow
		      directionChangeCount = 0
	      } 
	      else if(directionNow != directionCompleted) {
		      #we are in a different direction than the last completed

		      directionChangeCount = directionChangeCount +1

		      if(directionChangeCount > directionChangePeriod)
		      {
			      startFrame = previousEnd
			      if(startFrame < 1)
				      startFrame = 1

			      startCurrent = startFrame
			      #-1 are because on C# count starts at 0 and here count starts at 1
			      endCurrent = ( (count -1) - directionChangeCount + (lastNonZero -1) ) / 2

			      if(endCurrent > startCurrent)
			      {
				      heightAtCurveStart = heightAccumulated

				      heightCurve = max(position[startCurrent:endCurrent]) - min(position[startCurrent:endCurrent])

				      previousEnd = endCurrent

				      heightAccumulated = heightAccumulated + heightCurve

				      heightCurve = abs(heightCurve) #mm -> cm
			      
				      sendCurve = TRUE

				      if(heightCurve >= min_height) {
					      #TODO: add the inertia stuff here?
					      
					      directionToStoreIsCon = ( position[endCurrent] > position[startCurrent] )

					      if( eccon == "c" && ! directionToStoreIsCon )
						      sendCurve = FALSE
					      if( (eccon == "ec" || eccon == "ecS") && directionToStoreIsCon && capturingFirstPhase )
						      sendCurve = FALSE
					      
					      #on ec, ecS don't have store two curves in the same direction
					      if( (eccon == "ec" || eccon == "ecS") && 
						 ! is.null(directionStoredIsCon) && 
						 directionToStoreIsCon == directionStoredIsCon )
						      sendCurve = FALSE

					      capturingFirstPhase = FALSE
				      } else {
					      sendCurve = FALSE
				      }

				      #store the curve
				      if(sendCurve) {
					      startStored[row] = startCurrent
					      endStored[row] = endCurrent
					      startHStored[row] = position[startCurrent]
					      row = row + 1
					      directionStoredIsCon = directionToStoreIsCon
				      }
			      }

			      previousFrameChange = count - directionChangeCount
			      directionChangeCount = 0
			      directionCompleted = directionNow
		      }
	      }

	      count = count +1
	}
	
	#if eccon it's 'ec' and last row it's 'e', delete it
	if(row > 1)
	{
		startStoredLast = startStored[length(startStored)]
		endStoredLast = endStored[length(endStored)]

		#write("startStoredLast, endStoredLast", stderr())
		#write(c(startStoredLast, endStoredLast), stderr())
	
		if ( position[startStoredLast] > position[endStoredLast] ) 
		{
			write("deleting last ecc row", stderr())
			startStored = startStored[-length(startStored)]
			endStored = endStored[-length(endStored)]
			startHStored = startHStored[-length(startHStored)]
		}
	}

	#if eccon == "ec" mix 'e' and 'c' curves
	if(eccon == "ec") {
		startStoredOld = startStored
		endStoredOld = endStored
		startHStoredOld = startHStored

		startStored = NULL
		endStored = NULL
		startHStored = NULL

		n=length(startStoredOld)
		count = 1
		for(i in seq(1, n, by=2)) {
			startStored[count] = startStoredOld[i]
			endStored[count] = endStoredOld[(i+1)]
			startHStored[count] = startHStoredOld[i]
			count = count +1
		}
	}


	if( isInertial && (eccon == "ec" || eccon == "ecS") ) 
	{
		#be careful on ec inertial to send an startStored of 1220.5 because can use the 1220 that can be a positive displacement
		#and then the 1221 can be negative and also the rest
		#then is better to take the second number (do ceiling) to avoid a change of direction on the beginning of the movement
		#
		#    AB          C
		#    /\          /\
		#   /  \        /
		#  /    \      /
		# /      \    /
		#         \__/
		#
		# On this example a ec can go from B to C. If it uses A,
		#then there will be a change of direction that will make getSpeed to produce a mistaken spline

		for(i in 1:length(startStored)) {
			startStored[i] = ceiling(startStored[i])
		}
	}

	if(draw) {
		lty=1
		col="black"
		plot((1:length(position))/1000			#ms -> s
		     ,position/10,				#mm -> cm
		     type="l",
		     xlim=c(1,length(position))/1000,		#ms -> s
		     xlab="",ylab="",axes=T,
		     lty=lty,col=col) 

		title(title, cex.main=1, font.main=1)
		mtext(paste(translateToPrint("time"),"(s)"),side=1,adj=1,line=-1)
		mtext(paste(translateToPrint("displacement"),"(cm)"),side=2,adj=1,line=-1)
	}
	
	return(as.data.frame(cbind(startStored,endStored,startHStored)))
}

#extrema method
#there are problems when small changes of direction are found
#unused now, use the new findCurves (above) that is equal to runEncoderCaptureCsharp()
findCurvesOld <- function(displacement, eccon, min_height, draw, title) {
	position=cumsum(displacement)
	position.ext=extrema(position)
	print("at findCurves")
	print(position.ext)

	start=0; end=0; startH=0
	tempStart=0; tempEnd=0;
	#TODO: fer algo per si no es detecta el minindex previ al salt
	if(eccon=="c") 
	{
		#1) fix big problems with set (signal). No minindex or no maxindex
		if(length(position.ext$minindex)==0) { position.ext$minindex=cbind(1,1) }
		if(length(position.ext$maxindex)==0) { position.ext$maxindex=cbind(length(position),length(position)) }

		#2) fixes if 1st minindex is after 1st maxindex
		if(position.ext$maxindex[1] < position.ext$minindex[1]) { position.ext$minindex = rbind(c(1,1),position.ext$minindex) } 
		
		#3 find the curves (enter the bucle)

		row=1; #row means each curve
		i=1; j=1 #i minimums; j maximums
		while(max(c(i,j)) <= min(c(length(position.ext$minindex[,1]),length(position.ext$maxindex[,1])))) 
		{
			#3.1) assign tempStart and tempEnd using extrema data

			#tempStart at the end of minindexs
			#tempStart = position.ext$minindex[i,2]

			#tempStart at the mean of minindexs
			#this is better because has more data in order to reduceCurveBySpeed
			#then we get similar results than pyserial_pyper.py
			tempStart = mean(c(position.ext$minindex[i,1],position.ext$minindex[i,2]))


			#end at the mean of maximum values then reduceCurveBySpeed will end it when first speed==0 at right of maxspeed 
			tempEnd = mean(c(position.ext$maxindex[j,1],position.ext$maxindex[j,2]))

			#end at the first maximum value
			#tempEnd = position.ext$maxindex[j,1]

			#3.2) get heigh. Store curve if it's enough

			height=position[tempEnd]-position[tempStart]
			if(height >= min_height) { 
				start[row] = tempStart
				end[row]   = tempEnd
				startH[row]= position[position.ext$minindex[i,1]]		#height at start
				row=row+1;
				#if(eccon=="c") { break } #c only needs one result
			} 
			i=i+1; j=j+1
		} #end of while
	} else { #ec, ecS, ce, ceS
		row=1; i=1; j=2

		referenceindex=0
		if(eccon=="ec" || eccon=="ecS") {
			referenceindex=position.ext$maxindex
		} else {
			referenceindex=position.ext$minindex
		}

		#when saved a row with ec-con, and there's only this curve, extrema doesn't find maxindex
		if(length(referenceindex) == 0) {
			start[1] = 1

			if(eccon=="ec" || eccon=="ecS")
				end[1] = mean(which(position == min(position)))
			else
				end[1] = mean(which(position == max(position)))

			startH[1]=position[1]
			start[2] =end[1]+1
			end[2]   =length(position)
			startH[2]=position[start[2]]
		}

		#if a person starts stand up and goes down, extrema maxindex don't find the initial position
		#if this person does 3 squats, only 2 will be found
		#add first value of all the serie (1ms time) to maxindex to help to detect this first curve
		referenceindex = rbind(c(1,1),referenceindex)

		while(j <= length(referenceindex[,1])) {
			tempStart = mean(c(referenceindex[i,1],referenceindex[i,2]))
			tempEnd   = mean(c(referenceindex[j,1],referenceindex[j,2]))

			if(eccon=="ec" || eccon=="ecS") {
				opposite=min(position[tempStart:tempEnd]) #find min value between the two tops
				mintop=min(c(position[tempStart],position[tempEnd])) #find wich top is lower
				height=mintop-opposite
			} else {
				opposite=max(position[tempStart:tempEnd]) #find max value between the two bottoms
				maxbottom=max(c(position[tempStart],position[tempEnd])) #find wich bottom is higher
				height=abs(maxbottom-opposite)
			}
			if(height >= min_height) { 
				if(eccon == "ecS" || eccon == "ceS") {
					start[row] = tempStart
					end[row]   = mean(which(position[tempStart:tempEnd] == opposite) + tempStart)
					startH[row] = position[referenceindex[i,1]]		#height at start
					row=row+1
					start[row] = end[(row-1)] + 1
					end[row]   = tempEnd
					startH[row] = position[start[row]]		#height at start
					row=row+1
					i=j
				} else {	#("ec" || "ce")
					start[row] = tempStart
					end[row]   = tempEnd
					startH[row] = position[referenceindex[i,1]]		#height at start
					row=row+1
					i=j
				}
			} else {
				if(eccon=="ec" || eccon=="ecS") {
					if(position[tempEnd] >= position[tempStart]) {
						i=j
					}
				} else {
					if(position[tempEnd] <= position[tempStart]) {
						i=j
					}
				}
			}
			j=j+1
		}
	}
	if(draw) {
		lty=1
		col="black"
		plot((1:length(position))/1000			#ms -> s
		     ,position/10,				#mm -> cm
		     type="l",
		     xlim=c(1,length(position))/1000,		#ms -> s
		     xlab="",ylab="",axes=T,
		     lty=lty,col=col) 

		title(title, cex.main=1, font.main=1)
		mtext(paste(translateToPrint("time"),"(s)"),side=1,adj=1,line=-1)
		mtext(paste(translateToPrint("displacement"),"(cm)"),side=2,adj=1,line=-1)
	}
	return(as.data.frame(cbind(start,end,startH)))
}

findSmoothingsECGetPower <- function(speed)
{
	acceleration <- getAcceleration(speed)
	acceleration$y <- acceleration$y * 1000
	force <- 50 * (acceleration$y + 9.81) #Do always with 50Kg right now. TODO: See if there's a need of real mass value
	power <- force * speed$y
	return(power)
}
#called on "ec" and "ce" to have a smoothingOneEC for every curve
#this smoothingOneEC has produce same speeds than smoothing "c"
findSmoothingsEC <- function(singleFile, displacement, curves, eccon, smoothingOneC) 
{
	#print(c("findSmoothingsEC: eccon smoothingOneC", eccon, smoothingOneC))

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
				eccentric.concentric = displacement[curves[i,1]:curves[i,2]]

				#get the position
				position=cumsum(displacement[curves[i,1]:curves[i,2]])

				#analyze the "c" phase
				#Note dividing phases can be done using the speed,
				#but there's no need of this small difference here 
				start = 0
				end = 0
				if(eccon=="ec") {
					start = mean(which(position == min(position)))
					end = length(position) -1
					#the -1 is because the line below: "concentric=" will fail in curves[i,1]+end
					#and will add an NA
				} else { #(eccon=="ce")
					start = 0
					end = mean(which(position == max(position)))
				}
				#print("start, end")
				#print(c(start, end))

				concentric=displacement[(curves[i,1]+start):(curves[i,1]+end)]
				
				#get max speed at "c"
				#print("calling speed 1")
				#print("unique(concentric)")
				#print(unique(concentric))
				#print("concentric")
				#print(concentric)
				
				speed <- getSpeed(concentric, smoothingOneC)
				#maxSpeedC=max(speed$y)
				
				powerC <- findSmoothingsECGetPower(speed)
				maxPowerC <- max(powerC) 

				#find max speed at "ec" that's similar to maxSpeedC
				smoothingOneEC = smoothingOneC
				for(j in seq(as.numeric(smoothingOneC),0,by=-.01)) 
				{
					#write("calling speed 2", stderr())
					speed <- getSpeed(concentric, j)
					
					smoothingOneEC = j
					
					#don't do it base on speed because difference is very tiny
					#do it based on power
					#maxSpeedEC=max(speed$y)
					#write(c("j",j,"maxC",round(maxSpeedC,3),"maxEC",round(maxSpeedEC,3)),stderr())
					#if(maxSpeedEC >= maxSpeedC)
					#	break

					powerEC <- findSmoothingsECGetPower(speed)
					maxPowerEC <- max(powerEC) 
					
					if(maxPowerEC >= maxPowerC)
						break
				}

				#use smoothingOneEC
				smoothings[i] = smoothingOneEC

				print(smoothings[i])
			}
		}
	}
		
	return(smoothings)
}




kinematicRanges <- function(singleFile, displacement, curves,
			    massBody, massExtra, exercisePercentBodyWeight,
			    encoderConfigurationName,diameter,diameterExt,anglePush,angleWeight,inertiaMomentum,gearedDown,
			    smoothingsEC, smoothingOneC, g, eccon, isPropulsive) {

	n=length(curves[,1])
	maxSpeedy=0; maxAccely=0; maxForce=0; maxPower=0
	myEccon = eccon

	for(i in 1:n) { 
		myMassBody = massBody
		myMassExtra = massExtra
		myExPercentBodyWeight = exercisePercentBodyWeight
			
		#encoderConfiguration
		myEncoderConfigurationName = encoderConfigurationName
		myDiameter = diameter
		myDiameterExt = diameterExt
		myAnglePush = anglePush
		myAngleWeight = angleWeight
		myInertiaMomentum = inertiaMomentum
		myGearedDown = gearedDown
		if(! singleFile) {
			myMassBody = curves[i,5]
			myMassExtra = curves[i,6]
			myEccon = curves[i,8]
			myExPercentBodyWeight = curves[i,10]

			#encoderConfiguration
			myEncoderConfigurationName = curves[i,11]
			myDiameter = curves[i,12]
			myDiameterExt = curves[i,13]
			myAnglePush = curves[i,14]
			myAngleWeight = curves[i,15]
			myInertiaMomentum = curves[i,16]
			myGearedDown = curves[i,17]
		}
	
		kn=kinematicsF(displacement[curves[i,1]:curves[i,2]],
			       myMassBody, myMassExtra, myExPercentBodyWeight,
			       myEncoderConfigurationName,myDiameter,myDiameterExt,myAnglePush,myAngleWeight,myInertiaMomentum,myGearedDown,
			       smoothingsEC[i], smoothingOneC, g, myEccon, isPropulsive)

		if(max(abs(kn$speedy)) > maxSpeedy)
			maxSpeedy = max(abs(kn$speedy))
		if(max(abs(kn$accely)) > maxAccely)
			maxAccely = max(abs(kn$accely))
		if(max(abs(kn$force)) > maxForce)
			maxForce = max(abs(kn$force))
		if(max(abs(kn$power)) > maxPower)
			maxPower = max(abs(kn$power))
	}

	return(list(
		speedy=c(-maxSpeedy,maxSpeedy),
		accely=c(-maxAccely,maxAccely),
		force=c(-maxForce,maxForce),
		power=c(-maxPower,maxPower)))
}

canJump <- function(encoderConfigurationName)
{
  if(encoderConfigurationName == "LINEAR" || encoderConfigurationName == "LINEARINVERTED")
    return(TRUE)
  
  return(FALSE)
}
  

paint <- function(displacement, eccon, xmin, xmax, yrange, knRanges, superpose, highlight,
	startX, startH, smoothingOneEC, smoothingOneC, massBody, massExtra, 
	encoderConfigurationName,diameter,diameterExt,anglePush,angleWeight,inertiaMomentum,gearedDown, #encoderConfiguration stuff
	title, subtitle, draw, showLabels, marShrink, showAxes, legend,
	Analysis, isPropulsive, inertialType, exercisePercentBodyWeight,
        showSpeed, showAccel, showForce, showPower	
	) {

	meanSpeedE = 0
	meanSpeedC = 0
	meanPowerE = 0
	meanPowerC = 0

	smoothing = 0
	if(eccon == "c")
		smoothing = smoothingOneC
	else
		smoothing = smoothingOneEC

	write(c("paint smoothing", smoothing), stderr())

	#eccons ec and ecS is the same here (only show one curve)
	#receive data as cumulative sum
	lty=c(1,1,1)

	print(c("xmin,xmax",xmin,xmax))

	displacement=displacement[xmin:xmax]
	position=cumsum(displacement)
	position=position+startH

	#to control the placement of the diferent axis on the right
	axisLineRight = 0
	marginRight = 8.5
	if(! showSpeed)
		marginRight = marginRight -2
	if(! showAccel)
		marginRight = marginRight -2
	if(! showForce)
		marginRight = marginRight -2
	if(! showPower)
		marginRight = marginRight -2

	#all in meters
	#position=position/1000

	if(draw) {
		#three vertical axis inspired on http://www.r-bloggers.com/multiple-y-axis-in-a-r-plot/
		par(mar=c(3, 3.5, 5, marginRight))
		if(marShrink) #used on "side" compare
			par(mar=c(1, 1, 4, 1))
	
		#plot distance
		#plot(a,type="h",xlim=c(xmin,xmax),xlab="time (ms)",ylab="Left: distance (mm); Right: speed (m/s), acceleration (m/s^2)",col="gray", axes=F) #this shows background on distance (nice when plotting distance and speed, but confusing when there are more variables)
		xlab="";ylab="";
		#if(showLabels) {
		#	xlab="time (ms)"
		#	ylab="Left: distance (mm); Right: speed (m/s), force (N), power (W)"
		#}
		ylim=yrange
		if(ylim[1]=="undefined") { ylim=NULL }
		plot(position-min(position),type="n",xlim=c(1,length(position)),ylim=ylim,xlab=xlab, ylab=ylab, col="gray", axes=F)

		title(main=title,line=-2,outer=T)
		mtext(subtitle,side=1,adj=0,cex=.8)
	

		if(showAxes) {
			axis(1) 	#can be added xmin
			axis(2)
		}
		
		par(new=T)
		colNormal="black"
		if(superpose)
			colNormal="gray30"
		yValues = position[startX:length(position)]-min(position[startX:length(position)])
		if(highlight==FALSE) {
			plot(startX:length(position),yValues,type="l",xlim=c(1,length(position)),ylim=ylim,
			     xlab="",ylab="",col="black",lty=lty[1],lwd=2,axes=F)
			par(new=T)
			plot(startX:length(position),yValues,type="h",xlim=c(1,length(position)),ylim=ylim,
			     xlab="",ylab="",col="grey90",lty=lty[1],lwd=1,axes=F)
		}
		else
			plot(startX:length(position),yValues,type="l",xlim=c(1,length(position)),ylim=ylim,xlab="",ylab="",col=colNormal,lty=2,lwd=3,axes=F)
		abline(h=0,lty=3,col="black")

		#abline(v=seq(from=0,to=length(position),by=500),lty=3,col="gray")


	}

	print(c("smoothing at paint=",smoothing))
	#speed
	speed <- getSpeed(displacement, smoothing)
	
	#show extrema values in speed
	speed.ext=extrema(speed$y)
	
	#accel (calculated here to use it on fixing inertialECstart and before plot speed
	accel <- getAcceleration(speed)
	#speed comes in mm/ms when derivate to accel its mm/ms^2 to convert it to m/s^2 need to *1000 because it's quadratic
	accel$y <- accel$y * 1000


	#if(draw & !superpose) 
	#	segments(x0=speed.ext$maxindex,y0=0,x1=speed.ext$maxindex,y1=speed$y[speed.ext$maxindex],col=cols[1])

	#declare variables:
	eccentric=NULL
	isometric=NULL
	concentric=NULL

	if(eccon=="c") {
		concentric=1:length(displacement)
	} else {	#"ec", "ce". Eccons "ecS" and "ceS" are not painted
		print("EXTREMA")
		#abline(v=speed.ext$maxindex,lty=3,col="yellow");
		#abline(v=speed.ext$minindex,lty=3,col="magenta")
		print(speed.ext)

		time1 = 0
		time2 = 0
		if(eccon=="ec") {
			time1 = max(which(speed$y == min(speed$y)))
			time2 = min(which(speed$y == max(speed$y)))
			labelsXeXc = c("Xe","Xc")
		} else { #(eccon=="ce")
			time1 = max(which(speed$y == max(speed$y)))
			time2 = min(which(speed$y == min(speed$y)))
			labelsXeXc = c("Xc","Xe")
		}

		print(c("eccon",eccon))
		print(c("time1",time1))
		print(c("time2",time2))
		crossMinRow=which(speed.ext$cross[,1] > time1 & speed.ext$cross[,1] < time2)
		
		isometricUse = TRUE
		#TODO: con-ecc is opposite
	
		print("at paint")
		
		if(isometricUse) {
			print("at isometricUse")
			print("speed.ext$cross")
			print(speed.ext$cross)
			print("crossMinRow")
			print(crossMinRow)
			print("speed.ext$cross[crossMinRow,1]")
			print(speed.ext$cross[crossMinRow,1])
			eccentric=1:min(speed.ext$cross[crossMinRow,1])
			isometric=min(speed.ext$cross[crossMinRow,1]+1):max(speed.ext$cross[crossMinRow,2])
			concentric=max(speed.ext$cross[crossMinRow,2]+1):length(displacement)
		} else {
			eccentric=1:mean(speed.ext$cross[crossMinRow,1])
			#isometric=mean(speed.ext$cross[crossMinRow,1]+1);mean(speed.ext$cross[crossMinRow,2])
			concentric=mean(speed.ext$cross[crossMinRow,2]+1):length(displacement)
		}

		if(draw) {
			abline(v=max(eccentric),col=cols[1])
			abline(v=min(concentric),col=cols[1])
			#mtext(text=paste(max(eccentric)," ",sep=""),side=1,at=max(eccentric),adj=1,cex=.8,col=cols[1])
			#mtext(text=paste(" ",min(concentric),sep=""),side=1,at=min(concentric),adj=0,cex=.8,col=cols[1])

			mtext(text=paste(round(min(isometric),1), " ",sep=""), 
			      side=1,at=min(isometric),adj=1,cex=.8,col=cols[1])
			mtext(text=paste(" ", round(max(isometric),1),sep=""), 
			      side=1,at=max(isometric),adj=0,cex=.8,col=cols[1])

			#don't need to show eccentric and concentric. It's pretty clear
			#mtext(text=paste(translateToPrint("eccentric")," ",sep=""),side=3,at=max(eccentric),cex=.8,adj=1,col=cols[1],line=.5)
			#mtext(text=paste(" ",translateToPrint("concentric"),sep=""),side=3,at=min(concentric),cex=.8,adj=0,col=cols[1],line=.5)
			mtext(text="ecc ",side=3,at=max(eccentric),cex=.8,adj=1,col=cols[1],line=.5)
			mtext(text=" con",side=3,at=min(concentric),cex=.8,adj=0,col=cols[1],line=.5)
		}
	}
	
	#time to arrive to max speed
	maxSpeedT=min(which(speed$y == max(speed$y)))

	maxSpeedTInConcentric = maxSpeedT
	if(eccon != "c")
		maxSpeedTInConcentric = maxSpeedT - (length(eccentric) + length(isometric))
		
	#define a propulsiveEnd value because it's used also in non-propulsive curves
	propulsiveEnd = length(displacement)

	if(isPropulsive) {
		propulsiveEnd = findPropulsiveEnd(accel$y, concentric, maxSpeedTInConcentric,
						  encoderConfigurationName, anglePush, angleWeight, 
						  massBody, massExtra, exercisePercentBodyWeight)
		if(eccon != "c")
			propulsiveEnd = length(eccentric) + length(isometric) + propulsiveEnd
	}


	print(c("propulsiveEnd at paint", propulsiveEnd))


	# ---- start of inertialECstart ----

	#on inertial ec. If the accel starts as negative, calcule avg values and peak values starting when the accel is positive
	#because the acceleration done on the disc can only be positive
	#inertialECstart = 1
	#if(length(eccentric) > 0)
	#	inertialECstart = min(eccentric)
	
	#as acceleration can oscillate, start at the eccentric part where there are not negative values
	#if(inertiaMomentum > 0 && eccon == "ec" && 
	#   length(eccentric) > 0 && min(accel$y[eccentric]) < 0) #if there is eccentric data and there are negative vlaues
	#{ 
	#	inertialECstart = max(which(accel$y[eccentric] < 0)) +1
	#	#abline(v=inertialECstart,lty=3,col="black") 
	#}
	#print(c("inertialECstart", inertialECstart))
	#print(c("accel$y[eccentric]", accel$y[eccentric]))
	#print(displacement[eccentric])

	#startX = inertialECstart #deactivated

	# ---- end of inertialECstart ----
	
	
	if(draw & showSpeed) {
		ylimHeight = max(abs(range(speed$y)))
		ylim=c(- 1.05 * ylimHeight, 1.05 * ylimHeight)	#put 0 in the middle, and have 5% margin at each side
		if(knRanges[1] != "undefined")
			ylim = knRanges$speedy
		par(new=T)
	
		speedPlot=speed$y
		#on rotatory inertial, concentric-eccentric, plot speed as ABS)
		#if(inertialType == "ri" && eccon == "ce")
		#	speedPlot=abs(speed$y)

		if(highlight==FALSE)
			plot(startX:length(speedPlot),speedPlot[startX:length(speedPlot)],type="l",
			     xlim=c(1,length(displacement)),ylim=ylim,xlab="",ylab="",col=cols[1],lty=lty[1],lwd=1,axes=F)
		else
			plot(startX:length(speedPlot),speedPlot[startX:length(speedPlot)],type="l",
			     xlim=c(1,length(displacement)),ylim=ylim,xlab="",ylab="",col="darkgreen",lty=2,lwd=3,axes=F)
		
	}
	
	if(draw & showSpeed & !superpose) {
		abline(v=maxSpeedT, col=cols[1])
		points(maxSpeedT, max(speed$y),col=cols[1])
		mtext(text=paste(round(max(speed$y),2),"m/s",sep=""),side=3,
		      at=maxSpeedT,cex=.8,col=cols[1], line=.5)
		mtext(text=maxSpeedT,side=1,at=maxSpeedT,cex=.8,col=cols[1],line=-.2)

		if(eccon != "c") {
			minSpeedT=min(which(speed$y == min(speed$y)))
			
			abline(v=minSpeedT, col=cols[1])
			points(minSpeedT, min(speed$y),col=cols[1])
			mtext(text=paste(round(min(speed$y),2),"m/s",sep=""),side=3,
			      at=minSpeedT,cex=.8,col=cols[1], line=.5)
			mtext(text=minSpeedT,side=1,at=minSpeedT,cex=.8,col=cols[1],line=-.2)
		}
	}

	


	meanSpeedC = mean(speed$y[min(concentric):max(concentric)])
	if(isPropulsive) {
		meanSpeedC = mean(speed$y[min(concentric):propulsiveEnd])
	}

	if(eccon == "c") {
		if(showSpeed) {
			arrows(x0=min(concentric),y0=meanSpeedC,x1=propulsiveEnd,y1=meanSpeedC,col=cols[1],code=3)
		}
	} else {
		meanSpeedE = mean(speed$y[startX:max(eccentric)])
		if(showSpeed) {
			arrows(x0=startX,y0=meanSpeedE,x1=max(eccentric),y1=meanSpeedE,col=cols[1],code=3)
			arrows(x0=min(concentric),y0=meanSpeedC,x1=propulsiveEnd,y1=meanSpeedC,col=cols[1],code=3)
		}
	}

	if(draw) {
		ylimHeight = max(abs(range(accel$y)))
		ylim=c(- 1.05 * ylimHeight, 1.05 * ylimHeight)	#put 0 in the middle, and have 5% margin at each side
		if(knRanges[1] != "undefined")
			ylim = knRanges$accely

		
		#always (single or side) show 0 line
		abline(h=0,lty=3,col="black")
		
		#plot the speed axis
		if(showAxes & showSpeed) {
			if(eccon == "c") {
				axis(4, at=c(min(axTicks(4)),0,max(axTicks(4)),meanSpeedC),
				     labels=c(min(axTicks(4)),0,max(axTicks(4)),
					      round(meanSpeedC,1)),
				     col=cols[1], lty=lty[1], line=axisLineRight, lwd=1, padj=-.5)
				axis(4, at=meanSpeedC,
				     labels="Xc",
				     col=cols[1], lty=lty[1], line=axisLineRight, lwd=1, padj=-2)
			}
			else {
				axis(4, at=c(min(axTicks(4)),0,max(axTicks(4)),meanSpeedE,meanSpeedC),
				     labels=c(min(axTicks(4)),0,max(axTicks(4)),
					      round(meanSpeedE,1),
					      round(meanSpeedC,1)),
				     col=cols[1], lty=lty[1], line=axisLineRight, lwd=1, padj=-.5)
				axis(4, at=c(meanSpeedE,meanSpeedC),
				     labels=labelsXeXc,
				     col=cols[1], lty=lty[1], line=axisLineRight, lwd=0, padj=-2)
			}
			axisLineRight = axisLineRight +2
		}

		if(showAccel) {
			par(new=T)
			if(highlight==FALSE)
				plot(startX:length(accel$y),accel$y[startX:length(accel$y)],type="l",
				     xlim=c(1,length(displacement)),ylim=ylim,xlab="",ylab="",col="magenta",lty=lty[2],lwd=1,axes=F)
			else
				plot(startX:length(accel$y),accel$y[startX:length(accel$y)],type="l",
				     xlim=c(1,length(displacement)),ylim=ylim,xlab="",ylab="",col="darkblue",lty=2,lwd=3,axes=F)
		}
		
		#show propulsive stuff if line if differentiation is relevant (propulsivePhase ends before the end of the movement)
		if(isPropulsive & propulsiveEnd < length(displacement)) {
			#propulsive stuff
			segments(0,-9.81,length(accel$y),-9.81,lty=3,col="magenta")
			#abline(v=propulsiveEnd,lty=3,col="magenta") 
			abline(v=propulsiveEnd,lty=1,col=cols[2]) 
			points(propulsiveEnd, -g, col="magenta")
			text(x=length(accel$y),y=-9.81,labels=" g",cex=1,adj=c(0,0),col="magenta")
		}
		
		if(showAxes & showAccel) {
			axis(4, col="magenta", lty=lty[1], line=axisLineRight, lwd=1, padj=-.5)
			axisLineRight = axisLineRight +2
		}
		#mtext(text=paste("max accel:",round(max(accel$y),3)),side=3,at=which(accel$y == max(accel$y)),cex=.8,col=cols[1],line=2)
	}

	dynamics = getDynamics(encoderConfigurationName,
			speed$y, accel$y, massBody, massExtra, exercisePercentBodyWeight, gearedDown, anglePush, angleWeight,
			displacement, diameter, inertiaMomentum, smoothing)
	mass = dynamics$mass
	force = dynamics$force
	power = dynamics$power


	if(draw && isInertial(encoderConfigurationName) && debug) 
	{
		#start blank graph with ylim of position
		ylim = yrange
		if(ylim[1] == "undefined") { 
			ylim = NULL 
			ylim = c(min(position-min(position)), max(position-min(position)))
		}
		par(new=T)
		plot(0,0,type="n",axes=F,xlab="",ylab="", ylim=ylim)
		
		abline(h=100*dynamics$loopsAblines, col="yellow") #m -> cm
		print("dynamics$loopsAblines")
		print(dynamics$loopsAblines)
		
		print("ylim")
		print(ylim)

		#TODO: add here angleSpeed graph when diameter is variable (version 1.5.3)
	}


	if(draw & showForce) {
		ylimHeight = max(abs(range(force)))
		ylim=c(- 1.05 * ylimHeight, 1.05 * ylimHeight)	#put 0 in the middle, and have 5% margin at each side
		if(knRanges[1] != "undefined")
			ylim = knRanges$force
		par(new=T)
		if(highlight==FALSE)
			plot(startX:length(force),force[startX:length(force)],type="l",
			     xlim=c(1,length(displacement)),ylim=ylim,xlab="",ylab="",col=cols[2],lty=lty[2],lwd=1,axes=F)
		else
			plot(startX:length(force),force[startX:length(force)],type="l",
			     xlim=c(1,length(displacement)),ylim=ylim,xlab="",ylab="",col="darkblue",lty=2,lwd=3,axes=F)
		if(showAxes) {
			axis(4, col=cols[2], lty=lty[2], line=axisLineRight, lwd=1, padj=-.5)
			axisLineRight = axisLineRight +2
		}
		
		if(isInertial(encoderConfigurationName) && debug) {
			#print("dynamics$forceDisc")
			#print(dynamics$forceDisc)
			par(new=T)
			plot(dynamics$forceDisc, col="blue", xlab="", ylab="", xlim=c(1,length(displacement)),ylim=ylim, type="p", pch=1, axes=F);

			par(new=T)
			plot(dynamics$forceBody, col="blue", xlab="", ylab="", xlim=c(1,length(displacement)),ylim=ylim, type="p", pch=3, axes=F);
		}
	}

	#mark when it's air and land
	#if it was a eccon concentric-eccentric, will be useful to calculate flight time
	#but this eccon will be not done
	#if(draw & (!superpose || (superpose & highlight)) & isJump) 
	if(draw & (!superpose || (superpose & highlight)) & exercisePercentBodyWeight == 100) {
		weight=mass*g
		abline(h=weight,lty=1,col=cols[2]) #body force, lower than this, person in the air (in a jump)
		text(x=length(force),y=weight,labels=paste(translateToPrint("Weight"),"(N)"),cex=.8,adj=c(.5,0),col=cols[2])

		#define like this, because if eccentric == 0, length(eccentric) == 1
		#and if eccentric is NULL, then length(eccentric) == 0, but max(eccentric) produces error
		if(length(eccentric) == 0)
			length_eccentric = 0
		else
			length_eccentric = length(eccentric)

		if(length(isometric) == 0)
			length_isometric = 0
		else
			length_isometric = length(isometric)


		#takeoff = max(which(force>=weight))
		#takeoff = min(which(force[concentric]<=weight)) + length_eccentric + length_isometric

		takeoff = -1
		if(! canJump(encoderConfigurationName) || length(which(force[concentric] <= 0)) == 0)
			takeoff = -1
		else {
			#1 get force only in concentric phase
			forceConcentric = force[concentric]
			#print(c("forceConcentric",forceConcentric))

			#2 get takeoff using maxSpeedT but relative to concentric, not all the ecc-con
			
			takeoff = findTakeOff(forceConcentric, maxSpeedTInConcentric)

			#3 add eccentric and isometric
			takeoff = takeoff + length_eccentric + length_isometric
			print(c("takeoff",takeoff))
			
			abline(v=takeoff,lty=1,col=cols[2]) 
			mtext(text=paste(translateToPrint("land")," ",sep=""),side=3,at=takeoff,cex=.8,adj=1,col=cols[2])
			mtext(text=paste(" ", translateToPrint("air"), " ",sep=""),side=3,at=takeoff,cex=.8,adj=0,col=cols[2])
		}

		if(eccon=="ec") {
			landing = -1
			#landing = min(which(force>=weight))
		
			if(length(which(force[eccentric] <= weight)) == 0)
				landing = -1
			else {
				landing = max(which(force[eccentric]<=weight))
				abline(v=landing,lty=1,col=cols[2]) 
				mtext(text=paste(translateToPrint("air")," ",sep=""),side=3,at=landing,cex=.8,adj=1,col=cols[2])
				mtext(text=paste(" ",translateToPrint("land")," ",sep=""),side=3,at=landing,cex=.8,adj=0,col=cols[2])
			}
		}

		print(c(is.numeric(takeoff), takeoff))
		if(is.numeric(takeoff) && takeoff != -1) {
			mtext(text=paste(translateToPrint("jump height"),"=", 
					 (position[concentric[length(concentric)]] - 
					  position[concentric[(takeoff - length_eccentric - length_isometric)]])/10,
					 "cm",sep=" "),
			      side=3, at=( takeoff + (length_eccentric + length(concentric)) )/2,
			      cex=.8,adj=0.5,col=cols[2])
		}
	}


	if(draw & showPower) {
		ylimHeight = max(abs(range(power)))
		ylim=c(- 1.05 * ylimHeight, 1.05 * ylimHeight)	#put 0 in the middle, and have 5% margin at each side
		if(knRanges[1] != "undefined")
			ylim = knRanges$power
		par(new=T);
		if(highlight==FALSE)
			plot(startX:length(power),power[startX:length(power)],type="l",
			     xlim=c(1,length(displacement)),ylim=ylim,xlab="",ylab="",col=cols[3],lty=lty[3],lwd=2,axes=F)
		else
			plot(startX:length(power),power[startX:length(power)],type="l",
			     xlim=c(1,length(displacement)),ylim=ylim,xlab="",ylab="",col="darkred",lty=2,lwd=3,axes=F)
	

		if(isInertial(encoderConfigurationName) && debug) {
			par(new=T)
			plot(dynamics$powerDisc, col="orangered3", xlab="", ylab="", xlim=c(1,length(displacement)),ylim=ylim, type="p", pch=1, axes=F);

			par(new=T)
			plot(dynamics$powerBody, col="orangered3", xlab="", ylab="", xlim=c(1,length(displacement)),ylim=ylim, type="p", pch=3, axes=F);
		}


		meanPowerC = mean(power[min(concentric):max(concentric)])
		if(isPropulsive) {
			meanPowerC = mean(power[min(concentric):propulsiveEnd])
		}

		if(eccon == "c") {
			arrows(x0=min(concentric),y0=meanPowerC,x1=propulsiveEnd,y1=meanPowerC,col=cols[3],code=3)
		} else {
			meanPowerE = mean(power[startX:max(eccentric)])
			arrows(x0=startX,y0=meanPowerE,x1=max(eccentric),y1=meanPowerE,col=cols[3],code=3)
			arrows(x0=min(concentric),y0=meanPowerC,x1=propulsiveEnd,y1=meanPowerC,col=cols[3],code=3)
		}

		if(showAxes) {
			if(eccon == "c") {
				axis(4, at=c(min(axTicks(4)),0,max(axTicks(4)),meanPowerC),
				     labels=c(min(axTicks(4)),0,max(axTicks(4)),
					      round(meanPowerC,1)),
				     col=cols[3], lty=lty[1], line=axisLineRight, lwd=2, padj=-.5)
				axis(4, at=meanPowerC,
				     labels="Xc",
				     col=cols[3], lty=lty[1], line=axisLineRight, lwd=2, padj=-2)
			}
			else {
				axis(4, at=c(min(axTicks(4)),0,max(axTicks(4)),meanPowerE,meanPowerC),
				     labels=c(min(axTicks(4)),0,max(axTicks(4)),
					      round(meanPowerE,1),
					      round(meanPowerC,1)),
				     col=cols[3], lty=lty[1], line=axisLineRight, lwd=1, padj=-.5)
				axis(4, at=c(meanPowerE,meanPowerC),
				     labels=labelsXeXc,
				     col=cols[3], lty=lty[1], line=axisLineRight, lwd=0, padj=-2)
			}
			axisLineRight = axisLineRight +2
		}
	}
	
	#time to arrive to peak power
	powerTemp <- power[startX:length(power)]
	peakPowerT <- min(which(power == max(powerTemp)))
	if(draw & !superpose & showPower) {
		abline(v=peakPowerT, col=cols[3])
		points(peakPowerT, max(powerTemp),col=cols[3])
		mtext(text=paste(round(max(powerTemp),1),"W",sep=""),side=3,
		      at=peakPowerT,cex=.8,col=cols[3],line=-.2)
		mtext(text=peakPowerT,side=1,at=peakPowerT,cex=.8,col=cols[3],line=.2)
		
		#don't show min power on repetitions where power it's in abs (like inertial)
		if(eccon != "c" && min(power) < 0) {
			minPowerT <- min(which(power == min(powerTemp)))

			abline(v=minPowerT, col=cols[3])
			points(minPowerT, min(powerTemp),col=cols[3])
			mtext(text=paste(round(min(powerTemp),1),"W",sep=""),side=3,
			      at=minPowerT,cex=.8,col=cols[3],line=-.2)
			mtext(text=minPowerT,side=1,at=minPowerT,cex=.8,col=cols[3],line=.2)
		}
	}
	
	#time to arrive to peak power negative on con-ecc
	if(eccon=="ce") {
		peakPowerTneg=min(which(power == min(power)))
		if(draw & !superpose) {
			abline(v=peakPowerTneg, col=cols[3])
			points(peakPowerTneg, min(power),col=cols[3])
			mtext(text=paste(round(min(power),1),"W",sep=""),side=1,line=-1,at=peakPowerTneg,adj=.5,cex=.8,col=cols[3])
			mtext(text=peakPowerTneg,side=1,at=peakPowerTneg,cex=.8,col=cols[3])
		}

		segments(peakPowerTneg,min(power),peakPowerT,min(power),lty=3,col="red")
		segments(peakPowerTneg,max(power),peakPowerT,max(power),lty=3,col="red")
		points(peakPowerTneg,(min(power) * -1),col="red")
		points(peakPowerT,(max(power) * -1),col="red")
	}

	#TODO: fix this to show only the cinematic values selected by user
	#legend, axes and title
	if(draw) {
		if(legend & showAxes) {
			legendText=c(paste(translateToPrint("Distance"),"(mm)"))
			lty=c(1)
			lwd=c(2)
			colors=c("black") 
			ncol=1

			if(showSpeed) {
				legendText=c(legendText, paste(translateToPrint("Speed"),"(m/s)"))
				lty=c(lty,1)
				lwd=c(lwd,2)
				colors=c(colors,cols[1]) 
				ncol=ncol+1
			}
			if(showAccel) {
				legendText=c(legendText, paste(translateToPrint("Accel."),"(m/s²)"))
				lty=c(lty,1)
				lwd=c(lwd,2)
				colors=c(colors,"magenta") 
				ncol=ncol+1
			}
			if(showForce) {
				legendText=c(legendText, paste(translateToPrint("Force"),"(N)"))
				lty=c(lty,1)
				lwd=c(lwd,2)
				colors=c(colors,cols[2]) 
				ncol=ncol+1
			}
			if(showPower) {
				legendText=c(legendText, paste(translateToPrint("Power"),"(W)"))
				lty=c(lty,1)
				lwd=c(lwd,2)
				colors=c(colors,cols[3]) 
				ncol=ncol+1
			}


			#plot legend on top exactly out
			#http://stackoverflow.com/a/7322792
			rng=par("usr")
			lg = legend(0,rng[2], 
				    legend=legendText, 
				    lty=lty, lwd=lwd, 
				    col=colors, 
				    cex=1, bg="white", ncol=ncol, bty="n", plot=F)
			legend(0,rng[4]+1.4*lg$rect$h, 
			       legend=legendText, 
			       lty=lty, lwd=lwd, 
			       col=colors, 
			       cex=1, bg="white", ncol=ncol, bty="n", plot=T, xpd=NA)
		}
		if(showLabels) {
			mtext(paste(translateToPrint("time"),"(ms)"),side=1,adj=1,line=-1,cex=.9)
			mtext(paste(translateToPrint("displacement"),"(mm)"),side=2,adj=1,line=-1,cex=.9)
		}
	}
}

textBox <- function(x,y,text,frontCol,bgCol,xpad=.1,ypad=1){

	w=strwidth(text)+xpad*strwidth(text)
	h=strheight(text)+ypad*strheight(text)

	rect(x-w/2,y-h/2,x+w/2,y+h/2,col=bgCol, density=60, angle=-30, border=NA)
	text(x,y,text,col=frontCol)
} 


paintPowerPeakPowerBars <- function(singleFile, title, paf, Eccon, height, n, showTTPP, showRange) 
{
	#if there's one or more inertial curves: show inertia instead of mass
	if(findInertialCurves(paf)) {
		load = paf[,findPosInPaf("Inertia","")]
		load = load * 10000
	} else
		load = paf[,findPosInPaf("Load","")]


	pafColors=c("tomato1","tomato4",topo.colors(10)[3])
	myNums = rownames(paf)
	height = abs(height/10)
	laterality = translateVector(as.vector(paf[,findPosInPaf("Laterality","")]))
	
	if(Eccon=="ecS" || Eccon=="ceS") {
		if(singleFile) {
			myEc=c("c","e")
			if(Eccon=="ceS")
				myEc=c("e","c")
			myNums = as.numeric(rownames(paf))
			myNums = paste(trunc((myNums+1)/2),myEc[((myNums%%2)+1)],sep="")
		}
	}
	
	powerData=rbind(paf[,findPosInPaf("Power","mean")], paf[,findPosInPaf("Power","max")])

	#when eccon != c show always ABS power
	#peakPower is always ABS
	if(Eccon == "c") {
		powerName = translateToPrint("Power")
		peakPowerName = translateToPrint("Peak Power")
	}
	else {
		powerName = translateToPrint("Power")
		peakPowerName = translateToPrint("Peak Power")
	}

	print("powerData")
	print(powerData)

	#put lowerY on power, but definetively, leave it at 0
	#lowerY=min(powerData)-100
	#if(lowerY < 0)
	#	lowerY = 0
	lowerY = 0
	
	marginRight = 6
	if(! showTTPP)
		marginRight = marginRight -3
	if(! showRange)
		marginRight = marginRight -3

	par(mar=c(2.5, 4, 5, marginRight))

	bpAngle = 0
	bpDensity = NULL
	if(Eccon == "ecS") {
		bpAngle = c(-45,-45,45,45)
		bpDensity = c(30,30,-1,-1) #concentric will be shown solid
	}

	bp <- barplot(powerData,beside=T,col=pafColors[1:2],width=c(1.4,.6),
			names.arg=paste(myNums," ",laterality,"\n",load,sep=""),xlim=c(1,n*3+.5),cex.name=0.8,
			xlab="",ylab=paste(translateToPrint("Power"),"(W)"), 
			ylim=c(lowerY,max(powerData)), xpd=FALSE,	#ylim, xpd = F,  makes barplot starts high (compare between them)
			angle=bpAngle, density=bpDensity
			) 
	title(main=title,line=-2,outer=T)
	box()
	
	loadWord = "Mass"
	if(findInertialCurves(paf))
		loadWord = "Inertia M."

	mtext(paste(translateToPrint("Repetition")," \n",translateToPrint(loadWord)," ",sep=""),side=1,at=1,adj=1,line=1,cex=.9)
	#mtext(translateToPrint("Laterality"),side=1,adj=1,line=0,cex=.9)

	axisLineRight=0

	#time to peak power
	if(showTTPP) {
		par(new=T, xpd=T)
		#on ecS, concentric has high value of time to peak power and eccentric has it very low. Don't draw lines
		if(Eccon=="ecS" || Eccon=="ceS")
			plot(bp[2,],paf[,findPosInPaf("Power","time")],type="p",lwd=2,
			     xlim=c(1,n*3+.5),ylim=c(0,max(paf[,findPosInPaf("Power","time")])),
			     axes=F,xlab="",ylab="",col="blue", bg="lightblue",cex=1.5,pch=21)
		else
			plot(bp[2,],paf[,findPosInPaf("Power","time")],type="b",lwd=2,
			     xlim=c(1,n*3+.5),ylim=c(0,max(paf[,findPosInPaf("Power","time")])),
			     axes=F,xlab="",ylab="",col=pafColors[3])

		axis(4, col=pafColors[3], line=axisLineRight,padj=-.5)
		mtext(paste(translateToPrint("Time to Peak Power"),"(ms)"), side=4, line=(axisLineRight-1))
		axisLineRight = axisLineRight +3
	}

	#range
	if(showRange) {	
		par(new=T)
		plot(bp[2,],height,type="p",lwd=2,xlim=c(1,n*3+.5),ylim=c(0,max(height)),axes=F,xlab="",ylab="",col="green")

		abline(h=max(height),lty=2, col="green")
		abline(h=min(height),lty=2, col="green")
		#text(max(bp[,2]),max(height),max(height),adj=c(0,.5),cex=0.8)
		axis(4, col="green", line=axisLineRight, padj=-.5)
		mtext(paste(translateToPrint("Range"),"(cm)"), side=4, line=(axisLineRight-1))
		axisLineRight = axisLineRight +3

		for(i in unique(load)) { 
			myLabel = round(mean(height[which(load == i)]),1)
			
			#text(x=mean(bp[2,which(load == i)]),
			#     y=mean(height[which(load == i)]),
			#     labels=myLabel,adj=c(.5,0),cex=.9,col="darkgreen")
			textBox(mean(bp[2,which(load == i)]),
				mean(height[which(load == i)]),
				myLabel, "darkgreen", "white", ypad=1)
			
			#print(paste("mean",i,mean(height[which(load == i)])))
			
			segments(
				 bp[2,min(which(load == i))],mean(height[which(load == i)]),
				 bp[2,max(which(load == i))],mean(height[which(load == i)]),
				 lty=1,col="green")
		}
	}
	
	legendText = c(powerName, peakPowerName)
	lty=c(0,0)
	lwd=c(1,1)
	pch=c(15,15)
	graphColors=c(pafColors[1],pafColors[2])

	if(showTTPP) {
		legendText = c(legendText, paste(translateToPrint("Time to Peak Power"),"    ",sep=""))
		lty=c(lty,1)
		lwd=c(lwd,2)
		pch=c(pch,NA)
		graphColors=c(graphColors,pafColors[3])
	}
	if(showRange) {
		legendText = c(legendText, translateToPrint("Range"))
		lty=c(lty,1)
		lwd=c(lwd,2)
		pch=c(pch,NA)
		graphColors=c(graphColors,"green")
	}

	#plot legend on top exactly out
	#http://stackoverflow.com/a/7322792
	rng=par("usr")
	lg = legend(rng[1], rng[2],
		    col=graphColors, lty=lty, lwd=lwd, pch=pch, 
		    legend=legendText, ncol=4, bty="n", plot=F)
	legend(rng[1], rng[4]+1.25*lg$rect$h,
	       col=graphColors, lty=lty, lwd=lwd, pch=pch, 
	       legend=legendText, ncol=4, bty="n", plot=T, xpd=NA)
}

#see paf for more info
findPosInPaf <- function(var, option) {
	pos = 0
	if(var == "Speed")
		pos = 1
	else if(var == "Power")
		pos = 4
	else if(var == "Force")
		pos = 8
	else if(var == "Load") #MassDisplaced
		pos = 11
	else if(var == "MassBody")
		pos = 12
	else if(var == "MassExtra")
		pos = 13
	else if(var == "Laterality")
		pos = 14
	else if(var == "Inertia")
		pos = 15
	
	if( ( var == "Speed" || var == "Power" || var == "Force") & option == "max")
		pos=pos+1
	if( ( var == "Speed" || var == "Power" || var == "Force") & option == "time")
		pos=pos+2

	return(pos)
}

#see if there's any inertial curve
#can be done by encoderConfigurationName
#but is faster just see if inertiaMoment is != than -1.
#But inertiaMoment is divided by 10000. So use the '> 0' to know when there's inertiaMoment
#
#returns TRUE if there is one or more inertial curve
findInertialCurves <- function(paf) {
	write("findInertialCurves",stderr())

	im = paf[,findPosInPaf("Inertia", "")]
	#write(im,stderr())

	if(length(im) < 1)
		return (FALSE)

	for(i in 1:length(im)) {
		#write(c("im: ", im[i]),stderr())
		if(im[i] > 0) {
			return (TRUE)
		}
	}

	return (FALSE)
}

addUnitsAndTranslate <- function (var) {
	if(var == "Speed")
		return (paste(translateToPrint("Speed"),"(m/s)"))
	else if(var == "Power")
		return (paste(translateToPrint("Power"),"(W)"))
	else if(var == "Load") #or Mass
		return (paste(translateToPrint("Mass"),"(Kg)"))
	else if(var == "Inertia")
		return (paste(translateToPrint("Inertia M."),"(Kg*cm^2)"))
	else if(var == "Force")
		return (paste(translateToPrint("Force"),"(N)"))

	return(var)
}

#if num is >= 0, plot '+'.
#else plot ''. (because the number will be displayed as negative)
#this is to avoid having a '+ -'
plotSign <- function (num) {
	if(num >= 0)
		return('+')
	else
		return('')
}

getModelPValueWithStars <- function(model) {
	#p.value = round(getModelPValue(model),6)
	p.value = getModelPValue(model)
	
	#don't plot stars if p.value is nan because there's too few data
	if(is.nan(p.value))
		return (p.value)

	stars = ""
	if(p.value <= 0.0001)
		stars = "***"
	else if(p.value <= 0.001)
		stars = "**"
	else if(p.value <= 0.05)
		stars = "*"
	else if(p.value <= 0.01)
		stars = "."
	return(paste(round.scientific(p.value), " ", stars, sep=""))
}
#http://r.789695.n4.nabble.com/extract-the-p-value-tp3933973p3934011.html
getModelPValue <- function(model) {
	stopifnot(inherits(model, "lm"))
	s <- summary.lm(model)
	pf(s$fstatistic[1L], s$fstatistic[2L], s$fstatistic[3L], lower.tail = FALSE)
}

#R returns zero on rounding if the exponent is bigger than the decimals of rounding. Eg:
#> round(0.0002,3)
#[1] 0
#> round(0.0002,4)
#[1] 2e-04
#> round(-0.0002,3)
#[1] 0
#> round(-0.0002,4)
#[1] -2e-04
round.scientific <- function(x) {
	print(c("at round.scientic",x))
	if(x == 0)
		return(0)

	negative = FALSE
	#the floor(log(10(x)) returns NaN if it's negative
	if(x < 0) {
		negative = TRUE
		x = x * -1
	}

	#http://r.789695.n4.nabble.com/Built-in-function-for-extracting-mantissa-and-exponent-of-a-numeric-td4670116.html
	e <- floor(log10(x))
	m <- x/10^e

	if(negative)
		m = m * -1

	dec = 2
	if(e == 0)
		return(round(m,dec))
	else
		return(paste(round(m,dec),"e",e,sep=""))
}

#http://stackoverflow.com/a/6234664
#see if two labels overlap
stroverlap <- function(x1,y1,s1, x2,y2,s2) {
	#print(c(x1,y1,s1, x2,y2,s2))
	sh1 <- strheight(s1)
	sw1 <- strwidth(s1)
	sh2 <- strheight(s2)
	sw2 <- strwidth(s2)

	overlap <- FALSE
	if (x1<x2) 
		overlap <- x1 + sw1 > x2
	else
		overlap <- x2 + sw2 > x1

	if (y1<y2)
		overlap <- overlap && (y1 +sh1>y2)
	else
		overlap <- overlap && (y2+sh2>y1)

	return(overlap)
}
#check all labels to see if newPoint overlaps one of them
stroverlapArray <- function(newPoint, points) {
	overlap = FALSE

	#print(c("at Array newPoint, points",newPoint, points))
	if(length(points$x) == 1)	#if there's only one row
		return (stroverlap(
			      as.numeric(newPoint[1]), as.numeric(newPoint[2]), newPoint[3],
			      as.numeric(points$x), as.numeric(points$y), points$curveNum ))
	
	#as.numeric is needed because ec-con uses "1e" in third element, and then three elements are strings

	for(i in 1:length(points$x)) {	#for every row
		overlap = stroverlap(
			      as.numeric(newPoint[1]), as.numeric(newPoint[2]), newPoint[3],
			      as.numeric(points$x[i]), as.numeric(points$y[i]), points$curveNum[i])
		if(overlap)
			return (TRUE)
	}
	return (FALSE)
}


#option: mean or max
paintCrossVariables <- function (paf, varX, varY, option, isAlone, title, singleFile, Eccon, ecconVector, seriesName, do1RM, do1RMMethod, outputData1) 
{
	#if there's one or more inertial curves: show inertia instead of mass
	if(varX == "Load" && findInertialCurves(paf))
		varX = "Inertia"

	x = (paf[,findPosInPaf(varX, option)])
	y = (paf[,findPosInPaf(varY, option)])

	if(varX == "Inertia")
		x = x * 10000

	#print("seriesName")
	#print(seriesName)

	colBalls = NULL
	bgBalls = NULL

	isPowerLoad = FALSE
	if( (varX == "Load" || varX == "Inertia") && varY == "Power" )
		isPowerLoad = TRUE

	varXut = addUnitsAndTranslate(varX)
	varYut = addUnitsAndTranslate(varY)
	
	#nums.print.df = NULL
	nums.print = NULL

	#if only one series
	if(length(unique(seriesName)) == 1) {
		cexBalls = 1.8
		cexNums = 1
		adjHor = 0
		colBalls="blue"
		bgBalls="lightBlue"
		if(isAlone == "RIGHT") {
			colBalls="red"
			bgBalls="pink"
		}
	
		pchVector = createPchVector(ecconVector)

		laterality = as.vector(paf[,findPosInPaf("Laterality","")])
		#bgBallsVector = rep(bgBalls, length(x))	
		#bgBallsVector[laterality=="L"] <- "red"
		#bgBallsVector[laterality=="R"] <- "blue"
		#plot(x,y, xlab=varXut, ylab="", pch=pchVector, col=colBalls,bg=bgBallsVector,cex=cexBalls,axes=F)
		
		plot(x,y, xlab=varXut, ylab="", pch=pchVector, col=colBalls,bg=bgBalls,cex=cexBalls,axes=F)
		points(x[laterality=="L"], y[laterality=="L"], type="p", cex=1, col=colBalls, pch=3) # font=5, pch=220) #172, 220 don't looks good
		points(x[laterality=="R"], y[laterality=="R"], type="p", cex=1, col=colBalls, pch=4) # font=5, pch=222) #174, 222 don't looks good
		

		for(i in 1:length(x)) {
			name = i
			if( ( Eccon=="ecS" || Eccon=="ceS" ) && singleFile) {
				#name = paste(trunc((name+1)/2),ecconVector[i],sep="")
				name = trunc((name+1)/2) #don't show e,c whe show the pch
			} else {
				#name = paste(name,ecconVector[i],sep="")
				#don't show e,c, we show the pch
			}
			
			newPoint = data.frame(x=x[i], y=y[i], curveNum=name)
			
			if(i == 1) {
				nums.print = data.frame()
				nums.print = rbind(nums.print, newPoint)
				colnames(nums.print) = c("x","y","curveNum")
			} else {
				overlaps = FALSE
				if( ! ( is.na(x[i]) && is.na(y[i]) ) )
					overlaps = stroverlapArray(newPoint, nums.print)
				if(! overlaps) {
					nums.print = rbind(nums.print, newPoint)
				}
			}
		}


		if(do1RM != FALSE & do1RM != "0") {	
			speed1RM = as.numeric(do1RM)

			print("speed1RM")
			print(speed1RM)
			
			#lineal stuff
			fit = lm(y ~ x) #declare
			if(do1RMMethod == "NONWEIGHTED")  {
				#without weights
				fit = lm(y ~ x)
			} else if(do1RMMethod == "WEIGHTED")  {
				#weights x
				fit = lm(y ~ x, weights=x/max(x)) 
				print(x/max(x))
			} else if(do1RMMethod == "WEIGHTED2")  {
				#weights x^2
				fit = lm(y ~ x, weights=x^2/max(x^2)) 
				print(x^2/max(x^2))
			} else if(do1RMMethod == "WEIGHTED3")  {
				#weights x^3 (as higher then more important are the right values) 
				fit = lm(y ~ x, weights=x^3/max(x^3)) 
				print(x^3/max(x^3))
			}

			c.intercept = coef(fit)[[1]]
			c.x = coef(fit)[[2]]

			if(is.na(c.x)) {
				plot(0,0,type="n",axes=F,xlab="",ylab="")
				text(x=0,y=0,translateToPrint("Not enough data."),cex=1.5)
				dev.off()
				write("1RM;-1", SpecialData)
				write("", outputData1)
				quit()
			}

			load1RM = ( speed1RM - c.intercept ) / c.x

			maxX=max(x)
			if(load1RM > maxX)
				maxX=load1RM
			plot(x,y, xlim=c(min(x),maxX), ylim=c(0, max(y)), xlab=varXut, ylab="", pch=21,col=colBalls,bg=bgBalls,cex=cexBalls,axes=F)
			abline(fit,col="red")
			abline(h=speed1RM,col="gray",lty=2)
			abline(v=load1RM,col="gray",lty=2)
			mtext("1RM", at=load1RM, side=1, line=2,col="red")
			mtext(round(load1RM,2), at=load1RM, side=1, line=3,col="red")
			mtext("1RM", at=speed1RM, side=2, line=2,col="red")
			mtext(speed1RM, at=speed1RM, side=2, line=3,col="red")
			points(load1RM,speed1RM,cex=2,col="red")

			write(paste("1RM;",round(load1RM,2),sep=""), SpecialData)

			#quadratic stuff
			#fit2 = lm(y ~ I(x^2) + x)
			#fit2line = predict(fit2, data.frame(x = 10:100))
			#lines(10:100 ,fit2line, col="red") #puts line on plot
		}
		else {
			if(length(unique(x)) >= 3) {
				fit = lm(y ~ x + I(x^2))
		
				coef.a <- fit$coefficient[3]
				coef.b <- fit$coefficient[2]
				coef.c <- fit$coefficient[1]

				x1 <- seq(min(x),max(x), (max(x) - min(x))/1000)
				y1 <- coef.a *x1^2 + coef.b * x1 + coef.c
				lines(x1,y1)

				#start plot the function expression, R^2 and p
				varXplot = varX
				if(varXplot == "Load")
					varXplot = "Mass"

				#for Speed,Power graph
				functionAt = max(x)
				functionAdj = 1
				if(isAlone == "LEFT") {
					functionAt = min(x)
					functionAdj = 0
				}

				mtext(paste(
					    varYut, " = ", 
					    round.scientific(coef.a), " * ", varXplot, "^2 ", plotSign(coef.b), " ",  
					    round.scientific(coef.b), " * ", varXplot, " ", plotSign(coef.c), " ", 
					    round.scientific(coef.c), sep=""), side=3, line=1, at=functionAt, adj=functionAdj, cex = .9)
				mtext(paste(
					    "R^2 = ", round(summary(fit)$r.squared,4),
					    "; R^2 (adjusted) = ", round(summary(fit)$adj.r.squared,4),
					    "; p = ", getModelPValueWithStars(fit)
					    , sep=""), side =3, line=0, at=functionAt, adj=functionAdj, cex=.9)
				#end of plot the function expression, R^2 and p
	
				if(isPowerLoad) {
					#xmax <-  -b / 2a
					xmax <- - coef.b / (2 * coef.a)

					#pmax <- ax^2 +bx +c
					pmax <- xmax^2 * coef.a + xmax * coef.b + coef.c

					abline(v=xmax,lty=3)
					points(xmax, pmax, pch=1, cex=3)

					#this check is to not have title overlaps on 'speed,power / load' graph
					if(title != "")
					   title = paste(title, " (pmax = ", round(pmax,1), " W with ", round(xmax,1), " Kg)", sep="")
				}
			}
		}
		
		text(as.numeric(nums.print$x), as.numeric(nums.print$y), paste("  ", nums.print$curveNum), adj=c(adjHor,.5), cex=cexNums)

		#don't write title two times on 'speed,power / load'
		if(isAlone == "ALONE" || isAlone =="RIGHT")
			title(title, cex.main=1, font.main=2, line=3)

		#don't write legend on 'speed,power / load' because it doesn't fits with the formulas and regressions
		if(isAlone == "ALONE") {
			#show legend
			legendText = c(translateToPrint("concentric"),
				       translateToPrint("eccentric"),
				       paste(translateToPrint("eccentric"),translateToPrint("concentric"),sep="-"),
				       translateToPrint("L"),
				       translateToPrint("R")
				       )
			rng=par("usr")
			lg = legend(rng[1],rng[4], 
				    legend=legendText, pch=c(24,25,21,3,4), col="black", pt.bg="white",
				    cex=1, ncol=2, bty="n",
				    plot=F)
			legend(rng[1],rng[4]+1*lg$rect$h, 
			       legend=legendText, pch=c(24,25,21,3,4),  col="black",  pt.bg="white",
			       cex=1, bg=bgBalls, ncol=2, bty="n",
			       plot=T, xpd=NA)
		}

	} else { #more than one series
		#colBalls = "black"
		uniqueColors=topo.colors(length(unique(seriesName)))

		#in x axis move a little every series to right in order to compare
		seqX = seq(0,length(unique(seriesName))-1,by=1)-(length(unique(seriesName))-1)/2

		plot(x,y, xlab=varXut, ylab="", type="n", axes=F)
		for(i in 1:length(seriesName)) {
			thisSerie = which(seriesName == unique(seriesName)[i])
			colBalls[thisSerie] = uniqueColors[i]
			#in x axis move a little every series to right in order to compare
			x[thisSerie] = x[thisSerie] + (seqX[i]/5)
		}
		
		points(x,y, pch=19, col=colBalls, cex=1.8)
		
		for(i in 1:length(seriesName)) {
			thisSerie = which(seriesName == unique(seriesName)[i])
			if(length(unique(x[thisSerie])) >= 4)
				lines(smooth.spline(x[thisSerie],y[thisSerie],df=4),col=uniqueColors[i],lwd=2)
		}
	
		#difficult to create a title in series graphs
		title(paste(varXut,"/",varYut), cex.main=1, font.main=2)
			
		#plot legend on top exactly out
		#http://stackoverflow.com/a/7322792
		rng=par("usr")
		lg = legend(rng[1],rng[4], 
			    legend=unique(seriesName), lty=1, lwd=2, col=uniqueColors, 
			    cex=1, bg="white", ncol=length(unique(seriesName)), bty="n",
			    plot=F)
		legend(rng[2]-1.25*lg$rect$w,rng[4]+1.25*lg$rect$h, 
		       legend=unique(seriesName), lty=1, lwd=2, col=uniqueColors, 
		       cex=1, bg="white", ncol=6, bty="n",
		       plot=T, xpd=NA)
	}
		
	if(isAlone == "ALONE") {
		axis(1)
		axis(2)
		mtext(varYut, side=2, line=3)
		#box()
	} else if(isAlone == "LEFT") {
		axis(1)
		axis(2,col=colBalls)
		mtext(varYut, side=2, line=3, col=colBalls)
		#box()
	} else { #"RIGHT"
		axis(4,col=colBalls)
		mtext(varYut, side=4, line=3, col=colBalls)
	}
	box()

}

#propulsive!!!!
paint1RMBadillo2010 <- function (paf, title, outputData1) {
	curvesLoad = (paf[,findPosInPaf("Load","")]) 		#mass: X
	curvesSpeed = (paf[,findPosInPaf("Speed", "mean")])	#mean speed Y

	par(mar=c(5,6,3,4))

	loadPercent <- seq(30,100, by=5)

	#msp: mean speed propulsive
	msp <- c(1.33, 1.235, 1.145, 1.055, 0.965, 0.88, 0.795,
		                          0.715, 0.635, 0.555, 0.475, 0.405, 0.325, 0.255, 0.185)
	#variation <- c(0.08, 0.07, 0.06, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.04, 0.04, 0.04, 0.04, 0.03, 0.04)

	maxy=max(c(msp,curvesSpeed))
	miny=min(c(msp,curvesSpeed))


	loadPercentCalc=8.4326*curvesSpeed^2 - 73.501*curvesSpeed + 112.33
	#sometimes there's a negative value, fix it
	for(i in 1:length(loadPercentCalc))
	       if(loadPercentCalc[i] < 0)
		       loadPercentCalc[i] = NA

	loadCalc= 100 * curvesLoad / loadPercentCalc

	#for calculations take only the curves slower or == than 1.33
	curvesSpeedInIntervalPos = which(curvesSpeed <= max(msp))

	if(length(curvesSpeedInIntervalPos) == 0) {
		plot(0,0,type="n",axes=F,xlab="",ylab="")
		text(x=0,y=0,translateToPrint("Not enough data."),cex=1.5)
		dev.off()
		write("1RM;-1", SpecialData)
		write("", outputData1)
		quit()
	}

	par(mar=c(6,5,3,4))

	plot(curvesLoad,curvesSpeed, type="p",
	     main=paste(title, "1RM", translateToPrint("prediction")),
	     sub=paste("\n",translateToPrint("Concentric mean speed on bench press 1RM =")," 0.185m/s.",
		      translateToPrint("Estimated percentual load ="),
		      " 8.4326 * ", translateToPrint("speed"), " ^2 - 73.501 * ", translateToPrint("speed"), " + 112.33\n",
		      translateToPrint("Adapted from")," Gonzalez-Badillo, Sanchez-Medina (2010)"),
	     xlim=c(min(curvesLoad),max(loadCalc[curvesSpeedInIntervalPos])),
	     ylim=c(miny,maxy), xlab="", ylab="",axes=T)

	mtext(side=1,line=2,"Kg")
	mtext(side=2,line=3,paste(translateToPrint("Mean speed in concentric propulsive phase"),"(m/s)"))
	mtext(side=4,line=2,"1RM (%)")

	abline(h=msp, lty=2, col="gray")
	mtext(side=4,at=msp, paste(" ",loadPercent), las=2)

	colors=c(rep(NA,29),rev(heat.colors(100)[0:71]))
	arrows(curvesLoad,curvesSpeed,loadCalc,0.185,code=2,col=colors[loadPercentCalc])

	closerValues = which(curvesLoad == max(curvesLoad))
	segments(loadCalc[closerValues],0.185,loadCalc[closerValues],0,lty=3)

	predicted1RM = mean(loadCalc[closerValues])

	segments(predicted1RM,0.185,predicted1RM,0,lty=1)
	mtext(side=1, at=predicted1RM, round(predicted1RM,2), cex=.8)
			
	write(paste("1RM;",round(predicted1RM,2),sep=""), SpecialData)
}

			
find.mfrow <- function(n) {
	if(n<=3) return(c(1,n))
	else if(n<=8) return(c(2,ceiling(n/2)))
	else return(c(3, ceiling(n/3)))
}

find.yrange <- function(singleFile, displacement, curves) {
	n=length(curves[,1])
	y.max = 0
	y.min = 10000
	for(i in 1:n) { 
		y.current = cumsum(displacement[curves[i,1]:curves[i,2]])
		if(max(y.current) > y.max)
			y.max = max(y.current)
		if(min(y.current) < y.min)
			y.min = min(y.current)
	}
	if(y.min < 0) {
		y.max = y.max + -1*y.min
		y.min = 0
	}
	return (c(y.min,y.max))
}
			
#create ecconLine vector that will write "e" or "c" at each position
createEcconVector <- function(singleFile, Eccon, curvesNum, ecconVectorNotSingleFile)
{
	ecconVector = NULL
	if(singleFile) {
		if(Eccon == "c" || Eccon == "ec" || Eccon == "ce")
			ecconVector = rep(Eccon,curvesNum) #will do ("c","c","c", ...) or ("ec","ec","ec",...)
		else if(Eccon == "ecS") {
			ecconVector = rep(c("e","c"),trunc(curvesNum/2))
			if(curvesNum%%2 == 1)
				ecconVector = c(ecconVector,"e")
		}
		else if(Eccon == "ceS") {
			ecconVector = rep(c("c","e"),trunc(curvesNum/2))
			if(curvesNum%%2 == 1)
				ecconVector = c(ecconVector,"c")
		}
	} else {
		ecconVector = ecconVectorNotSingleFile
	}

	return (ecconVector)
}
createPchVector <- function(ecconVector) {
	pchVector = ecconVector
	pchVector[pchVector == "ec"] <- "21"
	pchVector[pchVector == "ce"] <- "21"
	pchVector[pchVector == "e"] <- "25"
	pchVector[pchVector == "c"] <- "24"

	return (as.numeric(pchVector))
}

#-------------------- EncoderConfiguration conversions --------------------------




#-------------- end of EncoderConfiguration conversions -------------------------

quitIfNoData <- function(n, curves, outputData1) {
	#if not found curves with this data, plot a "sorry" message and exit
	if(n == 1 & curves[1,1] == 0 & curves[1,2] == 0) {
		plot(0,0,type="n",axes=F,xlab="",ylab="")
		text(x=0,y=0,translateToPrint("Sorry, no curves matched your criteria."),cex=1.5)
		dev.off()
		write("", outputData1)
		quit()
	}
}

loadLibraries <- function(os) {
	#library("EMD")
	#library("sfsmisc")
	if(os=="Windows")
		library("Cairo")
}

doProcess <- function(options) 
{
	op <- assignOptions(options)
	
	print(c("1 Title=",op$Title))
	#unicoded titles arrive here like this "\\", convert to "\", as this is difficult, do like this:
	#http://stackoverflow.com/a/17787736
	op$Title=parse(text = paste0("'", op$Title, "'"))
	print(c("1 Title=",op$Title))
	
	#options 30 and 31 is assigned on the top of the file to be available in all functions
	#print(options[30])
	#print(options[31])
	#options 32 is this graph.R file and it's used in call_graph.R to call this as source (in RDotNet)

	#--- include files ---
	#if(op$scriptOne != "none")
	#	source(op$scriptOne)
	if(op$scriptTwo != "none")
		source(op$scriptTwo)


	print(op$File)
	print(op$OutputGraph)
	print(op$OutputData1)
	print(op$OutputData2)
	print(op$SpecialData)

	#read AnalysisOptions
	#if is propulsive and rotatory inertial is: "p;ri" 
	#if nothing: "-;-"
	analysisOptionsTemp = unlist(strsplit(op$AnalysisOptions, "\\;"))
	isPropulsive = (analysisOptionsTemp[1] == "p")
	inertialType = ""	#TODO: use EncoderConfiguration
	if(length(analysisOptionsTemp) > 1) {
		inertialType = analysisOptionsTemp[2] #values: "" || "li" || "ri"
	}


	#inertial cannot be propulsive
	if(isInertial(op$EncoderConfigurationName))
		isPropulsive = FALSE

	#in "li": linear encoder with inertial machines,
        #it's recommended to attach string to the rolling axis
	#because then we have the information of the machine.
	#If we attach the string to the body of the person, it's wrong because:
        #1 there's a loose time (without tension) where person moves independent of rolling machine
	#2 (more important) person changes direction on the top, and this is BIG a change of speed and acceleration, 
	#	but machine is rolling at same direction and speed.
	#	Measuring what person does there is giving incorrect high values of power
	#	because that acceleration of the body is not related to any change of movement of the inertial machine 
	#3 Also, linear encoder on the body has another problem:
	#	the force of the disc to the body to make it go down when the body is in the top,
	#	is a force that contributes greatly on the change of direction,
	#	then this force has to be added to the gravity in the power calculation
	#	This is not calculated yet.

	
	if(op$Analysis != "exportCSV") {
		if(op$OperatingSystem=="Windows")
			Cairo(op$Width, op$Height, file = op$OutputGraph, type="png", bg="white")
		else
			png(op$OutputGraph, width=op$Width, height=op$Height)

		op$Title=gsub('_',' ',op$Title)
		op$Title=gsub('-','    ',op$Title)
	}

	titleType = "c"
	#if(isJump)
	#	titleType="jump"

	curvesPlot = FALSE
	if(op$Analysis == "curves") {
		curvesPlot = TRUE
		par(mar=c(2,2.5,2,1))
	}

	#when a csv is used (it links to lot of files) then singleFile = false
	singleFile = TRUE
	if(nchar(op$File) >= 40) {
		#file="/tmp...../chronojump-encoder-graph-input-multi.csv"
		#substr(file, nchar(file)-39, nchar(file))
		#[1] "chronojump-encoder-graph-input-multi.csv"
		if(substr(op$File, nchar(op$File)-39, nchar(op$File)) == "chronojump-encoder-graph-input-multi.csv") {
			singleFile = FALSE
		}
	}
	
	#declare here
	SmoothingsEC = 0


	if(! singleFile) {	#reads CSV with curves to analyze
		#this produces a displacement, but note that a position = cumsum(displacement) cannot be done because:
		#this are separated movements
		#maybe all are concentric (there's no returning to 0 phase)

		#this version of curves has added specific data cols:
		#status, exerciseName, mass, smoothingOne, dateTime, myEccon

		inputMultiData=read.csv(file=op$File,sep=",",stringsAsFactors=F)

		displacement = NULL
		count = 1
		start = NULL; end = NULL; startH = NULL
		status = NULL; id = NULL; exerciseName = NULL; massBody = NULL; massExtra = NULL
		dateTime = NULL; myEccon = NULL; curvesHeight = NULL
		seriesName = NULL; percentBodyWeight = NULL;

		#encoderConfiguration
		econfName = NULL; econfd = NULL; econfD = NULL; econfAnglePush = NULL; econfAngleWeight = NULL; 
		econfInertia = NULL; econfGearedDown = NULL;
		laterality = NULL;

		newLines=0;
		countLines=1; #useful to know the correct ids of active curves
		
		#in neuromuscular, when sending individual repetititions,
		#if all are concentric, csv has a header but not datarows
		#it will be better to do not allow to plot graph from Chronojump,
		#but meanwhile we can check like this:
		if(length(inputMultiData[,1]) == 0) {
			plot(0,0,type="n",axes=F,xlab="",ylab="")
			text(x=0,y=0,translateToPrint("Not enough data."),
			     cex=1.5)
			dev.off()
			write("", op$OutputData1)
			quit()
		}
			
		for(i in 1:length(inputMultiData[,1])) { 
			#plot only active curves
			status = as.vector(inputMultiData$status[i])

			if(status != "active") {
				newLines=newLines-1; 
				countLines=countLines+1;
				next;
			}
			
			dataTempFile=scan(file=as.vector(inputMultiData$fullURL[i]),sep=",")

			#if curves file ends with comma. Last character will be an NA. remove it
			#this removes all NAs on a curve
			dataTempFile  = dataTempFile[!is.na(dataTempFile)]

			if(isInertial(inputMultiData$econfName[i])) {
				dataTempFile = getDisplacementInertial(
								       dataTempFile, inputMultiData$econfName[i], 
								       inputMultiData$econfd[i], inputMultiData$econfD[i])
				#getDisplacementInertialBody is not needed because it's done on curve save
			} else {
				dataTempFile = getDisplacement(inputMultiData$econfName[i], dataTempFile, op$diameter, op$diameterExt)
			}


			dataTempPhase=dataTempFile
			processTimes = 1
			changePos = 0
			#if this curve is ecc-con and we want separated, divide the curve in two
			if(as.vector(inputMultiData$eccon[i]) != "c" & (op$Eccon=="ecS" || op$Eccon=="ceS") ) {
				changePos = mean(which(cumsum(dataTempFile) == min(cumsum(dataTempFile))))
				processTimes = 2
			}
			for(j in 1:processTimes) {
				if(processTimes == 2) {
					if(j == 1) {
						dataTempPhase=dataTempFile[1:changePos]
					} else {
						#IMP: 
						#note that following line without the parentheses on changePos+1
						#gives different data.
						#never forget parentheses to operate inside the brackets
						dataTempPhase=dataTempFile[(changePos+1):length(dataTempFile)]
						newLines=newLines+1
					}
				}
				displacement = c(displacement, dataTempPhase)
				id[(i+newLines)] = countLines
				start[(i+newLines)] = count
				end[(i+newLines)] = length(dataTempPhase) + count -1
				startH[(i+newLines)] = 0
				exerciseName[(i+newLines)] = as.vector(inputMultiData$exerciseName[i])

				#mass[(i+newLines)] = inputMultiData$mass[i]
				massBody[(i+newLines)] = inputMultiData$massBody[i]
				massExtra[(i+newLines)] = inputMultiData$massExtra[i]

				dateTime[(i+newLines)] = as.vector(inputMultiData$dateTime[i])
				percentBodyWeight[(i+newLines)] = as.vector(inputMultiData$percentBodyWeight[i])
				
				#also encoder configuration stuff
				econfName[(i+newLines)] = inputMultiData$econfName[i]
				econfd[(i+newLines)] = inputMultiData$econfd[i]
				econfD[(i+newLines)] = inputMultiData$econfD[i]
				econfAnglePush[(i+newLines)] = inputMultiData$econfAnglePush[i]
				econfAngleWeight[(i+newLines)] = inputMultiData$econfAngleWeight[i]
				econfInertia[(i+newLines)] = inputMultiData$econfInertia[i]/10000.0 #comes in Kg*cm^2 eg: 100; convert it to Kg*m^2 eg: 0.010
				econfGearedDown[(i+newLines)] = inputMultiData$econfGearedDown[i]

				curvesHeight[(i+newLines)] = sum(dataTempPhase)

				if(processTimes == 2) {
					if(j == 1) {
						myEccon[(i+newLines)] = "e"
						id[(i+newLines)] = paste(countLines, myEccon[(i+newLines)], sep="")
					} else {
						myEccon[(i+newLines)] = "c"
						id[(i+newLines)] = paste(countLines, myEccon[(i+newLines)], sep="")
						countLines = countLines + 1
					}
				} else {
					if(inputMultiData$eccon[i] == "c")
						myEccon[(i+newLines)] = "c"
					else
						myEccon[(i+newLines)] = "ec"
					countLines = countLines + 1
				}
				
				seriesName[(i+newLines)] = as.vector(inputMultiData$seriesName[i])
				laterality[(i+newLines)] = as.vector(inputMultiData$laterality[i])

				count = count + length(dataTempPhase)
			}
		}		

		#position=cumsum(displacement)

		#curves = data.frame(id,start,end,startH,exerciseName,mass,dateTime,myEccon,stringsAsFactors=F,row.names=1)
		#this is a problem when there's only one row as seen by the R code of data.frame. ?data.frame:
		#"If row names are supplied of length one and the data frame has a
		#single row, the ‘row.names’ is taken to specify the row names and
		#not a column (by name or number)."
		#then a column id is created when there's only on row, but it is not created there's more than one.
		#solution:
		if(length(id)==1) {
			curves = data.frame(start,end,startH,exerciseName,massBody,massExtra,
					    dateTime,myEccon,seriesName,percentBodyWeight,
					    econfName,econfd,econfD,econfAnglePush,econfAngleWeight,econfInertia,econfGearedDown,
					    laterality,
					    stringsAsFactors=F,row.names=id)
		} else {
			curves = data.frame(id,start,end,startH,exerciseName,massBody,massExtra,
					    dateTime,myEccon,seriesName,percentBodyWeight,
					    econfName,econfd,econfD,econfAnglePush,econfAngleWeight,econfInertia,econfGearedDown,
					    laterality,
					    stringsAsFactors=F,row.names=1)
		}


		n=length(curves[,1])
		quitIfNoData(n, curves, op$OutputData1)
		
		#print(curves, stderr())
	
		#find SmoothingsEC
		SmoothingsEC = findSmoothingsEC(singleFile, displacement, curves, op$Eccon, op$SmoothingOneC)
	} else {	#singleFile == True. reads a signal file
		displacement=scan(file=op$File,sep=",")
		#if data file ends with comma. Last character will be an NA. remove it
		#this removes all NAs
		displacement  = displacement[!is.na(displacement)]

		if(isInertial(op$EncoderConfigurationName)) 
		{
			diametersPerMs = getInertialDiametersPerMs(displacement, op$diameter)
			displacement = getDisplacementInertial(displacement, op$EncoderConfigurationName, diametersPerMs, op$diameterExt)
		
			displacement = getDisplacementInertialBody(0, displacement, curvesPlot, op$Title)
			#positionStart is 0 in graph.R. It is different on capture.R because depends on the start of every repetition

			curvesPlot = FALSE
		} else {
			displacement = getDisplacement(op$EncoderConfigurationName, displacement, op$diameter, op$diameterExt)
		}

		#TODO: is this needed at all?
		if(length(displacement)==0) {
			plot(0,0,type="n",axes=F,xlab="",ylab="")
			text(x=0,y=0,translateToPrint("Encoder is not connected."),cex=1.5)
			dev.off()
			write("", op$OutputData1)
			quit()
		}
			
		position=cumsum(displacement)

		#print(c("position",position))
		#print(c("displacement",displacement))
		
		#curves=findCurvesOld(displacement, op$Eccon, op$MinHeight, curvesPlot, op$Title)
		curves=findCurvesNew(displacement, op$Eccon, isInertial(op$EncoderConfigurationName), 
				     op$MinHeight, curvesPlot, op$Title)
		
		if(op$Analysis == "curves")
			curvesPlot = TRUE

		n=length(curves[,1])
		quitIfNoData(n, curves, op$OutputData1)
		
		print("curves before reduceCurveBySpeed")
		print(curves)
	
		#reduceCurveBySpeed, don't do in inertial because it doesn't do a good right adjust on changing phase
		#what reduceCurveBySpeed is doing in inertial is adding a value at right, and this value is a descending value
		#and this produces a high acceleration there
		if( ! isInertial(op$EncoderConfigurationName)) {
			for(i in 1:n) {
				reduceTemp=reduceCurveBySpeed(op$Eccon, i, 
							      curves[i,1], curves[i,3], #startT, startH
							      displacement[curves[i,1]:curves[i,2]], #displacement
							      op$SmoothingOneC
							      )
				curves[i,1] = reduceTemp[1]
				curves[i,2] = reduceTemp[2]
				curves[i,3] = reduceTemp[3]
			}
		}
		
		print("curves after reduceCurveBySpeed")
		print(curves)
		
		#find SmoothingsEC
		SmoothingsEC = findSmoothingsEC(singleFile, displacement, curves, op$Eccon, op$SmoothingOneC)
		print(c("SmoothingsEC:",SmoothingsEC))
		
		if(curvesPlot) {
			#/10 mm -> cm
			for(i in 1:length(curves[,1])) { 
				myLabel = i
				myY = min(position)/10
				adjVert = 0
				if(op$Eccon=="ceS")
					adjVert = 1

				if(op$Eccon=="ecS" || op$Eccon=="ceS") {
					myEc=c("c","e")
					if(op$Eccon=="ceS")
						myEc=c("e","c")
					
					myLabel = paste(trunc((i+1)/2),myEc[((i%%2)+1)],sep="")
					myY = position[curves[i,1]]/10
					if(i%%2 == 1) {
						adjVert = 1
						if(op$Eccon=="ceS")
							adjVert = 0
					}
				}
				text(x=((curves[i,1]+curves[i,2])/2/1000),	#/1000 ms -> s
				     y=myY,labels=myLabel, adj=c(0.5,adjVert),cex=.9,col="blue")
				arrows(x0=(curves[i,1]/1000),y0=myY,x1=(curves[i,2]/1000),	#/1000 ms -> s
				       y1=myY, col="blue",code=0,length=0.1)
				#mtext(at=((curves[i,1]+curves[i,2])/2/1000),	#/1000 ms -> s
				#     side=1,text=myLabel, cex=.8, col="blue")
				abline(v=c(curves[i,1],curves[i,2])/1000, lty=3, col="gray")
			}
		

			#plot speed
			par(new=T)	
			
			smoothingAll= 0.1
			speed <- getSpeed(displacement, smoothingAll)
			plot((1:length(displacement))/1000, speed$y, col="green2",
		     		type="l", 
				xlim=c(1,length(displacement))/1000,	#ms -> s
				#ylim=c(-.25,.25),		#to test speed at small changes
		     		xlab="",ylab="",axes=F)
			
			if(isInertial(op$EncoderConfigurationName))
				mtext(translateToPrint("body speed"),side=4,adj=1,line=-1,col="green2",cex=.8)
			else
				mtext(translateToPrint("speed"),side=4,adj=1,line=-1,col="green2")

			abline(h=0,lty=2,col="gray")
		}
	}

	
	write("starting analysis...", stderr())

	#make some check here, because this file is being readed in chronojump

	#write(paste("(4/5)",translateToPrint("Repetitions processed")), op$OutputData2)
	print("Creating (op$OutputData2)4.txt with touch method...")
	file.create(paste(op$OutputData2,"4.txt",sep=""))
	print("Created")
	#print(curves)

	if(op$Analysis=="single") {
		if(op$Jump>0) {
			myMassBody = op$MassBody
			myMassExtra = op$MassExtra
			myEccon = op$Eccon
			myStart = curves[op$Jump,1]
			myEnd = curves[op$Jump,2]
			myExPercentBodyWeight = op$ExercisePercentBodyWeight
			
			#encoderConfiguration
			myEncoderConfigurationName = op$EncoderConfigurationName
			myDiameter = op$diameter
			myDiameterExt = op$diameterExt
			myAnglePush = op$anglePush
			myAngleWeight = op$angleWeight
			myInertiaMomentum = op$inertiaMomentum
			myGearedDown = op$gearedDown
			myLaterality = ""
			if(! singleFile) {
				myMassBody = curves[op$Jump,5]
				myMassExtra = curves[op$Jump,6]
				myEccon = curves[op$Jump,8]
				myExPercentBodyWeight = curves[op$Jump,10]

				#encoderConfiguration
				myEncoderConfigurationName = curves[op$Jump,11]
				myDiameter = curves[op$Jump,12]
				myDiameterExt = curves[op$Jump,13]
				myAnglePush = curves[op$Jump,14]
				myAngleWeight = curves[op$Jump,15]
				myInertiaMomentum = curves[op$Jump,16]
				myGearedDown = curves[op$Jump,17]
				myLaterality = curves[op$Jump,18]
			}
			myCurveStr = paste(translateToPrint("Repetition"),"=", op$Jump, " ", myLaterality, " ", myMassExtra, "Kg", sep="")
		
			#don't do this, because on inertial machines string will be rolled to machine and not connected to the body
			#if(inertialType == "li") {
			#	displacement[myStart:myEnd] = fixRawdataLI(displacement[myStart:myEnd])
			#	myEccon="c"
			#}

			#find which SmoothingsEC is needed
			smoothingPos <- 1
			if(op$Jump %in% rownames(curves))
				smoothingPos <- which(rownames(curves) == op$Jump)
		
			
			paint(displacement, myEccon, myStart, myEnd,"undefined","undefined",FALSE,FALSE,
			      1,curves[op$Jump,3],SmoothingsEC[smoothingPos],op$SmoothingOneC,myMassBody,myMassExtra,
			      myEncoderConfigurationName,myDiameter,myDiameterExt,myAnglePush,myAngleWeight,myInertiaMomentum,myGearedDown,
			      paste(op$Title, " ", op$Analysis, " ", myEccon, ". ", myCurveStr, sep=""),
			      "", #subtitle
			      TRUE,	#draw
			      TRUE,	#showLabels
			      FALSE,	#marShrink
			      TRUE,	#showAxes
			      TRUE,	#legend
			      op$Analysis, isPropulsive, inertialType, myExPercentBodyWeight,
			      (op$AnalysisVariables[1] == "Speed"), #show speed
			      (op$AnalysisVariables[2] == "Accel"), #show accel
			      (op$AnalysisVariables[3] == "Force"), #show force
			      (op$AnalysisVariables[4] == "Power")  #show power
			      )	
		}
	}

	if(op$Analysis=="side") {
		#comparar 6 salts, falta que xlim i ylim sigui el mateix
		par(mfrow=find.mfrow(n))

		yrange=find.yrange(singleFile, displacement, curves)

		#if !singleFile kinematicRanges takes the 'curves' values
		knRanges=kinematicRanges(singleFile, displacement, curves, 
					 op$MassBody, op$MassExtra, op$ExercisePercentBodyWeight, 
			    		 op$EncoderConfigurationName,op$diameter,op$diameterExt,op$anglePush,op$angleWeight,op$inertiaMomentum,op$gearedDown,
					 SmoothingsEC, op$SmoothingOneC, 
					 g, op$Eccon, isPropulsive)

		for(i in 1:n) {
			myMassBody = op$MassBody
			myMassExtra = op$MassExtra
			myEccon = op$Eccon
			myExPercentBodyWeight = op$ExercisePercentBodyWeight
			
			#encoderConfiguration
			myEncoderConfigurationName = op$EncoderConfigurationName
			myDiameter = op$diameter
			myDiameterExt = op$diameterExt
			myAnglePush = op$anglePush
			myAngleWeight = op$angleWeight
			myInertiaMomentum = op$inertiaMomentum
			myGearedDown = op$gearedDown
			myLaterality = ""
			if(! singleFile) {
				myMassBody = curves[i,5]
				myMassExtra = curves[i,6]
				myEccon = curves[i,8]
				myExPercentBodyWeight = curves[i,10]

				#encoderConfiguration
				myEncoderConfigurationName = curves[i,11]
				myDiameter = curves[i,12]
				myDiameterExt = curves[i,13]
				myAnglePush = curves[i,14]
				myAngleWeight = curves[i,15]
				myInertiaMomentum = curves[i,16]
				myGearedDown = curves[i,17]
				myLaterality = curves[i,18]
			}

			myTitle = ""
			if(i == 1)
				myTitle = paste(op$Title)
			
			mySubtitle = paste("curve=", rownames(curves)[i], ", ", myLaterality, " ", myMassExtra, "Kg", sep="")

			paint(displacement, myEccon, curves[i,1],curves[i,2],yrange,knRanges,FALSE,FALSE,
			      1,curves[i,3],SmoothingsEC[i],op$SmoothingOneC,myMassBody,myMassExtra,
			      myEncoderConfigurationName,myDiameter,myDiameterExt,myAnglePush,myAngleWeight,myInertiaMomentum,myGearedDown,
			      myTitle,mySubtitle,
			      TRUE,	#draw
			      FALSE,	#showLabels
			      TRUE,	#marShrink
			      FALSE,	#showAxes
			      FALSE,	#legend
			      op$Analysis, isPropulsive, inertialType, myExPercentBodyWeight,
			      (op$AnalysisVariables[1] == "Speed"), #show speed
			      (op$AnalysisVariables[2] == "Accel"), #show accel
			      (op$AnalysisVariables[3] == "Force"), #show force
			      (op$AnalysisVariables[4] == "Power")  #show power
			      )
		}
		par(mfrow=c(1,1))
	}
#	if(op$Analysis=="superpose") {	#TODO: fix on ec startH
#		#falta fer un graf amb les 6 curves sobreposades i les curves de potencia (per exemple) sobrepossades
#		#fer que acabin al mateix punt encara que no iniciin en el mateix
#		#arreglar que els eixos de l'esq han de seguir un ylim,
#		#pero els de la dreta un altre, basat en el que es vol observar
#		#fer que es pugui enviar colors que es vol per cada curva, o linetypes
#		wide=max(curves$end-curves$start)
#
#		#position=cumsum(displacement)
#		#yrange=c(min(position),max(position))
#		yrange=find.yrange(singleFile, displacement,curves)
#
#		knRanges=kinematicRanges(singleFile,displacement,curves,Mass,SmoothingOneEC,SmoothingOneC,g,Eccon,isPropulsive)
#		for(i in 1:n) {
#			#in superpose all jumps end at max height
#			#start can change, some are longer than other
#			#xmin and xmax should be the same for all in terms of X concordance
#			#but line maybe don't start on the absolute left
#			#this is controled by startX
#			startX = curves[i,1]-(curves[i,2]-wide)+1;
#			myTitle = "";
#			if(i==1)
#				myTitle = paste(titleType,Jump);
#
#			paint(displacement, Eccon, curves[i,2]-wide,curves[i,2],yrange,knRanges,TRUE,(i==Jump),
#			      startX,curves[i,3],SmoothingOneEC,SmoothingOneC,Mass,myTitle,"",
#			      TRUE,	#draw
#			      TRUE,	#showLabels
#			      FALSE,	#marShrink
#			      (i==1),	#showAxes
#			      TRUE,	#legend
#			      op$Analysis, isPropulsive, inertialType, op$ExercisePercentBodyWeight 
#			      )
#			par(new=T)
#		}
#		par(new=F)
#		#print(knRanges)
#	}

	#since Chronojump 1.3.6, encoder analyze has a treeview that can show the curves
	#when an analysis is done, curves file has to be written
	writeCurves = TRUE
	#but don't writeCurves on exportCSV because outputfile is the same 
	if(op$Analysis == "exportCSV")
		writeCurves = FALSE

	if(
	   op$Analysis == "powerBars" || op$Analysis == "cross" || 
	   op$Analysis == "1RMBadillo2010" || op$Analysis == "1RMAnyExercise" || 
	   op$Analysis == "curves" || op$Analysis == "neuromuscularProfile" ||
	   writeCurves) 
	{
		paf = data.frame()
		discardedCurves = NULL
		discardingCurves = FALSE
		for(i in 1:n) { 
			myMassBody = op$MassBody
			myMassExtra = op$MassExtra
			myEccon = op$Eccon
			myExPercentBodyWeight = op$ExercisePercentBodyWeight
			
			#encoderConfiguration
			myEncoderConfigurationName = op$EncoderConfigurationName
			myDiameter = op$diameter
			myDiameterExt = op$diameterExt
			myAnglePush = op$anglePush
			myAngleWeight = op$angleWeight
			myInertiaMomentum = op$inertiaMomentum
			myGearedDown = op$gearedDown
			myLaterality = ""
			if(! singleFile) {
				myMassBody = curves[i,5]
				myMassExtra = curves[i,6]
				myEccon = curves[i,8]
				myExPercentBodyWeight = curves[i,10]

				#encoderConfiguration
				myEncoderConfigurationName = curves[i,11]
				myDiameter = curves[i,12]
				myDiameterExt = curves[i,13]
				myAnglePush = curves[i,14]
				myAngleWeight = curves[i,15]
				myInertiaMomentum = curves[i,16]
				myGearedDown = curves[i,17]
				myLaterality = curves[i,18]

				#only use concentric data	
				if( (op$Analysis == "1RMBadillo2010" || op$Analysis == "1RMAnyExercise") & myEccon == "e") {
					discardedCurves = c(i,discardedCurves)
					discardingCurves = TRUE
					next;
				}
			} else {
				if( (op$Analysis == "1RMBadillo2010" || op$Analysis == "1RMAnyExercise") & op$Eccon == "ecS" & i%%2 == 1) {
					discardedCurves = c(i,discardedCurves)
					discardingCurves = TRUE
					next;
				}
				else if( (op$Analysis == "1RMBadillo2010" || op$Analysis == "1RMAnyExercise") & op$Eccon == "ceS" & i%%2 == 0) {
					discardedCurves = c(i,discardedCurves)
					discardingCurves = TRUE
					next;
				}
			}

			#print(c("i, curves[i,1], curves[i,2]", i, curves[i,1],curves[i,2]))

			#if ecS go kinematics first time with "e" and second with "c"
			myEcconKn = myEccon
			if(myEccon=="ecS") {
			       if(i%%2 == 1)
				       myEcconKn = "e"
			       else
				       myEcconKn = "c"
			}
			else if(op$Eccon=="ceS") {
			       if(i%%2 == 1)
				       myEcconKn = "c"
			       else
				       myEcconKn = "e"
			}
			print("start integer before kinematicsF __________")
			print(c(curves[i,1], displacement[curves[i,1]]))

			paf = rbind(paf,(pafGenerate(
						     myEccon,
						     kinematicsF(displacement[curves[i,1]:curves[i,2]], 
								 myMassBody, myMassExtra, myExPercentBodyWeight,
								 myEncoderConfigurationName,myDiameter,myDiameterExt,myAnglePush,myAngleWeight,
								 myInertiaMomentum,myGearedDown,
								 SmoothingsEC[i],op$SmoothingOneC, 
								 g, myEcconKn, isPropulsive),
						     myMassBody, myMassExtra, myLaterality, myInertiaMomentum
						     )))
		}

		#on 1RMBadillo discard curves "e", because paf has this curves discarded
		#and produces error on the cbinds below			
		if(discardingCurves)
			curves = curves[-discardedCurves,]

		rownames(paf)=rownames(curves)
		
		if(op$Analysis == "powerBars") {
			if(! singleFile) 
				paintPowerPeakPowerBars(singleFile, op$Title, paf, 
							op$Eccon,	 			#Eccon
							curvesHeight,			#height 
							n, 
			      				(op$AnalysisVariables[1] == "TimeToPeakPower"), 	#show time to pp
			      				(op$AnalysisVariables[2] == "Range") 		#show range
							)		
			else 
				paintPowerPeakPowerBars(singleFile, op$Title, paf, 
							op$Eccon,					#Eccon
							position[curves[,2]]-curves[,3], 	#height
							n, 
			      				(op$AnalysisVariables[1] == "TimeToPeakPower"), 	#show time to pp
			      				(op$AnalysisVariables[2] == "Range") 		#show range
							) 
		}
		else if(op$Analysis == "cross") {
			mySeries = "1"
			if(! singleFile)
				mySeries = curves[,9]

			ecconVector = createEcconVector(singleFile, op$Eccon, length(curves[,1]), curves[,8])

			#print("AnalysisVariables:")
			#print(op$AnalysisVariables[1])
			#print(op$AnalysisVariables[2])
			#print(op$AnalysisVariables[3])

			if(op$AnalysisVariables[1] == "Speed,Power") {
				par(mar=c(5,4,5,5))
				analysisVertVars = unlist(strsplit(op$AnalysisVariables[1], "\\,"))
				paintCrossVariables(paf, op$AnalysisVariables[2], analysisVertVars[1], 
						    op$AnalysisVariables[3], "LEFT", "",
						    singleFile,
						    op$Eccon,
						    ecconVector,
						    mySeries, 
						    FALSE, FALSE, op$OutputData1) 
				par(new=T)
				paintCrossVariables(paf, op$AnalysisVariables[2], analysisVertVars[2], 
						    op$AnalysisVariables[3], "RIGHT", op$Title,
						    singleFile,
						    op$Eccon,
						    ecconVector,
						    mySeries, 
						    FALSE, FALSE, op$OutputData1) 
			} else {
				par(mar=c(5,4,5,2))
				paintCrossVariables(paf, op$AnalysisVariables[2], op$AnalysisVariables[1], 
						    op$AnalysisVariables[3], "ALONE", op$Title,
						    singleFile,
						    op$Eccon,
						    ecconVector,
						    mySeries, 
						    FALSE, FALSE, op$OutputData1) 
			}
		}
		else if(op$Analysis == "1RMAnyExercise") {
			mySeries = "1"
			if(! singleFile)
				mySeries = curves[,9]
			
			ecconVector = createEcconVector(singleFile, op$Eccon, length(curves[,1]), curves[,8])

			paintCrossVariables(paf, "Load", "Speed", 
					    "mean", "ALONE", op$Title,
					    singleFile,
					    op$Eccon,
					    ecconVector,
					    mySeries, 
					    op$AnalysisVariables[1], op$AnalysisVariables[2], #speed1RM, method
					    op$OutputData1) 
		}
		else if(op$Analysis == "1RMBadillo2010") {
			paint1RMBadillo2010(paf, op$Title, op$OutputData1)
		} 
		else if(op$Analysis == "neuromuscularProfile") {
			#only signal, it's a jump, use mass of the body (100%) + mass Extra if any

			npj <- neuromuscularProfileGetData(singleFile, displacement, curves, (op$MassBody + op$MassExtra), op$SmoothingOneC)

			if(is.double(npj) && npj == -1) {
				plot(0,0,type="n",axes=F,xlab="",ylab="")
				text(x=0,y=0,paste(translateToPrint("Not enough data."), "\n",
						   translateToPrint("Need at least three jumps")),
						   cex=1.5)
				dev.off()
				write("", op$OutputData1)
				quit()
			}
					    
			np.bar.load <- mean(c(
					      npj[[1]]$e1$rfd.avg,
					      npj[[2]]$e1$rfd.avg,
					      npj[[3]]$e1$rfd.avg
					      ))
			np.bar.explode <- mean(c(
						 npj[[1]]$c$cl.rfd.avg,
						 npj[[2]]$c$cl.rfd.avg,
						 npj[[3]]$c$cl.rfd.avg
						 ))
			np.bar.drive <- mean(c(
					       npj[[1]]$c$cl.i,
					       npj[[2]]$c$cl.i,
					       npj[[3]]$c$cl.i
					       ))

			par(mar=c(3,4,2,4))
			par(mfrow=c(2,1))
			neuromuscularProfilePlotBars(op$Title, np.bar.load, np.bar.explode, np.bar.drive)
			
			par(mar=c(4,4,1,4))

			neuromuscularProfilePlotOther(
						      displacement, #curves,
						      list(npj[[1]]$l.context, npj[[2]]$l.context, npj[[3]]$l.context),
						      list(npj[[1]]$mass, npj[[2]]$mass, npj[[3]]$mass),
						      op$SmoothingOneC)

			#TODO: calcular un SmothingOneECE i passar-lo a PlotOther enlloc del SmoothingOneC
			par(mfrow=c(1,1))


			#don't write the curves, write npj
			writeCurves = FALSE

			neuromuscularProfileWriteData(npj, op$OutputData1)
		}
		
		if(op$Analysis == "curves" || writeCurves) {
			#this columns are going to be removed from paf:

			#write("paf and pafCurves", stderr())
			#write(paf$meanSpeed, stderr())
			pafCurves <- subset( paf, select = -c(mass, massBody, massExtra, inertiaMomentum) )
			#write(pafCurves$meanSpeed, stderr())


			if(singleFile)
				pafCurves = cbind(
					  "1",			#seriesName
					  "exerciseName",
					  op$MassBody,
					  op$MassExtra,
					  curves[,1],
					  curves[,2]-curves[,1],position[curves[,2]]-curves[,3],pafCurves)
			else {
				if(discardingCurves)
					curvesHeight = curvesHeight[-discardedCurves]

				pafCurves = cbind(
					  curves[,9],		#seriesName
					  curves[,4],		#exerciseName
					  curves[,5],		#massBody
					  curves[,6],		#massExtra
					  curves[,1],		
					  curves[,2]-curves[,1],curvesHeight,pafCurves)

			}

			colnames(pafCurves) = c("series","exercise","massBody","massExtra",
					"start","width","height",
					"meanSpeed","maxSpeed","maxSpeedT",
					"meanPower","peakPower","peakPowerT",
					"pp_ppt",
					"meanForce", "maxForce", "maxForceT",
					"laterality"
					)
		

			#Add "AVG" and "SD" when analyzing, not on "curves"
			if(op$Analysis != "curves") {
				addSD = FALSE
				if(length(pafCurves[,1]) > 1)
					addSD = TRUE

				if(typeof(pafCurves) == "list") {
					#1) AVG
					pafCurves = rbind(pafCurves, 
							  c("","", mean(pafCurves$massBody), mean(pafCurves$massExtra),
							    mean(pafCurves$start),mean(pafCurves$width),mean(pafCurves$height),
							    mean(pafCurves$meanSpeed),mean(pafCurves$maxSpeed),mean(pafCurves$maxSpeedT),
							    mean(pafCurves$meanPower),mean(pafCurves$peakPower),mean(pafCurves$peakPowerT),
							    mean(pafCurves$pp_ppt),
							    mean(pafCurves$meanForce), mean(pafCurves$maxForce), mean(pafCurves$maxForceT),
							    ""
							    )
							  )
					rownames(pafCurves)[length(pafCurves[,1])] = "AVG"

					#2) Add SD if there's more than one data row.
					if(addSD) {	
						pafCurves = rbind(pafCurves, 
								  c("","", sd(pafCurves$massBody), sd(pafCurves$massExtra),
								    sd(pafCurves$start),sd(pafCurves$width),sd(pafCurves$height),
								    sd(pafCurves$meanSpeed),sd(pafCurves$maxSpeed),sd(pafCurves$maxSpeedT),
								    sd(pafCurves$meanPower),sd(pafCurves$peakPower),sd(pafCurves$peakPowerT),
								    sd(pafCurves$pp_ppt),
								    sd(pafCurves$meanForce), sd(pafCurves$maxForce), sd(pafCurves$maxForceT),
								    ""
								    )
								  )
						rownames(pafCurves)[length(pafCurves[,1])] = "SD"
					}
				}
			}

			print(pafCurves)

			write.csv(pafCurves, op$OutputData1, quote=FALSE)
			#print("curves written")
		}
	}
	if(op$Analysis=="exportCSV") {
		print("Starting export...")
		write("starting export", stderr())

		curvesNum = length(curves[,1])

		maxLength = 0
		for(i in 1:curvesNum) { 
			myLength = curves[i,2]-curves[i,1]
			if(myLength > maxLength)
				maxLength=myLength
		}

		curveCols = 6	#change this value if there are more colums
		names=c("DIST.", "DIST. +", "SPEED", "ACCEL.", "FORCE", "POWER")
		nums=1:curvesNum
		nums=rep(nums,each=curveCols)		
		namesNums=paste(names, nums)
		units=c("\n(mm)", "\n(mm)", "\n(m/s)", "\n(m/s^2)", "\n(N)", "\n(W)")
		namesNums=paste(namesNums, units)

		for(i in 1:curvesNum) { 
			kn = kinematicsF (displacement[curves[i,1]:curves[i,2]], 
					  op$MassBody, op$MassExtra, op$ExercisePercentBodyWeight,
					  op$EncoderConfigurationName,op$diameter,op$diameterExt,op$anglePush,op$angleWeight,op$inertiaMomentum,op$gearedDown,
					  SmoothingsEC[i], op$SmoothingOneC, g, op$Eccon, isPropulsive)

			#fill with NAs in order to have the same length
			col1 = displacement[curves[i,1]:curves[i,2]]
			col2 = position[curves[i,1]:curves[i,2]]

			#add mean, max, and time to max
			col1=append(col1,
				    c(NA,NA,NA,NA),
				    after=0)
			col2=append(col2,
				    c(NA,NA,NA,range(col2)[2]-range(col2)[1]),
				    after=0)
			kn$speedy=append(kn$speedy,
					 c(
					   mean(abs(kn$speedy)),
					   max(kn$speedy),
					   (min(which(kn$speedy == max(kn$speedy)))/1000),
					   NA),
					 after=0)
			kn$accely=append(kn$accely,
					 c(
					   mean(abs(kn$accely)),
					   max(kn$accely),
					   NA,
					   NA),
					 after=0)
			kn$force=append(kn$force,
					c(
					  mean(abs(kn$force)),
					  max(kn$force),
					  NA,
					  NA),
					after=0)
			kn$power=append(kn$power,
					c(
					  mean(abs(kn$power)),
					  max(kn$power),
					  (min(which(kn$power == max(kn$power)))/1000),
					  NA),
					after=0)

			extraRows=4
			length(col1)=maxLength+extraRows
			length(col2)=maxLength+extraRows
			length(kn$speedy)=maxLength+extraRows
			length(kn$accely)=maxLength+extraRows
			length(kn$force)=maxLength+extraRows
			length(kn$power)=maxLength+extraRows

			if(i==1)
				df=data.frame(cbind(col1, col2,
						    kn$speedy, kn$accely, kn$force, kn$power))
			else
				df=data.frame(cbind(df, col1, col2,
						    kn$speedy, kn$accely, kn$force, kn$power))
		}

		rownames(df) = c("MEAN", "MAX", "TIME TO MAX", "RANGE", 1:maxLength)
		colnames(df) = namesNums

		#TODO: time
		#TODO: tenir en compte el startH

		#op$Title=gsub('_',' ',Top$itle)
		#print(op$Title)
		#titleColumns=unlist(strsplit(op$Title,'-'))
		#colnames(df)=c(titleColumns[1]," ", titleColumns[2],titleColumns[3],rep(" ",(curvesNum*curveCols-4)))

		if(op$DecimalSeparator == "COMMA")
			write.csv2(df, file = op$OutputData1, row.names=T, na="")
		else
			write.csv(df, file = op$OutputData1, row.names=T, na="")

		print("Export done.")
	}
	if(op$Analysis != "exportCSV")
		dev.off()

	#make some check here, because this file is being readed in chronojump
	#write(paste("(5/5)",translateToPrint("R tasks done")), op$OutputData2)
	print("Creating (op$OutputData2)5.txt with touch method...")
	file.create(paste(op$OutputData2,"5.txt",sep=""))
	print("Created")
	write("created ...5.txt", stderr())

	warnings()
}


