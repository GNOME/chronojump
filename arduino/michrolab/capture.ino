void startLoadCellCapture(void)
{
  // Serial.println("<startLoadCellCapture");

  //Check that calibration_factor is set
  if (scale.get_scale() == 0) {
    Serial.println("Not a previous calibration");
    return;
  }

  attachInterrupt(rcaPin, changedRCA, CHANGE);
  scale.power_up();
  totalTime = 0;
  measuredMax = scale.get_units();
  impulse = 0;
  maxRFD100 = 0;
  maxRFD200 = 0;

  //filling the array of forces ant times with initial force
  lastMeasure = scale.get_units();
  measured = lastMeasure;
  for (unsigned int i = 0; i < freq; i++) {
    forces1s[i] = lastMeasure;
  }

  for (unsigned int i = 0; i < samples200ms; i++) {
    totalTimes1s[i] = 0;
  }

  maxMeanForce1s = lastMeasure;

  //Initializing variability variables
  sumSSD = 0.0;
  sumMeasures = lastMeasure;
  samplesSSD = 0;
  sensor = loadCell;
  maxString = "F";
  plotPeriod = 5;
  if (capturingSteadiness) {
    newGraphMin = -10;
    newGraphMax = forceGoal * 1.5;
  } else if (capturingSteadiness)  {
    newGraphMin = forceGoal * 0.5;
    newGraphMax = forceGoal * 1.5;
  } else {
    newGraphMin = -100;
    newGraphMax = max(100, measuredMax * 1.5);
  }

  currentConfigSetMenu = personSelect;

  while( currentConfigSetMenu != quit && currentConfigSetMenu != capture ) {
    if( currentConfigSetMenu == personSelect) {
      selectPersonDialog();
      } else if( currentConfigSetMenu == exerciseSelect) {

        if (totalForceTypes == 0) readExercisesFile(force);

        selectExerciseType(force);

        //Checking the reason of exit from electExerciseType(force)
        if (prevConfigSetMenu) {
          Serial.println("-");
          prevConfigSetMenu = false;
          currentConfigSetMenu = personSelect;
        }
        if (nextConfigSetMenu) {
          nextConfigSetMenu = false;
          currentConfigSetMenu = capture;
        }
      }

    if( currentConfigSetMenu == quit) {
      Serial.println("Returning");
      drawMenuBackground();
      showMenuEntry(currentMenuIndex);
      //Serial.println("startLoadCellCapture (return)>");
      return;
    };

    if(forceTypes[currentExerciseType].tare)
    {
      tft.fillScreen(BLACK);
      printTftText(currentMenu[currentMenuIndex].description, 12, 100, 2);
      printTftText("Taring...", 100, 100);

      drawLeftButton("-", BLACK, BLACK);
      drawRightButton("-", BLACK, BLACK);

      
      scale.tare(50); //Reset the scale to 0 using the mean of 255 raw values
      printTftText("Taring...", 100, 100, BLACK);
      printTftText("  Tared  ", 100, 100);
      delay(300);
    }
  }
  if(currentConfigSetMenu == capture) {
    capturing = true;
    Serial.println("Starting capture...");
  } else {
    currentConfigSetMenu = personSelect;
  }

  // Serial.println("startLoadCellCapture>");
}

void endLoadCellCapture()
{
  detachInterrupt(rcaPin);
  capturing = false;
  sensor = none;
  Serial.println("Capture ended:");

  //If the device is controlled by the PC the results menu is not showed
  //because during the menu navigation the Serial is not listened.
  if (!PcControlled) {
    //Restoring tare value in the EEPROM. Necessary after Tare&Capture
    EEPROM.get(tareAddress, tareValue);
    scale.set_offset(tareValue);
    //Serial.println(scale.get_offset());
    showLoadCellResults();
  }
  scale.power_down();
  drawMenuBackground();
  showMenuEntry(currentMenuIndex);
}


/*
void startTareCapture(void)
{

  printTftText(currentMenu[currentMenuIndex].description, 12, 100, 2);
  printTftText("Taring...", 100, 100);
  scale.tare(50); //Reset the scale to 0 using the mean of 255 raw values
  printTftText("Taring...", 100, 100, BLACK);
  printTftText("  Tared  ", 100, 100);
  delay(300);
  startLoadCellCapture();
}
*/


//Any change in the RCA activates the timer
void changedRCA() {
  rcaTime = totalTime;
  rcaState = digitalRead(rcaPin);
  rcaTimer.begin(rcaDebounce, rcaDebounceTime);
  if (sensor == raceAnalyzer)
  {
    raceAnalyzerSample.displacement = encoder.read();
    encoder.write(0);
  }
  //Serial.print("-");
}

//After the debounce time the state of the RCA is checked again to see if it has changed
void rcaDebounce()
{
  rcaTimer.end();
  rcaState = !digitalRead(rcaPin);
  if (rcaState != lastRcaState)
  {
    lastRcaState = rcaState;
    rcaFlag = true;
  }
}

void updateJumpTime()
{
  updateTime(0, jumpTypes[currentExerciseType].timeLimit);
}

