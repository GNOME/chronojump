/*
  Example using the SparkFun HX711 breakout board with a scale
  By: Nathan Seidle
  SparkFun Electronics
  Date: November 19th, 2014
  License: This code is public domain but you buy me a beer if you use this and we meet someday (Beerware license).

  This is the calibration sketch. Use it to determine the calibration_factor that the main example uses. It also
  outputs the zero_factor useful for projects that have a permanent mass on the scale in between power cycles.

  Setup your scale and start the sketch WITHOUT a weight on the scale
  Once readings are displayed place the weight on the scale
  Press +/- or a/z to adjust the calibration_factor until the output readings match the known weight
  Use this calibration_factor on the example sketch

  This example assumes pounds (lbs). If you prefer kilograms, change the Serial.print(" lbs"); line to kg. The
  calibration factor will be significantly different but it will be linearly related to lbs (1 lbs = 0.453592 kg).

  Your calibration factor may be very positive or very negative. It all depends on the setup of your scale system
  and the direction the sensors deflect from zero state
  This example code uses bogde's excellent library: https://github.com/bogde/HX711
  bogde's library is released under a GNU GENERAL PUBLIC LICENSE
  Arduino pin
  2 -> HX711 CLK
  3 -> DOUT
  5V -> VCC
  GND -> GND

  Most any pin on the Arduino Uno will be compatible with DOUT/CLK.

  The HX711 board can be powered from 2.7V to 5V so the Arduino 5V power should be fine.

*/

#include "HX711.h"

#define DOUT  3
#define CLK  2

//Version number
String version = "Force_Sensor-0.1";

HX711 scale(DOUT, CLK);

//Data comming from the cell after resting the offset weight
float offsetted_data = 0;

//Data resulting of appying the calibration_factor to the offsetted_data
float scaled_data = 0;

//The weight used to calibrate the cell
float weight = 0.0;

//Wether the sensor has to capture or not
boolean capturing = false;

unsigned long lastTime = 0;
unsigned long currentTime = 0;

void setup() {
  Serial.begin(115200);

  
  //The factor to convert the units coming from the cell to the units used in the calibration
  scale.set_scale(915.0); // Usual value  in Chronojump strength gauge

}

void loop() {

  unsigned long elapsedTime = 0;
  if (capturing)
  {
    currentTime = micros();

    //Managing the timer overflow
    if (currentTime > lastTime)      //No overflow
    {
      elapsedTime = currentTime - lastTime;
    } else  if (currentTime <= lastTime)  //Overflow
    {
      elapsedTime = (4294967295 - lastTime) + currentTime; //Time from the last measure to the overflow event plus the currentTime
    }

    Serial.print(elapsedTime);
    Serial.print(";");
    Serial.println(scale.get_units(), 1); //scale.get_units() returns a float
  }

}

void serialEvent()
{
  String inputString = Serial.readString();
  String commandString = inputString.substring(0, inputString.lastIndexOf(":"));
//  while (Serial.available())
//  {
//    char inChar = (char)Serial.read();
//    inputString += inChar;
//    if (inChar == '\n') {
//       commandString = inputString.substring(0, inputString.lastIndexOf(":"));
//    }

      if (commandString == "start_capture") {
        start_capture();
      } else if (commandString == "end_capture"){
        end_capture();
      } else if (commandString == "get_version"){
        get_version();
      } else if (commandString == "get_calibration_factor"){
        get_calibration_factor();
      } else if (commandString == "set_calibration_factor"){
        set_calibration_factor(inputString);
      } else if (commandString == "calibrate"){
        calibrate(inputString);
      } else if (commandString == "get_tare"){
        get_tare();
      } else if (commandString == "set_tare"){
        set_tare(inputString);
      } else if (commandString == "tare"){
        tare();
      } else {
        Serial.println("Not a valid command");
      }
      inputString = "";
//  }
}

void start_capture()
{
  Serial.println("Starting capture...");
  capturing = true;
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

void get_calibration_factor()
{
  Serial.println(scale.get_scale());
}

void set_calibration_factor(String inputString)
{
  //Reading the argument of the command. Located within the ":" and the ";"
  String calibration_factor = get_command_argument(inputString);
  //Serial.println(calibration_factor.toFloat());
  scale.set_scale(calibration_factor.toFloat());
  Serial.println("Calibration factor set");
}

void calibrate(String inputString)
{
  //Reading the argument of the command. Located within the ":" and the ";"
  String weightString = get_command_argument(inputString);
  float weight = weightString.toFloat();
  //mean of 255 values comming from the cell after resting the offset.
  double offsetted_data = scale.get_value(255);

  //offsetted_data / calibration_factor
  float calibration_factor = offsetted_data / weight / 9.81; //We want to return Newtons.
  scale.set_scale(calibration_factor);
  Serial.println("Calibrating OK");
}

void tare()
{
  scale.tare(255); //Reset the scale to 0 using the mean of 255 raw values
  Serial.println("Taring OK");
}

void get_tare()
{
  Serial.println(scale.get_offset());
}

void set_tare(String inputString)
{
  String tare = get_command_argument(inputString);
  long value = tare.toInt();
  scale.set_offset(value);
  Serial.println("Tare set");
}

String get_command_argument(String inputString)
{
  return(inputString.substring(inputString.lastIndexOf(":") + 1, inputString.lastIndexOf(";")));
}

