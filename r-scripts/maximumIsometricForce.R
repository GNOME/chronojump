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
#   Copyright (C) 2017-2020   	Xavier Padullés <x.padulles@gmail.com>
#   Copyright (C) 2017-2021     Xavier de Blas <xaviblas@gmail.com>

#call from Chronojump or:

#Rscript path_to/maximumIsometricForce.R path_tmp

#Read each non commented line of the Roptions file
assignOptions <- function(options)
{
    drawRfdOptions = rep(NA, 10)
    drawRfdOptions[1]      = options[12]
    drawRfdOptions[2]      = options[13]
    drawRfdOptions[3]      = options[14]
    drawRfdOptions[4]      = options[15]
    drawRfdOptions[5]      = options[16]
    drawRfdOptions[6]      = options[17]
    drawRfdOptions[7]      = options[18]
    drawRfdOptions[8]      = options[19]
    drawRfdOptions[9]      = options[20]
    drawRfdOptions[10]      = options[21]
    
    return(list(
        os 			= options[1],
        decimalCharAtFile 	= options[2], 	#unused on multiple
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
        drawImpulseOptions      = options[22],
        testLength 		= as.numeric(options[23]),
        captureOptions 		= options[24],
        title 	 		= options[25],
        exercise 	 	= options[26],
        date	 	 	= options[27],
        time 	 		= options[28],
        scriptsPath 		= options[29],
        triggersOnList  	= as.numeric(unlist(strsplit(options[30], "\\;"))),
        triggersOffList  	= as.numeric(unlist(strsplit(options[31], "\\;"))),
        startSample 	= as.numeric(options[32]),
        endSample 	= as.numeric(options[33]),
        startEndOptimized 	= options[34], 	#bool
	singleOrMultiple 	= options[35],   	#bool (true is single)
	decimalCharAtExport	= options[36],
        maxAvgWindowSeconds 	= as.numeric(options[37]),
	includeImagesOnExport 	= options[38]   	#bool (true is single)
    ))
}


#-------------- get params -------------
args <- commandArgs(TRUE)

tempPath <- args[1]
optionsFile <- paste(tempPath, "/Roptions.txt", sep="")

#-------------- scan options file -------------
options <- scan(optionsFile, comment.char="#", what=character(), sep="\n")

#-------------- assign options -------------
op <- assignOptions(options)
print(op)

source(paste(op$scriptsPath, "/scripts-util.R", sep=""))

#Fits the data to the model f = fmax*(1 - exp(-K*t))
#Important! It fits the data with the axes moved to startTime.
#TODO: Implement the movement of the axes in the function
getForceModel <- function(time, force, startTime, # startTime is the instant when the force start to increase
                          fmaxi,           # fmaxi is the initial value for the fmax. For numeric purpouses
                          previousForce,
                          timeShift = TRUE)              # previousForce is the sustained force before the increase
{
    print("Entered in getForceModel() function")

    time = time - startTime
    
    data = data.frame(time = time, force = force)
    # print(data)
    print(paste("startTime:", startTime, "fmaxi: ", fmaxi, "previousForce:", previousForce))
    if(timeShift)
    {
        model = nls( force ~ fmax*(1-exp(-K*(time - T0))), data, start=list(fmax=fmaxi, K=1, T0 = 0), control=nls.control(warnOnly=TRUE))
        
        T0 = summary(model)$coeff[3,1]
    } else if (!timeShift)
    {
        model = nls( force ~ fmax*(1-exp(-K*(time))), data, start=list(fmax=fmaxi, K=1), control=nls.control(warnOnly=TRUE))
        T0 = 0
    }
    # print(model)
    fmax = summary(model)$coeff[1,1]
    K = summary(model)$coeff[2,1]
    # print(summary(model))
    # print("leaving getForceModel()")
    return(list(fmax = fmax, K = K, T0 = T0, error = 100*residuals(model)/mean(data$force)))
}

getDynamicsFromLoadCellFile <- function(captureOptions, inputFile, decimalChar, averageLength = 0.1, percentChange = 5, testLength = -1, startSample, endSample)
{
    print("Entered getDynamicsFromLoadCellFile")
    
    originalTest = read.csv(inputFile, header = F, dec = decimalChar, sep = ";", skip = 2)
    colnames(originalTest) <- c("time", "force")
    originalTest$time = as.numeric(originalTest$time / 1000000)  # Time is converted from microseconds to seconds
    
    if(captureOptions == "ABS")
        originalTest$force = abs(originalTest$force)
    else if(captureOptions == "INVERTED")
        originalTest$force = -1 * originalTest$force
    
    print(paste("startSample: ", startSample))
    print(paste("endtSample: ", endSample))
    
    #If Roptions.txt does have endSample values greater than 1 it means that the user has selected a range
    if( startSample != endSample  & (endSample > 1) & startSample <= length(originalTest$time) & endSample <= length(originalTest$time))
    {
        print("Range selected by user. Analyzed the specified range")
        
        originalTest = originalTest[(startSample:endSample),]
        originalTest$time = originalTest$time - originalTest$time[1]
        # print("originalTest$time:")
        # print(originalTest$time)
        
        #Atention! The row names of the list are not automaticaly renumbered but the rownames of the objects of the list are changed
        row.names(originalTest) <- 1:nrow(originalTest)
    }
    
    if(op$startEndOptimized == "TRUE")
    {
        
            
            bestFit = getBestFit(originalTest
                                 , averageLength = averageLength
                                 , percentChange = percentChange
                                 , testLength = testLength)
        
        startSample = bestFit$startSample
        startTime = bestFit$startTime
        endSample = bestFit$endSample
        endTime = bestFit$endTime
        model = bestFit$model
        previousForce = originalTest$force[startSample]
  
    } else if(op$startEndOptimized == "FALSE")
    {
        print("startEndOptimized == FALSE")
        # #Extrapolating the test to cross the horizontal axe.
        # originalTest = extrapolateToZero(originalTest$time, originalTest$force)
        # names(originalTest) <- c("time", "force")
        originalTest$time = originalTest$time - originalTest$time[1]
        # print(originalTest)
        
        startSample = 1
        startTime = originalTest$time[2]
        endSample = length(originalTest$time)
        endTime = originalTest$time[length(originalTest$time)]
        model = getForceModel(originalTest$time, originalTest$force, 0, max(originalTest$force), originalTest$force[2], timeShift = FALSE)
        previousForce = originalTest$force[2]
    }
    
    
    
    #Instantaneous RFD
    rfd = getRFD(originalTest)
    
    fmax.raw = max(originalTest$force[startSample:endSample])
    
    f.smoothed = getMovingAverageForce(originalTest, averageLength = averageLength) #Running average with equal weight averageLength seconds
    fmax.smoothed = max(f.smoothed, na.rm = TRUE)

    # print("####### model ######")
    # print(model)
    f.fitted =model$fmax*(1-exp(-model$K*(originalTest$time - startTime)))
    
    f0.raw = originalTest$force[startSample]                              # Force at startTime. ATENTION. This value is different than previousForce
    rfd0.fitted = model$fmax * model$K                                  # RFD at t=0ms using the exponential model
    # tfmax.raw = originalTest$time[which.min(abs(originalTest$force - fmax.raw))] - startTime            # Time needed to reach the Fmax
    tfmax.raw = originalTest$time[which.min(abs(originalTest$force - fmax.raw))]            # Time needed to reach the Fmax

    return(list(nameOfFile = inputFile, time = originalTest$time,
                fmax.fitted = model$fmax, k.fitted = model$K, tau.fitted = 1/model$K,
                startTime = startTime, endTime = endTime,
                startSample = startSample, endSample = endSample,
                totalSample = length(originalTest$time),
                previousForce = previousForce,
                f0.raw = f0.raw,
                fmax.raw = fmax.raw, fmax.smoothed = fmax.smoothed,
                tfmax.raw = tfmax.raw,
                rfd = rfd,
                f.raw = originalTest$force, f.smoothed = f.smoothed, f.fitted = f.fitted,
                endTime = endTime,
                meanError = mean(abs(model$error[!is.nan(model$error)]))
    ))
}

