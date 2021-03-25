years = seq(from=2004, to=2020, by=1)
results = c(1,4,2,1,0,1,2,4,10,10,24,21,44,51,69,100,153) #2021: 31 at March 25

png(filename="chronojumpScholarGraph.png", width=756, height=500, units="px")
bp = barplot(results, names.arg=years, las=2, main="New results of 'Chronojump' on Google Scholar by years", sub="Excluding cites and patents", col=rev(heat.colors(length(years))))
fit <- lm(results ~ poly(bp[,1], 3))
x0 <- seq(min(bp[,1]), max(bp[,1]), length = length(results))
y0 <- predict.lm(fit, newdata = list(x = x0))
lines(x0,y0, lwd=2)
mtext(side=3, at=x0[length(x0)], results[length(x0)])
dev.off()
