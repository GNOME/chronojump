#F = sqrt( (cos(angle) * (ForceSensor + Mass * accel))^2 +                   #Horizontal component
#          (sin(angle) * (ForceSensor + Mass * accel) + Mass * 9.81)^2)      #Vertical component

getDynamicsFromForceSensor <- function(file = "/home/xpadulles/.local/share/Chronojump/forceSensor/83/1_Xavier Padullés_2019-10-01_13-03-41.csv",
                                       totalMass = 75,
                                       angle = 0,
                                       smooth = 5,
                                       minDisplacement = 0.1
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
        
        repetitions = getRepetitions(dynamics[, "time"], dynamics[, "position"], dynamics[, "rawForce"], minDisplacement)
        
        plot(dynamics[, "time"]
             ,dynamics[, "position"]
             , type = "l", xlab = "Time", ylab = "Position"
             #,xlim = c(0, 8)
             #, ylim = c(0,0.04)
             #, axes = F
             )
        #plot(position2, type = "l")
        # lines(dynamics[, "time"], dynamics[, "position2"], col = "grey")
        
        # print(extremesSamples)
        points(dynamics[repetitions$extremesSamples, "time"],
               #extremesSamples,
               dynamics[repetitions$extremesSamples,"position"])
        text(dynamics[repetitions$extremesSamples, "time"], dynamics[repetitions$extremesSamples,"position"], pos =4
             , paste("(", round(dynamics[repetitions$extremesSamples, "time"], digits = 2), ", ", round(dynamics[repetitions$extremesSamples,"position"], digits = 2), ")", sep="")
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

getRepetitions <- function(time, position, force, minDisplacement){
        
        currentSample = 1
        startDebounceSample = 1                 #Sample of a possible start of the phase
        debouncing = FALSE                      #If an extreme is found we must check if it keeps increasing or decreasing for at least minDisplacement
        RFDs = NA
        meanSpeeds = NA
        
        #Detecting the first phase
        
        while(currentSample < length(position) -1){
                
                currentSample = currentSample +1
                if(     #Minimum
                        (position[currentSample] < position[currentSample -1] &  position[currentSample] < position[currentSample+1]) || 
                        #Maximum
                        (position[currentSample] > position[currentSample -1] &  position[currentSample] > position[currentSample + 1]))
                {
                        startDebounceSample = currentSample
                }
                if (position[currentSample] - position[startDebounceSample] > minDisplacement){
                        print("Starting in Concentric")
                        concentric = 1
                        break()
                } else if(position[currentSample] - position[startDebounceSample] < -minDisplacement){
                        print("Starting in Eccentric")
                        concentric = -1
                        break()
                }
        }
        
        print(paste("force:", force[currentSample]))
        extremesSamples = startDebounceSample
        #debouncing = T
        print(paste("extremeSamples:", extremesSamples))
        print("-------------------end of First phase-------------------")
        
        currentSample = currentSample +1
        
        while(currentSample < length(position) -1){
                
                #Checking if it is an extreme.
                if (concentric * position[currentSample +1] < concentric * position[currentSample]
                    #&& concentric * position[currentSample -1] < concentric * position[currentSample]
                    )
                {
                        lastRFD = (force[currentSample] - force[startDebounceSample]) / (time[currentSample] - time[startDebounceSample])
                        lastMeanSpeed = (position[currentSample] - position[startDebounceSample]) / (time[currentSample] - time[startDebounceSample])
                        
                        startDebounceSample = currentSample             #The next debounces will be checked from this sample
                        debouncing = TRUE                                  #Starting a new debounce process
                        print(paste("startDebounceSample:", startDebounceSample))
                        # print(paste("debouncin:", debouncing))
                        concentric = -concentric
                        
                        # print(paste(currentSample, "Concentric =", concentric))
                        # print(paste(position[currentSample],  position[currentSample +1]))
                        # print(paste("startDebounceSample:", startDebounceSample))
                }
                
                #if debouncing, check if the position is far enough from the last extreme
                if(debouncing && concentric * (position[currentSample] - position[startDebounceSample]) > minDisplacement){
                        print(paste("-----------minDisplacement detected at", currentSample))
                        print(paste("Extreme added at:", startDebounceSample))
                        #We can consider that the last extreme in an actual change of phase.
                        extremesSamples = c(extremesSamples, startDebounceSample)
                        RFDs = c(RFDs, lastRFD)
                        meanSpeeds = c(meanSpeeds, lastMeanSpeed)
                        #concentric = -concentric                        #The slope of the signal has changed
                        print(paste("Current phase is", concentric))
                        
                        #End of debouncing
                        debouncing = F
                }
                
                currentSample = currentSample +1
        }
        
        #Checking if the las sample is far enough from the last extreme
        currentSample = currentSample + 1
        if( abs(position[currentSample] - position[startDebounceSample]) > minDisplacement ) {
                extremesSamples = c(extremesSamples, currentSample)
        }
        return(list(
                extremesSamples = extremesSamples[1:length(extremesSamples)]
                , RFDs = RFDs[2:length(RFDs)]
                , meanSpeeds = meanSpeeds[2:length(meanSpeeds)]))
}

dynamics = getDynamicsFromForceSensor(file = "/home/xpadulles/.local/share/Chronojump/forceSensor/83/1_Xavier Padullés_2019-10-02_15-31-40.csv",
                                      smooth = 10, totalMass = 0, angle = 0, minDisplacement = 0.5)