//Shows time in seconds at right lower corner. If limit is present non-zero it will we a countdown
void updateTime() {
  updateTime( 0, 0.0 );
}
void updateTime( unsigned int decimals ) {
  updateTime( decimals, 0.0 );
}
void updateTime( unsigned int decimals, float limit )
{
  tft.fillRect(268, 215, 48, 16, BLACK);
  //  if (totalTime > 1000000)
  //  {
  //    tft.setTextColor(BLACK);
  //    printTftValue(totalTime / 1000000 - 1, 302, 215, 2, 0);
  //  }
  if (limit == 0) {
    printTftValue(totalTime / 1000000, 302, 215, 2, decimals);
  }
  else if (limit != 0) {
    printTftValue(limit - totalTime / 1000000, 302, 215, 2, decimals);
  }
}


void showLoadCellResults() {
  int textSize = 2;
  tft.fillScreen(BLACK);
  printTftText("Results", 100, 100, 3, BLACK);

  tft.drawLine(0, 20, 320, 20, GREY);
  tft.drawLine(160, 240, 160, 20, GREY);
  tft.setTextSize(textSize);

  printTftText("F", 0, 40);
  printTftText("max", 12, 48, WHITE, 1);
  printTftValue(measuredMax, 112, 40, textSize, 0);

  printTftText("F", 170, 40);
  printTftText("max1s", 182, 48, WHITE, 1);

  printTftValue(maxMeanForce1s, 296, 40, textSize, 0);

  printTftText("F", 0, 80);
  printTftText("trig", 12, 88, WHITE);

  printTftValue(forceTrigger, 100, 80, textSize, 1);

  printTftText("Imp", 170, 80);
  printTftValue(impulse, 296, 80, textSize, 0);

  printTftText("RFD", 0, 120);
  printTftText("100", 36, 128, WHITE, 1);
  printTftValue(maxRFD100, 118, 120, textSize, 0);

  printTftText("RFD", 170, 120);

  printTftText("200", 206, 128, WHITE, 1);
  printTftValue(maxRFD200, 298, 120, textSize, 0);

  if (RMSSD != 0)
  {
    printTftText("RMSSD", 0, 160);
    printTftValue(RMSSD, 100, 160, textSize, 1);

    printTftText("CV", 170, 160);
    printTftText("RMSSD", 194, 168, WHITE, 1);
    printTftValue(cvRMSSD, 280, 160, textSize, 1);
  }

  //Red button exits results
  cenButton.update();
  while (!cenButton.fell()) {
    cenButton.update();
  }
  tft.fillRect(0, 20, 320, 240, BLACK);
  drawMenuBackground();
}

void startSteadiness(void)
{
  /* Cambiado por Mon 
  if (!selectExerciseType(jumps) || !selectExerciseType(inertial) || !selectExerciseType(force) || !selectExerciseType(encoderRace)) 
  {
    return;
  }
  */
  sensor = loadCell;
  totalTime = 0;
  capturing = true;
  capturingPreSteadiness = true;
  capturingSteadiness = false;
  startLoadCellCapture();
}

void end_steadiness()
{
  capturing = false;
  capturingSteadiness = false;
}

