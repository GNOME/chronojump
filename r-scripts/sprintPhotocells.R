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
#   Copyright (C) 2017   	Xavier Padullés <x.padulles@gmail.com>
#   Copyright (C) 2017,2022   	Xavier de Blas <xaviblas@gmail.com>

#This code uses splitTimes: accumulated time (not lap time)

#-------------- get params -------------
args <- commandArgs(TRUE)

tempPath <- args[1]
optionsFile <- paste(tempPath, "/Roptions.txt", sep="")
pngFile <- paste(tempPath, "/sprintGraph.png", sep="")

#-------------- scan options file -------------
options <- scan(optionsFile, comment.char="#", what=character(), sep="\n")

#-------------- load sprintUtil.R -------------
#options[1] is scriptsPath
source(paste(options[1], "/sprintUtil.R", sep=""))


assignOptions <- function(options) {
        return(list(
                scriptsPath	= options[1],
                os 		= options[2],
                graphWidth 	= as.numeric(options[3]),
                graphHeight	= as.numeric(options[4]),
                positions  	= as.numeric(unlist(strsplit(options[5], "\\;"))),
                splitTimes 	= as.numeric(unlist(strsplit(options[6], "\\;"))),
                mass 	= as.numeric(options[7]),
                personHeight = as.numeric(options[8]),
                personName	= options[9],
                tempC 	= as.numeric(options[10]),
		singleOrMultiple = options[11],
		decimalCharAtExport = options[12],
		includeImagesOnExport = options[13]
        ))
}

#-------------- assign options -------------
op <- assignOptions(options)
#print(op$positions)

#Returns the K and Vmax parameters of the sprint using a number of pairs (time, position)
getSprintFromPhotocell <- function(positions, splitTimes, noise=0)
{
        #noise is for testing purpouses.        
        # TODO: If the photocell is not in the 0 meters we must find how long is the time from
        #starting the race to the reaching of the photocell
        # t0 = 0
        # splitTimes = splitTimes + t0
        
        print("positions:")
        print(positions)
        print("splitTimes:")
        print(splitTimes)
        
        # Checking that time and positions have the same length
        if(length(splitTimes) != length(positions)){
                print("Positions and splitTimes have diferent lengths")
                return()
        }
        
        # For the correct calculation we need at least 2 values in the position and time
        if(length(positions) <= 2){
                print("Not enough data")
                return()
        }
        
	modelSuccess = TRUE
        if (length(positions) == 3){
                #Asuming that the first time and position are 0s it is not necessary to use the non linear regression
                #if there's only three positions. Substituting x1 = x(t1), and x2 = x(t2) whe have an exact solution.
                #2 variables (K and Vmax) and 2 equations.
                model = getSprintFrom2SplitTimes(positions[2], positions[3], splitTimes[2], splitTimes[3], tolerance = 0.0001, initK = 1 )
        } else if (length(positions) >= 4){
                positions = positions[which(positions != 0)]
                splitTimes = splitTimes[which(splitTimes != 0)]

		tryCatch({
			print("calling model")
	                model <- getModelWithOptimalTimeCorrection(data.frame(position = positions, time = splitTimes))
			print("called model")
		}, error=function(cond)
                {
			print("error model")
                        message(cond)
			#following line does not always work, sometimes it does not return and continues with the next of the function, so better just change a variable
			#return(list(modelSuccess = FALSE))
			modelSuccess <<- FALSE #need <<- global assignment operator to ensure its assigned
                })
        }

	if(modelSuccess)
	        return(list(modelSuccess = modelSuccess, K = model$K, Vmax = model$Vmax, T0 = model$T0))
	else
		return(list(modelSuccess = FALSE))
}

