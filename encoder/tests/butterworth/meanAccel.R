library (signal)

doGraph <- function (bwCutoff, accStart, accEnd, title)
{
	bf <- butter(n = 2, W = bwCutoff/(1000/2), type = "low")
	filtered_data <- filter(bf, d) * 1000

	plot (filtered_data, main=paste(title, "BW cutoff: ", bwCutoff))
	par(new=T)
	plot(diff(filtered_data), col="red", axes=F)
	axis(4, col="red")
	abline(v=c(accStart, accEnd), col="brown")
	mtext(side=1, at=(accStart+accEnd)/2, paste("mean acc", accStart, ":", accEnd, "=", round(mean(diff(filtered_data)[accStart:accEnd]),4)), col="red")
}

d=scan("2243-00-prova-2026-02-11_14-41-09.txt", sep=",")
accStart = 1100
accEnd = 2400
title="20 kg, 0 contrapesos"

#d=scan("2243-00-prova-2026-02-11_14-18-52.txt", sep=",")
#accStart = 1200
#accEnd = 7500
#title="2,5 kg, 8 contrapesos"

par(mfcol=c(2,1))
doGraph (3, accStart, accEnd, title)
doGraph (15, accStart, accEnd, title)
par(mfcol=c(1,1))

