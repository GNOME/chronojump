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

source(paste(options[1], "/scripts-util.R", sep=""))

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
        a.fitted = Vmax*K*exp(-K*time)
        f.fitted = Mass*a.fitted + Ka*(v.fitted - Vw)^2
        p.fitted = f.fitted * v.fitted
        
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
        pmax.fitted = fmax.fitted * Vmax / 4         # Obtained from the function P(v) = F(v) * v. It is a parabele. The apex is in Vmax/2
        pmax.rel.fitted = pmax.fitted / Mass
        tpmax.fitted = log(2) / K                    # Obtained from P'(t) = 0
        
        return(list(Mass = Mass,
                    Height = Height, Temperature = Temperature,
                    Vw = Vw,
                    Ka = Ka,
                    t.fitted = time,
                    K.fitted = K,
                    Vmax.fitted = Vmax,
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
                    p.fitted = p.fitted ))
}

exportSprintDynamics <- function(sprintDynamics)
{
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


