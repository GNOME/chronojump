library(GDD)
GDD(file="/var/www/web/server/images/evaluators.png", type="png", w=670, h=670)
library(RSQLite)
drv = dbDriver("SQLite")
file = "/root/.local/share/Chronojump/database/chronojump_server.db"
con = dbConnect(drv, file)

persons <- dbGetQuery(con, "SELECT COUNT(DISTINCT(person77.uniqueID)) AS conta, SEvaluator.name AS names FROM person77, SEvaluator, session, personSession77 WHERE person77.uniqueID=personSession77.personID AND session.uniqueID=personSession77.sessionID AND session.evaluatorID=Sevaluator.uniqueID GROUP BY SEvaluator.name ORDER BY SEvaluator.name DESC;")

jumps <- dbGetQuery(con, "SELECT COUNT(jump.uniqueID) AS conta, SEvaluator.name AS names FROM jump, SEvaluator, session WHERE jump.sessionID=session.uniqueID AND session.evaluatorID=Sevaluator.uniqueID GROUP BY SEvaluator.name ORDER BY SEvaluator.name;")

jumpsRj <- dbGetQuery(con, "SELECT COUNT(jumpRj.uniqueID) AS conta, SEvaluator.name AS names FROM jumpRj, SEvaluator, session WHERE jumpRj.sessionID=session.uniqueID AND session.evaluatorID=Sevaluator.uniqueID GROUP BY SEvaluator.name ORDER BY conta DESC;")

#runs <- dbGetQuery(con, "SELECT COUNT(run.uniqueID) AS conta, SEvaluator.name AS names FROM run, SEvaluator, session WHERE run.sessionID=session.uniqueID AND session.evaluatorID=Sevaluator.uniqueID GROUP BY SEvaluator.name ORDER BY SEvaluator.name;")

runsInterval <- dbGetQuery(con, "SELECT COUNT(runInterval.uniqueID) AS conta, SEvaluator.name AS names FROM runInterval, SEvaluator, session WHERE runInterval.sessionID=session.uniqueID AND session.evaluatorID=Sevaluator.uniqueID GROUP BY SEvaluator.name ORDER BY SEvaluator.name;")

reactionTimes <- dbGetQuery(con, "SELECT COUNT(reactiontime.uniqueID) AS conta, SEvaluator.name AS names FROM reactiontime, SEvaluator, session WHERE reactiontime.sessionID=session.uniqueID AND session.evaluatorID=Sevaluator.uniqueID GROUP BY SEvaluator.name ORDER BY SEvaluator.name;")

pulses <- dbGetQuery(con, "SELECT COUNT(pulse.uniqueID) AS conta, SEvaluator.name AS names FROM pulse, SEvaluator, session WHERE pulse.sessionID=session.uniqueID AND session.evaluatorID=Sevaluator.uniqueID GROUP BY SEvaluator.name ORDER BY SEvaluator.name;")

#multichronopic <- dbGetQuery(con, "SELECT COUNT(multichronopic.uniqueID) AS conta, SEvaluator.name AS names FROM multichronopic, SEvaluator, session WHERE multichronopic.sessionID=session.uniqueID AND session.evaluatorID=Sevaluator.uniqueID GROUP BY SEvaluator.name ORDER BY SEvaluator.name;")

par(oma=c(1,7,1,1))

#a <- merge(persons, jumps, by="names", all.x=T)
#colnames(a)=c("names", "persons", "jumps")
#a <- merge(a, jumpsRj, by="names", all.x=T)
#colnames(a)=c("names", "persons", "jumps", "jumps_reactive")
#a <- merge(a, runs, by="names", all.x=T)
#colnames(a)=c("names", "persons", "jumps", "jumps_reactive", "runs")
#a <- merge(a, runsInterval, by="names", all.x=T)
#colnames(a)=c("names", "persons", "jumps", "jumps_reactive", "runs", "runs_intervallic")
#a <- merge(a, reactionTimes, by="names", all.x=T)
#colnames(a)=c("names", "persons", "jumps", "jumps_reactive", "runs", "runs_intervallic", "reaction_times")
#a <- merge(a, pulses, by="names", all.x=T)
#colnames(a)=c("names", "persons", "jumps", "jumps_reactive", "runs", "runs_intervallic", "reaction_times", "pulses")
#a <- merge(a, multichronopic, by="names", all.x=T)
#colnames(a)=c("names", "persons", "jumps", "jumps_reactive", "runs", "runs_intervallic", "reaction_times", "pulses", "multichronopic")

a <- merge(persons, jumps, by="names", all.x=T)
colnames(a)=c("names", "persons", "jumps")
a <- merge(a, jumpsRj, by="names", all.x=T)
colnames(a)=c("names", "persons", "jumps", "jumps_reactive")
a <- merge(a, runsInterval, by="names", all.x=T)
colnames(a)=c("names", "persons", "jumps", "jumps_reactive", "runs_intervallic")
a <- merge(a, reactionTimes, by="names", all.x=T)
colnames(a)=c("names", "persons", "jumps", "jumps_reactive", "runs_intervallic", "reaction_times")
a <- merge(a, pulses, by="names", all.x=T)
colnames(a)=c("names", "persons", "jumps", "jumps_reactive", "runs_intervallic", "reaction_times", "pulses")

#prepare sort
a <- replace(a,is.na(a),0)
#fix columns with all "" because there's no data
a$runs <- as.numeric(a$runs)
a$multichronopic <- as.numeric(a$multichronopic)
#fix duplicates:
a[(a$names == "Josep M. Padullés"),2:9] = a[(a$names == "Josep M. Padullés"),2:9] + a[(a$names == "Josep M Padullés"),2:9]
a <- subset(a,a$names != "Josep M Padullés")
a[(a$names == "Jeffrey Pagaduan"),2:9] = a[(a$names == "Jeffrey Pagaduan"),2:9] + a[(a$names == "Jeffrey C. Pagaduan"),2:9]
a <- subset(a,a$names != "Jeffrey C. Pagaduan")
#sort
#a <- a[order(a$persons + a$jumps + a$jumps_reactive + a$runs + a$runs_intervallic + a$reaction_times + a$pulses + a$multichronopic),]
a <- a[order(a$persons + a$jumps + a$jumps_reactive + a$runs_intervallic + a$reaction_times + a$pulses),]

#prepare graph
#b=cbind(a$persons, a$jumps , a$jumps_reactive, a$runs, a$runs_intervallic, a$reaction_times, a$pulses, a$multichronopic)
#colnames(b)=c("persons", "jumps", "jumps_reactive", "runs", "runs_intervallic", "reaction_times", "pulses", "multichronopic")
b=cbind(a$persons, a$jumps , a$jumps_reactive, a$runs_intervallic, a$reaction_times, a$pulses)
colnames(b)=c("persons", "jumps", "jumps_reactive", "runs_intervallic", "reaction_times", "pulses")
rownames(b)=a$names

#graph
cex=.8
#change colors to 8 when add runs and multichronopic
colors=6
barplot(t(b), horiz=T, las=2, col=topo.colors(colors), cex.names=cex)
legend("bottomright", colnames(b), pch=15, col=topo.colors(colors))

title(main="Data uploaded by evaluator",
  sub=paste(Sys.Date(),"(YYYY-MM-DD)"), cex.sub = 0.75, font.sub = 3, col.sub = "red")

dev.off()
