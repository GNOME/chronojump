;****************************************************************************
;*  chronopic.asm      Febrero 2005                                         *
;---------------------------------------------------------------------------*
; Software para el microcontrolador PIC16F876A utilizado para el            *
; cronometro del proyecto chronojump                                        *
;                                                                           *
; Para las pruebas se esta utilizando la tarjeta SKYPIC                     *
;---------------------------------------------------------------------------*
;  Juan Gonzalez <juan@iearobotics.com>                                     *
;  LICENCIA GPL                                                             *
;****************************************************************************

;-- Establecer el PIC a emplear
  LIST p=16f873
  INCLUDE "p16f873.inc"

;*****************************************
;*         CONSTANTES                    *
;*****************************************
;-- Estado logico de la entrada (no el estado fisico)
;-- Es la informacion que se envia en la trama de cambio
ENTRADA_OFF EQU 0  ;-- Pulsador no apreado
ENTRADA_ON  EQU 1  ;-- Pulsador apretado

;-- Cabecera de la trama asincrona de "cambio"
TCHANGE    EQU 'X'

;-- Cabecera de la trama de "ESTADO"
TESTADO    EQU 'E'  ; Solicitud de Estado de la plataforma
RESTADO    EQU 'E'  ; Respuesta de la trama de estado

;-- Valor de inicializacion del TIMER0 para conseguir TICKS de 10ms de
;-- duracion. Se utiliza para el antirrebotes
TICK    EQU 0xD9 

;-- Valor del tiempo antirrebotes (en unidades de 10 milisegundos)
;-- Este valor se puede modificar, para establecerse el mas adecuado
;-- Las senales con una duracion inferior a este valor se consideran
;-- pulsos espureos
;-- Este valor se puede modificar, segun los criterios de exclusion
;-- de senales espureas
TIEMPO_ANTIRREBOTES   EQU 0x05

;-- Estado del automata principal
EST_ESPERANDO_EVENTO  EQU 0x00                
EST_ANTIRREBOTES      EQU 0x01
EST_TRAMAX            EQU 0x02

;*******************************************
;*               VARIABLES                 *
;*******************************************
  CBLOCK  0x20
  
    ;-- Guardar el contexto cuando llegan interrupciones
    savew   ; Almacenamiento temporal del registro W
    saves   ; Almacenamiento temporial del registro STATUS

    ;-- Extension del temporizador 1 
    TMR1HH 
    
    ;-- Contador del antirrebotes
    CONTANTI

    ;-- Marca de tiempo de un evento
    TIMESTAMP_HH ;-- Parte alta-alta
    TIMESTAMP_H  ;-- Parte alta
    TIMESTAMP_L  ;-- Parte baja
    
    ;-- Estado del pulsador
    entrada         ;-- Entrada estable actual Valor: (0-1)
    entrada_nueva   ;-- Nueva entrada (estable) Valor: (0-1)

    ;-- Estado del automata
    estado
    
    ;-- Reset: Indica si se habia hecho un reset de los eventos
    ;-- Reset=1, significa que el siguiente evento que llegue se
    ;-- considerara el primero, por lo que su marca de tiempo no 
    ;-- tiene sentido, y se debe devolver un 0.
    ;-- Reset=0, es el estado normal.
    reset
    
    ;-- Variable de 16 bits para hacer una pausa 
    ;-- (usado por rutina pausa)
    pausa_h
    pausa_l
    
    ;-- Almacenar un caracter de la trama recibida
    car
  ENDC

;****************************
;* Comienzo del programa 
;****************************
  ORG 0
  GOTO inicio 

  ORG 4
;**************************************************
;* Rutina de atencion a las interrupciones        *
;**************************************************
int

  ;-----------------------------
  ;-- Guardar el contexto
  ;-----------------------------
  MOVWF savew        ;-- Guardar valor del registro W
  SWAPF STATUS,W     ;-- Guardar valor registro STATUS
  MOVWF saves

  ;-- Determinar la causa de la interrupcion
  BTFSC INTCON,T0IF
  GOTO isr_timer0      ;-- Es debida al timer0
  
  BTFSC INTCON,RBIF
  GOTO isr_portb       ;-- Es debida a un cambio en el puerto B
  
  BTFSC PIR1,TMR1IF    ;-- Es debida al timer 1
  GOTO isr_timer1
   
  ;-- WARNING! WARNIG! WARNING! Aqui no deberia llegar!!!!
  GOTO int
  
  ;-------------------------
  ;- Fin de interrupcion
  ;-------------------------
