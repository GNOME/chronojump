#! /bin/sh

PROJECT=ChronoJump
FILE=
CONFIGURE=configure.ac

: ${AUTOCONF=autoconf}
: ${AUTOHEADER=autoheader}
: ${AUTOMAKE=automake}
: ${LIBTOOLIZE=libtoolize}
: ${INTLTOOLIZE=intltoolize}
: ${ACLOCAL=aclocal}
: ${LIBTOOL=libtool}

srcdir=`dirname $0`
test -z "$srcdir" && srcdir=.

ORIGDIR=`pwd`
cd $srcdir
TEST_TYPE=-f
aclocalinclude="-I m4/shamrock -I m4/shave $ACLOCAL_FLAGS"
conf_flags="--enable-maintainer-mode"

#test $TEST_TYPE $FILE || {
#        echo "You must run this script in the top-level $PROJECT directory"
#        exit 1
#}

if test -z "$*"; then
        echo "I am going to run ./configure with no arguments - if you wish "
        echo "to pass any to it, please specify them on the $0 command line."
fi

case $CC in
*xlc | *xlc\ * | *lcc | *lcc\ *) am_opt=--include-deps;;
esac

(grep "^AM_PROG_LIBTOOL" $CONFIGURE >/dev/null) && {
    echo "Running $LIBTOOLIZE ..."
    $LIBTOOLIZE --force --copy --automake
}

(grep "^IT_PROG_INTLTOOL" $CONFIGURE >/dev/null) && {
    echo "Running $INTLTOOLIZE ..."
    $INTLTOOLIZE --force --copy --automake
}

echo "Running $ACLOCAL $aclocalinclude ..."
$ACLOCAL $aclocalinclude

echo "Running $AUTOMAKE --gnu $am_opt ..."
$AUTOMAKE --add-missing --gnu $am_opt

echo "Running $AUTOCONF ..."
$AUTOCONF

echo Running $srcdir/configure $conf_flags "$@" ...
$srcdir/configure  $conf_flags "$@" \
