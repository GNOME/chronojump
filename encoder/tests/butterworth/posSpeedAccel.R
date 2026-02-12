library (signal)

getFilteredData <- function (variable, bwCutoff)
{
	bf <- butter(n = 2, W = bwCutoff/(1000/2), type = "low")
	filtered_data <- filter(bf, variable) * 1000
	return (filtered_data)
}

doGraph <- function (pos, speed, accel, title)
{
	plot(pos, type="l", main=title, axes=F, xlab="Time (ms)", ylab="Position (mm)")
	axis(1)
	axis(2)

	par(new=T)
	plot(speed, ,col="green", axes=F, xlab="", ylab="")
	abline(h=0, col="green", lty=2)
	axis(4, col="green", line=-2)

	par(new=T)
	plot(accel, col="red", axes=F, xlab="", ylab="")
	abline(h=0, col="red", lty=2)
	axis(4, col="red", line=-0)
}

#d=scan("2243-00-prova-2026-02-11_14-41-09.txt", sep=",")
#title="20 kg, 0 contrapesos"

bwCutoff = 1
d=scan("2243-00-prova-2026-02-11_14-18-52.txt", sep=",")
title = paste ("2,5 kg, 8 contrapesos. Bw cutoff:", bwCutoff)

#speed
d_f = 0.001 * getFilteredData (d, bwCutoff)

#position
pos = 0.001 * cumsum(d_f)
pos_f = getFilteredData (pos, bwCutoff)

#acceleration
accel = diff(d_f)
accel_f = getFilteredData (accel, bwCutoff)

doGraph (pos_f, d_f, accel_f, title)

#mean accel
accStart = 2500
accEnd = 7500
abline(v=c(accStart, accEnd), col="brown")
mtext(side=1, at=(accStart+accEnd)/2, paste("mean acc", accStart, ":", accEnd, "=", round(mean(accel_f[accStart:accEnd]),4)), col="red")

