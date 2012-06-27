;****************************************************************************
;*  chronopic.asm      Febrero 2005                                         *
;---------------------------------------------------------------------------*
; Software for the microcontroller PIC16F876A used by the Chronopic        *
; chronometer on Chronojump project                                         *
;                                                                           *
;---------------------------------------------------------------------------*
;  Juan Gonzalez <juan@iearobotics.com>                                     *
;  2010 English translation: Xavier de Blas <xaviblas@gmail.com>            *
;  LICENSE: GPL                                                             *
;****************************************************************************

;-- this PIC is going to be used:
  LIST p=16f873
  INCLUDE "p16f873.inc"

;*****************************************
;*         CONSTANTS                     *
;*****************************************
;-- Logical status of the input (not physical status)
;-- It's the information sent to the frame of changing
INPUT_OFF EQU 0  ;-- Push button not pushed
INPUT_ON  EQU 1  ;-- Push button pushed

;-- Header of the asyncronous frame of "changing"
FCHANGE    EQU 'X'

;-- Header of the "status" frame
FSTATUS    EQU 'E'  ; Petition of status of the platform
RSTATUS    EQU 'E'  ; Response of the status frame

;-- Initialization value of the TIMER0 to have TICKS with a duration of 10ms
;-- It's used for the debouncing time
TICK    EQU 0xD9 

;-- Value of the debouncing time (in units of 10 milliseconds)
;-- This value can be changed, in order to select the most suitable
;-- Signals with a duration lower than this value are considered spurious

;-- 0ms
;-- DEBOUNCE_TIME   EQU 0x00
;-- 10ms
;-- DEBOUNCE_TIME   EQU 0x01
;-- 50ms
DEBOUNCE_TIME   EQU 0x05
;-- 100ms
;-- DEBOUNCE_TIME   EQU 0x0A

;-- Status of main automaton
STAT_WAITING_EVENT EQU 0x00                
STAT_DEBOUNCE      EQU 0x01
STAT_FRAMEX      EQU 0x02

;*******************************************
;*               VARIABLES                 *
;*******************************************
  CBLOCK  0x20
  
    ;-- Save the context when interruptions arrive
    savew   ; Temporal storage of W register
    saves   ; temporal storage of the STATUS register

    ;-- Extension of timer 1 
    TMR1HH 
    
    ;-- Counter of the debouncer
    COUNTDEBOUNCE

    ;-- Timestamp of an event
    TIMESTAMP_HH ;-- high-high part
    TIMESTAMP_H  ;-- high part
    TIMESTAMP_L  ;-- low part
    
    ;-- Pushbutton status
    input         ;-- Current stable input Value: (0-1)
    input_new     ;-- New stable input Value: (0-1)

    ;-- Automaton status
    status
    
    ;-- Reset: Shows if has been done a reset of events
    ;-- Reset=1, means that next incoming event will be considered the first
    ;-- that means that it's timestamp has no sense, and a 0 must be returned 
    ;-- Reset=0, it's normal status.
    reset
    
    ;-- 16 bits variable to do a pause 
    ;-- (used by the pause routine)
    pause_h
    pause_l
    
    ;-- Store a character of the received frame
    char
  ENDC

;****************************
;* Starting of the program 
;****************************
  ORG 0
  GOTO start 

  ORG 4
;**************************************************
;* Interruptions routine                          *
;**************************************************
int

  ;-----------------------------
  ;-- Store the context
  ;-----------------------------
  MOVWF savew        ;-- Store value of W register
  SWAPF STATUS,W     ;-- Store value of STATUS register
  MOVWF saves

  ;-- Find the interruption cause
  BTFSC INTCON,T0IF
  GOTO isr_timer0      ;-- Caused by timer0
  
  BTFSC INTCON,RBIF
  GOTO isr_portb       ;-- Caused by a change on B port
  
  BTFSC PIR1,TMR1IF    ;-- Caused by timer 1
  GOTO isr_timer1
   
  ;-- WARNING! WARNIG! WARNING! Here we should never be!!!!
  GOTO int
  
  ;-------------------------
  ;- End of interruption
  ;-------------------------
end_int

  ;--   Recuperate context
  SWAPF saves,W
  MOVWF STATUS
  SWAPF savew, F
  SWAPF savew, W
        
        RETFIE

;********************************************************
;* Routine of interruption of timer1
;* timer 1 controls the cronometring
;* This routine is invoked when there's an overflow
;* Timer 1 gets extended with 1 more byte: TMR1HH
;********************************************************
isr_timer1
  BCF PIR1,TMR1IF   ; Remove overflow flag
  
  ;-- Overflow control
  ;-- Check if counter has arrived to it's maximum value 
  ;-- If it's maximum, then not increment
  
  MOVLW 0xFF           ; TMR1HH = 0xFF?
  SUBWF TMR1HH,W
  BTFSC STATUS,Z          ; No--> Continue
  GOTO  end_int           ; Yes--> Don't increment chrono. END
  
  ;-- Increment 
  INCF TMR1HH,F
  
  goto end_int

