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

#This code uses splitTimes: accumulated time (not lap time)

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

#Reads a .rad file, trims the header and the last line and finds the curve that best fits with the data of the file.
getSprintFromRadar <- function(radFile)
{
        nlines = length(readLines(radFile))     # The number of lines of the file
        #Store the values of the file in the radar variable
        #Skips the 18 first lines and the las line
        radar = read.fwf(file=radFile, sep="", widths = c(7, 8, 8, 8, 9), dec = ",", skip = 18, n = nlines - 19)
        radar = radar[1:(length(radar[,1]) -1 ),]  #Trim the last line
        radar[,2] = as.numeric(gsub(",", "\\.", radar[,2])) #Substitute the comas by dots in the second column
        radar = data.frame(t = na.omit(radar[,2]), v= na.omit(radar[,3]), position = na.omit(radar[,5]))
        
        #Adjusting the measured data to the model
        model = nls( v ~ Vmax*(1-exp(-K*t)), radar, start=list(Vmax=10, K=1))
        Vmax = summary(model)$coeff[1,1]
        K = summary(model)$coeff[2,1]
        
        return(list(K = K, Vmax = Vmax))
        
}

#Calculates all kinematic and dynamic variables using the sprint parameters (K and Vmax) and the conditions of the test (Mass and Height of the subject,
#Temperature in the moment of the test and Velocity of the wind).
getDynamicsFromSprint <- function(K, Vmax, Mass, Temperature = 25, Height , Vw = 0, maxTime = 10)
{
	# maxTime is used for the numerical calculations
        # Constants for the air friction modeling
        ro0 = 1.293
        Pb = 760
        Cd = 0.9
        ro = ro0*(Pb/760)*273/(273 + Temperature)
        Af = 0.2025*(Height^0.725)*(Mass^0.425)*0.266 # Model of the frontal area
        Ka = 0.5*ro*Af*Cd
        

        # Calculating the kinematic and dynamic variables from the fitted model.
        amax.fitted = Vmax * K
        fmax.fitted = Vmax * K * Mass + Ka*(Vmax - Vw)^2        # Exponential model. Considering the wind, the maximum force is developed at v=0
        fmax.rel.fitted = fmax.fitted / Mass
        sfv.fitted = -fmax.fitted / Vmax                        # Mean slope of the force velocity curve using the predicted values in the range of v=[0:Vmax]
        sfv.rel.fitted = sfv.fitted / Mass

        # Getting values from the exponential model. Used for numerical calculations
        time = seq(0,maxTime, by = 0.01)      
        v.fitted=Vmax*(1-exp(-K*time))
        a.fitted = Vmax*K*(1 - v.fitted/Vmax)
        f.fitted = Vmax*Mass*K*(1 - v.fitted/Vmax) + Ka*(v.fitted - Vw)^2
        power.fitted = f.fitted * v.fitted
        pmax.fitted = max(power.fitted)                 #TODO: Make an interpolation between the two closest points
        pmax.rel.fitted = pmax.fitted / Mass
        tpmax.fitted = time[which.max(power.fitted)]
        
        #Modeling F-v with the wind friction.
        # a(v) = Vmax*K*(1 - v/Vmax)
        # F(v) = a(v)*mass + Faero(v)
        # Faero(v) = Ka*(V - Va)²
        # Ka = 0.5 * ro * Af * Cd
        
        fvModel = lm(f.fitted ~ v.fitted)              # Linear regression of the fitted values
        F0 = fvModel$coefficients[1]                 # The same as fmax.fitted. F0 is the interception of the linear regression with the vertical axis
        F0.rel = F0 / Mass
        sfv.lm = fvModel$coefficients[2]             # Slope of the linear regression
        sfv.rel.lm = sfv.lm / Mass
        V0 = -F0/fvModel$coefficients[2]             # Similar to Vmax.fitted. V0 is the interception of the linear regression with the horizontal axis
        pmax.lm = V0 * F0/4                          # Max Power Using the linear regression. The maximum is found in the middle of the parabole p(v)
        pmax.rel.lm = pmax.lm / Mass
        
        return(list(Mass = Mass, Height = Height, Temperature = Temperature, Vw = Vw, Ka = Ka, K.fitted = K, Vmax.fitted = Vmax,
                    amax.fitted = amax.fitted, fmax.fitted = fmax.fitted, fmax.rel.fitted = fmax.rel.fitted, sfv.fitted = sfv.fitted, sfv.rel.fitted = sfv.rel.fitted,
                    pmax.fitted = pmax.fitted, pmax.rel.fitted = pmax.rel.fitted, tpmax.fitted = tpmax.fitted, F0 = F0, F0.rel = F0.rel, V0 = V0,
                    sfv.lm = sfv.lm, sfv.rel.lm = sfv.rel.lm, pmax.lm = pmax.lm, pmax.rel.lm = pmax.rel.lm, v.fitted = v.fitted, a.fitted = a.fitted, f.fitted = f.fitted, p.fitted = power.fitted ))
}

