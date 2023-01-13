#include <TimerOne.h>   //It allows to set the duty cycle of the pwm
#include <elapsedMillis.h>

int ledPin = (A1);              //Led indicating the state
elapsedMillis totalTime = 0;

//Emitter signal
//float duty = 30;                //Duty cycle in %
//int emitterPin = 9;             //InfraRed pin that emits the signal carrier
//bool bursting = false;          //wether the signal carrier is continuous or bursting. Set to true just for debugging purposes
//bool active = true;             //The carrier signal can periodically switch from active to inactive if bursting is true
//unsigned int period = 18;       //18 -> 56kHz, 26 -> 38kHz
unsigned int inputPin = 2;      // 2 -> OR config (contact platform). 3 -> AND config (photocell)


void setup() {
  Serial.begin(115200);
  Serial.println("IR_Platform-0.2");
  //Timer1.initialize(period);
  //Timer1.pwm(emitterPin,  duty * 1023 / 100);

  //Sensor config
  pinMode(ledPin, OUTPUT);
  pinMode(2, INPUT);
  pinMode(3, INPUT);
  digitalWrite(ledPin, digitalRead(inputPin));
  digitalWrite(A0,LOW);
  attachInterrupt(digitalPinToInterrupt(inputPin), changedPin, CHANGE);

//  for (int i = 0; i< 10; i++){
//    digitalWrite(ledPin, active);
//    active = !active;
//    delay(25);
//  }

//Just to advertise in which mode it is
//  if (inputPin == 3){
//    for (int i = 0; i< 10; i++){
//      digitalWrite(ledPin, active);
//      active = !active;
//      delay(150);
//    }
//  }
//  digitalWrite(ledPin, HIGH);
//  delay(500);
//  active = true;
}

void loop() {
//  if (bursting) {
//    if (totalTime >= 1000) {
//      active = !active;
//      if (active) {
//        Timer1.initialize(period);
//        Timer1.pwm(emitterPin,  duty * 1023 / 100);
//      } else if (!active) {
//        Timer1.disablePwm(emitterPin);
//        digitalWrite(emitterPin, LOW);
//      }
//      totalTime = 0;
//    }
//  }
}

void changedPin() {
  bool carrierDetected = !digitalRead(inputPin);
  digitalWrite(ledPin, carrierDetected);
  //Serial.println(carrierDetected);
}
