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
	if(os == "Windows")
		Cairo(width, height, file = pngFile, type="png", bg="white")
	else
		png(pngFile, width=width, height=height)
}

endGraph <- function()
{
	dev.off()
}

assignOptions <- function(options) {
	return(list(
		    os 		= options[1],
		    graphWidth 	= as.numeric(options[2]),
		    graphHeight	= as.numeric(options[3]),
		    averageLength = as.numeric(options[4]),
		    percentChange = as.numeric(options[5]),
		    vlineT0 	= as.numeric(options[6]),
		    vline50fmax.raw 	= as.numeric(options[7]),
		    vline50fmax.fitted 	= as.numeric(options[8]),
		    hline50fmax.raw 	= as.numeric(options[9]),
		    hline50fmax.fitted 	= as.numeric(options[10]),
		    rfd0.fitted 	= as.numeric(options[11]),
		    rfd100.raw 	        = as.numeric(options[12]),
		    rfd0_100.raw 	= as.numeric(options[13]),
		    rfd0_100.fitted     = as.numeric(options[14]),
		    rfd200.raw  	= as.numeric(options[15]),
		    rfd0_200.raw 	= as.numeric(options[16]),
		    rfd0_200.fitted 	= as.numeric(options[17]),
		    rfd50fmax.raw 	= as.numeric(options[18]),
		    rfd50fmax.fitted 	= as.numeric(options[19])
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


#Fits the data to the model f = fmax*(1 - exp(-K*t))
#Important! It fits the data with the axes moved to f0 and startTime. The real maximum force is fmax + f0
getForceModel <- function(time, force, startTime, # startTime is the instant when the force start to increase
                   fmaxi,           # fmaxi is the initial value for the force. For numeric purpouses
                   f0)              # f0 is the sustained force before the increase
        {
        timeTrimmed = time[which(time == startTime):length(time)]
        forceTrimmed = force[which(time == startTime):length(time)]
        timeTrimmed = timeTrimmed -  startTime
        data = data.frame(time = timeTrimmed, force = forceTrimmed)
        model = nls( force ~ fmax*(1-exp(-K*time)), data, start=list(fmax=fmaxi, K=1), control=nls.control(warnOnly=TRUE))
        fmax = summary(model)$coeff[1,1]
        K = summary(model)$coeff[2,1]
        return(list(fmax = fmax - f0, K = K))
}

getDynamicsFromLoadCellFile <- function(inputFile, averageLength = 0.1, percentChange = 5)
{
        originalTest = read.csv(inputFile, header = F, dec = ".", sep = ";", skip = 2)
        colnames(originalTest) <- c("Time", "Force")
        originalTest$Time = as.numeric(originalTest$Time)
        
        #Finding the decrease of the foce to detect the end of the maximum voluntary force
        trimmingSamples = getTrimmingSamples(originalTest, averageLength = averageLength, percentChange = percentChange)
        startSample = trimmingSamples$startSample
        startTime = originalTest$Time[startSample]
        
        endSample = trimmingSamples$endSample
        endTime = originalTest$Time[endSample]
        
        f0.raw = mean(originalTest$Force[1:(startSample - 10)]) # Initial force.
        fmax.raw = max(originalTest$Force)
        
        #Trimming the data before and after contraction
        test = originalTest[startSample:endSample,]

        f.smoothed = getMovingAverageForce(originalTest, averageLength = averageLength) #Running average with equal weight of 1/18 (200 ms)
        fmax.smoothed = max(f.smoothed, na.rm =T)
        
        model = getForceModel(test$Time, test$Force, startTime, fmax.smoothed, f0.raw)
        f.fitted = f0.raw + model$fmax*(1-exp(-model$K*(originalTest$Time - startTime)))
        
        f100.raw = test$Force[which.min(abs(test$Time - (startTime + 0.1)))]         #Force at t=100ms
        f100.fitted = model$fmax*(1 - exp( - model$K * 0.1)) + f0.raw
        f200.raw = test$Force[which.min(abs(test$Time - (startTime + 0.2)))]         #Force at t=200ms
        f200.fitted = model$fmax*(1 - exp( - model$K * 0.2)) + f0.raw
        f90.raw = test$Force[which.min(abs(test$Time - (startTime + 0.09)))]        #Force at t=90ms
        f110.raw = test$Force[which.min(abs(test$Time - (startTime + 0.11)))]       #Force at t=110ms
        f190.raw = test$Force[which.min(abs(test$Time - (startTime + 0.19)))]       #Force at t=190ms
        f210.raw = test$Force[which.min(abs(test$Time - (startTime + 0.21)))]       #Force at t=210ms
        f50fmax.raw = test$Force[which.min(abs(test$Force - fmax.raw/2))]
        f50fmax.fitted = (model$fmax + f0.raw)/2
        
        rfd0.fitted = model$fmax * model$K # RFD at t=0ms using the exponential model
        rfd100.raw = (f110.raw - f90.raw) / 0.02  #rfd at t= 100ms. Mean value using de previous and the next value and divided by 20ms
        rfd0_100.raw = (f100.raw - f0.raw) / 0.1  #Mean rfd during t=[0..100]
        rfd0_100.fitted = (f100.fitted - f0.raw) / 0.1
        rfd200.raw = (f210.raw - f190.raw) / 0.02
        rfd0_200.raw = (f200.raw - f0.raw) / 0.2
        rfd0_200.fitted = (f200.fitted - f0.raw) / 0.2
        rfd100_200.raw = ((f200.raw - f100.raw) * 10)
        
        t50fmax.raw = test$Time[which.min(abs(test$Force - fmax.raw/2))]
        t50fmax.fitted = originalTest$Time[which.min(abs(f.fitted - (model$fmax + f0.raw)/ 2))]
        rfd50fmax.raw = (test$Force[which.min(abs(test$Force - fmax.raw/2)) + 1] - test$Force[which.min(abs(test$Force - fmax.raw/2)) - 1]) * 100 / 2 #RFD at the moment that the force is 50% of the fmax
        rfd50fmax.fitted = model$fmax * model$K * exp(-model$K*(t50fmax.fitted - startTime))
        
        return(list(nameOfFile = inputFile, time = originalTest[, "Time"], fmax.fitted = model$fmax, k.fitted = model$K,
                    startTime = startTime, entTime = endTime,
                    startSample = startSample, endSample = endSample,
                    f0 = f0.raw,
                    fmax.raw = fmax.raw, fmax.smoothed = fmax.smoothed,
                    f100.raw = f100.raw,
                    f100.fitted = f100.fitted,
                    f200.raw = f200.raw,
                    f200.fitted = f200.fitted,
                    f50fmax.raw = f50fmax.raw,
                    f50fmax.fitted = f50fmax.fitted,
                    rfd0.fitted = rfd0.fitted,
                    rfd100.raw = rfd100.raw,
                    rfd0_100.raw = rfd0_100.raw,
                    rfd0_100.fitted = rfd0_100.fitted,
                    rfd0_200.raw = rfd0_200.raw,
                    rfd0_200.fitted = rfd0_200.fitted,
                    rfd200.raw = rfd200.raw,
                    t50fmax.raw = t50fmax.raw, rfd50fmax.raw = rfd50fmax.raw,
                    t50fmax.fitted = t50fmax.fitted, rfd50fmax.fitted = rfd50fmax.fitted,
                    f.raw = originalTest$Force, f.smoothed = f.smoothed, f.fitted = f.fitted,
                    endTime = endTime))
}

#drawDynamicsFromLoadCell <- function(dynamics, pdfFilename = "/tmp/loadCellsMaxIsomForce.pdf", vlineT0=T, vline50fmax.raw=F, vline50fmax.fitted=F, hline50fmax.raw=F, hline50fmax.fitted=F,
drawDynamicsFromLoadCell <- function(
				     dynamics, vlineT0=T, vline50fmax.raw=F, vline50fmax.fitted=F,
				     hline50fmax.raw=F, hline50fmax.fitted=F,
                                     rfd0.fitted=F, rfd100.raw=F, rfd0_100.raw=F, rfd0_100.fitted = F, rfd200.raw=F, rfd0_200.raw=F,
				     rfd0_200.fitted = F, rfd50fmax.raw=F, rfd50fmax.fitted=F,
                                     xlimits=NA)
{
#        pdf(file = pdfFilename, paper="a4r", width = 12, height = 12)
        
        #Plotting raw data
        print(dynamics$time[dynamics$startSample])
        yHeight = dynamics$fmax.raw * 1.1
        if (!is.na(xlimits[1])){
                xWidth = xlimits[2] - xlimits[1]
                plot(dynamics$time[dynamics$startSample:dynamics$endSample] , dynamics$f.raw[dynamics$startSample:dynamics$endSample],
                     type="l", xlab="Time[s]", ylab="Force[N]", xlim = xlimits, ylim=c(0, yHeight),
                     main = dynamics$nameOfFile, yaxs= "i", xaxs = "i")
        } else if (is.na(xlimits[1])){
                xmin = dynamics$startTime*0.97 - dynamics$time[1]*0.03
                xmax = dynamics$endTime*1.1 - dynamics$startTime*0.1
                xWidth = xmax - xmin
                plot(dynamics$time[dynamics$startSample:dynamics$endSample] , dynamics$f.raw[dynamics$startSample:dynamics$endSample],
                     type="l", xlab="Time[s]", ylab="Force[N]",
                     xlim = c(xmin, xmax),
                     ylim=c(0, yHeight),
                     main = dynamics$nameOfFile, yaxs= "i", xaxs = "i")
        }
        lines(dynamics$time[1:dynamics$startSample] , dynamics$f.raw[1:dynamics$startSample], type = "c")
        lines(dynamics$time[dynamics$endSample: length(dynamics$time)] , dynamics$f.raw[dynamics$endSample: length(dynamics$time)], type = "c")
        
        text( x = min(which(dynamics$f.raw == max(dynamics$f.raw))/100), y = dynamics$fmax.raw, labels = paste("Fmax = ", round(dynamics$fmax.raw, digits=2), " N", sep=""), pos = 3)
        
        #Plotting fitted data
        lines(dynamics$time, dynamics$f.fitted, col="blue")
        text(x = dynamics$time[length(dynamics$time)], y = dynamics$fmax.fitted - 30, labels = paste("Fmax =", round(dynamics$fmax.fitted, digits = 2), "N"), pos = 4, col="blue")
        axis(4, at = dynamics$fmax.fitted + dynamics$f0, labels = round(dynamics$fmax.fitted + dynamics$f0, digits = 2), col = "blue")
        
        #Plottting smoothed data
        lines(dynamics$time, dynamics$f.smoothed, col="grey")
        
        if(vlineT0){
                abline(v = dynamics$startTime, lty = 2)
                axis(3, at = dynamics$startTime, labels = dynamics$startTime)
        }
        if(hline50fmax.raw){
                abline(h = dynamics$fmax.raw/2, lty = 2)
                text( x = dynamics$t50fmax.raw + 0.5, y = dynamics$fmax.raw/2, labels = paste("Fmax/2 =", round(dynamics$fmax.raw/2, digits=2), "N", sep=""), pos = 3)
        }
        if(hline50fmax.fitted){
                abline( h = (dynamics$fmax.fitted + dynamics$f0)/2, lty = 2, col = "blue")
                text( x =dynamics$t50fmax.fitted + 0.5, y = (dynamics$fmax.fitted + dynamics$f0)/2, labels = paste("Fmax/2 =", round((dynamics$fmax.fitted + dynamics$f0)/2, digits=2), "N", sep=""), pos = 1, col="blue")
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
        
        #Plotting RFD0 
        if(rfd0.fitted)
        {
                windowSlope = dynamics$rfd0.fitted*(plotHeight/yHeight)/(plotWidth/xWidth)
                intercepstartTime.fitted = dynamics$f.fitted[which.min(abs(dynamics$time - dynamics$startTime))] - dynamics$rfd0.fitted*(dynamics$startTime)
                abline(a = intercepstartTime.fitted, b = dynamics$rfd0.fitted, col="blue", lty = 2)
                text(x = (yHeight - 5 - intercepstartTime.fitted)/dynamics$rfd0.fitted, y = yHeight - 5,
                     label=paste("RFD0 =", round(dynamics$rfd0.fitted, digits=0), "N/s"),
                     srt=atan(windowSlope)*180/pi, pos = 2, cex = 0.75, col = "blue")
                points(x = dynamics$startTime, y = dynamics$f0, col = "blue")
        }
        
        if(rfd100.raw)
        {
                windowSlope = dynamics$rfd100.raw*(plotHeight/yHeight)/(plotWidth/xWidth)
                intercept100.raw = dynamics$f100.raw - dynamics$rfd100.raw * (dynamics$startTime + 0.1)
                abline(a = intercept100.raw, b = dynamics$rfd100.raw, lty = 2)
                text(x = (yHeight - 5 - intercept100.raw)/dynamics$rfd100.raw, y = yHeight - 5,
                     label=paste("RFD100 =", round(dynamics$rfd100, digits=0), "N/s"),
                     srt=atan(windowSlope)*180/pi, pos = 2, cex = 0.75)
                points(x = dynamics$startTime + 0.1, y = dynamics$f100.raw)
        }
        
        if(rfd0_100.raw)
        {
                windowSlope = dynamics$rfd0_100.raw*(plotHeight/yHeight)/(plotWidth/xWidth)
                intercepstartTime_100.raw = dynamics$f0 - dynamics$rfd0_100.raw * dynamics$startTime
                abline(a = intercepstartTime_100.raw, b = dynamics$rfd0_100.raw, lty = 2)
                text(x = (yHeight - 5 - intercepstartTime_100.raw)/dynamics$rfd0_100.raw, y = yHeight - 5,
                     label=paste("RFD0_100 =", round(dynamics$rfd0_100.raw, digits=0), "N/s"),
                     srt=atan(windowSlope)*180/pi, pos = 2, cex = 0.75)
                points(x = c(dynamics$startTime, dynamics$startTime + 0.1), y = c(dynamics$f0, dynamics$f100.raw))
        }
        
        if(rfd0_100.fitted)
        {
                windowSlope = dynamics$rfd0_100.fitted*(plotHeight/yHeight)/(plotWidth/xWidth)
                intercepstartTime_100.fitted = dynamics$f0 - dynamics$rfd0_100.fitted * dynamics$startTime
                abline(a = intercepstartTime_100.fitted, b = dynamics$rfd0_100.fitted, lty = 2, col = "blue")
                text(x = (yHeight - 5 - intercepstartTime_100.fitted)/dynamics$rfd0_100.fitted, y = yHeight - 5,
                     label=paste("RFD0_100 =", round(dynamics$rfd0_100.fitted, digits=0), "N/s"),
                     srt=atan(windowSlope)*180/pi, pos = 2, cex = 0.75, col = "blue")
                points(x = c(dynamics$startTime, dynamics$startTime + 0.1), y = c(dynamics$f0, dynamics$f100.fitted), col = "blue")
        }
        
        if(rfd200.raw)
        {
                windowSlope = dynamics$rfd200.raw*(plotHeight/yHeight)/(plotWidth/xWidth)
                intercept200.raw = dynamics$f200.raw - dynamics$rfd200.raw * (dynamics$startTime + 0.2)
                abline(a = intercept200.raw, b = dynamics$rfd200.raw, lty = 2)
                text(x = (yHeight - 5 - intercept200.raw)/dynamics$rfd200.raw, y = yHeight - 5,
                     label=paste("RFD200 =", round(dynamics$rfd200.raw, digits=0), "N/s"),
                     srt=atan(windowSlope)*180/pi, pos = 2, cex = 0.75)
                points(x = dynamics$startTime + 0.2, y = dynamics$f200.raw)
        }
        if(rfd0_200.raw)
        {
                windowSlope = dynamics$rfd0_200.raw*(plotHeight/yHeight)/(plotWidth/xWidth)
                intercepstartTime_200.raw = dynamics$f0 - dynamics$rfd0_200.raw * dynamics$startTime
                abline(a = intercepstartTime_200.raw, b = dynamics$rfd0_200.raw, lty = 2)
                text(x = (yHeight - 5 - intercepstartTime_200.raw)/dynamics$rfd0_200.raw, y = yHeight - 5,
                     label=paste("RFD0_200 =", round(dynamics$rfd0_200.raw, digits=0), "N/s"),
                     srt=atan(windowSlope)*180/pi, pos = 2, cex = 0.75)
                points(x = c(dynamics$startTime, dynamics$startTime + 0.2), y = c(dynamics$f0,dynamics$f200.raw))
        }
        
        if(rfd0_200.fitted)
        {
                windowSlope = dynamics$rfd0_200.fitted*(plotHeight/yHeight)/(plotWidth/xWidth)
                intercepstartTime_200.fitted = dynamics$f0 - dynamics$rfd0_200.fitted * dynamics$startTime
                abline(a = intercepstartTime_200.fitted, b = dynamics$rfd0_200.fitted, lty = 2, col = "blue")
                text(x = (yHeight - 5 - intercepstartTime_200.fitted)/dynamics$rfd0_200.fitted, y = yHeight - 5,
                     label=paste("RFD0_200 =", round(dynamics$rfd0_200.fitted, digits=0), "N/s"),
                     srt=atan(windowSlope)*180/pi, pos = 2, cex = 0.75, col = "blue")
                points(x = c(dynamics$startTime, dynamics$startTime + 0.2), y = c(dynamics$f0,dynamics$f200.fitted), col = "blue")
        }
        
        if(rfd50fmax.raw){
                windowSlope = dynamics$rfd50fmax.raw*(plotHeight/yHeight)/(plotWidth/xWidth)
                intercept50fmax.raw = dynamics$f50fmax.raw - dynamics$rfd50fmax.raw * dynamics$t50fmax.raw
                abline(a = intercept50fmax.raw, b = dynamics$rfd50fmax.raw, lty = 2)
                text(x = (yHeight - 5 - intercept50fmax.raw)/dynamics$rfd50fmax.raw, y = yHeight - 5,
                     label=paste("RFD50fmax =", round(dynamics$rfd50fmax.raw, digits=0), "N/s"),
                     srt=atan(windowSlope)*180/pi, pos = 2, cex = 0.75)
                points(x =dynamics$t50fmax.raw, y = dynamics$f50fmax.raw)
        }
        
        if(rfd50fmax.fitted){
                windowSlope = dynamics$rfd50fmax.fitted*(plotHeight/yHeight)/(plotWidth/xWidth)
                intercept50fmax.fitted = dynamics$f50fmax.fitted - dynamics$rfd50fmax.fitted * dynamics$t50fmax.fitted
                abline(a = intercept50fmax.fitted, b = dynamics$rfd50fmax.fitted, col ="blue", lty = 2)
                text(x = (yHeight - 5 - intercept50fmax.fitted)/dynamics$rfd50fmax.fitted, y = yHeight - 5, col = "blue",
                     label=paste("RFD50fmax =", round(dynamics$rfd50fmax.fitted, digits=0), "N/s"),
                     srt=atan(windowSlope)*180/pi, pos=2, cex = 0.75)
                points(x = dynamics$t50fmax.fitted, y = dynamics$f50fmax.fitted, col = "blue")
        }
        #dev.off()
}

getDynamicsFromLoadCellFolder <- function(folderName, resultFileName, export2Pdf)
{
        originalFiles = list.files(path=folderName, pattern="*")
        nFiles = length(originalFiles)
        results = matrix(rep(NA, 16*nFiles), ncol = 16)
        colnames(results)=c("fileName", "fmax.fitted", "k.fitted", "fmax.raw", "startTime", "f0", "fmax.smoothed",
                            "rfd0.fitted", "rfd100.raw", "rfd0_100.raw", "rfd0_100.fitted",
                            "rfd200.raw", "rfd0_200.raw", "rfd0_200.fitted",
                            "rfd50fmax.raw", "rfd50fmax.fitted")
        
        results[,"fileName"] = originalFiles
        
        for(i in 1:nFiles)
        {
                dynamics = getDynamicsFromLoadCellFile(paste(folderName,originalFiles[i], sep = ""))
                
                results[i, "fileName"] = dynamics$nameOfFile
                results[i, "fmax.fitted"] = dynamics$fmax.fitted
                results[i, "k.fitted"] = dynamics$k.fitted
                results[i, "fmax.raw"] = dynamics$fmax.raw
                results[i, "startTime"] = dynamics$startTime
                results[i, "f0"] = dynamics$f0
                results[i, "fmax.smoothed"] = dynamics$fmax.smoothed
                results[i, "rfd0.fitted"] = dynamics$rfd0.fitted
                results[i, "rfd100.raw"] = dynamics$rfd100.raw
                results[i, "rfd0_100.raw"] = dynamics$rfd0_100.raw
                results[i, "rfd0_100.fitted"] = dynamics$rfd0_100.fitted
                results[i, "rfd200.raw"] = dynamics$rfd200.raw
                results[i, "rfd0_200.raw"] = dynamics$rfd0_200.raw
                results[i, "rfd0_200.fitted"] = dynamics$rfd0_200.fitted
                results[i, "rfd50fmax.raw"] = dynamics$rfd50fmax.rawfilter(test$Force, rep(1/19, 19), sides = 2)
                results[i, "rfd50fmax.fitted"] = dynamics$rfd50fmax.fitted
        }
        write.table(results, file = resultFileName, sep = ";", dec = ",", col.names = NA)
        return(results)
        
}

#Finds the sample in which thpercentChangee force decrease a given percentage of the maximum force.
#The maximum force is calculed from the moving average of averageLength seconds
getTrimmingSamples <- function(test, movingAverageForce, averageLength = 0.1, percentChange = 5)
{
        movingAverageForce = getMovingAverageForce(test, averageLength = 0.1)
        maxAverageForce = max(movingAverageForce, na.rm = T)
        maxSample = min(which(movingAverageForce == maxAverageForce), na.rm = T)
        
        #Detecting an increase of percentChange% of the maximum force in the initial signal
        startSample = 1
        while((test$Force[startSample] + test$Force[startSample +2]) / 2 <= maxAverageForce*1.5*percentChange/100)
        {
                startSample = startSample + 1
        } 
        
        #Detecting a decrease of percentChange% in the maximum force
        endSample = min(which((movingAverageForce < maxAverageForce*(100 - percentChange) / 100 &
                                       test$Time > test$Time[maxSample])), na.rm = T)
        
        return(list(startSample = startSample, endSample = endSample))
}

getMovingAverageForce <- function(test, averageLength = 0.1)
{
        sampleRate = (length(test$Time) - 1) / (test$Time[length(test$Time)] - test$Time[1])
        lengthSamples = round(averageLength * sampleRate, digits = 0)
        movingAverageForce = filter(test$Force, rep(1/lengthSamples, lengthSamples), sides = 2)
        return(movingAverageForce)
}

prepareGraph(op$os, pngFile, op$graphWidth, op$graphHeight)
dynamics = getDynamicsFromLoadCellFile(dataFile, op$averageLength, op$percentChange)
drawDynamicsFromLoadCell(dynamics, op$vlineT0, op$vline50fmax.raw, op$vline50fmax.fitted, op$hline50fmax.raw, op$hline50fmax.fitted,
                         op$rfd0.fitted, op$rfd100.raw, op$rfd0_100.raw, op$rfd0_100.fitted, op$rfd200.raw, op$rfd0_200.raw, op$rfd0_200.fitted,
                         op$rfd50fmax.raw, op$rfd50fmax.fitted)
endGraph()

#dynamics = getDynamicsFromLoadCellFile("~/ownCloud/Xavier/Recerca/Yoyo-Tests/Galga/RowData/APl1", averageLength = 0.1, percentChange = 5, sep = ";", dec = ",")
#drawDynamicsFromLoadCell(dynamics, vlineT0=F, vline50fmax.raw=F, vline50fmax.fitted=T, hline50fmax.raw=F, hline50fmax.fitted=T, 
#                         rfd0.fitted=T, rfd100.raw=F, rfd0_100.raw=F, rfd0_100.fitted = F, rfd200.raw=F, rfd0_200.raw=F, rfd0_200.fitted = F,
#                         rfd50fmax.raw=F, rfd50fmax.fitted=T)
#getDynamicsFromLoadCellFolder("~/Documentos/RowData/", resultFileName = "~/Documentos/results.csv")
