/*
 * Version 2016.x
 *
  2005 Original Firmware from Juan González <juan@iearobotics.com>
  2010 Translated comments to english by Xavi de Blas <xaviblas@gmail.com>
  2011-2013 Conversion to C by Teng Wei Hua <wadegang@gmail.com>
  2014 Xavier de Blas: Improvements on Read/send version; Change debounce
  2015 Ferran Suarez & Xavier de Blas:
  	implementation of new outputs
	anticipation in an animated "led-wheel", using pauses on Timer2
	fast blinking led (flickr)
  2016 Ferran Suarez & Xavier de Blas: reaction time: Validation of led-wheel, discriminative fully implemented


 *
 *
 * Translating firmware to SDCC:
 * source: http://git.gnome.org/browse/chronojump/plain/chronopic-firmware/chronopic-firmware.asm


encoder:
  * brown: VCC	  
  * blue : GND	  
  * black: CLK	  pin 21
  * white: VATA	  pin 23
  
PIC16F87
  * RB5 : option  pin 26

History:
  2011-01-01 complete TMR0 interrupt
  2011-05-13 ERROR: TIME = 04 ED
             modify configuration Bits: close WDT
             modify ISR and MAIN LOOP's if --> else if
             limit COUNTDEBOUNCE overflow in INTERRUPT(isr)
	     assembler is more efficient than C
  2012-04-19 if PC send command 'J' for port scanning, Chronopic will return 'J'  2014-08-30 if PC send command 'V' for getting version, ex: 2.1\n
  	     if PC send command 'a' get debounce time , ex:0x01
	     if PC send command 'bx' for setting debounce time, x is from byte value 0~255(\x0 ~ \xFF) 
  2015-02-19 
	     if PC send command 'R','r' for reaction time protocol on pin RB3 (R/r open/close this light)
	     if PC send command 'S','s' for reaction time protocol on pin RB6 (S/s open/close this light)
	     if PC send command 'T','t' for reaction time protocol on pin RB7 (T/t open/close this light)
*/

//-- this PIC is going to be used:
//#include <pic16f876A.h> 	//sdcc for Windows
#include <pic16f876a.h>		//sdcc for Linux




//*****************************************
//*         CONSTANTS                     *
//*****************************************
//-- Logical status of the input (not physical status)
//-- It's the information sent to the frame of changing
unsigned char INPUT_OFF = 0;  //-- Push button not pushed
unsigned char INPUT_ON  = 1;  //-- Push button pushed

//-- Header of the asyncronous frame of "changing"
unsigned char FCHANGE = 'X';

//-- Header of the "status" frame
unsigned char FSTATUS = 'E';  // Petition of status of the platform
unsigned char RSTATUS = 'E';  // Response of the status frame

//-- Initialization value of the TIMER0 to have TICKS with a duration of 10ms
//-- It's used for the debouncing time
unsigned char TICK = 0xD9;

//-- Value of the debouncing time (in units of 10 milliseconds)
//-- This value can be changed, in order to select the most suitable
//-- Signals with a duration lower than this value are considered spurious
unsigned char DEBOUNCE_TIME = 0x05;

//-- Status of main automaton
unsigned char STAT_WAITING_EVENT = 0x00;
unsigned char STAT_DEBOUNCE = 0x01;
unsigned char STAT_FRAMEX = 0x02;

//*******************************************
//*               VARIABLES                 *
//*******************************************

//-- Save the context when interruptions arrive
unsigned char savew;   // Temporal storage of W register
unsigned char saves;   // temporal storage of the STATUS register

//-- Extension of timer 1
unsigned char TMR1HH;

//-- Counter of the debouncer
unsigned char COUNTDEBOUNCE;

//-- Timestamp of an event
unsigned char TIMESTAMP_HH; //-- high-high part
unsigned char TIMESTAMP_H;  //-- high part
unsigned char TIMESTAMP_L;  //-- low part

//-- Pushbutton status
unsigned char input;         //-- Current stable input Value: (0-1)
unsigned char input_new;     //-- New stable input Value: (0-1)

