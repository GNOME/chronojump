void savePersonsList()
{
  SD.remove("persons.txt");
  File personsFile = SD.open("persons.txt");
  for (unsigned int i = 0; i < totalPersons; i++)
  {
    personsFile.print(persons[i].index);
    personsFile.print("," + persons[i].name + "," + persons[i].surname + ",");
    personsFile.print(persons[i].weight);
    personsFile.print(",");
    personsFile.println(persons[i].heigh);
  }
  personsFile.close();
}

void printPersonsList()
{
  for (unsigned int i = 0; i < totalPersons; i++)
  {
    Serial.print(persons[i].index);
    Serial.print("," + persons[i].name + "," + persons[i].surname + ",");
    Serial.print(persons[i].weight);
    Serial.print(",");
    Serial.println(persons[i].heigh);
  }
}

void addPerson(String row)
{
  int prevComaIndex = row.indexOf(":");
  int nextComaIndex = row.indexOf(",");
  currentPerson = row.substring(prevComaIndex + 1, nextComaIndex).toInt();
  persons[currentPerson].index = currentPerson;

  if (currentPerson >= totalPersons) totalPersons = currentPerson + 1;
  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  persons[currentPerson].name = row.substring(prevComaIndex + 1 , nextComaIndex);
  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  persons[currentPerson].surname = row.substring(prevComaIndex + 1 , nextComaIndex);

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  persons[currentPerson].weight = row.substring(prevComaIndex + 1 , nextComaIndex).toFloat();

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  persons[currentPerson].heigh = row.substring(prevComaIndex + 1 , nextComaIndex).toFloat();
}

//void readPersonsFile(struct personType * persons)
void readPersonsFile()
{
  /*
     Ecample of persons.txt format
    0,Blancaneus,160, 65,
    1,Pulgarcito,16, 6,
    3,Tres porquets,50, 20,
  */
  String row = "";
  char readChar;
  String filename = "group" + String(group) + ".txt";
  File  personsFile = SD.open(filename.c_str());
  if (personsFile)
  {
    currentPerson = 0;
    personsFile.seek(0);

    // read from the file until there's nothing else in it:
    while (currentPerson < totalPersons)
    {
      readChar = personsFile.read();
      if (readChar != '\n' && readChar != '\r')
      {
        row = row + readChar;
      } else if (readChar == '\n' || readChar == '\r')
      {
        addPerson(row);
        row = "";
        currentPerson++;
      }
    }
    // close the file:
    personsFile.close();
  } else {
    // if the file didn't open, print an error:
    Serial.println("error opening " + filename);
  }
  currentPerson = 0;
}

unsigned int getTotalPerson()
{
  char readChar;
  String readString = "";
  String filename = "group" + String(group) + ".txt";
  File  personsFile = SD.open(filename.c_str());
  if (personsFile)
  {
    //Start reading from the last byte
    unsigned long pos = personsFile.size() - 4;

    //Reading the person number of the last row
    while (readChar != '\n' && readChar != '\r')
    {
      personsFile.seek(pos);
      readChar = personsFile.peek();
      pos--;
    }
    pos++;
    personsFile.seek(pos);
    readChar = personsFile.read();
    while (readChar != ',')
    {
      readChar = personsFile.read();
      readString = readString + readChar;
    }
  }
  totalPersons = readString.toInt() + 1;
  return (totalPersons);
}

void updatePersonSet()
{
  String personSet = "Set: " + addLeadingZeros(setNumber, 2) + "   Person: " + addLeadingZeros(currentPerson, 2);
  tft.setTextSize(1);
  tft.fillRect(148, 207, 120, 8, BLACK);
  tft.setTextColor(BLACK);
  tft.setCursor(148, 223);
  tft.print(persons[currentPerson].name + " " + persons[currentPerson].surname);
  currentPerson = (currentPerson + 1) % totalPersons;

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
  totalPersons = getTotalPerson();
  readPersonsFile();
  dirNumber -= 1; //It makes not to increase the session number
  dirName = createNewDir();
  menuItemsNum = systemMenuItems;
  showMenuEntry(currentMenuIndex);
}

void selectPersonDialog()
{
  tft.fillScreen(BLACK);
  showPersonList(WHITE);

  drawLeftButton("Next", WHITE, BLUE);
  drawRightButton("Accept", WHITE, RED);

  blueButton.update();
  redButton.update();
  while (!redButton.fell())
  {
    if (blueButton.fell())
    {
      //Deleting last list
      showPersonList(BLACK);

      currentPerson = (currentPerson + 1) % totalPersons;

      //Printing new list
      showPersonList(WHITE);
    }
    blueButton.update();
    redButton.update();
  }
}

void showPersonList(unsigned int color)
{
  int xPos = 10;
  int midYPos = 110;
  int currentY = 0;
  printTftText("Select person", 40, 20, color, 3);
  for (int i = -3; i <= 3; i++) {
    if (i == 0) {
      //Do nothing
    } else {
      if (i < 0 ) {
        currentY = midYPos + i * 16 - 3;
      } else if (i > 0) {
        currentY = midYPos + i * 16 + 8;
      }
      printTftText(persons[(currentPerson + totalPersons + i) % totalPersons].name + " " + persons[(currentPerson + totalPersons + i) % totalPersons].surname,
                   xPos, currentY, color, 2);
    }
  }
  //  printTftText("[" + (persons[currentPerson].name + " " + persons[currentPerson].surname).substring(0,14) + "]",
  //               xPos, midYPos, color, 3);
  tft.fillRoundRect(0, midYPos -1 ,320, 25, 5, RED);
  printTftText((persons[currentPerson].name + " " + persons[currentPerson].surname).substring(0, 17),
               xPos, midYPos, color, 3);
}
