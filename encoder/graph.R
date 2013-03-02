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


#concentric, eccentric-concentric, repetitions of eccentric-concentric
#currently only used "c" and "ec". no need of ec-rep because c and ec are repetitive
#"ecS" is like ec but eccentric and concentric phases are separated, used in findCurves, this is good for treeview to know power... on the 2 phases
eccons=c("c","ec","ec-rep","ecS") 

g = 9.81
smoothingAll= 0.1

colSpeed="springgreen3"; colForce="blue2"; colPower="tomato2"	#colors
#colSpeed="black"; colForce="black"; colPower="black"		#black & white
cols=c(colSpeed,colForce,colPower); lty=rep(1,3)	


#--- user commands ---
#way A. passing options to a file
getOptionsFromFile <- function(optionsFile) {
	optionsCon <- file(optionsFile, 'r')
	options=readLines(optionsCon,n=17)
	close(optionsCon)
	return (options)
}

#way B. put options as arguments
#unused because maybe command line gets too long
#options <- commandArgs(TRUE)


args <- commandArgs(TRUE)
optionsFile =args[1]

print(optionsFile)

options=getOptionsFromFile(optionsFile);

print(options)

OutputData2=options[4] #currently used to display status
OperatingSystem=options[17]

write("(1/5) Starting R", OutputData2)


