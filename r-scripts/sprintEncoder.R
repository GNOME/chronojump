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
#   Copyright (C) 2018   	Xavier Padullés <x.padulles@gmail.com>

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
                graphHeight	= as.numeric(options[9])
        ))
}

#-------------- assign options -------------
op <- assignOptions(options)

getSprintFromEncoder <- function(filename, testLength, Mass, Temperature = 25, Height , Vw = 0)
{
        # Constants for the air friction modeling
        ro0 = 1.293
        Pb = 760
        Cd = 0.9
        ro = ro0*(Pb/760)*273/(273 + Temperature)
        Af = 0.2025*(Height^0.725)*(Mass^0.425)*0.266 # Model of the frontal area
        Ka = 0.5*ro*Af*Cd
        
        encoderCarrera = read.csv2(file = filename, sep = ";")
        colnames(encoderCarrera) = c("displacement", "time")
        totalTime = encoderCarrera$time/1E6     #Converting microseconds to seconds
        elapsedTime = diff(c(0,totalTime))      #The elapsed time between each sample
        
        #The encoder can be used with both loose ends of the rope. This means that the encoder can rotate
        #in both directions. We considre always the speed as positive.
        if(mean(encoderCarrera$displacement) < 0)
                encoderCarrera$displacement = -encoderCarrera$displacement
        diameter = 0.16         #Diameter of the pulley
        #encoderCarrera$displacement = encoderCarrera$displacement * 2 * pi * diameter / 200
        #In 30m there are 12267 pulses.
        #TODO: measure this several times to have an accurate value
        encoderCarrera$displacement = encoderCarrera$displacement * 30 / 12267
        position = cumsum(encoderCarrera$displacement)
        speed = encoderCarrera$displacement / elapsedTime
        #accel = c(0,diff(speed)/elapsedTime[2:length(elapsedTime)])
        accel = (speed[3] - speed[1]) / (totalTime[3] - totalTime[1])
        for(i in 3:length(speed))
        {
                accel = c(accel, (speed[i+1] - speed[i-1]) / (totalTime[i +1] - totalTime[i -1]))
        }
        accel = c(accel, accel[length(accel)])
        force = accel * Mass + Ka*(speed - Vw)^2
        power = force * speed
        
        #Finding when the sprint starts
        trimmingSamples = getTrimmingSamples(totalTime, position, speed, accel, testLength)
        print(trimmingSamples)
        #Zeroing time to the initial acceleration sample
        time = totalTime - totalTime[trimmingSamples$start]
        #Zeroing position to the initial acceleration sample
        position = position - position[trimmingSamples$start]
        data = data.frame(time = time[trimmingSamples$start:trimmingSamples$end], speed = speed[trimmingSamples$start:trimmingSamples$end])
        model = nls(speed ~ Vmax*(1-exp(-K*time)), data,
                    start = list(Vmax = max(speed), K = 1), control=nls.control(warnOnly=TRUE))
        Vmax =summary(model)$coeff[1,1]
        K = summary(model)$coeff[2,1]
        return(list(Vmax = Vmax, K = K,
                    time = time, rawPosition = position, rawSpeed = speed, rawAccel = accel, rawForce = force, rawPower = power,
                    rawVmax = max(speed[trimmingSamples$start:trimmingSamples$end]), rawAmax = max(accel[trimmingSamples$start:trimmingSamples$end]), rawFmax = max(force[trimmingSamples$start:trimmingSamples$end]), rawPmax = max(power[trimmingSamples$start:trimmingSamples$end]),
                    startSample = trimmingSamples$start, endSample = trimmingSamples$end, testLength = testLength))
}

