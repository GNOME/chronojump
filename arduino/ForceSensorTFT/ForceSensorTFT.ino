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

#include <EEPROM.h>
#include "SPI.h"
#include "ILI9341_t3.h"
#include "HX711.h"
#include <Bounce2.h>
#include <Encoder.h>
#include <SD.h>
#include <elapsedMillis.h>

#define DOUT  2
#define CLK  3

//Version number //it always need to start with: "Force_Sensor-"
//Device commented for memory optimization
//String device = "Force_Sensor";
String version = "0.7";


//Encoder variables
Encoder encoder(8, 9);
int lastPosition = 0;
short encoderPhase = 1;    // 1 means concentric, -1 means eccentric
long startPhasePosition = 0;
long startPhaseTime = 0;
float avgVelocity = 0;
float maxAvgVelocity = 0;
int numRepetitions = 0;

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

//Variables to capture
enum sensorType {
  loadCell,
  incEncoder,
  loadCellIncEncoder,
  none
};

sensorType sensor = none;
String maxString = "";

////Wether the sync time must be sent or not
//bool sendSyncTime = false;

//wether the tranmission is in binary format or not
boolean binaryFormat = false;

//RFD variables
float RFD200 = 0.0;   //average RFD during 200 ms
float RFD100 = 0.0;  //average RFD during 100 ms
float maxRFD200 = 0.0;   //Maximim average RFD during 200 ms
float maxRFD100 = 0.0;  //Maximim average RFD during 100 ms
bool elapsed200 = false;  //Wether it has passed 200 ms since the start
bool elapsed100 = false;  //Wether it has passed 100 ms since the start


unsigned long rcaTime = 0;  //Time at which RCA changed
elapsedMicros totalTime = 0;
unsigned long lastSampleTime;

/* Not used in order to optimize memory
  //Used to sync 2 evices
  unsigned long syncTime = 0;
  unsigned int samples = 0;
*/

const unsigned short redButtonPin = 4;
Bounce redButton = Bounce(redButtonPin, 50);

const unsigned short blueButtonPin = 5;
Bounce blueButton = Bounce(blueButtonPin, 50);

//TODO. Manage it with timer interruptions
//unsigned short lcdDelay = 25; //to be able to see the screen. Seconds are also printed in delay but 25 values are less than one second
//unsigned short lcdCount = 0;
//float measuredLcdDelayMax = 0; //The max in the lcdDelay periodca
float measuredMax = 0; // The max since starting capture
float measuredMin = 0;
float measured = 0;
double newGraphMax = measuredMax;
double newGraphMin = measuredMin;
double graphMin = measuredMin;
double graphMax = measuredMax;


const unsigned short rcaPin = 3;

unsigned long triggerTime = 0;        //The instant in which the trigger signal is activated
bool rcaState = digitalRead(rcaPin);  //Wether the RCA is shorted or not
bool lastRcaState = rcaState;         //The previous state of the RCA

unsigned short menu = 0;              //Main menu state
unsigned short submenu = 0;           //submenus state

const String menuList [] = {
  "Raw Force",
  "Raw Velocity",
  "RawPower",
  "Tared Force",
  "F. Steadiness",
  "System"
};

int numMenuItems = 6;

const String menuDescription [] = {
  "Shows standard graph of\nthe force and the summary of the set.\n(Maximum Force, RFD and\nImpulse)" ,
  "Show a standard graph of linear velocity",
  "Measure Force and Speed\nat the same time.\nOnly power is shown in thegraph",
  "Offset the force before\nmeasuring it.\nUseful to substract body\nweight.",
  "RMSSD and cvRMSSD.\nMeasure the steadyness\nof the force signal.\nAfter achieving the\ndesired steady force press\nRedButton to get the\nsteadiness of the next 5s.",
  "Performs calibration or\ntare and shows some system\ninformation."
};

const String systemOptions[] = {
  "Tare",
  "Calibrate",
  "Info",
};

const String systemDescriptions[] = {
  "Set the offset of the\nsensor",
  "Set the equivalence\nbetween the sensor values\nand the force measured",
  "Hardware information"
};

//Mean force in 1s
//Circular buffer where all measures in 1s are stored
//ADC has 84.75 ~ 85 samples/second. If we need the diference in 1 second we need 1 more sample
//TODO: Adjust values to 160Hz
unsigned int freq = 86;
//Cirtular buffer of force measures
float forces1s[86];
//200 ms -> 16.95 ~ 17 samples. To know the elapsed time in 200 ms we need 1 more sample
unsigned short samples200ms = 18;
unsigned short samples100ms = 9;
//Circular buffer of times
unsigned long totalTimes1s[18];
//The current slot in the array
unsigned short currentFSlot = 0;
unsigned short currentTSlot = 0;
//Mean force during the last second
float meanForce1s;
//Maximum mean force exerted during 1 second. It could be at any moment durint the capture
float maxMeanForce1s = 0.0;

