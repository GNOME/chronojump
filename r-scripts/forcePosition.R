#F = sqrt( (cos(angle) * (ForceSensor + Mass * accel))^2 +                   #Horizontal component
#          (sin(angle) * (ForceSensor + Mass * accel) + Mass * 9.81)^2)      #Vertical component

getDynamicsFromForceSensor <- function(file = "/home/xpadulles/.local/share/Chronojump/forceSensor/83/1_Xavier Padullés_2019-10-01_13-03-41.csv",
                                       totalMass = 75,
                                       angle = 0,
                                       smooth = 5,
                                       conMinDisplacement = 0.1, eccMinDisplacement = 0.1
)
{
        forceSensor = read.csv(file, sep =";", dec = ",", header = TRUE)
        colnames(forceSensor) = c("time", "rawForce")
        forceSensor$time = forceSensor$time / 1E6                               #Converting microseconds to seconds
        
        position = forceSensor$rawForce/71.97                                   #71.93 N/m measured in the black rubber
        position2 = filter(position, rep(1/smooth, smooth, sides = 2))          #Moving average
        
        speed = NA
        for(i in 2:(length(forceSensor$time) -1)){
                speed = c(speed, (position2[i + 1] - position2[i - 1]) / (forceSensor$time[i+1] - forceSensor$time[i -1]) )
        }
        
        speed = c(speed, speed[length(speed)])
        speed2 = filter(speed, rep(1/smooth, smooth, sides = 2))
        
        
        accel = NA
        for(i in 2:(length(forceSensor$time) -1)){
                accel = c(accel, (speed2[i + 1] - speed2[i - 1]) / (forceSensor$time[i+1] - forceSensor$time[i -1]) )
        }
        
        accel = c(accel, accel[length(accel)])
        accel2 = filter(accel, rep(1/smooth, smooth, sides = 2))
        
        resultantForce = sqrt( (cos(angle*pi/180) * (forceSensor$rawForce + totalMass * accel2))^2 +                   #Horizontal component
                                       (sin(angle*pi/180) * (forceSensor$rawForce + totalMass * accel2) + totalMass * 9.81)^2)      #Vertical component
        power = speed2 * (forceSensor$rawForce + totalMass * accel2) +               #Power associated to the acceleration of the mass
                speed2 * (sin(angle*pi/180) * totalMass * 9.81)                    #Power associated to the gravitatory field
        
        dynamics = matrix(c(forceSensor$time, forceSensor$rawForce, position, position2, speed, speed2, accel, accel2, resultantForce, power), ncol = 10)
        colnames(dynamics) = c("time", "rawForce", "position", "position2", "speed", "speed2", "accel", "accel2", "resultantForce", "power")
        
        #Trimming rows with NA
        dynamics = dynamics[(which(!is.na(dynamics[,"accel2"]))), ]
        
        # par(mfrow = c(1,1))
        # # 
        # #par(new = TRUE)
        # plot(forceSensor$time, speed, type = "l", col = "green", axes = F, xlab ="", ylab = "") 
        # #axis(side = 4, col = "green")
        # lines(forceSensor$time, speed2, col = "green2")
        # 
        # 
        # par(new = TRUE)
        # plot(accel, type = "l", col = "pink", main = "", xlab = "", ylab = "", axes = F)
        # lines(accel2, col = "magenta")
        # par(new = T)
        # 
        # plot(forceSensor$time, forceSensor$rawForce, col = "grey", type = "l"
        #      #,ylim =c(min(c(na.omit(forceSensor$rawForce), na.omit(resultantForce))), max(c(na.omit(forceSensor$rawForce), na.omit(resultantForce))))
        #      #,axes = F
        #      , ylab = "", xlab = ""
        #      ,xlim = c(0, 8)
        #      )
        # lines(forceSensor$time, resultantForce, col = "blue")
        # par(new = T)
        # 
        # plot(forceSensor$time, power, col = "red", type = "l",
        #      axes = F, ylab = "", xlab = "")
        # #axis(side = 4)
        # par(new = T)
        
        #Getting the basic information of each repetition
        repetitions = getRepetitions(dynamics[, "time"], dynamics[, "position"], dynamics[, "rawForce"], conMinDisplacement, eccMinDisplacement)
        
        plot(#dynamics[, "time"]
                dynamics[, "position"]
                , type = "l", xlab = "Time", ylab = "Position"
                #,xlim = c(50, 150)
                #, ylim = c(0.25,1.1)
                #, axes = F
        )
        
        # plot(position2, type = "l")
        # lines(dynamics[, "time"], dynamics[, "position2"], col = "grey")
        
        points(#dynamics[repetitions$extremesSamples, "time"],
                repetitions$extremesSamples,
                dynamics[repetitions$extremesSamples,"position"])
        text(repetitions$extremesSamples, dynamics[repetitions$extremesSamples,"position"], pos =4
             #, paste("(", round(dynamics[repetitions$extremesSamples, "time"], digits = 4), ", ", round(dynamics[repetitions$extremesSamples,"position"], digits = 4), ")", sep="")
             , paste("(",repetitions$extremesSamples, ", ", round(dynamics[repetitions$extremesSamples,"position"], digits = 2), ")", sep=""), cex = 0.66
        )
        
        return(list(
                time=dynamics[,"time"],
                rawForce=dynamics[,"rawForce"],
                position=dynamics[,"position"],
                position2=dynamics[,"position2"],
                speed=dynamics[,"speed"],
                speed2=dynamics[,"speed2"],
                accel=dynamics[,"accel"],
                accel2=dynamics[,"accel2"],
                resultantForce=dynamics[,"resultantForce"],
                power=dynamics[,"power"],
                # minSamples = extremes$minSamples,
                # maxSamples = extremes$maxSamples,
                extremesSamples = repetitions$extremesSamples
                ,RFDs = repetitions$RFDs
                ,meanSpeeds = repetitions$meanSpeeds
        )
        )
}

