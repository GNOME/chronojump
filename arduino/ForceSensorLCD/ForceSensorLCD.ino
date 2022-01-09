

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
//Device commented for memory optimization
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
float RFD200 = 0.0;   //average RFD during 200 ms
float RFD100 = 0.0;  //average RFD during 100 ms
float maxRFD200 = 0.0;   //Maximim average RFD during 200 ms
float maxRFD100 = 0.0;  //Maximim average RFD during 100 ms
bool elapsed200 = false;  //Wether it has passed 200 ms since the start
bool elapsed100 = false;  //Wether it has passed 100 ms since the start


unsigned long currentTime = 0;  //Direct value from micros()
unsigned long elapsedTime = 0;  //Elapsed time between 2 consecutives measures. No overflow manage
unsigned long totalTime = 0;    //Elapsed time since start of capture. Overflow managed

/* Not used in order to optimize memory
//Used to sync 2 evices
unsigned long syncTime = 0;
unsigned int samples = 0;
*/

const unsigned short redButtonPin = 13;
bool redButtonState;

const unsigned short blueButtonPin = 6;
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
//-> Start of non starndard charachters
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
//End of non standard characters <---

//Physical configuration of the LCD
LiquidCrystal lcd(12, 11, 10, 9, 8, 7);

const unsigned short rcaPin = 3;

unsigned long triggerTime = 0;        //The instant in which the trigger signal is activated
bool rcaState = digitalRead(rcaPin);  //Wether the RCA is shorted or not
bool lastRcaState = rcaState;         //The previous state of the RCA

unsigned short menu = 0;              //Main menu state
unsigned short submenu = 0;           //Submenus state

const String menuList [] = {
  "1-Measure",
  "2-TareMeasure",
  "3-Tare",
  "4-Calibrate",
  "5-System"
};

//Mean force in 1s
//Circular buffer where all measures in 1s are stored
//ADC has 84.75 ~ 85 samples/second. If we need the diference in 1 second we need 1 more sample
unsigned short freq = 86;
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

//Impulse. Impulse = Sumatory of the force*time
float impulse = 0;
bool elapsed1Sample = false;    //Wether there's a previous sample. Needed to calculate impulse

//Force in trigger
float forceTrigger = 0.0;       //Measured force at the moment the RCA is triggered

//If device is controled by computer don't show results on LCD
bool PCControlled = false;

void setup() {
  pinMode(redButtonPin, INPUT);
  pinMode(blueButtonPin, INPUT);
  lcd.begin(16, 2);

  Serial.begin(115200);

  //Activate interruptions activated by any RCA state change
  attachInterrupt(digitalPinToInterrupt(rcaPin), changingRCA, CHANGE);

  lcd.setCursor(2, 0);
  lcd.print("CHRONOJUMP");
  lcd.setCursor(2, 1);
  lcd.print("Boscosystem");
  delay(1000);

  long tare = 0;
  EEPROM.get(tareAddress, tare);
  //If the arduino has not been tared the default value in the EEPROM is -151.
  //TODO: Check that it is stil true in the current models
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

  //Every second the battery level is updated via interrupts
  MsTimer2::set(1000, showBatteryLevel);
  MsTimer2::start();
 
  showMenu();
}

