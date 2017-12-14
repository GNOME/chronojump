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

#call from Chronojump or:

#Rscript path_to/maximumIsometricForce.R path_tmp

prepareGraph <- function(os, pngFile, width, height)
{
        if(os == "Windows"){
                library("Cairo")
                Cairo(width, height, file = pngFile, type="png", bg="white")
        }
        else
                png(pngFile, width=width, height=height)
        #pdf(file = "/tmp/maxIsomForce.pdf", width=width, height=height)
}

#Ends the graph

endGraph <- function()
{
        dev.off()
}

#Read each non commented line of the Roptions file

assignOptions <- function(options)
{
        drawRfdOptions = rep(NA, 4)
        drawRfdOptions[1]      = options[12]
        drawRfdOptions[2]      = options[13]
        drawRfdOptions[3]      = options[14]
        drawRfdOptions[4]      = options[15]
        
        return(list(
                os 			= options[1],
                decimalChar 		= options[2],
                graphWidth 		= as.numeric(options[3]),
                graphHeight		= as.numeric(options[4]),
                averageLength 		= as.numeric(options[5]),
                percentChange 		= as.numeric(options[6]),
                vlineT0 		= options[7],
                vline50fmax.raw 	= options[8],
                vline50fmax.fitted 	= options[9],
                hline50fmax.raw 	= options[10],
                hline50fmax.fitted 	= options[11],
                drawRfdOptions          = drawRfdOptions,
                drawImpulseOptions      = options[16],
                testLength 		= as.numeric(options[17])
        ))
}

#-------------- get params -------------
args <- commandArgs(TRUE)

tempPath <- args[1]
optionsFile <- paste(tempPath, "/Roptions.txt", sep="")
dataFile <- paste(tempPath, "/cj_mif_Data.csv", sep="")
pngFile <- paste(tempPath, "/cj_mif_Graph.png", sep="")

#-------------- scan options file -------------
options <- scan(optionsFile, comment.char="#", what=character(), sep="\n")

#-------------- assign options -------------
op <- assignOptions(options)
print(op)


#Fits the data to the model f = fmax*(1 - exp(-K*t))
#Important! It fits the data with the axes moved to initf and startTime. The real maximum force is fmax + initf
getForceModel <- function(time, force, startTime, # startTime is the instant when the force start to increase
                          fmaxi,           # fmaxi is the initial value for the force. For numeric purpouses
                          initf)              # initf is the sustained force before the increase
{
        #We force that the function crosses the (0,0) to better fit the monoexponential
        force = force
        time = time - startTime
        
        data = data.frame(time = time, force = force)
        model = nls( force ~ fmax*(1-exp(-K*time)), data, start=list(fmax=fmaxi, K=1), control=nls.control(warnOnly=TRUE))
        fmax = summary(model)$coeff[1,1]
        K = summary(model)$coeff[2,1]
        return(list(fmax = fmax, K = K))
}

getDynamicsFromLoadCellFile <- function(inputFile, averageLength = 0.1, percentChange = 5)
{
        originalTest = read.csv(inputFile, header = F, dec = op$decimalChar, sep = ";", skip = 2)
        colnames(originalTest) <- c("time", "force")
        originalTest$time = as.numeric(originalTest$time / 1000000)  # Time is converted from microseconds to seconds
        
        #Instantaneous RFD
        rfd = getRFD(originalTest)
        
        #Finding the decrease of the foce to detect the end of the maximum voluntary force
        trimmingSamples = getTrimmingSamples(originalTest, rfd, averageLength = averageLength, percentChange = percentChange,
					     testLength = op$testLength)
        startSample = trimmingSamples$startSample
        startTime = originalTest$time[startSample]
        
        endSample = trimmingSamples$endSample
        endTime = originalTest$time[endSample]
        
        # Initial force. It is needed to perform an initial steady force to avoid jerks and great peaks in the force
        if(startSample <= 20)
        {
                print("Not previos steady tension applied before performing the test")
                return(NA)
        }
        

        initf = mean(originalTest$force[(startSample - 20):(startSample - 10)]) #ATENTION. This value is different from f0.raw
        fmax.raw = max(originalTest$force[startSample:endSample])
        
        #Trimming the data before and after contraction
        testTrimmed = originalTest[startSample:endSample,]
        
        f.smoothed = getMovingAverageForce(originalTest, averageLength = averageLength) #Running average with equal weight averageLength seconds
        fmax.smoothed = max(f.smoothed, na.rm = TRUE)
        
        model = getForceModel(testTrimmed$time, testTrimmed$force, startTime, fmax.smoothed, initf)
        f.fitted =model$fmax*(1-exp(-model$K*(originalTest$time - startTime)))
        
        f0.raw = testTrimmed$force[1]                                                  # Force at t=0ms. ATENTION. This value is different than initf
        rfd0.fitted = model$fmax * model$K                                      # RFD at t=0ms using the exponential model
        tfmax.raw = testTrimmed$time[which.min(abs(testTrimmed$force - fmax.raw))]            # Time needed to reach the Fmax
        
        return(list(nameOfFile = inputFile, time = originalTest[, "time"],
                    fmax.fitted = model$fmax, k.fitted = model$K, tau.fitted = 1/model$K,
                    startTime = startTime, endTime = endTime,
                    startSample = startSample, endSample = endSample,
                    totalSample = length(originalTest$time),
                    initf = initf,
                    f0.raw = f0.raw,
                    fmax.raw = fmax.raw, fmax.smoothed = fmax.smoothed,
                    tfmax.raw = tfmax.raw,
                    rfd = rfd,
                    f.raw = originalTest$force, f.smoothed = f.smoothed, f.fitted = f.fitted,
                    endTime = endTime))
}

