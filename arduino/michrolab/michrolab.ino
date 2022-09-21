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
#include "michrolab.h"

//Version number //it always need to start with: "MiChroLab-"
//Device commented for memory optimization
String version = "0.1";

//#define teensy_3_2
#define teensy_4_0

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
  jumps,
  inertial,
  gravitatory,
  force,
  encoderRace,
  photocelRace
};

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


const unsigned int rcaPin = 16;
volatile bool rcaState = LOW;
bool lastRcaState = LOW;         //The previous state of the RCA

unsigned int currentMenuIndex = 0;              //Main menu state
unsigned int submenu = 0;           //submenus state

//!!Atention I don't understant why but eliminating this line returns error in menuEntry initialitzation
functionPointer FArray[3] = {&fakeFunction, &fakeFunction, &fakeFunction};

menuEntry mainMenu[10] = {
  { "Jumps", "Shows bars with the jumps height", &jumpsCapture},
  //  { "Drop Jumps", "Jumps with a previous\nfalling height (previous\njump or fixed height)\nShows bars with the heightof jumps", &dropJumpsCapture},
  { "Raw Force", "Shows standard graph of\nthe force and the summary of the set.\n(Maximum Force, RFD and\nImpulse)", &startLoadCellCapture},
  { "Lin. Velocity", "Show bars of linear velocity", &startGravitEncoderCapture },
  { "Inert. Velocity", "Show a bars of the velocity of the person in inertial machines", &startInertEncoderCapture },
  { "RaceAnalyzer", "Measure speed with a raceAnalyzer", &startRaceAnalyzerCapture},
  { "RawPower", "Measure Force and Speed\nat the same time.\nOnly power is shown in thegraph", &startPowerCapture},
  //{ "Tared Force", "Offset the force before\nmeasuring it.\nUseful to substract body\nweight.", &startTareCapture},
  { "F. Steadiness", "RMSSD and cvRMSSD.\nSteadynessof the force.\nWhen ready, press the Red Button to get the\nsteadiness of the next 5s.", &startSteadiness},
  { "System", "Performs calibration or\ntare and shows some system\ninformation.", &showSystemMenu}
};

int mainMenuItems = 8;

menuEntry systemMenu[10] {
  { "Group", "Select the group you are going to use.\nUp to 9 groups can be\nselected", &selectGroup},
  { "Tare", "Set the offset of the\nsensor.", &tare },
  { "Calibrate", "Set the equivalence\nbetween the sensor values\nand the force measured.", &calibrateTFT },
  { "Force Goal", "Set the goal force for\nsteadiness measurements.", &setForceGoal },
  { "Inert. Calib.", "Set the Exact point in which the concentric phase ends", &calibrateInertial},
  { "Info", "Hardware, firmware and config information.", &showSystemInfo },
  { "Exit", "Goes back to main menu", &backMenu },
  { "", "", &backMenu},
  { "", "", &backMenu},
  { "", "", &backMenu}
};

int systemMenuItems = 7;

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

// Display variables
const int chipSelect = 6;


#ifdef teensy_4_0
#define TFT_DC      20
#define TFT_CS      10
#define TFT_RST    255  // 255 = unused, connect to 3.3V
#define TFT_MOSI    11
#define TFT_MISO    12
#define TFT_SCLK    13

const unsigned int redButtonPin = 5;
const unsigned int blueButtonPin = 4;
#endif

#ifdef teensy_3_2
#define TFT_DC      20
#define TFT_CS      21
#define TFT_RST    255  // 255 = unused, connect to 3.3V
#define TFT_MISO    12
#define TFT_MOSI     7
#define TFT_SCLK    14

const unsigned int redButtonPin = 4;
const unsigned int blueButtonPin = 5;
#endif

Bounce redButton = Bounce(redButtonPin, 50);
Bounce blueButton = Bounce(blueButtonPin, 50);

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

IntervalTimer rcaTimer;
String fullFileName;
File dataFile;
int sampleNum = 0;
String fileBuffer;        //Text mode
//char fileBuffer[100];       //binary mode. Using char type cannot write speeds greater than +-127 pulses/mm

String textList[7] = {"First", "Second", "Thirth", "Fourth", "Fifth", "Sixtth", "Seventh"} ;

void setup() {
  //Attention: some SD cards fails to initalize after uploading the firmware
  // See if the card is present and can be initialized:
  //TODO. Open a dialog with advertising of this situation

  while (!SD.begin(chipSelect))
  {
    Serial.println("Card failed, or not present");
    tft.println("Card failed, or not present");
    //delay(1000);
  }
  tft.println("Card initialized");
  Serial.println("card initialized");

  pinMode(redButtonPin, INPUT_PULLUP);
  pinMode(blueButtonPin, INPUT_PULLUP);

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

  //Start TFT
  tft.begin();
#ifdef teensy_3_2
  tft.setRotation(1);
#endif

#ifdef teensy_4_0
  tft.setRotation(3);
#endif

  dirName = createNewDir();
  //totalPersons = getTotalPerson();
  readPersonsFile();

  //TODO: Read exercises only if necessary
  currentExerciseType = 0;

  tft.fillScreen(BLACK);

  Serial.println("Microlab-" + version);

  drawMenuBackground();
  backMenu();
  showMenuEntry(currentMenuIndex);
}

void loop()
{
  if (!capturing)
  {
    showMenu();
  } else
  {
    captureRaw();
  }

  //With Teensy serialEvent is not called automatically after loop
  if (Serial.available()) serialEvent();
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
  printTftText(text, x, y, WHITE, 2, false);
}
void printTftText(String text, int x, int y, unsigned int color) {
  printTftText(text, x, y, color, 2, false);
}
void printTftText(String text, int x, int y, unsigned int color, int fontSize) {
  printTftText(text, x, y, color, fontSize, false);
}
void printTftText(String text, int x, int y, unsigned int color, int fontSize, bool alignRight)
{
  if (alignRight)
  {
    int len = text.length();
    x = x - 6 * fontSize * len;
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
  } else {
    Serial.println("Not a valid command");
  }
  inputString = "";

}

