displacement = scan(file=as.vector("signal_inertial3.txt"),sep=",")
#Read 29483 items

source("../../util.R")
source("../../graph.R")

optionsFile = "Roptions.txt"
options <- scan(optionsFile, comment.char="#", what=character(), sep="\n")

English = unlist(strsplit(options[30], "\\;"))
Translated = unlist(strsplit(options[31], "\\;"))

op <- assignOptions(options)

#displacement=displacement[7610:11727]
plot(displacement)
position=cumsum(displacement)
plot(position)

curvesPlot = TRUE

diametersPerMs = getInertialDiametersPerMs(displacement, op$diameter)
displacement = getDisplacementInertial(displacement, op$EncoderConfigurationName, diametersPerMs, op$diameterExt)

#smooth before InertialBody displacement
smoothingAll= 0.1
speed <- getSpeed(displacement, smoothingAll)
#smoothed displacement
displacement=speed$y

displacement = getDisplacementInertialBody(0, displacement, curvesPlot, op$Title)
#positionStart is 0 in graph.R. It is different on capture.R because depends on the start of every repetition

curvesPlot = FALSE

plot(displacement)

position=cumsum(displacement)
plot(position, type="l")
#abline(v=c(7610,11727))

#speed$y = speed$y * -1

par(new=T)
plot(speed, type="l", col="green")


#xmin=1
#xmax=length(displacement -1)

#----------- grafic curva 3 --------
#xmin=7610
#xmax=9814
xmin=9815
xmax=11727
plot(position[xmin:xmax], type="l",axes=F)
par(new=T)
plot(speed$y[xmin:xmax], type="l", lty=3, col="green",axes=F)

print(c("max speed",max(speed$y[xmin:xmax])))

accel <- getAcceleration(speed)
#speed comes in mm/ms when derivate to accel its mm/ms^2 to convert it to m/s^2 need to *1000 because it's quadratic
accel$y <- accel$y * 1000

par(new=T)
plot(accel$y[xmin:xmax], type="l", col="orange",axes=F)
axis(2)
#abline(h=0,col="orange",lty=2)

smoothing=0.1
dynamics = getDynamics(op$EncoderConfigurationName,
		       speed$y, accel$y, op$MassBody, op$MassExtra, op$ExercisePercentBodyWeight, op$gearedDown, op$anglePush, op$angleWeight,
		       displacement, op$diameter, op$inertiaMomentum, smoothing)
mass = dynamics$mass
force = dynamics$force
power = dynamics$power
forceDisc = dynamics$forceDisc
forceBody = dynamics$forceBody
accelHere = dynamics$accelHere

print("signal_inertial3")
print(max(speed$y[xmin:xmax]))
print(max(accelHere[xmin:xmax]))


par(new=T)
plot(accelHere[xmin:xmax], type="p", col="red", axes=F)

par(new=T)
plot(forceDisc[xmin:xmax], type="p", col="yellow", axes=F)
par(new=T)
plot(forceBody[xmin:xmax], type="l", col="darkblue", axes=F)

par(new=T)
plot(force[xmin:xmax], type="l", col="blue", axes=F)
axis(4)

par(new=T)
plot(power[xmin:xmax], type="l", col="red", axes=F)
#axis(4)

print(c("max power",max(power[xmin:xmax])))



