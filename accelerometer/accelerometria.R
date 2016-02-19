# This file is part of ChronoJump
# 
# ChronoJump is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; either version 2 of the License, or   
# (at your option) any later version.
#    
# ChronoJump is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
# GNU General Public License for more details.
# 
# You should have received a copy of the GNU General Public License
# along with this program; if not, write to the Free Software
# Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
# 
# Copyright (C) 2014-   Xavier Padullés <x.padulles@gmail.com> 


getAbsPosition <- function(data)
{
        #data <- calibrate(data)
        #"Local" or "relative" means "in the reference of the the accelerometer axis"
        #"Absolute" or "global" means "in the floor reference"
        omega <- getOmega(data) #Read the data coming from the gyros
        localRotation <- getLocalRotationQuaternion(omega) # Translate the local rotation in degrees per second to quaternions
        localBasis <- getLocalBasis(localRotation) # Create a local basis expressed in the absolute basis. (200Hz)
        relAccel <- getRelAcc(data) # Read data coming from the accelerometers
        absAccel <- getAbsAcceleration(localBasis, relAccel) # Translate the local acceleration to the absolute acceleration
        absSpeed <- getAbsSpeed(absAccel) # Integrate the acceleration to thet the speed
        
        #Read de absolute speed and integrate it to get the absolute position
        abs_pos <- matrix(, ncol=3, nrow=length(absSpeed[,1]))
        abs_pos[1,] <- c(0,0,0)
        for (i in 2:length(absSpeed[,1])){
                abs_pos[i,1] <- abs_pos[(i-1),1] + absSpeed[(i-1),1]/200
                abs_pos[i,2] <- abs_pos[(i-1),2] + absSpeed[(i-1),2]/200
                abs_pos[i,3] <- abs_pos[(i-1),3] + absSpeed[(i-1),3]/200
        }
        return(abs_pos)
}

getOmega <- function(data)
{
        #Reads the gyroscope values. Omega is the rotation expressed in the local axes of the accelerometer.
        omega <- matrix(c(data$GYRO_X, data$GYRO_Y, data$GYRO_Z), ncol=3, byrow=FALSE)
        return(omega)
}

getLocalRotationQuaternion <- function(omega)
{
        #Get the quaternions of the local rotations given by omega. Each column of omega is Wx, Wy, Wz in degrees
        q <-  list(getRotationQuaternion(omega[1,1] , 1 , 0 , 0) * getRotationQuaternion(omega[1,2], 0 , 1 , 0) * getRotationQuaternion(omega[1,3], 0 , 0 , 1))
        for ( n in 2:nrow(omega))
        {
                q[[n]] <- getRotationQuaternion(omega[n,1] , 1 , 0 , 0) * getRotationQuaternion(omega[n,2], 0 , 1 , 0) * getRotationQuaternion(omega[n,3], 0 , 0 , 1)
        }
        return(q)
}

getRotationQuaternion <- function(alpha,a,b,c)
{
        # Needs onion package
        # Get the rotation quaternion given an axis of rotation (a, b, c) and an angle(alpha) in degrees
        
        #Divided by 2 and converted from degrees to radians
        alphaRad <- alpha*pi/360
        
        w <- cos(alphaRad)
        s <- sin(alphaRad)
        x <- a*s
        y <- b*s
        z <- c*s
        
        #Normalization of the pure quaternion
        
        magnitude <- sqrt(w^2 + x^2 + y^2 + z^2)
        w <- w/magnitude
        x <- x/magnitude
        y <- y/magnitude
        z <- z/magnitude
        
        #returning the quaternion
        q <- as.quaternion(matrix(c(w, x, y, z), byrow=FALSE))
        return(q)
}

getLocalBasis <- function(localRotation)
{
        #Get the orientation of the basis given the local rotation quaternion obtained with the gyroscope.
        #The basis is expressed in the global coordinate system
        rotatedBasis <- list(diag(3))
        for (n in 2:length(localRotation))
        {
                rotatedBasis[[n]] <- rotate(rotatedBasis[[n-1]], localRotation[[n-1]] / 200)
        }
        return(rotatedBasis)
}

getRelAcc <- function(dataAccelerometer)
{
        relAcc <- matrix(c(dataAccelerometer$ACC_X, dataAccelerometer$ACC_Y, dataAccelerometer$ACC_Z), ncol=3, byrow=FALSE)
        return(relAcc)
}

getAbsAcceleration <- function(basis, relativeAccel)
{
        #Reads the relative acceleration (accelerometer refference) and the evolution of the axes
        #and transform relative to absolute acceleration (ground refference)
        
        absoluteAccel <- list(matrix(relativeAccel[1,], ncol=3))
        g <- matrix(c(0, 0, 1000), ncol=3)
        for (i in 1:length(relativeAccel[,1])){
                absoluteAccel[[i]] <- relativeAccel[i,] %*% basis[[i]] - g
        }
        return(matrix(unlist(absoluteAccel), ncol=3, byrow = TRUE))
}