void startLoadCellCapture(void)
{
  attachInterrupt(rcaPin, changedRCA, CHANGE);
  Serial.println("Starting capture...");
  scale.power_up();
  totalTime = 0;
  measuredMax = scale.get_units();
  impulse = 0;
  maxRFD100 = 0;
  maxRFD200 = 0;

  //filling the array of forces ant times with initial force
  lastMeasure = scale.get_units();
  measured = lastMeasure;
  for (unsigned int i = 0; i < freq; i++) {
    forces1s[i] = lastMeasure;
  }

  for (unsigned int i = 0; i < samples200ms; i++) {
    totalTimes1s[i] = 0;
  }

  maxMeanForce1s = lastMeasure;

  //Initializing variability variables
  sumSSD = 0.0;
  sumMeasures = lastMeasure;
  samplesSSD = 0;
  capturing = true;
  sensor = loadCell;
  maxString = "F";
  plotPeriod = 5;
  if (capturingSteadiness) {
    newGraphMin = -10;
    newGraphMax = forceGoal * 1.5;
  } else if (capturingSteadiness)  {
    newGraphMin = forceGoal * 0.5;
    newGraphMax = forceGoal * 1.5;
  } else {
    newGraphMin = -100;
    newGraphMax = max(100, measuredMax * 1.5);
  }

  selectPersonDialog();
  if (totalForceTypes == 0) readExercisesFile(force);
  selectExerciseType(force);
  if(forceTypes[currentExerciseType].tare)
  {
    tft.fillScreen(BLACK);
    printTftText(currentMenu[currentMenuIndex].description, 12, 100, 2);
    printTftText("Taring...", 100, 100);
    scale.tare(50); //Reset the scale to 0 using the mean of 255 raw values
    printTftText("Taring...", 100, 100, BLACK);
    printTftText("  Tared  ", 100, 100);
    delay(300);
  }
  
}

void endLoadCellCapture()
{
  detachInterrupt(rcaPin);
  capturing = false;
  sensor = none;
  Serial.println("Capture ended:");

  //If the device is controlled by the PC the results menu is not showed
  //because during the menu navigation the Serial is not listened.
  if (!PcControlled) {
    //Restoring tare value in the EEPROM. Necessary after Tare&Capture
    EEPROM.get(tareAddress, tareValue);
    scale.set_offset(tareValue);
    //Serial.println(scale.get_offset());
    showLoadCellResults();
  }
  scale.power_down();
  drawMenuBackground();
  showMenuEntry(currentMenuIndex);
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
}

/*
void startTareCapture(void)
{

  printTftText(currentMenu[currentMenuIndex].description, 12, 100, 2);
  printTftText("Taring...", 100, 100);
  scale.tare(50); //Reset the scale to 0 using the mean of 255 raw values
  printTftText("Taring...", 100, 100, BLACK);
  printTftText("  Tared  ", 100, 100);
  delay(300);
  startLoadCellCapture();
}
*/

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

//Any change in the RCA activates the timer
void changedRCA() {
  rcaTime = totalTime;
  rcaState = digitalRead(rcaPin);
  rcaTimer.begin(rcaDebounce, rcaDebounceTime);
  if (sensor == raceAnalyzer)
  {
    raceAnalyzerSample.displacement = encoder.read();
    encoder.write(0);
  }
  //Serial.print("-");
}

//After the debounce time the state of the RCA is checked again to see if it has changed
void rcaDebounce()
{
  rcaTimer.end();
  rcaState = !digitalRead(rcaPin);
  if (rcaState != lastRcaState)
  {
    lastRcaState = rcaState;
    rcaFlag = true;
  }
}

void calibrateTFT(void) {
  float weight = selectValueDialog("Select the weight to use", "1,5,100", "1,5", 0);
  String calibrateCommand = "calibrate:" + String(weight, DEC) + ";";
  calibrate(calibrateCommand);
  printTftText("Calibrated", 120, 150);
  delay(300);
  printTftText("Calibrated", 120, 150, 2, BLACK, false);
  drawMenuBackground();
  showMenuEntry(currentMenuIndex);
}

//function to read battery level. Not implemented in SportAnalyzer hardware version 1.0
void showBatteryLevel() {
  //float sensorValue = analogRead(A0);

}

void updateJumpTime()
{
  updateTime(0, jumpTypes[currentExerciseType].timeLimit);
}

//Shows time in seconds at right lower corner. If limit is present non-zero it will we a countdown
void updateTime() {
  updateTime( 0, 0.0 );
}
void updateTime( unsigned int decimals ) {
  updateTime( decimals, 0.0 );
}
void updateTime( unsigned int decimals, float limit )
{
  tft.fillRect(268, 215, 48, 16, BLACK);
  //  if (totalTime > 1000000)
  //  {
  //    tft.setTextColor(BLACK);
  //    printTftValue(totalTime / 1000000 - 1, 302, 215, 2, 0);
  //  }
  if (limit == 0) {
    printTftValue(totalTime / 1000000, 302, 215, 2, decimals);
  }
  else if (limit != 0) {
    printTftValue(limit - totalTime / 1000000, 302, 215, 2, decimals);
  }
}

//TODO: Add more information or eliminate
void showSystemInfo(void) {

  //Erases the description of the upper menu entry
  printTftText(currentMenu[currentMenuIndex].description, 12, 100, 2, BLACK, false);

  printTftText("System Info", 100, 100);
  redButton.update();
  while (!redButton.fell()) {
    redButton.update();
  }
  printTftText("System Info", 100, 100, BLACK);
  showMenuEntry(currentMenuIndex);
}

