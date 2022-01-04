

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


*/

#include <HX711.h>
#include <EEPROM.h>
#include <LiquidCrystal.h>
#include <MsTimer2.h>

#define DOUT  5
#define CLK  4

//Version number //it always need to start with: "Force_Sensor-"
String device = "Force_Sensor";
String version = "0.7";


int tareAddress = 0;
int calibrationAddress = 4;

HX711 scale(DOUT, CLK);

//Data comming from the cell after resting the offset weight
float offsetted_data = 0;

//Data resulting of appying the calibration_factor to the offsetted_data
float scaled_data = 0;

//The weight used to calibrate the cell
float weight = 0.0;

//Wether the sensor has to capture or not
boolean capturing = false;

//Wether the sync time must be sent or not
bool sendSyncTime = false;

//wether the tranmission is in binary format or not
boolean binaryFormat = false;

unsigned long lastTime = 0;

//RFD variables
//for RFD cannot used lastTime, can have overflow problems. Better use elapsedTime
unsigned long rfdTimePre = 0;
unsigned long rfdTimePre2 = 0;
float rfdMeasuredPre = 0;
float rfdMeasuredPre2 = 0;
bool rfdDataPreOk = false;
bool rfdDataPre2Ok = false;
bool rfdCalculed = false;
float rfdValueMax = 0;


unsigned long currentTime = 0;
unsigned long elapsedTime = 0;
unsigned long totalTime = 0;
unsigned long syncTime = 0;
unsigned int samples = 0;

const int redButtonPin = 13;
bool redButtonState;

const int blueButtonPin = 6;
bool blueButtonState;
float voltage;


unsigned int lcdDelay = 25; //to be able to see the screen. Seconds are also printed in delay but 25 values are less than one second
unsigned int lcdCount = 0;
float measuredLcdDelayMax = 0; //The max in the lcdDelay periodca
float measuredMax = 0; // The max since starting capture
float measured = scale.get_units();

byte recordChar[] = {
  B00000,
  B01110,
  B11111,
  B11111,
  B11111,
  B11111,
  B01110,
  B00000
};

byte menuChar[] = {
  B00000,
  B01110,
  B10001,
  B10001,
  B10001,
  B10001,
  B01110,
  B00000
};

byte battery0[] = {
  B01110,
  B11111,
  B10001,
  B10001,
  B10001,
  B10001,
  B10001,
  B11111
};

byte battery1[] = {
  B01110,
  B11111,
  B10001,
  B10001,
  B10001,
  B10001,
  B11111,
  B11111
};

byte battery2[] = {
  B01110,
  B11111,
  B10001,
  B10001,
  B10001,
  B11111,
  B11111,
  B11111
};

byte battery3[] = {
  B01110,
  B11111,
  B10001,
  B10001,
  B11111,
  B11111,
  B11111,
  B11111
};

byte battery4[] = {
  B01110,
  B11111,
  B10001,
  B11111,
  B11111,
  B11111,
  B11111,
  B11111
};

byte battery5[] = {
  B01110,
  B11111,
  B11111,
  B11111,
  B11111,
  B11111,
  B11111,
  B11111
};

bool showRecordChar = false;


LiquidCrystal lcd(12, 11, 10, 9, 8, 7);

const int rcaPin = 3;

unsigned long triggerTime = 0;
bool rcaState = digitalRead(rcaPin);
bool lastRcaState = rcaState;

unsigned short menu = 0;
unsigned short submenu = 0;

const String menuList [] = {
  "--- Measure - ",
  "-Tare+Measure-",
  "---- Tare --- ",
  "-- Calibrate -",
  "---- System - "
};

//Array where all measures in 1s are stored
float forces1s[90];
//The current position in the array
int currentPosition = 0;
//mean force during 1s
float meanForce1s;
float maxMeanForce1s = 0;

