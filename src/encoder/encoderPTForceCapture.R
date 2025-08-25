d <- read.csv2("/tmp/forceEncoder.csv")

#force sensor
dForce <- d[which(d$sensor==2),]
plot (1, xlim=c(1,max(d$time)), ylim=c(min(dForce$value),max(dForce$value)), type="n")
points (dForce$time, dForce$value)

# encoder
dEncoder <- d[which(d$sensor==3),]
dEncoderZ <- d[which(d$sensor==4),]

encoderCumsum = cumsum (dEncoder$value)
par(new = T)
plot (1, xlim=c(1,max(d$time)), ylim=c(min(encoderCumsum),max(encoderCumsum)), type="n", axes=F)
axis(4, col="green")
points (dEncoder$time, encoderCumsum, col="green")

abline (v=dEncoderZ$time, col="blue")