;******************************************************
;* Routine of interruption of timer0
;* timer0 is used to control debouncing time 
;* A change on input signal is stable if time it's at minimum equal to debouncing time
;* This timer is working all the time. 
;* Main automaton know when there's valid information
;******************************************************
isr_timer0
  BCF INTCON,T0IF   ;  Remove overflow flag

  ;-- Execute timer again inside a click
  MOVLW TICK
  MOVWF TMR0

  ;-- Decrese debouncing counter
  DECF COUNTDEBOUNCE,F
  
  goto end_int

;****************************************************
; Routine of port B interruption 
; Called everytime that's a change on bit RB4
; This is the main part.
; Everytime that's a change on input signal,
; it's timestamp is recorded on variable (TIMESTAMP, 3 bytes)
; and we start debouncing time phase
;****************************************************
isr_portb

  MOVF reset,F            ; Reset=1?
  BTFSC STATUS,Z
  GOTO no_reset           ; No--> normal status

  ;-- It's the first event after reset
  ;-- Put counter on zero and go to status reset=0
  CLRF TMR1HH
  CLRF TMR1H
  CLRF TMR1L
  CLRF reset

no_reset
  ;-- Store the value of chronometer on TIMESTAMP
  ;-- This is the timestamp of this event
  MOVFW TMR1HH
  MOVWF TIMESTAMP_HH
  MOVFW TMR1H
  MOVWF TIMESTAMP_H
  MOVFW TMR1L
  MOVWF TIMESTAMP_L
  
  ;-- Initialize timer 1
  CLRF TMR1HH
  CLRF TMR1H
  CLRF TMR1L
  
  ;-- Initialize debouncing counter
  MOVLW DEBOUNCE_TIME
  MOVWF COUNTDEBOUNCE
  
  ;-- start debouncing status
  MOVLW STAT_DEBOUNCE
  MOVWF status
  
  ;-- start debouncing timer on a tick
  MOVLW TICK
  MOVWF TMR0
  
  ;-- Remove interruption flag
  MOVFW PORTB
  BCF INTCON,RBIF 
  
  ;-- Inhabilite B port interruption
  BCF INTCON,RBIE 
 
  ;-- Exit interruption
  goto end_int

;---------------------------------------
;-- CONFIGURATION OF SERIAL PORT    
;-- 9600 BAUD, N81
;---------------------------------------
sci_configuration
  BSF STATUS,RP0    ; Access to bank 1
  MOVLW 0x19        ; Speed: 9600 baud
  MOVWF SPBRG

  MOVLW 0x24
  MOVWF TXSTA       ; Configure transmitter

  BCF STATUS,RP0    ; Access to bank 0
  MOVLW 0x90        ; Configure receiver
  MOVWF RCSTA
  RETURN
  
;**************************************************
;* Receive a character by the SCI
;-------------------------------------------------
; OUTPUTS:
;    Register W contains received data
;**************************************************
sci_readchar
  BTFSS PIR1,RCIF   ; RCIF=1?
  GOTO sci_readchar  ; no--> Wait

  ;-- Read the character
  MOVFW RCREG       ; W = recived data

  RETURN

;*****************************************************
;* Transmit a character by the SCI               
;*---------------------------------------------------
;* INPUTS:
;*    Register W:  character to send         
;*****************************************************
;-- Wait to Flag allows to send it to 1 (this comment may be bad translated)
sci_sendchar
wait
  BTFSS PIR1,TXIF   ; TXIF=1?
  goto wait       ; No--> wait

  ;; -- Transmission can be done
  MOVWF TXREG
  RETURN

;*******************************************
;* Routine of pause, by active waiting
;*******************************************
pause
  MOVLW 0xFF        ; Initialize counter (high part)
  MOVWF pause_h

loopl
  MOVLW 0xFF        ; Initialize counter (low part)
  MOVWF pause_l
  CLRWDT
repeat
  DECFSZ pause_l,F  ; Decrease pause_l, pause_l=0?
  goto repeat     ;  NO--> Repeat

  DECFSZ pause_h,F  ; Decrease pause_h, pause_h=0?
  goto loopl     ; No--> Goto loopl

  ;-- If we arrived here means counter has arrived to 0000
  ;-- (pause_h=0 and pause_l=0)

  RETURN

