using System.Linq;
using System.Text.RegularExpressions;

namespace RevitElementBipChecker.Model
{
    public static class StringUtils
    {
        public static string RemoveInvalid(this string str)
        {
            string rExp = @"[^\w\d]";
            string replace = Regex.Replace(str, rExp, "");
            string trim = replace.Trim();
            return trim.TrimWhitespace().FirstOrDefault();
        }

        private static Regex Whitespaces = new Regex(@"\s+");
        public static string[] TrimWhitespace(this string input)
        {
            return Whitespaces.Split(input.Trim());
        }
    }
}
