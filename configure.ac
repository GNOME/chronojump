AC_PREREQ([2.54])

# PACKAGE_VERSION variable comes from here (e.g. to show it to the user,
# distribution package names, etc.)
AC_INIT([chronojump], [m4_esyscmd_s([git describe])])

# This is used in AssemblyInfo.cs.in: it can only be major.minor.release format
AC_SUBST([PACKAGE_SIMPLIFIED_VERSION], [m4_esyscmd_s([git describe --abbrev=0])])

# Date of the last commit
AC_SUBST([BUILD_CHRONOJUMP_LAST_COMMIT_TIMESTAMP], [m4_esyscmd_s([git log -1 --format=%ct])])

# GIT_BRANCH is used by cerbero to know which branch needs to be build (on OSX)
AC_SUBST([GIT_BRANCH], [m4_esyscmd_s([git symbolic-ref --short HEAD])])


AM_INIT_AUTOMAKE([foreign])

AC_CONFIG_MACRO_DIR([build/m4])
AC_SUBST([ACLOCAL_AMFLAGS], ["-I build/m4/shamrock -I build/m4/shave \${ACLOCAL_FLAGS}"])

AM_PROG_CC_STDC
AC_ISC_POSIX
AC_PROG_CC
AC_C_CONST
AC_HEADER_STDC
AM_PROG_LIBTOOL
AM_PATH_PYTHON
LT_INIT

dnl Check for pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
        AC_MSG_ERROR([You need to install pkg-config])
fi

SHAMROCK_EXPAND_LIBDIR
SHAMROCK_EXPAND_BINDIR
SHAMROCK_EXPAND_DATADIR


AC_PROG_INSTALL

#*******************************************************************************
# Internationalization
#*******************************************************************************
GETTEXT_PACKAGE=chronojump
AC_SUBST(GETTEXT_PACKAGE)
AC_DEFINE_UNQUOTED(GETTEXT_PACKAGE,"$GETTEXT_PACKAGE", [GETTEXT package name])

dnl Check for gettext utils
AC_PATH_PROG(MSGFMT, msgfmt, no)
if test "x$MSGFMT" = "xno"; then
	AC_MSG_ERROR([gettext not found])
else
	AC_SUBST(MSGFMT,[msgfmt])
fi

IT_PROG_INTLTOOL([0.40.0])
AM_GLIB_GNU_GETTEXT

dnl Mono and C# compiler
dnl Check first for a 4.0 compiler
AC_PATH_PROG(MONO, mono, yes)
AC_PATH_PROG(MCS, mcs, yes)

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
    GMCS_FLAGS="$GMCS_FLAGS -define:OSTYPE_WINDOWS"
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
AC_SUBST(GMCS_FLAGS, ["$GMCS_FLAGS"])
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

dnl Disable tests
AM_CONDITIONAL(ENABLE_TESTS, test x = y)


dnl package checks, common for all configs
PKG_CHECK_MODULES([GLIB_SHARP_30], [glib-sharp-3.0])
PKG_CHECK_MODULES([GTK_SHARP_30], [gtk-sharp-3.0])

AC_CONFIG_FILES([
Makefile
build/Makefile
build/m4/Makefile
build/m4/shave/shave
build/m4/shave/shave-libtool
binariesMac/Makefile
src/Makefile
src/Mini/Makefile
src/chronojump-importer/Makefile
po/Makefile.in
encoder/Makefile
r-scripts/Makefile
manual/Makefile
src/AssemblyInfo.cs
src/buildInfo.cs
src/chronojump
src/chronojump.desktop
src/Mini/chronojump_mini
win32/chronojump_innosetup.iss
win32/Makefile
])

AC_CONFIG_FILES([deb-pkg.sh], [chmod +x deb-pkg.sh])

AC_OUTPUT