#Given x(t) = Vmax*(t + (1/K)*exp(-K*t)) -Vmax - 1/K
# x1 = x(t1)    eq. (1)
# x2 = x(t2)    eq. (2)
#Isolating Vmax from the first expressiona and sustituting in the second one we have:
# x2*(t1 + exp(-K*t1)/K - 1/K) = x1*(t2 + exp(-K*t2)/K -1/K)    eq. (3)
#Passing all the terms of (3) at the left of the equation to have the form y(K) = 0
#Using the iterative Newton's method of the tangent aproximation to find K
#Derivative: y'(K) =  (x2/x1)*(t1 - exp(-K*t1)*(K*t1 + 1)/K^2 + 1/K^2) + exp(-K*t2)*(K*t2 +1) - 1/K^2
getSprintFrom2SplitTimes <- function(x1, x2, t1, t2, tolerance = 0.0001, initK = 1)
{
        #We have to find the K where y = 0.
        K = initK
        y = (x2/x1)*( t1 + exp(-K*t1)/K - 1/K) - t2 - exp(-K*t2)/K + 1/K
        nIterations = 0
        while ((abs(y) > tolerance) && (nIterations < 10000)){
                nIterations = nIterations + 1
                derivY = (x2/x1)*(t1 - exp(-K*t1)*(K*t1 + 1)/K^2 + 1/K^2) + exp(-K*t2)*(K*t2 +1) - 1/K^2
                K = K - y / derivY
                y = (x2/x1)*( t1 + exp(-K*t1)/K - 1/K) - t2 - exp(-K*t2)/K + 1/K
        }
        #Calculing Vmax substituting the K found in the eq. (1)
        Vmax = x1/(t1 + exp(-K*t1)/K -1/K)
        return(list(K = K, Vmax = Vmax))
}

# get the model adjusting the time_correction to the best fit
getModelWithOptimalTimeCorrection <- function(splitTimes)
{
        print("In getModelWithOptimalTimeCorrection()")
        # print(splitTimes)
        bestT0 = 0
        currentT0 = bestT0
        if(length(splitTimes$position) == 3){
                steps = 1
                model = nls(
                        position ~ Vmax*((time + bestT0) + (1/K)*exp(-K*(time + bestT0))) -Vmax/K, splitTimes
                        , start = list(K = 1, Vmax = 8)
                        , control=nls.control(warnOnly=TRUE))
                
                currentError = summary(model)$sigma
                minError = 1E6
                deltaT0 = 0.1
                
                #Looking for the T0 until the error increases
                while(abs(deltaT0) > 0.0001){
                        while( currentError <= minError){
                                bestT0 = currentT0
                                minError = currentError
                                currentT0 = currentT0 + deltaT0
                                steps = steps +1
                                model = nls(
                                        position ~ Vmax*((time + currentT0) + (1/K)*exp(-K*(time + currentT0))) -Vmax/K, splitTimes
                                        , start = list(K = 1, Vmax = 10)
                                        , control=nls.control(warnOnly=TRUE))
                                currentError = summary(model)$sigma
                        }
                        #The error has increased. Inverting the direction and decreasing the deltaT0 size
                        deltaT0 = - deltaT0/10
                        currentT0 =  currentT0 + deltaT0
                        model = nls(
                                position ~ Vmax*((time + currentT0) + (1/K)*exp(-K*(time + currentT0))) -Vmax/K, splitTimes
                                , start = list(K = 1, Vmax = 10)
                                , control=nls.control(warnOnly=TRUE))
                        
                        currentError = summary(model)$sigma
                        minError = currentError
                }
                model = nls(
                        position ~ Vmax*((time + bestT0) + (1/K)*exp(-K*(time + bestT0))) -Vmax/K, splitTimes
                        , start = list(K = 1, Vmax = 10)
                        , control=nls.control(warnOnly=TRUE))

                
        } else if(length(splitTimes$position) >= 3){
                model = nls(
                        position ~ Vmax*((time + T0) + (1/K)*exp(-K*(time + T0))) -Vmax/K, splitTimes
                        , start = list(K = 1, Vmax = 10, T0 = 0.2)
                        , control=nls.control(warnOnly=TRUE))
                bestT0 = summary(model)$parameters[3]
        }
        
        # print("### With optimal correction ###")
        # print(paste("Time correction:", bestT0))
        # print(model)
        
        return(list(K = summary(model)$parameters[1],  Vmax = summary(model)$parameters[2], T0 = bestT0))
}

