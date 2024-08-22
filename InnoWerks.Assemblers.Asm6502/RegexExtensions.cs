using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace InnoWerks.Assemblers
{
    internal static class RegexExtensions
    {
        public static Dictionary<string, string> MatchNamedCaptures(this Regex regex, string input)
        {
            ArgumentNullException.ThrowIfNull(regex);

            var namedCaptureDictionary = new Dictionary<string, string>();
            var groups = regex.Match(input).Groups;
            var groupNames = regex.GetGroupNames();

            foreach (string groupName in groupNames)
            {
                if (groups[groupName].Captures.Count > 0)
                {
                    namedCaptureDictionary.Add(groupName, groups[groupName].Value);
                }
            }

            return namedCaptureDictionary;
        }
    }
}
