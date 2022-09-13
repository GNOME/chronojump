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
#   Copyright (C) 2017-2020     Xavier de Blas <xaviblas@gmail.com>

#F = sqrt( (cos(angle) * (ForceSensor + Mass * accel))^2 +                   #Horizontal component
#          (sin(angle) * (ForceSensor + Mass * accel) + Mass * 9.81)^2)      #Vertical component

getDynamicsFromForceSensor <- function(file = "/home/xpadulles/.local/share/Chronojump/forceSensor/83/1_Xavier Padullés_2019-10-01_13-03-41.csv",
                                       totalMass = 75,
				       stiffness = 71.93, 	#71.93 N/m measured in the black rubber
                                       angle = 0,
                                       smooth = 5,
                                       conMinDisplacement = 0.1, eccMinDisplacement = 0.1,
				       plotGraph = FALSE
)
{
        forceSensor = read.csv(file, sep =";", dec = ",", header = TRUE)
        colnames(forceSensor) = c("time", "rawForce")
        forceSensor$time = forceSensor$time / 1E6                               #Converting microseconds to seconds
        
        position = forceSensor$rawForce / stiffness
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

        if(plotGraph == TRUE){
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
        }
        
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

getRepetitions <- function(time, position, force, conMinDisplacement, eccMinDisplacement)
{
        # plot(#dynamics[, "time"]
        #         position
        #         , type = "l", xlab = "Time", ylab = "Position"
        #         #,xlim = c(50, 150)
        #         #, ylim = c(0.25,1.1)
        #         #, axes = F
        # )
        
        #The comments supposes that the current phase is concentric. In the case that the phase is eccentric
        #the signal is inverted by multiplying it by -1.
        extremesSamples_l = 1
        
        #for each phase, stores the sample number of the biggest current sample.
        possibleExtremeSample = 1
        
        lastExtremeSample = 1 	#Stores the sample of the last actual maximum of the phase
        currentSample = 2
        
        RFDs = NA 		#mean RFD of each phase
        meanSpeeds = NA 	#mean speed of each phase

	# to find if there is a previous extreme than first one with minDisplacement
	searchingFirstExtreme = TRUE
	minimumPosBeforeFirstExtreme = 1
	maximumPosBeforeFirstExtreme = 1
	minimumValueBeforeFirstExtreme = position[minimumPosBeforeFirstExtreme]
	maximumValueBeforeFirstExtreme = position[maximumPosBeforeFirstExtreme]

        #Detecting the first phase type
        if(position[currentSample] > position[possibleExtremeSample])
        {
                concentric = 1
                minDisplacement = eccMinDisplacement #minDisplacement is referred to the next phase
        } else {
                concentric = -1
                minDisplacement = conMinDisplacement
	}

        while(currentSample < length(position) -1)
	{
		if(searchingFirstExtreme)
		{
			if(position[currentSample] > maximumValueBeforeFirstExtreme)
			{
				maximumValueBeforeFirstExtreme = position[currentSample]
				maximumPosBeforeFirstExtreme = currentSample
			}
			if(position[currentSample] < minimumValueBeforeFirstExtreme)
			{
				minimumValueBeforeFirstExtreme = position[currentSample]
				minimumPosBeforeFirstExtreme = currentSample
			}
		}
                
                #Checking if the current position is greater than the previous possible maximum
                if(concentric * position[currentSample] > concentric * position[possibleExtremeSample])
		{
                        #The current sample is the new candidate to be a maximum
                        #print(paste("updated possibleExtremeSample to:", currentSample, "position:", position[currentSample]))
                        possibleExtremeSample = currentSample
			# if(concentric == 1)
			# 	points(x=possibleExtremeSample, y=-0.1, col="green")
			# else
			# 	points(x=possibleExtremeSample, y=-0.2, col="red")
                }

                #Checking if the current position is at minDisplacement below the last possible extreme
		#we check if distance from currentSample to possibleExtremeSample is greater than minimum, in order to accept possibleExtremeSample
                if(concentric * position[currentSample] - concentric * position[possibleExtremeSample] < - minDisplacement)
		{
			#possibleExtremeSample is now the new extreme

			#firstExtreme will find if there is a previous extreme with minDisplacement
			if(searchingFirstExtreme)
			{
				samplePreFirst = minimumPosBeforeFirstExtreme
				if(concentric == -1)
					samplePreFirst = maximumPosBeforeFirstExtreme

				if( samplePreFirst < possibleExtremeSample && (
				   (concentric == 1 && position[possibleExtremeSample] - position[samplePreFirst] >= conMinDisplacement) ||
				   (concentric == -1 && position[samplePreFirst] - position[possibleExtremeSample] >= eccMinDisplacement) ) )
				{
					#points(x=samplePreFirst, y=position[samplePreFirst], col="blue", cex=8)
					extremesSamples_l = c(extremesSamples_l, samplePreFirst)
					#lastExtremeSample = possibleExtremeSample
					lastExtremeSample = samplePreFirst
				}
				searchingFirstExtreme = FALSE
			}
			#abline(v=currentSample, lty=2)
                        
                        # print(paste("-----------minDisplacement detected at", currentSample))
                        # print(paste("Extreme added at:", possibleExtremeSample))
                        
			#points(x=possibleExtremeSample, y=position[possibleExtremeSample], col="red", cex=4)

			#We can consider that the last extreme in an actual change of phase.
			extremesSamples_l = c(extremesSamples_l, possibleExtremeSample)

			#Calculate mean RFD and mean speed of the phase
			lastRFD = (force[possibleExtremeSample] - force[lastExtremeSample]) / (time[possibleExtremeSample] - time[lastExtremeSample])
			lastMeanSpeed = (position[possibleExtremeSample] - position[lastExtremeSample]) / (time[possibleExtremeSample] - time[lastExtremeSample])

			RFDs = c(RFDs, lastRFD)
			meanSpeeds = c(meanSpeeds, lastMeanSpeed)

			#Save the sample of the last extrme in order to compare new samples with it
			lastExtremeSample = possibleExtremeSample

			#Changing the phase from concentric to eccentric or viceversa
			concentric = -concentric

			if (concentric == 1){
                                minDisplacement = eccMinDisplacement
                        } else {
                                minDisplacement = conMinDisplacement
                        }
                }

                currentSample = currentSample +1
        }

	#note first value of extremesSamples_l is discarded (it is the first value of the samples vector)
        return(list(
                extremesSamples_l = c(extremesSamples_l[2:length(extremesSamples_l)], possibleExtremeSample) #TODO: remember to add his last extreme to the C# code
                , RFDs = RFDs[2:length(RFDs)]
                , meanSpeeds = meanSpeeds[2:length(meanSpeeds)]))
}

testPadu <- function(conMinDisplacement = .5, eccMinDisplacement = .5)
{
	testDir = "/home/xpadulles/Descargas/Piscina-mati/separat-per-punticoma/"
	allFiles = dir(testDir)

	for(i in 1:5)
	{
		dynamics = getDynamicsFromForceSensor(file = paste(testDir, allFiles[i], sep =""),
						      smooth = 10, totalMass = 0, stiffness = 71.93, angle = 0, conMinDisplacement, eccMinDisplacement)
	}
}
testXavi <- function(conMinDisplacement = 10, eccMinDisplacement = 10) #try with 10 or 1
{
	dynamics = getDynamicsFromForceSensor(file = "tests/forcePosition/36_prova2_2017-11-24_12-17-30.csv",
					      smooth = 10, totalMass = 0, stiffness = 60, angle = 0, conMinDisplacement, eccMinDisplacement)
}
testXavi2 <- function(conMinDisplacement = 1, eccMinDisplacement = 1)
{
	dynamics = getDynamicsFromForceSensor(file = "tests/forcePosition/36_prova2_2020-02-07_12-17-44.csv",
					      smooth = 10, totalMass = 0, stiffness = 60, angle = 0, conMinDisplacement, eccMinDisplacement)
}
testXavi2b <- function(conMinDisplacement = 1, eccMinDisplacement = 1)
{
	dynamics = getDynamicsFromForceSensor(file = "tests/forcePosition/36_prova2_2020-02-07_12-17-44_inici_con.csv",
					      smooth = 10, totalMass = 0, stiffness = 60, angle = 0, conMinDisplacement, eccMinDisplacement)
}