drawSprintFromPhotocells <- function(sprintFittedDynamics, splitTimes, positions, splitPositionAll, title, plotFittedSpeed = T, plotFittedAccel = T, plotFittedForce = T, plotFittedPower = T)
{
	#return if fitted values of a or fmax are <= 0, it would make fail the seq by operator
	#done it before start drawing to avoid png being created
	if(max(sprintFittedDynamics$a.fitted) <= 0)
		return (NULL)
	if(max(sprintFittedDynamics$fmax.fitted) <= 0)
		return (NULL)
        T0 = sprintFittedDynamics$T0
        maxTime = splitTimes[length(splitTimes)]
        time = seq(0, maxTime, by=0.01)
        #Calculating measured average speeds
        avg.speeds = diff(positions)/diff(splitTimes)
        textXPos = splitTimes[1:length(splitTimes) - 1] + diff(splitTimes)/2

        xlims = c(-sprintFittedDynamics$T0, splitTimes[length(splitTimes)])
        
        # Plotting average speed
        par(mar = c(7, 4, 5, 7.5))
        
        barColors = "grey"
        for(i in 2:length(avg.speeds)) {
                if (avg.speeds[i] - avg.speeds[i-1] < 0) barColors = c(barColors, "red")
                else barColors = c("grey", barColors)
        }
        barplot(height = avg.speeds, width = diff(splitTimes), space = 0,
                ylim = c(0, max(c(avg.speeds, sprintFittedDynamics$Vmax) + 1)), xlim = xlims,
                col = barColors,
                main=title,
                #sub = substitute(v(t) == Vmax*(1-e^(-K*t)), list(Vmax="Vmax", K="K")),
                xlab="Time[s]", ylab="Velocity[m/s]",
                axes = FALSE, yaxs= "i", xaxs = "i")
        text(textXPos, avg.speeds, round(avg.speeds, digits = 2), pos = 3)

        axis(3, at = c(-sprintFittedDynamics$T0,splitTimes), labels = c(round(-sprintFittedDynamics$T0, digits = 3),splitTimes))
        
        # Fitted speed plotting
        par(new=T)
        plot((sprintFittedDynamics$t.fitted - sprintFittedDynamics$T0), sprintFittedDynamics$v.fitted, type = "l", xlab="", ylab = "",
             ylim = c(0, max(c(avg.speeds, sprintFittedDynamics$Vmax) + 1)), xlim = xlims,
             yaxs= "i", xaxs = "i", axes = F) # Fitted data
        axis(2, at = seq(0, sprintFittedDynamics$Vmax + 1, by = 1))
        abline(h = sprintFittedDynamics$Vmax, lty = 2)
        mtext(side = 1, line = 3, at = splitTimes[length(splitTimes)]*0.25, cex = 1.5 , substitute(v(t) == Vmax*(1-e^(-K*t)), list(Vmax="Vmax", K="K")))
        
        if(plotFittedPower)
        {
                lines(c(-T0, sprintFittedDynamics$tpmax.fitted -T0), c(sprintFittedDynamics$vpmax.fitted, sprintFittedDynamics$vpmax.fitted)
                , col = "black", lty = 3, lwd = 2)
                text(-T0, sprintFittedDynamics$vpmax.fitted , round(sprintFittedDynamics$vpmax.fitted, 2)
                     , col = "black", adj = c(-0.1, -0.5))
        }
        
        if(plotFittedAccel)
        {
                par(new = T)
                plot((sprintFittedDynamics$t.fitted - sprintFittedDynamics$T0), sprintFittedDynamics$a.fitted, type = "l", col = "magenta", yaxs= "i", xaxs = "i", xlab="", ylab = "",
                     ylim=c(0,sprintFittedDynamics$amax.fitted), xlim = xlims,
                     axes = FALSE )
		axis(side = 4, col ="magenta", at = seq(0,max(sprintFittedDynamics$a.fitted), by = 1))
        }
        
        #Force plotting
        if(plotFittedForce)
        {
                par(new=T)
                plot((sprintFittedDynamics$t.fitted - sprintFittedDynamics$T0), sprintFittedDynamics$f.fitted, type="l", col="blue", yaxs= "i", xaxs = "i", xlab="", ylab="",
                     ylim=c(0,sprintFittedDynamics$fmax.fitted), xlim = xlims,
                     axes = FALSE)
		axis(line = 2.5, side = 4, col ="blue", at = seq(0, sprintFittedDynamics$fmax.fitted + 100, by = 100))
        }

        #Power plotting
        if(plotFittedPower)
        {
                par(new=T)
                plot((sprintFittedDynamics$t.fitted - sprintFittedDynamics$T0), sprintFittedDynamics$p.fitted, type="l", axes = FALSE, xlab="", ylab="", col="red"
                     , ylim=c(0,sprintFittedDynamics$pmax.fitted + .1 * sprintFittedDynamics$pmax.fitted), xlim = xlims
                     , yaxs= "i", xaxs = "i")
                abline(v = (sprintFittedDynamics$tpmax.fitted- sprintFittedDynamics$T0), col="red", lty = 2)
                axis(line = 5, side = 4, col ="red", at = seq(0, sprintFittedDynamics$pmax.fitted, by = 200))
                axis(3, at = sprintFittedDynamics$tpmax.fitted- sprintFittedDynamics$T0, labels = round(sprintFittedDynamics$tpmax.fitted, 3))
                #text(sprintFittedDynamics$tpmax.fitted, sprintFittedDynamics$pmax.fitted, paste("Pmax fitted =", round(sprintFittedDynamics$pmax.fitted, digits = 2)),  pos = 3)
                # mtext(side = 1, line = 5, at = splitTimes[length(splitTimes)]*0.75, cex = 1.5,
                #       substitute(P(t) == A*e^(-K*t)*(1-e^(-K*t)) + B*(1-e^(-K*t))^3,
                #                         list(A=round(sprintFittedDynamics$Vmax.fitted^2*sprintFittedDynamics$Mass, digits=3),
                #                              B = round(sprintFittedDynamics$Vmax.fitted^3*sprintFittedDynamics$Ka, digits = 3),
                #                              Vmax=round(sprintFittedDynamics$Vmax.fitted, digits=3),
                #                              K=round(sprintFittedDynamics$K.fitted, digits=3)))
                #       , col ="red")
                
                
        }
        
        legend (x = time[length(time)], y = sprintFittedDynamics$pmax.fitted / 2,
                xjust = 1, yjust = 0.5, cex = 1,
                legend = c(paste("K =", round(sprintFittedDynamics$K.fitted, digits = 2)),
                           paste("\u03C4 =", round(1/sprintFittedDynamics$K.fitted, digits = 2), "s"),
                           paste("Vmax =", round(sprintFittedDynamics$Vmax.fitted, digits = 2), "m/s"),
                           paste("Amax =", round(sprintFittedDynamics$amax.fitted, digits = 2), "m/s\u00b2"),
                           paste("fmax =", round(sprintFittedDynamics$fmax.rel.fitted, digits = 2), "N/kg"),
                           paste("pmax =", round(sprintFittedDynamics$pmax.rel.fitted, digits = 2), "W/kg"),
                           paste("Vpmax =", round(sprintFittedDynamics$vpmax.fitted, digits = 2), "m/s")),
                text.col = c("black", "black", "black", "magenta", "blue", "red", "black"))

	#TODO: check if we can pass raw values of v,a,F,P
        return (exportSprintDynamicsPrepareRow(sprintFittedDynamics, NULL, splitTimes, positions, splitPositionAll, op$decimalCharAtExport == ","))
}

