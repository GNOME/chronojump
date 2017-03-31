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
        pmax.fitted = max(p.fitted)                 #TODO: Make an interpolation between the two closest points
        pmax.rel.fitted = pmax.fitted / Mass
        tpmax.fitted = time[which.max(p.fitted)]
        
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
                    sfv.lm = sfv.lm, sfv.rel.lm = sfv.rel.lm, pmax.lm = pmax.lm, pmax.rel.lm = pmax.rel.lm, v.fitted = v.fitted, a.fitted = a.fitted, f.fitted = f.fitted, p.fitted = p.fitted ))
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
        while (abs(y) > tolerance){
                derivY = (x2/x1)*(t1 - exp(-K*t1)*(K*t1 + 1)/K^2 + 1/K^2) + exp(-K*t2)*(K*t2 +1) - 1/K^2
                K = K - y / derivY
                y = (x2/x1)*( t1 + exp(-K*t1)/K - 1/K) - t2 - exp(-K*t2)/K + 1/K
        }
        #Calculing Vmax substituting the K found in the eq. (1)
        Vmax = x1/(t1 + exp(-K*t1)/K -1/K)
        return(list(K = K, Vmax = Vmax))
}


prepareGraph <- function(os, pngFile, width, height)
{
	if(os == "Windows")
		Cairo(width, height, file = pngFile, type="png", bg="white")
	else
		png(pngFile, width=width, height=height)
}

endGraph <- function()
{
	dev.off()
}

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