void captureRaw()
{
  writeCaptureHeaders();

  //Position graph's lower left corner.
  double graphX = 30;
  double graphY = 200;

  //Size of the graph
  double graphW = 290;
  double graphH = 200;

  //Minimum and maximum values to show
  double xMin = 0;
  double xMax = 290;

  //Size and num of divisions
  double yDivSize = 100.0;
  double yDivN = 10.0;
  double xDivSize = 100.0;
  double yBuffer[320];
  yBuffer[0] = measured;

  double plotBuffer[plotPeriod];

  bool resized = true;

  long lastUpdateTime = 0;

  tft.fillScreen(BLACK);

  double xGraph = 1;

  printTftText(maxString, 10, 215, WHITE, 2);
  printTftText("max", 22, 223, WHITE, 1);
  printTftText(":", 40, 215, 2, WHITE);
  printTftValue(measuredMax, 94, 215, 2, 1);
  if (! PcControlled)
  {
    updatePersonSet();
  }

  while (capturing)
  {
    //Deleting the previous plotted points
    for (int i = xMin; i < xGraph; i++)
    {
      Graph(tft, i, yBuffer[i], graphX, graphY, graphW, graphH, xMin, xMax, xDivSize, graphMin, graphMax, yDivSize, "", "", "", WHITE, WHITE, BLACK, WHITE, BLACK, startOver);
    }
    startOver = true;
    //redrawAxes(tft, graphX, graphY, graphW, graphH, xMin, xMax, graphMin, graphMax, yDivSize, "", "", "", WHITE, BLACK, BLACK, BLACK, BLACK, BLACK, resized);
    graphMax = newGraphMax;
    graphMin = newGraphMin;
    yDivSize = (graphMax - graphMin) / yDivN;
    if (resized) {
      for (int i = xMin; i < xGraph; i++)
      {
        Graph(tft, i, yBuffer[i], graphX, graphY, graphW, graphH, xMin, xMax, xDivSize, graphMin, graphMax, yDivSize, "", "", "", WHITE, WHITE, BLUE, WHITE, BLACK, startOver);
      }
    }
    redrawAxes(tft, graphX, graphY, graphW, graphH, xMin, xMax, graphMin, graphMax, yDivSize, "", "", "", WHITE, GREY, WHITE, WHITE, BLACK, RED, resized);
    resized = false;

    if (xGraph >= xMax) xGraph = 0;
    while ((xGraph < xMax && !resized) && capturing)
    {
      for (int n = 0; n < plotPeriod; n++)
      {
        //Checking the RCA state
        if (rcaState != lastRcaState) {       //Event generated by the RCA
          Serial.print(rcaTime);
          Serial.print(";");

          if (rcaState) {
            Serial.println("R");
            forceTrigger = measured;
          } else {
            Serial.println("r");
          }
          lastRcaState = rcaState;

          //If no RCA event, read the sensor as usual
        } else {
          //Calculation of the variables shown in the results
          if (sensor == incLinEncoder || sensor == incRotEncoder ) getEncoderDynamics();
          else if (sensor == loadCell) getLoadCellDynamics();
          else if (sensor == loadCellincEncoder) getPowerDynamics();
          else if (sensor == raceAnalyzer) getRaceAnalyzerDynamics();

          //Value exceeds the plotting area
          if (measured > newGraphMax) {
            newGraphMax = measured + (graphMax - graphMin) * 0.5;
            resized = true;
          }
          if (measured < newGraphMin) {
            newGraphMin = measured - (graphMax - graphMin) * 0.5;
            resized = true;
          }
        }

        //        Serial.print(totalTime); Serial.print(";");
        //        Serial.println(measured, 2); //scale.get_units() returns a float

        if (!PcControlled) saveData(fileName);
        plotBuffer[n] = measured;

        //Pressing blue or red button ends the capture
        //Check the buttons state
        cenButton.update();
        rightButton.update();
        if (cenButton.fell())
        {
          n = plotPeriod;
          if (sensor == incLinEncoder || sensor == incRotEncoder)
          {
            endEncoderCapture();
          } else if (sensor == loadCell)
          {
            if (! (capturingPreSteadiness || capturingSteadiness))
            {
              endLoadCellCapture();
              //xGraph = xMax;
            } else if (capturingPreSteadiness)  //In Pre steadiness. Showing force until button pressed
            {
              capturingPreSteadiness = false;
              capturingSteadiness = true;
              printTftValue(totalTime / 1000000, 284, 215, 2, 0, BLACK);
              redrawAxes(tft, graphX, graphY, graphW, graphH, xMin, xMax, graphMin, graphMax, yDivSize, "", "", "", WHITE, BLACK, BLACK, BLACK, BLACK, BLACK, resized);
              startLoadCellCapture();
              newGraphMax = forceGoal * 1.5;
              newGraphMin = forceGoal * 0.5;
              resized = true;
              //              Serial.println("going to change. Future Y limits:");
              //              Serial.print(newGraphMin);
              //              Serial.print(",\t");
              //              Serial.println(newGraphMax);
              redrawAxes(tft, graphX, graphY, graphW, graphH, xMin, xMax, graphMin, graphMax, yDivSize, "", "", "", WHITE, GREY, WHITE, WHITE, BLACK, RED, resized);
              tft.setCursor(80, 10);
              tft.setTextColor(WHITE, RED);
              tft.print("Hold force  5s");
              tft.setTextColor(WHITE);
            }

          } else if (sensor == loadCellincEncoder) {
            endPowerCapture();
          } else if (sensor == raceAnalyzer) {
            endRaceAnalyzerCapture();
          }
          //xGraph = xMax;
        }
        if (rightButton.fell() && !PcControlled)
        {
          selectPerson();
        }
      }
      //      Serial.println("Ended plotPeriod");

      if (capturing)
      {
        yBuffer[(int)xGraph] = 0;

        for (int i = 0; i < plotPeriod; i++)
        {
          yBuffer[(int)xGraph] = yBuffer[(int)xGraph] + plotBuffer[i];
        }

        yBuffer[(int)xGraph] = yBuffer[(int)xGraph] / plotPeriod;
        Graph(tft, xGraph, yBuffer[(int)xGraph], graphX, graphY, graphW, graphH, xMin, xMax, xDivSize, graphMin, graphMax, yDivSize, "", "", "", WHITE, WHITE, BLUE, WHITE, BLACK, startOver);
        xGraph++;
        if (measured > measuredMax)
        {
          printTftValue(measuredMax, 94, 215, 2, 1, BLACK);
          measuredMax = measured;
          printTftValue(measuredMax, 94, 215, 2, 1);
        }

        if ((lastUpdateTime - totalTime) >= 1000000) {
          lastUpdateTime = totalTime;
          updateTime();
        }
      }
      if (Serial.available()) serialEvent();
    }
  }
  dataFile.close();
  if (!capturingPreSteadiness) setNumber++;
}

