//
// Mono.Posix.Catalog.cs: Wrappers for the libintl library.
//
// Author:
//   Edd Dumbill (edd@usefulinc.com)
//
// (C) 2004 Edd Dumbill
//
// This file implements the low-level syscall interface to the POSIX
// subsystem.
//
// This file tries to stay close to the low-level API as much as possible
// using enumerations, structures and in a few cases, using existing .NET
// data types.
//
// Implementation notes:
//
//    Since the values for the various constants on the API changes
//    from system to system (even Linux on different architectures will
//    have different values), we define our own set of values, and we
//    use a set of C helper routines to map from the constants we define
//    to the values of the native OS.
//
//    Bitfields are flagged with the [Map] attribute, and a helper program
//    generates a set of map_XXXX routines that we can call to convert
//    from our value definitions to the value definitions expected by the
//    OS.
//
//    Methods that require tuning are bound as `internal syscal_NAME' methods
//    and then a `NAME' method is exposed.
//
using System;
using System.Runtime.InteropServices;

namespace Mono.Unix
{

    /// <summary>
    /// Custom implementation of <see cref="Mono.Unix.Catalog"/>
    /// to support translations from the domain set by the plugin.
    /// </summary>
    public static class Catalog
    {
        static string CurrentDomain;

        [DllImport("libintl-8.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libintl_bindtextdomain")]
        static extern IntPtr bindtextdomain(string domainname, string dirname);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="domainname"></param>
        /// <returns></returns>
        [DllImport("libintl-8.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "textdomain")]
        static extern IntPtr textdomain(string domainname);


        [DllImport("libintl-8.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libintl_bind_textdomain_codeset")]
        static extern IntPtr bind_textdomain_codeset(string domainname,
                                                      string codeset);

        public static void Init(String package, String localedir)
        {
            CurrentDomain = package;

            if (bindtextdomain(package, localedir) == IntPtr.Zero)
                throw new Exception();
            if (bind_textdomain_codeset(package, "UTF-8") == IntPtr.Zero)
                throw new Exception();
            if (textdomain(package) == IntPtr.Zero)
                throw new Exception();
        }

        [DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr g_dgettext(string domain, IntPtr instring);

        public static String GetString(String s)
        {

            IntPtr sptr = GLib.Marshaller.StringToPtrGStrdup(s);
            try
            {
                IntPtr r = g_dgettext(CurrentDomain, sptr);
                return GLib.Marshaller.Utf8PtrToString(r);
            }
            finally
            {
                GLib.Marshaller.Free(sptr);
            }
        }

        [DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr g_dngettext(string domain, IntPtr singular, IntPtr plural, Int32 n);

        public static String GetPluralString(String s, String p, Int32 n)
        {
            IntPtr ints = GLib.Marshaller.StringToPtrGStrdup(s);
            IntPtr intp = GLib.Marshaller.StringToPtrGStrdup(p);


            try
            {
                IntPtr r = g_dngettext(CurrentDomain, ints, intp, n);
                return GLib.Marshaller.Utf8PtrToString(r);
            }
            finally
            {
                GLib.Marshaller.Free(ints);
                GLib.Marshaller.Free(intp);
            }
        }
    }
}