//-- Automaton status
unsigned char status;

//-- Reset: Shows if has been done a reset of events
//-- Reset=1, means that next incoming event will be considered the first
//-- that means that it's timestamp has no sense, and a 0 must be returned
//-- Reset=0, it's normal status.
unsigned char reset;

//-- 16 bits variable to do a pause
//-- (used by the pause routine)
unsigned char pause_h;
unsigned char pause_l;

//-- Store a character of the received frame
unsigned char my_char;

//-- wade's addition variables
unsigned char i = 0, j = 0;
unsigned char option = 0;     // option: 0 button enable, 1 encoder enable
unsigned char command_port_scanning = 'J';	// for port scanning, it will return 'J'
unsigned char command_get_version = 'V';	// for getting version, it will return '2.1'
unsigned char command_get_debounce_time = 'a';	// for getting debounce time, it will return x:0~255(HEX)
unsigned char command_set_debounce_time = 'b';	// for setting debounce time, pc send two unsigned char, 'Sx' -- x:0~255
unsigned char command_reaction_time_animation_light = 'l';
unsigned char command_reaction_time_animation_flicker = 'f';

unsigned char command_reaction_time_disc_red 		= 'r';
unsigned char command_reaction_time_disc_yellow 	= 's';
unsigned char command_reaction_time_disc_green 		= 't';
unsigned char command_reaction_time_disc_red_yellow	= 'u';
unsigned char command_reaction_time_disc_red_green	= 'y';
unsigned char command_reaction_time_disc_yellow_green	= 'w';
unsigned char command_reaction_time_disc_all		= 'x';

unsigned char command_reaction_time_disc_red_bz 	= 'R';
unsigned char command_reaction_time_disc_yellow_bz 	= 'S';
unsigned char command_reaction_time_disc_green_bz 	= 'T';
unsigned char command_reaction_time_disc_red_yellow_bz	= 'U';
unsigned char command_reaction_time_disc_red_green_bz	= 'Y';
unsigned char command_reaction_time_disc_yellow_green_bz= 'W';
unsigned char command_reaction_time_disc_all_bz		= 'X';

unsigned char command_reaction_time_disc_only_bz	= 'Z';


unsigned char position = 0;
unsigned int timer2Times;

//in timer 2 prescaler
//160 times x 61 = 1/2 second
//TODO: fix this comment:
//Chronojump will pass the 160
//then max value will be 255 == 0,8 seconds
//min 1 == 0.003125
//TODO: maybe n_times have to be done 4 times to have reasonable values (between 0.0125 and 3.2 seconds)
//2016 - With 1000 fps camera validation, found animation_tick=63 the best value
unsigned int animation_tick = 63;
unsigned int animation_tick_n_times = 160; //default value

unsigned char animation_light_should_run = 0; //0 until and 'l' is sent from Chronojump
unsigned char flicker_light_should_run = 0; //0 until and 'f' is sent from Chronojump

unsigned char discriminative_light_should_run = 0; 
unsigned char discriminative_running = 0;
unsigned char discriminative_light_signal;

char version_major = '1';
char version_minor = '2';




//-- encoder's valus
//char encoder_count = 0; //wade

void zero_value_timer_1()
{
	TMR1HH = 0;
	TMR1H = 0;
	TMR1L = 0;
}


