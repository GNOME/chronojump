void addJump(String row)
{
  int prevComaIndex = row.indexOf(":");
  int nextComaIndex = row.indexOf(",");
  //currentJumpType = row.substring(prevComaIndex + 1, nextComaIndex).toInt();
  jumpTypes[currentJumpType].id = row.substring(prevComaIndex + 1, nextComaIndex).toInt();

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[currentJumpType].name = row.substring(prevComaIndex + 1 , nextComaIndex);
  prevComaIndex = nextComaIndex;

  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[currentJumpType].jumpLimit = row.substring(prevComaIndex + 1, nextComaIndex).toInt();
  prevComaIndex = nextComaIndex;
  
  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[currentJumpType].timeLimit = row.substring(prevComaIndex + 1, nextComaIndex).toFloat();
  prevComaIndex = nextComaIndex;
  
  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[currentJumpType].hardTimeLimit = (row.substring(prevComaIndex + 1 , nextComaIndex) == 1);
  prevComaIndex = nextComaIndex;
  
  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[currentJumpType].percentBodyWeight = row.substring(prevComaIndex + 1 , nextComaIndex).toFloat();
  prevComaIndex = nextComaIndex;
    
  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[currentJumpType].fall = row.substring(prevComaIndex + 1, nextComaIndex).toFloat();
  prevComaIndex = nextComaIndex;
  
  prevComaIndex = nextComaIndex;
  jumpTypes[currentJumpType].startIn = (row.substring(prevComaIndex + 1, prevComaIndex + 2) == "1");
}

void readJumpTypesFile()
{
  char readChar;
  String readString = "";
  unsigned long pos = 0;    //Position in the file
  int numRows = 0;          //Number of valid rows in the file
  
  File  jumpsFile = SD.open("jumpType.txt");
  if (jumpsFile)
  {
    while (pos <= jumpsFile.size())
    {
      readChar = '0';
      String readString = "";
      while (readChar != '\n' && readChar != '\r' && pos<=jumpsFile.size())
      {
        readChar = jumpsFile.read();
        readString = readString + readChar;
        pos++;
      }
      //Check that it is a valid row.
      if ( isDigit(readString[0]) )
      {
        numRows++;
        currentJumpType = numRows - 1;
        addJump(readString);
        totalJumpTypes = numRows;
      }
    }
  }
 jumpsFile.close();
}

void printJumpTypesList()
{
  for (unsigned int i = 0; i < totalJumpTypes; i++)
  {
    Serial.print(jumpTypes[i].id);
    Serial.print("," + jumpTypes[i].name + " ,");
    Serial.print(String( jumpTypes[i].jumpLimit , 2) + " ,");
    Serial.print(String( jumpTypes[i].timeLimit ) + "s ,");
    Serial.print(String( jumpTypes[i].hardTimeLimit) + " ,");
    Serial.print(String( jumpTypes[i].percentBodyWeight , 2) + "% ,");
    Serial.print(String( jumpTypes[i].fall , 2) + "cm ,");
    Serial.println(jumpTypes[i].startIn);
  }
}