void setup() {
  pinMode(redButtonPin, INPUT);
  pinMode(blueButtonPin, INPUT);
  analogWrite(6, 20);
  lcd.begin(16, 2);

  Serial.begin(115200);

  attachInterrupt(digitalPinToInterrupt(rcaPin), changingRCA, CHANGE);

  if (redButtonState == 0) {
    lcd.setCursor(2, 0);
    lcd.print("CHRONOJUMP");
    lcd.setCursor(2, 1);
    lcd.print("Boscosystem");
    //    kangaroo();
    //      printLcdFormat (-1.23456, 3, 0, 3);
  }

  long tare = 0;
  EEPROM.get(tareAddress, tare);
  if (tare == -151) {
    scale.set_offset(10000);// Usual value  in Chronojump strength gauge
    EEPROM.put(tareAddress, 10000);
  } else {
    scale.set_offset(tare);
  }


  //The factor to convert the units coming from the cell to the units used in the calibration
  float calibration_factor = 0.0f;
  EEPROM.get(calibrationAddress, calibration_factor);
  if (isnan(calibration_factor)) {
    scale.set_scale(915.0);// Usual value  in Chronojump strength gauge
    EEPROM.put(calibrationAddress, 915.0);
  } else {
    scale.set_scale(calibration_factor);
  }

  MsTimer2::set(1000, showBatteryLevel);
  MsTimer2::start();

  showMenu();
  //  lcd.print("Red: Start");
}

void loop()
{
  if (!capturing)
  {
    blueButtonState = digitalRead(blueButtonPin);
    if (blueButtonState) {
      blueButtonState = false;
      menu++;
      menu = menu % 5;
      showMenu();
    }
    delay(100);

    redButtonState = digitalRead(redButtonPin);
    if (redButtonState)
    {
      //    Serial.println("redButton");
      //    lcd.clear();
      //    lcd.setCursor(1, 0);
      //    lcd.print(menu);
      //    lcd.setCursor(3, 0);
      //    lcd.print(menuList[menu]);
      //    lcd.setCursor(2, 1);

      if (menu == 0)
      {
        lcd.clear();
        lcd.setCursor(0, 0);
        lcd.print("Starting capture");
        lcd.print("Capturing");
        delay(200);
        start_capture();
      } else if (menu == 1)
      {
        tare();
        start_capture();
      } else if (menu == 2)
      {
        tare();
        menu = 0;
        showMenu();
      } else if (menu == 3)
      {
        calibrateLCD();
        menu = 0;
        showMenu();
      } else if (menu == 4)
      {
        showSystemInfo();
        menu = 0;
        showMenu();
      }
      redButtonState = false;
    }
  } else
  {
    capture();
  }
}

void showMenu(void)
{
  //  Serial.println("Menu");
  lcd.clear();
  showBatteryLevel();
  lcd.setCursor(1, 0);
  lcd.print(menuList[menu]);
  lcd.createChar(6, menuChar);
  lcd.setCursor(15, 0);
  lcd.write(byte (6));
  lcd.createChar(7, recordChar);
  lcd.setCursor(15, 1);
  lcd.write(byte (7));
  lcd.setCursor(3, 1);

  if (menu == 0)
  {
    lcd.print("Start ");
  } else if (menu == 1)
  {
    lcd.print(" Start");
  } else if (menu == 2)
  {
    lcd.print(" Tare");
  } else if (menu == 3)
  {
    lcd.print(" Calibrate");
  } else if (menu == 4)
  {
    lcd.print(" System Info");
  }
  delay(100);
}

