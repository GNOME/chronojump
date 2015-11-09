# 
#  This file is part of ChronoJump
# 
#  ChronoJump is free software; you can redistribute it and/or modify
#   it under the terms of the GNU General Public License as published by
#    the Free Software Foundation; either version 2 of the License, or   
#     (at your option) any later version.
#     
#  ChronoJump is distributed in the hope that it will be useful,
#   but WITHOUT ANY WARRANTY; without even the implied warranty of
#    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
#     GNU General Public License for more details.
# 
#  You should have received a copy of the GNU General Public License
#   along with this program; if not, write to the Free Software
#    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
# 
#   Copyright (C) 2004-2015  	Xavier de Blas <xaviblas@gmail.com> 
#   Copyright (C) 2015   	Xavier Padullés <x.padulles@gmail.com>
# 
 
RMIndirect <- function(Q, nrep, nRM) {
#Q = load in Kg
#nrep = number of maximum repetitions
#n = the number of nRM you want to know

        rm = matrix(rep(c(0,0,0,0,0,0,0,0), nRM), ncol=8)
        colnames(rm) = c("Brzycki", "Epley", "Lander", "Lombardi", "Mayhew", "Oconner", "Wathan", "AVG")
        rm = as.data.frame(rm)
        rm[1,1] = Q * (36 / (37 - nrep))                                    #Brzycki
        rm[1,2] = Q * (1 + 0.0333  *  nrep)                                 #Epley
        rm[1,3] = (100 * Q) / (101.3 - 2.67123 * nrep)                      #Lander   
        rm[1,4] = Q * nrep^0.1                                              #Lombardi
        rm[1,5] = (100 * Q) / (52.2 + (41.9 * exp(-0.055 * nrep)))          #Mayhew
        rm[1,6] = Q * (1 + 0.025 * nrep)                                    #O'Conner
        rm[1,7] = (100 * Q) / (48.8 + (53.8 * exp(-0.075 * nrep)))          #Wathan
	rm[1,8] = mean(as.numeric(rm[1,1:7]))

	if(nRM < 2) return(rm)
        for(i in 2:nRM) {
                rm[i,1] = rm[1,1] * (37 - i) / 36                           #Brzycki
                rm[i,2] = rm[1,2] / (1 + (i / 30))                          #Epley
                rm[i,3] = rm[1,3] * (101.3 - 2.67123 * i) / 100             #Lander
                rm[i,4] = rm[1,4] / (i ^ (1 / 10))                          #Lombardi
                rm[i,5] = rm[1,5] * (52.2 + (41.9 * exp(-1 * (i * 0.055)))) / 100       #Mayhew
                rm[i,6] = rm[1,6] / (1 + i * 0.025)                         #O'Conner
                rm[i,7] = rm[1,7]* (48.8 + (53.8 * exp(-1 * (i * 0.075)))) / 100        #Wathan
		rm[i,8] = mean(as.numeric(rm[i,1:7]))
                }
        return(rm)
}

plotRMIndirect <- function (RMIMatrix) 
{
	nrep = length(RMIMatrix[,1])

	ntests = length(RMIMatrix[1,]) -1 #-1 because we don't count the AVG
	uniqueColors=rainbow(ntests)

	#a) create an empty plot; b) create grid; c) draw all points except AVG; d) draw AVG line

	plot(1, xlim=c(1,nrep),ylim=c(min(RMIMatrix),max(RMIMatrix)), type="n",
	     xlab="Repetitions", ylab="Mass (Kg)")					#a)

	abline(h=seq(0,max(RMIMatrix),by=5), lty=2, col="gray")				#b)

	for(i in 1:ntests)
		lines(RMIMatrix[,i], type="p", pch=19, col=uniqueColors[i])		#c)

	lines(RMIMatrix$AVG, type="l", lwd=2)						#d)

	#AVGs on top. Note ntests is the AVG column
	for(i in 1:nrep)
		mtext(round(RMIMatrix[i,(ntests+1)],1), side=3, at=i, adj=0.5, cex=.8)
	mtext("AVG", side=3, at=0, adj=.5, cex=.8)

	legend("topright", legend=names(RMIMatrix), col=c(uniqueColors,"Black"), lwd=1, 
	       lty=c(rep(0,ntests),1), pch=c(rep(19,ntests),NA), cex=.8, bg="White") #legend
}

#Example
plotRMIndirect(RMIndirect(80,3,10))

