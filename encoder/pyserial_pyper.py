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
#   Copyright (C) 2004-2014   Teng Wei Hua <wadedang@gmail.com>, Xavier de Blas <xaviblas@gmail.com> 
# 
# encoding=utf-8
#
# This program is for reading data from Chronopic.

import serial
import sys
from datetime import datetime
from struct import unpack
from pyper import *


#import subprocess
#import pygame.image
#import pygame.display
import pygame
from pygame.locals import * #mouse and key definitions


print(sys.argv)

FALSE = 0
TRUE = 1

# ============
# = Variable =
# ============
title = sys.argv[1]
outputFile = sys.argv[2]
record_time = int(sys.argv[3])*1000		#from s to ms
minRange = int(sys.argv[4])			#all is stored, but only display when vertical range is >= minRange
isJump = sys.argv[5]
mass = float(sys.argv[6])
smoothingOneEC = .6 #float(sys.argv[7])
smoothingOneC = float(sys.argv[7])
eccon = sys.argv[8]				#contraction "ec" or "c"
analysisOptions = sys.argv[9]			#["p","none"]: propulsive or none
heightHigherCondition = int(sys.argv[10])
heightLowerCondition = int(sys.argv[11])
meanSpeedHigherCondition = float(sys.argv[12])
meanSpeedLowerCondition = float(sys.argv[13])
maxSpeedHigherCondition = float(sys.argv[14])
maxSpeedLowerCondition = float(sys.argv[15])
powerHigherCondition = int(sys.argv[16])
powerLowerCondition = int(sys.argv[17])
peakPowerHigherCondition = int(sys.argv[18])
peakPowerLowerCondition = int(sys.argv[19])
mainVariable = sys.argv[20]
inverted = sys.argv[21]
w_serial_port = sys.argv[22]

delete_initial_time = 20			#delete first records because there's encoder bug
#w_baudrate = 9600                           # Setting the baudrate of Chronopic(9600)
w_baudrate = 115200                           # Setting the baudrate of Chronopic(115200)
#w_serial_port = 4                           # Setting the serial port (Windows), windows's device number need minus 1
#w_serial_port = "/dev/ttyUSB0"             # Setting the serial port (Linux)
direction_change_period = 25                # how long to recognize as change direction. (I am not sure if this describe correctly.)
direction_change_count = 0
direction_pull_to_push = 'pull_to_push'     # if people change the motion for pull to push, record this value.
direction_push_to_pull = 'push_to_pull'     # if people change the motion for push to pull, record this value.
direction_now = 1			# 1 or -1
direction_last_ms = 1			# 1 or -1
direction_completed = -1		# 1 or -1

#TODO: do also something about don't accept a phase that has less tha X cm of distance.
#This will be useful to know the start of movement

frames_pull_top1 = list()
frames_push_bottom1 = list()
previous_frame_change = 0

mode = "graph"
#mode = "text"

#https://wiki.archlinux.org/index.php/Color_Bash_Prompt#Prompt_escapes


#sound stuff
#http://code.activestate.com/recipes/521884-play-sound-files-with-pygame-in-a-cross-platform-m/
# global constants
#FREQ = 44100   # same as audio CD
FREQ = 8000   # same as audio CD
BITSIZE = -16  # unsigned 16 bit
#CHANNELS = 2   # 1 == mono, 2 == stereo
CHANNELS = 1   # 1 == mono, 2 == stereo
BUFFER = 1024  # audio buffer size in no. of samples
FRAMERATE = 30 # how often to check if playback has finished
def playsound(soundfile):
    sound = pygame.mixer.Sound(soundfile)
    clock = pygame.time.Clock()
    sound.play()
    while pygame.mixer.get_busy():
        clock.tick(FRAMERATE)

