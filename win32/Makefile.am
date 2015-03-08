deploy:
	python deploy_win32.py
	cp -r deps/* dist/ #this will copy also the old libglade that correctly translates gui
	python ../encoder/setup-pyserial_pyper_windows.py install
	python ../encoder/setup-pyserial_pyper_windows.py py2exe -d "dist/bin/encoder"
	cd ../chronopic-firmware/chronopic-firmwarecord/; python setup.py -d "dist/bin"
	mkdir dist/bin/chronopic-firmwarecord-dir
	cp ../chronopic-firmware/chronopic-firmwarecord/dist/* dist/bin/chronopic-firmwarecord-dir/
	
	make install -C ../
	cp ${prefix}/bin/libcesarplayer-0.dll dist/bin
	cp -r ${prefix}/lib/chronojump dist/lib/
	cp -r ${prefix}/share/chronojump dist/share/
	cp -r ../rdotnet/Rdll/i386/* dist/bin/i386/
	mkdir dist/bin/x64
	cp -r ../rdotnet/Rdll/x64/* dist/bin/x64/
	cp logchronojump.bat dist/bin/
	


bundle: deploy
	windres logo.rc logo.o
	MONO_PATH=${MONO_PATH}\;dist/lib/chronojump mkbundle --config ${prefix}/etc/mono/config dist/lib/chronojump/Chronojump.exe --deps -c -o temp.c -oo temp.o
	${CC} -g -o dist/bin/Chronojump.exe -Wall temp.c `pkg-config --cflags --libs mono-2`  logo.o temp.o
	MONO_PATH=${MONO_PATH}\;dist/lib/chronojump mkbundle dist/lib/chronojump/Chronojump_Mini.exe --nodeps -c -o temp.c -oo temp.o
	${CC} -g -o dist/bin/Chronojump_mini.exe -Wall temp.c `pkg-config --cflags --libs mono-2`  logo.o temp.o

installer: deploy bundle
	"/c/Program Files (x86)/Inno Setup 5/ISCC.exe" chronojump_innosetup.iss
