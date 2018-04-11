
int val;
int encoderPinA = 2;
int encoderPinB = 3;
volatile int encoderDisplacement = 0;
volatile unsigned long changingTime;
unsigned long elapsedTime;
unsigned long lastTime;

void setup() {
  pinMode (encoderPinA, INPUT);
  pinMode (encoderPinB, INPUT);
  Serial.begin (115200);

  //Using the rising flank of the A photocell we have a 200 PPR.
  attachInterrupt(digitalPinToInterrupt(encoderPinA), changingA, RISING);

  //Using the CHANGE with both photocells WE CAN HAVE 800 PPR
  //attachInterrupt(digitalPinToInterrupt(encoderPinB), changingB, CHANGE);
}

void loop() {
  //With a diameter is of 160mm, each pulse is 2.513274mm. 4 pulses equals 1.00531cm
  if ( abs(encoderDisplacement) >= 4 ) {

    int lastEncoderDisplacement = encoderDisplacement; //Assigned to another variable for in the case that encoder displacement changes before printing it
    unsigned long Time = changingTime;
    encoderDisplacement = 0;

    //Managing the timer overflow
    if (Time > lastTime)      //No overflow
    {
      elapsedTime = Time - lastTime;
    } else  if (Time <= lastTime)  //Overflow
    {
      elapsedTime = (4294967295 - lastTime) + Time; //Time from the last measure to the overflow event plus the changingTime
    }

    Serial.print(lastEncoderDisplacement);
    Serial.print(";");
    Serial.println(elapsedTime);
    lastTime = changingTime;
  }
  delayMicroseconds(100);
}

void changingA() {
  changingTime = micros();
  if (digitalRead(encoderPinB) == HIGH) {
    encoderDisplacement--;
  } else {
    encoderDisplacement++;
  }
}

/***********************************************************************
   If we need more precission we can use both flanks of both photocells
 * *********************************************************************

  void changingA() {
  changingTime = micros();
  if (digitalRead(encoderPinA) == HIGH) {
    if (digitalRead(encoderPinB) == HIGH) {
      encoderDisplacement--;
    } else {
      encoderDisplacement++;
    }
  } else {
    if (digitalRead(encoderPinB) == HIGH) {
      encoderDisplacement++;
    } else {
      encoderDisplacement--;
    }
  }
  }

  void changingB() {
  changingTime = micros();
  if (digitalRead(encoderPinB) == HIGH) {
    if (digitalRead(encoderPinA) == HIGH) {
      encoderDisplacement++;
    } else {
      encoderDisplacement--;
    }
  } else {
    if (digitalRead(encoderPinA) == HIGH) {
      encoderDisplacement--;
    } else {
      encoderDisplacement++;
    }
  }
  }

*/