getAbsSpeed <- function(absoluteAccel)
{
        #Read the absolute acceleration and integrate it to get the absolute velocity
        absSpeed <- matrix(, ncol=3, nrow=length(absoluteAccel[,1]))
        absSpeed[1,] <- c(0,0,0)
        for (i in 2:length(absoluteAccel[,1])){
                #Acceleration expressed in miliGravity, Sample rate = 200hz
                absSpeed[i,1] <- absSpeed[(i-1),1] + absoluteAccel[(i-1),1] * (9.81 / 1000) / 200 
                absSpeed[i,2] <- absSpeed[(i-1),2] + absoluteAccel[(i-1),2] * (9.81 / 1000) / 200
                absSpeed[i,3] <- absSpeed[(i-1),3] + absoluteAccel[(i-1),3] * (9.81 / 1000) / 200
        }
        return(absSpeed)
}

drawPosition <- function(position)
{
        l <- length(position[,1])
        aspect3d("iso")
        i = diag(3)
        points3d(position[1:l,], color=heat.colors(l)) #Color starts on red
}

drawQuaternion <- function (quaternion) 
{
        #Needs the rgl library. Needs some messa-dev linux packages
        #Draws a quaternion in the direction of the rotating axis and the length of the cos(alpha/2)
        aspect3d("iso")
        i <- matrix(c(1,0,0,0,1,0,0,0,1), ncol=3, byrow=FALSE)
        magnitude <- sqrt( Re(quaternion)^2 + i(quaternion)^2 + j(quaternion)^2 + k(quaternion)^2)
        plot3d(i)
        segments3d( c(0 , i(quaternion)*Re(quaternion)/magnitude), c(0 , j(quaternion)*Re(quaternion)/magnitude), c(0 , k(quaternion)*Re(quaternion)/magnitude), col="red")
}


calibrate <- function(accelerometer){
        accelerometer$ACC_X <- accelerometer$ACC_X - getCalibration(accelerometer$ACC_X)[1]
        accelerometer$ACC_Y <- accelerometer$ACC_Y - getCalibration(accelerometer$ACC_Y)[1]
        accelerometer$ACC_Z <- accelerometer$ACC_Z - (getCalibration(accelerometer$ACC_Z)[1] - 1000)
        accelerometer$GYRO_X <- accelerometer$GYRO_X - getCalibration(accelerometer$GYRO_X)[1]
        accelerometer$GYRO_Y <- accelerometer$GYRO_Y - getCalibration(accelerometer$GYRO_Y)[1]
        accelerometer$GYRO_Z <- accelerometer$GYRO_Z - getCalibration(accelerometer$GYRO_Z)[1]
        return(accelerometer)
}

getCalibration <- function(data)
{
        #Reads the 200 first values (1s) and the last 200 last values and try to calculate the offset.
        calibration <- c(mean(data[1:200]), mean(data[(length(data)-200):(length(data))]))
        return(calibration)
}

drawBasis <- function(p0, basis){
        #open3d()
        #axes3d(xlab="X", ylab="Y", zlab="Z")
        lim = c(-1,1)
#         x = plot3d(c(p0[1], p0[1] + basis[1,1]), c(p0[2], p0[2] + basis[1,2]), c(p0[3], p0[3] + basis[1,3]), type="l")
#         y = plot3d(c(p0[1], p0[1] + basis[2,1]), c(p0[2], p0[2] + basis[2,2]), c(p0[3], p0[3] + basis[2,3]), type="l")
#         z = plot3d(c(p0[1], p0[1] + basis[3,1]), c(p0[2], p0[2] + basis[3,2]), c(p0[3], p0[3] + basis[3,3]), xlim = lim, , ylim = lim , zlim = lim, xlab = "X", ylab="Y", zlab="Z", type="l")
        
#        for(i in 1:length(basis)){
                 segments3d(c(p0[1], p0[1] + basis[1,1]), c(p0[2], p0[2] + basis[1,2]), c(p0[3], p0[3] + basis[1,3]), color=2)
                 segments3d(c(p0[1], p0[1] + basis[2,1]), c(p0[2], p0[2] + basis[2,2]), c(p0[3], p0[3] + basis[2,3]), color=3)
                 segments3d(c(p0[1], p0[1] + basis[3,1]), c(p0[2], p0[2] + basis[3,2]), c(p0[3], p0[3] + basis[3,3]), color=4)
#                 Sys.sleep(0.002)
#                 rgl.pop(type="shapes", id= x)
#                 rgl.pop(type="shapes", id= y)
#                 rgl.pop(type="shapes", id= z)
#         }
}

