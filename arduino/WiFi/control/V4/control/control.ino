/*
  Controler for devices using the NRf24L01 wireless module

  Copyright (C) 2018 Xavier de Blas xaviblas@gmail.com
  Copyright (C) 2018-2024 Xavier Padullés support@chronojump.org

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
//#include <nRF24L01.h>
#include <RF24.h>
//#include <printf.h>
#include <MsTimer2.h>
#include <TimerOne.h>
#include <elapsedMillis.h>
#include  <util/parity.h>

// The first number refers to the hardware version. The seccond to firmware version for this hardware
String version = "Wifi-Controller-4.5"; //"Wifi-Controller-" is mandatori. Chronojump expects it


//
// Hardware configuration

//          pin2  ----- Green LED
//          pin3  ----- RCA
//          pin11 -----  M0
//          pin12 ----- MSO
// Arduino  pin13 ----- SCK  NRf24L01
//          pin A4 ---- CSN
//          pin A3 ----  CE
//          3,3v       NRf24L01
//          GND        NRf24L01



// Set up nRF24L01 radio on SPI bus plus pins  (CE & CS)
RF24 radio(A3, A4);

#define LED_on digitalWrite(2,HIGH)
#define LED_off digitalWrite(2,LOW)

int radioDelay = 0;

int rcaPin = 3;
bool flagint = LOW;   //Interruption flag. Activated when the sensos changes
volatile bool lastPinState = LOW;  //stores the state of the RCA pin before the interruption
unsigned long debounceTime = 2000;
enum pinMode {
  input,
  output,
};

enum pinMode rcaMode = input;

bool blinkingLED = false;
int blinkPeriod = 75; //Time between two consecutives rising flank of the LED

// binary commands: each bit represents RED, GREEN, BLUE, BUZZER, BLINK_RED, BLINK_GREEN, BLINK_BLUE, SENSOR
// 1 means ON
// 0 means OFF
const uint16_t batteryLevel  = 0b10000000000; //1024
const uint16_t ping           = 0b1000000000; //512
const uint16_t sensorUnlimited = 0b100000000; //256
const uint16_t red =              0b10000000; //128
const uint16_t green =            0b01000000; //64
const uint16_t blue =             0b00100000; //32
const uint16_t buzzer =           0b00010000; //16
const uint16_t blinkRed =         0b00001000; //8
const uint16_t blinkGreen =       0b00000100; //4
const uint16_t blinkBlue =        0b00000010; //2
const uint16_t sensorOnce =       0b00000001; //1
const uint16_t deactivate =       0b00000000; //0

struct instruction_t
{
  uint16_t command;       //The command to be executed
  short int termNum;  //The terminal number that have to execute the command
};

struct instruction_t instruction = {.command = deactivate, .termNum = 0};
int size_instruction = sizeof(instruction);

struct sample_t
{
  bool state;           //State of the sensor
  short int termNum;    //Terminal number. Configured with the switches.
  unsigned long int data;
};

struct sample_t sample = {.state = LOW, .termNum = 0, .data = 0};
int sample_size = sizeof(sample);       //sample_size es la longitud de variables a recibir .

// First channel to be used. The 6xswitches control the terminal number and the number to add the terminal0Channel
// The channel 125 is used to listen from the terminals.
// Channels 116 - 64 (descending) are used to send to the terminals
uint8_t terminal0Channel = 116; //TODO: Select the listening channel with the switches

//Channel of the controler
uint8_t control0Channel = 125; //Channel resulting of the switch at zero state
uint8_t controlSwitch = 0;      //State of the 3xswithes

const uint64_t pipes[2] = { 0xF0F0F0F0E1LL, 0xF0F0F0F0D2LL }; //Two radio pipes. One for emitting and the other for receiving

bool binaryMode = false;
//unsigned long startTime;      //local time when the reset_time function is executed
unsigned long lastSampleTime; //local time at which some sample has been received without overflow correction
elapsedMillis totalTime;      //Total elapsed time since startTime

// unsigned long responseTime = 0;  //For testing response time in a ping

bool waitingData = false;

bool debug = false;
bool read13Commands = false; //note making this true is a big problem because then we cannot send a get_version or a discover (less than 13 commands)


void setup(void)
{


  Serial.begin(115200);


  Serial.println(version);

  //When something arrives to the serial, how long to wait for being sure that the whole text is received
  //TODO: Try to minimize this parameter as it adds lag from instruction to sensor activation. 1 is too low.
  //Maybe increasing the baud rate we could set it to 1
  Serial.setTimeout(2);
  //printf_begin();       //Needed by radio.printDetails();

  LED_on;  //turn off the LED

  // channel of the controler
  //************************************************************************************
  // A0, A1, A2 connected to the 3xswith

  pinMode(A0, INPUT_PULLUP);
  pinMode(A1, INPUT_PULLUP);
  pinMode(A2, INPUT_PULLUP);

  //   En estas entradas se pondra un microswich , de 3 botones
  //   Se leeran en binario y se sumarán al canal por defecto 101
  if (!digitalRead(A0)) {
    controlSwitch = 1; //
  }
  if (!digitalRead(A1)) {
    controlSwitch = controlSwitch + 2;
  }
  if (!digitalRead(A2)) {
    controlSwitch = controlSwitch + 4;
  }


  //  Serial.print("ControlChannel: ");
  //  Serial.print(control0Channel);
  //  Serial.print(" - ");
  //  Serial.println(controlSwitch);

  radio.begin();
  radio.setRetries(0, 15); // Delay and maximum retries. 0.250*(1 + delay) microseconds of delay between retries.

  //maximum 125 channels. cell phone and wifi uses 2402-2472. Free from channel 73 to channel 125. Each channels is 1Mhz separated
  radio.setChannel(control0Channel - controlSwitch);
  radio.openWritingPipe(pipes[0]);
  radio.openReadingPipe(1, pipes[1]);
  radio.startListening();

  //  Serial.println(" Status Radio");
  //  radio.printDetails();

  //Activate interruption service each time the sensor changes state

  pinMode(2, OUTPUT);   //The LED is in output mode
  setRcaMode(rcaMode);
  // Config of the debounce timer
  Timer1.initialize(debounceTime);
  Timer1.attachInterrupt(debounce);
  Timer1.stop();

  Serial.println("the instructions are [termNum]:[command];");

  Serial.println("NumTerm\tTime\tState");
  Serial.println("------------------------");

  //startTime = millis();
}


void loop(void)
{
  readSample();
  // if (batteryTimer > 2000) {
  //   instruction.command = 1024;
  //   instruction.termNum = 1;
  //   sendInstruction(&instruction);
  //   waitingData = true;
  //   batteryTimer = 0;
  // }
}


void controlint()
{
  Timer1.initialize(debounceTime);
}

void debounce() {
  Timer1.stop();

  if (digitalRead(rcaPin) != lastPinState) {
    flagint = HIGH;
    lastPinState = !lastPinState;
    sample.state = lastPinState;
  }
}

//count the number of commands.
int countSemicolons (String str)
{
  int countSemicolons = 0;
  int currentPos = -1;
  while (true)
  {
    if(str.indexOf (";", (currentPos +1)) == -1)
      break;
    else {
      currentPos = str.indexOf (";", (currentPos +1));
      countSemicolons ++;
    }
  }
  return countSemicolons;
}


void serialEvent()
{
  String inputString = "";
  // String pendingString = "";   // Used to retry failed transmissions
  // int retries = 0;   // Used to retry failed transmissions

  // Reads until the NewLine character ("\n") is found
  while ( !lineHasNL(inputString) ) {
    inputString.concat(Serial.readString());
  }


  // String originalString = inputString;   // Used to start over the whole process of sending the inputString
  String currentInstruction = "";

  int lastIndex = inputString.lastIndexOf(";");
  // Serial.println(lastIndex);
  int prevSeparatorIndex = -1;
  int nextSeparatorIndex = 0;
  
  while (prevSeparatorIndex < lastIndex) {
    nextSeparatorIndex = inputString.indexOf(";", prevSeparatorIndex +1);
    currentInstruction = inputString.substring(prevSeparatorIndex +1 , nextSeparatorIndex +1);
    prevSeparatorIndex = nextSeparatorIndex;

    int separatorPosition = currentInstruction.indexOf(":");

    String terminalString = currentInstruction.substring(0, separatorPosition);
    String commandString = currentInstruction.substring(separatorPosition + 1, currentInstruction.lastIndexOf(";"));

    if (terminalString == "all")  //The command is sent to all the terminals
    {
      instruction.command = commandString.toInt();
      sendToAll(instruction.command);
    } else if (terminalString == "local") {
      if (commandString == "get_version") {
        Serial.println(version);
      } else if (commandString == "set_binary_mode") {
        Serial.println("Setting binary mode");
        binaryMode = true;
      } else if (commandString == "set_text_mode") {
        Serial.println("Setting text mode");
        binaryMode = false;
      } else if (commandString == "reset_time") {
        totalTime = 0;
      } else if (commandString == "get_channel") {
        Serial.println(controlSwitch);
      } else if (commandString == "discover") {
        discoverTerminals();
      } else if(commandString == "set_rca_mode:output") {
        setRcaMode(output);
      } else if(commandString == "set_rca_mode:input") {
        setRcaMode(input);
      } else {
          Serial.println("Wrong local command");
      }
    } else {  // if terminalString is a single remote terminal, Command to a single terminal
      instruction.command = commandString.toInt();
      instruction.termNum = terminalString.toInt();

      // To check that the transmission is ok a parity bit is added to the MSB
      // instruction.command = instruction.command | (parity_even_bit(instruction.command)<<15 );

      sendInstruction(&instruction);

      // responseTime = micros();   // For testing response time in a ping
      

      /* //If unseccessful sending, keep the instruction for later trying.
      if (sendInstruction(&instruction) > 0) {
        pendingString.concat( currentInstruction );
      }

      // If last instruction processed, process the unsuccesful instructions
      if (prevSeparatorIndex >= lastIndex && pendingString != "" ) {
        if (retries < 3) {
        Serial.print("pendingCommands: ");
        Serial.println(pendingString);
        inputString = pendingString;
        pendingString = "";
        lastIndex = inputString.lastIndexOf(";");
        prevSeparatorIndex = -1;
        nextSeparatorIndex = 0;
        retries++;
        delay(radioDelay);
        } else if (retries >= 3 ) {
          inputString = originalString;
          delay(radioDelay);
        }
      }
      */

      if((instruction.command == ping) ||  (instruction.command == batteryLevel) ) {
        waitingData = true;
      }
    }

    if (instruction.command & sensorOnce) {
      blinkStart(blinkPeriod);
      blinkingLED = true;
    }
  }
}