void captureBars() {
  captureBars(false);
}
void captureBars(float fullScreen)
{
  writeCaptureHeaders();
  maxString = "V";
  float graphRange = 5;
  int currentSlot = 0;

  tft.fillScreen(BLACK);

  float h = 0;
  if (fullScreen) h = 240;
  else h = 200;

  if (!fullScreen)
  {
    //Info at the lower part of the screen
    printTftText(maxString, 10, 215, WHITE, 2);
    printTftText("max", 22, 223, WHITE, 1);
    printTftText(":", 40, 215, WHITE, 2);
    printTftValue(maxAvgVelocity, 94, 215, 2, 1);
    updatePersonSet(false);
  }

  redrawAxes(tft, 30, h, 290, h, 290, h, 0, graphRange, graphRange / 10, "", "", "", WHITE, GREY, WHITE, WHITE, BLACK, RED, true, 1);

  while (capturing)
  {
    getEncoderDynamics();
    if (redrawBars)
    {
      currentSlot = (numRepetitions - 1) % 10;
      redrawBars = false;
      if (bars[currentSlot] > maxAvgVelocity)
      {
        if (!fullScreen) printTftValue(maxAvgVelocity, 94, 215, 2, 1, BLACK);
        maxAvgVelocity = bars[currentSlot];
        if (!fullScreen) printTftValue(maxAvgVelocity, 94, 215, 2, 1);
      }
      if (bars[currentSlot] > graphRange)
      {
        redrawAxes(tft, 30, h, 290, h, 290, h, 0, graphRange, graphRange / 10, "", "", "", WHITE, GREY, WHITE, WHITE, BLACK, RED, true, 1);
        graphRange = bars[currentSlot] * 1.25;
      }
      barPlot(30, h, 290, h, graphRange, 5, currentSlot, 0.75, RED);
    }
    cenButton.update();
    if (cenButton.fell())
    {
      endEncoderCapture();
    }
  }
}

void writeCaptureHeaders()
{
  fileName = "S" + addLeadingZeros(setNumber, 2) + "P" + addLeadingZeros(currentPerson, 2);
  if (sensor == loadCell) fileName = fileName + "-F";
  else if (sensor == incLinEncoder) fileName = fileName + "-G";
  else if (sensor == incRotEncoder) fileName = fileName + "-I";
  else if (sensor == loadCellincEncoder) fileName = fileName + "-P";
  else if (sensor == raceAnalyzer) fileName = fileName + "-R";

  fullFileName = "/" + dirName + "/" + fileName + ".TXT";
  Serial.println(fullFileName);
  dataFile = SD.open(fullFileName.c_str(), FILE_WRITE);
  dataFile.println("Date:" + String(year()) + "/" + month() + "/" + day() + " " + hour() + ":" + minute() + ":" + second());
  dataFile.println("Person:" + String(persons[currentPerson].index) + "," + persons[currentPerson].name + " " + persons[currentPerson].surname);

  if (sensor == incLinEncoder)
  {
    dataFile.println("Exercise:" + String(gravTypes[currentExerciseType].id) + "," + gravTypes[currentExerciseType].name);
    dataFile.println("Load:" + String(load));
  } else if (sensor == incRotEncoder)
  {
    dataFile.println("Exercise:" + String(inertTypes[currentExerciseType].id) + "," + inertTypes[currentExerciseType].name);
    dataFile.println("Load:" + String(load));
  } else if (sensor == loadCell)
  {
    dataFile.println("Exercise:" + String(forceTypes[currentExerciseType].id) + "," + forceTypes[currentExerciseType].name);
  } else if (sensor == raceAnalyzer)
  {
    dataFile.println("Exercise:" + String(raceAnalyzerTypes[currentExerciseType].id) + "," + raceAnalyzerTypes[currentExerciseType].name);
  }
}

//text mode
// void saveEncoderSpeed()
// {
//   long position = encoder.read();
//   String currentValue = String(position - lastSamplePosition) + ",";

//   if (PcControlled) {
//     Serial.print(currentValue);
//   } else if (!PcControlled) {
//     fileBuffer = fileBuffer + currentValue;
//     sampleNum++;
//     if (sampleNum >= 50) {
//       dataFile.print(fileBuffer);
//       //Serial.println(fileBuffer);
//       fileBuffer = "";
//       sampleNum = 0;
//     }
//   }
//   lastSamplePosition = position;
// }


  //binary mode
  void saveEncoderSpeed()
  {
    // Serial.println("<saveEncoderSpeed");
    long position = encoder.read();
    binFileBuffer[sampleNum] =(char)(position - lastSamplePosition)/4;
    sampleNum++;
    lastSamplePosition = position;
    if (sampleNum >= 50){
      dataFile.write(binFileBuffer, 50);
      sampleNum = 0;
    }
    // Serial.println("saveEncoderSpeed>");
  }

