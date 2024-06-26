#emulating kinematicsF
#CROSSVALIDATESMOOTH=0
#g=9.81
source("/home/xavier/informatica/progs_meus/chronojump/encoder/util.R")
source("/home/xavier/informatica/progs_meus/chronojump/encoder/graph.R") #for findCurvesNew

displSet = NULL
displSession = NULL
posSet = NULL
posSession = NULL

getData <- function ()
{
	#Set of 85Kg
	#Set variables are related to analyze set
	#Session variables are related to analyze as session
	displSet <<- scan ("1569-leonor-2024-02-26_11-37-20.txt", sep = ",")
	displSession <<- scan ("chronojump_enc_curve_6.txt", sep = ",")

	#remove NAs
	displSet <<- displSet[!is.na(displSet)]
	displSession <<- displSession[!is.na(displSession)]

	posSet <<- cumsum (displSet)[1:5000]
	posSession <<- cumsum (displSession)
}

compare <- function ()
{
	plot (posSet, type = "l")
	#lines (posSession, col = "red", lty=3)

	min(which(posSet == min(posSet, na.rm=T)))  		#3099
	min(which(posSession == min(posSession, na.rm=T))) 	#1401
	xDiff = 3099-1401 #1698

	#just to plot
	posSession2 = c(rep(NA, 1698), posSession)
	lines (posSession2, col = "red")

	#SET
	#from graph.R singleFile
	curvesSet <- findCurvesNew(displSet, "ecS", FALSE, 5) #op$minHeight
	curvesSet
	#  startStored endStored startHStored
	#1        21.5    3191.5            0
	#2      3191.5    6884.0         -130
	#...
	i=1
	displacementTemp = displSet[curvesSet[i,1]:curvesSet[i,2]]
	reducedCurve_l <- reduceCurveByPredictStartEnd (displacementTemp, "e", 5) #op$MinHeight)
	#"zerosAtLeft, zerosAtRight" "182" "30" 
	reducedCurve_l$startPos
	# 1659
	#abline (v=1659, col="black")
	reducedCurve_l$endPos #3069
	#abline (v=3069, col="black")

	curvesSet[i,2] <- curvesSet[i,1] + (reducedCurve_l$endPos -1) #3089.5
	#print(curvesSet[i,2])
	abline (v=3089, col="black")
	mtext (side=3, at=3089, "EccSetEnd")

	curvesSet[i,1] <- curvesSet[i,1] + (reducedCurve_l$startPos -1) #1679.5
	abline (v=1679.5, col="black")
	mtext (side=3, at=1679.5, "EccSetStart")

	#SESSION
	#from graph.R !singleFile
	endEcc = mean(which(posSession == min(posSession)))
	ecS_ecc_l <- reduceCurveByPredictStartEnd (displSession[1:endEcc], "e", 5) #op$minHeight
	#"zerosAtLeft, zerosAtRight" "32" "30"
	ecS_ecc_l$start 	#313
	ecS_ecc_l$endPos 	#1391

	#abline (v=313, col="red", lty=3)
	abline (v=313+xDiff, col="red")
	mtext (side=1, at=313+xDiff, "EccSessionStart", col="red")
	abline (v=1391+xDiff, col="red")
	mtext (side=1, at=1391+xDiff, "EccSessionEnd", col="red")

	#FIX: not using reduceCurveByPredictStartEnd at start SESSION
	abline (v=xDiff, col="red")
	mtext (side=1, at=xDiff, "EccSessionStart FIX", col="red", line = -1)
}

png ("compare.png", width=1920, height=1200)
getData ()
compare ()
dev.off ()

#the problem is:
#on saving the curves to the file, the curves start at xDiff: 1698. And then on graph.R it is made also a reduceCurveByPredictStartEnd that reduces more the ecc. So the ecc starts just when is clearly going down.

#The solution (see FIX) is not reduce curve on !singleCurve at start of ecc because it was already reduced on capture.R
#see compare.png
