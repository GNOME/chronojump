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
  #   Copyright (C) 2022     Xavier Cano <xaviercanoferrer@gmail.com>


*/

#include "EEPROM.h"
#include "SPI.h"
// ¡¡¡Atention, if you are using Teensy, the ILI9341_t3 library must be the teensy specific located at:
// arduino/hardware/teensy/avr/libraries/ILI9341_t3
// If you have this library in your Arduino/libraries delete it
// To check the library used go to preferences -> Detailed output for compilation
#include "ILI9341_t3.h"

// ¡¡¡Atention, HX711 library of the arduino library manager has some issues with teensy.
// Use the code from https://github.com/bogde/HX711
#include "HX711.h"
#include "Bounce2.h"
#include "Encoder.h"
#include "SD.h"
#include "elapsedMillis.h"
#include "TimeLib.h"
#include "michrolab.h"

//Version number //it always need to start with: "MiChroLab-"
//Device commented for memory optimization
String version = "0.1";

//#define teensy_3_2
#define teensy_4_0

//Real time clock stuff
//time_t RTCTime; //TODO: Check if it is necessary

//Encoder variables
const unsigned int encoderAPin = 8;
const unsigned int encoderBPin = 9;
Encoder encoder(encoderAPin, encoderBPin);
IntervalTimer encoderTimer;
volatile int position = 0;
float load = 0.0;
bool inertialMode = false;
long lastPosition = 0;
long lastSamplePosition;
int encoderBuffer[20];
byte encoderBufferIndex = 0;
String encoderString = "";
int encoderPhase = 0;    // 1 means concentric, -1 means eccentric, 0 unknown (prior detecting first repetition)
long startPhasePosition = 0;
long startPhaseTime = 0;
int minRom = 100;      //Minimum range of movement to consider the start of a new repetition
long localMax = 0;    //Local maximum in the position. It must be checked if it is a point of phase change
float lastVelocity = 0;
float avgVelocity = 0;
float maxAvgVelocity = 0;
bool propulsive = true;
bool calibratedInertial = false;
bool encoderFlag = false;
int pps = 40;

int numRepetitions = 0;
bool redrawBars = false;

int tareAddress = 0;
int calibrationAddress = 4;
int forceGoalAddress = 8;
int groupAddress = 12;
int ppsAddress = 16;

#define DOUT  2
#define CLK  3

//HX711 scale(DOUT, CLK);
HX711 scale;

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
  none = 0,
  rca = 1,
  loadCell = 2,
  incLinEncoder = 3,
  incRotEncoder = 4,
  raceAnalyzer = 5,
  loadCellincEncoder = 6
};

enum exerciseType {
  jumps, //"Jumps" on main menu
  inertial, //"Iner. Velocity" on main menu
  gravitatory, //"Lin. Velocity" on main menu
  force, //"Raw Force" on main menu
  encoderRace, //"RaceAnalyzer" on main menu
  photocelRace,
  
  rawPower, 
  steadiness,
  credits //name variable could be replaced
};

// Sets the menu to show before capturing
enum configSetMenu {
  personSelect = 0,
  exerciseSelect = 1,
  valueSelect = 2,
  capture = 3,
  quit = 4
};

//Align of the text
enum alignType {
  alignLeft = 0,
  alignRight = 1,
  alignCenter = 2
};

configSetMenu currentConfigSetMenu = personSelect;
bool nextConfigSetMenu = false;
bool prevConfigSetMenu = false;

sensorType sensor = none;
String maxString = "";

struct frame {
  int displacement;
  unsigned long totalTime;
  sensorType sensor;
};

frame raceAnalyzerSample = {.displacement = 0, .totalTime = 0, sensor = incRotEncoder};

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


volatile unsigned long rcaTime = 0;  //Time at which RCA changed
unsigned long lastRcaTime = 0;
volatile bool rcaFlag = false;
elapsedMicros totalTime = 0;
elapsedMicros totalTestTime;
unsigned long lastSampleTime;
unsigned long currentSampleTime;
unsigned long lastMeasuredTime;
//By default the debounce time for the RCA is 10000.
//With the foot pedal 2000 is too short for jumps and some values are repeated
unsigned int rcaDebounceTime = 10000;

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

float bars[10] = {0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0};

const unsigned int rcaPin = 7;
volatile bool rcaState = LOW;
bool lastRcaState = LOW;         //The previous state of the RCA

unsigned int currentMenuIndex = 0;              //Main menu state
unsigned int submenu = 0;           //submenus state

//!!Atention I don't understant why but eliminating this line returns error in menuEntry initialitzation
functionPointer FArray[3] = {&fakeFunction, &fakeFunction, &fakeFunction};

