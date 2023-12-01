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
#   Copyright (C) 2004-2023  	Xavier de Blas <xaviblas@gmail.com>
#   Copyright (C) 2014-2020   	Xavier Padullés <x.padulles@gmail.com>
# 

#TODO: current BUGS
#peakpowerTime is not working ok at con-ecc
#paint "eccentric","concentric" labels are not ok at con-ecc


#----------------------------------
#Explanation of the code. Workflow:
#----------------------------------
#Define some variables, read options file and define more variables
#at end of file: call to loadLibraries(OperatingSystem)
#at end of file: call to doProcess(options)
#doProcess:
#assign variables reading options
#process curves:
#if(! singleFile) reads "chronojump-encoder-graph-input-multi.csv", then read each file and define curves using files
#if(singleFile) define curves using findCurves function
#if analysis is single: paint 
#if analysis is side: kinematicRanges will call kinematicsF to know common axes (max and mins) and the call to paint 
#using curves and powerBars, paf table will be created. This will be used always, because writeCurves (on a file) is always TRUE
#if(Analysis=="exportCSV") data will be exported to CSV file
#----------------------------------

#TODO: bug: 705214 - Encoders selection and management
#Change notebook to have a third horizontal tab
#
#Inertial machines has to be always using the axis:
#1. if it's rotatory (friction): in contact with axis
#2. if it's rotatory (axis): connected to axis
#3. if it's linear (string): rolled at the axis
#
#in all need to provide the inertia momentum, body weight and extra weight (if
#									   any)
#
#in the 2. and 3. need to provide the diameter of the axis 
#
#In all the cases, need to convert data using fixRawdataInertial
#because we need to know the change of direction
#
#also preselect con-ecc when inertial machine is selected. Put in the same tab
#
#Need also to save encoder type and load encoder type. This has to be useful to
#have data introduced always and have different encoders.
#
#

debugOld = FALSE #this will be deprecated and DEBUG will be used
displacementDebug <<- NULL # just to debug
curvesDebug <<- NULL
ecconDebug <<- NULL
singleFileDebug <<- NULL

displacementCurvesDebug <- function (plot)
{
	filename = paste (DebugFileName, "-singleFile", singleFileDebug, "-", ecconDebug, sep="")
	if (plot)
	{
		plot (cumsum(displacementDebug), type="l")
		abline (v=curvesDebug[,1], col="red")
		abline (v=curvesDebug[,2], col="blue")
	}

	if (file.exists (filename))
		file.remove (filename)

        write (paste("Eccon", ecconDebug), filename, append=TRUE)
	write ("\ndisplacement", filename, append=TRUE)
	write (displacementDebug, filename, ncolumns=20, append=TRUE, sep=",")
	for (i in 1:length (curvesDebug[,1]))
	{
		write (paste("\ncurve:",rownames(curvesDebug[i,])), filename, append=TRUE)
		write (displacementDebug[curvesDebug[i,1]:curvesDebug[i,2]], filename, ncolumns=20, append=TRUE, sep=",")
	}
}

#concentric, eccentric-concentric, repetitions of eccentric-concentric
#currently only used "c" and "ec". no need of ec-rep because c and ec are repetitive
#"ecS" is like ec but eccentric and concentric phases are separated, used in findCurves, this is good for treeview to know power... on the 2 phases
eccons=c("c","ec","ecS","ce","ceS") 

g = 9.81

colSpeed="springgreen3"; colForce="blue2"; colPower="tomato2"	#colors
#colSpeed="black"; colForce="black"; colPower="black"		#black & white
cols=c(colSpeed,colForce,colPower); lty=rep(1,3)	

unicodeWorks = FALSE

#needed for some windows machines
checkUnicodeWorks <- function()
{
        tryCatch (
                {
			#this can make the test fail
                        #plot(1,1, xlab="unicode stuff: \u00ED \u00E1")
			#maybe it failed because in the past was together and with \U
			#just print and is safer:
                        print("unicode stuff: \u00ED \u00E1")
			if(DEBUG)
				write("unicode stuff: \u00ED \u00E1", DebugFileName, append=TRUE)

			return(TRUE)
                }, 
                error=function(cond) { 
                        message(cond)
                        return(FALSE) }
        )
}


#translateToPrint An expression is returned andcan only be printed. Don't do operations
#Important: An expression is returned because is the best way to handle unicode on windows
#take care not to do operations with this. Just print it
translateToPrint <- function(englishWord) 
{
        if(! unicodeWorks)
                return (englishWord) #unicode is not working, return english word
        
        if(length(Translated[which(English == englishWord)]) == 0)
                return (englishWord) #not found, return english word
        
        myWord = Translated[which(English == englishWord)]
        
        #Needed conversion for Windows:
        #unicoded titles arrive here like this "\\", convert to "\", as this is difficult, do like this:
        #http://stackoverflow.com/a/17787736
	#myWord = parse(text = paste0("'", myWord, "'"))
	#but also as.character in order to be able to do combines c() on paintCrossVariables() and operations on f.horlegend()
	myWord = as.character(parse(text = paste0("'", myWord, "'")))
        
        return(myWord)
}

translateVector <- function(englishVector) {
        translatedVector <- englishVector
        for(i in 1:length(englishVector)) {
                translatedVector[i] <- translateToPrint(englishVector[i])
        }
        
        return(translatedVector)
}

findCurvesByTriggers <- function(displacement, triggersOnList)
{
        position <- cumsum(displacement)
        
        start  	<- 0
        end    	<- 0
        startH 	<- 0

        print(c("triggersOnList:", triggersOnList))

	start[1]  <- 1
	startH[1] <- 0
	end[1]    <- (triggersOnList[1] -1)

	if(length(triggersOnList) == 1)
		return(as.data.frame(cbind(start, end, startH)))

	for( i in 1:(length(triggersOnList) -1) )
        {
                start[i+1]  <- triggersOnList[i]
                startH[i+1] <- position[triggersOnList[i]]
                end[i+1]    <- (triggersOnList[i+1] -1)
        }
        
        return(as.data.frame(cbind(start, end, startH)))
}

#this is equal to runEncoderCaptureCsharp()
#but note getDisplacement hapens before this function, so no need getDisplacement here
#also don't need byteReadedRaw, and encoderReadedRaw. encoderReaded is 'displacement' here
findCurvesNew <- function(displacement, eccon, inertial, min_height)
{
        #---- 1) declare variables ----
        
        byteReaded = NULL
        
        position = cumsum(displacement)
        
        sum = 0
        
        directionChangePeriod = 25
        directionChangeCount = 0
        directionNow = 1
        directionLastMSecond = 1
        directionCompleted = -1
        previousFrameChange = 0
        previousEnd = 1
        lastNonZero = 0
        
        heightAtCurveStart = 0
        heightAccumulated = 0
        
        inertialCaptureDirectionChecked = FALSE
        inertialCaptureDirectionInverted = FALSE
        
        capturingFirstPhase = TRUE
        
        startCurrent = NULL
        endCurrent = NULL
        
        startStored = 0
        endStored= 0
        startHStored = 0
        
        #curve we are about to store is concentric
        directionToStoreIsCon = NULL #bool
        #last stored curve is concentric. It's important on 'ec' to store first 'e', next 'c' ...
        directionStoredIsCon = NULL #bool
        
        #               3
        #              / \
        #             /   B
        #            /     \
        # --1       /
        #    \     /
        #     \   A
        #      \2/
        #
        # At B, we evaluate if 2-3 curve is concentric.
        # directionToStoreIsCon == TRUE
        
        
        #               3               5
        #              / \             /
        #             /   B           /
        #            /     \         /
        # --1       /       \       /
        #    \     /         \     /
        #     \   A           \   C
        #      \2/             \4/
        #
        # At C, we evaluate if 3-4 curve is eccentric.
        # directionToStoreIsCon == FALSE
        # Also we see that last stored curve is opposite:
        # directionStoredIsCon == TRUE
        #
        
        
        row = 1
        
        count = 1
        
        #---- 2) start ----
        
        while(count <= length(displacement)) {
                
                byteReaded = displacement[count]
                
                #TODO: need this?	
                #if(inertialCaptureDirectionInverted)
                #	byteReadedRaw = -1 * byteReadedRaw
                
                sum = sum + byteReaded
                
                #the inertial change of direction is not needed here because runEncoderCaptureCsharp() stores data already 'changed'
                #so no change here is needed
                
                #if string goes up or down, store the direction
                if(byteReaded != 0)
                        directionNow = byteReaded / ( 1.0 * abs(byteReaded) ) #1 (up) or -1 (down)
                
                #if we don't have changed the direction, store the last non-zero that we can find
                if(directionChangeCount == 0 && directionNow == directionLastMSecond) {
                        if(byteReaded != 0)
                                lastNonZero = count
                }
                
                #if it's different than the last direction, mark the start of change
                if(directionNow != directionLastMSecond) {
                        directionLastMSecond = directionNow
                        directionChangeCount = 0
                } 
                else if(directionNow != directionCompleted) {
                        #we are in a different direction than the last completed
                        
                        directionChangeCount = directionChangeCount +1
                        
                        if(directionChangeCount > directionChangePeriod)
                        {
                                startFrame = previousEnd
                                if(startFrame < 1)
                                        startFrame = 1
                                
                                startCurrent = startFrame
                                #-1 are because on C# count starts at 0 and here count starts at 1
                                endCurrent = ( (count -1) - directionChangeCount + (lastNonZero -1) ) / 2
                                
                                if(endCurrent > startCurrent)
                                {
                                        heightAtCurveStart = heightAccumulated
                                        
                                        heightCurve = max(position[startCurrent:endCurrent]) - min(position[startCurrent:endCurrent])
                                        
                                        previousEnd = endCurrent
                                        
                                        heightAccumulated = heightAccumulated + heightCurve
                                        
                                        heightCurve = abs(heightCurve) #mm -> cm
                                        
                                        sendCurve = TRUE
                                        
                                        if(heightCurve >= min_height) {
                                                #TODO: add the inertia stuff here?
                                                
                                                directionToStoreIsCon = ( position[endCurrent] > position[startCurrent] )
                                                
                                                if( eccon == "c" && ! directionToStoreIsCon )
                                                        sendCurve = FALSE
                                                if( (eccon == "ec" || eccon == "ecS") && directionToStoreIsCon && capturingFirstPhase )
                                                        sendCurve = FALSE

                                                #on ec, ecS don't have store two curves in the same direction
                                                if( (eccon == "ec" || eccon == "ecS") && 
                                                    ! is.null(directionStoredIsCon) && 
                                                    directionToStoreIsCon == directionStoredIsCon )
                                                        sendCurve = FALSE
                                                
                                                if(sendCurve)
                                                        capturingFirstPhase = FALSE
                                        } else {
                                                sendCurve = FALSE
                                        }
                                        
                                        #store the curve
                                        if(sendCurve) {
                                                startStored[row] = startCurrent
                                                endStored[row] = endCurrent
                                                startHStored[row] = position[startCurrent]
                                                row = row + 1
                                                directionStoredIsCon = directionToStoreIsCon
                                        }
                                }
                                
                                previousFrameChange = count - directionChangeCount
                                directionChangeCount = 0
                                directionCompleted = directionNow
                        }
                }
                
                count = count +1
        }
        
        #if eccon it's 'ec' and last row it's 'e', delete it
        if(row > 1)
        {
                startStoredLast = startStored[length(startStored)]
                endStoredLast = endStored[length(endStored)]
                
                #write("startStoredLast, endStoredLast", stderr())
                #write(c(startStoredLast, endStoredLast), stderr())
                
                if ( position[startStoredLast] > position[endStoredLast] ) 
                {
                        write("deleting last ecc row", stderr())
                        startStored = startStored[-length(startStored)]
                        endStored = endStored[-length(endStored)]
                        startHStored = startHStored[-length(startHStored)]
                }
        }
        
        #if eccon == "ec" mix 'e' and 'c' curves
        if(eccon == "ec") {
                startStoredOld = startStored
                endStoredOld = endStored
                startHStoredOld = startHStored
                
                startStored = NULL
                endStored = NULL
                startHStored = NULL
                
                n=length(startStoredOld)
                count = 1
                for(i in seq(1, n, by=2)) {
                        startStored[count] = startStoredOld[i]
                        endStored[count] = endStoredOld[(i+1)]
                        startHStored[count] = startHStoredOld[i]
                        count = count +1
                }
        }
        
        
        if( inertial && (eccon == "ec" || eccon == "ecS") ) 
        {
                #be careful on ec inertial to send an startStored of 1220.5 because can use the 1220 that can be a positive displacement
                #and then the 1221 can be negative and also the rest
                #then is better to take the second number (do ceiling) to avoid a change of direction on the beginning of the movement
                #
                #    AB          C
                #    /\          /\
                #   /  \        /
                #  /    \      /
                # /      \    /
                #         \__/
                #
                # On this example a ec can go from B to C. If it uses A,
                #then there will be a change of direction that will make getSpeed to produce a mistaken spline
                
                for(i in 1:length(startStored)) {
                        startStored[i] = ceiling(startStored[i])
                }
                
                #When we have this
                #        _
                #       / \ 
                #      /   \
                #     /     \
                #- - - - - - - - -
                #   /         \
                #  /           \
                # /             \
                #getDisplacementInertialBody converts it to:
                #
                #
                #
                #   AB      CD
                #- - - - - - - - -
                #   / \     / \
                #  /   \   /   \
                # /     \_/     \
                #
                #AB are two points of same position.
                #note that first ascending phase will be cutted by findCurvesNew in B
                #and then this will mean a big change in direction at the end (going up to a pixel-change-to-horizontal)
                #splines are very problematic if there are pixel changes at the end
                #so, make end in A
                
                for(i in 1:length(endStored)) {
                        #endStored[i] = floor(endStored[i])
                        
                        #this if does not work because sometimes the difference is very tiny
                        #	print(position[endStored[i]])
                        #	[1] -0.29
                        #	print(position[(endStored[i] -1)])
                        #	[1] -0.29
                        #	position[endStored[i]] == position[(endStored[i] -1)]
                        #	FALSE
                        #	print(position[endStored[i]] - position[(endStored[i] -1)])
                        #	[1] 1.965095e-14
                        #
                        #if(position[endStored[i]] == position[(endStored[i] -1)]) {
                        #	endStored[i] = endStored[i] -1
                        #}
                        endStored[i] = endStored[i] -1
                }
        }
        
        
        return(as.data.frame(cbind(startStored,endStored,startHStored)))
}

startCurvesPlot <- function(position, title)
{
        lty=1
        col="black"
        plot((1:length(position))/1000			#ms -> s
             ,position/10,				#mm -> cm
             type="l",
             xlim=c(1,length(position))/1000,		#ms -> s
             xlab="",ylab="",axes=T,
             lty=lty,col=col)
        
        title(title, cex.main=1, font.main=1)
        mtext(paste(translateToPrint("time"),"(s)"),side=1,adj=1,line=-1)
        mtext(paste(translateToPrint("displacement"),"(cm)"),side=2,adj=1,line=-1)
}

kinematicRanges <- function(singleFile, displacement, curves,
                            massBody, massExtra, exercisePercentBodyWeight,
                            encoderConfigurationName,diameter,diameterExt,anglePush,angleWeight,inertiaMomentum,gearedDown,
                            smoothingsEC, smoothingOneC, g, eccon, isPropulsive, minHeight) {
        
        n=length(curves[,1])
        maxSpeedy=0; maxAccely=0; maxForce=0; maxPower=0
        
        for(i in 1:n) { 
                repOp <- assignRepOptions(
                        singleFile, curves, i,
                        massBody, massExtra, eccon, exercisePercentBodyWeight, 
                        encoderConfigurationName, diameter, diameterExt, 
                        anglePush, angleWeight, inertiaMomentum, gearedDown,
                        "") #laterality 
                
                kn <- kinematicsF(displacement[curves[i,1]:curves[i,2]],
                                  repOp, smoothingsEC[i], smoothingOneC, g, isPropulsive, TRUE,
				  minHeight)

                if(max(abs(kn$speedy)) > maxSpeedy)
                        maxSpeedy = max(abs(kn$speedy))
                if(max(abs(kn$accely)) > maxAccely)
                        maxAccely = max(abs(kn$accely))
                if(max(abs(kn$force)) > maxForce)
                        maxForce = max(abs(kn$force))
                if(max(abs(kn$power)) > maxPower)
                        maxPower = max(abs(kn$power))
        }
        
        return(list(
                speedy=c(-maxSpeedy,maxSpeedy),
                accely=c(-maxAccely,maxAccely),
                force=c(-maxForce,maxForce),
                power=c(-maxPower,maxPower)))
}

canJump <- function(encoderConfigurationName)
{
        if(encoderConfigurationName == "LINEAR" || encoderConfigurationName == "LINEARINVERTED")
                return(TRUE)
        
        return(FALSE)
}

