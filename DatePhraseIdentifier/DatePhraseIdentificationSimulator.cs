using System;

namespace DatePhraseIdentifier
{
    // This is a simple class to run a console simulator.
    // The user can enter in sentences to the console, and get feedback
    // also in the console as to the date phrases (if any) identified
    // in the input sentence.
    // I created an interface - IDatePhraseDetector - because it seemed clear that
    // the simple date phrase detector I would build could be improved with more
    // research. By using the interface here, I can switch detectors with minimal
    // change here.
    // At the very least, a new detector might add additional regex's, but there are
    // methods in literature that use additional approaches for detection, including
    // conditional random fields, named entity recognition (e.g. stanford temporal tagger).
    // There is a literature on time expression detection and resolution, but I avoided
    // this in this example, because date phrases is only a subset of the more
    // complicated time expression detection task, and I think it beyond the scope
    // of this test to go and research too far into these sorts of NLP techniques.
    public class DatePhraseIdentificationSimulator
    {
        private readonly IDatePhraseDetector _datePhraseDetector;

        public DatePhraseIdentificationSimulator(IDatePhraseDetector datePhraseDetector)
        {
            _datePhraseDetector = datePhraseDetector;
        }

        // This method can be called to run the simulator. The only other content here
        // is a field containing the implemented IDatePhraseParser to use, which can
        // be passed in by the program that creates an instance of this class.
        public void RunSimulator()
        {
            Console.WriteLine("DATE PHRASE IDENTIFIER");
            Console.WriteLine();

            do
            {
                Console.WriteLine("Enter a sentence to parse, or 'q' to exit:");
                var input = Console.ReadLine();

                if (input == "q")
                    Environment.Exit(0);
                else
                {
                    var dateMentions = _datePhraseDetector.FindDatePhrases(input);
                    if (dateMentions.Count == 0)
                        Console.WriteLine("\nNo date phrases identified.");
                    else
                    {
                        Console.WriteLine("\nDate phrases identified:");
                        foreach (var mention in dateMentions)
                            Console.WriteLine(mention);
                    }
                    Console.WriteLine("------------------------------");
                    Console.WriteLine();
                }

            } while (true);
        }
    }
}