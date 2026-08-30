using System;
using System.Collections.Generic;

namespace QuickMsiBuilder.CLI
{
    /// <summary>
    /// The Revit releases offered by default. Autodesk ships the next year's release ahead of the
    /// calendar year and keeps roughly five in support, so the list is derived from the clock rather
    /// than hard coded - it stays correct without anyone editing it every spring.
    /// </summary>
    public static class RevitYearRange
    {
        /// <summary>Number of releases offered, newest included.</summary>
        public const int SupportedCount = 5;

        /// <summary>Next year's release, which ships before the year it is named after.</summary>
        public static int Newest
        {
            get { return DateTime.Now.Year + 1; }
        }

        public static int Oldest
        {
            get { return Newest - (SupportedCount - 1); }
        }

        /// <summary>Supported years, oldest first.</summary>
        public static List<string> Supported
        {
            get
            {
                var years = new List<string>(SupportedCount);
                for (var year = Oldest; year <= Newest; year++) years.Add(year.ToString());
                return years;
            }
        }

        /// <summary>
        /// Used when no year is supplied. The add-in always passes the running Revit version, so
        /// this only applies to direct CLI use.
        /// </summary>
        public static string Default
        {
            get { return DateTime.Now.Year.ToString(); }
        }
    }
}