;***************************************************************************
;* Read input on RB4 (push button status)
;* INPUTS: None
;* OUTPUTS: None
;* RETURNS: W contains input status (INPUT_ON, INPUT_OFF)
;***************************************************************************
read_input
  ;-- Check status of bit RB4  
  BTFSC PORTB,4         ; RB4==0 ? 
  RETLW INPUT_OFF     ; No --> Push button not pushed
  RETLW INPUT_ON      ; Yes --> Push button pushed
  
;*************************************************************
;* Update led with stable status of input
;* Stable status is on variable: input
;* INPUTS: None
;* OUTPUTS: None
;* RETURNS: Nothing
;*************************************************************
update_led
  ;-- Led is on bit RB1. Input variable contains
  ;-- only an information bit (1,0) on less signficant bit
  RLF input,W    ; W= input<<1
  XORLW 0x02       ; negative logic (Comment this line if you want positive logic)
  MOVWF PORTB      ; Update port B
  RETURN

;*****************************************************
;* Activate interruption of changing of port B
;* INPUT: None
;* OUTPUT:  None
;* RETURN: Nothing
;*****************************************************
portb_int_enable
  ;-- Remove interruption flag, just in case it's activated
  MOVFW PORTB
  BCF INTCON,RBIF  
  
  ;-- Activate interruption of change
  BSF INTCON,RBIE  
  RETURN

;****************************************************************
;* Status service. Returns the status of the platform           *
;****************************************************************
status_serv

  ;--Deactivate interruption of change while frame is sent
  BCF INTCON,RBIE
  
  ;-- Send response code
  MOVLW RSTATUS 
  CALL sci_sendchar
  
  ;-- Send status of the push button
  MOVFW input
  CALL sci_sendchar
  
  ;-- Activate interruption of change
  BSF INTCON,RBIE  

  RETURN

;*************************************************************************
;*                        MAIN PROGRAM
;*************************************************************************
start

;-----------------------------
;- CONFIGURE PORT B           
;-----------------------------
;-- Pins I/O: RB0,RB4 inputs, all the other are outputs
  BSF STATUS,RP0      ; Change to bank 1
  MOVLW 0x11
  MOVWF TRISB
  
;-- Pull-ups of port B enabled
;-- Prescaler of timer0 at 256
;--   RBPU = 0, INTEDG=0, T0CS=0, T0SE=0, PSA=0, [PS2,PS1,PS0]=111
  MOVLW 0x07
  MOVWF OPTION_REG
  
;----------------------------------------------
;  CONFIGURATION OF SERIAL COMMUNICATIONS
;----------------------------------------------
  CALL sci_configuration

;----------------------------------------------
;- CONFIGURATION OF TIMER 0
;----------------------------------------------
  ;-- Remove interruption flag, just in case it's activated
  BCF INTCON,T0IF   ;  Remove overflow flag
  
  ;-- Activate timer. Inside an interruption tick
  BCF STATUS,RP0    ; Change to bank 0
  MOVLW TICK
  MOVWF TMR0

;----------------------------------------------
;- CONFIGURATION OF TIMER 1
;----------------------------------------------
 
  ;-- Activate timer
  BCF STATUS,RP0    ; Access to bank 0
  MOVLW 0x31
  MOVWF T1CON
  
  ;-- Zero value
  CLRF TMR1HH
  CLRF TMR1H
  CLRF TMR1L
  
  ;-- Enable interruption
  BSF STATUS,RP0      ; Change to bank 1
  BSF PIE1,TMR1IE
  BCF STATUS,RP0    ; Access to bank 0

;----------------------------
;- Interruptions port B
;----------------------------
  ;-- Wait to port B get's stable
  CALL pause
  
  ;-- Enable interruption of change on port B
  CALL portb_int_enable
  
;------------------------------
;- INITIALIZE VARIABLES
;------------------------------
  ;-- Initialize extended counter
  CLRF TMR1HH
  
  ;-- Initialize debounce counter
  MOVLW DEBOUNCE_TIME
  MOVWF COUNTDEBOUNCE
  
  ;-- Initialize automaton
  MOVLW STAT_WAITING_EVENT
  MOVWF status
  
  ;-- At start, system is on Reset
  MOVLW 1
  MOVWF reset

  ;-- Read input status and update input variable
  CALL read_input
  MOVWF input
  
;----------------------
;- INITIAL LED STATUS
;----------------------
  ;-- Update led with the stable status of input variable
  CALL update_led
  
;--------------------------
;- Interruption TIMER 0
;--------------------------
  BSF INTCON,T0IE   ; Activate interruption overflow TMR0  

