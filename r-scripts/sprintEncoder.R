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
#   Copyright (C) 2020-2021   	Xavier de Blas <xaviblas@gmail.com>


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
		splitLength	= as.numeric(options[11]),
                title 	 	= options[12],
                datetime 	= options[13],
                startAccel 	= as.numeric(options[14]),
                plotRawAccel 	= as.logical(options[15]),
                plotFittedAccel = as.logical(options[16]),
                plotRawForce 	= as.logical(options[17]),
                plotFittedForce = as.logical(options[18]),
                plotRawPower 	= as.logical(options[19]),
                plotFittedPower = as.logical(options[20]),
		triggersOnList  = as.numeric(unlist(strsplit(options[21], "\\;"))),
		triggersOffList  = as.numeric(unlist(strsplit(options[22], "\\;"))),
		singleOrMultiple = options[23],
		decimalCharAtExport = options[24],
		includeImagesOnExport = options[25],
		includeInstantaneousOnExport = options[26]
        ))
}

#-------------- assign options -------------
op <- assignOptions(options)

op$title = fixTitleAndOtherStrings(op$title)
op$datetime = fixDatetime(op$datetime)

getSprintFromEncoder <- function(filename, testLength, Mass, Temperature = 25, Height , Vw = 0, device = "MANUAL", startAccel, splitLength)
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
        
        #Discarding trigger rows with displacement equal to zero
        raceAnalyzer = raceAnalyzer[which(raceAnalyzer[,"displacement"] != 0), ]
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
	        metersPerPulse = 0.0030321 	#HARDCODED, same as: src/runEncoder.cs
	else                                    #resisted race analyzer - sled device
	        metersPerPulse = 4 * 30 / 12267 	#With an encoder of 200 ppr, in 30m there are 12267 pulses.

        raceAnalyzer$displacement = raceAnalyzer$displacement * metersPerPulse
        position = cumsum(raceAnalyzer$displacement)
        speed = raceAnalyzer$displacement[2:length(raceAnalyzer$displacement)] / elapsedTime
        speed = c(0,speed)
        
        #Adjusting the time of each sample to the mean time between two samples
        # for( i in 2:length(totalTime)){
        #         totalTime[i] = totalTime[i] - elapsedTime[i-1] / 2
        # }
        
        #Accel of the sample N is the mean accel between N-1 and N+1 samples.
        #The time of the accel sample is the same as the speed sample.
        #TODO. Adjust the time at which this accel is measured.
        accel = speed[2] / totalTime[2]
        for(i in 3:(length(speed) -1))
        {
                accel = c(accel, (speed[i+1] - speed[i-1]) / (totalTime[i +1] - totalTime[i -1]))
        }
        accel = c(accel, accel[length(accel)])
        accel = c(0, accel)
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

        # timeBefore = speed[trimmingSamples$start] * ((time[trimmingSamples$start + 1]) / (speed[trimmingSamples$start + 1] - speed[trimmingSamples$start]))
        # time = time + timeBefore
        # data = data.frame(time = time[trimmingSamples$start:trimmingSamples$end], speed = speed[trimmingSamples$start:trimmingSamples$end])
        data = data.frame(time = time[trimmingSamples$start:trimmingSamples$end], position = position[trimmingSamples$start:trimmingSamples$end])
        # print(data)
        
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
        Vmax =summary(regression$model)$coeff[2,1]
        K = summary(regression$model)$coeff[1,1]
        T0 = summary(regression$model)$coeff[3,1]
        P0 = summary(regression$model)$coeff[4,1]
        print(paste("T0:", T0))
        time = time + T0
        print(paste("P0:", P0))

        print("startTime:")
        print(totalTime[trimmingSamples$start] + T0)
        
        #Getting values of the splits
        splits = getSplits(time, position + P0
                           , totalForce, power
                           , trimmingSamples$start, trimmingSamples$end
                           , testLength, splitLength)
        
        splitPosition = splits$position
        splitTime = splits$time
        meanSpeed = splits$meanSpeed
        meanForce = splits$meanForce
        meanPower = splits$meanPower

        return(list(Vmax = Vmax, K = K, T0 = T0, Ka=Ka, Vw=Vw, Mass=Mass,
                    time = time, rawPosition = position + P0, rawSpeed = speed, rawAccel = accel, rawForce = totalForce, rawPower = power,
                    rawVmax = max(speed[trimmingSamples$start:trimmingSamples$end]),
		    rawAmax = max(accel[trimmingSamples$start:trimmingSamples$end]),
		    rawFmax = max(totalForce[trimmingSamples$start:trimmingSamples$end]),
		    rawPmax = max(power[trimmingSamples$start:trimmingSamples$end]),
                    startSample = trimmingSamples$start, startTime = totalTime[trimmingSamples$start] + T0,
		    endSample = trimmingSamples$end, testLength = testLength,
		    longEnough = longEnough, regressionDone = regression$regressionDone, timeBefore = T0, startAccel = startAccel,
                    splitTime = splitTime, splitPosition = splitPosition, meanSpeed = meanSpeed, meanForde = meanForce, meanPower = meanPower))
}

