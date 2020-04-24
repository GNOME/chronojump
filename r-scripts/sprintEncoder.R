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
#   Copyright (C) 2018-2020   	Xavier Padullés <x.padulles@gmail.com>
#   Copyright (C) 2020   	Xavier de Blas <xaviblas@gmail.com>


#-------------- get params -------------
args <- commandArgs(TRUE)

tempPath <- args[1]
optionsFile <- paste(tempPath, "/Roptions.txt", sep="")
pngFile <- paste(tempPath, "/sprintEncoderGraph.png", sep="")

#-------------- scan options file -------------
options <- scan(optionsFile, comment.char="#", what=character(), sep="\n")

#-------------- load sprintUtil.R -------------
#options[1] is scriptsPath
source(paste(options[1], "/sprintUtil.R", sep=""))
source(paste(options[1], "/scripts-util.R", sep=""))

assignOptions <- function(options) {
        return(list(
                scriptsPath	= options[1],
                filename  	= options[2],
                mass 	= as.numeric(options[3]),
                personHeight = as.numeric(options[4]),
                tempC 	= as.numeric(options[5]),
                testLength = as.numeric(options[6]),
                os 		= options[7],
                graphWidth 	= as.numeric(options[8]),
                graphHeight	= as.numeric(options[9]),
                device  	= options[10],
                title 	 	= options[11],
                datetime 	= options[12],
                startAccel 	= options[13],
                plotRawAccel 	= as.logical(options[14]),
                plotFittedAccel = as.logical(options[15]),
                plotRawForce 	= as.logical(options[16]),
                plotFittedForce = as.logical(options[17]),
                plotRawPower 	= as.logical(options[18]),
                plotFittedPower = as.logical(options[19]),
		triggersOnList  = as.numeric(unlist(strsplit(options[20], "\\;"))),
		triggersOffList  = as.numeric(unlist(strsplit(options[21], "\\;")))
        ))
}

#-------------- assign options -------------
op <- assignOptions(options)

op$title = fixTitleAndOtherStrings(op$title)
op$datetime = fixDatetime(op$datetime)

