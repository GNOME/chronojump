#!/bin/bash

cd "$(dirname "$0")"
$1 -m venv ble
source ble/bin/activate
$1 -m pip show bleak
if [ "$?" != "0" ]; then
    #$1 -m pip install bleak #AT 0.22.3 not working ok
    $1 -m pip install https://github.com/hbldh/bleak/archive/refs/heads/develop.zip #now installs 2.1.1 and works nice
fi
$1 ble.py --mode $2 --value $3
