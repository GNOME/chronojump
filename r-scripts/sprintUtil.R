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
#   Copyright (C) 2017-2022   	Xavier de Blas <xaviblas@gmail.com>

#This code uses splitTimes: accumulated time (not lap time)

source(paste(options[1], "/scripts-util.R", sep=""))

#Calculates all kinematic and dynamic variables using the sprint parameters (K and Vmax) and the conditions of the test (Mass and Height of the subject,
#Temperature in the moment of the test and Velocity of the wind).
getDynamicsFromSprint <- function(K, Vmax, Mass, T0 = 0, Temperature = 25, Height , Vw = 0, maxTime = 10)
{
        print("In getDynamicsFromSprint()")
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
	#print(paste("T0:", T0))
	if(maxTime + T0 <= 0) #check to not fail the following seq by
		return (NULL)

        time = seq(0,maxTime + T0, by = 0.01)      
        v.fitted=Vmax*(1-exp(-K*time))
        a.fitted = Vmax*K*exp(-K*time)
        f.fitted = Mass*a.fitted + Ka*(v.fitted - Vw)^2
        p.fitted = f.fitted * v.fitted
        
        #Modeling F-v with the wind friction.
        # a(v) = Vmax*K*(1 - v/Vmax)
        # F(v) = a(v)*mass + Faero(v)
        # Faero(v) = Ka*(V - Va)²
        # Ka = 0.5 * ro * Af * Cd

	modelSuccess = TRUE
	tryCatch ({
	        fvModel = lm(f.fitted ~ v.fitted)              # Linear regression of the fitted values
		}, error=function(cond)
                {
			modelSuccess <<- FALSE #need <<- global assignment operator to ensure its assigned
                })
	if(! modelSuccess)
		return (NULL)

        F0 = fvModel$coefficients[1]                 # The same as fmax.fitted. F0 is the interception of the linear regression with the vertical axis
        F0.rel = F0 / Mass
        sfv.lm = fvModel$coefficients[2]             # Slope of the linear regression
        sfv.rel.lm = sfv.lm / Mass
        V0 = -F0/fvModel$coefficients[2]             # Similar to Vmax.fitted. V0 is the interception of the linear regression with the horizontal axis
        pmax.lm = V0 * F0/4                          # Max Power Using the linear regression. The maximum is found in the middle of the parabole p(v)
        pmax.rel.lm = pmax.lm / Mass
        pmax.fitted = max(p.fitted)
        pmax.rel.fitted = pmax.fitted / Mass
        tpmax.fitted = time[which(p.fitted == pmax.fitted)]
        vpmax.fitted = Vmax * ( 1 - exp( -K * tpmax.fitted))
        
        return(list(Mass = Mass,
                    Height = Height, Temperature = Temperature,
                    Vw = Vw,
                    Ka = Ka,
                    t.fitted = time,
                    K.fitted = K,
                    Vmax.fitted = Vmax,
                    T0.fitted = T0,
                    amax.fitted = amax.fitted,
                    fmax.fitted = fmax.fitted,
                    fmax.rel.fitted = fmax.rel.fitted,
                    sfv.fitted = sfv.fitted,
                    sfv.rel.fitted = sfv.rel.fitted,
                    pmax.fitted = pmax.fitted,
                    pmax.rel.fitted = pmax.rel.fitted,
                    tpmax.fitted = tpmax.fitted,
                    F0 = F0,
                    F0.rel = F0.rel,
                    V0 = V0,
                    sfv.lm = sfv.lm,
                    sfv.rel.lm = sfv.rel.lm,
                    pmax.lm = pmax.lm,
                    pmax.rel.lm = pmax.rel.lm,
                    v.fitted = v.fitted,
                    a.fitted = a.fitted,
                    f.fitted = f.fitted,
                    p.fitted = p.fitted,
                    vpmax.fitted = vpmax.fitted))
}

