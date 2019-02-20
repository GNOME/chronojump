#include <Wire.h>
#include <Adafruit_ADS1015.h>
#include <EEPROM.h>

Adafruit_ADS1115 loadCell;

int encoderPinA = 3;
int encoderPinB = 4;
volatile int encoderDisplacement = 0;
int lastEncoderDisplacement = 0;
volatile unsigned long changingTime = 0;
unsigned long elapsedTime = 0;
unsigned long totalTime = 0;
unsigned long lastTime = 0;
//int position = 0;

//Version of the firmware
String version = "Race_Analyzer-0.1";

int pps = 40; //Pulses Per Sample. How many pulses are needed to get a sample
int ppsAddress = 0; //Where is stored the pps value in the EEPROM

int offset = 0;
float calibrationFactor = 0;

//Wether the sensor has to capture or not
boolean capturing = false;

//Wether the encoder has reached the number of pulses per sample or not
boolean processSample = false;

//wether the tranmission is in binary format or not
boolean binaryFormat = false;

void setup() {
  pinMode (encoderPinA, INPUT);
  pinMode (encoderPinB, INPUT);
  Serial.begin (115200);
  //Serial.begin (1000000);
  Wire.setClock(1000000);

  loadCell.begin();
  loadCell.setGain(GAIN_ONE);
  //tare();

  //EEPROM.get(ppsAddress, pps);
  //Using the rising flank of the A photocell we have a 200 PPR.
  attachInterrupt(digitalPinToInterrupt(encoderPinA), changingA, RISING);

  //Using the CHANGE with both photocells WE CAN HAVE 800 PPR
  //attachInterrupt(digitalPinToInterrupt(encoderPinB), changingB, CHANGE);
}

void loop() {
  long int total = 0;
  int nReadings = 0;
  int offsettedData = 0;
  

  if (capturing)
  {

    //With a diameter is of 160mm, each pulse is 2.513274mm. 4 pulses equals 1.00531cm
    while (!processSample) {
      offsettedData = readOffsettedData(0);
      total += offsettedData;
      nReadings++;
    }

    unsigned long Time = changingTime;
    //int lastEncoderDisplacement = encoderDisplacement; //Assigned to another variable for in the case that encoder displacement changes before printing it

    //Managing the timer overflow
    if (Time > lastTime)      //No overflow
    {
      elapsedTime = Time - lastTime;
    } else  if (Time <= lastTime)  //Overflow
    {
      elapsedTime = (4294967295 - lastTime) + Time; //Time from the last measure to the overflow event plus the changingTime
    }
    totalTime += elapsedTime;
    int meanOffsettedData = total / nReadings;
    lastTime = Time;
    
      //Sending in text mode
    Serial.print(lastEncoderDisplacement);
    Serial.print(";");
    Serial.print(totalTime);
    Serial.print(";");
    Serial.println(offsettedData);

    processSample = false;

//    //Sending in binary mode
//    sendInt(lastEncoderDisplacement);
//    sendInt(totalTime);
//    sendInt(offsettedData);
//
//    //End of the binari sample
//    Serial.write(0xff);
//    Serial.write(0xff);
//    Serial.write(0xff);
//    Serial.write(0xff);
   
  }
}

void changingA() {
  changingTime = micros();
  if (digitalRead(encoderPinB) == HIGH) {
    encoderDisplacement--;
    //digitalWrite(13, HIGH);
  } else {
    encoderDisplacement++;
    //digitalWrite(13, LOW);
  }
  if (abs(encoderDisplacement) >= pps){
    lastEncoderDisplacement = encoderDisplacement;
    encoderDisplacement = 0;
    processSample = true;
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
  } else if (commandString == "tare") {
    tare();
  } else if (commandString == "get_transmission_format"){
        get_transmission_format();
    //  } else if (commandString == "get_calibration_factor") {
    //    get_calibration_factor();
    //  } else if (commandString == "set_calibration_factor") {
    //    set_calibration_factor(inputString);
    //  } else if (commandString == "calibrate") {
    //    calibrate(inputString);
    //  } else if (commandString == "get_tare") {
    //    get_tare();
    //  } else if (commandString == "set_tare") {
    //    set_tare(inputString);
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
  return (inputString.substring(inputString.lastIndexOf(":") + 1, inputString.lastIndexOf(";")));
}

void tare(void)
{
  int total = 0;
  for (int i = 1; i <= 100;  i++)
  {
    total += loadCell.readADC_SingleEnded(0);
  }

  offset = total / 100;
  EEPROM.put(0, offset);
}

int readOffsettedData(int sensor)
{
  return (loadCell.readADC_SingleEnded(sensor) - offset);
}

void calibrate(float load)
{
  float calibrationFactor = 0;
  float total = 0;
  for (int i = 1; i <= 1000;  i++)
  {
    total += readOffsettedData(0);
  }

  calibrationFactor = load / (total / 1000.0);
  EEPROM.put(6, calibrationFactor);
}

float readForce(int sensor)
{
  int offsettedData = readOffsettedData(0);
  float force = calibrationFactor * (float)offsettedData;
  return (force);
}

void sendInt(int i) {
  byte * b = (byte *) &i;
  Serial.write(b[0]); //Least significant byte
  Serial.write(b[1]); //Most significant byte
  Serial.flush();
  return;
}

void sendLong(long l) {
  byte * b = (byte *) &l;
  Serial.write(b[0]); //Least significant byte
  Serial.write(b[1]);
  Serial.write(b[2]);
  Serial.write(b[3]); //Most significant byte
  Serial.flush();
  return;
}


void sendFloat(float f) {
  byte * b = (byte *) &f;
  Serial.write(b[0]); //Least significant byte
  Serial.write(b[1]);
  Serial.write(b[2]);
  Serial.write(b[3]); //Most significant byte
  Serial.flush();
  return;
}

void get_transmission_format()
{
  if (binaryFormat)
  {
    Serial.println("binary");
  } else
  {
    Serial.println("text");
  }
}