drawAcceleration <- function(p0, acceleration){
        lim = c(-max(abs(acceleration)), max(abs(acceleration)))
         #x = plot3d(c(0,i[1,1]), c(0,i[1,2]), c(0,i[1,3]), xlim = lim, , ylim = lim , zlim = lim, type="l")
         #y = plot3d(c(0,i[2,1]), c(0,i[2,2]), c(0,i[2,3]), xlim = lim, , ylim = lim , zlim = lim, type="l")
         #z = plot3d(c(0,i[3,1]), c(0,i[3,2]), c(0,i[3,3]), xlim = lim, , ylim = lim , zlim = lim, xlab = "X", ylab="Y", zlab="Z", type="l")
        
        segments3d(c(p0[1], p0[1] + acceleration[1]), c(p0[2], p0[2] + acceleration[2]), c(p0[3], p0[3] + acceleration[3]), color=5)
}

drawAll <- function(data, n){
        open3d()
        i = diag(3)
        omega <- getOmega(data) #Read the data coming from the gyros
        localRotation <- getLocalRotationQuaternion(omega) # Translate the local rotation in degrees per second to quaternions
        localBasis <- getLocalBasis(localRotation) # Create a local basis expressed in the absolute basis. (200Hz)
        relAccel <- getRelAcc(data) # Read data coming from the accelerometers
        absAccel <- getAbsAcceleration(localBasis, relAccel) # Translate the local acceleration to the absolute acceleration
        absAccel = absAccel / 1000
        position = getAbsPosition(data)
        lim = c(-max(abs(absAccel)), max(abs(absAccel)))
        
        plot3d(c(0,i[1,1]), c(0,i[1,2]), c(0,i[1,3]), xlim = lim, , ylim = lim , zlim = lim, type="l")
        plot3d(c(0,i[2,1]), c(0,i[2,2]), c(0,i[2,3]), xlim = lim, , ylim = lim , zlim = lim, xlab = "X", ylab="Y", zlab="Z", type="l")
        #plot3d(c(0,i[3,1]), c(0,i[3,2]), c(0,i[3,3]), xlim = lim, , ylim = lim , zlim = lim, xlab = "X", ylab="Y", zlab="Z", type="l")
        drawPosition(position)
       
        #steps	
        p = length(position[,1]) %/% n
        
        #drawBasis(position[1,], localBasis[[1]])
        drawAcceleration(position[1,],absAccel[1,])
        for (i in 1:n){
                drawBasis(position[i*p,], localBasis[[i*p]] / 10)
                drawAcceleration(position[i*p,],absAccel[i*p,])  
                #drawAcceleration(position[i*p,],relAccel[i*p,])    
        }
}

correctSignal <- function(signal, s){
        #see Giansanti, D.; Maccioni, G.; Macellari, V. Guidelines for Calibration and Drift Compensation of a
        #Wearable Device with Rate-Gyroscopes and Accelerometers. In Proceedings of 29th Annual
        #International Conference of the IEEE Engineering in Medicine and Biology Society, Shanghai,
        #China, 1–4 September 2007; pp. 2342–2345.
        
        # s is a matrix with the six measures of the three axis. Measures in columns, axis in rows.
        # l is the matrix with the supposed values.
        
        g = 1000
        l = matrix(c(-g, 0, 0, g, 0, 0, 0, -g, 0, 0, g, 0, 0, 0, -g, 0, 0, g), ncol=6)
        c = l %*% t(s) %*% (solve(s %*% t(s)))
        return(signal %*% c)
}

######### Not used#################
# getAbsoluteRotationQuaternion <- function(localRotation)
# {
#   #Given the local rotation quaternions obtained with the Gyroscope returns the total global rotation
#   
#   q <- localRotation[1]
#   for (n in 2:length(localRotation))
#   {
#     q <-  c(q, localRotation[n] * q[n-1])
#     #print(q[n])
#   }
#   return(q)
# }

drawRelative2D <- function(data, toFile)
{
        data <- data *0.00981 #miliG to ms^2
        
	if(toFile) {
		png("drawRelative2D.png",width=600, height=400)
	}

        plot(data$ACC_X, ylim=c(-11, 11), type ="l", col="red",
             xlab="Temps (ms)", ylab="Acceleració (m/s^2)",
             main="Acceleracions 3D relatives al llarg del temps")
        legend("topright", c("X","Y","Z"), col=c("red","green","blue"),lty=1,lwd=1)
        lines(data$ACC_Y, col="green")
        lines(data$ACC_Z, col="blue")
        abline(v=c(200,300,400,600), lty=2)
        mtext("A",side=3, at=100)
        mtext("B",side=3, at=250)
        mtext("C",side=3, at=350)
        mtext("D",side=3, at=500)
        mtext("E",side=3, at=700)
	
	if(toFile) {
        	dev.off()
	}
}

library("rgl") #installing on Debian needs packages: mesa-common-dev, libglu1-mesa-dev
library("onion")

#setwd("ownCloud/Xavier/Chronojump/Accelerometre/")
data = read.csv("captures/0.981m.csv", sep = ",", dec = ",")

drawRelative2D(data, TRUE) #toFile = TRUE
drawAll(data, 30) #100 nicer
getwd()
