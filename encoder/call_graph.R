# 
#  This file is part of ChronoJump
#   Copyright (C) 2014-2015  	Xavier de Blas <xaviblas@gmail.com> 
# 

#Is A LOT faster to call this file from C#, and this file will do a source(scriptGraphR)
#than calling that file directly


args <- commandArgs(TRUE)

optionsFile <- args[1]

options <- scan(optionsFile, comment.char="#", what=character(), sep="\n")


#---------------------------------------------------------------------
#			Attention
#this code should be the same as utilEncoder.cs RunEncoderGraphRDotNet
#---------------------------------------------------------------------

OutputData2 <- options[4] #currently used to display processing feedback
SpecialData <- options[5]
OperatingSystem <- options[27]
EncoderConfigurationName <- ""

English = unlist(strsplit(options[30], "\\;"))
Translated = unlist(strsplit(options[31], "\\;"))

#TODO: now that there's a start and continue of each process from R,
#instead of pass and script from R,
#pass the directory
#and R will source() the needed files (maybe all the first time)
#and it will not do again when the process is continued (see encoderRProc.cs)

scriptUtilR = options[28]
source(scriptUtilR)

scriptGraphR = options[32]

#Note:
#We just touch this files because in the past we created a unique status file from here
#and we update it
#but we read it at the same time from chronojump and this produces some crashes on windows
#now we just touch here, and in chronojump we just read if exist

#write(paste("(1/5)",translate("Starting R")), OutputData2)
print("Creating (OutputData2)1.txt with touch method...")
file.create(paste(OutputData2,"1.txt",sep=""))
print("Created")

source(scriptGraphR)

#write(paste("(2/5)",translate("Loading libraries")), OutputData2)
print("Creating (OutputData2)2.txt with touch method...")
file.create(paste(OutputData2,"2.txt",sep=""))
print("Created")

loadLibraries(OperatingSystem)
	
#write(paste("(3/5)",translate("Starting process")), OutputData2)
print("Creating (OutputData2)3.txt with touch method...")
file.create(paste(OutputData2,"3.txt",sep=""))
print("Created")

while(TRUE) {
	doProcess(options)

	#continue the process or exit
	f <- file("stdin")
	open(f)

	input <- readLines(f, n = 1L)
	if(input[1] == "Q")
		quit("no")
			
	write("received a continue signal", stderr())
	options <- getOptionsFromFile(optionsFile, 32)
	
	#TODO 1: check if all the Output2, SpecialData, ... variables have to be reassigned
	#TODO 2: check if neuromuscularProfile should be loaded
}