getSprintFromEncoder <- function(filename, testLength, Mass, Temperature = 25, Height , Vw = 0, device = "MANUAL", startAccel)
{
        print("#####Entering in getSprintFromEncoder###############")
        # Constants for the air friction modeling
        ro0 = 1.293
        Pb = 760
        Cd = 0.9
        ro = ro0*(Pb/760)*273/(273 + Temperature)
        Af = 0.2025*(Height^0.725)*(Mass^0.425)*0.266 # Model of the frontal area
        Ka = 0.5*ro*Af*Cd
        
        raceAnalyzer = read.csv2(file = filename, sep = ";")
        colnames(raceAnalyzer) = c("displacement", "time", "force")
        raceAnalyzer$force = (raceAnalyzer$force) * 0.175806451613 /2  #TODO: Implement the calibration factor comming from the arduino
        totalTime = raceAnalyzer$time/1E6     #Converting microseconds to seconds
        elapsedTime = diff(totalTime)      #The elapsed time between each sample
        #raceAnalyzer = raceAnalyzer[2:length(raceAnalyzer)]
        #totalTime = totalTime[1:length(totalTime)]
        
        #The encoder can be used with both loose ends of the rope. This means that the encoder can rotate
        #in both directions. We consider always the speed as positive.
        if(mean(raceAnalyzer$displacement) < 0)
                raceAnalyzer$displacement = -raceAnalyzer$displacement
        
        #TODO: measure metersPerPulse several times to have an accurate value
	metersPerPulse = NULL
	if(device == "MANUAL")                 #manual race analyzer - Hand device
	        metersPerPulse = 0.003003
	else                                    #resisted race analyzer - sled device
	        metersPerPulse = 4 * 30 / 12267 	#With an encoder of 200 ppr, in 30m there are 12267 pulses.

        raceAnalyzer$displacement = raceAnalyzer$displacement * metersPerPulse
        position = cumsum(raceAnalyzer$displacement)
        speed = raceAnalyzer$displacement[2:length(raceAnalyzer$displacement)] / elapsedTime
        
        #Adjusting the time of each sample to the mean time between two samples
        for( i in 2:length(totalTime)){
                totalTime[i] = totalTime[i] - elapsedTime[i-1] / 2
        }
        
        #Accel of the sample N is the mean accel between N-1 and N+1 samples.
        #The time of the accel sample is the same as the speed sample.
        #TODO. Adjust the time at which this accel is measured.
        accel = speed[2] / totalTime[2]
        for(i in 3:(length(speed) -1))
        {
                accel = c(accel, (speed[i+1] - speed[i-1]) / (totalTime[i +1] - totalTime[i -1]))
        }
        accel = c(accel, accel[length(accel)])
        forceBody = accel * Mass + Ka*(speed - Vw)^2
        totalForce = forceBody + raceAnalyzer$force
        power = totalForce * speed
        # print("totalTime:")
        # print(totalTime)
        # print("time:")
        # print(raceAnalyzer$time)
        # print("elapsedTime:")
        # print(elapsedTime)
        # print("position:")
        # print(position)
        # print("speed:")
        # print(speed)
        # print("accel:")
        # print(accel)
        # print("forceBody:")
        # print(forceBody)
        # print("forceRope:")
        # print(raceAnalyzer$force)
        # print("power:")
        # print(power)
        
        
        # #Checking if the sprint is long enough
        # longEnough = TRUE
        # if(position[length(position)] <= testLength){
        #         longEnough = FALSE
        #         return(list(longEnough = longEnough))
        # }
        
        #Checking that the position reaches at least testLength
        if(max(position) < testLength)
        {
                plot(totalTime, position, type = "l",
                     ylim = c(min(position), testLength)*1.05,
                     xlab = "Time (s)", ylab = "Position (m)")
                abline(h = testLength, lty = 2)
                text(x = (totalTime[length(totalTime)] + totalTime[1])/2,
                     y = testLength,
                     labels = (paste("Configured test length :", testLength, " m", sep = "")),
                     pos = 3)
                text(x = (totalTime[length(totalTime)] + totalTime[1])/2, testLength /2,
                     labels = "The capture does not reach the length of the test", cex = 2, pos = 3)
                print("Capture too short")
                longEnough = FALSE
                return(list(longEnough = longEnough))
        } else{
                longEnough = TRUE
        }
        
        #Finding when the sprint starts
        trimmingSamples = getTrimmingSamples(totalTime, position, speed, accel, testLength, startAccel)
        print(trimmingSamples)

	#plot error if no enough acceleration
	if(trimmingSamples$start == -1 & trimmingSamples$end == -1)
	{
		plot(position, type = "l", ylab = "Position (m)")
                text(x = length(position)/2, y = max(position)/2,
                     labels = "The capture has not enough accel", cex = 2, pos = 3)
                text(x = length(position)/2, y = max(position)/3,
                     labels = paste("Max raw detected accel: ", round(max(accel),2), "\nMinimum raw accel needed: ", startAccel), cex = 2, pos = 3)
                text(x = length(position)/2, y = max(position)/4,
                     labels = "or does not seem a sprint", cex = 2, pos = 3)
		print("Capture has not enough accel")
		return("Capture has not enough accel")
	}

        #Zeroing time to the initial acceleration sample
        time = totalTime - totalTime[trimmingSamples$start]
        #Zeroing position to the initial acceleration sample
        position = position - position[trimmingSamples$start]
        
        #generating an initial speed of zero
        #1. Find the line defined by the two first samples
        #2. Look for the cross with the X axis.
        #3. X of the cross is the time whe need to add to all the samples

        timeBefore = speed[trimmingSamples$start] * ((time[trimmingSamples$start + 1]) / (speed[trimmingSamples$start + 1] - speed[trimmingSamples$start]))
        time = time + timeBefore
        data = data.frame(time = c(0,time[trimmingSamples$start:trimmingSamples$end]), speed = c(0,speed[trimmingSamples$start:trimmingSamples$end]))
        #print(data)
        
        print("Trying nls")
        regression = tryNLS(data)
        
        print("regression:")
        print(regression)
        print(paste("longEnough:", longEnough))
        print(paste("regressionDone:", regression$regressionDone))
        
        if (! regression$regressionDone)
        {
                print("NLS regression problem")
                plot(totalTime[2:length(totalTime)], speed, type = "l",
                     #ylim = c(min(speed), testLength)*1.05,
                     xlab = "Time (s)", ylab = "Speed (m/s)")
                #abline(h = testLength, lty = 2)
                # text(x = (totalTime[length(totalTime)] + totalTime[1])/2,
                #      y = testLength,
                #      labels = (paste("Configured test length :", testLength, " m", sep = "")),
                #      pos = 3)
                text(x = (totalTime[length(totalTime)] + totalTime[1])/2, max(speed) /2,
                     labels = "The graph doesn't seem a sprint", cex = 2, pos = 3)
                return(list(longEnough = longEnough, regressionDone = regression$regressionDone))
        }
        
                
        #model = nls(speed ~ Vmax*(1-exp(-K*time)), data,
        #            start = list(Vmax = max(speed), K = 1), control=nls.control(warnOnly=TRUE))
        Vmax =summary(regression$model)$coeff[1,1]
        K = summary(regression$model)$coeff[2,1]
        return(list(Vmax = Vmax, K = K,
                    time = time, rawPosition = position, rawSpeed = speed, rawAccel = accel, rawForce = totalForce, rawPower = power,
                    rawVmax = max(speed[trimmingSamples$start:trimmingSamples$end]), rawAmax = max(accel[trimmingSamples$start:trimmingSamples$end]), rawFmax = max(totalForce[trimmingSamples$start:trimmingSamples$end]), rawPmax = max(power[trimmingSamples$start:trimmingSamples$end]),
                    startSample = trimmingSamples$start, endSample = trimmingSamples$end, testLength = testLength, longEnough = longEnough, regressionDone = regression$regressionDone, timeBefore = timeBefore, startAccel = startAccel))
}

