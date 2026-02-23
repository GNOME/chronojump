/*
  terminal for Wichro fotocels using the NRf24L01 wireless module with the inverted input.
  Useful for impact detection with tilt/shock sensors

  Copyright (C) 2018 Xavier de Blas xaviblas@gmail.com
  Copyright (C) 2018 Xavier Padullés support@chronojump.org

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

#include <SPI.h>
//#include <nRF24L01.h>     //TODO: Check that it is necessary
#include <RF24.h>
// #include <printf.h>
#include <MsTimer2.h>
#include <TimerOne.h>
#include "pinout.h"
//#include  <util/parity.h>

unsigned int deviceType = 1; //Photocel and LightChro sensor
unsigned int deviceVersion = 26;

// Set up nRF24L01 radio on SPI bus plus pins  (CE & CS)

//RF24 radio(A3, A4);    //Old versions
RF24 radio(RADIO_CE, RADIO_CS);       //New version
#define red_on digitalWrite(RED_PIN,LOW)
#define green_on digitalWrite(GREEN_PIN,LOW)
#define blue_on digitalWrite(BLUE_PIN,LOW)
#define buzzer_on digitalWrite(BUZZER_PIN,HIGH)
#define red_off digitalWrite(RED_PIN,HIGH)
#define green_off digitalWrite(GREEN_PIN,HIGH)
#define blue_off digitalWrite(BLUE_PIN,HIGH)
#define buzzer_off digitalWrite(BUZZER_PIN,LOW)

struct instruction_t
{
  uint16_t command;       //The command to be executed
  short int termNum;  //The terminal number that have to execute the command
};

// binary commands: each bit represents DEACTIVATE, SENSORONCE, RED, GREEN, BLUE, BUZZER, INTERMITTENCY...
// 1 means ON
// 0 means OFF

const uint16_t batteryLevel  = 0b10000000000; //1024
const uint16_t ping           = 0b1000000000; //512
const uint16_t sensorUnlimited = 0b100000000; //256
//const uint16_t empty1 =         0b10000000; //128
//const uint16_t empty2 =         0b01000000; //64
const uint16_t setIntermittency = 0b00100000; //32
const uint16_t buzzer =           0b00010000; //16
const uint16_t blue =             0b00001000; //8
const uint16_t green =            0b00000100; //4
const uint16_t red =              0b00000010; //2
const uint16_t sensorOnce =       0b00000001; //1
const uint16_t deactivate =       0b00000000; //0

struct instruction_t instruction = {.command = deactivate, .termNum = 0};
int size_instruction = sizeof(instruction);

struct sample_t
{
  bool state;           //State of the sensor
  short int termNum;    //Terminal number. Configured with the switches.
  unsigned long int data;
};

struct sample_t sample = {.state = LOW, .termNum = 0, .data = 0};

int sample_size = sizeof(sample);

unsigned long time0;  //Time when the command is received

bool flagint = LOW;   //Interruption flag. Activated when the sensos changes
volatile bool lastPinState = LOW;  //stores the state of the pin 2 before the interruption
unsigned long debounceTime = 10000;
//The timer overflows inmediately after initialization.

// First channel to be used. The 6xswitches control the terminal number and the number to add the terminal0Channel
// The channel 125 is used to listen from the terminals.
// Channels 116 - 64 (descending) are used to send to the terminals
uint8_t terminal0Channel = 116; //TODO: Select the listening channel with the switches

//Channel of the controler
uint8_t control0Channel = 125; //Channel resulting of the switch at zero state
uint8_t controlSwitch = 0;      //State of the 3xswithes

bool waitingSensor = true; //Wether the sensor is activated or not
bool unlimitedMode = true; // sensorOnce deactivate the unlimited mode

// Used to know if each output (LEDs and Buzzer) is active
bool redIsActive = false;
bool greenIsActive = false;
bool blueIsActive = false;
bool activeBuzzer = false;

// Used to alternate on/off continuously the active outputs
bool  intermittency = false;

int blinkPeriod = 75; //Time between two consecutives rising flank of the LED

//bool debug = false;

void setup(void)
{

  //radio.setPALevel (RF24_PA_HIGH); //funcion  que solo se llama una vez si se quiere cambiar de potencia
  //pot= getPALevel( ); //función que retorna potencia programada

  Serial.begin(115200);
  // printf_begin(); //Used by radio.printDetails()

  Serial.print("Wifi-Sensor-");
  Serial.println(deviceVersion);

  pinMode(BATTERY_PIN, INPUT);
  radio.begin();
  radio.setRetries(1, 15);

  //Reading each pin to stablish the terminal number and the listening channel

bool waitingSensor = true; //Wether the sensor is activated or not
bool unlimitedMode = true; // sensorOnce deactivate the unlimited mode

  //¡¡¡¡Atention!!!!, the first version of lightChro the pin7 is associated to the buzzer.
  //The first versions of the Quick the microswitch was 5xSwitches. The last one associated to the buzzer
  //In the first versions of the photocells the microswitch was 8xSwitches
  //Remember to change comment/uncomment depending on the hardware version
  int idPin[] = {ID1_PIN, ID2_PIN, ID3_PIN, ID4_PIN, ID5_PIN, ID6_PIN};

  //for (int pin = 6; pin >= 3; pin--)  //Old versions
  for (int i = 5; i >= 0; i--)    //New versions
  {
    pinMode(idPin[i], INPUT_PULLUP);
    sample.termNum = sample.termNum * 2; //Each bit will be multiplied by 2 as much times as his significance
    if (!digitalRead(idPin[i])) sample.termNum++;
  }


  Serial.print("termNum: ");
  Serial.println(sample.termNum);

  //maximum 125 channels. cell phone and wifi uses 2402-2472. Free from channel 73 to channel 125. Each channels is 1Mhz separated
  radio.setChannel(terminal0Channel - sample.termNum);
  // Serial.print("Terminal Channel: " + String(terminal0Channel) + " - " + String(sample.termNum) + " = ");
  // Serial.println(terminal0Channel - sample.termNum);

  //  printf(" Status Radio\n\r");
  //  radio.printDetails();

  //  Serial.print("Channel set to: ");
  //  Serial.println(radio.getChannel());

  Serial.flush(); //Flushing the buffer serial buffer to avoid spurious data.

  // channel of the controler. 3xmicroswith controls this channel
  //************************************************************************************
  // A0, A1, A2 connected to the 3xswith

  //pinMode(A0, INPUT_PULLUP);  //Old versions
  pinMode(CH1_PIN, INPUT_PULLUP);    //New version
  pinMode(CH2_PIN, INPUT_PULLUP);
  pinMode(CH3_PIN, INPUT_PULLUP);

  if (analogRead(CH1_PIN)<128) {    // A7 cannot be read as digital
      controlSwitch = 1; //
  }
  if (analogRead(CH2_PIN)<128) {
    controlSwitch = controlSwitch + 2;
  }
  if (analogRead(CH3_PIN)<128) {
    controlSwitch = controlSwitch + 4;
  }

  // Fixed pipes
  uint64_t pipes[2] = { 0xF0F0F0F0E1LL, 0xF0F0F0F0D2LL }; //Two radio pipes.

  // Pipes depending on the controlSwitch. This is useful to isolate controler+terminals in the same controler channel
  // from other controler+terminals in other channels.
  pipes[0] += controlSwitch;
  pipes[1] += controlSwitch;
  radio.openWritingPipe(pipes[1]);     // Terminal -> Controler Pipe
  radio.openReadingPipe(1, pipes[0]);  // Controler -> Terminal Pipe

  radio.startListening();

  Serial.println("Channel: " + String(controlSwitch));
  // Serial.print("ControlChannel: " + String(control0Channel) + " - " + String(controlSwitch) + " = ");
  // Serial.println(control0Channel - controlSwitch);

  //Activate interruption service each time the sensor changes state
  attachInterrupt(digitalPinToInterrupt(2), controlint, CHANGE);

  pinMode(BLUE_PIN, OUTPUT);    //Blue
  pinMode(GREEN_PIN, OUTPUT);    //Green
  pinMode(RED_PIN, OUTPUT);    //Red
  pinMode(BUZZER_PIN, OUTPUT);     //Buzzer
  buzzer_on;
  delay(100);
  buzzer_off;
  pinMode(SENSOR_PIN, INPUT_PULLUP); //Sensor

  red_off;
  green_off;
  blue_off;


  //  noInterrupts();   //Don't watch the sensor state

  //  Serial.print("Is chip connected:");
  //  Serial.println(radio.isChipConnected());

  Timer1.initialize(debounceTime);
  Serial.print("debounceTime: ");
  Serial.println(debounceTime);
  Timer1.attachInterrupt(debounce);
  Timer1.stop();
  sample.state = digitalRead(SENSOR_PIN);
  lastPinState = sample.state;
  Serial.print("Power: ");
  radio.setPALevel(RF24_PA_MAX); // RF24_PA_MIN(0), RF24_PA_LOW(1), RF24_PA_HIGH(2), RF24_PA_MAX(3)
  Serial.println(radio.getPALevel());
  flagint = LOW;
  Serial.print("Speed:");
  // radio.setDataRate(RF24_250KBPS); // 0=RF24_250KBPS 1=RF24_1MBPS 2=250Kbps
  Serial.println(radio.getDataRate());
}


void loop(void)
{
  //if (flagint == HIGH && lastPinState != sample.state) //The sensor has changed
  if (flagint == HIGH) //The sensor has changed
  {
    sendSample();
    // Serial.println(sample.state);
  }

  while (radio.available())
  {
    radio.read(  &instruction, size_instruction );
    radio.stopListening();
    // delay(100);
    // Serial.print("Command received: ");
    // Serial.println(instruction.command);
    // radio.flush_rx();

    if (instruction.termNum != sample.termNum) {   
      radio.startListening();
      break;
    }

    /*
    //Confirming the reception
    if ( parityError(instruction.command) && debug) {
      beep(100);
      Serial.println(instruction.command, BIN);
    }
    */
    
    executeCommand(instruction.command);
    radio.setChannel(terminal0Channel - sample.termNum);
    radio.startListening();
  }
}