;------------------------------------------
;- ACTIVATE GLOBAL INTERRUPTIONS
;- The party starts, now!!! 
;------------------------------------------
  ;-- Activate peripheral interruptions
  BSF INTCON,PEIE
  
  ;-- Activate global interruptions
  BSF INTCON,GIE 

;****************************
;*   MAIN LOOP 
;****************************
main
  CLRWDT      ;-- used on the simulation
  
  ;-- Analize serial port waiting to a frame
  BTFSS PIR1,RCIF   ; RCIF=1?
  GOTO automaton     ; no--> Go to automaton
  
  ;-- A character has arrived: read it
  call sci_readchar
  MOVWF char
  
  MOVLW FSTATUS  		;  status frame?
	SUBWF char,W
	BTFSC STATUS,Z
	CALL status_serv	;  Yes--> Service of platform status
  
automaton
  ;-----------------------------------------------------
  ;- DEPENDING AUTOMATON STATUS, GO TO DIFFERENT ROUTINES
  ;------------------------------------------------------
  MOVLW STAT_WAITING_EVENT        ;  status = STAT_WAITING_EVENT?
  SUBWF status,W
  BTFSC STATUS,Z
  GOTO stat_waiting_event
  
  MOVLW STAT_DEBOUNCE            ; status = DEBOUNCE?
  SUBWF status,W
  BTFSC STATUS,Z
  GOTO stat_debounce
  
  MOVLW STAT_FRAMEX                  ; status = FRAMEX?
  SUBWF status,W
  BTFSC STATUS,Z
  GOTO stat_framex
  
  ;-- WARNING WARNING WARNING!!! We shouldn't be here
  GOTO main
 
;----------------------------
;- STATUS WAITING EVENT  
;----------------------------
stat_waiting_event
  GOTO main

;----------------------------
;- STATUS DEBOUNCING
;----------------------------
stat_debounce
  MOVF COUNTDEBOUNCE,F       ; count_debounce=0?
  BTFSS STATUS,Z
  goto main             ; No--> It hasn't ended yet
  
  ;-- End of debounce
  ;-- Remove interruption flag of port B: to clean. We do not want or need
  ;-- what came during that time
  MOVFW PORTB
  BCF INTCON,RBIF 
  
  ;-- input_new = input status
  CALL read_input   
  MOVWF input_new
  
  ;-- Compare new input with stable input
  MOVFW input_new          ;  input_new==input?
  SUBWF input,W
  BTFSC STATUS,Z
  GOTO spurious           ; Yes--> It's an spurious pulse
  
  ;-- input!=input_new: Happened an stable change
  
  ;-- Store new stable input
  MOVFW input_new
  MOVWF input
   
  ;-- Change to status FRAMEX to send frame with the event
  MOVLW STAT_FRAMEX
  MOVWF status
  
  GOTO main
 
spurious 
  ;-- It came an spurious pulse (change with a duration
  ;-- lower than debounce time). It's ignored.
  ;-- We continue like if nothing happened
  ;-- The value of the counter should be: actual + TIMESTAMP
  ;-- TMR1 = TIMR1 + TIMESTAMP
  
  MOVFW TIMESTAMP_L
  ADDWF TMR1L,F
  ;-- Add carry, if any
  BTFSC STATUS,C   ;-- Carry = 1?
  INCF TMR1H,F     ;-- Yes--> Add it to high part
                   ;-- No--> Continue
 
  MOVFW TIMESTAMP_H
  ADDWF TMR1H,F
  ;-- Add carry, if any
  BTFSC STATUS,C   ;-- Carry = 1?
  INCF TMR1HH,F    ;-- Yes--> Add it to "higher weight" part            ;-- No--> Continue
  
  MOVFW TIMESTAMP_HH
  ADDWF TMR1HH,F

  ;-- Change status to waiting event
  MOVLW STAT_WAITING_EVENT
  MOVWF status 
  
  ;-- Activate interruption port B
  CALL portb_int_enable
  
  GOTO main

;----------------------------
;- STATUS FRAMEX 
;----------------------------
stat_framex
  ;-- Send frame of changing input
  ;-- First the frame identifier
  MOVLW FCHANGE
  CALL sci_sendchar
  
  ;-- Send push button status
  MOVFW input
  CALL sci_sendchar
  
  ;-- Send timestamp
  MOVFW TIMESTAMP_HH 
  CALL sci_sendchar
  MOVFW TIMESTAMP_H
  CALL sci_sendchar
  MOVFW TIMESTAMP_L
  CALL sci_sendchar
  
  ;-- Change to next status
  MOVLW STAT_WAITING_EVENT
  MOVWF status
  
  ;-- Update led status, depending on stable input status
  CALL update_led
  
  ;-- Activate port B interruption
  CALL portb_int_enable
  
  GOTO main

  END
