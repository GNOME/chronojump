
#include <SPI.h>
#include <nRF24L01.h>
#include <RF24.h>
#include <printf.h>
#include <MsTimer2.h>

String version = "LightChro-Sensor-1.05";
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

// Set up nRF24L01 radio on SPI bus plus pins  (CE & CS)
RF24 radio(10, 9);
#define red_on digitalWrite(A4,LOW)
#define green_on digitalWrite(A5,LOW)
#define blue_on digitalWrite(A3,LOW)
#define buzzer_on digitalWrite(7,HIGH)
#define red_off digitalWrite(A4,HIGH)
#define green_off digitalWrite(A5,HIGH)
#define blue_off digitalWrite(A3,HIGH)
#define buzzer_off digitalWrite(7,LOW)

struct instruction_t
{
  byte command;       //The command to be executed
  short int termNum;  //The terminal number that have to execute the command
};

// binary commands: each bit represents RED, GREEN, BLUE, BUZZER, BLINK_RED, BLINK_GREEN, BLINK_BLUE, SENSOR
// 1 means ON
// 0 means OFF
const byte red = 0b10000000;
const byte green = 0b01000000;
const byte blue = 0b00100000;
const byte buzzer = 0b00010000;
const byte blinkRed = 0b00001000;
const byte blinkGreen = 0b00000100;
const byte blinkBlue = 0b00000010;
const byte sensor = 0b00000001;
const byte deactivate = 0b00000000;

struct instruction_t instruction = {.command = deactivate, .termNum = 0};
int size_instruction = sizeof(instruction);

struct sample_t
{
  bool state;           //State of the sensor
  short int termNum;    //Terminal number. Configured with the switches.
  //TODO: treat as a binary to activate each terminal separately
  unsigned long int elapsedTime;
};

struct sample_t sample = {.state = LOW, .termNum = 0, .elapsedTime = 0};

int sample_size = sizeof(sample); 

unsigned long time0;  //Time when the command is received

bool flagint = LOW;   //Interruption flag. Activated when the sensos changes

// First channel to be used. The 5xswitches control the terminal number and the number to add the baseChannel
// The channel 125 is used to listen from the terminals. Channels 90-124 are used to send to the terminals
uint8_t baseChannel = 90;

bool waitingSensor = false; //Wether the sensor is activated or not

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
  printf_begin();


  Serial.println(version);


  radio.begin();

  //Reading each pin to stablish the terminal number and the listening channel

  pinMode(3, INPUT_PULLUP);   //Least significant bit
  pinMode(4, INPUT_PULLUP);
  pinMode(5, INPUT_PULLUP);
  pinMode(6, INPUT_PULLUP);
  
  //¡¡¡¡Atention!!!!, the first version of the hardware the pin7 is associated to the buzzer.
  //Remember to change comment/uncomment depending on the hardware version
//  pinMode(7, INPUT_PULLUP);   //Most significant bit

  for (int pin = 6; pin >= 3; pin--)
  {
    sample.termNum = sample.termNum * 2; //Each bit will be multiplied by 2 as much times as his significance
    if (!digitalRead(pin))
    {
      sample.termNum++;
    }
  }

  Serial.print("termNum: ");
  Serial.println(sample.termNum);

  //maximum 125 channels. cell phone and wifi uses 2402-2472. Free from channel 73 to channel 125. Each channels is 1Mhz separated
  radio.setChannel(baseChannel + sample.termNum);
  Serial.println(baseChannel + sample.termNum);

  radio.openWritingPipe(pipes[1]);
  radio.openReadingPipe(1, pipes[0]);
  //radio.enableDynamicAck();
  radio.startListening();
  printf(" Status Radio\n\r");
  radio.printDetails();
  Serial.print("Channel set to: ");
  Serial.println(radio.getChannel());
  Serial.flush(); //Flushing the buffer serial buffer to avoid spurious data.

  //Activate interruption service each time the sensor changes state
  attachInterrupt(digitalPinToInterrupt(2), controlint, CHANGE);

  pinMode(A3, OUTPUT);    //Blue
  pinMode(A4, OUTPUT);    //Green
  pinMode(A5, OUTPUT);    //Red
  pinMode(7, OUTPUT);     //Buzzer
  pinMode(2, INPUT_PULLUP); //Sensor

  red_off;
  green_off;
  blue_off;

  noInterrupts();   //Don't watch the sensor state
}


void loop(void)
{


  if (flagint == HIGH ) //The sensor has changed

  {
    sample.elapsedTime = (millis() - time0);
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
    radio.setChannel(125);
//    Serial.print("getChannel = ");
//    Serial.println(radio.getChannel());
    delay(10);
    bool en = radio.write( &sample, sample_size);
    if (en) {
      Serial.println("Sent OK");
    } else {
      Serial.println("Error sending");
    }
//    Serial.print("getChannel = ");
//    Serial.println(radio.getChannel());
    buzzer_on;
    delay(25);
    buzzer_off;
    flagint = LOW;
    waitingSensor = false;
    radio.setChannel(baseChannel + sample.termNum);
    radio.startListening();
//    Serial.println("startListening()");
//    Serial.print("getChannel = ");
//    Serial.println(radio.getChannel());
    delay(10);

  }

  while (radio.available())
  {
    radio.read(  &instruction, size_instruction);
    Serial.print("Command received: ");
    Serial.println(instruction.command);
    if (instruction.termNum == sample.termNum)
    {
      executeCommand(instruction.command);
    }
  }
}

void controlint()
{
  if (waitingSensor == true) {
    flagint = HIGH;
    sample.state = digitalRead(2);
    
  }
}

void executeCommand(byte command)
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
//      Serial.println("activating RED");
      red_on;
    }

    if ((command & green) == green) {
      Serial.println("activating GREEN");
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

    if ((command & sensor) == sensor) {
//      Serial.println("activating sensor");
      time0 = millis(); //empieza a contar time
      waitingSensor = true;  //Terminal set to waiting touch/proximity
      interrupts();
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
  } else if (commandString == "get_version"){
    Serial.println(version);
  }
}