plotSprintFromEncoder <- function(sprintRawDynamics, sprintFittedDynamics, title = "Test graph",
                                  plotRawSpeed = TRUE, plotRawAccel = TRUE, plotRawForce = FALSE, plotRawPower = FALSE,
                                  plotFittedSpeed = TRUE, plotFittedAccel = FALSE, plotFittedForce = FALSE, plotFittedPower = FALSE)
{
        #Plotting position
        # plot(sprintRawDynamics$time[sprintRawDynamics$startSample:sprintRawDynamics$endSample], sprintRawDynamics$rawPosition[sprintRawDynamics$startSample:sprintRawDynamics$endSample],
        #      main = paste(50, "PPR"), xlab = "Time (s)", ylab = "Position (m)", type = "l", yaxs= "i", xaxs = "i")
        # raceTime = interpolateXAtY(sprintRawDynamics$time, sprintRawDynamics$rawPosition, sprintRawDynamics$testLength)
        # abline(v = raceTime)
        # abline(h = sprintRawDynamics$testLength, lty = 3)
        # points(raceTime, sprintRawDynamics$testLength)
        # mtext(side = 3, at = raceTime, text = paste(sprintRawDynamics$testLength, "m", sep=""))
        # mtext(side = 1, at = raceTime, text = paste(round(raceTime, digits = 3), "s", sep=""))

        par(mar = c(7, 4, 5, 6.5))
        
        #Checking that the position reach at least testLength
        if(max(sprintRawDynamics$rawPosition) < sprintRawDynamics$testLength)
        {
                plot(sprintRawDynamics$time, sprintRawDynamics$rawPosition, type = "l",
                     ylim = c(min(sprintRawDynamics$rawPosition), sprintRawDynamics$testLength)*1.05,
                     xlab = "Time (s)", ylab = "Position (m)")
                abline(h = sprintRawDynamics$testLength, lty = 2)
                text(x = (sprintRawDynamics$time[length(sprintRawDynamics$time)] + sprintRawDynamics$time[1])/2,
                     y = sprintRawDynamics$testLength,
                     labels = (paste("Configured test length :", sprintRawDynamics$testLength, "m", sep = "")),
                     pos = 3)
                text(x = (sprintRawDynamics$time[length(sprintRawDynamics$time)] + sprintRawDynamics$time[1])/2, sprintRawDynamics$testLength /2,
                     labels = "The capture does not reach the length of the test", cex = 2, pos = 3)
                print("Capture too short")
                return("Capture too short")
        }
        
        #Plotting rawSpeed
        ylimits = c(0, sprintRawDynamics$rawVmax*1.05)
        xlimits =c(0, sprintRawDynamics$time[sprintRawDynamics$endSample]*1.05)
        plot(sprintRawDynamics$time[sprintRawDynamics$startSample:sprintRawDynamics$endSample], sprintRawDynamics$rawSpeed[sprintRawDynamics$startSample:sprintRawDynamics$endSample],
             #plot(sprintRawDynamics$time, sprintRawDynamics$rawSpeed,
             type = "l",
             ylim = ylimits, xlim = xlimits,
             main = title, xlab = "Time(s)", ylab = "Speed(m/s)",
             yaxs = "i", xaxs = "i")
        legendText = paste("Vmax.raw =", round(max(sprintRawDynamics$rawSpeed), digits = 2), "m/s")
        legendColor = "black"
        
        #Calculing 5m lap times
        lapPosition = 5
        while(lapPosition < sprintRawDynamics$testLength)
        {
                #print(paste("lapPosition :", lapPosition))
                lapTime = interpolateXAtY(sprintRawDynamics$time[sprintRawDynamics$startSample:sprintRawDynamics$endSample],
                                          sprintRawDynamics$rawPosition[sprintRawDynamics$startSample:sprintRawDynamics$endSample], lapPosition)
                abline(v = lapTime)
                #abline(h = lapPosition, lty = 3)
                #points(lapTime, lapPosition)
                mtext(side = 3, at = lapTime, text = paste(lapPosition, "m", sep=""))
                mtext(side = 1, at = lapTime, text = paste(round(lapTime, digits = 3), "s", sep=""))
                lapPosition = lapPosition + 5
        }
        #Plot the total time of the test
        lapPosition = sprintRawDynamics$testLength
        lapTime = interpolateXAtY(sprintRawDynamics$time, sprintRawDynamics$rawPosition, lapPosition)
        abline(v = lapTime)
        #abline(h = lapPosition, lty = 3)
        #points(lapTime, lapPosition)
        mtext(side = 3, at = lapTime, text = paste(lapPosition, "m", sep=""))
        mtext(side = 1, at = lapTime, text = paste(round(lapTime, digits = 3), "s", sep=""))
        lapPosition = lapPosition + 5
        
        # par(new = TRUE)
        # plot(sprintRawDynamics$time[sprintRawDynamics$startSample:sprintRawDynamics$endSample], sprintRawDynamics$rawPosition[sprintRawDynamics$startSample:sprintRawDynamics$endSample],
        #            main = paste(50, "PPR"), xlab = "Time (s)", ylab = "Position (m)", type = "l", yaxs= "i", xaxs = "i")
        # abline(h = 5)
        

        if (plotFittedSpeed)
        {
                #Plotting fitted speed
                lines(sprintFittedDynamics$t.fitted, sprintFittedDynamics$v.fitted, lty = 2)
                legendText = c(legendText, paste("Vmax.fitted =", round(sprintFittedDynamics$Vmax.fitted, digits = 2), "m/s"))
                legendColor = c(legendColor, "black")
        }
        
        if (plotRawAccel || plotFittedAccel)
        {
                ylimits = c(min(sprintRawDynamics$rawAccel[sprintRawDynamics$startSample:sprintRawDynamics$endSample])*0.95, max(c(sprintRawDynamics$rawAmax, sprintFittedDynamics$amax.fitted)*1.05))
                if (plotRawAccel)
                {
                        #Plotting rawAccel
                        par(new = TRUE)
                        plot(sprintRawDynamics$time[sprintRawDynamics$startSample:sprintRawDynamics$endSample], sprintRawDynamics$rawAccel[sprintRawDynamics$startSample:sprintRawDynamics$endSample],
                             ylim = ylimits, type = "l", col = "magenta",
                             xlab = "", ylab = "",
                             axes = FALSE, yaxs = "i", xaxs = "i")
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
                        legendText = c(legendText, paste("Amax.fitted =", round(max(sprintFittedDynamics$amax.fitted), digits = 2), "m/s^2"))
                        legendColor = c(legendColor, "magenta")
                }
                axis(side = 4, col = "magenta")
        }
        
        if(plotRawForce|| plotFittedForce)
        {
                ylimits = c(min(sprintRawDynamics$rawForce[sprintRawDynamics$startSample:sprintRawDynamics$endSample])*0.95, max(c(sprintRawDynamics$rawFmax, sprintFittedDynamics$fmax.fitted)*1.05))
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
                        #Plotting rawAccel
                        par(new = TRUE)
                        plot(sprintFittedDynamics$t.fitted, sprintFittedDynamics$f.fitted,
                             ylim = ylimits, type = "l", col = "blue", lty = 2,
                             xlab = "", ylab = "",
                             axes = FALSE, yaxs = "i", xaxs = "i")
                        legendText = c(legendText, paste("Fmax.fitted =", round(sprintFittedDynamics$fmax.fitted, digits = 2), "N"))
                        legendColor = c(legendColor, "blue")
                }
                axis(side = 4, col = "blue", line = 2)
        }
        
        if(plotRawPower|| plotFittedPower)
        {
                ylimits = c(min(sprintRawDynamics$rawPower[sprintRawDynamics$startSample:sprintRawDynamics$endSample])*0.95, max(c(sprintRawDynamics$rawPmax, sprintFittedDynamics$pmax.fitted)*1.05))
                if (plotRawPower)
                {
                        #Plotting rawForce
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
                        #Plotting rawAccel
                        par(new = TRUE)
                        plot(sprintFittedDynamics$t.fitted, sprintFittedDynamics$p.fitted,
                             ylim = ylimits, type = "l", col = "red", lty = 2,
                             xlab = "", ylab = "",
                             axes = FALSE, yaxs = "i", xaxs = "i")
                        legendText = c(legendText, paste("Pmax.fitted =", round(sprintFittedDynamics$pmax.fitted, digits = 2), "N"))
                        legendColor = c(legendColor, "red")
                }
                axis(side = 4, col = "red", line = 4)
        }
        plotSize = par("usr")
        legend(x = plotSize[2], y = plotSize[3] + (plotSize[4] - plotSize[3])*0.66,
                xjust = 1, yjust = 0.5, cex = 1,
                legend = legendText,
                text.col = legendColor)
}