menuEntry mainMenu[10] = {
  { "Jumps", "Shows bars with the jumps height", "Ju", &jumpCapture},
  //  { "Drop Jumps", "Jumps with a previous\nfalling height (previous\njump or fixed height)\nShows bars with the heightof jumps", &dropJumpsCapture},
  { "Raw Force", "Shows standard graph of\nthe force and the summary of the set.\n(Maximum Force, RFD and\nImpulse)", "RF", &startLoadCellCapture},
  { "Lin. Velocity", "Show bars of linear velocity", "LV", &startGravitEncoderCapture },
  { "Iner. Velocity", "Show bars of the velocity of the person in inertial machines", "IV", &startInertEncoderCapture },
  { "RaceAnalyzer", "Measure speed with a raceAnalyzer", "RA", &startRaceAnalyzerCapture},
  { "RawPower", "Measure Force and Speed\nat the same time.\nOnly power is shown in thegraph", "RP", &startPowerCapture},
  //{ "Tared Force", "Offset the force before\nmeasuring it.\nUseful to substract body\nweight.", &startTareCapture},
  { "F. Steadiness", "RMSSD and cvRMSSD.\nSteadynessof the force.\nWhen ready, press the Red Button to get the\nsteadiness of the next 5s.", "FS", &startSteadiness},
  { "System", "Performs calibration or\ntare and shows some system information.", "Sy", &showSystemMenu}
};

int mainMenuItems = 8;

menuEntry systemMenu[10] {
  { "Group", "Select the group you are going to use.\nUp to 9 groups can be\nselected", "Gr", &selectGroup},
  { "Tare", "Set the offset of the\nsensor.", "Ta", &tare },
  { "Calibrate", "Set the equivalence\nbetween the sensor values\nand the force measured.", "Ca", &calibrateTFT },
  { "Sel. load cell", "Select from a list of \nload cells. It allows to \nsave the calibrations", "SL", &selectLoadCellDialog},
  { "Force Goal", "Set the goal force for\nsteadiness measurements.", "FG", &setForceGoal },
  { "Inert. Calib.", "Set the Exact point in which the concentric phase ends", "IC", &calibrateInertial},
  { "Info", "Hardware, firmware and config information.", "In", &showSystemInfo, },
  { "Exit", "Goes back to main menu", "Ex", &backMenu},
  /*
  { "", "", &backMenu},
  { "", "", &backMenu}
  */
};

int systemMenuItems = 8;

menuEntry currentMenu[10];


int menuItemsNum = mainMenuItems;
//Mean force in 1s
//Circular buffer where all measures in 1s are stored
//ADC has 84.75 ~ 85 samples/second. If we need the diference in 1 second we need 1 more sample
//TODO: Adjust values to 160Hz
unsigned int freq = 86;
//Cirtular buffer of force measures
float forces1s[86];
//200 ms -> 16.95 ~ 17 samples. To know the elapsed time in 200 ms we need 1 more sample
unsigned int samples200ms = 18;
unsigned int samples100ms = 9;
//Circular buffer of times
unsigned long totalTimes1s[18];
//The current slot in the array
unsigned int currentFSlot = 0;
unsigned int currentTSlot = 0;
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
float forceGoal = 300;

//Impulse. Impulse = Sumatory of the force*time
float impulse = 0;
bool elapsed1Sample = false;    //Wether there's a previous sample. Needed to calculate impulse

//Force in trigger
float forceTrigger = 0.0;       //Measured force at the moment the RCA is triggered

//Power variables
float power = 0;
float maxPower = 0;

//If device is controled by computer don't show results on TFT
bool PcControlled = false;

long tareValue = 0;

//Images of the multidirectional switch
#include "IMGS/left.c"
#include "IMGS/right.c"
#include "IMGS/leftRight.c"
#include "IMGS/up.c"
#include "IMGS/down.c"
#include "IMGS/upDown.c"
#include "IMGS/all.c"
#include "IMGS/center.c"

// Pin config of the SD reader
const int SD_CS = 6;

//Pin config of the TFT
#ifdef teensy_4_0
#define TFT_DC      20  // TFT display/command pin
#define TFT_CS      10  //TFT select pin
#define TFT_RST    255  // 255 = unused, connect to 3.3V
#define TFT_MOSI    11
#define TFT_MISO    12
#define TFT_SCLK    13


//Older buttons where red and blue. Their pins are 4 and 5 respectively
//const unsigned int cenButtonPin = 4;
//const unsigned int rightButtonPin = 5;

/*
//(older prototype)
const unsigned int cenButtonPin = 15; //Accept
const unsigned int downButtonPin = 14; //Cancel or go to previous menu
const unsigned int rightButtonPin = 16; //-->
const unsigned int leftButtonPin = 17; //<--
*/

