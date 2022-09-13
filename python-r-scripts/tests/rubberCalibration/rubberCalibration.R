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
#   Copyright (C) 2020   	Xavier Padullés <x.padulles@gmail.com>


#Calibration of a rubber using the encoder and the force sensor measurement concurrently
rubberCalibration <- function(encoderFile, gaugeFile)
{
        #TODO: make relative paths
        source("/home/xpadulles/chronojump/r-scripts/scripts-util.R")
        source("/home/xpadulles/chronojump/r-scripts/forcePosition.R")
        
        data <- read.csv(encoderFile, header=FALSE)
        
        #time in seconds, position in meters
        encoder = list(position = cumsum(matrix(as.numeric(data), ncol = 1)) / 1000
                       , time = ((1:length(data)) / 1000)
                       , force = rep(0,length(data))
        )
        
        #Finding the extremes of the encoder data to determine de repetitions
        encoderRepetitions = getRepetitions(encoder$time, encoder$position, encoder$force, 0.1, 0.1)

        #dev.off()
        par(mfrow = c(2,2))
        
        #Plotting the encoder signal
        plot(encoder$time, encoder$position, type = "l")
        points(encoder$time[encoderRepetitions$extremesSamples], encoder$position[encoderRepetitions$extremesSamples])
        text(encoder$time[encoderRepetitions$extremesSamples], encoder$position[encoderRepetitions$extremesSamples]
             #, labels = paste("(", encoder$time[encoderRepetitions$extremesSamples], ", ", encoder$position[encoderRepetitions$extremesSamples], ")", sep = "")
             , labels = (1:length(encoderRepetitions$extremesSamples))
             , pos = 3
             )
        
        #Asking what is the first extreme at which the analysis will be done
        #It must correspond to the same extreme set at the gauge signal
        encoderFirstExtreme = as.integer(readline("Encoder. What is the first extreme:"))
        
        #The starting time is used to offset the whole signal in the X axis
        encoderStartingTime = encoder$time[encoderRepetitions$extremesSamples[encoderFirstExtreme]]
        
        #Finding the extremes of the gauge data to determine de repetitions
        gaugeDynamics = getDynamicsFromForceSensor(gaugeFile
                                                   , stiffness = 0.05
                                                   , conMinDisplacement = 50, eccMinDisplacement = 50
        )
        
        #Plotting the gauge signal
        plot(gaugeDynamics$time, gaugeDynamics$rawForce, type = "l")
        points(gaugeDynamics$time[gaugeDynamics$extremesSamples], gaugeDynamics$rawForce[gaugeDynamics$extremesSamples])
        text(gaugeDynamics$time[gaugeDynamics$extremesSamples], gaugeDynamics$rawForce[gaugeDynamics$extremesSamples]
             #, labels = paste("(", round(gaugeDynamics$time[gaugeDynamics$extremesSamples], digits = 2), ", ", round(gaugeDynamics$rawForce[gaugeDynamics$extremesSamples], digits = 2), ")"
             #, sep = ""
             , labels = (1:length(gaugeDynamics$extremesSamples))
             , pos = 3
        )
        
        #Asking what is the first extreme at which the analysis will be done.
        #It must correspond to the same extreme set at the encoder signal
        gaugeFirstExtreme = as.integer(readline("Gauge. What is the first extreme:"))
        
        #The starting time is used to offset the whole signal in the X axis
        gaugeStartingTime = gaugeDynamics$time[gaugeDynamics$extremesSamples[gaugeFirstExtreme]]

        #If the number of extremes after the first extreme differs from encoder to gauge, the minimum are used in order to have the same number
        encoderRestinExtremes = length(encoderRepetitions$extremesSamples) - encoderFirstExtreme
        gaugeRestinExtremes = length(gaugeDynamics$extremesSamples) - gaugeFirstExtreme
        nExtremes = min(c(encoderRestinExtremes, gaugeRestinExtremes)) - 1
        
        #Determining the time of the last extreme
        encoderEndingTime = encoder$time[encoderRepetitions$extremesSamples[encoderFirstExtreme + nExtremes]]
        gaugeEndingTime = gaugeDynamics$time[gaugeDynamics$extremesSamples[gaugeFirstExtreme + nExtremes]]
        
        print(paste("gaugeStartingTime:", gaugeStartingTime, "gaugeEndingTime:", gaugeEndingTime))
        
        #Time offset
        print("########Encoder offset:########")
        print(encoderStartingTime)
        encoder$time = encoder$time - encoderStartingTime

        #Encoder and gauge time measures differ ~1%. It is necessary to scale the X axis
        gaugeDynamics$time = (gaugeDynamics$time - gaugeStartingTime) / (gaugeEndingTime - gaugeStartingTime) * (encoderEndingTime - encoderStartingTime)    #Time offsetted and scaled
        gaugeDynamics$rawForce = gaugeDynamics$rawForce - gaugeDynamics$rawForce[1]

        #The samples of the gauge are measured at diferent moments that the samples of the encoder.
        #It is necessary to interpolate the data of the encoder to have the encoder position at the exact time of the gauge samples
        #The interpolation is made only of the data analyzed
        samplesToInterpolate = gaugeDynamics$extremesSamples[gaugeFirstExtreme]:gaugeDynamics$extremesSamples[gaugeFirstExtreme + nExtremes]
        encoderPositions2 = interpolateXAtY(encoder$position, encoder$time, gaugeDynamics$time[samplesToInterpolate[1]])
        for (i in 2:length(samplesToInterpolate)){
                encoderPositions2 = c(encoderPositions2, interpolateXAtY(encoder$position, encoder$time, gaugeDynamics$time[samplesToInterpolate[i]]))
        }
        
        #Linear, quadratic and cubic models (first, second and third degree plynomials)
        model = lm(encoderPositions2 ~ gaugeDynamics$rawForce[samplesToInterpolate])
        model2 = lm(encoderPositions2 ~ gaugeDynamics$rawForce[samplesToInterpolate] + I(gaugeDynamics$rawForce[samplesToInterpolate]^2))
        model3 = lm(encoderPositions2 ~ gaugeDynamics$rawForce[samplesToInterpolate] + I(gaugeDynamics$rawForce[samplesToInterpolate]^2) + I(gaugeDynamics$rawForce[samplesToInterpolate]^3) )

        #encoderPositions2 and rawForce have diferent number of samples
        range = 1:length(encoderPositions2)
        plotSamples = samplesToInterpolate[range]
        
        #Plotting the linear regression
        plot(gaugeDynamics$rawForce[plotSamples], encoderPositions2[range]
             , col =  heat.colors(length(plotSamples))[range*0.8]
             , pch = 3
             #, xlim = c(0, 2), ylim = c(0,2)
        )
        abline(model$coefficients[1], model$coefficients[2])
        text(min(gaugeDynamics$rawForce[plotSamples]), max(encoderPositions2) * 0.9
             , labels = paste("X = ", round(model$coefficients[2], 4), "·F + ", round(model$coefficients[1], 4), sep ="")
             , pos = 4)
        text(min(gaugeDynamics$rawForce[plotSamples]), max(encoderPositions2) * 0.8
             , labels = paste("X = ", round(model2$coefficients[3], 4), "·F² + ", round(model2$coefficients[2], 4), "·F + ", round(model2$coefficients[1], 4), sep = "")
             , pos = 4)
        
        
        #Plotting both signals to check if they match
        #Real position measured with the encoder
        plot(gaugeDynamics$time[samplesToInterpolate], encoderPositions2, type = "l")
        
        #Position predicted by a linear model using the force
        lines(gaugeDynamics$time[samplesToInterpolate]
              , gaugeDynamics$rawForce[samplesToInterpolate]*model$coefficients[2]
              + model$coefficients[1]
              , col = "red")
        
        #Position predicted by a quadratic model using the force
        lines(gaugeDynamics$time[samplesToInterpolate]
              , model2$coefficients[3]*gaugeDynamics$rawForce[samplesToInterpolate]^2
              + model2$coefficients[2]*gaugeDynamics$rawForce[samplesToInterpolate]
              + model2$coefficients[1]
              , col = "green")
        
        #Position predicted by a cubic model using the force
        lines(gaugeDynamics$time[samplesToInterpolate]
              , model3$coefficients[4]*gaugeDynamics$rawForce[samplesToInterpolate]^3
              + model3$coefficients[3]*gaugeDynamics$rawForce[samplesToInterpolate]^2
              + model3$coefficients[2]*gaugeDynamics$rawForce[samplesToInterpolate]
              + model3$coefficients[1]
              , col = "blue")
        
        
        return(list(model1 = model, model2 = model2, model3 = model3))
}

