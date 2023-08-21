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
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[totalJumpTypes].jumpLimit = row.substring(prevComaIndex + 1, nextComaIndex).toInt();

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[totalJumpTypes].timeLimit = row.substring(prevComaIndex + 1, nextComaIndex).toFloat();

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[totalJumpTypes].hardTimeLimit = (row.substring(prevComaIndex + 1 , nextComaIndex) == 1);

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[totalJumpTypes].percentBodyWeight = row.substring(prevComaIndex + 1 , nextComaIndex).toFloat();

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[totalJumpTypes].fall = row.substring(prevComaIndex + 1, nextComaIndex).toFloat();

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[totalJumpTypes].startIn = (row.substring(prevComaIndex + 1, nextComaIndex) == "1");
  //Serial.println("totalJumpTypes: " + String(totalJumpTypes));

  
  //Brief description added to every type of exercise
  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(".", prevComaIndex + 1 );
  jumpTypes[totalJumpTypes].description = (row.substring(prevComaIndex + 1, nextComaIndex));
  //Serial.println("totalJumpTypes: " + String(totalJumpTypes));
  totalJumpTypes++;

}

void saveJumpsType()
{
  SD.remove("EXERCISE/JUMPTYPE.TXT");
 
  File jumpFile = SD.open("EXERCISE/JUMPTYPE.TXT", FILE_WRITE);

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
    
    jumpFile.println("," + String(jumpTypes[i].description));  
    jumpFile.print("," + String(jumpTypes[i].startIn));
  }
  jumpFile.close();
  Serial.println("Saved " + String(totalJumpTypes) + " to /EXERCISE/JUMPTYPE.TXT");
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
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  gravTypes[totalGravTypes].description = row.substring(prevComaIndex + 1, nextComaIndex);

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  gravTypes[totalGravTypes].percentBodyWeight = row.substring(prevComaIndex + 1 , nextComaIndex).toFloat();

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  gravTypes[totalGravTypes].speed1Rm = row.substring(prevComaIndex + 1, nextComaIndex).toFloat();
  totalGravTypes++;
}

void saveGravitatoryType()
{
  SD.remove("EXERCISE/GRAVTYPE.TXT");
 
  File gravFile = SD.open("EXERCISE/GRAVTYPE.TXT", FILE_WRITE);

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
  Serial.println("Saved " + String(totalGravTypes) + " to /EXERCISE/GRAVTYPE.TXT");
}

void addInertial(String row)
{
  int prevComaIndex = row.indexOf(":");
  int nextComaIndex = row.indexOf(",");
  //totalInertTypes = row.substring(prevComaIndex + 1, nextComaIndex).toInt();
  inertTypes[totalInertTypes].id = row.substring(prevComaIndex + 1, nextComaIndex).toInt();

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  inertTypes[totalInertTypes].name = row.substring(prevComaIndex + 1 , nextComaIndex);

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  inertTypes[totalInertTypes].description = row.substring(prevComaIndex + 1, nextComaIndex);

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  inertTypes[totalInertTypes].percentBodyWeight = row.substring(prevComaIndex + 1 , nextComaIndex).toFloat();

  totalInertTypes++;
}


void saveInertialType()
{
  SD.remove("EXERCISE/INERTYPE.TXT");
 
  File inertFile = SD.open("EXERCISE/INERTYPE.TXT", FILE_WRITE);

//  if(gravFile) Serial.println("File created");
//  else Serial.println("Error creating file");

  for (unsigned int i = 0; i < totalInertTypes; i++)
  {
    inertFile.print(jumpTypes[i].id);
    inertFile.print("," + String(gravTypes[i].name));
    inertFile.print("," + gravTypes[i].description );
    inertFile.print("," + String(gravTypes[i].percentBodyWeight));
    inertFile.println("," + String(gravTypes[i].speed1Rm));
  }
  inertFile.close();
  Serial.println("Saved " + String(totalInertTypes) + " to /EXERCISE/GRAVTYPE.TXT");
}

