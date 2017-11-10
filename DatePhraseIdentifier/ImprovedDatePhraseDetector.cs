using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using static DatePhraseIdentifier.ExtensionMethods;

namespace DatePhraseIdentifier
{
    public class ImprovedDatePhraseDetector : IDatePhraseDetector
    {
        private readonly List<Regex> _simpleDatePhrases = new List<Regex>();
        private readonly List<Regex> _compositePhrases = new List<Regex>();
        private readonly Regex _basicPhraseToken = new Regex(@"%X");
        private readonly Regex _dayOfWeekToken = new Regex(@"%D");
        // Made this next list variable just for easy reference when looking for
        // composite phrases involving a day of the week.
        private readonly List<Regex> _daysOfWeek = new List<Regex>();

        // This date-phrase detector has a level of nesting.
        // It searches for basic phrases, corresponding directly to dates,
        // and also for dates formed by finding a date relative to a basic date.
        // e.g. "Two weeks from %X", where %X could be yesterday, or May 4th etc.
        // Days of the week are also specifically tokened and searched for in composite
        // date phrases, since basic phrases can often be nested inside an expression
        // that includes a redundant day of the week (e.g., "Wednesday, July 1, 2009")
        // and it is also very common for there to be basic date phrases where
        // that include a day of the week. Rather than separately create a basic date
        // phrase for each day of the week, I treat these as a composite phrase.
        // So %D week, instead of separately using "Friday week", "Monday week" etc.
        // Note that the days of the week are also listed in the basic phrases patterns,
        // so they can be searched for at the end as non-composite phrases. e.g. ("It happens Tuesday")
        public ImprovedDatePhraseDetector()
        {
            _simpleDatePhrases.AddRange(ReadRegexesFromFile(new FileInfo("regexes.txt")));
            _compositePhrases.AddRange(ReadRegexesFromFile(new FileInfo("relativeDateRegexes.txt")));
            PopulateDaysOfWeekRegex();
        }

        private void PopulateDaysOfWeekRegex()
        {
            // Only accept "[Ww]ed, [Ss]at or[Ssun] if with a number,
            // since "wed", "sat" and "sun" are words, not just
            // abbreviations.
            _daysOfWeek.Add(new Regex(@"[Mm]on(?:\.|day)?\b"));
            _daysOfWeek.Add(new Regex(@"[Tt] ues? (?:\.|day)?\b"));
            _daysOfWeek.Add(new Regex(@"[Ww]ed(?:\.|nesday)"));
            _daysOfWeek.Add(new Regex(@"[Tt]hurs(?:\.|day)?\b"));
            _daysOfWeek.Add(new Regex(@"[Ff]ri(?:\.|day)?\b"));
            _daysOfWeek.Add(new Regex(@"[Ss]at(?:\.|urday)"));
            _daysOfWeek.Add(new Regex(@"[Ss]un(?:\.|day)"));
        }

        List<string> IDatePhraseDetector.FindDatePhrases(string input)
        {
            var matchedBasicPhrases = FindSimpleExpressions(input);
            if (matchedBasicPhrases.Count == 0)
                return matchedBasicPhrases;

            return FindCompositePhrases(matchedBasicPhrases, input);
        }

        private List<string> FindSimpleExpressions(string input)
        {
            // This will loop through the list of known date phrases patterns,
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

        private List<string> FindCompositePhrases(List<string> basicPhrases, string input)
        {
            // This will loop through the list of known composite date phrase patterns,
            // And try out each of the matched basicPhrases in each of the composite patterns.
            // If the pattern includes a day of the week, then each day of the week is also
            // tried in the pattern. This results in 3 nested loops, so that each specific
            // pattern can be searched in the input string.
            // If there is a match,
            // that match is removed from the input, before continuing to
            // search the reamining input for further matches.
            // If the remaining input is reduced to nothing, then the search
            // stops.
            // TODO: see if further optimisation can be achieved by un-nesting loops,
            // TODO: or at least reducing the patterns considered in each loop.
            //
            // TODO: Try and pull out methods to avoid duplicating code.
            //
            // TODO: This only considers 1 level of composite phrase. However, it's conceivable
            // TODO: that there could be multiple levels of nesting, which wouldn't be detected
            // TODO: by this. For example, this will detect "Halloween" (basic),
            // TODO: and "Halloween next year" (1st level composite), but won't detect
            // TODO: "Friday after Halloween next year" (2nd level composite).
            // TODO: can fix this buy calling FindCompositePhrases again, using the identified
            // TODO: composite phrases from the 1st run as input, and keep doing this
            // TODO: until no more composite phrases are found.
            // TODO: at the moment, i clumsily re-search for basic phrases after taking out any
            // TODO: 1st order composite phrases by just putting %X and %D at the end of the
            // TODO: composite phrases regexes.
            
            var result = new List<string>();
            var remainingInput = input;

            foreach (var pattern in _compositePhrases)
            {
                // Note there is some additional nesting in if statements etc here, because I 
                // make a couple of checks to avoid spending time searching for patterns that
                // won't be there. All the nested code is a but awkward, but hopefully a bit faster.

                // if the pattern does not contain %X, then it must only contain %D,
                // so dont loop through all the basic phrases for this pattern,
                // just loop through days of week.
                if (!_basicPhraseToken.IsMatch(pattern.ToString()))
                {
                    foreach (var dayOfWeekInstance in _daysOfWeek)
                    {
                        var compositePatternWithDay = new Regex(_dayOfWeekToken.Replace(pattern.ToString(),
                            dayOfWeekInstance.ToString()));
                        var matches = compositePatternWithDay.Matches(remainingInput);

                        foreach (Match match in matches)
                        {
                            result.Add(match.Value);
                        }
                        // Remove any matches from the remaining input.
                        if (matches.Count > 0)
                            remainingInput = compositePatternWithDay.Replace(remainingInput, "");

                        // Stop looking if there is no remaining input.
                        if (remainingInput == "")
                            return result;
                    }
                }
                // If the pattern does contain %X, then need to loop through both the basicPhrases,
                // and potentially also the days of the week.
                else
                {
                    foreach (var basicPhrase in basicPhrases)
                    {
                        var compositePattern = new Regex(_basicPhraseToken.Replace(pattern.ToString(), basicPhrase));

                        if (_dayOfWeekToken.IsMatch(compositePattern.ToString()))
                        {
                            foreach (var dayOfWeekInstance in _daysOfWeek)
                            {
                                var compositePatternWithDay = new Regex(_dayOfWeekToken.Replace(
                                    compositePattern.ToString(),
                                    dayOfWeekInstance.ToString()));
                                var matches = compositePatternWithDay.Matches(remainingInput);

                                foreach (Match match in matches)
                                {
                                    result.Add(match.Value);
                                }
                                // Remove any matches from the remaining input.
                                if (matches.Count > 0)
                                    remainingInput = compositePatternWithDay.Replace(remainingInput, "");

                                // Stop looking if there is no remaining input.
                                if (remainingInput == "")
                                    return result;
                            }
                        }
                        else
                        {
                            // The pattern contains %X, but not %D. dont need to loop through days of the week.
                            var matches = compositePattern.Matches(remainingInput);

                            foreach (Match match in matches)
                            {
                                result.Add(match.Value);
                            }
                            // Remove any matches from the remaining input.
                            if (matches.Count > 0)
                                remainingInput = compositePattern.Replace(remainingInput, "");

                            // Stop looking if there is no remaining input.
                            if (remainingInput == "")
                                return result;
                        }
                    }
                }
            }
            return result;
        }
    }
}