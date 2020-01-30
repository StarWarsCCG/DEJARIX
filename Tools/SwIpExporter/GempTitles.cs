using System.Collections.Generic;

namespace SwIpExporter
{
    public class GempTitles
    {
        public Dictionary<string, string> DarkSide { get; set; }
        public Dictionary<string, string> LightSide { get; set; }

        public static Dictionary<string, Dictionary<string, string>> Organized(
            Dictionary<string, string> dictionary)
        {
            var result = new Dictionary<string, Dictionary<string, string>>();

            foreach (var pair in dictionary)
            {
                int n = pair.Key.IndexOf('_');
                var expansionId = pair.Key.Substring(0, n);

                if (!result.TryGetValue(expansionId, out var idByTitle))
                {
                    idByTitle = new Dictionary<string, string>();
                    result.Add(expansionId, idByTitle);
                }

                idByTitle[pair.Value] = pair.Key;
            }

            return result;
        }
    }
}