#this will replace below methods: findPics1ByMinindex, findPics2BySpeed
findCurves <- function(rawdata, eccon, min_height, draw, title) {
	a=cumsum(rawdata)
	b=extrema(a)
	print("at findCurves")
	print(b)
	
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

		#if a person starts stand up and goes down, extrema maxindex don't find the initial position
		#if this person does 3 squats, only 2 will be found
		#add first value of all the serie (1ms time) to maxindex to help to detect this first curve
		b$maxindex = rbind(c(1,1),b$maxindex)

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
		plot((1:length(a))/1000			#ms -> s
		     ,a/10,				#mm -> cm
		     type="l",
		     xlim=c(1,length(a))/1000,		#ms -> s
		     xlab="",ylab="",axes=T) 
		title(title, cex.main=1, font.main=1)
		mtext("time (s) ",side=1,adj=1,line=-1)
		mtext("height (cm) ",side=2,adj=1,line=-1)
		abline(v=b$maxindex/1000,lty=3); abline(v=b$minindex/1000,lty=3)	#ms -> s
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
kinematicsF <- function(a, mass, smoothingOne, g, eccon, analysisOptions) {
	print("length unique x in spline")
	print(length(unique(1:length(a))))

	speed <- smooth.spline( 1:length(a), a, spar=smoothingOne)
	accel <- predict( speed, deriv=1 )
	#speed comes in mm/ms when derivate to accel its mm/ms^2 to convert it to m/s^2 need to *1000 because it's quadratic
	accel$y <- accel$y * 1000 
	errorSearching = FALSE

	eccentric = 0

	#search propulsiveEnds
	if(analysisOptions == "p") {
		if(eccon=="c") {
			concentric=1:length(a)
		} else {	#"ec", "ec-rep"
			b=extrema(speed$y)
			print(b)
			#In all the extrema minindex values, search which range (row) has the min values,
			#and in this range search last value
			print("searchMinSpeedEnd")
			searchMinSpeedEnd = max(which(speed$y == min(speed$y)))
			print(searchMinSpeedEnd)
			#In all the extrema maxindex values, search which range (row) has the max values,
			#and in this range search first value
			print("searchMaxSpeedIni")
			searchMaxSpeedIni = min(which(speed$y == max(speed$y)))
			print(searchMaxSpeedIni)
			#find the cross between both
			print("b-Cross")
			print(b$cross[,1])
			print("search min cross: crossMinRow")
			crossMinRow=which(b$cross[,1] > searchMinSpeedEnd & b$cross[,1] < searchMaxSpeedIni)

			if (length(crossMinRow) > 0) {
				print(crossMinRow)

				eccentric=1:b$cross[crossMinRow,1]
				concentric=b$cross[crossMinRow,2]:length(a)
			} else {
				propulsiveEnds = length(a)
				errorSearching = TRUE
			}
		}

		if(! errorSearching) {
			#propulsive phase ends when accel is -9.8
			if(length(which(accel$y[concentric]<=-g)) > 0 & analysisOptions == "p") {
				propulsiveEnds = max(eccentric) + min(which(accel$y[concentric]<=-g))
			} else {
				propulsiveEnds = max(concentric)
			}
		}
	}
	#end of search propulsiveEnds


#	force <- mass*accel$y
#	if(isJump)
		force <- mass*(accel$y+g)	#g:9.81 (used when movement is against gravity)

	power <- force*speed$y

	if(analysisOptions == "p")
		return(list(speedy=speed$y[1:propulsiveEnds], accely=accel$y[1:propulsiveEnds], 
			    force=force[1:propulsiveEnds], power=power[1:propulsiveEnds], mass=mass))
	else
		return(list(speedy=speed$y, accely=accel$y, force=force, power=power, mass=mass))
}

powerBars <- function(kinematics) {
	meanSpeed <- mean(kinematics$speedy)
	maxSpeed <- max(abs(kinematics$speedy))
	meanPower <- mean(kinematics$power)
	peakPower <- max(abs(kinematics$power))
	peakPowerT <- min(which(abs(kinematics$power) == peakPower))
	pp_ppt <- peakPower / (peakPowerT/1000)	# ms->s
	meanForce <- mean(kinematics$force)
	maxForce <- max(abs(kinematics$force))

	#here paf is generated
	#mass is not used by powerBars, but used by Kg/W (loadVSPower)
	#meanForce and maxForce are not used by powerBars, but used by F/S (forceVSSpeed)
	return(data.frame(meanSpeed, maxSpeed, meanPower,peakPower,peakPowerT,pp_ppt,
			  kinematics$mass,meanForce,maxForce))
}

kinematicRanges <- function(singleFile,rawdata,curves,mass,smoothingOne,g,eccon,analysisOptions) {
	n=length(curves[,1])
	maxSpeedy=0;maxForce=0;maxPower=0
	myEccon = eccon
	for(i in 1:n) { 
		myMass = mass
		mySmoothingOne = smoothingOne
		if(! singleFile) {
			myMass = curves[i,5]
			mySmoothingOne = curves[i,6]
			myEccon = curves[i,8]
		}
		kn=kinematicsF(rawdata[curves[i,1]:curves[i,2]],myMass,mySmoothingOne,g,myEccon,analysisOptions)
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
	startX, startH, smoothing, mass, title, subtitle, draw, showLabels, marShrink, showAxes, legend,
	Analysis, AnalysisOptions, ExercisePercentBodyWeight 
	) {

	meanSpeedE = 0
	meanSpeedC = 0
	meanPowerE = 0
	meanPowerC = 0

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
		par(mar=c(3, 3.5, 5, 8.5))
		if(marShrink) #used on "side" compare
			par(mar=c(1, 1, 4, 1))
	
		#plot distance
		#plot(a,type="h",xlim=c(xmin,xmax),xlab="time (ms)",ylab="Left: distance (mm); Right: speed (m/s), accelration (m/s^2)",col="gray", axes=F) #this shows background on distance (nice when plotting distance and speed, but confusing when there are more variables)
		xlab="";ylab="";
		#if(showLabels) {
		#	xlab="time (ms)"
		#	ylab="Left: distance (mm); Right: speed (m/s), force (N), power (W)"
		#}
		ylim=yrange
		if(ylim[1]=="undefined") { ylim=NULL }
		plot(a-min(a),type="n",xlim=c(1,length(a)),ylim=ylim,xlab=xlab, ylab=ylab, col="gray", axes=F)

		title(main=title,line=-2,outer=T)
		mtext(subtitle,side=1,adj=0,cex=.8)
		
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
			plot(startX:length(a),yValues,type="l",xlim=c(1,length(a)),ylim=ylim,
			     xlab="",ylab="",col="black",lty=lty[1],lwd=2,axes=F)
			par(new=T)
			plot(startX:length(a),yValues,type="h",xlim=c(1,length(a)),ylim=ylim,
			     xlab="",ylab="",col="grey90",lty=lty[1],lwd=1,axes=F)
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
			plot(startX:length(speed$y),speed$y[startX:length(speed$y)],type="l",
			     xlim=c(1,length(a)),ylim=ylim,xlab="",ylab="",col=cols[1],lty=lty[1],lwd=1,axes=F)
		else
			plot(startX:length(speed$y),speed$y[startX:length(speed$y)],type="l",
			     xlim=c(1,length(a)),ylim=ylim,xlab="",ylab="",col="darkgreen",lty=2,lwd=3,axes=F)
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
		print("EXTREMA")
		#abline(v=b$maxindex,lty=3,col="yellow");
		#abline(v=b$minindex,lty=3,col="magenta")
		print(b)

		#In all the extrema minindex values, search which range (row) has the min values,
		#and in this range search last value
		print("searchMinSpeedEnd")
		searchMinSpeedEnd = max(which(speed$y == min(speed$y)))
		#In all the extrema maxindex values, search which range (row) has the max values,
		#and in this range search first value
		print("searchMaxSpeedIni")
		searchMaxSpeedIni = min(which(speed$y == max(speed$y)))
		#find the cross between both
		print("search min cross: crossMinRow")
		crossMinRow=which(b$cross[,1] > searchMinSpeedEnd & b$cross[,1] < searchMaxSpeedIni)

		eccentric=1:b$cross[crossMinRow,1]
		concentric=b$cross[crossMinRow,2]:length(a)
		isometric=c(b$cross[crossMinRow,1],b$cross[crossMinRow,2])
		if(draw) {
			abline(v=max(eccentric),col=cols[1])
			abline(v=min(concentric),col=cols[1])
			#mtext(text=paste(max(eccentric)," ",sep=""),side=1,at=max(eccentric),adj=1,cex=.8,col=cols[1])
			#mtext(text=paste(" ",min(concentric),sep=""),side=1,at=min(concentric),adj=0,cex=.8,col=cols[1])
			mtext(text=mean(isometric),side=1,at=mean(isometric),adj=0.5,cex=.8,col=cols[1])
			mtext(text="eccentric ",side=3,at=max(eccentric),cex=.8,adj=1,col=cols[1],line=.5)
			mtext(text=" concentric ",side=3,at=min(concentric),cex=.8,adj=0,col=cols[1],line=.5)
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
		if(length(which(accel$y[concentric]<=-g)) > 0 & AnalysisOptions == "p") {
			propulsiveEnds = max(eccentric) + min(which(accel$y[concentric]<=-g))
		} else {
			propulsiveEnds = max(concentric)
		}

		ylim=c(-max(abs(range(accel$y))),max(abs(range(accel$y))))	 #put 0 in the middle

		meanSpeedC = mean(speed$y[min(concentric):max(concentric)])
		if(AnalysisOptions == "p") {
			meanSpeedC = mean(speed$y[min(concentric):propulsiveEnds])
		}

		if(eccon == "c") {
			arrows(x0=min(concentric),y0=meanSpeedC,x1=propulsiveEnds,y1=meanSpeedC,col=cols[1],code=3)
		} else {
			meanSpeedE = mean(speed$y[min(eccentric):max(eccentric)])
			arrows(x0=min(eccentric),y0=meanSpeedE,x1=max(eccentric),y1=meanSpeedE,col=cols[1],code=3)
			arrows(x0=min(concentric),y0=meanSpeedC,x1=propulsiveEnds,y1=meanSpeedC,col=cols[1],code=3)
		}

		
		#plot the speed axis
		if(showAxes) {
			abline(h=0,lty=3,col="black")
			if(eccon == "c") 
				axis(4, at=c(min(axTicks(4)),0,max(axTicks(4)),meanSpeedC),
				     labels=c(min(axTicks(4)),0,max(axTicks(4)),
					      parse(text=paste("paste(bar(x),'c=',",round(meanSpeedC,1),")"))),
				     col=cols[1], lty=lty[1], line=0, lwd=1, padj=-.5)
			else
				axis(4, at=c(min(axTicks(4)),0,max(axTicks(4)),meanSpeedE,meanSpeedC),
				     labels=c(min(axTicks(4)),0,max(axTicks(4)),
					      parse(text=paste("paste(bar(x),'e=',",round(meanSpeedE,1),")")),
					      parse(text=paste("paste(bar(x),'c=',",round(meanSpeedC,1),")"))),
				     col=cols[1], lty=lty[1], line=0, lwd=1, padj=-.5)
		}

		par(new=T)
		if(highlight==FALSE)
			plot(startX:length(accel$y),accel$y[startX:length(accel$y)],type="l",
			     xlim=c(1,length(a)),ylim=ylim,xlab="",ylab="",col="magenta",lty=lty[2],lwd=1,axes=F)
		else
			plot(startX:length(accel$y),accel$y[startX:length(accel$y)],type="l",
			     xlim=c(1,length(a)),ylim=ylim,xlab="",ylab="",col="darkblue",lty=2,lwd=3,axes=F)
			
		#propulsive stuff
		abline(h=-g,lty=3,col="magenta")
		abline(v=propulsiveEnds,lty=3,col="magenta") 
		points(propulsiveEnds, -g, col="magenta")
		
		if(showAxes)
			axis(4, col="magenta", lty=lty[1], line=2, lwd=1, padj=-.5)
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
			plot(startX:length(force),force[startX:length(force)],type="l",
			     xlim=c(1,length(a)),ylim=ylim,xlab="",ylab="",col=cols[2],lty=lty[2],lwd=1,axes=F)
		else
			plot(startX:length(force),force[startX:length(force)],type="l",
			     xlim=c(1,length(a)),ylim=ylim,xlab="",ylab="",col="darkblue",lty=2,lwd=3,axes=F)
		if(showAxes)
			axis(4, col=cols[2], lty=lty[2], line=4, lwd=1, padj=-.5)
	}

	
	#mark when it's air and land
	#if it was a eccon concentric-eccentric, will be useful to calculate flight time
	#but this eccon will be not done
	#if(draw & (!superpose || (superpose & highlight)) & isJump) {
	if(draw & (!superpose || (superpose & highlight)) & ExercisePercentBodyWeight == 100) {
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
			plot(startX:length(power),power[startX:length(power)],type="l",
			     xlim=c(1,length(a)),ylim=ylim,xlab="",ylab="",col=cols[3],lty=lty[3],lwd=2,axes=F)
		else
			plot(startX:length(power),power[startX:length(power)],type="l",
			     xlim=c(1,length(a)),ylim=ylim,xlab="",ylab="",col="darkred",lty=2,lwd=3,axes=F)


		meanPowerC = mean(power[min(concentric):max(concentric)])
		if(AnalysisOptions == "p") {
			meanPowerC = mean(power[min(concentric):propulsiveEnds])
		}

		if(eccon == "c") {
			arrows(x0=min(concentric),y0=meanPowerC,x1=propulsiveEnds,y1=meanPowerC,col=cols[3],code=3)
		} else {
			meanPowerE = mean(power[min(eccentric):max(eccentric)])
			arrows(x0=min(eccentric),y0=meanPowerE,x1=max(eccentric),y1=meanPowerE,col=cols[3],code=3)
			arrows(x0=min(concentric),y0=meanPowerC,x1=propulsiveEnds,y1=meanPowerC,col=cols[3],code=3)
		}

		if(showAxes) {
			if(eccon == "c") 
				axis(4, at=c(min(axTicks(4)),0,max(axTicks(4)),meanPowerC),
				     labels=c(min(axTicks(4)),0,max(axTicks(4)),
					      parse(text=paste("paste(bar(x),'c=',",round(meanPowerC,1),")"))),
				     col=cols[3], lty=lty[1], line=6, lwd=2, padj=-.5)
			else
				axis(4, at=c(min(axTicks(4)),0,max(axTicks(4)),meanPowerE, meanPowerC),
				     labels=c(min(axTicks(4)),0,max(axTicks(4)),
					      parse(text=paste("paste(bar(x),'e=',",round(meanPowerE,1),")")),
					      parse(text=paste("paste(bar(x),'c=',",round(meanPowerC,1),")"))),
				     col=cols[3], lty=lty[1], line=6, lwd=2, padj=-.5)
		}
	}

	#time to arrive to peak power
	peakPowerT=min(which(power == max(power)))
	if(draw & !superpose) {
		abline(v=peakPowerT, col=cols[3])
		points(peakPowerT, max(power),col=cols[3])
		mtext(text=paste("peak power:",round(max(power),3)),side=3,at=peakPowerT,cex=.8,col=cols[3])
		mtext(text=peakPowerT,side=1,at=peakPowerT,cex=.8,col=cols[3])
	}

	#legend, axes and title
	if(draw) {
		if(legend & showAxes) {
			#plot legend on top exactly out
			#http://stackoverflow.com/a/7322792
			rng=par("usr")
			lg = legend(0,rng[2], 
				    legend=c("Distance","Speed","Accel.","Force","Power"), 
				    lty=c(1,1,1,1,1), lwd=c(2,2,2,2,2), 
				    col=c("black",cols[1],"magenta",cols[2],cols[3]), 
				    cex=1, bg="white", ncol=6, bty="n", plot=F)
			legend(0,rng[4]+1.25*lg$rect$h, 
			       legend=c("Distance","Speed","Accel.","Force","Power"), 
			       lty=c(1,1,1,1,1), lwd=c(2,2,2,2,2), 
			       col=c("black",cols[1],"magenta",cols[2],cols[3]), 
			       cex=1, bg="white", ncol=6, bty="n", plot=T, xpd=NA)
		}
		if(showLabels) {
			mtext("time (ms) ",side=1,adj=1,line=-1,cex=.9)
			mtext("height (mm) ",side=2,adj=1,line=-1,cex=.9)
		}
	}
}

paintPowerPeakPowerBars <- function(singleFile, title, paf, myEccons, Eccon, height, n) {
	pafColors=c("tomato1","tomato4",topo.colors(10)[3])
	myNums = rownames(paf)
	height = abs(height/10)
	
	if(Eccon=="ecS") {
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
	
	par(mar=c(2.5, 4, 5, 5))
	bp <- barplot(powerData,beside=T,col=pafColors[1:2],width=c(1.4,.6),
			names.arg=paste(myNums,"\n",paf[,7],sep=""),xlim=c(1,n*3+.5),cex.name=0.9,
			xlab="",ylab="Power (W)", 
			ylim=c(lowerY,max(powerData)), xpd=FALSE) #ylim, xpd = F,  makes barplot starts high (compare between them)
	title(main=title,line=-2,outer=T)
	mtext("Curve\nLoad",side=1,at=1,adj=0,line=1,cex=.9)
	par(new=T, xpd=T)
	#on ecS, concentric has high value of time to peak power and eccentric has it very low. Don't draw lines
	if(Eccon=="ecS")
		plot(bp[2,],paf[,5],type="p",lwd=2,xlim=c(1,n*3+.5),ylim=c(0,max(paf[,5])),
		     axes=F,xlab="",ylab="",col="blue", bg="lightblue",cex=1.5,pch=21)
	else
		plot(bp[2,],paf[,5],type="b",lwd=2,xlim=c(1,n*3+.5),ylim=c(0,max(paf[,5])),
		     axes=F,xlab="",ylab="",col=pafColors[3])
	
	axis(4, col=pafColors[3], line=0,padj=-.5)
	mtext("Time to peak power (ms)", side=4, line=-1)
	
	par(new=T)
	plot(bp[2,],height,type="b",lwd=2,xlim=c(1,n*3+.5),ylim=c(0,max(height)),axes=F,xlab="",ylab="",col="green")

	#plot legend on top exactly out
	#http://stackoverflow.com/a/7322792
	rng=par("usr")
	lg = legend(rng[1], rng[2],
		    col=c(pafColors,"green"), lty=c(0,0,1,1), lwd=c(1,1,2,2), pch=c(15,15,NA,NA), 
		    legend=c("Power","Peak Power", "Time to Peak Power    ", "Range"), ncol=4, bty="n", plot=F)
	legend(rng[1], rng[4]+1.25*lg$rect$h,
	       col=c(pafColors,"green"), lty=c(0,0,1,1), lwd=c(1,1,2,2), pch=c(15,15,NA,NA), 
	       legend=c("Power","Peak Power", "Time to Peak Power    ", "Range"), ncol=4, bty="n", plot=T, xpd=NA)
	
	abline(h=max(height),lty=2, col="green")
	abline(h=min(height),lty=2, col="green")
	#text(max(bp[,2]),max(height),max(height),adj=c(0,.5),cex=0.8)
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
	title(title, cex.main=1, font.main=2)
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

#propulsive!!!!
paint1RMBadillo2010 <- function (paf, title) {
	curvesLoad = (paf[,7]) 					#mass: X
	curvesSpeed = (paf[,findPosInPaf("Speed", "mean")])	#mean speed Y

	par(mar=c(5,6,3,4))

	loadPercent <- seq(30,100, by=5)

	#msp: mean speed propulsive
	msp <- c(1.33, 1.235, 1.145, 1.055, 0.965, 0.88, 0.795,
		                          0.715, 0.635, 0.555, 0.475, 0.405, 0.325, 0.255, 0.185)
	#variation <- c(0.08, 0.07, 0.06, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.04, 0.04, 0.04, 0.04, 0.03, 0.04)

	maxy=max(c(msp,curvesSpeed))
	miny=min(c(msp,curvesSpeed))

	loadPercentCalc=8.4326*curvesSpeed^2 - 73.501*curvesSpeed + 112.33
	loadCalc= 100 * curvesLoad / loadPercentCalc

	par(mar=c(5,5,3,4))

	plot(curvesLoad,curvesSpeed, type="p",
	     main=paste(title, "1RM prediction"),
	     sub="Adapted from Gonzalez-Badillo, Sanchez-Medina (2010)",
	     xlim=c(min(curvesLoad),max(loadCalc)),
	     ylim=c(miny,maxy), xlab="", ylab="",axes=T)

	mtext(side=1,line=2,"Kg")
	mtext(side=2,line=3,"Mean speed in propulsive phase (m/s)")
	mtext(side=4,line=2,"1RM (%)")

	abline(h=msp, lty=2, col="gray")
	mtext(side=4,at=msp, paste(" ",loadPercent), las=2)

	colors=c(rep(NA,29),rev(heat.colors(100)[0:71]))
	arrows(curvesLoad,curvesSpeed,loadCalc,0.185,code=2,col=colors[loadPercentCalc])

	closerValues = which(curvesLoad == max(curvesLoad))
	segments(loadCalc[closerValues],0.185,loadCalc[closerValues],0,lty=3)

	predicted1RM = mean(loadCalc[closerValues])

	segments(predicted1RM,0.185,predicted1RM,0,lty=1)
	mtext(side=1, at=predicted1RM, round(predicted1RM,2), cex=.8)
}

			
find.mfrow <- function(n) {
	if(n<=3) return(c(1,n))
	else if(n<=8) return(c(2,ceiling(n/2)))
	else return(c(3, ceiling(n/3)))
}

find.yrange <- function(singleFile, rawdata, curves) {
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
	if(y.min < 0) {
		y.max = y.max + -1*y.min
		y.min = 0
	}
	return (c(y.min,y.max))
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

loadLibraries <- function(os) {
	library("EMD")
	#library("sfsmisc")
	if(os=="Windows")
		library("Cairo")
}

doProcess <- function(options) {

	File=options[1]
	OutputGraph=options[2]
	OutputData1=options[3]
	OutputData2=options[4] #currently used to display status
	MinHeight=as.numeric(options[5])*10 #from cm to mm
	ExercisePercentBodyWeight=as.numeric(options[6])	#was isJump=as.logical(options[6])
	Mass=as.numeric(options[7])
	Eccon=options[8]
	Analysis=options[9]	#in cross comes as "cross.Force.Speed.mean"
	AnalysisOptions=options[10]	#p: propulsive
	SmoothingOne=options[11]
	Jump=options[12]
	Width=as.numeric(options[13])
	Height=as.numeric(options[14])
	DecimalSeparator=options[15]
	Title=options[16]
	OperatingSystem=options[17]

	print(File)
	print(OutputGraph)
	print(OutputData1)
	print(OutputData2)
	
	if(Analysis != "exportCSV") {
		if(OperatingSystem=="Windows")
			Cairo(Width, Height, file=OutputGraph, type="png", bg="white")
		else
			png(OutputGraph, width=Width, height=Height)

		Title=gsub('_',' ',Title)
		Title=gsub('-','    ',Title)
	}

	titleType = "c"
	#if(isJump)
	#	titleType="jump"

	curvesPlot = FALSE
	if(Analysis=="curves") {
		curvesPlot = TRUE
		par(mar=c(2,2.5,2,1))
	}

	#when a csv is used (it links to lot of files) then singleFile = false
	singleFile = TRUE
	if(nchar(File) >= 40) {
		#file="/tmp...../chronojump-encoder-graph-input-multi.csv"
		#substr(file, nchar(file)-39, nchar(file))
		#[1] "chronojump-encoder-graph-input-multi.csv"
		if(substr(File, nchar(File)-39, nchar(File)) == "chronojump-encoder-graph-input-multi.csv") {
			singleFile = FALSE
		}
	}

	if(! singleFile) {
		#this produces a rawdata, but note that a cumsum(rawdata) cannot be done because:
		#this are separated movements
		#maybe all are concentric (there's no returning to 0 phase)

		#this version of curves has added specific data cols:
		#status, exerciseName, mass, smoothingOne, dateTime, myEccon

		inputMultiData=read.csv(file=File,sep=",",stringsAsFactors=F)

		rawdata = NULL
		count = 1
		start = NULL; end = NULL; startH = NULL
		status = NULL; id = NULL; exerciseName = NULL; mass = NULL; smooth = NULL
		dateTime = NULL; myEccon = NULL; curvesHeight = NULL

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

			#if curves file ends with comma. Last character will be an NA. remove it
			#this removes all NAs on a curve
			dataTempFile  = dataTempFile[!is.na(dataTempFile)]

			dataTempPhase=dataTempFile
			processTimes = 1
			changePos = 0
			#if this curve is ecc-con and we want separated, divide the curve in two
			if(as.vector(inputMultiData$eccon[i]) != "c" & Eccon =="ecS") {
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
			curves = data.frame(start,end,startH,exerciseName,mass,smooth,
					    dateTime,myEccon,stringsAsFactors=F,row.names=id)
		} else {
			curves = data.frame(id,start,end,startH,exerciseName,mass,smooth,
					    dateTime,myEccon,stringsAsFactors=F,row.names=1)
		}

		n=length(curves[,1])
		quitIfNoData(n, curves, OutputData1)
	} else {
		rawdata=scan(file=File,sep=",")

		if(length(rawdata)==0) {
			plot(0,0,type="n",axes=F,xlab="",ylab="")
			text(x=0,y=0,"Encoder is not connected.",cex=1.5)
			dev.off()
			write("", OutputData1)
			quit()
		}

		rawdata.cumsum=cumsum(rawdata)

		curves=findCurves(rawdata, Eccon, MinHeight, curvesPlot, Title)
		print(curves)
		n=length(curves[,1])
		quitIfNoData(n, curves, OutputData1)

		for(i in 1:n) { 
			curves[i,1]=reduceCurveBySpeed(Eccon, i, curves[i,1],
						       rawdata[curves[i,1]:curves[i,2]], SmoothingOne)
		}
		if(curvesPlot) {
			#/10 mm -> cm
			for(i in 1:length(curves[,1])) { 
				myLabel = i
				myY = min(rawdata.cumsum)/10
				adjVert = 0
				if(Eccon=="ecS") {
					myEc=c("c","e")
					myLabel = paste(trunc((i+1)/2),myEc[((i%%2)+1)],sep="")
					myY = rawdata.cumsum[curves[i,1]]/10
					if(i%%2 == 1) {
						adjVert = 1
					}
				}
				text(x=((curves[i,1]+curves[i,2])/2/1000),	#/1000 ms -> s
				     y=myY,labels=myLabel, adj=c(0.5,adjVert),cex=1,col="blue")
				arrows(x0=(curves[i,1]/1000),y0=myY,x1=(curves[i,2]/1000),	#/1000 ms -> s
				       y1=myY, col="blue",code=3,length=0.1)
			}
		}
	}

	write("(4/5) Curves processed", OutputData2)

	if(Analysis=="single") {
		if(Jump>0) {
			myMass = Mass
			mySmoothingOne = SmoothingOne
			myEccon = Eccon
			myStart = curves[Jump,1]
			myEnd = curves[Jump,2]
			if(! singleFile) {
				myMass = curves[Jump,5]
				mySmoothingOne = curves[Jump,6]
				myEccon = curves[Jump,8]
			}
			myCurveStr = paste("curve=", Jump, ", ", myMass, "Kg", sep="")
			paint(rawdata, myEccon, myStart, myEnd,"undefined","undefined",FALSE,FALSE,
			      1,curves[Jump,3],mySmoothingOne,myMass,
			      paste(Title, " ", Analysis, " ", myEccon, " ", myCurveStr,
				    " (smoothing: ",mySmoothingOne,")",sep=""),
			      "", #subtitle
			      TRUE,	#draw
			      TRUE,	#showLabels
			      FALSE,	#marShrink
			      TRUE,	#showAxes
			      TRUE,	#legend
			      Analysis, AnalysisOptions, ExercisePercentBodyWeight 
			      )	
		}
	}

	if(Analysis=="side") {
		#comparar 6 salts, falta que xlim i ylim sigui el mateix
		par(mfrow=find.mfrow(n))

		#a=cumsum(rawdata)
		#yrange=c(min(a),max(a))
		yrange=find.yrange(singleFile, rawdata, curves)

		knRanges=kinematicRanges(singleFile,rawdata,curves,Mass,SmoothingOne,g,Eccon,AnalysisOptions)

		for(i in 1:n) {
			myMass = Mass
			mySmoothingOne = SmoothingOne
			myEccon = Eccon
			if(! singleFile) {
				myMass = curves[i,5]
				mySmoothingOne = curves[i,6]
				myEccon = curves[i,8]
			}

			myTitle = ""
			if(i == 1)
				myTitle = paste(Title)
			
			mySubtitle = paste("curve=", rownames(curves)[i], ", ", myMass, "Kg", sep="")

			paint(rawdata, myEccon, curves[i,1],curves[i,2],yrange,knRanges,FALSE,FALSE,
			      1,curves[i,3],mySmoothingOne,myMass,myTitle,mySubtitle,
			      TRUE,	#draw
			      FALSE,	#showLabels
			      TRUE,	#marShrink
			      FALSE,	#showAxes
			      FALSE,	#legend
			      Analysis, AnalysisOptions, ExercisePercentBodyWeight 
			      )
		}
		par(mfrow=c(1,1))
	}
	if(Analysis=="superpose") {	#TODO: fix on ec startH
		#falta fer un graf amb les 6 curves sobreposades i les curves de potencia (per exemple) sobrepossades
		#fer que acabin al mateix punt encara que no iniciin en el mateix
		#arreglar que els eixos de l'esq han de seguir un ylim,
		#pero els de la dreta un altre, basat en el que es vol observar
		#fer que es pugui enviar colors que es vol per cada curva, o linetypes
		wide=max(curves$end-curves$start)

		#a=cumsum(rawdata)
		#yrange=c(min(a),max(a))
		yrange=find.yrange(singleFile, rawdata,curves)

		knRanges=kinematicRanges(singleFile,rawdata,curves,Mass,SmoothingOne,g,Eccon,AnalysisOptions)
		for(i in 1:n) {
			#in superpose all jumps end at max height
			#start can change, some are longer than other
			#xmin and xmax should be the same for all in terms of X concordance
			#but line maybe don't start on the absolute left
			#this is controled by startX
			startX = curves[i,1]-(curves[i,2]-wide)+1;
			myTitle = "";
			if(i==1)
				myTitle = paste(titleType,Jump);

			paint(rawdata, Eccon, curves[i,2]-wide,curves[i,2],yrange,knRanges,TRUE,(i==Jump),
			      startX,curves[i,3],SmoothingOne,Mass,myTitle,"",
			      TRUE,	#draw
			      TRUE,	#showLabels
			      FALSE,	#marShrink
			      (i==1),	#showAxes
			      TRUE,	#legend
			      Analysis, AnalysisOptions, ExercisePercentBodyWeight 
			      )
			par(new=T)
		}
		par(new=F)
		#print(knRanges)
	}

	#since Chronojump 1.3.6, encoder analyze has a treeview that can show the curves
	#when an analysis is done, curves file has to be written
	writeCurves = TRUE

	#Analysis in cross variables comes as:
	#"cross.Speed.Force.mean" 	#2nd is Y, 3d is X. "mean" can also be "max"
	#there's a double XY plot:
	#"cross.Speed,Power.Load.mean" 	#Speed,power are Y (left and right), 3d: Load is X.
	analysisCross = unlist(strsplit(Analysis, "\\."))
	if(
	   Analysis == "powerBars" || analysisCross[1] == "cross" || 
	   Analysis == "1RMBadillo2010" || Analysis == "curves" ||
	   writeCurves) 
	{
		paf = data.frame()
		for(i in 1:n) { 
			myMass = Mass
			mySmoothingOne = SmoothingOne
			myEccon = Eccon
			if(! singleFile) {
				myMass = curves[i,5]
				mySmoothingOne = curves[i,6]
				myEccon = curves[i,8]
			}
			print("i:")
			print(i)
			paf=rbind(paf,(powerBars(kinematicsF(rawdata[curves[i,1]:curves[i,2]], 
							     myMass, mySmoothingOne, g, myEccon, AnalysisOptions))))
		}
		#print(paf)
		rownames(paf)=rownames(curves) #put correct rownames when there are inactive curves
		print("----------------------------")
		print(paf)

		if(Analysis == "powerBars") {
			if(! singleFile) 
				paintPowerPeakPowerBars(singleFile, Title, paf, 
							curves[,8], Eccon,	 	#myEccon, Eccon
							curvesHeight, n)			#height
			else 
				paintPowerPeakPowerBars(singleFile, Title, paf, 
							curves[,8], Eccon,		#myEccon, Eccon
							rawdata.cumsum[curves[,2]]-curves[,3], n) #height
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
		else if(Analysis == "1RMBadillo2010") {
			paint1RMBadillo2010(paf, Title)
		}
		
		if(Analysis == "curves" || writeCurves) {
			if(singleFile)
				paf=cbind(
					  "exerciseName",
					  Mass,
					  curves[,1],
					  curves[,2]-curves[,1],rawdata.cumsum[curves[,2]]-curves[,3],paf)
			else
				paf=cbind(
					  exerciseName,
					  curves[i,5],		#mass
					  curves[,1],		
					  curves[,2]-curves[,1],curvesHeight,paf)

			colnames(paf)=c("exercise","mass",
					"start","width","height","meanSpeed","maxSpeed",
					"meanPower","peakPower","peakPowerT","pp_ppt")
			write.csv(paf, OutputData1, quote=FALSE)
			print("curves written")
		}
	}
	if(Analysis=="exportCSV") {
		print("Starting export...")
		File=OutputData1;
		curvesNum = length(curves[,1])

		maxLength = 0
		for(i in 1:curvesNum) { 
			myLength = curves[i,2]-curves[i,1]
			if(myLength > maxLength)
				maxLength=myLength
		}

		curveCols = 6	#change this value if there are more colums
		names=c("DIST.", "DIST. +", "SPEED", "ACCEL.", "FORCE", "POWER")
		nums=1:curvesNum
		nums=rep(nums,each=curveCols)		
		namesNums=paste(names, nums)
		units=c("\n(mm)", "\n(mm)", "\n(m/s)", "\n(m/s^2)", "\n(N)", "\n(W)")
		namesNums=paste(namesNums, units)

		for(i in 1:curvesNum) { 
			kn = kinematicsF (rawdata[curves[i,1]:curves[i,2]], Mass, SmoothingOne, g, Eccon, AnalysisOptions)

			#fill with NAs in order to have the same length
			col1 = rawdata[curves[i,1]:curves[i,2]]
			col2 = rawdata.cumsum[curves[i,1]:curves[i,2]]

			#add mean, max, and time to max
			col1=append(col1,
				    c(NA,NA,NA,NA),
				    after=0)
			col2=append(col2,
				    c(NA,NA,NA,range(col2)[2]-range(col2)[1]),
				    after=0)
			kn$speedy=append(kn$speedy,
					 c(
					   mean(abs(kn$speedy)),
					   max(kn$speedy),
					   (min(which(kn$speedy == max(kn$speedy)))/1000),
					   NA),
					 after=0)
			kn$accely=append(kn$accely,
					 c(
					   mean(abs(kn$accely)),
					   max(kn$accely),
					   NA,
					   NA),
					 after=0)
			kn$force=append(kn$force,
					c(
					  mean(abs(kn$force)),
					  max(kn$force),
					  NA,
					  NA),
					after=0)
			kn$power=append(kn$power,
					c(
					  mean(abs(kn$power)),
					  max(kn$power),
					  (min(which(kn$power == max(kn$power)))/1000),
					  NA),
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

		rownames(df) = c("MEAN (ABS)", "MAX", "TIME TO MAX", "RANGE", 1:maxLength)
		colnames(df) = namesNums

		#TODO: time
		#TODO: tenir en compte el startH

		#Title=gsub('_',' ',Title)
		#print(Title)
		#titleColumns=unlist(strsplit(Title,'-'))
		#colnames(df)=c(titleColumns[1]," ", titleColumns[2],titleColumns[3],rep(" ",(curvesNum*curveCols-4)))

		if(DecimalSeparator == "COMMA")
			write.csv2(df, file=File, row.names=T, na="")
		else
			write.csv(df, file=File, row.names=T, na="")

		print("Export done.")
	}
	if(Analysis != "exportCSV")
		dev.off()

	write("(5/5) R tasks done", OutputData2)

	warnings()
}

write("(2/5) Loading libraries", OutputData2)

loadLibraries(OperatingSystem)
	
write("(3/5) Starting process", OutputData2)

doProcess(options)

