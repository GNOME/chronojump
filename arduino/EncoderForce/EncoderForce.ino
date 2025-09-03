/*
  #
  #  This file is part of ChronoJump
  #
  #  ChronoJump is free software; you can redistribute it and/or modify
  #   it under the terms of the GNU General Public License as published by
  #    the Free Software Foundation; either version 2 of the License, or
  #     (at your option) any later version.
  #
  #  ChronoJump is distributed in the hope that it will be useful,
  #   but WITHOUT ANY WARRANTY; without even the implied warranty of
  #    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  #     GNU General Public License for more details.
  #
  #  You should have received a copy of the GNU General Public License
  #   along with this program; if not, write to the Free Software
  #    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
  #
  #   Copyright (C) 2017     Xavier Padullés <x.padulles@gmail.com>
  #   Copyright (C) 2017     Xavier de Blas <xaviblas@gmail.com>

V0.1
  Initial version
V0.2
  data is int instead of float (to easy C# binary read). Force is multiplied by 100.
  Added start_capture_binary & start_capture_text for testing purposes. Note start_capture is binary.
  changedZ is sent by serial port (not yet to ble) as sensor_t.incEncoderZ
  buffer...
  TODO: write the rest of the changes

--------------
When encoder pulses reach PPS (PulsesPerSample) the time is acquired
Every 6250 us (160 Hz) the force is measured
*/

#include <Preferences.h>  // Used to store tare and calibration in the flash
#include <elapsedMillis.h>
#include <HX711.h>

//Version number. It always need to start with: "Force_Sensor-"
String version = "EncoderForce-0.3";

bool commandFlag = false;

enum captureMode_t {
  normalText = 0,
  normalBinary = 1,
  testBinary12Num = 2,
  testTextEncCountUp = 3,
  testBinaryEnc5Bytes = 4,
  testBinaryEnc8Bytes = 5,
};
captureMode_t captureMode;


enum sensor_t {  // TODO: will be event and it will be a byte
  none = 0,
  rca = 1,
  loadCell = 2,
  incEncoder = 3,
  incEncoderZ = 4,
};

int linePrint = 0;

// no need the enum (4 bytes), 1 byte is enough
// 0 = none;
// 1 = rca on; 2 = rca off
// 3 = load cell
// 4 = incEncoderPos; 5 = incEncoderNeg; 6 = incEncoderZ
byte event = 0;

//TODO:
// 1 treure el codi que no cal,
// 2 veure que falla igual
// 3 sensor_t seria byte event RCA també tindria 2 i incEncoder seria pos o neg
// tenir struct per encoder i struct per galga
// a chronojump llegir 1er byte i si és galga, llavors llegir 11 més. Si és encoder llegir 4 més

// que passa si el fallo és que alguna part de lal estructura s'actualitza en aquell moment i és null?

// sample is the same struct for force & encoder
struct sample_t {
  sensor_t sensorType;
  unsigned int time;
  //float data;
  int data;
};
sample_t sample;

struct sampleForce_t {
  byte event;
  unsigned int time;
  int data;
};
sampleForce_t sampleForce;

struct sampleEncoder_t {
  byte event;
  unsigned int time;
  //float data;c
  //int data; //not needed, it is the pps
};
sampleEncoder_t sampleEncoder;

// ---- testing stuff ---->
struct testing12Data_t {
  int ta = 151653132;  //9*256^3 + 10*256^2 + 11*256^1 +12
  int tb = 84281096;   //5*256^3 + 6*256^2 + 7*256^1 +8
  int tc = 16909060;   //1*256^3 + 2*256^2 + 3*256^1 +4
};

testing12Data_t testing12Data;
//bool testing12Data_b = false;
// <---- testing stuff ----

// ---- buffer stuff ---->
/*
int currentSampleBuffer = 0;
sample_t sampleBuffer[100];       //binary mode. Using char type cannot write speeds greater than +-127 pulses/ms
bool bufferPrinting = false;
*/
// <---- buffer stuff ----

/*
//whether the tranmission is in binary format or not
enum format_t {
  binary = 0,
  text = 1,
};

//format_t transmissionFormat = binary;  //default
//format_t transmissionFormat = text;
*/


