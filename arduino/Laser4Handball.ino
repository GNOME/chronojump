/*
Laser4Handball: Measure handball kicking on a goal by lasers (using Arduino and leds).

Copyright (C) 2017 Xavier de Blas xaviblas@gmail.com
Copyright (C) 2017 Xavier Padullés support@chronojump.org
Copyright (C) 2017 Victor Tremps victortremps@gmail.com

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

/*
This is an implementation of a goal where the player sees three lights on at corners and have to throw to opposite corner.
Detection of the ball is done by four lasers.
When player enters on a platform, three lights are opened at the same time, but the delay between entering and opening of lights is random.
Player has a limited time to throw.
Present program is not good for measure ball speed because contact time with the laser can be on the middle of the ball or at the side, so laser "contact" time is not an accurate measure of ball speed.
Present program can be good for training and for motivation. Implementation have been tested on handball.
This program is not ended because there's a need of a better detectZone after the lasersSpy.
*/

int pinPlatform = 2;
int pinLaser[4];
int pinLed[4];

//unsigned int num;
unsigned int timeStart[4];

//
bool exists(unsigned int num)
{
  for (int i = 0; i < 4; i ++) {
    if (num == timeStart[i]) {
      return true;
    }
  }
  return false;
}

void createOrder()
{
  unsigned int num;

  //default value at timeStart:
  for (int i = 0; i < 4; i ++) {
    timeStart[i] = 10;
  }

  //assign a random value
  for (int i = 0; i < 4; i ++) {
    do {
      num = random(0, 4);
    }
    while (exists(num) == true);

    timeStart[i] = num;
  }

  Serial.println("Wait seconds");
  for (int i = 0; i < 4; i ++) {
    Serial.print(timeStart[i]);
  }
  Serial.println("------------");
}


void setup()
{
  pinMode(A0, INPUT);
  Serial.begin(9600);

  //is different every time
  //reading from a inexistent analogport
  randomSeed(analogRead(0));

  pinLaser[0] = 5;
  pinLaser[1] = 6;
  pinLaser[2] = 7;
  pinLaser[3] = 8;

  pinLed[0] = 9;
  pinLed[1] = 10;
  pinLed[2] = 11;
  pinLed[3] = 12;

  pinMode(pinPlatform, INPUT);
  for (int i = 0; i < 4; i ++) {
    pinMode(pinLaser[i], INPUT);
    pinMode(pinLed[i], OUTPUT);
  }

  //turnoof lights
  offLeds();
}

//detect cut zone of the ball
int detectZone()
{
  boolean l0 = false;
  boolean l1 = false;
  boolean l2 = false;
  boolean l3 = false;

  if (digitalRead(pinLaser[0]) == HIGH) {
    l0 = true;
  }
  if (digitalRead(pinLaser[1]) == HIGH) {
    l1 = true;
  }
  if (digitalRead(pinLaser[2]) == HIGH) {
    l2 = true;
  }
  if (digitalRead(pinLaser[3]) == HIGH) {
    l3 = true;
  }
  /*
  visual layout of the zones
   l: (L)aser; m: llu(m)
        l2     l3
    -----------------
    |m1|         |m2|
  l1|  |3   4   5|  |
    |  |         |  |
    |  |2       6|  |
    |  |         |  |
  l0|m0|1   8   7|m3|
  
  0 means: no cut
  -1 means: error, more than two lasers cut
  */

  if     (  l0 && ! l1 &&   l2 && ! l3)
    return 1;
  else if (! l0 && ! l1 &&   l2 && ! l3)
    return 2;
  else if (! l0 &&   l1 &&   l2 && ! l3)
    return 3;
  else if (! l0 &&   l1 && ! l2 && ! l3)
    return 4;
  else if (! l0 &&   l1 && ! l2 &&   l3)
    return 5;
  else if (! l0 && ! l1 && ! l2 &&   l3)
    return 6;
  else if (  l0 && ! l1 && ! l2 &&   l3)
    return 7;
  else if (  l0 && ! l1 && ! l2 && ! l3)
    return 8;
  else if (! l0 && ! l1 && ! l2 && ! l3)
    return 0;

  return -1;
}

/*
light is the corner (0-3)
z1, z2 are the cutzones: zone1 i 2
if the ball cut one laser it will be 1 poitn, 
if the ball cut two lasers it will be 2 point
it prints the points
*/
void printPoints(unsigned int led, int z1, int z2)
{
  unsigned int points = 0;

  if (led == 0)
  {
    if (z1 == 1 || z2 == 1 ) {
      points = 2;
    } else if (z1 == 2 || z1 == 8 || z2 == 2 || z2 == 8) {
      points = 1;
    }
  }
  else if (led == 1)
  {
    if (z1 == 3 || z2 == 3 ) {
      points = 2;
    } else if (z1 == 2 || z1 == 4 || z2 == 2 || z2 == 4) {
      points = 1;
    }
  }
  else if (led == 2)
  {
    if (z1 == 5 || z2 == 5 ) {
      points = 2;
    } else if (z1 == 4 || z1 == 6 || z2 == 4 || z2 == 6) {
      points = 1;
    }
  }
  else if (led == 3)
  {
    if (z1 == 7 || z2 == 7 ) {
      points = 2;
    } else if (z1 == 6 || z1 == 8 || z2 == 6 || z2 == 8) {
      points = 1;
    }
  }

  if (points == 2) {
    Serial.println("Good! 2 points!!!");
  } else if ( points == 1) {
    Serial.println("1 point!");
  } else {
    Serial.println("0 points");
  }
}

