using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class NumberUtil
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string ToNegativeSignString(this byte num)
        {
            return num.ToString().Replace("−", "-");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string ToNegativeSignString(this short num)
        {
            return num.ToString().Replace("−", "-");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string ToNegativeSignString(this int num)
        {            
            return num.ToString().Replace("−", "-");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string ToNegativeSignString(this long num)
        {
            return num.ToString().Replace("−", "-");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string ToNegativeSignString(this decimal num)
        {
            return num.ToString().Replace("−", "-");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string ToNegativeSignString(this float num)
        {
            return num.ToString().Replace("−", "-");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string ToNegativeSignString(this double num)
        {
            return num.ToString().Replace("−", "-");
        }
    }
}