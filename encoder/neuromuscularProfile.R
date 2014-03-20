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
#   Copyright (C) 2014  	Xavier de Blas <xaviblas@gmail.com> 
# 


g = 9.81


#comes with every jump of the three best (in flight time)
#e1, c, e2 are displacements
neuromuscularProfileJump <- function(e1, c, e2, mass, smoothingC)
{
	#          /\
	#         /  \ 
	# _     c/    \e2
	#  \    /      \
	# e1\  /        \
	#    \/          \
	
	weight <- mass * g

	#----------------
	#1.- e1 variables
	#----------------

	#e1.range <- range of e1
	e1.pos <- cumsum(e1)
	e1.range <- e1.pos[length(e1.pos)]

	#e1f (when force is done)
	#from max(abs(speed$y)) at e1, to end of e1
	e1.speed <- getSpeed(e1, smoothingC)
	e1f <- e1[max(abs(e1.speed$y)):length(e1)]

	#e1f.t duration of e1f
	e1f.t <- length(e1f)

	#e1f.fmax <- max Force on e1f
	e1f.speed <- getSpeed(e1f, smoothingC)
	e1f.accel <- getAcceleration(e1f.speed)
	e1f.accel$y <- e1f.accel$y * 1000
	e1f.force <- mass * (e1f.accel$y + g)
	e1f.fmax <- max(e1f.force)

	#e1f.rdf.avg
	#average force on e1f / e1f.t
	e1f.rfd.avg <- mean(e1f.force) / e1f.t #bars LOAD

	#e1f.i (Impulse)
	#average force on e1f * e1f.t / weight
	e1f.i <- mean(e1f.force) * e1f.t / weight 

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
	c.accel = getAcceleration(c.speed) 
	#speed comes in mm/ms when derivate to accel its mm/ms^2 to convert it to m/s^2 need to *1000 because it's quadratic
	c.accel$y <- c.accel$y * 1000
	c.force <- mass * (c.accel$y + g)

	c.position = cumsum(c)
	c.takeoff = min(which(c.force <= weight))
	#c.jumpHeight = (c.position[length(c.position)] - c.position[c.takeoff]) /10

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
	
	#cl.rfd.avg = average force on cl / cl.t / weight #bars EXPLODE
	cl.speed <- getSpeed(cl, smoothingC)
	cl.accel = getAcceleration(cl.speed) 
	#speed comes in mm/ms when derivate to accel its mm/ms^2 to convert it to m/s^2 need to *1000 because it's quadratic
	cl.accel$y <- cl.accel$y * 1000
	cl.force <- mass * (cl.accel$y + g)

	cl.rfd.avg <- mean(cl.force) / cl.t / weight
	
	#cl.i = average force on cl * cl.t / weight #impulse #bars DRIVE
	cl.i <- mean(cl.force) * cl.t / weight

	#cl.f.avg = average force on cl / weight
	cl.f.avg <- mean(cl.force) / weight

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

	#----------------
	#3.- e2 variables
	#----------------

	#get landing
	e2.speed <- getSpeed(e2, smoothingC)
	e2.accel = getAcceleration(e2.speed) 
	#speed comes in mm/ms when derivate to accel its mm/ms^2 to convert it to m/s^2 need to *1000 because it's quadratic
	e2.accel$y <- e2.accel$y * 1000
	e2.force <- mass * (e2.accel$y + g)
	e2.land.pos = max(which(e2.force <= weight))

	#e2f (when force is done)
	#is the same as contact phase (land on eccentric)
	e2f <- e2[e2.land.pos:length(e2)]
	
	#e2f.t duration of e2f
	e2f.t <- length(e2f) 

	#for this variables, we use e2 instead of e2f because there's lot more force on e2f
	#so there's no need to use e2f
	#e2f.f.max = max force on e2f
	e2f.f.max <- max(e2.force)

	#e2fFmaxt = duration from land to max force
	e2f.f.max.t <- min(which(e2.force == e2f.f.max)) - e2.land.pos

	#e2f.rfd.max = e2f.f.max / e2f.f.max.t
	e2f.rfd.max <- e2f.f.max / e2f.f.max.t

	e2.list = list(
		      e2f.t = e2f.t,
		      e2f.f.max  = e2f.f.max,
		      e2f.f.max.t  = e2f.f.max.t,
		      e2f.rfd.max  = e2f.rfd.max
		      )

	#return an object, yes, object oriented, please
	return (list(e1 = e1.list, c = c.list, e2 = e2.list))
}

#Manuel Lapuente analysis of 6 separate ABKs (e1, c, e2)
neuromuscularProfileGetData <- function(displacement, curves, mass, smoothingC)
{
	weight=mass*g

	#get the maxheight of the 6 jumps
	#sequence is e,c,e,c for every jump. There are 6 jumps. Need the first c of every jump
	nums = NULL
	heights = NULL
	count = 1
	for(i in seq(2,22,length=6)) {
		d = displacement[curves[i,1]:curves[i,2]]
		speed <- getSpeed(d, smoothingC)
		
		accel = getAcceleration(speed) 
		#speed comes in mm/ms when derivate to accel its mm/ms^2 to convert it to m/s^2 need to *1000 because it's quadratic
		accel$y <- accel$y * 1000
		
		force <- mass * (accel$y + g)

		position = cumsum(d)
		takeoff = min(which(force <= weight))
		jumpHeight = (position[length(position)] - position[takeoff]) /10
		print(paste("Jump Height =", jumpHeight))

		#store variables
		nums[count] = i
		heights[count] = jumpHeight
		count = count +1
	}

	df=data.frame(cbind(nums,heights))
	bests=rev(order(df$heights))[1:3]
	
	print(c("best three jumps are:", df$nums[bests]))
	print(c("heights are:", df$heights[bests]))

	#with the best three jumps (in jump height) do:

	npj <- list()
	count = 1
	for(i in df$nums[bests]) {
		npj[[count]] <- neuromuscularProfileJump(
						  displacement[curves[(i-1),1]:curves[(i-1),2]],	#e1
						  displacement[curves[(i),1]:curves[(i),2]],	#c
						  displacement[curves[(i+1),1]:curves[(i+1),2]],	#e2
						  mass, smoothingC)
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

neuromuscularProfilePlotBars <- function(load, explode, drive)
{
	barplot(c(load,explode,drive),col=topo.colors(3),names.arg=c("Load","Explode","Drive"))
	print(c("load, explode, drive", load, explode, drive))
	
	#show small text related to graph result and how to train
}

neuromuscularProfilePlotOther <- function() 
{
	#plot
	#curve e1,c,e2 distance,speed,force /time of best jump
	#curve e1,c,e2 force/time  (of the three best jumps)
	#to plot e1,c,e2 curves, just sent to paint() the xmin:xmax from start e1 to end of e2
}

neuromuscularProfileWriteData <- function(npj, outputData1)
{	
	#values of first, 2nd and 3d jumps
	jump1 <- as.numeric(c(npj[[1]]$e1, npj[[1]]$c, npj[[1]]$e2))
	jump2 <- as.numeric(c(npj[[2]]$e1, npj[[2]]$c, npj[[2]]$e2))
	jump3 <- as.numeric(c(npj[[3]]$e1, npj[[3]]$c, npj[[3]]$e2))

	df <- data.frame(rbind(jump1,jump2,jump3))
	colnames(df) <- c(paste("e1.",names(npj[[1]]$e1),sep=""), names(npj[[1]]$c), names(npj[[1]]$e2))
	print(df)

	write.csv(df, outputData1, quote=FALSE)
}