drawDynamicsFromLoadCell <- function(title, exercise, datetime,
    dynamics, captureOptions, vlineT0=T, vline50fmax.raw=F, vline50fmax.fitted=F,
    hline50fmax.raw=F, hline50fmax.fitted=F,
    rfdDrawingOptions, triggersOn = "", triggersOff = "", xlimits = NA, forceLines = TRUE, timeLines = TRUE)
{
    print("Dynamics in Draw:")
    # print(dynamics$time)

    titleFull = paste(title, exercise, sep=" - ")
    dynamics$time = dynamics$time - dynamics$startTime
    dynamics$tfmax.raw = dynamics$tfmax.raw - dynamics$startTime
    dynamics$endTime = dynamics$endTime - dynamics$startTime
    dynamics$startTime = 0
    if(is.na(dynamics$time[1]))
    {
        print("Dynamics not available:")
        print(dynamics)
        return(NA)
    }
    par(mar = c(6, 4, 6, 4))

    exportValues = NULL

    #Detecting if the duration of the sustained force is enough
    # print("f.raw")
    # print(dynamics$f.raw)
    meanForce = mean(dynamics$f.raw[dynamics$startSample:dynamics$endSample])
    print(paste("meanForce: ", meanForce, "fmax.raw: ", dynamics$fmax.raw))
    #TODO: Is this necessary?. Is this value acceptable?
    if( meanForce < dynamics$fmax.raw*0.75 ){
        sustainedForce = F
        yHeight = dynamics$fmax.raw
    } else if( meanForce >= dynamics$fmax.raw*0.75 ){
        sustainedForce = T
        yHeight = max(dynamics$fmax.raw, dynamics$fmax.fitted) * 1.1
    }
    
    par(mar=c(4,4,3,1))
    #Plotting raw data from startTime to endTime (Only the analysed data)
    if (!is.na(xlimits[1])){
        xWidth = xlimits[2] - xlimits[1]
        plot(dynamics$time[dynamics$startSample:dynamics$endSample], dynamics$f.raw[dynamics$startSample:dynamics$endSample],
             type="l", xlab="Time[s]", ylab="Force[N]",
             xlim = xlimits, ylim=c(0, yHeight),
             #main = dynamics$nameOfFile,
             main = paste(parse(text = paste0("'", titleFull, "'"))), #process unicode, needed paste because its an expression. See graph.R
             yaxs= "i", xaxs = "i")
        mtext(datetime, line = 0)
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
             main = paste(parse(text = paste0("'", titleFull, "'"))), #process unicode, needed paste because its an expression. See graph.R
             yaxs= "i", xaxs = "i")
        mtext(datetime, line = 0)
    }
    
    
    # if(!sustainedForce){
    #     text(x = (xmax + xmin)/2, y = yHeight/2, labels = "Bad execution. Probably not sustained force", adj = c(0.5, 0.5), cex = 2, col = "red")
    #     #Plotting not analysed data
    #     lines(dynamics$time[1:dynamics$startSample] , dynamics$f.raw[1:dynamics$startSample], col = "grey") #Pre-analysis
    #     lines(dynamics$time[dynamics$endSample: dynamics$totalSample] , dynamics$f.raw[dynamics$endSample: dynamics$totalSample], col = "grey") #Post-analysis
    #     return()
    # }
    
    #Plotting Impulse
    
    print("--------Impulse-----------")
    print(op$drawImpulseOptions)
    impulseOptions = readImpulseOptions(op$drawImpulseOptions)
    
    impulseLegend = ""
    impulse = NULL
    
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
            impulseColor = "black"
            
            #Drawing the area under the force curve (Impulse)
            polygon(c(dynamics$time[startImpulseSample:endImpulseSample], dynamics$time[endImpulseSample] , dynamics$time[startImpulseSample]),
                    c(dynamics$f.raw[startImpulseSample:endImpulseSample], 0, 0), col = "grey")
            
            #Calculation of the impulse
            impulse = getAreaUnderCurve(dynamics$time[startImpulseSample:endImpulseSample], dynamics$f.raw[startImpulseSample:endImpulseSample])
            
        } else if(impulseOptions$impulseFunction == "FITTED")
        {
            impulseColor = "blue"
            
            #Drawing the area under the force curve (Impulse)
            polygon(c(dynamics$time[startImpulseSample:endImpulseSample], dynamics$time[endImpulseSample] , dynamics$time[startImpulseSample]),
                    c(dynamics$f.fitted[startImpulseSample:endImpulseSample], 0, 0), col = "grey")
            #Redraw the raw force
            lines(x = dynamics$time[startImpulseSample:endImpulseSample] , y = dynamics$f.raw[startImpulseSample:endImpulseSample])
            
            #Calculation of the impulse
            #Sum of the area of all the triangles formed with a vertex in the origin and the other vertex in the
            #n-th and (n+1)-th point of the polygon
            # impulse = 0
            # for(n in startImpulseSample:(endImpulseSample - 1))
            # {
            #         #Area of the paralelograms, not the triangle
            #         area = ((dynamics$time[n+1] - dynamics$time[dynamics$startSample]) * dynamics$f.fitted[n] - (dynamics$time[n] - dynamics$time[dynamics$startSample]) * dynamics$f.fitted[n+1])
            #         impulse = impulse + area
            # }
            # 
            # area = (dynamics$time[endImpulseSample] - dynamics$time[dynamics$startSample]) * dynamics$f.fitted[endImpulseSample]
            # impulse = impulse + area
            #Area under the curve is one half of the sum of the area of paralelograms
            #impulse = impulse / 2
            impulse = getAreaUnderCurve(dynamics$time[startImpulseSample:endImpulseSample], dynamics$f.fitted[startImpulseSample:endImpulseSample])
            
            
        }
        
        text(x = (dynamics$startTime + (dynamics$time[endImpulseSample] - dynamics$time[startImpulseSample])*0.66), y = mean(dynamics$f.raw[startImpulseSample:endImpulseSample])*0.66,
             labels = paste("Impulse =", round(impulse, digits = 2), "N\u00B7s"), pos = 4, cex = 1.5)
        
        impulseLegend = paste("Impulse", impulseOptions$start, "-", impulseOptions$end, " = ", round(impulse, digits = 2), " N\u00B7s", sep = "") #\u00B7 is ·
    }
    
    #Plotting not analysed data
    lines(dynamics$time[1:dynamics$startSample] , dynamics$f.raw[1:dynamics$startSample], col = "grey") #Pre-analysis
    lines(dynamics$time[dynamics$endSample: dynamics$totalSample] , dynamics$f.raw[dynamics$endSample: dynamics$totalSample], col = "grey") #Post-analysis
    
    
    #Plotting fitted data
    lines(dynamics$time, dynamics$f.fitted, col="blue")
    abline(h = dynamics$fmax.fitted, lty = 2, col = "blue")
    text(x = xmax, y = dynamics$fmax.fitted,
         labels = paste("Fmax =", round(dynamics$fmax.fitted,digits = 2), "N"),
         col = "blue", cex = 1.5, adj=c(1,0))
    
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
         labels = paste(round(dynamics$tau.fitted, digits = 2), "s"), pos = 3, cex = 1.5, col = "blue")
    # text(x = (dynamics$tau.fitted / 2), y = -20,
    #      labels = paste(round(dynamics$tau.fitted, digits = 2), "s"), pos = 3, cex = 1.5, xpd = TRUE)
    
    arrows(x0 = 0, y0 = 0,
           x1 = 0, y1 = dynamics$fmax.fitted*0.6321206)
    
    text(x = 0, y = dynamics$fmax.fitted*0.6321206 / 2,
         labels = "63% of fmax", pos = 2, cex = 1.5, srt = 90, col = "blue")
    
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
    
    #triggers
    abline(v=triggersOn, col="green")
    abline(v=triggersOff, col="red")
    
    
    legendText = c(
        paste("Fmax =", round(dynamics$fmax.fitted, digits = 2), "N"),
        paste("K = ", round(dynamics$k.fitted, digits = 2),"s\u207B\u00B9"),
        paste("\u03C4 = ", round(dynamics$tau.fitted, digits = 2),"s")
    )
    legendColor = c("blue", "blue", "blue")
 
    exportValues = c(dynamics$fmax.fitted)

    #The coordinates where the lines and dots are plotted are calculated with the sampled data in raw and fitted data.
    #The slopes are calculated in that points
    for (n in 1:length(rfdDrawingOptions))
    {
        RFDoptions = readRFDOptions(op$drawRfdOptions[n])
        
        print(paste("---- RFD number", n, "--------"))
        print(options)
        if(RFDoptions$type == "AVERAGE" & (RFDoptions$start == RFDoptions$end)){
            RFDoptions$type = "INSTANTANEOUS"
        }

	RFDNum = paste ("RFD ", n)
	RFDNumColon = paste ("RFD ", n, ": ")

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
                    
                    legendText = c(legendText, paste(RFDNumColon, RFDoptions$start*1000, " = ", round(RFD, digits = 1), " N/s", sep = ""))
                    legendColor = c(legendColor, "blue")
                    
                } else if(RFDoptions$rfdFunction == "RAW")
                {
                    #Slope of the line of the sampled point.
                    RFD = interpolateXAtY(dynamics$rfd, dynamics$time, time1)
                    
                    #Y coordinate of a point of the line
                    force1 = interpolateXAtY(dynamics$f.raw, dynamics$time, RFDoptions$start)
                    
                    legendText = c(legendText, paste(RFDNumColon, RFDoptions$start*1000, " = ", round(RFD, digits = 1), " N/s", sep = ""))
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
                    
                    legendText = c(legendText, paste(RFDNumColon, RFDoptions$start*1000, "-", RFDoptions$end*1000, " = ", round(RFD, digits = 1), " N/s", sep = ""))
                    legendColor = c(legendColor, "blue")
                    
                } else if(RFDoptions$rfdFunction == "RAW")
                {
                    force1 = interpolateXAtY(dynamics$f.raw, dynamics$time, RFDoptions$start)
                    force2 = interpolateXAtY(dynamics$f.raw, dynamics$time, RFDoptions$end)
                    
                    #Slope of the line
                    RFD = (force2 - force1) / (time2 - time1)
                    
                    legendText = c(legendText, paste(RFDNumColon, RFDoptions$start*1000, "-", RFDoptions$end*1000, " = ", round(RFD, digits = 1), " N/s", sep = ""))
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
                    
                    legendText = c(legendText, paste(RFDNumColon, percent, "%Fmax", " = ", round(RFD, digits = 1), " N/s", sep = ""))
                    legendColor = c(legendColor, "blue")
                    valuesCol = "blue"
                    
                } else if(RFDoptions$rfdFunction == "RAW")
                {
                    time1 = interpolateXAtY(dynamics$time, dynamics$f.raw, dynamics$fmax.raw * percent / 100)
                    
                    #Slope of the line
                    RFD = interpolateXAtY(dynamics$rfd, dynamics$time, time1)
                    
                    #Y coordinate of a point of the line
                    force1 = interpolateXAtY(dynamics$f.raw, dynamics$time, time1)
                    
                    legendText = c(legendText, paste(RFDNumColon, percent, "%", "Fmax", " = ", round(RFD, digits = 1), " N/s", sep = ""))
                    legendColor = c(legendColor, "black")
                    valuesCol = "black"
                    
                }
                
                if(timeLines){
                  abline(v=time1, lty = 3, col ="brown1")
                  text(x = time1, y = 0, labels = paste(round(time1, digits = 3),"s"), pos = 3, col = valuesCol)
                }
                if(forceLines){
                  abline(h=force1, lty = 3, col ="brown1")
                  text(x = xmin, y = force1, labels = paste(round(force1, digits = 2),"N"), pos = 4, col = valuesCol)
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
                    
                    legendText = c(legendText, paste(RFDNumColon, "max = ", round(RFD, digits = 1), " N/s", sep = ""))
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
                    
                    legendText = c(legendText, paste(RFDNumColon, "max = ", round(RFD, digits = 1), " N/s", sep = ""))
                    legendColor = c(legendColor, "black")
                    
                }
                
            } else if(RFDoptions$type == "BEST_AVG_RFD_IN_X_MS")
            {
                window = RFDoptions$start / 1000
                print("detected BEST_AVG_RFD_IN_X_MS")
                if (RFDoptions$rfdFunction == "FITTED")
                {
                    print("FITTED")
                    #In the model the max RFD is always the RFD0
                    time1 = dynamics$time[dynamics$startSample]
                    # print(paste("In drawDynamics time1:", time1))
                    time2 = time1 + window
                    force1 = 0
                    force2 = dynamics$fmax.fitted*(1 - exp(-dynamics$k.fitted*window))
                    RFD = force2 / window
                    legendText = c(legendText, paste(RFDNumColon, "max avg in ", RFDoptions$start, "ms = ", round(RFD, digits = 1), " N/s", sep = ""))
                    legendColor = c(legendColor, "blue")
                } else if(RFDoptions$rfdFunction == "RAW")
                {
                    maxAvgRFD = getAvgRfdXSeconds(dynamics, window )
                    force1 = maxAvgRFD$force1
                    
                    time1 = maxAvgRFD$time1
                    time2 = maxAvgRFD$time2
                    force1 = maxAvgRFD$force1
                    force2 = maxAvgRFD$force2
                    RFD = maxAvgRFD$RFD
                    legendColor = c(legendColor, "black")
                    legendText = c(legendText, paste(RFDNumColon, "max avg in  ", RFDoptions$start, "ms = ", round(RFD, digits = 1), " N/s", sep = ""))
                }
	        }

            #The Y coordinate of the line when it crosses the Y axis
            intercept = force1 - RFD * time1
            print(paste("Intercetp:", intercept))
            
            #The slope of the line seen in the screen(pixels units), NOT in the time-force units
            windowSlope = RFD*(plotHeight/yHeight)/(plotWidth/xWidth)
            
            #Drawing the line
            text(x = (yHeight - 5 - intercept)/RFD, y = yHeight - 5,
                 label = RFDNum,
                 srt=atan(windowSlope)*180/pi, pos = 2, col = color)
            #Drawing the points where the line touch the function
            points(x = c(time1, time2), y = c(force1, force2), col = color)
            abline(a = intercept, b = RFD, lty = 2, col = color)

	    if(is.null(RFD))
		    exportValues = c(exportValues, NA)
	    else
		    exportValues = c(exportValues, RFD)
	}

    }

    exportValues = c(exportValues, impulse)

    #adding also model error
    exportValues = c(exportValues, dynamics$meanError)

    if(impulseLegend != ""){
        legendText = c(legendText, impulseLegend)
        legendColor = c(legendColor, impulseColor)
    }
    
    legendText = c(legendText, paste("MeanError = ", round(dynamics$meanError, digits = 2), "%", sep =""))
    if (dynamics$meanError >= 5){
        legendColor = c(legendColor, "red")
        text(x = xmax, y = dynamics$fmax.fitted*0.01, labels = "The mean error is larger than 5%. Possible bad execution", col = "red", pos = 2)
    } else {
        legendColor = c(legendColor, "grey40")
    }

    #experimental avg error on RFD calculation from 0 to 50 ms
    #window = RFDoptions$start / 1000
    #force2 = dynamics$fmax.fitted*(1 - exp(-dynamics$k.fitted*window))
    #modelRFD=force2/0.05
    #rawRFD = getAvgRfdXSeconds(dynamics, 0.05 )$RFD
    #legendText = c(legendText, paste("RFD0-50 error = ", round((rawRFD - modelRFD)*100/rawRFD, 2), "%"))
    #legendColor = c(legendColor, "grey20")

    legend(x = xmax, y = dynamics$fmax.fitted/2, legend = legendText, xjust = 1, yjust = 0.1, text.col = legendColor)

    if(op$singleOrMultiple == "FALSE")
    {
	return(exportValues)
    }
}

