setwd("~/chronojump/encoder/tests/")
source("../util.R")

#Read a double vector indicating the initial diameter of every loop of the rope
#plus the final diameter of the last loop and returns a dataframe with the radius
#correspending to the total number of ticks of the encoder
#This can be run only once per machine

# Example of input of the sequence of the loop and diameter of the loop
# We use diameters but in the next step we convert to radii
# d_vector <- c(1.5, 1.5, 1.5, 1.5, 2, 2.5, 2.7, 2.9, 2.95, 3)
getInertialDiametersPerTick <- function(d_vector)
{
  print("Diameter in getInertialDiametersPerTick before conversion")
  print(d_vector)
  #If only one diameter is returned, we assume that the diameter is constant
  #and only a double is returned
  if (length(d_vector) == 1){
    return(d_vector)
  }
  
  # Numerating the loops of the rope
  d <- matrix(c(seq(from=0, to=(length(d_vector) -1), by=1), d_vector), ncol=2)
  
  # Converting the number of the loop to ticks of the encoder
  d[,1] <- d[,1]*200
  
  # Adding an extra point at the begining of the diameters matrix to match better the first point
  x1 <- d[1,1]
  y1 <- d[1,2]
  x2 <- d[2,1]
  y2 <- d[2,2]
  lambda <- 200
  x0 <- x1 - lambda
  y0 <- y1 - lambda*(y2 - y1)/(x2 - x1)
  p0 <- matrix(c(x0, y0), ncol=2)
  d <- rbind(p0, d)
  
  # Adding an extra point at the end of the diameters matrix to match better the last point
  last <- length(d[,1])
  x1 <- d[(last - 1),1]
  y1 <- d[(last - 1),2]
  x2 <- d[last,1]
  y2 <- d[last,2]
  lambda <- 200
  xFinal <- x2 + lambda
  yFinal <- y2 + lambda*(y2 - y1)/(x2 - x1)
  pFinal <- matrix(c(xFinal, yFinal), ncol=2)
  d <- rbind(d, pFinal)
  
  
  # Linear interpolation of the radius across the lenght of the measurement of the diameters
  #d.approx <- approx(x=d[,1], y=d[,2], seq(from=1, to=d[length(d[,1]),1]))
  print(d)
  d.smoothed <- smooth.spline(d, spar=0.4)
  d.approx <- predict(d.smoothed, 0:d[length(d[,1]), 1],0)
  #print("Diameter in getInertialDiametersPerTick after conversion")
  print(length(d.approx$y))
  return(d.approx$y)
}
#Returns the instant diameter every milisecond
getInertialDiametersPerMs <- function(displacement, diametersPerTick)
{
  print("Diameter in getInertialDiameterPerMs before conversion")
  print(diametersPerTick)
  diameter <- diametersPerTick[abs(cumsum(displacement)) + 1]
  print("Diameter in getInertialDiameterPerMs after conversion")
  print(diameter)
  return(diameter)
}

getOptionsFromFile <- function(optionsFile, lines) {
  optionsCon <- file(optionsFile, 'r')
  options=readLines(optionsCon, n=lines)
  close(optionsCon)
  return (options)
}

options <- getOptionsFromFile("~/chronojump/encoder/tests/conical_diameters_Roptions.txt", 32)
op <- assignOptions(options)
diameter    <- as.numeric(unlist(strsplit("1.5; 1.91; 2.64; 3.38; 3.83; 4.14; 4.28; 4.46; 4.54; 4.77; 4.96; 5.13; 5.3; 5.55", "\\;")))
diametersPerTick <- getInertialDiametersPerTick(diameter)
diametersPerMs <- getInertialDiametersPerMs(displacement, diametersPerTick)
displacementConical <- getDisplacementInertial(displacement,"ROTARYAXISINERTIAL", diametersPerMs, 6)
positionConical <- cumsum(displacementConical)
plot(position, t="l")
lines(diametersPerMs*100, t="l", col="red")
lines(positionConical, t="l", col="green")
speedConical <- getSpeed(displacementConical, 0.7)


