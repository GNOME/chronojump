# 
#  This file is part of ChronoJump
#   Copyright (C) 2014-2017  	Xavier de Blas <xaviblas@gmail.com> 
# 

#Is A LOT faster to call this file from C#, and this file will do a source("graph.R")
#than calling that file directly


args <- commandArgs(TRUE)

optionsFile <- args[1]

options <- scan(optionsFile, comment.char="#", what=character(), sep="\n")


#---------------------------------------------------------------------
#			Attention
#this code should be the same as utilEncoder.cs RunEncoderGraphRDotNet
#---------------------------------------------------------------------

FeedbackFileBase <- paste(options[5], "/chronojump-encoder-status-", sep="")
SpecialData <- paste(options[5], "/chronojump-special-data.txt", sep="")
OperatingSystem <- options[28]

EncoderConfigurationName <- ""

English = unlist(strsplit(options[29], "\\;"))
Translated = unlist(strsplit(options[30], "\\;"))

DEBUG <- FALSE
DebugFileName <- paste(options[5], "/chronojump-debug.txt", sep="")

CROSSVALIDATESMOOTH <- FALSE

source(paste(options[4], "/util.R", sep=""))
source(paste(options[4], "/graphSmoothingEC.R", sep=""))


#Note:
#We just touch this files because in the past we created a unique status file from here
#and we update it
#but we read it at the same time from chronojump and this produces some crashes on windows
#now we just touch here, and in chronojump we just read if exist

print("Creating (FeedbackFileBase)1.txt with touch method...")
file.create(paste(FeedbackFileBase,"1.txt",sep=""))

source(paste(options[4], "/graph.R", sep=""))

print("Creating (FeedbackFileBase)2.txt with touch method...")
file.create(paste(FeedbackFileBase,"2.txt",sep=""))

loadLibraries(OperatingSystem)
	
#open stdin connection
f <- file("stdin")
open(f)


while(TRUE) {
	print("Creating (FeedbackFileBase)3.txt with touch method...")
	file.create(paste(FeedbackFileBase,"3.txt",sep=""))
	
	doProcess(options)

	input <- readLines(f, n = 1L)
	if(input[1] == "Q")
		quit("no")
			
	write("call_graph.R received a continue signal", stderr())
			
	#answer the ping
	#eg. input = 'PINGC:/Temp.../1234.txt'

	input=substring(input,5,) #input = 'C:/Temp.../1234.txt'
	file.create(input)
	print(paste(input, "created from call_graph.R"))
	write("created from call_graph.R", stderr())
	
	#Wait to the Continue "C"
	#Needed to prepare outputFileCheck files
	input <- readLines(f, n = 1L)
			
	options <- scan(optionsFile, comment.char="#", what=character(), sep="\n")
}
