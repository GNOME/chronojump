#include <elapsedMillis.h>
//#include <TimerOne.h>

String version = "IRPlatform-0.4";

#define INDICATOR_PIN    D9             //Led indicating the beam detection
#define RED_PIN     D8                  //Dual led for on/off indicator
#define GREEN_PIN     D7                 //Dual led for battery indicator
#define BATT_LEV_PIN  13
elapsedMillis totalTime = 0;

//Emitter signal
uint32_t duty = 10;                //Duty cycle in %

//---Start PWM stuff---
// use first channel of 16 channels (started from zero)
#define LEDC_CHANNEL_0     0
// use 10 bit precission for LEDC timer
#define LEDC_TIMER_10_BIT  10
// use LEDC base frequency in Hz
#define LEDC_BASE_FREQ     38000
// fade LED PIN (replace with LED_BUILTIN constant for built-in LED)
#define EMITTER_PIN  3

bool bursting = false;          //wether the signal carrier is continuous or bursting. Set to true just for debugging purposes
bool active = true;             //The carrier signal can periodically switch from active to inactive if bursting is true
unsigned int inputNorPin = 1;      // 1 -> OR config (contact platform). 15 -> AND config (photocell)
unsigned int inputNandPin = 2;      // 2 -> OR config (contact platform). 15 -> AND config (photocell)
unsigned int inputPin = inputNorPin;
volatile bool currentSensorState = LOW; //HIGH -> Contact/Presence/Beam interrupted
volatile bool lastSensorState = LOW;    //HIGH -> Contact/Presence/Beam interrupted
volatile bool sensorChange = false;
unsigned int debounceTime = 2000;   //Time in microseconds to filter spurious signals

//Timer
// hw_timer_t * timer = NULL;
// portMUX_TYPE timerMux = portMUX_INITIALIZER_UNLOCKED;
// volatile bool timerFlag = false;

// Arduino like analogWrite
// value has to be between 0 and valueMax
void ledcAnalogWrite(uint8_t channel, unsigned int value, uint32_t valueMax = 255) {
  // calculate duty, 1023 from 2 ^ NumBits - 1
  uint32_t dutyTemp = ((pow(2, LEDC_TIMER_10_BIT) - 1) / valueMax) * min(value, valueMax);

  // write duty to LEDC
  ledcWrite(channel, dutyTemp);
}

hw_timer_t *timer = NULL;

//When the sensor changes its state, this is function called after debounceTime in microseconds
void IRAM_ATTR debounce() {
  currentSensorState = !digitalRead(inputPin);
  timerStop(timer);
  if (currentSensorState != lastSensorState) {
    sensorChange = true;
  }
}

void setup() {
  Serial.begin(115200);
  Serial.println(version);


  //---Start PWM---
  // Setup timer and attach timer to a led pin
  ledcSetup(LEDC_CHANNEL_0, LEDC_BASE_FREQ, LEDC_TIMER_10_BIT);
  ledcAttachPin(EMITTER_PIN, LEDC_CHANNEL_0);
  ledcAnalogWrite(LEDC_CHANNEL_0, duty, 100);
  //---End PWM stuff---


  //Sensor config
  pinMode(RED_PIN, OUTPUT);
  pinMode(GREEN_PIN, OUTPUT);
  pinMode(INDICATOR_PIN, OUTPUT);
  pinMode(inputNorPin, INPUT_PULLUP);
  pinMode(inputNandPin, INPUT_PULLUP);
  pinMode(BATT_LEV_PIN, INPUT_PULLUP);

  attachInterrupt(digitalPinToInterrupt(inputPin), changedPin, CHANGE);

  for (int i = 0 ; i<5; i++ ){
    digitalWrite(RED_PIN, LOW);
    delay(25);
    digitalWrite(GREEN_PIN, LOW);
    delay(25);
    digitalWrite(RED_PIN, HIGH);
    delay(25);
    digitalWrite(GREEN_PIN, HIGH);
    delay(25);
  }
  delay(10);

  digitalWrite(RED_PIN, !digitalRead(inputPin));
  digitalWrite(GREEN_PIN, digitalRead(inputPin));
  digitalWrite(INDICATOR_PIN, !digitalRead(inputPin));

  //Activating the timer for debouncing
  timer = timerBegin(0, 80, true);
  timerAttachInterrupt(timer, &debounce, true);
  timerAlarmWrite(timer, debounceTime, true);
  timerAlarmEnable(timer); // Habilitar la alarma
  timerStop(timer);


//startSleepMode();
}

