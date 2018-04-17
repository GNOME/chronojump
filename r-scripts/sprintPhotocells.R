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
#   Copyright (C) 2017   	Xavier de Blas <xaviblas@gmail.com>

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
                positions  	= as.numeric(unlist(strsplit(options[2], "\\;"))),
                splitTimes 	= as.numeric(unlist(strsplit(options[3], "\\;"))),
                mass 	= as.numeric(options[4]),
                personHeight = as.numeric(options[5]),
                tempC 	= as.numeric(options[6]),
                os 		= options[7],
                graphWidth 	= as.numeric(options[8]),
                graphHeight	= as.numeric(options[9])
        ))
}

#-------------- assign options -------------
op <- assignOptions(options)
#print(op$positions)

#Returns the K and Vmax parameters of the sprint using a number of pairs (time, position)
getSprintFromPhotocell <- function(positions, splitTimes, noise=0)
{
        # TODO: If the photocell is not in the 0 meters we must find how long is the time from
        #starting the race to the reaching of the photocell
        # t0 = 0
        # splitTimes = splitTimes + t0
                                                
	#noise is for testing purpouses.
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
        
        #Asuming that the first time and position are 0s it is not necessary to use the non linear regression
        #if there's only three positions. Substituting x1 = x(t1), and x2 = x(t2) whe have an exact solution.
        #2 variables (K and Vmax) and 2 equations.
        if (length(positions) == 3){
                return(getSprintFrom2SplitTimes(positions[2], positions[3], splitTimes[2], splitTimes[3], tolerance = 0.0001, initK = 1 ))
        }
        
        photocell = data.frame(time = splitTimes, position = positions)
        
        # Using the model of v = Vmax(1 - exp(-K*t)). If this function are integrated and we calculate the integration constant (t=0 -> position = 0)
        # position = Vmax*(time + (1/K)*exp(-K*time)) -Vmax/K
        pos.model = nls(position ~ Vmax*(time + (1/K)*exp(-K*time)) -Vmax/K, photocell, start = list(K = 0.81, Vmax = 10), control=nls.control(maxiter=1000, warnOnly=TRUE))
        K = summary(pos.model)$coeff[1,1]
        Vmax = summary(pos.model)$coeff[2,1]

        summary(pos.model)$coef[1:2,1]
        return(list(K = K, Vmax = Vmax))
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

drawSprintFromPhotocells <- function(sprintDynamics, splitTimes, positions, title, plotFittedSpeed = T, plotFittedAccel = T, plotFittedForce = T, plotFittedPower = T)
{
        
        maxTime = splitTimes[length(splitTimes)]
        time = seq(0, maxTime, by=0.01)
        #Calculating measured average speeds
        avg.speeds = diff(positions)/diff(splitTimes)
        textXPos = splitTimes[1:length(splitTimes) - 1] + diff(splitTimes)/2
        
        # Plotting average speed
        par(mar = c(7, 4, 5, 6.5))
        barplot(height = avg.speeds, width = diff(splitTimes), space = 0, ylim = c(0, max(c(avg.speeds, sprintDynamics$Vmax) + 1)),
                main=title, xlab="Time[s]", ylab="Velocity[m/s]",
                axes = FALSE, yaxs= "i", xaxs = "i")
        text(textXPos, avg.speeds, round(avg.speeds, digits = 2), pos = 3)
        axis(3, at = splitTimes, labels = splitTimes)
        
        # Fitted speed plotting
        par(new=T)
        plot(time, sprintDynamics$v.fitted, type = "l", xlab="", ylab = "",  ylim = c(0, max(c(avg.speeds, sprintDynamics$Vmax) + 1)), yaxs= "i", xaxs = "i") # Fitted data
        #axis(2, at = sprintDynamics$Vmax.fitted, labels = round(sprintDynamics$Vmax.fitted, digits = 2), line = 1, lwd = 0)
        abline(h = sprintDynamics$Vmax, lty = 2)
        #mtext(side = 1, line = 5, at = splitTimes[length(splitTimes)]*0.25, cex = 2 , substitute(v(t) == Vmax*(1-e^(-K*t)), list(Vmax=round(sprintDynamics$Vmax.fitted, digits=3), K=round(sprintDynamics$K.fitted, digits=3))))
        mtext(side = 1, line = 5, at = splitTimes[length(splitTimes)]*0.25, cex = 2 , substitute(v(t) == Vmax*(1-e^(-K*t)), list(Vmax="Vmax", K="K")))
        
        if(plotFittedAccel)
        {
                par(new = T)
                plot(time, sprintDynamics$a.fitted, type = "l", col = "green", yaxs= "i", xaxs = "i", xlab="", ylab = "",
                     ylim=c(0,sprintDynamics$amax.fitted), axes = FALSE )
                axis(line = 4, side = 4, col ="green", at = seq(0,max(sprintDynamics$a.fitted), by = 1))
        }
        
        #Force plotting
        if(plotFittedForce)
        {
                par(new=T)
                plot(time, sprintDynamics$f.fitted, type="l", col="blue", yaxs= "i", xaxs = "i", xlab="", ylab="",
                     ylim=c(0,sprintDynamics$fmax.fitted), axes = FALSE)
                axis(line = 2, side = 4, col ="blue", at = seq(0, sprintDynamics$fmax.fitted + 100, by = 100))

        }
        
        #Power plotting
        if(plotFittedPower)
        {
                par(new=T)
                plot(time, sprintDynamics$p.fitted, type="l", axes = FALSE, xlab="", ylab="", col="red", ylim=c(0,sprintDynamics$pmax.fitted + .1 * sprintDynamics$pmax.fitted), yaxs= "i", xaxs = "i")
                abline(v = sprintDynamics$tpmax.fitted, col="red", lty = 2)
                axis(side = 4, col ="red", at = seq(0, sprintDynamics$pmax.fitted, by = 200))
                axis(3, at = sprintDynamics$tpmax.fitted, labels = round(sprintDynamics$tpmax.fitted, 3))
                #text(sprintDynamics$tpmax.fitted, sprintDynamics$pmax.fitted, paste("Pmax fitted =", round(sprintDynamics$pmax.fitted, digits = 2)),  pos = 3)
                # mtext(side = 1, line = 5, at = splitTimes[length(splitTimes)]*0.75, cex = 1.5,
                #       substitute(P(t) == A*e^(-K*t)*(1-e^(-K*t)) + B*(1-e^(-K*t))^3,
                #                         list(A=round(sprintDynamics$Vmax.fitted^2*sprintDynamics$Mass, digits=3),
                #                              B = round(sprintDynamics$Vmax.fitted^3*sprintDynamics$Ka, digits = 3),
                #                              Vmax=round(sprintDynamics$Vmax.fitted, digits=3),
                #                              K=round(sprintDynamics$K.fitted, digits=3)))
                #       , col ="red")
        }
        
        legend (x = time[length(time)], y = sprintDynamics$pmax.fitted / 2,
                xjust = 1, yjust = 0.5, cex = 1,
                legend = c(paste("K =", round(sprintDynamics$K.fitted, digits = 2)),
                           paste("Vmax =", round(sprintDynamics$Vmax.fitted, digits = 2), "m/s"),
                           paste("Amax =", round(sprintDynamics$amax.fitted, digits = 2), "m/s²"),
                           paste("fmax =", round(sprintDynamics$fmax.rel.fitted, digits = 2), "N/Kg"),
                           paste("pmax =", round(sprintDynamics$pmax.rel.fitted, digits = 2), "W/Kg")),
                text.col = c("black", "black", "green", "blue", "red"))
        
        exportData = list(Mass = sprintDynamics$Mass,
                          Height = sprintDynamics$Height,
                          Temperature = sprintDynamics$Temperature,
                          Vw = sprintDynamics$Vw,
                          Ka = sprintDynamics$Ka,
                          K.fitted = sprintDynamics$K.fitted,
                          Vmax.fitted = sprintDynamics$Vmax,
                          amax.fitted = sprintDynamics$amax.fitted,
                          fmax.fitted = sprintDynamics$fmax.fitted,
                          fmax.rel.fitted = sprintDynamics$fmax.rel.fitted,
                          sfv.fitted = sprintDynamics$sfv.fitted,
                          sfv.rel.fitted = sprintDynamics$sfv.rel.fitted,
                          sfv.lm = sprintDynamics$sfv.lm,
                          sfv.rel.lm = sprintDynamics$sfv.rel.lm,
                          pmax.fitted = sprintDynamics$pmax.fitted,
                          pmax.rel.fitted = sprintDynamics$pmax.rel.fitted,
                          tpmax.fitted = sprintDynamics$tpmax.fitted,
                          F0 = sprintDynamics$F0,
                          F0.rel = sprintDynamics$F0.rel,
                          V0 = sprintDynamics$V0,
                          pmax.lm = sprintDynamics$pmax.lm,
                          pmax.rel.lm = sprintDynamics$pmax.rel.lm)
        write.csv2(exportData, file = paste(tempPath, "/sprintResults.csv", sep = ""))
        
}

testPhotocellsCJ <- function(positions, splitTimes, mass, personHeight, tempC)
{
	sprint = getSprintFromPhotocell(position = positions, splitTimes = splitTimes)
	sprintDynamics = getDynamicsFromSprint(K = sprint$K, Vmax = sprint$Vmax, mass, tempC, personHeight, maxTime = max(splitTimes))
	print(paste("K =",sprintDynamics$K.fitted, "Vmax =", sprintDynamics$Vmax.fitted))

	drawSprintFromPhotocells(sprintDynamics = sprintDynamics, splitTimes, positions, title = "Testing graph")
}

#----- execute code

prepareGraph(op$os, pngFile, op$graphWidth, op$graphHeight)
testPhotocellsCJ(op$positions, op$splitTimes, op$mass, op$personHeight, op$tempC)
endGraph()


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
#	sprintDynamics = getDynamicsFromSprint(K = sprint$K, Vmax = sprint$Vmax, 75, 25, 1.65)
#	print(paste("K =",sprintDynamics$K.fitted, "Vmax =", sprintDynamics$Vmax.fitted))
#	drawSprintFromPhotocells(sprintDynamics = sprintDynamics, splitTimes, positions, title = "Testing graph")
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
#	sprintDynamics = getDynamicsFromSprint(K = sprint$K, Vmax = sprint$Vmax, mass, tempC, personHeight, maxTime = max(splitTimes))
#	print(paste("K =",sprintDynamics$K.fitted, "Vmax =", sprintDynamics$Vmax.fitted))
#	drawSprintFromPhotocells(sprintDynamics = sprintDynamics, splitTimes, positions, title = "Testing graph")
#}
