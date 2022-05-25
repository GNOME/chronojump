
//Manages the current menu
void showMenu()
{
    //The blue button navigates through the Menu options
    blueButton.update();
    if (blueButton.fallingEdge()) {
      currentMenuIndex++;
      currentMenuIndex = currentMenuIndex % menuItemsNum;
      showMenuEntry(currentMenuIndex);
    }

    //The red button activates the menu option
    redButton.update();
    if (redButton.fallingEdge())
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
  for (int i = 0; i< 10; i++){
    currentMenu[i].title = mainMenu[i].title;
    currentMenu[i].description = mainMenu[i].description;
    currentMenu[i].function = mainMenu[i].function;
  }
  menuItemsNum = 6;
  showMenuEntry(currentMenuIndex);
  showMenu();
}

//Erases screen and draw the left and right buttons in the upper part of screen
void drawMenuBackground() {
  tft.fillScreen(BLACK);
  tft.fillRoundRect(0, 0, 30, 50, 10, WHITE);
  tft.fillRoundRect(290, 0, 30, 50, 10, WHITE);
  tft.setCursor(30, 20);
}

//Set the currentMenu to systemMenu and shows it
void showSystemMenu(void)
{
  drawMenuBackground();
  currentMenuIndex = 0;
  for (int i = 0; i< 10; i++){
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
