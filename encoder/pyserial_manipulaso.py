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

# PYTHON PACKAGES (pip3 install ...)
#pyserial
#pygame

# Have installed on commandline
#mplayer

# RUNNING
#no time end (need to press escape)
#python3 pyserial_manipulaso.py -1 /dev/ttyUSB0 /dev/ttyUSB1 pyserial_manipulaso_sounds.txt
#end at 100 seconds
#python3 pyserial_manipulaso.py 100 /dev/ttyUSB0 /dev/ttyUSB1 pyserial_manipulaso_sounds.txt

import serial
#from serial import Serial
import sys
#from datetime import datetime
import os
from struct import unpack
import pygame
from pygame.locals import * #mouse and key definitions
from subprocess import Popen, PIPE #to call mplayer to play m4a
#from mplayer import Player
import random
import colorsys


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
            'temp_cumsum':list(),       # current bar position (used also to check speed)
            'last_cumsum':0,            # last bar final position
            'last2_cumsum':0,           # penultimate bar final position
            'last_stored':0,
            #'w_time':0
            },
        {
            'name':'right',
            'port':'',
#            'ser':0, #cannot put this in dictionary
            'dir_change_count':0,
            'dir_pull_to_push':'pull_to_push',     # if people change the motion for pull to push, record this value.
            'dir_push_to_pull':'push_to_pull',     # if people change the motion for push to pull, record this value.
            'dir_now':1,			# 1 or -1
            'dir_last_ms':1,			# 1 or -1
            'dir_completed':-1,		# 1 or -1
            'frames_pull_top1':list(),
            'frames_push_bottom1':list(),
            'previous_frame_change':0,
            'temp':list (),
            'temp_cumsum':list(),       # current bar position (used also to check speed)
            'last_cumsum':0,            # last bar final position
            'last2_cumsum':0,           # penultimate bar final position
            'last_stored':0,
            #'w_time':0
            }
        ]


# ================
# = Sounds stuff =
# ================

sounds_l =[ ]

class soundsLR:
    def __init__(self, title, left, right, musicUrlName):
        self.title = title
        self.left = left
        self.right = right
        if ";" in musicUrlName:
            (self.musicUrl, self.musicName) = musicUrlName.split (';') #split in two (1 split): 1st name, 2nd value/s
        else:
            self.musicUrl = musicUrlName
            self.musicName = ""

    def left (title):
        return self.title

    def left (self):
        return self.left

    def right (self):
        return self.right

    def musicUrl (self):
        return self.musicUrl

    def musicName (self):
        return self.musicName

    def printAll (self):
        print("'{}': L: {}, R: {}, music: {}, music:Name: {}".format(self.title, self.left,
            self.right, self.musicUrl, self.musicName))


class soundManage:
    def __init__(self):
        self.count = 0

    def soundsNext (self):
        self.count += 1
        if self.count >= len (sounds_l):
            self.count = 0

    def soundsPre (self):
        self.count -= 1
        if self.count < 0:
            self.count = 0

    def soundLeftIsEmpty (self): #silence
        return len (sounds_l[self.count].left) == 0

    def soundRightIsEmpty (self): #silence
        return len (sounds_l[self.count].right) == 0

    def soundMusicIsEmpty (self): #silence
        return len (sounds_l[self.count].musicUrl) == 0

    def soundsPlayLeft (self):
        playSound (sounds_l[self.count].left)

    def soundsPlayRight (self):
        playSound (sounds_l[self.count].right)

    #finally use mplayer and in the main loop is what works best and we do not need
    #to care on encoder loop or pygame loop

    #def soundsPlayMusic (self):
         #playSound (sounds_l[self.count].music)
         #this plays .m4a
         #pipes = dict(stdin=PIPE, stdout=PIPE, stderr=PIPE)
         #mplayer = Popen(["mplayer", sounds_l[self.count].music], **pipes)
     #    pygame.mixer.music.load (sounds_l[self.count].music)
     #    pygame.mixer.music.play (-1)

   #def soundsStopMusic (self):
        #mplayer.kill ()
        #pygame.mixer.music.stop ()

    def getTitle (self):
         return (sounds_l[self.count].title)

    def getMusicUrl (self):
         return (sounds_l[self.count].musicUrl)

    def getMusicName (self):
         return (sounds_l[self.count].musicName)

