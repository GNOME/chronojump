#include <elapsedMillis.h>
#include <EEPROM.h>

// All the bluetooth code was inspired from https://wiki.seeedstudio.com/xiao_esp32s3_bluetooth/#ble-serverclient
// To connect in linux terminal: https://www.jaredwolff.com/get-started-with-bluetooth-low-energy/
//Bluetooth Low Energy libriaries
#include <BLEDevice.h>
#include <BLEUtils.h>
#include <BLEServer.h>
#include <BLE2902.h>

#define SERVICE1_UUID        "b0f3d089-3ce3-4ed6-9920-43afae288982"
#define SERVICE2_UUID        "7ddcf4e4-acad-4935-bc53-5e29e3a130bf"
#define LAST_EVENT_CHARACTERISTIC_UUID "19a8d494-a941-4ae3-99c0-28e9d352a170"
#define COMMAND_CHARACTERISTIC_UUID "1a2ae85a-8118-4644-9e3b-387122d8cd9e"
#define OUTPUT_CHARACTERISTIC_UUID "3bf2da15-f408-414f-a8dd-8cf1590b4a4a"

BLECharacteristic *plastEvent;      // The time of the change of the sensor state.
BLECharacteristic *pCommand;        // Commands to config the platform. "sync" turns off the beam in order to synchronize the clocks of two barriers
BLECharacteristic *pOutput;         // For writing the log in debug mode

bool deviceConnected = false;
bool commandFlag = false;

class MyServerCallbacks: public BLEServerCallbacks {
  void onConnect(BLEServer* pServer) {
    deviceConnected = true;
    BLEDevice::startAdvertising();
  };
  
  void onDisconnect(BLEServer* pServer) {
    deviceConnected = false;
  }
};

class MyCallbacks: public BLECharacteristicCallbacks {
    void onWrite(BLECharacteristic *pCharacteristic) {
      std::string value = pCharacteristic->getValue();
      String characteristicUUID = pCharacteristic->getUUID().toString().c_str();
      if (value.length() <= 0) {
        return;
      }

      if (characteristicUUID == COMMAND_CHARACTERISTIC_UUID) { commandFlag = true;}
    }
};

String version = "IRPlatform-0.4-XIAO";

#define INDICATOR_PIN    D9             //Led indicating the beam detection
#define RED_PIN     D8                  //Dual led for on/off indicator
#define GREEN_PIN     D7                //Dual led for battery indicator
#define BATT_LEV_PIN  13
elapsedMillis totalTime = 0;
elapsedMillis burstingTime = 0;

int lastEvent = 0;                      // Negative numbers means leaving the sensor (Beam not detected -> Beam detected).
                                        //Positive means arriving at the sensor (Beam detected -> Beam not detected).

//Emitter signal
uint32_t duty = 20;                     //Duty cycle in %

//---Start PWM stuff---
// use first channel of 16 channels (started from zero)
#define LEDC_CHANNEL_0     0
// use 10 bit precission for LEDC timer
#define LEDC_TIMER_10_BIT  10
//#define LEDC_TIMER_10_BIT  8
// use LEDC base frequency in Hz
#define LEDC_BASE_FREQ     38000
//#define LEDC_BASE_FREQ     56000
// fade LED PIN (replace with LED_BUILTIN constant for built-in LED)
#define EMITTER_PIN  3

bool pulsing = false;             //wether the signal carrier is continuous or bursting. Set to true just for debugging purposes
unsigned int pulsePeriod = 1;     //The period for a pulse cicle. Half ot the period is on and half is off
bool carrierActive = true;             //The signal carrier can periodically switch from active to inactive if pulsing is true
unsigned int inputNorPin = 1;      // 1 -> OR config (contact platform). 15 -> AND config (photocell)
unsigned int inputNandPin = 2;      // 2 -> OR config (contact platform). 15 -> AND config (photocell)
unsigned int inputPin = inputNorPin;
volatile bool currentSensorState = LOW; //HIGH -> Contact/Presence/Beam interrupted
volatile bool lastSensorState = LOW;    //HIGH -> Contact/Presence/Beam interrupted
volatile bool sensorChange = false;
unsigned int debounceTime = 2000;   //Time in microseconds to filter spurious signals
bool debug = true;


// Arduino like analogWrite
// value has to be between 0 and valueMax
void ledcAnalogWrite(uint8_t channel, unsigned int value, uint32_t valueMax = 255) {
  // calculate duty, 1023 from 2 ^ NumBits - 1
  uint32_t dutyTemp = ((pow(2, LEDC_TIMER_10_BIT) - 1) / valueMax) * min(value, valueMax);

  // write duty to LEDC
  ledcWrite(channel, dutyTemp);
}

