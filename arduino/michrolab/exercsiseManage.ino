void readJumpsFile(){
    /*
     Example of jumpsTypes.txt
     0,SJ,1
     1,CMJ,1
     2,ABK,1
     3,DJna,0
     4,SJl,1
  */
  String row = "";
  char readChar;
  File  jumpsFile = SD.open("/jumpType.txt");
  if (jumpsFile)
  {
    currentJumpType = 0;
    jumpsFile.seek(0);

    // read from the file until there's nothing else in it:
    while (currentJumpType < totalJumpTypes)
    {
      readChar = jumpsFile.read();
      if (readChar != '\n' && readChar != '\r')
      {
        row = row + readChar;
      } else if (readChar == '\n' || readChar == '\r')
      {
        addJump(row);
        row = "";
        currentJumpType++;
      }
    }
    // close the file:
    jumpsFile.close();
  } else {
    // if the file didn't open, print an error:
    Serial.println("error opening jumpType.txt");
  } 
}

void addJump(String row)
{
  int prevComaIndex = row.indexOf(":");
  int nextComaIndex = row.indexOf(",");
  currentJumpType = row.substring(prevComaIndex + 1, nextComaIndex).toInt();
  jumpTypes[currentJumpType].id = currentJumpType;

  if (currentJumpType >= totalJumpTypes) totalJumpTypes = currentJumpType + 1;
  prevComaIndex = nextComaIndex;
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[currentJumpType].name = row.substring(prevComaIndex + 1 , nextComaIndex);
  prevComaIndex = nextComaIndex;
  
  nextComaIndex = row.indexOf(",", prevComaIndex + 1 );
  jumpTypes[currentJumpType].startIn = (row.substring(prevComaIndex + 1 , nextComaIndex) == "1");
}

unsigned int getTotalJumpTypes()
{
  char readChar;
  String readString = "";
  File  jumpsFile = SD.open("jumpType.txt");
  if (jumpsFile)
  {
    //Start reading from the last byte
    unsigned long pos = jumpsFile.size() - 4;

    //Reading the jump number of the last row
    while (readChar != '\n' && readChar != '\r')
    {
      jumpsFile.seek(pos);
      readChar = jumpsFile.peek();
      pos--;
    }
    pos++;
    jumpsFile.seek(pos);
    readChar = jumpsFile.read();
    while (readChar != ',')
    {
      readChar = jumpsFile.read();
      readString = readString + readChar;
    }
  }
  totalJumpTypes = readString.toInt() + 1;
  return (totalJumpTypes);
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
