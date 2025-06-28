#!/bin/bash

#apt-get install libglib2.0-dev

cd "$(dirname "$0")"
python3 -m venv ble
source ble/bin/activate
python3 -m pip show bluepy
if [ "$?" != "0" ]; then
    python3 -m pip install bluepy
fi
python3 bluepy-ble.py