void getEncoderDynamics()
{
  int duration = totalTime - lastMeasuredTime;
  if (duration >= 1000)
  {
    lastMeasuredTime = totalTime;

    long position = encoder.read();

    //TODO: Calculate positoion depending on the parameters of the encoder/machine
    if (inertialMode) position = - abs(position);
    measured = (float)(position - lastPosition) * 1000 / (duration);
    if (measured > measuredMax) measuredMax = measured;
    //measured = position;
    //    if(position != lastPosition) Serial.println(String(localMax) + "\t" + String(lastPosition) +
    //      "\t" + String(position) + "\t" + String(encoderPhase * (position - localMax)));
    float accel = (measured - lastVelocity) * 1000000 / duration;
    if (propulsive && accel <= -9.81) {
      //Serial.println("End propulsive at time: " + String(lastMeasuredTime));
      propulsive = false;
    }
    //measured = position;
    //if (measured != 0) Serial.println(measured);
    //Before detecting the first repetition we don't know the direction of movement
    if (encoderPhase == 0)
    {
      if (position >= minRom) {
        encoderPhase = 1;
        localMax = position;
        //Serial.println("Start in CONcentric");
      }
      else if (position <= -minRom) {
        encoderPhase = 1;
        localMax = position;
        //Serial.println("Start in ECCentric");
      }
    }

    //Detecting the phanse change
    //TODO. Detect propulsive phase
    if (encoderPhase * (position - localMax) > 0)  //Local maximum detected
    {
      //Serial.println("New localMax : " + String(position) + "\t" + String(localMax));
      localMax = position;
    }

    //Checking if this local maximum is actually the start of the new phase
    if (encoderPhase * (localMax - position) > minRom)
    {
      encoderPhase *= -1;
      propulsive = true;
      avgVelocity = (float)(position - startPhasePosition) * 1000 / (lastMeasuredTime - startPhaseTime);
      bars[numRepetitions % 10] = abs(avgVelocity);
      redrawBars = true;

      //printBarsValues();

      numRepetitions++;
      if (avgVelocity > maxAvgVelocity) maxAvgVelocity = avgVelocity;
      //        Serial.println(String(position) + " - " + String(startPhasePosition) + " = " + String(position - localMax) + "\t" + String(encoderPhase));
      //        Serial.println("Change of phase at: " + String(lastMeasuredTime));
      //        Serial.print(String(1000 * (float)(position - startPhasePosition) / (lastMeasuredTime - startPhaseTime)) + " m/s\t" );
      //        Serial.println(String(1000*(persons[currentPerson].weight * 9.81 * (position - startPhasePosition)) /
      //        (lastMeasuredTime - startPhaseTime))+" W");
      startPhasePosition = position;
      startPhaseTime = lastMeasuredTime;
    }
    lastPosition = position;
    lastVelocity = measured;
    //Serial.println(String(measured) + "\t" + String(accel));
  }
}

void startInertEncoderCapture()
{

  /*
  if (!selectExerciseType(inertial)) 
  {
    return;
  } 
  */ 

  if (!calibratedInertial) calibrateInertial();
  inertialMode = true;
  sensor = incRotEncoder;
  
  if(totalInertTypes == 0) readExercisesFile(inertial);
  selectExerciseType(inertial);
  load = selectValueDialog("Select the amount of extra loads attached to the machine", "0,18", "1", 0);
  startEncoderCapture();
}

void startGravitEncoderCapture()
{
  /* Cambiado por Mon
  if (!selectExerciseType(jumps) || !selectExerciseType(inertial) || !selectExerciseType(force) || !selectExerciseType(encoderRace)) 
  {
    return;
  }
  */
  inertialMode = false;
  sensor = incLinEncoder;
  if(totalGravTypes == 0) readExercisesFile(gravitatory);

  selectPersonDialog();
  selectExerciseType(gravitatory);
  load = selectValueDialog("Select the load you are\ngoing to move", "0,5,20,200", "0.5,1,5", 1);
  startEncoderCapture();
}

void startEncoderCapture(void)
{
  attachInterrupt(rcaPin, changedRCA, CHANGE);
  capturing = true;
  //Serial.println(sensor);
  maxString = "V";
  plotPeriod = 1;
  newGraphMin = -10;
  newGraphMax = 10;
  measuredMax = 0;
  measured = 0;
  totalTime = 0;
  encoderPhase = 0;
  localMax = 0;
  //encoder.write(0);
  lastPosition = 0;
  lastMeasuredTime = 0;
  startPhasePosition = 0;
  startPhaseTime = 0;
  avgVelocity = 0;
  maxAvgVelocity = 0;
  lastVelocity = 0;
  //captureRaw();
  encoderTimer.begin(saveEncoderSpeed, 10000);
  captureBars(false);
}

void endEncoderCapture()
{
  detachInterrupt(rcaPin);
  capturing = false;
  encoderTimer.end();
  numRepetitions = ceil((float)(numRepetitions / 2));
  sensor = none;
  Serial.println("Capture ended:");
  dataFile.close();
  //If the device is controlled by the PC the results menu is not showed
  //because during the menu navigation the Serial is not listened.
  tft.fillScreen(BLACK);
  if (!PcControlled) {
    showEncoderResults();
  }
  showMenuEntry(currentMenuIndex);
}

void showEncoderResults()
{
  resultsBackground();
  printTftText("V", 0, 40);
  printTftText("peak", 12, 48, WHITE, 1);
  printTftValue(measuredMax, 100, 40, 2, 1);

  printTftText("Vrep", 170, 40, WHITE);
  printTftText("max", 218, 48, WHITE, 1);
  printTftValue(maxAvgVelocity, 268, 40, 2, 2);

  printTftText("nRep", 0, 80);
  printTftValue(numRepetitions, 100, 80, 2, 0);

  cenButton.update();
  while (!cenButton.fell()) {
    cenButton.update();
  }
  tft.fillRect(0, 20, 320, 240, BLACK);
  drawMenuBackground();
}