void loop() {

  //bursting activates and deactivates the carrier signal alternatively each second;
  if (bursting) {
    if (totalTime >= 1000) {
      active = !active;

      if (active) {
        ledcSetup(LEDC_CHANNEL_0, LEDC_BASE_FREQ, LEDC_TIMER_10_BIT);
        ledcAttachPin(EMITTER_PIN, LEDC_CHANNEL_0);
        ledcAnalogWrite(LEDC_CHANNEL_0, duty, 100);
        digitalWrite(INDICATOR_PIN, HIGH);
      } else if (!active) {
        //        Timer1.disablePwm(emitterPin);
        //        digitalWrite(emitterPin, LOW);
        ledcSetup(LEDC_CHANNEL_0, 0, LEDC_TIMER_10_BIT);
        ledcAttachPin(EMITTER_PIN, LEDC_CHANNEL_0);
        digitalWrite(INDICATOR_PIN, LOW);
      }
      totalTime = 0;
    }
  }

  //check if there's incoming data in the serial port
  if (Serial.available()) {
    processSerial();
  }
  if (sensorChange) {
    digitalWrite(INDICATOR_PIN, currentSensorState);
    digitalWrite(RED_PIN, currentSensorState);
    digitalWrite(GREEN_PIN, !currentSensorState);
    lastSensorState = currentSensorState;
    sensorChange = false;
  }
}

void changedPin() {
  timerStart(timer);
}

//set the duty cicle of the IR emitter. Value between 0 and 100
void setIRPower(uint32_t power) {
  ledcAnalogWrite(LEDC_CHANNEL_0, power, 100);
  //Serial.println("Power set to " + String(power) + "%");
}

void processSerial()
{
  String inputString = Serial.readString();
  String commandString = inputString.substring(0, inputString.lastIndexOf(":"));

  if (commandString == "setIRPower") {
    String powerString = get_command_argument(inputString);
    duty = powerString.toInt();
    setIRPower( duty );
  } else if (commandString == "get_version") {
    getVersion();
  } else if (commandString == "setOrMode") {
    setInputPin(inputNorPin);
  } else if (commandString == "setAndMode") {
    setInputPin(inputNandPin);
  } else if (commandString == "startSleepMode") {
    startSleepMode();
  } else if (commandString == "endSleepMode") {
    endSleepMode();
  } else if (commandString == "getBattLev") {
    Serial.println(getBatteryLevel(100));
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

//In startSleepMode IR is activated only in long intervals to check if there is presence
void startSleepMode() {
  setIRPower(0);
//  timerAlarmEnable(timer);
}

void endSleepMode() {
//  timerAlarmDisable(timer);
  setIRPower(duty);
}

int getBatteryLevel(void) { getBatteryLevel(100); }
int getBatteryLevel(int measures) {
  unsigned int sum = 0;
  for (int i=1; i<=measures; i++) {
    sum = sum + analogRead(BATT_LEV_PIN);
    Serial.print(i);
    Serial.print("\t");
    Serial.println(sum);
  }
  return sum/measures;
}

void setInputPin(unsigned int pin) {
  detachInterrupt(digitalPinToInterrupt(inputPin));
  inputPin = pin;
  attachInterrupt(digitalPinToInterrupt(inputPin), changedPin, CHANGE);
  Serial.print("Pin set to: ");
  Serial.println(inputPin);
}
