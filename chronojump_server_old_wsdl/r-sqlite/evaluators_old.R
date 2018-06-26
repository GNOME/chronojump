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
par(new=FALSE, oma=c(1,1,4,1))
nf <- layout(matrix(c(1,2,3), 3, 1, byrow=TRUE), heights=c(11,10,7), respect=FALSE)
#par(mfcol=c(3,1))

personsOrdered = persons[order(persons$conta),]
jumpsOrdered = jumps[order(jumps$conta),]
jumpsRjOrdered = jumpsRj[order(jumpsRj$conta),]

cex=.7
dotchart(personsOrdered$conta, labels=personsOrdered$names, main=paste("Persons"," [",sum(persons$conta),"]"), cex=cex)
dotchart(jumpsOrdered$conta, labels=jumpsOrdered$names, main=paste("Jumps (simple)"," [",sum(jumps$conta),"]"), cex=cex)
dotchart(jumpsRjOrdered$conta, labels=jumpsRjOrdered$names, main=paste("Jumps (reactive)"," [",sum(jumpsRj$conta),"]"), cex=cex)


par(def.par)#- reset to default
par(new=TRUE)
plot(-1,type="n",axes=F,xlab='',ylab='')
title(main="Data uploaded by evaluator",
  sub=paste(Sys.Date(),"(YYYY-MM-DD)"), cex.sub = 0.75, font.sub = 3, col.sub = "red")

dev.off()
