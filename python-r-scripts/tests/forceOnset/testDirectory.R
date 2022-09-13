source("~/chronojump/r-scripts/maximumIsometricForce.R")
setwd("~/chronojump/r-scripts/tests/forceOnset/")

print("Reading files")
files = list.files("./data/")
errors = matrix(nrow = length(files), ncol = 3)
colnames(errors) = c("file", "bestFit", "SD")
for(i in 1:length(files))
{
  print(files[i])
  errors[i, "file"] = files[i]
  dynamics = getDynamicsFromLoadCellFile(op$captureOptions, paste("./data/", files[i], sep = ""), op$averageLength, op$percentChange, bestFit = TRUE, testLength = -1)
  errors[i, "bestFit"] = dynamics$meanError
  dynamics = getDynamicsFromLoadCellFile(op$captureOptions, paste("./data/", files[i], sep = ""), op$averageLength, op$percentChange, bestFit = FALSE, testLength = -1)
  errors[i, "SD"] = dynamics$meanError
}

write.csv2(errors, "~/chronojump/r-scripts/tests/forceOnset/errors.csv")