getAvgRfdXSeconds <-function(dynamics, window)
{
    print("In getAvgRfdxSeconds")
    RFD = 0
    
    #if window does not fit in graph, discard it
    if (dynamics$time[dynamics$startSample] + window > dynamics$time[dynamics$endSample])
        next
    #Measure how much samples corresponds to the window
    time = dynamics$time[dynamics$startSample:dynamics$endSample]
    windowSamples = which.min(time - (dynamics$time[dynamics$startSample] + window))
    for (i in dynamics$startSample:(dynamics$endSample - windowSamples))
    {
        forceTemp1 = dynamics$f.raw[i]
        forceTemp2 = interpolateXAtY(dynamics$f.raw, dynamics$time, dynamics$time[i] + window)
        RFDtemp = (forceTemp2 - forceTemp1) / window
        
        if (RFDtemp > RFD)
        {
            force1 = forceTemp1
            force2 = forceTemp2
            RFD = RFDtemp
            time1 = dynamics$time[i]
            time2 = interpolateXAtY(dynamics$time, dynamics$time, dynamics$time[i] + window)
        }
    }
    return(list(RFD = RFD, time1 = time1, force1 = force1, time2 = time2, force2=force2))
}

#getDynamicsFromLoadCellFolder <- function(folderName, resultFileName, captureOptions)
#{
#    originalFiles = list.files(path=folderName, pattern="*")
#    nFiles = length(originalFiles)
#    results = matrix(rep(NA, 16*nFiles), ncol = 16)
#    colnames(results)=c("fileName", "fmax.fitted", "k.fitted", "fmax.raw", "startTime", "previousForce", "fmax.smoothed",
#                        "rfd0.fitted", "rfd100.raw", "rfd0_100.raw", "rfd0_100.fitted",
#                        "rfd200.raw", "rfd0_200.raw", "rfd0_200.fitted",
#                        "rfd50pfmax.raw", "rfd50pfmax.fitted")