// Interruptions routine
// Find the interruption cause
void isr(void) __interrupt 0
{
	//while (!TXIF);
	//TXREG = 0xaa;

	//RB7 = 0; RB3 = 1; RB0 = 0; RB2 = 0; timer2_delay_long(10);


	if (option == 0) 
	{

		//******************************************************
		//* Routine of interruption of timer0
		//* timer0 is used to control debouncing time
		//* A change on input signal is stable if time it's at minimum equal to debouncing time
		//* This timer is working all the time.
		//* Main automaton know when there's valid information
		//******************************************************  
		// Cause by timer0
		if (T0IF == 1)
		{	
			T0IF = 0;		 // Remove overflow flag
			TMR0 = TICK;		 //-- Execute timer again inside a click
			if (COUNTDEBOUNCE > 0)	 // wade : limit COUNTDEBOUNCE overflow
				COUNTDEBOUNCE--;	 //-- Decrese debouncing counter
			//while (!TXIF); //wade
			//TXREG = COUNTDEBOUNCE; //wade
		}
		//****************************************************
		// Routine of port B interruption
		// Called everytime that's a change on bit RB4
		// This is the main part.
		// Everytime that's a change on input signal,
		// it's timestamp is recorded on variable (TIMESTAMP, 3 bytes)
		// and we start debouncing time phase
		//****************************************************    
		// Caused by a change on B port
		else if (RBIF == 1)
		{
			//-- Inhabilite B port interruption
			// wade : take care
			RBIE = 0;	
			//-- Remove interruption flag
			RBIF = 0;
			if (reset == 1)
			{
				//-- It's the first event after reset
				//-- Put counter on zero and go to status reset=0
				zero_value_timer_1();
				reset = 0;
			}
			//-- Store the value of chronometer on TIMESTAMP
			//-- This is the timestamp of this event
			TIMESTAMP_HH = TMR1HH;
			TIMESTAMP_H = TMR1H;
			TIMESTAMP_L = TMR1L;
			//-- Initialize timer 1
			zero_value_timer_1();
			//-- Initialize debouncing counter
			COUNTDEBOUNCE =  DEBOUNCE_TIME;

			//-- start debouncing status
			status = STAT_DEBOUNCE;
			//-- start debouncing timer on a tick
			TMR0 = TICK;
		}
		//********************************************************
		//* Routine of interruption of timer1
		//* timer 1 controls the cronometring
		//* This routine is invoked when there's an overflow
		//* Timer 1 gets extended with 1 more byte: TMR1HH
		//********************************************************
		// Caused by timer1
		else if (TMR1IF == 1)
		{
			TMR1IF = 0;  // Remove overflow flag
			if (TMR1HH != 0xFF)
			{
				//-- Overflow control
				//-- Check if counter has arrived to it's maximum value
				//-- If it's maximum, then not increment
				TMR1HH++;
			}   
		}
	} // end of (option == 0)
}

//---------------------------------------
//-- CONFIGURATION OF SERIAL PORT
//-- 9600 BAUD, N81
//---------------------------------------
void sci_configuration()
{
	// wade : start
	// formula: Baud = Frequency / ( 16(x+1) )
	// SPBRG = 0X19;   // crystal:  4MHz Speed: 9600 baud 
	// SPBRG = 0X81;   // crystal: 20MHz Speed: 9600 baud
	// SPBRG = 0X0A;   // crystal: 20MHz Speed: 115200 baud
	// SPBRG = 0X01;   // crystal: 20MHz Speed: 625000
	SPBRG = 0X19;   // Speed: 9600 baud

	// wade : end
	TXSTA = 0X24;   // Configure transmitter
	RCSTA = 0X90;   // Configure receiver
}

//**************************************************
//* Receive a character by the SCI
//-------------------------------------------------
// OUTPUTS:
//    Register W contains received data
//**************************************************
unsigned char sci_readchar()
{
    while (!RCIF);	// P1R1:RCIF  Receive Interrupt Flag
    return RCREG;
}

//*****************************************************
//* Transmit a character by the SCI
//*---------------------------------------------------
//* INPUTS:
//*    Register W:  character to send
//*****************************************************
//-- Wait to Flag allows to send it to 1 (this comment may be bad translated)
void sci_sendchar(unsigned char my_w)
{
    while (!TXIF);
    TXREG = my_w;
}

void sci_sendline(unsigned char my_w)
{
    while (!TXIF);
    TXREG = my_w;
    while (!TXIF);
    //TXREG = '\n';
}



/************************************
 * Send version
 ************************************/
void send_version()
{
    while (!TXIF);
    TXREG = version_major;
    while (!TXIF);
    TXREG = '.';
    while (!TXIF);
    TXREG = version_minor;
    while (!TXIF);
    //TXREG = '\n';
}

