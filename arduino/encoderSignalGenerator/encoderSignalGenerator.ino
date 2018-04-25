/*
  signalGenerator: Generates a digital signal following different sequences defined in sequences[]

  Copyright (C) 2018 Xavier de Blas xaviblas@gmail.com
  Copyright (C) 2018 Xavier Padullés support@chronojump.org

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

#define   photocellA  3
#define   photocellB  2
#define   ledPin      13

void setup() {
  pinMode(photocellA, OUTPUT);
  pinMode(photocellB, OUTPUT);
  Serial.begin(115200);
  randomSeed(analogRead(0));
  digitalWrite(photocellA, HIGH);
  digitalWrite(photocellB, HIGH);
}

void loop() {
    float repRange = (int) random(10, 1000);
    concentric(repRange,(int) random(200, 3000));
    delay(1);
    eccentric(repRange,(int) random(200, 3000));
    delay(1);
    if(random(100) > 95){
      delay(10000);
    }
}

//range in mm, time of the phase in ms
void concentric(int repRange, int repTime) {
  for (int i = 0; i < repRange; i++) {
    encoderForward();
    delayMicroseconds((repTime / repRange) * 1000);
  }
}

void eccentric(int repRange, int repTime) {
  for (int i = 0; i < repRange; i++) {
    encoderBackward();
    delayMicroseconds((repTime / repRange) * 1000);
  }
}

void encoderBackward(void)
{
  digitalWrite(photocellB, LOW);
  delayMicroseconds(10);
  digitalWrite(photocellA, HIGH);
  delayMicroseconds(90);
  digitalWrite(photocellA, LOW);
}

void encoderForward(void)
{
  digitalWrite(photocellB, HIGH);
  delayMicroseconds(10);
  digitalWrite(photocellA, HIGH);
  delayMicroseconds(90);
  digitalWrite(photocellA, LOW);
}

//
//// https://stackoverflow.com/questions/9072320/split-string-into-string-array
//String getValue(String data, char separator, int index)
//{
//  int found = 0;
//  int strIndex[] = {
//    0, -1
//  };
//  int maxIndex = data.length() - 1;
//
//  for (int i = 0; i <= maxIndex && found <= index; i++) {
//    if (data.charAt(i) == separator || i == maxIndex) {
//      found++;
//      strIndex[0] = strIndex[1] + 1;
//      strIndex[1] = (i == maxIndex) ? i + 1 : i;
//    }
//  }
//
//  return found > index ? data.substring(strIndex[0], strIndex[1]) : "";
//}
//
//void serialEvent()
//{
//  String inputString = Serial.readString();
//  inputString = inputString.substring(0, inputString.lastIndexOf(";"));
//  if (inputString == "start")
//  {
//    Serial.println("Starting signal");
//    generateSignal = true;
//  } else if (inputString == "stop")
//  {
//    generateSignal = false;
//    Serial.println("Stoping signal");
//  } else
//  {
//    Serial.println("mode = " + inputString);
//    mode = inputString.toInt();
//  }
//
//}

