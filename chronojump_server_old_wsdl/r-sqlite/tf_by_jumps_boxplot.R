#library(GDD)
#file = "/root/.local/share/Chronojump/database/chronojump_server.db"
#GDD(file="/var/www/web/server/images/tf_by_jumps_boxplot.png", 
#  width = 670, height= 670, ps = 12, type="png")
png(file="tf_by_jumps_boxplot.png", w=800, h=800) #local PNG
library(RSQLite)
drv <- dbDriver("SQLite")
file = "~/.local/share/Chronojump/database/chronojump_server_2013-07-05.db"
con <- dbConnect(drv, file)

jumps <- dbGetQuery(con, "SELECT person77.sex, jump.* FROM person77, jump WHERE person77.uniqueID == jump.personID")
jumpsM <- subset(jumps, jumps$sex=="M")
jumpsF <- subset(jumps, jumps$sex=="F")

par(mfrow=c(2,1))

ntypes <- length(levels(as.factor (jumpsM$type)))
bp=boxplot(jumpsM$tv ~ jumpsM$type, las=2, col=terrain.colors(ntypes),cex.axis=.7)
mtext("n=", at=0, col = "grey20", cex=0.8)
mtext(format(bp$n), at=1:ntypes, xpd = TRUE, col = "grey20", cex=0.8)
title(main="Flight times by jumps in males")

ntypes <- length(levels(as.factor (jumpsF$type)))
bp=boxplot(jumpsF$tv ~ jumpsF$type, las=2, col=topo.colors(ntypes), cex.axis=.7)
mtext("n=", at=0, col = "grey20", cex=0.8)
mtext(format(bp$n), at=1:ntypes, xpd = TRUE, col = "grey20", cex=0.8)
title(main="Flight times by jumps in females",
  sub=paste(Sys.Date(),"(YYYY-MM-DD)"), cex.sub = 0.75, font.sub = 3, col.sub = "red")

par(mfrow=c(1,1))

dev.off()

