
#include <SPI.h>
//#include <nRF24L01.h>     //TODO: Check that it is necessary
#include <RF24.h>
#include <printf.h>
#include <MsTimer2.h>
#include <TimerOne.h>

unsigned int deviceType = 1; //Photocel and LightChro sensor
unsigned int deviceVersion = 12;
//
// Hardware configuration


//          pin11 -----  M0
//          pin12 ----- MSO
// Arduino  pin13 ----- SCK  NRf24L01
//          pin A4 ---- CSN
//          pin A3 ----  CE
//          3,3v       NRf24L01
//          GND        NRf24L01

// Set up nRF24L01 radio on SPI bus plus pins  (CE & CS)


RF24 radio(A3, A4);    //Old versions
//RF24 radio(10, 9);       //New version
#define red_on digitalWrite(A4,LOW)
#define green_on digitalWrite(A5,LOW)
#define blue_on digitalWrite(A3,LOW)
//#define buzzer_on digitalWrite(7,HIGH)  //Old versions
#define buzzer_on digitalWrite(A0,HIGH) //New versions
#define red_off digitalWrite(A4,HIGH)
#define green_off digitalWrite(A5,HIGH)
#define blue_off digitalWrite(A3,HIGH)
//#define buzzer_off digitalWrite(7,LOW)  //Old versions
#define buzzer_off digitalWrite(A0,LOW) //New versions

struct instruction_t
{
  uint16_t command;       //The command to be executed
  short int termNum;  //The terminal number that have to execute the command
};

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
volatile int debounceTime = 1;
//The timer overflows inmediately after initialization.
//We are aware only in the second overflow
volatile int debounceCount = 0;  //Number of the timer overflows

// First channel to be used. The 6xswitches control the terminal number and the number to add the terminal0Channel
// The channel 125 is used to listen from the terminals.
// Channels 116 - 64 (descending) are used to send to the terminals
uint8_t terminal0Channel = 116; //TODO: Select the listening channel with the switches

//Channel of the controler
uint8_t control0Channel = 125; //Channel resulting of the switch at zero state
uint8_t controlSwitch = 0;      //State of the 3xswithes

bool waitingSensor = true; //Wether the sensor is activated or not
bool unlimitedMode = true;

//Variables to control the blinking of each Color
bool blinkingRed = false;
bool blinkingGreen = false;
bool blinkingBlue = false;

int blinkPeriod = 75; //Time between two consecutives rising flank of the LED

const uint64_t pipes[2] = { 0xF0F0F0F0E1LL, 0xF0F0F0F0D2LL }; //Two radio pipes. One for emitting and the other for receiving