void showLoadCellResults() {
  int textSize = 2;
  tft.fillScreen(BLACK);
  printTftText("Results", 100, 100, 3, BLACK);

  tft.drawLine(0, 20, 320, 20, GREY);
  tft.drawLine(160, 240, 160, 20, GREY);
  tft.setTextSize(textSize);

  printTftText("F", 0, 40);
  printTftText("max", 12, 48, WHITE, 1);
  printTftValue(measuredMax, 112, 40, textSize, 0);

  printTftText("F", 170, 40);
  printTftText("max1s", 182, 48, WHITE, 1);

  printTftValue(maxMeanForce1s, 296, 40, textSize, 0);

  printTftText("F", 0, 80);
  printTftText("trig", 12, 88, WHITE);

  printTftValue(forceTrigger, 100, 80, textSize, 1);

  printTftText("Imp", 170, 80);
  printTftValue(impulse, 296, 80, textSize, 0);

  printTftText("RFD", 0, 120);
  printTftText("100", 36, 128, WHITE, 1);
  printTftValue(maxRFD100, 118, 120, textSize, 0);

  printTftText("RFD", 170, 120);

  printTftText("200", 206, 128, WHITE, 1);
  printTftValue(maxRFD200, 298, 120, textSize, 0);

  if (RMSSD != 0)
  {
    printTftText("RMSSD", 0, 160);
    printTftValue(RMSSD, 100, 160, textSize, 1);

    printTftText("CV", 170, 160);
    printTftText("RMSSD", 194, 168, WHITE, 1);
    printTftValue(cvRMSSD, 280, 160, textSize, 1);
  }

  //Red button exits results
  redButton.update();
  while (!redButton.fell()) {
    redButton.update();
  }
  tft.fillRect(0, 20, 320, 240, BLACK);
  drawMenuBackground();
}

void startSteadiness(void)
{
  sensor = loadCell;
  totalTime = 0;
  capturing = true;
  capturingPreSteadiness = true;
  capturingSteadiness = false;
  startLoadCellCapture();
}

void end_steadiness()
{
  capturing = false;
  capturingSteadiness = false;
}

void captureRaw()
{
  writeCaptureHeaders();  

  //Position graph's lower left corner.
  double graphX = 30;
  double graphY = 200;

  //Size of the graph
  double graphW = 290;
  double graphH = 200;

  //Minimum and maximum values to show
  double xMin = 0;
  double xMax = 290;

  //Size and num of divisions
  double yDivSize = 100;
  double yDivN = 10;
  double xDivSize = 100;
  double yBuffer[320];
  yBuffer[0] = measured;

  double plotBuffer[plotPeriod];

  bool resized = true;

  long lastUpdateTime = 0;

  tft.fillScreen(BLACK);

  double xGraph = 1;

  printTftText(maxString, 10, 215, WHITE, 2, false);
  printTftText("max", 22, 223, WHITE, 1);
  printTftText(":", 40, 215, 2, WHITE, false);
  printTftValue(measuredMax, 94, 215, 2, 1);
  if (! PcControlled)
  {
    updatePersonSet();
  }

  while (capturing)
  {
    //Deleting the previous plotted points
    for (int i = xMin; i < xGraph; i++)
    {
      Graph(tft, i, yBuffer[i], graphX, graphY, graphW, graphH, xMin, xMax, xDivSize, graphMin, graphMax, yDivSize, "", "", "", WHITE, WHITE, BLACK, WHITE, BLACK, startOver);
    }
    startOver = true;
    //redrawAxes(tft, graphX, graphY, graphW, graphH, xMin, xMax, graphMin, graphMax, yDivSize, "", "", "", WHITE, BLACK, BLACK, BLACK, BLACK, BLACK, resized);
    graphMax = newGraphMax;
    graphMin = newGraphMin;
    //    Serial.println("Y scale changed. Y limits:");
    //    Serial.print(graphMin);
    //    Serial.print(",\t");
    //    Serial.println(graphMax);
    yDivSize = (graphMax - graphMin) / yDivN;
    if (resized) {
      for (int i = xMin; i < xGraph; i++)
      {
        Graph(tft, i, yBuffer[i], graphX, graphY, graphW, graphH, xMin, xMax, xDivSize, graphMin, graphMax, yDivSize, "", "", "", WHITE, WHITE, BLUE, WHITE, BLACK, startOver);
      }
    }
    redrawAxes(tft, graphX, graphY, graphW, graphH, xMin, xMax, graphMin, graphMax, yDivSize, "", "", "", WHITE, GREY, WHITE, WHITE, BLACK, RED, resized);
    resized = false;

    if (xGraph >= xMax) xGraph = 0;
    while ((xGraph < xMax && !resized) && capturing)
    {
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
          if (sensor == incLinEncoder || sensor == incRotEncoder ) getEncoderDynamics();
          else if (sensor == loadCell) getLoadCellDynamics();
          else if (sensor == loadCellincEncoder) getPowerDynamics();
          else if (sensor == raceAnalyzer) getRaceAnalyzerDynamics();

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

        if (!PcControlled) saveData(fileName);
        plotBuffer[n] = measured;

        //Pressing blue or red button ends the capture
        //Check the buttons state
        redButton.update();
        blueButton.update();
        if (redButton.fell())
        {
          n = plotPeriod;
          if (sensor == incLinEncoder || sensor == incRotEncoder)
          {
            endEncoderCapture();
          } else if (sensor == loadCell)
          {
            if (! (capturingPreSteadiness || capturingSteadiness))
            {
              endLoadCellCapture();
              //xGraph = xMax;
            } else if (capturingPreSteadiness)  //In Pre steadiness. Showing force until button pressed
            {
              capturingPreSteadiness = false;
              capturingSteadiness = true;
              printTftValue(totalTime / 1000000, 284, 215, 2, 0, BLACK);
              redrawAxes(tft, graphX, graphY, graphW, graphH, xMin, xMax, graphMin, graphMax, yDivSize, "", "", "", WHITE, BLACK, BLACK, BLACK, BLACK, BLACK, resized);
              startLoadCellCapture();
              newGraphMax = forceGoal * 1.5;
              newGraphMin = forceGoal * 0.5;
              resized = true;
              //              Serial.println("going to change. Future Y limits:");
              //              Serial.print(newGraphMin);
              //              Serial.print(",\t");
              //              Serial.println(newGraphMax);
              redrawAxes(tft, graphX, graphY, graphW, graphH, xMin, xMax, graphMin, graphMax, yDivSize, "", "", "", WHITE, GREY, WHITE, WHITE, BLACK, RED, resized);
              tft.setCursor(80, 10);
              tft.setTextColor(WHITE, RED);
              tft.print("Hold force  5s");
              tft.setTextColor(WHITE);
            }

          } else if (sensor == loadCellincEncoder) {
            endPowerCapture();
          } else if (sensor == raceAnalyzer) {
            endRaceAnalyzerCapture();
          }
          //xGraph = xMax;
        }
        if (blueButton.fell() && !PcControlled)
        {
          selectPerson();
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
          printTftValue(measuredMax, 94, 215, 2, 1, BLACK);
          measuredMax = measured;
          printTftValue(measuredMax, 94, 215, 2, 1);
        }

        if ((lastUpdateTime - totalTime) >= 1000000) {
          lastUpdateTime = totalTime;
          updateTime();
        }
      }
      if (Serial.available()) serialEvent();
    }
  }
  dataFile.close();
  if (!capturingPreSteadiness) setNumber++;
}