#    results[,"fileName"] = originalFiles
#    
#    for(i in 1:nFiles)
#    {
#        dynamics = getDynamicsFromLoadCellFile(captureOptions, paste(folderName,originalFiles[i], sep = ""))
#        
#        results[i, "fileName"] = dynamics$nameOfFile
#        results[i, "fmax.fitted"] = dynamics$fmax.fitted
#        results[i, "k.fitted"] = dynamics$k.fitted
#        results[i, "fmax.raw"] = dynamics$fmax.raw
#        results[i, "startTime"] = dynamics$startTime
#        results[i, "previousForce"] = dynamics$previousForce
#        results[i, "fmax.smoothed"] = dynamics$fmax.smoothed
#        results[i, "rfd0.fitted"] = dynamics$rfd0.fitted
#        results[i, "rfd100.raw"] = dynamics$rfd100.raw
#        results[i, "rfd0_100.raw"] = dynamics$rfd0_100.raw
#        results[i, "rfd0_100.fitted"] = dynamics$rfd0_100.fitted
#        results[i, "rfd200.raw"] = dynamics$rfd200.raw
#        results[i, "rfd0_200.raw"] = dynamics$rfd0_200.raw
#        results[i, "rfd0_200.fitted"] = dynamics$rfd0_200.fitted
#        results[i, "rfd50pfmax.raw"] = dynamics$rfd50pfmax.rawfilter(test$force, rep(1/19, 19), sides = 2)
#        results[i, "rfd50pfmax.fitted"] = dynamics$rfd50pfmax.fitted
#    }
#    write.table(results, file = resultFileName, sep = ";", dec = ",", col.names = NA)
#    return(results)
#    
#}

