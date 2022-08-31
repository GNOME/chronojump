void addJump(String row)
{
  int prevComaIndex = row.indexOf(":");
  int nextComaIndex = row.indexOf(",");
  //totalJumpTypes = row.substring(prevComaIndex + 1, nextComaIndex).toInt();
  jumpTypes[totalJumpTypes].id = row.substring(prevComaIndex + 1, nextComaIndex).toInt();

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[totalJumpTypes].name = row.substring(prevComaIndex + 1 , nextComaIndex);
  prevComaIndex = nextComaIndex;

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[totalJumpTypes].jumpLimit = row.substring(prevComaIndex + 1, nextComaIndex).toInt();
  prevComaIndex = nextComaIndex;

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[totalJumpTypes].timeLimit = row.substring(prevComaIndex + 1, nextComaIndex).toFloat();
  prevComaIndex = nextComaIndex;

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[totalJumpTypes].hardTimeLimit = (row.substring(prevComaIndex + 1 , nextComaIndex) == 1);
  prevComaIndex = nextComaIndex;

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[totalJumpTypes].percentBodyWeight = row.substring(prevComaIndex + 1 , nextComaIndex).toFloat();
  prevComaIndex = nextComaIndex;

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[totalJumpTypes].fall = row.substring(prevComaIndex + 1, nextComaIndex).toFloat();
  prevComaIndex = nextComaIndex;

  prevComaIndex = nextComaIndex;
  jumpTypes[totalJumpTypes].startIn = (row.substring(prevComaIndex + 1, prevComaIndex + 2) == "1");
  totalJumpTypes++;
  //Serial.println("totalJumpTypes: " + String(totalJumpTypes));
}

void saveJumpsList()
{
  SD.remove("JUMPTYPE.TXT");
 
  File jumpFile = SD.open("JUMPTYPE.TXT", FILE_WRITE);

//  if(jumpFile) Serial.println("File created");
//  else Serial.println("Error creating file");

  for (unsigned int i = 0; i < totalJumpTypes; i++)
  {
    jumpFile.print(jumpTypes[i].id);
    jumpFile.print("," + String(jumpTypes[i].name));
    jumpFile.print("," + String(jumpTypes[i].jumpLimit));
    jumpFile.print("," + String(jumpTypes[i].timeLimit));
    jumpFile.print("," + String(jumpTypes[i].hardTimeLimit));
    jumpFile.print("," + String(jumpTypes[i].percentBodyWeight));
    jumpFile.print("," + String(jumpTypes[i].fall));
    jumpFile.println("," + String(jumpTypes[i].startIn));
  }
  jumpFile.close();
  Serial.println("Saved " + String(totalJumpTypes) + " to JUMPTYPE.TXT");
}

void addGravitatory(String row)
{
  int prevComaIndex = row.indexOf(":");
  int nextComaIndex = row.indexOf(",");
  //totalGravTypes = row.substring(prevComaIndex + 1, nextComaIndex).toInt();
  gravTypes[totalGravTypes].id = row.substring(prevComaIndex + 1, nextComaIndex).toInt();

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  gravTypes[totalGravTypes].name = row.substring(prevComaIndex + 1 , nextComaIndex);
  prevComaIndex = nextComaIndex;

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  gravTypes[totalGravTypes].description = row.substring(prevComaIndex + 1, nextComaIndex);
  prevComaIndex = nextComaIndex;

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  gravTypes[totalGravTypes].percentBodyWeight = row.substring(prevComaIndex + 1 , nextComaIndex).toFloat();
  prevComaIndex = nextComaIndex;

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  gravTypes[totalGravTypes].speed1Rm = row.substring(prevComaIndex + 1, nextComaIndex).toFloat();
  totalGravTypes++;
}

void saveGravitatoryList()
{
  SD.remove("GRAVTYPE.TXT");
 
  File gravFile = SD.open("GRAVTYPE.TXT", FILE_WRITE);

//  if(gravFile) Serial.println("File created");
//  else Serial.println("Error creating file");

  for (unsigned int i = 0; i < totalGravTypes; i++)
  {
    gravFile.print(jumpTypes[i].id);
    gravFile.print("," + String(gravTypes[i].name));
    gravFile.print("," + gravTypes[i].description );
    gravFile.print("," + String(gravTypes[i].percentBodyWeight));
    gravFile.println("," + String(gravTypes[i].speed1Rm));
  }
  gravFile.close();
  Serial.println("Saved " + String(totalGravTypes) + " to GRAVTYPE.TXT");
}

void readExercisesFile(String parameters){
  parameters = parameters.substring(0, parameters.lastIndexOf(";"));
  if ( parameters == "jumps" ) readExercisesFile(jumps);
  else if ( parameters == "gravitatory" ) readExercisesFile(gravitatory);
  else Serial.print("Not a valid parameter");
}
void readExercisesFile(exerciseType mode)
{
  char readChar;
  String readString = "";
  unsigned long pos = 0;    //Position in the file
  int numRows = 0;          //Number of valid rows in the file
  String file = "";

  if (mode == jumps) {
    //Serial.println("J");
    file = "JUMPTYPE.TXT";
  }
  else if (mode == gravitatory) {
    //Serial.println("G");
    file = "GRAVTYPE.TXT";
  }

  File  exercisesFile = SD.open(file);

  if (exercisesFile)
  {
    //Serial.println("File size = " + String(exercisesFile.size() ) );
    while (pos <= exercisesFile.size())
    {
      readChar = '0';
      String readString = "";
      while (readChar != '\n' && readChar != '\r' && pos <= exercisesFile.size())
      {
        readChar = exercisesFile.read();
        readString = readString + readChar;
        pos++;
      }
      
      //Serial.print(readString);

      //Check that it is a valid row.
      if ( isDigit(readString[0]) )
      {
        numRows++;
        currentExerciseType = numRows - 1;

        if (mode == jumps) {
          addJump(readString);
          totalJumpTypes = numRows;
        } else if (mode == gravitatory) {
          addGravitatory(readString);
          totalGravTypes = numRows;
        }
      }
    }
    Serial.println("Total:" + String(numRows));
  }
  exercisesFile.close();
}

void printJumpTypesList()
{
  Serial.println("id, name, jumpLimit,timeLimit, hardTimeLimit, percentBodyWeight, fall, startIn");
  for (unsigned int i = 0; i < totalJumpTypes; i++)
  {
    Serial.print(jumpTypes[i].id);
    Serial.print("," + jumpTypes[i].name + ", ");
    Serial.print(String( jumpTypes[i].jumpLimit) + "j, ");
    Serial.print(String( jumpTypes[i].timeLimit ) + "s, ");
    if(jumpTypes[i].hardTimeLimit) Serial.print("Yes, ");
    else Serial.print("No, ");
    Serial.print(String( jumpTypes[i].percentBodyWeight , 2) + "%, ");
    Serial.print(String( jumpTypes[i].fall , 2) + "cm, ");
    if(jumpTypes[i].startIn) Serial.println("Yes");
    else Serial.println("No");
  }
}

void printGravTypesList()
{
  Serial.println("id, name, description, percentBodyWeight, speed1RM");
  for (unsigned int i = 0; i < totalGravTypes; i++)
  {
    Serial.print(String(gravTypes[i].id) + ", ");
    Serial.print(gravTypes[i].name + ", ");
    Serial.print(gravTypes[i].description + ", ");
    Serial.print(String( gravTypes[i].percentBodyWeight , 2) + "%, ");
    Serial.println(String( gravTypes[i].speed1Rm , 2) + "m/s");
  }
}
