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
#   Copyright (C) 2024 Xavier de Blas <xaviblas@gmail.com>
# 
# based on pyserial_pyper.py

#PYTHON PACKAGES (pip3 install ...)
#pyserial
#pygame

#RUNNING
#python3 pyserial_manipulaso.py title 10 10 /dev/ttyUSB0 hola

import serial
#from serial import Serial
import sys
from datetime import datetime
from struct import unpack
import pygame
from pygame.locals import * #mouse and key definitions


print(sys.argv)

FALSE = 0
TRUE = 1

# list for both encoders
enc_l = [ #encoder list
        {
            'name':'left',
            'port':'',                          # serial port
#            'ser':0, #cannot put this in dictionary
            'dir_change_count':0,
            'dir_pull_to_push':'pull_to_push',     # if people change the motion for pull to push, record this value.
            'dir_push_to_pull':'push_to_pull',     # if people change the motion for push to pull, record this value.
            'dir_now':1,		# 1 or -1
            'dir_last_ms':1,			# 1 or -1
            'dir_completed':-1,		# 1 or -1
            'frames_pull_top1':list(),
            'frames_push_bottom1':list(),
            'previous_frame_change':0,
            'temp':list (),
            'temp_cumsum':0,
            'w_time':0
#            },
#        {
#            'name':'right',
#            'port':'',
##            'ser':0, #cannot put this in dictionary
#            'dir_change_count':0,
#            'dir_pull_to_push':'pull_to_push',     # if people change the motion for pull to push, record this value.
#            'dir_push_to_pull':'push_to_pull',     # if people change the motion for push to pull, record this value.
#            'dir_now':1,			# 1 or -1
#            'dir_last_ms':1,			# 1 or -1
#            'dir_completed':-1,		# 1 or -1
#            'frames_pull_top1':list(),
#            'frames_push_bottom1':list(),
#            'previous_frame_change':0,
#            'temp':list (),
#            'temp_cumsum':0,
#            'w_time':0
            }
        ]

# ============
# = Variable =
# ============
title = sys.argv[1]
record_time = int(sys.argv[2])*1000		#from s to ms
minRange = int(sys.argv[3])
enc_l[0]['port'] = sys.argv[4] #left encoder
#enc_l[1]['port'] = sys.argv[5] #right encoder

delete_initial_time = 20			#delete first records because there's encoder bug
#w_baudrate = 9600                           # Setting the baudrate of Chronopic(9600)
w_baudrate = 115200                           # Setting the baudrate of Chronopic(115200)
dir_change_period = 25                # how long to recognize as change direction.

serL = 0
serR = 0

mode = "graph"
graphsWidth = 792 #800-4-4
updateGraphAtMs = 25


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

#disabled clock calls to go faster
#more info on playsound here https://pythonprogramming.net/adding-sounds-music-pygame/
def playsound(soundfile):
    sound = pygame.mixer.Sound(soundfile)
    #clock = pygame.time.Clock()
    sound.play()
    #while pygame.mixer.get_busy():
    #    clock.tick(FRAMERATE)

soundFileStart = "/home/xavier/informatica/progs_meus/chronojump/encoder/Question.wav"
soundFileGood = "/home/xavier/informatica/progs_meus/chronojump/encoder/Asterisk.wav"
#soundFileBad = "/home/xavier/informatica/progs_meus/chronojump/encoder/Beep.wav"
soundFileBad = "/home/xavier/informatica/progs_meus/chronojump/encoder/Hand.wav"

#BLACK = 30
#RED = 31
#GREEN = 32
#BLUE = 34
#REDINV = 41
#GREENINV = 42
#BLUEINV = 44
#
#def colorize(text, color, bold):
#    ESCAPE = '%s[' % chr(27)
#    RESET = '%s0m' % ESCAPE
#    if(bold):
#        FORMAT = '1;%dm'
#    else:
#        FORMAT = '0;%dm'
#    return ESCAPE + (FORMAT % (color, )) + text + RESET
#
#def assignColor(found, conditionHigher, conditionLower):
#    if conditionHigher != -1 and found >= conditionHigher:
#        return GREEN
#    elif conditionLower != -1 and found <= conditionLower:
#        return RED
#    else:
#        return BLACK

