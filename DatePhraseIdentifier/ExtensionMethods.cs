using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace DatePhraseIdentifier
{
    public static class ExtensionMethods
    {
        public static StreamReader ReadFileToStream(this FileInfo file)
        {
            StreamReader streamR = null;

            try
            {
                streamR = file.OpenText();
            }
            catch
            {
                Console.WriteLine("There was a problem opening {0}.", file.Name);
                Console.WriteLine("Press enter to exit...");
                Console.ReadLine();
                Environment.Exit(0);
            }

            return streamR;
        }

        public static IEnumerable<Regex> ReadRegexesFromFile(FileInfo file)
        {
            var result = new List<Regex>();

            var countOfInvalidExpressions = 0;
            using (var streamR = file.ReadFileToStream())
            {
                string line;

                do
                {
                    line = streamR.ReadLine();

                    // Lines starting with # in regexes.txt are comments/headings etc.
                    if (string.IsNullOrEmpty(line) || line[0] == '#')
                        continue;

                    if (!IsValidRegex(line))
                    {
                        countOfInvalidExpressions++;
                        continue;
                    }

                    result.Add(new Regex(line));

                } while (!string.IsNullOrEmpty(line));

                if (countOfInvalidExpressions > 0)
                    Console.WriteLine($"{countOfInvalidExpressions} invalid expressions skipped.");
            }
            return result;
        }

        private static bool IsValidRegex(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return false;

            try
            {
                var regex = new Regex(pattern);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