soundFileStart = "/home/xavier/informatica/progs_meus/chronojump/chronojump/encoder/Question.wav"
soundFileGood = "/home/xavier/informatica/progs_meus/chronojump/chronojump/encoder/Asterisk.wav"
#soundFileBad = "/home/xavier/informatica/progs_meus/chronojump/chronojump/encoder/Beep.wav"
soundFileBad = "/home/xavier/informatica/progs_meus/chronojump/chronojump/encoder/Hand.wav"


BLACK = 30
RED = 31
GREEN = 32
BLUE = 34
REDINV = 41
GREENINV = 42
BLUEINV = 44

def colorize(text, color, bold):
	ESCAPE = '%s[' % chr(27)
	RESET = '%s0m' % ESCAPE
	if(bold):
		FORMAT = '1;%dm'
	else:
		FORMAT = '0;%dm'
	return ESCAPE + (FORMAT % (color, )) + text + RESET

def assignColor(found, conditionHigher, conditionLower):
	if conditionHigher != -1 and found >= conditionHigher:
		return GREEN
	elif conditionLower != -1 and found <= conditionLower:
		return RED
	else:
		return BLACK

rangeList = list()
meanSpeedList = list()
maxSpeedList = list()
meanPowerList = list()
peakPowerList = list()
def calculate_all_in_r(temp, top_values, bottom_values, direction_now,
		smoothingOneEC, smoothingOneC, eccon, minRange, isJump):
	if (len(top_values)>0 and len(bottom_values)>0):
		if direction_now == 1:
			start=top_values[len(top_values)-1]
			end=bottom_values[len(bottom_values)-1]
		else:
			start=bottom_values[len(bottom_values)-1]
			end=top_values[len(top_values)-1]
		
		if eccon == "c":
			myR.assign('smoothingOne',smoothingOneC)
		else:
			myR.assign('smoothingOne',smoothingOneEC)

		myR.assign('a',temp[start:end])
		
		#declare
		x_ini = 0	
		
		#this will have same values than graph.R:
		#1) first remove at start with reduceCurveBySpeed
		#2) do speed and accel for curve once reducedCurveBySpeed
		#3) find propulsive (if appropiate)
		#4) remove at end with propulsive (if appropiate)

		#1) first remove at start with reduceCurveBySpeed
		if direction_now == -1:
			#some variables only used for cutting
			myR.run('speedCut <- smooth.spline( 1:length(a), a, spar=smoothingOne)')


			#reduce curve by speed, the same way as graph.R
			myR.run('b=extrema(speedCut$y)')
			myR.run('maxSpeedT <- min(which(speedCut$y == max(speedCut$y)))')
			maxSpeedT = myR.get('maxSpeedT')
			bcrossLen = myR.get('length(b$cross[,2])')
			bcross = myR.get('b$cross[,2]')
			
			#debug
			b = myR.get('b')
			print("printing bbbbbbbbbbbb")
			print(b)
			print("printing bbbbbbbbbbbb cross")
			print(bcross)
			print("printing bbbbbbbbbbbb maxSpeedT")
			print(maxSpeedT)

			if bcrossLen == 0:
				x_ini = 0
			if bcrossLen == 1:
				if bcross < maxSpeedT:
					x_ini = bcross	#if bcross has only one item, then this fails: 'bcross[0]'. Just do 'bcross'
			else:
				x_ini = bcross[0]	#not 1, we are in python now
				for i in bcross:
					if i < maxSpeedT:
						x_ini = i  #left adjust
			
			myR.assign('a',temp[(start+x_ini):end])
			#parenthesis [( )] were totally needed in this operation!!
			#eg:
			#k=seq(1:100)
			#> kwoParentheses = k[10+10:90]
			#> length(kwoParentheses)
			#[1] 81
			#> fivenum(kwoParentheses)
			#[1]  20  40  60  80 100
			
			#> kwParentheses = k[(10+10):90]
			#> length(kwParentheses)
			#[1] 71
			#> fivenum(kwParentheses)
			#[1] 20.0 37.5 55.0 72.5 90.0



		#2) do speed and accel for curve once reducedCurveBySpeed
		myR.run('speed <- smooth.spline( 1:length(a), a, spar=smoothingOne)')
		myR.run('accel <- predict( speed, deriv=1 )')

	
		myR.run('a.cumsum <- cumsum(a)')
		myR.run('range <- abs(a.cumsum[length(a)]-a.cumsum[1])')


		#3) find propulsive (if appropiate)
		if direction_now == -1 and analysisOptions == "p":
			myR.run('accelCut <- predict( speed, deriv=1 )')
			myR.run('accelCut$y <- accelCut$y * 1000') #input data is in mm, conversion to m
			myR.run('propulsiveData <- which(accelCut$y <= -9.81)') 
			propulsiveData = myR.get('propulsiveData') 
			propulsiveDataLength = myR.get('length(propulsiveData)') 
			if propulsiveDataLength > 0:
				end = min(propulsiveData)

			#myR.assign('x_ini',x_ini)
			myR.assign('end',end)

			#4) remove at end with propulsive (if appropiate)
			myR.run('speed$y <- speed$y[1:end]')
			myR.run('accel$y <- accel$y[1:end]')

		
