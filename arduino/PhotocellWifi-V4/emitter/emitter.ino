//radio.setRetries(15,15); primer parametro pausa entre intentos,(en múltiplos de 250µS, por eso 0 son 250µS  y 15 son  400µS.),hará hasta 15 intentos en caso de error
//Versión 1.04
#include <SPI.h>
#include <nRF24L01.h>
#include <RF24.h>
#include <printf.h>
#include <MsTimer2.h>
//


//          pin11 -----  M0
//          pin12 ----- MSO
// Arduino  pin13 ----- SCK  NRf24L01
//          pin A4 ---- CSN
//          pin A3 ----  CE
//          3,3v            NRf24L01
//          GND --------    NRf24L01



// Set up nRF24L01 radio on SPI bus plus pins  (CE & CS)

RF24 radio(10, 9);   //9,10 originalmente , ahora A3,A4

const int intPin = 0;         //Para definir pin interrupción  0 = interrupción  por pin D2
const int debounceTime = 1;
bool pinState = LOW;                //estado pin 2 Alto/Bajo
bool lastPinState = LOW;
volatile int cont;  // esta variable es especial para trabajar en la interrupción si hace falta
volatile bool debounceFlag = LOW;
uint8_t channel0 = 111; // channel0 defecto frecuencia emisión radio
uint8_t channel_switch = 0; // recibira  el valor en decimal de microswich ajuste de frecuencia radio
char c[4];  //para debug veriable frecuencia channel0 envio serie
uint8_t pot = 0;
int veces = 0;
unsigned long tiempo1 = 0; // se le carga millis() cuando entra interrupcion flanco negativo
unsigned long tiempo2 = 0; // se le carga millis () al detectar en pin d2 flanco positivo
// se restan las dos variables para determinar la duración del pulso
//int i;   //index para bucle
//bool pinState = LOW;

struct frame {
  short id;
  bool state;
  unsigned long totalTime;
};

frame data = {.id = 0, .state = LOW, .totalTime = 0};
int len_data = sizeof(data);

// Single radio pipe address for the 2 nodes to communicate.
const uint64_t pipe = 0xE8E8F0F0E1LL;



void setup(void)
{

  /**
    Set Power Amplifier (PA) level to one of four levels:
    RF24_PA_MIN, RF24_PA_LOW, RF24_PA_HIGH and RF24_PA_MAX

    The power levels correspond to the following output levels respectively:
    NRF24L01: -18dBm, -12dBm,-6dBM, and 0dBm

    SI24R1: -6dBm, 0dBm, 3dBM, and 7dBm.

    @param level Desired PA level.

    void setPALevel ( RF24_PA_LOW );*/

  //radio.setPALevel (RF24_PA_LOW); //funcion  que solo se llama una vez si se quiere cambiar de potencia
  //pot= radio.getPALevel( ); //función que retorna potencia programada

  Serial.begin(115200);
  //
  //  configure rf radio
  //
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

  //Máximo 125 canales, telefonia wifi ocupa de 2402 a 2472
  //Nosotros llegamos hasta 2525 total libre del 72 al 125, 1mhz entre canales
  //Con canales próximos se interfieren en las comunicaciones
  //Como hay (125 - 71) = 54 canales, hacemos que cada bit del switch aumente en 2 channel0es (7Mhz)

  radio.setChannel(channel0 + channel_switch * 2);
  //****************************************************************************************

  //-------------------------------------------------------
  // Open pipes to other nodes for communication
  // This simple sketch opens a single pipes for these two nodes to communicate
  // back and forth.  One listens on it, the other talks to it.
  radio.openWritingPipe(pipe);    //radio.openReadingPipe(1,pipe);
  //--------------------------------------------------------
  radio.stopListening();
  //  printf(" Versión Emisor 1.02 \n\r");
  Serial.println("Emitter version 1.03");
  //----------------------------------------------------

//  radio.printDetails();  //imprime configuración del emisor

  //------------------------------------------------------


  // Set pull-up resistors for all buttons

  pinMode(2, INPUT_PULLUP);
  pinMode(3, INPUT_PULLUP);
  pinMode(4, INPUT_PULLUP);
  pinMode(5, INPUT_PULLUP);
  pinMode(6, INPUT_PULLUP);
  pinMode(7, INPUT_PULLUP);
  pinMode(8, INPUT_PULLUP);
  pinMode(9, INPUT_PULLUP);
  pinMode(10, INPUT_PULLUP);

data.id = 0;
for(int i = 9; i>=3; i--){
  data.id = 2 * data.id;
  if(!digitalRead(i)) {
    data.id++;
  }
}

  //------------------------------------------------------
  pinState = digitalRead(2);
  lastPinState = pinState;
  attachInterrupt(digitalPinToInterrupt(2), controlint, CHANGE); //define interrupcion por  entrada 2, por flanco negativo

  //----------------------------------------------------------

  MsTimer2::set(debounceTime, debounce);
}

void loop(void)   // bucle programa
{
  if (debounceFlag == HIGH ) //ha ocurrido interrupción
  {
    pinState = digitalRead(2);
    if(pinState != lastPinState) {
//      data.totalTime = millis();
//      if (pinState == LOW)
//      {
//        data.state = 'I';
//      } else if (pinState = HIGH)
//      {
//        data.state = 'O';
//      }
      data.state = pinState;
      bool en  = radio.write( &data, len_data -4);
      lastPinState = pinState;

      debounceFlag = LOW;
      Serial.print(data.id);
      Serial.print("\t");
      Serial.println(pinState);
      //TODO: comprobar si es mejor desactivar interrupciones o no
      //interrupts();
    }
  }
}

void controlint()
{
  //TODO: comprobar si es mejor desactivar interrupciones o no
  //noInterrupts();
  pinState = digitalRead(2);
  debounceFlag = LOW;
  MsTimer2::start();
}

void debounce(){
  MsTimer2::stop();
  debounceFlag = HIGH;
  }
