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

#options <- getOptionsFromFile(optionsFile, 1)
options <- getOptionsFromFile(optionsFile, 32)


#scriptUtilR = options[1]
scriptUtilR = options[28]
source(scriptUtilR)

#print("Loaded libraries")
	
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
	
	position = cumsum(displacement)
				
	reduceTemp = reduceCurveBySpeed(op$Eccon, 1, 
				      1, 0, #startT, startH
				      displacement, #displacement
				      op$SmoothingOneC #SmoothingOneC
				      )
	start = reduceTemp[1]
	end = reduceTemp[2]
	if(end > length(displacement))
		end = length(displacement)
	
	#print("printing reduceTemp")
	#print(c(start, end, length(displacement)))

	displacement = displacement[start:end]

	#print("reduced")
				
	g = 9.81
		    
	#read AnalysisOptions
	#if is propulsive and rotatory inertial is: "p;ri" 
	#if nothing: "-;-"
	analysisOptionsTemp = unlist(strsplit(op$AnalysisOptions, "\\;"))
	isPropulsive = (analysisOptionsTemp[1] == "p")


	#simplify on capture and have the SmoothingEC == SmoothingC
	SmoothingsEC = op$SmoothingOneC

	#print("pre kinematicsResult")


	kinematicsResult <- kinematicsF(displacement, 
		    op$MassBody, op$MassExtra, op$ExercisePercentBodyWeight,
		    op$EncoderConfigurationName, op$diameter, op$diameterExt, op$anglePush, op$angleWeight, op$inertiaMomentum, op$gearedDown,
		    SmoothingsEC, op$SmoothingOneC, 
		    g, op$Eccon, isPropulsive)
	#print("kinematicsResult")
	#print(kinematicsResult)

	paf = pafGenerate("c", kinematicsResult, op$MassBody, op$MassExtra)
	#print("paf")
	#print(paf)

	#do not use print because it shows the [1] first. Use cat:
	cat(paste(paf$meanSpeed, paf$maxSpeed, paf$maxSpeedT, paf$meanPower, paf$peakPower, paf$peakPowerT, paf$pp_ppt, sep=", "))
	cat("\n") #mandatory to read this from C#, but beware, there we will need a trim to remove the windows \r\n


	input <- readLines(f, n = 1L)
}

write("Ending capture.R", stderr())
quit()
