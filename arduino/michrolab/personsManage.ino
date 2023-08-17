void savePersonsList()
{
  String file = "GROUP" + String(group) + ".TXT";
  SD.remove(file.c_str());
  File personsFile = SD.open(file.c_str(), FILE_WRITE);

  if(personsFile) Serial.println("File created");
  else Serial.println("Error creating file");

  Serial.println("totalPersons:" + String(totalPersons));
  for (unsigned int i = 0; i < totalPersons; i++)
  {
    personsFile.print(persons[i].index);
    personsFile.print("," + persons[i].name + "," + persons[i].surname + ",");
    personsFile.print(persons[i].weight);
    personsFile.print(",");
    personsFile.println(persons[i].heigh);
  }
  personsFile.flush();
  personsFile.close();
  File root = SD.open("/");
  printDirectory(root, 4);
}

void printPersonsList()
{
  Serial.println("Current group:" + String(group));
  for (unsigned int i = 0; i < totalPersons; i++)
  {
    Serial.print(persons[i].index);
    Serial.print("," + persons[i].name + "," + persons[i].surname + ",");
    Serial.print(persons[i].weight);
    Serial.print(",");
    Serial.println(persons[i].heigh);
  }
}

void deletePersonsList()
{
  totalPersons = 0;
}

//TODO: Check the the id is unique
void addPerson(String row)
{
  int prevComaIndex = row.indexOf(":");
  int nextComaIndex = row.indexOf(",");
  
  persons[totalPersons].index = row.substring(prevComaIndex + 1, nextComaIndex).toInt();

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  persons[totalPersons].name = row.substring(prevComaIndex + 1 , nextComaIndex);
  
  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  persons[totalPersons].surname = row.substring(prevComaIndex + 1 , nextComaIndex);

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  persons[totalPersons].heigh = row.substring(prevComaIndex + 1 , nextComaIndex).toFloat();

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  persons[totalPersons].weight = row.substring(prevComaIndex + 1 , nextComaIndex).toFloat();

  totalPersons++;
}

//void readPersonsFile(struct personType * persons)
void readPersonsFile()
{
  /*
    Example of persons file format
    0,Blancaneus,160, 65,
    1,Pulgarcito,16, 6,
    3,Tres porquets,50, 20,
  */
  String row = "";
  char readChar;
  String rowString = "";
  unsigned long pos = 0;    //Position in the file
  String fileName = "GROUPS/GROUP" + String(group) + ".TXT";
  File  personsFile = SD.open(fileName.c_str());
  currentPerson = 0;

  //Checking that the file is read
  if (!personsFile) {
    Serial.println("error opening " + fileName);
    return;
  }

  if (personsFile)
  {
    currentPerson = 0;
    personsFile.seek(0);
    totalPersons = 0;

    // read from the file until there's nothing else in it:
    while ( pos <= personsFile.size() )
    {
      readChar = '0';
      rowString = "";

      //Reading the new row
      while (readChar != '\n' && readChar != '\r' && pos <= personsFile.size())
      {
        readChar = personsFile.read();
        rowString = rowString + readChar;
        pos++;
      }

      if ( isDigit(rowString[0]) )
      {
        addPerson(rowString);
      }
    }
    // close the file:
    personsFile.close();
  }
}

void updatePersonSet() {updatePersonSet(true); }
void updatePersonSet(bool nextPerson)
{
  String personSet = "Set: " + addLeadingZeros(setNumber, 2) + "   Person: " + addLeadingZeros(currentPerson, 2);
  tft.setTextSize(1);
  tft.fillRect(148, 207, 120, 8, BLACK);
  tft.setTextColor(BLACK);
  tft.setCursor(148, 223);
  tft.print(persons[currentPerson].name + " " + persons[currentPerson].surname);
  if( nextPerson ) currentPerson = (currentPerson + 1) % totalPersons;

  personSet = "Set: " + addLeadingZeros(setNumber, 2) + "   Person: " + addLeadingZeros(currentPerson, 2);
  fileName = "S" + addLeadingZeros(setNumber, 2) + "P" + addLeadingZeros(currentPerson, 2);
  tft.setTextColor(WHITE);
  tft.setCursor(148, 207);
  tft.print(personSet);
  tft.setCursor(148, 223);
  tft.print(persons[currentPerson].name + " " + persons[currentPerson].surname);
  tft.setTextSize(2);
}

