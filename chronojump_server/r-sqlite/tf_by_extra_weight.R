#library(GDD)
#GDD(file="/var/www/web/server/images/tf_by_extra_weight.png", type="png", w=670, h=670)
png(file="tf_by_extra_weight.png", w=800, h=800) #local PNG

library(RSQLite)
drv = dbDriver("SQLite")
file = "~/.local/share/Chronojump/database/chronojump_server_2013-07-05.db"
con = dbConnect(drv, file)

def.par <- par(no.readonly = TRUE) # save default, for resetting...

SJlM <- dbGetQuery(con, "SELECT jump.tv AS TF, jump.weight AS Weight FROM jump, person77 WHERE (jump.type='SJ' OR jump.type='SJl')  AND person77.sex='M' AND jump.personID = person77.UniqueID")
SJlF <- dbGetQuery(con, "SELECT jump.tv AS TF, jump.weight AS Weight FROM jump, person77 WHERE (jump.type='SJ' OR jump.type='SJl') AND person77.sex='F' AND jump.personID = person77.UniqueID")
CMJlM <- dbGetQuery(con, "SELECT jump.tv AS TF, jump.weight AS Weight FROM jump, person77 WHERE (jump.type='CMJ' OR jump.type='CMJl') AND person77.sex='M' AND jump.personID = person77.UniqueID")
CMJlF <- dbGetQuery(con, "SELECT jump.tv AS TF, jump.weight AS Weight FROM jump, person77 WHERE (jump.type='CMJ' OR jump.type='CMJl') AND person77.sex='F' AND jump.personID = person77.UniqueID")

par(new=FALSE, oma=c(1,1,5,1))
par(mfcol=c(2,2))

a=SJlM$TF
b=as.double(SJlM$Weight)
plot(a~b, main="SJ Man", xlab="Extra weight (%)", ylab="Flight time (s)")
abline(lm(a~b),col='red')
stat=paste("Pearson=",round(cor(a,b),3)," R²=",round(cor(a,b)^2,3))
mtext(stat, cex=.7)

a=CMJlM$TF
b=as.double(CMJlM$Weight)
plot(a~b, main="CMJ Man", xlab="Extra weight (%)", ylab="Flight time (s)")
abline(lm(a~b),col='red')
stat=paste("Pearson=",round(cor(a,b),3)," R²=",round(cor(a,b)^2,3))
mtext(stat, cex=.7)

a=SJlF$TF
b=as.double(SJlF$Weight)
plot(a~b, main="SJ Woman", xlab="Extra weight (%)", ylab="Flight time (s)")
abline(lm(a~b),col='red')
stat=paste("Pearson=",round(cor(a,b),3)," R²=",round(cor(a,b)^2,3))
mtext(stat, cex=.7)

a=CMJlF$TF
b=as.double(CMJlF$Weight)
if(length(a)>0) {
        plot(a~b, main="CMJ Woman", xlab="Extra weight (%)", ylab="Flight time (s)")
        if(max(b)>0) {
                abline(lm(a~b),col='red')
                stat=paste("Pearson=",round(cor(a,b),3)," R²=",round(cor(a,b)^2,3))
                mtext(stat, cex=.7)
        }
}

par(def.par)#- reset to default

par(new=TRUE)
plot(-1,type="n",axes=F,xlab='',ylab='')
title(main="Correlations between extra weight in % and flight time",
  sub=paste(Sys.Date(),"(YYYY-MM-DD)"), cex.sub = 0.75, font.sub = 3, col.sub = "red")

dev.off()

