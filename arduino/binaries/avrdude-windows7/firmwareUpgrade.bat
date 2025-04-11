avrdude.exe "-Cavrdude.conf" -v -V -patmega328p -carduino "-P%1" -b115200 -D "-Uflash:w:%2:i"