#Finds the time correspondig to a given position in the formula x(t) = Vmax*(t + (1/K)*exp(-K*t)) -Vmax - 1/K
#Uses the iterative Newton's method of the tangent aproximation
splitTime <- function(Vmax, K, position, tolerance = 0.001, initTime = 1)
{
        #Trying to find the solution of Position(time) = f(time)
        #We have to find the time where y = 0.
        y = Vmax*(initTime + (1/K)*exp(-K*initTime)) -Vmax/K - position
        t = initTime
        while (abs(y) > tolerance){
                v = Vmax*(1 - exp(-K*t))
                t = t - y / v
                y = Vmax*(t + (1/K)*exp(-K*t)) -Vmax/K - position
        }
        return(t)
}

# Reads all the .rad files in a folder and processes it geting the kinematics, dynamics, and plotting the graphs in pdf files
getRadarDynamicsFromFolder <- function(radDir, athletesFile, splitDistance, resultsFile = "results.csv", decimalSeparator =",")
{
        #model v(t) = Vmax*(1 - exp(-K*t))
        
        ro0 = 1.293                     #Air density at 1ATM
        Pb = 760                        #Preasure in mmHg. We assume the test is performed at normal conditions (1ATM)
        Cd = 0.9                        #Drag coefficient
        
        athletes = read.csv(file = athletesFile, sep = ";", dec = ",")
        
        #Looking for all .rad files
        originalFiles = list.files(path=radDir, pattern="*.rad")
        nFiles = length(originalFiles)
        
        #Naming the columns of the split times corresponding to the splitDistance values.  For each distance theres is the predicted(fitted) and raw time
        tcolnameFitted = NULL
        tcolnameRaw = NULL
        for (i in 1:length(splitDistance)){
                tcolnameFitted = c(tcolnameFitted, paste("t", splitDistance[i], "mFitted", sep=""))
                tcolnameRaw  = c(tcolnameRaw, paste("t", splitDistance[i], "mRaw", sep=""))
                
        }
        
        #20 variables plus the 2*splittimes (raw and predicted)
        results = matrix(rep(NA, nFiles*(24 + 2*length(splitDistance))), ncol=(24 + 2*length(splitDistance)))
        
        colnames(results)=c("fileName", "Mass", "Height", "Temperature", "Vw", "Vmax.fitted", "K.fitted", "amax.fitted", "fmax.fitted", "fmax.rel.fitted", "sfv.fitted", "sfv.rel.fitted",
                            "pmax.fitted", "pmax.rel.fitted", "tpmax.fitted", "F0", "F0.rel", "V0", "sfv.lm", "sfv.rel.lm", "pmax.lm", "pmax.rel.lm",
                            "rsquared", "pvalue", tcolnameFitted, tcolnameRaw)
        results = as.data.frame(results)
        results$fileName = originalFiles
        results$Mass = athletes$Mass
        results$Temperature = athletes$Temperature
        results$Height = athletes$Heigh
        results$Vw = athletes$Vw
        
        print(results$fileName)
        dir.create("graphs", showWarnings = FALSE)
        
        vel.global = NULL               # vel.global contais all the measured velocities of all the tests
        v.fitted.global = NULL          # vel.fitted.global contais all the predicted velocities of all the tests
        
        for(n in 1:nFiles){
                
                #Calculing the wind friction parameters
                ro = ro0*(Pb/760)*273/(273 + athletes$Temperature[n])                   # Air density at the given preasure ant temperature
                Af = 0.2025*(athletes$Height[n]^0.725)*(athletes$Mass[n]^0.425)*0.266   #Frontal area of the athlete
                Ka = 0.5*ro*Af*Cd                                                       #Aerodinamic friction coefficient
                
                print(paste("Reading: ",radDir, "/", originalFiles[n], sep=""))
                
                nlines = length(readLines(paste(radDir, "/", originalFiles[n], sep="")))    # The number of .rad files in the dir
                
                # Stalker radar use fixed width column format
                radar = read.fwf(file=paste(radDir, "/", originalFiles[n], sep=""), widths = c(7, 8, 8, 8, 9), #The withds of each column
                                 dec = decimalSeparator,            # decimal separator depends on the stalker configuration
                                 skip = 18,                         #The first 18 lines are metadata of the test
                                 n = nlines - 19)                   #The last line contains "END OF FILE" So it is discarded
                radar = data.frame(t = na.omit(radar[,2]), v= na.omit(radar[,3]), position = na.omit(radar[,5]))        #Discarding NA values
                
                options(warn = -1)
                #Adjusting the measured data to the model of getSprintFromRadar
                model = getSprintFromRadar(paste(radDir, "/", originalFiles[n], sep=""))
                options(warn = 0)
                K = model$K
                Vmax = model$Vmax
                
                #Storing the calculated parameters to the results dataset. Vmax and K defines completely the curve
                results$Vmax.fitted[n] = Vmax
                results$K.fitted[n] = K
                
                dynamics = getDynamicsFromSprint(K = K, Vmax = Vmax, Mass = athletes$Mass[n], Temperature = athletes$Temperature[n], Height = athletes$Height[n], Vw = athletes$Vw[n], maxTime = radar$t[length(radar$t)])
                
                results$amax.fitted[n] = dynamics$amax.fitted
                results$fmax.fitted[n] = dynamics$fmax.fitted
                results$fmax.rel.fitted[n] = dynamics$fmax.rel.fitted
                results$sfv.fitted[n] = dynamics$sfv.fitted
                results$sfv.rel.fitted[n] = dynamics$sfv.rel.fitted
                results$F0[n] = as.numeric(dynamics$F0)
                results$F0.rel[n] = dynamics$F0.rel
                results$V0[n] = as.numeric(dynamics$V0)
                results$sfv.lm[n] = dynamics$sfv.lm
                results$sfv.rel.lm[n] = dynamics$sfv.rel.lm
                results$pmax.fitted[n] = dynamics$pmax.fitted
                results$pmax.rel.fitted[n] = dynamics$pmax.fitted
                results$tpmax.fitted[n] = dynamics$tpmax.fitted
                results$pmax.lm[n] = dynamics$pmax.lm
                results$pmax.rel.lm[n] = dynamics$pmax.rel.lm
                
                #v.fitted and f.fitted and power.fitted are used for getting all the predicted values and plotting it
                v.fitted=Vmax*(1-exp(-K*radar$t))
                f.fitted = Vmax*athletes$Mass[n]*K*(1 - v.fitted/Vmax) + Ka*(v.fitted - athletes$Vw[n])^2
                power.fitted = f.fitted * v.fitted
                
                # Comparing the predicted vs the measured values
                prediction = lm( v.fitted ~ radar$v)
                results$rsquared[n] = summary(prediction)$r.squared
                results$pvalue[n] = anova(prediction)$`Pr(>F)`[1]
                
                # Calculing all split times
                for (split in 1:length(splitDistance)){
                        # With the fitted exponential model
                        results[n, 24 + split] = splitTime(Vmax, K, position = splitDistance[split])
                        # With the raw data
                        results[n, (24 + length(splitDistance) + split)] = radar$t[which(abs(radar$position - splitDistance[split]) == min(abs(radar$position - splitDistance[split])))[1]]
                }
                
                
                # In vel.global is stored all the measured speeds of all tests. It is used to see how good are the predictions
                vel.global = c(vel.global, radar$v)
                
                # In vel.global is stored all the predicted speeds of all tests. It is used to see how good are the predictions
                v.fitted.global = c(v.fitted.global, v.fitted)

                ########## Plotting all graphs ###########
 
                # Create the directory. If it exists it isn't overwritten.
                dir.create(paste(radDir,"/graphs", sep=""), showWarnings=F)
                
                # Plotting the raw data v(t)
                pdf(paste(radDir,"/graphs/", results$fileName[n], "-v-F-p-t", ".pdf", sep=""), width = 16, height = 8)
                plot(radar$t, radar$v, main=originalFiles[n], xlab="Time(s)", ylab="Velocity(m/s)") # Raw data
                lines(radar$t, v.fitted, col = "green")
                text(4, Vmax*0.8, substitute(v(t) == Vmax*(1-e^-(K*t)), list(Vmax=round(Vmax, digits=3), K=round(K, digits=3))), pos=4, cex=2, col="green")
                #Force drawing
                par(new=T)
                plot(radar$t, f.fitted, type="l", axes = FALSE, xlab="", ylab="", col="grey", ylim=c(0,results$amax.fitted[n]))
                
                #Plotting power(t)
                par(new=T)
                plot(radar$t,power.fitted, type="l", axes = FALSE, xlab="", ylab="", col="red", ylim=c(0,results$pmax.fitted[n]))
                abline(v = results$tpmax.fitted[n], col="red")
                axis(4, col="red")
                mtext(4, text="Power(W/Kg)", col="red")
                text(results$tpmax[n], results$pmax.fitted[n]*0.3, substitute(P(t) == A*e^(-K*t)*(1-e^(-K*t)) + B*(1-e^(-K*t))^3, list(A=round(Vmax^2*athletes$Mass[n], digits=3), B = round(Vmax^3*Ka, digits = 3), Vmax=round(Vmax, digits=3), K=round(K, digits=3))), pos=4, cex=2, col ="red")
                dev.off()
                
                #Plotting F(v)
                pdf(paste(radDir,"/graphs/", results$fileName[n], "-F-v.pdf", sep=""), width = 16, height = 8)
                plot(v.fitted, f.fitted, xlab = "Velocity[m/s]", xlim=c(0,results$V0[n]), ylim=c(0,results$F0[n]), ylab = "Force[N]",
                     xaxs = "i", yaxs = "i", main = paste(results$fileName[n]), sub = paste ("F0 =", round(results$F0[n], digits = 2), "   V0 =", round(results$V0[n], digits = 2))) 
                abline(results$F0[n], results$sfv.lm[n], col = "red")   # Linear regression plot
                
                # fvModel = lm(f.fitted ~ v.fitted)
                # F0 = fvModel$coefficients[1]            # The same as fmax.fitted. F0 is the interception of the linear regression with the vertical axis
                # sfv.lm = fvModel$coefficients[2]
                # V0 = -F0/fvModel$coefficients[2]        # The same as Vmax.fitted. V0 is the interception of the linear regression with the horizontal axis
                # abline(F0, sfv.lm, col = "green")
                dev.off()
                
                # Plotting the comparation predicted vs measured
                pdf(paste(radDir,"/graphs/", results$fileName[n], "-fitting.pdf", sep=""), width = 16, height = 8)
                plot(radar$v, v.fitted, main=paste("Fit of",results$fileName[n], results$repetition[n], sep=""))
                abline( a = summary(prediction)$coeff[1,1], b = summary(prediction)$coeff[2,1], col="red")
                dev.off()
        }
        
        #Write al the calculus in a csv table
        write.table(results, file=resultsFile, sep=";", dec=",", col.names = NA)
        
        # Comparing the global predicted vs measured values
        prediction = lm( v.fitted.global ~ vel.global - 1)
        print(summary(prediction))
        print(anova(prediction))
        
        #Plotting the global predicted vs measured values
        pdf(paste(radDir,"/graphs/", "global-prediction", ".pdf", sep=""), width = 10, height = 10)
        plot(vel.global, v.fitted.global, main = "All Measures vs Predictions", pch=3, xlim=c(0,11), ylim=c(0,11), xlab="Measured velocity", ylab="Predicted velocity")
        abline( a = 0, summary(prediction)$coeff, col="red")
        text(6,2, paste("p = ", anova(prediction)$`Pr(>F)`[1]))
        text(6,1, paste("R² = ", summary(prediction)$r.squared))
        dev.off()
        
        return(results)
}