void send_error()
{
    while (!TXIF);
    TXREG = '-';
    while (!TXIF);
    TXREG = '1';
    while (!TXIF);
    //TXREG = '\n';
}


//*******************************************
//* Routine of pause, by active waiting
//*******************************************
void pause()
{
    unsigned char i,j;
    for (i = 0; i < 0xff; i++)
    {
	__asm
	CLRWDT	
	__endasm;
	for (j = 0; j < 0xff; j++);
    }
}


// wade : long delay
void pause1()
{
    long i;long j;
    for (i = 0; i < 4; i++)
	for (j = 0; j < 10000; j++)
		;
}
void pause2(int mytime)
{
    long i; //no se pq amb long va be i amb int no
    for (i = 0; i < mytime; i++)
	    ;
}

void pause3()
{
    long i;
    for (i = 0; i < 40000; i++)
	    ;
}


//***************************************************************************
//* Read input on RB4 (push button status)
//* INPUTS: None
//* OUTPUTS: None
//* RETURNS: W contains input status (INPUT_ON, INPUT_OFF)
//***************************************************************************
unsigned char read_input()
{
    //-- Check status of bit RB4
    if (RB4 == 0)
	return INPUT_ON;
    return INPUT_OFF;
}

//*************************************************************
//* Update led with stable status of input
//* Stable status is on variable: input
//* INPUTS: None
//* OUTPUTS: None
//* RETURNS: Nothing
//*************************************************************
void update_led()
{
    //-- Led is on bit RB1. Input variable contains
    //-- only an information bit (1,0) on less signficant bit
    RB1 = !input;
}

//Timer 2 delays ----
/*
void timer2_delay_long(unsigned char t2ini)
{
  //-- Dar valor inicial del timer
  TMR2=t2ini;
 
  //-- Flag de interrupcion a cero
  TMR2IF=0;

  //-- Esperar a que transcurra el tiempo indicado
  while(TMR2IF==0);
}
*/

void timer2_start() {
	TMR2=animation_tick;

	//-- Flag de interrupcion a cero
	TMR2IF=0;
}

//end of Timer 2 delays ----


void reaction_time_animation_lights_do()
{
	if(TMR2IF == 1) {

		timer2_start();
		timer2Times --;

		if(timer2Times <= 0) {

		   	//RB7 = 0; RB3 = 0; RB0 = 0; RB2 = 0; //OFF
			switch(position) {
				case 0:
			        RB7 = 1; RB3 = 0; RB0 = 1; RB2 = 0;
			        break;
			    case 1:
			        RB7 = 1; RB3 = 0; RB0 = 1; RB2 = 1;
			        break;
			    case 2:
			        RB7 = 1; RB3 = 0; RB0 = 0; RB2 = 0;
			        break;
			    case 3:
			        RB7 = 1; RB3 = 0; RB0 = 0; RB2 = 1;
			        break;
			    case 4:
			        RB7 = 1; RB3 = 1; RB0 = 1; RB2 = 0;
			        break;
			    case 5:
			        RB7 = 1; RB3 = 1; RB0 = 1; RB2 = 1;
			        break;
			    case 6:
			        RB7 = 1; RB3 = 1; RB0 = 0; RB2 = 0;
			        break;
			    case 7:
			        RB7 = 1; RB3 = 1; RB0 = 0; RB2 = 1;
			        break;
			    case 8:
			        RB7 = 0; RB3 = 0; RB0 = 1; RB2 = 0;
			        break;
			    case 9:
			        RB7 = 0; RB3 = 0; RB0 = 0; RB2 = 1;
			        break;
			    case 10:
			        RB7 = 0; RB3 = 0; RB0 = 1; RB2 = 1;
			        break;
			    case 11:
			        RB7 = 0; RB3 = 1; RB0 = 0; RB2 = 0;
			        break;
			    case 12:
			        RB7 = 0; RB3 = 1; RB0 = 0; RB2 = 1;
			        break;
			    case 13:
			        RB7 = 0; RB3 = 1; RB0 = 1; RB2 = 0;
			        break;
			    case 14:
			        RB7 = 0; RB3 = 1; RB0 = 1; RB2 = 1;
			}

			timer2Times = animation_tick_n_times;
			 
			position ++;
	    	if(position > 14)
	        	position = 0;
	    }

	}

  //-- Esperar a que transcurra el tiempo indicado
  //while(TMR2IF==0);

}

