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

HX711 scale(DOUT, CLK);
//The factor to convert the units coming from the cell to the units used in the calibration
float calibration_factor = 915; // Usual value With Chronojump strength gauge
//float raw_data = 0;

//Data comming from the cell after resting the offset weight
float offsetted_data = 0;

//Data resulting of appying the calibration_factor to the offsetted_data
float scaled_data = 0;

//The weight used to calibrate the cell
float weight = 0.0;

void setup() {
  Serial.begin(115200);
  Serial.println("HX711 calibration sketch");
  Serial.println("Remove all weight from scale and press Enter");
  while(Serial.available() == 0){}
  Serial.read();
  Serial.println("Adjusting tare...");
  scale.tare(255); //Reset the scale to 0
  
  Serial.println("Enter the weight you will use");
  while(!Serial.available()){}
  weight = Serial.parseFloat();
  
  Serial.println("Put the weight on the scale and press Enter");

  Serial.flush();
  Serial.read();
  while(!Serial.available()){}
  Serial.println("Calibrating...");

  //mean of 255 values comming from the cell after resting the offset.
  offsetted_data = scale.get_value(255);

  //offsetted_data / calibration_factor
  calibration_factor = offsetted_data / weight / 9.81; //We want to return Newtons.
  scale.set_scale(calibration_factor);

  //mean of 255 values after being scaled to the desired units
  scaled_data = scale.get_units(255);
  Serial.print("Calibration factor:");
  Serial.print("\t");
  Serial.println(calibration_factor);
}

void loop() {

//  //Raw data
//  raw_data = scale.read_average(100);
//  Serial.print(raw_data);
//  Serial.print("\t");
//

}