void captureBars() {
  captureBars(false);
}
void captureBars(float fullScreen)
{
  writeCaptureHeaders();
  maxString = "V";
  float graphRange = 5;
  int currentSlot = 0;

  tft.fillScreen(BLACK);

  float h = 0;
  if (fullScreen) h = 240;
  else h = 200;

  if (!fullScreen)
  {
    //Info at the lower part of the screen
    printTftText(maxString, 10, 215, WHITE, 2);
    printTftText("max", 22, 223, WHITE, 1);
    printTftText(":", 40, 215, WHITE, 2);
    printTftValue(maxAvgVelocity, 94, 215, 2, 1);
    updatePersonSet(false);
  }

  redrawAxes(tft, 30, h, 290, h, 290, h, 0, graphRange, graphRange / 10, "", "", "", WHITE, GREY, WHITE, WHITE, BLACK, RED, true, 1);

  while (capturing)
  {
    getEncoderDynamics();
    if (redrawBars)
    {
      currentSlot = (numRepetitions - 1) % 10;
      redrawBars = false;
      if (bars[currentSlot] > maxAvgVelocity)
      {
        if (!fullScreen) printTftValue(maxAvgVelocity, 94, 215, 2, 1, BLACK);
        maxAvgVelocity = bars[currentSlot];
        if (!fullScreen) printTftValue(maxAvgVelocity, 94, 215, 2, 1);
      }
      if (bars[currentSlot] > graphRange)
      {
        redrawAxes(tft, 30, h, 290, h, 290, h, 0, graphRange, graphRange / 10, "", "", "", WHITE, GREY, WHITE, WHITE, BLACK, RED, true, 1);
        graphRange = bars[currentSlot] * 1.25;
      }
      redrawAxes(tft, 30, h, 290, h, 290, h, 0, graphRange, graphRange / 10, "", "", "", WHITE, GREY, WHITE, WHITE, BLACK, RED, true, 1);
      barPlot(30, h, 290, h, graphRange, 10, currentSlot, 0.75, RED);
    }
    redButton.update();
    if (redButton.fell())
    {
      endEncoderCapture();
    }
  }
}

void writeCaptureHeaders()
{
  fileName = "S" + addLeadingZeros(setNumber, 2) + "P" + addLeadingZeros(currentPerson, 2);
  if (sensor == loadCell) fileName = fileName + "-F";
  else if (sensor == incLinEncoder) fileName = fileName + "-G";
  else if (sensor == incRotEncoder) fileName = fileName + "-I";
  else if (sensor == loadCellincEncoder) fileName = fileName + "-P";
  else if (sensor == raceAnalyzer) fileName = fileName + "-R";

  fullFileName = "/" + dirName + "/" + fileName + ".TXT";
  Serial.println(fullFileName);
  dataFile = SD.open(fullFileName.c_str(), FILE_WRITE);
  dataFile.println("Person:" + String(persons[currentPerson].index) + "," + persons[currentPerson].name + " " + persons[currentPerson].surname);


  if (sensor == incLinEncoder)
  {
    dataFile.println("Exercise:" + String(gravTypes[currentExerciseType].id) + "," + gravTypes[currentExerciseType].name);
    dataFile.println("Load:" + String(load));
  } else if (sensor == incRotEncoder)
  {
    dataFile.println("Exercise:" + String(inertTypes[currentExerciseType].id) + "," + inertTypes[currentExerciseType].name);
    dataFile.println("Load:" + String(load));
  } else if (sensor == loadCell)
  {
    dataFile.println("Exercise:" + String(forceTypes[currentExerciseType].id) + "," + forceTypes[currentExerciseType].name);
  } else if (sensor == raceAnalyzer)
  {
    dataFile.println("Exercise:" + String(raceAnalyzerTypes[currentExerciseType].id) + "," + raceAnalyzerTypes[currentExerciseType].name);
  }
}

//text mode
void saveEncoderSpeed()
{
  long position = encoder.read();
  String currentValue = String(position - lastSamplePosition) + ",";

  if (PcControlled) {
    Serial.print(currentValue);
  } else {
    fileBuffer = fileBuffer + currentValue;
    sampleNum++;
    if (sampleNum >= 100) {
      dataFile.print(fileBuffer);
      Serial.println(fileBuffer);
      fileBuffer = "";
      sampleNum = 0;
    }
  }
  lastSamplePosition = position;
}