fin_int

  ;--   Recuperar contexto
  SWAPF saves,W
  MOVWF STATUS
  SWAPF savew, F
  SWAPF savew, W
        
        RETFIE

;********************************************************
;* Rutina de atencion a la interrupcion del timer1
;* El timer 1 es el que lleva el cronometraje. Esta rutina
;* se invoca cuando hay un overflow. Se extiende el Timer 1
;* con un Byte mas: TMR1HH
;********************************************************
isr_timer1
  BCF PIR1,TMR1IF   ; Quitar flag de overflow
  
  ;-- Control del desbordamiento
  ;-- Comprobar si contador de ha llegado a su 
  ;-- maximo valor. Si es asi no se incrementa
  
  MOVLW 0xFF           ; TMR1HH = 0xFF?
  SUBWF TMR1HH,W
  BTFSC STATUS,Z          ; No--> Continuar
  GOTO  fin_int           ; Si--> No incrementar crono. FIN
  
  ;-- Incrementar 
  INCF TMR1HH,F
  
  goto fin_int

;******************************************************
;* Rutina de atencion a la interrupcion del timer0
;* El timer0 se utiliza para temporizar el tiempo 
;* antirrebotes. Para considerar que un cambio de la
;* senal de entrada es estable, debe transcurrir al menos
;* un tiempo igual al ANTIRREBOTES.
;* Este timer esta todo el rato funcionando. Es el automata
;* principal es que sabe cuando hay informacion valida
;******************************************************
isr_timer0
  BCF INTCON,T0IF   ;  Quitar flag overflow

  ;-- Volver a lanzar temporizador dentro de un tick
  MOVLW TICK
  MOVWF TMR0

  ;-- Decrementar contador antirrebotes
  DECF CONTANTI,F
  
  goto fin_int

;****************************************************
; Rutina de atencion a la interrupcion del puerto B  
; Se llama cada vez que haya un cambio en el bit RB4
; Esta es la parte principal. Cada vez que hay un 
; cambio en la senal de entrada, se registra su
; marca de tiempo en la variable (TIMESTAMP, de 3 bytes)
; y se entra en la fase de ANTIRREBOTES
;****************************************************
isr_portb

  MOVF reset,F            ; Reset=1?
  BTFSC STATUS,Z
  GOTO no_reset           ; No--> estado normal

  ;-- Es el primer evento despues del reset
  ;-- Poner contador a cero y pasar al estado reset=0
  CLRF TMR1HH
  CLRF TMR1H
  CLRF TMR1L
  CLRF reset

no_reset
  ;-- Almacenar el valor del cronometro en TIMESTAMP
  ;-- Esta es la marca de tiempo de este evento
  MOVFW TMR1HH
  MOVWF TIMESTAMP_HH
  MOVFW TMR1H
  MOVWF TIMESTAMP_H
  MOVFW TMR1L
  MOVWF TIMESTAMP_L
  
  ;-- Inicializar temporizador 1
  CLRF TMR1HH
  CLRF TMR1H
  CLRF TMR1L
  
  ;-- Inicializar el contador ANTIRREBOTES
  MOVLW TIEMPO_ANTIRREBOTES
  MOVWF CONTANTI
  
  ;-- Pasar al estado antirrebotes
  MOVLW EST_ANTIRREBOTES
  MOVWF estado
  
  ;-- lanzar temporizador antirrebotes dentro de un tick
  MOVLW TICK
  MOVWF TMR0
  
  ;-- Quitar flag de interrupcion
  MOVFW PORTB
  BCF INTCON,RBIF 
  
  ;-- Deshabilitar la interrupicion del puerto B
  BCF INTCON,RBIE 
 
  ;-- Salir de la interrupcion
  goto fin_int

;---------------------------------------
;-- CONFIGURACION DEL PUERTO SERIE    
;-- 9600 BAUDIOS, N81
;---------------------------------------
sci_configuration
  BSF STATUS,RP0    ; Acceso al banco 1
  MOVLW 0x19        ; Velocidad: 9600 baudios
  MOVWF SPBRG

  MOVLW 0x24
  MOVWF TXSTA       ; Configurar transmisor

  BCF STATUS,RP0    ; Acceso al banco 0
  MOVLW 0x90        ; Configurar receptor
  MOVWF RCSTA
  RETURN
  
