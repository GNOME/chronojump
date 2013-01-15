# 
#  This file is part of ChronoJump
# 
#  ChronoJump is free software; you can redistribute it and/or modify
#   it under the terms of the GNU General Public License as published by
#    the Free Software Foundation; either version 2 of the License, or   
#     (at your option) any later version.
#     
#  ChronoJump is distributed in the hope that it will be useful,
#   but WITHOUT ANY WARRANTY; without even the implied warranty of
#    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
#     GNU General Public License for more details.
# 
#  You should have received a copy of the GNU General Public License
#   along with this program; if not, write to the Free Software
#    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
# 
#   Copyright (C) 2004-2012   Xavier de Blas <xaviblas@gmail.com> 
# 


#this will replace below methods: findPics1ByMinindex, findPics2BySpeed
findCurves <- function(rawdata, eccon, min_height, draw, title) {
	a=cumsum(rawdata)
	b=extrema(a)
	
	start=0; end=0; startH=0
	tempStart=0; tempEnd=0;
	#TODO: fer algo per si no es detecta el minindex previ al salt
	if(eccon=="c") {
		if(length(b$minindex)==0) { b$minindex=cbind(1,1) }
		if(length(b$maxindex)==0) { b$maxindex=cbind(length(a),length(a)) }
		#fixes if 1st minindex is after 1st maxindex
		if(b$maxindex[1] < b$minindex[1]) { b$minindex = rbind(c(1,1),b$minindex) } 
		row=1; i=1; j=1
		while(max(c(i,j)) <= min(c(length(b$minindex[,1]),length(b$maxindex[,1])))) {
			tempStart = mean(c(b$minindex[i,1],b$minindex[i,2]))
			tempEnd = mean(c(b$maxindex[j,1],b$maxindex[j,2]))
			height=a[tempEnd]-a[tempStart]
#			print(paste(height,i,j))
			if(height >= min_height) { 
				start[row] = tempStart
				end[row]   = tempEnd
				startH[row]= a[b$minindex[i,1]]		#height at start
				row=row+1;
#				if(eccon=="c") { break } #c only needs one result
			} 
			i=i+1; j=j+1
		}
	} else { #ec, and ec-rep
		row=1; i=1; j=2
		#when saved a row with ec-con, and there's only this curve, extrema doesn't find maxindex
		if(length(b$maxindex) == 0) {
			start[1] =1
			end[1]   = mean(which(a == min(a)))
			startH[1]=a[1]
			start[2] =end[1]+1
			end[2]   =length(a)
			startH[2]=a[start[2]]
		}
		while(j <= length(b$maxindex[,1])) {
			tempStart = mean(c(b$maxindex[i,1],b$maxindex[i,2]))
			tempEnd   = mean(c(b$maxindex[j,1],b$maxindex[j,2]))
			bottom=min(a[tempStart:tempEnd]) #find min value between the two tops
			mintop=min(c(a[tempStart],a[tempEnd])) #find wich top is lower
			height=mintop-bottom
			if(height >= min_height) { 
				if(eccon == "ecS") {
					start[row] = tempStart
					end[row]   = mean(which(a[tempStart:tempEnd] == bottom) + tempStart)
					startH[row] = a[b$maxindex[i,1]]		#height at start
					row=row+1
					start[row] = end[(row-1)] + 1
					end[row]   = tempEnd
					startH[row] = a[start[row]]		#height at start
					row=row+1
					i=j
				} else {
					start[row] = tempStart
					end[row]   = tempEnd
					startH[row] = a[b$maxindex[i,1]]		#height at start
					row=row+1
					i=j
				}
			} else {
				if(a[tempEnd] >= a[tempStart]) {
					i=j
				}
			}
			j=j+1
		}
	}
	if(draw) {
		plot(a/10,type="l",xlim=c(1,length(a)),xlab="",ylab="",axes=T) #/10 mm -> cm
		title(title, cex.main=1, font.main=1)
		mtext("time (ms) ",side=1,adj=1,line=-1)
		mtext("height (cm) ",side=2,adj=1,line=-1)
		abline(v=b$maxindex,lty=3); abline(v=b$minindex,lty=3)
	}
	return(as.data.frame(cbind(start,end,startH)))
}


#based on findPics2BySpeed
#only used in eccon "c"
#if this changes, change also in python capture file
reduceCurveBySpeed <- function(eccon, row, startT, rawdata, smoothing) {
	a=rawdata
	speed <- smooth.spline( 1:length(a), a, spar=smoothing) 
	b=extrema(speed$y)

	#find the b$cross at left of max speed
	x.ini=1
	
	#from searchValue, go to the left, searchValue is at max speed on going up
	#but is min speed on going down (this happens when not "concentric" and when phase is odd (impar)
	searchValue = max(speed$y)
	if(eccon == "ec")
		searchValue = min(speed$y)
	else if(eccon == "ecS" & row%%2 == 1)
		searchValue = min(speed$y)

	maxSpeedT <- min(which(speed$y == searchValue))
	
	for(i in b$cross[,2]) 		{ if(i < maxSpeedT) { x.ini = i } } #left adjust

	return(startT+x.ini)
}

#go here with every single jump
kinematicsF <- function(a, mass, smoothingOne, g) {
	speed <- smooth.spline( 1:length(a), a, spar=smoothingOne)
	accel <- predict( speed, deriv=1 )
	#speed comes in mm/ms when derivate to accel its mm/ms^2 to convert it to m/s^2 need to *1000 because it's quadratic
	accel$y <- accel$y * 1000 

#	force <- mass*accel$y
#	if(isJump)
		force <- mass*(accel$y+g)	#g:9.81 (used when movement is against gravity)

	power <- force*speed$y

	return(list(speedy=speed$y, accely=accel$y, force=force, power=power, mass=mass))
}

