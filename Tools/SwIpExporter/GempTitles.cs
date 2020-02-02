using System.Collections.Generic;
using System.Collections.Immutable;

namespace SwIpExporter
{
    public class GempTitles
    {
        public static readonly ImmutableDictionary<string, string> GempExpansions = new Dictionary<string, string>
        {
            ["Hoth"] = "3",
            ["Premiere"] = "1",
            ["Special Edition"] = "7",
            ["Dagobah"] = "4",
            ["Theed Palace"] = "14",
            ["Enhanced Cloud City"] = "109",
            ["Jabba's Palace"] = "6",
            ["Reflections III"] = "13",
            ["Tatooine"] = "11",
            ["Third Anthology"] = "111",
            ["Coruscant"] = "12",
            ["Endor"] = "8",
            ["Cloud City"] = "5",
            ["Virtual Card Set #0"] = "200",
            ["Reflections II"] = "10",
            ["Death Star II"] = "9",
            ["A New Hope"] = "2",
            ["Virtual Defensive Shields"] = "200",
            ["Jabba's Palace Sealed Deck"] = "112",
            ["Official Tournament Sealed Deck"] = "106",
            ["Enhanced Premiere Pack"] = "108",
            ["Enhanced Jabba's Palace"] = "110",
            ["Hoth 2 Player"] = "104",
            ["Virtual Card Set #1"] = "201",
            ["Jedi Pack"] = "102",
            ["Premiere 2 Player"] = "101",
            ["Rebel Leader Cards"] = "103",
            ["Virtual Card Set #2"] = "202",
            ["Virtual Card Set #3"] = "203",
            ["Virtual Card Set #4"] = "204",
            ["Virtual Card Set #5"] = "205",
            ["Demonstration Deck Premium Card Set"] = "301",
            ["Virtual Card Set #6"] = "206",
            ["Virtual Card Set #7"] = "207",
            ["Virtual Card Set #8"] = "208",
            ["Virtual Card Set #9"] = "209",
            ["Virtual Card Set #10"] = "210",
            ["Virtual Card Set #11"] = "211"
        }.ToImmutableDictionary();

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
        
        public Dictionary<string, string> DarkSide { get; set; }
        public Dictionary<string, string> LightSide { get; set; }
    }
}