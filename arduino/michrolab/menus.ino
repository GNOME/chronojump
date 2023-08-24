void updateCurrentMenu(menuEntry newMenu[10], int newNumEntries)
{
  // Serial.println("<updateCurrentMenu");
  for (int i = 0; i < newNumEntries; i++)
  {
    currentMenu[i].title = newMenu[i].title;
    currentMenu[i].description = newMenu[i].description;
    currentMenu[i].abbreviation = newMenu[i].abbreviation;
    currentMenu[i].function = newMenu[i].function;
  }
  // Serial.println("updateCurrentMenu>");
}
void drawUpperBar(menuEntry upperMenu[10], int numEntries)
{
  // Serial.println("<drawUpperBar");
  int x = 0;
  int y = 0;

  tft.fillRect(0,0,320,15,BLUE);
  tft.fillRect(currentMenuIndex * 320 / numEntries, 0, 320 / numEntries, 15, RED);
  for (int i = 0; i < numEntries; i++)
  {
    x = (i+0.5) * 320 / numEntries;
    printTftText(upperMenu[i].abbreviation, x,y, WHITE, 2, alignCenter);
  }

  // Serial.println("drawUpperBar>");
}

//Manages the current menu
void showMenu()
{
  // Serial.println("<showMenu");

  //The right/left buttons navigates through the Menu options.
  //The buttons are read in loop()
  if (leftButton.fell()) {
    currentMenuIndex--;
    currentMenuIndex = currentMenuIndex % menuItemsNum;
    if (currentMenuIndex < 0) {
      currentMenuIndex = menuItemsNum - 1;
    }
    showMenuEntry(currentMenuIndex);
  }
  
  if (rightButton.fell()) {
    currentMenuIndex++;
    currentMenuIndex = currentMenuIndex % menuItemsNum;
    showMenuEntry(currentMenuIndex);
  }

  //The red button activates the menu option
  if (cenButton.fell())
  {
    PcControlled = false;
    currentMenu[currentMenuIndex].function();
  }
  // Serial.println("showMenu>");
}

//Exits the currentMenu and sets the currentMenu to mainMenu
void backMenu(void)
{
  // Serial.println("<backMenu");
  currentMenuIndex = 0;
  drawMenuBackground();
  currentMenuIndex = 0;
  updateCurrentMenu(mainMenu, mainMenuItems);
  showMenuEntry(currentMenuIndex);
  showMenu();
  // Serial.println("backMenu>");
}

//Erases screen and draw the left and right buttons in the upper part of screen
void drawMenuBackground() {
  // Serial.println("<drawMenuBackground");
  tft.fillScreen(BLACK);
  tft.writeRect(0, 20, 25, 25, (uint16_t*)left);
  tft.writeRect(295, 20, 25, 25, (uint16_t*)right);
  tft.writeRect(145, 215, 25, 25, (uint16_t*)center);
  printTftText("Enter",143, 210, WHITE, 1,alignLeft);
  drawUpperBar(mainMenu, mainMenuItems);
  // Serial.println("drawMenuBackground>");
}

//Set the currentMenu to systemMenu and shows it
void showSystemMenu(void)
{
  // Serial.println("<showSystemMenu");
  drawMenuBackground();
  currentMenuIndex = 0;
  updateCurrentMenu(systemMenu, systemMenuItems);
  drawUpperBar(systemMenu, systemMenuItems);
  showSystemEntry(currentMenuIndex);
  // Serial.println("showSystemMenu>");
}

void showSystemEntry(unsigned int currentMenuIndex)
{
  // Serial.println("<showSystemEntry");
  tft.fillRect(30, 15, 260, 50, BLACK);
  printTftText(currentMenu[currentMenuIndex].title, 40, 20, WHITE, 3);
  //This erases the last index description
  updateButtons();
  if ( rightButton.fell() ) printTftText(currentMenu[(currentMenuIndex + systemMenuItems - 1) % systemMenuItems].description, 12, 100, BLACK);
  if (leftButton.fell() )   printTftText(currentMenu[(currentMenuIndex + systemMenuItems + 1) % systemMenuItems].description, 12, 100, BLACK);
    
  updateButtons();
  printTftText(currentMenu[currentMenuIndex].description, 12, 100);
  // Serial.println("showSystemEntry>");
}

