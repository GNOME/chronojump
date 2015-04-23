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
  
  # Linear interpolation of the radius across the lenght of the measurement of the diameters
  d.approx <- approx(x=d[,1], y=d[,2], seq(from=1, to=d[length(d[,1]),1]))
  print("Diameter in getInertialDiametersPerTick after conversion")
  print(d.approx$y)
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
op <- assignOptions(optionsFile)
diameter    <- as.numeric(unlist(strsplit("1.5; 1.91; 2.64; 3.38; 3.83; 4.14; 4.28; 4.46; 4.54; 4.77; 4.96; 5.13; 5.3; 5.55", "\\;")))
diametersPerTick <- getInertialDiametersPerTick(diameter)
diametersPerMs <- getInertialDiametersPerMs(displacement, diameter)
