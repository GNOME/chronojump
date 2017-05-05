#include <SPI.h>    //Cargamos la librería SPI incluida en el IDE de Arduino
#include <RFID.h>   //Cargamos la librería RC522_RFID de Paul Kourany
 
#define SS_PIN 10   // Declaramos el pin SDA del Arduino
#define RST_PIN 9   // Declaramos el pin RST del Arduino
RFID rfid(SS_PIN, RST_PIN);   //Iniciamos el objeto RFID
 
String cardID;      //Declaramos una variable de tipo string 
                    //para almacenar el valor de los datos obtenidos de
                    //la etiqueta RFID
 
void setup() { 
Serial.begin(9600); //Iniciamos la comunicación serie para leer las respuestas del módulo
SPI.begin();        //Iniciamos la comunicación SPI
rfid.init();        //Iniciamos el objeto RFID
}
 
void loop() {
  if (rfid.isCard()) {   //Si hay una tarjeta cerca del lector...
    if (rfid.readCardSerial()) {     //Leemos la ID de la tarjeta
      cardID = String(rfid.serNum[0]) + "," + String(rfid.serNum[1]) + "," + String(rfid.serNum[2]) + 
      "," + String(rfid.serNum[3]) + "," + String(rfid.serNum[4]);  //Le damos una formato de cadena de carácteres
      Serial.println(cardID);  //La mostramos en el monitor serie
      }
    }
}