#Finds the sample in which the force start incresing with two optional methods
# - SD method: When the force increase 3 times the standard deviation
# - RFD method: When the RFD is at least 20% of the maximum RFD
#

# #### DEPRECATED ########
# #This function also finds the sample at which there is a decrease of a given percentage of the maximum force.
# #The maximum force is calculed from the moving average of averageLength seconds
# getAnalysisRange <- function(test, rfd, movingAverageForce, averageLength = 0.1, percentChange = 5, testLength = -1, startDetectingMethod = "SD")
# {
#     print("Entered in getAnalysisRange")
#     print("test:")
#     print(test)
#     movingAverageForce = getMovingAverageForce(test, averageLength = 0.1)
#     maxRFD = max(rfd[2:(length(rfd) - 1)])
#     maxRFDSample = which.max(rfd[2:(length(rfd) - 1)])
#     print(maxRFDSample)
#     
#     #Detecting when the force is greater than (mean of 20 samples) + 3*SD
#     #If in various sample the force are greater, the last one before the maxRFD are taken
#     #See Rate of force development: physiological and methodological considerations. Nicola A. Maffiuletti1 et al.
#     
#     startSample = NULL
#     if (startDetectingMethod == "SD"){
#         
#         for(currentSample in 21:maxRFDSample)
#         {
#             print(paste(currentSample, test$time[currentSample], test$force[currentSample]))
#             
#             if(test$force[currentSample] < mean(test$force[currentSample:(currentSample - 20)]) + 3*sd(test$force[currentSample:(currentSample - 20)]))
#                 startSample = currentSample
#         }
#         
#         while(test$force[startSample] - test$force[startSample -1] >= 0){ #Going back to the last sample that decreased
#             startSample = startSample - 1
#         }
#         #Detecting when accurs a great growth of the force (great RFD)
#     } else if (startDetectingMethod == "RFD"){
#         for(currentSample in 2:maxRFDSample)
#         {
#             if(rfd[currentSample] <= maxRFD*0.2)
#                 startSample = currentSample
#         }
#     }
#     #Using the decrease of the force to detect endingSample
#     if (testLength <= -1){
#         endSample = startSample + 1
#         maxMovingAverageForce = movingAverageForce[endSample]
#         while(movingAverageForce[endSample] >= maxMovingAverageForce*(100 - percentChange) / 100 &
#               endSample < length(test$time))
#         {
#             if(movingAverageForce[endSample] > maxMovingAverageForce)
#             {
#                 print("New max")
#                 maxMovingAverageForce = movingAverageForce[endSample]
#             }
#             endSample = endSample + 1
#             print(paste("Current endSample: ", endSample))
#             print(paste("Current movingAverageForce: ", movingAverageForce[endSample]))
#         }
#     } else if(testLength >= 0 && testLength < 0.1){
#         print("Test interval too short")
#         
#         #Using the fixed time to detect endSample
#     } else {
#         endSample = which.min(abs(test$time[startSample] + testLength - test$time))
#     }
#     print(paste("startSample:", startSample, "endSample:", endSample))
#     
#     return(list(startSample = startSample, endSample = endSample))
# }

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
    print("Entered getMovingAverageForce()")
    sampleRate = (length(test$time) - 1) / (test$time[length(test$time)] - test$time[1])
    lengthSamples = round(averageLength * sampleRate, digits = 0)
    print(paste("lengthSamples: ", lengthSamples))
    movingAverageForce = filter(test$force, rep(1/lengthSamples, lengthSamples), sides = 2)
    
    # print("movingAverageForce:")
    # print(movingAverageForce)
    # 
    # print("movingAverageForce[1:(lengthSamples %/% 2)] :")
    # print(movingAverageForce[1:(lengthSamples %/% 2)])
    
    #filling the NAs with the closest value
    movingAverageForce[1:(lengthSamples %/% 2)] = movingAverageForce[(lengthSamples %/% 2) +1]
    
    # print("movingAverageForce[1:(lengthSamples %/% 2)]")
    # print(movingAverageForce[1:(lengthSamples %/% 2)])
    
    
    # print(" movingAverageForce[(lengthSamples %/% 2) +1] :")
    # print( movingAverageForce[(lengthSamples %/% 2) +1])
    movingAverageForce[(length(movingAverageForce) - ceiling(lengthSamples / 2)): length(movingAverageForce)] = movingAverageForce[(length(movingAverageForce) - ceiling(lengthSamples / 2) +1)]
    # print(" movingAverageForce[(lengthSamples %/% 2) +1] :")
    # print( movingAverageForce[(lengthSamples %/% 2) +1])

    print("reconstructed movingAverageForce:")
    print(movingAverageForce)
    
}

