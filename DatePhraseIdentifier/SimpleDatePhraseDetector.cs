using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using static DatePhraseIdentifier.ExtensionMethods;

namespace DatePhraseIdentifier
{
    public class SimpleDatePhraseDetector : IDatePhraseDetector
    {
        private readonly List<Regex> _simpleDatePhrases = new List<Regex>();

        // This simple detector just does one pass, and doesn't look for composite
        // date phrases. So it could find e.g., 12-02-2009, but not "The day after 12-02-2009".
        public SimpleDatePhraseDetector()
        {
            _simpleDatePhrases.AddRange(ReadRegexesFromFile(new FileInfo("regexes.txt")));
        }

        List<string> IDatePhraseDetector.FindDatePhrases(string input)
        {
            // This will loop through the list of known date phrase patterns,
            // and search for them in the input string. If there is a match,
            // that match is removed from the input, before continuing to
            // search the reamining input for further matches.
            // If the remaining input is reduced to nothing, then the search
            // stops.

            var remainingInput = input;

            var result = new List<string>();

            foreach (var pattern in _simpleDatePhrases)
            {
                var matches = pattern.Matches(remainingInput);

                foreach (Match match in matches)
                {
                    result.Add(match.Value);
                }
                // Remove any matches from the remaining input.
                if (matches.Count > 0)
                    remainingInput = pattern.Replace(remainingInput, "");
                    //TODO: see if there is a more efficient way than recreating immutable strings
                
                // Stop looking if there is no remaining input.
                if (remainingInput == "")
                    return result;
            }

            return result;
        }
    }
}