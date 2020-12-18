
require(shorts)
WorlChampionshipSplitTimes <- read.csv2("~/chronojump/r-scripts/tests/shortsLibrary/WorlChampionshipSplitTimes.csv")

results = matrix(nrow=length(WorlChampionshipSplitTimes[,1]), ncol = 16)
colnames(results) = c("Vmax3P", "Tau3P"
                      , "Vmax3CorrectedP", "Tau3CorrectedP"
                      , "Vmax3CorrectedS", "Tau3CorrectedS"
                      , "Vmax4P", "Tau4P"
                      , "Vmax4CorrectedP", "Tau4CorrectedP"
                      , "Vmax4S", "Tau4S"
                      , "Vmax4CorrectedS", "Tau4CorrectedS"
                      , "Vmax5CorrectedS", "Tau5CorrectedS"
)

for(i in 1:length(WorlChampionshipSplitTimes[,1])){
    
    splitTimes5 = data.frame(
        position = c(10,20,30,40, 50),
        time = c(WorlChampionshipSplitTimes[i,5], WorlChampionshipSplitTimes[i,6], WorlChampionshipSplitTimes[i,7], WorlChampionshipSplitTimes[i,8], WorlChampionshipSplitTimes[i,9])
    )
    
    splitTimes4 = data.frame(
        position = c(10,20,30,40),
        time = c(WorlChampionshipSplitTimes[i,5], WorlChampionshipSplitTimes[i,6], WorlChampionshipSplitTimes[i,7], WorlChampionshipSplitTimes[i,8])
    )
    
    
    # Padu's 3 split times model without correction
    splitTimes3 = data.frame(
        position = c(10,20,30),
        time = c(WorlChampionshipSplitTimes[i,5], WorlChampionshipSplitTimes[i,6], WorlChampionshipSplitTimes[i,7])
    )
    
    model = nls(position ~ Vmax*(time + (1/K)*exp(-K*time)) -Vmax/K, splitTimes3
                , start = list(K = 0.81, Vmax = 10), control=nls.control(maxiter=1000, warnOnly=TRUE))
    # print(paste("P --- Vmax:", summary(model)$parameters[2], "Tau:", 1/summary(model)$parameters[1]))
    results[i, "Vmax3P"] = summary(model)$parameters[2]
    results[i, "Tau3P"] = summary(model)$parameters[1]
    
    #Shorts 3 split times model with correction (Padu's correction)
    model = getModelWithOptimalTimeCorrection(splitTimes3)
    results[i, "Vmax3CorrectedS"] = summary(model)$parameters[1]
    results[i, "Tau3CorrectedS"] = summary(model)$parameters[2]
    
    # Padu's 4 split times model without correction
    model = nls(position ~ Vmax*(time + (1/K)*exp(-K*time)) -Vmax/K, splitTimes4
                , start = list(K = 0.81, Vmax = 10), control=nls.control(maxiter=1000, warnOnly=TRUE))
    # print(paste("P --- Vmax:", summary(model)$parameters[2], "Tau:", 1/summary(model)$parameters[1]))
    results[i, "Vmax4P"] = summary(model)$parameters[2]
    results[i, "Tau4P"] = summary(model)$parameters[1]
    
    #Shorts 4 split times model without correction
    model = with(
        splitTimes4,
        model_using_splits(position, time)
    )
    # print(paste("Shorts --- Vmax:", summary(model)$parameters[1], "Tau:", 1/summary(model)$parameters[2]))
    results[i, "Vmax4S"] = summary(model)$parameters[1]
    results[i, "Tau4S"] = summary(model)$parameters[2]
    
    #Shorts 4 split times model with time correction
    model = with(
        splitTimes4,
        model_using_splits_with_time_correction(position, time)
    )
    # print(paste("Shorts --- Vmax:", summary(model)$parameters[1], "Tau:", 1/summary(model)$parameters[2]))
    results[i, "Vmax4CorrectedS"] = summary(model)$parameters[1]
    results[i, "Tau4CorrectedS"] = summary(model)$parameters[2]    
    
    #Shorts 5 split times model with time and distance correction
    model = with(
        splitTimes5,
        model_using_splits_with_time_correction(position, time)
    )
    # print(paste("Shorts --- Vmax:", summary(model)$parameters[1], "Tau:", 1/summary(model)$parameters[2]))
    results[i, "Vmax5CorrectedS"] = summary(model)$parameters[1]
    results[i, "Tau5CorrectedS"] = summary(model)$parameters[2]
    
}

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
print(results)
