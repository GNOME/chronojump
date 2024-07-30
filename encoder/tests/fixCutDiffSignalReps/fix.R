
### TODO: check 2nd con



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
#disSerie <- scan("1569-2024-02-26_11-37-20.txt", sep=",")
#disRep1 <- scan("chronojump_enc_curve_6.txt", sep=",")
#disRep2 <- scan("chronojump_enc_curve_7.txt", sep=",")
#disRep3 <- scan("chronojump_enc_curve_8.txt", sep=",")
#minHeight = 10 #cm

disSerie <- scan("1569-2024-02-26_11-31-28.txt", sep=",")
disRep1 <- scan("chronojump_enc_curve_1.txt", sep=",")
disRep2 <- scan("chronojump_enc_curve_2.txt", sep=",")
minHeight = 20 #cm

minHeight = minHeight *10 #mm

disSerie <- disSerie[!is.na(disSerie)]
disRep1 <- disRep1[!is.na(disRep1)]
disRep2 <- disRep2[!is.na(disRep2)]
#disRep3 <- disRep3[!is.na(disRep3)]

posSerie = cumsum(disSerie)
posRep1 = cumsum(disRep1)
posRep2 = cumsum(disRep2)
#posRep3 = cumsum(disRep3)

#1) get the curves on singleFile and plot them
curves <- getRepsLikeFindCurvesNew (disSerie, "ecS", FALSE, minHeight)
print (curves)

png ("fix.png", width=1920, height=1080)
plot (posSerie, type="l", ylim = c(min(posSerie) -20, max(posSerie)+15), main="green should be = blue")
legend("topright",
       col=c("green", "red", "black", "blue1", "blue4", "brown"),
       lty=1,
       legend=c(
		"As set reduced ecc, con",
		"As set not-reduced ecc, con",
		"Full set",
		"As reps reduced ecc",
		"As reps reduced con",
		"Full rep ecc, con")
)


for (i in 1:length(curves[,1]))
{
	print ("----------------")
	print (paste ("as set rep:", i))
	# "ecS"
	displacementTemp = disSerie[curves[i,1]:curves[i,2]]
	#abline (v=c(curves[i,1], curves[i,2]), col="black")
	posTemp = cumsum (displacementTemp)

	#if (i==3)
	#	print (posTemp)
		#print (displacementTemp)

	reducedCurve_l <- NULL
	ySpace <- NULL
	if (posSerie[curves[i,1] -1] < posSerie[curves[i,2] -1])
	{
		reducedCurve_l <- reduceCurveByPredictStartEnd (displacementTemp, "c", minHeight)
		ySpace = 5
	} else {
		if (i==3)
			print (tail(displacementTemp, n=200))

		reducedCurve_l <- reduceCurveByPredictStartEnd (displacementTemp, "e", minHeight)
		ySpace = 10
	}
	
	lines (curves[i,1]:(curves[i,1]+length(posTemp)-1), posTemp+posSerie[curves[i,1]] + ySpace, col="red")

	print (paste ("curvesStart:", curves[i,1], "; curveEnd:", curves[i,2]))
	print ("reducedCurve_l startPos, endPos")
	print (c(reducedCurve_l$startPos, reducedCurve_l$endPos))

	curves[i,2] <- curves[i,1] + (reducedCurve_l$endPos -1)
	curves[i,1] <- curves[i,1] + (reducedCurve_l$startPos -1)
	
	displacementTempReduced = disSerie[curves[i,1]:curves[i,2]]
	posTempReduced = cumsum (displacementTempReduced)

	if (i==3)
		print (paste ("length displacementTempReduced)", length(displacementTempReduced)))
	#	print (posTempReduced)

	lines (curves[i,1]:(curves[i,1]+length(posTempReduced)-1), posTempReduced+posSerie[curves[i,1]]+15, col="green")
	abline (v=c(curves[i,1], curves[i,2]), col="brown")
	print (paste ("NAs:", sum(is.na(posTempReduced)) ))
}

#2) now as reps
graphRep <- function (repN, displRep, xPlotStart, yPlotStartE, yPlotStartC, debug)
{
	posRep = cumsum (displRep)

	#print (displRep)
	#print (posRep)
	
	#put abline in the middle
	abline (v=xPlotStart + mean(which(posRep == min(posRep))))

	#endEcc = mean(which(posRep == min(posRep)))
	endEcc = max(which(posRep == min(posRep)))
	#print (paste("endEcc", endEcc))
	endEccDispl = endEcc +1
	if (endEccDispl > length(displRep))
		endEccDispl = length(displRep)

	#startCon = mean(which(posRep == min(posRep)))
	startCon = min(which(posRep == min(posRep))) #this is pos, make it displ
	#print (paste("startCon", startCon))
	startConDispl = startCon +1
	if (startConDispl > length(displRep))
		startConDispl = length(displRep)

	ePos = posRep [1:endEcc]
	#lines (xPlotStart + 1:endEcc, yPlotStartE + ePos -15, col="brown")
	cPos = posRep [startCon:length(posRep)]
	#lines (xPlotStart + startCon:(startCon+length(cPos)-1), yPlotStartC + cPos -20, col="brown")
	lines (xPlotStart + 1:length(posRep), yPlotStartE + posRep -15, col="brown")

	#if (debug)
		#print (cumsum(displRep[1:endEccDispl]))
	#	print (c("length(ePos)", length(ePos)))

	#print (c("**startCon**", startCon))
	#print (c("**length Con**", length(posRep)-startCon))
	if (debug)
		print (tail(displRep[1:endEccDispl], n=200))

	#ecS_ecc_l <- reduceCurveByPredictStartEnd (displRep[1:endEccDispl], "e", minHeight)
	ecS_ecc_l <- reduceCurveByPredictStartEnd (displRep[1:endEcc], "e", minHeight) #TODO: check if endEcc or endEccDispl
	ecS_con_l <- reduceCurveByPredictStartEnd (displRep[startConDispl:length(displRep)], "c", minHeight)

	ecS_ecc_l$startPos = 1
	ecS_con_l$endPos = length(displRep) - startConDispl

	#+1 to be pos
	eStart = ecS_ecc_l$startPos
	eEnd = ecS_ecc_l$endPos #+ 1
	cStart = ecS_con_l$startPos + startConDispl# + 1
	cEnd = ecS_con_l$endPos + startConDispl# +1
	print (c("eStart", eStart))
	print (c("eEnd", eEnd))
	#print (c("cStart", cStart))
	#print (c("cEnd", cEnd))

	ePos = posRep [eStart:eEnd]
	cPos = posRep [cStart:cEnd]
	lines (xPlotStart + eStart:(eStart+length(ePos)-1), yPlotStartE + ePos -5, col="blue1")
	lines (xPlotStart + cStart:(cStart+length(cPos)-1), yPlotStartC + cPos -5, col="blue4")
	
	print (paste ("as rep e length:", length(ePos)))
	print (paste ("as rep c length:", length(cPos)))
	#if (debug)
	#	print (ePos)
	#	print (cPos)

	print (paste("rep:", repN, "con: E; NAs:", sum(is.na(ePos)) ))
	print (paste("rep:", repN, "con: C; NAs:", sum(is.na(cPos)) ))
}
graphRep (1, disRep1, curves[1,1], posSerie[curves[1,1]], posSerie[curves[1,1]], F)
graphRep (2, disRep2, curves[3,1], posSerie[curves[3,1]], posSerie[curves[3,1]], T)
#graphRep (3, disRep3, curves[5,1], posSerie[curves[5,1]], posSerie[curves[5,1]], F)
	
dev.off ()