#Detecting where the sprint start and stops
getTrimmingSamples <- function(totalTime, position, speed, accel, testLength)
{
        #The test starts when the speed is grater than 1
        startSample = 0
        startingSample = FALSE
        while(!startingSample)
        {
                startSample = startSample +1
                if(accel[startSample+1] > 5)
                {
                        #Looking is after 2 seconds the position has increased a meter at least.
                        sampleAfterSecond = which.min(totalTime - (totalTime[startSample] +1) )
                        positionAfterSecnod = position[sampleAfterSecond]
                        if(abs(positionAfterSecnod - position[startSample]) > 2){
                                startingSample = TRUE
                        }
                }
        }
        
        #Going back in the time to find a really slow velocity
        while(speed[startSample] > 0.5)
        {
                startSample = startSample -1
        }
        
        #Zeroing time to the initial acceleration sample
        totalTime = totalTime - totalTime[startSample]
        #Zeroing position to the initial acceleration sample
        position = position - position[startSample]
        
        #Detecting when starts the braking phase
        endSample = which.min(abs(position - testLength))
        return(list(start = startSample, end = endSample ))
}

testEncoderCJ <- function(filename, testLength, mass, personHeight, tempC)
{
        sprintRawDynamics = getSprintFromEncoder(filename, testLength, op$mass, op$tempC, op$personHeight)
        sprintFittedDynamics = getDynamicsFromSprint(K = sprintRawDynamics$K, Vmax = sprintRawDynamics$Vmax, mass, tempC, personHeight)
        print(paste("K =",sprintFittedDynamics$K.fitted, "Vmax =", sprintFittedDynamics$Vmax.fitted))
        plotSprintFromEncoder(sprintRawDynamic = sprintRawDynamics, sprintFittedDynamics = sprintFittedDynamics, title = "Testing graph")
        exportSprintDynamics(sprintFittedDynamics)
}

prepareGraph(op$os, pngFile, op$graphWidth, op$graphHeight)
testEncoderCJ(op$filename, op$testLength, op$mass, op$personHeight, op$tempC)
endGraph()
