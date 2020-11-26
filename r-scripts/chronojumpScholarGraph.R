#2019
years = seq(from=2004, to=2019, by=1)
results = c(1,5,2,1,0,1,1,5,10,10,22,20,44,51,70,81)
png(filename="chronojumpScholarGraph.png", width=756, height=500, units="px")
barplot(results, names.arg=years, las=2, main="New results of 'Chronojump' on Google Scholar by years", sub="Excluding cites and patents", col=rev(heat.colors(length(years))))
dev.off()


#2020
years = seq(from=2004, to=2020, by=1)
results = c(1,5,2,1,0,1,1,5,10,10,22,20,44,51,70,81,120) #waiting until end of year

bp = barplot(results, names.arg=years, las=2, main="New results of 'Chronojump' on Google Scholar by years", sub="Excluding cites and patents", col=rev(heat.colors(length(years))))
fit <- lm(results ~ poly(bp[,1], 3))
x0 <- seq(min(bp[,1]), max(bp[,1]), length = length(results))
y0 <- predict.lm(fit, newdata = list(x = x0))
lines(x0,y0, lwd=2)