powerBars <- function(kinematics) {
	meanSpeed <- mean(abs(kinematics$speedy))
	maxSpeed <- max(abs(kinematics$speedy))
	meanPower <- mean(abs(kinematics$power))
	peakPower <- max(abs(kinematics$power))
	peakPowerT <- min(which(abs(kinematics$power) == peakPower))
	pp_ppt <- peakPower / (peakPowerT/1000)	# ms->s
	meanForce <- mean(abs(kinematics$force))
	maxForce <- max(abs(kinematics$force))

	#here paf is generated
	#mass is not used by powerBars, but used by Kg/W (loadVSPower)
	#meanForce and maxForce are not used by powerBars, but used by F/S (forceVSSpeed)
	return(data.frame(meanSpeed, maxSpeed, meanPower,peakPower,peakPowerT,pp_ppt, kinematics$mass,meanForce,maxForce))
}

kinematicRanges <- function(singleFile,rawdata,curves,mass,smoothingOne,g) {
	n=length(curves[,1])
	maxSpeedy=0;maxForce=0;maxPower=0
	for(i in 1:n) { 
		myMass = mass
		mySmoothingOne = smoothingOne
		if(! singleFile) {
			myMass = curves[i,5]
			mySmoothingOne = curves[i,6]
		}
		kn=kinematicsF(rawdata[curves[i,1]:curves[i,2]],myMass,mySmoothingOne,g)
		if(max(abs(kn$speedy)) > maxSpeedy)
			maxSpeedy = max(abs(kn$speedy))
		if(max(abs(kn$force)) > maxForce)
			maxForce = max(abs(kn$force))
		if(max(abs(kn$power)) > maxPower)
			maxPower = max(abs(kn$power))
	}
	return(list(
		speedy=c(-maxSpeedy,maxSpeedy),
		force=c(-maxForce,maxForce),
		power=c(-maxPower,maxPower)))
}

