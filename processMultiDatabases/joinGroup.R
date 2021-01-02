#Copyright (C) 2020 Aurora González-Vidal <aurora.gonzalez2@um.es>, Xavier de Blas <xaviblas@gmail.com> 

library("data.table")
library("readxl")
library("stringr")
library("dplyr")

#path <- ""
path <- "/home/xavier/Documents/academic/investigacio/Encoder_SITLESS_nogit/"

mergeAndWrite <- function (filename, data, centre = "BL")
{
	df <- fread(filename, sep=",", fill=T)
	#on belfast personCodes go to 1 to 334 but should go from 3001 to 3334
	if(centre == "BL") {
		df$personCode = df$personCode + 3000
		data = data[data$centre==3,]
	}
	if(centre == "DN") {
		data = data[data$centre==1,]
	}
	if(centre == "UL") {
		data = data[data$centre==4,]
	}	
	if(centre == "BA") {
		data = data[data$centre==2,]
	}
	data <- data[c(2,4,7)] # nos quedamos solo con la columna grupo y la del identificador
	names(df)[10] = "exercise2" 	# En df hay dos columnas con el nombre "exercise", cambiamos la segunda
	df2 <- merge(df, data, by.x = "personCode", by.y ="participante", all = F)
	write.csv(df2, str_replace(filename, ".csv", "-added-group-age.csv"))
}

data <- read_excel(paste(path, "ID_GROUP.xlsx", sep=""))

#mergeAndWrite (paste(path, "chronojump-processMultiEncoder-belfast-done.csv", sep=""), data, centre = "BL")
#mergeAndWrite (paste(path, "chronojump-processMultiEncoder-belfast-done-biceps-4Kg.csv", sep=""), data, centre = "BL")
#mergeAndWrite (paste(path, "chronojump-processMultiEncoder-denmark-done.csv", sep=""), data, centre = "DN")
#mergeAndWrite (paste(path, "chronojump-processMultiEncoder-ulm-done.csv", sep=""), data, centre = "UL")
#mergeAndWrite (paste(path, "chronojump-processMultiEncoder-barcelona-done.csv", sep=""), data, centre = "BA")
mergeAndWrite (paste(path, "chronojump-processMultiEncoder-barcelona-done-biceps.csv", sep=""), data, centre = "BA")


# ---- start of initial code to test denmark data and check number of observations ---->

#df <- fread("/home/xavier/Documents/academic/investigacio/Encoder_SITLESS_nogit/chronojump-processMultiEncoder-denmark-done.csv", sep=",", fill=T)

#names(df)[10] = "exercise2" 	# En df hay dos columnas con el nombre "exercise", cambiamos la segunda
#df2 <- merge(df, data, by.x = "personCode", by.y ="participante", all = F)

#write.csv(df2, "/home/xavier/Documents/academic/investigacio/Encoder_SITLESS_nogit/chronojump-processMultiEncoder-denmark-done-added-group-age.csv")

# df2 es el dataset que te interesa, ¿por qué tiene menos observaciones que df?
#dfx <- merge(df, data, by.x = "personCode", by.y ="participante", all.x=T)
#notmatch <- setdiff(dfx,df2)
# Porque algunas observaciones no tienen ID, como se aprecia en "notmatch".

# <---- end of initial code to test denmark data and check number of observations ----