paint <- function(displacement, eccon, xmin, xmax, xrange, yrange, knRanges, paintMode, nrep, highlight,
                  startX, startH, smoothingOneEC, smoothingOneC, massBody, massExtra, minHeight,
                  encoderConfigurationName,diameter,diameterExt,anglePush,angleWeight,inertiaMomentum,gearedDown,laterality, #encoderConfiguration stuff
                  title, subtitle, draw, width, showLabels, marShrink, showAxes, legend,
                  Analysis, isPropulsive, inertialType, exercisePercentBodyWeight,
                  showPosition, showSpeed, showAccel, showForce, showPower,
		  triggersOnList #will be empty if cutByTriggers
		  ) {
        
        meanSpeedE = 0
        meanSpeedC = 0
        meanPowerE = 0
        meanPowerC = 0
        
        smoothing = 0
        if(eccon == "c")
                smoothing = smoothingOneC
        else
                smoothing = smoothingOneEC
        
        write(c("paint smoothing", smoothing), stderr())
        
        #eccons ec and ecS is the same here (only show one curve)
        #receive data as cumulative sum
        #lty=c(1,1,1)
        
        print(c("xmin,xmax",xmin,xmax))
        
	displacement=displacement[xmin:xmax]
	if(eccon=="c")
		displacement <- reduceCurveByPredictStartEnd (displacement, "c", minHeight)$curve
	else
		displacement <- reduceCurveByPredictStartEnd (displacement, "ec", minHeight)$curve

        position=cumsum(displacement)
        position=position+startH
        
        #to control the placement of the diferent axis on the right
        axisLineRight = 0
        marginRight = 8.5
        if(! showSpeed)
                marginRight = marginRight -2
        if(! showAccel)
                marginRight = marginRight -2
        if(! showForce)
                marginRight = marginRight -2
        if(! showPower)
                marginRight = marginRight -2
        
        #all in meters
        #position=position/1000

	#set colors
        colPosition = "black"
	colSpeed = cols[1]
	colAccel = "magenta"
	colForce = cols[2]
	colPower = cols[3]

	ltyPosition = 1
	ltySpeed = 1
	ltyAccel = 1
	ltyForce = 1
	ltyPower = 1

	superposeSeqCharsN = 5
        
        if(draw) {
                #three vertical axis inspired on http://www.r-bloggers.com/multiple-y-axis-in-a-r-plot/
                par(mar=c(3, 3.5, 5, marginRight))
                if(marShrink) #used on "side" && "sideShareX" compare
                        par(mar=c(1, 1, 4, 1))
                
                #plot distance
                #plot(a,type="h",xlim=c(xmin,xmax),xlab="time (ms)",ylab="Left: distance (mm); Right: speed (m/s), acceleration (m/s^2)",col="gray", axes=F) #this shows background on distance (nice when plotting distance and speed, but confusing when there are more variables)
                xlab="";ylab="";
                #if(showLabels) {
                #	xlab="time (ms)"
                #	ylab="Left: distance (mm); Right: speed (m/s), force (N), power (W)"
                #}

		xlim=xrange
                if(xlim[1]=="undefined") { xlim=c(1,length(position)) }

                ylim=yrange
                if(ylim[1]=="undefined") { ylim=NULL }

		plot(position-min(position),type="n",xlim=xlim,ylim=ylim,xlab=xlab, ylab=ylab, col="gray", axes=F)
                
                title(main=title,line=-2,outer=T)
                mtext(subtitle,side=1,adj=0,cex=.8)
                
                
                if(showAxes) {
                        axis(1) 	#can be added xmin
			if(showPosition)
				axis(2)
                }
                
		if(showPosition)
		{
			par(new=T)
			#                if(superpose)
			#                        colNormal="gray30"
			yValues = position[startX:length(position)]-min(position[startX:length(position)])
			#                if(highlight==FALSE)
			plot(startX:length(position),yValues,type="l",xlim=xlim,ylim=ylim,
			     xlab="",ylab="",col=colPosition,lty=ltyPosition,lwd=2,axes=F)

			if(paintMode == "superpose")
				addRepCharsAboveLine(yValues, colPosition, nrep, laterality)

			# show horizontal bars on all graphs except on superpose (on this mode only on first graph)
			if(paintMode != "superpose" || nrep == 1)
			{
				abline(h=0,lty=3,col="black")
			}
		}
                #abline(v=seq(from=0,to=length(position),by=500),lty=3,col="gray")
        }
        
        print(c("smoothing at paint=",smoothing))
        #speed
        speed <- getSpeed(displacement, smoothing)

        #accel (calculated here to use it on fixing inertialECstart and before plot speed
        accel <- getAcceleration(speed)
        #speed comes in mm/ms when derivate to accel its mm/ms^2 to convert it to m/s^2 need to *1000 because it's quadratic
        accel$y <- accel$y * 1000
        
	xlim=xrange
        if(xlim[1]=="undefined") { xlim=c(1,length(displacement)) }

        #declare variables:
        eccentric=NULL
        isometric=NULL
        concentric=NULL
        
        if(eccon=="c") {
		concentric <- reduceCurveByPredictStartEnd (displacement, "c", minHeight)$curve
                concentric = 1:length(concentric)
		printLHT (concentric, "concentric after reduce")
        } else
	{	#"ec", "ce". Eccons "ecS" and "ceS" are not painted
		labelsXeXc = c("Xe","Xc") # on unused eccon == ce: labelsXeXc = c("Xc","Xe")
                
                print(c("eccon",eccon))
		phases_l <- findECPhases (displacement, minHeight)
		eccentric <- phases_l$eccentric
		isometric <- phases_l$isometric
		concentric <- phases_l$concentric

                if(draw && paintMode != "superpose")
		{
                        abline(v=max(eccentric),col=cols[1])
                        abline(v=min(concentric),col=cols[1])
                        #mtext(text=paste(max(eccentric)," ",sep=""),side=1,at=max(eccentric),adj=1,cex=.8,col=cols[1])
                        #mtext(text=paste(" ",min(concentric),sep=""),side=1,at=min(concentric),adj=0,cex=.8,col=cols[1])

			mtext(text=paste(round(max(eccentric),1), " ",sep=""),
                              side=1,at=max(eccentric),adj=1,cex=.8,col=cols[1])
                        mtext(text=paste(" ", round(min(concentric),1),sep=""),
                              side=1,at=min(concentric),adj=0,cex=.8,col=cols[1])
                        
                        #don't need to show eccentric and concentric. It's pretty clear
                        #mtext(text=paste(translateToPrint("eccentric")," ",sep=""),side=3,at=max(eccentric),cex=.8,adj=1,col=cols[1],line=.5)
                        #mtext(text=paste(" ",translateToPrint("concentric"),sep=""),side=3,at=min(concentric),cex=.8,adj=0,col=cols[1],line=.5)
                        mtext(text="ecc ",side=3,at=max(eccentric),cex=.8,adj=1,col=cols[1],line=.5)
                        mtext(text=" con",side=3,at=min(concentric),cex=.8,adj=0,col=cols[1],line=.5)
                }
        }
        
        #time to arrive to max speed
        maxSpeedT=min(which(speed$y == max(speed$y)))
        
        maxSpeedTInConcentric = maxSpeedT
        if(eccon != "c")
                maxSpeedTInConcentric = maxSpeedT - (length(eccentric) + length(isometric))
        
        #define a propulsiveEnd value because it's used also in non-propulsive curves
        propulsiveEnd = length(displacement)
        
        if(isPropulsive) {
                propulsiveEnd = findPropulsiveEnd(accel$y, concentric, maxSpeedTInConcentric,
                                                  encoderConfigurationName, anglePush, angleWeight, 
                                                  massBody, massExtra, exercisePercentBodyWeight)
                if(eccon != "c")
                        propulsiveEnd = length(eccentric) + length(isometric) + propulsiveEnd
        }

	#draw polygon under position, but only if position is shown
	if(paintMode != "superpose" && showPosition) {
		polygon(c(startX:propulsiveEnd, propulsiveEnd, startX),
			c(yValues[startX:propulsiveEnd], min(yValues), min(yValues)),
			col="grey90")
	}

	if(triggersOnList != "" && triggersOnList != -1)
	{
		print("triggersOnList-xmin")
		print(triggersOnList-xmin)
		abline(v=(triggersOnList-xmin), col="yellow3", lwd=2, lty=2)
		#mtext(side=3, at=(triggersOnList-xmin), text=(triggersOnList-xmin), cex=.8)
	}

        print(c("propulsiveEnd at paint", propulsiveEnd))
        
        
        # ---- start of inertialECstart ----
        
        #on inertial ec. If the accel starts as negative, calcule avg values and peak values starting when the accel is positive
        #because the acceleration done on the disc can only be positive
        #inertialECstart = 1
        #if(length(eccentric) > 0)
        #	inertialECstart = min(eccentric)
        
        #as acceleration can oscillate, start at the eccentric part where there are not negative values
        #if(inertiaMomentum > 0 && eccon == "ec" && 
        #   length(eccentric) > 0 && min(accel$y[eccentric]) < 0) #if there is eccentric data and there are negative vlaues
        #{ 
        #	inertialECstart = max(which(accel$y[eccentric] < 0)) +1
        #	#abline(v=inertialECstart,lty=3,col="black") 
        #}
        #print(c("inertialECstart", inertialECstart))
        #print(c("accel$y[eccentric]", accel$y[eccentric]))
        #print(displacement[eccentric])
        
        #startX = inertialECstart #deactivated
        
        # ---- end of inertialECstart ----

        if(draw & showSpeed) {
                ylimHeight = max(abs(range(speed$y)))
                ylim=c(- 1.05 * ylimHeight, 1.05 * ylimHeight)	#put 0 in the middle, and have 5% margin at each side
                if(knRanges[1] != "undefined")
                        ylim = knRanges$speedy
                par(new=T)
                
                speedPlot=speed$y
                #on rotatory inertial, concentric-eccentric, plot speed as ABS)
                #if(inertialType == "ri" && eccon == "ce")
                #	speedPlot=abs(speed$y)
                
                #if(highlight==FALSE)
                        plot(startX:length(speedPlot),speedPlot[startX:length(speedPlot)],type="l",
                             xlim=xlim,ylim=ylim,xlab="",ylab="",col=colSpeed,lty=ltySpeed,lwd=1,axes=F)

		if(paintMode == "superpose")
			addRepCharsAboveLine(speedPlot, colSpeed, nrep, laterality)
                #else
                #        plot(startX:length(speedPlot),speedPlot[startX:length(speedPlot)],type="l",
                #             xlim=xlim,ylim=ylim,xlab="",ylab="",col="darkgreen",lty=2,lwd=3,axes=F)
                
        }
        
        if(draw & showSpeed & paintMode != "superpose")
	{
                abline(v=maxSpeedT, col=cols[1])
                points(maxSpeedT, max(speed$y),col=cols[1])
                mtext(text=paste(round(max(speed$y),2),"m/s",sep=""),side=3,
                      at=maxSpeedT,cex=.8,col=cols[1], line=.5)
                mtext(text=maxSpeedT,side=1,at=maxSpeedT,cex=.8,col=cols[1],line=-.2)
                
                if(eccon != "c") {
                        minSpeedT=min(which(speed$y == min(speed$y)))
                        
                        abline(v=minSpeedT, col=cols[1])
                        points(minSpeedT, min(speed$y),col=cols[1])
                        mtext(text=paste(round(min(speed$y),2),"m/s",sep=""),side=3,
                              at=minSpeedT,cex=.8,col=cols[1], line=.5)
                        mtext(text=minSpeedT,side=1,at=minSpeedT,cex=.8,col=cols[1],line=-.2)
                }
        }
        
        
        #---------------- call to getDynamics to get mass, force, power ----------------->
        
        dynamics = getDynamics(encoderConfigurationName,
                               speed$y, accel$y, massBody, massExtra, exercisePercentBodyWeight, gearedDown, anglePush, angleWeight,
                               displacement, diameter, inertiaMomentum, smoothing)
        mass = dynamics$mass
        force = dynamics$force
        power = dynamics$power
        
        #---------------- calculate landing ------------>
        
        #calculate landing, needed to plot speed, force and power
        #used to define the beginning of the ground phase
        landing = -1
        if(eccon=="ec") {
                #landing = min(which(force>=weight))
                
                if(! canJump(encoderConfigurationName) || length(which(force[eccentric] <= 0)) == 0)
                        landing = -1
                else
                        landing = max(which(force[eccentric]<= 0))
        }
        
        #---------------- speed stuff ------------>

	if(draw && showSpeed)
		axisLineRight = paintMeansArrowsAxis(speed$y, paintMode == "superpose", eccon, isPropulsive, startX, landing,
				     showAxes, axisLineRight, concentric, propulsiveEnd, eccentric, colSpeed, ltySpeed, labelsXeXc)
        
        if(draw) {
                ylimHeight = max(abs(range(accel$y)))
                ylim=c(- 1.05 * ylimHeight, 1.05 * ylimHeight)	#put 0 in the middle, and have 5% margin at each side
                if(knRanges[1] != "undefined")
                        ylim = knRanges$accely
                
                if(showAccel) {
                        par(new=T)
                        #if(highlight==FALSE)
                                plot(startX:length(accel$y),accel$y[startX:length(accel$y)],type="l",
                                     xlim=xlim,ylim=ylim,xlab="",ylab="",col=colAccel,lty=ltyAccel,lwd=1,axes=F)

			if(paintMode == "superpose")
				addRepCharsAboveLine(accel$y, colAccel, nrep, laterality)

                        #else
                        #        plot(startX:length(accel$y),accel$y[startX:length(accel$y)],type="l",
                        #             xlim=xlim,ylim=ylim,xlab="",ylab="",col="darkblue",lty=2,lwd=3,axes=F)
                }

                #show propulsive stuff if line if differentiation is relevant (propulsivePhase ends before the end of the movement)
                if(paintMode != "superpose" & isPropulsive & propulsiveEnd < length(displacement))
		{
                        abline(v=propulsiveEnd,lty=1,col=cols[2])
			mtext(text=paste(translateToPrint("Propulsive"), " ", sep=""),
			      side=1,at=propulsiveEnd,adj=1,cex=.8,col=cols[2],line=-1)
			mtext(text=paste(" ", translateToPrint("Non propulsive"), sep=""),
			      side=1,at=propulsiveEnd,adj=0,cex=.8,col=cols[2],line=-1)
			if(showAccel) {
				segments(0,-9.81,length(accel$y),-9.81,lty=3,col="magenta")
				points(propulsiveEnd, -g, col="magenta")
				text(x=length(accel$y),y=-9.81,labels=" g",cex=1,adj=c(0,0),col="magenta")
			}
                }

		#on superpose show also line but without text (to not have overlapped texts) and saying wich is the repetition
		if(paintMode == "superpose" & isPropulsive & propulsiveEnd < length(displacement))
		{
			abline(v=propulsiveEnd,lty=1,col=cols[2])
			mtext(text=nrep,side=1,at=propulsiveEnd,cex=.8,col=cols[2],line=-.2)
		}

		if(showAxes & showAccel) {
                        #axis(4, col=colAccel, lty=ltyAccel, line=axisLineRight, lwd=1, padj=-.5)
                        #axisLineRight = axisLineRight +2
			axisLineRight = paintMeansArrowsAxis(accel$y, paintMode == "superpose", eccon, isPropulsive, startX, landing,
							     showAxes, axisLineRight, concentric, propulsiveEnd, eccentric, colAccel, ltyAccel, labelsXeXc)
                }
                #mtext(text=paste("max accel:",round(max(accel$y),3)),side=3,at=which(accel$y == max(accel$y)),cex=.8,col=cols[1],line=2)
        }
        
        
        if(draw && isInertial(encoderConfigurationName) && debugOld) 
        {
                #start blank graph with ylim of position
                ylim = yrange
                if(ylim[1] == "undefined") { 
                        ylim = NULL 
                        ylim = c(min(position-min(position)), max(position-min(position)))
                }
                par(new=T)
                plot(0,0,type="n",axes=F,xlab="",ylab="", ylim=ylim)
                
                abline(h=100*dynamics$loopsAblines, col="yellow") #m -> cm
                print("dynamics$loopsAblines")
                print(dynamics$loopsAblines)
                
                print("ylim")
                print(ylim)
                
                #TODO: add here angleSpeed graph when diameter is variable (version 1.5.3)
        }
        
        #---------------- force stuff ------------>
        
        if(draw & showForce) {
                ylimHeight = max(abs(range(force)))
                ylim=c(- 1.05 * ylimHeight, 1.05 * ylimHeight)	#put 0 in the middle, and have 5% margin at each side
                if(knRanges[1] != "undefined")
                        ylim = knRanges$force
                par(new=T)
                #if(highlight==FALSE)
                        plot(startX:length(force),force[startX:length(force)],type="l",
                             xlim=xlim,ylim=ylim,xlab="",ylab="",col=colForce,lty=ltyForce,lwd=1,axes=F)
                #else
                #        plot(startX:length(force),force[startX:length(force)],type="l",
                #             xlim=xlim,ylim=ylim,xlab="",ylab="",col="darkblue",lty=2,lwd=3,axes=F)

		if(paintMode == "superpose")
			addRepCharsAboveLine(force, colForce, nrep, laterality)

     #           if(showAxes) {
                        #axis(4, col=colForce, lty=ltyForce, line=axisLineRight, lwd=1, padj=-.5)
                        #axisLineRight = axisLineRight +2
			axisLineRight = paintMeansArrowsAxis(force, paintMode == "superpose", eccon, isPropulsive, startX, landing,
							     showAxes, axisLineRight, concentric, propulsiveEnd, eccentric, colForce, ltyForce, labelsXeXc)
      #          }
                
                if(isInertial(encoderConfigurationName) && debugOld) {
                        #print("dynamics$forceDisc")
                        #print(dynamics$forceDisc)
                        par(new=T)
                        plot(dynamics$forceDisc, col="blue", xlab="", ylab="", xlim=xlim,ylim=ylim, type="p", pch=1, axes=F);
                        
                        par(new=T)
                        plot(dynamics$forceBody, col="blue", xlab="", ylab="", xlim=xlim,ylim=ylim, type="p", pch=3, axes=F);
                }
        }
        
        #mark when it's air and land
        #if it was a eccon concentric-eccentric, will be useful to calculate flight time
        #but this eccon will be not done
        #if(draw & (!superpose || (superpose & highlight)) & isJump) 
        if(draw & (paintMode != "superpose" || (paintMode == "superpose" & highlight)) & exercisePercentBodyWeight == 100) {
                weight=mass*g
                abline(h=weight,lty=3,col=cols[2]) #body force, lower than this, person in the air (in a jump)
                text(x=length(force),y=weight,labels=paste(translateToPrint("Weight"),"(N)"),cex=.8,adj=c(.5,0),col=cols[2])
                
                #define like this, because if eccentric == 0, length(eccentric) == 1
                #and if eccentric is NULL, then length(eccentric) == 0, but max(eccentric) produces error
                if(length(eccentric) == 0)
                        length_eccentric = 0
                else
                        length_eccentric = length(eccentric)
                
                if(length(isometric) == 0)
                        length_isometric = 0
                else
                        length_isometric = length(isometric)
                
                
                #takeoff = max(which(force>=weight))
                #takeoff = min(which(force[concentric]<=weight)) + length_eccentric + length_isometric
                
                takeoff = -1
                if(! canJump(encoderConfigurationName) || length(which(force[concentric] <= 0)) == 0)
                        takeoff = -1
                else {
                        #1 get force only in concentric phase
                        forceConcentric = force[concentric]
                        #print(c("forceConcentric",forceConcentric))
                        
                        #2 get takeoff using maxSpeedT but relative to concentric, not all the ecc-con
                        
                        takeoff = findTakeOff(forceConcentric, maxSpeedTInConcentric)
                        
                        #3 add eccentric and isometric
                        takeoff = takeoff + length_eccentric + length_isometric
                        print(c("takeoff",takeoff))
                        
                        abline(v=takeoff,lty=1,col=cols[2]) 
                        mtext(text=paste(translateToPrint("land")," ",sep=""),side=3,at=takeoff,cex=.8,adj=1,col=cols[2])
                        mtext(text=paste(" ", translateToPrint("air"), " ",sep=""),side=3,at=takeoff,cex=.8,adj=0,col=cols[2])
                }
                
                if(eccon=="ec" && landing != -1)
                {
                        abline(v=landing,lty=1,col=cols[2])
                        mtext(text=paste(translateToPrint("air")," ",sep=""),side=3,at=landing,cex=.8,adj=1,col=cols[2])
                        mtext(text=paste(" ",translateToPrint("land")," ",sep=""),side=3,at=landing,cex=.8,adj=0,col=cols[2])
                }
                
                print(c(is.numeric(takeoff), takeoff))
                if(is.numeric(takeoff) && takeoff != -1) {
                        mtext(text=paste(translateToPrint("jump height"),"=", 
                                         (position[concentric[length(concentric)]] - 
                                                  position[concentric[(takeoff - length_eccentric - length_isometric)]])/10,
                                         "cm",sep=" "),
                              side=3, at=( takeoff + (length_eccentric + length(concentric)) )/2,
                              cex=.8,adj=0.5,col=cols[2])
                }
        }
        
        #---------------- power stuff ------------>
        
        if(draw & showPower) {
                ylimHeight = max(abs(range(power)))
                ylim=c(- 1.05 * ylimHeight, 1.05 * ylimHeight)	#put 0 in the middle, and have 5% margin at each side
                if(knRanges[1] != "undefined")
                        ylim = knRanges$power
                par(new=T)

		lwdPower = 2
		if(paintMode == "superpose")
			lwdPower = 1

                #if(highlight==FALSE)
                        plot(startX:length(power),power[startX:length(power)],type="l",
                             xlim=xlim,ylim=ylim,xlab="",ylab="",col=colPower,lty=ltyPower,lwd=lwdPower,axes=F)
                #else
                #        plot(startX:length(power),power[startX:length(power)],type="l",
                #             xlim=xlim,ylim=ylim,xlab="",ylab="",col="darkred",lty=2,lwd=3,axes=F)

		if(paintMode == "superpose")
			addRepCharsAboveLine(power, colPower, nrep, laterality)
                
                if(isInertial(encoderConfigurationName) && debugOld) {
                        par(new=T)
                        plot(dynamics$powerDisc, col="orangered3", xlab="", ylab="", xlim=xlim, ylim=ylim, type="p", pch=1, axes=F);
                        
                        par(new=T)
                        plot(dynamics$powerBody, col="orangered3", xlab="", ylab="", xlim=xlim, ylim=ylim, type="p", pch=3, axes=F);
                }
                
		axisLineRight = paintMeansArrowsAxis(power, paintMode == "superpose", eccon, isPropulsive, startX, landing,
				     showAxes, axisLineRight, concentric, propulsiveEnd, eccentric, colPower, ltyPower, labelsXeXc)
        }
        
        #time to arrive to peak power
        powerTemp <- power[startX:length(power)]
        peakPowerT <- min(which(power == max(powerTemp)))
        if(draw & paintMode != "superpose" & showPower) {
                abline(v=peakPowerT, col=cols[3])
                points(peakPowerT, max(powerTemp),col=cols[3])
                mtext(text=paste(round(max(powerTemp),1),"W",sep=""),side=3,
                      at=peakPowerT,cex=.8,col=cols[3],line=-.2)
                mtext(text=peakPowerT,side=1,at=peakPowerT,cex=.8,col=cols[3],line=.2)
                
                #don't show min power on repetitions where power it's in abs (like inertial)
                if(eccon != "c" && min(power) < 0) {
                        minPowerT <- min(which(power == min(powerTemp)))
                        
                        abline(v=minPowerT, col=cols[3])
                        points(minPowerT, min(powerTemp),col=cols[3])
                        mtext(text=paste(round(min(powerTemp),1),"W",sep=""),side=3,
                              at=minPowerT,cex=.8,col=cols[3],line=-.2)
                        mtext(text=minPowerT,side=1,at=minPowerT,cex=.8,col=cols[3],line=.2)
                }
        }
        
        #time to arrive to peak power negative on con-ecc
        if(eccon=="ce") {
                peakPowerTneg=min(which(power == min(power)))
                if(draw & paintMode != "superpose") {
                        abline(v=peakPowerTneg, col=cols[3])
                        points(peakPowerTneg, min(power),col=cols[3])
                        mtext(text=paste(round(min(power),1),"W",sep=""),side=1,line=-1,at=peakPowerTneg,adj=.5,cex=.8,col=cols[3])
                        mtext(text=peakPowerTneg,side=1,at=peakPowerTneg,cex=.8,col=cols[3])
                }
                
                segments(peakPowerTneg,min(power),peakPowerT,min(power),lty=3,col="red")
                segments(peakPowerTneg,max(power),peakPowerT,max(power),lty=3,col="red")
                points(peakPowerTneg,(min(power) * -1),col="red")
                points(peakPowerT,(max(power) * -1),col="red")
        }
        
        #TODO: fix this to show only the cinematic values selected by user
        #legend, axes and title
        if(draw) {
		#show 0 line
		if( (paintMode != "superpose" || nrep == 1) &&
		   (showSpeed || showAccel || showForce || showPower) )
		{
			abline(h=0,lty=3,col="black")
		}

                #if(legend & showAxes) {}
                if(legend) {
                        paintVariablesLegend(showPosition, showSpeed, showAccel, showForce, showPower,
					     (triggersOnList != "" && triggersOnList != -1))
                }
                if(showLabels) {
                        mtext(paste(translateToPrint("time"),"(ms)"),side=1,adj=1,line=-1,cex=.9)
			if(showPosition)
				mtext(paste(translateToPrint("displacement"),"(mm)"),side=2,adj=1,line=-1,cex=.9)
                }

		#on sideShareX draw a box to see better graphs (to undertand better white space)
		if(paintMode == "sideShareX")
			box(col="gray", lty=2)
        }
}

paintMeansArrowsAxis <- function(vect, superpose, eccon, isPropulsive, startX, landing,
				 showAxes, axisLineRight, concentric, propulsiveEnd, eccentric, col, lty, labelsXeXc)
{
	meanC = mean(vect[min(concentric):max(concentric)])
	if(isPropulsive)
		meanC = mean(vect[min(concentric):propulsiveEnd])

	if(eccon == "c") {
		if(! superpose)
			arrows(x0=min(concentric),y0=meanC,x1=propulsiveEnd,y1=meanC,col=col,code=3)
	} else {
		if(landing == -1)
			meanE = mean(vect[startX:max(eccentric)])
		else
			meanE = mean(vect[landing:max(eccentric)])

		if(! superpose) {
			if(landing == -1)
				arrows(x0=startX,y0=meanE,x1=max(eccentric),y1=meanE,col=col,code=3)
			else
				arrows(x0=landing,y0=meanE,x1=max(eccentric),y1=meanE,col=col,code=3)

			arrows(x0=min(concentric),y0=meanC,x1=propulsiveEnd,y1=meanC,col=col,code=3)
		}
	}

	if(showAxes) {
		paintAxis(superpose, eccon, meanC, meanE, axisLineRight, col,lty, labelsXeXc)
		axisLineRight = axisLineRight +2;
	}

	return (axisLineRight)
}

#on paint different axis on the right are plotted depending on superpose and eccon
paintAxis <- function(superpose, eccon, meanC, meanE, axisLineRight, col, lty, labelsXeXc)
{
	if(eccon == "c") {
		if(! superpose) {
			axis(4, at=c(min(axTicks(4)),0,max(axTicks(4)),meanC),
			     labels=c(min(axTicks(4)),0,max(axTicks(4)),
				      round(meanC,1)),
			     col=col, lty=lty, line=axisLineRight, lwd=2, padj=-.5)
			axis(4, at=meanC,
			     labels="Xc",
			     col=col, lty=lty, line=axisLineRight, lwd=2, padj=-2)
		} else {
			axis(4, at=c(min(axTicks(4)),0,max(axTicks(4))),
			     labels=c(min(axTicks(4)),0,max(axTicks(4))),
			     col=col, lty=lty, line=axisLineRight, lwd=2, padj=-.5)
		}
	}
	else {
		if(! superpose) {
			axis(4, at=c(min(axTicks(4)),0,max(axTicks(4)),meanE,meanC),
			     labels=c(min(axTicks(4)),0,max(axTicks(4)),
				      round(meanE,1),
				      round(meanC,1)),
			     col=col, lty=lty, line=axisLineRight, lwd=1, padj=-.5)
			axis(4, at=c(meanE,meanC),
			     labels=labelsXeXc,
			     col=col, lty=lty, line=axisLineRight, lwd=0, padj=-2)
		} else {
			axis(4, at=c(min(axTicks(4)),0,max(axTicks(4))),
			     labels=c(min(axTicks(4)),0,max(axTicks(4))),
			     col=col, lty=lty, line=axisLineRight, lwd=1, padj=-.5)
		}
	}
}

