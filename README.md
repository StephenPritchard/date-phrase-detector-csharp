# Simple Date Phrase Identifier
This is a console program that includes a date phrase detector.
The user can enter sentences into the console as input, and
the program returns output listing all the identified date phrases
in the input.

Note, there are two files in the main directory, "regexes.txt",
and "relativeDateRegexes.txt". These need to be in the same
directory as the executable before you run. 

Much of the logic here is contained in the regexes within these
files. The public holidays listed are from an Australian database,
you could use your own for a different country by editing the regexes.txt
file.

Code:
* Program.cs (main)
* IDatePhraseDetector.cs (interface specifying a date phrase detector)
* SimpleDatePhraseDetector.cs (1st attempt, now superceded. Basic date-phrases only)
* ImprovedDatePhraseDetector.cs (includes composite date-phrases with 1 level of nesting)
* DatePhraseIdentificationSimulator.cs (simple class to put the detector to work on the console)
* ExtensionMethods.cs (just some extra methods, mainly for reading in the regexes from file)