drawDynamicsFromLoadCell <- function(
        dynamics, vlineT0=T, vline50fmax.raw=F, vline50fmax.fitted=F,
        hline50fmax.raw=F, hline50fmax.fitted=F,
        rfdDrawingOptions, xlimits = NA)
{
        dynamics$time = dynamics$time - dynamics$startTime
        dynamics$tfmax.raw = dynamics$tfmax.raw - dynamics$startTime
        dynamics$endTime = dynamics$endTime - dynamics$startTime
        dynamics$startTime = 0
        if(is.na(dynamics$time[1]))
        {
                print("Dynamics not available:")
                return(NA)
        }
        par(mar = c(6, 4, 6, 4))
        
        #Detecting if the duration of the sustained force enough
        meanForce = mean(dynamics$f.raw[dynamics$startSample:dynamics$endSample])
        if( meanForce < dynamics$fmax.raw*0.75 ){
                sustainedForce = F
                yHeight = dynamics$fmax.raw
        } else if( meanForce >= dynamics$fmax.raw*0.75 ){
                sustainedForce = T
                yHeight = max(dynamics$fmax.raw, dynamics$fmax.fitted) * 1.1
        }
                
	par(mar=c(4,4,1,1))
        #Plotting raw data from startTime to endTime (Only the analysed data)
        if (!is.na(xlimits[1])){
                xWidth = xlimits[2] - xlimits[1]
                plot(dynamics$time[dynamics$startSample:dynamics$endSample], dynamics$f.raw[dynamics$startSample:dynamics$endSample],
                     type="l", xlab="Time[s]", ylab="Force[N]",
                     xlim = xlimits, ylim=c(0, yHeight),
                     #main = dynamics$nameOfFile,
		     yaxs= "i", xaxs = "i")
                xmin = xlimits[1]
                xmax = xlimits[2]
                #points(dynamics$time[dynamics$startSample:dynamics$endSample] , dynamics$f.raw[dynamics$startSample:dynamics$endSample])
        } else if (is.na(xlimits[1])){
                xmin = -0.2
                xmax = dynamics$endTime*1.1 - dynamics$startTime*0.1
                xWidth = xmax - xmin
                plot(dynamics$time[dynamics$startSample:dynamics$endSample], dynamics$f.raw[dynamics$startSample:dynamics$endSample],
                     type="l", xlab="Time[s]", ylab="Force[N]",
                     xlim = c(xmin, xmax),
                     ylim=c(0, yHeight),
                     #main = dynamics$nameOfFile,
		     yaxs= "i", xaxs = "i")
        }
        

        if(!sustainedForce){
                text(x = (xmax + xmin)/2, y = yHeight/2, labels = "Bad execution. Probably not sustained force", adj = c(0.5, 0.5), cex = 2, col = "red")
                #Plotting not analysed data
                lines(dynamics$time[1:dynamics$startSample] , dynamics$f.raw[1:dynamics$startSample], col = "grey") #Pre-analysis
                lines(dynamics$time[dynamics$endSample: dynamics$totalSample] , dynamics$f.raw[dynamics$endSample: dynamics$totalSample], col = "grey") #Post-analysis
                return()
        }
        
        #Plotting Impulse
        
        print("--------Impulse-----------")
        print(op$drawImpulseOptions)
        impulseOptions = readImpulseOptions(op$drawImpulseOptions)

	impulseLegend = ""
        
        if(impulseOptions$impulseFunction != "-1")
        {
                print(impulseOptions)
                if(impulseOptions$type == "IMP_RANGE")
                {
                        startImpulseSample = which.min(abs(dynamics$time - impulseOptions$start/1000))
                        endImpulseSample = which.min(abs(dynamics$time - impulseOptions$end/1000))
                } else if(impulseOptions$type == "IMP_UNTIL_PERCENT_F_MAX")
                {
                        startImpulseSample = dynamics$startSample
                        
                        #Finding the sample at which the force is greater that percentage of fmax
                        endImpulseSample = startImpulseSample
                        while(dynamics$f.raw[endImpulseSample + 1] < dynamics$fmax.raw*impulseOptions$end/100)
                        {
                                endImpulseSample = endImpulseSample +1
                                
                        }
                }

                if(impulseOptions$impulseFunction == "RAW")
                {
                        
                        #Drawing the area under the force curve (Impulse)
                        polygon(c(dynamics$time[startImpulseSample:endImpulseSample], dynamics$time[endImpulseSample] , dynamics$time[startImpulseSample]),
                                c(dynamics$f.raw[startImpulseSample:endImpulseSample], 0, 0), col = "grey")
                        
                        #Calculation of the impulse
                        #Sum of the area of all the triangles formed with a vertex in the origin and the other vertex in the
                        #n-th and (n+1)-th point of the polygon
                        impulse = 0
                        for(n in startImpulseSample:(endImpulseSample - 1))
                        {
                                #Area of the paralelograms, not the triangle
                                area = ((dynamics$time[n+1] - dynamics$time[dynamics$startSample]) * dynamics$f.raw[n] - (dynamics$time[n] - dynamics$time[dynamics$startSample]) * dynamics$f.raw[n+1])
                                impulse = impulse + area
                        }
                        
                        area = (dynamics$time[endImpulseSample] - dynamics$time[dynamics$startSample]) * dynamics$f.raw[endImpulseSample]
                        impulse = impulse + area
                        
                        #Area under the curve is one half of the sum of the area of paralelograms
                        impulse = impulse / 2
                } else if(impulseOptions$impulseFunction == "FITTED")
                {
                        
                        #Drawing the area under the force curve (Impulse)
                        polygon(c(dynamics$time[startImpulseSample:endImpulseSample], dynamics$time[endImpulseSample] , dynamics$time[startImpulseSample]),
                                c(dynamics$f.fitted[startImpulseSample:endImpulseSample], 0, 0), col = "grey")
                        #Redraw the raw force
                        lines(x = dynamics$time[startImpulseSample:endImpulseSample] , y = dynamics$f.raw[startImpulseSample:endImpulseSample])
                        
                        #Calculation of the impulse
                        #Sum of the area of all the triangles formed with a vertex in the origin and the other vertex in the
                        #n-th and (n+1)-th point of the polygon
                        impulse = 0
                        for(n in startImpulseSample:(endImpulseSample - 1))
                        {
                                #Area of the paralelograms, not the triangle
                                area = ((dynamics$time[n+1] - dynamics$time[dynamics$startSample]) * dynamics$f.fitted[n] - (dynamics$time[n] - dynamics$time[dynamics$startSample]) * dynamics$f.fitted[n+1])
                                impulse = impulse + area
                        }
                        
                        area = (dynamics$time[endImpulseSample] - dynamics$time[dynamics$startSample]) * dynamics$f.fitted[endImpulseSample]
                        impulse = impulse + area
                        
                        #Area under the curve is one half of the sum of the area of paralelograms
                        impulse = impulse / 2
                }
                
                text(x = (dynamics$startTime + (dynamics$time[endImpulseSample] - dynamics$time[startImpulseSample])*0.66), y = mean(dynamics$f.raw[startImpulseSample:endImpulseSample])*0.66,
                     labels = paste("Impulse =", round(impulse, digits = 2), "N·s"), pos = 4, cex = 1.5)

		impulseLegend = paste("Impulse", impulseOptions$start, "-", impulseOptions$end, " = ", round(impulse, digits = 2), " N·s", sep = "")
        }
        
        #Plotting not analysed data
        lines(dynamics$time[1:dynamics$startSample] , dynamics$f.raw[1:dynamics$startSample], col = "grey") #Pre-analysis
        lines(dynamics$time[dynamics$endSample: dynamics$totalSample] , dynamics$f.raw[dynamics$endSample: dynamics$totalSample], col = "grey") #Post-analysis
        
        
        #Plotting fitted data
        lines(dynamics$time, dynamics$f.fitted, col="blue")
        abline(h = dynamics$fmax.fitted, lty = 2, col = "blue")
        text(x = xmin + (xmax - xmin)*0.25, y = dynamics$fmax.fitted,
             labels = paste("Fmax =", round(dynamics$fmax.fitted,digits = 2), "N"),
             col = "blue", pos = 3, cex = 1.5)
        
        #Plottting smoothed data
        #lines(dynamics$time, dynamics$f.smoothed, col="grey")
        
        #Plotting RFD
        #lines(dynamics$time, dynamics$rfd/100, col = "red")
        
        #Plotting tau
        abline(v = dynamics$tau.fitted, col = "green4", lty = 3)
        abline(h = dynamics$fmax.fitted*0.6321206, col = "green4", lty = 3)
        points(dynamics$tau.fitted, dynamics$fmax.fitted*0.6321206, col = "green4")
        arrows(x0 = 0, y0 = dynamics$f0.raw,
               x1 = dynamics$tau.fitted, y1 = dynamics$f0.raw)
        text(x = (dynamics$tau.fitted / 2), y = dynamics$f0.raw,
              labels = paste("τ =", round(dynamics$tau.fitted, digits = 2), "s"), pos = 3, cex = 1.5)
        # text(x = (dynamics$tau.fitted / 2), y = -20,
        #      labels = paste("τ =", round(dynamics$tau.fitted, digits = 2), "s"), pos = 3, cex = 1.5, xpd = TRUE)
        
        arrows(x0 = 0, y0 = 0,
               x1 = 0, y1 = dynamics$fmax.fitted*0.6321206)
        
        text(x = 0, y = dynamics$fmax.fitted*0.6321206 / 2,
              labels = "63% of fmax", pos = 2, cex = 1.5, srt = 90)
        
        #Plotting fmax.raw
        text( x = dynamics$tfmax.raw, y = dynamics$fmax.raw,
              labels = paste("Fmax = ", round(dynamics$fmax.raw, digits=2), " N", sep=""), pos = 3, cex = 1.5)
        points(x = dynamics$tfmax.raw, y = dynamics$fmax.raw)
        
        if(vlineT0){
                abline(v = dynamics$startTime, lty = 2)
                axis(3, at = dynamics$startTime, labels = dynamics$startTime)
        }
        if(hline50fmax.raw){
                abline(h = dynamics$fmax.raw/2, lty = 2)
                text( x = dynamics$t50fmax.raw + 0.5, y = dynamics$fmax.raw/2, labels = paste("Fmax/2 =", round(dynamics$fmax.raw/2, digits=2), "N", sep=""), pos = 3)
        }
        if(hline50fmax.fitted){
                abline( h = (dynamics$fmax.fitted)/2, lty = 2, col = "blue")
                text( x =dynamics$t50fmax.fitted + 0.5, y = (dynamics$fmax.fitted)/2, labels = paste("Fmax/2 =", round((dynamics$fmax.fitted)/2, digits=2), "N", sep=""), pos = 1, col="blue")
        }
        if(vline50fmax.raw){
                abline(v = dynamics$t50fmax.raw, lty = 2)
                axis(3, at = dynamics$t50fmax.raw, labels = dynamics$t50fmax.raw)
        }
        if(vline50fmax.fitted){
                abline(v = dynamics$t50fmax.fitted, lty = 2)
                axis(3, at = dynamics$t50fmax.fitted, labels = dynamics$t50fmax.fitted)
        }
        
        #The angle in the srt parameter of the text is an absolute angle seen in the window, not the angle in the coordinates system of the plot area.
        pixelsPerLine = 14    #This value has been found with test and error.
        
        # 72 dpi is the default resolutions for pdf. Margins units are text hight
        plotWidth = dev.size()[1]*72 - (par()$mar[2] + par()$mar[4])*pixelsPerLine      
        plotHeight = dev.size()[2]*72 - (par()$mar[1] + par()$mar[3])*pixelsPerLine
        
        #Drawing the RFD data
        print("-----------RFD-----------")
        print(paste("op$drawRfdOptions =", op$drawRfdOptions))
        
        legendText = c(
		       paste("Fmax =", round(dynamics$fmax.fitted, digits = 2), "N"),
		       paste("K = ", round(dynamics$k.fitted, digits = 2),"s⁻¹"),
		       paste("τ = ", round(dynamics$tau.fitted, digits = 2),"s")
		       )
        legendColor = c("blue", "blue")
        
        #The coordinates where the lines and dots are plotted are calculated with the sampled data in raw and fitted data.
        #The slopes are calculated in that points
        for (n in 1:length(rfdDrawingOptions))
        {
                RFDoptions = readRFDOptions(op$drawRfdOptions[n])
                
                print(paste("---- RFD number", n, "--------"))
                print(options)
                if(RFDoptions$rfdFunction == "-1")        
                {
                        next
                } else
                {
                        
                        RFD = NULL
                        pointForce1 = NULL
                        color = ""
                        
                        #Needed when only one point is plotted
                        sample2 = NULL
                        pointForce2 = NULL
                        time2 = NULL
                        force2 = NULL
                        
                        if(RFDoptions$rfdFunction == "FITTED")
                        {
                                color = "blue"
                        } else if(RFDoptions$rfdFunction == "RAW")
                        {
                                color = "black"
                        }
                        
                        if(RFDoptions$type == "INSTANTANEOUS") # TODO: || percent ...(all except AVG)
                        {
                                #Converting RFDoptions$start to seconds
                                RFDoptions$start = RFDoptions$start/1000
                                
                                time1 = RFDoptions$start
                                
                                if (RFDoptions$rfdFunction == "FITTED")
                                {
                                        #Slope of the line. Deriving the model:
                                        RFD = dynamics$fmax.fitted * dynamics$k.fitted * exp(-dynamics$k.fitted * time1)
                                        #Y coordinate of a point of the line
                                        force1 = dynamics$fmax.fitted * (1 - exp(-dynamics$k.fitted * RFDoptions$start))
                                        
                                        legendText = c(legendText, paste("RFD", RFDoptions$start*1000, " = ", round(RFD, digits = 1), " N/s", sep = ""))
                                        legendColor = c(legendColor, "blue")
                                        
                                } else if(RFDoptions$rfdFunction == "RAW")
                                {
                                        #Slope of the line of the sampled point.
                                        RFD = getForceAtTime(dynamics$time, dynamics$rfd, time1)
                                        
                                        #Y coordinate of a point of the line
                                        force1 = getForceAtTime(dynamics$time, dynamics$f.raw, RFDoptions$start)
                                        
                                        legendText = c(legendText, paste("RFD", RFDoptions$start*1000, " = ", round(RFD, digits = 1), " N/s", sep = ""))
                                        legendColor = c(legendColor, "black")
                                }
                        } else if(RFDoptions$type == "AVERAGE")
                        {
                                #Converting RFDoptions to seconds
                                RFDoptions$start = RFDoptions$start/1000
                                RFDoptions$end = RFDoptions$end/1000
                                
                                time1 = RFDoptions$start
                                time2 = RFDoptions$end
                                
                                if (RFDoptions$rfdFunction == "FITTED")
                                {
                                        #Y axis of the points
                                        force1 = dynamics$fmax.fitted * (1 - exp(-dynamics$k.fitted * RFDoptions$start))
                                        force2 = dynamics$fmax.fitted * (1 - exp(-dynamics$k.fitted * RFDoptions$end))
                                        
                                        #Slope of the line
                                        RFD = (force2 - force1) / (time2 - time1)
                                        
                                        legendText = c(legendText, paste("RFD", RFDoptions$start*1000, "-", RFDoptions$end*1000, " = ", round(RFD, digits = 1), " N/s", sep = ""))
                                        legendColor = c(legendColor, "blue")
                                        
                                } else if(RFDoptions$rfdFunction == "RAW")
                                {
                                        force1 = getForceAtTime(dynamics$time, dynamics$f.raw, RFDoptions$start)
                                        force2 = getForceAtTime(dynamics$time, dynamics$f.raw, RFDoptions$end)
                                        
                                        #Slope of the line
                                        RFD = (force2 - force1) / (time2 - time1)
                                        
                                        legendText = c(legendText, paste("RFD", RFDoptions$start*1000, "-", RFDoptions$end*1000, " = ", round(RFD, digits = 1), " N/s", sep = ""))
                                        legendColor = c(legendColor, "black")
                                }
                                
                        } else if(RFDoptions$type == "PERCENT_F_MAX")
                        {
                                percent = RFDoptions$start
                                
                                if (RFDoptions$rfdFunction == "FITTED")
                                {
                                        time1 = - log(1 - percent/100) / dynamics$k.fitted
                                        
                                        #RFD at the point with a % of the fmax.fitted
                                        RFD = dynamics$fmax.fitted * dynamics$k.fitted * exp(-dynamics$k.fitted * time1)
                                        
                                        #Y coordinate of a point of the line
                                        
                                        force1 = dynamics$fmax.fitted * (1 - exp(-dynamics$k.fitted * time1))
                                        
                                        legendText = c(legendText, paste("RFD", percent, "%Fmax", " = ", round(RFD, digits = 1), " N/s", sep = ""))
                                        legendColor = c(legendColor, "blue")
                                        
                                } else if(RFDoptions$rfdFunction == "RAW")
                                {
                                        time1 = getTimeAtForce(dynamics$time, dynamics$f.raw, dynamics$fmax.raw * percent / 100)
                                        
                                        #Slope of the line
                                        RFD = getForceAtTime(dynamics$time, dynamics$rfd, time1)
                                        
                                        #Y coordinate of a point of the line
                                        force1 = getForceAtTime(dynamics$time, dynamics$f.raw, time1)
                                        
                                        legendText = c(legendText, paste("RFD", percent, "%", "Fmax", " = ", round(RFD, digits = 1), " N/s", sep = ""))
                                        legendColor = c(legendColor, "black")
                                        
                                }
                        } else if(RFDoptions$type == "RFD_MAX")
                        {
                                
                                if (RFDoptions$rfdFunction == "FITTED")
                                {
                                        #max is always in the initial point.
                                        time1 = 0
                                        
                                        #Slope of the line. Deriving the model:
                                        RFD = dynamics$fmax.fitted * dynamics$k.fitted
                                        
                                        #Y coordinate of a point of the line
                                        pointForce1 = 0
                                        
                                        force1 = 0
                                        
                                        legendText = c(legendText, paste("RFDMax", " = ", round(RFD, digits = 1), " N/s", sep = ""))
                                        legendColor = c(legendColor, "blue")
                                        
                                } else if(RFDoptions$rfdFunction == "RAW")
                                {
                                        #Calculing the sample at which the rfd is max. Using only the initial
                                        sample1 = dynamics$startSample + which.max(dynamics$rfd[dynamics$startSample:dynamics$endSample]) -1
                                        
                                        time1 = dynamics$time[sample1]
                                        
                                        #Translating RFDoptions$start to time in seconds
                                        RFDoptions$start = dynamics$time[sample1]
                                        
                                        #Slope of the line
                                        RFD = dynamics$rfd[sample1]
                                        
                                        #Y coordinate of a point of the line
                                        force1 = dynamics$f.raw[sample1]
                                        
                                        legendText = c(legendText, paste("RFDmax = ", round(RFD, digits = 1), " N/s", sep = ""))
                                        legendColor = c(legendColor, "black")
                                        
                                }
                                
                        }
                        
                        #The Y coordinate of the line when it crosses the Y axis
                        intercept = force1 - RFD * time1
                        
                        #The slope of the line seen in the screen(pixels units), NOT in the time-force units
                        windowSlope = RFD*(plotHeight/yHeight)/(plotWidth/xWidth)
                        
                        #Drawing the line
                        text(x = (yHeight - 5 - intercept)/RFD, y = yHeight - 5,
                             label = paste(round(RFD, digits=0), "N/s"),
                             srt=atan(windowSlope)*180/pi, pos = 2, col = color)
                        #Drawing the points where the line touch the function
                        points(x = c(time1, time2), y = c(force1, force2), col = color)
                        abline(a = intercept, b = RFD, lty = 2, col = color)
                }
        }
        
	if(impulseLegend != "")
		legendText = c(legendText, impulseLegend)

	legend(x = xmax, y = dynamics$fmax.fitted/2, legend = legendText, xjust = 1, yjust = 0.1, text.col = legendColor)
}

