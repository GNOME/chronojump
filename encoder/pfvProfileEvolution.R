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
        return(length(levels(analyzeTable$date)))
}

#Returns the table of force and velocity of each repetitions of a session
pfvProfileGetSessionFV <- function(analyzeTable, sessionNum)
{
        f = analyzeTable$meanForce[which(analyzeTable$date == levels(analyzeTable$date)[sessionNum])]
        v = analyzeTable$meanSpeed[which(analyzeTable$date == levels(analyzeTable$date)[sessionNum])]
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
        sessionNumber = length(levels(analyzeTable$date))
        dates = levels(analyzeTable$date)
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
pfvProfileDrawProfilesEvolution <- function(analyzetable)
{
        profiles = pfvProfileGetAnalysisProfiles(analyzeTable)
        
        f0 = profiles$f0
        v0 = profiles$v0
        pmax = f0*v0/4
        dates = profiles$dates
        
        flimits = c(min(f0) - (max(f0) - min(f0))*0.1, max(f0) + (max(f0) - min(f0))*0.1)
        vlimits = c(min(v0) - (max(v0) - min(v0))*0.1, max(v0) + (max(v0) - min(v0))*0.1)
        #flimits = c(min(f0)*0.9, max(f0)*1.1)
        #vlimits = c(min(v0)*0.9, max(v0)*1.1)
        
        
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
                xlim = vlimits, ylim = flimits,
                lty = 2, nlevels = 30, method = "edge")
        
        #Plot the evolution of the session profile
        arrows(v0[1:4], f0[1:4], v0[2:5], f0[2:5], length = 0.15, col = "red")
        points(v0, f0)
        
        #Date of the session
        text(v0, f0, labels = dates, pos = 1, offset = 0.5, cex = 0.75)
        #Pmax of the session
        text(v0, f0, labels = paste(round(pmax, digits = 0), "W", sep=""), pos = 3, offset = 0.5, cex = 0.75)
        mtext("Maximum mean power using the F-V profile of each session", side = 4, line = -2)
}

pfvProfileExecute <- function(inputFile,graphMode)
{
        analyzeTable = read.csv(inputFile, dec = ".", sep = ",")
        colnames(analyzeTable)[2] = "date"
        pfvProfileDrawProfilesEvolution(analyzeTable)
}

pfvProfileExecute("/tmp/chronojump-last-encoder-analyze-table.txt")
