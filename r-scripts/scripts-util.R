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

fixTitleAndOtherStrings <- function(str)
{
	print(c("1 fixTitle=", str))

	#this tryCatch is to fix some problems with asian characters on a windows machine (maybe caused by unupdated R)
	#for latin accents, ... tryCatch was not needed
	tryCatch ({
			#this works nice with latin accents
			#unicoded titles arrive here like this "\\", convert to "\", as this is difficult, do like this:
			#http://stackoverflow.com/a/17787736
			str=parse(text = paste0("'", str, "'"))
			print(c("2 fixTitle=", str))
		},
		error= function(e)
		{
			print("error on fixTitleAndOtherStrings:")
			print(message(e))
			return(str)
		})

	#convert str to spaces
	str=gsub('_',' ', str)
	str=gsub('-','    ', str)

	return (str)
}

fixDatetime <- function(str)
{
	str=gsub('_',' ', str)
	str=gsub('-',':', str)
}

#Function to get the interpolated x at a given y
interpolateXAtY <- function(X, Y, desiredY){
    # print(paste("finding the X at desiredY:", desiredY))
        if(max(Y) < desiredY){
                # print("desiredY is greater than max(Y)")
                return(max(Y))
        }
                
        #find the closest sample
        nextSample = 1
        # print("Calculating the interpolation")
        # print(paste("desiredY:", desiredY))
        while (Y[nextSample] < desiredY){
                nextSample = nextSample +1
        }
        
        previousSample = nextSample - 1
        
        if(Y[nextSample] == desiredY){
                correspondingX = X[nextSample]
        } else {
                correspondingX = X[previousSample] + (desiredY  - Y[previousSample]) * (X[nextSample] - X[previousSample]) / (Y[nextSample] - Y[previousSample])
        }
        # print(paste("CorrespondingX:", correspondingX))
        return(correspondingX)
}


#Calculate the area under a curve using the cross product of consecutive vectors with the origin in the first point
getAreaUnderCurve <- function(x, y)
{
        print("Calculating Area")
        # print("X:")
        # print(x)
        # print("Y:")
        # print(y)
        x = c(x, x[length(x)], x[1])
        y = c(y, 0, 0)
        totalArea = 0
        # print(paste("V",1," = ", "(",x[1 + 1] - x[1],",", y[1 + 1] - y[1], ")", sep = ""))
        for(i in 2:(length(x) -1))
        {
                parallelogramArea = ((x[i + 1] - x[1])* (y[i] - y[1]) - (x[i] - x[1]) * (y[i+1] - y[1]))
                
                # print(paste("V",i," = ", "(",x[i + 1] - x[1],",", y[i + 1] - y[1], ")", sep = ""))
                # print(parallelogramArea/2)
                
                totalArea = totalArea + parallelogramArea
        }
        # print(paste("toalArea:", totalArea/2))
        return(totalArea/2) #The area of the parallelograms are twice the triangles areas
}

getAreaUnderCurve2 <- function(x, y)
{
        # print("Calculating Area")
        # print("X:")
        # print(x)
        # print("Y:")
        # print(y)

        totalArea = 0
        # print(paste("V",1," = ", "(",x[1 + 1] - x[1],",", y[1 + 1] - y[1], ")", sep = ""))
        for(i in 1:length(x))
        {
                barArea = y[i]*(x[i+1] - x[i])
                
                # print(paste("V",i," = ", "(",x[i + 1] - x[1],",", y[i + 1] - y[1], ")", sep = ""))
                # print(parallelogramArea/2)
                
                totalArea = totalArea + barArea
        }
        # print(paste("toalArea:", totalArea/2))
        return(totalArea/2) #The area of the parallelograms are twice the triangles areas
}

#Calculates the mean of a curve interval
getMeanValue <- function(X, Y, startX, endX)
{
        # print(paste("Calculating mean in the X range of [", startX, ",", endX, "]"))
        # print("x:")
        # print(X)
        # print("y:")
        # print(Y)
        
        #Calculating the value of Y corresponding at startX
        #print("Calculating the first Y value")
        startY = interpolateXAtY(X = Y, Y = X, desiredY = startX) #The order are changed because we are looking for the Y value instead of X value
        
        #Calculating the last value using the interpolation
        #print("Calculating the last Y value")
        endY = interpolateXAtY(X = Y, Y = X, desiredY = endX) #The order are changed because we are looking for the Y value instead of X value

        X = X[which(X > startX & X < endX)]
        X = c(startX, X, endX)
        
        Y = Y[which(X > startX & X < endX)]
        Y = c(startY, Y, endY)
        
        #calculating the area under the curve (integral)
        area = getAreaUnderCurve(X , Y)

        return(area / (X[length(X)] - X[1]))
}

prepareGraph <- function(os, pngFile, width, height)
{
        if(os == "Windows"){
                library("Cairo")
                Cairo(width, height, file = pngFile, type="png", bg="white")
        }
        else
                png(pngFile, width=width, height=height)
        #pdf(file = "/tmp/maxIsomForce.pdf", width=width, height=height)
}

#Ends the graph

endGraph <- function()
{
        dev.off()
}