//shows the current entry of the current menu
void showMenuEntry(unsigned int currentMenuIndex)
{
  // Serial.println("<showMenuEntry");
  drawUpperBar(currentMenu, menuItemsNum);
  tft.fillRect(30, 20, 260, 25, BLACK);
  printTftText(currentMenu[currentMenuIndex].title, 40, 20, WHITE, 3);
  //This erases the last index description
  if (rightButton.fell()) {
    printTftText(currentMenu[(currentMenuIndex + menuItemsNum - 1) % menuItemsNum].description, 12, 100, BLACK);
  }
    if (leftButton.fell()) {
    printTftText(currentMenu[(currentMenuIndex + menuItemsNum + 1) % menuItemsNum].description, 12, 100, BLACK);
  }
  updateButtons();
  printTftText(currentMenu[currentMenuIndex].description, 12, 100);
  // Serial.println("showMenuEntry>");
}

void drawRightButton(void) { drawRightButton(295, 212, "", WHITE, BLACK); }
void drawRightButton(String label) {drawRightButton(295, 206, label, WHITE, BLACK); }
void drawRightButton(String label, uint16_t tColor) {drawRightButton(295, 206, label, tColor, BLACK); }
void drawRightButton(String label, uint16_t tColor, uint16_t bColor) {drawRightButton(295, 206, label, tColor, bColor); }
void drawRightButton(int x, int y) { drawRightButton(x, y, "", WHITE, BLACK); }
void drawRightButton(int x, int y, String label) { drawRightButton(x, y, label, WHITE, BLACK); }
void drawRightButton(int x, int y, String label, uint16_t tColor) { drawRightButton(x, y, label, tColor, BLACK); }
void drawRightButton(int x, int y, String label, uint16_t tColor, uint16_t bColor)
{
  tft.writeRect(x, y, 25, 25, (uint16_t*)right);
  if (label.length() > 0) {
    printTftText(label, x-1, y+6, tColor, 2, alignRight);
    }
}

void drawLeftButton(void) { drawLeftButton(0, 206, "", WHITE, BLACK); }
void drawLeftButton(String label) {drawLeftButton(0, 206, label, WHITE, BLACK); }
void drawLeftButton(String label, uint16_t tColor) {drawLeftButton(0, 206, label, tColor, BLACK); }
void drawLeftButton(String label, uint16_t tColor, uint16_t bColor) {drawLeftButton(0, 206, label, tColor, bColor); }
void drawLeftButton(int x, int y) { drawLeftButton(x, y, "", WHITE, BLACK); }
void drawLeftButton(int x, int y, String label) { drawLeftButton(x, y, label, WHITE, BLACK); }
void drawLeftButton(int x, int y, String label, uint16_t tColor) { drawLeftButton(x, y, label, tColor, BLACK); }
void drawLeftButton(int x, int y, String label, uint16_t tColor, uint16_t bColor)
{
  tft.writeRect(x, y, 25, 25, (uint16_t*)left);
  if (label.length() > 0) {
    printTftText(label, x + 26, y+6, tColor, 2);
  }
}


void drawUpDownButton(void) { drawUpDownButton(0, 206, "", WHITE, BLACK); }
void drawUpDownButton(String label) {drawUpDownButton(0, 206, label, WHITE, BLACK); }
void drawUpDownButton(String label, uint16_t tColor) {drawUpDownButton(0, 206, label, tColor, BLACK); }
void drawUpDownButton(String label, uint16_t tColor, uint16_t bColor) {drawUpDownButton(0, 206, label, tColor, bColor); }
void drawUpDownButton(int x, int y) { drawUpDownButton(x, y, "", WHITE, BLACK); }
void drawUpDownButton(int x, int y, String label) { drawUpDownButton(x, y, label, WHITE, BLACK); }
void drawUpDownButton(int x, int y, String label, uint16_t tColor) { drawUpDownButton(x, y, label, tColor, BLACK); }
void drawUpDownButton(int x, int y, String label, uint16_t tColor, uint16_t bColor)
{
  tft.writeRect(x, y, 25, 25, (uint16_t*)upDown);
  if (label.length() > 0) {
    printTftText(label, x + 26, y+6, tColor, 2);
  }
}