paintVariablesLegend <- function(showPosition, showSpeed, showAccel, showForce, showPower, showTriggers)
{
	colPosition = "black"
	colSpeed = cols[1]
	colAccel = "magenta"
	colForce = cols[2]
	colPower = cols[3]

	ltyPosition = 1
	ltySpeed = 1
	ltyAccel = 1
	ltyForce = 1
	ltyPower = 1

	legendText=NULL
	lty=NULL
	lwd=NULL
	colors=NULL
	ncol=0

        if(showPosition) {
		legendText=c(legendText, paste(translateToPrint("Distance"),"(mm)"))
		lty=c(lty,ltyPosition)
		lwd=c(lwd,2)
		colors=c(colors,colPosition)
		ncol=ncol+1
	}
        
        if(showSpeed) {
                legendText=c(legendText, paste(translateToPrint("Speed"),"(m/s)"))
                lty=c(lty,ltySpeed)
                lwd=c(lwd,2)
                colors=c(colors,colSpeed)
                ncol=ncol+1
        }
        if(showAccel) {
                legendText=c(legendText, paste(translateToPrint("Accel."),"(m/s^2)"))
                lty=c(lty,ltyAccel)
                lwd=c(lwd,2)
                colors=c(colors,colAccel)
                ncol=ncol+1
        }
        if(showForce) {
                legendText=c(legendText, paste(translateToPrint("Force"),"(N)"))
                lty=c(lty,ltyForce)
                lwd=c(lwd,2)
                colors=c(colors,colForce)
                ncol=ncol+1
        }
        if(showPower) {
                legendText=c(legendText, paste(translateToPrint("Power"),"(W)"))
                lty=c(lty,ltyPower)
                lwd=c(lwd,2)
                colors=c(colors,colPower)
                ncol=ncol+1
        }
        if(showTriggers) {
                legendText=c(legendText, translateToPrint("Triggers"))
                lty=c(lty,2)
                lwd=c(lwd,2)
                colors=c(colors,"yellow3")
                ncol=ncol+1
        }

	if(ncol == 0)
		return()

        #plot legend on top exactly out
        #http://stackoverflow.com/a/7322792
        rng=par("usr")
        lg = legend(0,rng[2], 
                    legend=legendText, 
                    lty=lty, lwd=lwd, 
                    col=colors, 
                    cex=1, bg="white", ncol=ncol, bty="n", plot=F)
        legend(0,rng[4]+1.4*lg$rect$h, 
               legend=legendText, 
               lty=lty, lwd=lwd, 
               col=colors, 
               cex=1, bg="white", ncol=ncol, bty="n", plot=T, xpd=NA)
}

addRepCharsAboveLine <- function(variable, col, label, laterality)
{
	seqChars = seq(from=1, to=length(variable), length.out=5)
	text(x=seqChars, y=variable[seqChars], labels=paste(label, laterality) , col=col, adj=c(.5,0)) #this adjust writes letter on the top of line
}


textBox <- function(x,y,text,frontCol,bgCol,xpad=.1,ypad=1){
        
        w=strwidth(text)+xpad*strwidth(text)
        h=strheight(text)+ypad*strheight(text)
        
        rect(x-w/2,y-h/2,x+w/2,y+h/2,col=bgCol, density=60, angle=-30, border=NA)
        text(x,y,text,col=frontCol)
}

#RFelber
#https://stackoverflow.com/a/45956950/12366369
#just done a minor change
f.horlegend <- function(pos, legend, xoff = 0, yoff = 0,
  lty = 0, lwd = 1, ln.col = 1, seg.len = 0.04,
  pch = NA, pt.col = 1, pt.bg = NA, pt.cex = par("cex"), pt.lwd = lwd,
  text.cex = par("cex"), text.col = par("col"), text.font = NULL, text.vfont = NULL,
  bty = "o", bbord = "black", bbg = par("bg"), blty = par("lty"), blwd = par("lwd"), bdens = NULL, bbx.adj = 0, bby.adj = 0.75
) {

  ### get original par values and re-set them at end of function
  op <- par(no.readonly = TRUE)
  on.exit(par(op))

  ### new par with dimension [0,1]
  par(new=TRUE, xaxs="i", yaxs="i", xpd=TRUE)
  plot.new()

  ### spacing between legend elements
  d0 <- 0.01 * (1 + bbx.adj)
  d1 <- 0.01
  d2 <- 0.02
  pch.len <- 0.008
  ln.len <- seg.len/2

  n.lgd <- length(legend)

  txt.h <- strheight(legend[1], cex = text.cex, font = text.font, vfont = text.vfont) *(1 + bby.adj)
  i.pch <- seq(1, 2*n.lgd, 2)
  i.txt <- seq(2, 2*n.lgd, 2)

  ### determine x positions of legend elements
  X <- c(d0 + pch.len, pch.len + d1, rep(strwidth(legend[-n.lgd])+d2+pch.len, each=2))
  X[i.txt[-1]] <- pch.len+d1

  ### adjust symbol space if line is drawn
  if (any(lty != 0)) {
    lty <- rep(lty, n.lgd)[1:n.lgd]
    ln.sep <- rep(ln.len - pch.len, n.lgd)[lty]
    ln.sep[is.na(ln.sep)] <- 0
    X <- X + rep(ln.sep, each=2)
    lty[is.na(lty)] <- 0
  }

  X <- cumsum(X)

  ### legend box coordinates
  bstart <- 0
  bend <- X[2*n.lgd]+strwidth(legend[n.lgd])+d0

  ### legend position
  if (pos == "top" | pos == "bottom" | pos == "center") x_corr <- 0.5 - bend/2 +xoff
  if (pos == "bottomright" | pos == "right" | pos == "topright") x_corr <- 1. - bend + xoff
  if (pos == "bottomleft" | pos == "left" | pos == "topleft") x_corr <- 0 + xoff

  if (pos == "bottomleft" | pos == "bottom" | pos == "bottomright") Y <- txt.h/2 + yoff
  if (pos == "left" | pos == "center" | pos =="right") Y <- 0.5 + yoff
  #if (pos == "topleft" | pos == "top" | pos == "topright") Y <- 1  - txt.h/2 + yoff
  if (pos == "topleft" | pos == "top" | pos == "topright") Y <- 1 + txt.h/2 # changed to show the legend just above the graph. If wanted some space just do: 1 + txt.h

  Y <- rep(Y, n.lgd)
  ### draw legend box
  if (bty != "n") rect(bstart+x_corr, Y-txt.h/2, x_corr+bend, Y+txt.h/2, border=bbord, col=bbg, lty=blty, lwd=blwd, density=bdens)

  ### draw legend symbols and text
  segments(X[i.pch]+x_corr-ln.len, Y, X[i.pch]+x_corr+ln.len, Y, col = ln.col, lty = lty, lwd = lwd)
  points(X[i.pch]+x_corr, Y, pch = pch, col = pt.col, bg = pt.bg, cex = pt.cex, lwd = pt.lwd)
  text(X[i.txt]+x_corr, Y, legend, pos=4, offset=0, cex = text.cex, col = text.col, font = text.font, vfont = text.vfont)
}


paintPowerPeakPowerBars <- function(singleFile, title, paf, Eccon, ecconVector, height, n, showImpulse, showTTPP, showRange, totalTime)
{
        # 1.- prepare data ------------------------------------------------

        #if there's one or more inertial curves: show inertia instead of mass
        hasInertia <- FALSE
        
        if(findInertialCurves(paf)) {
                hasInertia <- TRUE
                load = paf[,findPosInPaf("Inertia","")]
                load = load * 10000
        } else
                load = paf[,findPosInPaf("Load","")]
        
        
        pafColors=c("tomato1","tomato4",topo.colors(10)[3])
        myNums = rownames(paf)
        height = abs(height/10)
        laterality = translateVector(as.vector(paf[,findPosInPaf("Laterality","")]))
        
        if(Eccon=="ecS" || Eccon=="ceS") {
                if(singleFile) {
                        myEc=c("c","e")
                        if(Eccon=="ceS")
                                myEc=c("e","c")
                        myNums = as.numeric(rownames(paf))
                        myNums = paste(trunc((myNums+1)/2),myEc[((myNums%%2)+1)],sep="")
                }
        }
        
        powerData=rbind(paf[,findPosInPaf("Power","mean")], paf[,findPosInPaf("Power","max")])
        
        #when eccon != c show always ABS power
        #peakPower is always ABS
        if(Eccon == "c") {
                powerName = translateToPrint("Average Power")
                peakPowerName = translateToPrint("Peak Power")
        }
        else {
                powerName = translateToPrint("Average Power")
                peakPowerName = translateToPrint("Peak Power")
        }
        
        #print("powerData")
        #print(powerData)
        
        #put lowerY on power, but definetively, leave it at 0
        #lowerY=min(powerData)-100
        #if(lowerY < 0)
        #	lowerY = 0
        lowerY = 0

        marginRight = 9
        if(! showImpulse)
                marginRight = marginRight -3
        if(! showTTPP)
                marginRight = marginRight -3
        if(! showRange)
                marginRight = marginRight -3
        
        par(mar=c(2.5, 4, 5, marginRight))
        
        bpAngle = 0
        bpDensity = NULL
	bpAngle = createAngleVector(ecconVector)
	bpDensity = createDensityVector(ecconVector)
        #print(c("bpAngle=",bpAngle))
        #print(c("bpDensity=",bpDensity))

	# 2.- plot main barplot ------------------------------------------------

        bp <- barplot(powerData,beside=T,col=pafColors[1:2],width=c(1.4,.6),
                      names.arg=paste(myNums," ",laterality,"\n",load,sep=""),xlim=c(1,n*3+.5),cex.name=0.8,
                      xlab="",ylab=paste(translateToPrint("Power"),"(W)"), 
                      ylim=c(lowerY,max(powerData)), xpd=FALSE,	#ylim, xpd = F,  makes barplot starts high (compare between them)
                      angle=bpAngle, density=bpDensity
        ) 
        title(main=title,line=-2,outer=T)
        box()
        
        loadWord = "Mass"
        if(hasInertia)
                loadWord = "Inertia M."
        
        mtext(paste(translateToPrint("Repetition")," \n",translateToPrint(loadWord)," ",sep=""),side=1,at=1,adj=1,line=1,cex=.9)
        #mtext(translateToPrint("Laterality"),side=1,adj=1,line=0,cex=.9)
        
        axisLineRight = 0

	# 3.- plot other variables and their axis ----------------------------------------
        
	if(showImpulse) {
		#Impulse
		#impulse = avg force of all the phase * time of the phase in seconds
		print("totalTime (s):")
		print(totalTime / 1000.0)
		impulse <- paf[,findPosInPaf("Force","")] * ( totalTime / 1000.0 )
		par(new=T)
		plot(bp[2,],impulse,type="b",lwd=2,xlim=c(1,n*3+.5),ylim=c(0,max(impulse)),axes=F,xlab="",ylab="",col="yellow3")
		print("impulse") #terminal
		print(impulse)

		axis(4, col="yellow3", line=axisLineRight, padj=-.5)
                mtext(paste(translateToPrint("Impulse"),"(N*s)"), side=4, line=(axisLineRight-1))
                axisLineRight = axisLineRight +3
	}

	#Work
	#aqui cal la força instantania, per tant caldria modificar el kinematicsF
	#work <- paf[,findPosInPaf("Force","")] * height
	#par(new=T)
        #plot(bp[2,],height,type="p",lwd=2,xlim=c(1,n*3+.5),ylim=c(0,max(height)),axes=F,xlab="",ylab="",col="gray")
        #time to peak power
        if(showTTPP) {
                par(new=T, xpd=T)
                #on ecS, concentric has high value of time to peak power and eccentric has it very low. Don't draw lines
                if(Eccon=="ecS" || Eccon=="ceS")
                        plot(bp[2,],paf[,findPosInPaf("Power","time")],type="p",lwd=2,
                             xlim=c(1,n*3+.5),ylim=c(0,max(paf[,findPosInPaf("Power","time")])),
                             axes=F,xlab="",ylab="",col="blue", bg="lightblue",cex=1.5,pch=21)
                else
                        plot(bp[2,],paf[,findPosInPaf("Power","time")],type="b",lwd=2,
                             xlim=c(1,n*3+.5),ylim=c(0,max(paf[,findPosInPaf("Power","time")])),
                             axes=F,xlab="",ylab="",col=pafColors[3])
                
                axis(4, col=pafColors[3], line=axisLineRight,padj=-.5)
                mtext(paste(translateToPrint("Time to Peak Power"),"(ms)"), side=4, line=(axisLineRight-1))
                axisLineRight = axisLineRight +3
        }
        
        #range
        if(showRange) {	
                par(new=T)
                plot(bp[2,],height,type="p",lwd=2,xlim=c(1,n*3+.5),ylim=c(0,max(height)),axes=F,xlab="",ylab="",col="green")
                
                #abline(h=max(height),lty=2, col="green")
                #abline(h=min(height),lty=2, col="green")
                #text(max(bp[,2]),max(height),max(height),adj=c(0,.5),cex=0.8)
                axis(4, col="green", line=axisLineRight, padj=-.5)
                mtext(paste(translateToPrint("Range"),"(cm)"), side=4, line=(axisLineRight-1))
                axisLineRight = axisLineRight +3
                
                for(i in unique(load)) { 
                        myLabel = round(mean(height[which(load == i)]),1)
                        
                        #text(x=mean(bp[2,which(load == i)]),
                        #     y=mean(height[which(load == i)]),
                        #     labels=myLabel,adj=c(.5,0),cex=.9,col="darkgreen")
                        textBox(mean(bp[2,which(load == i)]),
                                mean(height[which(load == i)]),
                                myLabel, "darkgreen", "white", ypad=1)
                        
                        segments(
                                bp[2,min(which(load == i))],mean(height[which(load == i)]),
                                bp[2,max(which(load == i))],mean(height[which(load == i)]),
                                lty=1,col="green")
                }
        }


        # 4.- legend ------------------------------------------------

        legendText = c(powerName, peakPowerName)
        lty=c(0,0)
        lwd=c(1,1)
        pch=c(15,15)
        graphColors=c(pafColors[1],pafColors[2])

	#ncol=2
        
	if(showImpulse) {
                legendText = c(legendText, translateToPrint("Impulse"))
                lty=c(lty,1)
                lwd=c(lwd,2)
                pch=c(pch,NA)
                graphColors=c(graphColors,"yellow3")
		#ncol = ncol +1
	}
        if(showRange) {
                legendText = c(legendText, translateToPrint("Range"))
                lty=c(lty,1)
                lwd=c(lwd,2)
                pch=c(pch,NA)
                graphColors=c(graphColors,"green")
		#ncol = ncol +1
        }
        if(showTTPP) {
                legendText = c(legendText, translateToPrint("Time to Peak Power"))
                lty=c(lty,1)
                lwd=c(lwd,3)
                pch=c(pch,NA)
                graphColors=c(graphColors,pafColors[3])
		#ncol = ncol +1
        }

	f.horlegend("topleft", legendText, pt.col=graphColors, ln.col=graphColors, lty=lty, lwd=lwd, pch=pch, bty="n", seg.len=0.03)
}

#see paf for more info
findPosInPaf <- function(var, option) {
        pos = 0
        if(var == "Speed")
                pos = 1
        else if(var == "Power")
                pos = 4
        else if(var == "Force")
                pos = 8
        else if(var == "Load") #MassDisplaced
                pos = 12
        else if(var == "MassBody")
                pos = 13
        else if(var == "MassExtra")
                pos = 14
        else if(var == "workJ")
                pos = 15
        else if(var == "impulse")
                pos = 16
        else if(var == "Laterality")
                pos = 17
        else if(var == "Inertia")
                pos = 18
        else if(var == "Diameter") #Inertial
                pos = 19
        else if(var == "EquivalentMass") #Inertial
                pos = 20
        
        if( ( var == "Speed" || var == "Power" || var == "Force") & option == "max")
                pos=pos+1
        if( ( var == "Speed" || var == "Power" || var == "Force") & option == "time")
                pos=pos+2
        
        return(pos)
}

#see if there's any inertial curve
#can be done by encoderConfigurationName
#but is faster just see if inertiaMoment is != than -1.
#But inertiaMoment is divided by 10000. So use the '> 0' to know when there's inertiaMoment
#
#returns TRUE if there is one or more inertial curve
findInertialCurves <- function(paf) {
        write("findInertialCurves",stderr())
        
        im = paf[,findPosInPaf("Inertia", "")]
        #write(im,stderr())
        
        if(length(im) < 1)
                return (FALSE)
        
        for(i in 1:length(im)) {
                #write(c("im: ", im[i]),stderr())
                if(im[i] > 0) {
                        return (TRUE)
                }
        }
        
        return (FALSE)
}

addUnitsAndTranslate <- function (var) {
        if(var == "Speed")
                return (paste(translateToPrint("Speed"),"(m/s)"))
        else if(var == "Power")
                return (paste(translateToPrint("Power"),"(W)"))
        else if(var == "Load") #or Mass
                return (paste(translateToPrint("Mass"),"(Kg)"))
        else if(var == "Inertia")
                return (paste(translateToPrint("Inertia M."),"(Kg*cm^2)"))
        else if(var == "Force")
                return (paste(translateToPrint("Force"),"(N)"))
        
        return(var)
}

#if num is >= 0, plot '+'.
#else plot ''. (because the number will be displayed as negative)
#this is to avoid having a '+ -'
plotSign <- function (num) {
        if(num >= 0)
                return('+')
        else
                return('')
}

getModelPValueWithStars <- function(model) {
        #p.value = round(getModelPValue(model),6)
        p.value = getModelPValue(model)
        
        #don't plot stars if p.value is nan because there's too few data
        if(is.nan(p.value))
                return (p.value)
        
        stars = ""
        if(p.value <= 0.0001)
                stars = "***"
        else if(p.value <= 0.001)
                stars = "**"
        else if(p.value <= 0.01)
                stars = "*"
        else if(p.value <= 0.05)
                stars = "."
        return(paste(round.scientific(p.value), " ", stars, sep=""))
}
#http://r.789695.n4.nabble.com/extract-the-p-value-tp3933973p3934011.html
getModelPValue <- function(model) {
        stopifnot(inherits(model, "lm"))
        s <- summary.lm(model)
        pf(s$fstatistic[1L], s$fstatistic[2L], s$fstatistic[3L], lower.tail = FALSE)
}

#R returns zero on rounding if the exponent is bigger than the decimals of rounding. Eg:
#> round(0.0002,3)
#[1] 0
#> round(0.0002,4)
#[1] 2e-04
#> round(-0.0002,3)
#[1] 0
#> round(-0.0002,4)
#[1] -2e-04
round.scientific <- function(x) {
        if(x == 0)
                return(0)
        
        negative = FALSE
        #the floor(log(10(x)) returns NaN if it's negative
        if(x < 0) {
                negative = TRUE
                x = x * -1
        }
        
        #http://r.789695.n4.nabble.com/Built-in-function-for-extracting-mantissa-and-exponent-of-a-numeric-td4670116.html
        e <- floor(log10(x))
        m <- x/10^e
        
        if(negative)
                m = m * -1
        
        dec = 2
        if(e == 0)
                return(round(m,dec))
        else
                return(paste(round(m,dec),"e",e,sep=""))
}

#http://stackoverflow.com/a/6234664
#see if two labels overlap
stroverlap <- function(x1,y1,s1, x2,y2,s2) {
        sh1 <- strheight(s1)
        sw1 <- strwidth(s1)
        sh2 <- strheight(s2)
        sw2 <- strwidth(s2)
        
        overlap <- FALSE
        if (x1<x2) 
                overlap <- x1 + sw1 > x2
        else
                overlap <- x2 + sw2 > x1
        
        if (y1<y2)
                overlap <- overlap && (y1 +sh1>y2)
        else
                overlap <- overlap && (y2+sh2>y1)
        
        return(overlap)
}
#check all labels to see if newPoint overlaps one of them
stroverlapArray <- function(newPoint, points) {
        overlap = FALSE
        
        if(length(points$x) == 1)	#if there's only one row
                return (stroverlap(
                        as.numeric(newPoint[1]), as.numeric(newPoint[2]), newPoint[3],
                        as.numeric(points$x), as.numeric(points$y), points$curveNum ))
        
        #as.numeric is needed because ec-con uses "1e" in third element, and then three elements are strings
        
        for(i in 1:length(points$x)) {	#for every row
                overlap = stroverlap(
                        as.numeric(newPoint[1]), as.numeric(newPoint[2]), newPoint[3],
                        as.numeric(points$x[i]), as.numeric(points$y[i]), points$curveNum[i])
                if(overlap)
                        return (TRUE)
        }
        return (FALSE)
}

#fitLine <- function(mode, x, y, col, lwd, lty) {
#	if(mode == "LINE") {
#		fit = lm(y ~ x)
#		abline(fit, col=col, lwd=lwd, lty=lty)
#	}
#	if(mode == "CURVE") {
#		fit = lm(y ~ x + I(x^2))
#
#		coef.a <- fit$coefficient[3]
#		coef.b <- fit$coefficient[2]
#		coef.c <- fit$coefficient[1]
#
#		x1 <- seq(min(x),max(x), (max(x) - min(x))/1000)
#		y1 <- coef.a *x1^2 + coef.b * x1 + coef.c
#		lines(x1, y1, col=col, lwd=lwd, lty=lty)
#
#		return (fit)
#	}
#}

fitLine <- function(x, y, col, lwd, lty) {
        write("drawing fitline", stderr())
        write(x, stderr())
        write(y, stderr())

        fit = lm(y ~ x)
        write(fit$coefficiens, stderr())
        abline(fit, col=col, lwd=lwd, lty=lty)
}

