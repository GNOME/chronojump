
//Manages the current menu
void showMenu()
{
  //The blue button navigates through the Menu options
  blueButton.update();
  if (blueButton.fell()) {
    currentMenuIndex++;
    currentMenuIndex = currentMenuIndex % menuItemsNum;
    showMenuEntry(currentMenuIndex);
  }

  //The red button activates the menu option
  redButton.update();
  if (redButton.fell())
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
  drawLeftButton("Next", WHITE, BLUE);
  drawRightButton("Accept", WHITE, RED);
}

//Set the currentMenu to systemMenu and shows it
void showSystemMenu(void)
{
  drawMenuBackground();
  currentMenuIndex = 0;
  for (int i = 0; i < 10; i++) {
    currentMenu[i].title = systemMenu[i].title;
    currentMenu[i].description = systemMenu[i].description;
    currentMenu[i].function = systemMenu[i].function;
  }
  menuItemsNum = systemMenuItems;
  showMenuEntry(currentMenuIndex);
  //showMenu();
}

//shows the current entry of the current menu
void showMenuEntry(unsigned int currentMenuIndex)
{
  tft.fillRect(30, 0, 260, 50, BLACK);
  tft.setCursor(40, 20);
  tft.setTextSize(3);
  tft.print(currentMenu[currentMenuIndex].title);

  tft.setTextSize(2);
  tft.setCursor(12, 100);
  tft.setTextColor(BLACK);
  tft.print(currentMenu[(currentMenuIndex + menuItemsNum - 1) % menuItemsNum].description);
  tft.setTextColor(WHITE);
  tft.setCursor(12, 100);
  tft.print(currentMenu[currentMenuIndex].description);
}

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
  tft.setTextColor(WHITE);
  tft.setCursor(10, 112);
  tft.print(description);

  //Blue button
  drawLeftButton("+" + String(incValues[0], decimals), WHITE, BLUE);

  //Red button
  drawRightButton("Accept", WHITE, RED);

  //Current value
  tft.setCursor(100, 174);
  tft.setTextColor(WHITE, BLACK);
  tft.print("Current:");
  tft.setCursor(220, 174);
  printTftValue(value, 236, 174, 2, 0);
  redButton.update();
  blueButton.update();
  
  while (!exitFlag) {

    //Selecting the force goal
    //TODO: Allow coninuous increasing by keeping pressed the button
    if (blueButton.fell()) {
      tft.setTextColor(BLACK);
      printTftValue(value, 236, 174, 2, decimals);
      
      value += incValues[currentSegment - 1];
      if (abs(value -  rangesValues[rangesNum] - incValues[currentSegment - 1]) < 0.0001) {
        tft.setTextColor(BLACK);
        printTftValue(value, 236, 174, 2, decimals);
        value = rangesValues[0];
        currentSegment = 1;
        drawLeftButton("+" + String(incValues[currentSegment - 1], decimals), WHITE, BLUE);
      }
      //Sometimes float values are not exatcly the expected one
      if (abs(value - rangesValues[currentSegment]) < 0.0001)
      {
        currentSegment++;
        drawLeftButton("+" + String(incValues[currentSegment - 1], decimals), WHITE, BLUE);
      }
      tft.setTextColor(WHITE);
      tft.setCursor(216, 150);
      printTftValue(value, 236, 174, 2, decimals);
    }

    //Change to Calibrate execution
    if (redButton.fell()) {

      //Deleting explanation
      tft.fillRect(0, 60, 320, 240, BLACK);

      submenu = 1;
      exitFlag = true;
    }
    //Waiting the red button push to start calibration process
    redButton.update();
    blueButton.update();
  }
  return (value);
}

void selectJumpType()
{
  tft.fillScreen(BLACK);
  tft.setCursor(40, 20);
  tft.setTextSize(3);
  tft.print("Jump type");
  
  drawLeftButton("Next", WHITE, BLUE);
  drawRightButton("Accept", WHITE, RED);

  tft.setTextSize(2);
  tft.setCursor(50, 100);
  tft.print(jumpTypes[currentJumpType].name);

  
  blueButton.update();
  redButton.update();
  while(!redButton.fell())
  {
    if(blueButton.fell())
    {      
      //Deleting last jumpType text
      tft.setCursor(50, 100);
      tft.setTextColor(BLACK);
      tft.print(jumpTypes[currentJumpType].name);

      //Printing new jump type text
      tft.setCursor(50, 100);
      tft.setTextColor(WHITE);
      currentJumpType = (currentJumpType + 1) % totalJumpTypes;
      tft.print(jumpTypes[currentJumpType].name);
    }
    blueButton.update();
    redButton.update();
  }
}