void reaction_time_flicker_do()
{
	if(TMR2IF == 1) {
		timer2_start();
		timer2Times --;

		if(timer2Times <= 0) {
			switch(position) {
				case 0:
					RB7 = 0; RB3 = 0; RB0 = 0; RB2 = 0; //OFF
					position = 1;
					break;
				case 1:
					RB7 = 0; RB3 = 0; RB0 = 0; RB2 = 1;
					position = 0;
					break;
			}
			
			timer2Times = animation_tick_n_times;
		}
	}
}

void reaction_time_discriminative_do() 
{
	//1st put timer to 0
    	zero_value_timer_1();
	
	//2nd fire signal
	switch(discriminative_light_signal) {
		case 'r':
			//RB7 = 0; RB3 = 1; RB0 = 0; RB2 = 0; //light red
			RB2 = 0;
			break;
		case 's':
			//RB7 = 0; RB3 = 0; RB0 = 1; RB2 = 1; //light yellow
			RB0 = 0;
			break;
		case 't':
			//RB7 = 0; RB3 = 0; RB0 = 1; RB2 = 0; //light green
			RB3 = 0;
			break;
		case 'y':	//red & green
			RB2 = 0;
			RB3 = 0;
			break;
		case 'T':	//green and buzzer
			//RB7 = 0; RB3 = 0; RB0 = 1; RB2 = 0; //light green
			RB3 = 0;
			RB7 = 0;
			break;
		case 'Z':
			//RB7 = 0; RB3 = 0; RB0 = 0; RB2 = 1; //buzzer
			RB7 = 0;
			break;
			//TODO if none of this green is lighted. Fix it 
	}
	
	//3rd don't call this again
	discriminative_light_should_run = 2;
}
void reaction_time_discriminative_stop()
{
	RB7 = 1; RB3 = 1; RB0 = 1; RB2 = 1;
	discriminative_running = 0;
}

//*****************************************************
//* Activate interruption of changing of port B
//* INPUT: None
//* OUTPUT:  None
//* RETURN: Nothing
//*****************************************************
void portb_int_enable()
{
    // Remove inerruption flag, just in case it's activated
    RBIF = 0;
    // Activate interruption of change
    RBIE = 1;
}

//****************************************************************
//* Status service. Returns the status of the platform           *
//****************************************************************
void status_serv()
{
    //--Deactivate interruption of change while frame is sent
    RBIE = 0;
    //-- Send response code
    sci_sendchar(RSTATUS);
    //-- Send status of the push button
    sci_sendchar(input);
    //-- Activate interruption of change
    RBIE = 1;
}

static void asm_ledon()
{
    __asm
    BANKSEL PORTC
    MOVLW   0XFF
    MOVWF PORTC
    __endasm;
}