#http://code.activestate.com/recipes/521884-play-sound-files-with-pygame-in-a-cross-platform-m/
# global constants
#FREQ = 44100   # same as audio CD
FREQ = 8000   # same as audio CD
BITSIZE = -16  # unsigned 16 bit
#CHANNELS = 2   # 1 == mono, 2 == stereo
CHANNELS = 1   # 1 == mono, 2 == stereo
BUFFER = 1024  # audio buffer size in no. of samples
FRAMERATE = 30 # how often to check if playback has finished
#clock = pygame.time.Clock()

#disabled clock calls to go faster
#more info on playSound here https://pythonprogramming.net/adding-sounds-music-pygame/
def playSound(soundfile):
    sound = pygame.mixer.Sound(soundfile)
    #clock = pygame.time.Clock()
    sound.play()
    #while pygame.mixer.get_busy():
    #    clock.tick(FRAMERATE)

soundFileStart = "/home/xavier/informatica/progs_meus/chronojump/encoder/Question.wav"
soundFileGood = "/home/xavier/informatica/progs_meus/chronojump/encoder/Asterisk.wav"
#soundFileBad = "/home/xavier/informatica/progs_meus/chronojump/encoder/Beep.wav"
soundFileBad = "/home/xavier/informatica/progs_meus/chronojump/encoder/Hand.wav"

def readSoundsListConfig (soundsListCfg):
    file = open(soundsListCfg, "r")
    soundPath = ""
    soundLines = file.read().splitlines()
    for soundLine in soundLines:
        if soundLine.startswith ('#'):
            continue

        (name, value) = soundLine.split (':', 1) #split in two (1 split): 1st name, 2nd value/s
        if name == "PATH":
            soundPath = value
        elif name == "SoundLR":
            (title, l, r, musicUrlName) = value.split (':')

            #add soundPath to l, r if they are not empty. To allow silence.
            if len(l) > 0:
                l = soundPath + l
            if len(r) > 0:
                r = soundPath + r
            if len(musicUrlName) > 0:
                #print ("soundPath is: " + soundPath)
                musicUrlName = soundPath + musicUrlName

            sounds_l.append (soundsLR (title, l, r, musicUrlName))

#note a sound can be empty just to be able to move hands
def checkSoundsListMissing ():
    for sound in sounds_l:
        sound.printAll ()
        if len (sound.left) > 0 and not os.path.isfile (sound.left):
            print ("Left sound does not exists: " + sound.left)
            return False
        if len (sound.right) > 0 and not os.path.isfile (sound.right):
            print ("Right sound does not exists: " + sound.right)
            return False
        if len (sound.musicUrl) > 0 and not os.path.isfile (sound.musicUrl):
            print ("Music sound does not exists: " + sound.musicUrl)
            return False
    return True


# ===================
# = Graphical stuff =
# ===================

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

graphsWidth = 1360 #1368-4-4
graphsHeight = 760 #768-4-4
updateGraphAtMs = 25

def colorDarker (color):
    return (pygame.Color(
        int(.5 * color[0]),
        int(.5 * color[1]),
        int(.5 * color[2]),
        255))

#https://stackoverflow.com/a/43437435
def colorRandomBright ():
    h,s,l = random.random(), 0.5 + random.random()/2.0, 0.4 + random.random()/5.0
    r,g,b = [int(256*i) for i in colorsys.hls_to_rgb(h,l,s)]
    return Color (r,g,b)


