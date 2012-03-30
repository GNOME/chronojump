import sys
import subprocess

print(sys.argv)

#R CMD BATCH --no-save 

subprocess.Popen([
	r"Rscript","/home/xavier/informatica/progs_meus/chronojump/chronojump/encoder/graph.R",
	sys.argv[1],			#input data
	sys.argv[2],			#output graph
	sys.argv[3],sys.argv[4],	#output data1, output data2
	sys.argv[5],			#ep.isJump
	sys.argv[6],			#ep.contractionEC
	sys.argv[7],			#ep.analysis
	sys.argv[8],			#ep.smooth
	sys.argv[9],			#ep.curve
	sys.argv[10],sys.argv[11]	#ep.width, ep.height
	]).wait()




#subprocess.Popen([r"R","CMD BATCH --no-save ","/home/xavier/informatica/progs_meus/chronojump/chronojump/encoder/graph.R",sys.argv[1],sys.argv[2],sys.argv[3]]).wait()

print "Python done"

