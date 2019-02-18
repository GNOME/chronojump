#Function to get the interpolated x at a given y
interpolateXAtY <- function(X, Y, desiredY){
        if(max(Y) < desiredY){
                print("desiredY is greater than max(Y)")
                return(max(Y))
        }
                
        #find the closest sample
        nextSample = 1
        print("X:")
        print(X)
        print("Y:")
        print(Y)
        print("desiredY:")
        print(desiredY)
        while (Y[nextSample] < desiredY){
                nextSample = nextSample +1
        }
        
        previousSample = nextSample - 1
        
        if(Y[nextSample] == desiredY){
                desiredX = X[nextSample]
        } else {
                desiredX = X[previousSample] + (desiredY  - Y[previousSample]) * (X[nextSample] - X[previousSample]) / (Y[nextSample] - Y[previousSample])
        }
        return(desiredX)
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
