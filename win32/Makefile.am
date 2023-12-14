deploy:
#	python3 deploy_win32.py
#	cp -r deps/* dist/
	
	rm dist/FTD2XX_NET_v1.2.0/Readme.chronojump.txt
	mv dist/FTD2XX_NET_v1.2.0/* dist/bin
	rmdir dist/FTD2XX_NET_v1.2.0

#	python3 ../src/chronojump-importer/py2exec_chronojump_importer.py py2exe -d "dist/bin/chronojump-importer"
#	python3 ../encoder/setup-pyserial_pyper_windows.py install
#	python3 ../encoder/setup-pyserial_pyper_windows.py py2exe -d "dist/bin/encoder"
#	cd ../chronopic-firmware/chronopic-firmwarecord/; python3 setup.py -d "dist/bin"
#	mkdir dist/bin/chronopic-firmwarecord-dir
#	cp ../chronopic-firmware/chronopic-firmwarecord/dist/* dist/bin/chronopic-firmwarecord-dir/
	
#	make install -C ../
#	cp -r ${prefix}/lib/chronojump dist/lib/
#	cp -r ${prefix}/share/chronojump dist/share/
#	mkdir dist/bin/x64
#	cp logchronojump.bat dist/bin/
	


bundle: deploy
#	windres logo.rc logo.o
#	MONO_PATH=${MONO_PATH}\;dist/lib/chronojump mkbundle --config-dir ${prefix}/etc/mono/ --machine-config ${prefix}/etc/mono/4.5/machine.config --config ${prefix}/etc/mono/config dist/lib/chronojump/Chronojump.exe --deps -c -o temp.c -oo temp.o
#	${CC} -g -o dist/bin/Chronojump.exe -Wall temp.c `pkg-config --cflags --libs mono-2`  logo.o temp.o
#	MONO_PATH=${MONO_PATH}\;dist/lib/chronojump mkbundle dist/lib/chronojump/Chronojump_Mini.exe --nodeps -c -o temp.c -oo temp.o
#	${CC} -g -o dist/bin/Chronojump_mini.exe -Wall temp.c `pkg-config --cflags --libs mono-2`  logo.o temp.o

installer: deploy bundle
#	WINEARCH=win32 WINEPREFIX=~/.wine32 wine "/mnt/d/Program Files (x86)/Inno Setup 6/ISCC.exe" chronojump_innosetup_x64.iss
#	WINEARCH=win32 WINEPREFIX=~/.wine32 wine package-x64.bat