plotSprintFromEncoder <- function(sprintRawDynamics, sprintFittedDynamics,
				  title = "Test graph",
				  subtitle = "",
				  triggersOn = "",
				  triggersOff = "",
                                  plotRawMeanSpeed = TRUE,
                                  plotRawSpeed = TRUE,
				  plotRawAccel = op$plotRawAccel,
				  plotRawForce = op$plotRawForce,
                                  plotMeanRawForce = TRUE,
				  plotRawPower = op$plotRawPower,
                                  plotMeanRawPower = TRUE,
                                  plotFittedSpeed = TRUE,
				  plotFittedAccel = op$plotFittedAccel,
				  plotFittedForce = op$plotFittedForce,
				  plotFittedPower = op$plotFittedPower,
                                  startAccel,
                                  plotStartDetection = TRUE)
{
        #Plotting position
        # plot(sprintRawDynamics$time[sprintRawDynamics$startSample:sprintRawDynamics$endSample], sprintRawDynamics$rawPosition[sprintRawDynamics$startSample:sprintRawDynamics$endSample],
        #      main = paste(50, "PPR"), xlab = "Time (s)", ylab = "Position (m)", type = "l", yaxs= "i", xaxs = "i")
        # raceTime = interpolateXAtY(sprintRawDynamics$time, sprintRawDynamics$rawPosition, sprintRawDynamics$testLength)
        # abline(v = raceTime)
        # abline(h = sprintRawDynamics$testLength, lty = 3)
        # points(raceTime, sprintRawDynamics$testLength)
        # mtext(side = 3, at = raceTime, text = paste(sprintRawDynamics$testLength, " m", sep=""))
        # mtext(side = 1, at = raceTime, text = paste(round(raceTime, digits = 3), " s", sep=""))

        print("#######Entering plotSprintFromEncoder###########")
        par(mar = c(7, 4, 5, 6.5))
        print("plotRawAccel")
        print(plotRawAccel)
        print(typeof(plotRawAccel))
        
        #Checking that the position reaches at least testLength
        if(max(sprintRawDynamics$rawPosition) < sprintRawDynamics$testLength)
        {
                plot(sprintRawDynamics$time, sprintRawDynamics$rawPosition, type = "l",
                     ylim = c(min(sprintRawDynamics$rawPosition), sprintRawDynamics$testLength)*1.05,
                     xlab = "Time (s)", ylab = "Position (m)")
                abline(h = sprintRawDynamics$testLength, lty = 2)
                text(x = (sprintRawDynamics$time[length(sprintRawDynamics$time)] + sprintRawDynamics$time[1])/2,
                     y = sprintRawDynamics$testLength,
                     labels = (paste("Configured test length :", sprintRawDynamics$testLength, " m", sep = "")),
                     pos = 3)
                text(x = (sprintRawDynamics$time[length(sprintRawDynamics$time)] + sprintRawDynamics$time[1])/2, sprintRawDynamics$testLength /2,
                     labels = "The capture does not reach the length of the test", cex = 2, pos = 3)
                print("Capture too short")
                return("Capture too short")
        }
        
        
        legendText = paste("Vmax.raw =", round(sprintRawDynamics$rawVmax, digits = 2), "m/s")
        legendColor = "black"
        
        legendText = c(legendText, paste("Vmax.fitted =", round(sprintFittedDynamics$Vmax.fitted, digits = 2), "m/s"))
        legendColor = c(legendColor, "black")
        
        legendText = c(legendText, paste("K =", round(sprintFittedDynamics$K.fitted, digits = 2), "s\u207B\u00B9"))
        legendColor = c(legendColor, "black")
        
        legendText = c(legendText, paste("\u03C4 =", round(1/sprintFittedDynamics$K.fitted, digits = 2), "s"))
        legendColor = c(legendColor, "black")
        
        legendText = c(legendText, paste("Amax.fitted =", round(max(sprintFittedDynamics$amax.fitted), digits = 2), "m/s\u00b2"))
        legendColor = c(legendColor, "magenta")
        
        legendText = c(legendText, paste("Fmax.fitted =", round(sprintFittedDynamics$fmax.fitted, digits = 2), "N"))
        legendColor = c(legendColor, "blue")
        
        legendText = c(legendText, paste("Pmax.fitted =", round(sprintFittedDynamics$pmax.fitted, digits = 2), "W"))
        legendColor = c(legendColor, "red")
        
        #Plotting rawSpeed
        ylimits = c(0, sprintRawDynamics$rawVmax*1.05)
        xlimits =c(0, sprintRawDynamics$time[sprintRawDynamics$endSample]*1.05)
        #Calculing 5m lap times
        splitPosition = min(sprintRawDynamics$testLength, 5)
        splitTime = interpolateXAtY(sprintRawDynamics$time[sprintRawDynamics$startSample:sprintRawDynamics$endSample],
                                  sprintRawDynamics$rawPosition[sprintRawDynamics$startSample:sprintRawDynamics$endSample],
                                  splitPosition)
        meanSpeed = splitPosition / splitTime
        meanForce =getMeanValue(sprintRawDynamics$time, sprintRawDynamics$rawForce, sprintRawDynamics$time[sprintRawDynamics$startSample], splitTime)
        meanPower =getMeanValue(sprintRawDynamics$time, sprintRawDynamics$rawPower, sprintRawDynamics$time[sprintRawDynamics$startSample], splitTime)

        while(splitPosition[length(splitPosition)] + 5 < sprintRawDynamics$testLength)
        {
                splitPosition = c(splitPosition, splitPosition[length(splitPosition)] + 5)
                splitTime = c(splitTime, interpolateXAtY(sprintRawDynamics$time[sprintRawDynamics$startSample:sprintRawDynamics$endSample],
                                                     sprintRawDynamics$rawPosition[sprintRawDynamics$startSample:sprintRawDynamics$endSample],
                                                     splitPosition[length(splitPosition)]))
                meanSpeed = c(meanSpeed, (splitPosition[length(splitPosition)] - splitPosition[length(splitPosition) -1]) /
                                      (splitTime[length(splitTime)] - splitTime[length(splitTime) -1]))
                meanForce = c(meanForce, getMeanValue(sprintRawDynamics$time, sprintRawDynamics$rawForce,
                                                      splitTime[length(splitTime) -1], splitTime[length(splitTime)]))
                meanPower = c(meanPower, getMeanValue(sprintRawDynamics$time, sprintRawDynamics$rawPower,
                                                      splitTime[length(splitTime) -1], splitTime[length(splitTime)]))
        }
        splitPosition = c(splitPosition, sprintRawDynamics$testLength)
        splitTime = c(splitTime, interpolateXAtY(sprintRawDynamics$time[sprintRawDynamics$startSample:sprintRawDynamics$endSample],
                                             sprintRawDynamics$rawPosition[sprintRawDynamics$startSample:sprintRawDynamics$endSample],
                                             sprintRawDynamics$testLength))
        meanSpeed = c(meanSpeed, (splitPosition[length(splitPosition)] - splitPosition[length(splitPosition) -1]) /
                              (splitTime[length(splitTime)] - splitTime[length(splitTime) -1]))
        
        meanForce = c(meanForce, getMeanValue(sprintRawDynamics$time, sprintRawDynamics$rawForce,
                                              splitTime[length(splitTime) -1], splitTime[length(splitTime)]))
        meanPower = c(meanPower, getMeanValue(sprintRawDynamics$time, sprintRawDynamics$rawPower,
                                              splitTime[length(splitTime) -1], splitTime[length(splitTime)]))
        print("meanForce:")
        print(meanForce)
        print("meanPower:")
        print(meanPower)
        
        if(plotRawMeanSpeed)
        {
                barplot(height = meanSpeed, width = diff(c(0,splitTime)), space = 0,
                        ylim = ylimits,
                        xlab = "Time (s)", ylab = "Speed (m/s)",
                        yaxs = "i", xaxs = "i")
                mtext(title, line = 2.5, cex = 1.5)
                mtext(subtitle, line = 1)
                lines(sprintRawDynamics$time[sprintRawDynamics$startSample:sprintRawDynamics$endSample],
                      sprintRawDynamics$rawSpeed[sprintRawDynamics$startSample:sprintRawDynamics$endSample])
                lines(c(0,sprintRawDynamics$time[sprintRawDynamics$startSample]),c(0,sprintRawDynamics$rawSpeed[sprintRawDynamics$startSample]))
                # points(sprintRawDynamics$time[sprintRawDynamics$startSample:sprintRawDynamics$endSample],
                #        sprintRawDynamics$rawSpeed[sprintRawDynamics$startSample:sprintRawDynamics$endSample])
                # abline(v=sprintRawDynamics$time[sprintRawDynamics$startSample])
                if(plotStartDetection){
                        points(sprintRawDynamics$time[sprintRawDynamics$startSample], sprintRawDynamics$rawSpeed[sprintRawDynamics$startSample])
                }
                print("########")
                lapTime = diff(c(0, splitTime))
                textXPos = c(0,splitTime[1:length(splitTime) -1]) + lapTime/2
                text(textXPos, meanSpeed, round(meanSpeed, digits = 2), pos = 3)
                text(textXPos, 0, paste(round(lapTime, digits = 3), " s", sep = ""), pos = 3)
        } else
        {
                plot(sprintRawDynamics$time[sprintRawDynamics$startSample:sprintRawDynamics$endSample],
                     sprintRawDynamics$rawSpeed[sprintRawDynamics$startSample:sprintRawDynamics$endSample],
                     type = "l", lty = 3, ylim = ylimits,
                     main = title, xlab = "Time(s)", ylab = "Speed(m/s)",
                     yaxs = "i", xaxs = "i")
                lines(x = c(0,sprintRawDynamics$time[sprintRawDynamics$startSample]), y = c(0,sprintRawDynamics$rawSpeed[sprintRawDynamics$startSample]))
        }
        
        abline(v = splitTime, lty = 3)
        mtext(side = 3, at = splitTime, text = paste(splitPosition, " m", sep=""))
        mtext(side = 1, at = splitTime, text = paste(round(splitTime, digits = 3), " s", sep=""))
        
        if (plotFittedSpeed)
        {
                #Plotting fitted speed
                lines(sprintFittedDynamics$t.fitted, sprintFittedDynamics$v.fitted
                      , lty = 2
                      #, col = "green"
                      )
        }
        
        if (plotRawAccel || plotFittedAccel)
        {
                ylimits = c(min(sprintRawDynamics$rawAccel[sprintRawDynamics$startSample:sprintRawDynamics$endSample])*0.95, max(c(sprintRawDynamics$rawAmax, sprintFittedDynamics$amax.fitted)*1.05))
                if (plotRawAccel)
                {
                        #Plotting rawAccel
                        par(new = TRUE)
                        plot(c(0,sprintRawDynamics$time[sprintRawDynamics$startSample:sprintRawDynamics$endSample]),
                             c(0,sprintRawDynamics$rawAccel[(sprintRawDynamics$startSample + 0):(sprintRawDynamics$endSample + 0)]),
                             ylim = ylimits,
                             type = "l", col = "magenta",
                             xlab = "", ylab = "",
                             axes = FALSE, yaxs = "i", xaxs = "i")
                        # points(sprintRawDynamics$time[sprintRawDynamics$startSample:sprintRawDynamics$endSample], sprintRawDynamics$rawAccel[(sprintRawDynamics$startSample + 0):(sprintRawDynamics$endSample + 0)],
                        #        col = "magenta", cex = 0.5)
                             
                        axis(side = 4)
                        #abline(h=c(0,sprintRawDynamics$startAccel), col = c("magenta", "magenta"), lty = c(1,2))
                        legendText = c(legendText, paste("Amax.raw =", round(sprintRawDynamics$rawAmax, digits = 2), "m/s"))
                        legendColor = c(legendColor, "magenta")
                }
                
                if (plotFittedAccel)
                {
                        #Plotting rawAccel
                        par(new = TRUE)
                        plot(sprintFittedDynamics$t.fitted, sprintFittedDynamics$a.fitted,
                             ylim = ylimits, type = "l", col = "magenta", lty = 2,
                             xlab = "", ylab = "",
                             axes = FALSE, yaxs = "i", xaxs = "i")
                }
                axis(side = 4, col = "magenta")
        }
        
        if(plotRawForce|| plotFittedForce)
        {
                if(plotRawForce){
                        ylimits = c(min(sprintRawDynamics$rawForce[sprintRawDynamics$startSample:sprintRawDynamics$endSample])*0.95, max(c(sprintRawDynamics$rawFmax, sprintFittedDynamics$fmax.fitted)*1.05))
                } else {
                        ylimits = c(0,sprintFittedDynamics$fmax.fitted) 
                }
                
                if (plotRawForce)
                {
                        #Plotting rawForce
                        par(new = TRUE)
                        plot(sprintRawDynamics$time[sprintRawDynamics$startSample:sprintRawDynamics$endSample], sprintRawDynamics$rawForce[sprintRawDynamics$startSample:sprintRawDynamics$endSample],
                             ylim = ylimits, type = "l", col = "blue",
                             xlab = "", ylab = "",
                             axes = FALSE, yaxs = "i", xaxs = "i")
                        legendText = c(legendText, paste("Fmax.raw =", round(sprintRawDynamics$rawFmax, digits = 2), "N"))
                        legendColor = c(legendColor, "blue")
                }
                
                if (plotFittedForce)
                {
                        #Plotting fittedForce
                        par(new = TRUE)
                        plot(sprintFittedDynamics$t.fitted, sprintFittedDynamics$f.fitted,
                             ylim = ylimits, type = "l", col = "blue", lty = 2,
                             xlab = "", ylab = "",
                             axes = FALSE, yaxs = "i", xaxs = "i")
                }
                axis(side = 4, col = "blue", line = 2)
                print("Mean force from the model")
                print(getMeanValue(sprintFittedDynamics$t.fitted, sprintRawDynamics$force.fitted, 0, 1.004))
        }     

        if(plotMeanRawForce)
        {
                par(new = TRUE)
                ylimits = c(min(sprintRawDynamics$rawForce[sprintRawDynamics$startSample:sprintRawDynamics$endSample])*0.95,
                            max(c(sprintRawDynamics$rawFmax, sprintFittedDynamics$fmax.fitted)*1.05))
                plot(NULL, NULL,
                     ylim = ylimits,
                     xlim = c(sprintRawDynamics$time[sprintRawDynamics$startSample], sprintRawDynamics$time[sprintRawDynamics$endSample]),
                     xlab = "", ylab = "",
                     axes = FALSE, yaxs = "i", xaxs = "i")
                text(splitTime[1]*0.2, meanForce[1], paste(round(meanForce[1], digits = 2), "N"), col = "blue")
                for(n in 1:length(meanForce))
                {
                        text(splitTime[n] + (splitTime[n+1] - splitTime[n])*0.2, meanForce[n+1], paste(round(meanForce[n+1], digits = 2), "N"), col = "blue")
                        
                }
                #axis(side = 4, col = "blue", line = 2)
        }
        
        if(plotRawPower|| plotFittedPower)
        {
                if (plotRawPower)
                {
                        ylimits = c(min(sprintRawDynamics$rawPower[sprintRawDynamics$startSample:sprintRawDynamics$endSample])*0.95,
                                    max(c(sprintRawDynamics$rawPmax, sprintFittedDynamics$pmax.fitted)*1.05))
                } else {
                        ylimits = c(0,sprintFittedDynamics$pmax.fitted*1.05)
                }
                if (plotRawPower)
                {
                        #Plotting rawPower
                        par(new = TRUE)
                        plot(sprintRawDynamics$time[sprintRawDynamics$startSample:sprintRawDynamics$endSample], sprintRawDynamics$rawPower[sprintRawDynamics$startSample:sprintRawDynamics$endSample],
                             ylim = ylimits, type = "l", col = "red",
                             xlab = "", ylab = "",
                             axes = FALSE, yaxs = "i", xaxs = "i")
                        legendText = c(legendText, paste("Pmax.raw =", round(sprintRawDynamics$rawPmax, digits = 2), "N"))
                        legendColor = c(legendColor, "red")
                }
                
                if (plotFittedPower)
                {
                        #Plotting fittedPower
                        par(new = TRUE)
                        plot(sprintFittedDynamics$t.fitted, sprintFittedDynamics$p.fitted
                             , ylim = ylimits , type = "l", col = "red"
                             ,xlab = "", ylab = ""
                             ,axes = FALSE, yaxs = "i", xaxs = "i")
                        text(x = sprintFittedDynamics$tpmax.fitted, y = sprintFittedDynamics$pmax.fitted
                             , labels = paste(round(sprintFittedDynamics$pmax.fitted, digits = 2), "W")
                             , pos = 3
                             , col = "red")
                        lines(c(sprintFittedDynamics$tpmax.fitted, sprintFittedDynamics$tpmax.fitted), c(sprintFittedDynamics$pmax.fitted, 0)
                              , lty = 2, col = "red")
                        mtext(paste(round(sprintFittedDynamics$tpmax.fitted, digits = 3), "s")
                              , at = sprintFittedDynamics$tpmax.fitted, side = 1
                              , col = "red")
                }
                axis(side = 4, col = "red", line = 4)
        }
        
        
        if(plotMeanRawPower)
        {
                par(new = TRUE)
                ylimits = c(min(sprintRawDynamics$rawPower[sprintRawDynamics$startSample:sprintRawDynamics$endSample])*0.95,
                            max(c(sprintRawDynamics$rawPmax, sprintFittedDynamics$pmax.fitted)*1.05))
                plot(NULL, NULL,
                     ylim = ylimits,
                     xlim = c(sprintRawDynamics$time[sprintRawDynamics$startSample], sprintRawDynamics$time[sprintRawDynamics$endSample]),
                     xlab = "", ylab = "",
                     axes = FALSE, yaxs = "i", xaxs = "i")
                text(splitTime[1]*0.8, meanPower[1], paste(round(meanPower[1], digits = 2), "W"), col = "red")
                for(n in 1:length(meanPower))
                {
                        text(splitTime[n] + (splitTime[n+1] - splitTime[n])*0.8, meanPower[n+1], paste(round(meanPower[n+1], digits = 2), "W"), col = "red")
                        
                }
                axis(side = 4, col = "red", line = 4)
        }

	#triggers
	abline(v=triggersOn, col="green")
	abline(v=triggersOff, col="red")


        plotSize = par("usr")
        legend(x = plotSize[2], y = plotSize[3] + (plotSize[4] - plotSize[3])*0.25,
                xjust = 1, yjust = 0.5, cex = 1,
                legend = legendText,
                text.col = legendColor)
}

