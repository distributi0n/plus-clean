﻿namespace Plus.Utilities
{
    using System;
    using System.Text.RegularExpressions;

    internal static class StringCharFilter
    {
        public static string Escape(string str, bool allowBreaks = false)
        {
            str = str.Trim();
            str = str.Replace(Convert.ToChar(1), ' ');
            str = str.Replace(Convert.ToChar(2), ' ');
            str = str.Replace(Convert.ToChar(3), ' ');
            str = str.Replace(Convert.ToChar(9), ' ');
            if (!allowBreaks)
            {
                str = str.Replace(Convert.ToChar(10), ' ');
                str = str.Replace(Convert.ToChar(13), ' ');
            }
            str = Regex.Replace(str, "<(.|\\n)*?>", string.Empty);
            return str;
        }
    }
}