void getPowerDynamics()
{
  float force = scale.get_units();
  //position = encoder.read();
  readEncoder();
  encoderBufferIndex = (encoderBufferIndex + 1) % 20;
  float velocity = (float)(position - lastPosition) * 1000 / (totalTime - lastSampleTime);
  lastSampleTime = totalTime;
  encoderString = "";
  measured = force * velocity;
  Serial.println(measured);
  if (measured > maxPower) maxPower = measured;
}

void startPowerCapture(void)
{
  attachInterrupt(rcaPin, changedRCA, CHANGE);
  scale.power_up();
  capturing = true;
  sensor = loadCellincEncoder;
  maxString = "P";
  plotPeriod = 5;
  newGraphMin = -200;
  newGraphMax = 500;
  measuredMax = 0;
  totalTime = 0;

  //Depending on the speed of the clock it can be adjusted
  //96 Mhz and 1000 us captures but the screen refreshing becomes unstable
  //72 Mhz and 2000 us captures but the screen refreshing becomes unstable
  encoderTimer.begin(readEncoder, 2000);
  captureRaw();
}

void readEncoder()
{
  lastPosition = position;
  position = encoder.read();
  //  encoderString = encoderString + String(position - lastPosition) + ",";
  encoderBuffer[encoderBufferIndex] = position - lastPosition;
}

void endPowerCapture()
{
  detachInterrupt(rcaPin);
  capturing = false;
  encoderTimer.end();
  sensor = none;
  Serial.println("Capture ended:");
  //If the device is controlled by the PC the results menu is not showed
  //because during the menu navigation the Serial is not listened.
  if (!PcControlled) {
    showPowerResults();
  }
  showMenuEntry(currentMenuIndex);
}

void showPowerResults()
{
  resultsBackground();
  printTftText("P", 0, 40, 2, WHITE);
  printTftText("peak", 12, 48, 2, WHITE);
  printTftValue(measuredMax, 100, 40, 2, 1);

  cenButton.update();
  while (!cenButton.fell()) {
    cenButton.update();
  }
  tft.fillRect(0, 20, 320, 240, BLACK);
  drawMenuBackground();
}