#		print("new curve")
#		print(start+x_ini)
#		myspeedy=myR.get('speed$y')
#		print(myspeedy)
#
#

		myR.run('accel$y <- accel$y * 1000') #input data is in mm, conversion to m
#		if isJump == "True":
		myR.run('force <- mass*(accel$y+9.81)')
#		else:
#			myR.run('force <- mass*accel$y')
		myR.run('power <- force*speed$y')

		if eccon == "c":
			myR.run('meanPower <- mean(power)')
		else:
			myR.run('meanPower <- mean(abs(power))')
		
		myR.run('peakPower <- max(power)')
		
		#without the 'min', if there's more than one value it returns a list and this make crash later in
		#this code:  pp_ppt = peakPower / peakPowerT
		myR.run('peakPowerT=min(which(power == peakPower))') 

		meanSpeed = myR.get('mean(abs(speed$y))')
		if direction_now == 1:
			maxSpeed = myR.get('min(speed$y)')
			phase = " down,"
			phaseCol = colorize(phase,BLACK,FALSE)
		else:
			maxSpeed = myR.get('max(speed$y)')
			phase = "   up,"
			phaseCol = colorize(phase,BLUE,TRUE)
		height = myR.get('range')
		meanPower = myR.get('meanPower')
		peakPower = myR.get('peakPower')
		peakPowerT = myR.get('peakPowerT/1000') #ms -> s
		pp_ppt = peakPower / peakPowerT

		meanSpeedCol = "%10.2f," % meanSpeed

