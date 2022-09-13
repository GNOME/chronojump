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
#   Copyright (C) 2017-2019   	Xavier Padullés <x.padulles@gmail.com>
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



#Reads a .rad file, trims the header and the last line and finds the curve that best fits with the data of the file.
getSprintFromRadar <- function(radFile, decimalSeparator = ",")
{
        nlines = length(readLines(radFile))     # The number of lines of the file
        #Store the values of the file in the radar variable
        #Skips the 18 first lines and the las line
        radar = read.fwf(file=radFile, sep="", widths = c(7, 8, 8, 8, 9), dec = decimalSeparator, skip = 18, n = nlines - 19)
        radar = radar[1:(length(radar[,1]) -1 ),]  #Trim the last line
        radar[,2] = as.numeric(gsub(",", "\\.", radar[,2])) #Substitute the comas by dots in the second column
        radar = data.frame(t = na.omit(radar[,2]), v= na.omit(radar[,3]), position = na.omit(radar[,5]))
        radar$t = radar$t - radar$t[1]
        
        #Adjusting the measured data to the model
        model = nls( v ~ Vmax*(1-exp(-K*t)), radar, start=list(Vmax=10, K=1))
        Vmax = summary(model)$coeff[1,1]
        K = summary(model)$coeff[2,1]
        
        return(list(K = K, Vmax = Vmax))
        
}

# Reads all the .rad files in a folder and processes it geting the kinematics, dynamics, and plotting the graphs in pdf files
getRadarDynamicsFromFolder <- function(radDir, athletesFile, splitDistance, resultsFile = "results.csv", decimalSeparator =",")
{
        #model v(t) = Vmax*(1 - exp(-K*t))
        
        ro0 = 1.293                     #Air density at 1ATM
        Pb = 760                        #Preasure in mmHg. We assume the test is performed at normal conditions (1ATM)
        Cd = 0.9                        #Drag coefficient
        
        athletes = read.csv(file = athletesFile, sep = ";", dec = ",")
        colnames(athletes) = c("File", "Mass", "Temperature", "Height", "Vw")
        
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
                radar$t = radar$t - radar$t[1]
                
                options(warn = -1)
                #Adjusting the measured data to the model of getSprintFromRadar
                model = getSprintFromRadar(paste(radDir, "/", originalFiles[n], sep=""), decimalSeparator = decimalSeparator)
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
                results$pmax.rel.fitted[n] = dynamics$pmax.fitted / athletes$Mass[n]
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

#----- execute code

prepareGraph(op$os, pngFile, op$graphWidth, op$graphHeight)
#TODO: call radar function
#testPhotocellsCJ(op$positions, op$splitTimes, op$mass, op$personHeight, op$tempC)
endGraph()


#Examples of use

# getSprintFromRadar("~/Documentos/Radar/APL_post24.rad")
# getDynamicsFromSprint(K = 0.8184887, Vmax = 9.547099, Mass = 60, Temperature = 25, Height = 1.65 )
# getRadarDynamicsFromFolder(radDir = "~/ownCloud/Xavier/Recerca/Yoyo-Tests/Radar", athletesFile = "~/ownCloud/Xavier/Chronojump/Projectes/Sprint/athletes.csv", splitDistance = c(5,10,20))
