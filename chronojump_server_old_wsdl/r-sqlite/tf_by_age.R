#library(GDD)
#GDD(file="/var/www/web/server/images/tf_by_age.png", type="png", w=670, h=670)
png(file="tf_by_age.png", w=800, h=800) #local PNG

library(RSQLite)
drv = dbDriver("SQLite")
file = "~/.local/share/Chronojump/database/chronojump_server_2013-07-05.db"
con = dbConnect(drv, file)

def.par <- par(no.readonly = TRUE) # save default, for resetting...


dades <- dbGetQuery(con, "SELECT jump.tv AS TF, (strftime('%Y', session.date) - strftime('%Y', person77.dateborn)) - (strftime('%m-%d', session.date) < strftime('%m-%d', person77.dateborn)) AS years, jump.type AS type, person77.sex AS sex FROM jump, person77, personSession77, session WHERE jump.personID = person77.UniqueID AND personSession77.personID = person77.uniqueID AND personSession77.sessionID = session.UniqueID AND years>1")
sjM <- subset(dades, dades$type=='SJ' & dades$sex == 'M')
sjF <- subset(dades, dades$type=='SJ' & dades$sex == 'F')
cmjM <- subset(dades, dades$type=='CMJ' & dades$sex == 'M')
cmjF <- subset(dades, dades$type=='CMJ' & dades$sex == 'F')


par(new=FALSE, oma=c(1,1,5,1))
par(mfcol=c(2,2))


a=sjM$TF
b=sjM$years
plot(a~b, main="SJ Man", xlab="Age (years)", ylab="Flight time (s)")
abline(lm(a~b),col='red')
stat=paste("Pearson=",round(cor(a,b),3)," R²=",round(cor(a,b)^2,3))
mtext(stat, cex=.7)

a=sjF$TF
b=sjF$years
plot(a~b, main="SJ Woman", xlab="Age (years)", ylab="Flight time (s)")
abline(lm(a~b),col='red')
stat=paste("Pearson=",round(cor(a,b),3)," R²=",round(cor(a,b)^2,3))
mtext(stat, cex=.7)

a=cmjM$TF
b=cmjM$years
plot(a~b, main="CMJ Man", xlab="Age (years)", ylab="Flight time (s)")
abline(lm(a~b),col='red')
stat=paste("Pearson=",round(cor(a,b),3)," R²=",round(cor(a,b)^2,3))
mtext(stat, cex=.7)

a=cmjF$TF
b=cmjF$years
plot(a~b, main="CMJ Woman", xlab="Age (years)", ylab="Flight time (s)")
abline(lm(a~b),col='red')
stat=paste("Pearson=",round(cor(a,b),3)," R²=",round(cor(a,b)^2,3))
mtext(stat, cex=.7)


par(def.par)#- reset to default

par(new=TRUE)
plot(-1,type="n",axes=F,xlab='',ylab='')
title(main="Correlations between age and flight time",
  sub=paste(Sys.Date(),"(YYYY-MM-DD)"), cex.sub = 0.75, font.sub = 3, col.sub = "red")

dev.off()

