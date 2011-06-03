library(GDD)
GDD(file="/var/www/web/server/images/evaluators.png", type="png", w=670, h=670)
library(RSQLite)
drv = dbDriver("SQLite")
file = "/root/.local/share/Chronojump/database/chronojump_server.db"
con = dbConnect(drv, file)

jumps <- dbGetQuery(con, "SELECT COUNT(jump.uniqueID) AS conta, SEvaluator.name AS names FROM jump, SEvaluator, session WHERE jump.sessionID=session.uniqueID AND session.evaluatorID=Sevaluator.uniqueID GROUP BY SEvaluator.name ORDER BY SEvaluator.name;")

jumpsRj <- dbGetQuery(con, "SELECT COUNT(jumpRj.uniqueID) AS conta, SEvaluator.name AS names FROM jumpRj, SEvaluator, session WHERE jumpRj.sessionID=session.uniqueID AND session.evaluatorID=Sevaluator.uniqueID GROUP BY SEvaluator.name ORDER BY conta DESC;")

persons <- dbGetQuery(con, "SELECT COUNT(DISTINCT(person77.uniqueID)) AS conta, SEvaluator.name AS names FROM person77, SEvaluator, session, personSession77 WHERE person77.uniqueID=personSession77.personID AND session.uniqueID=personSession77.sessionID AND session.evaluatorID=Sevaluator.uniqueID GROUP BY SEvaluator.name ORDER BY SEvaluator.name;")

def.par <- par(no.readonly = TRUE) # save default, for resetting...
par(new=FALSE, oma=c(1,1,5,1))
#nf <- layout(matrix(c(1,1,2,3), 2, 2, byrow=TRUE), respect=TRUE)
par(mfcol=c(3,1))

persons$names<-factor(persons$names)
jumps$names<-factor(jumps$names)
jumpsRj$names<-factor(jumpsRj$names)

cex=.7
dotchart(persons$conta[order (persons$conta)], labels=levels(persons$names)[order (persons$conta)], main="Persons", cex=cex)
dotchart(jumps$conta[order (jumps$conta)], labels=levels(jumps$names)[order (jumps$conta)], main="Jumps (simple)", cex=cex)
dotchart(jumpsRj$conta[order (jumpsRj$conta)], labels=levels(jumpsRj$names)[order (jumpsRj$conta)], main="Jumps (reactive)", cex=cex)


par(def.par)#- reset to default
par(new=TRUE)
plot(-1,type="n",axes=F,xlab='',ylab='')
title(main="Data uploaded by evaluator",
  sub=paste(Sys.Date(),"(YYYY-MM-DD)"), cex.sub = 0.75, font.sub = 3, col.sub = "red")

dev.off()
