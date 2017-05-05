/*
  Example using the SparkFun HX711 breakout board with a scale
  By: Nathan Seidle
  SparkFun Electronics
  Date: November 19th, 2014
  License: This code is public domain but you buy me a beer if you use this and we meet someday (Beerware license).

  This example demonstrates basic scale output. See the calibration sketch to get the calibration_factor for your
  specific load cell setup.

  This example code uses bogde's excellent library: https://github.com/bogde/HX711
  bogde's library is released under a GNU GENERAL PUBLIC LICENSE

  The HX711 does one thing well: read load cells. The breakout board is compatible with any wheat-stone bridge
  based load cell which should allow a user to measure everything from a few grams to tens of tons.
  Arduino pin 2 -> HX711 CLK
  3 -> DAT
  5V -> VCC
  GND -> GND

  The HX711 board can be powered from 2.7V to 5V so the Arduino 5V power should be fine.

*/

#include "HX711.h"

#define calibration_factor -920.80 //This value is obtained using the SparkFun_HX711_Calibration sketch

#define DOUT  3
#define CLK  2

HX711 scale(DOUT, CLK);

String inputString = "";         // a string to hold incoming data
boolean stringComplete = false;  // whether the string is complete
boolean capturing = false;
boolean parsedOk = false;

void setup() {
  delay(100);     //Needed to clean the garbage in the serial output
  Serial.begin(115200);

  scale.set_scale(calibration_factor); //This value is obtained by using the SparkFun_HX711_Calibration sketch
  scale.tare(); //Assuming there is no weight on the scale at start up, reset the scale to 0

  Serial.flush();
}

void loop()
{
  if (capturing)
  {
    char buffer[16];
    double time = millis() / 1000.0;
    dtostrf(time, 5, 3, buffer);
    Serial.print(buffer);
    Serial.print(";");
    Serial.println(scale.get_units(), 1); //scale.get_units() returns a float
  }
  else if (stringComplete)
  {
    Serial.print(inputString);
    inputString = "";
    stringComplete = false;
  }
}

void serialEvent()
{
  while (Serial.available()) {
    // get the new byte:
    char inChar = (char)Serial.read();
    // add it to the inputString:
    inputString += inChar;
    // if the incoming character is a newline, set a flag
    // so the main loop can do something about it:
    if (inChar == '\n') {
      stringComplete = true;
      if (inputString.startsWith("Start:"))
      {
        float f = parseInput(inputString.substring(6, inputString.lastIndexOf(":")));
        Serial.println("Parsed done" + String(f));
        Serial.println(parsedOk);

        if (parsedOk)
        {
          Serial.println("StartedOk");
          capturing = true;
        }
      }
      else if (inputString.startsWith("Stop"))
      {
        capturing = false;
      }
    }
  }
}


float parseInput(String input)
{
  Serial.println("Parsed" + input);
  if (input.length() == 0)
  {
    parsedOk = false;
    return (0);
  }

  parsedOk = true;
  return (input.toFloat());
}