void capture(void)
{
  MsTimer2::stop();
  if (capturing)
  {
    //Checking the RCA state
    if (rcaState != lastRcaState) {       //Event generated by the RCA
      checkTimeOverflow();
      Serial.print(totalTime);
      Serial.print(";");

      if (rcaState) {
        Serial.println("R");
      } else {
        Serial.println("r");
      }
      lastRcaState = rcaState;

    } else {                             //If no RCA event, read the force as usual

      currentTime = micros();
      checkTimeOverflow();
      float measured = scale.get_units();
      
      currentPosition ++;
      if (currentPosition >= 90) currentPosition = 0;
      forces1s[currentPosition] = measured;

      //Calculating the average during 1s
      float sumForces = 0;
      for (int i = 0; i<90; i++){
        sumForces += forces1s[i];
      }

      meanForce1s = sumForces / 90;

      if (abs(meanForce1s) > abs(maxMeanForce1s)) maxMeanForce1s = meanForce1s;

      //RFD stuff start ------>
      if (rfdDataPre2Ok) {
        float rfdValue =  (measured - rfdMeasuredPre2) / ((elapsedTime + rfdTimePre) / 1000000.0);
        rfdCalculed = true;
        if (rfdValue > rfdValueMax) {
          rfdValueMax = rfdValue;
        }
      }

      if (rfdDataPreOk) {
        rfdTimePre2 = rfdTimePre;
        rfdMeasuredPre2 = rfdMeasuredPre;
        rfdDataPre2Ok = true;
      }

      rfdTimePre = elapsedTime;
      rfdMeasuredPre = measured;
      rfdDataPreOk = true;
      //<------- RFD stuff end

      if (abs(measured) > abs(measuredLcdDelayMax)) {
        measuredLcdDelayMax = measured;
      }
      if (abs(measured) > abs(measuredMax)) {
        measuredMax = measured;
      }

      Serial.print(totalTime); Serial.print(";");
      Serial.println(measured, 2); //scale.get_units() returns a float

      printOnLcd();
      //      if (rcaState) {
      //        end_capture();
      //      }
    }
    redButtonState = digitalRead(redButtonPin);
    if (redButtonState) {
      end_capture();
    }
  }
  MsTimer2::start();
}

void printOnLcd() {
  lcdCount = lcdCount + 1;
  if (lcdCount >= lcdDelay)
  {
    lcd.clear();
    showBatteryLevel();

    printLcdFormat (measuredLcdDelayMax, 4, 0, 1);

    //printLcdFormat (measuredMax, 4, 1, 1);
    printLcdFormat (maxMeanForce1s, 4, 1, 1);
    int totalTimeInSec = totalTime / 1000000;
    printLcdFormat (totalTimeInSec, 10, 0, 0);

    if (rfdCalculed) {
      printLcdFormat (rfdValueMax, 13, 1, 1);

      measuredLcdDelayMax = 0;
      lcdCount = 0;
    }

    if (showRecordChar) {
      lcd.createChar(7, recordChar);
      lcd.setCursor(0, 0);
      lcd.write(byte (7));
      showRecordChar = false;
    } else if (!showRecordChar) {
      lcd.setCursor(0, 0);
      lcd.print(" ");
      showRecordChar = true;
    }
  }
}


void printLcdFormat (float val, int xStart, int y, int decimal) {

  /*How many characters are to the left of the units number.
     Examples:
     1.23   -> 0 charachters
     12.34  -> 1 characters
     123.45 -> 2 characters
  */

  int valLength = floor(log10(abs(val)));

  // Adding the extra characters to the left
  if (valLength > 0) {
    xStart -= valLength;
  }

  // In negatives numbers the units are in the same position and the minus one position to the left
  if (val < 0) {
    xStart--;
  }
  lcd.setCursor(xStart  , y);
  lcd.print(val, decimal);
}

//void kangaroo() {
//  byte kangaroo1[] = {
//    B00000,
//    B00000,
//    B00000,
//    B10001,
//    B11011,
//    B01110,
//    B00100,
//    B00000
//  };
//  byte kangaroo2[] = {
//    B00110,
//    B01111,
//    B11111,
//    B11111,
//    B01000,
//    B01100,
//    B00100,
//    B00110
//  };
//  byte kangaroo3[] = {
//    B01000,
//    B00100,
//    B11110,
//    B11111,
//    B11000,
//    B01000,
//    B10000,
//    B00000
//  };
//  lcd.createChar(0, kangaroo1);
//  lcd.setCursor(13, 0);
//  lcd.write(byte (0));
//  lcd.createChar(1, kangaroo2);
//  lcd.setCursor(14, 0);
//  lcd.write(byte(1));
//  lcd.createChar(2, kangaroo3);
//  lcd.setCursor(15, 0);
//  lcd.write(byte(2));
//}


void serialEvent() {
  String inputString = Serial.readString();
  String commandString = inputString.substring(0, inputString.lastIndexOf(":"));
  //  while (Serial.available())
  //  {
  //    char inChar = (char)Serial.read();
  //    inputString += inChar;
  //    if (inChar == '\n') {
  //       commandString = inputString.substring(0, inputString.lastIndexOf(":"));
  //    }




  if (commandString == "start_capture") {
    start_capture();
    //capture();
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
  } else if (commandString == "get_transmission_format") {
    get_transmission_format();
  } else if (commandString == "send_sync_signal") {
    sendSyncSignal();
  } else if (commandString == "listen_sync_signal") {
    listenSyncSignal();
  } else {
    Serial.println("Not a valid command");
  }
  inputString = "";

}

