// Wichro V10 Hardware pinout
#define WICHRO

//  ARDUINO   NRf24L01
//  ------------------ 
//  pin11     M0
//  pin12     MSO
//  pin13     SCK  
//  pin10     CSN
//  pin9      CE
//  3.3V      3.3V
//  GND       GND

//Old versions
// #define RADIO_CE A3
// #define RADIO_CS A4

//New version matching arduino with NRf24L01 integrated
#define RADIO_CE 10
#define RADIO_CS 9

#define SENSOR_PIN 2
// Pin for the buzzer
#define BUZZER_PIN A0

// Pin for reading the battery level
#define BATTERY_PIN A6

// Pins for the channel selector
#define CH1_PIN A1
#define CH2_PIN A2
#define CH3_PIN A7

//Versions 3 and 4

// Pins for the RGB LEDs
#define RED_PIN 3
#define GREEN_PIN 5
#define BLUE_PIN 6

// Pins for the ID of the terminal
#define ID1_PIN A3
#define ID2_PIN 4
#define ID3_PIN A4
#define ID4_PIN A5
#define ID5_PIN 7
#define ID6_PIN 8