/*
static void animation_light_convert_old(char sentData)
{
	switch(sentData) {
		case 0:
			animation_tick_n_times = 5;
			break;
		case 1:
			animation_tick_n_times = 10;
			break;
		case 2:
			animation_tick_n_times = 20;
			break;
		case 3:
			animation_tick_n_times = 40;
			break;
		case 4:
			animation_tick_n_times = 80;
			break;
		case 5:
			animation_tick_n_times = 160; //160[default] * 63 = ,5 seconds: 500ms
			break;
		case 6:
			animation_tick_n_times = 320; //1 second
			break;
		case 7:
			animation_tick_n_times = 640; //2 seconds
			break;
	}
}
*/
static void animation_light_convert(char sentData)
{
	switch(sentData) {
		case 0:
			animation_tick_n_times = 5; //15,125ms
			break;
		case 1:
			animation_tick_n_times = 10; //31,25
			break;
		case 2:
			animation_tick_n_times = 15; //46,875
			break;
		case 3:
			animation_tick_n_times = 20; //62,5
			break;
		case 4:
			animation_tick_n_times = 25; //78,125
			break;
		case 5:
			animation_tick_n_times = 30; //93,75
			break;
		case 6:
			animation_tick_n_times = 35; //109,375
			break;
		case 7:
			animation_tick_n_times = 40; //125
			break;
	}
}
static void flickr_light_convert(char sentData)
{
	switch(sentData) {
		case 0:
			animation_tick_n_times = 1;
			break;
		case 1:
			animation_tick_n_times = 2;
			break;
		case 2:
			animation_tick_n_times = 3;
			break;
		case 3:
			animation_tick_n_times = 4;
			break;
		case 4:
			animation_tick_n_times = 5;
			break;
		case 5:
			animation_tick_n_times = 6;
			break;
		case 6:
			animation_tick_n_times = 7;
			break;
		case 7:
			animation_tick_n_times = 8;
			break;
	}
}


