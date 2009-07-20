export MONO_INSTALL_PATH=/c/Mono-2.4.2.2/
export PATH=$PATH:/c/Mono-2.4.2.2/bin
export MONO_PATH=.
export PKG_CONFIG_PATH=$MONO_INSTALL_PATH/lib/pkgconfig/
windres logo.rc logo.o
$MONO_INSTALL_PATH/bin/mono.exe $MONO_INSTALL_PATH/lib/mono/2.0/mkbundle.exe ./chronojump.prg --deps -c -o temp.c -oo temp.o
gcc -mno-cygwin -g -o ../bin/ChronoJump.exe -Wall temp.c `pkg-config --cflags --libs mono`  logo.o temp.o
$MONO_INSTALL_PATH/bin/mono.exe $MONO_INSTALL_PATH/lib/mono/2.0/mkbundle.exe ./chronojump_mini.prg --deps -c -o temp.c -oo temp.o
gcc -mno-cygwin -g -o ../bin/ChronoJump_mini.exe -Wall temp.c `pkg-config --cflags --libs mono`  logo.o temp.o