#Make the calibration of all the files in a dir containing a "data/" subdir with the .txt and .csv files
readDir <- function(dirname)
{
        fileList = list.files(paste(dirname, "data/", sep = ""), "*.csv")
        print(fileList)
        n = length(fileList)
        results = matrix(rep(NA, n*12), ncol = 12)
        colnames(results) = c("g1.indep", "g1.f", "g1.R2", "g2.indep", "g2.f", "g2.f2", "g2.R2", "g3.indep", "g3.f", "g3.f2", "g3.f3", "g3.R2")
        for(i in 1:n){
                calibration = rubberCalibration(paste(dirname, "data/", i, ".txt", sep = "")
                                                , paste(dirname, "data/", i, ".csv", sep ="")
                )
                results[i,"g1.indep"] = calibration$model1$coefficients[1]
                results[i,"g1.f"] = calibration$model1$coefficients[2]
                results[i,"g1.R2"] = summary(calibration$model1)$r.squared
                
                results[i,"g2.indep"] = calibration$model2$coefficients[1]
                results[i,"g2.f"] = calibration$model2$coefficients[2]
                results[i,"g2.f2"] = calibration$model2$coefficients[3]
                results[i,"g2.R2"] = summary(calibration$model2)$r.squared
                
                results[i,"g3.indep"] = calibration$model3$coefficients[1]
                results[i,"g3.f"] = calibration$model3$coefficients[2]
                results[i,"g3.f2"] = calibration$model3$coefficients[3]
                results[i,"g3.f3"] = calibration$model3$coefficients[4]
                results[i,"g3.R2"] = summary(calibration$model3)$r.squared
        }
        write.csv2(results, paste(dirname, "results.csv", sep = ""))
}
readDir("~/ownCloud/Xavier/Chronojump/Projectes/RepeticionsGalga/galga+encoder/")
