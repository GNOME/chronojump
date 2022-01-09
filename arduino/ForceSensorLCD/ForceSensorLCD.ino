

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
//String device = "Force_Sensor";
String version = "0.7";


int tareAddress = 0;
int calibrationAddress = 4;

HX711 scale(DOUT, CLK);

//Data comming from the cell after resting the offset weight
float offsetted_data = 0;

//Data resulting of appying the calibration_factor to the offsetted_data
float scaled_data = 0;

////The weight used to calibrate the cell
//float weight = 0.0;

//Wether the sensor has to capture or not
boolean capturing = false;

////Wether the sync time must be sent or not
//bool sendSyncTime = false;

//wether the tranmission is in binary format or not
boolean binaryFormat = false;

unsigned long lastTime = 0;

//RFD variables
//for RFD cannot used lastTime, can have overflow problems. Better use elapsedTime
/* Old method)
unsigned long timePre = 0;
unsigned long timePre2 = 0;
float measuredPre = 0;
float measuredPre2 = 0;
bool dataPreOk = false;
bool dataPre2Ok = false;
bool rfdCalculed = false;
float rfdValueMax = 0;
*/
float RFD200 = 0.0;   //average RFD during 200 ms
float RFD100 = 0.0;  //average RFD during 100 ms
float maxRFD200 = 0.0;   //Maximim average RFD during 200 ms
float maxRFD100 = 0.0;  //Maximim average RFD during 100 ms
bool elapsed200 = false;  //Wether it has passed 200 ms from the start
bool elapsed100 = false;  //Wether it has passed 100 ms from the start


unsigned long currentTime = 0;
unsigned long elapsedTime = 0;
unsigned long totalTime = 0;
//unsigned long syncTime = 0;
//unsigned int samples = 0;

const int redButtonPin = 13;
bool redButtonState;

const int blueButtonPin = 6;
bool blueButtonState;

//TODO. Manage it with timer interruptions
unsigned short lcdDelay = 25; //to be able to see the screen. Seconds are also printed in delay but 25 values are less than one second
unsigned short lcdCount = 0;
float measuredLcdDelayMax = 0; //The max in the lcdDelay periodca
float measuredMax = 0; // The max since starting capture
float measured = scale.get_units();


/***** Atention!!! *****
 *  lcd.createChar() function makes a mess with the cursor position and it must be specified
 *  again the cursos with lcd.setCursor()
 */


byte downArrow[] = {
  B00000,
  B00000,
  B00000,
  B10000,
  B01001,
  B00101,
  B00011,
  B01111
};

byte upArrow[] = {
  B01111,
  B00011,
  B00101,
  B01001,
  B10000,
  B00000,
  B00000,
  B00000
};

