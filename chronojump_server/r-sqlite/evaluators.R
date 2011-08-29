#server
#library(GDD)
#GDD(file="/var/www/web/server/images/evaluators.png", type="png", w=670, h=670)
#file = "/root/.local/share/Chronojump/database/chronojump_server.db"

#local
pdf(file="evaluators.pdf", width=7, height=7)
file = "/home/xavier/.local/share/Chronojump/database/chronojump_server_2011-07-26.db"

library(RSQLite)
drv = dbDriver("SQLite")
con = dbConnect(drv, file)

persons <- dbGetQuery(con, "SELECT COUNT(DISTINCT(person77.uniqueID)) AS conta, SEvaluator.name AS names FROM person77, SEvaluator, session, personSession77 WHERE person77.uniqueID=personSession77.personID AND session.uniqueID=personSession77.sessionID AND session.evaluatorID=Sevaluator.uniqueID GROUP BY SEvaluator.name ORDER BY SEvaluator.name DESC;")

jumps <- dbGetQuery(con, "SELECT COUNT(jump.uniqueID) AS conta, SEvaluator.name AS names FROM jump, SEvaluator, session WHERE jump.sessionID=session.uniqueID AND session.evaluatorID=Sevaluator.uniqueID GROUP BY SEvaluator.name ORDER BY SEvaluator.name;")

jumpsRj <- dbGetQuery(con, "SELECT COUNT(jumpRj.uniqueID) AS conta, SEvaluator.name AS names FROM jumpRj, SEvaluator, session WHERE jumpRj.sessionID=session.uniqueID AND session.evaluatorID=Sevaluator.uniqueID GROUP BY SEvaluator.name ORDER BY conta DESC;")

#runs <- dbGetQuery(con, "SELECT COUNT(run.uniqueID) AS conta, SEvaluator.name AS names FROM run, SEvaluator, session WHERE run.sessionID=session.uniqueID AND session.evaluatorID=Sevaluator.uniqueID GROUP BY SEvaluator.name ORDER BY SEvaluator.name;")

runsInterval <- dbGetQuery(con, "SELECT COUNT(runInterval.uniqueID) AS conta, SEvaluator.name AS names FROM runInterval, SEvaluator, session WHERE runInterval.sessionID=session.uniqueID AND session.evaluatorID=Sevaluator.uniqueID GROUP BY SEvaluator.name ORDER BY SEvaluator.name;")

reactionTimes <- dbGetQuery(con, "SELECT COUNT(reactiontime.uniqueID) AS conta, SEvaluator.name AS names FROM reactiontime, SEvaluator, session WHERE reactiontime.sessionID=session.uniqueID AND session.evaluatorID=Sevaluator.uniqueID GROUP BY SEvaluator.name ORDER BY SEvaluator.name;")

pulses <- dbGetQuery(con, "SELECT COUNT(pulse.uniqueID) AS conta, SEvaluator.name AS names FROM pulse, SEvaluator, session WHERE pulse.sessionID=session.uniqueID AND session.evaluatorID=Sevaluator.uniqueID GROUP BY SEvaluator.name ORDER BY SEvaluator.name;")

#multichronopic <- dbGetQuery(con, "SELECT COUNT(multichronopic.uniqueID) AS conta, SEvaluator.name AS names FROM multichronopic, SEvaluator, session WHERE multichronopic.sessionID=session.uniqueID AND session.evaluatorID=Sevaluator.uniqueID GROUP BY SEvaluator.name ORDER BY SEvaluator.name;")

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
#a$runs <- as.numeric(a$runs)
#a$multichronopic <- as.numeric(a$multichronopic)
#fix duplicates:
columns = 7 #when add runs and multichronopic put 9.
a[(a$names == "Josep M. Padullés"),2:columns] = a[(a$names == "Josep M. Padullés"),2:columns] + a[(a$names == "Josep M Padullés"),2:columns]
a <- subset(a,a$names != "Josep M Padullés")
a[(a$names == "Jeffrey Pagaduan"),2:columns] = a[(a$names == "Jeffrey Pagaduan"),2:columns] + a[(a$names == "Jeffrey C. Pagaduan"),2:columns]
a <- subset(a,a$names != "Jeffrey C. Pagaduan")
#sort
#a <- a[order(a$persons + a$jumps + a$jumps_reactive + a$runs + a$runs_intervallic + a$reaction_times + a$pulses + a$multichronopic),]
a <- a[order(a$persons + a$jumps + a$jumps_reactive + a$runs_intervallic + a$reaction_times + a$pulses),]

