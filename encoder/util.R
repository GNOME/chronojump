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

getSpeed <- function(displacement, smoothing) {
	#no change affected by encoderConfiguration

	return (smooth.spline( 1:length(displacement), displacement, spar=smoothing))
}

getAcceleration <- function(speed) {
	#no change affected by encoderConfiguration
	return (predict( speed, deriv=1 ))
}

#gearedDown is positive, normally 2
getMass <- function(mass, gearedDown, angle) {
	if(mass == 0)
		return (0)

	#default value of angle is 90 degrees. If is not selected, it's -1
	if(angle == -1)
		angle = 90

	return ( ( mass / gearedDown ) * sin( angle * pi / 180 ) )
}

getMassBodyByExercise <- function(massBody, exercisePercentBodyWeight) {
	if(massBody == 0 || exercisePercentBodyWeight == 0)
		return (0)
	
	return (massBody * exercisePercentBodyWeight / 100.0)
}

getMassByEncoderConfiguration <- function(encoderConfigurationName, massBody, massExtra, exercisePercentBodyWeight, gearedDown, anglePush, angleWeight)
{
	massBody = getMassBodyByExercise(massBody,exercisePercentBodyWeight)

	if(
	   encoderConfigurationName == "WEIGHTEDMOVPULLEYLINEARONPERSON1" ||
	   encoderConfigurationName == "WEIGHTEDMOVPULLEYLINEARONPERSON1INV" ||
	   encoderConfigurationName == "WEIGHTEDMOVPULLEYLINEARONPERSON2" ||
	   encoderConfigurationName == "WEIGHTEDMOVPULLEYLINEARONPERSON2INV" ||
	   encoderConfigurationName == "WEIGHTEDMOVPULLEYROTARYFRICTION" ||
	   encoderConfigurationName == "WEIGHTEDMOVPULLEYROTARYAXIS" ) 
	{
		massExtra = getMass(massExtra, gearedDown, anglePush)
	} else if(encoderConfigurationName == "LINEARONPLANE") {
		massBody = getMass(massBody, gearedDown, anglePush)
		massExtra = getMass(massExtra, gearedDown, anglePush)
	} else if(encoderConfigurationName == "LINEARONPLANEWEIGHTDIFFANGLE") {
		massBody = getMass(massBody, gearedDown, anglePush)
		massExtra = getMass(massExtra, gearedDown, angleWeight)
	}
		
	mass = massBody + massExtra
	return (mass)
}