#Detecting where the sprint start and stops
getTrimmingSamples <- function(totalTime, position, speed, accel, testLength, startAccel)
{
        print("#########Entering getTrimmingSamples###########33")
        #The test starts when the acceleration is greater than startAccel m/s²
        startSample = 0
        startingSample = FALSE
        while(!startingSample & startSample < (length(speed)-2))
        {
                startSample = startSample +1
                if(accel[startSample] > startAccel)
                {
                        print(paste("accel[", startSample,"] = ", accel[startSample], sep = ""))
                        
                        #Looking if after 1 second the position has increased at least .5m.
                        sampleAfterSecond = which.min(abs(totalTime - (totalTime[startSample] +1)))
                        print(paste("sampleAfterSecond =", sampleAfterSecond))
                        positionAfterSecond = position[sampleAfterSecond]
                        #Checking if the displacement has been at least .5m
                        if(abs(positionAfterSecond - position[startSample]) > 0.5){
                                startingSample = TRUE
                        }
                }
        }
        
        if(startSample == (length(speed) -2))
        {
                print("No start detected")
		return(list(start = -1, end = -1, errorInStart = TRUE ))
        }
        
        
        # #Going back in the time to find a really slow velocity
        # while(speed[startSample] > 1)
        # {
        #         startSample = startSample -1
        #         
        #         #If the sprint doesn't start at a null speed, the first sample is used
        #         #In old versions of RaceAnalyzer it was possible to start with a high speed.
        #         if(startSample == 0)
        #         {
        #                 startSample = 1
        #                 break
        #         }
        # }
        
        #Zeroing time to the initial acceleration sample
        totalTime = totalTime - totalTime[startSample]
        #Zeroing position to the initial acceleration sample
        position = position - position[startSample]
        
        #Detecting when starts the braking phase
        endSample = which.min(abs(position - testLength))
        if(position[endSample] < testLength)
                endSample = endSample +1
        return(list(start = startSample, end = endSample, errorInStart = !startingSample ))
}