getDynamicsFromLoadCellFolder <- function(folderName, resultFileName, export2Pdf)
{
        originalFiles = list.files(path=folderName, pattern="*")
        nFiles = length(originalFiles)
        results = matrix(rep(NA, 16*nFiles), ncol = 16)
        colnames(results)=c("fileName", "fmax.fitted", "k.fitted", "fmax.raw", "startTime", "initf", "fmax.smoothed",
                            "rfd0.fitted", "rfd100.raw", "rfd0_100.raw", "rfd0_100.fitted",
                            "rfd200.raw", "rfd0_200.raw", "rfd0_200.fitted",
                            "rfd50pfmax.raw", "rfd50pfmax.fitted")
        
        results[,"fileName"] = originalFiles
        
        for(i in 1:nFiles)
        {
                dynamics = getDynamicsFromLoadCellFile(paste(folderName,originalFiles[i], sep = ""))
                
                results[i, "fileName"] = dynamics$nameOfFile
                results[i, "fmax.fitted"] = dynamics$fmax.fitted
                results[i, "k.fitted"] = dynamics$k.fitted
                results[i, "fmax.raw"] = dynamics$fmax.raw
                results[i, "startTime"] = dynamics$startTime
                results[i, "initf"] = dynamics$initf
                results[i, "fmax.smoothed"] = dynamics$fmax.smoothed
                results[i, "rfd0.fitted"] = dynamics$rfd0.fitted
                results[i, "rfd100.raw"] = dynamics$rfd100.raw
                results[i, "rfd0_100.raw"] = dynamics$rfd0_100.raw
                results[i, "rfd0_100.fitted"] = dynamics$rfd0_100.fitted
                results[i, "rfd200.raw"] = dynamics$rfd200.raw
                results[i, "rfd0_200.raw"] = dynamics$rfd0_200.raw
                results[i, "rfd0_200.fitted"] = dynamics$rfd0_200.fitted
                results[i, "rfd50pfmax.raw"] = dynamics$rfd50pfmax.rawfilter(test$force, rep(1/19, 19), sides = 2)
                results[i, "rfd50pfmax.fitted"] = dynamics$rfd50pfmax.fitted
        }
        write.table(results, file = resultFileName, sep = ";", dec = ",", col.names = NA)
        return(results)
        
}

