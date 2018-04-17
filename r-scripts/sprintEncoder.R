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

getSprintFromEncoder <- function(filename, testLength)
{
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
        accel = c(0,diff(speed)/elapsedTime[2:length(elapsedTime)])
        
        #Finding when the sprint starts
        trimmingSamples = getTrimmingSamples(totalTime, position, speed, accel, testLength)
        
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
                    time = time, rawPosition = position, rawSpeed = speed, rawAccel = accel,
                    startSample = trimmingSamples$start, endSample = trimmingSamples$end, testLength = testLength))
}

drawSprintFromEncoder <- function(sprint, sprintDynamics, title = "Test graph")
{
        #Plotting position
        par(mar = c(7, 4, 5, 6.5))
        plot(sprint$time[sprint$startSample:sprint$endSample], sprint$rawPosition[sprint$startSample:sprint$endSample],
             main = paste(50, "PPR"), xlab = "Time (s)", ylab = "Position (m)", type = "l")
        print(sprint$time)
        print(sprint$rawPosition)
        raceTime = interpolateXAtY(sprint$time, sprint$rawPosition, sprint$testLength)
        abline(v = raceTime)
        abline(h = sprint$testLength, lty = 3)
        points(raceTime, sprint$testLength)
        mtext(side = 3, at = raceTime, text = paste(sprint$testLength, "m", sep=""))
        mtext(side = 1, at = raceTime, text = paste(round(raceTime, digits = 3), "s", sep=""))
        
        #Calculing 5m lap times
        lapPosition = 5
        while(lapPosition < sprint$testLength)
        {
                lapTime = interpolateXAtY(sprint$time, sprint$rawPosition, lapPosition)
                
                abline(v = lapTime)
                abline(h = lapPosition, lty = 3)
                points(lapTime, lapPosition)
                mtext(side = 3, at = lapTime, text = paste(lapPosition, "m", sep=""))
                mtext(side = 1, at = lapTime, text = paste(round(lapTime, digits = 3), "s", sep=""))
                lapPosition = lapPosition + 5
        }
        
        # Getting values from the exponential model. Used for numerical calculations
        time.fitted = seq(0,sprint$time[length(sprint$time)], by = 0.01)      
        v.fitted=sprintDynamics$Vmax.fitted*(1-exp(-sprintDynamics$K.fitted*time.fitted))
        
        #Calculating the fitted dynamics
        # a.fitted = Vmax*K*exp(-K*time)
        # f.fitted = Mass*a.fitted + Ka*(v.fitted - Vw)^2
        # p.fitted = f.fitted * v.fitted
        
        #Plotting rawSpeed
        par(new = TRUE)
        plot(sprint$time[sprint$startSample:sprint$endSample], sprint$rawSpeed[sprint$startSample:sprint$endSample],
             type = "l", col = "green", axes = FALSE, xlab = "", ylab = "")
        
        #Plotting fitted speed
        lines(time.fitted, v.fitted, col = "grey")
        axis(side = 4)
        
        #Plotting rawAccel
        par(new = TRUE)
        plot(sprint$time[sprint$startSample:sprint$endSample], sprint$rawAccel[sprint$startSample:sprint$endSample],
             type = "l", col = "magenta", axes = FALSE, xlab ="", ylab = "")
}

#Detecting where the sprint start and stops
getTrimmingSamples <- function(totalTime, position, speed, accel, testLength)
{
        print(totalTime)
        #The analysis starts two samples before the maximum acceleration
        start = which.max(accel)
        while(speed[start] > 1)
        {
                start = start -1
        }
        
        #Zeroing time to the initial acceleration sample
        totalTime = totalTime - totalTime[start]
        #Zeroing position to the initial acceleration sample
        position = position - position[start]
        
        #Detecting when starts the braking phase
        end = which.min(abs(position - testLength))
        print(paste("endTime = ",totalTime[end], "s"))
        print(paste("endPosition = ",position[end], "m"))
        return(list(start = start, end = end ))
}

#Function to get the interpolated x at a given y
#TODO: Include this function in scripts-util.R
interpolateXAtY <- function(X, Y, desiredY){
        #find the closest sample
        nextSample = 1
        while (Y[nextSample] < desiredY){
                nextSample = nextSample +1
        }
        
        previousSample = nextSample - 1
        
        if(Y[nextSample] == desiredY){
                desiredX = X[nextSample]
        } else {
                desiredX = X[previousSample] + (desiredY  - Y[previousSample]) * (X[nextSample] - X[previousSample]) / (Y[nextSample] - Y[previousSample])
        }
        return(desiredX)
}

testEncoderCJ <- function(filename, testLength, mass, personHeight, tempC)
{
        sprint = getSprintFromEncoder(filename, testLength)
        sprintDynamics = getDynamicsFromSprint(K = sprint$K, Vmax = sprint$Vmax, mass, tempC, personHeight)
        print(paste("K =",sprintDynamics$K.fitted, "Vmax =", sprintDynamics$Vmax.fitted))
        drawSprintFromEncoder(sprint = sprint, sprintDynamics = sprintDynamics, title = "Testing graph")
}

prepareGraph(op$os, pngFile, op$graphWidth, op$graphHeight)
testEncoderCJ(op$filename, op$testLength, op$mass, op$personHeight, op$tempC)
endGraph()

