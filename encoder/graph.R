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
#   Copyright (C) 2004-2014  	Xavier de Blas <xaviblas@gmail.com> 
#   Copyright (C) 2014   	Xavier Padullés <x.padulles@gmail.com>
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

#concentric, eccentric-concentric, repetitions of eccentric-concentric
#currently only used "c" and "ec". no need of ec-rep because c and ec are repetitive
#"ecS" is like ec but eccentric and concentric phases are separated, used in findCurves, this is good for treeview to know power... on the 2 phases
eccons=c("c","ec","ecS","ce","ceS") 

g = 9.81

smoothingAll= 0.1

colSpeed="springgreen3"; colForce="blue2"; colPower="tomato2"	#colors
#colSpeed="black"; colForce="black"; colPower="black"		#black & white
cols=c(colSpeed,colForce,colPower); lty=rep(1,3)	


#--- user commands ---
#way A. passing options to a file
getOptionsFromFile <- function(optionsFile) {
	optionsCon <- file(optionsFile, 'r')
	options=readLines(optionsCon,n=27)
	close(optionsCon)
	return (options)
}

#way B. put options as arguments
#unused because maybe command line gets too long
#options <- commandArgs(TRUE)


args <- commandArgs(TRUE)
optionsFile =args[1]

print(optionsFile)

options=getOptionsFromFile(optionsFile)

print(options)

OutputData2 = options[4] #currently used to display processing feedback
SpecialData = options[5]
OperatingSystem=options[27]
EncoderConfigurationName = ""

write("(1/5) Starting R", OutputData2)

#extrema function is part of EMD package
#It's included here to save time, because 'library("EMD")' is quite time consuming
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

	list(minindex=minindex, maxindex=maxindex, nextreme=nextreme, cross=cross, ncross=ncross)
}    


#comes with every jump of the three best (in flight time)
neuromuscularProfileForceTimeGetVariables <- function(displacement, e1TimeStart, cTimeStart, e2TimeStart, weight)
{

	#          /\
	#         /  \ 
	# _     c/    \e2
	#  \    /      \
	# e1\  /        \
	#    \/          \

	

	#----------------
	#1.- e1 variables
	#----------------

	#e1Range = range of e1

	#e1f (when force is done)
	#from max(abs(speed$y)) at e1, to end of e1

	#e1ft duration of e1f

	#e1fFmax = max Force on e1f

	#e1fRFDavg
	#average force on e1f / e1ft

	#e1fI (Impulse)
	#average force on e1f * e1ft / weight

	#----------------
	#2.- c variables
	#----------------

	#c1l "land" from bottom to takeoff (force < weight)
	#c1a "air" from takeoff to max height
	#c1 = c1l + c1a

	#c1aRange
	#flight phase on concentric

	#c1lt = contact time on c1l
	
	#c1lRFDavg = average force on c1l / c1lt / weight
	#c1lImpulse = average force on c1l * c1lt / weight

	#c1lFavg = average force on c1l / weight

	#c1lvF (vF -> valley Force)
	#minimum force on c1l before de concentric Speed max

	#c1lFmax = max force at right of valley


	#c1lSavg = avg Speed on c1l
	#c1lPavg = avg Power on c1l
	#c1lSmax = max Speed on c1l
	#c1lPmax = max Power on c1l


	#----------------
	#3.- e2 variables
	#----------------

	#e2f (when force is done)
	#is the same as contact phase (land on eccentric)
	
	#e2ft duration of e2f

	#e2fFmax = max force on e2f

	#e2fFmaxt = duration from land to max force

	#e2fRFDmax = e2fFmax / e2fFmaxT



	#return an object, yes, object oriented, please
}

#Manuel Lapuente analysis of 6 separate ABKs (e1, c, e2)
neuromuscularProfileForceTimeDoAnalysis <- function(displacement, weight)
{
	#get the maxheight of the 6 jumps
	#with the best three jumps (in jump height) do:

	#neuromuscularProfileForceTimeGetVariables <- function(displacement, e1TimeStart cTimeStart, e2TimeStart, weight)

	#show avg of each three values
	#plot a graph with these averages

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

#separate phases using initial height of full extended person
#this methods replaces getDisplacement and fixRawdataInertial
#here comes a signal: (singleFile)
#it shows the disc rotation and the person movement
getDisplacementInertial <- function(displacement, draw, title) 
{
	position=cumsum(displacement)
	position.ext=extrema(position)

	print("at findCurvesInertial")
	print(position.ext)

	#Fix if disc goes wrong direction at start
	if(position.ext$maxindex[1] < position.ext$minindex[1]) {
		displacement = displacement * -1
		position=cumsum(displacement)
		position.ext=extrema(position)
	}
	
	firstDownPhaseTime = position.ext$minindex[1]

	downHeight = abs(position[1] - position[firstDownPhaseTime])
		
	positionPerson = abs(cumsum(displacement))*-1
	#this is to make "inverted cumsum"
	displacementPerson = c(0,diff(positionPerson))
	
	if(draw) {
		col="black"
		plot((1:length(position))/1000			#ms -> s
		     ,position/10,				#mm -> cm
		     type="l",
		     xlim=c(1,length(position))/1000,		#ms -> s
		     xlab="",ylab="",axes=T,
		     lty=2,col=col) 

		abline(h=0, lty=2, col="gray")
	
		lines((1:length(position))/1000,positionPerson/10,lty=1,lwd=2)
		
		title(title, cex.main=1, font.main=1)
		mtext("time (s) ",side=1,adj=1,line=-1)
		mtext("height (cm) ",side=2,adj=1,line=-1)
	}
	return(displacementPerson)
}

findCurves <- function(displacement, eccon, min_height, draw, title) {
	position=cumsum(displacement)
	position.ext=extrema(position)
	print("at findCurves")
	print(position.ext)
	
	start=0; end=0; startH=0
	tempStart=0; tempEnd=0;
	#TODO: fer algo per si no es detecta el minindex previ al salt
	if(eccon=="c") {
		if(length(position.ext$minindex)==0) { position.ext$minindex=cbind(1,1) }
		if(length(position.ext$maxindex)==0) { position.ext$maxindex=cbind(length(position),length(position)) }
		#fixes if 1st minindex is after 1st maxindex
		if(position.ext$maxindex[1] < position.ext$minindex[1]) { position.ext$minindex = rbind(c(1,1),position.ext$minindex) } 
		row=1; i=1; j=1
		while(max(c(i,j)) <= min(c(length(position.ext$minindex[,1]),length(position.ext$maxindex[,1])))) {

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
			
			height=position[tempEnd]-position[tempStart]
			if(height >= min_height) { 
				start[row] = tempStart
				end[row]   = tempEnd
				startH[row]= position[position.ext$minindex[i,1]]		#height at start
				row=row+1;
#				if(eccon=="c") { break } #c only needs one result
			} 
			i=i+1; j=j+1
		}
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
		mtext("time (s) ",side=1,adj=1,line=-1)
		mtext("height (cm) ",side=2,adj=1,line=-1)
	}
	return(as.data.frame(cbind(start,end,startH)))
}



#called on "ec" and "ce" to have a smoothingOneEC for every curve
#this smoothingOneEC has produce same speeds than smoothing "c"
findSmoothingsEC <- function(displacement, curves, eccon, smoothingOneC) {
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

			concentric=displacement[(curves[i,1]+start):(curves[i,1]+end)]

			#get max speed at "c"
			speed <- getSpeed(concentric, smoothingOneC)
			maxSpeedC=max(speed$y)

			#find max speed at "ec" that's similar to maxSpeedC
			smoothingOneEC = smoothingOneC
			for(j in seq(as.numeric(smoothingOneC),0,by=-.01)) {
				speed <- getSpeed(eccentric.concentric, j)
				smoothingOneEC = j
				maxSpeedEC=max(speed$y)
				print(c("j",j,"maxC",round(maxSpeedC,3),"maxEC",round(maxSpeedEC,3)))
				if(maxSpeedEC >= maxSpeedC)
					break
			}

			#use smoothingOneEC
			smoothings[i] = smoothingOneEC
			
			print(smoothings[i])
		}
	}
		
	return(smoothings)
}