#posL_l and posR_l are lists of pos for L and R, 1st element of each one is current, 3rd the penultimate
def update_graph (title, posL_l, posR_l, #speedL, speedR,
        my_s_width, my_s_height, barsColor, horizPosToCopy, vertPosToCopy, hasDecimals):
    #s = pygame.Surface((my_s_width,my_s_height), pygame.SRCALPHA)
    s = pygame.Surface((my_s_width,my_s_height))
    
    s.fill(ColorBackground) #color the surface
    #print("speedL: {}; speedR: {}".format(speedL, speedR))

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
    barL_height = [0, 0, 0]
    barR_height = [0, 0, 0]

    for i in range (0, 3):
        barL_height[i] = (my_s_height -vert_margin) * posL_l[i] / barMax
        barR_height[i] = (my_s_height -vert_margin) * posR_l[i] / barMax

    #bar_width = (my_s_width -left_margin -right_margin -sep) / 9 #each bar 1/3 of screen
    #width of last2, last will be double, current quadriple
    #1 + 2 + 4 + 2 (free space) = 9 #18 because have to be space for left and for right
    min_bar_width = (my_s_width -left_margin -right_margin) / 18
    #print ("min_bar_width {}".format(min_bar_width))

    colorNow = barsColor

    barL_left = my_s_width /2 - 4 * min_bar_width -min_bar_width #left part of left bar
    barR_left = my_s_width /2 +min_bar_width           #left part of right bar
    bar_width = 4 * min_bar_width
    for i in range (0, 3):
        #colorNow[3] = int(.66 * colorNow[3]) # no luck with transparency
        colorNow = colorDarker (colorNow)
        #print ("i {}, left {}, bar_width {}".format(i, left, bar_width))
        pygame.draw.rect(s, colorNow,
                (barL_left,
                    my_s_height -barL_height[i],
                    bar_width,
                    barL_height[i])
                )
        #outline:
        pygame.draw.rect(s, (255, 255, 255),
                (barL_left,
                    my_s_height -barL_height[i],
                    bar_width,
                    barL_height[i])
                , width=4)

        pygame.draw.rect(s, colorNow,
                (barR_left,
                    my_s_height -barR_height[i],
                    bar_width,
                    barR_height[i])
                ) #0: filled

        #outline
        pygame.draw.rect(s, (255, 255, 255),
                (barR_left,
                    my_s_height -barR_height[i],
                    bar_width,
                    barR_height[i])
                , width=4)

        barR_left += bar_width
        bar_width /= 2
        barL_left -= bar_width
    
    s_rect=s.get_rect() #get the rectangle bounds for the surface
    screen.blit(s,(horizPosToCopy,vertPosToCopy)) #render the surface into the rectangle
    pygame.display.flip() #update the screen

#option can be "start", "end",
#or time left: "5 s", "4 s", ..
def printHeader(option):
    s=pygame.Surface((graphsWidth,32))
    s.fill(ColorBackground) #color the surface

    string = "ManipulaSo"
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

    textpos = text.get_rect(right=graphsWidth-10,centery=14)
    s.blit(text,textpos)

    #screen.blit(s,(4,4)) #render the surface into the rectangle
    screen.blit(s,(0,4)) #render the surface into the rectangle
    pygame.display.flip() #update the screen

def printMusic(musicName):
    s=pygame.Surface((graphsWidth,123))
    s.fill(ColorBackground) #color the surface

    text = FontMusic.render(musicName,1, (255,255,255))
    textpos = text.get_rect(left=10,centery=14)
    textpos.center = (graphsWidth/2, 123/2)
    #textpos = text.get_rect(right=graphsWidth-10,centery=14)
    s.blit(text,textpos)

    screen.blit(s,(0,33)) #render the surface into the rectangle
    pygame.display.flip() #update the screen

# =================
# = Encoder stuff =
# =================

delete_initial_time = 20			#delete first records because there's encoder bug
#w_baudrate = 9600                           # Setting the baudrate of Chronopic(9600)
w_baudrate = 115200                           # Setting the baudrate of Chronopic(115200)
dir_change_period = 25                # how long to recognize as change direction.
minRangeMm = 150
#minRangeMm = 300
TRIGGER_ON = 84; #'T' from TRIGGER_ON on encoder firmware
TRIGGER_OFF = 116; #'t' from TRIGGER_OFF on encoder firmware

serL = 0
serR = 0

def isTrigger (byte_data):
    return isTriggerOn (byte_data) or isTriggerOff (byte_data)

def isTriggerOn (byte_data):
    return unpack('b' * len(byte_data), byte_data)[0] == TRIGGER_ON

def isTriggerOff (byte_data):
    return unpack('b' * len(byte_data), byte_data)[0] == TRIGGER_OFF