void addForce(String row)
{
  int prevComaIndex = row.indexOf(":");
  int nextComaIndex = row.indexOf(",");
  forceTypes[totalForceTypes].id = row.substring(prevComaIndex + 1, nextComaIndex).toInt();

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  forceTypes[totalForceTypes].name = row.substring(prevComaIndex + 1 , nextComaIndex);
  prevComaIndex = nextComaIndex;

  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  forceTypes[totalForceTypes].description = row.substring(prevComaIndex + 1, nextComaIndex);
  prevComaIndex = nextComaIndex;

  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  forceTypes[totalForceTypes].percentBodyWeight = row.substring(prevComaIndex + 1 , nextComaIndex).toFloat();
  prevComaIndex = nextComaIndex;

  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  forceTypes[totalForceTypes].angle = row.substring(prevComaIndex + 1 , nextComaIndex).toFloat();
  prevComaIndex = nextComaIndex;

  forceTypes[totalForceTypes].tare = ( row.substring(prevComaIndex + 1 , prevComaIndex + 2) == "1" );
  totalForceTypes++;
}

void addRaceAnalyzer(String row)
{
  int prevComaIndex = row.indexOf(":");
  int nextComaIndex = row.indexOf(",");
  raceAnalyzerTypes[totalRaceAnalyzerTypes].id = row.substring(prevComaIndex + 1, nextComaIndex).toInt();

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  raceAnalyzerTypes[totalRaceAnalyzerTypes].name = row.substring(prevComaIndex + 1 , nextComaIndex);
  prevComaIndex = nextComaIndex;

  nextComaIndex = row.length() - 1; //Eliminating the last character (end of line)
  raceAnalyzerTypes[totalRaceAnalyzerTypes].description = row.substring(prevComaIndex + 1,nextComaIndex);
  
  totalRaceAnalyzerTypes++;
}

void saveForceType()
{
  SD.remove("EXERCISE/FORCTYPE.TXT");
 
  File forceFile = SD.open("EXERCISE/FORCTYPE.TXT", FILE_WRITE);

//  if(gravFile) Serial.println("File created");
//  else Serial.println("Error creating file");

  for (unsigned int i = 0; i < totalForceTypes; i++)
  {
    forceFile.print(forceTypes[i].id);
    forceFile.print("," + String(forceTypes[i].name));
    forceFile.print("," + forceTypes[i].description );
    forceFile.print("," + String(forceTypes[i].percentBodyWeight));
    forceFile.print("," + String(forceTypes[i].angle));
    forceFile.println("," + String(forceTypes[i].tare));
  }
  forceFile.close();
  Serial.println("Saved " + String(totalForceTypes) + " to FORCTYPE.TXT");
}


void saveRaceAnalyzerTypes()
{
  SD.remove("EXERCISE/ERTYPE.TXT");
 
  File raceAnalyzerFile = SD.open("EXERCISE/ERTYPE.TXT", FILE_WRITE);

//  if(gravFile) Serial.println("File created");
//  else Serial.println("Error creating file");

  for (unsigned int i = 0; i < totalRaceAnalyzerTypes; i++)
  {
    raceAnalyzerFile.print(forceTypes[i].id);
    raceAnalyzerFile.print("," + String(forceTypes[i].name));
    raceAnalyzerFile.println("," + forceTypes[i].description );
  }
  
  raceAnalyzerFile.close();
  Serial.println("Saved " + String(totalRaceAnalyzerTypes) + " to /EXERCISE/ERTYPE.TXT");
}

void readExercisesFile(String parameters){
  parameters = parameters.substring(0, parameters.lastIndexOf(";"));
  if ( parameters == "jumps" ) readExercisesFile(jumps);
  else if ( parameters == "gravitatory" ) readExercisesFile(gravitatory);
  else if ( parameters == "inertial" ) readExercisesFile(inertial);
  else if ( parameters == "force" ) readExercisesFile( force );
  else if ( parameters == "raceAnalyzer" ) readExercisesFile( encoderRace );
  else Serial.println("Not a valid parameter");
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
    file = "EXERCISE/JUMPTYPE.TXT";
  } else if (mode == gravitatory) {
    //Serial.println("G");
    file = "EXERCISE/GRAVTYPE.TXT";
  } else if (mode == inertial) {
    //Serial.println("I");
    file = "EXERCISE/INERTYPE.TXT";
  } else if (mode == force) {
    //Serial.println("F");
    file = "EXERCISE/FORCTYPE.TXT";
  } else if (mode == encoderRace) {
    //Serial.println("ER");
    file = "EXERCISE/ERTYPE.TXT";
  }

  File  exercisesFile = SD.open(file.c_str());

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
        } else if (mode == inertial) {
          addInertial(readString);
          totalInertTypes = numRows;
        } else if (mode == force) {
          addForce(readString);
          totalForceTypes = numRows;
        } else if (mode == encoderRace) {
          addRaceAnalyzer(readString);
          totalRaceAnalyzerTypes = numRows;
        }
      }
    }
    Serial.println("Total:" + String(numRows));
  }
  exercisesFile.close();
}

