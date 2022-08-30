void addJump(String row)
{
  int prevComaIndex = row.indexOf(":");
  int nextComaIndex = row.indexOf(",");
  //currentExerciseType = row.substring(prevComaIndex + 1, nextComaIndex).toInt();
  jumpTypes[currentExerciseType].id = row.substring(prevComaIndex + 1, nextComaIndex).toInt();

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[currentExerciseType].name = row.substring(prevComaIndex + 1 , nextComaIndex);
  prevComaIndex = nextComaIndex;

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[currentExerciseType].jumpLimit = row.substring(prevComaIndex + 1, nextComaIndex).toInt();
  prevComaIndex = nextComaIndex;

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[currentExerciseType].timeLimit = row.substring(prevComaIndex + 1, nextComaIndex).toFloat();
  prevComaIndex = nextComaIndex;

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[currentExerciseType].hardTimeLimit = (row.substring(prevComaIndex + 1 , nextComaIndex) == 1);
  prevComaIndex = nextComaIndex;

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[currentExerciseType].percentBodyWeight = row.substring(prevComaIndex + 1 , nextComaIndex).toFloat();
  prevComaIndex = nextComaIndex;

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[currentExerciseType].fall = row.substring(prevComaIndex + 1, nextComaIndex).toFloat();
  prevComaIndex = nextComaIndex;

  prevComaIndex = nextComaIndex;
  jumpTypes[currentExerciseType].startIn = (row.substring(prevComaIndex + 1, prevComaIndex + 2) == "1");
}

void saveJumpsList()
{
  SD.remove("jumpType.txt");
  if( !SD.exists("jumpType.txt") ) Serial.println("File doesn't exists");
  else Serial.println("File exists");
 
  File jumpFile = SD.open("jumpType.txt", FILE_WRITE);


  for (int i = 0; i < totalJumps; i++)
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
  jumpFile.flush();
  jumpFile.close();
  
  Serial.println("Jump types saved in jumpType.txt");
}

void addGravitatory(String row)
{
  int prevComaIndex = row.indexOf(":");
  int nextComaIndex = row.indexOf(",");
  //currentExerciseType = row.substring(prevComaIndex + 1, nextComaIndex).toInt();
  gravTypes[currentExerciseType].id = row.substring(prevComaIndex + 1, nextComaIndex).toInt();

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  gravTypes[currentExerciseType].name = row.substring(prevComaIndex + 1 , nextComaIndex);
  prevComaIndex = nextComaIndex;

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  gravTypes[currentExerciseType].description = row.substring(prevComaIndex + 1, nextComaIndex);
  prevComaIndex = nextComaIndex;

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  gravTypes[currentExerciseType].percentBodyWeight = row.substring(prevComaIndex + 1 , nextComaIndex).toFloat();
  prevComaIndex = nextComaIndex;

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  gravTypes[currentExerciseType].speed1Rm = row.substring(prevComaIndex + 1, nextComaIndex).toFloat();
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
    file = "jumpType.txt";
  }
  else if (mode == gravitatory) {
    //Serial.println("G");
    file = "gravType.txt";
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
    if(jumpTypes[i].startIn) Serial.println("Yes, ");
    else Serial.println("No, ");
  }
}

void printGravTypesList()
{
  Serial.println();
  Serial.println("totalGravTypes: " + String(totalGravTypes) );
  Serial.println("id, name, description, percentBodyWeight, speed1RM");
  for (unsigned int i = 0; i < totalGravTypes; i++)
  {
    Serial.print(String(gravTypes[i].id) + ", ");
    Serial.print(gravTypes[i].name + ", ");
    Serial.print(gravTypes[i].description + ", ");
    Serial.print(String( gravTypes[i].percentBodyWeight , 2) + "%, ");
    Serial.println(String( gravTypes[i].speed1Rm , 2) + "m/s, ");
  }
}