void main(void)
{
    // =========
    //   START
    // =========
    
    //-----------------------------
    //- Detect option pin 26
    //-----------------------------
    
    //-----------------------------
    //- CONFIGURE PORT B
    //-----------------------------
    //-- Pins I/O: RB0,RB4 inputs, all the other are outputs
    // 2012-04-02 wade: start
    //TRISB = 0x11;
    // 2015 RB0 is going to be an output
    TRISB = 0x10;
    
    // 2012-04-02 wade: end
    //-- Pull-ups of port B enabled
    //-- Prescaler of timer0 at 256
    //--   RBPU = 0, INTEDG=0(1:rising edge, 0:falling edge), T0CS=0, T0SE=0, PSA=0, [PS2,PS1,PS0]=111
    OPTION_REG = 0x07;

    //----------------------------------------------
    //  CONFIGURATION OF SERIAL COMMUNICATIONS
    //----------------------------------------------
    sci_configuration();

    //-- Configure Timer 2
    //-- Temporizer mode
    TOUTPS2=0; TOUTPS1=0; TOUTPS0=0;
    //-- Set Prescaler at 16
    T2CKPS1=1; T2CKPS0=1;
    //Start temporizer
    TMR2ON=1; 
    //----------------------------------------------
    //- CONFIGURATION OF TIMER 0
    //----------------------------------------------
    //-- Remove interruption flag, just in case it's activated
    T0IF = 0;	//  Remove overflow flag
    //-- Activate timer. Inside an interruption tick
    // wade : start
    if (option == 0)
	TMR0 = TICK;
    if (option == 1)
	TMR0 = 0xFF;
    // wade : end

    //----------------------------------------------
    //- CONFIGURATION OF TIMER 1
    //----------------------------------------------
    //-- Activate timer 
    //   set prescaler
    // wade : start
    if (option == 0)
	T1CON = 0X31;
    // wade : end
    //-- Zero value
    zero_value_timer_1();
    //-- Enable interruption
    // wade : sart
    if (option == 0)
	TMR1IE = 1;
    // wade : end

    //----------------------------
    //- Interruptions port B
    //----------------------------
    //-- Wait to port B get's stable
    pause();
    //-- Enable interruption of change on port B
    // wade : start
    if (option == 0)
	portb_int_enable();
    // wade : end

    //------------------------------
    //- INITIALIZE VARIABLES
    //------------------------------
    //-- Initialize extended counteri
    TMR1HH = 0;
    //-- Initialize debounce counter
    COUNTDEBOUNCE = DEBOUNCE_TIME;
    //-- Initialize automaton
    status = STAT_WAITING_EVENT;
    //-- At start, system is on Reset
    reset = 1;
    //-- Read input status and update input variable   
    input = read_input();
    
    //----------------------
    //- INITIAL LED STATUS
    //----------------------
    //-- Update led with the stable status of input variable
    update_led();

    //-------------
    //for discriminative. Attention: this is because currently Ferran hardware is reversed. Change this soon!
    //RB7 = 0; RB3 = 0; RB0 = 0; RB2 = 0; //OFF
    RB7 = 1; RB3 = 1; RB0 = 1; RB2 = 1; //OFF
    //-------------

    //--------------------------
    //- Interruption TIMER 0
    //--------------------------
    T0IE = 1;	// Activate interruption overflow TMR0
   
    //--------------------------
    //- Interruption INT RB0
    //-------------------------- 
    // wade : start
    if (option == 1)
	INTE = 1;
    // wade : end
    // wade : for testing
    /*
    for (i = 0; i <= 0x77; i++)
    {
	TXEN = 1;
	for(j = 0; j < 0xFF; j++)
	{
	    while (!TXIF);
	    TXREG = i;
	    while (!TXIF);
	    TXREG = j;
	}
	TXEN = 0;	
    }
    */
    // wade : for testing

    //------------------------------------------
    //- ACTIVATE GLOBAL INTERRUPTIONS
    //- The party starts, now!!!
    //------------------------------------------
    //-- Activate peripheral interruptions
    PEIE = 1;
    //-- Activate global interruptions

    GIE = 1;

    //initialize lights stuff for animation wheel
    position = 0;
    timer2Times = animation_tick_n_times;
    timer2_start(); //TODO: delete this?

    
    //****************************
    //*   MAIN LOOP
    //**************************** 
    while(1)
    {
        //-- used on the simulation
	__asm
	CLRWDT
	__endasm;
        // wade : start
        // sci_sendchar(FCHANGE);
        //sci_sendchar(0xFF);
        //sci_sendchar(0xFF);
	// wade : end

	if (option == 0)
	{
	    //-- Analize serial port waiting to a frame
	    if (RCIF == 1)	// Yes--> Service of platform status
	    {	        
		my_char = sci_readchar();
		if (my_char == FSTATUS)
			status_serv();
	    	else if (my_char == command_port_scanning)	// 'J'
			sci_sendchar(command_port_scanning);
		else if (my_char == command_get_version)	// 'V'
			send_version();
		else if (my_char == command_get_debounce_time)	// 'a'
			sci_sendline(DEBOUNCE_TIME + '0'); 	//if DEBOUNCE is 50ms (0x05), returns a 5 (5 * 10ms = 50ms)
		else if (my_char == command_set_debounce_time)	// 'b'
			DEBOUNCE_TIME = sci_readchar();
		//else if (my_char == command_reaction_time_rb3_on) // 'R'
		//	RB0 = 1; //RB3 = 1
		//else if (my_char == command_reaction_time_rb6_on) // 'S'
		//	RB2 = 1; //RB6 = 1
		//else if (my_char == command_reaction_time_rb7_on) // 'T'
		//	RB7 = 1;
		//else if (my_char == command_reaction_time_rb3_off) // 'r'
		//	RB0 = 0; //RB3 = 0
		//else if (my_char == command_reaction_time_rb6_off) // 's'
		//	RB2 = 0; //RB6 = 0
		//else if (my_char == command_reaction_time_rb7_off) // 't'
		//	RB7 = 0;
		else if (my_char == command_reaction_time_animation_light) { // 'l'
			animation_light_convert(sci_readchar());
			animation_light_should_run = 1;
		}
		else if (my_char == command_reaction_time_animation_flicker) { // 'f'
			flickr_light_convert(sci_readchar());
			flicker_light_should_run = 1;
		}
		else if (
				my_char == command_reaction_time_disc_red ||
				my_char == command_reaction_time_disc_yellow ||
				my_char == command_reaction_time_disc_green ||
				my_char == command_reaction_time_disc_red_yellow ||
				my_char == command_reaction_time_disc_red_green ||
				my_char == command_reaction_time_disc_yellow_green ||
				my_char == command_reaction_time_disc_all ||
				my_char == command_reaction_time_disc_red_bz ||
				my_char == command_reaction_time_disc_yellow_bz ||
				my_char == command_reaction_time_disc_green_bz ||
				my_char == command_reaction_time_disc_red_yellow_bz ||
				my_char == command_reaction_time_disc_red_green_bz ||
				my_char == command_reaction_time_disc_yellow_green_bz ||
				my_char == command_reaction_time_disc_all_bz ||
				my_char == command_reaction_time_disc_only_bz
			) {
			//use 10ms debounce
			DEBOUNCE_TIME = 0x01;

			//reaction_time_discriminative_do(my_char);
			discriminative_light_signal = my_char;
		
			discriminative_light_should_run = 1;
		}
		else
			send_error();
	    }

	    //------------------------------------------------------
	    //- DEPENDING AUTOMATON STATUS, GO TO DIFFERENT ROUTINES
	    //------------------------------------------------------
	    if (status == STAT_WAITING_EVENT)   // status = STAT_WAITING_EVENT?
	    {
		    //TODO: only if an 'l' has been sent before
		if(animation_light_should_run == 1)
			reaction_time_animation_lights_do();
		else if(flicker_light_should_run == 1)
			reaction_time_flicker_do();
		else if(discriminative_light_should_run == 1) {
			discriminative_running = 1;
			reaction_time_discriminative_do();
		}
	    }
	    else if (status == STAT_DEBOUNCE)   // status = DEBOUNCE?
	    {
		//----------------------------
		//- STATUS DEBOUNCING
		//----------------------------
		if (COUNTDEBOUNCE == 0)
		{
		    //-- End of debounce
		    //-- Remove interruption flag of port B: to clean. We do not want or need
		    //-- what came during that time
		    RBIF = 0;
		    //-- input_new = input status
		    input_new = read_input();
		    //-- Compare new input with stable input
		    if (input_new == input)
                    {
			//-- It came an spurious pulse (change with a duration
			//-- lower than debounce time). It's ignored.
			//-- We continue like if nothing happened
			//-- The value of the counter should be: actual + TIMESTAMP
			//-- TMR1 = TIMR1 + TIMESTAMP
			TMR1L = TMR1L + TIMESTAMP_L;
			//-- Add carry, if any
			if (C == 1)	    //-- Yes--> Add it to high part
			    TMR1H++;
			TMR1H = TMR1H + TIMESTAMP_H;
			//-- Add carry, if any
			if (C == 1)	    //-- Yes--> Add it to "higher weight" part
			    TMR1HH++;
			TMR1HH = TMR1HH + TIMESTAMP_HH;
			//-- Change status to waiting event
			status = STAT_WAITING_EVENT;
			//-- Activate interruption port B
			portb_int_enable();
		    }
		    else
		    { 
			//-- input!=input_new: Happened an stable change
			//-- Store new stable input
			input = input_new;
			//-- Change to status FRAMEX to send frame with the event
			status = STAT_FRAMEX;
		    }
		}
	    }
	    else if (status == STAT_FRAMEX)	    // status = FRAMEX?
	    {
		//----------------------------
		//- STATUS FRAMEX
		//----------------------------
		//
		//stop animation light, and go to the beginning
		animation_light_should_run = 0;
		position = 0;
			
		if(discriminative_running)
			reaction_time_discriminative_stop();

		
		//-- Send frame of changing input
		//-- First the frame identifier
		sci_sendchar(FCHANGE);
		//-- Send push button status
		sci_sendchar(input);
		//-- Send timestamp
		sci_sendchar(TIMESTAMP_HH);
		sci_sendchar(TIMESTAMP_H);
		sci_sendchar(TIMESTAMP_L);
		//-- Change to next status
		status = STAT_WAITING_EVENT;
		//-- Update led status, depending on stable input status
		update_led();
		//-- Activate port B interruption
		portb_int_enable();
                input_new = read_input();
                if (input_new != input)
                    RBIF = 1;
	    }
	} // end of if (option == 0)
	
    } // end of while

}