void start_capture()
{
  Serial.println("Starting capture...");
  totalTime = 0;
  lastTime = micros();
  measuredMax = 0;
  //samples = 0;
  rfdDataPreOk = false;
  rfdDataPre2Ok = false;
  rfdCalculed = false;
  rfdValueMax = 0;

  //filling the array of forces with initial force
  float measured = scale.get_units();
  for (int i; i< 90; i++){
    forces1s[i] = measured;
  }
  
  capturing = true;
  delay(500);
  capturing = true;
}

void end_capture()
{
  capturing = false;
  Serial.print("Capture ended:");
  Serial.println(scale.get_offset());
  lcd.setCursor(0, 0);
  //  lcd.write(" ");
  //  Serial.print("Menu =");
  //  Serial.println(menu);
  showMenu();
  //lcd.print("Red: Start");
  delay(500);
}

void get_version()
{
  Serial.print(device);
  Serial.print("-");
  Serial.println(version);
}

void get_calibration_factor()
{
  Serial.println(scale.get_scale());
}

void set_calibration_factor(String inputString)
{
  //Reading the argument of the command. Located within the ":" and the ";"
  String calibration_factor = get_command_argument(inputString);
  //Serial.println(calibration_factor.toFloat());
  scale.set_scale(calibration_factor.toFloat());
  float stored_calibration = 0.0f;
  EEPROM.get(calibrationAddress, stored_calibration);
  if (stored_calibration != calibration_factor.toFloat()) {
    EEPROM.put(calibrationAddress, calibration_factor.toFloat());
  }
  Serial.println("Calibration factor set");
}

void calibrate(String inputString)
{
  //Reading the argument of the command. Located within the ":" and the ";"
  String weightString = get_command_argument(inputString);
  float weight = weightString.toFloat();
  //mean of 255 values comming from the cell after resting the offset.
  double offsetted_data = scale.get_value(50);

  //offsetted_data / calibration_factor
  float calibration_factor = offsetted_data / weight / 9.81; //We want to return Newtons.
  scale.set_scale(calibration_factor);
  EEPROM.put(calibrationAddress, calibration_factor);
  Serial.print("Calibrating OK:");
  Serial.println(calibration_factor);
}

void tare()
{
  lcd.print("taring");
  scale.tare(50); //Reset the scale to 0 using the mean of 255 raw values
  EEPROM.put(tareAddress, scale.get_offset());
  Serial.print("Taring OK:");
  Serial.println(scale.get_offset());


  lcd.setCursor(2, 1);
  lcd.print("Tared ");
  delay(300);
  showMenu();
  //lcd.print("Red: Start");
}

void get_tare()
{
  Serial.println(scale.get_offset());
}

void set_tare(String inputString)
{
  String tare = get_command_argument(inputString);
  long value = tare.toInt();
  scale.set_offset(value);
  long stored_tare = 0;
  EEPROM.get(tareAddress, stored_tare);
  if (stored_tare != value) {
    EEPROM.put(tareAddress, value);
    Serial.println("updated");
  }
  Serial.println("Tare set");
}

String get_command_argument(String inputString)
{
  return (inputString.substring(inputString.lastIndexOf(":") + 1, inputString.lastIndexOf(";")));
}

void get_transmission_format()
{
  if (binaryFormat)
  {
    Serial.println("binary");
  } else
  {
    Serial.println("text");
  }
}

void changingRCA() {
  //TODO: Check the overflow of the lastTriggerTime
  detachInterrupt(digitalPinToInterrupt(rcaPin));
  currentTime = micros();

  rcaState = digitalRead(rcaPin);

  attachInterrupt(digitalPinToInterrupt(rcaPin), changingRCA, CHANGE);
}

void checkTimeOverflow() {

  //Managing the timer overflow
  if (currentTime > lastTime)     //No overflow
    elapsedTime = currentTime - lastTime;
  else  if (currentTime <= lastTime) //Overflow
    elapsedTime = (4294967295 - lastTime) + currentTime; //Time from the last measure to the overflow event plus the currentTime

  //calculations
  totalTime += elapsedTime;
  lastTime = currentTime;
}