tryNLS <- function(data){
        print("#######Entering tryNLS#########")
        tryCatch (
                {
                        model = nls(speed ~ Vmax*(1-exp(-K*time)), data,
                                    start = list(Vmax = max(data[,"speed"]), K = 1), control=nls.control(warnOnly=TRUE))
                        # print("model:")
                        # print(model)
                        if (! model$convInfo$isConv){
                                return(list(regressionDone = FALSE, model = model))
                        } else {
                                return(list(regressionDone = TRUE, model = model))
                        }
                }, 
                error=function(cond)
                { 
                        message(cond)
                        return(list(regressionDone = FALSE))
                }
        )
}

testEncoderCJ <- function(filename, testLength, mass, personHeight, tempC, startAccel)
{
        sprintRawDynamics = getSprintFromEncoder(filename, testLength, op$mass, op$tempC, op$personHeight, Vw = 0, device = op$device, startAccel)
        # print("sprintRawDynamics:")
        # print(sprintRawDynamics)
        if (sprintRawDynamics$longEnough & sprintRawDynamics$regressionDone)
        {
                sprintFittedDynamics = getDynamicsFromSprint(K = sprintRawDynamics$K, Vmax = sprintRawDynamics$Vmax, mass, tempC, personHeight)
                print(paste("K =",sprintFittedDynamics$K.fitted, "Vmax =", sprintFittedDynamics$Vmax.fitted))
                plotSprintFromEncoder(sprintRawDynamic = sprintRawDynamics, sprintFittedDynamics = sprintFittedDynamics,
                                      title = op$title,
                                      subtitle = op$datetime,
				      triggersOn = op$triggersOnList,
				      triggersOff = op$triggersOffList,
                                      plotRawMeanSpeed = TRUE,
                                      plotRawSpeed = TRUE,
                                      plotRawAccel = op$plotRawAccel,
                                      plotRawForce = op$plotRawForce,
                                      plotMeanRawForce = FALSE,
                                      plotRawPower = op$plotRawPower,
                                      plotMeanRawPower = FALSE,
                                      plotFittedSpeed = TRUE,
                                      plotFittedAccel = op$plotFittedAccel,
                                      plotFittedForce = op$plotFittedForce,
                                      plotFittedPower = op$plotFittedPower,
				      startAccel,
                                      plotStartDetection = TRUE)
                exportSprintDynamics(sprintFittedDynamics)
        } else
          print("Couldn't calculate the sprint model")
}

prepareGraph(op$os, pngFile, op$graphWidth, op$graphHeight)
testEncoderCJ(op$filename, op$testLength, op$mass, op$personHeight, op$tempC, op$startAccel)
endGraph()
