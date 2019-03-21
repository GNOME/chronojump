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
unsigned long lastSampleTime = micros();
unsigned long sampleTime = 0;

//Version of the firmware
String version = "Race_Analyzer-0.1";

int pps = 40; //Pulses Per Sample. How many pulses are needed to get a sample
int ppsAddress = 0; //Where is stored the pps value in the EEPROM

int offset = 14230;
int offsetAddress = 2;
float calibrationFactor = 0.140142;
int calibrationAddress = 4;

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
  //double total = 0;
  long int total = 0;
  int nReadings = 0;
  int offsettedData = 0;
  float force = 0;


  if (capturing)
  {

    //With a diameter is of 160mm, each pulse is 2.513274mm. 4 pulses equals 1.00531cm
    while (!processSample) {
      offsettedData = readOffsettedData(0);
      //Serial.println(offsettedData);
      total +=  offsettedData;
      //force = readOffsettedData(0);
      //total += force;
      nReadings++;
      if (Serial.available() > 0) {
        changingTime = micros();
        lastEncoderDisplacement = encoderDisplacement;
        encoderDisplacement = 0;
        processSample = true;
      }
    }

    //int lastEncoderDisplacement = encoderDisplacement; //Assigned to another variable for in the case that encoder displacement changes before printing it

    //Managing the timer overflow
    if (sampleTime > lastSampleTime)      //No overflow
    {
      elapsedTime = sampleTime -  lastSampleTime;
    } else  if (sampleTime <=  lastSampleTime)  //Overflow
    {
      elapsedTime = (4294967295 -  lastSampleTime) + sampleTime; //Time from the last measure to the overflow event plus the changingTime
    }
    totalTime += elapsedTime;
    double meanForce = total / nReadings;
    lastSampleTime = sampleTime;

    //Sending in text mode
    Serial.print(lastEncoderDisplacement);
    Serial.print(";");
    Serial.print(totalTime);
    Serial.print(";");
    Serial.println(total / nReadings);

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
  if (abs(encoderDisplacement) >= pps) {
    lastEncoderDisplacement = encoderDisplacement;  //We need to save this value because it can change very quickly
    sampleTime = changingTime;                      //We need to save this value because it can change very quickly
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
  } else if (commandString == "get_transmission_format") {
    get_transmission_format();
  } else if (commandString == "get_calibration_factor") {
    get_calibration_factor();
  } else if (commandString == "set_calibration_factor") {
    set_calibration_factor(inputString);
  } else if (commandString == "calibrate") {
    calibrate(inputString);
  } else if (commandString == "get_offset") {
    get_offset();
  } else if (commandString == "set_offset") {
    set_offset(inputString);
  } else {
    Serial.println("Not a valid command");
  }
  inputString = "";
}

void start_capture()
{
  Serial.println("Starting capture...");

  totalTime = 0;
  lastSampleTime = micros();
  sampleTime = lastSampleTime +1;

  //First sample with a low speed is mandatory to good detection of the start
  Serial.print(0);
  Serial.print(";");
  Serial.print(1);
  Serial.print(";");
  Serial.println(0);
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
  long int total = 0;
  for (int i = 1; i <= 100;  i++)
  {
    total += loadCell.readADC_SingleEnded(0);
  }

  offset = total / 100;
  EEPROM.put(offsetAddress, offset);
  Serial.println("Taring OK");
}

int readOffsettedData(int sensor)
{
  return (loadCell.readADC_SingleEnded(sensor) - offset);
}

void calibrate(String inputString)
{
  //Reading the argument of the command. Located within the ":" and the ";"
  String loadString = get_command_argument(inputString);
  float load = loadString.toFloat();
  float calibrationFactor = 0;
  float total = 0;
  for (int i = 1; i <= 1000;  i++)
  {
    total += readOffsettedData(0);
  }

  calibrationFactor = load * 9.81 / (total / 1000.0);
  EEPROM.put(calibrationAddress, calibrationFactor);
  Serial.println("Calibrating OK");
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

void set_calibration_factor(String inputString)
{
  //Reading the argument of the command. Located within the ":" and the ";"
  String calibration_factor = get_command_argument(inputString);
  //Serial.println(calibration_factor.toFloat());
  calibrationFactor = calibration_factor.toFloat();
  float stored_calibration = 0.0f;
  EEPROM.get(calibrationAddress, stored_calibration);
  if (stored_calibration != calibrationFactor) {
    EEPROM.put(calibrationAddress, calibrationFactor);
  }
  Serial.println("Calibration factor set");
}

void set_offset(String inputString)
{
  String offsetString = get_command_argument(inputString);
  long value = offsetString.toInt();
  offset = value;
  int stored_offset = 0;
  EEPROM.get(offsetAddress, stored_offset);
  if (stored_offset != value) {
    EEPROM.put(offsetAddress, value);
    Serial.println("updated");
  }
  Serial.println("offset set");
}

void get_offset(void)
{
  Serial.println(offset);
}

void get_calibration_factor(void)
{
  Serial.println(calibrationFactor);
}
