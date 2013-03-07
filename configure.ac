dnl Warning: This is an automatically generated file, do not edit!
dnl Process this file with autoconf to produce a configure script.
AC_PREREQ([2.54])
AC_INIT([chronojump], [1.3.6])
AM_INIT_AUTOMAKE([foreign])
AM_MAINTAINER_MODE

AC_CONFIG_MACRO_DIR([m4])
AC_SUBST([ACLOCAL_AMFLAGS], ["-I m4/shamrock -I m4/shave \${ACLOCAL_FLAGS}"])

AM_PROG_LIBTOOL

AM_PATH_PYTHON

dnl Check for pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
        AC_MSG_ERROR([You need to install pkg-config])
fi

SHAMROCK_EXPAND_LIBDIR
SHAMROCK_EXPAND_BINDIR
SHAMROCK_EXPAND_DATADIR


AC_PROG_INSTALL

dnl Mono and C# compiler
dnl Check first for a 4.0 compiler or than fallback to 2.0
SHAMROCK_CHECK_MONO_MODULE(2.4.0)
PKG_CHECK_EXISTS([mono >= 2.8],
		[SHAMROCK_FIND_MONO_4_0_COMPILER
		SHAMROCK_CHECK_MONO_4_0_GAC_ASSEMBLIES([
			System.Data
			Mono.Cairo
			Mono.Posix
			Mono.Data.Sqlite
		])]
,
		[SHAMROCK_FIND_MONO_2_0_COMPILER
		SHAMROCK_CHECK_MONO_2_0_GAC_ASSEMBLIES([
			System.Data
			Mono.Cairo
			Mono.Posix
			Mono.Data.Sqlite
		])])
AC_SUBST(MCS, ["$MCS"])
SHAMROCK_FIND_MONO_RUNTIME

dnl Check for gettext utils
AC_PATH_PROG(MSGFMT, msgfmt, no)
if test "x$MSGFMT" = "xno"; then
	AC_MSG_ERROR([gettext not found])
else 
	AC_SUBST(MSGFMT,[msgfmt])
fi

AC_MSG_CHECKING([for the OS type])
ostype=""

case "$host" in
  *-*-mingw*|*-*-cygwin*)
    ostype=Windows
    AC_DEFINE(OSTYPE_WINDOWS)
    ;;
  *-*-darwin*)
    AC_DEFINE(OSTYPE_OS_X)
    ostype=OS_X
    ;;
  *-*-linux*)
    AC_DEFINE(OSTYPE_LINUX)
    ostype=Linux
    ;;
esac
AM_CONDITIONAL(OSTYPE_WINDOWS, test x$ostype = xWindows)
AM_CONDITIONAL(OSTYPE_OS_X, test x$ostype = xOS_X)
AM_CONDITIONAL(OSTYPE_LINUX, test x$ostype = xLinux)
AC_MSG_RESULT($ostype)


AC_ARG_ENABLE(debug,
	AC_HELP_STRING([--enable-debug],
		[Use 'DEBUG' Configuration [default=NO]]),
		enable_debug=yes, enable_debug=no)
AM_CONDITIONAL(ENABLE_DEBUG, test x$enable_debug = xyes)
if test "x$enable_debug" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(release,
	AC_HELP_STRING([--enable-release],
		[Use 'RELEASE' Configuration [default=YES]]),
		enable_release=yes, enable_release=no)
AM_CONDITIONAL(ENABLE_RELEASE, test x$enable_release = xyes)
if test "x$enable_release" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
if test -z "$CONFIG_REQUESTED" ; then
	AM_CONDITIONAL(ENABLE_RELEASE, true)
	enable_release=yes
fi


dnl package checks, common for all configs
PKG_CHECK_MODULES([GLIB_SHARP_20], [glib-sharp-2.0])
PKG_CHECK_MODULES([GTK_SHARP_20], [gtk-sharp-2.0])
PKG_CHECK_MODULES([GLADE_SHARP_20], [glade-sharp-2.0])

PKG_CHECK_MODULES(CESARPLAYER, [gtk+-2.0 >= 2.8 gdk-2.0 gio-2.0 glib-2.0 gstreamer-0.10 gstreamer-audio-0.10 gstreamer-video-0.10 gstreamer-pbutils-0.10 gobject-2.0 gstreamer-interfaces-0.10 gstreamer-app-0.10 gstreamer-tag-0.10])
AC_SUBST(CESARPLAYER_CFLAGS)
AC_SUBST(CESARPLAYER_LIBS)

AC_CONFIG_FILES([
Makefile
chronopic-firmware/Makefile
chronopic-firmware/chronopic-firmwarecord/Makefile
chronopic-firmware/chronopic-firmwarecord/libIris/Makefile
chronopic-firmware/chronopic-firmwarecord/chronopic-firmwarecord
chronopic-tests/Makefile
src/Makefile
po/Makefile
chronojump_server/Makefile
encoder/Makefile
manual/Makefile
src/AssemblyInfo.cs
src/chronojump
src/chronojump_mini
src/chronojump.desktop
libcesarplayer/Makefile
CesarPlayer/Makefile
CesarPlayer/AssemblyInfo.cs
CesarPlayer/CesarPlayer.dll.config
win32/chronojump_innosetup.iss
rdotnet/Makefile
])

AC_OUTPUT
