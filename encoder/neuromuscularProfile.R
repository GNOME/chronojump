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
#   Copyright (C) 2014-2017  	Xavier de Blas <xaviblas@gmail.com> 
# 


g = 9.81


#comes with every jump of the three best (in flight time)
#e1, c, e2 are displacements

neuromuscularProfileJump <- function(l.context, e1, c, mass, smoothingC)
{
	#          /
	#         /
	# _     c/
	#  \    /
	# e1\  /
	#    \/
	
	#weight <- mass * g


	#----------------
	#1.- e1 variables
	#----------------

	#e1.range <- range of e1
	e1.pos <- cumsum(e1)
	e1.range <- e1.pos[length(e1.pos)]

	#e1f (when force is done)
	#from max(abs(speed$y)) at e1, to end of e1
	e1.speed <- getSpeed(e1, smoothingC)
	e1.maxspeed.pos <- mean(which(abs(e1.speed$y) == max(abs(e1.speed$y))))
	e1f <- e1[e1.maxspeed.pos:length(e1)] #TODO: check if has to be determined by force > 0 or a > -9.81 instead of maxspeed

	print(c("e max speed.t",e1.maxspeed.pos))
	print(c("length e1",length(e1)))

	#e1f.t duration of e1f (ms)
	e1f.t <- length(e1f)

	#e1f.fmax <- max Force on e1f
	e1f.speed <- getSpeed(e1f, smoothingC)
	e1f.accel <- getAccelerationSafe(e1f.speed)
	e1f.accel$y <- e1f.accel$y * 1000
	e1f.force <- mass * (e1f.accel$y + g)
	e1f.fmax <- max(e1f.force)

	print(c("e1f.t",e1f.t))
	print(c("mean(e1f.force)",mean(e1f.force)))

	#e1f.rdf.avg
	#average force on e1f / e1f.t (s)
	#TODO: Fix this calculation. It is not the same mean(force)/totalTime than mean(force/time)
	e1f.rfd.avg <- mean(e1f.force) / (e1f.t / 1000)  #bars LOAD

	#e1f.i (relative Impulse)
	#average force on e1f * e1f.t (s) / mass (Kg)
	e1f.i <- mean(e1f.force) * (e1f.t / 1000) / mass 

	e1.list = list(
		       range = e1.range,
		       t = e1f.t,
		       fmax = e1f.fmax,
		       rfd.avg = e1f.rfd.avg,
		       i = e1f.i
		       )
	
	#----------------
	#2.- c variables
	#----------------

	#find takeoff
	c.speed <- getSpeed(c, smoothingC)
	c.accel = getAccelerationSafe(c.speed) 
	#speed comes in mm/ms when derivate to accel its mm/ms^2 to convert it to m/s^2 need to *1000 because it's quadratic
	c.accel$y <- c.accel$y * 1000
	c.force <- mass * (c.accel$y + g)

	c.position = cumsum(c)
	
	#c.takeoff = min(which(c.force <= weight))
	#c.takeoff = min(which(c.force <= 0))
	c.maxSpeedT = min(which(c.speed$y == max(c.speed$y)))
	c.takeoff = findTakeOff(c.force, c.maxSpeedT)
	
	c.jumpHeight = (c.position[length(c.position)] - c.position[c.takeoff]) /10
	print(c("jumpHeight", c.jumpHeight))

	#cl "land" from bottom to takeoff (force < weight)
	#ca "air" from takeoff to max height
	#c = cl + ca
	cl = c[1:c.takeoff]
	ca = c[c.takeoff:length(c)]

	#ca.range
	#flight phase on concentric
	ca.pos = cumsum(ca)
	ca.range = ca.pos[length(ca)] 

	#cl.t = contact time (duration) on cl
	cl.t <- length(cl)
	
	#cl.rfd.avg = average force on cl / cl.t (s) / mass (Kg) #bars EXPLODE
	cl.speed <- getSpeed(cl, smoothingC)
	cl.accel = getAccelerationSafe(cl.speed)
	#speed comes in mm/ms when derivate to accel its mm/ms^2 to convert it to m/s^2 need to *1000 because it's quadratic
	cl.accel$y <- cl.accel$y * 1000
	cl.force <- mass * (cl.accel$y + g)

	print(c("cl.t",cl.t))
	print(c("mean clforce",mean(cl.force)))

	
	#TODO: Fix this calculation. It is not the same mean(force)/totalTime than mean(force/time) or finalForce/totalTime
	cl.rfd.avg <- mean(cl.force) / (cl.t / 1000) / mass
	
	#cl.i = average force on cl * cl.t (s) / mass (Kg) #impulse #bars DRIVE
	#It is the same as the take-off speed F*t/m = m*a*t/m = a*t = finalVelocity
	cl.i <- mean(cl.force) * (cl.t / 1000) / mass

	#cl.f.avg = average force on cl / mass (N/Kg)
	cl.f.avg <- mean(cl.force) / mass

	#cl.vf (vF -> valley Force)
	#minimum force on cl before concentric Speed max
	cl.speed.max.pos <- min(which(cl.speed$y == max(cl.speed$y)))
	cl.vf.pos <- min(which(cl.speed$y == min(cl.speed$y[1:cl.speed.max.pos])))
	cl.vf <- cl.force[cl.vf.pos]

	#cl.f.max = max force at right of valley
	cl.f.max <- max(cl.force[cl.vf.pos:length(cl)])

	#cl.s.avg = avg Speed on cl
	cl.s.avg <- mean(cl.speed$y)
	#cl.s.max = max Speed on cl
	cl.s.max <- max(cl.speed$y)

	#power
	cl.p <- cl.force * cl.speed$y
	#cl.p.avg = avg Power on cl
	cl.p.avg <- mean(cl.p)
	#cl.p.max = max Power on cl
	cl.p.max <- max(cl.p)

	c.list = list(
		       ca.range = ca.range,
		       cl.t = cl.t,
		       cl.rfd.avg = cl.rfd.avg,
		       cl.i = cl.i,
		       cl.f.avg = cl.f.avg,
		       cl.vf = cl.vf,
		       cl.f.max = cl.f.max,
		       cl.s.avg = cl.s.avg, cl.s.max = cl.s.max,
		       cl.p.avg = cl.p.avg, cl.p.max = cl.p.max
		       )

	return (list(l.context = l.context, e1 = e1.list, c = c.list, mass = mass))
}