//NEW PROTOTYPE
const unsigned int cenButtonPin = 0; //Accept 
const unsigned int downButtonPin = 5; // ↓ Cancel or go to previous menu
const unsigned int rightButtonPin = 1; //--> //down when vertical menu //++
const unsigned int leftButtonPin = 4; //<-- //--
const unsigned int upButtonPin = 21; // ↑ Used when items are placed vertically

#endif

#ifdef teensy_3_2
#define TFT_DC      20
#define TFT_CS      21
#define TFT_RST    255  // 255 = unused, connect to 3.3V
#define TFT_MISO    12
#define TFT_MOSI     7
#define TFT_SCLK    14

const unsigned int cenButtonPin = 4;
const unsigned int blueButtonPin = 5;
const unsigned int leftButtonPin = 17; 
const unsigned int rightButtonPin = 16; 
#endif

Bounce cenButton = Bounce(cenButtonPin, 50);
Bounce rightButton = Bounce(rightButtonPin, 50);
Bounce leftButton = Bounce(leftButtonPin, 50);
Bounce downButton = Bounce(downButtonPin, 50);
Bounce upButton = Bounce(upButtonPin, 50);


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

#define BUFFPIXEL 125  //Used for bmp

//Don't plot all the samples. values plotted every plotPeriod samples
int plotPeriod = 10;
boolean startOver = true;
double ox , oy ;
double x, y;

//SD stuff
unsigned int currentPerson = 0;
unsigned int setNumber = 0;
unsigned int dirNumber;
String dirName = "";
String fileName = "";
int group = 0;

struct personType {
  unsigned int index;
  String name;
  String surname;
  float weight;
  float heigh;
};
unsigned int totalPersons = 0;
personType persons[100];

struct jumpType {
  unsigned int id;
  String name;
  int jumpLimit;
  float timeLimit;
  bool hardTimeLimit;
  float percentBodyWeight;
  float fall;
  bool startIn;   //If the time of contact is required, start outside or start inside but make a previous jump
  String description;
};

jumpType jumpTypes[100];
unsigned int totalJumpTypes = 0;
unsigned int currentExerciseType = 0;
int totalJumps = 0;
//In simple jumps the firstPhase is the contact. In DropJumps the first phase is flight

struct gravitType {
  unsigned int id;
  String name;
  String description;
  float percentBodyWeight;
  float speed1Rm;
};

gravitType gravTypes[100];
unsigned int totalGravTypes = 0;

struct inertType {
  unsigned int id;
  String name;
  String description;
  float percentBodyWeight;
};

inertType inertTypes[100];
unsigned int totalInertTypes = 0;

struct inertMachineType {
  unsigned int id;
  String name;
  String description;
  String diameters;
  float gearedDown;
};

inertMachineType inertMachines[10];
unsigned int totalInertMachines = 0;
unsigned int currentInertMachine = 0;

struct forceType {
  unsigned int id;
  String name;
  String description;
  float percentBodyWeight;
  float angle;
  bool tare;
};

forceType forceTypes[100];
unsigned int totalForceTypes = 0;

struct raceAnalyzerType {
  unsigned int id;
  String name;
  String description;
};

raceAnalyzerType raceAnalyzerTypes[100];
unsigned int totalRaceAnalyzerTypes = 0;

// Calibration stuff
struct calibrationType {
  unsigned int id;
  long tare;
  float calibration;
  String description;
};

calibrationType calibrations[100];
unsigned int totalCalibrations = 0;
unsigned int currentCalibration = 0;

IntervalTimer rcaTimer;
String fullFileName;
File dataFile;
int sampleNum = 0;
String fileBuffer;        //Text mode
char binFileBuffer[100];       //binary mode. Using char type cannot write speeds greater than +-127 pulses/ms

String textList[7] = {"First", "Second", "Thirth", "Fourth", "Fifth", "Sixtth", "Seventh"} ;