def manageDirection (byte_data, e, msCount):
    playSound = False

    # conver HEX to INT value
    signedChar_data = unpack('b' * len(byte_data), byte_data)[0]
    #print ("sc:" + str(signedChar_data))
    e['temp'].append(signedChar_data)

    previous = 0
    if (len (e['temp_cumsum']) > 0):
        previous = e['temp_cumsum'][-1]
    e['temp_cumsum'].append (previous + signedChar_data)

    # Judging if direction has changed
    if signedChar_data != 0:
        e['dir_now'] = signedChar_data / abs(signedChar_data) #1 (pull) or -1 (push)
    if e['dir_now'] != e['dir_last_ms']:
        e['dir_last_ms'] = e['dir_now']
        e['dir_change_count'] = 0
        if e['dir_now'] == -1:
            e['last_stored'] = e['temp_cumsum'][-1]
    elif e['dir_now'] != e['dir_completed']:
        #we cannot add signedChar_data because then is difficult to come back n frames to know the max point
        #direction_change_count = direction_change_count + signedChar_data
        e['dir_change_count'] = e['dir_change_count'] + 1
        if e['dir_change_count'] >= dir_change_period:

            k = list(e['temp_cumsum'][e['previous_frame_change']:msCount-dir_change_period])
            #print ("k")
            #print (k)

            if e['dir_now'] == 1:
                #we are going up, we passed the direction_change_count
                #then we can record the bottom moment
                #and print speed on going down (Not done anymore)

                #this has (maybe) 0,-1,0,0,0,0,0, .... (and -1 can be selected (min(k)).
                #Then, do not pass this to frames_push_bottom, pass the next new_frame_change

                new_frame_change = e['previous_frame_change']+k.index(min(k))
                e['frames_push_bottom1'].append(new_frame_change)
                new_frame_change = e['previous_frame_change']+len(k)-1-k[::-1].index(min(k))
            else:
                new_frame_change = e['previous_frame_change']+k.index(max(k))
                e['frames_pull_top1'].append(new_frame_change)
                new_frame_change = e['previous_frame_change']+len(k)-1-k[::-1].index(max(k))

            if len(e['frames_pull_top1'])>0 and len(e['frames_push_bottom1'])>0:
                if e['dir_now'] == -1 and len(k) > 2 and k[-1] -k[0] > minRangeMm:
                    #print ("send concentric:" + str(k))
                    playSound = True

                    e['last2_cumsum'] = e['last_cumsum']
                    e['last_cumsum'] = e['last_stored']

            e['previous_frame_change'] = new_frame_change
            e['dir_change_count'] = 0
            e['dir_completed'] = e['dir_now']

    return playSound