#Manuel Lapuente analysis of 6 separate ABKs (e1, c, e2)
neuromuscularProfileGetData <- function(singleFile, displacement, curves, mass, smoothingC)
{
	#weight=mass*g

	#get the maxheight of the jumps
	#sequence is e,c for every jump. Need the c of every jump
	nums = NULL
	heights = NULL
	count = 1
	for(i in seq(2,length(curves[,1]),by=2)) {
		d = displacement[curves[i,1]:curves[i,2]]
		speed <- getSpeed(d, smoothingC)
		
		accel = getAccelerationSafe(speed) 
		#speed comes in mm/ms when derivate to accel its mm/ms^2 to convert it to m/s^2 need to *1000 because it's quadratic
		accel$y <- accel$y * 1000
	
		
		myMass = mass	
		if(! singleFile) {
			myMassBody = curves[i,5]
			myMassExtra = curves[i,6]
			myMass = myMassBody + myMassExtra
		}

		force <- myMass * (accel$y + g)

		position = cumsum(d)

		takeoff = NULL
		#takeoff = min(which(force <= weight))
		#takeoff = min(which(force <= 0))
		maxSpeedT = min(which(speed$y == max(speed$y)))
		takeoff = findTakeOff(force, maxSpeedT)
		

		if(! is.null(takeoff)) {
			jumpHeight = (position[length(position)] - position[takeoff]) /10
			print(paste("Jump Height =", jumpHeight))

			if(! is.na(jumpHeight)) {
				#store variables
				print(c("Jump accepted count,i,height",count,i,jumpHeight))
				nums[count] = i
				heights[count] = jumpHeight
				count = count +1
			}
		}
	}

	print(c("nums length",length(nums)))
	#if less than three jumps are detected (with jump height), then return an error message
	#this happens if the person stops in the middle of the jump, and then continues, 
	#and jumps are supposed to be in the 1c, 3c, ... and then they change to 4c, 6c, ...
	if(length(nums) < 3) {
		return (-1)
	}

	df=data.frame(cbind(nums,heights))
	bests=rev(order(df$heights))[1:3]
	
	print(c("best three jumps are:", df$nums[bests]))
	print(c("heights are:", df$heights[bests]))

	#with the best three jumps (in jump height) do:

	npj <- list()
	count = 1
	for(i in df$nums[bests]) {
		numJump <- i / 2 #this calculates which jump is this concentric phase

		print(c("numJump",numJump,"height",heights[which(df$nums == i)]))

		l.context <- list(numJump  = numJump,
				  jumpHeight = heights[which(df$nums == i)], 
				  start.e1 = curves[(i-1),1],	#start of e1
				  end.e1   = curves[(i-1),2],	#end of e1
				  start.c  = curves[i,1],	#start of c
				  end.c    = curves[i,2]	#end of c
				  )
		
		myMass = mass	
		if(! singleFile) {
			myMassBody = curves[i,5]
			myMassExtra = curves[i,6]
			myMass = myMassBody + myMassExtra
		}

		npj[[count]] <- neuromuscularProfileJump(
							 l.context,
							 displacement[curves[(i-1),1]:curves[(i-1),2]],	#e1
							 displacement[curves[(i),1]:curves[(i),2]],	#c
							 myMass, smoothingC)
		count = count +1
		
	}
	
	#create a list of avg of each three values
	#npmeans = list(
	#	  e1.fmax = mean(npj[[1]]$e1$fmax, npj[[2]]$e1$fmax, npj[[3]]$e1$fmax),
	#	  c.fmax  = mean(npj[[1]]$c$fmax,  npj[[2]]$c$fmax,  npj[[3]]$c$fmax),
	#	  e2.fmax = mean(npj[[1]]$e2$fmax, npj[[2]]$e2$fmax, npj[[3]]$e2$fmax)
	#	  )
	#return the list
	#return (npmeans)
	
	return (npj)
}

