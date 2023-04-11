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
#   Copyright (C) 2014-2023  	Xavier de Blas <xaviblas@gmail.com>
#   Copyright (C) 2014-2020   	Xavier Padullés <x.padulles@gmail.com>
# 

#    ----
#    This code is taken from util.R
#    ----

extrema <- function(y, ndata = length(y), ndatam1 = ndata - 1) {

	minindex <- maxindex <- NULL; nextreme <- 0; cross <- NULL; ncross <- 0 

	z1 <- sign(diff(y))
	index1 <- seq(1, ndatam1)[z1 != 0]; z1 <- z1[z1 != 0]  

	if (!(is.null(index1) || all(z1==1) || all(z1==-1))) {

		index1 <- index1[c(z1[-length(z1)] != z1[-1], FALSE)] + 1 
		z1 <- z1[c(z1[-length(z1)] != z1[-1], FALSE)]  

		nextreme <- length(index1)

		if(nextreme >= 2)
			for(i in 1:(nextreme-1)) {
				tmpindex <- index1[i]:(index1[i+1]-1)
				if(z1[i] > 0) {
					tmpindex <- tmpindex[y[index1[i]] == y[tmpindex]]
					maxindex <- rbind(maxindex, c(min(tmpindex), max(tmpindex)))
				} else {
					tmpindex <- tmpindex[y[index1[i]] == y[tmpindex]]
					minindex <- rbind(minindex, c(min(tmpindex), max(tmpindex)))
				}     
			} 

		tmpindex <- index1[nextreme]:ndatam1  
		if(z1[nextreme] > 0) {
			tmpindex <- tmpindex[y[index1[nextreme]] == y[tmpindex]]
			maxindex <- rbind(maxindex, c(min(tmpindex), max(tmpindex)))
		} else {
			tmpindex <- tmpindex[y[index1[nextreme]] == y[tmpindex]]
			minindex <- rbind(minindex, c(min(tmpindex), max(tmpindex)))
		}  

		### Finding the index of zero crossing  

		if (!(all(sign(y) >= 0) || all(sign(y) <= 0) || all(sign(y) == 0))) {
			index1 <- c(1, index1)
			for (i in 1:nextreme) {
				if (y[index1[i]] == 0) {
					tmp <- c(index1[i]:index1[i+1])[y[index1[i]:index1[i+1]] == 0]
					cross <- rbind(cross, c(min(tmp), max(tmp)))                 
				} else
					if (y[index1[i]] * y[index1[i+1]] < 0) {
						tmp <- min(c(index1[i]:index1[i+1])[y[index1[i]] * y[index1[i]:index1[i+1]] <= 0])
						if (y[tmp] == 0) {
							tmp <- c(tmp:index1[i+1])[y[tmp:index1[i+1]] == 0]
							cross <- rbind(cross, c(min(tmp), max(tmp))) 
						} else 
							cross <- rbind(cross, c(tmp-1, tmp)) 
					}
			}
			#if (y[ndata] == 0) {
			#    tmp <- c(index1[nextreme+1]:ndata)[y[index1[nextreme+1]:ndata] == 0]
			#    cross <- rbind(cross, c(min(tmp), max(tmp)))         
			#} else
			if (any(y[index1[nextreme+1]] * y[index1[nextreme+1]:ndata] <= 0)) {
				tmp <- min(c(index1[nextreme+1]:ndata)[y[index1[nextreme+1]] * y[index1[nextreme+1]:ndata] <= 0])
				if (y[tmp] == 0) {
					tmp <- c(tmp:ndata)[y[tmp:ndata] == 0]
					cross <- rbind(cross, c(min(tmp), max(tmp))) 
				} else
					cross <- rbind(cross, c(tmp-1, tmp))
			}
			ncross <- nrow(cross)        
		}
	}

	#extrema cross working:
	#a=c(-3,-2,-1,0,1,2,1)
	#extrema(a)$cross
	#     [,1] [,2]
	#[1,]    4    4
	#extrema(a)$ncross
	#[1] 1
	#
	#But, if there's no change of direction, cross does not find anything:
	#a=c(-3,-2,-1,0,1,2)
	#extrema(a)$cross
	#NULL
	#extrema(a)$ncross
	#[1] 0
	#
	#then find a cross in this situation. Find just one cross.
	if(ncross == 0)	{
		positiveAtStart = (y[1] >= 0)
		for(i in 1:length(y)) {
			if( (y[i] >= 0) != positiveAtStart) { #if there's a sign change
				cross = rbind(cross, c(i-1,i))
				ncross = 1
				break
			}
		}
	}

	list(minindex=minindex, maxindex=maxindex, nextreme=nextreme, cross=cross, ncross=ncross)
}