void printJumpTypes()
{
  Serial.println("id, name, jumpLimit,timeLimit, hardTimeLimit, percentBodyWeight, fall, startIn, description");
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
    if(jumpTypes[i].startIn) Serial.print("Yes,");
    else Serial.print("No,");
    Serial.println(String( jumpTypes[i].description ) + ", ");
  }
}

void printGravTypes()
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

void printInertTypes()
{
  Serial.println("id, name, description, percentBodyWeight");
  for (unsigned int i = 0; i < totalInertTypes; i++)
  {
    Serial.print(String(inertTypes[i].id) + ", ");
    Serial.print(inertTypes[i].name + ", ");
    Serial.print(inertTypes[i].description + ", ");
    Serial.println(String( inertTypes[i].percentBodyWeight , 2) + "%, ");
  }
}

void printForceTypes()
{
  Serial.println("id, name, description, percentBodyWeight, angle, tare");
  for (unsigned int i = 0; i < totalForceTypes; i++)
  {
    Serial.print(String(forceTypes[i].id) + ", ");
    Serial.print(forceTypes[i].name + ", ");
    Serial.print(forceTypes[i].description + ", ");
    Serial.print(String( forceTypes[i].percentBodyWeight , 2) + "%, ");
    Serial.print(String( forceTypes[i].angle , 2) + ", ");
    Serial.println(forceTypes[i].tare);
  }
}

void printRaceAnalyzerTypes()
{
  Serial.println("id, name, description");
  for (unsigned int i = 0; i < totalRaceAnalyzerTypes; i++)
  {
    Serial.print(String(raceAnalyzerTypes[i].id) + ", ");
    Serial.print(raceAnalyzerTypes[i].name + ", ");
    Serial.println(raceAnalyzerTypes[i].description);
  }
}

