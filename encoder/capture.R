#http://stackoverflow.com/questions/26053302/is-there-a-way-to-use-standard-input-output-stream-to-communicate-c-sharp-and-r/26058010#26058010

#Caution: Do not 'print, cat' stuff because it's readed from gui/encoder as results

#cat("Arrived at capture.R\n")

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

options <- getOptionsFromFile(optionsFile, 1)


scriptUtilR = options[1]

source(scriptUtilR)

#print("Loaded libraries")
	
input <- readLines(f, n = 1L)
while(input[1] != "Q") {
	#Sys.sleep(4) #just to test how Chronojump reacts if process takes too long
	#cat(paste("input is:", input, "\n"))

	displacement = as.numeric(unlist(strsplit(input, " ")))
	#if data file ends with comma. Last character will be an NA. remove it
	#this removes all NAs
	displacement  = displacement[!is.na(displacement)]
	
	position = cumsum(displacement)
				
	reduceTemp = reduceCurveBySpeed("c", 1, 
				      1, 0, #startT, startH
				      displacement, #displacement
				      .7 #SmoothingOneC
				      )
	start = reduceTemp[1]
	end = reduceTemp[2]
	if(end > length(displacement))
		end = length(displacement)
	
	#print("printing reduceTemp")
	#print(c(start, end, length(displacement)))

	displacement = displacement[start:end]

	#print("reduced")
				
	myMassBody = 50
	myMassExtra = 100
	myExPercentBodyWeight = 0
	myEncoderConfigurationName = "LINEAR"
	myDiameter = 0
	myDiameterExt = 0
	myAnglePush = 90
	myAngleWeight = 90
	myInertiaMomentum = 0
	myGearedDown = 0
	SmoothingsEC = 0.7
	SmoothingOneC = 0.7
	g = 9.81
	myEcconKn = "c" #in ec can be "e" or "c"
	isPropulsive = TRUE
	
	#print("pre kinematicsResult")

	kinematicsResult <- kinematicsF(displacement, 
		    myMassBody, myMassExtra, myExPercentBodyWeight,
		    myEncoderConfigurationName,myDiameter,myDiameterExt,myAnglePush,myAngleWeight,myInertiaMomentum,myGearedDown,
		    SmoothingsEC,SmoothingOneC, 
		    g, myEcconKn, isPropulsive)
	#print("kinematicsResult")
	#print(kinematicsResult)

	paf = pafGenerate("c", kinematicsResult, myMassBody, myMassExtra)
	#print("paf")
	#print(paf)

	#do not use print because it shows the [1] first. Use cat:
	cat(paste(paf$meanSpeed, paf$maxSpeed, paf$maxSpeedT, paf$meanPower, paf$peakPower, paf$peakPowerT, paf$pp_ppt, sep=", "))
	cat("\n") #mandatory to read this from C#, but beware, there we will need a trim to remove the windows \r\n


	input <- readLines(f, n = 1L)
}
#cat("Ending capture.R\n")
quit()