plotSprintFromEncoder <- function(sprintRawDynamics, sprintFittedDynamics,
				  splitLength,
				  title = "Test graph",
				  subtitle = "",
				  triggersOn = "",
				  triggersOff = "",
                                  plotRawMeanSpeed = TRUE,
                                  plotRawSpeed = TRUE,
				  plotRawAccel,
				  plotRawForce,
                                  plotMeanRawForce = TRUE,
				  plotRawPower,
                                  plotMeanRawPower = TRUE,
                                  plotFittedSpeed = TRUE,
				  plotFittedAccel,
				  plotFittedForce,
				  plotFittedPower,
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
        par(mar = c(4.5, 4.5, 5, 2))
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
        
	ltyRaw = 1;
	ltyFitted = 2;
	lwdRaw = 1;
	lwdFitted = 2;
        
        legendText = paste("V max (raw) =", round(sprintRawDynamics$rawVmax, digits = 2), "m/s")
        legendColor = "black"
        legendLty = ltyRaw
        legendLwd = lwdRaw
        
        legendText = c(legendText, paste("V max (fitted) =", round(sprintFittedDynamics$Vmax.fitted, digits = 2), "m/s"))
        legendColor = c(legendColor, "black")
        legendLty = c(legendLty, ltyFitted)
        legendLwd = c(legendLwd, lwdFitted)
        
        legendText = c(legendText, paste("K =", round(sprintFittedDynamics$K.fitted, digits = 2), "s\u207B\u00B9"))
        legendColor = c(legendColor, "black")
        legendLty = c(legendLty, 0)
        legendLwd = c(legendLwd, 0)

        legendText = c(legendText, paste("\u03C4 =", round(1/sprintFittedDynamics$K.fitted, digits = 2), "s"))
        legendColor = c(legendColor, "black")
        legendLty = c(legendLty, 0)
        legendLwd = c(legendLwd, 0)

        #Plotting rawSpeed
        ylimits = c(0, sprintRawDynamics$rawVmax*1.05)
        xlimits =c(0, sprintRawDynamics$time[sprintRawDynamics$endSample])

        
        if(plotRawMeanSpeed)
        {
                barplot(height = sprintRawDynamics$meanSpeed, width = diff(c(0,sprintRawDynamics$splitTime)), space = 0,
                        ylim = ylimits, xlim = xlimits,
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
                lapTime = diff(c(0, sprintRawDynamics$splitTime))
                textXPos = c(0,sprintRawDynamics$splitTime[1:length(sprintRawDynamics$splitTime) -1]) + lapTime/2
                text(textXPos, sprintRawDynamics$meanSpeed/2, round(sprintRawDynamics$meanSpeed, digits = 2), pos = 3)
                text(textXPos, 0, paste(round(lapTime, digits = 3), " s", sep = ""), pos = 3)
        } else
        {
                plot(sprintRawDynamics$time[sprintRawDynamics$startSample:sprintRawDynamics$endSample],
                     sprintRawDynamics$rawSpeed[sprintRawDynamics$startSample:sprintRawDynamics$endSample],
                     type = "l", lty = 3, ylim = ylimits, xlim = xlimits,
                     main = title, xlab = "Time (s)", ylab = "Speed (m/s)",
                     yaxs = "i", xaxs = "i")
                lines(x = c(0,sprintRawDynamics$time[sprintRawDynamics$startSample]), y = c(0,sprintRawDynamics$rawSpeed[sprintRawDynamics$startSample]))
        }
        
        abline(v = sprintRawDynamics$splitTime, lty = 3)
        mtext(side = 3, at = sprintRawDynamics$splitTime, text = paste(sprintRawDynamics$splitPosition, " m", sep=""))
        mtext(side = 1, at = sprintRawDynamics$splitTime, text = paste(round(sprintRawDynamics$splitTime, digits = 3), " s", sep=""))
        
        if (plotFittedSpeed)
        {
                #Plotting fitted speed
                lines(sprintFittedDynamics$t.fitted, sprintFittedDynamics$v.fitted
                      , lty = 2, lwd = 2
                      #, col = "green"
                      )
        }
        
        if (plotRawAccel || plotFittedAccel)
        {
                if(plotRawAccel){
			ylimits = c(min(sprintRawDynamics$rawAccel[sprintRawDynamics$startSample:sprintRawDynamics$endSample])*0.95, max(c(sprintRawDynamics$rawAmax, sprintFittedDynamics$amax.fitted)*1.05))
		} else {
                        ylimits = c(0,sprintFittedDynamics$amax.fitted)
		}

                if (plotRawAccel)
                {
                        #Plotting rawAccel
                        par(new = TRUE)
                        plot(c(0,sprintRawDynamics$time[sprintRawDynamics$startSample:sprintRawDynamics$endSample]),
                             c(0,sprintRawDynamics$rawAccel[(sprintRawDynamics$startSample + 0):(sprintRawDynamics$endSample + 0)]),
                             ylim = ylimits, xlim = xlimits,
                             type = "l", col = "magenta",
                             xlab = "", ylab = "",
                             axes = FALSE, yaxs = "i", xaxs = "i")
                        # points(sprintRawDynamics$time[sprintRawDynamics$startSample:sprintRawDynamics$endSample], sprintRawDynamics$rawAccel[(sprintRawDynamics$startSample + 0):(sprintRawDynamics$endSample + 0)],
                        #        col = "magenta", cex = 0.5)
                             
                        axis(side = 4)
                        #abline(h=c(0,sprintRawDynamics$startAccel), col = c("magenta", "magenta"), lty = c(1,2))
                        legendText = c(legendText, paste("A max (raw) =", round(sprintRawDynamics$rawAmax, digits = 2), "m/s\u00b2"))
                        legendColor = c(legendColor, "magenta")
			legendLty = c(legendLty, ltyRaw)
			legendLwd = c(legendLwd, lwdRaw)
                }
                
                if (plotFittedAccel)
                {
                        #Plotting rawAccel
                        par(new = TRUE)
                        plot(sprintFittedDynamics$t.fitted, sprintFittedDynamics$a.fitted,
                             ylim = ylimits, xlim = xlimits,
                             type = "l", col = "magenta", lty = 2, lwd = 2,
                             xlab = "", ylab = "",
                             axes = FALSE, yaxs = "i", xaxs = "i")

			legendText = c(legendText, paste("A max (fitted) =", round(max(sprintFittedDynamics$amax.fitted), digits = 2), "m/s\u00b2"))
			legendColor = c(legendColor, "magenta")
			legendLty = c(legendLty, ltyFitted)
			legendLwd = c(legendLwd, lwdFitted)
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
                             ylim = ylimits, xlim = xlimits,
                             type = "l", col = "blue",
                             xlab = "", ylab = "",
                             axes = FALSE, yaxs = "i", xaxs = "i")
                        legendText = c(legendText, paste("F max (raw) =", round(sprintRawDynamics$rawFmax, digits = 2), "N"))
                        legendColor = c(legendColor, "blue")
			legendLty = c(legendLty, ltyRaw)
			legendLwd = c(legendLwd, lwdRaw)
                }
                
                if (plotFittedForce)
                {
                        #Plotting fittedForce
                        par(new = TRUE)
                        plot(sprintFittedDynamics$t.fitted, sprintFittedDynamics$f.fitted,
                             ylim = ylimits, xlim = xlimits,
                             type = "l", col = "blue", lty = 2, lwd = 2,
                             xlab = "", ylab = "",
                             axes = FALSE, yaxs = "i", xaxs = "i")

			legendText = c(legendText, paste("F max (fitted) =", round(sprintFittedDynamics$fmax.fitted, digits = 2), "N"))
			legendColor = c(legendColor, "blue")
			legendLty = c(legendLty, ltyFitted)
			legendLwd = c(legendLwd, lwdFitted)
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
                     ylim = ylimits, xlim = xlimits,
                     xlab = "", ylab = "",
                     axes = FALSE, yaxs = "i", xaxs = "i")
                text(sprintRawDynamics$splitTime[1]*0.2, sprintRawDynamics$meanForce[1], paste(round(sprintRawDynamics$meanForce[1], digits = 2), "N"), col = "blue")
                for(n in 1:length(sprintRawDynamics$meanForce))
                {
                        text(sprintRawDynamics$splitTime[n] + (sprintRawDynamics$splitTime[n+1] - sprintRawDynamics$splitTime[n])*0.2, sprintRawDynamics$meanForce[n+1], paste(round(sprintRawDynamics$meanForce[n+1], digits = 2), "N"), col = "blue")
                        
                }
                #axis(side = 4, col = "blue", line = 2)
        }
        
        if(plotRawPower|| plotFittedPower)
        {
		#this 1.075 were 1.05, but sometimes the textg value in top of fitted power got out of boundaries
                if (plotRawPower)
                {
                        ylimits = c(min(sprintRawDynamics$rawPower[sprintRawDynamics$startSample:sprintRawDynamics$endSample])*0.95,
                                    max(c(sprintRawDynamics$rawPmax, sprintFittedDynamics$pmax.fitted)*1.075))
                } else {
                        ylimits = c(0,sprintFittedDynamics$pmax.fitted*1.075)
                }
                if (plotRawPower)
                {
                        #Plotting rawPower
                        par(new = TRUE)
                        plot(sprintRawDynamics$time[sprintRawDynamics$startSample:sprintRawDynamics$endSample], sprintRawDynamics$rawPower[sprintRawDynamics$startSample:sprintRawDynamics$endSample],
                             ylim = ylimits, xlim = xlimits,
                             type = "l", col = "red",
                             xlab = "", ylab = "",
                             axes = FALSE, yaxs = "i", xaxs = "i")
                        legendText = c(legendText, paste("P max (raw) =", round(sprintRawDynamics$rawPmax, digits = 2), "N"))
                        legendColor = c(legendColor, "red")
			legendLty = c(legendLty, ltyRaw)
			legendLwd = c(legendLwd, lwdRaw)
                }
                
                if (plotFittedPower)
                {
                        #Plotting fittedPower
                        par(new = TRUE)
                        plot(x = sprintFittedDynamics$t.fitted, y = sprintFittedDynamics$p.fitted
                             ,ylim = ylimits , xlim = xlimits
                             ,type = "l", col = "red", lty = 2, lwd = 2
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

			legendText = c(legendText, paste("P max (fitted) =", round(sprintFittedDynamics$pmax.fitted, digits = 2), "W"))
			legendColor = c(legendColor, "red")
			legendLty = c(legendLty, ltyFitted)
			legendLwd = c(legendLwd, lwdFitted)
                }
                axis(side = 4, col = "red", line = 4)
        }
        
        
        if(plotMeanRawPower)
        {
                par(new = TRUE)
                ylimits = c(min(sprintRawDynamics$rawPower[sprintRawDynamics$startSample:sprintRawDynamics$endSample])*0.95,
                            max(c(sprintRawDynamics$rawPmax, sprintFittedDynamics$pmax.fitted)*1.05))
                plot(NULL, NULL,
                     ylim = ylimits, xlim = xlimits,
                     #xlim = c(sprintRawDynamics$time[sprintRawDynamics$startSample], sprintRawDynamics$time[sprintRawDynamics$endSample]),
                     xlab = "", ylab = "",
                     axes = FALSE, yaxs = "i", xaxs = "i")
                text(sprintRawDynamics$splitTime[1]*0.8, sprintRawDynamics$meanPower[1], paste(round(sprintRawDynamics$meanPower[1], digits = 2), "W"), col = "red")
                for(n in 1:length(sprintRawDynamics$meanPower))
                {
                        text(sprintRawDynamics$splitTime[n] + (sprintRawDynamics$splitTime[n+1] - sprintRawDynamics$splitTime[n])*0.8, sprintRawDynamics$meanPower[n+1], paste(round(sprintRawDynamics$meanPower[n+1], digits = 2), "W"), col = "red")
                        
                }
                axis(side = 4, col = "red", line = 4)
        }

	#triggers
        # triggersOn = triggersOn + sprintFittedDynamics$T0
        # triggersOff = triggersOff + sprintFittedDynamics$T0
        print("triggersOn on plot:")
        print(triggersOn)
        #TODO: Find why the T0 have to be added twice
        triggersOn = triggersOn + 2*sprintFittedDynamics$T0
        print(triggersOn)
	abline(v=triggersOn, col="green")
	print("triggersOff plot:")
	print(triggersOff)
	triggersOff = triggersOff + 2*sprintFittedDynamics$T0
	print(triggersOff)
	abline(v=triggersOff, col="red")


        plotSize = par("usr")
        legend(x = plotSize[2], y = plotSize[3] + (plotSize[4] - plotSize[3])*0.25,
                xjust = 1, yjust = 0.5, cex = 1,
                legend = legendText,
                col = legendColor,
                text.col = legendColor,
		lty = legendLty,
		lwd = legendLwd,
		pch = NA)
}

#Detecting where the sprint start and stops
getTrimmingSamples <- function(totalTime, position, speed, accel, testLength, startAccel)
{
        print("#########Entering getTrimmingSamples###########33")
        print(paste("startAccel =", startAccel))
        #The test starts when the acceleration is greater than startAccel m/s²
        startSample = 0
        startingSample = FALSE          #Wether the starting sample has been found or not
        while(!startingSample & startSample < (length(speed)-2))        #While starting sample not found and not at the end of the signal
        {
                startSample = startSample +1
                if(accel[startSample] > startAccel)
                {
                        print("Current accel > startAccel")
                        print(paste("startAccel =", startAccel))
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

#Getting the mean values of the dynamics in each split
getSplits <- function(time, rawPosition, rawForce, rawPower, startSample, endSample, testLength, splitLength)
{
        splitPosition = min(testLength, splitLength)
        splitTime = interpolateXAtY(time[startSample:endSample],
                                    rawPosition[startSample:endSample],
                                    splitPosition)
        
        meanSpeed = splitPosition / splitTime
        meanForce =getMeanValue(time, rawForce, time[startSample], splitTime)
        meanPower =getMeanValue(time, rawPower, time[startSample], splitTime)
        
        #while the next split position is within the testLength
        while(splitPosition[length(splitPosition)] + splitLength < testLength)
        {
                splitPosition = c(splitPosition, splitPosition[length(splitPosition)] + splitLength)
                print(paste("Going to interpolate at:", splitPosition[length(splitPosition)]))
                splitTime = c(splitTime, interpolateXAtY(X = time[startSample:endSample],
                                                         Y = rawPosition[startSample:endSample],
                                                         desiredY = splitPosition[length(splitPosition)]))
                
                meanSpeed = c(meanSpeed, (splitPosition[length(splitPosition)] - splitPosition[length(splitPosition) -1]) /
                                      (splitTime[length(splitTime)] - splitTime[length(splitTime) -1]))
                meanForce = c(meanForce, getMeanValue(time, rawForce,
                                                      splitTime[length(splitTime) -1], splitTime[length(splitTime)]))
                meanPower = c(meanPower, getMeanValue(time, rawPower,
                                                      splitTime[length(splitTime) -1], splitTime[length(splitTime)]))
        }
        splitPosition = c(splitPosition, testLength)
        splitTime = c(splitTime, interpolateXAtY(X = time[startSample:length(rawPosition)],
                                                 Y = rawPosition[startSample:length(rawPosition)],
                                                 desiredY = testLength))
        
        meanSpeed = c(meanSpeed, (splitPosition[length(splitPosition)] - splitPosition[length(splitPosition) -1]) /
                              (splitTime[length(splitTime)] - splitTime[length(splitTime) -1]))
        
        meanForce = c(meanForce, getMeanValue(time, rawForce,
                                              splitTime[length(splitTime) -1], splitTime[length(splitTime)]))
        meanPower = c(meanPower, getMeanValue(time, rawPower,
                                              splitTime[length(splitTime) -1], splitTime[length(splitTime)]))
        print("splitPosition:")
        print(splitPosition)
        print("splitTime:")
        print(splitTime)
        print("meanForce:")
        print(meanForce)
        print("meanPower:")
        print(meanPower)
        
        return(list(positions = splitPosition, time = splitTime, meanSpeed = meanSpeed, meanForce = meanForce, meanPower = meanPower))
}

tryNLS <- function(data){
        print("#######Entering tryNLS#########")
        # print("data:")
        # print(data)
        tryCatch (
                {
                        # model = nls(speed ~ Vmax*(1-exp(-K*(time + T0))), data,
                        #             start = list(Vmax = max(data[,"speed"]), K = 1, T0 = 0.2), control=nls.control(warnOnly=TRUE))
                        model = nls(position ~ Vmax*(time + T0 + (1/K)*exp(-K*(time + T0))) -Vmax/K + P0, data
                                    , start = list(K = 0.81, Vmax = 10, T0 = 0.2, P0 = 0.1), control=nls.control(warnOnly=TRUE))
                        print("model:")
                        print(model)
                        print(model$convInfo)
                        print(model$convInfo$inConv)
                        print(model$convInfo$stopCode)
                        if (! model$convInfo$isConv && (model$convInfo$stopCode != 2)){
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

testEncoderCJ <- function(filename, filenameInstantaneous, testLength, splitLength, splitPositionAll,
		mass, personHeight, tempC, device, title, datetime, startAccel, triggersOn, triggersOff)
{
        sprintRawDynamics = getSprintFromEncoder(filename, testLength, mass, tempC, personHeight, Vw = 0, device = device, startAccel, splitLength)
	#print("sprintRawDynamics:")
	#print(sprintRawDynamics)
	#print("sprintRawDynamics$longEnough")
	#print(sprintRawDynamics$longEnough)
	#print("sprintRawDynamics$regressionDone")
	#print(sprintRawDynamics$regressionDone)

	exportRow = NULL

        if (sprintRawDynamics$longEnough && sprintRawDynamics$regressionDone)
        {
                print(paste("Vmax:", sprintRawDynamics$Vmax))
                print(paste("T0:", sprintRawDynamics$T0))
                sprintFittedDynamics = getDynamicsFromSprint(K = sprintRawDynamics$K, Vmax = sprintRawDynamics$Vmax, Mass = mass, T0 = sprintRawDynamics$T0, Temperature = tempC, Height = personHeight)
                print(paste("K =",sprintFittedDynamics$K.fitted, "Vmax =", sprintFittedDynamics$Vmax.fitted))
                # print("triggersOn in testEncoderCJ:")
                # print(triggersOn)
                triggersOn = triggersOn/1E6 - sprintRawDynamics$startTime
                # print("triggersOn in testEncoderCJ:")
                # print(triggersOn)
                triggersOff = triggersOff/1E6 - sprintRawDynamics$startTime
                # print("triggersOff in testEncoderCJ:")
                print(op$triggersOffList)
                plotSprintFromEncoder(sprintRawDynamic = sprintRawDynamics, sprintFittedDynamics = sprintFittedDynamics,
				      splitLength = splitLength,
                                      title,
                                      datetime, 	#subtitle
				      triggersOn = triggersOn,
				      triggersOff = triggersOff,
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
				      startAccel = startAccel,
                                      plotStartDetection = TRUE)

		#splitPositionAll is NULL on (op$singleOrMultiple == "TRUE")
		exportRow = exportSprintDynamicsPrepareRow(sprintFittedDynamics, sprintRawDynamics$splitTime, sprintRawDynamics$splitPosition, splitPositionAll)

		if(filenameInstantaneous != "")
		{
			srd = sprintRawDynamics #to shorten formulas

			print("srd lengths:")
			#print(length(srd$time))
			#print(length(srd$rawPosition))
			#print(length(srd$rawSpeed))
			#print(length(srd$rawAccel))
			#print(length(srd$rawForce))
			#print(length(srd$rawPower))

			s.fitted = srd$Vmax * (1-exp(-srd$K * srd$time))
			a.fitted = srd$Vmax * srd$K * exp(-srd$K * srd$time)
			f.fitted = srd$Mass * a.fitted + srd$Ka * (s.fitted - srd$Vw)^2
			p.fitted = f.fitted * s.fitted

			#make all fitted data be 0 when time is < 0
			s.fitted[srd$time < 0] <- 0
			a.fitted[srd$time < 0] <- 0
			f.fitted[srd$time < 0] <- 0
			p.fitted[srd$time < 0] <- 0

			print(length(s.fitted))
			print(length(a.fitted))
			print(length(f.fitted))
			print(length(p.fitted))

			print("srd$Mass, srd$Ka, srd$Vw:")
			print(srd$Mass)
			print(srd$Ka)
			print(srd$Vw)

			exportInstantaneous <- cbind (srd$time, srd$rawPosition,
					srd$rawSpeed, srd$rawAccel, #0s are to have same length in all variables
					srd$rawForce, srd$rawPower,
					s.fitted, a.fitted, f.fitted, p.fitted
					)

			colnames(exportInstantaneous) = c("Time", "Position",
					"Speed (raw)", "Accel (raw)", "Force (raw)", "Power (raw)",
					"Speed (fitted)", "Accel (fitted)", "Force (fitted)", "Power (fitted)")

			if(op$decimalCharAtExport == ".")
				write.csv(exportInstantaneous, file = filenameInstantaneous, row.names = FALSE, na="")
			else if(op$decimalCharAtExport == ",")
				write.csv2(exportInstantaneous, file = filenameInstantaneous, row.names = FALSE, na="")
		}
        } else
		print("Couldn't calculate the sprint model")

	return(exportRow)
}

start <- function(op)
{
	if(op$singleOrMultiple == "TRUE")
	{
		prepareGraph(op$os, pngFile, op$graphWidth, op$graphHeight)
		exportRow = testEncoderCJ(op$filename, "", op$testLength, op$splitLength, NULL,
					  op$mass, op$personHeight, op$tempC,
					  op$device, op$title, op$datetime, op$startAccel,
					  op$triggersOnList, op$triggersOffList)
		exportSprintDynamicsWriteRow (exportRow)
		endGraph()
		return()
	}

	# ------------------ op$singleOrMultiple == "FALSE" ------------------------->


	#2) read the csv
	dataFiles = read.csv(file = paste(tempPath, "/cj_race_analyzer_input_multi.csv", sep=""), sep=";", stringsAsFactors=F)

	#3) call testEncoderCJ
	progressFolder = paste(tempPath, "/chronojump_export_progress", sep ="")
	tempGraphsFolder = paste(tempPath, "/chronojump_race_analyzer_export_graphs/", sep ="")
	tempInstantFolder = paste(tempPath, "/chronojump_race_analyzer_export_instantaneous/", sep ="")
	exportDF = NULL

	#find the colums needed for different split position values
        splitPositionAll = NULL
	for(i in 1:length(dataFiles[,1]))
	{
		splitPositionAll = c(splitPositionAll, seq(from=dataFiles$splitLength[i], to=dataFiles$testLength[i], by=dataFiles$splitLength[i]))
	}
	splitPositionAll = sort(unique(splitPositionAll))
	#print("splitPositionAll")
	#print(splitPositionAll)

	for(i in 1:length(dataFiles[,1]))
	{
		print("fullURL")
		print(as.vector(dataFiles$fullURL[i]))

		pngFile <- paste(tempGraphsFolder, i, ".png", sep="")  #but remember to graph also when model fails
		prepareGraph(op$os, pngFile, op$graphWidth, op$graphHeight)
		exportRow = testEncoderCJ(
				as.vector(dataFiles$fullURL[i]), paste(tempInstantFolder, i, ".csv", sep = ""),
				dataFiles$testLength[i], dataFiles$splitLength[i], splitPositionAll,
				dataFiles$mass[i], dataFiles$personHeight[i], dataFiles$tempC[i],
				dataFiles$device[i], dataFiles$title[i], dataFiles$datetime[i],	op$startAccel,
				as.numeric(unlist(strsplit(as.character(dataFiles$triggersOn[i]), "\\,"))), #as.character() because -1 (no triggers) is readed as a number and then the strsplit fails
				as.numeric(unlist(strsplit(as.character(dataFiles$triggersOff[i]), "\\,")))
		)

		if(! is.null(exportRow))
		{
			names = names(exportRow) #exportRow is a list, get the names
			exportRow = unlist(exportRow) #convert to a vector

			exportRowDF = data.frame(dataFiles$title[i], dataFiles$datetime[i]) #create dataframe for this row with some columns
			#add exportRow data (this way we solve problems of adding strings with numbers without converting the numbers to strings
			#(to control if we print them as , or .)
			for(j in 1:length(exportRow))
				exportRowDF = cbind (exportRowDF, exportRow[j])
			exportRowDF = cbind (exportRowDF, dataFiles$comments[i])
			if(op$includeImagesOnExport)
				exportRowDF = cbind(exportRowDF, paste(i, ".png", sep=""))
			if(op$includeInstantaneousOnExport)
				exportRowDF = cbind(exportRowDF, paste(i, ".csv", sep=""))

			#write the correct names of the row dataframe
			namesDF = c("Title","Datetime",names,"comments")
			if(op$includeImagesOnExport)
				namesDF = c(namesDF, "Image")
			if(op$includeInstantaneousOnExport)
				namesDF = c(namesDF, "Instantaneous")
			colnames(exportRowDF) = namesDF

			exportDF <- rbind (exportDF, exportRowDF) #rbind with exportDF
		}

		endGraph()

		progressFilename = paste(progressFolder, "/", i, sep="")
		file.create(progressFilename)
		print("done")
	}

	#write the data frame
	#na="" to not print NA on empty comments
	if(op$decimalCharAtExport == ".")
		write.csv(exportDF, file = paste(tempPath, "/sprintResults.csv", sep = ""), row.names = FALSE, na="")
	else if(op$decimalCharAtExport == ",")
		write.csv2(exportDF, file = paste(tempPath, "/sprintResults.csv", sep = ""), row.names = FALSE, na="")
}

start(op)
