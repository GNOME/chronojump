 ## Communication
 WiJump can comminicate through usb serial al 115200 bits/s or via Bluetooth.
 The commands are sent with the form [command]:[argument];

 ### Bluetooth services

 Actual data service

 #### Service 1. Sensor data ###
 Service: **b0f3d089-3ce3-4ed6-9920-43afae288982**

 Characteristic: **19a8d494-a941-4ae3-99c0-28e9d352a170**

 Used for receiving data from the sensors. Sends the time at which the state has changed. Negative numbers means leaving the sensor (Beam not detected -> Beam detected). Positive means arriving at the sensor (Beam detected -> Beam not detected).

 ### Service 2. Commands ###
 Service **7ddcf4e4-acad-4935-bc53-5e29e3a130bf**

 Command Characteristic **1a2ae85a-8118-4644-9e3b-387122d8cd9e**

 Used to send commands to the device

 In Bluetooth communication the commands and their output are sent as UTF-8 text
 ### Commands

 - *setIRPower:[duty];*     Sets the duty cicle of the IR LED carrier
 - *get_version:*           Returns the current version of the firmware
 - *setAndMode:*            All sensors must detect presence to trigger the signal
 - *setOrMode:*             Only one sensor can trigger the presence signal
 - *sleep:*                 Enters low consumtion mode
 - *wakeup:*                Leaves low consumtion mode
 - *getBattLev:*            Returns the battery charge level in % (Pending to implement)
 - *setBLEName:*            Changes the name of the Bluetooth device and restart it
 - *sync:*                  Set the time of the device to 0 ms and turns off the IR LEDs in order to synchronize with the other WiJump. Used before the *delay* command
 - *delay:[millis];*  Used to synchronize WiJumps once the difference between clocks are calculated with sync command
 - *startPulsing:*          Turns on and off periodically the carrier signal of the IR LEDs
 - *endPulsing:*            Sets the carrier signal of the IR LEDs to normal mode (Not pulsing)
 - *setPulsePeriod:*        Set the period of pulsing mode in milliseconds
 - *restart:*               Restarts the ESP32

 Output Characteristic **3bf2da15-f408-414f-a8dd-8cf1590b4a4a**

 Used to read the output of the commands