void sendSyncSignal() {
  pinMode(rcaPin, OUTPUT);

  syncTime = micros();

  digitalWrite(rcaPin, HIGH);
  delay(200);
  digitalWrite(rcaPin, LOW);

  sendSyncTime = true;

  pinMode(rcaPin, INPUT);
}

void listenSyncSignal() {
  //detachInterrupt(digitalPinToInterrupt(rcaPin));
  attachInterrupt(digitalPinToInterrupt(rcaPin), getSyncTime, FALLING);
  Serial.println("listening sync signal");
}

void getSyncTime() {
  syncTime = micros();
  sendSyncTime = true;
  //detachInterrupt(digitalPinToInterrupt(rcaPin));
  attachInterrupt(digitalPinToInterrupt(rcaPin), changingRCA, FALLING);
}

void calibrateLCD(void) {
  String weights[] = {"10", "20", "30", "40"};
  short int submenu = 0;
  bool exitFlag = false;
  String calibrateCommand = "calibrate:";
  showCalibrateMenu(weights[submenu]);
  redButtonState = digitalRead(redButtonPin);
  while (!exitFlag) {
    if (redButtonState) {
      Serial.println("Red pressed");
      calibrateCommand = calibrateCommand + weights[submenu] + ";";
      calibrate(calibrateCommand);
      showMenu();
      exitFlag = true;
    }
    if (blueButtonState) {
      submenu++;
      submenu = submenu % 5;
      showCalibrateMenu(weights[submenu]);
    }
    redButtonState = digitalRead(redButtonPin);
    blueButtonState = digitalRead(blueButtonPin);
  }
  Serial.println("Exit bucle");
  delay(1000);
}

void showCalibrateMenu(String weight) {
  lcd.setCursor(3, 0);
  lcd.print("Calibrate ");
  lcd.print(weight);
  lcd.print(" kg");
  lcd.setCursor(2, 1);
  lcd.print("Change Weight");
  delay(500);
}

void showBatteryLevel() {
  float sensorValue = analogRead(A0);
  lcd.setCursor(0, 0);
  if (sensorValue >= 788) {
    lcd.createChar(5, battery5);
    lcd.write(byte(5));
  } else if (sensorValue < 788 && sensorValue >= 759) {
    lcd.createChar(4, battery4);
    lcd.write(byte(4));
  } else if (sensorValue < 759 && sensorValue >= 730) {
    lcd.createChar(3, battery3);
    lcd.write(byte(3));
  } else if (sensorValue < 730 && sensorValue >= 701) {
    lcd.createChar(2, battery2);
    lcd.write(byte(2));
  } else if (sensorValue < 701 && sensorValue >= 672) {
    lcd.createChar(1, battery1);
    lcd.write(byte(1));
  } else if (sensorValue <= 701) {
    lcd.createChar(0, battery0);
    lcd.write(byte (0));
  }
}

void showSystemInfo() {
  MsTimer2::stop();
  lcd.clear();
  lcd.setCursor(2, 0);
  lcd.print("Ver: ");
  lcd.print(version);
  lcd.setCursor(2, 1);
  lcd.print("Submenu: ");
  lcd.print(submenu);
  delay(1000);
  redButtonState = digitalRead(redButtonPin);
  submenu = 0;
  while (!redButtonState) {
    blueButtonState = digitalRead(blueButtonPin);
    if (blueButtonState) {
      delay(200);
      submenu++;
      submenu = submenu % 3;
      if (submenu == 0) {
        lcd.setCursor(2, 0);
        lcd.print("Ver: ");
        lcd.print(version);
        lcd.setCursor(2, 1);
        lcd.print("Submenu: ");
        lcd.print(submenu);
      } else if (submenu == 1) {
        lcd.setCursor(2, 1);
        lcd.print("Submenu: ");
        lcd.print(submenu);
      } else if (submenu == 2) {
        lcd.setCursor(2, 1);
        lcd.print("Submenu: ");
        lcd.print(submenu);
      }
    }
    redButtonState = digitalRead(redButtonPin);
  }
  MsTimer2::start();
}
