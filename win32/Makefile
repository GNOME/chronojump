deploy dist/bin/libcesarplayer-0.dll:
	python deploy_win32.py
	cp -r deps/* dist/

	#pyserial_pyper_windows 64 bits don't do
	#python ../encoder/setup-pyserial_pyper_windows.py install
	#python ../encoder/setup-pyserial_pyper_windows.py py2exe -d "dist/bin/encoder"
	#cp ../encoder/*.wav dist/bin/encoder

	#pyserial_pyper_windows 32 bits
	c:/Python27-32/Python.exe ../encoder/setup-pyserial_pyper_windows.py install
	c:/Python27-32/Python.exe ../encoder/setup-pyserial_pyper_windows.py py2exe -d "dist/bin/encoder"
	
	#chronopic-firmwarecord
	cd ../chronopic-firmware/chronopic-firmwarecord/;c:/Python27-32/Python.exe setup.py -d "dist/bin"
	mkdir dist/bin/chronopic-firmwarecord-dir
	cp ../chronopic-firmware/chronopic-firmwarecord/dist/* dist/bin/chronopic-firmwarecord-dir/
	
	make install -C ../
	mv dist/lib/bin/libcesarplayer-0.dll dist/bin


bundle: dist/bin/libcesarplayer-0.dll
	windres logo.rc logo.o
	MONO_PATH=dist/lib/chronojump mkbundle dist/lib/chronojump/Chronojump.exe --deps -c -o temp.c -oo temp.o
	${CC} -g -o dist/bin/Chronojump.exe -Wall temp.c `pkg-config --cflags --libs mono-2`  logo.o temp.o
	MONO_PATH=dist/lib/chronojump mkbundle dist/lib/chronojump/Chronojump_Mini.exe --nodeps -c -o temp.c -oo temp.o
	${CC} -g -o dist/bin/Chronojump_mini.exe -Wall temp.c `pkg-config --cflags --libs mono-2`  logo.o temp.o

installer: deploy bundle
	#this executes the installer, but the chronojump_innosetup.iss.in is converted in chronojump_innosetup.iss in the autogen.sh
	"/c/Program Files (x86)/Inno Setup 5/ISCC.exe" chronojump_innosetup.iss
