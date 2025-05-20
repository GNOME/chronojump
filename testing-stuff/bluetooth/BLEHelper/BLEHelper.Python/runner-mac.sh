#!/bin/sh

cd "$(dirname "$0")"
python3 -m venv ble
source ble/bin/activate
python3 -m pip install bleak
python3 ble.py