#prepare graph
#b=cbind(a$persons, a$jumps , a$jumps_reactive, a$runs, a$runs_intervallic, a$reaction_times, a$pulses, a$multichronopic)
#colnames(b)=c("persons", "jumps", "jumps_reactive", "runs", "runs_intervallic", "reaction_times", "pulses", "multichronopic")
b=cbind(a$persons, a$jumps , a$jumps_reactive, a$runs_intervallic, a$reaction_times, a$pulses)

#colnames(b)=c("persons", "jumps", "jumps_reactive", "runs_intervallic", "reaction_times", "pulses")
colnames(b)=c(
	paste("persons (", sum(persons$conta), ")"), 
	paste("jumps (", sum(jumps$conta), ")"), paste("jumps_reactive (", sum(jumpsRj$conta), ")"), 
	paste("runs_intervallic (", sum(runsInterval$conta), ")"), 
	paste("reaction_times (", sum(reactionTimes$conta), ")"), 
	paste("pulses (", sum(pulses$conta), ")")
	)

rownames(b)=a$names

#graph
cex=.8
#change colors to 8 when add runs and multichronopic
colors=topo.colors(6)

par(mar=c(5,4,5.5,2), oma=c(1,7,1,1))

barplot(t(b), horiz=T, las=2, col=colors, cex.names=cex,axes=F)
axis(3, cex.axis=.8)
legend("right", colnames(b), pch=15, col=colors, cex=.8)

title(main="Data uploaded by evaluator",
  sub=paste(Sys.Date(),"(YYYY-MM-DD)"), cex.sub = 0.75, font.sub = 3, col.sub = "red")

par(new=TRUE)
par(mar=c(5.2,12,20,2))

persons <- dbGetQuery(con, "SELECT session.uploadedDate AS date, count(personSession77.uniqueID) AS conta FROM session,personSession77 WHERE personSession77.sessionID == session.UniqueID AND session.uploadedDate != '2007-07-30' GROUP BY date;")
jumps <- dbGetQuery(con, "SELECT session.uploadedDate AS date, count(jump.uniqueID) AS conta FROM session,jump WHERE jump.sessionID == session.UniqueID AND session.uploadedDate != '2007-07-30' GROUP BY date;")
jumpsRj <- dbGetQuery(con, "SELECT session.uploadedDate AS date, count(jumpRj.uniqueID) AS conta FROM session,jumpRj WHERE jumpRj.sessionID == session.UniqueID AND session.uploadedDate != '2007-07-30' GROUP BY date;")
dates = c(as.Date(persons$date),as.Date(jumps$date),as.Date(jumpsRj$date))
minx=min(dates)
maxx=max(dates)
maxy=max(c(sum(persons$conta),sum(jumps$conta),sum(jumpsRj$conta)))

plot(as.Date(persons$date), cumsum(persons$conta), type='s', lwd=2, col=colors[1], xlim=c(minx,maxx), ylim=c(0,maxy), ylab="", cex.axis=.8, las=T)
abline(v=seq(as.Date("2009/1/1"), as.Date("2020/1/1"), by="3 months"),lty=3)
par(new=TRUE)
plot(as.Date(jumps$date), cumsum(jumps$conta), type='s', lwd=2, col=colors[2], xlim=c(minx,maxx), ylim=c(0,maxy), ylab="", axes=F)
par(new=TRUE)
plot(as.Date(jumpsRj$date), cumsum(jumpsRj$conta), type='s', lwd=2, col=colors[3], xlim=c(minx,maxx), ylim=c(0,maxy), ylab="", axes=F)

dev.off()
