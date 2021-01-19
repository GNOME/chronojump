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
#   Copyright (C) 2016-2018  	Xavier Padullés <x.padulles@gmail.com>
# 

#Returns the number of sessions analysed
pfvProfileGetNumberSession <- function(analyzeTable)
{
        return(length(unique(analyzeTable$date)))
}

#Returns the table of force and velocity of each repetitions of a session
pfvProfileGetSessionFV <- function(analyzeTable, sessionNum)
{
        print("In pfvProfileGetSessionFV()")
        print(paste("SessionNum: ", sessionNum))
        f = analyzeTable$meanForce[which(analyzeTable$date == unique(analyzeTable$date)[sessionNum])]
        v = analyzeTable$meanSpeed[which(analyzeTable$date == unique(analyzeTable$date)[sessionNum])]
        return(list(f = f, v = v))
}

#Returns the F0 and V0 of a session
pfvProfileGetSessionProfile <- function(force, speed)
{
        profile = lm(force ~ speed)
        v0 = -coef(profile)[[1]] / coef(profile)[[2]]
        f0 = coef(profile)[[1]]
        return(list(v0 = v0, f0 = f0))
}

#Returns the profile F0 and V0 of each session
pfvProfileGetAnalysisProfiles <- function(analyzeTable)
{
        print("In pfvProfileGetAnalysisProfiles()")
        print(analyzeTable)
        sessionNumber = length(unique(analyzeTable$date))
        dates = unique(analyzeTable$date)
        session = 1
        #Get the force and velocity of each repetition in the first session
        sessionFV = pfvProfileGetSessionFV(analyzeTable, session)
        #Get the profile of the first session
        sessionProfile = pfvProfileGetSessionProfile(sessionFV$f, sessionFV$v)
        f0 = sessionProfile$f0
        v0 = sessionProfile$v0
        if (sessionNumber > 1){
                for (session in 2:sessionNumber)
                {
                        #Get the force and velocity of each repetition in the current session
                        sessionFV = pfvProfileGetSessionFV(analyzeTable, session)
                        #Get the profile of the current session
                        sessionProfile = pfvProfileGetSessionProfile(sessionFV$f, sessionFV$v)
                        f0 = c(f0, sessionProfile$f0)
                        v0 = c(v0, sessionProfile$v0)
                }
        }
        #print("F0's:")
        #print(f0)
        #print("V0's:")
        #print(v0)
        return(list(f0 = f0, v0 = v0, dates = dates))
}