#Linear interpolation of the values
interpolation <- function(variable, value){
        if(variable == "load")
                percent <- 100 * value / 20000 #Linear between 0..20000
        else if(variable == "explode")
                percent <- 100 * value / 250 #Linear between 0..250
        else if(variable == "drive")
                percent = 100 * (value - 3) / (8-3) #Linear between 3..8
        else
                print("Wrong variable name")

        #Let's correct when negative or >100
        if(percent < 0)
                percent <- 0
        else if(percent > 100)
                percent <- 100

        return(percent)
}

neuromuscularProfilePlotBars <- function(title, load, explode, drive)
{
	#print(c("load, explode, drive", load, explode, drive))

	load100 = interpolation("load",load)
	explode100 = interpolation("explode",explode)
	drive100 = interpolation("drive",drive)
	#print(c("load100, explode100, drive100", load100, explode100, drive100))

	barplot(main=title, c(load100,explode100,drive100),col=topo.colors(3),ylim=c(0,100),
		names.arg=c(
			    paste("Load (avg RFD ecc)\n",round(load,2)," -> ",round(load100,2),"%",sep=""),
			    paste("Explode (avg rel. RFD con)\n",round(explode,2)," -> ",round(explode100,2),"%",sep=""),
			    paste("Drive (avg rel. Impulse con)\n",round(drive,2)," -> ",round(drive100,2),"%",sep="")
		))
	
	#show small text related to graph result and how to train
}