bool selectExerciseType(exerciseType mode)
{
  // Serial.println("<selectExerciseType");
  tft.fillScreen(BLACK); 
  drawLeftButton(0, 20, "", BLACK, BLACK);
  drawRightButton(295, 20, "", BLACK, BLACK);
  drawUpDownButton(20,65, "", BLACK, BLACK);

  if (mode == jumps) {
    printTftText("Jump type", 40, 20, WHITE, 3);
    printTftText(jumpTypes[currentExerciseType].name, 50, 70);
    printTftText(jumpTypes[currentExerciseType].description, 30, 115);
  } else if (mode == gravitatory) {
    printTftText("Gravit. type", 40, 20, WHITE, 3);
    printTftText(gravTypes[currentExerciseType].name, 50, 70);
    printTftText(gravTypes[currentExerciseType].description, 30, 115);
  } else if (mode == inertial) {
    printTftText("Inert. type", 40, 20, WHITE, 3);
    printTftText(inertTypes[currentExerciseType].name, 50, 70);
    printTftText(inertTypes[currentExerciseType].description, 30, 115);
  } else if (mode == force) {
    printTftText("Force type", 40, 20, WHITE, 3);
    printTftText(forceTypes[currentExerciseType].name, 50, 70);
    printTftText(forceTypes[currentExerciseType].description, 30, 115);
  } else if (mode == encoderRace) {
    printTftText("Race type", 40, 20, WHITE, 3);
    printTftText(raceAnalyzerTypes[currentExerciseType].name, 50, 70);    
    printTftText(raceAnalyzerTypes[currentExerciseType].description, 30, 115);
  } else if (mode == rawPower) {
    startPowerCapture();
  } else if (mode == steadiness) {
    startSteadiness();
  }
  /*
  } else if (mode == credits) {
    showSystemMenu();    
  }
  */

  /*old next & accept buttons
  drawLeftButton("Next", WHITE, BLUE);
  drawRightButton("Accept", WHITE, RED);
  */
  
  
  updateButtons();

  while( !cenButton.fell() && !rightButton.fell() && !leftButton.fell())
  {
    //FORDWARD
    if(downButton.fell())
    {      
      if (mode == jumps) {
        printTftText(jumpTypes[currentExerciseType].name, 50, 70, BLACK);
        printTftText(jumpTypes[currentExerciseType].description, 30, 115, BLACK);
        currentExerciseType = selectNextItem(currentExerciseType, totalJumpTypes);
        printTftText(jumpTypes[currentExerciseType].name, 50, 70);
        printTftText(jumpTypes[currentExerciseType].description, 30, 115);
      } else if (mode == gravitatory) {
        printTftText(gravTypes[currentExerciseType].name, 50, 70, BLACK);
        printTftText(gravTypes[currentExerciseType].description, 30, 115, BLACK);
        currentExerciseType = selectNextItem(currentExerciseType, totalGravTypes);
        printTftText(gravTypes[currentExerciseType].name, 50, 70); 
        printTftText(gravTypes[currentExerciseType].description, 30, 115);  
      } else if (mode == inertial) {
        printTftText(inertTypes[currentExerciseType].name, 50, 70, BLACK);
        printTftText(inertTypes[currentExerciseType].description, 30, 115, BLACK);
        currentExerciseType = selectNextItem(currentExerciseType, totalInertTypes);
        printTftText(inertTypes[currentExerciseType].name, 50, 70);   
        printTftText(inertTypes[currentExerciseType].description, 30, 115);
      } else if (mode == force) {
        printTftText(forceTypes[currentExerciseType].name, 50, 70, BLACK);
        printTftText(forceTypes[currentExerciseType].description, 30, 115, BLACK);
        currentExerciseType = selectNextItem(currentExerciseType, totalForceTypes);   
        printTftText(forceTypes[currentExerciseType].name, 50, 70);
        printTftText(forceTypes[currentExerciseType].description, 30, 115);   
      } else if (mode == encoderRace) {
        printTftText(raceAnalyzerTypes[currentExerciseType].name, 50, 70, BLACK);
        printTftText(raceAnalyzerTypes[currentExerciseType].description, 30, 115, BLACK);
        currentExerciseType = selectNextItem(currentExerciseType, totalRaceAnalyzerTypes);   
        printTftText(raceAnalyzerTypes[currentExerciseType].name, 50, 70);   
        printTftText(raceAnalyzerTypes[currentExerciseType].description, 30, 115);
      }
    }
    //BACKWARD
    if(upButton.fell()) {
      if (mode == jumps) {
        printTftText(jumpTypes[currentExerciseType].name, 50, 70, BLACK);
        printTftText(jumpTypes[currentExerciseType].description, 30, 115, BLACK);
        currentExerciseType = selectPreviousItem(currentExerciseType, totalJumpTypes);
        printTftText(jumpTypes[currentExerciseType].name, 50, 70);
        printTftText(jumpTypes[currentExerciseType].description, 30, 115);
      } else if (mode == force) {
        printTftText(forceTypes[currentExerciseType].name, 50, 70, BLACK);
        printTftText(forceTypes[currentExerciseType].description, 30, 115, BLACK);
        currentExerciseType = selectPreviousItem(currentExerciseType, totalForceTypes);   
        printTftText(forceTypes[currentExerciseType].name, 50, 70);
        printTftText(forceTypes[currentExerciseType].description, 30, 115);
      } else if (mode == gravitatory) {
        printTftText(gravTypes[currentExerciseType].name, 50, 70, BLACK);
        printTftText(gravTypes[currentExerciseType].description, 30, 115, BLACK);
        currentExerciseType = selectPreviousItem(currentExerciseType, totalGravTypes);   
        printTftText(gravTypes[currentExerciseType].name, 50, 70);   
        printTftText(gravTypes[currentExerciseType].description, 30, 115);  
      } else if (mode == inertial) {
        printTftText(inertTypes[currentExerciseType].name, 50, 70, BLACK);
        printTftText(inertTypes[currentExerciseType].description, 30, 115, BLACK);
        currentExerciseType = selectPreviousItem(currentExerciseType, totalInertTypes);
        printTftText(inertTypes[currentExerciseType].name, 50, 70);   
        printTftText(inertTypes[currentExerciseType].description, 30, 115);
      } else if (mode == encoderRace) {
        printTftText(raceAnalyzerTypes[currentExerciseType].name, 50, 70, BLACK);
        printTftText(raceAnalyzerTypes[currentExerciseType].description, 30, 115, BLACK);
        currentExerciseType = selectPreviousItem(currentExerciseType, totalRaceAnalyzerTypes);   
        printTftText(raceAnalyzerTypes[currentExerciseType].name, 50, 70);
        printTftText(raceAnalyzerTypes[currentExerciseType].description, 30, 115);
      } 
    }
    updateButtons();
  }

    //Go to the previous menu
    if(leftButton.fell()) {
      prevConfigSetMenu = true;
      backMenu();
      showMenu();
    }
    //Go to the next menu
    if (rightButton.fell() || cenButton.fell()) {
      nextConfigSetMenu = true;
    }
  // Serial.println("selectExerciseType>");
  return true;
}
