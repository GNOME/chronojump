typedef void (*functionPointer) (void);

struct menuEntry {
    String title;
    String description;
    functionPointer function;
};

//Starts reading from the load cell alone
void startLoadCellCapture(void);

//Calculations of the load cell data: Max, Mean, Impulse, RFD...
void getLoadCellDynamics(void);

void endLoadCellCapture();

void showLoadCellResults();

//Starts reading from the encoder alone
void startEncoderCapture(void);

void getEncoderDynamics();

void showEncoderResults();

void endEncoderCapture();

//Sets the inertial mode on and sets the position of Con/Ecc change
void startInertialEncoderCapture();

//sets the position of Con/Ecc change
void calibrateInertial();

//Reads from the load cell and encoder simultaneously
void startPowerCapture(void);

void getPowerDynamics();

void readEncoder();

void endPowerCapture();

void showPowerResults();

//Starts to read from load cell and waits to press the red button to start the actual steadiness calculations
void startSteadiness(void);

void end_steadiness();

//Reads from the RCA to calculate jump heights
void startJumpsCapture();

//Measuring the selected sensors as well as plotting and saving to SD
//It also manage the buttons pressed
void capture();

//Prints a float number with the units number at the selected positoin and precission
void printTftFormat (float val, int x, int y, int fontSize, int decimal);

//Reads whatever it is in the serial buffer
void serialEvent();

//return the version of the firmware as well as the hardware type
void get_version();

//returns the load cell calibration factor
void get_calibration_factor();

//Sets the calibration factor of the load cell
void set_calibration_factor(String inputString);

//Process of the calibration. The inputString includes the load used to calibrate
void calibrate(String inputString);

//Process of offsetting the load cell
void tare();

//Process of tare and capture. Usefull when the body weight must be substracted from the load cell readings
void startTareCapture(void);

//Returns the offset of the load cell
void get_tare();

//Set the offset of the load cell
void set_tare(String inputString);

//Returns the argument of the command with the form [command]:[argument];
String get_command_argument(String inputString);

//Not used. Tells if the info sent to the Serial is in text or binary format
void get_transmission_format();

//Funcion called when the RCA state has changed
void changingRCA();

//Process of calibration controled from the tft
void calibrateTFT(void);

//function to read battery level. Not implemented in SportAnalyzer hardware version 1.0
void showBatteryLevel();

//Update the time in the graph every second
void updateTime();

//TODO: Add more information or eliminate
void showSystemInfo(void);

//Sets the horizontal line shown in steadiness measure
void setForceGoal();

//Saves the meadured data in the SD
void saveSD(String fileName);

//Count how many dirs exists. Used to create new dirs with the correct numeration
int countDirs();

//Process of changing the person with the blue button
void selectPerson();
