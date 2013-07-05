#library(GDD)
#GDD(file="/var/www/web/server/images/tf_by_level_sex_jumptype.png", type="png", w=670, h=670)
png(file="tf_by_level_sex_jumptype.png", w=800, h=800) #local PNG
library(RSQLite)
drv = dbDriver("SQLite")
file = "~/.local/share/Chronojump/database/chronojump_server_2013-07-05.db"
con = dbConnect(drv, file)

jumps <- dbGetQuery(con, "SELECT person77.sex AS sex, personSession77.practice AS level, jump.* from person77, personSession77, jump WHERE person77.uniqueID == personSession77.personID AND level>=0 AND simulated>=0 AND person77.uniqueID == jump.personID")

jumps$sexF <- factor(jumps$sex, levels=c('M','F'), ordered=TRUE)
jumps$typeF <- factor(jumps$type)

library(car)
jumps$sexNum <- recode(jumps$sexF, '"M" = .1; "F" = -.1; ', 
  as.factor.result=FALSE)
     

  library(lattice)
  cols=c(topo.colors(4)[1], topo.colors(4)[2])
     par(pch=19, col=cols, cex.sub=0.75, font.sub=3, col.sub="red")
  xyplot(tv ~ (level+sexNum) |  typeF, groups=sexF, 
     #simpleTheme don't work in R 2.7.1 
     #par.settings = simpleTheme(pch=19,col=cols),
     scales=list(x=list(tick.number=3, relation='same'), 
     y=list(relation='same')),
     xlab="level",
     ylab="tf",
     pch=19, col=cols,
     auto.key=list(border=FALSE, text=c("Males", "Females"), col=cols, points = FALSE),
       main="TF by level, sex and jump type", 
        sub=paste(Sys.Date(),"(YYYY-MM-DD)"), cex.sub = 0.75, font.sub = 3, col.sub = "red",
     #key=list(title(main="TF by level, sex and jump type", 
     #   sub=paste(Sys.Date(),"(YYYY-MM-DD)"), cex.sub = 0.75, font.sub = 3, col.sub = "red")),
     data=jumps)
     
  dev.off()