void sendSample(void) {
  //    lastPinState = sample.state;
  // sample.data = (millis() - time0);
  flagint = LOW;
  buzzer_off;
  red_off;
  green_off;
  blue_off;
  MsTimer2::stop();
  intermittency = false;
  //    Serial.println(sample.state);
  radio.stopListening();
  radio.setChannel(control0Channel - controlSwitch);
  bool sent = radio.write( &sample, sample_size);
  int retries = 0;
  while (!sent && (retries < 10) ) {
    sent = radio.write( &sample, sample_size);
    retries++;
  }
  flagint = LOW;

  // On sensorOnce mode send also the other state in order to facilitate Chronojump the reading
  if (! unlimitedMode) {
    waitingSensor = false;
    sample.state = !sample.state;
    delay(2);
    sent = radio.write( &sample, sample_size);
    int retries = 0;
    while ((retries < 10) && (!sent) ) {
      sent = radio.write( &sample, sample_size);
      retries++;
    }
  }

  if (!sent) {
    beep(500);
  } else {
    beep(25);
  }

  radio.setChannel(terminal0Channel - sample.termNum);
  radio.startListening();

  //    Serial.println("startListening()");
  //    Serial.print("getChannel = ");
  //    Serial.println(radio.getChannel());
  //    Serial.println(sample.data);
}

