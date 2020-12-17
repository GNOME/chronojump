### ¡¡¡The model needs 3 split times for adjusting without any correction!!!

require(shorts)

split_times3 <- data.frame(
  distance = c(10, 20, 30),
  time = c(1.708, 3.059, 4.334)
)

# Simple model
simple_model <- with(
    split_times3,
    model_using_splits(distance, time)
)

print(simple_model)

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