/*
  //binary mode
  void saveEncoderSpeed()
  {
  long position = encoder.read();
  fileBuffer[sampleNum] =(char)(position - lastSamplePosition);
  sampleNum++;
  lastSamplePosition = position;
  if (sampleNum >= 99){
    dataFile.write(fileBuffer, 100);
    sampleNum = 0;
  }
  }
*/

void getEncoderDynamics()
{
  int duration = totalTime - lastMeasuredTime;
  if (duration >= 1000)
  {
    lastMeasuredTime = totalTime;

    long position = encoder.read();

    //TODO: Calculate positoion depending on the parameters of the encoder/machine
    if (inertialMode) position = - abs(position);
    measured = (float)(position - lastPosition) * 1000 / (duration);
    if (measured > measuredMax) measuredMax = measured;
    //measured = position;
    //    if(position != lastPosition) Serial.println(String(localMax) + "\t" + String(lastPosition) +
    //      "\t" + String(position) + "\t" + String(encoderPhase * (position - localMax)));
    float accel = (measured - lastVelocity) * 1000000 / duration;
    if (propulsive && accel <= -9.81) {
      //Serial.println("End propulsive at time: " + String(lastMeasuredTime));
      propulsive = false;
    }
    //measured = position;
    //if (measured != 0) Serial.println(measured);
    //Before detecting the first repetition we don't know the direction of movement
    if (encoderPhase == 0)
    {
      if (position >= minRom) {
        encoderPhase = 1;
        localMax = position;
        //Serial.println("Start in CONcentric");
      }
      else if (position <= -minRom) {
        encoderPhase = 1;
        localMax = position;
        //Serial.println("Start in ECCentric");
      }
    }

    //Detecting the phanse change
    //TODO. Detect propulsive phase
    if (encoderPhase * (position - localMax) > 0)  //Local maximum detected
    {
      //Serial.println("New localMax : " + String(position) + "\t" + String(localMax));
      localMax = position;
    }

    //Checking if this local maximum is actually the start of the new phase
    if (encoderPhase * (localMax - position) > minRom)
    {
      encoderPhase *= -1;
      propulsive = true;
      avgVelocity = (float)(position - startPhasePosition) * 1000 / (lastMeasuredTime - startPhaseTime);
      bars[numRepetitions % 10] = abs(avgVelocity);
      redrawBars = true;

      //printBarsValues();

      numRepetitions++;
      if (avgVelocity > maxAvgVelocity) maxAvgVelocity = avgVelocity;
      //        Serial.println(String(position) + " - " + String(startPhasePosition) + " = " + String(position - localMax) + "\t" + String(encoderPhase));
      //        Serial.println("Change of phase at: " + String(lastMeasuredTime));
      //        Serial.print(String(1000 * (float)(position - startPhasePosition) / (lastMeasuredTime - startPhaseTime)) + " m/s\t" );
      //        Serial.println(String(1000*(persons[currentPerson].weight * 9.81 * (position - startPhasePosition)) /
      //        (lastMeasuredTime - startPhaseTime))+" W");
      startPhasePosition = position;
      startPhaseTime = lastMeasuredTime;
    }
    lastPosition = position;
    lastVelocity = measured;
    //Serial.println(String(measured) + "\t" + String(accel));
  }
}

void startInertEncoderCapture()
{
  inertialMode = true;
  sensor = incRotEncoder;
  if (!calibratedInertial) calibrateInertial();
  
  if(totalInertTypes == 0) readExercisesFile(inertial);
  selectExerciseType(inertial);
  load = selectValueDialog("Select the amount of extra loads attached to the machine", "0,18", "1", 0);
  startEncoderCapture();
}

void startGravitEncoderCapture()
{
  inertialMode = false;
  sensor = incLinEncoder;
  if(totalGravTypes == 0) readExercisesFile(gravitatory);
  selectExerciseType(gravitatory);
  load = selectValueDialog("Select the load you are\ngoing to move", "0,5,20,200", "0.5,1,5", 1);
  startEncoderCapture();
}

void startEncoderCapture(void)
{
  attachInterrupt(rcaPin, changedRCA, CHANGE);
  capturing = true;
  //Serial.println(sensor);
  maxString = "V";
  plotPeriod = 1;
  newGraphMin = -10;
  newGraphMax = 10;
  measuredMax = 0;
  measured = 0;
  totalTime = 0;
  encoderPhase = 0;
  localMax = 0;
  //encoder.write(0);
  lastPosition = 0;
  lastMeasuredTime = 0;
  startPhasePosition = 0;
  startPhaseTime = 0;
  avgVelocity = 0;
  maxAvgVelocity = 0;
  lastVelocity = 0;
  selectPersonDialog();
  //captureRaw();
  encoderTimer.begin(saveEncoderSpeed, 1000);
  captureBars(false);
}

void endEncoderCapture()
{
  detachInterrupt(rcaPin);
  capturing = false;
  encoderTimer.end();
  numRepetitions = ceil((float)(numRepetitions / 2));
  sensor = none;
  Serial.println("Capture ended:");
  dataFile.close();
  //If the device is controlled by the PC the results menu is not showed
  //because during the menu navigation the Serial is not listened.
  tft.fillScreen(BLACK);
  if (!PcControlled) {
    showEncoderResults();
  }
  dataFile.close();
  showMenuEntry(currentMenuIndex);
}

void showEncoderResults()
{
  resultsBackground();
  printTftText("V", 0, 40);
  printTftText("peak", 12, 48, WHITE, 1);
  printTftValue(measuredMax, 100, 40, 2, 1);

  printTftText("Vrep", 170, 40, WHITE);
  printTftText("max", 218, 48, WHITE, 1);
  printTftValue(maxAvgVelocity, 268, 40, 2, 2);

  printTftText("nRep", 0, 80);
  printTftValue(numRepetitions, 100, 80, 2, 0);

  redButton.update();
  while (!redButton.fell()) {
    redButton.update();
  }
  tft.fillRect(0, 20, 320, 240, BLACK);
  drawMenuBackground();
}

