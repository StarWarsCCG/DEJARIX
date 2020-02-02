namespace SwIpExporter
{
    public static class Strings
    {
        private static bool InRange(char c, char low, char high) => low <= c && c <= high;
        
        public static string Filtered(string s)
        {
            return s
                .Replace((char)65533, 'é')
                .Replace("1/2", "½")
                .Replace("1/4", "¼")
                .Replace("<>", "♢")
                .Replace("(*]", "•")
                .Replace("(*)", "(•)")
                .Replace("(**)", "(••)")
                .Replace("(***)", "(•••)")
                .Replace("\\par ", "\n")
                .Replace("\\b0 ", "")
                .Replace("\\b0", "")
                .Replace("\\b ", "")
                .Replace("\\ul0 ", "")
                .Replace("\\ul0", "")
                .Replace("\\ul ", "")
                .Replace("  ", " ")
                .Replace(" \n", "\n")
                ;
        }

        public static string Normalized(string text)
        {
            if (text == null)
                return null;
            
            var buffer = new char[text.Length];
            int n = 0;

            foreach (var c in text)
            {
                if (InRange(c, 'a', 'z') || InRange(c, '0', '9'))
                {
                    buffer[n++] = c;
                }
                else if ('A' <= c && c <= 'Z')
                {
                    buffer[n++] = (char)(c + 32);
                }
            }

            return new string(buffer, 0, n);
        }
    }
}