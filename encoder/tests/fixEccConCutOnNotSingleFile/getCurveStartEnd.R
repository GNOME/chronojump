#this data is from mastercede session
serie <- scan("serie.txt", skip=5)
sessio <- scan("sessio.txt", skip=5)

displacement <- serie

firstInitialNonZero <- min(which(displacement != 0))
lastFinalNonZero <- max(which(displacement != 0))

disCut <- displacement[firstInitialNonZero:lastFinalNonZero]
#print (c("serie", serie))
#print (c("disCut", disCut))

position <- cumsum (disCut)
#print (position)

x <- 1:length(position)
#model <- lm (position ~ poly(x,3))
#x0 <- x1
#y0 <- predict.lm (model, newdata = list (x = x0))

positionSplineCvT <- smooth.spline (x, position, cv=TRUE)
positionSplineCvF <- smooth.spline (x, position, cv=FALSE)

#get spline by decreasing weights
#left
weights <- 1-(position/20)
weights[weights <= 0.05] <- 0.05
positionSplineLeft <- smooth.spline (x, position, w=weights)
#right
posTemp = abs(position-(max(position)))
weights <- 1 - posTemp/20
weights[weights <= 0.05] <- 0.05
positionSplineRight <- smooth.spline (x, position, w=weights)

png ("getCurveStartEnd.png", width=1920, height=1080)
par(mfrow=c(1,3))
#plot (position)
#plot left side
plot (position, ylim=c(0,10))
lines (positionSplineLeft, col="red")
lines (positionSplineCvT, col="green")
lines (positionSplineCvF, col="blue")

#plot right side
plot (position, ylim=c(max(position)-10,max(position)))
lines (positionSplineRight, col="red")
lines (positionSplineCvT, col="green")
lines (positionSplineCvF, col="blue")

plot (position)
dev.off()
#plot (x0,y0, col="red")

#predict left start
xPre <- seq(from=-50, to=0, length.out=400)
prediction <- predict(positionSplineLeft, x = xPre)
#print (prediction)
pos <- max(which(abs(prediction$y) == min(abs(prediction$y))))
#xAtYZero <- prediction$x[pos]
print (c(prediction$x[pos], prediction$y[pos]))
print ("needed zeros at left: ")
print (round (abs(prediction$x[pos]), 0))

#predict right end
xPre <- seq(from=max(x), to=max(x)+50, length.out=400)
prediction <- predict(positionSplineRight, x = xPre)

prediction$y <- abs(prediction$y - (max(position)+1))
print ("prediction at right")
#print (prediction$y)
pos <- min(which(abs(prediction$y) == min(abs(prediction$y))))
#xAtYMaxPlus1 <- prediction$x[pos]
print (c(prediction$x[pos], prediction$y[pos]))
print ("needed zeros at right: ")
print (round (prediction$x[pos], 0) - length(position))

