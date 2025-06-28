#!/bin/bash

cd "$(dirname "$0")"
python3 -m venv ble
source ble/bin/activate
python3 -m pip show simplepyble
if [ "$?" != "0" ]; then
    python3 -m pip install simplepyble
fi
python3 simple-ble.py
