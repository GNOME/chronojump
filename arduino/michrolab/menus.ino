

//Manages the current menu
void showMenu()
{
  //The blue button navigates through the Menu options
  
  rightButton.update();
  leftButton.update();
  if (leftButton.fell()) {
    currentMenuIndex--;
    currentMenuIndex = currentMenuIndex % menuItemsNum;
    if (currentMenuIndex < 0) {
      currentMenuIndex = menuItemsNum - 1;
    }
    leftButtonPressed = true;
    showMenuEntry(currentMenuIndex);
  }
  
  if (rightButton.fell()) {
    currentMenuIndex++;
    currentMenuIndex = currentMenuIndex % menuItemsNum;
    rightButtonPressed = true; 
    showMenuEntry(currentMenuIndex);
  }

  //The red button activates the menu option
  cenButton.update();
  if (cenButton.fell())
  {
    PcControlled = false;
    currentMenu[currentMenuIndex].function();
  }
}

//Exits the currentMenu and sets the currentMenu to mainMenu
void backMenu(void)
{
  currentMenuIndex = 0;
  drawMenuBackground();
  currentMenuIndex = 0;
  for (int i = 0; i < 10; i++) {
    currentMenu[i].title = mainMenu[i].title;
    currentMenu[i].description = mainMenu[i].description;
    currentMenu[i].function = mainMenu[i].function;
  }
  menuItemsNum = mainMenuItems;
  showMenuEntry(currentMenuIndex);
  showMenu();
}

//Erases screen and draw the left and right buttons in the upper part of screen
void drawMenuBackground() {
  tft.fillScreen(BLACK);
  tft.fillRoundRect(0, 0, 30, 50, 10, WHITE);
  tft.fillRoundRect(290, 0, 30, 50, 10, WHITE);
  tft.setCursor(30, 20);
    
  drawLeftButton("Prev", WHITE, RED);
  drawRightButton("Next", WHITE, BLUE);
}

//Set the currentMenu to systemMenu and shows it
void showSystemMenu(void)
{
  /*
  if (!selectExerciseType(jumps) || !selectExerciseType(inertial) || !selectExerciseType(force) || !selectExerciseType(encoderRace)) 
  {
    //Try adding showmenu & backmenu functions    
    return;
  }
  */  
  drawMenuBackground();
  currentMenuIndex = 0;
  for (int i = 0; i < 10; i++) {
    currentMenu[i].title = systemMenu[i].title;
    currentMenu[i].description = systemMenu[i].description;
    currentMenu[i].function = systemMenu[i].function;
  }
  //menuItemsNum = systemMenuItems;
  //Create a new function to navigate through the system menu
  //showMenuEntry(currentMenuIndex);
  showSystemEntry(currentMenuIndex);
  //showMenu();
}

void showSystemEntry(unsigned int currentMenuIndex)
{
  tft.fillRect(30, 0, 260, 50, BLACK);
  printTftText(currentMenu[currentMenuIndex].title, 40, 20, WHITE, 3);
  drawLeftButton("Prev", WHITE, RED);
  drawRightButton("Next", WHITE, BLUE);
  //This erases the last index description
  rightButton.update();
  leftButton.update();
  Serial.println(systemMenuItems);  
  if (rightButtonPressed) {
    printTftText(currentMenu[(currentMenuIndex + systemMenuItems - 1) % systemMenuItems].description, 12, 100, BLACK);   
  }
  else if (leftButtonPressed) { //Fordwards
    printTftText(currentMenu[(currentMenuIndex + systemMenuItems + 1) % systemMenuItems].description, 12, 100, BLACK);
  }
    
  rightButton.update();
  leftButton.update();
  Serial.println(currentMenuIndex);
  printTftText(currentMenu[currentMenuIndex].description, 12, 100); 
}

//shows the current entry of the current menu
void showMenuEntry(unsigned int currentMenuIndex)
{
  tft.fillRect(30, 0, 260, 50, BLACK);
  printTftText(currentMenu[currentMenuIndex].title, 40, 20, WHITE, 3);
  drawLeftButton("Prev", WHITE, RED);
  drawRightButton("Next", WHITE, BLUE);
  //This erases the last index description
  rightButton.update();
  leftButton.update();  
  if (rightButtonPressed) {
    printTftText(currentMenu[(currentMenuIndex + menuItemsNum - 1) % menuItemsNum].description, 12, 100, BLACK);
    rightButtonPressed = false;
  }
  else if (leftButtonPressed) {
    printTftText(currentMenu[(currentMenuIndex + menuItemsNum + 1) % menuItemsNum].description, 12, 100, BLACK);     
    leftButtonPressed = false;
  }
  else if (leftButtonPressed && leftButton.fell()) {
    printTftText(currentMenu[(currentMenuIndex + menuItemsNum + 1) % menuItemsNum].description, 12, 100, BLACK);     
    leftButtonPressed = false;
  }
  rightButton.update();
  leftButton.update();
  printTftText(currentMenu[currentMenuIndex].description, 12, 100);
}

