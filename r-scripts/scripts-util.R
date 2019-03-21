#Function to get the interpolated x at a given y
interpolateXAtY <- function(X, Y, desiredY){
        if(max(Y) < desiredY){
                print("desiredY is greater than max(Y)")
                return(max(Y))
        }
                
        #find the closest sample
        nextSample = 1
        print("Calculating the interpolation")
        print(paste("desiredY:", desiredY))
        while (Y[nextSample] < desiredY){
                nextSample = nextSample +1
        }
        
        previousSample = nextSample - 1
        
        if(Y[nextSample] == desiredY){
                correspondingX = X[nextSample]
        } else {
                correspondingX = X[previousSample] + (desiredY  - Y[previousSample]) * (X[nextSample] - X[previousSample]) / (Y[nextSample] - Y[previousSample])
        }
        print(paste("CorrespondingX:", correspondingX))
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

#Calculates the mean of a curve interval
getMeanValue <- function(X, Y, startX, endX)
{
        print(paste("Calculating mean In the X range of [", startX, ",", endX, "]"))
        
        
        #Calculating the value of Y corresponding at startX
        print("Calculating the first Y value")
        startY = interpolateXAtY(X = Y, Y = X, desiredY = startX) #The order are changed because we are looking for the Y value instead of X value
        
        #Calculating the last value using the interpolation
        print("Calculating the last Y value")
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
