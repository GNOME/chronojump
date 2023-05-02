#emulating kinematicsF
CROSSVALIDATESMOOTH=0
g=9.81
source("/home/xavier/informatica/progs_meus/chronojump/encoder/util.R")
library("pspline")
library("signal")

#this data is from mastercede session
serie <- scan("serie.txt", skip=5)
sessio <- scan("sessio.txt", skip=5)

print("serie")
print(serie)
print("sessio")
print(sessio)

speedSerie <- getSpeed(serie, .7)
speedSessio <- getSpeed(sessio, .7)
#speedSerie <- getSpeed(serie, .5)
#speedSessio <- getSpeed(sessio, .5)
#speedSerie <- getSpeedSplineUsingWeights(serie, .7)
#speedSessio <- getSpeedSplineUsingWeights(sessio, .7)
#speedSerie <- getSpeedTrimmingZerosInitialAndEnd(serie, .7)
#speedSessio <- getSpeedTrimmingZerosInitialAndEnd(sessio, .7)

accelSerie <- getAccelerationSafe(speedSerie)
accelSessio <- getAccelerationSafe(speedSessio)

accelSerie$y <- accelSerie$y * 1000
accelSessio$y <- accelSessio$y * 1000

eccentricSerie = 0
concentricSerie = 0
propulsiveEndSerie = 0
eccentricSessio = 0
concentricSessio = 0
propulsiveEndSessio = 0

concentricSerie=1:length(serie)
concentricSessio=1:length(sessio)

maxSpeedTSerie <- min(which(speedSerie$y == max(speedSerie$y)))
maxSpeedTSessio <- min(which(speedSessio$y == max(speedSessio$y)))

maxSpeedTInConcentricSerie = maxSpeedTSerie
maxSpeedTInConcentricSessio = maxSpeedTSessio

propulsiveEndSerie = findPropulsiveEnd(accelSerie$y, concentricSerie, maxSpeedTInConcentricSerie, "c", -1, -1, 72, 40, 0)
propulsiveEndSessio = findPropulsiveEnd(accelSessio$y, concentricSessio, maxSpeedTInConcentricSessio, "c", -1, -1, 72, 40, 0)

#png ("compareSetSession.png", width=1920, height=1080)
png ("compareSetSession.png", width=1920, height=6080)
plot (cumsum(serie), type="l", col="black", lty=1)
lines (cumsum(sessio), col="black", lty=2)

par(new=T)
plot(speedSerie$x, speedSerie$y, type="l", col="green", lty=1)
lines((2+speedSessio$x), speedSessio$y, col="green", lty=2)

abline (v=propulsiveEndSerie, lty=1)
abline (v=2+propulsiveEndSessio, lty=2)

par(new=T)
plot(accelSerie$x, accelSerie$y, type="l", col="magenta", lty=1)
lines((2+accelSessio$x), accelSessio$y, col="magenta", lty=2)

#Serie
dynamicsSerie = getDynamics("c",
		speedSerie$y, accelSerie$y, 72, 40, 0, -1, -1, -1,
		serie, -1, -1, .7)
massSerie = dynamicsSerie$mass
forceSerie = dynamicsSerie$force
powerSerie = dynamicsSerie$power

startSerie <- 1
endSerie <- length(speedSerie$y)
endSerie <- propulsiveEndSerie
print (mean(powerSerie[startSerie:endSerie]))
#print (mean(powerSerie[(startSerie+2):endSerie]))

#Sessio
dynamicsSessio = getDynamics("c",
		speedSessio$y, accelSessio$y, 72, 40, 0, -1, -1, -1,
		serie, -1, -1, .7)
massSessio = dynamicsSessio$mass
forceSessio = dynamicsSessio$force
powerSessio = dynamicsSessio$power

startSessio <- 1
endSessio <- length(speedSessio$y)
endSessio <- propulsiveEndSessio
print (mean(powerSessio[startSessio:endSessio]))

par(new=T)
plot(1:length(powerSerie), powerSerie, type="l", col="red", lty=1, axes=F)
#plot(1:length(powerSerie), powerSerie, type="l", col="red", lty=1, axes=F, ylim=c(0,100))
#plot(1:length(powerSerie), powerSerie, type="l", col="red", lty=1, axes=F, ylim=c(1500,2500))
abline(h=mean (powerSerie[startSerie:endSerie]), col="red")
axis(4)
lines(3:(length(powerSessio)+2), powerSessio, col="red", lty=2)
abline(h=mean (powerSessio[startSessio:endSessio]), col="red", lty=2)
dev.off()

#print ("powerSerie")
#print (powerSerie)
#print ("powerSessio")
#print (powerSessio)