drawSprintFromPhotocells <- function(sprintDynamics, splitTimes, positions, title, plotFittedSpeed = T, plotFittedAccel = T, plotFittedForce = T, plotFittedPower = T)
{
        
        maxTime = splitTimes[length(splitTimes)]
        time = seq(0, maxTime, by=0.01)
        #Calculating measured average speeds
        avg.speeds = diff(positions)/diff(splitTimes)
        textXPos = splitTimes[1:length(splitTimes) - 1] + diff(splitTimes)/2
        
        # Plotting average speed
        pdf("/tmp/photocellsSprintGraph.pdf", width = 16, height = 8)
        barplot(height = avg.speeds, width = diff(splitTimes), space = 0, ylim = c(0, max(c(avg.speeds, sprintDynamics$Vmax) + 1)), main=title, xlab="Time(s)", ylab="Velocity(m/s)", axes = FALSE, yaxs= "i", xaxs = "i")
        text(textXPos, avg.speeds, round(avg.speeds, digits = 2), pos = 3)
        
        # Fitted speed plotting
        par(new=T)
	print(time)
	print(sprintDynamics$v.fitted)
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
                print(paste("Power =",sprintDynamics$power.fitted))
                abline(v = sprintDynamics$tpmax.fitted, col="red")
                axis(4, col="red")
                mtext(4, text="Power(W/Kg)", col="red")
                text(3, 250, substitute(P(t) == A*e^(-K*t)*(1-e^(-K*t)) + B*(1-e^(-K*t))^3, 
                                        list(A=round(sprintDynamics$Vmax.fitted^2*sprintDynamics$Mass, digits=3), 
                                             B = round(sprintDynamics$Vmax.fitted^3*sprintDynamics$Ka, digits = 3),
                                             Vmax=round(sprintDynamics$Vmax.fitted, digits=3),
                                             K=round(sprintDynamics$K.fitted, digits=3))),
                     pos=4, cex=1, col ="red")
                dev.off()
        }
        
}