void controlint()
{
  if (waitingSensor == true) {
    Timer1.initialize(debounceTime);
  }
}

void debounce() {
  bool pinState = digitalRead(SENSOR_PIN);
  Timer1.stop();

  if (pinState != lastPinState) {
    flagint = HIGH;
    sample.state = pinState;
    lastPinState = pinState;
    #ifdef WICHRO
      digitalWrite(RED_PIN, pinState);
    #endif
  }
}

void executeCommand(uint16_t command)
{
  //    Serial.println("deactivating LEDs, Buzzer and sensor");
  deactivateAll();
  intermittency = false;
  MsTimer2::stop();

  if ((command & red) == red) {
    // Serial.println("activating RED");
    red_on;
    redIsActive = true;
  }

  if ((command & green) == green) {
    // Serial.println("activating GREEN");
    green_on;
    greenIsActive = true;
  }

  if ((command & blue) == blue) {
    //      Serial.println("activating BLUE");
    blue_on;
    blueIsActive = true;
  }

  if ((command & buzzer) == buzzer) {
    //      Serial.println("activating BUZZER");
    buzzer_on;
    activeBuzzer = true;
  }

  if ((command & setIntermittency) == setIntermittency) {
    //      Serial.println("Intermittent");
    intermittency = true;
    intermittencyStart(blinkPeriod);
  }

  if ((command & sensorOnce) == sensorOnce) {
    //      Serial.println("activating sensor once");
    time0 = millis(); //empieza a contar time
    lastPinState = digitalRead(SENSOR_PIN);
    waitingSensor = true;  //Terminal set to waiting touch/proximity
    unlimitedMode = false;
    interrupts();
  }

  if ((command & sensorUnlimited) == sensorUnlimited) {
    //      Serial.println("activating sensor unlimited");
    time0 = millis(); //empieza a contar time
    lastPinState = digitalRead(SENSOR_PIN);
    waitingSensor = true;  //Terminal set to waiting touch/proximity
    unlimitedMode = true;
    interrupts();
  }

  if ((command & ping) == ping) {
    sample.state = digitalRead(SENSOR_PIN);
    sendPong();
  }

  if (( (command & batteryLevel) == batteryLevel)) {
    sendBatteryLevel();
  }
}

