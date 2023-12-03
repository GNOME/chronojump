using Gdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chronojump
{
    /// <summary>
    /// A custom class MyPixbuf to ignore images issues
    /// </summary>
    internal static class MyPixbuf
    {
        /// <summary>
        /// Get Pixbuf by assembly name and resource path
        /// </summary>
        /// <param name="assembly">Assembly name</param>
        /// <param name="resource">Resource path</param>
        /// <returns>Return null if doesn't exist</returns>
        public static Pixbuf Get(Assembly assembly, string resource)
        {
            Pixbuf pixbuf = null;
            try
            {
                pixbuf = new Pixbuf(assembly, resource);
            }
            catch
            {
            }
            return pixbuf;
        }

        /// <summary>
        /// Get Pixbuf by file name
        /// </summary>
        /// <param name="filename">File name</param>
        /// <returns>Return null if doesn't exist</returns>
        public static Pixbuf Get(string filename)
        {
            Pixbuf pixbuf = null;
            try
            {
                pixbuf = new Pixbuf(filename);
            }
            catch
            {
            }
            return pixbuf;
        }
    }
}