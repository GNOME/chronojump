//from the book "Mono. A Developer's Notebook". Edd Dumbill; Niel M. Bornstein p. 162

using System;
using System.Runtime.InteropServices;

class Catalog {
	[DllImport("libc")]
		static extern IntPtr bindtextdomain (IntPtr domainname, IntPtr dirname);
	[DllImport("libc")]
		static extern IntPtr bindtextdomain_codeset (IntPtr domainname, String codeset);
	[DllImport("libc")]
		static extern IntPtr textdomain (IntPtr domainname);

	static bool have_gettext;

	public static void Init (String package, String localedir)
	{
		IntPtr ipackage = Marshal.StringToHGlobalAuto (package);
		IntPtr ilocaledir = Marshal.StringToHGlobalAuto (localedir);
		IntPtr iutf8 = Marshal.StringToHGlobalAuto ("UTF-8");
		
		try {
			bindtextdomain (ipackage, ilocaledir);
			textdomain (ipackage);
			Marshal.FreeHGlobal (ipackage);
			Marshal.FreeHGlobal (ilocaledir);
			Marshal.FreeHGlobal (iutf8);

			have_gettext = true;
		} catch {
			have_gettext = false;
		}
	}
	
	[DllImport("libc")]
		static extern IntPtr gettext (IntPtr s);

	public static String GetString (String s)
	{
		if(have_gettext) {
			IntPtr inptr = Marshal.StringToHGlobalAuto(s);
			IntPtr sptr = gettext (inptr);
			Marshal.FreeHGlobal (inptr);
			if (inptr == sptr)
				return s;
			else 
				return Marshal.PtrToStringAuto (sptr);
		} else { 
			return s;
		}
	}

	
	[DllImport("libc")]
		static extern IntPtr ngettext (IntPtr s, IntPtr p, Int32 n);

	public static String GetPluralString (String s, String p, Int32 n)
	{
		IntPtr inptrs = Marshal.StringToHGlobalAuto(s);
		IntPtr inptrp = Marshal.StringToHGlobalAuto(p);
		IntPtr sptr = ngettext (inptrs, inptrp, n);
		Marshal.FreeHGlobal (inptrs);
		Marshal.FreeHGlobal (inptrp);
		if (sptr == inptrs)
			return s;
		else if (sptr == inptrp)
			return p;
		else
			return Marshal.PtrToStringAuto (sptr);
		
	}
}