#used in alls eccons
reduceCurveBySpeed <- function(eccon, row, startT, startH, displacement, smoothingOneC) 
{
	#print("at reduceCurveBySpeed")

	#In 1.4.0 and before, we use smoothingOneEC on "ec", "ce"
	#but the problem is findSmoothingsEC has problems knowing the smoothingEC when users stays stand up lot of time before jump.
        #is better to reduceCurveBySpeed first in order to remove the not-moving phase
	#and then do findSmoothingsEC
	#for this reason, following code is commented, and it's only used smoothingOneC	

	#smoothing = smoothingOneEC
	#if(eccon == "c" || eccon == "ecS" || eccon == "ceS")
	#	smoothing = smoothingOneC
		
	smoothing = smoothingOneC

	speed <- getSpeed(displacement, smoothing)
	
	speed.ext=extrema(speed$y)

	#in order to reduce curve by speed, we search the cross of speed (in 0m/s)
        #before and after the peak value, but in "ec" and "ce" there are two peak values:
	#
	#ec         2
	#\         / \
	# \       /   \
	#-----------------
	#   \   /       \
	#    \ /         \
	#     1     
	#
	#ce   1
	#    / \         /
	#   /   \       /
	#-----------------
	# /       \   /
	#/         \ /
	#           2
	#
	#then we need two times: time1, time2 to search cross speed 0 before and after them

	time1 = 0
	time2 = 0
	if(eccon=="ec") {
		time1 = min(which(speed$y == min(speed$y)))
		time2 = max(which(speed$y == max(speed$y)))
	} else if(eccon=="ce") {
		time1 = min(which(speed$y == max(speed$y)))
		time2 = max(which(speed$y == min(speed$y)))
	} else {
		speed$y=abs(speed$y)
		time1 = min(which(speed$y == max(speed$y)))
		time2 = max(which(speed$y == max(speed$y)))
	}

	#now that times are defined we can work in ABS for all the curves
	speed$y=abs(speed$y)

	#left adjust
	#find the speed.ext$cross at left of max speed
	x.ini = 0 #good to declare here
	ext.cross.len = length(speed.ext$cross[,2])
	if(ext.cross.len == 0)
		x.ini = 0
	else if(ext.cross.len == 1) {
		if(speed.ext$cross[,2] < time1) 
			x.ini = speed.ext$cross[,2]
	} else { 
		for(i in speed.ext$cross[,2]) 
			if(i < time1) 
				x.ini = i
	}

	#right adjust
	#find the speed.ext$cross at right of max speed
	x.end = length(displacement) #good to declare here
	#ext.cross.len = length(speed.ext$cross[,2])
	if(ext.cross.len == 0)
		x.end = length(displacement)
	else if(ext.cross.len == 1) {
		if(speed.ext$cross[,2] > time2) 
			x.end = speed.ext$cross[,2]
	} else { 
		for(i in rev(speed.ext$cross[,2])) 
			if(i > time2) 
				x.end = i
	}

	#debug
	#print(speed.ext$cross[,2])
	#print(ext.cross.len)
	#print(c("time1,time2",time1,time2))
	#print(c("x.ini x.end",x.ini,x.end))

	#to know the new startH
	#calculate displacement from original start to the new: x.ini
	startH.old = startH
	startH = startH + sum(displacement[1:x.ini])
	#print(c("old startH:", startH.old, "; new startH:", startH))

	return(c(startT + x.ini, startT + x.end, startH))
}

#go here with every single curve
#eccon="c" one time each curve
#eccon="ec" one time each curve
#eccon="ecS" means ecSeparated. two times each curve: one for "e", one for "c"
kinematicsF <- function(displacement, massBody, massExtra, exercisePercentBodyWeight,
			encoderConfigurationName,diameter,diameterExt,anglePush,angleWeight,inertiaMomentum,gearedDown,
			smoothingOneEC, smoothingOneC, g, eccon, isPropulsive) {

	smoothing = 0
	if(eccon == "c" || eccon == "e")
		smoothing = smoothingOneC
	else
		smoothing = smoothingOneEC

#print(c(" smoothing:",smoothing))

	#x vector should contain at least 4 different values
	if(length(displacement) >= 4)
		speed <- getSpeed(displacement, smoothing)
	else
		speed=list(y=rep(0,length(displacement)))
	
	if(length(displacement) >= 4)
		accel <- getAcceleration(speed)
	else
		accel=list(y=rep(0,length(displacement)))

	#print(c(" ms",round(mean(speed$y),5)," ma",round(mean(accel$y),5)))
	#print(c(" Ms",round(max(speed$y),5)," Ma",round(max(accel$y),5)))
	#print(c(" |ms|",round(mean(abs(speed$y)),5)," |ma|:",round(mean(abs(accel$y)),5)))
	#print(c(" |Ms|",round(max(abs(speed$y)),5)," |Ma|",round(max(abs(accel$y)),5)))

	#speed comes in mm/ms when derivate to accel its mm/ms^2 to convert it to m/s^2 need to *1000 because it's quadratic
	accel$y <- accel$y * 1000 
	errorSearching = FALSE

	concentric = 0
	propulsiveEnd = 0

	#print(c("at kinematicsF eccon: ", eccon, " length(displacement): ",length(displacement)))

	#search propulsiveEnd
	if(isPropulsive) {
		if(eccon=="c") {
			concentric=1:length(displacement)
			
			maxSpeedT <- min(which(speed$y == max(speed$y)))
			maxSpeedTInConcentric = maxSpeedT
			
			propulsiveEnd = findPropulsiveEnd(accel$y,concentric,maxSpeedTInConcentric,
							  encoderConfigurationName, anglePush, angleWeight, 
							  massBody, massExtra, exercisePercentBodyWeight)
		} else if(eccon=="ec") {
			phases=findECPhases(displacement,speed$y)
			eccentric = phases$eccentric
			isometric = phases$isometric
			concentric = phases$concentric
	
			#temporary fix problem of found MinSpeedEnd at right
			if(eccentric == 0 && isometric == 0 && concentric == 0)
				propulsiveEnd = length(displacement)
			else {
				maxSpeedT <- min(which(speed$y == max(speed$y)))
				maxSpeedTInConcentric = maxSpeedT - (length(eccentric) + length(isometric))

				propulsiveEnd = length(eccentric) + length(isometric) + 
						findPropulsiveEnd(accel$y,concentric,maxSpeedTInConcentric, 
								  encoderConfigurationName, anglePush, angleWeight, 
								  massBody, massExtra, exercisePercentBodyWeight)
				#print(c("lengths: ", length(eccentric), length(isometric), findPropulsiveEnd(accel$y,concentric), propulsiveEnd))
			}
		} else if(eccon=="e") {
			#not eccon="e" because not propulsive calculations on eccentric
		} else { #ecS
			#print("WARNING ECS\n\n\n\n\n")
		}
	}

	dynamics = getDynamics(encoderConfigurationName,
			speed$y, accel$y, massBody, massExtra, exercisePercentBodyWeight, gearedDown, anglePush, angleWeight,
			displacement, diameter, diameterExt, inertiaMomentum, smoothing)
	mass = dynamics$mass
	force = dynamics$force
	power = dynamics$power


	if( isPropulsive && ( eccon== "c" || eccon == "ec" ) )
		return(list(speedy=speed$y[1:propulsiveEnd], accely=accel$y[1:propulsiveEnd], 
			    force=force[1:propulsiveEnd], power=power[1:propulsiveEnd], mass=mass))
	else
		return(list(speedy=speed$y, accely=accel$y, force=force, power=power, mass=mass))
}