neuromuscularProfilePlotOther <- function(displacement, l.context, l.mass, smoothingC)
{
	#plot
	#curve e1,c distance,speed,force /time of best jump
	#curve e1,c force/time  (of the three best jumps)
	#to plot e1,c curves, just sent to paint() the xmin:xmax from start e1 to end of c

	minimumForce = 0
	maximumForce = 0
	maximumLength = 0
	forceFirst = NULL
	forceSecond = NULL
	forceThird = NULL
	l.force.max.c.pos = NULL

	for(i in 1:3) {
		d = displacement[as.integer(l.context[[i]]$start.e1):as.integer(l.context[[i]]$end.c)]
		speed <- getSpeed(d, smoothingC)

		accel = getAccelerationSafe(speed) 
		#speed comes in mm/ms when derivate to accel its mm/ms^2 to convert it to m/s^2 need to *1000 because it's quadratic
		accel$y <- accel$y * 1000

		mass <- l.mass[[i]]
		force <- mass * (accel$y + g)

		if(i == 1)
			forceFirst <- force
		else if(i == 2)
			forceSecond <- force
		else
			forceThird <- force

		#find min/maxs for graph
		if(max(length(force)) > maximumLength)
			maximumLength <- max(length(force))
		if(min(force) < minimumForce)
			minimumForce <- min(force)
		if(max(force) > maximumForce)
			maximumForce <- max(force)

		#find the max force moment in concentric
		#first know start.c relative to this jump
		start.c = as.integer(l.context[[i]]$start.c) - as.integer(l.context[[i]]$start.e1)
		end.c   = as.integer(l.context[[i]]$end.c)   - as.integer(l.context[[i]]$start.e1)

		#get the max force position between start.c and end.c
		l.force.max.c.pos[i] = min(which(force[start.c:end.c] == max(force[start.c:end.c])))
		#add start.c to this position in order to be at right of e1
		l.force.max.c.pos[i] = l.force.max.c.pos[i] + start.c
	}

	cols <- c("red","green","blue")
	plot(forceFirst, type="n", xlab=paste(translateToPrint("time"),"(ms)"), ylab=paste(translateToPrint("Force"),"(N)"), 
	     xlim=c(0,maximumLength), ylim=c(minimumForce, maximumForce))

	#align curves to the right, add NAs at start
	#forceFirst <- c(rep(NA, (maximumLength - length(forceFirst))), forceFirst)
	#forceSecond <- c(rep(NA, (maximumLength - length(forceSecond))), forceSecond)
	#forceThird <- c(rep(NA, (maximumLength - length(forceThird))), forceThird)
	#align at max force concentric
	forceFirst <- c(rep(NA, (max(l.force.max.c.pos) - l.force.max.c.pos[1])), forceFirst)
	forceSecond <- c(rep(NA, (max(l.force.max.c.pos) - l.force.max.c.pos[2])), forceSecond)
	forceThird <- c(rep(NA, (max(l.force.max.c.pos) - l.force.max.c.pos[3])), forceThird)
	
	lines(forceFirst, col=cols[1])
	lines(forceSecond, col=cols[2])
	lines(forceThird, col=cols[3])
	abline(v=max(l.force.max.c.pos),lty=2)
	legend("topleft", col=cols, lty=1, cex=.9,
	       legend=c(paste("Jump ", l.context[[1]]$numJump, " (", l.context[[1]]$jumpHeight, " cm)",sep=""), 
			paste("Jump ", l.context[[2]]$numJump, " (", l.context[[2]]$jumpHeight, " cm)",sep=""), 
			paste("Jump ", l.context[[3]]$numJump, " (", l.context[[3]]$jumpHeight, " cm)",sep=""))
	       )
}

# --------- 1 person ------------------>
neuromuscularProfile3JLoadAvg <- function (npj)
{
	return (mean(c(npj[[1]]$e1$rfd.avg, npj[[2]]$e1$rfd.avg, npj[[3]]$e1$rfd.avg)))
}

neuromuscularProfile3JExplodeAvg <- function (npj)
{
	return (mean(c( npj[[1]]$c$cl.rfd.avg, npj[[2]]$c$cl.rfd.avg, npj[[3]]$c$cl.rfd.avg)))
}

neuromuscularProfile3JDriveAvg <- function (npj)
{
	return (mean(c( npj[[1]]$c$cl.i, npj[[2]]$c$cl.i, npj[[3]]$c$cl.i)))
}

neuromuscularProfile3JAvg <- function (jump1, jump2, jump3)
{
	jumpAvg <- NULL
	for (i in 1:length(jump1))
		jumpAvg <- c(jumpAvg, mean (c(jump1[i], jump2[i], jump3[i])))

	return (jumpAvg)
}