#signal is the information coming from the encoder, graph is to debug
#see codeExplained/image detect-and-fix-inertial-string-not-fully-extended.png
fixInertialSignalIfNotFullyExtended <- function(signal, saveFile, graph)
{
	write("at fixInertialSignalIfNotFullyExtended", stderr())
	angle <- cumsum(signal) #360 degrees every 200 ticks

	maximums <- extrema(angle)$maxindex[,1]
	minimums <- extrema(angle)$minindex[,1]
	maximumsCopy <- maximums #store this value
	minimumsCopy <- minimums #store this value

	#if we have more than 2 max & mins, remove the first and last value
	if(length(maximums) > 2 & length(minimums) > 2)
	{
		#if there's any max extrema value negative (remove it), same for positive min values
		maximums.temp = NULL
		minimums.temp = NULL
		for( i in maximums )
			if(angle[i] > 0)
				maximums.temp <- c(maximums.temp, i)
		for( i in minimums )
			if(angle[i] < 0)
				minimums.temp <- c(minimums.temp, i)

		maximums <- maximums.temp
		minimums <- minimums.temp

		if(length(maximums) < 1 | length(minimums) < 1)
			return(signal)


		#remove the first value of the maximums OR minimums (just the first one of both)
		if(maximums[1] < minimums[1])
			maximums <- maximums[-1]
		else
			minimums <- minimums[-1]

		if(length(maximums) < 1 | length(minimums) < 1)
			return(signal)


		#remove the last value of the maximums OR minimums (just the last one of both)
		if(maximums[length(maximums)] > minimums[length(minimums)])
			maximums <- maximums[-length(maximums)]
		else
			minimums <- minimums[-length(minimums)]
	}

	#return if no data
	if(length(maximums) < 1 | length(minimums) < 1)
		return(signal)

	#ensure both maximums and minimums have same length
	while(length(maximums) != length(minimums))
	{
		if(length(maximums) > length(minimums))
			maximums <- maximums[-length(maximums)]
		else if(length(maximums) < length(minimums))
			minimums <- minimums[-length(minimums)]
	}

	meanByExtrema <- mean(c(angle[maximums], angle[minimums]))
	angleCorrected <- angle - meanByExtrema

	#remove the initial part of the signal. Remove from ms 1 to when angleCorrected crosses 0
	angleCorrectedCrossZero = extrema(angleCorrected)$cross[1,1]

	if(graph) {
		png (paste(saveFile,".png",sep=""), width=1200, height=1000)
		par(mfrow=c(1,2))

		#1st graph (left)
		plot(angle, type="l", lty=2, xlab="time", ylab="Angle", main="String NOT fully extended",
		     ylim=c(min(c(angle[minimums], -angle[maximums])) - abs(meanByExtrema), max(c(angle[maximums], -angle[minimums])) + abs(meanByExtrema)))
		lines(abs(angle)*-1, lwd=2)
		points(maximumsCopy, angle[maximumsCopy], col="black", cex=1)
		points(minimumsCopy, angle[minimumsCopy], col="black", cex=1)
		points(maximums, angle[maximums], col="green", cex=3)
		points(minimums, angle[minimums], col="green", cex=3)
		abline(h = meanByExtrema, col="red")
		text(x = 0, y = meanByExtrema, labels = round(meanByExtrema,2), adj=0)

		#2nd graph (right)
		plot(angleCorrected, type="l", lty=2, xlab="time", ylab="angle", main="Corrected set",
		     ylim=c(min(c(angle[minimums], -angle[maximums])) - 2*abs(meanByExtrema), max(c(angle[maximums], -angle[minimums]))))
		lines(abs(angleCorrected)*-1, lwd=2)
		abline(v=c(angleCorrectedCrossZero, length(angleCorrected)), col="green")
		mtext("Start", at=angleCorrectedCrossZero, side=3, col="green")
		mtext("EnfixInertialSignalIfNotFullyExtendedd", at=length(angleCorrected), side=3, col="green")

		par(mfrow=c(1,1))
		dev.off ()
	}

	#write(signal, file="/tmp/old.txt", ncolumns=length(signal), sep=", ")
	signal <- signal[angleCorrectedCrossZero:length(signal)]

	#write("SIGNAL CORRECTED", specialDataFile)

	#write to file and return displacement to be used
	write(signal, file=saveFile, ncolumns=length(signal), sep=", ")
}

doProcess <- function (originalDir, convertedDir, graph)
{
	setwd (originalDir)
	filenames = list.files()
	print (filenames)
	for (i in 1:length(filenames))
	{
		signal <- scan (file=filenames[i], sep=",")
		print (paste(convertedDir, filenames[i], sep="/"))
		fixInertialSignalIfNotFullyExtended (signal, paste(convertedDir, filenames[i], sep="/"), graph)
	}
}


