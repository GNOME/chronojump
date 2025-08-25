void changedZ() {
  flagZ = HIGH;
  //Serial.println("#"); maybe habilite again to have absolute position

  sample.sensorType = incEncoderZ;
  sample.time = encoderTime;
  sample.data = 1;
  if (transmissionFormat == text) {
        Serial.print(encoderTime);
        Serial.print(";");
        Serial.print(1);
        Serial.print(";");
        Serial.println(sample.sensorType);
  }
  else { // (transmissionFormat == binary)
    Serial.write((byte*)&sample,12);
  }
}

void changedA() {
  if (digitalRead(EncoderBPin)) {
    currentPosition++;
  } else {
    currentPosition--;
  }
  if (abs(currentPosition) >= pps) {
    encoderTime = totalTime;
    lastPosition = currentPosition;
    currentPosition = 0;
    encoderFlag = true;
  }
}