#Finds the sample in which the force start incresing (RFD > 20% of maxRFD)
#and decrease a given percentage of the maximum force.
#The maximum force is calculed from the moving average of averageLength seconds
getTrimmingSamples <- function(test, rfd, movingAverageForce, averageLength = 0.1, percentChange = 5, testLength = -1, startDetectingMethod = "SD")
{
        movingAverageForce = getMovingAverageForce(test, averageLength = 0.1)
        maxRFD = max(rfd[2:(length(rfd) - 1)])
        
        #Detecting when the force is greater of the mean + 3*SD of 20 samples
        #See Rate of force development: physiological and methodological considerations. Nicola A. Maffiuletti1 et al.
        if (startDetectingMethod == "SD"){
                startSample = 21
                while(test$force[startSample] < mean(test$force[startSample:(startSample - 20)]) + 3*sd(test$force[startSample:(startSample - 20)]))
                {
                        startSample = startSample + 1
                }
                
                while(test$force[startSample] - test$force[startSample -1] >= 0){ #Detecting the sample to decrease
                        startSample = startSample - 1
                }
                
        #Detecting when accurs a greate growth of the force
        } else if (startDetectingMethod == "RFD"){
                startSample = 2
                while(rfd[startSample] <= maxRFD*0.2)
                {
                        startSample = startSample + 1
                }
        }
        
        if (testLength <= -1){
                endSample = startSample + 1
                maxAverageForce = movingAverageForce[endSample]
                while(movingAverageForce[endSample] > maxAverageForce*(100 - percentChange) / 100 &
                      endSample < length(test$time))
                {
                        if(movingAverageForce[endSample] > maxAverageForce)
                        {
                                maxAverageForce = movingAverageForce[endSample]
                        }
                        endSample = endSample + 1
                }
        } else if(testLength >= 0 && testLength < 0.1){
                print("Test interval too short")
        } else {
                endSample = which.min(abs(test$time[startSample] + testLength - test$time))
        }

        return(list(startSample = startSample, endSample = endSample))
}

