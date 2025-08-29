//#define freq 2000  // Samples per second
//#define freq 500  // Samples per second 500 works!
//#define freq 2000  // works!
//#define freq 10000  // a few of the lines are not really printed (just \n, or 1\n). Checked in moserial. BUT on chronojump with text buffer worked
//#define freq 12000 //works
//#define freq 20000 //fails
//#define freq 30000 // fails with chronojump

//#define PPR 10  // Pulses Per Revolution // unsused

int freq = 10000;
//int freq = 30000; //xungo

String version = "EncoderForce-0.3";
bool flagA = false;//true;
//int encoderPosition = 1000000; //it is not really watching encoder, is just a sum
int encoderPosition = 0; //if freq is high (> 10000) this fails equally than starting with encoderPosition: 1000000. So the problem is more related to freq than string length

hw_timer_t *timerA = NULL;

void setup()
{
  Serial.begin(115200);
  Serial.flush();
  delay(100);
  //Serial.println("Inici");
  timerA = timerBegin(1000000); // Number of increments of the counter per second. 1E6 -> microseconds
  timerAlarm(timerA, 1E6 / (freq), true, 0);     // Each cycle the pin must change twice: Period [microseconds] = 2 * 1E6 [us / s] / freq [Khz]
  timerAttachInterrupt(timerA, &changedA);
  timerStop(timerA);
}

void loop() {
  if (flagA)
  {
    Serial.print(encoderPosition);
    Serial.print(";");
    Serial.println(1); //the encoderPosition (1 or -1 if pps == 1)
    
    // this is NOT better:
    //Serial.println(String(encoderPosition) + ";1");

    
    flagA = false;
  }

  if (Serial.available()) {
    processCommand(Serial.readString());
    }
}

void changedA() {
    encoderPosition++ ;
    flagA = true;
}

void processCommand (String inputString)
{
  String commandString = inputString.substring(0, inputString.lastIndexOf(":"));
  
  if (commandString == "start_capture_textEncCountUp") {
    Serial.println("Starting capture...");
    //encoderPosition = 1000000;
    encoderPosition = 0; //if freq is high (> 10000) this fails equally than starting with encoderPosition: 1000000. So the problem is more related to freq than string length
    startTimer();
  } else if (commandString == "end_capture") {
    stopTimer();
    //Serial.print("at stop timer:");
    //Serial.println(encoderPosition);
    Serial.println("Capture ended:");
  } else if (commandString == "get_version") {
    get_version();
  } else if (commandString == "set_freq") {
    set_freq (inputString);
  } 
}

void get_version() {
  Serial.println(version);
}

void set_freq(String inputString)
{ 
  String argument = get_command_argument(inputString);
  int newFreq = argument.toInt();
  if (newFreq != freq) {  //Trying to reduce the number of writings
    freq = newFreq;
  }

  /*
  not working with just this instructions
  timerAlarmWrite(timerA, 1E6 / (freq), true);     // Each cycle the pin must change twice: Period [microseconds] = 2 * 1E6 [us / s] / freq [Khz]
  timerAlarmEnable(timerA);
  timerAttachInterrupt(timerA, &changedA, true);
  timerStop(timerA);
  
  
  Serial.print("freq set to: ");
  Serial.println(freq);
  */
} 

String get_command_argument(String inputString)
{
  return (inputString.substring(inputString.lastIndexOf(":") + 1, inputString.lastIndexOf(";")));
}

void startTimer()
{
  //Serial.println("At startTimer");
  // next 2 commands have to be in this exact order:
  timerStart(timerA);
  timerWrite(timerA, 0);
}

void stopTimer()
{
  flagA = false;
  timerStop(timerA);
}