void drawRightButton(String label) { drawRightButton(label, WHITE, RED); }
void drawRightButton(String label, uint16_t tColor) { drawRightButton(label, tColor, RED); }
void drawRightButton(String label, uint16_t tColor, uint16_t bColor)
{
  //Red button
  tft.setTextSize(2);
  tft.fillRect(242, 210, 78, 32, bColor);
  //Half of the width of the label: label.length * 6 * textSize / 2
  //Middle of the button = 142 + width/2 = 281
  tft.setCursor(281 - label.length() * 6 , 218);
  tft.setTextColor(tColor);
  tft.print(label);
}


void drawLeftButton(String label) { drawLeftButton(label, WHITE, BLUE); }
void drawLeftButton(String label, uint16_t tColor) { drawLeftButton(label, tColor, BLUE); }
void drawLeftButton(String label, uint16_t tColor, uint16_t bColor)
{
  //Red button
  tft.setTextSize(2);
  tft.fillRect(0, 210, 78, 32, bColor);
  tft.setCursor(39 - label.length() * 12 / 2 , 218);
  tft.setTextColor(tColor);
  tft.print(label);
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
  int currentSegment = 1;
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

  drawLeftButton("-" + String(incValues[currentSegment - 1], decimals), WHITE, RED);
  drawRightButton("+" + String(incValues[currentSegment - 1], decimals), WHITE, BLUE);

  //Current value
  printTftText("Current:", 100, 174);
  printTftValue(value, 236, 174, 2, 0);
  cenButton.update();
  rightButton.update();
  leftButton.update();
  
  while (!exitFlag) {

    //Selecting the force goal
    //TODO: Allow coninuous increasing by keeping pressed the button
    if (rightButton.fell()) {
      printTftValue(value, 236, 174, 2, decimals, BLACK);
      
      value += incValues[currentSegment - 1];
      if (abs(value -  rangesValues[rangesNum] - incValues[currentSegment - 1]) < 0.0001) {
        printTftValue(value, 236, 174, 2, decimals, BLACK);
        value = rangesValues[0];
        currentSegment = 1;
        drawLeftButton("+" + String(incValues[currentSegment - 1], decimals), WHITE, RED);
      }
      //Sometimes float values are not exatcly the expected one
      if (abs(value - rangesValues[currentSegment]) < 0.0001)
      {
        currentSegment++;
        drawLeftButton("-" + String(incValues[currentSegment - 1], decimals), WHITE, RED);
        drawRightButton("+" + String(incValues[currentSegment - 1], decimals), WHITE, BLUE);
      }
      printTftValue(value, 236, 174, 2, decimals);
    }
    
    //TODO: Allow coninuous increasing by keeping pressed the button
    if (leftButton.fell()) {
      printTftValue(value, 236, 174, 2, decimals, BLACK);
      
      value -= incValues[currentSegment - 1];
      if (abs(value -  rangesValues[rangesNum] - incValues[currentSegment - 1]) < 0.0001) {
        printTftValue(value, 236, 174, 2, decimals, BLACK);
        value = rangesValues[0];
        currentSegment = 1;
        drawLeftButton("-" + String(incValues[currentSegment - 1], decimals), WHITE, RED);
        drawRightButton("+" + String(incValues[currentSegment - 1], decimals), WHITE, BLUE);
        //Meter aquí el "- " y mismo decrecimiento
      }
      //Sometimes float values are not exatcly the expected one
      if (abs(value - rangesValues[currentSegment]) < 0.0001)
      {
        currentSegment++;
        drawLeftButton("+" + String(incValues[currentSegment - 1], decimals), WHITE, BLUE);
      }
      if (value < 0 || value == 0) {
        value = 0;
      }
      printTftValue(value, 236, 174, 2, decimals);
    }

    //Change to Calibrate execution
    if (cenButton.fell()) {
      //Deleting explanation
      tft.fillRect(0, 60, 320, 240, BLACK);

      submenu = 1;
      exitFlag = true;
    }
    //Waiting the red button push to start calibration process
    cenButton.update();
    rightButton.update();
    leftButton.update();

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

  uint16_t redBackRect[78 * 32];
  tft.readRect(242, 210, 78, 32, redBackRect);
  drawRightButton("Yes");

  uint16_t blueBackRect[78 * 32];
  tft.readRect(0, 210, 78, 32, blueBackRect);
  drawLeftButton("No");

  printTftText(message, x, y, RED);
  cenButton.update();
  while ( !cenButton.fell() && !rightButton.fell() )
  {
    cenButton.update();
    rightButton.update();
  }
  printTftText(message, x, y, BLACK);
  tft.writeRect(x, y, w, h, textBackRect);
  tft.writeRect(242, 210, 78, 32, redBackRect);
  tft.writeRect(0, 210, 78, 32, redBackRect);
  answer = cenButton.fell();
  cenButton.update();
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
