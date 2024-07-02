
disSerie <- scan("1569-2024-02-26_11-37-20.txt", sep=",")
disRep <- scan("chronojump_enc_curve_7_old.txt", sep=",")

disSerie <- disSerie[!is.na(disSerie)]
disRep <- disRep[!is.na(disRep)]

posSerie = cumsum(disSerie)
posRep = cumsum(disRep)

plot (posSerie, type="l", xlim=c(5800,11000))
abline (v=c(8319, 8319+266, 8319+2283), lty=2)
mtext(side=3, at=8319, "singleFile\nfindCurvesNew cuts\nhere!")
#This is incorrect, as should be on 7847:7887, and then reduceCurve will cut a bit on the right (like in reps)

#graph.R this rep starts at 6901
repStart = 6901
#graph.R singleFile beforeReduce H is 107
lines (repStart:(repStart+length(posRep)-1),posRep+107, col="red")

abline (v=c(1+repStart, 967+repStart, 1336+repStart, 3700+repStart), col="red", lty=3)

source("/home/xavier/informatica/progs_meus/chronojump/encoder/util.R")

conMinDisplacement = 10
eccMinDisplacement = 10

par(mfrow=c(3,1))
plot (posSerie, type="l", main="concentric")
repsForEccon_l = getRepetitionsForEccon (posSerie, "c", conMinDisplacement, eccMinDisplacement)
abline (v=repsForEccon_l$repStart)
abline (v=repsForEccon_l$repEnd, col="red")

plot (posSerie, type="l", main="ec")
repsForEccon_l = getRepetitionsForEccon (posSerie, "ec", conMinDisplacement, eccMinDisplacement)
abline (v=repsForEccon_l$repStart)
abline (v=repsForEccon_l$repEnd, col="red")

plot (posSerie, type="l", main="ecS")
repsForEccon_l = getRepetitionsForEccon (posSerie, "ecS", conMinDisplacement, eccMinDisplacement)
abline (v=repsForEccon_l$repStart)
abline (v=repsForEccon_l$repEnd, col="red")
par(mfrow=c(1,1))

#-------------------------------------- 2024 jul 2

source("/home/xavier/informatica/progs_meus/chronojump/encoder/util.R")
source("/home/xavier/informatica/progs_meus/chronojump/encoder/graph.R")

disSerie <- scan("1569-2024-02-26_11-37-20.txt", sep=",")
disRep6 <- scan("chronojump_enc_curve_6.txt", sep=",")
disRep7 <- scan("chronojump_enc_curve_7.txt", sep=",")
disRep8 <- scan("chronojump_enc_curve_8.txt", sep=",")

disSerie <- disSerie[!is.na(disSerie)]
disRep6 <- disRep6[!is.na(disRep6)]
disRep7 <- disRep7[!is.na(disRep7)]
disRep8 <- disRep8[!is.na(disRep8)]

posSerie = cumsum(disSerie)
posRep6 = cumsum(disRep6)
posRep7 = cumsum(disRep7)
posRep8 = cumsum(disRep8)

curves <- getRepsLikeFindCurvesNew (disSerie, "ecS", FALSE, 10)
print (curves)

#get the curves on singleFile and plot them
plot (posSerie, type="l")
for (i in 1:length(curves[,1]))
{
	# "ecS"
	displacementTemp = disSerie[curves[i,1]:curves[i,2]]
	posTemp = cumsum (displacementTemp)
	lines (curves[i,1]:(curves[i,1]+length(posTemp)-1), posTemp+posSerie[curves[i,1]]+5, col="red")

	reducedCurve_l <- NULL
	if (posSerie[curves[i,1]] < posSerie[curves[i,2]])
		reducedCurve_l <- reduceCurveByPredictStartEnd (displacementTemp, "c", 10)
	else
		reducedCurve_l <- reduceCurveByPredictStartEnd (displacementTemp, "e", 10)

	print ("reducedCurve_l startPos, endPos")
	print (c(reducedCurve_l$startPos, reducedCurve_l$endPos))

	curves[i,2] <- curves[i,1] + (reducedCurve_l$endPos -1)
	curves[i,1] <- curves[i,1] + (reducedCurve_l$startPos -1)
	
	displacementTempReduced = disSerie[curves[i,1]:curves[i,2]]
	posTempReduced = cumsum (displacementTempReduced)

	lines (curves[i,1]:(curves[i,1]+length(posTempReduced)-1), posTempReduced+posSerie[curves[i,1]]+10, col="green")
}

#lines (curves[1,1]:(curves[1,1]+length(posRep6)-1), posRep6, col="red")
#lines (curves[2,1]:(curves[2,1]+length(posRep7)-1), posRep7+100, col="green")
#lines (curves[3,1]:(curves[3,1]+length(posRep8)-1), posRep8+150, col="blue")


