#previous to 1.5.1
#ec smooth seem not well found by findSmoothingsEC on some curves
#in this sample. ec smooth calculated is 0.7. e smooth is 0.7 and c smooth is 0.7
#The 0.7 is calculated by findSmoothingsEC using speed as reference. The difference on speed at different smoothings is very low.
#now we demonstrate that use power is much better.
#At 1.5.1 we have improved findSmoothingsEC is created (it compares power instead of speed)
#old method found a suitable ec smooth of 0.7
#new method found a suitable ec smooth of 0.58 that adjust perfectly with the 0.7 e and 0.7 c

#---- methods ----

getSpeed <- function(displacement, smoothing) {
	#no change affected by encoderConfiguration
	return (smooth.spline( 1:length(displacement), displacement, spar=smoothing))
}

getAcceleration <- function(speed) {
	#no change affected by encoderConfiguration
	return (predict( speed, deriv=1 ))
}

findSmoothingsECGetPower <- function(speed)
{                               
	acceleration <- getAcceleration(speed)
	acceleration$y <- acceleration$y * 1000
	force <- 50 * (acceleration$y + 9.81) #Do always with 50Kg right now. TODO: See if there's a need of real mass value
	power <- force * speed$y        
	return(power)                   
}     

#---- read data ----

d <- scan(file=as.vector("ec-ecS-smooth-comparison-signal.txt"),sep=",")
#Read 11564 items

plot(d)

#---- perform calculations and graphs ----

#displacement
d.ec.1 <- d[2570:3338]
d.e.1  <- d[2570:2947]
d.c.1  <- d[2947:3338]

#speed
s.ec.1 <- getSpeed(d.ec.1,0.7)
s.e.1  <- getSpeed(d.e.1,0.7)
s.c.1  <- getSpeed(d.c.1,0.7)
#graph speed
plot(s.ec.1$y,type="l")
lines(s.e.1$y,type="l",col="green")
lines(x=((length(s.e.1$y)+1):(length(s.e.1$y)+length(s.c.1$y))),y=s.c.1$y,type="l",col="blue")

#power
p.ec.1  <-  findSmoothingsECGetPower(s.ec.1)
p.e.1   <-  findSmoothingsECGetPower(s.e.1)
p.c.1   <-  findSmoothingsECGetPower(s.c.1)
#graph power
plot(p.ec.1,type="l",lty=1, ylim=c(min(p.e.1),max(p.c.1)))
lines(p.e.1,lty=2,col="darkgreen")
lines(x=((length(p.e.1)+1):(length(p.e.1)+length(p.c.1))),y=p.c.1,lty=2,col="blue")

#with new findSmoothingsEC method (1.5.1), ec smooth for this signal is 0.58
s.ec.1.b <- getSpeed(d.ec.1,0.58)
p.ec.1.b <-  findSmoothingsECGetPower(s.ec.1.b)

#it adjust perfectly
lines(p.ec.1.b,lty=3,lwd=2,col="red")