;**************************************************
;* Recibir un caracter por el SCI
;-------------------------------------------------
; SALIDAS:
;    Registro W contiene el dato recibido
;**************************************************
sci_readcar
  BTFSS PIR1,RCIF   ; RCIF=1?
  GOTO sci_readcar  ; no--> Esperar

  ;-- Leer el caracter
  MOVFW RCREG       ; W = dato recibido

  RETURN

;*****************************************************
;* Transmitir un caracter por el SCI               
;*---------------------------------------------------
;* ENTRADAS:
;*    Registro W:  caracter a enviar         
;*****************************************************
;-- Esperar a que Flag de listo para transmitir este a 1
sci_sendcar
wait
  BTFSS PIR1,TXIF   ; TXIF=1?
  goto wait       ; No--> wait

  ;; -- Ya se puede hacer la transmision
  MOVWF TXREG
  RETURN

;*******************************************
;* Rutina de pausa, por espera activa
;*******************************************
pausa
  MOVLW 0xFF        ; Inicializar parte alta contador
  MOVWF pausa_h

buclel
  MOVLW 0xFF        ; Inicializar parte baja contador
  MOVWF pausa_l
  CLRWDT
repite
  DECFSZ pausa_l,F  ; Decrementa pausa_l, pausa_l=0?
  goto repite     ;  NO--> Repite

  DECFSZ pausa_h,F  ; Decrementa pausa_h, pausa_h=0?
  goto buclel     ; No--> Ve a buclel

  ;-- Si se ha llegado aqui es porque el contador ha llegado a 0000
  ;-- (pausa_h=0 y pausa_l=0)

  RETURN

;***************************************************************************
;* Leer la entrada RB4 (estado del pulsador)
;* ENTRADAS: Ninguna
;* SALIDA: Ninguna
;* DEVUELVE: W contiene el estado de la entrada (ENTRADA_ON, ENTRADA_OFF)
;***************************************************************************
leer_entrada
  ;-- Comprobar estado del bit RB4  
  BTFSC PORTB,4         ; RB4==0 ? 
  RETLW ENTRADA_OFF     ; No --> Pulsador no apretado
  RETLW ENTRADA_ON      ; Si --> Pulsador apretado
  
;*************************************************************
;* Actualizar el led con el estado estable de la entrada
;* El estado estable se encuentra en la variable entrada
;* ENTRADAS: Ninguna   
;* SALIDAS: Ninguna
;* Devuelve: Nada
;*************************************************************
actualizar_led
  ;-- El led esta en el bit RB1. La variable entrada contiene
  ;-- solo un bit de informacion (1,0) en el bit menos significativo
  RLF entrada,W    ; W= entrada<<1
  XORLW 0x02       ; Logica negativa (Comentar esta linea para log. positiva)
  MOVWF PORTB      ; Actualizar puerto B
  RETURN

;*****************************************************
;* Activar la interrupcion de cambio del puerto B
;* ENTRADA: Ninguna
;* SALIDA:  Ninguna
;* DEVUELVE: Nada
;*****************************************************
portb_int_enable
  ;-- Quitar flag de interrupcion, por si estuviese activado
  MOVFW PORTB
  BCF INTCON,RBIF  
  
  ;-- Activar la interrupcion de cambio
  BSF INTCON,RBIE  
  RETURN

;****************************************************************
;* Servicio de estado. Se devuelve el estado de la plataforma   *
;****************************************************************
serv_estado

  ;--Desactivar la interrupcion de cambio mientras
  ;--se envia la trama
  BCF INTCON,RBIE
  
  ;-- Enviar codido de respuesta
  MOVLW RESTADO 
	CALL sci_sendcar
  
  ;-- Enviar el estado del pulsador
  MOVFW entrada
  CALL sci_sendcar
  
  ;-- Activar la interrupcion de cambio
  BSF INTCON,RBIE  

  RETURN

;*************************************************************************
;*                        PROGRAMA PRINCIPAL
;*************************************************************************
inicio

