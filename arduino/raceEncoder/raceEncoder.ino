#include <EEPROM.h>

int encoderPinA = 2;
int encoderPinB = 3;
volatile int encoderDisplacement = 0;
volatile unsigned long changingTime = 0;
unsigned long elapsedTime = 0;
unsigned long totalTime = 0;
unsigned long lastTime = 0;
//int position = 0;

//Version of the firmware
String version = "Race_Encoder-0.1";

int pps; //Pulses Per Sample. How many pulses are needed to get a sample
int ppsAddress = 0; //Where is stored the pps value in the EEPROM

//Wether the sensor has to capture or not
boolean capturing = false;

void setup() {
  pinMode (encoderPinA, INPUT);
  pinMode (encoderPinB, INPUT);
  Serial.begin (115200);

  EEPROM.get(ppsAddress, pps);

  //Using the rising flank of the A photocell we have a 200 PPR.
  attachInterrupt(digitalPinToInterrupt(encoderPinA), changingA, RISING);

  //Using the CHANGE with both photocells WE CAN HAVE 800 PPR
  //attachInterrupt(digitalPinToInterrupt(encoderPinB), changingB, CHANGE);
}

void loop() {
  if (capturing)
  {
    //With a diameter is of 160mm, each pulse is 2.513274mm. 4 pulses equals 1.00531cm
    if ( abs(encoderDisplacement) >= pps ) {

      int lastEncoderDisplacement = encoderDisplacement; //Assigned to another variable for in the case that encoder displacement changes before printing it
      unsigned long Time = changingTime;
      encoderDisplacement = 0;

      //Managing the timer overflow
      if (Time > lastTime)      //No overflow
      {
        elapsedTime = Time - lastTime;
      } else  if (Time <= lastTime)  //Overflow
      {
        elapsedTime = (4294967295 - lastTime) + Time; //Time from the last measure to the overflow event plus the changingTime
      }
      totalTime += elapsedTime;
      Serial.print(lastEncoderDisplacement);
      Serial.print(";");
      Serial.println(totalTime);
      lastTime = Time;
    }
    delayMicroseconds(100);
  }
}

void changingA() {
  changingTime = micros();
  if (digitalRead(encoderPinB) == HIGH) {
    encoderDisplacement--;
  } else {
    encoderDisplacement++;
  }
}

void serialEvent()
{
  String inputString = Serial.readString();
  String commandString = inputString.substring(0, inputString.lastIndexOf(":"));
  if (commandString == "start_capture") {
    start_capture();
  } else if (commandString == "end_capture") {
    end_capture();
  } else if (commandString == "get_version") {
    get_version();
  } else if (commandString == "set_pps") {
    set_pps(inputString);
  } else if (commandString == "get_pps") {
    get_pps();
  } else {
    Serial.println("Not a valid command");
  }
  inputString = "";
}

void start_capture()
{
  Serial.println("Starting capture...");
  totalTime = 0;
  lastTime = micros();
  capturing = true;
  encoderDisplacement = 0;
}

void end_capture()
{
  capturing = false;
  Serial.println("Capture ended");
}

void get_version()
{
  Serial.println(version);
}

//Setting how many pulses are needed to get a sample
void set_pps(String inputString)
{
  String argument = get_command_argument(inputString);
  pps = argument.toInt();
  EEPROM.put(ppsAddress, pps);
  Serial.print("pps set to: ");
  Serial.println(pps);
}

void get_pps()
{
  Serial.println(pps);
}

String get_command_argument(String inputString)
{
  return(inputString.substring(inputString.lastIndexOf(":") + 1, inputString.lastIndexOf(";")));
}