void drawLeftRightButton(void) { drawLeftRightButton(0, 206, "", WHITE, BLACK); }
void drawLeftRightButton(String label) {drawLeftRightButton(0, 206, label, WHITE, BLACK); }
void drawLeftRightButton(String label, uint16_t tColor) {drawLeftRightButton(0, 206, label, tColor, BLACK); }
void drawLeftRightButton(String label, uint16_t tColor, uint16_t bColor) {drawLeftRightButton(0, 206, label, tColor, bColor); }
void drawLeftRightButton(int x, int y) { drawLeftRightButton(x, y, "", WHITE, BLACK); }
void drawLeftRightButton(int x, int y, String label) { drawLeftRightButton(x, y, label, WHITE, BLACK); }
void drawLeftRightButton(int x, int y, String label, uint16_t tColor) { drawLeftRightButton(x, y, label, tColor, BLACK); }
void drawLeftRightButton(int x, int y, String label, uint16_t tColor, uint16_t bColor)
{
  tft.writeRect(x, y, 25, 25, (uint16_t*)leftRight);
  if (label.length() > 0) {
    printTftText(label, x + 26, y+6, tColor, 2);
  }
}


//Dialog for selecting float value
float selectValueDialog(String description, String rangesString, String incString) {
  return selectValueDialog(description, rangesString, incString,0);
}
float selectValueDialog(String description, String rangesString, String incString, unsigned int decimals)
{
  //ranges are of the format "1,10,500"
  //increments are in the format of  "2,10"
  //From 1..10 increment by 2
  //From 10..500 increment by 10
  //increments must have the number of ranges elements -1
  //Maximum number of ranges is 10 (11 key values)

  int prevColon = 0;
  int nextColon = rangesString.indexOf(",");
  unsigned int rangesNum = 0;

  //Counting ranges
  do
  {
    rangesNum++;
    prevColon = nextColon + 1;
    nextColon = rangesString.indexOf(",", prevColon);
  } while (nextColon != -1);
  float rangesValues[11];
  float incValues[10];

  //Assigning key values of the ranges
  prevColon = 0;
  nextColon = rangesString.indexOf(",");
  for (unsigned int i = 0; i <= rangesNum; i++)
  {
    rangesValues[i] = rangesString.substring(prevColon, nextColon).toFloat();
    prevColon = nextColon + 1;
    nextColon = rangesString.indexOf(",", prevColon);
  }

  //Assigning increment values
  prevColon = 0;
  nextColon = incString.indexOf(",");
  for (unsigned int i = 0; i < rangesNum; i++)
  {
    incValues[i] = incString.substring(prevColon, nextColon).toFloat();
    prevColon = nextColon + 1;
    nextColon = incString.indexOf(",", prevColon);
  }

  float value = rangesValues[0];
  submenu = 0;
  unsigned int currentSegment = 1;
  int nextSegment = 0;
  bool exitFlag = false;
  //Delete description
  tft.fillRect(0, 50, 320, 190, BLACK);

//  tft.setCursor(30, 80);
//  tft.print(title);

  //Explanation of the process
  printTftText(description, 10, 112);

  //Blue button
  //drawLeftButton("+" + String(incValues[0], decimals), WHITE, BLUE);

  //Red button
  //drawRightButton("Accept", WHITE, RED);

  drawUpDownButton(0,168, "+-" + String(incValues[currentSegment - 1], decimals), WHITE, RED);
  //drawUpButton("+" + String(incValues[currentSegment - 1], decimals), WHITE, BLUE);

  //Current value
  printTftText("Current:", 112, 174);
  printTftValue(value, 248, 174, 2, 0);
  updateButtons();
  
  while (!exitFlag) {

    if ( upButton.fell() || downButton.fell() ) {
      printTftValue(value, 248, 174, 2, decimals, BLACK);

      if (upButton.fell()) value += incValues[currentSegment - 1];
      if (downButton.fell()) value -= incValues[currentSegment - 1];

      //Limit the value to the minimum and maximum
      if ( value <= rangesValues[0] ) value = rangesValues[0];
      if ( value >= rangesValues[rangesNum] ) value = rangesValues[rangesNum];

       //Value reached the lower limit of the range
      if ( value <= rangesValues[currentSegment - 1] && currentSegment > 0) nextSegment = currentSegment - 1;

      //Value reached the upper limit of the range
      if (value >= rangesValues[currentSegment] && currentSegment < rangesNum) nextSegment = currentSegment + 1;

      //Updating segment
      if (nextSegment != 0) {
        drawUpDownButton(0,168, "+-" + String(incValues[currentSegment - 1], decimals), BLACK, RED);
        //drawUpButton("+" + String(incValues[currentSegment - 1], decimals), BLACK, BLUE);
        currentSegment = nextSegment;
        nextSegment = 0;
        drawUpDownButton(0,168, "+-" + String(incValues[currentSegment - 1], decimals), WHITE, RED);
        //drawUpButton("+" + String(incValues[currentSegment - 1], decimals), WHITE, BLUE);
      }
      printTftValue(value, 248, 174, 2, decimals);
    }
    
    //Change to Calibrate execution
    if ( cenButton.fell()|| rightButton.fell() ){
      //Deleting explanation
      tft.fillRect(0, 60, 320, 240, BLACK);

      submenu = 1;
      exitFlag = true;
    }

    updateButtons();
  }
  return value;
}

