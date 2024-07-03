
#disSerie <- scan("1569-2024-02-26_11-37-20.txt", sep=",")
#disRep <- scan("chronojump_enc_curve_7_old.txt", sep=",")
#
#disSerie <- disSerie[!is.na(disSerie)]
#disRep <- disRep[!is.na(disRep)]
#
#posSerie = cumsum(disSerie)
#posRep = cumsum(disRep)
#
#plot (posSerie, type="l", xlim=c(5800,11000))
#abline (v=c(8319, 8319+266, 8319+2283), lty=2)
#mtext(side=3, at=8319, "singleFile\nfindCurvesNew cuts\nhere!")
##This is incorrect, as should be on 7847:7887, and then reduceCurve will cut a bit on the right (like in reps)
#
##graph.R this rep starts at 6901
#repStart = 6901
##graph.R singleFile beforeReduce H is 107
#lines (repStart:(repStart+length(posRep)-1),posRep+107, col="red")
#
#abline (v=c(1+repStart, 967+repStart, 1336+repStart, 3700+repStart), col="red", lty=3)
#
#source("/home/xavier/informatica/progs_meus/chronojump/encoder/util.R")
#
#conMinDisplacement = 10
#eccMinDisplacement = 10
#
#par(mfrow=c(3,1))
#plot (posSerie, type="l", main="concentric")
#repsForEccon_l = getRepetitionsForEccon (posSerie, "c", conMinDisplacement, eccMinDisplacement)
#abline (v=repsForEccon_l$repStart)
#abline (v=repsForEccon_l$repEnd, col="red")
#
#plot (posSerie, type="l", main="ec")
#repsForEccon_l = getRepetitionsForEccon (posSerie, "ec", conMinDisplacement, eccMinDisplacement)
#abline (v=repsForEccon_l$repStart)
#abline (v=repsForEccon_l$repEnd, col="red")
#
#plot (posSerie, type="l", main="ecS")
#repsForEccon_l = getRepetitionsForEccon (posSerie, "ecS", conMinDisplacement, eccMinDisplacement)
#abline (v=repsForEccon_l$repStart)
#abline (v=repsForEccon_l$repEnd, col="red")
#par(mfrow=c(1,1))

#-------------------------------------- 2024 jul 2

source("/home/xavier/informatica/progs_meus/chronojump/encoder/util.R")
source("/home/xavier/informatica/progs_meus/chronojump/encoder/graph.R")

#0) read set and repetitions
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

#1) get the curves on singleFile and plot them
curves <- getRepsLikeFindCurvesNew (disSerie, "ecS", FALSE, 10)
print (curves)

png ("fix.png", width=1920, height=1080)
plot (posSerie, type="l", ylim = c(min(posSerie) -20, max(posSerie)+15), main="green should be = blue")
legend("topright",
       col=c("green", "red", "black", "blue", "brown"),
       lty=1,
       legend=c(
		"As set reduced ecc, con",
		"As set not-reduced ecc, con",
		"Full set",
		"As reps reduced ecc, con",
		"Full rep ecc, con")
)

for (i in 1:length(curves[,1]))
{
	# "ecS"
	displacementTemp = disSerie[curves[i,1]:curves[i,2]]
	posTemp = cumsum (displacementTemp)

	reducedCurve_l <- NULL
	ySpace <- NULL
	if (posSerie[curves[i,1]] < posSerie[curves[i,2]])
	{
		reducedCurve_l <- reduceCurveByPredictStartEnd (displacementTemp, "c", 10)
		ySpace = 5
	} else {
		reducedCurve_l <- reduceCurveByPredictStartEnd (displacementTemp, "e", 10)
		ySpace = 10
	}
	
	lines (curves[i,1]:(curves[i,1]+length(posTemp)-1), posTemp+posSerie[curves[i,1]] + ySpace, col="red")

	#print ("reducedCurve_l startPos, endPos")
	#print (c(reducedCurve_l$startPos, reducedCurve_l$endPos))

	curves[i,2] <- curves[i,1] + (reducedCurve_l$endPos -1)
	curves[i,1] <- curves[i,1] + (reducedCurve_l$startPos -1)
	
	displacementTempReduced = disSerie[curves[i,1]:curves[i,2]]
	posTempReduced = cumsum (displacementTempReduced)

	lines (curves[i,1]:(curves[i,1]+length(posTempReduced)-1), posTempReduced+posSerie[curves[i,1]]+15, col="green")
}

#2) now as reps
graphRep <- function (displRep, xPlotStart, yPlotStartE, yPlotStartC)
{
	posRep = cumsum (displRep)

	#print (displRep)
	#print (posRep)
	
	#put abline in the middle
	abline (v=xPlotStart + mean(which(posRep == min(posRep))))

	#endEcc = mean(which(posRep == min(posRep)))
	endEcc = max(which(posRep == min(posRep)))
	#print (paste("endEcc", endEcc))
	endEccDispl = endEcc -1
	if (endEccDispl < 1)
		endEccDispl = 1

	#startCon = mean(which(posRep == min(posRep)))
	startCon = min(which(posRep == min(posRep))) #this is pos, make it displ
	#print (paste("startCon", startCon))
	startConDispl = startCon -1
	if (startConDispl < 1)
		startConDispl = 1

	ePos = posRep [1:endEcc]
	lines (xPlotStart + 1:endEcc, yPlotStartE + ePos -15, col="brown")
	cPos = posRep [startCon:length(posRep)]
	lines (xPlotStart + startCon:(startCon+length(cPos)-1), yPlotStartC + cPos -20, col="brown")

	#print (c("**startCon**", startCon))
	#print (c("**length Con**", length(posRep)-startCon))
	ecS_ecc_l <- reduceCurveByPredictStartEnd (displRep[1:endEccDispl], "e", 10)
	ecS_con_l <- reduceCurveByPredictStartEnd (displRep[startConDispl:length(displRep)], "c", 10)

	ecS_ecc_l$startPos = 1
	ecS_con_l$endPos = length(posRep)

	#+1 to be pos
	eStart = ecS_ecc_l$startPos
	eEnd = ecS_ecc_l$endPos + 1
	cStart = ecS_con_l$startPos + startCon + 1#TODO: check that graph.R has thi]s
	cEnd = ecS_con_l$endPos + startCon +1 #TODO: same as above
	#print (c("eStart", eStart))
	#print (c("eEnd", eEnd))
	#print (c("cStart", cStart))
	#print (c("cEnd", cEnd))

	ePos = posRep [eStart:eEnd]
	cPos = posRep [cStart:cEnd]
	lines (xPlotStart + eStart:(eStart+length(ePos)-1), yPlotStartE + ePos -5, col="blue")
	lines (xPlotStart + cStart:(cStart+length(cPos)-1), yPlotStartC + cPos -10, col="blue")
}
graphRep (disRep6, curves[1,1], posSerie[curves[1,1]], posSerie[curves[1,1]])
graphRep (disRep7, curves[3,1], posSerie[curves[3,1]], posSerie[curves[3,1]])
graphRep (disRep8, curves[5,1], posSerie[curves[5,1]], posSerie[curves[5,1]])
	
dev.off ()


