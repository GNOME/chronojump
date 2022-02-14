//Versión 1.03

#include <SPI.h>
#include <RF24.h>
#include <printf.h>
//
// Hardware configuration


//          pin11 -----  M0
//          pin12 ----- MSO
// Arduino  pin13 ----- SCK  NRf24L01
//          pin A4 ---- CSN
//          pin A3 ----  CE
//          3,3v       NRf24L01
//          GND        NRf24L01

String version = "WiFi_Receiver-1.03";

// Set up nRF24L01 radio on SPI bus plus pins  (CE & CS)

RF24 radio(A3, A4);   //9,10 originalmente , ahora A3,A4

//Structure of the received data
struct frame {
  short id;
  bool state;
  unsigned long totalTime;
};

//data stores the radio incoming data
frame data = {.id = 0, .state = LOW, .totalTime = 0};
int len_data = sizeof(data);

unsigned long totalTime;
unsigned long sampleTime;
unsigned long lastSampleTime;
unsigned long startTime;
uint8_t channel0 = 111; // Canal defecto frecuencia emisión radio
uint8_t channel_switch = 0; // recibira  el valor en decimal de microswich ajuste de frecuencia radio
uint8_t pot = 0;
bool capturing = false;
unsigned long baudRate = 115200;
//
// Topology
//

// Single radio pipe address for the 2 nodes to communicate.
const uint64_t pipe = 0xE8E8F0F0E1LL;

void setup(void)
{
//  SPI.setClockDivider(SPI_CLOCK_DIV2);

  /**
    Set Power Amplifier (PA) level to one of four levels:
    RF24_PA_MIN, RF24_PA_LOW, RF24_PA_HIGH and RF24_PA_MAX

    The power levels correspond to the following output levels respectively:
    NRF24L01: -18dBm, -12dBm,-6dBM, and 0dBm

    SI24R1: -6dBm, 0dBm, 3dBM, and 7dBm.

    @param level Desired PA level.

    void setPALevel ( RF24_PA_LOW );*/

  //radio.setPALevel (RF24_PA_HIGH); //funcion  que solo se llama una vez si se quiere cambiar de potencia
  //pot= getPALevel( ); //función que retorna potencia programada



  printf_begin();
  Serial.begin (baudRate);
  Serial.print("Initial setup: Baud rate : ");
  Serial.println(baudRate);



  radio.begin();

  // establece frecuencia canal
  //************************************************************************************
  pinMode(A0, INPUT);       //
  digitalWrite(A0, HIGH);   //
  pinMode(A1, INPUT);       // Pone como entradas A0,A1,A3 y a nivel alto
  digitalWrite(A1, HIGH);   //
  pinMode(A2, INPUT);       //
  digitalWrite(A2, HIGH);   //

  //   En estas entradas se pondra un microswich , de 3 botones
  //   Se leeran en binario y se sumarán al canal por defecto 101
  if (!digitalRead(A0)) {
    channel_switch = 1; //
  }
  if (!digitalRead(A1)) {
    channel_switch = channel_switch + 2;
  }
  if (!digitalRead(A2)) {
    channel_switch = channel_switch + 4;
  }
  
  //maximo 125 canales, telefonia wifi ocupa de 2402 a 2472 y 
  //Nosotros llegamos hasta 2525 total libre del 72 al 125 , separacion entre canales 1 Mhz
  //Con canales próximos se interfieren en las comunicaciones
  //Como hay (125 - 71) = 54 canales, hacemos que cada bit del switch aumente en 7 canales (7Mhz)
  // 000 -> canal 75, 001 -> canal 82, 010 -> canal 89 ... 111 -> canal 124

  radio.setChannel(channel0 + channel_switch * 2);
  //****************************************************************************************

  
  // This simple sketch opens a single pipes for these two nodes to communicate
  // back and forth.  One listens on it, the other talks to it.


  radio.openReadingPipe(1, pipe); //radio.openWritingPipe(pipe);
//  radio.printDetails();
  radio.startListening();


  pinMode(2, OUTPUT);
  digitalWrite(2, LOW); // pone el led receptor apagado

  startTime = millis();

}

void loop(void)
{
  int situacio = 1;   //Testing with diferens places to read the time. The optimal is (1)
  
  if (capturing)
  {
    if(situacio==1) { sampleTime = millis(); } // (1) Time read now to avoid random delay on radio.availabe() and radio.read()
    while (radio.available())
    {
        if(situacio==2) { sampleTime = millis(); } // (2) Time read now to avoid random delay on radio.availabe() and radio.read()
        //Reading from the radio and storing in data
        radio.read(  &data, len_data);
        if(situacio==3) { sampleTime = millis(); } // (3) Time read now to avoid random delay on radio.availabe() and radio.read()

        if(sampleTime > startTime)            //No overflow
        {
          totalTime= sampleTime - startTime;
          //Serial.print(startTime); Serial.print("\t"); Serial.print(sampleTime); Serial.print("\t"); Serial.println(totalTime);
        } else if (sampleTime <= startTime)   //Overflow
        {
          //Time from the last measure to the overflow event plus the sampleTime
          totalTime = (4294967295 -  lastSampleTime) + sampleTime;
        }
        Serial.print(data.id); Serial.print(";");
        Serial.print(totalTime); Serial.print(";");
        digitalWrite(2, !data.state);
//      if (data.state)               //se ha recibido flanco de bajada
//      {
//        digitalWrite(2, HIGH);          // enciende led receptor
//        //Serial.println("I");                    // envia el paso por conector RCA y por port serie
//      } else if (!data.state)               //se ha recibido flanco de subida
//      {
//        digitalWrite(2, LOW);               // apaga led receptor
//        //Serial.println("O");                        //envia  por port serie
//      }

      Serial.println(data.state);
    }
  }
}

void serialEvent()
{
  capturing = false;
  String inputString = Serial.readString();
  String commandString = inputString.substring(0, inputString.lastIndexOf(":"));
  if (commandString == "start_capture") {
    start_capture();
  } else if (commandString == "end_capture") {
    end_capture();
  } else if (commandString == "get_status") {
    get_status();
  } else if (commandString == "get_version") {
    get_version();
  } else {
    Serial.println("Not a valid command");
  }
  inputString = "";
}

void start_capture()
{
  radio.flush_rx();
  Serial.println("Starting capture");
  startTime = millis();
  lastSampleTime = startTime + 1;
  capturing = true;
}

void end_capture()
{
  capturing = false;
  digitalWrite(2, LOW);
  Serial.println("\nCapture ended:");
  //asm("jmp 0x0000"); // ejecuta un reset para poner a "0" millis()
}

void get_status()
{
  Serial.println("status:");
  radio.printDetails();
}

void get_version()
{
  Serial.println(version);
}