fitCurveCalc <- function(x, y) {
        write("fitCurveCalc 0",stderr())
        write(c("x: ",x),stderr())
        write(c("y: ",y),stderr())
        
	#convert x to numeric
	x=as.numeric(x)
	fit = lm(y ~ x + I(x^2))
        
        write("fitCurveCalc 1",stderr())
        coef.a <- fit$coefficient[3]
        coef.b <- fit$coefficient[2]
        coef.c <- fit$coefficient[1]
        
        plotSize = par("usr")
        
        #if there's no plot defined plotSize is [0, 1, 0, 1]
        if (plotSize[1] == 0 && plotSize[2] == 1 && plotSize[3] == 0 && plotSize[4] == 1){
                x1 <- seq(min(x),max(x), (max(x) - min(x))/1000)
        } else {
                x1 <- seq(plotSize[1], plotSize[2], (plotSize[2] - plotSize[1]) / 1000)
        }
        
        y1 <- coef.a *x1^2 + coef.b * x1 + coef.c
        
        write("fitCurveCalc done!",stderr())
        return(list(fit, x1, y1))
}
fitCurvePlot <- function(x1, y1, col, lwd, lty) {
        lines(x1, y1, col=col, lwd=lwd, lty=lty)
}

paintCrossVariablesLaterality <- function(x, y, laterality, colBalls, varX = "Load", varY = "Power") 
{
        write("paintCrossVariablesLaterality 0",stderr())
        points(x[laterality=="L"], y[laterality=="L"], type="p", cex=1, col=colBalls, pch=3) # font=5, pch=220) #172, 220 don't looks good
        points(x[laterality=="R"], y[laterality=="R"], type="p", cex=1, col=colBalls, pch=4) # font=5, pch=222) #174, 222 don't looks good
        
        if(length(unique(laterality)) > 1) 
        {
                if (varX == "Load" & varY == "Power")
                {
                        #if(length(laterality[laterality == "R"]) >= 3 && length(unique(x[laterality == "R"])) > 1)
                        if(length(unique(x[laterality == "R"])) > 2){
                                fit = fitCurveCalc(x[laterality=="R"],y[laterality=="R"])
                                fitCurvePlot(fit[[2]], fit[[3]], "black", 1, 2)
                        }
                        #if(length(laterality[laterality == "L"]) >= 3 && length(unique(x[laterality == "L"])) > 1)
                        if(length(unique(x[laterality == "L"])) > 2){
                                fit = fitCurveCalc(x[laterality=="L"],y[laterality=="L"])
                                fitCurvePlot(fit[[2]], fit[[3]], "black", 1, 3)
                        }
                        #if(length(laterality[laterality == "RL"]) >= 3 && length(unique(x[laterality == "RL"])) > 1)
                        if(length(unique(x[laterality == "RL"])) > 2){
                                fit = fitCurveCalc(x[laterality=="RL"],y[laterality=="RL"])
                                fitCurvePlot(fit[[2]], fit[[3]], "black", 1, 4)
                        }
                } else
                {
                        if(length(unique(x[laterality == "R"])) >= 2)
                                fitLine(x[laterality=="R"],y[laterality=="R"], "black", 1, 2)
                        if(length(unique(x[laterality == "L"])) >= 2)
                                fitLine(x[laterality=="L"],y[laterality=="L"], "black", 1, 3)
                        if(length(unique(x[laterality == "RL"])) >= 2)
                                fitLine(x[laterality=="RL"],y[laterality=="RL"], "black", 1, 4)
                }
        }
        write("paintCrossVariablesLaterality done!",stderr())
}

#option: mean or max
paintCrossVariables <- function (paf, varX, varY, option, 
                                 dateAsX, dateTime,
                                 isAlone, title, singleFile, Eccon, ecconVector, seriesName,
				 inertialGraphX,
                                 do1RM, do1RMMethod, outputData1) 
{
        hasInertia <- FALSE
        if(findInertialCurves(paf))
                hasInertia <- TRUE

        #if there's one or more inertial curves: show inertia instead of mass
        if(varX == "Load" && hasInertia)
	{
		if(inertialGraphX == "EQUIVALENT_MASS")
			varX = "EquivalentMass"
		else if(inertialGraphX == "INERTIA_MOMENT")
			varX = "Inertia"
		else
			varX = "Diameter"
	}

        if(varX == "Load")
                x = (paf[,findPosInPaf("MassExtra", option)])
        else
                x = (paf[,findPosInPaf(varX, option)])
        
        y = (paf[,findPosInPaf(varY, option)])
        
        if(varX == "Inertia")
                x = x * 10000
        
        colBalls = NULL
        bgBalls = NULL
        doBox = TRUE
        
        isPowerLoad = FALSE
        if( (varX == "Load" || varX == "Inertia") && varY == "Power" )
                isPowerLoad = TRUE
        if(! dateAsX)
        {
                varXut = addUnitsAndTranslate(varX)
        } else
                varXut = "Date"
        
        varYut = addUnitsAndTranslate(varY)
        
        #right now we can select if use equivalent mass, inertia moment or diameter, so this block gets commented:
	#if diameter or gearedDown changes in this data, the use resistant momentum
        #if(length(unique(diameter)) > 1 || length(unique(gearedDown)) > 1) {
        #        x = x * gearedDown / diameter
        #        varX = "Resistant torque"
        #        varXut = "Resistant torque (Kg*cm)"
        #}
	#if in the future we use this code again, use cols from paf
        
        if(dateAsX) {
                xCopy <- x
                x <- as.Date(dateTime)
                seriesName <- xCopy
        }
        cexNums = 1
        adjHor = 0
        #if only one series
        if(length(unique(seriesName)) == 1) {
                cexBalls = 1.8
                colBalls="blue"
                bgBalls="lightBlue"
                if(isAlone == "RIGHT") {
                        colBalls="red"
                        bgBalls="pink"
                }
                
                pchVector = createPchVector(ecconVector)
                
                laterality = as.vector(paf[,findPosInPaf("Laterality","")])
                #bgBallsVector = rep(bgBalls, length(x))	
                #bgBallsVector[laterality=="L"] <- "red"
                #bgBallsVector[laterality=="R"] <- "blue"
                #plot(x,y, xlab=varXut, ylab="", pch=pchVector, col=colBalls,bg=bgBallsVector,cex=cexBalls,axes=F)
                
		numsPrint.done = FALSE
                
                if(do1RM != FALSE & do1RM != "0") {	
                        speed1RM = as.numeric(do1RM)
                        
                        #lineal stuff
                        fit = lm(y ~ x) #declare
                        if(do1RMMethod == "NONWEIGHTED")  {
                                #without weights
                                fit = lm(y ~ x)
                        } else if(do1RMMethod == "WEIGHTED")  {
                                #weights x
                                fit = lm(y ~ x, weights=x/max(x)) 
                        } else if(do1RMMethod == "WEIGHTED2")  {
                                #weights x^2
                                fit = lm(y ~ x, weights=x^2/max(x^2)) 
                        } else if(do1RMMethod == "WEIGHTED3")  {
                                #weights x^3 (as higher then more important are the right values) 
                                fit = lm(y ~ x, weights=x^3/max(x^3)) 
                        }
                        
                        c.intercept = coef(fit)[[1]]
                        c.x = coef(fit)[[2]]
                        
                        if(is.na(c.x)) {
                                plot(0,0,type="n",axes=F,xlab="",ylab="")
                                text(x=0,y=0,translateToPrint("Not enough data."),cex=1.5)
                                dev.off()
                                write("1RM;-1", SpecialData)
                                write("", outputData1)
                                quit()
                        }
                        
                        load1RM = ( speed1RM - c.intercept ) / c.x
                        
                        maxX=max(x)
                        if(load1RM > maxX)
                                maxX=load1RM
                        plot(x,y, xlim=c(min(x),maxX), ylim=c(0, max(y)), xlab=varXut, ylab="", pch=21,col=colBalls,bg=bgBalls,cex=cexBalls,axes=F)
                        abline(fit,col="red")
                        abline(h=speed1RM,col="gray",lty=2)
                        abline(v=load1RM,col="gray",lty=2)
                        mtext("1RM", at=load1RM, side=1, line=2,col="red")
                        mtext(round(load1RM,2), at=load1RM, side=1, line=3,col="red")
                        mtext("1RM", at=speed1RM, side=2, line=2,col="red")
                        mtext(speed1RM, at=speed1RM, side=2, line=3,col="red")
                        points(load1RM,speed1RM,cex=2,col="red")
                        
                        write(paste("1RM;",round(load1RM,2),sep=""), SpecialData)
                        
                        #quadratic stuff
                        #fit2 = lm(y ~ I(x^2) + x)
                        #fit2line = predict(fit2, data.frame(x = 10:100))
                        #lines(10:100 ,fit2line, col="red") #puts line on plot
                }
                else {
                        #if less than 3 different X then cannot calculate fittings. Just plot here to not crash on stroverlap (after)
                        if( (varY == "Power" && length(unique(x)) < 3) ||
                           length(unique(x)) < 2 ) {
                                plot(x,y, xlab=varXut, ylab="", pch=pchVector, col=colBalls,bg=bgBalls,cex=cexBalls,axes=F)
                                
                                paintCrossVariablesLaterality(x, y, laterality, colBalls, varX = varX, varY = varY)
                        } else {
                                if(varY == "Power" && ! dateAsX) {
                                        #1) fitCurveCalc is calculated first to know plot ylim (curve has to be inside the plot)
                                        temp.list <- fitCurveCalc(x,y)
                                        fit <- temp.list[[1]]
                                        x1 <- temp.list[[2]]
                                        y1 <- temp.list[[3]]
                                        
                                        coef.a <- fit$coefficient[3]
                                        coef.b <- fit$coefficient[2]
                                        coef.c <- fit$coefficient[1]
                                        
                                        #2) plot graph
                                        plot(x,y, ylim=c(min(c(y,y1)), max(c(y,y1+8))), #+8 to allow to be seen the pmax circle
                                             xlab=varXut, ylab="", pch=pchVector, col=colBalls,bg=bgBalls,cex=cexBalls,axes=F)
                                        
                                        paintCrossVariablesLaterality(x, y, laterality, colBalls, varX = varX, varY = varY)
                                        
                                        #3) add curve
                                        fitCurvePlot(x1, y1, "black", 1, 1)
                                        
                                        #start plot the function expression, R^2 and p
                                        varXplot = varX
                                        if(varXplot == "Load")
                                                varXplot = "Mass"
                                        
                                        #for Speed,Power graph
                                        functionAt = max(x)
                                        functionAdj = 1
                                        if(isAlone == "LEFT") {
                                                functionAt = min(x)
                                                functionAdj = 0
                                        }
                                        
                                        mtext(paste(
                                                varYut, " = ", 
                                                round.scientific(coef.a), " · ", varXplot, "² ", plotSign(coef.b), " ",  
                                                round.scientific(coef.b), " · ", varXplot, " ", plotSign(coef.c), " ", 
                                                round.scientific(coef.c), sep=""), side=3, line=1, at=functionAt, adj=functionAdj, cex = .9)
                                        mtext(paste(
                                                "R^2 = ", round(summary(fit)$r.squared,4),
                                                "; R^2 (adjusted) = ", round(summary(fit)$adj.r.squared,4),
                                                "; p = ", getModelPValueWithStars(fit)
                                                , sep=""), side =3, line=0, at=functionAt, adj=functionAdj, cex=.9)
                                        #end of plot the function expression, R^2 and p
                                        
                                        if(isPowerLoad) {
                                                #xmax <-  -b / 2a
                                                xmax <- - coef.b / (2 * coef.a)
                                                
                                                #pmax <- ax^2 +bx +c
                                                pmax <- xmax^2 * coef.a + xmax * coef.b + coef.c
                                                
                                                abline(v=xmax,lty=3)
                                                points(xmax, pmax, pch=1, cex=3, col="red")
                                                
                                                massUnit <- "Kg"
                                                if(hasInertia)
                                                        massUnit <- "Kg·cm²"
                                                
                                                #this check is to not have title overlaps on 'speed,power / load' graph
                                                if(title != "")
                                                        title = paste(title, " (pmax = ", round(pmax,1), " W with ", 
                                                                      round(xmax,1), " ", massUnit, sep="")
						#do not show pmax because it can go out of graph, just show in the legend
						#text(xmax, pmax, label = paste("Pmax=", round(pmax,1), "W*"), pos = 3, col = "red")
                                                if (coef.a < 0){
                                                        legend(x = par("usr")[2], y = par("usr")[4]-(par("usr")[4] - par("usr")[3])*0.1, 
                                                               legend = c(paste("Pmax = ", round(pmax,1), "W", sep=""),
                                                                          paste("Load: ", round(xmax,1), massUnit, sep = "")),
                                                               xjust = 1, text.col = c("red", "black"), cex = 1.3)
                                                } else {
                                                        legend(x = par("usr")[2], y = par("usr")[4]-(par("usr")[4] - par("usr")[3])*0.1, 
                                                               legend = c("Data not fitted to expected parabole",
                                                                          "Middle points too low"),
                                                               xjust = 1, text.col = c("red", "red"), cex = 1.3)
                                                }
                                                mtext(text = paste("*", translateToPrint("Mean power parabole using the Power-Load data"), sep=""), side = 4, line = 4)
                                        }
                                }
                                else {
					#is "Force,Power" but using Force to have less problems, or
                                        if(varX == "Speed" && (varY == "Force" || varY == "Load"))
                                        {
                                                fit = lm(y ~ x)
                                                V0 = -coef(fit)[[1]] / coef(fit)[[2]]
                                                F0 = coef(fit)[[1]] #or L0
                                                plot(x,y,
                                                     xlim=c(0, max(x,V0)), ylim=c(0, max(y,F0)),
                                                     xlab=varXut, ylab="", pch=pchVector, col=colBalls,bg=bgBalls,cex=cexBalls,axes=F)
                                                
                                                #Calculing optimal load
                                                mass = unlist(paf$mass)
                                                speed = unlist(paf$meanSpeed)
                                                speedFit = lm(speed ~ mass)
                                                optimLoad = -coef(speedFit)[[1]] / coef(speedFit)[[2]] / 2 - paf$massBody[1]
                                                
                                                
                                                #don't plot box because it's not nice with 0,0 axis
                                                doBox = FALSE
                                                
                                                #force x and y axis to start at 0
                                                axis(1,pos=0)
                                                axis(2,pos=0)
                                                #draw ablines to arrive to the fitLine values
                                                abline(h=0)
                                                abline(v=0)
                                                #draw points and mtext
                                                points(V0,0,col="darkgreen")
                                                points(0,F0,col="blue")
                                                text(x=V0, y=0, paste("V0 = ", round(V0,2), " m/s", sep=""), col="darkgreen", pos = 3)
						if(varY == "Force")
						{
	                                                text(x=0, y=F0, paste("F0 = ", round(F0,2), " N", sep=""), col="blue", pos = 4)
						} else { #(varY == "Load")
	                                                text(x=0, y=F0, paste("L0 = ", round(F0,2), " Kg", sep=""), col="blue", pos = 4)
	                                                text(x=V0*.25, y=F0*.25, paste("M0 (F0*V0/2) =\n", round(F0*V0/2,2), " Kg*m/2", sep=""), col="black")
						}
                                        } else {
                                                plot(x,y, xlab=varXut, ylab="", pch=pchVector, col=colBalls,bg=bgBalls,cex=cexBalls,axes=F)
                                        }
                                        
                                        paintCrossVariablesLaterality(x, y, laterality, colBalls, varX = varX, varY = varY)
                                        
                                        fitLine(x,y, "black", 1, 1)
                                        
                                        #Display formula of the Force / Load fitted line
                                        if((varX == "Load" || varX == "Inertia") && varY == "Force")
                                        {
                                                fit = lm(y ~ x)
                                                mtext(side = 3, line = 1,
                                                      text = paste("Force = ", round(fit$coefficients[2], digits = 3),
                                                                   "·Load +  " , round(fit$coefficients[1], digits = 3), sep = ""),
                                                      at = max(x), adj = 1, cex = 1.2)
                                                mtext(paste(
                                                        "R^2 = ", round(summary(fit)$r.squared,4),
                                                        "; R^2 (adjusted) = ", round(summary(fit)$adj.r.squared,4),
                                                        "; p = ", getModelPValueWithStars(fit)
                                                        , sep=""), side =3, line=0, at = max(x), adj = 1, cex=.9)
                                        }
                                        
                                        #Draw the power parabole
                                        if(varX == "Speed" && varY == "Force") #is "Force,Power" but using Force to have less problems
                                        {
						paintCrossVariablesNumsPrint(singleFile, Eccon, x, y, adjHor, cexNums, laterality)
						numsPrint.done = TRUE

                                                xpower = seq(0, V0, by = V0 / 100)
                                                #ypower = 4 * xpower * (coef(fit)[2]*xpower + coef(fit)[1]) / V0
                                                ypower = xpower * (coef(fit)[2]*xpower + coef(fit)[1])
                                                #lines(xpower,ypower)
                                                par(new=T)
                                                plot(xpower,ypower, type="l", axes=F, col = "red", xlab="", ylab="")
                                                axis(4)
                                                mtext(side = 4, line = 3, "Power(W)", col = "red")
                                                mtext(side = 4, line = 4, paste("*", translateToPrint("Maximum mean power using the F-V profile"), sep=""))
                                                points(x = V0 / 2, y = V0 * F0 / 4, col = "red")
                                                #text(x = V0 / 2, y = V0 * F0 / 4, labels = paste("Pmax = ",round(F0 * V0 / 4, digits = 2),"W*", sep =""), pos = 3, col = "red")
                                                mtext(side = 3, at = V0 / 2, paste("Pmax = ",round(F0 * V0 / 4, digits = 2),"W*", sep =""), col = "red")
                                                legend(x = V0*1.04, y = V0 * F0 * 0.2, xjust = 1, yjust = 0.1,
                                                       text.col = c("Blue", "darkgreen", "red", "black"), cex = 1.3,
                                                       legend = c(paste("F0 = ", round(F0, digits = 0), " N", sep = ""),
                                                                  paste("V0 = ", round(V0, digits = 2), " m/s", sep = ""),
                                                                  paste("Pmax = ", round(F0*V0/4, digits = 0), " W", sep = "")
                                                                  #,paste("Load = ", round(optimLoad, digits=1), "Kg", sep = "")),
                                                                  ))
                                        }
					else if(varX == "Speed" && varY == "Load") #is "Load,Power" but using Force to have less problems
                                        {
                                                legend(x = V0*1.04, y = F0, xjust = 1, yjust = 1,
                                                       text.col = c("Blue", "darkgreen", "black"), cex = 1.3,
                                                       legend = c(paste("L0 = ", round(F0, digits = 0), " Kg", sep = ""),
                                                                  paste("V0 = ", round(V0, digits = 2), " m/s", sep = ""),
                                                                  paste("M0 = ", round(F0*V0/2, digits = 2), " Kg*m/s", sep = "")
                                                                  #,paste("Load = ", round(optimLoad, digits=1), "Kg", sep = "")),
                                                                  ))
					}
                                }
                        }
                }
                
		if(! numsPrint.done)
			paintCrossVariablesNumsPrint(singleFile, Eccon, x, y, adjHor, cexNums, laterality)
                
                #don't write title two times on 'speed,power / load'
                if(isAlone == "ALONE" || isAlone =="RIGHT")
                        title(title, cex.main=1, font.main=2, line=3)
                
                #don't write legend on 'speed,power / load' because it doesn't fits with the formulas and regressions
                if(isAlone == "ALONE") {
                        #show legend
                        
                        #disabled translation because some OSX cannot show accents on this expression,
                        #and some parallels on this OSX hang when try to show this
                        #legendText = c(translateToPrint("concentric"),
                        #	       translateToPrint("eccentric"),
                        #	       paste(translateToPrint("eccentric"),translateToPrint("concentric"),sep="-"),
                        #	       translateToPrint("L"),
                        #	       translateToPrint("R")
                        #	       )
			if(do1RM == FALSE) #do not show this legend on 1RM
			{
				legendText = c(translateToPrint("concentric"), translateToPrint("eccentric"), paste(translateToPrint("eccentric"),translateToPrint("concentric"),sep="-"), "L", "R")
				rng=par("usr")
				lg = legend(rng[1],rng[4],
						legend=legendText, pch=c(24,25,21,3,4), col="black", pt.bg="white",
						cex=1, ncol=2, bty="n",
						plot=F)
				legend(rng[1],rng[4]+1*lg$rect$h,
						legend=legendText, pch=c(24,25,21,3,4), col="black",  pt.bg="white",
						cex=1, bg=bgBalls, ncol=2, bty="n",
						plot=T, xpd=NA)
			}
                }
                
        } else { #more than one series
                #colBalls = "black"
                uniqueColors=rainbow(length(unique(seriesName)))
                
                # #in x axis move a little every series to right in order to compare
                # seqX = seq(0,length(unique(seriesName))-1,by=1)-(length(unique(seriesName))-1)/2
                
                maxy <- max(y)
                miny <- min(y)
                maxx <- max(x)
                minx <- min(x)
                for(i in 1:length(unique(seriesName))) {
                        thisSerie = which(seriesName == unique(seriesName)[i])
                        
                        write(paste("Serie number:", i), stderr())
                        write(thisSerie, stderr())
                        
                        colBalls[thisSerie] = uniqueColors[i]
                        
                        # if(! dateAsX) {
                        #         #in x axis move a little every series to right in order to compare
                        #         x[thisSerie] = x[thisSerie] + (seqX[i]/5)
                        # }
                        
                        #find min/max Y on power
                        if(varY == "Power" && length(unique(x[thisSerie])) >= 3 && ! dateAsX) {
                                temp.list <- fitCurveCalc(x[thisSerie],y[thisSerie])
                                y1 <- temp.list[[3]]
                                if(max(y1) > maxy)
                                        maxy <- max(y1)
                                if(min(y1) < miny)
                                        miny <- min(y1)
                        } else if(varX == "Speed" && varY == "Force") #is "Force,Power" but using Force to have less problems
			{
                                if(length(y[thisSerie]) > 1) #At least 2 points needed
                                {
                                        fit = lm(y[thisSerie] ~ x[thisSerie])
                                        V0 = -coef(fit)[[1]] / coef(fit)[[2]]
                                        F0 = coef(fit)[[1]]
                                        if (F0 > maxy)
                                                maxy = F0
                                        if (V0 > maxx)
                                                maxx = V0
                                        miny = 0
                                        minx = 0
                                }
                        }
                }
                ylim <- c(miny, maxy)
                xlim <- c(minx, maxx)
                
                plot(x,y, xlim = xlim, ylim=ylim, xlab=varXut, ylab="", type="n", axes = FALSE, doBox = FALSE)
                points(x,y, pch=19, col=colBalls, cex=1.8)
                
		#laterality stuff
		laterality = as.vector(paf[,findPosInPaf("Laterality","")])
		#print(c("laterality more than one serie", laterality))
		lateralityCount = 1
		for(i in 1:length(unique(seriesName)))
		{
                        thisSerie = which(seriesName == unique(seriesName)[i])

			lateralityThisSerie = laterality[lateralityCount:(lateralityCount -1 + length(x[thisSerie]))]
			lateralityCount = lateralityCount + length(x[thisSerie])
			#print(c("laterality this serie", lateralityThisSerie))

			paintCrossVariablesLaterality(x[thisSerie], y[thisSerie], lateralityThisSerie, colBalls[thisSerie], varX = varX, varY = varY)
		}

		for(i in 1:length(seriesName)) {
                        thisSerie = which(seriesName == unique(seriesName)[i])

                        if(length(unique(x[thisSerie])) >= 3) {
                                if(varY == "Power" && ! dateAsX) {
                                        temp.list <- fitCurveCalc(x[thisSerie],y[thisSerie])
                                        x1 <- temp.list[[2]]
                                        y1 <- temp.list[[3]]
                                        fitCurvePlot(x1, y1, uniqueColors[i], 2, 1)
                                }
                                else if(varX == "Speed" && varY == "Force") #is "Force,Power" but using Force to have less problems
				{
                                        fit = lm(y[thisSerie] ~ x[thisSerie])
                                        V0 = -coef(fit)[[1]] / coef(fit)[[2]]
                                        F0 = coef(fit)[[1]]
                                        #draw points and mtext
                                        points(V0,0, col = uniqueColors[i])
                                        points(0,F0, col = uniqueColors[i])
                                        text(x=V0, y=0, paste(round(V0,2), "\n\n\n", sep=""), col = uniqueColors[i], cex=.8, adj=0.5)
                                        text(x=0, y=F0, paste("   ", round(F0,2), sep=""), col = uniqueColors[i], cex=.8, adj=0)
                                        #force x and y axis to start at 0
                                        axis(1,pos=0)
                                        axis(2,pos=0)
                                        fitLine(x[thisSerie],y[thisSerie], uniqueColors[i], 2, 1)
                                        doBox = FALSE
                                } else
                                        fitLine(x[thisSerie],y[thisSerie], uniqueColors[i], 2, 1)
                        }
                }

		#singleFile will be always false here (more than one serie)
		paintCrossVariablesNumsPrint(singleFile, Eccon, x, y, adjHor, cexNums, laterality)
                
                #difficult to create a title in series graphs
                title(paste(varXut,"/",varYut), cex.main=1, font.main=2)

		#on equivalent mass, series are numbers, round it and add Kg
		if(inertialGraphX == "EQUIVALENT_MASS" && is.numeric(seriesName))
			seriesName = paste(round(seriesName,2), "Kg")

                #plot legend on top exactly out
                #http://stackoverflow.com/a/7322792
                rng=par("usr")
                lg = legend(rng[1],rng[4], 
                            legend=unique(seriesName), lty=1, lwd=2, col=uniqueColors, 
                            cex=1, bg="white", ncol=length(unique(seriesName)), bty="n",
                            plot=F)
                legend(rng[2]-1.25*lg$rect$w,rng[4]+1*lg$rect$h,
                       legend=unique(seriesName), lty=1, lwd=2, col=uniqueColors, 
                       cex=1, bg="white", ncol=6, bty="n",
                       plot=T, xpd=NA)
        }
        
        if(isAlone == "ALONE") {
                if(doBox) {
                        if(dateAsX)
                                axis.Date(1,as.Date(x))
                        else
                                axis(1)
                        
                        axis(2)
                }
                mtext(varYut, side=2, line=3)
        } else if(isAlone == "LEFT") {
                axis(1)
                axis(2,col=colBalls)
                mtext(varYut, side=2, line=3, col=colBalls)
        } else { #"RIGHT"
                axis(4,col=colBalls)
                mtext(varYut, side=4, line=3, col=colBalls)
        }
        
        if(doBox) {
                box()
        }
        
}