getRFD <- function(test)
{
        #Instantaneous RFD
        rfd = rep(NA, length(test$time))
        for (n in 2:(length(test$time) - 1))
        {
                rfd[n] = (test$force[n + 1] - test$force[n - 1])/(test$time[n + 1] - test$time[n - 1])
        }
        return(rfd)
}
getMovingAverageForce <- function(test, averageLength = 0.1)
{
        sampleRate = (length(test$time) - 1) / (test$time[length(test$time)] - test$time[1])
        lengthSamples = round(averageLength * sampleRate, digits = 0)
        movingAverageForce = filter(test$force, rep(1/lengthSamples, lengthSamples), sides = 2)
        return(movingAverageForce)
}

#Converts the drawRfdOptions string to a list of parameters
readRFDOptions <- function(optionsStr)
{
        if(optionsStr == "-1")          #Not drawing
        {
                return(list(
                        rfdFunction     = "-1",
                        type            = "-1",
                        start           = -1,
                        end             = -1
                ))
        } else
        {
                options = unlist(strsplit(optionsStr, "\\;"))
                
                return(list(
                        rfdFunction     = options[1],            # raw or fitted
                        type            = options[2],            # instantaeous, average, %fmax, rfdmax
                        #start and end can be in milliseconds (instant and average RFD), percentage (%fmax) or -1 if not needed
                        start           = as.numeric(options[3]),            # instant at which the analysis starts
                        end             = as.numeric(options[4])             # instant at which the analysis ends
                ))
        } 
}

