cd %~dp0
%~d0
python -m venv ble
call ble\Scripts\activate
python -m pip install bleak
python ble.py