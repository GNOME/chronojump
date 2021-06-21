
#include <SPI.h>
#include <nRF24L01.h>
#include <RF24.h>
#include <printf.h>
#include <MsTimer2.h>

String version = "LightChro-Controler-1.05";
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
const byte red = 0b10000000;          // 128
const byte green = 0b01000000;        // 64
const byte blue = 0b00100000;         // 32
const byte buzzer = 0b00010000;       // 16
const byte blinkRed = 0b00001000;     // 8
const byte blinkGreen = 0b00000100;   // 4
const byte blinkBlue = 0b00000010;    // 2
const byte sensor = 0b00000001;       // 1
const byte deactivate = 0b00000000;   // 0

struct instruction_t
{
  byte command;       //The command to be executed
  short int termNum;  //The terminal number that have to execute the command
};

struct instruction_t instruction = {.command = deactivate, .termNum = 0};
int size_instruction = sizeof(instruction);

struct sample_t
{
  bool state;           //State of the sensor
  short int termNum;    //Terminal number. Configured with the switches.
  unsigned long int time;
};

struct sample_t sample = {.state = LOW, .termNum = 0, .time = 0};
int sample_size = sizeof(sample);       //num_buttom_pins es la longitud de variables a recibir .

// First channel to be used. The 5xswitches control the terminal number and the number to add the baseChannel
// The channel 125 is used to listen from the terminals. Channels 90-124 are used to send to the terminals
uint8_t baseChannel = 90;

const uint64_t pipes[2] = { 0xF0F0F0F0E1LL, 0xF0F0F0F0D2LL }; //Two radio pipes. One for emitting and the other for receiving

void setup(void)
{


  Serial.begin(115200);
  //When something arrives to the serial, how long to wait for being sure that the whole text is received
  //TODO: Try to minimize this parameter as it adds lag from instruction to sensor activation. 1 is too low.
  //Maybe increasing the baud rate we could set it to 1
  Serial.setTimeout(2);
  printf_begin();       //Needed by radio.printDetails();
  pinMode(2, OUTPUT);   //The LED is in output mode
  LED_on;  //turn off the LED

  radio.begin();

  //maximum 125 channels. cell phone and wifi uses 2402-2472. Free from channel 73 to channel 125. Each channels is 1Mhz separated
  radio.setChannel(125);
  radio.openWritingPipe(pipes[0]);
  radio.openReadingPipe(1, pipes[1]);
  radio.stopListening();
  Serial.println(version);
  Serial.println(" Status Radio");
  radio.printDetails();
  Serial.println("the instructions are [termNum]:[command];");

  Serial.println("NumTerm\tTime\tState");
  Serial.println("------------------------");
}


void loop(void)
{
  while (radio.available()) //Some terminal has sent a response
  {
    radio.read(  &sample, sample_size);

    blinkStop();
    Serial.print(sample.termNum);
    Serial.print("\t");
    Serial.print(sample.time);
    Serial.print("\t");
    Serial.println(sample.state);
    LED_off;
    delay(50);
    LED_on;
    //      }
  }
}


void serialEvent()
{

  String inputString = Serial.readString();
  //  Serial.print("Instruction received from Serial: \"");
  //  Serial.print(inputString);
  //  Serial.println("\"");
  int separatorPosition = inputString.lastIndexOf(":");

  String commandString = inputString.substring(separatorPosition + 1, inputString.lastIndexOf(";"));
  instruction.command = commandString.toInt();
  //  Serial.print("Command: ");
  //  Serial.println(instruction.command);

  String termNumString = inputString.substring(0, separatorPosition);
  //  Serial.print("termNumString:\"");
  //  Serial.print(termNumString);
  //  Serial.println("\"");

  if (termNumString == "all")  //The command is sent to all the terminals
  {
    activateAll(instruction.command);
  } else {  // if (termNumString != "all")   Command to a single terminal
    instruction.termNum = termNumString.toInt();
    //      Serial.print("instruction.termNum:\"");
    //      Serial.print(instruction.termNum);
    //      Serial.println("\"");
    sendInstruction(&instruction);
  }
  if (instruction.command & sensor) {
    blinkStart(blinkPeriod);
    blinkingLED = true;
  }
  inputString = "";
}

void sendInstruction(struct instruction_t *instruction)
{
  //  Serial.print("Sending command \'");
  //  Serial.print(instruction->command);
  //  Serial.print("\' to terminal num ");
  //  Serial.println(instruction->termNum);
  //  Serial.println(baseChannel + instruction->termNum);
  radio.setChannel(baseChannel + instruction->termNum); //Setting the channel correspondig to the terminal number

  radio.stopListening();    //To sent it is necessary to stop listening

  bool en = radio.write( instruction, size_instruction );
  if (en)  //en is 1 if radio.write went OK
  {
    //    Serial.println("Ok");
    radio.startListening();  //Going back to listening mode
  } else {
    Serial.println("Error");
  }
  radio.setChannel(125);    //setting the the channel to the reading channel
  LED_off;
  instruction->termNum = 0;
}

void activateAll(byte command)
{
  Serial.println("---------Activating All---------");
  radio.stopListening();
  for (int i = 0; i <= 31; i++) {
    radio.setChannel(baseChannel + i);
    //  Serial.print("getChannel = ");
    //  Serial.println(radio.getChannel());
    instruction.termNum = i;
    instruction.command = command;
    sendInstruction(&instruction);
  }
  radio.startListening();
  radio.setChannel(125);
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
