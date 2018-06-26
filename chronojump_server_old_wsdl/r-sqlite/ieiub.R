#library(GDD)
#GDD(file="/var/www/web/server/images/ieiub.png", type="png", w=670, h=670)
png(file="ieiub.png", w=800, h=800) #local PNG

library(RSQLite)
drv = dbDriver("SQLite")
file = "~/.local/share/Chronojump/database/chronojump_server_2013-07-05.db"
con = dbConnect(drv, file)

def.par <- par(no.readonly = TRUE) # save default, for resetting...

IE <- dbGetQuery(con, "SELECT person77.name, person77.sex AS sex, personSession77.practice AS level,  ( AVG(j1.tv) - AVG(j2.tv) )*100/(AVG(j2.tv)*1.0) AS myIndex, AVG(j1.tv), AVG(j2.tv) FROM jump AS j1, jump AS j2, person77, personSession77 WHERE j1.sessionID == j2.sessionID AND j1.type == 'CMJ'  AND j2.type == 'SJ'  AND j1.personID == person77.uniqueID  AND j2.personID == person77.uniqueID AND person77.UniqueID == personSession77.PersonID GROUP BY j1.personID, j1.sessionID ORDER BY  myIndex DESC")
IUB <- dbGetQuery(con, "SELECT person77.name, person77.sex AS sex, personSession77.practice AS level,  ( AVG(j1.tv) - AVG(j2.tv) )*100/(AVG(j2.tv)*1.0) AS myIndex, AVG(j1.tv), AVG(j2.tv) FROM jump AS j1, jump AS j2, person77, personSession77 WHERE j1.sessionID == j2.sessionID AND j1.type == 'ABK'  AND j2.type == 'CMJ'  AND j1.personID == person77.uniqueID  AND j2.personID == person77.uniqueID AND person77.UniqueID == personSession77.PersonID GROUP BY j1.personID, j1.sessionID ORDER BY  myIndex DESC")
IEm <- subset (IE, IE$sex == 'M')
IEf <- subset (IE, IE$sex == 'F')
IUBm <- subset (IUB, IUB$sex == 'M')
IUBf <- subset (IUB, IUB$sex == 'F')

par(new=FALSE, oma=c(1,1,5,1))
par(mfcol=c(2,2))
levels=c("sed.", "regul.", "compet.", "elite")

bp = boxplot(IEm$myIndex~IEm$level, main="IE Man by practice level",  axes=F)
text(1:4, -5, paste("n=",format(bp$n),sep=""), xpd = TRUE)
mtext("IE=100*(CMJ-SJ)/SJ", cex=.7)
axis(1, at = 1:4, labels = levels)
axis(2)
box()
abline(h=0)

bp = boxplot(IUBm$myIndex~IUBm$level, main="IUB Man by practice level",  axes=F)
text(1:4, -5, paste("n=",format(bp$n),sep=""), xpd = TRUE)
mtext("IUB=100*(ABK-CMJ)/CMJ",cex=.7)
axis(1, at = 1:4, labels = levels)
axis(2)
box()
abline(h=0)

bp = boxplot(IEf$myIndex~IEf$level, main="IE Woman by practice level", axes=F)
text(1:4, -5, paste("n=",format(bp$n),sep=""), xpd = TRUE)
mtext("IE=100*(CMJ-SJ)/SJ",cex=.7)
axis(1, at = 1:4, labels = levels)
axis(2)
box()
abline(h=0)

bp = boxplot(IUBf$myIndex~IUBf$level, main="IUB Woman by practice level", axes=F)
text(1:4, -5, paste("n=",format(bp$n),sep=""), xpd = TRUE)
mtext("IUB=100*(ABK-CMJ)/CMJ",cex=.7)
axis(1, at = 1:4, labels = levels)
axis(2)
box()
abline(h=0)


par(def.par)#- reset to default

par(new=TRUE)
plot(-1,type="n",axes=F,xlab='',ylab='')
title(main="Elasticity Index and Using Arms Index",
  sub=paste("Values are AVG of both jumps of each person on every session. Date of graph: ",Sys.Date(),"(YYYY-MM-DD)"), cex.sub = 0.75, font.sub = 3, col.sub = "red")

dev.off()
