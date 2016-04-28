using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VVVV.Packs.Messaging
{
    public static class MessageUtils
    {
        /// <summary>Extension method for creating a regex out of a wildcard pattern</summary>
        /// <param name="message">The message whose data should be injected.</param>
        public static Regex CreateWildCardRegex(this string wildcardPattern)
        {
            var regexPattern = "^" + Regex.Escape(wildcardPattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            return regex;
        }

        #region static utilities
        // "Type[N] name"
        // Name can constitute of alphanumericals, dots, underscores and hyphens.
        public static Regex Parser = new Regex(@"^(\w*?)(\[\d*\])*\s+([\w\._-]+?)$");
        public static Regex NameParser = new Regex(@"^([\w\._-]+?)$");
        public static ISet<string> ForbiddenNames = new HashSet<string>(new[] { "", "ID", "Output", "Input", "Message", "Keep", "Topic", "Timestamp" }); // These names are likely to be pin names

        public static bool IsValidFieldName(this string fieldName)
        {
            return NameParser.IsMatch(fieldName.Trim()) && !ForbiddenNames.Contains(fieldName.Trim());
        }
        #endregion static utilities
    }
}