getRepetitions <- function(time, position, force, conMinDisplacement, eccMinDisplacement){
        
        #The comments supposes that the current phase is concentric. In the case that the phase is eccentric
        #the signal is inverted by multiplying it by -1.
        extremesSamples = 1
        
        #for each phase, stores the sample number of the biggest current sample.
        possibleExtremeSample = 1
        
        #Stores the sample of the last actual maximum of the phase
        lastExtremeSample = 1
        
        currentSample = 2
        
        #mean RFD of each phase
        RFDs = NA
        
        #mean speed of each phase
        meanSpeeds = NA
        
        #The firstPhase is treated different
        firstPhase = TRUE

        #Detecting the first phase type
        if(position[currentSample] > position[possibleExtremeSample])
        {
                concentric = 1
                minDisplacement = eccMinDisplacement
        
        } else {
                concentric = -1
                minDisplacement = conMinDisplacement}
        
        #print(paste("starting in mode:", concentric) )

        while(currentSample < length(position) -1){
                
                #Checking if the current position is greater than the previous possilble maximum
                if(concentric * position[currentSample] > concentric * position[possibleExtremeSample])
                        {
                        #The current sample is the new candidate to be a maximum
                        #print(paste("updated possibleExtremeSample to:", currentSample, "position:", position[currentSample]))
                        possibleExtremeSample = currentSample
                }

                #Checking if the current position is at minDisplacement below the last possible extreme
                if(concentric * position[currentSample] - concentric * position[possibleExtremeSample] < - minDisplacement
                   #For the first phase the minDisplacement is considered much smaller in order to detect an extreme in small oscillations
                   || (firstPhase
                       && (concentric * position[currentSample] - concentric * position[possibleExtremeSample] < - minDisplacement / 10) )
                   )
                        {
                        
                        if(firstPhase) {firstPhase = !firstPhase}               #End of the first phase special treatment
                        
                        # print(paste("-----------minDisplacement detected at", currentSample))
                        # print(paste("Extreme added at:", possibleExtremeSample))
                        
                        #We can consider that the last extreme in an actual change of phase.
                        extremesSamples = c(extremesSamples, possibleExtremeSample)
                        
                        #Save the sample of the last extrme in order to compare new samples with it
                        lastExtremeSample = possibleExtremeSample
                        
                        #Changing the phase from concentric to eccentril or viceversa
                        concentric = -concentric
                        if (concentric == 1){
                                minDisplacement = eccMinDisplacement
                        } else {
                                minDisplacement = conMinDisplacement
                        }
                        
                        #Calculate mean RFD and mean speed of the phase
                        lastRFD = (force[currentSample] - force[lastExtremeSample]) / (time[currentSample] - time[lastExtremeSample])
                        lastMeanSpeed = (position[currentSample] - position[lastExtremeSample]) / (time[currentSample] - time[lastExtremeSample])
                        RFDs = c(RFDs, lastRFD)
                        meanSpeeds = c(meanSpeeds, lastMeanSpeed)
                }

                currentSample = currentSample +1
        }

        return(list(
                extremesSamples = c(extremesSamples[2:length(extremesSamples)], possibleExtremeSample)
                , RFDs = RFDs[2:length(RFDs)]
                , meanSpeeds = meanSpeeds[2:length(meanSpeeds)]))
}

testDir = "/home/xpadulles/Descargas/Piscina-mati/separat-per-punticoma/"
allFiles = dir(testDir)

for(i in 1:5)
{
        dynamics = getDynamicsFromForceSensor(file = paste(testDir, allFiles[i], sep ="")
                                              ,smooth = 10, totalMass = 0, angle = 0, conMinDisplacement = 0.5, eccMinDisplacement = 0.1)
}
