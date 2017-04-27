#!/bin/sh
#works when Impress is not fullscreen
export DISPLAY=:0.0
WINDOW=`xdotool search --name "Impress"`
#WINDOW=52428837
if ! test -z $WINDOW
then
	xdotool windowactivate $WINDOW
	xdotool windowfocus $WINDOW
	xdotool key Prior
fi
