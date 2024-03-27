Programming the Bluetooth LE stuff:
https://wiki.seeedstudio.com/xiao_esp32s3_bluetooth/#ble-serverclient

Checking in android client:
Use the app nRF Connect

Checking in a Linux desktop:

# bluetoothctl typical use

## Generic
- "help" for a list of commands of the current menu.
- "menu gatt" for entering gatt interactive mode. For not having to write "gatt.[...]"
- "back" for going to main menu

## Conect:
- "scan on" for scanning bluetooth devices and the UUID of each device
- "connect [MAC]" will connect and change the prompt to the human readable name of the device (48:27:E2:E6:EF:D5 for "Rosa" testing unit)
- "gatt.list-attributes" will list the available services and attributes and their UUID
- "pair [MAC] for pairing BLE device

## Read time values
- "gatt.select-attribute 19a8d494-a941-4ae3-99c0-28e9d352a170" select the attribute lastEvent. Negative numbers means leaving the sensor beam and viceversa.
- "gatt.read" will read the value of the attribute once.
- "gatt.notify on" reads the selected attribute each time it changes and notifies it.


## Send commands
- "gat.write "0x73 0x65 0x74 0x50 0x75 0x6c 0x73 0x65 0x50 0x65 0x72 0x69 0x6f 0x64 0x3a 0x32 0x3b" to set the period to 2 (change every 1 second)
- "gat.write "0x73 0x65 0x74 0x50 0x75 0x6c 0x73 0x65 0x50 0x65 0x72 0x69 0x6f 0x64 0x3a 0x35 0x3b" to set the period to 5
- "gatt.write "0x73 0x74 0x61 0x72 0x74 0x50 0x75 0x6c 0x73 0x69 0x6e 0x67 0x3a" " to start pulsing mode
- "gatt.select-attribute 1a2ae85a-8118-4644-9e3b-387122d8cd9e" to connect to command attribute

## Read logs and output
- "gat.select-attribute 3bf2da15-f408-414f-a8dd-8cf1590b4a4a" select the output attribute. For logs and command outputs
- "gatt.read" reads the output