bool yesNoDialog(String message, float x, float y) {
  return yesNoDialog(message, x, y, 2);
}
bool yesNoDialog(String message, float x, float y, int fontSize)
{
  bool answer = false;
  int len = message.length();
  unsigned int w = 6 * fontSize * len;
  unsigned int h = 8 * fontSize;
  uint16_t textBackRect[w * h];
  tft.readRect(x, y, w, h, textBackRect);

  uint16_t redBackRect[78 * 36];
  tft.readRect(242, 206, 78, 36, redBackRect);
  drawRightButton("No");

  uint16_t blueBackRect[78 * 36];
  tft.readRect(0, 206, 78, 36, blueBackRect);
  drawLeftButton("Yes");

  printTftText(message, x, y, RED);
  leftButton.update();
  while ( !leftButton.fell() && !rightButton.fell() )
  {
    leftButton.update();
    rightButton.update();
  }
  printTftText(message, x, y, BLACK);
  tft.writeRect(x, y, w, h, textBackRect);
  tft.writeRect(242, 206, 78, 36, redBackRect);
  tft.writeRect(0, 206, 78, 36, blueBackRect);
  answer = leftButton.fell();
  leftButton.update();
  rightButton.update();
  return answer ;
}

unsigned int selectNextItem (int currentExerciseType, int arrayElements)
{
  currentExerciseType = (currentExerciseType + 1) % arrayElements;
  return currentExerciseType;
}

unsigned int selectPreviousItem (int currentExerciseType, int arrayElements)
{
  currentExerciseType = (currentExerciseType - 1 + arrayElements) % arrayElements;
  return currentExerciseType;
}

void selectLoadCellDialog()
{
  // Serial.println("<selectLoadCellDialog");
  readCalibrationsFile();
  tft.fillScreen(BLACK);
  printTftText("Sel. load cell", 40, 20, WHITE, 3);
  for (int i = -3; i <= 3; i++)
  {
    textList[i+3] = String( calibrations[ (currentCalibration + i + totalCalibrations) % totalCalibrations].id )
    + "-"
    + calibrations[ (currentCalibration + i + totalCalibrations) % totalCalibrations].description;
  }
  
  //showPersonList(WHITE);
  showList(WHITE);

  //drawLeftButton("Next", WHITE, BLUE);
  //drawRightButton("Accept", WHITE, RED);

  updateButtons();
  while (!cenButton.fell() )
  {
    //If selection changed delete list
    if (downButton.fell() || upButton.fell() )
    {
      //Deleting last list
      showList(BLACK);
    }
    //update selection depending on the button pressed
    if (downButton.fell() ) currentCalibration = ( currentCalibration + 1) % totalCalibrations;
    if (upButton.fell() ) currentCalibration = ( currentCalibration -1 ) % totalCalibrations;

    for (int i = -3; i <= 3; i++)
    {
      textList[i+3] = String( calibrations[ (currentCalibration + i + totalCalibrations) % totalCalibrations].id )
      + "-"
      + calibrations[ (currentCalibration + i + totalCalibrations) % totalCalibrations].description;
    }
    //If selection changed show the new list
    if (downButton.fell() || upButton.fell() )
    {
      showList(WHITE);
    }
    updateButtons();
  }
  scale.set_scale(calibrations[currentCalibration].calibration);
  scale.set_offset(calibrations[currentCalibration].tare);
  EEPROM.put(tareAddress, calibrations[currentCalibration].tare);
  EEPROM.put(calibrationAddress, calibrations[currentCalibration].calibration);
  drawMenuBackground();
  showMenuEntry(currentMenuIndex);
  // Serial.print("Current calibration: ");
  // Serial.println(currentCalibration);
  // Serial.println("selectLoadCellDialog>");
}