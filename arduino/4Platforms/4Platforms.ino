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


//1 Front Left
//2 Front Right
//3 Back Left
//4 Back Right

String version = "4Platforms-0.1";

int sensorPin[4] = {1, 2, 9, 8}; //Front Left, Front Right, Back Left, Back Right

volatile bool sensorState[4] = {LOW, LOW, LOW, LOW};
volatile bool lastSensorState[4] = {LOW, LOW, LOW, LOW};


volatile bool sensorChange[4] = {false, false, false, false};     //Flag for indicating a true change (debounced) in the sensor
unsigned int debounceTime = 10000;   //Time in microseconds to filter spurious signals

int totalSensors = 3;

elapsedMillis phaseTime[4] = {0, 0, 0, 0};
int lastPhaseDuration[4] = {0, 0, 0, 0};                      // Negative numbers means leaving the sensor (Beam not detected -> Beam detected).
                                                              //Positive means arriving at the sensor (Beam detected -> Beam not detected).

// debounceTimer used for debouncing
// hw_timer_t *FLTimer= NULL;
// hw_timer_t *FRTimer= NULL;
// hw_timer_t *BLTimer= NULL;
// hw_timer_t *BRTimer= NULL;
hw_timer_t *debounceTimer[4]= {NULL, NULL, NULL, NULL};

//debounceTime microseconds after the sensor changes its state, this is the functions called
void IRAM_ATTR FLdebounce() {
  debounce(0);
}

void IRAM_ATTR FRdebounce() {
  debounce(1);
}

void IRAM_ATTR BLdebounce() {
  debounce(2);
}

void IRAM_ATTR BRdebounce() {
  debounce(3);
}

void debounce(int i)
{
  sensorState[i] = !digitalRead(sensorPin[i]);
  timerStop(debounceTimer[i]);
  if (sensorState[i] != lastSensorState[i]) {
    lastPhaseDuration[i] = phaseTime[i];
    phaseTime[i] = 0;
    sensorChange[i] = true;
  }
}

void setup() {
  Serial.begin(115200);

  pinMode(LED_BUILTIN, OUTPUT);
  digitalWrite(LED_BUILTIN, HIGH);
  delay(100);
  Serial.println(version);

  for(int i=0; i<=3; i++)
  {
    pinMode(sensorPin[i], INPUT_PULLDOWN);
    // pinMode(sensorPin[i], INPUT_PULLUP);

  }

  //When the input pin changes the function changedPin() is called
  attachInterrupt(digitalPinToInterrupt(sensorPin[0]), changedFL, CHANGE);
  attachInterrupt(digitalPinToInterrupt(sensorPin[1]), changedFR, CHANGE);
  attachInterrupt(digitalPinToInterrupt(sensorPin[2]), changedBL, CHANGE);
  attachInterrupt(digitalPinToInterrupt(sensorPin[3]), changedBR, CHANGE);

  // Just to sabilize the inputPin
  delay(100);
  for(int i=0; i<=3; i++)
  {
    sensorState[i] = !digitalRead(sensorPin[i]);
    lastSensorState[i] = sensorState[i];
  }

  digitalWrite(LED_BUILTIN, lastSensorState[1]);

  //Configuring the timer for debouncing
  configDebounceTimers(debounceTime);

  Serial.flush();
}

void loop() {

  //Act only when the signal is stable
  for (int i=0; i<=3; i++)
  {
    if (sensorChange[i]) {
      Serial.print(i);
      Serial.print(":");
      digitalWrite(LED_BUILTIN, sensorState[i]);

      //save the stable state as the true one as it has survived the debounce process
      lastSensorState[i] = sensorState[i];

      //Change the sign of the time value to know in which sense the change has accurred
      if (sensorState[i] == LOW) {
        lastPhaseDuration[i] = - lastPhaseDuration[i];
      }
      sensorChange[i] = false;
      Serial.println(lastPhaseDuration[i]);
    }
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
void changedFL() {
  changedSensor(0);
}

void changedFR() {
  changedSensor(1);
}

void changedBL() {
  changedSensor(2);
}

void changedBR() {
  changedSensor(3);
}

//Start the debouncing process
void changedSensor(int i) {
  if( !digitalRead(sensorPin[i]) != lastSensorState[i]) {
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
    debounceTimer[i] = timerBegin(i, 80, true); // Timer0, clock divider 80
  }

  setDebounceTime(value);

  timerAttachInterrupt(debounceTimer[0], &FLdebounce, true);
  timerAttachInterrupt(debounceTimer[1], &FRdebounce, true);
  timerAttachInterrupt(debounceTimer[2], &BLdebounce, true);
  timerAttachInterrupt(debounceTimer[3], &BRdebounce, true);

}

void setDebounceTime(unsigned int value) {

  for(int i= 0; i<=3; i++)
  {
    timerAlarmWrite(debounceTimer[i], value, true);
    timerAlarmEnable(debounceTimer[i]); // Habilitar la alarma
    timerStop(debounceTimer[i]);
  }
  Serial.print("Debounce set to: ");
  Serial.println(value);
}
