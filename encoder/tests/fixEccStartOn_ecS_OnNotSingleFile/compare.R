CROSSVALIDATESMOOTH=0 #for getSpeed
#g=9.81
source("/home/xavier/informatica/progs_meus/chronojump/encoder/util.R")
source("/home/xavier/informatica/progs_meus/chronojump/encoder/graph.R") #for findCurvesNew

minHeight = 5 * 10
displSet = NULL
displSession = NULL
i = NULL
posSet = NULL
posSession = NULL
posSetStart = NULL
startHStored = NULL

getData <- function ()
{
	#Set of 85Kg
	#Set variables are related to analyze set
	#Session variables are related to analyze as session
	displSet <<- scan ("1569-leonor-2024-02-26_11-37-20.txt", sep = ",")

	#displSession <<- scan ("chronojump_enc_curve_6.txt", sep = ",")
	#i <<- 1 #is the 1st curve on above set
	#posSetStart <<- 1
	#startHStored <<- 0
	displSession <<- scan ("chronojump_enc_curve_7.txt", sep = ",")
	i <<- 3  #is the 3rd curve on above set
	posSetStart <<- 6884
	startHStored <<- 107

	#remove NAs
	displSet <<- displSet[!is.na(displSet)]
	displSession <<- displSession[!is.na(displSession)]

	posSet <<- cumsum (displSet)[posSetStart:(posSetStart+1000)] #to show 1s
	posSession <<- cumsum (displSession) +startHStored
}

compare <- function ()
{
	plot (posSet, type = "l")
	#plot (posSet, type = "l", xlim=c(0,100))
	#lines (posSession, col = "red", lty=3)

	min(which(posSet == min(posSet, na.rm=T)))  		#3099
	min(which(posSession == min(posSession, na.rm=T))) 	#1401
	#xDiff = 3099-1401 #1698
	xDiff = min(which(posSet == min(posSet, na.rm=T))) - min(which(posSession == min(posSession, na.rm=T)))

	#just to plot
	posSession2 = c(rep(NA, xDiff), posSession)
	lines (1:length(posSession2),(posSession2 -5), col = "red") #-5 to have it below in order to compare

	#SET
	print ("SET")
	#from graph.R singleFile
	curvesSet <- findCurvesNew(displSet, "ecS", FALSE, minHeight)
	print (curvesSet)
	#  startStored endStored startHStored
	#1        21.5    3191.5            0
	#2      3191.5    6884.0         -130
	#3      6884.0    7866.5          107
	#4      8319.0   10716.5         -128
	displacementTemp = displSet[curvesSet[i,1]:curvesSet[i,2]]
	reducedCurve_l <- reduceCurveByPredictStartEnd (displacementTemp, "e", minHeight)

	#print ("displacementTemp")  
	#print (displacementTemp)  
	print ("reducedCurve_l")
	print (reducedCurve_l)
	#"zerosAtLeft, zerosAtRight" "182" "30" 
	print(c("start:", reducedCurve_l$startPos))
	# 1659
	#abline (v=1659, col="black")
	print(c("end:", reducedCurve_l$endPos)) #2729
	#abline (v=2729, col="black")

	curvesSet[i,2] <- curvesSet[i,1] + (reducedCurve_l$endPos -1) #2749.5
	print(c("curvesSet[i,2]", curvesSet[i,2]))
	abline (v=round(curvesSet[i,2]) - posSetStart, col="black")
	mtext (side=3, at=round(curvesSet[i,2]) - posSetStart, adj=0, "EccSetEnd")

	curvesSet[i,1] <- curvesSet[i,1] + (reducedCurve_l$startPos -1) #1679.5
	print(c("curvesSet[i,1]", curvesSet[i,1]))
	abline (v=round(curvesSet[i,1]) - posSetStart, col="black")
	mtext (side=3, at=round(curvesSet[i,1]) - posSetStart, adj=0, "EccSetStart")

	#SESSION
	print ("SESSION")
	#from graph.R !singleFile
	endEcc = mean(which(posSession == min(posSession)))
	ecS_ecc_l <- reduceCurveByPredictStartEnd (displSession[1:endEcc], "e", minHeight)
	#"zerosAtLeft, zerosAtRight" "32" "30"
	print ("reduceCurve")
	print (ecS_ecc_l$startPos) #313  #this will not be used as signal is already cutted
	print (ecS_ecc_l$endPos)   #1051 #this should be used

	#abline (v=313, col="red", lty=3)
	abline (v=ecS_ecc_l$startPos+xDiff, col="red")
	mtext (side=1, at=ecS_ecc_l$startPos+xDiff, "EccSessionStart", adj=0, col="red")
	abline (v=ecS_ecc_l$endPos+xDiff, col="red")
	mtext (side=1, at=ecS_ecc_l$endPos+xDiff, "EccSessionEnd", adj=0, col="red")

	#FIX: not using reduceCurveByPredictStartEnd at start SESSION
	abline (v=xDiff, col="red")
	mtext (side=1, at=xDiff, "EccSessionStart FIX", col="red", adj=0, line = -1)

	#SPEEDS
	spar = .7
	#show speeds after fix. SET
	print ("displ for SET:")
	print (displSet[round(curvesSet[i,1]):round(curvesSet[i,2])])
	speedYSet = getSpeed (displSet[round(curvesSet[i,1]):round(curvesSet[i,2])], spar)$y
	print (c("mean speed set", mean (speedYSet)))
	#print (which (speedYSet == min (speedYSet)))
	#lines(1679:(1679-1+length(speedYSet)), speedYSet*250) #*250 to be able to see it
	par (new =T)
	plot (speedYSet, type="l", col="green", axes=F)

	#show speeds after fix. SESSION
	print ("displ for SESSION:")
	print (displSession[1:ecS_ecc_l$endPos])
	speedYSession = getSpeed (displSession[1:ecS_ecc_l$endPos], spar)$y
	print (c("mean speed session", mean (speedYSession)))
	#print (min (speedYSession))
	##lines(xDiff:(xDiff-1+length(speedYSession)), speedYSession*250, col="red") #*250 to be able to see it
	par (new =T)
	plot (speedYSession, type="l", col="blue", axes=F)

	#abline(v=round(curvesSet[i,2]) +21, col="green") #note here is where chronojump is reducing the end!!!
	#abline(v=3191, col="cyan") #center of the isometric down phase
}

png ("compare.png", width=1920, height=1200)
getData ()
compare ()
dev.off ()

#the problem is:
#on saving the curves to the file, the curves start at xDiff: 1698. And then on graph.R it is made also a reduceCurveByPredictStartEnd that reduces more the ecc. So the ecc starts just when is clearly going down.

#The solution (see FIX) is not reduce curve on !singleCurve at start of ecc because it was already reduced on capture.R
#see compare.png
