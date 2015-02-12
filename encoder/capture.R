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
		    

calcule <- function(displacement, op) 
{
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


	kinematicsResult <- kinematicsF(displacement, 
		    op$MassBody, op$MassExtra, op$ExercisePercentBodyWeight,
		    op$EncoderConfigurationName, op$diameter, op$diameterExt, op$anglePush, op$angleWeight, op$inertiaMomentum, op$gearedDown,
		    SmoothingsEC, op$SmoothingOneC, 
		    g, myEcconKn, isPropulsive)

	paf = data.frame()
	paf = pafGenerate(op$Eccon, kinematicsResult, op$MassBody, op$MassExtra)
		
	position = cumsum(displacement)

	#do not use print because it shows the [1] first. Use cat:
	cat(paste(#start, #start is not used because we have no data of the initial zeros
		  (end-start), (position[end]-position[start]),
		  paf$meanSpeed, paf$maxSpeed, paf$maxSpeedT, paf$meanPower, paf$peakPower, paf$peakPowerT, paf$pp_ppt, sep=", "))
	cat("\n") #mandatory to read this from C#, but beware, there we will need a trim to remove the windows \r\n
}
	
input <- readLines(f, n = 1L)
while(input[1] != "Q") {
	#Sys.sleep(4) #just to test how Chronojump reacts if process takes too long
	#cat(paste("input is:", input, "\n"))
	
	op <- assignOptions(options)

	#print ("----op----")
	#print (op)

	displacement = as.numeric(unlist(strsplit(input, " ")))
	#if data file ends with comma. Last character will be an NA. remove it
	#this removes all NAs
	displacement  = displacement[!is.na(displacement)]
		
	if(isInertial(op$EncoderConfigurationName)) 
	{
		displacement = fixDisplacementInertial(displacement, op$EncoderConfigurationName, op$diameter, op$diameterExt)

		displacement = getDisplacementInertialBody(displacement, FALSE, op$Title) #draw: FALSE
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

	#if isInertial: getDisplacementInertialBody separate phases using initial height of full extended person
	#so now there will be two different curves to process
	if(isInertial(op$EncoderConfigurationName)) 
	{
		position = cumsum(displacement)
		positionBottom <- floor(mean(which(position == min(position))))
		displacement1 = displacement[1:positionBottom]
		calcule(displacement1, op)

		if( (positionBottom +1) < length(displacement)){
			displacement2 = displacement[(positionBottom+1):length(displacement)]
			calcule(displacement2, op)
		}
		write(c("positionBottom", positionBottom), stderr())
		write(c("length(displacement)", length(displacement)), stderr())
	} else {
		calcule(displacement, op)
	}

	input <- readLines(f, n = 1L)
}
		


write("Ending capture.R", stderr())
quit()