void loop()
{
  if (!capturing)
  {
    //The blue button navigates through the Menu options
    blueButtonState = digitalRead(blueButtonPin);
    if (blueButtonState) {
      blueButtonState = false;
      menu++;
      menu = menu % 5;
      showMenu();
    }
    delay(100);

    //The red button activates the menu option
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
  //Showing the menu option
  lcd.setCursor(0, 0);
  lcd.print(menuList[menu]);
  //Showing the next menu number in the upper right corner
  lcd.setCursor(14,0);
  lcd.print(menu + 1);
  //The up arrow is associated to the blue button
  lcd.createChar(6, upArrow);
  lcd.setCursor(15, 0);
  lcd.write(byte (6));
  //the down arrow is associated to the red button
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
      checkTimeOverflow();                //If micros had an overflow it is detected and corrected
      Serial.print(totalTime);
      Serial.print(";");

      if (rcaState) {
        Serial.println("R");
        forceTrigger = measured;
      } else {
        Serial.println("r");
      }
      lastRcaState = rcaState;
      
    //If no RCA event, read the force as usual
    } else {
      currentTime = micros();
      checkTimeOverflow();
      measured = scale.get_units();

      //When current Force Slot is equal to size of the buffer it starts over to 0
      currentFSlot = (currentFSlot + 1) % freq;
      //wHEN current Time Slot is equal to the size of the buffer it starts over to 0
      currentTSlot = (currentTSlot + 1) % samples200ms;

      if(currentTSlot > 0) elapsed1Sample = true;     //There's a previous sample
      if(currentTSlot >= (samples200ms -1)) elapsed200 = true;
      if(currentTSlot >= (samples100ms -1)) elapsed100 = true;
      
      forces1s[currentFSlot] = measured;
      totalTimes1s[currentTSlot] = totalTime;

      //Calculating the average during 1s
      float sumForces = 0;
      for (int i = 0; i < freq; i++) {
        sumForces += forces1s[i];
      }

      //Mean forces = sum of the forces divided by the number of samples in 1 second
      meanForce1s = sumForces / freq;

      if (abs(meanForce1s) > abs(maxMeanForce1s)) maxMeanForce1s = meanForce1s;

        sumSSD += (sq(measured - lastMeasure));
        sumMeasures += measured;
        samplesSSD++;
        lastMeasure = measured;
        RMSSD = sqrt(sumSSD / (samplesSSD - 1));
        cvRMSSD = 100 * RMSSD / ( sumMeasures / samplesSSD);

      //RFD stuff start ------>
      
      //To go backwards N slots use [currentSlot + TotalPositions - N]
      if(elapsed1Sample){
        impulse += (((measured + forces1s[(currentFSlot + freq - 1) % freq])  / 2) *      //Mean force between 2 samples
          (totalTime - totalTimes1s[(currentTSlot + samples200ms - 1) % samples200ms]) / 1e6);  //Elapsed time between 2 samples
      }
      
      if(elapsed200){
        RFD200 = (measured - forces1s[(currentFSlot + freq - samples200ms) % freq]) /     //Increment of the force in 200ms
          ((totalTime - totalTimes1s[(currentTSlot + 1) % samples200ms]) / 1e6);          //Increment of time
        if(abs(maxRFD200) < abs(RFD200)) maxRFD200 = RFD200;
      }

      if(elapsed100){
        RFD100 = (measured - forces1s[(currentFSlot + freq - samples100ms) % freq]) /     //Increment of the force in 200ms
          ((totalTime - totalTimes1s[(currentTSlot + samples200ms - samples100ms) % samples200ms]) / 1e6); //Increment of time
        if(abs(maxRFD100) < abs(RFD100)) maxRFD100 = RFD100;
      }
      //<------- RFD stuff end

      //Negative numbers treated as positives to calculate the max
      if (abs(measured) > abs(measuredLcdDelayMax)) {
        measuredLcdDelayMax = measured;
      }
      if (abs(measured) > abs(measuredMax)) {
        measuredMax = measured;
      }

      Serial.print(totalTime); Serial.print(";");
      Serial.println(measured, 2); //scale.get_units() returns a float

      printOnLcd();
    }

    //Pressing blue or red button ends the capture
    redButtonState = digitalRead(redButtonPin);
    blueButtonState = digitalRead(blueButtonPin);
    if (redButtonState || blueButtonState) {
      redButtonState = false;
      blueButtonState = false;
      end_capture();
    }
  }
  MsTimer2::start();
}

//TODO: manage the delay in LCD write with a timer
void printOnLcd() {
  lcdCount = lcdCount + 1;
  if (lcdCount >= lcdDelay)
  {
    lcd.clear();

    //Upper left
    printLcdFormat (measuredLcdDelayMax, 4, 0, 1);
    //Lower left
    printLcdFormat (maxMeanForce1s, 4, 1, 1);
    //Upper right
    printLcdFormat (totalTime / 1e6, 15, 0, 0); //Showing total capture time in seconds
    //Lower right
    printLcdFormat (impulse, 13, 1, 1);

    measuredLcdDelayMax = 0;
    lcdCount = 0;
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

void start_capture()
{
  //Disabling the battery level indicator
  MsTimer2::stop();
  Serial.println("Starting capture...");
  totalTime = 0;
  lastTime = micros();
  measuredMax = 0;
  impulse = 0;

  //filling the array of forces ant times with initial force
  lastMeasure = scale.get_units();
  for (short i; i < freq; i++) {
    forces1s[i] = lastMeasure;
  }

  for (short i; i < samples200ms; i++) {
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
  delay(500);

  //If the device is controlled by the PC the results menu is not showed
  //because during the menu navigation the Serial is not listened.
  if(!PCControlled){   
    lcd.clear();
    lcd.setCursor(4, 0);
    lcd.print("Results:");
    showResults();
  }
  //Activating the Battery level indicator
  MsTimer2::start();
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

/* Disabled due to memory optimization
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
*/

void calibrateLCD(void) {
  int weight = 5;
  submenu = 0;
  bool exitFlag = false;
  String calibrateCommand = calibrateCommand + String(weight, DEC) + ";";
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
  delay(1000);
  showMenu();
}

//During load selection each time the load is changed it show the new load
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

//TODO: Add more information or eliminate
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

  //Characters in the right side indicating the function of buttons
  lcd.clear();
  lcd.setCursor(15, 0);
  lcd.print(">");
  lcd.createChar(7, exitChar);
  lcd.setCursor(15, 1);
  lcd.write(byte (7));
  
  //Showing menu 0
  lcd.setCursor(0,0);
  lcd.print("Fmax ");
  printLcdFormat(measuredMax, 11, 0, 1);
  lcd.setCursor(0,1);
  lcd.print("Fmax1s ");
  printLcdFormat(maxMeanForce1s, 11, 1, 1);
  //Red button exits results
  while(!redButtonState){
    blueButtonState = digitalRead(blueButtonPin);
    redButtonState = digitalRead(redButtonPin);
    //Blue button changes menu option
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
