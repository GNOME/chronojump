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

#-------------- assign options -------------
op <- assignOptions(options)
#print(op$positions)



#Returns the K and Vmax parameters of the sprint using a number of pairs (time, position)
getSprintFromPhotocell <- function(positions, splitTimes, noise=0)
{
	#noise is for testing purpouses.
	# Checking that time and positions have the same length
        if(length(splitTimes) != length(positions)){
                print("Positions and splitTimes have diferent lengths")
                return()
        }
        
        # For the correct calculation we need at least 2 values in the position and time
        if(length(positions) < 2){
                print("Not enough data")
                return()
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

drawSprintFromPhotocells <- function(sprintDynamics, splitTimes, positions, title, plotFittedSpeed = T, plotFittedAccel = T, plotFittedForce = T, plotFittedPower = T)
{
        
        maxTime = splitTimes[length(splitTimes)]
        time = seq(0, maxTime, by=0.01)
        #Calculating measured average speeds
        avg.speeds = diff(positions)/diff(splitTimes)
        textXPos = splitTimes[1:length(splitTimes) - 1] + diff(splitTimes)/2
        
        # Plotting average speed
        barplot(height = avg.speeds, width = diff(splitTimes), space = 0, ylim = c(0, max(c(avg.speeds, sprintDynamics$Vmax) + 1)), main=title, xlab="Time(s)", ylab="Velocity(m/s)", axes = FALSE, yaxs= "i", xaxs = "i")
        text(textXPos, avg.speeds, round(avg.speeds, digits = 2), pos = 3)
        
        # Fitted speed plotting
        par(new=T)
        plot(time, sprintDynamics$v.fitted, type = "l", xlab="", ylab = "",  ylim = c(0, max(c(avg.speeds, sprintDynamics$Vmax) + 1)), yaxs= "i", xaxs = "i") # Fitted data
        text(4, sprintDynamics$Vmax.fitted/2, substitute(v(t) == Vmax*(1-e^(-K*t)), list(Vmax=round(sprintDynamics$Vmax.fitted, digits=3), K=round(sprintDynamics$K.fitted, digits=3))), pos=4, cex=2)
        
        if(plotFittedAccel)
        {
                par(new = T)
                plot(time, sprintDynamics$a.fitted, type = "l", col = "green", yaxs= "i", xaxs = "i", xlab="", ylab = "", axes = FALSE )
        }
        
        #Force plotting
        if(plotFittedForce)
        {
                par(new=T)
                plot(time, sprintDynamics$f.fitted, type="l", axes = FALSE, xlab="", ylab="", col="blue", ylim=c(0,sprintDynamics$fmax.fitted), yaxs= "i", xaxs = "i")

        }
        
        #Power plotting
        if(plotFittedPower)
        {
                par(new=T)
                plot(time, sprintDynamics$p.fitted, type="l", axes = FALSE, xlab="", ylab="", col="red", ylim=c(0,sprintDynamics$pmax.fitted), yaxs= "i", xaxs = "i")
                abline(v = sprintDynamics$tpmax.fitted, col="red")
                axis(4, col="red")
                mtext(4, text="Power(W/Kg)", col="red")
                text(3, 250, substitute(P(t) == A*e^(-K*t)*(1-e^(-K*t)) + B*(1-e^(-K*t))^3, 
                                        list(A=round(sprintDynamics$Vmax.fitted^2*sprintDynamics$Mass, digits=3), 
                                             B = round(sprintDynamics$Vmax.fitted^3*sprintDynamics$Ka, digits = 3),
                                             Vmax=round(sprintDynamics$Vmax.fitted, digits=3),
                                             K=round(sprintDynamics$K.fitted, digits=3))),
                     pos=4, cex=1, col ="red")
        }
        
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
