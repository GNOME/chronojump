
#This version uses only one formula commented in each factor. It returns the mm advanced each ms.
getMovement <- function(
        encoderSignal, #ticks per ms coming from the encoder
        inverted,  # The encoder is inverted if in concentric movement the signal is negative
        #-1 (encoder inverted) or 1 (encoder NOT inverted). 
        gearedDown, # speed of person divided by speed of machine. Used in machines with moving pulleys
        encoderOnMachine,  # 1 if the encoder measures directly the movement of the machine. 0 if the encoder measures the movememt of the body
        encoderType, 
        diameterRope, # diameter where the rope of the machine is wrapped.
        diameterEncoder)        # In encoders "LINEAR" or "ROTARYAXISFRICTION", diameter where the encoder are measuring.
                                # If the encoder and the rope are attached to the same point diameterRope=diameterEncoder or
                                # If gravitatory, in most cases diameterRope = diameterEncoder
{
        # On AXIS encoder movement  is R * (signal/200) * (2*pi) = signal * D * pi / 200
        # We asume D / (200 / pi) = D / d ----> d = 200 / pi
        
        # It should be assignet in the interface if encoderType = "ROTARYAXIS"
        if(encoderType == "ROTARYAXIS"){
                diameterEncoder == 200 / pi
        }
        movementBody = (
                encoderSignal
                * inverted           #Change the sign of the signal if is inverted (-1) and do nothing if not inverted (1)
                * (diameterRope / diameterEncoder) #On ROTARYFRICTION or LINEAR, if the encoder is not in the same diameter
                #as the rope this proportion gives the realmovement of the rope
                * gearedDown          #If the encoder is attached to the machine movementBody is multiplied by this factor
                ^ encoderOnMachine    #If the encoder is attached to the machine (1) gearedDown is multiplied
        )                       #If the encoder is attached to the the body (0) gearedDown is not used
        
        movementMachine = (
                encoderSignal
                * inverted                         #Change the sign of the signal if is inverted (-1) and do nothing if not inverted (1)
                * (diameterRope / diameterEncoder) #On ROTARYFRICTION, if the encoder is not in the same diameter
                #as the rope this proportion gives the realmovement of the rope
                / gearedDown                       #If the encoder is attached to the body movementMachine is divided by this factor
                ^ (1 - encoderOnMachine)           #If the encoder is attached to the body (0) gearedDown is divided
        )                                    #If the encoder is attached to the the body (1) gearedDown is not used
        
        return(list(body = movementBody, machine = movementMachine))
}


