setwd("~/chronojump/encoder/tests/")
source("../util.R")

options <- scan(optionsFile, comment.char="#", what=character(), sep="\n")
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


