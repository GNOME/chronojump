long powerPressTime = 0;

void goToSleep() {
  systemOn = false;
  powerPressTime = 0;
  delay(10);
  
  for(int i=0; i<=3; i++)
  {
    rgbLeds.setPixelColor(i, BLACK);
  }
  rgbLeds.show();

  // Configure wakeup source - wake on button press (LOW level).
  // TODO: Now it awakes on LOW and HIGH. Check how to avoid release event
  esp_sleep_enable_ext0_wakeup((gpio_num_t)POWER_PIN, LOW);
  
  delay(100);
  Serial.println("Sleeping");
  // Enter deep sleep
  esp_deep_sleep_start();
}

void powerButtonChanged() {

  // If the button is pressed start measuring how long it is pressed
  if ( !digitalRead(POWER_PIN) ) {
    startPowerTimer();
    return;
  }
  //If it reaches this point it is a button release

  // If the system was On it means that the press time was not long enough
  if (systemOn) return;

  // If the system was Off it means that the release is after a poweroff
  goToSleep();
}



void startPowerTimer() {
  powerPressTime = millis();
}

void checkPressTime() {
  // A release while system is on means that the press was not long enough
  if (systemOn) return;
  if ( (millis() - powerPressTime) < 2000 ) {
    goToSleep();
  }
}


void powerConfig() {
  pinMode(POWER_PIN, INPUT_PULLUP);
  attachInterrupt(digitalPinToInterrupt(POWER_PIN), powerButtonChanged, CHANGE);
  // attachInterrupt(digitalPinToInterrupt(POWER_PIN), checkPressTime, RISING);

  esp_sleep_wakeup_cause_t wakeup_reason = esp_sleep_get_wakeup_cause();
  /*// Checking the cause of wake up
  switch (wakeup_reason) {
    case ESP_SLEEP_WAKEUP_EXT0:     Serial.println("Wakeup caused by external signal using RTC_IO");
    case ESP_SLEEP_WAKEUP_EXT1:     Serial.println("Wakeup caused by external signal using RTC_CNTL");
    case ESP_SLEEP_WAKEUP_TIMER:    Serial.println("Wakeup caused by timer");
    case ESP_SLEEP_WAKEUP_TOUCHPAD: Serial.println("Wakeup caused by touchpad");
    case ESP_SLEEP_WAKEUP_ULP:      Serial.println("Wakeup caused by ULP program");
    default:                        Serial.printf("Wakeup was not caused by deep sleep: %d\n", wakeup_reason);
  }
  */

  if (wakeup_reason == ESP_SLEEP_WAKEUP_EXT0) {
    if( digitalRead(POWER_PIN) ) {
      // Serial.println("Button released. Reset power timer");
      powerPressTime = 0;
      goToSleep();
    } else {
      // Serial.println("Button pressed.");
      startPowerTimer();
    }
  }
}

void checkPowerButton() {
  //The power button is not being pressed
  if (powerPressTime == 0) return;
  if (digitalRead(POWER_PIN)) {
    return;
  }
  // Serial.printf("%i System:%d\n", (millis() - powerPressTime), systemOn);
  // delay(10);

  // If the power button keeps pressed check how long it has been pressed
  // As the setup() has a delay of 1 second, this is added to this time for power ON but not for power Off
  if ( (millis() - powerPressTime) < 1000 ) return;

  //Enogh time -> Stop measuring the time
  // Serial.printf("Reset powerPressTime. SystemOn: %b\n", systemOn);
  powerPressTime = 0;
  // if the system is on -> sleep
  if(systemOn) {
    goToSleep();

  // If the system was off, wake up
  } else if (!systemOn) {
    Serial.println("Waking up");
    systemOn = true;
    powerPressTime == 0;
  }
}

void updateBatteryLevel() {
  // CHARGE_OFF;
  for (int i = 0; i<10; i++) {
    battLev = battLev + analogRead(BATT_LEV_PIN);
  }
  // CHARGE_ON;
  battLev = battLev / 10;
  // Serial.print(battLev);
  battLev = map(battLev, 2000, 2400, 0, 100);
  if (battLev > 100) 
    battLev = 100;
  if (battLev <0 )
    battLev = 0;
  updateBatteryCharacteristic(battLev);
  // Serial.printf(";%i%%;", battLev);
}

void powerLeds() {
  for (int i=0; i<4; i++) {
    rgbLeds.setPixelColor(i, BLACK);
  }
  rgbLeds.show();
  int circleOrder[4] = {0, 1, 3, 2};
  for (int i=0; i<4; i++) {
    rgbLeds.setPixelColor(sensorMapping[circleOrder[i]], RED);
    rgbLeds.show();
    delay(250);
  }
}

void updateChargeState() {
    int chargeState = analogRead(CHARGE_STATE_PIN);
    // Serial.println(chargeState);
    if (chargeState > 1200 && chargeState < 1300) {
      digitalWrite(CHARGE_LED_PIN, HIGH);
    } else if (chargeState > 2000 && chargeState < 3000) {
      // Fully charged
      digitalWrite(CHARGE_LED_PIN, HIGH);
    } else if (chargeState > 3000) {
      // Charging
      digitalWrite(CHARGE_LED_PIN, LOW);
    }
}