paint <- function(rawdata, eccon, xmin, xmax, yrange, knRanges, superpose, highlight,
	startX, startH, smoothing, mass, title, draw, showLabels, marShrink, showAxes, legend) {
	#eccons ec and ec-rep is the same here (only show one curve)
	#receive data as cumulative sum
	lty=c(1,1,1)

	rawdata=rawdata[xmin:xmax]
	a=cumsum(rawdata)
	a=a+startH

	#all in meters
	#a=a/1000

	if(draw) {
		#three vertical axis inspired on http://www.r-bloggers.com/multiple-y-axis-in-a-r-plot/
		par(mar=c(5, 4, 4, 8))
			if(marShrink) #used on "side" compare
				par(mar=c(1, 1, 4, 1))
	
		#plot distance
		#plot(a,type="h",xlim=c(xmin,xmax),xlab="time (ms)",ylab="Left: distance (mm); Right: speed (m/s), accelration (m/s^2)",col="gray", axes=F) #this shows background on distance (nice when plotting distance and speed, but confusing when there are more variables)
		xlab="";ylab="";
		if(showLabels) {
			xlab="time (ms)"
			ylab="Left: distance (mm); Right: speed (m/s), force (N), power (W)"
		}
		ylim=yrange
		if(ylim[1]=="undefined") { ylim=NULL }
		plot(a-min(a),type="n",xlim=c(1,length(a)),ylim=ylim,xlab=xlab, ylab=ylab, col="gray", axes=F, main=title)
		if(showAxes) {
			axis(1) 	#can be added xmin
			axis(2)
		}
		
		par(new=T)
		colNormal="black"
		if(superpose)
			colNormal="gray30"
		yValues = a[startX:length(a)]-min(a[startX:length(a)])
		if(highlight==FALSE) {
			plot(startX:length(a),yValues,type="l",xlim=c(1,length(a)),ylim=ylim,xlab="",ylab="",col="black",lty=lty[1],lwd=2,axes=F)
			par(new=T)
			plot(startX:length(a),yValues,type="h",xlim=c(1,length(a)),ylim=ylim,xlab="",ylab="",col="grey90",lty=lty[1],lwd=1,axes=F)
		}
		else
			plot(startX:length(a),yValues,type="l",xlim=c(1,length(a)),ylim=ylim,xlab="",ylab="",col=colNormal,lty=2,lwd=3,axes=F)
		abline(h=0,lty=3,col="black")

		#abline(v=seq(from=0,to=length(a),by=500),lty=3,col="gray")
	}

	#speed
	#scan file again (raw data: mm displaced every ms, no cumulative sum)
	a=rawdata
	speed <- smooth.spline( 1:length(a), a, spar=smoothing) 
	if(draw) {
		ylim=c(-max(abs(range(a))),max(abs(range(a))))	#put 0 in the middle 
		if(knRanges[1] != "undefined")
			ylim = knRanges$speedy
		par(new=T)
		if(highlight==FALSE)
			plot(startX:length(speed$y),speed$y[startX:length(speed$y)],type="l",xlim=c(1,length(a)),ylim=ylim,xlab="",ylab="",col=cols[1],lty=lty[1],lwd=1,axes=F)
		else
			plot(startX:length(speed$y),speed$y[startX:length(speed$y)],type="l",xlim=c(1,length(a)),ylim=ylim,xlab="",ylab="",col="darkgreen",lty=2,lwd=3,axes=F)
		if(showAxes) {
			axis(4, col=cols[1], lty=lty[1], line=0, padj=-.5)
			abline(h=0,lty=3,col="black")
		}
		#mtext(text=paste("max speed:",round(max(speed$y),3)),side=3,at=which(speed$y == max(speed$y)),cex=.8,col=cols[1])
	}

	#show extrema values in speed
	b=extrema(speed$y)
	#if(draw & !superpose) 
	#	segments(x0=b$maxindex,y0=0,x1=b$maxindex,y1=speed$y[b$maxindex],col=cols[1])

	#declare variables:
	concentric=0
	eccentric=0
	if(eccon=="c") {
		concentric=1:length(a)
	} else {	#"ec", "ec-rep"
		crossSpeedInMiddle = b$cross[,1]
		crossDownToUp=0
		count=1

		#the -2 is to know if speed goes from down to up or vicerversa
		#we use -2 to know the 2ms before
		#maybe data has not started, then look what happens 2 ms later
		for(i in crossSpeedInMiddle) {
			if(i>2) {
				if(speed$y[(i-2)]<0) {
					crossDownToUp[count]=i
					count=count+1
				}
			} else {
				if(speed$y[(i+2)]>0) {
					crossDownToUp[count]=i
					count=count+1
				}
			}
		}
		eccentric=1:min(crossDownToUp)
		concentric=max(crossDownToUp):length(a)
		if(draw) {
			abline(v=min(crossDownToUp),col=cols[1])
			abline(v=max(crossDownToUp),col=cols[1])
			mtext(text=min(crossDownToUp),side=1,at=min(crossDownToUp),cex=.8,col=cols[1])
			mtext(text=max(crossDownToUp),side=1,at=max(crossDownToUp),cex=.8,col=cols[1])
			mtext(text="eccentric ",side=3,at=min(crossDownToUp),cex=.8,adj=1,col=cols[1],line=.5)
			mtext(text=" concentric ",side=3,at=max(crossDownToUp),cex=.8,adj=0,col=cols[1],line=.5)
		}
	}

	accel <- predict( speed, deriv=1 )
	#speed comes in mm/ms when derivate to accel its mm/ms^2 to convert it to m/s^2 need to *1000 because it's quadratic
	accel$y <- accel$y * 1000
	
	#print(accel$y)
	#alternative R method (same result)
	#accel2 <- D1ss( 1:length(speed$y), speed$y )
	#accel2 <- accel2 * 1000
	#print(accel2)

	if(draw) {
		#propulsive phase ends when accel is -9.8
		if(length(which(accel$y[concentric]<=-g)) > 0) {
			propulsiveEnds = min(which(accel$y[concentric]<=-g))
		} else {
			propulsiveEnds=length(concentric)
		}

		#mean speed propulsive in concentric
		myMeanSpeed = mean(speed$y[concentric[1]:length(concentric)])
		myMeanSpeedRight = length(concentric)
		
		if(eccon != "c") {
			propulsiveEnds = propulsiveEnds + concentric[1]
			myMeanSpeedRight = length(eccentric) + length(concentric)
		}

		if(analysisOptions == "p") {
			myMeanSpeed = mean(speed$y[concentric[1]:propulsiveEnds])
			myMeanSpeedRight = propulsiveEnds
		}
		arrows(x0=min(concentric),y0=myMeanSpeed,x1=myMeanSpeedRight,y1=myMeanSpeed,col=cols[1],code=3)
		mtext(paste("mean speed:",round(myMeanSpeed,3)),side=2,at=myMeanSpeed,line=-1.8,col=cols[1],cex=.8,padj=0)

		ylim=c(-max(abs(range(accel$y))),max(abs(range(accel$y))))	 #put 0 in the middle
		#if(knRanges[1] != "undefined")
		#	ylim = knRanges$force
		par(new=T)
		if(highlight==FALSE)
			plot(startX:length(accel$y),accel$y[startX:length(accel$y)],type="l",xlim=c(1,length(a)),ylim=ylim,xlab="",ylab="",col="magenta",lty=lty[2],lwd=1,axes=F)
		else
			plot(startX:length(accel$y),accel$y[startX:length(accel$y)],type="l",xlim=c(1,length(a)),ylim=ylim,xlab="",ylab="",col="darkblue",lty=2,lwd=3,axes=F)
			
		#propulsive stuff
		abline(h=-g,lty=3,col="magenta")
		abline(v=propulsiveEnds,lty=3,col="magenta") 
		points(propulsiveEnds, -g, col="magenta")
		
		if(showAxes)
			axis(4, col="magenta", lty=lty[1], line=2, padj=-.5)
		#mtext(text=paste("max accel:",round(max(accel$y),3)),side=3,at=which(accel$y == max(accel$y)),cex=.8,col=cols[1],line=2)
	}

#print(c(knRanges$accely, max(accel$y), min(accel$y)))
#	force <- mass*accel$y
#	if(isJump)
		force <- mass*(accel$y+g)	#g:9.81 (used when movement is against gravity)

#print("MAXFORCE!!!!!")
#print(max(force))

	if(draw) {
		ylim=c(-max(abs(range(force))),max(abs(range(force))))	 #put 0 in the middle
		if(knRanges[1] != "undefined")
			ylim = knRanges$force
		par(new=T)
		if(highlight==FALSE)
			plot(startX:length(force),force[startX:length(force)],type="l",xlim=c(1,length(a)),ylim=ylim,xlab="",ylab="",col=cols[2],lty=lty[2],lwd=1,axes=F)
		else
			plot(startX:length(force),force[startX:length(force)],type="l",xlim=c(1,length(a)),ylim=ylim,xlab="",ylab="",col="darkblue",lty=2,lwd=3,axes=F)
		if(showAxes)
			axis(4, col=cols[2], lty=lty[2], line=4, padj=-.5)
	}

	
	#mark when it's air and land
	#if it was a eccon concentric-eccentric, will be useful to calculate flight time
	#but this eccon will be not done
	#if(draw & (!superpose || (superpose & highlight)) & isJump) {
	if(draw & (!superpose || (superpose & highlight)) & exercisePercentBodyWeight == 100) {
		weight=mass*9.81
		abline(h=weight,lty=1,col=cols[2]) #body force, lower than this, person in the air (in a jump)
		takeoff = max(which(force>=weight))
		abline(v=takeoff,lty=1,col=cols[2]) 
		mtext(text="land ",side=3,at=takeoff,cex=.8,adj=1,col=cols[2])
		mtext(text=" air ",side=3,at=takeoff,cex=.8,adj=0,col=cols[2])
		text(x=length(force),y=weight,labels="Weight (N)",cex=.8,adj=c(.5,0),col=cols[2])
		if(eccon=="ec") {
			landing = min(which(force>=weight))
			abline(v=landing,lty=1,col=cols[2]) 
			mtext(text="air ",side=3,at=landing,cex=.8,adj=1,col=cols[2])
			mtext(text=" land ",side=3,at=landing,cex=.8,adj=0,col=cols[2])
		}
	}
	#forceToBodyMass <- force - weight
	#b=extrema(forceToBodyMass)
	#abline(v=b$cross[,1],lty=3,col=cols[2]) #body force, lower than this, person in the air (in a jump)
	#text(x=(mean(b$cross[1,1],b$cross[1,2])+mean(b$cross[2,1],b$cross[2,2]))/2, y=weight, 
	#		labels=paste("flight time:", mean(b$cross[2,1],b$cross[2,2])-mean(b$cross[1,1],b$cross[1,2]),"ms"), 
	#		col=cols[2], cex=.8, adj=c(0.5,0))

	#power #normalment m=massa barra + peses: 	F=m*a #com es va contra gravetat: 		F=m*a+m*g  	F=m*(a+g) #g sempre es positiva. a es negativa en la baixada de manera que en caiguda lliure F=0 #cal afegir la resistencia del encoder a la força #Potència	P=F*V #si es treballa amb el pes corporal, cal afegir-lo

	#F=m*a		#bar
	#F=(m*a)+(m*g) #jump m*(a+g) F=m*0

	power <- force*speed$y
	if(draw) {
		ylim=c(-max(abs(range(power))),max(abs(range(power))))	#put 0 in the middle
		if(knRanges[1] != "undefined")
			ylim = knRanges$power
		par(new=T);
		if(highlight==FALSE)
			plot(startX:length(power),power[startX:length(power)],type="l",xlim=c(1,length(a)),ylim=ylim,xlab="",ylab="",col=cols[3],lty=lty[3],lwd=2,axes=F)
		else
			plot(startX:length(power),power[startX:length(power)],type="l",xlim=c(1,length(a)),ylim=ylim,xlab="",ylab="",col="darkred",lty=2,lwd=3,axes=F)
		if(showAxes) 
			axis(4, col=cols[3], lty=lty[3], line=6, lwd=2, padj=-.5)
	}

	#time to arrive to peak power
	peakPowerT=which(power == max(power))
	if(draw & !superpose) {
		abline(v=peakPowerT, col=cols[3])
		points(peakPowerT, max(power),col=cols[3])
		mtext(text=paste("peak power:",round(max(power),3)),side=3,at=peakPowerT,cex=.8,col=cols[3])
		mtext(text=peakPowerT,side=1,at=peakPowerT,cex=.8,col=cols[3])
	}
	#average power

	meanPowerE = 0
	if(eccon != "c") 
		meanPowerE = mean(abs(power[eccentric]))
	meanPowerC = mean(abs(power[concentric]))
	if(draw & !superpose & analysisOptions != "p") {
		if(eccon != "c") {
			arrows(x0=1,y0=meanPowerE,x1=max(eccentric),y1=meanPowerE,col=cols[3],code=3)
			#text(x=min(eccentric), y=meanPowerE, labels=expression(bar(x)), adj=c(1,0),cex=.8,col=cols[3])
			text(x=mean(eccentric), y=meanPowerE, labels=paste("mean power:",round(meanPowerE,3)), adj=c(0.5,0),cex=.8,col=cols[3])
			#mtext(paste("mean power:",round(meanPowerE,3)),side=4,at=meanPowerE,line=-2,col=cols[3],cex=.8)
		}
		arrows(x0=min(concentric),y0=meanPowerC,x1=max(concentric),y1=meanPowerC,col=cols[3],code=3)
		#text(x=min(concentric), y=meanPowerC, labels=expression(bar(x)), adj=c(1,0),cex=.8,col=cols[3])
		text(x=mean(concentric), y=meanPowerC, labels=paste("mean power:",round(meanPowerC,3)), adj=c(0.5,0),cex=.8,col=cols[3])
		#mtext(paste("mean power:",round(meanPowerC,3)),side=4,at=meanPowerC,line=-2,col=cols[3],cex=.8)
	}
		
	#propulsive phase ends when accel is -9.8
	if(draw & analysisOptions == "p") {
		#mean power propulsive in concentric
		meanPowerPropulsive = mean(power[concentric[1]:propulsiveEnds])
		arrows(x0=min(concentric),y0=meanPowerPropulsive,x1=propulsiveEnds,y1=meanPowerPropulsive,col=cols[3],code=3)
		mtext(paste("mean power:",round(meanPowerPropulsive,3)),side=4,at=meanPowerPropulsive,line=-2,col=cols[3],cex=.8)
	}

	#legend, axes and title
	if(draw) {
		if(legend & showAxes) {
			legendPos = "bottom"
			par(xpd=T)
			legend(legendPos, xjust=1, legend=c("Distance","","Speed","Accel.","Force","Power"), lty=c(1,0,1,1,1,1), 
					lwd=c(1,1,1,1,1,2), col=c("black","black",cols[1],"magenta",cols[2],cols[3]), cex=1, bg="white", ncol=6, inset=-.2)
			par(xpd=F)
			#mtext(text="[ESC: Quit; mouse left: Zoom in; mouse right: Zoom out]",side=3)
		}
		if(showLabels) {
			mtext("time (ms) ",side=1,adj=1,line=-1,cex=.9)
			mtext("height (mm) ",side=2,adj=1,line=-1,cex=.9)
		}
	}
}

