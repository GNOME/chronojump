#The first argument of the script must be the Mono installation path
export MONO_INSTALL_PATH=$1
export PATH=$PATH:/c/Mono-2.4/bin
export MONO_PATH=.
export PKG_CONFIG_PATH=$MONO_INSTALL_PATH/lib/pkgconfig/
windres logo.rc logo.o
$MONO_INSTALL_PATH/bin/mono.exe $MONO_INSTALL_PATH/lib/mono/2.0/mkbundle.exe ./Chronojump.exe --deps -c -o temp.c -oo temp.o
gcc -mno-cygwin -g -o ../bin/Chronojump.exe -Wall temp.c `pkg-config --cflags --libs mono`  logo.o temp.o
$MONO_INSTALL_PATH/bin/mono.exe $MONO_INSTALL_PATH/lib/mono/2.0/mkbundle.exe ./Chronojump_Mini.exe --deps -c -o temp.c -oo temp.o
gcc -mno-cygwin -g -o ../bin/Chronojump_mini.exe -Wall temp.c `pkg-config --cflags --libs mono`  logo.o temp.o