paintCrossVariablesNumsPrint <- function (singleFile, Eccon, x, y, adjHor, cexNums, laterality)
{
        nums.print = NULL

	#show numbers at the side of names (take care of overlaps)
	#note stroverlap should be called after plot
	for(i in 1:length(x))
	{
		name = i
		if( ( Eccon=="ecS" || Eccon=="ceS" ) && singleFile) {
			#name = paste(trunc((name+1)/2),ecconVector[i],sep="")
			name = trunc((name+1)/2) #don't show e,c whe show the pch
		} else {
			#name = paste(name,ecconVector[i],sep="")
			#don't show e,c, we show the pch
		}

		newPoint = data.frame(x=x[i], y=y[i], curveNum=name)

		if(i == 1) {
			nums.print = data.frame()
			nums.print = rbind(nums.print, newPoint)
			colnames(nums.print) = c("x","y","curveNum")
		} else {
			overlaps = FALSE
			if( ! ( is.na(x[i]) && is.na(y[i]) ) )
				overlaps = stroverlapArray(newPoint, nums.print)
			if(! overlaps) {
				nums.print = rbind(nums.print, newPoint)
			}
		}
	}
        print(nums.print)
        print(laterality)
	text(as.numeric(nums.print$x), as.numeric(nums.print$y), paste("   ", nums.print$curveNum, laterality[nums.print$curveNum], sep = ""), adj=c(adjHor,.5), cex=cexNums)
}


#propulsive!!!!
paint1RMBadilloExercise <- function (exercise, paf, title, outputData1)
{
        curvesLoadTotal = (paf[,findPosInPaf("Load","")]) 		#mass: X
        curvesLoadExtra = (paf[,findPosInPaf("MassExtra","")])
        curvesSpeed = (paf[,findPosInPaf("Speed", "mean")])	#mean speed Y
        
        par(mar=c(5,6,3,4))
        
        loadPercent <- seq(30,100, by=5)
        
        #variables that are different on each exercise. Declare them here
        msp = NULL #msp: mean speed propulsive
        loadPercentCalc = NULL
        subtitle = NULL
        speed1RM = NULL
        speed1RMText = NULL
        formula = NULL
        reference = NULL
        
        #solve the quadratic ecuation on each %1RM vel = (-b - sqrt(b^2 -4ac))/(2a)
        if(exercise == "BENCH")
        {
                #a = 8.4326, b = - 73.501, c = 112.33
                msp <- c(1.33, 1.235, 1.145, 1.055, 0.965, 0.88, 0.795,
                         0.715, 0.635, 0.555, 0.475, 0.405, 0.325, 0.255, 0.185)
                #variation <- c(0.08, 0.07, 0.06, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.04, 0.04, 0.04, 0.04, 0.03, 0.04)
                
                loadPercentCalc <- 8.4326*curvesSpeed^2 - 73.501*curvesSpeed + 112.33
                subtitle <- translateToPrint("Concentric mean speed on bench press 1RM =")
                speed1RM <- 0.185
                speed1RMText <- " 0.185m/s"
                formula <- paste(" 8.4326 * ", translateToPrint("speed"), " ^2 - 73.501 * ", translateToPrint("speed"), " + 112.33")
                reference <- " Gonzalez-Badillo, Sanchez-Medina (2010)"
        } else
        { # == "SQUAT"
                #a = -2.079, b = -59.5, c = 120.5
                msp <- c(1.4477703, 1.3712719, 1.2943984, 1.2171443, 1.1395039, 1.0614714, 0.9830407,
                         0.9042056, 0.8249600, 0.7452972, 0.6652106, 0.5846934, 0.5037386, 0.4223390, 0.3404870)
                
                loadPercentCalc <- -2.079*curvesSpeed^2 - 59.5*curvesSpeed + 120.5
                subtitle <- translateToPrint("Concentric mean speed on squat 1RM =")
                #Note this prediction uses 0.340 as 1RM concentric speed on squat, but same authors found 0.310 on other articles
                #reason seem to be: this prediction is calculated on full squat
                speed1RM <- 0.340
                speed1RMText <- " 0.34m/s"
                formula <- paste("-2.079 * ", translateToPrint("speed"), " ^2 - 59.5 * ", translateToPrint("speed"), " + 120.5")
                reference <- " Gonzalez-Badillo, Sanchez-Medina (2015)"
        }
        
        maxy=max(c(msp,curvesSpeed))
        miny=min(c(msp,curvesSpeed))
        
        #sometimes there's a negative value, fix it
        for(i in 1:length(loadPercentCalc))
                if(loadPercentCalc[i] < 0)
                        loadPercentCalc[i] = NA
        
        #Isolating loadCalcExtra from loadPercentCalc = 100 * curvesLoadExtra / loadCalcExtra we have:
        loadCalcExtra <- 100 * curvesLoadExtra / loadPercentCalc
        
        #for calculations take only the curves slower or == than 1.33
        curvesSpeedInIntervalPos = which(curvesSpeed <= max(msp))
        
        if(length(curvesSpeedInIntervalPos) == 0) {
                plot(0,0,type="n",axes=F,xlab="",ylab="")
                text(x=0,y=0,translateToPrint("Not enough data."),cex=1.5)
                dev.off()
                write("1RM;-1", SpecialData)
                write("", outputData1)
                quit()
        }
        
        par(mar=c(6,5,3,4))
        
        plot(curvesLoadExtra,curvesSpeed, type="p",
             main=paste(title, "1RM", translateToPrint("prediction")),
             sub=paste(subtitle, speed1RMText,
                       translateToPrint("Estimated percentual load ="),
                       formula, "\n",
                       translateToPrint("Adapted from"), reference),
             xlim=c(min(curvesLoadExtra),max(loadCalcExtra[curvesSpeedInIntervalPos])),
             ylim=c(miny,maxy), xlab="", ylab="",axes=T)
        
        mtext(side=1,line=2,"Kg")
        mtext(side=2,line=3,paste(translateToPrint("Mean speed in concentric propulsive phase"),"(m/s)"))
        mtext(side=4,line=2,"1RM (%)")
        
        abline(h=msp, lty=2, col="gray")
        mtext(side=4,at=msp, paste(" ",loadPercent), las=2)
        
        colors=c(rep(NA,29),rev(heat.colors(100)[0:71]))
        arrows(curvesLoadExtra, curvesSpeed, loadCalcExtra, speed1RM, code=2, col=colors[loadPercentCalc])
        
        closerValues = which(curvesLoadExtra == max(curvesLoadExtra))
        segments(loadCalcExtra[closerValues], speed1RM, loadCalcExtra[closerValues], 0, lty=3)
        
        predicted1RM = mean(loadCalcExtra[closerValues])
        
        segments(predicted1RM, speed1RM, predicted1RM, 0, lty=1)
        mtext(side=1, at=predicted1RM, round(predicted1RM,2), cex=.8)
        
        mtext(speed1RM, at=speed1RM, side=2, cex=.8, las=2)
        
        write(paste("1RM;",round(predicted1RM,2),sep=""), SpecialData)
}

#---- RM Indirect start ----

RMIndirect <- function(Q, nrep) {
        #Q = load in Kg
        #nrep = number of maximum repetitions
        
        #nRM = the number of nRM you want to know
        #minimum 10, or more to match nrep
        if(nrep < 10)
                nRM = 10
        else
                nRM = nrep
        
        rm = matrix(rep(c(0,0,0,0,0,0,0,0), nRM), ncol=8)
        colnames(rm) = c("Brzycki", "Epley", "Lander", "Lombardi", "Mayhew", "Oconner", "Wathan", "AVG")
        rm = as.data.frame(rm)
        rm[1,1] = Q * (36 / (37 - nrep))                                    #Brzycki
        rm[1,2] = Q * (1 + 0.0333  *  nrep)                                 #Epley
        rm[1,3] = (100 * Q) / (101.3 - 2.67123 * nrep)                      #Lander   
        rm[1,4] = Q * nrep^0.1                                              #Lombardi
        rm[1,5] = (100 * Q) / (52.2 + (41.9 * exp(-0.055 * nrep)))          #Mayhew
        rm[1,6] = Q * (1 + 0.025 * nrep)                                    #O'Conner
        rm[1,7] = (100 * Q) / (48.8 + (53.8 * exp(-0.075 * nrep)))          #Wathan
        rm[1,8] = mean(as.numeric(rm[1,1:7]))
        
        if(nRM < 2) return(rm)
        for(i in 2:nRM) {
                rm[i,1] = rm[1,1] * (37 - i) / 36                           #Brzycki
                rm[i,2] = rm[1,2] / (1 + (i / 30))                          #Epley
                rm[i,3] = rm[1,3] * (101.3 - 2.67123 * i) / 100             #Lander
                rm[i,4] = rm[1,4] / (i ^ (1 / 10))                          #Lombardi
                rm[i,5] = rm[1,5] * (52.2 + (41.9 * exp(-1 * (i * 0.055)))) / 100       #Mayhew
                rm[i,6] = rm[1,6] / (1 + i * 0.025)                         #O'Conner
                rm[i,7] = rm[1,7]* (48.8 + (53.8 * exp(-1 * (i * 0.075)))) / 100        #Wathan
                rm[i,8] = mean(as.numeric(rm[i,1:7]))
        }
        return(rm)
}
plotRMIndirect <- function (RMIMatrix, Q, nrep) 
{
        #plotMode = "BALLS"
        plotMode = "PCHS"
        
        nRM = length(RMIMatrix[,1])
        
        ntests = length(RMIMatrix[1,]) -1 #-1 because we don't count the AVG
        uniqueColors=rainbow(ntests)
        
        par(mar=c(5,4,7,2)) #more space on the top
        
        #Create an empty plot
        plot(1, xlim=c(1,nRM),ylim=c(min(RMIMatrix),max(RMIMatrix)), type="n",
             xlab="Repetitions", ylab="Mass (Kg)", xaxt="n", las=2)
        axis(1,1:nRM) #plot xaxis ensuring 1:nRM is written
        
        #Draw grid
        abline(h=seq(0,max(RMIMatrix),by=5), lty=2, col="gray")
        abline(v=nrep, lty=2, col="gray")	
        
        #Draw all points except AVG (all the tests)
        if(plotMode == "BALLS") {
                # Note: this is fine tuned to have points at X:
                # -0.12, -0.8, -0.4, 0, 0.4, 0.8, 0.12
                # if there are more tests than 7, this need to be adjusted
                xmov = -0.12
                for(i in 1:ntests) {
                        points((1:nRM)+xmov, RMIMatrix[,i], type="p", pch=19, col=uniqueColors[i])
                        xmov = xmov +.04
                }
        } else { # "PCHS"
                for(i in 1:ntests)
                        points(RMIMatrix[,i], type="p", pch=i, col=uniqueColors[i])
        }
        
        #Draw AVG line
        lines(RMIMatrix$AVG, type="l", lwd=2)
        
        #Title
        mtext(paste("Indirect RM prediction with", nrep, "repetitions and", Q,  "Kg"), 
              side=3, at=5, adj=0.5, cex=1, line=5, font=2)
        
        #AVGs on top. Note ntests is the AVG column
        font = 2 #first column will be bold
        for(i in 1:nRM) {
                mtext(paste(i,"RM",sep=""), side=3, at=i, adj=0.5, cex=.8, line=2.5, font=font)
                mtext(round(RMIMatrix[i,(ntests+1)],1), side=3, at=i, adj=0.5, cex=.8, line=1.5, font=font)
                font = 1 #rest of the columns will not be bold
        }
        mtext("AVG", side=3, at=0, adj=.5, cex=.8, line=1.5)
        
        if(plotMode == "BALLS")
                legend("topright", legend=names(RMIMatrix), col=c(uniqueColors,"Black"), lwd=1, 
                       lty=c(rep(0,ntests),1), pch=c(rep(19,ntests),NA), cex=.8, bg="White") #legend
        else #PCHS
                legend("topright", legend=names(RMIMatrix), col=c(uniqueColors,"Black"), lwd=1, 
                       lty=c(rep(0,ntests),1), pch=c(rep(1:7),NA), cex=.8, bg="White") #legend
        
        par(mar=c(5,4,4,2))
        
        write(paste("1RM;",round(RMIMatrix[1,(ntests+1)],2),sep=""), SpecialData)
}

#---- RM Indirect end ----

find.mfrow <- function(n) {
        if(n<=3) return(c(1,n))
        else if(n<=8) return(c(2,ceiling(n/2)))
        else return(c(3, ceiling(n/3)))
}

find.xrange <- function(singleFile, displacement, curves) {
        n=length(curves[,1])
        x.max = 0
        for(i in 1:n) {
                x.current = length(displacement[curves[i,1]:curves[i,2]])
                if(max(x.current) > x.max)
                        x.max = max(x.current)
        }
        return (c(0,x.max))
}

