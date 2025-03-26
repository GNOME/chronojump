args <- commandArgs(TRUE)
print (args[1])
d = read.csv2(args[1])

png ("/tmp/encoderDebug.png", width=1920, height=1080)

plot (cumsum(d$yUnfiltered), type="l", main = "Encoder pos, speed, accel", xlab="", ylab="")
lines (cumsum(d$speed)+10, type="l", col="brown")
legend ("topleft", lty=1, col=c("black", "brown", "green", "red"),
	c("Position no filter (mm)", "Position Bw 15 (mm +10)", "Speed Bw 15 (m/s)", "Accel Bw 15 (m/s²)"))

#speed
par (new=T)
ylimValue = max(d$speed)
if (abs(min(d$speed)) > ylimValue)
	ylimValue = abs(min(d$speed))
plot (d$speed, type="l", col="green", axes=F, xlab="", ylab="", ylim=c(-ylimValue,ylimValue))
axis (4, col="green", line=-3)

#accel *1000 to convert from mm/ms to m/s
par (new=T)
ylimValue = max(d$accel)
if (abs(min(d$accel)) > ylimValue)
	ylimValue = abs(min(d$accel))
plot (d$accel, type="l", col="red", axes=F, xlab="", ylab="", ylim=c(-ylimValue,ylimValue))
axis (4, col="red")
abline(h=0, col="red", lty=2)

dev.off ()