paintPowerPeakPowerBars <- function(title, paf, myEccons, height) {
	pafColors=c("tomato1","tomato4",topo.colors(10)[3])
	myNums = rownames(paf)
	height = abs(height/10)
	
	if(eccon=="ecS") {
		if(singleFile) {
			myEc=c("c","e")
			myNums = as.numeric(rownames(paf))
			myNums = paste(trunc((myNums+1)/2),myEc[((myNums%%2)+1)],sep="")
		}
	}
	
	powerData=rbind(paf[,3], paf[,4])

	#put lowerY on power, but definetively, leave it at 0
	#lowerY=min(powerData)-100
	#if(lowerY < 0)
	#	lowerY = 0
	lowerY = 0
	
	par(mar=c(5, 4, 4, 5))
	bp <- barplot(powerData,beside=T,col=pafColors[1:2],width=c(1.4,.6),
			names.arg=paste(myNums,"\n",paf[,7],sep=""),xlim=c(1,n*3+.5),cex.name=0.9,
			xlab="",ylab="Power (W)", main=title,
			ylim=c(lowerY,max(powerData)), xpd=FALSE) #ylim, xpd = F,  makes barplot starts high (compare between them)
	mtext("Curve\nLoad",side=1,at=0,adj=1,line=1,cex=.9)
	par(new=T, xpd=T)
	#on ecS, concentric has high value of time to peak power and eccentric has it very low. Don't draw lines
	if(eccon=="ecS")
		plot(bp[2,],paf[,5],type="p",lwd=2,xlim=c(1,n*3+.5),ylim=c(0,max(paf[,5])),axes=F,xlab="",ylab="",col="blue", bg="lightblue",cex=1.5,pch=21)
	else
		plot(bp[2,],paf[,5],type="b",lwd=2,xlim=c(1,n*3+.5),ylim=c(0,max(paf[,5])),axes=F,xlab="",ylab="",col=pafColors[3])
	
	axis(4, col=pafColors[3], line=0,padj=-.5)
	mtext("Time to peak power (ms)", side=4, line=-1)
	
	par(new=T, xpd=T)
	plot(bp[2,],height,type="b",lwd=2,xlim=c(1,n*3+.5),ylim=c(0,max(height)),axes=F,xlab="",ylab="",col="green")

print(height)

	legend("bottom",col=c(pafColors,"green"), lty=c(0,0,1,1), lwd=c(1,1,2,2), pch=c(15,15,NA,NA), legend=c("Power","Peak Power", "Time to Peak Power    ", "Range"), ncol=4, inset=-.2)
	abline(h=max(height),lty=2, col="green")
	abline(h=min(height),lty=2, col="green")
#	text(max(bp[,2]),max(height),max(height),adj=c(0,.5),cex=0.8)
	axis(4, col="green", line=3, padj=-.5)
	mtext("Range (cm)", side=4, line=2)
}

