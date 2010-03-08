library(GDD)
GDD(file="/var/www/web/server/images/evaluators.png", type="png", w=670, h=670)
library(RSQLite)
drv = dbDriver("SQLite")
file = "/root/.local/share/Chronojump/database/chronojump_server.db"
con = dbConnect(drv, file)

jumps <- dbGetQuery(con, "SELECT COUNT(jump.uniqueID) AS conta, SEvaluator.name AS names FROM jump, SEvaluator, session WHERE jump.sessionID=session.uniqueID AND session.evaluatorID=Sevaluator.uniqueID GROUP BY SEvaluator.name ORDER BY SEvaluator.name;")

persons <- dbGetQuery(con, "SELECT COUNT(DISTINCT(person77.uniqueID)) AS conta, SEvaluator.name AS names FROM person77, SEvaluator, session, personSession77 WHERE person77.uniqueID=personSession77.personID AND session.uniqueID=personSession77.sessionID AND session.evaluatorID=Sevaluator.uniqueID GROUP BY SEvaluator.name ORDER BY SEvaluator.name;")

colors=c(topo.colors(8)[3],topo.colors(8)[6])
o.j.d <- order(jumps$conta, decreasing=T)

barplot(rbind(jumps$conta[o.j.d], persons$conta[o.j.d]), beside=T, legend=c("Jumps","Persons"), names=jumps$names[o.j.d], col=colors)

title(main="Jumps and persons count by evaluator",
  sub=paste(Sys.Date(),"(YYYY-MM-DD)"), cex.sub = 0.75, font.sub = 3, col.sub = "red")

dev.off()