unsigned int sendInstruction(struct instruction_t *instruction)
{
  
  if (instruction->termNum > 63) {
    Serial.println("Terminal number too high");
    return(0);
  }

  radio.setChannel(terminal0Channel - instruction->termNum); //Setting the channel correspondig to the terminal number

  radio.stopListening();    //To sent it is necessary to stop listening

  bool sent = radio.write( instruction, size_instruction );
  bool reply = false;
  unsigned int sendingRetries = 0;
  unsigned int confirmingTries = 0;

  while ( !sent && (sendingRetries < 4) ) {
    delay(radioDelay);
    sent = radio.write( instruction, size_instruction );
    sendingRetries++;
  }

  radio.startListening();
  radio.setChannel(control0Channel - controlSwitch);

  if(debug && sendingRetries >0) {
    Serial.println(instruction->termNum);
  }

  return(sendingRetries);
}

// Atention this function is not valid for ping all terminals as it does not wait for response.
void sendToAll(uint16_t command)
{
  radio.stopListening();
  for (int i = 0; i <= 63; i++) {
    radio.setChannel(terminal0Channel - i);
    //    Serial.print("getChannel = ");
    //    Serial.println(radio.getChannel());
    instruction.termNum = i;
    instruction.command = command;
    unsigned int retries = sendInstruction(&instruction);
  }
  radio.startListening();
  radio.setChannel(control0Channel - controlSwitch);
}

