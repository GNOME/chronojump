/*
  Copyright (C) 2018 Xavier Padullés support@chronojump.org

  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

#include <Wire.h>
#include <Adafruit_ADS1015.h>
#include <EEPROM.h>

Adafruit_ADS1015 loadCell;


//Version number
//it always need to start with: "Force_Sensor-" Since 0.3 data is sent in binary format
String version = "Force_Sensor-0.3";

int offset[] = {0, 0, 0, 0};
float calibrationFactor[] = {0.0, 0.0, 0.0, 0.0};

unsigned long lastTime = 0;
unsigned long currentTime = 0;
unsigned long elapsedTime = 0;
unsigned long totalTime = 0;

//Wether the sensor has to capture or not
boolean capturing = false;

void setup(void)
{

  Serial.begin(115200);
  //Serial.begin(250000);

  loadCell.begin();
  loadCell.setGain(GAIN_TWO);
  tare();
  // Serial.println("taring complete");
}

void loop(void)
{
  if (capturing)
  {
    int offsettedData[] = {0, 0, 0, 0};
    float force[] = {0, 0, 0, 0};
    long int total[] = {0, 0, 0, 0};

    //sendLong(initSymbol); //sembla que abans dels 4 255 aixo envia 2 bytes que no son 255, o aquests 2 bytes van al final dels 4 del for
    //sendLong(micros());

    int currentSimulatedValue = -100;

    for (int i = 0; i < 100; i++)
    {
      currentTime = micros();
      //
      //    //Reading each of the 4 sensors
      //    for (int sensor = 0; sensor <= 3; sensor++)
      //    {
      //      offsettedData[sensor] = readOffsetedData(sensor);
      //      //      total[sensor] += offsettedData[sensor];
      //    }


      Serial.write(0xff);
      Serial.write(0xff);
      Serial.write(0xff);
      Serial.write(0xff);

      //Managing the timer overflow
      if (currentTime > lastTime)      //No overflow
      {
        elapsedTime = currentTime - lastTime;
      } else  if (currentTime <= lastTime)  //Overflow
      {
        elapsedTime = (4294967295 - lastTime) + currentTime; //Time from the last measure to the overflow event plus the currentTime
      }
      totalTime += elapsedTime;
      lastTime = currentTime;
      sendLong(totalTime);

      sendInt(currentSimulatedValue ++);
      sendInt(currentSimulatedValue ++);
      sendInt(currentSimulatedValue ++);
      sendInt(currentSimulatedValue ++);

      if(currentSimulatedValue > 100)
	currentSimulatedValue = -100;
    }
  }
}

void tare(void)
{
  long int total[] = {0, 0, 0, 0};
  for (int i = 1; i <= 100;  i++)
  {
    for (int sensor = 0; sensor <= 3; sensor++)
    {
      total[sensor] += loadCell.readADC_SingleEnded(sensor);
    }
  }

  for (int sensor = 0; sensor <= 3; sensor++)
  {
    offset[sensor] = total[sensor] / 100;
    EEPROM.put(sensor * 2, offset[sensor]);
  }
}

int readOffsetedData(int sensor)
{
  return (loadCell.readADC_SingleEnded(sensor) - offset[sensor]);
}

void calibrate(float load)
{
  float calibrationFactor[] = {0, 0, 0, 0};
  float total[] = {0, 0, 0, 0};
  for (int i = 1; i <= 1000;  i++)
  {
    for (int sensor = 0; sensor <= 3; sensor++)
    {
      total[sensor] += readOffsetedData(sensor);
    }
  }

  for (int sensor = 0; sensor <= 3; sensor++)
  {
    calibrationFactor[sensor] = load / (total[sensor] / 1000.0);
    EEPROM.put(6 + sensor * 4, calibrationFactor[sensor]);
  }
}

float readForce(int sensor)
{
  int offsetedData = readOffsetedData[sensor];
  float force = calibrationFactor[sensor] * (float)offsetedData;
  return (force);
}

void sendInt(int i) {
  byte * b = (byte *) &i;
  Serial.write(b[0]); //Least significant bit
  Serial.write(b[1]); //Most significant bit
  Serial.flush();
  return;
}

void sendLong(long l) {
  byte * b = (byte *) &l;
  Serial.write(b[0]); //Least significant bit
  Serial.write(b[1]);
  Serial.write(b[2]);
  Serial.write(b[3]); //Most significant bit
  Serial.flush();
  return;
}


void sendFloat(float f) {
  byte * b = (byte *) &f;
  Serial.write(b[0]); //Least significant bit
  Serial.write(b[1]);
  Serial.write(b[2]);
  Serial.write(b[3]); //Most significant bit
  Serial.flush();
  return;
}

void serialEvent()
{
  String inputString = Serial.readString();
  String commandString = inputString.substring(0, inputString.lastIndexOf(":"));

  if (commandString == "start_capture") {
    start_capture();
  } else if (commandString == "end_capture") {
    end_capture();
  } else if (commandString == "get_version") {
    get_version();
    //      } else if (commandString == "get_calibration_factor"){
    //        get_calibration_factor();
    //      } else if (commandString == "set_calibration_factor"){
    //        set_calibration_factor(inputString);
    //      } else if (commandString == "calibrate"){
    //        calibrate(inputString);
    //      } else if (commandString == "get_tare"){
    //        get_tare();
    //      } else if (commandString == "set_tare"){
    //        set_tare(inputString);
  } else if (commandString == "tare") {
    tare();
  } else {
    Serial.println("Not a valid command");
  }
  inputString = "";
  //  }
}

void start_capture()
{
  Serial.println("Starting capture...");
  totalTime = 0;
  lastTime = micros();
  capturing = true;
}

void end_capture()
{
  capturing = false;
  Serial.println("Capture ended");
}
void get_version()
{
  Serial.println(version);
}