void getPowerDynamics()
{
  float force = scale.get_units();
  //position = encoder.read();
  float velocity = (float)(position - lastPosition) * 1000 / (totalTime - lastSampleTime);
  lastSampleTime = totalTime;
  lastPosition = position;
  for (int i = 0; i < encoderBufferIndex; i++)
  {
    Serial.print(encoderBuffer[i]);
    Serial.print(",");
  }
  encoderString = "";
  encoderBufferIndex = 0;
  measured = force * velocity;
  if (measured > maxPower) maxPower = measured;
}

void startPowerCapture(void)
{
  attachInterrupt(rcaPin, changedRCA, CHANGE);
  capturing = true;
  sensor = loadCellincEncoder;
  maxString = "P";
  plotPeriod = 5;
  newGraphMin = -200;
  newGraphMax = 500;
  measuredMax = 0;
  totalTime = 0;

  //Depending on the speed of the clock it can be adjusted
  //96 Mhz and 1000 us captures but the screen refreshing becomes unstable
  //72 Mhz and 2000 us captures but the screen refreshing becomes unstable
  encoderTimer.begin(readEncoder, 2000);
  captureRaw();
}

void readEncoder()
{
  position = encoder.read();
  //  encoderString = encoderString + String(position - lastPosition) + ",";
  encoderBuffer[encoderBufferIndex] = position - lastPosition;
  lastPosition = position;
  encoderBufferIndex++;
}

void endPowerCapture()
{
  detachInterrupt(rcaPin);
  capturing = false;
  encoderTimer.end();
  sensor = none;
  Serial.println("Capture ended:");
  //If the device is controlled by the PC the results menu is not showed
  //because during the menu navigation the Serial is not listened.
  if (!PcControlled) {
    showPowerResults();
  }
  showMenuEntry(currentMenuIndex);
}

void showPowerResults()
{
  resultsBackground();
  printTftText("P", 0, 40, 2, WHITE, false);
  printTftText("peak", 12, 48, 2, WHITE, false);
  printTftValue(measuredMax, 100, 40, 2, 1);

  redButton.update();
  while (!redButton.fell()) {
    redButton.update();
  }
  tft.fillRect(0, 20, 320, 240, BLACK);
  drawMenuBackground();
}