def update_graph(posL, posR, my_s_width, my_s_height, color, horizPosToCopy, vertPosToCopy, hasDecimals):
    s=pygame.Surface((my_s_width,my_s_height))
    
    s.fill(ColorBackground) #color the surface

    left_margin = 20
    right_margin = 20
    vert_margin = 40
    sep=20		#between bars
            
    #barMax = max(paramList)
    barMax = 1000
    #if posL > barMax:
    #    barMax = posL * 1.2
    #if posR > barMax:
    #    barMax = posR * 1.2

    bar_heightL = (my_s_height -vert_margin) * posL / barMax 
    bar_heightR = (my_s_height -vert_margin) * posR / barMax 
    bar_width = (my_s_width -left_margin -right_margin -sep) / 3 #each bar 1/3 of screen

    colorNow = color

    left = left_margin + bar_width
    pygame.draw.rect(s, colorNow,
            (left_margin, my_s_height -bar_heightL,
                bar_width, bar_heightR)
            , 0) #0: filled
    pygame.draw.rect(s, (0, 100, 0),
            (my_s_width - right_margin -bar_width, my_s_height -bar_heightR,
                bar_width, bar_heightR),
            0) #0: filled
    
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
# = Main         =
# ================

#try:
if __name__ == '__main__':
    print("Please, wait...\n")
    # initialize pygame.mixer module
    # if these setting do not work with your audio system
    # change the global constants accordingly
    try:
        pygame.mixer.init(FREQ, BITSIZE, CHANNELS, BUFFER)
    except pygame.error:
        print >>sys.stderr, "Could not initialize sound system: %s" % exc
    except exc:
        print >>sys.stderr, "Could not initialize sound system: %s" % exc
    
    print("START!\n")
    playsound(soundFileStart)

    pygame.font.init
    pygame.init()
    screen = pygame.display.set_mode((800,600)) #make window
    pygame.display.set_caption("Chronojump encoder")

    FontBig = pygame.font.Font(None, 22)
    FontSmall = pygame.font.Font(None, 18)

    ColorBackground = (30,30,30)
    ColorBad = (255,0,0)
    ColorGood = (0,255,0)

    serL = serial.Serial (enc_l[0]['port'], w_baudrate)
    #serR = serial.Serial (enc_l[1]['port'], w_baudrate)

    for i in range (0, len(enc_l)):
        enc_l[i]['temp_cumsum'] = 0
        enc_l[i]['w_time'] = datetime.now().second
        print ("start read data on " + enc_l[i]['name'] + " at " + enc_l[i]['port'])
    
    for j in range(delete_initial_time):
        serL.read()
        #serR.read()

    #print title
    title = title.replace('_',' ')
    title = title.replace('-',' ')
    printHeader("start")

    secondsLeft = int(record_time / 1000)
    msCount = 0
    countDisplayUpdate = 0

    userStops = FALSE
    for t in range(record_time):
        for event in pygame.event.get():
            if event.type == pygame.QUIT or (event.type == KEYUP and event.key == K_ESCAPE):
                userStops = TRUE

        if userStops:
            print ("USER BREAKS")
            break

        byte_data = serL.read()
        # conver HEX to INT value
        signedChar_data = unpack('b' * len(byte_data), byte_data)[0]
        enc_l[0]['temp'].append(signedChar_data)
        enc_l[0]['temp_cumsum'] += signedChar_data
        #TODO: same for R
                
        countDisplayUpdate += 1
        if countDisplayUpdate >= updateGraphAtMs:
            update_graph(
                    enc_l[0]['temp_cumsum'],
                    enc_l[0]['temp_cumsum'] + 20,
                    graphsWidth, 440, (222,0,0),
                    4, 156, False)
            countDisplayUpdate = 0

    for i in range (0, len(enc_l)):
        enc_l[i]['w_time'] = datetime.now().second - enc_l[i]['w_time']
        serL.close ()
    
    print ("\nDone! Please, close this window.")
    printHeader("end")

    while 1:
        for event in pygame.event.get():
            if event.type == pygame.QUIT or (event.type == KEYUP and event.key == K_ESCAPE):
                sys.exit()

        pygame.time.delay(30)
        pygame.display.flip() #update the screen
        #TODO: http://stackoverflow.com/questions/10466590/hiding-pygame-display