#In the plane V0-F0, each point corresponds to a profile.
#Each profile has unique Pmax. Each isopotential line corresponds to the points that have the same Pmax
pfvProfileDrawProfilesEvolution <- function(profiles)
{
        
        #Clean the data and discard the wrong profiles
        correctProfiles = which(profiles$f0 > 0 & profiles$v0 > 0)
        f0 = profiles$f0[correctProfiles]
        v0 = profiles$v0[correctProfiles]
        dates = profiles$dates[correctProfiles]
        
        print("profiles:")
        print(profiles)
        
        if(min(v0) <= 0 || min(f0) <= 0){
		plot(0,0,type="n",axes=F,xlab="",ylab="")
		text(x=0,y=0,translateToPrint("Some of the F-V profiles is wrong.\nProbably the F0 or V0 is negative (A)"),cex=1.5)
		dev.off()
		quit()
	}
        pmax = f0*v0/4
        
        flimits = c(min(f0) - (max(f0) - min(f0))*0.1, max(f0) + (max(f0) - min(f0))*0.1)
        vlimits = c(min(v0) - (max(v0) - min(v0))*0.1, max(v0) + (max(v0) - min(v0))*0.1)
        #flimits = c(min(f0)*0.9, max(f0)*1.1)
	#vlimits = c(min(v0)*0.9, max(v0)*1.1)

	if(is.nan(flimits[2]) || is.infinite(flimits[2]) || is.nan(vlimits[2]) || is.infinite(vlimits[2]))
	{
		plot(0,0,type="n",axes=F,xlab="",ylab="")
		text(x=0,y=0,translateToPrint("Some of the F-V profiles is wrong.\nProbably the F0 or V0 is negative (B)"),cex=1.5)
		dev.off()
		quit()
	}


        #To plot the isopotentials we need to cerate the surface of the P(f,v) function
        #Creating equidistant values for force and velocity axes
        #f = seq(flimits[1], flimits[2], by = (flimits[2] - flimits[1]) / 100)
        #v = seq(vlimits[1], vlimits[2], by = (vlimits[2] - vlimits[1]) / 100)
        f = seq(0, flimits[2], by = flimits[2] / 100)
        v = seq(0, vlimits[2], by = vlimits[2] / 100)
        
        #Create a grid with all the combinations of the force and velocity values
        fv <- expand.grid(f,v)
        
        #Calculate the Pmax of each force and velocity combination
        p <- fv[,1]*fv[,2]/4
        power = matrix(p, ncol=101)
        
        #Plot the isopotential lines
        par(mar=c(5,4,4,5))
        contour(x = v, y = f, z = power,
                main = "Pmax(F0,V0) evolution", xlab = "V0 (m/s)", ylab = "F0 (N)",
                xlim = vlimits, ylim = flimits, vfont = NULL,
                lty = 2, nlevels = 30, method = "edge")
        
        #Plot the evolution of the session profile
        arrows(v0[1:(length(v0) -1)], f0[1:(length(v0) -1)], v0[2:length(v0)], f0[2:length(v0)], length = 0.15, col = "red")
        points(v0, f0)
        
        #Detecting if is there are close points
        minYDistance = (max(f0) - min(f0)) / 20
        minXDistance = ((max(v0) - min(v0)) / 20)
        
        #Each point(profile) must be compared to the other points
        for (currentProfile in 1:length(f0))
        {
                paintCurrentDate = TRUE
                text(v0[currentProfile], f0[currentProfile], labels = currentProfile, pos = 2, adj = 1, cex = 0.75)
                for(comparingProfile in 1:length(f0))
                {
                        if(abs(f0[comparingProfile] - f0[currentProfile]) < minYDistance &    #Not far enough vertically
                           abs(v0[comparingProfile] - v0[currentProfile]) < minXDistance &    #Not far enaugh horizontally
                           currentProfile != comparingProfile)                                #We are not comparing the same point
                        {
                                paintCurrentDate = FALSE
                                break()
                        }
                }
                if(paintCurrentDate) #No other profiles overlaps with the current one
                {
                        #Date of the session
                        text(v0[currentProfile], f0[currentProfile], labels = dates[currentProfile], pos = 1, offset = 0.5, cex = 0.75)
                        #Pmax of the session
                        text(v0[currentProfile], f0[currentProfile], labels = paste(round(pmax[currentProfile], digits = 0), "W", sep=""), pos = 3, offset = 0.5, cex = 0.75)
                }
        }
        mtext("Maximum mean power using the F-V profile of each session", side = 4, line = 2)
        if(length(profiles$f0) != length(f0)) #If there are discarded dates
        {
                mtext("Dates discarded due an incorrect profile:", side = 3, line = 1.5, col = "red", at = vlimits[1], adj = 0)
                mtext(paste(profiles$dates[-correctProfiles], collapse = ", "), side = 3, line = 0.5, col = "red", at = vlimits[1], adj = 0)
        }
}

pfvProfileExecute <- function(analyzeTable)
{
	print("analyzeTable")
        #Removing the rows that not contain date
        analyzeTable = analyzeTable[which(analyzeTable$date != ""), ]
        print(analyzeTable)
        
        profiles = pfvProfileGetAnalysisProfiles(analyzeTable)
        pfvProfileDrawProfilesEvolution(profiles)
}