#Converts the line string of Roptions to a list of parameters
readImpulseOptions <- function(optionsStr)
{
        if(optionsStr == "-1")          #Not drawing
        {
                return(list(
                        impulseFunction     = "-1",
                        type            = "-1",
                        start           = -1,
                        end             = -1
                ))
        } else
        {
                options = unlist(strsplit(optionsStr, "\\;"))
                
                return(list(
                        impulseFunction     = options[1],                    # RAW or FITTED
                        type            = options[2],                        # IMP_RANGE or IMP_UNTIL_PERCENT_F_MAX
                        start           = as.numeric(options[3]),            # instant at which the analysis starts in ms
                        end             = as.numeric(options[4])             # instant at which the analysis ends in ms
                ))
        } 
}

#Function to get the interpolated force at a given time in seconds)
getForceAtTime <- function(time, force, desiredTime){
        #find the closest sample
        closestSample = which.min(abs(time - desiredTime))
        
        if(time[closestSample] - desiredTime >= 0){
                previousSample = closestSample - 1
                nextSample = closestSample
        } else if(time[closestSample] - desiredTime < 0){
                previousSample = closestSample
                nextSample = closestSample + 1
        }
        print("Samples:")
        print(paste(previousSample, nextSample))
        print("Times:")
        print(paste(time[previousSample], time[nextSample]))
        print("Forces:")
        print(paste(force[previousSample], force[nextSample]))
        
        #Interpolation between two samples
        desiredForce = force[previousSample] + ((force[nextSample] - force[previousSample]) / (time[nextSample] - time[previousSample]))*(desiredTime - time[previousSample])
        print("DesiredForce:")
        print(desiredForce)
        return(desiredForce)
}

