/*
  Controler for 4 contact devices

  Copyright (C) 2024 Xavier Padullés testing@chronojump.org

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

#include <elapsedMillis.h>
#include <Adafruit_NeoPixel.h>


//1 Front Left
//2 Front Right
//3 Back Left
//4 Back Right
#define LEDS_PIN        5
#define NUMPIXELS 4
#define CJ_YELLOW rgbLeds.Color(45,25,0)
#define CJ_BLUE rgbLeds.Color(0,0,50)

#define CHARGE_ON digitalWrite(4, LOW);
#define CHARGE_OFF digitalWrite(4, HIGH);

#define BATT_LEV_PIN 14

Adafruit_NeoPixel rgbLeds(NUMPIXELS, LEDS_PIN, NEO_GRB + NEO_KHZ800);

String version = "Chronopic-4.0";
// Front Left, Front Right, Back Left, Back Right 
// int sensorPin[4] = {2, 1, 8, 9}; //Xiao ESP32S3. Hardware V1
//int sensorPin[4] = {D0, D1, D10, D9}; //Xiao ESP32S3. Hardware V2
int sensorPin[4] = {1, 2, 9, 8}; //ESP32S3. Hardware V4

// Changing the order of the pins/RGB LEDs
int sensorMapping[4] = {1, 0, 3, 2}; // Hardware V2

volatile bool sensorState[4] = {LOW, LOW, LOW, LOW};
volatile bool lastSensorState[4] = {LOW, LOW, LOW, LOW};


volatile bool sensorChange[4] = {false, false, false, false};     //Flag for indicating a true change (debounced) in the sensor
unsigned int debounceTime = 10000;   //Time in microseconds to filter spurious signals

int totalSensors = 3;

elapsedMicros phaseTime[4] = {0, 0, 0, 0};
long lastPhaseDuration[4] = {0, 0, 0, 0};                      // Negative numbers means leaving the sensor (Beam not detected -> Beam detected).
                                                              //Positive means arriving at the sensor (Beam detected -> Beam not detected).

// debounceTimer used for debouncing
// hw_timer_t *FLTimer= NULL;
// hw_timer_t *FRTimer= NULL;
// hw_timer_t *BLTimer= NULL;
// hw_timer_t *BRTimer= NULL;
hw_timer_t *debounceTimer[4]= {NULL, NULL, NULL, NULL};

int battLev = 0;
elapsedMillis batteryCycle = 0;

//debounceTime microseconds after the sensor changes its state, this is the functions called
void IRAM_ATTR debounce0() {
  debounce(0);
}

void IRAM_ATTR debounce1() {
  debounce(1);
}

void IRAM_ATTR debounce2() {
  debounce(2);
}

void IRAM_ATTR debounce3() {
  debounce(3);
}

void debounce(int i)
{
  sensorState[i] = !digitalRead(sensorPin[ sensorMapping[i] ]);
  timerStop(debounceTimer[i]);
  if (sensorState[i] != lastSensorState[i]) {
    lastPhaseDuration[i] = phaseTime[i];
    phaseTime[i] = 0; //Comment for absolute time. Uncomnmnent for relative time (time of phase)
    sensorChange[i] = true;
  }
}

void setup() {
  Serial.begin(115200);
  rgbLeds.begin(); // INITIALIZE NeoPixel strip object (REQUIRED)

  // pinMode(LED_BUILTIN, OUTPUT);
  // digitalWrite(LED_BUILTIN, HIGH);

  for(int i=0; i<=3; i++)
  {
    pinMode(sensorPin[ sensorMapping[i] ], INPUT_PULLDOWN);
    rgbLeds.setPixelColor(i, CJ_BLUE);
  }

  pinMode(4, OUTPUT);

  //When the input pin changes the function changedPin() is called
  attachInterrupt(digitalPinToInterrupt(sensorPin[ sensorMapping[0] ]), changed0, CHANGE);
  attachInterrupt(digitalPinToInterrupt(sensorPin[ sensorMapping[1] ]), changed1, CHANGE);
  attachInterrupt(digitalPinToInterrupt(sensorPin[ sensorMapping[2] ]), changed2, CHANGE);
  attachInterrupt(digitalPinToInterrupt(sensorPin[ sensorMapping[3] ]), changed3, CHANGE);

  // Just to sabilize the inputPin
  delay(1000);
  Serial.println(version);

  // Initial state of sensors
  for(int i=0; i<=3; i++)
  {
    sensorState[i] = !digitalRead(sensorPin[ sensorMapping[i] ]);
    lastSensorState[i] = sensorState[i];
    if (sensorState[i] == HIGH) {
      lastPhaseDuration[i] = - lastPhaseDuration[i];
      rgbLeds.setPixelColor(sensorMapping[i], CJ_BLUE);
    } else {
      rgbLeds.setPixelColor(sensorMapping[i], CJ_YELLOW);
    }
    // Serial.println("Sensor" + String(i) + "->GPIO" + String(sensorPin[ sensorMapping[i] ] ) ); //For debugging pin mapping
  }
  rgbLeds.show();

  // digitalWrite(LED_BUILTIN, lastSensorState[1]);

  //Configuring the timer for debouncing
  configDebounceTimers(debounceTime);
  initializeBLE();
  Serial.flush();
  updateBatteryLevel();
}

void loop() {

  //Act only when the signal is stable
  for (int i=0; i<=3; i++)
  {
    if (sensorChange[i]) {
      Serial.print(i);
      Serial.print(":");
      // digitalWrite(LED_BUILTIN, sensorState[i]);

      //save the stable state as the true one as it has survived the debounce process
      lastSensorState[i] = sensorState[i];

      //Change the sign of the time value to know in which sense the change has accurred
      if (sensorState[i] == HIGH) {
        lastPhaseDuration[i] = - lastPhaseDuration[i];
        rgbLeds.setPixelColor(sensorMapping[i], CJ_BLUE);
      } else {
        rgbLeds.setPixelColor(sensorMapping[i], CJ_YELLOW);
      }
      rgbLeds.show();
      sensorChange[i] = false;
      Serial.println(lastPhaseDuration[i]);
      sendToBLE(i, lastPhaseDuration[i]);
    }
  }

  if (batteryCycle >= 60000) {
    updateBatteryLevel();
    batteryCycle = 0;
  }
  
  //check if there's incoming data in the serial port
  if (Serial.available()) {
    processSerial();
  }
}

void processSerial()
{
  String inputString = Serial.readString();
  processCommand(inputString);
}

void processCommand(String inputString) {
  String commandString = inputString.substring(0, inputString.lastIndexOf(":"));
  String argumentString = get_command_argument(inputString);
  if (commandString == "get_version") {
    getVersion();
  } else if (commandString == "start_capture") {
    startCapture();
  } else if (commandString == "end_capture") {
    endCapture();
  } else if (commandString == "set_debounce") {
    setDebounceTime(argumentString.toInt());
  } else if (commandString == "set_rgb") {
    setRgb(argumentString);
  } else if (commandString == "get_battery_level") {
    Serial.printf("Battery: %i\n", battLev);
  } else if (commandString == "disable_charge") {
    CHARGE_OFF;
  } else if (commandString == "enable_charge") {
    CHARGE_ON;
  } else {
    Serial.println("Not a valid command");
  }
  inputString = "";
}

String get_command_argument(String inputString)
{
  return (inputString.substring(inputString.lastIndexOf(":") + 1, inputString.lastIndexOf(";")));
}

void getVersion()
{
  Serial.println(version);
}

// functions called when a sensor change
void changed0() {
  changedSensor(0);
}

void changed1() {
  changedSensor(1);
}

void changed2() {
  changedSensor(2);
}

void changed3() {
  changedSensor(3);
}

//Start the debouncing process
void changedSensor(int i) {
  if( !digitalRead(sensorPin[ sensorMapping[i] ]) != lastSensorState[i]) {
    timerStop(debounceTimer[i]);
    timerStart(debounceTimer[i]);
    timerWrite(debounceTimer[i],0);
  }
}

void startCapture() {
  Serial.flush();
  Serial.print("Starting capture;Status:");
  for (int i = 0; i<=3; i++) {
    Serial.print(lastSensorState[i]);
    Serial.print(";");
    phaseTime[i] = 0;
    lastPhaseDuration[i] = 0;
  }
  Serial.println();
}

void endCapture() {
  Serial.print("Capture ended");
}

void configDebounceTimers() { configDebounceTimers(debounceTime); }
void configDebounceTimers(unsigned int value) {
  for(int i= 0; i<=3; i++)
  {
    debounceTimer[i] = timerBegin(1000000); // Timer0, clock divider 80
  }

  setDebounceTime(value);

  timerAttachInterrupt(debounceTimer[0], &debounce0);
  timerAttachInterrupt(debounceTimer[1], &debounce1);
  timerAttachInterrupt(debounceTimer[2], &debounce2);
  timerAttachInterrupt(debounceTimer[3], &debounce3);

}

void setDebounceTime(unsigned int value) {

  for(int i= 0; i<=3; i++)
  {
    timerAlarm(debounceTimer[i], value, true,0);
    timerStop(debounceTimer[i]);
  }
  Serial.printf("Debounce set to: %u us\n", value);
}

void setRgb(String argumentString) {
  int i = argumentString.substring(0, argumentString.indexOf(",")).toInt();
  String colours = argumentString.substring(argumentString.indexOf(",") + 1, argumentString.indexOf(";"));
  int red = colours.substring(0, colours.indexOf(",")).toInt();
  colours = colours.substring(colours.indexOf(",") + 1, colours.indexOf(";"));
  int green = colours.substring(0, colours.indexOf(",")).toInt();
  colours = colours.substring(colours.indexOf(",") + 1, colours.indexOf(";"));
  int blue = colours.substring(0, colours.indexOf(";")).toInt();
  rgbLeds.setPixelColor(i, rgbLeds.Color(red, green, blue));
}

void updateBatteryLevel() {
  CHARGE_OFF;
  for (int i = 0; i<10; i++) {
    battLev = battLev + analogRead(BATT_LEV_PIN);
  }
  CHARGE_ON;
  battLev = battLev / 10;
  Serial.print(battLev);
  battLev = map(battLev, 2000, 2400, 0, 100);
  if (battLev > 100) 
    battLev = 100;
  if (battLev <0 )
    battLev = 0;
  updateBatteryCharacteristic(battLev);
  Serial.printf(";%i%%\n", battLev);
}