void blinkStart(int period)
{
  MsTimer2::set(period / 2, blinkLed);  //A change in the state of the LEDS must occur every period/2 milliseconds
  MsTimer2::start();
  LED_on;
}

void blinkStop(void)
{
  MsTimer2::stop();
}

void blinkLed(void)
{
  digitalWrite(2, !digitalRead(2));
}

void blinkOnce(void)
{
  LED_off;
  MsTimer2::set(50, blinkStop);
  LED_on;
}

void discoverTerminals() { discoverTerminals(5); }
void discoverTerminals(int maxTries) {
  String terminalsFound = "terminals:";
  int totalFound = 0;
  instruction.command = ping;
  for (int i = 0; i <= 63; i++) {
    for (int tries = 1;  tries <= maxTries; tries++) {

      radio.flush_tx();
      radio.flush_rx();
      instruction.termNum = i;
      waitingData = true;
      sendInstruction(&instruction);

      delay(radioDelay+2);
      bool readed = readSample();
      if (readed && sample.termNum == i) { //do not do more tries
        terminalsFound = terminalsFound + i + ";";
        totalFound++;
        break;
      }
    }
  }
  Serial.print("Total found: ");
  Serial.println(totalFound);
  Serial.print("Terminals found: ");
  Serial.println(terminalsFound);
  waitingData = false;
}