void jumpCapture()
{
  attachInterrupt(rcaPin, changedRCA, CHANGE);
  if (totalJumpTypes == 0) readExercisesFile(jumps);
  //printJumpTypes();
  //this if is to go back to the main menu and stop the jump items from appearing on it
  if (!selectExerciseType(jumps)) 
  {
    return;
  }
  
  IntervalTimer testTime;             //Timer that controls the refreshing of time in lower right corner
  capturing = true;
  //In the first change of state the header of the row is writen.
  //TODO: MAKE IT SENSITIVE TO THE startIn parameter
  bool waitingFirstPhase = true;
  bool timeEnded = false;             //Used to manage soft time limit. Allows to wait until the last contact
  float maxJump = 0;
  int bestJumper = 0;
  float graphRange = 50;              //Height of the jumps are in cm
  int index = 0;                      //Specify the slot used in bars[] circular buffer
  bool rowCreated = false;

  fileName = "S" + addLeadingZeros(setNumber,2) + "J" + addLeadingZeros(jumpTypes[currentExerciseType].id,2) + "-J";
  fullFileName = "/" + dirName + "/" + fileName + ".TXT";
  dataFile = SD.open(fullFileName.c_str(), FILE_WRITE);

  lastRcaState = !digitalRead(rcaPin);
  rcaFlag = false;
  //lastPhaseTime can be contactTime or flightTime depending on the phase
  float lastPhaseTime = 0;

  //Initializing values of the bars
  for (int i = 0; i < 10; i++)
  {
    bars[i] = 0;
  }

  //Drawing axes
  tft.fillScreen(BLACK);
  redrawAxes(tft, 30, 200, 290, 200, 290, 200, 0, graphRange, graphRange / 10, "", "", "", WHITE, GREY, WHITE, WHITE, BLACK, RED, true);
  cenButton.update();

  updatePersonJump(totalJumps);

  //Print summary results
  printTftText("H:", 10, 215, WHITE, 2);
  printTftValue(maxJump, 58, 215, 2, 2);
  totalTestTime = 0;
  totalTime = 0;

  //Draws the time if necessary
  if ( jumpTypes[currentExerciseType].timeLimit > 0 ) updateJumpTime();

  //Pressing the cenButton during a test ends it
  while ( !cenButton.fell())
  {
    while (capturing && !cenButton.fell())
    {      
      //Person has changed
      if ( rightButton.fell() ) {
        currentPerson = (currentPerson + 1) % totalPersons;
      }
      
      //Person has changed
      if ( leftButton.fell() ) {
        currentPerson = (currentPerson - 1 + totalPersons) % totalPersons;
      }

      if ( rightButton.fell() || leftButton.fell() ) {
        delay(50);
        updatePersonJump(totalJumps);
        waitingFirstPhase = true;
        totalJumps = 0;
        totalTestTime = 0;
        testTime.end();
        totalTime = 0;
        timeEnded = false;
      }
      
      //There's been a change in the mat state. Landing or taking off.
      if (rcaFlag)
      {
        // Serial.println("!");
        rcaFlag = false;
        //Calculate the time of the last phase. Flight or Contact time
        lastSampleTime = rcaTime - lastRcaTime;
        lastPhaseTime = ((float)rcaTime - (float)lastRcaTime) / 1E6;  //Time in seconds
        lastRcaState = rcaState;
        lastRcaTime = rcaTime;

        //If there's been a previous contact it means that this is the start or end of flight time
        if (!waitingFirstPhase) {
          // Serial.println("!waitingFirstPhase");
          dataFile.print(",");
          //Hard coded microsonds precission
          dataFile.print(lastPhaseTime, 6);
          Serial.print(",");
          Serial.print(lastPhaseTime, 6);

          //Stepping on the mat. End of flight time. Starts contact.
          if (rcaState)
          {
            dataFile.print("R");
            Serial.print("R");
            tft.fillRect(30, 0, 290, 200, BLACK);
            bars[index] = 122.6 * lastPhaseTime * lastPhaseTime; //In cm
            //We always add 10 to index to avoid the negative number in (index - 1) when index is 0
            printTftValue(bars[(index + 10 - 1) % 10], 58, 215, 2, 2, BLACK);
            printTftValue(bars[index], 58, 215, 2, 2);
            //Check if a new best jump is performed
            if (bars[index] > maxJump)
            {
              maxJump = bars[index];
              bestJumper = currentPerson;
            }

            if (bars[index] > graphRange)
            {
              redrawAxes(tft, 30, 200, 290, 200, 290, 200, 0, graphRange, graphRange / 10, "", "", "", WHITE, BLACK, WHITE, BLACK, BLACK, RED, true);
              graphRange = bars[index] * 1.25;
            }
            redrawAxes(tft, 30, 200, 290, 200, 290, 200, 0, graphRange, graphRange / 10, "", "", "", WHITE, GREY, WHITE, WHITE, BLACK, RED, true);
            barPlot(30, 200, 290, 200, graphRange, 10, index, 0.5, RED);
            index = (index + 1) % 10;
            totalJumps++;
            updatePersonJump(totalJumps);

            //If soft time limit check if time was ended in the moment of the contact
            if ( timeEnded )
            {
              capturing = false;
              waitingFirstPhase = true;
            }

            //Taking off. Ends contact. start of flight time
          } else if (!rcaState)
          {
            dataFile.print("r");
            Serial.print("r");
          }
          //Waiting first phase.
        } else if (waitingFirstPhase && capturing) {

          //The state  previous to change was WRONG
          //The first change of RCA is in the state that is supposed to be at start of the test.
          if ( jumpTypes[currentExerciseType].startIn == rcaState ) {
            // Serial.println("startIn == rcaState");
            if (rcaState) {             //Landing. Don't measure the Time of Flight
              //Do nothing
            } else if ( !rcaState) {  //Take off.
              //Measure one more jump..
              totalJumps = -1;
              waitingFirstPhase = false;
            }
            //The state previous change was RIGHT
            //The first change of RCA is to the state that is NOT supposed to be at start of the test.
          } else if ( jumpTypes[currentExerciseType].startIn != rcaState) {
            // Serial.println("startIn != rcaState");
            waitingFirstPhase = false;
          }
          totalTestTime = 0;
          totalTime = 0;
          rcaTime = 0;
          lastRcaTime = 0;
          setNumber++;
          if ( !rowCreated ) {
            //Headers of each line
            dataFile.print(String(setNumber) + ","
              + String(currentPerson) + "," + persons[currentPerson].name + " " + persons[currentPerson].surname +
              "," + String(jumpTypes[currentExerciseType].id) + "," + jumpTypes[currentExerciseType].name);
                                    
//            if (PcControlled)
//            {
//              Serial.print(String(setNumber) + ","
//                + String(currentPerson) + "," + persons[currentPerson].name + " " + persons[currentPerson].surname +
//                "," + String(jumpTypes[currentExerciseType].id) + "," + jumpTypes[currentExerciseType].name);
//            }
            rowCreated = true;
          }

          //Starting timer
          if (jumpTypes[currentExerciseType].timeLimit != 0)
          {
            //Hardcoded to show integers and update every second
            testTime.begin(updateJumpTime, 1000000);
            updateJumpTime();
          }
        }

        //Check jumps limit
        if (jumpTypes[currentExerciseType].jumpLimit > 0                //Jumps limit set
            && totalJumps >= jumpTypes[currentExerciseType].jumpLimit)  //Jumps equal or exceeded to limit
          capturing = false;

      } //End of rcaFlag
      //Check time limit
      if ( !waitingFirstPhase
           && !timeEnded                                                                    //Only check once
           && jumpTypes[currentExerciseType].timeLimit > 0                                      //time limit set
           && totalTestTime >= (unsigned int)jumpTypes[currentExerciseType].timeLimit * 1000000) //time limit exceeded
      {
        timeEnded = true;
        //Check if test must end. Hard time limit or soft time limit but sepping on the mat
        if ( jumpTypes[currentExerciseType].hardTimeLimit                         //Hard time limit
             || ( !jumpTypes[currentExerciseType].hardTimeLimit && rcaState ) )   //Soft time limit and in contact with the mat
        {
          capturing = false;
          rcaFlag = false;
        }
      }
      cenButton.update();
      rightButton.update();
      leftButton.update();
    }
    //The current test has ended
    Serial.println();
    dataFile.println();

    waitingFirstPhase = true;
    rcaFlag = false;
    totalJumps = 0;
    totalTestTime = 0;
    testTime.end();
    totalTime = 0;
    timeEnded = false;
    rowCreated = false;

    //check if the user wants to perform another one
    if ( yesNoDialog("Continue with " + jumpTypes[currentExerciseType].name + "?", 40, 100))
    {
      rowCreated = false;
      if (jumpTypes[currentExerciseType].timeLimit > 0) updateJumpTime();
    } else
      break;

    //If the user chooses yes continue and start over in the while(capturing)
    cenButton.update();
    rightButton.update();
    capturing = true;
    //rca may have changed after finishing the test
    rcaFlag = false;
    lastRcaState = rcaState;
  }
  dataFile.close();
  showJumpsResults(maxJump, bestJumper, totalJumps);
  capturing = false;
  drawMenuBackground();
  cenButton.update();
  rightButton.update();
  detachInterrupt(rcaPin);
  showMenuEntry(currentMenuIndex);
}

