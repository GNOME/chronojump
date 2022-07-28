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
  
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[currentJumpType].startIn = (row.substring(prevComaIndex + 1 , nextComaIndex) == "1");
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
      //Check that it is a valid row
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
    Serial.print("," + jumpTypes[i].name + ",");
    Serial.println(jumpTypes[i].startIn);
  }
}