#see paf for more info
findPosInPaf <- function(var, option) {
	pos = 0
	if(var == "Speed")
		pos = 1
	else if(var == "Power")
		pos = 3
	else if(var == "Load") #or Mass
		pos = 7
	else if(var == "Force")
		pos = 8
	if( ( var == "Speed" || var == "Power" || var == "Force") & option == "max")
		pos=pos+1
	return(pos)
}

#option: mean or max
paintCrossVariables <- function (paf, varX, varY, option, isAlone, title) {
	x = (paf[,findPosInPaf(varX, option)])
	y = (paf[,findPosInPaf(varY, option)])

	#problem with balls is that two values two close looks bad
	#suboption="balls"
	suboption="side"
	if(suboption == "balls") {
		cexBalls = 3
		cexNums = 1
		adjHor = 0.5
		nums=rownames(paf)
	} else if (suboption == "side") {
		cexBalls = 1.8
		cexNums = 1
		adjHor = 0
		nums=paste("  ", rownames(paf))
	}
	
	colBalls="blue"
	bgBalls="lightBlue"
	if(isAlone == "RIGHT") {
		colBalls="red"
		bgBalls="pink"
	}

	plot(x,y, xlab=varX, ylab="", pch=21,col=colBalls,bg=bgBalls,cex=cexBalls,axes=F)
	title(title, cex.main=1, font.main=1)
	text(x,y,nums,adj=c(adjHor,.5),cex=cexNums)

	#lines(smooth.spline(x,y,spar=.5),col="darkblue")
	
	#x vector should contain at least 4 different values
	if(length(unique(x)) >= 4)
		lines(smooth.spline(x,y,df=4),col=colBalls)

	if(isAlone == "ALONE") {
		axis(1)
		axis(2)
		mtext(varY, side=2, line=3)
		#box()
	} else if(isAlone == "LEFT") {
		axis(1)
		axis(2,col=colBalls)
		mtext(varY, side=2, line=3, col=colBalls)
		#box()
	} else { #"RIGHT"
		axis(4,col=colBalls)
		mtext(varY, side=4, line=3, col=colBalls)
	}
}
			
find.mfrow <- function(n) {
	if(n<=3) return(c(1,n))
	else if(n<=8) return(c(2,ceiling(n/2)))
	else return(c(3, ceiling(n/3)))
}

find.yrange <- function(singleFile, rawdata, curves) {
	if(singleFile) {
		a=cumsum(rawdata)
		y.max = max(a)
		y.min = min(a)
		if(y.min < 0) {
			y.max = y.max + -1*y.min
			y.min = 0
		}
		return (c(y.min,y.max))
	} else {
		n=length(curves[,1])
		y.max = 0
		y.min = 10000
		for(i in 1:n) { 
			y.current = cumsum(rawdata[curves[i,1]:curves[i,2]])
			if(max(y.current) > y.max)
				y.max = max(y.current)
			if(min(y.current) < y.min)
				y.min = min(y.current)
		}
		return (c(y.min,y.max))
	}
}

quitIfNoData <- function(n, curves, outputData1) {
	#if not found curves with this data, plot a "sorry" message and exit
	if(n == 1 & curves[1,1] == 0 & curves[1,2] == 0) {
		plot(0,0,type="n",axes=F,xlab="",ylab="")
		text(x=0,y=0,"Sorry, no curves matched your criteria.",cex=1.5)
		dev.off()
		write("", outputData1)
		quit()
	}
}

#TODO: capitalize first letter on all global variables names


