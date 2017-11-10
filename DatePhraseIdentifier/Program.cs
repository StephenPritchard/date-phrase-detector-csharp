namespace DatePhraseIdentifier
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var datePhraseDector = new ImprovedDatePhraseDetector();
            var simulator = new DatePhraseIdentificationSimulator(datePhraseDector);

            simulator.RunSimulator();
        }
    }
}
