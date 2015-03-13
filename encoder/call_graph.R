# 
#  This file is part of ChronoJump
#   Copyright (C) 2014-2015  	Xavier de Blas <xaviblas@gmail.com> 
# 

#Is A LOT faster to call this file from C#, and this file will do a source(scriptGraphR)
#than calling that file directly


args <- commandArgs(TRUE)
optionsFile <- args[1]
print(optionsFile)

#--- user commands ---
getOptionsFromFile <- function(optionsFile, lines) {
	optionsCon <- file(optionsFile, 'r')
	options=readLines(optionsCon, n=lines)
	close(optionsCon)
	return (options)
}

#way A. passing options to a file
options <- getOptionsFromFile(optionsFile, 32)

#way B. put options as arguments
#unused because maybe command line gets too long
#options <- commandArgs(TRUE)

#print(options)

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

doProcess(options)