// debounceTimerused for debouncing
hw_timer_t *debounceTimer= NULL;

//When the sensor changes its state, this is function called after debounceTime in microseconds
void IRAM_ATTR debounce() {
  currentSensorState = !digitalRead(inputPin);
  timerStop(debounceTimer);
  if (currentSensorState != lastSensorState) {
    sensorChange = true;
  }
}

void setup() {
  Serial.begin(115200);
  Serial.println(version);

  EEPROM.begin(512);

  
  //BLEDevice::init("Funciona");
  setBLEName();

  BLEServer *pServer = BLEDevice::createServer();
  pServer->setCallbacks(new MyServerCallbacks());
  BLEService *pService1 = pServer->createService(SERVICE1_UUID);

  plastEvent = pService1->createCharacteristic(
                                         LAST_EVENT_CHARACTERISTIC_UUID,
                                         BLECharacteristic::PROPERTY_READ
                                         | BLECharacteristic::PROPERTY_NOTIFY
                                       );

  BLEService *pService2 = pServer->createService(SERVICE2_UUID);

  pCommand = pService2->createCharacteristic(
                                         COMMAND_CHARACTERISTIC_UUID,
                                         BLECharacteristic::PROPERTY_READ
                                         | BLECharacteristic::PROPERTY_WRITE
                                       );

  pOutput = pService2->createCharacteristic(
                                         OUTPUT_CHARACTERISTIC_UUID,
                                         BLECharacteristic::PROPERTY_READ
                                         | BLECharacteristic::PROPERTY_NOTIFY
                                       );

  pCommand->setCallbacks(new MyCallbacks());                                     

  // Needed to read continuous data from. In nRF Connect the icon with multiple arrow must be with the cross 
  // pPlatformState->addDescriptor(new BLE2902());
  plastEvent->addDescriptor(new BLE2902());
  pOutput->addDescriptor(new BLE2902());

  pService1->start(); 
  pService2->start();
  plastEvent->setValue((uint8_t*)&lastEvent, 4);
  BLEAdvertising *pAdvertising = BLEDevice::getAdvertising();
  pAdvertising->addServiceUUID(SERVICE1_UUID);
  pAdvertising->setScanResponse(true);
  pAdvertising->setMinPreferred(0x06);  // functions that help with iPhone connections issue
  pAdvertising->setMinPreferred(0x12);
  BLEDevice::startAdvertising();

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

  currentSensorState = !digitalRead(inputPin);
  digitalWrite(RED_PIN, currentSensorState);
  digitalWrite(GREEN_PIN, !currentSensorState);
  digitalWrite(INDICATOR_PIN, currentSensorState);

  // if ( currentSensorState) {pPlatformState->setValue("Out");}
  // else if ( !currentSensorState) {pPlatformState->setValue("In");}
  // pPlatformState->notify();

  //Activating the timer for debouncing
  debounceTimer = timerBegin(0, 80, true); // debounceTimer0, clock divider 80
  timerAttachInterrupt(debounceTimer, &debounce, true); 
  timerAlarmWrite(debounceTimer, debounceTime, true);
  timerAlarmEnable(debounceTimer); // Habilitar la alarma
  timerStop(debounceTimer);
}

void loop() {

  if (pulsing) { pulse(); } // For debugging purpouses

  if (sensorChange) {
    digitalWrite(INDICATOR_PIN, currentSensorState);
    digitalWrite(RED_PIN, currentSensorState);
    digitalWrite(GREEN_PIN, !currentSensorState);

    if (currentSensorState == HIGH) {
      // Serial.println("In");
    } else if (currentSensorState == LOW) {
      lastEvent = - lastEvent;
      // Serial.println("Out");
    }
    printLog(String(lastEvent));

    if (deviceConnected) {
      plastEvent->setValue( (uint8_t*)&lastEvent, 4 );
      plastEvent->notify();
      // pPlatformState->notify();
    }

    lastSensorState = currentSensorState;
    sensorChange = false;
  }

  if (commandFlag) { //Some command has been sent
    processBLECommand();
    commandFlag = false;
  }

  //check if there's incoming data in the serial port
  if (Serial.available()) {
    processSerial();
  }
}

void changedPin() {
  lastEvent = totalTime;
  // To test the sync uncommend the next line. Step on one barrier and check if both barriers registers the same time
  //setIRPower(0);
  timerStart(debounceTimer);
}