#concentric, eccentric-concentric, repetitions of eccentric-concentric
#currently only used "c" and "ec". no need of ec-rep because c and ec are repetitive
#"ecS" is like ec but eccentric and concentric phases are separated, used in findCurves, this is good for treeview to know power... on the 2 phases
eccons=c("c","ec","ec-rep","ecS") 

g = 9.81
smoothingAll= 0.1
#file="data_falla.txt"; isJump=FALSE #TODO em sembla que falla perque no hi ha cap curve, pq totes son mes petites que minHeight

colSpeed="springgreen3"; colForce="blue2"; colPower="tomato2"	#colors
#colSpeed="black"; colForce="black"; colPower="black"		#black & white
cols=c(colSpeed,colForce,colPower); lty=rep(1,3)	

#--- user commands ---
args <- commandArgs(TRUE)
print(args)
if(length(args) < 3) {
#	print("USAGE:\nRscript graph.R c superpose graph.png\neccons:curves, single, side, superpose, powerBars \nsingle and superpose needs a param at end (the jump):\nRscript graph.R c single graph.png 2\n")
} else {
	file=args[1]
	outputGraph=args[2]
	outputData1=args[3]
	outputData2=args[4] #currently used to display status
	minHeight=as.numeric(args[5])*10 #from cm to mm
	exercisePercentBodyWeight=as.numeric(args[6])	#was isJump=as.logical(args[6])
	Mass=as.numeric(args[7])
	eccon=args[8]
	analysis=args[9]	#in cross comes as "cross.Force.Speed.mean"
	analysisOptions=args[10]	#p: propulsive
	smoothingOne=args[11]
	jump=args[12]
	width=as.numeric(args[13])
	height=as.numeric(args[14])
	Title=args[15]

	write("(1/4) Starting R", outputData2)

	library("EMD")
	#library("sfsmisc")

	write("(2/4) Starting process", outputData2)

	if(analysis != "exportCSV") {
		png(outputGraph, width=width, height=height)
		Title=gsub('_',' ',Title)
		Title=gsub('-','    ',Title)
	}

	titleType = "n"
	#if(isJump)
	#	titleType="jump"
	
	curvesPlot = FALSE
	if(analysis=="curves") {
		curvesPlot = TRUE
		par(mar=c(2,2.5,2,1))
	}

	singleFile = TRUE
	if(nchar(file) >= 40) {
		#file="/tmp...../chronojump-encoder-graph-input-multi.csv"
		#substr(file, nchar(file)-39, nchar(file))
		#[1] "chronojump-encoder-graph-input-multi.csv"
		if(substr(file, nchar(file)-39, nchar(file)) == "chronojump-encoder-graph-input-multi.csv") {
			singleFile = FALSE
		}
	}
	
	if(! singleFile) {
		#this produces a rawdata, but note that a cumsum(rawdata) cannot be done because:
		#this are separated movements
		#maybe all are concentric (there's no returning to 0 phase)

		#this version of curves has added specific data cols:
		#status, exerciseName, mass, smoothingOne, dateTime, myEccon

		inputMultiData=read.csv(file=file,sep=",",stringsAsFactors=F)

		rawdata = NULL
		count = 1
		start = NULL; end = NULL; startH = NULL
		status = NULL; id = NULL; exerciseName = NULL; mass = NULL; smooth = NULL; dateTime = NULL; myEccon = NULL
		curvesHeight = NULL
		newLines=0;
		countLines=1; #useful to know the correct ids of active curves
		for(i in 1:length(inputMultiData[,1])) { 
			#plot only active curves
			status = as.vector(inputMultiData$status[i])
			if(status != "active") {
				newLines=newLines-1; 
				countLines=countLines+1;
				next;
			}

			dataTempFile=scan(file=as.vector(inputMultiData$fullURL[i]),sep=",")
			dataTempPhase=dataTempFile
			processTimes = 1
			changePos = 0
			#if this curve is ecc-con and we want separated, divide the curve in two
			if(as.vector(inputMultiData$eccon[i]) != "c" & eccon =="ecS") {
				changePos = mean(which(cumsum(dataTempFile) == min(cumsum(dataTempFile))))
				processTimes = 2
			}
			for(j in 1:processTimes) {
				if(processTimes == 2) {
					if(j == 1) {
						dataTempPhase=dataTempFile[1:changePos]
					} else {
						#IMP: 
						#note that following line without the parentheses on changePos+1
						#gives different data.
						#never forget parentheses to operate inside the brackets
						dataTempPhase=dataTempFile[(changePos+1):length(dataTempFile)]
						newLines=newLines+1
					}
				}
				rawdata = c(rawdata, dataTempPhase)
				id[(i+newLines)] = countLines
				start[(i+newLines)] = count
				end[(i+newLines)] = length(dataTempPhase) + count -1
				startH[(i+newLines)] = 0
				exerciseName[(i+newLines)] = as.vector(inputMultiData$exerciseName[i])
				mass[(i+newLines)] = inputMultiData$mass[i]
				smooth[(i+newLines)] = inputMultiData$smoothingOne[i]
				dateTime[(i+newLines)] = as.vector(inputMultiData$dateTime[i])

				curvesHeight[(i+newLines)] = sum(dataTempPhase)

				if(processTimes == 2) {
					if(j == 1) {
						myEccon[(i+newLines)] = "e"
						id[(i+newLines)] = paste(countLines, myEccon[(i+newLines)], sep="")
					} else {
						myEccon[(i+newLines)] = "c"
						id[(i+newLines)] = paste(countLines, myEccon[(i+newLines)], sep="")
						countLines = countLines + 1
					}
				} else {
					if(inputMultiData$eccon[i] == "c")
						myEccon[(i+newLines)] = "c"
					else
						myEccon[(i+newLines)] = "ec"
					countLines = countLines + 1
				}

				count = count + length(dataTempPhase)
			}
		}		
		
		#rawdata.cumsum=cumsum(rawdata)

		#curves = data.frame(id,start,end,startH,exerciseName,mass,smooth,dateTime,myEccon,stringsAsFactors=F,row.names=1)
		#this is a problem when there's only one row as seen by the R code of data.frame. ?data.frame:
		#"If row names are supplied of length one and the data frame has a
     		#single row, the ‘row.names’ is taken to specify the row names and
     		#not a column (by name or number)."
		#then a column id is created when there's only on row, but it is not created there's more than one.
		#solution:
		if(length(id)==1) {
			curves = data.frame(start,end,startH,exerciseName,mass,smooth,dateTime,myEccon,stringsAsFactors=F,row.names=id)
		} else {
			curves = data.frame(id,start,end,startH,exerciseName,mass,smooth,dateTime,myEccon,stringsAsFactors=F,row.names=1)
		}

		n=length(curves[,1])
		quitIfNoData(n, curves, outputData1)
	} else {
		rawdata=scan(file=file,sep=",")

		if(length(rawdata)==0) {
			plot(0,0,type="n",axes=F,xlab="",ylab="")
			text(x=0,y=0,"Encoder is not connected.",cex=1.5)
			dev.off()
			write("", outputData1)
			quit()
		}

		rawdata.cumsum=cumsum(rawdata)
	
		curves=findCurves(rawdata, eccon, minHeight, curvesPlot, Title)
		print(curves)
		n=length(curves[,1])
		quitIfNoData(n, curves, outputData1)
		
		for(i in 1:n) { 
			curves[i,1]=reduceCurveBySpeed(eccon, i, curves[i,1],rawdata[curves[i,1]:curves[i,2]], smoothingOne)
		}
		if(curvesPlot) {
			#/10 mm -> cm
			for(i in 1:length(curves[,1])) { 
				myLabel = i
				myY = min(rawdata.cumsum)/10
				adjVert = 0
				if(eccon=="ecS") {
					myEc=c("c","e")
					myLabel = paste(trunc((i+1)/2),myEc[((i%%2)+1)],sep="")
					myY = rawdata.cumsum[curves[i,1]]/10
					if(i%%2 == 1) {
						adjVert = 1
					}
				}
				text(x=(curves[i,1]+curves[i,2])/2,y=myY,labels=myLabel, adj=c(0.5,adjVert),cex=1,col="blue")
				arrows(x0=curves[i,1],y0=myY,x1=curves[i,2],y1=myY,
						col="blue",code=3,length=0.1)
			}
		}
	}
	
	write("(3/4) Curves processed", outputData2)
		
	if(analysis=="single") {
		if(jump>0) {
			myMass = Mass
			mySmoothingOne = smoothingOne
			myEccon = eccon
			myStart = curves[jump,1]
			myEnd = curves[jump,2]
			if(! singleFile) {
				myMass = curves[jump,5]
				mySmoothingOne = curves[jump,6]
				myEccon = curves[jump,8]
			}
			paint(rawdata, myEccon, myStart, myEnd,"undefined","undefined",FALSE,FALSE,
					1,curves[jump,3],mySmoothingOne,myMass,
					paste(Title, " ", analysis, " ", myEccon, " ", titleType, " ", jump,
					      " (smoothing: ",mySmoothingOne,")",sep=""),
					TRUE,	#draw
					TRUE,	#showLabels
					FALSE,	#marShrink
					TRUE,	#showAxes
					TRUE	#legend
					)	
		}
	}

	if(analysis=="side") {
		#comparar 6 salts, falta que xlim i ylim sigui el mateix
		par(mfrow=find.mfrow(n))

		#a=cumsum(rawdata)
		#yrange=c(min(a),max(a))
		yrange=find.yrange(singleFile, rawdata, curves)

		knRanges=kinematicRanges(singleFile,rawdata,curves,Mass,smoothingOne,g)

		for(i in 1:n) {
			myMass = Mass
			mySmoothingOne = smoothingOne
			myEccon = eccon
			if(! singleFile) {
				myMass = curves[i,5]
				mySmoothingOne = curves[i,6]
				myEccon = curves[i,8]
			}
			paint(rawdata, myEccon, curves[i,1],curves[i,2],yrange,knRanges,FALSE,FALSE,
				1,curves[i,3],mySmoothingOne,myMass,paste(Title, " ", titleType,rownames(curves)[i]),
				TRUE,	#draw
				FALSE,	#showLabels
				TRUE,	#marShrink
				FALSE,	#showAxes
				FALSE	#legend
				)
		}
		par(mfrow=c(1,1))
	}
	if(analysis=="superpose") {	#TODO: fix on ec startH
		#falta fer un graf amb les 6 curves sobreposades i les curves de potencia (per exemple) sobrepossades
		#fer que acabin al mateix punt encara que no iniciin en el mateix
		#arreglar que els eixos de l'esq han de seguir un ylim,pero els de la dreta un altre, basat en el que es vol observar
		#fer que es pugui enviar colors que es vol per cada curva, o linetypes
		wide=max(curves$end-curves$start)
		
		#a=cumsum(rawdata)
		#yrange=c(min(a),max(a))
		yrange=find.yrange(singleFile, rawdata,curves)

		knRanges=kinematicRanges(singleFile,rawdata,curves,Mass,smoothingOne,g)
		for(i in 1:n) {
			#in superpose all jumps end at max height
			#start can change, some are longer than other
			#xmin and xmax should be the same for all in terms of X concordance
			#but line maybe don't start on the absolute left
			#this is controled by startX
			startX = curves[i,1]-(curves[i,2]-wide)+1;
			myTitle = "";
			if(i==1)
				myTitle = paste(titleType,jump);

			paint(rawdata, eccon, curves[i,2]-wide,curves[i,2],yrange,knRanges,TRUE,(i==jump),
				startX,curves[i,3],smoothingOne,Mass,myTitle,
				TRUE,	#draw
				TRUE,	#showLabels
				FALSE,	#marShrink
				(i==1),	#showAxes
				TRUE	#legend
				)
			par(new=T)
		}
		par(new=F)
		#print(knRanges)
	}

	#analysis in cross variables comes as:
	#"cross.Speed.Force.mean" 	#2nd is Y, 3d is X. "mean" can also be "max"
	#there's a double XY plot:
	#"cross.Speed,Power.Load.mean" 	#Speed,power are Y (left and right), 3d: Load is X.
	analysisCross = unlist(strsplit(analysis, "\\."))
	if(
			analysis == "powerBars" || analysisCross[1] == "cross" || analysis == "curves") 
	{
		paf = data.frame()
		for(i in 1:n) { 
			myMass = Mass
			mySmoothingOne = smoothingOne
			myEccon = eccon
			if(! singleFile) {
				myMass = curves[i,5]
				mySmoothingOne = curves[i,6]
				myEccon = curves[i,8]
			}
			paf=rbind(paf,(powerBars(kinematicsF(rawdata[curves[i,1]:curves[i,2]], myMass, mySmoothingOne, g))))
		}
		#print(paf)
		rownames(paf)=rownames(curves) #put correct rownames when there are inactive curves
print("----------------------------")
		print(paf)

		if(analysis == "powerBars") {
			if(! singleFile) 
				paintPowerPeakPowerBars(Title, paf, curves[,8], 		#myEccon
						curvesHeight)				#height
			else 
				paintPowerPeakPowerBars(Title, paf, curves[,8], 		#myEccon
						rawdata.cumsum[curves[,2]]-curves[,3])	#height
		}
		else if(analysisCross[1] == "cross") {
			if(analysisCross[2] == "Speed,Power") {
				par(mar=c(5,4,4,5))
				analysisCrossVertVars = unlist(strsplit(analysisCross[2], "\\,"))
				paintCrossVariables(paf, analysisCross[3], analysisCrossVertVars[1], 
						    analysisCross[4], "LEFT", Title)
				par(new=T)
				paintCrossVariables(paf, analysisCross[3], analysisCrossVertVars[2], 
						    analysisCross[4], "RIGHT", "")
			} else
				paintCrossVariables(paf, analysisCross[3], analysisCross[2], 
						    analysisCross[4], "ALONE", Title)
		}
		else if(analysis == "curves") {
			paf=cbind(curves[,1],curves[,2]-curves[,1],rawdata.cumsum[curves[,2]]-curves[,3],paf)
			colnames(paf)=c("start","width","height","meanSpeed","maxSpeed",
				"meanPower","peakPower","peakPowerT","pp_ppt")
			write.csv(paf, outputData1, quote=FALSE)
		}
	}
	if(analysis=="exportCSV") {
		print("Starting export...")
		file=outputData1;
		curvesNum = length(curves[,1])
		
		maxLength = 0
		for(i in 1:curvesNum) { 
			myLength = curves[i,2]-curves[i,1]
			if(myLength > maxLength)
				maxLength=myLength
		}

		curveCols = 6	#change this value if there are more colums
		names=c("Dist.", "Dist. +", "Speed", "Accel.", "Force", "Power")
		nums=1:curvesNum
		nums=rep(nums,each=curveCols)		
		namesNums=paste(names, nums)
		
		for(i in 1:curvesNum) { 
			kn = kinematicsF (rawdata[curves[i,1]:curves[i,2]], Mass, smoothingOne, g)
			
			#fill with NAs in order to have the same length
			col1 = rawdata[curves[i,1]:curves[i,2]]
			col2 = rawdata.cumsum[curves[i,1]:curves[i,2]]
			
			#add mean and max
			col1=append(col1,
				    c(NA,NA,NA,namesNums[((i-1)*curveCols)+1]),
				    after=0)
			col2=append(col2,
				    c(NA,"mean (ABS):","max:",namesNums[((i-1)*curveCols)+2]),
				    after=0)
			kn$speedy=append(kn$speedy,
					 c(
					   namesNums[((i-1)*curveCols)+3],
					   mean(abs(kn$speedy)),max(kn$speedy),
					   namesNums[((i-1)*curveCols)+3]),
					 after=0)
			kn$accely=append(kn$accely,
					 c(
					   namesNums[((i-1)*curveCols)+4],
					   mean(abs(kn$accely)),max(kn$accely),
					   namesNums[((i-1)*curveCols)+4]),
					 after=0)
			kn$force=append(kn$force,
					c(
					  namesNums[((i-1)*curveCols)+5],
					  mean(abs(kn$force)),max(kn$force),
					  namesNums[((i-1)*curveCols)+5]),
					after=0)
			kn$power=append(kn$power,
					c(
					  namesNums[((i-1)*curveCols)+6],
					  mean(abs(kn$power)),max(kn$power),
					  namesNums[((i-1)*curveCols)+6]),
					after=0)
			
			extraRows=4
			length(col1)=maxLength+extraRows
			length(col2)=maxLength+extraRows
			length(kn$speedy)=maxLength+extraRows
			length(kn$accely)=maxLength+extraRows
			length(kn$force)=maxLength+extraRows
			length(kn$power)=maxLength+extraRows

			if(i==1)
				df=data.frame(cbind(col1, col2,
						    kn$speedy, kn$accely, kn$force, kn$power))
			else
				df=data.frame(cbind(df, col1, col2,
						    kn$speedy, kn$accely, kn$force, kn$power))
		}
	
		#TODO: time
		#TODO: tenir en compte el startH
		
		Title=gsub('_',' ',Title)
		print(Title)
		titleColumns=unlist(strsplit(Title,'-'))
		colnames(df)=c(titleColumns[1]," ", titleColumns[2],titleColumns[3],rep(" ",(curvesNum*curveCols-4)))

		write.csv2(df, file=file, row.names=F, na="")
		#write.csv2(df, file=file, quotes=F)
		print("Export done.")
	}
	if(analysis != "exportCSV")
		dev.off()

	write("(4/4) R tasks done", outputData2)
}

warnings()


