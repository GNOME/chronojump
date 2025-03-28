drawLine <- function (var, varName, units, start, xmargin, ymargin, col, mtextSide, mtextLine)
{
	lines (start:(start+length(var)-1), var+ymargin, col=col)
	points (x=start, y=var[1]+ymargin, col=col, cex=2)
	points (x=start+length(var)-1, y=var[length(var)], col=col, cex=2)
	mtext (paste("X ", varName, " using +", xmargin, " smooth : ", round (mean (var), 4), " ", units, sep=""), at=start, adj=0, side=mtextSide, line=mtextLine, col=col)
}

df = read.csv2("/tmp/debugAll.csv")
df5446 = read.csv2("/tmp/debug_5446.csv")
df6253 = read.csv2("/tmp/debug_6253.csv")


png ("/tmp/debugAll.png", width=6000, height=2000)

#position
var = df$pos
plot (var, type="l", xlab="", ylab="", ylim=c(- max(abs(var)), max(abs(var))), col="black", lwd=3)
abline (h=0, lty=2, col="gray")
legend ("topleft", lty=1, col=c("black","darkgreen","blue","red"), legend=c("Position","Speed","Force","Power"))

#draw vertical lines
abline(v=c(5446, 6253, 6253+length(df6253$pos)))

#mtext (paste("mean pos of range using all rep: ", round (mean(df$pos[5446:(5446+length(df5446$pos))], na.rm=T), 4), " ", "cm", sep=""), at=5446, adj=0, side=3, line=-1, col="black")
#drawLine (df5446$pos, "pos", "cm", 1, 50, "black", 3, -2)

#speed
par (new=T)
var = df$speed
plot (var, type="l", xlab="", ylab="", ylim=c(- max(abs(var)), max(abs(var))), col="green", axes=F, lwd=3)
axis(4, col="darkgreen", line=-4)

start=5446
mtext (paste("X speed using all set: ", round (mean(df$speed[start:(start+length(df5446$pos))], na.rm=T), 4), " ", "m/s", sep=""), at=start, adj=0, side=3, line=-4, col="darkgreen")
drawLine (df5446$speed, "speed", "m/s", start, 200, .2, "darkgreen", 3, -5)
start=6253
mtext (paste("X speed using all set: ", round (mean(df$speed[start:(start+length(df6253$pos))], na.rm=T), 4), " ", "m/s", sep=""), at=start, adj=0, side=3, line=-4, col="darkgreen")
drawLine (df6253$speed, "speed", "m/s", start, 200, .2, "darkgreen", 3, -5)

#force
par (new=T)
var = df$force
plot (var, type="l", xlab="", ylab="", ylim=c(- max(abs(var)), max(abs(var))), col="blue", axes=F, lwd=3)
axis(4, col="blue", line=-2)

start=5446
mtext (paste("X force using all set: ", round (mean(df$force[start:(start+length(df5446$pos))], na.rm=T), 4), " ", "N", sep=""), at=start, adj=0, side=3, line=-1, col="blue")
drawLine (df5446$force, "force", "N", start, 200, 100, "blue", 3, -2)
start=6253
mtext (paste("X force using all set: ", round (mean(df$force[start:(start+length(df6253$pos))], na.rm=T), 4), " ", "N", sep=""), at=start, adj=0, side=3, line=-1, col="blue")
drawLine (df6253$force, "force", "N", start, 200, 100, "blue", 3, -2)

#power
par (new=T)
var = df$power
plot (var, type="l", xlab="", ylab="", ylim=c(- max(abs(var)), max(abs(var))), col="red", axes=F, lwd=3)
axis(4, col="red", line=0)

start=5446
mtext (paste("X power using all set: ", round (mean(df$power[start:(start+length(df5446$pos))], na.rm=T), 4), " ", "W", sep=""), at=start, adj=0, side=1, line=-1, col="red")
drawLine (df5446$power, "power", "W", start, 200, -100, "red", 1, -2)
start=6253
mtext (paste("X power using all set: ", round (mean(df$power[start:(start+length(df6253$pos))], na.rm=T), 4), " ", "W", sep=""), at=start, adj=0, side=1, line=-1, col="red")
drawLine (df6253$power, "power", "W", start, 200, -100, "red", 1, -2)

dev.off ()