byte exitChar[] = {
  B11111,
  B10001,
  B10101,
  B10101,
  B00100,
  B10101,
  B01110,
  B00100
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

const short rcaPin = 3;

unsigned long triggerTime = 0;
bool rcaState = digitalRead(rcaPin);
bool lastRcaState = rcaState;

unsigned short menu = 0;
unsigned short submenu = 0;

//TODO. Try to eliminate spaces and the number to save memory
const String menuList [] = {
  "1-Measure",
  "2-TareMeasure",
  "3-Tare",
  "4-Calibrate",
  "5-System"
};

//Mean force in 1s

//Array where all measures in 1s are stored
//ADC has 84.75 ~ 85 samples/second. If we need the diference in 1 second we need 1 more sample
unsigned short freq = 86;
//Cirtular buffer of measures
float forces1s[86];
//200 ms -> 16.95 ~ 17 samples. To know the elapsed time in 200 ms we need 1 more sample
short int samples200ms = 18;
short int samples100ms = 9;
//Circular buffer of times
unsigned long totalTimes1s[18];
//The current position in the array
unsigned short currentFPosition = 0;
unsigned short currentTPosition = 0;
//mean force during 1s
float meanForce1s;
float maxMeanForce1s = 0.0;

//Variability
float sumSSD = 0.0;
float sumMeasures = 0.0;
unsigned int samplesSSD = 0;
float RMSSD = 0.0;
float cvRMSSD = 0.0;
//bool calculeVariability = true;
float lastMeasure = 0;

//Impulse
float impulse = 0;
bool elapsed1Sample = false;

//Force in trigger
float forceTrigger = 0.0;

//If it is controled by computer don't show results
bool PCControlled = false;

void setup() {
  pinMode(redButtonPin, INPUT);
  pinMode(blueButtonPin, INPUT);
//  analogWrite(6, 20);
  lcd.begin(16, 2);

  Serial.begin(115200);

  attachInterrupt(digitalPinToInterrupt(rcaPin), changingRCA, CHANGE);

  if (redButtonState == 0) {
    lcd.setCursor(2, 0);
    lcd.print("CHRONOJUMP");
    lcd.setCursor(2, 1);
    lcd.print("Boscosystem");
    delay(1000);
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
      redButtonState = false;
      if (menu == 0)
      {
        lcd.clear();
        lcd.setCursor(0, 0);
        lcd.print("Starting capture");
        PCControlled = false;
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
    }
  } else
  {
    capture();
  }
}

void showMenu(void)
{
  lcd.clear();
  showBatteryLevel();
  lcd.setCursor(0, 0);
  lcd.print(menuList[menu]);
  lcd.setCursor(14,0);
  lcd.print(menu + 1);
  lcd.createChar(6, upArrow);
  lcd.setCursor(15, 0);
  lcd.write(byte (6));
  lcd.createChar(7, downArrow);
  lcd.setCursor(15, 1);
  lcd.write(byte (7));

  if (menu == 0)
  {
    lcd.setCursor(10, 1);
    lcd.print("Start");
  } else if (menu == 1)
  {
    lcd.setCursor(5, 1);
    lcd.print("Tare&Start");
  } else if (menu == 2)
  {
    lcd.setCursor(10, 1);
    lcd.print("Start");
  } else if (menu == 3)
  {
    lcd.setCursor(2, 1);
    lcd.print("SetLoad&Start");
  } else if (menu == 4)
  {
    lcd.setCursor(11, 1);
    lcd.print("Show");
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
        forceTrigger = measured;
      } else {
        Serial.println("r");
      }
      lastRcaState = rcaState;

    } else {                             //If no RCA event, read the force as usual
      currentTime = micros();
      checkTimeOverflow();
      measured = scale.get_units();

      //When currentFPosition is > freq +1 it starts over to 0
      currentFPosition = (currentFPosition + 1) % freq;
      currentTPosition = (currentTPosition + 1) % samples200ms;
//      Serial.print(currentTPosition); Serial.print("\t");
      if(currentTPosition > 0) elapsed1Sample = true;
      if(currentTPosition >= (samples200ms -1)) elapsed200 = true;
      if(currentTPosition >= (samples100ms -1)) elapsed100 = true;
      
      forces1s[currentFPosition] = measured;
      totalTimes1s[currentTPosition] = totalTime;

      //Calculating the average during 1s
      float sumForces = 0;
      for (int i = 0; i < freq; i++) {
        sumForces += forces1s[i];
      }

      meanForce1s = sumForces / freq;

      if (abs(meanForce1s) > abs(maxMeanForce1s)) maxMeanForce1s = meanForce1s;

//      if (calculeVariability) {
        sumSSD += (sq(measured - lastMeasure));
        sumMeasures += measured;
        samplesSSD++;
        lastMeasure = measured;
        RMSSD = sqrt(sumSSD / (samplesSSD - 1));
        cvRMSSD = 100 * RMSSD / ( sumMeasures / samplesSSD);
//      }

      //RFD stuff start ------>
      /* Old method
       *  
      if (dataPre2Ok) {
        float rfdValue =  (measured - measuredPre2) / ((totalTime - timePre2) / 1e6.0);
        rfdCalculed = true;
        if (rfdValue > rfdValueMax) {
          rfdValueMax = rfdValue;
        }
      }

      if (dataPreOk) {
        impulse += ((measured + measuredPre) * (totalTime - timePre) / 1e6.0 / 2);
        timePre2 = timePre;
        measuredPre2 = measuredPre;
        dataPre2Ok = true;
      }

      timePre = totalTime;
      measuredPre = measured;
      dataPreOk = true;
      */

      if(elapsed1Sample){
        impulse += (((measured + forces1s[(currentFPosition + freq - 1) % freq])  / 2) *      //Mean force between 2 samples
          (totalTime - totalTimes1s[(currentTPosition + samples200ms - 1) % samples200ms]) / 1e6);  //Elapsed time between 2 samples
      }

      //To go backwards N position use [currentFPosition + TotalPositions - N]
      if(elapsed200){
        //Increment of the force in 200ms
        RFD200 = (measured - forces1s[(currentFPosition + freq - samples200ms) % freq]) /
          ((totalTime - totalTimes1s[(currentTPosition + 1) % samples200ms]) / 1e6);
//          Serial.print(currentTPosition);
//          Serial.print("\t");
//          Serial.print((currentTPosition + 1) % samples200ms);
//          Serial.print("\t");
//          Serial.print(totalTime);
//          Serial.print("\t");
//          Serial.print((totalTimes1s[(currentTPosition + 1) % samples200ms]));
//          Serial.print("\t");
//          Serial.print((totalTime - totalTimes1s[(currentTPosition + 1) % samples200ms]) / 1e6);
//          Serial.print("\t");
//          Serial.print(totalTime); Serial.print("\t");
//          Serial.print(totalTimes1s[(currentTPosition + samples200ms - 17) % freq]); Serial.print("\t");
//          Serial.print(RFD200); Serial.print("\t");
        if(abs(maxRFD200) < abs(RFD200)) maxRFD200 = RFD200;
      }

      if(elapsed100){
        //Increment of the force in 100ms
        RFD100 = (measured - forces1s[(currentFPosition + freq - samples100ms) % freq]) /
          ((totalTime - totalTimes1s[(currentTPosition + samples200ms - samples100ms) % samples200ms]) / 1e6);
//          Serial.print(RFD100); Serial.print("\t");
        if(abs(maxRFD100) < abs(RFD100)) maxRFD100 = RFD100;
      }

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
//  showResults();
}

void printOnLcd() {
  lcdCount = lcdCount + 1;
  if (lcdCount >= lcdDelay)
  {
    lcd.clear();
//    showBatteryLevel();

    printLcdFormat (measuredLcdDelayMax, 4, 0, 1);

    //printLcdFormat (measuredMax, 4, 1, 1);
    printLcdFormat (maxMeanForce1s, 4, 1, 1);
    int totalTimeInSec = totalTime / 1e6;
    printLcdFormat (totalTimeInSec, 15, 0, 0);

    /* Old method
    if (rfdCalculed) {
      printLcdFormat (rfdValueMax, 13, 1, 1);
      printLcdFormat (cvRMSSD, 13, 1, 1);

      measuredLcdDelayMax = 0;
      lcdCount = 0;
    }
    */

      printLcdFormat (impulse, 13, 1, 1);

      measuredLcdDelayMax = 0;
      lcdCount = 0;

//    if (showRecordChar) {
//      lcd.createChar(7, downArrow);
//      lcd.setCursor(0, 0);
//      lcd.write(byte (7));
//      showRecordChar = false;
//    } else if (!showRecordChar) {
//      lcd.setCursor(0, 0);
//      lcd.print(" ");
//      showRecordChar = true;
//    }
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
    PCControlled = true;
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
//  } else if (commandString == "send_sync_signal") {
//    sendSyncSignal();
//  } else if (commandString == "listen_sync_signal") {
//    listenSyncSignal();
  } else {
    Serial.println("Not a valid command");
  }
  inputString = "";

}

void start_capture()
{
  
  MsTimer2::stop();
  Serial.println("Starting capture...");
  totalTime = 0;
  lastTime = micros();
  measuredMax = 0;
  //samples = 0;
  /* old method
  dataPreOk = false;
  dataPre2Ok = false;
  rfdCalculed = false;
  rfdValueMax = 0;
  */
  impulse = 0;

  //filling the array of forces ant times with initial force
  lastMeasure = scale.get_units();
  for (int i; i < freq; i++) {
    forces1s[i] = lastMeasure;
  }

  for (short int i; i < samples200ms; i++) {
    totalTimes1s[i] = 0;
  }
  
  maxMeanForce1s = lastMeasure;

  //Initializing variability variables
  sumSSD = 0.0;
  sumMeasures = lastMeasure;
  samplesSSD = 0;
  lcd.clear();
  capturing = true;
}

void end_capture()
{
  capturing = false;
  Serial.println("Capture ended:");
//  Serial.println(scale.get_offset());
  delay(500);
  if(!PCControlled){   
    lcd.clear();
    lcd.setCursor(4, 0);
    lcd.print("Results:");
    showResults();
  }
  MsTimer2::start();
  showMenu();
}

void get_version()
{
  Serial.print("Force_Sensor-");
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
  lcd.clear();
  lcd.setCursor(3,0);
  lcd.print("Taring...");
  scale.tare(50); //Reset the scale to 0 using the mean of 255 raw values
  EEPROM.put(tareAddress, scale.get_offset());
  Serial.print("Taring OK:");
  Serial.println(scale.get_offset());


  lcd.setCursor(3,0);
  lcd.print("  Tared  ");
  delay(200);
  showMenu();
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

//void sendSyncSignal() {
//  pinMode(rcaPin, OUTPUT);
//
//  syncTime = micros();
//
//  digitalWrite(rcaPin, HIGH);
//  delay(200);
//  digitalWrite(rcaPin, LOW);
//
//  sendSyncTime = true;
//
//  pinMode(rcaPin, INPUT);
//}
//
//void listenSyncSignal() {
//  //detachInterrupt(digitalPinToInterrupt(rcaPin));
//  attachInterrupt(digitalPinToInterrupt(rcaPin), getSyncTime, FALLING);
//  Serial.println("listening sync signal");
//}
//
//void getSyncTime() {
//  syncTime = micros();
//  sendSyncTime = true;
//  //detachInterrupt(digitalPinToInterrupt(rcaPin));
//  attachInterrupt(digitalPinToInterrupt(rcaPin), changingRCA, FALLING);
//}

void calibrateLCD(void) {
  int weight = 5;
  submenu = 0;
  bool exitFlag = false;
  String calibrateCommand = "calibrate:";
  showCalibrateLoad(String(weight, DEC));
  redButtonState = false;
  while (!exitFlag) {
    if (submenu == 0) {

      if (redButtonState) {
        weight += 5;
        Serial.println("Red pressed");
        showCalibrateLoad(String(weight, DEC));
        calibrateCommand = calibrateCommand + String(weight, DEC) + ";";
        delay(200);
      }
      if (blueButtonState) {
        //Change to Calibrate execution
        lcd.clear();
        lcd.setCursor(9, 0);
        lcd.print("Cancel");
        lcd.setCursor(10,1);
        lcd.print("Start");
        submenu = 1;
        Serial.println(submenu);
        blueButtonState = false;
        delay(200);
      }
    }
    
    if (submenu == 1) {
      if (redButtonState) {
        lcd.clear();
        lcd.setCursor(1,0);
        lcd.print("Calibrating...");
        calibrate(calibrateCommand);
        lcd.clear();
        lcd.setCursor(2,0);
        lcd.print("Calibrated...");
        exitFlag = true;
        delay(200);
      }
      if (blueButtonState) {
          exitFlag = true;
      }
    }
    
    redButtonState = digitalRead(redButtonPin);
    blueButtonState = digitalRead(blueButtonPin);
  }

//  Serial.println("Exit bucle");F
  delay(1000);
  showMenu();
}

void showCalibrateLoad(String weight) {
  lcd.clear();
  lcd.setCursor(3, 0);
  lcd.print("Set load");
  lcd.setCursor(15, 0);
  lcd.print(">");
  lcd.setCursor(0,1);
  lcd.print("Current:" );
  lcd.print(weight);
  lcd.setCursor(14, 1);
  lcd.print("+5");
  delay(200);
}

void showBatteryLevel() {
  float sensorValue = analogRead(A0);
  if (sensorValue >= 788) {
    lcd.createChar(0, battery5);
  } else if (sensorValue < 788 && sensorValue >= 759) {
    lcd.createChar(0, battery4);
  } else if (sensorValue < 759 && sensorValue >= 730) {
    lcd.createChar(0, battery3);
  } else if (sensorValue < 730 && sensorValue >= 701) {
    lcd.createChar(0, battery2);
  } else if (sensorValue < 701 && sensorValue >= 672) {
    lcd.createChar(0, battery1);
  } else if (sensorValue <= 701) {
    lcd.createChar(0, battery0);
  }
  lcd.setCursor(0, 1);
  lcd.write(byte (0));
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

void showResults(){
  int submenu = 4;
  redButtonState = false;
  
  lcd.clear();
  lcd.setCursor(15, 0);
  lcd.print(">");
  lcd.createChar(7, exitChar);
  lcd.setCursor(15, 1);
  lcd.write(byte (7));
  

  lcd.setCursor(0,0);
  lcd.print("Fmax ");
  printLcdFormat(measuredMax, 11, 0, 1);
  lcd.setCursor(0,1);
  lcd.print("Fmax1s ");
  printLcdFormat(maxMeanForce1s, 11, 1, 1);
  while(!redButtonState){    
    blueButtonState = digitalRead(blueButtonPin);
    redButtonState = digitalRead(redButtonPin);
    if(blueButtonState){
      blueButtonState = false;
      submenu = (submenu + 1) % 4;
      lcd.clear();
      if (submenu == 0) {
        lcd.setCursor(0,0);
        lcd.print("Fmax ");
        printLcdFormat(measuredMax, 11, 0, 1);
        lcd.setCursor(0,1);
        lcd.print("Fmax1s ");
        printLcdFormat(maxMeanForce1s, 11, 1, 1);
      } else if(submenu == 1) {
        lcd.setCursor(0,0);
        lcd.print("Ftrg ");
        printLcdFormat(forceTrigger, 11, 0, 1);
        lcd.setCursor(0,1);
        lcd.print("Impulse ");
        printLcdFormat(impulse, 11, 1, 1);
      } else if(submenu == 2) {
        lcd.setCursor(0,0);
        lcd.print("RFD100 ");
        printLcdFormat(maxRFD100, 11, 0, 1);
        lcd.setCursor(0,1);
        lcd.print("RFD200  ");
        printLcdFormat(maxRFD200, 11, 1, 1);
      } else if (submenu >=3) {
        lcd.setCursor(0,0);
        lcd.print("RMSSD ");
        printLcdFormat(RMSSD, 11, 0, 1);
        lcd.setCursor(0,1);
        lcd.print("cvRMSSD  ");
        printLcdFormat(cvRMSSD, 11, 1, 1);
      }
      delay(200);
      lcd.setCursor(15, 0);
      lcd.print(">");
      lcd.setCursor(15, 1);
      lcd.write(byte (7));
    }
  }
  redButtonState = false;
  delay(200);
}
