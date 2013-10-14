ASSEMBLY = RDotNet.NativeLibrary
TARGET = library

LINK = $(REF_DEP_RDOTNET_NATIVE)

SOURCES = \
    Properties/AssemblyInfo.cs \
    UnmanagedDll.cs \
    NativeUtility.cs

if !OSTYPE_WINDOWS
ASSEMBLY_BUILD_FLAGS = -define:UNIX
endif

include $(top_srcdir)/build/build.mk
