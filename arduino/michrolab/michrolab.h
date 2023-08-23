typedef void (*functionPointer) (void);

struct menuEntry {
    String title;
    String description;
    String abbreviation;
    functionPointer function;
};

//Starts reading from the load cell alone
void startLoadCellCapture(void);

//Calculations of the load cell data: Max, Mean, Impulse, RFD...
void getLoadCellDynamics(void);

void endLoadCellCapture();

void showLoadCellResults();

void resultsBackground();

//Starts reading from the encoder alone
void startEncoderCapture(void);

void getEncoderDynamics();

void showEncoderResults();

void endEncoderCapture();

//Sets the inertial mode on and sets the position of Con/Ecc change
void startInertEncoderCapture();

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
void jumpCapture();

//Measuring the selected sensors as well as plotting raw data and saving to SD
//It also manage the buttons pressed
void captureRaw();

//Measuring the selected sensors as well as plotting bars of each repetition and saving to SD
//It also manage the buttons pressed
void captureBars();

//Prints a float number with the units number at the selected positoin and precission
void printTftValue (float val, int x, int y, int fontSize, int decimal);

/*  Prints in the TFT a text. By default:
 *  color = WHITE
 *  fontSize = 2
 *  alignRight = false;
 */
 //Align of the text

//void printTftText(String text, int x, int y, unsigned int color, int fontSize, alignType align);

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
void changedRCA();

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
void saveData(String fileName);

//Saves the results of the current jump
void saveJump();

//Count how many dirs exists. Used to create new dirs with the correct numeration
int countDirs();

//Process of changing the person with the blue button
void selectPerson();

//Plot raw data in y axis over time horizontal axis
void Graph(ILI9341_t3 & d, double x, double y, double gx, double gy, double w, double h, double xlo, double xhi, double xinc, double ylo, double yhi, double yinc, String title, String xlabel, String ylabel, unsigned int gcolor, unsigned int acolor, unsigned int pcolor, unsigned int tcolor, unsigned int bcolor, boolean & startOver);

//Redraw axes and vertical labels of the graph
void redrawAxes(ILI9341_t3 & d, double gx, double gy, double w, double h, double xlo, double xhi, double ylo, double yhi, double yinc, String title, String xlabel, String ylabel, unsigned int gcolor, unsigned int acolor, unsigned int pcolor, unsigned int tcolor, unsigned int bcolor, unsigned int goalColor, boolean resize);

//Plot a set of bars stored in bars[]
void barPlot (float gx, float gy, float w, float h, float yhi, int numBars, int currentIndex, float abRatio, unsigned int color);

//Read the JUMPTYPE.TXT file and assign each row to a jumpTypes[] element
void readJumpsFile();

//Assign a jumpType to a jumpTypes[] element. The input String is of the same format as in the JUMPTYPE.TXT
void addJump(String row);

//Read how many rows has the JUMPTYPE.TXT
void gettotalJumpTypes();

//Print in the Serial a list of all jump types
void printJumpTypesList();

//Shows a simple report of the jump
void showJumpsResults(float maxJump, unsigned int bestJumper, int totalJumps);

//Goes to the next item in the array
unsigned int selectNextItem (int currentExerciseType, int arrayElements);

//Goes to the previous item in the array
unsigned int selectPreviousItem (int currentExerciseType, int arrayElements);

//Navigates through the System Menu items
void showSystemEntry(int currentMenuIndex);

//Shows the date and hour in format 10:39:4 2023/8/7 hh:mm:ss YYYY/M/D
void getRtcTime();

//Sets the time of Teensy RTC. Seconds since 1970/1/1 0h:0m:0s
void setRtcTime(String time);

//Update the state of all directional buttons
void updateButtons();