;-----------------------------
;- CONFIGURAR EL PUERTO B           
;-----------------------------
;-- Pines E/S: RB0,RB4 entradas, resto salidas
  BSF STATUS,RP0      ; Cambiar al banco 1
  MOVLW 0x11
  MOVWF TRISB
  
;-- Pull-ups del puerto B habilitados
;-- Prescaler del timer0 a 256
;--   RBPU = 0, INTEDG=0, T0CS=0, T0SE=0, PSA=0, [PS2,PS1,PS0]=111
  MOVLW 0x07
  MOVWF OPTION_REG
  
;----------------------------------------------
;  CONFIGURACION DE LAS COMUNICACIONES SERIE
;----------------------------------------------
  CALL sci_configuration

;----------------------------------------------
;- CONFIGURACION DEL TEMPORIZADOR 0
;----------------------------------------------
  ;-- Quitar flag de interrupcion, por si estuviese activado
  BCF INTCON,T0IF   ;  Quitar flag overflow
  
  ;-- Activar temporizador. Dentro de un tick interrupcion
  BCF STATUS,RP0    ; Cambiar al banco 0
  MOVLW TICK
  MOVWF TMR0

;----------------------------------------------
;- CONFIGURACION DEL TEMPORIZADOR 1
;----------------------------------------------
 
  ;-- Activar temporizador
  BCF STATUS,RP0    ; Acceso al banco 0
  MOVLW 0x31
  MOVWF T1CON
  
  ;-- Ponerlo a cero
  CLRF TMR1HH
  CLRF TMR1H
  CLRF TMR1L
  
  ;-- Habilitar interrupcion
  BSF STATUS,RP0      ; Cambiar al banco 1
  BSF PIE1,TMR1IE
  BCF STATUS,RP0    ; Acceso al banco 0

;----------------------------
;- Interrupciones puerto B
;----------------------------
  ;-- Esperar a que se estabilice puerto B
  CALL pausa
  
  ;-- Habilitar la interrupcion de cambio en puerto B
  CALL portb_int_enable
  
;------------------------------
;- INICIALIZAR LAS VARIABLES
;------------------------------
  ;-- Inicializar contador extendido
  CLRF TMR1HH
  
  ;-- Inicializar el contador ANTIRREBOTES
  MOVLW TIEMPO_ANTIRREBOTES
  MOVWF CONTANTI
  
  ;-- Inicializar automata
  MOVLW EST_ESPERANDO_EVENTO
  MOVWF estado
  
  ;-- Inicialmente el sistema esta en Reset
  MOVLW 1
  MOVWF reset

  ;-- Leer estado de la entrada y actualizar la variable entrada
  CALL leer_entrada
  MOVWF entrada
  
;----------------------
;- ESTADO INICIAL LED 
;----------------------
  ;-- Actualizar led con el estado estable de la entrada
  CALL actualizar_led
  
;--------------------------
;- Interrupcion TIMER 0
;--------------------------
  BSF INTCON,T0IE   ; Activar interrupcion overflow TMR0  

;------------------------------------------
;- ACTIVAR LAS INTERRUPCIONES GLOBALES
;- Que comience la fiesta!!! 
;------------------------------------------
  ;-- Activar las interrupciones de los perifericos
  BSF INTCON,PEIE
  
  ;-- Activar interrupciones globales
  BSF INTCON,GIE 

;****************************
;*   BUCLE PRINCIPAL. 
;****************************
main
  CLRWDT      ;-- usado en la simulacion
  
  ;-- Analizar puerto serie por si viene alguna trama
  BTFSS PIR1,RCIF   ; RCIF=1?
  GOTO automata     ; no--> Ir al automata
  
  ;-- Ha llegado un caracter: leerlo
  call sci_readcar
  MOVWF car
  
  MOVLW TESTADO  		;  Trama de estado?
	SUBWF car,W
	BTFSC STATUS,Z
	CALL serv_estado	;  Si--> Servicio de estado de la plataforma
  