getSplitsForPrepareRow <- function (splitTime, splitMeanSpeed, splitMeanForce, splitMeanPower,
		splitPosition, splitPositionAll, decimalIsComma)
{
	splits = NULL

	#print("exportSprintDynamicsPrepareRow splitPositionAll:")
	#print(splitPositionAll)

	nameVector = NULL
	timeVector = "m_Time" #this is at a given time
	magnitudeVector = c("m_Speed", "m_Force", "m_Power") #this are from a split to the next (avg)
	#sprintPhotocells does not send right now splitMeanSpeed, splitMeanForce, splitMeanPower

	if(is.null(splitPositionAll))
	{
		#print(paste("splitTime:",splitTime))
		#print(paste("splitPosition:",splitPosition))

		if (is.null (splitMeanSpeed))
			splits = as.list(splitTime)
		else {
			for(i in 1:length(splitTime))
			{
				splits = c(splits, splitTime[i])
				splits = c(splits, splitMeanSpeed[i])
				splits = c(splits, splitMeanForce[i])
				splits = c(splits, splitMeanPower[i])
			}
		}

		oldSplitPosition = 0
		for (i in 1:length(splitPosition))
		{
			#use "to" and not "-" because R would convert the "-" to "."
			dist = paste(oldSplitPosition, "to", splitPosition[i], sep="")
			if(decimalIsComma)
			{
				nameVector = c(nameVector, gsub("\\.",",",paste(splitPosition[i], timeVector, sep="")))
				if (! is.null (splitMeanSpeed))
					nameVector = c(nameVector, gsub("\\.",",",paste(dist, magnitudeVector, sep="")))
			} else {
				nameVector = c(nameVector, paste(splitPosition[i], timeVector, sep=""))
				if (! is.null (splitMeanSpeed))
					nameVector = c(nameVector, paste(dist, magnitudeVector, sep=""))
			}
			oldSplitPosition = splitPosition[i]
		}
	} else
	{
		splitTimeSFPVector = NULL #TimeSpeedForcePower
		for(i in 1:length(splitPositionAll))
		{
			pos = match(splitPositionAll[i], splitPosition)
			if(is.na(pos)) # current set to export has not this position
			{
				if (is.null (splitMeanSpeed))
					splitTimeSFPVector = c(splitTimeSFPVector, NA) #create an NA "value" for time
				else
					splitTimeSFPVector = c(splitTimeSFPVector, NA, NA, NA, NA) #create an NA "value" for time, speed, force, power
			} else {
				if (is.null (splitMeanSpeed))
					splitTimeSFPVector = c(splitTimeSFPVector, splitTime[pos])
				else {
					splitTimeSFPVector = c(splitTimeSFPVector, splitTime[pos])
					splitTimeSFPVector = c(splitTimeSFPVector, splitMeanSpeed[pos])
					splitTimeSFPVector = c(splitTimeSFPVector, splitMeanForce[pos])
					splitTimeSFPVector = c(splitTimeSFPVector, splitMeanPower[pos])
				}
			}
		}

		splits = as.list(splitTimeSFPVector)

		oldSplitPosition = 0
		for (i in 1:length(splitPositionAll))
		{
			#use "to" and not "-" because R would convert the "-" to "."
			dist = paste(oldSplitPosition, "to", splitPositionAll[i], sep="")
			if(decimalIsComma)
			{
				nameVector = c(nameVector, gsub("\\.",",",paste(splitPositionAll[i], timeVector, sep="")))
				if (! is.null (splitMeanSpeed))
					nameVector = c(nameVector, gsub("\\.",",",paste(dist, magnitudeVector, sep="")))
			} else {
				nameVector = c(nameVector, paste(splitPositionAll[i], timeVector, sep=""))
				if (! is.null (splitMeanSpeed))
					nameVector = c(nameVector, paste(dist, magnitudeVector, sep=""))
			}
			oldSplitPosition = splitPositionAll[i]
		}
	}

	#debug
	#print ("splits:"); print (splits)
	#print ("nameVector:"); print (nameVector)

	names(splits) = nameVector

	return (splits)
}

#data is an sprint and has been modelized
exportSprintDynamicsPrepareRow <- function(sprintFittedDynamics, sprintRawDynamics, splitTime, splitPosition, splitPositionAll, decimalIsComma)
{
	print ("at exportSprintDynamicsPrepareRow decimalIsComma:")
	print (decimalIsComma)
	splits <- getSplitsForPrepareRow (splitTime,
			sprintRawDynamics$meanSpeed, sprintRawDynamics$meanForce, sprintRawDynamics$meanPower,
			splitPosition, splitPositionAll, decimalIsComma)

	#print("exportSprintDynamicsPrepareRow names(splits):")
	#print(names(splits))

	#on Race analyzer we send raw data, but on photocells we are not sending raw data here (right now).
	rawVmax = 0
	rawAmax = 0
	rawFmax = 0
	rawPmax = 0
	if (! is.null(sprintRawDynamics))
	{
		   rawVmax = sprintRawDynamics$rawVmax
		   rawAmax = sprintRawDynamics$rawAmax
		   rawFmax = sprintRawDynamics$rawFmax
		   rawPmax = sprintRawDynamics$rawPmax
	}

        row = c(list(Mass = sprintFittedDynamics$Mass,
                   Height = sprintFittedDynamics$Height,
                   Temperature = sprintFittedDynamics$Temperature,
                   V.wind = sprintFittedDynamics$Vw,
                   Ka = sprintFittedDynamics$Ka,
                   K.fitted = sprintFittedDynamics$K.fitted,
                   Vmax.fitted = sprintFittedDynamics$Vmax,
                   amax.fitted = sprintFittedDynamics$amax.fitted,
                   fmax.fitted = sprintFittedDynamics$fmax.fitted,
                   fmax.rel.fitted = sprintFittedDynamics$fmax.rel.fitted,
                   sfv.fitted = sprintFittedDynamics$sfv.fitted,
                   sfv.rel.fitted = sprintFittedDynamics$sfv.rel.fitted,
                   sfv.lm = sprintFittedDynamics$sfv.lm,
                   sfv.rel.lm = sprintFittedDynamics$sfv.rel.lm,
                   pmax.fitted = sprintFittedDynamics$pmax.fitted,
                   pmax.rel.fitted = sprintFittedDynamics$pmax.rel.fitted,
                   tpmax.fitted = sprintFittedDynamics$tpmax.fitted,
                   vpmax.fitted = sprintFittedDynamics$vpmax.fitted,
                   F0 = sprintFittedDynamics$F0,
                   F0.rel = sprintFittedDynamics$F0.rel,
                   V0 = sprintFittedDynamics$V0,
                   pmax.lm = sprintFittedDynamics$pmax.lm,
                   pmax.rel.lm = sprintFittedDynamics$pmax.rel.lm,
		   raw.Vmax = rawVmax,
		   raw.Amax = rawAmax,
		   raw.Fmax = rawFmax,
		   raw.Pmax = rawPmax
		   ), splits)
        print(row)

        return(row)
}