//Variability
float sumSSD = 0.0;
float sumMeasures = 0.0;
unsigned int samplesSSD = 0;
float RMSSD = 0.0;
float cvRMSSD = 0.0;
float lastMeasure = 0;
bool capturingPreSteadiness = false;
bool capturingSteadiness = false;

//Impulse. Impulse = Sumatory of the force*time
float impulse = 0;
bool elapsed1Sample = false;    //Wether there's a previous sample. Needed to calculate impulse

//Force in trigger
float forceTrigger = 0.0;       //Measured force at the moment the RCA is triggered

//Power variables
float power = 0;
float maxPower = 0;

//If device is controled by computer don't show results on TFT
bool PCControlled = false;

long tareValue = 0;

// Display variables
const int chipSelect = 6;
#define TFT_DC      20
#define TFT_CS      21
#define TFT_RST    255  // 255 = unused, connect to 3.3V
#define TFT_MOSI     7
#define TFT_SCLK    14
#define TFT_MISO    12

ILI9341_t3 tft = ILI9341_t3(TFT_CS, TFT_DC, TFT_RST, TFT_MOSI, TFT_SCLK, TFT_MISO);
//Adafruit_ILI9341 tft = Adafruit_ILI9341(TFT_CS, TFT_DC, TFT_MOSI, TFT_SCLK, TFT_RST, TFT_MISO);
// Display 16-bit color values:
#define BLUE      0x025D
#define TEAL      0x0438
#define GREEN     0x07E0
#define CYAN      0x07FF
#define RED       0xF800
#define MAGENTA   0xF81F
#define YELLOW    0xFFE0
#define ORANGE    0xFC00
#define PINK      0xF81F
#define PURPLE    0x8010
#define GREY      0x4A49
#define WHITE     0xFFFF
#define BLACK     0x0000
#define CJCOLOR   0X1109

//Don't plot all the samples. values plotted every plotPeriod samples
int plotPeriod = 10;
boolean startOver = true;
double ox , oy ;
double x, y;