automata  
  ;-----------------------------------------------------
  ;- SEGUN EL ESTADO DEL AUTOMATA SALTAR A LA RUTINA
  ;- QUE LO TRATA                                           
  ;------------------------------------------------------
  MOVLW EST_ESPERANDO_EVENTO        ;  estado = ESPERANDO_EVENTO?
  SUBWF estado,W
  BTFSC STATUS,Z
  GOTO est_esperando_evento
  
  MOVLW EST_ANTIRREBOTES            ; estado = ANTIRREBOTES?
  SUBWF estado,W
  BTFSC STATUS,Z
  GOTO est_antirrebotes
  
  MOVLW EST_TRAMAX                  ; estado = TRAMAX?
  SUBWF estado,W
  BTFSC STATUS,Z
  GOTO est_tramax
  
  ;-- WARNING WARNING WARNING!!! Aqui no deberia llegar
  GOTO main
 
;----------------------------
;- ESTADO ESPERANDO EVENTO  
;----------------------------
est_esperando_evento
  GOTO main

;----------------------------
;- ESTADO ANTIRREBOTES
;----------------------------
est_antirrebotes
  MOVF CONTANTI,F       ; cont_antirrebotes=0?
  BTFSS STATUS,Z
  goto main             ; No--> Todavia no ha terminado
  
  ;-- Fin del antirrebotes
  ;-- Quitar flag de interrupcion del puerto B: para limpiar. No nos
  ;-- interesa lo que haya venido durante ese tiempo
  MOVFW PORTB
  BCF INTCON,RBIF 
  
  ;-- entrada_nueva = estado de la entrada
  CALL leer_entrada   
  MOVWF entrada_nueva
  
  ;-- Comparar la nueva entrada con la estable
  MOVFW entrada_nueva          ;  entrada_nueva==entrada?
  SUBWF entrada,W
  BTFSC STATUS,Z
  GOTO pulso_espureo           ; Si--> Es un pulso espureo
  
  ;-- entrada!=entrada_nueva: Ha ocurrido un cambio estable
  
  ;-- Almacenar la nueva entrada estable
  MOVFW entrada_nueva
  MOVWF entrada
   
  ;-- Pasar al estado TRAMAX para enviar la trama con el evento
  MOVLW EST_TRAMAX
  MOVWF estado
  
  GOTO main
 
pulso_espureo 
  ;-- Ha venido un pulso espureo (cambio con duracion
  ;-- menor que el tiempo de antirrebotes). Se ignora.
  ;-- Se continua como si nada hubiese ocurrido
  ;-- El valor de contador debe ser el actual + TIMESTAMP
  ;-- TMR1 = TIMR1 + TIMESTAMP
  
  MOVFW TIMESTAMP_L
  ADDWF TMR1L,F
  ;-- Sumar acarreo si lo hay
  BTFSC STATUS,C   ;-- Carry = 1?
  INCF TMR1H,F     ;-- Si--> Sumarselo a la parte alta
                   ;-- No--> Continuar
 
  MOVFW TIMESTAMP_H
  ADDWF TMR1H,F
  ;-- Sumar acarreo si lo hay
  BTFSC STATUS,C   ;-- Carry = 1?
  INCF TMR1HH,F    ;-- Si--> Sumarselo a la parte de mayor peso                   ;-- No--> Continuar
  
  MOVFW TIMESTAMP_HH
  ADDWF TMR1HH,F

  ;-- Pasar al estado esperando evento
  MOVLW EST_ESPERANDO_EVENTO
  MOVWF estado 
  
  ;-- Activar interrupcion puerto B
  CALL portb_int_enable
  
  GOTO main

;----------------------------
;- ESTADO TRAMAX 
;----------------------------
est_tramax
  ;-- Enviar trama de cambio en entrada
  ;-- Primero el identificador de trama
  MOVLW TCHANGE
  CALL sci_sendcar
  
  ;-- Enviar el estado del pulsador
  MOVFW entrada
  CALL sci_sendcar
  
  ;-- Enviar la marca de tiempo
  MOVFW TIMESTAMP_HH 
  CALL sci_sendcar
  MOVFW TIMESTAMP_H
  CALL sci_sendcar
  MOVFW TIMESTAMP_L
  CALL sci_sendcar
  
  ;-- Pasar al siguiente estado
  MOVLW EST_ESPERANDO_EVENTO
  MOVWF estado
  
  ;-- Actualizar estado del led, segun el estado de la entrada estable
  CALL actualizar_led
  
  ;-- Activar interrupcion del puerto B
  CALL portb_int_enable
  
  GOTO main

  END