#		if(meanSpeed > 2): colSpeed = GREENINV
#		else: colSpeed = GREEN
		
		height = height / 10 #from cm to mm
		
		#F mean Formatted
		heightF = "%6i," % height
		colorHeight = assignColor(height, heightHigherCondition, heightLowerCondition)
		
		meanSpeedF = "%10.2f," % meanSpeed
		colorMeanSpeed = assignColor(meanSpeed, meanSpeedHigherCondition, meanSpeedLowerCondition)
		
		maxSpeedF = "%10.2f," % maxSpeed
		colorMaxSpeed = assignColor(maxSpeed, maxSpeedHigherCondition, maxSpeedLowerCondition)
		
		meanPowerF = "%10.2f," % meanPower
		colorMeanPower = assignColor(meanPower, powerHigherCondition, powerLowerCondition)

		peakPowerF = "%10.2f," % peakPower
		colorPeakPower = assignColor(peakPower, peakPowerHigherCondition, peakPowerLowerCondition)

		play = False
		#if only one param is bad, will play soundFileBad
		if colorHeight == RED or colorMeanSpeed == RED or colorMaxSpeed == RED or colorMeanPower == RED or colorPeakPower == RED:
			play = True
			soundFile = soundFileBad
		elif colorHeight == GREEN or colorMeanSpeed == GREEN or colorMaxSpeed == GREEN or colorMeanPower == GREEN or colorPeakPower == GREEN:
			play = True
			soundFile = soundFileGood

		if eccon == "ec" or direction_now == -1:
			if height >= minRange:
				#print phaseCol + colorize(heightF,colorHeight,colorHeight!=BLACK) + colorize(meanSpeedF,colorMeanSpeed,colorMeanSpeed!=BLACK) + colorize(maxSpeedF,colorMaxSpeed,colorMaxSpeed!=BLACK) + colorize(meanPowerF,colorMeanPower,colorMeanPower!=BLACK) + colorize(peakPowerF,colorPeakPower,colorPeakPower!=BLACK) + "%10.2f" % peakPowerT  + "%10.2f" % pp_ppt 
				print phaseCol + colorize(heightF,colorHeight,colorHeight!=BLACK) + colorize(meanSpeedF,colorMeanSpeed,colorMeanSpeed!=BLACK) + colorize(maxSpeedF,colorMaxSpeed,colorMaxSpeed!=BLACK) + colorize(meanPowerF,colorMeanPower,colorMeanPower!=BLACK) + colorize(peakPowerF,colorPeakPower,colorPeakPower!=BLACK) + "%10.2f" % peakPowerT
				if play:
					playsound(soundFile)

				rangeList.append(height)
				meanSpeedList.append(meanSpeed)
				maxSpeedList.append(maxSpeed)
				meanPowerList.append(meanPower)
				peakPowerList.append(peakPower)
				
				graphsWidth = 792 #800-4-4
				hasRightMargin = True
				if (	heightLowerCondition == -1 and heightHigherCondition == -1 and
					powerLowerCondition == -1 and powerHigherCondition == -1
				   ):
					hasRightMargin = False

				update_graph("Range (cm)", rangeList, 
						heightLowerCondition, heightHigherCondition, hasRightMargin,
						graphsWidth, 112, (222,222,222), 4, 40, False)
				#vertical_height: 112, position it at 40 pixels vert

				#position it at 40+112+4 pixels vert: 156
				#vertical_height: 600 -4 (lower sep) - 156 : 440
				if mainVariable == "Mean Speed":
					update_graph("Mean Speed (m/s)", meanSpeedList, 
							meanSpeedLowerCondition, meanSpeedHigherCondition, hasRightMargin,
							graphsWidth, 440, (222,222,222), 4, 156, True)
				elif mainVariable == "Max Speed":
					update_graph("Max Speed (m/s)", maxSpeedList, 
							maxSpeedLowerCondition, maxSpeedHigherCondition, hasRightMargin,
							graphsWidth, 440, (222,222,222), 4, 156, True)
				elif mainVariable == "Mean Power":
					update_graph("Mean Power (W)", meanPowerList, 
							powerLowerCondition, powerHigherCondition, hasRightMargin,
							graphsWidth, 440, (222,222,222), 4, 156, False)
				else: #mainVariable == "Peak Power"
					update_graph("Peak Power (W)", peakPowerList, 
							peakPowerLowerCondition, peakPowerHigherCondition, hasRightMargin,
							graphsWidth, 440, (222,222,222), 4, 156, False)

			else:
				print chr(27) + "[0;47m" + phase + "%6i," % height + " " + "Discarded" + chr(27)+"[0m"



def calculate_range(temp_cumsum, top_values, bottom_values, direction_now):
	if len(top_values)>0:
		rmax=temp_cumsum[top_values[len(top_values)-1]]
		if len(bottom_values)>0:
			rmin=temp_cumsum[bottom_values[len(bottom_values)-1]]
		else:
			rmin=0
##		if(direction_now == 1): text="rangedown"
##		else: text="rangeup"
#		print(text,rmax-rmin)
		return(rmax-rmin)