testPhotocellsCJ <- function(positions, splitTimes, splitPositionAll, mass, personHeight, tempC, personName)
{
        sprint = getSprintFromPhotocell(position = positions, splitTimes = splitTimes)
        print(sprint)
	if(! sprint$modelSuccess)
	{
		print("no model success")
		return (NULL)
	}

        sprintFittedDynamics = getDynamicsFromSprint(K = sprint$K, Vmax = sprint$Vmax, T0 = sprint$T0
                                               , Mass = mass
                                               , Temperature = tempC
                                               , Height = personHeight
                                               , maxTime = max(splitTimes))
        print(paste("K =",sprintFittedDynamics$K.fitted, "Vmax =", sprintFittedDynamics$Vmax.fitted))

	if(is.null(sprintFittedDynamics))
		return (NULL)
	else
	        return(drawSprintFromPhotocells(sprintFittedDynamics = sprintFittedDynamics, splitTimes, positions, splitPositionAll, title = personName))
}

#----- execute code

start <- function(op)
{
	if(op$singleOrMultiple == "TRUE")
	{
		prepareGraph(op$os, pngFile, op$graphWidth, op$graphHeight)
		exportRow = testPhotocellsCJ(op$positions, op$splitTimes, NULL, op$mass, op$personHeight, op$tempC, op$personName)
		if(is.null(exportRow))
		{
			plot(0,0,type="n",axes=F,xlab="",ylab="")
			text(x=0, y=0, adj=0.5, cex=1.2, col="red", "This data does not seem a sprint.")
		} else
		{
			exportSprintDynamicsWriteRow (exportRow)
		}

		endGraph()
		return()
	}

	# ------------------ op$singleOrMultiple == "FALSE" ------------------------->

	#2) read the csv
	dataFiles = read.csv(file = paste(tempPath, "/sprintInputMulti.csv", sep=""), sep=";", stringsAsFactors=F)

	#3) call testPhotocelssCJ
	progressFolder = paste(tempPath, "/chronojump_export_progress", sep ="")
	tempGraphsFolder = paste(tempPath, "/chronojump_sprint_export_graphs/", sep ="")
	exportDF = NULL

	#find the colums needed for different split position values
        splitPositionAll = NULL
	for(i in 1:length(dataFiles[,1]))
	{
		splits = as.numeric(unlist(strsplit(as.character(dataFiles$positions[i]), "\\_")))
		for(j in 1:length(splits))
			splitPositionAll = c(splitPositionAll, splits[j])
	}
	splitPositionAll = sort(unique(splitPositionAll))

	for(i in 1:length(dataFiles[,1]))
	{
		print("splitTimes at for: ")
		print(as.numeric(unlist(strsplit(as.character(dataFiles$splitTimes[i]), "\\_"))))

		pngFile <- paste(tempGraphsFolder, i, ".png", sep="")  #but remember to graph also when model fails

		prepareGraph(op$os, pngFile, op$graphWidth, op$graphHeight)
		exportRow = testPhotocellsCJ(
					     as.numeric(unlist(strsplit(as.character(dataFiles$positions[i]), "\\_"))),
					     as.numeric(unlist(strsplit(as.character(dataFiles$splitTimes[i]), "\\_"))),
					     splitPositionAll,
					     dataFiles$mass[i],
					     dataFiles$personHeight[i], dataFiles$tempC[i], dataFiles$personName[i])

		if(! is.null(exportRow))
		{
			names = names(exportRow) #exportRow is a list, get the names
			exportRow = unlist(exportRow) #convert to a vector

			exportRowDF = data.frame(dataFiles$personName[i]) #create dataframe for this row with some columns
			#add exportRow data (this way we solve problems of adding strings with numbers without converting the numbers to strings
			#(to control if we print them as , or .)
			for(j in 1:length(exportRow))
				exportRowDF = cbind (exportRowDF, exportRow[j])
			if(op$includeImagesOnExport)
				exportRowDF = cbind(exportRowDF, paste(i, ".png", sep=""))

			#write the correct names of the row dataframe
			namesDF = c("Person",names)
			if(op$includeImagesOnExport)
				namesDF = c(namesDF, "Image")
			colnames(exportRowDF) = namesDF

			exportDF <- rbind (exportDF, exportRowDF) #rbind with exportDF

			endGraph() #we only call dev.off on ! is.null(exportRow)
		}

		progressFilename = paste(progressFolder, "/", i, sep="")
		file.create(progressFilename)
	}

	#write the data frame
	#na="" to not print NA on empty comments
	if(op$decimalCharAtExport == ".")
		write.csv(exportDF, file = paste(tempPath, "/sprintResults.csv", sep = ""), row.names = FALSE, na="")
	else if(op$decimalCharAtExport == ",")
		write.csv2(exportDF, file = paste(tempPath, "/sprintResults.csv", sep = ""), row.names = FALSE, na="")
}


