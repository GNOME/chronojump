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
		
	if(isInertial(op$EncoderConfigurationName)) 
	{
		displacement = fixDisplacementInertial(displacement, op$EncoderConfigurationName, op$diameter, op$diameterExt)

		displacement = getDisplacementInertialBody(displacement, FALSE, op$Title) #draw: FALSE
	} else {
		displacement = getDisplacement(op$EncoderConfigurationName, displacement, op$diameter, op$diameterExt)
	}
	
	write(displacement,stderr())
	position = cumsum(displacement)
	write(position,stderr())
		
	#curves=findCurves(displacement, op$Eccon, op$MinHeight, FALSE, op$Title) #draw: FALSE
	#print("curves",stderr())
	#print(curves,stderr())
				
	if( ! isInertial(op$EncoderConfigurationName)) {
		reduceTemp = reduceCurveBySpeed(op$Eccon, 1, 
						#curves[1,1], curves[1,3], #startT, startH
						1, 0, #startT, startH
						displacement, #displacement
						op$SmoothingOneC #SmoothingOneC
						)
				
		#curves[1,1] = reduceTemp[1]
		#curves[1,2] = reduceTemp[2]
		#curves[1,3] = reduceTemp[3]
	}
		
#	SmoothingsEC = findSmoothingsEC(singleFile, displacement, curves, op$Eccon, op$SmoothingOneC)
#	write(c("SmoothingsEC:",SmoothingsEC),stderr())
	
	write("curves after reduceCurveBySpeed",stderr())
	#write(as.vector(curves[1,]),stderr())

	start = reduceTemp[1]
	end = reduceTemp[2]
	#write("printing reduceTemp2", stderr())
	#write(reduceTemp[2], stderr())
	if(end > length(displacement))
		end = length(displacement)
	
	#write("printing reduceTemp", stderr())
	#write(c(start, end, length(displacement)),stderr())

	#displacement = displacement[start:end]

	#print("reduced")

	#if(curves[1,2] > length(displacement)) 
	#	curves[1,2] = length(displacement)
				
	g = 9.81
		    
	#read AnalysisOptions
	#if is propulsive and rotatory inertial is: "p;ri" 
	#if nothing: "-;-"
	analysisOptionsTemp = unlist(strsplit(op$AnalysisOptions, "\\;"))
	isPropulsive = (analysisOptionsTemp[1] == "p")


	#simplify on capture and have the SmoothingEC == SmoothingC
	SmoothingsEC = op$SmoothingOneC

	#print("pre kinematicsResult")


	#kinematicsResult <- kinematicsF(displacement[curves[1,1]:curves[1,2]], 
	kinematicsResult <- kinematicsF(displacement, 
		    op$MassBody, op$MassExtra, op$ExercisePercentBodyWeight,
		    op$EncoderConfigurationName, op$diameter, op$diameterExt, op$anglePush, op$angleWeight, op$inertiaMomentum, op$gearedDown,
		    #SmoothingsEC[1], op$SmoothingOneC, 
		    SmoothingsEC, op$SmoothingOneC, 
		    g, op$Eccon, isPropulsive)
	#print("kinematicsResult")
	#print(kinematicsResult)

	paf = data.frame()
	paf = pafGenerate(op$Eccon, kinematicsResult, op$MassBody, op$MassExtra)
	#write("paf",stderr())
	#write(paf,stderr())

	#do not use print because it shows the [1] first. Use cat:
	cat(paste(#start, #start is not used because we have no data of the initial zeros
		  (end-start), (position[end]-position[start]),
		  #curves[1,2]-curves[1,1], position[curves[1,2]]-curves[1,3],
		  paf$meanSpeed, paf$maxSpeed, paf$maxSpeedT, paf$meanPower, paf$peakPower, paf$peakPowerT, paf$pp_ppt, sep=", "))
	cat("\n") #mandatory to read this from C#, but beware, there we will need a trim to remove the windows \r\n


	input <- readLines(f, n = 1L)
}

write("Ending capture.R", stderr())
quit()