findECPhases <- function(displacement,speed) {
	speed.ext=extrema(speed)
	#print(speed.ext)
	#print(speed)
	
	#In all the extrema minindex values, search which range (row) has the min values,
	#and in this range search last value
	#print("searchMinSpeedEnd")
	searchMinSpeedEnd = max(which(speed == min(speed)))
	#print(searchMinSpeedEnd)
	
	#In all the extrema maxindex values, search which range (row) has the max values,
	#and in this range search first value
	#print("searchMaxSpeedIni")
	searchMaxSpeedIni = min(which(speed == max(speed)))
	#print(searchMaxSpeedIni)
	
	#find the cross between both
	#print("speed.ext-Cross")
	#print(speed.ext$cross[,1])
	#print("search min cross: crossMinRow")
	crossMinRow=which(speed.ext$cross[,1] > searchMinSpeedEnd & speed.ext$cross[,1] < searchMaxSpeedIni)
	#print(crossMinRow)
			
	#if (length(crossMinRow) > 0) {
	#	print(crossMinRow)
	#} else {
	#	propulsiveEnd = length(displacement)
	#	errorSearching = TRUE
	#}
	
	eccentric = 0
	isometric = 0
	concentric = 0

	#temporary fix problem of found MinSpeedEnd at right
	if(searchMinSpeedEnd > searchMaxSpeedIni)
		return(list(
			    eccentric=0,
			    isometric=0,
			    concentric=0))


	isometricUse = TRUE

	#print("at findECPhases")

	if(isometricUse) {
		eccentric=1:min(speed.ext$cross[crossMinRow,1])
		isometric=min(speed.ext$cross[crossMinRow,1]+1):max(speed.ext$cross[crossMinRow,2])
		concentric=max(speed.ext$cross[crossMinRow,2]+1):length(displacement)
	} else {
		eccentric=1:mean(speed.ext$cross[crossMinRow,1])
		#isometric=mean(speed.ext$cross[crossMinRow,1]+1):mean(speed.ext$cross[crossMinRow,2])
		concentric=mean(speed.ext$cross[crossMinRow,2]+1):length(displacement)
	}
	return(list(
		eccentric=eccentric,
		isometric=isometric,
		concentric=concentric))
}