//set the duty cicle of the IR emitter. Value between 0 and 100
void setIRPower(uint32_t power) {
  ledcAnalogWrite(LEDC_CHANNEL_0, power, 100);
  Serial.println("Power set to " + String(power) + "%");
}

void processSerial()
{
  String inputString = Serial.readString();
  processCommand(inputString);
}

void processBLECommand () {
  String inputString = pCommand->getValue().c_str();
  processCommand(inputString);

}

String get_command_argument(String inputString)
{
  return (inputString.substring(inputString.lastIndexOf(":") + 1, inputString.lastIndexOf(";")));
}

void getVersion()
{
  Serial.println(version);
  pOutput->setValue(version.c_str());
  pOutput->notify();
}

//In startSleepMode IR is activated only in long intervals to check if there is presence
void startSleepMode() {
  setIRPower(0);
  digitalWrite(INDICATOR_PIN, LOW);
//  timerAlarmEnable(debounceTimer);
}

void endSleepMode() {
//  timerAlarmDisable(debounceTimer);
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

void sync() {
  setIRPower(0);
  printLog( "Power set to 0" );
  totalTime=0;
  delay(30000);
  setIRPower(duty);
  printLog( "Power set to " + String(duty));
}

//Set the BLE Name with the parameter
//If no parameter is provided, it reads the EEPROM
//If the EEPROM is empty it creates a name with the form of "IR-Platform"+[random string]
void setBLEName( void ) { setBLEName(EEPROM.readString(0)); }  //By default reads the name stored in the EEPROM
void setBLEName(String BLEName) {

  // If it is the first time the code is run the Name is not set.
  // In that case a random name is created and stored in the EEPROM
  if (BLEName.length() == 0 ) {
    char letters[] = "abcdefghijklmnopqrstuvwxyz0123456789";
    BLEName = "IR-Platform_";
    for (int i = 1; i<=4; i++) {
      BLEName = BLEName + letters[random(0,35)];
    }
  }
  if (EEPROM.readString(0) != BLEName) {
    EEPROM.writeString(0, BLEName.c_str() );
    EEPROM.commit();
  }
  BLEDevice::init(BLEName.c_str());
  //printLog( "BLE Name set to: " + BLEName );
}

//pulse activates and deactivates the carrier signal alternatively each second;
void pulse()
{
  if (burstingTime >= pulsePeriod*500) {
    lastEvent = totalTime;
    carrierActive = !carrierActive;

    if (carrierActive) {
      //setIRPower(0);
      digitalWrite(INDICATOR_PIN, HIGH);
    } else if (!carrierActive) {
      setIRPower(duty);
      digitalWrite(INDICATOR_PIN, LOW);
    }
    burstingTime = 0;
    plastEvent->setValue( (uint8_t*)&lastEvent, 4 );
    plastEvent->notify();
  }
}

void printLog(String output) {
  if (!debug) {return;}
  Serial.println(output);
  pOutput->setValue(output.c_str());
  pOutput->notify();
}

void processCommand(String inputString) {
  String commandString = inputString.substring(0, inputString.lastIndexOf(":"));
  String argumentString = get_command_argument(inputString);
  if (commandString == "setIRPower") {
    duty = argumentString.toInt();
    setIRPower( duty );
  } else if (commandString == "get_version") {
    getVersion();
  } else if (commandString == "setOrMode") {
    setInputPin(inputNorPin);
  } else if (commandString == "setAndMode") {
    setInputPin(inputNandPin);
  } else if (commandString == "sleep") {
    startSleepMode();
  } else if (commandString == "wakeup") {
    endSleepMode();
  } else if (commandString == "getBattLev") {
    Serial.println(getBatteryLevel(100));
  } else if (commandString == "setBLEName") {
    setBLEName();
    ESP.restart();
  } else if (commandString == "sync") {
    sync();
  } else if (commandString == "setPulsePeriod") {
    pulsePeriod = get_command_argument(inputString).toInt();
  } else if (commandString == "startPulsing") {
    pulsing = true;
  } else if (commandString == "endPulsing") {
    pulsing = false;
  } else if (commandString == "restart") {
    ESP.restart();
  } else if (commandString == "delay") {
    totalTime = totalTime - argumentString.toInt();
    printLog("totalTime: " + String(totalTime));
  } else {
    printLog("Not a valid BLE command");
    for (int i = 0; i<10; i++) {
      digitalWrite(RED_PIN, HIGH);
      delay(100);
      digitalWrite(RED_PIN, LOW);
      delay(100);
    }
    digitalWrite(RED_PIN, currentSensorState);
  }
  inputString = "";
}