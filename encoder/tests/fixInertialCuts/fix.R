source ("../../util.R")
source ("../../graph.R")

showCodes <- function (curves)
{
	eccon = "e"
	side = 3
	for (i in 1:length(curves[,1]))
	{
		rep = floor((i+1)/2)
		mtext (side=side, paste(rep,eccon,">",sep=""), at=curves[i,1])
		mtext (side=side, paste("<",rep,eccon,sep=""), at=curves[i,2], col="red")
		if (eccon == "e")
			eccon = "c"
		else
			eccon = "e"

		if (side == 3)
			side = 1
		else
			side = 3
	}
}

getDisplacement <- function (filename, fixSignalIfNotFullyExtended)
{
	displacement = scan(filename, sep=",")
	displacement <- displacement[!is.na(displacement)]
	displacementInertialNotBody <- NULL
	#pos = cumsum(displacement)
	#plot(pos, ylim=c(-max(abs(pos)), max(abs(pos))), type="l")

	if (fixSignalIfNotFullyExtended)
	{
		displacement <- fixInertialSignalIfNotFullyExtended(displacement, 4, "/tmp/chronojump-last-encoder-data.txt", "/tmp/chronojump-special-data.txt", FALSE)
		#pos = cumsum(displacement)
		#lines(pos, type="l", col="red")
	}

	diametersPerMs <- getInertialDiametersPerMs(displacement, 3)
	displacement <- getDisplacementInertial(displacement, "ROTARYAXISINERTIALMOVPULLEY", diametersPerMs, -1, .5)
	#pos = cumsum(displacement)
	#lines(pos, type="l", col="green")

	displacementInertialNotBody <- displacement #store a copy to be used on "single" (all set) to have a better set smooth
	displacement <- getDisplacementInertialBody(0, displacement, F, "title")
	#pos = cumsum(displacement)
	#lines(pos, type="l", col="blue")

	return (displacement)
}


displacement <- getDisplacement ("chronojump-last-encoder-data.txt", F)
curves2023 = findCurvesNew (displacement, "ecS", T, 50)

par(mfrow=c(2,1))
plot(cumsum(displacement), type="l", main="using findCurvesNew")
abline(v=curves2023[,1], col="black")
abline(v=curves2023[,2], col="red")
showCodes (curves2023)

displacement <- getDisplacement ("chronojump-last-encoder-data.txt", T)
curves2024 = getRepsLikeFindCurvesNew (displacement, "ecS", T, 50)

plot(cumsum(displacement), type="l", main="using getRepsLikeFindCurvesNew")
abline(v=curves2024[,1], col="black")
abline(v=curves2024[,2], col="red")
showCodes (curves2024)

par(mfrow=c(1,1))

#print (cumsum(d)[curves2024[6,1]:curves2024[6,2]])