def getSpeed (con_l):
    if len (con_l) == 0:
        return 0

    dist = con_l[-1] - con_l[0]
    print ("dist: " + str(dist))

    if dist <= 0:
        return 0
    else:
        return dist / len (con_l)

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

    #argv parameters
    print(sys.argv)

    record_time = int(sys.argv[1])
    if record_time > 0: #-1 is forever
        record_time = int(sys.argv[1])*1000		#from s to ms

    enc_l[0]['port'] = sys.argv[2] #left encoder
    enc_l[1]['port'] = sys.argv[3] #right encoder
    soundsListCfg = sys.argv[4]

    #read sounds file
    readSoundsListConfig (soundsListCfg)
    if not checkSoundsListMissing ():
        sys.exit()

    sm = soundManage ()

    print("START!\n")
    #playSound(soundFileStart)

    pygame.font.init
    pygame.init()
    pygame.mixer.init()

    #screen = pygame.display.set_mode((800,600)) #make window
    screen = pygame.display.set_mode((graphsWidth,graphsHeight)) #make window
    pygame.display.set_caption("Chronojump encoder")

    FontMusic = pygame.font.Font(None, 60)
    FontBig = pygame.font.Font(None, 22)
    FontSmall = pygame.font.Font(None, 18)

    ColorBackground = (30,30,30)
    ColorBad = (255,0,0)
    ColorGood = (0,255,0)

    #timeout=1 because one time I had: evice reports readiness to read but returned no data (device disconnected or multiple access on port?
    #https://stackoverflow.com/questions/43521818/python-reading-from-serial
    #or if fails more try this:
    #ihttps://stackoverflow.com/questions/28343941/python-serialexception-device-reports-readiness-to-read-but-returned-no-data-d
    #it happens on moving encoder/chronopic, maybe a bit of cable disconnect.
    #timout=1 does not help
    #serL = serial.Serial (enc_l[0]['port'], w_baudrate, timeout=1)
    #serR = serial.Serial (enc_l[1]['port'], w_baudrate, timeout=1)
    serL = serial.Serial (enc_l[0]['port'], w_baudrate)
    serR = serial.Serial (enc_l[1]['port'], w_baudrate)

    for i in range (0, len(enc_l)):
        #enc_l[i]['temp_cumsum'].append (0)
        enc_l[i]['temp_cumsum'] = list ()
        #enc_l[i]['w_time'] = datetime.now().second
        print ("start read data on " + enc_l[i]['name'] + " at " + enc_l[i]['port'])
    
    for j in range(delete_initial_time):
        try:
            serL.read()
        except serial.serialutil.SerialException:
            time.sleep (1)
        try:
            serR.read()
        except serial.serialutil.SerialException:
            time.sleep (1)

    #print title
    #title = title.replace('_',' ')
    #title = title.replace('-',' ')
    printHeader (sm.getTitle ())
    printMusic ("")

    secondsLeft = int(record_time / 1000)
    countDisplayUpdate = 0

    msCount = 0
    userStops = FALSE
    pipes = dict(stdin=PIPE, stdout=PIPE, stderr=PIPE)
    playingMusic = False
    barsColor = colorRandomBright ()

    printMusic ("hola")
    while True:
        for event in pygame.event.get():
            if event.type == pygame.QUIT or (event.type == KEYUP and event.key == K_ESCAPE):
                userStops = TRUE
            elif (event.type == KEYUP and event.key == K_s) and not sm.soundMusicIsEmpty ():
                print ("s pressed")
                if not playingMusic:
                    mplayer = Popen(["mplayer", sm.getMusicUrl ()], **pipes)
                    playingMusic = True
                    if sm.getMusicName () != "":
                        printMusic (sm.getMusicName ())

            elif (event.type == KEYUP and event.key == K_e) and not sm.soundMusicIsEmpty ():
                print ("e pressed")
                try:
                    mplayer.kill ()
                except:
                    print("could not mplayer kill")

        if userStops:
            print ("USER BREAKS")
            break

        if record_time > 0 and msCount > record_time:
            print ("BREAK BY TIME END")
            break

        soundChanged = False
        # read for left encoder.
        # try except works great on encoder/chronopic movement
        try:
            byte_dataL = serL.read()
        except serial.serialutil.SerialException:
            time.sleep (1)
            continue

        if isTrigger (byte_dataL):
            if isTriggerOn (byte_dataL):
                sm.soundsPre ()
                soundChanged = True
        elif not sm.soundLeftIsEmpty ():
            if manageDirection (byte_dataL, enc_l[0], msCount):
                sm.soundsPlayLeft ()

        # read for right encoder
        # try except works great on encoder/chronopic movement
        try:
            byte_dataR = serR.read()
        except serial.serialutil.SerialException:
            time.sleep (1)
            continue

        if isTrigger (byte_dataR):
            if isTriggerOn (byte_dataR):
                sm.soundsNext ()
                soundChanged = True
        elif not sm.soundRightIsEmpty ():
            if manageDirection (byte_dataR, enc_l[1], msCount):
                sm.soundsPlayRight ()

        if soundChanged:
            printHeader (sm.getTitle ())
            barsColor = colorRandomBright ()
            if playingMusic:
                mplayer.kill ()
        else:
            msCount += 1

        # update screen
        countDisplayUpdate += 1
        if countDisplayUpdate >= updateGraphAtMs:
            update_graph(
                    sm.getTitle,
                    [enc_l[0]['temp_cumsum'][-1], enc_l[0]['last_cumsum'],
                        enc_l[0]['last2_cumsum']],
                    [enc_l[1]['temp_cumsum'][-1], enc_l[1]['last_cumsum'],
                        enc_l[1]['last2_cumsum']],
                    #getSpeed
                    #(list(enc_l[0]['temp_cumsum'][enc_l[0]['previous_frame_change']:enc_l[0]['temp_cumsum'][-1]])),
                    #getSpeed
                    #(list(enc_l[1]['temp_cumsum'][enc_l[1]['previous_frame_change']:enc_l[1]['temp_cumsum'][-1]])),
                    graphsWidth, graphsHeight -156,
                    #barsColor, 4, 156, False)
                    barsColor, 0, 156, False)
            countDisplayUpdate = 0

    for i in range (0, len(enc_l)):
        #enc_l[i]['w_time'] = datetime.now().second - enc_l[i]['w_time']
        serL.close ()
    
    print ("\nDone! Please, close this window.")
    printHeader("end")

#    while 1:
#        for event in pygame.event.get():
#            if event.type == pygame.QUIT or (event.type == KEYUP and event.key == K_ESCAPE):
#                sys.exit()
#
#        pygame.time.delay(30)
#        pygame.display.flip() #update the screen
#        #TODO: http://stackoverflow.com/questions/10466590/hiding-pygame-display

