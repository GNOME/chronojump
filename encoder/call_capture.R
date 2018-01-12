# 
#  This file is part of ChronoJump
#   Copyright (C) 2016-2017  	Xavier de Blas <xaviblas@gmail.com> 
# 

#Is A LOT faster to call this file from C#, and this file will do a source(scriptCaptureR)
#than calling that file directly
#


args <- commandArgs(TRUE)

optionsFile <- args[1]

options <- scan(optionsFile, comment.char="#", what=character(), sep="\n")

source(paste(options[4], "/util.R", sep=""))

scriptCaptureR = options[1]

#comes ".../chronojump-captured"
#will be ".../chronojump-captured-000.txt", 001 ... 999
filenameBegins = options[2]

source(scriptCaptureR)

DEBUG <- FALSE
DebugFileName <- paste(options[5], "/chronojump-debug.txt", sep="")

CROSSVALIDATESMOOTH <- FALSE

f <- file("stdin")
open(f)

doProcess(options)
write("Ending capture.R", stderr())
quit()
