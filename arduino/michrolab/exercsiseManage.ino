//void readJumpsFile(){
//    /*
//     Example of jumpsType.txt
//     0,SJ,1
//     1,CMJ,1
//     2,ABK,1
//     3,DJna,0
//     4,SJl,1
//  */
//  String row = "";
//  char readChar;
//  File  jumpsFile = SD.open("/jumpType.txt");
//  if (jumpsFile)
//  {
//    currentJumpType = 0;
//    jumpsFile.seek(0);
//
//    // read from the file until there's nothing else in it:
//    while (currentJumpType < totalJumpTypes)
//    {
//      readChar = jumpsFile.read();
//      if (readChar != '\n' && readChar != '\r')
//      {
//        row = row + readChar;
//      } else if (readChar == '\n' || readChar == '\r')
//      {
//        //Serial.println(row);
//        addJump(row);
//        row = "";
//        currentJumpType++;
//      }
//    }
//    // close the file:
//    jumpsFile.close();
//    //printJumpTypesList();
//  } else {
//    // if the file didn't open, print an error:
//    Serial.println("error opening jumpType.txt");
//  }
//}

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

void readJumpsTypesFile()
{
  char readChar;
  String readString = "";
  unsigned long pos = 0;    //Position in the file
  int numRows = 0;          //Number of valid rows in the file
  
  File  jumpsFile = SD.open("jumpType.txt");
  if (jumpsFile)
  {
//    //Start reading from the last byte
//    unsigned long pos = jumpsFile.size() - 4;
//
//    //Reading the jump number of the last row
//    while (readChar != '\n' && readChar != '\r')
//    {
//      jumpsFile.seek(pos);
//      readChar = jumpsFile.peek();
//      pos--;
//    }
//    pos++;
//    jumpsFile.seek(pos);
//    readChar = jumpsFile.read();
//    while (readChar != ',')
//    {
//      readChar = jumpsFile.read();
//      readString = readString + readChar;
//    }
    while (pos <= jumpsFile.size())
    {
      readChar = NULL;
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
        Serial.print(readString);
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