// Preparing the flash storage for tare and calibration.
Preferences preferences;

//Whether the sensor has to capture or not
boolean capturing = false;

elapsedMicros totalTime = 0;

const int rcaPin = 3;

unsigned long triggerTime = 0;
bool rcaState = digitalRead(rcaPin);
bool lastRcaState = rcaState;

//Scale stuff
HX711 scale;
#define DOUT 3
#define CLK 2

// Timer to be sure that enough time has elapsed between ADC reading
hw_timer_t *adcTimer = NULL;
int adcPeriod = 6225;  //Time between adc readings. Necessary to let the ADC make its conversion
// int adcPeriod = 1000000;

//Data comming from the cell after resting the offset weight
float offsetted_data = 0;

//Data resulting of appying the calibration_factor to the offsetted_data
float scaled_data = 0;

//The weight used to calibrate the cell
float weight = 0.0;

//The value of the readed force
volatile float force = 0.0;
int forceInt = 0;  //send to Chronojump

volatile unsigned long forceTime = 0;

volatile bool adcFlag = false;

// Encoder stuff
#define EncoderAPin 9
#define EncoderBPin 8
#define EncoderZPin 7
volatile bool encoderFlag = false;
volatile bool flagZ = false;
volatile int currentPosition = 0;
volatile int lastPosition = 0;
volatile unsigned long encoderTime = 0;
//unsigned int pps = 1; // movent lentament funciona. Movent ràpid falla, encara que no s'envii la Z
//unsigned int pps = 2; // també falla
//unsigned int pps = 5; // funciona perfecte movent rapid la rodeta d'encoder (tant moserial com a chronojump)
unsigned int pps = 1;
volatile int debugCount = 0;

void setup() {
  Serial.begin(115200);
  //Serial.begin(460800); //same problem
  Serial.flush();
  scale.begin(DOUT, CLK);


  attachInterrupt(EncoderZPin, changedZ, FALLING);
  attachInterrupt(EncoderAPin, changedA, RISING);  //TODO: implement the possibility of detect every change in both signals. It makes the precission 4 times better
  pinMode(EncoderAPin, INPUT_PULLDOWN);
  pinMode(EncoderBPin, INPUT_PULLDOWN);
  pinMode(EncoderZPin, INPUT_PULLDOWN);

  // pinMode(LED_BUILTIN, OUTPUT);

  preferences.begin("forceSensor", false);               //Initate namespace
  scale.set_offset(preferences.getLong("tare", 0));      //Set tare value with stored value
  scale.set_scale(preferences.getFloat("calibration"));  //Set calibration value with stored value

  // attachInterrupt(digitalPinToInterrupt(rcaPin), changingRCA, CHANGE);
  initializeBLE();
  adcTimer = timerBegin(1000000); // Number of increments of the counter per second. 1E6 -> microseconds
  timerAlarm(adcTimer, adcPeriod, true,0);
  timerAttachInterrupt(adcTimer, &readADC);
}

void loop() {
  if (capturing) {
    loopCapturing();
  }

  //Checking if there's incoming serial data
  if (Serial.available()) {
    processCommand(Serial.readString());
  }

  if (commandFlag) {
    // digitalWrite(LED_BUILTIN, HIGH);
  }
}