void setup(void)
{

  //radio.setPALevel (RF24_PA_HIGH); //funcion  que solo se llama una vez si se quiere cambiar de potencia
  //pot= getPALevel( ); //función que retorna potencia programada

  Serial.begin(115200);
  printf_begin(); //Used by radio.printDetails()

  Serial.print("Wifi-Sensor-");
  Serial.println(deviceVersion);

  radio.begin();

  //Reading each pin to stablish the terminal number and the listening channel

  //¡¡¡¡Atention!!!!, the first version of lightChro the pin7 is associated to the buzzer.
  //The first versions of the Quick the microswitch was 5xSwitches. The last one associated to the buzzer
  //In the first versions of the photocells the microswitch was 8xSwitches
  //Remember to change comment/uncomment depending on the hardware version

  pinMode(3, INPUT_PULLUP);   //Least significant bit
  pinMode(4, INPUT_PULLUP);
  pinMode(5, INPUT_PULLUP);
  pinMode(6, INPUT_PULLUP);
  pinMode(7, INPUT_PULLUP);   //Comment in old versions
  pinMode(8, INPUT_PULLUP);   //Most significant bit.

  //for (int pin = 6; pin >= 3; pin--)  //Old versions
  for (int pin = 8; pin >= 3; pin--)    //New versions
  {
    sample.termNum = sample.termNum * 2; //Each bit will be multiplied by 2 as much times as his significance
    if (!digitalRead(pin)) sample.termNum++;
  }


  Serial.print("termNum: ");
  Serial.println(sample.termNum);

  //maximum 125 channels. cell phone and wifi uses 2402-2472. Free from channel 73 to channel 125. Each channels is 1Mhz separated
  radio.setChannel(terminal0Channel - sample.termNum);
  Serial.print("Terminal Channel: ");
  Serial.println(terminal0Channel - sample.termNum);


  radio.openWritingPipe(pipes[1]);
  radio.openReadingPipe(1, pipes[0]);

  //radio.enableDynamicAck();
  radio.startListening();

  //  printf(" Status Radio\n\r");
  //  radio.printDetails();

  //  Serial.print("Channel set to: ");
  //  Serial.println(radio.getChannel());

  Serial.flush(); //Flushing the buffer serial buffer to avoid spurious data.

  // channel of the controler. 3xmicroswith controls this channel
  //************************************************************************************
  // A0, A1, A2 connected to the 3xswith

  pinMode(A0, INPUT_PULLUP);  //Old versions
  //pinMode(A7, INPUT_PULLUP);    //New version
  pinMode(A1, INPUT_PULLUP);
  pinMode(A2, INPUT_PULLUP);

  //   Se leeran en binario y se restará al canal por defecto 125
  if ( !digitalRead(A0)) {     //Old versions
  //  if (analogRead(A7)<128) {   //New versions
    controlSwitch = 1; //
  }
  if (!digitalRead(A1)) {
    controlSwitch = controlSwitch + 2;
  }
  if (!digitalRead(A2)) {
    controlSwitch = controlSwitch + 4;
  }

  Serial.print("ControlChannel: ");
  Serial.println(control0Channel - controlSwitch);

  //Activate interruption service each time the sensor changes state
  attachInterrupt(digitalPinToInterrupt(2), controlint, CHANGE);

  pinMode(A3, OUTPUT);    //Blue
  pinMode(A4, OUTPUT);    //Green
  pinMode(A5, OUTPUT);    //Red
  pinMode(A0, OUTPUT);     //Buzzer
  buzzer_on;
  delay(100);
  buzzer_off;
  pinMode(2, INPUT_PULLUP); //Sensor

  red_off;
  green_off;
  blue_off;


  //  noInterrupts();   //Don't watch the sensor state

  //  Serial.print("Is chip connected:");
  //  Serial.println(radio.isChipConnected());

  Timer1.initialize(2000); //Initializing the debounce timer to 2 ms
  Timer1.detachInterrupt();
  sample.state = digitalRead(2);
  lastPinState = sample.state;
  Serial.println("Initial state");
  Serial.print(lastPinState);
  Serial.print("\t");
  Serial.println(sample.state);
  
  //radio.printPrettyDetails();
  Serial.print("Power: ");
  Serial.println(radio.getPALevel());
}


void loop(void)
{
  //if (flagint == HIGH && lastPinState != sample.state) //The sensor has changed
  if (flagint == HIGH) //The sensor has changed
  {
    sendSample();
  }

  while (radio.available())
  {
    radio.read(  &instruction, size_instruction);
    radio.stopListening();
//    delay(100);
//    Serial.print("Command received: ");
//    Serial.print("termNum received: ");
//    Serial.println(instruction.termNum);
//    radio.flush_rx();

    //Some times the terminal receives instructions of other terminals
    if (instruction.termNum == sample.termNum)
    {
      executeCommand(instruction.command);
    }
    radio.startListening();
  }
}

void sendSample(void) {
  //    lastPinState = sample.state;
  sample.data = (millis() - time0);
  flagint = LOW;
  buzzer_off;
  red_off;
  green_off;
  blue_off;
  MsTimer2::stop();
  blinkingRed = false;
  blinkingGreen = false;
  blinkingBlue = false;
  //    Serial.println(sample.state);
  radio.stopListening();
  radio.setChannel(control0Channel - controlSwitch);
  //    Serial.print("getChannel = ");
  //    Serial.println(radio.getChannel());
  bool en = radio.write( &sample, sample_size);
  //    if (en) {
  //      Serial.println("Sent OK");
  //    } else {
  //      Serial.println("Error sending");
  //    }
  //    Serial.print("getChannel = ");
  //    Serial.println(radio.getChannel());
  beep(25);
  flagint = LOW;
  if (! unlimitedMode) waitingSensor = false;
  radio.setChannel(terminal0Channel - sample.termNum);
  radio.startListening();

  //    Serial.println("startListening()");
  //    Serial.print("getChannel = ");
  //    Serial.println(radio.getChannel());
  //    Serial.println(sample.data);
}