void setup() {
  pinMode(redButtonPin, INPUT_PULLUP);
  pinMode(blueButtonPin, INPUT_PULLUP);

  Serial.begin(256000);

  //Activate interruptions activated by any RCA state change
  attachInterrupt(digitalPinToInterrupt(rcaPin), changingRCA, CHANGE);

  EEPROM.get(tareAddress, tareValue);
  //If the arduino has not been tared the default value in the EEPROM is -151.
  //TODO: Check that it is stil true in the current models
  if (tareValue == -151) {
    scale.set_offset(10000);// Usual value  in Chronojump strength gauge
    EEPROM.put(tareAddress, 10000);
  } else {
    scale.set_offset(tareValue);
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

  //Start TFT
  tft.begin();
  tft.setRotation(1);
  tft.fillRect(0, 50, 320, 240, BLACK);
  drawMenuBackground();
  showMenu();
}

void loop()
{
  if (!capturing)
  {
    //The blue button navigates through the Menu options
    blueButton.update();
    if (blueButton.fallingEdge()) {
      menu++;
      menu = menu % numMenuItems;
      showMenu();
    }

    //The red button activates the menu option
    redButton.update();
    if (redButton.fallingEdge())
    {
      if (menu == 0)
      {
        PCControlled = false;
        startLoadCellCapture();
      } else if (menu == 1)
      {
        Serial.println("Velocity");
        startEncoderCapture();
      } else if (menu == 2)
      {
        startPowerCapture();
      } else if (menu == 3)
      {
        start_steadiness();
        startLoadCellCapture();

      } else if (menu == 4)
      {
        tareTemp();
        startLoadCellCapture();
      } else if (menu == 5)
      {
        systemMenu();
        menu = 0;
        showMenu();
      }
    }
  } else
  {
    capture();
  }

  //With Teensy serialEvent is not called automatically after loop
  if (Serial.available()) serialEvent();
}

void showMenu(void)
{
  tft.fillRect(30, 0, 260, 50, BLACK);
  tft.setCursor(40, 20);
  tft.setTextSize(3);
  tft.print(menuList[menu]);

  tft.setTextSize(2);
  tft.setCursor(12, 100);
  tft.setTextColor(BLACK);
  tft.print(menuDescription[(menu + numMenuItems - 1) % numMenuItems]);
  tft.setTextColor(WHITE);
  tft.setCursor(12, 100);
  tft.print(menuDescription[menu]);
}

void getLoadCellDynamics(void)
{
  measured = scale.get_units();

  //When current Force Slot is equal to size of the buffer it starts over to 0
  currentFSlot = (currentFSlot + 1) % freq;
  //wHEN current Time Slot is equal to the size of the buffer it starts over to 0
  currentTSlot = (currentTSlot + 1) % samples200ms;

  if (currentTSlot > 0) elapsed1Sample = true;    //There's a previous sample
  if (currentTSlot >= (samples200ms - 1)) elapsed200 = true;
  if (currentTSlot >= (samples100ms - 1)) elapsed100 = true;

  forces1s[currentFSlot] = measured;
  totalTimes1s[currentTSlot] = totalTime;

  //Calculating the average during 1s
  float sumForces = 0;
  for (unsigned short i = 0; i < freq; i++) {
    sumForces += forces1s[i];
  }

  //Mean forces = sum of the forces divided by the number of samples in 1 second
  meanForce1s = sumForces / freq;

  if (abs(meanForce1s) > abs(maxMeanForce1s)) maxMeanForce1s = meanForce1s;

  //In the final phase of steadiness measure. Actual calculation
  if (capturingSteadiness)
  {
    sumSSD += (sq(measured - lastMeasure));
    sumMeasures += measured;
    samplesSSD++;
    lastMeasure = measured;
    RMSSD = sqrt(sumSSD / (samplesSSD - 1));
    cvRMSSD = 100 * RMSSD / ( sumMeasures / samplesSSD);
    if (samplesSSD >= 5 * (freq - 1))
    {
      end_steadiness();
      endLoadCellCapture();
    }
  }


  //RFD stuff start ------>

  //To go backwards N slots use [currentSlot + TotalPositions - N]
  if (elapsed1Sample) {
    impulse += (((measured + forces1s[(currentFSlot + freq - 1) % freq])  / 2) *      //Mean force between 2 samples
                (totalTime - totalTimes1s[(currentTSlot + samples200ms - 1) % samples200ms]) / 1e6);  //Elapsed time between 2 samples
  }

  if (elapsed200) {
    RFD200 = (measured - forces1s[(currentFSlot + freq - samples200ms) % freq]) /     //Increment of the force in 200ms
             ((totalTime - totalTimes1s[(currentTSlot + 1) % samples200ms]) / 1e6);          //Increment of time
    if (RFD200 > maxRFD200) maxRFD200 = RFD200;
  }

  if (elapsed100) {
    RFD100 = (measured - forces1s[(currentFSlot + freq - samples100ms) % freq]) /     //Increment of the force in 200ms
             ((totalTime - totalTimes1s[(currentTSlot + samples200ms - samples100ms) % samples200ms]) / 1e6); //Increment of time
    if (RFD100 > maxRFD100) maxRFD100 = RFD100;
  }
}

void printTftFormat (float val, int xStart, int y, int fontSize, int decimal) {

  /*How many characters are to the left of the units number.
     Examples:
     1.23   -> 0 charachters
     12.34  -> 1 characters
     123.45 -> 2 characters
  */

  //Font sizes: 5x8, 10x16, 15x24, or 20x32
  //Theres a pixel between characters
  int charWidth = 5 * fontSize + 1;
  int valLength = floor(log10(abs(val)));

  // Adding the extra characters to the left
  if (valLength > 0) {
    xStart = xStart - valLength * charWidth;
  }

  // In negatives numbers the units are in the same position and the minus one position to the left
  if (val < 0) {
    xStart = xStart - charWidth;
  }
  tft.setTextSize(fontSize);
  tft.setCursor(xStart , y);
  tft.print(val, decimal);
}

void serialEvent() {
  String inputString = Serial.readString();
  String commandString = inputString.substring(0, inputString.lastIndexOf(":"));


  if (commandString == "start_capture") {
    PCControlled = true;
    startLoadCellCapture();
    //capture();
  } else if (commandString == "end_capture") {
    endLoadCellCapture();
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
    /* Commented due to memory optimization
      //  } else if (commandString == "send_sync_signal") {
      //    sendSyncSignal();
      //  } else if (commandString == "listen_sync_signal") {
      //    listenSyncSignal();
    */
  } else {
    Serial.println("Not a valid command");
  }
  inputString = "";

}

void startLoadCellCapture()
{
  Serial.println("Starting capture...");
  totalTime = 0;
  measuredMax = scale.get_units();
  impulse = 0;
  maxRFD100 = 0;
  maxRFD200 = 0;

  //filling the array of forces ant times with initial force
  lastMeasure = scale.get_units();
  for (unsigned short i = 0; i < freq; i++) {
    forces1s[i] = lastMeasure;
  }

  for (short i = 0; i < samples200ms; i++) {
    totalTimes1s[i] = 0;
  }

  maxMeanForce1s = lastMeasure;

  //Initializing variability variables
  sumSSD = 0.0;
  sumMeasures = lastMeasure;
  samplesSSD = 0;
  capturing = true;
  sensor = loadCell;
  maxString = "Fmax:        N";
  plotPeriod = 5;
  newGraphMin = -100;
  newGraphMax = 300;
}

void endLoadCellCapture()
{
  capturing = false;
  sensor = none;
  Serial.println("Capture ended:");

  //If the device is controlled by the PC the results menu is not showed
  //because during the menu navigation the Serial is not listened.
  if (!PCControlled) {
    //Restoring tare value in the EEPROM. Necessary after Tare&Capture
    EEPROM.get(tareAddress, tareValue);
    scale.set_offset(tareValue);
    //Serial.println(scale.get_offset());
    showLoadCellResults();
  }

  showMenu();
}

void get_version()
{
  //Device string not in a variable due to memory optimization
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
  tft.setCursor(120, 100);
  tft.print("Taring...");
  scale.tare(50); //Reset the scale to 0 using the mean of 255 raw values
  EEPROM.put(tareAddress, scale.get_offset());
  tft.setTextColor(BLACK);
  tft.setCursor(120, 100);
  tft.print("Taring...");
  tft.setTextColor(WHITE);
  tft.setCursor(120, 100);
  tft.print("Tared");


  Serial.print("Taring OK:");
  Serial.println(scale.get_offset());

  delay(300);
  tft.setTextColor(BLACK);
  tft.setCursor(120, 100);
  tft.print("Tared");
}

void tareTemp()
{
  tft.setTextSize(2);
  tft.setCursor(12, 100);
  tft.setTextColor(BLACK);
  tft.print(menuDescription[1]);
  tft.setTextColor(WHITE);
  tft.setCursor(100, 100);
  tft.print("Taring...");
  scale.tare(50); //Reset the scale to 0 using the mean of 255 raw values
  tft.setTextColor(BLACK);
  tft.setCursor(100, 100);
  tft.print("Taring...");
  tft.setTextColor(WHITE);
  tft.setCursor(100, 100);
  tft.print("  Tared  ");
  delay(300);
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
  rcaTime = totalTime;

  rcaState = digitalRead(rcaPin);

  attachInterrupt(digitalPinToInterrupt(rcaPin), changingRCA, CHANGE);
}

void calibrateTFT(void) {
  short increment = 1;
  int weight = 1;
  submenu = 0;
  bool exitFlag = false;
  String calibrateCommand = "calibrate:" + String(weight, DEC) + ";";
  //showCalibrateLoad(String(weight, DEC));
  //Delete description
  tft.setCursor(24, 100);
  tft.setTextColor(BLACK);
  tft.print(systemDescriptions[1]);

  //Explanation of the process
  tft.setTextColor(WHITE);
  tft.setCursor(50, 100);
  tft.print("Select the weight to use");

  //Blue button
  tft.setCursor(12, 218);
  tft.setTextColor(WHITE, BLUE);
  tft.print("+");
  tft.print(increment);

  //Red button
  tft.setCursor(248, 218);
  tft.setTextColor(WHITE, RED);
  tft.print("Accept");

  //Current weight
  tft.setCursor(120, 150);
  tft.setTextColor(WHITE, BLACK);
  tft.print("Current:");
  tft.setCursor(216, 150);
  tft.print(weight);
  redButton.update();
  blueButton.update();
  while (!exitFlag) {

    //Selecting the weight
    if (submenu == 0) {
      //TODO: Allow coninuous increasing by keeping pressed the button
      if (blueButton.fallingEdge()) {
        tft.setTextColor(BLACK);
        tft.setCursor(216, 150);
        tft.print(weight);
        weight += increment;
        if (weight == 101) {
          weight = 1;
        }
        tft.setTextColor(WHITE);
        tft.setCursor(216, 150);
        tft.print(weight);

        if (weight == 5) {
          increment = 5;
          tft.setCursor(24, 218);
          tft.setTextColor(WHITE, BLUE);
          tft.print(increment);

        } else if (weight == 100) {
          increment = 1;
          tft.setCursor(24, 218);
          tft.setTextColor(WHITE, BLUE);
          tft.print(increment);
        }
        calibrateCommand = "calibrate:" + String(weight, DEC) + ";";
      }

      //Change to Calibrate execution
      if (redButton.fallingEdge()) {

        //Deleting explanation
        tft.setTextColor(BLACK);
        tft.setCursor(50, 100);
        tft.print("Select the weight to use");
        tft.setCursor(120, 150);
        tft.print("Current:");
        tft.setCursor(216, 150);
        tft.print(weight);

        //Delete Blue button
        tft.fillRect(12, 218, 72, 16, BLACK);

        //        //Delete Red button
        tft.fillRect(248, 218, 72, 16, BLACK);

        tft.setTextColor(WHITE);
        tft.setCursor(100, 100);
        tft.print("Press Red to start Calibration");

        //Blue button
        tft.setCursor(12, 218);
        tft.setTextColor(WHITE, BLUE);
        tft.print("Cancel");

        //Red button
        tft.setCursor(248, 218);
        tft.setTextColor(WHITE, RED);
        tft.print("Start");

        submenu = 1;
      }
    }
    //Waiting the red button push to start calibration process
    if (submenu == 1) {
      if (redButton.fallingEdge()) {

        tft.setTextColor(BLACK);
        tft.setCursor(100, 100);
        tft.print("Press Red to start Calibration");
        tft.setCursor(150, 200);
        tft.print("Current:");
        tft.setCursor(246, 200);
        tft.print(weight);

        tft.setTextColor(WHITE);
        tft.setCursor(120, 150);
        tft.print("Calibrating...");


        calibrate(calibrateCommand);

        tft.setTextColor(BLACK);
        tft.setCursor(120, 150);
        tft.print("Calibrating...");

        tft.setTextColor(WHITE);
        tft.setCursor(120, 150);
        tft.print("Calibrated");

        exitFlag = true;
        delay(200);

        tft.setTextColor(BLACK);
        tft.setCursor(120, 150);
        tft.print("Calibrated");

        //Delete Blue button
        tft.fillRect(12, 218, 72, 16, BLACK);

        //Delete Red button
        tft.fillRect(248, 218, 60, 16, BLACK);

      }
      if (blueButton.fallingEdge()) {
        exitFlag = true;
      }
    }
    redButton.update();
    blueButton.update();
  }
  showMenu();
}

//function to read battery level. Not implemented in SportAnalyzer hardware version 1.0
void showBatteryLevel() {
  //float sensorValue = analogRead(A0);

}

void updateTime() {
  printTftFormat(totalTime / 1000000, 284, 215, 2, 0);
}
//TODO: Add more information or eliminate
void showSystemInfo() {

  //Erases the description of the upper menu entry
  tft.setTextSize(2);
  tft.setCursor(24, 100);
  tft.setTextColor(BLACK);
  tft.print(systemDescriptions[2]);
  redButton.update();
  while (!redButton.fallingEdge()) {
    redButton.update();
  }
  tft.setTextColor(BLACK);
  tft.setCursor(50, 60);
  tft.print(systemOptions[submenu]);
}

void showLoadCellResults() {
  int textSize = 2;
  tft.fillScreen(BLACK);
  tft.setTextSize(3);
  tft.setCursor(100, 0);
  tft.print("Results");

  tft.drawLine(0, 20, 320, 20, GREY);
  tft.drawLine(160, 240, 160, 20, GREY);
  tft.setTextSize(textSize);

  tft.setCursor(0, 40);
  tft.print("F");
  tft.setCursor(12, 48);
  tft.setTextSize(1);
  tft.print("max");
  printTftFormat(measuredMax, 100, 40, textSize, 1);

  tft.setTextSize(2);
  tft.setCursor(170, 40);
  tft.print("F");
  tft.setTextSize(1);
  tft.setCursor(182, 48);
  tft.print("max1s");
  printTftFormat(maxMeanForce1s, 280, 40, textSize, 1);

  tft.setTextSize(2);
  tft.setCursor(0, 80);
  tft.print("F");
  tft.setTextSize(1);
  tft.setCursor(12, 88);
  tft.print("trig");
  printTftFormat(forceTrigger, 100, 80, textSize, 1);


  tft.setTextSize(2);
  tft.setCursor(170, 80);
  tft.print("Imp");
  printTftFormat(impulse, 280, 80, textSize, 1);

  tft.setCursor(0, 120);
  tft.print("RFD");
  tft.setTextSize(1);
  tft.setCursor(36, 128);
  tft.print("100");
  printTftFormat(maxRFD100, 118, 120, textSize, 0);

  tft.setTextSize(2);
  tft.setCursor(170, 120);
  tft.print("RFD");
  tft.setTextSize(1);
  tft.setCursor(206, 128);
  tft.print("200");
  printTftFormat(maxRFD200, 298, 120, textSize, 0);

  if (RMSSD != 0)
  {
    tft.setCursor(0, 160);
    tft.print("RMSSD");
    printTftFormat(RMSSD, 100, 160, textSize, 1);


    tft.setTextSize(2);
    tft.setCursor(170, 160);
    tft.print("CV");
    tft.setTextSize(1);
    tft.setCursor(194, 168);
    tft.print("RMSSD");
    printTftFormat(cvRMSSD, 280, 160, textSize, 1);
  }

  //Red button exits results
  redButton.update();
  while (!redButton.fallingEdge()) {
    redButton.update();
  }
  tft.fillRect(0, 20, 320, 240, BLACK);
  drawMenuBackground();
}

void systemMenu()
{
  bool exitFlag = false;

  //Deletes the descripiton of system menu entry
  tft.setTextSize(2);
  tft.setCursor(12, 100);
  tft.setTextColor(BLACK);
  tft.print(menuDescription[5]);

  tft.setCursor(12, 100);
  tft.print(" Set the offset of the\nsensor.");

  showsystemMenu();


  redButton.update();
  blueButton.update();
  while (!exitFlag) {
    while (!blueButton.fallingEdge() && !redButton.fallingEdge())
    {
      redButton.update();
      blueButton.update();
    }

    //Blue button pressed. Change submenu option
    if (blueButton.fallingEdge()) {
      blueButton.update();
      submenu = (submenu + 1) % 3;
      showsystemMenu();

    }
    //Red button pressed. Execute the menu option
    if (redButton.fallingEdge()) {
      exitFlag = true;
      if (submenu == 0) {
        tft.setTextColor(BLACK);
        tft.setCursor(12, 100);
        tft.print(" Set the offset of the\nsensor.");
        tare();
      } else if (submenu == 1)
      {
        calibrateTFT();
      } else if (submenu == 2) {
        showSystemInfo();
      }
      tft.setTextColor(BLACK);
      tft.setCursor(50, 60);
      tft.print(systemOptions[submenu]);
      menu = 0;
      showMenu();
    }
  }
}

void showsystemMenu() {

  tft.setTextColor(BLACK);
  tft.setCursor(50, 60);
  tft.print(systemOptions[(submenu + 2) % 3]);
  tft.setTextColor(WHITE);
  tft.setCursor(50, 60);
  tft.print(systemOptions[submenu]);

  tft.setCursor(24, 100);
  tft.setTextColor(BLACK);
  tft.print(systemDescriptions[(submenu + 2) % 3]);
  tft.setCursor(24, 100);
  tft.setTextColor(WHITE);
  tft.print(systemDescriptions[submenu]);
}

void start_steadiness()
{
  totalTime = 0;

  capturing = true;
  capturingPreSteadiness = true;
  capturingSteadiness = false;
}

void end_steadiness()
{
  capturing = false;
  capturingSteadiness = false;
}

/*
  function to draw a cartesian coordinate system and plot whatever data you want
  just pass x and y and the graph will be drawn
  huge arguement list
  &d name of your display object
  x = x data point
  y = y datapont
  gx = x graph location (lower left)
  gy = y graph location (lower left)
  w = width of graph
  h = height of graph
  xlo = lower bound of x axis
  xhi = upper bound of x asis
  xinc = division of x axis (distance not count)
  ylo = lower bound of y axis
  yhi = upper bound of y asis
  yinc = division of y axis (distance not count)
  title = title of graph
  xlabel = x axis label
  ylabel = y axis label
  gcolor = graph line colors
  acolor = axis line colors
  pcolor = color of your plotted data
  tcolor = text color
  bcolor = background color
  &redraw = flag to redraw graph on fist call only
*/

void Graph(ILI9341_t3 & d, double x, double y, double gx, double gy, double w, double h, double xlo, double xhi, double xinc, double ylo, double yhi, double yinc, String title, String xlabel, String ylabel, unsigned int gcolor, unsigned int acolor, unsigned int pcolor, unsigned int tcolor, unsigned int bcolor, boolean & startOver)
{
  //double ydiv, xdiv;
  // initialize old x and old y in order to draw the first point of the graph
  // but save the transformed value
  // note my transform funcition is the same as the map function, except the map uses long and we need doubles
  //static double ox = (x - xlo) * ( w) / (xhi - xlo) + gx;
  //static double oy = (y - ylo) * (gy - h - gy) / (yhi - ylo) + gy;
  //double temp;
  //int rot, newrot;

  //Mapping values to coordinates
  x =  (x - xlo) * ( w) / (xhi - xlo) + gx;
  y =  (y - ylo) * (gy - h - gy) / (yhi - ylo) + gy;

  if (startOver == true)
  {

    startOver = false;
    //In startOver, a point is plotted at the most left point
    ox = x;
    oy = y;
  }

  //graph drawn now plot the data
  // the entire plotting code are these few lines...
  // recall that ox and oy are initialized as static above
  //Drawing 3 lines slows the drawing and erasing
  d.drawLine(ox, oy, x, y, pcolor);
  //  d.drawLine(ox, oy + 1, x, y + 1, pcolor);
  //  d.drawLine(ox, oy - 1, x, y - 1, pcolor);
  ox = x;
  oy = y;

}

void redrawAxes(ILI9341_t3 & d, double gx, double gy, double w, double h, double xlo, double xhi, double ylo, double yhi, double yinc, String title, String xlabel, String ylabel, unsigned int gcolor, unsigned int acolor, unsigned int pcolor, unsigned int tcolor, unsigned int bcolor, boolean resize)
{
  //double ydiv, xdiv;
  // initialize old x and old y in order to draw the first point of the graph
  // but save the transformed value
  // note my transform funcition is the same as the map function, except the map uses long and we need doubles
  //static double ox = (x - xlo) * ( w) / (xhi - xlo) + gx;
  //static double oy = (y - ylo) * (- h) / (yhi - ylo) + gy;
  double yAxis;
  //double xAxis;

  d.setTextSize(1);
  d.setTextColor(tcolor, bcolor);

  //Vertical line
  d.drawLine(gx, gy, gx, gy - h, acolor);

  // draw y scale
  for (double i = ylo; i <= yhi; i += yinc)
  {
    // compute the transform
    yAxis =  (i - ylo) * (-h) / (yhi - ylo) + gy;

    d.drawLine(gx, yAxis, gx + w, yAxis, acolor);
    //If the scale has changed the numbers must be redrawn
    if (resize)
    {
      printTftFormat(i, gx - 6, yAxis - 3, 1, 0);
    }
  }

  //  xAxis =  (-xlo) * ( w) / (xhi - xlo) + gx;
  //  d.drawLine(gx, gy, gx, gy - h, acolor);

  //now draw the labels

  d.setTextSize(1);
  d.setTextColor(acolor, bcolor);
  d.setCursor(gx , gy + 20);
  d.println(xlabel);

  d.setTextSize(1);
  d.setTextColor(acolor, bcolor);
  d.setCursor(gx - 30, gy - h - 10);
  d.println(ylabel);

  d.setTextSize(2);
  d.setTextColor(tcolor, bcolor);
  d.setCursor(gx , gy - h - 30);
  d.println(title);
}

void drawMenuBackground() {
  tft.fillRect(0, 0, 320, 50, BLACK);
  tft.fillRoundRect(0, 0, 30, 50, 10, WHITE);
  tft.fillRoundRect(290, 0, 30, 50, 10, WHITE);
  tft.setTextSize(3);
  tft.setCursor(30, 20);
}

void capture()
{
  //Position graph's lower left corner.
  double graphX = 30;
  double graphY = 200;

  //Size of the graph
  double graphW = 290;
  double graphH = 200;

  //Minimum and maximum values to show
  double xMin = 0;
  double xMax = 290;

  //Size an num of divisions
  double yDivSize = 100;
  double yDivN = 10;
  double xDivSize = 100;
  double yBuffer[320];

  double plotBuffer[plotPeriod];

  bool resized = true;

  long lastUpdateTime = 0;

  tft.fillScreen(BLACK);

  double xGraph = 1;

  //Print summary results
  tft.setTextSize(2);
  tft.setCursor(10, 215);
  tft.print(maxString);
  printTftFormat(measuredMax, 100, 215, 2, 2);
  tft.setCursor(308, 215);
  tft.print("s");

  Serial.println(graphMin);
  Serial.println(graphMax);

  while (capturing)
  {
    //Deleting the previous plotted points
    for (int i = xMin; i < xGraph; i++)
    {
      Graph(tft, i, yBuffer[i], graphX, graphY, graphW, graphH, xMin, xMax, xDivSize, graphMin, graphMax, yDivSize, "", "", "", WHITE, WHITE, BLACK, WHITE, BLACK, startOver);
    }
    startOver = true;
    redrawAxes(tft, graphX, graphY, graphW, graphH, xMin, xMax, graphMin, graphMax, yDivSize, "", "", "", WHITE, BLACK, BLACK, BLACK, BLACK, resized);
    graphMax = newGraphMax;
    graphMin = newGraphMin;
    yDivSize = (graphMax - graphMin) / yDivN;
    if (resized) {
      Graph(tft, xMin, yBuffer[(int)xMin], graphX, graphY, graphW, graphH, xMin, xMax, xDivSize, graphMin, graphMax, yDivSize, "", "", "", WHITE, WHITE, BLACK, WHITE, BLACK, startOver);
      for (int i = xMin; i < xGraph; i++)
      {
        Graph(tft, i, yBuffer[i], graphX, graphY, graphW, graphH, xMin, xMax, xDivSize, graphMin, graphMax, yDivSize, "", "", "", WHITE, WHITE, BLUE, WHITE, BLACK, startOver);
      }
    }
    redrawAxes(tft, graphX, graphY, graphW, graphH, xMin, xMax, graphMin, graphMax, yDivSize, "", "", "", WHITE, GREY, WHITE, WHITE, BLACK, resized);
    resized = false;

    if (xGraph >= xMax) xGraph = 0;
    while ((xGraph < xMax && !resized) && capturing) {
      for (int n = 0; n < plotPeriod; n++)
      {
        //Checking the RCA state
        if (rcaState != lastRcaState) {       //Event generated by the RCA
          Serial.print(rcaTime);
          Serial.print(";");

          if (rcaState) {
            Serial.println("R");
            forceTrigger = measured;
          } else {
            Serial.println("r");
          }
          lastRcaState = rcaState;

          //If no RCA event, read the sensor as usual
        } else {
          //Calculation of the variables shown in the results
          if (sensor == incEncoder) geEncoderDynamics();
          else if (sensor == loadCell) getLoadCellDynamics();
          else if (sensor == loadCellIncEncoder) getPowerDynamics();

          //Value exceeds the plotting area
          if (measured > newGraphMax) {
            newGraphMax = measured + (graphMax - graphMin) * 0.5;
            resized = true;
          }
          if (measured < newGraphMin) {
            newGraphMin = measured - (graphMax - graphMin) * 0.5;
            resized = true;
          }
        }
        //        Serial.print(totalTime); Serial.print(";");
        //        Serial.println(measured, 2); //scale.get_units() returns a float
        plotBuffer[n] = measured;

        //Pressing blue or red button ends the capture
        //Check the buttons state
        redButton.update();
        blueButton.update();
        if (redButton.fallingEdge() || blueButton.fallingEdge()) {
          Serial.println("Button pressed");
          if (sensor == incEncoder) endEncoderCapture();
          else if (sensor == loadCell) endLoadCellCapture();
          else if (sensor == loadCellIncEncoder) endPowerCapture();
          xGraph = xMax;
        }
      }
      //      Serial.println("Ended plotPeriod");

      if (capturing)
      {
        yBuffer[(int)xGraph] = 0;

        for (int i = 0; i < plotPeriod; i++)
        {
          yBuffer[(int)xGraph] = yBuffer[(int)xGraph] + plotBuffer[i];
        }

        yBuffer[(int)xGraph] = yBuffer[(int)xGraph] / plotPeriod;
        Graph(tft, xGraph, yBuffer[(int)xGraph], graphX, graphY, graphW, graphH, xMin, xMax, xDivSize, graphMin, graphMax, yDivSize, "", "", "", WHITE, WHITE, BLUE, WHITE, BLACK, startOver);
        xGraph++;
        if (measured > measuredMax)
        {
          measuredMax = measured;
          printTftFormat(measuredMax, 100, 215, 2, 2);
        }

        if ((lastUpdateTime - totalTime) > 1000000) {
          lastUpdateTime = totalTime;
          updateTime();
        }
      }
    }
  }
}

void geEncoderDynamics()
{
  int sampleDuration = totalTime - lastSampleTime;
  if (sampleDuration >= 10000)
  {
    lastSampleTime = totalTime;
    long position = encoder.read();
    measured = (float)(position - lastPosition) * 1000 / (sampleDuration);
    if (encoderPhase * (position - lastPosition) < 0)
    {
      encoderPhase *= -1;
      numRepetitions++;
      avgVelocity = (float)(position - startPhasePosition) * 1000 / (lastSampleTime - startPhaseTime);
      Serial.println(avgVelocity);
      if (avgVelocity > maxAvgVelocity) maxAvgVelocity = avgVelocity;
      startPhasePosition = position;
      startPhaseTime = lastSampleTime;
    }
    lastPosition = position;

  }
}

void startEncoderCapture()
{
  capturing = true;
  sensor = incEncoder;
  Serial.println(sensor);
  maxString = "Vmax:        m/s";
  plotPeriod = 500;
  newGraphMin = -10;
  newGraphMax = 10;
  measuredMax = 0;
  totalTime = 0;
  capture();
}

void endEncoderCapture()
{
  capturing = false;
  sensor = none;
  //If the device is controlled by the PC the results menu is not showed
  //because during the menu navigation the Serial is not listened.
  if (!PCControlled) {
    showEncoderResults();
  }
  showMenu();
}

void showEncoderResults()
{
  int textSize = 2;
  tft.fillScreen(BLACK);
  tft.setTextSize(3);
  tft.setCursor(100, 0);
  tft.print("Results");

  tft.drawLine(0, 20, 320, 20, GREY);
  tft.drawLine(160, 240, 160, 20, GREY);
  tft.setTextSize(textSize);

  tft.setCursor(0, 40);
  tft.print("v");
  tft.setCursor(12, 48);
  tft.setTextSize(1);
  tft.print("peak");
  printTftFormat(measuredMax, 100, 40, textSize, 1);

  tft.setTextSize(2);
  tft.setCursor(170, 40);
  tft.print("Vrep");
  tft.setTextSize(1);
  tft.setCursor(218, 48);
  tft.print("max");
  printTftFormat(maxAvgVelocity, 268, 40, textSize, 2);

  tft.setTextSize(2);
  tft.setCursor(0, 80);
  tft.print("nRep");
  printTftFormat(numRepetitions, 100, 80, textSize, 0);

  redButton.update();
  while (!redButton.fallingEdge()) {
    redButton.update();
  }
  tft.fillRect(0, 20, 320, 240, BLACK);
  drawMenuBackground();
}

void getPowerDynamics()
{
  float force = scale.get_units();
  long position = encoder.read();
  float velocity = (float)(position - lastPosition) * 1000 / (totalTime - lastSampleTime);
  lastSampleTime = totalTime;
  lastPosition = position;
  measured = force * velocity;
  Serial.println(measured);
  if (measured > maxPower) maxPower = measured;
}

void startPowerCapture()
{
  capturing = true;
  sensor = loadCellIncEncoder;
  maxString = "Pmax:        W";
  plotPeriod = 5;
  newGraphMin = -200;
  newGraphMax = 500;
  measuredMax = 0;
  totalTime = 0;
  capture();
}

void endPowerCapture()
{
  capturing = false;
  sensor = none;
  //If the device is controlled by the PC the results menu is not showed
  //because during the menu navigation the Serial is not listened.
  if (!PCControlled) {
    showPowerResults();
  }
  showMenu();
}

void showPowerResults()
{
  int textSize = 2;
  tft.fillScreen(BLACK);
  tft.setTextSize(3);
  tft.setCursor(100, 0);
  tft.print("Results");

  tft.drawLine(0, 20, 320, 20, GREY);
  tft.drawLine(160, 240, 160, 20, GREY);
  tft.setTextSize(textSize);
  
  tft.setCursor(0, 40);
  tft.print("P");
  tft.setCursor(12, 48);
  tft.setTextSize(1);
  tft.print("peak");
  printTftFormat(measuredMax, 100, 40, textSize, 1);
  
  redButton.update();
  while (!redButton.fallingEdge()) {
    redButton.update();
  }
  tft.fillRect(0, 20, 320, 240, BLACK);
  drawMenuBackground();
}