// separated from loop, so return from here will allow to read sugin Serial.available()
void loopCapturing() {
  /* Triggers not yet implemented
    if (rcaState != lastRcaState) {       //Event generated by the RCA
      Serial.print(triggerTime);
      Serial.print(";");

      if (rcaState) {
        Serial.println("R");
      } else {
        Serial.println("r");
      }
      lastRcaState = rcaState;
    }
    */

  if (captureMode == testBinary12Num) {
    if (adcFlag)  //TODO: try it with encoderFlag
    {
      // TODO: implement also sampleBuffer
      Serial.write((byte *)&testing12Data, 12);

      adcFlag = false;
    }
    return;
  }

  if (captureMode == testTextEncCountUp) {
    if (encoderFlag) {
      Serial.print(debugCount);
      Serial.print(";");
      Serial.println(lastPosition);

      encoderFlag = false;
    }
    return;
  }

  // note this mode fails, but it works sending only the event or the time
  if (captureMode == testBinaryEnc5Bytes) {
    if (encoderFlag) {
      if (lastPosition > 0) {
        Serial.write(0x04);
      } else {
        Serial.write(0x05);
      }
      //delayMicroseconds (500); it helps but is all data sent?
      Serial.write((byte *)&encoderTime, 4);

      encoderFlag = false;
    }
    return;
  }

  // note this mode fails, but it works sending only the event or the time
  if (captureMode == testBinaryEnc8Bytes) {
    if (encoderFlag) {
      int event = 0;
      if (lastPosition > 0) {
        event = 4;
      } else {
        event = 5;
      }
      Serial.write((byte *)&event, 4);
      //delayMicroseconds (500); it helps but is all data sent?
      Serial.write((byte *)&encoderTime, 4);

      encoderFlag = false;
    }
    return;
  }

  //here captureMode is normalText or normalBinary
  if (adcFlag) {
    // detachInterrupt(digitalPinToInterrupt(rcaPin));
    //Printing to serial lasts 32 ms
    sample.sensorType = loadCell;
    sample.time = forceTime;
    //sample.data = force;
    forceInt = (int)100 * force;
    sample.data = forceInt;
    if (captureMode == normalText) {
      Serial.print(3);  // event.force
      Serial.print(";");
      Serial.print(forceTime);
      Serial.print(";");
      Serial.println(forceInt);
      // attachInterrupt(digitalPinToInterrupt(rcaPin), changingRCA, CHANGE);
      rcaState = digitalRead(rcaPin);
    } else {  // (captureMode == normalBinary)
      Serial.write((byte *)&sample, 12);
      //Serial.write((byte*)&sample.sensorType,4);
      //Serial.write((byte*)&sample.time,4);
      //Serial.write((byte*)&sample.data,4);
    }
    sendToBLE(loadCell);
    adcFlag = false;
  }

  //here captureMode is normalText or normalBinary
  if (encoderFlag) {
    if (captureMode == normalText) {
      if (lastPosition > 0) {
        Serial.print(4);  // event.incEncoderPos
      } else {
        Serial.print(5);  // event.incEncoderNeg
      }
      Serial.print(";");
      Serial.println(encoderTime);
    } else {
      // TODO
    }

    //TODO: clean all this:

    /*
        sample.sensorType = incEncoder;
        sample.time = encoderTime;
        //sample.data = (float)lastPosition;
        sample.data = lastPosition;
        */

    /*  
        if (lastPosition > 0) {
          sampleEncoder.event = 4;
        } else {
          sampleEncoder.event = 5;
        }

        sampleEncoder.time = encoderTime;
        */


    /*
      if (captureMode == normalText) {
        Serial.print(encoderTime);
        Serial.print(";");
        Serial.print(lastPosition);
        Serial.print(";");
        Serial.println(sample.sensorType);
      }
      else { // (captureMode == normalBinary)
      */
    //Serial.write((byte*)&sample,12);
    //sampleBuffer[currentSampleBuffer] = sample;
    //currentSampleBuffer ++;

    /*
        sampleBuffer[currentSampleBuffer ++] = sample;
	
  if (currentSampleBuffer >= 50)
	//if (! bufferPrinting && currentSampleBuffer >= 50)
  //if (! bufferPrinting && currentSampleBuffer >= 5)
	{
		//bufferPrinting = true;
		//Serial.write((byte*)&sampleBuffer, 50 * sizeof(sampleBuffer));
		//Serial.write((byte*)sampleBuffer, 50 * sizeof(sampleBuffer));
		Serial.write((byte*)sampleBuffer, currentSampleBuffer * sizeof(sample));
		currentSampleBuffer = 0;
		//bufferPrinting = false;
	}
  */
    //}
    //Serial.write((byte*)&sampleEncoder, sizeof(sampleEncoder));
    //Serial.write((byte*)&sampleEncoder, 5);             // problems here as sampleEncoder byte is printed as 4 bytes (maybe because 2nd element is time and has 4 bytes)
    //Serial.write((byte*)&sampleEncoder.event, 1);     // this works perfect (just this line)
    //Serial.write((byte*)&sampleEncoder.time, 4);    // this works perfect (just this line)

    /*
      //printing both together. Both together fail as sometimees event sent is 0 (instead of 4 or 5)
      Serial.write((byte*)&sampleEncoder.event, 1);
      Serial.write((byte*)&sampleEncoder.time, 4);
      */

    //Serial.print(encoderTime);

    /* // code to see on Arduino IDE serial monitor. This works perfect
      Serial.print(sampleEncoder.event);
      Serial.print(" ");
      Serial.print(sampleEncoder.time);
      linePrint ++;
      if (linePrint == 10){
        Serial.println("");
        linePrint = 0;
      } else {
        Serial.print(" ");
      }
      */

    /* checking sizes
      Serial.print("----");
      Serial.print(sizeof(sampleEncoder.event)); //1
      Serial.print(sizeof(sampleEncoder.time)); //4
      Serial.println(sizeof(sampleEncoder)); //8!!!
      */
    sendToBLE(incEncoder);
    encoderFlag = false;
  }
}