void setup() {

  SPI.beginTransaction(SPISettings(100000000, MSBFIRST, SPI_MODE0));
  //Real time clock sync with the temp clock
  setSyncProvider(getTeensy3Time);
  //Attention: some SD cards fails to initalize after uploading the firmware
  // See if the card is present and can be initialized:
  //TODO. Open a dialog with advertising of this situation

  while (!SD.begin(SD_CS))
  {
    tft.fillScreen(BLACK);
    Serial.println("Card failed, or not present");
    tft.println("Card failed, or not present");
    //delay(1000);
  }
  tft.println("Card initialized");
  Serial.println("card initialized");

  pinMode(cenButtonPin, INPUT_PULLUP);
  pinMode(rightButtonPin, INPUT_PULLUP);
  pinMode(leftButtonPin, INPUT_PULLUP);
  pinMode(downButtonPin, INPUT_PULLUP);
  pinMode(upButtonPin, INPUT_PULLUP);
  


  //  Serial.begin(256000);
  Serial.begin(115200);


  pinMode(rcaPin, INPUT_PULLUP);

  scale.begin(DOUT, CLK);
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

  scale.power_down();

  EEPROM.get(forceGoalAddress, forceGoal);
  if (isnan(forceGoal)) {
    EEPROM.put(forceGoalAddress, 300);
  }

  EEPROM.get(groupAddress, group);
  if (group == 65535) {
    group = 0;
    EEPROM.put(groupAddress, 0);
  }

  EEPROM.get(ppsAddress, pps);
  //if pps is 0 it means that it has never been set. We use the default value
  if (pps < 0) {
    pps = 40;
    EEPROM.put(ppsAddress, pps);
  }



  //Start TFT
  tft.begin();
#ifdef teensy_3_2
  tft.setRotation(1);
#endif

#ifdef teensy_4_0
  tft.setRotation(1);
#endif

  dirName = createNewDir();
  //totalPersons = getTotalPerson();
  readPersonsFile();

  //TODO: Read exercises only if necessary
  currentExerciseType = 0;

  tft.fillScreen(BLACK);
  
  drawUpperBar(mainMenu, mainMenuItems);

  Serial.println("Microlab-" + version);
  drawMenuBackground();
  backMenu();
  showMenuEntry(currentMenuIndex);
}

void loop()
{
  // Serial.println("<loop");
  if (!capturing)
  {
    updateButtons();
    while(!leftButton.fell() && !rightButton.fell() && !cenButton.fell()) 
    {
      updateButtons();
      if (Serial.available()) serialEvent();
    }
    showMenu();
  } else captureRaw();

  //With Teensy serialEvent is not called automatically after loop
  // Serial.println("loop>");
}

void getLoadCellDynamics(void)
{
  measured = scale.get_units();
  lastMeasuredTime = totalTime;
  Serial.println(String(lastMeasuredTime) + ";" + String(measured));
  //When current Force Slot is equal to size of the buffer it starts over to 0
  currentFSlot = (currentFSlot + 1) % freq;
  //wHEN current Time Slot is equal to the size of the buffer it starts over to 0
  currentTSlot = (currentTSlot + 1) % samples200ms;

  if (currentTSlot > 0) elapsed1Sample = true;    //There's a previous sample
  if (currentTSlot >= (samples200ms - 1)) elapsed200 = true;
  if (currentTSlot >= (samples100ms - 1)) elapsed100 = true;

  forces1s[currentFSlot] = measured;
  totalTimes1s[currentTSlot] = lastMeasuredTime;

  //Calculating the average during 1s
  float sumForces = 0;
  for (unsigned int i = 0; i < freq; i++) {
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
                (lastMeasuredTime - totalTimes1s[(currentTSlot + samples200ms - 1) % samples200ms]) / 1e6);  //Elapsed time between 2 samples
  }

  if (elapsed200) {
    RFD200 = (measured - forces1s[(currentFSlot + freq - samples200ms) % freq]) /     //Increment of the force in 200ms
             ((lastMeasuredTime - totalTimes1s[(currentTSlot + 1) % samples200ms]) / 1e6);          //Increment of time
    if (RFD200 > maxRFD200) maxRFD200 = RFD200;
  }

  if (elapsed100) {
    RFD100 = (measured - forces1s[(currentFSlot + freq - samples100ms) % freq]) /     //Increment of the force in 200ms
             ((lastMeasuredTime - totalTimes1s[(currentTSlot + samples200ms - samples100ms) % samples200ms]) / 1e6); //Increment of time
    if (RFD100 > maxRFD100) maxRFD100 = RFD100;
  }
}


void printTftValue (float val, int x, int y, int fontSize, int decimal) {
  printTftValue (val, x, y, fontSize, decimal, WHITE);
}
void printTftValue (float val, int x, int y, int fontSize, int decimal, int color) {

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
    x = x - valLength * charWidth;
  }

  // In negatives numbers the units are in the same position and the minus one position to the left
  if (val < 0) {
    x = x - 1 * charWidth;
  }
  tft.setTextColor(color);
  tft.setTextSize(fontSize);
  tft.setCursor(x , y);
  tft.print(val, decimal);
  tft.setTextColor(WHITE);
}

void printTftText(String text, int x, int y) {
  printTftText(text, x, y, WHITE, 2, alignLeft);
}
void printTftText(String text, int x, int y, unsigned int color) {
  printTftText(text, x, y, color, 2, alignLeft);
}
void printTftText(String text, int x, int y, unsigned int color, int fontSize) {
  printTftText(text, x, y, color, fontSize, alignLeft);
}
void printTftText(String text, int x, int y, unsigned int color, int fontSize, alignType align)
{
  if (align == alignRight)
  {
    int len = text.length();
    x = x - 6 * fontSize * len;
  } else if (align == alignCenter)
  {
    int len = text.length();
    x = x - 3 * fontSize * len;
  }
  tft.setTextColor(color);
  tft.setTextSize(fontSize);
  tft.setCursor(x, y);
  tft.print(text);

  tft.setTextColor(WHITE);
  tft.setTextSize(2);
}

