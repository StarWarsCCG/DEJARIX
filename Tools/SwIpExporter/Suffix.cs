namespace SwIpExporter
{
    public static class Suffix
    {
        private static readonly string[] Suffixes = new string[]
        {
            " (AI)",
            " (V)",
            " (EP1)",
            " (CC)",
            " (Frozen)",
            " (Starship)",
            " (Vehicle)",
            " (Premiere)",
            " (Coruscant)"
        };

        public static string Removed(string cardName)
        {
            foreach (var suffix in Suffixes)
            {
                if (cardName.EndsWith(suffix))
                    cardName = cardName[..^suffix.Length];
            }
            
            return cardName;
        }
    }
}