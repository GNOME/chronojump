require(shorts)
bolt = data.frame(distance = c(10, 20, 30, 40, 50, 60, 70, 80, 90, 100)
                  , time = cumsum(c(1.85, 1.02, 0.91, 0.87, 0.85, 0.82, 0.82, 0.82, 0.83, 0.90)))
bolt = data.frame(time = bolt$time, position = bolt$distance, distance = bolt$distance)

WorlChampionshipSplitTimes <- read.csv2("~/chronojump/r-scripts/tests/shortsLibrary/WorlChampionshipSplitTimes.csv")

split_times3 <- data.frame(
    distance = c(10, 20, 30),
    time = c(1.614, 2.821, 3.966)
)


# get the model adjusting the time_correction to the best fit
getModelWithOptimalTimeCorrection <- function(split_times)
{
    print("In getModelWithOptimalTimeCorrection()")
    bestTimeCorrection = 0
    currentTimeCorrection = bestTimeCorrection
    
    model <- with(
        split_times,
        model_using_splits(distance, time, time_correction = bestTimeCorrection)
    )
    # 
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
            model_using_splits(distance, time, time_correction = currentTimeCorrection)
        )
        # print(model$model_fit$RSE)
        if (model$model_fit$RSE < minError){
            bestTimeCorrection = currentTimeCorrection
        }
    }
    
    model <- with(
        split_times,
        model_using_splits(distance, time, time_correction = bestTimeCorrection)
    )
    
    print("### With optimal correction ###")
    print(paste("Time correction:", bestTimeCorrection))
    print(model)
    
    return(model)
}

### ¡¡¡The model needs 4 split times for adjusting the time!!!
split_times4 <- data.frame(
     distance = c(10, 20, 30, 40),
     time = c(1.893, 3.149, 4.313, 5.444)
)

# Model with time_correction estimation
model_with_time_correction_estimation <- with(
    split_times4,
    model_using_splits_with_time_correction(distance, time)
)

print("Model with 4 times")
print(model_with_time_correction_estimation)

### ¡¡¡The model needs 5 split times for adjusting the time!!!
split_times5 <- data.frame(
    distance = c(5, 10, 20, 30, 40),
    time = c(1.158, 1.893, 3.149, 4.313, 5.444)
)

# Model with time and distance correction estimation
model_with_time_distance_correction_estimation <- with(
    split_times5,
    model_using_splits_with_corrections(distance, time)
)

print(model_with_time_distance_correction_estimation)

kimberley_data <- filter(split_times, athlete == "Kimberley")