find.yrange <- function(singleFile, displacement, curves) {
        n=length(curves[,1])
        y.max = 0
        y.min = 10000
        for(i in 1:n) { 
                y.current = cumsum(displacement[curves[i,1]:curves[i,2]])
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

#create ecconLine vector that will write "e" or "c" at each position
createEcconVector <- function(singleFile, Eccon, curvesNum, ecconVectorNotSingleFile)
{
        ecconVector = NULL
        if(singleFile) {
                if(Eccon == "c" || Eccon == "ec" || Eccon == "ce")
                        ecconVector = rep(Eccon,curvesNum) #will do ("c","c","c", ...) or ("ec","ec","ec",...)
                else if(Eccon == "ecS") {
                        ecconVector = rep(c("e","c"),trunc(curvesNum/2))
                        if(curvesNum%%2 == 1)
                                ecconVector = c(ecconVector,"e")
                }
                else if(Eccon == "ceS") {
                        ecconVector = rep(c("c","e"),trunc(curvesNum/2))
                        if(curvesNum%%2 == 1)
                                ecconVector = c(ecconVector,"c")
                }
        } else {
                ecconVector = ecconVectorNotSingleFile
        }
        
        return (ecconVector)
}
createPchVector <- function(ecconVector)
{
        pchVector = ecconVector
        pchVector[pchVector == "ec"] <- "21"
        pchVector[pchVector == "ce"] <- "21"
        pchVector[pchVector == "e"] <- "25"
        pchVector[pchVector == "c"] <- "24"
        
        return (as.numeric(pchVector))
}
createAngleVector <- function(ecconVector)
{
        angleVector = ecconVector
	j <- 1
	for(i in 1:length(ecconVector))
	{
		angle <- 45
		if(ecconVector[i] == "e")
			angle <- -45

		angleVector[j] <- angle
		j <- j +1
		angleVector[j] <- angle
		j <- j +1
	}
	return (as.numeric(angleVector))
}
createDensityVector <- function(ecconVector)
{
        densityVector = ecconVector
	j <- 1
	for(i in 1:length(ecconVector))
	{
		density <- -1
		if(ecconVector[i] == "e")
			density <- 30

		densityVector[j] <- density
		j <- j +1
		densityVector[j] <- density
		j <- j +1
	}
	return (as.numeric(densityVector))
}


#-------------------- EncoderConfiguration conversions --------------------------




#-------------- end of EncoderConfiguration conversions -------------------------

quitIfNoData <- function(curvesPlot, n, curves, outputData1, minHeight) 
{
        debugParameters(listN(n, curves, outputData1), "quitIfNoData")
        
        #if not found curves with this data, plot a "sorry" message and exit
        if( n== 0 || 
            ( n == 1 && (is.na(curves[1,1]) || curves[1,1] == 0 || is.na(curves[1,2]) || curves[1,2] <= 0) )  #bad curves[1,2] on inertial returns -1
        ) {
                #if curvesPlot, then findCurvesNew has started a graph, don't need to start again 
                if(! curvesPlot)
                        plot(0,0,type="n",axes=F,xlab="",ylab="")
                
                text(x=0, y=0, adj=0, cex=1.2, col="red",
                     paste(translateToPrint("Sorry, no repetitions matched your criteria."),"\nMin height is = ",minHeight/10,"cm"))
                dev.off()
                write("", outputData1)
                quit()
        }
}


#check if there are different values of laterality
checkLateralityDifferent <- function(curves)
{
	if(length(unique(curves$laterality)) > 1)
		return(TRUE)

	return(FALSE)
}

loadLibraries <- function(os) {
        #library("EMD")
        #library("sfsmisc")
        if(os=="Windows")
                library("Cairo")
}

doProcess <- function(options) 
{
        op <- assignOptions(options)
        
        DEBUG <<- op$Debug

        #if unicodeWorks, then translations will be done
        #<<- to assign to a global variable
        unicodeWorks <<- checkUnicodeWorks()
        
        CROSSVALIDATESMOOTH <<- op$CrossValidate
        
        print(c("1 Title=",op$Title))
        #unicoded titles arrive here like this "\\", convert to "\", as this is difficult, do like this:
        #http://stackoverflow.com/a/17787736
        op$Title=parse(text = paste0("'", op$Title, "'"))
        print(c("1 Title=",op$Title))
        
        #--- include files ---
        if(op$Analysis == "neuromuscularProfile")
                source(paste(op$EncoderRScriptsPath, "/neuromuscularProfile.R", sep=""))
        else if(op$Analysis == "cross" && op$AnalysisVariables[1] == "Pmax(F0,V0)")
                source(paste(op$EncoderRScriptsPath, "/pfvProfileEvolution.R", sep=""))
        
        print(op$File)
        print(op$OutputGraph)
        print(op$OutputData1)
        print(op$FeedbackFileBase)
        print(op$SpecialData)
        
        #read AnalysisOptions
        #if is propulsive and rotatory inertial is: "p;ri" 
        #if nothing: "-;-"
        analysisOptionsTemp = unlist(strsplit(op$AnalysisOptions, "\\;"))
        isPropulsive = (analysisOptionsTemp[1] == "p")
        inertialType = ""	#TODO: use EncoderConfiguration
        if(length(analysisOptionsTemp) > 1) {
                inertialType = analysisOptionsTemp[2] #values: "" || "li" || "ri"
        }
        
        
        #inertial cannot be propulsive
        if(isInertial(op$EncoderConfigurationName))
                isPropulsive = FALSE
        
        #in "li": linear encoder with inertial machines,
        #it's recommended to attach string to the rolling axis
        #because then we have the information of the machine.
        #If we attach the string to the body of the person, it's wrong because:
        #1 there's a loose time (without tension) where person moves independent of rolling machine
        #2 (more important) person changes direction on the top, and this is BIG a change of speed and acceleration, 
        #	but machine is rolling at same direction and speed.
        #	Measuring what person does there is giving incorrect high values of power
        #	because that acceleration of the body is not related to any change of movement of the inertial machine 
        #3 Also, linear encoder on the body has another problem:
        #	the force of the disc to the body to make it go down when the body is in the top,
        #	is a force that contributes greatly on the change of direction,
        #	then this force has to be added to the gravity in the power calculation
        #	This is not calculated yet.
        
        
        if(op$Analysis != "exportCSV") {
                if(op$OperatingSystem=="Windows")
                        Cairo(op$Width, op$Height, file = op$OutputGraph, type="png", bg="white")
                else
                        png(op$OutputGraph, width=op$Width, height=op$Height)
                
                op$Title=gsub('_',' ',op$Title)
                op$Title=gsub('-','    ',op$Title)
        }
        
        titleType = "c"
        #if(isJump)
        #	titleType="jump"
        
        curvesPlot = FALSE
        if(op$Analysis == "curves" || op$Analysis == "curvesAC") {
                curvesPlot = TRUE
                par(mar=c(2,2.5,2,1))
        }
        
        #when a csv is used (it links to lot of files) then singleFile = FALSE
        singleFile = TRUE
        if(nchar(op$File) >= 40) {
                #file="/tmp...../chronojump-encoder-graph-input-multi.csv"
                #substr(file, nchar(file)-39, nchar(file))
                #[1] "chronojump-encoder-graph-input-multi.csv"
                if(substr(op$File, nchar(op$File)-39, nchar(op$File)) == "chronojump-encoder-graph-input-multi.csv") {
                        singleFile = FALSE
                }
        }
        
        #declare here
        SmoothingsEC = 0
        curvesHeight = NULL
        
        if(! singleFile) {	#reads CSV with curves to analyze
                #this produces a displacement, but note that a position = cumsum(displacement) cannot be done because:
                #this are separated movements
                #maybe all are concentric (there's no returning to 0 phase)
                
                #this version of curves has added specific data cols:
                #status, exerciseName, mass, smoothingOne, dateTime, myEccon
                
                inputMultiData=read.csv(file=op$File,sep=",",stringsAsFactors=F)
                
                displacement = NULL
                count = 0 #count of the readed values (each ms there is a value)
                start = NULL; end = NULL; startH = NULL
                status = NULL; id = NULL; exerciseName = NULL; massBody = NULL; massExtra = NULL
                dateTime = NULL; myEccon = NULL
                seriesName = NULL; percentBodyWeight = NULL
                
                #encoderConfiguration
                econfName = NULL; econfd = NULL; econfD = NULL; econfAnglePush = NULL; econfAngleWeight = NULL
                econfInertia = NULL; econfGearedDown = NULL
                laterality = NULL
                
                newLines=0;
                countLines=1; #useful to know the correct ids of active curves
                
                #in neuromuscular, when sending individual repetititions,
                #if all are concentric, csv has a header but not datarows
                #it will be better to do not allow to plot graph from Chronojump,
                #but meanwhile we can check like this:
                if(length(inputMultiData[,1]) == 0) {
                        plot(0,0,type="n",axes=F,xlab="",ylab="")

			if(op$Analysis == "neuromuscularProfile")
				text(x=0,y=0,paste(translateToPrint("Not enough data."), "\n",
							translateToPrint("Need at least three eccentric-concentric jumps")), cex=1.5)
			else
				text(x=0,y=0,translateToPrint("Not enough data."), cex=1.5)

                        dev.off()
                        write("", op$OutputData1)
                        quit()
                }
                
                for(i in 1:length(inputMultiData[,1])) { 
                        #plot only active curves
                        status = as.vector(inputMultiData$status[i])
                        
                        if(status != "active") {
                                newLines=newLines-1; 
                                countLines=countLines+1;
                                next;
                        }
                        
                        #fix gearedDown
                        inputMultiData$econfGearedDown[i] = readFromFile.gearedDown(inputMultiData$econfGearedDown[i])
                        
                        dataTempFile=scan(file=as.vector(inputMultiData$fullURL[i]),sep=",")
                        
                        #if curves file ends with comma. Last character will be an NA. remove it
                        #this removes all NAs on a curve
                        dataTempFile  = dataTempFile[!is.na(dataTempFile)]
                        
                        if(isInertial(inputMultiData$econfName[i])) {
                                dataTempFile = getDisplacementInertial(
                                        dataTempFile, inputMultiData$econfName[i], 
                                        inputMultiData$econfd[i], inputMultiData$econfD[i], 
                                        inputMultiData$econfGearedDown[i] )
                                #getDisplacementInertialBody is not needed because it's done on curve save
                        } else {
                                #dataTempFile = getDisplacement(inputMultiData$econfName[i], dataTempFile, op$diameter, op$diameterExt,
                                dataTempFile = getDisplacement(FALSE, inputMultiData$econfName[i], dataTempFile, inputMultiData$econfd, inputMultiData$econfD,
                                                               inputMultiData$econfGearedDown[i])
                        }
                        
                        
                        dataTempPhase=dataTempFile
                        processTimes = 1
			ecS_ecc_l = NULL
			ecS_con_l = NULL
			c_con_l = NULL
			startPos = 0
			endPos = 0

			if (as.vector(inputMultiData$eccon[i]) == "c")
			{
				c_con_l <- reduceCurveByPredictStartEnd (dataTempFile, "c", op$MinHeight)

				startPos <- c_con_l$startPos
				endPos <- c_con_l$endPos
			}
			else {
				#note if the row on inputMultiData is !c, the criteria of ec or ecS is in Roptions.txt
				if (op$Eccon == "ec")
				{
					reducedCurve_l <- reduceCurveByPredictStartEnd (dataTempFile,
											op$Eccon, op$MinHeight)
					startPos <- reducedCurve_l$startPos
					endPos <- reducedCurve_l$endPos
				}
				else #(op$Eccon=="ecS" || op$Eccon=="ceS") )
				{
					#if this curve is ecc-con and we want separated, divide the curve in two
					processTimes = 2

					#2023 May 4. Set the end of ecc and start of con at center of depression, and let reduce fix them
					endEcc = mean(which(cumsum(dataTempFile) == min(cumsum(dataTempFile))))
					startCon = mean(which(cumsum(dataTempFile) == min(cumsum(dataTempFile))))

					ecS_ecc_l <- reduceCurveByPredictStartEnd (dataTempFile[1:endEcc],
										   "e", op$MinHeight)
					ecS_con_l <- reduceCurveByPredictStartEnd (dataTempFile[startCon:length(dataTempFile)],
										   "c", op$MinHeight)
				}
                        }

			for(j in 1:processTimes)
			{
				if(processTimes == 2)
				{
                                        if(j == 1) {
						startPos <- ecS_ecc_l$startPos
						endPos <- ecS_ecc_l$endPos
						dataTempPhase <- dataTempFile[1:endEcc]
                                        } else {
						startPos <- ecS_con_l$startPos
						endPos <- ecS_con_l$endPos
						dataTempPhase <- dataTempFile[startCon:length(dataTempFile)]

                                                newLines=newLines+1
                                        }
                                }
                                displacement = c(displacement, dataTempPhase)
                                id[(i+newLines)] = countLines

				start[(i+newLines)] = count + startPos
				end[(i+newLines)] = count + endPos
				startH[(i+newLines)] = 0; #TODO check this on e

				#print ("assigned startPos and endPos")
				#print ("count, startPos, endPos, start[(i+newLines)], end[(i+newLines)]")
				#print (c(count, startPos, endPos, start[(i+newLines)], end[(i+newLines)]))

                                exerciseName[(i+newLines)] = as.vector(inputMultiData$exerciseName[i])
                                
                                #mass[(i+newLines)] = inputMultiData$mass[i]
                                massBody[(i+newLines)] = inputMultiData$massBody[i]
                                massExtra[(i+newLines)] = inputMultiData$massExtra[i]
                                
                                dateTime[(i+newLines)] = as.vector(inputMultiData$dateTime[i])
                                percentBodyWeight[(i+newLines)] = as.vector(inputMultiData$percentBodyWeight[i])
                                
                                #also encoder configuration stuff
                                econfName[(i+newLines)] = inputMultiData$econfName[i]
                                econfd[(i+newLines)] = inputMultiData$econfd[i]
                                econfD[(i+newLines)] = inputMultiData$econfD[i]
                                econfAnglePush[(i+newLines)] = inputMultiData$econfAnglePush[i]
                                econfAngleWeight[(i+newLines)] = inputMultiData$econfAngleWeight[i]
                                econfInertia[(i+newLines)] = inputMultiData$econfInertia[i]/10000.0 #comes in Kg*cm^2 eg: 100; convert it to Kg*m^2 eg: 0.010
                                econfGearedDown[(i+newLines)] = inputMultiData$econfGearedDown[i]
                                
                                myPosition = cumsum(dataTempPhase)
                                if(processTimes == 2) {
                                        if(j == 1) {
                                                myEccon[(i+newLines)] = "e"
                                                id[(i+newLines)] = paste(countLines, myEccon[(i+newLines)], sep="")
                                        } else {
                                                myEccon[(i+newLines)] = "c"
                                                id[(i+newLines)] = paste(countLines, myEccon[(i+newLines)], sep="")
                                                countLines = countLines + 1
                                        }
                                        curvesHeight[(i+newLines)] = max(myPosition) - min(myPosition)
                                } else {
                                        if(inputMultiData$eccon[i] == "c") {
                                                myEccon[(i+newLines)] = "c"
                                                curvesHeight[(i+newLines)] = max(myPosition) - min(myPosition)
                                        } else {
                                                myEccon[(i+newLines)] = "ec"
                                                curvesHeight[(i+newLines)] = findDistanceAbsoluteEC(myPosition)
                                        }
                                        countLines = countLines + 1
                                }

                                seriesName[(i+newLines)] = as.vector(inputMultiData$seriesName[i])
                                
                                laterality[(i+newLines)] = as.vector(inputMultiData$laterality[i])
                                
                                count = count + length(dataTempPhase)
                        }
                        write(paste("***",i,"***",sep=""), stderr())
                }
                
                #position=cumsum(displacement)
                
                #curves = data.frame(id,start,end,startH,exerciseName,mass,dateTime,myEccon,stringsAsFactors=F,row.names=1)
                #this is a problem when there's only one row as seen by the R code of data.frame. ?data.frame:
                #"If row names are supplied of length one and the data frame has a
                #single row, the ‘row.names’ is taken to specify the row names and
                #not a column (by name or number)."
                #then a column id is created when there's only on row, but it is not created there's more than one.
                #solution:
                if(length(id)==1) {
                        curves = data.frame(start,end,startH,exerciseName,massBody,massExtra,
                                            dateTime,myEccon,seriesName,percentBodyWeight,
                                            econfName,econfd,econfD,econfAnglePush,econfAngleWeight,econfInertia,econfGearedDown,
                                            laterality,
                                            stringsAsFactors=F,row.names=id)
                } else {
                        curves = data.frame(id,start,end,startH,exerciseName,massBody,massExtra,
                                            dateTime,myEccon,seriesName,percentBodyWeight,
                                            econfName,econfd,econfD,econfAnglePush,econfAngleWeight,econfInertia,econfGearedDown,
                                            laterality,
                                            stringsAsFactors=F,row.names=1)
                }
                
                print("Creating (op$FeedbackFileBase)4.txt with touch method...")
                file.create(paste(op$FeedbackFileBase,"4.txt",sep=""))
                
                n=length(curves[,1])
                quitIfNoData(curvesPlot, n, curves, op$OutputData1, op$MinHeight)
                
                print(curves, stderr())
                
                #find SmoothingsEC. TODO: fix this
                if(CROSSVALIDATESMOOTH) {
                        for(i in 1:n)
                                SmoothingsEC[i] = 0
                }
                else {
                        singleCurveNum <- -1
                        if(op$Analysis == "single" && op$Jump > 0)
                                singleCurveNum <- op$Jump
                        SmoothingsEC <- findSmoothingsEC(singleFile, displacement, curves, singleCurveNum, op$Eccon, op$SmoothingOneC, op$MinHeight,
                                                         NULL, NULL, NULL, NULL) #this row is only needed for singleFile (signal)
                }
                
                print(c("SmoothingsEC:",SmoothingsEC))
        }
	else
	{	#singleFile == TRUE reads a signal file

                displacement <- scan(file=op$File,sep=",")
                #if data file ends with comma. Last character will be an NA. remove it
                #this removes all NAs
                displacement <- displacement[!is.na(displacement)]
                displacementInertialNotBody <- NULL
                
                if(isInertial(op$EncoderConfigurationName)) 
                {
                        #Disabled just after 1.7.0
                        #This process is only done on the curves after capture (not on recalculate or load)
                        #if(op$Analysis == "curvesAC" && op$CheckFullyExtended > 0)
                        #	displacement <- fixInertialSignalIfNotFullyExtended(displacement,
                        #							    op$CheckFullyExtended,
                        #							    paste(op$EncoderTempPath,
                        #								  "/chronojump-last-encoder-data.txt",
                        #								  sep=""),
                        #							    op$SpecialData,
                        #							    FALSE)
                        
                        diametersPerMs <- getInertialDiametersPerMs(displacement, op$diameter)
                        displacement <- getDisplacementInertial(displacement, op$EncoderConfigurationName, 
                                                                diametersPerMs, op$diameterExt, op$gearedDown)
                        
                        displacementInertialNotBody <- displacement #store a copy to be used on "single" (all set) to have a better set smooth
                        displacement <- getDisplacementInertialBody(0, displacement, curvesPlot, op$Title)
                        #positionStart is 0 in graph.R. It is different on capture.R because depends on the start of every repetition
                        
                        curvesPlot <- FALSE
                } else {
                        displacement <- getDisplacement(FALSE, op$EncoderConfigurationName, displacement, op$diameter, op$diameterExt, op$gearedDown)
                }
                
                #TODO: is this needed at all?
                if(length(displacement)==0) {
                        plot(0,0,type="n",axes=F,xlab="",ylab="")
                        text(x=0,y=0,translateToPrint("Encoder is not connected."),cex=1.5)
                        dev.off()
                        write("", op$OutputData1)
                        quit()
                }
                
                position=cumsum(displacement)

		#if(usingTriggers)
		if(cutByTriggers(op))
                        curves <- findCurvesByTriggers(displacement, op$TriggersOnList)
                else
                        curves <- findCurvesNew(displacement, op$Eccon,
                                                isInertial(op$EncoderConfigurationName), op$MinHeight)
                
                if(curvesPlot)
                        startCurvesPlot(position, op$Title)
                
                
                if(op$Analysis == "curves" || op$Analysis == "curvesAC")
                        curvesPlot = TRUE
                
                n=length(curves[,1])
                quitIfNoData(curvesPlot, n, curves, op$OutputData1, op$MinHeight)
                
                print("curves before reducing")
                print(curves)
                
                #reduceCurveBySpeed, don't do in inertial because it doesn't do a good right adjust on changing phase
                #what reduceCurveBySpeed is doing in inertial is adding a value at right, and this value is a descending value
                #and this produces a high acceleration there
                
                for(i in 1:n)
                {
			reducedCurve_l = NULL

			#reduceCurveBySpeed only when ! cutBytriggers
			if(! cutByTriggers(op))
                        {
				displacementTemp = displacement[curves[i,1]:curves[i,2]]
				if (op$Eccon == "c" || op$Eccon == "e")
				{
					reducedCurve_l <- reduceCurveByPredictStartEnd (displacementTemp,
											op$Eccon, op$MinHeight)

					#1st assign end because start will also change
					curves[i,2] <- curves[i,1] + (reducedCurve_l$endPos -1)
					curves[i,1] <- curves[i,1] + (reducedCurve_l$startPos -1)

					#curves[i,3] <- position[reducedCurve_l$startPos] - position [curves[i,1]]
					#TODO: fix above line
					curves[i,3] <- 0
				}
				else if (op$Eccon == "ecS")
				{
					reducedCurve_l <- NULL
					if (position[curves[i,1]] < position[curves[i,2]])
						reducedCurve_l <- reduceCurveByPredictStartEnd (displacementTemp,
												"c", op$MinHeight)
					else
						reducedCurve_l <- reduceCurveByPredictStartEnd (displacementTemp,
												"e", op$MinHeight)

					#1st assign end because start will also change
					curves[i,2] <- curves[i,1] + (reducedCurve_l$endPos -1)
					curves[i,1] <- curves[i,1] + (reducedCurve_l$startPos -1)

					#curves[i,3] <- position[reducedCurve_l$startPos] - position [curves[i,1]]
					#TODO: fix above line
					curves[i,3] <- 0
				}
				else if (op$Eccon == "ec")
				{
					reducedCurve_l <- reduceCurveByPredictStartEnd (displacementTemp,
											op$Eccon, op$MinHeight)

					curves[i,2] <- curves[i,1] + (reducedCurve_l$endPos -1)
					curves[i,1] <- curves[i,1] + (reducedCurve_l$startPos -1)
					#curves[i,3] <- position (curves[i,3] - ecc_l$startPos)
					#TODO: fix above line
					curves[i,3] <- 0
				}
                        }
                        
                        myPosition = cumsum(displacement[curves[i,1]:curves[i,2]])
                        if(op$Eccon == "ec")
                                curvesHeight[i] = findDistanceAbsoluteEC(myPosition)
                        else
                                curvesHeight[i] = max(myPosition) - min(myPosition)
                }
                
                #print("curves after reduceCurveBySpeed")
                #print(curves)
                
                #find SmoothingsEC
		#if(CROSSVALIDATESMOOTH) {
			#for(i in 1:n)
				#SmoothingsEC[i] = 0
		#}
		#else {
			singleCurveNum <- -1
			if(op$Analysis == "single" && op$Jump > 0)
				singleCurveNum <- op$Jump
			SmoothingsEC <- findSmoothingsEC(
				singleFile, displacement, curves, singleCurveNum, op$Eccon, op$SmoothingOneC, op$MinHeight,
				op$EncoderConfigurationName, op$diameter, op$inertiaMomentum, op$gearedDown
			) #second row is needed for singleFile (signal)
		#}
		print(c("SmoothingsEC:",SmoothingsEC))

                if(curvesPlot) {
                        #/10 mm -> cm
                        for(i in 1:length(curves[,1])) { 
                                myLabel = i
                                myY = min(position)/10
                                adjVert = 0
                                if(op$Eccon=="ceS")
                                        adjVert = 1
                                
                                if(op$Eccon=="ecS" || op$Eccon=="ceS") {
                                        myEc=c("c","e")
                                        if(op$Eccon=="ceS")
                                                myEc=c("e","c")
                                        
                                        myLabel = paste(trunc((i+1)/2),myEc[((i%%2)+1)],sep="")
                                        myY = position[curves[i,1]]/10
                                        if(i%%2 == 1) {
                                                adjVert = 1
                                                if(op$Eccon=="ceS")
                                                        adjVert = 0
                                        }
                                }
                                text(x=((curves[i,1]+curves[i,2])/2/1000),	#/1000 ms -> s
                                     y=myY,labels=myLabel, adj=c(0.5,adjVert),cex=.9,col="blue")
                                
                                arrows(x0=(curves[i,1]/1000),y0=myY,x1=(curves[i,2]/1000),	#/1000 ms -> s
                                       y1=myY, col="blue",code=0,length=0.1)
                                
                                #mtext(at=((curves[i,1]+curves[i,2])/2/1000),	#/1000 ms -> s
                                #     side=1,text=myLabel, cex=.8, col="blue")
                                abline(v=c(curves[i,1],curves[i,2])/1000, lty=3, col="grey30")

				# if(! cutByTriggers(op) && op$TriggersOnList != "" && op$TriggersOnList != -1)
				# {
				# 	abline(v=op$TriggersOnList/1000, col="yellow3", lwd=2, lty=2)
				# }
                        }

			##plot speed
			#par(new=T)
			#
			#smoothingAll= 0.1
			#speed <- getSpeed(displacement, smoothingAll)
			#plot((1:length(displacement))/1000, speed$y, col="green2",
				#type="l",
				#xlim=c(1,length(displacement))/1000,	#ms -> s
				##ylim=c(-.25,.25),		#to test speed at small changes
				#xlab="",ylab="",axes=F)
			#
			#if(isInertial(op$EncoderConfigurationName))
				#mtext(translateToPrint("body speed"),side=4,adj=1,line=-1,col="green2",cex=.8)
			#else
				#mtext(translateToPrint("speed"),side=4,adj=1,line=-1,col="green2")
			#
			#abline(h=0,lty=2,col="gray")
                }
        }
        
        
        write("starting analysis...", stderr())
        
        #make some check here, because this file is being readed in chronojump
        
        print("Creating (op$FeedbackFileBase)5.txt with touch method...")
        file.create(paste(op$FeedbackFileBase,"5.txt",sep=""))
        print(curves)

	#set debug global optionw
	displacementDebug <<- displacement
	curvesDebug <<- curves
	ecconDebug <<- op$Eccon
	singleFileDebug <<- singleFile

        if(op$Analysis == "single" || op$Analysis == "singleAllSet")
        {
                showPosition <- (op$AnalysisVariables[1] == "Position")
                showSpeed <- (op$AnalysisVariables[2] == "Speed")
                showAccel <- (op$AnalysisVariables[3] == "Accel")
                showForce <- (op$AnalysisVariables[4] == "Force")
                showPower <- (op$AnalysisVariables[5] == "Power")
                df = NULL
                
		if(op$Analysis == "single")
		{
                        myStart = curves[op$Jump,1]
                        myEnd = curves[op$Jump,2]
                        
                        repOp <- assignRepOptions(
                                singleFile, curves, op$Jump,
                                op$MassBody, op$MassExtra, op$Eccon, op$ExercisePercentBodyWeight, 
                                op$EncoderConfigurationName, op$diameter, op$diameterExt, 
                                op$anglePush, op$angleWeight, op$inertiaMomentum, op$gearedDown,
                                "") #op$laterality
                        
                        
                        
                        myCurveStr = paste(translateToPrint("Repetition"),"=", op$Jump, " ", repOp$laterality, " ", repOp$massExtra, "Kg", sep="")
                        
                        #don't do this, because on inertial machines string will be rolled to machine and not connected to the body
                        #if(inertialType == "li") {
                        #	displacement[myStart:myEnd] = fixRawdataLI(displacement[myStart:myEnd])
                        #	repOp$eccon="c"
                        #}
                        
                        #find which SmoothingsEC is needed
                        smoothingPos <- 1
                        if(op$Jump %in% rownames(curves))
                                smoothingPos <- which(rownames(curves) == op$Jump)

			triggersOnList = "";
		        if(! cutByTriggers(op))
				triggersOnList = op$TriggersOnList;

                        paint(displacement, repOp$eccon, myStart, myEnd, "undefined","undefined","undefined",op$Analysis,1,FALSE,
                              1,curves[op$Jump,3],SmoothingsEC[smoothingPos],op$SmoothingOneC,repOp$massBody,repOp$massExtra, op$MinHeight,
                              repOp$econfName,repOp$diameter,repOp$diameterExt,repOp$anglePush,repOp$angleWeight,repOp$inertiaM,repOp$gearedDown,"", #laterality
                              paste(op$Title, " ", op$Analysis, " ", repOp$eccon, ". ", myCurveStr, sep=""),
                              "", #subtitle
                              TRUE,	#draw
                              op$Width,
                              TRUE,	#showLabels
                              FALSE,	#marShrink
                              TRUE,	#showAxes
                              TRUE,	#legend
                              op$Analysis, isPropulsive, inertialType, repOp$exPercentBodyWeight,
                              showPosition, showSpeed, showAccel, showForce, showPower,
			      triggersOnList
                        )
                        
                        
                        #record array of data	
                        write("going to create array of data", stderr())
                        kn <- kinematicsF(displacement[curves[op$Jump,1]:curves[op$Jump,2]],
                                          repOp,
                                          #SmoothingsEC[smoothingPos], op$SmoothingOneC, g, isPropulsive)
                                          SmoothingsEC[smoothingPos], op$SmoothingOneC, g, 
                                          FALSE,	#create array of data with all curve and not only propulsive phase
                                          FALSE,		#show all the repetition, not only ground phase on ecc
					  op$MinHeight
                        ) 
                        
                        #smoothing for displacement
                        smoothingTemp = 0
                        if(repOp$eccon == "c" || repOp$eccon == "e")
                                smoothingTemp = op$SmoothingOneC
                        else
                                smoothingTemp = SmoothingsEC[smoothingPos]
                        
                        #prepare dataframe (will be written later)
                        df=data.frame(cbind(getPositionSmoothed(kn$displ,smoothingTemp), kn$speedy, kn$accely, kn$force, kn$power))
                }
		else 	# op$Analysis == "singleAllSet"
		{
                        #1) find maxPowerAtAnyRep
                        maxPowerAtAnyRep <- 0
                        for(i in 1:n) {
                                i.displ <- displacement[curves[i,1]:curves[i,2]]	
                                speed <- getSpeed(i.displ, op$SmoothingOneC)
                                accel <- getAccelerationSafe(speed)
                                #speed comes in mm/ms when derivate to accel its mm/ms^2 to convert it to m/s^2 need to *1000 because it's quadratic
                                accel$y <- accel$y * 1000 
                                
                                dynamics <- getDynamics (op$EncoderConfigurationName,
                                                         speed$y, accel$y, 
                                                         op$MassBody, op$MassExtra, op$ExercisePercentBodyWeight,
                                                         op$gearedDown, op$anglePush, op$angleWeight,
                                                         i.displ, op$diameter, 
                                                         op$inertiaMomentum, op$SmoothingOneC
                                )
                                if(max(dynamics$power) > maxPowerAtAnyRep)
                                        maxPowerAtAnyRep <- max(dynamics$power)
                        }
                        
                        #if is inertial, then use displacement of the machine (not the body) to have a better smooth
                        displacementAllSet <- displacement
                        if(isInertial(op$EncoderConfigurationName))
                                displacementAllSet <- displacementInertialNotBody
                        
                        #2.a) find max power (y) for some smoothings(x)
                        x <- seq(from = op$SmoothingOneC, to = 0, length.out = 30)
                        y <- smoothAllSetYPoints(x, displacementAllSet, 
                                                 op$EncoderConfigurationName, op$MassBody, op$MassExtra, op$ExercisePercentBodyWeight, 
                                                 op$gearedDown, op$anglePush, op$angleWeight, op$diameter, op$inertiaMomentum)
                        
                        smoothProblems = FALSE
                        #if we cannot find the max repetition power on the full set (at any spar value), use spar = 0
                        if(max(y) < maxPowerAtAnyRep) {
                                smoothingAll <- 0
                                smoothProblems = TRUE
                        } else {
                                #2.b) create a model with x,y to find optimal x
                                smodel <- smooth.spline(y,x)
                                smoothingAll <- predict(smodel, maxPowerAtAnyRep)$y
                                
                                debugParameters(listN(x, y, maxPowerAtAnyRep, smoothingAll), "paint all smoothing 1")
                                
                                #2.c) find x values close to previous model
                                temp.list <- findXValuesClose(x, y, maxPowerAtAnyRep)
                                xUpperValue <- temp.list[[1]]
                                xLowerValue <- temp.list[[2]]
                                
                                debugParameters(listN(xUpperValue, xLowerValue), "paint all smoothing 2")
                                
                                #3.a) find max power (y) for some smoothings(x) (closer)
                                x <- seq(from = xUpperValue, to = xLowerValue, length.out = 8)
                                y <- smoothAllSetYPoints(x, displacementAllSet, 
                                                         op$EncoderConfigurationName, op$MassBody, op$MassExtra, op$ExercisePercentBodyWeight, 
                                                         op$gearedDown, op$anglePush, op$angleWeight, op$diameter, op$inertiaMomentum)
                                
                                #3.b) create a model with x,y to find optimal x (in closer values)
                                #but only if we have 4+ unique values for smooth.spline. If not, previous smoothingAll will be used
                                if(length(unique(x)) >= 4 && length(unique(y)) >= 4) {
                                        smodel <- smooth.spline(y,x)
                                        smoothingAll <- predict(smodel, maxPowerAtAnyRep)$y
                                }
                        }
                        
                        debugParameters(listN(x, y, maxPowerAtAnyRep, smoothingAll), "paint all smoothing 3")
                        
                        #4) create dynamics data for this smoothing
                        speed <- getSpeed(displacementAllSet, smoothingAll)
                        accel <- getAccelerationSafe(speed)
                        #speed comes in mm/ms when derivate to accel its mm/ms^2 to convert it to m/s^2 need to *1000 because it's quadratic
                        accel$y <- accel$y * 1000
                        
                        dynamics <- getDynamics (op$EncoderConfigurationName,
                                                 speed$y, accel$y, 
                                                 op$MassBody, op$MassExtra, op$ExercisePercentBodyWeight,
                                                 op$gearedDown, op$anglePush, op$angleWeight,
                                                 displacementAllSet, op$diameter, 
                                                 op$inertiaMomentum, smoothingAll		#inertiaMomentum, smoothing
                        )
                        
                        
                        #5) prepare dataframe (will be written later)
                        df=data.frame(cbind(getPositionSmoothed(displacement,smoothingAll), speed$y, accel$y, dynamics$force, dynamics$power))
                        
                        #6) paint
                        
                        axisLineRight = 0
                        marginRight = 8.5
                        if (! showSpeed)
                                marginRight = marginRight -2
                        if(! showAccel)
                                marginRight = marginRight -2
                        if(! showForce)
                                marginRight = marginRight -2
                        if(! showPower)
                                marginRight = marginRight -2
                        
                        
                        par(mar=c(3, 3.5, 5, marginRight))
			if(showPosition)
			{
				plot(position,	#mm
				     type="l", xlab="", ylab="",axes=T, lty=1,col="black")
			} else {
				plot(position,	#mm
				     type="n", xlab="", ylab="",axes=F, lty=1,col="black")
				axis(1)
			}
			title(main="All set (experimental!)",line=-2,outer=T)
                        
                        mtext(paste(translateToPrint("time"),"(ms)"),side=1,adj=1,line=-1)
                        #mtext(paste(translateToPrint("displacement"),"(mm)"),side=2,adj=1,line=-1)
                        
                        if(smoothProblems)
                                mtext("Caution! smoothing is not accurate on this set. Maximum values shown here are too small.",side=3,adj=.5,line=-1)
                        
                        #show vertical lines for every curve
                        for(i in 1:n) {
                                abline(v=c(curves[i,1], curves[i,2]), lty=2)
                                mtext(i, side=3, at=(curves[i,1] + curves[i,2])/2)
                        }

			if(op$TriggersOnList != "" && op$TriggersOnList != -1)
				abline(v=op$TriggersOnList, col="yellow3", lwd=2, lty=2);

                        if (showSpeed)
			{
                                speedCorrected = speed$y

				if(isInertial(op$EncoderConfigurationName))
				{
					#If the speed at the first repetition has the right sign, we change it because we will
					#start changing in the first repetition
					if(mean(speedCorrected[(curves[1,"endStored"] - 100):curves[1,"endStored"]]) > 0)
					{
						changingRep = 2
						speedCorrected[1:curves[1,"startStored"]] = -speedCorrected[1:curves[1,"startStored"]]
					}
					else if(mean(speedCorrected[(curves[1,"endStored"] - 100):curves[1,"endStored"]]) < 0)
					{
						changingRep = 1
					}

					#Changing the sign of each repetition alternately (changed, NOTchanged, changed, NOTchanged,....)
					while(changingRep <= length(curves[,"startStored"]))
					{
						speedCorrected[curves[changingRep,"startStored"]:curves[changingRep,"endStored"]] =
							-speedCorrected[curves[changingRep,"startStored"]:curves[changingRep,"endStored"]]
						changingRep = changingRep +2
					}
				}
                                
                                par(new=T)
                                ylimHeight = max(abs(range(speed$y)))
                                ylim=c(- 1.05 * ylimHeight, 1.05 * ylimHeight)	#put 0 in the middle, and have 5% margin at each side
                                #plot(speed$y, col=cols[1], ylim=ylim, type="l", xlab="",ylab="",axes=F)
                                plot(speedCorrected, col=cols[1], ylim=ylim, type="l", xlab="",ylab="",axes=F)
                                axis(4, col=cols[1], lty=lty[1], line=axisLineRight, lwd=1, padj=-.5)
                                axisLineRight = axisLineRight +2
                        }
                        
                        if (showAccel) {
                                par(new=T)	
                                ylimHeight = max(abs(range(accel$y)))
                                ylim=c(- 1.05 * ylimHeight, 1.05 * ylimHeight)	#put 0 in the middle, and have 5% margin at each side
                                
                                if(isInertial(op$EncoderConfigurationName))
                                        plot(abs(accel$y), col="magenta", ylim=ylim, type="l", xlab="",ylab="",axes=F)
                                else
                                        plot(accel$y, col="magenta", ylim=ylim, type="l", xlab="",ylab="",axes=F)
                                
                                axis(4, col="magenta", lty=lty[1], line=axisLineRight, lwd=1, padj=-.5)
                                axisLineRight = axisLineRight +2
                        }
                        
                        if (showForce) {
                                par(new=T)	
                                ylimHeight = max(abs(range(dynamics$force)))
                                ylim=c(- 1.05 * ylimHeight, 1.05 * ylimHeight)	#put 0 in the middle, and have 5% margin at each side
                                plot(dynamics$force, col=cols[2], ylim=ylim, type="l", xlab="",ylab="",axes=F)
                                axis(4, col=cols[2], lty=lty[1], line=axisLineRight, lwd=1, padj=-.5)
                                axisLineRight = axisLineRight +2
                        }
                        
                        if (showPower) {
                                par(new=T)	
                                ylimHeight = max(abs(range(dynamics$power)))
                                ylim=c(- 1.05 * ylimHeight, 1.05 * ylimHeight)	#put 0 in the middle, and have 5% margin at each side
                                plot(dynamics$power, col=cols[3], ylim=ylim, type="l", lwd=2, xlab="",ylab="",axes=F)
                                axis(4, col=cols[3], lty=lty[1], line=axisLineRight, lwd=1, padj=-.5)
                                axisLineRight = axisLineRight +2
                        }
                        
                        #always (single or side) show 0 line
                        if(showSpeed || showAccel || showForce || showPower)
                                abline(h=0,lty=3,col="black")
                        
                        paintVariablesLegend(showPosition, showSpeed && ! isInertial(op$EncoderConfigurationName), showAccel, showForce, showPower,
					     (op$TriggersOnList != "" && op$TriggersOnList != -1))
                }
                
                #needed to align the AB vertical lines on C#
                write(op$Width, op$SpecialData)
                write(par("usr"), op$SpecialData, append=TRUE)
                write(par("plt"),  op$SpecialData, append=TRUE)
                
                #write dataframe to file	
                colnames(df)=c("displacement","speed","acceleration","force","power")
                write("going to write it to file", stderr())
                write.csv(df, paste(op$EncoderTempPath,"/chronojump-analysis-instant.csv",sep=""), append=TRUE, quote=FALSE)
                write("done!", stderr())
        }
        
        if(op$Analysis=="side" || op$Analysis=="sideShareX")
	{
                #comparar 6 salts, falta que xlim i ylim sigui el mateix
                par(mfrow=find.mfrow(n))
                
		xrange="undefined"
		if(op$Analysis=="sideShareX") {
	                xrange=find.xrange(singleFile, displacement, curves)
		}

                yrange=find.yrange(singleFile, displacement, curves)
                
                #if !singleFile kinematicRanges takes the 'curves' values
                knRanges=kinematicRanges(singleFile, displacement, curves, 
                                         op$MassBody, op$MassExtra, op$ExercisePercentBodyWeight, 
                                         op$EncoderConfigurationName,op$diameter,op$diameterExt,
                                         op$anglePush,op$angleWeight,op$inertiaMomentum,op$gearedDown,
                                         SmoothingsEC, op$SmoothingOneC, 
                                         g, op$Eccon, isPropulsive, op$MinHeight)
                
                for(i in 1:n) {
                        repOp <- assignRepOptions(
                                singleFile, curves, i,
                                op$MassBody, op$MassExtra, op$Eccon, op$ExercisePercentBodyWeight, 
                                op$EncoderConfigurationName, op$diameter, op$diameterExt, 
                                op$anglePush, op$angleWeight, op$inertiaMomentum, op$gearedDown,
                                "") #op$laterality
                        
                        myTitle = ""
                        if(i == 1)
                                myTitle = paste(op$Title)
                        
                        mySubtitle = paste("Repetition=", rownames(curves)[i], ", ", repOp$laterality, " ", repOp$massExtra, "Kg", sep="")
                        
			triggersOnList = "";
		        if(! cutByTriggers(op))
				triggersOnList = op$TriggersOnList;

                        paint(displacement, repOp$eccon, curves[i,1],curves[i,2],xrange,yrange,knRanges,op$Analysis,i,FALSE,
                              1,curves[i,3],SmoothingsEC[i],op$SmoothingOneC,repOp$massBody,repOp$massExtra, op$MinHeight,
                              repOp$econfName,repOp$diameter,repOp$diameterExt,repOp$anglePush,repOp$angleWeight,repOp$inertiaM,repOp$gearedDown,"", #laterality
                              myTitle,mySubtitle,
                              TRUE,	#draw
                              op$Width,
                              FALSE,	#showLabels
                              TRUE,	#marShrink
                              FALSE,	#showAxes
                              FALSE,	#legend
                              op$Analysis, isPropulsive, inertialType, repOp$exPercentBodyWeight,
                              (op$AnalysisVariables[1] == "Position"), #show position
                              (op$AnalysisVariables[2] == "Speed"), #show speed
                              (op$AnalysisVariables[3] == "Accel"), #show accel
                              (op$AnalysisVariables[4] == "Force"), #show force
                              (op$AnalysisVariables[5] == "Power"),  #show power
			      triggersOnList
                        )
                }
                par(mfrow=c(1,1))
        }

	if(op$Analysis=="superpose")
	{	#TODO: fix on ec startH
        #		#falta fer un graf amb les 6 curves sobreposades i les curves de potencia (per exemple) sobrepossades
        #		#fer que acabin al mateix punt encara que no iniciin en el mateix
        #		#arreglar que els eixos de l'esq han de seguir un ylim,
        #		#pero els de la dreta un altre, basat en el que es vol observar
        #		#fer que es pugui enviar colors que es vol per cada curva, o linetypes
        #		wide=max(curves$end-curves$start)
        #
        #		#position=cumsum(displacement)
        #		#yrange=c(min(position),max(position))
        #		yrange=find.yrange(singleFile, displacement,curves)
        #
        #		knRanges=kinematicRanges(singleFile,displacement,curves,Mass,SmoothingOneEC,SmoothingOneC,g,Eccon,isPropulsive)
        #		for(i in 1:n) {
        #			#in superpose all jumps end at max height
        #			#start can change, some are longer than other
        #			#xmin and xmax should be the same for all in terms of X concordance
        #			#but line maybe don't start on the absolute left
        #			#this is controled by startX
        #			startX = curves[i,1]-(curves[i,2]-wide)+1;
        #			myTitle = "";
        #			if(i==1)
        #				myTitle = paste(titleType,Jump);
        #
        #			paint(displacement, Eccon, curves[i,2]-wide,curves[i,2],yrange,knRanges,TRUE,(i==Jump),
        #			      startX,curves[i,3],SmoothingOneEC,SmoothingOneC,Mass,myTitle,"",
        #			      TRUE,	#draw
        #			      TRUE,	#showLabels
        #			      FALSE,	#marShrink
        #			      (i==1),	#showAxes
        #			      TRUE,	#legend
        #			      op$Analysis, isPropulsive, inertialType, op$ExercisePercentBodyWeight 
        #			      )
        #			par(new=T)
        #		}
        #		par(new=F)
        #		#print(knRanges)

		xrange=find.xrange(singleFile, displacement, curves)
		yrange=find.yrange(singleFile, displacement, curves)

		#if !singleFile kinematicRanges takes the 'curves' values
		knRanges=kinematicRanges(singleFile, displacement, curves,
					 op$MassBody, op$MassExtra, op$ExercisePercentBodyWeight,
					 op$EncoderConfigurationName,op$diameter,op$diameterExt,
					 op$anglePush,op$angleWeight,op$inertiaMomentum,op$gearedDown,
					 SmoothingsEC, op$SmoothingOneC,
					 g, op$Eccon, isPropulsive, op$MinHeight)

		#check if there are different values of laterality
		lateralityDifferent = checkLateralityDifferent(curves)

		for(i in 1:n) {
			repOp <- assignRepOptions(
						  singleFile, curves, i,
						  op$MassBody, op$MassExtra, op$Eccon, op$ExercisePercentBodyWeight,
						  op$EncoderConfigurationName, op$diameter, op$diameterExt,
						  op$anglePush, op$angleWeight, op$inertiaMomentum, op$gearedDown,
						  op$laterality)

			if(! lateralityDifferent)
				repOp$laterality = ""

			myTitle = ""
			if(i == 1)
				myTitle = paste(op$Title)

			#mySubtitle = paste("curve=", rownames(curves)[i], ", ", repOp$laterality, " ", repOp$massExtra, "Kg", sep="")

			triggersOnList = "";
			if(! cutByTriggers(op))
				triggersOnList = op$TriggersOnList;

			#TODO: highlight can ve used asking user if want to highlight any repetition, and this will be lwd=2 or 3

			paint(displacement, repOp$eccon, curves[i,1],curves[i,2],xrange,yrange,knRanges, op$Analysis, rownames(curves)[i], FALSE,
			      1,curves[i,3],SmoothingsEC[i],op$SmoothingOneC,repOp$massBody,repOp$massExtra, op$MinHeight,
			      repOp$econfName,repOp$diameter,repOp$diameterExt,repOp$anglePush,repOp$angleWeight,repOp$inertiaM,repOp$gearedDown,repOp$laterality,
			      myTitle, "", #title, subtitle
			      TRUE,	#draw
			      op$Width,
			      FALSE,	#showLabels
			      FALSE,	#marShrink
			      (i==1),	#showAxes
			      (i==1),	#legend
			      op$Analysis, isPropulsive, inertialType, repOp$exPercentBodyWeight,
                              (op$AnalysisVariables[1] == "Position"), #show position
			      (op$AnalysisVariables[2] == "Speed"), #show speed
			      (op$AnalysisVariables[3] == "Accel"), #show accel
			      (op$AnalysisVariables[4] == "Force"), #show force
			      (op$AnalysisVariables[5] == "Power"),  #show power
			      triggersOnList
			      )

			par(new=T)
		}
	} #end of superpose
        
        #since Chronojump 1.3.6, encoder analyze has a treeview that can show the curves
        #when an analysis is done, curves file has to be written
        writeCurves = TRUE

	#declare pafCurves
	pafCurves = NULL

        #but don't writeCurves on exportCSV because outputfile is the same 
        if(op$Analysis == "exportCSV" || op$Analysis=="1RMIndirect")
                writeCurves = FALSE
        
        if(
                op$Analysis == "powerBars" || op$Analysis == "cross" || 
                op$Analysis == "1RMBadillo2010" || op$Analysis == "1RMAnyExercise" || 
                op$Analysis == "curves" || op$Analysis == "curvesAC" || op$Analysis == "curvesProcessMultiDB" ||
                op$Analysis == "neuromuscularProfile" ||
                op$Analysis == "pfvProfileEvolution" ||
                writeCurves) 
        {
                paf = data.frame()
                discardedCurves = NULL
                discardingCurves = FALSE
                for(i in 1:n) { 
                        repOp <- assignRepOptions(
                                singleFile, curves, i,
                                op$MassBody, op$MassExtra, op$Eccon, op$ExercisePercentBodyWeight, 
                                op$EncoderConfigurationName, op$diameter, op$diameterExt, 
                                op$anglePush, op$angleWeight, op$inertiaMomentum, op$gearedDown,
                                "") #laterality 
                        
                        if(! singleFile) {
                                #only use concentric data	
                                if( (op$Analysis == "1RMBadillo2010" || op$Analysis == "1RMAnyExercise" ||
				     (op$Analysis == "cross" && op$AnalysisVariables[1] == "Pmax(F0,V0)")
				     ) && repOp$eccon == "e")
				{
                                        discardedCurves = c(i,discardedCurves)
                                        discardingCurves = TRUE
                                        next;
                                }
                        } else {
                                if( (op$Analysis == "1RMBadillo2010" || op$Analysis == "1RMAnyExercise") & repOp$eccon == "ecS" & i%%2 == 1) {
                                        discardedCurves = c(i,discardedCurves)
                                        discardingCurves = TRUE
                                        next;
                                }
                                else if( (op$Analysis == "1RMBadillo2010" || op$Analysis == "1RMAnyExercise") & repOp$eccon == "ceS" & i%%2 == 0) {
                                        discardedCurves = c(i,discardedCurves)
                                        discardingCurves = TRUE
                                        next;
                                }
                        }
                        
                        #print(c("i, curves[i,1], curves[i,2]", i, curves[i,1],curves[i,2]))
                        
                        #if ecS go kinematics first time with "e" and second with "c"
                        repOpSeparated = repOp
                        if(repOp$eccon=="ecS") {
                                if(i%%2 == 1)
                                        repOpSeparated$eccon = "e"
                                else
                                        repOpSeparated$eccon = "c"
                        }
                        else if(repOp$eccon=="ceS") {
                                if(i%%2 == 1)
                                        repOpSeparated$eccon = "c"
                                else
                                        repOpSeparated$eccon = "e"
                        }

                        paf = rbind(paf,(pafGenerate(
                                repOpSeparated$eccon,
                                kinematicsF(displacement[curves[i,1]:curves[i,2]], 
                                            #myMassBody, myMassExtra, myExPercentBodyWeight,
                                            #myEncoderConfigurationName,myDiameter,myDiameterExt,myAnglePush,myAngleWeight,
                                            #myInertiaMomentum,myGearedDown,
                                            repOpSeparated,
                                            SmoothingsEC[i],op$SmoothingOneC, 
                                            g, isPropulsive, TRUE, op$MinHeight),
                                repOp$massBody, repOp$massExtra, repOp$laterality, repOp$inertiaM, repOp$diameter, repOp$gearedDown
                        )))
                }
                
                #on 1RMBadillo discard curves "e", because paf has this curves discarded
                #and produces error on the cbinds below			
                if(discardingCurves)
                        curves = curves[-discardedCurves,]
                
                rownames(paf)=rownames(curves)
                
                if(op$Analysis == "powerBars") {
                        ecconVector = createEcconVector(singleFile, op$Eccon, length(curves[,1]), curves[,8])
                        if(! singleFile) 
                                paintPowerPeakPowerBars(singleFile, op$Title, paf, 
                                                        op$Eccon,	 			#Eccon
							ecconVector,
                                                        curvesHeight,			#height 
                                                        n, 
                                                        (op$AnalysisVariables[1] == "Impulse"), 	#show impulse
                                                        (op$AnalysisVariables[2] == "TimeToPeakPower"), #show time to pp
                                                        (op$AnalysisVariables[3] == "Range"), 		#show range
							curves[,2]-curves[,1] 				#totalTime
                                )		
                        else 
                                paintPowerPeakPowerBars(singleFile, op$Title, paf, 
                                                        op$Eccon,					#Eccon
							ecconVector,
                                                        curvesHeight,			#height 
                                                        n, 
                                                        (op$AnalysisVariables[1] == "Impulse"), 	#show impulse
                                                        (op$AnalysisVariables[2] == "TimeToPeakPower"), #show time to pp
                                                        (op$AnalysisVariables[3] == "Range"), 		#show range
							curves[,2]-curves[,1] 				#totalTime
                                ) 
                }
                else if(op$Analysis == "cross" && op$AnalysisVariables[1] != "Pmax(F0,V0)")
		{
			print("printing PAF")
                        print(paf)
                        mySeries = "1"
                        myDateTime = NULL
                        if(! singleFile) {
                                mySeries = curves[,9]
                                myDateTime = curves[,7]
                        }
                        
                        ecconVector = createEcconVector(singleFile, op$Eccon, length(curves[,1]), curves[,8])
                        par(mar=c(5,4,4,6))
			if(
			   (op$AnalysisVariables[1] == "Speed" && op$AnalysisVariables[2] == "Load") ||
			   (op$AnalysisVariables[1] == "Force" && op$AnalysisVariables[2] == "Load") ||
			   (op$AnalysisVariables[1] == "Power" && op$AnalysisVariables[2] == "Speed") )
			{
				par(mar=c(5,4,4,2))
			}

                        if(op$AnalysisVariables[1] == "Speed,Power")
			{
                                analysisVertVars = unlist(strsplit(op$AnalysisVariables[1], "\\,"))
                                paintCrossVariables(paf, op$AnalysisVariables[2], analysisVertVars[1], op$AnalysisVariables[3], 
                                                    FALSE, NULL,
                                                    "LEFT", "",
                                                    singleFile,
                                                    op$Eccon,
                                                    ecconVector,
                                                    mySeries, 
                                                    op$InertialGraphX,
                                                    FALSE, FALSE, op$OutputData1) 
                                par(new=T)
                                paintCrossVariables(paf, op$AnalysisVariables[2], analysisVertVars[2], op$AnalysisVariables[3], 
                                                    FALSE, NULL,
                                                    "RIGHT", op$Title,
                                                    singleFile,
                                                    op$Eccon,
                                                    ecconVector,
                                                    mySeries, 
                                                    op$InertialGraphX,
                                                    FALSE, FALSE, op$OutputData1) 
                        } else {
				#if Force,Power is on Y, then send only force to paintCrossVariables. Power will be added on that function
				if(op$AnalysisVariables[1] == "Force,Power") {
					op$AnalysisVariables[1] = "Force";
				}
				#same for Load,Power
				else if(op$AnalysisVariables[1] == "Load,Power") {
					op$AnalysisVariables[1] = "Load";
				}

                                dateAsX <- FALSE
                                if(length(op$AnalysisVariables) == 4 && op$AnalysisVariables[4] == "Date")
                                        dateAsX <- TRUE
                                paintCrossVariables(paf, op$AnalysisVariables[2], op$AnalysisVariables[1], op$AnalysisVariables[3], 
                                                    dateAsX, myDateTime,
                                                    "ALONE", op$Title,
                                                    singleFile,
                                                    op$Eccon,
                                                    ecconVector,
                                                    mySeries, 
                                                    op$InertialGraphX,
                                                    FALSE, FALSE, op$OutputData1) 
                        }
                }
                else if(op$Analysis == "1RMAnyExercise") {
                        mySeries = "1"
                        if(! singleFile)
                                mySeries = curves[,9]
                        
                        ecconVector = createEcconVector(singleFile, op$Eccon, length(curves[,1]), curves[,8])
                        
                        paintCrossVariables(paf, "Load", "Speed", "mean",
                                            FALSE, NULL,
                                            "ALONE", op$Title,
                                            singleFile,
                                            op$Eccon,
                                            ecconVector,
                                            mySeries, 
					    op$InertialGraphX,
                                            op$AnalysisVariables[1], op$AnalysisVariables[2], #speed1RM, method
                                            op$OutputData1) 
                }
                else if(op$Analysis == "1RMBadilloBench") {
                        paint1RMBadilloExercise("BENCH", paf, op$Title, op$OutputData1)
                }
                else if(op$Analysis == "1RMBadilloSquat") {
                        paint1RMBadilloExercise("SQUAT", paf, op$Title, op$OutputData1)
                } 
                else if(op$Analysis == "neuromuscularProfile")
		{
                        #only signal, it's a jump, use mass of the body (100%) + mass Extra if any

			#TODO: need to also split by massExtra. but then each set instead to have best repetition save all, as the criteria of best or three, but best is not the same because it takes account e & c (powers) and neuromuscular chooses best by jump height... yes! best seems to be the best by concentric power
			#TODO: fix when a person has just 2 jumps (eg. Alvaro Perez Campo). This is also related to previous (comprovar on passa el error)
			#on groupal_current_session, seriesName is different:
			if (length(unique(curves$seriesName)) > 1)
			{
				curves_l = split (curves, curves$seriesName)
				npj_l <- list() #list of lists
				names_c <- NULL
				for (curves_li in 1:length (curves_l))
				{
					npj <- neuromuscularProfileGetData (FALSE, displacement, curves_l[[curves_li]], (op$MassBody + op$MassExtra), op$SmoothingOneC)
					if(is.double(npj) && npj == -1)
						next;

					npj_l[[curves_li]] <- npj
					names_c <- c(names_c, curves_l[[curves_li]][1,]$seriesName)
				}
				writeCurves = FALSE

				if (length (npj_l) == 0)
				{
					plot(0,0,type="n",axes=F,xlab="",ylab="")
					text(x=0,y=0,paste(translateToPrint("Not enough data."), "\n",
							   translateToPrint("Need at least three jumps of same person")),
					     cex=1.5)
					dev.off()
					write("", op$OutputData1)
					quit()
				}

				np.bar.load <- neuromuscularProfile3NAvg (npj_l, "LOAD")
				np.bar.explode <- neuromuscularProfile3NAvg (npj_l, "EXPLODE")
				np.bar.drive <- neuromuscularProfile3NAvg (npj_l, "DRIVE")

				par(mar=c(3,4,2,4))
				neuromuscularProfilePlotBars(op$Title, np.bar.load, np.bar.explode, np.bar.drive)

				neuromuscularProfileWriteDataNPersons (npj_l, names_c, op$OutputData1)
			} else {
				npj <- neuromuscularProfileGetData(singleFile, displacement, curves, (op$MassBody + op$MassExtra), op$SmoothingOneC)

				if(is.double(npj) && npj == -1) {
					plot(0,0,type="n",axes=F,xlab="",ylab="")
					text(x=0,y=0,paste(translateToPrint("Not enough data."), "\n",
							   translateToPrint("Need at least three jumps")),
					     cex=1.5)
					dev.off()
					write("", op$OutputData1)
					quit()
				}

				np.bar.load <- neuromuscularProfile3JLoadAvg (npj)
				np.bar.explode <- neuromuscularProfile3JExplodeAvg (npj)
				np.bar.drive <- neuromuscularProfile3JDriveAvg (npj)

				par(mar=c(3,4,2,4))
				par(mfrow=c(2,1))
				neuromuscularProfilePlotBars(op$Title, np.bar.load, np.bar.explode, np.bar.drive)
				par(mar=c(4,4,1,4))

				neuromuscularProfilePlotOther(
							      displacement, #curves,
							      list(npj[[1]]$l.context, npj[[2]]$l.context, npj[[3]]$l.context),
							      list(npj[[1]]$mass, npj[[2]]$mass, npj[[3]]$mass),
							      op$SmoothingOneC)

				#TODO: calcular un SmothingOneECE i passar-lo a PlotOther enlloc del SmoothingOneC
				par(mfrow=c(1,1))


				#don't write the curves, write npj
				writeCurves = FALSE

				names_c <- curves$seriesName
				neuromuscularProfileWriteData1Person (npj, names_c, op$OutputData1)
			}
		}

                if(op$Analysis == "curves" || op$Analysis == "curvesAC" || op$Analysis == "curvesProcessMultiDB" || writeCurves)
		{
                        #create pafCurves to be printed on CSV. This columns are going to be removed:
                        
                        #print("---- 1 ----")
                        #print(paf)
                        
                        #write("paf and pafCurves", stderr())
                        #write(paf$meanSpeed, stderr())
                        pafCurves <- subset( paf, select = -c(mass, massBody, massExtra) )
                        #write(pafCurves$meanSpeed, stderr())
                        
                        #print("---- 2 ----")
                        #print(pafCurves)
                        
                        if(singleFile)
                                pafCurves = cbind(
                                        "1",			#seriesName
                                        "exerciseName",
                                        op$MassBody,
                                        op$MassExtra,
                                        curves[,1],
                                        curves[,2]-curves[,1],curvesHeight,pafCurves)
                        else {
                                if(discardingCurves)
                                        curvesHeight = curvesHeight[-discardedCurves]

				#by default use sessions as seriesNames
				mySeriesNames = curves[,9]

				#special config separate by days
				if(op$SeparateSessionInDays)
				{
					chunks = unlist(strsplit(curves[,7], " ")) #separate "2018-09-06 12:12:4" in two chunks
					chunks = chunks[seq(1, length(chunks), by = 2)] #this takes only odd elements
					print("chunks: ")
					print(chunks)
					mySeriesNames = chunks #mySeriesNames will be: "2018-09-06" (so different series will have the same series number and analyzed together)
				}

                                pafCurves = cbind(
                                        mySeriesNames,		#seriesName
                                        curves[,4],		#exerciseName
                                        curves[,5],		#massBody
                                        curves[,6],		#massExtra
                                        curves[,1],		
                                        curves[,2]-curves[,1],curvesHeight,pafCurves)
                                
                        }
                        
                        colnames(pafCurves) = c("series","exercise","massBody","massExtra",
                                                "start","width","height",
                                                "meanSpeed","maxSpeed","maxSpeedT",
                                                "meanPower","peakPower","peakPowerT",
                                                "pp_ppt",
                                                "meanForce", "maxForce", "maxForceT",
                                                "maxForce_maxForceT",
						"workJ", "impulse",
                                                "laterality", "inertiaM", "diameter",
						"equivalentMass"
                        )
                        
                        #Add "Max", "AVG" and "SD" when analyzing, not on "curves", not on "curvesAC", not on "curvesProcessMultiDB"
                        if(op$Analysis != "curves" && op$Analysis != "curvesAC" && op$Analysis != "curvesProcessMultiDB")
			{
                                addSD = FALSE
                                if(length(pafCurves[,1]) > 1)
                                        addSD = TRUE
                                
                                if(typeof(pafCurves) == "list") {
                                        #1) MAX
                                        pafCurvesMax = c("","", max(pafCurves$massBody), max(pafCurves$massExtra),
                                                         max(pafCurves$start),max(pafCurves$width),max(pafCurves$height),
                                                         max(pafCurves$meanSpeed),max(pafCurves$maxSpeed),max(pafCurves$maxSpeedT),
                                                         max(pafCurves$meanPower),max(pafCurves$peakPower),max(pafCurves$peakPowerT),
                                                         max(pafCurves$pp_ppt),
                                                         max(pafCurves$meanForce), max(pafCurves$maxForce), max(pafCurves$maxForceT),
                                                         max(pafCurves$maxForce_maxForceT),
							 max(pafCurves$workJ), max(pafCurves$impulse),
                                                         "", max(pafCurves$inertiaM), max(pafCurves$diameter), max(pafCurves$equivalentMass)
                                        )
                                        
                                        #2) AVG
                                        pafCurvesAVG = c("","", mean(pafCurves$massBody), mean(pafCurves$massExtra),
                                                         mean(pafCurves$start),mean(pafCurves$width),mean(pafCurves$height),
                                                         mean(pafCurves$meanSpeed),mean(pafCurves$maxSpeed),mean(pafCurves$maxSpeedT),
                                                         mean(pafCurves$meanPower),mean(pafCurves$peakPower),mean(pafCurves$peakPowerT),
                                                         mean(pafCurves$pp_ppt),
                                                         mean(pafCurves$meanForce), mean(pafCurves$maxForce), mean(pafCurves$maxForceT),
                                                         mean(pafCurves$maxForce_maxForceT),
							 mean(pafCurves$workJ), mean(pafCurves$impulse),
                                                         "", mean(pafCurves$inertiaM), mean(pafCurves$diameter), mean(pafCurves$equivalentMass)
                                        )
                                        
                                        #3) Add SD if there's more than one data row.
                                        if(addSD)
                                                pafCurvesSD = c("","", sd(pafCurves$massBody), sd(pafCurves$massExtra),
                                                                sd(pafCurves$start),sd(pafCurves$width),sd(pafCurves$height),
                                                                sd(pafCurves$meanSpeed),sd(pafCurves$maxSpeed),sd(pafCurves$maxSpeedT),
                                                                sd(pafCurves$meanPower),sd(pafCurves$peakPower),sd(pafCurves$peakPowerT),
                                                                sd(pafCurves$pp_ppt),
                                                                sd(pafCurves$meanForce), sd(pafCurves$maxForce), sd(pafCurves$maxForceT),
								sd(pafCurves$maxForce_maxForceT),
								sd(pafCurves$workJ), sd(pafCurves$impulse),
                                                                "", sd(pafCurves$inertiaM), sd(pafCurves$diameter), sd(pafCurves$equivalentMass)
                                                )
                                        
                                        
                                        pafCurves = rbind(pafCurves, pafCurvesMax)
                                        rownames(pafCurves)[length(pafCurves[,1])] = "MAX"
                                        
                                        pafCurves = rbind(pafCurves, pafCurvesAVG)
                                        rownames(pafCurves)[length(pafCurves[,1])] = "AVG"
                                        
                                        if(addSD) {
                                                pafCurves = rbind(pafCurves, pafCurvesSD)
                                                rownames(pafCurves)[length(pafCurves[,1])] = "SD"
                                        }
                                }
                        }
                        
                        #print("---- 3 ----")
                        #print(pafCurves)
                        
                        #write.csv(pafCurves, op$OutputData1, quote=FALSE)
			#we create pafCurvesWrite and mantain pafCurves because pafCurves is used on Pmax(F0,V0)
			pafCurvesWrite = pafCurves
                        if(! singleFile)
			{
				myTimestamps = curves[,7]
				if( length(pafCurvesWrite[,1]) > length(curves[,7]) )
					myTimestamps = c( myTimestamps, rep(NA, length(pafCurvesWrite[,1]) - length(curves[,7])) )

				if(op$AnalysisMode == "GROUPAL_CURRENT_SESSION")
					pafCurvesWrite[,1] = paste(pafCurves[,1], myTimestamps) #pafCurves[,1] is the name, needed on 4th analysis mode
				else
					pafCurvesWrite[,1] = myTimestamps
			}
                        #print("---- 4 ----")
                        #print(pafCurvesWrite)
			write.csv(pafCurvesWrite, op$OutputData1, quote=FALSE)
                        #print("curves written")
                }
        }

	#Pmax(F0,V0) will use pafCurves
        if(op$Analysis == "cross" && op$AnalysisVariables[1] == "Pmax(F0,V0)")
	{
		print("KKKKKKKK")
		print(pafCurves)
		pmaxArray = data.frame(pafCurves$series, as.numeric(pafCurves$meanSpeed), as.numeric(pafCurves$meanForce))
		colnames(pmaxArray) = c("date", "meanSpeed", "meanForce")
		pfvProfileExecute(pmaxArray)
	}
        
        if(op$Analysis=="1RMIndirect") {
                #Q <- getMassBodyByExercise(op$MassBody, op$ExercisePercentBodyWeight) + op$MassExtra
                Q <- op$MassExtra
                
                nrep <- length(curves[,1])
                if(op$Eccon != "c")
                        nrep = abs(nrep/2) #only concentric
                
                temp <- RMIndirect(Q, nrep)
                plotRMIndirect(temp, Q, nrep)
                
                write.csv(temp, op$OutputData1, quote=FALSE)
        }
        
        if(op$Analysis=="exportCSV") {
                print("Starting export...")
                write("starting export", stderr())
                
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
                units=c(" (mm)", " (mm)", " (m/s)", " (m/s^2)", " (N)", " (W)")
                namesNums=paste(namesNums, units)
                
                for(i in 1:curvesNum) { 
                        #exportCSV exports a signal, for this reason op$MassBody, op$MassExtra are ok. Don't need to check parameters of different signals
                        repOp <- assignRepOptions(
                                TRUE, NULL, NULL,
                                op$MassBody, op$MassExtra, op$Eccon, op$ExercisePercentBodyWeight,
                                op$EncoderConfigurationName,op$diameter,op$diameterExt,
                                op$anglePush,op$angleWeight,op$inertiaMomentum,op$gearedDown,
                                "") #laterality 
                        
                        kn <- kinematicsF(displacement[curves[i,1]:curves[i,2]],
                                          repOp, SmoothingsEC[i], op$SmoothingOneC, g, isPropulsive,
                                          FALSE,		#show all the repetition, not only ground phase on ecc
					  op$MinHeight
                        )
                        
                        #fill with NAs in order to have the same length
                        col1 = displacement[curves[i,1]:curves[i,2]]
                        col2 = position[curves[i,1]:curves[i,2]]
                        
                        #add absmean, mean, max, and time to max
                        col1=append(col1,
                                    c(NA,NA,NA,NA,NA),
                                    after=0)
                        col2=append(col2,
                                    c(NA,NA,NA,NA,range(col2)[2]-range(col2)[1]),
                                    after=0)
                        kn$speedy=append(kn$speedy,
                                         c(
                                                 mean(kn$speedy),
                                                 mean(abs(kn$speedy)),
                                                 max(kn$speedy),
                                                 (min(which(kn$speedy == max(kn$speedy)))/1000),
                                                 NA),
                                         after=0)
                        kn$accely=append(kn$accely,
                                         c(
                                                 mean(kn$accely),
                                                 mean(abs(kn$accely)),
                                                 max(kn$accely),
                                                 NA,
                                                 NA),
                                         after=0)
                        kn$force=append(kn$force,
                                        c(
                                                mean(kn$force),
                                                mean(abs(kn$force)),
                                                max(kn$force),
                                                NA,
                                                NA),
                                        after=0)
                        kn$power=append(kn$power,
                                        c(
                                                mean(kn$power),
                                                mean(abs(kn$power)),
                                                max(kn$power),
                                                (min(which(kn$power == max(kn$power)))/1000),
                                                NA),
                                        after=0)
                        
                        extraRows=5
                        length(col1) <- maxLength + extraRows
                        length(col2) <- maxLength + extraRows
                        length(kn$speedy) <- maxLength + extraRows
                        length(kn$accely) <- maxLength + extraRows
                        length(kn$force) <- maxLength + extraRows
                        length(kn$power) <- maxLength + extraRows
                        
                        if(i==1)
                                df=data.frame(cbind(col1, col2,
                                                    kn$speedy, kn$accely, kn$force, kn$power))
                        else
                                df=data.frame(cbind(df, col1, col2,
                                                    kn$speedy, kn$accely, kn$force, kn$power))
                }
                
                rownames(df) = c("MEAN", "MEAN (ABS)", "MAX", "TIME TO MAX", "RANGE", 1:maxLength)
                colnames(df) = namesNums
                
                #TODO: time
                #TODO: tenir en compte el startH
                
                #op$Title=gsub('_',' ',Top$itle)
                #print(op$Title)
                #titleColumns=unlist(strsplit(op$Title,'-'))
                #colnames(df)=c(titleColumns[1]," ", titleColumns[2],titleColumns[3],rep(" ",(curvesNum*curveCols-4)))
                
                if(op$DecimalSeparator == "COMMA")
                        write.csv2(df, file = op$OutputData1, row.names=T, na="")
                else
                        write.csv(df, file = op$OutputData1, row.names=T, na="")
                
                print("Export done.")
        }
        if(op$Analysis != "exportCSV")
                dev.off()
        
        #make some check here, because this file is being readed in chronojump
        #write(paste("(5/5)",translateToPrint("R tasks done")), op$FeedbackFileBase)
        print("Creating (op$FeedbackFileBase)6.txt with touch method...")
        file.create(paste(op$FeedbackFileBase,"6.txt",sep=""))
        write("created ...6.txt", stderr())
        
        warnings()
}


