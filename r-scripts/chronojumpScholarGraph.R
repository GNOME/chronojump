years = seq(from=2004, to=2022, by=1)

# from 2023 to 2004
#Do it like this and just go fro 2023 to 2004 at ylo. And make grow start if needed
#https://scholar.google.com/scholar?start=829&q=chronojump&hl=ca&as_sdt=0,5&as_ylo=2023&as_vis=1
#because using the specific interval there can be huge differences
#IMP: it seems after has me asked if I am a human, all the results are exact instead of "approximated"
#f23 means from 2023
f23 = 98
f22 = 301
f21 = 483
f20 = 654
f19 = 759
f18 = 839
f17 = 881
f16 = 930
f15 = 949
f14 = 969
f13 = 980
f12 = 989
f11 = 993
f10 = 995
f09 = 995
f08 = 996
f07 = 997
f06 = 999
f05 = 1010
f04 = 1010
from2023To2004 = c(f23, f22, f21, f20, f19, f18, f17, f16, f15, f14, f13, f12, f11, f10, f09, f08, f07, f06, f05, f04)
results = rev (diff (from2023To2004)) #as it is a diff, it will show from 2022


png(filename="chronojumpScholarGraph.png", width=756, height=500, units="px")
bp = barplot(results, names.arg=years, las=2, main="New results of 'Chronojump' on Google Scholar by years", sub="Excluding cites and patents", col=rev(heat.colors(length(years))))
fit <- lm(results ~ poly(bp[,1], 3))
x0 <- seq(min(bp[,1]), max(bp[,1]), length = length(results))
y0 <- predict.lm(fit, newdata = list(x = x0))
#lines(x0,y0, lwd=2)
mtext(side=3, at=x0[length(x0)], results[length(x0)])
dev.off()
