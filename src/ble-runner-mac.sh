#!/bin/sh

cd "$(dirname "$0")"
#$1 -m venv ble
#source ble/bin/activate
$1 -m pip show bleak
if [ "$?" != "0" ]; then      
    $1 -m pip install bleak
fi
$1 ble.py