getDynamics <- function(
        movementBody, #movement of the body
        movementMachine, #movement of the load. mm in gravitatory. Rad in inertial
        encoderType, # "LINEAR" , "ROTARYFRICTION" or "ROTARYAXIS"
        anglePush, # Angle from the floor to the line of movement of the person
        angleWeight, # Angle from the floor to the line of movement of the resistance
        diameterRope, # Diameter of the axis
        diameterEncoder, # Diameter where Linear or RotaryFriction encoder are attached
        massBody, # mass in Kg of the person
        exerciseBodyWeight, # Percentage of the body moved in this exercise
        massLoad) # mass of the extra load
{
        forceMachineInertial = abs(inertiaMomentum * angleAccel) * (2 / diameter.m) #Force raveived by the inertal machine
        forceMachineGravitatory = massExtra*(g*sin(angleWeight * pi / 180) + accelMachine) #Force received by gravitatory machine
        forceBody = massBody * (exerciseBodyWeight / 100) * accelBody #Force received by the body
        forceBodyWeight = g * sin(anglePush * pi / 180 # Body Weight force
                                  powerMachineInertial = abs((inertiaMomentum * angleAccel) * angleSpeed) #Power received by the inertial machine
                                  powerMachineGravitatory = forceMachineGravitatory * speedMachine #Power received by the gravitatory machine
                                  powerBody = abs( (massBody * accelBody + massLoad * g * sin(anglePush * pi / 180)) * speed) #Total power received by the body
                                  
                                  force = forceMachineInertial + forceMachineGravitatory + forceTotalBody + forceBodyWeight
                                  power = powerMachineInertial + PowerMachineGravitatory + powerBody
                                  
                                  return(list(
                                          massBody = massBody,
                                          force = force,
                                          power = power,
                                          forceMachineInertial = forceMachineInertial,
                                          forceMachineGravitatory = forceMachineGravitatory,
                                          forceBody = forceBody,
                                          forceBodyWeight = forceBodyWeight,
                                          powerMachineInertial = powerMachineInertial,
                                          PowerMachineGravitatory = PowerMachineGravitatory,
                                          powerBody = powerBody))
}

################### Not necessary if the above function is used###################################################
#
# #TODO. It doesn't work with ROTARYAXIS
# getMovementGravitatory <- function(encoderSignal,     #number of ticks advanced every ms in the encoder.
#                                    inverted,          # The encoder is inverted if in concentric movement the signal is negative
#                                    #-1 (encoder inverted) or 1 (encoder NOT inverted). 
#                                    gearedDown,        # speed of person divided by speed of machine. Used in machines with moving pulleys
#                                    encoderOnMachine,  # TRUE if the encoder measures directly the movement of the body.
#                                    # FALSE the encoder measures the movememt of the load
#                                    encoderType        # LINEAR , ROTARYFRICTION or ROTARYAXIS
#                                    diameterRope,      # diameter where the rope of the machine is wrapped.
#                                    diameterEncoder)   # In encoders "LINEAR" or "ROTARYAXIS", diameter where the encoder are measuring.
#   # If the encoder and the rope are attached to the same point diameterRope=diameterEncoder) 
# {
#   if(gearedDown == 1){          # The body and the machine have the same movement
#     movementBody = encoderSignal * (diameterRope / diameterEncoder)
#     movementMachine = encoderSignal * diameterRope / diameterEncoder
#   }
# } else if(gearedDown != 1){   # The body moves gearedDown times faster than the machine
#   if (encoderOnMachine == 0){
#     movementBody = encoderSignal * inverted * (diameterRope / diameterEncoder)
#     movementMachine = encoderSignal * inverted * (diameterRope / diameterEncoder) / gearedDown
#   } else if(encoderOnMachine == 1){
#     movementBody = encoderSignal * inverted * (diameterRope / diameterEncoder) * gearedDown
#     movementMachine = encoderSignal * inverted * (diameterRope / diameterEncoder)
#   }
# }
# return(list(movementBody = movementBody, movementMachine = movementMachine))
# }
# 
# getMovementInertial <- function(encoderSignal, # ticks per ms coming from the encoder
#                                 encoderType, # "LINEAR", "ROTARYFRICTION" OR "ROTARYAXIS"
#                                 ticksPerRevolution, # Technical specifiaction of the encoder. In ROTARYAXIS encoders, number of ticks every revolution
#                                 diameterRope, # diameter where the rope of the machine is wrapped
#                                 diameterEncoder) # In encoders "LINEAR" or "ROTARYAXIS", diameter where the encoder are measuring.
#   # If the encoder and the rope are attached to the same point diameterRope=diameterEncoder
# {
#   if(gearedDown == 1){
#     if (encoderType == "LINEAR" || encoderType == "ROTARYFRICTION"){ # Both types of encoders measures mm per ms
#       movementBody = encoderSignal * diameterRope / diameterEncoder
#       movementMachine = encoderSignal * 2 / diameterEncoder # The movement is measured in rad/ms
#     } else if(encoderType == "ROTARYAXIS"){
#       movementBody = encoderSignal * (2 * pi / ticksPerRevolution) * (diameterRope / 2) * gearedDown
#       movementMachine = encoderSignal * (2 * pi / ticksPerRevolution) # The movement is measured in rad/ms
#     }
#   }
#   return(list(movementBody = movementBody, movementMachine = movementMachine))
# }
#
################################################################################################################