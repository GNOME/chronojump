
void readADC() {
  force = scale.get_units();
  // force = scale.get_value();
  forceTime = totalTime;
  adcFlag = true;
  timerStop(adcTimer);
  timerStart(adcTimer);
  timerWrite(adcTimer, 0);
}
void get_calibration_factor()
{
  Serial.println(scale.get_scale());
}

void set_calibration_factor(String inputString)
{
  //Reading the argument of the command. Located within the ":" and the ";"
  String calibrationString = get_command_argument(inputString);
  //Serial.println(calibration_factor.toFloat());
  scale.set_scale(calibrationString.toFloat());
  preferences.putFloat("calibration", scale.get_scale());
  Serial.println("Calibration factor set");
}

void calibrate(String inputString)
{
  //Reading the argument of the command. Located within the ":" and the ";"
  String weightString = get_command_argument(inputString);
  float weight = weightString.toFloat();
  //mean of 255 values comming from the cell after resting the offset.
  double offsetted_data = scale.get_value(50);
  Serial.print(offsetted_data);
  Serial.print("\t");
  Serial.println(weight);

  //offsetted_data / calibration_factor
  float currentCalibration = offsetted_data / weight / 9.81; //We want to return Newtons.
  scale.set_scale(currentCalibration);
  preferences.putFloat("calibration", currentCalibration);
  Serial.print("Calibrating OK:");
  Serial.println(currentCalibration);
}

void tare()
{
  scale.tare(50); //Reset the scale to 0 using the mean of 255 raw values
  preferences.putLong("tare", scale.get_offset());
  Serial.print("Taring OK:");
  Serial.println(scale.get_offset());
}

void get_tare()
{
  Serial.println(scale.get_offset());
}

void set_tare(String inputString)
{
  String tareString = get_command_argument(inputString);
  scale.set_offset(tareString.toInt());
  preferences.putLong("tare", scale.get_offset());
  Serial.println("Tare set");
}