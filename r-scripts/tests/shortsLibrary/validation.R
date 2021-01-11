
require(shorts)

getModelWithOptimalTimeCorrection <- function(split_times)
{
    # print("In getModelWithOptimalTimeCorrection()")
    bestTimeCorrection = 0
    currentTimeCorrection = bestTimeCorrection
    
    model <- with(
        split_times,
        model_using_splits(position, time, time_correction = bestTimeCorrection)
    )
    
    # print("### Without correction ###")
    # print(model)
    
    minError = 1E6
    
    #TODO: Use better algorithm for finding optimal correction
    while(model$model_fit$RSE < minError){
        minError = model$model_fit$RSE
        # print(paste("New minError:", minError))
        # print(paste("current RSE:", model$model_fit$RSE))
        currentTimeCorrection = currentTimeCorrection + 0.001
        # Simple model
        model <- with(
            split_times,
            model_using_splits(position, time, time_correction = currentTimeCorrection)
        )
        # print(model$model_fit$RSE)
        if (model$model_fit$RSE < minError){
            bestTimeCorrection = currentTimeCorrection
        }
    }
    
    model <- with(
        split_times,
        model_using_splits(position, time, time_correction = bestTimeCorrection)
    )
    
    # print("### With optimal correction ###")
    # print(paste("Time correction:", bestTimeCorrection))
    # print(model)
    
    return(model)
}
WorlChampionshipSplitTimes <- read.csv2("~/chronojump/r-scripts/tests/shortsLibrary/WorlChampionshipSplitTimes.csv")

resultsColumns = c("Vmax3P", "Tau3P", "RSE3P"
          , "Vmax3CorrectedP", "Tau3CorrectedP", "RSE3CorrectedP"
          , "Vmax3S", "Tau3S", "RSE3S"
          , "Vmax3CorrectedS", "Tau3CorrectedS", "RSE3CorrectedS"
          , "Vmax4P", "Tau4P", "RSE4P"
          , "Vmax4CorrectedP", "Tau4CorrectedP", "RSE4CorrectedP"
          , "Vmax4S", "Tau4S", "RSE4S"
          , "Vmax4CorrectedS", "Tau4CorrectedS", "RSE4CorrectedS"
          , "Vmax5CorrectedS", "Tau5CorrectedS", "RSE5CorrectedS"
)
results = matrix(nrow=length(WorlChampionshipSplitTimes[,1]), ncol = length(resultsColumns))
colnames(results) = resultsColumns

for(i in 1:length(WorlChampionshipSplitTimes[,1])){
    
    splitTimes5 = data.frame(
        position = c(10,20,30,40, 50),
        time = c(WorlChampionshipSplitTimes[i,5], WorlChampionshipSplitTimes[i,6], WorlChampionshipSplitTimes[i,7], WorlChampionshipSplitTimes[i,8], WorlChampionshipSplitTimes[i,9])
    )
    
    splitTimes4 = data.frame(
        position = c(10,20,30,40),
        time = c(WorlChampionshipSplitTimes[i,5], WorlChampionshipSplitTimes[i,6], WorlChampionshipSplitTimes[i,7], WorlChampionshipSplitTimes[i,8])
    )
    
    
    splitTimes3 = data.frame(
        position = c(10,20,30),
        time = c(WorlChampionshipSplitTimes[i,5], WorlChampionshipSplitTimes[i,6], WorlChampionshipSplitTimes[i,7])
    )
    
    # Padu's 3 split times model without correction
    model = nls(position ~ Vmax*(time + (1/K)*exp(-K*time)) -Vmax/K, splitTimes3
                , start = list(K = 0.81, Vmax = 10), control=nls.control(warnOnly=TRUE))
    # print(paste("P --- Vmax:", summary(model)$parameters[2], "Tau:", 1/summary(model)$parameters[1]))
    results[i, "Vmax3P"] = summary(model)$parameters[2]
    results[i, "Tau3P"] = summary(model)$parameters[1]
    results[i, "RSE3P"] = summary(model)$sigma
    
    #Shorts 3 split times model without correction
    model = with(
        splitTimes3,
        model_using_splits(position, time)
    )
    results[i, "Vmax3S"] = summary(model)$parameters[1]
    results[i, "Tau3S"] = summary(model)$parameters[2]
    results[i, "RSE3S"] = model$model_fit$RSE
    
    #Shorts 3 split times model with correction (Padu's correction)
    model = getModelWithOptimalTimeCorrection(splitTimes3)
    results[i, "Vmax3CorrectedS"] = summary(model)$parameters[1]
    results[i, "Tau3CorrectedS"] = summary(model)$parameters[2]
    results[i, "RSE3CorrectedS"] = model$model_fit$RSE
    
    # Padu's 4 split times model without correction
    model = nls(position ~ Vmax*(time + (1/K)*exp(-K*time)) -Vmax/K, splitTimes4
                , start = list(K = 0.81, Vmax = 10), control=nls.control(warnOnly=TRUE))
    # print(paste("P --- Vmax:", summary(model)$parameters[2], "Tau:", 1/summary(model)$parameters[1]))
    results[i, "Vmax4P"] = summary(model)$parameters[2]
    results[i, "Tau4P"] = summary(model)$parameters[1]
    results[i, "RSE4P"] = summary(model)$sigma
    
    # Padu's 4 split times model with correction
    model = nls(position ~ Vmax*(time + T0 + (1/K)*exp(-K*(time + T0))) -Vmax/K, splitTimes4
                , start = list(K = 0.81, Vmax = 10, T0 = 0.2), control=nls.control(warnOnly=TRUE))
    # print(paste("P --- Vmax:", summary(model)$parameters[2], "Tau:", 1/summary(model)$parameters[1]))
    results[i, "Vmax4CorrectedP"] = summary(model)$parameters[2]
    results[i, "Tau4CorrectedP"] = summary(model)$parameters[1]
    results[i, "RSE4CorrectedP"] = summary(model)$sigma
    
    #Shorts 4 split times model without correction
    model = with(
        splitTimes4,
        model_using_splits(position, time)
    )
    # print(paste("Shorts --- Vmax:", summary(model)$parameters[1], "Tau:", 1/summary(model)$parameters[2]))
    results[i, "Vmax4S"] = summary(model)$parameters[1]
    results[i, "Tau4S"] = summary(model)$parameters[2]
    results[i, "RSE4S"] = model$model_fit$RSE
    
    #Shorts 4 split times model with time correction
    model = with(
        splitTimes4,
        model_using_splits_with_time_correction(position, time)
    )
    # print(paste("Shorts --- Vmax:", summary(model)$parameters[1], "Tau:", 1/summary(model)$parameters[2]))
    results[i, "Vmax4CorrectedS"] = summary(model)$parameters[1]
    results[i, "Tau4CorrectedS"] = summary(model)$parameters[2]
    results[i, "RSE4CorrectedS"] = model$model_fit$RSE
    
    #Shorts 5 split times model with time and distance correction
    model = with(
        splitTimes5,
        model_using_splits_with_time_correction(position, time)
    )
    # print(paste("Shorts --- Vmax:", summary(model)$parameters[1], "Tau:", 1/summary(model)$parameters[2]))
    results[i, "Vmax5CorrectedS"] = summary(model)$parameters[1]
    results[i, "Tau5CorrectedS"] = summary(model)$parameters[2]
    results[i, "RSE5CorrectedS"] = model$model_fit$RSE
    
}

print(results)
