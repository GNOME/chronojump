#http://stackoverflow.com/questions/26053302/is-there-a-way-to-use-standard-input-output-stream-to-communicate-c-sharp-and-r/26058010#26058010
f <- file("stdin")
open(f)

input <- readLines(f, n = 1L)
while(input[1] != "Q") {
	#Sys.sleep(4) #just to test how Chronojump reacts if process takes too long
	cat(paste("input is:", input, "\n"))
	input <- readLines(f, n = 1L)
}
quit()