bool readSample(void) {
  bool readed = false;
  if (radio.available()) //Some terminal has sent a response
  {
    radio.read(  &sample, sample_size);
    readed = true;
    blinkStop();

    // Testing the response time in a ping
    // responseTime = micros() - responseTime;
    // if (responseTime > 1000) {
    //   Serial.println(responseTime);
    // }
    printSample();
  }

  if (flagint) {
    flagint = false;
    readed = true;
    printSample();
  }
  return (readed);
}

void printSample() {
  if (!binaryMode) {
    //Serial.print(" READED: ");
    Serial.print(sample.termNum);
    Serial.print(";");
    Serial.print(totalTime);
    Serial.print(";");
    Serial.print(sample.state);
    if(waitingData) {
      Serial.print(";");
      Serial.print(sample.data);
      waitingData = false;
    }

    Serial.println();
  } else {
    Serial.write((byte*)&sample, sample_size);
  }
//blinkOnce();
digitalWrite(2, !sample.state);
if (rcaMode == output) { digitalWrite(rcaPin, !sample.state); }
//Serial.print("readed will be:");
//Serial.println(readed);

}

void setRcaMode(enum pinMode mode) {
  rcaMode = mode;
  if (rcaMode == input) {
    Serial.println("RCA in INPUT");
    pinMode(rcaPin, INPUT_PULLUP);
    attachInterrupt(digitalPinToInterrupt(rcaPin), controlint, CHANGE);
    lastPinState = digitalRead(rcaPin);
  } else if (rcaMode == output) {
    Serial.println("RCA in OUTPUT");
    detachInterrupt(digitalPinToInterrupt(rcaPin));
    pinMode(rcaPin, OUTPUT);
  }
}

// Check that the string has a new line character ( '\n' ).
bool lineHasNL(String &inputString) {
  return(inputString.indexOf('\n') != -1);
}
