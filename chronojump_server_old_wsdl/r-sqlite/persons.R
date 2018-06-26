#library(GDD)
#GDD(file="/var/www/web/server/images/persons.png", type="png", w=670, h=670)
#file = "/root/.local/share/Chronojump/database/chronojump_server.db"
png(file="persons.png", w=800, h=800) #local PNG
file = "~/.local/share/Chronojump/database/chronojump_server_2013-07-05.db"
title = "Data uploaded by evaluator"
subtitle=paste(Sys.Date(),"(YYYY-MM-DD)")
col.sub="red"
col.sex=rainbow(2)
col.level=rainbow(4)

#local PDF
#pdf(file="persons.pdf", width=7, height=7)
#file = "/home/xavier/.local/share/Chronojump/database/chronojump_server_2012-03-07.db"
#title = ""
#subtitle="2012-02-16 (YYYY-MM-DD)"
#col.sub="gray30"
#col.sex=c("gray30","gray60")
#col.level=c("black","gray30","gray60","white")
#--------------------------------------------------------


library(RSQLite)
drv = dbDriver("SQLite")
con = dbConnect(drv, file)

def.par <- par(no.readonly = TRUE) # save default, for resetting...

#refered to person77
persons <- dbGetQuery(con, "SELECT sport.name AS sport, speciallity.name AS speciallity, country.name AS country, person77.sex AS sex, personSession77.practice AS level FROM person77, personSession77, country, sport, speciallity WHERE person77.uniqueID=personSession77.personID AND person77.countryID = country.uniqueID AND personSession77.sportID == sport.uniqueID AND personSession77.speciallityID=speciallity.uniqueID GROUP BY personID")

#done separately because it's refered to personSession77
ages <- dbGetQuery(con, "SELECT (strftime('%Y', session.date) - strftime('%Y', person77.dateborn)) - (strftime('%m-%d', session.date) < strftime('%m-%d', person77.dateborn)) AS years from person77, personSession77, session WHERE personSession77.personID = person77.uniqueID AND personSession77.sessionID = session.UniqueID AND years>1")


persons$sportF<-factor(persons$sport)
#persons$speciallityF<-factor(persons$speciallity)
persons$countryF<-factor(persons$country)
persons$sexF<-factor(persons$sex)
persons$levelF<-factor(persons$level)

par(new=FALSE, oma=c(1,1,5,0))
#par(mfcol=c(2,2))
nf <- layout(matrix(c(1,1,2,3,4,5), 3, 2, byrow=FALSE), widths=c(2,1), heights=c(10,10,13), respect=FALSE)
#layout.show(nf)

dotchart(table(persons$sportF)[order (table(persons$sportF))], labels=strtrim(levels(persons$sportF),15)[order (table(persons$sportF))], main="Sport",cex=.8)
abline(v=seq(from=0,to=max(table(persons$sportF)),by=10),col = "lightgray", lty=3)
#dotchart(table(persons$speciallityF), labels=levels(persons$speciallityF), main="athletics speciallities")
#SPECIALLITIES fer amb un altre select i nomes d'atletisme

dotchart(table(persons$countryF)[order (table(persons$countryF))], labels=strtrim(levels(persons$countryF),15)[order (table(persons$countryF))], main="Country",cex=.8)
abline(v=seq(from=0,to=max(table(persons$countryF)),by=25),col = "lightgray", lty=3)

pie(table(persons$sexF), labels=levels(persons$sexF), main="Gender", col=col.sex)

levels(persons$levelF)=c("Sedentary", "Regular practice", "Competition", "Elite") #undefined is impossible on server
  pie(table(persons$levelF), main="Level", col=col.level,cex=.8)

hist(ages$years, breaks=10, main="Age", xlab="Years (at session day)")

  #par(mfcol=c(1,1))
  par(def.par)#- reset to default

par(new=TRUE)
plot(-1,type="n",axes=F,xlab='',ylab='')
title(main=title, sub=subtitle, cex.sub = 0.8, font.sub = 3, col.sub = col.sub)

dev.off()


