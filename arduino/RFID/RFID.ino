#include <SPI.h>    //Load SPI on Arduino IDE
#include <RFID.h>   //Load RC522_RFID library (Paul Kourany)

String version = "RFID-0.1";

#define SS_PIN 10   // Declare SDA pin on Arduino
#define RST_PIN 9   // Declare RST pin on Arduino
RFID rfid(SS_PIN, RST_PIN);   //Start RFID object
 
String cardID;      //Declare string variable to print RFID value

String inputString = "";
 
void setup() { 
	Serial.begin(9600); //Start serial
	SPI.begin();        //Start SPI
	rfid.init();        //Start RFID object
}
 
void loop() {
	if (rfid.isCard()) {   //If there is a card close to the reader ...
		if (rfid.readCardSerial()) {     //Read the card ID
                        cardID = "s";            //'s'tart mark
			cardID += String(rfid.serNum[0]) + "," + String(rfid.serNum[1]) + "," + String(rfid.serNum[2]) +
				"," + String(rfid.serNum[3]) + "," + String(rfid.serNum[4]);  //Convert to str
			cardID += "e"; 		//'e'nd mark
			Serial.println(cardID); 	//show on serial monitor
		}
	}
	delay(50);
}

void serialEvent()
{
	while (Serial.available())
	{
		char inChar = (char)Serial.read();
		inputString += inChar;
		if (inChar == '\n')
		{
			if (inputString.startsWith("Chronojump RFID"))
			{
				Serial.println("YES Chronojump RFID");
			} else if(inputString = "get_version:")
      {
        Serial.println(version);
      }
			inputString = "";
		}
	}
}