def update_graph(paramName, paramList, lowCondition, highCondition, hasRightMargin,
		my_surface_width, my_surface_height, color, horizPosToCopy, vertPosToCopy, hasDecimals):
	s=pygame.Surface((my_surface_width,my_surface_height))

	s.fill(ColorBackground) #color the surface

	left_margin = 10
	right_margin = 0
	if hasRightMargin:
		right_margin = 40

	vert_margin = 40
	sep=20		#between reps
	sep_small=2	#between bars
	count = 0

	paramMax = max(paramList)
	if lowCondition > paramMax:
		paramMax = lowCondition
	if highCondition > paramMax:
		paramMax = highCondition

	param_low_height = my_surface_height - ((my_surface_height -vert_margin ) * lowCondition / paramMax)
	param_high_height = my_surface_height - ((my_surface_height -vert_margin ) * highCondition / paramMax)
	pygame.draw.line(s, (255,0,0), (left_margin, param_low_height), (my_surface_width-right_margin, param_low_height), 2)
	pygame.draw.line(s, (0,255,0), (left_margin, param_high_height), (my_surface_width-right_margin, param_high_height), 2)
	if lowCondition > 0:
		string = "%s" % lowCondition
		text = FontSmall.render(string,1, ColorBad, ColorBackground)
		textpos = text.get_rect(right=my_surface_width-10,centery=param_low_height)
		s.blit(text,textpos)
	if highCondition > 0:
		string = "%s" % highCondition
		text = FontSmall.render(string,1, ColorGood, ColorBackground)
		textpos = text.get_rect(right=my_surface_width-10,centery=param_high_height)
		s.blit(text,textpos)

	for param in paramList:
		if len(paramList) >= 10:
			sep = 10
		
		param_height = ( my_surface_height - vert_margin ) * param / paramMax 
		if len(paramList) == 1:
			width = (my_surface_width - left_margin - right_margin) / 2 #do not fill all the screen with only one bar
		else:
			width = (my_surface_width - left_margin - right_margin) / len(paramList)
		
		colorNow = color
		if param < lowCondition:
			colorNow = ColorBad
		elif highCondition > 0 and param > highCondition:
			colorNow = ColorGood
			
		left = left_margin + width*count
		param_width = width - sep
		pygame.draw.rect(s, colorNow, (left, my_surface_height, param_width, -param_height), 0) #0: filled

		if hasDecimals:
			string = "%.2f" % param
		else:
			string = "%i" % param

		text = FontBig.render(string,1,color, ColorBackground)
		if len(paramList) > 20:
			text = FontSmall.render(string,1,color, ColorBackground)

		textpos = text.get_rect(centerx=left+(param_width/2), centery=my_surface_height-param_height-vert_margin/2)
	       	s.blit(text,textpos)
		
		count = count +1
	
	string = "%s" % paramName
	text = FontBig.render(string,1, color, ColorBackground)
	textpos = text.get_rect(left=10,centery=10)
	s.blit(text,textpos)

	s_rect=s.get_rect() #get the rectangle bounds for the surface
        screen.blit(s,(horizPosToCopy,vertPosToCopy)) #render the surface into the rectangle
	pygame.display.flip() #update the screen

#option can be "start", "end",
#or time left: "5 s", "4 s", ..
def printHeader(option):
	s=pygame.Surface((792,32))
	s.fill(ColorBackground) #color the surface
	
	string = "%s" % title
	text = FontBig.render(string,1, (255,255,255))
	textpos = text.get_rect(left=10,centery=14)
	s.blit(text,textpos)

	if option == "start":
		string = "Start!"
		text = FontBig.render(string,1, (255,91,0))
	elif option == "end":
		string = "Done! Please close this window."
		text = FontBig.render(string,1, (255,91,0))
	else:
		string = option
		text = FontBig.render(string,1, (255,91,0))

	textpos = text.get_rect(right=792-10,centery=14)
	s.blit(text,textpos)

        screen.blit(s,(4,4)) #render the surface into the rectangle
	pygame.display.flip() #update the screen


