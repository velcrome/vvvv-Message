using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VVVV.Packs.Messaging
{
    /// <summary>
    /// Utils and Constraints for Message 
    /// </summary>
    public static class MessageUtils
    {
        /// <summary>Extension method for creating a regex out of a wildcard pattern</summary>
        /// <param name="wildcardPattern">A string that can contain * as a placeholder for any amount of characters</param>
        public static Regex CreateWildCardRegex(this string wildcardPattern)
        {
            var regexPattern = "^" + Regex.Escape(wildcardPattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            return regex;
        }

        /// <summary>
        /// A Parser that can check if a single trimmed field configuration is structurally sound
        /// </summary>
        /// <remarks>Type[N] name</remarks>
        /// <remarks>Name can constitute of alphanumericals, dots, underscores and hyphens.</remarks>
        public static readonly Regex ConfigurationParser = new Regex(@"^(\w*?)(\[\d*\])*\s+([\w\._-]+?)$");

        /// <summary>
        /// A Parser that can check, if a string contains only alphanumericals, dots, underscores and hyphens
        /// </summary>
        public static readonly Regex NameParser = new Regex(@"^([\w\._-]+?)$");

        /// <summary>
        /// A number of forbidden names for a Message Field. This helps to prevent vvvv to create potentially ambiguous pins
        /// </summary>
        public static readonly ISet<string> ForbiddenNames = new HashSet<string>(new[] { "", "ID", "Output", "Input", "Message", "Keep", "Topic", "Timestamp" }); // These names are likely to be pin names

        /// <summary>
        /// Convenience check if a string would qualify as a proper Field name
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns>true, if string could be used without restrictions.</returns>
        public static bool IsValidFieldName(this string fieldName)
        {
            return NameParser.IsMatch(fieldName.Trim()) && !ForbiddenNames.Contains(fieldName.Trim());
        }
    }
}