void serialEvent() {
  String inputString = Serial.readString();
  String commandString = inputString.substring(0, inputString.lastIndexOf(":"));
  String parameters = inputString.substring(inputString.lastIndexOf(":") + 1);

  if (commandString == "start_capture") {
    PcControlled = true;
    startLoadCellCapture();
    //captureRaw();
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
  } else if (commandString == "addPerson") {
    addPerson(parameters);
  } else if (commandString == "readPersonsFile") {
    readPersonsFile();
  } else if (commandString == "getPersons") {
    printPersonsList();
  } else if (commandString == "savePersons") {
    savePersonsList();
    Serial.println("Saved in GROUP" + String(group) + ".TXT");
  } else if (commandString == "listDir") {
    File dir = SD.open("/");
    printDirectory(dir, 2);
    dir.close();
  } else if (commandString == "getFile") {
    sendFile(parameters);
  } else if (commandString == "setGroup") {
    setGroup(parameters);
  } else if (commandString == "getGroup") {
    Serial.println(group);
  } else if (commandString == "addJumpType") {
    addJump(parameters);
    //Serial.println("Jump added");
  } else if (commandString == "getJumpTypes") {
    printJumpTypes();
  } else if (commandString == "saveJumpTypes") {
    saveJumpsType();
  } else if (commandString == "deleteJumpTypes") {
    totalJumpTypes = 0;
  } else if (commandString == "readExercisesFile") {
    readExercisesFile(parameters);
  } else if (commandString == "getGravitatoryTypes") {
    printGravTypes();
  } else if (commandString == "addGravitatoryType") {
    addGravitatory(parameters);
    //Serial.println("Gravitatory added");  
  } else if (commandString == "deleteGravitatoryTypes") {
    totalGravTypes = 0;
  } else if (commandString == "saveGravitatoryTypes") {
    saveGravitatoryType();
  } else if (commandString == "getInertialTypes") {
    printInertTypes();
  } else if (commandString == "addInertialType") {
    addInertial(parameters);
    //Serial.println("Gravitatory added");  
  } else if (commandString == "deleteInertialTypes") {
    totalInertTypes = 0;
  } else if (commandString == "saveInertialTypes") {
    saveInertialType();
  } else if (commandString == "addInertialMachine") {
    addInertMachine(parameters);
  } else if (commandString == "saveInertialMachines") {
    saveInertMachines();
  } else if (commandString == "readInertialMachinesFile") {
    readInertMachineFile();
  } else if (commandString == "getForceTypes") {
    printForceTypes();
  } else if (commandString == "addForceType") {
    addForce(parameters);
  } else if (commandString == "deleteForceTypes") {
    totalForceTypes = 0;
  } else if (commandString == "saveForceTypes") {
    saveForceType();
  } else if (commandString == "getRaceAnalyzerTypes") {
    printRaceAnalyzerTypes();
  } else if (commandString == "addRaceAnalyzerType") {
    addRaceAnalyzer(parameters);
  } else if (commandString == "deleteRaceAnalyzereTypes") {
    totalRaceAnalyzerTypes = 0;
  } else if (commandString == "saveRaceAnalyzerTypes") {
    saveRaceAnalyzerTypes();
  } else if (commandString == "startRaceAnalyzerCapture") {
    PcControlled = true;
    startRaceAnalyzerCapture();
  } else if (commandString == "endRaceAnalyzerCapture") {
    endRaceAnalyzerCapture();
  } else if (commandString == "set_pps") {
    set_pps(inputString);
  } else if (commandString == "get_pps") {
    get_pps();
  } else if (commandString == "readCalibrationsFile") {
    readCalibrationsFile();
  } else if (commandString == "addCalibration") {
    addCalibration(parameters);
  } else if (commandString == "getCalibrations") {
    printCalibrationsList();
  } else if (commandString == "getRtcTime") {
    getRtcTime();
  } else if (commandString == "setRtcTime") {
    setRtcTime(parameters);
  } else {
    Serial.println("Not a valid command");
  }
  inputString = "";

}