#data is not sprint or model has failed
exportSprintRawPrepareRow <- function (sprintRawDynamics, splitPositionAll, decimalIsComma)
{
	splits <- getSplitsForPrepareRow (sprintRawDynamics$splitTime,
			sprintRawDynamics$meanSpeed, sprintRawDynamics$meanForce, sprintRawDynamics$meanPower,
			sprintRawDynamics$splitPosition, splitPositionAll, decimalIsComma)

        row = c(list(Mass = sprintRawDynamics$Mass,
                   Height = sprintRawDynamics$height,
                   Temperature = sprintRawDynamics$temperature,
                   V.wind = sprintRawDynamics$Vw,
                   Ka = 0,
                   K.fitted = 0,
                   Vmax.fitted = 0,
                   amax.fitted = 0,
                   fmax.fitted = 0,
                   fmax.rel.fitted = 0,
                   sfv.fitted = 0,
                   sfv.rel.fitted = 0,
                   sfv.lm = 0,
                   sfv.rel.lm = 0,
                   pmax.fitted = 0,
                   pmax.rel.fitted = 0,
                   tpmax.fitted = 0,
                   vpmax.fitted = 0,
                   F0 = 0,
                   F0.rel = 0,
                   V0 = 0,
                   pmax.lm = 0,
                   pmax.rel.lm = 0,
		   raw.Vmax = sprintRawDynamics$rawVmax,
		   raw.Amax = sprintRawDynamics$rawAmax,
		   raw.Fmax = sprintRawDynamics$rawFmax,
		   raw.Pmax = sprintRawDynamics$rawPmax
		   ), splits)
        print(row)

        return(row)
}

exportSprintDynamicsWriteRow <- function(exportRow)
{
        write.csv2(exportRow, file = paste(tempPath, "/sprintResults.csv", sep = ""), row.names = FALSE)
}

#This is only used on sprintRadar
#Finds the time correspondig to a given position in the formula x(t) = Vmax*(t + (1/K)*exp(-K*t)) -Vmax - 1/K
#Uses the iterative Newton's method of the tangent aproximation
splitTime <- function(Vmax, K, position, tolerance = 0.001, initTime = 1)
{
        #Trying to find the solution of Position(time) = f(time)
        #We have to find the time where y = 0.
        t = initTime
        y = Vmax*(t + (1/K)*exp(-K*t)) -Vmax/K - position
        while (abs(y) > tolerance){
                v = Vmax*(1 - exp(-K*t))
                t = t - y / v
                y = Vmax*(t + (1/K)*exp(-K*t)) -Vmax/K - position
        }
        return(t)
}

drawFVProfile <- function(F0, V0, pngFile)
{
        #png(pngFile)
        par(mar = c(5.1, 4.1, 4.1, 5.1))
        v = seq(0, V0, by = 0.01)
        p = (F0 -F0*v/V0)*v
        plot(c(V0, 0),c(0, F0), type = "l", axes = F, xlab = "Speed [m/s]", ylab = "Force [N]")
        axis(1, pos = 0)
        axis(2, pos = 0)
        par(new = T)
        plot(v, p, axes = F, type = "l", col = "red", ylab = "", xlab = "")
        axis(4, pos = V0)
        mtext(text = "Power [W]", side = 4, line = 2, col = "red")
        #dev.off()
}
