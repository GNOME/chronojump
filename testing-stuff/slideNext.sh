#!/bin/sh
export DISPLAY=:0.0
WINDOW=`xdotool search --name "Impress"`
#WINDOW=52428837
xdotool windowactivate $WINDOW
xdotool windowfocus $WINDOW
xdotool key Next