findPropulsiveEnd <- function(accel, concentric, maxSpeedTInConcentric,
			     encoderConfigurationName, anglePush, angleWeight, 
			     massBody, massExtra, exercisePercentBodyWeight) {

	propulsiveEndsAt <- -g

	if(encoderConfigurationName == "LINEARONPLANE") {
		#propulsive phase ends at: -g*sin(alfa)
		propulsiveEndsAt <- -g * sin(anglePush * pi / 180)
	} else if(encoderConfigurationName == "LINEARONPLANEWEIGHTDIFFANGLE") {
		massBodyUsed <- getMassBodyByExercise(massBody, exercisePercentBodyWeight)
		
		#propulsive phase ends at: [massBodyUsed*sin(anglePush) + massExtra*sin(angleWeight)] / (massBodyUsed + massExtra)
		propulsiveEndsAt <- -g * (massBodyUsed * sin(anglePush * pi / 180) + massExtra * sin(angleWeight * pi / 180)) / (massBodyUsed + massExtra)
	}

	if(length(which(accel[concentric] <= propulsiveEndsAt)) > 0) {
		#this:
		#	propulsiveEnd = min(which(accel[concentric] <= -g))
		#can be a problem because some people does an strange countermovement at start of concentric movement
		#this people moves arms down and legs go up
		#at this moment acceleration can be lower than -g
		#if this happens, propulsiveEnd will be very early and detected jump will be very high
		#is exactly the same problem than findTakeOff, see that method for further help
		#another option can be using extrema

		accelCon = accel[concentric]
		df=data.frame(accelCon <= propulsiveEndsAt, accelCon, abs(1:length(accelCon)-maxSpeedTInConcentric))
		colnames(df)=c("belowG","accel","dist")
		df2 = subset(df,subset=df$belowG)
	
		df2row = min(which(df2$dist == min(df2$dist)))
		propulsiveEnd = as.integer(rownames(df2)[df2row])
	}
	else
		propulsiveEnd = length(concentric)
	
return (propulsiveEnd)
}

pafGenerate <- function(eccon, kinematics, massBody, massExtra) {
	#print("speed$y")
	#print(kinematics$speedy)

	meanSpeed <- mean(kinematics$speedy)
	#max speed and max speed time can be at eccentric or concentric
	maxSpeed <- max(abs(kinematics$speedy))
	maxSpeedT <- min(which(abs(kinematics$speedy) == maxSpeed))

	if(eccon == "c")
		meanPower <- mean(kinematics$power)
	else
		meanPower <- mean(abs(kinematics$power))

	#print(c("eccon meanPowerSigned meanPowerABS",eccon, mean(kinematics$power), mean(abs(kinematics$power))))
	#print("kinematics$power")
	#print(abs(kinematics$power))

	peakPower <- max(abs(kinematics$power))
	peakPowerT <- min(which(abs(kinematics$power) == peakPower))
	
	#print(which(abs(kinematics$power) == peakPower))

	pp_ppt <- peakPower / (peakPowerT/1000)	# ms->s
	meanForce <- mean(kinematics$force)
	maxForce <- max(abs(kinematics$force))

	#here paf is generated
	#mass is not used by pafGenerate, but used by Kg/W (loadVSPower)
	#meanForce and maxForce are not used by pafGenerate, but used by F/S (forceVSSpeed)
	return(data.frame(
			  meanSpeed, maxSpeed, maxSpeedT,
			  meanPower, peakPower, peakPowerT, pp_ppt,
			  meanForce, maxForce,
			  kinematics$mass, massBody, massExtra)) #kinematics$mass is Load
}

