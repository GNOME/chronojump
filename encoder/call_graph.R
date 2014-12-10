# 
#  This file is part of ChronoJump
# 
#  ChronoJump is free software; you can redistribute it and/or modify
#   it under the terms of the GNU General Public License as published by
#    the Free Software Foundation; either version 2 of the License, or   
#     (at your option) any later version.
#     
#  ChronoJump is distributed in the hope that it will be useful,
#   but WITHOUT ANY WARRANTY; without even the implied warranty of
#    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
#     GNU General Public License for more details.
# 
#  You should have received a copy of the GNU General Public License
#   along with this program; if not, write to the Free Software
#    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
# 
#   Copyright (C) 2014-  	Xavier de Blas <xaviblas@gmail.com> 
# 

#Is A LOT faster to call this file from C#, and this file will do a source(scriptGraphR)
#than calling that file directly
#
#This call_graph.R is not written for this purpose,
#it's written to be called if RDotNet is not working. Then this fill will call graph.R
#
#if RDotNet works, then graph.R will be in memory and there's no need to write Roptions.txt,
#and there's no nead reading it from here because it will be done using RDotNet



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