neuromuscularProfileWriteData1Person <- function(npj, names_c, outputData1)
{	
	#values of first, 2nd and 3d jumps
	jump1 <- as.numeric(c(npj[[1]]$e1, npj[[1]]$c))
	jump2 <- as.numeric(c(npj[[2]]$e1, npj[[2]]$c))
	jump3 <- as.numeric(c(npj[[3]]$e1, npj[[3]]$c))

	df <- data.frame(rbind(jump1,jump2,jump3,
			       neuromuscularProfile3JAvg (jump1, jump2, jump3)))

	df <- cbind (rep(names_c[1], 4),
		     c(npj[[1]]$l.context$numJump, npj[[2]]$l.context$numJump, npj[[3]]$l.context$numJump, "AVG"),
		     77, #TODO: put weight here
		     df)

	#colnames(df) <- c(paste("e1.",names(npj[[1]]$e1),sep=""), names(npj[[1]]$c))
	colnames(df) <- c("person", "jump.n", "mass.extra", paste("e1.",names(npj[[1]]$e1),sep=""), names(npj[[1]]$c))
	#rownames(df) <- c(
	#		  paste(translateToPrint("jump"),npj[[1]]$l.context$numJump), 
	#		  paste(translateToPrint("jump"),npj[[2]]$l.context$numJump),
	#		  paste(translateToPrint("jump"),npj[[3]]$l.context$numJump),
	#		  "AVG")
	rownames(df) <- 1:4
	print(df)

	write.csv(df, outputData1, quote=FALSE)
}
# <--------- 1 person ------------------


# --------- n persons ------------------>
neuromuscularProfile3NAvg <- function (npj_l, type)
{
	sum <- 0
	n <- length (npj_l) *3

	for (i in 1:length (npj_l))
	{
		npj <- npj_l[[i]]
		if (type == "LOAD")
			sum = sum + npj[[1]]$e1$rfd.avg + npj[[2]]$e1$rfd.avg + npj[[3]]$e1$rfd.avg
		else if (type == "EXPLODE")
			sum = sum + npj[[1]]$c$cl.rfd.avg + npj[[2]]$c$cl.rfd.avg + npj[[3]]$c$cl.rfd.avg
		else if (type == "DRIVE")
			sum = sum + npj[[1]]$c$cl.i + npj[[2]]$c$cl.i + npj[[3]]$c$cl.i
	}

	if (! is.numeric (sum) || ! is.numeric (n))
		return (0)
	else if (sum == 0 || n == 0)
		return (0)
	else
		return (sum/n)
}

neuromuscularProfileWriteDataNPersons <- function(npj_l, names_c, outputData1)
{
	dfBig <- data.frame()
	rowname <- 1
	for (i in 1:length (npj_l))
	{
		npj <- npj_l[[i]]

		#values of first, 2nd and 3d jumps
		jump1 <- c(as.numeric(c(npj[[1]]$e1, npj[[1]]$c)))
		jump2 <- c(as.numeric(c(npj[[2]]$e1, npj[[2]]$c)))
		jump3 <- c(as.numeric(c(npj[[3]]$e1, npj[[3]]$c)))

		df <- data.frame(rbind(jump1,jump2,jump3, neuromuscularProfile3JAvg (jump1, jump2, jump3)))

		df <- cbind (names_c[i],
		       c(npj[[1]]$l.context$numJump, npj[[2]]$l.context$numJump, npj[[3]]$l.context$numJump, "AVG"),
		       77, #TODO: put weight here
		       df)

		print ("df")
		print (df)
		print ("colnames:")
		print (c("person", "jump.n", "mass.extra", paste("e1.",names(npj[[1]]$e1),sep=""), names(npj[[1]]$c)))

		colnames(df) <- c("person", "jump.n", "mass.extra", paste("e1.",names(npj[[1]]$e1),sep=""), names(npj[[1]]$c))
#		rownames(df) <- c(names_c[i], names_c[i], names_c[i], names_c[i])
		rownames(df) <- c(rowname, rowname +1, rowname +2, rowname +3)
		rowname = rowname +4

		dfBig <- rbind (dfBig, df)
	}
	write.csv(dfBig, outputData1, quote=FALSE)
}

# <--------- n persons ------------------
