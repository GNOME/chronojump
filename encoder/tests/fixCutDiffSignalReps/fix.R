#source("/home/xavier/informatica/progs_meus/chronojump/encoder/util.R")

disSerie <- scan("1569-2024-02-26_11-37-20.txt", sep=",")
disRep <- scan("chronojump_enc_curve_7.txt", sep=",")

disSerie <- disSerie[!is.na(serie)]
disRep <- disRep[!is.na(rep)]

posSerie = cumsum(disSerie)
posRep = cumsum(disRep)

plot (posSerie, type="l", xlim=c(5800,11000))
abline (v=c(8319, 8319+266, 8319+2283), lty=2)
mtext(side=3, at=8319, "singleFile\nfindCurvesNew cuts\nhere!")
#This is incorrect, as should be on 7847:7887, and then reduceCurve will cut a bit on the right (like in reps)

#graph.R this rep starts at 6901
repStart = 6901
#graph.R singleFile beforeReduce H is 107
lines (repStart:(repStart+length(posRep)-1),posRep+107, col="red")

abline (v=c(1+repStart, 967+repStart, 1336+repStart, 3700+repStart), col="red", lty=3)