void jumpsCapture()
{
  attachInterrupt(rcaPin, changedRCA, CHANGE);
  if (totalJumpTypes == 0) readExercisesFile(jumps);
  //printJumpTypes();
  selectExerciseType(jumps);
  IntervalTimer testTime;             //Timer that controls the refreshing of time in lower right corner
  capturing = true;
  //In the first change of state the header of the row is writen.
  //TODO: MAKE IT SENSITIVE TO THE startIn parameter
  bool waitingFirstPhase = true;
  bool timeEnded = false;             //Used to manage soft time limit. Allows to wait until the last contact
  float maxJump = 0;
  int bestJumper = 0;
  float graphRange = 50;              //Height of the jumps are in cm
  int index = 0;                      //Specify the slot used in bars[] circular buffer
  bool rowCreated = false;

  fileName = "S" + addLeadingZeros(setNumber,2) + "J" + addLeadingZeros(jumpTypes[currentExerciseType].id,2) + "-J";
  fullFileName = "/" + dirName + "/" + fileName + ".TXT";
  dataFile = SD.open(fullFileName.c_str(), FILE_WRITE);

  lastRcaState = !digitalRead(rcaPin);
  rcaFlag = false;
  //lastPhaseTime can be contactTime or flightTime depending on the phase
  float lastPhaseTime = 0;

  //Initializing values of the bars
  for (int i = 0; i < 10; i++)
  {
    bars[i] = 0;
  }

  //Drawing axes
  tft.fillScreen(BLACK);
  redrawAxes(tft, 30, 200, 290, 200, 290, 200, 0, graphRange, graphRange / 10, "", "", "", WHITE, GREY, WHITE, WHITE, BLACK, RED, true);
  redButton.update();

  updatePersonJump(totalJumps);

  //Print summary results
  printTftText("H:", 10, 215, WHITE, 2, false);
  printTftValue(maxJump, 58, 215, 2, 2);
  totalTestTime = 0;
  totalTime = 0;

  //Draws the time if necessary
  if ( jumpTypes[currentExerciseType].timeLimit > 0 ) updateJumpTime();

  //Pressing the redButton during a test ends it
  while ( !redButton.fell())
  {
    while (capturing && !redButton.fell())
    {
      //Person has changed
      if ( blueButton.fell() ) {
        currentPerson = (currentPerson + 1) % totalPersons;
        updatePersonJump(totalJumps);
        waitingFirstPhase = true;
        totalJumps = 0;
        totalTestTime = 0;
        testTime.end();
        totalTime = 0;
        timeEnded = false;
      }

      //There's been a change in the mat state. Landing or taking off.
      if (rcaFlag)
      {
        //Serial.print("!");
        rcaFlag = false;
        //Calculate the time of the last phase. Flight or Contact time
        lastSampleTime = rcaTime - lastRcaTime;
        lastPhaseTime = ((float)rcaTime - (float)lastRcaTime) / 1E6;  //Time in seconds
        lastRcaState = rcaState;
        lastRcaTime = rcaTime;

        //If there's been a previous contact it means thet this is the start or end of flight time
        if (!waitingFirstPhase) {
          //Serial.print("*");
          dataFile.print(",");
          //Hard coded microsonds precission
          dataFile.print(lastPhaseTime, 6);
          Serial.print(",");
          Serial.print(lastPhaseTime, 6);

          //Stepping on the mat. End of flight time. Starts contact.
          if (rcaState)
          {
            dataFile.print("R");
            Serial.print("R");
            tft.fillRect(30, 0, 290, 200, BLACK);
            bars[index] = 122.6 * lastPhaseTime * lastPhaseTime; //In cm
            //We always add 10 to index to avoid the negative number in (index - 1) when index is 0
            printTftValue(bars[(index + 10 - 1) % 10], 58, 215, 2, 2, BLACK);
            printTftValue(bars[index], 58, 215, 2, 2);
            //Check if a new best jump is performed
            if (bars[index] > maxJump)
            {
              maxJump = bars[index];
              bestJumper = currentPerson;
            }

            if (bars[index] > graphRange)
            {
              redrawAxes(tft, 30, 200, 290, 200, 290, 200, 0, graphRange, graphRange / 10, "", "", "", WHITE, BLACK, WHITE, BLACK, BLACK, RED, true);
              graphRange = bars[index] * 1.25;
            }
            redrawAxes(tft, 30, 200, 290, 200, 290, 200, 0, graphRange, graphRange / 10, "", "", "", WHITE, GREY, WHITE, WHITE, BLACK, RED, true);
            barPlot(30, 200, 290, 200, graphRange, 10, index, 0.5, RED);
            index = (index + 1) % 10;
            totalJumps++;
            updatePersonJump(totalJumps);

            //If soft time limit check if time was ended in the moment of the contact
            if ( timeEnded )
            {
              capturing = false;
              waitingFirstPhase = true;
            }

            //Taking off. Ends contact. start of flight time
          } else if (!rcaState)
          {
            dataFile.print("r");
            Serial.print("r");
          }
          //Waiting first phase.
        } else if (waitingFirstPhase && capturing) {

          //The state  previous to change was WRONG
          //The first change of RCA is in the state that is supposed to be at start of the test.
          if ( jumpTypes[currentExerciseType].startIn == rcaState ) {
            if (rcaState) {             //Landing. Don't measure the Time of Flight
              //Do nothing
            } else if ( !rcaState) {  //Take off.
              //Measure one more jump..
              totalJumps = -1;
              waitingFirstPhase = false;
            }
            //The state previous change was RIGHT
            //The first change of RCA is to the state that is NOT supposed to be at start of the test.
          } else if ( jumpTypes[currentExerciseType].startIn != rcaState) {
            waitingFirstPhase = false;
          }
          totalTestTime = 0;
          totalTime = 0;
          rcaTime = 0;
          lastRcaTime = 0;
          setNumber++;
          if ( !rowCreated ) {
            dataFile.print(String(setNumber) + ","
              + String(currentPerson) + "," + persons[currentPerson].name + " " + persons[currentPerson].surname +
              "," + String(jumpTypes[currentExerciseType].id) + "," + jumpTypes[currentExerciseType].name);
            if (PcControlled)
            {
              Serial.print(String(setNumber) + ","
                + String(currentPerson) + "," + persons[currentPerson].name + " " + persons[currentPerson].surname +
                "," + String(jumpTypes[currentExerciseType].id) + "," + jumpTypes[currentExerciseType].name);
              rowCreated = true;
            }
          }

          //Starting timer
          if (jumpTypes[currentExerciseType].timeLimit != 0)
          {
            //Hardcoded to show integers and update every second
            testTime.begin(updateJumpTime, 1000000);
            updateJumpTime();
          }
        }

        //Check jumps limit
        if (jumpTypes[currentExerciseType].jumpLimit > 0                //Jumps limit set
            && totalJumps >= jumpTypes[currentExerciseType].jumpLimit)  //Jumps equal or exceeded to limit
          capturing = false;

      } //End of rcaFlag
      //Check time limit
      if ( !waitingFirstPhase
           && !timeEnded                                                                    //Only check once
           && jumpTypes[currentExerciseType].timeLimit > 0                                      //time limit set
           && totalTestTime >= (unsigned int)jumpTypes[currentExerciseType].timeLimit * 1000000) //time limit exceeded
      {
        timeEnded = true;
        //Check if test must end. Hard time limit or soft time limit but sepping on the mat
        if ( jumpTypes[currentExerciseType].hardTimeLimit                         //Hard time limit
             || ( !jumpTypes[currentExerciseType].hardTimeLimit && rcaState ) )   //Soft time limit and in contact with the mat
        {
          capturing = false;
          rcaFlag = false;
        }
      }
      redButton.update();
      blueButton.update();
    }
    //The current test has ended
    Serial.println();
    dataFile.println();

    waitingFirstPhase = true;
    rcaFlag = false;
    totalJumps = 0;
    totalTestTime = 0;
    testTime.end();
    totalTime = 0;
    timeEnded = false;
    rowCreated = false;

    //check if the user wants to perform another one
    if ( yesNoDialog("Continue with " + jumpTypes[currentExerciseType].name + "?", 10, 10))
    {
      rowCreated = false;
      if (jumpTypes[currentExerciseType].timeLimit > 0) updateJumpTime();
    } else
      break;

    //If the user chooses yes continue and start over in the while(capturing)
    redButton.update();
    blueButton.update();
    capturing = true;
    //rca may have changed after finishing the test
    rcaFlag = false;
    lastRcaState = rcaState;
  }
  dataFile.close();
  showJumpsResults(maxJump, bestJumper, totalJumps);
  capturing = false;
  drawMenuBackground();
  redButton.update();
  blueButton.update();
  detachInterrupt(rcaPin);
  showMenuEntry(currentMenuIndex);
}

void showJumpsResults(float maxJump, unsigned int bestJumper, int totalJumps)
{
  resultsBackground();
  tft.drawLine(160, 240, 160, 80, BLACK);
  int textSize = 2;

  printTftText("J", 0, 40, WHITE, 2, false);
  printTftText("max", 12, 48, WHITE, 1);
  printTftValue(maxJump, 100, 40, textSize, 2);

  printTftText("N", 170, 40, WHITE, 2, false);
  printTftText("Jumps", 218, 40, WHITE, 1);
  printTftValue(totalJumps, 268, 40, textSize, 0);

  printTftText("Best Jumper: ", 0, 80, WHITE, 2, false);

  printTftText(persons[bestJumper].name + " " + persons[bestJumper].surname, 12, 100, WHITE, 2, false);

  redButton.update();
  while (!redButton.fell()) {
    redButton.update();
  }
  tft.fillRect(0, 20, 320, 240, BLACK);
  drawMenuBackground();
}

