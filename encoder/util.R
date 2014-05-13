#extrema function is part of R EMD package
#It's included here to save time, because 'library("EMD")' is quite time consuming

extrema <- function(y, ndata = length(y), ndatam1 = ndata - 1) {

	minindex <- maxindex <- NULL; nextreme <- 0; cross <- NULL; ncross <- 0 

	z1 <- sign(diff(y))
	index1 <- seq(1, ndatam1)[z1 != 0]; z1 <- z1[z1 != 0]  

	if (!(is.null(index1) || all(z1==1) || all(z1==-1))) {

		index1 <- index1[c(z1[-length(z1)] != z1[-1], FALSE)] + 1 
		z1 <- z1[c(z1[-length(z1)] != z1[-1], FALSE)]  

		nextreme <- length(index1)

		if(nextreme >= 2)
			for(i in 1:(nextreme-1)) {
				tmpindex <- index1[i]:(index1[i+1]-1)
				if(z1[i] > 0) {
					tmpindex <- tmpindex[y[index1[i]] == y[tmpindex]]
					maxindex <- rbind(maxindex, c(min(tmpindex), max(tmpindex)))
				} else {
					tmpindex <- tmpindex[y[index1[i]] == y[tmpindex]]
					minindex <- rbind(minindex, c(min(tmpindex), max(tmpindex)))
				}     
			} 

		tmpindex <- index1[nextreme]:ndatam1  
		if(z1[nextreme] > 0) {
			tmpindex <- tmpindex[y[index1[nextreme]] == y[tmpindex]]
			maxindex <- rbind(maxindex, c(min(tmpindex), max(tmpindex)))
		} else {
			tmpindex <- tmpindex[y[index1[nextreme]] == y[tmpindex]]
			minindex <- rbind(minindex, c(min(tmpindex), max(tmpindex)))
		}  

		### Finding the index of zero crossing  

		if (!(all(sign(y) >= 0) || all(sign(y) <= 0) || all(sign(y) == 0))) {
			index1 <- c(1, index1)
			for (i in 1:nextreme) {
				if (y[index1[i]] == 0) {
					tmp <- c(index1[i]:index1[i+1])[y[index1[i]:index1[i+1]] == 0]
					cross <- rbind(cross, c(min(tmp), max(tmp)))                 
				} else
					if (y[index1[i]] * y[index1[i+1]] < 0) {
						tmp <- min(c(index1[i]:index1[i+1])[y[index1[i]] * y[index1[i]:index1[i+1]] <= 0])
						if (y[tmp] == 0) {
							tmp <- c(tmp:index1[i+1])[y[tmp:index1[i+1]] == 0]
							cross <- rbind(cross, c(min(tmp), max(tmp))) 
						} else 
							cross <- rbind(cross, c(tmp-1, tmp)) 
					}
			}
			#if (y[ndata] == 0) {
			#    tmp <- c(index1[nextreme+1]:ndata)[y[index1[nextreme+1]:ndata] == 0]
			#    cross <- rbind(cross, c(min(tmp), max(tmp)))         
			#} else
			if (any(y[index1[nextreme+1]] * y[index1[nextreme+1]:ndata] <= 0)) {
				tmp <- min(c(index1[nextreme+1]:ndata)[y[index1[nextreme+1]] * y[index1[nextreme+1]:ndata] <= 0])
				if (y[tmp] == 0) {
					tmp <- c(tmp:ndata)[y[tmp:ndata] == 0]
					cross <- rbind(cross, c(min(tmp), max(tmp))) 
				} else
					cross <- rbind(cross, c(tmp-1, tmp))
			}
			ncross <- nrow(cross)        
		}
	} 

	list(minindex=minindex, maxindex=maxindex, nextreme=nextreme, cross=cross, ncross=ncross)
}


findTakeOff <- function(forceConcentric, maxSpeedTInConcentric) 
{
	#this can be a problem because some people does an strange countermovement at start of concentric movement
	#this people moves arms down and legs go up
	#at this moment can be a force == 0, and with this method can be detected as takeoff
	#if this happens, takeoff will be very early and detected jump will be very high
	#takeoff = min(which(force[concentric]<=0)) + length_eccentric + length_isometric

	#then: find the force == 0 in concentric that is closer to max speed time

	#------- example ------
	#force=c(2000,1800,1600,1000,400,100,-10,-25,-5,150,400,600,200,11,-20,-60,-120,-40,5,150)
	#maxSpeedT=17

	#df=data.frame(force<0,force,abs(1:length(force)-maxSpeedT))
	#colnames(df)=c("belowZero","force","dist")

	#df2 = subset(df,subset=df$belowZero)
	#> df2
	#	   belowZero force dist
	#	7       TRUE   -10   10
	#	8       TRUE   -25    9
	#	9       TRUE    -5    8
	#	15      TRUE   -20    2
	#	16      TRUE   -60    1
	#	17      TRUE  -120    0
	#	18      TRUE   -40    1

	#min(which(df2$dist == min(df2$dist)))
	#[1] 6
	#------- end of example ------

	#1 create df dataFrame with forceData and it's distance to maxSpeedT
	df=data.frame(forceConcentric < 0, forceConcentric, abs(1:length(forceConcentric)-maxSpeedTInConcentric))
	colnames(df)=c("belowZero","force","dist")

	#2 create df2 with only the rows where force is below or equal zero
	df2 = subset(df,subset=df$belowZero)

	#print("df2")
	#print(df2)

	#3 find takeoff as the df2 row with less distance to maxSpeedT
	df2row = min(which(df2$dist == min(df2$dist)))
	takeoff = as.integer(rownames(df2)[df2row])

	#print(c("takeoff",takeoff))

	return(takeoff)
}

