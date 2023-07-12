d=read.csv2("sample-signal-output.csv")
xini = 6545
xend = 7263

#trajectory raw and filtered
plot(d$Raw.coordinate..mm., type="l", xlim=c(xini,xend), ylim=c(min(d$Raw.coordinate..mm[xini:xend]),max(d$Raw.coordinate..mm[xini:xend])), axes=F, ylab="")
lines(d$Filtered.coordinate..mm., type="l", xlim=c(xini,xend), ylim=c(min(d$Raw.coordinate..mm[xini:xend]),max(d$Raw.coordinate..mm[xini:xend])), col="yellow3")

#speed
speed = d$Velocity..mm.ms.
speedMax = max(abs(speed[xini:xend]), na.rm=T)

par(new=T)
plot(speed, type="l", xlim=c(xini,xend), col="green", ylim=c(-speedMax * 1.1, speedMax * 1.1), axes=F, ylab="speed")
axis(2)

#accel
accel = d$Acceleration..mm.ms.. * 1000
accelMax = max(abs(accel[xini:xend]), na.rm=T)

par(new=T)
plot(accel, type="l", xlim=c(xini,xend), col="red", ylim=c(-accelMax * 1.1, accelMax * 1.1), axes=F, ylab="")
mtext(side=4, "accel")
axis(4)

#end
box()
