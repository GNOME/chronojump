returnsNullList <- function ()
{
	ecc = NULL
	iso = NULL
	notNullScalar = 5
	notNullVector = c(9,4,7)
	con = NULL

	return (list(ecc=ecc, iso=iso,
				notNullScalar=notNullScalar,
				notNullVector=notNullVector,
				con=con))
}

myValues = returnsNullList ()
print ("ecc:")
print (myValues$ecc)
print ("iso:")
print (myValues$iso)
print ("notNullScalar:")
print (myValues$notNullScalar)
print ("notNullVector:")
print (myValues$notNullVector)
print ("con:")
print (myValues$con)

