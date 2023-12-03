#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Chronojump
{
    /// <summary>
    /// DllImport resolver for native libraries
    /// </summary>
    public static class NativeLibraryResolver
    {
        static readonly List<Library> GTK_LIBS = new List<Library> {
            new Library ("intl", "8"),
            new Library ("glib-2.0"),
            new Library ("gio-2.0"),
            new Library ("gobject-2.0"),
            new Library ("gthread-2.0"),
            new Library ("gmodule-2.0"),
            new Library ("gtk-win32-2.0"),
            new Library ("gdk-win32-2.0"),
            new Library ("gdk_pixbuf-2.0"),
            new Library ("cairo", "2"),
            new Library ("atk-1.0"),
            new Library ("pango-1.0"),
            new Library ("pango-cairo-1.0"),
            new Library ("pangocairo-1.0"),
            new Library ("gtksharpglue-2", dllImportName: "gtksharpglue-2", isMSVC:true, isModule:true ),
            new Library ("glibsharpglue-2", dllImportName:"glibsharpglue-2", isMSVC:true, isModule:true ),
            new Library ("pangosharpglue-2", dllImportName:"pangosharpglue-2", isMSVC:true, isModule:true ),
        };

        static string libDir = "";

        static Dictionary<string, IntPtr> librariesCache = new Dictionary<string, IntPtr>();

        static Dictionary<string, Library> libraries;

        /// <summary>
        /// Initializes the resolver with the directory search path
        /// </summary>
        /// <param name="libDir">Directory where shared libraries are located</param>
        public static void Init(string libDir)
        {
            libraries = GTK_LIBS.ToDictionary(l => l.DllImportName, l => l);

            NativeLibraryResolver.libDir = libDir;
            // Chronojump
            NativeLibrary.SetDllImportResolver(typeof(NativeLibraryResolver).Assembly, ImportResolver);
            // gtk-sharp
            NativeLibrary.SetDllImportResolver(typeof(Gtk.Misc).Assembly, ImportResolver);
            // glib-sharp
            NativeLibrary.SetDllImportResolver(typeof(GLib.AbiField).Assembly, ImportResolver);
            // pango-sharp
            NativeLibrary.SetDllImportResolver(typeof(Pango.AttrBackground).Assembly, ImportResolver);
            // cairo-sharp
            NativeLibrary.SetDllImportResolver(typeof(Cairo.CairoAPI).Assembly, ImportResolver);
            // gdk-sharp
            NativeLibrary.SetDllImportResolver(typeof(Gdk.Atom).Assembly, ImportResolver);
            // Fluendo.SDK
        }

        static IntPtr ImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            IntPtr libHandle;

            if (librariesCache.TryGetValue(libraryName, out libHandle))
            {
                return libHandle;
            }

            if (libraries.TryGetValue(libraryName, out Library library))
            {
                //Determin dll path according to architecture [By Joeries]
                string path = string.Empty;
                if (UtilAll.IsWindows())
                {
                    path = Path.Combine(libDir, Environment.Is64BitProcess ? "x64" : "x86", library.ToString());
                    if (!File.Exists(path))
                    {
                        path = Path.Combine(libDir, library.ToString());
                    }
                }
                else
                {
                    path = Path.Combine(libDir, library.ToString());
                }
                if (!File.Exists(path))
                {
                    throw new Exception($"Library not found at the expected location: {path}");
                }
                NativeLibrary.TryLoad(path, out libHandle);
                if (libHandle == IntPtr.Zero)
                {
                    throw new Exception($"Could not load library {libraryName} at path {path}");
                }
            }
            else if (searchPath != null)
            {
                NativeLibrary.TryLoad(libraryName, assembly, searchPath, out libHandle);
            }
            else
            {
                NativeLibrary.TryLoad(libraryName, assembly, DllImportSearchPath.ApplicationDirectory, out libHandle);
            }
            if (libHandle == IntPtr.Zero)
            {
                throw new Exception($"Could not load library {libraryName}");
            }
            librariesCache.Add(libraryName, libHandle);
            return libHandle;
        }

        struct Library
        {
            public Library(string name, string version = "0", bool isMSVC = false,
                bool isModule = false, string dllImportName = null)
            {
                Name = name;
                Version = version;
                IsMSVC = isMSVC;
                IsModule = isModule;
                DllImportName = dllImportName;
                if (DllImportName == null)
                {
                    DllImportName = GetLibraryName(OSPlatform.Windows);
                }
            }

            public string Name { get; set; }
            public string Version { get; set; }
            public bool IsMSVC { get; set; }
            public bool IsModule { get; set; }
            public string DllImportName { get; set; }


            public override string ToString()
            {
                OSPlatform platform;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    platform = OSPlatform.OSX;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    platform = OSPlatform.Linux;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    platform = OSPlatform.Windows;
                }
                else
                {
                    throw new NotSupportedException($"Platform  {Environment.OSVersion.Platform} not supported");
                }
                return GetLibraryName(platform);
            }

            private readonly string GetLibraryName(OSPlatform platform)
            {
                string libSuffix;
                string libPrefix = "";
                string lib = Name;

                if (platform == OSPlatform.OSX)
                {

                    if (IsModule)
                    {
                        libSuffix = $".so";
                    }
                    else if (Version != null)
                    {
                        libSuffix = $".{Version}.dylib";
                    }
                    else
                    {
                        libSuffix = $".dylib";
                    }

                    libPrefix = "lib";
                    lib = lib.Replace("win32", "quartz");
                }
                else if (platform == OSPlatform.Linux)
                {
                    if (Version != null)
                    {
                        libSuffix = $".so.{Version}";
                    }
                    else
                    {
                        libSuffix = $".so";
                    }
                    libPrefix = "lib";
                    lib = lib.Replace("win32", "x11");
                }
                else if (platform == OSPlatform.Windows)
                {
                    if (Version != null)
                    {
                        libSuffix = $"-{Version}.dll";
                    }
                    else
                    {
                        libSuffix = $".dll";
                    }
                    if (!IsMSVC)
                    {
                        libPrefix = "lib";
                    }
                }
                else
                {
                    throw new NotSupportedException($"Platform  {Environment.OSVersion.Platform} not supported");
                }

                return $"{libPrefix}{lib}{libSuffix}";
            }
        }
    }
}
#endif
