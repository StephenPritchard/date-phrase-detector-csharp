using System.Collections.Generic;

namespace DatePhraseIdentifier
{
    // A date phrase detector should take a sentence in the form of a string as input, and return
    // a list of strings as output, where the output strings (if any) are recognised date phrases
    // discovered within the input sentence.
    public interface IDatePhraseDetector
    {
        List<string> FindDatePhrases(string input);
    }
}