#estrapolate a function to extend the line joining the two first samples until it cross the horizontal axe
extrapolateToZero <- function(x, y)
{
    # #t0 is the x at which an extrapolation of the two first samples would cross the horizontal axis
    # t0 = y[1] * (x[2] - x[1]) / (y[2] - y[1])
    # print(paste("t0: ", t0))
    # 
    # 
    # #Adding the (0,0)
    # x = c(x[1] - t0, x)
    # y = c(0, y)
    
    return(list(x = x, y = y))
}

# bestFit() changes startSample in order to find which minimizes the mean error of the model.
# bestFit() gives better resuls for model variables. The counterpart is that there's no scientific papers using it and some control of the procerss is lost
#as it is more difficult to predict the startSample by humans.
getBestFit <- function(originalTest
                       , averageLength = 0.1, percentChange = 5, testLength = -1)
{
    print("Entered in bestFit")
    # print("originalTest:")
    # print(originalTest)
    rfd = getRFD(originalTest)
    maxRFDSample = which.max(rfd)
    print(paste("maxRFDSample:", maxRFDSample))
    
    maxForce = max(originalTest$force)
    print(paste("maxForce: ", maxForce))
    
    movingAverageForce = getMovingAverageForce(originalTest, averageLength)
    
    # print("movingAverageForce:")
    # print(movingAverageForce)
    
    #Going back from maxRFD sample until the force increase
    startSample = maxRFDSample -1
    
    # print(paste("originalTest$force[startSample]", originalTest$force[startSample]))
    # print(paste("originalTest$force[startSample +1]", originalTest$force[startSample +1]))
    
    while(originalTest$force[startSample] < originalTest$force[startSample + 1])
    {
        # print(startSample)
        startSample = startSample - 1
        if(startSample < 1)
            break()
    }
    
    startSample = startSample + 1
    
    #Calculing the end sample of the analysis
    if(testLength > 0)      #The user selected the fixed length of the test
    {
        print("Detection of endSample by test length")
        endSample = which.min(abs(originalTest$time - (originalTest$time[startSample] + testLength)))
        
        maxMovingAverageForce = max(movingAverageForce[startSample:endSample])
        
    } else #The user selected to detect a decrease in the force
    {
        print("Detection of endSample by decrease in the force")
        print(paste("percentChange: ", percentChange))
        
        endSample = maxRFDSample
        
        # print(paste("startSample: ", startSample))
        # print(paste("initial endSample: ", endSample))

        # print("movingAverageForce[startSample:endSample]:")
        # print(movingAverageForce[startSample:endSample])
        
        maxMovingAverageForce = max(movingAverageForce[startSample:endSample])
        
        # print(paste("MaxMovingAverageForce: ", maxMovingAverageForce, "Current Limit: ", maxMovingAverageForce*(100 - percentChange) / 100))
        # print(paste("Current movingAverageForce: ", movingAverageForce[endSample], "rawForce: ", originalTest$force[endSample]))
        
        while(movingAverageForce[endSample] >= maxMovingAverageForce*(100 - percentChange) / 100 &
              endSample < length(originalTest$time))
        {
            if(movingAverageForce[endSample] > maxMovingAverageForce)
            {
                maxMovingAverageForce = movingAverageForce[endSample]
                # print(paste("New max:", maxMovingAverageForce))
            }
            endSample = endSample + 1
            
            # print(paste("Current endSample: ", endSample))
            # print(paste("Current movingAverageForce: ", movingAverageForce[endSample]))
        }
    }
    
    #Moving all the sample to make the fisrt sample of the trimmed test the (t0, f0)
    trimmedTest = originalTest[startSample:endSample,]
    
    # print(paste("endSample in getBestFit: ", endSample, "   endForce: ", originalTest$force[endSample]))
    
    #Extrapolating the test to cross the horizontal axe.
    trimmedTest = extrapolateToZero(trimmedTest$time, trimmedTest$force)
    names(trimmedTest) <- c("time", "force")
    trimmedTest$time = trimmedTest$time - trimmedTest$time[1]
    
    # print(paste("startTime:", trimmedTest$time[1], "fmaxi:", maxForce, "previousForce: ", originalTest$force[1]))
    
    #In each iteration the error of the current model is compared with the last error of the last model
    forceModel <- getForceModel(time = trimmedTest$time
                                , force = trimmedTest$force
                                , startTime = trimmedTest$time[1]
                                , fmaxi = maxForce
                                , previousForce = originalTest$force[1]
                                , timeShift = TRUE)
    currentMeanError = mean(abs(forceModel$error[!is.nan(forceModel$error)]))
    
    lastMeanError = 1E6
    
    # print(paste("currentMeanError: ", currentMeanError, "lastMeanError: ", lastMeanError))
    # print(paste(startSample, ":", endSample, sep = ""))
    # print("Entering the while")
    
    while(currentMeanError <= lastMeanError & startSample <= maxRFDSample & endSample < length(originalTest$time))
    {
        startSample = startSample +1
        if (testLength > 0)
        {
          endSample = endSample +1
        }
        
        lastMeanError = currentMeanError
        
        trimmedTest = originalTest[startSample:endSample,]
        
        #Extrapolating the test
        trimmedTest = extrapolateToZero(trimmedTest$time, trimmedTest$force)
        names(trimmedTest) <- c("time", "force")
        trimmedTest$time = trimmedTest$time - trimmedTest$time[1]
        
        # print("In getBestFit during the while")
        # print(paste("startTime: ", trimmedTest$time[1], "fmaxi: ", maxForce, "previousForce: ", originalTest$force[1]))
        forceModel <- getForceModel(time = trimmedTest$time
                                    , force = trimmedTest$force
                                    , startTime = trimmedTest$time[1]
                                    , fmaxi = maxForce
                                    , previousForce = originalTest$force[1]
                                    , timeShift = TRUE)
        currentMeanError = mean(abs(forceModel$error[!is.nan(forceModel$error)]))
        # print("----------")
        # print(paste("currentMeanError: ", currentMeanError, "lastMeanError: ", lastMeanError))
        # print(paste(startSample, ":", endSample, sep = ""))
    }
    
    startSample = startSample -1
    if (testLength < 0)
    {
      endSample = endSample -1
    }
    
    lastMeanError = currentMeanError
    
    trimmedTest = originalTest[startSample:endSample,]
    
    #Extrapolating the test.
    trimmedTest = extrapolateToZero(trimmedTest$time, trimmedTest$force)
    names(trimmedTest) <- c("time", "force")
    
    #The first sample is at t=0
    trimmedTest$time = trimmedTest$time - trimmedTest$time[1]
    
    #Saving the absolute time of startSample and endSample before all the times are adjusted to the onset
    startTime = originalTest$time[startSample]
    endTime = originalTest$time[endSample]
    #Moving the original test to match the times in trimmedTest
    # originalTest$time = originalTest$time - originalTest$time[startSample] + trimmedTest$time[2]
    originalTest$time = originalTest$time - originalTest$time[startSample]
    print("In getBestFit() after the while")
    print(paste("startTime: ", trimmedTest$time[1], "fmaxi: ", maxForce, "previousForce: ", trimmedTest$force[1]))
    forceModel <- getForceModel(time = trimmedTest$time
                                , force = trimmedTest$force
                                , startTime = trimmedTest$time[1]
                                , fmaxi = maxForce
                                , previousForce = originalTest$force[1]
                                , timeShift = TRUE)
    
    # print("forceModel:")
    # print(forceModel)
    
    currentMeanError = mean(abs(forceModel$error[!is.nan(forceModel$error)]))
    
    #Correcting the startSample to the closest to the beginning of the model
    startSampleCorrected = which.min(abs(originalTest$time - forceModel$T0))
    endSample = endSample - (startSample - startSampleCorrected)
    startSample = startSampleCorrected
    
    # startTime = originalTest$time[startSample]
    # endTime = originalTest$time[endSample]
    
    print(paste("currentMeanError: ", currentMeanError, "lastMeanError: ", lastMeanError))
    print(paste("samples: ", startSample, ":", endSample, sep = ""))
    print(paste("time without T0:", startTime, "T0:", forceModel$T0, "StartSampleCorrecte:", startSampleCorrected))
    print(paste("time: ", startTime + forceModel$T0, ":", endTime + forceModel$T0))
    
    return(list(model = forceModel
                , startSample = startSampleCorrected, startTime = startTime + forceModel$T0
                , endSample = endSample, endTime = endTime + forceModel$T0
    ))
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
            type            = options[2],            # instantaneous, average, %fmax, rfdmax, BEST_AVG_RFD_IN_X_MS
            #start and end can be in milliseconds (instant and average RFD), percentage (%fmax) or -1 if not needed
            start           = as.numeric(options[3]),            # instant at which the analysis starts (or time window in BEST_AVG_RFD)
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

doProcess <- function(pngFile, dataFile, decimalChar, title, exercise, datetime, captureOptions, startSample, endSample)
{
	title = fixTitleAndOtherStrings(title)
	exercise = fixTitleAndOtherStrings(exercise)

	print("Going to enter prepareGraph")
	prepareGraph(op$os, pngFile, op$graphWidth, op$graphHeight)

	print("Going to enter getDynamicsFromLoadCellFille")
	dynamics = getDynamicsFromLoadCellFile(captureOptions, dataFile, decimalChar,
			op$averageLength, op$percentChange, testLength = op$testLength, startSample, endSample)

	print("Going to draw")
	exportedValues = drawDynamicsFromLoadCell(title, exercise, datetime, dynamics, captureOptions,
			op$vlineT0, op$vline50fmax.raw, op$vline50fmax.fitted, op$hline50fmax.raw, op$hline50fmax.fitted,
			op$drawRfdOptions, triggersOn = op$triggersOnList, triggersOff = op$triggersOffList)
#                       op$drawRfdOptions, xlimits = c(0.5, 1.5))
	endGraph()

	return(exportedValues)
}

plotABGraph <- function(pngFile, dataFile, decimalChar, title, exercise, datetime, captureOptions, startSample, endSample,
			maxAvgForceInWindow, maxAvgForceInWindowSampleStart, maxAvgForceInWindowSampleEnd)
{
	title = fixTitleAndOtherStrings(title)
	exercise = fixTitleAndOtherStrings(exercise)

	print("Going to enter prepareGraph")
	prepareGraph(op$os, pngFile, op$graphWidth, op$graphHeight)

	originalTest = read.csv(dataFile, header = F, dec = decimalChar, sep = ";", skip = 2)
	colnames(originalTest) <- c("time", "force")
	x = as.numeric(originalTest$time / 1000000)  # Time is converted from microseconds to seconds
	y = originalTest$force

	if(captureOptions == "ABS")
		y = abs(y)
	else if(captureOptions == "INVERTED")
		y = -1 * y

	plot(y ~ x, type="l", xlab="Time (s)", ylab="Force (N)")

	#ablines to know startSample endSample
	abline(v=c(x[startSample], x[endSample]), lty=2, col="gray20")

	#mark max point
	points(x[which(y == max(y))[1]], max(y), col="red", cex=2)

	#draw a segment related to max avg force in window
	if(maxAvgForceInWindow > 0)
	{
		segments(x[maxAvgForceInWindowSampleStart], maxAvgForceInWindow,
			 x[maxAvgForceInWindowSampleEnd], maxAvgForceInWindow, lwd=2, col="green4")
		topTick = maxAvgForceInWindow/100
		segments(x[maxAvgForceInWindowSampleStart], maxAvgForceInWindow - topTick,
			 x[maxAvgForceInWindowSampleStart], maxAvgForceInWindow + topTick,
			 lwd=2, col="green4")
		segments(x[maxAvgForceInWindowSampleEnd], maxAvgForceInWindow - topTick,
			 x[maxAvgForceInWindowSampleEnd], maxAvgForceInWindow + topTick,
			 lwd=2, col="green4")
	}

	endGraph()
}

start <- function(op)
{
	if(op$singleOrMultiple == "TRUE")
	{
		datetime = paste(op$date, op$time, sep=" ")
		dataFile <- paste(tempPath, "/cj_mif_Data.csv", sep="")
		pngFile <- paste(tempPath, "/cj_mif_Graph.png", sep="")
		doProcess(pngFile, dataFile, op$decimalCharAtFile, op$title, op$exercise,
				datetime, op$captureOptions, op$startSample, op$endSample)
	} else {
		#export
		#1) define exportDF and the model vector if model does not succeed
		exportDF = NULL
		exportModelVector = NULL

		exportModelVectorOnFail = NA 						#fmax
		for(i in 1:length(op$drawRfdOptions)) {					#RFDs
			RFDoptions = readRFDOptions(op$drawRfdOptions[i])
			if(RFDoptions$rfdFunction != "-1")
				exportModelVectorOnFail = c(exportModelVectorOnFail, NA)
		}
		impulseOptions = readImpulseOptions(op$drawImpulseOptions)
		if(impulseOptions$impulseFunction != "-1")
			exportModelVectorOnFail = c(exportModelVectorOnFail, NA) 		#impulse

		exportModelVectorOnFail = c(exportModelVectorOnFail, NA) 		#model error

		#preparing header row (each set will have this in the result dataframe to be able to combine them)
		maxAvgWindowSecondsHeader = op$maxAvgWindowSeconds
		if(op$decimalCharAtExport == ",")
			maxAvgWindowSecondsHeader = format(maxAvgWindowSecondsHeader, decimal.mark=",")

		exportNames = c("Name","Date","Time","Exercise","Laterality","Set","Repetition","MaxForce (raw)",paste("Max AVG Force in", maxAvgWindowSecondsHeader, "s (raw)"),"MaxForce (model)")
		for(i in 1:length(op$drawRfdOptions))
		{
			RFDoptions = readRFDOptions(op$drawRfdOptions[i])
			if(RFDoptions$rfdFunction != "-1")
				exportNames = c(exportNames, paste("RFD", RFDoptions$rfdFunction, RFDoptions$type,
						RFDoptions$start, RFDoptions$end, sep ="_"))
		}

		if(impulseOptions$impulseFunction != "-1")
			exportNames = c(exportNames, paste("Impulse", impulseOptions$impulseFunction, impulseOptions$type,
					impulseOptions$start, impulseOptions$end, sep ="_"))

		exportNames = c(exportNames, "Model mean error (%)")
		exportNames = c(exportNames, "Comments (set)")

		if(op$includeImagesOnExport)
			exportNames = c(exportNames, "Image")

		#2) read the csv
		dataFiles = read.csv(file = paste(tempPath, "/maximumIsometricForceInputMulti.csv", sep=""), sep=";", stringsAsFactors=F)

		#3) call doProcess
		progressFolder = paste(tempPath, "/chronojump_export_progress", sep ="")
		tempGraphsFolder = paste(tempPath, "/chronojump_force_sensor_export_graphs_rfd/", sep ="")
		tempGraphsABFolder = paste(tempPath, "/chronojump_force_sensor_export_graphs_ab/", sep ="")

		#countGraph = 1
		for(i in 1:length(dataFiles[,1]))
		{
			print("fullURL")
			print(as.vector(dataFiles$fullURL[i]))
			pngFile <- paste(tempGraphsFolder, i, ".png", sep="")  #but remember to graph also when model fails

			modelOk = FALSE
			executing  <- tryCatch({
				exportModelVector = doProcess(pngFile, as.vector(dataFiles$fullURL[i]),
					dataFiles$decimalChar[i], dataFiles$title[i], dataFiles$exercise[i], paste(dataFiles$date[i], dataFiles$time[i], sep=" "),
					dataFiles$captureOptions[i], dataFiles$startSample[i], dataFiles$endSample[i])
				#countGraph = countGraph +1 #only adds if not error, so the numbering of graphs matches rows in CSV

				modelOk = TRUE
			}, error = function(e) {
				print("error on doProcess:")
				print(message(e))
				endGraph() #close graph that is being done to not receive error: too many open devices
			})

			pngFile <- paste(tempGraphsABFolder, i, ".png", sep="")  #but remember to graph also when model fails
			plotABGraph(pngFile, as.vector(dataFiles$fullURL[i]),
					dataFiles$decimalChar[i], dataFiles$title[i], dataFiles$exercise[i], paste(dataFiles$date[i], dataFiles$time[i], sep=" "),
					dataFiles$captureOptions[i], dataFiles$startSample[i], dataFiles$endSample[i],
					dataFiles$maxAvgForceInWindow[i],
					(dataFiles$maxAvgForceInWindowSampleStart[i] +1), # +1 because the C# count starts at 0 and R at 1
					(dataFiles$maxAvgForceInWindowSampleEnd[i] +1)
			)

			if(! modelOk)
				exportModelVector = exportModelVectorOnFail #done here and not on the catch, because it didn't worked there

			#mix strings and numbers directly in a data frame to not have numbers as text (and then cannot export with decimal , or .)
			exportSetDF = data.frame(dataFiles$title[i], dataFiles$date[i], dataFiles$time[i],
					dataFiles$exercise[i], dataFiles$laterality[i], dataFiles$set[i], dataFiles$rep[i])
			exportSetDF = cbind (exportSetDF, dataFiles$maxForceRaw[i])
			exportSetDF = cbind (exportSetDF, dataFiles$maxAvgForceInWindow[i])
			for(j in 1:length(exportModelVector))
				exportSetDF = cbind (exportSetDF, exportModelVector[j])

			exportSetDF = cbind (exportSetDF, dataFiles$comments[i])
			if(op$includeImagesOnExport)
				exportSetDF = cbind(exportSetDF, paste(i, ".png", sep=""))

			colnames(exportSetDF) = exportNames

			#add to export data frame: exportDF
			exportDF <- rbind(exportDF, exportSetDF)

			progressFilename = paste(progressFolder, "/", i, sep="")
			file.create(progressFilename)
			print("done")
		}

		#3) write the file
		if(is.null(exportDF)) {
			# write something blank to be able to know in C# that operation ended
			write(0, file = paste(tempPath, "/chronojump_force_sensor_export.csv", sep = ""))
		} else {
			#print csv
			if(op$decimalCharAtExport == ".")
				write.csv(exportDF, file = paste(tempPath, "/chronojump_force_sensor_export.csv", sep = ""), row.names = FALSE, col.names = TRUE, quote = FALSE, na="")
			else if(op$decimalCharAtExport == ",")
				write.csv2(exportDF, file = paste(tempPath, "/chronojump_force_sensor_export.csv", sep = ""), row.names = FALSE, col.names = TRUE, quote = FALSE, na="")
		}
	}
}

start(op)

#dynamics = getDynamicsFromLoadCellFile("~/ownCloud/Xavier/Recerca/Yoyo-Tests/Galga/RowData/APl1", averageLength = 0.1, percentChange = 5, sep = ";", dec = ",")
#drawDynamicsFromLoadCell(titlefull, dynamics, vlineT0=F, vline50fmax.raw=F, vline50fmax.fitted=T, hline50fmax.raw=F, hline50fmax.fitted=T, 
#                         rfd0.fitted=T, rfd100.raw=F, rfd0_100.raw=F, rfd0_100.fitted = F, rfd200.raw=F, rfd0_200.raw=F, rfd0_200.fitted = F,
#                         rfd50pfmax.raw=F, rfd50pfmax.fitted=T)
#getDynamicsFromLoadCellFolder("~/Documentos/RowData/", resultFileName = "~/Documentos/results.csv", op$captureOptions)