void get_version()
{
  //Device string not in a variable due to memory optimization
  Serial.print("MiChroLab-");
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
  scale.power_up();
  //Reading the argument of the command. Located within the ":" and the ";"
  String weightString = get_command_argument(inputString);
  float weight = weightString.toFloat();
  //mean of 255 values comming from the cell after resting the offset.
  double offsetted_data = scale.get_value(50);

  //offsetted_data / calibration_factor
  float calibration_factor = offsetted_data / weight / 9.81; //We want to return Newtons.
  scale.set_scale(calibration_factor);
  scale.power_down();
  EEPROM.put(calibrationAddress, calibration_factor);
  Serial.print("Calibrating OK:");
  Serial.println(calibration_factor);
}

void tare()
{
  scale.power_up();
  printTftText(currentMenu[currentMenuIndex].description, 12, 100, BLACK);
  printTftText("Taring...", 120, 100);
  scale.tare(50); //Reset the scale to 0 using the mean of 255 raw values
  EEPROM.put(tareAddress, scale.get_offset());
  printTftText("Taring...", 120, 100, BLACK);
  printTftText("Tared", 120, 100);

  Serial.print("Taring OK:");
  Serial.println(scale.get_offset());

  delay(300);
  printTftText("Tared", 120, 100, BLACK);
  showMenuEntry(currentMenuIndex);
  scale.power_down();
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

void calibrateTFT(void) {
  float weight = selectValueDialog("Select the weight to use", "1,5,100", "1,5", 0);
  String calibrateCommand = "calibrate:" + String(weight, DEC) + ";";
  calibrate(calibrateCommand);
  printTftText("Calibrated", 120, 150);
  delay(300);
  printTftText("Calibrated", 120, 150, 2, BLACK);
  drawMenuBackground();
  showMenuEntry(currentMenuIndex);
}

//function to read battery level. Not implemented in SportAnalyzer hardware version 1.0
void showBatteryLevel() {
  //float sensorValue = analogRead(A0);

}

//TODO: Add more information or eliminate
void showSystemInfo(void) {

  //Erases the description of the upper menu entry
  printTftText(currentMenu[currentMenuIndex].description,12,100,BLACK);
  drawLeftButton("-", BLACK, BLACK);
  drawRightButton("Back", WHITE, RED);

  printTftText("System Info", 100, 100);
  cenButton.update();
  while (!cenButton.fell()) {
    cenButton.update();
  }

  printTftText("System Info", 100, 100, BLACK);
  showMenuEntry(currentMenuIndex);
}

void setForceGoal()
{
  forceGoal = selectValueDialog("Select the force goal in Newtons.\nAn horizontal red line will be drawn", "10,50,1000,10000", "10,100,500", 0);
  Serial.println(forceGoal);
  menuItemsNum = systemMenuItems;
  showMenuEntry(currentMenuIndex);
}



//Create a new folder with an incremental number if necessari
String createNewDir()
{
  //Checking dirs in /
  int numDirs = 0;
  String lastDirName;
  File dir = SD.open("/");
  File file = dir.openNextFile();

  while (file) {
    if (dir.isDirectory())
    {
      lastDirName = file.name();
      if (lastDirName.substring(0, 2) == "ML" && lastDirName.substring(2, 5).toInt() < 1000)
      {
        numDirs++;
      }
    }
    file.close();
    file = dir.openNextFile();
  }

  file.close();
  
  File lastDir = SD.open(lastDirName.c_str());
  file = lastDir.openNextFile();
  //The dir is not empty or the group is not the same, create a new dir
  if (file || lastDirName.substring(7) != String(group)) {
    //Serial.println("File name: " + String(file.name()));
    numDirs++;
    dirName = "ML" + addLeadingZeros(numDirs, 4) + "G" + String(group);
    SD.mkdir(dirName.c_str());
    file.close();
    //The dir is empty reuse the lastDir
  } else {  
    //Serial.println("Empty dir");
    dirName = lastDirName;
  }
  lastDir.close();
  return (dirName.c_str());
}

String addLeadingZeros(int number, int totalDigits)
{
  int leadingZeros = 0;
  if (number != 0) leadingZeros = (totalDigits - (floor(log10(number)) + 1));
  else if (number == 0) leadingZeros = totalDigits - 1;
  String fixLenNumber = String(number);
  for (int i = 1; i <= leadingZeros; i++)
  {
    fixLenNumber = "0" + fixLenNumber;
  }
  return (fixLenNumber);
}

void calibrateInertial()
{
  printTftText(currentMenu[currentMenuIndex].description, 12, 100, BLACK);
  printTftText("Extend the rope or belt.\nOnce extended press cenButton", 12, 100);
  drawLeftButton("-", BLACK, BLACK);
  drawRightButton("Ok", WHITE, RED);
  
  printTftText("Position: ", 12, 190);
  position = encoder.read();
  printTftText(position, 124, 190);
  cenButton.update();
  while (!cenButton.fell())
  {
    position = encoder.read();
    if (position != lastPosition) {
      printTftText(lastPosition, 124, 190, BLACK);
      printTftText(position, 124, 190);
      lastPosition = position;
    }
    cenButton.update();
  }

  //Deleting text
  printTftText("Extend the rope or belt.\nOnce extended press cenButton", 12, 100, BLACK);
  drawLeftButton("-", BLACK, BLACK);
  drawRightButton("Ok", BLACK, BLACK);
  printTftText("Position: ", 12, 190, BLACK);
  printTftText(lastPosition, 124, 190, BLACK);

  printTftText("Calibrated", 100, 150, WHITE, 3);
  delay(500);
  printTftText("Calibrated", 100, 150, BLACK, 3);

  printTftText(currentMenu[currentMenuIndex].description, 12, 100);

  encoder.write(0);
  Serial.print(encoder.read());
  lastPosition = 0;
  calibratedInertial = true;
}

void fakeFunction()
{
}

void resultsBackground()
{
  tft.fillScreen(BLACK);
  printTftText("Results", 100, 0, WHITE, 3);

  tft.drawLine(0, 20, 320, 20, GREY);
  tft.drawLine(160, 240, 160, 20, GREY);
}


void printBarsValues(int currentIndex)
{
  for (int i = 9; i > 0; i--)
  {
    Serial.print(bars[i]);
    if (i == currentIndex) Serial.print("*");
    Serial.print("\t");
  }
  Serial.println();
}

void printDirectory(File dir, int numTabs)
{
  while (true) {
    File entry =  dir.openNextFile();
    if (! entry) {
      // no more files
      break;
    }
    for (uint8_t i = 0; i < numTabs; i++) {
      Serial.print('\t');
    }
    Serial.print(entry.name());
    if (entry.isDirectory()) {
      Serial.println("/");
      printDirectory(entry, numTabs + 1);
    } else {
      // files have sizes, directories do not
      Serial.print("\t\t");
      Serial.println(entry.size(), DEC);
    }
    entry.close();
  }
}

void addInertMachine(String row)
{
  int prevComaIndex = row.indexOf(":");
  int nextComaIndex = row.indexOf(",");
  //totalinertMachines = row.substring(prevComaIndex + 1, nextComaIndex).toInt();
  inertMachines[totalInertMachines].id = row.substring(prevComaIndex + 1, nextComaIndex).toInt();

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  inertMachines[totalInertMachines].name = row.substring(prevComaIndex + 1 , nextComaIndex);

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  inertMachines[totalInertMachines].description = row.substring(prevComaIndex + 1, nextComaIndex);

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  inertMachines[totalInertMachines].diameters = row.substring(prevComaIndex + 1 , nextComaIndex);

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  inertMachines[totalInertMachines].gearedDown = row.substring(prevComaIndex + 1, nextComaIndex).toFloat();
  totalInertMachines++;
}

void saveInertMachines()
{
  SD.remove("/CONFIGS/INERMACH.TXT");
 
  File inertFile = SD.open("/CONFIGS/INERMACH.TXT", FILE_WRITE);

//  if(gravFile) Serial.println("File created");
//  else Serial.println("Error creating file");

  for (unsigned int i = 0; i < totalInertMachines; i++)
  {
    inertFile.print(inertMachines[i].id);
    inertFile.print("," + inertMachines[i].name);
    inertFile.print("," + inertMachines[i].description );
    inertFile.print("," + inertMachines[i].diameters);
    inertFile.println("," + String(inertMachines[i].gearedDown));
  }
  inertFile.close();
  Serial.println("Saved " + String(totalInertMachines) + " to /CONFIGS/INERMACH.TXT");
}

void readInertMachineFile()
{
  // Serial.println("<readInertialMachinesFile");
  char readChar;
  String readString = "";
  unsigned long pos = 0;    //Position in the file
  int numRows = 0;          //Number of valid rows in the file

  File  machinesFile = SD.open("/CONFIGS/INERMACH.TXT");

  if (!machinesFile) {
    Serial.println("Error opening /CONFIGS/INERMACH.TXT");
    return;
  }

  //Serial.println("File size = " + String(exercisesFile.size() ) );
  while (pos <= machinesFile.size())
  {
    readChar = '0';
    String readString = "";
    while (readChar != '\n' && readChar != '\r' && pos <= machinesFile.size())
    {
      readChar = machinesFile.read();
      readString = readString + readChar;
      pos++;
    }
    
    //Serial.print(readString);

    //Check that it is a valid row.
    if ( isDigit(readString[0]) )
    {
      numRows++;
      currentInertMachine = numRows - 1;
      addInertMachine(readString);
    }
  }
  
  totalInertMachines = numRows;
  Serial.println("Total:" + String(totalInertMachines));
  machinesFile.close();
  // Serial.println("readInertialMachinesFile>");
}

void showList(int color)
{
  int xPos = 10;
  int midYPos = 110;
  int currentY = 0;
  for (int i = -3; i <= 3; i++) {
    if (i == 0) {
      //Do nothing
    } else {
      if (i < 0 ) {
        currentY = midYPos + i * 16 - 3;
      } else if (i > 0) {
        currentY = midYPos + i * 16 + 8;
      }
      printTftText(textList[i+3], xPos, currentY, color, 2);
    }
  }
  tft.fillRoundRect(0, midYPos -1 ,320, 25, 5, RED);
  printTftText(textList[3], xPos, midYPos, color, 3);
}

void sendFile(String fullFileName)
{
  //Extracting the text before the ";"
  fullFileName = fullFileName.substring(0, fullFileName.indexOf(";"));
  Serial.println("Retrieving file \"" + fullFileName + "\"");
  if (! SD.exists(fullFileName.c_str()) )
  {
    Serial.println("File not found");
    return;
  } 
  File file = SD.open(fullFileName.c_str(), FILE_READ);
  //Serial.println(file.name());
  unsigned long int fileSize = file.size();
  unsigned long int pos = 0;
  while (pos < fileSize)
  {
    Serial.print((char)file.read());
    pos++;
  }
  file.close();
}

void getRtcTime()
{
  Serial.print(hour());
  Serial.print(":");
  Serial.print(minute());
  Serial.print(":");
  Serial.print(second());
  Serial.print(" ");
  Serial.print(year());
  Serial.print("/");
  Serial.print(month());
  Serial.print("/");
  Serial.print(day());
  Serial.println();
}

//Sets the time of Teensy RTC. Seconds since 1970/1/1 0h:0m:0s
void setRtcTime(String time)
{
  setTime( (time_t)time.toInt() );
  Serial.print("tine set to: ");
  getRtcTime();
}

time_t getTeensy3Time() {
  return Teensy3Clock.get();
}

void updateButtons(void){
  rightButton.update();
  leftButton.update();
  upButton.update();
  downButton.update();
  cenButton.update();
}

void addCalibration(String row)
{
  int prevComaIndex = row.indexOf(":");
  int nextComaIndex = row.indexOf(",");
  
  calibrations[totalCalibrations].id = row.substring(prevComaIndex + 1, nextComaIndex).toInt();

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  calibrations[totalCalibrations].tare = row.substring(prevComaIndex + 1 , nextComaIndex).toInt();
  
  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  calibrations[totalCalibrations].calibration = row.substring(prevComaIndex + 1 , nextComaIndex).toFloat();

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.length() - 1; //Eliminating the last character (end of line)
  calibrations[totalCalibrations].description = row.substring(prevComaIndex + 1 , nextComaIndex);

  totalCalibrations++;
}

void readCalibrationsFile()
{
  /*
    Example of calibrations file format
    0,14241,915.00,Galga de 200kg
    1,15000,896.00,Galga del 90-20
  */
  String row = "";
  char readChar;
  String rowString = "";
  unsigned long pos = 0;    //Position in the file
  String fileName = "/CONFIGS/CALIBRAT.TXT";
  File  calibrationsFile = SD.open(fileName.c_str());
  currentCalibration = 0;

  //Checking that the file is read
  if (!calibrationsFile) {
    Serial.println("error opening " + fileName);
    return;
  }

  calibrationsFile.seek(0);
  totalCalibrations = 0;

  // read from the file until there's nothing else in it:
  while ( pos <= calibrationsFile.size() )
  {
    readChar = '0';
    rowString = "";

    //Reading the new row
    while (readChar != '\n' && readChar != '\r' && pos <= calibrationsFile.size())
    {
      readChar = calibrationsFile.read();
      rowString = rowString + readChar;
      pos++;
    }

    if ( isDigit(rowString[0]) )
    {
      addCalibration(rowString);
    }
  }
  // close the file:
  calibrationsFile.close();
}

void printCalibrationsList()
{
  Serial.println("Current calibration:" + String(calibrations[currentCalibration].id));
  for (unsigned int i = 0; i < totalCalibrations; i++)
  {
    Serial.print(calibrations[i].id);
    Serial.print(",");
    Serial.print(calibrations[i].tare);
    Serial.print(",");
    Serial.print(calibrations[i].calibration);
    Serial.print(",");
    Serial.println(calibrations[i].description);
  }
}

//Setting how many pulses are needed to get a sample
void set_pps(String inputString)
{
  String argument = get_command_argument(inputString);
  int newPps = argument.toInt();
  if (newPps != pps) {  //Trying to reduce the number of writings
    EEPROM.put(ppsAddress, newPps);
    pps = newPps;
  }
  Serial.print("pps set to: ");
  Serial.println(pps);
}

void get_pps()
{
  Serial.println(pps);
}
