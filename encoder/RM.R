 
RMIndirect <- function(Q, nrep, n){
#Q = load in Kg
#nrep = number of maximum repetitions
#n = the number of nRM you want to know

        rm = matrix(rep(c(0,0,0,0,0,0,0), n), ncol=7)
        colnames(rm) = c("Brzycki", "Epley", "Lander", "Lombardi", "Mayhew", "Oconner", "Wathan")
        rm = as.data.frame(rm)
        rm[1,1] = Q * (36 / (37 - nrep))                                    #Brzycki
        rm[1,2] = Q * (1 + 0.0333  *  nrep)                                 #Epley
        rm[1,3] = (100 * Q) / (101.3 - 2.67123 * nrep)                      #Lander   
        rm[1,4] = Q * nrep^0.1                                              #Lombardi
        rm[1,5] = (100 * Q) / (52.2 + (41.9 * exp(-0.055 * nrep)))          #Mayhew
        rm[1,6] = Q * (1 + 0.025 * nrep)                                    #O'Conner
        rm[1,7] = (100 * Q) / (48.8 + (53.8 * exp(-0.075 * nrep)))          #Wathan
        if(n==1) return(rm)
        for(i in 2:n){
                rm[i,1] = rm[1,1] * (37 - i) / 36                           #Brzycki
                rm[i,2] = rm[1,2] / (1 + (i / 30))                          #Epley
                rm[i,3] = rm[1,3] * (101.3 - 2.67123 * i) / 100             #Lander
                rm[i,4] = rm[1,4] / (i ^ (1 / 10))                          #Lombardi
                rm[i,5] = rm[1,5] * (52.2 + (41.9 * exp(-1 * (i * 0.055)))) / 100       #Mayhew
                rm[i,6] = rm[1,6] / (1 + i * 0.025)                         #O'Conner
                rm[i,7] = rm[1,7]* (48.8 + (53.8 * exp(-1 * (i * 0.075)))) / 100        #Wathan
                }
        return(rm)
}
