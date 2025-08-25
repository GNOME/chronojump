d <- read.csv2("/tmp/forceEncoder.csv")

#force sensor
dForce <- d[which(d$sensor==2),]
plot (1, xlim=c(1,max(d$time)), ylim=c(min(dForce$value),max(dForce$value)), type="n")
points (dForce$time, dForce$value)

# encoder
dEncoder <- d[which(d$sensor==3),]
dEncoderZ <- d[which(d$sensor==4),]

encoderCumsum = cumsum (dEncoder$value)
par(new = T)
plot (1, xlim=c(1,max(d$time)), ylim=c(min(encoderCumsum),max(encoderCumsum)), type="n", axes=F)
axis(4, col="green")
points (dEncoder$time, encoderCumsum, col="green")

abline (v=dEncoderZ$time, col="blue")


# debug:

# On Arduino IDE: start_capture:
# On Moserial register and store in:  encoderForce.txt
# hexdump -v -e '/1 "%u\n"' encoderForce.txt | while read c; do   echo $c; done > encoderForce_text.txt
# on R do:

d = scan ("encoderForce_text.txt")

#https://www.geeksforgeeks.org/r-language/how-to-split-vector-and-data-frame-in-r/
# Create sample vector
#my_vector <- 1:10

# Define the number of elements you want in each chunk
chunk_size <- 12

# Initialize an empty list to store chunks
chunks <- list()
chunks4Values <- list() #fer que aixo estigui mulltiplicat

# Iterate over the vector and extract subsets for each chunk
for (i in seq(1, length(d), by = chunk_size))
{
  # Determine the end index for the current chunk
  end_index <- min(i + chunk_size - 1, length(d))

  # Extract subset for the current chunk
  chunk <- d[i:end_index]
  #chunks4 <- d[i:(end_index-8)]
  chunks4 <- d[i] + 256*d[i+1] + (256^2)*d[i+2] + (256^3)*d[i+3]

  # Add the chunk to the list
  chunks[[length(chunks) + 1]] <- chunk
  chunks4Values[[length(chunks4Values) + 1]] <- chunks4
}

# Print the chunks
#print(chunks)
head(unlist(chunks4Values))

#for (i in 1:length(chunks))
#for (i in 1:200)
#{
#	print (unlist(chunks[i])[1:4])
#}