start(op)

#Examples of use

#testPhotocells <- function()
#{
#	Vmax = 9.54709925453619
#	K = 0.818488730889454
#	noise = 0
#	splitTimes = seq(0,10, by=1)
#	#splitTimes = c(0, 1, 5, 10)
#	positions = Vmax*(splitTimes + (1/K)*exp(-K*splitTimes)) -Vmax/K
#	photocell.noise = data.frame(time = splitTimes + noise*rnorm(length(splitTimes), 0, 1), position = positions)
#	sprint = getSprintFromPhotocell(position = photocell.noise$position, splitTimes = photocell.noise$time)
#	sprintFittedDynamics = getDynamicsFromSprint(K = sprint$K, Vmax = sprint$Vmax, 75, 25, 1.65)
#	print(paste("K =",sprintFittedDynamics$K.fitted, "Vmax =", sprintFittedDynamics$Vmax.fitted))
#	drawSprintFromPhotocells(sprintFittedDynamics = sprintFittedDynamics, splitTimes, positions, title = "Testing graph")
#}

#Test wiht data like is coming from Chronojump
#testPhotocellsCJSample <- function()
#{
#	#Data coming from Chronojump. Example: Usain Bolt
#	positions  = c(0, 20   , 40   , 70   )
#	splitTimes = c(0,  2.73,  4.49,  6.95)
#	mass = 75
#	tempC = 25
#	personHeight = 1.65
#
#	sprint = getSprintFromPhotocell(position = positions, splitTimes = splitTimes)
#	sprintFittedDynamics = getDynamicsFromSprint(K = sprint$K, Vmax = sprint$Vmax, mass, tempC, personHeight, maxTime = max(splitTimes))
#	print(paste("K =",sprintFittedDynamics$K.fitted, "Vmax =", sprintFittedDynamics$Vmax.fitted))
#	drawSprintFromPhotocells(sprintFittedDynamics = sprintFittedDynamics, splitTimes, positions, title = "Testing graph")
#}