void intermittencyStart(int period)
{
  MsTimer2::set(period / 2, switchPins);  //A change in the state of the LEDS must occur every period/2 milliseconds
  MsTimer2::start();
  
  if(redIsActive) red_on;
  if(greenIsActive) green_on;
  if(blueIsActive) blue_on;
  
}

//Function that changes the state of the LEDS that should be blinking
void switchPins(void)
{
  
  if (redIsActive) digitalWrite(RED_PIN, !digitalRead(RED_PIN));
  if (greenIsActive) digitalWrite(GREEN_PIN, !digitalRead(GREEN_PIN));
  if (blueIsActive) digitalWrite(BLUE_PIN, !digitalRead(BLUE_PIN));
  if (activeBuzzer) digitalWrite(BUZZER_PIN, !digitalRead(BUZZER_PIN));
}

void deactivateAll(void)
{
  MsTimer2::stop();
  red_off;
  green_off;
  blue_off;
  buzzer_off;
  redIsActive = false;
  greenIsActive = false;
  blueIsActive = false;
  activeBuzzer = false;
  waitingSensor = false;
}

//For debuging some commands can be received by the Serial port
void serialEvent()
{
  // Serial.println("SerialEvent");
  String inputString = Serial.readString();

  //Trimming all the characters after the ";" including it
  String commandString = inputString.substring(0, inputString.lastIndexOf(";"));
  if ( commandString == "get_channel") {
    Serial.println(radio.getChannel());
  } else if (commandString == "get_version") {
    Serial.print("Wifi-Sensor-");
    Serial.println(deviceVersion);
  }
}

void beep(unsigned int duration)
{
  MsTimer2::set(duration, beepStop);
  MsTimer2::start();
  buzzer_on;
}

void beepStop(void)
{
  MsTimer2::stop();
  buzzer_off;
}

void sendPong(void) {
  sample.data = deviceType * 1000000 + deviceVersion;
  // Serial.println(sample.data);
  // Serial.print("Wifi-Sensor-");
  // Serial.println(deviceVersion);
  flagint = LOW;
  unlimitedMode = true;
  waitingSensor = true;
  sendSample();
  buzzer_on;
  redIsActive = false;
  greenIsActive = true;
  blueIsActive = false;
  intermittency = true;
  intermittencyStart(75);
  delay(250);
  MsTimer2::stop();
  // TODO: Return to the state before the ping
  buzzer_off;
  green_off;
  greenIsActive = false;
}

void sendBatteryLevel() {
  int batteryValue = 0;
  for (int i = 0; i<10; i++) {
    batteryValue += analogRead(BATTERY_PIN);
    delay (100);
  }
  batteryValue = batteryValue / 10;
  batteryValue = map(batteryValue,600,790, 0, 100);
  if (batteryValue < 0) batteryValue = 0;
  if (batteryValue > 100) batteryValue = 100;
  sample.data = (unsigned long int)batteryValue;
  radio.stopListening();
  radio.setChannel(control0Channel - controlSwitch);
  radio.write( &sample, sample_size);
  radio.setChannel(terminal0Channel - sample.termNum);
  radio.startListening();
  // Serial.println(sample.data);
}

// Returns the number of active LEDs (red, green and/or blue)
// Useful to atenuate the intensity in the case of combining more than one color
int getActiveLeds() {
  return ( ((instruction.command & red) == red)
  + ( ((instruction.command & green) == green) )
  + ( ((instruction.command & blue) == blue) ) );
}

/*
// The instruction.command includes a parity bit in the Most Significative Bit.
// This fuction checks that the command has arrived without corruption.
// It returns true if an even number of bits has changed
bool parityError (uint16_t var) {
  // var & 0b1000000000000000 seeks for the parity bit. The Most significant bit
  // >> 15 shift the bit to the least significant position to have a bool
  return ( (var & 0b1000000000000000)>>15 != (parity_even_bit(var) ));
}
*/