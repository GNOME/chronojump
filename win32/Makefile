deploy:
	python deploy_win32.py
	cp -r deps/* dist/
	make install -C ../
	mv dist/lib/bin/libcesarplayer-0.dll dist/bin

bundle:
	windres logo.rc logo.o
	MONO_PATH=dist/lib/chronojump mkbundle dist/lib/chronojump/Chronojump.exe --deps -c -o temp.c -oo temp.o
	${CC} -g -o dist/bin/Chronojump.exe -Wall temp.c `pkg-config --cflags --libs mono-2`  logo.o temp.o
	MONO_PATH=dist/lib/chronojump mkbundle dist/lib/chronojump/Chronojump_Mini.exe --nodeps -c -o temp.c -oo temp.o
	${CC} -g -o dist/bin/Chronojump_mini.exe -Wall temp.c `pkg-config --cflags --libs mono-2`  logo.o temp.o