isInertial <- function(encoderConfigurationName) {
	if(encoderConfigurationName == "LINEARINERTIAL" ||
	   encoderConfigurationName == "ROTARYFRICTIONSIDEINERTIAL" ||
	   encoderConfigurationName == "ROTARYFRICTIONAXISINERTIAL" ||
	   encoderConfigurationName == "ROTARYAXISINERTIAL") 
		return(TRUE)
	else
		return(FALSE)
}

getDynamics <- function(encoderConfigurationName,
			speed, accel, massBody, massExtra, exercisePercentBodyWeight, gearedDown, anglePush, angleWeight,
			displacement, diameter, diameterExt, inertiaMomentum, smoothing)
{
	mass = getMassByEncoderConfiguration (encoderConfigurationName, massBody, massExtra, exercisePercentBodyWeight, gearedDown, anglePush, angleWeight)

	if(isInertial(encoderConfigurationName))
		return (getDynamicsInertial(encoderConfigurationName, displacement, diameter, diameterExt, mass, inertiaMomentum, smoothing))
	else 
		return (getDynamicsNotInertial (encoderConfigurationName, speed, accel, mass, exercisePercentBodyWeight, gearedDown, anglePush, angleWeight))
}

#mass extra can be connected to body or connected to a pulley depending on encoderConfiguration
getDynamicsNotInertial <- function(encoderConfigurationName, speed, accel, mass, exercisePercentBodyWeight, gearedDown, anglePush, angleWeight) 
{
	force <- mass*(accel+g)	#g:9.81 (used when movement is against gravity)

	power <- force*speed

	return(list(mass=mass, force=force, power=power))
}

#d: diameter axis
#D: diameter external (disc)
#angle: angle (rotation of disc) in radians
#angleSpeed: speed of angle
#angleAccel: acceleration of angle
#encoderConfiguration:
#  LINEARINERTIAL Linear encoder on inertial machine (rolled on axis)
#  ROTARYFRICTIONSIDEINERTIAL Rotary friction encoder connected to inertial machine on the side of the disc
#  ROTARYFRICTIONAXISINERTIAL Rotary friction encoder connected to inertial machine on the axis
#  ROTARYAXISINERTIAL Rotary axis encoder  connected to inertial machine on the axis

getDynamicsInertial <- function(encoderConfigurationName, displacement, d, D, mass, inertiaMomentum, smoothing)
{
	speed = getSpeed(displacement, smoothing) #mm/ms == m/s 

	# accel will be:
	accel = getAcceleration(speed) 

	accel$y = accel$y * 1000 # mm/ms² -> m/s²
	
	#use the values
	speed = speed$y
	accel = accel$y
	
	#----------------------
	#let's work with SI now
	#----------------------

	position.m = abs(cumsum(displacement)) / 1000 #m
	d.m = d / 100 #cm -> m
	D.m = D / 100 #cm -> m

	angle = position.m * 2 / d.m
	angleSpeed = speed * 2 / d.m
	angleAccel = accel * 2 / d.m

	force = abs(inertiaMomentum * angleAccel) * (2 / d.m) + mass * (accel + g)
	power = abs((inertiaMomentum * angleAccel) * angleSpeed) + mass * (accel + g) * speed
	powerBody = mass * (accel + g) * speed
	powerWheel = abs((inertiaMomentum * angleAccel) * angleSpeed)

	#print(c("displacement",displacement))
	#print(c("displacement cumsum",cumsum(displacement)))
	#print(c("inertia momentum",inertiaMomentum))
        #print(c("d",d))
        #print(c("mass",mass))
        #print(c("g",g))
        #print(c("max angle",max(abs(angle))))
	#print(c("max angleSpeed",max(abs(angleSpeed))))
	#print(c("max angleAccel",max(abs(angleAccel))))
	#print(c("speed",speed))
	#print(c("accel",accel))
	#print(c("max force",max(abs(force))))
	#print(c("max power at inertial",max(abs(power))))
	#print(c("powerBody",powerBody[1000]))
	#print(c("powerWheel",powerWheel[1000]))
	#print(c("d.m, D.m", d.m, D.m))
	#print(c("max angle, max pos", max(angle), max(position.m)))

	return(list(displacement=displacement, mass=mass, force=force, power=power))
}
