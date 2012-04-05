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
#   Copyright (C) 2004-2012   Teng Wei Hua <wadedang@gmail.com>, Xavier de Blas <xaviblas@gmail.com> 
# 
# encoding=utf-8
#
# This problem is for reading data form Chronopic.



import serial
import sys
from datetime import datetime
from struct import unpack
from pyper import *


#import subprocess
#import pygame.image
#import pygame.display
import pygame
#from pygame.locals import * #mouse and key definitions


print(sys.argv)

FALSE = 0
TRUE = 1

# ============
# = Variable =
# ============
outputFile = sys.argv[1]
record_time = int(sys.argv[2])*1000		#from s to ms
minHeight = int(sys.argv[3])			#all is stored, but only display when vertical range is >= minHeight
isJump = sys.argv[4]
mass = float(sys.argv[5])
smoothingOne = float(sys.argv[6])
eccon = sys.argv[7]				#contraction "ec" or "c"
heightHigherCondition = int(sys.argv[8])
heightLowerCondition = int(sys.argv[9])
meanSpeedHigherCondition = int(sys.argv[10])
meanSpeedLowerCondition = int(sys.argv[11])
maxSpeedHigherCondition = int(sys.argv[12])
maxSpeedLowerCondition = int(sys.argv[13])
powerHigherCondition = int(sys.argv[14])
powerLowerCondition = int(sys.argv[15])
peakPowerHigherCondition = int(sys.argv[16])
peakPowerLowerCondition = int(sys.argv[17])

delete_initial_time = 20			#delete first records because there's encoder bug
#w_baudrate = 9600                           # Setting the baudrate of Chronopic(9600)
w_baudrate = 115200                           # Setting the baudrate of Chronopic(115200)
#w_serial_port = 4                           # Setting the serial port (Windows), windows's device number need minus 1
w_serial_port = "/dev/ttyUSB0"             # Setting the serial port (Linux)
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
frames_pull_top2 = list()
frames_push_bottom1 = list()
frames_push_bottom2 = list()
previous_frame_change = 0

lag=20



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


def calculate_all_in_r(temp, top_values, bottom_values, direction_now, smoothingOne, eccon, minHeight, isJump):
	if (len(top_values)>0 and len(bottom_values)>0):
		if direction_now == 1:
			start=top_values[len(top_values)-1]
			end=bottom_values[len(bottom_values)-1]
		else:
			start=bottom_values[len(bottom_values)-1]
			end=top_values[len(top_values)-1]
		
		myR.assign('smoothingOne',smoothingOne)
		myR.assign('a',temp[start:end])
		
		if direction_now == -1:
			myR.run('speed <- smooth.spline( 1:length(a), a, spar=smoothingOne)')

			#reduce curve by speed, the same way as graph.R
			myR.run('b=extrema(speed$y)')
			myR.run('maxSpeedT <- min(which(speed$y == max(speed$y)))')
			maxSpeedT = myR.get('maxSpeedT')
			bcrossLen = myR.get('length(b$cross[,2])')
			bcross = myR.get('b$cross[,2]')
			if bcrossLen == 0:
				return
			if bcrossLen == 1:
				x_ini = bcrossLen
			else:
				for i in bcross:
					if i < maxSpeedT:
						x_ini = i  #left adjust
			
			myR.assign('a',temp[start+x_ini:end])
	
		myR.run('speed <- smooth.spline( 1:length(a), a, spar=smoothingOne)')
		myR.run('a.cumsum <- cumsum(a)')
		myR.run('range <- abs(a.cumsum[length(a)]-a.cumsum[1])')
		myR.run('accel <- predict( speed, deriv=1 )')
		myR.run('accel$y <- accel$y * 1000') #input data is in mm, conversion to m
		if isJump == "True":
			myR.run('force <- mass*(accel$y+9.81)')
		else:
			myR.run('force <- mass*accel$y')
		myR.run('power <- force*speed$y')
		myR.run('meanPower <- mean(abs(power))')
		myR.run('peakPower <- max(power)')
		myR.run('peakPowerT=which(power == peakPower)')
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
		peakPowerT = myR.get('peakPowerT')

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
		#if only one param is bad, will sound bad
		if colorHeight == RED or colorMeanSpeed == RED or colorMaxSpeed == RED or colorMeanPower == RED or colorPeakPower == RED:
			play = True
			soundFile = soundFileBad
		elif colorHeight == GREEN or colorMeanSpeed == GREEN or colorMaxSpeed == GREEN or colorMeanPower == GREEN or colorPeakPower == GREEN:
			play = True
			soundFile = soundFileGood

		if eccon == "ec" or direction_now == -1:
			if height >= minHeight:
				print phaseCol + colorize(heightF,colorHeight,colorHeight!=BLACK) + colorize(meanSpeedF,colorMeanSpeed,colorMeanSpeed!=BLACK) + colorize(maxSpeedF,colorMaxSpeed,colorMaxSpeed!=BLACK) + colorize(meanPowerF,colorMeanPower,colorMeanPower!=BLACK) + colorize(peakPowerF,colorPeakPower,colorPeakPower!=BLACK) + "%11i" % peakPowerT
				if play:
					playsound(soundFile)
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