void setForceGoal()
{
  forceGoal = selectValueDialog("Select the force goal in Newtons.\nAn horizontal red line will be drawn", "10,50,1000,10000", "10,100,500", 0);
  Serial.println(forceGoal);
  menuItemsNum = systemMenuItems;
  showMenuEntry(currentMenuIndex);
}

void saveData(String fileName)
{
  if (sensor == loadCell)
    dataFile.println(String(lastSampleTime) + ";" + String(measured));
  else if(sensor == raceAnalyzer)
  {
    //Only write on interruption
    if (rcaFlag || encoderFlag)
    {
      encoderFlag = false;
      rcaFlag = false;
      if (!binaryFormat) Serial.println(String(raceAnalyzerSample.totalTime) + ";" + String(raceAnalyzerSample.displacement) + ";" + String(rcaState));
      else Serial.write((byte*)&raceAnalyzerSample, 9);
      if(!PcControlled)
        dataFile.println(String(raceAnalyzerSample.totalTime) + ";" + String(raceAnalyzerSample.displacement) + ";" + String(rcaState));
    }
  }
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
  
  File lastDir = SD.open(lastDirName);
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
  printTftText("Extend the rope or belt.\nOnce extended press RedButton", 12, 100);

  printTftText("Position: ", 12, 190);
  position = encoder.read();
  printTftText(position, 124, 190);
  redButton.update();
  while (!redButton.fell())
  {
    position = encoder.read();
    if (position != lastPosition) {
      printTftText(lastPosition, 124, 190, BLACK);
      printTftText(position, 124, 190);
      lastPosition = position;
    }
    redButton.update();
  }

  //Deleting text
  printTftText("Extend the rope or belt.\nOnce extended press RedButton", 12, 100, BLACK);
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

void selectPerson()
{
  //setNumber++;
  updatePersonSet();
  while (!redButton.fell())
  {
    blueButton.update();
    if (blueButton.fell()) {
      updatePersonSet();
    }
    redButton.update();
  }
}

//void saveSimpleJump(float lastPhaseTime)
//{
//  fullFileName = "/" + dirName + "/" + fileName + ".TXT";
//  dataFile = SD.open(fullFileName.c_str(), FILE_WRITE);
//  if ( !rcaState)
//  {
//    dataFile.print(String(currentPerson) + ";" + jumpTypes[currentExerciseType].id + ";" + String(lastPhaseTime, 6) );
//  }
//  else if (rcaState)
//  {
//    dataFile.println(";" + String(lastPhaseTime, 6));
//  }
//  dataFile.close();
//}

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
  SD.remove("INERMACH.TXT");
 
  File inertFile = SD.open("INERMACH.TXT", FILE_WRITE);

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
  Serial.println("Saved " + String(totalInertMachines) + " to INERMACH.TXT");
}

void readInertMachineFile()
{
  char readChar;
  String readString = "";
  unsigned long pos = 0;    //Position in the file
  int numRows = 0;          //Number of valid rows in the file

  File  machinesFile = SD.open("INERMACH.TXT");

  if (machinesFile)
  {
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
  }
  machinesFile.close();
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

void startRaceAnalyzerCapture()
{
  encoderFlag = false;
  rcaFlag = false;
  attachInterrupt(encoderAPin, encoderAChange, CHANGE);
  //attachInterrupt(encoderBPin, encoderBChange, CHANGE);
  attachInterrupt(rcaPin, changedRCA, CHANGE);
  calibratedInertial = false;
  totalTime = 0;
  sensor = raceAnalyzer;
  pps = 40;
  capturing = true;
  maxString = "V";
  plotPeriod = 5;
  newGraphMin = -1;
  newGraphMax = 10;
  measuredMax = 0;
  measured = 0;
  totalTime = 0;

  if(totalRaceAnalyzerTypes == 0) readExercisesFile(encoderRace);
  selectExerciseType(encoderRace);
  
  captureRaw();
  endRaceAnalyzerCapture();
}

void endRaceAnalyzerCapture()
{
  detachInterrupt(encoderAPin);
  //detachInterrupt(encoderBPin);
  detachInterrupt(rcaPin);
  redButton.update();
  blueButton.update();
  capturing = false;
  drawMenuBackground();
  showMenuEntry(currentMenuIndex);
}

void encoderAChange()
{
  position = encoder.read();
  if(abs(position) >= pps)
  {
    //Serial.println(raceAnalyzerSample.displacement);
    raceAnalyzerSample.displacement = position;
    currentSampleTime = totalTime;
    encoder.write(0);
    encoderFlag = true;
  }
}

void getRaceAnalyzerDynamics()
{
  if((encoderFlag || rcaFlag))
  {
    if(encoderFlag)
    {
      measured = abs( (float)raceAnalyzerSample.displacement * 1000000 * 0.0030321 /  4 / (float)(currentSampleTime - raceAnalyzerSample.totalTime) );
      raceAnalyzerSample.totalTime = currentSampleTime;
      raceAnalyzerSample.sensor = raceAnalyzer;
    }

    if(rcaFlag)
    {
      raceAnalyzerSample.sensor = rca;
    }
  }
}

/*
void encoderBChange()
{
  raceAnalyzerSample.displacement = encoder.read();
  currentSampleTime = totalTime;
  if(abs(raceAnalyzerSample.displacement) >= pps)
  {
    measured = raceAnalyzerSample.displacement * 1000 /  (currentSampleTime - raceAnalyzerSample.totalTime);
    raceAnalyzerSample.totalTime = currentSampleTime;
    encoder.write(0);
    encoderFlag = true;
  }
}
*/
