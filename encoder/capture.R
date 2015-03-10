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
# 


#http://stackoverflow.com/questions/26053302/is-there-a-way-to-use-standard-input-output-stream-to-communicate-c-sharp-and-r/26058010#26058010

#Caution: Do not 'print, cat' stuff because it's readed from gui/encoder as results
#it can be printed safely to stderr. See end of this file

write("Arriving at capture.R", stderr())

f <- file("stdin")
open(f)


args <- commandArgs(TRUE)
optionsFile <- args[1]


getOptionsFromFile <- function(optionsFile, lines) {
	optionsCon <- file(optionsFile, 'r')
	options=readLines(optionsCon, n=lines)
	close(optionsCon)
	return (options)
}

options <- getOptionsFromFile(optionsFile, 32)


scriptUtilR = options[28]
source(scriptUtilR)

g = 9.81

debug = FALSE
		    
filename = options[1]
file.create(filename)


calcule <- function(displacement, start, end, op) 
{
	if(debug)
		write("At calcule", stderr())
	#read AnalysisOptions
	#if is propulsive and rotatory inertial is: "p;ri" 
	#if nothing: "-;-"
	analysisOptionsTemp = unlist(strsplit(op$AnalysisOptions, "\\;"))
	isPropulsive = (analysisOptionsTemp[1] == "p")


	#simplify on capture and have the SmoothingEC == SmoothingC
	SmoothingsEC = op$SmoothingOneC
	
	
	#if ecS go kinematics first time with "e" and second with "c"
	#ceS do the opposite
	myEcconKn = op$Eccon
	if(myEcconKn == "ecS" || myEcconKn == "ceS") { 
		if(mean(displacement) < 0)
			myEcconKn = "e"
		else
			myEcconKn = "c"
	}


	if(debug)
		write("At calcule calling kinematics", stderr())
	kinematicsResult <- kinematicsF(displacement, 
		    op$MassBody, op$MassExtra, op$ExercisePercentBodyWeight,
		    op$EncoderConfigurationName, op$diameter, op$diameterExt, op$anglePush, op$angleWeight, op$inertiaMomentum, op$gearedDown,
		    SmoothingsEC, op$SmoothingOneC, 
		    g, myEcconKn, isPropulsive)

	paf = data.frame()
	myLaterality = "" #TODO
	paf = pafGenerate(op$Eccon, kinematicsResult, op$MassBody, op$MassExtra, myLaterality)
		
	position = cumsum(displacement)

	#do not use print because it shows the [1] first. Use cat:
	#cat(paste(#start, #start is not used because we have no data of the initial zeros
	#	  #(end-start), (position[end]-position[start]), #this is not used because the start, end values are not ok now
	#	  0, 0, 
	#	  paf$meanSpeed, paf$maxSpeed, paf$maxSpeedT, 
	#	  paf$meanPower, paf$peakPower, paf$peakPowerT, paf$pp_ppt, 
	#	  paf$meanForce, paf$maxForce, paf$maxForceT,
	#	  sep=", "))
	#cat("\n") #mandatory to read this from C#, but beware, there we will need a trim to remove the windows \r\n
	write(paste(#start, #start is not used because we have no data of the initial zeros
		  0, 0, 
		  paf$meanSpeed, paf$maxSpeed, paf$maxSpeedT, 
		  paf$meanPower, paf$peakPower, paf$peakPowerT, paf$pp_ppt, 
		  paf$meanForce, paf$maxForce, paf$maxForceT,
		  sep=", "), filename, append=TRUE)
	if(debug)
		write("ended calcule", stderr())
}
		
getPositionStart <- function(input) 
{
	inputVector = unlist(strsplit(input, " "))
	if( length(inputVector) == 2 && inputVector[1] == "ps" )
		return (as.numeric(inputVector[2]))
	else
		return (0)
}