#def update_graph(xmin, xmax):
#	subprocess.Popen([r"Rscript","graph.R",str(xmin),str(xmax)]).wait()
#	picture = pygame.image.load("graph.png")
#	pygame.display.set_mode(picture.get_size())
#	main_surface= pygame.display.get_surface()
#	main_surface.blit(picture, (0,0))
#	pygame.display.update()


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

	file = open(outputFile, 'w')

	ser = serial.Serial(w_serial_port)
	ser.baudrate = w_baudrate
	temp = list()		#raw values
	temp_cumsum = list()	#cumulative sums of raw values
	temp_cumsum.append(0)
	temp_speed = list()
	w_time = datetime.now().second
	#print "start read data"
	# Detecting if serial port is available and Recording the data from Chronopic.
	for i in xrange(delete_initial_time):
		#if ser.readable(): #commented because don't work on linux
		ser.read()

	print("START!\n")
	playsound(soundFileStart)
	print("phase, range, meanSpeed, maxSpeed, meanPower, peakPower, peakPowerT")
	for i in xrange(record_time):
		#if ser.readable(): #commented because don't work on linux
		byte_data = ser.read()
		# conver HEX to INT value
		signedChar_data = unpack('b' * len(byte_data), byte_data)[0]
    
		temp.append(signedChar_data)
		if(i>0):
			temp_cumsum.append(temp_cumsum[i-1]+signedChar_data)
		if(i>lag):
			temp_speed.append(1.0*(temp_cumsum[i]-temp_cumsum[i-lag])/lag)
		else:
			temp_speed.append(0)

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
	
				phase = 0
				speed = 0
	
				if direction_now == 1:
					#we are going up, we passed the ditection_change_count
					#then we can record the bottom moment
					#and print speed on going down
					new_frame_change = previous_frame_change+k.index(min(k)) 
					frames_push_bottom1.append(new_frame_change)
					new_frame_change = previous_frame_change+len(k)-1-k[::-1].index(min(k))
					frames_push_bottom2.append(new_frame_change)
					phase = " down"
					if previous_frame_change != 0 and new_frame_change != 0:
						speed = min(temp_speed[previous_frame_change:new_frame_change])
				else:
					new_frame_change = previous_frame_change+k.index(max(k))
					frames_pull_top1.append(new_frame_change)
					new_frame_change = previous_frame_change+len(k)-1-k[::-1].index(max(k))
					frames_pull_top2.append(new_frame_change)
					phase = "   up"
					if previous_frame_change != 0 and new_frame_change != 0:
						speed = max(temp_speed[previous_frame_change:new_frame_change])
	

				if len(frames_pull_top1)>0 and len(frames_push_bottom1)>0:
					calculate_all_in_r(temp, frames_pull_top1, frames_push_bottom1, 
							direction_now, smoothingOne, eccon, minHeight, isJump)
					
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

#except:
#	print "aarrgggggh!!"
#	print sys.exc_info()
#	ser.close()
#	file = open("data.txt", 'w')
#	for i in temp:
#		file.write(i)
#	file.close()