/*
we want to know what are the lasers reading
we spy the lasers during 1000 ms 
rates e0,1,2,3 are the status of each laser
*/
int lasersSpy()
{
  //declaring rates
  unsigned long spyTimeStart;
  unsigned long timeNow;

  boolean e[4];
  for (int i = 0; i < 4; i ++) {
    e[i] = false;
  }

  for (int i = 0; i < 4; i ++) {
    if (digitalRead(pinLaser[i]) == HIGH) {
      e[i] = true;
    }
  }

  /*
  when it is produced the firts the lasersSpy() start.If there ara some on/off the lasersSpy() will said.
  therefore lasersSpy() prnt changes when the first laser is on, and will be working 1000ms (at least one off will be detected)
  */
  spyTimeStart = millis();
  timeNow = millis();

  while (timeNow - spyTimeStart < 1000)
  {
    for (int i = 0; i < 4; i ++) {
      if (digitalRead(pinLaser[i]) != e[i]) {
        e[i] = !e[i];
        spyPrint(i, timeNow - spyTimeStart, e[i]);
      }
    }
    timeNow = millis();
  }
  Serial.println("Spy ends");
}
//Print the information of espialaser (laser/time/Status(on/off)
void spyPrint(int laserNum, unsigned long time, boolean newStatus)
{
    Serial.print("Laser / ms / new status: ");
    Serial.print(laserNum);
    Serial.print(" / ");
    Serial.print(time);
    Serial.print(" / ");
    Serial.println(newStatus);
}

//turn on a random light
//led on is LOW
unsigned int OpenThreeLights()
{
  unsigned int num = random(0, 4); //between 0 and 3 (3 included)

  for (int i = 0; i < 4; i ++) {
	  if(num == i) {
		  digitalWrite(pinLed[i], HIGH);
	  } else {
		  digitalWrite(pinLed[i], LOW);
	  }
  }

  return num;
}

void offLeds()
{
  for (int i = 0; i < 4; i ++) {
    digitalWrite(pinLed[i], HIGH);
  }
  Serial.println("Lights off!");
  delay(200);
}

//it starts generating the random order for the time that lights will be turn on
void loop()
{
  createOrder();

  for (int i = 0; i < 4; i ++)
  {
    Serial.print("\n----- Try: ");
    Serial.println(i);

    //if the platform is presses, set free
    if (digitalRead(pinPlatform) == HIGH) {
      Serial.println("\nExit the platform: ");
      while (digitalRead(pinPlatform) == HIGH) {
        //waiting player exits
        offLeds();
        delay(1000);
      }
    }

    //Now player can start
    Serial.println("\nEnter the platform: ");
    //while platform is not pressed... (LOW means 'not pressed')
    while (digitalRead(pinPlatform) == LOW) {
      //waiting player enters
    }

    //wait a while (definite in timeStart)
    Serial.println("Wait... these milliseconds:");
    Serial.println(timeStart[i] * 1000);
    delay(timeStart[i] * 1000);

    //turn on three lights
    Serial.println("Three ligths on!");
    unsigned int threeLedsOnTime = OpenThreeLights();
    unsigned long timeStartLeds = millis();

    //failOrLate is when the ball doesn't touch any laser or laser is LOW
    boolean failOrLate = false;

    int zone1 = 0;
    //while the laser is LOW (without contact: LOW)
    while (zone1 <= 0 && failOrLate == false)
    {
      zone1 = detectZone();
      //waiting for be pressed

      unsigned long timeNow = millis();
      if (timeNow - timeStartLeds > 3000) {
        Serial.print(" !!dins fallat o tard!!, timeNow: ");
        Serial.print(timeNow);
        Serial.print("; timeStartLeds: ");
        Serial.print(timeStartLeds);
        failOrLate = true;
      }

    }

    if (failOrLate) {
      Serial.println("FALLAT o TARD!");
      //activate pinLaserDetect to not have a problems at next iteration

      for (int i = 0; i < 4; i ++) {
	      digitalWrite(pinLaser[i], HIGH);
      }
    }
    //print the results (reactiontime=temprreaccio, Points)
    else {
      unsigned long reactionTime = millis() - timeStartLeds;
      Serial.print("Reaction time: ");
      Serial.println(reactionTime);

      //delay(10);
      Serial.print("1st Detected: ");
      Serial.println(zone1);
      //Serial.print("2nd Detected: ");
      //Serial.println(zone2);

      //printPoints(threeLedsOnTime, zone1, zone2);
      lasersSpy();
    }

    delay(50);

    offLeds();
  }
}
