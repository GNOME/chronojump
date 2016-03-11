source("../../util.R")

speedToSpeedChanges <- function(displ)
{
#	position <- cumsum(displacement)

	charOld = displ[1]
	time = 1
	count = 1
	x = NULL
	y = NULL
	
	#TODO: TRUE or FALSE???
	lastWasTaken = FALSE #because first value will be taken at the end of the function
	for(i in 1:length(displ))  
	{
		if(displ[i] != charOld) {
			if(! lastWasTaken) {
				x[count] = time
				y[count] = displ[i]
			
				count = count +1
			}

			lastWasTaken = ! lastWasTaken
				
			charOld = displ[i]
		}

		time = time +1
	}

	print(length(x))
	print(length(y))

	#return(cbind(x, yPos))
	return( cbind(c(1,x), c(displ[1],y)) ) #we add also first number
}


#signal from encoder
#displ.raw <- scan("set.txt",sep=",")
#displ.raw = displ.raw[4000:6000]
#displ.raw <- c(2,1,1,1,1,2,1,1,1,1,2,1,1,1,1,2)

displ.raw <- scan("oscillating-signal.txt",sep=",")

#-------- new method ------

displ.changes <- speedToSpeedChanges(displ.raw)
pos.raw <- cumsum(displ.raw)
pos.diffspeed <- pos.raw[displ.changes[,1]]
pos.diffspeed.smooth <- smooth.spline(displ.changes[,1], pos.diffspeed, spar=.7)
#pos.diffspeed.smooth <- smooth.spline(displ.changes[,1], pos.diffspeed, spar=.1)

#plot
#plot(pos.raw, type="b", col="red")
#points(displ.changes[,1], pos.diffspeed, col="green", cex=2)
#points(pos.diffspeed.smooth, col="blue", cex=1)
#lines(pos.diffspeed.smooth, col="blue")

speed = predict( pos.diffspeed.smooth, deriv=1 )
accel = predict( pos.diffspeed.smooth, deriv=2 )

#-------- old method ------

speedOld = getSpeed(displ.raw, 0.7)
accelOld = getAcceleration(speedOld)


#-------- graph all --------
# left side
par(mfrow=c(1,2))
plot(pos.raw, type="l", col="black", axes=F, xlab="old method", ylab="")
abline(v=2500)
par(new=T)
plot(speedOld, col="green", type="l", lwd=2, xlab="")
par(new=T)
plot(accelOld, col="magenta", type="l", lwd=2, axes=F, xlab="", ylab="")
axis(4)

# right side
plot(pos.raw, type="l", col="black", axes=F, xlab="new method", ylab="")
abline(v=2500)
par(new=T)
plot(speed, col="green", type="l", lwd=2, xlab="")
par(new=T)
plot(accel, col="magenta", type="l", lwd=2, axes=F, xlab="", ylab="")
axis(4)
par(mfrow=c(1,1))


#displ.raw[2480:2530]
 [1]  0 -1  0  0 -1  0  0  0 -1  0  0 -1  0  0  0 -1  0  0  0  0 -1  0  0  0  0
[26] -1  0  0 -1  0  0 -1  0  0 -1  0  0 -1  0  0 -1  0  0  0 -1  0  0 -1  0  0

#note the: 0  0  0  0 -1  0  0  0  0


