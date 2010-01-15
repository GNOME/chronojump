library(GDD)
GDD(file="/var/www/web/server/images/persons.png", type="png", w=670, h=670)
library(RSQLite)
drv = dbDriver("SQLite")
file = "/root/.local/share/Chronojump/database/chronojump_server.db"
con = dbConnect(drv, file)

def.par <- par(no.readonly = TRUE) # save default, for resetting...


persons <- dbGetQuery(con, "select sport.name as sport, speciallity.name as speciallity, country.name as country, person.sex as sex, person.practice as level from person, country, sport, speciallity where person.countryID = country.uniqueID and person.sportID == sport.uniqueID and person.speciallityID=speciallity.uniqueID")

persons$sportF<-factor(persons$sport)
#persons$speciallityF<-factor(persons$speciallity)
persons$countryF<-factor(persons$country)
persons$sexF<-factor(persons$sex)
persons$levelF<-factor(persons$level)

par(new=FALSE, oma=c(1,1,5,1))
#par(mfcol=c(2,2))
nf <- layout(matrix(c(1,1,1,2,3,4), 3, 2, byrow=FALSE), respect=TRUE)
#layout.show(nf)

dotchart(table(persons$sportF)[order (table(persons$sportF))], labels=levels(persons$sportF)[order (table(persons$sportF))], main="Sport")
#dotchart(table(persons$speciallityF), labels=levels(persons$speciallityF), main="athletics speciallities")
#SPECIALLITIES fer amb un altre select i nomes d'atletisme

dotchart(table(persons$countryF)[order (table(persons$countryF))], labels=levels(persons$countryF)[order (table(persons$countryF))], main="Country")

pie(table(persons$sexF), labels=levels(persons$sexF), main="Sex",
  col=rainbow(length(levels(persons$sexF))))

levels(persons$levelF)=c("Sedentary", "Regular practice", "Competition", "Elite") #undefined is impossible on server
  pie(table(persons$levelF), main="Level", col=rainbow(length(levels(persons$levelF))))

  #par(mfcol=c(1,1))
  par(def.par)#- reset to default

par(new=TRUE)
plot(-1,type="n",axes=F,xlab='',ylab='')
title(main="Persons data in server",
  sub=paste(Sys.Date(),"(YYYY-MM-DD)"), cex.sub = 0.75, font.sub = 3, col.sub = "red")

  dev.off()



