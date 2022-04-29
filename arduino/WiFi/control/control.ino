
#include <SPI.h>
//#include <nRF24L01.h>
#include <RF24.h>
#include <printf.h>
#include <MsTimer2.h>

String version = "Wifi-Controller-1.11"; //"Wifi-Controller-" is mandatori. Chronojump expects it
//
// Hardware configuration

//          pin2  ----- Green LED
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

bool blinkingLED = false;
int blinkPeriod = 75; //Time between two consecutives rising flank of the LED

// binary commands: each bit represents RED, GREEN, BLUE, BUZZER, BLINK_RED, BLINK_GREEN, BLINK_BLUE, SENSOR
// 1 means ON
// 0 means OFF
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
unsigned long startTime;      //local time when the reset_time function is executed
unsigned long lastSampleTime; //local time at which some sample has been received without overflow correction
unsigned long totalTime;      //Total elapsed time since startTime

bool waitingVersion = false;

void setup(void)
{


  Serial.begin(115200);


  Serial.println(version);

  //When something arrives to the serial, how long to wait for being sure that the whole text is received
  //TODO: Try to minimize this parameter as it adds lag from instruction to sensor activation. 1 is too low.
  //Maybe increasing the baud rate we could set it to 1
  Serial.setTimeout(2);
  printf_begin();       //Needed by radio.printDetails();
  pinMode(2, OUTPUT);   //The LED is in output mode
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
  radio.setRetries(15, 1); //this is working flawlessly, at least in short distance
  //radio.setRetries(15, 15); //this is supposedly to fix when there's lot of distance

  //maximum 125 channels. cell phone and wifi uses 2402-2472. Free from channel 73 to channel 125. Each channels is 1Mhz separated
  radio.setChannel(control0Channel - controlSwitch);
  radio.openWritingPipe(pipes[0]);
  radio.openReadingPipe(1, pipes[1]);
  radio.startListening();

  //  Serial.println(" Status Radio");
  //  radio.printDetails();

  Serial.println("the instructions are [termNum]:[command];");

  Serial.println("NumTerm\tTime\tState");
  Serial.println("------------------------");

  startTime = millis();
  //  discoverTerminals();
}


void loop(void)
{
  readSample();
}


void serialEvent()
{

  String inputString = Serial.readString();
  //  Serial.print("Instruction received from Serial: \"");
  //  Serial.print(inputString);
  //  Serial.println("\"");
  int separatorPosition = inputString.lastIndexOf(":");

  String terminalString = inputString.substring(0, separatorPosition);
  //  Serial.print("terminalString:\"");
  //  Serial.print(terminalString);
  //  Serial.println("\"");

  String commandString = inputString.substring(separatorPosition + 1, inputString.lastIndexOf(";"));
  //  Serial.print("commandString: \"");
  //  Serial.print(commandString);
  //  Serial.println("\"");

  if (terminalString == "all")  //The command is sent to all the terminals
  {
    activateAll(instruction.command);
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
      startTime = millis();
    } else if (commandString == "get_channel") {
      Serial.println(controlSwitch);
    } else if (commandString == "discover") {
      discoverTerminals();
    } else {
      Serial.println("Wrong local command");
    }
  } else if(terminalString == "get_version") {
    Serial.println(version);
  } else {  // if terminalString is a single remote terminal, Command to a single terminal
    instruction.command = commandString.toInt();
    //    Serial.print("Command: ");
    //    Serial.println(instruction.command);
    instruction.termNum = terminalString.toInt();
    //      Serial.print("instruction.termNum:\"");
    //      Serial.print(instruction.termNum);
    //      Serial.println("\"");
    sendInstruction(&instruction);
    if(instruction.command == ping) waitingVersion = true;
  }
  if (instruction.command & sensorOnce) {
    blinkStart(blinkPeriod);
    blinkingLED = true;
  }
  inputString = "";
}

bool sendInstruction(struct instruction_t *instruction)
{

  //  Serial.print("Sending command \'");
  //  Serial.print(instruction->command);
  //  Serial.print("\' to terminal num ");
  //  Serial.println(instruction->termNum);
  //  Serial.println(terminal0Channel - instruction->termNum);

  radio.setChannel(terminal0Channel - instruction->termNum); //Setting the channel correspondig to the terminal number

  radio.stopListening();    //To sent it is necessary to stop listening

  bool en = radio.write( instruction, size_instruction );
  radio.setChannel(control0Channel - controlSwitch);    //setting the the channel to the reading channel
  radio.startListening();  //Going back to listening mode
  LED_off;
  instruction->termNum = 0;

  if (!en)  //en is 1 if radio.write went OK
  {
    //    Serial.println("Error sending");
    return (false);
  } else {
    //    Serial.print("send instruction:");
    //    Serial.println(instruction->command);
    //    Serial.println(instruction->termNum);
    return (true);
  }
}

// Atention this function is not valid for ping all terminals as it does not wait for response.
void activateAll(uint16_t command)
{
  Serial.println("---------Activating All---------");
  radio.stopListening();
  for (int i = 0; i <= 63; i++) {
    radio.setChannel(terminal0Channel - i);
    //    Serial.print("getChannel = ");
    //    Serial.println(radio.getChannel());
    instruction.termNum = i;
    instruction.command = command;
    sendInstruction(&instruction);
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

//This fucnction manages the time elapsed from the start of the
unsigned long getLocalTime(void)
{
  //not to be confused with sample.data . This is the local time at which the sample has been received.
  //sample.data is the elapsed time since the terminal received the activating sensor command untill actual activation.
  unsigned long localSampleTime = millis();
  if (localSampleTime > startTime)           //No overflow
  {
    totalTime = localSampleTime - startTime;
  } else if (localSampleTime <= startTime)   //Overflow
  {
    //Time from the last measure to the overflow event plus the sampleTime
    totalTime = (4294967295 -  lastSampleTime) + localSampleTime;
  }
  return (totalTime);
}

void discoverTerminals(void) {
  String terminalsFound = "terminals:";
  instruction.command = ping;
  for (int i = 0; i <= 63; i++) {
    //    Serial.print("TERM: ");
    //    Serial.println(i);
    //    Serial.println("------");


    bool found = false;
    for (int tries = 1; tries <= 10 && ! found; tries++) {
      //      Serial.println(tries);

      radio.flush_tx();
      radio.flush_rx();
      instruction.termNum = i;
      waitingVersion = true;
      sendInstruction(&instruction);

      delay(5);
      bool readed = readSample();
      if (readed && sample.termNum == i) { //do not do more tries
        found = true;
        terminalsFound = terminalsFound + i + ";";
      }
    }
    //    Serial.println();
  }
  Serial.println(terminalsFound);
}

bool readSample(void) {
  bool readed = false;
  while (radio.available()) //Some terminal has sent a response
  {
    totalTime = getLocalTime();
    radio.read(  &sample, sample_size);
    blinkStop();
    if (!binaryMode) {
      //Serial.print(" READED: ");
      Serial.print(sample.termNum);
      Serial.print(";");
      Serial.print(totalTime);
      Serial.print(";");
      Serial.print(sample.state);
      if(waitingVersion) {
        Serial.print(";");
        Serial.print(sample.data);
        waitingVersion = false;
      }
      Serial.println();
      readed = true;
    } else {
      Serial.write((byte*)&sample, sample_size);
    }
    //blinkOnce();
    digitalWrite(2, !sample.state);
    //Serial.print("readed will be:");
    //Serial.println(readed);
  }
  return (readed);
}