void controlint()
{
//  sample.state = digitalRead(2);
//  Serial.println("Int");
  if (waitingSensor == true) {
//  flagint = HIGH;
//    Timer1.initialize(1000000); //Initializing the debounce timer to 1s
    Timer1.attachInterrupt(debounce);
//    Serial.print(lastPinState);
//    Serial.print("\t");
//    Serial.println(sample.state);
    debounceCount = 0;
  }
}

void debounce() {
  debounceCount++;
//  Serial.println(debounceCount);
  if (debounceCount >= 2) {
    sample.state = digitalRead(2);
    Timer1.detachInterrupt();
//    Serial.println("in debounce");

    if (sample.state != lastPinState) {
      flagint = HIGH;
      lastPinState = sample.state;
      Serial.print(lastPinState);
      Serial.print("\t");
      Serial.println(sample.state);
//      Serial.println(millis());
    }
  }
}

void executeCommand(uint16_t command)
{
  if (command == deactivate) {
    //    Serial.println("deactivating leds and sensor");
    deactivateAll();
  } else
  {
    red_off;
    green_off;
    blue_off;
    blinkingRed = false;
    blinkingGreen = false;
    blinkingBlue = false;
    MsTimer2::stop();

    if ((command & red) == red) {
      Serial.println("activating RED");
      red_on;
    }

    if ((command & green) == green) {
      //      Serial.println("activating GREEN");
      green_on;
    }

    if ((command & blue) == blue) {
      //      Serial.println("activating BLUE");
      blue_on;
    }

    if ((command & buzzer) == buzzer) {
      //      Serial.println("activating BUZZER");
      buzzer_on;
    }

    if ((command & blinkRed) == blinkRed) {
      //      Serial.println("blinking RED");
      blinkingRed = true;
      blinkStart(blinkPeriod);
    }

    if ((command & blinkGreen) == blinkGreen) {
      //      Serial.println("blinking GREEN");
      blinkingGreen = true;
      blinkStart(blinkPeriod);
    }

    if ((command & blinkBlue) == blinkBlue) {
      //      Serial.println("blinking BLUE");
      blinkingBlue = true;
      blinkStart(blinkPeriod);
    }

    if ((command & sensorOnce) == sensorOnce) {
      //      Serial.println("activating sensor once");
      time0 = millis(); //empieza a contar time
      waitingSensor = true;  //Terminal set to waiting touch/proximity
      unlimitedMode = false;
      interrupts();
    }

    if ((command & sensorUnlimited) == sensorUnlimited) {
      //      Serial.println("activating sensor unlimited");
      time0 = millis(); //empieza a contar time
      waitingSensor = true;  //Terminal set to waiting touch/proximity
      unlimitedMode = true;
      interrupts();
    }

    if ((command & ping) == ping) {
      sample.state = digitalRead(2);
      //radio.setRetries(15, 15);
      sendPong();
      //radio.setRetries(5, 15);
    }
  }
}

void blinkStart(int period)
{
  MsTimer2::set(period / 2, blinkLed);  //A change in the state of the LEDS must occur every period/2 milliseconds
  MsTimer2::start();

  if (blinkingRed) red_on;
  if (blinkingGreen) green_on;
  if (blinkingBlue) blue_on;
}

//Function that changes the state of the LEDS that should be blinking
void blinkLed(void)
{
  if (blinkingRed) digitalWrite(A4, !digitalRead(A4));
  if (blinkingGreen) digitalWrite(A5, !digitalRead(A5));
  if (blinkingBlue) digitalWrite(A3, !digitalRead(A3));
}

void deactivateAll(void)
{
  MsTimer2::stop();
  red_off;
  green_off;
  blue_off;
  buzzer_off;
  waitingSensor = false;
}

//For debuging some commands can be received by the Serial port
void serialEvent()
{
  Serial.println("SerialEvent");
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

void beep(int duration)
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
//  Serial.println(sample.data);
//  Serial.print("Wifi-Sensor-");
//  Serial.println(deviceVersion);
  flagint = LOW;
  MsTimer2::stop();
  radio.stopListening();
  delay(10);
  radio.setChannel(control0Channel - controlSwitch);
  //delay(10);
  bool en = radio.write( &sample, sample_size);
  flagint = LOW;
  if (! unlimitedMode) waitingSensor = false;
  //delay(10);
  radio.setChannel(terminal0Channel - sample.termNum);
  //delay(10);
  //radio.startListening();
  //delay(10);
}
