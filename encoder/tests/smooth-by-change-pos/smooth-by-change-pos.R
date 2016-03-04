source("../../util.R")

d <- scan("set.txt",sep=",")

displacementToChange <- function(displacement)
{
	position <- cumsum(displacement)

	charOld = position[1]
	time = 1
	pos = 1
	x = NULL
	y = NULL
	for(i in 1:length(position))  
	{
		if(position[i] != charOld) {
			x[pos] = time
			y[pos] = position[i]
			charOld = position[i]
			pos = pos +1
		}
		time = time +1
	}

#	print(x[2]:length(x))
	print(length(x))
	print(length(y))
	yPos = c(diff(y)[1],diff(y)) #diff to have displacement again (not pos)

	return(cbind(x, yPos))
}

#keep just a concentric
#d=d[10000:12500]

d2 <- displacementToChange(d)
head(d2)

speed = getSpeed(d, 0.5)
speed2 <- smooth.spline( d2[,1], d2[,2], spar=0.5)

plot(speed, type="l")
lines(speed2,col="red") #revisar pq la curva vermella te una oscil.lacio

#start <- 10000; end <- 12500
#plot(start:end, speed$y[start:end])
#lines(speed2$x[start:end], speed2$y[start:end],col="red")