#converts data: "0*5 1 0 -1*3 2"
#into: 0  0  0  0  0  1  0 -1 -1 -1  2
uncompress <- function(curveSent)
{
	chunks = unlist(strsplit(curveSent, " "))
	ints = NULL
	for(i in 1:length(chunks)) 
	{
		if(grepl("\\*",chunks[i])) {
			chunk = as.numeric(unlist(strsplit(chunks[i], "\\*"))) #from "0*1072" to: 0 1072 (as integers)
			chunk = rep(chunk[1],chunk[2])
		} else {
			chunk=chunks[i]
		}
		ints = c(ints,chunk)
	}
	return (as.numeric(ints))
}

doProcess <- function() 
{
	if(debug)
		write("doProcess", stderr())
	op <- assignOptions(options)


	#print ("----op----")
	#print (op)
	
	input <- readLines(f, n = 1L)
	while(input[1] != "Q") {
		if(debug)
			write("doProcess main while", stderr())
		
		#Sys.sleep(4) #just to test how Chronojump reacts if process takes too long
		#cat(paste("input is:", input, "\n"))

		#from Chronojump first it's send the eg: "ps -1000", meaning curve starts at -1000
		#then it's send the displacement
		positionStart = getPositionStart(input)

		#-- read the curve (from some lines that finally end on an 'E')
		readingCurve = TRUE
		input = NULL
		while(readingCurve) {
			inputLine <- readLines(f, n = 1L)
			if(inputLine[1] == "E")
				readingCurve = FALSE
			else
				input = c(input, inputLine)
		}
		#-- curve readed
		
		if(debug)
			write("doProcess input", stderr())
		#write(input, stderr())

		#when data is sent uncompressed
		#displacement = as.numeric(unlist(strsplit(input, " ")))
		#when data is sent compressed
		displacement = uncompress(input)

		#if data file ends with comma. Last character will be an NA. remove it
		#this removes all NAs
		displacement  = displacement[!is.na(displacement)]


		if(debug)
			write("doProcess 2", stderr())
		if(isInertial(op$EncoderConfigurationName)) 
		{
			displacement = fixDisplacementInertial(displacement, op$EncoderConfigurationName, op$diameter, op$diameterExt)

			displacement = getDisplacementInertialBody(positionStart, displacement, FALSE, op$Title) #draw: FALSE
		} else {
			displacement = getDisplacement(op$EncoderConfigurationName, displacement, op$diameter, op$diameterExt)
		}

		start = 1
		end = length(displacement)
		if( ! isInertial(op$EncoderConfigurationName)) {
			reduceTemp = reduceCurveBySpeed(op$Eccon, 1, 
							1, 0, #startT, startH
							displacement, #displacement
							op$SmoothingOneC #SmoothingOneC
							)

			start = reduceTemp[1]
			end = reduceTemp[2]
			#write("printing reduceTemp2", stderr())
			#write(reduceTemp[2], stderr())
			if(end > length(displacement))
				end = length(displacement)

			displacement = displacement[start:end]
		}
		if(debug)
			write("doProcess 3", stderr())

		#if isInertial: getDisplacementInertialBody separate phases using initial height of full extended person
		#so now there will be two different curves to process
		if(isInertial(op$EncoderConfigurationName)) 
		{
			position = cumsum(displacement)
			positionTop <- floor(mean(which(position == max(position))))
			displacement1 = displacement[1:positionTop]
			displacement2 = displacement[(positionTop+1):length(displacement)]

			if(op$Eccon == "c") {
				calcule(displacement1, start, end, op) #TODO: check this start, end
			} else {
				calcule(displacement1, start, end, op) #TODO: check this start, end
				calcule(displacement2, start, end, op) #TODO: check this start, end
			}

			#write(c("positionTop", positionTop), stderr())
			#write(c("length(displacement)", length(displacement)), stderr())
		} else {
			calcule(displacement, start, end, op) #TODO: check this start, end
		}
		if(debug)
			write("doProcess 4", stderr())

		input <- readLines(f, n = 1L)
	}

}
		

doProcess()
write("Ending capture.R", stderr())
quit()
