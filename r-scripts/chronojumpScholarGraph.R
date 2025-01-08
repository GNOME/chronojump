years = seq(from=2004, to=2024, by=1)

# from 2023 to 2004
#Do it like this and just go fro 2023 to 2004 at ylo. And make grow start if needed
#https://scholar.google.com/scholar?start=829&q=chronojump&hl=ca&as_sdt=0,5&as_ylo=2023&as_vis=1
#because using the specific interval there can be huge differences
#IMP: it seems after has me asked if I am a human, all the results are exact instead of "approximated"
#f23 means from 2023
f25 = 3
f24 = 263
f23 = 468
f22 = 659
f21 = 833
f20 = 991
f19 = 1090
f18 = 1180
f17 = 1230
f16 = 1280
f15 = 1300
f14 = 1320
f13 = 1330
f12 = 1340
f11 = 1350
f10 = 1350
f09 = 1350
f08 = 1350
f07 = 1350
f06 = 1350
f05 = 1360
f04 = 1360
from2025To2004 = c(f25, f24, f23, f22, f21, f20, f19, f18, f17, f16, f15, f14, f13, f12, f11, f10, f09, f08, f07, f06, f05, f04)
results = rev (diff (from2025To2004)) #Important! as it is a diff, it will show from 2024


png(filename="chronojumpScholarGraph.png", width=756, height=500, units="px")
bp = barplot(results, names.arg=years, las=2, main="New results of 'Chronojump' on Google Scholar by years", sub="Excluding cites and patents", col=rev(heat.colors(length(years))))
fit <- lm(results ~ poly(bp[,1], 3))
x0 <- seq(min(bp[,1]), max(bp[,1]), length = length(results))
y0 <- predict.lm(fit, newdata = list(x = x0))
#lines(x0,y0, lwd=2)
mtext(side=3, at=x0[length(x0)], results[length(x0)])
dev.off()
