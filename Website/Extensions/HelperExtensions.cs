using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Extensions
{
    public static class HelperExtensions
    {
        public static string CleanQuote(this string str)
        {
            str = str.Replace("`", "");
            str = str.Replace("*", "");
            return str;
        }
    }
}
