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
interpolateXAtY <- function(X, Y, desiredY, debug = FALSE)
{
    if(debug)
    {
        print("    In interpolate...")
        print("    X:")
        print(X)
        print("    Y:")
        print(Y)
     	print(paste("finding the X at desiredY:", desiredY))
    	#print(paste("max(Y):", max(Y)))
    }
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
	#previousSample == 0 fails on RAW %FMAX=0, for this reason now 1 is the minimum value using that options
        
        print(paste("sample1:", previousSample, "X:", X[previousSample], "Y:", Y[previousSample]))
        print(paste("sample2:", nextSample, "X:", X[nextSample], "Y:", Y[nextSample]))
        if(Y[nextSample] == desiredY){
                correspondingX = X[nextSample]
        } else {
                correspondingX = X[previousSample] + (desiredY  - Y[previousSample]) * (X[nextSample] - X[previousSample]) / (Y[nextSample] - Y[previousSample])
        }
        # print(paste("CorrespondingX:", correspondingX))
        return(correspondingX)
}


#Calculate the area under a curve using the cross product of consecutive vectors with the origin of each vector in the first point
getAreaUnderCurve <- function(x, y)
{
        #Adding a starting point at (x[1], 0) and last point at (x[last], 0)
        x = c(x[1], x, x[length(x)])
        y = c(0,y, 0)
        totalArea = 0
        
        # print("Calculating Area")
        # print("X:")
        # print(x)
        # print("Y:")
        # print(y)
        
        # print(paste("V",1," = ", "(",x[1 + 1] - x[1],",", y[1 + 1] - y[1], ")", sep = ""))
        for(i in 2:(length(x) -1))
        {
                parallelogramArea = ((x[i + 1] - x[1])* (y[i] - y[1]) - (x[i] - x[1]) * (y[i+1] - y[1]))
                #print(paste("V",i," = ", "(",x[i + 1] - x[1],",", y[i + 1] - y[1], ") = " , parallelogramArea / 2, sep = ""))

                totalArea = totalArea + parallelogramArea/2     #The area of the parallelograms are twice the triangles areas
        }
        #print(paste("toalArea:", totalArea))
        return(totalArea)
}

#Calculates the mean of a curve interval
getMeanValue <- function(X, Y, startX, endX, debug = FALSE)
{

        #Calculating the value of Y corresponding at startX
        #print("Calculating the first Y value")
        startY = interpolateXAtY(X = Y, Y = X, desiredY = startX) #The order are changed because we are looking for the Y value instead of X value
        
        #Calculating the last value using the interpolation
        #print("Calculating the last Y value")
        endY = interpolateXAtY(X = Y, Y = X, desiredY = endX) #The order are changed because we are looking for the Y value instead of X value
        
        #trimming X to the values that are between startX and endX
        selectedRange = which(X > startX & X < endX)
        X = X[selectedRange]
        #Adding Xs of the limits
        X = c(startX, X, endX)
        
        #trimming Y to the values that are between startY and endY
        Y = Y[selectedRange]
        #Adding Ys of the limits
        Y = c(startY, Y, endY)
        if (debug){
            print("    In getMeanValue:")
            print(paste("    startX:", startX))
            print(paste("    endX:", endX))
            print(paste("    startY:", startY))
            print(paste("    endY:", endY))
            print(paste("Calculating mean in the X range of [", startX, ",", endX, "]"))
            print("x:")
            print(X)
            print("y:")
            print(Y)
        }
        
        # print("Xs in getMeanValue():")
        # print(X)
        # print("Ys in getMeanValue():")
        # print(Y)
        
        
        #calculating the area under the curve (integral)
        area = getAreaUnderCurve(X , Y)

        #The mean value is the area under the curve divided by the lenth in the X axis of the curve
        return(area / (last(X) - X[1]))
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

last <- function(input) {
    return( tail(input, n=1) )
}
