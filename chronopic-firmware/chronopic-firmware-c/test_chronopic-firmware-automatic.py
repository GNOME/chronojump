import serial
ser = serial.Serial("/dev/ttyUSB0")
ser.baudrate = 9600

#get version
ser.write("V")
major = ser.read()
#1
point = ser.read()
#.
minor = ser.read()
#1
print "version == '1.1' ?", str(major) + str(point) + str(minor) == '1.1'

#comand_port_scanning
ser.write("J")
port = ser.read()
port == 'J'
print "port == J ?", port == 'J'

#get debounce time
ser.write("a")
debounce = int(ser.read())
print "current debounce =", debounce * 10
#5. Ok, it's 5 * 10 ms = 50 ms

#set debounce time
ser.write("b\x01")
print "changing to 10 ms"

#get debounce time
ser.write("a")
debounce = int(ser.read())
print "current debounce =", debounce * 10

#set debounce time
ser.write("b\x05")
print "changing again to 50 ms"

#get debounce time
ser.write("a")
debounce = int(ser.read())
print "current debounce =", debounce * 10

