#!/bin/bash

cd "$(dirname "$0")"
#python3 -m venv ble
#source ble/bin/activate
python3 -m pip show bleak
if [ "$?" != "0" ]; then      
    python3 -m pip install bleak
fi
python3 ble.py