void showJumpsResults(float maxJump, unsigned int bestJumper, int totalJumps)
{
  resultsBackground();
  tft.drawLine(160, 240, 160, 80, BLACK);
  int textSize = 2;

  printTftText("J", 0, 40, WHITE, 2);
  printTftText("max", 12, 48, WHITE, 1);
  printTftValue(maxJump, 100, 40, textSize, 2);

  printTftText("N", 170, 40, WHITE, 2);
  printTftText("Jumps", 218, 40, WHITE, 1);
  printTftValue(totalJumps, 268, 40, textSize, 0);

  printTftText("Best Jumper: ", 0, 80, WHITE, 2);

  printTftText(persons[bestJumper].name + " " + persons[bestJumper].surname, 12, 100, WHITE, 2);

  cenButton.update();
  while (!cenButton.fell()) {
    cenButton.update();
  }
  tft.fillRect(0, 20, 320, 240, BLACK);
  drawMenuBackground();
}

void saveData(String fileName)
{
  if (sensor == loadCell) {
    Serial.println("Saving loadCell data: " + String(lastSampleTime) + ";" + String(measured));
    dataFile.println(String(lastSampleTime) + ";" + String(measured));
  }
  else if(sensor == raceAnalyzer)
  {
    //Only write on interruption
    if (rcaFlag || encoderFlag)
    {
      encoderFlag = false;
      rcaFlag = false;
      if (!binaryFormat) Serial.println(String(raceAnalyzerSample.totalTime) + ";" + String(raceAnalyzerSample.displacement) + ";" + String(rcaState));
      else Serial.write((byte*)&raceAnalyzerSample, 9);
      if(!PcControlled)
        dataFile.println(String(raceAnalyzerSample.totalTime) + ";" + String(raceAnalyzerSample.displacement) + ";" + String(rcaState));
    }
  }
}

void startRaceAnalyzerCapture()
{
  /* Cambiado por Mon
  if (!selectExerciseType(jumps) || !selectExerciseType(inertial) || !selectExerciseType(force) || !selectExerciseType(encoderRace)) 
  {
    return;
  }
  */
  downButton.update();
  //if (!downButton.fell()) {
  encoderFlag = false;
  rcaFlag = false;
  attachInterrupt(encoderAPin, encoderAChange, CHANGE);
  //attachInterrupt(encoderBPin, encoderBChange, CHANGE);
  attachInterrupt(rcaPin, changedRCA, CHANGE);
  calibratedInertial = false;
  totalTime = 0;
  sensor = raceAnalyzer;
  pps = 40;             //TODO: Manage the PPS by serial commands
  capturing = true;
  maxString = "V";
  plotPeriod = 50;
  newGraphMin = -1;
  newGraphMax = 10;
  measuredMax = 0;
  measured = 0;
  totalTime = 0;

  if(totalRaceAnalyzerTypes == 0) readExercisesFile(encoderRace);
  selectExerciseType(encoderRace);
  
  captureRaw();
  endRaceAnalyzerCapture();
  //}

}

void endRaceAnalyzerCapture()
{
  detachInterrupt(encoderAPin);
  //detachInterrupt(encoderBPin);
  detachInterrupt(rcaPin);
  cenButton.update();
  rightButton.update();
  capturing = false;
  drawMenuBackground();
  showMenuEntry(currentMenuIndex);
}

void encoderAChange()
{
  position = encoder.read();
  if(abs(position) >= pps)
  {
    //Serial.println(raceAnalyzerSample.displacement);
    raceAnalyzerSample.displacement = position;
    currentSampleTime = totalTime;
    encoder.write(0);
    encoderFlag = true;
  }
}

void getRaceAnalyzerDynamics()
{
  if((encoderFlag || rcaFlag))
  {
    if(encoderFlag)
    {
      measured = abs( (float)raceAnalyzerSample.displacement * 1000000 * 0.0030321 /  4 / (float)(currentSampleTime - raceAnalyzerSample.totalTime) );
      raceAnalyzerSample.totalTime = currentSampleTime;
      raceAnalyzerSample.sensor = raceAnalyzer;
    }

    if(rcaFlag)
    {
      raceAnalyzerSample.sensor = rca;
    }
  }
}

