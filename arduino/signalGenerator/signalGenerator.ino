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

  BUGS, TODO: note when capture starts it cannot be stopped, mode changed or get_version
*/

#define   signalPin   2
#define   ledPin      13


//Version number //it always need to start with: "Sigan_Generator-"
String version = "Signal_Generator-0.1";

// -------------- CHANGE variable here ----------->
//modes:
//  -1 random sequence
//  0 read sequence 0
//  1 read sequence 1
//  ...

int mode = 3;
bool generateSignal = true;
bool debug = false;
//this can be quite real data promoting a bit double contacts
const int randomTCMin = 4;
const int randomTCMax = 500;
const int randomTFMin = 40;
const int randomTFMax = 1200;

//first num is the last element position
//2nd param is start mode. IN = ON, OUT = OFF
//values are in milliseconds
const String sequences [] = {
  "3;IN;NotSet;NotSet", // This mode is modified through serial input
  "7;IN;30;25;15;1000;25;40;19;60;24;800,30",
  "5;IN;100;1500;200;5000",
  "6;OUT;1100;40;1200;30;8000",
  "3;OUT;200;100"
};
// <---------- end of CHANGE variable here --------

void setup() {
  pinMode(signalPin, OUTPUT);
  Serial.begin(115200);
  Serial.println("Signal generator");
  Serial.println("Sequence: " + sequences[mode]);
  if (mode == -1)
  {
    randomSeed(analogRead(0));
  }
}

void loop() {
  //signalOn(500);
  //signalOff(100);

  if (generateSignal)
  {
    if (mode >= 0)
    {
      processString(mode);
    }
    else
    {
      signalOn(random(randomTCMin, randomTCMax));
      signalOff(random(randomTFMin, randomTFMax));
    }
  }
}

void processString(int n)
{
  String sequence = sequences[n]; //TODO: check n is not greater than sequences length
  int last = getValue(sequence, ';', 0).toInt();
  String currentStatus = getValue(sequence, ';', 1);
  for (int i = 2; i <= last; i++)
  {
    int duration = getValue(sequence, ';', i).toInt();

    if (currentStatus == "IN")
      signalOn(duration);
    else
      signalOff(duration);

    //invert status
    if (currentStatus == "IN")
      currentStatus = "OUT";
    else
      currentStatus = "IN";
  }
}

// https://stackoverflow.com/questions/9072320/split-string-into-string-array
String getValue(String data, char separator, int index)
{
  int found = 0;
  int strIndex[] = {
    0, -1
  };
  int maxIndex = data.length() - 1;

  for (int i = 0; i <= maxIndex && found <= index; i++) {
    if (data.charAt(i) == separator || i == maxIndex) {
      found++;
      strIndex[0] = strIndex[1] + 1;
      strIndex[1] = (i == maxIndex) ? i + 1 : i;
    }
  }

  return found > index ? data.substring(strIndex[0], strIndex[1]) : "";
}

//TC
void signalOn(int duration) {
  if (debug) {
    Serial.print("\nsignalON ");
    Serial.println(duration);
  }
  digitalWrite(signalPin, HIGH);
  digitalWrite(ledPin, HIGH);
  delay(duration);
}

//TF
void signalOff(int duration) {
  if (debug) {
    Serial.print("\nsignalOFF ");
    Serial.println(duration);
  }
  digitalWrite(signalPin, LOW);
  digitalWrite(ledPin, LOW);
  delay(duration);
}

void serialEvent()
{
  String inputString = Serial.readString();
  String commandString = inputString.substring(0, inputString.indexOf(";"));
  if (commandString == "start")
  {
    Serial.println("Starting signal");
    generateSignal = true;
  } else if (commandString == "stop")
  {
    generateSignal = false;
    Serial.println("Stoping signal");
  } else if( commandString == "get_version" )
  {
    get_version();
  } else if( commandString == "start_debug" )
  {
    debug = true;
    Serial.println("Debug ON");
  } else if( commandString == "end_debug" )
  {
    Serial.println("Debug OFF");
    debug = false;
  } else if( commandString == "get_sequence" )
  {
    Serial.println(sequences[mode]);
  } else if( commandString == "set_sequence" )
  {
    String modeString = inputString.substring( (inputString.indexOf(";") + 1) , inputString.lastIndexOf(";") );
    sequences[0] = modeString;
    mode = 0;
  } else
  {
    Serial.println("mode = " + commandString);
    mode = commandString.toInt();
  }
}


void get_version()
{
  Serial.println(version);
}