#Function to get the interpolated time at a given force in N
getTimeAtForce <- function(time, force, desiredForce){
        #find the closest sample
        nextSample = 1
        while (force[nextSample] < desiredForce){
                nextSample = nextSample +1
        }
        
        previousSample = nextSample - 1
        
        if(force[nextSample] == desiredForce){
                desiredTime = time[nextSample]
        } else {
                desiredTime = time[previousSample] + (desiredForce  - force[previousSample]) * (time[nextSample] - time[previousSample]) / (force[nextSample] - force[previousSample])
        }
        return(desiredTime)
}


prepareGraph(op$os, pngFile, op$graphWidth, op$graphHeight)
dynamics = getDynamicsFromLoadCellFile(dataFile, op$averageLength, op$percentChange)
drawDynamicsFromLoadCell(dynamics, op$vlineT0, op$vline50fmax.raw, op$vline50fmax.fitted, op$hline50fmax.raw, op$hline50fmax.fitted,
                         op$drawRfdOptions)
#                         op$drawRfdOptions, xlimits = c(0.5, 1.5))
endGraph()

#dynamics = getDynamicsFromLoadCellFile("~/ownCloud/Xavier/Recerca/Yoyo-Tests/Galga/RowData/APl1", averageLength = 0.1, percentChange = 5, sep = ";", dec = ",")
#drawDynamicsFromLoadCell(dynamics, vlineT0=F, vline50fmax.raw=F, vline50fmax.fitted=T, hline50fmax.raw=F, hline50fmax.fitted=T, 
#                         rfd0.fitted=T, rfd100.raw=F, rfd0_100.raw=F, rfd0_100.fitted = F, rfd200.raw=F, rfd0_200.raw=F, rfd0_200.fitted = F,
#                         rfd50pfmax.raw=F, rfd50pfmax.fitted=T)
#getDynamicsFromLoadCellFolder("~/Documentos/RowData/", resultFileName = "~/Documentos/results.csv")