#used in alls eccons
reduceCurveBySpeed <- function(eccon, row, startT, displacement, smoothingOneC) 
{
	print("at reduceCurveBySpeed")

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
	} else {
		speed$y=abs(speed$y)
		time1 = min(which(speed$y == max(speed$y)))
		time2 = max(which(speed$y == max(speed$y)))
	}

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

	#debug
	print(speed.ext$cross[,2])
	#print(ext.cross.len)
	print(c("time1,time2",time1,time2))
	print(c("x.ini x.end",x.ini,x.end))

	return(c(startT + x.ini, startT + x.end))
}

findECPhases <- function(displacement,speed) {
	speed.ext=extrema(speed)
	#print(speed.ext)
	#print(speed)
	
	#In all the extrema minindex values, search which range (row) has the min values,
	#and in this range search last value
	print("searchMinSpeedEnd")
	searchMinSpeedEnd = max(which(speed == min(speed)))
	print(searchMinSpeedEnd)
	
	#In all the extrema maxindex values, search which range (row) has the max values,
	#and in this range search first value
	print("searchMaxSpeedIni")
	searchMaxSpeedIni = min(which(speed == max(speed)))
	print(searchMaxSpeedIni)
	
	#find the cross between both
	print("speed.ext-Cross")
	print(speed.ext$cross[,1])
	print("search min cross: crossMinRow")
	crossMinRow=which(speed.ext$cross[,1] > searchMinSpeedEnd & speed.ext$cross[,1] < searchMaxSpeedIni)
	print(crossMinRow)
			
	#if (length(crossMinRow) > 0) {
	#	print(crossMinRow)
	#} else {
	#	propulsiveEnd = length(displacement)
	#	errorSearching = TRUE
	#}
	
	eccentric = 0
	isometric = 0
	concentric = 0
				
	isometricUse = TRUE
	if(isometricUse) {
		eccentric=1:min(speed.ext$cross[crossMinRow,1])
		isometric=c(min(speed.ext$cross[crossMinRow,1]), max(speed.ext$cross[crossMinRow,2]))
		concentric=max(speed.ext$cross[crossMinRow,2]):length(displacement)
	} else {
		eccentric=1:mean(speed.ext$cross[crossMinRow,1])
		isometric=c(mean(speed.ext$cross[crossMinRow,1]), mean(speed.ext$cross[crossMinRow,2]))
		concentric=mean(speed.ext$cross[crossMinRow,2]):length(displacement)
	}
	return(list(
		eccentric=eccentric,
		isometric=isometric,
		concentric=concentric))
}

#TODO: this can have problems if there's an initial brake when lifting and this goes under -9.8
#better use extrema, and if there's more than one minindex:
#take the last minindex and it's previous maxindex
#go from that maxindex to the minindex and on the first moment that goes under -9.8 assign propulsiveEnd there
#Also use more this funcion (eg on paint)
findPropulsiveEnd <- function(accel, concentric) {
	if(length(which(accel[concentric]<=-g)) > 0) 
		propulsiveEnd = min(concentric) + min(which(accel[concentric] <= -g))
	else
		propulsiveEnd = max(concentric)
	
return (propulsiveEnd)
}

#go here with every single curve
#eccon="c" one time each curve
#eccon="ec" one time each curve
#eccon="ecS" means ecSeparated. two times each curve: one for "e", one for "c"
kinematicsF <- function(displacement, massBody, massExtra, exercisePercentBodyWeight,
			encoderConfigurationName,diameter,diameterExt,anglePush,angleWeight,inertiaMomentum,gearedDown,
			smoothingOneEC, smoothingOneC, g, eccon, isPropulsive) {

	smoothing = 0
	if(eccon == "c" || eccon == "e")
		smoothing = smoothingOneC
	else
		smoothing = smoothingOneEC

print(c(" smoothing:",smoothing))

	#x vector should contain at least 4 different values
	if(length(displacement) >= 4)
		speed <- getSpeed(displacement, smoothing)
	else
		speed=list(y=rep(0,length(displacement)))
	
	if(length(displacement) >= 4)
		accel <- getAcceleration(speed)
	else
		accel=list(y=rep(0,length(displacement)))

print(c(" ms",round(mean(speed$y),5)," ma",round(mean(accel$y),5)))
print(c(" Ms",round(max(speed$y),5)," Ma",round(max(accel$y),5)))
print(c(" |ms|",round(mean(abs(speed$y)),5)," |ma|:",round(mean(abs(accel$y)),5)))
print(c(" |Ms|",round(max(abs(speed$y)),5)," |Ma|",round(max(abs(accel$y)),5)))

	#speed comes in mm/ms when derivate to accel its mm/ms^2 to convert it to m/s^2 need to *1000 because it's quadratic
	accel$y <- accel$y * 1000 
	errorSearching = FALSE

	concentric = 0
	propulsiveEnd = 0

#print("at kinematicsF eccon==")
#print(eccon)

	#search propulsiveEnd
	if(isPropulsive) {
		if(eccon=="c") {
			concentric=1:length(displacement)
			propulsiveEnd = findPropulsiveEnd(accel$y,concentric)
		} else if(eccon=="ec") {
			phases=findECPhases(displacement,speed$y)
			eccentric = phases$eccentric
			isometric = phases$isometric
			concentric = phases$concentric
			propulsiveEnd = findPropulsiveEnd(accel$y,concentric)
		} else if(eccon=="e") {
			#not eccon="e" because not propulsive calculations on eccentric
		} else { #ecS
			print("WARNING ECS\n\n\n\n\n")
		}
	}

	dynamics = getDynamics(encoderConfigurationName,
			speed$y, accel$y, massBody, massExtra, exercisePercentBodyWeight, gearedDown, anglePush, angleWeight,
			displacement, diameter, diameterExt, inertiaMomentum, smoothing)
	mass = dynamics$mass
	force = dynamics$force
	power = dynamics$power


	if( isPropulsive && ( eccon== "c" || eccon == "ec" ) )
		return(list(speedy=speed$y[1:propulsiveEnd], accely=accel$y[1:propulsiveEnd], 
			    force=force[1:propulsiveEnd], power=power[1:propulsiveEnd], mass=mass))
	else
		return(list(speedy=speed$y, accely=accel$y, force=force, power=power, mass=mass))
}

pafGenerate <- function(eccon, kinematics, massBody, massExtra) {
	#print("speed$y")
	#print(kinematics$speedy)

	meanSpeed <- mean(kinematics$speedy)
	#max speed and max speed time can be at eccentric or concentric
	maxSpeed <- max(abs(kinematics$speedy))
	maxSpeedT <- min(which(abs(kinematics$speedy) == maxSpeed))

	if(eccon == "c")
		meanPower <- mean(kinematics$power)
	else
		meanPower <- mean(abs(kinematics$power))

	#print(c("eccon meanPowerSigned meanPowerABS",eccon, mean(kinematics$power), mean(abs(kinematics$power))))
	#print("kinematics$power")
	#print(abs(kinematics$power))

	peakPower <- max(abs(kinematics$power))
	peakPowerT <- min(which(abs(kinematics$power) == peakPower))
	
	#print(which(abs(kinematics$power) == peakPower))

	pp_ppt <- peakPower / (peakPowerT/1000)	# ms->s
	meanForce <- mean(kinematics$force)
	maxForce <- max(abs(kinematics$force))


	#here paf is generated
	#mass is not used by pafGenerate, but used by Kg/W (loadVSPower)
	#meanForce and maxForce are not used by pafGenerate, but used by F/S (forceVSSpeed)
	return(data.frame(
			  meanSpeed, maxSpeed, maxSpeedT,
			  meanPower, peakPower, peakPowerT, pp_ppt,
			  meanForce, maxForce,
			  kinematics$mass, massBody, massExtra)) #kinematics$mass is Load
}