void updatePersonJump(int totalJumps)
{
  //Deleting last string
  tft.fillRect(141, 207, 127, 24, BLACK);

  //Writing new string
  printTftText(jumpTypes[currentExerciseType].name, 141, 207, WHITE, 1);
  printTftText("Person: " + addLeadingZeros(currentPerson, 2), 195, 207, WHITE, 1);
  printTftText(persons[currentPerson].name + " " + persons[currentPerson].surname, 141, 223, WHITE, 1);
  tft.setTextSize(2);
}

void selectGroup()
{
  group = selectValueDialog("Select the group number", "0,9", "1", 0);

  EEPROM.put(groupAddress, group);
  printTftText("Selecting...", 30, 115, WHITE, 3);
  //totalPersons = getTotalPerson();
  readPersonsFile();
  dirNumber -= 1; //It makes not to increase the session number
  dirName = createNewDir();
  menuItemsNum = systemMenuItems;
  printTftText("Selecting...", 30, 115, BLACK, 3);
  showMenuEntry(currentMenuIndex);
}

void selectPersonDialog()
{
  // Serial.println("<selectPersonDialog");
  tft.fillScreen(BLACK);
  printTftText("Select person", 40, 20, WHITE, 3);
  for (int i = -3; i <= 3; i++)
  {
    textList[i+3] = persons[ (i + totalPersons) % totalPersons].name + " " + persons[ (i + totalPersons) % totalPersons].surname; 
  }
  
  //showPersonList(WHITE);
  showList(WHITE);

  //drawLeftButton("Next", WHITE, BLUE);
  //drawRightButton("Accept", WHITE, RED);

  rightButton.update();
  upButton.update();
  cenButton.update();
  while (currentConfigSetMenu == personSelect )
  {
    if (downButton.fell())
    {
      //Deleting last list
      showList(BLACK);

      currentPerson = (currentPerson + 1) % totalPersons;
      for (int i = -3; i <= 3; i++)
      {
        textList[i+3] = persons[ (currentPerson + i + totalPersons) % totalPersons].name + " " + persons[ (currentPerson + i + totalPersons) % totalPersons].surname;
      }      
      // Serial.println("Changed to " + String(currentPerson));
      //Printing new list
      showList(WHITE);
    }
    if (upButton.fell())
    {
      //Deleting last list
      showList(BLACK);
      currentPerson = (currentPerson - 1 + totalPersons) % totalPersons;
      for (int i = -3; i <= 3; i++)
      {
        textList[i+3] = persons[ (currentPerson + i + totalPersons) % totalPersons].name + " " + persons[ (currentPerson + i + totalPersons) % totalPersons].surname;
      }
      // Serial.println("Changed to " + String(currentPerson));
      //Printing new list
      showList(WHITE);      
    }
    if ( rightButton.fell() || cenButton.fell() ) currentConfigSetMenu = exerciseSelect;
    if ( leftButton.fell() ) currentConfigSetMenu = quit;
    updateButtons();
  }
  Serial.println(currentConfigSetMenu);
  // Serial.println("selectPersonDialog>");
}

void selectPerson()
{
  //setNumber++;
  updatePersonSet();
  while (!cenButton.fell())
  {
    rightButton.update();
    if (rightButton.fell()) {
      updatePersonSet();
    }
    cenButton.update();
  }
}

void setGroup(String parameters)
{
  group = parameters.substring(0, parameters.lastIndexOf(";")).toInt();
  EEPROM.put(groupAddress, group);
  Serial.println("Group:" + String(group));
}
