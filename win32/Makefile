deploy:
	python deploy_win32.py
	cp -r deps/* dist/
	python ../encoder/setup-pyserial_pyper_windows.py install
	python ../encoder/setup-pyserial_pyper_windows.py py2exe -d "dist/bin/encoder"
	cp ../encoder/*.wav dist/bin/encoder
	make install -C ../
	mv dist/lib/bin/libcesarplayer-0.dll dist/bin

bundle: deploy
	windres logo.rc logo.o
	MONO_PATH=dist/lib/chronojump mkbundle dist/lib/chronojump/Chronojump.exe --deps -c -o temp.c -oo temp.o
	${CC} -g -o dist/bin/Chronojump.exe -Wall temp.c `pkg-config --cflags --libs mono-2`  logo.o temp.o
	MONO_PATH=dist/lib/chronojump mkbundle dist/lib/chronojump/Chronojump_Mini.exe --nodeps -c -o temp.c -oo temp.o
	${CC} -g -o dist/bin/Chronojump_mini.exe -Wall temp.c `pkg-config --cflags --libs mono-2`  logo.o temp.o

installer: bundle
	"/c/Program Files (x86)/Inno Setup 5/ISCC.exe" chronojump_innosetup.iss