kinematicRanges <- function(singleFile, displacement, curves,
			    massBody, massExtra, exercisePercentBodyWeight,
			    encoderConfiguration,diameter,diameterExt,anglePush,angleWeight,inertiaMomentum,gearedDown,
			    smoothingsEC, smoothingOneC, g, eccon, isPropulsive) {
	n=length(curves[,1])
	maxSpeedy=0; maxAccely=0; maxForce=0; maxPower=0
	myEccon = eccon
	for(i in 1:n) { 
		myMassBody = massBody
		myMassExtra = massExtra
		myExPercentBodyWeight = exercisePercentBodyWeight
			
		#encoderConfiguration
		myEncoderConfigurationName = EncoderConfigurationName
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

	#speed
	speed <- getSpeed(displacement, smoothing)
       	
	if(draw & showSpeed) {
		ylim=c(-max(abs(range(displacement))),max(abs(range(displacement))))	#put 0 in the middle 
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
	
	#time to arrive to max speed
	maxSpeedT=min(which(speed$y == max(speed$y)))
	if(draw & showSpeed & !superpose) {
		abline(v=maxSpeedT, col=cols[1])
		points(maxSpeedT, max(speed$y),col=cols[1])
		mtext(text=paste(round(max(speed$y),2),"m/s",sep=""),side=3,
		      at=maxSpeedT,cex=.8,col=cols[1], line=.5)
		mtext(text=maxSpeedT,side=1,at=maxSpeedT,cex=.8,col=cols[1])
	}


	#show extrema values in speed
	speed.ext=extrema(speed$y)


	#if(draw & !superpose) 
	#	segments(x0=speed.ext$maxindex,y0=0,x1=speed.ext$maxindex,y1=speed$y[speed.ext$maxindex],col=cols[1])

	#declare variables:
	eccentric=0
	isometric=0
	concentric=0
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
		crossMinRow=which(speed.ext$cross[,1] > time1 & speed.ext$cross[,1] < time2)
		
		isometricUse = TRUE
		#TODO: con-ecc is opposite
		if(isometricUse) {
			eccentric=1:min(speed.ext$cross[crossMinRow,1])
			isometric=c(min(speed.ext$cross[crossMinRow,1]), max(speed.ext$cross[crossMinRow,2]))
			concentric=max(speed.ext$cross[crossMinRow,2]):length(displacement)
		} else {
			eccentric=1:mean(speed.ext$cross[crossMinRow,1])
			isometric=c(mean(speed.ext$cross[crossMinRow,1]), mean(speed.ext$cross[crossMinRow,2]))
			concentric=mean(speed.ext$cross[crossMinRow,2]):length(displacement)
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
			mtext(text="eccentric ",side=3,at=max(eccentric),cex=.8,adj=1,col=cols[1],line=.5)
			mtext(text=" concentric ",side=3,at=min(concentric),cex=.8,adj=0,col=cols[1],line=.5)
		}
	}
		
	#on rotatory inertial, concentric-eccentric, use speed as ABS)
	#if(inertialType == "ri" && eccon == "ce")
	#	speed$y=abs(speed$y)

	accel <- getAcceleration(speed)
	#speed comes in mm/ms when derivate to accel its mm/ms^2 to convert it to m/s^2 need to *1000 because it's quadratic
	accel$y <- accel$y * 1000
	
	#print(accel$y)
	#alternative R method (same result)
	#accel2 <- D1ss( 1:length(speed$y), speed$y )
	#accel2 <- accel2 * 1000
	#print(accel2)

	#propulsive phase ends when accel is -9.8
	if(length(which(accel$y[concentric]<=-g)) > 0 & isPropulsive) {
		propulsiveEnd = min(concentric) + min(which(accel$y[concentric]<=-g))
	} else {
		propulsiveEnd = max(concentric)
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
		meanSpeedE = mean(speed$y[min(eccentric):max(eccentric)])
		if(showSpeed) {
			arrows(x0=min(eccentric),y0=meanSpeedE,x1=max(eccentric),y1=meanSpeedE,col=cols[1],code=3)
			arrows(x0=min(concentric),y0=meanSpeedC,x1=propulsiveEnd,y1=meanSpeedC,col=cols[1],code=3)
		}
	}

	if(draw) {
		ylim=c(-max(abs(range(accel$y))),max(abs(range(accel$y))))	 #put 0 in the middle
		if(knRanges[1] != "undefined")
			ylim = knRanges$accely

		
		#plot the speed axis
		if(showAxes & showSpeed) {
			abline(h=0,lty=3,col="black")
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
			
		if(isPropulsive) {
			#propulsive stuff
			segments(0,-9.81,length(accel$y),-9.81,lty=3,col="magenta")
			abline(v=propulsiveEnd,lty=3,col="magenta") 
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
			displacement, diameter, diameterExt, inertiaMomentum, smoothing)
	mass = dynamics$mass
	force = dynamics$force
	power = dynamics$power

#print(c(knRanges$accely, max(accel$y), min(accel$y)))
#	force <- mass*accel$y
#	if(isJump)
#		force <- mass*(accel$y+g)	#g:9.81 (used when movement is against gravity)

#print("MAXFORCE!!!!!")
#print(max(force))

	if(draw & showForce) {
		ylim=c(-max(abs(range(force))),max(abs(range(force))))	 #put 0 in the middle
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
	}

	
	#mark when it's air and land
	#if it was a eccon concentric-eccentric, will be useful to calculate flight time
	#but this eccon will be not done
	#if(draw & (!superpose || (superpose & highlight)) & isJump) 
	if(draw & (!superpose || (superpose & highlight)) & exercisePercentBodyWeight == 100) {
		weight=mass*9.81
		abline(h=weight,lty=1,col=cols[2]) #body force, lower than this, person in the air (in a jump)
		#takeoff = max(which(force>=weight))
		takeoff = min(which(force[concentric]<=weight)) + length(eccentric)
		abline(v=takeoff,lty=1,col=cols[2]) 
		mtext(text="land ",side=3,at=takeoff,cex=.8,adj=1,col=cols[2])
		mtext(text=" air ",side=3,at=takeoff,cex=.8,adj=0,col=cols[2])
		text(x=length(force),y=weight,labels="Weight (N)",cex=.8,adj=c(.5,0),col=cols[2])
		if(eccon=="ec") {
			#landing = min(which(force>=weight))
			landing = max(which(force[eccentric]<=weight))
			abline(v=landing,lty=1,col=cols[2]) 
			mtext(text="air ",side=3,at=landing,cex=.8,adj=1,col=cols[2])
			mtext(text=" land ",side=3,at=landing,cex=.8,adj=0,col=cols[2])
		}
	}
	#forceToBodyMass <- force - weight
	#force.ext=extrema(forceToBodyMass)
	#abline(v=force.ext$cross[,1],lty=3,col=cols[2]) #body force, lower than this, person in the air (in a jump)
	#text(x=(mean(force.ext$cross[1,1],force.ext$cross[1,2])+mean(force.ext$cross[2,1],force.ext$cross[2,2]))/2, y=weight, 
	#		labels=paste("flight time:", 
	#			mean(force.ext$cross[2,1],force.ext$cross[2,2])-mean(force.ext$cross[1,1],force.ext$cross[1,2]),"ms"), 
	#		col=cols[2], cex=.8, adj=c(0.5,0))

	#power #normalment m=massa barra + peses: 	F=m*a #com es va contra gravetat: 		F=m*a+m*g  	F=m*(a+g) #g sempre es positiva. a es negativa en la baixada de manera que en caiguda lliure F=0 #cal afegir la resistencia del encoder a la força #Potència	P=F*V #si es treballa amb el pes corporal, cal afegir-lo

	#F=m*a		#bar
	#F=(m*a)+(m*g) #jump m*(a+g) F=m*0



	#power = NULL

	#if(inertialType == "li" || inertialType == "ri") {
		#Explanation rotatory encoder on inertial machine
		#speed$y comes in mm/ms, is the same than m/s
		#speedw in meters:
	 #	speedw <- speed$y / diameter #m radius
		#accel$y comes in meters
		#accelw in meters:
	#	accelw <- accel$y / diameter

		#power = power to the inertial machine (rotatory disc) + power to the displaced body mass (lineal)
		#power = ( inertia momentum * angular acceleration * angular velocity ) + mass(includes extra weight if any) * accel$y * speed$y  
		#abs(speedw) because disc is rolling in the same direction and we don't have to make power to change it
	#	power <- inertiaMomentum * accelw * speedw + mass * (accel$y +g) * speed$y
	
		#print("at Paint")	
		#print(c("mass",mass))
		#print(c("speed$y",speed$y))
		#print(c("speedw",speedw))
		#print(c("accel$y",accel$y))
		#print(c("accelw",accelw))
		#print(c("power",power))
	#}
	#else #(inertialType == "")
	#	power <- force*speed$y



	if(draw & showPower) {
		ylim=c(-max(abs(range(power))),max(abs(range(power))))	#put 0 in the middle
		if(knRanges[1] != "undefined")
			ylim = knRanges$power
		par(new=T);
		if(highlight==FALSE)
			plot(startX:length(power),power[startX:length(power)],type="l",
			     xlim=c(1,length(displacement)),ylim=ylim,xlab="",ylab="",col=cols[3],lty=lty[3],lwd=2,axes=F)
		else
			plot(startX:length(power),power[startX:length(power)],type="l",
			     xlim=c(1,length(displacement)),ylim=ylim,xlab="",ylab="",col="darkred",lty=2,lwd=3,axes=F)


		meanPowerC = mean(power[min(concentric):max(concentric)])
		if(isPropulsive) {
			meanPowerC = mean(power[min(concentric):propulsiveEnd])
		}

		if(eccon == "c") {
			arrows(x0=min(concentric),y0=meanPowerC,x1=propulsiveEnd,y1=meanPowerC,col=cols[3],code=3)
		} else {
			meanPowerE = mean(power[min(eccentric):max(eccentric)])
			arrows(x0=min(eccentric),y0=meanPowerE,x1=max(eccentric),y1=meanPowerE,col=cols[3],code=3)
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
	peakPowerT=min(which(power == max(power)))
	if(draw & !superpose & showPower) {
		abline(v=peakPowerT, col=cols[3])
		points(peakPowerT, max(power),col=cols[3])
		mtext(text=paste(round(max(power),1),"W",sep=""),side=3,at=peakPowerT,adj=0.5,cex=.8,col=cols[3])
		mtext(text=peakPowerT,side=1,at=peakPowerT,cex=.8,col=cols[3])
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
			legendText=c("Distance (mm)")
			lty=c(1)
			lwd=c(2)
			colors=c("black") 
			ncol=1

			if(showSpeed) {
				legendText=c(legendText, "Speed (m/s)")
				lty=c(lty,1)
				lwd=c(lwd,2)
				colors=c(colors,cols[1]) 
				ncol=ncol+1
			}
			if(showAccel) {
				legendText=c(legendText, "Accel. (m/s²)")
				lty=c(lty,1)
				lwd=c(lwd,2)
				colors=c(colors,"magenta") 
				ncol=ncol+1
			}
			if(showForce) {
				legendText=c(legendText, "Force (N)")
				lty=c(lty,1)
				lwd=c(lwd,2)
				colors=c(colors,cols[2]) 
				ncol=ncol+1
			}
			if(showPower) {
				legendText=c(legendText, "Power (W)")
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
			mtext("time (ms) ",side=1,adj=1,line=-1,cex=.9)
			mtext("height (mm) ",side=2,adj=1,line=-1,cex=.9)
		}
	}
}

textBox <- function(x,y,text,frontCol,bgCol,xpad=.1,ypad=1){

	w=strwidth(text)+xpad*strwidth(text)
	h=strheight(text)+ypad*strheight(text)

	rect(x-w/2,y-h/2,x+w/2,y+h/2,col=bgCol, density=60, angle=-30, border=NA)
	text(x,y,text,col=frontCol)
} 


paintPowerPeakPowerBars <- function(singleFile, title, paf, Eccon, height, n, showTTPP, showRange) {
	pafColors=c("tomato1","tomato4",topo.colors(10)[3])
	myNums = rownames(paf)
	height = abs(height/10)
	load = paf[,findPosInPaf("Load","")]
	
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
		powerName = "Power"
		peakPowerName = "Peak Power (ABS)"
	}
	else {
		powerName = "Power (ABS)"
		peakPowerName = "Peak Power (ABS)"
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
	bp <- barplot(powerData,beside=T,col=pafColors[1:2],width=c(1.4,.6),
			names.arg=paste(myNums,"\n",load,sep=""),xlim=c(1,n*3+.5),cex.name=0.9,
			xlab="",ylab="Power (W)", 
			ylim=c(lowerY,max(powerData)), xpd=FALSE) #ylim, xpd = F,  makes barplot starts high (compare between them)
	title(main=title,line=-2,outer=T)
	mtext("Curve \nLoad ",side=1,at=1,adj=1,line=1,cex=.9)
	
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
		mtext("Time to peak power (ms)", side=4, line=(axisLineRight-1))
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
		mtext("Range (cm)", side=4, line=(axisLineRight-1))
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
		legendText = c(legendText, "Time to Peak Power    ")
		lty=c(lty,1)
		lwd=c(lwd,2)
		pch=c(pch,NA)
		graphColors=c(graphColors,pafColors[3])
	}
	if(showRange) {
		legendText = c(legendText, "Range")
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
		pos = 10
	else if(var == "MassBody")
		pos = 11
	else if(var == "MassExtra")
		pos = 12
	
	if( ( var == "Speed" || var == "Power" || var == "Force") & option == "max")
		pos=pos+1
	if( ( var == "Speed" || var == "Power") & option == "time")
		pos=pos+2

	return(pos)
}

addUnits <- function (var) {
	if(var == "Speed")
		return ("Speed (m/s)")
	else if(var == "Power")
		return ("Power (W)")
	else if(var == "Load") #or Mass
		return ("Load (Kg)")
	else if(var == "Force")
		return ("Force (N)")

	return(var)
}

#option: mean or max
paintCrossVariables <- function (paf, varX, varY, option, isAlone, title, singleFile, Eccon, seriesName, do1RM, do1RMMethod, outputData1) {
	x = (paf[,findPosInPaf(varX, option)])
	y = (paf[,findPosInPaf(varY, option)])

	print("seriesName")
	print(seriesName)

	colBalls = NULL
	bgBalls = NULL

	varX = addUnits(varX)
	varY = addUnits(varY)

	#if only one series
	if(length(unique(seriesName)) == 1) {
		myNums = rownames(paf)
		if(Eccon=="ecS" || Eccon=="ceS") {
			if(singleFile) {
				myEc=c("c","e")
				if(Eccon=="ceS")
					myEc=c("e","c")
				myNums = as.numeric(rownames(paf))
				myNums = paste(trunc((myNums+1)/2),myEc[((myNums%%2)+1)],sep="")
			}
		}

		#problem with balls is that two values two close looks bad
		#suboption="balls"
		suboption="side"
		if(suboption == "balls") {
			cexBalls = 3
			cexNums = 1
			adjHor = 0.5
			nums=myNums
		} else if (suboption == "side") {
			cexBalls = 1.8
			cexNums = 1
			adjHor = 0
			nums=paste("  ", myNums)
		}

		colBalls="blue"
		bgBalls="lightBlue"
		if(isAlone == "RIGHT") {
			colBalls="red"
			bgBalls="pink"
		}
		
		plot(x,y, xlab=varX, ylab="", pch=21,col=colBalls,bg=bgBalls,cex=cexBalls,axes=F)
	
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
				text(x=0,y=0,"Not enough data.",cex=1.5)
				dev.off()
				write("1RM;-1", SpecialData)
				write("", outputData1)
				quit()
			}

			load1RM = ( speed1RM - c.intercept ) / c.x

			#plot(x,y, xlim=c(min(x),load1RM), ylim=c(speed1RM, max(y)), xlab=varX, ylab="", pch=21,col=colBalls,bg=bgBalls,cex=cexBalls,axes=F)
			maxX=max(x)
			if(load1RM > maxX)
				maxX=load1RM
			plot(x,y, xlim=c(min(x),maxX), ylim=c(0, max(y)), xlab=varX, ylab="", pch=21,col=colBalls,bg=bgBalls,cex=cexBalls,axes=F)
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
			#x vector should contain at least 4 different values
			if(length(unique(x)) >= 4)
				lines(smooth.spline(x,y,df=4),col=colBalls,lwd=2)
		}
		
		title(title, cex.main=1, font.main=2)
		text(x,y,nums,adj=c(adjHor,.5),cex=cexNums)
		

	} else { #more than one series
		#colBalls = "black"
		uniqueColors=topo.colors(length(unique(seriesName)))

		#in x axis move a little every series to right in order to compare
		seqX = seq(0,length(unique(seriesName))-1,by=1)-(length(unique(seriesName))-1)/2

		plot(x,y, xlab=varX, ylab="", type="n", axes=F)
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
		title(paste(varX,"/",varY), cex.main=1, font.main=2)
			
		#plot legend on top exactly out
		#http://stackoverflow.com/a/7322792
		rng=par("usr")
		lg = legend(rng[1],rng[2], 
			    legend=unique(seriesName), lty=1, lwd=2, col=uniqueColors, 
			    cex=1, bg="white", ncol=length(unique(seriesName)), bty="n",
			    plot=F)
		legend(rng[1],rng[4]+1.25*lg$rect$h, 
		       legend=unique(seriesName), lty=1, lwd=2, col=uniqueColors, 
		       cex=1, bg="white", ncol=6, bty="n",
		       plot=T, xpd=NA)
	}
		
	if(isAlone == "ALONE") {
		axis(1)
		axis(2)
		mtext(varY, side=2, line=3)
		#box()
	} else if(isAlone == "LEFT") {
		axis(1)
		axis(2,col=colBalls)
		mtext(varY, side=2, line=3, col=colBalls)
		#box()
	} else { #"RIGHT"
		axis(4,col=colBalls)
		mtext(varY, side=4, line=3, col=colBalls)
	}

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
		text(x=0,y=0,"Not enough data.",cex=1.5)
		dev.off()
		write("1RM;-1", SpecialData)
		write("", outputData1)
		quit()
	}

	par(mar=c(6,5,3,4))

	plot(curvesLoad,curvesSpeed, type="p",
	     main=paste(title, "1RM prediction"),
	     sub="\nConcentric mean speed on bench press 1RM is 0.185m/s. Estimated percentual load = 8.4326 * speed ^2 - 73.501 * speed + 112.33\nAdapted from Gonzalez-Badillo, Sanchez-Medina (2010)",
	     xlim=c(min(curvesLoad),max(loadCalc[curvesSpeedInIntervalPos])),
	     ylim=c(miny,maxy), xlab="", ylab="",axes=T)

	mtext(side=1,line=2,"Kg")
	mtext(side=2,line=3,"Mean speed in concentric propulsive phase (m/s)")
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

#-------------------- EncoderConfiguration conversions --------------------------

isInertial <- function(encoderConfigurationName) {
	if(encoderConfigurationName == "LINEARINERTIAL" ||
	   encoderConfigurationName == "ROTARYFRICTIONSIDEINERTIAL" ||
	   encoderConfigurationName == "ROTARYFRICTIONAXISINERTIAL" ||
	   encoderConfigurationName == "ROTARYAXISINERTIAL") 
		return(TRUE)
	else
		return(FALSE)
}

#in signals and curves, need to do conversions (invert, inertiaMomentum, diameter)
#we use 'data' variable because can be position or displacement
getDisplacement <- function(encoderConfigurationName, data, diameter, diameterExt) {
	#no change
	#WEIGHTEDMOVPULLEYLINEARONPERSON1, WEIGHTEDMOVPULLEYLINEARONPERSON1INV,
	#WEIGHTEDMOVPULLEYLINEARONPERSON2, WEIGHTEDMOVPULLEYLINEARONPERSON2INV,
	#LINEARONPLANE
	#ROTARYFRICTIONSIDE
	#WEIGHTEDMOVPULLEYROTARYFRICTION

	if(
	   encoderConfigurationName == "LINEARINVERTED" ||
	   encoderConfigurationName == "WEIGHTEDMOVPULLEYLINEARONPERSON1INV" ||
	   encoderConfigurationName == "WEIGHTEDMOVPULLEYLINEARONPERSON2INV") 
	{
		data = -data
	} else if(encoderConfigurationName == "WEIGHTEDMOVPULLEYONLINEARENCODER") {
		#default is: gearedDown = 2. Future maybe this will be a parameter
		data = data *2
	} else if(encoderConfigurationName == "ROTARYFRICTIONAXIS") {
		data = data * diameter / diameterExt
	} else if(encoderConfigurationName == "ROTARYAXIS" || 
		  encoderConfigurationName == "WEIGHTEDMOVPULLEYROTARYAXIS") {
		ticksRotaryEncoder = 200 #our rotary axis encoder send 200 ticks by turn
		#diameter m -> mm
		data = ( data / ticksRotaryEncoder ) * 2 * pi * ( diameter * 1000 / 2 )
	}
		
	return(data)
}

getSpeed <- function(displacement, smoothing) {
	#no change affected by encoderConfiguration

	return (smooth.spline( 1:length(displacement), displacement, spar=smoothing))
}

getAcceleration <- function(speed) {
	#no change affected by encoderConfiguration
	return (predict( speed, deriv=1 ))
}

#gearedDown is positive, normally 2
getMass <- function(mass, gearedDown, angle) {
	if(mass == 0)
		return (0)

	#default value of angle is 90 degrees. If is not selected, it's -1
	if(angle == -1)
		angle = 90

	return ( ( mass / gearedDown ) * sin( angle * pi / 180 ) )
}

getMassBodyByExercise <- function(massBody, exercisePercentBodyWeight) {
	if(massBody == 0 || exercisePercentBodyWeight == 0)
		return (0)
	
	return (massBody * exercisePercentBodyWeight / 100.0)
}

getMassByEncoderConfiguration <- function(encoderConfigurationName, massBody, massExtra, exercisePercentBodyWeight, gearedDown, anglePush, angleWeight)
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
	} else if(encoderConfigurationName == "LINEARONPLANE") {
		massBody = getMass(massBody, gearedDown, anglePush)
		massExtra = getMass(massExtra, gearedDown, anglePush)
	} else if(encoderConfigurationName == "LINEARONPLANEWEIGHTDIFFANGLE") {
		massBody = getMass(massBody, gearedDown, anglePush)
		massExtra = getMass(massExtra, gearedDown, angleWeight)
	}
		
	mass = massBody + massExtra
	return (mass)
}

getDynamics <- function(encoderConfigurationName,
			speed, accel, massBody, massExtra, exercisePercentBodyWeight, gearedDown, anglePush, angleWeight,
			displacement, diameter, diameterExt, inertiaMomentum, smoothing)
{
	mass = getMassByEncoderConfiguration (encoderConfigurationName, massBody, massExtra, exercisePercentBodyWeight, gearedDown, anglePush, angleWeight)

	if(isInertial(encoderConfigurationName))
		return (getDynamicsInertial(encoderConfigurationName, displacement, diameter, diameterExt, mass, inertiaMomentum, smoothing))
	else 
		return (getDynamicsNotInertial (encoderConfigurationName, speed, accel, mass, exercisePercentBodyWeight, gearedDown, anglePush, angleWeight))
}

#mass extra can be connected to body or connected to a pulley depending on encoderConfiguration
getDynamicsNotInertial <- function(encoderConfigurationName, speed, accel, mass, exercisePercentBodyWeight, gearedDown, anglePush, angleWeight) 
{
	force <- mass*(accel+g)	#g:9.81 (used when movement is against gravity)

	power <- force*speed

	return(list(mass=mass, force=force, power=power))
}

#d: diameter axis
#D: diameter external (disc)
#angle: angle (rotation of disc) in radians
#angleSpeed: speed of angle
#angleAccel: acceleration of angle
#encoderConfiguration:
#  LINEARINERTIAL Linear encoder on inertial machine (rolled on axis)
#  ROTARYFRICTIONSIDEINERTIAL Rotary friction encoder connected to inertial machine on the side of the disc
#  ROTARYFRICTIONAXISINERTIAL Rotary friction encoder connected to inertial machine on the axis
#  ROTARYAXISINERTIAL Rotary axis encoder  connected to inertial machine on the axis

getDynamicsInertial <- function(encoderConfigurationName, displacement, d, D, mass, inertiaMomentum, smoothing)
{
	#first: Internationational System units
	displacement = displacement / 1000 #mm -> m
	d=d/100 #cm -> m
	D=D/100 #cm -> m

	#2nd, on friction side: know displacement of the "person"
	if(encoderConfigurationName == "ROTARYFRICTIONSIDEINERTIAL")
	{
		displacement = displacement * d / D #displacement of the axis
	}

	#position = abs(cumsum(displacement)) / 1000 #mm -> m
	position = abs(cumsum(displacement))

	if(encoderConfigurationName == "LINEARINERTIAL" ||
	   encoderConfigurationName == "ROTARYFRICTIONSIDEINERTIAL" ||
	   encoderConfigurationName == "ROTARYFRICTIONAXISINERTIAL") {

		speed = getSpeed(displacement, smoothing)  # getSpeed returns in m/ms because displacement is in m/ms
		speed$y = speed$y * 1000 # m/ms -> m/s
		
		# accel will be:
	        # x = ms (there's one value for each ms)
		# y = m/ms²
		accel = getAcceleration(speed) 
		
		accel$y = accel$y * 1000000 # m/ms² -> m/s²
		
		#use the values
		speed = speed$y
		accel = accel$y

	   	if(encoderConfigurationName == "ROTARYFRICTIONSIDEINERTIAL") {
			angle = position * 2 / D
			angleSpeed = speed * 2 / D
			angleAccel = accel * 2 / D
		} else {
			angle = position * 2 / d
			angleSpeed = speed * 2 / d
			angleAccel = accel * 2 / d
		}
	} else {
		#(encoderConfigurationName == "ROTARYAXISINERTIAL")
		ticksRotaryEncoder = 200 #our rotary axis encoder send 200 ticks by turn

		#angle in radians
		angle = abs(cumsum(displacement * 1000)) * 2 * pi / ticksRotaryEncoder

		#angleSpeed in radians/ms
		angleSpeed = getSpeed(angle, smoothing)
		angleSpeed$y = angleSpeed$y * 1000 # rad/ms -> rad/s

		# accel will be:
	        # x = ms (there's one value for each ms)
		# y = rad/ms²
		angleAccel = getAcceleration(angleSpeed)      
		
		#angleAccel$y = angleAccel$y * 1000000 # rad/ms² -> rad/s²
		
		#use the values
		angleSpeed = angleSpeed$y
		angleAccel = angleAccel$y

		position = angle * d / 2
		speed = angleSpeed * d / 2
		accel = angleAccel * d / 2
	}

	force = abs(inertiaMomentum * angleAccel) * (2 / d) + mass * (accel + g)
	power = abs((inertiaMomentum * angleAccel) * angleSpeed) + mass * (accel + g) * speed
	powerBody = mass * (accel + g) * speed
	powerWheel = abs((inertiaMomentum * angleAccel) * angleSpeed)

	#print(c("displacement",displacement))
	#print(c("inertia momentum",inertiaMomentum))
        #print(c("d",d))
        #print(c("mass",mass))
        #print(c("g",g))
	#print(c("angleAccel",angleAccel))
	#print(c("angleSpeed",angleSpeed))
	#print(c("speed",speed))
	#print(c("accel",accel))
	#print(c("force",force))
	#print(c("power at inertial",power))
	#print(c("powerBody",powerBody[1000]))
	#print(c("powerWheel",powerWheel[1000]))

	return(list(displacement=displacement, mass=mass, force=force, power=power))
}

#-------------- end of EncoderConfiguration conversions -------------------------

quitIfNoData <- function(n, curves, outputData1) {
	#if not found curves with this data, plot a "sorry" message and exit
	if(n == 1 & curves[1,1] == 0 & curves[1,2] == 0) {
		plot(0,0,type="n",axes=F,xlab="",ylab="")
		text(x=0,y=0,"Sorry, no curves matched your criteria.",cex=1.5)
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

doProcess <- function(options) {

	File=options[1]
	OutputGraph=options[2]
	OutputData1=options[3]
	OutputData2=options[4] #currently used to display processing feedback
	SpecialData=options[5] #currently used to write 1RM. variable;result (eg. "1RM;82.78")
	MinHeight=as.numeric(options[6])*10 #from cm to mm
	ExercisePercentBodyWeight=as.numeric(options[7])	#was isJump=as.logical(options[6])
	MassBody=as.numeric(options[8])	
	MassExtra=as.numeric(options[9])	

	Eccon=options[10]
	
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
	Analysis=options[11]	
	AnalysisVariables=unlist(strsplit(options[12], "\\;"))
	
	AnalysisOptions=options[13]	

	#TODO: all this have to be applicable also on ! singleFILE
	EncoderConfigurationName=	options[14]	#just the name of the EncoderConfiguration	
	diameter=	as.numeric(options[15])	#in meters, eg: 0.0175
	diameterExt=	as.numeric(options[16])	#in meters, eg: 0.0175
	anglePush =	as.numeric(options[17])
	angleWeight =	as.numeric(options[18])
	inertiaMomentum=as.numeric(options[19])/10000.0	#comes in Kg*cm^2 eg: 100; convert it to Kg*m^2 eg: 0.010
	gearedDown =	as.numeric(options[20])

	SmoothingOneC=as.numeric(options[21])
	Jump=options[22]
	Width=as.numeric(options[23])
	Height=as.numeric(options[24])
	DecimalSeparator=options[25]
	Title=options[26]
	OperatingSystem=options[27]	#if this changes, change it also at start of this R file
	#IMPORTANT, if this grows, change the readLines value on getOptionsFromFile

	print(File)
	print(OutputGraph)
	print(OutputData1)
	print(OutputData2)
	print(SpecialData)

	#read AnalysisOptions
	#if is propulsive and rotatory inertial is: "p;ri" 
	#if nothing: "-;-"
	analysisOptionsTemp = unlist(strsplit(AnalysisOptions, "\\;"))
	isPropulsive = (analysisOptionsTemp[1] == "p")
	inertialType = ""	#TODO: use EncoderConfiguration
	if(length(analysisOptionsTemp) > 1) {
		inertialType = analysisOptionsTemp[2] #values: "" || "li" || "ri"
	}

	#inertial cannot be propulsive
	if(isInertial(EncoderConfigurationName))
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

	
	if(Analysis != "exportCSV") {
		if(OperatingSystem=="Windows")
			Cairo(Width, Height, file=OutputGraph, type="png", bg="white")
		else
			png(OutputGraph, width=Width, height=Height)

		Title=gsub('_',' ',Title)
		Title=gsub('-','    ',Title)
	}

	titleType = "c"
	#if(isJump)
	#	titleType="jump"

	curvesPlot = FALSE
	if(Analysis == "curves") {
		curvesPlot = TRUE
		par(mar=c(2,2.5,2,1))
	}

	#when a csv is used (it links to lot of files) then singleFile = false
	singleFile = TRUE
	if(nchar(File) >= 40) {
		#file="/tmp...../chronojump-encoder-graph-input-multi.csv"
		#substr(file, nchar(file)-39, nchar(file))
		#[1] "chronojump-encoder-graph-input-multi.csv"
		if(substr(File, nchar(File)-39, nchar(File)) == "chronojump-encoder-graph-input-multi.csv") {
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

		inputMultiData=read.csv(file=File,sep=",",stringsAsFactors=F)

		displacement = NULL
		count = 1
		start = NULL; end = NULL; startH = NULL
		status = NULL; id = NULL; exerciseName = NULL; massBody = NULL; massExtra = NULL
		dateTime = NULL; myEccon = NULL; curvesHeight = NULL
		seriesName = NULL; percentBodyWeight = NULL;

		#encoderConfiguration
		econfName = NULL; econfd = NULL; econfD = NULL; econfAnglePush = NULL; econfAngleWeight = NULL; 
		econfInertia = NULL; econfGearedDown = NULL;

		newLines=0;
		countLines=1; #useful to know the correct ids of active curves
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

			dataTempFile = getDisplacement(inputMultiData$econfName[i], dataTempFile, diameter, diameterExt)

			dataTempPhase=dataTempFile
			processTimes = 1
			changePos = 0
			#if this curve is ecc-con and we want separated, divide the curve in two
			if(as.vector(inputMultiData$eccon[i]) != "c" & (Eccon=="ecS" || Eccon=="ceS") ) {
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
				econfInertia[(i+newLines)] = inputMultiData$econfInertia[i]
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
					    stringsAsFactors=F,row.names=id)
		} else {
			curves = data.frame(id,start,end,startH,exerciseName,massBody,massExtra,
					    dateTime,myEccon,seriesName,percentBodyWeight,
					    econfName,econfd,econfD,econfAnglePush,econfAngleWeight,econfInertia,econfGearedDown,
					    stringsAsFactors=F,row.names=1)
		}

		n=length(curves[,1])
		quitIfNoData(n, curves, OutputData1)
		
		print("curves")
		print(curves)
		
		#find SmoothingsEC
		SmoothingsEC = findSmoothingsEC(displacement, curves, Eccon, SmoothingOneC)
	} else {	#singleFile == True. reads a signal file
		displacement=scan(file=File,sep=",")
			
		#if data file ends with comma. Last character will be an NA. remove it
		#this removes all NAs
		displacement  = displacement[!is.na(displacement)]

		if(isInertial(EncoderConfigurationName)) 
		{
			if(EncoderConfigurationName == "ROTARYAXISINERTIAL") {
				displacementMeters = displacement / 1000 #mm -> m
				diameterMeters = diameter / 100 #cm -> m

				ticksRotaryEncoder = 200 #our rotary axis encoder send 200 ticks by turn
				#angle in radians
				angle = abs(cumsum(displacementMeters * 1000)) * 2 * pi / ticksRotaryEncoder
				position = angle * diameterMeters / 2
				position = position * 1000	#m -> mm
				#this is to make "inverted cumsum"
				displacement = c(0,diff(position))
			}
			
			displacement = getDisplacementInertial(displacement, curvesPlot, Title)
			curvesPlot = FALSE
		} else {
			displacement = getDisplacement(EncoderConfigurationName, displacement, diameter, diameterExt)
		}

		if(length(displacement)==0) {
			plot(0,0,type="n",axes=F,xlab="",ylab="")
			text(x=0,y=0,"Encoder is not connected.",cex=1.5)
			dev.off()
			write("", OutputData1)
			quit()
		}
			
		position=cumsum(displacement)

		#print(c("position",position))
		#print(c("displacement",displacement))
		
		curves=findCurves(displacement, Eccon, MinHeight, curvesPlot, Title)

		if(Analysis == "curves")
			curvesPlot = TRUE

		n=length(curves[,1])
		quitIfNoData(n, curves, OutputData1)
	
		#reduceCurveBySpeed
		for(i in 1:n) {
			reduceTemp=reduceCurveBySpeed(Eccon, i, curves[i,1], displacement[curves[i,1]:curves[i,2]], SmoothingOneC)
			curves[i,1] = reduceTemp[1]
			curves[i,2] = reduceTemp[2]
		}
		
		#find SmoothingsEC
		SmoothingsEC = findSmoothingsEC(displacement, curves, Eccon, SmoothingOneC)
		print(c("SmoothingsEC:",SmoothingsEC))
		
		print("curves before reduceCurveBySpeed")
		print(curves)

		
		if(curvesPlot) {
			#/10 mm -> cm
			for(i in 1:length(curves[,1])) { 
				myLabel = i
				myY = min(position)/10
				adjVert = 0
				if(Eccon=="ceS")
					adjVert = 1

				if(Eccon=="ecS" || Eccon=="ceS") {
					myEc=c("c","e")
					if(Eccon=="ceS")
						myEc=c("e","c")
					
					myLabel = paste(trunc((i+1)/2),myEc[((i%%2)+1)],sep="")
					myY = position[curves[i,1]]/10
					if(i%%2 == 1) {
						adjVert = 1
						if(Eccon=="ceS")
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
			speed <- getSpeed(displacement, smoothingAll)
			plot((1:length(displacement))/1000, speed$y, col="green2",
		     		type="l", 
				xlim=c(1,length(displacement))/1000,	#ms -> s
				#ylim=c(-.25,.25),		#to test speed at small changes
		     		xlab="",ylab="",axes=F)
			
			if(isInertial(EncoderConfigurationName))
				mtext("body speed ",side=4,adj=1,line=-1,col="green2",cex=.8)
			else
				mtext("speed ",side=4,adj=1,line=-1,col="green2")

			abline(h=0,lty=2,col="gray")
		}
	}

	write("(4/5) Curves processed", OutputData2)

	if(Analysis=="single") {
		if(Jump>0) {
			myMassBody = MassBody
			myMassExtra = MassExtra
			myEccon = Eccon
			myStart = curves[Jump,1]
			myEnd = curves[Jump,2]
			myExPercentBodyWeight = ExercisePercentBodyWeight
			
			#encoderConfiguration
			myEncoderConfigurationName = EncoderConfigurationName
			myDiameter = diameter
			myDiameterExt = diameterExt
			myAnglePush = anglePush
			myAngleWeight = angleWeight
			myInertiaMomentum = inertiaMomentum
			myGearedDown = gearedDown
			if(! singleFile) {
				myMassBody = curves[Jump,5]
				myMassExtra = curves[Jump,6]
				myEccon = curves[Jump,8]
				myExPercentBodyWeight = curves[Jump,10]

				#encoderConfiguration
				myEncoderConfigurationName = curves[Jump,11]
				myDiameter = curves[Jump,12]
				myDiameterExt = curves[Jump,13]
				myAnglePush = curves[Jump,14]
				myAngleWeight = curves[Jump,15]
				myInertiaMomentum = curves[Jump,16]
				myGearedDown = curves[Jump,17]
			}
			
			myCurveStr = paste("curve=", Jump, ", ", myMassExtra, "Kg", sep="")
		
			#don't do this, because on inertial machines string will be rolled to machine and not connected to the body
			#if(inertialType == "li") {
			#	displacement[myStart:myEnd] = fixRawdataLI(displacement[myStart:myEnd])
			#	myEccon="c"
			#}

			paint(displacement, myEccon, myStart, myEnd,"undefined","undefined",FALSE,FALSE,
			      1,curves[Jump,3],SmoothingsEC[as.numeric(Jump)],SmoothingOneC,myMassBody,myMassExtra,
			      myEncoderConfigurationName,myDiameter,myDiameterExt,myAnglePush,myAngleWeight,myInertiaMomentum,myGearedDown,
			      paste(Title, " ", Analysis, " ", myEccon, " ", myCurveStr, sep=""),
			      "", #subtitle
			      TRUE,	#draw
			      TRUE,	#showLabels
			      FALSE,	#marShrink
			      TRUE,	#showAxes
			      TRUE,	#legend
			      Analysis, isPropulsive, inertialType, myExPercentBodyWeight,
			      (AnalysisVariables[1] == "Speed"), #show speed
			      (AnalysisVariables[2] == "Accel"), #show accel
			      (AnalysisVariables[3] == "Force"), #show force
			      (AnalysisVariables[4] == "Power")  #show power
			      )	
		}
	}

	if(Analysis=="side") {
		#comparar 6 salts, falta que xlim i ylim sigui el mateix
		par(mfrow=find.mfrow(n))

		yrange=find.yrange(singleFile, displacement, curves)

		#if !singleFile kinematicRanges takes the 'curves' values
		knRanges=kinematicRanges(singleFile, displacement, curves, 
					 MassBody, MassExtra, ExercisePercentBodyWeight, 
			    		 EncoderConfigurationName,diameter,diameterExt,anglePush,angleWeight,inertiaMomentum,gearedDown,
					 SmoothingsEC, SmoothingOneC, 
					 g, Eccon, isPropulsive)

		for(i in 1:n) {
			myMassBody = MassBody
			myMassExtra = MassExtra
			myEccon = Eccon
			myExPercentBodyWeight = ExercisePercentBodyWeight
			
			#encoderConfiguration
			myEncoderConfigurationName = EncoderConfigurationName
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

			myTitle = ""
			if(i == 1)
				myTitle = paste(Title)
			
			mySubtitle = paste("curve=", rownames(curves)[i], ", ", myMassExtra, "Kg", sep="")

			paint(displacement, myEccon, curves[i,1],curves[i,2],yrange,knRanges,FALSE,FALSE,
			      1,curves[i,3],SmoothingsEC[i],SmoothingOneC,myMassBody,myMassExtra,
			      myEncoderConfigurationName,myDiameter,myDiameterExt,myAnglePush,myAngleWeight,myInertiaMomentum,myGearedDown,
			      myTitle,mySubtitle,
			      TRUE,	#draw
			      FALSE,	#showLabels
			      TRUE,	#marShrink
			      FALSE,	#showAxes
			      FALSE,	#legend
			      Analysis, isPropulsive, inertialType, myExPercentBodyWeight,
			      (AnalysisVariables[1] == "Speed"), #show speed
			      (AnalysisVariables[2] == "Accel"), #show accel
			      (AnalysisVariables[3] == "Force"), #show force
			      (AnalysisVariables[4] == "Power")  #show power
			      )
		}
		par(mfrow=c(1,1))
	}
#	if(Analysis=="superpose") {	#TODO: fix on ec startH
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
#			      Analysis, isPropulsive, inertialType, ExercisePercentBodyWeight 
#			      )
#			par(new=T)
#		}
#		par(new=F)
#		#print(knRanges)
#	}

	#since Chronojump 1.3.6, encoder analyze has a treeview that can show the curves
	#when an analysis is done, curves file has to be written
	writeCurves = TRUE

	if(
	   Analysis == "powerBars" || Analysis == "cross" || 
	   Analysis == "1RMBadillo2010" || Analysis == "1RMAnyExercise" || 
	   Analysis == "curves" || writeCurves) 
	{
		paf = data.frame()
		discardedCurves = NULL
		discardingCurves = FALSE
		for(i in 1:n) { 
			myMassBody = MassBody
			myMassExtra = MassExtra
			myEccon = Eccon
			myExPercentBodyWeight = ExercisePercentBodyWeight
			
			#encoderConfiguration
			myEncoderConfigurationName = EncoderConfigurationName
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

				#only use concentric data	
				if( (Analysis == "1RMBadillo2010" || Analysis == "1RMAnyExercise") & myEccon == "e") {
					discardedCurves = c(i,discardedCurves)
					discardingCurves = TRUE
					next;
				}
			} else {
				if( (Analysis == "1RMBadillo2010" || Analysis == "1RMAnyExercise") & Eccon == "ecS" & i%%2 == 1) {
					discardedCurves = c(i,discardedCurves)
					discardingCurves = TRUE
					next;
				}
				else if( (Analysis == "1RMBadillo2010" || Analysis == "1RMAnyExercise") & Eccon == "ceS" & i%%2 == 0) {
					discardedCurves = c(i,discardedCurves)
					discardingCurves = TRUE
					next;
				}
			}

			print(c("i, curves[i,1], curves[i,2]", i, curves[i,1],curves[i,2]))

			#if ecS go kinematics first time with "e" and second with "c"
			myEcconKn = myEccon
			if(myEccon=="ecS") {
			       if(i%%2 == 1)
				       myEcconKn = "e"
			       else
				       myEcconKn = "c"
			}
			else if(Eccon=="ceS") {
			       if(i%%2 == 1)
				       myEcconKn = "c"
			       else
				       myEcconKn = "e"
			}
			paf = rbind(paf,(pafGenerate(
						     myEccon,
						     kinematicsF(displacement[curves[i,1]:curves[i,2]], 
								 myMassBody, myMassExtra, myExPercentBodyWeight,
								 myEncoderConfigurationName,myDiameter,myDiameterExt,myAnglePush,myAngleWeight,myInertiaMomentum,myGearedDown,
								 SmoothingsEC[i],SmoothingOneC, 
								 g, myEcconKn, isPropulsive),
						     myMassBody, myMassExtra
						     )))
		}

		#on 1RMBadillo discard curves "e", because paf has this curves discarded
		#and produces error on the cbinds below			
		if(discardingCurves)
			curves = curves[-discardedCurves,]

		rownames(paf)=rownames(curves)
		print("--------CURVES (propulsive is not calculated yet) --------------")
		print(curves)
		print("----------PAF---------------")
		print(paf)

		if(Analysis == "powerBars") {
			if(! singleFile) 
				paintPowerPeakPowerBars(singleFile, Title, paf, 
							Eccon,	 			#Eccon
							curvesHeight,			#height 
							n, 
			      				(AnalysisVariables[1] == "TimeToPeakPower"), 	#show time to pp
			      				(AnalysisVariables[2] == "Range") 		#show range
							)		
			else 
				paintPowerPeakPowerBars(singleFile, Title, paf, 
							Eccon,					#Eccon
							position[curves[,2]]-curves[,3], 	#height
							n, 
			      				(AnalysisVariables[1] == "TimeToPeakPower"), 	#show time to pp
			      				(AnalysisVariables[2] == "Range") 		#show range
							) 
		}
		else if(Analysis == "cross") {
			mySeries = "1"
			if(! singleFile)
				mySeries = curves[,9]

			print("AnalysisVariables:")
			print(AnalysisVariables[1])
			print(AnalysisVariables[2])
			print(AnalysisVariables[3])

			if(AnalysisVariables[1] == "Speed,Power") {
				par(mar=c(5,4,4,5))
				analysisVertVars = unlist(strsplit(AnalysisVariables[1], "\\,"))
				paintCrossVariables(paf, AnalysisVariables[2], analysisVertVars[1], 
						    AnalysisVariables[3], "LEFT", Title,
						    singleFile,Eccon,mySeries, 
						    FALSE, FALSE, OutputData1) 
				par(new=T)
				paintCrossVariables(paf, AnalysisVariables[2], analysisVertVars[2], 
						    AnalysisVariables[3], "RIGHT", "",
						    singleFile,Eccon,mySeries, 
						    FALSE, FALSE, OutputData1) 
			} else
				paintCrossVariables(paf, AnalysisVariables[2], AnalysisVariables[1], 
						    AnalysisVariables[3], "ALONE", Title,
						    singleFile,Eccon,mySeries, 
						    FALSE, FALSE, OutputData1) 
		}
		else if(Analysis == "1RMAnyExercise") {
			mySeries = "1"
			if(! singleFile)
				mySeries = curves[,9]

			paintCrossVariables(paf, "Load", "Speed", 
					    "mean", "ALONE", Title,
					    singleFile,Eccon,mySeries, 
					    AnalysisVariables[1], AnalysisVariables[2], #speed1RM, method
					    OutputData1) 
		}
		else if(Analysis == "1RMBadillo2010") {
			paint1RMBadillo2010(paf, Title, OutputData1)
		} 
		
		if(Analysis == "curves" || writeCurves) {
			if(singleFile)
				paf = cbind(
					  "1",			#seriesName
					  "exerciseName",
					  MassBody,
					  MassExtra,
					  curves[,1],
					  curves[,2]-curves[,1],position[curves[,2]]-curves[,3],paf)
			else {
				if(discardingCurves)
					curvesHeight = curvesHeight[-discardedCurves]

				paf = cbind(
					  curves[,9],		#seriesName
					  curves[,4],		#exerciseName
					  curves[,5],		#massBody
					  curves[,6],		#massExtra
					  curves[,1],		
					  curves[,2]-curves[,1],curvesHeight,paf)
			}

			colnames(paf)=c("series","exercise","massBody","massExtra",
					"start","width","height",
					"meanSpeed","maxSpeed","maxSpeedT",
					"meanPower","peakPower","peakPowerT",
					"pp_ppt")
			write.csv(paf, OutputData1, quote=FALSE)
			print("curves written")
		}
	}
	if(Analysis=="exportCSV") {
		print("Starting export...")
		File=OutputData1;
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
					  MassBody, MassExtra, ExercisePercentBodyWeight,
					  EncoderConfigurationName,diameter,diameterExt,anglePush,angleWeight,inertiaMomentum,gearedDown,
					  SmoothingsEC[i], SmoothingOneC, g, Eccon, isPropulsive)

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

		rownames(df) = c("MEAN (ABS)", "MAX", "TIME TO MAX", "RANGE", 1:maxLength)
		colnames(df) = namesNums

		#TODO: time
		#TODO: tenir en compte el startH

		#Title=gsub('_',' ',Title)
		#print(Title)
		#titleColumns=unlist(strsplit(Title,'-'))
		#colnames(df)=c(titleColumns[1]," ", titleColumns[2],titleColumns[3],rep(" ",(curvesNum*curveCols-4)))

		if(DecimalSeparator == "COMMA")
			write.csv2(df, file=File, row.names=T, na="")
		else
			write.csv(df, file=File, row.names=T, na="")

		print("Export done.")
	}
	if(Analysis != "exportCSV")
		dev.off()

	write("(5/5) R tasks done", OutputData2)

	warnings()
}

write("(2/5) Loading libraries", OutputData2)

loadLibraries(OperatingSystem)
	
write("(3/5) Starting process", OutputData2)

doProcess(options)