void changingRCA() {
  triggerTime = totalTime;
  rcaState = digitalRead(rcaPin);
}

//In old version SerialEvent() was used but the nano every don't support it
void processCommand(String inputString) {
  String commandString = inputString.substring(0, inputString.lastIndexOf(":"));

  if (commandString == "start_capture") {
    captureMode = normalText;
    start_capture();
  } else if (commandString == "start_capture_binary") {
    captureMode = normalBinary;
    start_capture();
  } else if (commandString == "start_capture_binary12Num") {
    captureMode = testBinary12Num;
    start_capture();
  } else if (commandString == "start_capture_textEncCountUp") {
    captureMode = testTextEncCountUp;
    start_capture();
  } else if (commandString == "start_capture_binaryEncoder5Bytes") {
    captureMode = testBinaryEnc5Bytes;
    start_capture();
  } else if (commandString == "start_capture_binaryEncoder8Bytes") {
    captureMode = testBinaryEnc8Bytes;
    start_capture();
  } else if (commandString == "end_capture") {
    end_capture();
  } else if (commandString == "get_version") {
    get_version();
  } else if (commandString == "get_calibration_factor") {
    get_calibration_factor();
  } else if (commandString == "set_calibration_factor") {
    set_calibration_factor(inputString);
  } else if (commandString == "calibrate") {
    calibrate(inputString);
  } else if (commandString == "get_tare") {
    get_tare();
  } else if (commandString == "set_tare") {
    set_tare(inputString);
  } else if (commandString == "tare") {
    tare();
  } else if (commandString == "set_bps") {
    set_bps(inputString);
  /*} else if (commandString == "get_transmission_format") {
    get_transmission_format();
  */} else {
    Serial.println("Not a valid command");
  }
  inputString = "";
  //  }
}

void start_capture() {
  Serial.println("Starting capture...");
  totalTime = 0;
  //currentSampleBuffer = 0;
  capturing = true;
}

void end_capture() {
  //TODO: send the remaining sampleBuffer
  capturing = false;
  Serial.println("Capture ended:");
}
void get_version() {
  Serial.println(version);
}

String get_command_argument(String inputString) {
  return (inputString.substring(inputString.lastIndexOf(":") + 1, inputString.lastIndexOf(";")));
}

/*
void get_transmission_format() {
  if (transmissionFormat == binary) {
    Serial.println("binary");
  } else if (transmissionFormat == text) {
    Serial.println("text");
  }
}
*/

void set_bps(String inputString) {
  String speedString = get_command_argument(inputString);
  unsigned long speed = speedString.toInt();

  Serial.print("setting to: ");
  Serial.print(speed);
  Serial.println(" bps");
  Serial.flush();
  Serial.begin(speed);
  Serial.print("Speed set to: ");
  Serial.print(speed);
  Serial.println(" bps");
}