# ================
# = Main Problem =
# ================

#try:
if __name__ == '__main__':
	print("Please, wait...\n")
	# initialize pygame.mixer module
	# if these setting do not work with your audio system
	# change the global constants accordingly
	try:
		pygame.mixer.init(FREQ, BITSIZE, CHANNELS, BUFFER)
	except pygame.error, exc:
		print >>sys.stderr, "Could not initialize sound system: %s" % exc
	
	#print "connecting with R"
	myR = R()
	myR.run('library("EMD")') #needed on reducing curve by speed (extrema)
	myR.assign('mass',mass)
	myR.run('weight=mass*9.81')
	myR.assign('k',2)

	print("START!\n")
	playsound(soundFileStart)
	print("phase, range, meanSpeed, MaxSpeed, meanPower, PeakPower, PeakPowerT")#, PPower/PPT")


	pygame.font.init
	pygame.init()
	screen = pygame.display.set_mode((800,600)) #make window
	pygame.display.set_caption("Chronojump encoder")
	
	FontBig = pygame.font.Font(None, 22)
	FontSmall = pygame.font.Font(None, 18)
	
	ColorBackground = (30,30,30)
	ColorBad = (255,0,0)
	ColorGood = (0,255,0)
	
	#start capture	
	file = open(outputFile, 'w')
	ser = serial.Serial(w_serial_port)
	ser.baudrate = w_baudrate
	temp = list()		#raw values
	temp_cumsum = list()	#cumulative sums of raw values
	temp_cumsum.append(0)
	w_time = datetime.now().second
	print "start read data"
	# Detecting if serial port is available and Recording the data from Chronopic.
	for i in xrange(delete_initial_time):
		#if ser.readable(): #commented because don't work on linux
		ser.read()
	
	#print title
	title = title.replace('_',' ')
	title = title.replace('-',' ')
	printHeader("start")

	secondsLeft = int(record_time / 1000)
	msCount = 0

	userStops = FALSE
	for i in xrange(record_time):
		for event in pygame.event.get():
			if event.type == pygame.QUIT or (event.type == KEYUP and event.key == K_ESCAPE):
				userStops = TRUE
		
		if userStops:
			print "USER BREAKS"
			break

		#if ser.readable(): #commented because don't work on linux
		byte_data = ser.read()

		# conver HEX to INT value
		signedChar_data = unpack('b' * len(byte_data), byte_data)[0]
		
		# invert sign if inverted is selected
		if inverted == 1:
			signedChard_data *= -1

		temp.append(signedChar_data)
		if(i>0):
			temp_cumsum.append(temp_cumsum[i-1]+signedChar_data)
		
		msCount = msCount +1
		if msCount == 1000 :
			secondsLeft = secondsLeft -1
			printHeader(`secondsLeft` + " s")
			msCount = 1

		# Judging if direction has changed
		if signedChar_data != 0:
			direction_now = signedChar_data / abs(signedChar_data) #1 (pull) or -1 (push)
		if direction_now != direction_last_ms:
			direction_last_ms = direction_now
			direction_change_count = 0
		elif direction_now != direction_completed: 
			#we cannot addd signedChar_data because then is difficult to come back n frames to know the max point
			#direction_change_count = direction_change_count + signedChar_data
			direction_change_count = direction_change_count + 1
			if direction_change_count >= direction_change_period:

				k=list(temp_cumsum[previous_frame_change:i-direction_change_period])
	
				if direction_now == 1:
					#we are going up, we passed the direction_change_count
					#then we can record the bottom moment
					#and print speed on going down (Not done anymore)
					
					#this has (maybe) 0,-1,0,0,0,0,0, .... (and -1 can be selected (min(k)).
					#Then, do not pass this to frames_push_bottom, pass the next new_frame_change
					
					new_frame_change = previous_frame_change+k.index(min(k))
					print("NFC 1 1 (start zeros on the bottom) %i" % new_frame_change) 
					
					frames_push_bottom1.append(new_frame_change)

					new_frame_change = previous_frame_change+len(k)-1-k[::-1].index(min(k))
					print("NFC 1 2 (end zeros on the bottom) %i" % new_frame_change) 
					
				else:
					new_frame_change = previous_frame_change+k.index(max(k))
					print("NFC 2 1 (start zeros on the top) %i" % new_frame_change) 
					
					frames_pull_top1.append(new_frame_change)
					
					new_frame_change = previous_frame_change+len(k)-1-k[::-1].index(max(k))
					print("NFC 2 2 (end zeros on the top) %i" % new_frame_change) 
					

				if len(frames_pull_top1)>0 and len(frames_push_bottom1)>0:
					calculate_all_in_r(temp, frames_pull_top1, frames_push_bottom1, 
							direction_now, smoothingOneEC, smoothingOneC, eccon, minRange, isJump)
					
				file.write(''+','.join([str(i) for i in temp[
					previous_frame_change:new_frame_change
					]])+'')
	            		file.write("\n")
	               	        file.flush()
				
				previous_frame_change = new_frame_change
				direction_change_count = 0
	                        direction_completed = direction_now
                        
	w_time = datetime.now().second - w_time
	ser.close()


	#TODO: arreglar aixo per a que el darrer calculate_range mostri el ultim tram (encara que no hagi acabat)
	if direction_completed == -1:
		calculate_range(temp_cumsum, frames_pull_top1, temp_cumsum, 1)
	else:
		calculate_range(temp_cumsum, frames_push_bottom1, temp_cumsum, -1)
    
	file.write(''+','.join([str(i) for i in temp[previous_frame_change:len(temp)]])+'')
	file.flush()
	file.close()


