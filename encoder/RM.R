RMIndirect <- function(Q, nrep){
        rm = c(0,0,0,0,0,0,0)
        rm[1] = Q * (36 / (37 - nrep))                                    #brzycki
        rm[2] = Q * (1 + 0.0333  *  nrep)                                 #Epley
        rm[3] = (100 * Q) / (101.3 - 2.67123 * nrep)                      #lander   
        rm[4] = Q * nrep^0.1                                              #lombardi
        rm[5] = (100 * Q) / (52.2 + (41.9 * exp(-0.055 * nrep)))          #mayhew
        rm[6] = Q * (1 + 0.025 * nrep)                                    #oconner
        rm[7] = (100 * Q) / (48.8 + (53.8 * exp(-0.075 * nrep)))          #wathan
        return(rm)
}