# 
#  This file is part of ChronoJump
# 
#   Copyright (C) 2014-2023  	Xavier de Blas <xaviblas@gmail.com>
# 


#http://stackoverflow.com/questions/26053302/is-there-a-way-to-use-standard-input-output-stream-to-communicate-c-sharp-and-r/26058010#26058010

#Caution: Do not 'print, cat' stuff because it's readed from gui/encoder as results
#it can be printed safely to stderr. See end of this file

write("Arriving at capture.R", stderr())

g = 9.81

#debug = FALSE

filenameCompose <- function(curveNum)
{
	if(curveNum > 99)
		return(paste(filenameBegins, "-", curveNum, sep=""))	#eg. "filename-123"
	else if(curveNum > 9)
		return(paste(filenameBegins, "-0", curveNum, sep=""))	#eg. "filename-023"
	else #(curveNum <= 9)
		return(paste(filenameBegins, "-00", curveNum, sep=""))	#eg. "filename-003"
}

#calcule <- function(displacement, start, end, op, curveNum)
#startInSet: know when this repetition starts in the current set
calcule <- function(displacement, op, curveNum, startInSet)
{
	#if(debug)
	#	write("At calcule", stderr())
	debugParameters(listN(displacement, op, curveNum), "calcule")

	#check displacement1/2 lengths because if it was bad executed,
	#getDisplacementInertialBody maybe returned really small curves that will fail in smooth.spline
	#so just don't do nothing and do not increase the curveNum count
	
	if(length(displacement) < 4)
		return (curveNum)

	#minHeight is checked when ! cut by triggers
	if(! cutByTriggers(op) && abs(sum(displacement)) < op$MinHeight)
		return (curveNum)


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


	#if(debug)
	#	write("At calcule calling kinematics", stderr())
	

	kinematicsResult <- kinematicsF(
				displacement, 
				assignRepOptions(
						 TRUE, NULL, NULL,
						 op$MassBody, op$MassExtra, myEcconKn, op$ExercisePercentBodyWeight,
						 op$EncoderConfigurationName, op$diameter, op$diameterExt, 
						 op$anglePush, op$angleWeight, op$inertiaMomentum, op$gearedDown,
						 ""), #laterality 
				SmoothingsEC, op$SmoothingOneC, g, isPropulsive, TRUE, op$MinHeight)

	paf = data.frame()
	myLaterality = "" #TODO
	paf = pafGenerate(op$Eccon, kinematicsResult, op$MassBody, op$MassExtra, myLaterality, op$inertiaMomentum, op$diameter, op$gearedDown)
		
	position = cumsum(displacement)

	filename <- filenameCompose(curveNum)
	con <- file(filename, "w")
	cat(paste(#start, #start is not used because we have no data of the initial zeros
		  (curveNum +1), startInSet, length(displacement), #title, start, width
		  sum(displacement), #height
		  paf$meanSpeed, paf$maxSpeed, paf$maxSpeedT, 
		  paf$meanPower, paf$peakPower, paf$peakPowerT, paf$pp_ppt, 
		  paf$meanForce, paf$maxForce, paf$maxForceT, paf$maxForce_maxForceT,
		  paf$workJ, 		# C# will convert to Kcal if needed
		  paf$impulse,
		  sep=", "), file = con)
	close(con)
	#if(debug)
	#	write("ended calcule", stderr())

	return(curveNum +1)
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
	

doProcess <- function(options) 
{
	#if(debug)
	#	write("doProcess", stderr())
	op <- assignOptions(options)
			
	DEBUG <<- op$Debug

	#print ("----op----")
	#print (op)

	curveNum = 0
	startInSet = 1

	#Don't measure on first phase (initial eccentric) 
	#inertialCapturingFirstPhase = TRUE
	
	input <- readLines(f, n = 1L)
	while(input[1] != "Q") {
		#if(debug) {
		#	write("doProcess main while", stderr())
		#	write(c("input = ", input), stderr())
		#}
			
		
		#if should continue with another capture
		#then read options again
		while(substring(input,1,4) == "PING") {
			write("capture.R received a continue signal", stderr())

			#answer the ping
			#eg. input = 'PINGC:/Temp.../1234.txt'

			input=substring(input,5,) #input = 'C:/Temp.../1234.txt'
			file.create(input)
			print(paste(input, "created from capture.R"))
			write("created from capture.R", stderr())
			
			options <- scan(optionsFile, comment.char="#", what=character(), sep="\n")
			op <- assignOptions(options)
			DEBUG <<- op$Debug

			curveNum = 0

			#inertialCapturingFirstPhase = TRUE
			input <- readLines(f, n = 1L)
	
			if(input[1] == "Q")
				quit("no") #quit without save
		}
		
		#Sys.sleep(4) #just to test how Chronojump reacts if process takes too long
		#cat(paste("input is:", input, "\n"))

		#-- read the curve (from some lines that finally end on an 'E')
		readingCurve = TRUE
		startInSet <- as.numeric(input)
		input = ""
		print(paste("startInSet: ", startInSet))
		while(readingCurve) {
			inputLine <- readLines(f, n = 1L)
			if(inputLine[1] == "E")
				readingCurve = FALSE
			else
				input = c(input, inputLine)
		}
		#-- curve readed

		
		#if(debug)
		#	write("doProcess input", stderr())
		#write(input, stderr())

		#when data is sent uncompressed
		#displacement = as.numeric(unlist(strsplit(input, " ")))
		#when data is sent compressed
		displacement = uncompress(input)

		#if data file ends with comma. Last character will be an NA. remove it
		#this removes all NAs
		displacement  = displacement[!is.na(displacement)]

		#if(debug)
		#	write("doProcess 2", stderr())
			
		if(isInertial(op$EncoderConfigurationName))
		{
			diametersPerTick = getInertialDiametersPerMs(displacement, op$diameter)
			displacement = getDisplacementInertial(displacement, op$EncoderConfigurationName, 
							       diametersPerTick, op$diameterExt, op$gearedDown)
		} else {
			displacement = getDisplacement(TRUE, op$EncoderConfigurationName, displacement, op$diameter, op$diameterExt, op$gearedDown)
		}
		
		#cut curve by reduceCurveByPredictStartEnd ---->

		start = NULL
		end = NULL
		if(cutByTriggers(op))
		{
			start = 1
			end = length(displacement)
		} else {
			reducedCurve_l <- reduceCurveByPredictStartEnd (displacement,
									op$Eccon, op$MinHeight) #works on c, e, ec
			#if (op$Eccon == "ec")
			#{
				#phases_l <- findECPhases (displacement, op$MinHeight)
				#start <- phases_l$eccentric[1]
				#end <- last(phases_l$concentric)
			#}

			start <- reducedCurve_l$startPos
			end <- reducedCurve_l$endPos
		}

		#reduceCurveBySpeed reduces the curve. Then startInSet has to change:
		print(paste("start:",start))
		startInSet = startInSet + start

		#reduceCurveBySpeed, on inertial doesn't do a good right adjust on changing phase,
		#it adds a value at right, and this value is a descending value that can produce a high acceleration there
		#delete that value
		if( isInertial(op$EncoderConfigurationName))
			end = end -1

		#write("printing reduceTemp2", stderr())
		#write(reduceTemp[2], stderr())
		if(end > length(displacement))
			end = length(displacement)

		displacement = displacement[start:end]

		#<---- cut curve by reduceCurveBySpeed done
		
			
		#if(debug)
		#	write("doProcess 3", stderr())

		position = cumsum(displacement)

		if(isInertial(op$EncoderConfigurationName)) 
		{
			if(abs(max(position) - min(position)) >= op$MinHeight) {
				#Update. Since 1.6.1 on inertial at C# two curves are sent "e" and "c"
				curveNum <- calcule(displacement, op, curveNum, startInSet)
			}
		} else {
			curveNum <- calcule(displacement, op, curveNum, startInSet)
		}

		#if(debug)
		#	write("doProcess 4", stderr())

		input <- readLines(f, n = 1L)
	}

}
		
