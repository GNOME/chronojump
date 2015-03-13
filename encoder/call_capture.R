# 
#  This file is part of ChronoJump
#   Copyright (C) 2015  	Xavier de Blas <xaviblas@gmail.com> 
# 

#Is A LOT faster to call this file from C#, and this file will do a source(scriptCaptureR)
#than calling that file directly
#


args <- commandArgs(TRUE)
optionsFile <- args[1]
#print(optionsFile)

#--- user commands ---
getOptionsFromFile <- function(optionsFile, lines) {
	optionsCon <- file(optionsFile, 'r')
	options=readLines(optionsCon, n=lines)
	close(optionsCon)
	return (options)
}

options <- getOptionsFromFile(optionsFile, 32)

scriptUtilR = options[28]
source(scriptUtilR)

scriptCaptureR = options[1]

#comes ".../chronojump-captured"
#will be ".../chronojump-captured-000.txt", 001 ... 999
filenameBegins = options[2] 

source(scriptCaptureR)

f <- file("stdin")
open(f)

doProcess(options)
write("Ending capture.R", stderr())
quit()
