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

displacementInertialNotBody = NULL
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
	displacement <- getDisplacementInertial(displacement, "ROTARYAXISINERTIALMOVPULLEY", diametersPerMs, -1, .5) #gearedDown -2 on Roptions.txt gets .5
	#pos = cumsum(displacement)
	#lines(pos, type="l", col="green")

	displacementInertialNotBody <<- displacement #store a copy to be used on "single" (all set) to have a better set smooth
	displacement <- getDisplacementInertialBody(0, displacement, F, "title")
	#pos = cumsum(displacement)
	#lines(pos, type="l", col="blue")

	return (displacement)
}

#filename = "chronojump-last-encoder-data.txt"
filename = "chronojump-last-encoder-data2.txt"

displacement <- getDisplacement (filename, F)
curves2023 = findCurvesNew (displacement, "ecS", T, 65)

par(mfrow=c(2,1))
plot(cumsum(displacement), type="l", main="using findCurvesNew")
abline(v=curves2023[,1], col="black")
abline(v=curves2023[,2], col="red")
showCodes (curves2023)

displacement <- getDisplacement (filename, T)
curves2024 = getRepsLikeFindCurvesNew (displacement, "ecS", T, displacementInertialNotBody, 65)

plot (cumsum(displacement), ylim=c(min(cumsum(displacement)),max(cumsum(displacementInertialNotBody))), main="using getRepsLikeFindCurvesNew (after will be reduced by stability)", axes=F, type="n")
lines (cumsum(displacementInertialNotBody), col="green", lty=3, lwd=3)
lines (cumsum(displacement), type="l")
axis(1, seq(from=0, to=length(displacement), by=100), las=2, cex.axis=.8)
axis(2)
box()
abline(v=curves2024[,1], col="black")
abline(v=curves2024[,2], col="red")
showCodes (curves2024)
par(mfrow=c(1,1))
