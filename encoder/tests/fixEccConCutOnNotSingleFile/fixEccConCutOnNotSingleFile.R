#NIRS_RM_D1, 1er subjecte, 1a sèrie, marcar només la 1a repetició

#estadístiques de sessio (! singleFile) 
#d <- scan ("/tmp/chronojump_enc_curve_1.txt", sep=",")
d <- scan ("curve1.txt", sep=",")

png ("por-sesion-previo.png", width=1920, height=1080)
par (mar=c(5,4,4,2))
plot (cumsum(d), main="Corte ecc/con como sesión previo a corrección", sub="Corte Ecc/Con en la media del valor más bajo.", xlab="Tiempo (ms)", ylab="Posición", type="l")
abline (v=868)
abline (v=870+1278)
mtext ("Ecc", side=3, at=868/2)
mtext ("Con", side=3, at=870+(1278/2))
dev.off ()

#provant amb el findCurvesNew

#source("/home/xavier/informatica/progs_meus/chronojump/encoder/graph.R")
d <- d[!is.na(d)] #if data file ends with comma. Last character will be an NA. remove it
#print (d)
#findCurvesNew (d, "ecS", FALSE, -1)

#no xuta pq no hi ha baixade del concentric després, per tant faig:
#d2=c(d, rep(-1,30))
#findCurvesNew (d, "ecS", FALSE, -1)
#findCurvesNew (d, "ecS", FALSE, 20)


#però donen també el ecc/con a meitat del valor més baix, provant ara el reduceCurveBySpeed

source("/home/xavier/informatica/progs_meus/chronojump/encoder/util.R")
CROSSVALIDATESMOOTH = FALSE
reduceCurveBySpeed ("ecS", 870, -293, d[870:(870+1278)], .7) #dona 1455 2149 -292

#pintar-ho:

png ("por-sesion-post.png", width=1920, height=1080)
par (mar=c(5,4,4,5))
plot (cumsum(d), main="Corte ecc/con como sesión corregidos", sub="Corte Ecc/Con separado y usando los cortes de la velocidad con 0.", xlab="Tiempo (ms)", ylab="Posición", type="l")
abline (v=868)
mtext ("Ecc", side=3, at=868/2)
mtext ("Con", side=3, at=1454+(694/2))

#speed com ho està fent el reduceCurveBySpeed (agafant a partir de 870)
speed <- getSpeed(d[870:(870+1278)], .7)
speed.ext <- extrema(speed$y)
par(new=T)
speed$x = speed$x + 870
plot (speed, col="green", xlim=c(1, 870+length(speed$y)), xlab="", ylab="", axes=F, type="l")
axis (4, col="green")
mtext ("Velocidad", side=4, line=2)
abline (h=0)
abline (v=1454) #per què aquí?
segments (x0=as.vector(speed.ext$cross + 870), y0=-.1, x1=as.vector(speed.ext$cross + 870), y1=.1, col="green")

dev.off ()


#fix the con start by using reduceCurveByPredictStartEnd (that also uses getStableConcentric/EccentricStart)
#és un rotllo pq la depressió que hi ha a final del ecc és el que agafarà aquest mètode
# la línia ___ està just 1 mm per sota de la ------
#  ecc                           con
# \                                /
#  \                              /
#   \                            /
#    \____-----------------------
#     Aixo
# See graph on getStableConcentricStart

minHeight <- 20
position <- cumsum (d)
posMin <- mean (which (position == min (position)))

png ("por-sesion-post-2023.png", width=1920, height=1080)

dCon <- d[posMin:length(d)]
dConShouldStart <- getStableConcentricStart (dCon, minHeight)
plot (cumsum(d), main="Corte ecc/con usando getStableConcentricStart y predictStartEnd", sub="red: getStableConcentricStart, blue: predictStartEnd", xlab="Tiempo (ms)", ylab="Posición", type="l")
abline (v=posMin)
abline (v=posMin+dConShouldStart, col="red")

dConReduced_l <- reduceCurveByPredictStartEnd (dCon, "c", minHeight)
abline (v=posMin+dConReduced_l$startPos, col="blue")
mtext ("con start", side=3, at=posMin+dConReduced_l$startPos)
abline (v=posMin+dConReduced_l$endPos, col="blue")
mtext ("con end", side=3, at=posMin+dConReduced_l$endPos)

dev.off ()


#test with user "un u de csv" 1st ecc-con ecS load file on capture tab, later try also "ec" like paint
d <- scan("curve_ec.txt", sep=",")
d <- d[!is.na(d)] #if data file ends with comma. Last character will be an NA. remove it
curves <- findCurvesNew (d, "ecS", FALSE, 20)

plot (cumsum (d), type="l")
abline (v=curves[,1], col="red")
abline (v=curves[,2], col="blue")

i <- 1 #1st curve, it is an e
dTemp = d[curves[i,1]:curves[i,2]]
reducedCurve_l <- reduceCurveByPredictStartEnd (dTemp, "e", 20)
print (reducedCurve_l)

#fixing the end

#test with "una dona de csv"
d <- scan("curve_ec2.txt", sep=",")
d <- d[!is.na(d)] #if data file ends with comma. Last character will be an NA. remove it
curves <- findCurvesNew (d, "c", FALSE, 20)

plot (cumsum (d), type="l")
abline (v=curves[,1], col="red")
abline (v=curves[,2], col="blue")

i <- 1 #1st curve, it is a "c"
dTemp = d[curves[i,1]:curves[i,2]]
reducedCurve_l <- reduceCurveByPredictStartEnd (dTemp, "c", 20)
print (reducedCurve_l)

#test with borrame3 borra5 18:02:16 rep 11 as current session
d <- scan("curve_ec3.txt", sep=",")
d <- d[!is.na(d)] #if data file ends with comma. Last character will be an NA. remove it

dataTempPhase <- d
endEcc = mean(which(cumsum(dataTempFile) == min(cumsum(dataTempFile))))
startCon = mean(which(cumsum(dataTempFile) == min(cumsum(dataTempFile))))

ecS_ecc_l <- reduceCurveByPredictStartEnd (dataTempFile[1:endEcc],
					   "e", op$MinHeight)
ecS_con_l <- reduceCurveByPredictStartEnd (dataTempFile[startCon:length(dataTempFile)],
					   "c", op$MinHeight)
start = NULL
end = NULL
start [[1]] = ecS_ecc_l$startPos
end [[1]] = ecS_ecc_l$endPos
start [[2]] = endEcc + ecS_con_l$startPos
end [[2]] = endEcc + ecS_con_l$endPos