testPhotocellsCJ <- function(positions, splitTimes, mass, personHeight, tempC)
{
	sprint = getSprintFromPhotocell(position = positions, splitTimes = splitTimes)
	sprintDynamics = getDynamicsFromSprint(K = sprint$K, Vmax = sprint$Vmax, mass, tempC, personHeight, maxTime = max(splitTimes))
	print(paste("K =",sprintDynamics$K.fitted, "Vmax =", sprintDynamics$Vmax.fitted))
	drawSprintFromPhotocells(sprintDynamics = sprintDynamics, splitTimes, positions, title = "Testing graph")
}


args <- commandArgs(TRUE)
optionsFile <- args[1]
options <- scan(optionsFile, comment.char="#", what=character(), sep="\n")
assignOptions <- function(options) {
	return(list(
		    positions  	= as.numeric(unlist(strsplit(options[1], "\\;"))),
		    splitTimes 	= as.numeric(unlist(strsplit(options[2], "\\;"))),
		    mass 	= as.numeric(options[3]),
		    personHeight = as.numeric(options[4]),
		    tempC 	= as.numeric(options[5])
		    ))
}

op <- assignOptions(options)
#print(op$positions)
testPhotocellsCJ(op$positions, op$splitTimes, op$mass, op$personHeight, op$tempC)


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


# getSprintFromRadar("~/Documentos/Radar/APL_post24.rad")
# getDynamicsFromSprint(K = 0.8184887, Vmax = 9.547099, Mass = 60, Temperature = 25, Height = 1.65 )
# getRadarDynamicsFromFolder(radDir = "~/ownCloud/Xavier/Recerca/Yoyo-Tests/Radar", athletesFile = "~/ownCloud/Xavier/Chronojump/Projectes/Sprint/athletes.csv", splitDistance = c(5,10,20))