#	print "creating graph..."
#	update_graph(0,record_time)
#
#	done = 0
#	mouse_pos=[0,0]
#	while not done:
#		for e in pygame.event.get():
#			if e.type == QUIT or (e.type == KEYUP and e.key == K_ESCAPE):
#				done = 1
#				break
#			elif e.type == MOUSEBUTTONDOWN and e.button == 1:
#				mouse_pos[:] = list(e.pos)
#				print mouse_pos[0]
#				mouse_pos[0] = mouse_pos[0]*record_time/1020 #mouse_pos scaled to signal width
#				print mouse_pos[0]
#				zoom_width=record_time/2
#				xmin=mouse_pos[0]-zoom_width/2
#				xmax=mouse_pos[0]+zoom_width/2
#				#TODO: do all this in R in an interactive way
#				#TODO: other click makes zoom in more
#				#TODO: or better, point with mouse and press 2,4,8 to zoom x times
#				if xmin < 0:
#					xmax += -xmin
#					xmin += -xmin
#				elif xmax > record_time:
#					xmin -= xmax-record_time
#					xmax -= xmax-record_time
#					
#				update_graph(xmin,xmax)
#			elif e.type == MOUSEBUTTONDOWN and (e.button == 2 or e.button == 3):
#				update_graph(0,record_time)


	print "\nDone! Please, close this window."
	printHeader("end")
	
	
	while 1:
		for event in pygame.event.get():
			if event.type == pygame.QUIT or (event.type == KEYUP and event.key == K_ESCAPE):
				sys.exit()

		pygame.time.delay(30)
		pygame.display.flip() #update the screen
		#TODO: http://stackoverflow.com/questions/10466590/hiding-pygame-display
		


#except:
#	print "aarrgggggh!!"
#	print sys.exc_info()
#	ser.close()
#	file = open("data.txt", 'w')
#	for i in temp:
#		file.write(i)
#	file.close()
